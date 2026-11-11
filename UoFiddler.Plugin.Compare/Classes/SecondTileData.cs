using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    /// <summary>
    /// Contains lists of <see cref="LandData">land</see> and <see cref="ItemData">item</see> tile data.
    /// <seealso cref="LandData" />
    /// <seealso cref="ItemData" />
    /// </summary>
    public class SecondTileData
    {
        /// <summary>
        /// Gets the list of <see cref="LandData">land tile data</see>.
        /// </summary>
        public LandData[] LandTable { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="ItemData">item tile data</see>.
        /// </summary>
        public ItemData[] ItemTable { get; private set; }

        public int[] HeightTable { get; private set; }

        public unsafe void Initialize(string path, bool useNeWTileDataFormat)
        {
            string filePath = path;
            if (filePath == null)
            {
                return;
            }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var landHeader = new int[512];
                int j = 0;
                var landTable = new LandData[0x4000];

                var buffer = new byte[fs.Length];
                fs.ReadExactly(buffer, 0, buffer.Length);
                int currentPos = 0;

                int landStructSize = useNeWTileDataFormat ? sizeof(NewLandTileDataMul) : sizeof(OldLandTileDataMul);

                // ReadUnaligned isn't bounds-checked, so reject a file shorter than the land section.
                long landSectionSize = 512L * (4 + (32L * landStructSize));
                if (buffer.Length < landSectionSize)
                {
                    throw new InvalidDataException(
                        $"tiledata.mul is {buffer.Length} bytes, shorter than its {landSectionSize}-byte land section.");
                }

                for (int i = 0; i < 0x4000; i += 32)
                {
                    landHeader[j++] = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(currentPos));
                    currentPos += 4;
                    for (int count = 0; count < 32; ++count)
                    {
                        if (useNeWTileDataFormat)
                        {
                            var cur = Unsafe.ReadUnaligned<NewLandTileDataMul>(ref buffer[currentPos]);
                            landTable[i + count] = new LandData(cur);
                        }
                        else
                        {
                            var cur = Unsafe.ReadUnaligned<OldLandTileDataMul>(ref buffer[currentPos]);
                            landTable[i + count] = new LandData(cur);
                        }
                        currentPos += landStructSize;
                    }
                }

                long remaining = buffer.Length - currentPos;

                int structSize = useNeWTileDataFormat ? sizeof(NewItemTileDataMul) : sizeof(OldItemTileDataMul);

                var itemHeader = new int[remaining / ((structSize * 32) + 4)];
                int itemLength = itemHeader.Length * 32;

                var itemTable = new ItemData[itemLength];
                var heightTable = new int[itemLength];

                j = 0;
                for (int i = 0; i < itemLength; i += 32)
                {
                    itemHeader[j++] = BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(currentPos));
                    currentPos += 4;
                    for (int count = 0; count < 32; ++count)
                    {
                        if (useNeWTileDataFormat)
                        {
                            var cur = Unsafe.ReadUnaligned<NewItemTileDataMul>(ref buffer[currentPos]);
                            itemTable[i + count] = new ItemData(cur);
                            heightTable[i + count] = cur.height;
                        }
                        else
                        {
                            var cur = Unsafe.ReadUnaligned<OldItemTileDataMul>(ref buffer[currentPos]);
                            itemTable[i + count] = new ItemData(cur);
                            heightTable[i + count] = cur.height;
                        }
                        currentPos += structSize;
                    }
                }

                // Publish only after the whole file has parsed.
                LandTable = landTable;
                ItemTable = itemTable;
                HeightTable = heightTable;
            }
        }
    }
}
