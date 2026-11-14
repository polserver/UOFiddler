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

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    internal enum GroupTransformKind
    {
        RotateLeft,
        RotateRight,
        MirrorX,
        MirrorY
    }

    /// <summary>
    /// Rotates or mirrors a group of tiles. Graphics in UO never turn, so a piece that faces a way
    /// (a stair, roof slope, door) is swapped for the piece that faces the new way.
    /// <list type="bullet">
    /// <item>Walls sit on cell edges, not in cells: they are turned into edges, the edges
    /// transformed, and the wall pieces chosen again, so corners and posts come out right.</item>
    /// <item>Pieces the client's house tables name by direction (roof slopes, stairs) take the
    /// piece of their style for the new direction. Classic tiles that copy such a piece count too.</item>
    /// <item>Everything else is turned by its art (<see cref="ArtFacing"/>): the twin with the
    /// mirrored picture is the same piece along the other axis. The facing table is used where it
    /// does not contradict the art.</item>
    /// </list>
    /// </summary>
    internal static class GroupTransform
    {
        private static readonly Dictionary<string, (int X, int Y)> _directions = new Dictionary<string, (int, int)>
        {
            ["North"] = (0, -1), ["East"] = (1, 0), ["South"] = (0, 1), ["West"] = (-1, 0)
        };

        // Door pieces: the end of the wall the hinge is at, and the swing.
        private static readonly Dictionary<string, ((int X, int Y) Hinge, bool Clockwise)> _doors = new Dictionary<string, ((int, int), bool)>
        {
            ["Piece3"] = ((-1, 0), true), ["Piece4"] = ((1, 0), false), ["Piece1"] = ((-1, 0), false), ["Piece2"] = ((1, 0), true),
            ["Piece5"] = ((0, 1), true), ["Piece6"] = ((0, -1), false), ["Piece7"] = ((0, 1), false), ["Piece8"] = ((0, -1), true)
        };

        private static readonly string[] _southColumns = { "South1", "South2", "South3", "WindowS", "AltWindowS", "SecondAltWindowS" };
        private static readonly string[] _eastColumns = { "East3", "East2", "East1", "WindowE", "AltWindowE", "SecondAltWindowE" };

        private static (int A, int B, int C, int D) Matrix(GroupTransformKind kind) => kind switch
        {
            // (x, y) -> (A*x + B*y, C*x + D*y)
            GroupTransformKind.RotateLeft => (0, 1, -1, 0),
            GroupTransformKind.RotateRight => (0, -1, 1, 0),
            GroupTransformKind.MirrorX => (-1, 0, 0, 1),
            _ => (1, 0, 0, -1)
        };

        public static bool SwapsAxes(GroupTransformKind kind) => kind is GroupTransformKind.RotateLeft or GroupTransformKind.RotateRight;

        /// <summary>
        /// The tiles to add for the transformed group, placed so the group's box keeps its top-left
        /// cell. Every tile of <paramref name="group"/> is replaced.
        /// </summary>
        public static List<(ushort Id, int X, int Y, int Z, bool Invisible, int Extra)> Apply(
            IReadOnlyCollection<MultiTile> group, HouseStyles styles, RotationTable table, GroupTransformKind kind)
        {
            var m = Matrix(kind);
            bool mirror = (m.A * m.D) - (m.B * m.C) < 0;
            bool swaps = SwapsAxes(kind);

            (double X, double Y) Map(double x, double y) => ((m.A * x) + (m.B * y), (m.C * x) + (m.D * y));
            (int X, int Y) MapCell(int x, int y)
            {
                var c = Map(x + 0.5, y + 0.5);
                return ((int)Math.Floor(c.X), (int)Math.Floor(c.Y));
            }

            // An edge of cell (x, y): its midpoint and direction move; the result is a +Y edge (S)
            // or a +X edge (E) of some cell.
            (bool South, int X, int Y) MapEdge(bool south, int x, int y)
            {
                var mid = south ? Map(x + 0.5, y + 1) : Map(x + 1, y + 0.5);
                var dir = south ? Map(1, 0) : Map(0, 1);
                return Math.Abs(dir.X) > 0.5
                    ? (true, (int)Math.Floor(mid.X), (int)Math.Round(mid.Y) - 1)
                    : (false, (int)Math.Round(mid.X) - 1, (int)Math.Floor(mid.Y));
            }

            // A post stands on the cell's +X/+Y corner: that point moves.
            (int X, int Y) MapVertex(int x, int y)
            {
                var v = Map(x + 1, y + 1);
                return ((int)Math.Round(v.X) - 1, (int)Math.Round(v.Y) - 1);
            }

            // A roof piece covers the area around its cell's -X/-Y corner: that point moves.
            (int X, int Y) MapLifted(int x, int y)
            {
                var v = Map(x, y);
                return ((int)Math.Round(v.X), (int)Math.Round(v.Y));
            }

            string MapDirection(string label)
            {
                var d = _directions[label];
                var t = Map(d.X, d.Y);
                return _directions.First(kv => kv.Value == ((int)Math.Round(t.X), (int)Math.Round(t.Y))).Key;
            }

            // Keep the group's top-left: shift by the change of the cell box's corner.
            int minX = group.Min(t => t.X), minY = group.Min(t => t.Y);
            var mapped = group.Select(t => MapCell(t.X, t.Y)).ToList();
            int shiftX = minX - mapped.Min(c => c.X);
            int shiftY = minY - mapped.Min(c => c.Y);

            var result = new List<(ushort, int, int, int, bool, int)>();
            var walls = new Dictionary<(HouseStyle Style, int Z), WallLayer>();
            var doorCells = new HashSet<(int, int, int)>();
            var doorEdges = new List<(int Z, bool South, int X, int Y)>();

            WallLayer Layer(HouseStyle style, int z)
            {
                if (!walls.TryGetValue((style, z), out WallLayer layer))
                {
                    walls[(style, z)] = layer = new WallLayer();
                }

                return layer;
            }

            foreach (MultiTile tile in group)
            {
                styles.TryGetRoleByArt(tile.Id, out HouseStyle style, out string slot, out bool exact);

                if (style?.Kind == HousePartKind.Wall && exact)
                {
                    WallLayer layer = Layer(style, tile.Z);
                    if (slot == "Post")
                    {
                        // Posts are rebuilt from the edges; one standing alone is put back afterwards.
                        var v = MapVertex(tile.X, tile.Y);
                        layer.Posts.Add((v.X + shiftX, v.Y + shiftY, tile.Id));
                        continue;
                    }

                    bool isSouth = slot == "Corner" || Array.IndexOf(_southColumns, slot) >= 0;
                    bool isEast = slot == "Corner" || Array.IndexOf(_eastColumns, slot) >= 0;
                    foreach (bool south in new[] { true, false })
                    {
                        if (south ? !isSouth : !isEast)
                        {
                            continue;
                        }

                        var e = MapEdge(south, tile.X, tile.Y);
                        var cell = (e.X + shiftX, e.Y + shiftY);
                        (e.South ? layer.S : layer.E).Add(cell);

                        if (slot is not ("Corner" or "South1" or "South2" or "South3" or "East1" or "East2" or "East3"))
                        {
                            // A window keeps its look: the same piece, or its twin along the other axis.
                            ushort keep = e.South == south ? tile.Id : WallTwin(styles, style, tile.Id, slot);
                            if (keep != 0)
                            {
                                (e.South ? layer.KeepS : layer.KeepE)[cell] = keep;
                            }
                        }
                    }

                    continue;
                }

                TilePlacement placement = ArtFacing.Placement(tile.Id);
                var (nx, ny) = placement == TilePlacement.Lifted ? MapLifted(tile.X, tile.Y) : MapCell(tile.X, tile.Y);
                ushort id = tile.Id;

                if (style?.Kind == HousePartKind.Door && exact && _doors.TryGetValue(slot, out var door))
                {
                    // A door is an edge too: it moves like one and swaps to the piece hinged the new way.
                    var e = MapEdge(WallPlanner.DoorAlongX(slot), tile.X, tile.Y);
                    (nx, ny) = (e.X, e.Y);
                    var h = Map(door.Hinge.X, door.Hinge.Y);
                    var hinge = ((int)Math.Round(h.X), (int)Math.Round(h.Y));
                    bool clockwise = door.Clockwise ^ mirror;
                    string newSlot = _doors.First(kv => kv.Value == (hinge, clockwise)).Key;
                    id = style.Get(newSlot) is var doorId and not 0 ? doorId : id;
                    doorCells.Add((nx + shiftX, ny + shiftY, tile.Z));
                    doorEdges.Add((tile.Z, e.South, nx + shiftX, ny + shiftY));
                    result.Add((id, nx + shiftX, ny + shiftY, tile.Z, tile.Invisible, tile.Extra));
                    continue;
                }

                string turnedSlot = style?.Kind is HousePartKind.Roof or HousePartKind.Stair ? TurnSlot(slot, MapDirection, swaps) : null;
                if (styles.TryGetStairCorner(tile.Id, out ushort[] corners, out int corner))
                {
                    // Corners go south-east, north-east, north-west, south-west on a left turn.
                    id = corners[kind switch
                    {
                        GroupTransformKind.RotateLeft => (corner + 1) % 4,
                        GroupTransformKind.RotateRight => (corner + 3) % 4,
                        GroupTransformKind.MirrorX => 3 - corner,
                        _ => corner ^ 1
                    }];
                }
                else if (turnedSlot != null && IsDirectional(slot))
                {
                    // Named by direction in the client's tables: the piece of the style for the new one.
                    if (turnedSlot != slot && style.Get(turnedSlot) is var turned and not 0)
                    {
                        id = exact ? turned : CopyOf(styles, turned, tile.Id);
                    }
                }
                else
                {
                    // Turned by its art. Wall-like pieces sit on an edge or a corner of their cell.
                    if (placement is TilePlacement.SouthEdge or TilePlacement.EastEdge)
                    {
                        var e = MapEdge(placement == TilePlacement.SouthEdge, tile.X, tile.Y);
                        (nx, ny) = (e.X, e.Y);
                    }
                    else if (placement == TilePlacement.Vertex)
                    {
                        (nx, ny) = MapVertex(tile.X, tile.Y);
                    }

                    id = TurnByArt(tile.Id, table, kind);
                }

                result.Add((id, nx + shiftX, ny + shiftY, tile.Z, tile.Invisible, tile.Extra));
            }

            foreach (var ((style, z), layer) in walls)
            {
                // Doors count as wall edges, so the wall joins them; their cells keep the door.
                foreach (var door in doorEdges.Where(d => d.Z == z))
                {
                    (door.South ? layer.S : layer.E).Add((door.X, door.Y));
                }

                var placed = new HashSet<(int, int)>();
                if (layer.S.Count + layer.E.Count > 0)
                {
                    int x0 = layer.S.Concat(layer.E).Min(c => c.Item1) - 1;
                    int y0 = layer.S.Concat(layer.E).Min(c => c.Item2) - 1;
                    int x1 = layer.S.Concat(layer.E).Max(c => c.Item1);
                    int y1 = layer.S.Concat(layer.E).Max(c => c.Item2);
                    for (int x = x0; x <= x1; ++x)
                    {
                        for (int y = y0; y <= y1; ++y)
                        {
                            if (doorCells.Contains((x, y, z)))
                            {
                                continue;
                            }

                            string column = WallPlanner.PieceFor(layer.S, layer.E, x, y);
                            if (column == null)
                            {
                                continue;
                            }

                            ushort id = 0;
                            if (column.StartsWith("South", StringComparison.Ordinal))
                            {
                                id = layer.KeepS.TryGetValue((x, y), out ushort keep) ? keep : FirstOf(style, column, "South2", "South1", "South3");
                            }
                            else if (column.StartsWith("East", StringComparison.Ordinal))
                            {
                                id = layer.KeepE.TryGetValue((x, y), out ushort keep) ? keep : FirstOf(style, column, "East2", "East1", "East3");
                            }
                            else
                            {
                                id = style.Get(column);
                            }

                            if (id != 0)
                            {
                                result.Add((id, x, y, z, false, 0));
                                placed.Add((x, y));
                            }
                        }
                    }
                }

                foreach (var post in layer.Posts)
                {
                    // A post no wall asked for (a pillar on its own) stays a post.
                    if (placed.Add((post.X, post.Y)) && !doorCells.Contains((post.X, post.Y, z)))
                    {
                        result.Add((post.Id, post.X, post.Y, z, false, 0));
                    }
                }
            }

            return result;
        }

        private sealed class WallLayer
        {
            public readonly HashSet<(int, int)> S = new HashSet<(int, int)>();
            public readonly HashSet<(int, int)> E = new HashSet<(int, int)>();
            public readonly Dictionary<(int, int), ushort> KeepS = new Dictionary<(int, int), ushort>();
            public readonly Dictionary<(int, int), ushort> KeepE = new Dictionary<(int, int), ushort>();
            public readonly List<(int X, int Y, ushort Id)> Posts = new List<(int, int, ushort)>();
        }

        private static ushort FirstOf(HouseStyle style, params string[] columns)
        {
            foreach (string column in columns)
            {
                ushort id = style.Get(column);
                if (id != 0)
                {
                    return id;
                }
            }

            return 0;
        }

        /// <summary>
        /// The piece that shows a window (or other wall variant) on a wall along the other axis: the
        /// tile with the mirrored art when it is a wall piece of that axis, else the paired column
        /// of the style, else 0 for a plain piece.
        /// </summary>
        private static ushort WallTwin(HouseStyles styles, HouseStyle style, ushort id, string slot)
        {
            bool south = Array.IndexOf(_southColumns, slot) >= 0;
            string[] wanted = south ? _eastColumns : _southColumns;

            ushort twin = ArtFacing.Transpose(id);
            if (twin != id && styles.TryGetRole(twin, out HouseStyle twinStyle, out string twinSlot) &&
                twinStyle.Kind == HousePartKind.Wall && Array.IndexOf(wanted, twinSlot) >= 0)
            {
                return twin;
            }

            int index = Array.IndexOf(south ? _southColumns : _eastColumns, slot);
            return index >= 0 ? style.Get(wanted[index]) : (ushort)0;
        }

        /// <summary>
        /// A tile turned without a named role. A quarter turn swaps the axes, so the art's mirror
        /// twin is the turned piece for everything that looks the same from both ends (walls,
        /// fences, tables). A piece with four facings needs the facing table for the other half of
        /// its turns; the table is believed unless the art says otherwise.
        /// </summary>
        private static ushort TurnByArt(ushort id, RotationTable table, GroupTransformKind kind)
        {
            ushort twin = ArtFacing.Transpose(id);
            ushort left = TableAgreesWithArt(id, table) ? table.TurnLeft(id) : id;
            ushort right = left != id ? table.TurnRight(id) : id;

            switch (kind)
            {
                case GroupTransformKind.RotateLeft:
                    return left != id ? left : twin;
                case GroupTransformKind.RotateRight:
                    return right != id ? right : twin;
                case GroupTransformKind.MirrorX:
                    // Flips what faces east or west: of those, the twin is the piece a right turn gives.
                    return left != id && twin == right && twin != id ? table.TurnLeft(left) : id;
                default:
                    return left != id && twin == left && twin != id ? table.TurnLeft(left) : id;
            }
        }

        /// <summary>
        /// Whether the table's turns for a tile fit its art. Turning left goes south, east, north,
        /// west; south/east and north/west are mirror twins. So a cycle of two must be twins, and
        /// in a cycle of four every twin the art finds must be the next or the previous tile.
        /// </summary>
        private static bool TableAgreesWithArt(ushort id, RotationTable table)
        {
            var cycle = new List<ushort> { id };
            for (ushort next = table.TurnLeft(id); next != id; next = table.TurnLeft(next))
            {
                if (cycle.Count == 4 || cycle.Contains(next) || table.TurnLeft(next) == next)
                {
                    return false;
                }

                cycle.Add(next);
            }

            if (cycle.Count == 1 || cycle.Count == 3)
            {
                return false;
            }

            for (int i = 0; i < cycle.Count; i++)
            {
                ushort twin = ArtFacing.Transpose(cycle[i]);
                if (twin == cycle[i])
                {
                    continue;
                }

                ushort next = cycle[(i + 1) % cycle.Count];
                ushort previous = cycle[(i + cycle.Count - 1) % cycle.Count];
                if (twin != next && twin != previous)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// For a tile that copies a listed piece: the copy of <paramref name="listed"/> closest to
        /// <paramref name="original"/>, so a classic roof stays in its classic ids.
        /// </summary>
        private static ushort CopyOf(HouseStyles styles, ushort listed, ushort original)
        {
            for (int distance = 0; distance <= 32; distance++)
            {
                foreach (int candidate in new[] { original - distance, original + distance })
                {
                    if (candidate >= 0 && candidate <= ushort.MaxValue &&
                        styles.TryGetListedTwin((ushort)candidate, out ushort twin) && twin == listed)
                    {
                        return (ushort)candidate;
                    }
                }
            }

            return listed;
        }

        private static bool IsDirectional(string slot)
        {
            return _directions.ContainsKey(slot) || slot.EndsWith("Dent", StringComparison.Ordinal) ||
                   slot.EndsWith("TPiece", StringComparison.Ordinal) || slot is "NSCrosspiece" or "EWCrosspiece";
        }

        /// <summary>
        /// The column a direction-named piece moves to: North/East/South/West and their dents and
        /// T pieces turn; crosspieces swap axes when the group turns a quarter.
        /// </summary>
        private static string TurnSlot(string slot, Func<string, string> mapDirection, bool swapsAxes)
        {
            foreach (string direction in _directions.Keys)
            {
                if (slot == direction)
                {
                    return mapDirection(direction);
                }
            }

            foreach (string suffix in new[] { "Dent", "TPiece" })
            {
                // NDent, ETPiece ...: the first letter is the direction.
                if (slot.Length == 1 + suffix.Length && slot.EndsWith(suffix, StringComparison.Ordinal))
                {
                    string direction = _directions.Keys.First(d => d[0] == slot[0]);
                    return mapDirection(direction)[0] + suffix;
                }
            }

            if (swapsAxes)
            {
                if (slot == "NSCrosspiece")
                {
                    return "EWCrosspiece";
                }

                if (slot == "EWCrosspiece")
                {
                    return "NSCrosspiece";
                }
            }

            return slot;
        }
    }
}
