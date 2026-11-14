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
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Places the draw tile on the hovered tile or the virtual floor, with the chosen brush size.
    /// </summary>
    internal sealed class DrawTool : EditorTool
    {
        public DrawTool(IEditorContext context) : base(context)
        {
        }

        public override bool TracksCursorCell => true;

        public override void OnMouseUp(MouseEventArgs e)
        {
            Context.CellAt(Context.MouseLoc, out int x, out int y, out int z);
            if (!Context.CompList.InBounds(x, y))
            {
                return;
            }

            if (Context.BrushSize <= 1)
            {
                Context.CompList.TileAdd(x, y, z, Context.DrawTileId, Replacing);
            }
            else
            {
                Context.CompList.TileAddBrush(x, y, z, Context.DrawTileId, Context.BrushSize, Replacing);
            }

            Context.RecordRecentTile(Context.DrawTileId);
            Context.AfterEdit(z);
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            Context.CellAt(Context.MouseLoc, out int x, out int y, out int z);
            if (!Context.CompList.InBounds(x, y))
            {
                return;
            }

            int half = Context.BrushSize / 2;
            for (int dx = -half; dx <= half; dx++)
            {
                for (int dy = -half; dy <= half; dy++)
                {
                    overlay.Ghost(Context.DrawTileId, x + dx, y + dy, z);
                }
            }
        }
    }
}
