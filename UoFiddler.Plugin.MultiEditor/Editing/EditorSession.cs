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
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Rendering;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// The tools of the editor, by the name the UI shows.
    /// </summary>
    internal enum ToolKind
    {
        Select,
        Lasso,
        Draw,
        Remove,
        Altitude,
        Pipette,
        Transparent,
        RectFill,
        Line,
        Wall,
        Floor,
        Door,
        Stair,
        Roof,
        Foundation,

        /// <summary>Lays a bordered floor pattern; chosen from the House page, not the palette.</summary>
        FloorPattern
    }

    /// <summary>
    /// One multi being edited: the tiles, what is selected and hovered, the view options and the
    /// active tool. It knows nothing about controls; the canvas feeds it pointer events and the
    /// panels listen to its events.
    /// </summary>
    internal sealed partial class EditorSession : IEditorContext, IMultiViewSettings
    {
        private readonly HashSet<MultiTile> _selection = new HashSet<MultiTile>();
        private readonly Dictionary<ToolKind, EditorTool> _tools = new Dictionary<ToolKind, EditorTool>();
        private readonly ArtMasks _masks;
        private readonly PasteTool _pasteTool;

        private EditorTool _temporaryTool;
        private ToolKind _toolKind = ToolKind.Select;
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
            _tools[ToolKind.Roof] = new RoofTool(this);
            _tools[ToolKind.Foundation] = new FoundationTool(this);
            _tools[ToolKind.FloorPattern] = new FloorPatternTool(this);
            _pasteTool = new PasteTool(this);

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

        /// <summary>True when there are edits since the last save or load.</summary>
        public bool Modified { get; private set; }

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
            Modified = false;
            DocumentChanged?.Invoke();
        }

        public bool Undo()
        {
            if (!CompList.UndoStep())
            {
                return false;
            }

            AfterEdit();
            return true;
        }

        public bool Redo()
        {
            if (!CompList.RedoStep())
            {
                return false;
            }

            AfterEdit();
            return true;
        }

        private void SetDocument(MultiEditorComponentList list)
        {
            CancelTool();
            CompList = list;
            _selection.Clear();
            HoverTile = null;
            _heightLimit = null;
            _activeLevel = null;
            _levelIndex = 0;
            _workPlaneOffset = 0;
            Modified = false;
            _firstFloorZ = DetectFirstFloorZ();
            _levelDefinitions.Clear();
            CustomLevels = false;
            RebuildLevels();

            DocumentChanged?.Invoke();
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
                CancelTool();
                _toolKind = value;
                ToolChanged?.Invoke();
                Invalidated?.Invoke();
            }
        }

        public EditorTool ActiveTool => _temporaryTool ?? _tools[_toolKind];

        /// <summary>The bordered floor laid by the floor pattern tool, or null.</summary>
        public FloorPattern FloorPattern { get; set; }

        /// <summary>The roof tool, for its shape options (ridge, gables, gable fill).</summary>
        public RoofTool Roof => (RoofTool)_tools[ToolKind.Roof];

        /// <summary>Call after a tool option changed, so the preview under the cursor follows.</summary>
        public void ToolOptionsChanged()
        {
            Invalidated?.Invoke();
        }

        /// <summary>True while a paste is waiting to be placed.</summary>
        public bool Pasting => _temporaryTool != null;

        public ushort DrawTileId => _drawTileId;

        public int BrushSize { get; set; } = 1;

        public PlacementMode Placement { get; set; } = PlacementMode.Stack;

        public HouseStyles HouseStyles { get; }

        public HouseStyle HouseStyle { get; set; }

        public int ZStep { get; set; } = 1;

        /// <summary>Drops a drag, a pending paste or any other half-done tool action.</summary>
        public void CancelTool()
        {
            _temporaryTool?.Cancel();
            _temporaryTool = null;
            _tools[_toolKind].Cancel();
            Invalidated?.Invoke();
        }

        /// <summary>Starts placing a group of tiles with the mouse.</summary>
        public void BeginPaste(List<ClipboardTile> tiles)
        {
            if (tiles == null || tiles.Count == 0 || !HasDocument)
            {
                return;
            }

            CancelTool();
            _pasteTool.Begin(tiles);
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

        // ---------------------------------------------------------------- selection and hover

        public MultiTile HoverTile { get; private set; }

        public IReadOnlyCollection<MultiTile> Selection => _selection;

        /// <summary>The selected tile when exactly one is selected.</summary>
        public MultiTile SelectedTile => _selection.Count == 1 ? _selection.First() : null;

        public bool IsSelected(MultiTile tile) => _selection.Contains(tile);

        public (int X, int Y, int Z) CursorCell => _cursorCell;

        public bool MouseInside => _mouseInside;

        public void SetSelection(IEnumerable<MultiTile> tiles, bool add)
        {
            if (!add)
            {
                _selection.Clear();
            }

            foreach (MultiTile tile in tiles)
            {
                _selection.Add(tile);
            }

            SelectionChanged?.Invoke();
            Invalidated?.Invoke();
        }

        public void ToggleSelected(MultiTile tile)
        {
            if (!_selection.Remove(tile))
            {
                _selection.Add(tile);
            }

            SelectionChanged?.Invoke();
            Invalidated?.Invoke();
        }

        public void ClearSelection()
        {
            if (_selection.Count == 0)
            {
                return;
            }

            _selection.Clear();
            SelectionChanged?.Invoke();
            Invalidated?.Invoke();
        }

        /// <summary>Takes tiles out of the selection, with one change notice.</summary>
        public void Deselect(IEnumerable<MultiTile> tiles)
        {
            bool changed = false;
            foreach (MultiTile tile in tiles)
            {
                changed |= _selection.Remove(tile);
            }

            if (changed)
            {
                SelectionChanged?.Invoke();
                Invalidated?.Invoke();
            }
        }

        private HashSet<MultiTile> _listHighlight = new HashSet<MultiTile>();

        /// <summary>
        /// Tiles pointed at outside the canvas (a row of the selection list), drawn like the hovered
        /// tile. Empty for none.
        /// </summary>
        public IReadOnlyCollection<MultiTile> ListHighlight
        {
            get => _listHighlight;
            set
            {
                var next = value == null ? new HashSet<MultiTile>() : new HashSet<MultiTile>(value);
                if (next.SetEquals(_listHighlight))
                {
                    return;
                }

                _listHighlight = next;
                Invalidated?.Invoke();
            }
        }

        public bool IsListHighlighted(MultiTile tile) => _listHighlight.Count > 0 && _listHighlight.Contains(tile);

        /// <summary>True when a tile can be hovered, selected and edited with the current view.</summary>
        public bool IsEditable(MultiTile tile)
        {
            return tile.Z <= EditMaxHeight && !(WorkZ is { } baseZ && baseZ > tile.Z) && CompList.IsShown(tile);
        }

        /// <summary>The topmost editable tile with a drawn pixel at a world point, or null.</summary>
        public MultiTile HitTest(Point world)
        {
            if (_planMode)
            {
                (int cx, int cy) = PlanView.CellAt(world);
                return CompList.InBounds(cx, cy) ? Plan.TopTile(cx, cy, EditMaxHeight, EditMinZ) : null;
            }

            MultiTile hit = null;

            // Draw order: the last match is the one on top. Runs on every mouse move over every
            // tile, so the point is first checked against the art's rectangle, which rules out
            // almost all of them without a lookup.
            foreach (MultiTile tile in CompList.RealTiles)
            {
                int px = world.X - tile.XMod;
                int py = world.Y - tile.YMod;
                if ((uint)px >= (uint)tile.ArtWidth || (uint)py >= (uint)tile.ArtHeight)
                {
                    continue;
                }

                if (IsEditable(tile) && _masks.Hit(tile.Id, px, py))
                {
                    hit = tile;
                }
            }

            return hit;
        }

        /// <summary>How long the last hover update's hit test took, for the performance box.</summary>
        public double LastHitTestMs { get; private set; }

        private bool UpdateHover()
        {
            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            MultiTile hover = _mouseInside && HasDocument && !_previewMode ? HitTest(_mouseLoc) : null;
            LastHitTestMs = System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            if (hover == HoverTile)
            {
                return false;
            }

            HoverTile = hover;
            HoverChanged?.Invoke();
            return true;
        }

        private bool UpdateCursorCell()
        {
            if (!_mouseInside || !HasDocument)
            {
                return false;
            }

            ConvertCoords(_mouseLoc, out int x, out int y, out int z);
            if ((x, y, z) == _cursorCell)
            {
                return false;
            }

            _cursorCell = (x, y, z);
            CursorCellChanged?.Invoke();
            return true;
        }

        // ---------------------------------------------------------------- pointer input (world space)

        public void PointerMove(Point world, MouseEventArgs e)
        {
            _mouseLoc = world;
            _mouseInside = true;
            if (!HasDocument || _previewMode)
            {
                return;
            }

            // Hover first: tools read the cell under the mouse, which depends on the hovered tile.
            bool hoverChanged = UpdateHover();
            bool cellChanged = UpdateCursorCell();
            EditorTool tool = ActiveTool;
            tool.OnMouseMove(e);
            if (hoverChanged || tool.IsDragging || (cellChanged && tool.TracksCursorCell))
            {
                Invalidated?.Invoke();
            }
        }

        public void PointerLeave()
        {
            _mouseInside = false;
            _cursorCell = (int.MinValue, int.MinValue, int.MinValue);
            UpdateHover();
            CursorCellChanged?.Invoke();
            Invalidated?.Invoke();
        }

        public void PointerDown(Point world, MouseEventArgs e)
        {
            _mouseLoc = world;
            _mouseInside = true;
            if (HasDocument && !_previewMode && e.Button == MouseButtons.Left)
            {
                UpdateHover();
                UpdateCursorCell();
                ActiveTool.OnMouseDown(e);
                Invalidated?.Invoke();
            }
        }

        public void PointerUp(Point world, MouseEventArgs e)
        {
            _mouseLoc = world;
            if (!HasDocument || _previewMode)
            {
                return;
            }

            if (e.Button == MouseButtons.Middle)
            {
                // Selects the hovered tile with any tool, so the cell stack lists its cell.
                if (HoverTile != null)
                {
                    SetSelection(new[] { HoverTile }, add: false);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                ActiveTool.OnMouseUp(e);
            }

            Invalidated?.Invoke();
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
            if (_selection.RemoveWhere(t => !CompList.Contains(t)) > 0)
            {
                SelectionChanged?.Invoke();
            }

            Modified = true;
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
            Invalidated?.Invoke();
        }

        void IEditorContext.RecordRecentTile(ushort id) => TileUsed?.Invoke(id);

        void IEditorContext.EndTemporaryTool()
        {
            _temporaryTool = null;
            ToolChanged?.Invoke();
            Invalidated?.Invoke();
        }

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
