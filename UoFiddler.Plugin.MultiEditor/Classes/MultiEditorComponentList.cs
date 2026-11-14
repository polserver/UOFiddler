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
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// The edited multi. Tiles are kept per cell, each cell sorted in draw order with its virtual
    /// floor tile, so walking the cells x-major then y gives the draw order of the whole multi.
    /// Every edit is recorded as an <see cref="EditCommand"/> for undo and redo.
    /// </summary>
    internal partial class MultiEditorComponentList
    {
        public const int DefaultUndoStackSize = 200;

        public const int GapXMod = 44;
        public const int GapYMod = 22;

        private readonly IMultiViewSettings _parent;
        private readonly List<EditCommand> _undoList = new();
        private readonly List<EditCommand> _redoList = new();
        private readonly List<MultiTile> _scratch = new();

        private List<MultiTile>[] _cells;
        private int _tileCount;
        private long _solverCounter;
        private EditCommand _current;

        public int WalkableCount;
        public int DoubleSurfaceCount;

        /// <summary>
        /// Create a blank ComponentList
        /// </summary>
        public MultiEditorComponentList(int width, int height, IMultiViewSettings parent)
        {
            _parent = parent;
            RebuildGrid(width, height, Array.Empty<MultiTile>());

            RecalculateMinMax();
        }

        /// <summary>
        /// Create a ComponentList from UltimaSDK
        /// </summary>
        public MultiEditorComponentList(MultiComponentList list, IMultiViewSettings parent)
        {
            _parent = parent;

            var tiles = new List<MultiTile>();
            for (int x = 0; x < list.Width; ++x)
            {
                for (int y = 0; y < list.Height; ++y)
                {
                    foreach (MTile mTile in list.Tiles[x][y])
                    {
                        tiles.Add(new MultiTile(mTile.Id, x, y, mTile.Z, mTile.Flag)
                        {
                            Extra = mTile.Unk1,
                            Solver = ++_solverCounter
                        });
                    }
                }
            }

            RebuildGrid(list.Width, list.Height, tiles);

            RecalculateMinMax();
        }

        public int Height { get; private set; }

        public int Width { get; private set; }

        /// <summary>
        /// Every tile, in draw order.
        /// </summary>
        public IEnumerable<MultiTile> RealTiles
        {
            get
            {
                foreach (List<MultiTile> cell in _cells)
                {
                    foreach (MultiTile tile in cell)
                    {
                        yield return tile;
                    }
                }
            }
        }

        public int TileCount => _tileCount;

        public IReadOnlyList<EditCommand> UndoList => _undoList;

        public IReadOnlyList<EditCommand> RedoList => _redoList;

        public int XMax { get; private set; }

        private int XMaxOrg { get; set; }

        public int XMin { get; private set; }

        private int XMinOrg { get; set; }

        private int YMax { get; set; }

        public int YMaxOrg { get; private set; }

        public int YMin { get; private set; }

        public int YMinOrg { get; private set; }

        public int ZMax { get; private set; }

        public int ZMin { get; private set; }

        /// <summary>
        /// Export to given multi id
        /// </summary>
        public void AddToSdkComponentList(int id)
        {
            Multis.Add(id, ConvertToSdk());
            Options.ChangedUltimaClass["Multis"] = true;
            ControlEvents.FireMultiChangeEvent(this, id);
        }

        public MultiComponentList ConvertToSdk()
        {
            var tiles = new MTileList[Width][];
            for (int x = 0; x < Width; ++x)
            {
                tiles[x] = new MTileList[Height];
                for (int y = 0; y < Height; ++y)
                {
                    tiles[x][y] = new MTileList();
                }
            }

            foreach (MultiTile tile in RealTiles)
            {
                tiles[tile.X][tile.Y].Add(tile.Id, (sbyte)tile.Z, tile.Invisible ? (sbyte)0 : (sbyte)1, tile.Extra);
            }

            return new MultiComponentList(tiles, _tileCount, Width, Height);
        }

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        /// <summary>
        /// False for tiles hidden one by one and, while walls are hidden, for tiles flagged as walls.
        /// </summary>
        public bool IsShown(MultiTile tile)
        {
            return !tile.Hidden && !(_parent.HideWalls && (TileData.ItemTable[tile.Id].Flags & TileFlag.Wall) != 0);
        }

        public int HiddenCount
        {
            get
            {
                int count = 0;
                foreach (MultiTile tile in RealTiles)
                {
                    if (tile.Hidden)
                    {
                        ++count;
                    }
                }

                return count;
            }
        }

        /// <summary>
        /// True while the tile is part of the multi. A tile removed by an edit or undo keeps its
        /// coordinates, so the lookup goes straight to its cell.
        /// </summary>
        public bool Contains(MultiTile tile)
        {
            return tile != null && InBounds(tile.X, tile.Y) && Cell(tile.X, tile.Y).Contains(tile);
        }

        /// <summary>
        /// The real tiles of one cell, bottom to top.
        /// </summary>
        public List<MultiTile> GetTilesAt(int x, int y)
        {
            return InBounds(x, y) ? new List<MultiTile>(Cell(x, y)) : new List<MultiTile>();
        }

        // ---------------------------------------------------------------- edits

        /// <summary>
        /// Resizes Multi size. Tiles outside the new size are removed.
        /// </summary>
        public void Resize(int width, int height)
        {
            if (width == Width && height == Height)
            {
                return;
            }

            BeginEdit("Resize");

            var outside = new List<MultiTile>();
            foreach (MultiTile tile in RealTiles)
            {
                if (tile.X >= width || tile.Y >= height)
                {
                    outside.Add(tile);
                }
            }

            foreach (MultiTile tile in outside)
            {
                RecordRemove(tile);
            }

            RebuildGrid(width, height, new List<MultiTile>(RealTiles));
            EndEdit();
        }

        /// <summary>
        /// Adds an <see cref="MultiTile"/> to specific location
        /// </summary>
        public void TileAdd(int x, int y, int z, ushort id, bool replace = false)
        {
            BeginEdit("Add Tile");
            RecordNew(id, x, y, z, false, 0, replace);
            EndEdit();
        }

        /// <summary>
        /// Moves given <see cref="MultiTile"/>
        /// </summary>
        public void TileMove(MultiTile tile, int newX, int newY)
        {
            // A tile from another multi is not in the list.
            if (!InBounds(newX, newY) || !Contains(tile))
            {
                return;
            }

            BeginEdit("Move Tile");
            RecordChange(tile, tile.Id, newX, newY, tile.Z, tile.Invisible);
            EndEdit();
        }

        /// <summary>
        /// Moves a group of tiles by an offset in one undo step. Nothing moves when any tile would
        /// leave the multi. Returns true when the group moved.
        /// </summary>
        public bool TileMoveBatch(IEnumerable<MultiTile> tiles, int dx, int dy, int dz, string action = "Move Tiles")
        {
            var group = new List<MultiTile>();
            foreach (MultiTile tile in tiles)
            {
                if (Contains(tile))
                {
                    if (!InBounds(tile.X + dx, tile.Y + dy))
                    {
                        return false;
                    }

                    group.Add(tile);
                }
            }

            if (group.Count == 0 || (dx == 0 && dy == 0 && dz == 0))
            {
                return false;
            }

            BeginEdit(action);
            foreach (MultiTile tile in group)
            {
                RecordChange(tile, tile.Id, tile.X + dx, tile.Y + dy, ClampZ(tile.Z + dz), tile.Invisible);
            }

            EndEdit();
            return true;
        }

        /// <summary>
        /// Removes all tiles within the given multi-space rectangle at or above minZ in a single undo entry.
        /// </summary>
        public void TileRemoveRect(int x0, int y0, int x1, int y1, int minZ, int maxZ = int.MaxValue)
        {
            BeginEdit("Remove Rect");

            for (int x = Math.Max(0, x0); x <= Math.Min(Width - 1, x1); ++x)
            {
                for (int y = Math.Max(0, y0); y <= Math.Min(Height - 1, y1); ++y)
                {
                    foreach (MultiTile tile in GetTilesAt(x, y))
                    {
                        // What is not shown is not removed.
                        if (tile.Z >= minZ && tile.Z <= maxZ && IsShown(tile))
                        {
                            RecordRemove(tile);
                        }
                    }
                }
            }

            EndEdit();
        }

        /// <summary>
        /// Removes multiple tiles in a single undo entry
        /// </summary>
        public void TileRemoveBatch(IEnumerable<MultiTile> tiles)
        {
            BeginEdit("Remove Tiles");
            foreach (MultiTile tile in new List<MultiTile>(tiles))
            {
                RecordRemove(tile);
            }

            EndEdit();
        }

        /// <summary>
        /// Removes specific <see cref="MultiTile"/>
        /// </summary>
        public void TileRemove(MultiTile tile)
        {
            if (!Contains(tile))
            {
                return;
            }

            BeginEdit("Remove Tile");
            RecordRemove(tile);
            EndEdit();
        }

        /// <summary>
        /// Alters Z level for given <see cref="MultiTile"/>
        /// </summary>
        public void TileZMod(MultiTile tile, int modZ)
        {
            TileZMod(new[] { tile }, modZ, "Modify Z");
        }

        /// <summary>
        /// Sets Z value of given <see cref="MultiTile"/>
        /// </summary>
        public void TileZSet(MultiTile tile, int setZ)
        {
            TileZSet(new[] { tile }, setZ, "Set Z");
        }

        /// <summary>
        /// Adjusts Z for a group of tiles in a single undo step.
        /// </summary>
        public void TileZMod(IEnumerable<MultiTile> tiles, int modZ, string action = "Group Modify Z")
        {
            BeginEdit(action);
            foreach (MultiTile tile in new List<MultiTile>(tiles))
            {
                if (Contains(tile))
                {
                    RecordChange(tile, tile.Id, tile.X, tile.Y, ClampZ(tile.Z + modZ), tile.Invisible);
                }
            }

            EndEdit();
        }

        /// <summary>
        /// Sets Z for a group of tiles in a single undo step.
        /// </summary>
        public void TileZSet(IEnumerable<MultiTile> tiles, int setZ, string action = "Group Set Z")
        {
            BeginEdit(action);
            int z = ClampZ(setZ);
            foreach (MultiTile tile in new List<MultiTile>(tiles))
            {
                if (Contains(tile))
                {
                    RecordChange(tile, tile.Id, tile.X, tile.Y, z, tile.Invisible);
                }
            }

            EndEdit();
        }

        /// <summary>
        /// Sets a tile's Dynamic (invisible) flag as an undoable change.
        /// </summary>
        public void TileSetInvisible(MultiTile tile, bool invisible)
        {
            if (!Contains(tile) || tile.Invisible == invisible)
            {
                return;
            }

            BeginEdit("Dynamic");
            RecordChange(tile, tile.Id, tile.X, tile.Y, tile.Z, invisible);
            EndEdit();
        }

        /// <summary>
        /// Fills a rectangular area with a tile at the given Z level.
        /// </summary>
        public void TileAddRect(int x0, int y0, int x1, int y1, int z, ushort id, bool replace = false)
        {
            BeginEdit("Rectangle Fill");
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    RecordNew(id, x, y, z, false, 0, replace);
                }
            }

            EndEdit();
        }

        /// <summary>
        /// Places tiles along a Bresenham line at the given Z level.
        /// </summary>
        public void TileAddLine(int x0, int y0, int x1, int y1, int z, ushort id, bool replace = false)
        {
            BeginEdit("Line Draw");
            foreach (var (x, y) in BresenhamLine(x0, y0, x1, y1))
            {
                RecordNew(id, x, y, z, false, 0, replace);
            }

            EndEdit();
        }

        public static IEnumerable<(int x, int y)> BresenhamLinePublic(int x0, int y0, int x1, int y1)
            => BresenhamLine(x0, y0, x1, y1);

        private static IEnumerable<(int x, int y)> BresenhamLine(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                yield return (x0, y0);
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// Places an NxN brush of tiles centered on (cx, cy) at the given Z level.
        /// </summary>
        public void TileAddBrush(int cx, int cy, int z, ushort id, int brushSize, bool replace = false)
        {
            BeginEdit("Draw Brush");
            int half = brushSize / 2;
            for (int dx = -half; dx <= half; dx++)
            {
                for (int dy = -half; dy <= half; dy++)
                {
                    RecordNew(id, cx + dx, cy + dy, z, false, 0, replace);
                }
            }

            EndEdit();
        }

        /// <summary>
        /// Gives tiles new ids in one undo step; position, Z and flags stay.
        /// </summary>
        public void TileSetIds(IEnumerable<(MultiTile Tile, ushort Id)> changes, string action)
        {
            BeginEdit(action);
            foreach (var (tile, id) in new List<(MultiTile, ushort)>(changes))
            {
                if (Contains(tile))
                {
                    RecordChange(tile, id, tile.X, tile.Y, tile.Z, tile.Invisible);
                }
            }

            EndEdit();
        }

        /// <summary>
        /// Removes some tiles and adds others in one undo step (wall and house part tools).
        /// </summary>
        public void ReplaceTiles(IEnumerable<MultiTile> remove, IEnumerable<(ushort Id, int X, int Y, int Z)> add, string action)
        {
            BeginEdit(action);
            foreach (MultiTile tile in new List<MultiTile>(remove))
            {
                RecordRemove(tile);
            }

            foreach (var (id, x, y, z) in add)
            {
                RecordNew(id, x, y, z, false, 0);
            }

            EndEdit();
        }

        /// <summary>
        /// Replaces a group of tiles with new ones, optionally at a new multi size, in one undo
        /// step (rotate and mirror). Returns the tiles added.
        /// </summary>
        public List<MultiTile> ReplaceTiles(IEnumerable<MultiTile> remove,
            IEnumerable<(ushort Id, int X, int Y, int Z, bool Invisible, int Extra)> add, string action, int width, int height)
        {
            BeginEdit(action);
            foreach (MultiTile tile in new List<MultiTile>(remove))
            {
                RecordRemove(tile);
            }

            if (width != Width || height != Height)
            {
                foreach (MultiTile tile in new List<MultiTile>(RealTiles))
                {
                    if (tile.X >= width || tile.Y >= height)
                    {
                        RecordRemove(tile);
                    }
                }

                RebuildGrid(width, height, new List<MultiTile>(RealTiles));
            }

            var added = new List<MultiTile>();
            foreach (var (id, x, y, z, invisible, extra) in add)
            {
                if (RecordNew(id, x, y, z, invisible, extra) is { } tile)
                {
                    added.Add(tile);
                }
            }

            EndEdit();
            return added;
        }

        /// <summary>
        /// Adds a batch of tiles in a single undo step (used by copy/paste).
        /// </summary>
        public void TileAddBatch(List<(ushort id, int x, int y, int z, bool invisible, int extra)> tiles)
        {
            BeginEdit("Paste");
            foreach (var (id, x, y, z, invisible, extra) in tiles)
            {
                RecordNew(id, x, y, z, invisible, extra);
            }

            EndEdit();
        }

        // ---------------------------------------------------------------- undo / redo

        public void Undo(int index)
        {
            if (index < 0 || index >= _undoList.Count)
            {
                return;
            }

            // Step back one entry at a time, so every state passed over, the current one included,
            // ends up on the redo stack and can be redone.
            for (int i = 0; i <= index; ++i)
            {
                UndoStep();
            }
        }

        /// <summary>Single-step undo (Ctrl+Z). Moves the step to the redo stack.</summary>
        public bool UndoStep()
        {
            if (_undoList.Count == 0)
            {
                return false;
            }

            EditCommand command = _undoList[0];
            _undoList.RemoveAt(0);
            Apply(command, undo: true);

            _redoList.Insert(0, command);
            TrimToSize(_redoList);
            return true;
        }

        /// <summary>Single-step redo (Ctrl+Y). Moves the step back to the undo stack.</summary>
        public bool RedoStep()
        {
            if (_redoList.Count == 0)
            {
                return false;
            }

            EditCommand command = _redoList[0];
            _redoList.RemoveAt(0);
            Apply(command, undo: false);

            _undoList.Insert(0, command);
            TrimToSize(_undoList);
            return true;
        }

        public void UndoClear()
        {
            _undoList.Clear();
            _redoList.Clear();
        }

        private static void TrimToSize(List<EditCommand> list)
        {
            if (list.Count > DefaultUndoStackSize)
            {
                list.RemoveRange(DefaultUndoStackSize, list.Count - DefaultUndoStackSize);
            }
        }

        private void Apply(EditCommand command, bool undo)
        {
            HashSet<MultiTile> toAdd = undo ? command.Removed : command.Added;
            HashSet<MultiTile> toRemove = undo ? command.Added : command.Removed;
            Dictionary<MultiTile, TileState> states = undo ? command.Before : command.After;
            int width = undo ? command.OldWidth : command.NewWidth;
            int height = undo ? command.OldHeight : command.NewHeight;

            foreach (MultiTile tile in toRemove)
            {
                GridRemove(tile);
            }

            if (width != Width || height != Height)
            {
                // Positions may only fit the other size, so set every tile and rebuild the grid.
                var all = new List<MultiTile>(RealTiles);
                foreach (var (tile, state) in states)
                {
                    state.ApplyTo(tile);
                }

                all.AddRange(toAdd);
                RebuildGrid(width, height, all);
            }
            else
            {
                foreach (var (tile, state) in states)
                {
                    GridRemove(tile);
                    state.ApplyTo(tile);
                    GridInsert(tile);
                }

                foreach (MultiTile tile in toAdd)
                {
                    GridInsert(tile);
                }
            }

            RecalculateMinMax();
        }

        private void BeginEdit(string action)
        {
            _current = new EditCommand(action, Width, Height);
        }

        private void EndEdit()
        {
            EditCommand command = _current;
            _current = null;

            command.NewWidth = Width;
            command.NewHeight = Height;
            command.CaptureAfter();
            if (command.IsEmpty)
            {
                return;
            }

            _undoList.Insert(0, command);
            TrimToSize(_undoList);
            _redoList.Clear();

            RecalculateMinMax();
        }

        /// <summary>
        /// Adds a new tile unless it is out of bounds or the same id already sits at the same Z in
        /// that cell. With <paramref name="replace"/>, other tiles of the same kind at that Z go
        /// first: a flat tile replaces flat tiles, a tile with height replaces tiles with height.
        /// </summary>
        private MultiTile RecordNew(ushort id, int x, int y, int z, bool invisible, int extra, bool replace = false)
        {
            if (!InBounds(x, y))
            {
                return null;
            }

            z = ClampZ(z);
            if (replace)
            {
                bool flat = TileData.ItemTable[id].Height == 0;
                foreach (MultiTile other in GetTilesAt(x, y))
                {
                    if (other.Z == z && other.Id != id && (other.Height == 0) == flat)
                    {
                        RecordRemove(other);
                    }
                }
            }

            foreach (MultiTile other in Cell(x, y))
            {
                if (other.Id == id && other.Z == z)
                {
                    return null;
                }
            }

            var tile = new MultiTile(id, x, y, z, invisible) { Extra = extra, Solver = ++_solverCounter };
            GridInsert(tile);
            _current.Added.Add(tile);
            return tile;
        }

        private void RecordRemove(MultiTile tile)
        {
            if (!GridRemove(tile))
            {
                return;
            }

            if (_current.Added.Remove(tile))
            {
                return;
            }

            if (_current.Before.Remove(tile, out TileState before))
            {
                // Undo puts it back as it was before this edit, not as it was when removed.
                before.ApplyTo(tile);
            }

            _current.Removed.Add(tile);
        }

        private void RecordChange(MultiTile tile, ushort id, int x, int y, int z, bool invisible)
        {
            var target = new TileState(id, x, y, z, invisible);
            if (TileState.Of(tile) == target || !InBounds(x, y))
            {
                return;
            }

            if (!_current.Added.Contains(tile) && !_current.Before.ContainsKey(tile))
            {
                _current.Before[tile] = TileState.Of(tile);
            }

            GridRemove(tile);
            target.ApplyTo(tile);
            GridInsert(tile);
        }

        // ---------------------------------------------------------------- grid

        private List<MultiTile> Cell(int x, int y) => _cells[(x * Height) + y];

        /// <summary>The tiles of one cell in draw order. Cells are drawn x by x, and y by y within each x.</summary>
        public IReadOnlyList<MultiTile> TilesIn(int x, int y) => Cell(x, y);

        private void GridInsert(MultiTile tile)
        {
            List<MultiTile> cell = Cell(tile.X, tile.Y);
            cell.Add(tile);
            cell.Sort();
            ++_tileCount;
        }

        private bool GridRemove(MultiTile tile)
        {
            if (!InBounds(tile.X, tile.Y) || !Cell(tile.X, tile.Y).Remove(tile))
            {
                return false;
            }

            --_tileCount;
            return true;
        }

        private void RebuildGrid(int width, int height, IEnumerable<MultiTile> tiles)
        {
            Width = width;
            Height = height;
            _cells = new List<MultiTile>[width * height];
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    _cells[(x * height) + y] = new List<MultiTile>();
                }
            }

            _tileCount = 0;
            foreach (MultiTile tile in tiles)
            {
                if (InBounds(tile.X, tile.Y))
                {
                    Cell(tile.X, tile.Y).Add(tile);
                    ++_tileCount;
                }
            }

            foreach (List<MultiTile> cell in _cells)
            {
                cell.Sort();
            }
        }

        /// <summary>
        /// Z is saved as a signed byte; larger values would wrap around.
        /// </summary>
        private static int ClampZ(int z) => Math.Clamp(z, sbyte.MinValue, sbyte.MaxValue);

        // ---------------------------------------------------------------- analysis

        public void CalcWalkable()
        {
            WalkableCount = 0;
            foreach (List<MultiTile> cell in _cells)
            {
                FillScratch(cell);
                foreach (MultiTile tile in _scratch)
                {
                    ItemData itemData = TileData.ItemTable[tile.Id];
                    if ((itemData.Flags & TileFlag.Surface) == 0)
                    {
                        continue;
                    }

                    if (tile.IsWalkable(tile.Z + itemData.CalcHeight, _scratch))
                    {
                        ++WalkableCount;
                    }
                }
            }
        }

        public void CalcDoubleSurface()
        {
            DoubleSurfaceCount = 0;
            foreach (List<MultiTile> cell in _cells)
            {
                FillScratch(cell);
                foreach (MultiTile tile in _scratch)
                {
                    if ((TileData.ItemTable[tile.Id].Flags & TileFlag.Surface) == 0)
                    {
                        continue;
                    }

                    if (tile.IsDoubleSurface(_scratch))
                    {
                        ++DoubleSurfaceCount;
                    }
                }
            }
        }

        private void FillScratch(List<MultiTile> cell)
        {
            _scratch.Clear();
            _scratch.AddRange(cell);
        }

        /// <summary>
        /// Recalculates bitmap size
        /// </summary>
        private void RecalculateMinMax()
        {
            // CalcEdgeTiles
            YMin = -44; // 0,0
            YMax = (Width + Height) * 22; // width,height
            XMin = (-Height * 22) - 22; // 0,height
            XMax = (Width * 22) + 22; // width,0
            ZMin = 127;
            ZMax = -128;

            foreach (MultiTile tile in RealTiles)
            {
                if (!ArtMetrics.TryGetSize(tile.Id, out int width, out int height))
                {
                    continue;
                }

                int px = tile.XMod - GapXMod;
                int py = tile.YMod - GapYMod;

                XMin = Math.Min(XMin, px);
                YMin = Math.Min(YMin, py);
                XMax = Math.Max(XMax, px + width);
                YMax = Math.Max(YMax, py + height);
                ZMax = Math.Max(ZMax, tile.Z);
                ZMin = Math.Min(ZMin, tile.Z);
            }

            XMinOrg = XMin;
            XMaxOrg = XMax;
            YMinOrg = YMin;
            YMaxOrg = YMax;

            if (_parent.ShowWalkables)
            {
                CalcWalkable();
            }

            if (_parent.ShowDoubleSurface)
            {
                CalcDoubleSurface();
            }
        }
    }
}
