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
using UoFiddler.Controls.Classes.TilesInfo;
using UoFiddler.Plugin.MultiEditor.Rendering;

namespace UoFiddler.Plugin.MultiEditor.Styles
{
    internal sealed partial class HouseStyles
    {
        /// <summary>The pieces a row lists under its window headings.</summary>
        private static HashSet<ushort> WindowPieces(TileStyle row)
        {
            var ids = new HashSet<ushort>();
            foreach (TileSlot slot in row.Slots)
            {
                if (!slot.IsEmpty && slot.IsValid && slot.TileId <= ushort.MaxValue && slot.Label.Contains("Window", System.StringComparison.Ordinal))
                {
                    ids.Add((ushort)slot.TileId);
                }
            }

            return ids;
        }

        /// <summary>The pieces a row lists, each once, in the row's order.</summary>
        private static List<ushort> PiecesOf(TileStyle row)
        {
            var ids = new List<ushort>();
            foreach (TileSlot slot in row.Slots)
            {
                if (!slot.IsEmpty && slot.IsValid && HouseStyle.InTileData(slot.TileId) && !ids.Contains((ushort)slot.TileId))
                {
                    ids.Add((ushort)slot.TileId);
                }
            }

            return ids;
        }

        private static readonly string[] _southWindows = { "WindowS", "AltWindowS", "SecondAltWindowS" };
        private static readonly string[] _eastWindows = { "WindowE", "AltWindowE", "SecondAltWindowE" };

        /// <summary>What a wall column's heading says its piece is.</summary>
        private static TilePlacement HeadingPlacement(string label)
        {
            return label == "Corner" ? TilePlacement.Cell
                : label == "Post" ? TilePlacement.Vertex
                : label.StartsWith("South", System.StringComparison.Ordinal) || label.EndsWith("S", System.StringComparison.Ordinal) ? TilePlacement.SouthEdge
                : TilePlacement.EastEdge;
        }

        /// <summary>
        /// True for a walls.txt category where at least three pieces in ten are not what their
        /// column heading says, going by the art: a piece under "South" that runs along Y, a
        /// "Corner" that is a straight wall. All the gargish sets and the gothic one are like
        /// that; in the other categories it is at most a spare window under "Corner".
        /// </summary>
        private static bool HeadingsAreOff(TileCategory category)
        {
            int pieces = 0, off = 0;
            foreach (TileStyle row in category.Styles)
            {
                foreach (TileSlot slot in row.Slots)
                {
                    if (slot.IsEmpty || !slot.IsValid || slot.TileId > ushort.MaxValue)
                    {
                        continue;
                    }

                    ++pieces;
                    if (ArtFacing.Placement((ushort)slot.TileId) != HeadingPlacement(slot.Label))
                    {
                        ++off;
                    }
                }
            }

            return pieces > 0 && off * 10 >= pieces * 3;
        }

        /// <summary>
        /// Fills a wall style from a row whose headings cannot be trusted. Each piece goes where
        /// its art says: a wall along X to the South columns, one along Y to the East columns, a
        /// piece that fills its cell's corner to Corner, a column to Post, windows to the window
        /// columns. The pieces keep the row's order. What does not fit (a fourth wall along X, a
        /// second corner, a piece of another height than most of the row) stays with the style as
        /// a "More" piece, to place by hand.
        /// </summary>
        private static void SortByArt(HouseStyle style, List<ushort> ids, HashSet<ushort> listedAsWindows)
        {
            if (ids.Count == 0)
            {
                return;
            }

            // The row's usual height: full, half or low walls.
            static int Class(ushort id) => Ultima.TileData.ItemTable[id].Height >= 15 ? 2 : Ultima.TileData.ItemTable[id].Height >= 8 ? 1 : 0;
            int usual = ids.GroupBy(Class).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).First().Key;

            int more = 0;
            void Put(string[] columns, ushort id)
            {
                foreach (string column in columns)
                {
                    if (style.Get(column) == 0)
                    {
                        style.Add(column, id);
                        return;
                    }
                }

                style.Add($"More{++more}", id);
            }

            foreach (ushort id in ids)
            {
                // An arch or opening the table lists under a window heading is one, flagged or not.
                bool window = (Ultima.TileData.ItemTable[id].Flags & Ultima.TileFlag.Window) != 0 || listedAsWindows.Contains(id);
                if (Class(id) != usual)
                {
                    style.Add($"More{++more}", id);
                    continue;
                }

                switch (ArtFacing.Placement(id))
                {
                    case TilePlacement.SouthEdge:
                        Put(window ? _southWindows : _southRun, id);
                        break;
                    case TilePlacement.EastEdge:
                        Put(window ? _eastWindows : _eastRun, id);
                        break;
                    case TilePlacement.Vertex:
                        Put(new[] { "Post" }, id);
                        break;
                    default:
                        Put(new[] { "Corner" }, id);
                        break;
                }
            }
        }

