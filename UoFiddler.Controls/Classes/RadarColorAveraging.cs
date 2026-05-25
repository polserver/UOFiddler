/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Ultima;
using Ultima.Helpers;

namespace UoFiddler.Controls.Classes
{
    public enum RadarAveragingStrategy
    {
        // Order roughly matches likelihood of matching the on-disk radarcol.mul.
        Mean5,
        Mean8Shift,
        Mean5Rounded,
        Mean5RoundedIncludeAlpha,
        Mean5BankersRound,
        Mean5RoundedNoOutline,
        SnapToLandPalette,
        SnapToItemPalette,
        MeanRounded,
        MeanLinear,
        Mode16,
        MedianPerChannel,
        MeanNoOutline,
        // Reproduces the historical UOFiddler behavior exactly (average in *31/255-truncated
        // 8-bit space, then ColorToHue with the same truncation). Kept so existing users can
        // get back the values they're used to.
        Legacy,
    }

    public static class RadarColorAveraging
    {
        public static IReadOnlyList<RadarAveragingStrategy> All { get; } = new[]
        {
            RadarAveragingStrategy.Mean5,
            RadarAveragingStrategy.Mean8Shift,
            RadarAveragingStrategy.Mean5Rounded,
            RadarAveragingStrategy.Mean5RoundedIncludeAlpha,
            RadarAveragingStrategy.Mean5BankersRound,
            RadarAveragingStrategy.Mean5RoundedNoOutline,
            RadarAveragingStrategy.SnapToLandPalette,
            RadarAveragingStrategy.SnapToItemPalette,
            RadarAveragingStrategy.MeanRounded,
            RadarAveragingStrategy.MeanLinear,
            RadarAveragingStrategy.Mode16,
            RadarAveragingStrategy.MedianPerChannel,
            RadarAveragingStrategy.MeanNoOutline,
            RadarAveragingStrategy.Legacy,
        };

        public static string DisplayName(RadarAveragingStrategy s) => s switch
        {
            RadarAveragingStrategy.Mean5 => "Mean (5-bit)",
            RadarAveragingStrategy.Mean8Shift => "Mean (8-bit, >>3 pack)",
            RadarAveragingStrategy.Mean5Rounded => "Mean (5-bit, rounded)",
            RadarAveragingStrategy.Mean5RoundedIncludeAlpha => "Mean (5-bit, rounded, incl. transparent)",
            RadarAveragingStrategy.Mean5BankersRound => "Mean (5-bit, banker's round)",
            RadarAveragingStrategy.Mean5RoundedNoOutline => "Mean (5-bit, rounded, no outline)",
            RadarAveragingStrategy.SnapToLandPalette => "Snap to land palette",
            RadarAveragingStrategy.SnapToItemPalette => "Snap to item palette",
            RadarAveragingStrategy.MeanRounded => "Mean (8-bit, rounded pack)",
            RadarAveragingStrategy.MeanLinear => "Mean (linear-light)",
            RadarAveragingStrategy.Mode16 => "Mode (dominant pixel)",
            RadarAveragingStrategy.MedianPerChannel => "Median per channel",
            RadarAveragingStrategy.MeanNoOutline => "Mean (no outline)",
            RadarAveragingStrategy.Legacy => "Legacy (UOFiddler)",
            _ => s.ToString(),
        };

        public static ushort Compute(Bitmap image, RadarAveragingStrategy strategy)
        {
            if (image == null)
            {
                return 0;
            }

            ushort[] pixels = IncludesTransparent(strategy)
                ? CollectAllPixels(image)
                : CollectOpaquePixels(image, out int _, out int _);
            if (pixels.Length == 0)
            {
                return 0;
            }

            return Dispatch(pixels, strategy);
        }

        private static bool IncludesTransparent(RadarAveragingStrategy strategy) =>
            strategy == RadarAveragingStrategy.Mean5RoundedIncludeAlpha;

