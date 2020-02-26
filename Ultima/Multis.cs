using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Ultima
{
    public sealed class Multis
    {
        private static MultiComponentList[] _mComponents = new MultiComponentList[0x3000];
        private static FileIndex _mFileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x3000, 14);

        public enum ImportType
        {
            TXT,
            UOA,
            UOAB,
            WSC,
            MULTICACHE,
            UOADESIGN
        }

		public static bool PostHsFormat { get; set; }
        /// <summary>
        /// ReReads multi.mul
        /// </summary>
        public static void Reload()
        {
            _mFileIndex = new FileIndex("Multi.idx", "Multi.mul", 0x3000, 14);
            _mComponents = new MultiComponentList[0x3000];
        }

        /// <summary>
        /// Gets <see cref="MultiComponentList"/> of multi
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static MultiComponentList GetComponents(int index)
        {
            MultiComponentList mcl;

            index &= 0x2FFF;

            if (index >= 0 && index < _mComponents.Length)
            {
                mcl = _mComponents[index];

                if (mcl == null)
                    _mComponents[index] = mcl = Load(index);
            }
            else
                mcl = MultiComponentList.Empty;

            return mcl;
        }

        public static MultiComponentList Load(int index)
        {
            try
            {
                int length, extra;
                bool patched;
                Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);

                if (stream == null)
                    return MultiComponentList.Empty;

				if (PostHsFormat || Art.IsUoahs())
                    return new MultiComponentList(new BinaryReader(stream), length / 16);
                else
                    return new MultiComponentList(new BinaryReader(stream), length / 12);
            }
            catch
            {
                return MultiComponentList.Empty;
            }
        }

        public static void Remove(int index)
        {
            _mComponents[index] = MultiComponentList.Empty;
        }

        public static void Add(int index, MultiComponentList comp)
        {
            _mComponents[index] = comp;
        }

        public static MultiComponentList ImportFromFile(int index, string fileName, ImportType type)
        {
            try
            {
                return _mComponents[index] = new MultiComponentList(fileName, type);
            }
            catch
            {
                return _mComponents[index] = MultiComponentList.Empty;
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
            List<MultiComponentList> multilist = new List<MultiComponentList>();
            using (StreamReader ip = new StreamReader(fileName))
            {
                string line;
                while ((line = ip.ReadLine()) != null)
                {
                    string[] split = Regex.Split(line, @"\s+");
                    if (split.Length == 7)
                    {
                        int count = Convert.ToInt32(split[2]);
                        multilist.Add(new MultiComponentList(ip, count));
                    }
                }
            }
            return multilist;
        }

        public static string ReadUoaString(BinaryReader bin)
        {
            byte flag = bin.ReadByte();

            if (flag == 0)
                return null;
            else
                return bin.ReadString();
        }
        public static List<object[]> LoadFromDesigner(string fileName)
        {
            List<object[]> multilist = new List<object[]>();
            string root = Path.GetFileNameWithoutExtension(fileName);
            string idx = $"{root}.idx";
            string bin = $"{root}.bin";
            if ((!File.Exists(idx)) || (!File.Exists(bin)))
                return multilist;
            using (FileStream idxfs = new FileStream(idx, FileMode.Open, FileAccess.Read, FileShare.Read),
                              binfs = new FileStream(bin, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader idxbin = new BinaryReader(idxfs),
                                    binbin = new BinaryReader(binfs))
                {
                    int count = idxbin.ReadInt32();
                    int version = idxbin.ReadInt32();

                    for (int i = 0; i < count; ++i)
                    {
                        object[] data = new object[2];
                        switch (version)
                        {
                            case 0:
                                data[0] = ReadUoaString(idxbin);
                                List<MultiComponentList.MultiTileEntry> arr = new List<MultiComponentList.MultiTileEntry>();
                                data[0] += "-" + ReadUoaString(idxbin);
                                data[0] += "-" + ReadUoaString(idxbin);
                                int width = idxbin.ReadInt32();
                                int height = idxbin.ReadInt32();
                                int uwidth = idxbin.ReadInt32();
                                int uheight = idxbin.ReadInt32();
                                long filepos = idxbin.ReadInt64();
                                int reccount = idxbin.ReadInt32();

                                binbin.BaseStream.Seek(filepos, SeekOrigin.Begin);
                                int index, x, y, z, level, hue;
                                for (int j = 0; j < reccount; ++j)
                                {
                                    index = x = y = z = level = hue = 0;
                                    int compVersion = binbin.ReadInt32();
                                    switch (compVersion)
                                    {
                                        case 0:
                                            index = binbin.ReadInt32();
                                            x = binbin.ReadInt32();
                                            y = binbin.ReadInt32();
                                            z = binbin.ReadInt32();
                                            level = binbin.ReadInt32();
                                            break;

                                        case 1:
                                            index = binbin.ReadInt32();
                                            x = binbin.ReadInt32();
                                            y = binbin.ReadInt32();
                                            z = binbin.ReadInt32();
                                            level = binbin.ReadInt32();
                                            hue = binbin.ReadInt32();
                                            break;
                                    }
                                    MultiComponentList.MultiTileEntry tempitem = new MultiComponentList.MultiTileEntry();
                                    tempitem.MItemId = (ushort)index;
                                    tempitem.MFlags = 1;
                                    tempitem.MOffsetX = (short)x;
                                    tempitem.MOffsetY = (short)y;
                                    tempitem.MOffsetZ = (short)z;
                                    tempitem.MUnk1 = 0;
                                    arr.Add(tempitem);

                                }
                                data[1] = new MultiComponentList(arr);
                                break;

                        }
                        multilist.Add(data);
                    }
                }
                return multilist;
            }
        }

        public static List<MultiComponentList.MultiTileEntry> RebuildTiles(MultiComponentList.MultiTileEntry[] tiles)
        {
            List<MultiComponentList.MultiTileEntry> newtiles = new List<MultiComponentList.MultiTileEntry>();
            newtiles.AddRange(tiles);

            if (newtiles[0].MOffsetX == 0 && newtiles[0].MOffsetY == 0 && newtiles[0].MOffsetZ == 0) // found a centeritem
            {
                if (newtiles[0].MItemId != 0x1) // its a "good" one
                {
                    for (int j = newtiles.Count - 1; j >= 0; --j) // remove all invis items
                    {
                        if (newtiles[j].MItemId == 0x1)
                            newtiles.RemoveAt(j);
                    }
                    return newtiles;
                }
                else // a bad one
                {
                    for (int i = 1; i < newtiles.Count; ++i) // do we have a better one?
                    {
                        if (newtiles[i].MOffsetX == 0 && newtiles[i].MOffsetY == 0 
                            && newtiles[i].MItemId != 0x1 && newtiles[i].MOffsetZ == 0 )
                        {
                            MultiComponentList.MultiTileEntry centeritem = newtiles[i];
                            newtiles.RemoveAt(i); // jep so save it
                            for (int j = newtiles.Count-1; j >= 0; --j) // and remove all invis
                            {
                                if (newtiles[j].MItemId == 0x1)
                                    newtiles.RemoveAt(j);
                            }
                            newtiles.Insert(0, centeritem);
                            return newtiles;
                        }
                    }
                    for (int j = newtiles.Count-1; j >= 1; --j) // nothing found so remove all invis exept the first
                    {
                        if (newtiles[j].MItemId == 0x1)
                            newtiles.RemoveAt(j);
                    }
                    return newtiles;
                }
            }
            for (int i = 0; i < newtiles.Count; ++i) // is there a good one
            {
                if (newtiles[i].MOffsetX == 0 && newtiles[i].MOffsetY == 0 
                    && newtiles[i].MItemId != 0x1 && newtiles[i].MOffsetZ == 0)
                {
                    MultiComponentList.MultiTileEntry centeritem = newtiles[i];
                    newtiles.RemoveAt(i); // store it
                    for (int j = newtiles.Count-1; j >= 0; --j) // remove all invis
                    {
                        if (newtiles[j].MItemId == 0x1)
                            newtiles.RemoveAt(j);
                    }
                    newtiles.Insert(0, centeritem);
                    return newtiles;
                }
            }
            for (int j = newtiles.Count-1; j >= 0; --j) // nothing found so remove all invis
            {
                if (newtiles[j].MItemId == 0x1)
                    newtiles.RemoveAt(j);
            }
            MultiComponentList.MultiTileEntry invisitem = new MultiComponentList.MultiTileEntry();
            invisitem.MItemId = 0x1; // and create a new invis
            invisitem.MOffsetX = 0;
            invisitem.MOffsetY = 0;
            invisitem.MOffsetZ = 0;
            invisitem.MFlags = 0;
            invisitem.MUnk1 = 0;
            newtiles.Insert(0, invisitem);
            return newtiles;
        }

        public static void Save(string path)
        {
			bool isUoahs = PostHsFormat || Art.IsUoahs();
            string idx = Path.Combine(path, "multi.idx");
            string mul = Path.Combine(path, "multi.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int index = 0; index < 0x3000; ++index)
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
                            binidx.Write((int)fsmul.Position); //lookup
                            if (isUoahs)
                                binidx.Write(tiles.Count * 16); //length
                            else
                                binidx.Write(tiles.Count * 12); //length
                            binidx.Write(-1); //extra
                            for (int i = 0; i < tiles.Count; ++i)
                            {
                                binmul.Write(tiles[i].MItemId);
                                binmul.Write(tiles[i].MOffsetX);
                                binmul.Write(tiles[i].MOffsetY);
                                binmul.Write(tiles[i].MOffsetZ);
                                binmul.Write(tiles[i].MFlags);
                                if (isUoahs)
                                    binmul.Write(tiles[i].MUnk1);

                            }
                        }
                    }
                }
            }
        }
    }

    public sealed class MultiComponentList
    {
        private Point _mMin, _mMax, _mCenter;
        private int _mWidth, _mHeight;
        private readonly int _mMaxHeight;
        private int _mSurface;
        private MTile[][][] _mTiles;
        private readonly MultiTileEntry[] _mSortedTiles;

        public static readonly MultiComponentList Empty = new MultiComponentList();

        public Point Min => _mMin;
        public Point Max => _mMax;
        public Point Center => _mCenter;
        public int Width => _mWidth;
        public int Height => _mHeight;
        public MTile[][][] Tiles => _mTiles;
        public int MaxHeight => _mMaxHeight;
        public MultiTileEntry[] SortedTiles => _mSortedTiles;
        public int Surface => _mSurface;


        public struct MultiTileEntry
        {
            public ushort MItemId;
            public short MOffsetX, MOffsetY, MOffsetZ;
            public int MFlags;
            public int MUnk1;
        }

        /// <summary>
        /// Returns Bitmap of Multi
        /// </summary>
        /// <returns></returns>
        public Bitmap GetImage()
        {
            return GetImage(300);
        }

        /// <summary>
        /// Returns Bitmap of Multi to maxheight
        /// </summary>
        /// <param name="maxheight"></param>
        /// <returns></returns>
        public Bitmap GetImage(int maxheight)
        {
            if (_mWidth == 0 || _mHeight == 0)
                return null;

            int xMin = 1000, yMin = 1000;
            int xMax = -1000, yMax = -1000;

            for (int x = 0; x < _mWidth; ++x)
            {
                for (int y = 0; y < _mHeight; ++y)
                {
                    MTile[] tiles = _mTiles[x][y];

                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        Bitmap bmp = Art.GetStatic(tiles[i].Id);

                        if (bmp == null)
                            continue;

                        int px = (x - y) * 22;
                        int py = (x + y) * 22;

                        px -= (bmp.Width / 2);
                        py -= tiles[i].Z << 2;
                        py -= bmp.Height;

                        if (px < xMin)
                            xMin = px;

                        if (py < yMin)
                            yMin = py;

                        px += bmp.Width;
                        py += bmp.Height;

                        if (px > xMax)
                            xMax = px;

                        if (py > yMax)
                            yMax = py;
                    }
                }
            }

            Bitmap canvas = new Bitmap(xMax - xMin, yMax - yMin);
            Graphics gfx = Graphics.FromImage(canvas);
            gfx.Clear(Color.Transparent);
            for (int x = 0; x < _mWidth; ++x)
            {
                for (int y = 0; y < _mHeight; ++y)
                {
                    MTile[] tiles = _mTiles[x][y];

                    for (int i = 0; i < tiles.Length; ++i)
                    {

                        Bitmap bmp = Art.GetStatic(tiles[i].Id);

                        if (bmp == null)
                            continue;
                        if ((tiles[i].Z) > maxheight)
                            continue;
                        int px = (x - y) * 22;
                        int py = (x + y) * 22;

                        px -= (bmp.Width / 2);
                        py -= tiles[i].Z << 2;
                        py -= bmp.Height;
                        px -= xMin;
                        py -= yMin;

                        gfx.DrawImageUnscaled(bmp, px, py, bmp.Width, bmp.Height);
                    }

                    int tx = (x - y) * 22;
                    int ty = (x + y) * 22;
                    tx -= xMin;
                    ty -= yMin;
                }
            }

            gfx.Dispose();

            return canvas;
        }

        public MultiComponentList(BinaryReader reader, int count)
        {
			bool useNewMultiFormat = Multis.PostHsFormat || Art.IsUoahs();
            _mMin = _mMax = Point.Empty;
            _mSortedTiles = new MultiTileEntry[count];
            for (int i = 0; i < count; ++i)
            {
                _mSortedTiles[i].MItemId = Art.GetLegalItemId(reader.ReadUInt16());
                _mSortedTiles[i].MOffsetX = reader.ReadInt16();
                _mSortedTiles[i].MOffsetY = reader.ReadInt16();
                _mSortedTiles[i].MOffsetZ = reader.ReadInt16();
                _mSortedTiles[i].MFlags = reader.ReadInt32();
                if (useNewMultiFormat)
                    _mSortedTiles[i].MUnk1 = reader.ReadInt32();
                else
                    _mSortedTiles[i].MUnk1 = 0;

                MultiTileEntry e = _mSortedTiles[i];

                if (e.MOffsetX < _mMin.X)
                    _mMin.X = e.MOffsetX;

                if (e.MOffsetY < _mMin.Y)
                    _mMin.Y = e.MOffsetY;

                if (e.MOffsetX > _mMax.X)
                    _mMax.X = e.MOffsetX;

                if (e.MOffsetY > _mMax.Y)
                    _mMax.Y = e.MOffsetY;

                if (e.MOffsetZ > _mMaxHeight)
                    _mMaxHeight = e.MOffsetZ;
            }
            ConvertList();
            reader.Close();
        }

        public MultiComponentList(string fileName, Multis.ImportType type)
        {
            _mMin = _mMax = Point.Empty;
            int itemcount;
            switch (type)
            {
                case Multis.ImportType.TXT:
                    itemcount = 0;
                    using (StreamReader ip = new StreamReader(fileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            itemcount++;
                        }
                    }
                    _mSortedTiles = new MultiTileEntry[itemcount];
                    itemcount = 0;
                    _mMin.X = 10000;
                    _mMin.Y = 10000;
                    using (StreamReader ip = new StreamReader(fileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            string[] split = line.Split(' ');

                            string tmp = split[0];
                            tmp = tmp.Replace("0x", "");

                            _mSortedTiles[itemcount].MItemId = ushort.Parse(tmp, System.Globalization.NumberStyles.HexNumber);
                            _mSortedTiles[itemcount].MOffsetX = Convert.ToInt16(split[1]);
                            _mSortedTiles[itemcount].MOffsetY = Convert.ToInt16(split[2]);
                            _mSortedTiles[itemcount].MOffsetZ = Convert.ToInt16(split[3]);
                            _mSortedTiles[itemcount].MFlags = Convert.ToInt32(split[4]);
                            _mSortedTiles[itemcount].MUnk1 = 0;

                            MultiTileEntry e = _mSortedTiles[itemcount];

                            if (e.MOffsetX < _mMin.X)
                                _mMin.X = e.MOffsetX;

                            if (e.MOffsetY < _mMin.Y)
                                _mMin.Y = e.MOffsetY;

                            if (e.MOffsetX > _mMax.X)
                                _mMax.X = e.MOffsetX;

                            if (e.MOffsetY > _mMax.Y)
                                _mMax.Y = e.MOffsetY;

                            if (e.MOffsetZ > _mMaxHeight)
                                _mMaxHeight = e.MOffsetZ;

                            itemcount++;
                        }
                        int centerx = _mMax.X - (int)(Math.Round((_mMax.X - _mMin.X) / 2.0));
                        int centery = _mMax.Y - (int)(Math.Round((_mMax.Y - _mMin.Y) / 2.0));

                        _mMin = _mMax = Point.Empty;
                        int i = 0;
                        for (; i < _mSortedTiles.Length; i++)
                        {
                            _mSortedTiles[i].MOffsetX -= (short)centerx;
                            _mSortedTiles[i].MOffsetY -= (short)centery;
                            if (_mSortedTiles[i].MOffsetX < _mMin.X)
                                _mMin.X = _mSortedTiles[i].MOffsetX;
                            if (_mSortedTiles[i].MOffsetX > _mMax.X)
                                _mMax.X = _mSortedTiles[i].MOffsetX;

                            if (_mSortedTiles[i].MOffsetY < _mMin.Y)
                                _mMin.Y = _mSortedTiles[i].MOffsetY;
                            if (_mSortedTiles[i].MOffsetY > _mMax.Y)
                                _mMax.Y = _mSortedTiles[i].MOffsetY;
                        }
                    }
                    break;
                case Multis.ImportType.UOA:
                    itemcount = 0;

                    using (StreamReader ip = new StreamReader(fileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            ++itemcount;
                            if (itemcount == 4)
                            {
                                string[] split = line.Split(' ');
                                itemcount = Convert.ToInt32(split[0]);
                                break;
                            }
                        }
                    }
                    _mSortedTiles = new MultiTileEntry[itemcount];
                    itemcount = 0;
                    _mMin.X = 10000;
                    _mMin.Y = 10000;
                    using (StreamReader ip = new StreamReader(fileName))
                    {
                        string line;
                        int i = -1;
                        while ((line = ip.ReadLine()) != null)
                        {
                            ++i;
                            if (i < 4)
                                continue;
                            string[] split = line.Split(' ');

                            _mSortedTiles[itemcount].MItemId = Convert.ToUInt16(split[0]);
                            _mSortedTiles[itemcount].MOffsetX = Convert.ToInt16(split[1]);
                            _mSortedTiles[itemcount].MOffsetY = Convert.ToInt16(split[2]);
                            _mSortedTiles[itemcount].MOffsetZ = Convert.ToInt16(split[3]);
                            _mSortedTiles[itemcount].MFlags = Convert.ToInt32(split[4]);
                            _mSortedTiles[itemcount].MUnk1 = 0;

                            MultiTileEntry e = _mSortedTiles[itemcount];

                            if (e.MOffsetX < _mMin.X)
                                _mMin.X = e.MOffsetX;

                            if (e.MOffsetY < _mMin.Y)
                                _mMin.Y = e.MOffsetY;

                            if (e.MOffsetX > _mMax.X)
                                _mMax.X = e.MOffsetX;

                            if (e.MOffsetY > _mMax.Y)
                                _mMax.Y = e.MOffsetY;

                            if (e.MOffsetZ > _mMaxHeight)
                                _mMaxHeight = e.MOffsetZ;

                            ++itemcount;
                        }
                        int centerx = _mMax.X - (int)(Math.Round((_mMax.X - _mMin.X) / 2.0));
                        int centery = _mMax.Y - (int)(Math.Round((_mMax.Y - _mMin.Y) / 2.0));

                        _mMin = _mMax = Point.Empty;
                        i = 0;
                        for (; i < _mSortedTiles.Length; ++i)
                        {
                            _mSortedTiles[i].MOffsetX -= (short)centerx;
                            _mSortedTiles[i].MOffsetY -= (short)centery;
                            if (_mSortedTiles[i].MOffsetX < _mMin.X)
                                _mMin.X = _mSortedTiles[i].MOffsetX;
                            if (_mSortedTiles[i].MOffsetX > _mMax.X)
                                _mMax.X = _mSortedTiles[i].MOffsetX;

                            if (_mSortedTiles[i].MOffsetY < _mMin.Y)
                                _mMin.Y = _mSortedTiles[i].MOffsetY;
                            if (_mSortedTiles[i].MOffsetY > _mMax.Y)
                                _mMax.Y = _mSortedTiles[i].MOffsetY;
                        }
                    }

                    break;
                case Multis.ImportType.UOAB:
                    using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        if (reader.ReadInt16() != 1) //Version check
                            return;
                        string tmp;
                        tmp = Multis.ReadUoaString(reader); //Name
                        tmp = Multis.ReadUoaString(reader); //Category
                        tmp = Multis.ReadUoaString(reader); //Subsection
                        int width = reader.ReadInt32();
                        int height = reader.ReadInt32();
                        int uwidth = reader.ReadInt32();
                        int uheight = reader.ReadInt32();

                        int count = reader.ReadInt32();
                        itemcount = count;
                        _mSortedTiles = new MultiTileEntry[itemcount];
                        itemcount = 0;
                        _mMin.X = 10000;
                        _mMin.Y = 10000;
                        for (; itemcount < count; ++itemcount)
                        {
                            _mSortedTiles[itemcount].MItemId = (ushort)reader.ReadInt16();
                            _mSortedTiles[itemcount].MOffsetX = reader.ReadInt16();
                            _mSortedTiles[itemcount].MOffsetY = reader.ReadInt16();
                            _mSortedTiles[itemcount].MOffsetZ = reader.ReadInt16();
                            reader.ReadInt16(); // level
                            _mSortedTiles[itemcount].MFlags = 1;
                            reader.ReadInt16(); // hue
                            _mSortedTiles[itemcount].MUnk1 = 0;

                            MultiTileEntry e = _mSortedTiles[itemcount];

                            if (e.MOffsetX < _mMin.X)
                                _mMin.X = e.MOffsetX;

                            if (e.MOffsetY < _mMin.Y)
                                _mMin.Y = e.MOffsetY;

                            if (e.MOffsetX > _mMax.X)
                                _mMax.X = e.MOffsetX;

                            if (e.MOffsetY > _mMax.Y)
                                _mMax.Y = e.MOffsetY;

                            if (e.MOffsetZ > _mMaxHeight)
                                _mMaxHeight = e.MOffsetZ;
                        }
                        int centerx = _mMax.X - (int)(Math.Round((_mMax.X - _mMin.X) / 2.0));
                        int centery = _mMax.Y - (int)(Math.Round((_mMax.Y - _mMin.Y) / 2.0));

                        _mMin = _mMax = Point.Empty;
                        itemcount = 0;
                        for (; itemcount < _mSortedTiles.Length; ++itemcount)
                        {
                            _mSortedTiles[itemcount].MOffsetX -= (short)centerx;
                            _mSortedTiles[itemcount].MOffsetY -= (short)centery;
                            if (_mSortedTiles[itemcount].MOffsetX < _mMin.X)
                                _mMin.X = _mSortedTiles[itemcount].MOffsetX;
                            if (_mSortedTiles[itemcount].MOffsetX > _mMax.X)
                                _mMax.X = _mSortedTiles[itemcount].MOffsetX;

                            if (_mSortedTiles[itemcount].MOffsetY < _mMin.Y)
                                _mMin.Y = _mSortedTiles[itemcount].MOffsetY;
                            if (_mSortedTiles[itemcount].MOffsetY > _mMax.Y)
                                _mMax.Y = _mSortedTiles[itemcount].MOffsetY;
                        }
                    }
                    break;

                case Multis.ImportType.WSC:
                    itemcount = 0;
                    using (StreamReader ip = new StreamReader(fileName))
                    {
                        string line;
                        while ((line = ip.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.StartsWith("SECTION WORLDITEM"))
                                ++itemcount;
                        }
                    }
                    _mSortedTiles = new MultiTileEntry[itemcount];
                    itemcount = 0;
                    _mMin.X = 10000;
                    _mMin.Y = 10000;
                    using (StreamReader ip = new StreamReader(fileName))
                    {
                        string line;
                        MultiTileEntry tempitem = new MultiTileEntry();
                        tempitem.MItemId = 0xFFFF;
                        tempitem.MFlags = 1;
                        tempitem.MUnk1 = 0;
                        while ((line = ip.ReadLine()) != null)
                        {
                            line = line.Trim();
                            if (line.StartsWith("SECTION WORLDITEM"))
                            {
                                if (tempitem.MItemId != 0xFFFF)
                                {
                                    _mSortedTiles[itemcount] = tempitem;
                                    ++itemcount;
                                }
                                tempitem.MItemId = 0xFFFF;
                            }
                            else if (line.StartsWith("ID"))
                            {
                                line = line.Remove(0, 2);
                                line = line.Trim();
                                tempitem.MItemId = Convert.ToUInt16(line);
                            }
                            else if (line.StartsWith("X"))
                            {
                                line = line.Remove(0, 1);
                                line = line.Trim();
                                tempitem.MOffsetX = Convert.ToInt16(line);
                                if (tempitem.MOffsetX < _mMin.X)
                                    _mMin.X = tempitem.MOffsetX;
                                if (tempitem.MOffsetX > _mMax.X)
                                    _mMax.X = tempitem.MOffsetX;
                            }
                            else if (line.StartsWith("Y"))
                            {
                                line = line.Remove(0, 1);
                                line = line.Trim();
                                tempitem.MOffsetY = Convert.ToInt16(line);
                                if (tempitem.MOffsetY < _mMin.Y)
                                    _mMin.Y = tempitem.MOffsetY;
                                if (tempitem.MOffsetY > _mMax.Y)
                                    _mMax.Y = tempitem.MOffsetY;
                            }
                            else if (line.StartsWith("Z"))
                            {
                                line = line.Remove(0, 1);
                                line = line.Trim();
                                tempitem.MOffsetZ = Convert.ToInt16(line);
                                if (tempitem.MOffsetZ > _mMaxHeight)
                                    _mMaxHeight = tempitem.MOffsetZ;

                            }
                        }
                        if (tempitem.MItemId != 0xFFFF)
                            _mSortedTiles[itemcount] = tempitem;

                        int centerx = _mMax.X - (int)(Math.Round((_mMax.X - _mMin.X) / 2.0));
                        int centery = _mMax.Y - (int)(Math.Round((_mMax.Y - _mMin.Y) / 2.0));

                        _mMin = _mMax = Point.Empty;
                        int i = 0;
                        for (; i < _mSortedTiles.Length; i++)
                        {
                            _mSortedTiles[i].MOffsetX -= (short)centerx;
                            _mSortedTiles[i].MOffsetY -= (short)centery;
                            if (_mSortedTiles[i].MOffsetX < _mMin.X)
                                _mMin.X = _mSortedTiles[i].MOffsetX;
                            if (_mSortedTiles[i].MOffsetX > _mMax.X)
                                _mMax.X = _mSortedTiles[i].MOffsetX;

                            if (_mSortedTiles[i].MOffsetY < _mMin.Y)
                                _mMin.Y = _mSortedTiles[i].MOffsetY;
                            if (_mSortedTiles[i].MOffsetY > _mMax.Y)
                                _mMax.Y = _mSortedTiles[i].MOffsetY;
                        }
                    }
                    break;
            }
            ConvertList();
        }

        public MultiComponentList(List<MultiTileEntry> arr)
        {
            _mMin = _mMax = Point.Empty;
            int itemcount = arr.Count;
            _mSortedTiles = new MultiTileEntry[itemcount];
            _mMin.X = 10000;
            _mMin.Y = 10000;
            int i = 0;
            foreach (MultiTileEntry entry in arr)
            {
                if (entry.MOffsetX < _mMin.X)
                    _mMin.X = entry.MOffsetX;

                if (entry.MOffsetY < _mMin.Y)
                    _mMin.Y = entry.MOffsetY;

                if (entry.MOffsetX > _mMax.X)
                    _mMax.X = entry.MOffsetX;

                if (entry.MOffsetY > _mMax.Y)
                    _mMax.Y = entry.MOffsetY;

                if (entry.MOffsetZ > _mMaxHeight)
                    _mMaxHeight = entry.MOffsetZ;
                _mSortedTiles[i] = entry;

                ++i;
            }
            arr.Clear();
            int centerx = _mMax.X - (int)(Math.Round((_mMax.X - _mMin.X) / 2.0));
            int centery = _mMax.Y - (int)(Math.Round((_mMax.Y - _mMin.Y) / 2.0));

            _mMin = _mMax = Point.Empty;
            for (i = 0; i < _mSortedTiles.Length; ++i)
            {
                _mSortedTiles[i].MOffsetX -= (short)centerx;
                _mSortedTiles[i].MOffsetY -= (short)centery;
                if (_mSortedTiles[i].MOffsetX < _mMin.X)
                    _mMin.X = _mSortedTiles[i].MOffsetX;
                if (_mSortedTiles[i].MOffsetX > _mMax.X)
                    _mMax.X = _mSortedTiles[i].MOffsetX;

                if (_mSortedTiles[i].MOffsetY < _mMin.Y)
                    _mMin.Y = _mSortedTiles[i].MOffsetY;
                if (_mSortedTiles[i].MOffsetY > _mMax.Y)
                    _mMax.Y = _mSortedTiles[i].MOffsetY;
            }
            ConvertList();
        }

        public MultiComponentList(StreamReader stream, int count)
        {
            string line;
            int itemcount = 0;
            _mMin = _mMax = Point.Empty;
            _mSortedTiles = new MultiTileEntry[count];
            _mMin.X = 10000;
            _mMin.Y = 10000;

            while ((line = stream.ReadLine()) != null)
            {
                string[] split = Regex.Split(line, @"\s+");
                _mSortedTiles[itemcount].MItemId = Convert.ToUInt16(split[0]);
                _mSortedTiles[itemcount].MFlags = Convert.ToInt32(split[1]);
                _mSortedTiles[itemcount].MOffsetX = Convert.ToInt16(split[2]);
                _mSortedTiles[itemcount].MOffsetY = Convert.ToInt16(split[3]);
                _mSortedTiles[itemcount].MOffsetZ = Convert.ToInt16(split[4]);
                _mSortedTiles[itemcount].MUnk1 = 0;

                MultiTileEntry e = _mSortedTiles[itemcount];

                if (e.MOffsetX < _mMin.X)
                    _mMin.X = e.MOffsetX;
                if (e.MOffsetY < _mMin.Y)
                    _mMin.Y = e.MOffsetY;
                if (e.MOffsetX > _mMax.X)
                    _mMax.X = e.MOffsetX;
                if (e.MOffsetY > _mMax.Y)
                    _mMax.Y = e.MOffsetY;
                if (e.MOffsetZ > _mMaxHeight)
                    _mMaxHeight = e.MOffsetZ;

                ++itemcount;
                if (itemcount == count)
                    break;

            }
            int centerx = _mMax.X - (int)(Math.Round((_mMax.X - _mMin.X) / 2.0));
            int centery = _mMax.Y - (int)(Math.Round((_mMax.Y - _mMin.Y) / 2.0));

            _mMin = _mMax = Point.Empty;
            int i = 0;
            for (; i < _mSortedTiles.Length; i++)
            {
                _mSortedTiles[i].MOffsetX -= (short)centerx;
                _mSortedTiles[i].MOffsetY -= (short)centery;
                if (_mSortedTiles[i].MOffsetX < _mMin.X)
                    _mMin.X = _mSortedTiles[i].MOffsetX;
                if (_mSortedTiles[i].MOffsetX > _mMax.X)
                    _mMax.X = _mSortedTiles[i].MOffsetX;

                if (_mSortedTiles[i].MOffsetY < _mMin.Y)
                    _mMin.Y = _mSortedTiles[i].MOffsetY;
                if (_mSortedTiles[i].MOffsetY > _mMax.Y)
                    _mMax.Y = _mSortedTiles[i].MOffsetY;
            }
            ConvertList();
        }

        private void ConvertList()
        {
            _mCenter = new Point(-_mMin.X, -_mMin.Y);
            _mWidth = (_mMax.X - _mMin.X) + 1;
            _mHeight = (_mMax.Y - _mMin.Y) + 1;

            MTileList[][] tiles = new MTileList[_mWidth][];
            _mTiles = new MTile[_mWidth][][];

            for (int x = 0; x < _mWidth; ++x)
            {
                tiles[x] = new MTileList[_mHeight];
                _mTiles[x] = new MTile[_mHeight][];

                for (int y = 0; y < _mHeight; ++y)
                    tiles[x][y] = new MTileList();
            }

            for (int i = 0; i < _mSortedTiles.Length; ++i)
            {
                int xOffset = _mSortedTiles[i].MOffsetX + _mCenter.X;
                int yOffset = _mSortedTiles[i].MOffsetY + _mCenter.Y;

                tiles[xOffset][yOffset].Add(_mSortedTiles[i].MItemId, (sbyte)_mSortedTiles[i].MOffsetZ, (sbyte)_mSortedTiles[i].MFlags, _mSortedTiles[i].MUnk1);
            }

            _mSurface = 0;

            for (int x = 0; x < _mWidth; ++x)
            {
                for (int y = 0; y < _mHeight; ++y)
                {
                    _mTiles[x][y] = tiles[x][y].ToArray();
                    for (int i = 0; i < _mTiles[x][y].Length; ++i)
                        _mTiles[x][y][i].Solver = i;
                    if (_mTiles[x][y].Length > 1)
                        Array.Sort(_mTiles[x][y]);
                    if (_mTiles[x][y].Length > 0)
                        ++_mSurface;
                }
            }
        }

        public MultiComponentList(MTileList[][] newtiles, int count, int width, int height)
        {
            _mMin = _mMax = Point.Empty;
            _mSortedTiles = new MultiTileEntry[count];
            _mCenter = new Point((int)(Math.Round((width / 2.0))) - 1, (int)(Math.Round((height / 2.0))) - 1);
            if (_mCenter.X < 0)
                _mCenter.X = width / 2;
            if (_mCenter.Y < 0)
                _mCenter.Y = height / 2;
            _mMaxHeight = -128;

            int counter = 0;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    MTile[] tiles = newtiles[x][y].ToArray();
                    for (int i = 0; i < tiles.Length; ++i)
                    {
                        _mSortedTiles[counter].MItemId = tiles[i].Id;
                        _mSortedTiles[counter].MOffsetX = (short)(x - _mCenter.X);
                        _mSortedTiles[counter].MOffsetY = (short)(y - _mCenter.Y);
                        _mSortedTiles[counter].MOffsetZ = (short)(tiles[i].Z);
                        _mSortedTiles[counter].MFlags = tiles[i].Flag;
                        _mSortedTiles[counter].MUnk1 = 0;

                        if (_mSortedTiles[counter].MOffsetX < _mMin.X)
                            _mMin.X = _mSortedTiles[counter].MOffsetX;
                        if (_mSortedTiles[counter].MOffsetX > _mMax.X)
                            _mMax.X = _mSortedTiles[counter].MOffsetX;
                        if (_mSortedTiles[counter].MOffsetY < _mMin.Y)
                            _mMin.Y = _mSortedTiles[counter].MOffsetY;
                        if (_mSortedTiles[counter].MOffsetY > _mMax.Y)
                            _mMax.Y = _mSortedTiles[counter].MOffsetY;
                        if (_mSortedTiles[counter].MOffsetZ > _mMaxHeight)
                            _mMaxHeight = _mSortedTiles[counter].MOffsetZ;
                        ++counter;
                    }
                }
            }
            ConvertList();
        }

        private MultiComponentList()
        {
            _mTiles = new MTile[0][][];
        }

        public void ExportToTextFile(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                for (int i = 0; i < _mSortedTiles.Length; ++i)
                {
                    tex.WriteLine(
                        $"0x{_mSortedTiles[i].MItemId:X} {_mSortedTiles[i].MOffsetX} {_mSortedTiles[i].MOffsetY} {_mSortedTiles[i].MOffsetZ} {_mSortedTiles[i].MFlags}");
                }
            }
        }

        public void ExportToWscFile(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                for (int i = 0; i < _mSortedTiles.Length; ++i)
                {
                    tex.WriteLine($"SECTION WORLDITEM {i}");
                    tex.WriteLine("{");
                    tex.WriteLine($"\tID\t{_mSortedTiles[i].MItemId}");
                    tex.WriteLine($"\tX\t{_mSortedTiles[i].MOffsetX}");
                    tex.WriteLine($"\tY\t{_mSortedTiles[i].MOffsetY}");
                    tex.WriteLine($"\tZ\t{_mSortedTiles[i].MOffsetZ}");
                    tex.WriteLine("\tColor\t0");
                    tex.WriteLine("}");

                }
            }
        }

        public void ExportToUoaFile(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("6 version");
                tex.WriteLine("1 template id");
                tex.WriteLine("-1 item version");
                tex.WriteLine($"{_mSortedTiles.Length} num components");
                for (int i = 0; i < _mSortedTiles.Length; ++i)
                {
                    tex.WriteLine(
                        $"{_mSortedTiles[i].MItemId} {_mSortedTiles[i].MOffsetX} {_mSortedTiles[i].MOffsetY} {_mSortedTiles[i].MOffsetZ} {_mSortedTiles[i].MFlags}");
                }
            }
        }
    }
}