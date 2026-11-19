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
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Storage;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
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

        private EditorCommand _openDesign, _saveDesign, _saveDesignAs;
        private EditorCommand _new, _open, _import, _importFolder, _saveClient, _export, _saveBlueprint, _resize, _close, _closeAll;
        private EditorCommand _undo, _redo, _cut, _copy, _paste, _duplicate, _repeat, _delete;
        private EditorCommand _selectAll, _invert, _selectConnected, _selectSameId, _replaceId;
        private EditorCommand _rotateLeft, _rotateRight, _mirrorX, _mirrorY, _swapStyle, _hide, _showHidden;
        private EditorCommand _grid, _hideWalls, _walkable, _doubleSurface, _hideAbove, _levelUp, _levelDown;
        private EditorCommand _fit, _center, _zoomIn, _zoomOut, _zoomReset, _shortcuts;
        private EditorCommand _zUp, _zDown, _planeUp, _planeDown, _cyclePlacement, _escape;
        private EditorCommand _drawWithSelected, _workPlane;
        private MultiTile _menuTile;
        private ContextMenuStrip _canvasMenu;
        private ToolStripMenuItem _canvasMenuTitle;
        private EditorCommand _preview, _moveReference, _planView, _allFloors, _showBrowser, _showInspector;

        private EditorCommand _settingsCommand;

        private EditorCommand _savePattern;
        private EditorCommand _newLayer, _moveToLayer, _selectLayer;

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
            _openDesign = Command("File", "Open design file...", OpenDesignFile, Keys.Control | Keys.Shift | Keys.O);
            _saveDesign = Command("File", "Save design file", () => SaveDesignFile(saveAs: false), Keys.Control | Keys.Shift | Keys.S, hasDoc);
            _saveDesignAs = Command("File", "Save design file as...", () => SaveDesignFile(saveAs: true), can: hasDoc);
            _saveClient = Command("File", "Save to client...", SaveToClient, Keys.Control | Keys.S, hasDoc, glyph: Glyphs.Save);
            _export = Command("File", "Export file...", ExportFile, Keys.Control | Keys.E, hasDoc);
            _saveBlueprint = Command("File", "Save as blueprint...", SaveBlueprint, can: hasDoc);
            _savePattern = Command("File", "Save selection as floor pattern...", SaveFloorPattern, can: hasSelection);
            _resize = Command("File", "Resize multi...", ResizePrompt, can: hasDoc);
            _settingsCommand = Command("File", "Settings...", ShowSettings);
            _close = Command("File", "Close design", () => CloseDocument(_active), Keys.Control | Keys.W, hasDoc);
            _closeAll = Command("File", "Close all designs", CloseAllDocuments, Keys.Control | Keys.Shift | Keys.W, hasDoc);

            _undo = Command("Edit", "Undo", () => Session.Undo(), Keys.Control | Keys.Z,
                () => Session != null && Session.CanUndo, glyph: Glyphs.Undo);
            _redo = Command("Edit", "Redo", () => Session.Redo(), Keys.Control | Keys.Y,
                () => Session != null && Session.CanRedo, glyph: Glyphs.Redo, alt: Keys.Control | Keys.Shift | Keys.Z);
            _cut = Command("Edit", "Cut", () => Session.Cut(), Keys.Control | Keys.X, hasSelection, glyph: Glyphs.Cut);
            // While a draft is being shaped, copy, paste and duplicate work on its selected wing.
            Func<bool> hasWing = () => Session != null && Session.ShapingRoof && Session.WingTool.HasSelectedWing;
            _copy = Command("Edit", "Copy", CopySelectionOrWing, Keys.Control | Keys.C, () => hasWing() || hasSelection(), glyph: Glyphs.Copy);
            _paste = Command("Edit", "Paste", PasteTilesOrWing, Keys.Control | Keys.V, hasDoc, glyph: Glyphs.Paste);
            _duplicate = Command("Edit", "Duplicate selection", DuplicateSelectionOrWing, Keys.Control | Keys.D, () => hasWing() || hasSelection());
            _repeat = Command("Edit", "Repeat selection...", RepeatSelectionPrompt, Keys.Control | Keys.Shift | Keys.D, hasSelection);
            _delete = Command("Edit", "Delete selection", DeleteSelectionOrWing, Keys.Delete,
                () => Session != null && (Session.ShapingRoof ? Session.WingTool.HasSelectedWing : Session.Selection.Count > 0), glyph: Glyphs.Delete);

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

            _newLayer = Command("Layers", "New layer", () => Session.AddLayer(), Keys.Control | Keys.Shift | Keys.N, hasDoc);
            _moveToLayer = Command("Layers", "Move selection to the active layer", () => Session.MoveSelectionToLayer(Session.ActiveLayer),
                Keys.Control | Keys.Shift | Keys.M, hasSelection);
            _selectLayer = Command("Layers", "Select tiles on the active layer", () => Session.SelectLayer(Session.ActiveLayer), can: hasDoc);

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
            _center = Command("View", "Center design", () => _canvas.CenterView(), Keys.Shift | Keys.Home, hasDoc, glyph: Glyphs.Center);
            _zoomIn = Command("View", "Zoom in", () => _canvas.ZoomStep(1), Keys.Add, glyph: Glyphs.ZoomIn, alt: Keys.Shift | Keys.Oemplus);
            _zoomOut = Command("View", "Zoom out", () => _canvas.ZoomStep(-1), Keys.Subtract, glyph: Glyphs.ZoomOut, alt: Keys.OemMinus);
            _zoomReset = Command("View", "Zoom 100%", () => _canvas.SetZoom(1f), Keys.Control | Keys.D0, glyph: Glyphs.ActualSize, alt: Keys.Control | Keys.NumPad0);
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
            AddTool(ToolKind.House, "House", Keys.D7, Glyphs.Foundation);
            AddTool(ToolKind.FloorPattern, "Floor pattern", Keys.None, Glyphs.Floor);

            _cyclePlacement = Command("Tools", "Next placement mode", CyclePlacement, Keys.M, hasDoc);
            foreach ((PlacementMode mode, string text, string tip) in new[]
                     {
                         (PlacementMode.Stack, "On top", "On top of the tile under the cursor"),
                         (PlacementMode.Replace, "Replace", "Replacing the tile under the cursor, at its Z"),
                         (PlacementMode.FixedZ, "At plane Z", "At the work plane's Z, in the cell under the cursor")
                     })
            {
                EditorCommand command = Command("Tools", $"Place: {text}", () =>
                {
                    Session.Placement = mode;
                    SyncOptions();
                }, can: hasDoc, isChecked: () => Session?.Placement == mode);
                _placementCommands.Add((command, text, tip));
            }

            foreach ((int size, Keys key) in new[] { (1, Keys.D1), (3, Keys.D2), (5, Keys.D3) })
            {
                _brushCommands.Add(Command("Tools", $"Brush {size}x{size}", () => Session.BrushSize = size, Keys.Control | key,
                    hasDoc, () => Session?.BrushSize == size));
            }
            _escape = Command("Tools", "Cancel tool, then clear selection", Escape, Keys.Escape, hasDoc);
            _placeRoof = Command("Tools", "Place the roof or house being shaped", () => Session.DraftInFocus?.Commit(), Keys.Enter,
                () => Session?.DraftInFocus != null);
            _shortcuts = Command("Help", "Keyboard shortcuts", ShowShortcuts, Keys.F1, glyph: Glyphs.Help);
            _drawWithSelected = Command("Tools", "Draw with this tile", DrawWithSelected, can: () => (_menuTile ?? Session?.SelectedTile) != null);
        }

        private void AddTool(ToolKind kind, string text, Keys key, string glyph)
        {
            _toolCommands[kind] = Command("Tools", text, () => SetTool(kind), key, () => Session != null,
                () => Session != null && !Session.Pasting && Session.Tool == kind, glyph);
        }

        private void SetTool(ToolKind kind, bool fromHousePage = false)
        {
            if (Session == null)
            {
                return;
            }

            Session.Tool = kind;

            if (IsHouseTool(kind) || kind == ToolKind.House)
            {
                TellStyleErrors();
            }

            // A tool chosen in place of the style a pick was waiting for: the pick is off.
            if (!fromHousePage && !_openingHousePage)
            {
                CancelHousePick();
            }

            // The house draft is worked on in its panel, which sits in the floors and selection sidebar.
            if (kind == ToolKind.House && !_inspectorShown)
            {
                ToggleSidebar(browser: false);
            }

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

        private void CyclePlacement()
        {
            Session.Placement = (PlacementMode)(((int)Session.Placement + 1) % 3);
            SyncOptions();
        }

        private void CopySelectionOrWing()
        {
            if (Session.ShapingRoof && Session.WingTool.HasSelectedWing)
            {
                Session.WingTool.CopyWing();
                _commands.RefreshStates();
            }
            else
            {
                Session.Copy();
            }
        }

        private void PasteTilesOrWing()
        {
            if (Session.ShapingRoof && Session.WingTool.CanPasteWing)
            {
                Session.WingTool.PasteWing();
            }
            else
            {
                Session.Paste();
            }
        }

        private void DuplicateSelectionOrWing()
        {
            if (Session.ShapingRoof && Session.WingTool.HasSelectedWing)
            {
                Session.WingTool.DuplicateWing();
            }
            else
            {
                Session.Duplicate();
            }
        }

        private void DeleteSelectionOrWing()
        {
            if (Session.ShapingRoof)
            {
                Session.WingTool.DeleteWing();
            }
            else
            {
                Session.DeleteSelection();
            }
        }

        private void Escape()
        {
            // A style pick the house panel is waiting for goes first.
            if (_housePick != null)
            {
                CancelHousePick();
                return;
            }

            // With a roof being shaped: the first Esc stops a drag, the next one drops the roof.
            if (Session.DraftInFocus is { } draft && !Session.ActiveTool.IsDragging)
            {
                draft.Discard();
                return;
            }

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
            Tell($"Replaced {count} tile(s)" + (selectionOnly ? " in the selection" : string.Empty));
        }

        /// <summary>Saves the selection as a blueprint, or the whole design when nothing is selected.</summary>
        private void SaveBlueprint()
        {
            bool whole = Session.Selection.Count == 0;
            List<ClipboardTile> tiles = whole ? Session.DesignAsClipboard() : Session.SelectionAsClipboard();
            if (tiles == null)
            {
                return;
            }

            if (tiles.Count > Blueprints.LargeBlueprintTiles &&
                MessageBox.Show(this, $"This blueprint has {tiles.Count:N0} tiles. Large blueprints are slow to place and to preview.\r\nSave it anyway?",
                    "Save blueprint", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            string name = Prompt.Text(this, whole ? "Save design as blueprint" : "Save blueprint", "Blueprint name:", whole ? Session.Title : string.Empty);
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            _browser.SaveBlueprint(name.Trim(), tiles);
        }

        private void DrawWithSelected()
        {
            Session.SetDrawTile((_menuTile ?? Session.SelectedTile).Id);
            Session.Tool = ToolKind.Draw;
        }

        /// <summary>
        /// Asks how often and how far apart to copy the selection, showing the copies on the canvas
        /// while the numbers change.
        /// </summary>
        private void RepeatSelectionPrompt()
        {
            EditorSession session = Session;
            int width = session.Selection.Max(t => t.X) - session.Selection.Min(t => t.X) + 1;
            using var dialog = new RepeatSelectionDialog(width);

            void Preview()
            {
                (int added, int skipped) = session.PreviewRepeat(dialog.Copies, dialog.StepX, dialog.StepY, dialog.StepZ);
                dialog.SetSummary(added == 0 && skipped == 0 ? "Set a step to place the copies."
                    : $"Adds {added:N0} tiles" + (skipped > 0 ? $" ({skipped:N0} outside the multi are skipped)" : string.Empty) +
                      (added > RepeatPreviewTool.MaxGhosts ? ". Too many to preview." : "."));
            }

            dialog.ValuesChanged += Preview;
            Preview();
            try
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    session.RepeatSelection(dialog.Copies, dialog.StepX, dialog.StepY, dialog.StepZ);
                }
            }
            finally
            {
                session.EndRepeatPreview();
                _canvas.Focus();
            }
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
            foreach (EditorCommand command in new[] { _cut, _copy, _paste, _duplicate, _repeat, _delete })
            {
                _canvasMenu.Items.Add(_commands.MenuItem(command));
            }

            _canvasMenu.Items.Add(new ToolStripSeparator());
            foreach (EditorCommand command in new[] { _zUp, _zDown, _rotateLeft, _rotateRight, _hide, _showHidden, _moveToLayer })
            {
                _canvasMenu.Items.Add(_commands.MenuItem(command));
            }

            _canvasMenu.Items.Add(new ToolStripSeparator());
            foreach (EditorCommand command in new[] { _selectConnected, _selectSameId, _selectAll, _saveBlueprint, _savePattern })
            {
                _canvasMenu.Items.Add(_commands.MenuItem(command));
            }

            _canvasMenu.Items.Add(new ToolStripSeparator());
            foreach (EditorCommand command in new[] { _workPlane, _grid, _fit, _center })
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

            // "Draw with this tile" means the tile under the cursor, also inside a larger selection.
            _menuTile = hover;

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

        private void TogglePlanView()
        {
            // The two views share nothing on screen, so each switch starts with the whole multi in view.
            Session.PlanMode = !Session.PlanMode;
            _canvas.FitToView();
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
    }
}
