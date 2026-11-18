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
    /// <summary>The shape of one roof wing.</summary>
    internal enum RoofShape
    {
        /// <summary>Two slopes meeting in a ridge.</summary>
        Gable,

        /// <summary>Four slopes: the ends slope too, instead of standing as gables.</summary>
        Hip,

        /// <summary>
        /// No slopes: a floor on top of the walls, as the towers have. Only a house wing can have
        /// it; the house draft lays it, not this planner.
        /// </summary>
        Flat
    }

    /// <summary>
    /// Roofs as a height per cell. A wing (a rectangle with a ridge direction) gives each of its
    /// cells a height, wings are joined by taking the higher one, and the roof.txt piece of a cell
    /// follows from how its neighbours' heights compare with its own. The rules were measured on
    /// the official multis and the UO Architect designs (109,000 roof cells):
    /// <list type="bullet">
    /// <item>a slope is named for the side it comes down to: lower to the west is a West piece;</item>
    /// <item>a ridge cell (lower on both sides) is the crosspiece running the other way;</item>
    /// <item>a valley cell, level with two neighbours and below the other two, is a Dent piece:
    /// higher to the west and south EDent, west and north SDent, east and north WDent, east and
    /// south NDent;</item>
    /// <item>where a wing's ridge runs into another ridge of the same height the cell is the T
    /// piece named for the side the wing comes from, and the X piece where two ridges cross.</item>
    /// <item>roof.txt has no pieces made for hips. The Dent pieces serve: each is a tile of two
    /// faces, and on a hip line the one with the same two faces closes the corner when it sits 3
    /// lower than the slopes beside it (the south-east one at their height). Found by drawing
    /// every piece at every corner; it closes cleanly in thatch, stone, slate and shingles and
    /// leaves small seams in Tile Roof.</item>
    /// </list>
    /// </summary>
    internal static class RoofPlanner
    {
        /// <summary>Z a slope gains per cell: the roof pieces are 3 high.</summary>
        public const int Rise = 3;

        /// <summary>Marks a fixed piece that sits lower than its cell's height: "EDent@-3".</summary>
        public const char LiftMark = '@';

        // The corner pieces of a hip, by the corner of the wing they lie towards.
        private const string _hipNorthWest = "NDent@-3";
        private const string _hipNorthEast = "EDent@-3";
        private const string _hipSouthEast = "SDent";
        private const string _hipSouthWest = "WDent@-3";

        /// <summary>Narrowest part of a house that gets a wing of its own.</summary>
        private const int _minWing = 3;

        /// <summary>
        /// The ridge direction the official houses would give a box: across the odd side, so the
        /// ridge gets its crosspiece; otherwise along the longer side, along Y when square.
        /// </summary>
        public static bool RidgeAlongY((int X0, int Y0, int X1, int Y1) box)
        {
            int spanX = box.X1 - box.X0 + 1, spanY = box.Y1 - box.Y0 + 1;
            return spanX % 2 != spanY % 2 ? spanX % 2 == 1 : spanY >= spanX;
        }

        /// <summary>
        /// Adds a wing to a roof: every cell of the box gets the wing's height there unless the
        /// roof is already higher. A gable climbs from its two long sides to the ridge. A hip
        /// climbs from all four sides; the cells on its hip lines get their corner piece in
        /// <paramref name="fixedPieces"/>. Cells with a negative coordinate are left out.
        /// </summary>
        public static void AddWing(Dictionary<(int X, int Y), int> heights, Dictionary<(int X, int Y), string> fixedPieces,
            (int X0, int Y0, int X1, int Y1) box, bool ridgeAlongY, RoofShape shape, int eaveZ)
        {
            int across0 = ridgeAlongY ? box.X0 : box.Y0;
            int across1 = ridgeAlongY ? box.X1 : box.Y1;
            for (int x = Math.Max(0, box.X0); x <= box.X1; ++x)
            {
                for (int y = Math.Max(0, box.Y0); y <= box.Y1; ++y)
                {
                    int a = ridgeAlongY ? x : y;
                    int steps = Math.Min(a - across0, across1 - a);
                    string corner = null;
                    if (shape == RoofShape.Hip)
                    {
                        int west = x - box.X0, east = box.X1 - x, north = y - box.Y0, south = box.Y1 - y;
                        steps = Math.Min(Math.Min(west, east), Math.Min(north, south));

                        // On a hip line two sides are equally near; on the ridge the opposite ones are.
                        if (west != east && north != south)
                        {
                            corner = west == steps && north == steps ? _hipNorthWest
                                : east == steps && north == steps ? _hipNorthEast
                                : east == steps && south == steps ? _hipSouthEast
                                : west == steps && south == steps ? _hipSouthWest
                                : null;
                        }
                    }

                    int z = eaveZ + (Rise * steps);
                    if (heights.TryGetValue((x, y), out int known) && known >= z)
                    {
                        continue;
                    }

                    heights[(x, y)] = z;
                    if (corner != null)
                    {
                        fixedPieces[(x, y)] = corner;
                    }
                    else
                    {
                        fixedPieces.Remove((x, y));
                    }
                }
            }
        }

        /// <summary>
        /// The roof.txt column of the piece for a cell, from the heights around it; null for a
        /// cell level with all its neighbours. <paramref name="fallback"/> is the plain piece to
        /// use when the style has no such joint piece.
        /// </summary>
        public static string Piece(IReadOnlyDictionary<(int X, int Y), int> heights, int x, int y, out string fallback)
        {
            fallback = null;
            if (!heights.TryGetValue((x, y), out int h))
            {
                return null;
            }

            // -1 lower (or no roof there), 0 level, 1 higher.
            int Side(int cx, int cy, int ch) => heights.TryGetValue((cx, cy), out int n) ? Math.Sign(n - ch) : -1;
            bool RidgeAlongYAt(int cx, int cy) => heights.TryGetValue((cx, cy), out int ch) && Side(cx - 1, cy, ch) < 0 && Side(cx + 1, cy, ch) < 0;
            bool RidgeAlongXAt(int cx, int cy) => heights.TryGetValue((cx, cy), out int ch) && Side(cx, cy - 1, ch) < 0 && Side(cx, cy + 1, ch) < 0;

            int w = Side(x - 1, y, h), e = Side(x + 1, y, h), n = Side(x, y - 1, h), s = Side(x, y + 1, h);

            // Valley: level with one neighbour and below the other on both axes.
            if (w + e == 1 && w * e == 0 && n + s == 1 && n * s == 0)
            {
                if (w > 0)
                {
                    fallback = s > 0 ? "East" : "South";
                    return s > 0 ? "EDent" : "SDent";
                }

                fallback = n > 0 ? "West" : "North";
                return n > 0 ? "WDent" : "NDent";
            }

            // Ridges of this height leaving the cell.
            bool armW = w == 0 && RidgeAlongXAt(x - 1, y), armE = e == 0 && RidgeAlongXAt(x + 1, y);
            bool armN = n == 0 && RidgeAlongYAt(x, y - 1), armS = s == 0 && RidgeAlongYAt(x, y + 1);
            int arms = (armW ? 1 : 0) + (armE ? 1 : 0) + (armN ? 1 : 0) + (armS ? 1 : 0);
            if (arms == 4)
            {
                fallback = "NSCrosspiece";
                return "XPiece";
            }

            if (arms == 3)
            {
                fallback = armN && armS ? "NSCrosspiece" : "EWCrosspiece";
                return !armW ? "ETPiece" : !armE ? "WTPiece" : !armN ? "STPiece" : "NTPiece";
            }

            bool ridgeY = w < 0 && e < 0, ridgeX = n < 0 && s < 0;
            if (ridgeY && ridgeX)
            {
                return armE || armW ? "EWCrosspiece" : "NSCrosspiece";
            }

            if (ridgeY)
            {
                return "NSCrosspiece";
            }

            if (ridgeX)
            {
                return "EWCrosspiece";
            }

            int climbX = e - w, climbY = s - n;
            if (climbX == 0 && climbY == 0)
            {
                return null;
            }

            // The roof's edge counts as lower, so at a gable end a cell of the top rows of an even
            // span drops as much to the edge as down its slope. The slope is the side where roof
            // lies lower, not the edge.
            if (Math.Abs(climbX) == Math.Abs(climbY))
            {
                bool roofLowerX = heights.ContainsKey((climbX > 0 ? x - 1 : x + 1, y));
                bool roofLowerY = heights.ContainsKey((x, climbY > 0 ? y - 1 : y + 1));
                if (roofLowerY && !roofLowerX)
                {
                    return climbY > 0 ? "North" : "South";
                }
            }

            return Math.Abs(climbX) >= Math.Abs(climbY) ? (climbX > 0 ? "West" : "East") : (climbY > 0 ? "North" : "South");
        }

        /// <summary>
        /// The wings that roof a house whose cells are <paramref name="region"/>: its largest
        /// rectangles, biggest first, each with its ridge along its longer side. A wing that runs
        /// through an earlier one across its ridge stops at that ridge, so the two meet in a T
        /// instead of the wing showing a gable on the far side. Parts narrower than three cells
        /// get no wing. A plain rectangle, or a shape too ragged to split, is one wing: the
        /// region's box.
        /// </summary>
        public static List<((int X0, int Y0, int X1, int Y1) Box, bool RidgeAlongY)> Wings(ICollection<(int X, int Y)> region)
        {
            var wings = new List<((int X0, int Y0, int X1, int Y1) Box, bool RidgeAlongY)>();
            if (region.Count == 0)
            {
                return wings;
            }

            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach ((int x, int y) in region)
            {
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }

            var whole = (minX, minY, maxX, maxY);
            int width = maxX - minX + 1, height = maxY - minY + 1;
            if (region.Count == width * height)
            {
                wings.Add((whole, RidgeAlongY(whole)));
                return wings;
            }

            // Filled cells before (x, y), so a rectangle is checked in one step.
            var sum = new int[width + 1, height + 1];
            var set = region as ISet<(int X, int Y)> ?? new HashSet<(int X, int Y)>(region);
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    sum[x + 1, y + 1] = sum[x, y + 1] + sum[x + 1, y] - sum[x, y] + (set.Contains((minX + x, minY + y)) ? 1 : 0);
                }
            }

            bool Full(int x0, int y0, int x1, int y1)
            {
                return x0 >= 0 && y0 >= 0 && x1 < width && y1 < height &&
                       sum[x1 + 1, y1 + 1] - sum[x0, y1 + 1] - sum[x1 + 1, y0] + sum[x0, y0] == (x1 - x0 + 1) * (y1 - y0 + 1);
            }

            var found = new List<(int X0, int Y0, int X1, int Y1)>();
            for (int x0 = 0; x0 < width; ++x0)
            {
                for (int y0 = 0; y0 < height; ++y0)
                {
                    for (int x1 = x0 + _minWing - 1; x1 < width && Full(x0, y0, x1, y0); ++x1)
                    {
                        for (int y1 = y0 + _minWing - 1; y1 < height && Full(x0, y0, x1, y1); ++y1)
                        {
                            if (!Full(x0 - 1, y0, x0 - 1, y1) && !Full(x1 + 1, y0, x1 + 1, y1) &&
                                !Full(x0, y0 - 1, x1, y0 - 1) && !Full(x0, y1 + 1, x1, y1 + 1))
                            {
                                found.Add((minX + x0, minY + y0, minX + x1, minY + y1));
                            }
                        }
                    }
                }
            }

            found.Sort((a, b) => ((b.X1 - b.X0 + 1) * (b.Y1 - b.Y0 + 1)).CompareTo((a.X1 - a.X0 + 1) * (a.Y1 - a.Y0 + 1)));
            if (found.Count == 0 || found.Count > 6)
            {
                wings.Add((whole, RidgeAlongY(whole)));
                return wings;
            }

            foreach ((int X0, int Y0, int X1, int Y1) rect in found)
            {
                var box = rect;
                bool alongY = box.Y1 - box.Y0 != box.X1 - box.X0 ? box.Y1 - box.Y0 > box.X1 - box.X0 : RidgeAlongY(box);
                foreach (((int X0, int Y0, int X1, int Y1) other, bool otherAlongY) in wings)
                {
                    bool overlap = box.X0 <= other.X1 && box.X1 >= other.X0 && box.Y0 <= other.Y1 && box.Y1 >= other.Y0;
                    if (!overlap || otherAlongY == alongY)
                    {
                        continue;
                    }

                    if (alongY)
                    {
                        // The other ridge runs along X: stop at its row when sticking out one way only.
                        bool south = box.Y1 > other.Y1, north = box.Y0 < other.Y0;
                        if (south && !north)
                        {
                            box.Y0 = Math.Max(box.Y0, (other.Y0 + other.Y1 + 1) / 2);
                        }
                        else if (north && !south)
                        {
                            box.Y1 = Math.Min(box.Y1, (other.Y0 + other.Y1) / 2);
                        }
                    }
                    else
                    {
                        bool east = box.X1 > other.X1, west = box.X0 < other.X0;
                        if (east && !west)
                        {
                            box.X0 = Math.Max(box.X0, (other.X0 + other.X1 + 1) / 2);
                        }
                        else if (west && !east)
                        {
                            box.X1 = Math.Min(box.X1, (other.X0 + other.X1) / 2);
                        }
                    }
                }

                wings.Add((box, alongY));
            }

            return wings;
        }
    }
}
