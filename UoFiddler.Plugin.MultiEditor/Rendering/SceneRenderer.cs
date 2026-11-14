/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// Turns an editor session into sprites and lines: the multi in draw order with its view
    /// highlights, the grid, then what the active tool shows.
    /// </summary>
    internal sealed class SceneRenderer : IDisposable, IToolOverlay
    {
        private static readonly SpriteTint _hover = new SpriteTint(0.85f, 0.9f, 1f, 1f, 0.05f, 0.15f, 0.5f);
        private static readonly SpriteTint _selected = new SpriteTint(1f, 0.65f, 0.65f, 1f, 0.45f, 0.05f, 0f);
        private static readonly SpriteTint _multiSelected = new SpriteTint(0.7f, 1f, 1f, 1f, 0f, 0.3f, 0.3f);
        private static readonly SpriteTint _aboveLevel = SpriteTint.Alpha(0.2f);
        private static readonly SpriteTint _belowLevel = new SpriteTint(0.55f, 0.55f, 0.55f, 1f, 0f, 0f, 0f);
        private static readonly SpriteTint _transparent = SpriteTint.Alpha(0.3f);
        private static readonly SpriteTint _standable = new SpriteTint(0.6f, 1f, 0.6f, 1f, 0f, 0.3f, 0f);
        private static readonly SpriteTint _toolGhost = new SpriteTint(1f, 1f, 1f, 0.6f, 0f, 0.08f, 0.16f);
        private static readonly SpriteTint _removeMark = new SpriteTint(1f, 0.3f, 0.3f, 0.85f, 0.45f, 0f, 0f);

        private static readonly SpriteTint _reference = new SpriteTint(0.7f, 0.8f, 1f, 0.4f, 0f, 0.05f, 0.15f);
        private static readonly SpriteTint _translucent = SpriteTint.Alpha(0.55f);


        // The user's colours, read once per frame.
        private Color _gridColor;
        private Color _boundsColor;
        private Color _marqueeLine;
        private Color _marqueeFill;
        private SpriteTint _workPlane;

        private readonly ArtAtlas _atlas;
        private readonly SpriteBatch _sprites;
        private readonly LineBatch _lines;

        private EditorSession _session;
        private RectangleF _visible;

        public SceneRenderer()
        {
            _atlas = new ArtAtlas();
            _sprites = new SpriteBatch();
            _lines = new LineBatch();
        }

        public ArtAtlas Atlas => _atlas;

        /// <summary>Advances animated tiles in preview mode (one step per 100 ms).</summary>
        public int AnimationTick { get; set; }

        /// <summary>True when the last preview frame drew an animated tile.</summary>
        public bool HasAnimation { get; private set; }

        /// <summary>Number of sprites drawn by the last frame.</summary>
        public int LastSpriteCount { get; private set; }

        public void Draw(EditorSession session, Camera2D camera, Size viewport)
        {
            _session = session;
            _visible = camera.VisibleWorld(viewport);
            LastSpriteCount = 0;
            HasAnimation = false;

            UI.EditorSettings settings = UI.EditorSettings.Current;
            _gridColor = settings.Grid;
            _boundsColor = settings.Outline;
            _marqueeLine = settings.SelectionBox;
            _marqueeFill = Color.FromArgb(40, settings.SelectionBox);
            _workPlane = SpriteTint.Solid(settings.WorkPlane);

            if (!session.HasDocument)
            {
                return;
            }

            if (session.PreviewMode)
            {
                AddPreviewTerrain(session);
            }

            AddReference(session);
            if (session.PreviewMode)
            {
                AddPreviewTiles(session);
                LastSpriteCount += _sprites.Count;
                _sprites.Flush(_atlas, camera, viewport);
                return;
            }

            if (session.PlanMode)
            {
                AddPlan(session);
            }
            else
            {
                AddTiles(session);
            }

            LastSpriteCount += _sprites.Count;
            _sprites.Flush(_atlas, camera, viewport);

            if (!session.PlanMode)
            {
                AddGrid(session);
            }

            _lines.Flush(camera, viewport);

            // Tool overlay: sprites first (ghost tiles), then the lines on top of them.
            session.ActiveTool.PaintOverlay(this);
            LastSpriteCount += _sprites.Count;
            _sprites.Flush(_atlas, camera, viewport);
            _lines.Flush(camera, viewport);
        }

        public void Dispose()
        {
            _atlas.Dispose();
            _sprites.Dispose();
            _lines.Dispose();
        }

        // ---------------------------------------------------------------- multi

        private void AddTiles(EditorSession session)
        {
            MultiEditorComponentList list = session.CompList;
            int maxHeight = session.HeightLimit ?? int.MaxValue;
            bool manySelected = session.Selection.Count > 1;
            MultiTile selected = session.SelectedTile;
            MultiTile hover = session.HoverTile;
            (int Floor, int Ceiling)? level = session.ActiveLevel;

            // The work plane goes into each cell's stack where its tiles reach the plane's height,
            // so what is below shows through it and what stands on it is drawn over it.
            bool plane = session.ShowWorkPlane && _atlas.TryGet(ArtAtlas.FloorKey, out _planeSprite);
            int planeZ = session.WorkPlaneZ;

            for (int x = 0; x < list.Width; ++x)
            {
                for (int y = 0; y < list.Height; ++y)
                {
                    bool planeDrawn = !plane;
                    foreach (MultiTile tile in list.TilesIn(x, y))
                    {
                        if (!planeDrawn && tile.SortKey >= planeZ)
                        {
                            AddPlaneCell(x, y, planeZ);
                            planeDrawn = true;
                        }

                        AddTile(session, list, tile, maxHeight, manySelected, selected, hover, level);
                    }

                    if (!planeDrawn)
                    {
                        AddPlaneCell(x, y, planeZ);
                    }
                }
            }
        }

        private AtlasSprite _planeSprite;

        /// <summary>One cell of the work plane: a diamond at the plane's height, when on screen.</summary>
        private void AddPlaneCell(int x, int y, int z)
        {
            PointF corner = EditorSession.CellCorner(x, y, z);
            float left = corner.X - 22;
            if (left < _visible.Right && left + _planeSprite.Width > _visible.Left &&
                corner.Y < _visible.Bottom && corner.Y + _planeSprite.Height > _visible.Top)
            {
                _sprites.Add(_planeSprite, left, corner.Y, _workPlane);
            }
        }

        private void AddTile(EditorSession session, MultiEditorComponentList list, MultiTile tile, int maxHeight,
            bool manySelected, MultiTile selected, MultiTile hover, (int Floor, int Ceiling)? level)
        {
            if (tile.Z > maxHeight || !list.IsShown(tile))
            {
                return;
            }

            if (!_atlas.TryGet(tile.Id, out AtlasSprite sprite) || !IsVisible(tile, sprite))
            {
                return;
            }

            SpriteTint tint;
            if (tile == hover)
            {
                tint = _hover;
            }
            else if (tile == selected)
            {
                tint = _selected;
            }
            else if (manySelected && session.IsSelected(tile))
            {
                tint = _multiSelected;
            }
            else if (level is { } above && tile.Z >= above.Ceiling)
            {
                if (session.HideAboveLevel)
                {
                    return;
                }

                tint = _aboveLevel;
            }
            else if (level is { } below && tile.Z < below.Floor)
            {
                tint = _belowLevel;
            }
            else if (tile.Transparent)
            {
                tint = _transparent;
            }
            else if ((session.ShowWalkables && tile.Walkable) || (session.ShowDoubleSurface && tile.DoubleSurface))
            {
                tint = _standable;
            }
            else
            {
                tint = SpriteTint.None;
            }

            _sprites.Add(sprite, tile.XMod, tile.YMod, tint);
        }

        /// <summary>The reference multi, faint and blue, under everything else.</summary>
        private void AddReference(EditorSession session)
        {
            MultiEditorComponentList reference = session.Reference;
            if (reference == null)
            {
                return;
            }

            (int dx, int dy, int dz) = session.ReferenceOffset;
            float offsetX = (dx - dy) * 22;
            float offsetY = ((dx + dy) * 22) - (dz * 4);
            foreach (MultiTile tile in reference.RealTiles)
            {
                if (_atlas.TryGet(tile.Id, out AtlasSprite sprite))
                {
                    _sprites.Add(sprite, tile.XMod + offsetX, tile.YMod + offsetY, _reference);
                }
            }
        }

        /// <summary>The first of four land tile variants per terrain, as the client's maps use them.</summary>
        private static int TerrainBase(PreviewTerrain terrain) => terrain switch
        {
            PreviewTerrain.Grass => 0x0003,
            PreviewTerrain.Dirt => 0x0071,
            PreviewTerrain.Sand => 0x0016,
            PreviewTerrain.Snow => 0x011A,
            _ => -1
        };

        /// <summary>
        /// Flat ground at Z 0 across the whole view, as if the multi stood in the world. The four
        /// variants of the terrain are mixed by cell so it does not look tiled.
        /// </summary>
        private void AddPreviewTerrain(EditorSession session)
        {
            int land = TerrainBase(session.PreviewTerrain);
            if (land < 0)
            {
                return;
            }

            // The cells the view's corners fall in bound every cell that can be seen.
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (PointF corner in new[]
                     {
                         new PointF(_visible.Left, _visible.Top), new PointF(_visible.Right, _visible.Top),
                         new PointF(_visible.Left, _visible.Bottom), new PointF(_visible.Right, _visible.Bottom)
                     })
            {
                EditorSession.PlaneCell(Point.Round(corner), 0, out int cx, out int cy);
                minX = Math.Min(minX, cx);
                minY = Math.Min(minY, cy);
                maxX = Math.Max(maxX, cx);
                maxY = Math.Max(maxY, cy);
            }

            for (int x = minX - 1; x <= maxX + 1; ++x)
            {
                for (int y = minY - 1; y <= maxY + 1; ++y)
                {
                    int variant = (int)((((uint)x * 73856093u) ^ ((uint)y * 19349663u)) >> 4) & 3;
                    if (!_atlas.TryGet(ArtAtlas.LandKey(land + variant), out AtlasSprite sprite))
                    {
                        continue;
                    }

                    PointF corner = EditorSession.CellCorner(x, y, 0);
                    float px = corner.X - 22;
                    if (px < _visible.Right && px + sprite.Width > _visible.Left && corner.Y < _visible.Bottom && corner.Y + sprite.Height > _visible.Top)
                    {
                        _sprites.Add(sprite, px, corner.Y, SpriteTint.None);
                    }
                }
            }
        }

        /// <summary>The multi as the client shows it: every tile, translucent ones see-through, animations running.</summary>
        private void AddPreviewTiles(EditorSession session)
        {
            foreach (MultiTile tile in session.CompList.RealTiles)
            {
                ItemData data = TileData.ItemTable[tile.Id];
                int id = tile.Id;
                if ((data.Flags & TileFlag.Animation) != 0 && Animdata.GetAnimData(id) is { FrameCount: > 0 } animation)
                {
                    HasAnimation = true;
                    int interval = Math.Max(1, (int)animation.FrameInterval);
                    id += animation.FrameData[AnimationTick / interval % animation.FrameCount];
                }

                // Frames stand on the same cell as the base art: centred on it, bottoms aligned.
                if (!_atlas.TryGet(id, out AtlasSprite sprite) || !ArtMetrics.TryGetSize(tile.Id, out int width, out int height))
                {
                    continue;
                }

                float x = tile.XMod + ((width - sprite.Width) / 2);
                float y = tile.YMod + height - sprite.Height;
                bool translucent = (data.Flags & TileFlag.Translucent) != 0;
                _sprites.Add(sprite, x, y, translucent ? _translucent : SpriteTint.None);
            }
        }

        private bool IsVisible(MultiTile tile, AtlasSprite sprite)
        {
            return tile.XMod < _visible.Right && tile.XMod + sprite.Width > _visible.Left &&
                   tile.YMod < _visible.Bottom && tile.YMod + sprite.Height > _visible.Top;
        }

        /// <summary>The outline of the multi at Z 0, and the cell grid when it is on.</summary>
        private void AddGrid(EditorSession session)
        {
            MultiEditorComponentList list = session.CompList;

            // On the ground, whatever floor is chosen: the work plane is what moves with the floors.
            const int z = 0;
            int width = list.Width;
            int height = list.Height;

            if (session.ShowGrid)
            {
                for (int x = 1; x < width; ++x)
                {
                    Line(EditorSession.CellCorner(x, 0, z), EditorSession.CellCorner(x, height, z), _gridColor);
                }

                for (int y = 1; y < height; ++y)
                {
                    Line(EditorSession.CellCorner(0, y, z), EditorSession.CellCorner(width, y, z), _gridColor);
                }
            }

            PointF a = EditorSession.CellCorner(0, 0, z);
            PointF b = EditorSession.CellCorner(width, 0, z);
            PointF c = EditorSession.CellCorner(width, height, z);
            PointF d = EditorSession.CellCorner(0, height, z);
            Line(a, b, _boundsColor);
            Line(b, c, _boundsColor);
            Line(c, d, _boundsColor);
            Line(d, a, _boundsColor);
        }

        private void Line(PointF from, PointF to, Color color)
        {
            _lines.Add(from.X, from.Y, to.X, to.Y, color);
        }

        // ---------------------------------------------------------------- plan view

        private static readonly Color _planGrid = Color.FromArgb(70, 128, 128, 128);
        private static readonly Color _planWall = Color.FromArgb(235, 235, 235);
        private static readonly Color _planDoor = Color.FromArgb(205, 133, 63);
        private static readonly Color _planSelected = Color.FromArgb(110, 255, 215, 0);
        private static readonly Color _planPlane = Color.FromArgb(70, 32, 192, 32);

        private void Fill(float x, float y, float width, float height, Color color)
        {
            if (_atlas.TryGet(ArtAtlas.WhiteKey, out AtlasSprite white))
            {
                _sprites.Add(white, x, y, width, height, SpriteTint.Solid(color));
            }
        }

        private static Color RadarColor(ushort id)
        {
            return Ultima.Helpers.HueHelpers.HueToColor(RadarCol.GetItemColor(id));
        }

        /// <summary>Wall and door lines of a tile on the edges of its cell.</summary>
        private void AddPlanShape(PlanShape shape, Point o, Color wall, Color door)
        {
            const int cell = PlanView.Cell;
            Color color = (shape & PlanShape.Door) != 0 ? door : wall;
            if ((shape & PlanShape.South) != 0)
            {
                Fill(o.X - 2, o.Y + cell - 2, cell + 4, 4, color);
            }

            if ((shape & PlanShape.East) != 0)
            {
                Fill(o.X + cell - 2, o.Y - 2, 4, cell + 4, color);
            }

            if ((shape & PlanShape.Post) != 0)
            {
                Fill(o.X + cell - 3, o.Y + cell - 3, 6, 6, wall);
            }
        }

        private void AddPlan(EditorSession session)
        {
            const int cell = PlanView.Cell;
            MultiEditorComponentList list = session.CompList;
            int maxHeight = session.EditMaxHeight;
            int minZ = session.EditMinZ;

            for (int x = 0; x < list.Width; ++x)
            {
                for (int y = 0; y < list.Height; ++y)
                {
                    Point o = PlanView.CellOrigin(x, y);
                    var tiles = list.GetTilesAt(x, y);

                    // Fill: the top tile that is not a wall, darker when below the work plane. A plan is
                    // for the layout, so roofs and loose wall pieces only fill cells with nothing else.
                    MultiTile fill = TopFill(session, tiles, maxHeight, skipCovers: true) ?? TopFill(session, tiles, maxHeight, skipCovers: false);
                    if (fill != null)
                    {
                        Color color = RadarColor(fill.Id);
                        if (fill.Z < minZ)
                        {
                            color = Color.FromArgb(color.R / 2, color.G / 2, color.B / 2);
                        }

                        Fill(o.X, o.Y, cell, cell, color);
                    }

                    if (session.ShowWorkPlane)
                    {
                        Fill(o.X, o.Y, cell, cell, _planPlane);
                    }

                    bool selected = false;
                    foreach (MultiTile tile in tiles)
                    {
                        if (tile.Z > maxHeight || tile.Z < minZ || !list.IsShown(tile))
                        {
                            continue;
                        }

                        AddPlanShape(session.PlanShapeOf(tile.Id), o, _planWall, _planDoor);
                        selected |= session.IsSelected(tile);
                    }

                    if (selected)
                    {
                        Fill(o.X, o.Y, cell, cell, _planSelected);
                    }
                }
            }

            int right = PlanView.CellOrigin(list.Width, 0).X;
            int bottom = PlanView.CellOrigin(0, list.Height).Y;
            Point origin = PlanView.CellOrigin(0, 0);
            for (int x = 0; x <= list.Width; ++x)
            {
                int px = PlanView.CellOrigin(x, 0).X;
                _lines.Add(px, origin.Y, px, bottom, _planGrid);
            }

            for (int y = 0; y <= list.Height; ++y)
            {
                int py = PlanView.CellOrigin(0, y).Y;
                _lines.Add(origin.X, py, right, py, _planGrid);
            }

            if (session.HoverTile is { } hover)
            {
                Point o = PlanView.CellOrigin(hover.X, hover.Y);
                _lines.Rectangle(new RectangleF(o.X + 1, o.Y + 1, cell - 2, cell - 2), _marqueeLine);
                _lines.Rectangle(new RectangleF(o.X + 2, o.Y + 2, cell - 4, cell - 4), _marqueeLine);
            }
        }

        private static MultiTile TopFill(EditorSession session, System.Collections.Generic.List<MultiTile> tiles, int maxHeight, bool skipCovers)
        {
            for (int i = tiles.Count - 1; i >= 0; --i)
            {
                MultiTile tile = tiles[i];
                if (tile.Z > maxHeight || !session.CompList.IsShown(tile) || session.PlanShapeOf(tile.Id) != PlanShape.Fill ||
                    (skipCovers && (TileData.ItemTable[tile.Id].Flags & (TileFlag.Roof | TileFlag.Wall)) != 0))
                {
                    continue;
                }

                return tile;
            }

            return null;
        }

        // ---------------------------------------------------------------- tool overlay

        void IToolOverlay.Ghost(ushort id, int x, int y, int z)
        {
            if (_session.PlanMode)
            {
                if (!_session.CompList.InBounds(x, y))
                {
                    return;
                }

                Point o = PlanView.CellOrigin(x, y);
                PlanShape shape = _session.PlanShapeOf(id);
                if (shape == PlanShape.Fill)
                {
                    Fill(o.X, o.Y, PlanView.Cell, PlanView.Cell, Color.FromArgb(150, RadarColor(id)));
                }
                else
                {
                    AddPlanShape(shape, o, Color.FromArgb(150, _planWall), Color.FromArgb(150, _planDoor));
                }

                return;
            }

            if (!_session.CompList.InBounds(x, y) || !_atlas.TryGet(id, out AtlasSprite sprite))
            {
                return;
            }

            float px = ((x - y) * 22) - (sprite.Width / 2) + MultiEditorComponentList.GapXMod;
            float py = ((x + y) * 22) - (z * 4) - sprite.Height + MultiEditorComponentList.GapYMod;
            _sprites.Add(sprite, px, py, _toolGhost);
        }

        void IToolOverlay.Highlight(MultiTile tile, TileHighlight kind)
        {
            if (_session.PlanMode)
            {
                Point o = PlanView.CellOrigin(tile.X, tile.Y);
                Fill(o.X, o.Y, PlanView.Cell, PlanView.Cell, Color.FromArgb(110, Color.Red));
                return;
            }

            if (_atlas.TryGet(tile.Id, out AtlasSprite sprite))
            {
                _sprites.Add(sprite, tile.XMod, tile.YMod, _removeMark);
            }
        }

        void IToolOverlay.Polyline(System.Collections.Generic.IReadOnlyList<Point> points, bool closed)
        {
            for (int i = 1; i < points.Count; i++)
            {
                _lines.Add(points[i - 1].X, points[i - 1].Y, points[i].X, points[i].Y, _marqueeLine);
            }

            if (closed && points.Count > 2)
            {
                _lines.Add(points[^1].X, points[^1].Y, points[0].X, points[0].Y, _marqueeLine);
            }
        }

        void IToolOverlay.Marquee(Rectangle rect)
        {
            if (_atlas.TryGet(ArtAtlas.WhiteKey, out AtlasSprite white))
            {
                _sprites.Add(white, rect.X, rect.Y, rect.Width, rect.Height, SpriteTint.Solid(_marqueeFill));
            }

            _lines.Rectangle(rect, _marqueeLine);
        }
    }
}
