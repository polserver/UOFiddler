/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// Selects the tiles whose cell centre lies inside a freehand outline. Ctrl adds to the selection.
    /// </summary>
    internal sealed class LassoTool : EditorTool
    {
        private readonly List<Point> _points = new List<Point>();
        private bool _drawing;

        public LassoTool(IEditorContext context) : base(context)
        {
        }

        public override bool IsDragging => _drawing;

        public override void OnMouseDown(MouseEventArgs e)
        {
            _points.Clear();
            _points.Add(Context.MouseLoc);
            _drawing = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (!_drawing)
            {
                return;
            }

            Point last = _points[^1];
            Point now = Context.MouseLoc;
            if (Math.Abs(now.X - last.X) + Math.Abs(now.Y - last.Y) >= 3)
            {
                _points.Add(now);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (!_drawing)
            {
                return;
            }

            _drawing = false;
            if (_points.Count < 3)
            {
                // A click, not an outline: select the tile under it like the Select tool.
                Context.SetSelection(Context.HoverTile != null ? new[] { Context.HoverTile } : Array.Empty<MultiTile>(), CtrlHeld);
                return;
            }

            int left = int.MaxValue, top = int.MaxValue, right = int.MinValue, bottom = int.MinValue;
            foreach (Point point in _points)
            {
                left = Math.Min(left, point.X);
                top = Math.Min(top, point.Y);
                right = Math.Max(right, point.X);
                bottom = Math.Max(bottom, point.Y);
            }

            var inside = new List<MultiTile>();
            foreach (MultiTile tile in Context.TilesInRect(Rectangle.FromLTRB(left, top, right + 1, bottom + 1)))
            {
                PointF anchor = ((EditorSession)Context).TileAnchor(tile);
                if (Contains(anchor.X, anchor.Y))
                {
                    inside.Add(tile);
                }
            }

            Context.SetSelection(inside, CtrlHeld);
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (_drawing && _points.Count > 1)
            {
                overlay.Polyline(_points, closed: true);
            }
        }

        public override void Cancel()
        {
            _drawing = false;
            _points.Clear();
        }

        /// <summary>Even-odd test of a point against the outline.</summary>
        private bool Contains(float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = _points.Count - 1; i < _points.Count; j = i++)
            {
                Point a = _points[i];
                Point b = _points[j];
                if ((a.Y > y) != (b.Y > y) && x < ((b.X - a.X) * (y - a.Y) / (float)(b.Y - a.Y)) + a.X)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
