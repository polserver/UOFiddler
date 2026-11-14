/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Scans a folder (and the folders below it) for every file format the editor imports, lists
    /// each multi with the format it was recognised as, and previews the highlighted one. Files
    /// that could not be read are listed greyed out with the reason.
    /// </summary>
    internal sealed partial class FolderImportDialog : Form
    {
        /// <summary>Designs with more tiles than this are not drawn on selection: it would stall the list.</summary>
        private const int _previewLimit = 8000;

        /// <summary>Format filter key for the rows that failed.</summary>
        private const string _errorsKey = "\0errors";

        private sealed class Row
        {
            public ImportedMulti Multi;
            public string File;
            public string RelativePath;
            public string Error;

            public string Name => Multi?.Name ?? Path.GetFileName(File);

            public string FormatText => Multi != null ? MultiFileImport.FormatName(Multi.Format) : "error";

            public string FilterKey => Multi != null ? FormatText : _errorsKey;
        }

        private sealed record ScanBatch(List<Row> Rows, int FilesChecked);

        /// <summary>An entry of the format list; a null key means all formats.</summary>
        private sealed record FormatChoice(string Key, string Text)
        {
            public override string ToString() => Text;
        }

        private readonly List<Row> _all = new List<Row>();
        private readonly List<Row> _shown = new List<Row>();
        private readonly HashSet<string> _filesWithMultis = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private string _formatFilter;
        private bool _syncingFormats;
        private CancellationTokenSource _scan;
        private int _filesChecked;
        private int _multiCount;
        private int _failedCount;
        private bool _bulkSelecting;

        public FolderImportDialog()
        {
            InitializeComponent();

            EditorSettings settings = EditorSettings.Current;
            _folder.Text = settings.ImportFolder ?? string.Empty;
            _recursive.Checked = settings.ImportRecursive;
            RebuildFormatChoices();
            UpdateSelection();
        }

        /// <summary>The multis to open, read when Open was pressed.</summary>
        public List<OpenedMulti> Opened { get; } = new List<OpenedMulti>();

        /// <summary>Chosen multis whose items could not be read when Open was pressed.</summary>
        public List<ImportFailure> Failures { get; } = new List<ImportFailure>();

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (_folder.Text.Length > 0 && Directory.Exists(_folder.Text))
            {
                StartScan();
                _filter.Focus();
            }
            else
            {
                _status.Text = "Choose a folder to look for multi files in.";
                _folder.Focus();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _scan?.Cancel();
            base.OnFormClosing(e);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _previewTimer.Stop();
            SetPreview(null);
            base.OnFormClosed(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_folder.Focused && keyData == Keys.Enter)
            {
                if (_scan == null)
                {
                    StartScan();
                }

                return true;
            }

            // Arrow keys in the filter box move the list selection, so the list needs no focus.
            if (_filter.Focused && (keyData == Keys.Down || keyData == Keys.Up) && _shown.Count > 0)
            {
                int index = _list.SelectedIndices.Count > 0 ? _list.SelectedIndices[0] : -1;
                index = Math.Clamp(index + (keyData == Keys.Down ? 1 : -1), 0, _shown.Count - 1);
                _list.SelectedIndices.Clear();
                _list.SelectedIndices.Add(index);
                _list.EnsureVisible(index);
                return true;
            }

            if (_filter.Focused && keyData == Keys.Enter)
            {
                Accept();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Folder with multi files to import",
                UseDescriptionForTitle = true,
                SelectedPath = _folder.Text
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _folder.Text = dialog.SelectedPath;
                StartScan();
            }
        }

        private void Scan_Click(object sender, EventArgs e)
        {
            if (_scan != null)
            {
                _scan.Cancel();
            }
            else
            {
                StartScan();
            }
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            RebuildList();
        }

        private void Format_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingFormats || _format.SelectedItem is not FormatChoice choice || choice.Key == _formatFilter)
            {
                return;
            }

            _formatFilter = choice.Key;
            RebuildList();
        }

        private void List_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // A virtual list must always get an item, even one it asks for while it shrinks.
            if (e.ItemIndex < 0 || e.ItemIndex >= _shown.Count)
            {
                e.Item = new ListViewItem(new[] { string.Empty, string.Empty, string.Empty, string.Empty });
                return;
            }

            Row row = _shown[e.ItemIndex];
            e.Item = new ListViewItem(row.Name);
            e.Item.SubItems.Add(row.FormatText);
            e.Item.SubItems.Add(row.Multi != null ? row.Multi.TileCount.ToString("N0") : string.Empty);
            e.Item.SubItems.Add(row.RelativePath);
            if (row.Multi == null)
            {
                e.Item.ForeColor = SystemColors.GrayText;
            }
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelection();
        }

        // Shift ranges in a virtual list.
        private void List_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            UpdateSelection();
        }

        private void List_DoubleClick(object sender, EventArgs e)
        {
            Accept();
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = e.SuppressKeyPress = true;
                Accept();
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                // Select all shown rows; a virtual list has no built-in Ctrl+A.
                e.Handled = e.SuppressKeyPress = true;
                _bulkSelecting = true;
                try
                {
                    for (int i = 0; i < _shown.Count; i++)
                    {
                        _list.SelectedIndices.Add(i);
                    }
                }
                finally
                {
                    _bulkSelecting = false;
                }

                UpdateSelection();
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            ShowPreview();
        }

        // ---------------------------------------------------------------- scanning

        private async void StartScan()
        {
            string folder = _folder.Text.Trim();
            if (folder.Length == 0 || !Directory.Exists(folder))
            {
                _status.Text = "The folder does not exist.";
                return;
            }

            bool recursive = _recursive.Checked;
            EditorSettings settings = EditorSettings.Current;
            if (settings.ImportFolder != folder || settings.ImportRecursive != recursive)
            {
                settings = settings.Clone();
                settings.ImportFolder = folder;
                settings.ImportRecursive = recursive;
                settings.Apply();
            }

            _all.Clear();
            _filesWithMultis.Clear();
            _filesChecked = _multiCount = _failedCount = 0;
            _formatFilter = null;
            RebuildFormatChoices();
            RebuildList();

            var scan = new CancellationTokenSource();
            _scan = scan;
            SetScanning(true);
            UpdateStatus(scanning: true, stopped: false);

            bool stopped = false;
            var progress = new Progress<ScanBatch>(OnBatch);
            try
            {
                await Task.Run(() => Scan(folder, recursive, progress, scan.Token));
            }
            catch (OperationCanceledException)
            {
                stopped = true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The folder went away or could not be listed; what was found stays.
                stopped = true;
            }
            finally
            {
                scan.Dispose();
            }

            if (IsDisposed)
            {
                return;
            }

            _scan = null;
            SetScanning(false);
            UpdateStatus(scanning: false, stopped);
        }

        /// <summary>Runs off the UI thread: reads every candidate file and reports the rows in batches.</summary>
        private static void Scan(string folder, bool recursive, IProgress<ScanBatch> progress, CancellationToken token)
        {
            var rows = new List<Row>();
            int files = 0;
            var clock = Stopwatch.StartNew();
            foreach (string file in MultiFileImport.CandidateFiles(folder, recursive))
            {
                token.ThrowIfCancellationRequested();
                files++;

                Multis.ImportType? type = MultiFileImport.Detect(file);
                if (type != null)
                {
                    string relative = Path.GetRelativePath(folder, file);
                    try
                    {
                        foreach (ImportedMulti multi in MultiFileImport.Load(file, type.Value))
                        {
                            rows.Add(new Row { Multi = multi, File = file, RelativePath = relative });
                        }
                    }
                    catch (Exception ex)
                    {
                        rows.Add(new Row { File = file, RelativePath = relative, Error = $"{MultiFileImport.FormatName(type.Value)}: {MultiFileImport.Reason(ex)}" });
                    }
                }

                if (clock.ElapsedMilliseconds > 150)
                {
                    progress.Report(new ScanBatch(rows, files));
                    rows = new List<Row>();
                    clock.Restart();
                }
            }

            progress.Report(new ScanBatch(rows, files));
        }

        private void OnBatch(ScanBatch batch)
        {
            if (IsDisposed)
            {
                return;
            }

            _filesChecked = batch.FilesChecked;
            _all.AddRange(batch.Rows);
            foreach (Row row in batch.Rows)
            {
                if (row.Multi != null)
                {
                    _multiCount++;
                    _filesWithMultis.Add(row.File);
                }
                else
                {
                    _failedCount++;
                }
            }

            RebuildFormatChoices();

            // New rows go on the end, so the selection and scroll position stay put.
            int before = _shown.Count;
            _shown.AddRange(batch.Rows.Where(Matches));
            if (_shown.Count != before)
            {
                _list.VirtualListSize = _shown.Count;
            }

            UpdateStatus(scanning: _scan != null, stopped: false);
        }

        private void SetScanning(bool scanning)
        {
            _scanButton.Text = scanning ? "Stop" : "Scan";
            _folder.Enabled = _browse.Enabled = _recursive.Enabled = !scanning;
        }

        private void UpdateStatus(bool scanning, bool stopped)
        {
            if (scanning)
            {
                _status.Text = $"Scanning... {_filesChecked:N0} files checked, {_multiCount:N0} multis found";
                return;
            }

            if (_all.Count == 0)
            {
                _status.Text = stopped ? "Stopped. No multis found so far." : "No multi files found in this folder.";
                return;
            }

            string text = $"{_multiCount:N0} multis in {_filesWithMultis.Count:N0} files";
            if (_failedCount > 0)
            {
                text += _failedCount == 1 ? ", 1 file could not be read" : $", {_failedCount:N0} files could not be read";
            }

            _status.Text = stopped ? text + " (stopped)" : text;
        }

        // ---------------------------------------------------------------- list

        private bool Matches(Row row)
        {
            if (_formatFilter != null && row.FilterKey != _formatFilter)
            {
                return false;
            }

            string text = _filter.Text.Trim();
            return text.Length == 0
                   || row.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                   || row.RelativePath.Contains(text, StringComparison.OrdinalIgnoreCase);
        }

        private void RebuildList()
        {
            // Empty the list before its rows change: clearing the selection repaints and asks for
            // items, which must still be there.
            _list.SelectedIndices.Clear();
            _list.VirtualListSize = 0;
            _shown.Clear();
            _shown.AddRange(_all.Where(Matches));
            _list.VirtualListSize = _shown.Count;
            _list.Invalidate();
            UpdateSelection();
        }

        /// <summary>
        /// The format choices: All, each format found (in the import dialog's order) and the files
        /// that failed, each with its count.
        /// </summary>
        private void RebuildFormatChoices()
        {
            var counts = new Dictionary<string, int>();
            foreach (Row row in _all)
            {
                counts[row.FilterKey] = counts.TryGetValue(row.FilterKey, out int n) ? n + 1 : 1;
            }

            var choices = new List<FormatChoice> { new FormatChoice(null, _all.Count == 0 ? "All" : $"All ({_all.Count:N0})") };
            foreach (string name in MultiFileImport.Formats.Select(f => MultiFileImport.FormatName(f.Type)).Distinct())
            {
                if (counts.TryGetValue(name, out int count))
                {
                    choices.Add(new FormatChoice(name, $"{name} ({count:N0})"));
                }
            }

            if (counts.TryGetValue(_errorsKey, out int errors))
            {
                choices.Add(new FormatChoice(_errorsKey, $"Could not be read ({errors:N0})"));
            }

            _syncingFormats = true;
            _format.BeginUpdate();
            try
            {
                _format.Items.Clear();
                _format.Items.AddRange(choices.ToArray());
                int selected = choices.FindIndex(c => c.Key == _formatFilter);
                _format.SelectedIndex = Math.Max(0, selected);
            }
            finally
            {
                _format.EndUpdate();
                _syncingFormats = false;
            }
        }

        private List<Row> SelectedRows()
        {
            var rows = new List<Row>();
            foreach (int index in _list.SelectedIndices)
            {
                if (index < _shown.Count)
                {
                    rows.Add(_shown[index]);
                }
            }

            return rows;
        }

        private Row FocusedRow()
        {
            int index = _list.FocusedItem?.Index ?? -1;
            if (index >= 0 && index < _shown.Count && _list.SelectedIndices.Contains(index))
            {
                return _shown[index];
            }

            return _list.SelectedIndices.Count > 0 && _list.SelectedIndices[0] < _shown.Count ? _shown[_list.SelectedIndices[0]] : null;
        }

        private void UpdateSelection()
        {
            if (_bulkSelecting)
            {
                return;
            }

            List<Row> rows = SelectedRows();
            int openable = rows.Count(r => r.Multi != null);
            _ok.Enabled = openable > 0;
            _ok.Text = openable > 1 ? $"Open {openable:N0}" : "Open";

            if (rows.Count > 1)
            {
                _info.Text = openable == rows.Count
                    ? $"{rows.Count:N0} selected: each opens in its own tab"
                    : $"{rows.Count:N0} selected, {openable:N0} can be opened: each opens in its own tab";
            }
            else if (rows.Count == 1)
            {
                Row row = rows[0];
                _info.Text = row.Multi != null
                    ? $"{row.FormatText}, {row.Multi.TileCount:N0} tiles\r\n{row.RelativePath}"
                    : $"Could not be read.\r\n{row.Error}\r\n{row.RelativePath}";
            }
            else
            {
                _info.Text = string.Empty;
            }

            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private void ShowPreview()
        {
            Row row = FocusedRow();
            if (row?.Multi == null)
            {
                SetPreview(null);
                return;
            }

            if (row.Multi.TileCount > _previewLimit)
            {
                SetPreview(null);
                _info.Text = $"{row.FormatText}, {row.Multi.TileCount:N0} tiles: too large to preview here. Open it to see it.\r\n{row.RelativePath}";
                return;
            }

            MultiComponentList multi;
            try
            {
                multi = row.Multi.Components;
            }
            catch (Exception ex)
            {
                SetPreview(null);
                _info.Text = $"Could not be read.\r\n{MultiFileImport.Reason(ex)}\r\n{row.RelativePath}";
                return;
            }

            if (_list.SelectedIndices.Count == 1)
            {
                _info.Text = $"{row.FormatText}, {multi.Width}x{multi.Height}, {row.Multi.TileCount:N0} tiles\r\n{row.RelativePath}";
            }

            try
            {
                SetPreview(multi.GetImage());
            }
            catch (Exception)
            {
                // A multi that cannot be drawn must not take the dialog down; it just has no preview.
                SetPreview(null);
            }
        }

        private void SetPreview(Image image)
        {
            Image old = _preview.Image;
            _preview.Image = image;
            old?.Dispose();
        }

        private void Accept()
        {
            List<Row> rows = SelectedRows().Where(r => r.Multi != null).ToList();
            if (rows.Count == 0)
            {
                return;
            }

            _scan?.Cancel();
            Opened.Clear();
            Failures.Clear();

            Cursor previous = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                foreach (Row row in rows)
                {
                    try
                    {
                        Opened.Add(new OpenedMulti(row.Multi.Components, row.Multi.Name, null));
                    }
                    catch (Exception ex)
                    {
                        Failures.Add(new ImportFailure(row.File, $"{row.Multi.Name}: {MultiFileImport.Reason(ex)}"));
                    }
                }
            }
            finally
            {
                Cursor.Current = previous;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
