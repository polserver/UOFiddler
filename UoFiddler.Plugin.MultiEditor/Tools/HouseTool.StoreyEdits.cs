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
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    internal sealed partial class HouseTool
    {
        /// <summary>The storey the work plane is on (the chosen floor); the ground floor with all floors shown.</summary>
        public int StoreyFor(int? workZ)
        {
            if (workZ is not { } z)
            {
                return 0;
            }

            return Math.Clamp((int)Math.Round((z - BaseZ) / (double)HouseDraft.StoreyHeight), 0, TopStorey - 1);
        }

        /// <summary>The wall edges of a storey as the draft has them now.</summary>
        public void EdgesOf(int storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east)
        {
            _draft.Edges(Wings, storey, out south, out east);
        }

        /// <summary>Adds wall edges to a storey (partitions), or takes edges away (openings).</summary>
        public void EditEdges(int storey, IReadOnlyCollection<(int X, int Y)> south, IReadOnlyCollection<(int X, int Y)> east, bool erase)
        {
            if (south.Count + east.Count == 0)
            {
                return;
            }

            Edit(() =>
            {
                if (erase)
                {
                    _draft.AddedSouth[storey].ExceptWith(south);
                    _draft.AddedEast[storey].ExceptWith(east);
                    _draft.ErasedSouth[storey].UnionWith(south);
                    _draft.ErasedEast[storey].UnionWith(east);
                }
                else
                {
                    _draft.ErasedSouth[storey].ExceptWith(south);
                    _draft.ErasedEast[storey].ExceptWith(east);
                    _draft.AddedSouth[storey].UnionWith(south);
                    _draft.AddedEast[storey].UnionWith(east);
                }
            });
        }

        /// <summary>
        /// Turns the wall in a cell into a window, or back. A window the draft put there by itself
        /// is set back to wall, and stays so. False when the cell has no straight wall.
        /// </summary>
        public bool ToggleWindow(int storey, int x, int y)
        {
            HouseDraft.StoreyLayout layout = _draft.LayOut(Wings, storey);
            if (layout.South.Contains((x, y)) == layout.East.Contains((x, y)))
            {
                return false;
            }

            Edit(() =>
            {
                if (_draft.Windows.Remove((storey, x, y)))
                {
                    return;
                }

                if (layout.Windows.Contains((x, y)))
                {
                    _draft.Suppressed.Add((storey, x, y));
                }
                else if (!_draft.Suppressed.Remove((storey, x, y)) || !_draft.LayOut(Wings, storey).Windows.Contains((x, y)))
                {
                    _draft.Windows.Add((storey, x, y));
                    _draft.Doors.Remove((storey, x, y));
                }
            });
            return true;
        }

        // Hinge variants per wall direction, in the order a click cycles through them.
        private static readonly string[] _doorsAlongX = { "Piece3", "Piece1", "Piece4", "Piece2" };
        private static readonly string[] _doorsAlongY = { "Piece5", "Piece7", "Piece6", "Piece8" };

        /// <summary>
        /// Hangs a door in the wall of a cell, or turns the door there to its next hinge variant;
        /// with <paramref name="remove"/> takes it out. False when the cell has no straight wall.
        /// </summary>
        public bool CycleDoor(int storey, int x, int y, HouseStyle style, bool remove)
        {
            HouseDraft.StoreyLayout layout = _draft.LayOut(Wings, storey);
            HashSet<(int X, int Y)> south = layout.South, east = layout.East;
            bool alongX = south.Contains((x, y));
            if (alongX == east.Contains((x, y)))
            {
                return false;
            }

            bool has = _draft.Doors.TryGetValue((storey, x, y), out (HouseStyle Style, string Piece) door);
            if (remove)
            {
                // A door the draft hung by itself is taken out for good: its doorway stays open.
                bool own = layout.Doors.ContainsKey((x, y));
                if (has || own)
                {
                    Edit(() =>
                    {
                        _draft.Doors.Remove((storey, x, y));
                        if (own)
                        {
                            _draft.Suppressed.Add((storey, x, y));
                        }
                    });
                }

                return has || own;
            }

            if (style == null || style.Kind != HousePartKind.Door)
            {
                return false;
            }

            string[] variants = alongX ? _doorsAlongX : _doorsAlongY;
            int next = has && door.Style == style ? (Array.IndexOf(variants, door.Piece) + 1) % variants.Length : 0;
            for (int i = 0; i < variants.Length; ++i)
            {
                string piece = variants[(next + i) % variants.Length];
                if (style.Get(piece) != 0)
                {
                    Edit(() =>
                    {
                        _draft.Doors[(storey, x, y)] = (style, piece);
                        _draft.Windows.Remove((storey, x, y));
                    });
                    return true;
                }
            }

            return false;
        }

        /// <summary>Lays an area of a storey's floor in another style.</summary>
        public void AddFloorArea(int storey, int x0, int y0, int x1, int y1, HouseStyle style)
        {
            if (style is { Kind: HousePartKind.Floor })
            {
                Edit(() => _draft.FloorAreas.Add((storey, Math.Min(x0, x1), Math.Min(y0, y1), Math.Max(x0, x1), Math.Max(y0, y1), style)));
            }
        }

        /// <summary>Puts stairs on a storey, their bottom step in a cell; they open the floor above.</summary>
        public void AddStairs(int storey, int x, int y, string column, HouseStyle style)
        {
            if (style is { Kind: HousePartKind.Stair } && style.Get(column) != 0)
            {
                Edit(() => _draft.Stairs.Add((storey, x, y, column, style)));
            }
        }

        /// <summary>Takes out the stairs whose bottom step is in a cell.</summary>
        public bool RemoveStairs(int storey, int x, int y)
        {
            int index = _draft.Stairs.FindLastIndex(s => s.Storey == storey && s.X == x && s.Y == y);
            if (index < 0)
            {
                return false;
            }

            Edit(() => _draft.Stairs.RemoveAt(index));
            return true;
        }
    }
}
