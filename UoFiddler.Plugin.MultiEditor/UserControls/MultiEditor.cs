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
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.UserControls
{
    public partial class MultiEditor : UserControl
    {
        private MultiEditorComponentList _compList;

        //Sin/Cos(45°)
        private static readonly double CoordinateTransform = Math.Sqrt(2) / 2;
        private const int DrawTileSizeHeight = 45;
        private const int DrawTileSizeWidth = 45;
        private List<int> _drawTilesList = new List<int>();
        private readonly List<MultiTile> _overlayList = new List<MultiTile>();
        private bool _loaded;
        private bool _moving;
        private readonly MultiTile _mDrawTile;
        private MultiTile _mHoverTile;
        private MultiTile _mSelectedTile;

        /// <summary>
        /// Current MouseLoc + Scrollbar values (for hover effect)
        /// </summary>
        private Point _mouseLoc;

        private Point _movingLoc;
        private int _pictureBoxDrawTilesCol;
        private int _pictureBoxDrawTilesRow;
        private bool _forceRefresh; // TODO: unused?

        public MultiEditor()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint,
                true);
            InitializeComponent();
            _mouseLoc = new Point();
            _movingLoc = new Point();
            _mDrawTile = new MultiTile();
            Selectedpanel.Visible = false;
            FloatingPreviewPanel.Visible = false;
            FloatingPreviewPanel.Tag = -1;
            BTN_Select.Checked = true;
            pictureBoxDrawTiles.MouseWheel += PictureBoxDrawTiles_OnMouseWheel;
            pictureBoxMulti.ContextMenuStrip = null;
            _forceRefresh = true;
        }

        /// <summary>
        /// Floor Z level
        /// </summary>
        public int DrawFloorZ { get; private set; }

        /// <summary>
        /// Current Hovered Tile (set inside MultiComponentList)
        /// </summary>
        public MultiTile HoverTile
        {
            get => _mHoverTile;
            set
            {
                _mHoverTile = value;
                toolTip1.SetToolTip(pictureBoxMulti, value != null ? $"ID: 0x{_mHoverTile.Id:X} Z: {_mHoverTile.Z}" : string.Empty);
            }
        }

        /// <summary>
        /// Current Selected Tile (set OnMouseUp)
        /// </summary>
        public MultiTile SelectedTile
        {
            get => _mSelectedTile;
            private set
            {
                _mSelectedTile = value;
                if (value != null)
                {
                    SelectedTileLabel.Text = $"ID: 0x{value.Id:X} Z: {value.Z}";
                    numericUpDown_Selected_X.Value = value.X;
                    numericUpDown_Selected_Y.Value = value.Y;
                    numericUpDown_Selected_Z.Value = value.Z;
                    DynamiccheckBox.Checked = value.Invisible;
                }
                else
                {
                    SelectedTileLabel.Text = "ID:";
                }
            }
        }

        public bool ShowWalkables => showWalkablesToolStripMenuItem.Checked;
        public bool ShowDoubleSurface => showDoubleSurfaceMenuItem.Checked;

        // Private Methods (35) 

        /// <summary>
        /// Creates new blank Multi
        /// </summary>
        private void BTN_CreateBlank_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to create a blank Multi?", "Create", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) != DialogResult.OK)
            {
                return;
            }

            int width = (int)numericUpDown_Size_Width.Value;
            int height = (int)numericUpDown_Size_Height.Value;
            _compList = new MultiEditorComponentList(width, height, this);
            UndoList_Clear();
            MaxHeightTrackBar.Minimum = _compList.ZMin;
            MaxHeightTrackBar.Maximum = _compList.ZMax;
            MaxHeightTrackBar.Value = _compList.ZMax;
            numericUpDown_Selected_X.Maximum = _compList.Width - 1;
            numericUpDown_Selected_Y.Maximum = _compList.Height - 1;
            ScrollbarsSetValue();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
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
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void BTN_Export_TXT_OnClick(object sender, EventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"{textBox_Export.Text}.txt");
            MultiComponentList sdkList = _compList.ConvertToSdk();
            sdkList.ExportToTextFile(fileName);
        }

        private void BTN_Export_UOA_OnClick(object sender, EventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"{textBox_Export.Text}.uoa");
            MultiComponentList sdkList = _compList.ConvertToSdk();
            sdkList.ExportToUOAFile(fileName);
        }

        private void BTN_Export_WSC_OnClick(object sender, EventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"{textBox_Export.Text}.wsc");
            MultiComponentList sdkList = _compList.ConvertToSdk();
            sdkList.ExportToWscFile(fileName);
        }

        /// <summary>
        /// Virtual Floor clicked (check on click)
        /// </summary>
        private void BTN_Floor_Clicked(object sender, EventArgs e)
        {
            DrawFloorZ = (int)numericUpDown_Floor.Value;
            ScrollbarsSetValue();
            _forceRefresh = true;
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
            _forceRefresh = true;
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
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Resize Multi Button clicked
        /// </summary>
        private void BTN_ResizeMulti_Click(object sender, EventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            int width = (int)numericUpDown_Size_Width.Value;
            int height = (int)numericUpDown_Size_Height.Value;
            _compList.Resize(width, height);
            MaxHeightTrackBar.Minimum = _compList.ZMin;
            MaxHeightTrackBar.Maximum = _compList.ZMax;
            MaxHeightTrackBar.Value = _compList.ZMax;
            numericUpDown_Selected_X.Maximum = _compList.Width - 1;
            numericUpDown_Selected_Y.Maximum = _compList.Height - 1;
            ScrollbarsSetValue();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Save Button clicked
        /// </summary>
        private void BTN_Save_Click(object sender, EventArgs e)
        {
            if (_compList != null &&
                Utils.ConvertStringToInt(textBox_SaveToID.Text, out int id, 0, 0x1FFF))
            {
                _compList.AddToSdkComponentList(id); //fires MultiChangeEvent
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
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void BTN_Toolbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox thisBox = (CheckBox)sender;
            switch (thisBox.Name)
            {
                case "BTN_Select":
                    thisBox.ImageKey = thisBox.Checked ? "SelectButton_Selected.bmp" : "SelectButton.bmp";
                    break;
                case "BTN_Draw":
                    thisBox.ImageKey = thisBox.Checked ? "DrawButton_Selected.bmp" : "DrawButton.bmp";
                    break;
                case "BTN_Remove":
                    thisBox.ImageKey = thisBox.Checked ? "RemoveButton_Selected.bmp" : "RemoveButton.bmp";
                    break;
                case "BTN_Z":
                    thisBox.ImageKey = thisBox.Checked ? "AltitudeButton_Selected.bmp" : "AltitudeButton.bmp";
                    break;
                case "BTN_Floor":
                    thisBox.ImageKey = thisBox.Checked ? "VirtualFloorButton_Selected.bmp" : "VirtualFloorButton.bmp";
                    break;
                case "BTN_Pipette":
                    thisBox.ImageKey = thisBox.Checked ? "PipetteButton_Selected.bmp" : "PipetteButton.bmp";
                    break;
                case "BTN_Trans":
                    thisBox.ImageKey = thisBox.Checked ? "TransButton_Selected.bmp" : "TransButton.bmp";
                    break;
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
            _forceRefresh = true;
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
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Converts pictureBox coords to Multi coords
        /// </summary>
        private void ConvertCoords(Point point, out int x, out int y, out int z)
        {
            //first check if current Tile matches
            if (HoverTile != null)
            {
                //visible?
                if (!BTN_Floor.Checked || HoverTile.Z >= DrawFloorZ)
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
            cx -= _compList.XMin;
            cy -= _compList.YMin;
            cy += MultiEditorComponentList.GapYMod; //Mod for a bit of gap
            cx += MultiEditorComponentList.GapXMod;

            double mx = point.X - cx;
            double my = point.Y - cy;
            double xx = mx;
            double yy = my;
            my = (xx * CoordinateTransform) - (yy * CoordinateTransform); //Rotate 45° Coordinate system
            mx = (yy * CoordinateTransform) + (xx * CoordinateTransform);
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
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Virtual Floor zValue changed
        /// </summary>
        private void NumericUpDown_Floor_Changed(object sender, EventArgs e)
        {
            if (_compList is null)
            {
                return;
            }

            DrawFloorZ = (int)numericUpDown_Floor.Value;
            _compList.SetFloorZ(DrawFloorZ);
            if (!BTN_Floor.Checked)
            {
                return;
            }

            ScrollbarsSetValue();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// SelectedTile panel X value changed 
        /// </summary>
        private void NumericUpDown_Selected_X_Changed(object sender, EventArgs e)
        {
            if (_compList != null && SelectedTile != null && (int)numericUpDown_Selected_X.Value != SelectedTile.X)
            {
                _compList.TileMove(SelectedTile, (int)numericUpDown_Selected_X.Value, SelectedTile.Y);
                _forceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        /// <summary>
        /// SelectedTile panel Y value changed 
        /// </summary>
        private void NumericUpDown_Selected_Y_Changed(object sender, EventArgs e)
        {
            if (_compList != null && SelectedTile != null && (int)numericUpDown_Selected_Y.Value != SelectedTile.Y)
            {
                _compList.TileMove(SelectedTile, SelectedTile.X, (int)numericUpDown_Selected_Y.Value);
                _forceRefresh = true;
                pictureBoxMulti.Invalidate();
            }
        }

        /// <summary>
        /// SelectedTile panel Z value changed 
        /// </summary>
        private void NumericUpDown_Selected_Z_Changed(object sender, EventArgs e)
        {
            if (_compList == null || SelectedTile == null || (int)numericUpDown_Selected_Z.Value == SelectedTile.Z)
            {
                return;
            }

            _compList.TileZSet(SelectedTile, (int)numericUpDown_Selected_Z.Value);

            MaxHeightTrackBar.Minimum = _compList.ZMin;
            MaxHeightTrackBar.Maximum = _compList.ZMax;

            if (MaxHeightTrackBar.Value < SelectedTile.Z)
            {
                MaxHeightTrackBar.Value = SelectedTile.Z;
            }

            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Event Ultima FilePath changed
        /// </summary>
        private void OnFilePathChangeEvent()
        {
            if (_loaded)
            {
                OnLoad(null, null);
            }
        }

        /// <summary>
        /// Load of UserControl
        /// </summary>
        private void OnLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["Multis"] = true;
            Options.LoadedUltimaClass["Hues"] = true;

            XML_InitializeToolBox();

            string path = Options.AppDataPath;
            string fileName = Path.Combine(path, "Multilist.xml");

            XmlDocument dom = null;
            XmlElement xMultis = null;
            if (File.Exists(fileName))
            {
                dom = new XmlDocument();
                dom.Load(fileName);
                xMultis = dom["Multis"];
            }

            treeViewMultiList.BeginUpdate();
            treeViewMultiList.Nodes.Clear();

            // Let's create a root for import from Multi file and put these in there
            TreeNode multiNode = new TreeNode("Multi.mul");
            for (int i = 0; i < 0x2000; ++i)
            {
                if (Multis.GetComponents(i) == MultiComponentList.Empty)
                {
                    continue;
                }

                TreeNode node;
                if (dom == null)
                {
                    node = new TreeNode(string.Format("{0,5} (0x{0:X})", i));
                }
                else
                {
                    XmlNodeList xMultiNodeList = xMultis.SelectNodes("/Multis/Multi[@id='" + i + "']");
                    string j = "";
                    foreach (XmlNode xMultiNode in xMultiNodeList)
                    {
                        j = xMultiNode.Attributes["name"].Value;
                    }

                    node = new TreeNode(string.Format("{0,5} (0x{0:X}) {1}", i, j));
                }

                node.Tag = i;
                node.Name = i.ToString();
                multiNode.Nodes.Add(node);
            }

            treeViewMultiList.Nodes.Add(multiNode);
            TreeNode fileNode = new TreeNode("From File");

            TreeNode txtNode = new TreeNode("Txt File") { Tag = "txt" };
            fileNode.Nodes.Add(txtNode);

            TreeNode uoaNode = new TreeNode("UOA File") { Tag = "uoa" };
            fileNode.Nodes.Add(uoaNode);

            TreeNode uoabNode = new TreeNode("UOA Binary File") { Tag = "uoab" };
            fileNode.Nodes.Add(uoabNode);

            TreeNode wscNode = new TreeNode("WSC File") { Tag = "wsc" };
            fileNode.Nodes.Add(wscNode);

            TreeNode cacheNode = new TreeNode("MultiCache File") { Tag = "cache" };
            fileNode.Nodes.Add(cacheNode);

            TreeNode uoadesignNode = new TreeNode("UOA Design File") { Tag = "uoadesign" };
            fileNode.Nodes.Add(uoadesignNode);

            treeViewMultiList.Nodes.Add(fileNode);
            treeViewMultiList.EndUpdate();
            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.MultiChangeEvent += OnMultiChangeEvent;
            }

            _loaded = true;
        }

        private void OnMultiChangeEvent(object sender, int id)
        {
            if (!_loaded)
                return;
            if (sender.Equals(this))
                return;
            bool done = false;
            bool remove = Multis.GetComponents(id) == MultiComponentList.Empty;
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
                        TreeNode node = new TreeNode(string.Format("{0,5} (0x{0:X})", id))
                        {
                            Tag = id,
                            Name = id.ToString()
                        };
                        treeViewMultiList.Nodes[0].Nodes.Insert(i, node);
                    }

                    done = true;
                    break;
                }
            }

            if (!remove && !done)
            {
                TreeNode node = new TreeNode(string.Format("{0,5} (0x{0:X})", id))
                {
                    Tag = id,
                    Name = id.ToString()
                };
                treeViewMultiList.Nodes[0].Nodes.Add(node);
            }
        }

        /// <summary>
        /// Hover effect
        /// </summary>
        private void PictureBoxMultiOnMouseMove(object sender, MouseEventArgs e)
        {
            _mouseLoc = e.Location;
            _mouseLoc.X += hScrollBar.Value;
            _mouseLoc.Y += vScrollBar.Value;
            if (_moving)
            {
                int deltaX = -1 * (e.X - _movingLoc.X);
                int deltaY = -1 * (e.Y - _movingLoc.Y);
                _movingLoc = e.Location;
                hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
                vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));
            }

            pictureBoxMulti.Invalidate();
        }

        private void PictureBoxMultiOnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _moving = true;
                _movingLoc = e.Location;
                Cursor = Cursors.Hand;
            }
            else
            {
                _moving = false;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Select/Draw/Remove/Z a Tile
        /// </summary>
        private void PictureBoxMultiOnMouseUp(object sender, MouseEventArgs e)
        {
            _moving = false;
            Cursor = Cursors.Default;

            if (e.Button == MouseButtons.Right)
            {
                return;
            }

            if (_compList == null)
            {
                return;
            }

            if (e.Button == MouseButtons.Middle)
            {
                _overlayList.Clear();
                if (_mHoverTile != null)
                {
                    foreach (MultiTile tile in _compList.GetMultiTileLitAtCoordinate(_mHoverTile.X, _mHoverTile.Y))
                    {
                        if (tile.IsVirtualFloor)
                        {
                            continue;
                        }

                        if (tile.Z == _mHoverTile.Z)
                        {
                            _overlayList.Add(tile);
                        }
                    }
                }
            }
            else if (BTN_Select.Checked)
            {
                SelectedTile = _mHoverTile;
            }
            else if (BTN_Draw.Checked)
            {
                // TODO: something wrong? looks like always true. Maybe it was meant to be > 0 ?
                if (_mDrawTile.Id >= 0)
                {
                    ConvertCoords(_mouseLoc, out int x, out int y, out int z);

                    if (x >= 0 && x < _compList.Width && y >= 0 && y < _compList.Height)
                    {
                        _compList.TileAdd(x, y, z, _mDrawTile.Id);
                        MaxHeightTrackBar.Minimum = _compList.ZMin;
                        MaxHeightTrackBar.Maximum = _compList.ZMax;
                        if (MaxHeightTrackBar.Value < z)
                        {
                            MaxHeightTrackBar.Value = z;
                        }
                    }
                }
            }
            else if (BTN_Remove.Checked)
            {
                if (_mHoverTile != null)
                {
                    _compList.TileRemove(_mHoverTile);
                }
                else
                {
                    int overX = 0;
                    const int overY = 0;
                    foreach (MultiTile tile in _overlayList)
                    {
                        Bitmap bmp = tile.GetBitmap();
                        if (bmp == null)
                        {
                            continue;
                        }

                        if (_mouseLoc.X > overX && _mouseLoc.X < overX + bmp.Width && _mouseLoc.Y > overY &&
                            _mouseLoc.Y < overY + bmp.Height)
                        {
                            //Check for transparent part
                            Color p = bmp.GetPixel(_mouseLoc.X - overX, _mouseLoc.Y - overY);
                            if (!(p.R == 0 && p.G == 0 && p.B == 0))
                            {
                                _compList.TileRemove(tile);
                                _overlayList.Remove(tile);
                                break;
                            }
                        }

                        overX += bmp.Width + 10;
                    }
                }

                MaxHeightTrackBar.Minimum = _compList.ZMin;
                MaxHeightTrackBar.Maximum = _compList.ZMax;
            }
            else if (BTN_Z.Checked)
            {
                if (_mHoverTile != null)
                {
                    int z = (int)numericUpDown_Z.Value;
                    _compList.TileZMod(_mHoverTile, z);
                    MaxHeightTrackBar.Minimum = _compList.ZMin;
                    MaxHeightTrackBar.Maximum = _compList.ZMax;
                    if (MaxHeightTrackBar.Value < _mHoverTile.Z)
                    {
                        MaxHeightTrackBar.Value = _mHoverTile.Z;
                    }
                }
            }
            else if (BTN_Pipette.Checked)
            {
                if (_mHoverTile != null)
                {
                    _mDrawTile.Set(_mHoverTile.Id, 0);
                    PictureBoxDrawTiles_Select();
                    DrawTileLabel.Text = $"Draw ID: 0x{_mHoverTile.Id:X}";
                }
            }
            else if (BTN_Trans.Checked)
            {
                if (_mHoverTile != null)
                {
                    _mHoverTile.Transparent = !_mHoverTile.Transparent;
                }
            }

            if (e.Button != MouseButtons.Middle && !BTN_Remove.Checked)
            {
                _overlayList.Clear();
            }
            else if (_overlayList.Count == 1)
            {
                _overlayList.Clear();
            }

            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private delegate void ADelegate(string t);

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
            if (_compList == null)
            {
                return;
            }

            _compList.GetImage(e.Graphics, hScrollBar.Value, vScrollBar.Value, MaxHeightTrackBar.Value, _mouseLoc,
                BTN_Floor.Checked);
            _forceRefresh = false;

            if (ShowWalkables)
            {
                showWalkablesToolStripMenuItem.Text = $"Show Walkable tiles ({_compList.WalkableCount})";
            }

            if (ShowDoubleSurface)
            {
                showDoubleSurfaceMenuItem.Text = $"Show double surface ({_compList.DoubleSurfaceCount})";
            }

            ConvertCoords(_mouseLoc, out int x, out int y, out int z);
            if (x >= 0 && x < _compList.Width && y >= 0 && y < _compList.Height)
            {
                Invoke(new ADelegate(SetToolStripText), $"{x},{y},{z}");
            }

            if (BTN_Draw.Checked)
            {
                // TODO: always true again? > 0 ?
                if (_mDrawTile.Id < 0)
                {
                    return;
                }

                if (x < 0 || x >= _compList.Width || y < 0 || y >= _compList.Height)
                {
                    return;
                }

                Invoke(new ADelegate(SetToolStripText), $"{x},{y},{z}");
                Bitmap bmp = _mDrawTile.GetBitmap();
                if (bmp == null)
                {
                    return;
                }

                int px = (x - y) * 22;
                int py = (x + y) * 22;

                px -= bmp.Width / 2;
                py -= z * 4;
                py -= bmp.Height;
                px -= _compList.XMin;
                py -= _compList.YMin;
                py += MultiEditorComponentList.GapYMod; //Mod for a bit of gap
                px += MultiEditorComponentList.GapXMod;
                px -= hScrollBar.Value;
                py -= vScrollBar.Value;
                e.Graphics.DrawImage(bmp, new Rectangle(px, py, bmp.Width, bmp.Height), 0, 0, bmp.Width,
                    bmp.Height, GraphicsUnit.Pixel, MultiTile.DrawColor);
            }
            else if (_overlayList.Count > 0)
            {
                int overX = 0;
                const int overY = 0;
                foreach (MultiTile tile in _overlayList)
                {
                    Bitmap bmp = tile.GetBitmap();
                    if (bmp == null)
                        continue;
                    e.Graphics.DrawImage(bmp, new Rectangle(overX, overY, bmp.Width, bmp.Height));
                    overX += bmp.Width + 10;
                }
            }
        }

        /// <summary>
        /// PictureBox size changed
        /// </summary>
        private void PictureBoxMultiOnResize(object sender, EventArgs e)
        {
            ScrollbarsSetValue();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Does the Multi fit inside the PictureBox
        /// </summary>
        private void ScrollbarsSetValue()
        {
            if (_compList == null)
                return;
            int yMin = _compList.YMinOrg;
            int yMax = _compList.YMaxOrg;

            if (BTN_Floor.Checked)
            {
                int floorZMod = (-DrawFloorZ * 4) - 44;
                if (yMin > floorZMod)
                {
                    yMin = floorZMod;
                }

                floorZMod = ((_compList.Width + _compList.Height) * 22) - (DrawFloorZ * 4);

                if (yMax < floorZMod)
                {
                    yMax = floorZMod;
                }
            }

            int height = yMax - yMin + (MultiEditorComponentList.GapYMod * 3);
            int width = _compList.XMax - _compList.XMin + (MultiEditorComponentList.GapXMod * 2);

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
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void TreeViewMultiList_LoadFromFile(Multis.ImportType importType)
        {
            OpenFileDialog dialog = new OpenFileDialog { Multiselect = false };
            string type;
            switch (importType)
            {
                case Multis.ImportType.TXT:
                    type = "txt";
                    break;
                case Multis.ImportType.UOA:
                    type = "uoa";
                    break;
                case Multis.ImportType.UOAB:
                    type = "uoab";
                    break;
                case Multis.ImportType.WSC:
                    type = "wsc";
                    break;
                case Multis.ImportType.MULTICACHE:
                    type = "Multicache.dat";
                    break;
                case Multis.ImportType.UOADESIGN:
                    type = "Designs";
                    break;
                default:
                    return;
            }

            dialog.Title = $"Choose {type} file to import";
            dialog.CheckFileExists = true;

            switch (importType)
            {
                case Multis.ImportType.MULTICACHE:
                    dialog.Filter = string.Format("{0} file ({0})|{0}", type);
                    break;
                case Multis.ImportType.UOADESIGN:
                    dialog.Filter = string.Format("{0} file ({0}.*)|{0}.*", type);
                    break;
                case Multis.ImportType.UOAB:
                    dialog.Filter = string.Format("{0} file (*.{0})|*.{0}", "uoa");
                    break;
                default:
                    dialog.Filter = string.Format("{0} file (*.{0})|*.{0}", type);
                    break;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                switch (importType)
                {
                    case Multis.ImportType.MULTICACHE:
                        {
                            List<MultiComponentList> list = Multis.LoadFromCache(dialog.FileName);
                            TreeNode node = treeViewMultiList.Nodes[1].Nodes[4];
                            node.Nodes.Clear();
                            for (int i = 0; i < list.Count; ++i)
                            {
                                TreeNode child = new TreeNode("Entry " + i) { Tag = list[i] };
                                node.Nodes.Add(child);
                            }

                            break;
                        }
                    case Multis.ImportType.UOADESIGN:
                        {
                            List<object[]> list = Multis.LoadFromDesigner(dialog.FileName);
                            TreeNode node = treeViewMultiList.Nodes[1].Nodes[5];
                            node.Nodes.Clear();
                            for (int i = 0; i < list.Count; ++i)
                            {
                                object[] data = list[i];
                                TreeNode child = new TreeNode(data[0] + "(" + i + ")")
                                {
                                    Tag = data[1]
                                };
                                node.Nodes.Add(child);
                            }

                            break;
                        }
                    default:
                        {
                            MultiComponentList multi = Multis.LoadFromFile(dialog.FileName, importType);
                            _compList = new MultiEditorComponentList(multi, this);
                            UndoList_Clear();
                            MaxHeightTrackBar.Minimum = _compList.ZMin;
                            MaxHeightTrackBar.Maximum = _compList.ZMax;
                            MaxHeightTrackBar.Value = _compList.ZMax;
                            textBox_SaveToID.Text = "0";
                            numericUpDown_Size_Width.Value = _compList.Width;
                            numericUpDown_Size_Height.Value = _compList.Height;
                            numericUpDown_Selected_X.Maximum = _compList.Width - 1;
                            numericUpDown_Selected_Y.Maximum = _compList.Height - 1;
                            vScrollBar.Value = 0;
                            hScrollBar.Value = 0;
                            ScrollbarsSetValue();
                            _forceRefresh = true;
                            pictureBoxMulti.Invalidate();
                            break;
                        }
                }
            }

            dialog.Dispose();
        }

        /// <summary>
        /// DoubleClick Node of Import tree view
        /// </summary>
        private void TreeViewMultiList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                return;
            }

            switch (e.Node.Tag.ToString())
            {
                case "txt":
                    TreeViewMultiList_LoadFromFile(Multis.ImportType.TXT);
                    return;
                case "uoa":
                    TreeViewMultiList_LoadFromFile(Multis.ImportType.UOA);
                    return;
                case "uoab":
                    TreeViewMultiList_LoadFromFile(Multis.ImportType.UOAB);
                    return;
                case "wsc":
                    TreeViewMultiList_LoadFromFile(Multis.ImportType.WSC);
                    return;
                case "cache":
                    TreeViewMultiList_LoadFromFile(Multis.ImportType.MULTICACHE);
                    return;
                case "uoadesign":
                    TreeViewMultiList_LoadFromFile(Multis.ImportType.UOADESIGN);
                    return;
            }

            if (MessageBox.Show("Do you want to open selected Multi?", "Open", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) != DialogResult.OK)
            {
                return;
            }

            if (e.Node.Parent?.Tag != null && e.Node.Parent.Tag.ToString() == "cache")
            {
                MultiComponentList list = (MultiComponentList)e.Node.Tag;
                if (list != null)
                {
                    _compList = new MultiEditorComponentList(list, this);
                    textBox_SaveToID.Text = "0";
                }
            }
            else if (e.Node.Parent?.Tag?.ToString() == "uoadesign")
            {
                MultiComponentList list = (MultiComponentList)e.Node.Tag;
                if (list != null)
                {
                    _compList = new MultiEditorComponentList(list, this);
                    textBox_SaveToID.Text = "0";
                }
            }
            else
            {
                _compList = new MultiEditorComponentList(Multis.GetComponents((int)e.Node.Tag), this);
                textBox_SaveToID.Text = e.Node.Tag.ToString();
            }

            UndoList_Clear();
            MaxHeightTrackBar.Minimum = _compList.ZMin;
            MaxHeightTrackBar.Maximum = _compList.ZMax;
            MaxHeightTrackBar.Value = _compList.ZMax;
            numericUpDown_Size_Width.Value = _compList.Width;
            numericUpDown_Size_Height.Value = _compList.Height;
            numericUpDown_Selected_X.Maximum = _compList.Width - 1;
            numericUpDown_Selected_Y.Maximum = _compList.Height - 1;
            vScrollBar.Value = 0;
            hScrollBar.Value = 0;
            ScrollbarsSetValue();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void TreeViewMultiList_NodeMouseHover(object sender, TreeNodeMouseHoverEventArgs e)
        {
            switch (e.Node.Tag)
            {
                case int nodeTag:
                    {
                        MultiComponentList list = Multis.GetComponents(nodeTag);
                        toolTip1.SetToolTip(treeViewMultiList, $"{list.Width}x{list.Height} {list.SortedTiles.Length}");
                        break;
                    }
                case MultiComponentList multiComponentList:
                    {
                        MultiComponentList list = multiComponentList;
                        toolTip1.SetToolTip(treeViewMultiList, $"{list.Width}x{list.Height} {list.SortedTiles.Length}");
                        break;
                    }
            }
        }

        private void TreeViewTilesXML_OnAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag == null)
            {
                return;
            }

            _drawTilesList = (List<int>)e.Node.Tag;
            vScrollBarDrawTiles.Maximum = (_drawTilesList.Count / _pictureBoxDrawTilesCol) + 1;
            vScrollBarDrawTiles.Minimum = 1;
            vScrollBarDrawTiles.SmallChange = 1;
            vScrollBarDrawTiles.LargeChange = 1;
            vScrollBarDrawTiles.Value = 1;
            pictureBoxDrawTiles.Invalidate();
        }

        private void Undo_onClick(object sender, EventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            int undo = (int)item.Tag;
            _compList.Undo(undo);
            MaxHeightTrackBar.Minimum = _compList.ZMin;
            MaxHeightTrackBar.Maximum = _compList.ZMax;
            MaxHeightTrackBar.Value = _compList.ZMax;
            numericUpDown_Size_Width.Value = _compList.Width;
            numericUpDown_Size_Height.Value = _compList.Height;
            numericUpDown_Selected_X.Maximum = _compList.Width - 1;
            numericUpDown_Selected_Y.Maximum = _compList.Height - 1;
            ScrollbarsSetValue();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void UndoList_BeforeOpening(object sender, EventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            foreach (ToolStripItem item in UndoItems.DropDownItems)
            {
                int index = (int)item.Tag;
                item.Text = _compList.UndoList[index].Tiles != null
                    ? _compList.UndoList[index].Action
                    : "---";
            }
        }

        private void UndoList_Clear()
        {
            _compList?.UndoClear();

            foreach (ToolStripItem item in UndoItems.DropDownItems)
            {
                item.Text = "---";
            }
        }

        private static void XML_AddChildren(TreeNode node, XmlElement mainNode)
        {
            foreach (XmlElement e in mainNode)
            {
                TreeNode tempNode = new TreeNode { Text = e.GetAttribute("name") };

                if (e.Name == "subgroup")
                {
                    tempNode.ImageIndex = 0;

                    if (e.HasChildNodes)
                    {
                        List<int> list = new List<int>();
                        foreach (XmlElement elem in e.ChildNodes)
                        {
                            int i = int.Parse(elem.GetAttribute("index"));
                            if (Art.IsValidStatic(i))
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
            string path = Options.AppDataPath;
            string fileName = Path.Combine(path, "plugins/multieditor.xml");
            if (!File.Exists(fileName))
            {
                return;
            }

            XmlDocument dom = new XmlDocument();
            dom.Load(fileName);

            XmlElement xTiles = dom["TileGroups"];
            if (xTiles == null)
            {
                return;
            }

            foreach (XmlElement xRootGroup in xTiles)
            {
                TreeNode mainNode = new TreeNode
                {
                    Text = xRootGroup.GetAttribute("name"),
                    Tag = null,
                    ImageIndex = 0
                };

                XML_AddChildren(mainNode, xRootGroup);
                treeViewTilesXML.Nodes.Add(mainNode);
            }
        }

        public void SelectDrawTile(ushort id)
        {
            _mDrawTile.Set(id, 0);
            PictureBoxDrawTiles_Select();
            DrawTileLabel.Text = $"Draw ID: 0x{id:X}";
        }

        private int GetIndex(int x, int y)
        {
            if (x >= _pictureBoxDrawTilesCol)
            {
                return -1;
            }

            int value = Math.Max(0,
                (_pictureBoxDrawTilesCol * (vScrollBarDrawTiles.Value - 1)) + x + (y * _pictureBoxDrawTilesCol));

            if (_drawTilesList.Count > value)
            {
                return _drawTilesList[value];
            }

            return -1;
        }

        private void PictureBoxDrawTiles_OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBoxDrawTiles.Focus();
            int x = e.X / (DrawTileSizeWidth - 1);
            int y = e.Y / (DrawTileSizeHeight - 1);

            int index = GetIndex(x, y);
            if (index < 0)
            {
                return;
            }

            _mDrawTile.Set((ushort)index, 0);
            DrawTileLabel.Text = $"Draw ID: 0x{index:X}";
            pictureBoxDrawTiles.Invalidate();
        }

        private void PictureBoxDrawTilesMouseMove(object sender, MouseEventArgs e)
        {
            int x = e.X / (DrawTileSizeWidth - 1);
            int y = e.Y / (DrawTileSizeHeight - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
            {
                if (index != (int)FloatingPreviewPanel.Tag)
                {
                    FloatingPreviewPanel.BackgroundImage = Art.GetStatic(index);
                    FloatingPreviewPanel.Size =
                        new Size(Art.GetStatic(index).Width + 10, Art.GetStatic(index).Height + 10);
                }

                FloatingPreviewPanel.Left = PointToClient(MousePosition).X;
                FloatingPreviewPanel.Top = PointToClient(MousePosition).Y - FloatingPreviewPanel.Size.Height;
                FloatingPreviewPanel.Visible = true;
                FloatingPreviewPanel.Tag = index;
                toolTip1.SetToolTip(pictureBoxDrawTiles, string.Format("0x{0:X} ({0})", index));
                pictureBoxDrawTiles.Invalidate();
            }
            else
            {
                FloatingPreviewPanel.Visible = false;
                toolTip1.SetToolTip(pictureBoxDrawTiles, string.Empty);
            }
        }

        private void PictureBoxDrawTilesMouseLeave(object sender, EventArgs e)
        {
            FloatingPreviewPanel.Visible = false;
        }

        private void PictureBoxDrawTiles_OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int y = 0; y < _pictureBoxDrawTilesRow; ++y)
            {
                for (int x = 0; x < _pictureBoxDrawTilesCol; ++x)
                {
                    int index = GetIndex(x, y);
                    if (index < 0)
                    {
                        continue;
                    }

                    Bitmap b = Art.GetStatic(index);

                    if (b == null)
                    {
                        continue;
                    }

                    Point loc = new Point((x * DrawTileSizeWidth) + 1, (y * DrawTileSizeHeight) + 1);
                    Size size = new Size(DrawTileSizeWidth - 1, DrawTileSizeHeight - 1);
                    Rectangle rect = new Rectangle(loc, size);

                    e.Graphics.Clip = new Region(rect);

                    if (index == _mDrawTile.Id)
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

        private void PictureBoxDrawTiles_OnResize(object sender, EventArgs e)
        {
            if (pictureBoxDrawTiles.Height == 0 || pictureBoxDrawTiles.Width == 0)
            {
                return;
            }

            _pictureBoxDrawTilesCol = pictureBoxDrawTiles.Width / DrawTileSizeWidth;
            _pictureBoxDrawTilesRow = (pictureBoxDrawTiles.Height / DrawTileSizeHeight) + 1;
            vScrollBarDrawTiles.Maximum = (_drawTilesList.Count / _pictureBoxDrawTilesCol) + 1;
            vScrollBarDrawTiles.Minimum = 1;
            vScrollBarDrawTiles.SmallChange = 1;
            vScrollBarDrawTiles.LargeChange = _pictureBoxDrawTilesRow;
            pictureBoxDrawTiles.Invalidate();
        }

        private void VScrollBarDrawTiles_Scroll(object sender, ScrollEventArgs e)
        {
            pictureBoxDrawTiles.Invalidate();
        }

        private void PictureBoxDrawTiles_OnMouseWheel(object sender, MouseEventArgs e)
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

        private void PictureBoxDrawTiles_Select()
        {
            foreach (TreeNode node in treeViewTilesXML.Nodes)
            {
                foreach (TreeNode childNode in node.Nodes)
                {
                    if (childNode.Tag == null)
                    {
                        continue;
                    }

                    foreach (int index in (List<int>)childNode.Tag)
                    {
                        if (index != _mDrawTile.Id)
                        {
                            continue;
                        }

                        treeViewTilesXML.SelectedNode = childNode;
                        pictureBoxDrawTiles.Invalidate();
                        return;
                    }
                }
            }
        }

        private void BTN_DynamicCheckBox_Changed(object sender, EventArgs e)
        {
            if (_compList == null || SelectedTile == null)
            {
                return;
            }

            if (SelectedTile.Invisible != DynamiccheckBox.Checked)
            {
                SelectedTile.Invisible = DynamiccheckBox.Checked;
            }
        }

        private void BTN_ShowWalkables_Click(object sender, EventArgs e)
        {
            showWalkablesToolStripMenuItem.Text = "Show Walkable tiles";
            if (!ShowWalkables || _compList == null)
            {
                return;
            }

            _compList.CalcWalkable();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void BTN_ShowAllTrans(object sender, EventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            foreach (MultiTile tile in _compList.Tiles)
            {
                tile.Transparent = false;
            }

            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void BTN_ShowDoubleSurface(object sender, EventArgs e)
        {
            showDoubleSurfaceMenuItem.Text = "Show double surface";
            if (!ShowDoubleSurface || _compList == null)
            {
                return;
            }

            _compList.CalcDoubleSurface();
            _forceRefresh = true;
            pictureBoxMulti.Invalidate();
        }

        private void OnDummyContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}