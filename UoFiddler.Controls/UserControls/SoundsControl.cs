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
using Ultima.Uop;
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

        // Shared underline font for translated entries — one instance reused
        // across all list items, recreated each reload (the control Font may change).
        private Font _underlineFont;

        // Localization support
        private Func<string, string?>? _localizationGetter;

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

            using (new WaitCursorScope(this))
            {
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

                    _underlineFont?.Dispose();
                    _underlineFont = new Font(Font, FontStyle.Underline);

                    var cache = new List<ListViewItem>();
                    for (int i = 0; i < _soundsLength; ++i)
                    {
                        if (Sounds.IsValidSound(i, out string name, out bool translated))
                        {
                            var item = new ListViewItem($"0x{i + _soundIdOffset:X3} {name}") { Tag = i };

                            if (translated)
                            {
                                item.ForeColor = Options.DarkMode ? Color.CornflowerBlue : Color.Blue;
                                item.Font = _underlineFont;
                            }

                            cache.Add(item);
                        }
                        else if (showFreeSlotsToolStripMenuItem.Checked)
                        {
                            cache.Add(new ListViewItem($"0x{i:X3} ")
                            {
                                Tag = i,
                                ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red
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

                if (oldItem != null)
                {
                    SearchId(oldItem.Value);
                }
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

        /// <summary>
        /// 设置本地化的字符串获取委托
        /// </summary>
        public void SetLocalization(Func<string, string?>? getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// 应用本地化翻译到所有 UI 控件
        /// </summary>
        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // Type 1: 菜单项文本汉化
            nameSortToolStripMenuItem.Text = _localizationGetter("Forms.SoundsControl.nameSortToolStripMenuItem") ?? "Name Sort";
            showFreeSlotsToolStripMenuItem.Text = _localizationGetter("Forms.SoundsControl.showFreeSlotsToolStripMenuItem") ?? "Show free slots";
            nextFreeSlotToolStripMenuItem.Text = _localizationGetter("Forms.SoundsControl.nextFreeSlotToolStripMenuItem") ?? "Find next free slot";
            playSoundToolStripMenuItem.Text = _localizationGetter("Forms.SoundsControl.playSoundToolStripMenuItem") ?? "Play";
            replaceToolStripMenuItem.Text = _localizationGetter("Forms.SoundsControl.replaceToolStripMenuItem") ?? "Insert/Replace";
            extractSoundToolStripMenuItem.Text = _localizationGetter("Forms.SoundsControl.extractSoundToolStripMenuItem") ?? "Extract";
            removeSoundToolStripMenuItem.Text = _localizationGetter("Forms.SoundsControl.removeSoundToolStripMenuItem") ?? "Remove";
            itemSave.Text = _localizationGetter("Forms.SoundsControl.itemSave") ?? "Save";

            // Type 1: GroupBox 标题
            groupBox1.Text = _localizationGetter("Forms.SoundsControl.groupBox1") ?? "Generic";
            SelectedSoundGroup.Text = _localizationGetter("Forms.SoundsControl.SelectedSoundGroup") ?? "Current Sound";
            groupBox2.Text = _localizationGetter("Forms.SoundsControl.groupBox2") ?? "Search";
            groupBox3.Text = _localizationGetter("Forms.SoundsControl.groupBox3") ?? "Insert/Replace";

            // Type 1: 按钮文本
            PlaySoundButton.Text = _localizationGetter("Forms.SoundsControl.PlaySoundButton") ?? "Play";
            StopSoundButton.Text = _localizationGetter("Forms.SoundsControl.StopSoundButton") ?? "Stop";
            ExtractSoundButton.Text = _localizationGetter("Forms.SoundsControl.ExtractSoundButton") ?? "Extract";
            RemoveSoundButton.Text = _localizationGetter("Forms.SoundsControl.RemoveSoundButton") ?? "Remove";
            SaveFileButton.Text = _localizationGetter("Forms.SoundsControl.SaveFileButton") ?? "Save";
            exportAllSoundsButton.Text = _localizationGetter("Forms.SoundsControl.exportAllSoundsButton") ?? "Export all sounds";
            ExportSoundListCsvButton.Text = _localizationGetter("Forms.SoundsControl.ExportSoundListCsvButton") ?? "Export sound list (.csv)";
            WavChooseInsertButton.Text = _localizationGetter("Forms.SoundsControl.WavChooseInsertButton") ?? "...";
            AddInsertReplaceButton.Text = _localizationGetter("Forms.SoundsControl.AddInsertReplaceButton") ?? "Add";
            GoPrevResultButton.Text = _localizationGetter("Forms.SoundsControl.GoPrevResultButton") ?? "< Prev";
            GoNextResultButton.Text = _localizationGetter("Forms.SoundsControl.GoNextResultButton") ?? "Next >";
            SearchByIdButton.Text = _localizationGetter("Forms.SoundsControl.SearchByIdButton") ?? "Search";
            SearchByNameButton.Text = _localizationGetter("Forms.SoundsControl.SearchByNameButton") ?? "Search";

            // Type 1: 标签和复选框
            label1.Text = _localizationGetter("Forms.SoundsControl.label1") ?? "ID:";
            label3.Text = _localizationGetter("Forms.SoundsControl.label3") ?? "WAV File:";
            SortByNameCheckbox.Text = _localizationGetter("Forms.SoundsControl.SortByNameCheckbox") ?? "Sort tree by name";
            includeSoundIdCheckBox.Text = _localizationGetter("Forms.SoundsControl.includeSoundIdCheckBox") ?? "Export with sound id in file name";

            // Type 1: 列表列头
            listViewColumn.Text = _localizationGetter("Forms.SoundsControl.listViewColumn") ?? "Sound";

            // Type 1: 工具栏文本
            stopButton.Text = _localizationGetter("Forms.SoundsControl.stopButton") ?? "Stop";
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
                // Type 12 初始值设置（在 ApplyLocalization 中已设置，此处保持为保障）
                replaceToolStripMenuItem.Text = _localizationGetter?.Invoke("Forms.SoundsControl.replaceToolStripMenuItem") ?? "Insert/Replace";
            }

            if (selected != null)
            {
                double length = Sounds.GetSoundLength((int)selected.Tag);
                // Type 8：动态文本 - 空插槽提示
                string emptySlotText = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.emptySlot") ?? "Empty Slot";
                seconds.Text = length > 0 ? $"{length:f}s" : emptySlotText;
            }

            bool isValidSound = selected != null && Sounds.IsValidSound((int)selected.Tag, out _, out _);

            playSoundToolStripMenuItem.Enabled = isValidSound;
            extractSoundToolStripMenuItem.Enabled = isValidSound;
            removeSoundToolStripMenuItem.Enabled = isValidSound;

            replaceToolStripMenuItem.Enabled = true;
            // Type 8：动态文本 - Replace/Insert 菜单项文本切换
            if (isValidSound)
            {
                replaceToolStripMenuItem.Text = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.replace") ?? "Replace";
            }
            else
            {
                replaceToolStripMenuItem.Text = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.insert") ?? "Insert";
            }

            SelectedSoundGroup.Visible = selected != null;

            if (selected != null)
            {
                // Type 8：动态文本 - GroupBox 标题，包含选中音效信息
                SelectedSoundGroup.Text = $"{_localizationGetter?.Invoke("Forms.SoundsControl.Messages.currentSoundFormat") ?? "Current Sound"}: {selected.Text} - {_localizationGetter?.Invoke("Forms.SoundsControl.Messages.duration") ?? "Duration"}: {seconds.Text}";
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

            // Type 6：消息框文本汉化 - 提取成功提示
            string message = string.Format(
                _localizationGetter?.Invoke("Forms.SoundsControl.Messages.soundSavedTo") ?? "Sound saved to {0}",
                fileName);
            string title = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.saved") ?? "Saved";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            ClientFileSaveCommand.Run(this, FileType.SoundLegacyMul, Sounds.Save, "Sound");
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
                selected.ForeColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
                selected.Font = Font;
            }

            AfterSelect(this, e);
            Options.ChangedUltimaClass["Sound"] = true;
        }

        private void OnClickExportSoundListCsv(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, "SoundList.csv");

            Sounds.SaveSoundListToCsv(fileName, _soundIdOffset);

            // Type 6：消息框文本汉化 - 导出音效列表完成
            string message = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.soundListSavedSuccessfully") ?? "SoundList saved successfully.";
            var dialog = new FileSavedDialog(fileName, message);
            if (_localizationGetter != null)
            {
                dialog.SetLocalization(_localizationGetter);
            }
            dialog.ShowDialog(FindForm());
            dialog.Dispose();
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
                    dialog.Title = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.chooseWaveFile") ?? "Choose wave file";
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

            // ✅ 修复：检查 file 是否为空或文件是否存在
            if (string.IsNullOrEmpty(file) || !File.Exists(file))
            {
                string errorMsg = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.invalidFilename") ?? "Invalid Filename";
                string titleMsg = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.addReplaceTitle") ?? "Add/Replace";
                MessageBox.Show(errorMsg, titleMsg, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;  // ✅ 修复：缺少的 return 语句
            }

            if (listView.SelectedItems.Count == 0)
            {
                return;
            }

            int id = (int)listView.SelectedItems[0].Tag;
            string name = Path.GetFileName(file);

            if (name.Length > 32)
            {
                name = name.Substring(0, 32);
            }

            if (Sounds.IsValidSound(id, out _, out _))
            {
                string confirmMsg = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.confirmReplace") ?? $"Are you sure to replace {listView.SelectedItems[0].Text}?";
                confirmMsg = string.Format(confirmMsg, listView.SelectedItems[0].Text);
                string titleMsg = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.replaceTitle") ?? "Replace";
                DialogResult result = MessageBox.Show(confirmMsg, titleMsg, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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
                string errorMsg = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.unexpectedWavFormat") ?? "Unexpected WAV format:";
                string titleMsg = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.errorTitle") ?? "Error";
                MessageBox.Show(errorMsg + "\n" + waveFormatException.Message, titleMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            // Type 6：消息框文本汉化 - 导出所有完成提示
            string message = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.extractAllComplete") ?? "Extract all sounds complete.";
            string title = _localizationGetter?.Invoke("Forms.SoundsControl.Messages.saved") ?? "Saved";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
    }
}
