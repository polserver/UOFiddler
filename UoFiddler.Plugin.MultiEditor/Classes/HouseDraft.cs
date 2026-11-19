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
    /// <summary>The outer wall of the first wing that gets the front door.</summary>
    internal enum FrontDoorSide
    {
        None,
        South,
        East
    }

    internal sealed class HouseDraft
    {
        /// <summary>The door pieces the draft hangs by itself in a wall along X and in one along Y.</summary>
        public const string DoorAlongX = "Piece3";
        public const string DoorAlongY = "Piece5";

        public const int StoreyHeight = 20;
        public const int MaxStoreys = 4;
        public const string WallsLayer = "Walls";
        public const string FloorsLayer = "Floors";
        public const string StairsLayer = "Stairs";
        public const string FoundationLayer = "Foundation";

        /// <summary>How far below the ground floor the foundation of most client houses starts.</summary>
        public const int UsualFoundationHeight = 7;

        /// <summary>Z of the ground floor.</summary>
        public int BaseZ { get; set; }

        public HouseStyle[] WallStyles { get; } = new HouseStyle[MaxStoreys];

        public HouseStyle[] FloorStyles { get; } = new HouseStyle[MaxStoreys];

        public HouseStyle RoofStyle { get; set; }

        /// <summary>The floor style a flat roof is laid in; without one, the floor style of the wing's top storey.</summary>
        public HouseStyle FlatRoofStyle { get; set; }

        public bool Foundation { get; set; }

        /// <summary>The wall style the foundation ring is built in; without one, the client houses' own (light brick).</summary>
        public HouseStyle FoundationStyle { get; set; }

        /// <summary>Rows of foundation pieces, one on the other: the stone tower, keep and castle have two.</summary>
        public int FoundationTiers { get; set; } = 1;

        /// <summary>How far below the ground floor the foundation starts.</summary>
        public int FoundationHeight { get; set; } = UsualFoundationHeight;

        /// <summary>The door style of the house: what a theme sets, and what doors made for the rooms are.</summary>
        public HouseStyle DoorStyle { get; set; }

        /// <summary>The stair style of the house.</summary>
        public HouseStyle StairStyle { get; set; }

        /// <summary>Which gable ends of the house's roofs are closed, and with what.</summary>
        public RoofGables Gables { get; set; } = RoofGables.Front;

        public RoofGableFill GableFill { get; set; } = RoofGableFill.Caps;

        /// <summary>
        /// True when flat roofs get battlements, as the client's tower, keep and castle have them:
        /// two rows of the walls' roof edging around the roof and a third of every second piece.
        /// </summary>
        public bool Parapet { get; set; }

        /// <summary>True when the doorways between rooms get a door of <see cref="DoorStyle"/>.</summary>
        public bool AutoDoors { get; set; }

        public FrontDoorSide FrontDoor { get; set; }

        /// <summary>Cells between the windows the draft puts along the outer walls; 0 for none.</summary>
        public int WindowSpacing { get; set; }

        /// <summary>Wall cells where a door or window the draft made by itself was taken out by hand.</summary>
        public HashSet<(int Storey, int X, int Y)> Suppressed { get; } = new HashSet<(int, int, int)>();

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
            copy.Suppressed.UnionWith(Suppressed);
            copy.FloorAreas.AddRange(FloorAreas);
            copy.Stairs.AddRange(Stairs);
            return copy;
        }

        /// <summary>Puts another style in the place of every style the draft holds: after the styles were read again.</summary>
        public void Remap(Func<HouseStyle, HouseStyle> successor)
        {
            for (int k = 0; k < MaxStoreys; ++k)
            {
                WallStyles[k] = successor(WallStyles[k]);
                FloorStyles[k] = successor(FloorStyles[k]);
            }

            RoofStyle = successor(RoofStyle);
            FlatRoofStyle = successor(FlatRoofStyle);
            FoundationStyle = successor(FoundationStyle);
            DoorStyle = successor(DoorStyle);
            StairStyle = successor(StairStyle);
            foreach (var key in new List<(int Storey, int X, int Y)>(Doors.Keys))
            {
                Doors[key] = (successor(Doors[key].Style), Doors[key].Piece);
            }

            for (int i = 0; i < FloorAreas.Count; ++i)
            {
                var a = FloorAreas[i];
                FloorAreas[i] = (a.Storey, a.X0, a.Y0, a.X1, a.Y1, successor(a.Style));
            }

            for (int i = 0; i < Stairs.Count; ++i)
            {
                var s = Stairs[i];
                Stairs[i] = (s.Storey, s.X, s.Y, s.Column, successor(s.Style));
            }

            _pieces.Clear();
        }

        /// <summary>A draft with the same materials and nothing changed by hand: the next house.</summary>
        public HouseDraft Fresh()
        {
            var copy = new HouseDraft
            {
                RoofStyle = RoofStyle, FlatRoofStyle = FlatRoofStyle, Foundation = Foundation,
                FoundationStyle = FoundationStyle, FoundationTiers = FoundationTiers, FoundationHeight = FoundationHeight,
                DoorStyle = DoorStyle, StairStyle = StairStyle,
                AutoDoors = AutoDoors, FrontDoor = FrontDoor, WindowSpacing = WindowSpacing,
                Gables = Gables, GableFill = GableFill, Parapet = Parapet
            };
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

        /// <summary>The wall edges of a storey with its generated doors and windows.</summary>
        public sealed class StoreyLayout
        {
            /// <summary>Cells with a wall on their +Y edge.</summary>
            public HashSet<(int X, int Y)> South { get; } = new HashSet<(int X, int Y)>();

            /// <summary>Cells with a wall on their +X edge.</summary>
            public HashSet<(int X, int Y)> East { get; } = new HashSet<(int X, int Y)>();

            /// <summary>Doors the draft makes itself (room doors, the front door): wall cell and door piece.</summary>
            public Dictionary<(int X, int Y), string> Doors { get; } = new Dictionary<(int X, int Y), string>();

            /// <summary>Wall cells the draft makes windows itself.</summary>
            public HashSet<(int X, int Y)> Windows { get; } = new HashSet<(int X, int Y)>();

            /// <summary>The wall cell of the front door, when the storey has one.</summary>
            public (int X, int Y)? FrontDoor { get; set; }
        }

        /// <summary>
        /// The wall edges of a storey: the outline of its rooms, with the edges added by hand that
        /// still touch a room and without the ones taken away.
        /// </summary>
        public void Edges(IReadOnlyList<RoofWing> wings, int storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east)
        {
            StoreyLayout layout = LayOut(wings, storey);
            south = layout.South;
            east = layout.East;
        }

        /// <summary>
        /// The walls of a storey and what the draft puts in them by itself: the outline of the
        /// wings, the walls between the rooms of each wing with their doorways (doors, with
        /// <see cref="AutoDoors"/>), the edges added and taken away by hand, the front door and
        /// the windows along the outer walls.
        /// </summary>
        public StoreyLayout LayOut(IReadOnlyList<RoofWing> wings, int storey)
        {
            var layout = new StoreyLayout();
            HashSet<(int X, int Y)> region = Region(wings, storey);
            var outerSouth = new HashSet<(int X, int Y)>();
            var outerEast = new HashSet<(int X, int Y)>();
            Outline(region, outerSouth, outerEast);
            layout.South.UnionWith(outerSouth);
            layout.East.UnionWith(outerEast);

            HashSet<(int X, int Y)> stairs = StairCells(storey);
            RoomPlan firstPlan = null;
            for (int index = 0; index < wings.Count; ++index)
            {
                RoofWing wing = wings[index];
                if (wing.Storeys <= storey)
                {
                    continue;
                }

                (int rooms, RoomLayout kind) = wing.RoomsOn(storey);
                RoomPlan plan = RoomPlanner.Plan(wing.X0 + 1, wing.Y0 + 1, wing.X1, wing.Y1, kind, Math.Clamp(rooms, 1, MaxRooms));
                RoomPlanner.Avoid(plan, stairs);
                if (index == 0)
                {
                    firstPlan = plan;
                }

                Partition(wings, index, storey, plan, stairs, layout);
            }

            WallOff(wings, storey, region, stairs, layout);

            foreach ((int x, int y) in AddedSouth[storey])
            {
                if (region.Contains((x, y)) || region.Contains((x, y + 1)))
                {
                    layout.South.Add((x, y));
                }
            }

            foreach ((int x, int y) in AddedEast[storey])
            {
                if (region.Contains((x, y)) || region.Contains((x + 1, y)))
                {
                    layout.East.Add((x, y));
                }
            }

            layout.South.ExceptWith(ErasedSouth[storey]);
            layout.East.ExceptWith(ErasedEast[storey]);

            if (storey == 0)
            {
                AddFrontDoor(wings, firstPlan, outerSouth, outerEast, stairs, layout);
            }

            if (WindowSpacing > 0)
            {
                AddWindows(storey, outerSouth, true, layout);
                AddWindows(storey, outerEast, false, layout);
            }

            return layout;
        }

        /// <summary>The cells stairs stand in on a storey: the stairs that start there and the opening of those that arrive.</summary>
        private HashSet<(int X, int Y)> StairCells(int storey)
        {
            var cells = new HashSet<(int X, int Y)>();
            foreach ((int stairStorey, int x, int y, string column, HouseStyle style) in Stairs)
            {
                if (stairStorey != storey && stairStorey != storey - 1)
                {
                    continue;
                }

                foreach ((ushort _, int tx, int ty, int _) in StairTool.Tiles(style, column, x, y, 0))
                {
                    cells.Add((tx, ty));
                }
            }

            return cells;
        }

        /// <summary>
        /// The walls between the rooms of a wing. Each pair of rooms the plan joins gets one
        /// doorway, in the middle of the wall they share, or as near to it as stairs allow. A
        /// wall is left out where another wing's rooms lie and where it would cut through stairs.
        /// </summary>
        private void Partition(IReadOnlyList<RoofWing> wings, int index, int storey, RoomPlan plan, HashSet<(int X, int Y)> stairs, StoreyLayout layout)
        {
            if (plan.Rooms.Count < 2)
            {
                return;
            }

            RoofWing wing = wings[index];
            int RoomAt(int x, int y)
            {
                for (int i = 0; i < plan.Rooms.Count; ++i)
                {
                    (int x0, int y0, int x1, int y1) = plan.Rooms[i];
                    if (x >= x0 && x <= x1 && y >= y0 && y <= y1)
                    {
                        return i;
                    }
                }

                return -1;
            }

            bool Other(int x, int y)
            {
                for (int j = 0; j < wings.Count; ++j)
                {
                    RoofWing other = wings[j];
                    if (j != index && other.Storeys > storey && x > other.X0 && x <= other.X1 && y > other.Y0 && y <= other.Y1)
                    {
                        return true;
                    }
                }

                return false;
            }

            var between = new Dictionary<(int A, int B), List<(int X, int Y, bool South)>>();
            for (int x = wing.X0 + 1; x <= wing.X1; ++x)
            {
                for (int y = wing.Y0 + 1; y <= wing.Y1; ++y)
                {
                    int room = RoomAt(x, y);
                    foreach ((int nx, int ny, bool south) in new[] { (x + 1, y, false), (x, y + 1, true) })
                    {
                        if (nx > wing.X1 || ny > wing.Y1)
                        {
                            continue;
                        }

                        int next = RoomAt(nx, ny);
                        if (next == room || Other(x, y) || Other(nx, ny) || (stairs.Contains((x, y)) && stairs.Contains((nx, ny))))
                        {
                            continue;
                        }

                        (int, int) key = (Math.Min(room, next), Math.Max(room, next));
                        if (!between.TryGetValue(key, out List<(int X, int Y, bool South)> shared))
                        {
                            between[key] = shared = new List<(int X, int Y, bool South)>();
                        }

                        shared.Add((x, y, south));
                    }
                }
            }

            var doorways = new HashSet<(int X, int Y, bool South)>();
            foreach ((int a, int b) in plan.Links)
            {
                if (!between.TryGetValue((Math.Min(a, b), Math.Max(a, b)), out List<(int X, int Y, bool South)> shared) || shared.Count == 0)
                {
                    continue;
                }

                int middle = (shared.Count - 1) / 2, pick = middle, nearest = int.MaxValue;
                for (int i = 0; i < shared.Count; ++i)
                {
                    (int x, int y, bool south) = shared[i];
                    bool free = !stairs.Contains((x, y)) && !stairs.Contains(south ? (x, y + 1) : (x + 1, y));
                    if (free && Math.Abs(i - middle) < nearest)
                    {
                        (pick, nearest) = (i, Math.Abs(i - middle));
                    }
                }

                doorways.Add(shared[pick]);
            }

            foreach (List<(int X, int Y, bool South)> shared in between.Values)
            {
                foreach ((int x, int y, bool south) in shared)
                {
                    if (doorways.Contains((x, y, south)))
                    {
                        // A doorway: open, or a door in the wall.
                        if (!AutoDoors || Suppressed.Contains((storey, x, y)))
                        {
                            continue;
                        }

                        layout.Doors[(x, y)] = south ? DoorAlongX : DoorAlongY;
                    }

                    (south ? layout.South : layout.East).Add((x, y));
                }
            }
        }

        /// <summary>
        /// The walls of the wings that keep them (<see cref="RoofWing.Walled"/>) against the wings
        /// they meet. Each cell belongs to one wing: a walled one where there is one, else the
        /// first. A wall stands between cells of different wings when either is walled, and each
        /// stretch of wall between the same two wings gets one doorway in its middle (a door, with
        /// <see cref="AutoDoors"/>), as near to it as stairs allow.
        /// </summary>
        private void WallOff(IReadOnlyList<RoofWing> wings, int storey, HashSet<(int X, int Y)> region, HashSet<(int X, int Y)> stairs, StoreyLayout layout)
        {
            bool any = false;
            foreach (RoofWing wing in wings)
            {
                any |= wing.Walled && wing.Storeys > storey;
            }

            if (!any)
            {
                return;
            }

            var owners = new Dictionary<(int X, int Y), int>();
            int Owner((int X, int Y) cell)
            {
                if (owners.TryGetValue(cell, out int known))
                {
                    return known;
                }

                int first = -1, walled = -1;
                for (int i = 0; i < wings.Count && walled < 0; ++i)
                {
                    RoofWing wing = wings[i];
                    if (wing.Storeys > storey && cell.X > wing.X0 && cell.X <= wing.X1 && cell.Y > wing.Y0 && cell.Y <= wing.Y1)
                    {
                        first = first < 0 ? i : first;
                        walled = wing.Walled ? i : walled;
                    }
                }

                return owners[cell] = walled >= 0 ? walled : first;
            }

            // Stretches of wall: along one line, between the same two wings, cell after cell.
            var stretches = new Dictionary<(bool South, int Line, int A, int B), List<(int X, int Y)>>();
            foreach ((int x, int y) in region)
            {
                foreach ((int nx, int ny, bool south) in new[] { (x + 1, y, false), (x, y + 1, true) })
                {
                    if (!region.Contains((nx, ny)) || (south ? layout.South : layout.East).Contains((x, y)))
                    {
                        continue;
                    }

                    int a = Owner((x, y)), b = Owner((nx, ny));
                    if (a == b || a < 0 || b < 0 || !(wings[a].Walled || wings[b].Walled))
                    {
                        continue;
                    }

                    var key = (south, south ? y : x, a, b);
                    if (!stretches.TryGetValue(key, out List<(int X, int Y)> cells))
                    {
                        stretches[key] = cells = new List<(int X, int Y)>();
                    }

                    cells.Add((x, y));
                }
            }

            foreach (KeyValuePair<(bool South, int Line, int A, int B), List<(int X, int Y)>> stretch in stretches)
            {
                bool south = stretch.Key.South;
                List<(int X, int Y)> cells = stretch.Value.OrderBy(c => south ? c.X : c.Y).ToList();
                for (int start = 0; start < cells.Count;)
                {
                    int end = start;
                    while (end + 1 < cells.Count && (south ? cells[end + 1].X - cells[end].X : cells[end + 1].Y - cells[end].Y) == 1)
                    {
                        ++end;
                    }

                    int middle = (start + end) / 2, pick = middle, nearest = int.MaxValue;
                    for (int i = start; i <= end; ++i)
                    {
                        (int x, int y) = cells[i];
                        bool free = !stairs.Contains((x, y)) && !stairs.Contains(south ? (x, y + 1) : (x + 1, y));
                        if (free && Math.Abs(i - middle) < nearest)
                        {
                            (pick, nearest) = (i, Math.Abs(i - middle));
                        }
                    }

                    for (int i = start; i <= end; ++i)
                    {
                        (int x, int y) = cells[i];
                        if (i == pick)
                        {
                            if (!AutoDoors || Suppressed.Contains((storey, x, y)))
                            {
                                continue;
                            }

                            layout.Doors[(x, y)] = south ? DoorAlongX : DoorAlongY;
                        }

                        (south ? layout.South : layout.East).Add((x, y));
                    }

                    start = end + 1;
                }
            }
        }

        /// <summary>
        /// The front door: in the chosen outer wall of the first wing, in the middle of it, or of
        /// the corridor or hall where that reaches the wall. Corners, wall ends and stairs are passed over.
        /// </summary>
        private void AddFrontDoor(IReadOnlyList<RoofWing> wings, RoomPlan plan, HashSet<(int X, int Y)> outerSouth, HashSet<(int X, int Y)> outerEast,
            HashSet<(int X, int Y)> stairs, StoreyLayout layout)
        {
            if (FrontDoor == FrontDoorSide.None || wings.Count == 0)
            {
                return;
            }

            RoofWing wing = wings[0];
            bool south = FrontDoor == FrontDoorSide.South;
            int from = south ? wing.X0 + 1 : wing.Y0 + 1, to = south ? wing.X1 : wing.Y1;
            double centre = (from + to) / 2.0;
            if (plan is { Main: >= 0 })
            {
                (int x0, int y0, int x1, int y1) = plan.Rooms[plan.Main];
                if (south ? y1 == wing.Y1 : x1 == wing.X1)
                {
                    centre = south ? (x0 + x1) / 2.0 : (y0 + y1) / 2.0;
                }
            }

            (int X, int Y)? best = null;
            double nearest = double.MaxValue;
            for (int i = from; i <= to; ++i)
            {
                (int X, int Y) cell = south ? (i, wing.Y1) : (wing.X1, i);
                bool wall = (south ? outerSouth : outerEast).Contains(cell) && (south ? layout.South : layout.East).Contains(cell);
                bool straight = wall && !(south ? layout.East : layout.South).Contains(cell);
                if (straight && !stairs.Contains(cell) && Math.Abs(i - centre) < nearest)
                {
                    (best, nearest) = (cell, Math.Abs(i - centre));
                }
            }

            if (best is { } door && !Suppressed.Contains((0, door.X, door.Y)))
            {
                layout.Doors[door] = south ? DoorAlongX : DoorAlongY;
                layout.FrontDoor = door;
            }
        }

        /// <summary>
        /// Windows along the outer walls, <see cref="WindowSpacing"/> cells apart and centred on
        /// each stretch of wall between corners and the walls that meet it (so on each room).
        /// A stretch has wall left at both ends; doors and cells set back by hand are passed over.
        /// </summary>
        private void AddWindows(int storey, HashSet<(int X, int Y)> outer, bool alongX, StoreyLayout layout)
        {
            var run = new List<(int X, int Y)>();
            void Close()
            {
                if (run.Count >= 3)
                {
                    int count = 1 + ((run.Count - 3) / WindowSpacing);
                    int start = (run.Count - 1 - ((count - 1) * WindowSpacing)) / 2;
                    for (int i = 0; i < count; ++i)
                    {
                        (int X, int Y) cell = run[start + (i * WindowSpacing)];
                        if (!Doors.ContainsKey((storey, cell.X, cell.Y)) && !layout.Doors.ContainsKey(cell) && !Suppressed.Contains((storey, cell.X, cell.Y)))
                        {
                            layout.Windows.Add(cell);
                        }
                    }
                }

                run.Clear();
            }

            foreach (IGrouping<int, (int X, int Y)> line in outer.GroupBy(c => alongX ? c.Y : c.X))
            {
                int last = int.MinValue;
                foreach ((int X, int Y) cell in line.OrderBy(c => alongX ? c.X : c.Y))
                {
                    int at = alongX ? cell.X : cell.Y;
                    if (at != last + 1)
                    {
                        Close();
                    }

                    last = at;
                    bool straight = (alongX ? layout.South : layout.East).Contains(cell) && !(alongX ? layout.East : layout.South).Contains(cell);

                    // A wall that meets this one from the far side ends the stretch as a corner does.
                    bool joined = alongX ? layout.East.Contains((cell.X, cell.Y + 1)) : layout.South.Contains((cell.X + 1, cell.Y));
                    if (!straight || joined)
                    {
                        Close();
                        continue;
                    }

                    run.Add(cell);
                }

                Close();
            }
        }

        /// <summary>
        /// Moves what was changed by hand inside a wing along with that wing: walls added and
        /// taken away, doors, windows, floor areas and stairs whose cell lies in the box.
        /// </summary>
        public void ShiftWithin((int X0, int Y0, int X1, int Y1) box, int dx, int dy)
        {
            if (dx == 0 && dy == 0)
            {
                return;
            }

            bool In(int x, int y) => x >= box.X0 && x <= box.X1 && y >= box.Y0 && y <= box.Y1;
            void Move(HashSet<(int X, int Y)> set)
            {
                var moved = new List<(int X, int Y)>();
                foreach ((int x, int y) in set)
                {
                    moved.Add(In(x, y) ? (x + dx, y + dy) : (x, y));
                }

                set.Clear();
                set.UnionWith(moved);
            }

            void MoveOnStoreys(HashSet<(int Storey, int X, int Y)> set)
            {
                var moved = new List<(int Storey, int X, int Y)>();
                foreach ((int storey, int x, int y) in set)
                {
                    moved.Add(In(x, y) ? (storey, x + dx, y + dy) : (storey, x, y));
                }

                set.Clear();
                set.UnionWith(moved);
            }

            for (int k = 0; k < MaxStoreys; ++k)
            {
                Move(AddedSouth[k]);
                Move(AddedEast[k]);
                Move(ErasedSouth[k]);
                Move(ErasedEast[k]);
            }

            MoveOnStoreys(Windows);
            MoveOnStoreys(Suppressed);
            var doors = new List<KeyValuePair<(int Storey, int X, int Y), (HouseStyle Style, string Piece)>>(Doors);
            Doors.Clear();
            foreach (var door in doors)
            {
                (int storey, int x, int y) = door.Key;
                Doors[In(x, y) ? (storey, x + dx, y + dy) : door.Key] = door.Value;
            }

            for (int i = 0; i < FloorAreas.Count; ++i)
            {
                var a = FloorAreas[i];
                if (In(a.X0, a.Y0) && In(a.X1, a.Y1))
                {
                    FloorAreas[i] = (a.Storey, a.X0 + dx, a.Y0 + dy, a.X1 + dx, a.Y1 + dy, a.Style);
                }
            }

            for (int i = 0; i < Stairs.Count; ++i)
            {
                var s = Stairs[i];
                if (In(s.X, s.Y))
                {
                    Stairs[i] = (s.Storey, s.X + dx, s.Y + dy, s.Column, s.Style);
                }
            }
        }

        /// <summary>
        /// What was changed by hand inside a wing, for a copy of the wing: walls added and taken
        /// away, doors, windows, floor areas and stairs whose cell lies in the box.
        /// </summary>
        public HouseDraft CopyWithin((int X0, int Y0, int X1, int Y1) box)
        {
            bool In(int x, int y) => x >= box.X0 && x <= box.X1 && y >= box.Y0 && y <= box.Y1;
            var part = new HouseDraft();
            for (int k = 0; k < MaxStoreys; ++k)
            {
                part.AddedSouth[k].UnionWith(AddedSouth[k].Where(c => In(c.X, c.Y)));
                part.AddedEast[k].UnionWith(AddedEast[k].Where(c => In(c.X, c.Y)));
                part.ErasedSouth[k].UnionWith(ErasedSouth[k].Where(c => In(c.X, c.Y)));
                part.ErasedEast[k].UnionWith(ErasedEast[k].Where(c => In(c.X, c.Y)));
            }

            foreach (var door in Doors)
            {
                if (In(door.Key.X, door.Key.Y))
                {
                    part.Doors[door.Key] = door.Value;
                }
            }

            part.Windows.UnionWith(Windows.Where(c => In(c.X, c.Y)));
            part.Suppressed.UnionWith(Suppressed.Where(c => In(c.X, c.Y)));
            part.FloorAreas.AddRange(FloorAreas.Where(a => In(a.X0, a.Y0) && In(a.X1, a.Y1)));
            part.Stairs.AddRange(Stairs.Where(s => In(s.X, s.Y)));
            return part;
        }

        /// <summary>Adds what <see cref="CopyWithin"/> took, moved by this many cells: the hand edits of a pasted wing.</summary>
        public void AddShifted(HouseDraft part, int dx, int dy)
        {
            HouseDraft moved = part.Clone();
            moved.Shift(dx, dy);

            // Where the pasted wing says otherwise, it wins: a wall added there is no longer
            // taken away, a door replaces a window, and the reverse.
            for (int k = 0; k < MaxStoreys; ++k)
            {
                ErasedSouth[k].ExceptWith(moved.AddedSouth[k]);
                ErasedEast[k].ExceptWith(moved.AddedEast[k]);
                AddedSouth[k].ExceptWith(moved.ErasedSouth[k]);
                AddedEast[k].ExceptWith(moved.ErasedEast[k]);
                AddedSouth[k].UnionWith(moved.AddedSouth[k]);
                AddedEast[k].UnionWith(moved.AddedEast[k]);
                ErasedSouth[k].UnionWith(moved.ErasedSouth[k]);
                ErasedEast[k].UnionWith(moved.ErasedEast[k]);
            }

            foreach (var door in moved.Doors)
            {
                Doors[door.Key] = door.Value;
                Windows.Remove(door.Key);
            }

            foreach (var window in moved.Windows)
            {
                Windows.Add(window);
                Doors.Remove(window);
            }

            Suppressed.UnionWith(moved.Suppressed);
            FloorAreas.AddRange(moved.FloorAreas);

            // A stair already starting in a cell gives way to the pasted one.
            foreach (var stair in moved.Stairs)
            {
                Stairs.RemoveAll(s => s.Storey == stair.Storey && s.X == stair.X && s.Y == stair.Y);
                Stairs.Add(stair);
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

            var suppressed = new List<(int Storey, int X, int Y)>(Suppressed);
            Suppressed.Clear();
            foreach ((int storey, int x, int y) in suppressed)
            {
                Suppressed.Add((storey, x + dx, y + dy));
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
                StoreyLayout layout = LayOut(wings, storey);
                HashSet<(int X, int Y)> south = layout.South, east = layout.East;

                HouseStyle foundation = FoundationStyle;
                if (foundation == null)
                {
                    styles.TryGetRole(FoundationTool.FoundationPost, out foundation, out _);
                }

                if (storey == 0 && Foundation && foundation != null)
                {
                    var ringSouth = new HashSet<(int X, int Y)>();
                    var ringEast = new HashSet<(int X, int Y)>();
                    Outline(region, ringSouth, ringEast);
                    foreach ((int x, int y) in WallCells(ringSouth, ringEast))
                    {
                        string column = WallPlanner.PieceFor(ringSouth, ringEast, x, y);
                        ushort id = column != null ? foundation.Get(column) : (ushort)0;
                        // One row, or rows one on the other, each as high as the piece.
                        int rise = id != 0 ? Math.Max(1, (int)Ultima.TileData.ItemTable[id].Height) : 0;
                        for (int tier = 0; id != 0 && tier < Math.Clamp(FoundationTiers, 1, 3); ++tier)
                        {
                            built.Tiles.Add((id, x, y, z - FoundationHeight + (tier * rise), FoundationLayer));
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

                    // A door the draft hangs by itself: between rooms, or the front door.
                    if (DoorStyle != null && layout.Doors.TryGetValue((x, y), out string ownPiece) &&
                        (WallPlanner.DoorAlongX(ownPiece) ? south : east).Contains((x, y)) && DoorStyle.Get(ownPiece) is var ownId and not 0)
                    {
                        built.Tiles.Add((ownId, x, y, z, WallsLayer));
                        built.Walls.Add((ownId, x, y, z));
                        continue;
                    }

                    string column = WallPlanner.PieceFor(wallStyle, south, east, x, y);
                    if (column == null || wallStyle == null)
                    {
                        continue;
                    }

                    ushort id = wallStyle.Get(column);
                    if ((Windows.Contains((storey, x, y)) || layout.Windows.Contains((x, y))) && column != "Corner" && column != "Post" &&
                        styles.WindowFor(wallStyle, south.Contains((x, y))) is var windowId and not 0)
                    {
                        id = windowId;
                    }

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

            if (Parapet)
            {
                AddParapets(wings, styles, built);
            }

            if (Foundation && StairStyle != null && top > 0 && LayOut(wings, 0).FrontDoor is { } front)
            {
                AddSteps(front, styles, built);
            }

            return built;
        }

        /// <summary>
        /// Steps up to the front door of a house on a foundation, as the client's houses have
        /// them: one row outside the door, a step under it and a corner piece on each side, 5
        /// below the floor (the pieces are 5 high).
        /// </summary>
        private void AddSteps((int X, int Y) door, HouseStyles styles, Built built)
        {
            bool south = FrontDoor == FrontDoorSide.South;
            ushort step = StairStyle.Get(south ? "North" : "West");
            if (step == 0)
            {
                return;
            }

            // The step's top meets the floor; on a foundation lower than the step it starts where the foundation does.
            int z = BaseZ - Math.Min(FoundationHeight, Math.Max(1, (int)Ultima.TileData.ItemTable[step].Height));
            (int X, int Y) at = south ? (door.X, door.Y + 1) : (door.X + 1, door.Y);
            built.Tiles.Add((step, at.X, at.Y, z, StairsLayer));

            // The corners: south-east, north-east, north-west, south-west of a stair set.
            ushort before = StairStyle.Get("Squared1"), after = StairStyle.Get("Squared2");
            if (after != 0 && styles.TryGetStairCorner(after, out ushort[] corners, out _) && corners.Length == 4)
            {
                (before, after) = south ? (corners[3], corners[0]) : (corners[1], corners[0]);
            }
            else if (!south)
            {
                (before, after) = (0, 0);
            }

            if (before != 0 && (south ? at.X : at.Y) > 0)
            {
                built.Tiles.Add((before, south ? at.X - 1 : at.X, south ? at.Y : at.Y - 1, z, StairsLayer));
            }

            if (after != 0)
            {
                built.Tiles.Add((after, south ? at.X + 1 : at.X, south ? at.Y : at.Y + 1, z, StairsLayer));
            }
        }

        /// <summary>
        /// Battlements around the flat roofs. The flat wings of one height are walled as one
        /// roof: along its edge, but not where a taller wing stands (its wall is there). Measured
        /// on the client's tower, keep and castle: the edging stands in the roof's wall cells at
        /// the roof's Z and once more on top of that, and a third row has every second straight
        /// piece, leaving the corners out.
        /// </summary>
        private void AddParapets(IReadOnlyList<RoofWing> wings, HouseStyles styles, Built built)
        {
            // A wing without a roof is a walled yard: its walls carry battlements too.
            foreach (IGrouping<int, RoofWing> level in wings.Where(w => w.NoRoof || w.Shape == RoofShape.Flat).GroupBy(w => Math.Clamp(w.Storeys, 1, MaxStoreys)))
            {
                int storeys = level.Key;
                HouseStyle style = styles.ParapetFor(WallStyles[storeys - 1]);
                if (style == null)
                {
                    continue;
                }

                HashSet<(int X, int Y)> above = Region(wings, storeys);
                var roof = new HashSet<(int X, int Y)>();
                foreach (RoofWing wing in level)
                {
                    for (int x = wing.X0 + 1; x <= wing.X1; ++x)
                    {
                        for (int y = wing.Y0 + 1; y <= wing.Y1; ++y)
                        {
                            if (!above.Contains((x, y)))
                            {
                                roof.Add((x, y));
                            }
                        }
                    }
                }

                if (roof.Count == 0)
                {
                    continue;
                }

                var south = new HashSet<(int X, int Y)>();
                var east = new HashSet<(int X, int Y)>();
                Outline(roof, south, east);
                south.RemoveWhere(c => above.Contains(c) || above.Contains((c.X, c.Y + 1)));
                east.RemoveWhere(c => above.Contains(c) || above.Contains((c.X + 1, c.Y)));

                // The wall cells start one before the roof cells; the tower counts its merlons from there.
                int minX = roof.Min(c => c.X) - 1, minY = roof.Min(c => c.Y) - 1;
                int z = BaseZ + (storeys * StoreyHeight);
                int tier = Math.Max(1, (int)Ultima.TileData.ItemTable[style.Get("South2")].Height);
                foreach ((int x, int y) in WallCells(south, east))
                {
                    string column = WallPlanner.PieceFor(south, east, x, y);
                    ushort id = column != null ? style.Get(column) : (ushort)0;
                    if (id == 0)
                    {
                        continue;
                    }

                    built.Tiles.Add((id, x, y, z, RoofTool.LayerName));
                    built.Tiles.Add((id, x, y, z + tier, RoofTool.LayerName));

                    // The merlons: every second straight piece.
                    bool alongX = column.StartsWith("South", StringComparison.Ordinal), alongY = column.StartsWith("East", StringComparison.Ordinal);
                    if ((alongX && (x - minX) % 2 == 0) || (alongY && (y - minY) % 2 == 0))
                    {
                        built.Tiles.Add((id, x, y, z + (2 * tier), RoofTool.LayerName));
                    }
                    else if (column == "Corner" && (x - minX) % 2 == 0 && style.Get("South2") is var merlon and not 0)
                    {
                        // A corner cell in step with the wall along X carries that wall's merlon (the tower's inner corner does).
                        built.Tiles.Add((merlon, x, y, z + (2 * tier), RoofTool.LayerName));
                    }
                }
            }
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
