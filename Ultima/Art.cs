using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima.Helpers;

namespace Ultima
{
    public static class Art
    {
        private static FileIndex _fileIndex = new FileIndex(
        "Artidx.mul", "Art.mul", "artLegacyMUL.uop", 0x14000, 4, ".tga", 0x13FDC, false);
        private static Bitmap[] _cache;
        private static bool[] _removed;
        private static readonly Dictionary<int, bool> _patched = new Dictionary<int, bool>();
        public static bool Modified;

        private static byte[] _streamBuffer;
        private static readonly byte[] _validBuffer = new byte[4];

        private struct CheckSums
        {
            public byte[] checksum;
            public int pos;
            public int length;
            // public int index;
        }

        private static List<CheckSums> _checksumsLand;
        private static List<CheckSums> _checksumsStatic;

        static Art()
        {
            _cache = new Bitmap[0x14000];
            _removed = new bool[0x14000];
        }

        public static int GetMaxItemID()
        {
            if (GetIdxLength() >= 0x13FDC)
            {
                return 0xFFFF;
            }

            if (GetIdxLength() == 0xC000)
            {
                return 0x7FFF;
            }

            return 0x3FFF;
        }

        public static bool IsUOAHS()
        {
            return GetIdxLength() >= 0x13FDC;
        }

        public static ushort GetLegalItemID(int itemId, bool checkMaxId = true)
        {
            if (itemId < 0)
            {
                return 0;
            }

            if (!checkMaxId)
            {
                return (ushort)itemId;
            }

            int max = GetMaxItemID();
            if (itemId > max)
            {
                return 0;
            }

            return (ushort)itemId;
        }

        public static int GetIdxLength()
        {
            return (int)(_fileIndex.IdxLength / 12);
        }

        /// <summary>
        /// ReReads Art.mul
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex(
                "Artidx.mul", "Art.mul", "artLegacyMUL.uop", 0x14000, 4, ".tga", 0x13FDC, false);
            _cache = new Bitmap[0x14000];
            _removed = new bool[0x14000];
            _patched.Clear();
            Modified = false;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_cache"/> of Static
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceStatic(int index, Bitmap bmp)
        {
            index = GetLegalItemID(index);
            index += 0x4000;

            _cache[index] = bmp;
            _removed[index] = false;

            if (_patched.ContainsKey(index))
            {
                _patched.Remove(index);
            }

            Modified = true;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_cache"/> of Land
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceLand(int index, Bitmap bmp)
        {
            index &= 0x3FFF;
            _cache[index] = bmp;
            _removed[index] = false;

            if (_patched.ContainsKey(index))
            {
                _patched.Remove(index);
            }

            Modified = true;
        }

        /// <summary>
        /// Removes Static index <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveStatic(int index)
        {
            index = GetLegalItemID(index);
            index += 0x4000;

            _removed[index] = true;
            Modified = true;
        }

        /// <summary>
        /// Removes Land index <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveLand(int index)
        {
            index &= 0x3FFF;
            _removed[index] = true;
            Modified = true;
        }

        /// <summary>
        /// Tests if Static is defined (width and height check)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidStatic(int index)
        {
            index = GetLegalItemID(index);
            index += 0x4000;

            if (_removed[index])
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            Stream stream = _fileIndex.Seek(index, out int _, out int _, out bool _);

            if (stream == null)
            {
                return false;
            }

            stream.Seek(4, SeekOrigin.Current);
            stream.Read(_validBuffer, 0, 4);

            ref var width = ref _validBuffer[0];
            ref var height = ref _validBuffer[2];

            return width > 0 && height > 0;
        }

        /// <summary>
        /// Tests if LandTile is defined
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidLand(int index)
        {
            index &= 0x3FFF;
            if (_removed[index])
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            return _fileIndex.Valid(index, out int _, out int _, out bool _);
        }

        /// <summary>
        /// Returns Bitmap of LandTile (with Cache)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Bitmap GetLand(int index)
        {
            return GetLand(index, out bool _);
        }

        /// <summary>
        /// Returns Bitmap of LandTile (with Cache) and verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public static Bitmap GetLand(int index, out bool patched)
        {
            index &= 0x3FFF;
            patched = _patched.ContainsKey(index) && _patched[index];

            if (_removed[index])
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out patched);
            if (stream == null)
            {
                return null;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            if (Files.CacheData)
            {
                return _cache[index] = LoadLand(stream, length);
            }

            return LoadLand(stream, length);
        }

        public static byte[] GetRawLand(int index)
        {
            index &= 0x3FFF;

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }

        /// <summary>
        /// Returns Bitmap of Static (with Cache)
        /// </summary>
        /// <param name="index"></param>
        /// <param name="checkMaxId"></param>
        /// <returns></returns>
        public static Bitmap GetStatic(int index, bool checkMaxId = true)
        {
            return GetStatic(index, out bool _, checkMaxId);
        }

        /// <summary>
        /// Returns Bitmap of Static (with Cache) and verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <param name="checkMaxId"></param>
        /// <returns></returns>
        public static Bitmap GetStatic(int index, out bool patched, bool checkMaxId = true)
        {
            index = GetLegalItemID(index, checkMaxId);
            index += 0x4000;

            patched = _patched.ContainsKey(index) && _patched[index];

            if (_removed[index])
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out patched);
            if (stream == null)
            {
                return null;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            if (Files.CacheData)
            {
                return _cache[index] = LoadStatic(stream, length);
            }

            return LoadStatic(stream, length);
        }

        public static byte[] GetRawStatic(int index)
        {
            index = GetLegalItemID(index);
            index += 0x4000;

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }

        public static unsafe void Measure(Bitmap bmp, out int xMin, out int yMin, out int xMax, out int yMax)
        {
            xMin = yMin = 0;
            xMax = yMax = -1;

            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
            {
                return;
            }

            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);

            int delta = (bd.Stride >> 1) - bd.Width;
            int lineDelta = bd.Stride >> 1;

            var pBuffer = (ushort*)bd.Scan0;
            ushort* pLineEnd = pBuffer + bd.Width;
            ushort* pEnd = pBuffer + (bd.Height * lineDelta);

            bool foundPixel = false;

            int x = 0, y = 0;

            while (pBuffer < pEnd)
            {
                while (pBuffer < pLineEnd)
                {
                    ushort c = *pBuffer++;

                    if ((c & 0x8000) != 0)
                    {
                        if (!foundPixel)
                        {
                            foundPixel = true;
                            xMin = xMax = x;
                            yMin = yMax = y;
                        }
                        else
                        {
                            if (x < xMin)
                            {
                                xMin = x;
                            }

                            if (y < yMin)
                            {
                                yMin = y;
                            }

                            if (x > xMax)
                            {
                                xMax = x;
                            }

                            if (y > yMax)
                            {
                                yMax = y;
                            }
                        }
                    }
                    ++x;
                }

                pBuffer += delta;
                pLineEnd += lineDelta;
                ++y;
                x = 0;
            }

            bmp.UnlockBits(bd);
        }

