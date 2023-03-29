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
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MapClearStaticsForm : Form
    {
        private readonly Action _refreshMap;
        private readonly Ultima.Map _map;

        public MapClearStaticsForm(Action refreshMap, Ultima.Map map)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _refreshMap = refreshMap;
            _map = map;

            numericUpDownX1.Maximum = map.Width;
            numericUpDownX2.Maximum = map.Width;
            numericUpDownY1.Maximum = map.Height;
            numericUpDownY2.Maximum = map.Height;
        }

        private void OnClickClear(object sender, EventArgs e)
        {
            int blockX1 = (int)numericUpDownX1.Value;
            int blockX2 = (int)numericUpDownX2.Value;

            int blockY1 = (int)numericUpDownY1.Value;
            int blockY2 = (int)numericUpDownY2.Value;

            int temp;

            if (blockX1 > blockX2)
            {
                temp = blockX1;

                blockX1 = blockX2;
                blockX2 = temp;
            }

            if (blockY1 > blockY2)
            {
                temp = blockY1;

                blockY1 = blockY2;
                blockY2 = temp;
            }

            blockX1 >>= 3;
            blockX2 >>= 3;

            blockY1 >>= 3;
            blockY2 >>= 3;

            for (int x = blockX1; x <= blockX2; ++x)
            {
                for (int y = blockY1; y <= blockY2; ++y)
                {
                    //HuedTile[][][] tiles = _map.Tiles.GetStaticBlock(x, y, false); // TODO: unused variable? do we need to call GetStaticBlock() here?
                    _map.Tiles.RemoveStaticBlock(x, y);
                }
            }

            _map.ResetCache();

            MessageBox.Show("Done", "Clear Static", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

            _refreshMap();
        }
    }
}
