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
using System.Globalization;
using System.Linq;
using System.Text;

namespace UoFiddler.Plugin.MultiEditor.Model
{
    /// <summary>
    /// A copied group of tiles, relative to its lowest X, Y and Z. The Dynamic flag and extra field
    /// travel with each tile, so a paste matches what was copied.
    /// </summary>
    internal sealed record ClipboardTile(ushort Id, int Dx, int Dy, int Dz, bool Invisible, int Extra)
    {
        /// <summary>
        /// Multi text lines (<c>0xID x y z flag</c>, as the TXT export writes them) with the extra
        /// field appended, which the TXT import ignores.
        /// </summary>
        public static string Format(IEnumerable<ClipboardTile> tiles)
        {
            var text = new StringBuilder();
            foreach (ClipboardTile t in tiles)
            {
                text.Append(CultureInfo.InvariantCulture, $"0x{t.Id:X4} {t.Dx} {t.Dy} {t.Dz} {(t.Invisible ? 0 : 1)} {t.Extra}").Append("\r\n");
            }

            return text.ToString();
        }

        /// <summary>
        /// Reads multi text lines back, moved so the lowest X, Y and Z are 0. Returns null when any
        /// non-empty line is not a tile line, so ordinary text on the clipboard is left alone.
        /// </summary>
        public static List<ClipboardTile> Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var tiles = new List<ClipboardTile>();
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5 || parts.Length > 6)
                {
                    return null;
                }

                string idText = parts[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? parts[0][2..] : parts[0];
                if (!ushort.TryParse(idText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort id) ||
                    !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) ||
                    !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y) ||
                    !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int z) ||
                    !int.TryParse(parts[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int flag))
                {
                    return null;
                }

                int extra = 0;
                if (parts.Length == 6 && !int.TryParse(parts[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out extra))
                {
                    return null;
                }

                tiles.Add(new ClipboardTile(id, x, y, z, flag == 0, extra));
            }

            if (tiles.Count == 0)
            {
                return null;
            }

            int minX = tiles.Min(t => t.Dx);
            int minY = tiles.Min(t => t.Dy);
            int minZ = tiles.Min(t => t.Dz);
            return tiles.Select(t => t with { Dx = t.Dx - minX, Dy = t.Dy - minY, Dz = t.Dz - minZ }).ToList();
        }
    }
}
