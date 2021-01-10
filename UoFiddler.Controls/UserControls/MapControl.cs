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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class MapControl : UserControl
    {
        public MapControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            if (!Files.CacheData)
            {
                PreloadMap.Visible = false;
            }

            ProgressBar.Visible = false;
            _refMarker = this;
            panel1.Visible = false;

            pictureBox.MouseWheel += OnMouseWheel;
        }

        private static MapControl _refMarker;
        public static double Zoom = 1;

        private Bitmap _map;
        private Map _currMap;
        private int _currMapId;
        private bool _syncWithClient;
        private int _clientX;
        private int _clientY;
        private int _clientZ;
        private int _clientMap;
        private Point _currPoint;
        private bool _moving;
        private Point _movingPoint;
        private bool _renderingZoom;

        public static int HScrollBar => _refMarker.hScrollBar.Value;
        public static int VScrollBar => _refMarker.vScrollBar.Value;
        public static Map CurrMap => _refMarker._currMap;

        private static bool _loaded;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            Zoom = 1;
            _moving = false;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            LoadMapOverlays();
            Options.LoadedUltimaClass["Map"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            _currMap = Map.Felucca;
            feluccaToolStripMenuItem.Checked = true;
            trammelToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            PreloadMap.Visible = true;
            ChangeMapNames();
            ZoomLabel.Text = $"Zoom: {Zoom}";
            SetScrollBarValues();
            Refresh();
            pictureBox.Invalidate();
            Cursor.Current = Cursors.Default;

            if (!_loaded)
            {
                ControlEvents.MapDiffChangeEvent += OnMapDiffChangeEvent;
                ControlEvents.MapNameChangeEvent += OnMapNameChangeEvent;
                ControlEvents.MapSizeChangeEvent += OnMapSizeChangeEvent;
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }
            _loaded = true;
        }

        private void OnMapDiffChangeEvent()
        {
            pictureBox.Invalidate();
        }

        private void OnMapNameChangeEvent()
        {
            ChangeMapNames();
        }

        private void OnMapSizeChangeEvent()
        {
            Reload();
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        public void RefreshMap()
        {
            pictureBox.Invalidate();
        }

        /// <summary>
        /// Changes the Names of maps
        /// </summary>
        private void ChangeMapNames()
        {
            if (!_loaded)
            {
                return;
            }

            feluccaToolStripMenuItem.Text = Options.MapNames[0];
            trammelToolStripMenuItem.Text = Options.MapNames[1];
            ilshenarToolStripMenuItem.Text = Options.MapNames[2];
            malasToolStripMenuItem.Text = Options.MapNames[3];
            tokunoToolStripMenuItem.Text = Options.MapNames[4];
            terMurToolStripMenuItem.Text = Options.MapNames[5];

            if (OverlayObjectTree.Nodes.Count <= 0)
            {
                return;
            }

            OverlayObjectTree.Nodes[0].Text = Options.MapNames[0];
            OverlayObjectTree.Nodes[1].Text = Options.MapNames[1];
            OverlayObjectTree.Nodes[2].Text = Options.MapNames[2];
            OverlayObjectTree.Nodes[3].Text = Options.MapNames[3];
            OverlayObjectTree.Nodes[4].Text = Options.MapNames[4];
            OverlayObjectTree.Nodes[5].Text = Options.MapNames[5];
            OverlayObjectTree.Invalidate();
        }

        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        public static int Round(int x)
        {
            return (x >> 3) << 3;
        }

        private void ZoomMap(ref Bitmap bmp0)
        {
            Bitmap bmp1 = new Bitmap((int)(_map.Width * Zoom), (int)(_map.Height * Zoom));
            Graphics graph = Graphics.FromImage(bmp1);
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;
            graph.PixelOffsetMode = PixelOffsetMode.Half;
            graph.DrawImage(bmp0, new Rectangle(0, 0, bmp1.Width, bmp1.Height));
            graph.Dispose();
            bmp0 = bmp1;
        }

        private void SetScrollBarValues()
        {
            vScrollBar.Minimum = 0;
            hScrollBar.Minimum = 0;
            ChangeScrollBar();
            hScrollBar.LargeChange = 40;
            hScrollBar.SmallChange = 8;
            vScrollBar.LargeChange = 40;
            vScrollBar.SmallChange = 8;
            vScrollBar.Value = 0;
            hScrollBar.Value = 0;
        }

        private void ChangeScrollBar()
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            hScrollBar.Maximum = _currMap.Width;
            hScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Width / Zoom) - 8);
            if (Zoom >= 1)
            {
                hScrollBar.Maximum += (int)(40 * Zoom);
            }
            else if (Zoom < 1)
            {
                hScrollBar.Maximum += (int)(40 / Zoom);
            }

            hScrollBar.Maximum = Math.Max(0, Round(hScrollBar.Maximum));
            vScrollBar.Maximum = _currMap.Height;
            vScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Height / Zoom) - 8);
            if (Zoom >= 1)
            {
                vScrollBar.Maximum += (int)(40 * Zoom);
            }
            else if (Zoom < 1)
            {
                vScrollBar.Maximum += (int)(40 / Zoom);
            }

            vScrollBar.Maximum = Math.Max(0, Round(vScrollBar.Maximum));
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (PreloadWorker.IsBusy)
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            ChangeScrollBar();
            pictureBox.Invalidate();
        }

        private void ChangeMap()
        {
            PreloadMap.Visible = !_currMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            SetScrollBarValues();
            pictureBox.Invalidate();
        }

        private void ResetCheckedMap()
        {
            feluccaToolStripMenuItem.Checked = false;
            trammelToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            terMurToolStripMenuItem.Checked = false;
        }

        private void ChangeMapFelucca(object sender, EventArgs e)
        {
            if (feluccaToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            feluccaToolStripMenuItem.Checked = true;
            _currMap = Map.Felucca;
            _currMapId = 0;
            ChangeMap();
        }

        private void ChangeMapTrammel(object sender, EventArgs e)
        {
            if (trammelToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            trammelToolStripMenuItem.Checked = true;
            _currMap = Map.Trammel;
            _currMapId = 1;
            ChangeMap();
        }

        private void ChangeMapIlshenar(object sender, EventArgs e)
        {
            if (ilshenarToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            ilshenarToolStripMenuItem.Checked = true;
            _currMap = Map.Ilshenar;
            _currMapId = 2;
            ChangeMap();
        }

        private void ChangeMapMalas(object sender, EventArgs e)
        {
            if (malasToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            malasToolStripMenuItem.Checked = true;
            _currMap = Map.Malas;
            _currMapId = 3;
            ChangeMap();
        }

        private void ChangeMapTokuno(object sender, EventArgs e)
        {
            if (tokunoToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            tokunoToolStripMenuItem.Checked = true;
            _currMap = Map.Tokuno;
            _currMapId = 4;
            ChangeMap();
        }

        private void ChangeMapTerMur(object sender, EventArgs e)
        {
            if (terMurToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();
            terMurToolStripMenuItem.Checked = true;
            _currMap = Map.TerMur;
            _currMapId = 5;
            ChangeMap();
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                _moving = true;
                _movingPoint.X = e.X;
                _movingPoint.Y = e.Y;
                Cursor = Cursors.Hand;
            }
            else
            {
                _moving = false;
                Cursor = Cursors.Default;
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            _moving = false;
            Cursor = Cursors.Default;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int xDelta = Math.Min(_currMap.Width, (int)(e.X / Zoom) + Round(hScrollBar.Value));
            int yDelta = Math.Min(_currMap.Height, (int)(e.Y / Zoom) + Round(vScrollBar.Value));

            CoordsLabel.Text = $"Coords: {xDelta},{yDelta}";

            if (!_moving)
            {
                return;
            }

            int deltaX = (int)(-1 * (e.X - _movingPoint.X) / Zoom);
            int deltaY = (int)(-1 * (e.Y - _movingPoint.Y) / Zoom);

            _movingPoint.X = e.X;
            _movingPoint.Y = e.Y;

            hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
            vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));

            pictureBox.Invalidate();
        }

        private void OnClick_ShowClientLoc(object sender, EventArgs e)
        {
            _syncWithClient = !_syncWithClient;
        }

        private void OnClick_GotoClientLoc(object sender, EventArgs e)
        {
            int x = 0;
            int y = 0;
            int z = 0;
            int mapClient = 0;
            if (!Client.Running)
            {
                return;
            }

            Client.Calibrate();
            if (!Client.FindLocation(ref x, ref y, ref z, ref mapClient))
            {
                return;
            }

            if (_currMapId != mapClient)
            {
                ResetCheckedMap();
                SwitchMap(mapClient);
                _currMapId = mapClient;
            }
            _clientX = x;
            _clientY = y;
            _clientZ = z;
            _clientMap = mapClient;
            SetScrollBarValues();
            hScrollBar.Value = (int)Math.Max(0, x - (pictureBox.Right / Zoom / 2));
            vScrollBar.Value = (int)Math.Max(0, y - (pictureBox.Bottom / Zoom / 2));
            pictureBox.Invalidate();
            ClientLocLabel.Text = $"ClientLoc: {x},{y},{z},{Options.MapNames[mapClient]}";
        }

        private void SwitchMap(int mapId)
        {
            switch (mapId)
            {
                case 0:
                    feluccaToolStripMenuItem.Checked = true;
                    _currMap = Map.Felucca;
                    break;
                case 1:
                    trammelToolStripMenuItem.Checked = true;
                    _currMap = Map.Trammel;
                    break;
                case 2:
                    ilshenarToolStripMenuItem.Checked = true;
                    _currMap = Map.Ilshenar;
                    break;
                case 3:
                    malasToolStripMenuItem.Checked = true;
                    _currMap = Map.Malas;
                    break;
                case 4:
                    tokunoToolStripMenuItem.Checked = true;
                    _currMap = Map.Tokuno;
                    break;
                case 5:
                    terMurToolStripMenuItem.Checked = true;
                    _currMap = Map.TerMur;
                    break;
            }
        }

        private void SyncClientTimer(object sender, EventArgs e)
        {
            if (!_syncWithClient)
            {
                return;
            }

            int x = 0;
            int y = 0;
            int z = 0;
            int mapClient = 0;
            string mapName = "";
            if (Client.Running)
            {
                Client.Calibrate();
                if (Client.FindLocation(ref x, ref y, ref z, ref mapClient))
                {
                    if (_clientX == x && _clientY == y && _clientZ == z && _clientMap == mapClient)
                    {
                        return;
                    }

                    _clientX = x;
                    _clientY = y;
                    _clientZ = z;
                    _clientMap = mapClient;
                    mapName = Options.MapNames[mapClient];
                }
            }

            ClientLocLabel.Text = $"ClientLoc: {x},{y},{z},{mapName}";
            pictureBox.Invalidate();
        }

        private void GetMapInfo(object sender, EventArgs e)
        {
            new MapDetailsForm(_currMap, _currPoint).Show();
        }

        private void OnOpenContext(object sender, CancelEventArgs e)  // Save for GetMapInfo
        {
            _currPoint = pictureBox.PointToClient(MousePosition);
            _currPoint.X = (int)(_currPoint.X / Zoom);
            _currPoint.Y = (int)(_currPoint.Y / Zoom);
            _currPoint.X += Round(hScrollBar.Value);
            _currPoint.Y += Round(vScrollBar.Value);
        }

        private void OnContextClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnDropDownClosed(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (_renderingZoom)
            {
                return;
            }

            //Needed to update current position of the cursor
            OnOpenContext(sender, null);

            //Scrolling goes up
            if (e.Delta > 0)
            {
                OnZoomPlus(sender, null);
            }
            //Scrolling goes down
            else
            {
                OnZoomMinus(sender, null);
            }
        }

        private void OnZoomMinus(object sender, EventArgs e)
        {
            if (Zoom / 2 < 0.25)
            {
                return;
            }

            Zoom /= 2;
            DoZoom();
        }

        private void OnZoomPlus(object sender, EventArgs e)
        {
            if (Zoom * 2 > 4)
            {
                return;
            }

            Zoom *= 2;
            DoZoom();
        }

        private void DoZoom()
        {
            if (_renderingZoom)
            {
                return;
            }

            _renderingZoom = true;
            ChangeScrollBar();
            ZoomLabel.Text = $"Zoom: {Zoom}";
            int x = Math.Max(0, _currPoint.X - ((int)(pictureBox.ClientSize.Width / Zoom) / 2));
            int y = Math.Max(0, _currPoint.Y - ((int)(pictureBox.ClientSize.Height / Zoom) / 2));
            x = Math.Min(x, hScrollBar.Maximum);
            y = Math.Min(y, vScrollBar.Maximum);
            hScrollBar.Value = Round(x);
            vScrollBar.Value = Round(y);
            pictureBox.Invalidate();
            _renderingZoom = false;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (PreloadWorker.IsBusy)
            {
                e.Graphics.DrawString("Preloading map. Please wait...", SystemFonts.DefaultFont, Brushes.Black, 60, 60);
                return;
            }

            _map = _currMap.GetImage(hScrollBar.Value >> 3, vScrollBar.Value >> 3,
                (int)((e.ClipRectangle.Width / Zoom) + 8) >> 3, (int)((e.ClipRectangle.Height / Zoom) + 8) >> 3,
                showStaticsToolStripMenuItem1.Checked);
            ZoomMap(ref _map);
            e.Graphics.DrawImageUnscaledAndClipped(_map, e.ClipRectangle);

            if (showCenterCrossToolStripMenuItem1.Checked)
            {
                using (Brush brush = new SolidBrush(Color.FromArgb(180, Color.White)))
                using (Pen pen = new Pen(brush))
                {
                    int x = Round(pictureBox.Width / 2);
                    int y = Round(pictureBox.Height / 2);

                    e.Graphics.DrawLine(pen, x - 4, y, x + 4, y);
                    e.Graphics.DrawLine(pen, x, y - 4, x, y + 4);
                }
            }

            if (showClientCrossToolStripMenuItem.Checked && Client.Running)
            {
                if (_clientX > hScrollBar.Value &&
                    _clientX < hScrollBar.Value + (e.ClipRectangle.Width / Zoom) &&
                    _clientY > vScrollBar.Value &&
                    _clientY < vScrollBar.Value + (e.ClipRectangle.Height / Zoom) &&
                    _clientMap == _currMapId)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(180, Color.Yellow)))
                    using (Pen pen = new Pen(brush))
                    {
                        int x = (int)((_clientX - Round(hScrollBar.Value)) * Zoom);
                        int y = (int)((_clientY - Round(vScrollBar.Value)) * Zoom);

                        e.Graphics.DrawLine(pen, x - 4, y, x + 4, y);
                        e.Graphics.DrawLine(pen, x, y - 4, x, y + 4);

                        e.Graphics.DrawEllipse(pen, x - 2, y - 2, 2 * 2, 2 * 2);
                    }
                }
            }

            if (OverlayObjectTree.Nodes.Count <= 0 || !showMarkersToolStripMenuItem.Checked)
            {
                return;
            }

            foreach (TreeNode obj in OverlayObjectTree.Nodes[_currMapId].Nodes)
            {
                OverlayObject o = (OverlayObject)obj.Tag;
                if (o.IsVisible(e.ClipRectangle, _currMapId))
                {
                    o.Draw(e.Graphics);
                }
            }
        }

        private void OnKeyDownGoto(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            string line = TextBoxGoto.Text.Trim();
            if (line.Length > 0)
            {
                string[] args = line.Split(' ');
                if (args.Length != 2)
                {
                    args = line.Split(',');
                }

                if (args.Length == 2 && int.TryParse(args[0], out int x) && int.TryParse(args[1], out int y))
                {
                    if (x >= 0 && y >= 0 && x <= _currMap.Width && x <= _currMap.Height)
                    {
                        contextMenuStrip1.Close();
                        hScrollBar.Value = (int)Math.Max(0, x - (pictureBox.Right / Zoom / 2));
                        vScrollBar.Value = (int)Math.Max(0, y - (pictureBox.Bottom / Zoom / 2));
                    }
                }
            }
            pictureBox.Invalidate();
        }

        private void OnClickSendClient(object sender, EventArgs e)
        {
            if (!Client.Running)
            {
                return;
            }

            int x = Round((int)(pictureBox.Width / Zoom / 2));
            int y = Round((int)(pictureBox.Height / Zoom / 2));
            x += Round(hScrollBar.Value);
            y += Round(vScrollBar.Value);
            SendCharTo(x, y);
        }

        private void OnClickSendClientToPos(object sender, EventArgs e)
        {
            if (Client.Running)
            {
                SendCharTo(_currPoint.X, _currPoint.Y);
            }
        }

        private void SendCharTo(int x, int y)
        {
            string format = "{0} " + Options.MapArgs;
            int z = _currMap.Tiles.GetLandTile(x, y).Z;
            Client.SendText(string.Format(format, Options.MapCmd, x, y, z, _currMapId, Options.MapNames[_currMapId]));
        }

        private void ExtractMapBmp(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Bmp);
        }

        private void ExtractMapTiff(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Tiff);
        }

        private void ExtractMapJpg(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Jpeg);
        }

        private void ExtractMapPng(object sender, EventArgs e)
        {
            ExtractMapImage(ImageFormat.Png);
        }

        private void ExtractMapImage(ImageFormat imageFormat)
        {
            Cursor.Current = Cursors.WaitCursor;

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{Options.MapNames[_currMapId]}.{fileExtension}");

            try
            {
                Bitmap extract = _currMap.GetImage(0, 0, _currMap.Width >> 3, _currMap.Height >> 3, showStaticsToolStripMenuItem1.Checked);

                if (showMarkersToolStripMenuItem.Checked)
                {
                    Graphics g = Graphics.FromImage(extract);
                    foreach (TreeNode obj in OverlayObjectTree.Nodes[_currMapId].Nodes)
                    {
                        OverlayObject o = (OverlayObject)obj.Tag;
                        if (o.Visible)
                        {
                            o.Draw(g);
                        }
                    }
                    g.Save();
                }
                extract.Save(fileName, imageFormat);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }

            MessageBox.Show($"Map saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private MapMarkerForm _mapMarkerForm;

        private void OnClickInsertMarker(object sender, EventArgs e)
        {
            if (_mapMarkerForm?.IsDisposed == false)
            {
                return;
            }

            _mapMarkerForm = new MapMarkerForm(_currPoint.X, _currPoint.Y, _currMapId)
            {
                TopMost = true
            };

            _mapMarkerForm.Show();
        }

        public static void AddOverlay(int x, int y, int mapId, Color color, string text)
        {
            OverlayCursor o = new OverlayCursor(new Point(x, y), mapId, text, color);
            TreeNode node = new TreeNode(text)
            {
                Tag = o
            };
            _refMarker.OverlayObjectTree.Nodes[mapId].Nodes.Add(node);
            _refMarker.pictureBox.Invalidate();
        }

        private void LoadMapOverlays()
        {
            OverlayObjectTree.BeginUpdate();
            try
            {
                OverlayObjectTree.Nodes.Clear();

                AddOverlayGroups();

                string fileName = Path.Combine(Options.AppDataPath, "MapOverlays.xml");
                if (!File.Exists(fileName))
                {
                    return;
                }

                XmlDocument dom = new XmlDocument();
                dom.Load(fileName);
                XmlElement xOptions = dom["Overlays"];
                var markers = xOptions?.SelectNodes("Marker");
                if (markers == null)
                {
                    return;
                }

                foreach (XmlElement element in markers)
                {
                    int x = int.Parse(element.GetAttribute("x"));
                    int y = int.Parse(element.GetAttribute("y"));
                    int m = int.Parse(element.GetAttribute("map"));
                    int c = int.Parse(element.GetAttribute("color"));
                    string text = element.GetAttribute("text");
                    OverlayCursor o = new OverlayCursor(new Point(x, y), m, text, Color.FromArgb(c));
                    TreeNode node = new TreeNode(text) {Tag = o};
                    OverlayObjectTree.Nodes[m].Nodes.Add(node);
                }
            }
            finally
            {
                OverlayObjectTree.EndUpdate();
            }
        }

        private void AddOverlayGroups()
        {
            TreeNode node = new TreeNode(Options.MapNames[0])
            {
                Tag = 0
            };
            OverlayObjectTree.Nodes.Add(node);

            node = new TreeNode(Options.MapNames[1])
            {
                Tag = 1
            };
            OverlayObjectTree.Nodes.Add(node);

            node = new TreeNode(Options.MapNames[2])
            {
                Tag = 2
            };
            OverlayObjectTree.Nodes.Add(node);

            node = new TreeNode(Options.MapNames[3])
            {
                Tag = 3
            };
            OverlayObjectTree.Nodes.Add(node);

            node = new TreeNode(Options.MapNames[4])
            {
                Tag = 4
            };
            OverlayObjectTree.Nodes.Add(node);

            node = new TreeNode(Options.MapNames[5])
            {
                Tag = 5
            };
            OverlayObjectTree.Nodes.Add(node);
        }

        public static void SaveMapOverlays()
        {
            if (!_loaded)
            {
                return;
            }

            string filepath = Options.AppDataPath;

            string fileName = Path.Combine(filepath, "MapOverlays.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Overlays");
            bool entries = false;
            for (int i = 0; i < 5; ++i)
            {
                foreach (TreeNode obj in _refMarker.OverlayObjectTree.Nodes[i].Nodes)
                {
                    OverlayObject o = (OverlayObject)obj.Tag;
                    XmlElement elem = dom.CreateElement("Marker");
                    o.Save(elem);
                    sr.AppendChild(elem);
                    entries = true;
                }
            }
            dom.AppendChild(sr);

            if (entries)
            {
                dom.Save(fileName);
            }
        }

        private void OnClickPreloadMap(object sender, EventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = (_currMap.Width >> 3) * (_currMap.Height >> 3);
            ProgressBar.Step = 1;
            ProgressBar.Value = 0;
            ProgressBar.Visible = true;
            PreloadWorker.RunWorkerAsync(new object[] { _currMap, showStaticsToolStripMenuItem1.Checked });
        }

        private void PreLoadDoWork(object sender, DoWorkEventArgs e)
        {
            //Ultima.Map workmap = (Ultima.Map)((object[])e.Argument)[0]; // TODO: unused variable?
            bool statics = (bool)((object[])e.Argument)[1];
            int width = _currMap.Width >> 3;
            int height = _currMap.Height >> 3;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    _currMap.PreloadRenderedBlock(x, y, statics);
                    PreloadWorker.ReportProgress(1);
                }
            }
        }

        private void PreLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }

        private void PreLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
            PreloadMap.Visible = false;
            pictureBox.Invalidate();
        }

        private void OnDoubleClickMarker(object sender, TreeNodeMouseClickEventArgs e)
        {
            OnClickGotoMarker(this, null);
        }

        private void OnClickGotoMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            OverlayObject o = (OverlayObject)OverlayObjectTree.SelectedNode.Tag;
            if (_currMapId != o.DefMap)
            {
                ResetCheckedMap();
                SwitchMap(o.DefMap);
                _currMapId = o.DefMap;
            }
            SetScrollBarValues();
            hScrollBar.Value = (int)Math.Max(0, o.Loc.X - (pictureBox.Right / Zoom / 2));
            vScrollBar.Value = (int)Math.Max(0, o.Loc.Y - (pictureBox.Bottom / Zoom / 2));
            pictureBox.Invalidate();
        }

        private void OnClickRemoveMarker(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            OverlayObjectTree.SelectedNode.Remove();
            pictureBox.Invalidate();
        }

        private void OnClickSwitchVisible(object sender, EventArgs e)
        {
            if (OverlayObjectTree.SelectedNode?.Parent == null)
            {
                return;
            }

            OverlayObject o = (OverlayObject)OverlayObjectTree.SelectedNode.Tag;
            o.Visible = !o.Visible;
            OverlayObjectTree.SelectedNode.ForeColor = !o.Visible ? Color.Red : Color.Black;

            OverlayObjectTree.Invalidate();
            pictureBox.Invalidate();
        }

        private void OnChangeView(object sender, EventArgs e)
        {
            PreloadMap.Visible = !_currMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            pictureBox.Invalidate();
        }

        private void OnClickDefragStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.DefragStatics(Options.OutputPath,
                _currMap, _currMap.Width, _currMap.Height, false);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Statics saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickDefragRemoveStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.DefragStatics(Options.OutputPath,
                _currMap, _currMap.Width, _currMap.Height, true);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Statics saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnResizeMap(object sender, EventArgs e)
        {
            if (PreloadWorker.IsBusy)
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            ChangeScrollBar();
            pictureBox.Invalidate();
        }

        private void OnClickRewriteMap(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Map.RewriteMap(Options.OutputPath,
                _currMapId, _currMap.Width, _currMap.Height);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Map saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickReportInvisStatics(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currMap.ReportInvisStatics(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Report saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickReportInvalidMapIDs(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _currMap.ReportInvalidMapIDs(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Report saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private MapReplaceForm _showForm;

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new MapReplaceForm(_currMap)
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private MapDiffInsertForm _showFormMapDiff;

        private void OnClickInsertDiffData(object sender, EventArgs e)
        {
            if (_showFormMapDiff?.IsDisposed == false)
            {
                return;
            }

            _showFormMapDiff = new MapDiffInsertForm(_currMap)
            {
                TopMost = true
            };
            _showFormMapDiff.Show();
        }

        private void OnClickStaticImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Select WSC Static file to import",
                Multiselect = false,
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                dialog.Dispose();
                return;
            }

            string path = dialog.FileName;
            dialog.Dispose();
            StaticImport(path);
        }

        private void StaticImport(string filename)
        {
            StreamReader ip = new StreamReader(filename);

            string line;
            StaticTile newTile = new StaticTile
            {
                Id = 0xFFFF,
                Hue = 0
            };

            int blockY = 0;
            int blockX = 0;
            while ((line = ip.ReadLine()) != null)
            {
                if ((line = line.Trim()).Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
                {
                    continue;
                }

                try
                {
                    if (line.StartsWith("SECTION WORLDITEM"))
                    {
                        if (newTile.Id != 0xFFFF)
                        {
                            _currMap.Tiles.AddPendingStatic(blockX, blockY, newTile);
                            blockX = blockY = 0;
                        }
                        newTile = new StaticTile
                        {
                            Id = 0xFFFF,
                            Hue = 0
                        };
                    }
                    else if (line.StartsWith("ID"))
                    {
                        line = line.Remove(0, 2);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newTile.Id = Art.GetLegalItemID(Convert.ToUInt16(line));
                    }
                    else if (line.StartsWith("X"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        int x = Convert.ToInt32(line);
                        blockX = x >> 3;
                        x &= 0x7;
                        newTile.X = (byte)x;
                    }
                    else if (line.StartsWith("Y"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        int y = Convert.ToInt32(line);
                        blockY = y >> 3;
                        y &= 0x7;
                        newTile.Y = (byte)y;
                    }
                    else if (line.StartsWith("Z"))
                    {
                        line = line.Remove(0, 1);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newTile.Z = Convert.ToSByte(line);
                    }
                    else if (line.StartsWith("COLOR"))
                    {
                        line = line.Remove(0, 5);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        newTile.Hue = Convert.ToInt16(line);
                    }
                }
                catch
                {
                    // TODO: add logging?
                    // ignored
                }
            }
            if (newTile.Id != 0xFFFF)
            {
                _currMap.Tiles.AddPendingStatic(blockX, blockY, newTile);
            }

            ip.Close();

            MessageBox.Show("Done", "Freeze Static", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            _currMap.ResetCache();
            pictureBox.Invalidate();
        }

        private MapMeltStaticsForm _showMeltStaticsForm;

        private void OnClickMeltStatics(object sender, EventArgs e)
        {
            if (_showMeltStaticsForm?.IsDisposed == false)
            {
                return;
            }

            _showMeltStaticsForm = new MapMeltStaticsForm(this, _currMap)
            {
                TopMost = true
            };
            _showMeltStaticsForm.Show();
        }

        private MapClearStaticsForm _showClearStaticsForm;

        private void OnClickClearStatics(object sender, EventArgs e)
        {
            if (_showClearStaticsForm?.IsDisposed == false)
            {
                return;
            }

            _showClearStaticsForm = new MapClearStaticsForm(this, _currMap)
            {
                TopMost = true
            };
            _showClearStaticsForm.Show();
        }

        private MapReplaceTilesForm _showMapReplaceTilesForm;

        private void OnClickReplaceTiles(object sender, EventArgs e)
        {
            if (_showMapReplaceTilesForm?.IsDisposed == false)
            {
                return;
            }

            _showMapReplaceTilesForm = new MapReplaceTilesForm(_currMap)
            {
                TopMost = true
            };
            _showMapReplaceTilesForm.Show();
        }
    }

    public class OverlayObject
    {
        public virtual bool IsVisible(Rectangle bounds, int m) { return false; }
        public virtual void Draw(Graphics g) { }
        public virtual void Save(XmlElement elem) { }
        public override string ToString() { return ""; }

        public bool Visible { get; set; }
        public Point Loc { get; protected set; }
        public int DefMap { get; protected set; }
    }

    public class OverlayCursor : OverlayObject, IDisposable
    {
        private readonly string _text;
        private readonly Color _col;
        private readonly Pen _pen;
        private static Brush _background;

        public OverlayCursor(Point location, int m, string t, Color c)
        {
            Loc = location;
            DefMap = m;
            _text = t;
            _col = c;
            Visible = true;
            Brush brush = new SolidBrush(_col);
            _pen = new Pen(brush);
            _background = new SolidBrush(Color.FromArgb(100, Color.White));
        }

        public override bool IsVisible(Rectangle bounds, int m)
        {
            if (!Visible)
            {
                return false;
            }

            if (DefMap != m)
            {
                return false;
            }

            return Loc.X > MapControl.HScrollBar &&
                Loc.X < MapControl.HScrollBar + (bounds.Width / MapControl.Zoom) &&
                Loc.Y > MapControl.VScrollBar &&
                Loc.Y < MapControl.VScrollBar + (bounds.Height / MapControl.Zoom);
        }

        public override void Draw(Graphics g)
        {
            int x = (int)((Loc.X - MapControl.Round(MapControl.HScrollBar)) * MapControl.Zoom);
            int y = (int)((Loc.Y - MapControl.Round(MapControl.VScrollBar)) * MapControl.Zoom);
            g.DrawLine(_pen, x - 4, y, x + 4, y);
            g.DrawLine(_pen, x, y - 4, x, y + 4);
            g.DrawEllipse(_pen, x - 2, y - 2, 2 * 2, 2 * 2);
            SizeF tSize = g.MeasureString(_text, Control.DefaultFont);
            int xStr = Loc.X + tSize.Width > MapControl.CurrMap.Width ? x - (int)tSize.Width - 6 : x + 6;
            g.FillRectangle(_background, xStr, y - tSize.Height, tSize.Width, tSize.Height);
            g.DrawString(_text, Control.DefaultFont, Brushes.Black, xStr, y - tSize.Height);
        }

        public override void Save(XmlElement elem)
        {
            elem.SetAttribute("x", Loc.X.ToString());
            elem.SetAttribute("y", Loc.Y.ToString());
            elem.SetAttribute("map", DefMap.ToString());
            elem.SetAttribute("color", _col.ToArgb().ToString());
            elem.SetAttribute("text", _text);
        }

        public override string ToString()
        {
            return _text;
        }

        public void Dispose()
        {
            _pen?.Dispose();
            _background?.Dispose();
        }
    }
}
