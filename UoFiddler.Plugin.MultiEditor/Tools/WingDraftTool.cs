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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// One rectangle of a roof or a house being shaped. For a roof the box is the cells the roof
    /// covers and <see cref="EaveZ"/> the height of its eaves; for a house the box is the wall
    /// cells and <see cref="EaveZ"/> the Z its ground floor stands on.
    /// </summary>
    internal sealed class RoofWing
    {
        public int X0 { get; set; }

        public int Y0 { get; set; }

        public int X1 { get; set; }

        public int Y1 { get; set; }

        public int EaveZ { get; set; }

        /// <summary>Storeys of a house wing.</summary>
        public int Storeys { get; set; } = 1;

        /// <summary>A house wing left without a roof.</summary>
        public bool NoRoof { get; set; }

        /// <summary>
        /// Rooms a house wing is divided into on each of its storeys: partitions across its longer
        /// side, each with a doorway left in the middle. 1 leaves the wing undivided.
        /// </summary>
        public int Rooms { get; set; } = 1;

        /// <summary>How the rooms of a house wing are laid out.</summary>
        public RoomLayout Layout { get; set; }

        /// <summary>
        /// Rooms and layout of the storeys above the ground floor that have their own; a storey
        /// without takes those of the storey below it. Index 0 is not used.
        /// </summary>
        public (int Rooms, RoomLayout Layout)?[] RoomsAbove { get; private set; } = new (int, RoomLayout)?[HouseDraft.MaxStoreys];

        /// <summary>Rooms and layout of a storey.</summary>
        public (int Rooms, RoomLayout Layout) RoomsOn(int storey)
        {
            for (int k = Math.Min(storey, HouseDraft.MaxStoreys - 1); k >= 1; --k)
            {
                if (RoomsAbove[k] is { } own)
                {
                    return own;
                }
            }

            return (Rooms, Layout);
        }

        public RoofShape Shape { get; set; }

        public bool RidgeAlongY { get; set; }

        /// <summary>True while the ridge follows the box's proportions as it is resized.</summary>
        public bool RidgeAuto { get; set; }

        public (int X0, int Y0, int X1, int Y1) Box => (X0, Y0, X1, Y1);

        public RoofWing Clone()
        {
            var copy = (RoofWing)MemberwiseClone();
            copy.RoomsAbove = ((int, RoomLayout)?[])RoomsAbove.Clone();
            return copy;
        }

        public void CopyFrom(RoofWing other)
        {
            (X0, Y0, X1, Y1, EaveZ) = (other.X0, other.Y0, other.X1, other.Y1, other.EaveZ);
            (Shape, RidgeAlongY, RidgeAuto, Storeys, NoRoof) = (other.Shape, other.RidgeAlongY, other.RidgeAuto, other.Storeys, other.NoRoof);
            Rooms = other.Rooms;
            Layout = other.Layout;
            RoomsAbove = ((int, RoomLayout)?[])other.RoomsAbove.Clone();
        }
    }

    /// <summary>
    /// Base for the tools that shape something out of wings before it is placed (roofs, houses).
    /// The first stroke starts a draft, shown as it will be built. Then: click a wing to select
    /// it, drag it to move it, drag an outermost row or column of the selected wing to resize
    /// it, drag anywhere else to add a wing, Shift+click to turn one, Delete to remove one.
    /// Nothing is written to the multi until <see cref="Commit"/>; <see cref="Discard"/> drops
    /// the draft. Every change to the draft is a step of its own undo history, which
    /// <see cref="UndoDraft"/> and <see cref="RedoDraft"/> walk; dropping the draft is such a
    /// step too, and undoing a placement in the multi brings the draft back with its history.
    /// </summary>
    internal abstract class WingDraftTool : EditorTool
    {
        private const int _historySize = 200;

        private enum DragKind
        {
            None,
            New,
            Move,
            Resize
        }

        private sealed class State
        {
            public List<RoofWing> Wings;
            public int Selected;
            public object Extra;
        }

        private List<RoofWing> _wings = new List<RoofWing>();
        private int _selected = -1;
        private List<State> _undo = new List<State>();
        private List<State> _redo = new List<State>();

        private DragKind _drag;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;
        private int _z;
        private RoofWing _before;
        private State _dragStart;
        private int _grabX;
        private int _grabY;
        private bool _west;
        private bool _east;
        private bool _north;
        private bool _south;

        // The last placement, so undoing it in the multi can bring the draft back.
        private State _placed;
        private List<State> _placedHistory;

        protected WingDraftTool(IEditorContext context) : base(context)
        {
        }

        /// <summary>Raised when the draft starts, changes or ends, and when the selected wing changes.</summary>
        public event Action Changed;

        /// <summary>Raised after the draft was placed.</summary>
        public event Action Committed;

        /// <summary>True while there is a draft that is not placed yet.</summary>
        public bool HasPending => _wings.Count > 0;

        public IReadOnlyList<RoofWing> Wings => _wings;

        public RoofWing SelectedWing => _selected >= 0 && _selected < _wings.Count ? _wings[_selected] : null;

        public bool HasSelectedWing => SelectedWing != null;

        public int SelectedIndex
        {
            get => SelectedWing != null ? _selected : -1;
            set
            {
                int index = value >= 0 && value < _wings.Count ? value : -1;
                if (index != _selected)
                {
                    _selected = index;
                    RaiseChanged();
                }
            }
        }

        public bool CanUndoDraft => _undo.Count > 0;

        public bool CanRedoDraft => _redo.Count > 0;

        /// <summary>The multi's undo step that placed the last draft, or null.</summary>
        public object PlacedBy { get; private set; }

        /// <summary>The multi's undo step whose undoing brought the current draft back, or null.</summary>
        public object ReopenedFrom { get; private set; }

        public override bool IsDragging => _drag != DragKind.None;

        public override (int Width, int Height)? DragSize => _drag switch
        {
            DragKind.New => (Math.Abs(_endX - _startX) + 1, Math.Abs(_endY - _startY) + 1),
            DragKind.Resize or DragKind.Move when SelectedWing is { } wing => (wing.X1 - wing.X0 + 1, wing.Y1 - wing.Y0 + 1),
            _ => null
        };

        public override bool TracksCursorCell => true;

        /// <summary>True when the tool has what it needs to build (its styles).</summary>
        protected abstract bool Ready { get; }

        /// <summary>The edit's name in the multi's undo list.</summary>
        protected abstract string ActionName { get; }

        /// <summary>The layer tiles go on when they name none.</summary>
        protected abstract string DefaultLayer { get; }

        /// <summary>The cell and Z a first stroke starts at.</summary>
        protected abstract void StartCell(out int x, out int y, out int z);

        /// <summary>The wings a stroke makes; none when the stroke means nothing. <paramref name="fitted"/> tells a stroke that was fitted to something there.</summary>
        protected abstract List<RoofWing> WingsFor(int startX, int startY, int endX, int endY, int z, out bool fitted);

        /// <summary>The tiles a draft of these wings takes away and adds; an added tile names its layer or none.</summary>
        protected abstract (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z, string Layer)> Add) Plan(IReadOnlyList<RoofWing> wings);

        /// <summary>The height a wing's outline and grips are drawn and taken hold of at.</summary>
        protected virtual int GripZ(RoofWing wing) => wing.EaveZ;

        /// <summary>The Z a further wing starts at.</summary>
        protected virtual int NextWingZ => GripZ(SelectedWing ?? _wings[^1]);

        /// <summary>True when the tool shows what a click would start before there is a draft.</summary>
        protected virtual bool PreviewsFirstClick => true;

        /// <summary>True when the draft is drawn only up to the highest Z the view shows (the chosen floor, the height slider).</summary>
        protected virtual bool GhostsFollowView => false;

        /// <summary>The layer of the multi that tiles planned for a layer are placed on: the same one, unless the tool gathers them.</summary>
        protected virtual string LayerFor(string layer) => layer;

        /// <summary>True when the tiles a draft puts on a layer are drawn in its preview. What is placed does not depend on it.</summary>
        protected virtual bool ShowsInPreview(string layer) => true;

        /// <summary>What the draft holds beside its wings, for the undo history.</summary>
        protected virtual object CaptureExtra() => null;

        protected virtual void RestoreExtra(object extra)
        {
        }

        /// <summary>Called when the draft was written to the multi, before it is cleared.</summary>
        protected virtual void OnPlaced()
        {
        }

        /// <summary>Makes these wings the draft, as one step of its history.</summary>
        protected void LoadWings(List<RoofWing> wings)
        {
            CancelDrag();
            Remember();
            _wings = wings;
            _selected = wings.Count > 0 ? 0 : -1;
        }

        /// <summary>
        /// Called while a wing is moved or resized, to pull it into line with the other wings.
        /// A moved wing is shifted whole; a resized one has the side in question moved.
        /// </summary>
        protected virtual void Snap(RoofWing wing, bool moved)
        {
        }

        /// <summary>Called when a wing's box changed.</summary>
        protected virtual void WingResized(RoofWing wing)
        {
        }

        /// <summary>Shift+click on a wing.</summary>
        protected virtual void Turn(RoofWing wing)
        {
        }

        /// <summary>Drawn after the ghost tiles and before the wing outlines.</summary>
        protected virtual void PaintDetails(IToolOverlay overlay, bool active)
        {
        }

        // ---------------------------------------------------------------- history

        private State Capture()
        {
            return new State { Wings = _wings.Select(w => w.Clone()).ToList(), Selected = _selected, Extra = CaptureExtra() };
        }

        private void Restore(State state)
        {
            _wings = state.Wings.Select(w => w.Clone()).ToList();
            _selected = state.Selected;
            RestoreExtra(state.Extra);
        }

        /// <summary>Call before changing the draft: the state as it is becomes an undo step.</summary>
        public void Remember()
        {
            Push(Capture());
        }

        private void Push(State state)
        {
            _undo.Add(state);
            if (_undo.Count > _historySize)
            {
                _undo.RemoveAt(0);
            }

            _redo.Clear();
            ReopenedFrom = null;
        }

        protected void RaiseChanged() => Changed?.Invoke();

        public bool UndoDraft()
        {
            if (_undo.Count == 0)
            {
                return false;
            }

            CancelDrag();
            _redo.Add(Capture());
            Restore(_undo[^1]);
            _undo.RemoveAt(_undo.Count - 1);
            RaiseChanged();
            return true;
        }

        public bool RedoDraft()
        {
            if (_redo.Count == 0)
            {
                return false;
            }

            CancelDrag();
            _undo.Add(Capture());
            Restore(_redo[^1]);
            _redo.RemoveAt(_redo.Count - 1);
            RaiseChanged();
            return true;
        }

        // ---------------------------------------------------------------- shaping

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (!Ready)
            {
                return;
            }

            if (HasPending)
            {
                int hit = HitWing(out int cx, out int cy);
                if (hit >= 0)
                {
                    RoofWing wing = _wings[hit];
                    bool wasSelected = hit == _selected;
                    if (ShiftHeld)
                    {
                        Remember();
                        _selected = hit;
                        Turn(wing);
                        RaiseChanged();
                        return;
                    }

                    _selected = hit;
                    _before = wing.Clone();
                    _dragStart = Capture();
                    (_grabX, _grabY) = (cx, cy);
                    Edges(wing, cx, cy, wasSelected, out _west, out _east, out _north, out _south);
                    _drag = _west || _east || _north || _south ? DragKind.Resize : DragKind.Move;
                    RaiseChanged();
                    return;
                }

                _z = NextWingZ;
                Context.PlaneCell(Context.MouseLoc, _z, out _startX, out _startY);
            }
            else
            {
                StartCell(out _startX, out _startY, out _z);
            }

            (_endX, _endY) = (_startX, _startY);
            _drag = DragKind.New;
        }

        /// <summary>
        /// The sides a press on a cell of the selected wing takes hold of: its outermost rows and
        /// columns. A side too short to have a middle moves the wing instead.
        /// </summary>
        private static void Edges(RoofWing wing, int cx, int cy, bool selected, out bool west, out bool east, out bool north, out bool south)
        {
            bool wide = wing.X1 - wing.X0 >= 2, deep = wing.Y1 - wing.Y0 >= 2;
            west = selected && wide && cx == wing.X0;
            east = selected && wide && cx == wing.X1;
            north = selected && deep && cy == wing.Y0;
            south = selected && deep && cy == wing.Y1;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            switch (_drag)
            {
                case DragKind.New:
                    Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
                    break;
                case DragKind.Move:
                case DragKind.Resize:
                    DragWing();
                    break;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            DragKind drag = _drag;
            if (drag is DragKind.Move or DragKind.Resize)
            {
                DragWing();
            }

            _drag = DragKind.None;
            if (drag == DragKind.New)
            {
                Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
                List<RoofWing> wings = WingsFor(_startX, _startY, _endX, _endY, _z, out bool fitted);
                if (wings.Count == 0 || (HasPending && !fitted && _startX == _endX && _startY == _endY))
                {
                    // A click beside the draft lets go of the selected wing.
                    _selected = -1;
                }
                else
                {
                    Remember();
                    _wings.AddRange(wings);
                    _selected = _wings.Count - wings.Count;
                }

                RaiseChanged();
            }
            else if (drag != DragKind.None)
            {
                if (SelectedWing is { } wing && _before != null && wing.Box != _before.Box && _dragStart != null)
                {
                    Push(_dragStart);
                }

                _before = null;
                _dragStart = null;
                RaiseChanged();
            }
        }

        /// <summary>Stops a drag: a wing being moved or resized goes back to where it was.</summary>
        public override void Cancel()
        {
            CancelDrag();
        }

        private void CancelDrag()
        {
            if (_drag is DragKind.Move or DragKind.Resize && _before != null && SelectedWing is { } wing)
            {
                wing.CopyFrom(_before);
            }

            _before = null;
            _dragStart = null;
            _drag = DragKind.None;
        }

        /// <summary>Drops the draft. A step of the draft's history: undo brings it back.</summary>
        public void Discard()
        {
            CancelDrag();
            if (!HasPending)
            {
                return;
            }

            Remember();
            _wings.Clear();
            _selected = -1;
            RestoreExtra(null);
            RaiseChanged();
        }

        /// <summary>Forgets the draft and its history (another document).</summary>
        public void Reset()
        {
            CancelDrag();
            bool had = HasPending;
            _wings.Clear();
            _selected = -1;
            _undo.Clear();
            _redo.Clear();
            _placed = null;
            _placedHistory = null;
            PlacedBy = null;
            ReopenedFrom = null;
            RestoreExtra(null);
            if (had)
            {
                RaiseChanged();
            }
        }

        /// <summary>Removes the selected wing from the draft.</summary>
        public void DeleteWing()
        {
            if (SelectedWing == null || IsDragging)
            {
                return;
            }

            Remember();
            _wings.RemoveAt(_selected);
            _selected = _wings.Count - 1;
            RaiseChanged();
        }

        /// <summary>
        /// Places the draft in one undo step of the multi. Undoing that step brings the draft
        /// back (<see cref="Reopen"/>).
        /// </summary>
        public void Commit()
        {
            CancelDrag();
            if (!HasPending)
            {
                return;
            }

            if (Ready)
            {
                var (remove, add) = Plan(_wings);
                if (remove.Count > 0 || add.Count > 0)
                {
                    int steps = Context.CompList.UndoList.Count;
                    object top = steps > 0 ? Context.CompList.UndoList[0] : null;
                    Context.CompList.ReplaceTilesOnLayers(remove, add.Select(a => (a.Id, a.X, a.Y, a.Z, LayerFor(a.Layer))), ActionName, DefaultLayer);
                    Context.AfterEdit(add.Count > 0 ? add.Max(a => a.Z) : null);
                    object placedBy = Context.CompList.UndoList.Count > 0 ? Context.CompList.UndoList[0] : null;
                    if (placedBy != null && !ReferenceEquals(placedBy, top))
                    {
                        _placed = Capture();
                        _placedHistory = new List<State>(_undo);
                        PlacedBy = placedBy;
                        OnPlaced();
                    }

                    Committed?.Invoke();
                }
            }

            _wings.Clear();
            _selected = -1;
            _undo.Clear();
            _redo.Clear();
            ReopenedFrom = null;
            RestoreExtra(null);
            RaiseChanged();
        }

        /// <summary>Brings back the draft that <see cref="PlacedBy"/> placed, after that step was undone.</summary>
        public void Reopen()
        {
            if (_placed == null)
            {
                return;
            }

            Restore(_placed);
            _undo = new List<State>(_placedHistory);
            _redo.Clear();
            ReopenedFrom = PlacedBy;
            PlacedBy = null;
            RaiseChanged();
        }

        /// <summary>Lets go of a reopened draft because its placement is being redone in the multi.</summary>
        public void CloseReopened()
        {
            if (ReopenedFrom == null)
            {
                return;
            }

            _placed = Capture();
            _placedHistory = new List<State>(_undo);
            PlacedBy = ReopenedFrom;
            ReopenedFrom = null;
            _wings.Clear();
            _selected = -1;
            _undo.Clear();
            _redo.Clear();
            RestoreExtra(null);
            RaiseChanged();
        }

        /// <summary>Lays a draft in one go, as a press at one cell and a release at another would start it. For scripts and tests.</summary>
        public void Place(int startX, int startY, int endX, int endY, int z)
        {
            if (!Ready)
            {
                return;
            }

            _wings.AddRange(WingsFor(startX, startY, endX, endY, z, out _));
            Commit();
        }

        /// <summary>The wing under the mouse, the selected one first, and the cell pointed at on its plane.</summary>
        private int HitWing(out int cx, out int cy)
        {
            (cx, cy) = (0, 0);
            for (int pass = 0; pass < 2; ++pass)
            {
                for (int i = _wings.Count - 1; i >= 0; --i)
                {
                    if ((pass == 0) != (i == _selected))
                    {
                        continue;
                    }

                    RoofWing wing = _wings[i];
                    Context.PlaneCell(Context.MouseLoc, GripZ(wing), out cx, out cy);
                    if (cx >= wing.X0 && cx <= wing.X1 && cy >= wing.Y0 && cy <= wing.Y1)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private void DragWing()
        {
            if (_before == null || SelectedWing is not { } wing)
            {
                return;
            }

            Context.PlaneCell(Context.MouseLoc, GripZ(_before), out int cx, out int cy);
            int dx = cx - _grabX, dy = cy - _grabY;
            if (_drag == DragKind.Move)
            {
                dx = Math.Max(dx, -_before.X0);
                dy = Math.Max(dy, -_before.Y0);
                (wing.X0, wing.X1, wing.Y0, wing.Y1) = (_before.X0 + dx, _before.X1 + dx, _before.Y0 + dy, _before.Y1 + dy);
                Snap(wing, moved: true);
                return;
            }

            wing.X0 = _west ? Math.Clamp(_before.X0 + dx, 0, _before.X1) : _before.X0;
            wing.X1 = _east ? Math.Max(_before.X1 + dx, _before.X0) : _before.X1;
            wing.Y0 = _north ? Math.Clamp(_before.Y0 + dy, 0, _before.Y1) : _before.Y0;
            wing.Y1 = _south ? Math.Max(_before.Y1 + dy, _before.Y0) : _before.Y1;
            Snap(wing, moved: false);
            WingResized(wing);
        }

        // ---------------------------------------------------------------- overlay

        public override void PaintOverlay(IToolOverlay overlay)
        {
            PaintDraft(overlay, active: true);
        }

        /// <summary>
        /// Draws the draft as it would be built. With <paramref name="active"/> (this tool is in
        /// use) it also shows what the mouse would take hold of: the wing under it, the side of
        /// the selected wing a press would resize, and the wing being dragged out.
        /// </summary>
        public void PaintDraft(IToolOverlay overlay, bool active)
        {
            if (!Ready)
            {
                return;
            }

            var wings = new List<RoofWing>(_wings);
            List<RoofWing> stroke = null;
            if (active && _drag == DragKind.New)
            {
                stroke = WingsFor(_startX, _startY, _endX, _endY, _z, out _);
            }
            else if (active && !HasPending && PreviewsFirstClick)
            {
                StartCell(out int x, out int y, out int z);
                stroke = WingsFor(x, y, x, y, z, out _);
            }

            if (stroke != null)
            {
                wings.AddRange(stroke);
            }

            var (remove, add) = Plan(wings);
            foreach (MultiTile tile in remove)
            {
                overlay.Highlight(tile, TileHighlight.Remove);
            }

            // A house is looked into like the multi: storeys above the chosen floor are left out.
            int highest = GhostsFollowView ? Context.MaxHeight : int.MaxValue;
            foreach (var (id, x, y, z, layer) in add)
            {
                if (z <= highest && ShowsInPreview(layer))
                {
                    overlay.Ghost(id, x, y, z);
                }
            }

            PaintDetails(overlay, active);

            int hover = -1, hx = 0, hy = 0;
            if (active && _drag == DragKind.None && HasPending)
            {
                hover = HitWing(out hx, out hy);
            }

            for (int i = 0; i < _wings.Count; ++i)
            {
                RoofWing wing = _wings[i];
                int z = GripZ(wing);
                bool selected = i == _selected;
                if (selected && active)
                {
                    // The rows and columns a press would resize by; the one under the mouse stands out.
                    Edges(wing, hx, hy, hover == i, out bool west, out bool east, out bool north, out bool south);
                    if (_drag == DragKind.Resize)
                    {
                        (west, east, north, south) = (_west, _east, _north, _south);
                    }

                    for (int x = wing.X0; x <= wing.X1; ++x)
                    {
                        for (int y = wing.Y0; y <= wing.Y1; ++y)
                        {
                            bool band = x == wing.X0 || x == wing.X1 || y == wing.Y0 || y == wing.Y1;
                            bool held = (west && x == wing.X0) || (east && x == wing.X1) || (north && y == wing.Y0) || (south && y == wing.Y1);
                            if (held)
                            {
                                overlay.CellMark(x, y, z, OverlayTone.Hover);
                            }
                            else if (band)
                            {
                                overlay.CellMark(x, y, z, OverlayTone.Selected);
                            }
                        }
                    }
                }

                OverlayTone tone = selected ? OverlayTone.Selected : i == hover ? OverlayTone.Hover : OverlayTone.Plain;
                Point[] corners = BoxOutline(wing.X0, wing.Y0, wing.X1, wing.Y1, z);
                overlay.Outline(corners, tone);
                if (!selected || !active)
                {
                    continue;
                }

                // Grips in the middle of the selected wing's sides.
                for (int side = 0; side < 4; ++side)
                {
                    Point a = corners[side], b = corners[(side + 1) % 4];
                    var mid = new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                    overlay.Outline(new[]
                    {
                        new Point(mid.X - 5, mid.Y - 5), new Point(mid.X + 5, mid.Y - 5),
                        new Point(mid.X + 5, mid.Y + 5), new Point(mid.X - 5, mid.Y + 5)
                    }, OverlayTone.Selected);
                }
            }

            if (stroke != null && _drag == DragKind.New)
            {
                foreach (RoofWing wing in stroke)
                {
                    overlay.Outline(BoxOutline(wing.X0, wing.Y0, wing.X1, wing.Y1, GripZ(wing)), OverlayTone.Hover);
                }
            }
        }

        /// <summary>The corners of a box of cells on the plane at height z: top, right, bottom, left.</summary>
        protected static Point[] BoxOutline(int x0, int y0, int x1, int y1, int z)
        {
            return new[] { Corner(x0, y0, z), Corner(x1 + 1, y0, z), Corner(x1 + 1, y1 + 1, z), Corner(x0, y1 + 1, z) };
        }

        /// <summary>Canvas position of the top corner of a cell's diamond at height z.</summary>
        protected static Point Corner(int x, int y, int z)
        {
            return new Point(((x - y) * 22) + MultiEditorComponentList.GapXMod, ((x + y) * 22) - (z * 4) - 22);
        }
    }
}
