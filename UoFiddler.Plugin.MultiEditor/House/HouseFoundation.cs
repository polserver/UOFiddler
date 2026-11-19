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
    /// A foundation as the client's houses have them: a low wall style around the house, in one
    /// or two rows, with the ground floor a given height above where it starts. From the style
    /// file; offered when the client has the wall style.
    /// </summary>
    internal sealed class HouseFoundation
    {
        public string Name { get; set; }

        /// <summary>Anchor piece of the wall style; 0 for the usual one (light brick quarter walls).</summary>
        public ushort Style { get; set; }

        public int Tiers { get; set; } = 1;

        public int Height { get; set; } = HouseDraft.UsualFoundationHeight;

        public override string ToString() => Name;
    }
}
