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
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class MapMeltStaticsForm : Form
    {
        private readonly MapControl _mapControlParent;
        private readonly Map _map;

        public MapMeltStaticsForm(MapControl controlParent, Map map)
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

        private void OnClickMelt(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                CheckPathExists = true,
                Title = "Choose the file to save to"
            };
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

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

            int count = 1;
            using (StreamWriter tex = new StreamWriter(new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write), System.Text.Encoding.GetEncoding(1252)))
            {
                for (int x = blockx1; x <= blockx2; ++x)
                {
                    for (int y = blocky1; y <= blocky2; ++y)
                    {
                        HuedTile[][][] tiles = _map.Tiles.GetStaticBlock(x, y, false);
                        for (int ix = 0; ix < 8; ++ix)
                        {
                            for (int iy = 0; iy < 8; ++iy)
                            {
                                foreach (HuedTile tile in tiles[ix][iy])
                                {
                                    tex.WriteLine("SECTION WORLDITEM {0}", count);
                                    tex.WriteLine("{");
                                    tex.WriteLine("  NAME #");
                                    tex.WriteLine("  ID {0}", tile.Id);
                                    tex.WriteLine("  X {0}", (x << 3) + ix);
                                    tex.WriteLine("  Y {0}", (y << 3) + iy);
                                    tex.WriteLine("  Z {0}", tile.Z);
                                    tex.WriteLine("  COLOR {0}", tile.Hue);
                                    tex.WriteLine("  CONT -1");
                                    tex.WriteLine("  TYPE 255");
                                    tex.WriteLine("}");
                                    ++count;
                                }
                            }
                        }
                        _map.Tiles.RemoveStaticBlock(x, y);
                    }
                }
            }

            dialog.Dispose();
            _map.ResetCache();
            MessageBox.Show("Done", "Melt Static", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            _mapControlParent.RefreshMap();
        }
    }
}
