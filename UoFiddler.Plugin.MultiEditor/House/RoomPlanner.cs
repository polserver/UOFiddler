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

namespace UoFiddler.Plugin.MultiEditor.House
{
    /// <summary>
    /// Divides the inside of a house wing into rooms. It only says where the rooms are and which
    /// are joined: the draft makes the walls, doorways, doors and windows from that.
    /// </summary>
    internal static class RoomPlanner
    {
        /// <summary>Narrowest a room may be, in cells.</summary>
        public const int MinRoom = 2;

        /// <summary>
        /// The rooms of the cells <paramref name="x0"/>..<paramref name="x1"/> by
        /// <paramref name="y0"/>..<paramref name="y1"/>. A layout the wing is too small for falls
        /// back to a row of rooms, and that to fewer rooms, down to the wing undivided.
        /// </summary>
        public static RoomPlan Plan(int x0, int y0, int x1, int y1, RoomLayout layout, int rooms)
        {
            int width = x1 - x0 + 1, depth = y1 - y0 + 1;
            bool alongX = width >= depth;
            int length = alongX ? width : depth, cross = alongX ? depth : width;
            rooms = Math.Max(1, rooms);

            // A rectangle given along the wing's longer side (a) and across it (b), as cells.
            (int, int, int, int) Rect((int From, int To) a, (int From, int To) b) => alongX
                ? (x0 + a.From, y0 + b.From, x0 + a.To, y0 + b.To)
                : (x0 + b.From, y0 + a.From, x0 + b.To, y0 + a.To);

            var plan = new RoomPlan();
            switch (layout)
            {
                case RoomLayout.Grid when rooms >= 3 && cross >= 2 * MinRoom:
                {
                    List<(int From, int To)> halves = Split(0, cross, 2);
                    List<(int From, int To)> first = Split(0, length, (rooms + 1) / 2), second = Split(0, length, rooms / 2);
                    foreach ((int From, int To) part in first)
                    {
                        plan.Rooms.Add(Rect(part, halves[0]));
                    }

                    foreach ((int From, int To) part in second)
                    {
                        plan.Rooms.Add(Rect(part, halves[1]));
                    }

                    for (int i = 1; i < first.Count; ++i)
                    {
                        plan.Links.Add((i - 1, i));
                    }

                    // Each room of the second row opens to the room of the first it shares most wall with.
                    for (int j = 0; j < second.Count; ++j)
                    {
                        int best = 0, most = -1;
                        for (int i = 0; i < first.Count; ++i)
                        {
                            int shared = Math.Min(first[i].To, second[j].To) - Math.Max(first[i].From, second[j].From) + 1;
                            if (shared > most)
                            {
                                (best, most) = (i, shared);
                            }
                        }

                        plan.Links.Add((best, first.Count + j));
                    }

                    return plan;
                }

                case RoomLayout.Corridor when rooms >= 2 && cross >= (2 * MinRoom) + 1:
                {
                    int corridor = cross >= 8 ? 2 : 1;
                    int side = (cross - corridor) / 2;
                    plan.Main = 0;
                    plan.Rooms.Add(Rect((0, length - 1), (side, side + corridor - 1)));
                    foreach ((int From, int To) part in Split(0, length, (rooms + 1) / 2))
                    {
                        plan.Links.Add((0, plan.Rooms.Count));
                        plan.Rooms.Add(Rect(part, (0, side - 1)));
                    }

                    foreach ((int From, int To) part in Split(0, length, rooms / 2))
                    {
                        plan.Links.Add((0, plan.Rooms.Count));
                        plan.Rooms.Add(Rect(part, (side + corridor, cross - 1)));
                    }

                    return plan;
                }

                case RoomLayout.Hall when rooms >= 2 && length >= 3 + MinRoom:
                {
                    int hall = Math.Clamp((int)Math.Round(length * 0.6), 3, length - MinRoom);
                    plan.Main = 0;
                    plan.Rooms.Add(Rect((0, hall - 1), (0, cross - 1)));
                    foreach ((int From, int To) part in Split(0, cross, rooms - 1))
                    {
                        plan.Links.Add((0, plan.Rooms.Count));
                        plan.Rooms.Add(Rect((hall, length - 1), part));
                    }

                    return plan;
                }
            }

            foreach ((int From, int To) part in Split(0, length, Math.Min(rooms, HouseDraft.MaxRooms)))
            {
                if (plan.Rooms.Count > 0)
                {
                    plan.Links.Add((plan.Rooms.Count - 1, plan.Rooms.Count));
                }

                plan.Rooms.Add(Rect(part, (0, cross - 1)));
            }

            return plan;
        }

