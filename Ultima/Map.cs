using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Ultima
{
    /// <summary>
    /// Altitude rendering mode for map preview generation
    /// </summary>
    public enum MapAltitudeMode
    {
        /// <summary>
        /// Normal flat rendering without altitude effects
        /// </summary>
        Normal,
        /// <summary>
        /// Normal rendering with altitude-based shading
        /// </summary>
        NormalWithAltitude,
        /// <summary>
        /// Pure altitude map (grayscale based on height)
        /// </summary>
        Altitude
    }

    /// <summary>
    /// Altitude shading preset configuration
    /// </summary>
    public enum AltitudeShadingPreset
    {
        /// <summary>
        /// Dramatic, high-contrast shading with sharp edges
        /// </summary>
        Sharp,
        /// <summary>
        /// More pronounced shading with higher contrast
        /// </summary>
        Normal,
        /// <summary>
        /// Very subtle, smooth shading (matches UO client closely)
        /// </summary>
        Soft,
        /// <summary>
        /// Custom settings (uses manual configuration)
        /// </summary>
        Custom
    }

    /// <summary>
    /// Configuration for altitude-based shading effects
    /// </summary>
    public class AltitudeShadingSettings
    {
        /// <summary>
        /// Surface normal Z-component (higher = softer shading)
        /// Sharp: 2.0, Normal: 4.0, Soft: 8.0+
        /// </summary>
        public float NormalZ { get; set; } = 8.0f;

        /// <summary>
        /// Brightness variation range (0.0 to 0.5)
        /// Sharp: 0.40 (±40%), Normal: 0.30 (±30%), Soft: 0.15 (±15%)
        /// </summary>
        public float BrightnessRange { get; set; } = 0.15f;

        /// <summary>
        /// Altitude gradient smoothing factor
        /// Sharp: 0.75, Normal: 0.50, Soft: 0.25
        /// </summary>
        public float GradientSmoothing { get; set; } = 0.25f;

        /// <summary>
        /// Gets preset configuration
        /// </summary>
        public static AltitudeShadingSettings GetPreset(AltitudeShadingPreset preset)
        {
            return preset switch
            {
                AltitudeShadingPreset.Sharp => new AltitudeShadingSettings
                {
                    NormalZ = 2.0f,
                    BrightnessRange = 0.40f,
                    GradientSmoothing = 0.75f
                },
                AltitudeShadingPreset.Normal => new AltitudeShadingSettings
                {
                    NormalZ = 4.0f,
                    BrightnessRange = 0.30f,
                    GradientSmoothing = 0.50f
                },
                AltitudeShadingPreset.Soft => new AltitudeShadingSettings
                {
                    NormalZ = 8.0f,
                    BrightnessRange = 0.15f,
                    GradientSmoothing = 0.25f
                },
                _ => new AltitudeShadingSettings() // Default to Soft
            };
        }
    }

    public sealed class Map
    {
        private TileMatrix _tiles;
        private readonly int _mapId;
        private readonly string _path;
        private static bool _useDiff;

        /// <summary>
        /// Controls the intensity of altitude-based shading (1-20, lower = more contrast)
        /// Default is 15 for subtle effect
        /// </summary>
        public static int AltitudeIntensity { get; set; } = 15;

        /// <summary>
        /// Current altitude shading preset
        /// </summary>
        public static AltitudeShadingPreset ShadingPreset { get; set; } = AltitudeShadingPreset.Normal;

        /// <summary>
        /// Custom altitude shading settings (used when ShadingPreset is Custom)
        /// </summary>
        public static AltitudeShadingSettings CustomShadingSettings { get; set; } = AltitudeShadingSettings.GetPreset(AltitudeShadingPreset.Soft);

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
            Felucca.Tiles.CloseStreams();
            Trammel.Tiles.CloseStreams();
            Ilshenar.Tiles.CloseStreams();
            Malas.Tiles.CloseStreams();
            Tokuno.Tiles.CloseStreams();
            TerMur.Tiles.CloseStreams();

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

        public TileMatrix Tiles
        {
            get
            {
                return _tiles ??= new TileMatrix(FileIndex, _mapId, Width, Height, _path);
            }
        }

        public int Width { get; set; }

        public int Height { get; }

        public int FileIndex { get; }

        ///// <summary>
        ///// Returns Bitmap with Statics
        ///// </summary>
        ///// <param name="x">8x8 Block</param>
        ///// <param name="y">8x8 Block</param>
        ///// <param name="width">8x8 Block</param>
        ///// <param name="height">8x8 Block</param>
        ///// <returns></returns>
        // TODO: unused?
        //public Bitmap GetImage(int x, int y, int width, int height)
        //{
        //    return GetImage(x, y, width, height, true);
        //}

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

        private ushort[][][] _cache;
        private ushort[][][] _cacheNoStatics;
        private ushort[][][] _cacheNoPatch;
        private ushort[][][] _cacheNoStaticsNoPatch;
        private ushort[] _black;

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
                    _black = new ushort[64];
                }

                return;
            }

            ushort[][][] cache;
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
                        _cache = cache = new ushort[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStatics = cache = new ushort[_tiles.BlockHeight][][];
                    }
                }
                else
                {
                    if (statics)
                    {
                        _cacheNoPatch = cache = new ushort[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStaticsNoPatch = cache = new ushort[_tiles.BlockHeight][][];
                    }
                }
            }

            if (cache[y] == null)
            {
                cache[y] = new ushort[_tiles.BlockWidth][];
            }

            if (cache[y][x] == null)
            {
                cache[y][x] = RenderBlock(x, y, statics, UseDiff);
            }

            _tiles.CloseStreams();
        }

        private ushort[] GetRenderedBlock(int x, int y, bool statics)
        {
            TileMatrix matrix = Tiles;

            if (x < 0 || y < 0 || x >= matrix.BlockWidth || y >= matrix.BlockHeight)
            {
                return _black ??= new ushort[64];
            }

            ushort[][][] cache;
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
                        _cache = cache = new ushort[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStatics = cache = new ushort[_tiles.BlockHeight][][];
                    }
                }
                else
                {
                    if (statics)
                    {
                        _cacheNoPatch = cache = new ushort[_tiles.BlockHeight][][];
                    }
                    else
                    {
                        _cacheNoStaticsNoPatch = cache = new ushort[_tiles.BlockHeight][][];
                    }
                }
            }

            if (cache[y] == null)
            {
                cache[y] = new ushort[_tiles.BlockWidth][];
            }

            ushort[] data = cache[y][x];

            if (data == null)
            {
                cache[y][x] = data = RenderBlock(x, y, statics, UseDiff);
            }

            return data;
        }

        private unsafe ushort[] RenderBlock(int x, int y, bool drawStatics, bool diff)
        {
            var data = new ushort[64];

            Tile[] tiles = _tiles.GetLandBlock(x, y, diff);

            fixed (ushort* pColors = RadarCol.Colors)
            {
                fixed (int* pHeight = TileData.HeightTable)
                {
                    fixed (Tile* ptTiles = tiles)
                    {
                        Tile* pTiles = ptTiles;

                        fixed (ushort* pData = data)
                        {
                            ushort* pvData = pData;

                            if (drawStatics)
                            {
                                HuedTile[][][] statics = _tiles.GetStaticBlock(x, y, diff);

                                for (int k = 0; k < 8; ++k)
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
                                                    z = pStatics->Z;
                                                    top = z + pHeight[pStatics->Id];

                                                    if (top > highTop || (z > highZ && top >= highTop))
                                                    {
                                                        highTop = top;
                                                        highZ = z;
                                                        highId = pStatics->Id;
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
                                                if (penS.X != p || penS.Y != k)
                                                {
                                                    continue;
                                                }

                                                z = penS.Z;
                                                top = z + pHeight[penS.Id];

                                                if (top <= highTop && (z <= highZ || top < highTop))
                                                {
                                                    continue;
                                                }

                                                highTop = top;
                                                highZ = z;
                                                highId = penS.Id;
                                                highHue = penS.Hue;
                                                highStatic = true;
                                            }
                                        }

                                        top = pTiles->Z;

                                        if (top > highTop)
                                        {
                                            highId = pTiles->Id;
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
                                    *pvData++ = pColors[(pTiles++)->Id];
                                }
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
        public void GetImage(int x, int y, int width, int height, Bitmap bmp)
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
                    ushort[] data = GetRenderedBlock(bx, by, statics);

                    fixed (ushort* pData = data)
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
                        *pRow7++ = *pvData;
                    }
                }
            }

            bmp.UnlockBits(bd);
            _tiles.CloseStreams();
        }

        public static void DefragStatics(string path, Map map, int width, int height, bool remove)
        {
            string indexPath = Files.GetFilePath($"staidx{map.FileIndex}.mul");
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

            string staticsPath = Files.GetFilePath($"statics{map.FileIndex}.mul");

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
                                    if (!remove) // without duplicate remove
                                    {
                                        bool firstitem = true;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            ushort graphic = staticsReader.ReadUInt16();
                                            byte sx = staticsReader.ReadByte();
                                            byte sy = staticsReader.ReadByte();
                                            sbyte sz = staticsReader.ReadSByte();
                                            short shue = staticsReader.ReadInt16();

                                            if (graphic > Art.GetMaxItemId())
                                            {
                                                continue;
                                            }

                                            if (shue < 0)
                                            {
                                                shue = 0;
                                            }

                                            if (firstitem)
                                            {
                                                binidx.Write((int)binmul.BaseStream.Position); // lookup
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
                                                if (tileList[i].Id > Art.GetMaxItemId())
                                                {
                                                    continue;
                                                }

                                                if (tileList[i].Hue < 0)
                                                {
                                                    tileList[i].Hue = 0;
                                                }

                                                if (firstitem)
                                                {
                                                    binidx.Write((int)binmul.BaseStream.Position); // lookup
                                                    firstitem = false;
                                                }

                                                binmul.Write(tileList[i].Id);
                                                binmul.Write(tileList[i].X);
                                                binmul.Write(tileList[i].Y);
                                                binmul.Write(tileList[i].Z);
                                                binmul.Write(tileList[i].Hue);
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
                                                Id = staticsReader.ReadUInt16(),
                                                X = staticsReader.ReadByte(),
                                                Y = staticsReader.ReadByte(),
                                                Z = staticsReader.ReadSByte(),
                                                Hue = staticsReader.ReadInt16()
                                            };

                                            if (tile.Id > Art.GetMaxItemId())
                                            {
                                                continue;
                                            }

                                            if (tile.Hue < 0)
                                            {
                                                tile.Hue = 0;
                                            }

                                            bool first = true;
                                            for (int k = 0; k < j; ++k)
                                            {
                                                if ((tileList[k].Id == tile.Id) && (tileList[k].X == tile.X) && (tileList[k].Y == tile.Y) && (tileList[k].Z == tile.Z) && (tileList[k].Hue == tile.Hue))
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
                                                if (pending[i].Id > Art.GetMaxItemId())
                                                {
                                                    continue;
                                                }

                                                if (pending[i].Hue < 0)
                                                {
                                                    pending[i].Hue = 0;
                                                }

                                                bool first = true;
                                                for (int k = 0; k < j; ++k)
                                                {
                                                    if ((tileList[k].Id == pending[i].Id) && (tileList[k].X == pending[i].X) && (tileList[k].Y == pending[i].Y) && (tileList[k].Z == pending[i].Z) && (tileList[k].Hue == pending[i].Hue))
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
                                            binidx.Write((int)binmul.BaseStream.Position); // lookup
                                            for (int i = 0; i < j; ++i)
                                            {
                                                binmul.Write(tileList[i].Id);
                                                binmul.Write(tileList[i].X);
                                                binmul.Write(tileList[i].Y);
                                                binmul.Write(tileList[i].Z);
                                                binmul.Write(tileList[i].Hue);
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
                                        binidx.Write(-1); // lookup
                                        binidx.Write(-1); // length
                                        binidx.Write(-1); // extra
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

        public static void RewriteMap(string path, int mapIndex, int width, int height)
        {
            string mapPath = Files.GetFilePath($"map{mapIndex}.mul");
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

            int blockX = width >> 3;
            int blockY = height >> 3;

            string mulPath = Path.Combine(path, $"map{mapIndex}.mul");

            using (var fileStream = new FileStream(mulPath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var memoryStream = new MemoryStream();
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    for (int x = 0; x < blockX; ++x)
                    {
                        for (int y = 0; y < blockY; ++y)
                        {
                            try
                            {
                                mapReader.BaseStream.Seek(((x * blockY) + y) * 196, SeekOrigin.Begin);
                                int header = mapReader.ReadInt32();
                                binaryWriter.Write(header);
                                for (int i = 0; i < 64; ++i)
                                {
                                    short tileId = mapReader.ReadInt16();
                                    sbyte z = mapReader.ReadSByte();

                                    if (tileId is < 0 or >= 0x4000)
                                    {
                                        tileId = 0;
                                    }

                                    binaryWriter.Write(tileId);
                                    binaryWriter.Write(z);
                                }
                            }
                            catch // fill rest
                            {
                                binaryWriter.BaseStream.Seek(((x * blockY) + y) * 196, SeekOrigin.Begin);
                                for (; x < blockX; ++x)
                                {
                                    for (; y < blockY; ++y)
                                    {
                                        binaryWriter.Write(0);
                                        for (int i = 0; i < 64; ++i)
                                        {
                                            binaryWriter.Write((short)0);
                                            binaryWriter.Write((sbyte)0);
                                        }
                                    }
                                    y = 0;
                                }
                            }
                        }
                    }

                    memoryStream.WriteTo(fileStream);
                }
            }

            mapReader.Close();
        }

        public void ReportInvisibleStatics(string reportFile)
        {
            reportFile = Path.Combine(reportFile, $"staticReport-{_mapId}.csv");

            using (var tex = new StreamWriter(new FileStream(reportFile, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("x;y;z;Static");

                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        Tile currentTile = Tiles.GetLandTile(x, y);

                        foreach (HuedTile currentStatic in Tiles.GetStaticTiles(x, y))
                        {
                            if (currentStatic.Z < currentTile.Z &&
                                TileData.ItemTable[currentStatic.Id].Height + currentStatic.Z < currentTile.Z)
                            {
                                tex.WriteLine("{0};{1};{2};0x{3:X}", x, y, currentStatic.Z, currentStatic.Id);
                            }
                        }
                    }
                }
            }
        }

        public void ReportInvalidMapIDs(string reportFile)
        {
            reportFile = Path.Combine(reportFile, $"ReportInvalidMapIDs-{_mapId}.csv");

            using (var tex = new StreamWriter(new FileStream(reportFile, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("x;y;z;Static;LandTile");

                for (int x = 0; x < Width; ++x)
                {
                    for (int y = 0; y < Height; ++y)
                    {
                        Tile currentTile = Tiles.GetLandTile(x, y);

                        if (!Art.IsValidLand(currentTile.Id))
                        {
                            tex.WriteLine("{0};{1};{2};0;0x{3:X}", x, y, currentTile.Z, currentTile.Id);
                        }

                        foreach (HuedTile currentStatics in Tiles.GetStaticTiles(x, y))
                        {
                            if (!Art.IsValidStatic(currentStatics.Id))
                            {
                                tex.WriteLine("{0};{1};{2};0x{3:X};0", x, y, currentStatics.Z, currentStatics.Id);
                            }
                        }
                    }
                }
            }
        }

        #region Altitude Map Rendering

        /// <summary>
        /// Returns Bitmap with altitude rendering mode support
        /// </summary>
        /// <param name="x">8x8 Block X</param>
        /// <param name="y">8x8 Block Y</param>
        /// <param name="width">Width in 8x8 Blocks</param>
        /// <param name="height">Height in 8x8 Blocks</param>
        /// <param name="statics">Include statics in rendering</param>
        /// <param name="altitudeMode">Altitude rendering mode</param>
        /// <returns>Rendered bitmap</returns>
        public Bitmap GetImageWithAltitude(int x, int y, int width, int height, bool statics, MapAltitudeMode altitudeMode)
        {
            PixelFormat format = altitudeMode == MapAltitudeMode.Altitude 
                ? PixelFormat.Format8bppIndexed 
                : PixelFormat.Format16bppRgb555;

            var bmp = new Bitmap(width << 3, height << 3, format);

            if (altitudeMode == MapAltitudeMode.Altitude)
            {
                // Create grayscale palette for altitude map
                ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                bmp.Palette = palette;
            }

            GetImageWithAltitude(x, y, width, height, bmp, statics, altitudeMode);

            return bmp;
        }

        /// <summary>
        /// Draws in given Bitmap with altitude rendering mode support
        /// </summary>
        /// <param name="x">8x8 Block X</param>
        /// <param name="y">8x8 Block Y</param>
        /// <param name="width">Width in 8x8 Blocks</param>
        /// <param name="height">Height in 8x8 Blocks</param>
        /// <param name="bmp">Target bitmap</param>
        /// <param name="statics">Include statics in rendering</param>
        /// <param name="altitudeMode">Altitude rendering mode</param>
        public unsafe void GetImageWithAltitude(int x, int y, int width, int height, Bitmap bmp, bool statics, MapAltitudeMode altitudeMode)
        {
            PixelFormat format = altitudeMode == MapAltitudeMode.Altitude 
                ? PixelFormat.Format8bppIndexed 
                : PixelFormat.Format16bppRgb555;

            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, width << 3, height << 3), ImageLockMode.WriteOnly, format);
            int stride = bd.Stride;
            int blockStride = stride << 3;

            var pStart = (byte*)bd.Scan0;

            if (altitudeMode == MapAltitudeMode.Altitude)
            {
                // 8-bit altitude mode
                for (int oy = 0, by = y; oy < height; ++oy, ++by, pStart += blockStride)
                {
                    var pRow0 = (byte*)(pStart + (0 * stride));
                    var pRow1 = (byte*)(pStart + (1 * stride));
                    var pRow2 = (byte*)(pStart + (2 * stride));
                    var pRow3 = (byte*)(pStart + (3 * stride));
                    var pRow4 = (byte*)(pStart + (4 * stride));
                    var pRow5 = (byte*)(pStart + (5 * stride));
                    var pRow6 = (byte*)(pStart + (6 * stride));
                    var pRow7 = (byte*)(pStart + (7 * stride));

                    for (int ox = 0, bx = x; ox < width; ++ox, ++bx)
                    {
                        sbyte[] altitudeData = GetAltitudeBlock(bx, by, statics);

                        for (int i = 0; i < 64; i++)
                        {
                            byte altValue = (byte)Math.Clamp(altitudeData[i] + 128, 0, 255);
                            int rowIndex = i / 8;
                            int colIndex = i % 8;

                            switch (rowIndex)
                            {
                                case 0: pRow0[ox * 8 + colIndex] = altValue; break;
                                case 1: pRow1[ox * 8 + colIndex] = altValue; break;
                                case 2: pRow2[ox * 8 + colIndex] = altValue; break;
                                case 3: pRow3[ox * 8 + colIndex] = altValue; break;
                                case 4: pRow4[ox * 8 + colIndex] = altValue; break;
                                case 5: pRow5[ox * 8 + colIndex] = altValue; break;
                                case 6: pRow6[ox * 8 + colIndex] = altValue; break;
                                case 7: pRow7[ox * 8 + colIndex] = altValue; break;
                            }
                        }
                    }
                }
            }
            else
            {
                // 16-bit color modes (Normal and NormalWithAltitude)
                for (int oy = 0, by = y; oy < height; ++oy, ++by, pStart += blockStride)
                {
                    var pRow0 = (ushort*)(pStart + (0 * stride));
                    var pRow1 = (ushort*)(pStart + (1 * stride));
                    var pRow2 = (ushort*)(pStart + (2 * stride));
                    var pRow3 = (ushort*)(pStart + (3 * stride));
                    var pRow4 = (ushort*)(pStart + (4 * stride));
                    var pRow5 = (ushort*)(pStart + (5 * stride));
                    var pRow6 = (ushort*)(pStart + (6 * stride));
                    var pRow7 = (ushort*)(pStart + (7 * stride));

                    for (int ox = 0, bx = x; ox < width; ++ox, ++bx)
                    {
                        ushort[] colorData = GetRenderedBlock(bx, by, statics);

                        if (altitudeMode == MapAltitudeMode.NormalWithAltitude)
                        {
                            sbyte[] altitudeData = GetAltitudeBlock(bx, by, statics);
                            colorData = ProcessBlockWithAltitude(colorData, altitudeData);
                        }

                        for (int i = 0; i < 64; i++)
                        {
                            int rowIndex = i / 8;
                            int colIndex = i % 8;

                            switch (rowIndex)
                            {
                                case 0: pRow0[ox * 8 + colIndex] = colorData[i]; break;
                                case 1: pRow1[ox * 8 + colIndex] = colorData[i]; break;
                                case 2: pRow2[ox * 8 + colIndex] = colorData[i]; break;
                                case 3: pRow3[ox * 8 + colIndex] = colorData[i]; break;
                                case 4: pRow4[ox * 8 + colIndex] = colorData[i]; break;
                                case 5: pRow5[ox * 8 + colIndex] = colorData[i]; break;
                                case 6: pRow6[ox * 8 + colIndex] = colorData[i]; break;
                                case 7: pRow7[ox * 8 + colIndex] = colorData[i]; break;
                            }
                        }
                    }
                }
            }

            bmp.UnlockBits(bd);
            _tiles.CloseStreams();
        }

        /// <summary>
        /// Gets altitude data for an 8x8 block
        /// </summary>
        private sbyte[] GetAltitudeBlock(int x, int y, bool drawStatics)
        {
            var altitudeData = new sbyte[64];
            TileMatrix matrix = Tiles;

            if (x < 0 || y < 0 || x >= matrix.BlockWidth || y >= matrix.BlockHeight)
            {
                return altitudeData;
            }

            Tile[] tiles = _tiles.GetLandBlock(x, y, UseDiff);

            unsafe
            {
                fixed (int* pHeight = TileData.HeightTable)
                {
                    fixed (Tile* ptTiles = tiles)
                    {
                        Tile* pTiles = ptTiles;

                        for (int k = 0; k < 8; ++k)
                        {
                            for (int p = 0; p < 8; ++p)
                            {
                                int idx = k * 8 + p;
                                int highTop = -255;
                                int highZ = -255;
                                int highId = 0;

                                if (drawStatics)
                                {
                                    HuedTile[][][] statics = _tiles.GetStaticBlock(x, y, UseDiff);
                                    HuedTile[] curStatics = statics[p][k];

                                    if (curStatics.Length > 0)
                                    {
                                        fixed (HuedTile* phtStatics = curStatics)
                                        {
                                            HuedTile* pStatics = phtStatics;
                                            HuedTile* pStaticsEnd = pStatics + curStatics.Length;

                                            while (pStatics < pStaticsEnd)
                                            {
                                                int z = pStatics->Z;
                                                int top = z + pHeight[pStatics->Id];

                                                if (top > highTop || (z > highZ && top >= highTop))
                                                {
                                                    highTop = top;
                                                    highZ = z;
                                                    highId = pStatics->Id;
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
                                            if (penS.X != p || penS.Y != k)
                                            {
                                                continue;
                                            }

                                            int z = penS.Z;
                                            int top = z + pHeight[penS.Id];

                                            if (top > highTop || (z > highZ && top >= highTop))
                                            {
                                                highTop = top;
                                                highZ = z;
                                                highId = penS.Id;
                                            }
                                        }
                                    }
                                }

                                int landTop = pTiles->Z;
                                if (landTop > highTop || !drawStatics)
                                {
                                    highZ = landTop;
                                }

                                altitudeData[idx] = (sbyte)Math.Clamp(highZ, -128, 127);
                                ++pTiles;
                            }
                        }
                    }
                }
            }

            return altitudeData;
        }

        /// <summary>
        /// Process colors with altitude-based shading (translates Pascal ProcessBlock/ProcessQuad/ProcessColor)
        /// </summary>
        private static ushort[] ProcessBlockWithAltitude(ushort[] colors, sbyte[] altitudes)
        {
            // Get current shading settings based on preset
            AltitudeShadingSettings settings = ShadingPreset == AltitudeShadingPreset.Custom 
                ? CustomShadingSettings 
                : AltitudeShadingSettings.GetPreset(ShadingPreset);

            // Use configurable intensity (lower = more contrast, higher = softer)
            int maxSlope = Math.Clamp(AltitudeIntensity, 1, 20);

            ushort[] processed = new ushort[64];
            Array.Copy(colors, processed, 64);

            // Light source comes from northwest (negative X, negative Y direction)
            const float lightX = -0.577f; // Northwest direction (normalized)
            const float lightY = -0.577f;
            const float lightZ = 0.577f;  // Equal components for 45-degree angle

            // Calculate lighting for each pixel using its surrounding pixels
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int idx = y * 8 + x;

                    // Get altitude differences using configurable smoothing
                    float dx = GetAltitudeDifference(altitudes, x, y, 1, 0, settings.GradientSmoothing);
                    float dy = GetAltitudeDifference(altitudes, x, y, 0, 1, settings.GradientSmoothing);

                    // Calculate surface normal with configurable nz
                    float nx = -dx;
                    float ny = -dy;
                    float nz = settings.NormalZ + (maxSlope - 10) * 0.5f; // Adjust based on intensity

                    // Normalize the normal vector
                    float length = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);
                    if (length > 0)
                    {
                        nx /= length;
                        ny /= length;
                        nz /= length;
                    }

                    // Calculate dot product with light direction (lambertian lighting)
                    float dotProduct = nx * lightX + ny * lightY + nz * lightZ;
                    dotProduct = Math.Clamp(dotProduct, 0.0f, 1.0f);

                    // Apply configurable brightness range
                    float baseIntensity = 20.0f / maxSlope; // Higher maxSlope = less intense
                    float range = settings.BrightnessRange * baseIntensity;
                    float multiplier = 1.0f + (dotProduct - 0.5f) * 2.0f * range;

                    // Clamp to safe range based on brightness range
                    float minMult = 1.0f - range;
                    float maxMult = 1.0f + range;
                    multiplier = Math.Clamp(multiplier, minMult, maxMult);

                    // Apply lighting to the color
                    processed[idx] = ApplyLighting(processed[idx], multiplier);
                }
            }

            return processed;
        }

        /// <summary>
        /// Get altitude difference in a specific direction, with bounds checking and averaging
        /// </summary>
        private static float GetAltitudeDifference(sbyte[] altitudes, int x, int y, int dx, int dy, float smoothing)
        {
            int x1 = Math.Max(0, Math.Min(7, x - dx));
            int y1 = Math.Max(0, Math.Min(7, y - dy));
            int x2 = Math.Max(0, Math.Min(7, x + dx));
            int y2 = Math.Max(0, Math.Min(7, y + dy));

            int idx1 = y1 * 8 + x1;
            int idx2 = y2 * 8 + x2;

            return (altitudes[idx2] - altitudes[idx1]) * smoothing;
        }

        /// <summary>
        /// Apply lighting multiplier to a color
        /// </summary>
        private static ushort ApplyLighting(ushort color, float multiplier)
        {
            // Extract RGB components from 15-bit color (RGB555)
            int red = (color >> 10) & 0x1F;
            int green = (color >> 5) & 0x1F;
            int blue = color & 0x1F;

            // Apply multiplier
            red = (int)(red * multiplier);
            green = (int)(green * multiplier);
            blue = (int)(blue * multiplier);

            // Clamp to 5-bit range
            red = Math.Clamp(red, 0, 31);
            green = Math.Clamp(green, 0, 31);
            blue = Math.Clamp(blue, 0, 31);

            // Recombine into RGB555 format
            return (ushort)((red << 10) | (green << 5) | blue);
        }

        #endregion
    }
}