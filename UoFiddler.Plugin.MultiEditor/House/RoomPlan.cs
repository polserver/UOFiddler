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

namespace UoFiddler.Plugin.MultiEditor.House
{
    /// <summary>The rooms of a wing on one storey: their cells, and which of them a doorway joins.</summary>
    internal sealed class RoomPlan
    {
        /// <summary>The rooms as cell rectangles, both ends included.</summary>
        public List<(int X0, int Y0, int X1, int Y1)> Rooms { get; } = new List<(int, int, int, int)>();

        /// <summary>Pairs of rooms with a doorway between them. Every room is reached through them.</summary>
        public List<(int A, int B)> Links { get; } = new List<(int, int)>();

        /// <summary>The room the others open to (the corridor, the hall), or -1 when there is none.</summary>
        public int Main { get; set; } = -1;
    }
}
