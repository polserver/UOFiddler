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
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Click removes the hovered tile. Dragging removes every shown tile in the rectangle between
    /// the work plane (once a floor is chosen or it was moved) and the height slider.
    /// </summary>
    internal sealed class RemoveTool : CellDragTool
    {
        public RemoveTool(IEditorContext context) : base(context)
        {
        }

        private int MinZ => Context.WorkZ ?? sbyte.MinValue;

        protected override void Commit(int z)
        {
            if (EndX != StartX || EndY != StartY)
            {
                var (x0, y0, x1, y1) = Bounds;
                Context.CompList.TileRemoveRect(x0, y0, x1, y1, MinZ, Context.MaxHeight);
            }
            else if (Context.HoverTile != null)
            {
                Context.CompList.TileRemove(Context.HoverTile);
            }

            Context.AfterEdit();
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (!Dragging)
            {
                return;
            }

            var (x0, y0, x1, y1) = Bounds;
            for (int x = x0; x <= x1; ++x)
            {
                for (int y = y0; y <= y1; ++y)
                {
                    foreach (MultiTile tile in Context.CompList.GetTilesAt(x, y))
                    {
                        if (tile.Z >= MinZ && tile.Z <= Context.MaxHeight && Context.CompList.IsShown(tile))
                        {
                            overlay.Highlight(tile, TileHighlight.Remove);
                        }
                    }
                }
            }
        }
    }
}
