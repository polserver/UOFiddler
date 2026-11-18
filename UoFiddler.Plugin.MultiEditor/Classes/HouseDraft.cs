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

        /// <summary>The wall style the foundation ring is built in; without one, the client houses' own (light brick).</summary>
        public HouseStyle FoundationStyle { get; set; }

        /// <summary>The door style of the house: what a theme sets, and what doors made for the rooms are.</summary>
        public HouseStyle DoorStyle { get; set; }

        /// <summary>The stair style of the house.</summary>
        public HouseStyle StairStyle { get; set; }

        /// <summary>Which gable ends of the house's roofs are closed, and with what.</summary>
        public RoofGables Gables { get; set; } = RoofGables.Front;

        public RoofGableFill GableFill { get; set; } = RoofGableFill.Caps;

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
                FoundationStyle = FoundationStyle, DoorStyle = DoorStyle, StairStyle = StairStyle,
                AutoDoors = AutoDoors, FrontDoor = FrontDoor, WindowSpacing = WindowSpacing,
                Gables = Gables, GableFill = GableFill
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
                if (index == 0)
                {
                    firstPlan = plan;
                }

                Partition(wings, index, storey, plan, stairs, layout);
            }

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
