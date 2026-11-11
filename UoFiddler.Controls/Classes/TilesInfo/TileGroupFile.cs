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
using System.Linq;
using System.Text;

namespace UoFiddler.Controls.Classes.TilesInfo
{
    /// <summary>One line of a tile group file: its text and the line break that followed it.</summary>
    public readonly record struct TileGroupLine(string Text, string Ending);

    public sealed class TileGroupFile
    {
        private readonly List<TileGroupLine> _preamble;
        private readonly Dictionary<string[], string> _rowEndings;
        private readonly string _defaultEnding;
        private readonly bool _endsWithNewline;

        public string FilePath { get; }
        public string DisplayName { get; }
        public string[] Headers { get; }
        public string[] TypeRow { get; }
        public List<string[]> RawRows { get; }
        public List<TileCategory> Categories { get; } = new List<TileCategory>();
        public bool HasStyleColumn { get; }
        public bool HasTidColumn { get; }
        public int CommentColumnIndex { get; }
        public bool IsDirty { get; set; }

        public TileGroupFile(string filePath, string displayName, string[] typeRow, string[] headers,
            List<string[]> rawRows, bool hasStyleColumn, bool hasTidColumn, int commentColumnIndex,
            List<TileGroupLine> preamble, Dictionary<string[], string> rowEndings, string defaultEnding, bool endsWithNewline)
        {
            _preamble = preamble;
            _rowEndings = rowEndings;
            _defaultEnding = defaultEnding;
            _endsWithNewline = endsWithNewline;
            FilePath = filePath;
            DisplayName = displayName;
            TypeRow = typeRow;
            Headers = headers;
            RawRows = rawRows;
            HasStyleColumn = hasStyleColumn;
            HasTidColumn = hasTidColumn;
            CommentColumnIndex = commentColumnIndex;
        }

        public void AddCategory(TileCategory category)
        {
            category.File = this;
            Categories.Add(category);
        }

        public int NextCategoryIndex()
        {
            int max = -1;
            foreach (var c in Categories)
            {
                if (c.Index > max) max = c.Index;
            }
            return max + 1;
        }

        public int NextStyleIndex(TileCategory category)
        {
            int max = -1;
            foreach (var s in category.Styles)
            {
                if (s.Index > max) max = s.Index;
            }
            return max + 1;
        }

        public TileCategory CreateNewCategory(string name, int tid)
        {
            int newCategoryIndex = NextCategoryIndex();
            string[] row = BuildBlankRow();
            row[0] = newCategoryIndex.ToString(CultureInfo.InvariantCulture);
            if (HasStyleColumn)
            {
                row[1] = "0";
                if (HasTidColumn)
                {
                    row[2] = tid.ToString(CultureInfo.InvariantCulture);
                }
            }
            row[CommentColumnIndex] = name;
            RawRows.Add(row);

            var category = new TileCategory(newCategoryIndex, name) { Tid = HasTidColumn ? tid : 0 };
            AddCategory(category);

            var style = new TileStyle(HasStyleColumn ? 0 : -1, name);
            AttachSlotsToRow(style, row);
            category.AddStyle(style);

            IsDirty = true;
            return category;
        }

        public TileStyle CreateNewStyle(TileCategory category, string name)
        {
            if (!HasStyleColumn)
            {
                return null;
            }

            int newStyleIndex = NextStyleIndex(category);
            string[] row = BuildBlankRow();
            row[0] = category.Index.ToString(CultureInfo.InvariantCulture);
            row[1] = newStyleIndex.ToString(CultureInfo.InvariantCulture);
            row[CommentColumnIndex] = name;
            InsertRowAfterCategoryBlock(category, row);

            var style = new TileStyle(newStyleIndex, name);
            AttachSlotsToRow(style, row);
            category.AddStyle(style);

            IsDirty = true;
            return style;
        }

        public void RemoveCategory(TileCategory category)
        {
            foreach (var style in category.Styles.ToList())
            {
                RemoveStyleRowOnly(style);
            }
            Categories.Remove(category);
            IsDirty = true;
        }

