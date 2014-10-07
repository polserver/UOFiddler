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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace FiddlerControls
{
    public partial class TextureAlternative : UserControl
    {
        public TextureAlternative()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            pictureBox.MouseWheel += new MouseEventHandler(OnMouseWheel);
            refMarker = this;
        }
        private static TextureAlternative refMarker = null;
        private List<int> TextureList = new List<int>();
        private int col;
        private int row;
        private int selected = -1;

        private bool Loaded = false;
        private void Reload()
        {
            if (!Loaded)
                return;
            TextureList = new List<int>();
            selected = -1;
            OnLoad(this, EventArgs.Empty);
        }

        public static bool SearchGraphic(int graphic)
        {
            for (int i = 0; i < refMarker.TextureList.Count; ++i)
            {
                if (refMarker.TextureList[i] == graphic)
                {
                    refMarker.selected = graphic;
                    refMarker.vScrollBar.Value = i / refMarker.col + 1;
                    refMarker.GraphicLabel.Text = String.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", graphic, Textures.GetTexture(graphic));
                    refMarker.pictureBox.Invalidate();
                    return true;
                }
            }
            return false;
        }

        public int GetIndex(int x, int y)
        {
            int value = Math.Max(0, ((col * (vScrollBar.Value - 1)) + (x + (y * col))));
            if (TextureList.Count > value)
                return TextureList[value];
            else
                return -1;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Texture"] = true;

            for (int i = 0; i < 0x1000; ++i)
            {
                if (Textures.TestTexture(i))
                    TextureList.Add(i);
            }
            vScrollBar.Maximum = TextureList.Count / col + 1;
            pictureBox.Invalidate();
            if (!Loaded)
            {
                FiddlerControls.Events.FilePathChangeEvent += new FiddlerControls.Events.FilePathChangeHandler(OnFilePathChangeEvent);
                FiddlerControls.Events.TextureChangeEvent += new FiddlerControls.Events.TextureChangeHandler(OnTextureChangeEvent);
            }
            Loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnTextureChangeEvent(object sender, int index)
        {
            if (!FiddlerControls.Options.DesignAlternative)
                return;
            if (!Loaded)
                return;
            if (sender.Equals(this))
                return;

            if (Ultima.Textures.TestTexture(index))
            {
                bool done = false;
                for (int i = 0; i < TextureList.Count; ++i)
                {
                    if (index < TextureList[i])
                    {
                        TextureList.Insert(i, index);
                        done = true;
                        break;
                    }
                    if (index == TextureList[i])
                    {
                        done = true;
                        break;
                    }
                }
                if (!done)
                    TextureList.Add(index);
                vScrollBar.Maximum = TextureList.Count / col + 1;
            }
            else
            {
                TextureList.Remove(index);
                vScrollBar.Maximum = TextureList.Count / col + 1;
            }
        }

        private void OnFilePathChangeEvent()
        {
            if (FiddlerControls.Options.DesignAlternative)
                Reload();
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value < vScrollBar.Maximum)
                {
                    vScrollBar.Value++;
                    pictureBox.Invalidate();
                }
            }
            else
            {
                if (vScrollBar.Value > 1)
                {
                    vScrollBar.Value--;
                    pictureBox.Invalidate();
                }
            }
        }

        private void onPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int x = 0; x <= col; ++x)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(x * 64, 0),
                    new Point(x * 64, row * 64));
            }

            for (int y = 0; y <= row; ++y)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(0, y * 64),
                    new Point(col * 64, y * 64));
            }

            for (int y = 0; y < row; ++y)
            {
                for (int x = 0; x < col; ++x)
                {
                    int index = GetIndex(x, y);
                    if (index >= 0)
                    {
                        bool patched;
                        Bitmap b = Textures.GetTexture(index, out patched);

                        if (b != null)
                        {
                            Point loc = new Point((x * 64) + 1, (y * 64) + 1);
                            Size size = new Size(64 - 1, 64 - 1);
                            Rectangle rect = new Rectangle(loc, size);

                            e.Graphics.Clip = new Region(rect);

                            int width = b.Width;
                            int height = b.Height;
                            if (width > size.Width)
                            {
                                width = size.Width;
                                height = size.Height * b.Height / b.Width;
                            }
                            if (height > size.Height)
                            {
                                height = size.Height;
                                width = size.Width * b.Width / b.Height;
                            }
                            e.Graphics.DrawImage(b, new Rectangle(loc, new Size(width, height)));
                            if (index == selected)
                                e.Graphics.DrawRectangle(Pens.LightBlue, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                            else if (patched)
                                e.Graphics.DrawRectangle(Pens.LightCoral, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                        }
                    }
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if ((pictureBox.Width == 0) || (pictureBox.Height == 0))
                return;
            col = pictureBox.Width / 64;
            row = pictureBox.Height / 64 + 1;
            vScrollBar.Maximum = TextureList.Count / col + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = row;
            pictureBox.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            int x = e.X / (64 - 1);
            int y = e.Y / (64 - 1);
            int index = GetIndex(x, y);
            if (index >= 0)
            {
                if (selected != index)
                {
                    selected = index;
                    GraphicLabel.Text = String.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", selected, Textures.GetTexture(selected).Width);
                    pictureBox.Invalidate();
                }
            }
        }

        private TextureSearch showform = null;
        private void OnClickSearch(object sender, EventArgs e)
        {
            if ((showform == null) || (showform.IsDisposed))
            {
                showform = new TextureSearch();
                showform.TopMost = true;
                showform.Show();
            }
        }

        private void onClickFindNext(object sender, EventArgs e)
        {
            int id, i;
            if (selected > -1)
            {
                id = selected + 1;
                i = TextureList.IndexOf(selected) + 1;
            }
            else
            {
                id = 1;
                i = 0;
            }
            for (; i < TextureList.Count; ++i, ++id)
            {
                if (id < TextureList[i])
                {
                    selected = TextureList[i];
                    vScrollBar.Value = i / refMarker.col + 1;
                    GraphicLabel.Text = String.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}", selected, Textures.GetTexture(selected));
                    pictureBox.Invalidate();
                    break;
                }
            }
        }

        private void onClickRemove(object sender, EventArgs e)
        {
            if (selected < 0)
                return;
            DialogResult result =
                        MessageBox.Show(String.Format("Are you sure to remove 0x{0:X}", selected),
                        "Save",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                Textures.Remove(selected);
                FiddlerControls.Events.FireTextureChangeEvent(this, selected);
                TextureList.Remove(selected);
                --selected;
                pictureBox.Invalidate();
                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        private void onClickReplace(object sender, EventArgs e)
        {
            if (selected >= 0)
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
                        Textures.Replace(selected, bmp);
                        FiddlerControls.Events.FireTextureChangeEvent(this, selected);
                        pictureBox.Invalidate();
                        Options.ChangedUltimaClass["Texture"] = true;
                    }
                }
            }
        }

        private void onTextChangedInsert(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, 0xFFF))
            {
                if (Textures.TestTexture(index))
                    InsertText.ForeColor = Color.Red;
                else
                    InsertText.ForeColor = Color.Black;
            }
            else
                InsertText.ForeColor = Color.Red;
        }

        private void onKeyDownInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(InsertText.Text, out index, 0, 0xFFF))
                {
                    if (Textures.TestTexture(index))
                        return;
                    contextMenuStrip1.Close();
                    using (OpenFileDialog dialog = new OpenFileDialog())
                    {
                        dialog.Multiselect = false;
                        dialog.Title = String.Format("Choose image file to insert at 0x{0:X}", index);
                        dialog.CheckFileExists = true;
                        dialog.Filter = "image files (*.tiff;*.bmp)|*.tiff;*.bmp";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            Bitmap bmp = new Bitmap(dialog.FileName);
                            if (((bmp.Width == 64) && (bmp.Height == 64)) || ((bmp.Width == 128) && (bmp.Height == 128)))
                            {
                                if (dialog.FileName.Contains(".bmp"))
                                    bmp = Utils.ConvertBmp(bmp);
                                Textures.Replace(index, bmp);
                                FiddlerControls.Events.FireTextureChangeEvent(this, index);
                                bool done = false;
                                for (int i = 0; i < TextureList.Count; ++i)
                                {
                                    if (index < TextureList[i])
                                    {
                                        TextureList.Insert(i, index);
                                        vScrollBar.Value = i / refMarker.col + 1;
                                        done = true;
                                        break;
                                    }
                                }
                                if (!done)
                                {
                                    TextureList.Add(index);
                                    vScrollBar.Value = TextureList.Count / refMarker.col + 1;
                                }
                                selected = index;
                                GraphicLabel.Text = String.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", selected, Textures.GetTexture(selected));
                                pictureBox.Invalidate();
                                Options.ChangedUltimaClass["Texture"] = true;
                            }
                            else
                                MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        }
                    }
                }
            }
        }

        private void onClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Textures.Save(FiddlerControls.Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(
                String.Format("Saved to {0}", FiddlerControls.Options.OutputPath),
                "Save",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Texture"] = false;
        }

        private void onClickExportTiff(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Texture {0}.tiff", selected));
            Bitmap bit = new Bitmap(Textures.GetTexture(selected));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Tiff);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void onClickExportBmp(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Texture {0}.bmp", selected));
            Bitmap bit = new Bitmap(Textures.GetTexture(selected));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Bmp);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void onClickExportJpg(object sender, EventArgs e)
        {
            string path = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(path, String.Format("Texture {0}.jpg", selected));
            Bitmap bit = new Bitmap(Textures.GetTexture(selected));
            if (bit != null)
                bit.Save(FileName, ImageFormat.Jpeg);
            bit.Dispose();
            MessageBox.Show(
                String.Format("Texture saved to {0}", FileName),
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

    }
}
