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
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class MultiMapControl : UserControl
    {
        public MultiMapControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            multiMapToolStripMenuItem.Tag = -1;
            facet00ToolStripMenuItem.Tag = 0;
            facet01ToolStripMenuItem.Tag = 1;
            facet02ToolStripMenuItem.Tag = 2;
            facet03ToolStripMenuItem.Tag = 3;
            facet04ToolStripMenuItem.Tag = 4;
            facet05ToolStripMenuItem.Tag = 5;
        }

        private bool _moving;
        private Point _movingPoint;

        private bool _loaded;

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            _moving = false;
            ToolStripMenuItem strip;
            if (multiMapToolStripMenuItem.Checked)
            {
                strip = multiMapToolStripMenuItem;
            }
            else if (facet00ToolStripMenuItem.Checked)
            {
                strip = facet00ToolStripMenuItem;
            }
            else if (facet01ToolStripMenuItem.Checked)
            {
                strip = facet01ToolStripMenuItem;
            }
            else if (facet02ToolStripMenuItem.Checked)
            {
                strip = facet02ToolStripMenuItem;
            }
            else if (facet03ToolStripMenuItem.Checked)
            {
                strip = facet03ToolStripMenuItem;
            }
            else if (facet04ToolStripMenuItem.Checked)
            {
                strip = facet04ToolStripMenuItem;
            }
            else if (facet05ToolStripMenuItem.Checked)
            {
                strip = facet05ToolStripMenuItem;
            }
            else
            {
                return;
            }

            strip.Checked = false;
            ShowImage(strip, EventArgs.Empty);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                return;
            }

            DisplayScrollBars();
            SetScrollBarValues();
            Refresh();
        }

        private void HandleScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void DisplayScrollBars()
        {
            hScrollBar.Enabled = pictureBox.Width <= pictureBox.Image.Width - vScrollBar.Width;
            vScrollBar.Enabled = pictureBox.Height <= pictureBox.Image.Height - hScrollBar.Height;
        }

        private void SetScrollBarValues()
        {
            vScrollBar.Minimum = 0;
            hScrollBar.Minimum = 0;
            if (pictureBox.Image.Size.Width - pictureBox.ClientSize.Width > 0)
            {
                hScrollBar.Maximum = pictureBox.Image.Size.Width - pictureBox.ClientSize.Width;
            }

            hScrollBar.LargeChange = hScrollBar.Maximum / 10;
            hScrollBar.SmallChange = hScrollBar.Maximum / 20;

            hScrollBar.Maximum += hScrollBar.LargeChange;

            if (pictureBox.Image.Size.Height - pictureBox.ClientSize.Height > 0)
            {
                vScrollBar.Maximum = pictureBox.Image.Size.Height - pictureBox.ClientSize.Height;
            }

            vScrollBar.LargeChange = vScrollBar.Maximum / 10;
            vScrollBar.SmallChange = vScrollBar.Maximum / 20;

            vScrollBar.Maximum += vScrollBar.LargeChange;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _moving = true;
                _movingPoint.X = e.X;
                _movingPoint.Y = e.Y;
                Cursor = Cursors.Hand;
            }
            else
            {
                _moving = false;
                Cursor = Cursors.Default;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_moving)
            {
                return;
            }

            if (pictureBox.Image == null)
            {
                return;
            }

            int deltaX = -1 * (e.X - _movingPoint.X);
            int deltaY = -1 * (e.Y - _movingPoint.Y);

            _movingPoint.X = e.X;
            _movingPoint.Y = e.Y;

            hScrollBar.Value = Math.Max(0, Math.Min(hScrollBar.Maximum, hScrollBar.Value + deltaX));
            vScrollBar.Value = Math.Max(0, Math.Min(vScrollBar.Maximum, vScrollBar.Value + deltaY));

            pictureBox.Invalidate();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            _moving = false;
            Cursor = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Bmp);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Tiff);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Jpeg);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            ExportMultiMapImage(ImageFormat.Png);
        }

        private void ExportMultiMapImage(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"{CheckedToString()}.{fileExtension}");

            pictureBox.Image.Save(fileName, imageFormat);

            MessageBox.Show($"{CheckedToString()} saved to {fileName}", "Export", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private string CheckedToString()
        {
            if (multiMapToolStripMenuItem.Checked)
            {
                return "MultiMap";
            }

            if (facet00ToolStripMenuItem.Checked)
            {
                return "Facet00";
            }
            if (facet01ToolStripMenuItem.Checked)
            {
                return "Facet01";
            }
            if (facet02ToolStripMenuItem.Checked)
            {
                return "Facet02";
            }
            if (facet03ToolStripMenuItem.Checked)
            {
                return "Facet03";
            }
            if (facet04ToolStripMenuItem.Checked)
            {
                return "Facet04";
            }
            if (facet05ToolStripMenuItem.Checked)
            {
                return "Facet05";
            }

            return "Unk";
        }

        private void ShowImage(object sender, EventArgs e)
        {
            if (!(sender is ToolStripMenuItem strip))
            {
                return;
            }

            if (strip.Checked)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;

            multiMapToolStripMenuItem.Checked =
                facet00ToolStripMenuItem.Checked =
                facet01ToolStripMenuItem.Checked =
                facet02ToolStripMenuItem.Checked =
                facet03ToolStripMenuItem.Checked =
                facet04ToolStripMenuItem.Checked =
                facet05ToolStripMenuItem.Checked = false;

            strip.Checked = true;

            pictureBox.Image = (int)strip.Tag == -1
                ? Ultima.MultiMap.GetMultiMap()
                : Ultima.MultiMap.GetFacetImage((int)strip.Tag);

            if (pictureBox.Image != null)
            {
                DisplayScrollBars();
                SetScrollBarValues();
            }
            Cursor.Current = Cursors.Default;
        }

        private void OnClickGenerateRLE(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Image to convert";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    Bitmap image = new Bitmap(dialog.FileName);

                    if (image.Height != 2048 || image.Width != 2560)
                    {
                        MessageBox.Show("Invalid image height or width", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        return;
                    }

                    string path = Options.OutputPath;
                    string fileName = Path.Combine(path, "MultiMap.rle");
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
                    {
                        BinaryWriter bin = new BinaryWriter(fs, Encoding.Unicode);
                        Ultima.MultiMap.SaveMultiMap(image, bin);
                    }

                    Cursor.Current = Cursors.Default;

                    MessageBox.Show($"MultiMap saved to {fileName}", "Convert",
                        MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("No image found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void OnClickGenerateFacetFromImage(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Image to convert";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    Bitmap image = new Bitmap(dialog.FileName);
                    string path = Options.OutputPath;
                    string fileName = Path.Combine(path, "facet.mul");
                    Ultima.MultiMap.SaveFacetImage(fileName, image);

                    Cursor.Current = Cursors.Default;

                    MessageBox.Show($"Facet saved to {fileName}", "Convert", MessageBoxButtons.OK,
                        MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("No image found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (_loaded)
            {
                return;
            }

            multiMapToolStripMenuItem.Checked = true;
            pictureBox.Image = Ultima.MultiMap.GetMultiMap();
            if (pictureBox.Image != null)
            {
                DisplayScrollBars();
                SetScrollBarValues();
            }
            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            _loaded = true;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            if (pictureBox.Image != null)
            {
                e.Graphics.DrawImage(pictureBox.Image,
                    e.ClipRectangle,
                    hScrollBar.Value, vScrollBar.Value, e.ClipRectangle.Width, e.ClipRectangle.Height,
                    GraphicsUnit.Pixel);
            }
        }
    }
}
