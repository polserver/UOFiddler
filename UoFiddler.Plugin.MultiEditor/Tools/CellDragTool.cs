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
    /// Shared drag state for tools that span two cells (rectangle, line, area remove).
    /// </summary>
    internal abstract class CellDragTool : EditorTool
    {
        private bool _dragging;

        protected CellDragTool(IEditorContext context) : base(context)
        {
        }

        protected int StartX { get; private set; }

        protected int StartY { get; private set; }

        protected int EndX { get; private set; }

        protected int EndY { get; private set; }

        public override bool IsDragging => _dragging;

        public override (int Width, int Height)? DragSize => _dragging ? (System.Math.Abs(EndX - StartX) + 1, System.Math.Abs(EndY - StartY) + 1) : null;

        protected bool Dragging => _dragging;

        public override void OnMouseDown(MouseEventArgs e)
        {
            Context.CellAt(Context.MouseLoc, out int x, out int y, out _);
            StartX = EndX = x;
            StartY = EndY = y;
            _dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            Context.CellAt(Context.MouseLoc, out int x, out int y, out _);
            EndX = x;
            EndY = y;
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            Context.CellAt(Context.MouseLoc, out int x, out int y, out int z);
            EndX = x;
            EndY = y;
            Commit(z);
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        protected abstract void Commit(int z);

        protected (int X0, int Y0, int X1, int Y1) Bounds =>
            (System.Math.Min(StartX, EndX), System.Math.Min(StartY, EndY), System.Math.Max(StartX, EndX), System.Math.Max(StartY, EndY));
    }
}
