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
using System.Drawing.Imaging;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// Static art as RGBA8 pixels, read from the art cache so replaced art shows.
    /// </summary>
    internal static class ArtPixels
    {
        public const int FloorSize = 44;

        /// <summary>Top-down RGBA8 pixels of a static, or null when it has no art.</summary>
        public static byte[] LoadStatic(ushort id, out int width, out int height)
        {
            return ToRgba(id != 0xFFFF ? Art.GetStatic(id) : null, out width, out height);
        }

        /// <summary>Top-down RGBA8 pixels of a land tile (a 44x44 diamond), or null when it has no art.</summary>
        public static byte[] LoadLand(int id, out int width, out int height)
        {
            return ToRgba(Art.GetLand(id), out width, out height);
        }

        private static unsafe byte[] ToRgba(Bitmap bitmap, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (bitmap == null)
            {
                return null;
            }

            width = bitmap.Width;
            height = bitmap.Height;
            byte[] rgba = new byte[width * height * 4];
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format16bppArgb1555);
            try
            {
                int i = 0;
                for (int y = 0; y < height; y++)
                {
                    ushort* row = (ushort*)((byte*)data.Scan0 + (y * data.Stride));
                    for (int x = 0; x < width; x++)
                    {
                        ushort c = row[x];
                        rgba[i++] = Expand5((c >> 10) & 0x1F);
                        rgba[i++] = Expand5((c >> 5) & 0x1F);
                        rgba[i++] = Expand5(c & 0x1F);
                        rgba[i++] = (c & 0x8000) != 0 ? (byte)255 : (byte)0;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return rgba;
        }

        /// <summary>A work plane cell: a translucent diamond with a stronger edge, coloured by its tint.</summary>
        public static byte[] FloorDiamond()
        {
            const int half = FloorSize / 2;
            byte[] rgba = new byte[FloorSize * FloorSize * 4];
            for (int y = 0; y < FloorSize; y++)
            {
                for (int x = 0; x < FloorSize; x++)
                {
                    // Distance from the centre in diamond steps, measured at pixel centres.
                    float d = System.Math.Abs(x + 0.5f - half) + System.Math.Abs(y + 0.5f - half);
                    if (d > half)
                    {
                        continue;
                    }

                    int i = ((y * FloorSize) + x) * 4;
                    bool edge = d > half - 1;
                    // Neutral, so a tint gives it any colour: a brighter, more solid edge around the fill.
                    byte value = edge ? (byte)255 : (byte)200;
                    rgba[i] = value;
                    rgba[i + 1] = value;
                    rgba[i + 2] = value;
                    rgba[i + 3] = edge ? (byte)255 : (byte)160;
                }
            }

            return rgba;
        }

        private static byte Expand5(int value)
        {
            return (byte)((value << 3) | (value >> 2));
        }
    }

    /// <summary>
    /// Which pixels of a static are drawn, for picking the tile under the cursor.
    /// </summary>
    internal sealed class ArtMasks
    {
        private sealed class Mask
        {
            public int Width;
            public int Height;
            public bool[] Opaque;
        }

        private readonly Dictionary<ushort, Mask> _masks = new Dictionary<ushort, Mask>();

        /// <summary>True when the art has a drawn pixel at the given offset from its top left.</summary>
        public bool Hit(ushort id, int x, int y)
        {
            if (x < 0 || y < 0)
            {
                return false;
            }

            if (!_masks.TryGetValue(id, out Mask mask))
            {
                byte[] rgba = ArtPixels.LoadStatic(id, out int width, out int height);
                mask = new Mask { Width = width, Height = height, Opaque = new bool[width * height] };
                for (int i = 0; rgba != null && i < mask.Opaque.Length; i++)
                {
                    mask.Opaque[i] = rgba[(i * 4) + 3] != 0;
                }

                _masks[id] = mask;
            }

            return x < mask.Width && y < mask.Height && mask.Opaque[(y * mask.Width) + x];
        }

        public void Clear()
        {
            _masks.Clear();
        }
    }
}
