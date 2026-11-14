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
    }
}
