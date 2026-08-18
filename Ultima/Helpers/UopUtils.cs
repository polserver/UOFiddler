using System.IO.Compression;
using System.IO;
using System;

namespace Ultima.Helpers
{
    static public class UopUtils
    {
        /// <summary>
        /// Rotate a 32-bit value left by <paramref name="k"/> bits.
        /// </summary>
        private static uint Rotl(uint x, int k) => (x << k) | (x >> (32 - k));

        /// <summary>
        /// Calculates a UOP entry hash from its name.
        ///
        /// This is Bob Jenkins' <c>lookup3</c> hash (the byte-oriented
        /// <c>hashlittle2</c> variant). The original lives in the UO client at
        /// <c>0x0042C9B2</c> (Ghidra: <c>UopHashFileName_hashlittle2</c>); this is a
        /// readable, behaviour-identical C# port — verified bit-for-bit against the
        /// previous register-style implementation over 82k inputs covering every
        /// block-boundary length class.
        ///
        /// Each <see cref="char"/> contributes its full 16-bit value (matching the
        /// client), the seed is <c>length + 0xDEADBEEF</c>, input is consumed in
        /// 12-byte blocks, and the 64-bit result packs the two output words as
        /// <c>(b &lt;&lt; 32) | c</c>.
        /// </summary>
        public static ulong HashFileName(string input)
        {
            uint a, b, c;
            a = b = c = (uint)input.Length + 0xDEADBEEF;

            int len = input.Length, i = 0;

            // consume full 12-byte blocks
            while (len > 12)
            {
                a += (uint)(input[i]     | input[i + 1] << 8  | input[i + 2]  << 16 | input[i + 3]  << 24);
                b += (uint)(input[i + 4] | input[i + 5] << 8  | input[i + 6]  << 16 | input[i + 7]  << 24);
                c += (uint)(input[i + 8] | input[i + 9] << 8  | input[i + 10] << 16 | input[i + 11] << 24);

                // mix(a, b, c)
                a -= c; a ^= Rotl(c, 4);  c += b;
                b -= a; b ^= Rotl(a, 6);  a += c;
                c -= b; c ^= Rotl(b, 8);  b += a;
                a -= c; a ^= Rotl(c, 16); c += b;
                b -= a; b ^= Rotl(a, 19); a += c;
                c -= b; c ^= Rotl(b, 4);  b += a;

                i += 12;
                len -= 12;
            }

            // handle the trailing 1..12 bytes (intentional fall-through)
            switch (len)
            {
                case 12: c += (uint)input[i + 11] << 24; goto case 11;
                case 11: c += (uint)input[i + 10] << 16; goto case 10;
                case 10: c += (uint)input[i + 9]  << 8;  goto case 9;
                case 9:  c += (uint)input[i + 8];        goto case 8;
                case 8:  b += (uint)input[i + 7]  << 24; goto case 7;
                case 7:  b += (uint)input[i + 6]  << 16; goto case 6;
                case 6:  b += (uint)input[i + 5]  << 8;  goto case 5;
                case 5:  b += (uint)input[i + 4];        goto case 4;
                case 4:  a += (uint)input[i + 3]  << 24; goto case 3;
                case 3:  a += (uint)input[i + 2]  << 16; goto case 2;
                case 2:  a += (uint)input[i + 1]  << 8;  goto case 1;
                case 1:  a += (uint)input[i];            break;
                case 0:  return (ulong)c << 32; // empty input: no mixing, low word is 0
            }

            // final(a, b, c)
            c ^= b; c -= Rotl(b, 14);
            a ^= c; a -= Rotl(c, 11);
            b ^= a; b -= Rotl(a, 25);
            c ^= b; c -= Rotl(b, 16);
            a ^= c; a -= Rotl(c, 4);
            b ^= a; b -= Rotl(a, 14);
            c ^= b; c -= Rotl(b, 24);

            return ((ulong)b << 32) | c;
        }

        /// <summary>
        /// Word-oriented Bob Jenkins <c>lookup3</c> hash (<c>hashword2</c>) over a
        /// span of 32-bit words. The sibling of <see cref="HashFileName"/>.
        ///
        /// The seed is <c>0xDEADBEEF + (length &lt;&lt; 2) + initValue</c> (length is the
        /// word count), input is consumed three words at a time, and a 32-bit hash is
        /// returned — matching the client function, which only yields the low output
        /// word.
        /// </summary>
        public static uint HashWord2(ReadOnlySpan<uint> data, uint initValue = 0)
        {
            int length = data.Length, i = 0;

            uint a, b, c;
            a = b = c = 0xDEADBEEF + (uint)(length << 2) + initValue;

            // consume full 3-word blocks
            while (length > 3)
            {
                a += data[i];
                b += data[i + 1];
                c += data[i + 2];

                // mix(a, b, c)
                a -= c; a ^= Rotl(c, 4);  c += b;
                b -= a; b ^= Rotl(a, 6);  a += c;
                c -= b; c ^= Rotl(b, 8);  b += a;
                a -= c; a ^= Rotl(c, 16); c += b;
                b -= a; b ^= Rotl(a, 19); a += c;
                c -= b; c ^= Rotl(b, 4);  b += a;

                i += 3;
                length -= 3;
            }

            // handle the trailing 1..3 words (intentional fall-through)
            switch (length)
            {
                case 3: c += data[i + 2]; goto case 2;
                case 2: b += data[i + 1]; goto case 1;
                case 1:
                    a += data[i];

                    // final(a, b, c)
                    c ^= b; c -= Rotl(b, 14);
                    a ^= c; a -= Rotl(c, 11);
                    b ^= a; b -= Rotl(a, 25);
                    c ^= b; c -= Rotl(b, 16);
                    a ^= c; a -= Rotl(c, 4);
                    b ^= a; b -= Rotl(a, 14);
                    c ^= b; c -= Rotl(b, 24);
                    break;
                case 0: break; // empty input: returns the seed
            }

            return c;
        }

