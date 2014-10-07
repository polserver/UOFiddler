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

namespace FiddlerControls
{
    public partial class MapReplace : Form
    {
        private Ultima.Map workingmap;
        public MapReplace(Ultima.Map currmap)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            workingmap = currmap;
            numericUpDownX1.Maximum = workingmap.Width;
            numericUpDownX2.Maximum = workingmap.Width;
            numericUpDownY1.Maximum = workingmap.Height;
            numericUpDownY2.Maximum = workingmap.Height;
            numericUpDownToX1.Maximum = workingmap.Width;
            numericUpDownToY1.Maximum = workingmap.Height;
            this.Text = String.Format("MapReplace ID:{0}",workingmap.FileIndex);
            comboBoxMapID.BeginUpdate();
            comboBoxMapID.Items.Add(new R_FeluccaOld());
            comboBoxMapID.Items.Add(new R_Felucca());
            comboBoxMapID.Items.Add(new R_Trammel());
            comboBoxMapID.Items.Add(new R_Ilshenar());
            comboBoxMapID.Items.Add(new R_Malas());
            comboBoxMapID.Items.Add(new R_Tokuno());
            comboBoxMapID.Items.Add(new R_TerMur());
            comboBoxMapID.EndUpdate();
            comboBoxMapID.SelectedIndex = 0;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select directory containing the map files";
            dialog.ShowNewFolderButton = false;
            if (dialog.ShowDialog() == DialogResult.OK)
                textBox1.Text = dialog.SelectedPath;
            dialog.Dispose();
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            string path = textBox1.Text;
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Path not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            SupportedMaps replacemap = comboBoxMapID.SelectedItem as SupportedMaps;
            if (replacemap == null)
            {
                MessageBox.Show("Invalid Map ID!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            int x1 = (int)numericUpDownX1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y1 = (int)numericUpDownY1.Value;
            int y2 = (int)numericUpDownY2.Value;
            int tox = (int)numericUpDownToX1.Value;
            int toy = (int)numericUpDownToY1.Value;

            if ((x1 < 0) || (x1 > replacemap.Width))
            {
                MessageBox.Show("Invalid X1 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((x2 < 0) || (x2 > replacemap.Width))
            {
                MessageBox.Show("Invalid X2 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((y1 < 0) || (y1 > replacemap.Height))
            {
                MessageBox.Show("Invalid Y1 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((y2 < 0) || (y2 > replacemap.Height))
            {
                MessageBox.Show("Invalid Y2 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((x1 > x2) || (y1 > y2))
            {
                MessageBox.Show("X1 and Y1 cannot be bigger than X2 and Y2!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((tox < 0) || (tox > workingmap.Width) || ((tox + (x2 - x1)) > workingmap.Width))
            {
                MessageBox.Show("Invalid toX coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((toy < 0) || (toy > workingmap.Height) || ((toy + (y2 - y1)) > workingmap.Height))
            {
                MessageBox.Show("Invalid toX coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            x1 >>= 3;
            x2 >>= 3;
            y1 >>= 3;
            y2 >>= 3;

            tox >>= 3;
            toy >>= 3;

            int tox2 = (x2 - x1) + tox;
            int toy2 = (y2 - y1) + toy;

            int blocky = workingmap.Height >> 3;
            int blockx = workingmap.Width >> 3;
            int blockyreplace = replacemap.Height >> 3;
            int blockxreplace = replacemap.Width >> 3;

            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Maximum=0;
            if (checkBoxMap.Checked)
                progressBar1.Maximum += blocky * blockx;
            if (checkBoxStatics.Checked)
                progressBar1.Maximum += blocky * blockx;
            
            if (checkBoxMap.Checked)
            {
                string copymap = Path.Combine(path, String.Format("map{0}.mul", replacemap.ID));
                if (!File.Exists(copymap))
                {
                    MessageBox.Show("Map file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }
                FileStream m_map_copy = new FileStream(copymap, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader m_mapReader_copy = new BinaryReader(m_map_copy);

                string mapPath = Ultima.Files.GetFilePath(String.Format("map{0}.mul", workingmap.FileIndex));
                FileStream m_map;
                BinaryReader m_mapReader;
                if (mapPath != null)
                {
                    m_map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    m_mapReader = new BinaryReader(m_map);
                }
                else
                {
                    MessageBox.Show("Map file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string mul = Path.Combine(FiddlerControls.Options.OutputPath, String.Format("map{0}.mul", workingmap.FileIndex));
                using (FileStream fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binmul = new BinaryWriter(fsmul))
                    {
                        for (int x = 0; x < blockx; ++x)
                        {
                            for (int y = 0; y < blocky; ++y)
                            {
                                if ((tox <= x) && (x <= tox2) && (toy <= y) && (y <= toy2))
                                {
                                    m_mapReader_copy.BaseStream.Seek((((x - tox + x1) * blockyreplace) + (y - toy + y1)) * 196, SeekOrigin.Begin);
                                    int header = m_mapReader_copy.ReadInt32();
                                    binmul.Write(header);
                                }
                                else
                                {
                                    m_mapReader.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                    int header = m_mapReader.ReadInt32();
                                    binmul.Write(header);
                                }
                                for (int i = 0; i < 64; ++i)
                                {
                                    ushort tileid;
                                    sbyte z;
                                    if ((tox <= x) && (x <= tox2) && (toy <= y) && (y <= toy2))
                                    {
                                        tileid = m_mapReader_copy.ReadUInt16();
                                        z = m_mapReader_copy.ReadSByte();
                                    }
                                    else
                                    {
                                        tileid = m_mapReader.ReadUInt16();
                                        z = m_mapReader.ReadSByte();
                                    }
                                    tileid = Art.GetLegalItemID(tileid);
                                    if (z < -128)
                                        z = -128;
                                    if (z > 127)
                                        z = 127;
                                    binmul.Write(tileid);
                                    binmul.Write(z);
                                }
                                progressBar1.PerformStep();
                            }
                        }
                    }
                }
                m_mapReader.Close();
                m_mapReader_copy.Close();
            }
            if (checkBoxStatics.Checked)
            {
                string indexPath = Files.GetFilePath(String.Format("staidx{0}.mul", workingmap.FileIndex));
                FileStream m_Index;
                BinaryReader m_IndexReader;
                if (indexPath != null)
                {
                    m_Index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    m_IndexReader = new BinaryReader(m_Index);
                }
                else
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string staticsPath = Files.GetFilePath(String.Format("statics{0}.mul", workingmap.FileIndex));

                FileStream m_Statics;
                BinaryReader m_StaticsReader;
                if (staticsPath != null)
                {
                    m_Statics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    m_StaticsReader = new BinaryReader(m_Statics);
                }
                else
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }


                string copyindexPath = Path.Combine(path, String.Format("staidx{0}.mul", replacemap.ID));
                if (!File.Exists(copyindexPath))
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }
                FileStream m_Index_copy = new FileStream(copyindexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader m_IndexReader_copy = new BinaryReader(m_Index_copy);

                string copystaticsPath = Path.Combine(path, String.Format("statics{0}.mul", replacemap.ID));
                if (!File.Exists(copystaticsPath))
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }
                FileStream m_Statics_copy = new FileStream(copystaticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader m_StaticsReader_copy = new BinaryReader(m_Statics_copy);

                string idx = Path.Combine(FiddlerControls.Options.OutputPath, String.Format("staidx{0}.mul", workingmap.FileIndex));
                string mul = Path.Combine(FiddlerControls.Options.OutputPath, String.Format("statics{0}.mul", workingmap.FileIndex));
                using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                                  fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                        binmul = new BinaryWriter(fsmul))
                    {
                        for (int x = 0; x < blockx; ++x)
                        {
                            for (int y = 0; y < blocky; ++y)
                            {
                                int lookup, length, extra;
                                if ((tox <= x) && (x <= tox2) && (toy <= y) && (y <= toy2))
                                {
                                    m_IndexReader_copy.BaseStream.Seek((((x - tox + x1) * blockyreplace) + (y - toy + y1)) * 12, SeekOrigin.Begin);
                                    lookup = m_IndexReader_copy.ReadInt32();
                                    length = m_IndexReader_copy.ReadInt32();
                                    extra = m_IndexReader_copy.ReadInt32();
                                }
                                else
                                {
                                    m_IndexReader.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                    lookup = m_IndexReader.ReadInt32();
                                    length = m_IndexReader.ReadInt32();
                                    extra = m_IndexReader.ReadInt32();
                                }

                                if (lookup < 0 || length <= 0)
                                {
                                    binidx.Write((int)-1); // lookup
                                    binidx.Write((int)-1); // length
                                    binidx.Write((int)-1); // extra
                                }
                                else
                                {
                                    if ((tox <= x) && (x <= tox2) && (toy <= y) && (y <= toy2))
                                        m_Statics_copy.Seek(lookup, SeekOrigin.Begin);
                                    else
                                        m_Statics.Seek(lookup, SeekOrigin.Begin);

                                    int fsmullength = (int)fsmul.Position;
                                    int count = length / 7;
                                    if (RemoveDupl.Checked)
                                    {
                                        StaticTile[] tilelist = new StaticTile[count];
                                        int j = 0;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            StaticTile tile = new StaticTile();
                                            if ((tox <= x) && (x <= tox2) && (toy <= y) && (y <= toy2))
                                            {
                                                tile.m_ID = m_StaticsReader_copy.ReadUInt16();
                                                tile.m_X = m_StaticsReader_copy.ReadByte();
                                                tile.m_Y = m_StaticsReader_copy.ReadByte();
                                                tile.m_Z = m_StaticsReader_copy.ReadSByte();
                                                tile.m_Hue = m_StaticsReader_copy.ReadInt16();
                                            }
                                            else
                                            {
                                                tile.m_ID = m_StaticsReader.ReadUInt16();
                                                tile.m_X = m_StaticsReader.ReadByte();
                                                tile.m_Y = m_StaticsReader.ReadByte();
                                                tile.m_Z = m_StaticsReader.ReadSByte();
                                                tile.m_Hue = m_StaticsReader.ReadInt16();
                                            }

                                            if ((tile.m_ID >= 0) &&  (tile.m_ID <= Art.GetMaxItemID()))
                                            {
                                                if (tile.m_Hue < 0)
                                                    tile.m_Hue = 0;
                                                bool first = true;
                                                for (int k = 0; k < j; ++k)
                                                {
                                                    if ((tilelist[k].m_ID == tile.m_ID)
                                                        && ((tilelist[k].m_X == tile.m_X) && (tilelist[k].m_Y == tile.m_Y))
                                                        && (tilelist[k].m_Z == tile.m_Z)
                                                        && (tilelist[k].m_Hue == tile.m_Hue))
                                                    {
                                                        first = false;
                                                        break;
                                                    }
                                                }
                                                if (first)
                                                    tilelist[j++] = tile;
                                            }
                                        }
                                        if (j > 0)
                                        {
                                            binidx.Write((int)fsmul.Position); //lookup
                                            for (int i = 0; i < j; ++i)
                                            {
                                                binmul.Write(tilelist[i].m_ID);
                                                binmul.Write(tilelist[i].m_X);
                                                binmul.Write(tilelist[i].m_Y);
                                                binmul.Write(tilelist[i].m_Z);
                                                binmul.Write(tilelist[i].m_Hue);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool firstitem = true;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            ushort graphic;
                                            short shue;
                                            byte sx, sy;
                                            sbyte sz;
                                            if ((tox <= x) && (x <= tox2) && (toy <= y) && (y <= toy2))
                                            {
                                                graphic = m_StaticsReader_copy.ReadUInt16();
                                                sx = m_StaticsReader_copy.ReadByte();
                                                sy = m_StaticsReader_copy.ReadByte();
                                                sz = m_StaticsReader_copy.ReadSByte();
                                                shue = m_StaticsReader_copy.ReadInt16();
                                            }
                                            else
                                            {
                                                graphic = m_StaticsReader.ReadUInt16();
                                                sx = m_StaticsReader.ReadByte();
                                                sy = m_StaticsReader.ReadByte();
                                                sz = m_StaticsReader.ReadSByte();
                                                shue = m_StaticsReader.ReadInt16();
                                            }

                                            if ((graphic >= 0) && (graphic <= Art.GetMaxItemID()))
                                            {
                                                if (shue < 0)
                                                    shue = 0;
                                                if (firstitem)
                                                {
                                                    binidx.Write((int)fsmul.Position); //lookup
                                                    firstitem = false;
                                                }
                                                binmul.Write(graphic);
                                                binmul.Write(sx);
                                                binmul.Write(sy);
                                                binmul.Write(sz);
                                                binmul.Write(shue);
                                            }
                                        }
                                    }
                                    fsmullength = (int)fsmul.Position - fsmullength;
                                    if (fsmullength > 0)
                                    {
                                        binidx.Write(fsmullength); //length
                                        binidx.Write(extra); //extra
                                    }
                                    else
                                    {
                                        binidx.Write((int)-1); //lookup
                                        binidx.Write((int)-1); //length
                                        binidx.Write((int)-1); //extra
                                    }
                                }
                                progressBar1.PerformStep();
                            }
                        }
                    }
                }
                m_IndexReader.Close();
                m_StaticsReader.Close();
                m_Index_copy.Close();
                m_StaticsReader_copy.Close();
            }

            MessageBox.Show(String.Format("Files saved to {0}", FiddlerControls.Options.OutputPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        class SupportedMaps
        {
            public int ID { get; private set; }
            public string Name { get; private set; }
            public int Height { get; private set; }
            public int Width { get; private set; }
            public SupportedMaps(int id, string name, int width, int height)
            {
                ID = id;
                Name = name;
                Width = width;
                Height = height;
            }
            public override string ToString()
            {
                return String.Format("{0} - {1} : {2}x{3}", ID, Name, Width, Height);
            }
        }
        class R_FeluccaOld : SupportedMaps
        {
            public R_FeluccaOld() : base(0, Options.MapNames[0] + "Old", 6144, 4096) { }
        }
        class R_Felucca : SupportedMaps
        {
            public R_Felucca() : base(0, Options.MapNames[0], 7168, 4096) { }
        }
        class R_Trammel : SupportedMaps
        {
            public R_Trammel() : base(1, Options.MapNames[1], 7168, 4096) { }
        }
        class R_Ilshenar : SupportedMaps
        {
            public R_Ilshenar() : base(2, Options.MapNames[2], 2304, 1600) { }
        }
        class R_Malas : SupportedMaps
        {
            public R_Malas() : base(3, Options.MapNames[3], 2560, 2048) { }
        }
        class R_Tokuno : SupportedMaps
        {
            public R_Tokuno() : base(4, Options.MapNames[4], 1448, 1448) { }
        }
        class R_TerMur : SupportedMaps
        {
            public R_TerMur() : base(5, Options.MapNames[5], 1280, 4096) { }
        }
    }
}
