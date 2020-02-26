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

using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;

namespace FiddlerControls
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
            else
            {
                return int.TryParse(text, NumberStyles.Integer, null, out result);
            }
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

        public static unsafe Bitmap ConvertBmpAnim(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = red / 8 * 1024 + green / 8 * 32 + blue / 8 + 32768;

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
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 & cur[x] != extraBack & cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        public static unsafe Bitmap ConvertBmpAnimCv5(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = red / 8 * 1024 + green / 8 * 32 + blue / 8 + 32768;

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
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 & cur[x] != 54965 & cur[x] != extraBack & cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }

        public static unsafe Bitmap ConvertBmpAnimKr(Bitmap bmp, int red, int green, int blue)
        {
            //Extra background
            int extraBack = red / 8 * 1024 + green / 8 * 32 + blue / 8 + 32768;
            //
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
                    //if (cur[X] != 53235)
                    //{
                    // Convert back to RGB
                    int blueTemp = (cur[x] - 32768) / 32;
                    blueTemp *= 32;
                    blueTemp = cur[x] - 32768 - blueTemp;
                    int greenTemp = (cur[x] - 32768) / 1024;
                    greenTemp *= 1024;
                    greenTemp = cur[x] - 32768 - greenTemp - blueTemp;
                    greenTemp /= 32;
                    int redTemp = (cur[x] - 32768) / 1024;
                    // remove green colors
                    if (greenTemp > blueTemp & greenTemp > redTemp & greenTemp > 10)
                    {
                        cur[x] = 65535;
                    }
                    //}

                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    if (cur[x] == 32768)
                    {
                        curNew[x] = 32769;
                    }

                    if (cur[x] != 65535 & cur[x] != 54965 & cur[x] != extraBack & cur[x] > 32768) //True White == BackGround
                    {
                        curNew[x] = cur[x];
                    }
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpNew.UnlockBits(bdNew);
            return bmpNew;
        }
    }
}




