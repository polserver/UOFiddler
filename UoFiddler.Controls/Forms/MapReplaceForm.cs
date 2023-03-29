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
    public partial class MapReplaceForm : Form
    {
        private readonly Map _workingMap;

        public MapReplaceForm(Map currentMap)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _workingMap = currentMap;
            numericUpDownX1.Maximum = _workingMap.Width;
            numericUpDownX2.Maximum = _workingMap.Width;
            numericUpDownY1.Maximum = _workingMap.Height;
            numericUpDownY2.Maximum = _workingMap.Height;
            numericUpDownToX1.Maximum = _workingMap.Width;
            numericUpDownToY1.Maximum = _workingMap.Height;
            Text = $"MapReplace ID:{_workingMap.FileIndex}";
            comboBoxMapID.BeginUpdate();
            comboBoxMapID.Items.Add(new RFeluccaOld());
            comboBoxMapID.Items.Add(new RFelucca());
            comboBoxMapID.Items.Add(new RTrammel());
            comboBoxMapID.Items.Add(new RIlshenar());
            comboBoxMapID.Items.Add(new RMalas());
            comboBoxMapID.Items.Add(new RTokuno());
            comboBoxMapID.Items.Add(new RTerMur());
            comboBoxMapID.EndUpdate();
            comboBoxMapID.SelectedIndex = 0;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog
            {
                Description = "Select directory containing the map files",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath;
            }

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

            if (!(comboBoxMapID.SelectedItem is SupportedMaps replaceMap))
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

            if (x1 < 0 || x1 > replaceMap.Width)
            {
                MessageBox.Show("Invalid X1 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x2 < 0 || x2 > replaceMap.Width)
            {
                MessageBox.Show("Invalid X2 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y1 < 0 || y1 > replaceMap.Height)
            {
                MessageBox.Show("Invalid Y1 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (y2 < 0 || y2 > replaceMap.Height)
            {
                MessageBox.Show("Invalid Y2 coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (x1 > x2 || y1 > y2)
            {
                MessageBox.Show("X1 and Y1 cannot be bigger than X2 and Y2!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (tox < 0 || tox > _workingMap.Width || tox + (x2 - x1) > _workingMap.Width)
            {
                MessageBox.Show("Invalid toX coordinate!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }

            if (toy < 0 || toy > _workingMap.Height || toy + (y2 - y1) > _workingMap.Height)
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

            int tox2 = x2 - x1 + tox;
            int toy2 = y2 - y1 + toy;

            int blockY = _workingMap.Height >> 3;
            int blockX = _workingMap.Width >> 3;
            int blockYReplace = replaceMap.Height >> 3;
            // int blockxreplace = replacemap.Width >> 3; // TODO: unused variable?

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
                string copyMap = Path.Combine(path, $"map{replaceMap.Id}.mul");
                if (!File.Exists(copyMap))
                {
                    MessageBox.Show("Map file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    return;
                }

                FileStream mMapCopy = new FileStream(copyMap, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader mMapReaderCopy = new BinaryReader(mMapCopy);
                string mapPath = Files.GetFilePath($"map{_workingMap.FileIndex}.mul");

                BinaryReader mMapReader;

                if (mapPath != null)
                {
                    FileStream mMap = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mMapReader = new BinaryReader(mMap);
                }
                else
                {
                    MessageBox.Show("Map file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                    return;
                }

                string mul = Path.Combine(Options.OutputPath, $"map{_workingMap.FileIndex}.mul");
                using (FileStream fsMul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binMul = new BinaryWriter(fsMul))
                    {
                        for (int x = 0; x < blockX; ++x)
                        {
                            for (int y = 0; y < blockY; ++y)
                            {
                                if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                {
                                    mMapReaderCopy.BaseStream.Seek((((x - tox + x1) * blockYReplace) + (y - toy) + y1) * 196, SeekOrigin.Begin);
                                    int header = mMapReaderCopy.ReadInt32();
                                    binMul.Write(header);
                                }
                                else
                                {
                                    mMapReader.BaseStream.Seek(((x * blockY) + y) * 196, SeekOrigin.Begin);
                                    int header = mMapReader.ReadInt32();
                                    binMul.Write(header);
                                }
                                for (int i = 0; i < 64; ++i)
                                {
                                    ushort tileId;
                                    sbyte z;

                                    if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                    {
                                        tileId = mMapReaderCopy.ReadUInt16();
                                        z = mMapReaderCopy.ReadSByte();
                                    }
                                    else
                                    {
                                        tileId = mMapReader.ReadUInt16();
                                        z = mMapReader.ReadSByte();
                                    }

                                    tileId = Art.GetLegalItemId(tileId);

                                    if (z < -128)
                                    {
                                        z = -128;
                                    }

                                    if (z > 127)
                                    {
                                        z = 127;
                                    }

                                    binMul.Write(tileId);
                                    binMul.Write(z);
                                }
                                progressBar1.PerformStep();
                            }
                        }
                    }
                }

                mMapReader.Close();
                mMapReaderCopy.Close();
            }

            if (checkBoxStatics.Checked)
            {
                string indexPath = Files.GetFilePath($"staidx{_workingMap.FileIndex}.mul");
                BinaryReader mIndexReader;

                if (indexPath != null)
                {
                    FileStream mIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    mIndexReader = new BinaryReader(mIndex);
                }
                else
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                string copyIndexPath = Path.Combine(path, $"staidx{replaceMap.Id}.mul");
                if (!File.Exists(copyIndexPath))
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                FileStream mIndexCopy = new FileStream(copyIndexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader mIndexReaderCopy = new BinaryReader(mIndexCopy);

                string copyStaticsPath = Path.Combine(path, $"statics{replaceMap.Id}.mul");
                if (!File.Exists(copyStaticsPath))
                {
                    MessageBox.Show("Static file not found!", "Map Replace", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    return;
                }

                FileStream mStaticsCopy = new FileStream(copyStaticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader mStaticsReaderCopy = new BinaryReader(mStaticsCopy);

                string idx = Path.Combine(Options.OutputPath, $"staidx{_workingMap.FileIndex}.mul");
                string mul = Path.Combine(Options.OutputPath, $"statics{_workingMap.FileIndex}.mul");
                using (FileStream fsIdx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                                  fsMul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (BinaryWriter binidx = new BinaryWriter(fsIdx),
                                        binmul = new BinaryWriter(fsMul))
                    {
                        for (int x = 0; x < blockX; ++x)
                        {
                            for (int y = 0; y < blockY; ++y)
                            {
                                int lookup, length, extra;
                                if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                {
                                    mIndexReaderCopy.BaseStream.Seek((((x - tox + x1) * blockYReplace) + (y - toy) + y1) * 12, SeekOrigin.Begin);
                                    lookup = mIndexReaderCopy.ReadInt32();
                                    length = mIndexReaderCopy.ReadInt32();
                                    extra = mIndexReaderCopy.ReadInt32();
                                }
                                else
                                {
                                    mIndexReader.BaseStream.Seek(((x * blockY) + y) * 12, SeekOrigin.Begin);
                                    lookup = mIndexReader.ReadInt32();
                                    length = mIndexReader.ReadInt32();
                                    extra = mIndexReader.ReadInt32();
                                }

                                if (lookup < 0 || length <= 0)
                                {
                                    binidx.Write(-1); // lookup
                                    binidx.Write(-1); // length
                                    binidx.Write(-1); // extra
                                }
                                else
                                {
                                    if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                    {
                                        mStaticsCopy.Seek(lookup, SeekOrigin.Begin);
                                    }
                                    else
                                    {
                                        mStatics.Seek(lookup, SeekOrigin.Begin);
                                    }

                                    int fsMulLength = (int)fsMul.Position;
                                    int count = length / 7;
                                    if (RemoveDupl.Checked)
                                    {
                                        var tileList = new StaticTile[count];
                                        int j = 0;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            StaticTile tile = new StaticTile();
                                            if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                            {
                                                tile.Id = mStaticsReaderCopy.ReadUInt16();
                                                tile.X = mStaticsReaderCopy.ReadByte();
                                                tile.Y = mStaticsReaderCopy.ReadByte();
                                                tile.Z = mStaticsReaderCopy.ReadSByte();
                                                tile.Hue = mStaticsReaderCopy.ReadInt16();
                                            }
                                            else
                                            {
                                                tile.Id = mStaticsReader.ReadUInt16();
                                                tile.X = mStaticsReader.ReadByte();
                                                tile.Y = mStaticsReader.ReadByte();
                                                tile.Z = mStaticsReader.ReadSByte();
                                                tile.Hue = mStaticsReader.ReadInt16();
                                            }

                                            if (tile.Id > Art.GetMaxItemId())
                                            {
                                                continue;
                                            }

                                            if (tile.Hue < 0)
                                            {
                                                tile.Hue = 0;
                                            }

                                            bool first = true;
                                            for (int k = 0; k < j; ++k)
                                            {
                                                if (tileList[k].Id == tile.Id && tileList[k].X == tile.X && tileList[k].Y == tile.Y && tileList[k].Z == tile.Z && tileList[k].Hue == tile.Hue)
                                                {
                                                    first = false;
                                                    break;
                                                }
                                            }
                                            if (first)
                                            {
                                                tileList[j++] = tile;
                                            }
                                        }
                                        if (j > 0)
                                        {
                                            binidx.Write((int)fsMul.Position); //lookup
                                            for (int i = 0; i < j; ++i)
                                            {
                                                binmul.Write(tileList[i].Id);
                                                binmul.Write(tileList[i].X);
                                                binmul.Write(tileList[i].Y);
                                                binmul.Write(tileList[i].Z);
                                                binmul.Write(tileList[i].Hue);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bool firstItem = true;
                                        for (int i = 0; i < count; ++i)
                                        {
                                            ushort graphic;
                                            short sHue;
                                            byte sx, sy;
                                            sbyte sz;
                                            if (tox <= x && x <= tox2 && toy <= y && y <= toy2)
                                            {
                                                graphic = mStaticsReaderCopy.ReadUInt16();
                                                sx = mStaticsReaderCopy.ReadByte();
                                                sy = mStaticsReaderCopy.ReadByte();
                                                sz = mStaticsReaderCopy.ReadSByte();
                                                sHue = mStaticsReaderCopy.ReadInt16();
                                            }
                                            else
                                            {
                                                graphic = mStaticsReader.ReadUInt16();
                                                sx = mStaticsReader.ReadByte();
                                                sy = mStaticsReader.ReadByte();
                                                sz = mStaticsReader.ReadSByte();
                                                sHue = mStaticsReader.ReadInt16();
                                            }

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
                                                binidx.Write((int)fsMul.Position); // lookup
                                                firstItem = false;
                                            }
                                            binmul.Write(graphic);
                                            binmul.Write(sx);
                                            binmul.Write(sy);
                                            binmul.Write(sz);
                                            binmul.Write(sHue);
                                        }
                                    }

                                    fsMulLength = (int)fsMul.Position - fsMulLength;
                                    if (fsMulLength > 0)
                                    {
                                        binidx.Write(fsMulLength); // length
                                        binidx.Write(extra); // extra
                                    }
                                    else
                                    {
                                        binidx.Write(-1); // lookup
                                        binidx.Write(-1); // length
                                        binidx.Write(-1); // extra
                                    }
                                }

                                progressBar1.PerformStep();
                            }
                        }
                    }
                }

                mIndexReader.Close();
                mStaticsReader.Close();
                mIndexCopy.Close();
                mStaticsReaderCopy.Close();
            }

            MessageBox.Show($"Files saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private class SupportedMaps
        {
            public int Id { get; }
            private string Name { get; }
            public int Height { get; }
            public int Width { get; }

            protected SupportedMaps(int id, string name, int width, int height)
            {
                Id = id;
                Name = name;
                Width = width;
                Height = height;
            }

            public override string ToString()
            {
                return $"{Id} - {Name} : {Width}x{Height}";
            }
        }

        private class RFeluccaOld : SupportedMaps
        {
            public RFeluccaOld() : base(0, Options.MapNames[0] + "Old", 6144, 4096) { }
        }

        private class RFelucca : SupportedMaps
        {
            public RFelucca() : base(0, Options.MapNames[0], 7168, 4096) { }
        }

        private class RTrammel : SupportedMaps
        {
            public RTrammel() : base(1, Options.MapNames[1], 7168, 4096) { }
        }

        private class RIlshenar : SupportedMaps
        {
            public RIlshenar() : base(2, Options.MapNames[2], 2304, 1600) { }
        }

        private class RMalas : SupportedMaps
        {
            public RMalas() : base(3, Options.MapNames[3], 2560, 2048) { }
        }

        private class RTokuno : SupportedMaps
        {
            public RTokuno() : base(4, Options.MapNames[4], 1448, 1448) { }
        }

        private class RTerMur : SupportedMaps
        {
            public RTerMur() : base(5, Options.MapNames[5], 1280, 4096) { }
        }
    }
}
