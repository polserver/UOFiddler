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
using System.ComponentModel;
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

            _refMarker = this;
        }

        private static TexturesControl _refMarker;
        private List<int> _textureList = new List<int>();
        private bool _showFreeSlots;
        private bool _loaded;
        private int _selectedTextureId = -1;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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

        public static bool Select(int textureId)
        {
            if (_refMarker == null)
            {
                return false;
            }

            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            if (!_refMarker._textureList.Contains(textureId))
            {
                return false;
            }

            TabPageNavigator.ActivateOwningTabPage(_refMarker);

            if (_refMarker.IsHandleCreated)
            {
                _refMarker.BeginInvoke(new Action(() =>
                {
                    // Reset focus index to ensure the view scrolls to the selected texture
                    _refMarker.TextureTileView.FocusIndex = -1;
                    _refMarker.SelectedTextureId = textureId;
                }));
            }
            else
            {
                _refMarker.TextureTileView.FocusIndex = -1;
                _refMarker.SelectedTextureId = textureId;
            }

            return true;
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

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
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
            }
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
            var ids = GetSelectedTextureIds().Where(Textures.TestTexture).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            string prompt = ids.Count == 1
                ? $"Are you sure to remove 0x{ids[0]:X}"
                : $"Are you sure to remove {ids.Count} textures?";

            DialogResult result = MessageBox.Show(prompt, "Save",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach (int id in ids)
            {
                Textures.Remove(id);
                ControlEvents.FireTextureChangeEvent(this, id);

                if (!_showFreeSlots)
                {
                    _textureList.Remove(id);
                }
            }

            TextureTileView.SelectedIndices.Clear();

            if (!_showFreeSlots)
            {
                TextureTileView.VirtualListSize = _textureList.Count;
                int moveToId = ids[0] - 1;
                SelectedTextureId = moveToId <= 0 ? 0 : moveToId; // TODO: get last index visible instead just curr -1
            }

            TextureTileView.Invalidate();

            Options.ChangedUltimaClass["Texture"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (TextureTileView.SelectedIndices.Count > 1)
            {
                ReplaceMultipleSelected();
                return;
            }

            if (_selectedTextureId < 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
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

        private void ReplaceMultipleSelected()
        {
            var ids = GetSelectedTextureIds();
            if (ids.Count == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose {ids.Count} image files to replace selected textures";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var files = dialog.FileNames.OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase).ToArray();

                if (files.Length != ids.Count)
                {
                    MessageBox.Show(
                        $"Selected {ids.Count} textures but chose {files.Length} images.\n\nNo changes made.",
                        "Selection Mismatch",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Load and validate all images first; abort the whole batch on any failure so no partial writes happen.
                var bitmaps = new List<Bitmap>(ids.Count);
                try
                {
                    for (int i = 0; i < ids.Count; ++i)
                    {
                        using (var bmpTemp = new Bitmap(files[i]))
                        {
                            bool validSize = (bmpTemp.Width == 64 && bmpTemp.Height == 64)
                                || (bmpTemp.Width == 128 && bmpTemp.Height == 128);

                            if (!validSize)
                            {
                                MessageBox.Show(
                                    $"Invalid texture dimensions!\n\n" +
                                    $"File: {Path.GetFileName(files[i])} ({bmpTemp.Width}x{bmpTemp.Height})\n" +
                                    $"Textures must be 64x64 or 128x128 pixels.\n\n" +
                                    $"No changes made.",
                                    "Invalid Size",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                return;
                            }

                            Bitmap bitmap = new Bitmap(bmpTemp);

                            if (files[i].Contains(".bmp"))
                            {
                                bitmap = Utils.ConvertBmp(bitmap);
                            }

                            bitmaps.Add(bitmap);
                        }
                    }
                }
                catch
                {
                    foreach (var bmp in bitmaps)
                    {
                        bmp.Dispose();
                    }
                    throw;
                }

                for (int i = 0; i < ids.Count; ++i)
                {
                    Textures.Replace(ids[i], bitmaps[i]);
                    ControlEvents.FireTextureChangeEvent(this, ids[i]);
                }

                TextureTileView.Invalidate();

                Options.ChangedUltimaClass["Texture"] = true;
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            Color invalidColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                InsertText.ForeColor = Textures.TestTexture(index) ? invalidColor : SystemColors.ControlText;
            }
            else
            {
                InsertText.ForeColor = invalidColor;
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

            contextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
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
            using (new WaitCursorScope(this))
            {
                Textures.Save(Options.OutputPath);
            }

            Options.ChangedUltimaClass["Texture"] = false;

            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Files saved successfully.");
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Bmp);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Tiff);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Jpeg);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Png);
        }

        private void ExportSelected(ImageFormat imageFormat)
        {
            var ids = GetSelectedTextureIds().Where(Textures.TestTexture).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            if (ids.Count == 1)
            {
                ExportTextureImage(ids[0], imageFormat);
                return;
            }

            ExportMultipleTextureImages(ids, imageFormat);
        }

        private void ExportMultipleTextureImages(List<int> ids, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            foreach (int index in ids)
            {
                var texture = Textures.GetTexture(index);
                if (texture is null)
                {
                    continue;
                }

                string fileName = Path.Combine(Options.OutputPath, $"Texture {Utils.FormatExportId(index)}.{fileExtension}");
                using (Bitmap bit = new Bitmap(texture))
                {
                    bit.Save(fileName, imageFormat);
                }
            }

            FileSavedDialog.Show(FindForm(), Options.OutputPath, $"{ids.Count} textures saved successfully.");
        }

        private static void ExportTextureImage(int index, ImageFormat imageFormat)
        {
            if (!Textures.TestTexture(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Texture {Utils.FormatExportId(index)}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Textures.GetTexture(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Texture saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Resolves the current tile selection to a sorted list of texture IDs.
        /// </summary>
        private List<int> GetSelectedTextureIds()
        {
            var ids = new List<int>();
            foreach (int idx in TextureTileView.SelectedIndices)
            {
                if (idx >= 0 && idx < _textureList.Count)
                {
                    ids.Add(_textureList[idx]);
                }
            }
            ids.Sort();
            return ids;
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
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + TextureTileView.TilePadding.Left, e.Bounds.Y + TextureTileView.TilePadding.Top);

            const int defaultTileWidth = 128;
            Size defaultTileSize = new Size(defaultTileWidth, defaultTileWidth);
            Rectangle tileRectangle = new Rectangle(itemPoint, defaultTileSize);

            using var previousClip = e.Graphics.Clip;

            using var clipRegion = new Region(tileRectangle);
            e.Graphics.Clip = clipRegion;

            Bitmap bitmap = Textures.GetTexture(_textureList[e.Index], out bool patched);

            if (bitmap == null)
            {
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

                using (new WaitCursorScope(this))
                {
                    foreach (var index in _textureList)
                    {
                        if (!Textures.TestTexture(index))
                        {
                            continue;
                        }

                        string fileName = Path.Combine(dialog.SelectedPath, $"Texture {Utils.FormatExportId(index)}.{fileExtension}");
                        var texture = Textures.GetTexture(index);
                        if (texture is null)
                        {
                            continue;
                        }

                        using (Bitmap bit = new Bitmap(texture))
                        {
                            bit.Save(fileName, imageFormat);
                        }
                    }
                }

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

            contextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose images to replace starting at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";

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

        private void OnClickSelectInLandTiles(object sender, EventArgs e)
        {
            if (_selectedTextureId < 0)
            {
                return;
            }

            if (!LandTilesControl.SearchGraphic(_selectedTextureId))
            {
                MessageBox.Show("You need to load the Land Tiles tab first.", "Information");
            }
        }

        private void contextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int selectedCount = TextureTileView.SelectedIndices.Count;
            removeToolStripMenuItem.Text = selectedCount > 1 ? $"Remove {selectedCount}" : "Remove";
            exportImageToolStripMenuItem.Text = selectedCount > 1 ? $"Export {selectedCount} Images..." : "Export Image..";
            replaceToolStripMenuItem.Text = selectedCount > 1 ? $"Replace {selectedCount}" : "Replace";

            bool hasLandTile = _selectedTextureId >= 0
                && _selectedTextureId < 0x4000
                && Art.IsValidLand(_selectedTextureId);
            selectInLandTilesTabToolStripMenuItem.Enabled = hasLandTile;
        }

        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue))
            {
                return;
            }

            var maximumIndex = Textures.GetIdxLength();

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue > maximumIndex)
            {
                indexValue = maximumIndex;
            }

            // we have to invalidate focus so it will scroll to item
            TextureTileView.FocusIndex = -1;
            SelectedTextureId = indexValue;
        }
    }
}
