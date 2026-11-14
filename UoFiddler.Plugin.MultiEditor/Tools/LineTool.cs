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
using System.Drawing;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Drag a line of draw tiles. Shift keeps it on the main axis.
    /// </summary>
    internal sealed class LineTool : CellDragTool
    {
        public LineTool(IEditorContext context) : base(context)
        {
        }

        protected override void Commit(int z)
        {
            var (ex, ey) = LineEnd();
            Context.CompList.TileAddLine(StartX, StartY, ex, ey, z, Context.DrawTileId, Replacing);
            Context.RecordRecentTile(Context.DrawTileId);
            Context.AfterEdit(z);
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (!Dragging)
            {
                return;
            }

            Context.CellAt(Context.MouseLoc, out _, out _, out int z);
            var (ex, ey) = LineEnd();
            foreach (var (x, y) in MultiEditorComponentList.BresenhamLinePublic(StartX, StartY, ex, ey))
            {
                overlay.Ghost(Context.DrawTileId, x, y, z);
            }
        }

        /// <summary>
        /// The line end, kept on the dominant axis while Shift is held.
        /// </summary>
        private (int X, int Y) LineEnd()
        {
            if (!ShiftHeld)
            {
                return (EndX, EndY);
            }

            return Math.Abs(EndX - StartX) >= Math.Abs(EndY - StartY)
                ? (EndX, StartY)
                : (StartX, EndY);
        }
    }
}
