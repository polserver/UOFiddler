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

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Floors of a house draft: a drag lays that area of the storey in the chosen floor style;
    /// the rest keeps the storey's own.
    /// </summary>
    internal sealed class DraftFloorTool : DraftDragTool
    {
        public DraftFloorTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        protected override void Finish()
        {
            House.AddFloorArea(Storey, StartX, StartY, EndX, EndY, Context.HouseStyle);
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            int x0, y0, x1, y1;
            if (Dragging)
            {
                (x0, y0, x1, y1) = (Math.Min(StartX, EndX), Math.Min(StartY, EndY), Math.Max(StartX, EndX), Math.Max(StartY, EndY));
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