        public void RemoveStyle(TileStyle style)
        {
            var category = style.Category;
            RemoveStyleRowOnly(style);

            if (category != null)
            {
                category.Styles.Remove(style);
                if (category.Styles.Count == 0)
                {
                    Categories.Remove(category);
                }
            }
            IsDirty = true;
        }

        public void RenameStyle(TileStyle style, string newName)
        {
            string[] row = FindRowForStyle(style);
            if (row != null && CommentColumnIndex < row.Length)
            {
                row[CommentColumnIndex] = newName;
            }
            style.Name = newName;
            if (style.Category != null && style.Category.Styles.Count > 0 && ReferenceEquals(style.Category.Styles[0], style))
            {
                style.Category.Name = newName;
            }
            IsDirty = true;
        }

        public void UpdateCategoryTid(TileCategory category, int tid)
        {
            if (!HasTidColumn || category.Styles.Count == 0)
            {
                return;
            }

            string[] firstRow = FindRowForStyle(category.Styles[0]);
            if (firstRow != null && firstRow.Length > 2)
            {
                firstRow[2] = tid.ToString(CultureInfo.InvariantCulture);
            }
            category.Tid = tid;
            IsDirty = true;
        }

        private string[] BuildBlankRow()
        {
            var row = new string[Headers.Length];
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = "0";
            }
            row[CommentColumnIndex] = string.Empty;
            return row;
        }

        private void AttachSlotsToRow(TileStyle style, string[] row)
        {
            int firstSlotColumn = HasStyleColumn ? (HasTidColumn ? 3 : 2) : 1;
            int lastSlotColumnExclusive = CommentColumnIndex;
            for (int col = CommentColumnIndex - 1; col >= 0; col--)
            {
                if (Headers[col].Equals("FeatureMask", System.StringComparison.OrdinalIgnoreCase))
                {
                    lastSlotColumnExclusive = col;
                    break;
                }
            }

            for (int col = firstSlotColumn; col < lastSlotColumnExclusive; col++)
            {
                style.AddSlot(new TileSlot(Headers[col], row, col));
            }
        }

        private void InsertRowAfterCategoryBlock(TileCategory category, string[] newRow)
        {
            int lastIndex = -1;
            for (int i = 0; i < RawRows.Count; i++)
            {
                var r = RawRows[i];
                if (r.Length > 0 && int.TryParse(r[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int cat) && cat == category.Index)
                {
                    lastIndex = i;
                }
            }

            if (lastIndex < 0)
            {
                RawRows.Add(newRow);
            }
            else
            {
                RawRows.Insert(lastIndex + 1, newRow);
            }
        }

        private void RemoveStyleRowOnly(TileStyle style)
        {
            string[] row = FindRowForStyle(style);
            if (row != null)
            {
                RawRows.Remove(row);
            }
        }

        private string[] FindRowForStyle(TileStyle style)
        {
            if (style.Slots.Count > 0)
            {
                return style.Slots[0].RawRow;
            }
            return null;
        }

        /// <summary>
        /// Writes the file to <paramref name="path"/>. The lines before the data, every line's own
        /// line break and a missing final newline are kept, so only edited rows differ from the
        /// file that was read.
        /// </summary>
        public void Save(string path)
        {
            var lines = new List<TileGroupLine>(_preamble.Count + RawRows.Count);
            lines.AddRange(_preamble);
            foreach (var row in RawRows)
            {
                lines.Add(new TileGroupLine(string.Join('\t', row), _rowEndings.GetValueOrDefault(row, _defaultEnding)));
            }

            var text = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                text.Append(lines[i].Text);
                bool last = i == lines.Count - 1;
                if (!last || _endsWithNewline)
                {
                    text.Append(lines[i].Ending.Length > 0 ? lines[i].Ending : _defaultEnding);
                }
            }

            string tmp = path + ".tmp";
            File.WriteAllBytes(tmp, Encoding.Latin1.GetBytes(text.ToString()));
            if (File.Exists(path))
            {
                File.Replace(tmp, path, null);
            }
            else
            {
                File.Move(tmp, path);
            }

            IsDirty = false;
        }
    }
}
