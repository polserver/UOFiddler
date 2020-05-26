using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Ultima
{
    public sealed class Map
    {
        private TileMatrix _tiles;
        private readonly int _mapId;
        private readonly string _path;
        private static bool _useDiff;

        public static bool UseDiff
        {
            get { return _useDiff; }
            set
            {
                _useDiff = value;
                Reload();
            }
        }

        public static Map Felucca = new Map(0, 0, 6144, 4096);
        public static Map Trammel = new Map(0, 1, 6144, 4096);
        public static readonly Map Ilshenar = new Map(2, 2, 2304, 1600);
        public static readonly Map Malas = new Map(3, 3, 2560, 2048);
        public static readonly Map Tokuno = new Map(4, 4, 1448, 1448);
        public static readonly Map TerMur = new Map(5, 5, 1280, 4096);
        public static Map Custom;

        public static void StartUpSetDiff(bool value)
        {
            _useDiff = value;
        }

        public Map(int fileIndex, int mapId, int width, int height)
        {
            FileIndex = fileIndex;
            _mapId = mapId;
            Width = width;
            Height = height;
            _path = null;
        }

        public Map(string path, int fileIndex, int mapId, int width, int height)
        {
            FileIndex = fileIndex;
            _mapId = mapId;
            Width = width;
            Height = height;
            _path = path;
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

            Felucca._cache = Trammel._cache = Ilshenar._cache = Malas._cache = Tokuno._cache = TerMur._cache = null;
            Felucca._tiles = Trammel._tiles = Ilshenar._tiles = Malas._tiles = Tokuno._tiles = TerMur._tiles = null;
            Felucca._cacheNoStatics =
                Trammel._cacheNoStatics =
                Ilshenar._cacheNoStatics = Malas._cacheNoStatics = Tokuno._cacheNoStatics = TerMur._cacheNoStatics = null;
            Felucca._cacheNoPatch =
                Trammel._cacheNoPatch =
                Ilshenar._cacheNoPatch = Malas._cacheNoPatch = Tokuno._cacheNoPatch = TerMur._cacheNoPatch = null;
            Felucca._cacheNoStaticsNoPatch =
                Trammel._cacheNoStaticsNoPatch =
                Ilshenar._cacheNoStaticsNoPatch =
                Malas._cacheNoStaticsNoPatch = Tokuno._cacheNoStaticsNoPatch = TerMur._cacheNoStaticsNoPatch = null;
        }

        public void ResetCache()
        {
            _cache = null;
            _cacheNoPatch = null;
            _cacheNoStatics = null;
            _cacheNoStaticsNoPatch = null;

            _isCachedDefault = false;
            _isCachedNoStatics = false;
            _isCachedNoPatch = false;
            _isCachedNoStaticsNoPatch = false;
        }

/*
 // TODO: unused?
        public bool LoadedMatrix { get { return _tiles != null; } }
*/

        public TileMatrix Tiles
        {
            get
            {
                return _tiles ?? (_tiles = new TileMatrix(FileIndex, _mapId, Width, Height, _path));
            }
        }

        public int Width { get; set; }

        public int Height { get; }

        public int FileIndex { get; }

/*
 // TODO: unused?
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
*/

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
            var bmp = new Bitmap(width << 3, height << 3, PixelFormat.Format16bppRgb555);

            GetImage(x, y, width, height, bmp, statics);

            return bmp;
        }

        private bool _isCachedDefault;
        private bool _isCachedNoStatics;
        private bool _isCachedNoPatch;
        private bool _isCachedNoStaticsNoPatch;

        private short[][][] _cache;
        private short[][][] _cacheNoStatics;
        private short[][][] _cacheNoPatch;
        private short[][][] _cacheNoStaticsNoPatch;
        private short[] _black;

        public bool IsCached(bool statics)
        {
            if (UseDiff)
            {
                return !statics ? _isCachedNoStatics : _isCachedDefault;
            }

            return !statics ? _isCachedNoStaticsNoPatch : _isCachedNoPatch;
        }

        public void PreloadRenderedBlock(int x, int y, bool statics)
        {
            TileMatrix matrix = Tiles;

            if (x < 0 || y < 0 || x >= matrix.BlockWidth || y >= matrix.BlockHeight)
            {
                if (_black == null)
                {
                    _black = new short[64];
                }

                return;
            }

            short[][][] cache;
            if (UseDiff)
            {
                if (statics)
                {
                    _isCachedDefault = true;
                }
                else
                {
                    _isCachedNoStatics = true;
                }

                cache = (statics ? _cache : _cacheNoStatics);
            }
            else
            {
                if (statics)
                {
                    _isCachedNoPatch = true;
                }
                else
                {
                    _isCachedNoStaticsNoPatch = true;
                }

                cache = (statics ? _cacheNoPatch : _cacheNoStaticsNoPatch);
            }

            if (cache == null)
            {
                if (UseDiff)
                {
                    if (statics)
                    {
                        _cache = cache = new short[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStatics = cache = new short[_tiles.BlockHeight][][];
                    }
                }
                else
                {
                    if (statics)
                    {
                        _cacheNoPatch = cache = new short[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStaticsNoPatch = cache = new short[_tiles.BlockHeight][][];
                    }
                }
            }

            if (cache[y] == null)
            {
                cache[y] = new short[_tiles.BlockWidth][];
            }

            if (cache[y][x] == null)
            {
                cache[y][x] = RenderBlock(x, y, statics, UseDiff);
            }

            _tiles.CloseStreams();
        }

        private short[] GetRenderedBlock(int x, int y, bool statics)
        {
            TileMatrix matrix = Tiles;

            if (x < 0 || y < 0 || x >= matrix.BlockWidth || y >= matrix.BlockHeight)
            {
                return _black ?? (_black = new short[64]);
            }

            short[][][] cache;
            if (UseDiff)
            {
                cache = (statics ? _cache : _cacheNoStatics);
            }
            else
            {
                cache = (statics ? _cacheNoPatch : _cacheNoStaticsNoPatch);
            }

            if (cache == null)
            {
                if (UseDiff)
                {
                    if (statics)
                    {
                        _cache = cache = new short[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStatics = cache = new short[_tiles.BlockHeight][][];
                    }
                }
                else
                {
                    if (statics)
                    {
                        _cacheNoPatch = cache = new short[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStaticsNoPatch = cache = new short[_tiles.BlockHeight][][];
                    }
                }
            }

            if (cache[y] == null)
            {
                cache[y] = new short[_tiles.BlockWidth][];
            }

            short[] data = cache[y][x];

            if (data == null)
            {
                cache[y][x] = data = RenderBlock(x, y, statics, UseDiff);
            }

            return data;
        }

        private unsafe short[] RenderBlock(int x, int y, bool drawStatics, bool diff)
        {
            var data = new short[64];

            Tile[] tiles = _tiles.GetLandBlock(x, y, diff);

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
                                // TODO: that drawStatics is already always true?
                                HuedTile[][][] statics = drawStatics ? _tiles.GetStaticBlock(x, y, diff) : null;

                                for (int k = 0, v = 0; k < 8; ++k, v += 8)
                                {
                                    for (int p = 0; p < 8; ++p)
                                    {
                                        int highTop = -255;
                                        int highZ = -255;
                                        int highId = 0;
                                        int highHue = 0;
                                        int z, top;
                                        bool highStatic = false;

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
                                                    top = z + pHeight[pStatics->ID];

                                                    if (top > highTop || (z > highZ && top >= highTop))
                                                    {
                                                        highTop = top;
                                                        highZ = z;
                                                        highId = pStatics->ID;
                                                        highHue = pStatics->Hue;
                                                        highStatic = true;
                                                    }

                                                    ++pStatics;
                                                }
                                            }
                                        }
                                        StaticTile[] pending = _tiles.GetPendingStatics(x, y);
                                        if (pending != null)
                                        {
                                            foreach (StaticTile penS in pending)
                                            {
                                                if (penS.m_X != p || penS.m_Y != k)
                                                {
                                                    continue;
                                                }

                                                z = penS.m_Z;
                                                top = z + pHeight[penS.m_ID];

                                                if (top <= highTop && (z <= highZ || top < highTop))
                                                {
                                                    continue;
                                                }

                                                highTop = top;
                                                highZ = z;
                                                highId = penS.m_ID;
                                                highHue = penS.m_Hue;
                                                highStatic = true;
                                            }
                                        }

                                        top = pTiles->m_Z;

                                        if (top > highTop)
                                        {
                                            highId = pTiles->m_ID;
                                            highHue = 0;
                                            highStatic = false;
                                        }

                                        if (highHue == 0)
                                        {
                                            try
                                            {
                                                if (highStatic)
                                                {
                                                    *pvData++ = pColors[highId + 0x4000];
                                                }
                                                else
                                                {
                                                    *pvData++ = pColors[highId];
                                                }
                                            }
                                            catch
                                            {
                                                // TODO: ignored?
                                                // ignored
                                            }
                                        }
                                        else
                                        {
                                            *pvData++ = Hues.GetHue(highHue - 1).Colors[(pColors[highId + 0x4000] >> 10) & 0x1F];
                                        }

                                        ++pTiles;
                                    }
                                }
                            }
                            else
                            {
                                Tile* pEnd = pTiles + 64;

                                while (pTiles < pEnd)
                                {
                                    *pvData++ = pColors[(pTiles++)->m_ID];
                                }
                            }
                        }
                    }
                }
            }

            return data;
        }

