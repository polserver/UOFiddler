/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MapPreview.Rendering;
using UoFiddler.Plugin.MapPreview.Scene;

namespace UoFiddler.Plugin.MapPreview.UI
{
    /// <summary>
    /// A map area drawn as the client would, with the toggles to look into it. One window is kept
    /// and pointed at each new spot; its toggles are the saved <see cref="PreviewSettings"/>.
    /// </summary>
    public partial class MapPreviewForm : Form
    {
        /// <summary>The highest Z: a cut here or above it hides nothing.</summary>
        private const int NoCut = 127;

        /// <summary>The client cuts this far above the ground the player stands on.</summary>
        private const int RoofCut = 16;

        /// <summary>
        /// Heights one notch of the slider moves. A notch of 1 rebuilt the view at nearly every
        /// pixel of a drag and flickered.
        /// </summary>
        private const int CutStep = 5;

        /// <summary>The slider's notches: -125 to 125, and a last notch past them for no cut.</summary>
        private const int MinCutNotch = -128 / CutStep;

        private const int NoCutNotch = (NoCut / CutStep) + 1;

        private readonly MapPreviewCanvas _canvas;
        private readonly TrackBar _maxZ;

        private Map _map;

        /// <summary>Which of the six facets <see cref="_map"/> is, or -1; found again after a client reload.</summary>
        private int _facet = -1;

        private string _mapName;
        private Point _center;
        private Rectangle _area;

        /// <summary>True while controls are set from the settings, so they do not write them back.</summary>
        private bool _syncing;

        /// <summary>
        /// Statics at or above this are hidden; null shows all. Auto sets it exactly, between the
        /// slider's notches.
        /// </summary>
        private int? _cutZ;

        /// <summary>True while the slider is moved to show <see cref="_cutZ"/>, so it does not round it.</summary>
        private bool _showingCut;

        public MapPreviewForm()
        {
            InitializeComponent();

            _canvas = new MapPreviewCanvas { Dock = DockStyle.Fill };
            _canvas.ContextFailed += message =>
            {
                _canvas.Visible = false;
                _statusLabel.Text = message;
            };
            _canvas.HoverChanged += text => _statusLabel.Text = text;
            _canvas.ZoomChanged += () =>
                _zoomLabel.Text = (_canvas.Camera.Zoom * 100f).ToString("0.#", CultureInfo.InvariantCulture) + "%";
            Controls.Add(_canvas);
            _canvas.BringToFront();

            _maxZ = new TrackBar
            {
                Minimum = MinCutNotch,
                Maximum = NoCutNotch,
                Value = NoCutNotch,
                SmallChange = 1,
                LargeChange = 4,
                TickStyle = TickStyle.BottomRight,
                TickFrequency = 1,
                AutoSize = false,
                Height = 28,
                Width = 260
            };
            _maxZ.ValueChanged += (_, _) =>
            {
                if (!_showingCut)
                {
                    SetCut(_maxZ.Value >= NoCutNotch ? null : _maxZ.Value * CutStep);
                }
            };
            // Back to the preview after a drag: a focused slider takes the arrow keys meant for
            // moving the area, and End jumps it to no cut.
            _maxZ.MouseUp += (_, _) => _canvas.Focus();
            _viewStrip.Items.Insert(_viewStrip.Items.IndexOf(_maxZLabel) + 1,
                new ToolStripControlHost(_maxZ) { AutoSize = false, Width = 260, Height = 28, ToolTipText = $"Hide statics at or above this height (steps of {CutStep})" });

            foreach (ToolStripButton toggle in new[] { _landButton, _staticsButton, _stretchButton, _lightingButton, _huesButton, _roofsButton, _cutLandButton })
            {
                toggle.CheckedChanged += (_, _) => SaveToggles();
            }

            _autoZButton.Click += (_, _) => CutAtCenter();
            _fitButton.Click += (_, _) => _canvas.FitToView();
            _oneToOneButton.Click += (_, _) => _canvas.ZoomToActual();
            _moveLeftButton.Click += (_, _) => MoveArea(-1, 0);
            _moveUpButton.Click += (_, _) => MoveArea(0, -1);
            _moveDownButton.Click += (_, _) => MoveArea(0, 1);
            _moveRightButton.Click += (_, _) => MoveArea(1, 0);
            _canvas.KeyDown += OnCanvasKeyDown;

            foreach (int step in PreviewSettings.MoveSteps)
            {
                _stepBox.Items.Add(step == 1 ? "1 tile" : $"{step} tiles");
            }

            _stepBox.SelectedIndexChanged += (_, _) => SaveStep();

            _gotoButton.Click += (_, _) => GoTo();
            _gotoBox.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    GoTo();
                    e.Handled = e.SuppressKeyPress = true;
                }
            };
            _exportButton.Click += (_, _) => Export();
            _refreshButton.Click += (_, _) => OnClientReloaded();
            _settingsButton.Click += (_, _) => EditSettings();

