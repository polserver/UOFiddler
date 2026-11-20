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
using Ultima;

namespace UoFiddler.Plugin.MapPreview.Scene
{
    /// <summary>What a drawable is.</summary>
    public enum DrawableKind : byte
    {
        Land,
        Static
    }

    /// <summary>One thing to draw, in the order the scene sorted it into.</summary>
    public readonly record struct Drawable(DrawableKind Kind, int Index, double Depth);

    /// <summary>A static in the area. X and Y are relative to the area.</summary>
    public readonly record struct SceneStatic(int X, int Y, sbyte Z, ushort Id, int Hue, TileFlag Flags, byte Height);

    /// <summary>
    /// A map area as the client would draw it: land with its four corner heights and per-corner
    /// light, the statics on it, and one painter's order for both. No GL here; the canvas turns this
    /// into vertices.
    /// </summary>
    /// <remarks>
    /// Coordinates are isometric pixels with the top corner of the area's first tile at z 0 as the
    /// origin: a tile's top corner is at <c>((x - y) * 22, (x + y) * 22 - z * 4)</c>.
    /// </remarks>
    public sealed class MapPreviewScene
    {
        public const int TileHalf = 22;
        public const int TileSize = 44;
        public const int PixelsPerZ = 4;

        /// <summary>Length of one tile edge in world units, so slopes come out at the right angle.</summary>
        private const float TileEdge = 31.1127f;

        /// <summary>Where light comes from, in tile axes: +y (south) and above, as the client lights land.</summary>
        private static readonly (float X, float Y, float Z) _light = Normalize(0f, 1f, 1f);

        /// <summary>How strongly a slope's angle to the light changes its brightness.</summary>
        private const float LightStrength = 1.1f;

        private readonly sbyte[] _heights;
        private readonly int _heightsWidth;

        private MapPreviewScene(Map map, Rectangle area, sbyte[] heights, int heightsWidth)
        {
            Map = map;
            Area = area;
            _heights = heights;
            _heightsWidth = heightsWidth;
            Land = new SceneLand[area.Width * area.Height];
        }

        public Map Map { get; }

        /// <summary>The tiles shown, in map coordinates.</summary>
        public Rectangle Area { get; }

        /// <summary>Row-major, <see cref="Area"/> wide.</summary>
        public SceneLand[] Land { get; }

        public List<SceneStatic> Statics { get; } = new List<SceneStatic>();

        /// <summary>
        /// Reads an area. Every Ultima call is made here, on the calling thread, so the caller picks
        /// the thread the map caches are used from.
        /// </summary>
        public static MapPreviewScene Build(Map map, Rectangle area)
        {
            area = Rectangle.Intersect(area, new Rectangle(0, 0, map.Width, map.Height));
            if (area.Width <= 0 || area.Height <= 0)
            {
                throw new ArgumentException("The area is outside the map.", nameof(area));
            }

            // Corner heights reach one tile past the area and the corner normals one more.
            const int margin = 2;
            int heightsWidth = area.Width + (margin * 2);
            int heightsHeight = area.Height + (margin * 2);
            sbyte[] heights = new sbyte[heightsWidth * heightsHeight];
            TileMatrix tiles = map.Tiles;

            for (int hy = 0; hy < heightsHeight; hy++)
            {
                int y = Math.Clamp(area.Y - margin + hy, 0, map.Height - 1);
                for (int hx = 0; hx < heightsWidth; hx++)
                {
                    int x = Math.Clamp(area.X - margin + hx, 0, map.Width - 1);
                    heights[(hy * heightsWidth) + hx] = tiles.GetLandTile(x, y).Z;
                }
            }

            var scene = new MapPreviewScene(map, area, heights, heightsWidth);

            for (int dy = 0; dy < area.Height; dy++)
            {
                for (int dx = 0; dx < area.Width; dx++)
                {
                    int x = area.X + dx;
                    int y = area.Y + dy;
                    scene.Land[(dy * area.Width) + dx] = scene.MakeLand(dx, dy, tiles.GetLandTile(x, y).Id);

                    foreach (HuedTile tile in tiles.GetStaticTiles(x, y))
                    {
                        if (IsNoDraw(tile.Id))
                        {
                            continue;
                        }

                        ItemData data = tile.Id < TileData.ItemTable.Length ? TileData.ItemTable[tile.Id] : default;
                        scene.Statics.Add(new SceneStatic(dx, dy, tile.Z, tile.Id, tile.Hue, data.Flags, data.Height));
                    }
                }
            }

            return scene;
        }

        /// <summary>
        /// The draw order, back to front. The key is the client's: the cell diagonal plus a small
        /// share of the priority height, so a tall thing can sort in front of the next diagonal.
        /// </summary>
        /// <param name="stretch">Whether sloped land is stretched; it moves where land sorts.</param>
        public Drawable[] Arrange(bool stretch)
        {
            int width = Area.Width;
            var order = new Drawable[Land.Length + Statics.Count];
            int n = 0;

            for (int i = 0; i < Land.Length; i++)
            {
                SceneLand land = Land[i];
                if (land.NoDraw)
                {
                    continue;
                }

                int priority = (stretch && land.Stretched ? land.AverageZ : land.Z) - 2;
                order[n++] = new Drawable(DrawableKind.Land, i, Depth((i % width) + (i / width), priority));
            }

            for (int i = 0; i < Statics.Count; i++)
            {
                SceneStatic s = Statics[i];
                order[n++] = new Drawable(DrawableKind.Static, i, Depth(s.X + s.Y, StaticPriority(s)));
            }

            Array.Resize(ref order, n);

            // Stable, so equal keys keep land before statics and statics in file order.
            var keys = new double[n];
            var sequence = new int[n];
            for (int i = 0; i < n; i++)
            {
                keys[i] = order[i].Depth;
                sequence[i] = i;
            }

            Array.Sort(sequence, (a, b) =>
            {
                int byDepth = keys[a].CompareTo(keys[b]);
                return byDepth != 0 ? byDepth : a.CompareTo(b);
            });

            var sorted = new Drawable[n];
            for (int i = 0; i < n; i++)
            {
                sorted[i] = order[sequence[i]];
            }

            return sorted;
        }