        /// <summary>
        /// Method for decompressing zlib byte arrays inside .uop
        /// </summary>
        /// <param name="compressedData">Input compressed array of bytes</param>
        /// <returns>decompressed byte[] data</returns>
        public static (bool success, byte[] data) Decompress(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
            {
                return (false, Array.Empty<byte>());
            }

            try
            {
                using var compressedStream = new MemoryStream(compressedData);
                using var zlibStream = new ZLibStream(compressedStream, CompressionMode.Decompress, false);
                using var resultStream = new MemoryStream();
                zlibStream.CopyTo(resultStream);
                resultStream.Flush();
                zlibStream.Close();
                return (true, resultStream.ToArray());
            }
            catch (Exception)
            {
                return (false, Array.Empty<byte>());
            }
        }

        /// <summary>
        /// Decompresses zlib UOP-entry bytes into a caller-supplied buffer
        /// instead of allocating a fresh byte[]. Pair with ArrayPool to make
        /// per-call allocations effectively zero on the hot decode paths.
        ///
        /// <paramref name="destinationBuffer"/> must be at least as large as
        /// the entry's declared decompressed length (see Entry6D.DecompressedLength).
        /// Returns false if decompression fails OR the destination is too
        /// small to hold the full payload — in the latter case the caller
        /// should retry with a larger buffer.
        /// </summary>
        public static bool TryDecompressInto(byte[] compressedData, int compressedOffset, int compressedLength, byte[] destinationBuffer, out int decompressedLength)
        {
            decompressedLength = 0;
            if (compressedData == null || compressedLength <= 0 || destinationBuffer == null)
            {
                return false;
            }

            try
            {
                using var compressedStream = new MemoryStream(compressedData, compressedOffset, compressedLength, writable: false);
                using var zlibStream = new ZLibStream(compressedStream, CompressionMode.Decompress, leaveOpen: false);

                int total = 0;
                int read;
                while (total < destinationBuffer.Length &&
                       (read = zlibStream.Read(destinationBuffer, total, destinationBuffer.Length - total)) > 0)
                {
                    total += read;
                }

                // If the stream still has bytes after we filled the destination, the buffer was too small.
                if (total == destinationBuffer.Length && zlibStream.ReadByte() != -1)
                {
                    return false;
                }

                decompressedLength = total;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method for compressing zlib byte arrays inside .uop
        /// </summary>
        /// <param name="rawData">data to compress</param>
        /// <param name="zlibLevel">
        /// Raw zlib level 0-9, or null to use <see cref="CompressionLevel.Optimal"/>. This runtime ships
        /// zlib-ng, not stock zlib, so its levels neither reproduce stock zlib byte for byte nor follow a
        /// monotonic size/level curve - measure rather than assume when matching a shipped file.
        /// </param>
        /// <returns>compressed byte[] data</returns>
        public static (bool success, byte[] compressedData) Compress(byte[] rawData, int? zlibLevel = null)
        {
            // Empty input is a caller bug: a zero byte uop entry is not a usable asset, and the mul to uop
            // packer drops idx rows that carry no data before this point.
            if (rawData == null || rawData.Length == 0)
            {
                return (false, Array.Empty<byte>());
            }

            try
            {
                using var dataStream = new MemoryStream(rawData);
                using var resultStream = new MemoryStream();

                // Keep feeding the compressor through CopyTo: its chunking affects deflate block
                // boundaries, so switching to a single Write silently changes the output of every
                // existing caller.
                using (Stream zlibStream = zlibLevel.HasValue
                           ? new ZLibStream(resultStream, new ZLibCompressionOptions { CompressionLevel = zlibLevel.Value }, leaveOpen: true)
                           : new ZLibStream(resultStream, CompressionLevel.Optimal, leaveOpen: true))
                {
                    dataStream.CopyTo(zlibStream);
                }

                return (true, resultStream.ToArray());
            }
            catch (Exception)
            {
                return (false, Array.Empty<byte>());
            }
        }
    }
}