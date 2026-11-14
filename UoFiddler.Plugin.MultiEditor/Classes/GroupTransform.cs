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
    /// (a stair, roof slope, door) is swapped for the piece of its style that faces the new way.
    /// Walls sit on cell edges, not in cells: they are turned into edges, the edges transformed,
    /// and the wall pieces chosen again, so corners and posts come out right.
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

        private static readonly string[] _windowPairs = { "WindowS", "WindowE", "AltWindowS", "AltWindowE", "SecondAltWindowS", "SecondAltWindowE" };

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
            var walls = new Dictionary<(HouseStyle Style, int Z), (HashSet<(int, int)> S, HashSet<(int, int)> E, Dictionary<(int, int), string> Windows)>();
            var doorCells = new HashSet<(int, int, int)>();
            var doorEdges = new List<(int Z, bool South, int X, int Y)>();

            foreach (MultiTile tile in group)
            {
                styles.TryGetRole(tile.Id, out HouseStyle style, out string slot);

                if (style?.Kind == HousePartKind.Wall && slot != "Post")
                {
                    if (!walls.TryGetValue((style, tile.Z), out var layer))
                    {
                        walls[(style, tile.Z)] = layer = (new HashSet<(int, int)>(), new HashSet<(int, int)>(), new Dictionary<(int, int), string>());
                    }

                    bool isSouth = slot == "Corner" || slot.StartsWith("South", StringComparison.Ordinal) || slot.EndsWith('S');
                    bool isEast = slot == "Corner" || slot.StartsWith("East", StringComparison.Ordinal) || slot.EndsWith('E');
                    foreach (bool south in new[] { true, false })
                    {
                        if (south ? !isSouth : !isEast)
                        {
                            continue;
                        }

                        var e = MapEdge(south, tile.X, tile.Y);
                        var cell = (e.X + shiftX, e.Y + shiftY);
                        (e.South ? layer.S : layer.E).Add(cell);

                        int window = Array.IndexOf(_windowPairs, slot);
                        if (window >= 0)
                        {
                            // A window keeps its kind on the wall's new axis.
                            layer.Windows[cell] = _windowPairs[(window & ~1) + (e.South ? 0 : 1)];
                        }
                    }

                    continue;
                }

                if (style?.Kind == HousePartKind.Wall)
                {
                    // Posts are rebuilt from the edges.
                    continue;
                }

                var (nx, ny) = MapCell(tile.X, tile.Y);
                ushort id = tile.Id;

                if (style?.Kind == HousePartKind.Door && _doors.TryGetValue(slot, out var door))
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
                }
                else if (style != null && slot != null)
                {
                    string newSlot = TurnSlot(slot, MapDirection, SwapsAxes(kind));
                    if (newSlot != slot && style.Get(newSlot) is var turned and not 0)
                    {
                        id = turned;
                    }
                }
                else if (kind == GroupTransformKind.RotateLeft)
                {
                    // Not in the client tables: the facing table, which only knows quarter turns.
                    id = table.TurnLeft(id);
                }
                else if (kind == GroupTransformKind.RotateRight)
                {
                    id = table.TurnRight(id);
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
                        if (column is "South1" or "South2" or "South3" or "East1" or "East2" or "East3" &&
                            layer.Windows.TryGetValue((x, y), out string window))
                        {
                            column = window;
                        }

                        ushort id = column != null ? style.Get(column) : (ushort)0;
                        if (id != 0)
                        {
                            result.Add((id, x, y, z, false, 0));
                        }
                    }
                }
            }

            return result;
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
