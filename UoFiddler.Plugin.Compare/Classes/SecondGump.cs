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
using System.IO;
using Ultima;
using Ultima.Helpers;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondGump
    {
        private static SecondFileIndex _fileIndex;

        private static Bitmap[] _cache = new Bitmap[0x10000];
        private static byte[] _streamBuffer;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            SetFileIndex(idxPath, mulPath, null);
        }

        public static void SetFileIndex(string idxPath, string mulPath, string uopPath)
        {
            _fileIndex = new SecondFileIndex(idxPath, mulPath, uopPath, 0xFFFF, ".tga", -1, true);
            _cache = new Bitmap[0x10000];
        }

        public static bool IsValidIndex(int index)
        {
            if (_fileIndex == null)
            {
                return false;
            }

            if (index < 0 || index >= _cache.Length)
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            SecondIEntry entry = null;
            if (_fileIndex.Seek(index, ref entry) == null || entry == null)
            {
                return false;
            }

            // Compressed UOP entries don't carry width/height in the idx — the
            // dimensions live inside the decompressed payload. Defer the check.
            if (entry.Flag >= SecondCompressionFlag.Zlib)
            {
                return entry.Length > 0;
            }

            if (entry.Extra == -1)
            {
                return false;
            }

            int width = entry.Extra1;
            int height = entry.Extra2;
            return width > 0 && height > 0;
        }

        public static byte[] GetRawGump(int index, out int width, out int height)
        {
            width = -1;
            height = -1;

            if (_fileIndex == null)
            {
                return null;
            }

            SecondIEntry entry = null;
            Stream stream = _fileIndex.Seek(index, ref entry);
            if (stream == null || entry == null)
            {
                return null;
            }

            int payloadLength = ReadEntryPayload(stream, entry, out width, out height);
            if (payloadLength <= 0 || width <= 0 || height <= 0)
            {
                return null;
            }

            // Hand back an exact-sized copy so callers can hash/compare safely.
            byte[] result = new byte[payloadLength];
            System.Buffer.BlockCopy(_streamBuffer, 0, result, 0, payloadLength);
            return result;
        }

        public static unsafe Bitmap GetGump(int index)
        {
            if (_fileIndex == null)
            {
                return null;
            }

            if (index < 0 || index >= _cache.Length)
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            SecondIEntry entry = null;
            Stream stream = _fileIndex.Seek(index, ref entry);
            if (stream == null || entry == null)
            {
                return null;
            }

            int payloadLength = ReadEntryPayload(stream, entry, out int width, out int height);
            if (payloadLength <= 0 || width <= 0 || height <= 0)
            {
                return null;
            }

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            fixed (byte* pData = _streamBuffer)
            {
                int* lookup = (int*)pData;
                ushort* dat = (ushort*)pData;

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

            return Files.CacheData ? _cache[index] = bmp : bmp;
        }

        /// Reads the pixel-RLE payload for a gump entry into <see cref="_streamBuffer"/>,
        /// transparently handling uncompressed MUL/UOP and zlib/Mythic-compressed UOP layouts.
        /// Returns the number of valid bytes at the start of <see cref="_streamBuffer"/>.
        private static int ReadEntryPayload(Stream stream, SecondIEntry entry, out int width, out int height)
        {
            int length = entry.Length;
            if (length <= 0)
            {
                width = height = -1;
                return 0;
            }

            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            stream.ReadExactly(_streamBuffer, 0, length);

            if (entry.Flag >= SecondCompressionFlag.Zlib)
            {
                byte[] compressed = new byte[length];
                System.Buffer.BlockCopy(_streamBuffer, 0, compressed, 0, length);

                var result = UopUtils.Decompress(compressed);
                if (!result.success)
                {
                    width = height = -1;
                    return 0;
                }

                byte[] decompressed = entry.Flag == SecondCompressionFlag.Mythic
                    ? MythicDecompress.Decompress(result.data)
                    : result.data;

                if (decompressed == null || decompressed.Length < 8)
                {
                    width = height = -1;
                    return 0;
                }

                width = (decompressed[3] << 24) | (decompressed[2] << 16) | (decompressed[1] << 8) | decompressed[0];
                height = (decompressed[7] << 24) | (decompressed[6] << 16) | (decompressed[5] << 8) | decompressed[4];
                entry.Extra1 = width;
                entry.Extra2 = height;

                int rleLen = decompressed.Length - 8;
                if (_streamBuffer.Length < rleLen)
                {
                    _streamBuffer = new byte[rleLen];
                }
                System.Buffer.BlockCopy(decompressed, 8, _streamBuffer, 0, rleLen);
                return rleLen;
            }

            width = entry.Extra1;
            height = entry.Extra2;
            return length;
        }
    }
}
