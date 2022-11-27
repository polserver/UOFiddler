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
    public partial class MultiEditorControl : UserControl
    {
        private MultiEditorComponentList _compList;

        //Sin/Cos(45°)
        private static readonly double _coordinateTransform = Math.Sqrt(2) / 2;
        private const int _drawTileSizeHeight = 45;
        private const int _drawTileSizeWidth = 45;
        private List<int> _drawTilesList = new List<int>();
        private readonly List<MultiTile> _overlayList = new List<MultiTile>();
        private bool _loaded;
        private bool _moving;
        private readonly MultiTile _drawTile;
        private MultiTile _hoverTile;
        private MultiTile _selectedTile;

        /// <summary>
        /// Current MouseLoc + Scrollbar values (for hover effect)
        /// </summary>
        private Point _mouseLoc;
        private Point _movingLoc;

        private int _pictureBoxDrawTilesCol;
        private int _pictureBoxDrawTilesRow;

        public MultiEditorControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            InitializeComponent();

            _mouseLoc = new Point();
            _movingLoc = new Point();

            _drawTile = new MultiTile();

            Selectedpanel.Visible = false;

            FloatingPreviewPanel.Visible = false;
            FloatingPreviewPanel.Tag = -1;

            BTN_Select.Checked = true;

            pictureBoxDrawTiles.MouseWheel += PictureBoxDrawTiles_OnMouseWheel;
            pictureBoxMulti.ContextMenuStrip = null;
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
            get => _hoverTile;
            set
            {
                _hoverTile = value;
                toolTip1.SetToolTip(pictureBoxMulti, value != null ? $"ID: 0x{_hoverTile.Id:X} Z: {_hoverTile.Z}" : string.Empty);
            }
        }

        /// <summary>
        /// Current Selected Tile (set OnMouseUp)
        /// </summary>
        public MultiTile SelectedTile
        {
            get => _selectedTile;
            private set
            {
                _selectedTile = value;
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

            pictureBoxMulti.Invalidate();
        }

        private void DoExport(Multis.ImportType type)
        {
            if (_compList == null)
            {
                return;
            }

            string path = Options.OutputPath;
            
            MultiComponentList sdkList = _compList.ConvertToSdk();

            switch (type)
            {
                case Multis.ImportType.TXT:
                    sdkList.ExportToTextFile(Path.Combine(path, $"{textBox_Export.Text}.txt"));
                    break;
                case Multis.ImportType.WSC:
                    sdkList.ExportToWscFile(Path.Combine(path, $"{textBox_Export.Text}.wsc"));
                    break;
                case Multis.ImportType.UOA:
                    sdkList.ExportToUOAFile(Path.Combine(path, $"{textBox_Export.Text}.uoa.txt"));
                    break;
                case Multis.ImportType.CSV:
                    sdkList.ExportToCsvFile(Path.Combine(path, $"{textBox_Export.Text}.csv"));
                    break;
                case Multis.ImportType.UOX3:
                    sdkList.ExportToUox3File(Path.Combine(path, $"{textBox_Export.Text}.uox3"));
                    break;
            }
        }

        private void BTN_Export_TXT_OnClick(object sender, EventArgs e)
        {
            DoExport(Multis.ImportType.TXT);
        }

        private void BTN_Export_UOA_OnClick(object sender, EventArgs e)
        {
            DoExport(Multis.ImportType.UOA);
        }

        private void BTN_Export_WSC_OnClick(object sender, EventArgs e)
        {
            DoExport(Multis.ImportType.WSC);
        }

        private void BTN_Export_CSV_OnClick(object sender, EventArgs e)
        {
            DoExport(Multis.ImportType.CSV);
        }

        private void BTN_Export_UOX3_Click(object sender, EventArgs e)
        {
            DoExport(Multis.ImportType.UOX3);
        }

        /// <summary>
        /// Virtual Floor clicked (check on click)
        /// </summary>
        private void BTN_Floor_Clicked(object sender, EventArgs e)
        {
            DrawFloorZ = (int)numericUpDown_Floor.Value;

            ScrollbarsSetValue();

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

            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Save Button clicked
        /// </summary>
        private void BTN_Save_Click(object sender, EventArgs e)
        {
            if (_compList != null && Utils.ConvertStringToInt(textBox_SaveToID.Text, out int id, 0, Multis.MaximumMultiIndex - 1))
            {
                _compList.AddToSdkComponentList(id); // fires MultiChangeEvent
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

            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Converts pictureBox coords to Multi coords
        /// </summary>
        private void ConvertCoords(Point point, out int x, out int y, out int z)
        {
            // first check if current Tile matches
            if (HoverTile != null)
            {
                // visible?
                if (!BTN_Floor.Checked || HoverTile.Z >= DrawFloorZ)
                {
                    x = HoverTile.X;
                    y = HoverTile.Y;
                    z = HoverTile.Z + HoverTile.Height;
                    return;
                }
            }

            // damn the hard way
            z = 0;

            int cx = 0; // Get MouseCoords for (0/0)
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
            my = (xx * _coordinateTransform) - (yy * _coordinateTransform); // Rotate 45° Coordinate system
            mx = (yy * _coordinateTransform) + (xx * _coordinateTransform);
            mx /= _coordinateTransform * 44; // Math.Sqrt(2)*22==CoordinateTransform*44
            my /= _coordinateTransform * 44; // CoordinateTransform=Math.Sqrt(2)/2
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
            for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
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

            TreeNode uoaNode = new TreeNode("UOA Text File") { Tag = "uoa" };
            fileNode.Nodes.Add(uoaNode);

            TreeNode uoabNode = new TreeNode("UOA Binary File") { Tag = "uoab" };
            fileNode.Nodes.Add(uoabNode);

            TreeNode wscNode = new TreeNode("WSC File") { Tag = "wsc" };
            fileNode.Nodes.Add(wscNode);

            TreeNode cacheNode = new TreeNode("MultiCache File") { Tag = "cache" };
            fileNode.Nodes.Add(cacheNode);

            TreeNode uoadesignNode = new TreeNode("UOA Design File") { Tag = "uoadesign" };
            fileNode.Nodes.Add(uoadesignNode);

            TreeNode csvNode = new TreeNode("CSV File") { Tag = "csv" };
            fileNode.Nodes.Add(csvNode);

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
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            bool done = false;
            bool remove = Multis.GetComponents(id) == MultiComponentList.Empty;

            for (int i = 0; i < treeViewMultiList.Nodes[0].Nodes.Count; ++i)
            {
                if (id == (int)treeViewMultiList.Nodes[0].Nodes[i].Tag)
                {
                    done = true;

                    if (remove)
                    {
                        treeViewMultiList.Nodes[0].Nodes.RemoveAt(i);
                    }

                    break;
                }

                if (id >= (int)treeViewMultiList.Nodes[0].Nodes[i].Tag)
                {
                    continue;
                }

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

                if (_hoverTile != null)
                {
                    foreach (MultiTile tile in _compList.GetMultiTileLitAtCoordinate(_hoverTile.X, _hoverTile.Y))
                    {
                        if (tile.IsVirtualFloor)
                        {
                            continue;
                        }

                        if (tile.Z == _hoverTile.Z)
                        {
                            _overlayList.Add(tile);
                        }
                    }
                }
            }
            else if (BTN_Select.Checked)
            {
                SelectedTile = _hoverTile;
            }
            else if (BTN_Draw.Checked)
            {
                ConvertCoords(_mouseLoc, out int x, out int y, out int z);

                if (x >= 0 && x < _compList.Width && y >= 0 && y < _compList.Height)
                {
                    _compList.TileAdd(x, y, z, _drawTile.Id);
                    MaxHeightTrackBar.Minimum = _compList.ZMin;
                    MaxHeightTrackBar.Maximum = _compList.ZMax;
                    if (MaxHeightTrackBar.Value < z)
                    {
                        MaxHeightTrackBar.Value = z;
                    }
                }
            }
            else if (BTN_Remove.Checked)
            {
                if (_hoverTile != null)
                {
                    _compList.TileRemove(_hoverTile);
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
                            // Check for transparent part
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
                if (_hoverTile != null)
                {
                    int z = (int)numericUpDown_Z.Value;
                    _compList.TileZMod(_hoverTile, z);
                    MaxHeightTrackBar.Minimum = _compList.ZMin;
                    MaxHeightTrackBar.Maximum = _compList.ZMax;
                    if (MaxHeightTrackBar.Value < _hoverTile.Z)
                    {
                        MaxHeightTrackBar.Value = _hoverTile.Z;
                    }
                }
            }
            else if (BTN_Pipette.Checked)
            {
                if (_hoverTile != null)
                {
                    _drawTile.Set(_hoverTile.Id, 0);
                    PictureBoxDrawTiles_Select();
                    DrawTileLabel.Text = $"Draw ID: 0x{_hoverTile.Id:X}";
                }
            }
            else if (BTN_Trans.Checked)
            {
                if (_hoverTile != null)
                {
                    _hoverTile.Transparent = !_hoverTile.Transparent;
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

            _compList.GetImage(e.Graphics, hScrollBar.Value, vScrollBar.Value, MaxHeightTrackBar.Value, _mouseLoc, BTN_Floor.Checked);

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
                if (x < 0 || x >= _compList.Width || y < 0 || y >= _compList.Height)
                {
                    return;
                }

                Invoke(new ADelegate(SetToolStripText), $"{x},{y},{z}");

                Bitmap bmp = _drawTile.GetBitmap();

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
                py += MultiEditorComponentList.GapYMod; // Mod for a bit of gap
                px += MultiEditorComponentList.GapXMod;
                px -= hScrollBar.Value;
                py -= vScrollBar.Value;

                e.Graphics.DrawImage(bmp, new Rectangle(px, py, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height,
                    GraphicsUnit.Pixel, MultiTile.DrawColor);
            }
            else if (_overlayList.Count > 0)
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

            pictureBoxMulti.Invalidate();
        }

        /// <summary>
        /// Does the Multi fit inside the PictureBox
        /// </summary>
        private void ScrollbarsSetValue()
        {
            if (_compList == null)
            {
                return;
            }

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
                    type = "txt";
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
                case Multis.ImportType.CSV:
                    type = "csv";
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
                case "csv":
                    TreeViewMultiList_LoadFromFile(Multis.ImportType.CSV);
                    return;
            }

            if (MessageBox.Show("Do you want to open selected Multi?", "Open", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) != DialogResult.OK)
            {
                return;
            }

            if (e.Node.Parent?.Tag != null && (e.Node.Parent.Tag.ToString() == "cache" || e.Node.Parent.Tag.ToString() == "uoadesign"))
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
                            {
                                list.Add(i);
                            }
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
            _drawTile.Set(id, 0);
            PictureBoxDrawTiles_Select();
            DrawTileLabel.Text = $"Draw ID: 0x{id:X}";
        }

        private int GetIndex(int x, int y)
        {
            if (x >= _pictureBoxDrawTilesCol)
            {
                return -1;
            }

            int value = Math.Max(0, (_pictureBoxDrawTilesCol * (vScrollBarDrawTiles.Value - 1)) + x + (y * _pictureBoxDrawTilesCol));

            if (_drawTilesList.Count > value)
            {
                return _drawTilesList[value];
            }

            return -1;
        }

        private void PictureBoxDrawTiles_OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBoxDrawTiles.Focus();

            int x = e.X / (_drawTileSizeWidth - 1);
            int y = e.Y / (_drawTileSizeHeight - 1);

            int index = GetIndex(x, y);
            if (index < 0)
            {
                return;
            }

            _drawTile.Set((ushort)index, 0);

            DrawTileLabel.Text = $"Draw ID: 0x{index:X}";

            pictureBoxDrawTiles.Invalidate();
        }

        private void PictureBoxDrawTilesMouseMove(object sender, MouseEventArgs e)
        {
            pictureBoxDrawTiles.Invalidate();
        }

        private void PictureBoxDrawTilesMouseLeave(object sender, EventArgs e)
        {
            FloatingPreviewPanel.Visible = false;

            pictureBoxDrawTiles.Invalidate();
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

                    Point loc = new Point((x * _drawTileSizeWidth) + 1, (y * _drawTileSizeHeight) + 1);
                    Size size = new Size(_drawTileSizeWidth - 1, _drawTileSizeHeight - 1);
                    Rectangle rect = new Rectangle(loc, size);

                    e.Graphics.Clip = new Region(rect);

                    if (index == _drawTile.Id)
                    {
                        e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                    }

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

            var pos = pictureBoxDrawTiles.PointToClient(MousePosition);
            if (!pictureBoxDrawTiles.ClientRectangle.Contains(pos))
            {
                return;
            }

            int x1 = pos.X / (_drawTileSizeWidth - 1);
            int y1 = pos.Y / (_drawTileSizeHeight - 1);
            int staticIdx = GetIndex(x1, y1);

            if (staticIdx >= 0)
            {
                if (staticIdx != (int)FloatingPreviewPanel.Tag)
                {
                    FloatingPreviewPanel.BackgroundImage = Art.GetStatic(staticIdx);
                    FloatingPreviewPanel.Size =
                        new Size(Art.GetStatic(staticIdx).Width + 10, Art.GetStatic(staticIdx).Height + 10);
                }

                var currentPos = PointToClient(MousePosition);
                FloatingPreviewPanel.Left = currentPos.X;
                FloatingPreviewPanel.Top = currentPos.Y - FloatingPreviewPanel.Size.Height;
                FloatingPreviewPanel.Visible = true;
                FloatingPreviewPanel.Tag = staticIdx;

                toolTip1.SetToolTip(pictureBoxDrawTiles, string.Format("0x{0:X} ({0})", staticIdx));
            }
            else
            {
                FloatingPreviewPanel.Visible = false;
                toolTip1.SetToolTip(pictureBoxDrawTiles, string.Empty);
            }
        }

        private void PictureBoxDrawTiles_OnResize(object sender, EventArgs e)
        {
            if (pictureBoxDrawTiles.Height == 0 || pictureBoxDrawTiles.Width == 0)
            {
                return;
            }

            _pictureBoxDrawTilesCol = pictureBoxDrawTiles.Width / _drawTileSizeWidth;
            _pictureBoxDrawTilesRow = (pictureBoxDrawTiles.Height / _drawTileSizeHeight) + 1;
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
                        if (index != _drawTile.Id)
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

            pictureBoxMulti.Invalidate();
        }

        private void OnDummyContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}