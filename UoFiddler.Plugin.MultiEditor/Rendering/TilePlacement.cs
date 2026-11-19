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

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>Where a tile sits in its cell, which decides how it moves when a group is turned.</summary>
    internal enum TilePlacement
    {
        /// <summary>Fills or stands in the cell (floors, roofs, furniture).</summary>
        Cell,

        /// <summary>A wall along the cell's +Y edge.</summary>
        SouthEdge,

        /// <summary>A wall along the cell's +X edge.</summary>
        EastEdge,

        /// <summary>A post at the cell's +X/+Y corner.</summary>
        Vertex,

        /// <summary>
        /// A roof piece. Its art is drawn half a cell up the screen, so it covers the area around
        /// the cell's -X/-Y corner: the official houses put their roofs one cell further +X/+Y
        /// than the walls for that reason.
        /// </summary>
        Lifted
    }
}
