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
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    internal sealed partial class RoofTool
    {
        // ---------------------------------------------------------------- planning

        /// <summary>
        /// The tiles a roof of these wings takes away and adds, in a roof style. An added tile
        /// with no layer goes on the roof layer; a re-pieced tile of an older roof names the layer
        /// it stays on. <paramref name="draftWalls"/> are walls about to be built with the roof (a
        /// house draft), which count as standing for the gable ends. <paramref name="hidden"/>
        /// tells roof cells that must not be built (inside a taller part of the house).
        /// </summary>
        public (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z, string Layer)> Add) PlanRoof(IReadOnlyList<RoofWing> wings,
            HouseStyle style, IReadOnlyList<(ushort Id, int X, int Y, int Z)> draftWalls, Func<int, int, int, bool> hidden)
        {
            var remove = new List<MultiTile>();
            var add = new List<(ushort Id, int X, int Y, int Z, string Layer)>();
            if (wings.Count == 0 || style == null)
            {
                return (remove, add);
            }

            _planStyle = style;
            _draftWalls = null;
            if (draftWalls != null)
            {
                _draftWalls = new Dictionary<(int X, int Y), List<(ushort Id, int Z)>>();
                foreach ((ushort id, int x, int y, int z) in draftWalls)
                {
                    if (!_draftWalls.TryGetValue((x, y), out List<(ushort Id, int Z)> there))
                    {
                        _draftWalls[(x, y)] = there = new List<(ushort Id, int Z)>();
                    }

                    there.Add((id, z));
                }
            }

            MultiEditorComponentList list = Context.CompList;
            var heights = new Dictionary<(int X, int Y), int>();
            var fixedPieces = new Dictionary<(int X, int Y), string>();
            foreach (RoofWing wing in wings)
            {
                RoofPlanner.AddWing(heights, fixedPieces, wing.Box, wing.RidgeAlongY, wing.Shape, wing.EaveZ);
            }

            if (hidden != null)
            {
                foreach (((int X, int Y) cell, int height) in new List<KeyValuePair<(int X, int Y), int>>(heights).Select(h => (h.Key, h.Value)))
                {
                    if (hidden(cell.X, cell.Y, height))
                    {
                        heights.Remove(cell);
                        fixedPieces.Remove(cell);
                    }
                }
            }

            var footprint = new HashSet<(int X, int Y)>(heights.Keys);
            JoinExisting(footprint, heights, fixedPieces, remove, add);

            foreach ((int x, int y) in footprint)
            {
                int lift = 0;
                if (fixedPieces.TryGetValue((x, y), out string column))
                {
                    // A hip's corner piece names how far below the slopes it sits.
                    int mark = column.IndexOf(RoofPlanner.LiftMark);
                    if (mark > 0)
                    {
                        lift = int.Parse(column.AsSpan(mark + 1));
                        column = column.Substring(0, mark);
                    }

                    // A hip's corner: a set with hip pieces of its own (ceramic) uses those, at the
                    // slopes' height, with the eave's piece where the hip line reaches the edge.
                    string hip = column switch { "NDent" => "HipNW", "EDent" => "HipNE", "SDent" => "HipSE", "WDent" => "HipSW", _ => null };
                    if (hip != null && style.Get(hip) != 0)
                    {
                        bool west = hip[^1] == 'W', north = hip[^2] == 'N';
                        bool atEdge = !heights.ContainsKey((x + (west ? -1 : 1), y)) && !heights.ContainsKey((x, y + (north ? -1 : 1)));
                        string atEave = "HipEave" + hip.Substring(3);
                        (column, lift) = (atEdge && style.Get(atEave) != 0 ? atEave : hip, 0);
                    }
                    else if (hip != null && style.Get(column) == 0)
                    {
                        // A style without corner pieces: the slope facing the same way.
                        (column, lift) = (column[0] switch { 'N' => "North", 'E' => "East", 'S' => "South", _ => "West" }, 0);
                    }
                }
                else
                {
                    column = RoofPlanner.Piece(heights, x, y, out string fallback) ?? (wings[0].RidgeAlongY ? "West" : "North");
                    if (style.Get(column) == 0 && fallback != null)
                    {
                        column = fallback;
                    }
                }

                // A slope at the roof's edge is the set's eave piece, when it has one.
                (int X, int Y) below = column switch
                {
                    "West" => (x - 1, y), "East" => (x + 1, y), "North" => (x, y - 1), "South" => (x, y + 1), _ => (int.MinValue, 0)
                };
                if (below.X != int.MinValue && !heights.ContainsKey(below) && style.Get("Eave" + column) != 0)
                {
                    column = "Eave" + column;
                }

                ushort id = style.Get(column);
                if (id != 0)
                {
                    add.Add((id, x, y, heights[(x, y)] + lift, null));
                }
            }

            // Caps from an earlier roof over these walls go too, so a turned ridge leaves none behind.
            foreach (RoofWing wing in wings)
            {
                for (int x = wing.X0 - 1; x <= wing.X1; ++x)
                {
                    for (int y = wing.Y0 - 1; y <= wing.Y1; ++y)
                    {
                        foreach (MultiTile tile in list.GetTilesAt(x, y))
                        {
                            if (IsCap(tile.Id) && list.IsShown(tile) && !remove.Contains(tile))
                            {
                                remove.Add(tile);
                            }
                        }
                    }
                }
            }

            // Every gable wing closes the ends it shows.
            var caps = new HashSet<(ushort, int, int, int)>();
            foreach (RoofWing wing in wings)
            {
                if (wing.Shape == RoofShape.Gable)
                {
                    AddGables(wing, heights, caps, add);
                }
            }

            return (remove, add);
        }

        /// <summary>
        /// Closes a gable's ends: in the wall row at the gable end, each wall cell is filled up
        /// to where the wing's slope has risen over it. An end that runs into another wing shows
        /// no gable and gets none: only cells with no roof beyond them are filled.
        /// </summary>
        private void AddGables(RoofWing wing, Dictionary<(int X, int Y), int> heights, HashSet<(ushort, int, int, int)> placed,
            List<(ushort Id, int X, int Y, int Z, string Layer)> add)
        {
            MultiEditorComponentList list = Context.CompList;
            bool ridgeAlongY = wing.RidgeAlongY;
            int z = wing.EaveZ;
            int wallX0 = wing.X0 - 1, wallX1 = wing.X1 - 1;
            int wallY0 = wing.Y0 - 1, wallY1 = wing.Y1 - 1;
            int across0 = ridgeAlongY ? wing.X0 : wing.Y0;
            int across1 = ridgeAlongY ? wing.X1 : wing.Y1;

            var gableRows = new List<(int Row, bool Front)>();
            if (Gables != RoofGables.None)
            {
                gableRows.Add((ridgeAlongY ? wallY1 : wallX1, true));
            }

            if (Gables == RoofGables.Both)
            {
                gableRows.Add((ridgeAlongY ? wallY0 : wallX0, false));
            }

            foreach ((int row, bool front) in gableRows)
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
                    int x = ridgeAlongY ? a : row;
                    int y = ridgeAlongY ? row : a;

                    // The front wall stands a cell inside the roof's last row, the back wall a
                    // cell before its first; the end shows where no roof lies past it.
                    (int X, int Y) beyond = front ? (ridgeAlongY ? (x, y + 2) : (x + 2, y)) : (x, y);
                    if (!list.InBounds(x, y) || heights.ContainsKey(beyond) || !HasStoreyWall(x, y, z))
                    {
                        continue;
                    }

                    int top = _rise * Math.Min(a - across0, across1 - a);
                    int filled = 0;
                    while (blockHeight > 0 && top - filled >= blockHeight)
                    {
                        if (placed.Add((block, x, y, z + filled)))
                        {
                            add.Add((block, x, y, z + filled, null));
                        }

                        filled += blockHeight;
                    }

                    while (cap != 0 && top - filled >= _rise)
                    {
                        if (placed.Add((cap, x, y, z + filled)))
                        {
                            add.Add((cap, x, y, z + filled, null));
                        }

                        filled += _rise;
                    }
                }
            }
        }

        /// <summary>A connected run of wall cells and the Z its walls end at.</summary>
        private readonly record struct WallBox(int X0, int Y0, int X1, int Y1, int Top, HashSet<(int X, int Y)> Cells);

        /// <summary>
        /// The cells a roof over these walls covers: the wall cells and everything they enclose,
        /// one cell further along X and Y. For walls that do not close it is the walls alone.
        /// </summary>
        private static HashSet<(int X, int Y)> RoofRegion(WallBox walls)
        {
            // Flood the outside, from a ring one cell around the box.
            var outside = new HashSet<(int X, int Y)>();
            var queue = new Queue<(int X, int Y)>();
            outside.Add((walls.X0 - 1, walls.Y0 - 1));
            queue.Enqueue((walls.X0 - 1, walls.Y0 - 1));
            while (queue.Count > 0)
            {
                (int x, int y) = queue.Dequeue();
                foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    (int X, int Y) next = (x + dx, y + dy);
                    if (next.X >= walls.X0 - 1 && next.X <= walls.X1 + 1 && next.Y >= walls.Y0 - 1 && next.Y <= walls.Y1 + 1 &&
                        !walls.Cells.Contains(next) && outside.Add(next))
                    {
                        queue.Enqueue(next);
                    }
                }
            }

            var region = new HashSet<(int X, int Y)>();
            for (int x = walls.X0; x <= walls.X1; ++x)
            {
                for (int y = walls.Y0; y <= walls.Y1; ++y)
                {
                    if (!outside.Contains((x, y)))
                    {
                        region.Add((x + 1, y + 1));
                    }
                }
            }

            return region;
        }

        /// <summary>
        /// Fits a new roof to the roof already there. A roof lying wholly under the new one is
        /// replaced. One that reaches beyond it is joined: each cell keeps the higher of the two,
        /// and the old cells next to the new roof get the piece their place now calls for (a
        /// valley, a T where the ridges meet), in their own style. Cells the old roof keeps are
        /// taken out of <paramref name="footprint"/>.
        /// </summary>
        private void JoinExisting(HashSet<(int X, int Y)> footprint, Dictionary<(int X, int Y), int> heights,
            Dictionary<(int X, int Y), string> fixedPieces, List<MultiTile> remove, List<(ushort Id, int X, int Y, int Z, string Layer)> add)
        {
            MultiEditorComponentList list = Context.CompList;
            var existing = new Dictionary<(int X, int Y), List<MultiTile>>();
            foreach (MultiTile tile in list.RealTiles)
            {
                if (!list.IsShown(tile) || !IsRoofPiece(tile.Id))
                {
                    continue;
                }

                if (!existing.TryGetValue((tile.X, tile.Y), out List<MultiTile> tiles))
                {
                    existing[(tile.X, tile.Y)] = tiles = new List<MultiTile>();
                }

                tiles.Add(tile);
            }

            var seen = new HashSet<(int X, int Y)>();
            var queue = new Queue<(int X, int Y)>();
            var kept = new Dictionary<(int X, int Y), MultiTile>();
            foreach ((int X, int Y) start in new List<(int X, int Y)>(footprint))
            {
                if (!existing.ContainsKey(start) || !seen.Add(start))
                {
                    continue;
                }

                var roof = new List<(int X, int Y)>();
                bool inside = true;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    (int x, int y) = queue.Dequeue();
                    roof.Add((x, y));
                    inside &= footprint.Contains((x, y)) || kept.ContainsKey((x, y));
                    foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        if (existing.ContainsKey((x + dx, y + dy)) && seen.Add((x + dx, y + dy)))
                        {
                            queue.Enqueue((x + dx, y + dy));
                        }
                    }
                }

                foreach ((int X, int Y) cell in roof)
                {
                    List<MultiTile> tiles = existing[cell];
                    MultiTile top = tiles[0];
                    foreach (MultiTile tile in tiles)
                    {
                        if (tile.Z > top.Z)
                        {
                            top = tile;
                        }
                    }

                    if (inside || (footprint.Contains(cell) && heights[cell] > top.Z))
                    {
                        remove.AddRange(tiles);
                        continue;
                    }

                    heights[cell] = top.Z;
                    fixedPieces.Remove(cell);
                    footprint.Remove(cell);
                    kept[cell] = top;
                }
            }

            foreach (((int x, int y), MultiTile tile) in kept)
            {
                bool beside = false;
                for (int dx = -2; dx <= 2 && !beside; ++dx)
                {
                    for (int dy = -2; dy <= 2 && !beside; ++dy)
                    {
                        beside = footprint.Contains((x + dx, y + dy));
                    }
                }

                string fallback = null;
                string column = beside ? RoofPlanner.Piece(heights, x, y, out fallback) : null;
                if (column == null)
                {
                    continue;
                }

                HouseStyle style = _planStyle;
                string now = null;
                if (Context.HouseStyles.TryGetRoleByArt(tile.Id, out HouseStyle owner, out string slot, out _) && owner.Kind == HousePartKind.Roof)
                {
                    (style, now) = (owner, slot);
                }

                if (now == column || (style.Get(column) == 0 && now == fallback))
                {
                    continue;
                }

                ushort id = style.Get(column) != 0 ? style.Get(column) : style.Get(fallback ?? column);
                if (id != 0 && id != tile.Id)
                {
                    remove.Add(tile);
                    add.Add((id, x, y, tile.Z, tile.Layer.Name));
                }
            }
        }

        private bool IsRoofPiece(ushort id)
        {
            return Context.HouseStyles.TryGetRole(id, out HouseStyle owner, out _)
                ? owner.Kind == HousePartKind.Roof
                : (TileData.ItemTable[id].Flags & TileFlag.Roof) != 0;
        }

        /// <summary>
        /// The walls a stroke means, among the walls standing on or just below
        /// <paramref name="z"/>. A click (one cell) takes the smallest box of connected wall cells
        /// around it. A drag takes the box whose roof it matches to within a cell on every side.
        /// False when no walls fit.
        /// </summary>
        private bool FitToWalls(int x0, int y0, int x1, int y1, int z, out WallBox box)
        {
            box = default;
            bool click = x0 == x1 && y0 == y1;
            long best = long.MaxValue;
            foreach (WallBox walls in WallBoxes(z))
            {
                long score;
                if (click)
                {
                    if (x0 < walls.X0 || x0 > walls.X1 || y0 < walls.Y0 || y0 > walls.Y1)
                    {
                        continue;
                    }

                    score = (long)(walls.X1 - walls.X0 + 1) * (walls.Y1 - walls.Y0 + 1);
                }
                else
                {
                    int dx0 = Math.Abs(x0 - (walls.X0 + 1)), dx1 = Math.Abs(x1 - (walls.X1 + 1));
                    int dy0 = Math.Abs(y0 - (walls.Y0 + 1)), dy1 = Math.Abs(y1 - (walls.Y1 + 1));
                    if (Math.Max(Math.Max(dx0, dx1), Math.Max(dy0, dy1)) > 1)
                    {
                        continue;
                    }

                    score = dx0 + dx1 + dy0 + dy1;
                }

                if (score < best)
                {
                    best = score;
                    box = walls;
                }
            }

            return best != long.MaxValue;
        }

        /// <summary>
        /// The boxes of connected wall cells of the storey that stands on or just below
        /// <paramref name="z"/> (the work plane, or the top of what the mouse points at), each with
        /// the Z its walls end at. Walls a little short of a storey count as a full one, as the
        /// official houses roof their 19 high plaster walls at 20.
        /// </summary>
        private List<WallBox> WallBoxes(int z)
        {
            MultiEditorComponentList list = Context.CompList;
            var tops = new Dictionary<(int, int), int>();
            foreach (MultiTile tile in list.RealTiles)
            {
                if (tile.Z > z || tile.Z <= z - _storey - 5 || !list.IsShown(tile) || !IsStoreyWall(tile))
                {
                    continue;
                }

                int height = tile.Height >= _storey - 3 ? Math.Max(tile.Height, _storey) : tile.Height;
                int top = tile.Z + height;
                if (!tops.TryGetValue((tile.X, tile.Y), out int known) || top > known)
                {
                    tops[(tile.X, tile.Y)] = top;
                }
            }

            var boxes = new List<WallBox>();
            var seen = new HashSet<(int, int)>();
            var queue = new Queue<(int X, int Y)>();
            var counts = new Dictionary<int, int>();
            foreach ((int, int) start in tops.Keys)
            {
                if (!seen.Add(start))
                {
                    continue;
                }

                int x0 = start.Item1, y0 = start.Item2, x1 = x0, y1 = y0;
                var cells = new HashSet<(int X, int Y)>();
                counts.Clear();
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    (int x, int y) = queue.Dequeue();
                    cells.Add((x, y));
                    x0 = Math.Min(x0, x);
                    y0 = Math.Min(y0, y);
                    x1 = Math.Max(x1, x);
                    y1 = Math.Max(y1, y);
                    int top = tops[(x, y)];
                    counts[top] = counts.GetValueOrDefault(top) + 1;
                    foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        (int, int) next = (x + dx, y + dy);
                        if (tops.ContainsKey(next) && seen.Add(next))
                        {
                            queue.Enqueue(next);
                        }
                    }
                }

                if (cells.Count > 1)
                {
                    int common = counts.OrderByDescending(c => c.Value).ThenByDescending(c => c.Key).First().Key;
                    boxes.Add(new WallBox(x0, y0, x1, y1, common, cells));
                }
            }

            return boxes;
        }

        /// <summary>True when a wall of the storey below <paramref name="z"/> stands in the cell.</summary>
        private bool HasStoreyWall(int x, int y, int z)
        {
            foreach (MultiTile tile in Context.CompList.GetTilesAt(x, y))
            {
                if (tile.Z < z && tile.Z > z - _storey - 5 && IsStoreyWall(tile))
                {
                    return true;
                }
            }

            if (_draftWalls != null && _draftWalls.TryGetValue((x, y), out List<(ushort Id, int Z)> planned))
            {
                foreach ((ushort id, int wallZ) in planned)
                {
                    if (wallZ < z && wallZ > z - _storey - 5 && TileData.ItemTable[id].Height >= _storey / 2)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>A wall, window or door at least half a storey high; not a gable cap or a roof piece.</summary>
        private bool IsStoreyWall(MultiTile tile)
        {
            if (tile.Height < _storey / 2)
            {
                return false;
            }

            if (Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out _))
            {
                return owner.Kind is HousePartKind.Wall or HousePartKind.Door;
            }

            TileFlag flags = TileData.ItemTable[tile.Id].Flags;
            return (flags & (TileFlag.Wall | TileFlag.Window | TileFlag.Door)) != 0 && (flags & TileFlag.Roof) == 0;
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
                (int X, int Y) cell = (alongX ? a : row, alongX ? row : a);
                foreach (MultiTile tile in list.GetTilesAt(cell.X, cell.Y))
                {
                    if (tile.Z < z && Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out _) && owner.Kind == HousePartKind.Wall)
                    {
                        wall = tile.Id;
                        break;
                    }
                }

                // The top storey of a house being drafted: the walls the gable stands on.
                if (wall == 0 && _draftWalls != null && _draftWalls.TryGetValue(cell, out List<(ushort Id, int Z)> planned))
                {
                    foreach ((ushort id, int wallZ) in planned.OrderByDescending(p => p.Z))
                    {
                        if (wallZ < z && Context.HouseStyles.TryGetRole(id, out HouseStyle owner, out _) && owner.Kind == HousePartKind.Wall)
                        {
                            wall = id;
                            break;
                        }
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
}
