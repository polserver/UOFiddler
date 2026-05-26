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

using System.Drawing;

namespace UoFiddler.Plugin.Compare.Classes
{
    public static class CompareColors
    {
        /// <summary>
        /// Perceived-luminance test so highlighted-row text stays readable on any selection color.
        /// </summary>
        public static bool IsDarkColor(Color c) => (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) < 128;

        /// <summary>
        /// Returns a readable text brush (white on dark, black on light) for the given background.
        /// </summary>
        public static Brush ContrastBrush(Color background) => IsDarkColor(background) ? Brushes.White : Brushes.Black;
    }
}
