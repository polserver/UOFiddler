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
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// A house being designed, beside its wings: the materials of each storey and what was changed
    /// by hand (partitions, openings, doors, windows, floor areas, stairs). <see cref="Build"/>
    /// turns wings and draft into tiles. A wing's box is its wall cells, as the wall tool drags
    /// them: walls stand on the +Y / +X edges of their cells, so the rooms inside are the box
    /// without its first row and column.
    /// </summary>
    internal sealed class HouseDraft
    {
        public const int StoreyHeight = 20;
        public const int MaxStoreys = 4;
        public const string WallsLayer = "Walls";
        public const string FloorsLayer = "Floors";
        public const string StairsLayer = "Stairs";
        public const string FoundationLayer = "Foundation";

        // The foundation ring stands this far below the ground floor, as in the client's house foundations.
        private const int _foundationBelow = 7;

        /// <summary>Z of the ground floor.</summary>
        public int BaseZ { get; set; }

        public HouseStyle[] WallStyles { get; } = new HouseStyle[MaxStoreys];

        public HouseStyle[] FloorStyles { get; } = new HouseStyle[MaxStoreys];

        public HouseStyle RoofStyle { get; set; }

        /// <summary>The floor style a flat roof is laid in; without one, the floor style of the wing's top storey.</summary>
        public HouseStyle FlatRoofStyle { get; set; }

        public bool Foundation { get; set; }

        /// <summary>Most rooms a wing can be divided into.</summary>
        public const int MaxRooms = 6;

        // Wall edges added and taken away by hand, per storey: cells with a wall on their +Y edge
        // (south) and on their +X edge (east).
        public HashSet<(int X, int Y)>[] AddedSouth { get; } = Sets();

        public HashSet<(int X, int Y)>[] AddedEast { get; } = Sets();

        public HashSet<(int X, int Y)>[] ErasedSouth { get; } = Sets();

        public HashSet<(int X, int Y)>[] ErasedEast { get; } = Sets();

        /// <summary>Doors by storey and wall cell: the door style and its piece (hinge variant).</summary>
        public Dictionary<(int Storey, int X, int Y), (HouseStyle Style, string Piece)> Doors { get; } = new Dictionary<(int, int, int), (HouseStyle, string)>();

        /// <summary>Wall cells whose piece is the style's window.</summary>
        public HashSet<(int Storey, int X, int Y)> Windows { get; } = new HashSet<(int, int, int)>();

        /// <summary>Floor areas laid in another style than the storey's; later ones lie on top.</summary>
        public List<(int Storey, int X0, int Y0, int X1, int Y1, HouseStyle Style)> FloorAreas { get; } = new List<(int, int, int, int, int, HouseStyle)>();

        /// <summary>Stairs by storey: the cell of the bottom step, the stair column (MultiNorth ...) and the style.</summary>
        public List<(int Storey, int X, int Y, string Column, HouseStyle Style)> Stairs { get; } = new List<(int, int, int, string, HouseStyle)>();

        private static HashSet<(int X, int Y)>[] Sets()
        {
            var sets = new HashSet<(int X, int Y)>[MaxStoreys];
            for (int i = 0; i < MaxStoreys; ++i)
            {
                sets[i] = new HashSet<(int X, int Y)>();
            }

            return sets;
        }

        /// <summary>A copy that shares nothing but the styles.</summary>
        public HouseDraft Clone()
        {
            HouseDraft copy = Fresh();
            copy.BaseZ = BaseZ;
            for (int k = 0; k < MaxStoreys; ++k)
            {
                copy.AddedSouth[k].UnionWith(AddedSouth[k]);
                copy.AddedEast[k].UnionWith(AddedEast[k]);
                copy.ErasedSouth[k].UnionWith(ErasedSouth[k]);
                copy.ErasedEast[k].UnionWith(ErasedEast[k]);
            }

            foreach (var door in Doors)
            {
                copy.Doors[door.Key] = door.Value;
            }

            copy.Windows.UnionWith(Windows);
            copy.FloorAreas.AddRange(FloorAreas);
            copy.Stairs.AddRange(Stairs);
            return copy;
        }

        /// <summary>A draft with the same materials and nothing changed by hand: the next house.</summary>
        public HouseDraft Fresh()
        {
            var copy = new HouseDraft { RoofStyle = RoofStyle, FlatRoofStyle = FlatRoofStyle, Foundation = Foundation };
            Array.Copy(WallStyles, copy.WallStyles, MaxStoreys);
            Array.Copy(FloorStyles, copy.FloorStyles, MaxStoreys);
            return copy;
        }

        /// <summary>Storeys of the tallest wing.</summary>
        public static int Top(IReadOnlyList<RoofWing> wings)
        {
            int top = 0;
            foreach (RoofWing wing in wings)
            {
                top = Math.Max(top, Math.Clamp(wing.Storeys, 1, MaxStoreys));
            }

            return top;
        }

        /// <summary>The cells inside the house on a storey (0 is the ground floor): the rooms of every wing that high.</summary>
        public static HashSet<(int X, int Y)> Region(IReadOnlyList<RoofWing> wings, int storey)
        {
            var region = new HashSet<(int X, int Y)>();
            foreach (RoofWing wing in wings)
            {
                if (wing.Storeys <= storey)
                {
                    continue;
                }

                for (int x = wing.X0 + 1; x <= wing.X1; ++x)
                {
                    for (int y = wing.Y0 + 1; y <= wing.Y1; ++y)
                    {
                        region.Add((x, y));
                    }
                }
            }

            return region;
        }

        /// <summary>The wall edges around a region: its outline.</summary>
        public static void Outline(HashSet<(int X, int Y)> region, HashSet<(int X, int Y)> south, HashSet<(int X, int Y)> east)
        {
            foreach ((int x, int y) in region)
            {
                if (!region.Contains((x, y + 1)))
                {
                    south.Add((x, y));
                }

                if (!region.Contains((x, y - 1)))
                {
                    south.Add((x, y - 1));
                }

                if (!region.Contains((x + 1, y)))
                {
                    east.Add((x, y));
                }

                if (!region.Contains((x - 1, y)))
                {
                    east.Add((x - 1, y));
                }
            }
        }

        /// <summary>
        /// The wall edges of a storey: the outline of its rooms, with the edges added by hand that
        /// still touch a room and without the ones taken away.
        /// </summary>
        public void Edges(IReadOnlyList<RoofWing> wings, int storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east)
        {
            HashSet<(int X, int Y)> region = Region(wings, storey);
            south = new HashSet<(int X, int Y)>();
            east = new HashSet<(int X, int Y)>();
            Outline(region, south, east);
            Partitions(wings, storey, south, east);
            foreach ((int x, int y) in AddedSouth[storey])
            {
                if (region.Contains((x, y)) || region.Contains((x, y + 1)))
                {
                    south.Add((x, y));
                }
            }

            foreach ((int x, int y) in AddedEast[storey])
            {
                if (region.Contains((x, y)) || region.Contains((x + 1, y)))
                {
                    east.Add((x, y));
                }
            }

            south.ExceptWith(ErasedSouth[storey]);
            east.ExceptWith(ErasedEast[storey]);
        }

        /// <summary>
        /// The walls that divide each wing of a storey into its rooms (<see cref="RoofWing.Rooms"/>):
        /// across the wing's longer side, evenly spaced, with the middle cell of each left open as
        /// a doorway. A wing too short for that many rooms gets as many as fit two cells each.
        /// </summary>
        private static void Partitions(IReadOnlyList<RoofWing> wings, int storey, HashSet<(int X, int Y)> south, HashSet<(int X, int Y)> east)
        {
            foreach (RoofWing wing in wings)
            {
                int width = wing.X1 - wing.X0, depth = wing.Y1 - wing.Y0;
                bool alongX = width >= depth;
                int length = alongX ? width : depth;
                int rooms = Math.Min(Math.Clamp(wing.Rooms, 1, MaxRooms), length / 2);
                if (wing.Storeys <= storey || rooms < 2)
                {
                    continue;
                }

                for (int i = 1; i < rooms; ++i)
                {
                    int at = (int)Math.Round(i * length / (double)rooms);
                    if (alongX)
                    {
                        int doorway = wing.Y0 + 1 + ((depth - 1) / 2);
                        for (int y = wing.Y0 + 1; y <= wing.Y1; ++y)
                        {
                            if (y != doorway)
                            {
                                east.Add((wing.X0 + at, y));
                            }
                        }
                    }
                    else
                    {
                        int doorway = wing.X0 + 1 + ((width - 1) / 2);
                        for (int x = wing.X0 + 1; x <= wing.X1; ++x)
                        {
                            if (x != doorway)
                            {
                                south.Add((x, wing.Y0 + at));
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Moves everything changed by hand along with the house.</summary>
        public void Shift(int dx, int dy)
        {
            static void Move(HashSet<(int X, int Y)> set, int dx, int dy)
            {
                var moved = new List<(int X, int Y)>(set.Count);
                foreach ((int x, int y) in set)
                {
                    moved.Add((x + dx, y + dy));
                }

                set.Clear();
                set.UnionWith(moved);
            }

            for (int k = 0; k < MaxStoreys; ++k)
            {
                Move(AddedSouth[k], dx, dy);
                Move(AddedEast[k], dx, dy);
                Move(ErasedSouth[k], dx, dy);
                Move(ErasedEast[k], dx, dy);
            }

            var doors = new List<KeyValuePair<(int Storey, int X, int Y), (HouseStyle Style, string Piece)>>(Doors);
            Doors.Clear();
            foreach (var door in doors)
            {
                Doors[(door.Key.Storey, door.Key.X + dx, door.Key.Y + dy)] = door.Value;
            }

            var windows = new List<(int Storey, int X, int Y)>(Windows);
            Windows.Clear();
            foreach ((int storey, int x, int y) in windows)
            {
                Windows.Add((storey, x + dx, y + dy));
            }

            for (int i = 0; i < FloorAreas.Count; ++i)
            {
                var a = FloorAreas[i];
                FloorAreas[i] = (a.Storey, a.X0 + dx, a.Y0 + dy, a.X1 + dx, a.Y1 + dy, a.Style);
            }

            for (int i = 0; i < Stairs.Count; ++i)
            {
                var s = Stairs[i];
                Stairs[i] = (s.Storey, s.X + dx, s.Y + dy, s.Column, s.Style);
            }
        }

        /// <summary>The tiles of a built draft, each with its layer, and the walls among them.</summary>
        public sealed class Built
        {
            public List<(ushort Id, int X, int Y, int Z, string Layer)> Tiles { get; } = new List<(ushort, int, int, int, string)>();

            public List<(ushort Id, int X, int Y, int Z)> Walls { get; } = new List<(ushort, int, int, int)>();
        }

        /// <summary>
        /// Foundation, walls with their doors and windows, floors and stairs of the house, storey
        /// by storey. The roof is not part of it: the roof tool plans it over these walls.
        /// </summary>
        public Built Build(IReadOnlyList<RoofWing> wings, HouseStyles styles)
        {
            var built = new Built();
            int top = Top(wings);
            var cut = new HashSet<(int X, int Y)>[MaxStoreys + 1];

            // Stairs first: a run opens the floor of the storey it climbs to.
            foreach ((int storey, int x, int y, string column, HouseStyle style) in Stairs)
            {
                if (storey >= top || !Region(wings, storey).Contains((x, y)))
                {
                    continue;
                }

                foreach ((ushort id, int tx, int ty, int tz) in StairTool.Tiles(style, column, x, y, BaseZ + (storey * StoreyHeight)))
                {
                    built.Tiles.Add((id, tx, ty, tz, StairsLayer));
                    (cut[storey + 1] ??= new HashSet<(int X, int Y)>()).Add((tx, ty));
                }
            }

            for (int storey = 0; storey < top; ++storey)
            {
                int z = BaseZ + (storey * StoreyHeight);
                HashSet<(int X, int Y)> region = Region(wings, storey);
                Edges(wings, storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east);

                if (storey == 0 && Foundation && styles.TryGetRole(FoundationTool.FoundationPost, out HouseStyle foundation, out _))
                {
                    var ringSouth = new HashSet<(int X, int Y)>();
                    var ringEast = new HashSet<(int X, int Y)>();
                    Outline(region, ringSouth, ringEast);
                    foreach ((int x, int y) in WallCells(ringSouth, ringEast))
                    {
                        string column = WallPlanner.PieceFor(ringSouth, ringEast, x, y);
                        ushort id = column != null ? foundation.Get(column) : (ushort)0;
                        if (id != 0)
                        {
                            built.Tiles.Add((id, x, y, z - _foundationBelow, FoundationLayer));
                        }
                    }
                }

                HouseStyle wallStyle = WallStyles[storey];
                foreach ((int x, int y) in WallCells(south, east))
                {
                    if (Doors.TryGetValue((storey, x, y), out (HouseStyle Style, string Piece) door) &&
                        (WallPlanner.DoorAlongX(door.Piece) ? south : east).Contains((x, y)) && door.Style.Get(door.Piece) is var doorId and not 0)
                    {
                        built.Tiles.Add((doorId, x, y, z, WallsLayer));
                        built.Walls.Add((doorId, x, y, z));
                        continue;
                    }

                    string column = WallPlanner.PieceFor(wallStyle, south, east, x, y);
                    if (column == null || wallStyle == null)
                    {
                        continue;
                    }

                    if (Windows.Contains((storey, x, y)) && column != "Corner" && column != "Post")
                    {
                        string window = column.StartsWith("South", StringComparison.Ordinal) ? "WindowS" : "WindowE";
                        if (wallStyle.Get(window) != 0)
                        {
                            column = window;
                        }
                    }

                    ushort id = wallStyle.Get(column);
                    if (id != 0)
                    {
                        built.Tiles.Add((id, x, y, z, WallsLayer));
                        built.Walls.Add((id, x, y, z));
                    }
                }

                foreach ((int x, int y) in region)
                {
                    if (cut[storey] != null && cut[storey].Contains((x, y)))
                    {
                        continue;
                    }

                    HouseStyle floorStyle = FloorStyles[storey];
                    foreach ((int areaStorey, int x0, int y0, int x1, int y1, HouseStyle areaStyle) in FloorAreas)
                    {
                        if (areaStorey == storey && x >= x0 && x <= x1 && y >= y0 && y <= y1)
                        {
                            floorStyle = areaStyle;
                        }
                    }

                    if (floorStyle == null)
                    {
                        continue;
                    }

                    ushort[] pieces = Pieces(floorStyle);
                    if (pieces.Length > 0)
                    {
                        // The same cell always gets the same piece, so the preview does not flicker.
                        built.Tiles.Add((pieces[(int)((uint)HashCode.Combine(x, y, storey) % (uint)pieces.Length)], x, y, z, FloorsLayer));
                    }
                }
            }

            // Flat roofs: the floor of the wing's top storey once more, on top of its walls. Where a
            // taller wing stands, its own floor is there already.
            foreach (RoofWing wing in wings)
            {
                int storeys = Math.Clamp(wing.Storeys, 1, MaxStoreys);
                HouseStyle style = FlatRoofStyle ?? FloorStyles[storeys - 1];
                if (wing.NoRoof || wing.Shape != RoofShape.Flat || style == null)
                {
                    continue;
                }

                HashSet<(int X, int Y)> above = Region(wings, storeys);
                ushort[] pieces = Pieces(style);
                for (int x = wing.X0 + 1; pieces.Length > 0 && x <= wing.X1; ++x)
                {
                    for (int y = wing.Y0 + 1; y <= wing.Y1; ++y)
                    {
                        if (!above.Contains((x, y)))
                        {
                            built.Tiles.Add((pieces[(int)((uint)HashCode.Combine(x, y, storeys) % (uint)pieces.Length)], x, y,
                                BaseZ + (storeys * StoreyHeight), RoofTool.LayerName));
                        }
                    }
                }
            }

            return built;
        }

        private readonly Dictionary<HouseStyle, ushort[]> _pieces = new Dictionary<HouseStyle, ushort[]>();

        private ushort[] Pieces(HouseStyle style)
        {
            if (!_pieces.TryGetValue(style, out ushort[] pieces))
            {
                _pieces[style] = pieces = style.Slots.Values.Distinct().ToArray();
            }

            return pieces;
        }

        /// <summary>Every cell that may hold a wall piece: the cells with an edge, and the ones a post goes in.</summary>
        private static HashSet<(int X, int Y)> WallCells(HashSet<(int X, int Y)> south, HashSet<(int X, int Y)> east)
        {
            var cells = new HashSet<(int X, int Y)>(south);
            cells.UnionWith(east);
            foreach ((int x, int y) in south)
            {
                cells.Add((x - 1, y));
            }

            foreach ((int x, int y) in east)
            {
                cells.Add((x, y - 1));
            }

            cells.RemoveWhere(c => c.X < 0 || c.Y < 0);
            return cells;
        }
    }
}
