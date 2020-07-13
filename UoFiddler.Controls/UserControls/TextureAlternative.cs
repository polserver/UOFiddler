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
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class TextureAlternative : UserControl
    {
        public TextureAlternative()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            pictureBox.MouseWheel += OnMouseWheel;
            _refMarker = this;
        }

        private static TextureAlternative _refMarker;
        private List<int> _textureList = new List<int>();
        private int _col;
        private int _row;
        private int _selected = -1;

        private bool _loaded;

        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _textureList = new List<int>();
            _selected = -1;
            OnLoad(this, EventArgs.Empty);
        }

        public static bool SearchGraphic(int graphic)
        {
            for (int i = 0; i < _refMarker._textureList.Count; ++i)
            {
                if (_refMarker._textureList[i] != graphic)
                {
                    continue;
                }

                _refMarker._selected = graphic;
                _refMarker.vScrollBar.Value = (i / _refMarker._col) + 1;
                _refMarker.GraphicLabel.Text = string.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", graphic, Textures.GetTexture(graphic));
                _refMarker.pictureBox.Invalidate();
                return true;
            }
            return false;
        }

        public int GetIndex(int x, int y)
        {
            int value = Math.Max(0, (_col * (vScrollBar.Value - 1)) + x + (y * _col));

            return _textureList.Count > value
                ? _textureList[value]
                : -1;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Texture"] = true;

            for (int i = 0; i < 0x1000; ++i)
            {
                if (Textures.TestTexture(i))
                {
                    _textureList.Add(i);
                }
            }

            vScrollBar.Maximum = (_textureList.Count / _col) + 1;
            pictureBox.Invalidate();

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.TextureChangeEvent += OnTextureChangeEvent;
            }
            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnTextureChangeEvent(object sender, int index)
        {
            if (!Options.DesignAlternative)
            {
                return;
            }

            if (!_loaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Textures.TestTexture(index))
            {
                bool done = false;
                for (int i = 0; i < _textureList.Count; ++i)
                {
                    if (index < _textureList[i])
                    {
                        _textureList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _textureList[i])
                    {
                        continue;
                    }

                    done = true;
                    break;
                }

                if (!done)
                {
                    _textureList.Add(index);
                }

                vScrollBar.Maximum = (_textureList.Count / _col) + 1;
            }
            else
            {
                _textureList.Remove(index);
                vScrollBar.Maximum = (_textureList.Count / _col) + 1;
            }
        }

        private void OnFilePathChangeEvent()
        {
            if (Options.DesignAlternative)
            {
                Reload();
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value >= vScrollBar.Maximum)
                {
                    return;
                }

                vScrollBar.Value++;
                pictureBox.Invalidate();
            }
            else
            {
                if (vScrollBar.Value <= 1)
                {
                    return;
                }

                vScrollBar.Value--;
                pictureBox.Invalidate();
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);

            for (int x = 0; x <= _col; ++x)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(x * 64, 0), new Point(x * 64, _row * 64));
            }

            for (int y = 0; y <= _row; ++y)
            {
                e.Graphics.DrawLine(Pens.Gray, new Point(0, y * 64), new Point(_col * 64, y * 64));
            }

            for (int y = 0; y < _row; ++y)
            {
                for (int x = 0; x < _col; ++x)
                {
                    int index = GetIndex(x, y);
                    if (index < 0)
                    {
                        continue;
                    }

                    Bitmap b = Textures.GetTexture(index, out bool patched);
                    if (b == null)
                    {
                        continue;
                    }

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

                    if (index == _selected)
                    {
                        e.Graphics.DrawRectangle(Pens.LightBlue, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    }
                    else if (patched)
                    {
                        e.Graphics.DrawRectangle(Pens.LightCoral, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    }
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (pictureBox.Width == 0 || pictureBox.Height == 0)
            {
                return;
            }

            _col = pictureBox.Width / 64;
            _row = (pictureBox.Height / 64) + 1;
            vScrollBar.Maximum = (_textureList.Count / _col) + 1;
            vScrollBar.Minimum = 1;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = _row;
            pictureBox.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            int x = e.X / (64 - 1);
            int y = e.Y / (64 - 1);
            int index = GetIndex(x, y);
            if (index < 0 || _selected == index)
            {
                return;
            }

            _selected = index;
            GraphicLabel.Text = string.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", _selected, Textures.GetTexture(_selected).Width);
            pictureBox.Invalidate();
        }

        private TextureSearch _showForm;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new TextureSearch
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void OnClickFindNext(object sender, EventArgs e)
        {
            int id, i;
            if (_selected > -1)
            {
                id = _selected + 1;
                i = _textureList.IndexOf(_selected) + 1;
            }
            else
            {
                id = 1;
                i = 0;
            }

            for (; i < _textureList.Count; ++i, ++id)
            {
                if (id >= _textureList[i])
                {
                    continue;
                }

                _selected = _textureList[i];
                vScrollBar.Value = (i / _refMarker._col) + 1;
                GraphicLabel.Text = string.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}", _selected, Textures.GetTexture(_selected));
                pictureBox.Invalidate();
                break;
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (_selected < 0)
            {
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure to remove 0x{_selected:X}", "Save",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Textures.Remove(_selected);
            ControlEvents.FireTextureChangeEvent(this, _selected);
            _textureList.Remove(_selected);
            --_selected;
            pictureBox.Invalidate();
            Options.ChangedUltimaClass["Texture"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (_selected < 0)
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

                Textures.Replace(_selected, bmp);
                ControlEvents.FireTextureChangeEvent(this, _selected);
                pictureBox.Invalidate();
                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0xFFF))
            {
                InsertText.ForeColor = Textures.TestTexture(index) ? Color.Red : Color.Black;
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

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0xFFF))
            {
                return;
            }

            if (Textures.TestTexture(index))
            {
                return;
            }

            contextMenuStrip1.Close();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Bitmap bmp = new Bitmap(dialog.FileName);
                if ((bmp.Width == 64 && bmp.Height == 64) || (bmp.Width == 128 && bmp.Height == 128))
                {
                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bmp = Utils.ConvertBmp(bmp);
                    }

                    Textures.Replace(index, bmp);
                    ControlEvents.FireTextureChangeEvent(this, index);
                    bool done = false;
                    for (int i = 0; i < _textureList.Count; ++i)
                    {
                        if (index >= _textureList[i])
                        {
                            continue;
                        }

                        _textureList.Insert(i, index);
                        vScrollBar.Value = (i / _refMarker._col) + 1;
                        done = true;
                        break;
                    }
                    if (!done)
                    {
                        _textureList.Add(index);
                        vScrollBar.Value = (_textureList.Count / _refMarker._col) + 1;
                    }
                    _selected = index;
                    GraphicLabel.Text = string.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", _selected, Textures.GetTexture(_selected));
                    pictureBox.Invalidate();
                    Options.ChangedUltimaClass["Texture"] = true;
                }
                else
                {
                    MessageBox.Show("Height or Width Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Textures.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show($"Saved to {Options.OutputPath}", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Texture"] = false;
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            ExportTextureImage(_selected, ImageFormat.Bmp);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            ExportTextureImage(_selected, ImageFormat.Tiff);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            ExportTextureImage(_selected, ImageFormat.Jpeg);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            ExportTextureImage(_selected, ImageFormat.Png);
        }

        private static void ExportTextureImage(int index, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Texture {index}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Texture saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }
    }
}
