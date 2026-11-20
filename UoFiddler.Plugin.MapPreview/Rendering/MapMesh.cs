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
using UoFiddler.Plugin.MapPreview.Scene;
using UoFiddler.Rendering;

namespace UoFiddler.Plugin.MapPreview.Rendering
{
    /// <summary>What the preview shows. Changing any of it rebuilds the vertices.</summary>
    public sealed record PreviewOptions
    {
        public bool ShowLand { get; init; } = true;

        public bool ShowStatics { get; init; } = true;

        public bool Stretch { get; init; } = true;

        public bool Lighting { get; init; } = true;

        public bool Hues { get; init; } = true;

        public bool HideRoofs { get; init; }

        /// <summary>Statics at or above this height are hidden; null shows all of them.</summary>
        public int? MaxZ { get; init; }

        /// <summary>
        /// <see cref="MaxZ"/> hides land too, so statics under the terrain show. The client's
        /// roof cut never hides land, so this is off by default.
        /// </summary>
        public bool CutLand { get; init; }
    }

    /// <summary>A drawn quad, kept for hit testing. Corners run top, right, bottom, left.</summary>
    internal readonly record struct MeshItem(DrawableKind Kind, int Index, PointF Top, PointF Right, PointF Bottom,
        PointF Left);

    /// <summary>Turns a sorted scene into quads for <see cref="MapBatch"/>.</summary>
    internal static class MapMesh
    {
        private const float TranslucentAlpha = 0.7f;

        public static float[] Build(MapPreviewScene scene, Drawable[] order, PreviewOptions options, ArtAtlas atlas,
            int hueCount, List<MeshItem> items, out int quads, out RectangleF bounds)
        {
            items.Clear();
            float[] vertices = new float[order.Length * MapBatch.FloatsPerQuad];
            int v = 0;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            int width = scene.Area.Width;

            foreach (Drawable drawable in order)
            {
                MeshItem item;
                if (drawable.Kind == DrawableKind.Land)
                {
                    if (!options.ShowLand || LandCut(scene.Land[drawable.Index], options) ||
                        !TryLand(scene.Land[drawable.Index], drawable.Index, drawable.Index % width,
                            drawable.Index / width, options, atlas, vertices, ref v, out item))
                    {
                        continue;
                    }
                }
                else
                {
                    SceneStatic s = scene.Statics[drawable.Index];
                    if (!options.ShowStatics || Hidden(s, options) ||
                        !TryStatic(s, drawable.Index, options, atlas, hueCount, vertices, ref v, out item))
                    {
                        continue;
                    }
                }

                items.Add(item);
                foreach (PointF corner in new[] { item.Top, item.Right, item.Bottom, item.Left })
                {
                    minX = Math.Min(minX, corner.X);
                    maxX = Math.Max(maxX, corner.X);
                    minY = Math.Min(minY, corner.Y);
                    maxY = Math.Max(maxY, corner.Y);
                }
            }

            quads = v / MapBatch.FloatsPerQuad;
            bounds = quads == 0 ? RectangleF.Empty : RectangleF.FromLTRB(minX, minY, maxX, maxY);
            return vertices;
        }

        private static bool Hidden(in SceneStatic s, PreviewOptions options)
        {
            return (options.HideRoofs && (s.Flags & TileFlag.Roof) != 0) || (options.MaxZ is int maxZ && s.Z >= maxZ);
        }

        /// <summary>Land cut by the height, at the height it is sorted by: a stretched tile's middle.</summary>
        private static bool LandCut(in SceneLand land, PreviewOptions options)
        {
            return options.CutLand && options.MaxZ is int maxZ &&
                   (options.Stretch && land.Stretched ? land.AverageZ : land.Z) >= maxZ;
        }

