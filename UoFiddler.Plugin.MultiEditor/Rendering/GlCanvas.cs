/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// The editor canvas: an OpenGL 3.3 core surface that draws the session it is given, on
    /// demand. It owns the camera gestures (pan, zoom) and passes the rest of the mouse to the
    /// session in world coordinates.
    /// </summary>
    internal sealed class GlCanvas : GLControl
    {
        private bool _ready;
        private bool _failed;
        private SceneRenderer _renderer;
        private EditorSession _session;

        private bool _panning;
        private bool _panned;
        private Point _panStart;
        private Point _panLast;
        private bool _spaceHeld;
        private Timer _animationTimer;
        private Timer _modifierTimer;
        private Keys _lastModifiers;

        /// <summary>Raised once when the context cannot be used; the text is for the user.</summary>
        public event Action<string> ContextFailed;

        /// <summary>Raised on a right click that was not a pan, with the canvas position.</summary>
        public event Action<Point> ContextRequested;

        /// <summary>Raised after pan or zoom changed.</summary>
        public event Action CameraChanged;

        public GlCanvas()
            : base(new GLControlSettings
            {
                API = ContextAPI.OpenGL,
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            })
        {
        }

        public Camera2D Camera { get; private set; } = new Camera2D();

        /// <summary>The session shown. Each session keeps its own camera, passed in with it.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EditorSession Session => _session;

        public void Show(EditorSession session, Camera2D camera)
        {
            if (_session != null)
            {
                _session.Invalidated -= Invalidate;
            }

            _session = session;
            Camera = camera ?? new Camera2D();
            if (_session != null)
            {
                _session.Invalidated += Invalidate;
            }

            Invalidate();
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);

            // A mouse move that asks for a frame starts the lag clock; moves that change nothing do not.
            if (_inMouseMove && _unshownMove == 0)
            {
                _unshownMove = _moveStart;
            }
        }

        // ---------------------------------------------------------------- performance box

        /// <summary>Timings for the performance box. Only collected while <see cref="CollectStats"/> is on.</summary>
        public FrameStats Stats { get; } = new FrameStats();

        /// <summary>Times frames, mouse moves and (with timer queries) the GPU.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool CollectStats { get; set; }

        private long _moveStart;
        private bool _inMouseMove;

        /// <summary>The first mouse move that asked for a frame not drawn yet; 0 when there is none.</summary>
        private long _unshownMove;

        // GPU timer queries, read a frame or two later so the CPU never waits for them.
        private const int GpuQueryCount = 4;
        private readonly int[] _gpuQueries = new int[GpuQueryCount];
        private readonly bool[] _gpuPending = new bool[GpuQueryCount];
        private int _gpuNext;

        /// <summary>Collects finished GPU timings and starts a new query; -1 when all are still in flight.</summary>
        private int BeginGpuQuery()
        {
            if (_gpuQueries[0] == 0)
            {
                GL.GenQueries(GpuQueryCount, _gpuQueries);
            }

            for (int i = 0; i < GpuQueryCount; i++)
            {
                if (!_gpuPending[i])
                {
                    continue;
                }

                GL.GetQueryObject(_gpuQueries[i], GetQueryObjectParam.QueryResultAvailable, out int available);
                if (available != 0)
                {
                    GL.GetQueryObject(_gpuQueries[i], GetQueryObjectParam.QueryResult, out long nanoseconds);
                    Stats.Gpu(nanoseconds / 1_000_000.0);
                    _gpuPending[i] = false;
                }
            }

            int next = _gpuNext;
            if (_gpuPending[next])
            {
                return -1;
            }

            GL.BeginQuery(QueryTarget.TimeElapsed, _gpuQueries[next]);
            _gpuPending[next] = true;
            _gpuNext = (next + 1) % GpuQueryCount;
            return next;
        }

        /// <summary>Art changed: forget the uploaded sprites.</summary>
        public void ResetArt()
        {
            if (_ready)
            {
                MakeCurrent();
                _renderer.Atlas.Clear();
            }

            Invalidate();
        }

        /// <summary>Centres the multi in the canvas.</summary>
        public void FitToView()
        {
            if (_session == null || !_session.HasDocument)
            {
                return;
            }

            Camera.Fit(SceneBounds(), ClientSize, _session.PlanMode ? 4f : 2f);
            OnCameraChanged();
        }

        public void SetZoom(float zoom)
        {
            Camera.ZoomAt(new Point(ClientSize.Width / 2, ClientSize.Height / 2), zoom);
            OnCameraChanged();
        }

        public void ZoomStep(int direction)
        {
            Camera.ZoomStep(new Point(ClientSize.Width / 2, ClientSize.Height / 2), direction);
            OnCameraChanged();
        }

        /// <summary>Reads the frame just drawn, top-down BGRA, for screenshots and tests.</summary>
        public Bitmap CaptureFrame()
        {
            if (!_ready)
            {
                return null;
            }

            MakeCurrent();
            Size size = ClientSize;
            Render(size);
            var bitmap = new Bitmap(size.Width, size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new Rectangle(Point.Empty, size),
                System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 4);
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, size.Width, size.Height, PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bitmap;
        }

        private RectangleF SceneBounds()
        {
            var list = _session.CompList;
            if (_session.PlanMode)
            {
                Size plan = MultiEditor.Classes.PlanView.ContentSize(list.Width, list.Height);
                return new RectangleF(0, 0, plan.Width, plan.Height);
            }

            // The floor diamond of the whole multi at Z 0, grown by the tiles standing on it.
            float left = EditorSession.CellCorner(0, list.Height, 0).X;
            float right = EditorSession.CellCorner(list.Width, 0, 0).X;
            float top = EditorSession.CellCorner(0, 0, 0).Y;
            float bottom = EditorSession.CellCorner(list.Width, list.Height, 0).Y;
            foreach (var tile in list.RealTiles)
            {
                if (!MultiEditor.Classes.ArtMetrics.TryGetSize(tile.Id, out int width, out int height))
                {
                    continue;
                }

                left = Math.Min(left, tile.XMod);
                top = Math.Min(top, tile.YMod);
                right = Math.Max(right, tile.XMod + width);
                bottom = Math.Max(bottom, tile.YMod + height);
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        // ---------------------------------------------------------------- painting

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Size size = ClientSize;
            if (_failed || IsDesignMode || size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            try
            {
                long start = Stopwatch.GetTimestamp();
                MakeCurrent();
                if (!_ready)
                {
                    Initialize();
                }

                bool stats = CollectStats;
                int query = stats ? BeginGpuQuery() : -1;
                long drawStart = Stopwatch.GetTimestamp();
                Render(size);
                long drawEnd = Stopwatch.GetTimestamp();
                if (query >= 0)
                {
                    GL.EndQuery(QueryTarget.TimeElapsed);
                }

                SwapBuffers();
                long end = Stopwatch.GetTimestamp();
                if (stats)
                {
                    Stats.Frame(FrameStats.Ms(drawStart, drawEnd), FrameStats.Ms(drawEnd, end),
                        FrameStats.Ms(start, end), _renderer.LastSpriteCount);
                    if (_unshownMove != 0)
                    {
                        Stats.Lag(FrameStats.Ms(_unshownMove, end));
                    }
                }

                _unshownMove = 0;
            }
            catch (Exception ex)
            {
                _failed = true;
                ContextFailed?.Invoke($"OpenGL rendering failed:\r\n{ex.Message}");
            }
        }

        private void Initialize()
        {
            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);
            if (major < 3 || (major == 3 && minor < 3))
            {
                throw new NotSupportedException(
                    $"OpenGL 3.3 is required, this system offers {major}.{minor} ({GL.GetString(StringName.Renderer)}).");
            }

            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
                BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);

            _renderer = new SceneRenderer();
            _ready = true;
        }

        private void Render(Size size)
        {
            GL.Viewport(0, 0, size.Width, size.Height);
            Color background = UI.EditorSettings.Current.EffectiveBackground;
            GL.ClearColor(background.R / 255f, background.G / 255f, background.B / 255f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (_session != null)
            {
                _renderer.Draw(_session, Camera, size);
            }

            // Animated tiles only move in preview mode; the timer runs just while one is on screen.
            bool animate = _session != null && _session.PreviewMode && _renderer.HasAnimation;
            if (animate && _animationTimer == null)
            {
                _animationTimer = new Timer { Interval = 100 };
                _animationTimer.Tick += (_, _) =>
                {
                    ++_renderer.AnimationTick;
                    Invalidate();
                };
            }

            if (_animationTimer != null)
            {
                _animationTimer.Enabled = animate;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animationTimer?.Dispose();
                _modifierTimer?.Dispose();
            }

            if (disposing && _ready)
            {
                try
                {
                    MakeCurrent();
                    _renderer.Dispose();
                    if (_gpuQueries[0] != 0)
                    {
                        GL.DeleteQueries(GpuQueryCount, _gpuQueries);
                    }
                }
                catch (Exception)
                {
                    // The context is already gone when the window is torn down first.
                }

                _ready = false;
            }

            base.Dispose(disposing);
        }

        // ---------------------------------------------------------------- input

        private Point World(Point screen)
        {
            PointF world = Camera.ScreenToWorld(screen);
            return new Point((int)Math.Floor(world.X), (int)Math.Floor(world.Y));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle ||
                (e.Button == MouseButtons.Left && _spaceHeld))
            {
                _panning = true;
                _panned = false;
                _panStart = _panLast = e.Location;
                Cursor = Cursors.SizeAll;
                return;
            }

            _session?.PointerDown(World(e.Location), e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!CollectStats)
            {
                HandleMouseMove(e);
                return;
            }

            long start = Stopwatch.GetTimestamp();
            _moveStart = start;
            _inMouseMove = true;
            try
            {
                HandleMouseMove(e);
            }
            finally
            {
                _inMouseMove = false;
            }

            if (!_panning && _session != null)
            {
                Stats.Move(FrameStats.Ms(start, Stopwatch.GetTimestamp()), _session.LastHitTestMs);
            }
        }

        private void HandleMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_panning)
            {
                if (Math.Abs(e.X - _panStart.X) > 3 || Math.Abs(e.Y - _panStart.Y) > 3)
                {
                    _panned = true;
                }

                if (_panned)
                {
                    Camera.Pan(e.X - _panLast.X, e.Y - _panLast.Y);
                    _panLast = e.Location;
                    OnCameraChanged();
                }

                return;
            }

            _session?.PointerMove(World(e.Location), e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_panning)
            {
                _panning = false;
                Cursor = Cursors.Default;
                if (_panned)
                {
                    return;
                }

                // A click without a drag. Right: stops what the tool is in the middle of, else asks
                // for the context menu. Middle: selects the hovered tile (below).
                if (e.Button == MouseButtons.Right)
                {
                    if (_session != null && (_session.Pasting || _session.ActiveTool.IsDragging))
                    {
                        _session.CancelTool();
                    }
                    else
                    {
                        ContextRequested?.Invoke(e.Location);
                    }

                    return;
                }

                if (e.Button == MouseButtons.Left)
                {
                    return;
                }
            }

            _session?.PointerUp(World(e.Location), e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            // Tools change what they would do with Shift or Ctrl held. Pressing a key moves no
            // mouse and the canvas may not have the keyboard, so the keys are watched while the
            // cursor is over it and the preview is redrawn when they change.
            _lastModifiers = ModifierKeys;
            if (_modifierTimer == null)
            {
                _modifierTimer = new Timer { Interval = 60 };
                _modifierTimer.Tick += (_, _) =>
                {
                    Keys now = ModifierKeys;
                    if (now != _lastModifiers)
                    {
                        _lastModifiers = now;
                        Invalidate();
                    }
                };
            }

            _modifierTimer.Start();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _modifierTimer?.Stop();
            _session?.PointerLeave();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta == 0)
            {
                return;
            }

            if ((ModifierKeys & Keys.Shift) != 0)
            {
                // Shift+wheel walks through the floors.
                _session?.StepLevel(Math.Sign(e.Delta));
                return;
            }

            Camera.ZoomStep(e.Location, Math.Sign(e.Delta));
            OnCameraChanged();
            _session?.PointerMove(World(e.Location), e);
        }

        protected override bool IsInputKey(Keys keyData)
        {
            // Arrow keys nudge the selection instead of moving focus.
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Space)
            {
                _spaceHeld = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (e.KeyCode == Keys.Space)
            {
                _spaceHeld = false;
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _spaceHeld = false;
        }

        private void OnCameraChanged()
        {
            CameraChanged?.Invoke();
            Invalidate();
        }
    }
}
