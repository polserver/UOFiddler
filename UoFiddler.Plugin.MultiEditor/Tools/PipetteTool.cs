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
}
