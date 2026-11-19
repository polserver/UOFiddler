/***************************************************************************
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
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// A floor with a border: four corner pieces, a run of pieces for each side and a block of
    /// fill tiles for the inside. Laid over a rectangle of any size, the sides and the fill repeat.
    /// Sides are named by the world: north is the low Y row, west the low X column.
    /// </summary>
    internal sealed class FloorPattern
    {
        public string Name { get; set; }

        /// <summary>Shipped with the editor, not saved to or deleted from the user's file.</summary>
        public bool BuiltIn { get; set; }

        public ushort NorthWest { get; set; }

        public ushort NorthEast { get; set; }

        public ushort SouthWest { get; set; }

        public ushort SouthEast { get; set; }

        /// <summary>Pieces along each side between the corners, in order of rising X (north, south) or Y (west, east).</summary>
        public List<ushort> North { get; set; } = new List<ushort>();

        public List<ushort> South { get; set; } = new List<ushort>();

        public List<ushort> West { get; set; } = new List<ushort>();

        public List<ushort> East { get; set; } = new List<ushort>();

        /// <summary>The inside as a block of tiles, row by row (Y major), <see cref="FillWidth"/> wide.</summary>
        public List<ushort> Fill { get; set; } = new List<ushort>();

        public int FillWidth { get; set; } = 1;

        public int FillHeight => FillWidth > 0 ? Math.Max(1, Fill.Count / FillWidth) : 1;

        /// <summary>A tile to show for the pattern in lists.</summary>
        public ushort IconId => NorthWest != 0 ? NorthWest : Fill.FirstOrDefault();

        /// <summary>Every distinct tile of the pattern, corners first.</summary>
        public IEnumerable<ushort> Pieces()
        {
            var seen = new HashSet<ushort>();
            foreach (ushort id in new[] { NorthWest, NorthEast, SouthWest, SouthEast }.Concat(North).Concat(East).Concat(South).Concat(West).Concat(Fill))
            {
                if (id != 0 && seen.Add(id))
                {
                    yield return id;
                }
            }
        }

        /// <summary>
        /// The tile for each cell of a rectangle. A rectangle one cell wide or high has no room for
        /// a border and gets fill only.
        /// </summary>
        public IEnumerable<(ushort Id, int X, int Y)> Layout(int x0, int y0, int x1, int y1)
        {
            bool border = x1 > x0 && y1 > y0;
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    ushort id = 0;
                    if (border)
                    {
                        bool west = x == x0, east = x == x1, north = y == y0, south = y == y1;
                        id = north && west ? NorthWest
                            : north && east ? NorthEast
                            : south && west ? SouthWest
                            : south && east ? SouthEast
                            : north ? At(North, x - x0 - 1)
                            : south ? At(South, x - x0 - 1)
                            : west ? At(West, y - y0 - 1)
                            : east ? At(East, y - y0 - 1)
                            : (ushort)0;
                    }

                    if (id == 0 && Fill.Count > 0)
                    {
                        // The fill block starts at the first inner cell, so it sits the same way at any size.
                        int fx = Mod(x - x0 - (border ? 1 : 0), FillWidth);
                        int fy = Mod(y - y0 - (border ? 1 : 0), FillHeight);
                        id = Fill[Math.Min(Fill.Count - 1, (fy * FillWidth) + fx)];
                    }

                    if (id != 0)
                    {
                        yield return (id, x, y);
                    }
                }
            }
        }

        private static ushort At(List<ushort> run, int index) => run.Count == 0 ? (ushort)0 : run[Mod(index, run.Count)];

        private static int Mod(int value, int by) => by <= 0 ? 0 : ((value % by) + by) % by;

        /// <summary>
        /// Learns a pattern from a floor laid out by hand: the selected tiles must fill a rectangle
        /// of at least 3 by 3 cells, one tile per cell (the highest selected one counts). Returns
        /// null with a reason when they do not.
        /// </summary>
        public static FloorPattern Learn(IReadOnlyCollection<MultiTile> selection, string name, out string problem)
        {
            problem = null;
            if (selection.Count == 0)
            {
                problem = "Select the floor tiles of one bordered rectangle first.";
                return null;
            }

            int x0 = selection.Min(t => t.X), x1 = selection.Max(t => t.X);
            int y0 = selection.Min(t => t.Y), y1 = selection.Max(t => t.Y);
            if (x1 - x0 < 2 || y1 - y0 < 2)
            {
                problem = "The selection must be at least 3 by 3 cells: corners, sides and something inside.";
                return null;
            }

            var top = new Dictionary<(int, int), MultiTile>();
            foreach (MultiTile tile in selection)
            {
                if (!top.TryGetValue((tile.X, tile.Y), out MultiTile have) || tile.Z > have.Z)
                {
                    top[(tile.X, tile.Y)] = tile;
                }
            }

            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    if (!top.ContainsKey((x, y)))
                    {
                        problem = $"The selection is not a full rectangle: cell {x}, {y} has no selected tile.";
                        return null;
                    }
                }
            }

            ushort Id(int x, int y) => top[(x, y)].Id;
            var pattern = new FloorPattern
            {
                Name = name,
                NorthWest = Id(x0, y0),
                NorthEast = Id(x1, y0),
                SouthWest = Id(x0, y1),
                SouthEast = Id(x1, y1),
                FillWidth = x1 - x0 - 1
            };

            for (int x = x0 + 1; x < x1; x++)
            {
                pattern.North.Add(Id(x, y0));
                pattern.South.Add(Id(x, y1));
            }

            for (int y = y0 + 1; y < y1; y++)
            {
                pattern.West.Add(Id(x0, y));
                pattern.East.Add(Id(x1, y));
            }

            for (int y = y0 + 1; y < y1; y++)
            {
                for (int x = x0 + 1; x < x1; x++)
                {
                    pattern.Fill.Add(Id(x, y));
                }
            }

            pattern.Shrink();
            return pattern;
        }

        /// <summary>Drops repeats: a side that is one piece over and over is stored as that piece, likewise the fill block.</summary>
        private void Shrink()
        {
            North = Period(North);
            South = Period(South);
            West = Period(West);
            East = Period(East);

            int width = FillWidth, height = FillHeight;
            int pw = width, ph = height;
            for (int w = 1; w <= width; w++)
            {
                if (Enumerable.Range(0, Fill.Count).All(i => Fill[i] == Fill[(i / width * width) + (i % width % w)]))
                {
                    pw = w;
                    break;
                }
            }

            for (int h = 1; h <= height; h++)
            {
                if (Enumerable.Range(0, Fill.Count).All(i => Fill[i] == Fill[(i / width % h * width) + (i % width)]))
                {
                    ph = h;
                    break;
                }
            }

            var block = new List<ushort>();
            for (int y = 0; y < ph; y++)
            {
                for (int x = 0; x < pw; x++)
                {
                    block.Add(Fill[(y * width) + x]);
                }
            }

            Fill = block;
            FillWidth = pw;
        }

        private static List<ushort> Period(List<ushort> run)
        {
            for (int length = 1; length < run.Count; length++)
            {
                if (Enumerable.Range(0, run.Count).All(i => run[i] == run[i % length]))
                {
                    return run.GetRange(0, length);
                }
            }

            return run;
        }
    }
}
