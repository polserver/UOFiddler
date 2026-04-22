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
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareCliLocControl : UserControl
    {
        public CompareCliLocControl()
        {
            InitializeComponent();
            _source = new BindingSource();
            _sortOrder = SortOrder.Ascending;
            _sortColumn = 0;
        }

        private static StringList _cliloc1;
        private static StringList _cliloc2;
        private static BindingSource _source;
        private static readonly Dictionary<int, CompareEntry> _compareList = new Dictionary<int, CompareEntry>();
        private static List<CompareEntry> _list = new List<CompareEntry>();
        private static bool _showOnlyDiff;

        private SortOrder _sortOrder;
        private int _sortColumn;

        private void OnLoad(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }

            string path = textBox1.Text;
            if (!File.Exists(path))
            {
                return;
            }

            _cliloc1 = new StringList("1", path, false);
            _cliloc1.Entries.Sort(new StringList.NumberComparer(false));

            if (_cliloc2 != null)
            {
                BuildList();
            }
        }

        private void OnLoad2(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text))
            {
                return;
            }

            string path = textBox2.Text;
            if (!File.Exists(path))
            {
                return;
            }

            _cliloc2 = new StringList("2", path, false);
            _cliloc2.Entries.Sort(new StringList.NumberComparer(false));

            if (_cliloc1 != null)
            {
                BuildList();
            }
        }

        private void BuildList()
        {
            if (_cliloc1 == null || _cliloc2 == null)
            {
                return;
            }

            foreach (var stringEntry in _cliloc1.Entries)
            {
                CompareEntry entry = new CompareEntry { CompareResult = CompareEntry.CompareRes.NewIn1 };
                StringEntry entr = stringEntry;
                entry.Number = entr.Number;
                entry.Text1 = entr.Text;
                entry.Text2 = string.Empty;
                _compareList.Add(entry.Number, entry);
            }

            foreach (var stringEntry in _cliloc2.Entries)
            {
                if (_compareList.ContainsKey(stringEntry.Number))
                {
                    CompareEntry entr1 = _compareList[stringEntry.Number];
                    entr1.Text2 = stringEntry.Text;
                    entr1.CompareResult = entr1.Text1 != stringEntry.Text
                        ? CompareEntry.CompareRes.Diff
                        : CompareEntry.CompareRes.Equal;
                }
                else
                {
                    CompareEntry entry = new CompareEntry
                    {
                        CompareResult = CompareEntry.CompareRes.NewIn2,
                        Number = stringEntry.Number,
                        Text1 = string.Empty,
                        Text2 = stringEntry.Text
                    };
                    _compareList.Add(entry.Number, entry);
                }
            }

            _list = new List<CompareEntry>();

            foreach (KeyValuePair<int, CompareEntry> key in _compareList)
            {
                if (_showOnlyDiff)
                {
                    if (key.Value.CompareResult == CompareEntry.CompareRes.Equal)
                    {
                        continue;
                    }
                }

                _list.Add(key.Value);
            }

            switch (_sortColumn)
            {
                case 0:
                    _list.Sort(new NumberComparer(_sortOrder == SortOrder.Descending));
                    break;
                case 1:
                    _list.Sort(new TextComparer1(_sortOrder == SortOrder.Descending));
                    break;
                case 2:
                    _list.Sort(new TextComparer2(_sortOrder == SortOrder.Descending));
                    break;
                case 3:
                    _list.Sort(new FlagComparer(_sortOrder == SortOrder.Descending));
                    break;
            }

            _compareList.Clear();
            _source = new BindingSource { DataSource = _list };
            dataGridView1.DataSource = _source;

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                dataGridView1.Columns[0].Width = 60;
                dataGridView1.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[3].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[3].Width = 105;
            }

            dataGridView1.Invalidate();
        }

        private void CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 1 || e.ColumnIndex == 2) // text1 & text2
            {
                return;
            }

            CompareEntry entry = _list[e.RowIndex];

            switch (entry.CompareResult)
            {
                case CompareEntry.CompareRes.Diff:
                    e.CellStyle.ForeColor = Color.Red;
                    break;
                case CompareEntry.CompareRes.NewIn1:
                    e.CellStyle.ForeColor = Color.Blue;
                    break;
                case CompareEntry.CompareRes.NewIn2:
                    e.CellStyle.ForeColor = Color.Orange;
                    break;
                case CompareEntry.CompareRes.Equal:
                default:
                    break;
            }
        }

        private void OnClickDirFile1(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose Cliloc file to open",
                CheckFileExists = true,
                Filter = "cliloc files (cliloc.*)|cliloc.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
            }

            dialog.Dispose();
        }

        private void OnClickDirFile2(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose Cliloc file to open",
                CheckFileExists = true,
                Filter = "cliloc files (cliloc.*)|cliloc.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dialog.FileName;
            }

            dialog.Dispose();
        }

        private void OnClickShowOnlyDiff(object sender, EventArgs e)
        {
            _showOnlyDiff = !_showOnlyDiff;
            BuildList();
        }

        private void OnClickFindNextDiff(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount <= 0)
            {
                return;
            }

            int i;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                i = dataGridView1.SelectedRows[0].Index + 1;
            }
            else
            {
                i = 0;
            }

            for (; i < dataGridView1.RowCount; i++)
            {
                if ((CompareEntry.CompareRes)dataGridView1.Rows[i].Cells[3].Value == CompareEntry.CompareRes.Equal)
                {
                    continue;
                }

                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;
                break;
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                diffRichTextBox1.Clear();
                diffRichTextBox2.Clear();
                return;
            }

            var entry = dataGridView1.SelectedRows[0].DataBoundItem as CompareEntry;
            if (entry == null)
            {
                return;
            }

            ShowWordDiff(entry.Text1 ?? string.Empty, entry.Text2 ?? string.Empty);
        }

        private void ShowWordDiff(string text1, string text2)
        {
            diffRichTextBox1.Clear();
            diffRichTextBox2.Clear();

            if (text1 == text2)
            {
                diffRichTextBox1.Text = text1;
                diffRichTextBox2.Text = text2;
                return;
            }

            var diff = ComputeWordDiff(text1, text2);
            bool first1 = true, first2 = true;

            foreach (var (op, word) in diff)
            {
                switch (op)
                {
                    case WordDiffOp.Equal:
                        AppendWord(diffRichTextBox1, word, SystemColors.Window, ref first1);
                        AppendWord(diffRichTextBox2, word, SystemColors.Window, ref first2);
                        break;
                    case WordDiffOp.Delete:
                        AppendWord(diffRichTextBox1, word, Color.FromArgb(255, 200, 200), ref first1);
                        break;
                    case WordDiffOp.Insert:
                        AppendWord(diffRichTextBox2, word, Color.FromArgb(200, 255, 200), ref first2);
                        break;
                }
            }

            diffRichTextBox1.SelectionStart = 0;
            diffRichTextBox1.SelectionLength = 0;
            diffRichTextBox2.SelectionStart = 0;
            diffRichTextBox2.SelectionLength = 0;
        }

        private static void AppendWord(RichTextBox rtb, string word, Color backColor, ref bool isFirst)
        {
            rtb.SelectionStart = rtb.TextLength;
            rtb.SelectionLength = 0;
            rtb.SelectionBackColor = backColor;
            rtb.SelectionColor = SystemColors.WindowText;
            rtb.SelectedText = isFirst ? word : " " + word;
            isFirst = false;
        }

        private enum WordDiffOp { Equal, Delete, Insert }

        private static List<(WordDiffOp Op, string Word)> ComputeWordDiff(string text1, string text2)
        {
            string[] a = text1.Split(' ');
            string[] b = text2.Split(' ');
            int m = a.Length, n = b.Length;

            var dp = new int[m + 1, n + 1];
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    dp[i, j] = a[i - 1] == b[j - 1]
                        ? dp[i - 1, j - 1] + 1
                        : Math.Max(dp[i - 1, j], dp[i, j - 1]);
                }
            }

            var result = new List<(WordDiffOp, string)>();
            int ii = m, jj = n;
            while (ii > 0 || jj > 0)
            {
                if (ii > 0 && jj > 0 && a[ii - 1] == b[jj - 1])
                {
                    result.Add((WordDiffOp.Equal, a[ii - 1]));
                    ii--;
                    jj--;
                }
                else if (jj > 0 && (ii == 0 || dp[ii, jj - 1] >= dp[ii - 1, jj]))
                {
                    result.Add((WordDiffOp.Insert, b[jj - 1]));
                    jj--;
                }
                else
                {
                    result.Add((WordDiffOp.Delete, a[ii - 1]));
                    ii--;
                }
            }

            result.Reverse();
            return result;
        }

        private void OnHeaderClicked(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (_sortColumn == e.ColumnIndex)
            {
                _sortOrder = _sortOrder == SortOrder.Ascending
                    ? SortOrder.Descending
                    : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
                dataGridView1.Columns[_sortColumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            }

            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = _sortOrder;
            _sortColumn = e.ColumnIndex;

            switch (_sortColumn)
            {
                case 0:
                    _list.Sort(new NumberComparer(_sortOrder == SortOrder.Descending));
                    break;
                case 1:
                    _list.Sort(new TextComparer1(_sortOrder == SortOrder.Descending));
                    break;
                case 2:
                    _list.Sort(new TextComparer2(_sortOrder == SortOrder.Descending));
                    break;
                case 3:
                    _list.Sort(new FlagComparer(_sortOrder == SortOrder.Descending));
                    break;
            }

            dataGridView1.Invalidate();
        }
    }

    public class CompareEntry
    {
        [Flags]
        public enum CompareRes
        {
            Diff = 0x0,
            NewIn1 = 0x1,
            NewIn2 = 0x2,
            Equal = NewIn1 | NewIn2
        }

        public int Number { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public CompareRes CompareResult { get; set; }
    }

    public class NumberComparer : IComparer<CompareEntry>
    {
        private readonly bool _sortDescending;

        public NumberComparer(bool sortDescending)
        {
            _sortDescending = sortDescending;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            if (objA.Number == objB.Number)
            {
                return 0;
            }

            if (_sortDescending)
            {
                return objA.Number < objB.Number ? 1 : -1;
            }
            else
            {
                return objA.Number < objB.Number ? -1 : 1;
            }
        }
    }

    public class FlagComparer : IComparer<CompareEntry>
    {
        private readonly bool _sortDescending;

        public FlagComparer(bool sortDescending)
        {
            _sortDescending = sortDescending;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            if ((byte)objA.CompareResult == (byte)objB.CompareResult)
            {
                if (objA.Number == objB.Number)
                {
                    return 0;
                }

                if (_sortDescending)
                {
                    return objA.Number < objB.Number ? 1 : -1;
                }

                return objA.Number < objB.Number ? -1 : 1;
            }

            if (_sortDescending)
            {
                return (byte)objA.CompareResult < (byte)objB.CompareResult ? 1 : -1;
            }
            else
            {
                return (byte)objA.CompareResult < (byte)objB.CompareResult ? -1 : 1;
            }
        }
    }

    public class TextComparer1 : IComparer<CompareEntry>
    {
        private readonly bool _sortDescending;

        public TextComparer1(bool sortDescending)
        {
            _sortDescending = sortDescending;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            return _sortDescending
                ? string.CompareOrdinal(objB.Text1, objA.Text1)
                : string.CompareOrdinal(objA.Text1, objB.Text1);
        }
    }

    public class TextComparer2 : IComparer<CompareEntry>
    {
        private readonly bool _sortDescending;

        public TextComparer2(bool sortDescending)
        {
            _sortDescending = sortDescending;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            return _sortDescending
                ? string.CompareOrdinal(objB.Text2, objA.Text2)
                : string.CompareOrdinal(objA.Text2, objB.Text2);
        }
    }
}