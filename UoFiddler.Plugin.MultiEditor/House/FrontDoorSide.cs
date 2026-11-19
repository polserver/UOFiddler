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

namespace UoFiddler.Plugin.MultiEditor.House
{
    /// <summary>
    /// A house being designed, beside its wings: the materials of each storey and what was changed
    /// by hand (partitions, openings, doors, windows, floor areas, stairs). <see cref="Build"/>
    /// turns wings and draft into tiles. A wing's box is its wall cells, as the wall tool drags
    /// them: walls stand on the +Y / +X edges of their cells, so the rooms inside are the box
    /// without its first row and column.
    /// </summary>
    /// <summary>The outer wall of the first wing that gets the front door.</summary>
    internal enum FrontDoorSide
    {
        None,
        South,
        East
    }
}
