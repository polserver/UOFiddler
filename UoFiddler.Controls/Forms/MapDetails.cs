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

using System.Drawing;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MapDetails : Form
    {
        public MapDetails(Map currentMap, Point point)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            TopMost = true;

            Tile currentTile = currentMap.Tiles.GetLandTile(point.X, point.Y);
            richTextBox.AppendText($"X: {point.X} Y: {point.Y}\n\n");
            richTextBox.AppendText("LandTile:\n");
            richTextBox.AppendText($"{TileData.LandTable[currentTile.ID].Name}: 0x{currentTile.ID:X} Altitude: {currentTile.Z}\n\n");
            HuedTile[] staticsAtPoint = currentMap.Tiles.GetStaticTiles(point.X, point.Y);
            richTextBox.AppendText("Statics:\n");
            foreach (HuedTile @static in staticsAtPoint)
            {
                ushort id = @static.ID;
                richTextBox.AppendText($"{TileData.ItemTable[id].Name}: 0x{id:X} Hue: {@static.Hue} Altitude: {@static.Z}\n");
            }
        }
    }
}