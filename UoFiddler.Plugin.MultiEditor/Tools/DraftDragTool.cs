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
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// A draft edit tool used by dragging from one cell of the storey's floor to another.
    /// </summary>
    internal abstract class DraftDragTool : DraftEditTool
    {
        protected DraftDragTool(IEditorContext context, HouseTool house) : base(context, house)
        {
        }

        protected bool Dragging { get; private set; }

        protected int StartX { get; private set; }

        protected int StartY { get; private set; }

        protected int EndX { get; private set; }

        protected int EndY { get; private set; }

        public override bool IsDragging => Dragging;

        public override (int Width, int Height)? DragSize => Dragging ? (Math.Abs(EndX - StartX) + 1, Math.Abs(EndY - StartY) + 1) : null;

        public override void OnMouseDown(MouseEventArgs e)
        {
            MouseCell(out int x, out int y);
            (StartX, StartY, EndX, EndY) = (x, y, x, y);
            Dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (Dragging)
            {
                MouseCell(out int x, out int y);
                (EndX, EndY) = (x, y);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!Dragging)
            {
                return;
            }

            Dragging = false;
            MouseCell(out int x, out int y);
            (EndX, EndY) = (x, y);
            Finish();
        }

        public override void Cancel()
        {
            Dragging = false;
        }

        /// <summary>The drag ended at (<see cref="EndX"/>, <see cref="EndY"/>).</summary>
        protected abstract void Finish();
    }
}
