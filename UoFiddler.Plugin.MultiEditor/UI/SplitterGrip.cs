/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Makes splitters visible: a row of dots in the middle of the bar, the usual sign that it
    /// can be dragged. A bare splitter is the same colour as the panels and easy to miss.
    /// </summary>
    internal static class SplitterGrip
    {
        public const int Thickness = 7;

        /// <summary>Draws the dots along the middle of a splitter bar.</summary>
        public static void Draw(Graphics g, Rectangle bar)
        {
            bool wide = bar.Width >= bar.Height;
            const int dots = 5, spacing = 6, size = 2;
            int length = (dots - 1) * spacing;
            int x = bar.X + (wide ? (bar.Width - length) / 2 : (bar.Width - size) / 2);
            int y = bar.Y + (wide ? (bar.Height - size) / 2 : (bar.Height - length) / 2);
            using var brush = new SolidBrush(SystemColors.GrayText);
            for (int i = 0; i < dots; i++)
            {
                g.FillRectangle(brush, x + (wide ? i * spacing : 0), y + (wide ? 0 : i * spacing), size, size);
            }
        }

        /// <summary>Gives a split container's bar the dots.</summary>
        public static void Attach(SplitContainer container)
        {
            container.SplitterWidth = Thickness;
            container.Paint += (_, e) => Draw(e.Graphics, container.SplitterRectangle);
            container.SplitterMoved += (_, _) => container.Invalidate();
            container.Resize += (_, _) => container.Invalidate();
        }
    }
}
