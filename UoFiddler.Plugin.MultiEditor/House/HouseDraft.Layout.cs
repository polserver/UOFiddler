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
using UoFiddler.Plugin.MultiEditor.Styles;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.House
{
    internal sealed partial class HouseDraft
    {
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
    }
}
