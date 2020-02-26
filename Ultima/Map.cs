using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public sealed class Map
    {
        private TileMatrix _mTiles;
        private readonly int _mFileIndex;
        private readonly int _mMapId;
        private int _mWidth;
        private readonly int _mHeight;
        private readonly string _mPath;

        private static bool _mUseDiff;

        public static bool UseDiff { get => _mUseDiff;
            set { _mUseDiff = value; Reload(); } }

        public static Map Felucca = new Map(0, 0, 6144, 4096);
        public static Map Trammel = new Map(0, 1, 6144, 4096);
        public static readonly Map Ilshenar = new Map(2, 2, 2304, 1600);
        public static readonly Map Malas = new Map(3, 3, 2560, 2048);
        public static readonly Map Tokuno = new Map(4, 4, 1448, 1448);
        public static readonly Map TerMur = new Map(5, 5, 1280, 4096);
        public static Map Custom;

        public static void StartUpSetDiff(bool value)
        {
            _mUseDiff = value;
        }

        public Map(int fileIndex, int mapId, int width, int height)
        {
            _mFileIndex = fileIndex;
            _mMapId = mapId;
            _mWidth = width;
            _mHeight = height;
            _mPath = null;
        }

        public Map(string path, int fileIndex, int mapId, int width, int height)
        {
            _mFileIndex = fileIndex;
            _mMapId = mapId;
            _mWidth = width;
            _mHeight = height;
            _mPath = path;
        }

        /// <summary>
        /// Sets cache-vars to null
        /// </summary>
        public static void Reload()
        {
            Felucca.Tiles.Dispose();
            Trammel.Tiles.Dispose();
            Ilshenar.Tiles.Dispose();
            Malas.Tiles.Dispose();
            Tokuno.Tiles.Dispose();
            TerMur.Tiles.Dispose();
            Felucca.Tiles.StaticIndexInit = false;
            Trammel.Tiles.StaticIndexInit = false;
            Ilshenar.Tiles.StaticIndexInit = false;
            Malas.Tiles.StaticIndexInit = false;
            Tokuno.Tiles.StaticIndexInit = false;
            TerMur.Tiles.StaticIndexInit = false;
            Felucca._mCache = Trammel._mCache = Ilshenar._mCache = Malas._mCache = Tokuno._mCache = TerMur._mCache = null;
            Felucca._mTiles = Trammel._mTiles = Ilshenar._mTiles = Malas._mTiles = Tokuno._mTiles = TerMur._mTiles = null;
            Felucca._mCacheNoStatics = Trammel._mCacheNoStatics = Ilshenar._mCacheNoStatics = Malas._mCacheNoStatics = Tokuno._mCacheNoStatics = TerMur._mCacheNoStatics = null;
            Felucca._mCacheNoPatch = Trammel._mCacheNoPatch = Ilshenar._mCacheNoPatch = Malas._mCacheNoPatch = Tokuno._mCacheNoPatch = TerMur._mCacheNoPatch = null;
            Felucca._mCacheNoStaticsNoPatch = Trammel._mCacheNoStaticsNoPatch = Ilshenar._mCacheNoStaticsNoPatch = Malas._mCacheNoStaticsNoPatch = Tokuno._mCacheNoStaticsNoPatch = TerMur._mCacheNoStaticsNoPatch = null;
        }

        public void ResetCache()
        {
            _mCache = null;
            _mCacheNoPatch = null;
            _mCacheNoStatics = null;
            _mCacheNoStaticsNoPatch = null;
            _isCachedDefault = false;
            _isCachedNoStatics = false;
            _isCachedNoPatch = false;
            _isCachedNoStaticsNoPatch = false;
        }

        public bool LoadedMatrix => (_mTiles != null);

        public TileMatrix Tiles
        {
            get
            {
                if (_mTiles == null)
                    _mTiles = new TileMatrix(_mFileIndex, _mMapId, _mWidth, _mHeight, _mPath);

                return _mTiles;
            }
        }

        public int Width
        {
            get => _mWidth;
            set => _mWidth = value;
        }

        public int Height => _mHeight;

        public int FileIndex => _mFileIndex;

        /// <summary>
        /// Returns Bitmap with Statics
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <returns></returns>
        public Bitmap GetImage(int x, int y, int width, int height)
        {
            return GetImage(x, y, width, height, true);
        }

        /// <summary>
        /// Returns Bitmap
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <param name="statics">8x8 Block</param>
        /// <returns></returns>
        public Bitmap GetImage(int x, int y, int width, int height, bool statics)
        {
            Bitmap bmp = new Bitmap(width << 3, height << 3, PixelFormat.Format16bppRgb555);

            GetImage(x, y, width, height, bmp, statics);

            return bmp;
        }

        private bool _isCachedDefault;
        private bool _isCachedNoStatics;
        private bool _isCachedNoPatch;
        private bool _isCachedNoStaticsNoPatch;

        private short[][][] _mCache;
        private short[][][] _mCacheNoStatics;
        private short[][][] _mCacheNoPatch;
        private short[][][] _mCacheNoStaticsNoPatch;
        private short[] _mBlack;

        public bool IsCached(bool statics)
        {
            if (UseDiff)
            {
                if (!statics)
                    return _isCachedNoStatics;
                else
                    return _isCachedDefault;
            }
            else
            {
                if (!statics)
                    return _isCachedNoStaticsNoPatch;
                else
                    return _isCachedNoPatch;
            }
        }

        public void PreloadRenderedBlock(int x, int y, bool statics)
        {
            TileMatrix matrix = Tiles;

            if (x < 0 || y < 0 || x >= matrix.BlockWidth || y >= matrix.BlockHeight)
            {
                if (_mBlack == null)
                    _mBlack = new short[64];
                return;
            }

            short[][][] cache;
            if (UseDiff)
            {
                if (statics)
                    _isCachedDefault = true;
                else
                    _isCachedNoStatics = true;
                cache = (statics ? _mCache : _mCacheNoStatics);
            }
            else
            {
                if (statics)
                    _isCachedNoPatch = true;
                else
                    _isCachedNoStaticsNoPatch = true;
                cache = (statics ? _mCacheNoPatch : _mCacheNoStaticsNoPatch);
            }

            if (cache == null)
            {
                if (UseDiff)
                {
                    if (statics)
                        _mCache = cache = new short[_mTiles.BlockHeight][][];
                    else
                        _mCacheNoStatics = cache = new short[_mTiles.BlockHeight][][];
                }
                else
                {
                    if (statics)
                        _mCacheNoPatch = cache = new short[_mTiles.BlockHeight][][];
                    else
                        _mCacheNoStaticsNoPatch = cache = new short[_mTiles.BlockHeight][][];
                }
            }

            if (cache[y] == null)
                cache[y] = new short[_mTiles.BlockWidth][];

            if (cache[y][x] == null)
                cache[y][x] = RenderBlock(x, y, statics, UseDiff);

            _mTiles.CloseStreams();
        }

        private short[] GetRenderedBlock(int x, int y, bool statics)
        {
            TileMatrix matrix = Tiles;

            if (x < 0 || y < 0 || x >= matrix.BlockWidth || y >= matrix.BlockHeight)
            {
                if (_mBlack == null)
                    _mBlack = new short[64];

                return _mBlack;
            }

            short[][][] cache;
            if (UseDiff)
                cache = (statics ? _mCache : _mCacheNoStatics);
            else
                cache = (statics ? _mCacheNoPatch : _mCacheNoStaticsNoPatch);

            if (cache == null)
            {
                if (UseDiff)
                {
                    if (statics)
                        _mCache = cache = new short[_mTiles.BlockHeight][][];
                    else
                        _mCacheNoStatics = cache = new short[_mTiles.BlockHeight][][];
                }
                else
                {
                    if (statics)
                        _mCacheNoPatch = cache = new short[_mTiles.BlockHeight][][];
                    else
                        _mCacheNoStaticsNoPatch = cache = new short[_mTiles.BlockHeight][][];
                }
            }

            if (cache[y] == null)
                cache[y] = new short[_mTiles.BlockWidth][];

            short[] data = cache[y][x];

            if (data == null)
                cache[y][x] = data = RenderBlock(x, y, statics, UseDiff);

            return data;
        }

        private unsafe short[] RenderBlock(int x, int y, bool drawStatics, bool diff)
        {
            short[] data = new short[64];

            Tile[] tiles = _mTiles.GetLandBlock(x, y, diff);

            fixed (short* pColors = RadarCol.Colors)
            {
                fixed (int* pHeight = TileData.HeightTable)
                {
                    fixed (Tile* ptTiles = tiles)
                    {
                        Tile* pTiles = ptTiles;

                        fixed (short* pData = data)
                        {
                            short* pvData = pData;

                            if (drawStatics)
                            {
                                HuedTile[][][] statics = drawStatics ? _mTiles.GetStaticBlock(x, y, diff) : null;

                                for (int k = 0, v = 0; k < 8; ++k, v += 8)
                                {
                                    for (int p = 0; p < 8; ++p)
                                    {
                                        int highTop = -255;
                                        int highZ = -255;
                                        int highId = 0;
                                        int highHue = 0;
                                        int z, top;
                                        bool highstatic = false;

                                        HuedTile[] curStatics = statics[p][k];

                                        if (curStatics.Length > 0)
                                        {
                                            fixed (HuedTile* phtStatics = curStatics)
                                            {
                                                HuedTile* pStatics = phtStatics;
                                                HuedTile* pStaticsEnd = pStatics + curStatics.Length;

                                                while (pStatics < pStaticsEnd)
                                                {
                                                    z = pStatics->m_Z;
                                                    top = z + pHeight[pStatics->Id];

                                                    if (top > highTop || (z > highZ && top >= highTop))
                                                    {
                                                        highTop = top;
                                                        highZ = z;
                                                        highId = pStatics->Id;
                                                        highHue = pStatics->Hue;
                                                        highstatic = true;
                                                    }

                                                    ++pStatics;
                                                }
                                            }
                                        }
                                        StaticTile[] pending = _mTiles.GetPendingStatics(x, y);
                                        if (pending != null)
                                        {
                                            foreach (StaticTile penS in pending)
                                            {
                                                if (penS.m_X == p)
                                                {
                                                    if (penS.m_Y == k)
                                                    {
                                                        z = penS.m_Z;
                                                        top = z + pHeight[penS.m_ID];

                                                        if (top > highTop || (z > highZ && top >= highTop))
                                                        {
                                                            highTop = top;
                                                            highZ = z;
                                                            highId = penS.m_ID;
                                                            highHue = penS.m_Hue;
                                                            highstatic = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        top = pTiles->m_Z;

                                        if (top > highTop)
                                        {
                                            highId = pTiles->m_ID;
                                            highHue = 0;
                                            highstatic = false;
                                        }

                                        if (highHue == 0)
                                        {
                                            try
                                            {
                                                if (highstatic)
                                                    *pvData++ = pColors[highId + 0x4000];
                                                else
                                                    *pvData++ = pColors[highId];
                                            }
                                            catch { }
                                        }
                                        else
                                            *pvData++ = Hues.GetHue(highHue - 1).Colors[(pColors[highId + 0x4000] >> 10) & 0x1F];

                                        ++pTiles;
                                    }
                                }
                            }
                            else
                            {
                                Tile* pEnd = pTiles + 64;

                                while (pTiles < pEnd)
                                    *pvData++ = pColors[(pTiles++)->m_ID];
                            }
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Draws in given Bitmap with Statics 
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <param name="bmp">8x8 Block</param>
        public unsafe void GetImage(int x, int y, int width, int height, Bitmap bmp)
        {
            GetImage(x, y, width, height, bmp, true);
        }

        /// <summary>
        /// Draws in given Bitmap
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <param name="bmp"></param>
        /// <param name="statics"></param>
        public unsafe void GetImage(int x, int y, int width, int height, Bitmap bmp, bool statics)
        {
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width << 3, height << 3), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb555);
            int stride = bd.Stride;
            int blockStride = stride << 3;

            byte* pStart = (byte*)bd.Scan0;

            for (int oy = 0, by = y; oy < height; ++oy, ++by, pStart += blockStride)
            {

                int* pRow0 = (int*)(pStart + (0 * stride));
                int* pRow1 = (int*)(pStart + (1 * stride));
                int* pRow2 = (int*)(pStart + (2 * stride));
                int* pRow3 = (int*)(pStart + (3 * stride));
                int* pRow4 = (int*)(pStart + (4 * stride));
                int* pRow5 = (int*)(pStart + (5 * stride));
                int* pRow6 = (int*)(pStart + (6 * stride));
                int* pRow7 = (int*)(pStart + (7 * stride));

                for (int ox = 0, bx = x; ox < width; ++ox, ++bx)
                {
                    short[] data = GetRenderedBlock(bx, by, statics);

                    fixed (short* pData = data)
                    {
                        int* pvData = (int*)pData;

                        *pRow0++ = *pvData++;
                        *pRow0++ = *pvData++;
                        *pRow0++ = *pvData++;
                        *pRow0++ = *pvData++;

                        *pRow1++ = *pvData++;
                        *pRow1++ = *pvData++;
                        *pRow1++ = *pvData++;
                        *pRow1++ = *pvData++;

                        *pRow2++ = *pvData++;
                        *pRow2++ = *pvData++;
                        *pRow2++ = *pvData++;
                        *pRow2++ = *pvData++;

                        *pRow3++ = *pvData++;
                        *pRow3++ = *pvData++;
                        *pRow3++ = *pvData++;
                        *pRow3++ = *pvData++;

                        *pRow4++ = *pvData++;
                        *pRow4++ = *pvData++;
                        *pRow4++ = *pvData++;
                        *pRow4++ = *pvData++;

                        *pRow5++ = *pvData++;
                        *pRow5++ = *pvData++;
                        *pRow5++ = *pvData++;
                        *pRow5++ = *pvData++;

                        *pRow6++ = *pvData++;
                        *pRow6++ = *pvData++;
                        *pRow6++ = *pvData++;
                        *pRow6++ = *pvData++;

                        *pRow7++ = *pvData++;
                        *pRow7++ = *pvData++;
                        *pRow7++ = *pvData++;
                        *pRow7++ = *pvData++;
                    }
                }
            }

            bmp.UnlockBits(bd);
            _mTiles.CloseStreams();
        }

        public static void DefragStatics(string path, Map map, int width, int height, bool remove)
        {
            string indexPath = Files.GetFilePath("staidx{0}.mul", map.FileIndex);
            FileStream mIndex;
            BinaryReader mIndexReader;
            if (indexPath != null)
            {
                mIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                mIndexReader = new BinaryReader(mIndex);
            }
            else
                return;

            string staticsPath = Files.GetFilePath("statics{0}.mul", map.FileIndex);

            FileStream mStatics;
            BinaryReader mStaticsReader;
            if (staticsPath != null)
            {
                mStatics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                mStaticsReader = new BinaryReader(mStatics);
            }
            else
                return;


            int blockx = width >> 3;
            int blocky = height >> 3;

            string idx = Path.Combine(path, $"staidx{map.FileIndex}.mul");
            string mul = Path.Combine(path, $"statics{map.FileIndex}.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                MemoryStream memidx = new MemoryStream();
                MemoryStream memmul = new MemoryStream();
                using (BinaryWriter binidx = new BinaryWriter(memidx),
                                    binmul = new BinaryWriter(memmul))
                {
                    for (int x = 0; x < blockx; ++x)
                    {
                        for (int y = 0; y < blocky; ++y)
                        {
                            try
                            {
                                mIndexReader.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                int lookup = mIndexReader.ReadInt32();
                                int length = mIndexReader.ReadInt32();
                                int extra = mIndexReader.ReadInt32();

                                if (((lookup < 0 || length <= 0)
                                    && (!map.Tiles.PendingStatic(x, y)))
                                    || (map.Tiles.IsStaticBlockRemoved(x, y)))
                                {
                                    binidx.Write(-1); // lookup
                                    binidx.Write(-1); // length
                                    binidx.Write(-1); // extra
                                }
                                else
                                {
                                    if ((lookup >= 0) && (length > 0))
                                        mStatics.Seek(lookup, SeekOrigin.Begin);

                                    int fsmullength = (int)binmul.BaseStream.Position;
                                    int count = length / 7;
                                    if (!remove) //without duplicate remove
                                    {
                                        bool firstitem = true;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            ushort graphic = mStaticsReader.ReadUInt16();
                                            byte sx = mStaticsReader.ReadByte();
                                            byte sy = mStaticsReader.ReadByte();
                                            sbyte sz = mStaticsReader.ReadSByte();
                                            short shue = mStaticsReader.ReadInt16();
                                            if ((graphic >= 0) && (graphic <= Art.GetMaxItemId()))
                                            {
                                                if (shue < 0)
                                                    shue = 0;
                                                if (firstitem)
                                                {
                                                    binidx.Write((int)binmul.BaseStream.Position); //lookup
                                                    firstitem = false;
                                                }
                                                binmul.Write(graphic);
                                                binmul.Write(sx);
                                                binmul.Write(sy);
                                                binmul.Write(sz);
                                                binmul.Write(shue);
                                            }
                                        }
                                        StaticTile[] tilelist = map.Tiles.GetPendingStatics(x, y);
                                        if (tilelist != null)
                                        {
                                            for (int i = 0; i < tilelist.Length; ++i)
                                            {
                                                if ((tilelist[i].m_ID >= 0) && (tilelist[i].m_ID <= Art.GetMaxItemId()))
                                                {
                                                    if (tilelist[i].m_Hue < 0)
                                                        tilelist[i].m_Hue = 0;
                                                    if (firstitem)
                                                    {
                                                        binidx.Write((int)binmul.BaseStream.Position); //lookup
                                                        firstitem = false;
                                                    }
                                                    binmul.Write(tilelist[i].m_ID);
                                                    binmul.Write(tilelist[i].m_X);
                                                    binmul.Write(tilelist[i].m_Y);
                                                    binmul.Write(tilelist[i].m_Z);
                                                    binmul.Write(tilelist[i].m_Hue);
                                                }
                                            }
                                        }
                                    }
                                    else //with duplicate remove
                                    {
                                        StaticTile[] tilelist = new StaticTile[count];
                                        int j = 0;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            StaticTile tile = new StaticTile();
                                            tile.m_ID = mStaticsReader.ReadUInt16();
                                            tile.m_X = mStaticsReader.ReadByte();
                                            tile.m_Y = mStaticsReader.ReadByte();
                                            tile.m_Z = mStaticsReader.ReadSByte();
                                            tile.m_Hue = mStaticsReader.ReadInt16();

                                            if ((tile.m_ID >= 0) && (tile.m_ID <= Art.GetMaxItemId()))
                                            {
                                                if (tile.m_Hue < 0)
                                                    tile.m_Hue = 0;
                                                bool first = true;
                                                for (int k = 0; k < j; ++k)
                                                {
                                                    if ((tilelist[k].m_ID == tile.m_ID)
                                                        && ((tilelist[k].m_X == tile.m_X) && (tilelist[k].m_Y == tile.m_Y))
                                                        && (tilelist[k].m_Z == tile.m_Z)
                                                        && (tilelist[k].m_Hue == tile.m_Hue))
                                                    {
                                                        first = false;
                                                        break;
                                                    }
                                                }
                                                if (first)
                                                {
                                                    tilelist[j] = tile;
                                                    j++;
                                                }
                                            }
                                        }
                                        if (map.Tiles.PendingStatic(x, y))
                                        {
                                            StaticTile[] pending = map.Tiles.GetPendingStatics(x, y);
                                            StaticTile[] old = tilelist;
                                            tilelist = new StaticTile[old.Length + pending.Length];
                                            old.CopyTo(tilelist, 0);
                                            for (int i = 0; i < pending.Length; ++i)
                                            {
                                                if ((pending[i].m_ID >= 0) && (pending[i].m_ID <= Art.GetMaxItemId()))
                                                {
                                                    if (pending[i].m_Hue < 0)
                                                        pending[i].m_Hue = 0;
                                                    bool first = true;
                                                    for (int k = 0; k < j; ++k)
                                                    {
                                                        if ((tilelist[k].m_ID == pending[i].m_ID)
                                                            && ((tilelist[k].m_X == pending[i].m_X) && (tilelist[k].m_Y == pending[i].m_Y))
                                                            && (tilelist[k].m_Z == pending[i].m_Z)
                                                            && (tilelist[k].m_Hue == pending[i].m_Hue))
                                                        {
                                                            first = false;
                                                            break;
                                                        }
                                                    }
                                                    if (first)
                                                        tilelist[j++] = pending[i];
                                                }
                                            }
                                        }
                                        if (j > 0)
                                        {
                                            binidx.Write((int)binmul.BaseStream.Position); //lookup
                                            for (int i = 0; i < j; ++i)
                                            {
                                                binmul.Write(tilelist[i].m_ID);
                                                binmul.Write(tilelist[i].m_X);
                                                binmul.Write(tilelist[i].m_Y);
                                                binmul.Write(tilelist[i].m_Z);
                                                binmul.Write(tilelist[i].m_Hue);
                                            }
                                        }
                                    }

                                    fsmullength = (int)binmul.BaseStream.Position - fsmullength;
                                    if (fsmullength > 0)
                                    {
                                        binidx.Write(fsmullength); //length
                                        if (extra == -1)
                                            extra = 0;
                                        binidx.Write(extra); //extra
                                    }
                                    else
                                    {
                                        binidx.Write(-1); //lookup
                                        binidx.Write(-1); //length
                                        binidx.Write(-1); //extra
                                    }
                                }
                            }
                            catch // fill the rest
                            {
                                binidx.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                for (; x < blockx; ++x)
                                {
                                    for (; y < blocky; ++y)
                                    {
                                        binidx.Write(-1); //lookup
                                        binidx.Write(-1); //length
                                        binidx.Write(-1); //extra
                                    }
                                    y = 0;
                                }
                            }
                        }
                    }
                    memidx.WriteTo(fsidx);
                    memmul.WriteTo(fsmul);
                }
            }
            mIndexReader.Close();
            mStaticsReader.Close();
        }

        public static void RewriteMap(string path, int map, int width, int height)
        {
            string mapPath = Files.GetFilePath("map{0}.mul", map);
            FileStream mMap;
            BinaryReader mMapReader;
            if (mapPath != null)
            {
                mMap = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                mMapReader = new BinaryReader(mMap);
            }
            else
                return;

            int blockx = width >> 3;
            int blocky = height >> 3;

            string mul = Path.Combine(path, $"map{map}.mul");
            using (FileStream fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                MemoryStream memmul = new MemoryStream();
                using (BinaryWriter binmul = new BinaryWriter(memmul))
                {
                    for (int x = 0; x < blockx; ++x)
                    {
                        for (int y = 0; y < blocky; ++y)
                        {
                            try
                            {
                                mMapReader.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                int header = mMapReader.ReadInt32();
                                binmul.Write(header);
                                for (int i = 0; i < 64; ++i)
                                {
                                    short tileid = mMapReader.ReadInt16();
                                    sbyte z = mMapReader.ReadSByte();
                                    if ((tileid < 0) || (tileid >= 0x4000))
                                        tileid = 0;
                                    if (z < -128)
                                        z = -128;
                                    if (z > 127)
                                        z = 127;
                                    binmul.Write(tileid);
                                    binmul.Write(z);
                                }
                            }
                            catch //fill rest
                            {
                                binmul.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                for (; x < blockx; ++x)
                                {
                                    for (; y < blocky; ++y)
                                    {
                                        binmul.Write(0);
                                        for (int i = 0; i < 64; ++i)
                                        {
                                            binmul.Write((short)0);
                                            binmul.Write((sbyte)0);
                                        }
                                    }
                                    y = 0;
                                }
                            }
                        }
                    }
                    memmul.WriteTo(fsmul);
                }
            }
            mMapReader.Close();
        }

        public void ReportInvisStatics(string reportfile)
        {
            reportfile = Path.Combine(reportfile, $"staticReport-{_mMapId}.csv");
            using (StreamWriter tex = new StreamWriter(new FileStream(reportfile, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("x;y;z;Static");
                for (int x = 0; x < _mWidth; ++x)
                {
                    for (int y = 0; y < _mHeight; ++y)
                    {
                        Tile currtile = Tiles.GetLandTile(x, y);
                        foreach (HuedTile currstatic in Tiles.GetStaticTiles(x, y))
                        {
                            if (currstatic.Z < currtile.Z)
                            {
                                if (TileData.ItemTable[currstatic.Id].Height + currstatic.Z < currtile.Z)
                                    tex.WriteLine($"{x};{y};{currstatic.Z};0x{currstatic.Id:X}");
                            }
                        }
                    }
                }
            }
        }

        public void ReportInvalidMapIDs(string reportfile)
        {
            reportfile = Path.Combine(reportfile, $"ReportInvalidMapIDs-{_mMapId}.csv");
            using (StreamWriter tex = new StreamWriter(new FileStream(reportfile, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("x;y;z;Static;LandTile");
                for (int x = 0; x < _mWidth; ++x)
                {
                    for (int y = 0; y < _mHeight; ++y)
                    {
                        Tile currtile = Tiles.GetLandTile(x, y);
                        if (!Art.IsValidLand(currtile.Id))
                            tex.WriteLine($"{x};{y};{currtile.Z};0;0x{currtile.Id:X}");
                        foreach (HuedTile currstatic in Tiles.GetStaticTiles(x, y))
                        {
                            if (!Art.IsValidStatic(currstatic.Id))
                                tex.WriteLine($"{x};{y};{currstatic.Z};0x{currstatic.Id:X};0");
                        }
                    }
                }
            }
        }
    }
}