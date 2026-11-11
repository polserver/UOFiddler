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
using System.Drawing;
using System.Globalization;
using System.Text;
using Ultima.Helpers;

namespace UoFiddler.Controls.Classes
{
    /// <summary>Reads and writes hue colors as text for the hue editor's typed input.</summary>
    public static class HueColorText
    {
        public static string ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        public static string Describe(ushort value)
        {
            Color color = HueHelpers.HueToColor(value);
            HueHelpers.HueExtract5(value, out int r5, out int g5, out int b5);
            return $"{ToHex(color)}  RGB {color.R}, {color.G}, {color.B}  (levels {r5}, {g5}, {b5})  0x{value:X4}";
        }

        /// <summary>
        /// Accepts "#RRGGBB", "RRGGBB", "r, g, b" (0-255, separated by commas, semicolons or spaces)
        /// and a raw 15-bit hue color written as "0x7FFF".
        /// </summary>
        public static bool TryParse(string text, out Color color)
        {
            color = Color.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            text = text.Trim();

            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (ushort.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort raw) && raw <= 0x7FFF)
                {
                    color = HueHelpers.HueToColor(raw);
                    return true;
                }

                return false;
            }

            string[] parts = text.Split([',', ';', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3)
            {
                if (TryByte(parts[0], out int r) && TryByte(parts[1], out int g) && TryByte(parts[2], out int b))
                {
                    color = Color.FromArgb(r, g, b);
                    return true;
                }

                return false;
            }

            string hex = text.TrimStart('#');
            if (hex.Length == 6 && int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int rgb))
            {
                color = Color.FromArgb((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
                return true;
            }

            return false;
        }

        /// <summary>All 32 colors as "#RRGGBB" lines, darkest index first.</summary>
        public static string FormatList(IReadOnlyList<ushort> colors)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < colors.Count; i++)
            {
                builder.AppendLine(ToHex(HueHelpers.HueToColor(colors[i])));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reads one color per non-empty line (any <see cref="TryParse"/> form; "//" starts a
        /// comment). Exactly <paramref name="count"/> colors are required.
        /// </summary>
        public static bool TryParseList(string text, int count, out ushort[] colors, out string error)
        {
            colors = new ushort[count];
            if (!TryParseLines(text, out List<ushort> found, out error))
            {
                return false;
            }

            if (found.Count != count)
            {
                error = found.Count > count
                    ? $"There are more than {count} colors."
                    : $"Found {found.Count} colors; a hue needs exactly {count}.";
                return false;
            }

            found.CopyTo(colors);
            return true;
        }

        /// <summary>
        /// Reads one color per non-empty line (any <see cref="TryParse"/> form; "//" starts a
        /// comment). Fails on the first line that is not a color.
        /// </summary>
        public static bool TryParseLines(string text, out List<ushort> colors, out string error)
        {
            colors = [];
            error = null;
            string[] lines = (text ?? string.Empty).Split('\n');
            for (int line = 0; line < lines.Length; line++)
            {
                string value = lines[line];
                int comment = value.IndexOf("//", StringComparison.Ordinal);
                if (comment >= 0)
                {
                    value = value[..comment];
                }

                value = value.Trim();
                if (value.Length == 0)
                {
                    continue;
                }

                if (!TryParse(value, out Color color))
                {
                    error = $"Line {line + 1}: \"{value}\" is not a color.";
                    return false;
                }

                colors.Add(HueRamp.Pack(color));
            }

            return true;
        }

        private static bool TryByte(string text, out int value)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value) && value is >= 0 and <= 255;
        }
    }
}
