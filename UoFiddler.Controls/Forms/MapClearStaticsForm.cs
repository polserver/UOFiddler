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
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class MapClearStaticsForm : Form
    {
        private readonly MapControl _mapControlParent;
        private readonly Ultima.Map _map;

        public MapClearStaticsForm(MapControl controlParent, Ultima.Map map)
        {
            _mapControlParent = controlParent;
            _map = map;
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            numericUpDownX1.Maximum = map.Width;
            numericUpDownX2.Maximum = map.Width;
            numericUpDownY1.Maximum = map.Height;
            numericUpDownY2.Maximum = map.Height;
        }

        private void OnClickClear(object sender, EventArgs e)
        {
            int blockx1 = (int)numericUpDownX1.Value;
            int blockx2 = (int)numericUpDownX2.Value;
            int blocky1 = (int)numericUpDownY1.Value;
            int blocky2 = (int)numericUpDownY2.Value;
            int temp;
            if (blockx1 > blockx2)
            {
                temp = blockx1;
                blockx1 = blockx2;
                blockx2 = temp;
            }
            if (blocky1 > blocky2)
            {
                temp = blocky1;
                blocky1 = blocky2;
                blocky2 = temp;
            }
            blockx1 >>= 3;
            blockx2 >>= 3;
            blocky1 >>= 3;
            blocky2 >>= 3;

            for (int x = blockx1; x <= blockx2; ++x)
            {
                for (int y = blocky1; y <= blocky2; ++y)
                {
                    //HuedTile[][][] tiles = _map.Tiles.GetStaticBlock(x, y, false); // TODO: unused variable? do we need to call GetStaticBlock() here?
                    _map.Tiles.RemoveStaticBlock(x, y);
                }
            }
            _map.ResetCache();
            MessageBox.Show("Done", "Clear Static", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            _mapControlParent.RefreshMap();
        }
    }
}
