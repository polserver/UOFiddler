/***************************************************************************
 *
 * $Author: MuadDib & Turley
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
using System.Windows.Forms;
using System.Xml;
using Ultima;

namespace MultiEditor
{
    public partial class MultiEditor : UserControl
    {
        #region Fields (14)

        private MultiEditorComponentList compList;
        //Sin/Cos(45°)
        private static readonly double CoordinateTransform = Math.Sqrt(2) / 2;
        private const int DrawTileSizeHeight = 45;
        private const int DrawTileSizeWidth = 45;
        private List<int> DrawTilesList = new List<int>();
        private List<MultiTile> OverlayList = new List<MultiTile>();
        private bool Loaded = false;
        private int m_DrawFloorZ;
        private bool moving;
        private MultiTile m_DrawTile;
        private MultiTile m_HoverTile;
        private MultiTile m_SelectedTile;
        /// <summary>
        /// Current MouseLoc + Scrollbar values (for hover effect)
        /// </summary>
        private Point MouseLoc;
        private Point MovingLoc;
        private int pictureboxDrawTilescol;
        private int pictureboxDrawTilesrow;
        private static MultiEditor refMarkerMulti = null;
        private bool ForceRefresh;

        #endregion Fields

        #region Constructors (1)

        public MultiEditor()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            InitializeComponent();
            refMarkerMulti = this;
            MouseLoc = new Point();
            MovingLoc = new Point();
            m_DrawTile = new MultiTile();
            Selectedpanel.Visible = false;
            FloatingPreviewPanel.Visible = false;
            FloatingPreviewPanel.Tag = -1;
            BTN_Select.Checked = true;
            pictureBoxDrawTiles.MouseWheel += new MouseEventHandler(pictureBoxDrawTiles_OnMouseWheel);
            pictureBoxMulti.ContextMenu = null;
            ForceRefresh = true;
        }

        #endregion Constructors

        #region Properties (3)

        /// <summary>
        /// Floor Z level
        /// </summary>
        public int DrawFloorZ { get { return m_DrawFloorZ; } }

        /// <summary>
        /// Current Hovered Tile (set inside MultiComponentList)
        /// </summary>
        public MultiTile HoverTile
        {
            get { return m_HoverTile; }
            set
            {
                m_HoverTile = value;
                if (value != null)
                    toolTip1.SetToolTip(pictureBoxMulti, String.Format("ID: 0x{0:X} Z: {1}", m_HoverTile.ID, m_HoverTile.Z));
                else
                    toolTip1.SetToolTip(pictureBoxMulti, String.Empty);
            }
        }

        /// <summary>
        /// Current Selected Tile (set OnMouseUp)
        /// </summary>
        public MultiTile SelectedTile
        {
            get { return m_SelectedTile; }
            set
            {
                m_SelectedTile = value;
                if (value != null)
                {
                    SelectedTileLabel.Text = String.Format("ID: 0x{0:X} Z: {1}", value.ID, value.Z);
                    numericUpDown_Selected_X.Value = value.X;
                    numericUpDown_Selected_Y.Value = value.Y;
                    numericUpDown_Selected_Z.Value = value.Z;
                    DynamiccheckBox.Checked = value.Invisible;
                }
                else
                    SelectedTileLabel.Text = "ID:";
            }
        }

        public bool ShowWalkables { get { return showWalkablesToolStripMenuItem.Checked; } }
        public bool ShowDoubleSurface { get { return showDoubleSurfaceMenuItem.Checked; } }


        #endregion Properties

        #region Methods (35)

        // Private Methods (35) 

        /// <summary>
        /// Creates new blank Multi
        /// </summary>
        private void BTN_CreateBlank_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to create a blank Multi?", "Create", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                int width = (int)numericUpDown_Size_Width.Value;
                int height = (int)numericUpDown_Size_Height.Value;
                compList = new MultiEditorComponentList(width, height, this);
                UndoList_Clear();
                MaxHeightTrackBar.Minimum = compList.zMin;
                MaxHeightTrackBar.Maximum = compList.zMax;
                MaxHeightTrackBar.Value = compList.zMax;
                numericUpDown_Selected_X.Maximum = compList.Width - 1;
                numericUpDown_Selected_Y.Maximum = compList.Height - 1;
                ScrollbarsSetValue();
                ForceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        /// <summary>
        /// Draw Button activate
        /// </summary>
        private void BTN_Draw_Click(object sender, EventArgs e)
        {
            BTN_Select.Checked = false;
            BTN_Draw.Checked = true;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = false;
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void BTN_Export_TXT_OnClick(object sender, EventArgs e)
        {
            if (compList != null)
            {
                string path = FiddlerControls.Options.OutputPath;
                string FileName = Path.Combine(path, String.Format(@"{0}.txt", textBox_Export.Text));
                MultiComponentList sdklist = compList.ConvertToSDK();
                sdklist.ExportToTextFile(FileName);
            }
        }

        private void BTN_Export_UOA_OnClick(object sender, EventArgs e)
        {
            if (compList != null)
            {
                string path = FiddlerControls.Options.OutputPath;
                string FileName = Path.Combine(path, String.Format(@"{0}.uoa", textBox_Export.Text));
                MultiComponentList sdklist = compList.ConvertToSDK();
                sdklist.ExportToUOAFile(FileName);
            }
        }

        private void BTN_Export_WSC_OnClick(object sender, EventArgs e)
        {
            if (compList != null)
            {
                string path = FiddlerControls.Options.OutputPath;
                string FileName = Path.Combine(path, String.Format(@"{0}.wsc", textBox_Export.Text));
                MultiComponentList sdklist = compList.ConvertToSDK();
                sdklist.ExportToWscFile(FileName);
            }
        }

        /// <summary>
        /// Virtual Floor clicked (check on click)
        /// </summary>
        private void BTN_Floor_Clicked(object sender, EventArgs e)
        {
            m_DrawFloorZ = (int)numericUpDown_Floor.Value;
            ScrollbarsSetValue();
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Change DrawTile to clicked Tile
        /// </summary>
        private void BTN_Pipette_Click(object sender, EventArgs e)
        {
            BTN_Select.Checked = false;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = true;
            BTN_Trans.Checked = false;
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Remove Button activate
        /// </summary>
        private void BTN_Remove_Click(object sender, EventArgs e)
        {
            BTN_Select.Checked = false;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = true;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = false;
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Resize Multi Button clicked
        /// </summary>
        private void BTN_ResizeMulti_Click(object sender, EventArgs e)
        {
            if (compList != null)
            {
                int width = (int)numericUpDown_Size_Width.Value;
                int height = (int)numericUpDown_Size_Height.Value;
                compList.Resize(width, height);
                MaxHeightTrackBar.Minimum = compList.zMin;
                MaxHeightTrackBar.Maximum = compList.zMax;
                MaxHeightTrackBar.Value = compList.zMax;
                numericUpDown_Selected_X.Maximum = compList.Width - 1;
                numericUpDown_Selected_Y.Maximum = compList.Height - 1;
                ScrollbarsSetValue();
                ForceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        /// <summary>
        /// Save Button clicked
        /// </summary>
        private void BTN_Save_Click(object sender, EventArgs e)
        {
            if (compList != null)
            {
                int id;
                if (FiddlerControls.Utils.ConvertStringToInt(textBox_SaveToID.Text, out id, 0, 0x1FFF))
                    compList.AddToSDKComponentList(id); //fires MultiChangeEvent
            }
        }

        /// <summary>
        /// Select Button activate
        /// </summary>
        private void BTN_Select_Click(object sender, EventArgs e)
        {
            BTN_Select.Checked = true;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = false;
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void BTN_Toolbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox thisBox = (CheckBox)sender;
            switch (thisBox.Name)
            {
                case "BTN_Select": thisBox.ImageKey = (thisBox.Checked) ? "SelectButton_Selected.bmp" : "SelectButton.bmp"; break;
                case "BTN_Draw": thisBox.ImageKey = (thisBox.Checked) ? "DrawButton_Selected.bmp" : "DrawButton.bmp"; break;
                case "BTN_Remove": thisBox.ImageKey = (thisBox.Checked) ? "RemoveButton_Selected.bmp" : "RemoveButton.bmp"; break;
                case "BTN_Z": thisBox.ImageKey = (thisBox.Checked) ? "AltitudeButton_Selected.bmp" : "AltitudeButton.bmp"; break;
                case "BTN_Floor": thisBox.ImageKey = (thisBox.Checked) ? "VirtualFloorButton_Selected.bmp" : "VirtualFloorButton.bmp"; break;
                case "BTN_Pipette": thisBox.ImageKey = (thisBox.Checked) ? "PipetteButton_Selected.bmp" : "PipetteButton.bmp"; break;
                case "BTN_Trans": thisBox.ImageKey = (thisBox.Checked) ? "TransButton_Selected.bmp" : "TransButton.bmp"; break;
            }
        }

        /// <summary>
        /// Z Button activate
        /// </summary>
        private void BTN_Z_Click(object sender, EventArgs e)
        {
            BTN_Select.Checked = false;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = true;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = false;
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void BTN_Trans_Clicked(object sender, EventArgs e)
        {
            BTN_Select.Checked = false;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = true;
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Converts pictureBox coords to Multicoords
        /// </summary>
        private void ConvertCoords(Point point, out int x, out int y, out int z)
        {
            //first check if current Tile matches
            if (HoverTile != null)
            {
                //visible?
                if ((!BTN_Floor.Checked) || (HoverTile.Z >= DrawFloorZ))
                {
                    x = HoverTile.X;
                    y = HoverTile.Y;
                    z = HoverTile.Z + HoverTile.Height;
                    return;
                }
            }
            //damn the hard way
            z = 0;

            int cx = 0; //Get MouseCoords for (0/0)
            int cy = 0;
            if (BTN_Floor.Checked)
            {
                cy -= DrawFloorZ * 4;
                z = DrawFloorZ;
            }
            cy -= 44;
            cx -= compList.xMin;
            cy -= compList.yMin;
            cy += MultiEditorComponentList.GapYMod; //Mod for a bit of gap
            cx += MultiEditorComponentList.GapXMod;

            double mx = point.X - cx;
            double my = point.Y - cy;
            double xx = mx;
            double yy = my;
            my = xx * CoordinateTransform - yy * CoordinateTransform; //Rotate 45° Coordinate system
            mx = yy * CoordinateTransform + xx * CoordinateTransform;
            mx /= CoordinateTransform * 44; //Math.Sqrt(2)*22==CoordinateTransform*44
            my /= CoordinateTransform * 44; //CoordinateTransform=Math.Sqrt(2)/2
            my *= -1;
            x = (int)mx;
            y = (int)my;
        }

        /// <summary>
        /// Value of TrackBar changed (for displayed MaxHeight)
        /// </summary>
        private void MaxHeightTrackBarOnValueChanged(object sender, EventArgs e)
        {
            ScrollbarsSetValue();
            toolTip1.SetToolTip(MaxHeightTrackBar, MaxHeightTrackBar.Value.ToString());
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Virtual Floor zValue changed
        /// </summary>
        private void numericUpDown_Floor_Changed(object sender, EventArgs e)
        {
            m_DrawFloorZ = (int)numericUpDown_Floor.Value;
            compList.SetFloorZ(m_DrawFloorZ);
            if (BTN_Floor.Checked)
            {
                ScrollbarsSetValue();
                ForceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        /// <summary>
        /// SelectedTile panel X value changed 
        /// </summary>
        private void numericUpDown_Selected_X_Changed(object sender, EventArgs e)
        {
            if (compList != null)
            {
                if (SelectedTile != null)
                {
                    if ((int)numericUpDown_Selected_X.Value != SelectedTile.X)
                    {
                        compList.TileMove(SelectedTile, (int)numericUpDown_Selected_X.Value, SelectedTile.Y);
                        ForceRefresh = true;
                        pictureBoxMulti.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// SelectedTile panel Y value changed 
        /// </summary>
        private void numericUpDown_Selected_Y_Changed(object sender, EventArgs e)
        {
            if (compList != null)
            {
                if (SelectedTile != null)
                {
                    if ((int)numericUpDown_Selected_Y.Value != SelectedTile.Y)
                    {
                        compList.TileMove(SelectedTile, SelectedTile.X, (int)numericUpDown_Selected_Y.Value);
                        ForceRefresh = true;
                        pictureBoxMulti.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// SelectedTile panel Z value changed 
        /// </summary>
        private void numericUpDown_Selected_Z_Changed(object sender, EventArgs e)
        {
            if (compList != null)
            {
                if (SelectedTile != null)
                {
                    if ((int)numericUpDown_Selected_Z.Value != SelectedTile.Z)
                    {
                        compList.TileZSet(SelectedTile, (int)numericUpDown_Selected_Z.Value);
                        MaxHeightTrackBar.Minimum = compList.zMin;
                        MaxHeightTrackBar.Maximum = compList.zMax;
                        if (MaxHeightTrackBar.Value < SelectedTile.Z)
                            MaxHeightTrackBar.Value = SelectedTile.Z;
                        ForceRefresh = true;
                        pictureBoxMulti.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Event Ultima FilePathes changed
        /// </summary>
        private void OnFilePathChangeEvent()
        {
            if (Loaded)
                OnLoad(null, null);
        }

        /// <summary>
        /// Load of Usercontrol
        /// </summary>
        private void OnLoad(object sender, EventArgs e)
        {
            FiddlerControls.Options.LoadedUltimaClass["TileData"] = true;
            FiddlerControls.Options.LoadedUltimaClass["Art"] = true;
            FiddlerControls.Options.LoadedUltimaClass["Multis"] = true;
            FiddlerControls.Options.LoadedUltimaClass["Hues"] = true;
            XML_InitializeToolBox();
            string path = FiddlerControls.Options.AppDataPath;

            string FileName = Path.Combine(path, "Multilist.xml");
            XmlDocument dom = null;
            XmlElement xMultis = null;
            if ((File.Exists(FileName)))
            {
                dom = new XmlDocument();
                dom.Load(FileName);
                xMultis = dom["Multis"];
            }
            treeViewMultiList.BeginUpdate();
            treeViewMultiList.Nodes.Clear();
            // Let's create a root for import from Multi file and put these in there
            TreeNode multinode = new TreeNode("Multi.mul");
            for (int i = 0; i < 0x2000; ++i)
            {
                if (Ultima.Multis.GetComponents(i) != MultiComponentList.Empty)
                {
                    TreeNode node;
                    if (dom == null)
                    {
                        node = new TreeNode(String.Format("{0,5} (0x{0:X})", i));
                    }
                    else
                    {
                        XmlNodeList xMultiNodeList = xMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                        string j = "";
                        foreach (XmlNode xMultiNode in xMultiNodeList)
                        {
                            j = xMultiNode.Attributes["name"].Value;
                        }
                        node = new TreeNode(String.Format("{0,5} (0x{0:X}) {1}", i, j));
                    }
                    node.Tag = i;
                    node.Name = i.ToString();
                    multinode.Nodes.Add(node);
                }
            }
            treeViewMultiList.Nodes.Add(multinode);
            TreeNode filenode = new TreeNode("From File");
            TreeNode txtnode = new TreeNode("Txt File");
            txtnode.Tag = "txt";
            filenode.Nodes.Add(txtnode);

            TreeNode uoanode = new TreeNode("UOA File");
            uoanode.Tag = "uoa";
            filenode.Nodes.Add(uoanode);

            TreeNode uoabnode = new TreeNode("UOA Binary File");
            uoabnode.Tag = "uoab";
            filenode.Nodes.Add(uoabnode);

            TreeNode wscnode = new TreeNode("WSC File");
            wscnode.Tag = "wsc";
            filenode.Nodes.Add(wscnode);

            TreeNode cachenode = new TreeNode("MultiCache File"); //LoadFromFile fixed
            cachenode.Tag = "cache";
            filenode.Nodes.Add(cachenode);

            TreeNode uoadesignnode = new TreeNode("UOA Design File"); //LoadFromFile fixed
            uoadesignnode.Tag = "uoadesign";
            filenode.Nodes.Add(uoadesignnode);

            treeViewMultiList.Nodes.Add(filenode);
            treeViewMultiList.EndUpdate();
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.MultiChangeEvent += new FiddlerControls.Events.MultiChangeHandler(OnMultiChangeEvent);
            }

            Loaded = true;
        }

        private void OnMultiChangeEvent(object sender, int id)
        {
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;
            bool done = false;
            bool remove = (Ultima.Multis.GetComponents(id) == MultiComponentList.Empty);
            for (int i = 0; i < treeViewMultiList.Nodes[0].Nodes.Count; ++i)
            {
                if (id == (int)treeViewMultiList.Nodes[0].Nodes[i].Tag)
                {
                    done = true;
                    if (remove)
                        treeViewMultiList.Nodes[0].Nodes.RemoveAt(i);
                    break;
                }
                else if (id < (int)treeViewMultiList.Nodes[0].Nodes[i].Tag)
                {
                    if (!remove)
                    {
                        TreeNode node = new TreeNode(String.Format("{0,5} (0x{0:X})", id));
                        node.Tag = id;
                        node.Name = id.ToString();
                        treeViewMultiList.Nodes[0].Nodes.Insert(i, node);
                    }
                    done = true;
                    break;
                }
            }
            if ((!remove) && (!done))
            {
                TreeNode node = new TreeNode(String.Format("{0,5} (0x{0:X})", id));
                node.Tag = id;
                node.Name = id.ToString();
                treeViewMultiList.Nodes[0].Nodes.Add(node);
            }
        }

        /// <summary>
        /// Hover effect
        /// </summary>
        private void PictureBoxMultiOnMouseMove(object sender, MouseEventArgs e)
        {
            MouseLoc = e.Location;
            MouseLoc.X += hScrollBar.Value;
            MouseLoc.Y += vScrollBar.Value;
            if (moving)
            {
                int deltax = (int)(-1 * (e.X - MovingLoc.X));
                int deltay = (int)(-1 * (e.Y - MovingLoc.Y));
                MovingLoc = e.Location;
                hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltax));
                vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltay));
            }
            pictureBoxMulti.Invalidate();
        }

        private void PictureBoxMultiOnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                moving = true;
                MovingLoc = e.Location;
                this.Cursor = Cursors.Hand;
            }
            else
            {
                moving = false;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Select/Draw/Remove/Z a Tile
        /// </summary>
        private void PictureBoxMultiOnMouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
            this.Cursor = Cursors.Default;
            if (e.Button == MouseButtons.Right)
                return;
            if (compList == null)
                return;
            if (e.Button == MouseButtons.Middle)
            {
                OverlayList.Clear();
                if (m_HoverTile != null)
                {
                    foreach (MultiTile tile in compList.GetXYArray(m_HoverTile.X, m_HoverTile.Y))
                    {
                        if (tile.isVirtualFloor)
                            continue;
                        if (tile.Z == m_HoverTile.Z)
                            OverlayList.Add(tile);
                    }
                }
            }
            else if (BTN_Select.Checked)
            {
                SelectedTile = m_HoverTile;
            }
            else if (BTN_Draw.Checked)
            {
                if (m_DrawTile.ID >= 0)
                {
                    int x, y, z;
                    ConvertCoords(MouseLoc, out x, out y, out z);
                    if ((x >= 0) && (x < compList.Width) && (y >= 0) && (y < compList.Height))
                    {
                        compList.TileAdd(x, y, z, m_DrawTile.ID);
                        MaxHeightTrackBar.Minimum = compList.zMin;
                        MaxHeightTrackBar.Maximum = compList.zMax;
                        if (MaxHeightTrackBar.Value < z)
                            MaxHeightTrackBar.Value = z;
                    }
                }
            }
            else if (BTN_Remove.Checked)
            {
                if (m_HoverTile != null)
                    compList.TileRemove(m_HoverTile);
                else
                {
                    int overx = 0, overy = 0;
                    foreach (MultiTile tile in OverlayList)
                    {
                        Bitmap bmp = tile.GetBitmap();
                        if (bmp == null)
                            continue;

                        if (((MouseLoc.X > overx) && (MouseLoc.X < (overx + bmp.Width))) &&
                            ((MouseLoc.Y > overy) && (MouseLoc.Y < (overy + bmp.Height))))
                        {
                            //Check for transparent part
                            Color p = bmp.GetPixel(MouseLoc.X - overx, MouseLoc.Y - overy);
                            if (!((p.R == 0) && (p.G == 0) && (p.B == 0)))
                            {
                                compList.TileRemove(tile);
                                OverlayList.Remove(tile);
                                break;
                            }
                        }
                        overx += bmp.Width + 10;
                    }
                }
                MaxHeightTrackBar.Minimum = compList.zMin;
                MaxHeightTrackBar.Maximum = compList.zMax;
            }
            else if (BTN_Z.Checked)
            {
                if (m_HoverTile != null)
                {
                    int z = (int)numericUpDown_Z.Value;
                    compList.TileZMod(m_HoverTile, z);
                    MaxHeightTrackBar.Minimum = compList.zMin;
                    MaxHeightTrackBar.Maximum = compList.zMax;
                    if (MaxHeightTrackBar.Value < m_HoverTile.Z)
                        MaxHeightTrackBar.Value = m_HoverTile.Z;
                }
            }
            else if (BTN_Pipette.Checked)
            {
                if (m_HoverTile != null)
                {
                    m_DrawTile.Set(m_HoverTile.ID, 0);
                    pictureBoxDrawTiles_Select();
                    DrawTileLabel.Text = String.Format("Draw ID: 0x{0:X}", m_HoverTile.ID);
                }
            }
            else if (BTN_Trans.Checked)
            {
                if (m_HoverTile != null)
                    m_HoverTile.Transparent = !m_HoverTile.Transparent;
            }

            if ((e.Button != MouseButtons.Middle) && (!BTN_Remove.Checked))
                OverlayList.Clear();
            else if (OverlayList.Count == 1)
                OverlayList.Clear();

            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        delegate void aDelegate(string t);
        private void SetToolStripText(string text)
        {
            toolStripLabelCoord.Text = text;
            toolStrip1.Update();
        }

        /// <summary>
        /// Draw Image
        /// </summary>
        private void PictureBoxMultiOnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            if (compList != null)
            {
                compList.GetImage(e.Graphics, hScrollBar.Value, vScrollBar.Value, MaxHeightTrackBar.Value, MouseLoc, BTN_Floor.Checked, ForceRefresh);
                ForceRefresh = false;

                if (ShowWalkables)
                    showWalkablesToolStripMenuItem.Text = String.Format("Show Walkable tiles ({0})", compList.WalkableCount);
                if (ShowDoubleSurface)
                    showDoubleSurfaceMenuItem.Text = String.Format("Show double surface ({0})", compList.DoubleSurfaceCount);

                int x, y, z;
                ConvertCoords(MouseLoc, out x, out y, out z);
                if ((x >= 0) && (x < compList.Width) && (y >= 0) && (y < compList.Height))
                {
                     base.Invoke(new aDelegate(SetToolStripText),String.Format("{0},{1},{2}", x, y, z) );
                }

                if (BTN_Draw.Checked)
                {
                    if (m_DrawTile.ID >= 0)
                    {
                        if ((x >= 0) && (x < compList.Width) && (y >= 0) && (y < compList.Height))
                        {
                            this.Invoke(new aDelegate(SetToolStripText), String.Format("{0},{1},{2}", x, y, z) );
                            Bitmap bmp = m_DrawTile.GetBitmap();
                            if (bmp == null)
                                return;
                            int px = (x - y) * 22;
                            int py = (x + y) * 22;

                            px -= (bmp.Width / 2);
                            py -= z * 4;
                            py -= bmp.Height;
                            px -= compList.xMin;
                            py -= compList.yMin;
                            py += MultiEditorComponentList.GapYMod; //Mod for a bit of gap
                            px += MultiEditorComponentList.GapXMod;
                            px -= hScrollBar.Value;
                            py -= vScrollBar.Value;
                            e.Graphics.DrawImage(bmp, new Rectangle(px, py, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, MultiTile.DrawColor);
                        }
                    }
                }
                else if (OverlayList.Count > 0)
                {
                    int overx = 0, overy = 0;
                    foreach (MultiTile tile in OverlayList)
                    {
                        Bitmap bmp = tile.GetBitmap();
                        if (bmp == null)
                            continue;
                        e.Graphics.DrawImage(bmp, new Rectangle(overx, overy, bmp.Width, bmp.Height));
                        overx += bmp.Width + 10;
                    }
                }
            }
        }

        /// <summary>
        /// PictureBox size changed
        /// </summary>
        private void PictureBoxMultiOnResize(object sender, EventArgs e)
        {
            ScrollbarsSetValue();
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Does the Multi fit inside the PictureBox
        /// </summary>
        private void ScrollbarsSetValue()
        {
            if (compList == null)
                return;
            int yMin = compList.yMinOrg;
            int yMax = compList.yMaxOrg;

            if (BTN_Floor.Checked)
            {
                int floorzmod = -DrawFloorZ * 4 - 44;
                if (yMin > floorzmod)
                    yMin = floorzmod;
                floorzmod = (compList.Width + compList.Height) * 22 - DrawFloorZ * 4;
                if (yMax < floorzmod)
                    yMax = floorzmod;
            }
            int height = yMax - yMin + MultiEditorComponentList.GapYMod * 3;
            int width = compList.xMax - compList.xMin + MultiEditorComponentList.GapXMod * 2;

            if (height <= pictureBoxMulti.Height + hScrollBar.Height)
            {
                vScrollBar.Enabled = false;
                vScrollBar.Value = 0;
            }
            else
            {
                vScrollBar.Enabled = true;
                vScrollBar.Maximum = height - pictureBoxMulti.Height + 10;
            }
            if (width <= pictureBoxMulti.Width + vScrollBar.Width)
            {
                hScrollBar.Enabled = false;
                vScrollBar.Value = 0;
            }
            else
            {
                hScrollBar.Enabled = true;
                hScrollBar.Maximum = width - pictureBoxMulti.Width + 10;
            }
        }

        /// <summary>
        /// Scrollbars changed
        /// </summary>
        private void ScrollBarsValueChanged(object sender, EventArgs e)
        {
            ForceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void treeViewMultiList_LoadFromFile(Multis.ImportType importtype)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            string type = "";
            switch (importtype)
            {
                case Multis.ImportType.TXT: type = "txt"; break;
                case Multis.ImportType.UOA: type = "uoa"; break;
                case Multis.ImportType.UOAB: type = "uoab"; break;
                case Multis.ImportType.WSC: type = "wsc"; break;
                case Multis.ImportType.MULTICACHE: type = "Multicache.dat"; break;
                case Multis.ImportType.UOADESIGN: type = "Designs"; break;
                default: return;
            }
            dialog.Title = String.Format("Choose {0} file to import", type);
            dialog.CheckFileExists = true;
            if (importtype == Multis.ImportType.MULTICACHE)
                dialog.Filter = String.Format("{0} file ({0})|{0}", type);
            else if (importtype == Multis.ImportType.UOADESIGN)
                dialog.Filter = String.Format("{0} file ({0}.*)|{0}.*", type);
            else if (importtype == Multis.ImportType.UOAB)
                dialog.Filter = String.Format("{0} file (*.{0})|*.{0}", "uoa");
            else
                dialog.Filter = String.Format("{0} file (*.{0})|*.{0}", type);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (importtype == Multis.ImportType.MULTICACHE)
                {
                    List<MultiComponentList> list = Ultima.Multis.LoadFromCache(dialog.FileName);
                    TreeNode node = treeViewMultiList.Nodes[1].Nodes[4];
                    node.Nodes.Clear();
                    for (int i = 0; i < list.Count; ++i)
                    {
                        TreeNode child = new TreeNode("Entry " + i);
                        child.Tag = list[i];
                        node.Nodes.Add(child);
                    }
                }
                else if (importtype == Multis.ImportType.UOADESIGN)
                {
                    List<Object[]> list = Ultima.Multis.LoadFromDesigner(dialog.FileName);
                    TreeNode node = treeViewMultiList.Nodes[1].Nodes[5];
                    node.Nodes.Clear();
                    for (int i = 0; i < list.Count; ++i)
                    {
                        Object[] data = list[i];
                        TreeNode child = new TreeNode(data[0] + "(" + i + ")");
                        child.Tag = data[1];
                        node.Nodes.Add(child);
                    }
                }
                else
                {
                    MultiComponentList multi = Ultima.Multis.LoadFromFile(dialog.FileName, importtype);
                    compList = new MultiEditorComponentList(multi, this);
                    UndoList_Clear();
                    MaxHeightTrackBar.Minimum = compList.zMin;
                    MaxHeightTrackBar.Maximum = compList.zMax;
                    MaxHeightTrackBar.Value = compList.zMax;
                    textBox_SaveToID.Text = "0";
                    numericUpDown_Size_Width.Value = compList.Width;
                    numericUpDown_Size_Height.Value = compList.Height;
                    numericUpDown_Selected_X.Maximum = compList.Width - 1;
                    numericUpDown_Selected_Y.Maximum = compList.Height - 1;
                    vScrollBar.Value = 0;
                    hScrollBar.Value = 0;
                    ScrollbarsSetValue();
                    ForceRefresh = true;
                    pictureBoxMulti.Invalidate();
                }
            }
            dialog.Dispose();
        }

        /// <summary>
        /// Doubleclick Node of Import treeview
        /// </summary>
        private void treeViewMultiList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null)
                return;

            switch (e.Node.Tag.ToString())
            {
                case "txt": treeViewMultiList_LoadFromFile(Multis.ImportType.TXT); return;
                case "uoa": treeViewMultiList_LoadFromFile(Multis.ImportType.UOA); return;
                case "uoab": treeViewMultiList_LoadFromFile(Multis.ImportType.UOAB); return;
                case "wsc": treeViewMultiList_LoadFromFile(Multis.ImportType.WSC); return;
                case "cache": treeViewMultiList_LoadFromFile(Multis.ImportType.MULTICACHE); return;
                case "uoadesign": treeViewMultiList_LoadFromFile(Multis.ImportType.UOADESIGN); return;
                default: break;
            }

            if (MessageBox.Show("Do you want to open selected Multi?", "Open", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                if ((e.Node.Parent != null) && (e.Node.Parent.Tag != null) && (e.Node.Parent.Tag.ToString() == "cache"))
                {
                    MultiComponentList list = (MultiComponentList)e.Node.Tag;
                    if (list != null)
                    {
                        compList = new MultiEditorComponentList(list, this);
                        textBox_SaveToID.Text = "0";
                    }
                }
                else if ((e.Node.Parent != null) && (e.Node.Parent.Tag != null) && (e.Node.Parent.Tag.ToString() == "uoadesign"))
                {
                    MultiComponentList list = (MultiComponentList)e.Node.Tag;
                    if (list != null)
                    {
                        compList = new MultiEditorComponentList(list, this);
                        textBox_SaveToID.Text = "0";
                    }
                }
                else
                {
                    compList = new MultiEditorComponentList(Ultima.Multis.GetComponents((int)e.Node.Tag), this);
                    textBox_SaveToID.Text = e.Node.Tag.ToString();
                }
                UndoList_Clear();
                MaxHeightTrackBar.Minimum = compList.zMin;
                MaxHeightTrackBar.Maximum = compList.zMax;
                MaxHeightTrackBar.Value = compList.zMax;
                numericUpDown_Size_Width.Value = compList.Width;
                numericUpDown_Size_Height.Value = compList.Height;
                numericUpDown_Selected_X.Maximum = compList.Width - 1;
                numericUpDown_Selected_Y.Maximum = compList.Height - 1;
                vScrollBar.Value = 0;
                hScrollBar.Value = 0;
                ScrollbarsSetValue();
                ForceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        private void treeViewMultiList_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            if (e.Node.Tag is int)
            {
                MultiComponentList list = Ultima.Multis.GetComponents((int)e.Node.Tag);
                toolTip1.SetToolTip(treeViewMultiList, String.Format("{0}x{1} {2}", list.Width, list.Height, list.SortedTiles.Length));
            }
            else if (e.Node.Tag is MultiComponentList)
            {
                MultiComponentList list = e.Node.Tag as MultiComponentList;
                toolTip1.SetToolTip(treeViewMultiList, String.Format("{0}x{1} {2}", list.Width, list.Height, list.SortedTiles.Length));
            }

        }

        private void TreeViewTilesXML_OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                DrawTilesList = (List<int>)e.Node.Tag;
                vScrollBarDrawTiles.Maximum = DrawTilesList.Count / pictureboxDrawTilescol + 1;
                vScrollBarDrawTiles.Minimum = 1;
                vScrollBarDrawTiles.SmallChange = 1;
                vScrollBarDrawTiles.LargeChange = 1;
                vScrollBarDrawTiles.Value = 1;
                pictureBoxDrawTiles.Invalidate();
            }
        }

        private void Undo_onClick(object sender, EventArgs e)
        {
            if (compList != null)
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                int undo = (int)item.Tag;
                compList.Undo(undo);
                MaxHeightTrackBar.Minimum = compList.zMin;
                MaxHeightTrackBar.Maximum = compList.zMax;
                MaxHeightTrackBar.Value = compList.zMax;
                numericUpDown_Size_Width.Value = compList.Width;
                numericUpDown_Size_Height.Value = compList.Height;
                numericUpDown_Selected_X.Maximum = compList.Width - 1;
                numericUpDown_Selected_Y.Maximum = compList.Height - 1;
                ScrollbarsSetValue();
                ForceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        private void UndoList_BeforeOpening(object sender, EventArgs e)
        {
            if (compList != null)
            {
                foreach (ToolStripItem item in UndoItems.DropDownItems)
                {
                    int index = (int)item.Tag;
                    if (compList.UndoList[index].Tiles != null)
                        item.Text = compList.UndoList[index].Action;
                    else
                        item.Text = "---";
                }
            }
        }

        private void UndoList_Clear()
        {
            if (compList != null)
                compList.UndoClear();
            foreach (ToolStripItem item in UndoItems.DropDownItems)
            {
                item.Text = "---";
            }
        }

        private void XML_AddChildren(TreeNode node, XmlElement mainNode)
        {
            foreach (XmlElement e in mainNode)
            {
                TreeNode tempNode = new TreeNode();

                tempNode.Text = e.GetAttribute("name");
                if (e.Name == "subgroup")
                {
                    tempNode.ImageIndex = 0;
                    if (e.HasChildNodes)
                    {
                        List<int> list = new List<int>();
                        foreach (XmlElement elem in e.ChildNodes)
                        {
                            int i = Int32.Parse(elem.GetAttribute("index"));
                            if (Ultima.Art.IsValidStatic(i))
                                list.Add(i);
                        }
                        tempNode.Tag = list;
                    }
                }

                node.Nodes.Add(tempNode);
            }
        }

        private void XML_InitializeToolBox()
        {
            string path = FiddlerControls.Options.AppDataPath;
            string FileName = Path.Combine(path, @"plugins/multieditor.xml");
            if (!File.Exists(FileName))
                return;

            XmlDocument dom = new XmlDocument();
            dom.Load(FileName);
            XmlElement xTiles = dom["TileGroups"];

            foreach (XmlElement xRootGroup in xTiles)
            {
                TreeNode mainNode = new TreeNode();
                mainNode.Text = xRootGroup.GetAttribute("name");
                mainNode.Tag = null;

                mainNode.ImageIndex = 0;

                XML_AddChildren(mainNode, xRootGroup);

                treeViewTilesXML.Nodes.Add(mainNode);
            }
        }

        #endregion Methods

        public void SelectDrawTile(ushort id)
        {
            m_DrawTile.Set(id, 0);
            pictureBoxDrawTiles_Select();
            DrawTileLabel.Text = String.Format("Draw ID: 0x{0:X}", id);
        }

        #region DrawTilesPictureBox Stuff
        private int GetIndex(int x, int y)
        {
            if (x >= pictureboxDrawTilescol)
                return -1;
            int value = Math.Max(0, ((pictureboxDrawTilescol * (vScrollBarDrawTiles.Value - 1)) + (x + (y * pictureboxDrawTilescol))));
            if (DrawTilesList.Count > value)
                return DrawTilesList[value];
            else
                return -1;
        }

        private void pictureBoxDrawTiles_OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBoxDrawTiles.Focus();
            int x = e.X / (DrawTileSizeWidth - 1);
            int y = e.Y / (DrawTileSizeHeight - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
            {
                m_DrawTile.Set((ushort)index, 0);
                DrawTileLabel.Text = String.Format("Draw ID: 0x{0:X}", index);
                pictureBoxDrawTiles.Invalidate();
            }
        }

        private void pictureBoxDrawTilesMouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X / (DrawTileSizeWidth - 1);
            int y = e.Y / (DrawTileSizeHeight - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
            {
                if (index != (int)FloatingPreviewPanel.Tag)
                {
                    FloatingPreviewPanel.BackgroundImage = Ultima.Art.GetStatic(index);
                    FloatingPreviewPanel.Size = new Size(Ultima.Art.GetStatic(index).Width + 10, Ultima.Art.GetStatic(index).Height + 10);
                }
                FloatingPreviewPanel.Left = PointToClient(MousePosition).X;
                FloatingPreviewPanel.Top = PointToClient(MousePosition).Y - FloatingPreviewPanel.Size.Height;
                FloatingPreviewPanel.Visible = true;
                FloatingPreviewPanel.Tag = index;
                toolTip1.SetToolTip(pictureBoxDrawTiles, String.Format("0x{0:X} ({0})", index));
                pictureBoxDrawTiles.Invalidate();
            }
            else
            {
                FloatingPreviewPanel.Visible = false;
                toolTip1.SetToolTip(pictureBoxDrawTiles, String.Empty);
            }
        }

        private void pictureBoxDrawTilesMouseLeave(object sender, EventArgs e)
        {
            FloatingPreviewPanel.Visible = false;
        }

        private void pictureBoxDrawTiles_OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int y = 0; y < pictureboxDrawTilesrow; ++y)
            {
                for (int x = 0; x < pictureboxDrawTilescol; ++x)
                {
                    int index = GetIndex(x, y);
                    if (index >= 0)
                    {
                        Bitmap b = Art.GetStatic(index);

                        if (b != null)
                        {
                            Point loc = new Point((x * DrawTileSizeWidth) + 1, (y * DrawTileSizeHeight) + 1);
                            Size size = new Size(DrawTileSizeWidth - 1, DrawTileSizeHeight - 1);
                            Rectangle rect = new Rectangle(loc, size);

                            e.Graphics.Clip = new Region(rect);

                            if (index == m_DrawTile.ID)
                                e.Graphics.FillRectangle(Brushes.LightBlue, rect);

                            int width = b.Width;
                            int height = b.Height;
                            if (width > size.Width)
                            {
                                width = size.Width;
                                height = size.Height * b.Height / b.Width;
                            }
                            if (height > size.Height)
                            {
                                height = size.Height;
                                width = size.Width * b.Width / b.Height;
                            }
                            e.Graphics.DrawImage(b, new Rectangle(loc, new Size(width, height)));
                        }
                    }
                }
            }
        }

        private void pictureBoxDrawTiles_OnResize(object sender, EventArgs e)
        {
            if ((pictureBoxDrawTiles.Height == 0) || (pictureBoxDrawTiles.Width == 0))
                return;
            pictureboxDrawTilescol = pictureBoxDrawTiles.Width / DrawTileSizeWidth;
            pictureboxDrawTilesrow = pictureBoxDrawTiles.Height / DrawTileSizeHeight + 1;
            vScrollBarDrawTiles.Maximum = DrawTilesList.Count / pictureboxDrawTilescol + 1;
            vScrollBarDrawTiles.Minimum = 1;
            vScrollBarDrawTiles.SmallChange = 1;
            vScrollBarDrawTiles.LargeChange = pictureboxDrawTilesrow;
            pictureBoxDrawTiles.Invalidate();
        }

        private void vScrollBarDrawTiles_Scroll(object sender, ScrollEventArgs e)
        {
            pictureBoxDrawTiles.Invalidate();
        }

        private void pictureBoxDrawTiles_OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBarDrawTiles.Value < vScrollBarDrawTiles.Maximum)
                {
                    vScrollBarDrawTiles.Value++;
                    pictureBoxDrawTiles.Invalidate();
                }
            }
            else
            {
                if (vScrollBarDrawTiles.Value > 1)
                {
                    vScrollBarDrawTiles.Value--;
                    pictureBoxDrawTiles.Invalidate();
                }
            }
        }

        private void pictureBoxDrawTiles_Select()
        {
            foreach (TreeNode node in treeViewTilesXML.Nodes)
            {
                foreach (TreeNode childnode in node.Nodes)
                {
                    if (childnode.Tag != null)
                    {
                        foreach (int index in (List<int>)childnode.Tag)
                        {
                            if (index == m_DrawTile.ID)
                            {
                                treeViewTilesXML.SelectedNode = childnode;
                                pictureBoxDrawTiles.Invalidate();
                                return;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        private void BTN_DynamicCheckBox_Changed(object sender, EventArgs e)
        {
            if (compList != null)
            {
                if (SelectedTile != null)
                {
                    if (SelectedTile.Invisible != DynamiccheckBox.Checked)
                        SelectedTile.Invisible = DynamiccheckBox.Checked;
                }
            }
        }

        private void BTN_ShowWalkables_Click(object sender, EventArgs e)
        {
            showWalkablesToolStripMenuItem.Text = "Show Walkable tiles";
            if (ShowWalkables)
            {
                if (compList != null)
                {
                    compList.CalcWalkable();
                    ForceRefresh = true;
                    pictureBoxMulti.Invalidate();
                }
            }
        }

        private void BTN_ShowAllTrans(object sender, EventArgs e)
        {
            if (compList != null)
            {
                for (int i = 0; i < compList.Tiles.Count; ++i)
                {
                    compList.Tiles[i].Transparent = false;
                }
                ForceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        private void BTN_ShowDoubleSurface(object sender, EventArgs e)
        {
            showDoubleSurfaceMenuItem.Text = "Show double surface";
            if (ShowDoubleSurface)
            {
                if (compList != null)
                {
                    compList.CalcDoubleSurface();
                    ForceRefresh = true;
                    pictureBoxMulti.Invalidate();
                }
            }
        }

        private void OnDummyContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }


    }
}
