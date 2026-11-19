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
    /// A canvas tool. The editor forwards left mouse button events and paints the overlay after
    /// the multi. Right button pans and cancels; middle button selects the hovered tile.
    /// </summary>
    internal abstract class EditorTool
    {
        protected EditorTool(IEditorContext context)
        {
            Context = context;
        }

        protected IEditorContext Context { get; }

        /// <summary>True while a drag is in progress, so every mouse move repaints.</summary>
        public virtual bool IsDragging => false;

        /// <summary>Cells a drag in progress spans, for the status bar; null when the tool has no such drag.</summary>
        public virtual (int Width, int Height)? DragSize => null;

        /// <summary>True when the overlay follows the cell under the cursor.</summary>
        public virtual bool TracksCursorCell => false;

        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        public virtual void PaintOverlay(IToolOverlay overlay)
        {
        }

        /// <summary>Drops a drag or pending action (Esc, right click, switching tools).</summary>
        public virtual void Cancel()
        {
        }

        /// <summary>True when new tiles replace tiles of the same kind at their Z.</summary>
        protected bool Replacing => Context.Placement == PlacementMode.Replace;

        protected static bool CtrlHeld => (Control.ModifierKeys & Keys.Control) != 0;

        protected static bool ShiftHeld => (Control.ModifierKeys & Keys.Shift) != 0;
    }
}
