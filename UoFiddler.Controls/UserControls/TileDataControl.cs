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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class TileDataControl : UserControl
    {
        public TileDataControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            AssignToolTipsToLabels();

            _refMarker = this;

            saveDirectlyOnChangesToolStripMenuItem.Checked = Options.TileDataDirectlySaveOnChange;
            saveDirectlyOnChangesToolStripMenuItem.CheckedChanged += SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged;

            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            ControlEvents.TileDataChangeEvent += OnTileDataChangeEvent;
            ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;

            pictureBoxItem.BackColor = Options.PreviewBackgroundColor;
        }

        private void InitLandTilesFlagsCheckBoxes()
        {
            checkedListBox2.BeginUpdate();
            try
            {
                checkedListBox2.Items.Clear();

                string[] enumNames = Enum.GetNames(typeof(TileFlag));
                int maxLength = Art.IsUOAHS() ? enumNames.Length : (enumNames.Length / 2) + 1;
                for (int i = 1; i < maxLength; ++i)
                {
                    checkedListBox2.Items.Add(enumNames[i], false);
                }

                // TODO: for now we present all flags. Needs research if landtiles have only selected flags or all of them?
                // TODO: looks like only small subset is used but it is still different then these 5 below
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Damaging), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Wet), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Impassable), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.Wall), false);
                //checkedListBox2.Items.Add(Enum.GetName(typeof(TileFlag), TileFlag.NoDiagonal), false);
            }
            finally
            {
                checkedListBox2.EndUpdate();
            }
        }

        private void InitItemsFlagsCheckBoxes()
        {
            checkedListBox1.BeginUpdate();
            try
            {
                checkedListBox1.Items.Clear();

                string[] enumNames = Enum.GetNames(typeof(TileFlag));
                int maxLength = Art.IsUOAHS() ? enumNames.Length : (enumNames.Length / 2) + 1;
                for (int i = 1; i < maxLength; ++i)
                {
                    checkedListBox1.Items.Add(enumNames[i], false);
                }
            }
            finally
            {
                checkedListBox1.EndUpdate();
            }
        }

        private static TileDataControl _refMarker;
        private bool _changingIndex;

        // Virtual ListView backing state. _itemIndices/_landIndices map each
        // visible row position to the real graphic id; default identity, narrowed
        // by ApplyFilterItem/ApplyFilterLand. _modifiedItems/_modifiedLand hold
        // graphic ids the user has edited in this session and should render in
        // the modified color (formerly SelectedNode.ForeColor = Red).
        private int[] _itemIndices = Array.Empty<int>();
        private int[] _landIndices = Array.Empty<int>();
        private readonly HashSet<int> _modifiedItems = new HashSet<int>();
        private readonly HashSet<int> _modifiedLand = new HashSet<int>();

        private static Color ModifiedColor => Options.DarkMode ? Color.OrangeRed : Color.Red;

        private int GetSelectedItemGraphic()
        {
            return listViewItem.SelectedIndices.Count > 0
                ? _itemIndices[listViewItem.SelectedIndices[0]]
                : -1;
        }

        private int GetSelectedLandGraphic()
        {
            return listViewLand.SelectedIndices.Count > 0
                ? _landIndices[listViewLand.SelectedIndices[0]]
                : -1;
        }

        private static string FormatItemRow(int graphic, string name)
        {
            return string.Create(null, stackalloc char[64], $"0x{graphic:X4} ({graphic}) {name}");
        }

        private static string FormatLandRow(int graphic, string name)
        {
            return string.Create(null, stackalloc char[64], $"0x{graphic:X4} ({graphic}) {name}");
        }

        private void OnRetrieveItemVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if ((uint)e.ItemIndex >= (uint)_itemIndices.Length)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            int graphic = _itemIndices[e.ItemIndex];
            string name = TileData.ItemTable[graphic].Name ?? string.Empty;
            var item = new ListViewItem(FormatItemRow(graphic, name)) { Tag = graphic };
            if (_modifiedItems.Contains(graphic))
            {
                item.ForeColor = ModifiedColor;
            }
            e.Item = item;
        }

        private void OnRetrieveLandVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if ((uint)e.ItemIndex >= (uint)_landIndices.Length)
            {
                e.Item = new ListViewItem(string.Empty);
                return;
            }

            int graphic = _landIndices[e.ItemIndex];
            string name = TileData.LandTable[graphic].Name ?? string.Empty;
            var item = new ListViewItem(FormatLandRow(graphic, name)) { Tag = graphic };
            if (_modifiedLand.Contains(graphic))
            {
                item.ForeColor = ModifiedColor;
            }
            e.Item = item;
        }

        private void RedrawItemRow(int graphic)
        {
            int pos = Array.IndexOf(_itemIndices, graphic);
            if (pos >= 0)
            {
                listViewItem.RedrawItems(pos, pos, false);
            }
        }

        private void RedrawLandRow(int graphic)
        {
            int pos = Array.IndexOf(_landIndices, graphic);
            if (pos >= 0)
            {
                listViewLand.RedrawItems(pos, pos, false);
            }
        }

        private void MarkItemModified(int graphic)
        {
            _modifiedItems.Add(graphic);
            RedrawItemRow(graphic);
        }

        private void MarkLandModified(int graphic)
        {
            _modifiedLand.Add(graphic);
            RedrawLandRow(graphic);
        }

        private void SelectItemRow(int rowPos)
        {
            listViewItem.SelectedIndices.Clear();
            if ((uint)rowPos < (uint)_itemIndices.Length)
            {
                listViewItem.SelectedIndices.Add(rowPos);
                listViewItem.EnsureVisible(rowPos);
                listViewItem.FocusedItem = listViewItem.Items[rowPos];
            }
        }

        private void SelectLandRow(int rowPos)
        {
            listViewLand.SelectedIndices.Clear();
            if ((uint)rowPos < (uint)_landIndices.Length)
            {
                listViewLand.SelectedIndices.Add(rowPos);
                listViewLand.EnsureVisible(rowPos);
                listViewLand.FocusedItem = listViewLand.Items[rowPos];
            }
        }

        private static int[] BuildIdentity(int length)
        {
            var array = new int[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = i;
            }
            return array;
        }

        public bool IsLoaded { get; private set; }

        public static void Select(int graphic, bool land)
        {
            if (_refMarker == null)
            {
                return;
            }

            // Activate the outer TileData TabPage so the virtual ListView is on
            // a visible tab before we set selection — assigning SelectedIndices
            // on a VirtualMode ListView whose parent TabPage hasn't been shown
            // does not stick across the later tab activation.
            TabPageNavigator.ActivateOwningTabPage(_refMarker);

            if (_refMarker.IsHandleCreated)
            {
                _refMarker.BeginInvoke(new Action(() => SearchGraphic(graphic, land)));
            }
            else
            {
                SearchGraphic(graphic, land);
            }
        }

        public static bool SearchGraphic(int graphic, bool land)
        {
            if (land)
            {
                int pos = Array.IndexOf(_refMarker._landIndices, graphic);
                if (pos < 0)
                {
                    // Filter may have excluded the target — reset and retry so
                    // cross-tab "Select in TileData" navigation always lands.
                    _refMarker.ResetLandView();
                    pos = Array.IndexOf(_refMarker._landIndices, graphic);
                }

                if (pos < 0)
                {
                    return false;
                }

                _refMarker.tabcontrol.SelectTab(1);
                _refMarker.SelectLandRow(pos);
                return true;
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
                    return false;
                }

                _refMarker.tabcontrol.SelectTab(0);
                _refMarker.SelectItemRow(pos);
                return true;
            }
        }

        private void ResetItemView()
        {
            int total = TileData.ItemTable?.Length ?? 0;
            _itemIndices = BuildIdentity(total);
            listViewItem.VirtualListSize = total;
            listViewItem.Invalidate();
        }

        private void ResetLandView()
        {
            int total = TileData.LandTable?.Length ?? 0;
            _landIndices = BuildIdentity(total);
            listViewLand.VirtualListSize = total;
            listViewLand.Invalidate();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
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
                        SearchName(searchByNameToolStripTextBox.Text, true, tabcontrol.SelectedIndex != 0);
                    }
                    else
                    {
                        SearchNamePrevious(searchByNameToolStripTextBox.Text, tabcontrol.SelectedIndex != 0);
                    }
                }
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public static bool SearchName(string name, bool next, bool land)
        {
            var searchMethod = SearchHelper.GetSearchMethod();
            var indices = land ? _refMarker._landIndices : _refMarker._itemIndices;
            var listView = land ? _refMarker.listViewLand : _refMarker.listViewItem;

            int start = 0;
            if (next && listView.SelectedIndices.Count > 0)
            {
                start = listView.SelectedIndices[0] + 1;
                if (start >= indices.Length)
                {
                    start = 0;
                }
            }

            for (int i = start; i < indices.Length; ++i)
            {
                int graphic = indices[i];
                string candidate = land
                    ? TileData.LandTable[graphic].Name
                    : TileData.ItemTable[graphic].Name;
                if (!searchMethod(name, candidate).EntryFound)
                {
                    continue;
                }

                _refMarker.tabcontrol.SelectTab(land ? 1 : 0);
                if (land)
                {
                    _refMarker.SelectLandRow(i);
                }
                else
                {
                    _refMarker.SelectItemRow(i);
                }
                return true;
            }

            return false;
        }

        public static bool SearchNamePrevious(string name, bool land)
        {
            var searchMethod = SearchHelper.GetSearchMethod();
            var indices = land ? _refMarker._landIndices : _refMarker._itemIndices;
            var listView = land ? _refMarker.listViewLand : _refMarker.listViewItem;

            int start = indices.Length - 1;
            if (listView.SelectedIndices.Count > 0)
            {
                start = listView.SelectedIndices[0] - 1;
                if (start < 0)
                {
                    start = indices.Length - 1;
                }
            }

            for (int i = start; i >= 0; --i)
            {
                int graphic = indices[i];
                string candidate = land
                    ? TileData.LandTable[graphic].Name
                    : TileData.ItemTable[graphic].Name;
                if (!searchMethod(name, candidate).EntryFound)
                {
                    continue;
                }

                _refMarker.tabcontrol.SelectTab(land ? 1 : 0);
                if (land)
                {
                    _refMarker.SelectLandRow(i);
                }
                else
                {
                    _refMarker.SelectItemRow(i);
                }
                return true;
            }

            return false;
        }

        public void ApplyFilterItem(ItemData item)
        {
            int total = TileData.ItemTable?.Length ?? 0;
            var matches = new List<int>(total);
            for (int i = 0; i < total; ++i)
            {
                ref readonly ItemData row = ref TileData.ItemTable[i];

                if (!string.IsNullOrEmpty(item.Name) && row.Name.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
                if (item.Animation != 0 && row.Animation != item.Animation)
                {
                    continue;
                }
                if (item.Weight != 0 && row.Weight != item.Weight)
                {
                    continue;
                }
                if (item.Quality != 0 && row.Quality != item.Quality)
                {
                    continue;
                }
                if (item.Quantity != 0 && row.Quantity != item.Quantity)
                {
                    continue;
                }
                if (item.Hue != 0 && row.Hue != item.Hue)
                {
                    continue;
                }
                if (item.StackingOffset != 0 && row.StackingOffset != item.StackingOffset)
                {
                    continue;
                }
                if (item.Value != 0 && row.Value != item.Value)
                {
                    continue;
                }
                if (item.Height != 0 && row.Height != item.Height)
                {
                    continue;
                }
                if (item.MiscData != 0 && row.MiscData != item.MiscData)
                {
                    continue;
                }
                if (item.Unk2 != 0 && row.Unk2 != item.Unk2)
                {
                    continue;
                }
                if (item.Unk3 != 0 && row.Unk3 != item.Unk3)
                {
                    continue;
                }
                if (item.Flags != 0 && (row.Flags & item.Flags) == 0)
                {
                    continue;
                }

                matches.Add(i);
            }

            _itemIndices = matches.ToArray();
            listViewItem.VirtualListSize = _itemIndices.Length;
            listViewItem.Invalidate();

            if (_itemIndices.Length > 0)
            {
                SelectItemRow(0);
            }
        }

        public static void ApplyFilterLand(LandData land)
        {
            int total = TileData.LandTable?.Length ?? 0;
            var matches = new List<int>(total);
            for (int i = 0; i < total; ++i)
            {
                ref readonly LandData row = ref TileData.LandTable[i];

                if (!string.IsNullOrEmpty(land.Name) && row.Name.IndexOf(land.Name, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }
                if (land.TextureId != 0 && row.TextureId != land.TextureId)
                {
                    continue;
                }
                if (land.Flags != 0 && (row.Flags & land.Flags) == 0)
                {
                    continue;
                }

                matches.Add(i);
            }

            _refMarker._landIndices = matches.ToArray();
            _refMarker.listViewLand.VirtualListSize = _refMarker._landIndices.Length;
            _refMarker.listViewLand.Invalidate();

            if (_refMarker._landIndices.Length > 0)
            {
                _refMarker.SelectLandRow(0);
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

            InitItemsFlagsCheckBoxes();
            InitLandTilesFlagsCheckBoxes();

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;

            // Reset modification markers on full (re)load — the data is fresh
            // from disk, so nothing is dirty until the user edits it again.
            _modifiedItems.Clear();
            _modifiedLand.Clear();

            ResetItemView();
            ResetLandView();

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
            pictureBoxItem.BackColor = Options.PreviewBackgroundColor;
            pictureBoxLand.BackColor = Options.PreviewBackgroundColor;

            int itemGraphic = GetSelectedItemGraphic();
            if (itemGraphic >= 0)
            {
                UpdateSelectedItemPreview(itemGraphic);
            }

            int landGraphic = GetSelectedLandGraphic();
            if (landGraphic >= 0)
            {
                UpdateSelectedLandPreview(landGraphic);
            }
        }

        private void OnTileDataChangeEvent(object sender, int index)
        {
            if (!IsLoaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (index > 0x3FFF) // items
            {
                int graphic = index - 0x4000;
                MarkItemModified(graphic);
                if (GetSelectedItemGraphic() == graphic)
                {
                    UpdateSelectedItemPreview(graphic);
                }
            }
            else
            {
                MarkLandModified(index);
                if (GetSelectedLandGraphic() == index)
                {
                    UpdateSelectedLandPreview(index);
                }
            }
        }

        private void OnItemSelectedIndexChanged(object sender, EventArgs e)
        {
            int graphic = GetSelectedItemGraphic();
            if (graphic < 0)
            {
                return;
            }

            UpdateSelectedItemPreview(graphic);
        }

        private void OnLandSelectedIndexChanged(object sender, EventArgs e)
        {
            int graphic = GetSelectedLandGraphic();
            if (graphic < 0)
            {
                return;
            }

            UpdateSelectedLandPreview(graphic);
        }

        private void UpdateSelectedItemPreview(int index)
        {
            Bitmap bit = Art.GetStatic(index);
            if (bit != null)
            {
                Bitmap newBit = new Bitmap(pictureBoxItem.Size.Width, pictureBoxItem.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(Options.PreviewBackgroundColor);
                    newGraph.DrawImage(bit, (pictureBoxItem.Size.Width - bit.Width) / 2, 1);
                }

                pictureBoxItem.Image?.Dispose();
                pictureBoxItem.Image = newBit;
            }
            else
            {
                pictureBoxItem.Image = null;
            }

            ItemData data = TileData.ItemTable[index];
            _changingIndex = true;
            textBoxName.Text = data.Name;
            textBoxAnim.Text = data.Animation.ToString();
            textBoxWeight.Text = data.Weight.ToString();
            textBoxQuality.Text = data.Quality.ToString();
            textBoxQuantity.Text = data.Quantity.ToString();
            textBoxHue.Text = data.Hue.ToString();
            textBoxStackOff.Text = data.StackingOffset.ToString();
            textBoxValue.Text = data.Value.ToString();
            textBoxHeigth.Text = data.Height.ToString();
            textBoxUnk1.Text = data.MiscData.ToString();
            textBoxUnk2.Text = data.Unk2.ToString();
            textBoxUnk3.Text = data.Unk3.ToString();

            Array enumValues = Enum.GetValues(typeof(TileFlag));
            int maxLength = Art.IsUOAHS() ? enumValues.Length : (enumValues.Length / 2) + 1;
            for (int i = 1; i < maxLength; ++i)
            {
                checkedListBox1.SetItemChecked(i - 1, (data.Flags & (TileFlag)enumValues.GetValue(i)) != 0);
            }
            _changingIndex = false;
        }

        private void UpdateSelectedLandPreview(int index)
        {
            Bitmap bit = Art.GetLand(index);
            if (bit != null)
            {
                Bitmap newBit = new Bitmap(pictureBoxLand.Size.Width, pictureBoxLand.Size.Height);
                using (Graphics newGraph = Graphics.FromImage(newBit))
                {
                    newGraph.Clear(Options.PreviewBackgroundColor);
                    newGraph.DrawImage(bit, (pictureBoxLand.Size.Width - bit.Width) / 2, 1);
                }

                pictureBoxLand.Image?.Dispose();
                pictureBoxLand.Image = newBit;
            }
            else
            {
                pictureBoxLand.Image = null;
            }

            LandData data = TileData.LandTable[index];
            _changingIndex = true;
            textBoxNameLand.Text = data.Name;
            textBoxTexID.Text = data.TextureId.ToString();

            Array enumValues = Enum.GetValues(typeof(TileFlag));
            int maxLength = Art.IsUOAHS() ? enumValues.Length : (enumValues.Length / 2) + 1;
            for (int i = 1; i < maxLength; ++i)
            {
                checkedListBox2.SetItemChecked(i - 1, (data.Flags & (TileFlag)enumValues.GetValue(i)) != 0);
            }

            _changingIndex = false;
        }

        private void OnClickSaveTiledata(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, "tiledata.mul");
            TileData.SaveTileData(fileName);
            Options.ChangedUltimaClass["TileData"] = false;
            FileSavedDialog.Show(FindForm(), fileName, "TileData saved successfully.");
        }

        private void OnClickSaveChanges(object sender, EventArgs e)
        {
            if (tabcontrol.SelectedIndex == 0) // items
            {
                int index = GetSelectedItemGraphic();
                if (index < 0)
                {
                    return;
                }

                ItemData item = TileData.ItemTable[index];
                string name = textBoxName.Text;
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }

                item.Name = name;
                if (short.TryParse(textBoxAnim.Text, out short shortRes))
                {
                    item.Animation = shortRes;
                }

                if (byte.TryParse(textBoxWeight.Text, out byte byteRes))
                {
                    item.Weight = byteRes;
                }

                if (byte.TryParse(textBoxQuality.Text, out byteRes))
                {
                    item.Quality = byteRes;
                }

                if (byte.TryParse(textBoxQuantity.Text, out byteRes))
                {
                    item.Quantity = byteRes;
                }

                if (byte.TryParse(textBoxHue.Text, out byteRes))
                {
                    item.Hue = byteRes;
                }

                if (byte.TryParse(textBoxStackOff.Text, out byteRes))
                {
                    item.StackingOffset = byteRes;
                }

                if (byte.TryParse(textBoxValue.Text, out byteRes))
                {
                    item.Value = byteRes;
                }

                if (byte.TryParse(textBoxHeigth.Text, out byteRes))
                {
                    item.Height = byteRes;
                }

                if (short.TryParse(textBoxUnk1.Text, out shortRes))
                {
                    item.MiscData = shortRes;
                }

                if (byte.TryParse(textBoxUnk2.Text, out byteRes))
                {
                    item.Unk2 = byteRes;
                }

                if (byte.TryParse(textBoxUnk3.Text, out byteRes))
                {
                    item.Unk3 = byteRes;
                }

                item.Flags = TileFlag.None;
                Array enumValues = Enum.GetValues(typeof(TileFlag));
                for (int i = 0; i < checkedListBox1.Items.Count; ++i)
                {
                    if (checkedListBox1.GetItemChecked(i))
                    {
                        item.Flags |= (TileFlag)enumValues.GetValue(i + 1);
                    }
                }

                TileData.ItemTable[index] = item;
                MarkItemModified(index);
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
                if (memorySaveWarningToolStripMenuItem.Checked)
                {
                    MessageBox.Show(
                        string.Format(
                            "Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index),
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
            else // land
            {
                int index = GetSelectedLandGraphic();
                if (index < 0)
                {
                    return;
                }

                LandData land = TileData.LandTable[index];
                string name = textBoxNameLand.Text;
                if (name.Length > 20)
                {
                    name = name.Substring(0, 20);
                }

                land.Name = name;
                if (ushort.TryParse(textBoxTexID.Text, out ushort shortRes))
                {
                    land.TextureId = shortRes;
                }

                land.Flags = TileFlag.None;
                Array enumValues = Enum.GetValues(typeof(TileFlag));
                for (int i = 0; i < checkedListBox2.Items.Count; ++i)
                {
                    if (checkedListBox2.GetItemChecked(i))
                    {
                        land.Flags |= (TileFlag)enumValues.GetValue(i + 1);
                    }
                }

                TileData.LandTable[index] = land;
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index);
                MarkLandModified(index);
                if (memorySaveWarningToolStripMenuItem.Checked)
                {
                    MessageBox.Show(
                        string.Format(
                            "Edits of 0x{0:X4} ({0}) saved to memory. Click 'Save Tiledata' to write to file.", index),
                        "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            Options.TileDataDirectlySaveOnChange = saveDirectlyOnChangesToolStripMenuItem.Checked;
        }

        private void OnTextChangedItemAnim(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!short.TryParse(textBoxAnim.Text, out short shortRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Animation = shortRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemName(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            string name = textBoxName.Text;
            if (name.Length == 0)
            {
                return;
            }

            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            item.Name = name;

            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemWeight(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxWeight.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Weight = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemQuality(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxQuality.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Quality = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemQuantity(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxQuantity.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Quantity = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemHue(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxHue.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Hue = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemStackOff(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxStackOff.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.StackingOffset = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemValue(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxValue.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Value = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemHeight(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxHeigth.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Height = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemMiscData(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!short.TryParse(textBoxUnk1.Text, out short shortRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.MiscData = shortRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemUnk2(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxUnk2.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Unk2 = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedItemUnk3(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            if (!byte.TryParse(textBoxUnk3.Text, out byte byteRes))
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            item.Unk3 = byteRes;
            TileData.ItemTable[index] = item;
            MarkItemModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
        }

        private void OnTextChangedLandName(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            LandData land = TileData.LandTable[index];
            string name = textBoxNameLand.Text;
            if (name.Length == 0)
            {
                return;
            }

            if (name.Length > 20)
            {
                name = name.Substring(0, 20);
            }

            land.Name = name;
            TileData.LandTable[index] = land;
            MarkLandModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index);
        }

        private void OnTextChangedLandTexID(object sender, EventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            if (!ushort.TryParse(textBoxTexID.Text, out ushort shortRes))
            {
                return;
            }

            LandData land = TileData.LandTable[index];
            land.TextureId = shortRes;
            TileData.LandTable[index] = land;
            MarkLandModified(index);
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, index);
        }

        private void OnFlagItemCheckItems(object sender, ItemCheckEventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (e.CurrentValue == e.NewValue)
            {
                return;
            }

            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            ItemData item = TileData.ItemTable[index];
            Array enumValues = Enum.GetValues(typeof(TileFlag));

            TileFlag changeFlag = (TileFlag)enumValues.GetValue(e.Index + 1);

            if ((item.Flags & changeFlag) != 0) // better double check
            {
                if (e.NewValue != CheckState.Unchecked)
                {
                    return;
                }

                item.Flags ^= changeFlag;
                TileData.ItemTable[index] = item;
                MarkItemModified(index);
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
            }
            else if ((item.Flags & changeFlag) == 0)
            {
                if (e.NewValue != CheckState.Checked)
                {
                    return;
                }

                item.Flags |= changeFlag;
                TileData.ItemTable[index] = item;
                MarkItemModified(index);
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index + 0x4000);
            }
        }

        private void OnFlagItemCheckLandTiles(object sender, ItemCheckEventArgs e)
        {
            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
            {
                return;
            }

            if (_changingIndex)
            {
                return;
            }

            if (e.CurrentValue == e.NewValue)
            {
                return;
            }

            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            LandData land = TileData.LandTable[index];
            TileFlag changeFlag;
            switch (e.Index)
            {
                case 0:
                    changeFlag = TileFlag.Damaging;
                    break;

                case 1:
                    changeFlag = TileFlag.Wet;
                    break;

                case 2:
                    changeFlag = TileFlag.Impassable;
                    break;

                case 3:
                    changeFlag = TileFlag.Wall;
                    break;

                case 4:
                    changeFlag = TileFlag.NoDiagonal;
                    break;

                default:
                    changeFlag = TileFlag.None;
                    break;
            }

            if ((land.Flags & changeFlag) != 0)
            {
                if (e.NewValue != CheckState.Unchecked)
                {
                    return;
                }

                land.Flags ^= changeFlag;
                TileData.LandTable[index] = land;
                MarkLandModified(index);
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index);
            }
            else if ((land.Flags & changeFlag) == 0)
            {
                if (e.NewValue != CheckState.Checked)
                {
                    return;
                }

                land.Flags |= changeFlag;
                TileData.LandTable[index] = land;
                MarkLandModified(index);
                Options.ChangedUltimaClass["TileData"] = true;
                ControlEvents.FireTileDataChangeEvent(this, index);
            }
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            if (tabcontrol.SelectedIndex == 0) // items
            {
                string fileName = Path.Combine(path, "ItemData.csv");
                TileData.ExportItemDataToCsv(fileName);

                FileSavedDialog.Show(FindForm(), fileName, "ItemData saved successfully.");
            }
            else
            {
                string fileName = Path.Combine(path, "LandData.csv");
                TileData.ExportLandDataToCsv(fileName);

                FileSavedDialog.Show(FindForm(), fileName, "LandData saved successfully.");
            }
        }
        private void OnClickSelectItem(object sender, EventArgs e)
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

        private void OnClickSelectInLandTiles(object sender, EventArgs e)
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

        private void OnClickSelectRadarItem(object sender, EventArgs e)
        {
            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            RadarColorControl.Select(index, false);
        }

        private void OnClickSelectRadarLand(object sender, EventArgs e)
        {
            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            RadarColorControl.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Options.ChangedUltimaClass["TileData"] = true;
                if (tabcontrol.SelectedIndex == 0) // items
                {
                    TileData.ImportItemDataFromCsv(dialog.FileName);
                }
                else
                {
                    TileData.ImportLandDataFromCsv(dialog.FileName);
                }

                Reload();
            }
        }

        private TileDataFilterForm _filterFormForm;

        private void OnClickSetFilter(object sender, EventArgs e)
        {
            if (_filterFormForm?.IsDisposed == false)
            {
                return;
            }

            _filterFormForm = new TileDataFilterForm(ApplyFilterItem, ApplyFilterLand)
            {
                TopMost = true
            };
            _filterFormForm.Show();
        }

        private const int _maleGumpOffset = 50_000;
        private const int _femaleGumpOffset = 60_000;

        private static void SelectInGumpsTab(int tiledataIndex, bool female = false)
        {
            int gumpOffset = female ? _femaleGumpOffset : _maleGumpOffset;
            var animation = TileData.ItemTable[tiledataIndex].Animation;

            GumpControl.Select(animation + gumpOffset);
        }

        private void SelectInGumpsTabMaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int graphic = GetSelectedItemGraphic();
            if (graphic <= 0)
            {
                return;
            }

            SelectInGumpsTab(graphic);
        }

        private void SelectInGumpsTabFemaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int graphic = GetSelectedItemGraphic();
            if (graphic <= 0)
            {
                return;
            }

            SelectInGumpsTab(graphic, true);
        }

        private void ItemsContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int graphic = GetSelectedItemGraphic();
            if (graphic <= 0)
            {
                selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
                selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
                selectInAnimDataTabToolStripMenuItem.Enabled = false;
            }
            else
            {
                var itemData = TileData.ItemTable[graphic];

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

                selectInAnimDataTabToolStripMenuItem.Enabled =
                    Animdata.GetAnimData(graphic) != null;
            }
        }

        private void SelectInAnimDataTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int graphic = GetSelectedItemGraphic();
            if (graphic <= 0)
            {
                return;
            }

            AnimDataControl.Select(graphic);
        }

        /// <summary>
        /// DoubleClick event handler on the TextBoxTexID. Sets the TexID to the Tag value of the node
        /// i.e. 0x256 (598) lava -> 598.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBoxTexID_DoubleClick(object sender, EventArgs e)
        {
            if (!setTextureOnDoubleClickToolStripMenuItem.Checked)
            {
                return;
            }

            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            if (!int.TryParse(textBoxTexID.Text, out int texIdValue) || texIdValue == index)
            {
                return;
            }

            textBoxTexID.Text = $"{index}";
        }

        /// <summary>
        /// Click event handler on the "Set Textures" menu item. Sets all the land tiles TextureId to their index.
        /// This is written under the assumption that LandTileID == TextureId for every LandTile.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetTextureMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to set TexID for all land tiles?\n\n" +
                "This operation assumes that land tile index value is equal to texture index value.\n\n" +
                "It will only consider land tiles where TexID is 0.\n\nContinue?",
                "Set textures",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            var updated = 0;
            for (int i = 0; i < TileData.LandTable.Length; ++i)
            {
                if (!Textures.TestTexture(i) || TileData.LandTable[i].TextureId != 0)
                {
                    continue;
                }

                TileData.LandTable[i].TextureId = (ushort)i;

                MarkLandModified(i);
                updated++;

                Options.ChangedUltimaClass["TileData"] = true;
            }

            MessageBox.Show(updated > 0 ? $"Updated {updated} land tile(s)." : "Nothing was updated.", "Set textures");
        }

        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int indexValue, 0, Art.GetMaxItemId()))
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

            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            SearchGraphic(indexValue, landTilesSelected);
        }

        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            if (e.KeyCode == Keys.F3)
            {
                if (e.Shift)
                {
                    SearchNamePrevious(searchByNameToolStripTextBox.Text, landTilesSelected);
                }
                else
                {
                    SearchName(searchByNameToolStripTextBox.Text, true, landTilesSelected);
                }
                return;
            }

            SearchName(searchByNameToolStripTextBox.Text, false, landTilesSelected);
        }

        private void SearchByNameToolStripButton_Click(object sender, EventArgs e)
        {
            var landTilesSelected = tabcontrol.SelectedIndex != 0;

            SearchName(searchByNameToolStripTextBox.Text, true, landTilesSelected);
        }

        private void HelpToolStripButton_Click(object sender, EventArgs e)
        {
            using var form = new TileDataHelpForm();
            form.ShowDialog(this);
        }

        private void AssignToolTipsToLabels()
        {
            // Statics
            toolTipComponent.SetToolTip(nameLabel, GetDescription(nameLabel));
            toolTipComponent.SetToolTip(animLabel, GetDescription(animLabel));
            toolTipComponent.SetToolTip(weightLabel, GetDescription(weightLabel));
            toolTipComponent.SetToolTip(layerLabel, GetDescription(layerLabel));
            toolTipComponent.SetToolTip(quantityLabel, GetDescription(quantityLabel));
            toolTipComponent.SetToolTip(valueLabel, GetDescription(valueLabel));
            toolTipComponent.SetToolTip(stackOffLabel, GetDescription(stackOffLabel));
            toolTipComponent.SetToolTip(hueLabel, GetDescription(hueLabel));
            toolTipComponent.SetToolTip(unknown2Label, GetDescription(unknown2Label));
            toolTipComponent.SetToolTip(miscDataLabel, GetDescription(miscDataLabel));
            toolTipComponent.SetToolTip(heightLabel, GetDescription(heightLabel));
            toolTipComponent.SetToolTip(unknown3Label, GetDescription(unknown3Label));

            // Land Tiles
            toolTipComponent.SetToolTip(landNameLabel, GetDescription(landNameLabel));
            toolTipComponent.SetToolTip(landTexIdLabel, GetDescription(landTexIdLabel));
        }

        private string GetDescription(object sender)
        {
            string description = string.Empty;

            if (sender == nameLabel)
            {
                description = "This field is for the name of the item, which can be a maximum of 20 characters.";
            }
            else if (sender == animLabel)
            {
                description = "This field is for the animation ID associated with the item.";
            }
            else if (sender == weightLabel)
            {
                description = "This field is for the weight of the item.";
            }
            else if (sender == layerLabel)
            {
                description = new StringBuilder()
                    .AppendLine("This field is for the layer of the item:")
                    .AppendLine("")
                    .AppendLine("1 One handed weapon")
                    .AppendLine("2 Two handed weapon, shield, or misc.")
                    .AppendLine("3 Shoes")
                    .AppendLine("4 Pants")
                    .AppendLine("5 Shirt")
                    .AppendLine("6 Helm / Line")
                    .AppendLine("7 Gloves")
                    .AppendLine("8 Ring")
                    .AppendLine("9 Talisman")
                    .AppendLine("10 Neck")
                    .AppendLine("11 Hair")
                    .AppendLine("12 Waist (half apron)")
                    .AppendLine("13 Torso (inner) (chest armor)")
                    .AppendLine("14 Bracelet")
                    .AppendLine("15 Unused (but backpackers for backpackers go to 21)")
                    .AppendLine("16 Facial Hair")
                    .AppendLine("17 Torso (middle) (surcoat, tunic, full apron, sash)")
                    .AppendLine("18 Earrings")
                    .AppendLine("19 Arms")
                    .AppendLine("20 Back (cloak)")
                    .AppendLine("21 Backpack")
                    .AppendLine("22 Torso (outer) (robe)")
                    .AppendLine("23 Legs (outer) (skirt / kilt)")
                    .AppendLine("24 Legs (inner) (leg armor)")
                    .AppendLine("25 Mount (horse, ostard, etc)")
                    .AppendLine("26 NPC Buy Restock container")
                    .AppendLine("27 NPC Buy no restock container")
                    .AppendLine("28 NPC Sell container")
                    .ToString();
            }
            else if (sender == quantityLabel)
            {
                description = "This field is for the quantity of the item.";
            }
            else if (sender == valueLabel)
            {
                description = "This field is for the value of the item.";
            }
            else if (sender == stackOffLabel)
            {
                description = new StringBuilder()
                    .AppendLine("StackOff refers to the stacking offset in pixels when multiple items are stacked.")
                    .AppendLine("A higher StackOff value means the items will appear further apart from each other within the stack.")
                    .ToString();
            }
            else if (sender == hueLabel)
            {
                description = "This field is for the hue (color) of the item.";
            }
            else if (sender == unknown2Label)
            {
                description = "This field is for the second unknown value.";
            }
            else if (sender == miscDataLabel)
            {
                description = "Old UO Demo weapon template definition";
            }
            else if (sender == heightLabel)
            {
                description = "This field is for the height of the item.";
            }
            else if (sender == unknown3Label)
            {
                description = "This field is for the third unknown value.";
            }
            else if (sender == landNameLabel)
            {
                description = "This field is for the name of the land tile, which can be a maximum of 20 characters.";
            }
            else if (sender == landTexIdLabel)
            {
                description = "This field is for the texture ID associated with the land tile.";
            }

            return description;
        }
    }
}