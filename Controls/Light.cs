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
            LandTileText.Text = LandTile.ToString();
            LightTileText.Text = LightTile.ToString();
        }

        private bool Loaded = false;
        private int LandTile = 0x3;
        private int LightTile = 0x0B20;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (Loaded)
                OnLoad(this, EventArgs.Empty);
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
                    TreeNode treeNode = new TreeNode(i.ToString());
                    treeNode.Tag = i;
                    treeView1.Nodes.Add(treeNode);
                }
            }
            treeView1.EndUpdate();
            if (treeView1.Nodes.Count > 0)
                treeView1.SelectedNode = treeView1.Nodes[0];
            if (!Loaded)
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private unsafe Bitmap GetImage()
        {
            if (treeView1.SelectedNode == null)
                return null;
            if (!iGPreviewToolStripMenuItem.Checked)
                return Ultima.Light.GetLight((int)treeView1.SelectedNode.Tag);

            Bitmap bit = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            using (Graphics g = Graphics.FromImage(bit))
            {
                Bitmap background = Ultima.Art.GetLand(LandTile);
                if (background != null)
                {
                    int i = 0;
                    for (int y = -22; y <= bit.Height; y += 22)
                    {
                        int x;
                        if (i % 2 == 0)
                            x = 0;
                        else
                            x = -22;

                        for (; x <= bit.Width; x += 44)
                        {
                            g.DrawImage(background, x, y);
                        }
                        ++i;
                    }
                }
                Bitmap lightbit = Ultima.Art.GetStatic(LightTile);
                if (lightbit != null)
                    g.DrawImage(lightbit, ((bit.Width - lightbit.Width) / 2), ((bit.Height - lightbit.Height) / 2));
            }

            int lightwidth, lightheight;
            byte[] light = Ultima.Light.GetRawLight((int)treeView1.SelectedNode.Tag, out lightwidth, out lightheight);

            if (light != null)
            {
                BitmapData bd = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                byte* imgPtr = (byte*)(bd.Scan0);

                int lightstartX = (bit.Width / 2) - (lightwidth / 2);
                int lightstartY = 30 + (bit.Height / 2) - (lightheight / 2);
                int lightendX = lightstartX + lightwidth;
                int lightendY = lightstartY + lightwidth;
                byte r, g, b;

                for (int y = 0; y < bd.Height; ++y)
                {
                    for (int x = 0; x < bd.Width; ++x)
                    {
                        b = *(imgPtr + 0);
                        g = *(imgPtr + 1);
                        r = *(imgPtr + 2);
                        double lightc = 0;
                        if ((x >= lightstartX) && (x < lightendX) && (y >= lightstartY) && (y < lightendY))
                        {
                            int offset = (y - lightstartY) * lightheight + (x - lightstartX);
                            if (offset < light.Length)
                            {
                                lightc = light[offset];
                                if (lightc > 31)
                                    lightc = 0;
                                else
                                    lightc *= 3 / 31D;
                            }
                        }
                        r /= 3;
                        g /= 3;
                        b /= 3;
                        r += (byte)(r * lightc);
                        g += (byte)(g * lightc);
                        b += (byte)(b * lightc);

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
                return;
            int i = (int)treeView1.SelectedNode.Tag;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove {0} (0x{0:X})", i),
                        "Remove",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Ultima.Light.Remove(i);
                treeView1.Nodes.Remove(treeView1.SelectedNode);
                treeView1.Invalidate();
                Options.ChangedUltimaClass["Light"] = true;
            }
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Choose image file to replace";
                    dialog.CheckFileExists = true;
                    dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Bitmap bmp = new Bitmap(dialog.FileName);
                        if (dialog.FileName.Contains(".bmp"))
                            bmp = Utils.ConvertBmp(bmp);
                        int i = (int)treeView1.SelectedNode.Tag;
                        Ultima.Light.Replace(i, bmp);
                        treeView1.Invalidate();
                        AfterSelect(this, (TreeViewEventArgs)null);
                        Options.ChangedUltimaClass["Light"] = true;
                    }
                }
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, 99))
            {
                if (Ultima.Light.TestLight(index))
                    InsertText.ForeColor = Color.Red;
                else
                    InsertText.ForeColor = Color.Black;
            }
            else
                InsertText.ForeColor = Color.Red;
        }

        private void OnKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, 99))
                {
                    if (Ultima.Light.TestLight(index))
                        return;
                    contextMenuStrip1.Close();
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = String.Format("Choose image file to insert at {0} (0x{0:X})", index);
                        dialog.CheckFileExists = true;
                        dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            Ultima.Light.Replace(index, bmp);
                            TreeNode treeNode = new TreeNode(index.ToString());
                            treeNode.Tag = index;
                            bool done = false;
                            foreach (TreeNode node in treeView1.Nodes)
                            {
                                if ((int)node.Tag > index)
                                {
                                    treeView1.Nodes.Insert(node.Index, treeNode);
                                    done = true;
                                    break;
                                }
                            }
                            if (!done)
                                treeView1.Nodes.Add(treeNode);
                            treeView1.Invalidate();
                            treeView1.SelectedNode = treeNode;
                            Options.ChangedUltimaClass["Light"] = true;
                        }
                    }
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Ultima.Light.Save(FiddlerControls.Options.OutputPath);
            MessageBox.Show(
                    String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                    "Save",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Light"] = false;
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            string path = FiddlerControls.Options.OutputPath;
            int i = (int)treeView1.SelectedNode.Tag;
            string FileName = Path.Combine(path, String.Format("Light {0}.bmp", i));
            Ultima.Light.GetLight(i).Save(FileName, ImageFormat.Bmp);
            MessageBox.Show(
                String.Format("Light saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            string path = FiddlerControls.Options.OutputPath;
            int i = (int)treeView1.SelectedNode.Tag;
            string FileName = Path.Combine(path, String.Format("Light {0}.tiff", i));
            Ultima.Light.GetLight(i).Save(FileName, ImageFormat.Tiff);
            MessageBox.Show(
                String.Format("Light saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            string path = FiddlerControls.Options.OutputPath;
            int i = (int)treeView1.SelectedNode.Tag;
            string FileName = Path.Combine(path, String.Format("Light {0}.jpg", i));
            Ultima.Light.GetLight(i).Save(FileName, ImageFormat.Jpeg);
            MessageBox.Show(
                String.Format("Light saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void IGPreviewClicked(object sender, EventArgs e)
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
            if (!Loaded)
                return;
            int index;
            if (Utils.ConvertStringToInt(LandTileText.Text, out index, 0, 0x3FFF))
            {
                if (!Ultima.Art.IsValidLand(index))
                    LandTileText.ForeColor = Color.Red;
                else
                    LandTileText.ForeColor = Color.Black;
            }
            else
                LandTileText.ForeColor = Color.Red;
        }

        private void LandTileKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(LandTileText.Text, out index, 0, 0x3FFF))
                {
                    if (!Ultima.Art.IsValidLand(index))
                        return;
                    contextMenuStrip2.Close();
                    LandTile = index;
                    pictureBox1.Image = GetImage();
                }
            }
        }

        private void LightTileTextChanged(object sender, EventArgs e)
        {
            if (!Loaded)
                return;
            int index;
            if (Utils.ConvertStringToInt(LightTileText.Text, out index, 0, Ultima.Art.GetMaxItemID()))
            {
                if (!Ultima.Art.IsValidStatic(index))
                    LightTileText.ForeColor = Color.Red;
                else
                    LightTileText.ForeColor = Color.Black;
            }
            else
                LightTileText.ForeColor = Color.Red;
        }

        private void LightTileKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(LightTileText.Text, out index, 0, Ultima.Art.GetMaxItemID()))
                {
                    if (!Ultima.Art.IsValidStatic(index))
                        return;
                    contextMenuStrip2.Close();
                    LightTile = index;
                    pictureBox1.Image = GetImage();
                }
            }
        }
    }
}
