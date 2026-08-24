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

namespace UoFiddler.Controls.Classes
{
    public class PackedItemOutput
    {
        public PackedMeta Meta { get; set; }
        public List<PackedItemEntry> Items { get; set; }
    }
}
