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
using Ultima;
using UoFiddler.Plugin.MultiEditor.Rendering;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>Where a tile sits in its cell, which decides how it moves when a group is turned.</summary>
    internal enum TilePlacement
    {
        /// <summary>Fills or stands in the cell (floors, roofs, furniture).</summary>
        Cell,

        /// <summary>A wall along the cell's +Y edge.</summary>
        SouthEdge,

        /// <summary>A wall along the cell's +X edge.</summary>
        EastEdge,

        /// <summary>A post at the cell's +X/+Y corner.</summary>
        Vertex,

        /// <summary>
        /// A roof piece. Its art is drawn half a cell up the screen, so it covers the area around
        /// the cell's -X/-Y corner: the official houses put their roofs one cell further +X/+Y
        /// than the walls for that reason.
        /// </summary>
        Lifted
    }

    /// <summary>
    /// What the art itself says about a tile's facing. Swapping the world's X and Y axes mirrors
    /// the screen left to right, so the tile that shows the same piece along the other axis is the
    /// one whose art is the mirror image: a south wall and its east wall, a roof slope and the
    /// slope a quarter turn on. That twin is looked for among the neighbouring ids. Nothing here
    /// is a table: it is measured from the client's art, so it follows whatever art is loaded.
    /// </summary>
    internal static class ArtFacing
    {
        private const int _searchRange = 48;
        private const double _sameShape = 0.90;
        private const double _twinShape = 0.80;

        private sealed class Shape
        {
            public int Width;
            public int Height;
            public byte[] Rgba;
            public int Left;
            public int Right;
            public int Opaque;
            public int MinX;
            public int MaxX;
            public int MaxY;
            public ulong Hash;
        }

        private static readonly Dictionary<ushort, Shape> _shapes = new Dictionary<ushort, Shape>();
        private static readonly Dictionary<ushort, ushort> _transposed = new Dictionary<ushort, ushort>();

        /// <summary>Forgets everything measured, for when the client files are loaded again.</summary>
        public static void Clear()
        {
            _shapes.Clear();
            _transposed.Clear();
        }

        /// <summary>A hash of the tile's art, 0 when it has none. Equal art gives an equal hash.</summary>
        public static ulong ArtHash(ushort id) => Load(id)?.Hash ?? 0;

        /// <summary>
        /// The tile showing the same piece with the X and Y axes swapped, or the id itself when the
        /// art is its own mirror image or no twin is found.
        /// </summary>
        public static ushort Transpose(ushort id)
        {
            if (_transposed.TryGetValue(id, out ushort known))
            {
                return known;
            }

            Shape shape = Load(id);
            if (shape == null || shape.Opaque == 0 || (TileData.ItemTable[id].Flags & TileFlag.Animation) != 0)
            {
                // No art, or frames of an animation: neighbouring ids are the next frames, not twins.
                return _transposed[id] = id;
            }

            if (Compare(shape, shape).Shape >= _sameShape)
            {
                ushort other = FindPictureTwin(id);
                if (other != id && FindPictureTwin(other) != id)
                {
                    other = id;
                }

                _transposed[id] = other;
                _transposed[other] = id;
                return other;
            }

            PairUp(id);
            return _transposed.TryGetValue(id, out known) ? known : _transposed[id] = id;
        }

        /// <summary>
        /// Where a tile sits. A wall-like tile is read from which half of its cell the art covers,
        /// a roof tile from the empty rows under its art; everything else is in the cell.
        /// </summary>
        public static TilePlacement Placement(ushort id)
        {
            if (id >= TileData.ItemTable.Length)
            {
                return TilePlacement.Cell;
            }

            TileFlag flags = TileData.ItemTable[id].Flags;
            Shape shape = Load(id);
            if (shape == null || shape.Opaque == 0)
            {
                return TilePlacement.Cell;
            }

            if ((flags & TileFlag.Roof) != 0)
            {
                // Half a cell is 22 pixels of empty rows under the art.
                return shape.Height - 1 - shape.MaxY >= 11 ? TilePlacement.Lifted : TilePlacement.Cell;
            }

            if ((flags & (TileFlag.Wall | TileFlag.Window)) == 0)
            {
                return TilePlacement.Cell;
            }

            // A wall on the +Y edge runs from the diamond's left corner to its bottom corner, so its
            // art stays left of the right quarter; a +X edge wall right of the left quarter; a post
            // is a column in the middle.
            int quarter = shape.Width / 4;
            bool reachesLeft = shape.MinX < quarter - 2;
            bool reachesRight = shape.MaxX > shape.Width - quarter + 1;
            if (!reachesLeft && !reachesRight)
            {
                return TilePlacement.Vertex;
            }

            if (reachesLeft && !reachesRight && shape.Left > shape.Right)
            {
                return TilePlacement.SouthEdge;
            }

            return reachesRight && !reachesLeft && shape.Right > shape.Left ? TilePlacement.EastEdge : TilePlacement.Cell;
        }

        /// <summary>
        /// Pairs the tiles around <paramref name="id"/> whose silhouettes mirror each other: every
        /// possible pair is scored and the best ones are taken first, so three near-equal wall
        /// pieces and their three twins pair off the same way whichever is asked for first.
        /// </summary>
        private static void PairUp(ushort id)
        {
            var group = new List<ushort> { id };
            var seen = new HashSet<ushort> { id };
            var pairs = new List<(ushort A, ushort B, double Score)>();
            for (int i = 0; i < group.Count && group.Count < 96; i++)
            {
                ushort a = group[i];
                foreach ((ushort b, double score) in Candidates(a))
                {
                    pairs.Add((a, b, score));
                    if (seen.Add(b))
                    {
                        group.Add(b);
                    }
                }
            }

            pairs.Sort((x, y) => y.Score != x.Score ? y.Score.CompareTo(x.Score) : (x.A + x.B).CompareTo(y.A + y.B));
            var taken = new HashSet<ushort>();
            foreach (var pair in pairs)
            {
                if (taken.Contains(pair.A) || taken.Contains(pair.B) || _transposed.ContainsKey(pair.A) || _transposed.ContainsKey(pair.B))
                {
                    continue;
                }

                taken.Add(pair.A);
                taken.Add(pair.B);
                _transposed[pair.A] = pair.B;
                _transposed[pair.B] = pair.A;
            }

            foreach (ushort member in group)
            {
                _transposed.TryAdd(member, member);
            }
        }

        /// <summary>The neighbouring tiles whose art could be the mirror image of this one, with a score.</summary>
        private static IEnumerable<(ushort Id, double Score)> Candidates(ushort id)
        {
            Shape shape = Load(id);
            if (shape == null || shape.Opaque == 0)
            {
                yield break;
            }

            ItemData data = TileData.ItemTable[id];
            int from = Math.Max(0, id - _searchRange);
            int to = Math.Min(TileData.ItemTable.Length - 1, id + _searchRange);
            for (int other = from; other <= to; other++)
            {
                if (other == id)
                {
                    continue;
                }

                ItemData otherData = TileData.ItemTable[other];
                if (otherData.Height != data.Height || !SameKind(otherData.Flags, data.Flags) || !SameSet(otherData.Name, data.Name) ||
                    (otherData.Flags & TileFlag.Animation) != 0)
                {
                    continue;
                }

                Shape candidate = Load((ushort)other);
                if (candidate == null || candidate.Opaque == 0 ||
                    Math.Abs(candidate.Width - shape.Width) > 4 || Math.Abs(candidate.Height - shape.Height) > 4 ||
                    Compare(candidate, candidate).Shape >= _sameShape)
                {
                    continue;
                }

                var score = Compare(shape, candidate);
                if (score.Shape >= _twinShape)
                {
                    // Silhouette first, then colour, then how close the ids are.
                    yield return ((ushort)other, score.Shape - (score.Colour / 400.0) - (Math.Abs(other - id) / 2000.0));
                }
            }
        }

        /// <summary>
        /// For a tile whose silhouette is its own mirror image (a floor): the neighbour with the same
        /// silhouette whose picture is clearly the mirrored one, such as planks running the other way.
        /// </summary>
        private static ushort FindPictureTwin(ushort id)
        {
            Shape shape = Load(id);
            double own = Compare(shape, shape).Colour;
            if (own <= 12)
            {
                return id;
            }

            ItemData data = TileData.ItemTable[id];
            ushort best = id;
            double bestColour = own * 0.6;
            int from = Math.Max(0, id - _searchRange);
            int to = Math.Min(TileData.ItemTable.Length - 1, id + _searchRange);
            for (int other = from; other <= to; other++)
            {
                ItemData otherData = TileData.ItemTable[other];
                Shape candidate = other == id || otherData.Height != data.Height || !SameKind(otherData.Flags, data.Flags) ? null : Load((ushort)other);
                if (candidate == null || candidate.Width != shape.Width || candidate.Height != shape.Height)
                {
                    continue;
                }

                var score = Compare(shape, candidate);
                if (score.Shape >= 0.98 && score.Colour < bestColour)
                {
                    best = (ushort)other;
                    bestColour = score.Colour;
                }
            }

            return best;
        }

        /// <summary>
        /// Tiledata names of one set: equal, or one is the other with a letter more ("shingle",
        /// "shingles"). A twin is always of the same set: a tile roof never turns into shingles.
        /// </summary>
        private static bool SameSet(string a, string b)
        {
            a ??= string.Empty;
            b ??= string.Empty;
            return Math.Abs(a.Length - b.Length) <= 1 &&
                   (a.StartsWith(b, StringComparison.OrdinalIgnoreCase) || b.StartsWith(a, StringComparison.OrdinalIgnoreCase));
        }

        private static bool SameKind(TileFlag a, TileFlag b)
        {
            const TileFlag kind = TileFlag.Wall | TileFlag.Roof | TileFlag.Door | TileFlag.Window | TileFlag.Surface | TileFlag.Bridge;
            return (a & kind) == (b & kind);
        }

        /// <summary>
        /// How well <paramref name="a"/> matches the mirror image of <paramref name="b"/>, both
        /// standing on the same cell: shared part of the silhouettes, and the colour difference there.
        /// </summary>
        private static (double Shape, double Colour) Compare(Shape a, Shape b)
        {
            int width = Math.Max(a.Width, b.Width) + 2;
            int height = Math.Max(a.Height, b.Height);
            int offsetA = (width / 2) - (a.Width / 2);
            int offsetB = (width / 2) - (b.Width / 2);
            int inter = 0, union = 0;
            long colour = 0;
            for (int y = 0; y < height; y++)
            {
                int ay = y - (height - a.Height);
                int by = y - (height - b.Height);
                for (int x = 0; x < width; x++)
                {
                    int ax = x - offsetA;
                    int bx = b.Width - 1 - (x - offsetB);
                    int ai = ay >= 0 && ax >= 0 && ax < a.Width ? ((ay * a.Width) + ax) * 4 : -1;
                    int bi = by >= 0 && bx >= 0 && bx < b.Width ? ((by * b.Width) + bx) * 4 : -1;
                    bool hasA = ai >= 0 && a.Rgba[ai + 3] != 0;
                    bool hasB = bi >= 0 && b.Rgba[bi + 3] != 0;
                    if (hasA || hasB)
                    {
                        union++;
                    }

                    if (hasA && hasB)
                    {
                        inter++;
                        colour += Math.Abs(a.Rgba[ai] - b.Rgba[bi]) + Math.Abs(a.Rgba[ai + 1] - b.Rgba[bi + 1]) + Math.Abs(a.Rgba[ai + 2] - b.Rgba[bi + 2]);
                    }
                }
            }

            return (union == 0 ? 0 : (double)inter / union, inter == 0 ? 255 : colour / (3.0 * inter));
        }

        private static Shape Load(ushort id)
        {
            if (_shapes.TryGetValue(id, out Shape shape))
            {
                return shape;
            }

            int width = 0, height = 0;
            byte[] rgba = Art.IsValidStatic(id) ? ArtPixels.LoadStatic(id, out width, out height) : null;
            if (rgba == null)
            {
                _shapes[id] = null;
                return null;
            }

            shape = new Shape { Width = width, Height = height, Rgba = rgba, MinX = width, MaxX = -1 };
            int centre = width / 2;
            ulong hash = 14695981039346656037UL ^ (ulong)((width << 16) | height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = ((y * width) + x) * 4;
                    bool opaque = rgba[i + 3] != 0;
                    uint pixel = opaque ? (uint)((rgba[i] << 16) | (rgba[i + 1] << 8) | rgba[i + 2]) | 0xFF000000 : 0;
                    hash = (hash ^ pixel) * 1099511628211UL;
                    if (!opaque)
                    {
                        continue;
                    }

                    shape.Opaque++;
                    shape.MinX = Math.Min(shape.MinX, x);
                    shape.MaxX = Math.Max(shape.MaxX, x);
                    shape.MaxY = y;
                    if (x < centre)
                    {
                        shape.Left++;
                    }
                    else
                    {
                        shape.Right++;
                    }
                }
            }

            shape.Hash = hash == 0 ? 1 : hash;
            _shapes[id] = shape;
            return shape;
        }
    }
}
