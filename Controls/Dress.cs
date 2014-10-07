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
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class Dress : UserControl
    {
        public Dress()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
        }

        #region Draworder Arrays
        private static int[] draworder ={
            0x00,// - Background
            0x14,// - Back (Cloak)
            0x05,// - Chest Clothing/Female Chest Armor
            0x04,// - Pants
            0x03,// - Foot Covering/Armor
            0x18,// - Legs (inner)(Leg Armor)
            0x13,// - Arm Covering/Armor
            0x0D,// - Torso (inner)(Chest Armor)
            0x11,// - Torso (Middle)(Surcoat, Tunic, Full Apron, Sash)
            0x08,// - Ring
            0x09,// - Talisman
            0x0E,// - Bracelet
            0x07,// - Gloves
            0x17,// - Legs (outer)(Skirt/Kilt)
            0x0A,// - Neck Covering/Armor
            0x0B,// - Hair
            0x0C,// - Waist (Half-Apron)
            0x16,// - Torso (outer)(Robe)
            0x10,// - Facial Hair
            0x12,// - Earrings
            0x06,// - Head Covering/Armor
            0x01,// - Single-Hand item/weapon
            0x02,// - Two-Hand item/weapon (including Shield)
            0x15 // - BackPack
        };
        private static int[] draworder2 ={
            0x00,// - Background
            0x05,// - Chest Clothing/Female Chest Armor
            0x04,// - Pants
            0x03,// - Foot Covering/Armor
            0x18,// - Legs (inner)(Leg Armor)
            0x13,// - Arm Covering/Armor
            0x0D,// - Torso (inner)(Chest Armor)
            0x11,// - Torso (Middle)(Surcoat, Tunic, Full Apron, Sash)
            0x08,// - Ring
            0x09,// - Talisman
            0x0E,// - Bracelet
            0x07,// - Gloves
            0x17,// - Legs (outer)(Skirt/Kilt)
            0x0A,// - Neck Covering/Armor
            0x0B,// - Hair
            0x0C,// - Waist (Half-Apron)
            0x16,// - Torso (outer)(Robe)
            0x10,// - Facial Hair
            0x12,// - Earrings
            0x06,// - Head Covering/Armor
            0x01,// - Single-Hand item/weapon
            0x02,// - Two-Hand item/weapon (including Shield)
            0x14,// - Back (Cloak)
            0x15 // - BackPack
        };
        #endregion
        private System.Drawing.Point drawpoint = new System.Drawing.Point(0, 0);
        private System.Drawing.Point drawpointAni = new System.Drawing.Point(100, 100);

        private object[] layers = new object[25];
        private bool[] layervisible = new bool[25];
        private bool female = false;
        private bool elve = false;
        private bool showPD = true;
        private bool animate = false;
        private Timer m_Timer = null;
        private Bitmap[] m_Animation;
        private int m_FrameIndex;
        private int facing = 1;
        private int action = 1;
        private bool Loaded = false;
        private int[] hues = new int[25];
        private int mount = 0;

        public void SetHue(int index, int color)
        {
            hues[index] = color;
        }

        /// <summary>
        /// Reload when loaded
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            Loaded = false;
            layers = new object[25];
            female = false;
            elve = false;
            showPD = true;
            animate = false;
            facing = 1;
            action = 1;
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
            EquipTable.Initialize();
            GumpTable.Initialize();
            OnLoad(this, EventArgs.Empty);
        }
        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Hues"] = true;
            Options.LoadedUltimaClass["Animations"] = true;
            Options.LoadedUltimaClass["Gumps"] = true;

            extractAnimationToolStripMenuItem.Visible = false;

            DressPic.Image = new Bitmap(DressPic.Width, DressPic.Height);
            pictureBoxDress.Image = new Bitmap(pictureBoxDress.Width, pictureBoxDress.Height);

            checkedListBoxWear.BeginUpdate();
            checkedListBoxWear.Items.Clear();
            for (int i = 0; i < layers.Length; ++i)
            {
                layers[i] = (object)0;
                checkedListBoxWear.Items.Add(String.Format("0x{0:X2}", i), true);
                layervisible[i] = true;
            }
            checkedListBoxWear.EndUpdate();

            groupBoxAnimate.Visible = false;
            animateToolStripMenuItem.Visible = false;
            FacingBar.Value = (facing + 3) & 7;
            ActionBar.Value = action;
            toolTip1.SetToolTip(FacingBar, FacingBar.Value.ToString());
            BuildDressList();
            DrawPaperdoll();
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void DrawPaperdoll()
        {
            if (!showPD)
            {
                DrawAnimation();
                return;
            }
            using (Graphics graphpic = Graphics.FromImage(DressPic.Image))
            {
                graphpic.Clear(Color.Black);
                if (layervisible[0])
                {
                    Bitmap background;
                    if (!female)
                        background = (!elve) ? Gumps.GetGump(0xC) : Gumps.GetGump(0xE);
                    else
                        background = (!elve) ? Gumps.GetGump(0xD) : Gumps.GetGump(0xF);
                    if (background != null)
                    {
                        if (hues[0] > 0)
                        {
                            Bitmap b = new Bitmap(background);
                            int hue = hues[0];
                            bool gray = false;
                            if ((hue & 0x8000) != 0)
                            {
                                hue ^= 0x8000;
                                gray = true;
                            }
                            Ultima.Hues.List[hue].ApplyTo(b, gray);
                            background = b;
                        }
                        graphpic.DrawImage(background, drawpoint);
                    }
                }
                for (int i = 1; i < draworder.Length; ++i)
                {
                    if ((int)layers[draworder[i]] != 0)
                    {
                        if (layervisible[draworder[i]])
                        {
                            int ani = TileData.ItemTable[(int)layers[draworder[i]]].Animation;
                            int gump = ani + 50000;
                            int hue = 0;
                            ConvertBody(ref ani, ref gump, ref hue);
                            if (female)
                            {
                                gump += 10000;
                                if (!Gumps.IsValidIndex(gump))  // female gump.def entry?
                                    ConvertGump(ref gump, ref hue);
                                if (!Gumps.IsValidIndex(gump)) // nope so male gump
                                    gump -= 10000;
                            }

                            if (!Gumps.IsValidIndex(gump)) // male (or invalid female)
                                ConvertGump(ref gump, ref hue);

                            if (Gumps.IsValidIndex(gump))
                            {
                                using (Bitmap bmp = new Bitmap(Gumps.GetGump(gump)))
                                {
                                    if (bmp == null)
                                        continue;
                                    if (hues[draworder[i]] > 0)
                                        hue = hues[draworder[i]];
                                    bool onlyHueGrayPixels = ((hue & 0x8000) != 0);
                                    hue = (hue & 0x3FFF) - 1;
                                    Hue hueObject = null;
                                    if (hue >= 0 && hue < Ultima.Hues.List.Length)
                                    {
                                        hueObject = Ultima.Hues.List[hue];
                                        hueObject.ApplyTo(bmp, onlyHueGrayPixels);
                                    }
                                    graphpic.DrawImage(bmp, drawpoint);
                                }
                            }
                        }
                    }
                }
            }
            DressPic.Invalidate();
        }

        private void DrawAnimation()
        {
            if (animate)
            {
                DoAnimation();
                return;
            }
            using (Graphics graphpic = Graphics.FromImage(DressPic.Image))
            {
                graphpic.Clear(Color.WhiteSmoke);
                int hue = 0;
                int back = 0;
                if (layervisible[0])
                {
                    if (!female)
                        back = (!elve) ? 400 : 605;
                    else
                        back = (!elve) ? 401 : 606;
                }
                Frame[] background;
                if (hues[0] > 0)
                {
                    hue = hues[0];
                    background = Animations.GetAnimation(back, action, facing, ref hue, true, true);
                }
                else
                    background = Animations.GetAnimation(back, action, facing, ref hue, false, true);

                System.Drawing.Point draw = new System.Drawing.Point();
                if (mount != 0)
                {
                    if ((action >= 23) && (action <= 29)) //mount animations
                    {
                        int mountaction;
                        switch (action)
                        {
                            case 23:
                                mountaction = 0;
                                break;
                            case 24:
                                mountaction = 1;
                                break;
                            case 25:
                                mountaction = 2;
                                break;
                            default:
                                mountaction = 5;
                                break;
                        }
                        if (Animations.IsActionDefined(mount, mountaction, facing))
                        {
                            hue = 0;
                            Frame[] mountframe = Animations.GetAnimation(mount, mountaction, facing, ref hue, false, false);
                            if ((mountframe.Length >= 0) && (mountframe[0].Bitmap != null))
                            {
                                draw.X = drawpointAni.X - mountframe[0].Center.X;
                                draw.Y = drawpointAni.Y - mountframe[0].Center.Y - mountframe[0].Bitmap.Height;
                                graphpic.DrawImage(mountframe[0].Bitmap, draw);
                            }
                        }
                    }
                }
                if (background != null)
                {
                    draw.X = drawpointAni.X - background[0].Center.X;
                    draw.Y = drawpointAni.Y - background[0].Center.Y - background[0].Bitmap.Height;
                    graphpic.DrawImage(background[0].Bitmap, draw);
                }
                int[] animorder = draworder2;
                if (((facing - 3) & 7) >= 4 && ((facing - 3) & 7) <= 6)
                    animorder = draworder;
                for (int i = 1; i < draworder.Length; ++i)
                {
                    if ((int)layers[animorder[i]] != 0)
                    {
                        if (layervisible[animorder[i]])
                        {
                            if (TileData.ItemTable == null)
                                break;
                            int ani = TileData.ItemTable[(int)layers[animorder[i]]].Animation;
                            int gump = ani + 50000;
                            hue = 0;
                            ConvertBody(ref ani, ref gump, ref hue);
                            if (!Animations.IsActionDefined(ani, action, facing))
                                continue;

                            Frame[] frames;
                            if (hues[animorder[i]] > 0)
                            {
                                hue = hues[animorder[i]];
                                frames = Animations.GetAnimation(ani, action, facing, ref hue, true, true);
                            }
                            else
                                frames = Animations.GetAnimation(ani, action, facing, ref hue, false, true);
                            Bitmap bmp = frames[0].Bitmap;
                            if (bmp == null)
                                continue;
                            draw.X = drawpointAni.X - frames[0].Center.X;
                            draw.Y = drawpointAni.Y - frames[0].Center.Y - frames[0].Bitmap.Height;

                            graphpic.DrawImage(bmp, draw);
                        }
                    }
                }
            }
            DressPic.Invalidate();
        }

        private void DoAnimation()
        {
            if (m_Timer == null)
            {
                int hue = 0;
                int back = 0;

                if (!female)
                    back = (!elve) ? 400 : 605;
                else
                    back = (!elve) ? 401 : 606;
                Frame[] mobile;
                if (hues[0] > 0)
                {
                    hue = hues[0];
                    mobile = Animations.GetAnimation(back, action, facing, ref hue, true, false);
                }
                else
                    mobile = Animations.GetAnimation(back, action, facing, ref hue, false, false);
                System.Drawing.Point draw = new System.Drawing.Point();

                int count = mobile.Length;
                m_Animation = new Bitmap[count];
                int[] animorder = draworder2;
                if (((facing - 3) & 7) >= 4 && ((facing - 3) & 7) <= 6)
                    animorder = draworder;

                for (int i = 0; i < count; ++i)
                {
                    m_Animation[i] = new Bitmap(DressPic.Width, DressPic.Height);
                    using (Graphics graph = Graphics.FromImage(m_Animation[i]))
                    {
                        graph.Clear(Color.WhiteSmoke);
                        if (mount != 0)
                        {
                            if ((action >= 23) && (action <= 29)) //mount animations
                            {
                                int mountaction;
                                switch (action)
                                {
                                    case 23:
                                        mountaction = 0;
                                        break;
                                    case 24:
                                        mountaction = 1;
                                        break;
                                    case 25:
                                        mountaction = 2;
                                        break;
                                    default:
                                        mountaction = 5;
                                        break;
                                }
                                if (Animations.IsActionDefined(mount, mountaction, facing))
                                {
                                    hue = 0;
                                    Frame[] mountframe = Animations.GetAnimation(mount, mountaction, facing, ref hue, false, false);
                                    if ((mountframe.Length > i) && (mountframe[i].Bitmap != null))
                                    {
                                        draw.X = drawpointAni.X - mountframe[i].Center.X;
                                        draw.Y = drawpointAni.Y - mountframe[i].Center.Y - mountframe[i].Bitmap.Height;
                                        graph.DrawImage(mountframe[i].Bitmap, draw);
                                    }
                                }
                            }
                        }
                        draw.X = drawpointAni.X - mobile[i].Center.X;
                        draw.Y = drawpointAni.Y - mobile[i].Center.Y - mobile[i].Bitmap.Height;
                        graph.DrawImage(mobile[i].Bitmap, draw);
                        for (int j = 1; j < animorder.Length; ++j)
                        {
                            if ((int)layers[animorder[j]] != 0)
                            {
                                if (layervisible[animorder[j]])
                                {
                                    int ani = TileData.ItemTable[(int)layers[animorder[j]]].Animation;
                                    int gump = ani + 50000;
                                    hue = 0;
                                    ConvertBody(ref ani, ref gump, ref hue);
                                    if (!Animations.IsActionDefined(ani, action, facing))
                                        continue;

                                    Frame[] frames;
                                    if (hues[animorder[j]] > 0)
                                    {
                                        hue = hues[animorder[j]];
                                        frames = Animations.GetAnimation(ani, action, facing, ref hue, true, false);
                                    }
                                    else
                                        frames = Animations.GetAnimation(ani, action, facing, ref hue, false, false);
                                    if ((frames.Length < i) || (frames[i].Bitmap == null))
                                        continue;
                                    draw.X = drawpointAni.X - frames[i].Center.X;
                                    draw.Y = drawpointAni.Y - frames[i].Center.Y - frames[i].Bitmap.Height;

                                    graph.DrawImage(frames[i].Bitmap, draw);
                                }
                            }
                        }
                    }
                }
                m_FrameIndex = 0;
                m_Timer = new Timer();
                m_Timer.Interval = 150;// 1000 / count;
                m_Timer.Tick += new EventHandler(AnimTick);
                m_Timer.Start();
            }
        }

        private void AnimTick(object sender, EventArgs e)
        {
            ++m_FrameIndex;

            if (m_FrameIndex >= m_Animation.Length)
                m_FrameIndex = 0;

            if (m_Animation == null)
                return;
            if (m_Animation[m_FrameIndex] == null)
                return;
            using (Graphics graph = Graphics.FromImage(DressPic.Image))
            {
                graph.DrawImage(m_Animation[m_FrameIndex], drawpoint);
            }
            DressPic.Invalidate();
        }

        private void AfterSelectTreeView(object sender, TreeViewEventArgs e)
        {
            int ani = TileData.ItemTable[(int)e.Node.Tag].Animation;
            int gump = ani + 50000;
            int gumporig = gump;
            int hue = 0;
            Animations.Translate(ref ani);
            ConvertBody(ref ani, ref gump, ref hue);
            if (female)
            {
                gump += 10000;
                if (!Gumps.IsValidIndex(gump))  // female gump.def entry?
                    ConvertGump(ref gump, ref hue);
                if (!Gumps.IsValidIndex(gump)) // nope so male gump
                    gump -= 10000;
            }

            if (!Gumps.IsValidIndex(gump)) // male (or invalid female)
                ConvertGump(ref gump, ref hue);

            using (Graphics graph = Graphics.FromImage(pictureBoxDress.Image))
            {
                graph.Clear(Color.Transparent);
                Bitmap bmp = Gumps.GetGump(gump);
                if (bmp != null)
                {
                    bool onlyHueGrayPixels = ((hue & 0x8000) != 0);
                    hue = (hue & 0x3FFF) - 1;
                    Hue hueObject = null;
                    if (hue >= 0 && hue < Ultima.Hues.List.Length)
                    {
                        hueObject = Ultima.Hues.List[hue];
                        hueObject.ApplyTo(bmp, onlyHueGrayPixels);
                    }
                    int width = bmp.Width;
                    int height = bmp.Height;
                    if (width > pictureBoxDress.Width)
                    {
                        width = pictureBoxDress.Width;
                        height = bmp.Height * bmp.Height / bmp.Width;
                    }
                    if (height > pictureBoxDress.Height)
                    {
                        height = pictureBoxDress.Height;
                        width = pictureBoxDress.Width * bmp.Width / bmp.Height;
                    }
                    graph.DrawImage(bmp, new Rectangle(0, 0, width, height));
                }
            }
            pictureBoxDress.Invalidate();
            TextBox.Clear();
            TextBox.AppendText(String.Format("Objtype: 0x{0:X4}  Layer: 0x{1:X2}\n",
                (int)e.Node.Tag,
                TileData.ItemTable[(int)e.Node.Tag].Quality));
            TextBox.AppendText(String.Format("GumpID: 0x{0:X4} (0x{1:X4}) Hue: {2}\n",
                gump,
                gumporig,
                hue + 1));
            TextBox.AppendText(String.Format("Animation: 0x{0:X4} (0x{1:X4})\n",
                ani,
                TileData.ItemTable[(int)e.Node.Tag].Animation));
            TextBox.AppendText(String.Format("ValidGump: {0} ValidAnim: {1}\n",
                Gumps.IsValidIndex(gump).ToString(),
                Animations.IsActionDefined(ani, 0, 0).ToString()));
            TextBox.AppendText(String.Format("ValidLayer: {0}",
                (Array.IndexOf(draworder, TileData.ItemTable[(int)e.Node.Tag].Quality) == -1 ? false : true)));
        }

        private void OnClick_Animate(object sender, EventArgs e)
        {
            animate = !animate;
            if (animate)
                extractAnimationToolStripMenuItem.Visible = true;
            else
                extractAnimationToolStripMenuItem.Visible = false;
            RefreshDrawing();
        }

        private void OnChangeFemale(object sender, EventArgs e)
        {
            female = !female;
            RefreshDrawing();
        }

        private void OnChangeElve(object sender, EventArgs e)
        {
            elve = !elve;
            RefreshDrawing();
        }

        private void OnClick_Dress(object sender, EventArgs e)
        {
            if (treeViewItems.SelectedNode == null)
                return;
            int objtype = (int)treeViewItems.SelectedNode.Tag;
            int layer = TileData.ItemTable[objtype].Quality;
            if (Array.IndexOf(draworder, layer) == -1)
                return;
            layers[layer] = (object)objtype;
            checkedListBoxWear.BeginUpdate();
            checkedListBoxWear.Items[layer] = String.Format("0x{0:X2} {1}", layer, TileData.ItemTable[objtype].Name);
            checkedListBoxWear.EndUpdate();
            RefreshDrawing();
        }

        private void OnClick_UnDress(object sender, EventArgs e)
        {
            if (checkedListBoxWear.SelectedIndex == -1)
                return;
            int layer = checkedListBoxWear.SelectedIndex;
            checkedListBoxWear.Items[checkedListBoxWear.SelectedIndex] = String.Format("0x{0:X2}", layer);
            layers[layer] = (object)0;
            RefreshDrawing();
        }

        private void OnClickUndressAll(object sender, EventArgs e)
        {
            for (int i = 0; i < layers.Length; ++i)
            {
                layers[i] = (object)0;
                checkedListBoxWear.Items[i] = String.Format("0x{0:X2}", i);
            }

            RefreshDrawing();
        }

        private void checkedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (Loaded)
            {
                layervisible[e.Index] = (e.NewValue == CheckState.Checked) ? true : false;
                RefreshDrawing();
            }
        }

        private void checkedListBox_Change(object sender, EventArgs e)
        {
            //RefreshDrawing();
            if (checkedListBoxWear.SelectedIndex == -1)
                return;
            int layer = checkedListBoxWear.SelectedIndex;
            int objtype = (int)layers[layer];
            int ani = TileData.ItemTable[objtype].Animation;
            int gumpidorig = ani + 50000;
            int gumpid = gumpidorig;
            int hue = 0;
            Animations.Translate(ref ani);
            ConvertBody(ref ani, ref gumpid, ref hue);
            if (female)
            {
                gumpid += 10000;
                if (!Gumps.IsValidIndex(gumpid))  // female gump.def entry?
                    ConvertGump(ref gumpid, ref hue);
                if (!Gumps.IsValidIndex(gumpid)) // nope so male gump
                    gumpid -= 10000;
            }

            if (!Gumps.IsValidIndex(gumpid)) // male (or invalid female)
                ConvertGump(ref gumpid, ref hue);

            TextBox.Clear();
            TextBox.AppendText(String.Format("Objtype: 0x{0:X4}  Layer: 0x{1:X2}\n",
                objtype,
                layer));
            TextBox.AppendText(String.Format("GumpID: 0x{0:X4} (0x{1:X4}) Hue: {2}\n",
                gumpid,
                gumpidorig,
                hue));
            TextBox.AppendText(String.Format("Animation: 0x{0:X4} (0x{1:X4})\n",
                ani,
                TileData.ItemTable[objtype].Animation));
            TextBox.AppendText(String.Format("ValidGump: {0} ValidAnim: {1}",
                Gumps.IsValidIndex(gumpid).ToString(),
                Animations.IsActionDefined(ani, 0, 0).ToString()));
        }

        private void OnChangeSort(object sender, EventArgs e)
        {
            if (LayerSort.Checked)
                treeViewItems.TreeViewNodeSorter = new LayerSorter();
            else
                treeViewItems.TreeViewNodeSorter = new ObjtypeSorter();
        }

        private void OnClick_ChangeDisplay(object sender, EventArgs e)
        {
            showPD = !showPD;
            if (showPD)
            {
                groupBoxAnimate.Visible = false;
                animateToolStripMenuItem.Visible = false;
                showAnimationToolStripMenuItem.Text = "Show Animation";
            }
            else
            {
                groupBoxAnimate.Visible = true;
                animateToolStripMenuItem.Visible = true;
                showAnimationToolStripMenuItem.Text = "Show Paperdoll";
            }
            RefreshDrawing();
        }

        private void BuildDressList()
        {
            treeViewItems.BeginUpdate();
            treeViewItems.Nodes.Clear();
            if (TileData.ItemTable != null)
            {
                for (int i = 0; i < TileData.ItemTable.Length; ++i)
                {
                    if (TileData.ItemTable[i].Wearable)
                    {
                        int ani = TileData.ItemTable[i].Animation;
                        if (ani != 0)
                        {
                            int hue = 0;
                            int gump = ani + 50000;
                            ConvertBody(ref ani, ref gump, ref hue);
                            if (!Gumps.IsValidIndex(gump))
                                ConvertGump(ref gump, ref hue);
                            bool hasani = Animations.IsActionDefined(ani, 0, 0);
                            bool hasgump = Gumps.IsValidIndex(gump);
                            TreeNode node = new TreeNode(String.Format("0x{0:X4} (0x{1:X2}) {2}",
                                        i,
                                        TileData.ItemTable[i].Quality,
                                        TileData.ItemTable[i].Name));
                            node.Tag = i;
                            if (Array.IndexOf(draworder, (int)TileData.ItemTable[i].Quality) == -1)
                                node.ForeColor = Color.DarkRed;
                            else if (!hasani)
                            {
                                if (!hasgump)
                                    node.ForeColor = Color.Red;
                                else
                                    node.ForeColor = Color.Orange;
                            }
                            else if (hasani && !hasgump)
                                node.ForeColor = Color.Blue;
                            treeViewItems.Nodes.Add(node);
                        }
                    }
                }
            }
            treeViewItems.EndUpdate();
        }

        public void RefreshDrawing()
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

            DrawPaperdoll();
        }

        private void OnScroll_Facing(object sender, EventArgs e)
        {
            facing = (FacingBar.Value - 3) & 7;
            toolTip1.SetToolTip(FacingBar, FacingBar.Value.ToString());
            RefreshDrawing();
        }

        private void OnScroll_Action(object sender, EventArgs e)
        {
            string[] tip = new string[]{"Walk_01","WalkStaff_01","Run_01","RunStaff_01","Idle_01","Idle_01",
                         "Fidget_Yawn_Stretch_01","CombatIdle1H_01","CombatIdle1H_01","AttackSlash1H_01",
                         "AttackPierce1H_01","AttackBash1H_01","AttackBash2H_01","AttackSlash2H_01",
                         "AttackPierce2H_01","CombatAdvance_1H_01","Spell1","Spell2","AttackBow_01",
                         "AttackCrossbow_01","GetHit_Fr_Hi_01","Die_Hard_Fwd_01","Die_Hard_Back_01",
                         "Horse_Walk_01","Horse_Run_01","Horse_Idle_01","Horse_Attack1H_SlashRight_01",
                         "Horse_AttackBow_01","Horse_AttackCrossbow_01","Horse_Attack2H_SlashRight_01",
                         "Block_Shield_Hard_01","Punch_Punch_Jab_01","Bow_Lesser_01","Salute_Armed1h_01",
                         "Ingest_Eat_01"};
            toolTip1.SetToolTip(ActionBar, ActionBar.Value.ToString() + " " + tip[ActionBar.Value]);
            action = ActionBar.Value;
            RefreshDrawing();
        }

        private void OnResizepictureDress(object sender, EventArgs e)
        {
            if (treeViewItems.SelectedNode != null)
            {
                pictureBoxDress.Image = new Bitmap(pictureBoxDress.Width, pictureBoxDress.Height);
                AfterSelectTreeView(this, new TreeViewEventArgs(treeViewItems.SelectedNode));
            }
        }

        private void OnResizeDressPic(object sender, EventArgs e)
        {
            DressPic.Image = new Bitmap(DressPic.Width, DressPic.Height);
            if (Loaded) // inital event
                RefreshDrawing();
        }

        private void ConvertGump(ref int gumpid, ref int hue)
        {
            if (GumpTable.Entries.Contains(gumpid))
            {
                GumpTableEntry entry = (GumpTableEntry)GumpTable.Entries[gumpid];
                hue = entry.NewHue;
                gumpid = entry.NewID;
            }
        }

        private void ConvertBody(ref int animId, ref int gumpid, ref int hue)
        {
            if (!elve)
            {
                if (!female)
                {
                    if (EquipTable.Human_male.Contains(animId))
                    {
                        EquipTableEntry entry = (EquipTableEntry)EquipTable.Human_male[animId];
                        gumpid = entry.NewID;
                        hue = entry.NewHue;
                        animId = entry.NewAnim;
                    }
                }
                else
                {
                    if (EquipTable.Human_female.Contains(animId))
                    {
                        EquipTableEntry entry = (EquipTableEntry)EquipTable.Human_female[animId];
                        gumpid = entry.NewID;
                        hue = entry.NewHue;
                        animId = entry.NewAnim;
                    }
                }
            }
            else
            {
                if (!female)
                {
                    if (EquipTable.Elven_male.Contains(animId))
                    {
                        EquipTableEntry entry = (EquipTableEntry)EquipTable.Elven_male[animId];
                        gumpid = entry.NewID;
                        hue = entry.NewHue;
                        animId = entry.NewAnim;
                    }
                }
                else
                {
                    if (EquipTable.Elven_female.Contains(animId))
                    {
                        EquipTableEntry entry = (EquipTableEntry)EquipTable.Elven_female[animId];
                        gumpid = entry.NewID;
                        hue = entry.NewHue;
                        animId = entry.NewAnim;
                    }
                }
            }
        }

        private HuePopUpDress showform = null;
        private void onClickHue(object sender, EventArgs e)
        {
            if (checkedListBoxWear.SelectedIndex == -1)
                return;

            if ((showform == null) || (showform.IsDisposed))
            {
                int layer = checkedListBoxWear.SelectedIndex;
                showform = new HuePopUpDress(this, hues[layer], layer);
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void OnKeyDownHue(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            if (checkedListBoxWear.SelectedIndex == -1)
                return;
            int index;
            if (Utils.ConvertStringToInt(toolStripTextBox1.Text, out index, 0, Ultima.Hues.List.Length))
            {
                hues[checkedListBoxWear.SelectedIndex] = index;
                RefreshDrawing();
            }
        }

        private void OnClickExtractImageBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            if (showPD)
            {
                string FileName = Path.Combine(path, "Dress PD.bmp");
                DressPic.Image.Save(FileName, ImageFormat.Bmp);
                MessageBox.Show(
                    String.Format("Paperdoll saved to {0}", FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
            else
            {
                string FileName = Path.Combine(path, "Dress IG.bmp");
                if (animate)
                    m_Animation[0].Save(FileName, ImageFormat.Bmp);
                else
                    DressPic.Image.Save(FileName, ImageFormat.Bmp);
                MessageBox.Show(
                    String.Format("InGame saved to {0}", FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExtractImageTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            if (showPD)
            {
                string FileName = Path.Combine(path, "Dress PD.tiff");
                DressPic.Image.Save(FileName, ImageFormat.Tiff);
                MessageBox.Show(
                    String.Format("Paperdoll saved to {0}", FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
            else
            {
                string FileName = Path.Combine(path, "Dress IG.tiff");
                if (animate)
                    m_Animation[0].Save(FileName, ImageFormat.Tiff);
                else
                    DressPic.Image.Save(FileName, ImageFormat.Tiff);
                MessageBox.Show(
                    String.Format("InGame saved to {0}", FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExtractImageJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            if (showPD)
            {
                string FileName = Path.Combine(path, "Dress PD.jpg");
                DressPic.Image.Save(FileName, ImageFormat.Jpeg);
                MessageBox.Show(
                    String.Format("Paperdoll saved to {0}", FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
            else
            {
                string FileName = Path.Combine(path, "Dress IG.jpg");
                if (animate)
                    m_Animation[0].Save(FileName, ImageFormat.Jpeg);
                else
                    DressPic.Image.Save(FileName, ImageFormat.Jpeg);
                MessageBox.Show(
                    String.Format("InGame saved to {0}", FileName),
                    "Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickExtractAnimBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = "Dress Anim";

            for (int i = 0; i < m_Animation.Length; ++i)
            {
                m_Animation[i].Save(Path.Combine(path, String.Format("{0}-{1}.bmp", FileName, i)), ImageFormat.Bmp);
            }
            MessageBox.Show(
                String.Format("InGame Anim saved to '{0}-X.bmp'", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExtractAnimTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = "Dress Anim";

            for (int i = 0; i < m_Animation.Length; ++i)
            {
                m_Animation[i].Save(Path.Combine(path, String.Format("{0}-{1}.tiff", FileName, i)), ImageFormat.Tiff);
            }
            MessageBox.Show(
                String.Format("InGame Anim saved to '{0}-X.tiff'", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExtractAnimJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = "Dress Anim";

            for (int i = 0; i < m_Animation.Length; ++i)
            {
                m_Animation[i].Save(Path.Combine(path, String.Format("{0}-{1}.jpg", FileName, i)), ImageFormat.Jpeg);
            }
            MessageBox.Show(
                String.Format("InGame Anim saved to '{0}-X.jpg'", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickBuildAnimationList(object sender, EventArgs e)
        {
            AnimEntry[] animentries = new AnimEntry[1000];
            for (int i = 0; i < animentries.Length; ++i)
            {
                animentries[i] = new AnimEntry();
                animentries[i].Animation = i;
                animentries[i].FirstGump = i + 50000;
                animentries[i].FirstGumpFemale = i + 60000;
                if (EquipTable.Human_male.Contains(i))
                    animentries[i].EquipTable[400] = (EquipTableEntry)EquipTable.Human_male[i];
                if (EquipTable.Human_female.Contains(i))
                    animentries[i].EquipTable[401] = (EquipTableEntry)EquipTable.Human_female[i];
                if (EquipTable.Elven_male.Contains(i))
                    animentries[i].EquipTable[605] = (EquipTableEntry)EquipTable.Elven_male[i];
                if (EquipTable.Elven_female.Contains(i))
                    animentries[i].EquipTable[606] = (EquipTableEntry)EquipTable.Elven_female[i];
                IDictionaryEnumerator _itr;
                if (EquipTable.Misc.Contains(i))
                {
                    _itr = ((Hashtable)EquipTable.Misc[i]).GetEnumerator();
                    while (_itr.MoveNext())
                    {
                        animentries[i].EquipTable[_itr.Key] = (EquipTableEntry)_itr.Value;
                    }
                }
                _itr = animentries[i].EquipTable.GetEnumerator();
                if (animentries[i].EquipTable.Count == 0)
                {
                    if (GumpTable.Entries.Contains(animentries[i].FirstGump))
                    {
                        animentries[i].GumpDef[0] = (GumpTableEntry)GumpTable.Entries[animentries[i].FirstGump];
                    }
                }
                else
                {
                    while (_itr.MoveNext())
                    {
                        int newgump = ((EquipTableEntry)_itr.Value).NewID;
                        if (GumpTable.Entries.Contains(newgump))
                        {
                            animentries[i].GumpDef[_itr.Key] = (GumpTableEntry)GumpTable.Entries[newgump];
                        }
                    }
                }
                _itr.Reset();
                if (animentries[i].EquipTable.Count == 0)
                {
                    int tmp = new int();
                    tmp = i;
                    animentries[i].TranslateAnim[0] = new TranslateAnimEntry();
                    ((TranslateAnimEntry)animentries[i].TranslateAnim[0]).bodydef = BodyTable.m_Entries.ContainsKey(tmp);
                    Animations.Translate(ref tmp);
                    ((TranslateAnimEntry)animentries[i].TranslateAnim[0]).fileindex = BodyConverter.Convert(ref tmp);
                    ((TranslateAnimEntry)animentries[i].TranslateAnim[0]).bodyandconf = tmp;
                }
                else
                {
                    while (_itr.MoveNext())
                    {
                        int tmp = new int();
                        tmp = ((EquipTableEntry)_itr.Value).NewAnim;
                        animentries[i].TranslateAnim[_itr.Key] = new TranslateAnimEntry();
                        ((TranslateAnimEntry)animentries[i].TranslateAnim[_itr.Key]).bodydef = BodyTable.m_Entries.ContainsKey(tmp);
                        Animations.Translate(ref tmp);
                        ((TranslateAnimEntry)animentries[i].TranslateAnim[_itr.Key]).fileindex = BodyConverter.Convert(ref tmp);
                        ((TranslateAnimEntry)animentries[i].TranslateAnim[_itr.Key]).bodyandconf = tmp;

                    }
                }
            }

            string FileName = Path.Combine(FiddlerControls.Options.OutputPath, "animationlist.html");
            using (StreamWriter Tex = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write), System.Text.Encoding.GetEncoding(1252)))
            {
                Tex.WriteLine("<html> <body> <table border='1' rules='all' cellpadding='2'>");
                Tex.WriteLine("<tr>");
                Tex.WriteLine("<td>Anim</td>");
                Tex.WriteLine("<td>Gump male/female</td>");
                Tex.WriteLine("<td>equipconv<br/>model:anim,gump,hue</td>");
                Tex.WriteLine("<td>gump.def<br/>gump,hue</td>");
                Tex.WriteLine("<td>body.def/bodyconv<br/>[model:]fileindex,anim</td>");
                Tex.WriteLine("<td>tiledata def</td>");
                Tex.WriteLine("</tr>");
                for (int i = 1; i < animentries.Length; ++i)
                {
                    Tex.WriteLine("<tr>");
                    Tex.Write("<td>");
                    bool openfont = false;

                    if (!Ultima.Animations.IsActionDefined(i, 0, 0))
                    {
                        Tex.Write("<font color=#FF0000>");
                        openfont = true;
                    }

                    Tex.Write(i);
                    if (openfont)
                        Tex.Write("</font>");
                    Tex.Write("</td>");
                    if (i >= 400)
                    {
                        Tex.Write("<td>");
                        openfont = false;
                        if (!Ultima.Gumps.IsValidIndex(animentries[i].FirstGump))
                        {
                            Tex.Write("<font color=#FF0000>");
                            openfont = true;
                        }
                        Tex.Write(animentries[i].FirstGump);
                        if (openfont)
                            Tex.Write("</font>");
                        Tex.Write("/");
                        openfont = false;
                        if (!Ultima.Gumps.IsValidIndex(animentries[i].FirstGumpFemale))
                        {
                            Tex.Write("<font color=#FF0000>");
                            openfont = true;
                        }
                        Tex.Write(animentries[i].FirstGumpFemale);
                        if (openfont)
                            Tex.Write("</font>");
                        Tex.Write("</td>");
                    }
                    else
                        Tex.Write("<td></td>");

                    IDictionaryEnumerator _itr;
                    _itr = animentries[i].EquipTable.GetEnumerator();
                    Tex.Write("<td>");
                    while (_itr.MoveNext())
                    {
                        if ((int)_itr.Key != 0)
                            Tex.Write(_itr.Key + ":");
                        openfont = false;
                        if (animentries[i].TranslateAnim.ContainsKey(_itr.Key))
                        {
                            if (!Ultima.Animations.IsAnimDefinied(((TranslateAnimEntry)animentries[i].TranslateAnim[_itr.Key]).bodyandconf, 0, 0,
                                ((TranslateAnimEntry)animentries[i].TranslateAnim[_itr.Key]).fileindex))
                            {
                                Tex.Write("<font color=#FF0000>");
                                openfont = true;
                            }

                        }
                        Tex.Write(((EquipTableEntry)_itr.Value).NewAnim);
                        if (openfont)
                            Tex.Write("</font>");
                        Tex.Write(",");
                        openfont = false;
                        if (!Ultima.Gumps.IsValidIndex(animentries[i].FirstGumpFemale))
                        {
                            Tex.Write("<font color=#FF0000>");
                            openfont = true;
                        }
                        Tex.Write(((EquipTableEntry)_itr.Value).NewID);
                        if (openfont)
                            Tex.Write("</font>");
                        Tex.Write(",");
                        Tex.Write(((EquipTableEntry)_itr.Value).NewHue);
                        Tex.Write("<br/>");
                    }
                    Tex.Write("</td>");
                    _itr = animentries[i].GumpDef.GetEnumerator();
                    Tex.Write("<td>");
                    while (_itr.MoveNext())
                    {
                        if ((int)_itr.Key != 0)
                            Tex.Write(_itr.Key + ":");
                        openfont = false;
                        if (!Ultima.Gumps.IsValidIndex(((GumpTableEntry)_itr.Value).NewID))
                        {
                            Tex.Write("<font color=#FF0000>");
                            openfont = true;
                        }
                        Tex.Write(((GumpTableEntry)_itr.Value).NewID);
                        if (openfont)
                            Tex.Write("</font>");
                        Tex.Write("," + ((GumpTableEntry)_itr.Value).NewHue + "<br/>");
                    }
                    Tex.Write("</td>");
                    _itr = animentries[i].TranslateAnim.GetEnumerator();
                    Tex.Write("<td>");
                    while (_itr.MoveNext())
                    {
                        if ((((TranslateAnimEntry)_itr.Value).fileindex == 1) && (!((TranslateAnimEntry)_itr.Value).bodydef))
                            continue;
                        if ((int)_itr.Key != 0)
                            Tex.Write(_itr.Key + ":");
                        Tex.Write(((TranslateAnimEntry)_itr.Value).fileindex
                            + "," + ((TranslateAnimEntry)_itr.Value).bodyandconf + "<br/>");
                    }
                    Tex.Write("</td>");
                    Tex.Write("<td>");
                    if (i >= 400)
                    {
                        for (int j = 0; j < Ultima.TileData.ItemTable.Length; ++j)
                        {
                            if (Ultima.TileData.ItemTable[j].Animation == i)
                                Tex.Write(String.Format("0x{0:X4} {1}<br/>", j, Ultima.TileData.ItemTable[j].Name));
                        }
                    }
                    Tex.Write("</td>");
                    Tex.WriteLine("</tr>");

                }
                Tex.WriteLine("</table> </body> </html>");
            }

            MessageBox.Show(
                String.Format("Report saved to '{0}'", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void MountTextBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(textBoxMount.Text, out index, 0, 0xFFFF))
                {
                    if (Ultima.Animations.IsActionDefined(index, 0, 0))
                    {
                        mount = index;
                        RefreshDrawing();
                    }
                }
            }
        }
    }

    public class TranslateAnimEntry
    {
        public int fileindex { get; set; }
        public int bodyandconf { get; set; }
        public bool bodydef { get; set; }
        public TranslateAnimEntry()
        {
        }
    }

    public class AnimEntry
    {
        public struct EquipTableDef { public int gump; public int anim;}
        public int Animation { get; set; }
        public int FirstGump { get; set; } //+50000
        public int FirstGumpFemale { get; set; }//+60000
        public Hashtable EquipTable { get; set; } //equipconv.def with model
        public Hashtable GumpDef { get; set; } //gump.def if gump invalid (only for paperdoll)
        public Hashtable TranslateAnim { get; set; }//body.def or bodyconv.def

        public AnimEntry()
        {
            EquipTable = new Hashtable();
            GumpDef = new Hashtable();
            TranslateAnim = new Hashtable();
        }
    }

    public class ObjtypeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            return string.Compare(tx.Text, ty.Text);
        }
    }

    public class LayerSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            int layerx = TileData.ItemTable[(int)tx.Tag].Quality;
            int layery = TileData.ItemTable[(int)ty.Tag].Quality;
            if (layerx == layery)
                return 0;
            else if (layerx < layery)
                return -1;
            else
                return 1;
        }
    }

    public class GumpTable
    {
        public static Hashtable Entries { get; private set; }

        // Seems only used if Gump is invalid
        static GumpTable()
        {
            Entries = new Hashtable();
            Initialize();
        }
        public static void Initialize()
        {
            string path = Files.GetFilePath("gump.def");
            if (path == null)
                return;

            using (StreamReader ip = new StreamReader(path))
            {
                string line;
                while ((line = ip.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        continue;

                    try
                    {
                        // <ORIG BODY> {<NEW BODY>} <NEW HUE>
                        int index1 = line.IndexOf("{");
                        int index2 = line.IndexOf("}");

                        string param1 = line.Substring(0, index1);
                        string param2 = line.Substring(index1 + 1, index2 - index1 - 1);
                        string param3 = line.Substring(index2 + 1);

                        int iParam1 = Convert.ToInt32(param1.Trim());
                        int iParam2 = Convert.ToInt32(param2.Trim());
                        int iParam3 = Convert.ToInt32(param3.Trim());

                        Entries[iParam1] = new GumpTableEntry(iParam1, iParam2, iParam3);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
    public class GumpTableEntry
    {
        public int OldID { get; private set; }
        public int NewID { get; private set; }
        public int NewHue { get; private set; }

        public GumpTableEntry(int oldID, int newID, int newHue)
        {
            OldID = oldID;
            NewID = newID;
            NewHue = newHue;
        }
    }

    public class EquipTable
    {
        public static Hashtable Human_male { get; private set; }
        public static Hashtable Human_female { get; private set; }
        public static Hashtable Elven_male { get; private set; }
        public static Hashtable Elven_female { get; private set; }
        public static Hashtable Misc { get; private set; }

        static EquipTable()
        {
            Human_male = new Hashtable();
            Human_female = new Hashtable();
            Elven_male = new Hashtable();
            Elven_female = new Hashtable();
            Misc = new Hashtable();
            Initialize();
        }

        public static void Initialize()
        {
            string path = Files.GetFilePath("equipconv.def");
            if (path == null)
                return;

            using (StreamReader ip = new StreamReader(path))
            {
                string line;
                while ((line = ip.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        continue;
                    //#bodyType	#equipmentID	#convertToID	#GumpIDToUse	#hue
                    //GumpID (0 = equipmentID + 50000, -1 = convertToID + 50000, other numbers are the actual gumpID )

                    try
                    {
                        string[] split = Regex.Split(line, @"\s+");

                        int bodytype = Convert.ToInt32(split[0]);
                        int animID = Convert.ToInt32(split[1]);
                        int convertID = Convert.ToInt32(split[2]);
                        int gumpID = Convert.ToInt32(split[3]);
                        int hue = Convert.ToInt32(split[4]);
                        if (gumpID == 0)
                            gumpID = animID + 50000;
                        else if (gumpID == -1)
                            gumpID = convertID + 50000;

                        EquipTableEntry entry = new EquipTableEntry(gumpID, hue, convertID);
                        if (bodytype == 400)
                            Human_male[animID] = entry;
                        else if (bodytype == 401)
                            Human_female[animID] = entry;
                        else if (bodytype == 605)
                            Elven_male[animID] = entry;
                        else if (bodytype == 606)
                            Elven_female[animID] = entry;
                        else
                        {
                            if (!Misc.Contains(animID))
                                Misc[animID] = new Hashtable();
                            ((Hashtable)Misc[animID])[bodytype] = entry;
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
    public class EquipTableEntry
    {
        public int NewID { get; private set; }
        public int NewHue { get; private set; }
        public int NewAnim { get; private set; }

        public EquipTableEntry(int newID, int newHue, int newAnim)
        {
            NewID = newID;
            NewHue = newHue;
            NewAnim = newAnim;
        }
    }
}
