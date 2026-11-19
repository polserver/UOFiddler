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

using System.Collections.Generic;
using System.Linq;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.House
{
    internal sealed partial class HouseDraft
    {
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
    }
}
