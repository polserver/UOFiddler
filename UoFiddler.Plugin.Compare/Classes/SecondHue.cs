using System;
using System.IO;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondHue
    {
        public static Hue[] List { get; private set; }

        /// <summary>
        /// Reads hues.mul and fills <see cref="List"/>
        /// </summary>
        public static void Initialize(string path)
        {
            int index = 0;

            List = new Hue[3000];

            if (path != null)
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int blockCount = (int)fs.Length / 708;

                    if (blockCount > 375)
                    {
                        blockCount = 375;
                    }

                    // Disk layout per HueDataMul: 32 ushorts (64) + 2 ushorts (4) + 20-byte name = 88 bytes.
                    // Each block = 4-byte header + 8 * 88 = 708 bytes.
                    const int hueDataSize = 88;
                    const int blockSize = 4 + 8 * hueDataSize;
                    var buffer = new byte[blockCount * blockSize];
                    fs.ReadExactly(buffer, 0, buffer.Length);
                    ReadOnlySpan<byte> bufferSpan = buffer;

                    int cursor = 0;
                    for (int i = 0; i < blockCount; ++i)
                    {
                        // 4-byte header per block is unused on the Compare side.
                        cursor += 4;

                        for (int j = 0; j < 8; ++j, ++index)
                        {
                            List[index] = new Hue(index, bufferSpan.Slice(cursor, hueDataSize));
                            cursor += hueDataSize;
                        }
                    }
                }
            }

            for (; index < List.Length; ++index)
            {
                List[index] = new Hue(index);
            }
        }

        // TODO: unused method?
        // public static Hue GetHue(int index)
        // {
        //     index &= 0x3FFF;
        //
        //     if (index >= 0 && index < 3000)
        //     {
        //         return List[index];
        //     }
        //
        //     return List[0];
        // }
    }
}
