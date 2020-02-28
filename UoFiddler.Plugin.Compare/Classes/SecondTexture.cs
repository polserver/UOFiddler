using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondTexture
    {
        private static SecondFileIndex _fileIndex;
        private static Bitmap[] _cache;
        private static byte[] _streamBuffer;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            _fileIndex = new SecondFileIndex(idxPath, mulPath, 0x4000);
            _cache = new Bitmap[0x4000];
        }

        // public static int GetIdxLength()
        // {
        //     return (int)(_fileIndex.IdxLength / 12);
        // }

        public static bool IsValidTexture(int index)
        {
            index &= 0x3FFF;
            if (_cache[index] != null)
            {
                return true;
            }

            bool valid = _fileIndex.Valid(index, out int length, out _);

            return valid && length != 0;
        }

        public static Bitmap GetTexture(int index)
        {
            index &= 0x3FFF;

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out _, out int extra);
            if (stream == null)
            {
                return null;
            }

            return Files.CacheData
                ? _cache[index] = LoadTexture(stream, extra)
                : LoadTexture(stream, extra);
        }

        // public static byte[] GetRawTexture(int index)
        // {
        //     index &= 0x3FFF;
        //
        //     var stream = _fileIndex.Seek(index, out var length, out var extra);
        //     if (stream == null)
        //         return null;
        //     var buffer = new byte[length];
        //     stream.Read(buffer, 0, length);
        //     return buffer;
        // }

        private static unsafe Bitmap LoadTexture(Stream stream, int extra)
        {
            int size = extra == 0 ? 64 : 128;

            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int max = size * size * 2;

            if (_streamBuffer == null || _streamBuffer.Length < max)
            {
                _streamBuffer = new byte[max];
            }

            stream.Read(_streamBuffer, 0, max);

            fixed (byte* data = _streamBuffer)
            {
                ushort* binData = (ushort*)data;
                for (int y = 0; y < size; ++y, line += delta)
                {
                    ushort* cur = line;
                    ushort* end = cur + size;

                    while (cur < end)
                    {
                        *cur++ = (ushort)(*binData++ ^ 0x8000);
                    }
                }
            }
            bmp.UnlockBits(bd);

            return bmp;
        }
    }
}

