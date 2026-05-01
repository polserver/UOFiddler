// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.Classes
{
    public class TileDataSyncChange
    {
        public TileDataSyncKind Kind { get; set; }
        public int Number { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
    }
}