        private static readonly string[] _southRun = { "South1", "South2", "South3" };
        private static readonly string[] _eastRun = { "East1", "East2", "East3" };

        /// <summary>
        /// Many wall rows list one piece per direction where the table has three columns (the
        /// start, middle and end of a run): the later sets, and most half and quarter walls. A
        /// run built from such a row had holes. The columns a row leaves empty take the piece of
        /// the nearest one it has, so every row builds whole walls. (The piece keeps the one role
        /// it is listed under.)
        /// </summary>
        private static void FillWallRuns(HouseStyle style, ushort south = 0, ushort east = 0)
        {
            foreach (string[] run in new[] { _southRun, _eastRun })
            {
                // The middle piece first: it stands in best for a missing end, and an end for a missing middle.
                ushort have = style.Get(run[1]) != 0 ? style.Get(run[1]) : style.Get(run[0]) != 0 ? style.Get(run[0]) : style.Get(run[2]);
                if (have == 0)
                {
                    have = run == _southRun ? south : east;
                }
                if (have == 0)
                {
                    continue;
                }

                foreach (string column in run)
                {
                    if (style.Get(column) == 0)
                    {
                        style.Add(column, have);
                    }
                }
            }
        }

        private static readonly string[] _wallFillerWords = { "Standard", "Walls", "Wall", "Set", "Style", "-" };

        /// <summary>
        /// The group and the row names of a walls.txt category. The table's comments name many
        /// rows alike ("Carved Wood 1" four times: full, windowed, half and quarter walls) or only
        /// by number ("Crystal Walls 2"). The group is the material: what the rows' names share,
        /// without filler words and numbers. A row whose name is used twice, or is only numbered,
        /// is named by what it holds instead: material + Walls, Window Walls, Half Walls or
        /// Quarter Walls, by the height of its pieces. Other rows keep their comment.
        /// </summary>
        private static (string Group, List<string> Names) WallNames(TileCategory category, HashSet<string> used, bool byArt)
        {
            var raw = new List<string>();
            foreach (TileStyle row in category.Styles)
            {
                string name = (row.Name ?? string.Empty).Trim().TrimEnd('\\').Trim();
                raw.Add(name);
            }

            // The material: the words every row starts with.
            string[] first = raw.Count > 0 ? raw[0].Split(' ', System.StringSplitOptions.RemoveEmptyEntries) : System.Array.Empty<string>();
            int shared = first.Length;
            foreach (string name in raw)
            {
                string[] words = name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                int same = 0;
                while (same < shared && same < words.Length && string.Equals(words[same], first[same], System.StringComparison.OrdinalIgnoreCase))
                {
                    ++same;
                }

                shared = same;
            }

            // A category of two materials (Weathered Stone and Granite) shares no words: it goes by its first row.
            bool oneMaterial = shared > 0;
            var material = new List<string>(first.Take(oneMaterial ? shared : first.Length));

            // "Gargish Wall Set 2": the set's number is part of what it is, as there are three such categories.
            string set = material.Count >= 3 && material[^2] == "Set" && int.TryParse(material[^1], out _) ? " Set " + material[^1] : string.Empty;
            while (material.Count > 0 && (System.Array.IndexOf(_wallFillerWords, material[^1]) >= 0 || int.TryParse(material[^1], out _)))
            {
                material.RemoveAt(material.Count - 1);
            }

            string group = material.Count > 0 ? string.Join(" ", material) + set : (category.Name ?? string.Empty).Trim();
            if (group.Length > 0)
            {
                group = char.ToUpperInvariant(group[0]) + group.Substring(1);
            }

            var names = new List<string>();
            for (int row = 0; row < raw.Count; ++row)
            {
                string name = raw[row];
                string[] words = name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                // In a category sorted by its art the comments say little: any numbered row of the material is renamed.
                bool numbered = words.Length >= 2 && int.TryParse(words[^1], out _) &&
                                (words[^2] is "Walls" or "Wall" or "Set" || raw.Count(n => n == name) > 1 ||
                                 (byArt && material.Count > 0 && name.StartsWith(string.Join(" ", material), System.StringComparison.OrdinalIgnoreCase)));
                bool vague = raw.Count(n => string.Equals(n, name, System.StringComparison.OrdinalIgnoreCase)) > 1 || numbered ||
                             name.Contains(" - Style", System.StringComparison.Ordinal);
                if (vague && oneMaterial && material.Count > 0)
                {
                    string kindName = WallKind(category.Styles[row]);
                    name = kindName == "Window Walls" && group.Contains("Window", System.StringComparison.OrdinalIgnoreCase) ? group : group + " " + kindName;
                }

                // Two rows of one kind: the second and later ones are numbered.
                string unique = name;
                for (int n = 2; !used.Add(unique); ++n)
                {
                    unique = $"{name} {n}";
                }

                names.Add(unique);
            }

            return (group, names);
        }

