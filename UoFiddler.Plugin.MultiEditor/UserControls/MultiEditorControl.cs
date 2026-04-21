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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Forms;

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

        // Multi-selection
        private readonly HashSet<MultiTile> _selectedTiles = new HashSet<MultiTile>();
        private bool _isMarqueeDragging;
        private Point _marqueeStart;
        private Point _marqueeEnd;

        // Copy/Paste
        private List<(ushort id, int dx, int dy, int dz)> _clipboard;
        private bool _isPasteMode;

        // Zoom
        private float _zoomFactor = 1.0f;

        // Minimap
        private bool _minimapDirty = true;
        private Bitmap _minimapTilesBmp;   // cached tile-dot layer, regenerated only when tiles change

        // Recently used tiles
        private readonly List<int> _recentTiles = new List<int>();
        private FlowLayoutPanel _recentPanel;
        private readonly List<PictureBox> _recentSlots = new List<PictureBox>();

        // Rectangle fill tool
        private bool _isRectFillDragging;
        private int _rectFillStartX, _rectFillStartY;
        private int _rectFillEndX, _rectFillEndY;

        // Line draw tool
        private bool _isLineDragging;
        private int _lineStartX, _lineStartY;
        private int _lineEndX, _lineEndY;

        // Bulk remove tool (multi-space coords, mirrors rect fill)
        private bool _isRemoveDragging;
        private int _removeStartX, _removeStartY;
        private int _removeEndX, _removeEndY;

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
            pictureBoxMulti.MouseWheel += PictureBoxMulti_OnMouseWheel;

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
        /// Current Selected Tile (set OnMouseUp). Setting this also syncs the single-tile selection panel.
        /// </summary>
        public MultiTile SelectedTile
        {
            get => _selectedTile;
            private set
            {
                _selectedTile = value;
                if (value != null)
                {
                    Selectedpanel.Visible = true;
                    SelectedTileLabel.Text = $"ID: 0x{value.Id:X} Z: {value.Z}";
                    numericUpDown_Selected_X.Value = value.X;
                    numericUpDown_Selected_Y.Value = value.Y;
                    numericUpDown_Selected_Z.Value = value.Z;
                    DynamiccheckBox.Checked = value.Invisible;
                }
                else if (_selectedTiles.Count == 0)
                {
                    Selectedpanel.Visible = false;
                    SelectedTileLabel.Text = "ID:";
                }
                else
                {
                    // Multiple tiles selected — show count in label, keep panel visible for Z operations
                    Selectedpanel.Visible = true;
                    SelectedTileLabel.Text = $"{_selectedTiles.Count} tiles selected";
                }
            }
        }

        public bool ShowWalkables => showWalkablesToolStripMenuItem.Checked;
        public bool ShowDoubleSurface => showDoubleSurfaceMenuItem.Checked;
        public HashSet<MultiTile> SelectedTiles => _selectedTiles;

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
            BTN_RectFill.Checked = false;
            BTN_LineDraw.Checked = false;

            pictureBoxMulti.Invalidate();
        }

        private void DoExport(Multis.ImportType type)
        {
            var designName = textBox_Export.Text;

            if (string.IsNullOrEmpty(designName))
            {
                MessageBox.Show(
                    "Export design name can't be empty.",
                    "Missing file name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1);

                return;
            }

            if (_compList == null)
            {
                return;
            }

            string path = Options.OutputPath;

            MultiComponentList sdkList = _compList.ConvertToSdk();

            var fileName = string.Empty;

            switch (type)
            {
                case Multis.ImportType.TXT:
                    fileName = Path.Combine(path, $"{designName}.txt");
                    sdkList.ExportToTextFile(fileName);
                    break;
                case Multis.ImportType.WSC:
                    fileName = Path.Combine(path, $"{designName}.wsc");
                    sdkList.ExportToWscFile(fileName);
                    break;
                case Multis.ImportType.UOA:
                    fileName = Path.Combine(path, $"{designName}.uoa.txt");
                    sdkList.ExportToUOAFile(fileName);
                    break;
                case Multis.ImportType.CSV:
                    fileName = Path.Combine(path, $"{designName}.csv");
                    sdkList.ExportToCsvFile(fileName);
                    break;
                case Multis.ImportType.UOX3:
                    fileName = Path.Combine(path, $"{designName}.uox3");
                    sdkList.ExportToUox3File(fileName);
                    break;
                case Multis.ImportType.XML:
                    fileName = Path.Combine(path, $"{designName}.xml");
                    sdkList.ExportToXmlFile(fileName, designName);
                    break;
            }

            MessageBox.Show($"Multi design saved to {fileName}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

        private void BTN_Export_XML_Click(object sender, EventArgs e)
        {
            DoExport(Multis.ImportType.XML);
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
            BTN_RectFill.Checked = false;
            BTN_LineDraw.Checked = false;

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
            BTN_RectFill.Checked = false;
            BTN_LineDraw.Checked = false;

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
            BTN_RectFill.Checked = false;
            BTN_LineDraw.Checked = false;

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
                case "BTN_RectFill":
                    thisBox.ImageKey = thisBox.Checked ? "RectFillButton_Selected.bmp" : "RectFillButton.bmp";
                    break;
                case "BTN_LineDraw":
                    thisBox.ImageKey = thisBox.Checked ? "LineDrawButton_Selected.bmp" : "LineDrawButton.bmp";
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
            BTN_RectFill.Checked = false;
            BTN_LineDraw.Checked = false;

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
            BTN_RectFill.Checked = false;
            BTN_LineDraw.Checked = false;

            pictureBoxMulti.Invalidate();
        }

        private void BTN_RectFill_Click(object sender, EventArgs e)
        {
            BTN_Select.Checked = false;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = false;
            BTN_LineDraw.Checked = false;
            BTN_RectFill.Checked = true;

            pictureBoxMulti.Invalidate();
        }

        private void BTN_LineDraw_Click(object sender, EventArgs e)
        {
            BTN_Select.Checked = false;
            BTN_Draw.Checked = false;
            BTN_Remove.Checked = false;
            BTN_Z.Checked = false;
            BTN_Pipette.Checked = false;
            BTN_Trans.Checked = false;
            BTN_RectFill.Checked = false;
            BTN_LineDraw.Checked = true;

            pictureBoxMulti.Invalidate();
        }


        private int BrushSize => RB_Brush_M.Checked ? 3 : RB_Brush_L.Checked ? 4 : 1;

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
                    // Cap placement Z to MaxHeightTrackBar when height filter is active
                    if (MaxHeightTrackBar.Value < MaxHeightTrackBar.Maximum)
                    {
                        z = Math.Min(z, MaxHeightTrackBar.Value);
                    }
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
            if (_compList == null)
            {
                return;
            }

            int newZ = (int)numericUpDown_Selected_Z.Value;

            if (_selectedTiles.Count > 1)
            {
                // Apply absolute Z to the whole group
                _compList.TileZSet(_selectedTiles, newZ);
                MaxHeightTrackBar.Minimum = _compList.ZMin;
                MaxHeightTrackBar.Maximum = _compList.ZMax;
                pictureBoxMulti.Invalidate();
                return;
            }

            if (SelectedTile == null || newZ == SelectedTile.Z)
            {
                return;
            }

            _compList.TileZSet(SelectedTile, newZ);

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

            if (!_loaded)
            {
                _recentTiles.Clear();
                _recentTiles.AddRange(TileRecentlyUsed.Load());
                InitializeRecentTab();
            }

            RefreshRecentPanel();

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

            TreeNode uopNode = new TreeNode("multicollection.uop") { Name = "uop" };
            if (!Multis.HasUopFile)
            {
                uopNode.Nodes.Add(new TreeNode("File not found"));
            }
            else
            {
                for (int i = 0; i < Multis.MaximumMultiIndex; ++i)
                {
                    if (Multis.GetUopComponents(i) == MultiComponentList.Empty)
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
                    uopNode.Nodes.Add(node);
                }
            }

            treeViewMultiList.Nodes.Add(uopNode);
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
            _mouseLoc = ToScrolledSpace(e.Location);

            if (_moving)
            {
                int deltaX = -1 * (e.X - _movingLoc.X);
                int deltaY = -1 * (e.Y - _movingLoc.Y);

                _movingLoc = e.Location;

                hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
                vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));
            }
            else if (_isMarqueeDragging)
            {
                _marqueeEnd = new Point(e.X + hScrollBar.Value, e.Y + vScrollBar.Value);
            }
            else if (_isRectFillDragging && _compList != null)
            {
                ConvertCoords(_mouseLoc, out int x, out int y, out int _);
                _rectFillEndX = x;
                _rectFillEndY = y;
            }
            else if (_isLineDragging && _compList != null)
            {
                ConvertCoords(_mouseLoc, out int x, out int y, out int _);
                _lineEndX = x;
                _lineEndY = y;
            }
            else if (_isRemoveDragging && _compList != null)
            {
                ConvertCoords(_mouseLoc, out int rx, out int ry, out int _);
                _removeEndX = rx;
                _removeEndY = ry;
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
            else if (e.Button == MouseButtons.Left && BTN_Select.Checked && _compList != null)
            {
                bool ctrlHeld = (ModifierKeys & Keys.Control) != 0;
                if (_hoverTile == null && !ctrlHeld)
                {
                    // Start marquee drag over empty space
                    _isMarqueeDragging = true;
                    _marqueeStart = new Point(e.X + hScrollBar.Value, e.Y + vScrollBar.Value);
                    _marqueeEnd = _marqueeStart;
                }
            }
            else if (e.Button == MouseButtons.Left && BTN_RectFill.Checked && _compList != null)
            {
                ConvertCoords(_mouseLoc, out int x, out int y, out int _);
                _isRectFillDragging = true;
                _rectFillStartX = x;
                _rectFillStartY = y;
                _rectFillEndX = x;
                _rectFillEndY = y;
            }
            else if (e.Button == MouseButtons.Left && BTN_LineDraw.Checked && _compList != null)
            {
                ConvertCoords(_mouseLoc, out int x, out int y, out int _);
                _isLineDragging = true;
                _lineStartX = x;
                _lineStartY = y;
                _lineEndX = x;
                _lineEndY = y;
            }
            else if (e.Button == MouseButtons.Left && BTN_Remove.Checked && _compList != null)
            {
                ConvertCoords(_mouseLoc, out int rx, out int ry, out int _);
                _isRemoveDragging = true;
                _removeStartX = rx;
                _removeStartY = ry;
                _removeEndX = rx;
                _removeEndY = ry;
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
                if (_isMarqueeDragging)
                {
                    _isMarqueeDragging = false;
                    pictureBoxMulti.Invalidate();
                }

                if (_isRemoveDragging)
                {
                    _isRemoveDragging = false;
                    pictureBoxMulti.Invalidate();
                    return;
                }

                if (_isPasteMode)
                {
                    _isPasteMode = false;
                    pictureBoxMulti.Invalidate();
                }

                return;
            }

            if (_compList == null)
            {
                return;
            }

            // Paste commit
            if (_isPasteMode && e.Button == MouseButtons.Left && _clipboard != null)
            {
                ConvertCoords(_mouseLoc, out int px, out int py, out int pz);
                var batch = _clipboard.Select(c => (c.id, px + c.dx, py + c.dy, pz + c.dz)).ToList();
                _compList.TileAddBatch(batch);
                _isPasteMode = false;
                MaxHeightTrackBar.Minimum = _compList.ZMin;
                MaxHeightTrackBar.Maximum = _compList.ZMax;
                pictureBoxMulti.Invalidate();
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
                bool ctrlHeld = (ModifierKeys & Keys.Control) != 0;

                if (_isMarqueeDragging)
                {
                    _isMarqueeDragging = false;
                    int sx = Math.Min(_marqueeStart.X, _marqueeEnd.X) - hScrollBar.Value;
                    int sy = Math.Min(_marqueeStart.Y, _marqueeEnd.Y) - vScrollBar.Value;
                    int sw = Math.Abs(_marqueeEnd.X - _marqueeStart.X);
                    int sh = Math.Abs(_marqueeEnd.Y - _marqueeStart.Y);

                    if (sw > 3 && sh > 3)
                    {
                        var rect = new Rectangle(sx, sy, sw, sh);
                        var tiles = _compList.GetTilesInScreenRect(
                            rect, hScrollBar.Value, vScrollBar.Value,
                            MaxHeightTrackBar.Value, BTN_Floor.Checked);
                        if (!ctrlHeld)
                        {
                            _selectedTiles.Clear();
                        }

                        foreach (var t in tiles)
                        {
                            _selectedTiles.Add(t);
                        }

                        SelectedTile = _selectedTiles.Count == 1 ? _selectedTiles.First() : null;
                    }
                    else
                    {
                        // Tiny drag — treat as plain click
                        if (!ctrlHeld)
                        {
                            _selectedTiles.Clear();
                        }

                        if (_hoverTile != null)
                        {
                            _selectedTiles.Add(_hoverTile);
                        }

                        SelectedTile = _hoverTile;
                    }
                }
                else if (ctrlHeld && _hoverTile != null)
                {
                    if (_selectedTiles.Contains(_hoverTile))
                    {
                        _selectedTiles.Remove(_hoverTile);
                    }
                    else
                    {
                        _selectedTiles.Add(_hoverTile);
                    }

                    SelectedTile = _selectedTiles.Count == 1 ? _selectedTiles.First() : null;
                }
                else
                {
                    _selectedTiles.Clear();
                    if (_hoverTile != null)
                    {
                        _selectedTiles.Add(_hoverTile);
                    }

                    SelectedTile = _hoverTile;
                }
            }
            else if (BTN_Draw.Checked)
            {
                ConvertCoords(_mouseLoc, out int x, out int y, out int z);

                if (x >= 0 && x < _compList.Width && y >= 0 && y < _compList.Height)
                {
                    int brushSize = BrushSize;
                    if (brushSize <= 1)
                    {
                        _compList.TileAdd(x, y, z, _drawTile.Id);
                    }
                    else
                    {
                        _compList.TileAddBrush(x, y, z, _drawTile.Id, brushSize);
                    }

                    RecordRecentTile(_drawTile.Id);
                    MaxHeightTrackBar.Minimum = _compList.ZMin;
                    MaxHeightTrackBar.Maximum = _compList.ZMax;
                    if (MaxHeightTrackBar.Value < z)
                    {
                        MaxHeightTrackBar.Value = z;
                    }
                }
            }
            else if (BTN_RectFill.Checked && _isRectFillDragging)
            {
                _isRectFillDragging = false;
                ConvertCoords(_mouseLoc, out int ex, out int ey, out int z);
                int x0 = Math.Min(_rectFillStartX, ex);
                int x1 = Math.Max(_rectFillStartX, ex);
                int y0 = Math.Min(_rectFillStartY, ey);
                int y1 = Math.Max(_rectFillStartY, ey);
                _compList.TileAddRect(x0, y0, x1, y1, z, _drawTile.Id);
                RecordRecentTile(_drawTile.Id);
                MaxHeightTrackBar.Minimum = _compList.ZMin;
                MaxHeightTrackBar.Maximum = _compList.ZMax;
                if (MaxHeightTrackBar.Value < z)
                {
                    MaxHeightTrackBar.Value = z;
                }
            }
            else if (BTN_LineDraw.Checked && _isLineDragging)
            {
                _isLineDragging = false;
                ConvertCoords(_mouseLoc, out int ex, out int ey, out int z);
                (ex, ey) = GetLineEnd(ex, ey);
                _compList.TileAddLine(_lineStartX, _lineStartY, ex, ey, z, _drawTile.Id);
                RecordRecentTile(_drawTile.Id);
                MaxHeightTrackBar.Minimum = _compList.ZMin;
                MaxHeightTrackBar.Maximum = _compList.ZMax;
                if (MaxHeightTrackBar.Value < z)
                {
                    MaxHeightTrackBar.Value = z;
                }
            }
            else if (BTN_Remove.Checked)
            {
                bool wasDrag = _isRemoveDragging &&
                    (_removeEndX != _removeStartX || _removeEndY != _removeStartY);
                _isRemoveDragging = false;

                if (wasDrag)
                {
                    int x0 = Math.Min(_removeStartX, _removeEndX);
                    int x1 = Math.Max(_removeStartX, _removeEndX);
                    int y0 = Math.Min(_removeStartY, _removeEndY);
                    int y1 = Math.Max(_removeStartY, _removeEndY);
                    _compList.TileRemoveRect(x0, y0, x1, y1, DrawFloorZ, MaxHeightTrackBar.Value);
                }
                else if (_hoverTile != null)
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
                int z = (int)numericUpDown_Z.Value;
                if (_selectedTiles.Count > 1)
                {
                    _compList.TileZMod(_selectedTiles, z);
                    MaxHeightTrackBar.Minimum = _compList.ZMin;
                    MaxHeightTrackBar.Maximum = _compList.ZMax;
                }
                else if (_hoverTile != null)
                {
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
                    UpdateRecentSelection();
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

            _minimapDirty = true;
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

            if (_zoomFactor != 1.0f)
            {
                float cx = pictureBoxMulti.Width / 2f;
                float cy = pictureBoxMulti.Height / 2f;
                e.Graphics.TranslateTransform(cx, cy);
                e.Graphics.ScaleTransform(_zoomFactor, _zoomFactor);
                e.Graphics.TranslateTransform(-cx, -cy);
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

            // Draw marquee selection rectangle
            if (_isMarqueeDragging)
            {
                int sx = Math.Min(_marqueeStart.X, _marqueeEnd.X) - hScrollBar.Value;
                int sy = Math.Min(_marqueeStart.Y, _marqueeEnd.Y) - vScrollBar.Value;
                int sw = Math.Abs(_marqueeEnd.X - _marqueeStart.X);
                int sh = Math.Abs(_marqueeEnd.Y - _marqueeStart.Y);
                using (var pen = new System.Drawing.Pen(Color.DodgerBlue, 1))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                    e.Graphics.DrawRectangle(pen, sx, sy, sw, sh);
                }

                using (var brush = new SolidBrush(Color.FromArgb(30, Color.DodgerBlue)))
                {
                    e.Graphics.FillRectangle(brush, sx, sy, sw, sh);
                }
            }

            // Bulk remove drag preview — highlight existing tiles in the multi-space rect
            if (_isRemoveDragging)
            {
                int rx0 = Math.Min(_removeStartX, _removeEndX);
                int rx1 = Math.Max(_removeStartX, _removeEndX);
                int ry0 = Math.Min(_removeStartY, _removeEndY);
                int ry1 = Math.Max(_removeStartY, _removeEndY);
                foreach (MultiTile ht in _compList.Tiles)
                {
                    if (ht.IsVirtualFloor || ht.Z < DrawFloorZ || ht.Z > MaxHeightTrackBar.Value || ht.X < rx0 || ht.X > rx1 || ht.Y < ry0 || ht.Y > ry1)
                    {
                        continue;
                    }

                    Bitmap htBmp = ht.GetBitmap();
                    if (htBmp == null)
                    {
                        continue;
                    }

                    int hpx = ht.XMod - _compList.XMin - hScrollBar.Value;
                    int hpy = ht.YMod - _compList.YMin - vScrollBar.Value;
                    e.Graphics.DrawImage(htBmp, new Rectangle(hpx, hpy, htBmp.Width, htBmp.Height),
                        0, 0, htBmp.Width, htBmp.Height, GraphicsUnit.Pixel, MultiTile.RemoveHighlightColor);
                }
            }

            // Paste ghost preview
            if (_isPasteMode && _clipboard != null)
            {
                ConvertCoords(_mouseLoc, out int px, out int py, out int pz);
                foreach (var (id, dx, dy, dz) in _clipboard)
                {
                    var ghost = new MultiTile(id, px + dx, py + dy, pz + dz, 1);
                    Bitmap ghostBmp = ghost.GetBitmap();
                    if (ghostBmp == null)
                    {
                        continue;
                    }

                    int gpx = (ghost.X - ghost.Y) * 22 - ghostBmp.Width / 2 - _compList.XMin + MultiEditorComponentList.GapXMod - hScrollBar.Value;
                    int gpy = (ghost.X + ghost.Y) * 22 - ghost.Z * 4 - ghostBmp.Height - _compList.YMin + MultiEditorComponentList.GapYMod - vScrollBar.Value;
                    e.Graphics.DrawImage(ghostBmp, new Rectangle(gpx, gpy, ghostBmp.Width, ghostBmp.Height),
                        0, 0, ghostBmp.Width, ghostBmp.Height, GraphicsUnit.Pixel, MultiTile.DrawColor);
                }
            }

            if (BTN_Draw.Checked)
            {
                if (x < 0 || x >= _compList.Width || y < 0 || y >= _compList.Height)
                {
                    return;
                }

                Invoke(new ADelegate(SetToolStripText), $"{x},{y},{z}");

                int brushSize = BrushSize;
                int half = brushSize / 2;
                for (int dx = -half; dx <= half; dx++)
                {
                    for (int dy = -half; dy <= half; dy++)
                    {
                        DrawTilePreviewAt(e.Graphics, x + dx, y + dy, z);
                    }
                }
            }
            else if (BTN_RectFill.Checked && _isRectFillDragging)
            {
                int x0 = Math.Min(_rectFillStartX, _rectFillEndX);
                int x1 = Math.Max(_rectFillStartX, _rectFillEndX);
                int y0 = Math.Min(_rectFillStartY, _rectFillEndY);
                int y1 = Math.Max(_rectFillStartY, _rectFillEndY);
                for (int rx = x0; rx <= x1; rx++)
                {
                    for (int ry = y0; ry <= y1; ry++)
                    {
                        DrawTilePreviewAt(e.Graphics, rx, ry, z);
                    }
                }
            }
            else if (BTN_LineDraw.Checked && _isLineDragging)
            {
                var (cex, cey) = GetLineEnd(_lineEndX, _lineEndY);
                foreach (var (lx, ly) in MultiEditorComponentList.BresenhamLinePublic(_lineStartX, _lineStartY, cex, cey))
                {
                    DrawTilePreviewAt(e.Graphics, lx, ly, z);
                }
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

            UpdateMinimap();
        }

        private void PictureBoxMulti_OnMouseWheel(object sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != 0)
            {
                float delta = e.Delta > 0 ? 0.1f : -0.1f;
                SetZoom(_zoomFactor + delta);
                if (e is HandledMouseEventArgs hme)
                {
                    hme.Handled = true;
                }

                return;
            }

            // Normal scroll: pan vertically
            int scrollDelta = e.Delta > 0 ? -22 : 22;
            vScrollBar.Value = Math.Clamp(vScrollBar.Value + scrollDelta, 0, vScrollBar.Maximum);
        }

        private Point ToScrolledSpace(Point screenPt)
        {
            float cx = pictureBoxMulti.Width / 2f;
            float cy = pictureBoxMulti.Height / 2f;
            float sx = (screenPt.X - cx) / _zoomFactor + cx + hScrollBar.Value;
            float sy = (screenPt.Y - cy) / _zoomFactor + cy + vScrollBar.Value;
            return new Point((int)sx, (int)sy);
        }

        private void DrawTilePreviewAt(Graphics gfx, int tx, int ty, int z)
        {
            if (tx < 0 || tx >= _compList.Width || ty < 0 || ty >= _compList.Height)
            {
                return;
            }

            Bitmap bmp = _drawTile.GetBitmap();
            if (bmp == null)
            {
                return;
            }

            int px = (tx - ty) * 22 - bmp.Width / 2 - _compList.XMin + MultiEditorComponentList.GapXMod - hScrollBar.Value;
            int py = (tx + ty) * 22 - z * 4 - bmp.Height - _compList.YMin + MultiEditorComponentList.GapYMod - vScrollBar.Value;
            gfx.DrawImage(bmp, new Rectangle(px, py, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height,
                GraphicsUnit.Pixel, MultiTile.DrawColor);
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
                MultiComponentList multi = e.Node.Parent?.Name == "uop"
                    ? Multis.GetUopComponents((int)e.Node.Tag)
                    : Multis.GetComponents((int)e.Node.Tag);

                _compList = new MultiEditorComponentList(multi, this);

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
                        MultiComponentList list = e.Node.Parent?.Name == "uop"
                            ? Multis.GetUopComponents(nodeTag)
                            : Multis.GetComponents(nodeTag);
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

            UndoItems.DropDownItems.Clear();
            for (int i = 0; i < _compList.UndoList.Count; i++)
            {
                var item = new ToolStripMenuItem(_compList.UndoList[i].Action ?? "---")
                {
                    Tag = i
                };
                item.Click += Undo_onClick;
                UndoItems.DropDownItems.Add(item);
            }

            if (UndoItems.DropDownItems.Count == 0)
            {
                UndoItems.DropDownItems.Add(new ToolStripMenuItem("---") { Enabled = false });
            }
        }

        private void UndoList_Clear()
        {
            _compList?.UndoClear();
            UndoItems.DropDownItems.Clear();
            _minimapTilesBmp?.Dispose();
            _minimapTilesBmp = null;
            _minimapDirty = true;
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
            UpdateRecentSelection();
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
            UpdateRecentSelection();
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_compList != null)
            {
                // Undo / Redo
                if (keyData == (Keys.Control | Keys.Z))
                {
                    if (_compList.UndoStep())
                    {
                        UpdateTrackBarAfterZChange();
                        _selectedTiles.Clear();
                        pictureBoxMulti.Invalidate();
                    }

                    return true;
                }

                if (keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z))
                {
                    if (_compList.RedoStep())
                    {
                        UpdateTrackBarAfterZChange();
                        _selectedTiles.Clear();
                        pictureBoxMulti.Invalidate();
                    }

                    return true;
                }

                // Z up/down for selection: [ = -1, ] = +1
                IEnumerable<MultiTile> zTargets = _selectedTiles.Count > 0
                    ? (IEnumerable<MultiTile>)_selectedTiles
                    : (SelectedTile != null ? new[] { SelectedTile } : null);

                if (keyData == Keys.OemOpenBrackets && zTargets != null)
                {
                    _compList.TileZMod(zTargets, -1);
                    UpdateTrackBarAfterZChange();
                    pictureBoxMulti.Invalidate();
                    return true;
                }

                if (keyData == Keys.OemCloseBrackets && zTargets != null)
                {
                    _compList.TileZMod(zTargets, 1);
                    UpdateTrackBarAfterZChange();
                    pictureBoxMulti.Invalidate();
                    return true;
                }

                // Copy / Paste
                if (keyData == (Keys.Control | Keys.C) && _selectedTiles.Count > 0)
                {
                    int ox = _selectedTiles.Min(t => t.X);
                    int oy = _selectedTiles.Min(t => t.Y);
                    int oz = _selectedTiles.Min(t => t.Z);
                    _clipboard = _selectedTiles.Select(t => (t.Id, t.X - ox, t.Y - oy, t.Z - oz)).ToList();
                    return true;
                }

                if (keyData == (Keys.Control | Keys.V) && _clipboard != null)
                {
                    _isPasteMode = true;
                    pictureBoxMulti.Invalidate();
                    return true;
                }

                if (keyData == Keys.Escape && _isPasteMode)
                {
                    _isPasteMode = false;
                    pictureBoxMulti.Invalidate();
                    return true;
                }

                // Navigation: arrow keys pan, PgUp/PgDn step floor
                const int PanStep = 22;
                if (keyData == Keys.Left)  { hScrollBar.Value = Math.Max(0, hScrollBar.Value - PanStep); return true; }
                if (keyData == Keys.Right) { hScrollBar.Value = Math.Min(hScrollBar.Maximum, hScrollBar.Value + PanStep); return true; }
                if (keyData == Keys.Up)    { vScrollBar.Value = Math.Max(0, vScrollBar.Value - PanStep); return true; }
                if (keyData == Keys.Down)  { vScrollBar.Value = Math.Min(vScrollBar.Maximum, vScrollBar.Value + PanStep); return true; }

                if (keyData == Keys.PageUp)
                {
                    numericUpDown_Floor.Value = Math.Min(numericUpDown_Floor.Maximum, numericUpDown_Floor.Value + 5);
                    return true;
                }

                if (keyData == Keys.PageDown)
                {
                    numericUpDown_Floor.Value = Math.Max(numericUpDown_Floor.Minimum, numericUpDown_Floor.Value - 5);
                    return true;
                }

                // Zoom
                if (keyData == Keys.Add || keyData == (Keys.Shift | Keys.Oemplus))
                {
                    SetZoom(_zoomFactor + 0.1f);
                    return true;
                }

                if (keyData == Keys.Subtract || keyData == Keys.OemMinus)
                {
                    SetZoom(_zoomFactor - 0.1f);
                    return true;
                }

                if (keyData == (Keys.Control | Keys.D0) || keyData == (Keys.Control | Keys.NumPad0))
                {
                    SetZoom(1.0f);
                    return true;
                }

                if (ActiveControl is not TextBox && ActiveControl is not NumericUpDown)
                {
                    if (keyData == Keys.S) { BTN_Select_Click(null, EventArgs.Empty); return true; }
                    if (keyData == Keys.D) { BTN_Draw_Click(null, EventArgs.Empty); return true; }
                    if (keyData == Keys.R) { BTN_Remove_Click(null, EventArgs.Empty); return true; }
                    if (keyData == Keys.E) { BTN_Z_Click(null, EventArgs.Empty); return true; }
                    if (keyData == Keys.F) { BTN_Floor.Checked = !BTN_Floor.Checked; return true; }
                    if (keyData == Keys.P) { BTN_Pipette_Click(null, EventArgs.Empty); return true; }
                    if (keyData == Keys.T) { BTN_Trans_Clicked(null, EventArgs.Empty); return true; }
                    if (keyData == Keys.B) { BTN_RectFill_Click(null, EventArgs.Empty); return true; }
                    if (keyData == Keys.L) { BTN_LineDraw_Click(null, EventArgs.Empty); return true; }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Returns the line end constrained to the dominant axis when Shift is held.
        /// </summary>
        private (int ex, int ey) GetLineEnd(int rawEx, int rawEy)
        {
            if ((ModifierKeys & Keys.Shift) == 0)
            {
                return (rawEx, rawEy);
            }

            int dx = Math.Abs(rawEx - _lineStartX);
            int dy = Math.Abs(rawEy - _lineStartY);
            return dx >= dy
                ? (rawEx, _lineStartY)   // horizontal axis
                : (_lineStartX, rawEy);  // vertical axis
        }

        private void SetZoom(float factor)
        {
            _zoomFactor = Math.Clamp(factor, 0.25f, 4.0f);
            toolStripBtnZoom.Text = $"{(int)Math.Round(_zoomFactor * 100)}%";
            pictureBoxMulti.Invalidate();
        }

        private void ToolStripBtnZoom_Click(object sender, EventArgs e)
        {
            SetZoom(1.0f);
        }

        private void ToolStripBtnHelp_Click(object sender, EventArgs e)
        {
            using var form = new ShortcutsHelpForm();
            form.ShowDialog(this);
        }

        private void UpdateTrackBarAfterZChange()
        {
            MaxHeightTrackBar.Minimum = _compList.ZMin;
            MaxHeightTrackBar.Maximum = _compList.ZMax;
            if (MaxHeightTrackBar.Value < _compList.ZMax)
            {
                MaxHeightTrackBar.Value = _compList.ZMax;
            }

            _minimapDirty = true;
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

        private void InitializeRecentTab()
        {
            var recentTab = new TabPage { Text = "Recent" };

            _recentPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(2)
            };

            for (int i = 0; i < TileRecentlyUsed.MaxRecentTiles; i++)
            {
                var slot = new PictureBox
                {
                    Size = new Size(44, 44),
                    BorderStyle = BorderStyle.FixedSingle,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Tag = -1,
                    BackColor = Color.Transparent
                };
                slot.Click += RecentSlot_Click;

                _recentSlots.Add(slot);
                _recentPanel.Controls.Add(slot);
            }

            recentTab.Controls.Add(_recentPanel);
            TC_MultiEditorToolbox.TabPages.Add(recentTab);
        }

        private void RefreshRecentPanel()
        {
            for (int i = 0; i < _recentSlots.Count; i++)
            {
                var slot = _recentSlots[i];
                if (i < _recentTiles.Count)
                {
                    slot.Tag = _recentTiles[i];
                    slot.Image = Ultima.Art.GetStatic(_recentTiles[i]);
                    toolTip1.SetToolTip(slot, $"0x{_recentTiles[i]:X}");
                }
                else
                {
                    slot.Tag = -1;
                    slot.Image = null;
                    toolTip1.SetToolTip(slot, string.Empty);
                }
            }

            UpdateRecentSelection();
        }

        private void UpdateRecentSelection()
        {
            foreach (var slot in _recentSlots)
            {
                slot.BackColor = slot.Tag is int id && id == _drawTile.Id
                    ? Color.LightBlue
                    : Color.Transparent;
            }
        }

        private void RecentSlot_Click(object sender, EventArgs e)
        {
            if (sender is PictureBox pb && pb.Tag is int id && id >= 0)
            {
                _drawTile.Set((ushort)id, 0);
                DrawTileLabel.Text = $"Draw ID: 0x{id:X}";
                PictureBoxDrawTiles_Select();
                pictureBoxDrawTiles.Invalidate();
                UpdateRecentSelection();
            }
        }

        private void RecordRecentTile(int tileId)
        {
            if (tileId < 0)
            {
                return;
            }

            _recentTiles.Remove(tileId);
            _recentTiles.Insert(0, tileId);
            if (_recentTiles.Count > TileRecentlyUsed.MaxRecentTiles)
            {
                _recentTiles.RemoveAt(_recentTiles.Count - 1);
            }

            TileRecentlyUsed.Save(_recentTiles);
            RefreshRecentPanel();
        }

        private void UpdateMinimap()
        {
            if (_compList == null || pictureBoxMinimap.Width <= 0 || pictureBoxMinimap.Height <= 0)
            {
                return;
            }

            int multiW = _compList.XMax - _compList.XMin + MultiEditorComponentList.GapXMod * 2;
            int multiH = _compList.YMaxOrg - _compList.YMin + MultiEditorComponentList.GapYMod * 3;
            if (multiW <= 0 || multiH <= 0)
            {
                return;
            }

            float scaleX = (float)pictureBoxMinimap.Width / multiW;
            float scaleY = (float)pictureBoxMinimap.Height / multiH;
            float scale = Math.Min(scaleX, scaleY);
            if (scale <= 0)
            {
                return;
            }

            // Regenerate tile-dot layer only when tiles changed
            if (_minimapDirty || _minimapTilesBmp == null)
            {
                _minimapDirty = false;
                _minimapTilesBmp?.Dispose();
                _minimapTilesBmp = new Bitmap(pictureBoxMinimap.Width, pictureBoxMinimap.Height);
                using (var gfx = Graphics.FromImage(_minimapTilesBmp))
                {
                    gfx.Clear(Color.DimGray);
                    int dotSize = Math.Max(1, (int)(2 * scale));
                    using (var brush = new SolidBrush(Color.LightGray))
                    {
                        foreach (MultiTile tile in _compList.Tiles)
                        {
                            if (tile.IsVirtualFloor)
                            {
                                continue;
                            }

                            int px = (int)((tile.XMod - _compList.XMin) * scale);
                            int py = (int)((tile.YMod - _compList.YMin) * scale);
                            gfx.FillRectangle(brush, px, py, dotSize, dotSize);
                        }
                    }
                }
            }

            // Composite: tile dots + fresh viewport rect every frame
            var bmp = new Bitmap(pictureBoxMinimap.Width, pictureBoxMinimap.Height);
            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.DrawImageUnscaled(_minimapTilesBmp, 0, 0);

                float vx = hScrollBar.Value * scale;
                float vy = vScrollBar.Value * scale;
                float vw = pictureBoxMulti.Width * scale;
                float vh = pictureBoxMulti.Height * scale;
                gfx.DrawRectangle(Pens.Red, vx, vy, Math.Max(1, vw), Math.Max(1, vh));
            }

            pictureBoxMinimap.Image?.Dispose();
            pictureBoxMinimap.Image = bmp;
        }

        private void PictureBoxMinimap_OnMouseClick(object sender, MouseEventArgs e)
        {
            if (_compList == null)
            {
                return;
            }

            int multiW = _compList.XMax - _compList.XMin + MultiEditorComponentList.GapXMod * 2;
            int multiH = _compList.YMaxOrg - _compList.YMin + MultiEditorComponentList.GapYMod * 3;
            if (multiW <= 0 || multiH <= 0)
            {
                return;
            }

            float scaleX = (float)pictureBoxMinimap.Width / multiW;
            float scaleY = (float)pictureBoxMinimap.Height / multiH;
            float scale = Math.Min(scaleX, scaleY);
            if (scale <= 0)
            {
                return;
            }

            int newHScroll = Math.Clamp((int)(e.X / scale) - pictureBoxMulti.Width / 2, 0, hScrollBar.Maximum);
            int newVScroll = Math.Clamp((int)(e.Y / scale) - pictureBoxMulti.Height / 2, 0, vScrollBar.Maximum);
            hScrollBar.Value = newHScroll;
            vScrollBar.Value = newVScroll;
        }
    }
}