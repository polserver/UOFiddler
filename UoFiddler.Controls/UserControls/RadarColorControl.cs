﻿/***************************************************************************
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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class RadarColorControl : UserControl
    {
        public RadarColorControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;
        }

        private int _selectedIndex = -1;
        private short _currCol = -1;
        private static RadarColorControl _refMarker;
        private bool _updating;
        public bool IsLoaded { get; private set; }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public short CurrColor
        {
            get => _currCol;
            set
            {
                if (_currCol == value)
                {
                    return;
                }

                _currCol = value;
                _updating = true;
                numericUpDownShortCol.Value = _currCol;
                Color col = Hues.HueToColor(_currCol);
                pictureBoxColor.BackColor = col;
                numericUpDownR.Value = col.R;
                numericUpDownG.Value = col.G;
                numericUpDownB.Value = col.B;
                _updating = false;
            }
        }

        public static void Select(int graphic, bool land)
        {
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            const int index = 0;
            if (land)
            {
                for (int i = index; i < _refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewLand.Nodes[i];
                    if ((int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabControl2.SelectTab(1);
                    _refMarker.treeViewLand.SelectedNode = node;
                    node.EnsureVisible();
                    break;
                }
            }
            else
            {
                for (int i = index; i < _refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewItem.Nodes[i];
                    if ((int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabControl2.SelectTab(0);
                    _refMarker.treeViewItem.SelectedNode = node;
                    node.EnsureVisible();
                    break;
                }
            }
        }

        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }

        public void OnLoad(object sender, EventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (IsLoaded && (!(e is MyEventArgs args) || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            treeViewItem.BeginUpdate();
            try
            {
                treeViewItem.Nodes.Clear();
                if (TileData.ItemTable != null)
                {
                    TreeNode[] nodes = new TreeNode[Art.GetMaxItemID() + 1];
                    for (int i = 0; i < Art.GetMaxItemID() + 1; ++i)
                    {
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                        {
                            Tag = i
                        };
                    }
                    treeViewItem.Nodes.AddRange(nodes);
                }
            }
            finally
            {
                treeViewItem.EndUpdate();
            }

            treeViewLand.BeginUpdate();
            try
            {
                treeViewLand.Nodes.Clear();
                if (TileData.LandTable != null)
                {
                    TreeNode[] nodes = new TreeNode[TileData.LandTable.Length];
                    for (int i = 0; i < TileData.LandTable.Length; ++i)
                    {
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name))
                        {
                            Tag = i
                        };
                    }
                    treeViewLand.Nodes.AddRange(nodes);
                }
            }
            finally
            {
                treeViewLand.EndUpdate();
            }

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            _selectedIndex = (int)e.Node.Tag;
            try
            {
                Bitmap bitmap = Art.GetStatic(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                Graphics newGraphic = Graphics.FromImage(newBitmap);
                newGraphic.Clear(Color.FromArgb(-1));
                newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                pictureBoxArt.Image = newBitmap;
            }
            catch
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }
            CurrColor = RadarCol.GetItemColor(_selectedIndex);
        }

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            _selectedIndex = (int)e.Node.Tag;
            try
            {
                Bitmap bitmap = Art.GetLand(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                Graphics newGraphic = Graphics.FromImage(newBitmap);
                newGraphic.Clear(Color.FromArgb(-1));
                newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                pictureBoxArt.Image = newBitmap;
            }
            catch
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }
            CurrColor = RadarCol.GetLandColor(_selectedIndex);
        }

        private void OnClickMeanColor(object sender, EventArgs e)
        {
            Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(_selectedIndex) : Art.GetLand(_selectedIndex);
            if (image == null)
            {
                return;
            }

            unsafe
            {
                BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                ushort* cur = line;
                int meanr = 0;
                int meang = 0;
                int meanb = 0;
                int count = 0;
                for (int y = 0; y < image.Height; ++y, line += delta)
                {
                    cur = line;
                    for (int x = 0; x < image.Width; ++x)
                    {
                        if (cur[x] == 0)
                        {
                            continue;
                        }

                        meanr += Hues.HueToColorR((short)cur[x]);
                        meang += Hues.HueToColorG((short)cur[x]);
                        meanb += Hues.HueToColorB((short)cur[x]);
                        ++count;
                    }
                }
                image.UnlockBits(bd);
                meanr /= count;
                meang /= count;
                meanb /= count;
                Color col = Color.FromArgb(meanr, meang, meanb);
                CurrColor = Hues.ColorToHue(col);
            }
        }

        private void OnClickSaveFile(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "radarcol.mul");
            RadarCol.Save(fileName);
            MessageBox.Show($"RadarCol saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["RadarCol"] = false;
        }

        private void OnClickSaveColor(object sender, EventArgs e)
        {
            if (_selectedIndex < 0)
            {
                return;
            }

            if (tabControl2.SelectedIndex == 0)
            {
                RadarCol.SetItemColor(_selectedIndex, CurrColor);
            }
            else
            {
                RadarCol.SetLandColor(_selectedIndex, CurrColor);
            }

            Options.ChangedUltimaClass["RadarCol"] = true;
        }

        private void OnChangeR(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrColor = Hues.ColorToHue(col);
        }

        private void OnChangeG(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrColor = Hues.ColorToHue(col);
        }

        private void OnChangeB(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrColor = Hues.ColorToHue(col);
        }

        private void OnNumericShortColChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                CurrColor = (short)numericUpDownShortCol.Value;
            }
        }

        private void OnClickmeanColorFromTo(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(textBoxMeanFrom.Text, out int from, 0, 0x4000) ||
                !Utils.ConvertStringToInt(textBoxMeanTo.Text, out int to, 0, 0x4000))
            {
                return;
            }

            if (to < from)
            {
                int temp = from;
                from = to;
                to = temp;
            }

            int gmeanr = 0;
            int gmeang = 0;
            int gmeanb = 0;

            for (int i = from; i < to; ++i)
            {
                Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(i) : Art.GetLand(i);
                if (image == null)
                {
                    continue;
                }

                unsafe
                {
                    BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                    ushort* line = (ushort*)bd.Scan0;
                    int delta = bd.Stride >> 1;
                    ushort* cur = line;
                    int meanr = 0;
                    int meang = 0;
                    int meanb = 0;
                    int count = 0;
                    for (int y = 0; y < image.Height; ++y, line += delta)
                    {
                        cur = line;
                        for (int x = 0; x < image.Width; ++x)
                        {
                            if (cur[x] != 0)
                            {
                                meanr += Hues.HueToColorR((short)cur[x]);
                                meang += Hues.HueToColorG((short)cur[x]);
                                meanb += Hues.HueToColorB((short)cur[x]);
                                ++count;
                            }
                        }
                    }
                    image.UnlockBits(bd);
                    meanr /= count;
                    meang /= count;
                    meanb /= count;
                    gmeanr += meanr;
                    gmeang += meang;
                    gmeanb += meanb;
                }
            }
            gmeanr /= to - from;
            gmeang /= to - from;
            gmeanb /= to - from;
            Color col = Color.FromArgb(gmeanr, gmeang, gmeanb);
            CurrColor = Hues.ColorToHue(col);
        }

        private void OnClickSelectItemsTab(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            if (Options.DesignAlternative)
            {
                ItemShowAlternativeControl.SearchGraphic(index);
            }
            else
            {
                ItemShowControl.SearchGraphic(index);
            }
        }

        private void OnClickSelectItemTiledataTab(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            TileDataControl.Select(index, false);
        }

        private void OnClickSelectLandtilesTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            if (Options.DesignAlternative)
            {
                LandTilesAlternativeControl.SearchGraphic(index);
            }
            else
            {
                LandTilesControl.SearchGraphic(index);
            }
        }

        private void OnClickSelectLandTiledataTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            TileDataControl.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["RadarCol"] = true;
                RadarCol.ImportFromCSV(dialog.FileName);
                if (tabControl2.SelectedTab == tabControl2.TabPages[0])
                {
                    if (treeViewItem.SelectedNode != null)
                    {
                        AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                    }
                }
                else
                {
                    if (treeViewLand.SelectedNode != null)
                    {
                        AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                    }
                }
            }
            dialog.Dispose();
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "RadarColor.csv");
            RadarCol.ExportToCSV(fileName);
            MessageBox.Show($"RadarColor saved to {fileName}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}
