/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Fills the dragged rectangle with a floor style, each cell a random piece of the style.
    /// Flat tiles already at that Z in those cells are replaced.
    /// </summary>
    internal sealed class FloorStyleTool : PlaneDragTool
    {
        private int _seed = Environment.TickCount;

        public FloorStyleTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Floor;

        protected override string ActionName => "Floor";

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            ushort[] pieces = Style.Slots.Values.Distinct().ToArray();
            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            MultiEditorComponentList list = Context.CompList;

            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); ++x)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); ++y)
                {
                    if (!list.InBounds(x, y))
                    {
                        continue;
                    }

                    // Same seed for preview and edit, so what is shown is what lands.
                    ushort id = pieces[(int)((uint)HashCode.Combine(x, y, _seed) % (uint)pieces.Length)];
                    remove.AddRange(list.GetTilesAt(x, y).Where(t => t.Z == z && t.Height == 0 && list.IsShown(t)));
                    add.Add((id, x, y, z));
                }
            }

            if (final)
            {
                _seed = Environment.TickCount ^ _seed;
            }

            return (remove, add);
        }
    }

    /// <summary>Which way a gable roof's ridge runs.</summary>
    internal enum RoofRidge
    {
        /// <summary>As the official houses: along the longer side, or so the ridge gets its crosspiece.</summary>
        Auto,
        AlongX,
        AlongY
    }

    /// <summary>Which gable ends of a roof are closed.</summary>
    internal enum RoofGables
    {
        /// <summary>The south or east end, as the official houses do.</summary>
        Front,
        Both,
        None
    }

    /// <summary>What closes a gable.</summary>
    internal enum RoofGableFill
    {
        /// <summary>3 high cap pieces matching the walls.</summary>
        Caps,

        /// <summary>Whole wall pieces as far as they fit under the slope, caps for the rest: for big, high roofs.</summary>
        Walls
    }

    /// <summary>
    /// A gable roof over a house. A click on (or inside) a walled room fits the roof to its walls;
    /// a drag over the wall cells sizes it by hand, as with the wall tool. The slopes rise 3 per
    /// cell from the eaves, like the official house roofs: West pieces climb to an NS crosspiece
    /// and East pieces come down (North / EW crosspiece / South the other way). Roof pieces
    /// already in those cells are replaced. <see cref="Ridge"/>, <see cref="Gables"/> and
    /// <see cref="GableFill"/> choose the shape; Shift turns the ridge and Ctrl switches between
    /// one and both gables for a single stroke.
    /// </summary>
    internal sealed class RoofTool : PlaneDragTool
    {
        private const int _rise = 3;
        private const string _capStyles = "Miscellaneous Roof Pieces";

        public RoofRidge Ridge { get; set; } = RoofRidge.Auto;

        public RoofGables Gables { get; set; } = RoofGables.Front;

        public RoofGableFill GableFill { get; set; } = RoofGableFill.Caps;

        public RoofTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Roof;

        protected override string ActionName => "Roof";

        /// <summary>
        /// The eaves go on the work plane, or on top of what the mouse points at (a wall top).
        /// </summary>
        protected override void BaseCell(out int x, out int y, out int z)
        {
            Context.CellAt(Context.MouseLoc, out x, out y, out z);
            if (Context.WorkZ is { } baseZ)
            {
                z = baseZ;
                Context.PlaneCell(Context.MouseLoc, z, out x, out y);
            }
        }

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            MultiEditorComponentList list = Context.CompList;

            // Dragged like the wall tool, over the wall cells. Walls sit on the +X/+Y edges of their
            // cells, so the roof over them is the same box one cell further along X and Y (as in the
            // official houses, which overhang their east and south walls by a cell).
            int wallX0 = Math.Min(startX, endX), wallX1 = Math.Max(startX, endX);
            int wallY0 = Math.Min(startY, endY), wallY1 = Math.Max(startY, endY);

            // A click, not a drag: take the walls of the room it points at.
            if (startX == endX && startY == endY && FitToWalls(startX, startY, z, out var fitted))
            {
                (wallX0, wallY0, wallX1, wallY1) = fitted;
            }

            int x0 = wallX0 + 1, x1 = wallX1 + 1;
            int y0 = wallY0 + 1, y1 = wallY1 + 1;

            // Like the official houses: the slopes go across the odd side, so the ridge gets its
            // crosspiece; otherwise the ridge runs along the longer side, along Y when square.
            int spanX = x1 - x0 + 1, spanY = y1 - y0 + 1;
            bool ridgeAlongY = Ridge switch
            {
                RoofRidge.AlongX => false,
                RoofRidge.AlongY => true,
                _ => spanX % 2 != spanY % 2 ? spanX % 2 == 1 : spanY >= spanX
            };
            ridgeAlongY ^= ShiftHeld;
            int across0 = ridgeAlongY ? x0 : y0;
            int across1 = ridgeAlongY ? x1 : y1;
            int span = across1 - across0 + 1;
            string low = ridgeAlongY ? "West" : "North";
            string high = ridgeAlongY ? "East" : "South";
            string ridge = ridgeAlongY ? "NSCrosspiece" : "EWCrosspiece";

            for (int x = x0; x <= x1; ++x)
            {
                for (int y = y0; y <= y1; ++y)
                {
                    if (!list.InBounds(x, y))
                    {
                        continue;
                    }

                    int a = ridgeAlongY ? x : y;
                    int fromLow = a - across0;
                    int fromHigh = across1 - a;
                    string column = span % 2 == 1 && fromLow == fromHigh ? ridge : fromLow < fromHigh ? low : high;
                    ushort id = Style.Get(column);
                    if (id == 0)
                    {
                        continue;
                    }

                    foreach (MultiTile tile in list.GetTilesAt(x, y))
                    {
                        if (Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out _) && owner.Kind == HousePartKind.Roof)
                        {
                            remove.Add(tile);
                        }
                    }

                    add.Add((id, x, y, z + (_rise * Math.Min(fromLow, fromHigh))));
                }
            }

            // Caps from an earlier roof over these walls go too, so a turned ridge leaves none behind.
            for (int x = wallX0; x <= x1; ++x)
            {
                for (int y = wallY0; y <= y1; ++y)
                {
                    foreach (MultiTile tile in list.GetTilesAt(x, y))
                    {
                        if (IsCap(tile.Id))
                        {
                            remove.Add(tile);
                        }
                    }
                }
            }

            // Gables: in the wall row at the gable end, filled up to where the slope has risen there.
            RoofGables gables = Gables;
            if (CtrlHeld)
            {
                gables = gables == RoofGables.Both ? RoofGables.Front : RoofGables.Both;
            }

            var gableRows = new List<int>();
            if (gables != RoofGables.None)
            {
                gableRows.Add(ridgeAlongY ? wallY1 : wallX1);
            }

            if (gables == RoofGables.Both)
            {
                gableRows.Add(ridgeAlongY ? wallY0 : wallX0);
            }

            foreach (int row in gableRows)
            {
                ushort wall = GableWall(row, ridgeAlongY, wallX0, wallX1, wallY0, wallY1, z);
                ushort cap = wall != 0 ? CapFor(wall, ridgeAlongY) : (ushort)0;

                // Whole walls: the plain piece of the wall's own style that runs along the gable.
                ushort block = 0;
                int blockHeight = 0;
                if (GableFill == RoofGableFill.Walls && wall != 0)
                {
                    block = wall;
                    if (Context.HouseStyles.TryGetRole(wall, out HouseStyle wallStyle, out _) &&
                        wallStyle.Get(ridgeAlongY ? "South2" : "East2") is var plain and not 0)
                    {
                        block = plain;
                    }

                    blockHeight = TileData.ItemTable[block].Height;
                }

                if (cap == 0 && blockHeight <= 0)
                {
                    continue;
                }

                for (int a = across0; a <= across1; ++a)
                {
                    int top = _rise * Math.Min(a - across0, across1 - a);
                    int x = ridgeAlongY ? a : row;
                    int y = ridgeAlongY ? row : a;
                    if (!list.InBounds(x, y))
                    {
                        continue;
                    }

                    int filled = 0;
                    while (blockHeight > 0 && top - filled >= blockHeight)
                    {
                        add.Add((block, x, y, z + filled));
                        filled += blockHeight;
                    }

                    while (cap != 0 && top - filled >= _rise)
                    {
                        add.Add((cap, x, y, z + filled));
                        filled += _rise;
                    }
                }
            }

            return (remove, add);
        }

        /// <summary>
        /// The walls a click means: the box of the connected run of wall cells, on the level just
        /// below <paramref name="z"/>, that holds the clicked cell. A click inside a room takes the
        /// smallest such box around it. False when there are no walls there.
        /// </summary>
        private bool FitToWalls(int cx, int cy, int z, out (int X0, int Y0, int X1, int Y1) box)
        {
            MultiEditorComponentList list = Context.CompList;
            bool IsWall(int x, int y)
            {
                foreach (MultiTile tile in list.GetTilesAt(x, y))
                {
                    if (tile.Z < z && tile.Z >= z - 20 &&
                        Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out _) && owner.Kind == HousePartKind.Wall)
                    {
                        return true;
                    }
                }

                return false;
            }

            box = default;
            long bestArea = long.MaxValue;
            var seen = new HashSet<(int, int)>();
            var queue = new Queue<(int X, int Y)>();
            for (int sx = 0; sx < list.Width; ++sx)
            {
                for (int sy = 0; sy < list.Height; ++sy)
                {
                    if (seen.Contains((sx, sy)) || !IsWall(sx, sy))
                    {
                        continue;
                    }

                    int x0 = sx, y0 = sy, x1 = sx, y1 = sy, cells = 0;
                    seen.Add((sx, sy));
                    queue.Enqueue((sx, sy));
                    while (queue.Count > 0)
                    {
                        (int x, int y) = queue.Dequeue();
                        ++cells;
                        x0 = Math.Min(x0, x);
                        y0 = Math.Min(y0, y);
                        x1 = Math.Max(x1, x);
                        y1 = Math.Max(y1, y);
                        foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                        {
                            (int nx, int ny) = (x + dx, y + dy);
                            if (list.InBounds(nx, ny) && !seen.Contains((nx, ny)) && IsWall(nx, ny))
                            {
                                seen.Add((nx, ny));
                                queue.Enqueue((nx, ny));
                            }
                        }
                    }

                    long area = (long)(x1 - x0 + 1) * (y1 - y0 + 1);
                    if (cells > 1 && cx >= x0 && cx <= x1 && cy >= y0 && cy <= y1 && area < bestArea)
                    {
                        bestArea = area;
                        box = (x0, y0, x1, y1);
                    }
                }
            }

            return bestArea != long.MaxValue;
        }

        private bool IsCap(ushort id)
        {
            return Context.HouseStyles.TryGetRole(id, out HouseStyle owner, out _) &&
                   owner.Kind == HousePartKind.Misc && owner.Group.StartsWith(_capStyles, StringComparison.Ordinal);
        }

        /// <summary>A wall tile below the eaves in a gable wall row, or 0 when the row has none.</summary>
        private ushort GableWall(int row, bool alongX, int wallX0, int wallX1, int wallY0, int wallY1, int z)
        {
            MultiEditorComponentList list = Context.CompList;
            ushort wall = 0;
            int from = alongX ? wallX0 : wallY0, to = alongX ? wallX1 : wallY1;
            for (int a = from; a <= to && wall == 0; ++a)
            {
                foreach (MultiTile tile in list.GetTilesAt(alongX ? a : row, alongX ? row : a))
                {
                    if (tile.Z < z && Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out _) && owner.Kind == HousePartKind.Wall)
                    {
                        wall = tile.Id;
                        break;
                    }
                }
            }

            return wall;
        }

        /// <summary>
        /// The cap for a gable over a wall: the misc.txt roof piece with the wall's tiledata name and
        /// the id nearest its own. Odd pieces close walls along X, even pieces walls along Y (as
        /// placed in the official multis). 0 when no cap matches.
        /// </summary>
        private ushort CapFor(ushort wall, bool alongX)
        {
            string name = TileData.ItemTable[wall].Name;
            ushort best = 0;
            foreach (HouseStyle style in Context.HouseStyles.Styles)
            {
                if (style.Kind != HousePartKind.Misc || !style.Group.StartsWith(_capStyles, StringComparison.Ordinal))
                {
                    continue;
                }

                for (int pair = 1; pair <= 7; pair += 2)
                {
                    ushort first = style.Get($"Piece{pair}");
                    ushort second = style.Get($"Piece{pair + 1}");
                    ushort cap = alongX ? first : second;
                    if (first == 0 || second == 0 || TileData.ItemTable[cap].Height > 4 ||
                        !string.Equals(TileData.ItemTable[cap].Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (best == 0 || Math.Abs(cap - wall) < Math.Abs(best - wall))
                    {
                        best = cap;
                    }
                }
            }

            return best;
        }
    }

    /// <summary>
    /// A house foundation like the client's customizable house multis: the dragged rectangle gets
    /// a ring of 5 high foundation walls at the base Z and a dirt floor 7 above it inside.
    /// </summary>
    internal sealed class FoundationTool : PlaneDragTool
    {
        // The foundation multis use Light Brick Quarter Walls (0x63-0x66) and the dirt floor.
        public const ushort FoundationPost = 0x0066;
        private const ushort _dirt = 0x31F4;
        private const int _floorAbove = 7;

        public FoundationTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Wall;

        protected override string ActionName => "Foundation";

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            var planner = new WallPlanner(Context.CompList, Context.HouseStyles, Style, z);
            var (south, east) = WallPlanner.DragEdges(startX, startY, endX, endY, line: false);
            var (remove, add) = planner.Plan(south, east, erase: false, Context.CompList.Width, Context.CompList.Height);

            // The foundation multis floor every cell with the same dirt piece.
            for (int x = Math.Min(startX, endX) + 1; x <= Math.Max(startX, endX); ++x)
            {
                for (int y = Math.Min(startY, endY) + 1; y <= Math.Max(startY, endY); ++y)
                {
                    if (Context.CompList.InBounds(x, y))
                    {
                        add.Add((_dirt, x, y, z + _floorAbove));
                    }
                }
            }

            return (remove, add);
        }
    }

    /// <summary>
    /// Puts a door of the chosen style into the wall in the clicked cell, facing along the wall.
    /// Clicking a door again turns it to its next hinge variant.
    /// </summary>
    internal sealed class DoorTool : PlaneDragTool
    {
        // Hinge variants per wall direction, in the order a click cycles through them.
        private static readonly string[] _alongX = { "Piece3", "Piece1", "Piece4", "Piece2" };
        private static readonly string[] _alongY = { "Piece5", "Piece7", "Piece6", "Piece8" };

        public DoorTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Door;

        protected override string ActionName => "Door";

        /// <summary>
        /// The hovered wall or door when there is one: a click on a wall means that wall, not the
        /// floor cell behind it.
        /// </summary>
        protected override void BaseCell(out int x, out int y, out int z)
        {
            if (Context.HoverTile is { } hover && Context.HouseStyles.TryGetRole(hover.Id, out HouseStyle owner, out _) &&
                owner.Kind is HousePartKind.Wall or HousePartKind.Door)
            {
                (x, y, z) = (hover.X, hover.Y, hover.Z);
                return;
            }

            base.BaseCell(out x, out y, out z);
        }

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            MultiEditorComponentList list = Context.CompList;
            if (!list.InBounds(startX, startY))
            {
                return (remove, add);
            }

            // Default: a wall along X. A wall piece or door already in the cell decides the direction.
            string[] variants = _alongX;
            int next = 0;
            foreach (MultiTile tile in list.GetTilesAt(startX, startY))
            {
                if (tile.Z != z || !Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out string slot))
                {
                    continue;
                }

                if (owner.Kind == HousePartKind.Door)
                {
                    variants = WallPlanner.DoorAlongX(slot) ? _alongX : _alongY;
                    next = owner == Style ? (Array.IndexOf(variants, slot) + 1) % variants.Length : 0;
                    remove.Add(tile);
                }
                else if (owner.Kind == HousePartKind.Wall)
                {
                    if (slot is "Corner" or "Post")
                    {
                        // No door fits a corner.
                        return (new List<MultiTile>(), add);
                    }

                    variants = slot.StartsWith("East", StringComparison.Ordinal) || slot.EndsWith('E') ? _alongY : _alongX;
                    remove.Add(tile);
                }
            }

            // Fall back to any piece the style has, in cycle order.
            for (int i = 0; i < variants.Length; ++i)
            {
                ushort id = Style.Get(variants[(next + i) % variants.Length]);
                if (id != 0)
                {
                    add.Add((id, startX, startY, z));
                    break;
                }
            }

            return (remove, add);
        }
    }

    /// <summary>
    /// Places the chosen style's stair multi: press on the bottom step and drag the way the stairs
    /// should climb (a click climbs north). The stairs rise one level (20).
    /// </summary>
    internal sealed class StairTool : PlaneDragTool
    {
        private const ushort _noDraw = 0x0001;

        public StairTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Stair;

        protected override string ActionName => "Stairs";

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            var add = new List<(ushort, int, int, int)>();
            int dx = endX - startX;
            int dy = endY - startY;
            string column = Math.Abs(dx) > Math.Abs(dy)
                ? (dx > 0 ? "MultiEast" : "MultiWest")
                : (dy > 0 ? "MultiSouth" : "MultiNorth");

            ushort multiId = Style.Get(column);
            MultiComponentList stairs = multiId != 0 ? Multis.GetComponents(multiId) : MultiComponentList.Empty;
            if (stairs == MultiComponentList.Empty || stairs.Width == 0)
            {
                return (new List<MultiTile>(), add);
            }

            // The lowest step goes under the cursor.
            (int X, int Y) bottom = (0, 0);
            int lowest = int.MaxValue;
            for (int x = 0; x < stairs.Width; ++x)
            {
                for (int y = 0; y < stairs.Height; ++y)
                {
                    int top = stairs.Tiles[x][y].Where(t => t.Id != _noDraw).Select(t => t.Z + TileData.ItemTable[t.Id].Height).DefaultIfEmpty(int.MaxValue).Max();
                    if (top < lowest)
                    {
                        lowest = top;
                        bottom = (x, y);
                    }
                }
            }

            for (int x = 0; x < stairs.Width; ++x)
            {
                for (int y = 0; y < stairs.Height; ++y)
                {
                    foreach (MTile tile in stairs.Tiles[x][y])
                    {
                        if (tile.Id != _noDraw)
                        {
                            add.Add((tile.Id, startX + x - bottom.X, startY + y - bottom.Y, z + tile.Z));
                        }
                    }
                }
            }

            return (new List<MultiTile>(), add);
        }
    }
}
