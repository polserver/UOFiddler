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

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    internal enum HousePartKind
    {
        Wall,
        Floor,
        Door,
        Stair,
        Roof,
        Misc,
        Teleporter
    }

    /// <summary>
    /// One row of a client house design table (walls.txt, floors.txt ...): a named set of pieces,
    /// each under the column label the client uses (South1, Corner, F3, MultiNorth ...).
    /// </summary>
    internal sealed class HouseStyle
    {
        private readonly Dictionary<string, ushort> _slots = new Dictionary<string, ushort>();

        public HouseStyle(HousePartKind kind, string group, string name)
        {
            Kind = kind;
            Group = group;
            Name = name;
        }

        public HousePartKind Kind { get; }

        /// <summary>The category name (the first style's comment), grouping a material's styles.</summary>
        public string Group { get; internal set; }

        public string Name { get; internal set; }

        /// <summary>The material, shared by the styles of every kind made of it. From the style file; may be empty.</summary>
        public string Family { get; internal set; } = string.Empty;

        /// <summary>True for a style the style file keeps out of the lists.</summary>
        public bool Hidden { get; internal set; }

        /// <summary>A piece that tells the style from every other of its kind: how files refer to it. 0 when it has none.</summary>
        public ushort Anchor { get; internal set; }

        /// <summary>False for a wall style that takes no post or corner from the other rows of its category.</summary>
        internal bool Borrows { get; set; } = true;

        /// <summary>The table category the style was read from, counted over all tables.</summary>
        internal int Category { get; set; } = -1;

        /// <summary>True for a wall style of a category sorted by its art.</summary>
        internal bool ByArt { get; set; }

        public IReadOnlyDictionary<string, ushort> Slots => _slots;

        /// <summary>
        /// True for a wall style whose run pieces are the two halves of an ornament (the shadow
        /// banner): South1 and South3 along X, East3 and East1 along Y, with South2 and East2 the
        /// plain wall. A run is then laid as pairs, not as start, middle and end.
        /// </summary>
        public bool Paired { get; internal set; }

        /// <summary>
        /// For a wall style that is a set of different panels (the carved red marble): how many
        /// it has along X (CycleSouth1 ...) and along Y (CycleEast1 ...). A run is laid as one
        /// panel after another, starting again after the last. 0 for an ordinary style.
        /// </summary>
        public int CycleSouth { get; internal set; }

        public int CycleEast { get; internal set; }

        /// <summary>A piece to show for the style in lists.</summary>
        public ushort IconId { get; private set; }

        /// <summary>The piece under a column label, or 0 when the style has none.</summary>
        public ushort Get(string label) => _slots.TryGetValue(label, out ushort id) ? id : (ushort)0;

        internal void Add(string label, ushort id)
        {
            _slots[label] = id;
            if (IconId == 0)
            {
                IconId = id;
            }
        }

        internal void Clear()
        {
            _slots.Clear();
            IconId = 0;
        }

        public override string ToString() => Name;
    }

    /// <summary>
    /// The client's house design tables, read once per client, with a lookup from tile id to the
    /// style and column it belongs to.
    /// </summary>
    internal sealed class HouseStyles
    {
        private static readonly Dictionary<string, HousePartKind> _kinds = new Dictionary<string, HousePartKind>
        {
            ["Walls"] = HousePartKind.Wall,
            ["Floors"] = HousePartKind.Floor,
            ["Doors"] = HousePartKind.Door,
            ["Stairs"] = HousePartKind.Stair,
            ["Roof"] = HousePartKind.Roof,
            ["Misc"] = HousePartKind.Misc,
            ["Teleporters"] = HousePartKind.Teleporter
        };

        private readonly Dictionary<ushort, (HouseStyle Style, string Slot)> _roles = new Dictionary<ushort, (HouseStyle, string)>();

        // Listed pieces by art hash, for tiles that copy one under another id. Filled on first use.
        private Dictionary<ulong, ushort> _listedByArt;

        // Stair corner pieces: the set of four each belongs to and its place in it. Filled on first use.
        private Dictionary<ushort, (ushort[] Corners, int Index)> _stairCorners;

        private HouseStyles()
        {
        }

        public List<HouseStyle> Styles { get; } = new List<HouseStyle>();

        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// The themes of the style file this client can build: those whose wall, floor and roof
        /// style it has. An older client has no gargish, crystal or shadow sets, and so none of
        /// their themes.
        /// </summary>
        public List<HouseTheme> Themes { get; } = new List<HouseTheme>();

        /// <summary>The foundations of the style file whose wall style this client has.</summary>
        public List<HouseFoundation> Foundations { get; } = new List<HouseFoundation>();

        /// <summary>The style of a kind that a piece names: the one with that anchor, or else the one that lists the piece.</summary>
        public HouseStyle Find(HousePartKind kind, ushort anchor)
        {
            if (anchor == 0)
            {
                return null;
            }

            foreach (HouseStyle style in Styles)
            {
                if (style.Kind == kind && style.Anchor == anchor)
                {
                    return style;
                }
            }

            return TryGetRole(anchor, out HouseStyle listed, out _) && listed.Kind == kind ? listed : null;
        }

        private static readonly string[] _windowsAlongX = { "WindowS", "AltWindowS", "SecondAltWindowS" };
        private static readonly string[] _windowsAlongY = { "WindowE", "AltWindowE", "SecondAltWindowE" };
        private readonly Dictionary<(HouseStyle Style, bool AlongX), ushort> _windows = new Dictionary<(HouseStyle, bool), ushort>();

        /// <summary>
        /// The window piece for a wall of a style, along X or along Y: the style's own first, and
        /// when it has none (many tables keep windows in a row of their own) the window of
        /// another style of its material that is as high as its walls. 0 when there is none.
        /// </summary>
        public ushort WindowFor(HouseStyle style, bool alongX)
        {
            if (style == null)
            {
                return 0;
            }

            if (_windows.TryGetValue((style, alongX), out ushort known))
            {
                return known;
            }

            string[] labels = alongX ? _windowsAlongX : _windowsAlongY;
            ushort Own(HouseStyle of)
            {
                foreach (string label in labels)
                {
                    if (of.Get(label) is var id and not 0)
                    {
                        return id;
                    }
                }

                return 0;
            }

            ushort window = Own(style);
            ushort wall = style.Get(alongX ? "South2" : "East2");
            if (window == 0 && wall != 0)
            {
                foreach (HouseStyle other in Styles)
                {
                    if (other != style && other.Kind == HousePartKind.Wall && other.Group == style.Group && Own(other) is var id and not 0 &&
                        Ultima.TileData.ItemTable[id].Height == Ultima.TileData.ItemTable[wall].Height)
                    {
                        window = id;
                        break;
                    }
                }
            }

            _windows[(style, alongX)] = window;
            return window;
        }

        private readonly Dictionary<HouseStyle, HouseStyle> _parapets = new Dictionary<HouseStyle, HouseStyle>();

        /// <summary>
        /// The style a wall style's battlements are built in: the roof edging of its material
        /// (the one named most like it, where a material has several), or else its quarter
        /// walls. Null when the material has neither.
        /// </summary>
        public HouseStyle ParapetFor(HouseStyle walls)
        {
            if (walls == null)
            {
                return null;
            }

            if (_parapets.TryGetValue(walls, out HouseStyle known))
            {
                return known;
            }

            string[] words = walls.Name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            HouseStyle best = null, low = null;
            int most = -1;
            foreach (HouseStyle other in Styles)
            {
                ushort piece = other.Get("South2");
                if (other.Kind != HousePartKind.Wall || other.Group != walls.Group || piece == 0 || other.Get("East2") == 0)
                {
                    continue;
                }

                if (other.Name.Contains("Roof Edging", System.StringComparison.OrdinalIgnoreCase))
                {
                    int shared = other.Name.Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Count(word => words.Contains(word));
                    if (shared > most)
                    {
                        (best, most) = (other, shared);
                    }
                }
                else if (low == null && Ultima.TileData.ItemTable[piece].Height < 8)
                {
                    low = other;
                }
            }

            return _parapets[walls] = best ?? low;
        }

        /// <summary>Reads the themes again after the user's copy of the style file changed.</summary>
        public void ReloadThemes()
        {
            SetThemes(HouseStyleConfig.Load());
        }

        private void SetThemes(HouseStyleConfig config)
        {
            Themes.Clear();
            Foundations.Clear();
            if (config == null)
            {
                return;
            }

            foreach (HouseFoundation foundation in config.Foundations)
            {
                if (foundation.Style == 0 ? TryGetRole(Tools.FoundationTool.FoundationPost, out _, out _) : Find(HousePartKind.Wall, foundation.Style) != null)
                {
                    Foundations.Add(foundation);
                }
            }

            foreach (HouseTheme theme in config.Themes)
            {
                if (Find(HousePartKind.Wall, theme.Walls) != null && Find(HousePartKind.Floor, theme.Floor) != null &&
                    Find(HousePartKind.Roof, theme.Roof) != null)
                {
                    Themes.Add(theme);
                }
            }
        }

        /// <summary>
        /// Reads the tables. With <paramref name="splitFloorsByColour"/> a floors.txt row, which is a
        /// palette of up to 16 tiles often in several colours, becomes one style per colour, so a
        /// floor laid with a style does not mix them. The style file (<see cref="HouseStyleConfig"/>)
        /// then names, groups, orders and corrects what the tables give.
        /// </summary>
        public static HouseStyles Load(bool splitFloorsByColour = false)
        {
            return Load(splitFloorsByColour, HouseStyleConfig.Load());
        }

        private bool _splitFloors;

        /// <summary>Raised after <see cref="Reload"/>: every style is a new object.</summary>
        public event Action Reloaded;

        /// <summary>
        /// Reads the tables and the style file again, into this same object: after the style
        /// file was changed. The styles are new objects; whoever holds one finds its successor
        /// with <see cref="Successor"/>.
        /// </summary>
        public void Reload()
        {
            HouseStyles fresh = Load(_splitFloors);
            Styles.Clear();
            Styles.AddRange(fresh.Styles);
            Errors.Clear();
            Errors.AddRange(fresh.Errors);
            Themes.Clear();
            Themes.AddRange(fresh.Themes);
            Foundations.Clear();
            Foundations.AddRange(fresh.Foundations);
            _roles.Clear();
            foreach (KeyValuePair<ushort, (HouseStyle Style, string Slot)> role in fresh._roles)
            {
                _roles[role.Key] = role.Value;
            }

            _windows.Clear();
            _parapets.Clear();
            _listedByArt = null;
            _stairCorners = null;
            Reloaded?.Invoke();
        }

        /// <summary>The style that an older style object now is: of its kind, with its anchor piece or else its name.</summary>
        public HouseStyle Successor(HouseStyle style)
        {
            if (style == null || Styles.Contains(style))
            {
                return style;
            }

            return Find(style.Kind, style.Anchor) ?? Styles.FirstOrDefault(s => s.Kind == style.Kind && s.Name == style.Name) ?? style;
        }

        /// <summary>
        /// A style made of the given pieces, with the empty wall columns filled as the tables'
        /// styles are: what the style editor shows before it saves.
        /// </summary>
        public static HouseStyle Compose(HousePartKind kind, string group, string name, IEnumerable<(string Role, ushort Id)> pieces, string layout)
        {
            var style = new HouseStyle(kind, group ?? string.Empty, name ?? string.Empty);
            foreach ((string role, ushort id) in pieces)
            {
                style.Add(role, id);
            }

            style.Paired = layout == HouseStyleEntry.LayoutPaired;
            if (layout == HouseStyleEntry.LayoutCycle)
            {
                style.CycleSouth = style.Slots.Keys.Count(label => label.StartsWith("CycleSouth", System.StringComparison.Ordinal));
                style.CycleEast = style.Slots.Keys.Count(label => label.StartsWith("CycleEast", System.StringComparison.Ordinal));
            }

            if (kind == HousePartKind.Wall)
            {
                FillWallRuns(style, style.Get("CycleSouth1"), style.Get("CycleEast1"));
            }

            return style;
        }

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
                if (!slot.IsEmpty && slot.IsValid && slot.TileId <= ushort.MaxValue && !ids.Contains((ushort)slot.TileId))
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

        /// <summary>The style and column a tile id is listed under, when any.</summary>
        public bool TryGetRole(ushort id, out HouseStyle style, out string slot)
        {
            if (_roles.TryGetValue(id, out var role))
            {
                style = role.Style;
                slot = role.Slot;
                return true;
            }

            style = null;
            slot = null;
            return false;
        }

        /// <summary>
        /// The style and column of a tile, also when the tables list it under another id with the
        /// same art: the classic roof and stair tiles are copies of the custom house pieces, which
        /// is how their facing is known. <paramref name="exact"/> tells the two cases apart.
        /// </summary>
        public bool TryGetRoleByArt(ushort id, out HouseStyle style, out string slot, out bool exact)
        {
            exact = TryGetRole(id, out style, out slot);
            if (exact)
            {
                return true;
            }

            return TryGetListedTwin(id, out ushort listed) && TryGetRole(listed, out style, out slot);
        }

        /// <summary>
        /// The listed piece a tile copies. Either the art is the same, or the tile sits in a run of
        /// ids that copies a listed run piece by piece (the classic roof sets do; one or two of
        /// their pieces were redrawn, and those are found by their place in the run).
        /// </summary>
        public bool TryGetListedTwin(ushort id, out ushort listed)
        {
            if (_roles.ContainsKey(id))
            {
                listed = id;
                return true;
            }

            if (TryGetSameArt(id, out listed))
            {
                return true;
            }

            // Neighbours of the same kind that copy a listed piece say where this one's twin should be.
            if (id >= Ultima.TileData.ItemTable.Length)
            {
                listed = 0;
                return false;
            }

            string name = Ultima.TileData.ItemTable[id].Name;
            var votes = new Dictionary<(int Offset, HouseStyle Style), int>();
            for (int distance = 1; distance <= 6; distance++)
            {
                foreach (int neighbour in new[] { id - distance, id + distance })
                {
                    if (neighbour >= 0 && neighbour < Ultima.TileData.ItemTable.Length && !_roles.ContainsKey((ushort)neighbour) &&
                        SameSet(Ultima.TileData.ItemTable[neighbour].Name, name) &&
                        TryGetSameArt((ushort)neighbour, out ushort twin))
                    {
                        var key = (twin - neighbour, _roles[twin].Style);
                        votes[key] = votes.TryGetValue(key, out int count) ? count + 1 : 1;
                    }
                }
            }

            foreach (KeyValuePair<(int Offset, HouseStyle Style), int> vote in votes)
            {
                int candidate = id + vote.Key.Offset;
                if (vote.Value >= 3 && candidate >= 0 && candidate <= ushort.MaxValue &&
                    _roles.TryGetValue((ushort)candidate, out (HouseStyle Style, string Slot) role) &&
                    role.Style == vote.Key.Style)
                {
                    listed = (ushort)candidate;
                    return true;
                }
            }

            listed = 0;
            return false;
        }

        /// <summary>
        /// The four corner pieces of a stair set, in the order a left turn goes through them
        /// (south-east, north-east, north-west, south-west), and where <paramref name="id"/> is in
        /// it. stairs.txt names only the two front corners of a stair that climbs north: the one
        /// that is its own mirror image is the south-east one, the mirror twin of the other is the
        /// north-east one, and the fourth is the id next to those three.
        /// </summary>
        public bool TryGetStairCorner(ushort id, out ushort[] corners, out int index)
        {
            if (_stairCorners == null)
            {
                _stairCorners = new Dictionary<ushort, (ushort[], int)>();
                foreach (HouseStyle style in Styles)
                {
                    if (style.Kind != HousePartKind.Stair)
                    {
                        continue;
                    }

                    AddStairCorners(style.Get("Squared1"), style.Get("Squared2"));
                    AddStairCorners(style.Get("Rounded1"), style.Get("Rounded2"));
                }
            }

            if (_stairCorners.TryGetValue(id, out (ushort[] Corners, int Index) found))
            {
                corners = found.Corners;
                index = found.Index;
                return true;
            }

            corners = null;
            index = -1;
            return false;
        }

        private void AddStairCorners(ushort first, ushort second)
        {
            if (first == 0 || second == 0)
            {
                return;
            }

            bool firstSymmetric = ArtFacing.Transpose(first) == first;
            bool secondSymmetric = ArtFacing.Transpose(second) == second;
            if (firstSymmetric == secondSymmetric)
            {
                return;
            }

            ushort southEast = firstSymmetric ? first : second;
            ushort southWest = firstSymmetric ? second : first;
            ushort northEast = ArtFacing.Transpose(southWest);

            int low = Math.Min(southEast, Math.Min(southWest, northEast));
            int high = Math.Max(southEast, Math.Max(southWest, northEast));
            if (high - low != 2)
            {
                return;
            }

            foreach (int candidate in new[] { low - 1, high + 1 })
            {
                if (candidate < 0 || candidate > ushort.MaxValue || _roles.ContainsKey((ushort)candidate) ||
                    ArtFacing.ArtHash((ushort)candidate) == 0 || ArtFacing.Transpose((ushort)candidate) != candidate)
                {
                    continue;
                }

                ushort[] corners = { southEast, northEast, (ushort)candidate, southWest };
                for (int i = 0; i < corners.Length; i++)
                {
                    _stairCorners.TryAdd(corners[i], (corners, i));
                }

                return;
            }
        }

        /// <summary>Tiledata names of one set: equal, or one is the other with a letter more ("shingle", "shingles").</summary>
        private static bool SameSet(string a, string b)
        {
            a ??= string.Empty;
            b ??= string.Empty;
            return Math.Abs(a.Length - b.Length) <= 1 &&
                   (a.StartsWith(b, StringComparison.OrdinalIgnoreCase) || b.StartsWith(a, StringComparison.OrdinalIgnoreCase));
        }

        private bool TryGetSameArt(ushort id, out ushort listed)
        {
            if (_listedByArt == null)
            {
                _listedByArt = new Dictionary<ulong, ushort>();
                foreach (KeyValuePair<ushort, (HouseStyle Style, string Slot)> role in _roles)
                {
                    if (role.Value.Style.Kind is not (HousePartKind.Roof or HousePartKind.Stair))
                    {
                        continue;
                    }

                    ulong roleHash = ArtFacing.ArtHash(role.Key);
                    if (roleHash != 0)
                    {
                        _listedByArt.TryAdd(roleHash, role.Key);
                    }
                }
            }

            ulong hash = ArtFacing.ArtHash(id);
            listed = 0;
            return hash != 0 && _listedByArt.TryGetValue(hash, out listed);
        }
    }
}
