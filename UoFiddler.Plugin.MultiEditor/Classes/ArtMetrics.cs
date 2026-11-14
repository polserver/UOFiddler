/***************************************************************************
 *
 * $Author: MuadDib & Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/


using System.Drawing;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// Size of a static's art, which places a tile on the canvas.
    /// </summary>
    internal static class ArtMetrics
    {
        public static bool TryGetSize(ushort id, out int width, out int height)
        {
            Bitmap bmp = id != 0xFFFF ? Art.GetStatic(id) : null;
            if (bmp == null)
            {
                width = 0;
                height = 0;
                return false;
            }

            width = bmp.Width;
            height = bmp.Height;
            return true;
        }
    }
}
