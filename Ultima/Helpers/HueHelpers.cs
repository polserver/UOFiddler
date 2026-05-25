// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System.Drawing;

namespace Ultima.Helpers
{
    public static class HueHelpers
    {
        // Canonical 8-bit RGB -> 15-bit hue. Uses bit-shift (>>3) packing with the rule:
        // input all-zero -> 0; else any lane that collapses to 0 -> 1.
        public static ushort ColorToHue(Color color) => ColorToHueShift(color.R, color.G, color.B);

        // Canonical 15-bit hue -> 32-bit ARGB, expanding 5-bit components via
        // (c<<3)|(c>>2) so 31 maps to 255 (not 248 as the previous *8 integer math did).
        public static Color HueToColor(ushort hue)
        {
            return Color.FromArgb(
                Expand5To8((hue & 0x7c00) >> 10),
                Expand5To8((hue & 0x03e0) >> 5),
                Expand5To8(hue & 0x001f));
        }

        public static int HueToColorR(ushort hue) => Expand5To8((hue & 0x7c00) >> 10);
        public static int HueToColorG(ushort hue) => Expand5To8((hue & 0x03e0) >> 5);
        public static int HueToColorB(ushort hue) => Expand5To8(hue & 0x001f);

        // Canonical RGB->555 packer for this format. Packs each channel via bit-shift:
        //   result = ((r>>3) << 10) | ((g>>3) << 5) | (b>>3)
        // Clamp rule: if r|g|b == 0 the pixel is 0 (transparent); else if a lane
        // downscales to 0, force that lane to 1.
        public static ushort ColorToHueShift(int r8, int g8, int b8)
        {
            if ((r8 | g8 | b8) == 0)
            {
                return 0;
            }

            int r5 = r8 >> 3;
            int g5 = g8 >> 3;
            int b5 = b8 >> 3;
            if (r5 == 0 && g5 == 0 && b5 == 0)
            {
                return 1;
            }

            return (ushort)((r5 << 10) | (g5 << 5) | b5);
        }

        // Rounding alternative: ((c*31 + 127) / 255). Same clamp rule as the shift version.
        public static ushort ColorToHueRounded(int r8, int g8, int b8)
        {
            if ((r8 | g8 | b8) == 0)
            {
                return 0;
            }

            int r5 = (r8 * 31 + 127) / 255;
            int g5 = (g8 * 31 + 127) / 255;
            int b5 = (b8 * 31 + 127) / 255;
            if (r5 == 0 && g5 == 0 && b5 == 0)
            {
                return 1;
            }

            return (ushort)((r5 << 10) | (g5 << 5) | b5);
        }

        public static void HueExtract5(ushort hue, out int r5, out int g5, out int b5)
        {
            r5 = (hue & 0x7c00) >> 10;
            g5 = (hue & 0x03e0) >> 5;
            b5 = hue & 0x001f;
        }

        // Canonical 5-bit -> 8-bit expansion: replicate top 3 bits into the low ones.
        // Equivalent to round(c5 * 255 / 31). 0->0, 31->255, monotonic.
        public static int Expand5To8(int c5)
        {
            return (c5 << 3) | (c5 >> 2);
        }
    }
}