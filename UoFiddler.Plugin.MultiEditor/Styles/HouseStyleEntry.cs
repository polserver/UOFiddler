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

namespace UoFiddler.Plugin.MultiEditor.Styles
{
    /// <summary>One <c>Style</c> element of the style file: what it says about one house style.</summary>
    internal sealed class HouseStyleEntry
    {
        public const string LayoutRun = "Run";
        public const string LayoutPaired = "Paired";
        public const string LayoutCycle = "Cycle";

        public HousePartKind Kind { get; set; }

        /// <summary>A piece of the style: the entry is about the first style of its kind that lists it.</summary>
        public ushort Anchor { get; set; }

        /// <summary>True for a style the tables do not have: it is made from the entry's pieces.</summary>
        public bool IsNew { get; set; }

        /// <summary>Pieces of further styles this one takes the place of.</summary>
        public List<ushort> Replaces { get; } = new List<ushort>();

        public string Name { get; set; }

        public string Group { get; set; }

        public string Family { get; set; }

        public bool? Hidden { get; set; }

        /// <summary>How a wall run is laid: <see cref="LayoutRun"/>, <see cref="LayoutPaired"/> or <see cref="LayoutCycle"/>. Null leaves it.</summary>
        public string Layout { get; set; }

        public bool? Borrows { get; set; }

        /// <summary>The style's pieces by column label, in place of the table's. Null keeps the table's.</summary>
        public List<(string Role, ushort Id)> Pieces { get; set; }

        public (HousePartKind Kind, ushort Anchor, bool IsNew) Key => (Kind, Anchor, IsNew);
    }
}
