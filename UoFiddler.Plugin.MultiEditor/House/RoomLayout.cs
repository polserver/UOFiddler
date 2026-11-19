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
    /// <summary>How the rooms of a house wing are laid out.</summary>
    internal enum RoomLayout
    {
        /// <summary>Side by side along the wing's longer side.</summary>
        Row,

        /// <summary>Two rows of rooms.</summary>
        Grid,

        /// <summary>A corridor along the wing's longer side with rooms on both sides of it.</summary>
        Corridor,

        /// <summary>One large room, and the others in a row across the wing's end.</summary>
        Hall
    }
}
