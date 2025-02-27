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
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class LightControl : UserControl
    {
        public LightControl()
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
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Light"] = true;

            treeViewLights.BeginUpdate();
            try
            {
                treeViewLights.Nodes.Clear();
                for (int i = 0; i < Ultima.Light.GetCount(); ++i)
                {
                    if (!Ultima.Light.TestLight(i))
                    {
                        continue;
                    }

                    var treeNode = new TreeNode(i.ToString())
                    {
                        Tag = i
                    };
                    treeViewLights.Nodes.Add(treeNode);
                }
            }
            finally
            {
                treeViewLights.EndUpdate();
            }

            if (treeViewLights.Nodes.Count > 0)
            {
                treeViewLights.SelectedNode = treeViewLights.Nodes[0];
            }

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
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
            if (treeViewLights.SelectedNode == null)
            {
                return null;
            }

            if (!iGPreviewToolStripMenuItem.Checked)
            {
                return Ultima.Light.GetLight((int)treeViewLights.SelectedNode.Tag);
            }

            var bit = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
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

            byte[] light = Ultima.Light.GetRawLight((int)treeViewLights.SelectedNode.Tag, out int lightWidth, out int lightHeight);

            if (light == null)
            {
                return bit;
            }

            BitmapData bd = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte* imgPtr = (byte*)bd.Scan0;

            int lightStartX = (bit.Width / 2) - (lightWidth / 2);
            int lightStartY = 30 + (bit.Height / 2) - (lightHeight / 2);

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
                        int offset = ((y - lightStartY) * lightHeight) + (x - lightStartX);
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
                imgPtr += bd.Stride - (bd.Width * 3);
            }
            bit.UnlockBits(bd);

            return bit;
        }

        private void AfterSelect(object sender, TreeViewEventArgs e)
        {
            pictureBoxPreview.Image = GetImage();
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            int i = (int)treeViewLights.SelectedNode.Tag;
            DialogResult result = MessageBox.Show(string.Format("Are you sure to remove {0} (0x{0:X})", i), "Remove",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Ultima.Light.Remove(i);
            treeViewLights.Nodes.Remove(treeViewLights.SelectedNode);
            treeViewLights.Invalidate();
            Options.ChangedUltimaClass["Light"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    var bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    int i = (int)treeViewLights.SelectedNode.Tag;

                    Ultima.Light.Replace(i, bitmap);

                    treeViewLights.Invalidate();
                    AfterSelect(this, null);

                    Options.ChangedUltimaClass["Light"] = true;
                }
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

            treeViewContextMenuStrip.Close();
            using (var dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = string.Format("Choose image file to insert at {0} (0x{0:X})", index);
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var bmp = new Bitmap(dialog.FileName);
                Ultima.Light.Replace(index, bmp);
                var treeNode = new TreeNode(index.ToString())
                {
                    Tag = index
                };
                bool done = false;
                foreach (TreeNode node in treeViewLights.Nodes)
                {
                    if ((int)node.Tag <= index)
                    {
                        continue;
                    }

                    treeViewLights.Nodes.Insert(node.Index, treeNode);
                    done = true;
                    break;
                }
                if (!done)
                {
                    treeViewLights.Nodes.Add(treeNode);
                }

                treeViewLights.Invalidate();
                treeViewLights.SelectedNode = treeNode;
                Options.ChangedUltimaClass["Light"] = true;
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Ultima.Light.Save(Options.OutputPath);
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Light"] = false;
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeViewLights.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.bmp");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Bmp);
            MessageBox.Show($"Light saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeViewLights.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.tiff");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Tiff);
            MessageBox.Show($"Light saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (treeViewLights.SelectedNode == null)
            {
                return;
            }

            string path = Options.OutputPath;
            int i = (int)treeViewLights.SelectedNode.Tag;
            string fileName = Path.Combine(path, $"Light {i}.jpg");
            Ultima.Light.GetLight(i).Save(fileName, ImageFormat.Jpeg);
            MessageBox.Show($"Light saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void IgPreviewClicked(object sender, EventArgs e)
        {
            iGPreviewToolStripMenuItem.Checked = !iGPreviewToolStripMenuItem.Checked;
            pictureBoxPreview.Image = GetImage();
        }

        private void OnPictureSizeChanged(object sender, EventArgs e)
        {
            pictureBoxPreview.Image = GetImage();
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

            previewContextMenuStrip.Close();
            _landTile = index;
            pictureBoxPreview.Image = GetImage();
        }

        private void LightTileTextChanged(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            if (Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemId()))
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
            if (e.KeyCode != Keys.Enter ||
                !Utils.ConvertStringToInt(LightTileText.Text, out int index, 0, Ultima.Art.GetMaxItemId()) ||
                !Ultima.Art.IsValidStatic(index)
                )
            {
                return;
            }

            previewContextMenuStrip.Close();

            _lightTile = index;

            pictureBoxPreview.Image = GetImage();
        }
    }
}
