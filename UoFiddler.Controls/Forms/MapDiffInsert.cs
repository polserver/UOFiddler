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
    public partial class MapDiffInsert : Form
    {
        private readonly Map _workingmap;

        public MapDiffInsert(Map currmap)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _workingmap = currmap;
            numericUpDownX1.Maximum = _workingmap.Width;
            numericUpDownX2.Maximum = _workingmap.Width;
            numericUpDownY1.Maximum = _workingmap.Height;
            numericUpDownY2.Maximum = _workingmap.Height;
            Text = $"Map Diff Insert ID:{_workingmap.FileIndex}";
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            int x1 = (int)numericUpDownX1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y1 = (int)numericUpDownY1.Value;
            int y2 = (int)numericUpDownY2.Value;

            if (x1 < 0 || x1 > _workingmap.Width)
            {
                MessageBox.Show("Invalid X1 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x2 < 0 || x2 > _workingmap.Width)
            {
                MessageBox.Show("Invalid X2 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y1 < 0 || y1 > _workingmap.Height)
            {
                MessageBox.Show("Invalid Y1 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y2 < 0 || y2 > _workingmap.Height)
            {
                MessageBox.Show("Invalid Y2 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x1 > x2 || y1 > y2)
            {
                MessageBox.Show("X1 and Y1 cannot be bigger than X2 and Y2!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            x1 >>= 3;
            x2 >>= 3;
            y1 >>= 3;
            y2 >>= 3;

            int blocky = _workingmap.Height >> 3;
            int blockx = _workingmap.Width >> 3;

            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Maximum = 0;

            if (checkBoxMap.Checked)
            {
                progressBar1.Maximum += blocky * blockx;
            }

            if (checkBoxStatics.Checked)
            {
                progressBar1.Maximum += blocky * blockx;
            }

            if (checkBoxMap.Checked)
            {
                string mapPath = Files.GetFilePath($"map{_workingmap.FileIndex}.mul");
                BinaryReader mMapReader;

                if (mapPath != null)
                {
                    var mMap = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mMapReader = new BinaryReader(mMap);
                }
                else
                {
                    MessageBox.Show("Map file not found!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string mul = Path.Combine(Options.OutputPath, $"map{_workingmap.FileIndex}.mul");
                using (FileStream fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binmul = new BinaryWriter(fsmul))
                    {
                        for (int x = 0; x < blockx; ++x)
                        {
                            for (int y = 0; y < blocky; ++y)
                            {
                                mMapReader.BaseStream.Seek((x * blocky + y) * 196, SeekOrigin.Begin);
                                int header = mMapReader.ReadInt32();
                                binmul.Write(header);
                                ushort tileid;
                                sbyte z;
                                bool patched = false;
                                if (x1 <= x && x <= x2 && y1 <= y && y <= y2)
                                {
                                    if (_workingmap.Tiles.Patch.IsLandBlockPatched(x, y))
                                    {
                                        patched = true;
                                        Tile[] patchtile = _workingmap.Tiles.Patch.GetLandBlock(x, y);
                                        for (int i = 0; i < 64; ++i)
                                        {
                                            tileid = patchtile[i].ID;
                                            z = (sbyte)patchtile[i].Z;
                                            tileid = Art.GetLegalItemID(tileid);
                                            if (z < -128)
                                            {
                                                z = -128;
                                            }

                                            if (z > 127)
                                            {
                                                z = 127;
                                            }

                                            binmul.Write(tileid);
                                            binmul.Write(z);
                                        }
                                    }
                                }

                                if (!patched)
                                {
                                    for (int i = 0; i < 64; ++i)
                                    {
                                        tileid = mMapReader.ReadUInt16();
                                        z = mMapReader.ReadSByte();
                                        tileid = Art.GetLegalItemID(tileid);
                                        if (z < -128)
                                        {
                                            z = -128;
                                        }

                                        if (z > 127)
                                        {
                                            z = 127;
                                        }

                                        binmul.Write(tileid);
                                        binmul.Write(z);
                                    }
                                }

                                progressBar1.PerformStep();
                            }
                        }
                    }
                }

                mMapReader.Close();
            }
            if (checkBoxStatics.Checked)
            {
                string indexPath = Files.GetFilePath($"staidx{_workingmap.FileIndex}.mul");
                BinaryReader mIndexReader;
                if (indexPath != null)
                {
                    var mIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mIndexReader = new BinaryReader(mIndex);
                }
                else
                {
                    MessageBox.Show("Static file not found!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string staticsPath = Files.GetFilePath($"statics{_workingmap.FileIndex}.mul");

                FileStream mStatics;
                BinaryReader mStaticsReader;
                if (staticsPath != null)
                {
                    mStatics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mStaticsReader = new BinaryReader(mStatics);
                }
                else
                {
                    MessageBox.Show("Static file not found!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string idx = Path.Combine(Options.OutputPath, $"staidx{_workingmap.FileIndex}.mul");
                string mul = Path.Combine(Options.OutputPath, $"statics{_workingmap.FileIndex}.mul");
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
                                mIndexReader.BaseStream.Seek((x * blocky + y) * 12, SeekOrigin.Begin);
                                var lookup = mIndexReader.ReadInt32();
                                var length = mIndexReader.ReadInt32();
                                var extra = mIndexReader.ReadInt32();
                                bool patched = false;
                                if (x1 <= x && x <= x2 && y1 <= y && y <= y2)
                                {
                                    if (_workingmap.Tiles.Patch.IsStaticBlockPatched(x, y))
                                    {
                                        patched = true;
                                    }
                                }

                                if (patched)
                                {
                                    HuedTile[][][] patchstat = _workingmap.Tiles.Patch.GetStaticBlock(x, y);
                                    int count = 0;
                                    for (int i = 0; i < 8; ++i)
                                    {
                                        for (int j = 0; j < 8; ++j)
                                        {
                                            if (patchstat[i][j] != null)
                                            {
                                                count += patchstat[i][j].Length;
                                            }
                                        }
                                    }

                                    if (count == 0)
                                    {
                                        binidx.Write(-1); // lookup
                                        binidx.Write(-1); // length
                                        binidx.Write(-1); // extra
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
                                                        StaticTile tile = new StaticTile
                                                        {
                                                            m_ID = htile.ID,
                                                            m_Z = (sbyte)htile.Z,
                                                            m_X = (byte)i,
                                                            m_Y = (byte)j,
                                                            m_Hue = (short)htile.Hue
                                                        };

                                                        if (tile.m_ID < 0 || tile.m_ID > Art.GetMaxItemID())
                                                        {
                                                            continue;
                                                        }

                                                        if (tile.m_Hue < 0)
                                                        {
                                                            tile.m_Hue = 0;
                                                        }

                                                        bool first = true;
                                                        for (int k = 0; k < m; ++k)
                                                        {
                                                            if (tilelist[k].m_ID == tile.m_ID && tilelist[k].m_X == tile.m_X && tilelist[k].m_Y == tile.m_Y && tilelist[k].m_Z == tile.m_Z && tilelist[k].m_Hue == tile.m_Hue)
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
                                            bool firstItem = true;
                                            for (int i = 0; i < 8; ++i)
                                            {
                                                for (int j = 0; j < 8; ++j)
                                                {
                                                    foreach (HuedTile tile in patchstat[i][j])
                                                    {
                                                        ushort graphic = tile.ID;
                                                        sbyte sz = (sbyte)tile.Z;
                                                        short sHue = (short)tile.Hue;

                                                        if (graphic < 0 || graphic > Art.GetMaxItemID())
                                                        {
                                                            continue;
                                                        }

                                                        if (sHue < 0)
                                                        {
                                                            sHue = 0;
                                                        }

                                                        if (firstItem)
                                                        {
                                                            binidx.Write((int)fsmul.Position); //lookup
                                                            firstItem = false;
                                                        }
                                                        binmul.Write(graphic);
                                                        binmul.Write((byte)i); //x
                                                        binmul.Write((byte)j); //y
                                                        binmul.Write(sz);
                                                        binmul.Write(sHue);
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
                                            binidx.Write(-1); //lookup
                                            binidx.Write(-1); //length
                                            binidx.Write(-1); //extra
                                        }
                                    }
                                }
                                else
                                {
                                    if (lookup < 0 || length <= 0)
                                    {
                                        binidx.Write(-1); // lookup
                                        binidx.Write(-1); // length
                                        binidx.Write(-1); // extra
                                    }
                                    else
                                    {
                                        mStatics.Seek(lookup, SeekOrigin.Begin);
                                        int fsmullength = (int)fsmul.Position;
                                        int count = length / 7;

                                        if (RemoveDupl.Checked)
                                        {
                                            StaticTile[] tilelist = new StaticTile[count];
                                            int j = 0;
                                            for (int i = 0; i < count; ++i)
                                            {
                                                StaticTile tile = new StaticTile
                                                {
                                                    m_ID = mStaticsReader.ReadUInt16(),
                                                    m_X = mStaticsReader.ReadByte(),
                                                    m_Y = mStaticsReader.ReadByte(),
                                                    m_Z = mStaticsReader.ReadSByte(),
                                                    m_Hue = mStaticsReader.ReadInt16()
                                                };

                                                if (tile.m_ID >= 0 && tile.m_ID <= Art.GetMaxItemID())
                                                {
                                                    if (tile.m_Hue < 0)
                                                    {
                                                        tile.m_Hue = 0;
                                                    }

                                                    bool first = true;
                                                    for (int k = 0; k < j; ++k)
                                                    {
                                                        if (tilelist[k].m_ID == tile.m_ID && tilelist[k].m_X == tile.m_X && tilelist[k].m_Y == tile.m_Y && tilelist[k].m_Z == tile.m_Z && tilelist[k].m_Hue == tile.m_Hue)
                                                        {
                                                            first = false;
                                                            break;
                                                        }
                                                    }
                                                    if (first)
                                                    {
                                                        tilelist[j++] = tile;
                                                    }
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
                                            bool firstItem = true;
                                            for (int i = 0; i < count; ++i)
                                            {
                                                var graphic = mStaticsReader.ReadUInt16();
                                                var sx = mStaticsReader.ReadByte();
                                                var sy = mStaticsReader.ReadByte();
                                                var sz = mStaticsReader.ReadSByte();
                                                var shue = mStaticsReader.ReadInt16();

                                                if (graphic >= 0 && graphic <= Art.GetMaxItemID())
                                                {
                                                    if (shue < 0)
                                                    {
                                                        shue = 0;
                                                    }

                                                    if (firstItem)
                                                    {
                                                        binidx.Write((int)fsmul.Position); //lookup
                                                        firstItem = false;
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
                                            binidx.Write(-1); //lookup
                                            binidx.Write(-1); //length
                                            binidx.Write(-1); //extra
                                        }
                                    }
                                }
                                progressBar1.PerformStep();
                            }
                        }
                    }
                }
                mIndexReader.Close();
                mStaticsReader.Close();
            }

            MessageBox.Show($"Files saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}
