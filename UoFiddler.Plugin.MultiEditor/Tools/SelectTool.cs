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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Click selects the hovered tile, Ctrl+click toggles it, dragging over empty space selects
    /// every tile the box touches (Ctrl+drag adds to the selection). Dragging a selected tile
    /// moves the whole selection.
    /// </summary>
    internal sealed class SelectTool : EditorTool
    {
        private bool _marquee;
        private Point _start;
        private Point _end;

        private MultiTile _moveAnchor;
        private int _moveDx;
        private int _moveDy;

        public SelectTool(IEditorContext context) : base(context)
        {
        }

        public override bool IsDragging => _marquee || _moveAnchor != null;

        public override void OnMouseDown(MouseEventArgs e)
        {
            MultiTile hover = Context.HoverTile;
            if (hover != null && !CtrlHeld && Context.Selection.Contains(hover))
            {
                _moveAnchor = hover;
                _start = Context.MouseLoc;
                _moveDx = _moveDy = 0;
            }
            else if (hover == null)
            {
                // With Ctrl the box adds to the selection when it is let go.
                _marquee = true;
                _start = _end = Context.MouseLoc;
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (_moveAnchor != null)
            {
                // Measured on the anchor tile's own height, so the tile stays under the cursor.
                Context.PlaneCell(_start, _moveAnchor.Z, out int x0, out int y0);
                Context.PlaneCell(Context.MouseLoc, _moveAnchor.Z, out int x1, out int y1);
                _moveDx = x1 - x0;
                _moveDy = y1 - y0;
            }
            else if (_marquee)
            {
                // Same zoom-aware space as the tiles, so the rectangle selects what it covers.
                _end = Context.MouseLoc;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            MultiTile hover = Context.HoverTile;
            MultiTile[] hovered = hover != null ? new[] { hover } : Array.Empty<MultiTile>();

            if (_moveAnchor != null)
            {
                _moveAnchor = null;
                if (_moveDx != 0 || _moveDy != 0)
                {
                    Context.CompList.TileMoveBatch(Context.Selection.ToList(), _moveDx, _moveDy, 0, "Move Selection");
                    Context.AfterEdit();
                    return;
                }

                // No movement: an ordinary click on a selected tile.
                Context.SetSelection(hovered, add: false);
            }
            else if (_marquee)
            {
                _marquee = false;
                Rectangle rect = MarqueeRect();
                if (rect.Width > 3 && rect.Height > 3)
                {
                    Context.SetSelection(
                        Context.TilesInRect(rect),
                        add: CtrlHeld);
                }
                else
                {
                    // Tiny drag: a plain click.
                    Context.SetSelection(hovered, add: CtrlHeld);
                }
            }
            else if (CtrlHeld && hover != null)
            {
                Context.ToggleSelected(hover);
            }
            else
            {
                Context.SetSelection(hovered, add: false);
            }
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (_moveAnchor != null && (_moveDx != 0 || _moveDy != 0))
            {
                foreach (MultiTile tile in Context.Selection)
                {
                    overlay.Ghost(tile.Id, tile.X + _moveDx, tile.Y + _moveDy, tile.Z);
                }

                return;
            }

            if (!_marquee)
            {
                return;
            }

            overlay.Marquee(MarqueeRect());
        }

        public override void Cancel()
        {
            _marquee = false;
            _moveAnchor = null;
        }

        private Rectangle MarqueeRect()
        {
            return new Rectangle(
                Math.Min(_start.X, _end.X) - Context.ScrollX,
                Math.Min(_start.Y, _end.Y) - Context.ScrollY,
                Math.Abs(_end.X - _start.X),
                Math.Abs(_end.Y - _start.Y));
        }
    }
}
