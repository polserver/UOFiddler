using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Ultima.Helpers;

namespace Ultima
{
    public sealed class Multis
    {
        public const int MaximumMultiIndex = 0x2200;

        private static MultiComponentList[] _components = new MultiComponentList[MaximumMultiIndex];
        private static FileIndex _fileIndex = new FileIndex("Multi.idx", "Multi.mul", MaximumMultiIndex, 14);

        public enum ImportType
        {
            TXT,
            UOA,
            UOAB,
            WSC,
            CSV, // Punt's multi tool csv format
            UOX3,
            MULTICACHE,
            UOADESIGN
        }

        /// <summary>
        /// ReReads multi.mul
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex("Multi.idx", "Multi.mul", MaximumMultiIndex, 14);
            _components = new MultiComponentList[MaximumMultiIndex];
        }

        /// <summary>
        /// Gets <see cref="MultiComponentList"/> of multi
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static MultiComponentList GetComponents(int index)
        {
            MultiComponentList mcl;

            if (index >= 0 && index < _components.Length)
            {
                mcl = _components[index];

                if (mcl == null)
                {
                    _components[index] = mcl = Load(index);
                }
            }
            else
            {
                mcl = MultiComponentList.Empty;
            }

            return mcl;
        }

        public static MultiComponentList Load(int index)
        {
            try
            {
                Stream stream = _fileIndex.Seek(index, out int length, out int _, out bool _);

                if (stream == null)
                {
                    return MultiComponentList.Empty;
                }

                if (Art.IsUOAHS())
                {
                    return new MultiComponentList(new BinaryReader(stream), length / 16, true);
                }
                else
                {
                    return new MultiComponentList(new BinaryReader(stream), length / 12, false);
                }
            }
            catch
            {
                return MultiComponentList.Empty;
            }
        }

        public static void Remove(int index)
        {
            _components[index] = MultiComponentList.Empty;
        }

        public static void Add(int index, MultiComponentList comp)
        {
            _components[index] = comp;
        }

        public static MultiComponentList ImportFromFile(int index, string fileName, ImportType type)
        {
            try
            {
                return _components[index] = new MultiComponentList(fileName, type);
            }
            catch
            {
                return _components[index] = MultiComponentList.Empty;
            }
        }

        public static MultiComponentList LoadFromFile(string fileName, ImportType type)
        {
            try
            {
                return new MultiComponentList(fileName, type);
            }
            catch
            {
                return MultiComponentList.Empty;
            }
        }

        public static List<MultiComponentList> LoadFromCache(string fileName)
        {
            var multiComponentLists = new List<MultiComponentList>();
            using (var ip = new StreamReader(fileName))
            {
                while (ip.ReadLine() is { } line)
                {
                    string[] split = Regex.Split(line, @"\s+");
                    if (split.Length != 7)
                    {
                        continue;
                    }

                    int count = Convert.ToInt32(split[2]);
                    multiComponentLists.Add(new MultiComponentList(ip, count));
                }
            }
            return multiComponentLists;
        }

