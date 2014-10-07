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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class RadarColor : UserControl
    {
        public RadarColor()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            refMarker = this;
        }

        private bool Loaded = false;
        private int SelectedIndex = -1;
        private short CurrCol = -1;
        private static RadarColor refMarker;
        private bool Updating = false;
        public bool isLoaded { get { return Loaded; } }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public short CurrColor
        {
            get { return CurrCol; }
            set
            {
                if (CurrCol != value)
                {
                    CurrCol = value;
                    Updating = true;
                    numericUpDownShortCol.Value = CurrCol;
                    Color col = Ultima.Hues.HueToColor(CurrCol);
                    pictureBoxColor.BackColor = col;
                    numericUpDownR.Value = col.R;
                    numericUpDownG.Value = col.G;
                    numericUpDownB.Value = col.B;
                    Updating = false;
                }
            }
        }

        public static void Select(int graphic, bool land)
        {
            if (!refMarker.isLoaded)
                refMarker.OnLoad(refMarker, EventArgs.Empty);
            int index = 0;
            if (land)
            {
                for (int i = index; i < refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = refMarker.treeViewLand.Nodes[i];
                    if ((int)node.Tag == graphic)
                    {
                        refMarker.tabControl2.SelectTab(1);
                        refMarker.treeViewLand.SelectedNode = node;
                        node.EnsureVisible();
                        break;
                    }
                }
            }
            else
            {
                for (int i = index; i < refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    TreeNode node = refMarker.treeViewItem.Nodes[i];
                    if ((int)node.Tag == graphic)
                    {
                        refMarker.tabControl2.SelectTab(0);
                        refMarker.treeViewItem.SelectedNode = node;
                        node.EnsureVisible();
                        break;
                    }
                }
            }
        }
        private void Reload()
        {
            if (Loaded)
                OnLoad(this, new MyEventArgs(MyEventArgs.TYPES.FORCERELOAD));
        }

        public void OnLoad(object sender, EventArgs e)
        {
            MyEventArgs _args = e as MyEventArgs;
            if (Loaded && (_args == null || _args.Type != MyEventArgs.TYPES.FORCERELOAD))
                return;
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            treeViewItem.BeginUpdate();
            treeViewItem.Nodes.Clear();
            if (TileData.ItemTable != null)
            {
                TreeNode[] nodes = new TreeNode[Art.GetMaxItemID() + 1];
                for (int i = 0; i < Art.GetMaxItemID() + 1; ++i)
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name));
                    node.Tag = i;
                    nodes[i] = node;
                }
                treeViewItem.Nodes.AddRange(nodes);
            }
            treeViewItem.EndUpdate();
            treeViewLand.BeginUpdate();
            treeViewLand.Nodes.Clear();
            if (TileData.LandTable != null)
            {
                TreeNode[] nodes = new TreeNode[TileData.LandTable.Length];
                for (int i = 0; i < TileData.LandTable.Length; ++i)
                {
                    TreeNode node = new TreeNode(String.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name));
                    node.Tag = i;
                    nodes[i] = node;
                }
                treeViewLand.Nodes.AddRange(nodes);
            }
            treeViewLand.EndUpdate();
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void AfterSelectTreeViewitem(object sender, TreeViewEventArgs e)
        {
            SelectedIndex = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Art.GetStatic(SelectedIndex);
                Bitmap newbit = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                newgraph.DrawImage(bit, (pictureBoxArt.Size.Width - bit.Width) / 2, 1);
                pictureBoxArt.Image = newbit;
            }
            catch
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }
            CurrColor = Ultima.RadarCol.GetItemColor(SelectedIndex);
        }

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            SelectedIndex = (int)e.Node.Tag;
            try
            {
                Bitmap bit = Ultima.Art.GetLand(SelectedIndex);
                Bitmap newbit = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                Graphics newgraph = Graphics.FromImage(newbit);
                newgraph.Clear(Color.FromArgb(-1));
                newgraph.DrawImage(bit, (pictureBoxArt.Size.Width - bit.Width) / 2, 1);
                pictureBoxArt.Image = newbit;
            }
            catch
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }
            CurrColor = Ultima.RadarCol.GetLandColor(SelectedIndex);
        }

        private void OnClickMeanColor(object sender, EventArgs e)
        {
            Bitmap image;
            if (tabControl2.SelectedIndex == 0)
                image = Art.GetStatic(SelectedIndex);
            else
                image = Art.GetLand(SelectedIndex);
            if (image == null)
                return;
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
                            meanr += Ultima.Hues.HueToColorR((short)cur[x]);
                            meang += Ultima.Hues.HueToColorG((short)cur[x]);
                            meanb += Ultima.Hues.HueToColorB((short)cur[x]);
                            ++count;
                        }
                    }
                }
                image.UnlockBits(bd);
                meanr /= count;
                meang /= count;
                meanb /= count;
                Color col = Color.FromArgb(meanr, meang, meanb);
                CurrColor = Ultima.Hues.ColorToHue(col);
            }
        }

        private void onClickSaveFile(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, "radarcol.mul");
            Ultima.RadarCol.Save(FileName);
            MessageBox.Show(
                String.Format("RadarCol saved to {0}", FileName),
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["RadarCol"] = false;
        }

        private void onClickSaveColor(object sender, EventArgs e)
        {
            if (SelectedIndex >= 0)
            {
                if (tabControl2.SelectedIndex == 0)
                    Ultima.RadarCol.SetItemColor(SelectedIndex, CurrColor);
                else
                    Ultima.RadarCol.SetLandColor(SelectedIndex, CurrColor);
                Options.ChangedUltimaClass["RadarCol"] = true;
            }
        }

        private void onChangeR(object sender, EventArgs e)
        {
            if (!Updating)
            {
                Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
                CurrColor = Ultima.Hues.ColorToHue(col);
            }
        }

        private void OnChangeG(object sender, EventArgs e)
        {
            if (!Updating)
            {
                Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
                CurrColor = Ultima.Hues.ColorToHue(col);
            }
        }

        private void OnChangeB(object sender, EventArgs e)
        {
            if (!Updating)
            {
                Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
                CurrColor = Ultima.Hues.ColorToHue(col);
            }
        }

        private void OnNumericShortColChanged(object sender, EventArgs e)
        {
            if (!Updating)
            {
                CurrColor = (short)numericUpDownShortCol.Value;
            }
        }

        private void OnClickmeanColorFromTo(object sender, EventArgs e)
        {
            int from, to;
            if ((Utils.ConvertStringToInt(textBoxMeanFrom.Text, out from, 0, 0x4000)) &&
                (Utils.ConvertStringToInt(textBoxMeanTo.Text, out to, 0, 0x4000)))
            {
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
                    Bitmap image;
                    if (tabControl2.SelectedIndex == 0)
                        image = Art.GetStatic(i);
                    else
                        image = Art.GetLand(i);
                    if (image == null)
                        continue;
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
                                    meanr += Ultima.Hues.HueToColorR((short)cur[x]);
                                    meang += Ultima.Hues.HueToColorG((short)cur[x]);
                                    meanb += Ultima.Hues.HueToColorB((short)cur[x]);
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
                gmeanr /= (to - from);
                gmeang /= (to - from);
                gmeanb /= (to - from);
                Color col = Color.FromArgb(gmeanr, gmeang, gmeanb);
                CurrColor = Ultima.Hues.ColorToHue(col);
            }
        }

        private void OnClickSelectItemsTab(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
                return;
            int index = (int)treeViewItem.SelectedNode.Tag;
            if (Options.DesignAlternative)
                FiddlerControls.ItemShowAlternative.SearchGraphic(index);
            else
                FiddlerControls.ItemShow.SearchGraphic(index);
        }

        private void OnClickSelectItemTiledataTab(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
                return;
            int index = (int)treeViewItem.SelectedNode.Tag;
            FiddlerControls.TileDatas.Select(index, false);
        }

        private void OnClickSelectLandtilesTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
                return;
            int index = (int)treeViewLand.SelectedNode.Tag;
            if (Options.DesignAlternative)
                FiddlerControls.LandTilesAlternative.SearchGraphic(index);
            else
                FiddlerControls.LandTiles.SearchGraphic(index);
        }

        private void OnClickSelectLandTiledataTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
                return;
            int index = (int)treeViewLand.SelectedNode.Tag;
            FiddlerControls.TileDatas.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose csv file to import";
            dialog.CheckFileExists = true;
            dialog.Filter = "csv files (*.csv)|*.csv";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["RadarCol"] = true;
                Ultima.RadarCol.ImportFromCSV(dialog.FileName);
                if (tabControl2.SelectedTab == tabControl2.TabPages[0])
                {
                    if (treeViewItem.SelectedNode != null)
                        AfterSelectTreeViewitem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                }
                else
                {
                    if (treeViewLand.SelectedNode != null)
                        AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
                }
            }
            dialog.Dispose();
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, "RadarColor.csv");
            Ultima.RadarCol.ExportToCSV(FileName);
            MessageBox.Show(String.Format("RadarColor saved to {0}", FileName), "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }
    }
}