        private static unsafe Bitmap LoadStatic(Stream stream, int length)
        {
            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            stream.Read(_streamBuffer, 0, length);
            stream.Close();

            Bitmap bmp;
            fixed (byte* data = _streamBuffer)
            {
                var binData = (ushort*)data;
                int count = 2;
                int width = binData[count++];
                int height = binData[count++];

                if (width <= 0 || height <= 0)
                {
                    return null;
                }

                var lookups = new int[height];

                int start = height + 4;

                for (int i = 0; i < height; ++i)
                {
                    lookups[i] = start + binData[count++];
                }

                bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(
                    new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                var line = (ushort*)bd.Scan0;
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

        private static unsafe Bitmap LoadLand(Stream stream, int length)
        {
            var bmp = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            stream.Read(_streamBuffer, 0, length);
            stream.Close();
            fixed (byte* binData = _streamBuffer)
            {
                var bdata = (ushort*)binData;
                int xOffset = 21;
                int xRun = 2;

                var line = (ushort*)bd.Scan0;
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

        /// <summary>
        /// Saves mul
        /// </summary>
        /// <param name="path"></param>
        public static unsafe void Save(string path)
        {
            _checksumsLand = new List<CheckSums>();
            _checksumsStatic = new List<CheckSums>();

            string idx = Path.Combine(path, "artidx.mul");
            string mul = Path.Combine(path, "art.mul");

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var memidx = new MemoryStream();
                var memmul = new MemoryStream();

                using (var binidx = new BinaryWriter(memidx))
                using (var binmul = new BinaryWriter(memmul))
                {
                    for (int index = 0; index < GetIdxLength(); index++)
                    {
                        Files.FireFileSaveEvent();
                        if (_cache[index] == null)
                        {
                            if (index < 0x4000)
                            {
                                _cache[index] = GetLand(index);
                            }
                            else
                            {
                                _cache[index] = GetStatic(index - 0x4000, false);
                            }
                        }

                        Bitmap bmp = _cache[index];
                        if (bmp == null || _removed[index])
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0);  // length
                            binidx.Write(-1); // extra
                        }
                        else if (index < 0x4000)
                        {
                            byte[] checksum = bmp.ToArray(PixelFormat.Format16bppArgb1555).ToSha256();
                            if (CompareSaveImagesLand(checksum, out CheckSums sum))
                            {
                                binidx.Write(sum.pos); // lookup
                                binidx.Write(sum.length);
                                binidx.Write(0);

                                continue;
                            }

                            // land
                            BitmapData bd = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                                PixelFormat.Format16bppArgb1555);
                            var line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;
                            binidx.Write((int)binmul.BaseStream.Position); // lookup
                            var length = (int)binmul.BaseStream.Position;
                            int x = 22;
                            int y = 0; // TODO: y is never used?
                            int lineWidth = 2;
                            for (int m = 0; m < 22; ++m, ++y, line += delta, lineWidth += 2)
                            {
                                --x;
                                ushort* cur = line;
                                for (int n = 0; n < lineWidth; ++n)
                                {
                                    binmul.Write((ushort)(cur[x + n] ^ 0x8000));
                                }
                            }

                            x = 0;
                            lineWidth = 44;
                            y = 22;
                            line = (ushort*)bd.Scan0;
                            line += delta * 22;
                            for (int m = 0; m < 22; m++, y++, line += delta, ++x, lineWidth -= 2)
                            {
                                ushort* cur = line;
                                for (int n = 0; n < lineWidth; n++)
                                {
                                    binmul.Write((ushort)(cur[x + n] ^ 0x8000));
                                }
                            }

                            int start = length;
                            length = (int)binmul.BaseStream.Position - length;
                            binidx.Write(length);
                            binidx.Write(0);
                            bmp.UnlockBits(bd);

                            _checksumsLand.Add(new CheckSums { pos = start, length = length, checksum = checksum });
                        }
                        else
                        {
                            byte[] checksum = bmp.ToArray(PixelFormat.Format16bppArgb1555).ToSha256();
                            if (CompareSaveImagesStatic(checksum, out CheckSums sum))
                            {
                                binidx.Write(sum.pos); // lookup
                                binidx.Write(sum.length);
                                binidx.Write(0);

                                continue;
                            }

                            // art
                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            var line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;
                            binidx.Write((int)binmul.BaseStream.Position); // lookup
                            var length = (int)binmul.BaseStream.Position;
                            binmul.Write(1234); // header //TODO: check what to write to header? Maybe different value will be better?
                            binmul.Write((short)bmp.Width);
                            binmul.Write((short)bmp.Height);
                            var lookup = (int)binmul.BaseStream.Position;
                            int streamLoc = lookup + (bmp.Height * 2);
                            int width = 0;
                            for (int i = 0; i < bmp.Height; ++i) // fill lookup
                            {
                                binmul.Write(width);
                            }

                            for (int y = 0; y < bmp.Height; ++y, line += delta)
                            {
                                ushort* cur = line;
                                width = (int)(binmul.BaseStream.Position - streamLoc) / 2;
                                binmul.BaseStream.Seek(lookup + (y * 2), SeekOrigin.Begin);
                                binmul.Write(width);
                                binmul.BaseStream.Seek(streamLoc + (width * 2), SeekOrigin.Begin);
                                int i = 0;
                                int x = 0;
                                while (i < bmp.Width)
                                {
                                    for (i = x; i <= bmp.Width; ++i)
                                    {
                                        // first pixel set
                                        if (i >= bmp.Width)
                                        {
                                            continue;
                                        }

                                        if (cur[i] != 0)
                                        {
                                            break;
                                        }
                                    }

                                    if (i >= bmp.Width)
                                    {
                                        continue;
                                    }

                                    int j;
                                    for (j = i + 1; j < bmp.Width; ++j)
                                    {
                                        // next non set pixel
                                        if (cur[j] == 0)
                                        {
                                            break;
                                        }
                                    }

                                    binmul.Write((short)(i - x)); // xOffset
                                    binmul.Write((short)(j - i)); // run

                                    for (int p = i; p < j; ++p)
                                    {
                                        binmul.Write((ushort)(cur[p] ^ 0x8000));
                                    }

                                    x = j;
                                }

                                binmul.Write((short)0); // xOffset
                                binmul.Write((short)0); // Run
                            }

                            int start = length;
                            length = (int)binmul.BaseStream.Position - length;
                            binidx.Write(length);
                            binidx.Write(0);
                            bmp.UnlockBits(bd);

                            _checksumsStatic.Add(new CheckSums {pos = start, length = length, checksum = checksum});
                        }
                    }

                    memidx.WriteTo(fsidx);
                    memmul.WriteTo(fsmul);
                }
            }
        }

        private static bool CompareSaveImagesLand(IReadOnlyList<byte> newChecksum, out CheckSums sum)
        {
            sum = new CheckSums();
            for (int i = 0; i < _checksumsLand.Count; ++i)
            {
                byte[] cmp = _checksumsLand[i].checksum;
                if (cmp == null || newChecksum == null || cmp.Length != newChecksum.Count)
                {
                    return false;
                }

                bool valid = true;

                for (int j = 0; j < cmp.Length; ++j)
                {
                    if (cmp[j] == newChecksum[j])
                    {
                        continue;
                    }

                    valid = false;
                    break;
                }

                if (!valid)
                {
                    continue;
                }

                sum = _checksumsLand[i];

                return true;
            }

            return false;
        }

        private static bool CompareSaveImagesStatic(byte[] newChecksum, out CheckSums sum)
        {
            sum = new CheckSums();
            for (int i = 0; i < _checksumsStatic.Count; ++i)
            {
                byte[] cmp = _checksumsStatic[i].checksum;
                if (cmp == null || newChecksum == null || cmp.Length != newChecksum.Length)
                {
                    return false;
                }

                bool valid = true;

                for (int j = 0; j < cmp.Length; ++j)
                {
                    if (cmp[j] == newChecksum[j])
                    {
                        continue;
                    }

                    valid = false;
                    break;
                }

                if (!valid)
                {
                    continue;
                }

                sum = _checksumsStatic[i];

                return true;
            }

            return false;
        }
    }
}