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

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Where the draw tools put a new tile.
    /// </summary>
    internal enum PlacementMode
    {
        /// <summary>On top of the hovered tile, or on an empty cell on the work plane (Z 0 with all floors and the plane not moved).</summary>
        Stack,

        /// <summary>At the hovered tile's own Z, replacing tiles of the same kind (flat or tall) there.</summary>
        Replace,

        /// <summary>At the work plane's Z, in the cell under the cursor on that plane.</summary>
        FixedZ
    }
}
