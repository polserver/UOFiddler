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

using System.Drawing;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Drag a rectangle to fill it with the draw tile.
    /// </summary>
    internal sealed class RectFillTool : CellDragTool
    {
        public RectFillTool(IEditorContext context) : base(context)
        {
        }

        protected override void Commit(int z)
        {
            var (x0, y0, x1, y1) = Bounds;
            Context.CompList.TileAddRect(x0, y0, x1, y1, z, Context.DrawTileId, Replacing);
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
            var (x0, y0, x1, y1) = Bounds;
            for (int x = x0; x <= x1; x++)
            {
                for (int y = y0; y <= y1; y++)
                {
                    overlay.Ghost(Context.DrawTileId, x, y, z);
                }
            }
        }
    }
}
