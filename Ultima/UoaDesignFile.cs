using System;
using System.Collections.Generic;
using System.IO;

namespace Ultima
{
    /// <summary>
    /// One design in a UO Architect file: what its header says, and where its items are.
    /// </summary>
    public sealed class UoaDesign
    {
        public string Name { get; internal set; }

        public string Category { get; internal set; }

        public string Subsection { get; internal set; }

        /// <summary>Size as UO Architect stored it; not always the extent of the items.</summary>
        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public int ItemCount { get; internal set; }

        /// <summary>The .uoa file, or the .idx of an idx + bin design set.</summary>
        public string FileName { get; internal set; }

        /// <summary>Position of the design within its file, from 0.</summary>
        public int Index { get; internal set; }

        /// <summary>Where the item records start: in the .uoa itself, or in the .bin of a design set.</summary>
        internal long ItemsPosition { get; set; }

        internal bool InDesignSet { get; set; }
    }

    /// <summary>
    /// Reads and writes UO Architect designs. Layouts as in UO Architect 2.7:
    /// <list type="bullet">
    /// <item>.uoa version 1: one design. Version 2: a count, then that many designs.</item>
    /// <item>A design: name, category, subsection (each a flag byte, then a length-prefixed string
    /// when the flag is 1), width, height, user width, user height, item count (int32 each), then
    /// per item six int16: id, x, y, z, level, hue.</item>
    /// <item>Design set (Designs.idx + Designs.bin): the idx holds a count, a version (0) and per
    /// design the same header followed by the int64 position and int32 count of its records in
    /// the bin; a bin record is int32 type (0 or 1), id, x, y, z, level, and hue when type is 1.</item>
    /// </list>
    /// A multi has no item hue or level, so reading drops them and writing stores 0.
    /// </summary>
    public static class UoaDesignFile
    {
        private const int _itemSize = 12;

        /// <summary>
        /// The designs a file holds, without reading their items. Takes a .uoa (either version) or
        /// the .idx of a design set. An unreadable or foreign file gives an empty list.
        /// </summary>
        public static List<UoaDesign> ReadHeaders(string fileName)
        {
            var designs = new List<UoaDesign>();
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new BinaryReader(fs))
                {
                    if (fileName.EndsWith(".idx", StringComparison.OrdinalIgnoreCase))
                    {
                        int count = reader.ReadInt32();
                        if (reader.ReadInt32() != 0 || count < 0)
                        {
                            return designs;
                        }

                        for (int i = 0; i < count; ++i)
                        {
                            UoaDesign design = ReadHeader(reader, fileName, i, countFollows: false);
                            design.ItemsPosition = reader.ReadInt64();
                            design.ItemCount = reader.ReadInt32();
                            design.InDesignSet = true;
                            designs.Add(design);
                        }

                        return designs;
                    }

                    short version = reader.ReadInt16();
                    int total = version == 1 ? 1 : version == 2 ? reader.ReadInt16() : 0;
                    for (int i = 0; i < total; ++i)
                    {
                        UoaDesign design = ReadHeader(reader, fileName, i, countFollows: true);
                        design.ItemsPosition = fs.Position;
                        fs.Seek((long)design.ItemCount * _itemSize, SeekOrigin.Current);
                        if (fs.Position > fs.Length)
                        {
                            break; // cut short: what was read so far stands
                        }

                        designs.Add(design);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                // What was read before the file ran out is returned.
            }

            return designs;
        }

        private static UoaDesign ReadHeader(BinaryReader reader, string fileName, int index, bool countFollows)
        {
            var design = new UoaDesign
            {
                FileName = fileName,
                Index = index,
                Name = ReadString(reader),
                Category = ReadString(reader),
                Subsection = ReadString(reader),
                Width = reader.ReadInt32(),
                Height = reader.ReadInt32()
            };
            reader.ReadInt32(); // user width
            reader.ReadInt32(); // user height
            if (countFollows)
            {
                design.ItemCount = reader.ReadInt32();
                if (design.ItemCount < 0)
                {
                    throw new IOException("Negative item count");
                }
            }

            return design;
        }

        /// <summary>The items of a design as a multi. Hue and level are dropped; Z is kept within a multi's range.</summary>
        public static MultiComponentList Load(UoaDesign design)
        {
            var entries = new List<MultiComponentList.MultiTileEntry>(Math.Max(0, design.ItemCount));
            string path = design.InDesignSet ? Path.ChangeExtension(design.FileName, ".bin") : design.FileName;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(fs))
            {
                fs.Seek(design.ItemsPosition, SeekOrigin.Begin);
                for (int i = 0; i < design.ItemCount; ++i)
                {
                    int id, x, y, z;
                    if (design.InDesignSet)
                    {
                        int type = reader.ReadInt32();
                        id = reader.ReadInt32();
                        x = reader.ReadInt32();
                        y = reader.ReadInt32();
                        z = reader.ReadInt32();
                        reader.ReadInt32(); // level
                        if (type == 1)
                        {
                            reader.ReadInt32(); // hue
                        }
                    }
                    else
                    {
                        id = (ushort)reader.ReadInt16();
                        x = reader.ReadInt16();
                        y = reader.ReadInt16();
                        z = reader.ReadInt16();
                        reader.ReadInt16(); // level
                        reader.ReadInt16(); // hue
                    }

                    entries.Add(new MultiComponentList.MultiTileEntry
                    {
                        ItemId = (ushort)id,
                        OffsetX = (short)x,
                        OffsetY = (short)y,
                        OffsetZ = (short)Math.Clamp(z, sbyte.MinValue, sbyte.MaxValue),
                        Flags = 1,
                        Unk1 = 0
                    });
                }
            }

            return entries.Count == 0 ? MultiComponentList.Empty : new MultiComponentList(entries);
        }

        /// <summary>
        /// Writes designs as a .uoa: version 1 for one design, version 2 (a batch) for several.
        /// </summary>
        public static void Write(string fileName, IReadOnlyList<(string Name, string Category, string Subsection, MultiComponentList Multi)> designs)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                if (designs.Count == 1)
                {
                    writer.Write((short)1);
                }
                else
                {
                    writer.Write((short)2);
                    writer.Write((short)designs.Count);
                }

                foreach ((string name, string category, string subsection, MultiComponentList multi) in designs)
                {
                    WriteString(writer, name);
                    WriteString(writer, category);
                    WriteString(writer, subsection);
                    writer.Write(multi.Width);
                    writer.Write(multi.Height);
                    writer.Write(multi.Width);  // user width
                    writer.Write(multi.Height); // user height
                    writer.Write(multi.SortedTiles.Length);
                    foreach (MultiComponentList.MultiTileEntry tile in multi.SortedTiles)
                    {
                        writer.Write((short)tile.ItemId);
                        writer.Write(tile.OffsetX);
                        writer.Write(tile.OffsetY);
                        writer.Write(tile.OffsetZ);
                        writer.Write((short)0); // level
                        writer.Write((short)0); // hue
                    }
                }
            }
        }

        internal static string ReadString(BinaryReader reader)
        {
            return reader.ReadByte() == 0 ? null : reader.ReadString();
        }

        internal static void WriteString(BinaryWriter writer, string value)
        {
            writer.Write((byte)(value == null ? 0 : 1));
            if (value != null)
            {
                writer.Write(value);
            }
        }
    }
}
