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
using System.Numerics;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareMapControl : UserControl
    {
        public CompareMapControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            pictureBox.MouseWheel += OnMouseWheel;
        }

        private bool _loaded;
        private bool _moving;
        private Point _movingPoint;
        private Point _currentPoint;
        private Map _currentMap;
        private Map _originalMap;
        private int _currentMapId;
        private Bitmap _map;
        private Bitmap _renderBuffer;
        private Bitmap _zoomBuffer;
        private Graphics _zoomBufferGraphics;
        private static double _zoom = 1;
        // One ulong per 8x8 block: bit (xb<<3 | yb) is set when that tile differs.
        // Flat 1D, indexed as blockX * _diffHeightBlocks + blockY.
        private ulong[] _diffMasks;
        private int _diffWidthBlocks;
        private int _diffHeightBlocks;
        private readonly System.Diagnostics.Stopwatch _dragRepaintTimer = new System.Diagnostics.Stopwatch();
        private System.Windows.Forms.Timer _dragTrailTimer;
        private bool _dragInvalidatePending;
        private double _dragAccumX;
        private double _dragAccumY;
        private const int DragRepaintIntervalMs = 16;

        private void OnLoad(object sender, EventArgs e)
        {
            _currentMap = Map.Custom;
            _originalMap = Map.Felucca;
            feluccaToolStripMenuItem.Checked = true;
            trammelToolStripMenuItem.Checked = false;
            ilshenarToolStripMenuItem.Checked = false;
            malasToolStripMenuItem.Checked = false;
            tokunoToolStripMenuItem.Checked = false;
            terMurToolStripMenuItem.Checked = false;
            showDifferencesToolStripMenuItem.Checked = true;
            showMap1ToolStripMenuItem.Checked = true;
            showMap2ToolStripMenuItem.Checked = false;
            SetScrollBarValues();
            ChangeMapNames();
            ZoomLabel.Text = $"Zoom: {_zoom}";

            Options.LoadedUltimaClass["Map"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

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
            CalculateDiffs();
            pictureBox.Invalidate();
        }

        private void OnMapNameChangeEvent()
        {
            ChangeMapNames();
        }

        private void OnMapSizeChangeEvent()
        {
            InternalUpdate();
        }

        private void OnFilePathChangeEvent()
        {
            InternalUpdate();
        }

        private void InternalUpdate()
        {
            SetScrollBarValues();
            if (_currentMap != null)
            {
                ChangeMap();
            }

            pictureBox.Invalidate();
        }

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
        }

        private static int Round(int x)
        {
            return (x >> 3) << 3;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
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

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            int xDelta = Math.Min(_originalMap.Width, (int)(e.X / _zoom) + Round(hScrollBar.Value));
            int yDelta = Math.Min(_originalMap.Height, (int)(e.Y / _zoom) + Round(vScrollBar.Value));

            CoordsLabel.Text = $"Coords: {xDelta},{yDelta}";

            string diff = string.Empty;

            if (_moving)
            {
                toolTip1.RemoveAll();

                // Accumulate the fractional part of the drag so high-zoom drags (where 1 mouse pixel
                // is less than 1 tile) don't lose precision.
                _dragAccumX += -(e.X - _movingPoint.X) / _zoom;
                _dragAccumY += -(e.Y - _movingPoint.Y) / _zoom;

                int deltaX = (int)_dragAccumX;
                int deltaY = (int)_dragAccumY;
                _dragAccumX -= deltaX;
                _dragAccumY -= deltaY;

                _movingPoint.X = e.X;
                _movingPoint.Y = e.Y;

                if (deltaX != 0 || deltaY != 0)
                {
                    hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
                    vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));
                    RequestDragRepaint();
                }
            }
            else if (_zoom >= 2 && _currentMap != null)
            {
                if (BlockDiff(xDelta >> 3, yDelta >> 3))
                {
                    Tile customTile = _currentMap.Tiles.GetLandTile(xDelta, yDelta);
                    Tile origTile = _originalMap.Tiles.GetLandTile(xDelta, yDelta);

                    if (customTile.Id != origTile.Id || customTile.Z != origTile.Z)
                    {
                        diff = $"Tile:\n\r0x{origTile.Id:X} {origTile.Z} -> 0x{customTile.Id:X} {customTile.Z}\n\r";
                    }

                    HuedTile[] customStatics = _currentMap.Tiles.GetStaticTiles(xDelta, yDelta);
                    HuedTile[] origStatics = _originalMap.Tiles.GetStaticTiles(xDelta, yDelta);

                    if (customStatics.Length != origStatics.Length)
                    {
                        diff += "Statics:\n\rorig:\n\r";

                        foreach (HuedTile tile in origStatics)
                        {
                            diff += $"0x{tile.Id:X} {tile.Z} {tile.Hue}\n\r";
                        }

                        diff += "new:\n\r";

                        foreach (HuedTile tile in customStatics)
                        {
                            diff += $"0x{tile.Id:X} {tile.Z} {tile.Hue}\n\r";
                        }
                    }
                    else
                    {
                        bool changed = false;
                        for (int i = 0; i < customStatics.Length; i++)
                        {
                            if (customStatics[i].Id != origStatics[i].Id
                                || customStatics[i].Z != origStatics[i].Z
                                || customStatics[i].Hue != origStatics[i].Hue)
                            {
                                if (!changed)
                                {
                                    diff += "Statics diff:\n\r";

                                    changed = true;
                                }
                                diff += $"0x{origStatics[i].Id:X} {origStatics[i].Z} {origStatics[i].Hue} -> 0x{customStatics[i].Id:X} {customStatics[i].Z} {customStatics[i].Hue}\n\r";
                            }
                        }
                    }
                }
                toolTip1.SetToolTip(pictureBox, diff);
                pictureBox.Invalidate();
            }

            if ((_zoom < 2) || !markDiffToolStripMenuItem.Checked || !string.IsNullOrEmpty(diff))
            {
                return;
            }

            Map drawMap = showMap1ToolStripMenuItem.Checked
                ? _originalMap
                : _currentMap;

            if (drawMap?.Tiles.Patch.LandBlocksCount > 0 && drawMap.Tiles.Patch.IsLandBlockPatched(xDelta >> 3, yDelta >> 3))
            {
                Tile patchTile = drawMap.Tiles.Patch.GetLandTile(xDelta, yDelta);
                Tile origTile = drawMap.Tiles.GetLandTile(xDelta, yDelta, false);
                diff = $"Tile:\n\r0x{origTile.Id:X} {origTile.Z} -> 0x{patchTile.Id:X} {patchTile.Z}\n\r";
            }

            if (drawMap?.Tiles.Patch.StaticBlocksCount > 0 && drawMap.Tiles.Patch.IsStaticBlockPatched(xDelta >> 3, yDelta >> 3))
            {
                HuedTile[] patchStatics = drawMap.Tiles.Patch.GetStaticTiles(xDelta, yDelta);
                HuedTile[] origStatics = drawMap.Tiles.GetStaticTiles(xDelta, yDelta, false);

                diff += "Statics:\n\rorig:\n\r";

                foreach (HuedTile tile in origStatics)
                {
                    diff += $"0x{tile.Id:X} {tile.Z} {tile.Hue}\n\r";
                }

                diff += "patch:\n\r";

                foreach (HuedTile tile in patchStatics)
                {
                    diff += $"0x{tile.Id:X} {tile.Z} {tile.Hue}\n\r";
                }
            }

            toolTip1.SetToolTip(pictureBox, diff);

            pictureBox.Invalidate();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _moving = false;
            Cursor = Cursors.Default;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            Map drawMap = showMap1ToolStripMenuItem.Checked ? _originalMap : _currentMap;
            if (drawMap == null)
            {
                return;
            }

            int blockX = hScrollBar.Value >> 3;
            int blockY = vScrollBar.Value >> 3;
            // +16 (2 blocks of padding) so the sub-block scroll offset never reveals empty space
            // along the right/bottom edge of the viewport.
            int widthBlocks = ((int)Math.Ceiling(e.ClipRectangle.Width / _zoom) + 16) >> 3;
            int heightBlocks = ((int)Math.Ceiling(e.ClipRectangle.Height / _zoom) + 16) >> 3;

            int bufferPixelW = widthBlocks << 3;
            int bufferPixelH = heightBlocks << 3;
            _map = EnsureRenderBuffer(bufferPixelW, bufferPixelH);

            drawMap.GetImage(blockX, blockY, widthBlocks, heightBlocks, _map, true);

            if (_currentMap != null && showDifferencesToolStripMenuItem.Checked && _diffMasks != null)
            {
                DrawDiffOverlay(blockX, blockY, widthBlocks, heightBlocks);
            }

            if (markDiffToolStripMenuItem.Checked)
            {
                int count = drawMap.Tiles.Patch.LandBlocksCount + drawMap.Tiles.Patch.StaticBlocksCount;
                if (count > 0)
                {
                    using (Graphics graphics = Graphics.FromImage(_map))
                    {
                        int maxX = Math.Min(blockX + widthBlocks, drawMap.Width >> 3);
                        int maxY = Math.Min(blockY + heightBlocks, drawMap.Height >> 3);

                        int gx = 0;
                        for (int x = blockX; x < maxX; x++, gx += 8)
                        {
                            int gy = 0;
                            for (int y = blockY; y < maxY; y++, gy += 8)
                            {
                                if (drawMap.Tiles.Patch.IsLandBlockPatched(x, y))
                                {
                                    graphics.FillRectangle(Brushes.Azure, gx, gy, 8, 8);
                                    graphics.FillRectangle(Brushes.Azure, gx, 0, 8, 2);
                                    graphics.FillRectangle(Brushes.Azure, 0, gy, 2, 8);
                                }

                                if (drawMap.Tiles.Patch.IsStaticBlockPatched(x, y))
                                {
                                    graphics.FillRectangle(Brushes.Azure, gx, gy, 8, 8);
                                    graphics.FillRectangle(Brushes.Azure, gx, 0, 8, 2);
                                    graphics.FillRectangle(Brushes.Azure, 0, gy, 2, 8);
                                }
                            }
                        }
                    }
                }
            }

            Bitmap toDraw = Math.Abs(_zoom - 1.0) < 1e-6 ? _map : ZoomMap(_map, _zoom);

            // The render buffer starts at the block boundary (blockX * 8). Shift the draw position
            // by the sub-block portion of the scroll so viewport pixel 0 maps to the exact tile
            // the scrollbar points at.
            int subTileX = hScrollBar.Value - (blockX << 3);
            int subTileY = vScrollBar.Value - (blockY << 3);
            int drawOffsetX = (int)Math.Round(subTileX * _zoom);
            int drawOffsetY = (int)Math.Round(subTileY * _zoom);

            e.Graphics.DrawImageUnscaled(toDraw, -drawOffsetX, -drawOffsetY);
        }

        /// <summary>
        /// Writes the red "diff" markers onto <see cref="_map"/> by locking its
        /// pixel buffer once and writing 16bpp pixels directly. Replaces the
        /// per-tile Graphics.DrawRectangle path that dominated drag/scroll cost
        /// when "Show Differences" was enabled.
        /// </summary>
        private unsafe void DrawDiffOverlay(int blockX, int blockY, int widthBlocks, int heightBlocks)
        {
            int maxX = Math.Min(blockX + widthBlocks, _diffWidthBlocks);
            int maxY = Math.Min(blockY + heightBlocks, _diffHeightBlocks);
            if (maxX <= blockX || maxY <= blockY)
            {
                return;
            }

            BitmapData bd = _map.LockBits(
                new Rectangle(0, 0, _map.Width, _map.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format16bppRgb555);
            try
            {
                ushort* basePtr = (ushort*)bd.Scan0;
                int stride = bd.Stride >> 1; // pixels per row
                const ushort red = 0x7C00;   // R=31, G=0, B=0 in 5-5-5
                int mapHeightBlocks = _diffHeightBlocks;

                int gx = 0;
                for (int x = blockX; x < maxX; x++, gx += 8)
                {
                    int colBase = x * mapHeightBlocks;
                    int gy = 0;
                    for (int y = blockY; y < maxY; y++, gy += 8)
                    {
                        ulong mask = _diffMasks[colBase + y];
                        if (mask == 0)
                        {
                            continue;
                        }

                        while (mask != 0)
                        {
                            int bit = BitOperations.TrailingZeroCount(mask);
                            mask &= mask - 1; // clear lowest set bit
                            int xb = bit >> 3;
                            int yb = bit & 7;

                            int px = gx + xb;
                            int py = gy + yb;

                            // 1x1 tile pixel
                            basePtr[py * stride + px] = red;
                            // Top-edge column marker (1 col wide, 2 rows tall)
                            basePtr[px] = red;
                            basePtr[stride + px] = red;
                            // Left-edge row marker (2 cols wide, 1 row tall)
                            basePtr[py * stride + 0] = red;
                            basePtr[py * stride + 1] = red;
                        }
                    }
                }
            }
            finally
            {
                _map.UnlockBits(bd);
            }
        }

        private Bitmap EnsureRenderBuffer(int pixelWidth, int pixelHeight)
        {
            if (_renderBuffer != null
                && _renderBuffer.Width == pixelWidth
                && _renderBuffer.Height == pixelHeight)
            {
                return _renderBuffer;
            }

            _renderBuffer?.Dispose();
            _renderBuffer = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format16bppRgb555);
            return _renderBuffer;
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

        private void OnResize(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            ChangeScrollBar();
            pictureBox.Invalidate();
        }

        private void ChangeScrollBar()
        {
            hScrollBar.Maximum = _originalMap.Width;
            hScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Width / _zoom) - 8);

            if (_zoom >= 1)
            {
                hScrollBar.Maximum += (int)(40 * _zoom);
            }
            else if (_zoom < 1)
            {
                hScrollBar.Maximum += (int)(40 / _zoom);
            }

            hScrollBar.Maximum = Math.Max(0, Round(hScrollBar.Maximum));
            vScrollBar.Maximum = _originalMap.Height;
            vScrollBar.Maximum -= Round((int)(pictureBox.ClientSize.Height / _zoom) - 8);

            if (_zoom >= 1)
            {
                vScrollBar.Maximum += (int)(40 * _zoom);
            }
            else if (_zoom < 1)
            {
                vScrollBar.Maximum += (int)(40 / _zoom);
            }

            vScrollBar.Maximum = Math.Max(0, Round(vScrollBar.Maximum));
        }

        private void SetScrollBarValues()
        {
            hScrollBar.Minimum = 0;
            vScrollBar.Minimum = 0;

            ChangeScrollBar();

            hScrollBar.LargeChange = 40;
            hScrollBar.SmallChange = 8;
            hScrollBar.Value = 0;

            vScrollBar.LargeChange = 40;
            vScrollBar.SmallChange = 8;
            vScrollBar.Value = 0;
        }

        private const double MinZoom = 0.25;
        private const double MaxZoom = 4;

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            // Position-from-cursor for DoZoom's recenter math; mirrors what the
            // context-menu opening handler does so wheel and right-click+zoom
            // land at the same place.
            UpdateCurrentPointFromMouse();

            if (e.Delta > 0)
            {
                OnZoomPlus(sender, EventArgs.Empty);
            }
            else if (e.Delta < 0)
            {
                OnZoomMinus(sender, EventArgs.Empty);
            }
        }

        private void UpdateCurrentPointFromMouse()
        {
            _currentPoint = pictureBox.PointToClient(MousePosition);
            _currentPoint.X = (int)(_currentPoint.X / _zoom);
            _currentPoint.Y = (int)(_currentPoint.Y / _zoom);
            _currentPoint.X += hScrollBar.Value;
            _currentPoint.Y += vScrollBar.Value;
        }

        private void OnZoomPlus(object sender, EventArgs e)
        {
            if (_zoom * 2 > MaxZoom)
            {
                return;
            }

            _zoom *= 2;

            DoZoom();
        }

        private void OnZoomMinus(object sender, EventArgs e)
        {
            if (_zoom / 2 < MinZoom)
            {
                return;
            }

            _zoom /= 2;

            DoZoom();
        }

        private void DoZoom()
        {
            ChangeScrollBar();

            ZoomLabel.Text = $"Zoom: {_zoom}";

            int x = Math.Max(0, _currentPoint.X - ((int)(pictureBox.ClientSize.Width / _zoom) / 2));
            int y = Math.Max(0, _currentPoint.Y - ((int)(pictureBox.ClientSize.Height / _zoom) / 2));

            x = Math.Min(x, hScrollBar.Maximum);
            y = Math.Min(y, vScrollBar.Maximum);

            hScrollBar.Value = Round(x);
            vScrollBar.Value = Round(y);

            pictureBox.Invalidate();
        }

        private void OnOpeningContext(object sender, CancelEventArgs e)
        {
            UpdateCurrentPointFromMouse();
        }

        private void OnClickBrowseLoc(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the map files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    toolStripTextBox1.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            string path = toolStripTextBox1.Text;
            if (Directory.Exists(path)
                && CompareFiles.IsLoadedClientFile(Path.Combine(path, $"map{_currentMapId}.mul"), $"map{_currentMapId}.mul"))
            {
                MessageBox.Show(
                    "The selected directory contains the same map file that is currently loaded.\n\n" +
                    "Choose a different directory to compare against.",
                    "Same File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            ChangeMap();
        }

        private void ChangeMap()
        {
            SetScrollBarValues();

            string path = toolStripTextBox1.Text;

            if (Directory.Exists(path))
            {
                _currentMap = Map.Custom = new Map(path, _originalMap.FileIndex, _currentMapId, _originalMap.Width, _originalMap.Height);
            }

            CalculateDiffs();

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

        private void OnClickChangeFelucca(object sender, EventArgs e)
        {
            if (feluccaToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();

            feluccaToolStripMenuItem.Checked = true;

            _originalMap = Map.Felucca;
            _currentMapId = 0;

            ChangeMap();
        }

        private void OnClickChangeTrammel(object sender, EventArgs e)
        {
            if (trammelToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();

            trammelToolStripMenuItem.Checked = true;

            _originalMap = Map.Trammel;
            _currentMapId = 1;

            ChangeMap();
        }

        private void OnClickChangeIlshenar(object sender, EventArgs e)
        {
            if (ilshenarToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();

            ilshenarToolStripMenuItem.Checked = true;

            _originalMap = Map.Ilshenar;
            _currentMapId = 2;

            ChangeMap();
        }

        private void OnClickChangeMalas(object sender, EventArgs e)
        {
            if (malasToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();

            malasToolStripMenuItem.Checked = true;

            _originalMap = Map.Malas;
            _currentMapId = 3;

            ChangeMap();
        }

        private void OnClickChangeTokuno(object sender, EventArgs e)
        {
            if (tokunoToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();

            tokunoToolStripMenuItem.Checked = true;

            _originalMap = Map.Tokuno;
            _currentMapId = 4;

            ChangeMap();
        }

        private void OnClickChangeTerMur(object sender, EventArgs e)
        {
            if (terMurToolStripMenuItem.Checked)
            {
                return;
            }

            ResetCheckedMap();

            terMurToolStripMenuItem.Checked = true;

            _originalMap = Map.TerMur;
            _currentMapId = 5;

            ChangeMap();
        }

        private void OnClickShowDiff(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnClickShowMap2(object sender, EventArgs e)
        {
            if (showMap2ToolStripMenuItem.Checked || _currentMap == null)
            {
                return;
            }

            showMap1ToolStripMenuItem.Checked = false;
            showMap2ToolStripMenuItem.Checked = true;

            pictureBox.Invalidate();
        }

        private void OnClickShowMap1(object sender, EventArgs e)
        {
            if (showMap1ToolStripMenuItem.Checked)
            {
                return;
            }

            showMap2ToolStripMenuItem.Checked = false;
            showMap1ToolStripMenuItem.Checked = true;

            pictureBox.Invalidate();
        }

        private void OnClickMarkDiff(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        private bool BlockDiff(int x, int y)
        {
            if (_diffMasks == null)
            {
                return false;
            }

            if (x < 0 || y < 0 || x >= _diffWidthBlocks || y >= _diffHeightBlocks)
            {
                return false;
            }

            return _diffMasks[x * _diffHeightBlocks + y] != 0;
        }

        private void CalculateDiffs()
        {
            if (_currentMap == null || _originalMap == null)
            {
                _diffMasks = null;
                _diffWidthBlocks = 0;
                _diffHeightBlocks = 0;
                return;
            }

            int width = _currentMap.Width >> 3;
            int height = _currentMap.Height >> 3;
            var masks = new ulong[width * height];

            using (new WaitCursorScope(this))
            {
                for (int x = 0; x < width; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        Tile[] customTiles = _currentMap.Tiles.GetLandBlock(x, y);
                        Tile[] origTiles = _originalMap.Tiles.GetLandBlock(x, y);

                        HuedTile[][][] customStatics = _currentMap.Tiles.GetStaticBlock(x, y);
                        HuedTile[][][] origStatics = _originalMap.Tiles.GetStaticBlock(x, y);

                        ulong mask = 0;
                        for (int xb = 0; xb < 8; xb++)
                        {
                            HuedTile[][] customCol = customStatics[xb];
                            HuedTile[][] origCol = origStatics[xb];
                            for (int yb = 0; yb < 8; yb++)
                            {
                                int tileIdx = (yb << 3) + xb;
                                bool isDiff;

                                if (customTiles[tileIdx].Id != origTiles[tileIdx].Id
                                 || customTiles[tileIdx].Z != origTiles[tileIdx].Z)
                                {
                                    isDiff = true;
                                }
                                else if (customCol[yb].Length != origCol[yb].Length)
                                {
                                    isDiff = true;
                                }
                                else
                                {
                                    isDiff = false;
                                    HuedTile[] cs = customCol[yb];
                                    HuedTile[] os = origCol[yb];
                                    for (int i = 0; i < cs.Length; i++)
                                    {
                                        if (cs[i].Id != os[i].Id
                                            || cs[i].Z != os[i].Z
                                            || cs[i].Hue != os[i].Hue)
                                        {
                                            isDiff = true;
                                            break;
                                        }
                                    }
                                }

                                if (isDiff)
                                {
                                    mask |= 1UL << ((xb << 3) | yb);
                                }
                            }
                        }

                        masks[x * height + y] = mask;
                    }
                }

                _diffMasks = masks;
                _diffWidthBlocks = width;
                _diffHeightBlocks = height;
            }
        }

        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
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
    }
}
