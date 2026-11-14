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
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Base for the House tab tools. A press picks a cell and a base Z; dragging follows the cell
    /// under the mouse on that Z's plane. The preview shows exactly what releasing would change.
    /// </summary>
    internal abstract class PlaneDragTool : EditorTool
    {
        private bool _dragging;
        private int _z;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;

        protected PlaneDragTool(IEditorContext context) : base(context)
        {
        }

        public override bool IsDragging => _dragging;

        public override bool TracksCursorCell => true;

        /// <summary>The House tab style this tool works with, when it is the chosen one.</summary>
        protected abstract HousePartKind Kind { get; }

        protected HouseStyle Style => Context.HouseStyle?.Kind == Kind ? Context.HouseStyle : null;

        /// <summary>The edit's name in the undo list.</summary>
        protected abstract string ActionName { get; }

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (Style == null)
            {
                return;
            }

            BaseCell(out _startX, out _startY, out _z);
            _endX = _startX;
            _endY = _startY;
            _dragging = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragging)
            {
                Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            Context.PlaneCell(Context.MouseLoc, _z, out _endX, out _endY);
            var (remove, add) = Plan(_startX, _startY, _endX, _endY, _z, final: true);
            if (remove.Count > 0 || add.Count > 0)
            {
                Context.CompList.ReplaceTiles(remove, add, ActionName);
                Context.AfterEdit(_z);
            }
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (Style == null)
            {
                return;
            }

            int sx, sy, ex, ey, z;
            if (_dragging)
            {
                (sx, sy, ex, ey, z) = (_startX, _startY, _endX, _endY, _z);
            }
            else
            {
                BaseCell(out sx, out sy, out z);
                (ex, ey) = (sx, sy);
            }

            var (remove, add) = Plan(sx, sy, ex, ey, z, final: false);
            foreach (MultiTile tile in remove)
            {
                overlay.Highlight(tile, TileHighlight.Remove);
            }

            foreach (var (id, x, y, tz) in add)
            {
                overlay.Ghost(id, x, y, tz);
            }
        }

        /// <summary>
        /// What releasing the mouse would change. <paramref name="final"/> is true for the real edit.
        /// </summary>
        protected abstract (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final);

        /// <summary>
        /// The cell under the mouse and the base Z: the work plane, the hovered tall tile's own Z
        /// (to stay on its level), or the top of the hovered flat tile.
        /// </summary>
        protected virtual void BaseCell(out int x, out int y, out int z)
        {
            Context.CellAt(Context.MouseLoc, out x, out y, out z);
            if (Context.WorkZ is { } baseZ)
            {
                z = baseZ;
                Context.PlaneCell(Context.MouseLoc, z, out x, out y);
            }
            else if (Context.HoverTile is { } hover && hover.Height >= 10)
            {
                z = hover.Z;
            }
        }
    }
}
