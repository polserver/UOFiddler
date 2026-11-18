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

        /// <summary>True for a theme from the user's copy of the style file.</summary>
        public bool IsUser { get; set; }

        public override string ToString() => Name;
    }
}