            _canvas.SceneFailed += message =>
            {
                _canvas.Scene = null;
                _statusLabel.Text = message;
            };

            PreviewSettings.Changed += OnSettingsChanged;
            ControlEvents.FilePathChangeEvent += OnClientReloaded;
            ControlEvents.MapSizeChangeEvent += OnClientReloaded;
            ShowToggles(PreviewSettings.Current);
        }

        /// <summary>
        /// Opens the settings. A change of area size applies to the next preview and, when a window
        /// is open, to the spot it shows.
        /// </summary>
        public static void EditSettings(IWin32Window owner)
        {
            using var dialog = new MapPreviewSettingsDialog(PreviewSettings.Current);
            if (dialog.ShowDialog(owner) == DialogResult.OK)
            {
                dialog.Result.Apply();
            }
        }

        /// <summary>Shows the area the settings give around a tile, replacing what was shown.</summary>
        public void ShowArea(Map map, Point center)
        {
            ShowArea(map, center, true);
        }

        /// <param name="recut">
        /// Redo the cut for the new spot: by Auto when the settings ask for it, else none. Off
        /// keeps the cut the user set.
        /// </param>
        private void ShowArea(Map map, Point center, bool recut)
        {
            _map = map;
            _facet = Facets.IndexOf(map);
            _center = center;
            ShowCurrentArea(false, recut);
        }

        /// <summary>
        /// The client files were reloaded or the map size changed: the facet may be a new instance,
        /// a different size, or gone, and cached art and hues belong to the old files.
        /// </summary>
        private void OnClientReloaded()
        {
            if (IsDisposed || _map == null)
            {
                return;
            }

            if (_facet >= 0)
            {
                _map = Facets.Get(_facet);
            }

            ShowCurrentArea(true, false);
        }

        private void ShowCurrentArea(bool reloadArt, bool recut)
        {
            _mapName = Facets.Name(_facet, _map);
            _area = AreaAround(_map, _center, PreviewSettings.Current);

            Text = $"In-game preview - {_mapName} {_center.X},{_center.Y} ({_area.Width} x {_area.Height})";

            // Shows where the preview is, ready to edit, unless the user is typing in it.
            if (!_gotoBox.Focused)
            {
                _gotoBox.Text = $"{_center.X},{_center.Y}";
            }
            if (!LoadScene(reloadArt) || !recut)
            {
                return;
            }

            // A cut made for one spot means little at a far one, so it is redone or dropped.
            if (PreviewSettings.Current.AutoRoofCut)
            {
                CutAtCenter();
            }
            else
            {
                SetCut(null);
            }
        }

        /// <summary>
        /// Moves the area by the chosen step in a screen direction. On the isometric view screen
        /// right is map east plus north, and screen down is east plus south, so a step of n moves
        /// n tiles along both map axes: n diamond widths across the screen.
        /// </summary>
        private void MoveArea(int screenX, int screenY)
        {
            if (_map == null)
            {
                return;
            }

            // After a button click, so the arrow keys carry on moving the area.
            _canvas.Focus();

            int step = PreviewSettings.Current.MoveStep;
            int x = Math.Clamp(_center.X + ((screenX + screenY) * step), 0, _map.Width - 1);
            int y = Math.Clamp(_center.Y + ((screenY - screenX) * step), 0, _map.Height - 1);
            if (x == _center.X && y == _center.Y)
            {
                return;
            }

            // A step keeps the height being looked at; Auto, when set, follows the ground.
            ShowArea(_map, new Point(x, y), PreviewSettings.Current.AutoRoofCut);
        }

        /// <summary>Previews the area around the coordinates typed in the go-to box.</summary>
        private void GoTo()
        {
            if (_map == null)
            {
                return;
            }

            if (!TryParseLocation(_gotoBox.Text, out int x, out int y))
            {
                _statusLabel.Text = "Go to: type X and Y, for example 1430,1690 or 1430 1690.";
                return;
            }

            if (x < 0 || y < 0 || x >= _map.Width || y >= _map.Height)
            {
                _statusLabel.Text = $"Go to: {x},{y} is outside {_mapName} (0-{_map.Width - 1}, 0-{_map.Height - 1}).";
                return;
            }

            ShowArea(_map, new Point(x, y));
            _canvas.Focus();
        }

        /// <summary>
        /// "x,y", "x y", "x, y, z" and the like: the first two whole numbers. A z, as coordinates
        /// are often written in game, is allowed and ignored.
        /// </summary>
        internal static bool TryParseLocation(string text, out int x, out int y)
        {
            x = y = 0;
            string[] parts = (text ?? string.Empty).Split(new[] { ',', ' ', ';', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length is 2 or 3 &&
                   int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out x) &&
                   int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out y) &&
                   (parts.Length == 2 || int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out _));
        }

        private void OnCanvasKeyDown(object sender, KeyEventArgs e)
        {
            (int x, int y) = e.KeyCode switch
            {
                Keys.Left => (-1, 0),
                Keys.Right => (1, 0),
                Keys.Up => (0, -1),
                Keys.Down => (0, 1),
                _ => (0, 0)
            };

            if (x != 0 || y != 0)
            {
                MoveArea(x, y);
                e.Handled = true;
            }
        }

        /// <summary>The settings' area centred on a tile, moved inside the facet near its edges.</summary>
        private static Rectangle AreaAround(Map map, Point center, PreviewSettings settings)
        {
            if (map == null || map.Width <= 0 || map.Height <= 0)
            {
                return Rectangle.Empty;
            }

            int width = Math.Min(settings.AreaWidth, map.Width);
            int height = Math.Min(settings.AreaHeight, map.Height);
            int x = Math.Clamp(center.X - (width / 2), 0, Math.Max(0, map.Width - width));
            int y = Math.Clamp(center.Y - (height / 2), 0, Math.Max(0, map.Height - height));
            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Reads the area again. A client that cannot be read (a wrong path, missing files) leaves
        /// an empty preview with the reason in the status bar rather than an error dialog.
        /// </summary>
        private bool LoadScene(bool reloadArt)
        {
            if (_map == null)
            {
                return false;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                // Old art goes first, so nothing drawn from here on mixes the two clients.
                if (reloadArt)
                {
                    _canvas.ReloadArt();
                }

                // On the UI thread: the map caches are not safe to read from two threads, and the
                // map tab reads them from this one.
                _canvas.ShowScene(MapPreviewScene.Build(_map, _area), _center);
                _statusLabel.Text = $"{_canvas.Scene.Statics.Count:N0} statics";
                return true;
            }
            catch (Exception ex)
            {
                _canvas.Scene = null;
                _statusLabel.Text = $"The area could not be read from the client files: {ex.Message}";
                return false;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void EditSettings()
        {
            EditSettings(this);
        }

        private void OnSettingsChanged()
        {
            ShowToggles(PreviewSettings.Current);

            // A new size takes effect on the spot shown now, not only on the next click.
            if (_map != null && AreaAround(_map, _center, PreviewSettings.Current) != _area)
            {
                ShowCurrentArea(false, false);
            }
        }

        private void ShowToggles(PreviewSettings settings)
        {
            _syncing = true;
            try
            {
                _landButton.Checked = settings.ShowLand;
                _staticsButton.Checked = settings.ShowStatics;
                _stretchButton.Checked = settings.Stretch;
                _lightingButton.Checked = settings.Lighting;
                _huesButton.Checked = settings.Hues;
                _roofsButton.Checked = settings.HideRoofs;
                _cutLandButton.Checked = settings.CutLand;
                _stepBox.SelectedIndex = Math.Max(0, Array.IndexOf(PreviewSettings.MoveSteps, settings.MoveStep));
            }
            finally
            {
                _syncing = false;
            }

            ApplyOptions();
        }

        /// <summary>A toggle flipped here is remembered, so the next preview starts the same way.</summary>
        private void SaveToggles()
        {
            if (_syncing)
            {
                return;
            }

            PreviewSettings settings = PreviewSettings.Current.Clone();
            settings.ShowLand = _landButton.Checked;
            settings.ShowStatics = _staticsButton.Checked;
            settings.Stretch = _stretchButton.Checked;
            settings.Lighting = _lightingButton.Checked;
            settings.Hues = _huesButton.Checked;
            settings.HideRoofs = _roofsButton.Checked;
            settings.CutLand = _cutLandButton.Checked;
            settings.Apply();
        }

        private void SaveStep()
        {
            if (_syncing || _stepBox.SelectedIndex < 0)
            {
                return;
            }

            PreviewSettings settings = PreviewSettings.Current.Clone();
            settings.MoveStep = PreviewSettings.MoveSteps[_stepBox.SelectedIndex];
            settings.Apply();

            // Back to the preview, so the arrow keys move the area again.
            _canvas.Focus();
        }

        private void ApplyOptions()
        {
            _maxZLabel.Text = _cutZ is int z ? $"Max Z: {z}" : "Max Z: all";
            _canvas.Options = PreviewSettings.Current.ToOptions(_cutZ);
        }

        /// <summary>Cuts at a height, or at none, and puts the slider on the nearest notch.</summary>
        private void SetCut(int? z)
        {
            _cutZ = z;

            _showingCut = true;
            try
            {
                _maxZ.Value = z is int cut
                    ? Math.Clamp((int)Math.Round(cut / (double)CutStep), MinCutNotch, NoCutNotch - 1)
                    : NoCutNotch;
            }
            finally
            {
                _showingCut = false;
            }

            ApplyOptions();
        }

        private void CutAtCenter()
        {
            int z = GroundAt(_center) + RoofCut;
            SetCut(z >= NoCut ? null : Math.Max(z, -128));
        }

        /// <summary>
        /// Where someone standing on a tile would stand: the land, or a floor on it within a step
        /// or two.
        /// </summary>
        private int GroundAt(Point tile)
        {
            MapPreviewScene scene = _canvas.Scene;
            if (scene == null)
            {
                return 0;
            }

            int dx = Math.Clamp(tile.X - scene.Area.X, 0, scene.Area.Width - 1);
            int dy = Math.Clamp(tile.Y - scene.Area.Y, 0, scene.Area.Height - 1);
            int ground = scene.HeightAt(dx, dy);
            int reach = ground + RoofCut;

            foreach (SceneStatic s in scene.Statics)
            {
                if (s.X == dx && s.Y == dy && (s.Flags & TileFlag.Surface) != 0 && s.Z + s.Height <= reach)
                {
                    ground = Math.Max(ground, s.Z + s.Height);
                }
            }

            return ground;
        }

        private void Export()
        {
            if (_map == null)
            {
                return;
            }

            float scale = _canvas.ExportScale();
            using var dialog = new SaveFileDialog
            {
                Filter = "PNG image (*.png)|*.png",
                FileName = $"{_mapName}_{_area.X}_{_area.Y}_{_area.Width}x{_area.Height}.png"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            try
            {
                using Bitmap image = _canvas.RenderImage(scale);
                if (image == null)
                {
                    return;
                }

                image.Save(dialog.FileName, ImageFormat.Png);
                _statusLabel.Text = scale < 1f
                    ? $"Saved {image.Width} x {image.Height} at {scale * 100f:0.#}% (an image side is capped at {MapPreviewCanvas.MaxExportSize} pixels)"
                    : $"Saved {image.Width} x {image.Height}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Export PNG", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            else if (e.KeyCode == Keys.G && e.Control)
            {
                _gotoBox.Focus();
                _gotoBox.SelectAll();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            PreviewSettings.Changed -= OnSettingsChanged;
            ControlEvents.FilePathChangeEvent -= OnClientReloaded;
            ControlEvents.MapSizeChangeEvent -= OnClientReloaded;
            base.OnFormClosed(e);
        }
    }
}
