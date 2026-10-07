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
    public partial class SpeechControl : UserControl
    {
        private const string _idEntryPlaceholder = "Find ID...";
        private const string _keywordEntryPlaceholder = "KeyWord...";

        private readonly BindingSource _source;
        private SortOrder _sortOrder;
        private int _sortColumn;
        private bool _loaded;
        private Func<string, string?>? _localizationGetter;

        public SpeechControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _source = new BindingSource();

            IDEntry.Text = _idEntryPlaceholder;
            KeyWordEntry.Text = _keywordEntryPlaceholder;
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
                // 应用列标题（初始时用英文或汉化值）
                ApplyColumnHeaderLocalization();
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
            // 跳过列标题改变事件（e.RowIndex == -1）
            if (e.RowIndex < 0 || e.RowIndex >= SpeechList.Entries.Count)
            {
                return;
            }

            SpeechList.Entries[e.RowIndex].KeyWord ??= string.Empty;

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

            string message = _localizationGetter?.Invoke("Forms.SpeechControl.Messages.IDNotFound") ?? "ID not found.";
            string title = _localizationGetter?.Invoke("Forms.SpeechControl.Messages.IDNotFoundTitle") ?? "Goto";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void FindKeyWord(int index)
        {
            string toFind = KeyWordEntry.Text;

            for (int i = index; i < dataGridView1.Rows.Count; ++i)
            {
                if (!dataGridView1.Rows[i].Cells[1].Value.ToString().Contains(toFind))
                {
                    continue;
                }

                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;

                return;
            }

            string message = _localizationGetter?.Invoke("Forms.SpeechControl.Messages.KeyWordNotFound") ?? "KeyWord not found.";
            string title = _localizationGetter?.Invoke("Forms.SpeechControl.Messages.KeyWordNotFoundTitle") ?? "Entry";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F3 || keyData == (Keys.F3 | Keys.Shift))
            {
                bool isNext = keyData == Keys.F3;

                if (IDEntry.Focused)
                {
                    if (isNext)
                    {
                        OnClickNextID(null, EventArgs.Empty);
                    }
                    else
                    {
                        FindIdPrevious();
                    }
                }
                else
                {
                    if (isNext)
                    {
                        OnClickNextKeyWord(null, EventArgs.Empty);
                    }
                    else
                    {
                        FindKeyWordPrevious();
                    }
                }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void FindIdPrevious()
        {
            if (!short.TryParse(IDEntry.Text, NumberStyles.Integer, null, out short nr))
            {
                MessageBox.Show("ID not found.", "Goto", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            int startRow = dataGridView1.SelectedRows.Count > 0
                ? dataGridView1.SelectedRows[0].Index - 1
                : dataGridView1.Rows.Count - 1;

            if (startRow < 0)
            {
                startRow = dataGridView1.Rows.Count - 1;
            }

            for (int i = startRow; i >= 0; --i)
            {
                if ((short)dataGridView1.Rows[i].Cells[0].Value != nr)
                {
                    continue;
                }

                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;
                return;
            }

            MessageBox.Show("ID not found.", "Goto", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void FindKeyWordPrevious()
        {
            if (string.IsNullOrEmpty(KeyWordEntry.Text) || KeyWordEntry.Text == _keywordEntryPlaceholder)
            {
                return;
            }

            string toFind = KeyWordEntry.Text;

            int startRow = dataGridView1.SelectedRows.Count > 0
                ? dataGridView1.SelectedRows[0].Index - 1
                : dataGridView1.Rows.Count - 1;

            if (startRow < 0)
            {
                startRow = dataGridView1.Rows.Count - 1;
            }

            for (int i = startRow; i >= 0; --i)
            {
                if (!dataGridView1.Rows[i].Cells[1].Value.ToString().Contains(toFind))
                {
                    continue;
                }

                dataGridView1.Rows[i].Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = i;
                return;
            }

            MessageBox.Show("KeyWord not found.", "Entry", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            dataGridView1.CancelEdit();

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "speech.mul");

            SpeechList.SaveSpeechList(fileName);

            dataGridView1.Invalidate();

            Options.ChangedUltimaClass["Speech"] = false;

            string message = _localizationGetter?.Invoke("Forms.SpeechControl.Messages.SpeechSavedSuccessfully") ?? "Speech saved successfully.";
            FileSavedDialog.Show(FindForm(), fileName, message, null, key => _localizationGetter?.Invoke(key));
        }

        private void OnAddEntry(object sender, EventArgs e)
        {
            _source.Add(new SpeechEntry(0, "", SpeechList.Entries.Count));

            dataGridView1.Invalidate();
            dataGridView1.Rows[^1].Selected = true;
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

            string message = _localizationGetter?.Invoke("Forms.SpeechControl.Messages.SpeechSavedSuccessfully") ?? "Speech saved successfully.";
            FileSavedDialog.Show(FindForm(), fileName, message, null, key => _localizationGetter?.Invoke(key));
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = _localizationGetter?.Invoke("Forms.SpeechControl.Dialogs.ImportTitle") ?? "Choose csv file to import",
                CheckFileExists = true,
                Filter = _localizationGetter?.Invoke("Forms.SpeechControl.Dialogs.CSVFilter") ?? "csv files (*.csv)|*.csv"
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

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// 应用列标题汉化（在数据绑定后调用，支持初始化和汉化两种场景）
        /// </summary>
        private void ApplyColumnHeaderLocalization()
        {
            // 设置表格列标题
            // 如果 _localizationGetter 为 null，会使用 ?? 后面的英文默认值
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].HeaderText = _localizationGetter?.Invoke("Forms.SpeechControl.ColumnId") ?? "Id";
                if (dataGridView1.Columns.Count > 1)
                    dataGridView1.Columns[1].HeaderText = _localizationGetter?.Invoke("Forms.SpeechControl.ColumnKeyWord") ?? "KeyWord";
            }
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;
            
            // 占位符
            string idPlaceholder = _localizationGetter("Forms.SpeechControl.IDEntryPlaceholder") ?? _idEntryPlaceholder;
            string keywordPlaceholder = _localizationGetter("Forms.SpeechControl.KeyWordEntryPlaceholder") ?? _keywordEntryPlaceholder;
            
            if (IDEntry.Text == _idEntryPlaceholder)
                IDEntry.Text = idPlaceholder;
            if (KeyWordEntry.Text == _keywordEntryPlaceholder)
                KeyWordEntry.Text = keywordPlaceholder;
            
            // 工具栏按钮
            IDButton.Text = _localizationGetter("Forms.SpeechControl.IDButton") ?? "Find";
            IDNextButton.Text = _localizationGetter("Forms.SpeechControl.IDNextButton") ?? "Find Next";
            KeyWordButton.Text = _localizationGetter("Forms.SpeechControl.KeyWordButton") ?? "Find";
            KeyWordNextButton.Text = _localizationGetter("Forms.SpeechControl.KeyWordNextButton") ?? "Find Next";
            
            // 保存按钮
            toolStripButton1.Text = _localizationGetter("Forms.SpeechControl.SaveButton") ?? "Save";
            
            // 杂项菜单及其下拉项
            toolStripDropDownButton1.Text = _localizationGetter("Forms.SpeechControl.MiscMenu") ?? "Misc";
            toolStripMenuItem1.Text = _localizationGetter("Forms.SpeechControl.ImportFromCSV") ?? "Import from CSV";
            toolStripMenuItem2.Text = _localizationGetter("Forms.SpeechControl.ExportToCSV") ?? "Export to CSV";
            
            // 右键菜单
            addEntryToolStripMenuItem.Text = _localizationGetter("Forms.SpeechControl.AddEntryMenu") ?? "Add Entry";
            deleteEntryToolStripMenuItem.Text = _localizationGetter("Forms.SpeechControl.DeleteEntryMenu") ?? "Delete Entry";
            
            // 表格列标题
            ApplyColumnHeaderLocalization();
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
