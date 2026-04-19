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
using System.Globalization;
using System.IO;

namespace UoFiddler.Plugin.Compare.Classes
{
    public static class Utils
    {
        /// <summary>
        /// The color key auto-detection filter.
        /// The color key is determined by the most common color of the 4x corner pixels.
        /// </summary>
        /// <param name="bmp">Image in 32-bit R8G8B8 format</param>
        /// <returns>Filtered image</returns>
        public static unsafe Bitmap CKeyFilter(Bitmap bmp)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            uint* line = (uint*)bd.Scan0;
            int delta = bd.Stride >> 2;
            uint* line2 = line + delta * (bmp.Height - 1);

            Bitmap newBmp = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            BitmapData newBitmapData = newBmp.LockBits(new Rectangle(0, 0, newBmp.Width, newBmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            uint* newLine = (uint*)newBitmapData.Scan0;
            int newDelta = newBitmapData.Stride >> 2;

            uint colorKey;

            uint colorKey1 = line[0];
            uint colorKey2 = line[bmp.Width - 1];
            uint colorKey3 = line2[0];
            uint colorKey4 = line2[bmp.Width - 1];

            if (colorKey1 == colorKey2 || colorKey1 == colorKey3 || colorKey1 == colorKey4)
            {
                colorKey = colorKey1;
            }
            else if (colorKey2 == colorKey3 || colorKey2 == colorKey4)
            {
                colorKey = colorKey2;
            }
            else if (colorKey3 == colorKey4)
            {
                colorKey = colorKey3;
            }
            else
            {
                colorKey = colorKey4;
            }

            for (int y = 0; y < bmp.Height; ++y, line += delta, newLine += newDelta)
            {
                uint* current = line;
                uint* currentNew = newLine;

                for (int x = 0; x < bmp.Width; ++x)
                {
                    if (current[x] == colorKey)
                    {
                        currentNew[x] = 0;
                    }
                    else
                    {
                        currentNew[x] = current[x];
                    }
                }
            }

            bmp.UnlockBits(bd);
            newBmp.UnlockBits(newBitmapData);

            return newBmp;
        }

        /// <summary>
        /// Black half-tone correction filter.
        /// Brightens the black halftones to 8, 8, 8. To ensure that black halftones are not mistaken for a color key when converting to R5G5B5.
        /// </summary>
        /// <param name="bmp">Image in 32-bit R8G8B8 format</param>
        /// <param name="forceCCol2BCol">If true, it lightens the color key to 8, 8, 8.</param>
        /// <returns>Filtered image</returns>
        public static unsafe Bitmap BColFilter(Bitmap bmp, bool forceCCol2BCol)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            uint* line = (uint*)bd.Scan0;
            int delta = bd.Stride >> 2;

            Bitmap bmpnew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            BitmapData bdnew = bmpnew.LockBits(new Rectangle(0, 0, bmpnew.Width, bmpnew.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            uint* linenew = (uint*)bdnew.Scan0;
            int deltanew = bdnew.Stride >> 2;

            for (int Y = 0; Y < bmp.Height; ++Y, line += delta, linenew += deltanew)
            {
                uint* cur = line;
                uint* curnew = linenew;
                for (int X = 0; X < bmp.Width; ++X)
                {
                    byte a = (byte)((cur[X] & 0xFF000000) >> 24);
                    byte r = (byte)((cur[X] & 0x00FF0000) >> 16);
                    byte g = (byte)((cur[X] & 0x0000FF00) >> 8);
                    byte b = (byte)((cur[X] & 0x000000FF) >> 0);

                    if (r < 8 && g < 8 && b < 8)
                    {
                        byte max = Math.Max(r, Math.Max(g, b));
                        if (!forceCCol2BCol)
                        {
                            if (r != 0 && r == max)
                            {
                                curnew[X] |= 0xFF080000;
                            }

                            if (g != 0 && g == max)
                            {
                                curnew[X] |= 0xFF000800;
                            }

                            if (b != 0 && b == max)
                            {
                                curnew[X] |= 0xFF000008;
                            }
                        }
                        else
                        {
                            if (r == max)
                            {
                                curnew[X] |= 0xFF080000;
                            }

                            if (g == max)
                            {
                                curnew[X] |= 0xFF000800;
                            }

                            if (b == max)
                            {
                                curnew[X] |= 0xFF000008;
                            }
                        }
                    }
                    else
                    {
                        curnew[X] = cur[X];
                    }
                }
            }
            bmp.UnlockBits(bd);
            bmpnew.UnlockBits(bdnew);
            return bmpnew;
        }

        /// <summary>
        /// Фильтр коррекции белых полутанов.
        /// Затемняет белые полутона до 247, 247, 247. Для того чтобы при конвертировании в R5G5B5 белые полутона небыли ошибочно восприняты как цветовой ключ.
        /// </summary>
        /// <param name="bmp">Изображение в 32х битном формате R8G8B8</param>
        /// <param name="forceCCol2BCol">Если true то осветляет цветовой ключ до 247, 247, 247.</param>
        /// <returns>Filtered image</returns>
        public static unsafe Bitmap WColFilter(Bitmap bmp, bool forceCCol2WCol)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            uint* line = (uint*)bd.Scan0;
            int delta = bd.Stride >> 2;

            Bitmap bmpnew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            BitmapData bdnew = bmpnew.LockBits(new Rectangle(0, 0, bmpnew.Width, bmpnew.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            uint* linenew = (uint*)bdnew.Scan0;
            int deltanew = bdnew.Stride >> 2;

            for (int Y = 0; Y < bmp.Height; ++Y, line += delta, linenew += deltanew)
            {
                uint* cur = line;
                uint* curnew = linenew;
                for (int X = 0; X < bmp.Width; ++X)
                {
                    byte a = (byte)((cur[X] & 0xFF000000) >> 24);
                    byte r = (byte)((cur[X] & 0x00FF0000) >> 16);
                    byte g = (byte)((cur[X] & 0x0000FF00) >> 8);
                    byte b = (byte)((cur[X] & 0x000000FF) >> 0);

                    if (r > 247 && g > 247 && b > 247)
                    {
                        byte min = Math.Min(r, Math.Max(g, b));
                        if (!forceCCol2WCol)
                        {
                            if (r != 255 && r == min)
                            {
                                curnew[X] |= 0xFFF70000;
                            }
                            else
                            {
                                curnew[X] |= ((uint)r << 16);
                            }

                            if (g != 255 && g == min)
                            {
                                curnew[X] |= 0xFF00F700;
                            }
                            else
                            {
                                curnew[X] |= ((uint)g << 8);
                            }

                            if (b != 255 && b == min)
                            {
                                curnew[X] |= 0xFF0000F7;
                            }
                            else
                            {
                                curnew[X] |= ((uint)b << 0);
                            }
                        }
                        else
                        {
                            if (r == min)
                            {
                                curnew[X] |= 0xFFF70000;
                            }
                            else
                            {
                                curnew[X] |= ((uint)r << 16);
                            }

                            if (g == min)
                            {
                                curnew[X] |= 0xFF00F700;
                            }
                            else
                            {
                                curnew[X] |= ((uint)g << 8);
                            }

                            if (b == min)
                            {
                                curnew[X] |= 0xFF0000F7;
                            }
                            else
                            {
                                curnew[X] |= ((uint)b << 0);
                            }
                        }
                    }
                    else
                    {
                        curnew[X] = cur[X];
                    }
                }
            }
            bmp.UnlockBits(bd);
            bmpnew.UnlockBits(bdnew);
            return bmpnew;
        }

        /// <summary>
        /// Фильтр для стирания нижних границ тайла для убирания эфекта "сетки" при использовании тайла с флагом Translucent.
        /// Входящее изображение должно иметь ширину 44, иначе фильтр возвратит передаенное ему изображение.
        /// </summary>
        /// <param name="bmp">Изображение в 32х битном формате R8G8B8</param>
        /// <param name="onlyraw">Если true то тайлы с высотой отличной от 44 не преобразуются.</param>
        /// <returns>Filtered image</returns>
        public static unsafe Bitmap CuttingRawBmpForTranslucent(Bitmap bmp, bool onlyraw = true)
        {
            if ((bmp.Height != 0 && bmp.Width != 44) || (onlyraw && bmp.Height != 44))
            {
                return bmp;
            }

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            uint* line = (uint*)bd.Scan0;
            int delta = bd.Stride >> 2;

            Bitmap bmpnew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppRgb);
            BitmapData bdnew = bmpnew.LockBits(new Rectangle(0, 0, bmpnew.Width, bmpnew.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);
            uint* linenew = (uint*)bdnew.Scan0;
            int deltanew = bdnew.Stride >> 2;

            int y0 = Math.Max(0, bmp.Height - 22);
            //line += y0 * delta;
            //linenew += y0 * deltanew;

            int x1 = y0 > 0 ? 0 : 22 - bmp.Height;
            int x2 = y0 > 0 ? 43 : 43 - x1;

            for (int Y = 0; Y < y0; ++Y, line += delta, linenew += deltanew)
            {
                uint* cur = line;
                uint* curnew = linenew;
                for (int X = 0; X < bmp.Width; ++X)
                    curnew[X] = cur[X];
            }
            line = (uint*)bd.Scan0 + y0 * delta;
            linenew = (uint*)bdnew.Scan0 + y0 * deltanew;
            for (int Y = y0; Y < bmp.Height; ++Y, line += delta, linenew += deltanew)
            {
                uint* cur = line;
                uint* curnew = linenew;
                for (int X = 0; X < bmp.Width; ++X)
                {
                    if (X != x1 && X != x2)
                    {
                        curnew[X] = cur[X];
                    }
                    else
                    {
                        curnew[X] = unchecked((ushort)0xFF000000);
                    }
                }
                ++x1;
                --x2;
            }
            bmp.UnlockBits(bd);
            bmpnew.UnlockBits(bdnew);
            return bmpnew;
        }

        /// <summary>
        /// Вычисляет средний цвет в изображении.
        /// Подсчет введется как среднее арифмитическое составляющие всех цветов, кроме абсолютно черного (0,0,0).
        /// </summary>
        /// <param name="bmp">Изображение в 32х битном формате R8G8B8 или 16 битном A1R5G5B5.</param>
        /// <param name="noneBlack">Если true то осветляет возвращаемый результат до (8,8,8) если он равен (0,0,0).</param>
        /// <returns>Hue - цвет в 16 битном формате.</returns>
        public static unsafe ushort AverageCol(Bitmap bmp, bool noneBlack = true)
        {
            ulong r = 0;
            ulong g = 0;
            ulong b = 0;
            ulong count = 0;

            if (bmp.PixelFormat == PixelFormat.Format32bppRgb)
            {
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                uint* line = (uint*)bd.Scan0;
                int delta = bd.Stride >> 2;
                for (int Y = 0; Y < bmp.Height; ++Y, line += delta)
                {
                    uint* cur = line;
                    for (int X = 0; X < bmp.Width; ++X)
                    {
                        if ((uint)(cur[X] & 0x00FFFFFF) == 0)
                        {
                            continue;
                        }

                        r += (ulong)((cur[X] & 0x00FF0000) >> 16);
                        g += (ulong)((cur[X] & 0x0000FF00) >> 8);
                        b += (ulong)((cur[X] & 0x000000FF) >> 0);
                        ++count;
                    }
                }
                bmp.UnlockBits(bd);
            }
            else if (bmp.PixelFormat == PixelFormat.Format16bppArgb1555)
            {
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int Y = 0; Y < bmp.Height; ++Y, line += delta)
                {
                    ushort* cur = line;
                    for (int X = 0; X < bmp.Width; ++X)
                    {
                        if ((ushort)(cur[X] & 0x7FFF) == 0)
                        {
                            continue;
                        }

                        r += (ulong)((cur[X] & 0x7C00) >> 10);
                        g += (ulong)((cur[X] & 0x03E0) >> 5);
                        b += (ulong)((cur[X] & 0x001F) >> 0);
                        ++count;
                    }
                }
                bmp.UnlockBits(bd);
            }
            else
            {
                throw new ArgumentException("Неподдерживываемый формат пикселя");
            }

            r = (ulong)Math.Round(((double)r / (double)count));
            g = (ulong)Math.Round(((double)g / (double)count));
            b = (ulong)Math.Round(((double)b / (double)count));

            ushort hue = 0x0421;
            if (bmp.PixelFormat == PixelFormat.Format32bppRgb)
            {
                hue = Ultima.Hues.ColorToHue(Color.FromArgb((int)r, (int)g, (int)b));
            }
            else if (bmp.PixelFormat == PixelFormat.Format16bppArgb1555)
            {
                hue = (ushort)((r << 10) | (g << 5) | (b));
            }

            if (noneBlack && (ushort)(hue & 0x7FFF) == 0)
            {
                hue = 0x0421;
            }

            return hue;
        }

        public static bool CompareBitmaps(Bitmap bmp10, Bitmap bmp20)
        {
            Bitmap bmp1 = new Bitmap(bmp10);
            Bitmap bmp2 = new Bitmap(bmp20);
            if (bmp1.Width != bmp2.Width || bmp1.Height != bmp2.Height)
            {
                return false;
            }

            ImageLockMode Mode = ImageLockMode.ReadWrite;
            Rectangle Range = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
            BitmapData BMPD1 = bmp1.LockBits(Range, Mode, bmp1.PixelFormat);
            BitmapData BMPD2 = bmp2.LockBits(Range, Mode, bmp2.PixelFormat);
            bool result = true;
            try
            {
                unsafe
                {
                    byte* p1 = (byte*)(void*)BMPD1.Scan0;
                    byte* p2 = (byte*)(void*)BMPD2.Scan0;

                    int c = Range.Height * BMPD1.Stride;
                    for (int i = 0; i < c; i++)
                    {
                        if (*p1 != *p2)
                        {
                            result = false;
                            break;
                        }
                        p1++;
                        p2++;
                    }
                }
            }
            finally
            {

            }

            bmp1.UnlockBits(BMPD1);
            bmp2.UnlockBits(BMPD2);
            return result;
        }

        public static unsafe Bitmap ConvertBmp(Bitmap bmp)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpnew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdnew = bmpnew.LockBits(new Rectangle(0, 0, bmpnew.Width, bmpnew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* linenew = (ushort*)bdnew.Scan0;
            int deltanew = bdnew.Stride >> 1;

            for (int Y = 0; Y < bmp.Height; ++Y, line += delta, linenew += deltanew)
            {
                ushort* cur = line;
                ushort* curnew = linenew;
                for (int X = 0; X < bmp.Width; ++X)
                {
                    if ((cur[X] != 32768) && (cur[X] != 65535)) //True Black/White
                    {
                        curnew[X] = cur[X];
                    }
                }
            }
            bmp.UnlockBits(bd);
            bmpnew.UnlockBits(bdnew);
            return bmpnew;
        }

        public static unsafe Bitmap ConvertBmpAnim(Bitmap bmp, int Red, int Green, int Blue)
        {
            //Extra background
            int ExtraBack;
            ExtraBack = (Red / 8) * 1024 + (Green / 8) * 32 + (Blue / 8) + 32768;
            //
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpnew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdnew = bmpnew.LockBits(new Rectangle(0, 0, bmpnew.Width, bmpnew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* linenew = (ushort*)bdnew.Scan0;
            int deltanew = bdnew.Stride >> 1;

            for (int Y = 0; Y < bmp.Height; ++Y, line += delta, linenew += deltanew)
            {
                ushort* cur = line;
                ushort* curnew = linenew;
                for (int X = 0; X < bmp.Width; ++X)
                {
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    //if ((cur[X] == 32768))
                    //    curnew[X] = 32769;
                    //if ((cur[X] != 65535 & cur[X] != ExtraBack & cur[X] > 32768)) //True White == BackGround
                    //    curnew[X] = cur[X];
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpnew.UnlockBits(bdnew);
            return bmpnew;
        }

        public static unsafe Bitmap ConvertBmpAnimCV5(Bitmap bmp, int Red, int Green, int Blue)
        {
            //Extra background
            int ExtraBack;
            ExtraBack = (Red / 8) * 1024 + (Green / 8) * 32 + (Blue / 8) + 32768;
            //
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpnew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdnew = bmpnew.LockBits(new Rectangle(0, 0, bmpnew.Width, bmpnew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* linenew = (ushort*)bdnew.Scan0;
            int deltanew = bdnew.Stride >> 1;

            for (int Y = 0; Y < bmp.Height; ++Y, line += delta, linenew += deltanew)
            {
                ushort* cur = line;
                ushort* curnew = linenew;
                for (int X = 0; X < bmp.Width; ++X)
                {
                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    //if ((cur[X] == 32768))
                    //    curnew[X] = 32769;
                    //if ((cur[X] != 65535 & cur[X] != 54965 & cur[X] != ExtraBack & cur[X] > 32768)) //True White == BackGround
                    //    curnew[X] = cur[X];
                    //End of Soulblighter Modification
                }
            }
            bmp.UnlockBits(bd);
            bmpnew.UnlockBits(bdnew);
            return bmpnew;
        }

        public static unsafe Bitmap ConvertBmpAnimKR(Bitmap bmp, int Red, int Green, int Blue)
        {
            //Extra background
            //int extraBack = (Red / 8) * 1024 + (Green / 8) * 32 + (Blue / 8) + 32768;
            //

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            Bitmap bmpnew = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format16bppArgb1555);
            BitmapData bdnew = bmpnew.LockBits(new Rectangle(0, 0, bmpnew.Width, bmpnew.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* linenew = (ushort*)bdnew.Scan0;
            int deltanew = bdnew.Stride >> 1;

            for (int Y = 0; Y < bmp.Height; ++Y, line += delta, linenew += deltanew)
            {
                ushort* cur = line;
                ushort* curnew = linenew;

                for (int X = 0; X < bmp.Width; ++X)
                {
                    //if (cur[X] != 53235)
                    //{
                    // Convert back to RGB
                    int BlueTemp = ((cur[X] - 32768) / 32);
                    BlueTemp = BlueTemp * 32;
                    BlueTemp = (cur[X] - 32768) - BlueTemp;

                    int GreenTemp = ((cur[X] - 32768) / 1024);
                    GreenTemp = GreenTemp * 1024;
                    GreenTemp = (cur[X] - 32768) - GreenTemp - BlueTemp;
                    GreenTemp = GreenTemp / 32;

                    int RedTemp = ((cur[X] - 32768) / 1024);

                    // remove green colors
                    if (GreenTemp > BlueTemp & GreenTemp > RedTemp & GreenTemp > 10)
                    {
                        cur[X] = 65535;
                    }
                    //}

                    //My Soulblighter Modification
                    // Convert color 0,0,0 to 0,0,8
                    //if ((cur[X] == 32768))
                    //    curnew[X] = 32769;
                    //if ((cur[X] != 65535 & cur[X] != 54965 & cur[X] != ExtraBack & cur[X] > 32768)) //True White == BackGround
                    //    curnew[X] = cur[X];
                    //End of Soulblighter Modification
                }
            }

            bmp.UnlockBits(bd);
            bmpnew.UnlockBits(bdnew);

            return bmpnew;
        }

        private static ColorPalette GetColorPalette(uint nColors)
        {
            // Assume monochrome image.
            PixelFormat bitscolordepth = PixelFormat.Format1bppIndexed;

            // Determine number of colors.
            if (nColors > 2)
            {
                bitscolordepth = PixelFormat.Format4bppIndexed;
            }

            if (nColors > 16)
            {
                bitscolordepth = PixelFormat.Format8bppIndexed;
            }

            // Make a new Bitmap object to get its Palette.
            ColorPalette palette;

            using (Bitmap bitmap = new Bitmap(1, 1, bitscolordepth))
            {
                palette = bitmap.Palette;
            }

            return palette;
        }

        private static ColorPalette GetColorPalette(uint nColors, Color[] usedColors)
        {
            PixelFormat bitscolordepth = nColors > 16
                ? PixelFormat.Format8bppIndexed
                : nColors > 2
                    ? PixelFormat.Format4bppIndexed
                    : PixelFormat.Format1bppIndexed;

            Bitmap bitmap = new Bitmap(1, 1, bitscolordepth);
            ColorPalette palette = bitmap.Palette;
            //palette.Flags = 0x00000001; // The color values in the array contain information about the alpha component.
            bitmap.Dispose();

            Array.Sort(usedColors);

            if (usedColors.Length > nColors)
            {

            }

            palette.Entries[0] = Color.FromArgb(0, 0, 0, 0);

            for (uint i = 1; i < nColors; i++)
            {

                uint intensity = i * 0xFF / (nColors - 1);    // Even distribution. 

                // The GIF encoder makes the first entry in the palette
                // that has a ZERO alpha the transparent color in the GIF.
                // Pick the first one arbitrarily, for demonstration purposes.

                //if (i == 0 && fTransparent) // Make this color index...
                int alpha = 0;

                // Create a gray scale for demonstration purposes.
                // Otherwise, use your favorite color reduction algorithm
                // and an optimum palette for that algorithm generated here.
                // For example, a color histogram, or a median cut palette.
                palette.Entries[i] = Color.FromArgb((int)alpha, (int)intensity, (int)intensity, (int)intensity);
            }

            return palette;
        }

        public static void SaveGifWithNewColorTable(Image image, string filename, uint nColors, bool fTransparent)
        {
            // GIF codec supports 256 colors maximum, monochrome minimum.
            if (nColors > 256)
            {
                nColors = 256;
            }

            if (nColors < 2)
            {
                nColors = 2;
            }

            // Make a new 8-BPP indexed bitmap that is the same size as the source image.
            int width = image.Width;
            int height = image.Height;

            // Always use PixelFormat8bppIndexed because that is the color
            // table-based interface to the GIF codec.
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            // Create a color palette big enough to hold the colors you want.
            ColorPalette pal = GetColorPalette(nColors);

            // Initialize a new color table with entries that are determined
            // by some optimal palette-finding algorithm; for demonstration 
            // purposes, use a greyscale.
            for (uint i = 0; i < nColors; i++)
            {
                uint alpha = 0xFF;                      // Colors are opaque.
                uint intensity = i * 0xFF / (nColors - 1);    // Even distribution. 

                // The GIF encoder makes the first entry in the palette
                // that has a ZERO alpha the transparent color in the GIF.
                // Pick the first one arbitrarily, for demonstration purposes.

                if (i == 0 && fTransparent) // Make this color index...
                {
                    alpha = 0;          // Transparent
                }

                // Create a gray scale for demonstration purposes.
                // Otherwise, use your favorite color reduction algorithm
                // and an optimum palette for that algorithm generated here.
                // For example, a color histogram, or a median cut palette.
                pal.Entries[i] = Color.FromArgb((int)alpha,
                                                 (int)intensity,
                                                 (int)intensity,
                                                 (int)intensity);
            }

            // Set the palette into the new Bitmap object.
            bitmap.Palette = pal;


            // Use GetPixel below to pull out the color data of Image.
            // Because GetPixel isn't defined on an Image, make a copy 
            // in a Bitmap instead. Make a new Bitmap that is the same size as the
            // image that you want to export. Or, try to
            // interpret the native pixel format of the image by using a LockBits
            // call. Use PixelFormat32BppARGB so you can wrap a Graphics  
            // around it.
            Bitmap bmpCopy = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            {
                Graphics g = Graphics.FromImage(bmpCopy);

                g.PageUnit = GraphicsUnit.Pixel;

                // Transfer the Image to the Bitmap
                g.DrawImage(image, 0, 0, width, height);

                // g goes out of scope and is marked for garbage collection.
                // Force it, just to keep things clean.
                g.Dispose();
            }

            // Lock a rectangular portion of the bitmap for writing.
            Rectangle rect = new Rectangle(0, 0, width, height);

            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            // Write to the temporary buffer that is provided by LockBits.
            // Copy the pixels from the source image in this loop.
            // Because you want an index, convert RGB to the appropriate
            // palette index here.
            IntPtr pixels = bitmapData.Scan0;

            unsafe
            {
                // Get the pointer to the image bits.
                // This is the unsafe operation.
                byte* pBits;
                if (bitmapData.Stride > 0)
                {
                    pBits = (byte*)pixels.ToPointer();
                }
                else
                    // If the Stide is negative, Scan0 points to the last 
                    // scanline in the buffer. To normalize the loop, obtain
                    // a pointer to the front of the buffer that is located 
                    // (Height-1) scanlines previous.
                {
                    pBits = (byte*)pixels.ToPointer() + bitmapData.Stride * (height - 1);
                }

                uint stride = (uint)Math.Abs(bitmapData.Stride);

                for (uint row = 0; row < height; ++row)
                {
                    for (uint col = 0; col < width; ++col)
                    {
                        // Map palette indexes for a gray scale.
                        // If you use some other technique to color convert,
                        // put your favorite color reduction algorithm here.
                        Color pixel;    // The source pixel.

                        // The destination pixel.
                        // The pointer to the color index byte of the
                        // destination; this real pointer causes this
                        // code to be considered unsafe.
                        byte* p8bppPixel = pBits + row * stride + col;

                        pixel = bmpCopy.GetPixel((int)col, (int)row);

                        // Use luminance/chrominance conversion to get grayscale.
                        // Basically, turn the image into black and white TV.
                        // Do not calculate Cr or Cb because you 
                        // discard the color anyway.
                        // Y = Red * 0.299 + Green * 0.587 + Blue * 0.114

                        // This expression is best as integer math for performance,
                        // however, because GetPixel listed earlier is the slowest 
                        // part of this loop, the expression is left as 
                        // floating point for clarity.

                        double luminance = (pixel.R * 0.299) +
                                           (pixel.G * 0.587) +
                                           (pixel.B * 0.114);

                        // Gray scale is an intensity map from black to white.
                        // Compute the index to the grayscale entry that
                        // approximates the luminance, and then round the index.
                        // Also, constrain the index choices by the number of
                        // colors to do, and then set that pixel's index to the 
                        // byte value.
                        *p8bppPixel = (byte)(luminance * (nColors - 1) / 255 + 0.5);

                    } /* end loop for col */
                } /* end loop for row */
            } /* end unsafe */

            // To commit the changes, unlock the portion of the bitmap.  
            bitmap.UnlockBits(bitmapData);

            bitmap.Save(filename, ImageFormat.Gif);

            // Bitmap goes out of scope here and is also marked for
            // garbage collection.
            // Pal is referenced by bitmap and goes away.
            // BmpCopy goes out of scope here and is marked for garbage
            // collection. Force it, because it is probably quite large.
            // The same applies to bitmap.
            bmpCopy.Dispose();
            bitmap.Dispose();

        }

        public static void FileRename(string folder, int startindex)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            var files = Directory.GetFiles(folder, "*.bmp", SearchOption.AllDirectories);
            var dicts = new Dictionary<int, string>(files.Length);
            foreach (var file in files)
            {
                string str_id;
                int int_id;

                if (Path.GetFileNameWithoutExtension(file)[1] == '0' && Path.GetFileNameWithoutExtension(file)[2] == 'x')
                {
                    str_id = Path.GetFileNameWithoutExtension(file).Substring(3);
                    if (int.TryParse(str_id, NumberStyles.HexNumber, null, out int_id))
                    {
                        dicts.Add(int_id, file);
                    }
                }
                else
                {
                    str_id = Path.GetFileNameWithoutExtension(file).Substring(1);
                    if (int.TryParse(str_id, NumberStyles.Integer, null, out int_id))
                    {
                        dicts.Add(int_id, file);
                    }
                }
            }

            var keys = new List<int>(dicts.Keys);
            keys.Sort();

            foreach (int key in keys)
            {
                var outf = Path.Combine(Path.Combine(folder, "__RENAMED__"), Path.GetDirectoryName(dicts[key]).Substring(folder.Length));
                var outn = $"{Path.GetFileName(dicts[key])[0]}0x{++startindex:X2}.{Path.GetExtension(dicts[key])}";
                Directory.CreateDirectory(outf);
                File.Move(dicts[key], Path.Combine(outf, outn));
            }
        }
    }

    //public class MyEventArgs : EventArgs
    //{
    //    public enum TYPES
    //    {
    //        COMMON = 0,
    //        FORCERELOAD
    //    }
    //    public TYPES Type { get; private set; }
    //    public MyEventArgs() { Type = TYPES.COMMON; }
    //    public MyEventArgs(TYPES type) { Type = type; }
    //}
}
