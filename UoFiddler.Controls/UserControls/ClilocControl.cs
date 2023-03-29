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
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class ClilocControl : UserControl
    {
        public ClilocControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _source = new BindingSource();
            FindEntry.TextBox.PreviewKeyDown += FindEntry_PreviewKeyDown;
        }

        private const string _searchNumberPlaceholder = "Enter Number";
        private const string _searchTextPlaceholder = "Enter Text";

        private static StringList _cliloc;
        private static BindingSource _source;
        private int _lang;
        private SortOrder _sortOrder;
        private int _sortColumn;
        private bool _loaded;

        /// <summary>
        /// Sets Language and loads cliloc
        /// </summary>
        private int Lang
        {
            get => _lang;
            set
            {
                _lang = value;
                switch (value)
                {
                    case 0:
                        _cliloc = new StringList("enu");
                        break;
                    case 1:
                        _cliloc = new StringList("deu");
                        break;
                    case 2:
                        TestCustomLang("cliloc.custom1");
                        _cliloc = new StringList("custom1");
                        break;
                    case 3:
                        TestCustomLang("cliloc.custom2");
                        _cliloc = new StringList("custom2");
                        break;
                }
            }
        }

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            _sortOrder = SortOrder.Ascending;
            _sortColumn = 0;
            LangComboBox.SelectedIndex = 0;
            Lang = 0;
            _cliloc.Entries.Sort(new StringList.NumberComparer(false));
            _source.DataSource = _cliloc.Entries;
            dataGridView1.DataSource = _source;
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                dataGridView1.Columns[0].Width = 60;
                dataGridView1.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].Width = 60;
                dataGridView1.Columns[2].ReadOnly = true;
            }
            dataGridView1.Invalidate();
            LangComboBox.Items[2] = Files.GetFilePath("cliloc.custom1") != null
                ? $"Custom 1 ({Path.GetExtension(Files.GetFilePath("cliloc.custom1"))})"
                : "Custom 1";

            LangComboBox.Items[3] = Files.GetFilePath("cliloc.custom2") != null
                ? $"Custom 2 ({Path.GetExtension(Files.GetFilePath("cliloc.custom2"))})"
                : "Custom 2";

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;

            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void TestCustomLang(string what)
        {
            if (Files.GetFilePath(what) != null)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose Cliloc file to open";
                dialog.CheckFileExists = true;
                dialog.Filter = "cliloc files (cliloc.*)|cliloc.*";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Files.SetMulPath(dialog.FileName, what);
                LangComboBox.BeginUpdate();
                if (what == "cliloc.custom1")
                {
                    LangComboBox.Items[2] = $"Custom 1 ({Path.GetExtension(dialog.FileName)})";
                }
                else
                {
                    LangComboBox.Items[3] = $"Custom 2 ({Path.GetExtension(dialog.FileName)})";
                }

                LangComboBox.EndUpdate();
            }
        }

        private void OnLangChange(object sender, EventArgs e)
        {
            if (LangComboBox.SelectedIndex == Lang)
            {
                return;
            }

            Lang = LangComboBox.SelectedIndex;
            _sortOrder = SortOrder.Ascending;
            _sortColumn = 0;
            _cliloc.Entries.Sort(new StringList.NumberComparer(false));
            _source.DataSource = _cliloc.Entries;

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
                dataGridView1.Columns[0].Width = 60;
                dataGridView1.Columns[1].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].HeaderCell.SortGlyphDirection = SortOrder.None;
                dataGridView1.Columns[2].Width = 60;
                dataGridView1.Columns[2].ReadOnly = true;
            }

            dataGridView1.Invalidate();
        }

        private void GotoNr(object sender, EventArgs e)
        {
            if (int.TryParse(GotoEntry.Text, NumberStyles.Integer, null, out int nr))
            {
                for (int i = 0; i < dataGridView1.Rows.Count; ++i)
                {
                    if ((int)dataGridView1.Rows[i].Cells[0].Value != nr)
                    {
                        continue;
                    }

                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }

            MessageBox.Show(
                "Number not found.",
                "Goto",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }

        private void FindEntryClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FindEntry.Text) || FindEntry.Text == _searchTextPlaceholder)
            {
                MessageBox.Show("Please provide search text", "Find Entry", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);

                return;
            }

            var searchMethod = SearchHelper.GetSearchMethod(RegexToolStripButton.Checked);

            bool hasErrors = false;

            for (int i = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected) + 1; i < dataGridView1.Rows.Count; ++i)
            {
                var searchResult = searchMethod(FindEntry.Text, dataGridView1.Rows[i].Cells[1].Value.ToString());
                if (searchResult.HasErrors)
                {
                    hasErrors = true;
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue;
                }

                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;
                return;
            }

            MessageBox.Show(hasErrors ? "Invalid regular expression." : "Entry not found.", "Find Entry",
                MessageBoxButtons.OK, MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();

            string path = Options.OutputPath;
            string fileName;

            if (_cliloc.Language == "custom1")
            {
                fileName = Path.Combine(path, $"Cliloc{Path.GetExtension(Files.GetFilePath("cliloc.custom1"))}");
            }
            else
            {
                fileName = _cliloc.Language == "custom2"
                    ? Path.Combine(path, $"Cliloc{Path.GetExtension(Files.GetFilePath("cliloc.custom2"))}")
                    : Path.Combine(path, $"Cliloc.{_cliloc.Language}");
            }

            _cliloc.SaveStringList(fileName);
            dataGridView1.Columns[_sortColumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            dataGridView1.Columns[0].HeaderCell.SortGlyphDirection = SortOrder.Ascending;
            _sortColumn = 0;
            _sortOrder = SortOrder.Ascending;
            dataGridView1.Invalidate();
            MessageBox.Show(
                $"CliLoc saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["CliLoc"] = false;
        }

        private void OnCell_dbClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            int cellNr = (int)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            string cellText = (string)dataGridView1.Rows[e.RowIndex].Cells[1].Value;

            new ClilocDetailForm(cellNr, cellText, SaveEntry).Show();
        }

        private void OnClick_AddEntry(object sender, EventArgs e)
        {
            new ClilocAddForm(IsNumberFree, AddEntry).Show();
        }

        private void OnClick_DeleteEntry(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count <= 0)
            {
                return;
            }

            _cliloc.Entries.RemoveAt(dataGridView1.SelectedCells[0].OwningRow.Index);
            dataGridView1.Invalidate();
            Options.ChangedUltimaClass["CliLoc"] = true;
        }

        private void OnHeaderClicked(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (_sortColumn == e.ColumnIndex)
            {
                _sortOrder = _sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
                dataGridView1.Columns[_sortColumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            }

            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = _sortOrder;
            _sortColumn = e.ColumnIndex;

            switch (e.ColumnIndex)
            {
                case 0:
                    _cliloc.Entries.Sort(new StringList.NumberComparer(_sortOrder == SortOrder.Descending));
                    break;
                case 1:
                    _cliloc.Entries.Sort(new StringList.TextComparer(_sortOrder == SortOrder.Descending));
                    break;
                default:
                    _cliloc.Entries.Sort(new StringList.FlagComparer(_sortOrder == SortOrder.Descending));
                    break;
            }

            dataGridView1.Invalidate();
        }

        private void OnCLick_CopyClilocNumber(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                Clipboard.SetDataObject(
                    ((int)dataGridView1.SelectedCells[0].OwningRow.Cells[0].Value).ToString(), true);
            }
        }

        private void OnCLick_CopyClilocText(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                Clipboard.SetDataObject(
                    (string)dataGridView1.SelectedCells[0].OwningRow.Cells[1].Value, true);
            }
        }

        public void SaveEntry(int number, string text)
        {
            for (int i = 0; i < _cliloc.Entries.Count; ++i)
            {
                if (_cliloc.Entries[i].Number != number)
                {
                    continue;
                }

                _cliloc.Entries[i].Text = text;
                _cliloc.Entries[i].Flag = StringEntry.CliLocFlag.Modified;

                dataGridView1.Invalidate();
                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;

                Options.ChangedUltimaClass["CliLoc"] = true;

                return;
            }
        }

        public bool IsNumberFree(int number)
        {
            foreach (StringEntry entry in _cliloc.Entries)
            {
                if (entry.Number == number)
                {
                    return false;
                }
            }

            return true;
        }

        public void AddEntry(int number)
        {
            int index = 0;

            foreach (StringEntry entry in _cliloc.Entries)
            {
                if (entry.Number > number)
                {
                    _cliloc.Entries.Insert(index, new StringEntry(number, "", StringEntry.CliLocFlag.Custom));

                    dataGridView1.Invalidate();
                    dataGridView1.Rows[index].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = index;

                    Options.ChangedUltimaClass["CliLoc"] = true;

                    return;
                }

                ++index;
            }
        }

        private static void FindEntry_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //if (e.KeyData == Keys.Control) || (e.Ke Keys.Alt | Keys.Tab | Keys.a))
            e.IsInputKey = true;
        }

        private void OnClickExportCSV(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "CliLoc.csv");

            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite)))
            {
                tex.WriteLine("Number;Text;Flag");

                foreach (StringEntry entry in _cliloc.Entries)
                {
                    tex.WriteLine("{0};{1};{2}", entry.Number, entry.Text, entry.Flag);
                }
            }

            MessageBox.Show($"CliLoc saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickImportCSV(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                using (StreamReader sr = new StreamReader(dialog.FileName))
                {
                    int count = 0;

                    while (sr.ReadLine() is { } line)
                    {
                        if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        {
                            continue;
                        }

                        if (line.StartsWith("Number;"))
                        {
                            continue;
                        }

                        try
                        {
                            string[] split = line.Split(';');
                            if (split.Length < 3)
                            {
                                continue;
                            }

                            int id = int.Parse(split[0].Trim());
                            string text = split[1].Trim();

                            int index = 0;
                            foreach (StringEntry entry in _cliloc.Entries)
                            {
                                if (entry.Number == id)
                                {
                                    if (entry.Text != text)
                                    {
                                        entry.Text = text;
                                        entry.Flag = StringEntry.CliLocFlag.Modified;
                                        count++;
                                    }
                                    break;
                                }

                                if (entry.Number > id)
                                {
                                    _cliloc.Entries.Insert(index, new StringEntry(id, text, StringEntry.CliLocFlag.Custom));
                                    count++;
                                    break;
                                }
                                ++index;
                            }

                            dataGridView1.Invalidate();
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    if (count > 0)
                    {
                        Options.ChangedUltimaClass["CliLoc"] = true;
                        MessageBox.Show(this, $"{count} entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "No entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            dialog.Dispose();
        }

        private void TileDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int count = 0;
            for (int index = 0; index < TileData.ItemTable.Length; index++)
            {
                ItemData itemData = TileData.ItemTable[index];
                int baseClilocId = GetCliLocBaseId(index);
                int id = index + baseClilocId;

                if (string.IsNullOrWhiteSpace(itemData.Name))
                {
                    int i = _cliloc.Entries.FindIndex(x => x.Number == id);

                    if (i >= 0)
                    {
                        _cliloc.Entries.RemoveAt(i);
                        count++;
                    }
                }
                else
                {
                    int entryIndex = 0;
                    foreach (StringEntry entry in _cliloc.Entries)
                    {
                        if (entry.Number == id)
                        {
                            if (entry.Text != itemData.Name)
                            {
                                entry.Text = itemData.Name;
                                entry.Flag = StringEntry.CliLocFlag.Modified;
                                count++;
                            }

                            break;
                        }

                        if (entry.Number > id)
                        {
                            _cliloc.Entries.Insert(entryIndex, new StringEntry(id, itemData.Name, StringEntry.CliLocFlag.Modified));
                            count++;
                            break;
                        }

                        entryIndex++;
                    }
                }
            }

            if (count > 0)
            {
                Options.ChangedUltimaClass["CliLoc"] = true;
                MessageBox.Show(this, $"{count} entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(this, "No entries changed.", "Import Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static int GetCliLocBaseId(int tileId)
        {
            if (tileId >= 0x4000u)
            {
                if (tileId >= 0x8000u)
                {
                    if (tileId < 0x10000)
                    {
                        return 1084024;
                    }
                }
                else
                {
                    return 1078872;
                }
            }
            else
            {
                return 1020000;
            }

            throw new ArgumentException("Tile id out of range.", nameof(tileId));
        }

        private void GotoEntry_Enter(object sender, EventArgs e)
        {
            if (GotoEntry.Text == _searchNumberPlaceholder)
            {
                GotoEntry.Text = "";
            }
        }

        private void FindEntry_Enter(object sender, EventArgs e)
        {
            if (FindEntry.Text == _searchTextPlaceholder)
            {
                FindEntry.Text = "";
            }
        }

        private void GotoEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            GotoNr(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        private void FindEntry_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            FindEntryClick(sender, e);
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
    }
}
