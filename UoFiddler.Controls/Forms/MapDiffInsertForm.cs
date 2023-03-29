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
    public partial class MapDiffInsertForm : Form
    {
        private readonly Map _workingMap;

        public MapDiffInsertForm(Map currentMap)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _workingMap = currentMap;
            numericUpDownX1.Maximum = _workingMap.Width;
            numericUpDownX2.Maximum = _workingMap.Width;
            numericUpDownY1.Maximum = _workingMap.Height;
            numericUpDownY2.Maximum = _workingMap.Height;
            Text = $"Map Diff Insert ID:{_workingMap.FileIndex}";
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            int x1 = (int)numericUpDownX1.Value;
            int x2 = (int)numericUpDownX2.Value;
            int y1 = (int)numericUpDownY1.Value;
            int y2 = (int)numericUpDownY2.Value;

            if (x1 < 0 || x1 > _workingMap.Width)
            {
                MessageBox.Show("Invalid X1 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x2 < 0 || x2 > _workingMap.Width)
            {
                MessageBox.Show("Invalid X2 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y1 < 0 || y1 > _workingMap.Height)
            {
                MessageBox.Show("Invalid Y1 coordinate!", "Map Diff Insert", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y2 < 0 || y2 > _workingMap.Height)
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

            int blockY = _workingMap.Height >> 3;
            int blockX = _workingMap.Width >> 3;

            progressBar1.Step = 1;
            progressBar1.Value = 0;
            progressBar1.Maximum = 0;

            if (checkBoxMap.Checked)
            {
                progressBar1.Maximum += blockY * blockX;
            }

            if (checkBoxStatics.Checked)
            {
                progressBar1.Maximum += blockY * blockX;
            }

            if (checkBoxMap.Checked)
            {
                string mapPath = Files.GetFilePath($"map{_workingMap.FileIndex}.mul");
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

                string mul = Path.Combine(Options.OutputPath, $"map{_workingMap.FileIndex}.mul");
                using (FileStream fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binmul = new BinaryWriter(fsmul))
                    {
                        for (int x = 0; x < blockX; ++x)
                        {
                            for (int y = 0; y < blockY; ++y)
                            {
                                mMapReader.BaseStream.Seek(((x * blockY) + y) * 196, SeekOrigin.Begin);
                                int header = mMapReader.ReadInt32();
                                binmul.Write(header);
                                ushort tileid;
                                sbyte z;
                                bool patched = false;
                                if (x1 <= x && x <= x2 && y1 <= y && y <= y2)
                                {
                                    if (_workingMap.Tiles.Patch.IsLandBlockPatched(x, y))
                                    {
                                        patched = true;
                                        Tile[] patchtile = _workingMap.Tiles.Patch.GetLandBlock(x, y);
                                        for (int i = 0; i < 64; ++i)
                                        {
                                            tileid = patchtile[i].Id;
                                            z = (sbyte)patchtile[i].Z;
                                            tileid = Art.GetLegalItemId(tileid);
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
                                        tileid = Art.GetLegalItemId(tileid);
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
                string indexPath = Files.GetFilePath($"staidx{_workingMap.FileIndex}.mul");
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

                string staticsPath = Files.GetFilePath($"statics{_workingMap.FileIndex}.mul");

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

                string idx = Path.Combine(Options.OutputPath, $"staidx{_workingMap.FileIndex}.mul");
                string mul = Path.Combine(Options.OutputPath, $"statics{_workingMap.FileIndex}.mul");
                using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                                  fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                        binmul = new BinaryWriter(fsmul))
                    {
                        for (int x = 0; x < blockX; ++x)
                        {
                            for (int y = 0; y < blockY; ++y)
                            {
                                mIndexReader.BaseStream.Seek(((x * blockY) + y) * 12, SeekOrigin.Begin);
                                var lookup = mIndexReader.ReadInt32();
                                var length = mIndexReader.ReadInt32();
                                var extra = mIndexReader.ReadInt32();
                                bool patched = false;
                                if (x1 <= x && x <= x2 && y1 <= y && y <= y2)
                                {
                                    if (_workingMap.Tiles.Patch.IsStaticBlockPatched(x, y))
                                    {
                                        patched = true;
                                    }
                                }

                                if (patched)
                                {
                                    HuedTile[][][] patchstat = _workingMap.Tiles.Patch.GetStaticBlock(x, y);
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
                                                            Id = htile.Id,
                                                            Z = (sbyte)htile.Z,
                                                            X = (byte)i,
                                                            Y = (byte)j,
                                                            Hue = (short)htile.Hue
                                                        };

                                                        if (tile.Id > Art.GetMaxItemId())
                                                        {
                                                            continue;
                                                        }

                                                        if (tile.Hue < 0)
                                                        {
                                                            tile.Hue = 0;
                                                        }

                                                        bool first = true;
                                                        for (int k = 0; k < m; ++k)
                                                        {
                                                            if (tilelist[k].Id == tile.Id && tilelist[k].X == tile.X && tilelist[k].Y == tile.Y && tilelist[k].Z == tile.Z && tilelist[k].Hue == tile.Hue)
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
                                                    binmul.Write(tilelist[i].Id);
                                                    binmul.Write(tilelist[i].X);
                                                    binmul.Write(tilelist[i].Y);
                                                    binmul.Write(tilelist[i].Z);
                                                    binmul.Write(tilelist[i].Hue);
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
                                                        ushort graphic = tile.Id;
                                                        sbyte sz = (sbyte)tile.Z;
                                                        short sHue = (short)tile.Hue;

                                                        if (graphic > Art.GetMaxItemId())
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
                                                    Id = mStaticsReader.ReadUInt16(),
                                                    X = mStaticsReader.ReadByte(),
                                                    Y = mStaticsReader.ReadByte(),
                                                    Z = mStaticsReader.ReadSByte(),
                                                    Hue = mStaticsReader.ReadInt16()
                                                };

                                                if (tile.Id <= Art.GetMaxItemId())
                                                {
                                                    if (tile.Hue < 0)
                                                    {
                                                        tile.Hue = 0;
                                                    }

                                                    bool first = true;
                                                    for (int k = 0; k < j; ++k)
                                                    {
                                                        if (tilelist[k].Id == tile.Id && tilelist[k].X == tile.X && tilelist[k].Y == tile.Y && tilelist[k].Z == tile.Z && tilelist[k].Hue == tile.Hue)
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
                                                    binmul.Write(tilelist[i].Id);
                                                    binmul.Write(tilelist[i].X);
                                                    binmul.Write(tilelist[i].Y);
                                                    binmul.Write(tilelist[i].Z);
                                                    binmul.Write(tilelist[i].Hue);
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

                                                if (graphic <= Art.GetMaxItemId())
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