        /// <summary>Background items sort under what stands on the same height, items with a height over it.</summary>
        public static int StaticPriority(in SceneStatic s)
        {
            int priority = s.Z;
            if ((s.Flags & TileFlag.Background) != 0)
            {
                priority--;
            }

            if (s.Height > 0)
            {
                priority++;
            }

            return priority;
        }

        public static double Depth(int diagonal, int priorityZ)
        {
            return diagonal + ((127 + priorityZ) * 0.01);
        }

        /// <summary>The land height under a tile of the area, clamped at the map edge.</summary>
        public sbyte HeightAt(int dx, int dy)
        {
            return _heights[((dy + 2) * _heightsWidth) + dx + 2];
        }

        private SceneLand MakeLand(int dx, int dy, ushort id)
        {
            sbyte top = HeightAt(dx, dy);
            sbyte right = HeightAt(dx + 1, dy);
            sbyte bottom = HeightAt(dx + 1, dy + 1);
            sbyte left = HeightAt(dx, dy + 1);

            var land = new SceneLand
            {
                Id = id,
                Z = top,
                Top = top,
                Right = right,
                Bottom = bottom,
                Left = left,
                AverageZ = AverageZ(top, right, bottom, left),
                NoDraw = id == 0x0002,
                LightTop = CornerLight(dx, dy),
                LightRight = CornerLight(dx + 1, dy),
                LightBottom = CornerLight(dx + 1, dy + 1),
                LightLeft = CornerLight(dx, dy + 1)
            };

            land.TextureId = id < TileData.LandTable.Length ? TileData.LandTable[id].TextureId : (ushort)0;

            // Land without a texmap has nothing to stretch, and stays flat whatever its corners say.
            bool canStretch = land.TextureId != 0 && Textures.TestTexture(land.TextureId);

            // A level tile next to a slope still stretches, so its corners share the slope's light.
            land.Stretched = canStretch &&
                             (top != right || top != bottom || top != left ||
                              !IsFlatAround(dx, dy) || !IsFlatAround(dx + 1, dy) ||
                              !IsFlatAround(dx + 1, dy + 1) || !IsFlatAround(dx, dy + 1));
            return land;
        }

        /// <summary>
        /// The client's average for sorting: the mean of the diagonal pair that is closer to level,
        /// which is the fold the quad is drawn along.
        /// </summary>
        public static int AverageZ(int top, int right, int bottom, int left)
        {
            return Math.Abs(top - bottom) <= Math.Abs(left - right)
                ? (top + bottom) >> 1
                : (left + right) >> 1;
        }

        /// <summary>True when a grid point's four neighbours are at its height.</summary>
        private bool IsFlatAround(int gx, int gy)
        {
            sbyte z = HeightAt(gx, gy);
            return HeightAt(gx - 1, gy) == z && HeightAt(gx + 1, gy) == z &&
                   HeightAt(gx, gy - 1) == z && HeightAt(gx, gy + 1) == z;
        }

        /// <summary>
        /// Brightness at a grid point from the slope through its neighbours, scaled so level ground
        /// is 1 and matches the unlit flat art beside it.
        /// </summary>
        private float CornerLight(int gx, int gy)
        {
            float dzdx = (HeightAt(gx + 1, gy) - HeightAt(gx - 1, gy)) * PixelsPerZ / (2f * TileEdge);
            float dzdy = (HeightAt(gx, gy + 1) - HeightAt(gx, gy - 1)) * PixelsPerZ / (2f * TileEdge);
            (float nx, float ny, float nz) = Normalize(-dzdx, -dzdy, 1f);

            float lit = (nx * _light.X) + (ny * _light.Y) + (nz * _light.Z);
            return Math.Clamp(1f + (LightStrength * (lit - _light.Z)), 0.45f, 1.4f);
        }

        /// <summary>Tiles the client never draws: the "no draw" marker and invisible blockers.</summary>
        private static bool IsNoDraw(ushort id)
        {
            return id == 0x0001 || id == 0x21BC;
        }

        private static (float X, float Y, float Z) Normalize(float x, float y, float z)
        {
            float length = MathF.Sqrt((x * x) + (y * y) + (z * z));
            return (x / length, y / length, z / length);
        }
    }

    /// <summary>A land tile with its corner heights, in the order top, right, bottom, left.</summary>
    public struct SceneLand
    {
        public ushort Id;
        public ushort TextureId;
        public sbyte Z;
        public sbyte Top;
        public sbyte Right;
        public sbyte Bottom;
        public sbyte Left;
        public int AverageZ;
        public bool Stretched;
        public bool NoDraw;
        public float LightTop;
        public float LightRight;
        public float LightBottom;
        public float LightLeft;
    }
}
