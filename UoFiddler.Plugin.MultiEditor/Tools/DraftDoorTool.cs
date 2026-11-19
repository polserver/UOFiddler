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
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Doors of a house draft: a click on a straight wall hangs the chosen door there, a click on
    /// that door turns it to its next hinge variant, Ctrl+click takes the door out.
    /// </summary>
    internal sealed class DraftDoorTool : DraftEditTool
    {
        public DraftDoorTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            MouseCell(out int x, out int y);
            House.CycleDoor(Storey, x, y, Context.HouseStyle, CtrlHeld);
        }

        protected override void PaintEdit(IToolOverlay overlay)
        {
            MouseCell(out int x, out int y);
            House.EdgesOf(Storey, out HashSet<(int X, int Y)> south, out HashSet<(int X, int Y)> east);
            bool fits = south.Contains((x, y)) != east.Contains((x, y));
            overlay.CellMark(x, y, StoreyZ, CtrlHeld || !fits ? OverlayTone.Remove : OverlayTone.Hover);
        }
    }
}
