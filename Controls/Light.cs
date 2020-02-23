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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace FiddlerControls
{
    public partial class Light : UserControl
    {
        public Light()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            LandTileText.Text = _landTile.ToString();
            LightTileText.Text = _lightTile.ToString();
        }

        private bool _loaded;
        private int _landTile = 0x3;
        private int _lightTile = 0x0B20;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Light"] = true;

            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            for (int i = 0; i < Ultima.Light.GetCount(); ++i)
            {
                if (Ultima.Light.TestLight(i))
                {
                    TreeNode treeNode = new TreeNode(i.ToString())
                    {
                        Tag = i
                    };
                    treeView1.Nodes.Add(treeNode);
                }
            }
            treeView1.EndUpdate();
            if (treeView1.Nodes.Count > 0)
            {
                treeView1.SelectedNode = treeView1.Nodes[0];
            }

            if (!_loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private unsafe Bitmap GetImage()
        {
            if (treeView1.SelectedNode == null)
            {
                return null;
            }

            if (!iGPreviewToolStripMenuItem.Checked)
            {
                return Ultima.Light.GetLight((int)treeView1.SelectedNode.Tag);
            }

            Bitmap bit = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            using (Graphics g = Graphics.FromImage(bit))
            {
                Bitmap background = Ultima.Art.GetLand(_landTile);
                if (background != null)
                {
                    int i = 0;
                    for (int y = -22; y <= bit.Height; y += 22)
                    {
                        int x = i % 2 == 0 ? 0 : -22;
                        for (; x <= bit.Width; x += 44)
                        {
                            g.DrawImage(background, x, y);
                        }
                        ++i;
                    }
                }
                Bitmap lightBit = Ultima.Art.GetStatic(_lightTile);
                if (lightBit != null)
                {
                    g.DrawImage(lightBit, (bit.Width - lightBit.Width) / 2, (bit.Height - lightBit.Height) / 2);
                }
            }

            byte[] light = Ultima.Light.GetRawLight((int)treeView1.SelectedNode.Tag, out int lightWidth, out int lightHeight);

            if (light != null)
            {
                BitmapData bd = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                byte* imgPtr = (byte*)bd.Scan0;

                int lightStartX = bit.Width / 2 - lightWidth / 2;
                int lightStartY = 30 + bit.Height / 2 - lightHeight / 2;

                int lightEndX = lightStartX + lightWidth;
                int lightEndY = lightStartY + lightWidth;

                for (int y = 0; y < bd.Height; ++y)
                {
                    for (int x = 0; x < bd.Width; ++x)
                    {
                        byte b = *(imgPtr + 0);
                        byte g = *(imgPtr + 1);
                        byte r = *(imgPtr + 2);

                        double lightC = 0;

                        if (x >= lightStartX && x < lightEndX && y >= lightStartY && y < lightEndY)
                        {
                            int offset = (y - lightStartY) * lightHeight + (x - lightStartX);
                            if (offset < light.Length)
                            {
                                lightC = light[offset];
                                if (lightC > 31)
                                {
                                    lightC = 0;
                                }
                                else
                                {
                                    lightC *= 3 / 31D;
                                }
                            }
                        }
                        r /= 3;
                        g /= 3;
                        b /= 3;
                        r += (byte)(r * lightC);
                        g += (byte)(g * lightC);
                        b += (byte)(b * lightC);

                        *imgPtr++ = b;
                        *imgPtr++ = g;
                        *imgPtr++ = r;
                    }
                    imgPtr += bd.Stride - bd.Width * 3;
                }
                bit.UnlockBits(bd);
            }
            return bit;
        }

        private void AfterSelect(object sender, TreeViewEventArgs e)
        {
            pictureBox1.Image = GetImage();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            int i = (int)treeView1.SelectedNode.Tag;
            DialogResult result =
                        MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", i),
                        "Remove",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Ultima.Light.Remove(i);
            treeView1.Nodes.Remove(treeView1.SelectedNode);
            treeView1.Invalidate();
            Options.ChangedUltimaClass["Light"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bmp = new Bitmap(dialog.FileName);
                if (dialog.FileName.Contains(".bmp"))
                {
                    bmp = Utils.ConvertBmp(bmp);
                }

                int i = (int)treeView1.SelectedNode.Tag;
                Ultima.Light.Replace(i, bmp);
                treeView1.Invalidate();
                AfterSelect(this, null);
                Options.ChangedUltimaClass["Light"] = true;
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 99))
            {
                InsertText.ForeColor = Ultima.Light.TestLight(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }

        private void OnKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 99))
            {
                return;
            }

            if (Ultima.Light.TestLight(index))
            {
                return;
            }

            contextMenuStrip1.Close();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = string.Format("Choose image file to insert at {0} (0x{0:X})", index);
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bmp = new Bitmap(dialog.FileName);
                Ultima.Light.Replace(index, bmp);
                TreeNode treeNode = new TreeNode(index.ToString())
                {
                    Tag = index
                };
                bool done = false;
                foreach (TreeNode node in treeView1.Nodes)
                {
                    if ((int)node.Tag <= index)
                    {
                        continue;
                    }

                    treeView1.Nodes.Insert(node.Index, treeNode);
                    done = true;
                    break;
                }
                if (!done)
                {
                    treeView1.Nodes.Add(treeNode);
                }

                treeView1.Invalidate();
                treeView1.SelectedNode = treeNode;
                Options.ChangedUltimaClass["Light"] = true;
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Ultima.Light.Save(Options.OutputPath);
            MessageBox.Show(
                $"Saved to {Options.OutputPath}",
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Light"] = false;
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeView1.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.bmp");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Bmp);
            MessageBox.Show(
                $"Light saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeView1.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.tiff");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Tiff);
            MessageBox.Show(
                $"Light saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeView1.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.jpg");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Jpeg);
            MessageBox.Show(
                $"Light saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void IgPreviewClicked(object sender, EventArgs e)
        {
            iGPreviewToolStripMenuItem.Checked = !iGPreviewToolStripMenuItem.Checked;
            pictureBox1.Image = GetImage();
        }

        private void OnPictureSizeChanged(object sender, EventArgs e)
        {
            pictureBox1.Image = GetImage();
        }

        private void LandTileTextChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            if (Utils.ConvertStringToInt(LandTileText.Text, out int index, 0, 0x3FFF))
            {
                LandTileText.ForeColor = !Ultima.Art.IsValidLand(index) ? Color.Red : Color.Black;
            }
            else
            {
                LandTileText.ForeColor = Color.Red;
            }
        }

        private void LandTileKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(LandTileText.Text, out int index, 0, 0x3FFF))
            {
                return;
            }

            if (!Ultima.Art.IsValidLand(index))
            {
                return;
            }

            contextMenuStrip2.Close();
            _landTile = index;
            pictureBox1.Image = GetImage();
        }

        private void LightTileTextChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            if (Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemID()))
            {
                LightTileText.ForeColor = !Ultima.Art.IsValidStatic(index) ? Color.Red : Color.Black;
            }
            else
            {
                LightTileText.ForeColor = Color.Red;
            }
        }

        private void LightTileKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemID()))
                {
                    if (!Ultima.Art.IsValidStatic(index))
                    {
                        return;
                    }

                    contextMenuStrip2.Close();
                    _lightTile = index;
                    pictureBox1.Image = GetImage();
                }
            }
        }
    }
}
