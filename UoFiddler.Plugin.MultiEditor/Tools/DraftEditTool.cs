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

using UoFiddler.Plugin.MultiEditor.House;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Base for the tools that stand in for the wall, door, floor and stair tools while a house
    /// draft is open: they change the draft, on the storey of the chosen floor, instead of the
    /// multi. The draft is drawn under what the tool itself shows.
    /// </summary>
    internal abstract class DraftEditTool : EditorTool
    {
        protected DraftEditTool(IEditorContext context, HouseTool house) : base(context)
        {
            House = house;
        }

        protected HouseTool House { get; }

        public override bool TracksCursorCell => true;

        /// <summary>The storey worked on: the chosen floor's.</summary>
        protected int Storey => House.StoreyFor(Context.WorkZ);

        protected int StoreyZ => House.BaseZ + (Storey * HouseDraft.StoreyHeight);

        /// <summary>The cell under the mouse on the storey's floor.</summary>
        protected void MouseCell(out int x, out int y)
        {
            Context.PlaneCell(Context.MouseLoc, StoreyZ, out x, out y);
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            House.PaintDraft(overlay, active: false);
            PaintEdit(overlay);
        }

        protected abstract void PaintEdit(IToolOverlay overlay);
    }
}
