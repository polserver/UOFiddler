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
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class SpeechControl : UserControl
    {
        private const string _idEntryPlaceholder = "Find ID...";
        private const string _keywordEntryPlaceholder = "KeyWord...";

        private readonly BindingSource _source;
        private SortOrder _sortOrder;
        private int _sortColumn;
        private bool _loaded;

        public SpeechControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _source = new BindingSource();

            IDEntry.Text = _idEntryPlaceholder;
            KeyWordEntry.Text = _keywordEntryPlaceholder;

            toolStrip2.Visible = false;
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

            Options.LoadedUltimaClass["Speech"] = true;

            _sortOrder = SortOrder.Ascending;
            _sortColumn = 2;
            _source.DataSource = SpeechList.Entries;

            dataGridView1.DataSource = _source;

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].Width = 60;
            }

            dataGridView1.Invalidate();

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnHeaderClick(object sender, DataGridViewCellMouseEventArgs e)
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

                if (_sortColumn != 2)
                {
                    dataGridView1.Columns[_sortColumn].HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }

            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = _sortOrder;

            _sortColumn = e.ColumnIndex;

            switch (e.ColumnIndex)
            {
                case 0:
                    SpeechList.Entries.Sort(new SpeechList.IdComparer(_sortOrder == SortOrder.Descending));
                    break;
                case 1:
                    SpeechList.Entries.Sort(new SpeechList.KeyWordComparer(_sortOrder == SortOrder.Descending));
                    break;
            }

            dataGridView1.Invalidate();
        }

        private void OnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (SpeechList.Entries[e.RowIndex].KeyWord == null)
            {
                SpeechList.Entries[e.RowIndex].KeyWord = string.Empty;
            }

            Options.ChangedUltimaClass["Speech"] = true;
        }

        private void FindId(int index)
        {
            if (short.TryParse(IDEntry.Text, NumberStyles.Integer, null, out short nr))
            {
                for (int i = index; i < dataGridView1.Rows.Count; ++i)
                {
                    if ((short)dataGridView1.Rows[i].Cells[0].Value != nr)
                    {
                        continue;
                    }

                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }

            MessageBox.Show("ID not found.", "Goto", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void FindKeyWord(int index)
        {
            string find = KeyWordEntry.Text;
            for (int i = index; i < dataGridView1.Rows.Count; ++i)
            {
                if (dataGridView1.Rows[i].Cells[1].Value.ToString().IndexOf(find, StringComparison.Ordinal) == -1)
                {
                    continue;
                }

                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;

                return;
            }

            MessageBox.Show("KeyWord not found.", "Entry", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void OnClickFindID(object sender, EventArgs e)
        {
            FindId(0);
        }

        private void OnClickNextID(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                FindId(dataGridView1.SelectedRows[0].Index + 1);
            }
            else
            {
                FindId(0);
            }
        }

        private void OnClickFindKeyWord(object sender, EventArgs e)
        {
            FindKeyWord(0);
        }

        private void OnClickNextKeyWord(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                FindKeyWord(dataGridView1.SelectedRows[0].Index + 1);
            }
            else
            {
                FindKeyWord(0);
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "speech.mul");

            SpeechList.SaveSpeechList(fileName);

            dataGridView1.Invalidate();

            MessageBox.Show($"Speech saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);

            Options.ChangedUltimaClass["Speech"] = false;
        }

        private void OnAddEntry(object sender, EventArgs e)
        {
            _source.Add(new SpeechEntry(0, "", SpeechList.Entries.Count));

            dataGridView1.Invalidate();
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;

            Options.ChangedUltimaClass["Speech"] = true;
        }

        private void OnDeleteEntry(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count <= 0)
            {
                return;
            }

            _source.RemoveCurrent();

            dataGridView1.Invalidate();

            Options.ChangedUltimaClass["Speech"] = true;
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "Speech.csv");

            SpeechList.ExportToCsv(fileName);

            MessageBox.Show($"Speech saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickImport(object sender, EventArgs e)
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
                Options.ChangedUltimaClass["Speech"] = true;

                SpeechList.ImportFromCsv(dialog.FileName);

                _source.DataSource = SpeechList.Entries;
                dataGridView1.Invalidate();
            }

            dialog.Dispose();
        }

        private void IDEntry_Enter(object sender, EventArgs e)
        {
            if (IDEntry.Text == _idEntryPlaceholder)
            {
                IDEntry.Text = string.Empty;
            }
        }

        private void KeyWordEntry_Enter(object sender, EventArgs e)
        {
            if (KeyWordEntry.Text == _keywordEntryPlaceholder)
            {
                KeyWordEntry.Text = string.Empty;
            }
        }
    }
}
