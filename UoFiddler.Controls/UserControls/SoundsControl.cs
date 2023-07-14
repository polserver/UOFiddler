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

            treeView.LabelEdit = true;
            treeView.BeforeLabelEdit += TreeView_BeforeLabelEdit;
            treeView.AfterLabelEdit += TreeViewOnAfterLabelEdit;

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

            if (treeView.SelectedNode != null)
            {
                oldItem = (int)treeView.SelectedNode.Tag;
            }

            treeView.BeginUpdate();
            try
            {
                treeView.Nodes.Clear();

                _soundIdOffset = GetSoundIdOffset();

                var cache = new List<TreeNode>();
                for (int i = 0; i < _soundsLength; ++i)
                {
                    if (Sounds.IsValidSound(i, out string name, out bool translated))
                    {
                        TreeNode node = new TreeNode($"0x{i + _soundIdOffset:X3} {name}")
                        {
                            Tag = i
                        };

                        if (translated)
                        {
                            node.ForeColor = Color.Blue;
                            node.NodeFont = new Font(Font, FontStyle.Underline);
                        }

                        cache.Add(node);
                    }
                    else if (showFreeSlotsToolStripMenuItem.Checked)
                    {
                        TreeNode node = new TreeNode($"0x{i:X3} ")
                        {
                            Tag = i,
                            ForeColor = Color.Red
                        };

                        cache.Add(node);
                    }
                }

                treeView.Nodes.AddRange(cache.ToArray());
            }
            finally
            {
                treeView.EndUpdate();
            }

            if (treeView.Nodes.Count > 0)
            {
                treeView.SelectedNode = treeView.Nodes[0];
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
            PlaySound((int)treeView.SelectedNode.Tag);
        }

        private void OnDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            PlaySound((int)e.Node.Tag);
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

            if (treeView.SelectedNode == null)
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

        private void BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (_playing)
            {
                StopSound();
            }
        }

        private void AfterSelect(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                playSoundToolStripMenuItem.Enabled = false;
                extractSoundToolStripMenuItem.Enabled = false;
                removeSoundToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Text = "Insert/Replace";
            }

            if (treeView.SelectedNode != null)
            {
                double length = Sounds.GetSoundLength((int)treeView.SelectedNode.Tag);
                seconds.Text = length > 0 ? $"{length:f}s" : "Empty Slot";
            }

            bool isValidSound = treeView.SelectedNode != null && Sounds.IsValidSound((int)treeView.SelectedNode.Tag, out _, out _);

            playSoundToolStripMenuItem.Enabled = isValidSound;
            extractSoundToolStripMenuItem.Enabled = isValidSound;
            removeSoundToolStripMenuItem.Enabled = isValidSound;

            replaceToolStripMenuItem.Enabled = true;
            replaceToolStripMenuItem.Text = isValidSound ? "Replace" : "Insert";

            SelectedSoundGroup.Visible = treeView.SelectedNode != null;

            if (treeView.SelectedNode != null)
            {
                SelectedSoundGroup.Text = $"Current Sound: {treeView.SelectedNode.Text} - Duration: {seconds.Text}";
                IdInsertTextbox.Text = $"0x{(int)treeView.SelectedNode.Tag + _soundIdOffset:X}";
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
            if (treeView.SelectedNode != null)
            {
                oldItem = (int)treeView.SelectedNode.Tag;
            }

            const string delimiter = " ";

            treeView.BeginUpdate();

            for (int i = 0; i < treeView.Nodes.Count; ++i)
            {
                string name = treeView.Nodes[i].Text;

                int splitIndex = nameSortToolStripMenuItem.Checked
                    ? name.IndexOf(delimiter, StringComparison.Ordinal)
                    : name.LastIndexOf(delimiter, StringComparison.Ordinal);

                treeView.Nodes[i].Text = $"{name.Substring(splitIndex).Trim()} {name.Substring(0, splitIndex).Trim()}";
            }

            treeView.Sort();
            treeView.EndUpdate();

            if (oldItem != null)
            {
                SearchId(oldItem.Value);
            }
        }

        private void DoSearchName(string name, bool next, bool prev)
        {
            int index = 0;

            if (prev)
            {
                if (treeView.SelectedNode.Index >= 0)
                {
                    index = treeView.SelectedNode.Index - _soundIdOffset;
                }

                if (index <= 0)
                {
                    index = 0;
                }

                for (int i = index - 1; i >= 0; --i)
                {
                    TreeNode node = treeView.Nodes[i];
                    if (!node.Text.ContainsCaseInsensitive(name))
                    {
                        continue;
                    }

                    treeView.SelectedNode = node;

                    node.EnsureVisible();

                    return;
                }
            }
            else
            {
                if (next)
                {
                    if (treeView.SelectedNode.Index >= 0)
                    {
                        index = treeView.SelectedNode.Index + 1;
                    }

                    if (index >= treeView.Nodes.Count)
                    {
                        index = 0;
                    }
                }

                for (int i = index; i < treeView.Nodes.Count; ++i)
                {
                    TreeNode node = treeView.Nodes[i];
                    if (!node.Text.ContainsCaseInsensitive(name))
                    {
                        continue;
                    }

                    treeView.SelectedNode = node;

                    node.EnsureVisible();

                    return;
                }
            }
        }

        private void OnClickExtract(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            int id = (int)treeView.SelectedNode.Tag;

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
            MessageBox.Show($"Saved to {path}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Sound"] = false;
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                return;
            }

            int id = (int)treeView.SelectedNode.Tag;

            DialogResult result = MessageBox.Show($"Are you sure to remove {treeView.SelectedNode.Text}?", "Remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            Sounds.Remove(id);

            if (!showFreeSlotsToolStripMenuItem.Checked)
            {
                treeView.SelectedNode.Remove();
            }
            else
            {
                treeView.SelectedNode.Text = $"0x{(int)treeView.SelectedNode.Tag + _soundIdOffset:X3}";
                treeView.SelectedNode.ForeColor = Color.Red;
            }

            AfterSelect(this, e);
            Options.ChangedUltimaClass["Sound"] = true;
        }

        private void OnClickExportSoundListCsv(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, "SoundList.csv");

            Sounds.SaveSoundListToCsv(fileName, _soundIdOffset);

            MessageBox.Show($"SoundList saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        public bool SearchId(int id)
        {
            for (int i = 0; i < treeView.Nodes.Count; ++i)
            {
                TreeNode node = treeView.Nodes[i];

                if ((int)node.Tag != id)
                {
                    continue;
                }

                treeView.SelectedNode = node;
                node.EnsureVisible();

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

            int id = (int)treeView.SelectedNode.Tag;
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
                DialogResult result = MessageBox.Show($"Are you sure to replace {treeView.SelectedNode.Text}?",
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

            TreeNode node = new TreeNode($"0x{id + _soundIdOffset:X3} {name}");

            if (nameSortToolStripMenuItem.Checked)
            {
                node.Text = $"{name} 0x{id + _soundIdOffset:X3}";
            }

            node.Tag = id;

            bool done = false;

            for (int i = 0; i < treeView.Nodes.Count; ++i)
            {
                if ((int)treeView.Nodes[i].Tag != id)
                {
                    continue;
                }

                done = true;

                treeView.Nodes.RemoveAt(i);
                treeView.Nodes.Insert(i, node);

                break;
            }

            if (!done)
            {
                treeView.Nodes.Add(node);
                treeView.Sort();
            }

            node.EnsureVisible();

            treeView.SelectedNode = node;
            treeView.Invalidate();

            Options.ChangedUltimaClass["Sound"] = true;
        }

        private void NextFreeSlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = treeView.Nodes.IndexOf(treeView.SelectedNode) + 1; i < treeView.Nodes.Count; ++i)
            {
                TreeNode node = treeView.Nodes[i];

                if (Sounds.IsValidSound((int)node.Tag, out _, out _))
                {
                    continue;
                }

                treeView.SelectedNode = node;

                node.EnsureVisible();

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
                if (treeView.SelectedNode == null)
                {
                    return;
                }

                treeView.SelectedNode.BeginEdit();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (treeView.Nodes.OfType<TreeNode>().Any(n => n.IsEditing))
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

        private void TreeViewOnAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            int id = (int)e.Node.Tag;

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

            e.Node.Text = $"0x{id + _soundIdOffset:X3} {name}";

            if (nameSortToolStripMenuItem.Checked)
            {
                e.Node.Text = $"{name} 0x{id + _soundIdOffset:X3}";
            }

            e.CancelEdit = true;
        }

        private void TreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            int id = (int)e.Node.Tag;

            if (Sounds.IsValidSound(id, out string name, out bool translated) && !translated)
            {
                treeView.SetEditText(name);
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
