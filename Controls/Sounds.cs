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
using System.IO;
using System.Windows.Forms;

namespace FiddlerControls
{
    public partial class Sounds : UserControl
    {
        public Sounds()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private System.Media.SoundPlayer sp;

        private bool Loaded = false;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            checkBox.Checked = false;
            OnLoad(this, EventArgs.Empty);
        }
        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Sound"] = true;

            string name = "";
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            List<TreeNode> cache = new List<TreeNode>();
            for (int i = 1; i <= 0xFFF; ++i)
            {
                if (Ultima.Sounds.IsValidSound(i - 1, out name))
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X3} {1}", i, name));
                    node.Tag = i;
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
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void PlaySound(object sender, EventArgs e)
        {
            sp.Stop();
            if (treeView.SelectedNode == null)
                return;
            Ultima.UOSound s = Ultima.Sounds.GetSound((int)treeView.SelectedNode.Tag - 1);
            using (MemoryStream m = new MemoryStream(s.buffer))
            {
                sp.Stream = m;
                sp.Play();
            }
        }

        private void OnDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            sp.Stop();
            if (treeView.SelectedNode == null)
                return;
            Ultima.UOSound s = Ultima.Sounds.GetSound((int)e.Node.Tag - 1);
            using (MemoryStream m = new MemoryStream(s.buffer))
            {
                sp.Stream = m;
                sp.Play();
            }
        }

        private void afterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;
            seconds.Text = String.Format("{0:f}s", Ultima.Sounds.GetSoundLength((int)treeView.SelectedNode.Tag - 1));
        }

        private void OnChangeSort(object sender, EventArgs e)
        {
            string delimiter = " ";
            char[] delim = delimiter.ToCharArray();
            string[] name;
            treeView.BeginUpdate();
            for (int i = 0; i < treeView.Nodes.Count; ++i)
            {
                name = treeView.Nodes[i].Text.Split(delim);
                treeView.Nodes[i].Text = String.Format("{0} {1} ", name[1], name[0]);
            }
            treeView.Sort();
            treeView.EndUpdate();
        }

        private void SearchName(object sender, EventArgs e)
        {
            if (!DoSearchName(textBox1.Text, false))
                MessageBox.Show("No sound found", "Result", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
        }

        private void SearchNext(object sender, EventArgs e)
        {
            if (!DoSearchName(textBox1.Text, true))
                MessageBox.Show("No sound found", "Result", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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

            int number;
            bool numeric = FiddlerControls.Utils.ConvertStringToInt(name, out number);

            for (int i = index; i < treeView.Nodes.Count; ++i)
            {
                TreeNode node = treeView.Nodes[i];
                if (numeric)
                {
                    if ((int)node.Tag == number)
                    {
                        treeView.SelectedNode = node;
                        node.EnsureVisible();
                        return true;
                    }
                }
                else if (node.Text.Contains(name))
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
            Ultima.Sounds.IsValidSound(id, out name);
            string FileName = Path.Combine(FiddlerControls.Options.OutputPath, String.Format("{0}", name));
            using (MemoryStream stream = new MemoryStream(Ultima.Sounds.GetSound(id).buffer))
            {
                using (FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    stream.WriteTo(fs);
                }
            }
            MessageBox.Show(String.Format("Sound saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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
                        MessageBox.Show(String.Format("Are you sure to remove {0}", treeView.SelectedNode.Text), "Remove",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Ultima.Sounds.Remove(id);
                treeView.SelectedNode.Remove();
                Options.ChangedUltimaClass["Sound"] = true;
            }
        }

        private void OnClickAddReplace(object sender, EventArgs e)
        {
            int id;
            if (Utils.ConvertStringToInt(textBoxID.Text, out id, 1, 0xFFF))
            {
                string name = textBoxName.Text;
                if (name != null)
                {
                    if (name.Length > 32)
                        name = name.Substring(0, 32);
                    string filename = textBoxWav.Text;
                    if (File.Exists(filename))
                    {
                        Ultima.Sounds.Add(id - 1, name, filename);

                        TreeNode node = new TreeNode(String.Format("0x{0:X3} {1}", id, name));
                        if (checkBox.Checked)
                            node.Text = String.Format("{1} 0x{0:X3}", id, name);
                        node.Tag = id;
                        bool done = false;
                        for (int i = 0; i < treeView.Nodes.Count; ++i)
                        {
                            if ((int)treeView.Nodes[i].Tag == id)
                            {
                                done = true;
                                treeView.Nodes.RemoveAt(i);
                                treeView.Nodes.Insert(i, node);
                                break;
                            }
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
                    else
                    {
                        MessageBox.Show(
                            "Invalid Filename",
                            "Add/Replace",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1);
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Invalid Name",
                        "Add/Replace",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }

            }
            else
            {
                MessageBox.Show(
                    "Invalid ID",
                    "Add/Replace",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickSelectWav(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose wave file to add";
                dialog.CheckFileExists = true;
                dialog.Filter = "wav file (*.wav)|*.wav";
                if (dialog.ShowDialog() == DialogResult.OK)
                    textBoxWav.Text = dialog.FileName;
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
    }
}
