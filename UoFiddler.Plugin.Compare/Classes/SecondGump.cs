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
using System.Buffers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using Ultima;
using Ultima.Helpers;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondGump
    {
        /// <summary>
        /// Gump id ceiling, matching <c>Ultima.Gumps</c>. Ids 69971..69985 ship in 7.0.98.1 and later,
        /// above the old 0xFFFF bound - with that bound no UOP name hash was ever generated for them,
        /// so they did not exist as far as this reader was concerned.
        /// </summary>
        private const int _maxGumpIndex = 0x12000;

        private static SecondFileIndex _fileIndex;

        private static Bitmap[] _cache = Array.Empty<Bitmap>();
        private static byte[] _streamBuffer;

        // Authoritative id range for this index; 0 until a second client is loaded.
        private static int _indexLength;

        private const byte _contentUnknown = 0;
        private const byte _contentEmpty = 1;
        private const byte _contentPresent = 2;

        /// <summary>
        /// Per id answer to "does this entry contain a drawable gump", filled in on demand.
        /// </summary>
        /// <remarks>
        /// A stored entry carries its real width/height in the index; a compressed one does not, so
        /// <see cref="SecondFileIndex"/> parks the "dimensions unknown" sentinel 0x0FFFFFFF there, which
        /// reads back as 0x0FFF x 0xFFFF - non zero, therefore "valid". EA ships 0x0 placeholder gumps
        /// (29, 33, 34, 37, 47, 49, 98 ...), so on a compressed client those listed and failed to draw.
        /// </remarks>
        private static byte[] _contentState = Array.Empty<byte>();

        /// <summary>
        /// Compressed bytes read when probing an entry for content - enough to inflate its first few
        /// output bytes, rather than the whole entry.
        /// </summary>
        private const int _contentPeekWindow = 4096;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            SetFileIndex(idxPath, mulPath, null);
        }

        public static void SetFileIndex(string idxPath, string mulPath, string uopPath)
        {
            // Build first: a bad UOP throws out of the ctor and leaves the previous index usable.
            var newIndex = new SecondFileIndex(idxPath, mulPath, uopPath, _maxGumpIndex, ".tga", -1, true);

            SecondFileIndex oldIndex = _fileIndex;
            Bitmap[] oldCache = _cache;

            _fileIndex = newIndex;
            _indexLength = newIndex.IndexLength;
            _cache = new Bitmap[_indexLength];
            _contentState = new byte[_indexLength];
            _streamBuffer = null;

            // Callers must have dropped any bitmap they still hold (see CompareGumpControl.Load_Click)
            // before we get here - these instances are the cached ones, not copies.
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

        /// <summary>
        /// Number of gump ids this index covers, 0 when no second client is loaded.
        /// </summary>
        public static int GetCount()
        {
            return _indexLength;
        }

        public static bool IsValidIndex(int index)
        {
            if (_fileIndex == null)
            {
                return false;
            }

            if (index < 0 || index > _indexLength - 1)
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            if (!_fileIndex.Valid(index, out int _, out int extra))
            {
                return false;
            }

            if (extra == -1)
            {
                return false;
            }

            byte state = _contentState[index];
            if (state != _contentUnknown)
            {
                return state == _contentPresent;
            }

            return ProbeContent(index, extra);
        }

        /// <summary>
        /// Works out once, and remembers, whether an entry actually holds a drawable gump.
        /// See <see cref="_contentState"/> for why the index alone cannot answer this.
        /// </summary>
        /// <remarks>
        /// Mirrors <c>Ultima.Gumps.ProbeContent</c> minus the verdata branch - this reader never applies
        /// verdata patches, so an entry's length high bit is never set. Keep the two in sync.
        /// </remarks>
        private static bool ProbeContent(int index, int packedExtra)
        {
            SecondIEntry entry = _fileIndex[index];
            if (entry == null || entry.Lookup < 0)
            {
                _contentState[index] = _contentEmpty;
                return false;
            }

            // The index can answer for stored entries. For zlib it still can: the payload is the eight
            // byte width/height header plus pixels, so a declared length of eight or less is a 0x0 gump.
            // Mythic cannot - there DecompressedLength is the inner stream length.
            if (entry.Flag == SecondCompressionFlag.None)
            {
                bool stored = ((packedExtra >> 16) & 0xFFFF) > 0 && (packedExtra & 0xFFFF) > 0;
                _contentState[index] = stored ? _contentPresent : _contentEmpty;
                return stored;
            }

            if (entry.Flag == SecondCompressionFlag.Zlib && entry.DecompressedLength <= 8)
            {
                _contentState[index] = _contentEmpty;
                return false;
            }

            Stream stream = _fileIndex.Seek(index, ref entry);
            if (stream == null)
            {
                return false;
            }

            bool? present = CompressedEntryHasContent(stream, entry, index);
            if (present == null)
            {
                // Unreadable, not empty: leave the state unknown so a later call retries.
                return false;
            }

            _contentState[index] = present.Value ? _contentPresent : _contentEmpty;

            return present.Value;
        }

        /// <summary>
        /// Inflates just the head of a compressed entry to find out whether it has any pixels. Null means
        /// the entry could not be read, which is not the same answer as an empty gump and is not cached.
        /// </summary>
        /// <remarks>
        /// Line for line the same logic as <c>Ultima.Gumps.CompressedEntryHasContent</c>, which is private
        /// and typed against <c>Ultima.IEntry</c>. Keep the two in sync: if the two sides disagree about
        /// which ids are valid, the compare tabs report differences that do not exist.
        /// </remarks>
        private static bool? CompressedEntryHasContent(Stream stream, SecondIEntry entry, int index)
        {
            int length = entry.Length & 0x7FFFFFFF;
            if (length <= 0)
            {
                return false;
            }

            int toRead = Math.Min(length, _contentPeekWindow);
            byte[] rented = ArrayPool<byte>.Shared.Rent(toRead);

            try
            {
                stream.ReadExactly(rented, 0, toRead);

                using var compressed = new MemoryStream(rented, 0, toRead, writable: false);
                using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);

                if (entry.Flag == SecondCompressionFlag.Mythic)
                {
                    // Layered zlib(mythic(payload)). The Mythic header carries its own decompressed length,
                    // so a payload of only the eight byte width/height header is a 0x0 gump.
                    var mythicHeader = new byte[4];
                    zlib.ReadExactly(mythicHeader, 0, mythicHeader.Length);

                    return MythicDecompress.PeekDecompressedLength(mythicHeader) > 8;
                }

                var head = new byte[8];
                zlib.ReadExactly(head, 0, head.Length);

                int width = head[0] | (head[1] << 8) | (head[2] << 16) | (head[3] << 24);
                int height = head[4] | (head[5] << 8) | (head[6] << 16) | (head[7] << 24);

                if (width <= 0 || height <= 0)
                {
                    return false;
                }

                _fileIndex.CacheDimensions(index, width, height);

                return true;
            }
            catch (EndOfStreamException)
            {
                // Runs off the end of the file - a permanent property of it.
                return false;
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException)
            {
                // Locked or gone. Say nothing rather than remember a wrong answer.
                return null;
            }
            catch (Exception)
            {
                // Corrupt payload - nothing drawable either way.
                return false;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        public static byte[] GetRawGump(int index, out int width, out int height)
        {
            width = -1;
            height = -1;

            if (_fileIndex == null)
            {
                return null;
            }

            if (index < 0 || index >= _indexLength)
            {
                return null;
            }

            SecondIEntry entry = null;
            Stream stream = _fileIndex.Seek(index, ref entry);
            if (stream == null || entry == null)
            {
                return null;
            }

            if (entry.Extra1 == -1)
            {
                return null;
            }

            int payloadLength = ReadEntryPayload(index, stream, entry, out width, out height);
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

            if (index < 0 || index >= _indexLength)
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

            int payloadLength = ReadEntryPayload(index, stream, entry, out int width, out int height);
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
        private static int ReadEntryPayload(int index, Stream stream, SecondIEntry entry, out int width, out int height)
        {
            int length = entry.Length & 0x7FFFFFFF;
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
                int decSize = entry.DecompressedLength;
                if (decSize <= 8)
                {
                    width = height = -1;
                    return 0;
                }

                byte[] zlibBuf = ArrayPool<byte>.Shared.Rent(decSize);
                byte[] mythicBuf = null;
                try
                {
                    if (!UopUtils.TryDecompressInto(_streamBuffer, 0, length, zlibBuf, out int zlibLen))
                    {
                        width = height = -1;
                        return 0;
                    }

                    byte[] payload;
                    int payloadLength;

                    if (entry.Flag == SecondCompressionFlag.Mythic)
                    {
                        uint mythicLen = MythicDecompress.PeekDecompressedLength(zlibBuf.AsSpan(0, zlibLen));
                        if (mythicLen <= 8 || mythicLen > int.MaxValue)
                        {
                            width = height = -1;
                            return 0;
                        }

                        mythicBuf = ArrayPool<byte>.Shared.Rent((int)mythicLen);
                        if (!MythicDecompress.TryDecompress(
                                zlibBuf.AsSpan(0, zlibLen), mythicBuf.AsSpan(0, (int)mythicLen), out _))
                        {
                            width = height = -1;
                            return 0;
                        }

                        payload = mythicBuf;
                        payloadLength = (int)mythicLen;
                    }
                    else
                    {
                        payload = zlibBuf;
                        payloadLength = zlibLen;
                    }

                    width = (payload[3] << 24) | (payload[2] << 16) | (payload[1] << 8) | payload[0];
                    height = (payload[7] << 24) | (payload[6] << 16) | (payload[5] << 8) | payload[4];

                    if (width <= 0 || height <= 0)
                    {
                        _contentState[index] = _contentEmpty;
                        return 0;
                    }

                    // Write-back has to go through the index: `entry` is a boxed copy, so assigning to
                    // entry.Extra1 here would be discarded.
                    _fileIndex.CacheDimensions(index, width, height);
                    _contentState[index] = _contentPresent;

                    int rleLen = payloadLength - 8;
                    if (_streamBuffer.Length < rleLen)
                    {
                        _streamBuffer = new byte[rleLen];
                    }
                    System.Buffer.BlockCopy(payload, 8, _streamBuffer, 0, rleLen);
                    return rleLen;
                }
                finally
                {
                    if (mythicBuf != null)
                    {
                        ArrayPool<byte>.Shared.Return(mythicBuf);
                    }
                    ArrayPool<byte>.Shared.Return(zlibBuf);
                }
            }

            width = entry.Extra1;
            height = entry.Extra2;
            return length;
        }
    }
}
