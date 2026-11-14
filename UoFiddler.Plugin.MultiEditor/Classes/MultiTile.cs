/***************************************************************************
 *
 * $Author: MuadDib & Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    public partial class FloorTile : MultiTile
    {
        public FloorTile(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool IsVirtualFloor => true;
    }

    public partial class MultiTile : IComparable
    {
        private int _x;
        private int _y;
        private int _z;

        public MultiTile(ushort id, int x, int y, int z, int flag)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Invisible = flag == 0;
            Solver = 0;
        }

        public MultiTile(ushort id, int x, int y, int z, bool flag)
        {
            Id = id;
            X = x;
            Y = y;
            Z = z;
            Invisible = flag;
            Solver = 0;
        }

        public MultiTile()
        {
            Id = 0xFFFF;
        }

        public int Height => TileData.ItemTable[Id].Height;

        public ushort Id { get; internal set; }

        public virtual bool IsVirtualFloor => false;

        public int X
        {
            get => _x;
            set
            {
                _x = value;
                RecalculateMod();
            }
        }

        public int XMod { get; private set; }

        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                RecalculateMod();
            }
        }

        public int YMod { get; private set; }

        public int Z
        {
            get => _z;
            set
            {
                _z = value;
                RecalculateMod();
            }
        }

        public bool Invisible { get; set; }

        /// <summary>
        /// The multi.mul extra field (MultiTileEntry.Unk1), kept so a save writes it back.
        /// </summary>
        public int Extra { get; set; }

        public bool Walkable { get; private set; }

        public bool DoubleSurface { get; private set; }

        public bool Transparent { get; set; }

        /// <summary>
        /// Hidden in the editor view (not drawn, hovered or selected). View only: not saved and
        /// not an undo step.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Draw order among tiles of one cell that sort equal. Unique and only ever assigned once,
        /// so a tile restored by undo goes back to its old place in the stack.
        /// </summary>
        public long Solver { get; set; }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is not MultiTile multiTile)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (X > multiTile.X)
            {
                return 1;
            }

            if (X < multiTile.X)
            {
                return -1;
            }

            if (Y > multiTile.Y)
            {
                return 1;
            }

            if (Y < multiTile.Y)
            {
                return -1;
            }

            // One key for every tile, the virtual floor included, so the order is transitive and a
            // cell sorts the same whatever order its tiles were added in. The floor counts as
            // threshold -1: below real tiles of the same sort Z.
            int ourThreshold = SortThreshold;
            int theirThreshold = multiTile.SortThreshold;

            int res = (Z + Math.Max(0, ourThreshold)).CompareTo(multiTile.Z + Math.Max(0, theirThreshold));
            if (res == 0)
            {
                res = ourThreshold.CompareTo(theirThreshold);
            }

            if (res == 0)
            {
                res = Solver.CompareTo(multiTile.Solver);
            }

            return res;
        }

        private int SortThreshold
        {
            get
            {
                if (IsVirtualFloor)
                {
                    return -1;
                }

                ItemData data = TileData.ItemTable[Id];
                int threshold = 0;
                if (data.Height > 0)
                {
                    ++threshold;
                }

                if (!data.Background)
                {
                    ++threshold;
                }

                return threshold;
            }
        }

        public void Set(ushort id, int z)
        {
            Id = id;
            Z = z;

            RecalculateMod();
        }

        private void RecalculateMod()
        {
            int width;
            int height;
            if (IsVirtualFloor)
            {
                width = 44;
                height = 44;
            }
            else if (!ArtMetrics.TryGetSize(Id, out width, out height))
            {
                return;
            }

            XMod = ((_x - _y) * 22) - (width / 2) + MultiEditorComponentList.GapXMod;
            YMod = ((_x + _y) * 22) - (_z * 4) - height + MultiEditorComponentList.GapYMod;
        }

        private const TileFlag _impassableOrSurface = TileFlag.Impassable | TileFlag.Surface;

        public bool IsWalkable(int z, List<MultiTile> tiles)
        {
            int top = z + 16; // Player Height

            foreach (MultiTile tile in tiles)
            {
                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & _impassableOrSurface) == 0)
                {
                    continue;
                }

                if ((itemData.Flags & TileFlag.Door) != 0)
                {
                    continue;
                }

                int checkTop = tile.Z + itemData.CalcHeight;
                if (checkTop <= z || tile.Z >= top)
                {
                    continue;
                }

                Walkable = false;
                return false;
            }

            Walkable = true;
            return true;
        }

        public bool IsDoubleSurface(List<MultiTile> tiles)
        {
            foreach (MultiTile tile in tiles)
            {
                if (tile.Z != Z)
                {
                    continue;
                }

                ItemData itemData = TileData.ItemTable[tile.Id];
                if ((itemData.Flags & TileFlag.Surface) == 0)
                {
                    continue;
                }

                if (tile == this)
                {
                    continue;
                }

                DoubleSurface = true;
                return true;
            }

            DoubleSurface = false;
            return false;
        }
    }
}
