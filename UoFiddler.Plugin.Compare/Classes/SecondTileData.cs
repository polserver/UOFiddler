using System;
using System.IO;
using System.Runtime.InteropServices;
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
                LandTable = new LandData[0x4000];

                var buffer = new byte[fs.Length];
                GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                long currentPos = 0;
                try
                {
                    fs.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < 0x4000; i += 32)
                    {
                        var ptrHeader = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                        currentPos += 4;
                        landHeader[j++] = (int)Marshal.PtrToStructure(ptrHeader, typeof(int));
                        for (int count = 0; count < 32; ++count)
                        {
                            var ptr = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                            if (useNeWTileDataFormat)
                            {
                                currentPos += sizeof(NewLandTileDataMul);
                                var cur = (NewLandTileDataMul)Marshal.PtrToStructure(ptr, typeof(NewLandTileDataMul));
                                LandTable[i + count] = new LandData(cur);
                            }
                            else
                            {
                                currentPos += sizeof(OldLandTileDataMul);
                                var cur = (OldLandTileDataMul)Marshal.PtrToStructure(ptr, typeof(OldLandTileDataMul));
                                LandTable[i + count] = new LandData(cur);
                            }
                        }
                    }

                    long remaining = buffer.Length - currentPos;

                    int structSize = useNeWTileDataFormat ? sizeof(NewItemTileDataMul) : sizeof(OldItemTileDataMul);

                    var itemHeader = new int[remaining / ((structSize * 32) + 4)];
                    int itemLength = itemHeader.Length * 32;

                    ItemTable = new ItemData[itemLength];
                    HeightTable = new int[itemLength];

                    j = 0;
                    for (int i = 0; i < itemLength; i += 32)
                    {
                        var ptrHeader = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                        currentPos += 4;
                        itemHeader[j++] = (int)Marshal.PtrToStructure(ptrHeader, typeof(int));
                        for (int count = 0; count < 32; ++count)
                        {
                            var ptr = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                            if (useNeWTileDataFormat)
                            {
                                currentPos += sizeof(NewItemTileDataMul);
                                var cur = (NewItemTileDataMul)Marshal.PtrToStructure(ptr, typeof(NewItemTileDataMul));
                                ItemTable[i + count] = new ItemData(cur);
                                HeightTable[i + count] = cur.height;
                            }
                            else
                            {
                                currentPos += sizeof(OldItemTileDataMul);
                                var cur = (OldItemTileDataMul)Marshal.PtrToStructure(ptr, typeof(OldItemTileDataMul));
                                ItemTable[i + count] = new ItemData(cur);
                                HeightTable[i + count] = cur.height;
                            }
                        }
                    }
                }
                finally
                {
                    gc.Free();
                }
            }
        }
    }
}