/*
 // TODO: unused?
        /// <summary>
        /// Draws in given Bitmap with Statics
        /// </summary>
        /// <param name="x">8x8 Block</param>
        /// <param name="y">8x8 Block</param>
        /// <param name="width">8x8 Block</param>
        /// <param name="height">8x8 Block</param>
        /// <param name="bmp">8x8 Block</param>
        public void GetImage(int x, int y, int width, int height, Bitmap bmp)
        {
            GetImage(x, y, width, height, bmp, true);
        }
*/

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
            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, width << 3, height << 3), ImageLockMode.WriteOnly, PixelFormat.Format16bppRgb555);
            int stride = bd.Stride;
            int blockStride = stride << 3;

            var pStart = (byte*)bd.Scan0;

            for (int oy = 0, by = y; oy < height; ++oy, ++by, pStart += blockStride)
            {
                var pRow0 = (int*)(pStart + (0 * stride));
                var pRow1 = (int*)(pStart + (1 * stride));
                var pRow2 = (int*)(pStart + (2 * stride));
                var pRow3 = (int*)(pStart + (3 * stride));
                var pRow4 = (int*)(pStart + (4 * stride));
                var pRow5 = (int*)(pStart + (5 * stride));
                var pRow6 = (int*)(pStart + (6 * stride));
                var pRow7 = (int*)(pStart + (7 * stride));

                for (int ox = 0, bx = x; ox < width; ++ox, ++bx)
                {
                    short[] data = GetRenderedBlock(bx, by, statics);

                    fixed (short* pData = data)
                    {
                        var pvData = (int*)pData;

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
            _tiles.CloseStreams();
        }

        public static void DefragStatics(string path, Map map, int width, int height, bool remove)
        {
            string indexPath = Files.GetFilePath("staidx{0}.mul", map.FileIndex);
            BinaryReader indexReader;
            if (indexPath != null)
            {
                FileStream index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                indexReader = new BinaryReader(index);
            }
            else
            {
                return;
            }

            string staticsPath = Files.GetFilePath("statics{0}.mul", map.FileIndex);

            FileStream staticsStream;
            BinaryReader staticsReader;
            if (staticsPath != null)
            {
                staticsStream = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                staticsReader = new BinaryReader(staticsStream);
            }
            else
            {
                return;
            }

            int blockx = width >> 3;
            int blocky = height >> 3;

            string idx = Path.Combine(path, $"staidx{map.FileIndex}.mul");
            string mul = Path.Combine(path, $"statics{map.FileIndex}.mul");

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var memidx = new MemoryStream();
                var memmul = new MemoryStream();
                using (var binidx = new BinaryWriter(memidx))
                using (var binmul = new BinaryWriter(memmul))
                {
                    for (int x = 0; x < blockx; ++x)
                    {
                        for (int y = 0; y < blocky; ++y)
                        {
                            try
                            {
                                indexReader.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                int lookup = indexReader.ReadInt32();
                                int length = indexReader.ReadInt32();
                                int extra = indexReader.ReadInt32();

                                if (((lookup < 0 || length <= 0) && (!map.Tiles.PendingStatic(x, y))) ||
                                    (map.Tiles.IsStaticBlockRemoved(x, y)))
                                {
                                    binidx.Write(-1); // lookup
                                    binidx.Write(-1); // length
                                    binidx.Write(-1); // extra
                                }
                                else
                                {
                                    if ((lookup >= 0) && (length > 0))
                                    {
                                        staticsStream.Seek(lookup, SeekOrigin.Begin);
                                    }

                                    var fsmullength = (int)binmul.BaseStream.Position;
                                    int count = length / 7;
                                    if (!remove) //without duplicate remove
                                    {
                                        bool firstitem = true;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            ushort graphic = staticsReader.ReadUInt16();
                                            byte sx = staticsReader.ReadByte();
                                            byte sy = staticsReader.ReadByte();
                                            sbyte sz = staticsReader.ReadSByte();
                                            short shue = staticsReader.ReadInt16();

                                            if (graphic > Art.GetMaxItemID())
                                            {
                                                continue;
                                            }

                                            if (shue < 0)
                                            {
                                                shue = 0;
                                            }

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

                                        StaticTile[] tileList = map.Tiles.GetPendingStatics(x, y);
                                        if (tileList != null)
                                        {
                                            for (int i = 0; i < tileList.Length; ++i)
                                            {
                                                if (tileList[i].m_ID > Art.GetMaxItemID())
                                                {
                                                    continue;
                                                }

                                                if (tileList[i].m_Hue < 0)
                                                {
                                                    tileList[i].m_Hue = 0;
                                                }

                                                if (firstitem)
                                                {
                                                    binidx.Write((int)binmul.BaseStream.Position); // lookup
                                                    firstitem = false;
                                                }

                                                binmul.Write(tileList[i].m_ID);
                                                binmul.Write(tileList[i].m_X);
                                                binmul.Write(tileList[i].m_Y);
                                                binmul.Write(tileList[i].m_Z);
                                                binmul.Write(tileList[i].m_Hue);
                                            }
                                        }
                                    }
                                    else // with duplicate remove
                                    {
                                        var tileList = new StaticTile[count];
                                        int j = 0;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            var tile = new StaticTile
                                            {
                                                m_ID = staticsReader.ReadUInt16(),
                                                m_X = staticsReader.ReadByte(),
                                                m_Y = staticsReader.ReadByte(),
                                                m_Z = staticsReader.ReadSByte(),
                                                m_Hue = staticsReader.ReadInt16()
                                            };

                                            if (tile.m_ID > Art.GetMaxItemID())
                                            {
                                                continue;
                                            }

                                            if (tile.m_Hue < 0)
                                            {
                                                tile.m_Hue = 0;
                                            }

                                            bool first = true;
                                            for (int k = 0; k < j; ++k)
                                            {
                                                if ((tileList[k].m_ID == tile.m_ID) &&
                                                    ((tileList[k].m_X == tile.m_X) &&
                                                     (tileList[k].m_Y == tile.m_Y)) &&
                                                    (tileList[k].m_Z == tile.m_Z) &&
                                                    (tileList[k].m_Hue == tile.m_Hue))
                                                {
                                                    first = false;
                                                    break;
                                                }
                                            }

                                            if (!first)
                                            {
                                                continue;
                                            }

                                            tileList[j] = tile;
                                            j++;
                                        }

                                        if (map.Tiles.PendingStatic(x, y))
                                        {
                                            StaticTile[] pending = map.Tiles.GetPendingStatics(x, y);
                                            StaticTile[] old = tileList;
                                            tileList = new StaticTile[old.Length + pending.Length];
                                            old.CopyTo(tileList, 0);
                                            for (int i = 0; i < pending.Length; ++i)
                                            {
                                                if (pending[i].m_ID > Art.GetMaxItemID())
                                                {
                                                    continue;
                                                }

                                                if (pending[i].m_Hue < 0)
                                                {
                                                    pending[i].m_Hue = 0;
                                                }

                                                bool first = true;
                                                for (int k = 0; k < j; ++k)
                                                {
                                                    if ((tileList[k].m_ID == pending[i].m_ID) &&
                                                        ((tileList[k].m_X == pending[i].m_X) &&
                                                         (tileList[k].m_Y == pending[i].m_Y)) &&
                                                        (tileList[k].m_Z == pending[i].m_Z) &&
                                                        (tileList[k].m_Hue == pending[i].m_Hue))
                                                    {
                                                        first = false;
                                                        break;
                                                    }
                                                }

                                                if (first)
                                                {
                                                    tileList[j++] = pending[i];
                                                }
                                            }
                                        }

                                        if (j > 0)
                                        {
                                            binidx.Write((int)binmul.BaseStream.Position); //lookup
                                            for (int i = 0; i < j; ++i)
                                            {
                                                binmul.Write(tileList[i].m_ID);
                                                binmul.Write(tileList[i].m_X);
                                                binmul.Write(tileList[i].m_Y);
                                                binmul.Write(tileList[i].m_Z);
                                                binmul.Write(tileList[i].m_Hue);
                                            }
                                        }
                                    }

                                    fsmullength = (int)binmul.BaseStream.Position - fsmullength;
                                    if (fsmullength > 0)
                                    {
                                        binidx.Write(fsmullength); // length
                                        if (extra == -1)
                                        {
                                            extra = 0;
                                        }

                                        binidx.Write(extra); // extra
                                    }
                                    else
                                    {
                                        binidx.Write(-1); // lookup
                                        binidx.Write(-1); // length
                                        binidx.Write(-1); // extra
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

            indexReader.Close();
            staticsReader.Close();
        }

        public static void RewriteMap(string path, int map, int width, int height)
        {
            string mapPath = Files.GetFilePath("map{0}.mul", map);
            BinaryReader mapReader;
            if (mapPath != null)
            {
                FileStream mapStream = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                mapReader = new BinaryReader(mapStream);
            }
            else
            {
                return;
            }

            int blockx = width >> 3;
            int blocky = height >> 3;

            string mul = Path.Combine(path, $"map{map}.mul");
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var memmul = new MemoryStream();
                using (var binmul = new BinaryWriter(memmul))
                {
                    for (int x = 0; x < blockx; ++x)
                    {
                        for (int y = 0; y < blocky; ++y)
                        {
                            try
                            {
                                mapReader.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                int header = mapReader.ReadInt32();
                                binmul.Write(header);
                                for (int i = 0; i < 64; ++i)
                                {
                                    short tileid = mapReader.ReadInt16();
                                    sbyte z = mapReader.ReadSByte();
                                    if ((tileid < 0) || (tileid >= 0x4000))
                                    {
                                        tileid = 0;
                                    }

                                    if (z < -128)
                                    {
                                        z = -128;
                                    }

                                    if (z > 127)
                                    {
                                        z = 127;
                                    }

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
            mapReader.Close();
        }

        public void ReportInvisStatics(string reportfile)
        {
            reportfile = Path.Combine(reportfile, $"staticReport-{_mapId}.csv");
            using (var tex = new StreamWriter(new FileStream(reportfile, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("x;y;z;Static");
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        Tile currtile = Tiles.GetLandTile(x, y);
                        foreach (HuedTile currstatic in Tiles.GetStaticTiles(x, y))
                        {
                            if (currstatic.Z < currtile.Z && TileData.ItemTable[currstatic.ID].Height + currstatic.Z < currtile.Z)
                            {
                                tex.WriteLine("{0};{1};{2};0x{3:X}", x, y, currstatic.Z, currstatic.ID);
                            }
                        }
                    }
                }
            }
        }

        public void ReportInvalidMapIDs(string reportfile)
        {
            reportfile = Path.Combine(reportfile, $"ReportInvalidMapIDs-{_mapId}.csv");
            using (var tex = new StreamWriter(new FileStream(reportfile, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("x;y;z;Static;LandTile");
                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        Tile currtile = Tiles.GetLandTile(x, y);
                        if (!Art.IsValidLand(currtile.ID))
                        {
                            tex.WriteLine("{0};{1};{2};0;0x{3:X}", x, y, currtile.Z, currtile.ID);
                        }

                        foreach (HuedTile currstatic in Tiles.GetStaticTiles(x, y))
                        {
                            if (!Art.IsValidStatic(currstatic.ID))
                            {
                                tex.WriteLine("{0};{1};{2};0x{3:X};0", x, y, currstatic.Z, currstatic.ID);
                            }
                        }
                    }
                }
            }
        }
    }
}