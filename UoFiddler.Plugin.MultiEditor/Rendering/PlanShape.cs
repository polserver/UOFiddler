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

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// How a tile shows in the plan view: as a wall on the cell's +Y (South) or +X (East) edge,
    /// a post at the +X/+Y corner, a door in an edge, or none of these (the cell's fill).
    /// </summary>
    [Flags]
    internal enum PlanShape
    {
        Fill = 0,
        South = 1,
        East = 2,
        Post = 4,
        Door = 8
    }
}
