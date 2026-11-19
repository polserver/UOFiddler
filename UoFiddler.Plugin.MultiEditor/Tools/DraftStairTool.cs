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

using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Stairs of a house draft: press on the cell of the bottom step and drag the way the stairs
    /// should climb (a click climbs north). They open the floor of the storey above.
    /// Ctrl+click on a bottom step takes the stairs out.
    /// </summary>
    internal sealed class DraftStairTool : DraftDragTool
    {
        public DraftStairTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        public override (int Width, int Height)? DragSize => null;

        protected override void Finish()
        {
            if (CtrlHeld)
            {
                House.RemoveStairs(Storey, StartX, StartY);
            }
            else
            {
                House.AddStairs(Storey, StartX, StartY, StairTool.Column(EndX - StartX, EndY - StartY), Context.HouseStyle);
            }
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            int x, y;
            if (Dragging)
            {
                (x, y) = (StartX, StartY);
            }
            else
            {
                MouseCell(out x, out y);
            }

            overlay.CellMark(x, y, StoreyZ, CtrlHeld ? OverlayTone.Remove : OverlayTone.Hover);
            if (!CtrlHeld && Context.HouseStyle is { Kind: HousePartKind.Stair } style)
            {
                string column = Dragging ? StairTool.Column(EndX - StartX, EndY - StartY) : "MultiNorth";
                foreach ((ushort id, int tx, int ty, int tz) in StairTool.Tiles(style, column, x, y, StoreyZ))
                {
                    overlay.Ghost(id, tx, ty, tz);
                }
            }
        }
    }
}
