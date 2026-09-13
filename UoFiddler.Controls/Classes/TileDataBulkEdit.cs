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
using System.Text;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Sparse description of an edit to an <see cref="ItemData"/> entry. Every member
    /// left null - and every flag left out of both masks - means "leave the target's
    /// current value alone". That is what lets a single edit be applied across a whole
    /// selection without flattening the fields the user never touched.
    /// </summary>
    public sealed class ItemDataEdit
    {
        public string Name { get; set; }
        public short? Animation { get; set; }
        public byte? Weight { get; set; }
        public byte? Quality { get; set; }
        public byte? Quantity { get; set; }
        public byte? Hue { get; set; }
        public byte? StackingOffset { get; set; }
        public byte? Value { get; set; }
        public byte? Height { get; set; }
        public short? MiscData { get; set; }
        public byte? Unk2 { get; set; }
        public byte? Unk3 { get; set; }

        /// <summary>Flags to turn on for every target.</summary>
        public TileFlag SetFlags { get; set; }

        /// <summary>Flags to turn off for every target.</summary>
        public TileFlag ClearFlags { get; set; }
    }

    /// <summary>
    /// Sparse description of an edit to a <see cref="LandData"/> entry.
    /// See <see cref="ItemDataEdit"/> for the "null means leave alone" contract.
    /// </summary>
    public sealed class LandDataEdit
    {
        public string Name { get; set; }
        public ushort? TextureId { get; set; }
        public TileFlag SetFlags { get; set; }
        public TileFlag ClearFlags { get; set; }
    }

    /// <summary>
    /// Applies sparse tiledata edits and describes them for confirmation prompts.
    /// Shared by the TileData tab's multi-selection "Save Changes" and by
    /// "Paste special", so both routes behave identically.
    /// </summary>
    public static class TileDataBulkEdit
    {
        /// <summary>tiledata.mul stores names as 20 ASCII bytes.</summary>
        public const int MaxNameLength = 20;

        /// <summary>How many flag names a description spells out before it gives up and counts.</summary>
        private const int MaxDescribedFlags = 6;

        public static ItemData Apply(ItemData data, ItemDataEdit edit)
        {
            if (edit == null)
            {
                return data;
            }

            if (edit.Name != null)
            {
                data.Name = TruncateName(edit.Name);
            }

            if (edit.Animation.HasValue)
            {
                data.Animation = edit.Animation.Value;
            }

            if (edit.Weight.HasValue)
            {
                data.Weight = edit.Weight.Value;
            }

            if (edit.Quality.HasValue)
            {
                data.Quality = edit.Quality.Value;
            }

            if (edit.Quantity.HasValue)
            {
                data.Quantity = edit.Quantity.Value;
            }

            if (edit.Hue.HasValue)
            {
                data.Hue = edit.Hue.Value;
            }

            if (edit.StackingOffset.HasValue)
            {
                data.StackingOffset = edit.StackingOffset.Value;
            }

            if (edit.Value.HasValue)
            {
                data.Value = edit.Value.Value;
            }

            if (edit.Height.HasValue)
            {
                data.Height = edit.Height.Value;
            }

            if (edit.MiscData.HasValue)
            {
                data.MiscData = edit.MiscData.Value;
            }

            if (edit.Unk2.HasValue)
            {
                data.Unk2 = edit.Unk2.Value;
            }

            if (edit.Unk3.HasValue)
            {
                data.Unk3 = edit.Unk3.Value;
            }

            data.Flags = (data.Flags | edit.SetFlags) & ~edit.ClearFlags;

            return data;
        }

        public static LandData Apply(LandData data, LandDataEdit edit)
        {
            if (edit == null)
            {
                return data;
            }

            if (edit.Name != null)
            {
                data.Name = TruncateName(edit.Name);
            }

            if (edit.TextureId.HasValue)
            {
                data.TextureId = edit.TextureId.Value;
            }

            data.Flags = (data.Flags | edit.SetFlags) & ~edit.ClearFlags;

            return data;
        }

        public static bool IsEmpty(ItemDataEdit edit)
        {
            return edit == null
                   || (edit.Name == null
                       && !edit.Animation.HasValue
                       && !edit.Weight.HasValue
                       && !edit.Quality.HasValue
                       && !edit.Quantity.HasValue
                       && !edit.Hue.HasValue
                       && !edit.StackingOffset.HasValue
                       && !edit.Value.HasValue
                       && !edit.Height.HasValue
                       && !edit.MiscData.HasValue
                       && !edit.Unk2.HasValue
                       && !edit.Unk3.HasValue
                       && edit.SetFlags == TileFlag.None
                       && edit.ClearFlags == TileFlag.None);
        }

        public static bool IsEmpty(LandDataEdit edit)
        {
            return edit == null
                   || (edit.Name == null
                       && !edit.TextureId.HasValue
                       && edit.SetFlags == TileFlag.None
                       && edit.ClearFlags == TileFlag.None);
        }

        /// <summary>
        /// Human readable summary of what an edit will change, e.g.
        /// "Name, Weight, Height, +Impassable, -Wall". Used in the confirmation prompt
        /// so a bulk write always says what it is about to touch.
        /// </summary>
        public static string Describe(ItemDataEdit edit)
        {
            if (IsEmpty(edit))
            {
                return string.Empty;
            }

            var parts = new List<string>();

            AddIf(parts, edit.Name != null, "Name");
            AddIf(parts, edit.Animation.HasValue, "Anim");
            AddIf(parts, edit.Weight.HasValue, "Weight");
            AddIf(parts, edit.Quality.HasValue, "Layer");
            AddIf(parts, edit.Quantity.HasValue, "Quantity");
            AddIf(parts, edit.Hue.HasValue, "Hue");
            AddIf(parts, edit.StackingOffset.HasValue, "StackOff");
            AddIf(parts, edit.Value.HasValue, "Value");
            AddIf(parts, edit.Height.HasValue, "Height");
            AddIf(parts, edit.MiscData.HasValue, "MiscData");
            AddIf(parts, edit.Unk2.HasValue, "Unk2");
            AddIf(parts, edit.Unk3.HasValue, "Unk3");

            AppendFlags(parts, edit.SetFlags, '+');
            AppendFlags(parts, edit.ClearFlags, '-');

            return string.Join(", ", parts);
        }

        public static string Describe(LandDataEdit edit)
        {
            if (IsEmpty(edit))
            {
                return string.Empty;
            }

            var parts = new List<string>();

            AddIf(parts, edit.Name != null, "Name");
            AddIf(parts, edit.TextureId.HasValue, "TexID");

            AppendFlags(parts, edit.SetFlags, '+');
            AppendFlags(parts, edit.ClearFlags, '-');

            return string.Join(", ", parts);
        }

        public static string TruncateName(string name)
        {
            if (name == null)
            {
                return string.Empty;
            }

            return name.Length > MaxNameLength ? name.Substring(0, MaxNameLength) : name;
        }

        private static void AddIf(List<string> parts, bool condition, string text)
        {
            if (condition)
            {
                parts.Add(text);
            }
        }

        private static void AppendFlags(List<string> parts, TileFlag flags, char prefix)
        {
            if (flags == TileFlag.None)
            {
                return;
            }

            Array enumValues = Enum.GetValues(typeof(TileFlag));
            var builder = new StringBuilder();
            int named = 0;
            int total = 0;

            for (int i = 1; i < enumValues.Length; ++i)
            {
                var flag = (TileFlag)enumValues.GetValue(i);
                if ((flags & flag) == 0)
                {
                    continue;
                }

                ++total;
                if (named >= MaxDescribedFlags)
                {
                    continue;
                }

                if (named > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(prefix).Append(flag);
                ++named;
            }

            if (total == 0)
            {
                return;
            }

            if (total > named)
            {
                builder.Append(", ").Append(prefix).Append('(').Append(total - named).Append(" more)");
            }

            parts.Add(builder.ToString());
        }
    }
}
