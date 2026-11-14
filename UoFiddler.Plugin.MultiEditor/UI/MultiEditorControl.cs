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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Rendering;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The multi editor tab: command bar and tool options on top, tool palette on the left,
    /// document tabs over the canvas in the middle, tile browser and inspector on the right,
    /// status line at the bottom. The fixed layout is made in the designer; the commands, the
    /// menus and buttons made from them and the settings wait for the first show.
    /// </summary>
    public partial class MultiEditorControl : UserControl
    {
        private sealed class EditorDocument
        {
            public EditorSession Session;
            public Camera2D Camera = new Camera2D();
            public int? ClientId;
            public bool Fitted;
        }

        private readonly ArtMasks _masks = new ArtMasks();
        private readonly List<EditorDocument> _documents = new List<EditorDocument>();
        private readonly CommandRegistry _commands = new CommandRegistry();
        private readonly Dictionary<ToolKind, EditorCommand> _toolCommands = new Dictionary<ToolKind, EditorCommand>();

        private bool _built;
        private bool _syncing;
        private HouseStyles _houseStyles;
        private EditorDocument _active;

        private Label _message;
        private readonly List<EditorCommand> _brushCommands = new List<EditorCommand>();
        private readonly List<ToolStripButton> _floorButtons = new List<ToolStripButton>();

        public MultiEditorControl()
        {
            InitializeComponent();
            SplitterGrip.Attach(_sideSplit);

            // Room after each tab's text for its close button, at any DPI.
            _docTabs.Padding = new Point((CloseGlyphSize + LogicalToDeviceUnits(16)) / 2, _docTabs.Padding.Y);
        }

        internal EditorSession Session => _active?.Session;

        internal GlCanvas Canvas => _canvas;

        // ---------------------------------------------------------------- entry points for the plugin

        /// <summary>Opens a multi from the loaded client files in a new document tab.</summary>
        public void OpenMulti(int id)
        {
            EnsureBuilt();
            MultiComponentList list = Multis.GetComponents(id);
            if (!_built || list == null || list == MultiComponentList.Empty)
            {
                return;
            }

            AddDocument(new OpenedMulti(list, $"Multi 0x{id:X4}", id));
        }

        /// <summary>Makes a tile the draw tile (Items tab context menu).</summary>
        public void SelectDrawTile(ushort id)
        {
            EnsureBuilt();
            if (Session != null)
            {
                Session.SetDrawTile(id);
                Session.Tool = ToolKind.Draw;
            }
        }

        /// <summary>Asks for an area of a map and starts pasting its statics (Map tab context menu).</summary>
        public void ExtractFromMap(Map map, Point location)
        {
            EnsureBuilt();
            if (!_built)
            {
                return;
            }

            List<ClipboardTile> tiles = DocumentIO.ExtractFromMap(this, map, location);
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            if (Session == null)
            {
                int width = tiles.Max(t => t.Dx) + 1;
                int height = tiles.Max(t => t.Dy) + 1;
                NewDocument(width, height, "Map extract");
            }

            Session.BeginPaste(tiles);
            _canvas.Focus();
        }

        // ---------------------------------------------------------------- construction

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible && !DesignMode)
            {
                EnsureBuilt();
                WatchHostForm();
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            WatchHostForm();
        }

        // ---------------------------------------------------------------- window resizing

        private Form _hostForm;
        private Timer _resizeTimer;
        private bool _sidebarsHeld;

        /// <summary>
        /// Starts OpenGL's windowing library ahead of the first show. It takes around 400 ms once
        /// per process, which is better spent while the application sits idle after start-up than
        /// when the tab is first clicked.
        /// </summary>
        internal static void WarmUpOpenGl()
        {
            try
            {
                OpenTK.Windowing.Desktop.GLFWProvider.EnsureInitialized();
            }
            catch (Exception)
            {
                // The canvas reports the problem when it is created.
            }
        }

        /// <summary>The tab can be undocked into its own window, so the form is looked up again each time.</summary>
        private void WatchHostForm()
        {
            Form form = FindForm();
            if (form == _hostForm)
            {
                return;
            }

            if (_hostForm != null)
            {
                _hostForm.ResizeBegin -= HostForm_ResizeBegin;
                _hostForm.ResizeEnd -= HostForm_ResizeEnd;
            }

            _hostForm = form;
            if (form != null)
            {
                form.ResizeBegin += HostForm_ResizeBegin;
                form.ResizeEnd += HostForm_ResizeEnd;
            }
        }

        /// <summary>
        /// While the window border is dragged, the two sidebars (hundreds of nested controls, most
        /// of a resize's cost) are laid out a few times a second instead of on every mouse step.
        /// The canvas and toolbars keep following the window live.
        /// </summary>
        private void HostForm_ResizeBegin(object sender, EventArgs e)
        {
            if (!_built || _sidebarsHeld)
            {
                return;
            }

            // Undocked, the sidebars keep their size and just ride along with their panel, which
            // costs nothing; docked, every step would lay out everything inside them.
            _sidebarsHeld = true;
            _sideSplit.Dock = DockStyle.None;
            _resizeTimer ??= new Timer { Interval = 300 };
            _resizeTimer.Tick -= ResizeTimer_Tick;
            _resizeTimer.Tick += ResizeTimer_Tick;
            _resizeTimer.Start();
        }

        private void ResizeTimer_Tick(object sender, EventArgs e)
        {
            if (_sidebarsHeld && _sideSplit.Parent != null && _sideSplit.Bounds != _sideSplit.Parent.ClientRectangle)
            {
                _sideSplit.Bounds = _sideSplit.Parent.ClientRectangle;
            }
        }

        private void HostForm_ResizeEnd(object sender, EventArgs e)
        {
            if (!_sidebarsHeld)
            {
                return;
            }

            _sidebarsHeld = false;
            _resizeTimer?.Stop();
            _sideSplit.Dock = DockStyle.Fill;
            LayoutSidebars();
        }

        private void EnsureBuilt()
        {
            if (_built || _message != null)
            {
                return;
            }

            EditorSettings.Load();
            _browserShown = EditorSettings.Current.ShowTileBrowser;
            _inspectorShown = EditorSettings.Current.ShowInspector;
            EditorSettings.Changed += OnSettingsChanged;
            Disposed += (_, _) => EditorSettings.Changed -= OnSettingsChanged;

            try
            {
                _houseStyles = HouseStyles.Load(splitFloorsByColour: true);
            }
            catch (Exception ex)
            {
                ShowMessage($"OpenGL could not be started:\r\n{ex.Message}");
                return;
            }

            SuspendLayout();
            DefineCommands();
            BuildCanvasMenu();

            _browser.SetHouseStyles(_houseStyles);
            _browser.TileChosen += id =>
            {
                Session?.SetDrawTile(id);
                if (Session != null && !IsTileTool(Session.Tool))
                {
                    Session.Tool = ToolKind.Draw;
                }
            };
            _browser.DesignOpenRequested += designs =>
            {
                if (!ConfirmOpenMany(designs.Count))
                {
                    return;
                }

                foreach (UoaDesign design in designs)
                {
                    OpenedMulti opened = LoadDesign(() => DocumentIO.FromDesign(design));
                    if (opened != null)
                    {
                        AddDocument(opened);
                    }
                }
            };
            _browser.DesignPlaceRequested += design =>
            {
                List<ClipboardTile> tiles = LoadDesign(() => DesignLibraryPanel.ToTiles(UoaDesignFile.Load(design)));
                if (tiles == null || tiles.Count == 0)
                {
                    return;
                }

                if (Session == null)
                {
                    NewDocument(tiles.Max(t => t.Dx) + 1, tiles.Max(t => t.Dy) + 1, DocumentIO.DesignTitle(design));
                }

                Session.BeginPaste(tiles);
                _canvas.Focus();
            };
            _browser.HouseStyleChosen += ChooseHouseStyle;
            _browser.FloorPatternChosen += pattern =>
            {
                if (Session != null)
                {
                    Session.FloorPattern = pattern;
                    SetTool(ToolKind.FloorPattern);
                }
            };
            _browser.FoundationChosen += () =>
            {
                // The foundation tool builds with the wall style its posts belong to.
                if (Session != null && _browser.FoundationStyle != null)
                {
                    Session.HouseStyle = _browser.FoundationStyle;
                }

                SetTool(ToolKind.Foundation, fromHousePage: true);
            };
            _browser.SwapToHouseStyleRequested += style =>
            {
                if (Session != null && style != null)
                {
                    Session.HouseStyle = style;
                    Session.SwapSelectionToHouseStyle();
                }
            };
            _browser.PrefabChosen += tiles =>
            {
                Session?.BeginPaste(tiles);
                _canvas.Focus();
            };

            // The hint takes its colours from the settings and its font from the host.
            _emptyHint.ForeColor = EditorSettings.Current.BackgroundText;
            _emptyHint.BackColor = EditorSettings.Current.EffectiveBackground;
            _emptyHint.Font = new Font(Font.FontFamily, 11f);

            // The strips and the sidebars are laid out in the designer (docking runs from the last
            // added control: strips first, then the sidebars on the right, the tool palette on the
            // left, and the canvas area takes what is left). Here they get the command items.
            BuildToolPalette();
            BuildOptionsStrip();
            BuildMainStrip();

            // The performance box floats over the canvas's top right corner (F12).
            _performancePanel = new PerformancePanel { Visible = false };
            _centre.Controls.Add(_performancePanel);
            _performancePanel.BringToFront();
            _performancePanel.Bind(_canvas.Stats, () => Session?.CompList.TileCount ?? 0);
            _performancePanel.SizeChanged += (_, _) => PlacePerformancePanel();
            _centre.Resize += (_, _) => PlacePerformancePanel();
            ResumeLayout(true);

            // Splitter distances only hold once the containers have their real size.
            LayoutSidebars();

            ControlEvents.FilePathChangeEvent += OnClientFilesChanged;
            ControlEvents.ItemChangeEvent += OnItemChanged;
            ControlEvents.TileDataChangeEvent += OnItemChanged;
            Disposed += (_, _) =>
            {
                ControlEvents.FilePathChangeEvent -= OnClientFilesChanged;
                ControlEvents.ItemChangeEvent -= OnItemChanged;
                ControlEvents.TileDataChangeEvent -= OnItemChanged;
            };

            _built = true;
            Activate(-1);
        }

        /// <summary>
        /// The host's tab control has an "UnDock" menu that every child without a menu of its own
        /// inherits. An empty menu that never opens keeps it off the canvas, toolbars and panels;
        /// undocking stays on the tab header.
        /// </summary>
        private void HostMenuBlocker_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void DocTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Activate(_docTabs.SelectedIndex);
        }

        /// <summary>
        /// The sidebars sit in a panel docked right with a plain splitter, not in a SplitContainer:
        /// they keep their width when the window is resized, and docked panels lay out far more cheaply.
        /// Between the two sidebars (the tile browser at full height, and beside it floors and
        /// selection) is a SplitContainer.
        /// </summary>
        private void SideSplitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            RememberSidebarWidths();
        }

        private void SideSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            RememberSidebarWidths();
        }

        private void ShowMessage(string text)
        {
            if (_message == null)
            {
                _message = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                Controls.Add(_message);
            }

            foreach (Control control in Controls)
            {
                control.Visible = control == _message;
            }

            _message.Text = text;
        }

        private void OnClientFilesChanged()
        {
            NewMultiDialog.Reset();
            ResetArt();
        }

        private void OnItemChanged(object sender, int id)
        {
            ResetArt();
        }

        private void ResetArt()
        {
            _masks.Clear();
            _canvas.ResetArt();
            _browser.ReloadData();
        }

        // ---------------------------------------------------------------- commands

        private EditorCommand Command(string group, string text, Action execute, Keys shortcut = Keys.None,
            Func<bool> can = null, Func<bool> isChecked = null, string glyph = null, Keys alt = Keys.None)
        {
            return _commands.Add(new EditorCommand(group, text, execute)
            {
                Shortcut = shortcut,
                AltShortcut = alt,
                CanExecute = can,
                IsChecked = isChecked,
                Glyph = glyph
            });
        }

        private EditorCommand _new, _open, _import, _importFolder, _saveClient, _export, _savePrefab, _resize, _close, _closeAll;
        private EditorCommand _undo, _redo, _cut, _copy, _paste, _delete;
        private EditorCommand _selectAll, _invert, _selectConnected, _selectSameId, _replaceId;
        private EditorCommand _rotateLeft, _rotateRight, _mirrorX, _mirrorY, _swapStyle, _hide, _showHidden;
        private EditorCommand _grid, _hideWalls, _walkable, _doubleSurface, _hideAbove, _levelUp, _levelDown;
        private EditorCommand _fit, _zoomIn, _zoomOut, _zoomReset, _shortcuts;
        private EditorCommand _zUp, _zDown, _planeUp, _planeDown, _cyclePlacement, _escape;
        private EditorCommand _drawWithSelected, _workPlane;
        private ContextMenuStrip _canvasMenu;
        private ToolStripMenuItem _canvasMenuTitle;
        private EditorCommand _preview, _moveReference, _planView, _allFloors, _showBrowser, _showInspector;
        private bool _browserShown = true;
        private bool _inspectorShown = true;
        private bool _layingOutSidebars;
        private bool _applyingOwnSettings;
        private EditorCommand _settingsCommand;
        private EditorCommand _performanceInfo;
        private PerformancePanel _performancePanel;
        private bool _performanceShown;
        private EditorCommand _savePattern;
        private bool _openingHousePage;
        private readonly List<EditorCommand> _terrainCommands = new List<EditorCommand>();
        private ContextMenuStrip _previewMenu;

        private void DefineCommands()
        {
            Func<bool> hasDoc = () => Session != null;
            Func<bool> hasSelection = () => Session != null && Session.Selection.Count > 0;

            _new = Command("File", "New multi...", NewDocumentPrompt, Keys.Control | Keys.N, glyph: Glyphs.New);
            _open = Command("File", "Open from client...", OpenFromClient, Keys.Control | Keys.O, glyph: Glyphs.Open);
            _import = Command("File", "Import files...", ImportFile, Keys.Control | Keys.I);
            _importFolder = Command("File", "Import from folder...", ImportFolder, Keys.Control | Keys.Shift | Keys.I);
            _saveClient = Command("File", "Save to client...", SaveToClient, Keys.Control | Keys.S, hasDoc, glyph: Glyphs.Save);
            _export = Command("File", "Export file...", ExportFile, Keys.Control | Keys.E, hasDoc);
            _savePrefab = Command("File", "Save selection as blueprint...", SavePrefab, can: hasSelection);
            _savePattern = Command("File", "Save selection as floor pattern...", SaveFloorPattern, can: hasSelection);
            _resize = Command("File", "Resize multi...", ResizePrompt, can: hasDoc);
            _settingsCommand = Command("File", "Settings...", ShowSettings);
            _close = Command("File", "Close design", () => CloseDocument(_active), Keys.Control | Keys.W, hasDoc);
            _closeAll = Command("File", "Close all designs", CloseAllDocuments, Keys.Control | Keys.Shift | Keys.W, hasDoc);

            _undo = Command("Edit", "Undo", () => Session.Undo(), Keys.Control | Keys.Z,
                () => Session != null && Session.CompList.UndoList.Count > 0, glyph: Glyphs.Undo);
            _redo = Command("Edit", "Redo", () => Session.Redo(), Keys.Control | Keys.Y,
                () => Session != null && Session.CompList.RedoList.Count > 0, glyph: Glyphs.Redo, alt: Keys.Control | Keys.Shift | Keys.Z);
            _cut = Command("Edit", "Cut", () => Session.Cut(), Keys.Control | Keys.X, hasSelection, glyph: Glyphs.Cut);
            _copy = Command("Edit", "Copy", () => Session.Copy(), Keys.Control | Keys.C, hasSelection, glyph: Glyphs.Copy);
            _paste = Command("Edit", "Paste", () => Session.Paste(), Keys.Control | Keys.V, hasDoc, glyph: Glyphs.Paste);
            _delete = Command("Edit", "Delete selection", () => Session.DeleteSelection(), Keys.Delete, hasSelection, glyph: Glyphs.Delete);

            _selectAll = Command("Select", "Select all", () => Session.SelectAll(), Keys.Control | Keys.A, hasDoc);
            _invert = Command("Select", "Invert selection", () => Session.InvertSelection(), Keys.Control | Keys.Shift | Keys.A, hasDoc);
            _selectConnected = Command("Select", "Grow to connected tiles of the same id", () => Session.SelectConnected(), Keys.Control | Keys.G, hasSelection);
            _selectSameId = Command("Select", "Select all tiles with the same id", SelectSameId, Keys.Control | Keys.Shift | Keys.G, hasSelection);
            _replaceId = Command("Select", "Replace tile id...", ReplaceIdPrompt, Keys.Control | Keys.H, hasDoc);

            _rotateLeft = Command("Arrange", "Rotate left", () => Transform(GroupTransformKind.RotateLeft), Keys.Oemcomma, hasDoc);
            _rotateRight = Command("Arrange", "Rotate right", () => Transform(GroupTransformKind.RotateRight), Keys.OemPeriod, hasDoc);
            _mirrorX = Command("Arrange", "Mirror X", () => Transform(GroupTransformKind.MirrorX), can: hasDoc);
            _mirrorY = Command("Arrange", "Mirror Y", () => Transform(GroupTransformKind.MirrorY), can: hasDoc);
            _swapStyle = Command("Arrange", "Swap selection to the chosen house style", () => Session.SwapSelectionToHouseStyle(),
                Keys.Control | Keys.R, () => Session != null && Session.Selection.Count > 0 && Session.HouseStyle != null);
            _zUp = Command("Arrange", "Raise selection (Z +1)", () => Session.ShiftSelectionZ(1), Keys.OemCloseBrackets, hasSelection, alt: Keys.Shift | Keys.Up);
            _zDown = Command("Arrange", "Lower selection (Z -1)", () => Session.ShiftSelectionZ(-1), Keys.OemOpenBrackets, hasSelection, alt: Keys.Shift | Keys.Down);
            _hide = Command("Arrange", "Hide selection", () => Session.HideSelection(), Keys.H, hasSelection);
            _showHidden = Command("Arrange", "Show hidden tiles", () => Session.ShowHiddenTiles(), Keys.Shift | Keys.H, hasDoc);

            _workPlane = Command("View", "Work plane", () => Session.ShowWorkPlane = !Session.ShowWorkPlane,
                Keys.F, hasDoc, () => Session?.ShowWorkPlane == true);
            _grid = Command("View", "Grid", () => Session.ShowGrid = !Session.ShowGrid, Keys.G, hasDoc, () => Session?.ShowGrid == true, Glyphs.Grid);
            _hideWalls = Command("View", "Hide walls", () => Session.HideWalls = !Session.HideWalls, Keys.W, hasDoc, () => Session?.HideWalls == true);
            _walkable = Command("View", "Mark walkable tiles", () => Session.ShowWalkables = !Session.ShowWalkables, can: hasDoc, isChecked: () => Session?.ShowWalkables == true);
            _doubleSurface = Command("View", "Mark double surfaces", () => Session.ShowDoubleSurface = !Session.ShowDoubleSurface, can: hasDoc, isChecked: () => Session?.ShowDoubleSurface == true);
            _hideAbove = Command("View", "Hide floors above the active one", () => Session.HideAboveLevel = !Session.HideAboveLevel, can: hasDoc, isChecked: () => Session?.HideAboveLevel == true);
            _levelUp = Command("View", "Floor up", () => Session.StepLevel(1), Keys.PageUp, hasDoc);
            _levelDown = Command("View", "Floor down", () => Session.StepLevel(-1), Keys.PageDown, hasDoc);
            _planeUp = Command("View", "Work plane up 1", () => Session.MoveWorkPlane(1), Keys.Control | Keys.PageUp, hasDoc);
            _planeDown = Command("View", "Work plane down 1", () => Session.MoveWorkPlane(-1), Keys.Control | Keys.PageDown, hasDoc);
            _allFloors = Command("View", "All floors", () => Session.LevelIndex = 0, Keys.Control | Keys.Home, hasDoc, () => Session?.LevelIndex == 0);
            _showBrowser = Command("View", "Tile browser sidebar", () => ToggleSidebar(browser: true), Keys.F9, isChecked: () => _browserShown);
            _showInspector = Command("View", "Floors and selection sidebar", () => ToggleSidebar(browser: false), Keys.F10, isChecked: () => _inspectorShown);
            _preview = Command("View", "Preview as in the client", () => Session.PreviewMode = !Session.PreviewMode,
                Keys.F5, hasDoc, () => Session?.PreviewMode == true);
            _planView = Command("View", "Plan view (top-down)", TogglePlanView, Keys.V, hasDoc, () => Session?.PlanMode == true);
            foreach (PreviewTerrain terrain in Enum.GetValues<PreviewTerrain>())
            {
                string name = terrain == PreviewTerrain.None ? "No ground" : terrain.ToString();
                _terrainCommands.Add(Command("View", $"Preview on: {name}", () => SetPreviewTerrain(terrain), can: hasDoc,
                    isChecked: () => Session?.PreviewTerrain == terrain));
            }

            _moveReference = Command("View", "Move reference...", MoveReferencePrompt, can: () => Session?.Reference != null);
            _fit = Command("View", "Fit to window", () => _canvas.FitToView(), Keys.Home, hasDoc, glyph: Glyphs.Fit);
            _zoomIn = Command("View", "Zoom in", () => _canvas.ZoomStep(1), Keys.Add, glyph: Glyphs.ZoomIn, alt: Keys.Shift | Keys.Oemplus);
            _zoomOut = Command("View", "Zoom out", () => _canvas.ZoomStep(-1), Keys.Subtract, glyph: Glyphs.ZoomOut, alt: Keys.OemMinus);
            _zoomReset = Command("View", "Zoom 100%", () => _canvas.SetZoom(1f), Keys.Control | Keys.D0, alt: Keys.Control | Keys.NumPad0);
            _performanceInfo = Command("View", "Performance info", TogglePerformanceInfo, Keys.F12, isChecked: () => _performanceShown);

            AddTool(ToolKind.Select, "Select", Keys.S, Glyphs.Select);
            AddTool(ToolKind.Lasso, "Lasso", Keys.Q, Glyphs.Lasso);
            AddTool(ToolKind.Draw, "Draw", Keys.D, Glyphs.Draw);
            AddTool(ToolKind.Remove, "Remove", Keys.R, Glyphs.Remove);
            AddTool(ToolKind.Altitude, "Altitude", Keys.E, Glyphs.Altitude);
            AddTool(ToolKind.Pipette, "Pick tile", Keys.P, Glyphs.Pipette);
            AddTool(ToolKind.Transparent, "See through", Keys.T, Glyphs.Transparent);
            AddTool(ToolKind.RectFill, "Rectangle", Keys.B, Glyphs.RectFill);
            AddTool(ToolKind.Line, "Line", Keys.L, Glyphs.Line);
            AddTool(ToolKind.Wall, "Walls", Keys.D1, Glyphs.Wall);
            AddTool(ToolKind.Floor, "Floors", Keys.D2, Glyphs.Floor);
            AddTool(ToolKind.Door, "Doors", Keys.D3, Glyphs.Door);
            AddTool(ToolKind.Stair, "Stairs", Keys.D4, Glyphs.Stair);
            AddTool(ToolKind.Roof, "Roofs", Keys.D5, Glyphs.Roof);
            AddTool(ToolKind.Foundation, "Foundation", Keys.D6, Glyphs.Foundation);
            AddTool(ToolKind.FloorPattern, "Floor pattern", Keys.None, Glyphs.Floor);

            _cyclePlacement = Command("Tools", "Next placement mode", CyclePlacement, Keys.M, hasDoc);
            foreach ((int size, Keys key) in new[] { (1, Keys.D1), (3, Keys.D2), (5, Keys.D3) })
            {
                _brushCommands.Add(Command("Tools", $"Brush {size}x{size}", () => Session.BrushSize = size, Keys.Control | key,
                    hasDoc, () => Session?.BrushSize == size));
            }
            _escape = Command("Tools", "Cancel tool, then clear selection", Escape, Keys.Escape, hasDoc);
            _shortcuts = Command("Help", "Keyboard shortcuts", ShowShortcuts, Keys.F1, glyph: Glyphs.Help);
            _drawWithSelected = Command("Tools", "Draw with this tile", DrawWithSelected, can: () => Session?.SelectedTile != null);
        }

        private void AddTool(ToolKind kind, string text, Keys key, string glyph)
        {
            _toolCommands[kind] = Command("Tools", text, () => SetTool(kind), key, () => Session != null,
                () => Session != null && !Session.Pasting && Session.Tool == kind, glyph);
        }

        private static bool IsTileTool(ToolKind kind)
        {
            return kind is ToolKind.Draw or ToolKind.RectFill or ToolKind.Line;
        }

        private static bool IsHouseTool(ToolKind kind)
        {
            return kind is ToolKind.Wall or ToolKind.Floor or ToolKind.Door or ToolKind.Stair or ToolKind.Roof or ToolKind.Foundation;
        }

        private void SetTool(ToolKind kind, bool fromHousePage = false)
        {
            if (Session == null)
            {
                return;
            }

            Session.Tool = kind;

            // A house tool builds with a style from the House page: bring that page up on the
            // matching kind. The page answers by choosing the style, which comes back here.
            if (IsHouseTool(kind) && !fromHousePage && !_openingHousePage && _browserShown)
            {
                _openingHousePage = true;
                try
                {
                    HousePartKind part = kind switch
                    {
                        ToolKind.Floor => HousePartKind.Floor,
                        ToolKind.Door => HousePartKind.Door,
                        ToolKind.Stair => HousePartKind.Stair,
                        ToolKind.Roof => HousePartKind.Roof,
                        _ => HousePartKind.Wall
                    };
                    _browser.ShowHousePage(part, foundation: kind == ToolKind.Foundation);
                }
                finally
                {
                    _openingHousePage = false;
                }
            }

            _canvas.Focus();
        }

        private void ChooseHouseStyle(HouseStyle style)
        {
            if (Session == null || style == null)
            {
                return;
            }

            Session.HouseStyle = style;
            ToolKind kind = style.Kind switch
            {
                HousePartKind.Floor => ToolKind.Floor,
                HousePartKind.Door => ToolKind.Door,
                HousePartKind.Stair => ToolKind.Stair,
                HousePartKind.Roof => ToolKind.Roof,
                _ => ToolKind.Wall
            };
            SetTool(kind, fromHousePage: true);
        }

        private void CyclePlacement()
        {
            Session.Placement = (PlacementMode)(((int)Session.Placement + 1) % 3);
            SyncOptions();
        }

        private void Escape()
        {
            // First Esc stops what the tool is doing, the next one clears the selection.
            if (Session.Pasting || Session.ActiveTool.IsDragging || Session.Selection.Count == 0)
            {
                Session.CancelTool();
            }
            else
            {
                Session.ClearSelection();
            }
        }

        private void Transform(GroupTransformKind kind)
        {
            string problem = Session.TransformGroup(kind);
            if (problem != null)
            {
                MessageBox.Show(this, problem, "Multi Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SelectSameId()
        {
            var ids = new HashSet<ushort>(Session.Selection.Select(t => t.Id));
            Session.SelectWhere(t => ids.Contains(t.Id), add: false);
        }

        private void ReplaceIdPrompt()
        {
            MultiTile selected = Session.SelectedTile ?? Session.Selection.FirstOrDefault();
            string from = Prompt.Text(this, "Replace tile id", "Tile id to replace (decimal or 0x hex):",
                selected != null ? $"0x{selected.Id:X4}" : string.Empty);
            if (from == null || !Prompt.TryParseId(from, out ushort fromId))
            {
                return;
            }

            string to = Prompt.Text(this, "Replace tile id", $"Replace 0x{fromId:X4} with (decimal or 0x hex):", $"0x{Session.DrawTileId:X4}");
            if (to == null || !Prompt.TryParseId(to, out ushort toId))
            {
                return;
            }

            bool selectionOnly = Session.Selection.Count > 1;
            int count = Session.ReplaceTileId(fromId, toId, selectionOnly);
            _statusHint.Text = $"Replaced {count} tile(s)" + (selectionOnly ? " in the selection" : string.Empty);
        }

        private void SavePrefab()
        {
            List<ClipboardTile> tiles = Session.SelectionAsClipboard();
            string name = tiles != null ? Prompt.Text(this, "Save blueprint", "Blueprint name:") : null;
            if (name == null)
            {
                return;
            }

            _browser.SaveBlueprint(name, tiles);
        }

        private void DrawWithSelected()
        {
            Session.SetDrawTile(Session.SelectedTile.Id);
            Session.Tool = ToolKind.Draw;
        }

        private void BuildCanvasMenu()
        {
            _canvasMenu = new ContextMenuStrip();
            _canvasMenuTitle = new ToolStripMenuItem { Enabled = false };
            _canvasMenu.Items.Add(_canvasMenuTitle);
            _canvasMenu.Items.Add(_commands.MenuItem(_drawWithSelected));
            var brushMenu = new ToolStripMenuItem("Brush size");
            foreach (EditorCommand brush in _brushCommands)
            {
                brushMenu.DropDownItems.Add(_commands.MenuItem(brush));
            }

            _canvasMenu.Items.Add(brushMenu);
            _canvasMenu.Items.Add(new ToolStripSeparator());
            foreach (EditorCommand command in new[] { _cut, _copy, _paste, _delete })
            {
                _canvasMenu.Items.Add(_commands.MenuItem(command));
            }

            _canvasMenu.Items.Add(new ToolStripSeparator());
            foreach (EditorCommand command in new[] { _zUp, _zDown, _rotateLeft, _rotateRight, _hide, _showHidden })
            {
                _canvasMenu.Items.Add(_commands.MenuItem(command));
            }

            _canvasMenu.Items.Add(new ToolStripSeparator());
            foreach (EditorCommand command in new[] { _selectConnected, _selectSameId, _selectAll, _savePrefab, _savePattern })
            {
                _canvasMenu.Items.Add(_commands.MenuItem(command));
            }

            _canvasMenu.Items.Add(new ToolStripSeparator());
            foreach (EditorCommand command in new[] { _workPlane, _grid, _fit })
            {
                _canvasMenu.Items.Add(_commands.MenuItem(command));
            }
        }

        /// <summary>
        /// Right click on the canvas: the menu acts on the selection, so a tile under the cursor
        /// that is not part of it becomes the selection first.
        /// </summary>
        /// <summary>Picks the preview ground for this document and as the default for the next ones.</summary>
        private void SetPreviewTerrain(PreviewTerrain terrain)
        {
            Session.PreviewTerrain = terrain;
            EditorSettings settings = EditorSettings.Current.Clone();
            settings.PreviewTerrain = (int)terrain;
            _applyingOwnSettings = true;
            settings.Apply();
            _applyingOwnSettings = false;
        }

        private void ShowCanvasMenu(Point location)
        {
            EditorSession session = Session;
            if (session == null)
            {
                return;
            }

            if (session.PreviewMode)
            {
                // Nothing can be edited in preview: the menu offers the ground and the way out.
                if (_previewMenu == null)
                {
                    _previewMenu = new ContextMenuStrip();
                    foreach (EditorCommand terrain in _terrainCommands)
                    {
                        _previewMenu.Items.Add(_commands.MenuItem(terrain));
                    }

                    _previewMenu.Items.Add(new ToolStripSeparator());
                    ToolStripMenuItem leave = _commands.MenuItem(_preview);
                    leave.Text = "Leave preview";
                    _previewMenu.Items.Add(leave);
                    _previewMenu.Items.Add(_commands.MenuItem(_fit));
                }

                _commands.RefreshStates();
                _previewMenu.Show(_canvas, location);
                return;
            }

            MultiTile hover = session.HoverTile;
            if (hover != null && !session.IsSelected(hover))
            {
                session.SetSelection(new[] { hover }, add: false);
            }

            MultiTile single = session.SelectedTile;
            int count = session.Selection.Count;
            _canvasMenuTitle.Text = single != null ? $"0x{single.Id:X4} {TileName(single.Id)}  (Z {single.Z})"
                : count > 1 ? $"{count} tiles selected"
                : "Nothing selected";

            _commands.RefreshStates();
            _canvasMenu.Show(_canvas, location);
        }

        /// <summary>
        /// Learns a bordered floor from the selection (a rectangle laid out by hand: corners, sides
        /// and fill) and shows it on the House page, ready to lay at any size.
        /// </summary>
        private void SaveFloorPattern()
        {
            string name = Prompt.Text(this, "Save floor pattern", "Pattern name:");
            if (name == null)
            {
                return;
            }

            FloorPattern pattern = FloorPattern.Learn(Session.Selection, name, out string problem);
            if (pattern == null)
            {
                MessageBox.Show(this, problem, "Multi Editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            FloorPatterns.Save(pattern);
            _browser.ShowPatterns(pattern);
        }

        /// <summary>Reads a design from the library, with a message instead of an exception when its file is gone or damaged.</summary>
        private T LoadDesign<T>(Func<T> load) where T : class
        {
            try
            {
                using (new WaitCursorScope(this))
                {
                    return load();
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, "The design could not be read: " + ex.Message, "Multi Editor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void ShowShortcuts()
        {
            using var form = new ShortcutsForm();
            form.ShowCommands(_commands.Commands);
            form.ShowDialog(this);
        }

        /// <summary>Lists the other open documents: one of them can be shown faintly under the active one.</summary>
        private void FillReferenceMenu(ToolStripMenuItem menu)
        {
            menu.DropDownItems.Clear();
            menu.Enabled = Session != null;
            if (Session == null)
            {
                return;
            }

            var none = new ToolStripMenuItem("None") { Checked = Session.Reference == null };
            none.Click += (_, _) => Session.SetReference(null);
            menu.DropDownItems.Add(none);

            foreach (EditorDocument document in _documents)
            {
                if (document == _active)
                {
                    continue;
                }

                MultiEditorComponentList list = document.Session.CompList;
                var item = new ToolStripMenuItem(document.Session.Title) { Checked = Session.Reference == list };
                item.Click += (_, _) => Session.SetReference(list);
                menu.DropDownItems.Add(item);
            }

            if (menu.DropDownItems.Count == 1)
            {
                menu.DropDownItems.Add(new ToolStripMenuItem("(open another multi to use it as a reference)") { Enabled = false });
            }
        }

        /// <summary>Narrowest a sidebar gets: below this its controls no longer fit.</summary>
        private const int _minSidebar = 240;

        /// <summary>
        /// Gives each sidebar its width from the settings, never below the minimum; a hidden one
        /// hands its space to the canvas. When the window is too narrow for both at their saved
        /// widths, the tile browser gives way first, then the other one, and the canvas keeps a
        /// strip. Safe to call at any time: it is run again whenever the editor changes size,
        /// because at start-up the first layout happens before the window has its real width.
        /// </summary>
        private void LayoutSidebars()
        {
            if (_sideSplit == null || _sidePanel == null)
            {
                return;
            }

            const int paletteWidth = 96;
            const int minCanvas = 200;
            int browser = Math.Max(_minSidebar, EditorSettings.Current.TileBrowserWidth);
            int inspector = Math.Max(_minSidebar, EditorSettings.Current.InspectorWidth);
            bool both = _browserShown && _inspectorShown;
            int bar = both ? _sideSplit.SplitterWidth : 0;

            _layingOutSidebars = true;
            try
            {
                int available = Width - paletteWidth - minCanvas - _sideSplitter.Width - bar;
                int over = (_browserShown ? browser : 0) + (_inspectorShown ? inspector : 0) - available;
                if (over > 0 && Width > 0)
                {
                    if (_browserShown)
                    {
                        int cut = Math.Min(over, browser - _minSidebar);
                        browser -= cut;
                        over -= cut;
                    }

                    if (over > 0 && _inspectorShown)
                    {
                        inspector -= Math.Min(over, inspector - _minSidebar);
                    }
                }

                int width = (_browserShown ? browser : 0) + (_inspectorShown ? inspector : 0) + bar;
                _sidePanel.Visible = _sideSplitter.Visible = width > 0;
                if (width == 0)
                {
                    return;
                }

                // Minimums off while the panels are resized: a split container refuses a width or a
                // distance that breaks them, and the old values belong to the old size.
                _sideSplit.Panel1MinSize = 25;
                _sideSplit.Panel2MinSize = 25;
                _sideSplit.Panel1Collapsed = !_browserShown;
                _sideSplit.Panel2Collapsed = !_inspectorShown;
                _sidePanel.Width = width;
                _sidePanel.PerformLayout();

                if (both)
                {
                    int distance = _sideSplit.Width - inspector - _sideSplit.SplitterWidth;
                    if (distance >= 25 && distance <= _sideSplit.Width - 25 - _sideSplit.SplitterWidth)
                    {
                        _sideSplit.SplitterDistance = distance;
                    }

                    if (_sideSplit.Width >= (2 * _minSidebar) + _sideSplit.SplitterWidth)
                    {
                        _sideSplit.Panel1MinSize = _minSidebar;
                        _sideSplit.Panel2MinSize = _minSidebar;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // The split container was too small for the numbers; the next resize tries again.
            }
            finally
            {
                _layingOutSidebars = false;
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_built && !_sidebarsHeld)
            {
                LayoutSidebars();
            }
        }

        /// <summary>A splitter was dragged: the sidebar widths go into the profile.</summary>
        private void RememberSidebarWidths()
        {
            if (_layingOutSidebars || !_built || _sidebarsHeld || !Visible)
            {
                return;
            }

            // Only a real drag is remembered: widths below the minimum come from a layout in
            // progress (the window still opening), not from the user.
            EditorSettings settings = EditorSettings.Current.Clone();
            if (_browserShown && _inspectorShown)
            {
                if (_sideSplit.Panel1.Width < _minSidebar || _sideSplit.Panel2.Width < _minSidebar)
                {
                    return;
                }

                settings.TileBrowserWidth = _sideSplit.Panel1.Width;
                settings.InspectorWidth = _sideSplit.Panel2.Width;
            }
            else if (_sidePanel.Width < _minSidebar)
            {
                return;
            }
            else if (_browserShown)
            {
                settings.TileBrowserWidth = _sidePanel.Width;
            }
            else if (_inspectorShown)
            {
                settings.InspectorWidth = _sidePanel.Width;
            }

            _applyingOwnSettings = true;
            settings.Apply();
            _applyingOwnSettings = false;
        }

        private void OnSettingsChanged()
        {
            if (!_built || _applyingOwnSettings)
            {
                return;
            }

            _emptyHint.BackColor = EditorSettings.Current.EffectiveBackground;
            _emptyHint.ForeColor = EditorSettings.Current.BackgroundText;
            _canvas.Invalidate();
        }

        private void ShowSettings()
        {
            using var dialog = new EditorSettingsDialog(EditorSettings.Current);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                dialog.Result.Apply();
            }
        }

        private void ToggleSidebar(bool browser)
        {
            if (browser)
            {
                _browserShown = !_browserShown;
            }
            else
            {
                _inspectorShown = !_inspectorShown;
            }

            EditorSettings settings = EditorSettings.Current.Clone();
            settings.ShowTileBrowser = _browserShown;
            settings.ShowInspector = _inspectorShown;
            _applyingOwnSettings = true;
            settings.Apply();
            _applyingOwnSettings = false;
            LayoutSidebars();
        }

        private void TogglePlanView()
        {
            // The two views share nothing on screen, so each switch starts with the whole multi in view.
            Session.PlanMode = !Session.PlanMode;
            _canvas.FitToView();
        }

        /// <summary>Shows or hides the performance box; timings are only collected while it is shown.</summary>
        private void TogglePerformanceInfo()
        {
            _performanceShown = !_performanceShown;
            _canvas.CollectStats = _performanceShown;
            _performancePanel.Visible = _performanceShown;
            if (_performanceShown)
            {
                _performancePanel.UpdateNumbers();
                PlacePerformancePanel();
            }

            _commands.RefreshStates();
        }

        private void PlacePerformancePanel()
        {
            if (_performancePanel == null)
            {
                return;
            }

            int margin = LogicalToDeviceUnits(8);
            _performancePanel.Location = new Point(_centre.ClientSize.Width - _performancePanel.Width - margin, _canvas.Top + margin);
        }

        private void MoveReferencePrompt()
        {
            (int x, int y, int z) = Session.ReferenceOffset;
            int[] offset = Prompt.Numbers(this, "Move reference", ("Cells X", x, -200, 200), ("Cells Y", y, -200, 200), ("Z", z, -128, 127));
            if (offset != null)
            {
                Session.ReferenceOffset = (offset[0], offset[1], offset[2]);
            }
        }

        // ---------------------------------------------------------------- layout pieces

        /// <summary>Fills the designer's main strip: the menus, and the command buttons after each separator.</summary>
        private void BuildMainStrip()
        {
            ToolStrip strip = _mainStrip;

            Menu(_fileMenu, _new, _open, _import, _importFolder, null, _saveClient, _export, _savePrefab, _savePattern, null, _resize, _settingsCommand, null, _close, _closeAll);
            Menu(_editMenu, _undo, _redo, null, _cut, _copy, _paste, _delete, null,
                _rotateLeft, _rotateRight, _mirrorX, _mirrorY, _swapStyle, null, _zUp, _zDown, _hide, _showHidden);
            Menu(_selectMenu, _selectAll, _invert, _selectConnected, _selectSameId, null, _replaceId);
            ToolStripDropDownButton view = Menu(_viewMenu, _preview, _planView, null, _workPlane, _grid, _hideWalls, _hideAbove, _walkable, _doubleSurface, null,
                _allFloors, _levelUp, _levelDown, _planeUp, _planeDown, null, _showBrowser, _showInspector, null, _fit, _zoomIn, _zoomOut, _zoomReset, null, _moveReference, null, _performanceInfo);
            var terrainMenu = new ToolStripMenuItem("Preview ground");
            foreach (EditorCommand terrain in _terrainCommands)
            {
                ToolStripMenuItem item = _commands.MenuItem(terrain);
                item.Text = terrain.Text.Replace("Preview on: ", string.Empty);
                terrainMenu.DropDownItems.Add(item);
            }

            view.DropDownItems.Insert(1, terrainMenu);

            var reference = new ToolStripMenuItem("Reference underlay");
            reference.DropDownItems.Add(new ToolStripMenuItem("None"));
            view.DropDownItems.Insert(view.DropDownItems.Count - 1, reference);
            view.DropDownOpening += (_, _) => FillReferenceMenu(reference);

            InsertButtons(strip, _mainSeparator1, _new, _open, _saveClient);
            InsertButtons(strip, _mainSeparator2, _undo, _redo);
            InsertButtons(strip, _mainSeparator3, _cut, _copy, _paste, _delete);
            InsertButtons(strip, _mainSeparator4, _workPlane, _grid, _hideWalls);

            // Right side: zoom and help. Right-aligned items run from the right edge in item order.
            int at = strip.Items.IndexOf(_zoomLabel);
            foreach (EditorCommand command in new[] { _shortcuts, _fit, _zoomIn })
            {
                ToolStripButton button = _commands.Button(command);
                button.Alignment = ToolStripItemAlignment.Right;
                strip.Items.Insert(at++, button);
            }

            ToolStripButton zoomOut = _commands.Button(_zoomOut);
            zoomOut.Alignment = ToolStripItemAlignment.Right;
            strip.Items.Add(zoomOut);
        }

        /// <summary>Puts a button for each command into a strip, right after a designer item.</summary>
        private void InsertButtons(ToolStrip strip, ToolStripItem after, params EditorCommand[] commands)
        {
            int at = strip.Items.IndexOf(after) + 1;
            foreach (EditorCommand command in commands)
            {
                strip.Items.Insert(at++, _commands.Button(command));
            }
        }

        /// <summary>Fills one of the designer's menus with command items; null is a separator.</summary>
        private ToolStripDropDownButton Menu(ToolStripDropDownButton menu, params EditorCommand[] commands)
        {
            foreach (EditorCommand command in commands)
            {
                menu.DropDownItems.Add(command == null ? new ToolStripSeparator() : _commands.MenuItem(command));
            }

            return menu;
        }

        private void Menu_DropDownOpening(object sender, EventArgs e)
        {
            _commands.RefreshStates();
        }

        /// <summary>Fills the designer's options strip: the brush buttons and the roof option buttons.</summary>
        private void BuildOptionsStrip()
        {
            ToolStrip strip = _optionsStrip;
            _toolLabel.Font = new Font(Font, FontStyle.Bold);

            int at = strip.Items.IndexOf(_brushLabel) + 1;
            foreach (EditorCommand brush in _brushCommands)
            {
                // One press each instead of a dropdown; the pressed one is the size in use.
                ToolStripButton button = _commands.Button(brush);
                button.Text = brush.Text.Replace("Brush ", string.Empty);
                button.AutoSize = false;
                button.Width = 34;
                button.Margin = new Padding(1, 1, 0, 2);
                strip.Items.Insert(at++, button);
            }

            // Roof options, shown only with the Roof tool.
            RoofGroup(strip, _ridgeSeparator, _ridgeLabel,
                ("Auto", () => Session.Roof.Ridge == RoofRidge.Auto, () => Session.Roof.Ridge = RoofRidge.Auto),
                ("X", () => Session.Roof.Ridge == RoofRidge.AlongX, () => Session.Roof.Ridge = RoofRidge.AlongX),
                ("Y", () => Session.Roof.Ridge == RoofRidge.AlongY, () => Session.Roof.Ridge = RoofRidge.AlongY));
            RoofGroup(strip, _gablesSeparator, _gablesLabel,
                ("Front", () => Session.Roof.Gables == RoofGables.Front, () => Session.Roof.Gables = RoofGables.Front),
                ("Both", () => Session.Roof.Gables == RoofGables.Both, () => Session.Roof.Gables = RoofGables.Both),
                ("None", () => Session.Roof.Gables == RoofGables.None, () => Session.Roof.Gables = RoofGables.None));
            RoofGroup(strip, _gableFillSeparator, _gableFillLabel,
                ("Caps", () => Session.Roof.GableFill == RoofGableFill.Caps, () => Session.Roof.GableFill = RoofGableFill.Caps),
                ("Walls", () => Session.Roof.GableFill == RoofGableFill.Walls, () => Session.Roof.GableFill = RoofGableFill.Walls));
        }

        private void PlacementCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Apply(() => Session.Placement = (PlacementMode)Math.Max(0, _placementCombo.SelectedIndex));
        }

        private void ZStepCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Apply(() => Session.ZStep = int.Parse(_zStepCombo.Text));
        }

        private readonly List<ToolStripItem> _roofItems = new List<ToolStripItem>();
        private readonly List<(ToolStripButton Button, Func<bool> IsOn)> _roofButtons = new List<(ToolStripButton, Func<bool>)>();

        /// <summary>
        /// A labelled group of pick-one buttons for a roof option. The separator and the caption
        /// come from the designer; the buttons go after the caption and share its tooltip.
        /// </summary>
        private void RoofGroup(ToolStrip strip, ToolStripSeparator separator, ToolStripLabel caption, params (string Text, Func<bool> IsOn, Action Set)[] choices)
        {
            string tip = caption.ToolTipText;
            int at = strip.Items.IndexOf(caption) + 1;
            _roofItems.Add(separator);
            _roofItems.Add(caption);
            foreach ((string text, Func<bool> isOn, Action set) in choices)
            {
                var button = new ToolStripButton(text)
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    ToolTipText = tip,
                    AutoToolTip = false,
                    Margin = new Padding(1, 1, 0, 2)
                };
                button.Click += (_, _) =>
                {
                    if (Session == null)
                    {
                        return;
                    }

                    set();
                    Session.ToolOptionsChanged();
                    SyncRoofOptions();
                    _canvas.Focus();
                };
                strip.Items.Insert(at++, button);
                _roofItems.Add(button);
                _roofButtons.Add((button, isOn));
            }
        }

        private void SyncRoofOptions()
        {
            bool show = Session != null && !Session.Pasting && Session.Tool == ToolKind.Roof;
            foreach (ToolStripItem item in _roofItems)
            {
                item.Visible = show;
            }

            foreach ((ToolStripButton button, Func<bool> isOn) in _roofButtons)
            {
                button.Checked = show && isOn();
            }
        }

        /// <summary>Runs a change coming from an options control, unless the control is being filled from the session.</summary>
        private void Apply(Action change)
        {
            if (_syncing || Session == null)
            {
                return;
            }

            change();
            _canvas.Focus();
        }

        /// <summary>Fills the designer's tool palette: the tool buttons under each section label.</summary>
        private void BuildToolPalette()
        {
            ToolStrip strip = _toolPalette;

            void Section(ToolStripLabel title, params ToolKind[] kinds)
            {
                int at = strip.Items.IndexOf(title) + 1;
                foreach (ToolKind kind in kinds)
                {
                    ToolStripButton button = _commands.Button(_toolCommands[kind], showText: true);
                    button.ImageAlign = ContentAlignment.MiddleLeft;
                    button.TextAlign = ContentAlignment.MiddleLeft;
                    button.AutoSize = false;
                    button.Size = new Size(92, 26);
                    strip.Items.Insert(at++, button);
                }
            }

            Section(_editToolsLabel, ToolKind.Select, ToolKind.Lasso, ToolKind.Altitude, ToolKind.Pipette, ToolKind.Transparent, ToolKind.Remove);
            Section(_drawToolsLabel, ToolKind.Draw, ToolKind.RectFill, ToolKind.Line);
            Section(_houseToolsLabel, ToolKind.Wall, ToolKind.Floor, ToolKind.Door, ToolKind.Stair, ToolKind.Roof, ToolKind.Foundation);
        }

        // ---------------------------------------------------------------- documents

        private void NewDocumentPrompt()
        {
            using var dialog = new NewMultiDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            if (dialog.Foundation != null)
            {
                // A custom house starts as a copy of the client's foundation; it is not tied to that id.
                AddDocument(new OpenedMulti(dialog.Foundation, "Custom house", null));
                Session.LevelIndex = 2;
                return;
            }

            NewDocument(dialog.MultiWidth, dialog.MultiHeight, "Untitled", dialog.FirstFloorZ);
        }

        private void NewDocument(int width, int height, string title, int firstFloorZ = 0)
        {
            EditorDocument document = CreateDocument(title, null);
            document.Session.NewBlank(width, height, firstFloorZ);
            ShowDocument(document);
        }

        private void OpenFromClient()
        {
            AddDocuments(DocumentIO.OpenFromClient(this));
        }

        private void ImportFile()
        {
            AddImported(DocumentIO.ImportFromFile(this));
        }

        private void ImportFolder()
        {
            AddImported(DocumentIO.ImportFromFolder(this));
        }

        /// <summary>Opens what an import read, then lists what it could not.</summary>
        private void AddImported(ImportResult result)
        {
            if (result == null)
            {
                return;
            }

            int opened = AddDocuments(result.Opened);
            MultiFileImport.ShowSummary(this, opened, result.Failures);
        }

        /// <summary>Opens each multi in its own tab, after asking when there are a lot of them. Returns how many opened.</summary>
        private int AddDocuments(List<OpenedMulti> opened)
        {
            if (opened == null || opened.Count == 0 || !ConfirmOpenMany(opened.Count))
            {
                return 0;
            }

            foreach (OpenedMulti multi in opened)
            {
                AddDocument(multi);
            }

            return opened.Count;
        }

        /// <summary>Asks before a selection opens more tabs than anyone picks by accident.</summary>
        private bool ConfirmOpenMany(int count)
        {
            const int withoutAsking = 12;
            return count <= withoutAsking || MessageBox.Show(this, $"Open {count} multis, each in its own tab?", "Multi Editor",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK;
        }

        private void AddDocument(OpenedMulti opened)
        {
            EditorDocument document = CreateDocument(opened.Title, opened.ClientId);
            document.Session.Load(opened.Components);
            ShowDocument(document);
        }

        private EditorDocument CreateDocument(string title, int? clientId)
        {
            var session = new EditorSession(_masks, _houseStyles) { Title = title };
            EditorSettings settings = EditorSettings.Current;
            session.ShowWorkPlane = settings.StartWithWorkPlane;
            session.ShowGrid = settings.StartWithGrid;
            session.HideAboveLevel = settings.HideFloorsAbove;
            session.PreviewTerrain = (PreviewTerrain)Math.Clamp(settings.PreviewTerrain, 0, 4);
            if (_active != null)
            {
                session.FloorPattern = _active.Session.FloorPattern;
                session.Roof.Ridge = _active.Session.Roof.Ridge;
                session.Roof.Gables = _active.Session.Roof.Gables;
                session.Roof.GableFill = _active.Session.Roof.GableFill;

                // A new tab starts with the tool setup you were working with.
                session.SetDrawTile(_active.Session.DrawTileId);
                session.Placement = _active.Session.Placement;
                session.BrushSize = _active.Session.BrushSize;
                session.ZStep = _active.Session.ZStep;
                session.HouseStyle = _active.Session.HouseStyle;
            }

            return new EditorDocument { Session = session, ClientId = clientId };
        }

        private void ShowDocument(EditorDocument document)
        {
            _documents.Add(document);
            _docTabs.TabPages.Add(new TabPage(document.Session.Title));
            if (_docTabs.SelectedIndex == _documents.Count - 1)
            {
                Activate(_documents.Count - 1);
            }
            else
            {
                _docTabs.SelectedIndex = _documents.Count - 1;
            }

            _canvas.Focus();
        }

        private void SaveToClient()
        {
            int? id = DocumentIO.SaveToClient(this, Session.CompList, _active.ClientId);
            if (id == null)
            {
                return;
            }

            _active.ClientId = id;
            Session.Title = $"Multi 0x{id:X4}";
            Session.MarkSaved();
        }

        private void ExportFile()
        {
            if (DocumentIO.ExportToFile(this, Session.CompList, Session.Title, Session.FirstFloorZ))
            {
                Session.MarkSaved();
            }
        }

        private void ResizePrompt()
        {
            int[] size = Prompt.Numbers(this, "Resize multi",
                ("Width (X)", Session.CompList.Width, 1, 200), ("Height (Y)", Session.CompList.Height, 1, 200));
            if (size != null)
            {
                Session.Resize(size[0], size[1]);
            }
        }

        /// <summary>Closes every tab, asking once when any of them has unsaved changes.</summary>
        private void CloseAllDocuments()
        {
            int modified = _documents.Count(d => d.Session.Modified);
            if (modified > 0 && MessageBox.Show(this,
                    (modified == 1 ? "1 design has" : $"{modified} designs have") +
                    " changes that were not saved to the client or exported.\r\nClose all designs anyway?",
                    "Multi Editor", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            // The others first, the active one last, so no other tab is opened on the way.
            foreach (EditorDocument document in _documents.Where(d => d != _active).Reverse().ToList())
            {
                CloseDocument(document, ask: false);
            }

            CloseDocument(_active, ask: false);
        }

        private void CloseDocument(EditorDocument document, bool ask = true)
        {
            if (document == null)
            {
                return;
            }

            if (ask && document.Session.Modified && MessageBox.Show(this,
                    $"\"{document.Session.Title}\" has changes that were not saved to the client or exported.\r\nClose it anyway?",
                    "Multi Editor", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            int index = _documents.IndexOf(document);
            if (_active == document)
            {
                Detach();
            }

            _documents.RemoveAt(index);
            _removingTab = true;
            try
            {
                _docTabs.TabPages.RemoveAt(index);
            }
            finally
            {
                _removingTab = false;
            }

            Activate(_docTabs.SelectedIndex);
        }

        private void DocTabs_MouseUp(object sender, MouseEventArgs e)
        {
            // Middle click closes a tab, like a browser.
            if (e.Button != MouseButtons.Middle)
            {
                return;
            }

            int index = TabAt(e.Location);
            if (index >= 0)
            {
                CloseDocument(_documents[index]);
            }
        }

        // ---------------------------------------------------------------- tab close buttons

        /// <summary>Tab whose close button is under the mouse, or -1.</summary>
        private int _hotCloseTab = -1;

        /// <summary>Set while a tab is removed, so the selection that follows is never cancelled.</summary>
        private bool _removingTab;

        private int CloseGlyphSize => LogicalToDeviceUnits(14);

        private int TabAt(Point location)
        {
            for (int i = 0; i < _docTabs.TabCount; i++)
            {
                if (_docTabs.GetTabRect(i).Contains(location))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The close button of a tab: a square at the right end of its header.</summary>
        private Rectangle CloseRect(Rectangle tab)
        {
            int size = CloseGlyphSize;
            return new Rectangle(tab.Right - size - LogicalToDeviceUnits(4), tab.Top + ((tab.Height - size) / 2), size, size);
        }

        private int CloseButtonAt(Point location)
        {
            int index = TabAt(location);
            return index >= 0 && CloseRect(_docTabs.GetTabRect(index)).Contains(location) ? index : -1;
        }

        private void DocTabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _docTabs.TabCount)
            {
                return;
            }

            bool selected = e.Index == _docTabs.SelectedIndex;
            Rectangle bounds = e.Bounds;
            using (var back = new SolidBrush(_docTabs.TabBackColor(selected)))
            {
                e.Graphics.FillRectangle(back, bounds);
            }

            Rectangle close = CloseRect(bounds);
            int inset = LogicalToDeviceUnits(6);
            var textBounds = new Rectangle(bounds.Left + inset, bounds.Top, close.Left - bounds.Left - inset, bounds.Height);
            TextRenderer.DrawText(e.Graphics, _docTabs.TabPages[e.Index].Text, _docTabs.Font, textBounds,
                _docTabs.TabTextColor(selected),
                // NoPadding: the tab's width was measured from the bare text.
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

            bool hot = e.Index == _hotCloseTab;
            if (hot)
            {
                using var hotBack = new SolidBrush(Color.FromArgb(232, 17, 35));
                e.Graphics.FillRectangle(hotBack, close);
            }

            // The cross, a little inside the button.
            int pad = LogicalToDeviceUnits(4);
            Color crossColor = hot ? Color.White : _docTabs.GlyphColor(selected);
            using var pen = new Pen(crossColor, Math.Max(1f, LogicalToDeviceUnits(1) * 1.4f));
            System.Drawing.Drawing2D.SmoothingMode previous = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(pen, close.Left + pad, close.Top + pad, close.Right - pad - 1, close.Bottom - pad - 1);
            e.Graphics.DrawLine(pen, close.Right - pad - 1, close.Top + pad, close.Left + pad, close.Bottom - pad - 1);
            e.Graphics.SmoothingMode = previous;
        }

        private void DocTabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // A press on another tab's close button closes it without switching to it first.
            if (!_removingTab && (MouseButtons & MouseButtons.Left) != 0 && CloseButtonAt(_docTabs.PointToClient(MousePosition)) >= 0)
            {
                e.Cancel = true;
            }
        }

        private void DocTabs_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int index = CloseButtonAt(e.Location);
            if (index >= 0)
            {
                SetHotCloseTab(-1);
                CloseDocument(_documents[index]);
            }
        }

        private void DocTabs_MouseMove(object sender, MouseEventArgs e)
        {
            SetHotCloseTab(CloseButtonAt(e.Location));
        }

        private void DocTabs_MouseLeave(object sender, EventArgs e)
        {
            SetHotCloseTab(-1);
        }

        private void SetHotCloseTab(int index)
        {
            if (index == _hotCloseTab)
            {
                return;
            }

            InvalidateTab(_hotCloseTab);
            _hotCloseTab = index;
            InvalidateTab(_hotCloseTab);
        }

        private void InvalidateTab(int index)
        {
            if (index >= 0 && index < _docTabs.TabCount)
            {
                _docTabs.Invalidate(_docTabs.GetTabRect(index));
            }
        }

        private void Detach()
        {
            if (_active == null)
            {
                return;
            }

            EditorSession session = _active.Session;
            session.CancelTool();
            session.DocumentChanged -= OnDocumentChanged;
            session.SelectionChanged -= OnSelectionChanged;
            session.HoverChanged -= UpdateStatus;
            session.CursorCellChanged -= UpdateStatus;
            session.ToolChanged -= OnToolChanged;
            session.DrawTileChanged -= OnDrawTileChanged;
            session.ViewChanged -= OnViewChanged;
            session.TileUsed -= _browser.RecordRecent;
            _active = null;
        }

        private void Activate(int index)
        {
            EditorDocument document = index >= 0 && index < _documents.Count ? _documents[index] : null;
            if (document == _active && document != null)
            {
                return;
            }

            Detach();
            _active = document;
            _emptyHint.Visible = document == null;
            _docTabs.Visible = _documents.Count > 0;

            if (document == null)
            {
                _canvas.Show(null, null);
                _inspector.Bind(null);
            }
            else
            {
                EditorSession session = document.Session;
                session.DocumentChanged += OnDocumentChanged;
                session.SelectionChanged += OnSelectionChanged;
                session.HoverChanged += UpdateStatus;
                session.CursorCellChanged += UpdateStatus;
                session.ToolChanged += OnToolChanged;
                session.DrawTileChanged += OnDrawTileChanged;
                session.ViewChanged += OnViewChanged;
                session.TileUsed += _browser.RecordRecent;

                _canvas.Show(session, document.Camera);
                if (!document.Fitted)
                {
                    document.Fitted = true;
                    _canvas.FitToView();
                }

                _inspector.Bind(session);
                if (session.DrawTileId != 0)
                {
                    _browser.SetCurrentTile(session.DrawTileId);
                }
            }

            SyncOptions();
            UpdateTitle();
            UpdateStatus();
            UpdateZoomLabel();
            _commands.RefreshStates();
        }

        // ---------------------------------------------------------------- keeping the UI in line

        private void OnDocumentChanged()
        {
            SyncLevels();
            UpdateTitle();
            UpdateStatus();
            _commands.RefreshStates();
        }

        private void OnSelectionChanged()
        {
            UpdateStatus();
            _commands.RefreshStates();
        }

        private void OnToolChanged()
        {
            SyncOptions();
            UpdateStatus();
            _commands.RefreshStates();
        }

        private void OnDrawTileChanged()
        {
            _browser.SetCurrentTile(Session.DrawTileId);
            SyncOptions();
        }

        private void OnViewChanged()
        {
            SyncLevels();
            _commands.RefreshStates();
            UpdateStatus();
        }

        /// <summary>
        /// One button per floor in the options bar, the active one pressed. Every floor has a
        /// button whether it holds tiles or not.
        /// </summary>
        private void SyncLevels()
        {
            var names = new List<(string Short, string Tip)> { ("All", "All floors (Ctrl+Home)") };
            if (Session != null)
            {
                names.AddRange(Session.Levels.Select(l => (l.ShortName, l.Description)));
            }

            if (_floorButtons.Count != names.Count || !_floorButtons.Select(b => b.Text).SequenceEqual(names.Select(n => n.Short)))
            {
                foreach (ToolStripButton old in _floorButtons)
                {
                    _optionsStrip.Items.Remove(old);
                    old.Dispose();
                }

                _floorButtons.Clear();
                int at = _optionsStrip.Items.IndexOf(_floorLabel) + 1;
                for (int i = 0; i < names.Count; i++)
                {
                    int index = i;
                    var button = new ToolStripButton(names[i].Short)
                    {
                        DisplayStyle = ToolStripItemDisplayStyle.Text,
                        ToolTipText = names[i].Tip,
                        AutoToolTip = false,
                        AutoSize = false,
                        Width = names[i].Short.Length > 2 ? 40 : 26,
                        Margin = new Padding(1, 1, 0, 2)
                    };
                    button.Click += (_, _) =>
                    {
                        if (Session != null)
                        {
                            Session.LevelIndex = index;
                            _canvas.Focus();
                        }
                    };
                    _floorButtons.Add(button);
                    _optionsStrip.Items.Insert(at + i, button);
                }
            }

            int active = Session?.LevelIndex ?? 0;
            for (int i = 0; i < _floorButtons.Count; i++)
            {
                _floorButtons[i].Checked = i == active;
                _floorButtons[i].Enabled = Session != null;
                _floorButtons[i].ToolTipText = i < names.Count ? names[i].Tip : string.Empty;
            }
        }

        private void SyncOptions()
        {
            SyncLevels();
            SyncRoofOptions();
            _syncing = true;
            try
            {
                EditorSession session = Session;
                bool has = session != null;
                _placementCombo.Enabled = _zStepCombo.Enabled = has;
                if (!has)
                {
                    _toolLabel.Text = string.Empty;
                    _drawTileLabel.Text = string.Empty;
                    return;
                }

                _toolLabel.Text = session.Pasting ? "Paste" : _toolCommands[session.Tool].Text;
                _placementCombo.SelectedIndex = (int)session.Placement;
                int zIndex = _zStepCombo.IndexOf(session.ZStep.ToString());
                _zStepCombo.SelectedIndex = Math.Max(0, zIndex);

                if (session.Tool == ToolKind.FloorPattern)
                {
                    _drawTileLabel.Text = session.FloorPattern != null ? $"Pattern: {session.FloorPattern.Name}" : "Pattern: pick one in the House page";
                }
                else if (IsHouseTool(session.Tool) && session.Tool != ToolKind.Foundation)
                {
                    _drawTileLabel.Text = session.HouseStyle != null ? $"Style: {session.HouseStyle.Name}" : "Style: pick one in the House page";
                }
                else if (session.Tool == ToolKind.Foundation)
                {
                    _drawTileLabel.Text = "Foundation";
                }
                else
                {
                    ushort id = session.DrawTileId;
                    _drawTileLabel.Text = id == 0 ? "Tile: pick one in the tile browser" : $"Tile: 0x{id:X4} {TileName(id)}";
                }
            }
            finally
            {
                _syncing = false;
            }
        }

        private static string TileName(ushort id)
        {
            return id < TileData.ItemTable.Length ? TileData.ItemTable[id].Name : string.Empty;
        }

        private void UpdateTitle()
        {
            for (int i = 0; i < _documents.Count; i++)
            {
                EditorSession session = _documents[i].Session;
                string text = session.Title + (session.Modified ? " *" : string.Empty);
                if (_docTabs.TabPages[i].Text != text)
                {
                    _docTabs.TabPages[i].Text = text;
                }
            }
        }

        private void UpdateZoomLabel()
        {
            if (_zoomLabel != null)
            {
                _zoomLabel.Text = $"{_canvas.Camera.Zoom * 100:0}%";
            }
        }

        private void UpdateStatus()
        {
            EditorSession session = Session;
            if (session == null)
            {
                _statusHint.Text = string.Empty;
                _statusCell.Text = _statusPlane.Text = _statusHover.Text = _statusSelection.Text = _statusSize.Text = string.Empty;
                return;
            }

            _statusHint.Text = session.PreviewMode ? "Preview: editing is off. Right click picks the ground. F5 returns to the editor."
                : session.Pasting ? "Click to place the tiles. Esc or right click cancels."
                : ToolHint(session.Tool);

            (int x, int y, int z) = session.CursorCell;
            _statusCell.Text = session.MouseInside && session.CompList.InBounds(x, y) ? $"Cell {x}, {y}, Z {z}" : string.Empty;
            int offset = session.WorkPlaneOffset;
            _statusPlane.Text = $"Work plane Z {session.WorkPlaneZ}" + (offset != 0 ? $" (floor {offset:+0;-0})" : string.Empty) +
                                (session.ShowWorkPlane ? string.Empty : " (hidden)");

            MultiTile hover = session.HoverTile;
            _statusHover.Text = hover != null ? $"0x{hover.Id:X4} {TileName(hover.Id)}  Z {hover.Z}  H {hover.Height}" : string.Empty;

            int count = session.Selection.Count;
            _statusSelection.Text = count == 0 ? string.Empty : $"{count} selected";
            _statusSize.Text = $"{session.CompList.Width} x {session.CompList.Height}, {session.CompList.TileCount} tiles";
        }

        private static string ToolHint(ToolKind kind)
        {
            return kind switch
            {
                ToolKind.Select => "Click a tile to select, Ctrl+click adds, drag a box over tiles, drag the selection to move it.",
                ToolKind.Lasso => "Draw an outline around tiles to select them. Ctrl adds to the selection.",
                ToolKind.Draw => "Click or drag to place the chosen tile. Placement mode (M) decides the Z.",
                ToolKind.Remove => "Click a tile to remove it, drag to remove an area.",
                ToolKind.Altitude => "Click raises the tile by the Z step, Shift+click lowers it.",
                ToolKind.Pipette => "Click a tile to draw with it.",
                ToolKind.Transparent => "Click a tile to see through it, click again to restore.",
                ToolKind.RectFill => "Drag a rectangle to fill with the chosen tile.",
                ToolKind.Line => "Drag a line of the chosen tile.",
                ToolKind.Wall => "Drag along the cells to build walls in the chosen style. Corners and posts are picked for you.",
                ToolKind.Floor => "Drag an area to lay the chosen floor style.",
                ToolKind.Door => "Click a wall cell to hang the chosen door.",
                ToolKind.Stair => "Drag in the direction the stairs climb.",
                ToolKind.Roof => "Click a walled room to roof it, or drag over the wall cells to size the roof by hand. Options: Ridge, Gables, Fill.",
                ToolKind.Foundation => "Drag an area to lay a foundation under it.",
                ToolKind.FloorPattern => "Drag an area to lay the bordered floor: corners, sides and fill repeat to fit.",
                _ => string.Empty
            };
        }

        // ---------------------------------------------------------------- keyboard

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_built || (keyData != Keys.Escape && IsTextInputFocused()))
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (_commands.TryRun(keyData))
            {
                _commands.RefreshStates();
                return true;
            }

            EditorSession session = Session;
            if (session == null)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            // Arrows move the selection one cell; without a selection, or with Ctrl, they pan.
            (int dx, int dy) = (keyData & Keys.KeyCode) switch
            {
                Keys.Left => (-1, 0),
                Keys.Right => (1, 0),
                Keys.Up => (0, -1),
                Keys.Down => (0, 1),
                _ => (0, 0)
            };

            Keys modifiers = keyData & Keys.Modifiers;
            if ((dx, dy) == (0, 0) || (modifiers != Keys.None && modifiers != Keys.Control))
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (modifiers == Keys.None && session.Selection.Count > 0)
            {
                session.NudgeSelection(dx, dy, 0);
            }
            else
            {
                _canvas.Camera.Pan(-dx * 44, -dy * 44);
                _canvas.Invalidate();
            }

            return true;
        }

        /// <summary>True while typing in a text, number or combo box, where letters must not switch tools.</summary>
        private bool IsTextInputFocused()
        {
            Control focused = this;
            while (focused is ContainerControl container && container.ActiveControl != null)
            {
                focused = container.ActiveControl;
            }

            return focused is TextBoxBase || focused is NumericUpDown || focused is ComboBox ||
                   focused.Parent is NumericUpDown || focused is ToolStrip;
        }
    }
}
