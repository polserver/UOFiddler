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
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Adds the Z step to the hovered tile, or to the whole selection when several are selected.
    /// </summary>
    internal sealed class AltitudeTool : EditorTool
    {
        public AltitudeTool(IEditorContext context) : base(context)
        {
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            // Shift lowers.
            int step = (Control.ModifierKeys & Keys.Shift) != 0 ? -Context.ZStep : Context.ZStep;
            if (Context.Selection.Count > 1)
            {
                Context.CompList.TileZMod(Context.Selection, step);
                Context.AfterEdit();
            }
            else if (Context.HoverTile != null)
            {
                MultiTile tile = Context.HoverTile;
                Context.CompList.TileZMod(tile, step);
                Context.AfterEdit(tile.Z);
            }
        }
    }

    /// <summary>
    /// Makes the hovered tile the draw tile and returns to the draw tool used last.
    /// </summary>
    internal sealed class PipetteTool : EditorTool
    {
        public PipetteTool(IEditorContext context) : base(context)
        {
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (Context.HoverTile != null)
            {
                Context.PickDrawTile(Context.HoverTile.Id);
            }
        }
    }

    /// <summary>
    /// Shows the hovered tile see-through, to reach what is behind it. View only, not saved.
    /// </summary>
    internal sealed class TransparentTool : EditorTool
    {
        public TransparentTool(IEditorContext context) : base(context)
        {
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (Context.HoverTile != null)
            {
                Context.HoverTile.Transparent = !Context.HoverTile.Transparent;
            }
        }
    }
}
