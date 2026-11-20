/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL4;
using Ultima;
using UoFiddler.Plugin.MapPreview.Scene;
using UoFiddler.Rendering;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace UoFiddler.Plugin.MapPreview.Rendering
{
    /// <summary>
    /// Draws a <see cref="MapPreviewScene"/> as the client would. Dragging pans, the wheel zooms
    /// towards the pointer and a double click fits the area.
    /// </summary>
    internal sealed class MapPreviewCanvas : GlHostControl
    {
        /// <summary>The largest side, in pixels, of an exported image.</summary>
        public const int MaxExportSize = 16384;

        private const int ExportTile = 2048;

        private readonly List<MeshItem> _items = new List<MeshItem>();

        private bool _ready;
        private bool _failed;
        private ArtAtlas _atlas;
        private HueTexture _hues;
        private MapBatch _batch;

        private MapPreviewScene _scene;
        private PreviewOptions _options = new PreviewOptions();
        private Drawable[] _order;
        private bool _orderStretched;
        private bool _meshDirty = true;
        private bool _fitPending = true;

        /// <summary>The scene changed since the bounds were taken, so the next mesh sets them.</summary>
        private bool _sceneChanged = true;

        /// <summary>A scene has been framed once; later scenes keep the zoom instead of fitting.</summary>
        private bool _hasView;
        private RectangleF _bounds;

        private bool _panning;
        private Point _panLast;

        /// <summary>Raised once when the context cannot be used; the text is for the user.</summary>
        public event Action<string> ContextFailed;

        /// <summary>
        /// Raised when the scene could not be turned into vertices, which means the client files
        /// could not be read. The canvas stays usable; the text is for the user.
        /// </summary>
        public event Action<string> SceneFailed;

        /// <summary>Raised when the pointer moves onto another tile, with a line for the status bar.</summary>
        public event Action<string> HoverChanged;

        /// <summary>Raised after the zoom changed.</summary>
        public event Action ZoomChanged;

        public Camera2D Camera { get; } = new Camera2D { MinZoom = 1f / 64f };

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MapPreviewScene Scene
        {
            get => _scene;
            set
            {
                _scene = value;
                _order = null;
                _meshDirty = true;
                _sceneChanged = true;
                _fitPending = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Shows a new scene. The first one is fitted to the view; after that the zoom the user
        /// chose is kept and the view is centred on <paramref name="focus"/>, a tile in map
        /// coordinates, so clicking around the map moves the preview without resetting it.
        /// </summary>
        public void ShowScene(MapPreviewScene scene, Point focus)
        {
            if (!_hasView || scene == null)
            {
                Scene = scene;
                return;
            }

            _scene = scene;
            _order = null;
            _meshDirty = true;
            _sceneChanged = true;

            int dx = Math.Clamp(focus.X - scene.Area.X, 0, scene.Area.Width - 1);
            int dy = Math.Clamp(focus.Y - scene.Area.Y, 0, scene.Area.Height - 1);
            float x = (dx - dy) * MapPreviewScene.TileHalf;
            float y = ((dx + dy) * MapPreviewScene.TileHalf) + MapPreviewScene.TileHalf -
                      (scene.HeightAt(dx, dy) * MapPreviewScene.PixelsPerZ);

            // The middle of the clicked tile's diamond, at its height.
            Camera.CenterOn(new RectangleF(x, y, 0f, 0f), ClientSize);
            Invalidate();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PreviewOptions Options
        {
            get => _options;
            set
            {
                if (_options == value)
                {
                    return;
                }

                _options = value;
                _meshDirty = true;
                Invalidate();
            }
        }

        public void FitToView()
        {
            if (_bounds.IsEmpty)
            {
                _fitPending = true;
                Invalidate();
                return;
            }

            Camera.Fit(_bounds, ClientSize, 2f, 16);
            ZoomChanged?.Invoke();
            Invalidate();
        }

        /// <summary>Shows the art at 100%, keeping what is in the middle of the view there.</summary>
        public void ZoomToActual()
        {
            Camera.ZoomAt(new Point(ClientSize.Width / 2, ClientSize.Height / 2), 1f);
            ZoomChanged?.Invoke();
            Invalidate();
        }

        /// <summary>Forgets cached art, after art, texmaps or hues changed.</summary>
        public void ReloadArt()
        {
            // Not drawn yet: the atlas and hues are made from the current files on first paint.
            if (!_ready || _failed || !IsHandleCreated)
            {
                return;
            }

            MakeCurrent();
            _atlas.Clear();
            _hues.Dispose();
            _hues = new HueTexture();
            _meshDirty = true;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Size size = ClientSize;
            if (_failed || DesignMode || size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            try
            {
                MakeCurrent();
                if (!_ready)
                {
                    Initialize();
                }

                // Client files are data, not GL: a failure there empties the view and leaves the
                // context working for the next scene.
                try
                {
                    RebuildIfNeeded();
                }
                catch (Exception ex)
                {
                    _scene = null;
                    _order = null;
                    _meshDirty = true;
                    RebuildIfNeeded();
                    SceneFailed?.Invoke($"The preview could not be drawn from the client files: {ex.Message}");
                }

                if (_fitPending && !_bounds.IsEmpty)
                {
                    _fitPending = false;
                    Camera.Fit(_bounds, size, 2f, 16);
                    ZoomChanged?.Invoke();
                }

                if (!_bounds.IsEmpty)
                {
                    _hasView = true;
                }

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, size.Width, size.Height);
                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                _batch.Draw(_atlas, _hues, Camera.OriginX, Camera.OriginY, Camera.Zoom, size.Width, size.Height);
                SwapBuffers();
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

            _atlas = new ArtAtlas();
            _hues = new HueTexture();
            _batch = new MapBatch();
            _ready = true;
        }

        private void RebuildIfNeeded()
        {
            if (!_meshDirty)
            {
                return;
            }

            _meshDirty = false;
            if (_scene == null)
            {
                _items.Clear();
                _batch.Upload(Array.Empty<float>(), 0);
                _bounds = RectangleF.Empty;
                return;
            }

            if (_order == null || _orderStretched != _options.Stretch)
            {
                _order = _scene.Arrange(_options.Stretch);
                _orderStretched = _options.Stretch;
            }

            float[] vertices = MapMesh.Build(_scene, _order, _options, _atlas, _hues.Count, _items, out int quads,
                out RectangleF bounds);
            _batch.Upload(vertices, quads);

            // The framing follows the area, not what a filter left of it, so toggles do not jump the view.
            if (_bounds.IsEmpty || _sceneChanged)
            {
                _bounds = bounds;
                _sceneChanged = false;
            }
        }

        /// <summary>
        /// Renders the whole area at <paramref name="scale"/> into a bitmap with a transparent
        /// background, a tile at a time so no single framebuffer gets large.
        /// </summary>
        public Bitmap RenderImage(float scale)
        {
            if (!_ready || _bounds.IsEmpty)
            {
                return null;
            }

            MakeCurrent();
            RebuildIfNeeded();

            int width = Math.Max(1, (int)Math.Ceiling(_bounds.Width * scale));
            int height = Math.Max(1, (int)Math.Ceiling(_bounds.Height * scale));

            var image = new Bitmap(width, height, GdiPixelFormat.Format32bppArgb);
            int framebuffer = GL.GenFramebuffer();
            int color = GL.GenTexture();
            try
            {
                GL.BindTexture(TextureTarget.Texture2D, color);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, ExportTile, ExportTile, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                    TextureTarget.Texture2D, color, 0);

                byte[] pixels = new byte[ExportTile * ExportTile * 4];
                for (int ty = 0; ty < height; ty += ExportTile)
                {
                    for (int tx = 0; tx < width; tx += ExportTile)
                    {
                        int w = Math.Min(ExportTile, width - tx);
                        int h = Math.Min(ExportTile, height - ty);

                        GL.Viewport(0, 0, w, h);
                        GL.ClearColor(0f, 0f, 0f, 0f);
                        GL.Clear(ClearBufferMask.ColorBufferBit);
                        _batch.Draw(_atlas, _hues, _bounds.X + (tx / scale), _bounds.Y + (ty / scale), scale, w, h);

                        GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
                        GL.ReadPixels(0, 0, w, h, PixelFormat.Bgra, PixelType.UnsignedByte,
                            pixels);
                        CopyFlipped(pixels, w, h, image, tx, ty);
                    }
                }
            }
            finally
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.DeleteFramebuffer(framebuffer);
                GL.DeleteTexture(color);
                Invalidate();
            }

            return image;
        }

        /// <summary>The largest export scale up to 1:1 that keeps both sides within <see cref="MaxExportSize"/>.</summary>
        public float ExportScale()
        {
            if (_bounds.IsEmpty)
            {
                return 1f;
            }

            return Math.Min(1f, MaxExportSize / Math.Max(_bounds.Width, _bounds.Height));
        }

        private static unsafe void CopyFlipped(byte[] pixels, int width, int height, Bitmap image, int x, int y)
        {
            BitmapData data = image.LockBits(new Rectangle(x, y, width, height), ImageLockMode.WriteOnly,
                GdiPixelFormat.Format32bppArgb);
            try
            {
                for (int row = 0; row < height; row++)
                {
                    // GL rows run bottom up.
                    var source = new ReadOnlySpan<byte>(pixels, (height - 1 - row) * width * 4, width * 4);
                    var target = new Span<byte>((byte*)data.Scan0 + (row * data.Stride), width * 4);
                    source.CopyTo(target);
                }
            }
            finally
            {
                image.UnlockBits(data);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (Camera.ShowsFit && !_bounds.IsEmpty)
            {
                Camera.Fit(_bounds, ClientSize, 2f, 16);
                ZoomChanged?.Invoke();
            }

            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (e.Button is MouseButtons.Left or MouseButtons.Right or MouseButtons.Middle)
            {
                _panning = true;
                _panLast = e.Location;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_panning)
            {
                Camera.Pan(e.X - _panLast.X, e.Y - _panLast.Y);
                _panLast = e.Location;
                Invalidate();
                return;
            }

            string text;
            try
            {
                text = Describe(Camera.ScreenToWorld(e.Location));
            }
            catch (Exception)
            {
                // Names and art come from client files that may be mid-change; a hover line is not
                // worth an error.
                text = string.Empty;
            }

            HoverChanged?.Invoke(text);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_panning)
            {
                _panning = false;
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            Camera.ZoomStep(e.Location, Math.Sign(e.Delta));
            ZoomChanged?.Invoke();
            Invalidate();
        }

        /// <summary>Arrow keys reach KeyDown instead of moving focus to the next control.</summary>
        protected override bool IsInputKey(Keys keyData)
        {
            return (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);

            FitToView();
        }

        /// <summary>The topmost thing drawn under a world point, as a status line.</summary>
        private string Describe(PointF world)
        {
            if (_scene == null)
            {
                return string.Empty;
            }

            for (int i = _items.Count - 1; i >= 0; i--)
            {
                MeshItem item = _items[i];
                if (item.Kind == DrawableKind.Static)
                {
                    if (world.X < item.Left.X || world.X >= item.Right.X || world.Y < item.Top.Y ||
                        world.Y >= item.Bottom.Y)
                    {
                        continue;
                    }

                    SceneStatic s = _scene.Statics[item.Index];
                    if (!IsOpaque(s.Id, (int)(world.X - item.Left.X), (int)(world.Y - item.Top.Y)))
                    {
                        continue;
                    }

                    string name = s.Id < TileData.ItemTable.Length ? TileData.ItemTable[s.Id].Name : string.Empty;
                    return $"{_scene.Area.X + s.X},{_scene.Area.Y + s.Y},{s.Z}  static 0x{s.Id:X4} {name}" +
                           (s.Hue != 0 ? $"  hue {s.Hue & 0x3FFF}" : string.Empty);
                }

                if (!InQuad(world, item))
                {
                    continue;
                }

                int width = _scene.Area.Width;
                SceneLand land = _scene.Land[item.Index];
                string landName = land.Id < TileData.LandTable.Length ? TileData.LandTable[land.Id].Name : string.Empty;
                return $"{_scene.Area.X + (item.Index % width)},{_scene.Area.Y + (item.Index / width)},{land.Z}  " +
                       $"land 0x{land.Id:X4} {landName}" + (land.Stretched ? $"  texmap 0x{land.TextureId:X4}" : string.Empty);
            }

            return string.Empty;
        }

        private static bool IsOpaque(ushort id, int x, int y)
        {
            Bitmap art = Art.GetStatic(id);
            return art != null && x >= 0 && y >= 0 && x < art.Width && y < art.Height && art.GetPixel(x, y).A != 0;
        }

        /// <summary>Point in a convex quad given clockwise on screen: top, right, bottom, left.</summary>
        private static bool InQuad(PointF p, in MeshItem item)
        {
            return Side(item.Top, item.Right, p) >= 0 && Side(item.Right, item.Bottom, p) >= 0 &&
                   Side(item.Bottom, item.Left, p) >= 0 && Side(item.Left, item.Top, p) >= 0;
        }

        private static float Side(PointF a, PointF b, PointF p)
        {
            return ((b.X - a.X) * (p.Y - a.Y)) - ((b.Y - a.Y) * (p.X - a.X));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _ready)
            {
                try
                {
                    MakeCurrent();
                    _batch.Dispose();
                    _hues.Dispose();
                    _atlas.Dispose();
                }
                catch (Exception)
                {
                    // The context is already gone when the window is torn down first.
                }

                _ready = false;
            }

            base.Dispose(disposing);
        }
    }
}
