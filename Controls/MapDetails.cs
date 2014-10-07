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
using System.Drawing;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class MapDetails : Form
    {
        public MapDetails(Ultima.Map currmap, Point point)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            this.TopMost = true;
            Tile currtile = currmap.Tiles.GetLandTile(point.X, point.Y);
            richTextBox.AppendText(String.Format("X: {0} Y: {1}\n\n", point.X, point.Y));
            richTextBox.AppendText("LandTile:\n");
            richTextBox.AppendText(String.Format("{0}: 0x{1:X} Altitute: {2}\n\n",
                                                 Ultima.TileData.LandTable[currtile.ID].Name,
                                                 currtile.ID,
                                                 currtile.Z));
            HuedTile[] currStatics = currmap.Tiles.GetStaticTiles(point.X, point.Y);
            richTextBox.AppendText("Statics:\n");

            foreach (HuedTile currstatic in currStatics)
            {
                ushort id = (ushort)currstatic.ID;
                richTextBox.AppendText(String.Format("{0}: 0x{1:X} Hue: {2} Altitute: {3}\n",
                                                     Ultima.TileData.ItemTable[id].Name,
                                                     id,
                                                     currstatic.Hue,
                                                     currstatic.Z));
            }
        }
    }
}