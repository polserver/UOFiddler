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
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
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
}
