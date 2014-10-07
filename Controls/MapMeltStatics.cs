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

namespace FiddlerControls
{
    public partial class MapMeltStatics : Form
    {
        FiddlerControls.Map MapParent;
        Ultima.Map Map;
        public MapMeltStatics(FiddlerControls.Map parent, Ultima.Map map)
        {
            MapParent = parent;
            Map = map;
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            numericUpDownX1.Maximum = map.Width;
            numericUpDownX2.Maximum = map.Width;
            numericUpDownY1.Maximum = map.Height;
            numericUpDownY2.Maximum = map.Height;
        }

        private void OnClickMelt(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.CheckPathExists = true;
            dialog.Title = "Choose the file to save to";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
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

            int count=1;
            using (StreamWriter Tex = new StreamWriter(new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write), System.Text.Encoding.GetEncoding(1252)))
            {
                for (int x = blockx1; x <= blockx2; ++x)
                {
                    for (int y = blocky1; y <= blocky2; ++y)
                    {
                        Ultima.HuedTile[][][] tiles = Map.Tiles.GetStaticBlock(x, y, false);
                        for (int ix = 0; ix < 8; ++ix)
                        {
                            for (int iy = 0; iy < 8; ++iy)
                            {
                                foreach (Ultima.HuedTile tile in tiles[ix][iy])
                                {
                                    Tex.WriteLine(String.Format("SECTION WORLDITEM {0}",count));
                                    Tex.WriteLine("{");
                                    Tex.WriteLine("  NAME #");
                                    Tex.WriteLine(String.Format("  ID {0}", tile.ID));
                                    Tex.WriteLine(String.Format("  X {0}", (x << 3) + ix));
                                    Tex.WriteLine(String.Format("  Y {0}", (y << 3) + iy));
                                    Tex.WriteLine(String.Format("  Z {0}", tile.Z));
                                    Tex.WriteLine(String.Format("  COLOR {0}", tile.Hue));
                                    Tex.WriteLine("  CONT -1");
                                    Tex.WriteLine("  TYPE 255");
                                    Tex.WriteLine("}");
                                    ++count;
                                }
                            }
                        }
                        Map.Tiles.RemoveStaticBlock(x,y);
                    }
                }
            }
            dialog.Dispose();
            Map.ResetCache();
            MessageBox.Show("Done", "Melt Static", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            MapParent.RefreshMap();
        }
    }
}
