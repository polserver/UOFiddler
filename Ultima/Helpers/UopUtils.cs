using System.IO.Compression;
using System.IO;
using System;

namespace Ultima.Helpers
{
    static public class UopUtils
    {
        /// <summary>
        /// Method for calculating entry hash by its name.
        /// Taken from Mythic.Package.dll
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static ulong HashFileName(string s)
        {
            uint eax, ecx, edx, ebx, esi, edi;

            eax = ecx = edx = ebx = esi = edi = 0;
            ebx = edi = esi = (uint)s.Length + 0xDEADBEEF;

            int i = 0;

            for (i = 0; i + 12 < s.Length; i += 12)
            {
                edi = (uint)((s[i + 7] << 24) | (s[i + 6] << 16) | (s[i + 5] << 8) | s[i + 4]) + edi;
                esi = (uint)((s[i + 11] << 24) | (s[i + 10] << 16) | (s[i + 9] << 8) | s[i + 8]) + esi;
                edx = (uint)((s[i + 3] << 24) | (s[i + 2] << 16) | (s[i + 1] << 8) | s[i]) - esi;

                edx = (edx + ebx) ^ (esi >> 28) ^ (esi << 4);
                esi += edi;
                edi = (edi - edx) ^ (edx >> 26) ^ (edx << 6);
                edx += esi;
                esi = (esi - edi) ^ (edi >> 24) ^ (edi << 8);
                edi += edx;
                ebx = (edx - esi) ^ (esi >> 16) ^ (esi << 16);
                esi += edi;
                edi = (edi - ebx) ^ (ebx >> 13) ^ (ebx << 19);
                ebx += esi;
                esi = (esi - edi) ^ (edi >> 28) ^ (edi << 4);
                edi += ebx;
            }

            if (s.Length - i > 0)
            {
                switch (s.Length - i)
                {
                    case 12:
                        esi += (uint)s[i + 11] << 24;
                        goto case 11;
                    case 11:
                        esi += (uint)s[i + 10] << 16;
                        goto case 10;
                    case 10:
                        esi += (uint)s[i + 9] << 8;
                        goto case 9;
                    case 9:
                        esi += (uint)s[i + 8];
                        goto case 8;
                    case 8:
                        edi += (uint)s[i + 7] << 24;
                        goto case 7;
                    case 7:
                        edi += (uint)s[i + 6] << 16;
                        goto case 6;
                    case 6:
                        edi += (uint)s[i + 5] << 8;
                        goto case 5;
                    case 5:
                        edi += (uint)s[i + 4];
                        goto case 4;
                    case 4:
                        ebx += (uint)s[i + 3] << 24;
                        goto case 3;
                    case 3:
                        ebx += (uint)s[i + 2] << 16;
                        goto case 2;
                    case 2:
                        ebx += (uint)s[i + 1] << 8;
                        goto case 1;
                    case 1:
                        ebx += (uint)s[i];
                        break;
                }

                esi = (esi ^ edi) - ((edi >> 18) ^ (edi << 14));
                ecx = (esi ^ ebx) - ((esi >> 21) ^ (esi << 11));
                edi = (edi ^ ecx) - ((ecx >> 7) ^ (ecx << 25));
                esi = (esi ^ edi) - ((edi >> 16) ^ (edi << 16));
                edx = (esi ^ ecx) - ((esi >> 28) ^ (esi << 4));
                edi = (edi ^ edx) - ((edx >> 18) ^ (edx << 14));
                eax = (esi ^ edi) - ((edi >> 8) ^ (edi << 24));

                return ((ulong)edi << 32) | eax;
            }

            return ((ulong)esi << 32) | eax;
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
        /// <returns>compressed byte[] data</returns>
        public static (bool success, byte[] compressedData) Compress(byte[] rawData)
        {
            if (rawData == null || rawData.Length == 0)
            {
                return (false, Array.Empty<byte>());
            }

            try
            {
                using var dataStream = new MemoryStream(rawData);
                using var resultStream = new MemoryStream();
                using var zlibStream = new ZLibStream(resultStream, CompressionLevel.Optimal);
                dataStream.CopyTo(zlibStream);
                zlibStream.Flush();
                zlibStream.Close();
                return (true, resultStream.ToArray());
            }
            catch (Exception)
            {
                return (false, Array.Empty<byte>());
            }
        }
    }
}