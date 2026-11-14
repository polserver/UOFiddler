// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Ultima.Helpers;

namespace Ultima
{
    public sealed class MultiComponentList
    {
        private readonly Point _min;
        private readonly Point _max;

        private Point _center;

        public static readonly MultiComponentList Empty = new MultiComponentList();

        public Point Min { get { return _min; } }
        public Point Max { get { return _max; } }
        public Point Center { get { return _center; } }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public MTile[][][] Tiles { get; private set; }
        public int MaxHeight { get; }
        public MultiTileEntry[] SortedTiles { get; }
        public int Surface { get; private set; }

        public static HashSet<ushort> DynamicItemIds { get; set; }

        public struct MultiTileEntry
        {
            public ushort ItemId;
            public short OffsetX;
            public short OffsetY;
            public short OffsetZ;
            public int Flags;
            public int Unk1;
        }

        /// <summary>
        /// Returns Bitmap of Multi to maximumHeight
        /// </summary>
        /// <param name="maximumHeight"></param>
        /// <returns></returns>
        public Bitmap GetImage(int maximumHeight = 300)
        {
            if (Width == 0 || Height == 0)
            {
                return null;
            }

            int xMin = 1000, yMin = 1000;
            int xMax = -1000, yMax = -1000;

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    foreach (var mTile in Tiles[x][y])
                    {
                        Bitmap bmp = Art.GetStatic(mTile.Id);

                        if (bmp == null)
                        {
                            continue;
                        }

                        int px = (x - y) * 22;
                        int py = (x + y) * 22;

                        px -= (bmp.Width / 2);
                        py -= mTile.Z << 2;
                        py -= bmp.Height;

                        if (px < xMin)
                        {
                            xMin = px;
                        }

                        if (py < yMin)
                        {
                            yMin = py;
                        }

                        px += bmp.Width;
                        py += bmp.Height;

                        if (px > xMax)
                        {
                            xMax = px;
                        }

                        if (py > yMax)
                        {
                            yMax = py;
                        }
                    }
                }
            }

