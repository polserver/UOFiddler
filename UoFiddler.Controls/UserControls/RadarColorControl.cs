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
using Ultima.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.UserControls
{
    public partial class RadarColorControl : UserControl
    {
        public RadarColorControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;

            // TileViewControl runtime config (TileSize/Margin/Padding/Border) +
            // checkbox-toggle sync into the canonical _selectedItems/_selectedLand
            // HashSets.
            ConfigureTileView(tileViewItem);
            ConfigureTileView(tileViewLand);
            tileViewItem.SelectedIndices.CollectionChanged += OnItemSelectedIndicesChanged;
            tileViewLand.SelectedIndices.CollectionChanged += OnLandSelectedIndicesChanged;
        }

        private int _selectedIndex = -1;
        // Tracks whether _selectedIndex refers to an item or a land tile. Set
        // when _selectedIndex is updated; consulted by SaveColor so that
        // committing the editor on a tab switch routes to the right table.
        // (Otherwise SaveColor would see the *new* tab and save the previous
        // tab's color to whatever index happens to be in _selectedIndex.)
        private bool _selectedIsItem;
        private ushort _currentColor;
        private static RadarColorControl _refMarker;
        private bool _updating;
        private readonly Dictionary<int, ushort> _originalItemColors = [];
        private readonly Dictionary<int, ushort> _originalLandColors = [];
        private Timer _debounceTimer;
        private const int _debounceTimeout = 500;
        // Canonical "checked" backing store, keyed by graphic id so the
        // selection survives filter changes. Sync'd both ways with the
        // TileViewControl.SelectedIndices collection (which uses *row positions*).
        private readonly HashSet<int> _selectedItems = [];
        private readonly HashSet<int> _selectedLand = [];

        // Visible row position → graphic id. Default identity; ApplyFilter narrows.
        private int[] _itemIndices = Array.Empty<int>();
        private int[] _landIndices = Array.Empty<int>();

        // Re-entrancy guard: when we mutate TileViewControl.SelectedIndices
        // programmatically (e.g. during a filter rebuild or Select All), we
        // don't want the CollectionChanged handler to re-mutate our HashSet
        // and double-count.
        private bool _syncingSelection;

        private static int[] BuildIdentity(int length)
        {
            var array = new int[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = i;
            }
            return array;
        }

        // TileViewControl strips TileSize/Margin/Padding/BorderWidth in the
        // Designer (DesignerSerializationVisibility.Hidden), so we apply them
        // here. Matches the compare plugin's ConfigureTileView.
        private static void ConfigureTileView(TileView.TileViewControl tv)
        {
            tv.TileSize = new Size(tv.TileSize.Width, 20);
            tv.TileMargin = new Padding(0);
            tv.TilePadding = new Padding(0);
            tv.TileBorderWidth = 0f;
        }

        private void OnTileViewSizeChanged(object sender, EventArgs e)
        {
            var tv = (TileView.TileViewControl)sender;
            int w = tv.DisplayRectangle.Width;
            if (w > 0 && tv.TileSize.Width != w)
            {
                tv.TileSize = new Size(w, tv.TileSize.Height);
            }
        }

        private int GetSelectedItemGraphic()
        {
            int focus = tileViewItem.FocusIndex;
            return focus >= 0 && focus < _itemIndices.Length ? _itemIndices[focus] : -1;
        }

        private int GetSelectedLandGraphic()
        {
            int focus = tileViewLand.FocusIndex;
            return focus >= 0 && focus < _landIndices.Length ? _landIndices[focus] : -1;
        }

        private void OnDrawItemRow(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if ((uint)e.Index >= (uint)_itemIndices.Length)
            {
                return;
            }
            int graphic = _itemIndices[e.Index];
            ref readonly ItemData row = ref TileData.ItemTable[graphic];
            DrawRow(e, graphic, row.Name, _originalItemColors.ContainsKey(graphic), RadarCol.GetItemColor(graphic));
        }

        private void OnDrawLandRow(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if ((uint)e.Index >= (uint)_landIndices.Length)
            {
                return;
            }
            int graphic = _landIndices[e.Index];
            ref readonly LandData row = ref TileData.LandTable[graphic];
            DrawRow(e, graphic, row.Name, _originalLandColors.ContainsKey(graphic), RadarCol.GetLandColor(graphic));
        }

        // Layout (left → right): [checkbox column from TileViewControl] | [color swatch] | [text]
        private const int SwatchSize = 12;
        private const int SwatchGap = 4;

        private static void DrawRow(TileView.TileViewControl.DrawTileListItemEventArgs e, int graphic, string name, bool modified, ushort radarHue)
        {
            bool focused = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // Row background.
            if (focused)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            // Color swatch — a small filled rectangle showing this row's radar color.
            int swatchX = e.ContentLeft + 2;
            int swatchY = e.Bounds.Y + (e.Bounds.Height - SwatchSize) / 2;
            var swatchRect = new Rectangle(swatchX, swatchY, SwatchSize, SwatchSize);
            Color swatchColor = HueHelpers.HueToColor(radarHue);
            using (var swatchBrush = new SolidBrush(swatchColor))
            {
                e.Graphics.FillRectangle(swatchBrush, swatchRect);
            }
            using (var swatchBorder = new Pen(focused ? SystemColors.HighlightText : SystemColors.ControlDark))
            {
                e.Graphics.DrawRectangle(swatchBorder, swatchRect);
            }

            // Text starts after the swatch.
            Color textColor;
            if (focused)
            {
                textColor = SystemColors.HighlightText;
            }
            else if (modified)
            {
                textColor = Color.Blue;
            }
            else
            {
                textColor = Options.DarkMode ? Color.White : SystemColors.WindowText;
            }

            string text = $"0x{graphic:X4} ({graphic}) {name}";
            int textX = swatchX + SwatchSize + SwatchGap;
            float textY = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(text, e.Font).Height) / 2f;
            using var brush = new SolidBrush(textColor);
            e.Graphics.DrawString(text, e.Font, brush, new PointF(textX, textY));
        }

        private void OnItemFocusChanged(object sender, TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0 || e.FocusedItemIndex >= _itemIndices.Length)
            {
                return;
            }
            UpdateSelectedItemPreview(_itemIndices[e.FocusedItemIndex]);
        }

        private void OnLandFocusChanged(object sender, TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0 || e.FocusedItemIndex >= _landIndices.Length)
            {
                return;
            }
            UpdateSelectedLandPreview(_landIndices[e.FocusedItemIndex]);
        }

        private void OnItemSelectedIndicesChanged(object sender, IndicesCollection.NotifyCollectionChangedEventArgs e)
        {
            if (_syncingSelection || e.ItemsChanged == null)
            {
                return;
            }

            foreach (int row in e.ItemsChanged)
            {
                if ((uint)row >= (uint)_itemIndices.Length)
                {
                    continue;
                }
                int graphic = _itemIndices[row];
                if (e.Action == IndicesCollection.NotifyCollectionChangedAction.Add)
                {
                    _selectedItems.Add(graphic);
                }
                else
                {
                    _selectedItems.Remove(graphic);
                }
            }
        }

        private void OnLandSelectedIndicesChanged(object sender, IndicesCollection.NotifyCollectionChangedEventArgs e)
        {
            if (_syncingSelection || e.ItemsChanged == null)
            {
                return;
            }

            foreach (int row in e.ItemsChanged)
            {
                if ((uint)row >= (uint)_landIndices.Length)
                {
                    continue;
                }
                int graphic = _landIndices[row];
                if (e.Action == IndicesCollection.NotifyCollectionChangedAction.Add)
                {
                    _selectedLand.Add(graphic);
                }
                else
                {
                    _selectedLand.Remove(graphic);
                }
            }
        }

        private void RedrawItemRow(int graphic)
        {
            int pos = Array.IndexOf(_itemIndices, graphic);
            if (pos >= 0)
            {
                tileViewItem.RedrawItem(pos);
            }
        }

        private void RedrawLandRow(int graphic)
        {
            int pos = Array.IndexOf(_landIndices, graphic);
            if (pos >= 0)
            {
                tileViewLand.RedrawItem(pos);
            }
        }

        private void SelectItemRow(int rowPos)
        {
            if ((uint)rowPos < (uint)_itemIndices.Length)
            {
                tileViewItem.FocusIndex = rowPos;
            }
        }

        private void SelectLandRow(int rowPos)
        {
            if ((uint)rowPos < (uint)_landIndices.Length)
            {
                tileViewLand.FocusIndex = rowPos;
            }
        }

        // After _itemIndices/_landIndices change (reset or filter), the row
        // positions in SelectedIndices are stale. Rebuild them from the canonical
        // _selectedItems/_selectedLand HashSets. _syncingSelection prevents the
        // CollectionChanged handler from feeding the writes back into the HashSet.
        private void SyncItemSelectedIndicesFromHashSet()
        {
            _syncingSelection = true;
            try
            {
                tileViewItem.SelectedIndices.Clear();
                for (int i = 0; i < _itemIndices.Length; ++i)
                {
                    if (_selectedItems.Contains(_itemIndices[i]))
                    {
                        tileViewItem.SelectedIndices.Add(i);
                    }
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private void SyncLandSelectedIndicesFromHashSet()
        {
            _syncingSelection = true;
            try
            {
                tileViewLand.SelectedIndices.Clear();
                for (int i = 0; i < _landIndices.Length; ++i)
                {
                    if (_selectedLand.Contains(_landIndices[i]))
                    {
                        tileViewLand.SelectedIndices.Add(i);
                    }
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private void ResetItemView()
        {
            int total = TileData.ItemTable != null ? Art.GetMaxItemId() : 0;
            _itemIndices = BuildIdentity(total);
            tileViewItem.VirtualListSize = _itemIndices.Length;
            SyncItemSelectedIndicesFromHashSet();
            tileViewItem.Invalidate();
        }

        private void ResetLandView()
        {
            int total = TileData.LandTable?.Length ?? 0;
            _landIndices = BuildIdentity(total);
            tileViewLand.VirtualListSize = _landIndices.Length;
            SyncLandSelectedIndicesFromHashSet();
            tileViewLand.Invalidate();
        }

        public bool IsLoaded { get; private set; }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor == value)
                {
                    return;
                }

                _currentColor = value;
                _updating = true;
                numericUpDownShortCol.Value = _currentColor;
                Color color = HueHelpers.HueToColor(_currentColor);
                pictureBoxColor.BackColor = color;
                numericUpDownR.Value = color.R;
                numericUpDownG.Value = color.G;
                numericUpDownB.Value = color.B;
                _updating = false;
            }
        }

        public static void Select(int graphic, bool land)
        {
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            if (land)
            {
                int pos = Array.IndexOf(_refMarker._landIndices, graphic);
                if (pos < 0)
                {
                    // Filter may exclude the target — reset and retry so
                    // cross-tab navigation always lands on the row.
                    _refMarker.ResetLandView();
                    pos = Array.IndexOf(_refMarker._landIndices, graphic);
                }

                if (pos < 0)
                {
                    return;
                }

                _refMarker.tabControl2.SelectTab(1);
                _refMarker.SelectLandRow(pos);
            }
            else
            {
                int pos = Array.IndexOf(_refMarker._itemIndices, graphic);
                if (pos < 0)
                {
                    _refMarker.ResetItemView();
                    pos = Array.IndexOf(_refMarker._itemIndices, graphic);
                }

                if (pos < 0)
                {
                    return;
                }

                _refMarker.tabControl2.SelectTab(0);
                _refMarker.SelectItemRow(pos);
            }
        }

        private void Reload()
        {
            if (IsLoaded)
            {
                OnLoad(this, new MyEventArgs(MyEventArgs.Types.ForceReload));
            }
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

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            // Fresh data from disk — nothing is checked or dirty until the
            // user edits it again.
            _selectedItems.Clear();
            _selectedLand.Clear();
            _originalItemColors.Clear();
            _originalLandColors.Clear();
            _selectedIndex = -1;
            _selectedIsItem = false;

            ResetItemView();
            ResetLandView();

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;

                pictureBoxArt.BackColor = Options.PreviewBackgroundColor;
            }

            IsLoaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
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

        private void OnPreviewBackgroundColorChanged()
        {
            pictureBoxArt.BackColor = Options.PreviewBackgroundColor;

            if (_selectedIndex < 0)
            {
                return;
            }

            if (tabControl2.SelectedIndex == 0)
            {
                UpdateSelectedItemPreview(_selectedIndex);
            }
            else
            {
                UpdateSelectedLandPreview(_selectedIndex);
            }
        }

        private void UpdateSelectedItemPreview(int graphic)
        {
            SaveColor();

            _selectedIndex = graphic;
            _selectedIsItem = true;

            if (Art.IsValidStatic(_selectedIndex))
            {
                Bitmap bitmap = Art.GetStatic(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Options.PreviewBackgroundColor);
                    newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                }

                pictureBoxArt.Image = newBitmap;
            }
            else
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }

            CurrentColor = RadarCol.GetItemColor(_selectedIndex);

            buttonRevert.Enabled = _originalItemColors.ContainsKey(_selectedIndex);
            buttonRevertAll.Enabled = _originalLandColors.Count > 0 || _originalItemColors.Count > 0;
        }

        private void UpdateSelectedLandPreview(int graphic)
        {
            SaveColor();

            _selectedIndex = graphic;
            _selectedIsItem = false;

            if (Art.IsValidLand(_selectedIndex))
            {
                Bitmap bitmap = Art.GetLand(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Options.PreviewBackgroundColor);
                    newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                }

                pictureBoxArt.Image = newBitmap;
            }
            else
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }

            CurrentColor = RadarCol.GetLandColor(_selectedIndex);

            buttonRevert.Enabled = _originalLandColors.ContainsKey(_selectedIndex);
            buttonRevertAll.Enabled = _originalLandColors.Count > 0 || _originalItemColors.Count > 0;
        }

        private void OnClickMeanColor(object sender, EventArgs e)
        {
            Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(_selectedIndex) : Art.GetLand(_selectedIndex);
            if (image == null)
            {
                return;
            }

            CurrentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
        }

        private void OnClickSaveFile(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "radarcol.mul");
            RadarCol.Save(fileName);

            _originalItemColors.Clear();
            _originalLandColors.Clear();
            tileViewItem.Invalidate();
            tileViewLand.Invalidate();

            Options.ChangedUltimaClass["RadarCol"] = false;

            FileSavedDialog.Show(FindForm(), fileName, "RadarCol saved successfully.");
        }

        private void SaveColor()
        {
            // Use the tab the index originated from, NOT tabControl2.SelectedIndex.
            // Otherwise switching tabs and clicking a row in the new tab would
            // commit the editor's color to the previous tab's id, mis-attributed.
            SaveColor(_selectedIndex, CurrentColor, _selectedIsItem);
        }

        private void SaveColor(int index, ushort color, bool isItemTile)
        {
            if (index < 0)
            {
                return;
            }

            if (isItemTile)
            {
                var datafileColor = RadarCol.GetItemColor(index);
                if (color != datafileColor && _originalItemColors.TryAdd(index, datafileColor))
                {
                    RedrawItemRow(index);
                }
                RadarCol.SetItemColor(index, color);
            }
            else
            {
                var datafileColor = RadarCol.GetLandColor(index);
                if (color != datafileColor && _originalLandColors.TryAdd(index, datafileColor))
                {
                    RedrawLandRow(index);
                }
                RadarCol.SetLandColor(index, color);
            }
        }

        private void OnClickRevertAll(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to revert all changes to items and land tiles?",
                "Revert All",
                MessageBoxButtons.YesNo
                );

            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach (var (index, color) in _originalItemColors)
            {
                RadarCol.SetItemColor(index, color);
                if (index == _selectedIndex && tabControl2.SelectedIndex == 0)
                {
                    CurrentColor = color;
                }
            }

            foreach (var (index, color) in _originalLandColors)
            {
                RadarCol.SetLandColor(index, color);
                if (index == _selectedIndex && tabControl2.SelectedIndex == 1)
                {
                    CurrentColor = color;
                }
            }

            Options.ChangedUltimaClass["RadarCol"] = false;
            buttonRevertAll.Enabled = false;
            buttonRevert.Enabled = false;

            _originalItemColors.Clear();
            _originalLandColors.Clear();
            tileViewItem.Invalidate();
            tileViewLand.Invalidate();
        }

        private void OnClickRevert(object sender, EventArgs e)
        {
            if (_selectedIndex > -1)
            {
                if (tabControl2.SelectedIndex == 0)
                {
                    if (_originalItemColors.TryGetValue(_selectedIndex, out var color))
                    {
                        CurrentColor = color;
                        RadarCol.SetItemColor(_selectedIndex, color);
                        _originalItemColors.Remove(_selectedIndex);
                        RedrawItemRow(_selectedIndex);
                    }
                }
                else if (_originalLandColors.TryGetValue(_selectedIndex, out var color))
                {
                    CurrentColor = color;
                    RadarCol.SetLandColor(_selectedIndex, color);
                    _originalLandColors.Remove(_selectedIndex);
                    RedrawLandRow(_selectedIndex);
                }
            }

            buttonRevert.Enabled = false;

            if (_originalItemColors.Count == 0 && _originalLandColors.Count == 0)
            {
                Options.ChangedUltimaClass["RadarCol"] = false;
                buttonRevertAll.Enabled = false;
            }
        }

        private void OnClickSaveColor(object sender, EventArgs e)
        {
            SaveColor();
            if (tabControl2.SelectedIndex == 0)
            {
                if (_originalItemColors.ContainsKey(_selectedIndex))
                {
                    buttonRevert.Enabled = true;
                    buttonRevertAll.Enabled = true;
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }
            else if (_originalLandColors.ContainsKey(_selectedIndex))
            {
                buttonRevert.Enabled = true;
                buttonRevertAll.Enabled = true;
                Options.ChangedUltimaClass["RadarCol"] = true;
            }
        }

        private void OnClickSetRangeFrom(object sender, EventArgs e)
        {
            int graphic = GetGraphicFromContextSource(sender);
            if (graphic >= 0)
            {
                textBoxMeanFrom.Text = graphic.ToString();
            }
        }

        private void OnClickSetRangeTo(object sender, EventArgs e)
        {
            int graphic = GetGraphicFromContextSource(sender);
            if (graphic >= 0)
            {
                textBoxMeanTo.Text = graphic.ToString();
            }
        }

        private int GetGraphicFromContextSource(object sender)
        {
            var tv = ((ContextMenuStrip)((ToolStripItem)sender).Owner).SourceControl as TileView.TileViewControl;
            if (tv == null || tv.FocusIndex < 0)
            {
                return -1;
            }
            var indices = tv == tileViewItem ? _itemIndices : _landIndices;
            if (tv.FocusIndex >= indices.Length)
            {
                return -1;
            }
            return indices[tv.FocusIndex];
        }

        private void OnChangeR(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnChangeG(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnChangeB(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnNumericShortColChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                CurrentColor = (ushort)numericUpDownShortCol.Value;
            }
        }

        private IEnumerable<int> GetValidSequence()
        {
            var isItem = tabControl2.SelectedIndex == 0;

            if (radioUseRange.Checked)
            {
                var maxIndex = isItem ? Art.GetMaxItemId() : 0x3FFF;

                if (!Utils.ConvertStringToInt(textBoxMeanFrom.Text, out int from, 0, maxIndex) ||
                    !Utils.ConvertStringToInt(textBoxMeanTo.Text, out int to, 0, maxIndex))
                {
                    MessageBox.Show($"Invalid parameters. Expected [to, from] between [0, {maxIndex} (0x{maxIndex:X4})]", "Error", MessageBoxButtons.OK);
                    return null;
                }

                if (from > to)
                {
                    (from, to) = (to, from);
                }

                return Enumerable.Range(from, to - from + 1);
            }
            else
            {
                var sequence = isItem ? _selectedItems : _selectedLand;
                if (sequence.Count == 0)
                {
                    MessageBox.Show("Invalid parameters. No tiles selected/checked.", "Error", MessageBoxButtons.OK);
                    return null;
                }
                return sequence;
            }
        }

        private ushort GetSequenceAverage(IEnumerable<int> sequence)
        {
            int gmeanr = 0;
            int gmeang = 0;
            int gmeanb = 0;

            foreach (int i in sequence)
            {
                Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(i) : Art.GetLand(i);
                if (image == null)
                {
                    continue;
                }

                unsafe
                {
                    BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                    ushort* line = (ushort*)bd.Scan0;
                    int delta = bd.Stride >> 1;
                    ushort* cur = line;

                    int meanr = 0;
                    int meang = 0;
                    int meanb = 0;

                    int count = 0;
                    for (int y = 0; y < image.Height; ++y, line += delta)
                    {
                        cur = line;
                        for (int x = 0; x < image.Width; ++x)
                        {
                            if (cur[x] != 0)
                            {
                                meanr += HueHelpers.HueToColorR(cur[x]);
                                meang += HueHelpers.HueToColorG(cur[x]);
                                meanb += HueHelpers.HueToColorB(cur[x]);
                                ++count;
                            }
                        }
                    }
                    image.UnlockBits(bd);

                    meanr /= count;
                    meang /= count;
                    meanb /= count;

                    gmeanr += meanr;
                    gmeang += meang;
                    gmeanb += meanb;
                }
            }

            var diff = sequence.Count();

            if (diff > 0)
            {

                gmeanr /= diff;
                gmeang /= diff;
                gmeanb /= diff;
            }

            Color col = Color.FromArgb(gmeanr, gmeang, gmeanb);
            return HueHelpers.ColorToHue(col);
        }

        private void OnClickCurrentToRangeAverage(object sender, EventArgs e)
        {
            var sequence = GetValidSequence();

            if (sequence == null)
            {
                return;
            }

            CurrentColor = GetSequenceAverage(sequence);
            SaveColor();

            var isItemTile = tabControl2.SelectedIndex == 0;
            var enableRevert = isItemTile ? _originalItemColors.ContainsKey(_selectedIndex) : _originalLandColors.ContainsKey(_selectedIndex);

            buttonRevert.Enabled = enableRevert;
            buttonRevertAll.Enabled |= enableRevert;
            Options.ChangedUltimaClass["RadarCol"] |= enableRevert;
        }

        private void OnClickRangeToRangeAverage(object sender, EventArgs e)
        {
            var sequence = GetValidSequence();

            if (sequence == null)
            {
                return;
            }

            var color = GetSequenceAverage(sequence);
            var isItemTile = tabControl2.SelectedIndex == 0;
            bool enableRevertAll = false;

            foreach (int i in sequence)
            {
                SaveColor(i, color, isItemTile);

                var enableRevert = isItemTile ? _originalItemColors.ContainsKey(i) : _originalLandColors.ContainsKey(i);

                if (i == _selectedIndex)
                {
                    CurrentColor = color;
                    buttonRevert.Enabled = enableRevert;
                }

                enableRevertAll |= enableRevert;
            }

            if (enableRevertAll)
            {
                buttonRevertAll.Enabled = true;
                Options.ChangedUltimaClass["RadarCol"] = true;
            }
        }

        private void OnClickRangeToIndividualAverage(object sender, EventArgs e)
        {
            var sequence = GetValidSequence();

            if (sequence == null)
            {
                return;
            }

            var isItemTile = tabControl2.SelectedIndex == 0;
            bool enableRevertAll = false;

            foreach (int i in sequence)
            {
                Bitmap image = isItemTile ? Art.GetStatic(i) : Art.GetLand(i);
                if (image == null)
                {
                    continue;
                }

                var color = HueHelpers.ColorToHue(AverageColorFrom(image));

                SaveColor(i, color, isItemTile);

                var enableRevert = isItemTile ? _originalItemColors.ContainsKey(i) : _originalLandColors.ContainsKey(i);

                if (i == _selectedIndex)
                {
                    CurrentColor = color;
                    buttonRevert.Enabled = enableRevert;
                }

                enableRevertAll |= enableRevert;
            }

            if (enableRevertAll)
            {
                buttonRevertAll.Enabled = true;
                Options.ChangedUltimaClass["RadarCol"] = true;
            }
        }

        private void OnClickSelectItemsTab(object sender, EventArgs e)
        {
            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            var found = ItemsControl.SearchGraphic(index);
            if (!found)
            {
                MessageBox.Show("You need to load Items tab first.", "Information");
            }
        }

        private void OnClickSelectItemTiledataTab(object sender, EventArgs e)
        {
            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            TileDataControl.Select(index, false);
        }

        private void OnClickSelectLandTilesTab(object sender, EventArgs e)
        {
            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            var found = LandTilesControl.SearchGraphic(index);
            if (!found)
            {
                MessageBox.Show("You need to load LandTiles tab first.", "Information");
            }
        }

        private void OnClickSelectLandTiledataTab(object sender, EventArgs e)
        {
            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            TileDataControl.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["RadarCol"] = true;
                RadarCol.ImportFromCSV(dialog.FileName);
                if (tabControl2.SelectedTab == tabControl2.TabPages[0])
                {
                    int graphic = GetSelectedItemGraphic();
                    if (graphic >= 0)
                    {
                        UpdateSelectedItemPreview(graphic);
                    }
                }
                else
                {
                    int graphic = GetSelectedLandGraphic();
                    if (graphic >= 0)
                    {
                        UpdateSelectedLandPreview(graphic);
                    }
                }
            }
            dialog.Dispose();
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "RadarColor.csv");
            RadarCol.ExportToCSV(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "RadarColor saved successfully.");
        }

        private void OnClickMeanColorAll(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to calculate and set new radar color values for all items and land tiles entries where current color is black or missing?",
                "Average All",
                MessageBoxButtons.YesNo
                );

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (TileData.ItemTable != null)
            {
                int itemsLength = Art.GetMaxItemId();
                progressBar1.Maximum = itemsLength;

                for (int i = 0; i < itemsLength; ++i)
                {
                    progressBar1.Value++;
                    if (!Art.IsValidStatic(i))
                    {
                        continue;
                    }

                    if (RadarCol.GetItemColor(i) != 0)
                    {
                        continue;
                    }

                    Bitmap image = Art.GetStatic(i);
                    if (image == null)
                    {
                        continue;
                    }

                    var currentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
                    RadarCol.SetItemColor(i, currentColor);
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }

            if (TileData.LandTable != null)
            {
                int landLength = TileData.LandTable.Length;
                progressBar2.Maximum = landLength;
                for (int i = 0; i < landLength; ++i)
                {
                    progressBar2.Value++;
                    if (!Art.IsValidLand(i))
                    {
                        continue;
                    }

                    if (RadarCol.GetLandColor(i) != 0)
                    {
                        continue;
                    }

                    Bitmap image = Art.GetLand(i);
                    if (image == null)
                    {
                        continue;
                    }

                    var currentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
                    RadarCol.SetLandColor(i, currentColor);
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }

            MessageBox.Show("Done!", "Average All");

            progressBar1.Value = 0;
            progressBar2.Value = 0;
        }

        private unsafe Color AverageColorFrom(Bitmap image)
        {
            BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            ushort* cur = line;

            int meanR = 0;
            int meanG = 0;
            int meanB = 0;

            int count = 0;
            for (int y = 0; y < image.Height; ++y, line += delta)
            {
                cur = line;
                for (int x = 0; x < image.Width; ++x)
                {
                    if (cur[x] == 0)
                    {
                        continue;
                    }

                    meanR += HueHelpers.HueToColorR(cur[x]);
                    meanG += HueHelpers.HueToColorG(cur[x]);
                    meanB += HueHelpers.HueToColorB(cur[x]);
                    ++count;
                }
            }
            image.UnlockBits(bd);

            if (count > 0)
            {
                meanR /= count;
                meanG /= count;
                meanB /= count;
            }

            return Color.FromArgb(meanR, meanG, meanB);
        }

        private void FilterChange(TextBox control, Action<string> filterCallback)
        {
            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
            }

            _debounceTimer = new Timer
            {
                Interval = _debounceTimeout
            };

            _debounceTimer.Tick += delegate (object sender, EventArgs args)
            {
                Invoke(() =>
                {
                    filterCallback(control.Text);
                });
                _debounceTimer.Stop();
            };

            _debounceTimer.Start();
        }

        private void OnTextChangedFilterLand(object sender, EventArgs e)
        {
            FilterChange(textFilterLand, FilterLand);
        }

        private void OnTextChangedFilterItems(object sender, EventArgs e)
        {
            FilterChange(textFilterItems, FilterItems);
        }

        private void FilterLand(string filterText)
        {
            int max = TileData.LandTable?.Length ?? 0;
            var matches = new List<int>(max);
            for (int i = 0; i < max; ++i)
            {
                string name = TileData.LandTable[i].Name;
                if (name.ContainsCaseInsensitive(filterText))
                {
                    matches.Add(i);
                }
            }
            _landIndices = matches.ToArray();
            tileViewLand.VirtualListSize = _landIndices.Length;
            SyncLandSelectedIndicesFromHashSet();
            tileViewLand.Invalidate();
        }

        private void FilterItems(string filterText)
        {
            int max = TileData.ItemTable != null ? Art.GetMaxItemId() : 0;
            var matches = new List<int>(max);
            for (int i = 0; i < max; ++i)
            {
                string name = TileData.ItemTable[i].Name;
                if (name.ContainsCaseInsensitive(filterText))
                {
                    matches.Add(i);
                }
            }
            _itemIndices = matches.ToArray();
            tileViewItem.VirtualListSize = _itemIndices.Length;
            SyncItemSelectedIndicesFromHashSet();
            tileViewItem.Invalidate();
        }

        private void SetAllCheckedItems(bool isChecked)
        {
            if (isChecked)
            {
                foreach (int graphic in _itemIndices)
                {
                    _selectedItems.Add(graphic);
                }
            }
            else
            {
                foreach (int graphic in _itemIndices)
                {
                    _selectedItems.Remove(graphic);
                }
            }
            SyncItemSelectedIndicesFromHashSet();
            tileViewItem.Invalidate();
        }

        private void SetAllCheckedLand(bool isChecked)
        {
            if (isChecked)
            {
                foreach (int graphic in _landIndices)
                {
                    _selectedLand.Add(graphic);
                }
            }
            else
            {
                foreach (int graphic in _landIndices)
                {
                    _selectedLand.Remove(graphic);
                }
            }
            SyncLandSelectedIndicesFromHashSet();
            tileViewLand.Invalidate();
        }

        private void OnClickSelectAllItems(object sender, EventArgs e)
        {
            SetAllCheckedItems(true);
        }

        private void OnClickSelectNoneItems(object sender, EventArgs e)
        {
            SetAllCheckedItems(false);
        }

        private void OnCheckedChangeUseSelection(object sender, EventArgs e)
        {
            textBoxMeanFrom.Enabled = false;
            textBoxMeanTo.Enabled = false;
            buttonRangeToRangeAverage.Text = "Selected tiles to selection average";
            buttonRangeToIndividualAverage.Text = "Selected tiles to individual average";
            buttonCurrentToRangeAverage.Text = "Current tile to selection average";
        }

        private void OnCheckedChangeUseRange(object sender, EventArgs e)
        {
            textBoxMeanFrom.Enabled = true;
            textBoxMeanTo.Enabled = true;
            buttonRangeToRangeAverage.Text = "Range tiles to range average";
            buttonRangeToIndividualAverage.Text = "Range tiles to individual average";
            buttonCurrentToRangeAverage.Text = "Current tile to range average";
        }

        private void OnClickSelectAllLand(object sender, EventArgs e)
        {
            SetAllCheckedLand(true);
        }

        private void OnClickSelectNoneLand(object sender, EventArgs e)
        {
            SetAllCheckedLand(false);
        }
    }
}