        private static bool TryLand(in SceneLand land, int index, int dx, int dy, PreviewOptions options, ArtAtlas atlas,
            float[] vertices, ref int v, out MeshItem item)
        {
            float x = (dx - dy) * MapPreviewScene.TileHalf;
            float row = (dx + dy) * MapPreviewScene.TileHalf;

            if (options.Stretch && land.Stretched && atlas.TryGet(ArtAtlas.TexmapKey(land.TextureId), out AtlasSprite texmap))
            {
                var top = new PointF(x, row - (land.Top * MapPreviewScene.PixelsPerZ));
                var right = new PointF(x + MapPreviewScene.TileHalf, row + MapPreviewScene.TileHalf - (land.Right * MapPreviewScene.PixelsPerZ));
                var bottom = new PointF(x, row + MapPreviewScene.TileSize - (land.Bottom * MapPreviewScene.PixelsPerZ));
                var left = new PointF(x - MapPreviewScene.TileHalf, row + MapPreviewScene.TileHalf - (land.Left * MapPreviewScene.PixelsPerZ));

                // Half a texel in from the edge, so neighbouring sprites in the atlas never bleed in.
                float u0 = texmap.X + 0.5f, v0 = texmap.Y + 0.5f;
                float u1 = texmap.X + texmap.Width - 0.5f, v1 = texmap.Y + texmap.Height - 0.5f;

                bool lit = options.Lighting;
                Put(vertices, ref v, top, u0, v0, texmap.Layer, lit ? land.LightTop : 1f, -1, false, 1f);
                Put(vertices, ref v, right, u1, v0, texmap.Layer, lit ? land.LightRight : 1f, -1, false, 1f);
                Put(vertices, ref v, left, u0, v1, texmap.Layer, lit ? land.LightLeft : 1f, -1, false, 1f);
                Put(vertices, ref v, bottom, u1, v1, texmap.Layer, lit ? land.LightBottom : 1f, -1, false, 1f);

                item = new MeshItem(DrawableKind.Land, index, top, right, bottom, left);
                return true;
            }

            if (!atlas.TryGet(ArtAtlas.LandKey(land.Id), out AtlasSprite art))
            {
                item = default;
                return false;
            }

            float artTop = row - (land.Z * MapPreviewScene.PixelsPerZ);
            AddSprite(vertices, ref v, art, x - MapPreviewScene.TileHalf, artTop, 1f, -1, false, 1f);

            item = new MeshItem(DrawableKind.Land, index,
                new PointF(x, artTop),
                new PointF(x + MapPreviewScene.TileHalf, artTop + MapPreviewScene.TileHalf),
                new PointF(x, artTop + MapPreviewScene.TileSize),
                new PointF(x - MapPreviewScene.TileHalf, artTop + MapPreviewScene.TileHalf));
            return true;
        }

        private static bool TryStatic(in SceneStatic s, int index, PreviewOptions options, ArtAtlas atlas, int hueCount,
            float[] vertices, ref int v, out MeshItem item)
        {
            if (!atlas.TryGet(s.Id, out AtlasSprite art))
            {
                item = default;
                return false;
            }

            float x = (s.X - s.Y) * MapPreviewScene.TileHalf;
            float row = ((s.X + s.Y) * MapPreviewScene.TileHalf) - (s.Z * MapPreviewScene.PixelsPerZ);

            // Bottom centre of the art on the bottom corner of the tile's diamond.
            float left = x - (art.Width / 2);
            float top = row + MapPreviewScene.TileSize - art.Height;

            int hue = -1;
            bool partial = false;
            if (options.Hues)
            {
                int index0 = (s.Hue & 0x3FFF) - 1;
                if (index0 >= 0 && index0 < hueCount)
                {
                    hue = index0;
                    partial = (s.Hue & 0x8000) != 0 || (s.Flags & TileFlag.PartialHue) != 0;
                }
            }

            float alpha = (s.Flags & TileFlag.Translucent) != 0 ? TranslucentAlpha : 1f;
            AddSprite(vertices, ref v, art, left, top, 1f, hue, partial, alpha);

            item = new MeshItem(DrawableKind.Static, index,
                new PointF(left, top),
                new PointF(left + art.Width, top),
                new PointF(left + art.Width, top + art.Height),
                new PointF(left, top + art.Height));
            return true;
        }

        private static void AddSprite(float[] vertices, ref int v, in AtlasSprite art, float left, float top, float light,
            int hue, bool partial, float alpha)
        {
            float right = left + art.Width;
            float bottom = top + art.Height;
            float u0 = art.X, v0 = art.Y, u1 = art.X + art.Width, v1 = art.Y + art.Height;

            Put(vertices, ref v, new PointF(left, top), u0, v0, art.Layer, light, hue, partial, alpha);
            Put(vertices, ref v, new PointF(right, top), u1, v0, art.Layer, light, hue, partial, alpha);
            Put(vertices, ref v, new PointF(left, bottom), u0, v1, art.Layer, light, hue, partial, alpha);
            Put(vertices, ref v, new PointF(right, bottom), u1, v1, art.Layer, light, hue, partial, alpha);
        }

        private static void Put(float[] vertices, ref int v, PointF position, float u, float texV, int layer, float light,
            int hue, bool partial, float alpha)
        {
            vertices[v++] = position.X;
            vertices[v++] = position.Y;
            vertices[v++] = u;
            vertices[v++] = texV;
            vertices[v++] = layer;
            vertices[v++] = light;
            vertices[v++] = hue;
            vertices[v++] = partial ? 1f : 0f;
            vertices[v++] = alpha;
        }
    }
}
