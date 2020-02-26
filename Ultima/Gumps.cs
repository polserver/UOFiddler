using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public sealed class Gumps
    {
		private static FileIndex _mFileIndex = new FileIndex("Gumpidx.mul", "Gumpart.mul", "gumpartLegacyMUL.uop", 0xFFFF, 12, ".tga", -1, true);

        private static Bitmap[] _mCache;
        private static bool[] _mRemoved;
        private static readonly Hashtable _mPatched = new Hashtable();

        private static byte[] _mPixelBuffer;
        private static byte[] _mStreamBuffer;
        private static byte[] _mColorTable;
        static Gumps()
        {
            if (_mFileIndex != null)
            {
                _mCache = new Bitmap[_mFileIndex.Index.Length];
                _mRemoved = new bool[_mFileIndex.Index.Length];
            }
            else
            {
                _mCache = new Bitmap[0xFFFF];
                _mRemoved = new bool[0xFFFF];
            }
        }
        /// <summary>
        /// ReReads gumpart
        /// </summary>
        public static void Reload()
        {
            try
            {
				_mFileIndex = new FileIndex("Gumpidx.mul", "Gumpart.mul", "gumpartLegacyMUL.uop", 12, -1, ".tga", -1, true);
                _mCache = new Bitmap[_mFileIndex.Index.Length];
                _mRemoved = new bool[_mFileIndex.Index.Length];
            }
            catch
            {
                _mFileIndex = null;
                _mCache = new Bitmap[0xFFFF];
                _mRemoved = new bool[0xFFFF];
            }

            _mPixelBuffer = null;
            _mStreamBuffer = null;
            _mColorTable = null;
            _mPatched.Clear();
        }

        public static int GetCount()
        {
            return _mCache.Length;
        }

        /// <summary>
        /// Replaces Gump <see cref="_mCache"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceGump(int index, Bitmap bmp)
        {
            _mCache[index] = bmp;
            _mRemoved[index] = false;
            if (_mPatched.Contains(index))
                _mPatched.Remove(index);

        }

        /// <summary>
        /// Removes Gumpindex <see cref="_mRemoved"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveGump(int index)
        {
            _mRemoved[index] = true;
        }

        /// <summary>
        /// Tests if index is definied
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidIndex(int index)
        {
            if (_mFileIndex == null)
                return false;
            if (index > _mCache.Length - 1)
                return false;
            if (_mRemoved[index])
                return false;
            if (_mCache[index] != null)
                return true;
            int length, extra;
            bool patched;

            if (!_mFileIndex.Valid(index, out length, out extra, out patched))
                return false;
            if (extra == -1)
                return false;
            int width = (extra >> 16) & 0xFFFF;
            int height = extra & 0xFFFF;

            if (width <= 0 || height <= 0)
                return false;

            return true;
        }

        public static byte[] GetRawGump(int index, out int width, out int height)
        {
            width = -1;
            height = -1;
            int length, extra;
            bool patched;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);
            if (stream == null)
                return null;
            if (extra == -1)
                return null;
            width = (extra >> 16) & 0xFFFF;
            height = extra & 0xFFFF;
            if (width <= 0 || height <= 0)
                return null;
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }

        /// <summary>
        /// Returns Bitmap of index and applies Hue
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hue"></param>
        /// <param name="onlyHueGrayPixels"></param>
        /// <returns></returns>
        public unsafe static Bitmap GetGump(int index, Hue hue, bool onlyHueGrayPixels, out bool patched)
        {
            int length, extra;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);

            if (stream == null)
                return null;
            if (extra == -1)
            {
                stream.Close();
                return null;
            }

            int width = (extra >> 16) & 0xFFFF;
            int height = extra & 0xFFFF;

            if (width <= 0 || height <= 0)
            {
                stream.Close();
                return null;
            }

            int bytesPerLine = width << 1;
            int bytesPerStride = (bytesPerLine + 3) & ~3;
            int bytesForImage = height * bytesPerStride;

            int pixelsPerStride = (width + 1) & ~1;
            int pixelsPerStrideDelta = pixelsPerStride - width;

            byte[] pixelBuffer = _mPixelBuffer;

            if (pixelBuffer == null || pixelBuffer.Length < bytesForImage)
                _mPixelBuffer = pixelBuffer = new byte[(bytesForImage + 2047) & ~2047];

            byte[] streamBuffer = _mStreamBuffer;

            if (streamBuffer == null || streamBuffer.Length < length)
                _mStreamBuffer = streamBuffer = new byte[(length + 2047) & ~2047];

            byte[] colorTable = _mColorTable;

            if (colorTable == null)
                _mColorTable = colorTable = new byte[128];

            stream.Read(streamBuffer, 0, length);

            fixed (short* psHueColors = hue.Colors)
            {
                fixed (byte* pbStream = streamBuffer)
                {
                    fixed (byte* pbPixels = pixelBuffer)
                    {
                        fixed (byte* pbColorTable = colorTable)
                        {
                            ushort* pHueColors = (ushort*)psHueColors;
                            ushort* pHueColorsEnd = pHueColors + 32;

                            ushort* pColorTable = (ushort*)pbColorTable;

                            ushort* pColorTableOpaque = pColorTable;

                            while (pHueColors < pHueColorsEnd)
                                *pColorTableOpaque++ = *pHueColors++;

                            ushort* pPixelDataStart = (ushort*)pbPixels;

                            int* pLookup = (int*)pbStream;
                            int* pLookupEnd = pLookup + height;
                            int* pPixelRleStart = pLookup;
                            int* pPixelRle;

                            ushort* pPixel = pPixelDataStart;
                            ushort* pRleEnd = pPixel;
                            ushort* pPixelEnd = pPixel + width;

                            ushort color, count;

                            if (onlyHueGrayPixels)
                            {
                                while (pLookup < pLookupEnd)
                                {
                                    pPixelRle = pPixelRleStart + *pLookup++;
                                    pRleEnd = pPixel;

                                    while (pPixel < pPixelEnd)
                                    {
                                        color = *(ushort*)pPixelRle;
                                        count = *(1 + (ushort*)pPixelRle);
                                        ++pPixelRle;

                                        pRleEnd += count;

                                        if (color != 0 && (color & 0x1F) == ((color >> 5) & 0x1F) && (color & 0x1F) == ((color >> 10) & 0x1F))
                                            color = pColorTable[color >> 10];
                                        else if (color != 0)
                                            color ^= 0x8000;

                                        while (pPixel < pRleEnd)
                                            *pPixel++ = color;
                                    }

                                    pPixel += pixelsPerStrideDelta;
                                    pPixelEnd += pixelsPerStride;
                                }
                            }
                            else
                            {
                                while (pLookup < pLookupEnd)
                                {
                                    pPixelRle = pPixelRleStart + *pLookup++;
                                    pRleEnd = pPixel;

                                    while (pPixel < pPixelEnd)
                                    {
                                        color = *(ushort*)pPixelRle;
                                        count = *(1 + (ushort*)pPixelRle);
                                        ++pPixelRle;

                                        pRleEnd += count;

                                        if (color != 0)
                                            color = pColorTable[color >> 10];

                                        while (pPixel < pRleEnd)
                                            *pPixel++ = color;
                                    }

                                    pPixel += pixelsPerStrideDelta;
                                    pPixelEnd += pixelsPerStride;
                                }
                            }
                            stream.Close();
                            return new Bitmap(width, height, bytesPerStride, PixelFormat.Format16bppArgb1555, (IntPtr)pPixelDataStart);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns Bitmap of index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public unsafe static Bitmap GetGump(int index)
        {
            bool patched;
            return GetGump(index, out patched);
        }

        /// <summary>
        /// Returns Bitmap of index and if verdata patched
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public unsafe static Bitmap GetGump(int index, out bool patched)
        {
            if (_mPatched.Contains(index))
                patched = (bool)_mPatched[index];
            else
                patched = false;
            if (index > _mCache.Length - 1)
                return null;
            if (_mRemoved[index])
                return null;
            if (_mCache[index] != null)
                return _mCache[index];
            int length, extra;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);
            if (stream == null)
                return null;
            if (extra == -1)
            {
                stream.Close();
                return null;
            }
            if (patched)
                _mPatched[index] = true;

            int width = (extra >> 16) & 0xFFFF;
            int height = extra & 0xFFFF;

            if (width <= 0 || height <= 0)
                return null;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            if (_mStreamBuffer == null || _mStreamBuffer.Length < length)
                _mStreamBuffer = new byte[length];
            stream.Read(_mStreamBuffer, 0, length);

            fixed (byte* data = _mStreamBuffer)
            {
                int* lookup = (int*)data;
                ushort* dat = (ushort*)data;

                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                int count = 0;
                for (int y = 0; y < height; ++y, line += delta)
                {
                    count = (*lookup++ * 2);

                    ushort* cur = line;
                    ushort* end = line + bd.Width;

                    while (cur < end)
                    {
                        ushort color = dat[count++];
                        ushort* next = cur + dat[count++];

                        if (color == 0)
                            cur = next;
                        else
                        {
                            color ^= 0x8000;
                            while (cur < next)
                                *cur++ = color;
                        }
                    }
                }
            }

            bmp.UnlockBits(bd);
            if (Files.CacheData)
                return _mCache[index] = bmp;
            else
                return bmp;
        }

        public static unsafe void Save(string path)
        {
            string idx = Path.Combine(path, "Gumpidx.mul");
            string mul = Path.Combine(path, "Gumpart.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int index = 0; index < _mCache.Length; index++)
                    {
                        if (_mCache[index] == null)
                            _mCache[index] = GetGump(index);

                        Bitmap bmp = _mCache[index];
                        if ((bmp == null) || (_mRemoved[index]))
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(-1); // length
                            binidx.Write(-1); // extra
                        }
                        else
                        {
                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            ushort* line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;

                            binidx.Write((int)fsmul.Position); //lookup
                            int length = (int)fsmul.Position;
                            int fill = 0;
                            for (int i = 0; i < bmp.Height; ++i)
                            {
                                binmul.Write(fill);
                            }
                            for (int y = 0; y < bmp.Height; ++y, line += delta)
                            {
                                ushort* cur = line;

                                int x = 0;
                                int current = (int)fsmul.Position;
                                fsmul.Seek(length + y * 4, SeekOrigin.Begin);
                                int offset = (current - length) / 4;
                                binmul.Write(offset);
                                fsmul.Seek(length + offset * 4, SeekOrigin.Begin);

                                while (x < bd.Width)
                                {
                                    int run = 1;
                                    ushort c = cur[x];
                                    while ((x + run) < bd.Width)
                                    {
                                        if (c != cur[x + run])
                                            break;
                                        ++run;
                                    }
                                    if (c == 0)
                                        binmul.Write(c);
                                    else
                                        binmul.Write((ushort)(c ^ 0x8000));
                                    binmul.Write((short)run);
                                    x += run;
                                }
                            }
                            length = (int)fsmul.Position - length;
                            binidx.Write(length);
                            binidx.Write((bmp.Width << 16) + bmp.Height);
                            bmp.UnlockBits(bd);
                        }
                    }
                }
            }
        }
    }
}