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

namespace FiddlerControls
{
    public partial class Speech : UserControl
    {
        public Speech()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            source = new BindingSource();
            toolStrip2.Visible = false;
        }

        private BindingSource source;
        private SortOrder sortorder;
        private int sortcolumn;
        private bool Loaded = false;

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["Speech"] = true;
            sortorder = SortOrder.Ascending;
            sortcolumn = 2;
            source.DataSource = SpeechList.Entries;
            dataGridView1.DataSource = source;
            if (dataGridView1.Columns.Count > 0)
                dataGridView1.Columns[0].Width = 60;
            dataGridView1.Invalidate();
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void onHeaderClick(object sender, DataGridViewCellMouseEventArgs e)
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
                if (sortcolumn != 2)
                    dataGridView1.Columns[sortcolumn].HeaderCell.SortGlyphDirection = SortOrder.None;
            }
            dataGridView1.Columns[e.ColumnIndex].HeaderCell.SortGlyphDirection = sortorder;
            sortcolumn = e.ColumnIndex;

            if (e.ColumnIndex == 0)
                SpeechList.Entries.Sort(new SpeechList.IDComparer(sortorder == SortOrder.Descending));
            else if (e.ColumnIndex == 1)
                SpeechList.Entries.Sort(new SpeechList.KeyWordComparer(sortorder == SortOrder.Descending));

            dataGridView1.Invalidate();
        }

        private void OnCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if ((SpeechList.Entries[e.RowIndex]).KeyWord == null)
                (SpeechList.Entries[e.RowIndex]).KeyWord = "";
            Options.ChangedUltimaClass["Speech"] = true;
        }

        private void FindID(int index)
        {
            short nr;
            if (Int16.TryParse(IDEntry.Text.ToString(), NumberStyles.Integer, null, out nr))
            {
                for (int i = index; i < dataGridView1.Rows.Count; ++i)
                {
                    if ((short)dataGridView1.Rows[i].Cells[0].Value == nr)
                    {
                        dataGridView1.Rows[i].Selected = true;
                        dataGridView1.FirstDisplayedScrollingRowIndex = i;
                        return;
                    }
                }
            }
            MessageBox.Show("ID not found.", "Goto", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void FindKeyWord(int index)
        {
            string find = KeyWordEntry.Text.ToString();
            for (int i = index; i < dataGridView1.Rows.Count; ++i)
            {
                if ((dataGridView1.Rows[i].Cells[1].Value.ToString().IndexOf(find)) != -1)
                {
                    dataGridView1.Rows[i].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = i;
                    return;
                }
            }
            MessageBox.Show("KeyWord not found.", "Entry", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void OnClickFindID(object sender, EventArgs e)
        {
            FindID(0);
        }

        private void OnClickNextID(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
                FindID(dataGridView1.SelectedRows[0].Index + 1);
            else
                FindID(0);
        }

        private void OnClickFindKeyWord(object sender, EventArgs e)
        {
            FindKeyWord(0);
        }

        private void OnClickNextKeyWord(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
                FindKeyWord(dataGridView1.SelectedRows[0].Index + 1);
            else
                FindKeyWord(0);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, "speech.mul");
            SpeechList.SaveSpeechList(FileName);
            dataGridView1.Invalidate();
            MessageBox.Show(String.Format("Speech saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Speech"] = false;
        }

        private void OnAddEntry(object sender, EventArgs e)
        {
            source.Add(new SpeechEntry(0, "", SpeechList.Entries.Count));
            dataGridView1.Invalidate();
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
            dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.Rows.Count - 1;
            Options.ChangedUltimaClass["Speech"] = true;
        }

        private void OnDeleteEntry(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                source.RemoveCurrent();
                dataGridView1.Invalidate();
                Options.ChangedUltimaClass["Speech"] = true;
            }
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, "Speech.csv");
            Ultima.SpeechList.ExportToCSV(FileName);
            MessageBox.Show(String.Format("Speech saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose csv file to import";
            dialog.CheckFileExists = true;
            dialog.Filter = "csv files (*.csv)|*.csv";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["Speech"] = true;
                Ultima.SpeechList.ImportFromCSV(dialog.FileName);
                source.DataSource = SpeechList.Entries;
                dataGridView1.Invalidate();
            }
            dialog.Dispose();
        }
    }
}