        private static ushort Dispatch(ushort[] pixels, RadarAveragingStrategy strategy) => strategy switch
        {
            RadarAveragingStrategy.Mean5 => Mean5(pixels, rounded: false),
            RadarAveragingStrategy.Mean5Rounded => Mean5(pixels, rounded: true),
            // Same math as Mean5Rounded; the difference is at pixel collection time
            // (zeros are included in the pooled array and dilute the average).
            RadarAveragingStrategy.Mean5RoundedIncludeAlpha => Mean5(pixels, rounded: true),
            RadarAveragingStrategy.Mean5BankersRound => Mean5Banker(pixels),
            RadarAveragingStrategy.SnapToLandPalette => SnapTo(Mean5Banker(pixels), GetLandPalette()),
            RadarAveragingStrategy.SnapToItemPalette => SnapTo(Mean5Banker(pixels), GetItemPalette()),
            RadarAveragingStrategy.Mean5RoundedNoOutline => Mean5RoundedNoOutline(pixels),
            RadarAveragingStrategy.Mean8Shift => Mean8(pixels, ConvertPack.Shift),
            RadarAveragingStrategy.MeanRounded => Mean8(pixels, ConvertPack.Rounded),
            RadarAveragingStrategy.MeanLinear => MeanLinear(pixels),
            RadarAveragingStrategy.Mode16 => Mode16(pixels),
            RadarAveragingStrategy.MedianPerChannel => MedianPerChannel(pixels),
            RadarAveragingStrategy.MeanNoOutline => MeanNoOutline(pixels),
            RadarAveragingStrategy.Legacy => Legacy(pixels),
            _ => Mean5(pixels, rounded: true),
        };

        /// <summary>
        /// Pools pixels across multiple bitmaps and runs the strategy once.
        /// </summary>
        /// <param name="images"></param>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public static ushort ComputeFromMany(IEnumerable<Bitmap> images, RadarAveragingStrategy strategy)
        {
            bool includeAlpha = IncludesTransparent(strategy);
            var pooled = new List<ushort>(4096);
            foreach (Bitmap img in images)
            {
                if (img == null)
                {
                    continue;
                }
                ushort[] px = includeAlpha
                    ? CollectAllPixels(img)
                    : CollectOpaquePixels(img, out int _, out int _);
                if (px.Length > 0)
                {
                    pooled.AddRange(px);
                }
            }
            if (pooled.Count == 0)
            {
                return 0;
            }
            return Dispatch(pooled.ToArray(), strategy);
        }

        /// <summary>
        /// Reads all non-zero pixels (zero is the canonical transparency marker in the
        /// Format16bppArgb1555 bitmaps Art.GetStatic / Art.GetLand produce) and returns them as a flat ushort[].
        /// Width/height returned for callers that care. Reads every pixel including zeros (transparent).
        /// Strategies that want to weight transparent area into the average operate on this output.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static unsafe ushort[] CollectAllPixels(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            BitmapData bd = image.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format16bppArgb1555);
            try
            {
                var result = new ushort[width * height];
                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                int idx = 0;
                for (int y = 0; y < height; ++y, line += delta)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        result[idx++] = line[x];
                    }
                }

