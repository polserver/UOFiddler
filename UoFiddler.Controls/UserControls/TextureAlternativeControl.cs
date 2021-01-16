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
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    // TODO: add "Show free slots" support
    public partial class TextureAlternativeControl : UserControl
    {
        public TextureAlternativeControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _refMarker = this;
        }

        private static TextureAlternativeControl _refMarker;
        private List<int> _textureList = new List<int>();
        private bool _loaded;

        private int _selectedTextureId = -1;
        public int SelectedTextureId
        {
            get => _selectedTextureId;
            set
            {
                _selectedTextureId = value < 0 ? 0 : value;
                UpdateLabels(_selectedTextureId);
                TextureTileView.FocusIndex = _textureList.IndexOf(_selectedTextureId);
            }
        }

        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _textureList = new List<int>();
            _selectedTextureId = -1;
            OnLoad(this, EventArgs.Empty);
        }

        public static bool SearchGraphic(int graphic)
        {
            if (_refMarker._textureList.All(id => id != graphic))
            {
                return false;
            }

            _refMarker.SelectedTextureId = graphic;

            return true;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Texture"] = true;

            for (int i = 0; i < Textures.GetIdxLength(); ++i)
            {
                if (Textures.TestTexture(i))
                {
                    _textureList.Add(i);
                }
            }

            TextureTileView.VirtualListSize = _textureList.Count;

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
            }
            else
            {
                _textureList.Remove(index);
            }

            TextureTileView.VirtualListSize = _textureList.Count;
            TextureTileView.Invalidate();
        }

        private void OnFilePathChangeEvent()
        {
            if (Options.DesignAlternative)
            {
                Reload();
            }
        }

        private TextureSearchForm _showForm;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new TextureSearchForm
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void OnClickFindNext(object sender, EventArgs e)
        {
            int id, i;
            if (_selectedTextureId > -1)
            {
                id = _selectedTextureId + 1;
                i = _textureList.IndexOf(_selectedTextureId) + 1;
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

                SelectedTextureId = _textureList[i];
                TextureTileView.Invalidate();

                break;
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (_selectedTextureId < 0)
            {
                return;
            }

            DialogResult result = MessageBox.Show($"Are you sure to remove 0x{_selectedTextureId:X}", "Save",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Textures.Remove(_selectedTextureId);
            ControlEvents.FireTextureChangeEvent(this, _selectedTextureId);
            _textureList.Remove(_selectedTextureId);
            SelectedTextureId = --_selectedTextureId;
            TextureTileView.VirtualListSize = _textureList.Count;
            TextureTileView.Invalidate();
            Options.ChangedUltimaClass["Texture"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (_selectedTextureId < 0)
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

                Textures.Replace(_selectedTextureId, bmp);
                ControlEvents.FireTextureChangeEvent(this, _selectedTextureId);
                TextureTileView.Invalidate();
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

                        done = true;
                        break;
                    }

                    if (!done)
                    {
                        _textureList.Add(index);
                    }

                    TextureTileView.VirtualListSize = _textureList.Count;
                    TextureTileView.Invalidate();
                    SelectedTextureId = index;

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
            ExportTextureImage(_selectedTextureId, ImageFormat.Bmp);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            ExportTextureImage(_selectedTextureId, ImageFormat.Tiff);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            ExportTextureImage(_selectedTextureId, ImageFormat.Jpeg);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            ExportTextureImage(_selectedTextureId, ImageFormat.Png);
        }

        private static void ExportTextureImage(int index, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Texture 0x{index:X4}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Texture saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void TextureTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            if (_textureList.Count == 0)
            {
                return;
            }

            SelectedTextureId = e.ItemIndex < 0 || e.ItemIndex > _textureList.Count
                ? _textureList[0]
                : _textureList[e.ItemIndex];
        }

        private void UpdateLabels(int graphic)
        {
            var width = Textures.TestTexture(graphic) ? Textures.GetTexture(graphic).Width : 0;

            GraphicLabel.Text = string.Format("Graphic: 0x{0:X4} ({0}) [{1}x{1}]", graphic, width);
        }

        private void TextureTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            Point itemPoint = new Point(e.Bounds.X + TextureTileView.TilePadding.Left, e.Bounds.Y + TextureTileView.TilePadding.Top);

            const int defaultTileWidth = 128;
            Size defaultTileSize = new Size(defaultTileWidth, defaultTileWidth);
            Rectangle tileRectangle = new Rectangle(itemPoint, defaultTileSize);

            Bitmap bitmap = Textures.GetTexture(_textureList[e.Index], out bool patched);
            if (patched)
            {
                // different background for verdata patched tiles
                e.Graphics.FillRectangle(Brushes.LightCoral, tileRectangle);
            }

            if (bitmap == null)
            {
                // TODO: partial empty slots support - drawing the tile
                itemPoint.Offset(2, 2);
                e.Graphics.FillRectangle(Brushes.Red, new Rectangle(itemPoint, defaultTileSize - new Size(4, 4)));
            }
            else
            {
                // center 64x64 instead of drawing int top left corner
                if (bitmap.Width < defaultTileWidth)
                {
                    itemPoint.Offset(bitmap.Width / 2, bitmap.Height / 2);
                }

                Rectangle textureRectangle = new Rectangle(itemPoint, new Size(bitmap.Width, bitmap.Height));
                e.Graphics.DrawImage(bitmap, textureRectangle);
            }
        }
    }
}
