using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultima
{
    public sealed class TileMatrix
    {
        private readonly HuedTile[][][][][] _mStaticTiles;
        private readonly Tile[][][] _mLandTiles;
        private bool[][] _mRemovedStaticBlock;
        private List<StaticTile>[][] _mStaticTilesToAdd;

        public static Tile[] InvalidLandBlock { get; private set; }
        public static HuedTile[][][] EmptyStaticBlock { get; private set; }

        private FileStream _mMap;
		private BinaryReader _mUopReader;
        private FileStream _mStatics;
        private Entry3D[] _mStaticIndex;
        public Entry3D[] StaticIndex { get { if (!StaticIndexInit) InitStatics(); return _mStaticIndex; } }
        public bool StaticIndexInit;

        public TileMatrixPatch Patch { get; }

        public int BlockWidth { get; }

        public int BlockHeight { get; }

        public int Width { get; }

        public int Height { get; }

        private readonly string _mapPath;
        private readonly string _indexPath;
        private readonly string _staticsPath;

        public void CloseStreams()
        {
            _mMap?.Close();
            _mUopReader?.Close();
            _mStatics?.Close();
        }

        public TileMatrix(int fileIndex, int mapId, int width, int height, string path)
        {
            Width = width;
            Height = height;
            BlockWidth = width >> 3;
            BlockHeight = height >> 3;

            if (path == null)
            { 
                _mapPath = Files.GetFilePath("map{0}.mul", fileIndex);
				if (string.IsNullOrEmpty(_mapPath) || !File.Exists(_mapPath))
					_mapPath = Files.GetFilePath("map{0}LegacyMUL.uop", fileIndex);

				if (_mapPath != null && _mapPath.EndsWith(".uop"))
					IsUopFormat = true;
			}
            else
            {
                _mapPath = Path.Combine(path, $"map{fileIndex}.mul");
				if (!File.Exists(_mapPath))
					_mapPath = Path.Combine(path, $"map{fileIndex}LegacyMUL.uop");
                if (!File.Exists(_mapPath))
                    _mapPath = null;
				else if (_mapPath != null && _mapPath.EndsWith(".uop"))
					IsUopFormat = true;
            }

            if (path == null)
                _indexPath = Files.GetFilePath("staidx{0}.mul", fileIndex);
            else
            {
                _indexPath = Path.Combine(path, $"staidx{fileIndex}.mul");
                if (!File.Exists(_indexPath))
                    _indexPath = null;
            }

            if (path == null)
                _staticsPath = Files.GetFilePath("statics{0}.mul", fileIndex);
            else
            {
                _staticsPath = Path.Combine(path, $"statics{fileIndex}.mul");
                if (!File.Exists(_staticsPath))
                    _staticsPath = null;
            }

            EmptyStaticBlock = new HuedTile[8][][];

            for (int i = 0; i < 8; ++i)
            {
                EmptyStaticBlock[i] = new HuedTile[8][];

                for (int j = 0; j < 8; ++j)
                {
                    EmptyStaticBlock[i][j] = new HuedTile[0];
                }
            }

            InvalidLandBlock = new Tile[196];

            _mLandTiles = new Tile[BlockWidth][][];
            _mStaticTiles = new HuedTile[BlockWidth][][][][];

            Patch = new TileMatrixPatch(this, mapId, path);
        }


        public void SetStaticBlock(int x, int y, HuedTile[][][] value)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
                return;

            if (_mStaticTiles[x] == null)
                _mStaticTiles[x] = new HuedTile[BlockHeight][][][];

            _mStaticTiles[x][y] = value;
        }

        public HuedTile[][][] GetStaticBlock(int x, int y)
        {
            return GetStaticBlock(x, y, true);
        }
        public HuedTile[][][] GetStaticBlock(int x, int y, bool patch)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
                return EmptyStaticBlock;

            if (_mStaticTiles[x] == null)
                _mStaticTiles[x] = new HuedTile[BlockHeight][][][];

            HuedTile[][][] tiles = _mStaticTiles[x][y];

            if (tiles == null)
                tiles = _mStaticTiles[x][y] = ReadStaticBlock(x, y);

            if ((Map.UseDiff) && (patch))
            {
                if (Patch.StaticBlocksCount > 0)
                {
                    if (Patch.StaticBlocks[x] != null)
                    {
                        if (Patch.StaticBlocks[x][y] != null)
                            tiles = Patch.StaticBlocks[x][y];
                    }
                }
            }
            return tiles;
        }
        public HuedTile[] GetStaticTiles(int x, int y, bool patch)
        {
            return GetStaticBlock(x >> 3, y >> 3, patch)[x & 0x7][y & 0x7];
        }
        public HuedTile[] GetStaticTiles(int x, int y)
        {
            return GetStaticBlock(x >> 3, y >> 3)[x & 0x7][y & 0x7];
        }

        public void SetLandBlock(int x, int y, Tile[] value)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
                return;

            if (_mLandTiles[x] == null)
                _mLandTiles[x] = new Tile[BlockHeight][];

            _mLandTiles[x][y] = value;
        }

        public Tile[] GetLandBlock(int x, int y)
        {
            return GetLandBlock(x, y, true);
        }
        public Tile[] GetLandBlock(int x, int y, bool patch)
        {
            if (x < 0 || y < 0 || x >= BlockWidth || y >= BlockHeight)
                return InvalidLandBlock;

            if (_mLandTiles[x] == null)
                _mLandTiles[x] = new Tile[BlockHeight][];

            Tile[] tiles = _mLandTiles[x][y];

            if (tiles == null)
                tiles = _mLandTiles[x][y] = ReadLandBlock(x, y);

            if ((Map.UseDiff) && (patch))
            {
                if (Patch.LandBlocksCount > 0)
                {
                    if (Patch.LandBlocks[x] != null)
                    {
                        if (Patch.LandBlocks[x][y] != null)
                            tiles = Patch.LandBlocks[x][y];
                    }
                }
            }
            return tiles;
        }
        public Tile GetLandTile(int x, int y, bool patch)
        {
            return GetLandBlock(x >> 3, y >> 3, patch)[((y & 0x7) << 3) + (x & 0x7)];
        }

        public Tile GetLandTile(int x, int y)
        {
            return GetLandBlock(x >> 3, y >> 3)[((y & 0x7) << 3) + (x & 0x7)];
        }


        private unsafe void InitStatics()
        {
            _mStaticIndex = new Entry3D[BlockHeight * BlockWidth];
            if (_indexPath == null)
                return;
            using (FileStream index = new FileStream(_indexPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _mStatics = new FileStream(_staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                int count = (int)(index.Length / 12);
                GCHandle gc = GCHandle.Alloc(_mStaticIndex, GCHandleType.Pinned);
                byte[] buffer = new byte[index.Length];
                index.Read(buffer, 0, (int)index.Length);
                Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)Math.Min(index.Length, BlockHeight * BlockWidth * 12));
                gc.Free();
                for (int i = (int)Math.Min(index.Length, BlockHeight * BlockWidth); i < BlockHeight * BlockWidth; ++i)
                {
                    _mStaticIndex[i].lookup = -1;
                    _mStaticIndex[i].length = -1;
                    _mStaticIndex[i].extra = -1;
                }
                StaticIndexInit = true;
            }

        }
        private static HuedTileList[][] _mLists;
        private static byte[] _mBuffer;
        private unsafe HuedTile[][][] ReadStaticBlock(int x, int y)
        {
            try
            {
                if (!StaticIndexInit)
                    InitStatics();
                if (_mStatics == null || !_mStatics.CanRead || !_mStatics.CanSeek)
                {
                    if (_staticsPath == null)
                        _mStatics = null;
                    else
                        _mStatics = new FileStream(_staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                if (_mStatics == null)
                    return EmptyStaticBlock;

                int lookup = _mStaticIndex[(x * BlockHeight) + y].lookup;
                int length = _mStaticIndex[(x * BlockHeight) + y].length;

                if (lookup < 0 || length <= 0)
                    return EmptyStaticBlock;
                else
                {
                    int count = length / 7;

                    _mStatics.Seek(lookup, SeekOrigin.Begin);

                    if (_mBuffer == null || _mBuffer.Length < length)
                        _mBuffer = new byte[length];

                    GCHandle gc = GCHandle.Alloc(_mBuffer, GCHandleType.Pinned);
                    try
                    {
                        _mStatics.Read(_mBuffer, 0, length);

                        if (_mLists == null)
                        {
                            _mLists = new HuedTileList[8][];

                            for (int i = 0; i < 8; ++i)
                            {
                                _mLists[i] = new HuedTileList[8];

                                for (int j = 0; j < 8; ++j)
                                    _mLists[i][j] = new HuedTileList();
                            }
                        }

                        HuedTileList[][] lists = _mLists;

                        for (int i = 0; i < count; ++i)
                        {
                            IntPtr ptr = new IntPtr((long)gc.AddrOfPinnedObject() + i * sizeof(StaticTile));
                            StaticTile cur = (StaticTile)Marshal.PtrToStructure(ptr, typeof(StaticTile));
                            lists[cur.m_X & 0x7][cur.m_Y & 0x7].Add(Art.GetLegalItemId(cur.m_ID), cur.m_Hue, cur.m_Z);
                        }

                        HuedTile[][][] tiles = new HuedTile[8][][];

                        for (int i = 0; i < 8; ++i)
                        {
                            tiles[i] = new HuedTile[8][];

                            for (int j = 0; j < 8; ++j)
                                tiles[i][j] = lists[i][j].ToArray();
                        }

                        return tiles;
                    }
                    finally
                    {
                        gc.Free();
                    }
                }
            }
            finally
            {
                //if (m_Statics != null)
                //    m_Statics.Close();
            }
        }

		/* UOP map files support code, written by Wyatt (c) www.ruosi.org
		 * It's not possible if some entry has unknown hash. Throwed exception
		 * means that EA changed maps UOPs again.
		 */
		#region UOP
		public bool IsUopFormat { get; set; }
		public bool IsUopAlreadyRead { get; set; }

		private struct UopFile
		{
			public readonly long Offset;
			public readonly int Length;

			public UopFile(long offset, int length)
			{
				Offset = offset;
				Length = length;
			}
		}

		private UopFile[] UopFiles { get; set; }
		private long UopLength => _mMap.Length;

        private void ReadUopFiles(string pattern)
		{
			_mUopReader = new BinaryReader(_mMap);

			_mUopReader.BaseStream.Seek(0, SeekOrigin.Begin);

			if (_mUopReader.ReadInt32() != 0x50594D)
				throw new ArgumentException("Bad UOP file.");

			_mUopReader.ReadInt64(); // version + signature
			long nextBlock = _mUopReader.ReadInt64();
			_mUopReader.ReadInt32(); // block capacity
			int count = _mUopReader.ReadInt32();

			UopFiles = new UopFile[count];

			Dictionary<ulong, int> hashes = new Dictionary<ulong, int>();

			for (int i = 0; i < count; i++)
			{
				string file = $"build/{pattern}/{i:D8}.dat";
				ulong hash = FileIndex.HashFileName(file);

				if (!hashes.ContainsKey(hash))
					hashes.Add(hash, i);
			}

			_mUopReader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

			do
			{
				int filesCount = _mUopReader.ReadInt32();
				nextBlock = _mUopReader.ReadInt64();

				for (int i = 0; i < filesCount; i++)
				{
					long offset = _mUopReader.ReadInt64();
					int headerLength = _mUopReader.ReadInt32();
					int compressedLength = _mUopReader.ReadInt32();
					int decompressedLength = _mUopReader.ReadInt32();
					ulong hash = _mUopReader.ReadUInt64();
					_mUopReader.ReadUInt32(); // Adler32
					short flag = _mUopReader.ReadInt16();

					int length = flag == 1 ? compressedLength : decompressedLength;

					if (offset == 0)
						continue;

					int idx;
					if (hashes.TryGetValue(hash, out idx))
					{
						if (idx < 0 || idx > UopFiles.Length)
							throw new IndexOutOfRangeException("hashes dictionary and files collection have different count of entries!");

						UopFiles[idx] = new UopFile(offset + headerLength, length);
					}
					else
					{
						throw new ArgumentException(
                            $"File with hash 0x{hash:X8} was not found in hashes dictionary! EA Mythic changed UOP format!");
					}
				}
			}
			while (_mUopReader.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
		}

		private long CalculateOffsetFromUop(long offset)
		{
			long pos = 0;

			foreach (UopFile t in UopFiles)
			{
				long currPos = pos + t.Length;

				if (offset < currPos)
					return t.Offset + (offset - pos);

				pos = currPos;
			}

			return UopLength;
		}
		#endregion
        private unsafe Tile[] ReadLandBlock(int x, int y)
        {
            if (_mMap == null || !_mMap.CanRead || !_mMap.CanSeek)
            {
                if (_mapPath == null)
                    _mMap = null;
                else
                    _mMap = new FileStream(_mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
				if (IsUopFormat && _mapPath != null && !IsUopAlreadyRead)
				{
					FileInfo fi = new FileInfo(_mapPath);
					string uopPattern = fi.Name.Replace(fi.Extension, "").ToLowerInvariant();

					ReadUopFiles(uopPattern);
					IsUopAlreadyRead = true;
				}
            }
            Tile[] tiles = new Tile[64];
            if (_mMap != null)
            {
				long offset = ((x * BlockHeight) + y) * 196 + 4;

				if (IsUopFormat)
					offset = CalculateOffsetFromUop(offset);

				_mMap.Seek(offset, SeekOrigin.Begin);

                GCHandle gc = GCHandle.Alloc(tiles, GCHandleType.Pinned);
                try
                {
                    if (_mBuffer == null || _mBuffer.Length < 192)
                        _mBuffer = new byte[192];

                    _mMap.Read(_mBuffer, 0, 192);

                    Marshal.Copy(_mBuffer, 0, gc.AddrOfPinnedObject(), 192);
                }
                finally
                {
                    gc.Free();
                }
                //m_Map.Close();
            }

            return tiles;
        }

        public void RemoveStaticBlock(int blockx, int blocky)
        {
            if (_mRemovedStaticBlock == null)
                _mRemovedStaticBlock = new bool[BlockWidth][];
            if (_mRemovedStaticBlock[blockx] == null)
                _mRemovedStaticBlock[blockx] = new bool[BlockHeight];
            _mRemovedStaticBlock[blockx][blocky] = true;
            if (_mStaticTiles[blockx] == null)
                _mStaticTiles[blockx] = new HuedTile[BlockHeight][][][];
            _mStaticTiles[blockx][blocky] = EmptyStaticBlock;
        }

        public bool IsStaticBlockRemoved(int blockx, int blocky)
        {
            if (_mRemovedStaticBlock?[blockx] == null)
                return false;
            return _mRemovedStaticBlock[blockx][blocky];
        }

        public bool PendingStatic(int blockx, int blocky)
        {
            if (_mStaticTilesToAdd?[blocky] == null)
                return false;
            if (_mStaticTilesToAdd[blocky][blockx] == null)
                return false;
            return true;
        }

        public void AddPendingStatic(int blockx, int blocky, StaticTile toadd)
        {
            if (_mStaticTilesToAdd == null)
                _mStaticTilesToAdd = new List<StaticTile>[BlockHeight][];
            if (_mStaticTilesToAdd[blocky] == null)
                _mStaticTilesToAdd[blocky] = new List<StaticTile>[BlockWidth];
            if (_mStaticTilesToAdd[blocky][blockx] == null)
                _mStaticTilesToAdd[blocky][blockx] = new List<StaticTile>();
            _mStaticTilesToAdd[blocky][blockx].Add(toadd);
        }

        public StaticTile[] GetPendingStatics(int blockx, int blocky)
        {
            if (_mStaticTilesToAdd?[blocky] == null)
                return null;
            if (_mStaticTilesToAdd[blocky][blockx] == null)
                return null;

            return _mStaticTilesToAdd[blocky][blockx].ToArray();
        }

        public void Dispose()
        {
            _mMap?.Close();

            _mUopReader?.Close();
            _mStatics?.Close();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StaticTile
    {
        public ushort m_ID;
        public byte m_X;
        public byte m_Y;
        public sbyte m_Z;
        public short m_Hue;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HuedTile
    {
        internal sbyte m_Z;
        internal ushort m_ID;
        internal int m_Hue;

        public ushort Id { get => m_ID;
            set => m_ID = value;
        }
        public int Hue { get => m_Hue;
            set => m_Hue = value;
        }
        public int Z { get => m_Z;
            set => m_Z = (sbyte)value;
        }

        public HuedTile(ushort id, short hue, sbyte z)
        {
            m_ID = id;
            m_Hue = hue;
            m_Z = z;
        }

        public void Set(ushort id, short hue, sbyte z)
        {
            m_ID = id;
            m_Hue = hue;
            m_Z = z;
        }
    }

    public struct MTile : IComparable
    {
        internal ushort MId;
        internal sbyte MZ;
        internal sbyte MFlag;
        internal int MUnk1;
        internal int MSolver;

        public ushort Id => MId;
        public int Z { get => MZ;
            set => MZ = (sbyte)value;
        }

        public int Flag { get => MFlag;
            set => MFlag = (sbyte)value;
        }
        public int Unk1 { get => MUnk1;
            set => MUnk1 = value;
        }
        public int Solver { get => MSolver;
            set => MSolver = value;
        }

        public MTile(ushort id, sbyte z)
        {
            MId = Art.GetLegalItemId(id);
            MZ = z;
            MFlag = 1;
            MSolver = 0;
            MUnk1 = 0;
        }

        public MTile(ushort id, sbyte z, sbyte flag)
        {
            MId = Art.GetLegalItemId(id);
            MZ = z;
            MFlag = flag;
            MSolver = 0;
            MUnk1 = 0;
        }

        public MTile(ushort id, sbyte z, sbyte flag, int unk1)
        {
            MId = Art.GetLegalItemId(id);
            MZ = z;
            MFlag = flag;
            MSolver = 0;
            MUnk1 = unk1;
        }

        public void Set(ushort id, sbyte z)
        {
            MId = Art.GetLegalItemId(id);
            MZ = z;
        }

        public void Set(ushort id, sbyte z, sbyte flag)
        {
            MId = Art.GetLegalItemId(id);
            MZ = z;
            MFlag = flag;
        }

        public void Set(ushort id, sbyte z, sbyte flag, int unk1)
        {
            MId = Art.GetLegalItemId(id);
            MZ = z;
            MFlag = flag;
            MUnk1 = unk1;
        }

        public int CompareTo(object x)
        {
            if (x == null)
                return 1;

            if (!(x is MTile))
                throw new ArgumentNullException();

            MTile a = (MTile)x;

            ItemData ourData = TileData.ItemTable[MId];
            ItemData theirData = TileData.ItemTable[a.Id];

            int ourTreshold = 0;
            if (ourData.Height > 0)
                ++ourTreshold;
            if (!ourData.Background)
                ++ourTreshold;
            int ourZ = Z;
            int theirTreshold = 0;
            if (theirData.Height > 0)
                ++theirTreshold;
            if (!theirData.Background)
                ++theirTreshold;
            int theirZ = a.Z;

            ourZ += ourTreshold;
            theirZ += theirTreshold;
            int res = ourZ - theirZ;
            if (res == 0)
                res = ourTreshold - theirTreshold;
            if (res == 0)
                res = MSolver - a.Solver;
            return res;
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tile : IComparable
    {
        internal ushort m_ID;
        internal sbyte m_Z;

        public ushort Id => m_ID;
        public int Z { get => m_Z;
            set => m_Z = (sbyte)value;
        }

        public Tile(ushort id, sbyte z)
        {
            m_ID = id;
            m_Z = z;
        }

        public Tile(ushort id, sbyte z, sbyte flag)
        {
            m_ID = id;
            m_Z = z;
        }

        public void Set(ushort id, sbyte z)
        {
            m_ID = id;
            m_Z = z;
        }

        public void Set(ushort id, sbyte z, sbyte flag)
        {
            m_ID = id;
            m_Z = z;
        }

        public int CompareTo(object x)
        {
            if (x == null)
                return 1;

            if (!(x is Tile))
                throw new ArgumentNullException();

            Tile a = (Tile)x;

            if (m_Z > a.m_Z)
                return 1;
            else if (a.m_Z > m_Z)
                return -1;

            ItemData ourData = TileData.ItemTable[m_ID];
            ItemData theirData = TileData.ItemTable[a.m_ID];

            if (ourData.Height > theirData.Height)
                return 1;
            else if (theirData.Height > ourData.Height)
                return -1;

            if (ourData.Background && !theirData.Background)
                return -1;
            else if (theirData.Background && !ourData.Background)
                return 1;

            return 0;
        }
    }
}