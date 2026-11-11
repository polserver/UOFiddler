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
    public sealed class TileCategory
    {
        public int Index { get; }
        public string Name { get; set; }
        public int Tid { get; set; }
        public List<TileStyle> Styles { get; } = new List<TileStyle>();
        public TileGroupFile File { get; internal set; }

        public TileCategory(int index, string name)
        {
            Index = index;
            Name = name ?? string.Empty;
        }

        public void AddStyle(TileStyle style)
        {
            style.Category = this;
            Styles.Add(style);
        }
    }
}
