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
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.UserControls
{
    public partial class ItemsControl : UserControl
    {
        public ItemsControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            RefMarker = this;
            DetailTextBox.AddBasicContextMenu();
        }

        private static readonly Regex _hexIndexRegex = new(@"0[xX][0-9a-fA-F]+", RegexOptions.Compiled);

        private List<int> _itemList = new List<int>();
        private bool _showFreeSlots;

        private int _selectedGraphicId = -1;
        private Func<string, string>? _localizationGetter;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedGraphicId
        {
            get => _selectedGraphicId;
            set
            {
                _selectedGraphicId = value < 0 ? 0 : value;
                ItemsTileView.FocusIndex = _itemList.Count == 0 ? -1 : _itemList.IndexOf(_selectedGraphicId);

                UpdateToolStripLabels(_selectedGraphicId);
                UpdateDetail(_selectedGraphicId);
            }
        }

        public IReadOnlyList<int> ItemList { get => _itemList.AsReadOnly(); }
        public static ItemsControl RefMarker { get; private set; }
        public static TileViewControl TileView => RefMarker.ItemsTileView;
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Updates if TileSize is changed
        /// </summary>
        public void UpdateTileView()
        {
            var newSize = new Size(Options.ArtItemSizeWidth, Options.ArtItemSizeHeight);

            ItemsTileView.TileBorderColor = Options.RemoveTileBorder
                ? Color.Transparent
                : Color.Gray;

            var sameBackColor = ItemsTileView.BackColor == Options.PreviewBackgroundColor;
            ItemsTileView.BackColor = Options.PreviewBackgroundColor;

            var sameTileSize = ItemsTileView.TileSize == newSize;
            var sameFocusColor = ItemsTileView.TileFocusColor == Options.TileFocusColor;
            var sameSelectionColor = ItemsTileView.TileHighlightColor == Options.TileSelectionColor;
            if (sameTileSize && sameFocusColor && sameSelectionColor && sameBackColor)
            {
                return;
            }

            ItemsTileView.TileFocusColor = Options.TileFocusColor;
            ItemsTileView.TileHighlightColor = Options.TileSelectionColor;

            ItemsTileView.TileSize = newSize;
            ItemsTileView.Invalidate();

            if (_selectedGraphicId != -1)
            {
                UpdateDetail(_selectedGraphicId);
            }
        }

        /// <summary>
        /// Searches graphic number and selects it
        /// </summary>
        /// <param name="graphic"></param>
        /// <returns></returns>
        public static bool SearchGraphic(int graphic)
        {
            if (RefMarker == null)
            {
                return false;
            }

            if (!RefMarker.IsLoaded)
            {
                RefMarker.OnLoad(RefMarker, EventArgs.Empty);
            }

            if (RefMarker._itemList.TrueForAll(t => t != graphic))
            {
                return false;
            }

            TabPageNavigator.ActivateOwningTabPage(RefMarker);

            if (RefMarker.IsHandleCreated)
            {
                RefMarker.BeginInvoke(new Action(() =>
                {
                    // we have to invalidate focus so it will scroll to item
                    RefMarker.ItemsTileView.FocusIndex = -1;
                    RefMarker.SelectedGraphicId = graphic;
                }));
            }
            else
            {
                RefMarker.ItemsTileView.FocusIndex = -1;
                RefMarker.SelectedGraphicId = graphic;
            }

            return true;
        }

        /// <summary>
        /// Searches for name and selects
        /// </summary>
        /// <param name="name"></param>
        /// <param name="next">starting from current selected</param>
        /// <returns></returns>
        public static bool SearchName(string name, bool next)
        {
            int index = 0;
            if (next)
            {
                if (RefMarker._selectedGraphicId >= 0)
                {
                    index = RefMarker._itemList.IndexOf(RefMarker._selectedGraphicId) + 1;
                }

                if (index >= RefMarker._itemList.Count)
                {
                    index = 0;
                }
            }

            var searchMethod = SearchHelper.GetSearchMethod();

            for (int i = index; i < RefMarker._itemList.Count; ++i)
            {
                var searchResult = searchMethod(name, TileData.ItemTable[RefMarker._itemList[i]].Name);
                if (searchResult.HasErrors)
                {
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue;
                }

                // we have to invalidate focus so it will scroll to item
                RefMarker.ItemsTileView.FocusIndex = -1;
                RefMarker.SelectedGraphicId = RefMarker._itemList[i];

                return true;
            }

            return false;
        }

        public void OnLoad(object sender, EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (IsLoaded && (!(e is MyEventArgs args) || args.Type != MyEventArgs.Types.ForceReload))
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                Options.LoadedUltimaClass["TileData"] = true;
                Options.LoadedUltimaClass["Art"] = true;
                Options.LoadedUltimaClass["Animdata"] = true;
                Options.LoadedUltimaClass["Hues"] = true;

                if (!IsLoaded) // only once
                {
                    Plugin.PluginEvents.FireModifyItemShowContextMenuEvent(TileViewContextMenuStrip);
                }

                UpdateTileView();

                _showFreeSlots = false;
                showFreeSlotsToolStripMenuItem.Checked = false;

                var prevSelected = SelectedGraphicId;

                int staticLength = Art.GetMaxItemId();
                _itemList = new List<int>(staticLength);
                for (int i = 0; i <= staticLength; ++i)
                {
                    if (Art.IsValidStatic(i))
                    {
                        _itemList.Add(i);
                    }
                }

                ItemsTileView.VirtualListSize = _itemList.Count;

                if (prevSelected >= 0)
                {
                    SelectedGraphicId = _itemList.Contains(prevSelected) ? prevSelected : 0;
                }

                if (!IsLoaded)
                {
                    ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                    ControlEvents.ItemChangeEvent += OnItemChangeEvent;
                    ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;
                    ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;
                }

                IsLoaded = true;
            }
        }

        /// <summary>
        /// ReLoads if loaded
        /// </summary>
        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnPreviewBackgroundColorChanged()
        {
            ItemsTileView.BackColor = Options.PreviewBackgroundColor;
            ItemsTileView.Invalidate();
            if (_selectedGraphicId != -1)
            {
                UpdateDetail(_selectedGraphicId);
            }
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

            if (id < 0x4000)
            {
                return;
            }

            id -= 0x4000;

            if (_selectedGraphicId != id)
            {
                return;
            }

            UpdateToolStripLabels(id);
            UpdateDetail(id);
        }

        private void OnItemChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Art.IsValidStatic(index))
            {
                bool done = false;
                for (int i = 0; i < _itemList.Count; ++i)
                {
                    if (index < _itemList[i])
                    {
                        _itemList.Insert(i, index);
                        done = true;
                        break;
                    }

                    if (index != _itemList[i])
                    {
                        continue;
                    }

                    done = true;
                    break;
                }

                if (!done)
                {
                    _itemList.Add(index);
                }
            }
            else
            {
                if (_showFreeSlots)
                {
                    return;
                }

                _itemList.Remove(index);
            }

            ItemsTileView.VirtualListSize = _itemList.Count;
            ItemsTileView.Invalidate();
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

        private void UpdateDetail(int graphic)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!IsLoaded)
            {
                return;
            }

            if (_scrolling)
            {
                return;
            }

            ItemData item = TileData.ItemTable[graphic];
            Bitmap bit = Art.GetStatic(graphic);

            int xMin = 0;
            int xMax = 0;
            int yMin = 0;
            int yMax = 0;

            const int defaultSplitterDistance = 180;
            if (bit == null)
            {
                splitContainer2.SplitterDistance = defaultSplitterDistance;
                Bitmap newBit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(Options.PreviewBackgroundColor);
                }

                DetailPictureBox.Image?.Dispose();
                DetailPictureBox.Image = newBit;
            }
            else
            {
                var distance = bit.Size.Height + 10;
                splitContainer2.SplitterDistance = distance < defaultSplitterDistance ? defaultSplitterDistance : distance;

                Bitmap newBit = new Bitmap(DetailPictureBox.Size.Width, DetailPictureBox.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(Options.PreviewBackgroundColor);
                    newGraph.DrawImage(bit, (DetailPictureBox.Size.Width - bit.Width) / 2, 5);
                }

                DetailPictureBox.Image?.Dispose();
                DetailPictureBox.Image = newBit;

                Art.Measure(bit, out xMin, out yMin, out xMax, out yMax);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.Name") ?? "Name")}: {item.Name}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.Graphic") ?? "Graphic")}: 0x{graphic:X4}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.HeightCapacity") ?? "Height/Capacity")}: {item.Height}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.Weight") ?? "Weight")}: {item.Weight}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.Animation") ?? "Animation")}: {item.Animation}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.QualityLayerLight") ?? "Quality/Layer/Light")}: {item.Quality}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.Quantity") ?? "Quantity")}: {item.Quantity}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.Hue") ?? "Hue")}: {item.Hue}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.StackingOffsetUnk4") ?? "StackingOffset/Unk4")}: {item.StackingOffset}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.Flags") ?? "Flags")}: {item.Flags}");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.GraphicPixelSizeWidthHeight") ?? "Graphic pixel size width, height")}: {bit?.Width ?? 0} {bit?.Height ?? 0} ");
            sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.GraphicPixelOffsetXMinYMinXMaxYMax") ?? "Graphic pixel offset xMin, yMin, xMax, yMax")}: {xMin} {yMin} {xMax} {yMax}");

            if ((item.Flags & TileFlag.Animation) != 0)
            {
                Animdata.AnimdataEntry info = Animdata.GetAnimData(graphic);
                if (info != null)
                {
                    sb.AppendLine($"{(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.AnimationFrameCount") ?? "Animation FrameCount")}: {info.FrameCount} {(_localizationGetter?.Invoke("Forms.ItemsControl.DetailLabels.AnimationInterval") ?? "Interval")}: {info.FrameInterval}");
                }
            }

            DetailTextBox.Clear();
            DetailTextBox.AppendText(sb.ToString());
        }

        private void ChangeBackgroundColorToolStripMenuItemDetail_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Options.PreviewBackgroundColor = colorDialog.Color;
            ControlEvents.FirePreviewBackgroundColorChangeEvent();
        }

        private bool _scrolling;

        private void OnClickFindFree(object sender, EventArgs e)
        {
            if (_showFreeSlots)
            {
                int i = _selectedGraphicId > -1 ? _itemList.IndexOf(_selectedGraphicId) + 1 : 0;
                for (; i < _itemList.Count; ++i)
                {
                    if (Art.IsValidStatic(_itemList[i]))
                    {
                        continue;
                    }

                    SelectedGraphicId = _itemList[i];
                    ItemsTileView.Invalidate();
                    break;
                }
            }
            else
            {
                int id, i;

                if (_selectedGraphicId > -1)
                {
                    id = _selectedGraphicId + 1;
                    i = _itemList.IndexOf(_selectedGraphicId) + 1;
                }
                else
                {
                    id = 0;
                    i = 0;
                }

                for (; i < _itemList.Count; ++i, ++id)
                {
                    if (id >= _itemList[i])
                    {
                        continue;
                    }

                    SelectedGraphicId = _itemList[i];
                    ItemsTileView.Invalidate();
                    break;
                }
            }
        }

        private void OnClickCopyImage(object sender, EventArgs e)
        {
            // The clipboard holds one image, so a multi selection copies the focused tile.
            int id = SelectedGraphicId;
            if (id < 0 || !Art.IsValidStatic(id))
            {
                return;
            }

            if (!ImageClipboard.TryCopy(Art.GetStatic(id), out string error))
            {
                MessageBox.Show(error, "Copy Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnClickPasteImage(object sender, EventArgs e)
        {
            var ids = GetSelectedGraphicIds();
            if (ids.Count == 0)
            {
                if (SelectedGraphicId < 0)
                {
                    return;
                }

                ids.Add(SelectedGraphicId);
            }

            using Bitmap pasted = ImageClipboard.TryPaste(out string error);
            if (pasted == null)
            {
                MessageBox.Show(error, "Paste Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ids.Count > 1)
            {
                DialogResult confirm = MessageBox.Show(
                    $"Paste this {pasted.Width}x{pasted.Height} image into {ids.Count} selected items?",
                    "Paste Image", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }
            }

            Bitmap converted = Utils.ToUoBitmap(pasted);

            if (!Art.ValidateStaticSize(converted, out int estimatedSize))
            {
                converted.Dispose();

                MessageBox.Show(
                    $"Image is too large for MUL format!\n\n" +
                    $"Image dimensions: {pasted.Width}x{pasted.Height}\n" +
                    $"Encoded size: {estimatedSize:N0} ushorts\n" +
                    $"Maximum allowed: 65,535 ushorts\n\n" +
                    "Try a smaller image or one with more transparent pixels.",
                    "Image Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (int id in ids)
            {
                // Each slot needs its own bitmap: the SDK keeps the instance it is handed.
                Art.ReplaceStatic(id, ids.Count == 1 ? converted : (Bitmap)converted.Clone());
                ControlEvents.FireItemChangeEvent(this, id);
            }

            if (ids.Count > 1)
            {
                converted.Dispose();
            }

            ItemsTileView.Invalidate();

            if (SelectedGraphicId >= 0)
            {
                UpdateToolStripLabels(SelectedGraphicId);
                UpdateDetail(SelectedGraphicId);
            }

            Options.ChangedUltimaClass["Art"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (ItemsTileView.SelectedIndices.Count > 1)
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

                    // Validate image size before replacing
                    if (!Art.ValidateStaticSize(bitmap, out int estimatedSize))
                    {
                        MessageBox.Show(
                            $"Image is too large for MUL format!\n\n" +
                            $"Image dimensions: {bitmap.Width}x{bitmap.Height}\n" +
                            $"Encoded size: {estimatedSize:N0} ushorts\n" +
                            $"Maximum allowed: 65,535 ushorts\n\n" +
                            $"The static art format encodes opaque pixel runs only; cost per row is\n" +
                            $"2 ushorts per run + 1 ushort per opaque pixel + 2 end markers.\n" +
                            $"Reduce the image size or the amount of opaque content.",
                            "Image Too Large",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        return;
                    }

                    Art.ReplaceStatic(_selectedGraphicId, bitmap);

                    ControlEvents.FireItemChangeEvent(this, _selectedGraphicId);

                    ItemsTileView.Invalidate();
                    UpdateToolStripLabels(_selectedGraphicId);
                    UpdateDetail(_selectedGraphicId);

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
                dialog.Title = $"Choose {ids.Count} image files to replace selected items";
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
                        $"Selected {ids.Count} items but chose {files.Length} images.\n\nNo changes made.",
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
                                    $"Encoded size: {estimatedSize:N0} ushorts\n" +
                                    $"Maximum allowed: 65,535 ushorts\n\n" +
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
                    Art.ReplaceStatic(ids[i], bitmaps[i]);
                    ControlEvents.FireItemChangeEvent(this, ids[i]);
                }

                ItemsTileView.Invalidate();
                UpdateToolStripLabels(_selectedGraphicId);
                UpdateDetail(_selectedGraphicId);

                Options.ChangedUltimaClass["Art"] = true;
            }
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            var ids = GetSelectedGraphicIds().Where(Art.IsValidStatic).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            string prompt = ids.Count == 1
                ? $"Are you sure to remove 0x{ids[0]:X}"
                : $"Are you sure to remove {ids.Count} items?";

            DialogResult result = MessageBox.Show(prompt, "Save",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach (int id in ids)
            {
                Art.RemoveStatic(id);
                ControlEvents.FireItemChangeEvent(this, id);

                if (!_showFreeSlots)
                {
                    _itemList.Remove(id);
                }
            }

            ItemsTileView.SelectedIndices.Clear();

            if (!_showFreeSlots)
            {
                ItemsTileView.VirtualListSize = _itemList.Count;
                int moveToId = ids[0] - 1;
                SelectedGraphicId = moveToId <= 0 ? 0 : moveToId; // TODO: get last index visible instead just curr -1
            }
            ItemsTileView.Invalidate();

            Options.ChangedUltimaClass["Art"] = true;
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            Color invalidColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemId()))
            {
                InsertText.ForeColor = Art.IsValidStatic(index) ? invalidColor : SystemColors.ControlText;
            }
            else
            {
                InsertText.ForeColor = invalidColor;
            }
        }

        private void OnKeyDownInsertText(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Art.GetMaxItemId()))
            {
                return;
            }

            if (Art.IsValidStatic(index))
            {
                return;
            }

            TileViewContextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose images to replace starting at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";

                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                AddSingleItem(dialog.FileName, index);
            }
        }

        private void UpdateToolStripLabels(int graphic)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (!IsLoaded)
            {
                return;
            }

            if (_scrolling)
            {
                return;
            }

            NameLabel.Text = !Art.IsValidStatic(graphic) ? "Name: FREE" : $"Name: {TileData.ItemTable[graphic].Name}";
            GraphicLabel.Text = $"Graphic: 0x{graphic:X4} ({graphic})";
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure? Will take a while", "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (result != DialogResult.Yes)
            {
                return;
            }

            ClientFileSaveCommand.Run(this, FileType.ArtLegacyMul, Art.Save, "Art",
                createProgress: () => new ProgressBarDialog(Art.GetIdxLength(), "Save"));
        }

        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;
            if (_showFreeSlots)
            {
                for (int j = 0; j <= Art.GetMaxItemId(); ++j)
                {
                    if (_itemList.Count > j)
                    {
                        if (_itemList[j] != j)
                        {
                            _itemList.Insert(j, j);
                        }
                    }
                    else
                    {
                        _itemList.Insert(j, j);
                    }
                }

                var prevSelected = SelectedGraphicId;

                ItemsTileView.VirtualListSize = _itemList.Count;

                if (prevSelected >= 0)
                {
                    SelectedGraphicId = prevSelected;
                }

                ItemsTileView.Invalidate();
            }
            else
            {
                Reload();
            }
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Bmp);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Tiff);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Jpeg);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            ExportSelected(ImageFormat.Png);
        }

        private void ExportSelected(ImageFormat imageFormat)
        {
            var ids = GetSelectedGraphicIds().Where(Art.IsValidStatic).ToList();
            if (ids.Count == 0)
            {
                return;
            }

            if (ids.Count == 1)
            {
                ExportItemImage(ids[0], imageFormat);
                return;
            }

            ExportMultipleItemImages(ids, imageFormat);
        }

        private void ExportMultipleItemImages(List<int> ids, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            foreach (int index in ids)
            {
                var artBitmap = Art.GetStatic(index);
                if (artBitmap is null)
                {
                    continue;
                }

                string fileName = Path.Combine(Options.OutputPath, $"Item {Utils.FormatExportId(index)}.{fileExtension}");
                using (Bitmap bit = new Bitmap(artBitmap))
                {
                    bit.Save(fileName, imageFormat);
                }
            }

            FileSavedDialog.Show(FindForm(), Options.OutputPath, $"{ids.Count} items saved successfully.");
        }

        private static void ExportItemImage(int index, ImageFormat imageFormat)
        {
            if (!Art.IsValidStatic(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Item {Utils.FormatExportId(index)}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Art.GetStatic(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Item saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            // Carry the whole selection over, so a set of tiles picked here can be
            // edited in one go on the TileData tab.
            List<int> ids = GetSelectedGraphicIds();
            if (ids.Count > 0)
            {
                TileDataControl.Select(ids, false);
            }
            else if (_selectedGraphicId >= 0)
            {
                TileDataControl.Select(_selectedGraphicId, false);
            }
        }

        private void OnClickSelectRadarCol(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
            {
                RadarColorControl.Select(_selectedGraphicId, false);
            }
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Bmp);
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Tiff);
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Jpeg);
        }

        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllItemImages(ImageFormat.Png);
        }

        private void ExportAllItemImages(ImageFormat imageFormat)
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
                    using (new ProgressBarDialog(_itemList.Count, $"Export to {fileExtension}", false))
                    {
                        foreach (var artItemIndex in _itemList)
                        {
                            ControlEvents.FireProgressChangeEvent();
                            Application.DoEvents();

                            int index = artItemIndex;
                            if (index < 0)
                            {
                                continue;
                            }

                            string fileName = Path.Combine(dialog.SelectedPath, $"Item {Utils.FormatExportId(index)}.{fileExtension}");
                            var artBitmap = Art.GetStatic(index);
                            if (artBitmap is null)
                            {
                                continue;
                            }

                            using (Bitmap bit = new Bitmap(artBitmap))
                            {
                                bit.Save(fileName, imageFormat);
                            }
                        }
                    }
                }

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All items saved successfully.");
            }
        }

        private void OnClickPreLoad(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = _itemList.Count;
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            int total = _itemList.Count;
            int reportEvery = Math.Max(1, total / 200);
            int sinceReport = 0;
            int done = 0;
            foreach (int item in _itemList)
            {
                Art.GetStatic(item);
                ++done;
                if (++sinceReport >= reportEvery)
                {
                    sinceReport = 0;
                    PreLoader.ReportProgress(done);
                }
            }
            PreLoader.ReportProgress(done);
        }

        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = Math.Min(ProgressBar.Maximum, Math.Max(ProgressBar.Minimum, e.ProgressPercentage));
        }

        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }

        private void ItemsTileView_DrawItem(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + ItemsTileView.TilePadding.Left, e.Bounds.Y + ItemsTileView.TilePadding.Top);

            Rectangle rect = new Rectangle(itemPoint, ItemsTileView.TileSize);

            using var previousClip = e.Graphics.Clip;

            using var clipRegion = new Region(rect);
            e.Graphics.Clip = clipRegion;

            var selected = ItemsTileView.SelectedIndices.Contains(e.Index);
            if (!selected)
            {
                e.Graphics.Clear(Options.PreviewBackgroundColor);
            }

            var bitmap = Art.GetStatic(_itemList[e.Index], out bool patched);
            if (bitmap == null)
            {
                rect.X += 5;
                rect.Y += 5;

                rect.Width -= 10;
                rect.Height -= 10;

                e.Graphics.FillRectangle(Brushes.Red, rect);
                e.Graphics.Clip = previousClip;
            }
            else
            {
                if (patched && !selected)
                {
                    e.Graphics.FillRectangle(Brushes.LightCoral, rect);
                }

                if (Options.ArtItemClip)
                {
                    e.Graphics.DrawImage(bitmap, itemPoint);
                }
                else
                {
                    int width = bitmap.Width;
                    int height = bitmap.Height;
                    if (width > ItemsTileView.TileSize.Width)
                    {
                        width = ItemsTileView.TileSize.Width;
                        height = ItemsTileView.TileSize.Height * bitmap.Height / bitmap.Width;
                    }

                    if (height > ItemsTileView.TileSize.Height)
                    {
                        height = ItemsTileView.TileSize.Height;
                        width = ItemsTileView.TileSize.Width * bitmap.Width / bitmap.Height;
                    }

                    e.Graphics.DrawImage(bitmap, new Rectangle(itemPoint, new Size(width, height)));
                }

                if (Art.IsStaticModified(_itemList[e.Index]))
                {
                    ModifiedMarker.Draw(e.Graphics, rect);
                }

                e.Graphics.Clip = previousClip;
            }
        }

        private void ItemsTileView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (!e.IsSelected)
            {
                return;
            }

            UpdateSelection(e.ItemIndex);
        }

        private void ItemsTileView_FocusSelectionChanged(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (!e.IsFocused)
            {
                return;
            }

            UpdateSelection(e.FocusedItemIndex);
        }

        /// <summary>
        /// Resolves the current tile selection to a sorted list of graphic IDs.
        /// </summary>
        private List<int> GetSelectedGraphicIds()
        {
            var ids = new List<int>();
            foreach (int idx in ItemsTileView.SelectedIndices)
            {
                if (idx >= 0 && idx < _itemList.Count)
                {
                    ids.Add(_itemList[idx]);
                }
            }
            ids.Sort();
            return ids;
        }

        private void UpdateSelection(int itemIndex)
        {
            if (_itemList.Count == 0)
            {
                return;
            }

            SelectedGraphicId = itemIndex < 0 || itemIndex > _itemList.Count
                ? _itemList[0]
                : _itemList[itemIndex];
        }

        public void ItemsTileView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (ItemsTileView.SelectedIndices.Count == 0)
            {
                return;
            }

            ItemDetailForm f = new ItemDetailForm(_itemList[ItemsTileView.SelectedIndices[0]])
            {
                TopMost = true
            };
            f.Show();
        }

        private void ItemsTileView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.PageDown || e.KeyData == Keys.PageUp)
            {
                _scrolling = true;
            }
        }

        private void ItemsTileView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData != Keys.PageDown && e.KeyData != Keys.PageUp)
            {
                return;
            }

            _scrolling = false;

            if (ItemsTileView.FocusIndex > 0)
            {
                UpdateToolStripLabels(_selectedGraphicId);
                UpdateDetail(_selectedGraphicId);
            }
        }

        private const int _maleGumpOffset = 50_000;
        private const int _femaleGumpOffset = 60_000;

        private static void SelectInGumpsTab(int graphicId, bool female = false)
        {
            int gumpOffset = female ? _femaleGumpOffset : _maleGumpOffset;
            var itemData = TileData.ItemTable[graphicId];

            GumpControl.Select(itemData.Animation + gumpOffset);
        }

        private void SelectInGumpsTabMaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedGraphicId <= 0)
            {
                return;
            }

            SelectInGumpsTab(SelectedGraphicId);
        }

        private void SelectInGumpsTabFemaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedGraphicId <= 0)
            {
                return;
            }

            SelectInGumpsTab(SelectedGraphicId, true);
        }

        private void DetailPictureBoxContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            copyImageToolStripMenuItemDetail.Enabled = SelectedGraphicId >= 0 && Art.IsValidStatic(SelectedGraphicId);
            pasteImageToolStripMenuItemDetail.Enabled = SelectedGraphicId >= 0 && ImageClipboard.ContainsImage();
        }

        private void TileViewContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            int selectedCount = ItemsTileView.SelectedIndices.Count;
            copyImageToolStripMenuItem.Enabled = SelectedGraphicId >= 0 && Art.IsValidStatic(SelectedGraphicId);
            pasteImageToolStripMenuItem.Enabled = selectedCount > 0 && ImageClipboard.ContainsImage();
            
            // 类型 12：菜单 Opening 事件中使用汉化基础文本构建动态文本
            string basePasteText = _localizationGetter?.Invoke("Forms.ItemsControl.pasteImageToolStripMenuItem") ?? "Paste Image";
            string baseRemoveText = _localizationGetter?.Invoke("Forms.ItemsControl.removeToolStripMenuItem") ?? "Remove";
            string baseExtractText = _localizationGetter?.Invoke("Forms.ItemsControl.extractToolStripMenuItem") ?? "Export Image..";
            string baseReplaceText = _localizationGetter?.Invoke("Forms.ItemsControl.replaceToolStripMenuItem") ?? "Replace...";
            string baseSelectTileDataText = _localizationGetter?.Invoke("Forms.ItemsControl.selectInTileDataTabToolStripMenuItem") ?? "Select in TileData tab";
            
            pasteImageToolStripMenuItem.Text = selectedCount > 1 ? $"{basePasteText} {selectedCount}" : basePasteText;
            removeToolStripMenuItem.Text = selectedCount > 1 ? $"{baseRemoveText} {selectedCount}" : baseRemoveText;
            extractToolStripMenuItem.Text = selectedCount > 1 ? $"{baseExtractText.Replace("...", "")} {selectedCount}..." : baseExtractText;
            replaceToolStripMenuItem.Text = selectedCount > 1 ? $"{baseReplaceText.Replace("...", "")} {selectedCount}..." : baseReplaceText;
            selectInTileDataTabToolStripMenuItem.Text = selectedCount > 1
                ? $"{baseSelectTileDataText.Replace(" tab", "")} {selectedCount} tab"
                : baseSelectTileDataText;

            if (SelectedGraphicId <= 0)
            {
                selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
                selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
            }
            else
            {
                var itemData = TileData.ItemTable[SelectedGraphicId];

                if (itemData.Animation > 0)
                {
                    selectInGumpsTabMaleToolStripMenuItem.Enabled =
                        GumpControl.HasGumpId(itemData.Animation + _maleGumpOffset);

                    selectInGumpsTabFemaleToolStripMenuItem.Enabled =
                        GumpControl.HasGumpId(itemData.Animation + _femaleGumpOffset);
                }
                else
                {
                    selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
                    selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
                }
            }
        }

        private void ReplaceStartingFromText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(ReplaceStartingFromText.Text, out int index, 0, Art.GetMaxItemId()))
            {
                return;
            }

            TileViewContextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose image file replace starting at 0x{index:X}";
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
                        AddSingleItem(dialog.FileNames[i], currentIdx);
                    }
                }

                ItemsTileView.VirtualListSize = _itemList.Count;
                ItemsTileView.Invalidate();

                SelectedGraphicId = index;

                UpdateToolStripLabels(index);
                UpdateDetail(index);
            }
        }

        /// <summary>
        /// Adds a single static item.
        /// </summary>
        /// <param name="fileName">Filename of the image to add.</param>
        /// <param name="index">Index where the static item will be added.</param>
        private void AddSingleItem(string fileName, int index)
        {
            using (var bmpTemp = new Bitmap(fileName))
            {
                Bitmap bitmap = new Bitmap(bmpTemp);

                if (fileName.Contains(".bmp"))
                {
                    bitmap = Utils.ConvertBmp(bitmap);
                }

                Art.ReplaceStatic(index, bitmap);

                ControlEvents.FireItemChangeEvent(this, index);

                Options.ChangedUltimaClass["Art"] = true;

                if (_showFreeSlots)
                {
                    SelectedGraphicId = index;

                    UpdateToolStripLabels(index);
                    UpdateDetail(index);
                }
                else
                {
                    bool done = false;

                    for (int i = 0; i < _itemList.Count; ++i)
                    {
                        if (index > _itemList[i])
                        {
                            continue;
                        }

                        _itemList[i] = index;

                        done = true;

                        break;
                    }

                    if (!done)
                    {
                        _itemList.Add(index);
                    }

                    ItemsTileView.VirtualListSize = _itemList.Count;
                    ItemsTileView.Invalidate();

                    SelectedGraphicId = index;

                    UpdateToolStripLabels(index);
                    UpdateDetail(index);
                }
            }
        }

        /// <summary>
        /// Check if it's valid index for land tile. Land tiles has fixed size 0x4000.
        /// </summary>
        /// <param name="index">Starting Index</param>
        private static bool IsIndexValid(int index)
        {
            return index >= 0 && index <= Art.GetMaxItemId();
        }

        private void OnClickReplaceFromFolder(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select folder containing images to replace";

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
                    AddSingleItem(file, index);
                    replacedLines.Add($"  0x{index:X4}  {name}");
                }
                catch
                {
                    skippedLines.Add($"  {name}  (failed to load image)");
                }
            }

            ItemsTileView.VirtualListSize = _itemList.Count;
            ItemsTileView.Invalidate();

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

        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue))
            {
                return;
            }

            var maximumIndex = Art.GetMaxItemId();

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue > maximumIndex)
            {
                indexValue = maximumIndex;
            }

            // we have to invalidate focus so it will scroll to item
            ItemsTileView.FocusIndex = -1;
            SelectedGraphicId = indexValue;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Copy/paste is handled here rather than as menu ShortcutKeys: a shortcut on a
            // ContextMenuStrip is processed for the whole form, which would swallow Ctrl+C/Ctrl+V in
            // every text box on every tab.
            if (keyData == (Keys.Control | Keys.C) && ItemsTileView.Focused)
            {
                OnClickCopyImage(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.V) && ItemsTileView.Focused)
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

            int index = RefMarker._itemList.Count - 1;
            if (RefMarker._selectedGraphicId >= 0)
            {
                index = RefMarker._itemList.IndexOf(RefMarker._selectedGraphicId) - 1;
                if (index < 0)
                {
                    index = RefMarker._itemList.Count - 1;
                }
            }

            for (int i = index; i >= 0; --i)
            {
                var searchResult = searchMethod(name, TileData.ItemTable[RefMarker._itemList[i]].Name);
                if (searchResult.HasErrors)
                {
                    break;
                }

                if (!searchResult.EntryFound)
                {
                    continue;
                }

                RefMarker.ItemsTileView.FocusIndex = -1;
                RefMarker.SelectedGraphicId = RefMarker._itemList[i];
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

        public void SetLocalization(Func<string, string> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null)
                return;

            // ToolStrip Items
            toolStripLabel1.Text = _localizationGetter("Forms.ItemsControl.toolStripLabel1") ?? "Index:";
            toolStripLabel2.Text = _localizationGetter("Forms.ItemsControl.toolStripLabel2") ?? "Name:";
            searchByNameToolStripButton.Text = _localizationGetter("Forms.ItemsControl.searchByNameToolStripButton") ?? "Find next";
            PreloadItemsToolStripButton.Text = _localizationGetter("Forms.ItemsControl.PreloadItemsToolStripButton") ?? "Preload Items";
            MiscToolStripDropDownButton.Text = _localizationGetter("Forms.ItemsControl.MiscToolStripDropDownButton") ?? "Misc";
            ExportAllToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.ExportAllToolStripMenuItem") ?? "Export all..";

            // Context Menu Items - Tile View
            showFreeSlotsToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.showFreeSlotsToolStripMenuItem") ?? "Show Free Slots";
            findNextFreeSlotToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.findNextFreeSlotToolStripMenuItem") ?? "Find Next Free Slot";
            ChangeBackgroundColorToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.ChangeBackgroundColorToolStripMenuItem") ?? "Change background color";
            extractToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.extractToolStripMenuItem") ?? "Export Image..";
            bmpToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.bmpToolStripMenuItem") ?? "As Bmp";
            tiffToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.tiffToolStripMenuItem") ?? "As Tiff";
            asJpgToolStripMenuItem1.Text = _localizationGetter("Forms.ItemsControl.asJpgToolStripMenuItem1") ?? "As Jpg";
            asPngToolStripMenuItem1.Text = _localizationGetter("Forms.ItemsControl.asPngToolStripMenuItem1") ?? "As Png";
            selectInTileDataTabToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.selectInTileDataTabToolStripMenuItem") ?? "Select in TileData tab";
            selectInRadarColorTabToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.selectInRadarColorTabToolStripMenuItem") ?? "Select in RadarColor tab";
            selectInGumpsTabMaleToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.selectInGumpsTabMaleToolStripMenuItem") ?? "Select in Gumps (M)";
            selectInGumpsTabFemaleToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.selectInGumpsTabFemaleToolStripMenuItem") ?? "Select in Gumps (F)";
            copyImageToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.copyImageToolStripMenuItem") ?? "Copy Image";
            pasteImageToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.pasteImageToolStripMenuItem") ?? "Paste Image";
            replaceToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.replaceToolStripMenuItem") ?? "Replace...";
            replaceStartingFromToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.replaceStartingFromToolStripMenuItem") ?? "Replace starting from..";
            replaceFromFolderToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.replaceFromFolderToolStripMenuItem") ?? "Replace from Folder...";
            insertAtToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.insertAtToolStripMenuItem") ?? "Insert At..";
            removeToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.removeToolStripMenuItem") ?? "Remove";
            saveToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.saveToolStripMenuItem") ?? "Save";

            // Context Menu Items - Detail Picture
            changeBackgroundColorToolStripMenuItemDetail.Text = _localizationGetter("Forms.ItemsControl.changeBackgroundColorToolStripMenuItemDetail") ?? "Change background color";
            copyImageToolStripMenuItemDetail.Text = _localizationGetter("Forms.ItemsControl.copyImageToolStripMenuItemDetail") ?? "Copy Image";
            pasteImageToolStripMenuItemDetail.Text = _localizationGetter("Forms.ItemsControl.pasteImageToolStripMenuItemDetail") ?? "Paste Image";

            // Export Formats
            asBmpToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.asBmpToolStripMenuItem") ?? "As Bmp";
            asTiffToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.asTiffToolStripMenuItem") ?? "As Tiff";
            asJpgToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.asJpgToolStripMenuItem") ?? "As Jpg";
            asPngToolStripMenuItem.Text = _localizationGetter("Forms.ItemsControl.asPngToolStripMenuItem") ?? "As Png";

            // Status Bar
            NameLabel.Text = _localizationGetter("Forms.ItemsControl.NameLabel") ?? "Name:";
            GraphicLabel.Text = _localizationGetter("Forms.ItemsControl.GraphicLabel") ?? "Graphic:";

            // Re-apply DetailTextBox context menu localization
            DetailTextBox.ContextMenuStrip = null;
            DetailTextBox.AddBasicContextMenu(_localizationGetter);

            // Refresh detail if needed
            if (_selectedGraphicId >= 0)
            {
                UpdateDetail(_selectedGraphicId);
            }
        }
    }
}
