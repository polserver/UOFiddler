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

namespace ComparePlugin
{
    public partial class CompareCliLoc : UserControl
    {
        public CompareCliLoc()
        {
            InitializeComponent();
            source = new BindingSource();
            sortorder = SortOrder.Ascending;
            sortcolumn = 0;
        }

        private static StringList cliloc1;
        private static StringList cliloc2;
        private static BindingSource source;
        static Dictionary<int, CompareEntry> comparelist = new Dictionary<int, CompareEntry>();
        static List<CompareEntry> list = new List<CompareEntry>();
        static bool ShowOnlyDiff = false;

        private SortOrder sortorder;
        private int sortcolumn;

        private void OnLoad1(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text))
                return;

            string path = textBox1.Text;
            if (File.Exists(path))
            {
                cliloc1 = new StringList("1", path);
                cliloc1.Entries.Sort(new StringList.NumberComparer(false));
                if (cliloc2 != null)
                    BuildList();
            }
        }

        private void OnLoad2(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox2.Text))
                return;

            string path = textBox2.Text;
            if (File.Exists(path))
            {
                cliloc2 = new StringList("2", path);
                cliloc2.Entries.Sort(new StringList.NumberComparer(false));
                if (cliloc1 != null)
                    BuildList();
            }
        }

        private void BuildList()
        {
            if ((cliloc1 == null) || (cliloc2 == null))
                return;
            for (int i = 0; i < cliloc1.Entries.Count; i++)
            {
                CompareEntry entry = new CompareEntry();
                entry.CompareResult = CompareEntry.CompareRes.NewIn1;
                StringEntry entr = cliloc1.Entries[i];
                entry.Number = entr.Number;
                entry.Text1 = entr.Text;
                entry.Text2 = "";
                comparelist.Add(entry.Number, entry);
            }
            for (int i = 0; i < cliloc2.Entries.Count; i++)
            {
                StringEntry entr = cliloc2.Entries[i];
                if (comparelist.ContainsKey(entr.Number))
                {
                    CompareEntry entr1 = comparelist[entr.Number];
                    entr1.Text2 = entr.Text;
                    if (entr1.Text1 != entr.Text)
                        entr1.CompareResult = CompareEntry.CompareRes.Diff;
                    else
                        entr1.CompareResult = CompareEntry.CompareRes.Equal;
                }
                else
                {
                    CompareEntry entry = new CompareEntry();
                    entry.CompareResult = CompareEntry.CompareRes.NewIn2;
                    entry.Number = entr.Number;
                    entry.Text1 = "";
                    entry.Text2 = entr.Text;
                    comparelist.Add(entry.Number, entry);
                }
            }
            list = new List<CompareEntry>();

            foreach (KeyValuePair<int, CompareEntry> key in comparelist)
            {
                if (ShowOnlyDiff)
                {
                    if (key.Value.CompareResult == CompareEntry.CompareRes.Equal)
                        continue;
                }
                list.Add(key.Value);

            }
            switch (sortcolumn)
            {
                case 0: list.Sort(new NumberComparer(sortorder == SortOrder.Descending)); break;
                case 1: list.Sort(new TextComparer1(sortorder == SortOrder.Descending)); break;
                case 2: list.Sort(new TextComparer2(sortorder == SortOrder.Descending)); break;
                case 3: list.Sort(new FlagComparer(sortorder == SortOrder.Descending)); break;
            }
            comparelist.Clear();
            source = new BindingSource();
            source.DataSource = list;
            dataGridView1.DataSource = source;

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
            if ((e.ColumnIndex == 1) || (e.ColumnIndex == 2)) //text1 & text2
                return;
            CompareEntry entry = list[e.RowIndex];
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
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose Cliloc file to open";
            dialog.CheckFileExists = true;
            dialog.Filter = "cliloc files (cliloc.*)|cliloc.*";
            if (dialog.ShowDialog() == DialogResult.OK)
                textBox1.Text = dialog.FileName;
            dialog.Dispose();
        }

        private void OnClickDirFile2(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose Cliloc file to open";
            dialog.CheckFileExists = true;
            dialog.Filter = "cliloc files (cliloc.*)|cliloc.*";
            if (dialog.ShowDialog() == DialogResult.OK)
                textBox2.Text = dialog.FileName;
            dialog.Dispose();
        }

        private void OnClickShowOnlyDiff(object sender, EventArgs e)
        {
            ShowOnlyDiff = !ShowOnlyDiff;
            BuildList();
        }

        private void OnClickFindNextDiff(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount > 0)
            {
                int i;
                if (dataGridView1.SelectedRows.Count > 0)
                    i = dataGridView1.SelectedRows[0].Index + 1;
                else
                    i = 0;
                for (; i < dataGridView1.RowCount; i++)
                {
                    if ((CompareEntry.CompareRes)dataGridView1.Rows[i].Cells[3].Value != CompareEntry.CompareRes.Equal)
                    {
                        dataGridView1.Rows[i].Selected = true;
                        dataGridView1.FirstDisplayedScrollingRowIndex = i;
                        break;
                    }
                }
            }
        }

        private void OnHeaderClicked(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (sortcolumn == e.ColumnIndex)
            {
                if (sortorder == SortOrder.Ascending)
                    sortorder = SortOrder.Descending;
                else
                    sortorder = SortOrder.Ascending;
            }
            else
            {
                sortorder = SortOrder.Ascending;
                dataGridView1.Columns[sortcolumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            }
            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = sortorder;
            sortcolumn = e.ColumnIndex;

            switch (sortcolumn)
            {
                case 0: list.Sort(new NumberComparer(sortorder == SortOrder.Descending)); break;
                case 1: list.Sort(new TextComparer1(sortorder == SortOrder.Descending)); break;
                case 2: list.Sort(new TextComparer2(sortorder == SortOrder.Descending)); break;
                case 3: list.Sort(new FlagComparer(sortorder == SortOrder.Descending)); break;
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
            Equal = 0x3
        }

        public int Number { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public CompareRes CompareResult { get; set; }
    }

    public class NumberComparer : IComparer<CompareEntry>
    {
        private bool m_desc;

        public NumberComparer(bool desc)
        {
            m_desc = desc;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            if (objA.Number == objB.Number)
                return 0;
            else if (m_desc)
                return (objA.Number < objB.Number) ? 1 : -1;
            else
                return (objA.Number < objB.Number) ? -1 : 1;
        }
    }

    public class FlagComparer : IComparer<CompareEntry>
    {
        private bool m_desc;

        public FlagComparer(bool desc)
        {
            m_desc = desc;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            if ((byte)objA.CompareResult == (byte)objB.CompareResult)
            {
                if (objA.Number == objB.Number)
                    return 0;
                else if (m_desc)
                    return (objA.Number < objB.Number) ? 1 : -1;
                else
                    return (objA.Number < objB.Number) ? -1 : 1;
            }
            else if (m_desc)
                return ((byte)objA.CompareResult < (byte)objB.CompareResult) ? 1 : -1;
            else
                return ((byte)objA.CompareResult < (byte)objB.CompareResult) ? -1 : 1;
        }
    }

    public class TextComparer1 : IComparer<CompareEntry>
    {
        private bool m_desc;

        public TextComparer1(bool desc)
        {
            m_desc = desc;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            if (m_desc)
                return String.Compare(objB.Text1, objA.Text1);
            else
                return String.Compare(objA.Text1, objB.Text1);
        }
    }
    public class TextComparer2 : IComparer<CompareEntry>
    {
        private bool m_desc;

        public TextComparer2(bool desc)
        {
            m_desc = desc;
        }

        public int Compare(CompareEntry objA, CompareEntry objB)
        {
            if (m_desc)
                return String.Compare(objB.Text2, objA.Text2);
            else
                return String.Compare(objA.Text2, objB.Text2);
        }
    }
}
