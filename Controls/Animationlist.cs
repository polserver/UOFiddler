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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;

namespace FiddlerControls
{
    public partial class Animationlist : UserControl
    {
        public Animationlist()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        #region AnimNames
        private string[][] AnimNames = {
            new string[]{"Walk","Idle","Die1","Die2","Attack1","Attack2","Attack3","AttackBow","AttackCrossBow",
                         "AttackThrow","GetHit","Pillage","Stomp","Cast2","Cast3","BlockRight","BlockLeft","Idle",
                         "Fidget","Fly","TakeOff","GetHitInAir"}, //Monster
            new string[]{"Walk","Run","Idle","Idle","Fidget","Attack1","Attack2","GetHit","Die1"},//sea
            new string[]{"Walk","Run","Idle","Eat","Alert","Attack1","Attack2","GetHit","Die1","Idle","Fidget",
                         "LieDown","Die2"},//animal
            new string[]{"Walk_01","WalkStaff_01","Run_01","RunStaff_01","Idle_01","Idle_01",
                         "Fidget_Yawn_Stretch_01","CombatIdle1H_01","CombatIdle1H_01","AttackSlash1H_01",
                         "AttackPierce1H_01","AttackBash1H_01","AttackBash2H_01","AttackSlash2H_01",
                         "AttackPierce2H_01","CombatAdvance_1H_01","Spell1","Spell2","AttackBow_01",
                         "AttackCrossbow_01","GetHit_Fr_Hi_01","Die_Hard_Fwd_01","Die_Hard_Back_01",
                         "Horse_Walk_01","Horse_Run_01","Horse_Idle_01","Horse_Attack1H_SlashRight_01",
                         "Horse_AttackBow_01","Horse_AttackCrossbow_01","Horse_Attack2H_SlashRight_01",
                         "Block_Shield_Hard_01","Punch_Punch_Jab_01","Bow_Lesser_01","Salute_Armed1h_01",
                         "Ingest_Eat_01"}//human
        };
        public string[][] GetAnimNames { get { return AnimNames; } }
        #endregion

        private Bitmap m_MainPicture;
        private int m_CurrentSelect = 0;
        private int m_CurrentSelectAction = 0;
        private bool m_Animate = false;
        private int m_FrameIndex;
        private Bitmap[] m_Animation;
        private bool m_ImageInvalidated = true;
        private Timer m_Timer = null;
        private Frame[] frames;
        private int customHue = 0;
        private int DefHue = 0;
        private int facing = 1;
        private bool sortalpha = false;
        private int DisplayType = 0;
        private bool Loaded = false;


        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            m_MainPicture = null;
            m_CurrentSelect = 0;
            m_CurrentSelectAction = 0;
            m_Animate = false;
            m_ImageInvalidated = true;
            StopAnimation();
            frames = null;
            customHue = 0;
            DefHue = 0;
            facing = 1;
            sortalpha = false;
            DisplayType = 0;
            OnLoad(this, EventArgs.Empty);
        }
        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Animations"] = true;
            Options.LoadedUltimaClass["Hues"] = true;
            TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
            if (!LoadXml())
            {
                Cursor.Current = Cursors.Default;
                return;
            }

            LoadListView();

            extractAnimationToolStripMenuItem.Visible = false;
            m_CurrentSelect = 0;
            m_CurrentSelectAction = 0;
            if (TreeViewMobs.Nodes[0].Nodes.Count > 0)
                TreeViewMobs.SelectedNode = TreeViewMobs.Nodes[0].Nodes[0];
            FacingBar.Value = (facing + 3) & 7;
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        /// <summary>
        /// Changes Hue of current Mob
        /// </summary>
        /// <param name="select"></param>
        public void ChangeHue(int select)
        {
            customHue = select + 1;
            CurrentSelect = CurrentSelect;
        }

