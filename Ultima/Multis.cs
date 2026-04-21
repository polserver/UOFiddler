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

        private static MultiComponentList[] _uopComponents = new MultiComponentList[MaximumMultiIndex];
        private static bool _uopLoaded;

        public enum ImportType
        {
            TXT,
            UOA,
            UOAB,
            WSC,
            CSV, // Punt's multi tool csv format
            UOX3,
            MULTICACHE,
            UOADESIGN,
            XML
        }

        /// <summary>
        /// ReReads multi.mul
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex("Multi.idx", "Multi.mul", MaximumMultiIndex, 14);
            _components = new MultiComponentList[MaximumMultiIndex];
            ReloadUop();
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

        public static bool HasUopFile => !string.IsNullOrEmpty(Files.GetFilePath("multicollection.uop"));

        public static void ReloadUop()
        {
            _uopComponents = new MultiComponentList[MaximumMultiIndex];
            _uopLoaded = false;
        }

        public static MultiComponentList GetUopComponents(int index)
        {
            if (!_uopLoaded)
            {
                LoadUop();
            }

            if (index >= 0 && index < _uopComponents.Length)
            {
                return _uopComponents[index] ?? MultiComponentList.Empty;
            }

            return MultiComponentList.Empty;
        }

        private static void LoadUop()
        {
            _uopLoaded = true;

            string path = Files.GetFilePath("multicollection.uop");
            if (path == null)
            {
                return;
            }

            try
            {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new BinaryReader(fileStream);

                uint magic = reader.ReadUInt32();
                if (magic != 0x0050594D)
                {
                    return;
                }

                uint version = reader.ReadUInt32();
                if (version > 5)
                {
                    return;
                }

                reader.ReadUInt32(); // signature
                ulong nextTableOffset = reader.ReadUInt64();
                reader.ReadUInt32(); // block capacity
                reader.ReadUInt32(); // file count
                reader.ReadUInt32(); // reserved
                reader.ReadUInt32(); // reserved
                reader.ReadUInt32(); // reserved

                var entries = new List<(long dataOffset, uint compressedSize, uint decompressedSize)>();

                ulong next = nextTableOffset;
                while (next != 0)
                {
                    fileStream.Seek((long)next, SeekOrigin.Begin);
                    int count = reader.ReadInt32();
                    next = reader.ReadUInt64();

                    for (int i = 0; i < count; i++)
                    {
                        ulong dataOffset = reader.ReadUInt64();
                        uint headerSize = reader.ReadUInt32();
                        uint compressedSize = reader.ReadUInt32();
                        uint decompressedSize = reader.ReadUInt32();
                        reader.ReadUInt64(); // hash
                        reader.ReadUInt32(); // unknown
                        ushort flag = reader.ReadUInt16();

                        if (dataOffset == 0 || decompressedSize == 0)
                        {
                            continue;
                        }

                        if (flag == 0)
                        {
                            compressedSize = 0;
                        }

                        entries.Add(((long)(dataOffset + headerSize), compressedSize, decompressedSize));
                    }
                }

                foreach (var (dataOffset, compressedSize, decompressedSize) in entries)
                {
                    fileStream.Seek(dataOffset, SeekOrigin.Begin);

                    byte[] raw;
                    if (compressedSize > 0)
                    {
                        byte[] compressed = reader.ReadBytes((int)compressedSize);
                        (bool ok, byte[] decompressed) = UopUtils.Decompress(compressed);
                        if (!ok)
                        {
                            continue;
                        }

                        raw = decompressed;
                    }
                    else
                    {
                        raw = reader.ReadBytes((int)decompressedSize);
                    }

                    using var memoryStream = new MemoryStream(raw);
                    using var binaryReader = new BinaryReader(memoryStream);

                    uint multiId = binaryReader.ReadUInt32();
                    int componentCount = binaryReader.ReadInt32();

                    if (multiId >= MaximumMultiIndex || componentCount <= 0)
                    {
                        continue;
                    }

                    var tiles = new List<MultiComponentList.MultiTileEntry>(componentCount);
                    for (int j = 0; j < componentCount; j++)
                    {
                        ushort graphic = binaryReader.ReadUInt16();
                        ushort ux = binaryReader.ReadUInt16();
                        ushort uy = binaryReader.ReadUInt16();
                        ushort uz = binaryReader.ReadUInt16();
                        ushort uflags = binaryReader.ReadUInt16();
                        int clilocsCount = binaryReader.ReadInt32();

                        if (clilocsCount > 0)
                        {
                            binaryReader.BaseStream.Seek(clilocsCount * 4L, SeekOrigin.Current);
                        }

                        tiles.Add(new MultiComponentList.MultiTileEntry
                        {
                            ItemId = graphic,
                            OffsetX = (short)ux,
                            OffsetY = (short)uy,
                            OffsetZ = (short)uz,
                            Flags = uflags != 0 ? 0 : 1,
                            Unk1 = 0
                        });
                    }

                    if (tiles.Count > 0)
                    {
                        _uopComponents[multiId] = new MultiComponentList(tiles);
                    }
                }
            }
            catch
            {
                // leave array in its current partially-populated state
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