        /// <summary>
        /// Moves the walls between rooms off stairs: a wall that would run between two stair
        /// cells steps one or two cells aside, where the rooms on both sides stay wide enough.
        /// A wall that cannot is left where it is (the draft then leaves it open at the stairs).
        /// </summary>
        public static void Avoid(RoomPlan plan, ICollection<(int X, int Y)> stairs)
        {
            if (stairs.Count == 0 || plan.Rooms.Count < 2)
            {
                return;
            }

            foreach (bool alongY in new[] { true, false })
            {
                // A wall along Y stands after column c: between the rooms that end at c and those that start at c + 1.
                int End((int X0, int Y0, int X1, int Y1) r) => alongY ? r.X1 : r.Y1;
                int Start((int X0, int Y0, int X1, int Y1) r) => alongY ? r.X0 : r.Y0;
                (int From, int To) Side((int X0, int Y0, int X1, int Y1) r) => alongY ? (r.Y0, r.Y1) : (r.X0, r.X1);
                bool Cut(int c, List<int> before)
                {
                    foreach (int i in before)
                    {
                        (int from, int to) = Side(plan.Rooms[i]);
                        for (int at = from; at <= to; ++at)
                        {
                            if (alongY ? stairs.Contains((c, at)) && stairs.Contains((c + 1, at)) : stairs.Contains((at, c)) && stairs.Contains((at, c + 1)))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                foreach (int c in plan.Rooms.Select(End).Distinct().ToList())
                {
                    var before = new List<int>();
                    var after = new List<int>();
                    for (int i = 0; i < plan.Rooms.Count; ++i)
                    {
                        if (End(plan.Rooms[i]) == c)
                        {
                            before.Add(i);
                        }
                        else if (Start(plan.Rooms[i]) == c + 1)
                        {
                            after.Add(i);
                        }
                    }

                    // Only a wall with the same rooms' length on both sides can move as one.
                    if (after.Count == 0 || before.Sum(i => Side(plan.Rooms[i]).To - Side(plan.Rooms[i]).From + 1) != after.Sum(i => Side(plan.Rooms[i]).To - Side(plan.Rooms[i]).From + 1) ||
                        !Cut(c, before))
                    {
                        continue;
                    }

                    foreach (int by in new[] { 1, -1, 2, -2 })
                    {
                        // A room keeps the least width; one already narrower (a corridor) keeps its own.
                        int least(int i) => Math.Min(MinRoom, End(plan.Rooms[i]) - Start(plan.Rooms[i]) + 1);
                        bool fits = before.All(i => c + by - Start(plan.Rooms[i]) + 1 >= least(i)) && after.All(i => End(plan.Rooms[i]) - (c + by + 1) + 1 >= least(i));
                        if (!fits || Cut(c + by, before))
                        {
                            continue;
                        }

                        foreach (int i in before)
                        {
                            (int x0, int y0, int x1, int y1) = plan.Rooms[i];
                            plan.Rooms[i] = alongY ? (x0, y0, c + by, y1) : (x0, y0, x1, c + by);
                        }

                        foreach (int i in after)
                        {
                            (int x0, int y0, int x1, int y1) = plan.Rooms[i];
                            plan.Rooms[i] = alongY ? (c + by + 1, y0, x1, y1) : (x0, c + by + 1, x1, y1);
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Cuts a span of cells into parts of about the same size, each at least
        /// <see cref="MinRoom"/> long: fewer parts when the span is too short for that many.
        /// </summary>
        private static List<(int From, int To)> Split(int from, int length, int parts)
        {
            parts = Math.Max(1, Math.Min(parts, length / MinRoom));
            var result = new List<(int, int)>();
            int start = 0;
            for (int i = 1; i <= parts; ++i)
            {
                int end = i == parts ? length : (int)Math.Round(i * length / (double)parts);
                result.Add((from + start, from + end - 1));
                start = end;
            }

            return result;
        }
    }
}
