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
