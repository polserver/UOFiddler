using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima.Caching;
using Ultima.Helpers;

namespace Ultima
{
    public static class Art
    {
        private static FileIndex _fileIndex = new FileIndex(
        "Artidx.mul", "Art.mul", "artLegacyMUL.uop", 0x14000, 4, ".tga", 0x13FDC, false);
        // LRU read cache replaces the old Bitmap[0x14000]. Sized via
        // Files.CacheCapacityArt so the host can tune for low-RAM machines.
        // User edits go in _replaced (below) — they are NOT subject to eviction.
        private static LruBitmapCache _cache;
        // User-edited bitmaps. Pinned (no eviction) so a Replace+Save round
        // trip can never lose modifications regardless of LRU pressure.
        private static readonly Dictionary<int, Bitmap> _replaced = new Dictionary<int, Bitmap>();
        private static bool[] _removed;
        private static readonly Dictionary<int, bool> _patched = new Dictionary<int, bool>();
        public static bool Modified;

        private static readonly byte[] _validBuffer = new byte[4];

        private struct ImageData
        {
            public int Position;
            public int Length;
        }

        // M3.5: dedup index keyed by xxHash128 of the bitmap pixels. Replaces
        // the previous List<ImageData> + SHA256-bytes-in-each-entry layout,
        // which made CompareSaveImages an O(n²) linear scan.
        private static Dictionary<UInt128, ImageData> _landImageData;
        private static Dictionary<UInt128, ImageData> _staticImageData;

        static Art()
        {
            _cache = new LruBitmapCache(Files.CacheCapacityArt);
            _removed = new bool[0x14000];
        }

        /// <summary>
        /// Override the LRU cap for the Art read cache. Lower values bound
        /// the working set on memory-constrained machines at the cost of
        /// more re-decodes during long browsing sessions.
        /// </summary>
        public static void SetCacheCapacity(int capacity)
        {
            _cache.SetCapacity(capacity);
        }

        /// <summary>
        /// Validates if a static bitmap will fit within the MUL format limits by computing
        /// the exact encoded size. The format uses 16-bit lookup table offsets, limiting total
        /// encoded data to 65,535 ushorts. A pixel is considered opaque when its alpha bit
        /// (0x8000) is set in 16bppArgb1555 — callers that want pure-black/white treated as
        /// transparent must run the bitmap through Utils.ConvertBmp first (mirrors the save path).
        /// Per-row encoded cost: 2 ushorts header per opaque run + 1 ushort per opaque pixel + 2 end markers.
        /// </summary>
        /// <param name="bmp">The bitmap to validate</param>
        /// <param name="estimatedSize">Encoded size in ushorts (output)</param>
        /// <returns>True if the image fits, false if it exceeds limits</returns>
        public static unsafe bool ValidateStaticSize(Bitmap bmp, out int estimatedSize)
        {
            estimatedSize = 0;
            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
            {
                return true;
            }

            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);

            int total = 0;
            try
            {
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < bmp.Height; ++y, line += delta)
                {
                    ushort* cur = line;
                    int x = 0;
                    while (x < bmp.Width)
                    {
                        while (x < bmp.Width && (cur[x] & 0x8000) == 0)
                        {
                            ++x;
                        }
                        if (x >= bmp.Width)
                        {
                            break;
                        }

                        int runStart = x;
                        while (x < bmp.Width && (cur[x] & 0x8000) != 0)
                        {
                            ++x;
                        }
                        total += 2 + (x - runStart);
                    }
                    total += 2;
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }

            estimatedSize = total;

            const int maxUshorts = 65535;
            return estimatedSize <= maxUshorts;
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

        public static bool IsUOAHS()
        {
            return GetIdxLength() >= 0x13FDC;
        }

