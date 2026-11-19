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

using System.Collections.Generic;
using System.Drawing;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// What a tool draws over the multi. The canvas decides how it looks.
    /// </summary>
    internal interface IToolOverlay
    {
        /// <summary>A translucent tile where a tool would place it.</summary>
        void Ghost(ushort id, int x, int y, int z);

        /// <summary>An existing tile, marked.</summary>
        void Highlight(MultiTile tile, TileHighlight kind);

        /// <summary>A selection box, in the canvas space of <see cref="IEditorContext.TilesInRect"/>.</summary>
        void Marquee(Rectangle rect);

        /// <summary>A line through canvas points, joined back to the start when closed.</summary>
        void Polyline(IReadOnlyList<Point> points, bool closed);

        /// <summary>A closed outline through canvas points, drawn as strongly as its tone asks.</summary>
        void Outline(IReadOnlyList<Point> points, OverlayTone tone);

        /// <summary>A cell tinted on the plane at height z.</summary>
        void CellMark(int x, int y, int z, OverlayTone tone);
    }
}
