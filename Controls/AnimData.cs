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
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class AnimData : UserControl
    {
        public AnimData()
        {
            InitializeComponent();
            splitContainer2.Panel2MinSize = 190;
            splitContainer2.SplitterDistance = splitContainer2.Width - splitContainer2.Panel2MinSize - splitContainer2.SplitterWidth;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }
        private static bool Loaded;
        private Animdata.Data selData;
        private int m_currAnim;
        private int curframe;
        private Timer m_Timer;
        private int Timer_frame;

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrAnim
        {
            get { return m_currAnim; }
            set
            {
                if (!Loaded)
                    return;
                selData = (Animdata.Data)Animdata.AnimData[value];
                if (m_currAnim != value)
                {
                    treeViewFrames.BeginUpdate();
                    treeViewFrames.Nodes.Clear();
                    for (int i = 0; i < selData.FrameCount; ++i)
                    {
                        TreeNode node = new TreeNode();
                        int frame = value + selData.FrameData[i];
                        node.Text = String.Format("0x{0:X4} {1}", frame, TileData.ItemTable[frame].Name);
                        treeViewFrames.Nodes.Add(node);
                    }
                    treeViewFrames.EndUpdate();
                }
                m_currAnim = value;

                if (m_Timer == null)
                {
                    int graphic = value;
                    if (curframe > -1)
                        graphic += selData.FrameData[curframe];
                    pictureBox1.Image = Art.GetStatic(graphic);
                }
                numericUpDownFrameDelay.Value = selData.FrameInterval;
                numericUpDownStartDelay.Value = selData.FrameStart;
            }
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (Loaded)
                onLoad(this, EventArgs.Empty);
        }

        private void onLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Animdata"] = true;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            treeView1.TreeViewNodeSorter = new AnimdataSorter();

            foreach (int id in Animdata.AnimData.Keys)
            {
                Animdata.Data data = (Animdata.Data)Animdata.AnimData[id];
                TreeNode node = new TreeNode();
                node.Tag = id;
                node.Text = String.Format("0x{0:X4} {1}", id, TileData.ItemTable[id].Name);
                if (!Art.IsValidStatic(id))
                    node.ForeColor = Color.Red;
                else if ((TileData.ItemTable[id].Flags & TileFlag.Animation) == 0)
                    node.ForeColor = Color.Blue;
                treeView1.Nodes.Add(node);
                for (int i = 0; i < data.FrameCount; ++i)
                {
                    int frame = id + data.FrameData[i];
                    if (Art.IsValidStatic(frame))
                    {
                        TreeNode subnode = new TreeNode();
                        subnode.Text = String.Format("0x{0:X4} {1}", frame, TileData.ItemTable[frame].Name);
                        node.Nodes.Add(subnode);
                    }
                    else
                        break;
                }
            }
            treeView1.EndUpdate();
            if (treeView1.Nodes.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void AfterNodeSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    curframe = -1;
                    CurrAnim = (int)treeView1.SelectedNode.Tag;
                }
                else
                {
                    curframe = treeView1.SelectedNode.Index;
                    CurrAnim = (int)treeView1.SelectedNode.Parent.Tag;
                }
            }
        }

        private void AfterSelectTreeViewFrames(object sender, TreeViewEventArgs e)
        {
            curframe = treeViewFrames.SelectedNode.Index;
            if (m_Timer != null)
                StopTimer();

            CurrAnim = CurrAnim;
        }

        private void onClickStartStop(object sender, EventArgs e)
        {
            if (m_Timer != null)
                StopTimer();
            else
            {
                m_Timer = new Timer();
                m_Timer.Interval = 100 * selData.FrameInterval + 1;
                m_Timer.Tick += new EventHandler(m_Timer_Tick);
                Timer_frame = 0;
                m_Timer.Start();
            }
        }

        void m_Timer_Tick(object sender, EventArgs e)
        {
            ++Timer_frame;
            if (Timer_frame >= selData.FrameCount)
                Timer_frame = 0;
            pictureBox1.Image = Art.GetStatic(CurrAnim + selData.FrameData[Timer_frame]);
        }

        private void onValueChangedStartDelay(object sender, EventArgs e)
        {
            if (selData != null)
            {
                selData.FrameStart = (byte)numericUpDownStartDelay.Value;
                Options.ChangedUltimaClass["Animdata"] = true;
            }
        }

        private void onValueChangedFrameDelay(object sender, EventArgs e)
        {
            if (selData != null)
            {
                selData.FrameInterval = (byte)numericUpDownFrameDelay.Value;
                if (m_Timer != null)
                    m_Timer.Interval = 100 * selData.FrameInterval + 1;
                Options.ChangedUltimaClass["Animdata"] = true;
            }
        }

        private void onClickFrameDown(object sender, EventArgs e)
        {
            if (selData != null)
            {
                if (treeViewFrames.Nodes.Count > 1)
                {
                    if (treeViewFrames.SelectedNode != null)
                    {
                        int index = treeViewFrames.SelectedNode.Index;
                        if (index < selData.FrameCount - 1)
                        {
                            sbyte temp = selData.FrameData[index];
                            selData.FrameData[index] = selData.FrameData[index + 1];
                            selData.FrameData[index + 1] = temp;

                            TreeNode listnode;
                            if (treeView1.SelectedNode.Parent == null)
                                listnode = treeView1.SelectedNode;
                            else
                                listnode = treeView1.SelectedNode.Parent;

                            int frame = CurrAnim + selData.FrameData[index];
                            treeViewFrames.Nodes[index].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);
                            listnode.Nodes[index].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);

                            frame = CurrAnim + selData.FrameData[index + 1];
                            treeViewFrames.Nodes[index + 1].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);
                            listnode.Nodes[index + 1].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);

                            treeViewFrames.SelectedNode = treeViewFrames.Nodes[index + 1];
                            Options.ChangedUltimaClass["Animdata"] = true;
                        }
                    }
                }
            }
        }

        private void onClickFrameUp(object sender, EventArgs e)
        {
            if (selData != null)
            {
                if (treeViewFrames.Nodes.Count > 1)
                {
                    if (treeViewFrames.SelectedNode != null)
                    {
                        int index = treeViewFrames.SelectedNode.Index;
                        if (index > 0)
                        {
                            sbyte temp = selData.FrameData[index];
                            selData.FrameData[index] = selData.FrameData[index - 1];
                            selData.FrameData[index - 1] = temp;

                            TreeNode listnode;
                            if (treeView1.SelectedNode.Parent == null)
                                listnode = treeView1.SelectedNode;
                            else
                                listnode = treeView1.SelectedNode.Parent;
                            int frame = CurrAnim + selData.FrameData[index];
                            treeViewFrames.Nodes[index].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);
                            listnode.Nodes[index].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);

                            frame = CurrAnim + selData.FrameData[index - 1];
                            treeViewFrames.Nodes[index - 1].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);
                            listnode.Nodes[index - 1].Text = String.Format("0x{0:X4} {1}",
                                frame,
                                TileData.ItemTable[frame].Name);
                            treeViewFrames.SelectedNode = treeViewFrames.Nodes[index - 1];
                            Options.ChangedUltimaClass["Animdata"] = true;
                        }
                    }
                }
            }
        }

        private void onTextChanged(object sender, EventArgs e)
        {
            int index;
            bool candone = Utils.ConvertStringToInt(textBoxAddFrame.Text, out index);
            if (checkBoxRelative.Checked)
                index += CurrAnim;
            if ((index > Art.GetMaxItemID()) || (index < 0))
                candone = false;
            if (candone)
            {
                if (!Art.IsValidStatic(index))
                    textBoxAddFrame.ForeColor = Color.Red;
                else
                    textBoxAddFrame.ForeColor = Color.Black;
            }
            else
                textBoxAddFrame.ForeColor = Color.Red;
        }

        private void onCheckChange(object sender, EventArgs e)
        {
            onTextChanged(this, EventArgs.Empty);
        }

        private void onClickAdd(object sender, EventArgs e)
        {
            if (selData != null)
            {
                int index;
                bool candone = Utils.ConvertStringToInt(textBoxAddFrame.Text, out index);
                if (checkBoxRelative.Checked)
                    index += CurrAnim;
                if ((index > Art.GetMaxItemID()) || (index < 0))
                    candone = false;
                if (candone)
                {
                    if (Art.IsValidStatic(index))
                    {
                        selData.FrameData[selData.FrameCount] = (sbyte)(index - CurrAnim);
                        selData.FrameCount++;
                        TreeNode node = new TreeNode();
                        node.Text = String.Format("0x{0:X4} {1}", index, TileData.ItemTable[index].Name);
                        treeViewFrames.Nodes.Add(node);
                        TreeNode subnode = new TreeNode();
                        subnode.Tag = selData.FrameCount - 1;
                        subnode.Text = String.Format("0x{0:X4} {1}", index, TileData.ItemTable[index].Name);
                        if (treeView1.SelectedNode.Parent == null)
                            treeView1.SelectedNode.Nodes.Add(subnode);
                        else
                            treeView1.SelectedNode.Parent.Nodes.Add(subnode);
                        Options.ChangedUltimaClass["Animdata"] = true;
                    }
                }
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (selData != null)
            {
                if (treeViewFrames.SelectedNode != null)
                {
                    int index = treeViewFrames.SelectedNode.Index;
                    int i;
                    for (i = index; i < selData.FrameCount - 1; ++i)
                    {
                        selData.FrameData[i] = selData.FrameData[i + 1];
                    }
                    for (; i < selData.FrameData.Length; ++i)
                        selData.FrameData[i] = 0;

                    selData.FrameCount--;
                    treeView1.BeginUpdate();
                    treeViewFrames.BeginUpdate();
                    treeViewFrames.Nodes.Clear();
                    TreeNode node;
                    if (treeView1.SelectedNode.Parent == null)
                        node = treeView1.SelectedNode;
                    else
                        node = treeView1.SelectedNode.Parent;
                    node.Nodes.Clear();
                    for (i = 0; i < selData.FrameCount; ++i)
                    {
                        int frame = CurrAnim + selData.FrameData[i];
                        if (Art.IsValidStatic(frame))
                        {
                            TreeNode subnode = new TreeNode();
                            subnode.Text = String.Format("0x{0:X4} {1}", frame, TileData.ItemTable[frame].Name);
                            node.Nodes.Add(subnode);
                            treeViewFrames.Nodes.Add((TreeNode)subnode.Clone());
                        }
                        else
                            break;
                    }
                    treeViewFrames.EndUpdate();
                    treeView1.EndUpdate();
                    Options.ChangedUltimaClass["Animdata"] = true;
                }
            }
        }

        private void onClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Animdata.Save(FiddlerControls.Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                "Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Animdata"] = false;
        }

        private void onClickRemoveAnim(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;

            Animdata.AnimData.Remove(CurrAnim);
            Options.ChangedUltimaClass["Animdata"] = true;
            treeView1.SelectedNode.Remove();
        }

        private void onClickNode(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                treeView1.SelectedNode = e.Node;
        }

        private void onTextChangeAdd(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(AddTextBox.Text, out index, 0, Art.GetMaxItemID()))
            {
                if (Animdata.GetAnimData(index) != null)
                    AddTextBox.ForeColor = Color.Red;
                else
                    AddTextBox.ForeColor = Color.Black;
            }
            else
                AddTextBox.ForeColor = Color.Red;
        }

        private void onKeyDownAdd(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(AddTextBox.Text, out index, 0, Art.GetMaxItemID()))
                {
                    if (Animdata.GetAnimData(index) == null)
                    {
                        Animdata.AnimData[index] = new Animdata.Data(new sbyte[64], 0, 1, 0, 0);
                        TreeNode node = new TreeNode();
                        node.Tag = index;
                        node.Text = String.Format("0x{0:X4} {1}", index, TileData.ItemTable[index].Name);
                        if ((TileData.ItemTable[index].Flags & TileFlag.Animation) == 0)
                            node.ForeColor = Color.Blue;
                        treeView1.Nodes.Add(node);
                        TreeNode subnode = new TreeNode();
                        subnode.Text = String.Format("0x{0:X4} {1}", index, TileData.ItemTable[index].Name);
                        node.Nodes.Add(subnode);
                        node.EnsureVisible();
                        treeView1.SelectedNode = node;
                        Options.ChangedUltimaClass["Animdata"] = true;
                    }
                }
            }
        }

        private void StopTimer()
        {
            if (m_Timer.Enabled)
                m_Timer.Stop();

            m_Timer.Dispose();
            m_Timer = null;
            Timer_frame = 0;
        }
    }

    public class AnimdataSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent != null)
                return 0;
            int ix = (int)tx.Tag;
            int iy = (int)ty.Tag;
            if (ix == iy)
                return 0;
            else if (ix < iy)
                return -1;
            else
                return 1;
        }
    }
}
