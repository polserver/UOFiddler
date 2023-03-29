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
    public partial class TexturesControl : UserControl
    {
        public TexturesControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
        }

        private List<int> _textureList = new List<int>();
        private bool _showFreeSlots;
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

            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;

            _selectedTextureId = -1;

            OnLoad(this, EventArgs.Empty);
        }

        private bool SearchGraphic(int graphic)
        {
            if (_textureList.All(id => id != graphic))
            {
                return false;
            }

            // we have to invalidate focus so it will scroll to item
            TextureTileView.FocusIndex = -1;
            SelectedTextureId = graphic;

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

            UpdateTileView();

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
                if (_showFreeSlots)
                {
                    return;
                }

                _textureList.Remove(index);
            }

            TextureTileView.VirtualListSize = _textureList.Count;
            TextureTileView.Invalidate();
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private TextureSearchForm _showForm;

        private void OnClickSearch(object sender, EventArgs e)
        {
            if (_showForm?.IsDisposed == false)
            {
                return;
            }

            _showForm = new TextureSearchForm(SearchGraphic)
            {
                TopMost = true
            };
            _showForm.Show();
        }

        private void OnClickFindNext(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = _selectedTextureId > -1 ? _textureList.IndexOf(_selectedTextureId) + 1 : 0;
                for (; i < _textureList.Count; ++i)
                {
                    if (Textures.TestTexture(_textureList[i]))
                    {
                        continue;
                    }

                    SelectedTextureId = _textureList[i];
                    TextureTileView.Invalidate();
                    break;
                }
            }
            else
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
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            if (_selectedTextureId < 0)
            {
                return;
            }

            if (!Textures.TestTexture(_selectedTextureId))
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

            if (!_showFreeSlots)
            {
                _textureList.Remove(_selectedTextureId);
                TextureTileView.VirtualListSize = _textureList.Count;
                var moveToIndex = --_selectedTextureId;
                SelectedTextureId = moveToIndex <= 0 ? 0 : _selectedTextureId; // TODO: get last index visible instead just curr -1
            }

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

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Textures.Replace(_selectedTextureId, bitmap);

                    ControlEvents.FireTextureChangeEvent(this, _selectedTextureId);

                    TextureTileView.Invalidate();

                    Options.ChangedUltimaClass["Texture"] = true;
                }
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
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

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
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

                using (Bitmap bmpTemp = new Bitmap(dialog.FileName))
                {
                    if ((bmpTemp.Width == 64 && bmpTemp.Height == 64) || (bmpTemp.Width == 128 && bmpTemp.Height == 128))
                    {
                        Bitmap bitmap = new Bitmap(bmpTemp);

                        if (dialog.FileName.Contains(".bmp"))
                        {
                            bitmap = Utils.ConvertBmp(bitmap);
                        }

                        Textures.Replace(index, bitmap);

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
            if (!Textures.TestTexture(index))
            {
                return;
            }

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
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + TextureTileView.TilePadding.Left, e.Bounds.Y + TextureTileView.TilePadding.Top);

            const int defaultTileWidth = 128;
            Size defaultTileSize = new Size(defaultTileWidth, defaultTileWidth);
            Rectangle tileRectangle = new Rectangle(itemPoint, defaultTileSize);

            var previousClip = e.Graphics.Clip;

            e.Graphics.Clip = new Region(tileRectangle);

            Bitmap bitmap = Textures.GetTexture(_textureList[e.Index], out bool patched);

            if (bitmap == null)
            {
                e.Graphics.Clip = new Region(tileRectangle);

                tileRectangle.X += 5;
                tileRectangle.Y += 5;

                tileRectangle.Width -= 10;
                tileRectangle.Height -= 10;

                e.Graphics.FillRectangle(Brushes.Red, tileRectangle);
                e.Graphics.Clip = previousClip;
            }
            else
            {
                if (patched)
                {
                    // different background for verdata patched tiles
                    e.Graphics.FillRectangle(Brushes.LightCoral, tileRectangle);
                }

                // center 64x64 instead of drawing int top left corner
                if (bitmap.Width < defaultTileWidth)
                {
                    itemPoint.Offset(bitmap.Width / 2, bitmap.Height / 2);
                }

                Rectangle textureRectangle = new Rectangle(itemPoint, new Size(bitmap.Width, bitmap.Height));
                e.Graphics.DrawImage(bitmap, textureRectangle);

                e.Graphics.Clip = previousClip;
            }
        }

        private void ExportAllAsBmp_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Bmp);
        }

        private void ExportAllAsTiff_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Tiff);
        }

        private void ExportAllAsJpeg_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Jpeg);
        }

        private void ExportAllAsPng_Click(object sender, EventArgs e)
        {
            ExportAllTextures(ImageFormat.Png);
        }

        private void ExportAllTextures(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (var index in _textureList)
                {
                    if (!Textures.TestTexture(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Texture 0x{index:X4}.{fileExtension}");
                    using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
                    {
                        bit.Save(fileName, imageFormat);
                    }
                }

                Cursor.Current = Cursors.Default;

                MessageBox.Show($"All textures saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void ReplaceStartingFrom_OnInsert(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            const int graphicIdMin = 0;
            const int graphicIdMax = 0x3FFF;

            if (!Utils.ConvertStringToInt(ReplaceStartingFromTb.Text, out int index, graphicIdMin, graphicIdMax))
            {
                return;
            }

            contextMenuStrip1.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose images to replace starting at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp)|*.tif;*.tiff;*.bmp";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                for (int i = 0; i < dialog.FileNames.Length; i++)
                {
                    var currentIdx = index + i;

                    if (IsIndexValid(currentIdx))
                    {
                        AddSingleTexture(dialog.FileNames[i], currentIdx);
                    }
                }

                TextureTileView.VirtualListSize = _textureList.Count;
                TextureTileView.Invalidate();
                SelectedTextureId = index;

                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        /// <summary>
        /// Check if it's valid index for texture. Textures has fixed size 0x4000.
        /// </summary>
        /// <param name="index">Starting Index</param>
        private static bool IsIndexValid(int index)
        {
            return index < 0x4000;
        }

        /// <summary>
        /// Adds a single texture.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the texture will be added.</param>
        private void AddSingleTexture(string fileName, int index)
        {
            using (Bitmap bmpTemp = new Bitmap(fileName))
            {
                if ((bmpTemp.Width == 64 && bmpTemp.Height == 64) || (bmpTemp.Width == 128 && bmpTemp.Height == 128))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (fileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Textures.Replace(index, bitmap);

                    ControlEvents.FireTextureChangeEvent(this, index);

                    bool done = false;

                    for (int i = 0; i < _textureList.Count; ++i)
                    {
                        if (index > _textureList[i])
                        {
                            continue;
                        }

                        _textureList[i] = index;

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
                    MessageBox.Show("Invalid Height or Width", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        public void UpdateTileView()
        {
            TextureTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            var sameFocusColor = TextureTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = TextureTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameFocusColor && sameSelectionColor)
            {
                return;
            }

            TextureTileView.TileFocusColor = Options.TileFocusColor;
            TextureTileView.TileHighlightColor = Options.TileSelectionColor;
            TextureTileView.Invalidate();
        }

        private void ShowFreeSlotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;

            if (_showFreeSlots)
            {
                for (int j = 0; j < Textures.GetIdxLength(); ++j)
                {
                    if (_textureList.Count > j)
                    {
                        if (_textureList[j] != j)
                        {
                            _textureList.Insert(j, j);
                        }
                    }
                    else
                    {
                        _textureList.Insert(j, j);
                    }
                }

                var prevSelected = SelectedTextureId;

                TextureTileView.VirtualListSize = _textureList.Count;

                if (prevSelected >= 0)
                {
                    SelectedTextureId = prevSelected;
                }

                TextureTileView.Invalidate();
            }
            else
            {
                Reload();
            }
        }
    }
}