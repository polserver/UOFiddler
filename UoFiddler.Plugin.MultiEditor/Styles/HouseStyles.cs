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
using UoFiddler.Plugin.MultiEditor.House;

namespace UoFiddler.Plugin.MultiEditor.Styles
{
    /// <summary>
    /// The client's house design tables, read once per client, with a lookup from tile id to the
    /// style and column it belongs to.
    /// </summary>
    internal sealed partial class HouseStyles
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
    }
}
