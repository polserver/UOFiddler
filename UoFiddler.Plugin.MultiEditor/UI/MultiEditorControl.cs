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
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Rendering;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

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

            /// <summary>The design file the document was opened from or last saved to, or null.</summary>
            public string FilePath;
        }

        private const string _messageTitle = "Multi Editor";

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
        private readonly List<(EditorCommand Command, string Text, string Tip)> _placementCommands = new List<(EditorCommand, string, string)>();
        private readonly List<ToolStripButton> _floorButtons = new List<ToolStripButton>();

        public MultiEditorControl()
        {
            InitializeComponent();
            SplitterGrip.Attach(_sideSplit);

            // The right sidebar has two pages: the design (floors, layers, selection) and the
            // house draft, which is too tall to share a page. The House page scrolls when the
            // window is short, and comes to the front with the House tool or an open draft.
            _housePanel = new HousePanel { Dock = DockStyle.Top };
            _housePanel.PickStyle += PickHouseMaterial;
            _housePanel.SaveTheme += SaveHouseTheme;
            _housePanel.DeleteTheme += DeleteHouseTheme;
            _housePanel.Message += Tell;
            _housePanel.HouseToolWanted += () => SetTool(ToolKind.House);
            _housePanel.SaveDraft += SaveHouseDraft;
            _housePanel.OpenDraft += OpenHouseDraft;
            _housePanel.SaveBlueprint += SaveHouseBlueprint;
            _housePanel.EditHouse += EditPlacedHouse;
            _housePanel.Used += () =>
            {
                _commands.RefreshStates();
                _canvas.Focus();
            };
            _designPage = new TabPage("Design");
            _housePage = new TabPage("House draft") { AutoScroll = true };
            _sideSplit.Panel2.Controls.Remove(_inspector);
            _designPage.Controls.Add(_inspector);
            _housePage.Controls.Add(_housePanel);
            _sideTabs = new UoFiddler.Controls.UserControls.ThemedTabControl { Dock = DockStyle.Fill };
            _sideTabs.TabPages.Add(_designPage);
            _sideTabs.TabPages.Add(_housePage);
            _sideSplit.Panel2.Controls.Add(_sideTabs);

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

            Session.BeginPaste(tiles, "Map extract");
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
                ShowMessage($"The client's house tables could not be read:\r\n{ex.Message}");
                return;
            }

            SuspendLayout();
            DefineCommands();
            BuildCanvasMenu();

            _browser.SetHouseStyles(_houseStyles);
            _styleErrors = _houseStyles.Errors.Count == 0 ? null : string.Join("\r\n", _houseStyles.Errors.Take(5));
            WireBrowser();

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

        /// <summary>What the tile browser asks of the editor.</summary>
        private void WireBrowser()
        {
            _browser.EditStylesRequested += EditHouseStyles;
            _browser.TileChosen += id =>
            {
                Session?.SetDrawTile(id);
                if (Session != null && !EditorSession.IsTileTool(Session.Tool))
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

                Session.BeginPaste(tiles, "Design");
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
            _browser.BlueprintChosen += tiles =>
            {
                Session?.BeginPaste(tiles, "Blueprint");
                _canvas.Focus();
            };
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
                MessageBox.Show(this, "The design could not be read: " + ex.Message, _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }
    }
}
