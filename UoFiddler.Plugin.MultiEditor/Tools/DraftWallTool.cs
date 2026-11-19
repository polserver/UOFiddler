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

using System.Collections.Generic;
using UoFiddler.Plugin.MultiEditor.House;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Walls of a house draft. A drag adds walls along the cells (a rectangle of them, or one
    /// straight wall with Shift or along a single row or column); Ctrl takes walls away, also
    /// outer ones, which leaves an opening. A click on a straight wall turns it into a window
    /// and back.
    /// </summary>
    internal sealed class DraftWallTool : DraftDragTool
    {
        public DraftWallTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        protected override void Finish()
        {
            if (StartX == EndX && StartY == EndY && !CtrlHeld && House.ToggleWindow(Storey, StartX, StartY))
            {
                return;
            }

            var (south, east) = WallPlanner.DragEdges(StartX, StartY, EndX, EndY, ShiftHeld);
            House.EditEdges(Storey, south, east, CtrlHeld);
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            if (!Dragging)
            {
                MouseCell(out int x, out int y);
                overlay.CellMark(x, y, StoreyZ, OverlayTone.Hover);
                return;
            }

            OverlayTone tone = CtrlHeld ? OverlayTone.Remove : OverlayTone.Hover;
            var (south, east) = WallPlanner.DragEdges(StartX, StartY, EndX, EndY, ShiftHeld);
            var cells = new HashSet<(int X, int Y)>(south);
            cells.UnionWith(east);
            foreach ((int x, int y) in cells)
            {
                overlay.CellMark(x, y, StoreyZ, tone);
            }
        }
    }
}
