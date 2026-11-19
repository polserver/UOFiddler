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

namespace UoFiddler.Plugin.MultiEditor.Tools
{
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
