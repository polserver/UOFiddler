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
using System.Text;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class MultiMap : UserControl
    {
        public MultiMap()
        {
            InitializeComponent();
            multiMapToolStripMenuItem.Tag = -1;
            facet00ToolStripMenuItem.Tag = 0;
            facet01ToolStripMenuItem.Tag = 1;
            facet02ToolStripMenuItem.Tag = 2;
            facet03ToolStripMenuItem.Tag = 3;
            facet04ToolStripMenuItem.Tag = 4;
            facet05ToolStripMenuItem.Tag = 5;
        }

        bool moving = false;
        Point movingpoint;

        private bool Loaded = false;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!Loaded)
                return;
            moving = false;
            ToolStripMenuItem strip;
            if (multiMapToolStripMenuItem.Checked)
                strip = multiMapToolStripMenuItem;
            else if (facet00ToolStripMenuItem.Checked)
                strip = facet00ToolStripMenuItem;
            else if (facet01ToolStripMenuItem.Checked)
                strip = facet01ToolStripMenuItem;
            else if (facet02ToolStripMenuItem.Checked)
                strip = facet02ToolStripMenuItem;
            else if (facet03ToolStripMenuItem.Checked)
                strip = facet03ToolStripMenuItem;
            else if (facet04ToolStripMenuItem.Checked)
                strip = facet04ToolStripMenuItem;
            else if (facet05ToolStripMenuItem.Checked)
                strip = facet05ToolStripMenuItem;
            else
                return;
            strip.Checked = false;
            ShowImage(strip, EventArgs.Empty);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
            {
                DisplayScrollBars();
                SetScrollBarValues();
                Refresh();
            }
        }

        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void DisplayScrollBars()
        {
            if (pictureBox.Width > pictureBox.Image.Width - vScrollBar.Width)
                hScrollBar.Enabled = false;
            else
                hScrollBar.Enabled = true;

            if (pictureBox.Height >
                pictureBox.Image.Height - hScrollBar.Height)
                vScrollBar.Enabled = false;
            else
                vScrollBar.Enabled = true;
        }

        private void SetScrollBarValues()
        {
            vScrollBar.Minimum = 0;
            hScrollBar.Minimum = 0;
            if ((pictureBox.Image.Size.Width - pictureBox.ClientSize.Width) > 0)
                hScrollBar.Maximum = pictureBox.Image.Size.Width - pictureBox.ClientSize.Width;

            hScrollBar.LargeChange = hScrollBar.Maximum / 10;
            hScrollBar.SmallChange = hScrollBar.Maximum / 20;

            hScrollBar.Maximum += hScrollBar.LargeChange;

            if ((pictureBox.Image.Size.Height - pictureBox.ClientSize.Height) > 0)
                vScrollBar.Maximum = pictureBox.Image.Size.Height - pictureBox.ClientSize.Height;

            vScrollBar.LargeChange = vScrollBar.Maximum / 10;
            vScrollBar.SmallChange = vScrollBar.Maximum / 20;

            vScrollBar.Maximum += vScrollBar.LargeChange;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                moving = true;
                movingpoint.X = e.X;
                movingpoint.Y = e.Y;
                this.Cursor = Cursors.Hand;
            }
            else
            {
                moving = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (moving)
            {
                if (pictureBox.Image != null)
                {
                    int deltax = (int)(-1 * (e.X - movingpoint.X));
                    int deltay = (int)(-1 * (e.Y - movingpoint.Y));
                    movingpoint.X = e.X;
                    movingpoint.Y = e.Y;
                    hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltax));
                    vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltay));
                    pictureBox.Invalidate();
                }
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
            this.Cursor = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("{0}.bmp",CheckedToString()));
            Bitmap bit = new Bitmap(pictureBox.Image);
            bit.Save(FileName, ImageFormat.Bmp);
            MessageBox.Show(String.Format("{0} saved to {1}", CheckedToString(), FileName), "Export",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("{0}.tiff", CheckedToString()));
            pictureBox.Image.Save(FileName, ImageFormat.Tiff);
            MessageBox.Show(String.Format("{0} saved to {1}", CheckedToString(), FileName), "Export",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("{0}.jpg", CheckedToString()));
            pictureBox.Image.Save(FileName, ImageFormat.Jpeg);
            MessageBox.Show(String.Format("{0} saved to {1}", CheckedToString(), FileName), "Export",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private string CheckedToString()
        {
            if (multiMapToolStripMenuItem.Checked)
                return "MultiMap";
            else if (facet00ToolStripMenuItem.Checked)
                return "Facet00";
            else if (facet01ToolStripMenuItem.Checked)
                return "Facet01";
            else if (facet02ToolStripMenuItem.Checked)
                return "Facet02";
            else if (facet03ToolStripMenuItem.Checked)
                return "Facet03";
            else if (facet04ToolStripMenuItem.Checked)
                return "Facet04";
            else if (facet05ToolStripMenuItem.Checked)
                return "Facet05";
            return "Unk";
        }

        private void ShowImage(object sender, EventArgs e)
        {
            ToolStripMenuItem strip = sender as ToolStripMenuItem;
            if (strip != null)
            {
                if (!strip.Checked)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    multiMapToolStripMenuItem.Checked = 
                        facet00ToolStripMenuItem.Checked = 
                        facet01ToolStripMenuItem.Checked = 
                        facet02ToolStripMenuItem.Checked = 
                        facet03ToolStripMenuItem.Checked = 
                        facet04ToolStripMenuItem.Checked = 
                        facet05ToolStripMenuItem.Checked = false;
                    strip.Checked = true;
                    if ((int)strip.Tag==-1)
                        pictureBox.Image = Ultima.MultiMap.GetMultiMap();
                    else
                        pictureBox.Image = Ultima.MultiMap.GetFacetImage((int)strip.Tag);
                    if (pictureBox.Image != null)
                    {
                        DisplayScrollBars();
                        SetScrollBarValues();
                    }
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void OnClickGenerateRLE(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Image to convert";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap image = new Bitmap(dialog.FileName);
                    if (image != null)
                    {
                        if ((image.Height != 2048) || (image.Width != 2560))
                        {
                            MessageBox.Show("Invalid image height or width", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            return;
                        }
                        Cursor.Current = Cursors.WaitCursor;
                        string path = FiddlerControls.Options.OutputPath;
                        string FileName = Path.Combine(path, "MultiMap.rle");
                        using (FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                        {
                            BinaryWriter bin = new BinaryWriter(fs, Encoding.Unicode);
                            Ultima.MultiMap.SaveMultiMap(image, bin);
                        }
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(String.Format("MultiMap saved to {0}", FileName), "Convert", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                    else
                        MessageBox.Show("No image found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClickGenerateFacetFromImage(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Image to convert";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Bitmap image = new Bitmap(dialog.FileName);
                    if (image != null)
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        string path = FiddlerControls.Options.OutputPath;
                        string FileName = Path.Combine(path, "facet.mul");
                        Ultima.MultiMap.SaveFacetImage(FileName, image);
                        Cursor.Current = Cursors.Default;
                        MessageBox.Show(String.Format("Facet saved to {0}", FileName), "Convert", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                    else
                        MessageBox.Show("No image found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (!Loaded)
            {
                multiMapToolStripMenuItem.Checked = true;
                pictureBox.Image = Ultima.MultiMap.GetMultiMap();
                if (pictureBox.Image != null)
                {
                    DisplayScrollBars();
                    SetScrollBarValues();
                }
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                Loaded = true;
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            if (pictureBox.Image != null)
                e.Graphics.DrawImage(pictureBox.Image,
                    e.ClipRectangle,
                    hScrollBar.Value,vScrollBar.Value,e.ClipRectangle.Width,e.ClipRectangle.Height,
                    GraphicsUnit.Pixel);
        }
    }
}
