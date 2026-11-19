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
    /// <summary>The shape of one roof wing.</summary>
    internal enum RoofShape
    {
        /// <summary>Two slopes meeting in a ridge.</summary>
        Gable,

        /// <summary>Four slopes: the ends slope too, instead of standing as gables.</summary>
        Hip,

        /// <summary>
        /// No slopes: a floor on top of the walls, as the towers have. Only a house wing can have
        /// it; the house draft lays it, not this planner.
        /// </summary>
        Flat
    }
}
