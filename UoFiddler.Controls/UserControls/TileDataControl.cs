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

        // Snapshot of the entries the last bulk apply overwrote, so one misplaced
        // "apply to everything selected" can be taken back. Only one level is kept.
        private TileDataBulkUndo _lastBulkUndo;

        private int GetSelectedItemGraphic()
        {
            return GetPrimaryGraphic(listViewItem, _itemIndices);
        }

        private int GetSelectedLandGraphic()
        {
            return GetPrimaryGraphic(listViewLand, _landIndices);
        }

        /// <summary>
        /// The entry the editor pane shows - the focused row while it is part of the
        /// selection, which is the one the user picked last, otherwise the first
        /// selected row.
        /// </summary>
        private static int GetPrimaryGraphic(ListView listView, int[] indices)
        {
            if (listView.SelectedIndices.Count == 0)
            {
                return -1;
            }

            int row = listView.FocusedItem?.Index ?? -1;
            if (row < 0 || !listView.SelectedIndices.Contains(row))
            {
                row = listView.SelectedIndices[0];
            }

            return (uint)row < (uint)indices.Length ? indices[row] : -1;
        }

        /// <summary>
        /// Every selected graphic id in ascending order. Rows are mapped through the
        /// filter projection, so this is ids, not row positions.
        /// </summary>
        private static int[] GetSelectedGraphics(ListView listView, int[] indices)
        {
            ListView.SelectedIndexCollection selected = listView.SelectedIndices;
            var graphics = new List<int>(selected.Count);
            foreach (int row in selected)
            {
                if ((uint)row < (uint)indices.Length)
                {
                    graphics.Add(indices[row]);
                }
            }

            graphics.Sort();
            return graphics.ToArray();
        }

        private int[] GetSelectedItemGraphics()
        {
            return GetSelectedGraphics(listViewItem, _itemIndices);
        }

        private int[] GetSelectedLandGraphics()
        {
            return GetSelectedGraphics(listViewLand, _landIndices);
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

        /// <summary>
        /// Selects several rows at once, focusing the last one so the editor pane
        /// shows it. Batched - a virtual ListView raises a selection event per row.
        /// </summary>
        private static void SelectRows(ListView listView, IReadOnlyList<int> rowPositions, int rowCount)
        {
            listView.BeginUpdate();
            try
            {
                listView.SelectedIndices.Clear();
                foreach (int rowPos in rowPositions)
                {
                    if ((uint)rowPos < (uint)rowCount)
                    {
                        listView.SelectedIndices.Add(rowPos);
                    }
                }
            }
            finally
            {
                listView.EndUpdate();
            }

            if (rowPositions.Count == 0)
            {
                return;
            }

            int last = rowPositions[rowPositions.Count - 1];
            if ((uint)last < (uint)rowCount)
            {
                listView.EnsureVisible(last);
                listView.FocusedItem = listView.Items[last];
            }
        }

        private void SelectItemRows(IReadOnlyList<int> rowPositions)
        {
            SelectRows(listViewItem, rowPositions, _itemIndices.Length);
        }

        private void SelectLandRows(IReadOnlyList<int> rowPositions)
        {
            SelectRows(listViewLand, rowPositions, _landIndices.Length);
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

        /// <summary>
        /// Cross-tab entry point carrying a whole selection over, e.g. from the Items
        /// tab's "Select in TileData tab". See <see cref="Select(int, bool)"/> for why
        /// the tab has to be activated before the selection is set.
        /// </summary>
        public static void Select(IReadOnlyList<int> graphics, bool land)
        {
            if (_refMarker == null || graphics == null || graphics.Count == 0)
            {
                return;
            }

            if (graphics.Count == 1)
            {
                Select(graphics[0], land);
                return;
            }

            TabPageNavigator.ActivateOwningTabPage(_refMarker);

            if (_refMarker.IsHandleCreated)
            {
                _refMarker.BeginInvoke(new Action(() => SearchGraphics(graphics, land)));
            }
            else
            {
                SearchGraphics(graphics, land);
            }
        }

        /// <summary>
        /// Selects every given graphic. Returns false only when none of them exist at
        /// all; ids the current filter hides are reached by resetting the view once,
        /// the same way <see cref="SearchGraphic"/> does for a single id.
        /// </summary>
        public static bool SearchGraphics(IReadOnlyList<int> graphics, bool land)
        {
            if (_refMarker == null || graphics == null || graphics.Count == 0)
            {
                return false;
            }

            int[] indices = land ? _refMarker._landIndices : _refMarker._itemIndices;
            List<int> rows = MapGraphicsToRows(graphics, indices);

            if (rows.Count < graphics.Count)
            {
                // At least one target is filtered out of the view - drop the filter so
                // the navigation always lands on the full selection.
                if (land)
                {
                    _refMarker.ResetLandView();
                    indices = _refMarker._landIndices;
                }
                else
                {
                    _refMarker.ResetItemView();
                    indices = _refMarker._itemIndices;
                }

                rows = MapGraphicsToRows(graphics, indices);
            }

            if (rows.Count == 0)
            {
                return false;
            }

            if (land)
            {
                _refMarker.tabcontrol.SelectTab(1);
                _refMarker.SelectLandRows(rows);
            }
            else
            {
                _refMarker.tabcontrol.SelectTab(0);
                _refMarker.SelectItemRows(rows);
            }

            return true;
        }

        /// <summary>
        /// Row lookup for a batch of ids. The projection arrays are always ascending -
        /// identity, or filter matches appended in order - so this can binary search
        /// instead of scanning the array once per id.
        /// </summary>
        private static List<int> MapGraphicsToRows(IReadOnlyList<int> graphics, int[] indices)
        {
            var rows = new List<int>(graphics.Count);
            foreach (int graphic in graphics)
            {
                int pos = Array.BinarySearch(indices, graphic);
                if (pos >= 0)
                {
                    rows.Add(pos);
                }
            }

            rows.Sort();
            return rows;
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
            // Copy/paste is handled here rather than as menu ShortcutKeys: a shortcut on
            // a ContextMenuStrip is processed for the whole form, which would swallow
            // Ctrl+C/Ctrl+V in every text box on every tab.
            if (keyData == (Keys.Control | Keys.C))
            {
                if (listViewItem.Focused)
                {
                    OnClickCopyItemTileData(this, EventArgs.Empty);
                    return true;
                }

                if (listViewLand.Focused)
                {
                    OnClickCopyLandTileData(this, EventArgs.Empty);
                    return true;
                }
            }
            else if (keyData == (Keys.Control | Keys.V))
            {
                if (listViewItem.Focused && _copiedItem != null)
                {
                    OnClickPasteSpecialItem(this, EventArgs.Empty);
                    return true;
                }

                if (listViewLand.Focused && _copiedLand != null)
                {
                    OnClickPasteSpecialLand(this, EventArgs.Empty);
                    return true;
                }
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

            // The snapshot refers to entries that have just been replaced wholesale.
            _lastBulkUndo = null;

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

            RefreshItemEditor();
            RefreshLandEditor();
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
                    QueueItemEditorRefresh();
                }
            }
            else
            {
                MarkLandModified(index);
                if (GetSelectedLandGraphic() == index)
                {
                    QueueLandEditorRefresh();
                }
            }
        }

        private void OnItemSelectedIndexChanged(object sender, EventArgs e)
        {
            QueueItemEditorRefresh();
        }

        private void OnLandSelectedIndexChanged(object sender, EventArgs e)
        {
            QueueLandEditorRefresh();
        }

        private void OnItemSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            QueueItemEditorRefresh();
        }

        private void OnLandSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            QueueLandEditorRefresh();
        }

        // SelectedIndexChanged fires once per row, so rubber-banding a few thousand
        // rows would otherwise repopulate the whole editor pane - art decode included -
        // once per row. Coalesce everything raised in one message-loop turn.
        private bool _itemRefreshPending;
        private bool _landRefreshPending;

        private void QueueItemEditorRefresh()
        {
            if (_itemRefreshPending || !IsHandleCreated || IsDisposed)
            {
                return;
            }

            _itemRefreshPending = true;
            BeginInvoke(new Action(() =>
            {
                _itemRefreshPending = false;
                if (!IsDisposed)
                {
                    RefreshItemEditor();
                }
            }));
        }

        private void QueueLandEditorRefresh()
        {
            if (_landRefreshPending || !IsHandleCreated || IsDisposed)
            {
                return;
            }

            _landRefreshPending = true;
            BeginInvoke(new Action(() =>
            {
                _landRefreshPending = false;
                if (!IsDisposed)
                {
                    RefreshLandEditor();
                }
            }));
        }

        private void RefreshItemEditor()
        {
            int[] selection = GetSelectedItemGraphics();
            UpdateMultiSelectInfoLabel(multiSelectItemInfoLabel, selection.Length);

            int primary = GetSelectedItemGraphic();
            if (primary < 0)
            {
                return;
            }

            UpdateSelectedItemPreview(primary);

            if (selection.Length > 1)
            {
                ApplyItemMixedState(selection);
                _itemMultiBaseline = BuildItemEditFromEditor();
            }
            else
            {
                _itemMultiBaseline = null;
            }
        }

        private void RefreshLandEditor()
        {
            int[] selection = GetSelectedLandGraphics();
            UpdateMultiSelectInfoLabel(multiSelectLandInfoLabel, selection.Length);

            int primary = GetSelectedLandGraphic();
            if (primary < 0)
            {
                return;
            }

            UpdateSelectedLandPreview(primary);

            if (selection.Length > 1)
            {
                ApplyLandMixedState(selection);
                _landMultiBaseline = BuildLandEditFromEditor();
            }
            else
            {
                _landMultiBaseline = null;
            }
        }

        private static void UpdateMultiSelectInfoLabel(Label label, int selectionCount)
        {
            if (selectionCount > 1)
            {
                label.Text =
                    $"{selectionCount} entries selected - 'Save Changes' writes what you edit to all of them."
                    + " Empty boxes and greyed flags are left unchanged.";
                label.Visible = true;
            }
            else
            {
                label.Visible = false;
            }
        }

        /// <summary>
        /// With more than one entry selected the pane shows the primary entry's values,
        /// then this blanks every box the selection disagrees on and greys every flag
        /// that is not uniformly set or clear. Blank and grey both read as "leave
        /// alone" when the edit is applied.
        /// </summary>
        private void ApplyItemMixedState(int[] selection)
        {
            ref readonly ItemData first = ref TileData.ItemTable[selection[0]];

            bool sameName = true;
            bool sameAnim = true;
            bool sameWeight = true;
            bool sameQuality = true;
            bool sameQuantity = true;
            bool sameHue = true;
            bool sameStackOff = true;
            bool sameValue = true;
            bool sameHeight = true;
            bool sameMisc = true;
            bool sameUnk2 = true;
            bool sameUnk3 = true;

            TileFlag inAll = first.Flags;
            TileFlag inAny = first.Flags;

            for (int i = 1; i < selection.Length; ++i)
            {
                ref readonly ItemData row = ref TileData.ItemTable[selection[i]];

                sameName &= string.Equals(row.Name, first.Name, StringComparison.Ordinal);
                sameAnim &= row.Animation == first.Animation;
                sameWeight &= row.Weight == first.Weight;
                sameQuality &= row.Quality == first.Quality;
                sameQuantity &= row.Quantity == first.Quantity;
                sameHue &= row.Hue == first.Hue;
                sameStackOff &= row.StackingOffset == first.StackingOffset;
                sameValue &= row.Value == first.Value;
                sameHeight &= row.Height == first.Height;
                sameMisc &= row.MiscData == first.MiscData;
                sameUnk2 &= row.Unk2 == first.Unk2;
                sameUnk3 &= row.Unk3 == first.Unk3;

                inAll &= row.Flags;
                inAny |= row.Flags;
            }

            _changingIndex = true;
            try
            {
                BlankIfMixed(textBoxName, sameName);
                BlankIfMixed(textBoxAnim, sameAnim);
                BlankIfMixed(textBoxWeight, sameWeight);
                BlankIfMixed(textBoxQuality, sameQuality);
                BlankIfMixed(textBoxQuantity, sameQuantity);
                BlankIfMixed(textBoxHue, sameHue);
                BlankIfMixed(textBoxStackOff, sameStackOff);
                BlankIfMixed(textBoxValue, sameValue);
                BlankIfMixed(textBoxHeigth, sameHeight);
                BlankIfMixed(textBoxUnk1, sameMisc);
                BlankIfMixed(textBoxUnk2, sameUnk2);
                BlankIfMixed(textBoxUnk3, sameUnk3);

                // Set somewhere but not everywhere.
                MarkMixedFlags(checkedListBox1, inAny & ~inAll);
            }
            finally
            {
                _changingIndex = false;
            }
        }

        private void ApplyLandMixedState(int[] selection)
        {
            ref readonly LandData first = ref TileData.LandTable[selection[0]];

            bool sameName = true;
            bool sameTexture = true;

            TileFlag inAll = first.Flags;
            TileFlag inAny = first.Flags;

            for (int i = 1; i < selection.Length; ++i)
            {
                ref readonly LandData row = ref TileData.LandTable[selection[i]];

                sameName &= string.Equals(row.Name, first.Name, StringComparison.Ordinal);
                sameTexture &= row.TextureId == first.TextureId;

                inAll &= row.Flags;
                inAny |= row.Flags;
            }

            _changingIndex = true;
            try
            {
                BlankIfMixed(textBoxNameLand, sameName);
                BlankIfMixed(textBoxTexID, sameTexture);

                MarkMixedFlags(checkedListBox2, inAny & ~inAll);
            }
            finally
            {
                _changingIndex = false;
            }
        }

        private static void BlankIfMixed(TextBox textBox, bool allAgree)
        {
            if (!allAgree)
            {
                textBox.Text = string.Empty;
            }
        }

        private static void MarkMixedFlags(CheckedListBox checkedListBox, TileFlag mixed)
        {
            if (mixed == TileFlag.None)
            {
                return;
            }

            Array enumValues = Enum.GetValues(typeof(TileFlag));
            for (int i = 0; i < checkedListBox.Items.Count; ++i)
            {
                if ((mixed & (TileFlag)enumValues.GetValue(i + 1)) != 0)
                {
                    checkedListBox.SetItemCheckState(i, CheckState.Indeterminate);
                }
            }
        }

        /// <summary>
        /// Multi-selection flag cycling, which CheckedListBox will not do on its own.
        /// A flag the selection disagreed on cycles leave alone -> set on all -> clear
        /// on all, so the "don't touch it" state stays reachable. A flag they all
        /// already agreed on just toggles: leaving it at the value it came up with is
        /// already a no-op, so it has no need of a third state.
        /// </summary>
        private static CheckState NextMultiSelectFlagState(CheckState current, bool wasMixed)
        {
            if (!wasMixed)
            {
                return current == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked;
            }

            switch (current)
            {
                case CheckState.Indeterminate:
                    return CheckState.Checked;

                case CheckState.Checked:
                    return CheckState.Unchecked;

                default:
                    return CheckState.Indeterminate;
            }
        }

        /// <summary>
        /// True when the selection disagreed on this flag at the time the pane was
        /// populated - i.e. the baseline left it out of both masks.
        /// </summary>
        private static bool FlagWasMixed(TileFlag baselineSet, TileFlag baselineClear, int flagIndex)
        {
            Array enumValues = Enum.GetValues(typeof(TileFlag));
            if ((uint)(flagIndex + 1) >= (uint)enumValues.Length)
            {
                return false;
            }

            var flag = (TileFlag)enumValues.GetValue(flagIndex + 1);
            return (baselineSet & flag) == 0 && (baselineClear & flag) == 0;
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
                if (listViewItem.SelectedIndices.Count > 1)
                {
                    ApplyItemEditToSelection(GetSelectedItemGraphics(), BuildItemEditForSelection());
                    return;
                }

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
                if (listViewLand.SelectedIndices.Count > 1)
                {
                    ApplyLandEditToSelection(GetSelectedLandGraphics(), BuildLandEditForSelection());
                    return;
                }

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

        /// <summary>
        /// Reads the editor pane into a sparse edit. An empty or unparseable box and an
        /// indeterminate flag are both left out, so applying it across a selection only
        /// touches what the user actually filled in.
        /// </summary>
        private ItemDataEdit BuildItemEditFromEditor()
        {
            var edit = new ItemDataEdit
            {
                Name = string.IsNullOrEmpty(textBoxName.Text) ? null : TileDataBulkEdit.TruncateName(textBoxName.Text),
                Animation = ParseShort(textBoxAnim.Text),
                Weight = ParseByte(textBoxWeight.Text),
                Quality = ParseByte(textBoxQuality.Text),
                Quantity = ParseByte(textBoxQuantity.Text),
                Hue = ParseByte(textBoxHue.Text),
                StackingOffset = ParseByte(textBoxStackOff.Text),
                Value = ParseByte(textBoxValue.Text),
                Height = ParseByte(textBoxHeigth.Text),
                MiscData = ParseShort(textBoxUnk1.Text),
                Unk2 = ParseByte(textBoxUnk2.Text),
                Unk3 = ParseByte(textBoxUnk3.Text)
            };

            ReadFlagMasks(checkedListBox1, out TileFlag setFlags, out TileFlag clearFlags);
            edit.SetFlags = setFlags;
            edit.ClearFlags = clearFlags;

            return edit;
        }

        private LandDataEdit BuildLandEditFromEditor()
        {
            var edit = new LandDataEdit
            {
                Name = string.IsNullOrEmpty(textBoxNameLand.Text)
                    ? null
                    : TileDataBulkEdit.TruncateName(textBoxNameLand.Text),
                TextureId = ushort.TryParse(textBoxTexID.Text, out ushort textureId) ? textureId : (ushort?)null
            };

            ReadFlagMasks(checkedListBox2, out TileFlag setFlags, out TileFlag clearFlags);
            edit.SetFlags = setFlags;
            edit.ClearFlags = clearFlags;

            return edit;
        }

        private static void ReadFlagMasks(CheckedListBox checkedListBox, out TileFlag setFlags, out TileFlag clearFlags)
        {
            setFlags = TileFlag.None;
            clearFlags = TileFlag.None;

            Array enumValues = Enum.GetValues(typeof(TileFlag));
            for (int i = 0; i < checkedListBox.Items.Count; ++i)
            {
                CheckState state = checkedListBox.GetItemCheckState(i);
                if (state == CheckState.Indeterminate)
                {
                    continue;
                }

                var flag = (TileFlag)enumValues.GetValue(i + 1);
                if (state == CheckState.Checked)
                {
                    setFlags |= flag;
                }
                else
                {
                    clearFlags |= flag;
                }
            }
        }

        private static byte? ParseByte(string text)
        {
            return byte.TryParse(text, out byte value) ? value : (byte?)null;
        }

        private static short? ParseShort(string text)
        {
            return short.TryParse(text, out short value) ? value : (short?)null;
        }

        // The pane as it stood when it was populated for the current multi-selection.
        // A bulk apply writes the difference against this, so boxes and flags the user
        // left alone are neither listed in the confirmation nor written back over
        // entries that already agree.
        private ItemDataEdit _itemMultiBaseline;
        private LandDataEdit _landMultiBaseline;

        /// <summary>
        /// What the user actually changed in the pane since it was populated for this
        /// selection.
        /// </summary>
        private ItemDataEdit BuildItemEditForSelection()
        {
            ItemDataEdit current = BuildItemEditFromEditor();
            ItemDataEdit baseline = _itemMultiBaseline;
            if (baseline == null)
            {
                return current;
            }

            return new ItemDataEdit
            {
                Name = OnlyIfChanged(current.Name, baseline.Name),
                Animation = OnlyIfChanged(current.Animation, baseline.Animation),
                Weight = OnlyIfChanged(current.Weight, baseline.Weight),
                Quality = OnlyIfChanged(current.Quality, baseline.Quality),
                Quantity = OnlyIfChanged(current.Quantity, baseline.Quantity),
                Hue = OnlyIfChanged(current.Hue, baseline.Hue),
                StackingOffset = OnlyIfChanged(current.StackingOffset, baseline.StackingOffset),
                Value = OnlyIfChanged(current.Value, baseline.Value),
                Height = OnlyIfChanged(current.Height, baseline.Height),
                MiscData = OnlyIfChanged(current.MiscData, baseline.MiscData),
                Unk2 = OnlyIfChanged(current.Unk2, baseline.Unk2),
                Unk3 = OnlyIfChanged(current.Unk3, baseline.Unk3),

                // Only flags the user moved to checked / unchecked from something else.
                SetFlags = current.SetFlags & ~baseline.SetFlags,
                ClearFlags = current.ClearFlags & ~baseline.ClearFlags
            };
        }

        private LandDataEdit BuildLandEditForSelection()
        {
            LandDataEdit current = BuildLandEditFromEditor();
            LandDataEdit baseline = _landMultiBaseline;
            if (baseline == null)
            {
                return current;
            }

            return new LandDataEdit
            {
                Name = OnlyIfChanged(current.Name, baseline.Name),
                TextureId = OnlyIfChanged(current.TextureId, baseline.TextureId),
                SetFlags = current.SetFlags & ~baseline.SetFlags,
                ClearFlags = current.ClearFlags & ~baseline.ClearFlags
            };
        }

        private static T? OnlyIfChanged<T>(T? current, T? baseline) where T : struct
        {
            return current.HasValue && !EqualityComparer<T?>.Default.Equals(current, baseline)
                ? current
                : null;
        }

        private static string OnlyIfChanged(string current, string baseline)
        {
            return current != null && !string.Equals(current, baseline, StringComparison.Ordinal)
                ? current
                : null;
        }

        /// <summary>
        /// Writes one sparse edit to every selected item entry, after confirming what is
        /// about to change and snapshotting the old values for a single level of undo.
        /// </summary>
        private void ApplyItemEditToSelection(int[] graphics, ItemDataEdit edit)
        {
            if (graphics.Length == 0)
            {
                return;
            }

            string what = TileDataBulkEdit.Describe(edit);
            if (!ConfirmBulkApply(what, graphics.Length))
            {
                return;
            }

            _lastBulkUndo = TileDataBulkUndo.ForItems(graphics, what);

            using (new WaitCursorScope(this))
            {
                foreach (int graphic in graphics)
                {
                    TileData.ItemTable[graphic] = TileDataBulkEdit.Apply(TileData.ItemTable[graphic], edit);

                    // Mark without RedrawItemRow - that scans the projection array per
                    // call, which would be quadratic over a large selection. One
                    // Invalidate below repaints the lot.
                    _modifiedItems.Add(graphic);
                    ControlEvents.FireTileDataChangeEvent(this, graphic + 0x4000);
                }
            }

            Options.ChangedUltimaClass["TileData"] = true;
            listViewItem.Invalidate();
            QueueItemEditorRefresh();

            ReportBulkApply(what, graphics.Length);
        }

        private void ApplyLandEditToSelection(int[] graphics, LandDataEdit edit)
        {
            if (graphics.Length == 0)
            {
                return;
            }

            string what = TileDataBulkEdit.Describe(edit);
            if (!ConfirmBulkApply(what, graphics.Length))
            {
                return;
            }

            _lastBulkUndo = TileDataBulkUndo.ForLand(graphics, what);

            using (new WaitCursorScope(this))
            {
                foreach (int graphic in graphics)
                {
                    TileData.LandTable[graphic] = TileDataBulkEdit.Apply(TileData.LandTable[graphic], edit);
                    _modifiedLand.Add(graphic);
                    ControlEvents.FireTileDataChangeEvent(this, graphic);
                }
            }

            Options.ChangedUltimaClass["TileData"] = true;
            listViewLand.Invalidate();
            QueueLandEditorRefresh();

            ReportBulkApply(what, graphics.Length);
        }

        private bool ConfirmBulkApply(string what, int count)
        {
            if (string.IsNullOrEmpty(what))
            {
                MessageBox.Show(
                    "Nothing to apply - every box is empty and every flag is left unchanged.",
                    "Apply to selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return MessageBox.Show(
                $"Apply {what} to {count} entries?",
                "Apply to selection", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        private void ReportBulkApply(string what, int count)
        {
            if (!memorySaveWarningToolStripMenuItem.Checked)
            {
                return;
            }

            MessageBox.Show(
                $"Applied {what} to {count} entries in memory.\r\n\r\nClick 'Save Tiledata' to write to file.",
                "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void MiscToolStripDropDownButton_DropDownOpening(object sender, EventArgs e)
        {
            undoBulkApplyToolStripMenuItem.Enabled = _lastBulkUndo != null;
            undoBulkApplyToolStripMenuItem.Text = _lastBulkUndo == null
                ? "Undo last bulk apply"
                : $"Undo last bulk apply ({_lastBulkUndo.Count} entries)";
        }

        private void OnClickUndoBulkApply(object sender, EventArgs e)
        {
            TileDataBulkUndo undo = _lastBulkUndo;
            if (undo == null)
            {
                return;
            }

            if (MessageBox.Show(
                    $"Restore {undo.Count} entries to the values they had before '{undo.Description}' was applied?",
                    "Undo bulk apply", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) != DialogResult.Yes)
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                for (int i = 0; i < undo.Ids.Length; ++i)
                {
                    int graphic = undo.Ids[i];
                    if (undo.Land)
                    {
                        TileData.LandTable[graphic] = undo.Lands[i];
                        ControlEvents.FireTileDataChangeEvent(this, graphic);
                    }
                    else
                    {
                        TileData.ItemTable[graphic] = undo.Items[i];
                        ControlEvents.FireTileDataChangeEvent(this, graphic + 0x4000);
                    }
                }
            }

            // The entries keep their modified marker on purpose: undoing restores what
            // was in memory before this apply, which is not necessarily what is on disk.
            _lastBulkUndo = null;

            if (undo.Land)
            {
                listViewLand.Invalidate();
                QueueLandEditorRefresh();
            }
            else
            {
                listViewItem.Invalidate();
                QueueItemEditorRefresh();
            }
        }

        private void SaveDirectlyOnChangesToolStripMenuItemOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            Options.TileDataDirectlySaveOnChange = saveDirectlyOnChangesToolStripMenuItem.Checked;
        }

        // "Save directly on changes" writes on every keystroke, which has no sensible
        // meaning across a multi-selection - half-typed values would land on every
        // selected entry. Bulk edits go through Save Changes instead, so the
        // per-keystroke path only runs while exactly one entry is selected.
        private bool DirectSaveItemEnabled =>
            saveDirectlyOnChangesToolStripMenuItem.Checked && listViewItem.SelectedIndices.Count == 1;

        private bool DirectSaveLandEnabled =>
            saveDirectlyOnChangesToolStripMenuItem.Checked && listViewLand.SelectedIndices.Count == 1;

        private void OnTextChangedItemAnim(object sender, EventArgs e)
        {
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveItemEnabled)
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
            if (!DirectSaveLandEnabled)
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
            if (!DirectSaveLandEnabled)
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
            if (_changingIndex)
            {
                return;
            }

            if (listViewItem.SelectedIndices.Count > 1)
            {
                bool wasMixed = _itemMultiBaseline != null
                                && FlagWasMixed(_itemMultiBaseline.SetFlags, _itemMultiBaseline.ClearFlags, e.Index);
                e.NewValue = NextMultiSelectFlagState(e.CurrentValue, wasMixed);
                return;
            }

            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
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
            if (_changingIndex)
            {
                return;
            }

            if (listViewLand.SelectedIndices.Count > 1)
            {
                bool wasMixed = _landMultiBaseline != null
                                && FlagWasMixed(_landMultiBaseline.SetFlags, _landMultiBaseline.ClearFlags, e.Index);
                e.NewValue = NextMultiSelectFlagState(e.CurrentValue, wasMixed);
                return;
            }

            if (!saveDirectlyOnChangesToolStripMenuItem.Checked)
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

            // The list holds every TileFlag in enum order (index 0 is None and is not
            // listed), the same mapping OnClickSaveChanges uses. It used to be a
            // hardcoded switch over five flags, which toggled the wrong bit for the
            // first five entries and did nothing at all past them.
            Array enumValues = Enum.GetValues(typeof(TileFlag));
            var changeFlag = (TileFlag)enumValues.GetValue(e.Index + 1);

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

        // In-process clipboard for tiledata entries, one slot per table so item data can
        // never land on a land tile. Not the Windows clipboard - there is no sensible
        // text form of a tiledata entry to hand to other applications.
        private static ItemData? _copiedItem;
        private static int _copiedItemGraphic = -1;
        private static LandData? _copiedLand;
        private static int _copiedLandGraphic = -1;

        private void OnClickCopyItemTileData(object sender, EventArgs e)
        {
            int graphic = GetSelectedItemGraphic();
            if (graphic < 0)
            {
                return;
            }

            _copiedItem = TileData.ItemTable[graphic];
            _copiedItemGraphic = graphic;
        }

        private void OnClickCopyLandTileData(object sender, EventArgs e)
        {
            int graphic = GetSelectedLandGraphic();
            if (graphic < 0)
            {
                return;
            }

            _copiedLand = TileData.LandTable[graphic];
            _copiedLandGraphic = graphic;
        }

        private void OnClickPasteSpecialItem(object sender, EventArgs e)
        {
            if (_copiedItem == null)
            {
                return;
            }

            int[] graphics = GetSelectedItemGraphics();
            if (graphics.Length == 0)
            {
                return;
            }

            using (var dialog = new TileDataPasteSpecialForm(_copiedItem.Value, _copiedItemGraphic, graphics.Length))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ApplyItemEditToSelection(graphics, dialog.BuildItemEdit());
            }
        }

        private void OnClickPasteSpecialLand(object sender, EventArgs e)
        {
            if (_copiedLand == null)
            {
                return;
            }

            int[] graphics = GetSelectedLandGraphics();
            if (graphics.Length == 0)
            {
                return;
            }

            using (var dialog = new TileDataPasteSpecialForm(_copiedLand.Value, _copiedLandGraphic, graphics.Length))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                ApplyLandEditToSelection(graphics, dialog.BuildLandEdit());
            }
        }

        private void LandTilesContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int selectedCount = listViewLand.SelectedIndices.Count;

            copyLandTileDataToolStripMenuItem.Enabled = selectedCount == 1;
            pasteSpecialLandToolStripMenuItem.Enabled = _copiedLand != null && selectedCount > 0;
            pasteSpecialLandToolStripMenuItem.Text = selectedCount > 1
                ? $"Paste special onto {selectedCount}..."
                : "Paste special...";
        }

        private void ItemsContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int selectedCount = listViewItem.SelectedIndices.Count;

            copyItemTileDataToolStripMenuItem.Enabled = selectedCount == 1;
            pasteSpecialItemToolStripMenuItem.Enabled = _copiedItem != null && selectedCount > 0;
            pasteSpecialItemToolStripMenuItem.Text = selectedCount > 1
                ? $"Paste special onto {selectedCount}..."
                : "Paste special...";

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