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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Rendering;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// One multi being edited: the tiles, what is selected and hovered, the view options and the
    /// active tool. It knows nothing about controls; the canvas feeds it pointer events and the
    /// panels listen to its events.
    /// </summary>
    internal sealed partial class EditorSession : IEditorContext, IMultiViewSettings
    {
        private readonly HashSet<MultiTile> _selection = new HashSet<MultiTile>();
        private readonly Dictionary<ToolKind, EditorTool> _tools = new Dictionary<ToolKind, EditorTool>();
        private readonly Dictionary<ToolKind, EditorTool> _draftTools = new Dictionary<ToolKind, EditorTool>();
        private readonly ArtMasks _masks;
        private readonly PasteTool _pasteTool;
        private readonly RepeatPreviewTool _repeatTool;

        private EditorTool _temporaryTool;
        private string _stampLabel = "Paste";
        private bool _pickClick;
        private ToolKind _toolKind = ToolKind.Select;
        private ToolKind _lastTileTool = ToolKind.Draw;
        private Point _mouseLoc;
        private bool _mouseInside;
        private (int X, int Y, int Z) _cursorCell = (int.MinValue, int.MinValue, int.MinValue);
        private int? _heightLimit;
        private (int Floor, int Ceiling)? _activeLevel;
        private bool _hideAboveLevel = true;
        private bool _hideWalls;
        private bool _showWalkables;
        private bool _showDoubleSurface;
        private bool _showGrid;
        private ushort _drawTileId;

        public EditorSession(ArtMasks masks, HouseStyles houseStyles)
        {
            _masks = masks;
            HouseStyles = houseStyles;

            _tools[ToolKind.Select] = new SelectTool(this);
            _tools[ToolKind.Lasso] = new LassoTool(this);
            _tools[ToolKind.Draw] = new DrawTool(this);
            _tools[ToolKind.Remove] = new RemoveTool(this);
            _tools[ToolKind.Altitude] = new AltitudeTool(this);
            _tools[ToolKind.Pipette] = new PipetteTool(this);
            _tools[ToolKind.Transparent] = new TransparentTool(this);
            _tools[ToolKind.RectFill] = new RectFillTool(this);
            _tools[ToolKind.Line] = new LineTool(this);
            _tools[ToolKind.Wall] = new WallTool(this);
            _tools[ToolKind.Floor] = new FloorStyleTool(this);
            _tools[ToolKind.Door] = new DoorTool(this);
            _tools[ToolKind.Stair] = new StairTool(this);
            var roof = new RoofTool(this);
            var house = new HouseTool(this, roof);
            foreach (WingDraftTool draft in new WingDraftTool[] { roof, house })
            {
                draft.Changed += () =>
                {
                    DraftChanged?.Invoke();
                    ToolChanged?.Invoke();
                    Invalidated?.Invoke();
                };
                draft.Committed += () => LayersChanged?.Invoke();
            }

            house.FreeName = FreeHouseName;
            house.Placed += placed => _placedHouses.Add(placed);

            _tools[ToolKind.Roof] = roof;
            _tools[ToolKind.House] = house;
            _draftTools[ToolKind.Wall] = new DraftWallTool(this, house);
            _draftTools[ToolKind.Door] = new DraftDoorTool(this, house);
            _draftTools[ToolKind.Floor] = new DraftFloorTool(this, house);
            _draftTools[ToolKind.Stair] = new DraftStairTool(this, house);
            _tools[ToolKind.Foundation] = new FoundationTool(this);
            _tools[ToolKind.FloorPattern] = new FloorPatternTool(this);
            _pasteTool = new PasteTool(this);
            _repeatTool = new RepeatPreviewTool(this);

            CompList = new MultiEditorComponentList(0, 0, this);
        }

        /// <summary>Something on the canvas changed: repaint.</summary>
        public event Action Invalidated;

        /// <summary>The tiles changed (edit, undo, redo, load).</summary>
        public event Action DocumentChanged;

        public event Action SelectionChanged;

        public event Action HoverChanged;

        /// <summary>The cell under the cursor changed.</summary>
        public event Action CursorCellChanged;

        public event Action ToolChanged;

        /// <summary>The layer list, a layer's name or visibility, the active layer or the tile counts changed.</summary>
        public event Action LayersChanged;

        public event Action DrawTileChanged;

        /// <summary>A view option changed (floor, level, height limit, filters).</summary>
        public event Action ViewChanged;

        /// <summary>A tile id was placed by a tool.</summary>
        public event Action<ushort> TileUsed;

        // ---------------------------------------------------------------- document

        public MultiEditorComponentList CompList { get; private set; }

        public bool HasDocument => CompList.Width > 0 && CompList.Height > 0;

        /// <summary>Name shown on the document tab.</summary>
        public string Title { get; set; } = "Untitled";

        /// <summary>
        /// True when there are edits since the last save or load. Undoing back to the step that
        /// was saved makes the design unchanged again; an edit that left no step in the history
        /// keeps it changed.
        /// </summary>
        public bool Modified { get; private set; }

        // The history step on top when the design was saved, and when it was last looked at.
        private EditCommand _savedStep;
        private EditCommand _lastStep;
        private bool _unloggedEdit;
        private bool _steppingHistory;

        private EditCommand TopStep => CompList.UndoList.Count > 0 ? CompList.UndoList[0] : null;

        private void MarkClean()
        {
            _savedStep = _lastStep = TopStep;
            _unloggedEdit = false;
            Modified = false;
        }

        /// <summary>The names of the steps Undo would take back, the latest first.</summary>
        public IReadOnlyList<string> UndoSteps(int most)
        {
            var names = new List<string>();
            if (UndoDraftTool == null)
            {
                for (int i = 0; i < CompList.UndoList.Count && i < most; ++i)
                {
                    names.Add(CompList.UndoList[i].Action);
                }
            }

            return names;
        }

        public void NewBlank(int width, int height, int firstFloorZ = 0)
        {
            SetDocument(new MultiEditorComponentList(width, height, this));
            FirstFloorZ = firstFloorZ;
        }

        public void Load(MultiComponentList list)
        {
            SetDocument(new MultiEditorComponentList(list, this));
        }

        public void MarkSaved()
        {
            MarkClean();
            DocumentChanged?.Invoke();
        }

        /// <summary>True when Undo has something to take back: a step of an open draft, or of the multi.</summary>
        public bool CanUndo => HasDocument && (UndoDraftTool != null || CompList.UndoList.Count > 0);

        public bool CanRedo => HasDocument && (RedoDraftTool != null || (OpenDraft == null && CompList.RedoList.Count > 0) ||
                                               (OpenDraft is { ReopenedFrom: not null } reopened && CompList.RedoList.Count > 0 &&
                                                ReferenceEquals(CompList.RedoList[0], reopened.ReopenedFrom)));

        // The draft whose history undo and redo walk first: the wing tool in use, else the house.
        private WingDraftTool UndoDraftTool => WingTool is { CanUndoDraft: true } tool ? tool : House.CanUndoDraft ? House : null;

        private WingDraftTool RedoDraftTool => WingTool is { CanRedoDraft: true } tool ? tool : House.CanRedoDraft ? House : null;

        /// <summary>
        /// Takes back the last step: of the draft being shaped while there is one, else of the
        /// multi. Undoing the step that placed a roof or house brings its draft back as it was.
        /// </summary>
        public bool Undo()
        {
            if (UndoDraftTool is { } draft)
            {
                return draft.UndoDraft();
            }

            EditCommand step = CompList.UndoList.Count > 0 ? CompList.UndoList[0] : null;
            if (!CompList.UndoStep())
            {
                return false;
            }

            foreach (WingDraftTool tool in new WingDraftTool[] { House, Roof })
            {
                // A roof draft shows only with the roof tool, so it comes back only there; a
                // house draft shows under every tool.
                if (tool.PlacedBy != null && ReferenceEquals(tool.PlacedBy, step) && !tool.HasPending &&
                    (tool is HouseTool || _toolKind == ToolKind.Roof))
                {
                    tool.Reopen();
                    if (tool is HouseTool reopened && reopened.LastPlaced != null)
                    {
                        _placedHouses.Remove(reopened.LastPlaced);
                    }
                }
            }

            _steppingHistory = true;
            AfterEdit();
            _steppingHistory = false;
            LayersChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Redoes a step of the draft being shaped, else of the multi. While a draft is open the
        /// multi's steps wait, except the one that placed that very draft.
        /// </summary>
        public bool Redo()
        {
            if (RedoDraftTool is { } draft)
            {
                return draft.RedoDraft();
            }

            if (OpenDraft is { } open)
            {
                if (open.ReopenedFrom == null || CompList.RedoList.Count == 0 || !ReferenceEquals(CompList.RedoList[0], open.ReopenedFrom))
                {
                    return false;
                }

                open.CloseReopened();
                if (open is HouseTool closed && closed.LastPlaced != null && !_placedHouses.Contains(closed.LastPlaced))
                {
                    _placedHouses.Add(closed.LastPlaced);
                }
            }

            if (!CompList.RedoStep())
            {
                return false;
            }

            _steppingHistory = true;
            AfterEdit();
            _steppingHistory = false;
            LayersChanged?.Invoke();
            return true;
        }

        private void SetDocument(MultiEditorComponentList list)
        {
            CancelTool();
            Roof.Reset();
            House.Reset();
            _placedHouses.Clear();
            CompList = list;
            _selection.Clear();
            HoverTile = null;
            _heightLimit = null;
            _activeLevel = null;
            _levelIndex = 0;
            _workPlaneOffset = 0;
            MarkClean();
            _firstFloorZ = DetectFirstFloorZ();
            _levelDefinitions.Clear();
            CustomLevels = false;
            RebuildLevels();

            DocumentChanged?.Invoke();
            LayersChanged?.Invoke();
            SelectionChanged?.Invoke();
            ViewChanged?.Invoke();
            Invalidated?.Invoke();
        }

        // ---------------------------------------------------------------- view options

        /// <summary>
        /// The Z you build on: the work plane's, once a floor is chosen or the plane was moved.
        /// Tiles below it cannot be edited, and drawing on an empty cell and the house tools land
        /// on it. Null with all floors shown and the plane where it starts.
        /// </summary>
        public int? WorkZ => _levelIndex > 0 || _workPlaneOffset != 0 ? WorkPlaneZ : null;

        /// <summary>Highest Z drawn, or null to draw everything.</summary>
        public int? HeightLimit
        {
            get => _heightLimit;
            set => SetView(ref _heightLimit, value);
        }

        /// <summary>The level being edited: tiles from Floor up to (not including) Ceiling. Null shows all.</summary>
        public (int Floor, int Ceiling)? ActiveLevel
        {
            get => _activeLevel;
            set => SetView(ref _activeLevel, value);
        }

        /// <summary>Tiles above the active level are hidden instead of drawn faint.</summary>
        public bool HideAboveLevel
        {
            get => _hideAboveLevel;
            set => SetView(ref _hideAboveLevel, value);
        }

        public bool HideWalls
        {
            get => _hideWalls;
            set => SetView(ref _hideWalls, value);
        }

        public bool ShowWalkables
        {
            get => _showWalkables;
            set
            {
                if (SetView(ref _showWalkables, value) && value)
                {
                    CompList.CalcWalkable();
                }
            }
        }

        public bool ShowDoubleSurface
        {
            get => _showDoubleSurface;
            set
            {
                if (SetView(ref _showDoubleSurface, value) && value)
                {
                    CompList.CalcDoubleSurface();
                }
            }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set => SetView(ref _showGrid, value);
        }

        /// <summary>
        /// Highest Z you work on: the height limit, lowered to the active level's top. Tiles above
        /// it can be seen (faint) but not hovered, selected or removed.
        /// </summary>
        public int EditMaxHeight
        {
            get
            {
                int limit = _heightLimit ?? sbyte.MaxValue;
                return _activeLevel is { } level ? Math.Min(limit, level.Ceiling - 1) : limit;
            }
        }

        public void HideSelection()
        {
            if (_selection.Count == 0)
            {
                return;
            }

            foreach (MultiTile tile in _selection)
            {
                tile.Hidden = true;
            }

            _selection.Clear();
            SelectionChanged?.Invoke();
            OnViewChanged();
        }

        public void ShowHiddenTiles()
        {
            foreach (MultiTile tile in CompList.RealTiles)
            {
                tile.Hidden = false;
            }

            OnViewChanged();
        }

        private bool SetView<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnViewChanged();
            return true;
        }

        /// <summary>After a view option changed: drop selected tiles that can no longer be seen, refind hover.</summary>
        private void OnViewChanged()
        {
            int removed = _selection.RemoveWhere(t =>
                !CompList.IsShown(t) || (_activeLevel is { } level && (t.Z >= level.Ceiling || t.Z < level.Floor)));
            if (removed > 0)
            {
                SelectionChanged?.Invoke();
            }

            _cursorCell = (int.MinValue, int.MinValue, int.MinValue);
            UpdateHover();
            UpdateCursorCell();
            ViewChanged?.Invoke();
            Invalidated?.Invoke();
        }

        // ---------------------------------------------------------------- tools

        public ToolKind Tool
        {
            get => _toolKind;
            set
            {
                // A roof still being shaped is placed when another tool is taken up.
                // A house draft stays open instead: the wall, door, floor and stair tools work on it.
                if (_toolKind == ToolKind.Roof && value != ToolKind.Roof)
                {
                    Roof.Commit();
                }

                CancelTool();
                _toolKind = value;
                if (IsTileTool(value))
                {
                    _lastTileTool = value;
                }

                ToolChanged?.Invoke();
                Invalidated?.Invoke();
            }
        }

        /// <summary>
        /// The tool in use. While a house draft is open the wall, door, floor and stair tools are
        /// the ones that change the draft.
        /// </summary>
        public EditorTool ActiveTool => _temporaryTool ??
            (House.HasPending && _draftTools.TryGetValue(_toolKind, out EditorTool draftTool) ? draftTool : _tools[_toolKind]);

        /// <summary>True for the tools that place the draw tile (draw, rectangle, line).</summary>
        public static bool IsTileTool(ToolKind kind)
        {
            return kind is ToolKind.Draw or ToolKind.RectFill or ToolKind.Line;
        }

        /// <summary>
        /// True while Alt is held with a draw tile tool and nothing is being dragged: a click then
        /// picks the tile under the cursor instead of placing one, and the tool shows no preview.
        /// </summary>
        public bool PickingWithAlt =>
            _temporaryTool == null && IsTileTool(_toolKind) && !_tools[_toolKind].IsDragging &&
            (Control.ModifierKeys & Keys.Alt) != 0;

        /// <summary>The bordered floor laid by the floor pattern tool, or null.</summary>
        public FloorPattern FloorPattern { get; set; }

        /// <summary>The roof tool, for its shape options (ridge, gables, gable fill) and the roof being shaped.</summary>
        public RoofTool Roof => (RoofTool)_tools[ToolKind.Roof];

        /// <summary>Call after a tool option changed, so the preview under the cursor follows.</summary>
        public void ToolOptionsChanged()
        {
            Invalidated?.Invoke();
        }

        /// <summary>True while tiles are on the cursor to be placed (paste, blueprint, duplicate) or a repeat is previewed.</summary>
        public bool Pasting => _temporaryTool != null;

        /// <summary>What is on the cursor while <see cref="Pasting"/>: "Paste", "Blueprint", "Duplicate"...</summary>
        public string StampLabel => _stampLabel;

        public ushort DrawTileId => _drawTileId;

        public int BrushSize { get; set; } = 1;

        public PlacementMode Placement { get; set; } = PlacementMode.Stack;

        public HouseStyles HouseStyles { get; }

        public HouseStyle HouseStyle { get; set; }

        public int ZStep { get; set; } = 1;

        /// <summary>Drops a drag, a pending paste or any other half-done tool action.</summary>
        public void CancelTool()
        {
            bool wasTemporary = _temporaryTool != null;
            _temporaryTool?.Cancel();
            _temporaryTool = null;
            _pickClick = false;
            if (wasTemporary)
            {
                // Esc and right click are the only ways out of a stamp: the labels follow.
                ToolChanged?.Invoke();
            }

            _tools[_toolKind].Cancel();
            Invalidated?.Invoke();
        }

        /// <summary>
        /// Starts placing a group of tiles with the mouse. They stay on the cursor for more
        /// placements until the tool is cancelled. <paramref name="label"/> names what is placed,
        /// for the tool label and the undo step.
        /// </summary>
        public void BeginPaste(List<ClipboardTile> tiles, string label = "Paste")
        {
            if (tiles == null || tiles.Count == 0 || !HasDocument)
            {
                return;
            }

            CancelTool();
            _stampLabel = label;
            _pasteTool.Begin(tiles, label);
            _temporaryTool = _pasteTool;
            ToolChanged?.Invoke();
            Invalidated?.Invoke();
        }

        public void SetDrawTile(ushort id)
        {
            _drawTileId = id;
            DrawTileChanged?.Invoke();
            Invalidated?.Invoke();
        }

        /// <summary>Makes a tile picked on the canvas the draw tile; the pick tool hands over to the draw tool used last.</summary>
        public void PickDrawTile(ushort id)
        {
            SetDrawTile(id);
            if (_temporaryTool == null && _toolKind == ToolKind.Pipette)
            {
                Tool = _lastTileTool;
            }
        }

        // ---------------------------------------------------------------- IEditorContext

        Point IEditorContext.MouseLoc => _mouseLoc;

        int IEditorContext.MaxHeight => EditMaxHeight;

        // Canvas space is world space here: nothing is scrolled.
        int IEditorContext.ScrollX => 0;

        int IEditorContext.ScrollY => 0;

        void IEditorContext.CellAt(Point canvasLoc, out int x, out int y, out int z) => ConvertCoords(canvasLoc, out x, out y, out z);

        void IEditorContext.PlaneCell(Point canvasLoc, int z, out int x, out int y) => CellOnPlane(canvasLoc, z, out x, out y);

        public List<MultiTile> TilesInRect(Rectangle worldRect)
        {
            if (_planMode)
            {
                return Plan.TilesInRect(worldRect, EditMaxHeight, EditMinZ);
            }

            var result = new List<MultiTile>();
            foreach (MultiTile tile in CompList.RealTiles)
            {
                if (IsEditable(tile) && ArtMetrics.TryGetSize(tile.Id, out int width, out int height) &&
                    worldRect.IntersectsWith(new Rectangle(tile.XMod, tile.YMod, width, height)))
                {
                    result.Add(tile);
                }
            }

            return result;
        }

        /// <summary>
        /// Call after an edit. With <paramref name="raiseTo"/> a height limit is raised to show
        /// tiles placed at that Z.
        /// </summary>
        public void AfterEdit(int? raiseTo = null)
        {
            if (_heightLimit is { } limit && raiseTo is { } raise && limit < raise)
            {
                _heightLimit = raise;
                ViewChanged?.Invoke();
            }

            // Tiles removed by the edit (or by undo) are no longer selectable.
            if (_selection.RemoveWhere(t => !CompList.Contains(t) || !CompList.IsShown(t)) > 0)
            {
                SelectionChanged?.Invoke();
            }

            // A change that left no step in the history cannot be undone back to the saved state.
            EditCommand top = TopStep;
            if (ReferenceEquals(top, _lastStep) && !_steppingHistory)
            {
                _unloggedEdit = true;
            }

            _lastStep = top;
            Modified = _unloggedEdit || !ReferenceEquals(top, _savedStep);
            RebuildLevels();
            if (_showWalkables)
            {
                CompList.CalcWalkable();
            }

            if (_showDoubleSurface)
            {
                CompList.CalcDoubleSurface();
            }

            UpdateHover();
            _cursorCell = (int.MinValue, int.MinValue, int.MinValue);
            UpdateCursorCell();
            DocumentChanged?.Invoke();
            LayersChanged?.Invoke();
            Invalidated?.Invoke();
        }

        void IEditorContext.RecordRecentTile(ushort id) => TileUsed?.Invoke(id);

        // ---------------------------------------------------------------- coordinates

        /// <summary>World position of the top corner of a cell's floor diamond at height z.</summary>
        public static PointF CellCorner(int x, int y, int z)
        {
            return new PointF(((x - y) * 22) + MultiEditorComponentList.GapXMod, ((x + y) * 22) - (z * 4) - 22);
        }

        /// <summary>The cell under a world point on the plane at height z, in the current view (isometric or plan).</summary>
        public void CellOnPlane(Point world, int z, out int x, out int y)
        {
            if (_planMode)
            {
                (x, y) = PlanView.CellAt(world);
                return;
            }

            PlaneCell(world, z, out x, out y);
        }

        /// <summary>The cell under a world point on the flat plane at height <paramref name="z"/>.</summary>
        public static void PlaneCell(Point world, int z, out int x, out int y)
        {
            // Relative to the top corner of cell (0,0) at that height.
            double mx = world.X - MultiEditorComponentList.GapXMod;
            double my = world.Y + (z * 4) + 22;

            // Floor, not truncation, so cells left of or above the multi come out negative.
            x = (int)Math.Floor((mx + my) / 44.0);
            y = (int)Math.Floor((my - mx) / 44.0);
        }

        /// <summary>
        /// The cell under a world point and the Z a new tile gets there in the current placement mode.
        /// </summary>
        private void ConvertCoords(Point world, out int x, out int y, out int z)
        {
            if (Placement == PlacementMode.FixedZ)
            {
                z = WorkPlaneZ;
                CellOnPlane(world, z, out x, out y);
                return;
            }

            MultiTile hover = HoverTile;
            if (hover != null && (WorkZ is not { } baseZ || hover.Z >= baseZ))
            {
                x = hover.X;
                y = hover.Y;
                if (Placement == PlacementMode.Replace)
                {
                    z = hover.Z;
                    return;
                }

                z = hover.Z + hover.Height;
                if (_heightLimit is { } limit)
                {
                    z = Math.Min(z, limit);
                }

                return;
            }

            // No tile under the mouse: the cell on the work plane, or on Z 0.
            z = WorkZ ?? 0;
            CellOnPlane(world, z, out x, out y);
        }
    }
}
