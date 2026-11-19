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
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Base for the tools that shape something out of wings before it is placed (roofs, houses).
    /// The first stroke starts a draft, shown as it will be built. Then: click a wing to select
    /// it, drag it to move it, drag an outermost row or column of the selected wing to resize
    /// it, drag anywhere else to add a wing, Shift+click to turn one, Delete to remove one.
    /// A copied or duplicated wing follows the mouse until a click drops it.
    /// Nothing is written to the multi until <see cref="Commit"/>; <see cref="Discard"/> drops
    /// the draft. Every change to the draft is a step of its own undo history, which
    /// <see cref="UndoDraft"/> and <see cref="RedoDraft"/> walk; dropping the draft is such a
    /// step too, and undoing a placement in the multi brings the draft back with its history.
    /// </summary>
    internal abstract partial class WingDraftTool : EditorTool
    {
        private const int _historySize = 200;

        private enum DragKind
        {
            None,
            New,
            Move,
            Resize,

            /// <summary>A pasted wing following the mouse until a click drops it.</summary>
            Float
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
        /// <summary>Least distance between a wing's first and last row or column when it is resized. A roof may be a single row.</summary>
        protected virtual int MinSpan => 0;

        private RoofWing _before;

        // A Ctrl+drag moves every wing: where they all stood, and how far they have been moved so far.
        private List<RoofWing> _beforeAll;
        private (int X, int Y) _movedAll;
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
            DragKind.Resize or DragKind.Move or DragKind.Float when SelectedWing is { } wing => (wing.X1 - wing.X0 + 1, wing.Y1 - wing.Y0 + 1),
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

        /// <summary>
        /// One wing is being moved: it stood as <paramref name="before"/> and is now this far
        /// from there. <paramref name="extraAtStart"/> is what the draft kept besides its wings
        /// when the drag began, so the move is always made from there, not step upon step.
        /// </summary>
        protected virtual void WingMoved(RoofWing before, int dx, int dy, object extraAtStart)
        {
        }

        /// <summary>Every wing was moved by this many cells: what a draft keeps besides its wings moves along.</summary>
        protected virtual void DraftMoved(int dx, int dy)
        {
        }

        /// <summary>What a copy of this wing takes along besides the wing itself.</summary>
        protected virtual object CopyWingExtra(RoofWing wing) => null;

        /// <summary>
        /// A pasted wing stands this far from the wing it was copied from: what came with it is
        /// laid there, onto <paramref name="extraAtStart"/>, what the draft kept before the paste.
        /// </summary>
        protected virtual void WingPasted(object extraAtStart, object copied, int dx, int dy)
        {
        }

        /// <summary>The Z a pasted wing stands at.</summary>
        protected virtual int PastedWingZ(RoofWing copy) => copy.EaveZ;

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
            StepPushed();
        }

        /// <summary>Called when a step was added to the draft's history.</summary>
        protected virtual void StepPushed()
        {
        }

        protected void RaiseChanged() => Changed?.Invoke();

        public bool UndoDraft()
        {
            if (_drag == DragKind.Float)
            {
                // A pasted wing not dropped yet: undo takes back only the paste.
                CancelDrag();
                RaiseChanged();
                return true;
            }

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

            if (_drag == DragKind.Float)
            {
                // A click drops the pasted wing, as one step of the draft's history. A right click never gets here: the view cancels the tool.
                DropFloat();
                RaiseChanged();
                return;
            }

            if (HasPending)
            {
                int hit = HitWing(out int cx, out int cy);
                if (hit >= 0)
                {
                    RoofWing wing = _wings[hit];
                    bool wasSelected = hit == _selected;

                    // Shift on a corner of the selected wing resizes it as a square; anywhere else on a wing it turns the wing.
                    Edges(wing, cx, cy, wasSelected, out bool cornerWest, out bool cornerEast, out bool cornerNorth, out bool cornerSouth);
                    bool corner = (cornerWest || cornerEast) && (cornerNorth || cornerSouth);
                    if (ShiftHeld && !corner)
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

                    // With Ctrl the whole draft moves, not the one wing.
                    _beforeAll = null;
                    if (CtrlHeld)
                    {
                        _drag = DragKind.Move;
                        _beforeAll = _wings.Select(w => w.Clone()).ToList();
                        _movedAll = (0, 0);
                    }

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
        /// columns, however narrow the wing is, so a wing can always be made wider again.
        /// </summary>
        private static void Edges(RoofWing wing, int cx, int cy, bool selected, out bool west, out bool east, out bool north, out bool south)
        {
            west = selected && cx == wing.X0;
            east = selected && cx == wing.X1;
            north = selected && cy == wing.Y0;
            south = selected && cy == wing.Y1;

            // A wing of one row or column has both sides in the same cell: the far side is the one to pull out again.
            west &= !east;
            north &= !south;
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
                case DragKind.Float:
                    DragWing();
                    break;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            DragKind drag = _drag;
            if (drag == DragKind.Float)
            {
                // A pasted wing stays on the mouse until a click on the view.
                return;
            }

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
            bool floating = _drag == DragKind.Float;
            CancelDrag();
            if (floating)
            {
                // The pasted wing is gone and the selection is back to what it was.
                RaiseChanged();
            }
        }

        private void CancelDrag()
        {
            if (_drag == DragKind.Float && _dragStart != null)
            {
                // A pasted wing not dropped yet: the draft is as it was before the paste.
                Restore(_dragStart);
                EndFloat();
                return;
            }

            if (_drag == DragKind.Move && _beforeAll != null && _beforeAll.Count == _wings.Count)
            {
                // The whole draft was being moved: all of it goes back.
                for (int i = 0; i < _wings.Count; ++i)
                {
                    _wings[i].CopyFrom(_beforeAll[i]);
                }

                DraftMoved(-_movedAll.X, -_movedAll.Y);
            }
            else if (_drag is DragKind.Move or DragKind.Resize && _before != null && SelectedWing is { } wing)
            {
                wing.CopyFrom(_before);
                if (_drag == DragKind.Move)
                {
                    WingMoved(_before, 0, 0, _dragStart?.Extra);
                }
            }

            _beforeAll = null;

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
            if (_drag == DragKind.Float)
            {
                // A pasted wing still on the mouse is placed with the rest, where it is shown.
                DropFloat();
            }

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
            if (_drag == DragKind.Move && _beforeAll != null && _beforeAll.Count == _wings.Count)
            {
                dx = Math.Max(dx, -_beforeAll.Min(w => w.X0));
                dy = Math.Max(dy, -_beforeAll.Min(w => w.Y0));
                for (int i = 0; i < _wings.Count; ++i)
                {
                    RoofWing was = _beforeAll[i];
                    (_wings[i].X0, _wings[i].X1, _wings[i].Y0, _wings[i].Y1) = (was.X0 + dx, was.X1 + dx, was.Y0 + dy, was.Y1 + dy);
                }

                if ((dx, dy) != _movedAll)
                {
                    DraftMoved(dx - _movedAll.X, dy - _movedAll.Y);
                    _movedAll = (dx, dy);
                }

                return;
            }

            if (_drag is DragKind.Move or DragKind.Float)
            {
                dx = Math.Max(dx, -_before.X0);
                dy = Math.Max(dy, -_before.Y0);
                (wing.X0, wing.X1, wing.Y0, wing.Y1) = (_before.X0 + dx, _before.X1 + dx, _before.Y0 + dy, _before.Y1 + dy);
                Snap(wing, moved: true);
                if (_drag == DragKind.Float)
                {
                    WingPasted(_dragStart?.Extra, _floatExtra, wing.X0 - _floatSource.X0, wing.Y0 - _floatSource.Y0);
                }
                else
                {
                    WingMoved(_before, wing.X0 - _before.X0, wing.Y0 - _before.Y0, _dragStart?.Extra);
                }

                return;
            }

            // Not narrower than the tool lets a wing be.
            wing.X0 = _west ? Math.Clamp(_before.X0 + dx, 0, Math.Max(0, _before.X1 - MinSpan)) : _before.X0;
            wing.X1 = _east ? Math.Max(_before.X1 + dx, _before.X0 + MinSpan) : _before.X1;
            wing.Y0 = _north ? Math.Clamp(_before.Y0 + dy, 0, Math.Max(0, _before.Y1 - MinSpan)) : _before.Y0;
            wing.Y1 = _south ? Math.Max(_before.Y1 + dy, _before.Y0 + MinSpan) : _before.Y1;

            // A corner dragged with Shift makes the wing square: the side dragged further sets the size.
            // (Keeping a ratio does not work on a grid of cells: the two sides round apart.)
            bool square = ShiftHeld && (_west || _east) && (_north || _south);
            if (square)
            {
                int side = Math.Max(wing.X1 - wing.X0, wing.Y1 - wing.Y0);

                // Not past the multi's first row or column, and not smaller than a wing can be.
                if (_west)
                {
                    side = Math.Min(side, _before.X1);
                }

                if (_north)
                {
                    side = Math.Min(side, _before.Y1);
                }

                side = Math.Max(side, 2);
                (wing.X0, wing.X1) = _west ? (_before.X1 - side, _before.X1) : (_before.X0, _before.X0 + side);
                (wing.Y0, wing.Y1) = _north ? (_before.Y1 - side, _before.Y1) : (_before.Y0, _before.Y0 + side);
            }

            // Snapping to a wing one cell away would stretch one side by a cell: a square stays as dragged.
            if (!square)
            {
                Snap(wing, moved: false);
            }

            WingResized(wing);
        }
    }
}
