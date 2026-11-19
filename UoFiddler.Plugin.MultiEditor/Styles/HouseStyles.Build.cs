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
        internal static HouseStyles Load(bool splitFloorsByColour, HouseStyleConfig config)
        {
            // New tables mean the client files may have changed: measure the art again.
            ArtFacing.Clear();

            var result = new HouseStyles { _splitFloors = splitFloorsByColour };
            var wallNamesUsed = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            List<TileGroupFile> files = TileGroupRegistry.LoadAll(out List<string> errors);
            int categoryNumber = 0;
            foreach (TileGroupFile file in files)
            {
                if (!_kinds.TryGetValue(file.DisplayName, out HousePartKind kind))
                {
                    continue;
                }

                foreach (TileCategory category in file.Categories)
                {
                    int number = categoryNumber++;

                    // A roof set with more pieces than a row of roof.txt holds runs on into a second
                    // row of its category: the rows are one style.
                    if (kind == HousePartKind.Roof)
                    {
                        HouseStyle roof = RoofStyle(category);
                        if (roof.Slots.Count > 0)
                        {
                            roof.Category = number;
                            result.Styles.Add(roof);
                        }

                        continue;
                    }

                    // A category whose pieces are mostly not what their headings say is sorted by its art.
                    bool byArt = kind == HousePartKind.Wall && HeadingsAreOff(category);

                    // Wall rows are named so that each row of a category can be told from the others.
                    (string wallGroup, List<string> wallNames) = kind == HousePartKind.Wall ? WallNames(category, wallNamesUsed, byArt) : (null, null);
                    for (int row = 0; row < category.Styles.Count; ++row)
                    {
                        TileStyle tileStyle = category.Styles[row];
                        var style = new HouseStyle(kind, wallGroup ?? category.Name, wallNames != null ? wallNames[row] : tileStyle.Name);
                        if (byArt)
                        {
                            SortByArt(style, PiecesOf(tileStyle), WindowPieces(tileStyle));
                        }
                        else
                        {
                            foreach (TileSlot slot in tileStyle.Slots)
                            {
                                // Stairs list stair multis; they are multi ids, not art.
                                bool multiColumn = IsMultiColumn(slot.Label);
                                if (slot.IsEmpty || slot.TileId > ushort.MaxValue || (!multiColumn && !slot.IsValid))
                                {
                                    continue;
                                }

                                style.Add(slot.Label, (ushort)slot.TileId);
                            }
                        }

                        if (style.Slots.Count == 0)
                        {
                            continue;
                        }

                        IEnumerable<HouseStyle> parts = splitFloorsByColour && kind == HousePartKind.Floor
                            ? SplitByColour(style)
                            : new[] { style };
                        foreach (HouseStyle part in parts)
                        {
                            part.Category = number;
                            part.ByArt = byArt;
                            result.Styles.Add(part);
                        }
                    }
                }
            }

            result.Apply(config);
            result.BuildRoles();
            result.FinishWalls();
            result.Order();
            result.SetThemes(config);
            foreach (HouseStyle style in result.Styles)
            {
                if (string.IsNullOrEmpty(style.Family))
                {
                    style.Family = style.Group ?? string.Empty;
                }
            }

            result.Errors.AddRange(errors);
            if (config != null)
            {
                result.Errors.AddRange(config.Errors);
            }

            return result;
        }

        /// <summary>Stairs list stair multis under "Multi" columns: multi ids, not art.</summary>
        private static bool IsMultiColumn(string label) => label.StartsWith("Multi", System.StringComparison.Ordinal);

        // Where each style named by the style file comes in the lists.
        private readonly Dictionary<HouseStyle, int> _rank = new Dictionary<HouseStyle, int>();

        /// <summary>
        /// Applies the style file to the styles read from the tables. An entry finds its style by
        /// a piece (its anchor): the first style of the kind that lists it. It can rename and
        /// regroup the style, give it other pieces, and take the place of further styles (a set
        /// the table spreads over several rows). An entry whose pieces this client has no art for
        /// is left out, so one file serves every client.
        /// </summary>
        private void Apply(HouseStyleConfig config)
        {
            var owners = new Dictionary<(HousePartKind Kind, ushort Id), HouseStyle>();
            foreach (HouseStyle style in Styles)
            {
                foreach (KeyValuePair<string, ushort> piece in style.Slots)
                {
                    if (!IsMultiColumn(piece.Key))
                    {
                        owners.TryAdd((style.Kind, piece.Value), style);
                    }
                }
            }

            foreach (HouseStyle style in Styles)
            {
                foreach (KeyValuePair<string, ushort> piece in style.Slots)
                {
                    if (!IsMultiColumn(piece.Key) && owners[(style.Kind, piece.Value)] == style)
                    {
                        style.Anchor = piece.Value;
                        break;
                    }
                }
            }

            if (config == null)
            {
                return;
            }

            var removed = new HashSet<HouseStyle>();
            foreach (HouseStyleEntry entry in config.Styles)
            {
                List<(string Role, ushort Id)> pieces = entry.Pieces?
                    .Where(piece => IsMultiColumn(piece.Role) || (piece.Id < Ultima.TileData.ItemTable.Length && Ultima.Art.IsValidStatic(piece.Id)))
                    .ToList();
                if (pieces != null && !pieces.Any(piece => !IsMultiColumn(piece.Role)))
                {
                    continue;
                }

                HouseStyle target = null;
                if (!entry.IsNew)
                {
                    owners.TryGetValue((entry.Kind, entry.Anchor), out target);
                }

                if (target == null)
                {
                    // A style of the file's own. It stands where the first style it replaces stood.
                    if (pieces == null)
                    {
                        continue;
                    }

                    HouseStyle first = null;
                    foreach (ushort id in entry.Replaces)
                    {
                        if (owners.TryGetValue((entry.Kind, id), out first))
                        {
                            break;
                        }
                    }

                    target = new HouseStyle(entry.Kind, entry.Group ?? entry.Name ?? string.Empty, entry.Name ?? string.Empty)
                    {
                        Category = first?.Category ?? -1,
                        ByArt = first?.ByArt ?? false
                    };
                    int at = first != null ? Styles.IndexOf(first) : -1;
                    Styles.Insert(at >= 0 ? at : Styles.Count, target);
                }

                if (pieces != null)
                {
                    target.Clear();
                    foreach ((string role, ushort id) in pieces)
                    {
                        target.Add(role, id);
                    }
                }

                foreach (ushort id in entry.Replaces)
                {
                    if (owners.TryGetValue((entry.Kind, id), out HouseStyle other) && other != target)
                    {
                        removed.Add(other);
                    }
                }

                target.Anchor = entry.Anchor != 0 ? entry.Anchor : target.IconId;
                target.Name = entry.Name ?? target.Name;
                target.Group = entry.Group ?? target.Group;
                target.Family = entry.Family ?? target.Family;
                target.Hidden = entry.Hidden ?? target.Hidden;
                target.Borrows = entry.Borrows ?? target.Borrows;
                if (entry.Layout != null)
                {
                    target.Paired = entry.Layout == HouseStyleEntry.LayoutPaired;
                    bool cycle = entry.Layout == HouseStyleEntry.LayoutCycle;
                    target.CycleSouth = cycle ? target.Slots.Keys.Count(label => label.StartsWith("CycleSouth", System.StringComparison.Ordinal)) : 0;
                    target.CycleEast = cycle ? target.Slots.Keys.Count(label => label.StartsWith("CycleEast", System.StringComparison.Ordinal)) : 0;
                }

                _rank[target] = _rank.Count;
            }

            Styles.RemoveAll(removed.Contains);

            // An order of the user's for a kind goes over the order of the entries.
            foreach (KeyValuePair<HousePartKind, List<ushort>> order in config.Order)
            {
                var placed = new HashSet<HouseStyle>();
                int next = _rank.Count == 0 ? 0 : _rank.Values.Max() + 1;
                foreach (ushort anchor in order.Value)
                {
                    HouseStyle style = Styles.FirstOrDefault(s => s.Kind == order.Key && s.Anchor == anchor);
                    if (style != null && placed.Add(style))
                    {
                        _rank[style] = next++;
                    }
                }

                // The styles of the kind the order does not name keep their place among themselves, behind it.
                foreach (HouseStyle style in Styles.Where(s => s.Kind == order.Key && !placed.Contains(s) && _rank.ContainsKey(s)).OrderBy(s => _rank[s]).ToList())
                {
                    _rank[style] = next++;
                }
            }
        }

        /// <summary>
        /// The lookup from a tile to its style and column: the first style that lists it. A panel
        /// of a style that goes through its panels in turn counts as a plain run piece for
        /// everything that asks what a tile is (redrawing, turning).
        /// </summary>
        private void BuildRoles()
        {
            foreach (HouseStyle style in Styles)
            {
                foreach (KeyValuePair<string, ushort> piece in style.Slots)
                {
                    if (IsMultiColumn(piece.Key))
                    {
                        continue;
                    }

                    string label = piece.Key.StartsWith("CycleSouth", System.StringComparison.Ordinal) ? "South2"
                        : piece.Key.StartsWith("CycleEast", System.StringComparison.Ordinal) ? "East2"
                        : piece.Key;
                    _roles.TryAdd(piece.Value, (style, label));
                }
            }
        }

        /// <summary>
        /// Makes every wall style build whole walls: empty run columns are filled, a row without a
        /// post borrows one from its category, and in a category sorted by its art corners and
        /// posts are lent between the rows. A style the file marks as not borrowing is two or
        /// three pieces on their own and stays as it is.
        /// </summary>
        private void FinishWalls()
        {
            var posts = new Dictionary<int, List<ushort>>();
            var sorted = new Dictionary<int, List<HouseStyle>>();
            foreach (HouseStyle style in Styles)
            {
                if (style.Kind != HousePartKind.Wall)
                {
                    continue;
                }

                if (style.ByArt && style.Borrows)
                {
                    if (!sorted.TryGetValue(style.Category, out List<HouseStyle> rows))
                    {
                        sorted[style.Category] = rows = new List<HouseStyle>();
                    }

                    rows.Add(style);
                }

                FillWallRuns(style, style.Get("CycleSouth1"), style.Get("CycleEast1"));

                // A row without a post (Celtic Walls 2) borrows the one of an earlier row
                // of its category that is as high: the same wall, differently decorated.
                if (!posts.TryGetValue(style.Category, out List<ushort> categoryPosts))
                {
                    posts[style.Category] = categoryPosts = new List<ushort>();
                }

                ushort post = style.Get("Post"), straight = style.Get("South1");
                if (post != 0)
                {
                    categoryPosts.Add(post);
                }
                else if (straight != 0 && style.Borrows)
                {
                    foreach (ushort other in categoryPosts)
                    {
                        if (Ultima.TileData.ItemTable[other].Height == Ultima.TileData.ItemTable[straight].Height)
                        {
                            style.Add("Post", other);
                            break;
                        }
                    }
                }
            }

            foreach (List<HouseStyle> rows in sorted.Values)
            {
                LendCornersAndPosts(rows);
            }
        }

        /// <summary>
        /// Puts the styles in the style file's order. A style the file does not name (one only
        /// another client version has) stays behind the style it follows in the tables.
        /// </summary>
        private void Order()
        {
            if (_rank.Count == 0)
            {
                return;
            }

            var last = new Dictionary<HousePartKind, int>();
            var keys = new Dictionary<HouseStyle, (int Rank, int Index)>();
            for (int i = 0; i < Styles.Count; ++i)
            {
                HouseStyle style = Styles[i];
                if (_rank.TryGetValue(style, out int rank))
                {
                    last[style.Kind] = rank;
                }
                else if (!last.TryGetValue(style.Kind, out rank))
                {
                    rank = -1;
                }

                keys[style] = (rank, _rank.ContainsKey(style) ? -1 : i);
            }

            List<HouseStyle> ordered = Styles.OrderBy(style => keys[style].Rank).ThenBy(style => keys[style].Index).ToList();
            Styles.Clear();
            Styles.AddRange(ordered);
        }

        /// <summary>
        /// In a category sorted by its art, a row often has walls but no corner or post of its
        /// own: the set's corners and posts sit in another row. Such a style takes the first
        /// piece of the category that is a corner (or post) of its walls' height.
        /// </summary>
        private static void LendCornersAndPosts(List<HouseStyle> styles)
        {
            static int Class(ushort id) => Ultima.TileData.ItemTable[id].Height >= 15 ? 2 : Ultima.TileData.ItemTable[id].Height >= 8 ? 1 : 0;
            var pieces = new List<ushort>();
            foreach (HouseStyle style in styles)
            {
                foreach (ushort id in style.Slots.Values)
                {
                    if (!pieces.Contains(id))
                    {
                        pieces.Add(id);
                    }
                }
            }

            foreach (HouseStyle style in styles)
            {
                ushort wall = style.Get("South1") != 0 ? style.Get("South1") : style.Get("East1");
                if (wall == 0)
                {
                    continue;
                }

                foreach ((string column, TilePlacement placement) in new[] { ("Corner", TilePlacement.Cell), ("Post", TilePlacement.Vertex) })
                {
                    if (style.Get(column) != 0)
                    {
                        continue;
                    }

                    foreach (ushort id in pieces)
                    {
                        if (Class(id) == Class(wall) && ArtFacing.Placement(id) == placement &&
                            (Ultima.TileData.ItemTable[id].Flags & Ultima.TileFlag.Window) == 0)
                        {
                            style.Add(column, id);
                            break;
                        }
                    }
                }
            }
        }
    }
}
