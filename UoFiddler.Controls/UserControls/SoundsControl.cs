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
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class SoundsControl : UserControl
    {
        private const int _soundsLength = 0xFFF;

        private System.Media.SoundPlayer _sp;
        private readonly Timer _spTimer;
        private int _spTimerMax;
        private DateTime _spTimerStart;

        private bool _playing;

        private bool _loaded;

        private int _soundIdOffset;

        public SoundsControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _spTimer = new Timer();
            _spTimer.Tick += OnSpTimerTick;

            listView.LabelEdit = true;
            listView.BeforeLabelEdit += ListView_BeforeLabelEdit;
            listView.AfterLabelEdit += ListViewOnAfterLabelEdit;
            // ListView's default Sort() uses ListView.Sorting; enable
            // ascending text sort so the existing toggle keeps working.
            listView.Sorting = System.Windows.Forms.SortOrder.Ascending;

            _soundIdOffset = GetSoundIdOffset();
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        public void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            nameSortToolStripMenuItem.Checked = false;

            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Sound"] = true;

            int? oldItem = null;

            if (listView.SelectedItems.Count > 0)
            {
                oldItem = (int)listView.SelectedItems[0].Tag;
            }

            listView.BeginUpdate();
            try
            {
                listView.Items.Clear();

                _soundIdOffset = GetSoundIdOffset();

                var cache = new List<ListViewItem>();
                for (int i = 0; i < _soundsLength; ++i)
                {
                    if (Sounds.IsValidSound(i, out string name, out bool translated))
                    {
                        var item = new ListViewItem($"0x{i + _soundIdOffset:X3} {name}") { Tag = i };

                        if (translated)
                        {
                            item.ForeColor = Color.Blue;
                            item.Font = new Font(Font, FontStyle.Underline);
                        }

                        cache.Add(item);
                    }
                    else if (showFreeSlotsToolStripMenuItem.Checked)
                    {
                        cache.Add(new ListViewItem($"0x{i:X3} ")
                        {
                            Tag = i,
                            ForeColor = Color.Red
                        });
                    }
                }

                listView.Items.AddRange(cache.ToArray());
            }
            finally
            {
                listView.EndUpdate();
            }

            if (listView.Items.Count > 0)
            {
                listView.Items[0].Selected = true;
                listView.Items[0].EnsureVisible();
            }

            _sp = new System.Media.SoundPlayer();
            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            _playing = false;

            Cursor.Current = Cursors.Default;

            if (oldItem != null)
            {
                SearchId(oldItem.Value);
            }
        }

        private static int GetSoundIdOffset()
        {
            return Options.PolSoundIdOffset ? 1 : 0;
        }

        private void OnSpTimerTick(object sender, EventArgs eventArgs)
        {
            BeginInvoke((Action)(() =>
                {
                    TimeSpan diff = DateTime.Now - _spTimerStart;
                    playing.Value = Math.Min(100, (int)(diff.TotalMilliseconds * 100d / _spTimerMax));
                    SoundPlaytimeBar.Value = playing.Value;

                    if (diff.TotalMilliseconds < _spTimerMax)
                    {
                        return;
                    }

                    playing.Visible = false;
                    SoundPlaytimeBar.Value = 0;

                    stopButton.Visible = false;
                    StopSoundButton.Enabled = false;
                    _spTimer.Stop();
                }));
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickPlay(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }
            PlaySound((int)listView.SelectedItems[0].Tag);
        }

        private void OnDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = listView.HitTest(e.Location);
            if (hit.Item == null)
            {
                return;
            }
            PlaySound((int)hit.Item.Tag);
        }

        private void OnClickStop(object sender, EventArgs e)
        {
            StopSound();
        }

        private void StopSound()
        {
            _sp.Stop();
            _spTimer.Stop();
            _playing = false;
            playing.Visible = false;
            SoundPlaytimeBar.Value = 0;
            stopButton.Visible = false;
            StopSoundButton.Enabled = false;
        }

        private void PlaySound(int id)
        {
            _sp.Stop();
            _spTimer.Stop();
            _playing = false;
            playing.Visible = false;
            SoundPlaytimeBar.Value = 0;
            stopButton.Visible = false;
            StopSoundButton.Enabled = false;

            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            UoSound sound = Sounds.GetSound(id);
            if (sound == null)
            {
                return;
            }

            using (MemoryStream mStream = new MemoryStream(sound.Buffer))
            {
                _sp.Stream = mStream;
                _sp.Play();

                playing.Value = 0;
                playing.Visible = true;
                SoundPlaytimeBar.Value = 0;
                stopButton.Visible = true;
                StopSoundButton.Enabled = true;
                _spTimerStart = DateTime.Now;
                _spTimerMax = (int)(Sounds.GetSoundLength(id) * 1000);
                _spTimer.Interval = 50;
                _spTimer.Start();

                _playing = true;
            }
        }

        private void AfterSelect(object sender, EventArgs e)
        {
            // Mirror the old TreeView BeforeSelect behaviour: stop playback
            // when the user moves to a different row.
            if (_playing)
            {
                StopSound();
            }

            ListViewItem selected = listView.SelectedItems.Count > 0 ? listView.SelectedItems[0] : null;

            if (selected == null)
            {
                playSoundToolStripMenuItem.Enabled = false;
                extractSoundToolStripMenuItem.Enabled = false;
                removeSoundToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Text = "Insert/Replace";
            }

            if (selected != null)
            {
                double length = Sounds.GetSoundLength((int)selected.Tag);
                seconds.Text = length > 0 ? $"{length:f}s" : "Empty Slot";
            }

            bool isValidSound = selected != null && Sounds.IsValidSound((int)selected.Tag, out _, out _);

            playSoundToolStripMenuItem.Enabled = isValidSound;
            extractSoundToolStripMenuItem.Enabled = isValidSound;
            removeSoundToolStripMenuItem.Enabled = isValidSound;

            replaceToolStripMenuItem.Enabled = true;
            replaceToolStripMenuItem.Text = isValidSound ? "Replace" : "Insert";

            SelectedSoundGroup.Visible = selected != null;

            if (selected != null)
            {
                SelectedSoundGroup.Text = $"Current Sound: {selected.Text} - Duration: {seconds.Text}";
                IdInsertTextbox.Text = $"0x{(int)selected.Tag + _soundIdOffset:X}";
            }
        }

        private void OnChangeSort(object sender, EventArgs e)
        {
            if (showFreeSlotsToolStripMenuItem.Checked)
            {
                showFreeSlotsToolStripMenuItem.Checked = false;
                nextFreeSlotToolStripMenuItem.Enabled = false;
                Reload();
                nameSortToolStripMenuItem.Checked = true;
            }

            int? oldItem = null;
            if (listView.SelectedItems.Count > 0)
            {
                oldItem = (int)listView.SelectedItems[0].Tag;
            }

            const string delimiter = " ";

            listView.BeginUpdate();

            for (int i = 0; i < listView.Items.Count; ++i)
            {
                string name = listView.Items[i].Text;

                int splitIndex = nameSortToolStripMenuItem.Checked
                    ? name.IndexOf(delimiter, StringComparison.Ordinal)
                    : name.LastIndexOf(delimiter, StringComparison.Ordinal);

                listView.Items[i].Text = $"{name.Substring(splitIndex).Trim()} {name.Substring(0, splitIndex).Trim()}";
            }

            listView.Sort();
            listView.EndUpdate();

            if (oldItem != null)
            {
                SearchId(oldItem.Value);
            }
        }

        private void DoSearchName(string name, bool next, bool prev)
        {
            int index = 0;
            int selectedIndex = listView.SelectedItems.Count > 0 ? listView.SelectedItems[0].Index : -1;

            if (prev)
            {
                if (selectedIndex >= 0)
                {
                    index = selectedIndex - _soundIdOffset;
                }

                if (index <= 0)
                {
                    index = 0;
                }

                for (int i = index - 1; i >= 0; --i)
                {
                    ListViewItem item = listView.Items[i];
                    if (!item.Text.ContainsCaseInsensitive(name))
                    {
                        continue;
                    }

                    listView.SelectedItems.Clear();
                    item.Selected = true;
                    item.EnsureVisible();
                    return;
                }
            }
            else
            {
                if (next)
                {
                    if (selectedIndex >= 0)
                    {
                        index = selectedIndex + 1;
                    }

                    if (index >= listView.Items.Count)
                    {
                        index = 0;
                    }
                }

                for (int i = index; i < listView.Items.Count; ++i)
                {
                    ListViewItem item = listView.Items[i];
                    if (!item.Text.ContainsCaseInsensitive(name))
                    {
                        continue;
                    }

                    listView.SelectedItems.Clear();
                    item.Selected = true;
                    item.EnsureVisible();
                    return;
                }
            }
        }

        private void OnClickExtract(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            int id = (int)listView.SelectedItems[0].Tag;

            Sounds.IsValidSound(id, out string name, out _);

            string fileName = Path.Combine(Options.OutputPath, $"{name}");

            if (!fileName.EndsWith(".wav"))
            {
                fileName += ".wav";
            }

            using (MemoryStream stream = new MemoryStream(Sounds.GetSound(id).Buffer))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    stream.WriteTo(fs);
                }
            }

            MessageBox.Show($"Sound saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string path = Options.OutputPath;
            Sounds.Save(path);
            Cursor.Current = Cursors.Default;
            Options.ChangedUltimaClass["Sound"] = false;

            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Files saved successfully.");
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            ListViewItem selected = listView.SelectedItems[0];
            int id = (int)selected.Tag;

            DialogResult result = MessageBox.Show($"Are you sure to remove {selected.Text}?", "Remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            Sounds.Remove(id);

            if (!showFreeSlotsToolStripMenuItem.Checked)
            {
                listView.Items.Remove(selected);
            }
            else
            {
                selected.Text = $"0x{id + _soundIdOffset:X3}";
                selected.ForeColor = Color.Red;
                selected.Font = Font;
            }

            AfterSelect(this, e);
            Options.ChangedUltimaClass["Sound"] = true;
        }

        private void OnClickExportSoundListCsv(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, "SoundList.csv");

            Sounds.SaveSoundListToCsv(fileName, _soundIdOffset);

            FileSavedDialog.Show(FindForm(), fileName, "SoundList saved successfully.");
        }

        public bool SearchId(int id)
        {
            for (int i = 0; i < listView.Items.Count; ++i)
            {
                ListViewItem item = listView.Items[i];

                if ((int)item.Tag != id)
                {
                    continue;
                }

                listView.SelectedItems.Clear();
                item.Selected = true;
                item.EnsureVisible();
                return true;
            }

            return false;
        }

        private void ShowFreeSlotsClick(object sender, EventArgs e)
        {
            Reload();

            nextFreeSlotToolStripMenuItem.Enabled = showFreeSlotsToolStripMenuItem.Checked;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            string file;
            if (sender != null)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose wave file";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "wav file (*.wav)|*.wav";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        file = dialog.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                file = _wavChosen;
            }

            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            int id = (int)listView.SelectedItems[0].Tag;
            string name = Path.GetFileName(file);

            if (!File.Exists(file))
            {
                MessageBox.Show("Invalid Filename", "Add/Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            if (name.Length > 32)
            {
                name = name.Substring(0, 32);
            }

            if (Sounds.IsValidSound(id, out _, out _))
            {
                DialogResult result = MessageBox.Show($"Are you sure to replace {listView.SelectedItems[0].Text}?",
                    "Replace", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            try
            {
                Sounds.Add(id, name, file);
            }
            catch (WaveFormatException waveFormatException)
            {
                MessageBox.Show("Unexpected WAV format:\n" + waveFormatException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ListViewItem item = new ListViewItem($"0x{id + _soundIdOffset:X3} {name}") { Tag = id };

            if (nameSortToolStripMenuItem.Checked)
            {
                item.Text = $"{name} 0x{id + _soundIdOffset:X3}";
            }

            bool done = false;

            for (int i = 0; i < listView.Items.Count; ++i)
            {
                if ((int)listView.Items[i].Tag != id)
                {
                    continue;
                }

                done = true;

                listView.Items.RemoveAt(i);
                listView.Items.Insert(i, item);

                break;
            }

            if (!done)
            {
                listView.Items.Add(item);
                listView.Sort();
            }

            listView.SelectedItems.Clear();
            item.Selected = true;
            item.EnsureVisible();
            listView.Invalidate();

            Options.ChangedUltimaClass["Sound"] = true;
        }

        private void NextFreeSlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int start = listView.SelectedItems.Count > 0 ? listView.SelectedItems[0].Index + 1 : 0;
            for (int i = start; i < listView.Items.Count; ++i)
            {
                ListViewItem item = listView.Items[i];

                if (Sounds.IsValidSound((int)item.Tag, out _, out _))
                {
                    continue;
                }

                listView.SelectedItems.Clear();
                item.Selected = true;
                item.EnsureVisible();
                return;
            }
        }

        private void TreeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                StopSound();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (listView.SelectedItems.Count == 0)
                {
                    return;
                }

                listView.SelectedItems[0].BeginEdit();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (_isEditingLabel)
                {
                    return;
                }

                OnClickPlay(this, e);

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F && e.Control)
            {
                SearchNameTextbox.Focus();

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private bool _isEditingLabel;

        private void ListViewOnAfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            _isEditingLabel = false;
            ListViewItem item = listView.Items[e.Item];
            int id = (int)item.Tag;

            UoSound sound = Sounds.GetSound(id);

            if (sound != null && e.Label != null)
            {
                string newName = e.Label;
                if (newName.Length > 32)
                {
                    newName = newName.Substring(0, 32);
                }

                string oldName = sound.Name;
                sound.Name = newName;
                if (oldName != newName)
                {
                    Options.ChangedUltimaClass["Sound"] = true;
                }
            }

            Sounds.IsValidSound(id, out string name, out _);

            item.Text = nameSortToolStripMenuItem.Checked
                ? $"{name} 0x{id + _soundIdOffset:X3}"
                : $"0x{id + _soundIdOffset:X3} {name}";

            // ListView semantics: CancelEdit=true rejects the framework's
            // auto-apply of e.Label, since we already updated Text above.
            e.CancelEdit = true;
        }

        private void ListView_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            ListViewItem item = listView.Items[e.Item];
            int id = (int)item.Tag;

            if (Sounds.IsValidSound(id, out string name, out bool translated) && !translated)
            {
                _isEditingLabel = true;
                // Seed the in-place edit textbox with the bare name (not the
                // formatted "0x... name" label) so renaming is ergonomic.
                BeginInvoke(new Action(() =>
                {
                    foreach (Control c in listView.Controls)
                    {
                        if (c is TextBox edit)
                        {
                            edit.Text = name;
                            edit.SelectAll();
                            break;
                        }
                    }
                }));
            }
            else
            {
                e.CancelEdit = true;
            }
        }

        private string _wavChosen;

        private void WavChooseInsertButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose wave file";
                dialog.CheckFileExists = true;
                dialog.Filter = "wav file (*.wav)|*.wav";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _wavChosen = dialog.FileName;
                    WavFileInsertTextbox.Text = _wavChosen;
                }
            }
        }

        private void AddInsertReplaceButton_Click(object sender, EventArgs e)
        {
            OnClickReplace(null, e);
        }

        private void SearchByIdButton_Click(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(SearchNameTextbox.Text, out int id))
            {
                return;
            }

            if (!SearchId(id))
            {
                MessageBox.Show($"Can't find Sound with ID {SearchNameTextbox.Text}?");
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F3)
            {
                GoNextResultButton_Click(null, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.F3 | Keys.Shift))
            {
                GoPrevResultButton_Click(null, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SearchByNameButton_Click(object sender, EventArgs e)
        {
            DoSearchName(SearchNameTextbox.Text, false, false);
        }

        private void GoNextResultButton_Click(object sender, EventArgs e)
        {
            DoSearchName(SearchNameTextbox.Text, true, false);
        }

        private void GoPrevResultButton_Click(object sender, EventArgs e)
        {
            DoSearchName(SearchNameTextbox.Text, false, true);
        }

        private void ExportAllSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportAllSounds();
        }

        private void ExportAllSoundsButton_Click(object sender, EventArgs e)
        {
            ExportAllSounds();
        }

        private void ExportAllSounds()
        {
            for (int i = 0; i < _soundsLength; ++i)
            {
                if (!Sounds.IsValidSound(i, out string name, out _))
                {
                    continue;
                }

                string fileName = includeSoundIdCheckBox.Checked
                    ? $"0x{i:X4} {name}"
                    : $"{name}";

                string path = Path.Combine(Options.OutputPath, fileName);

                if (!path.EndsWith(".wav"))
                {
                    path += ".wav";
                }

                using (MemoryStream stream = new MemoryStream(Sounds.GetSound(i).Buffer))
                {
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        stream.WriteTo(fs);
                    }
                }
            }

            MessageBox.Show("Extract all sounds complete.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
    }
}
