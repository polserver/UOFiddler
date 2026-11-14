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

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// Builds walls from cell edges. A wall piece sits on the +Y edge of its cell (a "South" piece,
    /// running along X), the +X edge (an "East" piece, running along Y), or both (Corner). A cell
    /// with no wall of its own gets a Post where walls meet at its +X/+Y corner. This matches the
    /// client's walls.txt columns and the official house multis.
    /// </summary>
    internal sealed class WallPlanner
    {
        private static readonly string[] _southColumns = { "South1", "South2", "South3", "WindowS", "AltWindowS", "SecondAltWindowS" };
        private static readonly string[] _eastColumns = { "East1", "East2", "East3", "WindowE", "AltWindowE", "SecondAltWindowE" };

        private readonly HouseStyle _style;
        private readonly HouseStyles _styles;
        private readonly int _z;

        // Edges of the wall layer: cells with a wall on their +Y edge (S) and +X edge (E).
        private readonly HashSet<(int X, int Y)> _south = new HashSet<(int, int)>();
        private readonly HashSet<(int X, int Y)> _east = new HashSet<(int, int)>();

        // Existing pieces of this style at this Z, and door cells (doors count as wall edges).
        private readonly Dictionary<(int X, int Y), MultiTile> _pieces = new Dictionary<(int, int), MultiTile>();
        private readonly HashSet<(int X, int Y)> _doors = new HashSet<(int, int)>();

        public WallPlanner(MultiEditorComponentList list, HouseStyles styles, HouseStyle style, int z)
        {
            _style = style;
            _styles = styles;
            _z = z;

            foreach (MultiTile tile in list.RealTiles)
            {
                if (tile.Z != z || !styles.TryGetRole(tile.Id, out HouseStyle owner, out string slot))
                {
                    continue;
                }

                var cell = (tile.X, tile.Y);
                if (owner.Kind == HousePartKind.Door)
                {
                    // Pieces 1-4 hang in walls along X, 5-8 in walls along Y.
                    _doors.Add(cell);
                    (DoorAlongX(slot) ? _south : _east).Add(cell);
                    continue;
                }

                if (owner != style)
                {
                    continue;
                }

                _pieces[cell] = tile;
                if (slot == "Corner" || Array.IndexOf(_southColumns, slot) >= 0)
                {
                    _south.Add(cell);
                }

                if (slot == "Corner" || Array.IndexOf(_eastColumns, slot) >= 0)
                {
                    _east.Add(cell);
                }
            }
        }

        public static bool DoorAlongX(string slot) => slot is "Piece1" or "Piece2" or "Piece3" or "Piece4";

        /// <summary>
        /// The wall edges a drag between two cells adds. Two cells in a row or column (or with
        /// <paramref name="line"/>) give a straight wall along the cells; otherwise the dragged
        /// rectangle is the room's outer wall cells, as in the official houses.
        /// </summary>
        public static (List<(int X, int Y)> South, List<(int X, int Y)> East) DragEdges(int x0, int y0, int x1, int y1, bool line)
        {
            var south = new List<(int, int)>();
            var east = new List<(int, int)>();
            int minX = Math.Min(x0, x1), maxX = Math.Max(x0, x1);
            int minY = Math.Min(y0, y1), maxY = Math.Max(y0, y1);

            if (line || minX == maxX || minY == maxY)
            {
                if (maxX - minX >= maxY - minY)
                {
                    for (int x = minX; x <= maxX; ++x)
                    {
                        south.Add((x, y0));
                    }
                }
                else
                {
                    for (int y = minY; y <= maxY; ++y)
                    {
                        east.Add((x0, y));
                    }
                }

                return (south, east);
            }

            for (int x = minX + 1; x <= maxX; ++x)
            {
                south.Add((x, minY));
                south.Add((x, maxY));
            }

            for (int y = minY + 1; y <= maxY; ++y)
            {
                east.Add((minX, y));
                east.Add((maxX, y));
            }

            return (south, east);
        }

        /// <summary>
        /// Adds (or with <paramref name="erase"/> removes) edges and works out the tiles to change
        /// in the cells around them. Doors are never touched; windows stay while their edge does.
        /// </summary>
        public (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            IEnumerable<(int X, int Y)> south, IEnumerable<(int X, int Y)> east, bool erase, int width, int height)
        {
            var touched = new HashSet<(int X, int Y)>();
            void Edit(HashSet<(int, int)> set, (int X, int Y) cell)
            {
                if (cell.X < 0 || cell.Y < 0 || cell.X >= width || cell.Y >= height || _doors.Contains(cell))
                {
                    return;
                }

                if (erase ? set.Remove(cell) : set.Add(cell))
                {
                    // A piece's choice depends on its neighbours along both axes.
                    for (int dx = -1; dx <= 1; ++dx)
                    {
                        for (int dy = -1; dy <= 1; ++dy)
                        {
                            touched.Add((cell.X + dx, cell.Y + dy));
                        }
                    }
                }
            }

            foreach (var cell in south)
            {
                Edit(_south, cell);
            }

            foreach (var cell in east)
            {
                Edit(_east, cell);
            }

            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            foreach (var cell in touched)
            {
                if (cell.X < 0 || cell.Y < 0 || cell.X >= width || cell.Y >= height || _doors.Contains(cell))
                {
                    continue;
                }

                string column = PieceFor(cell.X, cell.Y);
                ushort want = column != null ? _style.Get(column) : (ushort)0;
                _pieces.TryGetValue(cell, out MultiTile have);

                if (have != null && KeepExisting(have, column))
                {
                    continue;
                }

                if (have != null && have.Id == want)
                {
                    continue;
                }

                if (have != null)
                {
                    remove.Add(have);
                }

                if (want != 0)
                {
                    add.Add((want, cell.X, cell.Y, _z));
                }
            }

            return (remove, add);
        }

        /// <summary>
        /// The walls.txt column for a cell: Corner, a South or East variant by its place in the run,
        /// Post where walls meet at the cell's corner, or null for no piece.
        /// </summary>
        public string PieceFor(int x, int y) => PieceFor(_south, _east, x, y);

        /// <summary>
        /// The walls.txt column for a cell given the +Y edges (<paramref name="southEdges"/>) and
        /// +X edges (<paramref name="eastEdges"/>) of a wall layer.
        /// </summary>
        public static string PieceFor(HashSet<(int X, int Y)> southEdges, HashSet<(int X, int Y)> eastEdges, int x, int y)
        {
            bool s = southEdges.Contains((x, y));
            bool e = eastEdges.Contains((x, y));

            if (s && e)
            {
                return "Corner";
            }

            if (s)
            {
                bool start = !southEdges.Contains((x - 1, y));
                bool end = !southEdges.Contains((x + 1, y));
                return start ? "South1" : end ? "South3" : "South2";
            }

            if (e)
            {
                bool start = !eastEdges.Contains((x, y - 1));
                bool end = !eastEdges.Contains((x, y + 1));
                return end ? "East1" : start ? "East3" : "East2";
            }

            return southEdges.Contains((x + 1, y)) || eastEdges.Contains((x, y + 1)) ? "Post" : null;
        }

        /// <summary>
        /// A window or other variant of the right direction stays, so redrawing a wall keeps its windows.
        /// </summary>
        private bool KeepExisting(MultiTile have, string column)
        {
            if (column == null || !_styles.TryGetRole(have.Id, out _, out string slot))
            {
                return false;
            }

            bool haveSouth = Array.IndexOf(_southColumns, slot) >= 0;
            bool haveEast = Array.IndexOf(_eastColumns, slot) >= 0;
            bool wantSouth = Array.IndexOf(_southColumns, column) >= 0;
            bool wantEast = Array.IndexOf(_eastColumns, column) >= 0;
            bool isPlain = slot is "South1" or "South2" or "South3" or "East1" or "East2" or "East3";
            return !isPlain && ((haveSouth && wantSouth) || (haveEast && wantEast));
        }
    }
}
