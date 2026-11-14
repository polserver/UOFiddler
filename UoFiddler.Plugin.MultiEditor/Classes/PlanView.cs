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
using System.Collections.Generic;
using System.Drawing;
using Ultima;
using Ultima.Helpers;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// How a tile shows in the plan view: as a wall on the cell's +Y (South) or +X (East) edge,
    /// a post at the +X/+Y corner, a door in an edge, or none of these (the cell's fill).
    /// </summary>
    [Flags]
    internal enum PlanShape
    {
        Fill = 0,
        South = 1,
        East = 2,
        Post = 4,
        Door = 8
    }

    /// <summary>
    /// The top-down plan view: every cell a square in the radar colour of its top tile, walls as
    /// lines on the edges they stand on. Uses the same filters as the isometric view (height
    /// slider, level, hidden tiles), so the same tools work on it.
    /// </summary>
    internal sealed class PlanView
    {
        public const int Cell = 24;
        public const int Margin = 22;

        private readonly MultiEditorComponentList _list;
        private readonly Func<ushort, PlanShape> _shape;

        public PlanView(MultiEditorComponentList list, Func<ushort, PlanShape> shape)
        {
            _list = list;
            _shape = shape;
        }

        public static Size ContentSize(int width, int height) => new Size((width * Cell) + (2 * Margin), (height * Cell) + (2 * Margin));

        /// <summary>Canvas position of the north-west corner of a cell.</summary>
        public static Point CellOrigin(int x, int y) => new Point(Margin + (x * Cell), Margin + (y * Cell));

        /// <summary>The cell under a canvas point (may be outside the multi).</summary>
        public static (int X, int Y) CellAt(Point point) =>
            ((int)Math.Floor((point.X - Margin) / (double)Cell), (int)Math.Floor((point.Y - Margin) / (double)Cell));

        /// <summary>
        /// The top tile of a cell that the editor lets you work on: shown, not above
        /// <paramref name="maxHeight"/> and not below <paramref name="minZ"/>.
        /// </summary>
        public MultiTile TopTile(int x, int y, int maxHeight, int minZ)
        {
            List<MultiTile> cell = _list.GetTilesAt(x, y);
            for (int i = cell.Count - 1; i >= 0; --i)
            {
                MultiTile tile = cell[i];
                if (tile.Z <= maxHeight && tile.Z >= minZ && _list.IsShown(tile))
                {
                    return tile;
                }
            }

            return null;
        }

        /// <summary>Every tile you could work on in the cells a canvas rectangle touches.</summary>
        public List<MultiTile> TilesInRect(Rectangle canvasRect, int maxHeight, int minZ)
        {
            var result = new List<MultiTile>();
            var (x0, y0) = CellAt(canvasRect.Location);
            var (x1, y1) = CellAt(new Point(canvasRect.Right, canvasRect.Bottom));
            for (int x = Math.Max(0, x0); x <= Math.Min(_list.Width - 1, x1); ++x)
            {
                for (int y = Math.Max(0, y0); y <= Math.Min(_list.Height - 1, y1); ++y)
                {
                    foreach (MultiTile tile in _list.GetTilesAt(x, y))
                    {
                        if (tile.Z <= maxHeight && tile.Z >= minZ && _list.IsShown(tile))
                        {
                            result.Add(tile);
                        }
                    }
                }
            }

            return result;
        }

        public void Draw(Graphics gfx, int scrollX, int scrollY, int maxHeight, int minZ, MultiTile hover, ICollection<MultiTile> selection, bool showFloorGrid)
        {
            gfx.TranslateTransform(-scrollX, -scrollY);
            try
            {
                using var gridPen = new Pen(Color.FromArgb(60, 128, 128, 128));
                using var wallPen = new Pen(Color.FromArgb(40, 40, 40), 4) { StartCap = System.Drawing.Drawing2D.LineCap.Square, EndCap = System.Drawing.Drawing2D.LineCap.Square };
                using var doorPen = new Pen(Color.SaddleBrown, 4) { DashPattern = new[] { 1f, 0.6f } };
                using var selectedBrush = new SolidBrush(Color.FromArgb(110, 255, 215, 0));
                using var floorBrush = new SolidBrush(Color.FromArgb(70, 32, 192, 32));

                for (int x = 0; x < _list.Width; ++x)
                {
                    for (int y = 0; y < _list.Height; ++y)
                    {
                        Point o = CellOrigin(x, y);
                        var square = new Rectangle(o.X, o.Y, Cell, Cell);
                        List<MultiTile> cell = _list.GetTilesAt(x, y);

                        // Fill: the top tile that is not a wall, darker when below the floor. A plan is
                        // for the layout, so roofs and loose wall pieces (gable caps, windows) only fill
                        // cells with nothing else in them.
                        MultiTile fill = TopFill(cell, maxHeight, skipCovers: true) ?? TopFill(cell, maxHeight, skipCovers: false);
                        if (fill != null)
                        {
                            Color color = HueHelpers.HueToColor(RadarCol.GetItemColor(fill.Id));
                            if (fill.Z < minZ)
                            {
                                color = Color.FromArgb(color.R / 2, color.G / 2, color.B / 2);
                            }

                            using var brush = new SolidBrush(color);
                            gfx.FillRectangle(brush, square);
                        }

                        if (showFloorGrid)
                        {
                            gfx.FillRectangle(floorBrush, square);
                        }

                        gfx.DrawRectangle(gridPen, square);

                        foreach (MultiTile tile in cell)
                        {
                            if (tile.Z > maxHeight || tile.Z < minZ || !_list.IsShown(tile))
                            {
                                continue;
                            }

                            PlanShape shape = _shape(tile.Id);
                            Pen pen = (shape & PlanShape.Door) != 0 ? doorPen : wallPen;
                            if ((shape & PlanShape.South) != 0)
                            {
                                gfx.DrawLine(pen, o.X, o.Y + Cell, o.X + Cell, o.Y + Cell);
                            }

                            if ((shape & PlanShape.East) != 0)
                            {
                                gfx.DrawLine(pen, o.X + Cell, o.Y, o.X + Cell, o.Y + Cell);
                            }

                            if ((shape & PlanShape.Post) != 0)
                            {
                                gfx.FillRectangle(Brushes.Black, o.X + Cell - 3, o.Y + Cell - 3, 6, 6);
                            }

                            if (selection.Contains(tile))
                            {
                                gfx.FillRectangle(selectedBrush, square);
                            }
                        }
                    }
                }

                if (hover != null)
                {
                    Point o = CellOrigin(hover.X, hover.Y);
                    using var hoverPen = new Pen(Color.DodgerBlue, 2);
                    gfx.DrawRectangle(hoverPen, o.X + 1, o.Y + 1, Cell - 2, Cell - 2);
                }
            }
            finally
            {
                gfx.TranslateTransform(scrollX, scrollY);
            }
        }

        private MultiTile TopFill(List<MultiTile> cell, int maxHeight, bool skipCovers)
        {
            for (int i = cell.Count - 1; i >= 0; --i)
            {
                MultiTile tile = cell[i];
                if (tile.Z > maxHeight || !_list.IsShown(tile) || _shape(tile.Id) != PlanShape.Fill ||
                    (skipCovers && (TileData.ItemTable[tile.Id].Flags & (TileFlag.Roof | TileFlag.Wall)) != 0))
                {
                    continue;
                }

                return tile;
            }

            return null;
        }

        /// <summary>A planned tile drawn faintly: its colour, or its wall line.</summary>
        public void DrawGhost(Graphics gfx, int scrollX, int scrollY, ushort id, int x, int y)
        {
            if (!_list.InBounds(x, y))
            {
                return;
            }

            Point o = CellOrigin(x, y);
            o.Offset(-scrollX, -scrollY);
            PlanShape shape = _shape(id);
            if (shape == PlanShape.Fill)
            {
                Color color = HueHelpers.HueToColor(RadarCol.GetItemColor(id));
                using var brush = new SolidBrush(Color.FromArgb(150, color));
                gfx.FillRectangle(brush, o.X, o.Y, Cell, Cell);
                return;
            }

            using var pen = new Pen(Color.FromArgb(150, 30, 30, 30), 4);
            if ((shape & PlanShape.South) != 0)
            {
                gfx.DrawLine(pen, o.X, o.Y + Cell, o.X + Cell, o.Y + Cell);
            }

            if ((shape & PlanShape.East) != 0)
            {
                gfx.DrawLine(pen, o.X + Cell, o.Y, o.X + Cell, o.Y + Cell);
            }

            if ((shape & PlanShape.Post) != 0)
            {
                using var brush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
                gfx.FillRectangle(brush, o.X + Cell - 3, o.Y + Cell - 3, 6, 6);
            }
        }

        /// <summary>A tile marked, e.g. for removal: its cell washed in a colour.</summary>
        public static void DrawMark(Graphics gfx, int scrollX, int scrollY, MultiTile tile, Color color)
        {
            Point o = CellOrigin(tile.X, tile.Y);
            using var brush = new SolidBrush(Color.FromArgb(110, color));
            gfx.FillRectangle(brush, o.X - scrollX, o.Y - scrollY, Cell, Cell);
        }
    }
}
