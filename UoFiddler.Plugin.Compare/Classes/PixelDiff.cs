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
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// What <see cref="PixelDiff.Build"/> found: one overlay per image, each the size of its own
    /// image and drawn over it, the pixel counts, and how the images were lined up.
    /// </summary>
    public sealed class PixelDiffResult : IDisposable
    {
        public PixelDiffResult(Bitmap firstOverlay, Bitmap secondOverlay, int differing, int visible, Point offset)
        {
            FirstOverlay = firstOverlay;
            SecondOverlay = secondOverlay;
            Differing = differing;
            Visible = visible;
            Offset = offset;
        }

        /// <summary>Drawn over the first image; null when there is nothing to mark.</summary>
        public Bitmap FirstOverlay { get; }

        /// <summary>Drawn over the second image; null when there is nothing to mark.</summary>
        public Bitmap SecondOverlay { get; }

        /// <summary>Pixels that differ between the two images.</summary>
        public int Differing { get; }

        /// <summary>Pixels visible in at least one of the images.</summary>
        public int Visible { get; }

        /// <summary>
        /// Where the second image's top-left corner was placed relative to the first's when they
        /// were compared. Not empty when the art sits at a different spot in its canvas.
        /// </summary>
        public Point Offset { get; }

        public void Dispose()
        {
            FirstOverlay?.Dispose();
            SecondOverlay?.Dispose();
        }
    }

    /// <summary>
    /// Compares two images pixel by pixel and builds an overlay for each that fades its unchanged
    /// pixels and outlines its changed ones. Each overlay only marks its own image's pixels, so one
    /// image's shape never shows on the other.
    /// </summary>
    public static class PixelDiff
    {
        /// <summary>One image's pixels as 32bpp ARGB, row by row.</summary>
        private readonly record struct Pixels(int[] Data, int Width, int Height);

        /// <summary>Both images placed on a shared canvas.</summary>
        private readonly record struct Layout(int Width, int Height, Point First, Point Second);

        /// <summary>
        /// The same art can sit at a different spot in its canvas in another client (a taller
        /// canvas, trimmed or added rows), so the images are lined up several ways: by their
        /// top-left corners, bottom-centered as the client places item art, and by the edges of
        /// their visible content. The one with the fewest differing pixels is used; top-left wins
        /// a tie.
        /// <para>
        /// A pixel differs when any channel (alpha included) differs by more than
        /// <paramref name="tolerance"/> (0-255), or when it is visible in one image and transparent
        /// or outside the other. Unchanged visible pixels are covered with the fade color at
        /// <paramref name="fadePercent"/>; when (nearly) every visible pixel differs nothing is faded
        /// or outlined, so two unrelated images are both shown as they are.
        /// </para>
        /// </summary>
        public static PixelDiffResult Build(Bitmap first, Bitmap second, int tolerance, Color firstFade, Color secondFade, int fadePercent, Color outline)
        {
            Pixels a = ReadPixels(first);
            Pixels b = ReadPixels(second);
            if ((a.Width == 0 || a.Height == 0) && (b.Width == 0 || b.Height == 0))
            {
                return new PixelDiffResult(null, null, 0, 0, Point.Empty);
            }

            tolerance = Math.Clamp(tolerance, 0, 255);

            Layout layout = default;
            int best = int.MaxValue;
            foreach (Point offset in CandidateOffsets(a, b))
            {
                Layout candidate = Place(a, b, offset);
                int count = Compare(a, b, candidate, tolerance, null, out _);
                if (count < best)
                {
                    best = count;
                    layout = candidate;
                }
            }

            var differs = new bool[layout.Width * layout.Height];
            int differing = Compare(a, b, layout, tolerance, differs, out int visible);
            var offsetUsed = new Point(layout.Second.X - layout.First.X, layout.Second.Y - layout.First.Y);

            if (differing == 0 || IsUnrelated(differing, visible))
            {
                return new PixelDiffResult(null, null, differing, visible, offsetUsed);
            }

            int fadeAlpha = (int)Math.Round(Math.Clamp(fadePercent, 0, 100) * 2.55);
            Bitmap firstOverlay = BuildOverlay(a, layout.First, differs, layout.Width, firstFade, fadeAlpha, outline);
            Bitmap secondOverlay = BuildOverlay(b, layout.Second, differs, layout.Width, secondFade, fadeAlpha, outline);
            return new PixelDiffResult(firstOverlay, secondOverlay, differing, visible, offsetUsed);
        }

        /// <summary>
        /// True when (nearly) every visible pixel differs: the images are different graphics, and
        /// fading the few pixels they share by chance would only clutter both.
        /// </summary>
        public static bool IsUnrelated(int differing, int visible) => visible > 0 && differing * 100L >= visible * 95L;

        /// <summary>Offsets of the second image's origin from the first's, top-left first.</summary>
        private static List<Point> CandidateOffsets(Pixels a, Pixels b)
        {
            var offsets = new List<Point> { Point.Empty };
            void Add(Point offset)
            {
                if (!offsets.Contains(offset))
                {
                    offsets.Add(offset);
                }
            }

            Add(new Point((a.Width - b.Width) / 2, a.Height - b.Height));

            // Stray pixels along one edge of the content (an added border row, say) move that
            // edge, so line the content up by each pair of edges and let the comparison choose.
            Rectangle contentA = ContentBounds(a);
            Rectangle contentB = ContentBounds(b);
            if (!contentA.IsEmpty && !contentB.IsEmpty)
            {
                int[] xs =
                [
                    contentA.Left - contentB.Left,
                    contentA.Right - contentB.Right,
                    ((contentA.Left + contentA.Right) - (contentB.Left + contentB.Right)) / 2,
                ];
                int[] ys = [contentA.Top - contentB.Top, contentA.Bottom - contentB.Bottom];
                foreach (int y in ys)
                {
                    foreach (int x in xs)
                    {
                        Add(new Point(x, y));
                    }
                }
            }

            return offsets;
        }

        private static Layout Place(Pixels a, Pixels b, Point offset)
        {
            int left = Math.Min(0, offset.X);
            int top = Math.Min(0, offset.Y);
            int right = Math.Max(a.Width, offset.X + b.Width);
            int bottom = Math.Max(a.Height, offset.Y + b.Height);
            return new Layout(right - left, bottom - top, new Point(-left, -top), new Point(offset.X - left, offset.Y - top));
        }

        /// <summary>Counts the differing pixels, and fills <paramref name="differs"/> when given.</summary>
        private static int Compare(Pixels a, Pixels b, Layout layout, int tolerance, bool[] differs, out int visible)
        {
            int differing = 0;
            visible = 0;
            for (int y = 0; y < layout.Height; y++)
            {
                for (int x = 0; x < layout.Width; x++)
                {
                    int pa = PixelAt(a, layout.First, x, y);
                    int pb = PixelAt(b, layout.Second, x, y);
                    if ((pa >>> 24) != 0 || (pb >>> 24) != 0)
                    {
                        visible++;
                    }

                    if (Differs(pa, pb, tolerance))
                    {
                        differing++;
                        if (differs != null)
                        {
                            differs[(y * layout.Width) + x] = true;
                        }
                    }
                }
            }

            return differing;
        }

        private static int PixelAt(Pixels image, Point origin, int canvasX, int canvasY)
        {
            int x = canvasX - origin.X;
            int y = canvasY - origin.Y;
            return x < 0 || y < 0 || x >= image.Width || y >= image.Height ? 0 : image.Data[(y * image.Width) + x];
        }

        /// <summary>The smallest rectangle holding every visible pixel; empty when there is none.</summary>
        private static Rectangle ContentBounds(Pixels image)
        {
            int left = image.Width;
            int top = image.Height;
            int right = -1;
            int bottom = -1;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if ((image.Data[(y * image.Width) + x] >>> 24) == 0)
                    {
                        continue;
                    }

                    left = Math.Min(left, x);
                    right = Math.Max(right, x);
                    top = Math.Min(top, y);
                    bottom = Math.Max(bottom, y);
                }
            }

            return right < 0 ? Rectangle.Empty : Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        /// <summary>
        /// The overlay for one image: transparent over its changed pixels so they show in their
        /// own colors, the outline color on the pixels just around them, and the fade color over
        /// its other visible pixels.
        /// </summary>
        private static Bitmap BuildOverlay(Pixels image, Point origin, bool[] differs, int canvasWidth, Color fade, int fadeAlpha, Color outline)
        {
            int width = image.Width;
            int height = image.Height;
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            bool Changed(int x, int y) =>
                differs[((y + origin.Y) * canvasWidth) + x + origin.X] && (image.Data[(y * width) + x] >>> 24) != 0;

            int fadeColor = (fadeAlpha << 24) | (fade.ToArgb() & 0xFFFFFF);
            int outlineColor = outline.ToArgb();
            var overlay = new int[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (Changed(x, y))
                    {
                        continue;
                    }

                    bool nextToChange = (x > 0 && Changed(x - 1, y))
                        || (x < width - 1 && Changed(x + 1, y))
                        || (y > 0 && Changed(x, y - 1))
                        || (y < height - 1 && Changed(x, y + 1));
                    if (nextToChange)
                    {
                        overlay[(y * width) + x] = outlineColor;
                    }
                    else if ((image.Data[(y * width) + x] >>> 24) != 0)
                    {
                        overlay[(y * width) + x] = fadeColor;
                    }
                }
            }

            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData data = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(overlay, y * width, data.Scan0 + (y * data.Stride), width);
                }
            }
            finally
            {
                result.UnlockBits(data);
            }

            return result;
        }

        /// <summary>
        /// Two fully transparent pixels never differ, whatever their color bits hold.
        /// </summary>
        private static bool Differs(int a, int b, int tolerance)
        {
            if (a == b || ((a >>> 24) == 0 && (b >>> 24) == 0))
            {
                return false;
            }

            for (int shift = 0; shift < 32; shift += 8)
            {
                if (Math.Abs(((a >> shift) & 0xFF) - ((b >> shift) & 0xFF)) > tolerance)
                {
                    return true;
                }
            }

            return false;
        }

        private static Pixels ReadPixels(Bitmap image)
        {
            if (image == null || image.Width <= 0 || image.Height <= 0)
            {
                return new Pixels([], 0, 0);
            }

            int width = image.Width;
            int height = image.Height;
            int[] pixels = new int[width * height];
            BitmapData data = image.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    Marshal.Copy(data.Scan0 + (y * data.Stride), pixels, y * width, width);
                }
            }
            finally
            {
                image.UnlockBits(data);
            }

            return new Pixels(pixels, width, height);
        }
    }
}
