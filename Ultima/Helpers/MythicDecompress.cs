using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultima.Helpers
{
    public static class MythicDecompress
    {
        private const uint HeaderXorKey = 0x8E2C9A3D;
        private const int FrequencyHeaderSize = 1024; // 256 ints

        public static byte[] Transform(byte[] buffer)
        {
            return MoveToFrontCoding.Encode(InternalCompress(buffer));
        }

        public static byte[] Detransform(byte[] buffer)
        {
            return InternalDecompress(MoveToFrontCoding.Decode(buffer));
        }

        public static byte[] Decompress(byte[] buffer)
        {
            return Decompress(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Decompresses a slice of <paramref name="buffer"/>. Lets callers pass
        /// pooled buffers that may be larger than the actual payload — the
        /// original Decompress(byte[]) overload reads BaseStream.Length, which
        /// would walk into uninitialized tail bytes when the input is pooled.
        /// </summary>
        public static byte[] Decompress(byte[] buffer, int offset, int length)
        {
            ReadOnlySpan<byte> source = buffer.AsSpan(offset, length);
            uint dataLength = PeekDecompressedLength(source);

            byte[] output = new byte[dataLength];
            if (!TryDecompress(source, output, out int written) || written != (int)dataLength)
            {
                throw new InvalidDataException(
                    $"Decompressed length {written} does not match expected {dataLength}. File is not in compressed cliloc format.");
            }

            return output;
        }

        /// <summary>
        /// Reads the embedded decompressed length from the 4-byte XOR-obfuscated
        /// header at the start of a Mythic payload. Lets callers size an
        /// <see cref="ArrayPool{T}"/> rent exactly before calling
        /// <see cref="TryDecompress"/>.
        /// </summary>
        public static uint PeekDecompressedLength(ReadOnlySpan<byte> source)
        {
            if (source.Length < 4)
            {
                return 0;
            }

            return BinaryPrimitives.ReadUInt32LittleEndian(source) ^ HeaderXorKey;
        }

        /// <summary>
        /// Pooled-friendly decompression. Reads the header, MTF-decodes the
        /// payload into a rented scratch span, and writes the final output
        /// into <paramref name="destination"/>. Returns false if the
        /// destination is too small or the payload is malformed.
        /// </summary>
        public static bool TryDecompress(ReadOnlySpan<byte> source, Span<byte> destination, out int written)
        {
            written = 0;

            if (source.Length < 4)
            {
                return false;
            }

            uint dataLength = BinaryPrimitives.ReadUInt32LittleEndian(source) ^ HeaderXorKey;
            if (destination.Length < (int)dataLength)
            {
                return false;
            }

            ReadOnlySpan<byte> mtfInput = source.Slice(4);

            byte[] rented = ArrayPool<byte>.Shared.Rent(mtfInput.Length);
            try
            {
                Span<byte> mtfBuffer = rented.AsSpan(0, mtfInput.Length);
                MoveToFrontCoding.Decode(mtfInput, mtfBuffer);

                if (!TryInternalDecompress(mtfBuffer, destination, out written))
                {
                    return false;
                }

                return written == (int)dataLength;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        public static byte[] InternalDecompress(Span<byte> input)
        {
            if (input.Length < FrequencyHeaderSize)
            {
                throw new InvalidDataException("Mythic payload smaller than frequency header.");
            }

            Span<int> header = stackalloc int[256];
            input.Slice(0, FrequencyHeaderSize).CopyTo(MemoryMarshal.AsBytes(header));

            int sum = 0;
            for (int i = 0; i < 256; i++)
            {
                sum += header[i];
            }

            if (sum == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] output = new byte[sum];
            if (!TryInternalDecompress(input, output, out int written) || written != sum)
            {
                throw new InvalidDataException("Mythic decompression produced unexpected length.");
            }

            return output;
        }

        /// <summary>
        /// Mythic stage 2: turns the MTF-decoded payload into the original
        /// bytes, writing into <paramref name="destination"/>. Returns false
        /// if the destination is too small or the input is malformed.
        /// </summary>
        public static bool TryInternalDecompress(ReadOnlySpan<byte> input, Span<byte> destination, out int written)
        {
            written = 0;

            try
            {
                if (input.Length < FrequencyHeaderSize)
                {
                    return false;
                }

                Span<byte> symbolTable = stackalloc byte[256];
                Span<byte> frequency = stackalloc byte[256];
                Span<int> partialInput = stackalloc int[256 * 3];

                partialInput.Clear();

                for (int i = 0; i < 256; i++)
                {
                    symbolTable[i] = (byte)i;
                }

                input.Slice(0, FrequencyHeaderSize).CopyTo(MemoryMarshal.AsBytes(partialInput));

                int sum = 0;
                for (int i = 0; i < 256; i++)
                {
                    sum += partialInput[i];
                }

                if (sum == 0)
                {
                    written = 0;
                    return true;
                }

                if (destination.Length < sum)
                {
                    return false;
                }

                int nonZeroCount = 0;
                for (int i = 0; i < 256; i++)
                {
                    if (partialInput[i] != 0)
                    {
                        nonZeroCount++;
                    }
                }

                Frequency(partialInput, frequency);

                for (int i = 0, m = 0; i < nonZeroCount; ++i)
                {
                    var freq = frequency[i];
                    symbolTable[input[m + FrequencyHeaderSize]] = freq;
                    partialInput[freq + 256] = m + 1;
                    m += partialInput[freq];
                    partialInput[freq + 512] = m;
                }

                byte val = symbolTable[0];
                int count = 0;

                do
                {
                    ref int firstValRef = ref partialInput[val + 256];
                    destination[count] = val;

                    if (firstValRef < partialInput[val + 512])
                    {
                        byte idx = input[firstValRef + FrequencyHeaderSize];
                        firstValRef++;

                        if (idx != 0)
                        {
                            ShiftLeft(symbolTable, idx);

                            symbolTable[idx] = val;
                            val = symbolTable[0];
                        }
                    }
                    else if (nonZeroCount-- > 0)
                    {
                        ShiftLeft(symbolTable, nonZeroCount);

                        val = symbolTable[0];
                    }

                    count++;
                } while (count < sum);

                written = sum;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //
        // Looking for max value in freq table, adding it index to output and erasing to find next max value except last one
        //
        static void Frequency(Span<int> input, Span<byte> output)
        {
            Span<int> tmp = stackalloc int[256];
            input.Slice(0, tmp.Length).CopyTo(tmp);

            for (var i = 0; i < 256; i++)
            {
                uint value = 0;
                byte index = 0;

                for (var j = 0; j < 256; j++)
                {
                    if (tmp[j] > value)
                    {
                        index = (byte)j;
                        value = (uint)tmp[j];
                    }
                }

                if (value == 0)
                {
                    break;
                }

                output[i] = index;
                tmp[index] = 0;
            }
        }

        // element - this is index of element until which we re shifting our array to the left, destroying zero element
        static void ShiftLeft(Span<byte> input, int element)
        {
            for (var i = 0; i < element; ++i)
            {
                input[i] = input[i + 1];
            }
        }

        public static byte[] InternalCompress(Span<byte> input)
        {
            Span<byte> symbolTable = stackalloc byte[256];
            Span<byte> frequency = stackalloc byte[256];
            Span<int> partialInput = stackalloc int[256 * 3];

            // counting frequencies
            for (int i = 0; i < input.Length; ++i)
            {
                partialInput[input[i]]++;
            }

            Frequency(partialInput, frequency);

            var nonZeroCount = 0;
            for (var i = 0; i < 256; i++)
            {
                if (partialInput[i] != 0)
                {
                    nonZeroCount++;
                }
            }

            byte[] output = new byte[input.Length + nonZeroCount + 1024];

            for (int i = 0, m = 0; i < nonZeroCount; ++i)
            {
                var freqIndex = frequency[i];
                partialInput[freqIndex + 256] = m + 1;
                m += partialInput[freqIndex];
                partialInput[freqIndex + 512] = m;
            }

            for (int i = 0; i < 256; ++i)
            {
                BinaryPrimitives.WriteInt32LittleEndian(output.AsSpan(i * 4, 4), partialInput[i]);
            }

            int count = input.Length - 1;

            Span<bool> added = stackalloc bool[256];
            int addedCount = 0;

            do
            {
                var val = input[count];

                ref int firstValRef = ref partialInput[val + 512];
                var outputAddress = firstValRef + 1024;

                // first add, just put it in symbolTable from the left and assign 0 idx
                if (!added[val])
                {
                    ShiftRight(symbolTable, addedCount);
                    symbolTable[0] = val;
                    added[val] = true;
                    addedCount++;
                    output[outputAddress] = 0;
                }
                // we're already have symbol in table, so getting it idx and putting it in output stream
                else if (firstValRef >= partialInput[val + 256])
                {
                    var idx = GetIdx(symbolTable, val, addedCount);
                    ShiftRight(symbolTable, idx);
                    symbolTable[0] = val;
                    output[outputAddress] = idx;
                }
                firstValRef--;

                count--;
            } while (count >= 0);

            for (int i = 0, m = 0; i < nonZeroCount; ++i)
            {
                var freqIndex = frequency[i];
                output[m + 1024] = GetIdx(symbolTable, freqIndex, nonZeroCount);
                m += partialInput[freqIndex];
            }

            return output;
        }

        private static byte GetIdx(Span<byte> input, byte val, int nonZeroCount)
        {
            for (byte i = 0; i < input.Length && i < nonZeroCount; ++i)
            {
                if (input[i] == val)
                {
                    return i;
                }
            }

            return 0;
        }

        static void ShiftRight(Span<byte> input, int element)
        {
            for (var i = element; i >= 1; --i)
            {
                input[i] = input[i - 1];
            }
        }
    }
}
