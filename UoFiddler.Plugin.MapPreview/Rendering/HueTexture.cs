/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using OpenTK.Graphics.OpenGL4;
using Ultima;

namespace UoFiddler.Plugin.MapPreview.Rendering
{
    /// <summary>
    /// Every hue as one row of 32 colours, so a shader can recolour a pixel by looking its gray
    /// level up in the hue's row.
    /// </summary>
    internal sealed class HueTexture : IDisposable
    {
        public const int ColorsPerHue = 32;

        public HueTexture()
        {
            Hue[] hues = Hues.List ?? Array.Empty<Hue>();
            Count = Math.Max(1, hues.Length);

            byte[] rgba = new byte[ColorsPerHue * Count * 4];
            for (int h = 0; h < hues.Length; h++)
            {
                ushort[] colors = hues[h]?.Colors;
                if (colors == null)
                {
                    continue;
                }

                for (int c = 0; c < ColorsPerHue && c < colors.Length; c++)
                {
                    int i = ((h * ColorsPerHue) + c) * 4;
                    ushort color = colors[c];
                    rgba[i] = Expand5((color >> 10) & 0x1F);
                    rgba[i + 1] = Expand5((color >> 5) & 0x1F);
                    rgba[i + 2] = Expand5(color & 0x1F);
                    rgba[i + 3] = 255;
                }
            }

            Texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, ColorsPerHue, Count, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, rgba);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        }

        public int Texture { get; private set; }

        /// <summary>How many hues the texture holds; a row index past this is not a hue.</summary>
        public int Count { get; }

        public void Dispose()
        {
            GL.DeleteTexture(Texture);
            Texture = 0;
        }

        private static byte Expand5(int value)
        {
            return (byte)((value << 3) | (value >> 2));
        }
    }
}
