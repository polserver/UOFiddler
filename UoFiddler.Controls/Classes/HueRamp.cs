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
using System.Drawing;
using Ultima.Helpers;

namespace UoFiddler.Controls.Classes
{
    /// <summary>How a gradient moves between its two end colors.</summary>
    public enum HueGradientCurve
    {
        Linear,
        EaseIn,
        EaseOut,
        Smooth,
    }

    /// <summary>
    /// The hue editor's color tools. They work on a hue's 32 colors (15-bit values, no high bit)
    /// over an inclusive cell range and do not touch any UI.
    /// </summary>
    public static class HueRamp
    {
        public const int ColorCount = 32;

        /// <summary>
        /// Packs a color into a 15-bit hue color. Pure black packs to 0, which the hue loader
        /// treats as invalid and reads back as 1, so it is clamped to 1 here as well.
        /// </summary>
        public static ushort Pack(Color color)
        {
            ushort value = HueHelpers.ColorToHue(color);
            return value == 0 ? (ushort)1 : value;
        }

        /// <summary>True when <paramref name="color"/> is black that <see cref="Pack"/> clamps.</summary>
        public static bool IsClampedBlack(Color color) => HueHelpers.ColorToHue(color) == 0;

        /// <summary>The color as it is stored: snapped to the 32 levels each channel has.</summary>
        public static Color Snap(Color color) => HueHelpers.HueToColor(Pack(color));

        /// <summary>Snaps one 0-255 channel value to the nearest stored level's 0-255 value.</summary>
        public static int SnapChannel(int value) => HueHelpers.Expand5To8(Math.Clamp(value, 0, 255) >> 3);

        /// <summary>Fills the cells between <paramref name="start"/> and <paramref name="end"/>, keeping both ends.</summary>
        public static bool Fill(ushort[] colors, int start, int end, HueGradientCurve curve)
        {
            Order(ref start, ref end);
            if (end - start < 2)
            {
                return false;
            }

            Interpolate(colors, start, end, HueHelpers.HueToColor(colors[start]), HueHelpers.HueToColor(colors[end]), curve);
            return true;
        }

        /// <summary>Sets both ends to the given colors and fills between them.</summary>
        public static bool Gradient(ushort[] colors, int start, int end, Color from, Color to, HueGradientCurve curve)
        {
            Order(ref start, ref end, ref from, ref to);
            if (end <= start)
            {
                return false;
            }

            colors[start] = Pack(from);
            colors[end] = Pack(to);
            Interpolate(colors, start, end, from, to, curve);
            return true;
        }

        /// <summary>
        /// Builds a whole hue from one color: index 0 is darkest, index 31 lightest. The base color
        /// sits at the cell that matches its own lightness, and the ends are pulled towards black
        /// by <paramref name="darkness"/> and towards white by <paramref name="lightness"/> (0-1).
        /// </summary>
        public static void FromBaseColor(ushort[] colors, Color baseColor, double darkness, double lightness)
        {
            ToHsl(baseColor, out double h, out double s, out double l);
            darkness = Math.Clamp(darkness, 0, 1);
            lightness = Math.Clamp(lightness, 0, 1);

            int anchor = Math.Clamp((int)Math.Round(l * (ColorCount - 1)), 4, ColorCount - 5);
            double darkL = l * (1 - darkness);
            double lightL = l + ((1 - l) * lightness);

            for (int i = 0; i < ColorCount; i++)
            {
                double li = i <= anchor
                    ? Lerp(darkL, l, (double)i / anchor)
                    : Lerp(l, lightL, (double)(i - anchor) / (ColorCount - 1 - anchor));
                colors[i] = Pack(FromHsl(h, s, li));
            }

            colors[anchor] = Pack(baseColor);
        }

        /// <summary>Reverses the order of the colors in the range.</summary>
        public static void Reverse(ushort[] colors, int start, int end)
        {
            Order(ref start, ref end);
            Array.Reverse(colors, start, end - start + 1);
        }

