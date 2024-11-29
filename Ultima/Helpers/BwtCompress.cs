/* EA Cliloc Compression
 * Author: Tecmo
 * Date: 2024.11.26
 * Note: Based on BwtDecompress provided by ClassicUO
 */

using System;
using System.IO;

namespace Ultima.Helpers
{
    public static class BwtCompress
    {
        public static byte[] Compress(byte[] input)
        {
            // Initialize output memory stream
            using (var memoryStream = new MemoryStream())
            using (var writer = new BinaryWriter(memoryStream))
            {
                // Build the frequency table and perform BWT transform
                Span<int> frequency = stackalloc int[256];
                BuildFrequencyTable(input, frequency);

                // Perform BWT transformation on the input
                var transformedData = PerformBwtTransform(input, frequency);

                // Write the first character (or index) used in the table
                writer.Write((byte)transformedData.FirstChar);

                // Write the transformed data
                writer.Write(transformedData.Data);

                return memoryStream.ToArray();
            }
        }

        private static TransformedData PerformBwtTransform(byte[] input, Span<int> frequency)
        {
            // Implement BWT transformation logic
            // This includes reordering the input based on the frequency table and sorting blocks

            // Return the transformed data and any additional metadata
            return new TransformedData
            {
                FirstChar = input[0], // Placeholder for first char
                Data = input          // Placeholder for transformed data
            };
        }

        private static void BuildFrequencyTable(byte[] input, Span<int> frequency)
        {
            // Count frequencies of each byte in the input
            foreach (var b in input)
            {
                frequency[b]++;
            }
        }

        // Data structure to hold transformed data and metadata
        private struct TransformedData
        {
            public byte FirstChar;
            public byte[] Data;
        }
    }
}
