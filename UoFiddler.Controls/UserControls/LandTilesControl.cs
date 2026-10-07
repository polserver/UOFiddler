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
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Ultima;
using Ultima.Uop;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class LandTilesControl : UserControl
    {
        public LandTilesControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            _refMarker = this;
        }

        public bool IsLoaded { get; private set; }

        private static readonly Regex _hexIndexRegex = new(@"0[xX][0-9a-fA-F]+", RegexOptions.Compiled);

        private const int _landTileMax = 0x4000;

        /// <summary>
        /// Land art is stored as a fixed size raw block, so every land bitmap is this wide and tall.
        /// </summary>
        private const int LandTileSize = 44;

        private static LandTilesControl _refMarker;
        private int _selectedGraphicId = -1;
        private readonly List<int> _tileList = new List<int>();
        private bool _showFreeSlots;
        private Func<string, string?>? _localizationGetter;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedGraphicId
        {
            get => _selectedGraphicId;
            set
            {
                _selectedGraphicId = value < 0 ? 0 : value;
                UpdateToolStripLabels(_selectedGraphicId);
                LandTilesTileView.FocusIndex = _tileList.IndexOf(_selectedGraphicId);
            }
        }

        /// <summary>
        /// Searches Objtype and Select
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public static bool SearchGraphic(int graphic)
        {
            if (_refMarker == null)
            {
                return false;
            }

            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            if (_refMarker._tileList.TrueForAll(t => t != graphic))
            {
                return false;
            }

            TabPageNavigator.ActivateOwningTabPage(_refMarker);

            if (_refMarker.IsHandleCreated)
            {
                _refMarker.BeginInvoke(new Action(() =>
                {
                    // we have to invalidate focus so it will scroll to item
                    _refMarker.LandTilesTileView.FocusIndex = -1;
                    _refMarker.SelectedGraphicId = graphic;
                }));
            }
            else
            {
                _refMarker.LandTilesTileView.FocusIndex = -1;
                _refMarker.SelectedGraphicId = graphic;
            }

            return true;
        }

        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">private bool Loaded = false;</param>
        /// <returns></returns>
        public static bool SearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (_refMarker._selectedGraphicId >= 0)
                {
                    index = _refMarker._tileList.IndexOf(_refMarker._selectedGraphicId) + 1;
                }

                if (index >= _refMarker._tileList.Count)
                {
                    index = 0;
                }
            }

            var searchMethod = SearchHelper.GetSearchMethod();

            for (int i = index; i < _refMarker._tileList.Count; ++i)
            {
                var searchResult = searchMethod(name, TileData.LandTable[_refMarker._tileList[i]].Name);
                if (searchResult.HasErrors)
                {
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue;
                }

                // we have to invalidate focus so it will scroll to item
                _refMarker.LandTilesTileView.FocusIndex = -1;
                _refMarker.SelectedGraphicId = _refMarker._tileList[i];

                return true;
            }

            return false;
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (!IsLoaded)
            {
                return;
            }

            _selectedGraphicId = -1;
            _tileList.Clear();

            OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
        }

        private void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                Options.LoadedUltimaClass["TileData"] = true;
                Options.LoadedUltimaClass["Art"] = true;

                _showFreeSlots = false;
                showFreeSlotsToolStripMenuItem.Checked = false;

                for (int i = 0; i < _landTileMax; ++i)
                {
                    if (Art.IsValidLand(i))
                    {
                        _tileList.Add(i);
                    }
                }

                LandTilesTileView.VirtualListSize = _tileList.Count;
                UpdateTileView();

                if (!IsLoaded)
                {
                    ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                    ControlEvents.LandTileChangeEvent += OnLandTileChangeEvent;
                    ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;
                    ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;
                }

                IsLoaded = true;
            }
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnPreviewBackgroundColorChanged()
        {
            LandTilesTileView.BackColor = Options.PreviewBackgroundColor;
            LandTilesTileView.Invalidate();
        }

        private void UpdateToolStripLabels(int graphic)
        {
            if (!IsLoaded)
            {
                return;
            }

            // Type 8: 获取汉化的基础标签文本，然后添加动态信息
            string nameLabel = _localizationGetter?.Invoke("Forms.LandTilesControl.NameLabel") ?? "Name:";
            string graphicLabel = _localizationGetter?.Invoke("Forms.LandTilesControl.GraphicLabel") ?? "ID:";
            string flagsLabel = _localizationGetter?.Invoke("Forms.LandTilesControl.FlagsLabel") ?? "Flags:";

            NameLabel.Text = $"{nameLabel} {TileData.LandTable[graphic].Name}";
            GraphicLabel.Text = $"{graphicLabel} 0x{graphic:X4} ({graphic})";
            FlagsLabel.Text = $"{flagsLabel} {TileData.LandTable[graphic].Flags}";
        }

        private void OnTileDataChangeEvent(object sender, int id)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (id < 0 || id > 0x3FFF)
            {
                return;
            }

            if (_selectedGraphicId != id)
            {
                return;
            }

            UpdateToolStripLabels(id);
        }

        private void OnLandTileChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Art.IsValidLand(index))
            {
                bool done = false;
                for (int i = 0; i < _tileList.Count; ++i)
                {
                    if (index < _tileList[i])
                    {
                        _tileList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _tileList[i])
                    {
                        continue;
                    }

                    done = true;
                    break;
                }

                if (!done)
                {
                    _tileList.Add(index);
                }
            }
            else
            {
                if (_showFreeSlots)
                {
                    return;
                }

                _tileList.Remove(index);
            }

            LandTilesTileView.VirtualListSize = _tileList.Count;
            LandTilesTileView.Invalidate();
        }

        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = _selectedGraphicId > -1 ? _tileList.IndexOf(_selectedGraphicId) + 1 : 0;
                for (; i < _tileList.Count; ++i)
                {
                    if (Art.IsValidLand(_tileList[i]))
                    {
                        continue;
                    }

                    SelectedGraphicId = _tileList[i];
                    LandTilesTileView.Invalidate();
                    break;
                }
            }
            else
            {
                int id = _selectedGraphicId;
                ++id;

                for (int i = _tileList.IndexOf(_selectedGraphicId) + 1; i < _tileList.Count; ++i, ++id)
                {
                    if (id >= _tileList[i])
                    {
                        continue;
                    }

                    SelectedGraphicId = _tileList[i];
                    LandTilesTileView.Invalidate();
                    break;
                }
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            var ids = GetSelectedGraphicIds().Where(Art.IsValidLand).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            // Type 6: MessageBox 确认对话框汉化
            string prompt = ids.Count == 1
                ? _localizationGetter?.Invoke($"Forms.LandTilesControl.Messages.RemoveSingleConfirm")?.Replace("{0}", ids[0].ToString()) ?? $"Are you sure to remove {ids[0]}"
                : _localizationGetter?.Invoke($"Forms.LandTilesControl.Messages.RemoveMultipleConfirm")?.Replace("{0}", ids.Count.ToString()) ?? $"Are you sure to remove {ids.Count} land tiles?";

            string removeTitle = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.RemoveTitle") ?? "Save";

            DialogResult result =
                        MessageBox.Show(prompt, removeTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach (int id in ids)
            {
                Art.RemoveLand(id);
                ControlEvents.FireLandTileChangeEvent(this, id);

                if (!_showFreeSlots)
                {
                    _tileList.Remove(id);
                }
            }

            LandTilesTileView.SelectedIndices.Clear();

            if (!_showFreeSlots)
            {
                LandTilesTileView.VirtualListSize = _tileList.Count;
                int moveToId = ids[0] - 1;
                SelectedGraphicId = moveToId <= 0 ? 0 : moveToId; // TODO: get last index visible instead just curr -1
            }
            LandTilesTileView.Invalidate();

            Options.ChangedUltimaClass["Art"] = true;
        }

        private void OnClickCopyImage(object sender, EventArgs e)
        {
            // The clipboard holds one image, so a multi selection copies the focused tile.
            int id = _selectedGraphicId;
            if (id < 0 || !Art.IsValidLand(id))
            {
                return;
            }

            if (!ImageClipboard.TryCopy(Art.GetLand(id), out string error))
            {
                MessageBox.Show(error, "Copy Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnClickPasteImage(object sender, EventArgs e)
        {
            var ids = GetSelectedGraphicIds();
            if (ids.Count == 0)
            {
                if (_selectedGraphicId < 0)
                {
                    return;
                }

                ids.Add(_selectedGraphicId);
            }

            using Bitmap pasted = ImageClipboard.TryPaste(out string error);
            if (pasted == null)
            {
                MessageBox.Show(error, "Paste Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Land art is a fixed 44x44 raw block - anything else would be written out as garbage.
            if (pasted.Width != LandTileSize || pasted.Height != LandTileSize)
            {
                MessageBox.Show(
                    $"Invalid land tile dimensions!\n\n" +
                    $"Clipboard image: {pasted.Width}x{pasted.Height}\n" +
                    $"Land tiles must be {LandTileSize}x{LandTileSize} pixels.\n\n" +
                    "No changes made.",
                    "Invalid Size", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ids.Count > 1)
            {
                // Type 6: 粘贴确认对话框汉化
                string pasteConfirm = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.PasteMultipleConfirm")?.Replace("{0}", ids.Count.ToString()) 
                    ?? $"Paste this image into {ids.Count} selected land tiles?";
                string pasteTitle = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.PasteImageTitle") ?? "Paste Image";
                
                DialogResult confirm = MessageBox.Show(
                    pasteConfirm,
                    pasteTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }
            }

            Bitmap converted = Utils.ToUoBitmap(pasted);

            foreach (int id in ids)
            {
                // Each slot needs its own bitmap: the SDK keeps the instance it is handed.
                Art.ReplaceLand(id, ids.Count == 1 ? converted : (Bitmap)converted.Clone());
                ControlEvents.FireLandTileChangeEvent(this, id);
            }

            if (ids.Count > 1)
            {
                converted.Dispose();
            }

            LandTilesTileView.Invalidate();

            if (_selectedGraphicId >= 0)
            {
                UpdateToolStripLabels(_selectedGraphicId);
            }

            Options.ChangedUltimaClass["Art"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (LandTilesTileView.SelectedIndices.Count > 1)
            {
                ReplaceMultipleSelected();
                return;
            }

            if (_selectedGraphicId < 0)
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

                    // Validate image size (land tiles should be 44x44 but check anyway)
                    if (!Art.ValidateStaticSize(bitmap, out int estimatedSize))
                    {
                        MessageBox.Show(
                            $"Image is too large for MUL format!\n\n" +
                            $"Image dimensions: {bitmap.Width}x{bitmap.Height}\n" +
                            $"Estimated encoded size: {estimatedSize:N0} ushorts\n" +
                            $"Maximum allowed: 65,535 ushorts\n\n" +
                            $"Note: Land tiles should typically be 44x44 pixels.",
                            "Image Too Large",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    Art.ReplaceLand(_selectedGraphicId, bitmap);

                    ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

                    LandTilesTileView.Invalidate();

                    Options.ChangedUltimaClass["Art"] = true;
                }
            }
        }

        private void ReplaceMultipleSelected()
        {
            var ids = GetSelectedGraphicIds();
            if (ids.Count == 0)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose {ids.Count} image files to replace selected land tiles";
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
                        $"Selected {ids.Count} land tiles but chose {files.Length} images.\n\nNo changes made.",
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
                            Bitmap bitmap = new Bitmap(bmpTemp);

                            if (files[i].Contains(".bmp"))
                            {
                                bitmap = Utils.ConvertBmp(bitmap);
                            }

                            if (!Art.ValidateStaticSize(bitmap, out int estimatedSize))
                            {
                                bitmap.Dispose();
                                MessageBox.Show(
                                    $"Image is too large for MUL format!\n\n" +
                                    $"File: {Path.GetFileName(files[i])}\n" +
                                    $"Estimated encoded size: {estimatedSize:N0} ushorts\n" +
                                    $"Maximum allowed: 65,535 ushorts\n\n" +
                                    $"Note: Land tiles should typically be 44x44 pixels.\n" +
                                    $"No changes made.",
                                    "Image Too Large",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                return;
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
                    Art.ReplaceLand(ids[i], bitmaps[i]);
                    ControlEvents.FireLandTileChangeEvent(this, ids[i]);
                }

                LandTilesTileView.Invalidate();
                UpdateToolStripLabels(_selectedGraphicId);

                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            Color invalidColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                InsertText.ForeColor = Art.IsValidLand(index) ? invalidColor : SystemColors.ControlText;
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

            const int graphicIdMin = 0;
            const int graphicIdMax = 0x3FFF;

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, graphicIdMin, graphicIdMax))
            {
                return;
            }

            if (Art.IsValidLand(index))
            {
                return;
            }

            LandTilesContextMenuStrip.Close();

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

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Art.ReplaceLand(index, bitmap);

                    ControlEvents.FireLandTileChangeEvent(this, index);

                    if (_showFreeSlots)
                    {
                        SelectedGraphicId = index;
                        UpdateToolStripLabels(index);
                    }
                    else
                    {
                        bool done = false;
                        for (int i = 0; i < _tileList.Count; ++i)
                        {
                            if (index >= _tileList[i])
                            {
                                continue;
                            }

                            _tileList.Insert(i, index);
                            done = true;
                            break;
                        }

                        if (!done)
                        {
                            _tileList.Add(index);
                        }

                        LandTilesTileView.VirtualListSize = _tileList.Count;
                        LandTilesTileView.Invalidate();
                        SelectedGraphicId = index;

                        Options.ChangedUltimaClass["Art"] = true;
                    }
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            // Type 6: Save 确认对话框汉化
            string saveConfirm = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.SaveConfirmation") ?? "Are you sure? Will take a while";
            string saveTitle = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.SaveTitle") ?? "Save";

            DialogResult result =
                        MessageBox.Show(saveConfirm, saveTitle,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            ClientFileSaveCommand.Run(this, FileType.ArtLegacyMul, Art.Save, "Art");
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
            var ids = GetSelectedGraphicIds().Where(Art.IsValidLand).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            if (ids.Count == 1)
            {
                ExportLandTileImage(ids[0], imageFormat);
                return;
            }

            ExportMultipleLandTileImages(ids, imageFormat);
        }

        private void ExportMultipleLandTileImages(List<int> ids, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            foreach (int index in ids)
            {
                var landTile = Art.GetLand(index);
                if (landTile is null)
                {
                    continue;
                }

                string fileName = Path.Combine(Options.OutputPath, $"Landtile {Utils.FormatExportId(index)}.{fileExtension}");
                using (Bitmap bit = new Bitmap(landTile))
                {
                    bit.Save(fileName, imageFormat);
                }
            }

            string message = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.AllLandTilesSavedSuccessfully") 
                ?? $"{ids.Count} land tiles saved successfully.";
            FileSavedDialog.Show(FindForm(), Options.OutputPath, message, null, _localizationGetter);
        }

        private void ExportLandTileImage(int index, ImageFormat imageFormat)
        {
            if (!Art.IsValidLand(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Landtile {Utils.FormatExportId(index)}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Art.GetLand(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            string message = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.LandtileSavedTo") ?? "Landtile saved to";
            string title = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.Saved") ?? "Saved";
            MessageBox.Show($"{message} {fileName}", title, MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            // Carry the whole selection over, so a set of tiles picked here can be
            // edited in one go on the TileData tab.
            List<int> ids = GetSelectedGraphicIds();
            if (ids.Count > 0)
            {
                TileDataControl.Select(ids, true);
            }
            else if (_selectedGraphicId >= 0)
            {
                TileDataControl.Select(_selectedGraphicId, true);
            }
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                RadarColorControl.Select(_selectedGraphicId, true);
            }
        }

        private void OnClickSelectTexture(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
            {
                return;
            }

            int textureId = TileData.LandTable[_selectedGraphicId].TextureId;
            if (!TexturesControl.Select(textureId))
            {
                MessageBox.Show("You need to load the Textures tab first.", "Information");
            }
        }

        private void LandTilesContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int selectedCount = LandTilesTileView.SelectedIndices.Count;
            copyImageToolStripMenuItem.Enabled = _selectedGraphicId >= 0 && Art.IsValidLand(_selectedGraphicId);
            pasteImageToolStripMenuItem.Enabled = selectedCount > 0 && ImageClipboard.ContainsImage();
            
            // ✅ Type 12 汉化：使用汉化基础文本构建动态文本
            string basePasteText = _localizationGetter?.Invoke("Forms.LandTilesControl.pasteImageToolStripMenuItem") ?? "Paste Image";
            string baseRemoveText = _localizationGetter?.Invoke("Forms.LandTilesControl.removeToolStripMenuItem") ?? "Remove";
            string baseExportText = _localizationGetter?.Invoke("Forms.LandTilesControl.exportImageToolStripMenuItem") ?? "Export Image..";
            string baseReplaceText = _localizationGetter?.Invoke("Forms.LandTilesControl.replaceToolStripMenuItem") ?? "Replace";
            string baseSelectTileDataText = _localizationGetter?.Invoke("Forms.LandTilesControl.selectInTileDataTabToolStripMenuItem") ?? "Select in TileData tab";
            
            pasteImageToolStripMenuItem.Text = selectedCount > 1 ? $"{basePasteText} {selectedCount}" : basePasteText;
            removeToolStripMenuItem.Text = selectedCount > 1 ? $"{baseRemoveText} {selectedCount}" : baseRemoveText;
            exportImageToolStripMenuItem.Text = selectedCount > 1 ? $"{baseExportText.Replace("...", "")} {selectedCount}..." : baseExportText;
            replaceToolStripMenuItem.Text = selectedCount > 1 ? $"{baseReplaceText} {selectedCount}" : baseReplaceText;
            selectInTileDataTabToolStripMenuItem.Text = selectedCount > 1
                ? $"{baseSelectTileDataText.Replace(" tab", "")} {selectedCount} tab"
                : baseSelectTileDataText;

            bool hasTexture = _selectedGraphicId >= 0
                && TileData.LandTable[_selectedGraphicId].TextureId != 0
                && Textures.TestTexture(TileData.LandTable[_selectedGraphicId].TextureId);
            selectInTexturesTabToolStripMenuItem.Enabled = hasTexture;
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Bmp);
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Tiff);
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Jpeg);
        }

        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllLandTiles(ImageFormat.Png);
        }

        private void ExportAllLandTiles(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.SelectDirectoryDescription") ?? "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (new WaitCursorScope(this))
                {
                    foreach (var index in _tileList)
                    {
                        if (!Art.IsValidLand(index))
                        {
                            continue;
                        }

                        string fileName = Path.Combine(dialog.SelectedPath, $"Landtile {Utils.FormatExportId(index)}.{fileExtension}");
                        var landTile = Art.GetLand(index);
                        if (landTile is null)
                        {
                            continue;
                        }

                        using (Bitmap bit = new Bitmap(landTile))
                        {
                            bit.Save(fileName, imageFormat);
                        }
                    }
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.AllLandTilesSavedSuccessfully") ?? "All land tiles saved successfully.", null, _localizationGetter);
            }
        }

        private void ChangeBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Options.PreviewBackgroundColor = colorDialog.Color;
            ControlEvents.FirePreviewBackgroundColorChangeEvent();
        }

        private void LandTilesTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + LandTilesTileView.TilePadding.Left, e.Bounds.Y + LandTilesTileView.TilePadding.Top);
            Size itemSize = new Size(LandTileSize, LandTileSize);
            Rectangle itemRec = new Rectangle(itemPoint, itemSize);

            using var previousClip = e.Graphics.Clip;

            using var clipRegion = new Region(itemRec);
            e.Graphics.Clip = clipRegion;

            var selected = LandTilesTileView.SelectedIndices.Contains(e.Index);
            if (!selected)
            {
                e.Graphics.Clear(Options.PreviewBackgroundColor);
            }

            Bitmap bitmap = Art.GetLand(_tileList[e.Index], out bool patched);

            if (bitmap == null)
            {
                itemRec.X += 5;
                itemRec.Y += 5;

                itemRec.Width -= 10;
                itemRec.Height -= 10;

                e.Graphics.FillRectangle(Brushes.Red, itemRec);
                e.Graphics.Clip = previousClip;
            }
            else
            {
                if (patched)
                {
                    // different background for verdata patched tiles
                    e.Graphics.FillRectangle(Brushes.LightCoral, itemRec);
                }

                e.Graphics.DrawImage(bitmap, itemRec);

                if (Art.IsLandModified(_tileList[e.Index]))
                {
                    ModifiedMarker.Draw(e.Graphics, itemRec);
                }

                e.Graphics.Clip = previousClip;
            }
        }

        /// <summary>
        /// Resolves the current tile selection to a sorted list of graphic IDs.
        /// </summary>
        private List<int> GetSelectedGraphicIds()
        {
            var ids = new List<int>();
            foreach (int idx in LandTilesTileView.SelectedIndices)
            {
                if (idx >= 0 && idx < _tileList.Count)
                {
                    ids.Add(_tileList[idx]);
                }
            }
            ids.Sort();
            return ids;
        }

        private void LandTilesTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            if (_tileList.Count == 0)
            {
                return;
            }

            SelectedGraphicId = e.ItemIndex < 0 || e.ItemIndex > _tileList.Count
                ? _tileList[0]
                : _tileList[e.ItemIndex];
        }

        private void ReplaceStartingFromTb_KeyDown(object sender, KeyEventArgs e)
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

            LandTilesContextMenuStrip.Close();

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
                        AddSingleLandTile(dialog.FileNames[i], currentIdx);
                    }
                }

                LandTilesTileView.VirtualListSize = _tileList.Count;
                LandTilesTileView.Invalidate();
                SelectedGraphicId = index;

                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        /// <summary>
        /// Check if it's valid index for land tile. Land tiles has fixed size 0x4000.
        /// </summary>
        /// <param name="index">Starting Index</param>
        private static bool IsIndexValid(int index)
        {
            return index < 0x4000;
        }

        private void OnClickReplaceFromFolder(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = _localizationGetter?.Invoke("Forms.LandTilesControl.Messages.SelectFolderForReplaceDescription") ?? "Select folder containing images to replace";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string[] allFiles = Directory.GetFiles(dialog.SelectedPath);
            var replacedLines = new List<string>();
            var skippedLines = new List<string>();

            foreach (string file in allFiles)
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".bmp" && ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".tif" && ext != ".tiff")
                {
                    continue;
                }

                string name = Path.GetFileName(file);
                Match match = _hexIndexRegex.Match(Path.GetFileNameWithoutExtension(file));
                if (!match.Success)
                {
                    skippedLines.Add($"  {name}  (no hex ID in filename)");
                    continue;
                }

                int index;
                try
                {
                    index = Convert.ToInt32(match.Value, 16);
                }
                catch
                {
                    skippedLines.Add($"  {name}  (invalid hex value)");
                    continue;
                }

                if (!IsIndexValid(index))
                {
                    skippedLines.Add($"  {name}  (index 0x{index:X} out of range)");
                    continue;
                }

                try
                {
                    AddSingleLandTile(file, index);
                    replacedLines.Add($"  0x{index:X4}  {name}");
                }
                catch
                {
                    skippedLines.Add($"  {name}  (failed to load image)");
                }
            }

            LandTilesTileView.VirtualListSize = _tileList.Count;
            LandTilesTileView.Invalidate();

            if (replacedLines.Count > 0)
            {
                Options.ChangedUltimaClass["Art"] = true;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Replaced: {replacedLines.Count}    Skipped: {skippedLines.Count}");

            if (replacedLines.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Replaced ({replacedLines.Count}):");
                foreach (string line in replacedLines)
                {
                    sb.AppendLine(line);
                }
            }

            if (skippedLines.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Skipped ({skippedLines.Count}):");
                foreach (string line in skippedLines)
                {
                    sb.AppendLine(line);
                }
            }

            using var resultForm = new ReplaceFromFolderResultForm(sb.ToString());
            resultForm.ShowDialog(this);
        }

        /// <summary>
        /// Adds a single land tile.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the land tile will be added.</param>
        private void AddSingleLandTile(string fileName, int index)
        {
            using (var bmpTemp = new Bitmap(fileName))
            {
                Bitmap bitmap = new Bitmap(bmpTemp);

                if (fileName.Contains(".bmp"))
                {
                    bitmap = Utils.ConvertBmp(bitmap);
                }

                Art.ReplaceLand(index, bitmap);

                ControlEvents.FireLandTileChangeEvent(this, index);

                bool done = false;

                for (int i = 0; i < _tileList.Count; ++i)
                {
                    if (index > _tileList[i])
                    {
                        continue;
                    }

                    _tileList[i] = index;
                    done = true;
                    break;
                }

                if (!done)
                {
                    _tileList.Add(index);
                }
            }
        }

        public void UpdateTileView()
        {
            LandTilesTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            LandTilesTileView.BackColor = Options.PreviewBackgroundColor;

            var sameFocusColor = LandTilesTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = LandTilesTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameFocusColor && sameSelectionColor)
            {
                return;
            }

            LandTilesTileView.TileFocusColor = Options.TileFocusColor;
            LandTilesTileView.TileHighlightColor = Options.TileSelectionColor;
            LandTilesTileView.Invalidate();
        }

        private void ShowFreeSlotsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;

            if (_showFreeSlots)
            {
                for (int j = 0; j < _landTileMax; ++j)
                {
                    if (_tileList.Count > j)
                    {
                        if (_tileList[j] != j)
                        {
                            _tileList.Insert(j, j);
                        }
                    }
                    else
                    {
                        _tileList.Insert(j, j);
                    }
                }

                var prevSelected = SelectedGraphicId;

                LandTilesTileView.VirtualListSize = _tileList.Count;

                if (prevSelected >= 0)
                {
                    SelectedGraphicId = prevSelected;
                }

                LandTilesTileView.Invalidate();
            }
            else
            {
                Reload();
            }
        }

        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue))
            {
                return;
            }

            const int maximumIndex = 0x3FFF;

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue > maximumIndex)
            {
                indexValue = maximumIndex;
            }

            // we have to invalidate focus so it will scroll to item
            LandTilesTileView.FocusIndex = -1;
            SelectedGraphicId = indexValue;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Copy/paste is handled here rather than as menu ShortcutKeys: a shortcut on a
            // ContextMenuStrip is processed for the whole form, which would swallow Ctrl+C/Ctrl+V in
            // every text box on every tab.
            if (keyData == (Keys.Control | Keys.C) && LandTilesTileView.Focused)
            {
                OnClickCopyImage(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.V) && LandTilesTileView.Focused)
            {
                OnClickPasteImage(this, EventArgs.Empty);
                return true;
            }

            if (keyData == Keys.F3 || keyData == (Keys.F3 | Keys.Shift))
            {
                if (searchByNameToolStripTextBox.TextBox.Focused)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(searchByNameToolStripTextBox.Text))
                {
                    if (keyData == Keys.F3)
                    {
                        SearchName(searchByNameToolStripTextBox.Text, true);
                    }
                    else
                    {
                        SearchNamePrevious(searchByNameToolStripTextBox.Text);
                    }
                }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public static bool SearchNamePrevious(string name)
        {
            var searchMethod = SearchHelper.GetSearchMethod();

            int index = _refMarker._tileList.Count - 1;
            if (_refMarker._selectedGraphicId >= 0)
            {
                index = _refMarker._tileList.IndexOf(_refMarker._selectedGraphicId) - 1;
                if (index < 0)
                {
                    index = _refMarker._tileList.Count - 1;
                }
            }

            for (int i = index; i >= 0; --i)
            {
                var searchResult = searchMethod(name, TileData.LandTable[_refMarker._tileList[i]].Name);
                if (searchResult.HasErrors)
                {
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue;
                }

                _refMarker.LandTilesTileView.FocusIndex = -1;
                _refMarker.SelectedGraphicId = _refMarker._tileList[i];
                return true;
            }

            return false;
        }

        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                if (e.Shift)
                {
                    SearchNamePrevious(searchByNameToolStripTextBox.Text);
                }
                else
                {
                    SearchName(searchByNameToolStripTextBox.Text, true);
                }
                return;
            }

            SearchName(searchByNameToolStripTextBox.Text, false);
        }

        private void SearchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, true);
        }

        public void SetLocalization(Func<string, string?>? getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // Menu Items
            showFreeSlotsToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.showFreeSlotsToolStripMenuItem") ?? "Show Free Slots";
            findNextFreeSlotToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.findNextFreeSlotToolStripMenuItem") ?? "Find Next Free Slot";
            changeBackgroundColorToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.changeBackgroundColorToolStripMenuItem") ?? "Change background color";
            exportImageToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.exportImageToolStripMenuItem") ?? "Export Image..";
            asBmpToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.asBmpToolStripMenuItem") ?? "As Bmp";
            asTiffToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.asTiffToolStripMenuItem") ?? "As Tiff";
            asJpgToolStripMenuItem1.Text = _localizationGetter("Forms.LandTilesControl.asJpgToolStripMenuItem1") ?? "As Jpg";
            asPngToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.asPngToolStripMenuItem") ?? "As Png";
            selectInTileDataTabToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.selectInTileDataTabToolStripMenuItem") ?? "Select in TileData tab";
            selectInRadarColorTabToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.selectInRadarColorTabToolStripMenuItem") ?? "Select in RadarColor tab";
            selectInTexturesTabToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.selectInTexturesTabToolStripMenuItem") ?? "Select in Textures tab";
            copyImageToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.copyImageToolStripMenuItem") ?? "Copy Image";
            pasteImageToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.pasteImageToolStripMenuItem") ?? "Paste Image";
            replaceToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.replaceToolStripMenuItem") ?? "Replace";
            replaceStartingFromToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.replaceStartingFromToolStripMenuItem") ?? "Replace starting from..";
            replaceFromFolderToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.replaceFromFolderToolStripMenuItem") ?? "Replace from Folder...";
            insertAtToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.insertAtToolStripMenuItem") ?? "Insert At..";
            removeToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.removeToolStripMenuItem") ?? "Remove";
            saveToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.saveToolStripMenuItem") ?? "Save";

            // Toolbar Labels
            IndexToolStripLabel.Text = _localizationGetter("Forms.LandTilesControl.IndexToolStripLabel") ?? "Index:";
            NameToolStripLabel.Text = _localizationGetter("Forms.LandTilesControl.NameToolStripLabel") ?? "Name:";
            searchByNameToolStripButton.Text = _localizationGetter("Forms.LandTilesControl.searchByNameToolStripButton") ?? "Find next";

            // Misc Menu
            MiscToolStripDropDownButton.Text = _localizationGetter("Forms.LandTilesControl.MiscToolStripDropDownButton") ?? "Misc";
            exportAllToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.exportAllToolStripMenuItem") ?? "Export All..";
            ExportAllAsBmp.Text = _localizationGetter("Forms.LandTilesControl.ExportAllAsBmp") ?? "As Bmp";
            ExportAllAsTiff.Text = _localizationGetter("Forms.LandTilesControl.ExportAllAsTiff") ?? "As Tiff";
            asJpgToolStripMenuItem.Text = _localizationGetter("Forms.LandTilesControl.asJpgToolStripMenuItem") ?? "As Jpg";
            asPngToolStripMenuItem1.Text = _localizationGetter("Forms.LandTilesControl.asPngToolStripMenuItem1") ?? "As Png";

            // Save Button
            SaveButton.Text = _localizationGetter("Forms.LandTilesControl.SaveButton") ?? "Save";
            SaveButton.ToolTipText = _localizationGetter("Forms.LandTilesControl.SaveButton") ?? "Save";

            // Status Bar Labels
            NameLabel.Text = _localizationGetter("Forms.LandTilesControl.NameLabel") ?? "Name:";
            GraphicLabel.Text = _localizationGetter("Forms.LandTilesControl.GraphicLabel") ?? "Graphic:";
            FlagsLabel.Text = _localizationGetter("Forms.LandTilesControl.FlagsLabel") ?? "Flags:";
        }
    }
}
