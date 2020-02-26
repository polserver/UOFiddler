using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima;

namespace ComparePlugin
{
    internal static class SecondGump
    {
        private static SecondFileIndex _mFileIndex;

        private static Bitmap[] _mCache = new Bitmap[0x10000];
        private static byte[] _mStreamBuffer;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            _mFileIndex = new SecondFileIndex(idxPath, mulPath, 0x10000);
            _mCache = new Bitmap[0x10000];
        }

        /// <summary>
        /// Tests if index is defined
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidIndex(int index)
        {
            if (_mFileIndex == null)
            {
                return false;
            }

            if (_mCache[index] != null)
            {
                return true;
            }

            if (!_mFileIndex.Valid(index, out _, out int extra))
            {
                return false;
            }

            if (extra == -1)
            {
                return false;
            }

            int width = (extra >> 16) & 0xFFFF;
            int height = extra & 0xFFFF;

            return width > 0 && height > 0;
        }

        public static byte[] GetRawGump(int index, out int width, out int height)
        {
            width = -1;
            height = -1;
            Stream stream = _mFileIndex.Seek(index, out int length, out int extra);
            if (stream == null)
            {
                return null;
            }

            if (extra == -1)
            {
                return null;
            }

            width = (extra >> 16) & 0xFFFF;
            height = extra & 0xFFFF;
            if (width <= 0 || height <= 0)
            {
                return null;
            }

            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// Returns Bitmap of index and if verdata patched
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static unsafe Bitmap GetGump(int index)
        {
            if (_mCache[index] != null)
            {
                return _mCache[index];
            }

            Stream stream = _mFileIndex.Seek(index, out int length, out int extra);
            if (stream == null)
            {
                return null;
            }

            if (extra == -1)
            {
                return null;
            }

            int width = (extra >> 16) & 0xFFFF;
            int height = extra & 0xFFFF;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            if (_mStreamBuffer == null || _mStreamBuffer.Length < length)
            {
                _mStreamBuffer = new byte[length];
            }

            stream.Read(_mStreamBuffer, 0, length);

            fixed (byte* data = _mStreamBuffer)
            {
                int* lookup = (int*)data;
                ushort* dat = (ushort*)data;

                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < height; ++y, line += delta)
                {
                    int count = *lookup++ * 2;
                    ushort* cur = line;
                    ushort* end = line + bd.Width;

                    while (cur < end)
                    {
                        ushort color = dat[count++];
                        ushort* next = cur + dat[count++];

                        if (color == 0)
                        {
                            cur = next;
                        }
                        else
                        {
                            color ^= 0x8000;
                            while (cur < next)
                            {
                                *cur++ = color;
                            }
                        }
                    }
                }
            }

            bmp.UnlockBits(bd);

            return Files.CacheData ? _mCache[index] = bmp : bmp;
        }
    }
}
