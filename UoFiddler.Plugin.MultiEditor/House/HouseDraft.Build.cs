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
