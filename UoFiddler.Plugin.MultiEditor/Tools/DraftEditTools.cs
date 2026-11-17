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
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Base for the tools that stand in for the wall, door, floor and stair tools while a house
    /// draft is open: they change the draft, on the storey of the chosen floor, instead of the
    /// multi. The draft is drawn under what the tool itself shows.
    /// </summary>
    internal abstract class DraftEditTool : EditorTool
    {
        protected DraftEditTool(IEditorContext context, HouseTool house) : base(context)
        {
            House = house;
        }

        protected HouseTool House { get; }

        public override bool TracksCursorCell => true;

        /// <summary>The storey worked on: the chosen floor's.</summary>
        protected int Storey => House.StoreyFor(Context.WorkZ);

        protected int StoreyZ => House.BaseZ + (Storey * HouseDraft.StoreyHeight);

        /// <summary>The cell under the mouse on the storey's floor.</summary>
        protected void MouseCell(out int x, out int y)
        {
            Context.PlaneCell(Context.MouseLoc, StoreyZ, out x, out y);
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            House.PaintDraft(overlay, active: false);
            PaintEdit(overlay);
        }

        protected abstract void PaintEdit(IToolOverlay overlay);
    }

    /// <summary>
    /// Walls of a house draft. A drag adds walls along the cells (a rectangle of them, or one
    /// straight wall with Shift or along a single row or column); Ctrl takes walls away, also
    /// outer ones, which leaves an opening. A click on a straight wall turns it into a window
    /// and back.
    /// </summary>
    internal sealed class DraftWallTool : DraftEditTool
    {
        private bool _dragging;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;

        public DraftWallTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        public override bool IsDragging => _dragging;

        public override void OnMouseDown(MouseEventArgs e)
        {
            MouseCell(out _startX, out _startY);
            (_endX, _endY) = (_startX, _startY);
            _dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                MouseCell(out _endX, out _endY);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            MouseCell(out _endX, out _endY);
            if (_startX == _endX && _startY == _endY && !CtrlHeld && House.ToggleWindow(Storey, _startX, _startY))
            {
                return;
            }

            var (south, east) = WallPlanner.DragEdges(_startX, _startY, _endX, _endY, ShiftHeld);
            House.EditEdges(Storey, south, east, CtrlHeld);
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            if (!_dragging)
            {
                MouseCell(out int x, out int y);
                overlay.CellMark(x, y, StoreyZ, OverlayTone.Hover);
                return;
            }

            OverlayTone tone = CtrlHeld ? OverlayTone.Remove : OverlayTone.Hover;
            var (south, east) = WallPlanner.DragEdges(_startX, _startY, _endX, _endY, ShiftHeld);
            var cells = new HashSet<(int X, int Y)>(south);
            cells.UnionWith(east);
            foreach ((int x, int y) in cells)
            {
                overlay.CellMark(x, y, StoreyZ, tone);
            }
        }
    }

    /// <summary>
    /// Doors of a house draft: a click on a straight wall hangs the chosen door there, a click on
    /// that door turns it to its next hinge variant, Ctrl+click takes the door out.
    /// </summary>
    internal sealed class DraftDoorTool : DraftEditTool
    {
        public DraftDoorTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            MouseCell(out int x, out int y);
            House.CycleDoor(Storey, x, y, Context.HouseStyle, CtrlHeld);
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            MouseCell(out int x, out int y);
            House.EdgesOf(Storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east);
            bool fits = south.Contains((x, y)) != east.Contains((x, y));
            overlay.CellMark(x, y, StoreyZ, CtrlHeld || !fits ? OverlayTone.Remove : OverlayTone.Hover);
        }
    }

    /// <summary>
    /// Floors of a house draft: a drag lays that area of the storey in the chosen floor style;
    /// the rest keeps the storey's own.
    /// </summary>
    internal sealed class DraftFloorTool : DraftEditTool
    {
        private bool _dragging;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;

        public DraftFloorTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        public override bool IsDragging => _dragging;

        public override void OnMouseDown(MouseEventArgs e)
        {
            MouseCell(out _startX, out _startY);
            (_endX, _endY) = (_startX, _startY);
            _dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                MouseCell(out _endX, out _endY);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            MouseCell(out _endX, out _endY);
            House.AddFloorArea(Storey, _startX, _startY, _endX, _endY, Context.HouseStyle);
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            int x0, y0, x1, y1;
            if (_dragging)
            {
                (x0, y0, x1, y1) = (Math.Min(_startX, _endX), Math.Min(_startY, _endY), Math.Max(_startX, _endX), Math.Max(_startY, _endY));
            }
            else
            {
                MouseCell(out x0, out y0);
                (x1, y1) = (x0, y0);
            }

            for (int x = x0; x <= x1; ++x)
            {
                for (int y = y0; y <= y1; ++y)
                {
                    overlay.CellMark(x, y, StoreyZ, OverlayTone.Hover);
                }
            }
        }
    }

    /// <summary>
    /// Stairs of a house draft: press on the cell of the bottom step and drag the way the stairs
    /// should climb (a click climbs north). They open the floor of the storey above.
    /// Ctrl+click on a bottom step takes the stairs out.
    /// </summary>
    internal sealed class DraftStairTool : DraftEditTool
    {
        private bool _dragging;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;

        public DraftStairTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        public override bool IsDragging => _dragging;

        public override void OnMouseDown(MouseEventArgs e)
        {
            MouseCell(out _startX, out _startY);
            (_endX, _endY) = (_startX, _startY);
            _dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                MouseCell(out _endX, out _endY);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            MouseCell(out _endX, out _endY);
            if (CtrlHeld)
            {
                House.RemoveStairs(Storey, _startX, _startY);
            }
            else
            {
                House.AddStairs(Storey, _startX, _startY, StairTool.Column(_endX - _startX, _endY - _startY), Context.HouseStyle);
            }
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            int x, y;
            if (_dragging)
            {
                (x, y) = (_startX, _startY);
            }
            else
            {
                MouseCell(out x, out y);
            }

            overlay.CellMark(x, y, StoreyZ, CtrlHeld ? OverlayTone.Remove : OverlayTone.Hover);
            if (!CtrlHeld && Context.HouseStyle is { Kind: HousePartKind.Stair } style)
            {
                string column = _dragging ? StairTool.Column(_endX - _startX, _endY - _startY) : "MultiNorth";
                foreach ((ushort id, int tx, int ty, int tz) in StairTool.Tiles(style, column, x, y, StoreyZ))
                {
                    overlay.Ghost(id, tx, ty, tz);
                }
            }
        }
    }
}
