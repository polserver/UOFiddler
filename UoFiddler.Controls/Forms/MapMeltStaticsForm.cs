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

namespace UoFiddler.Controls.Forms
{
    public partial class MapMeltStaticsForm : Form
    {
        private readonly Action _refreshMap;
        private readonly Map _map;
        private Func<string, string?>? _localizationGetter;

        public MapMeltStaticsForm(Action refreshMap, Map map)
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

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.MapMeltStaticsForm.Title") ?? "Melt Statics";
            label2.Text = _localizationGetter("Forms.MapMeltStaticsForm.X1Label") ?? "X1";
            label3.Text = _localizationGetter("Forms.MapMeltStaticsForm.Y1Label") ?? "Y1";
            label4.Text = _localizationGetter("Forms.MapMeltStaticsForm.X2Label") ?? "X2";
            label5.Text = _localizationGetter("Forms.MapMeltStaticsForm.Y2Label") ?? "Y2";
            button1.Text = _localizationGetter("Forms.MapMeltStaticsForm.MeltButton") ?? "Melt";
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

            int count = 1;

            using (StreamWriter tex = new StreamWriter(new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write), System.Text.Encoding.GetEncoding(1252)))
            {
                for (int x = blockX1; x <= blockX2; ++x)
                {
                    for (int y = blockY1; y <= blockY2; ++y)
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

            string title = _localizationGetter?.Invoke("Forms.MapMeltStaticsForm.DoneTitle") ?? "Melt Static";
            string message = _localizationGetter?.Invoke("Forms.MapMeltStaticsForm.DoneMessage") ?? "Done";
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

            _refreshMap();
        }
    }
}
