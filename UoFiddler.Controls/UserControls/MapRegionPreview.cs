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
using System.Windows.Forms;
using Ultima;
using Ultima.Maps;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.UserControls
{
    public enum MapPreviewMode
    {
        /// <summary>Shows the selection but does not let it be changed.</summary>
        ReadOnly,

        /// <summary>Dragging with the left button draws a new selection rectangle.</summary>
        Rectangle,

        /// <summary>Dragging with the left button moves the selection without changing its size.</summary>
        MoveFixedSize
    }

    /// <summary>
    /// Draws a window of a map facet with a block-aligned selection rectangle over it, so a region
    /// can be seen and placed rather than only typed. The left button sets the region, the right
    /// button pans, the wheel zooms and a double click re-fits.
    /// </summary>
    /// <remarks>
    /// Rendering reuses <see cref="Map.GetImage(int, int, int, int, Bitmap, bool)"/> and its half and
    /// quarter resolution siblings, which fill a caller-supplied bitmap from a block rectangle. The
    /// rendered window is cached and rebuilt on a worker behind a short debounce, while painting maps
    /// blocks to the control every frame - so a pan slides the cached bitmap straight away and the
    /// fresh render simply replaces it when it lands.
    /// </remarks>
    public sealed partial class MapRegionPreview : UserControl
    {
        /// <summary>Blocks of context left around the selection when the view is framed on it.</summary>
        private const int DefaultMarginBlocks = 6;

        /// <summary>How far in the wheel can go. Eight blocks across a panel is 64 tiles.</summary>
        private const int MinViewBlocks = 8;

        /// <summary>
        /// Map rendering walks per-instance caches and file streams that are not safe to use from
        /// two threads, and the source map is shared between a source panel and the overlay of a
        /// destination panel, so every render in the process takes this.
        /// </summary>
        private static readonly object _renderLock = new object();

        private readonly System.Windows.Forms.Timer _debounce = new System.Windows.Forms.Timer { Interval = 120 };
        private readonly BackgroundWorker _worker = new BackgroundWorker();

        private Bitmap _rendered;
        private Bitmap _overlayRendered;
        private RenderRequest _renderedRequest;
        private RenderRequest _pending;
        private bool _hasPending;

        private Map _map;
        private MapSize _mapSize;
        private bool _showStatics = true;
        private BlockRectangle _selection;
        private Map _overlayMap;
        private BlockRectangle _overlaySelection;
        private string _message;

        /// <summary>
        /// The blocks on screen. Deliberately not derived from the selection: drawing a region must
        /// not move the map out from under the hand drawing it.
        /// </summary>
        private int _viewX;
        private int _viewY;
        private int _viewWidth;
        private int _viewHeight;
        private bool _viewFramed;

        private bool _dragging;
        private Point _dragStartBlock;
        private BlockRectangle _dragOrigin;

        private bool _panning;
        private Point _panStartMouse;
        private int _panStartX;
        private int _panStartY;

        public MapRegionPreview()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);

            BackColor = Options.PreviewBackgroundColor;

            _debounce.Tick += OnDebounceTick;
            _worker.DoWork += OnWorkerDoWork;
            _worker.RunWorkerCompleted += OnWorkerCompleted;
        }

        /// <summary>Raised when a drag changed <see cref="Selection"/>.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>The facet to draw. May be built on a directory other than the loaded client.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Map Map
        {
            get => _map;
            set
            {
                _map = value;
                ResetView();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapSize MapSize
        {
            get => _mapSize;
            set
            {
                _mapSize = value;
                ResetView();
            }
        }

        [DefaultValue(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowStatics
        {
            get => _showStatics;
            set
            {
                if (_showStatics == value)
                {
                    return;
                }

                _showStatics = value;
                Rebuild();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapPreviewMode Mode { get; set; } = MapPreviewMode.ReadOnly;

        /// <summary>The block rectangle outlined on top of the map.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BlockRectangle Selection
        {
            get => _selection;
            set
            {
                bool wasVisible = _viewFramed && Intersects(CurrentWindow(), value);

                _selection = value;

                // A selection that lands somewhere else entirely brings the view with it; one that is
                // still on screen leaves the view where the user put it.
                if (!wasVisible)
                {
                    _viewFramed = false;
                }

                Rebuild();
            }
        }

        /// <summary>
        /// Optional second map whose <see cref="OverlaySelection"/> is drawn inside
        /// <see cref="Selection"/>, so a destination panel can show the piece that will land there
        /// rather than an empty outline.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Map OverlayMap
        {
            get => _overlayMap;
            set
            {
                _overlayMap = value;
                Rebuild();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BlockRectangle OverlaySelection
        {
            get => _overlaySelection;
            set
            {
                _overlaySelection = value;
                Rebuild();
            }
        }

        /// <summary>Tints the blocks it returns true for. Used to mark blocks a diff covers.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<int, int, bool> BlockHighlight { get; set; }

        /// <summary>Shown instead of a render, for "choose a folder first" and the like.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                Invalidate();
            }
        }

        /// <summary>True when part of the selection lies outside the blocks on screen.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SelectionTruncated { get; private set; }

        /// <summary>Queues a rebuild of the rendered window.</summary>
        public void Rebuild()
        {
            _debounce.Stop();
            _debounce.Start();
            Invalidate();
        }

        /// <summary>The blocks the panel is showing, selection plus margin, panned and clamped.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BlockRectangle Window => CurrentWindow();

        /// <summary>Drops the pan and zoom back to framing the selection.</summary>
        public void ResetView()
        {
            _viewFramed = false;

            Rebuild();
        }

        /// <summary>
        /// The blocks currently on screen. Pure arithmetic, so painting can call it every frame.
        /// </summary>
        private BlockRectangle CurrentWindow()
        {
            if (_mapSize.IsEmpty)
            {
                SelectionTruncated = false;

                return default;
            }

            if (!_viewFramed)
            {
                FrameSelection();
            }

            ClampView();

            var window = new BlockRectangle(_viewX, _viewY,
                _viewX + _viewWidth - 1, _viewY + _viewHeight - 1);

            SelectionTruncated =
                _selection.BlockX1 < window.BlockX1 || _selection.BlockX2 > window.BlockX2 ||
                _selection.BlockY1 < window.BlockY1 || _selection.BlockY2 > window.BlockY2;

            return window;
        }

        /// <summary>Puts the selection back on screen with a margin of context around it.</summary>
        private void FrameSelection()
        {
            int width = Math.Max(1, _selection.BlockWidth) + (DefaultMarginBlocks * 2);
            int height = Math.Max(1, _selection.BlockHeight) + (DefaultMarginBlocks * 2);

            SetView(_selection.BlockX1 - DefaultMarginBlocks, _selection.BlockY1 - DefaultMarginBlocks,
                width, height);

            _viewFramed = true;
        }

        /// <summary>
        /// Stores a view, grown to the shape of the panel so the map fills it rather than sitting in
        /// a letterbox, then clamped to the facet. The growth is centred on what was asked for.
        /// </summary>
        private void SetView(int x, int y, int width, int height)
        {
            if (_mapSize.IsEmpty)
            {
                return;
            }

            int wantedWidth = Math.Clamp(width, MinViewBlocks, _mapSize.BlockWidth);
            int wantedHeight = Math.Clamp(height, MinViewBlocks, _mapSize.BlockHeight);

            width = wantedWidth;
            height = wantedHeight;

            if (ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                double aspect = (double)ClientSize.Width / ClientSize.Height;

                int neededWidth = (int)Math.Round(height * aspect);
                int neededHeight = (int)Math.Round(width / aspect);

                // Only ever grow, and only when a whole block is missing, so putting a view that
                // already has the panel's shape back through here leaves it alone. Rounding up on
                // every pass would inflate the view a block at a time.
                if (neededWidth > width)
                {
                    width = neededWidth;
                }
                else if (neededHeight > height)
                {
                    height = neededHeight;
                }
            }

            _viewWidth = width;
            _viewHeight = height;

            // Growing one axis must not shove the view sideways off what was asked for.
            _viewX = x - ((_viewWidth - wantedWidth) / 2);
            _viewY = y - ((_viewHeight - wantedHeight) / 2);

            ClampView();
        }

        /// <summary>Pulls the stored view back onto the facet without reshaping it.</summary>
        private void ClampView()
        {
            _viewWidth = Math.Clamp(_viewWidth, 1, _mapSize.BlockWidth);
            _viewHeight = Math.Clamp(_viewHeight, 1, _mapSize.BlockHeight);

            _viewX = Math.Clamp(_viewX, 0, Math.Max(0, _mapSize.BlockWidth - _viewWidth));
            _viewY = Math.Clamp(_viewY, 0, Math.Max(0, _mapSize.BlockHeight - _viewHeight));
        }

        /// <summary>
        /// The largest of the three native scales whose bitmap still fits a couple of panels worth
        /// of pixels. Anything finer would be thrown away by the scale down to the panel.
        /// </summary>
        private int ChoosePixelsPerBlock(BlockRectangle window)
        {
            int budgetX = Math.Max(64, ClientSize.Width * 2);
            int budgetY = Math.Max(64, ClientSize.Height * 2);

            foreach (int candidate in new[] { 8, 4, 2 })
            {
                if (window.BlockWidth * candidate <= budgetX && window.BlockHeight * candidate <= budgetY)
                {
                    return candidate;
                }
            }

            return 2;
        }

        /// <summary>Pixels per block on screen for the window currently framed.</summary>
        private float ScreenScale(BlockRectangle window)
        {
            if (window.BlockWidth <= 0 || window.BlockHeight <= 0)
            {
                return 0f;
            }

            return Math.Min((float)ClientSize.Width / window.BlockWidth, (float)ClientSize.Height / window.BlockHeight);
        }

        /// <summary>Top left of the framed window in control pixels, centred.</summary>
        private PointF ScreenOrigin(BlockRectangle window, float scale)
        {
            return new PointF(
                (ClientSize.Width - (window.BlockWidth * scale)) / 2f,
                (ClientSize.Height - (window.BlockHeight * scale)) / 2f);
        }

        private static bool Intersects(BlockRectangle window, BlockRectangle selection)
        {
            return selection.BlockX2 >= window.BlockX1 && selection.BlockX1 <= window.BlockX2 &&
                   selection.BlockY2 >= window.BlockY1 && selection.BlockY1 <= window.BlockY2;
        }

        private void OnDebounceTick(object sender, EventArgs e)
        {
            _debounce.Stop();

            if (_map == null || _mapSize.IsEmpty || ClientSize.Width < 8 || ClientSize.Height < 8)
            {
                return;
            }

            BlockRectangle window = CurrentWindow();

            var request = new RenderRequest
            {
                Map = _map,
                Window = window,
                PixelsPerBlock = ChoosePixelsPerBlock(window),
                Statics = _showStatics,
                OverlayMap = _overlayMap,
                OverlayWindow = _overlaySelection
            };

            if (request.Equals(_renderedRequest))
            {
                Invalidate();

                return;
            }

            _pending = request;
            _hasPending = true;

            if (_worker.IsBusy)
            {
                return;
            }

            _hasPending = false;
            _worker.RunWorkerAsync(request);
        }

        private void OnWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var request = (RenderRequest)e.Argument;

            lock (_renderLock)
            {
                Bitmap map = Render(request.Map, request.Window, request.PixelsPerBlock, request.Statics);
                Bitmap overlay = request.OverlayMap == null
                    ? null
                    : Render(request.OverlayMap, request.OverlayWindow, request.PixelsPerBlock, request.Statics);

                e.Result = new RenderResult { Request = request, Map = map, Overlay = overlay };
            }
        }

        private static Bitmap Render(Map map, BlockRectangle window, int pixelsPerBlock, bool statics)
        {
            if (map == null || window.BlockWidth <= 0 || window.BlockHeight <= 0)
            {
                return null;
            }

            var bitmap = new Bitmap(window.BlockWidth * pixelsPerBlock, window.BlockHeight * pixelsPerBlock,
                PixelFormat.Format16bppRgb555);

            switch (pixelsPerBlock)
            {
                case 8:
                    map.GetImage(window.BlockX1, window.BlockY1, window.BlockWidth, window.BlockHeight, bitmap, statics);
                    break;

                case 4:
                    map.GetImageHalf(window.BlockX1, window.BlockY1, window.BlockWidth, window.BlockHeight, bitmap, statics);
                    break;

                default:
                    map.GetImageQuarter(window.BlockX1, window.BlockY1, window.BlockWidth, window.BlockHeight, bitmap, statics);
                    break;
            }

            return bitmap;
        }

        private void OnWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null && e.Result is RenderResult result)
            {
                _rendered?.Dispose();
                _overlayRendered?.Dispose();

                _rendered = result.Map;
                _overlayRendered = result.Overlay;
                _renderedRequest = result.Request;

                Invalidate();
            }

            if (!_hasPending || _map == null || IsDisposed)
            {
                return;
            }

            // Something changed while that render was in flight.
            _hasPending = false;
            _worker.RunWorkerAsync(_pending);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            if (!string.IsNullOrEmpty(_message))
            {
                DrawCentredText(e.Graphics, _message);

                return;
            }

            BlockRectangle window = CurrentWindow();
            float scale = ScreenScale(window);

            if (_rendered == null || scale <= 0)
            {
                DrawCentredText(e.Graphics, "Rendering...");

                return;
            }

            PointF origin = ScreenOrigin(window, scale);

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            // The cached bitmap covers whatever window it was rendered for, which is not necessarily
            // the one framed now - during a pan it lags by the distance dragged. Drawing it at its
            // own block position makes the stale image slide with the pan instead of sitting still.
            e.Graphics.DrawImage(_rendered, BlockRect(origin, window, scale, _renderedRequest.Window));

            DrawHighlights(e.Graphics, origin, window, scale);
            DrawOverlay(e.Graphics, origin, window, scale);
            DrawSelection(e.Graphics, origin, window, scale);

            if (SelectionTruncated)
            {
                DrawNote(e.Graphics, "the region runs past the view - zoom out with the wheel to see all of it");
            }
            else if (_debounce.Enabled || _worker.IsBusy)
            {
                DrawNote(e.Graphics, "rendering...");
            }
        }

        /// <summary>Where a block rectangle sits on screen, given the window currently framed.</summary>
        private static RectangleF BlockRect(PointF origin, BlockRectangle window, float scale, BlockRectangle blocks)
        {
            return new RectangleF(
                origin.X + ((blocks.BlockX1 - window.BlockX1) * scale),
                origin.Y + ((blocks.BlockY1 - window.BlockY1) * scale),
                Math.Max(1f, blocks.BlockWidth * scale),
                Math.Max(1f, blocks.BlockHeight * scale));
        }

        private void DrawHighlights(Graphics g, PointF origin, BlockRectangle window, float scale)
        {
            if (BlockHighlight == null)
            {
                return;
            }

            using (var brush = new SolidBrush(Color.FromArgb(90, Color.Gold)))
            {
                for (int x = window.BlockX1; x <= window.BlockX2; ++x)
                {
                    for (int y = window.BlockY1; y <= window.BlockY2; ++y)
                    {
                        if (!BlockHighlight(x, y))
                        {
                            continue;
                        }

                        g.FillRectangle(brush,
                            origin.X + ((x - window.BlockX1) * scale),
                            origin.Y + ((y - window.BlockY1) * scale),
                            Math.Max(1f, scale), Math.Max(1f, scale));
                    }
                }
            }
        }

        private void DrawOverlay(Graphics g, PointF origin, BlockRectangle window, float scale)
        {
            if (_overlayRendered == null)
            {
                return;
            }

            g.DrawImage(_overlayRendered, BlockRect(origin, window, scale, _selection));
        }

        private void DrawSelection(Graphics g, PointF origin, BlockRectangle window, float scale)
        {
            RectangleF rectangle = BlockRect(origin, window, scale, _selection);

            using (var shadow = new Pen(Color.FromArgb(160, Color.Black), 3f))
            using (var pen = new Pen(Options.DarkMode ? Color.OrangeRed : Color.Red, 1.5f))
            {
                g.DrawRectangle(shadow, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
                g.DrawRectangle(pen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            }
        }

        private void DrawCentredText(Graphics g, string text)
        {
            SizeF size = g.MeasureString(text, Font);

            g.DrawString(text, Font, SystemBrushes.ControlText,
                (ClientSize.Width - size.Width) / 2f, (ClientSize.Height - size.Height) / 2f);
        }

        private void DrawNote(Graphics g, string text)
        {
            SizeF size = g.MeasureString(text, Font);

            using (var brush = new SolidBrush(Color.FromArgb(170, Color.Black)))
            {
                g.FillRectangle(brush, 0, ClientSize.Height - size.Height, ClientSize.Width, size.Height);
            }

            g.DrawString(text, Font, Brushes.White, 2, ClientSize.Height - size.Height);
        }

        private bool TryBlockAt(Point location, out int blockX, out int blockY)
        {
            blockX = 0;
            blockY = 0;

            if (_mapSize.IsEmpty)
            {
                return false;
            }

            BlockRectangle window = CurrentWindow();
            float scale = ScreenScale(window);

            if (scale <= 0)
            {
                return false;
            }

            PointF origin = ScreenOrigin(window, scale);

            blockX = window.BlockX1 + (int)Math.Floor((location.X - origin.X) / scale);
            blockY = window.BlockY1 + (int)Math.Floor((location.Y - origin.Y) / scale);

            blockX = Math.Clamp(blockX, 0, _mapSize.BlockWidth - 1);
            blockY = Math.Clamp(blockY, 0, _mapSize.BlockHeight - 1);

            return true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {
                _panning = true;
                _panStartMouse = e.Location;
                _panStartX = _viewX;
                _panStartY = _viewY;
                Cursor = Cursors.SizeAll;

                return;
            }

            if (e.Button != MouseButtons.Left || Mode == MapPreviewMode.ReadOnly ||
                !TryBlockAt(e.Location, out int blockX, out int blockY))
            {
                return;
            }

            _dragging = true;
            _dragStartBlock = new Point(blockX, blockY);
            _dragOrigin = _selection;

            if (Mode == MapPreviewMode.Rectangle)
            {
                SetSelection(new BlockRectangle(blockX, blockY, blockX, blockY));
            }
            else
            {
                MoveSelectionTo(blockX, blockY);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_panning)
            {
                Pan(e.Location);

                return;
            }

            if (!_dragging || !TryBlockAt(e.Location, out int blockX, out int blockY))
            {
                return;
            }

            if (Mode == MapPreviewMode.Rectangle)
            {
                SetSelection(new BlockRectangle(
                    Math.Min(_dragStartBlock.X, blockX), Math.Min(_dragStartBlock.Y, blockY),
                    Math.Max(_dragStartBlock.X, blockX), Math.Max(_dragStartBlock.Y, blockY)));
            }
            else
            {
                MoveSelectionTo(blockX, blockY);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_panning && e.Button == MouseButtons.Right)
            {
                _panning = false;
                Cursor = Cursors.Default;
            }

            if (e.Button == MouseButtons.Left)
            {
                _dragging = false;
            }
        }

        /// <summary>Drags the view under the cursor, a block per block-sized step of the mouse.</summary>
        private void Pan(Point location)
        {
            BlockRectangle window = CurrentWindow();
            float scale = ScreenScale(window);

            if (scale <= 0)
            {
                return;
            }

            _viewX = _panStartX - (int)Math.Round((location.X - _panStartMouse.X) / scale);
            _viewY = _panStartY - (int)Math.Round((location.Y - _panStartMouse.Y) / scale);

            ClampView();

            Rebuild();
        }

        /// <summary>Centres the fixed-size selection on a block, clamped so it stays on the map.</summary>
        private void MoveSelectionTo(int blockX, int blockY)
        {
            int width = _dragOrigin.BlockWidth;
            int height = _dragOrigin.BlockHeight;

            int x1 = Math.Clamp(blockX - (width / 2), 0, Math.Max(0, _mapSize.BlockWidth - width));
            int y1 = Math.Clamp(blockY - (height / 2), 0, Math.Max(0, _mapSize.BlockHeight - height));

            SetSelection(new BlockRectangle(x1, y1, x1 + width - 1, y1 + height - 1));
        }

        /// <summary>
        /// Sets the selection from a drag. Unlike the property, this leaves the pan alone - the user
        /// is working inside the view they arranged.
        /// </summary>
        private void SetSelection(BlockRectangle selection)
        {
            _selection = selection;

            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (_mapSize.IsEmpty)
            {
                return;
            }

            BlockRectangle window = CurrentWindow();
            float scale = ScreenScale(window);

            if (scale <= 0)
            {
                return;
            }

            PointF origin = ScreenOrigin(window, scale);

            // Where the cursor sits in the window, as a fraction of it. Keeping that fraction over
            // the same block is what makes the wheel zoom towards what is under the pointer.
            double acrossX = Math.Clamp((e.Location.X - origin.X) / (window.BlockWidth * scale), 0d, 1d);
            double acrossY = Math.Clamp((e.Location.Y - origin.Y) / (window.BlockHeight * scale), 0d, 1d);

            double anchorX = window.BlockX1 + (acrossX * window.BlockWidth);
            double anchorY = window.BlockY1 + (acrossY * window.BlockHeight);

            double factor = e.Delta > 0 ? 1d / 1.3d : 1.3d;

            int width = (int)Math.Round(window.BlockWidth * factor);
            int height = (int)Math.Round(window.BlockHeight * factor);

            SetView((int)Math.Round(anchorX - (acrossX * width)),
                (int)Math.Round(anchorY - (acrossY * height)), width, height);

            Rebuild();
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            ResetView();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (_viewFramed)
            {
                SetView(_viewX, _viewY, _viewWidth, _viewHeight);
            }

            Rebuild();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _debounce.Stop();
                _debounce.Dispose();
                _worker.Dispose();
                _rendered?.Dispose();
                _overlayRendered?.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private struct RenderRequest : IEquatable<RenderRequest>
        {
            public Map Map;
            public BlockRectangle Window;
            public int PixelsPerBlock;
            public bool Statics;
            public Map OverlayMap;
            public BlockRectangle OverlayWindow;

            public bool Equals(RenderRequest other)
            {
                return ReferenceEquals(Map, other.Map) &&
                       PixelsPerBlock == other.PixelsPerBlock &&
                       Statics == other.Statics &&
                       Same(Window, other.Window) &&
                       ReferenceEquals(OverlayMap, other.OverlayMap) &&
                       Same(OverlayWindow, other.OverlayWindow);
            }

            public override bool Equals(object obj) => obj is RenderRequest other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Map, Window.BlockX1, Window.BlockY1,
                Window.BlockX2, Window.BlockY2, PixelsPerBlock, Statics, OverlayMap);

            private static bool Same(BlockRectangle a, BlockRectangle b)
            {
                return a.BlockX1 == b.BlockX1 && a.BlockY1 == b.BlockY1 &&
                       a.BlockX2 == b.BlockX2 && a.BlockY2 == b.BlockY2;
            }
        }

        private sealed class RenderResult
        {
            public RenderRequest Request { get; init; }

            public Bitmap Map { get; init; }

            public Bitmap Overlay { get; init; }
        }
    }
}