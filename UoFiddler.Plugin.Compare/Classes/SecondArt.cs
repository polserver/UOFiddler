using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondArt
    {
        private static SecondFileIndex _fileIndex;
        private static Bitmap[] _cache;

        private static byte[] _streamBuffer;
        private static byte[] _validBuffer;

        internal static event Action FileIndexChanged;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            SetFileIndex(idxPath, mulPath, null);
        }

        public static void SetFileIndex(string idxPath, string mulPath, string uopPath)
        {
            // Build first: a bad UOP throws out of the ctor and leaves the previous index usable.
            var newIndex = new SecondFileIndex(idxPath, mulPath, uopPath, 0x14000, ".tga", 0x13FDC, false);

            SecondFileIndex oldIndex = _fileIndex;
            Bitmap[] oldCache = _cache;

            _fileIndex = newIndex;
            _cache = new Bitmap[0x14000];
            _streamBuffer = null;

            // Order matters: the cache hands out its own Bitmap instances, and three tabs park them in
            // PictureBox.BackgroundImage. Let the subscribers drop those references before we dispose.
            FileIndexChanged?.Invoke();

            oldIndex?.Dispose();
            DisposeCache(oldCache);
        }

        private static void DisposeCache(Bitmap[] cache)
        {
            if (cache == null)
            {
                return;
            }

            for (int i = 0; i < cache.Length; ++i)
            {
                cache[i]?.Dispose();
                cache[i] = null;
            }
        }

        public static int GetMaxItemId()
        {
            // High Seas
            if (GetIdxLength() >= 0x13FDC)
            {
                return 0xFFDC;
            }

            // Stygian Abyss
            if (GetIdxLength() == 0xC000)
            {
                return 0x7FFF;
            }

            // ML and older
            return 0x3FFF;
        }

        private static ushort GetLegalItemId(int itemId)
        {
            if (itemId < 0)
            {
                return 0;
            }

            int max = GetMaxItemId();
            if (itemId > max)
            {
                return 0;
            }

            return (ushort)itemId;
        }

        private static int GetIdxLength()
        {
            // Reached through the public GetMaxItemId/IsUOAHS, which callers may hit before a load.
            return _fileIndex == null ? 0 : (int)(_fileIndex.IdxLength / 12);
        }

        public static bool IsUOAHS()
        {
            return GetIdxLength() >= 0x13FDC;
        }

        public static bool IsValidStatic(int index)
        {
            if (_fileIndex == null || _cache == null)
            {
                return false;
            }

            index = GetLegalItemId(index);
            index += 0x4000;

            if (_cache[index] != null)
            {
                return true;
            }

            Stream stream = _fileIndex.Seek(index, out _, out _);

            if (stream == null)
            {
                return false;
            }

            if (_validBuffer == null)
            {
                _validBuffer = new byte[4];
            }

            stream.Seek(4, SeekOrigin.Current);
            stream.ReadExactly(_validBuffer, 0, 4);

            short width = (short)(_validBuffer[0] | (_validBuffer[1] << 8));
            short height = (short)(_validBuffer[2] | (_validBuffer[3] << 8));

            return width > 0 && height > 0;
        }

        public static Bitmap GetStatic(int index)
        {
            if (_fileIndex == null || _cache == null)
            {
                return null;
            }

            index = GetLegalItemId(index);
            index += 0x4000;

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _);
            if (stream == null)
            {
                return null;
            }

            if (Files.CacheData)
            {
                return _cache[index] = LoadStatic(stream, length);
            }
            else
            {
                return LoadStatic(stream, length);
            }
        }

        // TODO: unused method?
        // public static byte[] GetRawStatic(int index)
        // {
        //     index = GetLegalItemId(index);
        //     index += 0x4000;
        //
        //     var stream = _fileIndex.Seek(index, out var length, out _);
        //     if (stream == null)
        //     {
        //         return null;
        //     }
        //
        //     var buffer = new byte[length];
        //     stream.Read(buffer, 0, length);
        //     return buffer;
        // }

        private static unsafe Bitmap LoadStatic(Stream stream, int length)
        {
            Bitmap bmp;
            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            // Do not close the stream: it is the index's cached handle, shared by every entry, and
            // SecondFileIndex.EnsureOpen would have to re-open the file for the next tile.
            stream.ReadExactly(_streamBuffer, 0, length);

            fixed (byte* data = _streamBuffer)
            {
                ushort* binData = (ushort*)data;
                int count = 2;
                // bin.ReadInt32(); // TODO: ???
                int width = binData[count++];
                int height = binData[count++];

                if (width <= 0 || height <= 0)
                {
                    return null;
                }

                int[] lookups = new int[height];

                int start = height + 4;

                for (int i = 0; i < height; ++i)
                {
                    lookups[i] = start + binData[count++];
                }

                bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < height; ++y, line += delta)
                {
                    count = lookups[y];

                    ushort* cur = line;
                    int xOffset, xRun;

                    while ((xOffset = binData[count++]) + (xRun = binData[count++]) != 0)
                    {
                        if (xOffset > delta)
                        {
                            break;
                        }

                        cur += xOffset;
                        if (xOffset + xRun > delta)
                        {
                            break;
                        }

                        ushort* end = cur + xRun;

                        while (cur < end)
                        {
                            *cur++ = (ushort)(binData[count++] ^ 0x8000);
                        }
                    }
                }
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        public static bool IsValidLand(int index)
        {
            if (_fileIndex == null || _cache == null)
            {
                return false;
            }

            index &= 0x3FFF;
            return _cache[index] != null || _fileIndex.Valid(index, out _, out _);
        }

        public static Bitmap GetLand(int index)
        {
            if (_fileIndex == null || _cache == null)
            {
                return null;
            }

            index &= 0x3FFF;

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _);
            if (stream == null)
            {
                return null;
            }

            return Files.CacheData
                ? _cache[index] = LoadLand(stream, length)
                : LoadLand(stream, length);
        }

        // TODO: unused method?
        // public static byte[] GetRawLand(int index)
        // {
        //     index &= 0x3FFF;
        //
        //     var stream = _fileIndex.Seek(index, out var length, out _);
        //     if (stream == null)
        //     {
        //         return null;
        //     }
        //
        //     var buffer = new byte[length];
        //     stream.Read(buffer, 0, length);
        //     return buffer;
        // }

        private static unsafe Bitmap LoadLand(Stream stream, int length)
        {
            Bitmap bmp = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            // See LoadStatic: the stream belongs to the index and stays open.
            stream.ReadExactly(_streamBuffer, 0, length);
            fixed (byte* binData = _streamBuffer)
            {
                ushort* bdata = (ushort*)binData;
                int xOffset = 21;
                int xRun = 2;

                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < 22; ++y, --xOffset, xRun += 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                    {
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                    }
                }

                xOffset = 0;
                xRun = 44;

                for (int y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                    {
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                    }
                }
            }
            bmp.UnlockBits(bd);
            return bmp;
        }
    }
}
