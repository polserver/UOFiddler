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
using UoFiddler.Plugin.MultiEditor.House;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
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

        public override (int Width, int Height)? DragSize => _dragging ? (Math.Abs(_endX - _startX) + 1, Math.Abs(_endY - _startY) + 1) : null;

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
}
