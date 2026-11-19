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
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    internal abstract partial class WingDraftTool
    {
        // ---------------------------------------------------------------- overlay

        public override void PaintOverlay(IToolOverlay overlay)
        {
            PaintDraft(overlay, active: true);
        }

        // The planned tiles in the order they are drawn, kept until the plan changes.
        private object _orderedFor;
        private List<(MultiTile Tile, string Layer)> _ordered;

        /// <summary>
        /// The planned tiles in the order the multi's own tiles are drawn: cell after cell, and
        /// within a cell from the bottom up, floors before what stands on them. Drawn in the
        /// order they were planned, a floor laid after its walls, or the floor of the storey
        /// above, would lie over walls that stand in front of it.
        /// </summary>
        private List<(MultiTile Tile, string Layer)> InDrawOrder(List<(ushort Id, int X, int Y, int Z, string Layer)> add)
        {
            if (!ReferenceEquals(_orderedFor, add) || _ordered == null || _ordered.Count != add.Count)
            {
                _ordered = new List<(MultiTile, string)>(add.Count);
                foreach (var (id, x, y, z, layer) in add)
                {
                    _ordered.Add((new MultiTile(id, x, y, z, 0), layer));
                }

                _ordered.Sort((a, b) => a.Tile.CompareTo(b.Tile));
                _orderedFor = add;
            }

            return _ordered;
        }

        /// <summary>
        /// Draws the draft as it would be built. With <paramref name="active"/> (this tool is in
        /// use) it also shows what the mouse would take hold of: the wing under it, the side of
        /// the selected wing a press would resize, and the wing being dragged out.
        /// </summary>
        public void PaintDraft(IToolOverlay overlay, bool active)
        {
            if (!Ready)
            {
                return;
            }

            var wings = new List<RoofWing>(_wings);
            List<RoofWing> stroke = null;
            if (active && _drag == DragKind.New)
            {
                stroke = WingsFor(_startX, _startY, _endX, _endY, _z, out _);
            }
            else if (active && !HasPending && PreviewsFirstClick)
            {
                StartCell(out int x, out int y, out int z);
                stroke = WingsFor(x, y, x, y, z, out _);
            }

            if (stroke != null)
            {
                wings.AddRange(stroke);
            }

            var (remove, add) = Plan(wings);
            foreach (MultiTile tile in remove)
            {
                overlay.Highlight(tile, TileHighlight.Remove);
            }

            // A house is looked into like the multi: storeys above the chosen floor are left out.
            int highest = GhostsFollowView ? Context.MaxHeight : int.MaxValue;
            foreach ((MultiTile tile, string layer) in InDrawOrder(add))
            {
                if (tile.Z <= highest && ShowsInPreview(layer))
                {
                    overlay.Ghost(tile.Id, tile.X, tile.Y, tile.Z);
                }
            }

            // Roof cells the style has no piece for stay open; they are marked so the gap is seen.
            if (ShowsInPreview(RoofTool.LayerName))
            {
                foreach ((int x, int y, int z) in RoofGaps)
                {
                    if (z <= highest)
                    {
                        overlay.CellMark(x, y, z, OverlayTone.Remove);
                    }
                }
            }

            PaintDetails(overlay, active);

            int hover = -1, hx = 0, hy = 0;
            if (active && _drag == DragKind.None && HasPending)
            {
                hover = HitWing(out hx, out hy);
            }

            for (int i = 0; i < _wings.Count; ++i)
            {
                RoofWing wing = _wings[i];
                int z = GripZ(wing);
                bool selected = i == _selected;
                if (selected && active)
                {
                    // The rows and columns a press would resize by; the one under the mouse stands out.
                    Edges(wing, hx, hy, hover == i, out bool west, out bool east, out bool north, out bool south);
                    if (_drag == DragKind.Resize)
                    {
                        (west, east, north, south) = (_west, _east, _north, _south);
                    }

                    for (int x = wing.X0; x <= wing.X1; ++x)
                    {
                        for (int y = wing.Y0; y <= wing.Y1; ++y)
                        {
                            bool band = x == wing.X0 || x == wing.X1 || y == wing.Y0 || y == wing.Y1;
                            bool held = (west && x == wing.X0) || (east && x == wing.X1) || (north && y == wing.Y0) || (south && y == wing.Y1);
                            if (held)
                            {
                                overlay.CellMark(x, y, z, OverlayTone.Hover);
                            }
                            else if (band)
                            {
                                overlay.CellMark(x, y, z, OverlayTone.Selected);
                            }
                        }
                    }
                }

                OverlayTone tone = selected ? OverlayTone.Selected : i == hover ? OverlayTone.Hover : OverlayTone.Plain;
                Point[] corners = BoxOutline(wing.X0, wing.Y0, wing.X1, wing.Y1, z);
                overlay.Outline(corners, tone);
                if (!selected || !active)
                {
                    continue;
                }

                // Grips in the middle of the selected wing's sides.
                for (int side = 0; side < 4; ++side)
                {
                    Point a = corners[side], b = corners[(side + 1) % 4];
                    var mid = new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
                    overlay.Outline(new[]
                    {
                        new Point(mid.X - 5, mid.Y - 5), new Point(mid.X + 5, mid.Y - 5),
                        new Point(mid.X + 5, mid.Y + 5), new Point(mid.X - 5, mid.Y + 5)
                    }, OverlayTone.Selected);
                }
            }

            if (stroke != null && _drag == DragKind.New)
            {
                foreach (RoofWing wing in stroke)
                {
                    overlay.Outline(BoxOutline(wing.X0, wing.Y0, wing.X1, wing.Y1, GripZ(wing)), OverlayTone.Hover);
                }
            }
        }

        /// <summary>The corners of a box of cells on the plane at height z: top, right, bottom, left.</summary>
        protected static Point[] BoxOutline(int x0, int y0, int x1, int y1, int z)
        {
            return new[] { Corner(x0, y0, z), Corner(x1 + 1, y0, z), Corner(x1 + 1, y1 + 1, z), Corner(x0, y1 + 1, z) };
        }

        /// <summary>Canvas position of the top corner of a cell's diamond at height z.</summary>
        protected static Point Corner(int x, int y, int z)
        {
            return new Point(((x - y) * 22) + MultiEditorComponentList.GapXMod, ((x + y) * 22) - (z * 4) - 22);
        }
    }
}
