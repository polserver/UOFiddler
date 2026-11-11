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

namespace UoFiddler.Controls.Classes.TilesInfo
{
    public sealed class TileStyle
    {
        public int Index { get; }
        public string Name { get; set; }
        public List<TileSlot> Slots { get; } = new List<TileSlot>();
        public TileCategory Category { get; internal set; }

        public TileStyle(int index, string name)
        {
            Index = index;
            Name = name ?? string.Empty;
        }

        public void AddSlot(TileSlot slot)
        {
            slot.Style = this;
            Slots.Add(slot);
        }
    }
}