        public static List<object[]> LoadFromDesigner(string fileName)
        {
            var multiList = new List<object[]>();

            string root = Path.GetFileNameWithoutExtension(fileName);
            string idx = $"{root}.idx";
            string bin = $"{root}.bin";

            if ((!File.Exists(idx)) || (!File.Exists(bin)))
            {
                return multiList;
            }

            using (var idxfs = new FileStream(idx, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var binfs = new FileStream(bin, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var idxbin = new BinaryReader(idxfs))
                using (var binbin = new BinaryReader(binfs))
                {
                    int count = idxbin.ReadInt32();
                    int version = idxbin.ReadInt32();

                    for (int i = 0; i < count; ++i)
                    {
                        var data = new object[2];

                        switch (version)
                        {
                            case 0:
                                data[0] = MultiHelpers.ReadUOAString(idxbin);
                                var arr = new List<MultiComponentList.MultiTileEntry>();
                                data[0] += "-" + MultiHelpers.ReadUOAString(idxbin);
                                data[0] += "-" + MultiHelpers.ReadUOAString(idxbin);

                                _ = idxbin.ReadInt32();
                                _ = idxbin.ReadInt32();
                                _ = idxbin.ReadInt32();
                                _ = idxbin.ReadInt32();

                                long filepos = idxbin.ReadInt64();
                                int reccount = idxbin.ReadInt32();

                                binbin.BaseStream.Seek(filepos, SeekOrigin.Begin);
                                for (int j = 0; j < reccount; ++j)
                                {
                                    int x;
                                    int y;
                                    int z;
                                    int index = x = y = z = 0;

                                    switch (binbin.ReadInt32())
                                    {
                                        case 0:
                                            index = binbin.ReadInt32();
                                            x = binbin.ReadInt32();
                                            y = binbin.ReadInt32();
                                            z = binbin.ReadInt32();
                                            binbin.ReadInt32();
                                            break;

                                        case 1:
                                            index = binbin.ReadInt32();
                                            x = binbin.ReadInt32();
                                            y = binbin.ReadInt32();
                                            z = binbin.ReadInt32();
                                            binbin.ReadInt32();
                                            binbin.ReadInt32();
                                            break;
                                    }

                                    var tempItem =
                                        new MultiComponentList.MultiTileEntry
                                        {
                                            ItemId = (ushort)index,
                                            Flags = 1,
                                            OffsetX = (short)x,
                                            OffsetY = (short)y,
                                            OffsetZ = (short)z,
                                            Unk1 = 0
                                        };
                                    arr.Add(tempItem);
                                }

                                data[1] = new MultiComponentList(arr);
                                break;
                        }

                        multiList.Add(data);
                    }
                }

                return multiList;
            }
        }

        private static List<MultiComponentList.MultiTileEntry> RebuildTiles(MultiComponentList.MultiTileEntry[] tiles)
        {
            var newTiles = new List<MultiComponentList.MultiTileEntry>();
            newTiles.AddRange(tiles);

            if (newTiles[0].OffsetX == 0 && newTiles[0].OffsetY == 0 && newTiles[0].OffsetZ == 0) // found a center item
            {
                if (newTiles[0].ItemId != 0x1) // its a "good" one
                {
                    for (int j = newTiles.Count - 1; j >= 0; --j) // remove all invis items
                    {
                        if (newTiles[j].ItemId == 0x1)
                        {
                            newTiles.RemoveAt(j);
                        }
                    }
                    return newTiles;
                }
                else // a bad one
                {
                    for (int i = 1; i < newTiles.Count; ++i) // do we have a better one?
                    {
                        if (newTiles[i].OffsetX != 0 || newTiles[i].OffsetY != 0 || newTiles[i].ItemId == 0x1 ||
                            newTiles[i].OffsetZ != 0)
                        {
                            continue;
                        }

                        MultiComponentList.MultiTileEntry centerItem = newTiles[i];
                        newTiles.RemoveAt(i); // jep so save it

                        for (int j = newTiles.Count-1; j >= 0; --j) // and remove all invis
                        {
                            if (newTiles[j].ItemId == 0x1)
                            {
                                newTiles.RemoveAt(j);
                            }
                        }

                        newTiles.Insert(0, centerItem);

                        return newTiles;
                    }

                    for (int j = newTiles.Count-1; j >= 1; --j) // nothing found so remove all invis except the first
                    {
                        if (newTiles[j].ItemId == 0x1)
                        {
                            newTiles.RemoveAt(j);
                        }
                    }

                    return newTiles;
                }
            }

            for (int i = 0; i < newTiles.Count; ++i) // is there a good one
            {
                if (newTiles[i].OffsetX != 0 || newTiles[i].OffsetY != 0 || newTiles[i].ItemId == 0x1 ||
                    newTiles[i].OffsetZ != 0)
                {
                    continue;
                }

                MultiComponentList.MultiTileEntry centerItem = newTiles[i];
                newTiles.RemoveAt(i); // store it
                for (int j = newTiles.Count-1; j >= 0; --j) // remove all invis
                {
                    if (newTiles[j].ItemId == 0x1)
                    {
                        newTiles.RemoveAt(j);
                    }
                }

                newTiles.Insert(0, centerItem);

                return newTiles;
            }

            for (int j = newTiles.Count-1; j >= 0; --j) // nothing found so remove all invis
            {
                if (newTiles[j].ItemId == 0x1)
                {
                    newTiles.RemoveAt(j);
                }
            }

            // and create a new invis
            var invisItem =
                new MultiComponentList.MultiTileEntry
                {
                    ItemId = 0x1,
                    OffsetX = 0,
                    OffsetY = 0,
                    OffsetZ = 0,
                    Flags = 0,
                    Unk1 = 0
                };

            newTiles.Insert(0, invisItem);

            return newTiles;
        }

        public static void Save(string path)
        {
            bool isUOAHS = Art.IsUOAHS();

            string idx = Path.Combine(path, "multi.idx");
            string mul = Path.Combine(path, "multi.mul");

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var binidx = new BinaryWriter(fsidx))
            using (var binmul = new BinaryWriter(fsmul))
            {
                for (int index = 0; index < MaximumMultiIndex; ++index)
                {
                    MultiComponentList comp = GetComponents(index);

                    if (comp == MultiComponentList.Empty)
                    {
                        binidx.Write(-1); // lookup
                        binidx.Write(-1); // length
                        binidx.Write(-1); // extra
                    }
                    else
                    {
                        List<MultiComponentList.MultiTileEntry> tiles = RebuildTiles(comp.SortedTiles);
                        binidx.Write((int)fsmul.Position); // lookup
                        if (isUOAHS)
                        {
                            binidx.Write(tiles.Count * 16); // length
                        }
                        else
                        {
                            binidx.Write(tiles.Count * 12); // length
                        }

                        binidx.Write(-1); // extra
                        for (int i = 0; i < tiles.Count; ++i)
                        {
                            binmul.Write(tiles[i].ItemId);
                            binmul.Write(tiles[i].OffsetX);
                            binmul.Write(tiles[i].OffsetY);
                            binmul.Write(tiles[i].OffsetZ);
                            binmul.Write(tiles[i].Flags);
                            if (isUOAHS)
                            {
                                binmul.Write(tiles[i].Unk1);
                            }
                        }
                    }
                }
            }
        }
    }
}