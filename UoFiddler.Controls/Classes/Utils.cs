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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;

namespace UoFiddler.Controls.Classes
{
    public static class Utils
    {
        /// <summary>
        /// Alpha value at which a pasted pixel is still considered opaque.
        /// </summary>
        private const uint AlphaCutoff = 128;

        /// <summary>
        /// Converts string to int with Hex recognition
        /// </summary>
        /// <param name="text">string to parse</param>
        /// <param name="result">out result</param>
        /// <param name="min">minvalue</param>
        /// <param name="max">maxvalue</param>
        /// <returns>bool could convert and between min/max</returns>
        public static bool ConvertStringToInt(string text, out int result, int min, int max)
        {
            bool canDone;
            if (text.Contains("0x"))
            {
                string convert = text.Replace("0x", "");
                canDone = int.TryParse(convert, NumberStyles.HexNumber, null, out result);
            }
            else
            {
                canDone = int.TryParse(text, NumberStyles.Integer, null, out result);
            }

            if (result > max || result < min)
            {
                canDone = false;
            }

            return canDone;
        }

        /// <summary>
        /// Converts string to int with Hex recognition
        /// </summary>
        /// <param name="text">string to parse</param>
        /// <param name="result">out result</param>
        /// <returns>bool could convert</returns>
        public static bool ConvertStringToInt(string text, out int result)
        {
            if (text.Contains("0x"))
            {
                string convert = text.Replace("0x", "");
                return int.TryParse(convert, NumberStyles.HexNumber, null, out result);
            }

            return int.TryParse(text, NumberStyles.Integer, null, out result);
        }

        /// <summary>
        /// Formats an ID for inclusion in an exported filename, honoring the user's
        /// hex/decimal preference in <see cref="Options.ExportFilenameInHex"/> and
        /// <see cref="Options.ExportFilenameDecimalPadded"/>.
        /// </summary>
        public static string FormatExportId(int id)
        {
            if (Options.ExportFilenameInHex)
            {
                return $"0x{id:X4}";
            }

            return Options.ExportFilenameDecimalPadded
                ? id.ToString("D5")
                : id.ToString();
        }

        public static unsafe Bitmap ConvertBmp(Bitmap bmp)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpNew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdNew = bmpNew.LockBits(new Rectangle(0, 0, bmpNew.Width, bmpNew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* lineNew = (ushort*)bdNew.Scan0;
            int deltaNew = bdNew.Stride >> 1;

            for (int y = 0; y < bmp.Height; ++y, line += delta, lineNew += deltaNew)
            {
                ushort* cur = line;
                ushort* curNew = lineNew;
                for (int x = 0; x < bmp.Width; ++x)
                {
                    if (cur[x] != 32768 && cur[x] != 65535) //True Black/White
                    {
                        curNew[x] = cur[x];
                    }
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        /// <summary>
        /// Converts an arbitrary bitmap into the 16bppArgb1555 form the mul save paths expect.
        /// </summary>
        /// <remarks>
        /// Which pixels end up transparent depends on what the source actually carries. An image with
        /// a real alpha channel is taken at its word, so pure black stays black. An image without one
        /// - a screenshot, a flattened bmp - falls back to <see cref="ConvertBmp"/>, whose pure
        /// black/pure white rule is what the Replace from file paths have always used.
        /// </remarks>
        public static unsafe Bitmap ToUoBitmap(Bitmap source)
        {
            if (!HasTranslucency(source))
            {
                return ConvertBmp(source);
            }

            Rectangle rectangle = new Rectangle(0, 0, source.Width, source.Height);

            Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format16bppArgb1555);
            BitmapData sourceData = source.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData resultData = result.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            try
            {
                uint* sourceLine = (uint*)sourceData.Scan0;
                int sourceDelta = sourceData.Stride >> 2;

                ushort* resultLine = (ushort*)resultData.Scan0;
                int resultDelta = resultData.Stride >> 1;

                for (int y = 0; y < source.Height; ++y, sourceLine += sourceDelta, resultLine += resultDelta)
                {
                    for (int x = 0; x < source.Width; ++x)
                    {
                        uint argb = sourceLine[x];

                        // One bit of alpha is all the format has, so anything half transparent or
                        // more drops out entirely.
                        resultLine[x] = (argb >> 24) < AlphaCutoff
                            ? (ushort)0
                            : (ushort)(0x8000 | ((argb >> 9) & 0x7C00) | ((argb >> 6) & 0x03E0) | ((argb >> 3) & 0x001F));
                    }
                }
            }
            finally
            {
                source.UnlockBits(sourceData);
                result.UnlockBits(resultData);
            }

            return result;
        }

        /// <summary>
        /// True when the bitmap declares an alpha channel and at least one pixel actually uses it.
        /// </summary>
        private static unsafe bool HasTranslucency(Bitmap bmp)
        {
            if (!Image.IsAlphaPixelFormat(bmp.PixelFormat))
            {
                return false;
            }

            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                uint* line = (uint*)data.Scan0;
                int delta = data.Stride >> 2;

                for (int y = 0; y < bmp.Height; ++y, line += delta)
                {
                    for (int x = 0; x < bmp.Width; ++x)
                    {
                        if ((line[x] >> 24) != 0xFF)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return false;
        }

        public static string GetFileExtensionFor(ImageFormat imageFormat)
        {
            if (Equals(imageFormat, ImageFormat.Bmp))
            {
                return "bmp";
            }

            if (Equals(imageFormat, ImageFormat.Tiff))
            {
                return "tiff";
            }

            if (Equals(imageFormat, ImageFormat.Jpeg))
            {
                return "jpg";
            }

            if (Equals(imageFormat, ImageFormat.Png))
            {
                return "png";
            }

            throw new ArgumentException($"Image format {imageFormat} is not supported", nameof(imageFormat));
        }
    }
}