        public static ushort GetLegalItemId(int itemId, bool checkMaxId = true)
        {
            if (itemId < 0)
            {
                return 0;
            }

            if (!checkMaxId)
            {
                return (ushort)itemId;
            }

            int max = GetMaxItemId();
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
            _fileIndex?.Dispose();
            _fileIndex = new FileIndex(
                "Artidx.mul", "Art.mul", "artLegacyMUL.uop", 0x14000, 4, ".tga", 0x13FDC, false);
            _cache?.Clear();
            _cache ??= new LruBitmapCache(Files.CacheCapacityArt);
            _replaced.Clear();
            _removed = new bool[0x14000];
            _patched.Clear();
            Modified = false;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_cache"/> of Static
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        /// <exception cref="ArgumentException">Thrown when the bitmap is too large for the MUL format</exception>
        public static void ReplaceStatic(int index, Bitmap bmp)
        {
            index = GetLegalItemId(index);
            index += 0x4000;

            if (bmp != null && !ValidateStaticSize(bmp, out int estimatedSize))
            {
                throw new ArgumentException(
                    $"Image is too large for MUL format. Estimated size: {estimatedSize} ushorts (max: 65535). " +
                    $"Image dimensions: {bmp.Width}x{bmp.Height}. " +
                    "Consider using a smaller image or one with more transparent pixels.");
            }

            _replaced[index] = bmp;
            _cache.Remove(index);
            _removed[index] = false;

            _patched.Remove(index);

            Modified = true;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_replaced"/> of Land
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceLand(int index, Bitmap bmp)
        {
            index &= 0x3FFF;
            _replaced[index] = bmp;
            _cache.Remove(index);
            _removed[index] = false;

            _patched.Remove(index);

            Modified = true;
        }

        /// <summary>
        /// Removes Static index <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveStatic(int index)
        {
            index = GetLegalItemId(index);
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
            index = GetLegalItemId(index);
            index += 0x4000;

            if (_removed[index])
            {
                return false;
            }

            if (_replaced.ContainsKey(index) || _cache.TryGet(index, out _))
            {
                return true;
            }

            Stream stream = _fileIndex.Seek(index, out int _, out int _, out bool _);

            if (stream == null)
            {
                return false;
            }

            stream.Seek(4, SeekOrigin.Current);
            stream.ReadExactly(_validBuffer, 0, 4);

            short width = (short)(_validBuffer[0] | (_validBuffer[1] << 8));
            short height = (short)(_validBuffer[2] | (_validBuffer[3] << 8));

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

            if (_replaced.ContainsKey(index) || _cache.TryGet(index, out _))
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

            if (_replaced.TryGetValue(index, out Bitmap replaced))
            {
                return replaced;
            }

            if (_cache.TryGet(index, out Bitmap cached))
            {
                return cached;
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

            Bitmap bmp = LoadLand(stream, length);
            if (Files.CacheData && bmp != null)
            {
                _cache.Set(index, bmp);
            }
            return bmp;
        }

        // ReSharper disable once UnusedMember.Global
        public static byte[] GetRawLand(int index)
        {
            index &= 0x3FFF;

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            var buffer = new byte[length];
            stream.ReadExactly(buffer, 0, length);
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
            index = GetLegalItemId(index, checkMaxId);
            index += 0x4000;

            patched = _patched.ContainsKey(index) && _patched[index];

            if (_removed[index])
            {
                return null;
            }

            if (_replaced.TryGetValue(index, out Bitmap replaced))
            {
                return replaced;
            }

            if (_cache.TryGet(index, out Bitmap cached))
            {
                return cached;
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

            Bitmap bmp = LoadStatic(stream, length);
            if (Files.CacheData && bmp != null)
            {
                _cache.Set(index, bmp);
            }
            return bmp;
        }

        // ReSharper disable once UnusedMember.Global
        public static byte[] GetRawStatic(int index)
        {
            index = GetLegalItemId(index);
            index += 0x4000;

            Stream stream = _fileIndex.Seek(index, out int length, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            var buffer = new byte[length];
            stream.ReadExactly(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// Decodes a static into a caller-supplied pixel buffer (16bppArgb1555).
        /// Lets the caller reuse one buffer across many decodes instead of
        /// paying the per-call `new Bitmap(...)` + GDI handle + LockBits cost
        /// that `GetStatic` does.
        ///
        /// `destination` must be at least <paramref name="width"/> *
        /// <paramref name="height"/> ushorts. Dimensions are populated in
        /// out parameters even when the buffer is too small, so callers can
        /// resize and retry.
        ///
        /// Cache semantics: does not touch _cache. Every call decodes from
        /// disk. Pair with TryGetStaticDimensions if you need to size the
        /// buffer first.
        /// </summary>
        public static unsafe bool TryGetStaticPixels(int index, Span<ushort> destination, out int width, out int height, out bool patched, bool checkMaxId = true)
        {
            width = 0;
            height = 0;
            patched = false;

            index = GetLegalItemId(index, checkMaxId);
            index += 0x4000;

            if (_removed[index])
            {
                return false;
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _, out patched);
            if (stream == null)
            {
                return false;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                stream.ReadExactly(buffer, 0, length);

                fixed (byte* data = buffer)
                {
                    var binData = (ushort*)data;
                    int count = 2;
                    width = binData[count++];
                    height = binData[count++];

                    if (width <= 0 || height <= 0)
                    {
                        return false;
                    }

                    if (destination.Length < width * height)
                    {
                        return false;
                    }

                    var lookups = new int[height];
                    int start = height + 4;
                    for (int i = 0; i < height; ++i)
                    {
                        lookups[i] = start + binData[count++];
                    }

                    fixed (ushort* destPtr = destination)
                    {
                        for (int y = 0; y < height; ++y)
                        {
                            count = lookups[y];

                            ushort* cur = destPtr + y * width;
                            ushort* lineEnd = cur + width;
                            int xOffset, xRun;

                            while ((xOffset = binData[count++]) + (xRun = binData[count++]) != 0)
                            {
                                if (xOffset > width)
                                {
                                    break;
                                }

                                cur += xOffset;
                                if (xOffset + xRun > width)
                                {
                                    break;
                                }

                                ushort* end = cur + xRun;
                                while (cur < end && cur < lineEnd)
                                {
                                    *cur++ = (ushort)(binData[count++] ^ 0x8000);
                                }
                            }
                        }
                    }
                }

                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Decodes a land tile into a caller-supplied 44x44 ushort buffer
        /// (16bppArgb1555). All land tiles are 44x44 so dimensions are fixed.
        /// `destination` must be at least 44*44 = 1936 ushorts.
        /// </summary>
        public static unsafe bool TryGetLandPixels(int index, Span<ushort> destination, out bool patched)
        {
            patched = false;
            index &= 0x3FFF;

            if (_removed[index])
            {
                return false;
            }

            if (destination.Length < 44 * 44)
            {
                return false;
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _, out patched);
            if (stream == null)
            {
                return false;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                stream.ReadExactly(buffer, 0, length);

                destination.Slice(0, 44 * 44).Clear();

                fixed (byte* binData = buffer)
                fixed (ushort* destPtr = destination)
                {
                    var bdata = (ushort*)binData;
                    int xOffset = 21;
                    int xRun = 2;
                    ushort* line = destPtr;

                    for (int y = 0; y < 22; ++y, --xOffset, xRun += 2, line += 44)
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
                    for (int y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += 44)
                    {
                        ushort* cur = line + xOffset;
                        ushort* end = cur + xRun;
                        while (cur < end)
                        {
                            *cur++ = (ushort)(*bdata++ | 0x8000);
                        }
                    }
                }

                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        /// <summary>
        /// Returns the dimensions of a static without decoding pixel data.
        /// Land tiles are always 44x44 so no dimension query is needed for them.
        /// </summary>
        public static bool TryGetStaticDimensions(int index, out int width, out int height, bool checkMaxId = true)
        {
            width = 0;
            height = 0;
            index = GetLegalItemId(index, checkMaxId);
            index += 0x4000;

            if (_removed[index])
            {
                return false;
            }

            Stream stream = _fileIndex.Seek(index, out int length, out _, out _);
            if (stream == null || length < 8)
            {
                return false;
            }

            // Header layout: 4 unknown ushorts then width, height as ushorts at offset 4 and 6.
            Span<byte> header = stackalloc byte[8];
            stream.ReadExactly(header);
            width = header[4] | (header[5] << 8);
            height = header[6] | (header[7] << 8);
            return width > 0 && height > 0;
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
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                stream.ReadExactly(buffer, 0, length);

                fixed (byte* data = buffer)
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

                    Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                    BitmapData bd = bmp.LockBits(
                        new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                    try
                    {
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
                    }
                    finally
                    {
                        bmp.UnlockBits(bd);
                    }

                    return bmp;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static unsafe Bitmap LoadLand(Stream stream, int length)
        {
            var bmp = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                stream.ReadExactly(buffer, 0, length);
                fixed (byte* binData = buffer)
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
            }
            finally
            {
                bmp.UnlockBits(bd);
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return bmp;
        }

        /// <summary>
        /// Saves mul
        /// </summary>
        /// <param name="path"></param>
        public static unsafe void Save(string path)
        {
            _landImageData = new Dictionary<UInt128, ImageData>();
            _staticImageData = new Dictionary<UInt128, ImageData>();

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
                        // GetLand / GetStatic transparently check _replaced
                        // first, then the LRU cache, then decode from disk.
                        Bitmap bmp = index < 0x4000
                            ? GetLand(index)
                            : GetStatic(index - 0x4000, false);
                        if (bmp == null || _removed[index])
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0);  // Length
                            binidx.Write(-1); // extra
                        }
                        else if (index < 0x4000)
                        {
                            // Lock once: used for both hashing (dedup) and encoding.
                            BitmapData bd = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                                PixelFormat.Format16bppArgb1555);
                            try
                            {
                                UInt128 hash = bd.Hash128();
                                if (_landImageData.TryGetValue(hash, out ImageData existing))
                                {
                                    binidx.Write(existing.Position); // lookup
                                    binidx.Write(existing.Length);
                                    binidx.Write(0);
                                    continue;
                                }

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

                                _landImageData[hash] = new ImageData
                                {
                                    Position = start,
                                    Length = length,
                                };
                            }
                            finally
                            {
                                bmp.UnlockBits(bd);
                            }
                        }
                        else
                        {
                            // Validate static art size before encoding
                            if (!ValidateStaticSize(bmp, out int estimatedSize))
                            {
                                // Skip this image and write empty entry
                                binidx.Write(-1); // lookup
                                binidx.Write(0);  // Length
                                binidx.Write(-1); // extra
                                System.Diagnostics.Debug.WriteLine(
                                    $"Warning: Skipping static art at index {index - 0x4000} - " +
                                    $"image too large ({bmp.Width}x{bmp.Height}, estimated {estimatedSize} ushorts, max 65535)");
                                continue;
                            }

                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            try
                            {
                                UInt128 hash = bd.Hash128();
                                if (_staticImageData.TryGetValue(hash, out ImageData existing))
                                {
                                    binidx.Write(existing.Position); // lookup
                                    binidx.Write(existing.Length);
                                    binidx.Write(0);
                                    continue;
                                }

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

                                            if ((cur[i] & 0x8000) != 0)
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
                                            if ((cur[j] & 0x8000) == 0)
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

                                _staticImageData[hash] = new ImageData
                                {
                                    Position = start,
                                    Length = length,
                                };
                            }
                            finally
                            {
                                bmp.UnlockBits(bd);
                            }
                        }
                    }

                    memidx.WriteTo(fsidx);
                    memmul.WriteTo(fsmul);
                }
            }
        }

    }
}