            var canvas = new Bitmap(xMax - xMin, yMax - yMin);
            Graphics gfx = Graphics.FromImage(canvas);
            gfx.Clear(Color.Transparent);

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    foreach (var mTile in Tiles[x][y])
                    {
                        Bitmap bmp = Art.GetStatic(mTile.Id);

                        if (bmp == null)
                        {
                            continue;
                        }

                        if (mTile.Z > maximumHeight)
                        {
                            continue;
                        }

                        int px = (x - y) * 22;
                        int py = (x + y) * 22;

                        px -= (bmp.Width / 2);
                        py -= mTile.Z << 2;
                        py -= bmp.Height;
                        px -= xMin;
                        py -= yMin;

                        gfx.DrawImageUnscaled(bmp, px, py, bmp.Width, bmp.Height);
                    }
                }
            }

            gfx.Dispose();

            return canvas;
        }

        public MultiComponentList(BinaryReader reader, int count, bool useNewMultiFormat)
        {
            _min = _max = Point.Empty;

            SortedTiles = new MultiTileEntry[count];

            for (int i = 0; i < count; ++i)
            {
                SortedTiles[i].ItemId = Art.GetLegalItemId(reader.ReadUInt16());
                SortedTiles[i].OffsetX = reader.ReadInt16();
                SortedTiles[i].OffsetY = reader.ReadInt16();
                SortedTiles[i].OffsetZ = reader.ReadInt16();
                SortedTiles[i].Flags = reader.ReadInt32();
                SortedTiles[i].Unk1 = useNewMultiFormat ? reader.ReadInt32() : 0;

                MultiTileEntry e = SortedTiles[i];

                if (e.OffsetX < _min.X)
                {
                    _min.X = e.OffsetX;
                }

                if (e.OffsetY < _min.Y)
                {
                    _min.Y = e.OffsetY;
                }

                if (e.OffsetX > _max.X)
                {
                    _max.X = e.OffsetX;
                }

                if (e.OffsetY > _max.Y)
                {
                    _max.Y = e.OffsetY;
                }

                if (e.OffsetZ > MaxHeight)
                {
                    MaxHeight = e.OffsetZ;
                }
            }
            ConvertList();
        }

        public MultiComponentList(string fileName, Multis.ImportType type)
        {
            _min = _max = Point.Empty;

            int itemCount;
            List<MultiTileEntry> parsed = null;

            switch (type)
            {
                case Multis.ImportType.TXT:
                    {
                        itemCount = 0;
                        using (var ip = new StreamReader(fileName))
                        {
                            while (ip.ReadLine() != null)
                            {
                                itemCount++;
                            }
                        }
                        SortedTiles = new MultiTileEntry[itemCount];
                        itemCount = 0;
                        _min.X = 10000;
                        _min.Y = 10000;
                        using (var ip = new StreamReader(fileName))
                        {
                            while (ip.ReadLine() is { } line)
                            {
                                string[] split = line.Split(' ');

                                string tmp = split[0];
                                tmp = tmp.Replace("0x", "");

                                SortedTiles[itemCount].ItemId = ushort.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                                SortedTiles[itemCount].OffsetX = Convert.ToInt16(split[1]);
                                SortedTiles[itemCount].OffsetY = Convert.ToInt16(split[2]);
                                SortedTiles[itemCount].OffsetZ = Convert.ToInt16(split[3]);
                                SortedTiles[itemCount].Flags = Convert.ToInt32(split[4]);
                                SortedTiles[itemCount].Unk1 = 0;

                                MultiTileEntry e = SortedTiles[itemCount];

                                if (e.OffsetX < _min.X)
                                {
                                    _min.X = e.OffsetX;
                                }

                                if (e.OffsetY < _min.Y)
                                {
                                    _min.Y = e.OffsetY;
                                }

                                if (e.OffsetX > _max.X)
                                {
                                    _max.X = e.OffsetX;
                                }

                                if (e.OffsetY > _max.Y)
                                {
                                    _max.Y = e.OffsetY;
                                }

                                if (e.OffsetZ > MaxHeight)
                                {
                                    MaxHeight = e.OffsetZ;
                                }

                                itemCount++;
                            }
                        }
                        break;
                    }
                case Multis.ImportType.UOA:
                    {
                        itemCount = 0;

                        using (var ip = new StreamReader(fileName))
                        {
                            while (ip.ReadLine() is { } line)
                            {
                                ++itemCount;

                                if (itemCount != 4)
                                {
                                    continue;
                                }

                                string[] split = line.Split(' ');
                                itemCount = Convert.ToInt32(split[0]);
                                break;
                            }
                        }
                        SortedTiles = new MultiTileEntry[itemCount];
                        itemCount = 0;
                        _min.X = 10000;
                        _min.Y = 10000;
                        using (var ip = new StreamReader(fileName))
                        {
                            int i = -1;
                            while (ip.ReadLine() is { } line)
                            {
                                ++i;
                                if (i < 4)
                                {
                                    continue;
                                }

                                string[] split = line.Split(' ');

                                SortedTiles[itemCount].ItemId = Convert.ToUInt16(split[0]);
                                SortedTiles[itemCount].OffsetX = Convert.ToInt16(split[1]);
                                SortedTiles[itemCount].OffsetY = Convert.ToInt16(split[2]);
                                SortedTiles[itemCount].OffsetZ = Convert.ToInt16(split[3]);
                                SortedTiles[itemCount].Flags = Convert.ToInt32(split[4]);
                                SortedTiles[itemCount].Unk1 = 0;

                                MultiTileEntry e = SortedTiles[itemCount];

                                if (e.OffsetX < _min.X)
                                {
                                    _min.X = e.OffsetX;
                                }

                                if (e.OffsetY < _min.Y)
                                {
                                    _min.Y = e.OffsetY;
                                }

                                if (e.OffsetX > _max.X)
                                {
                                    _max.X = e.OffsetX;
                                }

                                if (e.OffsetY > _max.Y)
                                {
                                    _max.Y = e.OffsetY;
                                }

                                if (e.OffsetZ > MaxHeight)
                                {
                                    MaxHeight = e.OffsetZ;
                                }

                                ++itemCount;
                            }
                        }

                        break;
                    }
                case Multis.ImportType.UOAB:
                    {
                        using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var reader = new BinaryReader(fs))
                        {
                            // Version 1 holds one design; version 2 is a batch, of which the first
                            // design is read here (UoaDesignFile reads them all).
                            short uoaVersion = reader.ReadInt16();
                            if (uoaVersion == 2)
                            {
                                if (reader.ReadInt16() < 1)
                                {
                                    return;
                                }
                            }
                            else if (uoaVersion != 1)
                            {
                                return;
                            }

                            _ = MultiHelpers.ReadUOAString(reader);
                            _ = MultiHelpers.ReadUOAString(reader); // Category
                            _ = MultiHelpers.ReadUOAString(reader); // Subsection

                            _ = reader.ReadInt32();
                            _ = reader.ReadInt32();
                            _ = reader.ReadInt32();
                            _ = reader.ReadInt32();

                            int count = reader.ReadInt32();
                            itemCount = count;
                            SortedTiles = new MultiTileEntry[itemCount];
                            itemCount = 0;
                            _min.X = 10000;
                            _min.Y = 10000;
                            for (; itemCount < count; ++itemCount)
                            {
                                SortedTiles[itemCount].ItemId = (ushort)reader.ReadInt16();
                                SortedTiles[itemCount].OffsetX = reader.ReadInt16();
                                SortedTiles[itemCount].OffsetY = reader.ReadInt16();
                                SortedTiles[itemCount].OffsetZ = reader.ReadInt16();
                                reader.ReadInt16(); // level
                                SortedTiles[itemCount].Flags = 1;
                                reader.ReadInt16(); // hue
                                SortedTiles[itemCount].Unk1 = 0;

                                MultiTileEntry e = SortedTiles[itemCount];

                                if (e.OffsetX < _min.X)
                                {
                                    _min.X = e.OffsetX;
                                }

                                if (e.OffsetY < _min.Y)
                                {
                                    _min.Y = e.OffsetY;
                                }

                                if (e.OffsetX > _max.X)
                                {
                                    _max.X = e.OffsetX;
                                }

                                if (e.OffsetY > _max.Y)
                                {
                                    _max.Y = e.OffsetY;
                                }

                                if (e.OffsetZ > MaxHeight)
                                {
                                    MaxHeight = e.OffsetZ;
                                }
                            }
                        }
                        break;
                    }
                case Multis.ImportType.WSC:
                    {
                        itemCount = 0;
                        using (var ip = new StreamReader(fileName))
                        {
                            while (ip.ReadLine() is { } line)
                            {
                                line = line.Trim();
                                if (line.StartsWith("SECTION WORLDITEM"))
                                {
                                    ++itemCount;
                                }
                            }
                        }
                        SortedTiles = new MultiTileEntry[itemCount];
                        itemCount = 0;
                        _min.X = 10000;
                        _min.Y = 10000;
                        using (var ip = new StreamReader(fileName))
                        {
                            var tempItem = new MultiTileEntry
                            {
                                ItemId = 0xFFFF,
                                Flags = 1,
                                Unk1 = 0
                            };

                            while (ip.ReadLine() is { } line)
                            {
                                line = line.Trim();
                                if (line.StartsWith("SECTION WORLDITEM"))
                                {
                                    if (tempItem.ItemId != 0xFFFF)
                                    {
                                        if (DynamicItemIds != null && DynamicItemIds.Contains(tempItem.ItemId))
                                        {
                                            tempItem.Flags = 0;
                                        }

                                        SortedTiles[itemCount] = tempItem;
                                        ++itemCount;
                                    }

                                    tempItem.ItemId = 0xFFFF;
                                    tempItem.Flags = 1;
                                }
                                else if (line.StartsWith("ID"))
                                {
                                    line = line.Remove(0, 2);
                                    line = line.Trim();
                                    tempItem.ItemId = Convert.ToUInt16(line);
                                }
                                else if (line.StartsWith("X"))
                                {
                                    line = line.Remove(0, 1);
                                    line = line.Trim();
                                    tempItem.OffsetX = Convert.ToInt16(line);
                                    if (tempItem.OffsetX < _min.X)
                                    {
                                        _min.X = tempItem.OffsetX;
                                    }

                                    if (tempItem.OffsetX > _max.X)
                                    {
                                        _max.X = tempItem.OffsetX;
                                    }
                                }
                                else if (line.StartsWith("Y"))
                                {
                                    line = line.Remove(0, 1);
                                    line = line.Trim();
                                    tempItem.OffsetY = Convert.ToInt16(line);
                                    if (tempItem.OffsetY < _min.Y)
                                    {
                                        _min.Y = tempItem.OffsetY;
                                    }

                                    if (tempItem.OffsetY > _max.Y)
                                    {
                                        _max.Y = tempItem.OffsetY;
                                    }
                                }
                                else if (line.StartsWith("Z"))
                                {
                                    line = line.Remove(0, 1);
                                    line = line.Trim();
                                    tempItem.OffsetZ = Convert.ToInt16(line);
                                    if (tempItem.OffsetZ > MaxHeight)
                                    {
                                        MaxHeight = tempItem.OffsetZ;
                                    }
                                }
                            }

                            if (tempItem.ItemId != 0xFFFF)
                            {
                                if (DynamicItemIds?.Contains(tempItem.ItemId) == true)
                                {
                                    tempItem.Flags = 0;
                                }

                                SortedTiles[itemCount] = tempItem;
                            }
                        }

                        // WSC files from a live UO world use absolute world coordinates.
                        // Tool-generated WSC files may already have relative offsets (possibly negative).
                        // Heuristic: if both min coords are positive AND each exceeds the multi's own
                        // extent, the file contains world coordinates and must be normalized.
                        int extentX = _max.X - _min.X;
                        int extentY = _max.Y - _min.Y;
                        if (_min.X > 0 && _min.Y > 0 && _min.X > extentX && _min.Y > extentY)
                        {
                            for (int i = 0; i < SortedTiles.Length; ++i)
                            {
                                SortedTiles[i].OffsetX = (short)(SortedTiles[i].OffsetX - _min.X);
                                SortedTiles[i].OffsetY = (short)(SortedTiles[i].OffsetY - _min.Y);
                            }
                            _max.X -= _min.X;
                            _max.Y -= _min.Y;
                            _min.X = 0;
                            _min.Y = 0;
                        }

                        break;
                    }
                case Multis.ImportType.CSV: 
                    {
                        const string headerCheck = "TileID,OffsetX";

                        itemCount = 0;

                        using (var ip = new StreamReader(fileName))
                        {
                            while (ip.ReadLine() is { } line)
                            {
                                line = line.Trim();

                                if (!line.StartsWith(headerCheck))
                                {
                                    ++itemCount;
                                }
                            }
                        }

                        SortedTiles = new MultiTileEntry[itemCount];

                        itemCount = 0;

                        _min.X = 10000;
                        _min.Y = 10000;

                        using (var ip = new StreamReader(fileName))
                        {
                            while (ip.ReadLine() is { } line)
                            {
                                if (line.StartsWith(headerCheck))
                                {
                                    continue;
                                }

                                string[] split = line.Split(',');

                                string tmp = split[0];
                                tmp = tmp.Replace("0x", "");

                                SortedTiles[itemCount].ItemId = ushort.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                                SortedTiles[itemCount].OffsetX = Convert.ToInt16(split[1]);
                                SortedTiles[itemCount].OffsetY = Convert.ToInt16(split[2]);
                                SortedTiles[itemCount].OffsetZ = Convert.ToInt16(split[3]);

                                tmp = split[4];
                                tmp = tmp.Replace("0x", "");

                                SortedTiles[itemCount].Flags = int.Parse(tmp, System.Globalization.NumberStyles.HexNumber);

                                // Column 5 carries the High Seas trailing int32; older exports left it empty.
                                SortedTiles[itemCount].Unk1 = split.Length > 5 ? ParseCsvUnk1(split[5]) : 0;

                                MultiTileEntry e = SortedTiles[itemCount];

                                if (e.OffsetX < _min.X)
                                {
                                    _min.X = e.OffsetX;
                                }

                                if (e.OffsetY < _min.Y)
                                {
                                    _min.Y = e.OffsetY;
                                }

                                if (e.OffsetX > _max.X)
                                {
                                    _max.X = e.OffsetX;
                                }

                                if (e.OffsetY > _max.Y)
                                {
                                    _max.Y = e.OffsetY;
                                }

                                if (e.OffsetZ > MaxHeight)
                                {
                                    MaxHeight = e.OffsetZ;
                                }

                                itemCount++;
                            }
                        }

                        break;
                    }
                case Multis.ImportType.UOX3:
                    parsed = ReadUox3(fileName);
                    break;
                case Multis.ImportType.XML:
                    parsed = ReadXml(fileName);
                    break;
                case Multis.ImportType.UOH:
                    parsed = ReadUoh(fileName);
                    break;
                case Multis.ImportType.WORLDFORGE:
                    parsed = ReadWorldForge(fileName);
                    break;
            }

            if (parsed != null)
            {
                // Formats read into a list: take the tiles and their bounds from it.
                SortedTiles = parsed.ToArray();
                if (SortedTiles.Length > 0)
                {
                    _min = new Point(int.MaxValue, int.MaxValue);
                    _max = new Point(int.MinValue, int.MinValue);
                    foreach (MultiTileEntry e in SortedTiles)
                    {
                        _min.X = Math.Min(_min.X, e.OffsetX);
                        _min.Y = Math.Min(_min.Y, e.OffsetY);
                        _max.X = Math.Max(_max.X, e.OffsetX);
                        _max.Y = Math.Max(_max.Y, e.OffsetY);
                        MaxHeight = Math.Max(MaxHeight, e.OffsetZ);
                    }
                }
            }

            ConvertList();
        }

        private static MultiTileEntry Entry(int id, int x, int y, int z, int flags = 1, int unk1 = 0)
        {
            return new MultiTileEntry
            {
                ItemId = (ushort)id,
                OffsetX = (short)x,
                OffsetY = (short)y,
                OffsetZ = (short)Math.Clamp(z, sbyte.MinValue, sbyte.MaxValue),
                Flags = flags,
                Unk1 = unk1
            };
        }

        /// <summary>
        /// A number written as decimal or as 0x hex.
        /// </summary>
        private static int ParseNumber(string text)
        {
            text = text.Trim();
            return text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? int.Parse(text[2..], System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture)
                : int.Parse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// UOX3 house items, as <see cref="ExportToUox3File"/> writes them:
        /// [HOUSE ITEM n] { ITEM=0x.. X=.. Y=.. Z=.. }.
        /// </summary>
        private static List<MultiTileEntry> ReadUox3(string fileName)
        {
            var list = new List<MultiTileEntry>();
            int id = -1, x = 0, y = 0, z = 0;
            foreach (string raw in File.ReadLines(fileName, Encoding.GetEncoding(1252)))
            {
                string line = raw.Trim();
                if (line.StartsWith('['))
                {
                    id = -1;
                    x = y = z = 0;
                    continue;
                }

                if (line == "}" && id >= 0)
                {
                    list.Add(Entry(id, x, y, z));
                    id = -1;
                    continue;
                }

                int equals = line.IndexOf('=');
                if (equals <= 0)
                {
                    continue;
                }

                string key = line[..equals].Trim().ToUpperInvariant();
                string value = line[(equals + 1)..];
                switch (key)
                {
                    case "ITEM":
                        id = ParseNumber(value);
                        break;
                    case "X":
                        x = ParseNumber(value);
                        break;
                    case "Y":
                        y = ParseNumber(value);
                        break;
                    case "Z":
                        z = ParseNumber(value);
                        break;
                }
            }

            return list;
        }

        /// <summary>
        /// The XML <see cref="ExportToXmlFile"/> writes: &lt;Entry&gt;&lt;Item X Y Z ID Flags Unk1 /&gt;.
        /// </summary>
        private static List<MultiTileEntry> ReadXml(string fileName)
        {
            var list = new List<MultiTileEntry>();
            var dom = new XmlDocument();
            dom.Load(fileName);
            foreach (XmlElement item in dom.GetElementsByTagName("Item"))
            {
                string flags = item.GetAttribute("Flags");
                string unk1 = item.GetAttribute("Unk1");
                list.Add(Entry(
                    ParseNumber(item.GetAttribute("ID")),
                    ParseNumber(item.GetAttribute("X")),
                    ParseNumber(item.GetAttribute("Y")),
                    ParseNumber(item.GetAttribute("Z")),
                    flags.Length > 0 ? ParseNumber(flags) : 1,
                    unk1.Length > 0 ? ParseNumber(unk1) : 0));
            }

            return list;
        }

        /// <summary>
        /// A UO House Editor design: a header line ("UOHE File ver 1.00",width,height,base), then
        /// one line per item: floor,x,y,altitude,item id (decimal). Floor 0 is the foundation with
        /// altitudes from 0; floor 1 starts at the base height and every further floor 20 higher.
        /// </summary>
        private static List<MultiTileEntry> ReadUoh(string fileName)
        {
            var list = new List<MultiTileEntry>();
            string[] lines = File.ReadAllLines(fileName, Encoding.GetEncoding(1252));
            if (lines.Length == 0 || !lines[0].StartsWith("\"UOHE File", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Not a UOHE design file");
            }

            string[] header = lines[0].Split(',');
            int baseHeight = header.Length > 3 ? ParseNumber(header[3]) : 7;

            for (int i = 1; i < lines.Length; ++i)
            {
                string[] parts = lines[i].Split(',');
                if (parts.Length < 5)
                {
                    continue;
                }

                int floor = ParseNumber(parts[0]);
                int altitude = ParseNumber(parts[3]);
                int z = floor == 0 ? altitude : baseHeight + ((floor - 1) * 20) + altitude;
                list.Add(Entry(ParseNumber(parts[4]), ParseNumber(parts[1]), ParseNumber(parts[2]), z));
            }

            return list;
        }

        /// <summary>
        /// A WorldForge export: &lt;tile id x y z hue /&gt; elements in world coordinates, moved so
        /// the lowest X, Y and Z are 0. Hues are not part of a multi and are dropped.
        /// </summary>
        private static List<MultiTileEntry> ReadWorldForge(string fileName)
        {
            var dom = new XmlDocument();
            dom.Load(fileName);
            var tiles = new List<(int Id, int X, int Y, int Z)>();
            foreach (XmlNode node in dom.GetElementsByTagName("*"))
            {
                if (node is XmlElement tile && tile.LocalName.Equals("tile", StringComparison.OrdinalIgnoreCase) && tile.HasAttribute("id"))
                {
                    tiles.Add((ParseNumber(tile.GetAttribute("id")), ParseNumber(tile.GetAttribute("x")),
                        ParseNumber(tile.GetAttribute("y")), ParseNumber(tile.GetAttribute("z"))));
                }
            }

            var list = new List<MultiTileEntry>(tiles.Count);
            if (tiles.Count == 0)
            {
                return list;
            }

            int minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            foreach (var t in tiles)
            {
                minX = Math.Min(minX, t.X);
                minY = Math.Min(minY, t.Y);
                minZ = Math.Min(minZ, t.Z);
            }

            foreach (var t in tiles)
            {
                list.Add(Entry(t.Id, t.X - minX, t.Y - minY, t.Z - minZ));
            }

            return list;
        }

        public MultiComponentList(List<MultiTileEntry> arr)
        {
            _min = _max = Point.Empty;
            int itemCount = arr.Count;
            SortedTiles = new MultiTileEntry[itemCount];
            _min.X = 10000;
            _min.Y = 10000;
            int i = 0;
            foreach (MultiTileEntry entry in arr)
            {
                if (entry.OffsetX < _min.X)
                {
                    _min.X = entry.OffsetX;
                }

                if (entry.OffsetY < _min.Y)
                {
                    _min.Y = entry.OffsetY;
                }

                if (entry.OffsetX > _max.X)
                {
                    _max.X = entry.OffsetX;
                }

                if (entry.OffsetY > _max.Y)
                {
                    _max.Y = entry.OffsetY;
                }

                if (entry.OffsetZ > MaxHeight)
                {
                    MaxHeight = entry.OffsetZ;
                }

                SortedTiles[i] = entry;

                ++i;
            }
            arr.Clear();

            ConvertList();
        }

        public MultiComponentList(StreamReader stream, int count)
        {
            int itemCount = 0;
            _min = _max = Point.Empty;
            SortedTiles = new MultiTileEntry[count];
            _min.X = 10000;
            _min.Y = 10000;

            while (stream.ReadLine() is { } line)
            {
                string[] split = Regex.Split(line, @"\s+");
                SortedTiles[itemCount].ItemId = Convert.ToUInt16(split[0]);
                SortedTiles[itemCount].Flags = Convert.ToInt32(split[1]);
                SortedTiles[itemCount].OffsetX = Convert.ToInt16(split[2]);
                SortedTiles[itemCount].OffsetY = Convert.ToInt16(split[3]);
                SortedTiles[itemCount].OffsetZ = Convert.ToInt16(split[4]);
                SortedTiles[itemCount].Unk1 = 0;

                MultiTileEntry e = SortedTiles[itemCount];

                if (e.OffsetX < _min.X)
                {
                    _min.X = e.OffsetX;
                }

                if (e.OffsetY < _min.Y)
                {
                    _min.Y = e.OffsetY;
                }

                if (e.OffsetX > _max.X)
                {
                    _max.X = e.OffsetX;
                }

                if (e.OffsetY > _max.Y)
                {
                    _max.Y = e.OffsetY;
                }

                if (e.OffsetZ > MaxHeight)
                {
                    MaxHeight = e.OffsetZ;
                }

                ++itemCount;
                if (itemCount == count)
                {
                    break;
                }
            }

            ConvertList();
        }

        private void ConvertList()
        {
            _center = new Point(-_min.X, -_min.Y);
            Width = (_max.X - _min.X) + 1;
            Height = (_max.Y - _min.Y) + 1;

            var tiles = new MTileList[Width][];
            Tiles = new MTile[Width][][];

            for (int x = 0; x < Width; ++x)
            {
                tiles[x] = new MTileList[Height];
                Tiles[x] = new MTile[Height][];

                for (int y = 0; y < Height; ++y)
                {
                    tiles[x][y] = new MTileList();
                }
            }

            for (int i = 0; i < SortedTiles.Length; ++i)
            {
                int xOffset = SortedTiles[i].OffsetX + _center.X;
                int yOffset = SortedTiles[i].OffsetY + _center.Y;

                tiles[xOffset][yOffset].Add(SortedTiles[i].ItemId, (sbyte)SortedTiles[i].OffsetZ,
                    (sbyte)SortedTiles[i].Flags, SortedTiles[i].Unk1);
            }

            Surface = 0;

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Tiles[x][y] = tiles[x][y].ToArray();
                    for (int i = 0; i < Tiles[x][y].Length; ++i)
                    {
                        Tiles[x][y][i].Solver = i;
                    }

                    if (Tiles[x][y].Length > 1)
                    {
                        Array.Sort(Tiles[x][y]);
                    }

                    if (Tiles[x][y].Length > 0)
                    {
                        ++Surface;
                    }
                }
            }
        }

        public MultiComponentList(MTileList[][] newTiles, int count, int width, int height)
        {
            _min = _max = Point.Empty;
            SortedTiles = new MultiTileEntry[count];
            _center = new Point((int)Math.Round(width / 2.0) - 1, (int)Math.Round(height / 2.0) - 1);
            if (_center.X < 0)
            {
                _center.X = width / 2;
            }

            if (_center.Y < 0)
            {
                _center.Y = height / 2;
            }

            MaxHeight = -128;

            int counter = 0;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    foreach (var mTile in newTiles[x][y].ToArray())
                    {
                        SortedTiles[counter].ItemId = mTile.Id;
                        SortedTiles[counter].OffsetX = (short)(x - _center.X);
                        SortedTiles[counter].OffsetY = (short)(y - _center.Y);
                        SortedTiles[counter].OffsetZ = mTile.Z;
                        SortedTiles[counter].Flags = mTile.Flag;
                        SortedTiles[counter].Unk1 = mTile.Unk1;

                        if (SortedTiles[counter].OffsetX < _min.X)
                        {
                            _min.X = SortedTiles[counter].OffsetX;
                        }

                        if (SortedTiles[counter].OffsetX > _max.X)
                        {
                            _max.X = SortedTiles[counter].OffsetX;
                        }

                        if (SortedTiles[counter].OffsetY < _min.Y)
                        {
                            _min.Y = SortedTiles[counter].OffsetY;
                        }

                        if (SortedTiles[counter].OffsetY > _max.Y)
                        {
                            _max.Y = SortedTiles[counter].OffsetY;
                        }

                        if (SortedTiles[counter].OffsetZ > MaxHeight)
                        {
                            MaxHeight = SortedTiles[counter].OffsetZ;
                        }

                        ++counter;
                    }
                }
            }
            ConvertList();
        }

        private MultiComponentList()
        {
            Tiles = Array.Empty<MTile[][]>();
        }

        // The exports below mirror what the matching imports read. Where an import skips a field
        // (UO Architect's category, level and hue, the cache header's other numbers), the export
        // writes a neutral value: only this reader's view of those formats is known here.

        private static void WriteUOAString(BinaryWriter writer, string value)
        {
            writer.Write((byte)(value == null ? 0 : 1));
            if (value != null)
            {
                writer.Write(value);
            }
        }

        /// <summary>UO Architect binary design (.uoa), version 1: one design with a name and its items.</summary>
        public void ExportToUOABinaryFile(string fileName, string name)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            {
                writer.Write((short)1); // version
                WriteUOAString(writer, name);
                WriteUOAString(writer, null); // category
                WriteUOAString(writer, null); // subsection
                writer.Write(Width);
                writer.Write(Height);
                writer.Write(Width);  // user width
                writer.Write(Height); // user height
                writer.Write(SortedTiles.Length);
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    writer.Write((short)SortedTiles[i].ItemId);
                    writer.Write(SortedTiles[i].OffsetX);
                    writer.Write(SortedTiles[i].OffsetY);
                    writer.Write(SortedTiles[i].OffsetZ);
                    writer.Write((short)0); // level
                    writer.Write((short)0); // hue
                }
            }
        }

        /// <summary>
        /// UO Architect design set: <c>name.idx</c> and <c>name.bin</c> next to
        /// <paramref name="fileName"/>, holding this one design (index version 0, item records of type 0).
        /// </summary>
        public void ExportToDesignerFiles(string fileName, string name)
        {
            string root = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fileName)) ?? string.Empty, Path.GetFileNameWithoutExtension(fileName));
            using (var idxfs = new FileStream(root + ".idx", FileMode.Create, FileAccess.Write))
            using (var binfs = new FileStream(root + ".bin", FileMode.Create, FileAccess.Write))
            using (var idx = new BinaryWriter(idxfs))
            using (var bin = new BinaryWriter(binfs))
            {
                idx.Write(1); // designs
                idx.Write(0); // version
                WriteUOAString(idx, name);
                WriteUOAString(idx, null); // category
                WriteUOAString(idx, null); // subsection
                idx.Write(Width);
                idx.Write(Height);
                idx.Write(Width);  // user width
                idx.Write(Height); // user height
                idx.Write(0L); // position of the first record in the bin
                idx.Write(SortedTiles.Length);

                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    bin.Write(0); // record type
                    bin.Write((int)SortedTiles[i].ItemId);
                    bin.Write((int)SortedTiles[i].OffsetX);
                    bin.Write((int)SortedTiles[i].OffsetY);
                    bin.Write((int)SortedTiles[i].OffsetZ);
                    bin.Write(0); // level
                }
            }
        }

        /// <summary>
        /// Client multi cache text (Multicache.dat) holding this one design: a header line whose third
        /// number is the item count, then one line per item: id, flags, x, y, z.
        /// </summary>
        public void ExportToCacheFile(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine($"0 0 {SortedTiles.Length} 0 0 0 0");
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    tex.WriteLine(
                        $"{SortedTiles[i].ItemId} {SortedTiles[i].Flags} {SortedTiles[i].OffsetX} {SortedTiles[i].OffsetY} {SortedTiles[i].OffsetZ}");
                }
            }
        }

        /// <summary>
        /// UO House Editor (UOHE) design: the header line, then floor, x, y, altitude, item id per item.
        /// Items below <paramref name="baseHeight"/> are on floor 0 (the foundation) with their Z as
        /// altitude; above it a floor is 20 high. X and Y start at 0.
        /// </summary>
        public void ExportToUohFile(string fileName, int baseHeight = 7)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine($"\"UOHE File ver 1.00\",{Width},{Height},{baseHeight}");
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    int z = SortedTiles[i].OffsetZ;
                    int floor = z < baseHeight ? 0 : 1 + ((z - baseHeight) / 20);
                    int altitude = floor == 0 ? z : z - baseHeight - ((floor - 1) * 20);
                    tex.WriteLine($"{floor},{SortedTiles[i].OffsetX - _min.X},{SortedTiles[i].OffsetY - _min.Y},{altitude},{SortedTiles[i].ItemId}");
                }
            }
        }

        /// <summary>WorldForge tile list: one <c>tile</c> element per item with id, x, y, z and hue 0, from 0,0.</summary>
        public void ExportToWorldForgeFile(string fileName)
        {
            var settings = new XmlWriterSettings { Indent = true };
            using (XmlWriter xml = XmlWriter.Create(fileName, settings))
            {
                xml.WriteStartElement("tiles");
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    xml.WriteStartElement("tile");
                    xml.WriteAttributeString("id", SortedTiles[i].ItemId.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    xml.WriteAttributeString("x", (SortedTiles[i].OffsetX - _min.X).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    xml.WriteAttributeString("y", (SortedTiles[i].OffsetY - _min.Y).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    xml.WriteAttributeString("z", SortedTiles[i].OffsetZ.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    xml.WriteAttributeString("hue", "0");
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }
        }

        public void ExportToTextFile(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    tex.WriteLine(
                        $"0x{SortedTiles[i].ItemId:X} {SortedTiles[i].OffsetX} {SortedTiles[i].OffsetY} {SortedTiles[i].OffsetZ} {SortedTiles[i].Flags}");
                }
            }
        }

        public void ExportToWscFile(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    tex.WriteLine($"SECTION WORLDITEM {i}");
                    tex.WriteLine("{");
                    tex.WriteLine($"\tID\t{SortedTiles[i].ItemId}");
                    tex.WriteLine($"\tX\t{SortedTiles[i].OffsetX}");
                    tex.WriteLine($"\tY\t{SortedTiles[i].OffsetY}");
                    tex.WriteLine($"\tZ\t{SortedTiles[i].OffsetZ}");
                    tex.WriteLine("\tColor\t0");
                    tex.WriteLine("}");
                }
            }
        }

        public void ExportToUox3File(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    tex.WriteLine($"[HOUSE ITEM {i}]");
                    tex.WriteLine("{");
                    tex.WriteLine($"ITEM=0x{SortedTiles[i].ItemId:X4}");
                    tex.WriteLine($"X={SortedTiles[i].OffsetX}");
                    tex.WriteLine($"Y={SortedTiles[i].OffsetY}");
                    tex.WriteLine($"Z={SortedTiles[i].OffsetZ}");
                    tex.WriteLine("}");
                    tex.WriteLine(string.Empty);
                }
            }
        }

        public void ExportToUOAFile(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("6 version");
                tex.WriteLine("1 template id");
                tex.WriteLine("-1 item version");
                tex.WriteLine($"{SortedTiles.Length} num components");
                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    tex.WriteLine(
                        $"{SortedTiles[i].ItemId} {SortedTiles[i].OffsetX} {SortedTiles[i].OffsetY} {SortedTiles[i].OffsetZ} {SortedTiles[i].Flags}");
                }
            }
        }

        /// <summary>
        /// Punt's multi tool csv format
        /// </summary>
        /// <param name="field"></param>
        /// <summary>
        /// Parses the sixth CSV column: an always-empty "Cliloc" in old exports, now
        /// <see cref="MultiTileEntry.Unk1"/>. Both forms have to load.
        /// </summary>
        private static int ParseCsvUnk1(string field)
        {
            string tmp = field.Trim();

            if (tmp.Length == 0)
            {
                return 0;
            }

            if (tmp.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(tmp.AsSpan(2), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out int hex) ? hex : 0;
            }

            return int.TryParse(tmp, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out int dec) ? dec : 0;
        }

        public void ExportToCsvFile(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                // The last column used to be an always-empty "Cliloc" and now carries the High Seas trailing
                // int32 - the 0x0100 visibility bit of the uop tile record, set on 8207 of 186695 shipped tiles.
                tex.WriteLine("TileID,OffsetX,OffsetY,OffsetZ,Flag,Unk1");

                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    tex.WriteLine($"0x{SortedTiles[i].ItemId:x4},{SortedTiles[i].OffsetX},{SortedTiles[i].OffsetY},{SortedTiles[i].OffsetZ},0x{SortedTiles[i].Flags:x},0x{SortedTiles[i].Unk1:x}");
                }
            }
        }

        public void ExportToXmlFile(string fileName, string entryId)
        {
            using (var xmlWriter = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartElement("Entry");
                xmlWriter.WriteAttributeString("ID", entryId);

                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    xmlWriter.WriteStartElement("Item");
                    xmlWriter.WriteAttributeString("X", SortedTiles[i].OffsetX.ToString());
                    xmlWriter.WriteAttributeString("Y", SortedTiles[i].OffsetY.ToString());
                    xmlWriter.WriteAttributeString("Z", SortedTiles[i].OffsetZ.ToString());
                    xmlWriter.WriteAttributeString("ID", $"0x{SortedTiles[i].ItemId:X4}");

                    // Both halves of the multi.mul row past the coordinates; without them the export cannot
                    // describe the tile fully.
                    xmlWriter.WriteAttributeString("Flags", $"0x{SortedTiles[i].Flags:X}");
                    xmlWriter.WriteAttributeString("Unk1", $"0x{SortedTiles[i].Unk1:X}");

                    xmlWriter.WriteEndElement(); // Item
                }

                xmlWriter.WriteEndElement();
            }
        }
    }
}