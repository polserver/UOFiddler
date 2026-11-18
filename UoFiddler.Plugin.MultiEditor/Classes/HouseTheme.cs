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

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// A house theme: the styles of every part of a house, picked together. Each style is named
    /// by its anchor piece (<see cref="HouseStyle.Anchor"/>); 0 leaves that part as it is, except
    /// for the foundation (0 is the usual one, light brick).
    /// Themes come from the style file; the ones the user saved are in the user's copy of it.
    /// </summary>
    internal sealed class HouseTheme
    {
        public string Name { get; set; }

        public ushort Walls { get; set; }

        public ushort Floor { get; set; }

        public ushort Roof { get; set; }

        public ushort FlatRoof { get; set; }

        public ushort Door { get; set; }

        public ushort Stairs { get; set; }

        public ushort Foundation { get; set; }

        /// <summary>Rows of the foundation's pieces, one on the other, and how far below the ground floor the foundation starts.</summary>
        public int FoundationTiers { get; set; } = 1;

        public int FoundationHeight { get; set; } = HouseDraft.UsualFoundationHeight;

        /// <summary>True for a theme from the user's copy of the style file.</summary>
        public bool IsUser { get; set; }

        public override string ToString() => Name;
    }

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
