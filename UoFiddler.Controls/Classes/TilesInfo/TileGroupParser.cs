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
using System.Globalization;
using System.IO;
using System.Text;

namespace UoFiddler.Controls.Classes.TilesInfo
{
    public static class TileGroupParser
    {
        /// <summary>
        /// Reads a client tile group file (walls.txt and the like). Returns null when the file has
        /// no type row and header row. Lines keep their own endings so an unedited file saves back
        /// byte for byte.
        /// </summary>
        public static TileGroupFile Parse(string filePath, string displayName)
        {
            // Latin-1 maps every byte to one char and back, whatever code page the client used.
            string text = Encoding.Latin1.GetString(File.ReadAllBytes(filePath));
            List<TileGroupLine> lines = SplitLines(text, out bool endsWithNewline);

            int i = 0;
            while (i < lines.Count && IsBlank(lines[i].Text)) i++;
            if (i >= lines.Count)
            {
                return null;
            }

            string[] typeRow = lines[i].Text.Split('\t');
            i++;

            while (i < lines.Count && IsBlank(lines[i].Text)) i++;
            if (i >= lines.Count)
            {
                return null;
            }

            string[] headers = lines[i].Text.Split('\t');
            i++;

            // Everything up to the header row is written back exactly as it was read.
            List<TileGroupLine> preamble = lines.GetRange(0, i);

            int commentColumnIndex = headers.Length - 1;
            bool hasStyleColumn = headers.Length > 1 && headers[1].Equals("Style", System.StringComparison.OrdinalIgnoreCase);
            bool hasTidColumn = hasStyleColumn && headers.Length > 2 && headers[2].Equals("TID", System.StringComparison.OrdinalIgnoreCase);

            int firstSlotColumn = hasStyleColumn ? (hasTidColumn ? 3 : 2) : 1;
            int lastSlotColumnExclusive = ResolveFeatureMaskBoundary(headers, commentColumnIndex);

            var rawRows = new List<string[]>(lines.Count - i);
            var rowEndings = new Dictionary<string[], string>(ReferenceEqualityComparer.Instance);
            string defaultEnding = MostCommonEnding(lines);
            for (; i < lines.Count; i++)
            {
                string[] row = lines[i].Text.Split('\t');
                rawRows.Add(row);
                rowEndings[row] = lines[i].Ending.Length > 0 ? lines[i].Ending : defaultEnding;
            }

            var file = new TileGroupFile(filePath, displayName, typeRow, headers, rawRows,
                hasStyleColumn, hasTidColumn, commentColumnIndex, preamble, rowEndings, defaultEnding, endsWithNewline);

            TileCategory currentCategory = null;
            foreach (var row in rawRows)
            {
                if (!IsDataRow(row, commentColumnIndex))
                {
                    continue;
                }

                int categoryIndex = ParseInt(row[0]);
                string comment = row.Length > commentColumnIndex ? row[commentColumnIndex] : string.Empty;

                if (hasStyleColumn)
                {
                    int styleIndex = ParseInt(row[1]);
                    int tid = hasTidColumn ? ParseInt(row[2]) : 0;

                    if (styleIndex == 0 || currentCategory == null || currentCategory.Index != categoryIndex)
                    {
                        currentCategory = new TileCategory(categoryIndex, comment) { Tid = tid };
                        file.AddCategory(currentCategory);
                    }
                    else if (tid != 0 && currentCategory.Tid == 0)
                    {
                        currentCategory.Tid = tid;
                    }

                    var style = new TileStyle(styleIndex, comment);
                    AddSlots(style, headers, row, firstSlotColumn, lastSlotColumnExclusive);
                    currentCategory.AddStyle(style);
                }
                else
                {
                    currentCategory = new TileCategory(categoryIndex, comment);
                    file.AddCategory(currentCategory);

                    var style = new TileStyle(-1, comment);
                    AddSlots(style, headers, row, firstSlotColumn, lastSlotColumnExclusive);
                    currentCategory.AddStyle(style);
                }
            }

            return file;
        }

        /// <summary>
        /// Splits on '\n'. Carriage returns before it belong to the line's ending, so "\r\n" and
        /// "\r\r\n" both survive a save.
        /// </summary>
        private static List<TileGroupLine> SplitLines(string text, out bool endsWithNewline)
        {
            var lines = new List<TileGroupLine>();
            int start = 0;
            while (start < text.Length)
            {
                int newline = text.IndexOf('\n', start);
                if (newline < 0)
                {
                    lines.Add(new TileGroupLine(text[start..], string.Empty));
                    break;
                }

                int end = newline;
                while (end > start && text[end - 1] == '\r')
                {
                    end--;
                }

                lines.Add(new TileGroupLine(text[start..end], text[end..(newline + 1)]));
                start = newline + 1;
            }

            endsWithNewline = text.Length == 0 || text[^1] == '\n';
            return lines;
        }

        private static string MostCommonEnding(List<TileGroupLine> lines)
        {
            var counts = new Dictionary<string, int>();
            string best = "\r\n";
            int bestCount = 0;
            foreach (TileGroupLine line in lines)
            {
                if (line.Ending.Length == 0)
                {
                    continue;
                }

                counts[line.Ending] = counts.GetValueOrDefault(line.Ending) + 1;
                if (counts[line.Ending] > bestCount)
                {
                    bestCount = counts[line.Ending];
                    best = line.Ending;
                }
            }

            return best;
        }

        private static void AddSlots(TileStyle style, string[] headers, string[] row, int firstColumn, int lastColumnExclusive)
        {
            int end = System.Math.Min(lastColumnExclusive, row.Length);
            for (int col = firstColumn; col < end; col++)
            {
                style.AddSlot(new TileSlot(headers[col], row, col));
            }
        }

        private static int ResolveFeatureMaskBoundary(string[] headers, int commentColumnIndex)
        {
            for (int col = commentColumnIndex - 1; col >= 0; col--)
            {
                if (headers[col].Equals("FeatureMask", System.StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }
            return commentColumnIndex;
        }

        private static bool IsDataRow(string[] row, int commentColumnIndex)
        {
            if (row.Length <= commentColumnIndex)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(row[0]))
            {
                return false;
            }

            return int.TryParse(row[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        private static bool IsBlank(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return true;
            }

            foreach (char c in line)
            {
                if (c != '\t' && !char.IsWhiteSpace(c))
                {
                    return false;
                }
            }
            return true;
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : 0;
        }
    }
}