        /// <summary>
        /// Shifts every color in the range: hue by degrees, saturation by a percentage of itself,
        /// lightness by percentage points, then adds the RGB offsets. All results are clamped.
        /// </summary>
        public static void Adjust(ushort[] colors, int start, int end, double hueShift, double saturationPct, double lightnessPct, int red, int green, int blue)
        {
            Order(ref start, ref end);
            for (int i = start; i <= end; i++)
            {
                Color color = HueHelpers.HueToColor(colors[i]);
                if (hueShift != 0 || saturationPct != 0 || lightnessPct != 0)
                {
                    ToHsl(color, out double h, out double s, out double l);
                    h = (((h + hueShift) % 360) + 360) % 360;
                    s = Math.Clamp(s * (1 + (saturationPct / 100)), 0, 1);
                    l = Math.Clamp(l + (lightnessPct / 100), 0, 1);
                    color = FromHsl(h, s, l);
                }

                colors[i] = Pack(Color.FromArgb(
                    Math.Clamp(color.R + red, 0, 255),
                    Math.Clamp(color.G + green, 0, 255),
                    Math.Clamp(color.B + blue, 0, 255)));
            }
        }

        public static double ApplyCurve(double t, HueGradientCurve curve) => curve switch
        {
            HueGradientCurve.EaseIn => t * t,
            HueGradientCurve.EaseOut => 1 - ((1 - t) * (1 - t)),
            HueGradientCurve.Smooth => t * t * (3 - (2 * t)),
            _ => t,
        };

        /// <summary>RGB to hue (0-360), saturation and lightness (0-1).</summary>
        public static void ToHsl(Color color, out double h, out double s, out double l)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            l = (max + min) / 2;
            if (delta == 0)
            {
                h = 0;
                s = 0;
                return;
            }

            s = delta / (1 - Math.Abs((2 * l) - 1));
            if (max == r)
            {
                h = 60 * (((g - b) / delta) % 6);
            }
            else if (max == g)
            {
                h = 60 * (((b - r) / delta) + 2);
            }
            else
            {
                h = 60 * (((r - g) / delta) + 4);
            }

            if (h < 0)
            {
                h += 360;
            }

            s = Math.Clamp(s, 0, 1);
        }

        /// <summary>Hue (0-360), saturation and lightness (0-1) to RGB.</summary>
        public static Color FromHsl(double h, double s, double l)
        {
            h = ((h % 360) + 360) % 360;
            s = Math.Clamp(s, 0, 1);
            l = Math.Clamp(l, 0, 1);

            double c = (1 - Math.Abs((2 * l) - 1)) * s;
            double x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
            double m = l - (c / 2);

            (double r, double g, double b) = h switch
            {
                < 60 => (c, x, 0.0),
                < 120 => (x, c, 0.0),
                < 180 => (0.0, c, x),
                < 240 => (0.0, x, c),
                < 300 => (x, 0.0, c),
                _ => (c, 0.0, x),
            };

            return Color.FromArgb(ToByte(r + m), ToByte(g + m), ToByte(b + m));
        }

        private static void Interpolate(ushort[] colors, int start, int end, Color from, Color to, HueGradientCurve curve)
        {
            int diff = end - start;
            for (int i = 1; i < diff; i++)
            {
                double t = ApplyCurve((double)i / diff, curve);
                colors[start + i] = Pack(Color.FromArgb(
                    (int)Math.Round(Lerp(from.R, to.R, t)),
                    (int)Math.Round(Lerp(from.G, to.G, t)),
                    (int)Math.Round(Lerp(from.B, to.B, t))));
            }
        }

        private static double Lerp(double a, double b, double t) => a + ((b - a) * t);

        private static int ToByte(double value) => Math.Clamp((int)Math.Round(value * 255), 0, 255);

        private static void Order(ref int start, ref int end)
        {
            if (start > end)
            {
                (start, end) = (end, start);
            }

            start = Math.Clamp(start, 0, ColorCount - 1);
            end = Math.Clamp(end, 0, ColorCount - 1);
        }

        private static void Order(ref int start, ref int end, ref Color from, ref Color to)
        {
            if (start > end)
            {
                (start, end) = (end, start);
                (from, to) = (to, from);
            }

            start = Math.Clamp(start, 0, ColorCount - 1);
            end = Math.Clamp(end, 0, ColorCount - 1);
        }
    }
}
