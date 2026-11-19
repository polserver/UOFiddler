/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Drawing;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Toolbar icons drawn from the Windows icon font, so the plugin ships no image files.
    /// </summary>
    internal static class Glyphs
    {
        public const string Select = "\uE8B0";
        public const string Lasso = "\uF408";
        public const string Draw = "\uE70F";
        public const string Remove = "\uE75C";
        public const string Altitude = "\uE8CB";
        public const string Pipette = "\uEF3C";
        public const string Transparent = "\uE7B3";
        public const string RectFill = "\uE739";
        public const string Line = "\uE738";
        public const string Wall = "\uE80F";
        public const string Floor = "\uE80A";
        public const string Door = "\uE8A7";
        public const string Stair = "\uE9D5";
        public const string Roof = "\uE80F";
        public const string Foundation = "\uE7C4";
        public const string New = "\uE7C3";
        public const string Open = "\uE8E5";
        public const string Save = "\uE74E";
        public const string Undo = "\uE7A7";
        public const string Redo = "\uE7A6";
        public const string Cut = "\uE8C6";
        public const string Copy = "\uE8C8";
        public const string Paste = "\uE77F";
        public const string Delete = "\uE74D";
        public const string Grid = "\uE80A";
        public const string ZoomIn = "\uE8A3";
        public const string ZoomOut = "\uE71F";
        public const string Fit = "\uE9A6";
        public const string Center = "\uE81D";

        /// <summary>Zoom 100%: Segoe MDL2 has no actual-size icon, so it is the text "1:1".</summary>
        public const string ActualSize = "1:1";
        public const string RotateLeft = "\uE7AD";
        public const string RotateRight = "\uE7AD";
        public const string Help = "\uE897";

        public static Bitmap Render(string glyph, int size, Color color)
        {
            var bitmap = new Bitmap(size, size);
            using Graphics g = Graphics.FromImage(bitmap);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // Icons are private-use characters of the icon font; anything else is short text drawn in a UI font.
            bool icon = glyph.Length > 0 && glyph[0] >= '' && glyph[0] <= '';
            using var font = icon
                ? new Font("Segoe MDL2 Assets", size * 0.72f, GraphicsUnit.Pixel)
                : new Font("Segoe UI", size * 0.55f, FontStyle.Bold, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(color);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(glyph, font, brush, new RectangleF(0, 0, size, size), format);
            return bitmap;
        }
    }
}
