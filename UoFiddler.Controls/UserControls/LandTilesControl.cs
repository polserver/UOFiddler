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

        private const int _landTileMax = 0x4000;

        private static LandTilesControl _refMarker;
        private int _selectedGraphicId = -1;
        private readonly List<int> _tileList = new List<int>();
        private bool _showFreeSlots;

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
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            if (_refMarker._tileList.TrueForAll(t => t != graphic))
            {
                return false;
            }

            // we have to invalidate focus so it will scroll to item
            _refMarker.LandTilesTileView.FocusIndex = -1;
            _refMarker.SelectedGraphicId = graphic;

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

            Cursor.Current = Cursors.WaitCursor;
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
            }

            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void UpdateToolStripLabels(int graphic)
        {
            if (!IsLoaded)
            {
                return;
            }

            NameLabel.Text = $"Name: {TileData.LandTable[graphic].Name}";
            GraphicLabel.Text = string.Format("ID: 0x{0:X4} ({0})", graphic);
            FlagsLabel.Text = $"Flags: {TileData.LandTable[graphic].Flags}";
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
            if (!Art.IsValidLand(_selectedGraphicId))
            {
                return;
            }

            DialogResult result =
                        MessageBox.Show($"Are you sure to remove {_selectedGraphicId}", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Art.RemoveLand(_selectedGraphicId);
            ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

            if (!_showFreeSlots)
            {
                _tileList.Remove(_selectedGraphicId);
                LandTilesTileView.VirtualListSize = _tileList.Count;
                var moveToIndex = --_selectedGraphicId;
                SelectedGraphicId = moveToIndex <= 0 ? 0 : _selectedGraphicId; // TODO: get last index visible instead just curr -1
            }
            LandTilesTileView.Invalidate();

            Options.ChangedUltimaClass["Art"] = true;
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
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

                    Art.ReplaceLand(_selectedGraphicId, bitmap);

                    ControlEvents.FireLandTileChangeEvent(this, _selectedGraphicId);

                    LandTilesTileView.Invalidate();

                    Options.ChangedUltimaClass["Art"] = true;
                }
            }
        }

        private void OnTextChangedInsert(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, 0x3FFF))
            {
                InsertText.ForeColor = Art.IsValidLand(index) ? Color.Red : Color.Black;
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
            DialogResult result =
                        MessageBox.Show("Are you sure? Will take a while", "Save",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Art.Save(Options.OutputPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show(
                $"Saved to {Options.OutputPath}",
                "Save",
                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Art"] = false;
        }

        private void OnClickExportBmp(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
            {
                return;
            }

            ExportLandTileImage(_selectedGraphicId, ImageFormat.Bmp);
        }

        private void OnClickExportTiff(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
            {
                return;
            }

            ExportLandTileImage(_selectedGraphicId, ImageFormat.Tiff);
        }

        private void OnClickExportJpg(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
            {
                return;
            }

            ExportLandTileImage(_selectedGraphicId, ImageFormat.Jpeg);
        }

        private void OnClickExportPng(object sender, EventArgs e)
        {
            if (_selectedGraphicId < 0)
            {
                return;
            }

            ExportLandTileImage(_selectedGraphicId, ImageFormat.Png);
        }

        private static void ExportLandTileImage(int index, ImageFormat imageFormat)
        {
            if (!Art.IsValidLand(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Landtile 0x{index:X4}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Art.GetLand(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show($"Landtile saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClickSelectTiledata(object sender, EventArgs e)
        {
            if (_selectedGraphicId >= 0)
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
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                foreach (var index in _tileList)
                {
                    if (!Art.IsValidLand(index))
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Landtile 0x{index:X4}.{fileExtension}");
                    using (Bitmap bit = new Bitmap(Art.GetLand(index)))
                    {
                        bit.Save(fileName, imageFormat);
                    }
                }

                Cursor.Current = Cursors.Default;

                MessageBox.Show($"All land tiles saved to {dialog.SelectedPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void LandTilesTileView_DrawItem(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Point itemPoint = new Point(e.Bounds.X + LandTilesTileView.TilePadding.Left, e.Bounds.Y + LandTilesTileView.TilePadding.Top);
            const int fixedTileSize = 44;
            Size itemSize = new Size(fixedTileSize, fixedTileSize);
            Rectangle itemRec = new Rectangle(itemPoint, itemSize);

            var previousClip = e.Graphics.Clip;

            e.Graphics.Clip = new Region(itemRec);

            Bitmap bitmap = Art.GetLand(_tileList[e.Index], out bool patched);

            if (bitmap == null)
            {
                e.Graphics.Clip = new Region(itemRec);

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

                e.Graphics.Clip = previousClip;
            }
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

        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, false);
        }

        private void SearchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            SearchName(searchByNameToolStripTextBox.Text, true);
        }
    }
}
