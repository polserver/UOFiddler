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
            reader.Close();
        }

        public MultiComponentList(string fileName, Multis.ImportType type)
        {
            _min = _max = Point.Empty;

            int itemCount;

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
                            if (reader.ReadInt16() != 1) // Version check
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
                                        SortedTiles[itemCount] = tempItem;
                                        ++itemCount;
                                    }
                                    tempItem.ItemId = 0xFFFF;
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
                                SortedTiles[itemCount] = tempItem;
                            }
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
            }

            ConvertList();
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
                        SortedTiles[counter].Unk1 = 0;

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
        /// <param name="fileName"></param>
        public void ExportToCsvFile(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("TileID,OffsetX,OffsetY,OffsetZ,Flag,Cliloc");

                for (int i = 0; i < SortedTiles.Length; ++i)
                {
                    tex.WriteLine($"0x{SortedTiles[i].ItemId:x4},{SortedTiles[i].OffsetX},{SortedTiles[i].OffsetY},{SortedTiles[i].OffsetZ},0x{SortedTiles[i].Flags:x},");
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
                    xmlWriter.WriteEndElement(); // Item
                }

                xmlWriter.WriteEndElement();
            }
        }
    }
}