using System.Collections.Generic;

namespace Ultima
{
    public sealed class HuedTileList
    {
        private readonly List<HuedTile> _mTiles;
        public HuedTileList()
        {
            _mTiles = new List<HuedTile>();
        }

        public int Count => _mTiles.Count;

        public void Add(ushort id, short hue, sbyte z)
        {
            _mTiles.Add(new HuedTile(id, hue, z));
        }

        public HuedTile[] ToArray()
        {
            HuedTile[] tiles = new HuedTile[Count];

            if (_mTiles.Count > 0)
                _mTiles.CopyTo(tiles);
            _mTiles.Clear();

            return tiles;
        }
    }

    public sealed class TileList
    {
        private readonly List<Tile> _mTiles;

        public TileList()
        {
            _mTiles = new List<Tile>();
        }

        public int Count => _mTiles.Count;

        public void Add(ushort id, sbyte z)
        {
            _mTiles.Add(new Tile(id, z));
        }
        public void Add(ushort id, sbyte z, sbyte flag)
        {
            _mTiles.Add(new Tile(id, z, flag));
        }

        public Tile[] ToArray()
        {
            Tile[] tiles = new Tile[Count];
            if (_mTiles.Count > 0)
                _mTiles.CopyTo(tiles);
            _mTiles.Clear();

            return tiles;
        }

        public Tile Get(int i)
        {
            return _mTiles[i];
        }
    }

    public sealed class MTileList
    {
        private readonly List<MTile> _mTiles;

        public MTileList()
        {
            _mTiles = new List<MTile>();
        }

        public int Count => _mTiles.Count;

        public void Add(ushort id, sbyte z)
        {
            _mTiles.Add(new MTile(id, z));
        }
        public void Add(ushort id, sbyte z, sbyte flag)
        {
            _mTiles.Add(new MTile(id, z, flag));
        }
        public void Add(ushort id, sbyte z, sbyte flag, int unk1)
        {
            _mTiles.Add(new MTile(id, z, flag, unk1));
        }

        public MTile[] ToArray()
        {
            MTile[] tiles = new MTile[Count];

            if (_mTiles.Count > 0)
                _mTiles.CopyTo(tiles);
            _mTiles.Clear();

            return tiles;
        }

        public MTile Get(int i)
        {
            return _mTiles[i];
        }

        public void Set(int i, ushort id, sbyte z)
        {
            if (i < Count)
                _mTiles[i].Set(id, z);
        }

        public void Set(int i, ushort id, sbyte z, sbyte flag)
        {
            if (i < Count)
                _mTiles[i].Set(id, z, flag);
        }

        public void Set(int i, ushort id, sbyte z, sbyte flag, int unk1)
        {
            if (i < Count)
                _mTiles[i].Set(id, z, flag, unk1);
        }
        public void Remove(int i)
        {
            if (i < Count)
                _mTiles.RemoveAt(i);
        }
    }
}