using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultima
{
    public sealed class TileMatrixPatch
    {
        public int LandBlocksCount { get; }
        public int StaticBlocksCount { get; }

        public Tile[][][] LandBlocks { get; }
        public HuedTile[][][][][] StaticBlocks { get; }

        private readonly int _blockWidth;
        private readonly int _blockHeight;

        private static byte[] _mBuffer;
        private static StaticTile[] _mTileBuffer = new StaticTile[128];

        public bool IsLandBlockPatched(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _blockWidth || y >= _blockHeight)
                return false;
            if (LandBlocks[x] == null)
                return false;
            if (LandBlocks[x][y] == null)
                return false;
            return true;
        }
        public Tile[] GetLandBlock(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _blockWidth || y >= _blockHeight)
                return TileMatrix.InvalidLandBlock;
            if (LandBlocks[x]==null)
                return TileMatrix.InvalidLandBlock;
            return LandBlocks[x][y];
        }

        public Tile GetLandTile(int x, int y)
        {
            return GetLandBlock(x >> 3, y >> 3)[((y & 0x7) << 3) + (x & 0x7)];
        }

        public bool IsStaticBlockPatched(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _blockWidth || y >= _blockHeight)
                return false;
            if (StaticBlocks[x] == null)
                return false;
            if (StaticBlocks[x][y] == null)
                return false;
            return true;
        }

        public HuedTile[][][] GetStaticBlock(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _blockWidth || y >= _blockHeight)
                return TileMatrix.EmptyStaticBlock;
            if (StaticBlocks[x] == null)
                return TileMatrix.EmptyStaticBlock;
            return StaticBlocks[x][y];
        }

        public HuedTile[] GetStaticTiles(int x, int y)
        {
            return GetStaticBlock(x >> 3, y >> 3)[x & 0x7][y & 0x7];
        }

        public TileMatrixPatch(TileMatrix matrix, int index, string path)
        {
            _blockWidth = matrix.BlockWidth;
            _blockHeight = matrix.BlockWidth;

            LandBlocksCount = StaticBlocksCount = 0;
            string mapDataPath, mapIndexPath;
            if (path == null)
            {
                mapDataPath = Files.GetFilePath("mapdif{0}.mul", index);
                mapIndexPath = Files.GetFilePath("mapdifl{0}.mul", index);
            }
            else
            {
                mapDataPath = Path.Combine(path, $"mapdif{index}.mul");
                if (!File.Exists(mapDataPath))
                    mapDataPath = null;
                mapIndexPath = Path.Combine(path, $"mapdifl{index}.mul");
                if (!File.Exists(mapIndexPath))
                    mapIndexPath = null;
            }

            if (mapDataPath != null && mapIndexPath != null)
            {
                LandBlocks = new Tile[matrix.BlockWidth][][];
                LandBlocksCount = PatchLand(matrix, mapDataPath, mapIndexPath);
            }

            string staDataPath, staIndexPath, staLookupPath;
            if (path == null)
            {
                staDataPath = Files.GetFilePath("stadif{0}.mul", index);
                staIndexPath = Files.GetFilePath("stadifl{0}.mul", index);
                staLookupPath = Files.GetFilePath("stadifi{0}.mul", index);
            }
            else
            {
                staDataPath = Path.Combine(path, $"stadif{index}.mul");
                if (!File.Exists(staDataPath))
                    staDataPath = null;
                staIndexPath = Path.Combine(path, $"stadifl{index}.mul");
                if (!File.Exists(staIndexPath))
                    staIndexPath = null;
                staLookupPath = Path.Combine(path, $"stadifi{index}.mul");
                if (!File.Exists(staLookupPath))
                    staLookupPath = null;
            }

            if (staDataPath != null && staIndexPath != null && staLookupPath != null)
            {
                StaticBlocks = new HuedTile[matrix.BlockWidth][][][][];
                StaticBlocksCount = PatchStatics(matrix, staDataPath, staIndexPath, staLookupPath);
            }
        }

        private unsafe int PatchLand(TileMatrix matrix, string dataPath, string indexPath)
        {
            using (FileStream fsData = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                              fsIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader indexReader = new BinaryReader(fsIndex))
                {
                    int count = (int)(indexReader.BaseStream.Length / 4);

                    for (int i = 0; i < count; ++i)
                    {
                        int blockId = indexReader.ReadInt32();
                        int x = blockId / matrix.BlockHeight;
                        int y = blockId % matrix.BlockHeight;

                        fsData.Seek(4, SeekOrigin.Current);

                        Tile[] tiles = new Tile[64];

                        GCHandle gc = GCHandle.Alloc(tiles, GCHandleType.Pinned);
                        try
                        {
                            if (_mBuffer == null || _mBuffer.Length < 192)
                                _mBuffer = new byte[192];

                            fsData.Read(_mBuffer, 0, 192);

                            Marshal.Copy(_mBuffer, 0, gc.AddrOfPinnedObject(), 192);
                        }
                        finally
                        {
                            gc.Free();
                        }
                        if (LandBlocks[x] == null)
                            LandBlocks[x] = new Tile[matrix.BlockHeight][];
                        LandBlocks[x][y] = tiles;
                    }
                    return count;
                }
            }
        }

        private unsafe int PatchStatics(TileMatrix matrix, string dataPath, string indexPath, string lookupPath)
        {
            using (FileStream fsData = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                              fsIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read),
                              fsLookup = new FileStream(lookupPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader indexReader = new BinaryReader(fsIndex),
                                    lookupReader = new BinaryReader(fsLookup))
                {
                    int count = Math.Min((int)(indexReader.BaseStream.Length / 4),
                                        (int)(lookupReader.BaseStream.Length / 12));
                    
                    HuedTileList[][] lists = new HuedTileList[8][];

                    for (int x = 0; x < 8; ++x)
                    {
                        lists[x] = new HuedTileList[8];

                        for (int y = 0; y < 8; ++y)
                            lists[x][y] = new HuedTileList();
                    }

                    for (int i = 0; i < count; ++i)
                    {
                        int blockId = indexReader.ReadInt32();
                        int blockX = blockId / matrix.BlockHeight;
                        int blockY = blockId % matrix.BlockHeight;

                        int offset = lookupReader.ReadInt32();
                        int length = lookupReader.ReadInt32();
                        lookupReader.ReadInt32(); // Extra

                        if (offset < 0 || length <= 0)
                        {
                            if (StaticBlocks[blockX] == null)
                                StaticBlocks[blockX] = new HuedTile[matrix.BlockHeight][][][];

                            StaticBlocks[blockX][blockY] = TileMatrix.EmptyStaticBlock;
                            continue;
                        }

                        fsData.Seek(offset, SeekOrigin.Begin);

                        int tileCount = length / 7;

                        if (_mTileBuffer.Length < tileCount)
                            _mTileBuffer = new StaticTile[tileCount];

                        StaticTile[] staTiles = _mTileBuffer;

                        GCHandle gc = GCHandle.Alloc(staTiles, GCHandleType.Pinned);
                        try
                        {
                            if (_mBuffer == null || _mBuffer.Length < length)
                                _mBuffer = new byte[length];

                            fsData.Read(_mBuffer, 0, length);

                            Marshal.Copy(_mBuffer, 0, gc.AddrOfPinnedObject(), length);

                            for (int j = 0; j < tileCount; ++j)
                            {
                                StaticTile cur = staTiles[j];
                                lists[cur.m_X & 0x7][cur.m_Y & 0x7].Add(Art.GetLegalItemId(cur.m_ID), cur.m_Hue, cur.m_Z);
                            }

                            HuedTile[][][] tiles = new HuedTile[8][][];

                            for (int x = 0; x < 8; ++x)
                            {
                                tiles[x] = new HuedTile[8][];

                                for (int y = 0; y < 8; ++y)
                                    tiles[x][y] = lists[x][y].ToArray();
                            }

                            if (StaticBlocks[blockX] == null)
                                StaticBlocks[blockX] = new HuedTile[matrix.BlockHeight][][][];

                            StaticBlocks[blockX][blockY] = tiles;
                        }
                        finally
                        {
                            gc.Free();
                        }
                    }

                    return count;
                }
            }
        }
    }
}