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
    /// <summary>What closes a gable.</summary>
    internal enum RoofGableFill
    {
        /// <summary>3 high cap pieces matching the walls.</summary>
        Caps,

        /// <summary>Whole wall pieces as far as they fit under the slope, caps for the rest: for big, high roofs.</summary>
        Walls
    }
}
