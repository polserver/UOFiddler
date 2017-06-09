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
using System.Runtime.InteropServices;
using System.Windows.Forms;

using FiddlerControls.Helpers;

using Ultima;

namespace FiddlerControls
{
    public partial class Sounds : UserControl
    {
        private static Sounds refMarker;

        public Sounds()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refMarker = this;
            this.sp_timer = new Timer();
            sp_timer.Tick += this.OnSpTimerTick;

            treeView.LabelEdit = true;
            treeView.BeforeLabelEdit += TreeView_BeforeLabelEdit;
            treeView.AfterLabelEdit += TreeViewOnAfterLabelEdit;
        }

        public static Sounds RefMarker => refMarker;

        private System.Media.SoundPlayer sp;
        private Timer sp_timer;
        private int sp_timer_max;
        private DateTime sp_timer_start;

        private bool Loaded = false;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            nameSortToolStripMenuItem.Checked = false;
            OnLoad(this, EventArgs.Empty);
        }
        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Sound"] = true;

            int? oldItem = null;
            if (this.treeView.SelectedNode != null)
            {
                oldItem = ((int)treeView.SelectedNode.Tag) - 1;
            }

            string name = "";
            bool translated = false;
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            List<TreeNode> cache = new List<TreeNode>();
            for (int i = 1; i <= 0xFFF; ++i)
            {
                if (Ultima.Sounds.IsValidSound(i - 1, out name, out translated))
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X3} {1}", i - 1, name));
                    node.Tag = i;
                    if (translated)
                    {
                        node.ForeColor = Color.Blue;
                        node.NodeFont = new Font(this.Font, FontStyle.Underline);
                    }

                    cache.Add(node);
                }
                else if (showFreeSlotsToolStripMenuItem.Checked)
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X3} ", i - 1));
                    node.Tag = i;
                    node.ForeColor = Color.Red;
                    cache.Add(node);
                }
            }
            treeView.Nodes.AddRange(cache.ToArray());

            treeView.EndUpdate();
            if (treeView.Nodes.Count > 0)
                treeView.SelectedNode = treeView.Nodes[0];
            sp = new System.Media.SoundPlayer();
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;

            Cursor.Current = Cursors.Default;

            if (oldItem != null)
            {
                SearchID(oldItem.Value);
            }
        }

        private void OnSpTimerTick(object sender, EventArgs eventArgs)
        {
            this.BeginInvoke((Action)(() =>
                {
                    var diff = DateTime.Now - sp_timer_start;
                    playing.Value = Math.Min(100, (int)((diff.TotalMilliseconds * 100d) / this.sp_timer_max));

                    if (diff.TotalMilliseconds >= sp_timer_max)
                    {
                        playing.Visible = false;
                        stopButton.Visible = false;
                        sp_timer.Stop();
                    }
                }));
        }

        public static bool SearchName(string text, bool v)
        {
            return RefMarker.DoSearchName(text, v);
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickPlay(object sender, EventArgs e)
        {
            this.PlaySound((int)treeView.SelectedNode.Tag - 1);
        }

        private void OnDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.PlaySound((int)e.Node.Tag - 1);
        }

        private void OnClickStop(object sender, EventArgs e)
        {
            this.StopSound();
        }

        private void StopSound()
        {
            this.sp.Stop();
            this.sp_timer.Stop();
            this.playing.Visible = false;
            this.stopButton.Visible = false;
        }

        private void PlaySound(int id)
        {
            this.sp.Stop();
            sp_timer.Stop();
            playing.Visible = false;
            stopButton.Visible = false;

            if (this.treeView.SelectedNode == null)
            {
                return;
            }

            Ultima.UOSound s = Ultima.Sounds.GetSound(id);
            if (s == null)
            {
                return;
            }

            using (MemoryStream m = new MemoryStream(s.buffer))
            {
                this.sp.Stream = m;
                this.sp.Play();

                playing.Value = 0;
                playing.Visible = true;
                stopButton.Visible = true;
                this.sp_timer_start = DateTime.Now;
                this.sp_timer_max = (int)(Ultima.Sounds.GetSoundLength(id) * 1000);
                sp_timer.Interval = 50;
                sp_timer.Start();
            }
        }

        private void afterSelect(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
            {
                playSoundToolStripMenuItem.Enabled = false;
                extractSoundToolStripMenuItem.Enabled = false;
                removeSoundToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Enabled = false;
                replaceToolStripMenuItem.Text = "Insert/Replace";
            }

            var length = Ultima.Sounds.GetSoundLength((int)treeView.SelectedNode.Tag - 1);
            seconds.Text = length > 0
                ? String.Format("{0:f}s", length)
                : "Empty Slot";

            string name;
            bool translated;
            var isValidSound = Ultima.Sounds.IsValidSound((int)this.treeView.SelectedNode.Tag - 1, out name, out translated);

            playSoundToolStripMenuItem.Enabled = isValidSound;
            extractSoundToolStripMenuItem.Enabled = isValidSound;
            removeSoundToolStripMenuItem.Enabled = isValidSound;
            replaceToolStripMenuItem.Enabled = true;
            replaceToolStripMenuItem.Text = isValidSound ? "Replace" : "Insert";
        }

        private void OnChangeSort(object sender, EventArgs e)
        {
            if (this.showFreeSlotsToolStripMenuItem.Checked)
            {
                this.showFreeSlotsToolStripMenuItem.Checked = false;
                this.nextFreeSlotToolStripMenuItem.Enabled = false;
                this.Reload();
                this.nameSortToolStripMenuItem.Checked = true;
            }

            int? oldItem = null;
            if (this.treeView.SelectedNode != null)
            {
                oldItem = ((int)treeView.SelectedNode.Tag) - 1;
            }

            string delimiter = " ";
            char[] delim = delimiter.ToCharArray();
            string name;
            int splitIndex = 0;
            treeView.BeginUpdate();
            for (int i = 0; i < treeView.Nodes.Count; ++i)
            {
                name = treeView.Nodes[i].Text;
                if (this.nameSortToolStripMenuItem.Checked)
                {
                    splitIndex = name.IndexOf(delimiter);
                }
                else
                {
                    splitIndex = name.LastIndexOf(delimiter);
                }

                treeView.Nodes[i].Text = String.Format("{0} {1}", name.Substring(splitIndex + 1), name.Substring(0, splitIndex));
            }

            treeView.Sort();
            treeView.EndUpdate();

            if (oldItem != null)
            {
                SearchID(oldItem.Value);
            }
        }

        private bool DoSearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (treeView.SelectedNode.Index >= 0)
                    index = treeView.SelectedNode.Index + 1;
                if (index >= treeView.Nodes.Count)
                    index = 0;
            }

            for (int i = index; i < treeView.Nodes.Count; ++i)
            {
                TreeNode node = treeView.Nodes[i];
                if (node.Text.ContainsCaseInsensitive(name))
                {
                    treeView.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
            }
            return false;
        }

        private void OnClickExtract(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;
            int id = (int)treeView.SelectedNode.Tag - 1;
            string name = "";
            bool translated = false;
            Ultima.Sounds.IsValidSound(id, out name, out translated);
            string fileName = Path.Combine(FiddlerControls.Options.OutputPath, String.Format("{0}", name));
            if (!fileName.EndsWith(".wav"))
            {
                fileName = fileName + ".wav";
            }

            using (MemoryStream stream = new MemoryStream(Ultima.Sounds.GetSound(id).buffer))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    stream.WriteTo(fs);
                }
            }
            MessageBox.Show(String.Format("Sound saved to {0}", fileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            string path = FiddlerControls.Options.OutputPath;
            Ultima.Sounds.Save(path);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(
                    String.Format("Saved to {0}", path),
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Sound"] = false;
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;
            int id = (int)treeView.SelectedNode.Tag - 1;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove {0}?", treeView.SelectedNode.Text), "Remove",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Ultima.Sounds.Remove(id);
                if (!showFreeSlotsToolStripMenuItem.Checked)
                {
                    treeView.SelectedNode.Remove();
                }
                else
                {
                    treeView.SelectedNode.Text = string.Format("0x{0:X3}", (int)treeView.SelectedNode.Tag - 1);
                    treeView.SelectedNode.ForeColor = Color.Red;
                }

                afterSelect(this, e);
                Options.ChangedUltimaClass["Sound"] = true;
            }
        }

        private void OnClickExtractSoundList(object sender, EventArgs e)
        {
            string FileName = Path.Combine(FiddlerControls.Options.OutputPath, "SoundList.csv");
            Ultima.Sounds.SaveSoundListToCSV(FileName);
            MessageBox.Show(String.Format("SoundList saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private SoundsSearch showform = null;

        private void SearchClick(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new SoundsSearch();
                showform.TopMost = true;
                showform.Show();
            }
        }

        public static bool SearchID(int id)
        {
            for (int i = 0; i < refMarker.treeView.Nodes.Count; ++i)
            {
                TreeNode node = refMarker.treeView.Nodes[i];
                if ((int)node.Tag == id + 1)
                {
                    refMarker.treeView.SelectedNode = node;
                    node.EnsureVisible();
                    return true;
                }
            }

            return false;
        }

        private void ShowFreeSlotsClick(object sender, EventArgs e)
        {
            Reload();

            this.nextFreeSlotToolStripMenuItem.Enabled = this.showFreeSlotsToolStripMenuItem.Checked;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            string file = null;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose wave file";
                dialog.CheckFileExists = true;
                dialog.Filter = "wav file (*.wav)|*.wav";
                if (dialog.ShowDialog() == DialogResult.OK)
                    file = dialog.FileName;
                else
                    return;
            }

            int id = (int)this.treeView.SelectedNode.Tag;
            string name = Path.GetFileName(file);
            if (name.Length > 32)
            {
                name = name.Substring(0, 32);
            }

            if (!File.Exists(file))
            {
                MessageBox.Show("Invalid Filename", "Add/Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            string orgName;
            bool translated;
            if (Ultima.Sounds.IsValidSound(id - 1, out orgName, out translated))
            {
                DialogResult result = MessageBox.Show(
                    String.Format("Are you sure to replace {0}?", treeView.SelectedNode.Text),
                    "Replace",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            try
            {
                Ultima.Sounds.Add(id - 1, name, file);
            }
            catch (WaveFormatException waveFormatException)
            {
                MessageBox.Show("Unexpected WAV format:\n" + waveFormatException.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TreeNode node = new TreeNode(String.Format("0x{0:X3} {1}", id - 1, name));
            if (this.nameSortToolStripMenuItem.Checked)
            {
                node.Text = String.Format("{1} 0x{0:X3}", id - 1, name);
            }

            node.Tag = id;
            bool done = false;
            for (int i = 0; i < this.treeView.Nodes.Count; ++i)
            {
                if ((int)this.treeView.Nodes[i].Tag == id)
                {
                    done = true;
                    this.treeView.Nodes.RemoveAt(i);
                    this.treeView.Nodes.Insert(i, node);
                    break;
                }
            }
            if (!done)
            {
                this.treeView.Nodes.Add(node);
                this.treeView.Sort();
            }

            node.EnsureVisible();
            this.treeView.SelectedNode = node;
            this.treeView.Invalidate();
            Options.ChangedUltimaClass["Sound"] = true;
        }

        private void nextFreeSlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = treeView.Nodes.IndexOf(treeView.SelectedNode) + 1; i < treeView.Nodes.Count; ++i)
            {
                TreeNode node = refMarker.treeView.Nodes[i];
                string name;
                bool translated;
                if (!Ultima.Sounds.IsValidSound((int)node.Tag - 1, out name, out translated))
                {
                    treeView.SelectedNode = node;
                    node.EnsureVisible();
                    return;
                }
            }
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.StopSound();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                if (this.treeView.SelectedNode != null)
                {
                    this.treeView.SelectedNode.BeginEdit();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                if (!this.treeView.Nodes.OfType<TreeNode>().Any(n => n.IsEditing))
                {
                    this.OnClickPlay(this, e);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
          else  if (e.KeyCode == Keys.F && e.Control)
            {
                this.SearchClick(sender, e);
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void TreeViewOnAfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            var id = ((int)e.Node.Tag) - 1;
            var sound = Ultima.Sounds.GetSound(id);
            if (sound != null && e.Label != null)
            {
                var newName = e.Label;
                if (newName.Length > 32)
                {
                    newName = newName.Substring(0, 32);
                }

                var oldName = sound.Name;
                sound.Name = newName;
                if (oldName != newName)
                {
                    Options.ChangedUltimaClass["Sound"] = true;
                }
            }

            string name;
            bool translated;
            Ultima.Sounds.IsValidSound(id, out name, out translated);
            e.Node.Text = String.Format("0x{0:X3} {1}", id, name);
            if (this.nameSortToolStripMenuItem.Checked)
            {
                e.Node.Text = String.Format("{1} 0x{0:X3}", id, name);
            }

            e.CancelEdit = true;
        }

        private void TreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            var id = ((int)e.Node.Tag) - 1;
            string name;
            bool translated;
            if (Ultima.Sounds.IsValidSound(id, out name, out translated) && !translated)
            {
                this.treeView.SetEditText(name);
            }
            else
            {
                e.CancelEdit = true;
            }
        }
    }
}