        /// <summary>
        /// Is Graphic already in TreeView
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public bool IsAlreadyDefinied(int graphic)
        {
            foreach (TreeNode node in TreeViewMobs.Nodes[0].Nodes)
            {
                if (((int[])node.Tag)[0] == graphic)
                    return true;
            }
            foreach (TreeNode node in TreeViewMobs.Nodes[1].Nodes)
            {
                if (((int[])node.Tag)[0] == graphic)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Adds Graphic with type and name to List
        /// </summary>
        /// <param name="graphic"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public void AddGraphic(int graphic, int type, string name)
        {
            TreeViewMobs.BeginUpdate();
            TreeViewMobs.TreeViewNodeSorter = null;
            TreeNode nodeparent = new TreeNode(name);
            nodeparent.Tag = new int[] { graphic, type };
            nodeparent.ToolTipText = Animations.GetFileName(graphic);
            if (type == 4)
            {
                TreeViewMobs.Nodes[1].Nodes.Add(nodeparent);
                type = 3;
            }
            else
                TreeViewMobs.Nodes[0].Nodes.Add(nodeparent);


            TreeNode node;
            for (int i = 0; i < AnimNames[type].GetLength(0); ++i)
            {
                if (Animations.IsActionDefined(graphic, i, 0))
                {
                    node = new TreeNode(i.ToString() + " " + AnimNames[type][i]);
                    node.Tag = i;
                    nodeparent.Nodes.Add(node);
                }
            }
            if (!sortalpha)
                TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
            else
                TreeViewMobs.TreeViewNodeSorter = new AlphaSorter();
            TreeViewMobs.Sort();
            TreeViewMobs.EndUpdate();
            LoadListView();
            TreeViewMobs.SelectedNode = nodeparent;
            nodeparent.EnsureVisible();
        }

        private bool Animate
        {
            get { return m_Animate; }
            set
            {
                if (m_Animate != value)
                {
                    m_Animate = value;
                    extractAnimationToolStripMenuItem.Visible = !m_Animate ? false : true;
                    StopAnimation();
                    m_ImageInvalidated = true;
                    MainPictureBox.Invalidate();
                }
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

            if (m_Animation != null)
            {
                for (int i = 0; i < m_Animation.Length; ++i)
                {
                    if (m_Animation[i] != null)
                        m_Animation[i].Dispose();
                }
            }

            m_Animation = null;
            m_FrameIndex = 0;
        }

        private int CurrentSelect
        {
            get { return m_CurrentSelect; }
            set
            {
                m_CurrentSelect = value;
                if (m_Timer != null)
                {
                    if (m_Timer.Enabled)
                        m_Timer.Stop();

                    m_Timer.Dispose();
                    m_Timer = null;
                }
                SetPicture();
                MainPictureBox.Invalidate();
            }
        }

        private void SetPicture()
        {
            frames = null;
            if (m_MainPicture != null)
                m_MainPicture.Dispose();
            if (m_CurrentSelect != 0)
            {
                if (Animate)
                    m_MainPicture = DoAnimation();
                else
                {
                    int body = m_CurrentSelect;
                    Animations.Translate(ref body);
                    int hue = customHue;
                    if (hue != 0)
                        frames = Animations.GetAnimation(m_CurrentSelect, m_CurrentSelectAction, facing, ref hue, true, false);
                    else
                    {
                        frames = Animations.GetAnimation(m_CurrentSelect, m_CurrentSelectAction, facing, ref hue, false, false);
                        DefHue = hue;
                    }

                    if (frames != null)
                    {
                        if (frames[0].Bitmap != null)
                        {
                            m_MainPicture = new Bitmap(frames[0].Bitmap);
                            BaseGraphicLabel.Text = "BaseGraphic: " + body.ToString();
                            GraphicLabel.Text = "Graphic: " + m_CurrentSelect.ToString() + String.Format("(0x{0:X})", m_CurrentSelect);
                            HueLabel.Text = "Hue: " + (hue + 1).ToString();
                        }
                        else
                            m_MainPicture = null;
                    }
                    else
                        m_MainPicture = null;
                }
            }
        }

        private Bitmap DoAnimation()
        {
            if (m_Timer == null)
            {
                int body = m_CurrentSelect;
                Animations.Translate(ref body);
                int hue = customHue;
                if (hue != 0)
                    frames = Animations.GetAnimation(m_CurrentSelect, m_CurrentSelectAction, facing, ref hue, true, false);
                else
                {
                    frames = Animations.GetAnimation(m_CurrentSelect, m_CurrentSelectAction, facing, ref hue, false, false);
                    DefHue = hue;
                }

                if (frames == null)
                    return null;
                BaseGraphicLabel.Text = "BaseGraphic: " + body.ToString();
                GraphicLabel.Text = "Graphic: " + m_CurrentSelect.ToString() + String.Format("(0x{0:X})", m_CurrentSelect);
                HueLabel.Text = "Hue: " + (hue + 1).ToString();
                int count = frames.Length;
                m_Animation = new Bitmap[count];

                for (int i = 0; i < count; ++i)
                {
                    m_Animation[i] = frames[i].Bitmap;
                }

                m_Timer = new Timer();
                m_Timer.Interval = 1000 / count;
                m_Timer.Tick += new EventHandler(AnimTick);
                m_Timer.Start();

                m_FrameIndex = 0;
                LoadListViewFrames(); // FrameTab neuladen
                if (m_Animation[0] != null)
                    return new Bitmap(m_Animation[0]);
                else
                    return null;
            }
            else
            {
                if (m_Animation[m_FrameIndex] != null)
                    return new Bitmap(m_Animation[m_FrameIndex]);
                else
                    return null;
            }
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++m_FrameIndex;

            if (m_FrameIndex == m_Animation.Length)
                m_FrameIndex = 0;

            m_ImageInvalidated = true;
            MainPictureBox.Invalidate();
        }

        private void OnPaint_MainPicture(object sender, PaintEventArgs e)
        {
            if (m_ImageInvalidated)
                SetPicture();
            if (m_MainPicture != null)
            {
                System.Drawing.Point location = System.Drawing.Point.Empty;
                Size size = Size.Empty;
                size = m_MainPicture.Size;
                location.X = (MainPictureBox.Width - m_MainPicture.Width) / 2;
                location.Y = (MainPictureBox.Height - m_MainPicture.Height) / 2;

                Rectangle destRect = new Rectangle(location, size);

                e.Graphics.DrawImage(m_MainPicture, destRect, 0, 0, m_MainPicture.Width, m_MainPicture.Height, System.Drawing.GraphicsUnit.Pixel);
            }
            else
                m_MainPicture = null;
        }

        private void TreeViewMobs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                if ((e.Node.Parent.Name == "Mobs") || (e.Node.Parent.Name == "Equipment"))
                {
                    m_CurrentSelectAction = 0;
                    CurrentSelect = ((int[])e.Node.Tag)[0];
                    if ((e.Node.Parent.Name == "Mobs") && (DisplayType == 1))
                    {
                        DisplayType = 0;
                        LoadListView();
                    }
                    else if ((e.Node.Parent.Name == "Equipment") && (DisplayType == 0))
                    {
                        DisplayType = 1;
                        LoadListView();
                    }
                }
                else
                {
                    m_CurrentSelectAction = (int)e.Node.Tag;
                    CurrentSelect = ((int[])e.Node.Parent.Tag)[0];
                    if ((e.Node.Parent.Parent.Name == "Mobs") && (DisplayType == 1))
                    {
                        DisplayType = 0;
                        LoadListView();
                    }
                    else if ((e.Node.Parent.Parent.Name == "Equipment") && (DisplayType == 0))
                    {
                        DisplayType = 1;
                        LoadListView();
                    }
                }
            }
            else
            {
                if ((e.Node.Name == "Mobs") && (DisplayType == 1))
                {
                    DisplayType = 0;
                    LoadListView();
                }
                else if ((e.Node.Name == "Equipment") && (DisplayType == 0))
                {
                    DisplayType = 1;
                    LoadListView();
                }
                TreeViewMobs.SelectedNode = e.Node.Nodes[0];
            }
        }

        private void Animate_Click(object sender, EventArgs e)
        {
            Animate = !Animate;
        }

        private bool LoadXml()
        {
            string path = FiddlerControls.Options.AppDataPath;

            string FileName = Path.Combine(path, "Animationlist.xml");
            if (!(File.Exists(FileName)))
                return false;
            TreeViewMobs.BeginUpdate();
            TreeViewMobs.Nodes.Clear();


            XmlDocument dom = new XmlDocument();
            dom.Load(FileName);
            XmlElement xMobs = dom["Graphics"];
            List<TreeNode> nodes = new List<TreeNode>();
            TreeNode rootnode, node, typenode;
            rootnode = new TreeNode("Mobs");
            rootnode.Name = "Mobs";
            rootnode.Tag = -1;
            nodes.Add(rootnode);

            foreach (XmlElement xMob in xMobs.SelectNodes("Mob"))
            {
                string name;
                int value;
                name = xMob.GetAttribute("name");
                value = int.Parse(xMob.GetAttribute("body"));
                int type = int.Parse(xMob.GetAttribute("type"));
                node = new TreeNode(name);
                node.Tag = new int[] { value, type };
                node.ToolTipText = Animations.GetFileName(value);
                rootnode.Nodes.Add(node);

                for (int i = 0; i < AnimNames[type].GetLength(0); ++i)
                {
                    if (Animations.IsActionDefined(value, i, 0))
                    {
                        typenode = new TreeNode(i.ToString() + " " + AnimNames[type][i]);
                        typenode.Tag = i;
                        node.Nodes.Add(typenode);
                    }
                }
            }
            rootnode = new TreeNode("Equipment");
            rootnode.Name = "Equipment";
            rootnode.Tag = -2;
            nodes.Add(rootnode);

            foreach (XmlElement xMob in xMobs.SelectNodes("Equip"))
            {
                string name;
                int value;
                name = xMob.GetAttribute("name");
                value = int.Parse(xMob.GetAttribute("body"));
                int type = int.Parse(xMob.GetAttribute("type"));
                node = new TreeNode(name);
                node.Tag = new int[] { value, type };
                node.ToolTipText = Animations.GetFileName(value);
                rootnode.Nodes.Add(node);

                for (int i = 0; i < AnimNames[type].GetLength(0); ++i)
                {
                    if (Animations.IsActionDefined(value, i, 0))
                    {
                        typenode = new TreeNode(i.ToString() + " " + AnimNames[type][i]);
                        typenode.Tag = i;
                        node.Nodes.Add(typenode);
                    }
                }
            }
            TreeViewMobs.Nodes.AddRange(nodes.ToArray());
            nodes.Clear();
            TreeViewMobs.EndUpdate();
            return true;
        }

        private void LoadListView()
        {
            listView.BeginUpdate();
            listView.Clear();
            ListViewItem item;
            foreach (TreeNode node in TreeViewMobs.Nodes[DisplayType].Nodes)
            {
                item = new ListViewItem("(" + ((int[])node.Tag)[0] + ")", 0);
                item.Tag = ((int[])node.Tag)[0];
                listView.Items.Add(item);
            }
            listView.EndUpdate();
        }

        private void selectChanged_listView(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
                TreeViewMobs.SelectedNode = TreeViewMobs.Nodes[DisplayType].Nodes[listView.SelectedItems[0].Index];
        }

        private void listView_DoubleClick(object sender, MouseEventArgs e)
        {
            tabControl1.SelectTab(tabPage1);
        }

        private void listViewdrawItem(object sender, DrawListViewItemEventArgs e)
        {
            int graphic = (int)e.Item.Tag;
            int hue = 0;
            frames = Animations.GetAnimation(graphic, 0, 1, ref hue, false, true);

            if (frames == null)
                return;
            Bitmap bmp = frames[0].Bitmap;
            int width = bmp.Width;
            int height = bmp.Height;

            if (width > e.Bounds.Width)
                width = e.Bounds.Width;

            if (height > e.Bounds.Height)
                height = e.Bounds.Height;

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
            if (listView.SelectedItems.Contains(e.Item))
                e.DrawFocusRectangle();
            else
                e.Graphics.DrawRectangle(new Pen(Color.Gray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
        }

        private HuePopUp showform = null;
        private void OnClick_Hue(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                if (customHue == 0)
                    showform = new HuePopUp(this, DefHue + 1);
                else
                    showform = new HuePopUp(this, customHue - 1);
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void LoadListViewFrames()
        {
            listView1.BeginUpdate();
            listView1.Clear();
            ListViewItem item;
            for (int frame = 0; frame < m_Animation.Length; ++frame)
            {
                item = new ListViewItem(frame.ToString(), 0);
                item.Tag = frame;
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();
        }

        private void Frames_ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            Bitmap bmp = m_Animation[(int)e.Item.Tag];
            int width = bmp.Width;
            int height = bmp.Height;

            if (width > e.Bounds.Width)
                width = e.Bounds.Width;

            if (height > e.Bounds.Height)
                height = e.Bounds.Height;

            e.Graphics.DrawImage(bmp, e.Bounds.X, e.Bounds.Y, width, height);
            e.DrawText(TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
            e.Graphics.DrawRectangle(new Pen(Color.Gray), e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);
        }

        private void OnScrollFacing(object sender, EventArgs e)
        {
            facing = (FacingBar.Value - 3) & 7;
            CurrentSelect = CurrentSelect;
        }

        private void OnClick_Sort(object sender, EventArgs e)
        {
            sortalpha = !sortalpha;
            TreeViewMobs.BeginUpdate();
            if (!sortalpha)
                TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
            else
                TreeViewMobs.TreeViewNodeSorter = new AlphaSorter();
            TreeViewMobs.Sort();
            TreeViewMobs.EndUpdate();
            LoadListView();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (TreeViewMobs.SelectedNode != null)
            {
                TreeNode node = TreeViewMobs.SelectedNode;
                if (node.Parent == null)
                    return;
                if ((node.Parent.Name != "Mobs") && (node.Parent.Name != "Equipment"))
                    node = node.Parent;
                node.Remove();
                LoadListView();
            }
        }

        private AnimationEdit animEditEntry;
        private void onClickAnimationEdit(object sender, EventArgs e)
        {
            if ((animEditEntry == null) || (animEditEntry.IsDisposed))
            {
                animEditEntry = new AnimationEdit();
                //animEditEntry.TopMost = true;
                animEditEntry.Show();
            }
        }

        private AnimationlistNewEntries animnewEntry;
        private void OnClickFindNewEntries(object sender, EventArgs e)
        {
            if ((animnewEntry == null) || (animnewEntry.IsDisposed))
            {
                animnewEntry = new AnimationlistNewEntries(this);
                animnewEntry.TopMost = true;
                animnewEntry.Show();
            }
        }

        private void RewriteXML(object sender, EventArgs e)
        {
            TreeViewMobs.BeginUpdate();
            TreeViewMobs.TreeViewNodeSorter = new GraphicSorter();
            TreeViewMobs.Sort();
            TreeViewMobs.EndUpdate();

            string filepath = FiddlerControls.Options.AppDataPath;

            string FileName = Path.Combine(filepath, "Animationlist.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Graphics");
            XmlComment comment = dom.CreateComment("Entries in Mob tab");
            sr.AppendChild(comment);
            comment = dom.CreateComment("Name=Displayed name");
            sr.AppendChild(comment);
            comment = dom.CreateComment("body=Graphic");
            sr.AppendChild(comment);
            comment = dom.CreateComment("type=0:Monster, 1:Sea, 2:Animal, 3:Human/Equipment");
            sr.AppendChild(comment);

            XmlElement elem;
            foreach (TreeNode node in TreeViewMobs.Nodes[0].Nodes)
            {
                elem = dom.CreateElement("Mob");
                elem.SetAttribute("name", node.Text);
                elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());

                sr.AppendChild(elem);
            }
            foreach (TreeNode node in TreeViewMobs.Nodes[1].Nodes)
            {
                elem = dom.CreateElement("Equip");
                elem.SetAttribute("name", node.Text);
                elem.SetAttribute("body", ((int[])node.Tag)[0].ToString());
                elem.SetAttribute("type", ((int[])node.Tag)[1].ToString());
                sr.AppendChild(elem);
            }
            dom.AppendChild(sr);
            dom.Save(FileName);
            MessageBox.Show("XML saved", "Rewrite", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";

            string FileName = Path.Combine(path, String.Format("{0} {1}.bmp", what, m_CurrentSelect));

            if (Animate)
            {
                using (Bitmap newbit = new Bitmap(m_Animation[0].Width, m_Animation[0].Height))
                {
                    Graphics newgraph = Graphics.FromImage(newbit);
                    newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                    newgraph.DrawImage(m_Animation[0], new System.Drawing.Point(0, 0));
                    newgraph.Save();
                    newbit.Save(FileName, ImageFormat.Bmp);
                }
            }
            else
            {
                using (Bitmap newbit = new Bitmap(m_MainPicture.Width, m_MainPicture.Height))
                {
                    Graphics newgraph = Graphics.FromImage(newbit);
                    newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                    newgraph.DrawImage(m_MainPicture, new System.Drawing.Point(0, 0));
                    newgraph.Save();
                    newbit.Save(FileName, ImageFormat.Bmp);
                }
            }
            MessageBox.Show(
                String.Format("{0} saved to {1}", what, FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";

            string FileName = Path.Combine(path, String.Format("{0} {1}.tiff", what, m_CurrentSelect));

            if (Animate)
            {
                using (Bitmap newbit = new Bitmap(m_Animation[0].Width, m_Animation[0].Height))
                {
                    Graphics newgraph = Graphics.FromImage(newbit);
                    newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                    newgraph.DrawImage(m_Animation[0], new System.Drawing.Point(0, 0));
                    newgraph.Save();
                    newbit.Save(FileName, ImageFormat.Tiff);
                }
            }
            else
            {
                using (Bitmap newbit = new Bitmap(m_MainPicture.Width, m_MainPicture.Height))
                {
                    Graphics newgraph = Graphics.FromImage(newbit);
                    newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                    newgraph.DrawImage(m_MainPicture, new System.Drawing.Point(0, 0));
                    newgraph.Save();
                    newbit.Save(FileName, ImageFormat.Tiff);
                }
            }
            MessageBox.Show(
                String.Format("{0} saved to {1}", what, FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void extract_Image_ClickJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";

            string FileName = Path.Combine(path, String.Format("{0} {1}.jpg", what, m_CurrentSelect));

            if (Animate)
            {
                using (Bitmap newbit = new Bitmap(m_Animation[0].Width, m_Animation[0].Height))
                {
                    Graphics newgraph = Graphics.FromImage(newbit);
                    newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                    newgraph.DrawImage(m_Animation[0], new System.Drawing.Point(0, 0));
                    newgraph.Save();
                    newbit.Save(FileName, ImageFormat.Jpeg);
                }
            }
            else
            {
                using (Bitmap newbit = new Bitmap(m_MainPicture.Width, m_MainPicture.Height))
                {
                    Graphics newgraph = Graphics.FromImage(newbit);
                    newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                    newgraph.DrawImage(m_MainPicture, new System.Drawing.Point(0, 0));
                    newgraph.Save();
                    newbit.Save(FileName, ImageFormat.Tiff);
                }
            }
            MessageBox.Show(
                String.Format("{0} saved to {1}", what, FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExtractAnimBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";

            string FileName = Path.Combine(path, String.Format("{0} {1}", what, m_CurrentSelect));
            if (Animate)
            {
                for (int i = 0; i < m_Animation.Length; ++i)
                {
                    Bitmap newbit = new Bitmap(m_Animation[i].Width, m_Animation[i].Height);
                    Graphics newgraph = Graphics.FromImage(newbit);
                    newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                    newgraph.DrawImage(m_Animation[i], new System.Drawing.Point(0, 0));
                    newgraph.Save();
                    newbit.Save(String.Format("{0}-{1}.bmp", FileName, i), ImageFormat.Bmp);
                }
                MessageBox.Show(
                    String.Format("{0} saved to '{1}-X.bmp'", what, FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExtractAnimTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";

            string FileName = Path.Combine(path, String.Format("{0} {1}", what, m_CurrentSelect));
            if (Animate)
            {
                for (int i = 0; i < m_Animation.Length; ++i)
                {
                    using (Bitmap newbit = new Bitmap(m_Animation[i].Width, m_Animation[i].Height))
                    {
                        Graphics newgraph = Graphics.FromImage(newbit);
                        newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                        newgraph.DrawImage(m_Animation[i], new System.Drawing.Point(0, 0));
                        newgraph.Save();
                        newbit.Save(String.Format("{0}-{1}.tiff", FileName, i), ImageFormat.Tiff);
                    }
                }
                MessageBox.Show(
                    String.Format("{0} saved to '{1}-X.tiff'", what, FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExtractAnimJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";

            string FileName = Path.Combine(path, String.Format("{0} {1}", what, m_CurrentSelect));
            if (Animate)
            {
                for (int i = 0; i < m_Animation.Length; ++i)
                {
                    using (Bitmap newbit = new Bitmap(m_Animation[i].Width, m_Animation[i].Height))
                    {
                        Graphics newgraph = Graphics.FromImage(newbit);
                        newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                        newgraph.DrawImage(m_Animation[i], new System.Drawing.Point(0, 0));
                        newgraph.Save();
                        newbit.Save(String.Format("{0}-{1}.jpg", FileName, i), ImageFormat.Jpeg);
                    }
                }
                MessageBox.Show(
                    String.Format("{0} saved to '{1}-X.jpg'", what, FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExportFrameBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";
            if (listView1.SelectedItems.Count < 1)
                return;

            string FileName = Path.Combine(path, String.Format("{0} {1}", what, m_CurrentSelect));

            Bitmap bit = m_Animation[(int)listView1.SelectedItems[0].Tag];
            using (Bitmap newbit = new Bitmap(bit.Width, bit.Height))
            {
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                newgraph.DrawImage(bit, new System.Drawing.Point(0, 0));
                newgraph.Save();
                newbit.Save(String.Format("{0}-{1}.bmp", FileName, (int)listView1.SelectedItems[0].Tag), ImageFormat.Bmp);
            }
        }

        private void OnClickExportFrameTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";
            if (listView1.SelectedItems.Count < 1)
                return;

            string FileName = Path.Combine(path, String.Format("{0} {1}", what, m_CurrentSelect));

            Bitmap bit = m_Animation[(int)listView1.SelectedItems[0].Tag];
            using (Bitmap newbit = new Bitmap(bit.Width, bit.Height))
            {
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                newgraph.DrawImage(bit, new System.Drawing.Point(0, 0));
                newgraph.Save();
                newbit.Save(String.Format("{0}-{1}.tiff", FileName, (int)listView1.SelectedItems[0].Tag), ImageFormat.Tiff);
            }
        }

        private void OnClickExportFrameJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string what = "Mob";
            if (DisplayType == 1)
                what = "Equipment";
            if (listView1.SelectedItems.Count < 1)
                return;

            string FileName = Path.Combine(path, String.Format("{0} {1}", what, m_CurrentSelect));

            Bitmap bit = m_Animation[(int)listView1.SelectedItems[0].Tag];
            using (Bitmap newbit = new Bitmap(bit.Width, bit.Height))
            {
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.FillRectangle(Brushes.White, 0, 0, newbit.Width, newbit.Height);
                newgraph.DrawImage(bit, new System.Drawing.Point(0, 0));
                newgraph.Save();
                newbit.Save(String.Format("{0}-{1}.jpg", FileName, (int)listView1.SelectedItems[0].Tag), ImageFormat.Jpeg);
            }
        }
    }

    public class AlphaSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null)  // dont change Mob and Equipment
            {
                if ((int)tx.Tag == -1) //mob
                    return -1;
                else
                    return 1;
            }
            if (tx.Parent.Parent != null)
                return (int)tx.Tag - (int)ty.Tag;
            return string.Compare(tx.Text, ty.Text);
        }
    }

    public class GraphicSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            if (tx.Parent == null)
            {
                if ((int)tx.Tag == -1) //mob
                    return -1;
                else
                    return 1;
            }
            if (tx.Parent.Parent != null)
                return (int)tx.Tag - (int)ty.Tag;

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
