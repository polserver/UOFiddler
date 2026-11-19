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

namespace UoFiddler.Plugin.MultiEditor.Storage
{
    /// <summary>A named page of favorite tiles.</summary>
    internal sealed class FavoritePage
    {
        public FavoritePage(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        /// <summary>Tile ids in the order they were added. The tile grid shows this list.</summary>
        public List<int> Tiles { get; } = new List<int>();
    }
}
