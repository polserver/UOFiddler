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

            // Add intensity submenu items to the Normal + Altitude menu
            AddAltitudeIntensityMenuItems();
        }

        private void AddAltitudeIntensityMenuItems()
        {
            // Create preset menu items at the top
            var sharpPresetItem = new ToolStripMenuItem("Sharp (High Contrast)") { Tag = "preset_sharp" };
            sharpPresetItem.Click += OnAltitudePresetChanged;

            var normalPresetItem = new ToolStripMenuItem("Normal (More Contrast)") { Tag = "preset_normal", Checked = true };
            normalPresetItem.Click += OnAltitudePresetChanged;

            var softPresetItem = new ToolStripMenuItem("Soft (Subtle)") { Tag = "preset_soft" };
            softPresetItem.Click += OnAltitudePresetChanged;

            // Separator between presets and intensity controls
            var presetSeparator = new ToolStripSeparator();

            // Create intensity preset menu items
            var subtleItem = new ToolStripMenuItem("    • Subtle Intensity") { Tag = 15, Checked = true };
            subtleItem.Click += OnAltitudeIntensityChanged;

            var normalItem = new ToolStripMenuItem("    • Normal Intensity") { Tag = 10 };
            normalItem.Click += OnAltitudeIntensityChanged;

            var strongItem = new ToolStripMenuItem("    • Strong Intensity") { Tag = 5 };
            strongItem.Click += OnAltitudeIntensityChanged;

                        // Add all items to Normal + Altitude menu
            altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems.Add(sharpPresetItem);
            altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems.Add(normalPresetItem);
            altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems.Add(softPresetItem);
            altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems.Add(presetSeparator);
            altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems.Add(subtleItem);
            altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems.Add(normalItem);
            altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems.Add(strongItem);
                    }

        private static MapControl _refMarker;
        public static double Zoom = 1;

        private Bitmap _map;
        private Bitmap _renderBuffer;
        private Bitmap _zoomBuffer;
        private Graphics _zoomBufferGraphics;
        private PixelFormat _renderBufferFormat;
        private int _currentMapId;
        private bool _syncWithClient;
        private int _clientX;
        private int _clientY;
        private int _clientZ;
        private int _clientMap;
        private Point _currentPoint;
        private bool _moving;
        private Point _movingPoint;
        private bool _renderingZoom;
        private MapAltitudeMode _altitudeMode = MapAltitudeMode.Normal;
        private readonly System.Diagnostics.Stopwatch _dragRepaintTimer = new System.Diagnostics.Stopwatch();
        private System.Windows.Forms.Timer _dragTrailTimer;
        private bool _dragInvalidatePending;
        private double _dragAccumX;
        private double _dragAccumY;
        private int _preloadValue;
        private int _preloadMax;

        private int HScrollBar => hScrollBar.Value;
        private int VScrollBar => vScrollBar.Value;
        public Map CurrentMap { get; private set; }

        private static bool _loaded;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
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
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                LoadMapOverlays();
                Options.LoadedUltimaClass["Map"] = true;
                Options.LoadedUltimaClass["RadarColor"] = true;

                CurrentMap = Map.Felucca;
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
            }

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

        private Bitmap ZoomMap(Bitmap source, double effectiveZoom)
        {
            int targetWidth = (int)(source.Width * effectiveZoom);
            int targetHeight = (int)(source.Height * effectiveZoom);

            if (targetWidth <= 0 || targetHeight <= 0)
            {
                return source;
            }

            if (_zoomBuffer == null || _zoomBuffer.Width != targetWidth || _zoomBuffer.Height != targetHeight)
            {
                _zoomBufferGraphics?.Dispose();
                _zoomBuffer?.Dispose();
                _zoomBuffer = new Bitmap(targetWidth, targetHeight, PixelFormat.Format32bppArgb);
                _zoomBufferGraphics = Graphics.FromImage(_zoomBuffer);
                _zoomBufferGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                _zoomBufferGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            }

            _zoomBufferGraphics.DrawImage(source, new Rectangle(0, 0, targetWidth, targetHeight));
            return _zoomBuffer;
        }

        private Bitmap EnsureRenderBuffer(int pixelWidth, int pixelHeight, PixelFormat format)
        {
            if (_renderBuffer != null
                && _renderBuffer.Width == pixelWidth
                && _renderBuffer.Height == pixelHeight
                && _renderBufferFormat == format)
            {
                return _renderBuffer;
            }

            _renderBuffer?.Dispose();
            _renderBuffer = new Bitmap(pixelWidth, pixelHeight, format);
            _renderBufferFormat = format;

            if (format == PixelFormat.Format8bppIndexed)
            {
                ColorPalette palette = _renderBuffer.Palette;
                for (int i = 0; i < 256; i++)
                {
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                }
                _renderBuffer.Palette = palette;
            }

            return _renderBuffer;
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

            hScrollBar.Maximum = CurrentMap.Width;
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
            vScrollBar.Maximum = CurrentMap.Height;
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
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
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
            PreloadMap.Visible = !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);
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
            CurrentMap = Map.Felucca;
            _currentMapId = 0;
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
            CurrentMap = Map.Trammel;
            _currentMapId = 1;
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
            CurrentMap = Map.Ilshenar;
            _currentMapId = 2;
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
            CurrentMap = Map.Malas;
            _currentMapId = 3;
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
            CurrentMap = Map.Tokuno;
            _currentMapId = 4;
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
            CurrentMap = Map.TerMur;
            _currentMapId = 5;
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
                _dragAccumX = 0;
                _dragAccumY = 0;
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

        private const int DragRepaintIntervalMs = 16;

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int xDelta = Math.Min(CurrentMap.Width, (int)(e.X / Zoom) + Round(hScrollBar.Value));
            int yDelta = Math.Min(CurrentMap.Height, (int)(e.Y / Zoom) + Round(vScrollBar.Value));

            CoordsLabel.Text = $"Coords: {xDelta},{yDelta}";

            if (!_moving)
            {
                return;
            }

            // Accumulate the fractional part of the drag so high-zoom drags (where 1 mouse pixel
            // is less than 1 tile) don't lose precision: at Zoom = 4, a 1px mouse move is 0.25
            // tiles and int-truncates to 0 without an accumulator.
            _dragAccumX += -(e.X - _movingPoint.X) / Zoom;
            _dragAccumY += -(e.Y - _movingPoint.Y) / Zoom;

            int deltaX = (int)_dragAccumX;
            int deltaY = (int)_dragAccumY;
            _dragAccumX -= deltaX;
            _dragAccumY -= deltaY;

            _movingPoint.X = e.X;
            _movingPoint.Y = e.Y;

            if (deltaX == 0 && deltaY == 0)
            {
                return;
            }

            hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
            vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));

            RequestDragRepaint();
        }

        private void RequestDragRepaint()
        {
            if (!_dragRepaintTimer.IsRunning || _dragRepaintTimer.ElapsedMilliseconds >= DragRepaintIntervalMs)
            {
                _dragInvalidatePending = false;
                _dragRepaintTimer.Restart();
                pictureBox.Invalidate();
                return;
            }

            // Coalesce into a trailing-edge repaint so the final position always lands on screen.
            if (_dragInvalidatePending)
            {
                return;
            }

            _dragInvalidatePending = true;
            if (_dragTrailTimer == null)
            {
                _dragTrailTimer = new System.Windows.Forms.Timer { Interval = DragRepaintIntervalMs };
                _dragTrailTimer.Tick += OnDragTrailTick;
            }
            _dragTrailTimer.Stop();
            _dragTrailTimer.Interval = Math.Max(1, DragRepaintIntervalMs - (int)_dragRepaintTimer.ElapsedMilliseconds);
            _dragTrailTimer.Start();
        }

        private void OnDragTrailTick(object sender, EventArgs e)
        {
            _dragTrailTimer.Stop();
            if (!_dragInvalidatePending)
            {
                return;
            }
            _dragInvalidatePending = false;
            _dragRepaintTimer.Restart();
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

            if (_currentMapId != mapClient)
            {
                ResetCheckedMap();
                SwitchMap(mapClient);
                _currentMapId = mapClient;
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
                    CurrentMap = Map.Felucca;
                    break;
                case 1:
                    trammelToolStripMenuItem.Checked = true;
                    CurrentMap = Map.Trammel;
                    break;
                case 2:
                    ilshenarToolStripMenuItem.Checked = true;
                    CurrentMap = Map.Ilshenar;
                    break;
                case 3:
                    malasToolStripMenuItem.Checked = true;
                    CurrentMap = Map.Malas;
                    break;
                case 4:
                    tokunoToolStripMenuItem.Checked = true;
                    CurrentMap = Map.Tokuno;
                    break;
                case 5:
                    terMurToolStripMenuItem.Checked = true;
                    CurrentMap = Map.TerMur;
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
            new MapDetailsForm(CurrentMap, _currentPoint).Show();
        }

        private void OnOpenContext(object sender, CancelEventArgs e)  // Save for GetMapInfo
        {
            _currentPoint = pictureBox.PointToClient(MousePosition);
            _currentPoint.X = (int)(_currentPoint.X / Zoom);
            _currentPoint.Y = (int)(_currentPoint.Y / Zoom);
            _currentPoint.X += Round(hScrollBar.Value);
            _currentPoint.Y += Round(vScrollBar.Value);
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
            int x = Math.Max(0, _currentPoint.X - ((int)(pictureBox.ClientSize.Width / Zoom) / 2));
            int y = Math.Max(0, _currentPoint.Y - ((int)(pictureBox.ClientSize.Height / Zoom) / 2));
            x = Math.Min(x, hScrollBar.Maximum);
            y = Math.Min(y, vScrollBar.Maximum);
            hScrollBar.Value = Round(x);
            vScrollBar.Value = Round(y);
            pictureBox.Invalidate();
            _renderingZoom = false;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (PreloadWorker.IsBusy)
            {
                e.Graphics.Clear(pictureBox.BackColor);
                const int textX = 60;
                const int textY = 60;
                string msg = "Preloading map. Please wait...";
                e.Graphics.DrawString(msg, SystemFonts.DefaultFont, SystemBrushes.ControlText, textX, textY);

                if (_preloadMax > 0)
                {
                    SizeF textSize = e.Graphics.MeasureString(msg, SystemFonts.DefaultFont);
                    int barX = textX;
                    int barY = textY + (int)textSize.Height + 6;
                    int barW = Math.Max(200, (int)textSize.Width);
                    const int barH = 14;

                    e.Graphics.FillRectangle(SystemBrushes.ControlDark, barX, barY, barW, barH);
                    int fillW = (int)((long)barW * _preloadValue / _preloadMax);
                    if (fillW > 0)
                    {
                        e.Graphics.FillRectangle(SystemBrushes.Highlight, barX, barY, fillW, barH);
                    }
                    e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, barX, barY, barW, barH);
                }
                return;
            }

            bool statics = showStaticsToolStripMenuItem1.Checked;
            int blockX = hScrollBar.Value >> 3;
            int blockY = vScrollBar.Value >> 3;
            // +16 (2 blocks of padding) so the sub-block scroll offset never reveals empty space
            // along the right/bottom edge of the viewport.
            int widthBlocks = ((int)Math.Ceiling(e.ClipRectangle.Width / Zoom) + 16) >> 3;
            int heightBlocks = ((int)Math.Ceiling(e.ClipRectangle.Height / Zoom) + 16) >> 3;

            // Mipmap selection: only kicks in for color modes (Normal / NormalWithAltitude),
            // not for pure 8bpp Altitude grayscale.
            bool colorMode = _altitudeMode != MapAltitudeMode.Altitude;
            int mipShift; // bits to shift block index to pixel size (3=full,2=half,1=quarter)
            if (colorMode && Zoom <= 0.25)
            {
                mipShift = 1;
            }
            else if (colorMode && Zoom <= 0.5)
            {
                mipShift = 2;
            }
            else
            {
                mipShift = 3;
            }

            // When using a mip, the rendered bitmap is already at the correct screen size for that
            // zoom level; only the residual factor (1.0) needs to go through ZoomMap, so we skip it.
            // For zoom != mip-native and zoom > 0.5 (i.e. zoom 1, 2, 4), we still need ZoomMap.
            double mipScale = 1 << mipShift; // 8, 4, or 2 pixels per block at this resolution
            double effectiveZoom = Zoom * 8.0 / mipScale;

            PixelFormat targetFormat = _altitudeMode == MapAltitudeMode.Altitude
                ? PixelFormat.Format8bppIndexed
                : PixelFormat.Format16bppRgb555;

            int bufferPixelW = widthBlocks << mipShift;
            int bufferPixelH = heightBlocks << mipShift;
            _map = EnsureRenderBuffer(bufferPixelW, bufferPixelH, targetFormat);

            if (mipShift == 3)
            {
                if (_altitudeMode != MapAltitudeMode.Normal)
                {
                    CurrentMap.GetImageWithAltitude(blockX, blockY, widthBlocks, heightBlocks, _map, statics, _altitudeMode);
                }
                else
                {
                    CurrentMap.GetImage(blockX, blockY, widthBlocks, heightBlocks, _map, statics);
                }
            }
            else if (mipShift == 2)
            {
                CurrentMap.GetImageHalf(blockX, blockY, widthBlocks, heightBlocks, _map, statics);
            }
            else
            {
                CurrentMap.GetImageQuarter(blockX, blockY, widthBlocks, heightBlocks, _map, statics);
            }

            MessageLabel.Text = CurrentMap.Tiles.AllFilesExist() ? "" : "One of map files is missing!";

            Bitmap toDraw;
            if (Math.Abs(effectiveZoom - 1.0) < 1e-6)
            {
                toDraw = _map;
            }
            else
            {
                toDraw = ZoomMap(_map, effectiveZoom);
            }

            // The render buffer starts at the block boundary (blockX * 8). Shift the draw position
            // by the sub-block portion of the scroll so viewport pixel 0 maps to the exact tile
            // the scrollbar points at. Without this, dragging at zoom > 1 looks "stuck" between
            // 8-tile block boundaries.
            int subTileX = hScrollBar.Value - (blockX << 3);
            int subTileY = vScrollBar.Value - (blockY << 3);
            int drawOffsetX = (int)Math.Round(subTileX * Zoom);
            int drawOffsetY = (int)Math.Round(subTileY * Zoom);

            e.Graphics.DrawImageUnscaled(toDraw, -drawOffsetX, -drawOffsetY);

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
                    _clientMap == _currentMapId)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(180, Color.Yellow)))
                    using (Pen pen = new Pen(brush))
                    {
                        int x = (int)((_clientX - hScrollBar.Value) * Zoom);
                        int y = (int)((_clientY - vScrollBar.Value) * Zoom);

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

            foreach (TreeNode obj in OverlayObjectTree.Nodes[_currentMapId].Nodes)
            {
                OverlayObject o = (OverlayObject)obj.Tag;
                if (o.IsVisible(e.ClipRectangle, _currentMapId, HScrollBar, VScrollBar, Zoom))
                {
                    o.Draw(e.Graphics, HScrollBar, VScrollBar, Zoom, CurrentMap.Width);
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
                    if (x >= 0 && y >= 0 && x <= CurrentMap.Width && x <= CurrentMap.Height)
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
                SendCharTo(_currentPoint.X, _currentPoint.Y);
            }
        }

        private void SendCharTo(int x, int y)
        {
            string format = "{0} " + Options.MapArgs;
            int z = CurrentMap.Tiles.GetLandTile(x, y).Z;
            Client.SendText(string.Format(format, Options.MapCmd, x, y, z, _currentMapId, Options.MapNames[_currentMapId]));
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
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{Options.MapNames[_currentMapId]}.{fileExtension}");

            using (new WaitCursorScope(this))
            {
                // Use altitude-aware rendering if mode is not Normal
                using Bitmap extract = _altitudeMode != MapAltitudeMode.Normal
                    ? CurrentMap.GetImageWithAltitude(0, 0, CurrentMap.Width >> 3, CurrentMap.Height >> 3,
                        showStaticsToolStripMenuItem1.Checked, _altitudeMode)
                    : CurrentMap.GetImage(0, 0, CurrentMap.Width >> 3, CurrentMap.Height >> 3,
                        showStaticsToolStripMenuItem1.Checked);

                // Skip markers in altitude mode
                if (showMarkersToolStripMenuItem.Checked && _altitudeMode != MapAltitudeMode.Altitude)
                {
                    using Graphics g = Graphics.FromImage(extract);
                    foreach (TreeNode obj in OverlayObjectTree.Nodes[_currentMapId].Nodes)
                    {
                        OverlayObject o = (OverlayObject)obj.Tag;
                        if (o.Visible)
                        {
                            o.Draw(g, Round(HScrollBar), Round(VScrollBar), Zoom, CurrentMap.Width);
                        }
                    }
                    g.Save();
                }
                extract.Save(fileName, imageFormat);
            }

            FileSavedDialog.Show(FindForm(), fileName, "Map saved successfully.");
        }

        private MapMarkerForm _mapMarkerForm;

        private void OnClickInsertMarker(object sender, EventArgs e)
        {
            if (_mapMarkerForm?.IsDisposed == false)
            {
                return;
            }

            _mapMarkerForm = new MapMarkerForm(AddOverlay, _currentPoint.X, _currentPoint.Y, _currentMapId)
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

            _preloadValue = 0;
            _preloadMax = (CurrentMap.Width >> 3) * (CurrentMap.Height >> 3);
            // Progress is drawn directly in the pictureBox so it appears under the
            // "Preloading map..." message rather than orphaned in the toolstrip.
            PreloadMap.Visible = false;
            ProgressBar.Visible = false;
            pictureBox.Invalidate();
            PreloadWorker.RunWorkerAsync(new object[] { CurrentMap, showStaticsToolStripMenuItem1.Checked });
        }

        private void PreLoadDoWork(object sender, DoWorkEventArgs e)
        {
            //Ultima.Map workmap = (Ultima.Map)((object[])e.Argument)[0]; // TODO: unused variable?
            bool statics = (bool)((object[])e.Argument)[1];
            int width = CurrentMap.Width >> 3;
            int height = CurrentMap.Height >> 3;
            int total = width * height;
            int reportEvery = Math.Max(1, total / 200); // ~200 UI updates total
            int sinceReport = 0;
            int done = 0;
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    CurrentMap.PreloadRenderedBlock(x, y, statics);
                    ++done;
                    if (++sinceReport >= reportEvery)
                    {
                        sinceReport = 0;
                        PreloadWorker.ReportProgress(done);
                    }
                }
            }

            // Final report so the bar reaches 100%.
            PreloadWorker.ReportProgress(done);
            CurrentMap.MarkPreloaded(statics);
            CurrentMap.Tiles.CloseStreams();
        }

        private void PreLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _preloadValue = e.ProgressPercentage;
            pictureBox.Invalidate();
        }

        private void PreLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _preloadMax = 0;
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
            if (_currentMapId != o.DefMap)
            {
                ResetCheckedMap();
                SwitchMap(o.DefMap);
                _currentMapId = o.DefMap;
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
            OverlayObjectTree.SelectedNode.ForeColor = !o.Visible
                ? (Options.DarkMode ? Color.OrangeRed : Color.Red)
                : SystemColors.ControlText;

            OverlayObjectTree.Invalidate();
            pictureBox.Invalidate();
        }

        private void OnChangeView(object sender, EventArgs e)
        {
            PreloadMap.Visible = !CurrentMap.IsCached(showStaticsToolStripMenuItem1.Checked);
            pictureBox.Invalidate();
        }

        private void OnClickDefragStatics(object sender, EventArgs e)
        {
            using (new WaitCursorScope(this))
            {
                Map.DefragStatics(Options.OutputPath,
                    CurrentMap, CurrentMap.Width, CurrentMap.Height, false);
            }

            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Statics saved successfully.");
        }

        private void OnClickDefragRemoveStatics(object sender, EventArgs e)
        {
            using (new WaitCursorScope(this))
            {
                Map.DefragStatics(Options.OutputPath,
                    CurrentMap, CurrentMap.Width, CurrentMap.Height, true);
            }
            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Statics saved successfully.");
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
            using (new WaitCursorScope(this))
            {
                Map.RewriteMap(Options.OutputPath,
                    _currentMapId, CurrentMap.Width, CurrentMap.Height);
            }
            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Files saved successfully.");
        }

        private void OnClickReportInvisStatics(object sender, EventArgs e)
        {
            using (new WaitCursorScope(this))
            {
                CurrentMap.ReportInvisibleStatics(Options.OutputPath);
            }
            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Report saved successfully.");
        }

        private void OnClickReportInvalidMapIDs(object sender, EventArgs e)
        {
            using (new WaitCursorScope(this))
            {
                CurrentMap.ReportInvalidMapIDs(Options.OutputPath);
            }
            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Report saved successfully.");
        }

        private MapReplaceForm _showForm;

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new MapReplaceForm(CurrentMap)
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

            _showFormMapDiff = new MapDiffInsertForm(CurrentMap)
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
                            CurrentMap.Tiles.AddPendingStatic(blockX, blockY, newTile);
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
                        newTile.Id = Art.GetLegalItemId(Convert.ToUInt16(line));
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
                CurrentMap.Tiles.AddPendingStatic(blockX, blockY, newTile);
            }

            ip.Close();

            MessageBox.Show("Done", "Freeze Static", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            CurrentMap.ResetCache();
            pictureBox.Invalidate();
        }

        private MapMeltStaticsForm _showMeltStaticsForm;

        private void OnClickMeltStatics(object sender, EventArgs e)
        {
            if (_showMeltStaticsForm?.IsDisposed == false)
            {
                return;
            }

            _showMeltStaticsForm = new MapMeltStaticsForm(RefreshMap, CurrentMap)
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

            _showClearStaticsForm = new MapClearStaticsForm(RefreshMap, CurrentMap)
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

            _showMapReplaceTilesForm = new MapReplaceTilesForm(CurrentMap)
            {
                TopMost = true
            };
            _showMapReplaceTilesForm.Show();
        }

        private void OnAltitudeModeNormal(object sender, EventArgs e)
        {
            _altitudeMode = MapAltitudeMode.Normal;

            // Update checkmarks to show only this mode is selected
            altitudeModeNormalToolStripMenuItem.Checked = true;
            altitudeModeNormalWithAltitudeToolStripMenuItem.Checked = false;
            altitudeModeAltitudeToolStripMenuItem.Checked = false;

            pictureBox.Invalidate();
        }

        private void OnAltitudeModeNormalWithAltitude(object sender, EventArgs e)
        {
            _altitudeMode = MapAltitudeMode.NormalWithAltitude;

            // Update checkmarks to show only this mode is selected
            altitudeModeNormalToolStripMenuItem.Checked = false;
            altitudeModeNormalWithAltitudeToolStripMenuItem.Checked = true;
            altitudeModeAltitudeToolStripMenuItem.Checked = false;

            pictureBox.Invalidate();
        }

        private void OnAltitudeModeAltitude(object sender, EventArgs e)
        {
            _altitudeMode = MapAltitudeMode.Altitude;

            // Update checkmarks to show only this mode is selected
            altitudeModeNormalToolStripMenuItem.Checked = false;
            altitudeModeNormalWithAltitudeToolStripMenuItem.Checked = false;
            altitudeModeAltitudeToolStripMenuItem.Checked = true;

            pictureBox.Invalidate();
        }

        private void OnAltitudeIntensityChanged(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem clickedItem || clickedItem.Tag is not int intensity)
            {
                return;
            }

            // Update the intensity setting
            Map.AltitudeIntensity = intensity;

            // Update checkmarks on all intensity items (skip preset items and separators)
            foreach (ToolStripItem item in altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Tag is int)
                {
                    menuItem.Checked = (int)menuItem.Tag == intensity;
                }
            }

            // Refresh the map if we're in altitude mode
            if (_altitudeMode == MapAltitudeMode.NormalWithAltitude)
            {
                pictureBox.Invalidate();
            }
        }

        private void OnAltitudePresetChanged(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem clickedItem || clickedItem.Tag is not string presetTag)
            {
                return;
            }

            // Update the preset setting
            AltitudeShadingPreset newPreset = presetTag switch
            {
                "preset_sharp" => AltitudeShadingPreset.Sharp,
                "preset_normal" => AltitudeShadingPreset.Normal,
                "preset_soft" => AltitudeShadingPreset.Soft,
                _ => AltitudeShadingPreset.Soft
            };

            Map.ShadingPreset = newPreset;

            // Update checkmarks on preset items
            foreach (ToolStripItem item in altitudeModeNormalWithAltitudeToolStripMenuItem.DropDownItems)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Tag is string tag && tag.StartsWith("preset_"))
                {
                    menuItem.Checked = tag == presetTag;
                }
            }

            // Refresh the map if we're in altitude mode
            if (_altitudeMode == MapAltitudeMode.NormalWithAltitude)
            {
                pictureBox.Invalidate();
            }
        }
    }

    public class OverlayObject
    {
        public virtual bool IsVisible(Rectangle bounds, int m, int hScrollBar, int vScrollBar, double zoom) { return false; }
        public virtual void Draw(Graphics g, int roundedHScrollbar, int roundedVScrollbar, double zoom, int width) { }
        public virtual void Save(XmlElement elem) { }
        public override string ToString() { return string.Empty; }

        public bool Visible { get; set; }
        public Point Loc { get; protected set; }
        public int DefMap { get; protected set; }
    }

    public class OverlayCursor : OverlayObject, IDisposable
    {
        private readonly string _text;
        private readonly Color _col;
        private readonly Pen _pen;
        private readonly Brush _brush;
        // Shared, immutable label backdrop — created once and never disposed
        // per-instance (it was previously static yet reallocated/disposed per
        // marker, which leaked brushes and could dispose one still in use).
        private static readonly Brush _background = new SolidBrush(Color.FromArgb(100, Color.White));

        public OverlayCursor(Point location, int m, string t, Color c)
        {
            Loc = location;
            DefMap = m;
            _text = t;
            _col = c;
            Visible = true;
            _brush = new SolidBrush(_col);
            _pen = new Pen(_brush);
        }

        public override bool IsVisible(Rectangle bounds, int m, int hScrollBar, int vScrollBar, double zoom)
        {
            if (!Visible)
            {
                return false;
            }

            if (DefMap != m)
            {
                return false;
            }

            return Loc.X > hScrollBar &&
                Loc.X < hScrollBar + (bounds.Width / zoom) &&
                Loc.Y > vScrollBar &&
                Loc.Y < vScrollBar + (bounds.Height / zoom);
        }

        public override void Draw(Graphics g, int roundedHScrollbar, int roundedVScrollbar, double zoom, int width)
        {
            int x = (int)((Loc.X - roundedHScrollbar) * zoom);
            int y = (int)((Loc.Y - roundedVScrollbar) * zoom);
            g.DrawLine(_pen, x - 4, y, x + 4, y);
            g.DrawLine(_pen, x, y - 4, x, y + 4);
            g.DrawEllipse(_pen, x - 2, y - 2, 2 * 2, 2 * 2);
            SizeF tSize = g.MeasureString(_text, Control.DefaultFont);
            int xStr = Loc.X + tSize.Width > width ? x - (int)tSize.Width - 6 : x + 6;
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
            _brush?.Dispose();
        }
    }
}
