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

using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
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
