/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
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
