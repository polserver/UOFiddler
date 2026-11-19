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
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Editing;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Lays the session's floor pattern over a dragged rectangle on the work plane (or on top of
    /// what the mouse points at). Flat tiles already at that Z in those cells are replaced.
    /// </summary>
    internal sealed class FloorPatternTool : EditorTool
    {
        private readonly EditorSession _session;
        private bool _dragging;
        private int _z;
        private int _startX;
        private int _startY;
        private int _endX;
        private int _endY;

        public FloorPatternTool(EditorSession session) : base(session)
        {
            _session = session;
        }

        public override bool IsDragging => _dragging;

        public override bool TracksCursorCell => true;

        public override void OnMouseDown(MouseEventArgs e)
        {
            if (_session.FloorPattern == null)
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
            var (remove, add) = Plan(_startX, _startY, _endX, _endY, _z);
            if (remove.Count > 0 || add.Count > 0)
            {
                Context.CompList.ReplaceTiles(remove, add, "Floor pattern");
                Context.AfterEdit(_z);
            }
        }

        public override void Cancel()
        {
            _dragging = false;
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (_session.FloorPattern == null)
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

            var (remove, add) = Plan(sx, sy, ex, ey, z);
            foreach (MultiTile tile in remove)
            {
                overlay.Highlight(tile, TileHighlight.Remove);
            }

            foreach ((ushort id, int x, int y, int tz) in add)
            {
                overlay.Ghost(id, x, y, tz);
            }
        }

        private void BaseCell(out int x, out int y, out int z)
        {
            Context.CellAt(Context.MouseLoc, out x, out y, out z);
            if (Context.WorkZ is { } baseZ)
            {
                z = baseZ;
                Context.PlaneCell(Context.MouseLoc, z, out x, out y);
            }
        }

        private (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(int startX, int startY, int endX, int endY, int z)
        {
            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            MultiEditorComponentList list = Context.CompList;
            int x0 = Math.Min(startX, endX), x1 = Math.Max(startX, endX);
            int y0 = Math.Min(startY, endY), y1 = Math.Max(startY, endY);

            foreach ((ushort id, int x, int y) in _session.FloorPattern.Layout(x0, y0, x1, y1))
            {
                if (!list.InBounds(x, y))
                {
                    continue;
                }

                bool there = false;
                foreach (MultiTile tile in list.GetTilesAt(x, y))
                {
                    if (tile.Z != z || tile.Height != 0)
                    {
                        continue;
                    }

                    if (tile.Id == id)
                    {
                        there = true;
                    }
                    else
                    {
                        remove.Add(tile);
                    }
                }

                if (!there)
                {
                    add.Add((id, x, y, z));
                }
            }

            return (remove, add);
        }
    }
}
