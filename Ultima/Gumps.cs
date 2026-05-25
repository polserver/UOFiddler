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
    public sealed class Gumps
    {
        private static FileIndex _fileIndex = new FileIndex(
            "Gumpidx.mul", "Gumpart.mul", "gumpartLegacyMUL.uop", 0xFFFF, 12, ".tga", -1, true);

        // LRU read cache replaces the old Bitmap[_fileIndex.IndexLength].
        // User edits go in _replaced (below) and are never evicted.
        private static LruBitmapCache _cache;
        private static readonly Dictionary<int, Bitmap> _replaced = new Dictionary<int, Bitmap>();
        private static bool[] _removed;
        private static readonly Dictionary<int, bool> _patched = new Dictionary<int, bool>();

        private static byte[] _pixelBuffer;
        private static byte[] _streamBuffer;
        private static byte[] _colorTable;

        // Authoritative id range — what _cache.Length used to be before the
        // LRU swap. Sourced from the FileIndex when available, falls back to
        // 0xFFFF (the gump id space ceiling) when no client is configured.
        private static int _indexLength;

        static Gumps()
        {
            _cache = new LruBitmapCache(Files.CacheCapacityGumps);
            _indexLength = _fileIndex?.IndexLength > 0 ? (int)_fileIndex.IndexLength : 0xFFFF;
            _removed = new bool[_indexLength];
        }

        /// <summary>
        /// Override the LRU cap for the Gumps read cache. See
        /// <see cref="Files.CacheCapacityGumps"/> for the default.
        /// </summary>
        public static void SetCacheCapacity(int capacity)
        {
            _cache.SetCapacity(capacity);
        }

        /// <summary>
        /// ReReads gumpart
        /// </summary>
        public static void Reload()
        {
            try
            {
                _fileIndex?.Dispose();
                _fileIndex = new FileIndex("Gumpidx.mul", "Gumpart.mul", "gumpartLegacyMUL.uop", 0xFFFF, 12, ".tga", -1, true);
                _indexLength = _fileIndex.IndexLength > 0 ? (int)_fileIndex.IndexLength : 0xFFFF;
                _cache?.Clear();
                _cache ??= new LruBitmapCache(Files.CacheCapacityGumps);
                _replaced.Clear();
                _removed = new bool[_indexLength];
            }
            catch
            {
                _fileIndex = null;
                _indexLength = 0xFFFF;
                _cache?.Clear();
                _cache ??= new LruBitmapCache(Files.CacheCapacityGumps);
                _replaced.Clear();
                _removed = new bool[_indexLength];
            }

            //_pixelBuffer = null;
            _streamBuffer = null;
            //_colorTable = null;
            _patched.Clear();
        }

        public static int GetCount()
        {
            return _indexLength;
        }

        /// <summary>
        /// Replaces Gump <see cref="_replaced"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceGump(int index, Bitmap bmp)
        {
            _replaced[index] = bmp;
            _cache.Remove(index);
            _removed[index] = false;
            _patched.Remove(index);
        }

        /// <summary>
        /// Removes Gumpindex <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveGump(int index)
        {
            _removed[index] = true;
        }

        /// <summary>
        /// Tests if index is defined
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidIndex(int index)
        {
            if (_fileIndex == null)
            {
                return false;
            }

            if (index > _indexLength - 1)
            {
                return false;
            }

            if (_removed[index])
            {
                return false;
            }

            if (_replaced.ContainsKey(index) || _cache.TryGet(index, out _))
            {
                return true;
            }

            if (!_fileIndex.Valid(index, out int _, out int extra, out bool _))
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

            IEntry entry = null;
            Stream stream = _fileIndex.Seek(index, ref entry, out bool patched);
            if (stream == null || entry == null)
            {
                return null;
            }

            if (entry.Extra1 == -1)
            {
                return null;
            }

            // Compressed UOPs
            if (entry.Flag >= CompressionFlag.Zlib)
            {
                if (patched)
                {
                    throw new InvalidOperationException("Verdata.mul is not supported for compressed UOP");
                }

                if (_streamBuffer == null || _streamBuffer.Length < entry.Length)
                {
                    _streamBuffer = new byte[entry.Length];
                }

                stream.ReadExactly(_streamBuffer, 0, entry.Length);

                var result = UopUtils.Decompress(_streamBuffer);
                if (result.success is false)
                {
                    return null;
                }

                if (entry.Flag == CompressionFlag.Mythic)
                {
                    _streamBuffer = MythicDecompress.Decompress(result.data);
                }

                using (BinaryReader reader = new BinaryReader(new MemoryStream(_streamBuffer)))
                {
                    byte[] extra = reader.ReadBytes(8);

                    width = (extra[3] << 24) | (extra[2] << 16) | (extra[1] << 8) | extra[0];
                    height = (extra[7] << 24) | (extra[6] << 16) | (extra[5] << 8) | extra[4];

                    // TODO: Tbh, whole code needs to be reworked with readers, as we're doing useless work here just re-reading everything but 8 first bytes
                    _streamBuffer = reader.ReadBytes(_streamBuffer.Length - 8);
                }

                entry.Extra1 = width;
                entry.Extra2 = height;
            }

            width = entry.Extra1;
            height = entry.Extra2;

            if (width <= 0 || height <= 0)
            {
                return null;
            }

            if (entry.Flag == CompressionFlag.Mythic)
            {
                return _streamBuffer;
            }

            var length = entry.Length;
            if (patched)
            {
                length = entry.Length & 0x7FFFFFFF;
            }

            var buffer = new byte[length];
            stream.ReadExactly(buffer, 0, length);

            return buffer;
        }

        /// <summary>
        /// Returns Bitmap of index and applies Hue
        /// </summary>
        /// <param name="index"></param>
        /// <param name="hue"></param>
        /// <param name="onlyHueGrayPixels"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        // TODO: Currently unused and may be broken because of recent UOP changes. Needs verdata `patched` checks and compression handling
        public static unsafe Bitmap GetGump(int index, Hue hue, bool onlyHueGrayPixels, out bool patched)
        {
            Stream stream = _fileIndex.Seek(index, out int length, out int extra, out patched);

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

            if (width <= 0 || height <= 0)
            {
                return null;
            }

            int bytesPerLine = width << 1;
            int bytesPerStride = (bytesPerLine + 3) & ~3;
            int bytesForImage = height * bytesPerStride;

            int pixelsPerStride = (width + 1) & ~1;
            int pixelsPerStrideDelta = pixelsPerStride - width;

            byte[] pixelBuffer = _pixelBuffer;

            if (pixelBuffer == null || pixelBuffer.Length < bytesForImage)
            {
                _pixelBuffer = pixelBuffer = new byte[(bytesForImage + 2047) & ~2047];
            }

            byte[] streamBuffer = _streamBuffer;

            if (streamBuffer == null || streamBuffer.Length < length)
            {
                _streamBuffer = streamBuffer = new byte[(length + 2047) & ~2047];
            }

            byte[] colorTable = _colorTable;

            if (colorTable == null)
            {
                _colorTable = colorTable = new byte[128];
            }

            stream.ReadExactly(streamBuffer, 0, length);

            fixed (ushort* psHueColors = hue.Colors)
            {
                fixed (byte* pbStream = streamBuffer)
                {
                    fixed (byte* pbPixels = pixelBuffer)
                    {
                        fixed (byte* pbColorTable = colorTable)
                        {
                            var pHueColors = psHueColors;
                            ushort* pHueColorsEnd = pHueColors + 32;

                            var pColorTable = (ushort*)pbColorTable;

                            ushort* pColorTableOpaque = pColorTable;

                            while (pHueColors < pHueColorsEnd)
                            {
                                *pColorTableOpaque++ = *pHueColors++;
                            }

                            var pPixelDataStart = (ushort*)pbPixels;

                            var pLookup = (int*)pbStream;
                            int* pLookupEnd = pLookup + height;
                            int* pPixelRleStart = pLookup;
                            int* pPixelRle;

                            ushort* pPixel = pPixelDataStart;
                            ushort* pRleEnd;
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
                                        {
                                            color = pColorTable[color >> 10];
                                        }
                                        else if (color != 0)
                                        {
                                            color ^= 0x8000;
                                        }

                                        while (pPixel < pRleEnd)
                                        {
                                            *pPixel++ = color;
                                        }
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
                                        {
                                            color = pColorTable[color >> 10];
                                        }

                                        while (pPixel < pRleEnd)
                                        {
                                            *pPixel++ = color;
                                        }
                                    }

                                    pPixel += pixelsPerStrideDelta;
                                    pPixelEnd += pixelsPerStride;
                                }
                            }

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
        public static Bitmap GetGump(int index)
        {
            return GetGump(index, out bool _);
        }

        /// <summary>
        /// Decodes a gump into a caller-supplied pixel buffer. Lets the caller
        /// reuse a single shared destination across many decodes (e.g. a
        /// listview rendering thumbnails) instead of paying the per-call
        /// `new Bitmap(...)` + GDI handle + LockBits cost.
        ///
        /// `destination` must be at least <paramref name="width"/> *
        /// <paramref name="height"/> ushorts; the buffer is filled with
        /// Format16bppArgb1555 pixels. Returns false if the gump is missing,
        /// removed, the entry has invalid dimensions, or the buffer is too
        /// small. width / height are out parameters and are filled even when
        /// the buffer is too small, so callers can resize and retry.
        ///
        /// Cache semantics: this method does not write to or read from
        /// _cache — every call decodes from disk. Use GetGump when you want
        /// the standard bitmap cache.
        /// </summary>
        public static unsafe bool TryGetGumpPixels(int index, Span<ushort> destination, out int width, out int height, out bool patched)
        {
            width = 0;
            height = 0;
            patched = _patched.ContainsKey(index) && _patched[index];

            if (index < 0 || index > _indexLength - 1)
            {
                return false;
            }

            if (_removed[index])
            {
                return false;
            }

            IEntry entry = null;
            Stream stream = _fileIndex.Seek(index, ref entry, out patched);
            if (stream == null || entry == null || entry.Extra1 == -1)
            {
                return false;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            int length = entry.Length;
            if (patched)
            {
                length = entry.Length & 0x7FFFFFFF;
            }

            byte[] rented = ArrayPool<byte>.Shared.Rent(length);
            byte[] zlibBuf = null;
            try
            {
                stream.ReadExactly(rented, 0, length);

                byte[] data = rented;
                int dataOffset = 0;
                width = entry.Extra1;
                height = entry.Extra2;

                if (entry.Flag >= CompressionFlag.Zlib)
                {
                    int decSize = entry.DecompressedLength;
                    if (decSize <= 8)
                    {
                        return false;
                    }

                    zlibBuf = ArrayPool<byte>.Shared.Rent(decSize);
                    if (!UopUtils.TryDecompressInto(rented, 0, length, zlibBuf, out int zlibLen))
                    {
                        return false;
                    }

                    if (entry.Flag == CompressionFlag.Mythic)
                    {
                        // Mythic still allocates the final byte[]; that's the next lever.
                        data = MythicDecompress.Decompress(zlibBuf, 0, zlibLen);
                    }
                    else
                    {
                        data = zlibBuf;
                    }

                    // Header: 4-byte width then 4-byte height (little-endian), pixel data at offset 8.
                    width = data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
                    height = data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24);
                    dataOffset = 8;
                    entry.Extra1 = width;
                    entry.Extra2 = height;
                }

                if (width <= 0 || height <= 0 || destination.Length < width * height)
                {
                    return false;
                }

                fixed (byte* dataPtr = data)
                fixed (ushort* destPtr = destination)
                {
                    byte* basePtr = dataPtr + dataOffset;
                    var lookup = (int*)basePtr;
                    var dat = (ushort*)basePtr;

                    for (int y = 0; y < height; ++y)
                    {
                        int count = (*lookup++ * 2);

                        ushort* cur = destPtr + y * width;
                        ushort* end = cur + width;

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

                return true;
            }
            finally
            {
                if (zlibBuf != null)
                {
                    ArrayPool<byte>.Shared.Return(zlibBuf);
                }
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        /// <summary>
        /// Returns the dimensions of a gump without decoding pixel data.
        /// Cheaper than TryGetGumpPixels when the caller only needs to size a
        /// destination buffer. For UOP-compressed entries we still have to
        /// decompress to recover width/height — those are paid for on the
        /// first hit and cached via entry.Extra1/Extra2.
        /// </summary>
        public static bool TryGetGumpDimensions(int index, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (index < 0 || index >= _indexLength || _fileIndex.FileAccessor == null)
            {
                return false;
            }

            IEntry entry = _fileIndex[index];
            if (entry.Lookup < 0 || entry.Extra1 == -1)
            {
                return false;
            }

            // For uncompressed entries the index already knows width/height.
            if (entry.Flag < CompressionFlag.Zlib)
            {
                width = entry.Extra1;
                height = entry.Extra2;
                return width > 0 && height > 0;
            }

            // Compressed entries need a one-shot decode to recover dims.
            // Falls through to TryGetGumpPixels with a 0-length destination
            // which returns false but populates width/height.
            return TryGetGumpPixels(index, Span<ushort>.Empty, out width, out height, out _);
        }

        /// <summary>
        /// Returns Bitmap of index and if verdata patched
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public static unsafe Bitmap GetGump(int index, out bool patched)
        {
            patched = _patched.ContainsKey(index) && _patched[index];

            if (index > _indexLength - 1)
            {
                return null;
            }

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

            IEntry entry = null;
            Stream stream = _fileIndex.Seek(index, ref entry, out patched);
            if (stream == null || entry == null)
            {
                return null;
            }

            if (entry.Extra1 == -1)
            {
                return null;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            var length = entry.Length;
            if (patched)
            {
                length = entry.Length & 0x7FFFFFFF;
            }

            byte[] rented = ArrayPool<byte>.Shared.Rent(length);
            byte[] zlibBuf = null;
            try
            {
                stream.ReadExactly(rented, 0, length);

                byte[] data = rented;
                int dataOffset = 0;

                uint width = (uint)entry.Extra1;
                uint height = (uint)entry.Extra2;

                // Compressed UOPs
                if (entry.Flag >= CompressionFlag.Zlib)
                {
                    int decSize = entry.DecompressedLength;
                    if (decSize <= 8)
                    {
                        return null;
                    }

                    zlibBuf = ArrayPool<byte>.Shared.Rent(decSize);
                    if (!UopUtils.TryDecompressInto(rented, 0, length, zlibBuf, out int zlibLen))
                    {
                        return null;
                    }

                    if (entry.Flag == CompressionFlag.Mythic)
                    {
                        // Mythic still allocates the final byte[]; that's the next lever.
                        data = MythicDecompress.Decompress(zlibBuf, 0, zlibLen);
                    }
                    else
                    {
                        data = zlibBuf;
                    }

                    width = (uint)(data[0] | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
                    height = (uint)(data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24));
                    dataOffset = 8;

                    entry.Extra1 = (int)width;
                    entry.Extra2 = (int)height;
                }

                if (width <= 0 || height <= 0)
                {
                    return null;
                }

                try
                {
                    var bmp = new Bitmap((int)width, (int)height, PixelFormat.Format16bppArgb1555);
                    BitmapData bd = bmp.LockBits(
                        new Rectangle(0, 0, (int)width, (int)height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

                    try
                    {
                        fixed (byte* dataPtr = data)
                        {
                            byte* basePtr = dataPtr + dataOffset;
                            var lookup = (int*)basePtr;
                            var dat = (ushort*)basePtr;

                            var line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;

                            for (int y = 0; y < (int)height; ++y, line += delta)
                            {
                                int count = (*lookup++ * 2);

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
                    }
                    finally
                    {
                        bmp.UnlockBits(bd);
                    }

                    if (Files.CacheData)
                    {
                        _cache.Set(index, bmp);
                    }

                    return bmp;
                }
                catch (Exception)
                {
                    // ignored
                    return null;
                }
            }
            finally
            {
                if (zlibBuf != null)
                {
                    ArrayPool<byte>.Shared.Return(zlibBuf);
                }
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        public static unsafe void Save(string path)
        {
            string idx = Path.Combine(path, "Gumpidx.mul");
            string mul = Path.Combine(path, "Gumpart.mul");

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var binidx = new BinaryWriter(fsidx))
            using (var binmul = new BinaryWriter(fsmul))
            {
                for (int index = 0; index < _indexLength; index++)
                {
                    Files.FireFileSaveEvent();
                    // GetGump transparently checks _replaced first, then the
                    // LRU cache, then decodes from disk.
                    Bitmap bmp = GetGump(index);
                    if ((bmp == null) || (_removed[index]))
                    {
                        binidx.Write(-1); // lookup
                        binidx.Write(0); // length
                        binidx.Write(0); // extra
                    }
                    else
                    {
                        BitmapData bd = bmp.LockBits(
                            new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                            PixelFormat.Format16bppArgb1555);

                        var line = (ushort*)bd.Scan0;
                        int delta = bd.Stride >> 1;

                        binidx.Write((int)fsmul.Position); // lookup
                        var length = (int)fsmul.Position;
                        const int fill = 0;
                        for (int i = 0; i < bmp.Height; ++i)
                        {
                            binmul.Write(fill);
                        }

                        for (int y = 0; y < bmp.Height; ++y, line += delta)
                        {
                            ushort* cur = line;

                            int x = 0;
                            var current = (int)fsmul.Position;
                            fsmul.Seek(length + (y * 4), SeekOrigin.Begin);
                            int offset = (current - length) / 4;
                            binmul.Write(offset);
                            fsmul.Seek(length + (offset * 4), SeekOrigin.Begin);

                            while (x < bd.Width)
                            {
                                int run = 1;
                                ushort c = cur[x];
                                while ((x + run) < bd.Width)
                                {
                                    if (c != cur[x + run])
                                    {
                                        break;
                                    }

                                    ++run;
                                }

                                if (c == 0)
                                {
                                    binmul.Write(c);
                                }
                                else
                                {
                                    binmul.Write((ushort)(c ^ 0x8000));
                                }

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