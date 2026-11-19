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
using System.Collections.Generic;

namespace UoFiddler.Plugin.MultiEditor.Styles
{
    /// <summary>
    /// One row of a client house design table (walls.txt, floors.txt ...): a named set of pieces,
    /// each under the column label the client uses (South1, Corner, F3, MultiNorth ...).
    /// </summary>
    internal sealed class HouseStyle
    {
        private readonly Dictionary<string, ushort> _slots = new Dictionary<string, ushort>();

        public HouseStyle(HousePartKind kind, string group, string name)
        {
            Kind = kind;
            Group = group;
            Name = name;
        }

        public HousePartKind Kind { get; }

        /// <summary>The category name (the first style's comment), grouping a material's styles.</summary>
        public string Group { get; internal set; }

        public string Name { get; internal set; }

        /// <summary>The material, shared by the styles of every kind made of it. From the style file; may be empty.</summary>
        public string Family { get; internal set; } = string.Empty;

        /// <summary>True for a style the style file keeps out of the lists.</summary>
        public bool Hidden { get; internal set; }

        /// <summary>A piece that tells the style from every other of its kind: how files refer to it. 0 when it has none.</summary>
        public ushort Anchor { get; internal set; }

        /// <summary>False for a wall style that takes no post or corner from the other rows of its category.</summary>
        internal bool Borrows { get; set; } = true;

        /// <summary>The table category the style was read from, counted over all tables.</summary>
        internal int Category { get; set; } = -1;

        /// <summary>True for a wall style of a category sorted by its art.</summary>
        internal bool ByArt { get; set; }

        public IReadOnlyDictionary<string, ushort> Slots => _slots;

        /// <summary>
        /// True for a wall style whose run pieces are the two halves of an ornament (the shadow
        /// banner): South1 and South3 along X, East3 and East1 along Y, with South2 and East2 the
        /// plain wall. A run is then laid as pairs, not as start, middle and end.
        /// </summary>
        public bool Paired { get; internal set; }

        /// <summary>
        /// For a wall style that is a set of different panels (the carved red marble): how many
        /// it has along X (CycleSouth1 ...) and along Y (CycleEast1 ...). A run is laid as one
        /// panel after another, starting again after the last. 0 for an ordinary style.
        /// </summary>
        public int CycleSouth { get; internal set; }

        public int CycleEast { get; internal set; }

        /// <summary>A piece to show for the style in lists.</summary>
        public ushort IconId { get; private set; }

        /// <summary>The piece under a column label, or 0 when the style has none.</summary>
        /// <summary>
        /// The tile of a piece, or 0 when the style has none. A tile past the client's tiledata (a
        /// style read from another client) counts as none; the Multi columns hold multi ids.
        /// </summary>
        public ushort Get(string label) =>
            _slots.TryGetValue(label, out ushort id) && (InTileData(id) || label.StartsWith("Multi", StringComparison.Ordinal)) ? id : (ushort)0;

        /// <summary>True when the client's tiledata has the item.</summary>
        public static bool InTileData(int id) => Ultima.TileData.ItemTable is { } items && id >= 0 && id < items.Length;

        internal void Add(string label, ushort id)
        {
            _slots[label] = id;
            if (IconId == 0)
            {
                IconId = id;
            }
        }

        internal void Clear()
        {
            _slots.Clear();
            IconId = 0;
        }

        public override string ToString() => Name;
    }
}
