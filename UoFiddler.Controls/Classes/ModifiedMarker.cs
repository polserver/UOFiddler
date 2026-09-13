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

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Draws the corner wedge that flags a graphic edited since the file type was loaded or last
    /// saved. Shared so the Items, LandTiles, Gumps and Textures tabs all mark entries the same way.
    /// </summary>
    public static class ModifiedMarker
    {
        private const int MaxSize = 9;
        private const int MinSize = 4;

        // Kept as statics: Draw runs once per visible tile on every paint, so nothing may allocate.
        // The outline matters because Options.PreviewBackgroundColor is user configurable and the
        // wedge would otherwise vanish against an orange background.
        private static readonly Brush _fill = new SolidBrush(Color.FromArgb(255, 120, 0));
        private static readonly Pen _outline = new Pen(Color.FromArgb(40, 40, 40));

        /// <summary>
        /// Draws the wedge into the top left corner of <paramref name="bounds"/>.
        /// </summary>
        public static void Draw(Graphics graphics, Rectangle bounds)
        {
            int size = Math.Min(MaxSize, Math.Min(bounds.Width, bounds.Height));
            if (size < MinSize)
            {
                return;
            }

            Point corner = new Point(bounds.X, bounds.Y);
            Point right = new Point(bounds.X + size, bounds.Y);
            Point down = new Point(bounds.X, bounds.Y + size);

            graphics.FillPolygon(_fill, new[] { corner, right, down });
            graphics.DrawLine(_outline, right, down);
        }
    }
}
