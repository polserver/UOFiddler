/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Rendering;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    internal sealed partial class EditorSession
    {
        // ---------------------------------------------------------------- selection and hover

        public MultiTile HoverTile { get; private set; }

        public IReadOnlyCollection<MultiTile> Selection => _selection;

        /// <summary>The selected tile when exactly one is selected.</summary>
        public MultiTile SelectedTile => _selection.Count == 1 ? _selection.First() : null;

        public bool IsSelected(MultiTile tile) => _selection.Contains(tile);

        public (int X, int Y, int Z) CursorCell => _cursorCell;

        public bool MouseInside => _mouseInside;

        public void SetSelection(IEnumerable<MultiTile> tiles, bool add)
        {
            if (!add)
            {
                _selection.Clear();
            }

            foreach (MultiTile tile in tiles)
            {
                _selection.Add(tile);
            }

            SelectionChanged?.Invoke();
            Invalidated?.Invoke();
        }

        public void ToggleSelected(MultiTile tile)
        {
            if (!_selection.Remove(tile))
            {
                _selection.Add(tile);
            }

            SelectionChanged?.Invoke();
            Invalidated?.Invoke();
        }

        public void ClearSelection()
        {
            if (_selection.Count == 0)
            {
                return;
            }

            _selection.Clear();
            SelectionChanged?.Invoke();
            Invalidated?.Invoke();
        }

        /// <summary>Takes tiles out of the selection, with one change notice.</summary>
        public void Deselect(IEnumerable<MultiTile> tiles)
        {
            bool changed = false;
            foreach (MultiTile tile in tiles)
            {
                changed |= _selection.Remove(tile);
            }

            if (changed)
            {
                SelectionChanged?.Invoke();
                Invalidated?.Invoke();
            }
        }

        private HashSet<MultiTile> _listHighlight = new HashSet<MultiTile>();

        /// <summary>
        /// Tiles pointed at outside the canvas (a row of the selection list), drawn like the hovered
        /// tile. Empty for none.
        /// </summary>
        public IReadOnlyCollection<MultiTile> ListHighlight
        {
            get => _listHighlight;
            set
            {
                var next = value == null ? new HashSet<MultiTile>() : new HashSet<MultiTile>(value);
                if (next.SetEquals(_listHighlight))
                {
                    return;
                }

                _listHighlight = next;
                Invalidated?.Invoke();
            }
        }

        public bool IsListHighlighted(MultiTile tile) => _listHighlight.Count > 0 && _listHighlight.Contains(tile);

        /// <summary>True when a tile can be hovered, selected and edited with the current view.</summary>
        public bool IsEditable(MultiTile tile)
        {
            return tile.Z <= EditMaxHeight && !(WorkZ is { } baseZ && baseZ > tile.Z) && CompList.IsShown(tile);
        }

        /// <summary>The topmost editable tile with a drawn pixel at a world point, or null.</summary>
        public MultiTile HitTest(Point world)
        {
            if (_planMode)
            {
                (int cx, int cy) = PlanView.CellAt(world);
                return CompList.InBounds(cx, cy) ? Plan.TopTile(cx, cy, EditMaxHeight, EditMinZ) : null;
            }

            MultiTile hit = null;

            // Draw order: the last match is the one on top. Runs on every mouse move over every
            // tile, so the point is first checked against the art's rectangle, which rules out
            // almost all of them without a lookup.
            foreach (MultiTile tile in CompList.RealTiles)
            {
                int px = world.X - tile.XMod;
                int py = world.Y - tile.YMod;
                if ((uint)px >= (uint)tile.ArtWidth || (uint)py >= (uint)tile.ArtHeight)
                {
                    continue;
                }

                if (IsEditable(tile) && _masks.Hit(tile.Id, px, py))
                {
                    hit = tile;
                }
            }

            return hit;
        }

        /// <summary>How long the last hover update's hit test took, for the performance box.</summary>
        public double LastHitTestMs { get; private set; }

        private bool UpdateHover()
        {
            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            MultiTile hover = _mouseInside && HasDocument && !_previewMode ? HitTest(_mouseLoc) : null;
            LastHitTestMs = System.Diagnostics.Stopwatch.GetElapsedTime(start).TotalMilliseconds;
            if (hover == HoverTile)
            {
                return false;
            }

            HoverTile = hover;
            HoverChanged?.Invoke();
            return true;
        }

        private bool UpdateCursorCell()
        {
            if (!_mouseInside || !HasDocument)
            {
                return false;
            }

            ConvertCoords(_mouseLoc, out int x, out int y, out int z);
            if ((x, y, z) == _cursorCell)
            {
                return false;
            }

            _cursorCell = (x, y, z);
            CursorCellChanged?.Invoke();
            return true;
        }

        // ---------------------------------------------------------------- pointer input (world space)

        public void PointerMove(Point world, MouseEventArgs e)
        {
            _mouseLoc = world;
            _mouseInside = true;
            if (!HasDocument || _previewMode)
            {
                return;
            }

            // Hover first: tools read the cell under the mouse, which depends on the hovered tile.
            bool hoverChanged = UpdateHover();
            bool cellChanged = UpdateCursorCell();
            EditorTool tool = ActiveTool;
            tool.OnMouseMove(e);
            if (hoverChanged || tool.IsDragging || (cellChanged && tool.TracksCursorCell))
            {
                Invalidated?.Invoke();
            }
        }

        public void PointerLeave()
        {
            _mouseInside = false;
            _cursorCell = (int.MinValue, int.MinValue, int.MinValue);
            UpdateHover();
            CursorCellChanged?.Invoke();
            Invalidated?.Invoke();
        }

        public void PointerDown(Point world, MouseEventArgs e)
        {
            _mouseLoc = world;
            _mouseInside = true;
            if (HasDocument && !_previewMode && e.Button == MouseButtons.Left)
            {
                UpdateHover();
                UpdateCursorCell();

                // Alt+click with a draw tool picks a tile: the tool does not see the click.
                _pickClick = PickingWithAlt;
                if (!_pickClick)
                {
                    ActiveTool.OnMouseDown(e);
                }

                Invalidated?.Invoke();
            }
        }

        public void PointerUp(Point world, MouseEventArgs e)
        {
            _mouseLoc = world;
            if (!HasDocument || _previewMode)
            {
                return;
            }

            if (e.Button == MouseButtons.Middle)
            {
                // Selects the hovered tile with any tool, so the cell stack lists its cell.
                if (HoverTile != null)
                {
                    SetSelection(new[] { HoverTile }, add: false);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (_pickClick)
                {
                    _pickClick = false;
                    if (HoverTile != null)
                    {
                        PickDrawTile(HoverTile.Id);
                    }
                }
                else
                {
                    ActiveTool.OnMouseUp(e);
                }
            }

            Invalidated?.Invoke();
        }
    }
}
