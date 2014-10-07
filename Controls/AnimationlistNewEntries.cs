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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class AnimationlistNewEntries : Form
    {
        private Animationlist Form;
        public AnimationlistNewEntries(Animationlist form)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            Form = form;
        }

        private int m_CurrentSelect;
        private int m_CurrentSelectAction;
        private int facing = 1;
        private bool animate = false;
        private Timer m_Timer = null;
        private int m_FrameIndex;
        private Frame[] m_Animation;

        public bool Animate
        {
            get { return animate; }
            set
            {
                if (animate != value)
                {
                    animate = value;
                    StopAnimation();
                    if (animate)
                        SetAnimation();
                }
            }
        }

        public int CurrentSelectAction
        {
            get { return m_CurrentSelectAction; }
            set { m_CurrentSelectAction = value; }
        }

        public int CurrentSelect
        {
            get { return m_CurrentSelect; }
            set
            {
                StopAnimation();
                m_CurrentSelect = value;
                if (animate)
                    SetAnimation();
                pictureBox1.Invalidate();
            }
        }

        private void StopAnimation()
        {
            if (m_Timer != null)
            {
                if (m_Timer.Enabled)
                    m_Timer.Stop();

                m_Timer.Dispose();
                m_Timer = null;
            }

            m_Animation = null;
            m_FrameIndex = 0;
        }

        private void SetAnimation()
        {
            int body = m_CurrentSelect;
            Animations.Translate(ref body);
            int hue = 0;
            m_Animation = Animations.GetAnimation(m_CurrentSelect, m_CurrentSelectAction, facing, ref hue, false, false);
            if (m_Animation != null)
            {
                m_FrameIndex = 0;
                m_Timer = new Timer();
                m_Timer.Interval = 1000 / m_Animation.Length;
                m_Timer.Tick += new EventHandler(AnimTick);
                m_Timer.Start();
            }
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++m_FrameIndex;

            if (m_FrameIndex == m_Animation.Length)
                m_FrameIndex = 0;

            pictureBox1.Invalidate();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            facingbar.Value = (facing + 3) & 7;
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.TreeViewNodeSorter = new AnimNewListGraphicSorter();

            MobTypes();

            TreeNode node;
            foreach (DictionaryEntry key in BodyTable.m_Entries) //body.def
            {
                BodyTableEntry entry = (BodyTableEntry)key.Value;
                if (!AlreadyFound(entry.NewID))
                {
                    if (!Form.IsAlreadyDefinied(entry.NewID))
                    {
                        node = new TreeNode(entry.NewID.ToString());
                        node.Tag = entry.NewID;
                        node.ToolTipText = String.Format("Found in body.def {0}", Animations.GetFileName(entry.NewID));
                        node.Tag = new int[] { entry.NewID, 0 };
                        treeView1.Nodes.Add(node);
                        SetActionType(node, entry.NewID, 0);
                    }
                }
            }
            if (BodyConverter.Table1 != null)
            {
                foreach (int entry in BodyConverter.Table1)  //bodyconv.def
                {
                    if (entry != -1)
                    {
                        if (!AlreadyFound(entry))
                        {
                            if (!Form.IsAlreadyDefinied(entry))
                            {
                                node = new TreeNode(entry.ToString());
                                node.ToolTipText = String.Format("Found in bodyconv.def {0}", Animations.GetFileName(entry));
                                node.Tag = entry;
                                node.Tag = new int[] { entry, 0 };
                                treeView1.Nodes.Add(node);
                                SetActionType(node, entry, 0);
                            }
                        }
                    }
                }
            }
            if (BodyConverter.Table2 != null)
            {
                foreach (int entry in BodyConverter.Table2)
                {
                    if (entry != -1)
                    {
                        if (!AlreadyFound(entry))
                        {
                            if (!Form.IsAlreadyDefinied(entry))
                            {
                                node = new TreeNode(entry.ToString());
                                node.ToolTipText = String.Format("Found in bodyconv.def {0}", Animations.GetFileName(entry));
                                node.Tag = entry;
                                node.Tag = new int[] { entry, 0 };
                                treeView1.Nodes.Add(node);
                                SetActionType(node, entry, 0);
                            }
                        }
                    }
                }
            }
            if (BodyConverter.Table3 != null)
            {
                foreach (int entry in BodyConverter.Table3)
                {
                    if (entry != -1)
                    {
                        if (!AlreadyFound(entry))
                        {
                            if (!Form.IsAlreadyDefinied(entry))
                            {
                                node = new TreeNode(entry.ToString());
                                node.ToolTipText = String.Format("Found in bodyconv.def {0}", Animations.GetFileName(entry));
                                node.Tag = entry;
                                node.Tag = new int[] { entry, 0 };
                                treeView1.Nodes.Add(node);
                                SetActionType(node, entry, 0);
                            }
                        }
                    }
                }
            }
            if (BodyConverter.Table4 != null)
            {
                foreach (int entry in BodyConverter.Table4)
                {
                    if (entry != -1)
                    {
                        if (!AlreadyFound(entry))
                        {
                            if (!Form.IsAlreadyDefinied(entry))
                            {
                                node = new TreeNode(entry.ToString());
                                node.ToolTipText = String.Format("Found in bodyconv.def {0}", Animations.GetFileName(entry));
                                node.Tag = entry;
                                node.Tag = new int[] { entry, 0 };
                                treeView1.Nodes.Add(node);
                                SetActionType(node, entry, 0);
                            }
                        }
                    }
                }
            }
            treeView1.EndUpdate();
        }

        private bool AlreadyFound(int graphic)
        {
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (((int[])node.Tag)[0] == graphic)
                    return true;
            }
            return false;
        }

        private void MobTypes()
        {
            string filePath = Files.GetFilePath("mobtypes.txt");

            if (filePath == null)
                return;
            TreeNode node;
            using (StreamReader def = new StreamReader(filePath))
            {
                string line;
                int graphic;

                while ((line = def.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        continue;
                    try
                    {
                        string[] split = line.Split('\t');
                        if (int.TryParse(split[0], out graphic))
                        {
                            if (!AlreadyFound(graphic))
                            {
                                if (!Form.IsAlreadyDefinied(graphic))
                                {
                                    node = new TreeNode(graphic.ToString());

                                    node.ToolTipText = String.Format("Found in mobtype.txt {0}", Animations.GetFileName(graphic));
                                    int type = 0;
                                    switch (split[1])
                                    {
                                        case "MONSTER": type = 0; break;
                                        case "SEA_MONSTER": type = 1; break;
                                        case "ANIMAL": type = 2; break;
                                        case "HUMAN": type = 3; break;
                                        case "EQUIPMENT": type = 3; break;
                                    }
                                    node.Tag = new int[] { graphic, type };
                                    treeView1.Nodes.Add(node);
                                    SetActionType(node, graphic, type);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private void SetActionType(TreeNode parent, int graphic, int type)
        {
            parent.Nodes.Clear();
            TreeNode node;
            if (type == 4) //Equipment==human
                type = 3;
            for (int i = 0; i < Form.GetAnimNames[type].GetLength(0); ++i)
            {
                if (Animations.IsActionDefined(graphic, i, 0))
                {
                    node = new TreeNode(String.Format("{0} {1}", i, Form.GetAnimNames[type][i]));
                    node.Tag = i;
                    parent.Nodes.Add(node);
                }
            }
        }

        private void onAfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent == null)
            {
                m_CurrentSelectAction = 0;
                CurrentSelect = ((int[])e.Node.Tag)[0];
                ComboBoxActionType.SelectedIndex = ((int[])e.Node.Tag)[1];
            }
            else
            {
                m_CurrentSelectAction = (int)e.Node.Tag;
                CurrentSelect = ((int[])e.Node.Parent.Tag)[0];
                ComboBoxActionType.SelectedIndex = ((int[])e.Node.Parent.Tag)[1];
            }
        }

        private void OnPaint_PictureBox(object sender, PaintEventArgs e)
        {
            if (CurrentSelect != 0)
            {
                if (animate)
                {
                    if (m_Animation != null)
                    {
                        if (m_Animation[m_FrameIndex].Bitmap != null)
                        {
                            Point loc = new Point();
                            loc.X = (pictureBox1.Width - m_Animation[m_FrameIndex].Bitmap.Width) / 2;
                            loc.Y = (pictureBox1.Height - m_Animation[m_FrameIndex].Bitmap.Height) / 2;
                            e.Graphics.DrawImage(m_Animation[m_FrameIndex].Bitmap, loc);
                        }
                    }
                }
                else
                {
                    int body = m_CurrentSelect;
                    Animations.Translate(ref body);
                    int hue = 0;
                    Frame[] frames = Animations.GetAnimation(m_CurrentSelect, m_CurrentSelectAction, facing, ref hue, false, false);
                    if (frames != null)
                    {
                        if (frames[0].Bitmap != null)
                        {
                            Point loc = new Point();
                            loc.X = (pictureBox1.Width - frames[0].Bitmap.Width) / 2;
                            loc.Y = (pictureBox1.Height - frames[0].Bitmap.Height) / 2;
                            e.Graphics.DrawImage(frames[0].Bitmap, loc);
                        }
                    }
                }
            }
        }

        private void onClickAnimate(object sender, EventArgs e)
        {
            Animate = !Animate;
        }

        private void OnChangeType(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                TreeNode node = treeView1.SelectedNode;
                if (node.Parent != null)
                    node = node.Parent;
                ((int[])node.Tag)[1] = ComboBoxActionType.SelectedIndex;
                SetActionType(node, ((int[])node.Tag)[0], ComboBoxActionType.SelectedIndex);
            }
        }

        private void onScrollFacing(object sender, EventArgs e)
        {
            facing = (facingbar.Value - 3) & 7;
            CurrentSelect = CurrentSelect;
        }

        private void onClickAdd(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                TreeNode node = treeView1.SelectedNode;
                if (node.Parent != null)
                    node = node.Parent;
                Form.AddGraphic(((int[])node.Tag)[0], ((int[])node.Tag)[1], node.Text);
                treeView1.SelectedNode.Remove();
            }
        }
    }

    public class AnimNewListGraphicSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent != null)
                return 0;
            if (ty.Parent != null)
                return 0;
            int[] ix = (int[])tx.Tag;
            int[] iy = (int[])ty.Tag;
            if (ix[0] == iy[0])
                return 0;
            else if (ix[0] < iy[0])
                return -1;
            else
                return 1;
        }
    }
}
