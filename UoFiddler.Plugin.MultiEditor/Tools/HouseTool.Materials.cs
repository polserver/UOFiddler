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
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    internal sealed partial class HouseTool
    {
        public void SetFoundation(bool on)
        {
            if (_draft.Foundation != on)
            {
                Edit(() => _draft.Foundation = on);
            }
        }

        /// <summary>
        /// Sets a wall, floor, roof, door or stair style. A wall or floor style goes to one
        /// storey, or to every storey when <paramref name="storey"/> is null. A door or stair
        /// style is the house's: the doors and stairs it already has change with it.
        /// </summary>
        public void SetMaterial(HouseStyle style, int? storey)
        {
            if (style == null)
            {
                return;
            }

            Edit(() =>
            {
                if (style.Kind == HousePartKind.Roof)
                {
                    _draft.RoofStyle = style;
                    return;
                }

                if (style.Kind is HousePartKind.Door or HousePartKind.Stair)
                {
                    Restyle(style);
                    return;
                }

                HouseStyle[] target = style.Kind == HousePartKind.Floor ? _draft.FloorStyles : style.Kind == HousePartKind.Wall ? _draft.WallStyles : null;
                for (int k = 0; target != null && k < HouseDraft.MaxStoreys; ++k)
                {
                    if (storey == null || storey == k)
                    {
                        target[k] = style;
                    }
                }
            });
        }

        /// <summary>Makes a door or stair style the house's, and gives it to the doors or stairs already there that it has the piece for.</summary>
        private void Restyle(HouseStyle style)
        {
            if (style.Kind == HousePartKind.Door)
            {
                _draft.DoorStyle = style;
                foreach (var key in _draft.Doors.Keys.ToList())
                {
                    string piece = _draft.Doors[key].Piece;
                    if (style.Get(piece) != 0)
                    {
                        _draft.Doors[key] = (style, piece);
                    }
                }
            }
            else if (style.Kind == HousePartKind.Stair)
            {
                _draft.StairStyle = style;
                for (int i = 0; i < _draft.Stairs.Count; ++i)
                {
                    var stairs = _draft.Stairs[i];
                    if (style.Get(stairs.Column) != 0)
                    {
                        _draft.Stairs[i] = (stairs.Storey, stairs.X, stairs.Y, stairs.Column, style);
                    }
                }
            }
        }

        /// <summary>The foundations this client can build.</summary>
        public IReadOnlyList<HouseFoundation> Foundations => (IReadOnlyList<HouseFoundation>)Context.HouseStyles?.Foundations ?? Array.Empty<HouseFoundation>();

        /// <summary>Gives the house one of the client's kinds of foundation: its wall style, rows and height.</summary>
        public void SetFoundation(HouseFoundation foundation)
        {
            if (foundation == null || Context.HouseStyles == null)
            {
                return;
            }

            HouseStyle style = Context.HouseStyles.Find(HousePartKind.Wall, foundation.Style);
            Edit(() =>
            {
                _draft.Foundation = true;
                (_draft.FoundationStyle, _draft.FoundationTiers, _draft.FoundationHeight) = (style, foundation.Tiers, foundation.Height);
            });
        }

        /// <summary>The kind of foundation the house has, or null when it has none or one put together by hand.</summary>
        public HouseFoundation CurrentFoundation()
        {
            if (!_draft.Foundation || Context.HouseStyles == null)
            {
                return null;
            }

            foreach (HouseFoundation foundation in Foundations)
            {
                if (_draft.FoundationStyle == Context.HouseStyles.Find(HousePartKind.Wall, foundation.Style) &&
                    _draft.FoundationTiers == foundation.Tiers && _draft.FoundationHeight == foundation.Height)
                {
                    return foundation;
                }
            }

            return null;
        }

        /// <summary>Sets the wall style the foundation ring is built in; null is the client houses' own.</summary>
        public void SetFoundationStyle(HouseStyle style)
        {
            if (style == null || style.Kind == HousePartKind.Wall)
            {
                Edit(() => _draft.FoundationStyle = style);
            }
        }

        /// <summary>The themes this client can build.</summary>
        public IReadOnlyList<HouseTheme> Themes => (IReadOnlyList<HouseTheme>)Context.HouseStyles?.Themes ?? Array.Empty<HouseTheme>();

        /// <summary>
        /// Gives the whole house the styles of a theme, as one step of the draft's history: walls
        /// and floors of every storey, roof, flat roof, doors, stairs and foundation. Returns the
        /// parts the theme names that this client does not have; those stay as they are.
        /// </summary>
        public List<string> ApplyTheme(HouseTheme theme)
        {
            var missing = new List<string>();
            if (theme == null || Context.HouseStyles == null)
            {
                return missing;
            }

            EnsureMaterials();
            HouseStyle Part(HousePartKind kind, ushort anchor, string what)
            {
                HouseStyle style = Context.HouseStyles.Find(kind, anchor);
                if (style == null && anchor != 0)
                {
                    missing.Add(what);
                }

                return style;
            }

            HouseStyle walls = Part(HousePartKind.Wall, theme.Walls, "walls"), floor = Part(HousePartKind.Floor, theme.Floor, "floor");
            HouseStyle roof = Part(HousePartKind.Roof, theme.Roof, "roof"), flatRoof = Part(HousePartKind.Floor, theme.FlatRoof, "flat roof");
            HouseStyle door = Part(HousePartKind.Door, theme.Door, "doors"), stairs = Part(HousePartKind.Stair, theme.Stairs, "stairs");
            HouseStyle foundation = Part(HousePartKind.Wall, theme.Foundation, "foundation");
            Edit(() =>
            {
                for (int k = 0; k < HouseDraft.MaxStoreys; ++k)
                {
                    _draft.WallStyles[k] = walls ?? _draft.WallStyles[k];
                    _draft.FloorStyles[k] = floor ?? _draft.FloorStyles[k];
                }

                _draft.RoofStyle = roof ?? _draft.RoofStyle;
                _draft.FlatRoofStyle = flatRoof ?? _draft.FlatRoofStyle;
                (_draft.FoundationStyle, _draft.FoundationTiers, _draft.FoundationHeight) = (foundation, theme.FoundationTiers, theme.FoundationHeight);
                if (door != null)
                {
                    Restyle(door);
                }

                if (stairs != null)
                {
                    Restyle(stairs);
                }
            });
            return missing;
        }

        /// <summary>The draft's styles as a theme of the given name: what "save as theme" stores.</summary>
        public HouseTheme ThemeOfDraft(string name)
        {
            EnsureMaterials();
            return new HouseTheme
            {
                Name = name,
                Walls = _draft.WallStyles[0]?.Anchor ?? 0,
                Floor = _draft.FloorStyles[0]?.Anchor ?? 0,
                Roof = _draft.RoofStyle?.Anchor ?? 0,
                FlatRoof = _draft.FlatRoofStyle?.Anchor ?? 0,
                Door = _draft.DoorStyle?.Anchor ?? 0,
                Stairs = _draft.StairStyle?.Anchor ?? 0,
                Foundation = _draft.FoundationStyle?.Anchor ?? 0,
                FoundationTiers = _draft.FoundationTiers,
                FoundationHeight = _draft.FoundationHeight,
                IsUser = true
            };
        }

        /// <summary>The theme the draft's styles are, or null when they are a mix of the user's own.</summary>
        public HouseTheme CurrentTheme()
        {
            if (Context.HouseStyles == null)
            {
                return null;
            }

            EnsureMaterials();
            bool Is(HouseStyle style, HousePartKind kind, ushort anchor) => anchor == 0 || style == Context.HouseStyles.Find(kind, anchor);
            foreach (HouseTheme theme in Themes)
            {
                bool storeys = true;
                for (int k = 0; k < Math.Max(1, TopStorey) && storeys; ++k)
                {
                    storeys = Is(_draft.WallStyles[k], HousePartKind.Wall, theme.Walls) && Is(_draft.FloorStyles[k], HousePartKind.Floor, theme.Floor);
                }

                if (storeys && Is(_draft.RoofStyle, HousePartKind.Roof, theme.Roof) &&
                    Is(_draft.FlatRoofStyle, HousePartKind.Floor, theme.FlatRoof) &&
                    Is(_draft.DoorStyle, HousePartKind.Door, theme.Door) && Is(_draft.StairStyle, HousePartKind.Stair, theme.Stairs) &&
                    _draft.FoundationStyle == Context.HouseStyles.Find(HousePartKind.Wall, theme.Foundation) &&
                    _draft.FoundationTiers == theme.FoundationTiers && _draft.FoundationHeight == theme.FoundationHeight)
                {
                    return theme;
                }
            }

            return null;
        }

        /// <summary>Sets the floor style flat roofs are laid in.</summary>
        public void SetFlatRoofStyle(HouseStyle style)
        {
            if (style is { Kind: HousePartKind.Floor })
            {
                Edit(() => _draft.FlatRoofStyle = style);
            }
        }
    }
}
