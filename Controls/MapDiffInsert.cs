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
    public partial class MapDiffInsert : Form
    {
        private Ultima.Map workingmap;
        public MapDiffInsert(Ultima.Map currmap)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            workingmap = currmap;
            numericUpDownX1.Maximum = workingmap.Width;
            numericUpDownX2.Maximum = workingmap.Width;
            numericUpDownY1.Maximum = workingmap.Height;
            numericUpDownY2.Maximum = workingmap.Height;
            this.Text = String.Format("Map Diff Insert ID:{0}",workingmap.FileIndex);
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            int x1 = (int)numericUpDownX1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y1 = (int)numericUpDownY1.Value;
            int y2 = (int)numericUpDownY2.Value;
            if ((x1<0) || (x1>workingmap.Width))
            {
                MessageBox.Show("Invalid X1 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((x2<0) || (x2>workingmap.Width))
            {
                MessageBox.Show("Invalid X2 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((y1 < 0) || (y1 > workingmap.Height))
            {
                MessageBox.Show("Invalid Y1 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((y2 < 0) || (y2 > workingmap.Height))
            {
                MessageBox.Show("Invalid Y2 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            if ((x1 > x2) || (y1 > y2))
            {
                MessageBox.Show("X1 and Y1 cannot be bigger than X2 and Y2!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            x1 >>= 3;
            x2 >>= 3;
            y1 >>= 3;
            y2 >>= 3;

            int blocky = workingmap.Height >> 3;
            int blockx = workingmap.Width >> 3;

            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Maximum=0;
            if (checkBoxMap.Checked)
                progressBar1.Maximum += blocky * blockx;
            if (checkBoxStatics.Checked)
                progressBar1.Maximum += blocky * blockx;
            
            if (checkBoxMap.Checked)
            {
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
                    MessageBox.Show("Map file not found!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                                m_mapReader.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                int header = m_mapReader.ReadInt32();
                                binmul.Write(header);
                                ushort tileid;
                                sbyte z;
                                bool patched = false;
                                if ((x1 <= x) && (x <= x2) && (y1 <= y) && (y <= y2))
                                {
                                    if (workingmap.Tiles.Patch.IsLandBlockPatched(x, y))
                                    {
                                        patched = true;
                                        Tile[] patchtile = workingmap.Tiles.Patch.GetLandBlock(x, y);
                                        for (int i = 0; i < 64; ++i)
                                        {
                                            tileid = patchtile[i].ID;
                                            z = (sbyte)patchtile[i].Z;
                                            tileid = Art.GetLegalItemID(tileid);
                                            if (z < -128)
                                                z = -128;
                                            if (z > 127)
                                                z = 127;
                                            binmul.Write(tileid);
                                            binmul.Write(z);
                                        }
                                    }
                                }
                                if (!patched)
                                {
                                    for (int i = 0; i < 64; ++i)
                                    {
                                        tileid = m_mapReader.ReadUInt16();
                                        z = m_mapReader.ReadSByte();
                                        tileid = Art.GetLegalItemID(tileid);
                                        if (z < -128)
                                            z = -128;
                                        if (z > 127)
                                            z = 127;
                                        binmul.Write(tileid);
                                        binmul.Write(z);
                                    }
                                }
                                progressBar1.PerformStep();
                            }
                        }
                    }
                }
                m_mapReader.Close();
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
                    MessageBox.Show("Static file not found!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                    MessageBox.Show("Static file not found!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

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
                                m_IndexReader.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                lookup = m_IndexReader.ReadInt32();
                                length = m_IndexReader.ReadInt32();
                                extra = m_IndexReader.ReadInt32();
                                bool patched = false;
                                if ((x1 <= x) && (x <= x2) && (y1 <= y) && (y <= y2))
                                {
                                    if (workingmap.Tiles.Patch.IsStaticBlockPatched(x, y))
                                        patched = true;
                                }
                                if (patched)
                                {
                                    HuedTile[][][] patchstat = workingmap.Tiles.Patch.GetStaticBlock(x, y);
                                    int count = 0;
                                    for (int i = 0; i < 8; ++i)
                                    {
                                        for (int j = 0; j < 8; ++j)
                                        {
                                            if (patchstat[i][j] != null)
                                                count += patchstat[i][j].Length;
                                        }
                                    }
                                    if (count == 0)
                                    {
                                        binidx.Write((int)-1); // lookup
                                        binidx.Write((int)-1); // length
                                        binidx.Write((int)-1); // extra
                                    }
                                    else
                                    {
                                        int fsmullength = (int)fsmul.Position;
                                        if (RemoveDupl.Checked)
                                        {
                                            StaticTile[] tilelist = new StaticTile[count];
                                            int m = 0;
                                            for (int i = 0; i < 8; ++i)
                                            {
                                                for (int j = 0; j < 8; ++j)
                                                {
                                                    foreach (HuedTile htile in patchstat[i][j])
                                                    {
                                                        StaticTile tile = new StaticTile();
                                                        tile.m_ID = (ushort)(htile.ID);
                                                        tile.m_Z = (sbyte)htile.Z;
                                                        tile.m_X = (byte)i;
                                                        tile.m_Y = (byte)j;
                                                        tile.m_Hue = (short)htile.Hue;

                                                        if ((tile.m_ID >= 0) && (tile.m_ID <= Art.GetMaxItemID()))
                                                        {
                                                            if (tile.m_Hue < 0)
                                                                tile.m_Hue = 0;
                                                            bool first = true;
                                                            for (int k = 0; k < m; ++k)
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
                                                            {
                                                                tilelist[m] = tile;
                                                                ++m;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (m > 0)
                                            {
                                                binidx.Write((int)fsmul.Position); //lookup
                                                for (int i = 0; i < m; ++i)
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
                                            ushort graphic;
                                            short shue;
                                            sbyte sz;
                                            bool firstitem = true;
                                            for (int i = 0; i < 8; ++i)
                                            {
                                                for (int j = 0; j < 8; ++j)
                                                {
                                                    foreach (HuedTile tile in patchstat[i][j])
                                                    {
                                                        graphic = tile.ID;
                                                        sz = (sbyte)tile.Z;
                                                        shue = (short)tile.Hue;

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
                                                            binmul.Write((byte)i); //x
                                                            binmul.Write((byte)j); //y
                                                            binmul.Write(sz);
                                                            binmul.Write(shue);
                                                        }
                                                    }
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
                                    
                                }
                                else
                                {
                                    if (lookup < 0 || length <= 0)
                                    {
                                        binidx.Write((int)-1); // lookup
                                        binidx.Write((int)-1); // length
                                        binidx.Write((int)-1); // extra
                                    }
                                    else
                                    {
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
                                                tile.m_ID = m_StaticsReader.ReadUInt16();
                                                tile.m_X = m_StaticsReader.ReadByte();
                                                tile.m_Y = m_StaticsReader.ReadByte();
                                                tile.m_Z = m_StaticsReader.ReadSByte();
                                                tile.m_Hue = m_StaticsReader.ReadInt16();
                                                
                                                if ((tile.m_ID >= 0) && (tile.m_ID <= Art.GetMaxItemID()))
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
                                                graphic = m_StaticsReader.ReadUInt16();
                                                sx = m_StaticsReader.ReadByte();
                                                sy = m_StaticsReader.ReadByte();
                                                sz = m_StaticsReader.ReadSByte();
                                                shue = m_StaticsReader.ReadInt16();

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
                                }
                                progressBar1.PerformStep();
                            }
                        }
                    }
                }
                m_IndexReader.Close();
                m_StaticsReader.Close();
            }

            MessageBox.Show(String.Format("Files saved to {0}", FiddlerControls.Options.OutputPath), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}
