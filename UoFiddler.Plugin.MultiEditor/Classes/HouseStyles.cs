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
        public string Group { get; }

        public string Name { get; }

        public IReadOnlyDictionary<string, ushort> Slots => _slots;

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
        /// Reads the tables. With <paramref name="splitFloorsByColour"/> a floors.txt row, which is a
        /// palette of up to 16 tiles often in several colours, becomes one style per colour, so a
        /// floor laid with a style does not mix them.
        /// </summary>
        public static HouseStyles Load(bool splitFloorsByColour = false)
        {
            // New tables mean the client files may have changed: measure the art again.
            ArtFacing.Clear();

            var result = new HouseStyles();
            List<TileGroupFile> files = TileGroupRegistry.LoadAll(out List<string> errors);
            foreach (TileGroupFile file in files)
            {
                if (!_kinds.TryGetValue(file.DisplayName, out HousePartKind kind))
                {
                    continue;
                }

                foreach (TileCategory category in file.Categories)
                {
                    foreach (TileStyle tileStyle in category.Styles)
                    {
                        var style = new HouseStyle(kind, category.Name, tileStyle.Name);
                        foreach (TileSlot slot in tileStyle.Slots)
                        {
                            // Stairs list stair multis; they are multi ids, not art.
                            bool multiColumn = slot.Label.StartsWith("Multi", System.StringComparison.Ordinal);
                            if (slot.IsEmpty || slot.TileId > ushort.MaxValue || (!multiColumn && !slot.IsValid))
                            {
                                continue;
                            }

                            style.Add(slot.Label, (ushort)slot.TileId);
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
                            result.Styles.Add(part);
                            foreach (KeyValuePair<string, ushort> piece in part.Slots)
                            {
                                // Stair multi columns hold multi ids, not art.
                                if (!piece.Key.StartsWith("Multi", System.StringComparison.Ordinal))
                                {
                                    result._roles.TryAdd(piece.Value, (part, piece.Key));
                                }
                            }
                        }
                    }
                }
            }

            result.Errors.AddRange(errors);
            return result;
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