        /// <summary>What a wall row holds, by its pieces: their usual height, and whether most are windows.</summary>
        private static string WallKind(TileStyle row)
        {
            var heights = new List<int>();
            int windows = 0;
            foreach (TileSlot slot in row.Slots)
            {
                if (slot.IsEmpty || slot.TileId > ushort.MaxValue || !slot.IsValid)
                {
                    continue;
                }

                heights.Add(Ultima.TileData.ItemTable[slot.TileId].Height);
                if ((Ultima.TileData.ItemTable[slot.TileId].Flags & Ultima.TileFlag.Window) != 0)
                {
                    ++windows;
                }
            }

            if (heights.Count == 0)
            {
                return "Walls";
            }

            heights.Sort();
            int height = heights[heights.Count / 2];
            return height >= 15 ? (windows * 2 > heights.Count ? "Window Walls" : "Walls") : height >= 8 ? "Half Walls" : "Quarter Walls";
        }

        /// <summary>
        /// One style from the rows of a roof.txt category, named after the category without its
        /// row number ("Straw Roof 1" and "Straw Roof 2" are "Straw Roof"). The first row's pieces
        /// keep their column headings; the pieces of a further row are extras of the set, kept
        /// under "More" + heading so they still count as its roof pieces. Sets whose rows do not
        /// follow the headings (ceramic, crystal, shadow) get their pieces from the style file,
        /// which has labels for what the headings have no word for: Eave + side (a slope with
        /// the roof's edge), Hip + corner (a hip line), HipEave + corner, NSEnd and EWEnd.
        /// </summary>
        private static HouseStyle RoofStyle(TileCategory category)
        {
            string name = category.Name ?? string.Empty;
            if (name.EndsWith(" 1", System.StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - 2);
            }

            var style = new HouseStyle(HousePartKind.Roof, name, name);
            for (int row = 0; row < category.Styles.Count; ++row)
            {
                foreach (TileSlot slot in category.Styles[row].Slots)
                {
                    if (slot.IsEmpty || slot.TileId > ushort.MaxValue || !slot.IsValid)
                    {
                        continue;
                    }

                    string label = row == 0 ? slot.Label : "More" + slot.Label;
                    if (style.Get(label) == 0)
                    {
                        style.Add(label, (ushort)slot.TileId);
                    }
                }
            }

            return style;
        }

        /// <summary>How far apart two radar colours may be (RGB distance) and still count as one floor colour.</summary>
        private const int _sameColour = 20;

        /// <summary>
        /// Splits a floor row into styles of one colour each, by the tiles' radar colours. Pieces keep
        /// their column labels; a tile listed twice counts once. Each part is named after its tiles'
        /// tiledata name, numbered where a row has several of the same, and grouped under the row's name.
        /// </summary>
        private static List<HouseStyle> SplitByColour(HouseStyle style)
        {
            var clusters = new List<(System.Drawing.Color Colour, List<KeyValuePair<string, ushort>> Pieces)>();
            var seen = new HashSet<ushort>();
            foreach (KeyValuePair<string, ushort> piece in style.Slots)
            {
                if (!seen.Add(piece.Value))
                {
                    continue;
                }

                System.Drawing.Color colour = Ultima.Helpers.HueHelpers.HueToColor(Ultima.RadarCol.GetItemColor(piece.Value));
                int index = clusters.FindIndex(c =>
                {
                    int dr = c.Colour.R - colour.R, dg = c.Colour.G - colour.G, db = c.Colour.B - colour.B;
                    return (dr * dr) + (dg * dg) + (db * db) <= _sameColour * _sameColour;
                });
                if (index < 0)
                {
                    clusters.Add((colour, new List<KeyValuePair<string, ushort>> { piece }));
                }
                else
                {
                    clusters[index].Pieces.Add(piece);
                }
            }

            if (clusters.Count <= 1)
            {
                return new List<HouseStyle> { style };
            }

            var names = new List<string>();
            foreach (var (_, pieces) in clusters)
            {
                string name = Ultima.TileData.ItemTable[pieces[0].Value].Name?.Trim();
                if (string.IsNullOrEmpty(name) || name.StartsWith("Missing", System.StringComparison.OrdinalIgnoreCase))
                {
                    name = style.Name;
                }

                names.Add(char.ToUpperInvariant(name[0]) + name.Substring(1));
            }

            var result = new List<HouseStyle>();
            for (int i = 0; i < clusters.Count; i++)
            {
                string name = names[i];
                if (names.FindAll(n => n == name).Count > 1)
                {
                    name += " " + (names.GetRange(0, i + 1).FindAll(n => n == names[i]).Count);
                }

                var part = new HouseStyle(style.Kind, style.Name, name);
                foreach (KeyValuePair<string, ushort> piece in clusters[i].Pieces)
                {
                    part.Add(piece.Key, piece.Value);
                }

                result.Add(part);
            }

            return result;
        }
    }
}
