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
using OpenTK.Graphics.OpenGL4;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>Where a sprite sits in the atlas, in pixels.</summary>
    internal readonly record struct AtlasSprite(int Layer, int X, int Y, int Width, int Height);

    /// <summary>
    /// Static art packed into one texture array, filled as tiles are first drawn. One texture
    /// means the whole multi draws in one call and keeps its draw order.
    /// </summary>
    internal sealed class ArtAtlas : IDisposable
    {
        public const int Size = 2048;

        /// <summary>A solid white block, for filled shapes.</summary>
        public const int WhiteKey = -1;

        /// <summary>The virtual floor diamond.</summary>
        public const int FloorKey = -2;

        private const int Padding = 2;

        /// <summary>Keys from here down are land tiles; see <see cref="LandKey"/>.</summary>
        private const int _landBase = -1000;

        /// <summary>The atlas key of a land tile's art (ground under a previewed multi).</summary>
        public static int LandKey(int landId) => _landBase - landId;

        private readonly Dictionary<int, AtlasSprite> _sprites = new Dictionary<int, AtlasSprite>();
        private readonly HashSet<int> _missing = new HashSet<int>();

        private int _texture;
        private int _layers;
        private int _layer;
        private int _shelfX;
        private int _shelfY;
        private int _shelfHeight;

        public ArtAtlas()
        {
            Allocate(2);
        }

        public int Texture => _texture;

        /// <summary>The sprite for a static id or one of the special keys. Needs the GL context.</summary>
        public bool TryGet(int key, out AtlasSprite sprite)
        {
            if (_sprites.TryGetValue(key, out sprite))
            {
                return true;
            }

            if (_missing.Contains(key))
            {
                return false;
            }

            byte[] pixels = Load(key, out int width, out int height);
            if (pixels == null || width > Size - Padding || height > Size - Padding)
            {
                _missing.Add(key);
                return false;
            }

            sprite = Place(width, height);
            Upload(sprite, pixels);
            _sprites[key] = sprite;
            return true;
        }

        /// <summary>Forgets every sprite, after art changed on disk or in memory.</summary>
        public void Clear()
        {
            _sprites.Clear();
            _missing.Clear();
            _layer = 0;
            _shelfX = 0;
            _shelfY = 0;
            _shelfHeight = 0;

            // A fresh texture: old pixels next to new sprites would bleed in when zoomed out.
            GL.DeleteTexture(_texture);
            Allocate(_layers);
        }

        public void Dispose()
        {
            GL.DeleteTexture(_texture);
            _texture = 0;
        }

        private static byte[] Load(int key, out int width, out int height)
        {
            switch (key)
            {
                case WhiteKey:
                    width = height = 4;
                    byte[] white = new byte[4 * 4 * 4];
                    Array.Fill(white, (byte)255);
                    return white;
                case FloorKey:
                    width = height = ArtPixels.FloorSize;
                    return ArtPixels.FloorDiamond();
                default:
                    return key <= _landBase
                        ? ArtPixels.LoadLand(_landBase - key, out width, out height)
                        : ArtPixels.LoadStatic((ushort)key, out width, out height);
            }
        }

        private AtlasSprite Place(int width, int height)
        {
            if (_shelfX + width + Padding > Size)
            {
                _shelfY += _shelfHeight;
                _shelfX = 0;
                _shelfHeight = 0;
            }

            if (_shelfY + height + Padding > Size)
            {
                ++_layer;
                _shelfX = 0;
                _shelfY = 0;
                _shelfHeight = 0;
            }

            if (_layer >= _layers)
            {
                Grow();
            }

            var sprite = new AtlasSprite(_layer, _shelfX, _shelfY, width, height);
            _shelfX += width + Padding;
            _shelfHeight = Math.Max(_shelfHeight, height + Padding);
            return sprite;
        }

        /// <summary>
        /// A texture array cannot grow, so a larger one is made and every sprite is uploaded again
        /// at the place it already had.
        /// </summary>
        private void Grow()
        {
            GL.DeleteTexture(_texture);
            Allocate(_layers * 2);
            foreach (KeyValuePair<int, AtlasSprite> entry in _sprites)
            {
                byte[] pixels = Load(entry.Key, out int width, out int height);
                if (pixels != null && width == entry.Value.Width && height == entry.Value.Height)
                {
                    Upload(entry.Value, pixels);
                }
            }
        }

        private void Allocate(int layers)
        {
            _layers = layers;
            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, _texture);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba8, Size, Size, layers, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // Unused space must be transparent: sprites are sampled next to it when zoomed out.
            byte[] clear = new byte[Size * Size * 4];
            for (int layer = 0; layer < layers; layer++)
            {
                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, layer, Size, Size, 1, PixelFormat.Rgba,
                    PixelType.UnsignedByte, clear);
            }
        }

        private void Upload(AtlasSprite sprite, byte[] pixels)
        {
            GL.BindTexture(TextureTarget.Texture2DArray, _texture);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, sprite.X, sprite.Y, sprite.Layer, sprite.Width,
                sprite.Height, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        }
    }
}