                return result;
            }
            finally
            {
                image.UnlockBits(bd);
            }
        }

        private static unsafe ushort[] CollectOpaquePixels(Bitmap image, out int width, out int height)
        {
            width = image.Width;
            height = image.Height;

            BitmapData bd = image.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format16bppArgb1555);

            try
            {
                var list = new List<ushort>(width * height);
                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < height; ++y, line += delta)
                {
                    for (int x = 0; x < width; ++x)
                    {
                        ushort p = line[x];

                        if (p != 0)
                        {
                            list.Add(p);
                        }
                    }
                }

                return list.ToArray();
            }
            finally
            {
                image.UnlockBits(bd);
            }
        }

        private enum ConvertPack { Shift, Rounded }

        private static ushort Mean5(ushort[] pixels, bool rounded)
        {
            long r = 0;
            long g = 0;
            long b = 0;

            int n = pixels.Length;

            for (int i = 0; i < n; ++i)
            {
                HueHelpers.HueExtract5(pixels[i], out int r5, out int g5, out int b5);

                r += r5;
                g += g5;
                b += b5;
            }

            int half = rounded ? n / 2 : 0;
            int rr = (int)((r + half) / n);
            int gg = (int)((g + half) / n);
            int bb = (int)((b + half) / n);

            return Pack5WithClamp(rr, gg, bb);
        }

        /// <summary>
        /// Round-half-to-even (banker's).
        /// Differs from Mean5(rounded) only when the channel sum is exactly half-way between two ints.
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        private static ushort Mean5Banker(ushort[] pixels)
        {
            long r = 0;
            long g = 0;
            long b = 0;

            int n = pixels.Length;

            for (int i = 0; i < n; ++i)
            {
                HueHelpers.HueExtract5(pixels[i], out int r5, out int g5, out int b5);

                r += r5;
                g += g5;
                b += b5;
            }

            return Pack5WithClamp(DivRoundEven(r, n), DivRoundEven(g, n), DivRoundEven(b, n));
        }

        private static int DivRoundEven(long sum, int n)
        {
            long q = sum / n;
            long rem = sum - q * n;
            long twice = rem * 2;
            if (twice < n)
            {
                return (int)q;
            }

            if (twice > n)
            {
                return (int)(q + 1);
            }

            // exact tie: round to even
            return (q & 1) == 0 ? (int)q : (int)(q + 1);
        }

        /// <summary>
        /// Drop near-black outline pixels (5-bit luma &lt; 2) then apply rounded 5-bit mean.
        /// Tests whether OSI excluded outline pixels before averaging.
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        private static ushort Mean5RoundedNoOutline(ushort[] pixels)
        {
            long r = 0;
            long g = 0;
            long b = 0;

            int n = 0;
            foreach (ushort p in pixels)
            {
                HueHelpers.HueExtract5(p, out int r5, out int g5, out int b5);

                int y5 = (r5 + g5 + b5) / 3;
                if (y5 < 2)
                {
                    continue;
                }

                r += r5;
                g += g5;
                b += b5;

                ++n;
            }

            if (n == 0)
            {
                return Mean5(pixels, rounded: true);
            }

            int half = n / 2;

            return Pack5WithClamp((int)((r + half) / n), (int)((g + half) / n), (int)((b + half) / n));
        }

        private static ushort Mean8(ushort[] pixels, ConvertPack pack)
        {
            long r = 0;
            long g = 0;
            long b = 0;

            int n = pixels.Length;
            for (int i = 0; i < n; ++i)
            {
                HueHelpers.HueExtract5(pixels[i], out int r5, out int g5, out int b5);

                r += HueHelpers.Expand5To8(r5);
                g += HueHelpers.Expand5To8(g5);
                b += HueHelpers.Expand5To8(b5);
            }

            int rr = (int)((r + n / 2) / n);
            int gg = (int)((g + n / 2) / n);
            int bb = (int)((b + n / 2) / n);

            return pack == ConvertPack.Shift
                ? HueHelpers.ColorToHueShift(rr, gg, bb)
                : HueHelpers.ColorToHueRounded(rr, gg, bb);
        }

        /// <summary>
        /// sRGB -> linear via x*x (cheap, monotonic, no transcendentals); average in linear;
        /// back via sqrt. This is a control candidate; 1997 tooling is unlikely to
        /// be gamma-aware so we don't expect a high exact-match.
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        private static ushort MeanLinear(ushort[] pixels)
        {
            double r = 0;
            double g = 0;
            double b = 0;

            int n = pixels.Length;
            for (int i = 0; i < n; ++i)
            {
                HueHelpers.HueExtract5(pixels[i], out int r5, out int g5, out int b5);

                double rr = HueHelpers.Expand5To8(r5) / 255.0;
                double gg = HueHelpers.Expand5To8(g5) / 255.0;
                double bb = HueHelpers.Expand5To8(b5) / 255.0;

                r += rr * rr;
                g += gg * gg;
                b += bb * bb;
            }

            int r8 = (int)Math.Round(Math.Sqrt(r / n) * 255.0);
            int g8 = (int)Math.Round(Math.Sqrt(g / n) * 255.0);
            int b8 = (int)Math.Round(Math.Sqrt(b / n) * 255.0);

            return HueHelpers.ColorToHueShift(r8, g8, b8);
        }

        private static ushort Mode16(ushort[] pixels)
        {
            var counts = new Dictionary<ushort, int>(pixels.Length);
            foreach (ushort p in pixels)
            {
                counts.TryGetValue(p, out int c);
                counts[p] = c + 1;
            }

            ushort best = pixels[0];

            int bestCount = 0;
            foreach (var kv in counts)
            {
                if (kv.Value > bestCount)
                {
                    best = kv.Key;
                    bestCount = kv.Value;
                }
            }

            // Strip stored alpha bit: radarcol entries never have 0x8000 set.
            return (ushort)(best & 0x7fff);
        }

        private static ushort MedianPerChannel(ushort[] pixels)
        {
            int n = pixels.Length;

            var rs = new int[n];
            var gs = new int[n];
            var bs = new int[n];

            for (int i = 0; i < n; ++i)
            {
                HueHelpers.HueExtract5(pixels[i], out int r5, out int g5, out int b5);
                rs[i] = r5; gs[i] = g5; bs[i] = b5;
            }

            Array.Sort(rs); Array.Sort(gs); Array.Sort(bs);

            int mid = n / 2;

            return Pack5WithClamp(rs[mid], gs[mid], bs[mid]);
        }

        /// <summary>
        /// Drop near-black outline pixels (5-bit luminance &lt; 2) before averaging.
        /// Falls back to plain Mean5 if everything is dark.
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        private static ushort MeanNoOutline(ushort[] pixels)
        {
            long r = 0;
            long g = 0;
            long b = 0;

            int n = 0;
            foreach (ushort p in pixels)
            {
                HueHelpers.HueExtract5(p, out int r5, out int g5, out int b5);

                // 5-bit Rec.601 luma weights would be ideal but for outline rejection a
                // simple mean of the channels in 5-bit space is adequate.
                int y5 = (r5 + g5 + b5) / 3;
                if (y5 < 2)
                {
                    continue;
                }

                r += r5;
                g += g5;
                b += b5;
                
                ++n;
            }

            if (n == 0)
            {
                return Mean5(pixels, rounded: false);
            }

            return Pack5WithClamp((int)(r / n), (int)(g / n), (int)(b / n));
        }

        // The pre-existing UOFiddler behavior, reproduced verbatim so users can opt back
        // in if they want bit-for-bit continuity with older builds. Math is inlined here
        // because the rest of the codebase has since migrated HueHelpers.HueToColor* /
        // ColorToHue to the OSI-canonical convention.
        private static ushort Legacy(ushort[] pixels)
        {
            const int legacyUpscale = 255 / 31; // == 8 (integer truncation, the historical bug)

            long r = 0;
            long g = 0;
            long b = 0;

            int n = pixels.Length;
            for (int i = 0; i < n; ++i)
            {
                ushort p = pixels[i];

                r += ((p & 0x7c00) >> 10) * legacyUpscale;
                g += ((p & 0x03e0) >> 5) * legacyUpscale;
                b += (p & 0x001f) * legacyUpscale;
            }

            int rr = (int)(r / n);
            int gg = (int)(g / n);
            int bb = (int)(b / n);

            // Legacy downscale: floor(c * 31 / 255) per channel, with the 0->1 clamp.
            const double legacyDownscale = 31.0 / 255;

            int newR = (int)(rr * legacyDownscale);
            if (newR == 0 && rr != 0)
            {
                newR = 1;
            }

            int newG = (int)(gg * legacyDownscale); if (newG == 0 && gg != 0)
            {
                newG = 1;
            }

            int newB = (int)(bb * legacyDownscale); if (newB == 0 && bb != 0)
            {
                newB = 1;
            }

            return (ushort)((newR << 10) | (newG << 5) | newB);
        }

        /// <summary>
        /// Snaps a 16-bit color to the nearest entry in the supplied palette by 5-bit squared Euclidean distance.
        /// Used for the land snap strategy: the loaded radarcol.mul has ~100 unique land colors (terrain palette),
        /// and any computed average should round to whichever of those it lies closest to.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="palette"></param>
        /// <returns></returns>
        private static ushort SnapTo(ushort candidate, ushort[] palette)
        {
            if (palette.Length == 0)
            {
                return candidate;
            }

            HueHelpers.HueExtract5(candidate, out int cr, out int cg, out int cb);

            ushort best = palette[0];

            int bestDist = int.MaxValue;
            for (int i = 0; i < palette.Length; ++i)
            {
                HueHelpers.HueExtract5(palette[i], out int pr, out int pg, out int pb);

                int dr = cr - pr, dg = cg - pg, db = cb - pb;
                int d = dr * dr + dg * dg + db * db;

                if (d < bestDist)
                {
                    bestDist = d;
                    best = palette[i];
                }
            }

            return best;
        }

        // Cached unique palettes built from the currently-loaded radarcol.mul. Invalidated
        // when the Colors array reference changes (load/import) — we compare references,
        // which is cheap and matches RadarCol's reassignment pattern.
        private static ushort[] _landPalette = Array.Empty<ushort>();
        private static ushort[] _itemPalette = Array.Empty<ushort>();
        private static ushort[] _palettesBuiltFor;

        private static void RebuildPalettesIfStale()
        {
            ushort[] cur = RadarCol.Colors;
            if (ReferenceEquals(cur, _palettesBuiltFor))
            {
                return;
            }

            _palettesBuiltFor = cur;

            if (cur == null || cur.Length == 0)
            {
                _landPalette = Array.Empty<ushort>();
                _itemPalette = Array.Empty<ushort>();
                return;
            }

            var landSet = new HashSet<ushort>();
            var itemSet = new HashSet<ushort>();
            int landEnd = Math.Min(0x4000, cur.Length);

            for (int i = 0; i < landEnd; ++i)
            {
                if (cur[i] != 0)
                {
                    landSet.Add(cur[i]);
                }
            }

            for (int i = 0x4000; i < cur.Length; ++i)
            {
                if (cur[i] != 0)
                {
                    itemSet.Add(cur[i]);
                }
            }

            _landPalette = new ushort[landSet.Count];
            landSet.CopyTo(_landPalette);

            _itemPalette = new ushort[itemSet.Count];
            itemSet.CopyTo(_itemPalette);
        }

        private static ushort[] GetLandPalette()
        {
            RebuildPalettesIfStale();

            return _landPalette;
        }

        private static ushort[] GetItemPalette()
        {
            RebuildPalettesIfStale();

            return _itemPalette;
        }

        /// <summary>
        /// Packs 5-bit components and applies the OSI clamp (all-zero -> 0, else any collapsed lane in a non-zero pixel -> 1).
        /// </summary>
        /// <param name="r5"></param>
        /// <param name="g5"></param>
        /// <param name="b5"></param>
        /// <returns></returns>
        private static ushort Pack5WithClamp(int r5, int g5, int b5)
        {
            r5 = Math.Clamp(r5, 0, 31);
            g5 = Math.Clamp(g5, 0, 31);
            b5 = Math.Clamp(b5, 0, 31);

            if ((r5 | g5 | b5) == 0)
            {
                return 0;
            }

            return (ushort)((r5 << 10) | (g5 << 5) | b5);
        }
    }
}
