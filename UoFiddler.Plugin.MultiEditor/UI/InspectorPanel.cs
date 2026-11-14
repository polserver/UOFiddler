/***************************************************************************
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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Lower right panel: the selected tile(s) with their cell stack, and the floors/view options.
    /// </summary>
    internal sealed partial class InspectorPanel : UserControl
    {
        private const int _iconSize = 32;

        private readonly Dictionary<ushort, int> _iconIndex = new Dictionary<ushort, int>();
        private readonly List<(string Name, int Floor, int Ceiling)> _levels = new List<(string Name, int Floor, int Ceiling)>();

        /// <summary>Rows of the selection list: one tile each, or every selected tile of one id when grouped.</summary>
        private readonly List<MultiTile[]> _selectionRows = new List<MultiTile[]>();
        private bool _groupedRows;
        private int _hoverRow = -1;
        private ContextMenuStrip _selectionMenu;

        private EditorSession _session;
        private bool _updating;

        public InspectorPanel()
        {
            InitializeComponent();

            _idLabel.Font = new Font(Font, FontStyle.Bold);
            _firstFloor.Maximum = EditorSession.MaxFirstFloorZ;
            Disposed += InspectorPanel_Disposed;

            Bind(null);
        }

        private void InspectorPanel_Disposed(object sender, EventArgs e)
        {
            _selectionMenu?.Dispose();
            if (_session != null)
            {
                _session.SelectionChanged -= Session_SelectionChanged;
                _session.DocumentChanged -= Session_DocumentChanged;
                _session.ViewChanged -= Session_ViewChanged;
                _session = null;
            }
        }

        private void X_ValueChanged(object sender, EventArgs e)
        {
            OnPositionChanged();
        }

        private void Y_ValueChanged(object sender, EventArgs e)
        {
            OnPositionChanged();
        }

        private void Z_ValueChanged(object sender, EventArgs e)
        {
            OnZChanged();
        }

        private void Dynamic_CheckedChanged(object sender, EventArgs e)
        {
            OnDynamicChanged();
        }

        private void StackZPlusButton_Click(object sender, EventArgs e)
        {
            StackZ(1);
        }

        private void StackZMinusButton_Click(object sender, EventArgs e)
        {
            StackZ(-1);
        }

        private void StackDeleteButton_Click(object sender, EventArgs e)
        {
            StackDelete();
        }

        private void StackPickButton_Click(object sender, EventArgs e)
        {
            StackPick();
        }

        private void GroupZMinus5Button_Click(object sender, EventArgs e)
        {
            GroupZ(-5);
        }

        private void GroupZMinus1Button_Click(object sender, EventArgs e)
        {
            GroupZ(-1);
        }

        private void GroupZPlus1Button_Click(object sender, EventArgs e)
        {
            GroupZ(1);
        }

        private void GroupZPlus5Button_Click(object sender, EventArgs e)
        {
            GroupZ(5);
        }

        private void GroupSetZButton_Click(object sender, EventArgs e)
        {
            GroupSetZ();
        }

        private void GroupDeleteButton_Click(object sender, EventArgs e)
        {
            GroupDelete();
        }

        private void GroupHideButton_Click(object sender, EventArgs e)
        {
            _session?.HideSelection();
        }

        private void SelectionUnselectButton_Click(object sender, EventArgs e)
        {
            UnselectRows();
        }

        private void SelectionKeepButton_Click(object sender, EventArgs e)
        {
            KeepRows(SelectedRowTiles());
        }

        private void GroupById_CheckedChanged(object sender, EventArgs e)
        {
            RebuildSelectionList();
        }

        private void Group_Resize(object sender, EventArgs e)
        {
            // Sized by hand: the list fills the rest of the panel below the buttons.
            _selectionList.Size = new Size(Math.Max(40, _group.ClientSize.Width - 8),
                Math.Max(40, _group.ClientSize.Height - _selectionList.Top - 4));
        }

        private void SelectionList_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            MultiTile[] row = e.ItemIndex < _selectionRows.Count ? _selectionRows[e.ItemIndex] : Array.Empty<MultiTile>();
            if (row.Length == 0)
            {
                e.Item = new ListViewItem(string.Empty);
                e.Item.SubItems.Add(string.Empty);
                e.Item.SubItems.Add(string.Empty);
                e.Item.SubItems.Add(string.Empty);
                return;
            }

            MultiTile first = row[0];
            if (_groupedRows)
            {
                int min = row.Min(t => t.Z);
                int max = row.Max(t => t.Z);
                e.Item = new ListViewItem($"0x{first.Id:X4}", IconFor(first.Id));
                e.Item.SubItems.Add(row.Length.ToString("N0"));
                e.Item.SubItems.Add(min == max ? min.ToString() : $"{min}–{max}");
            }
            else
            {
                e.Item = new ListViewItem(first.Invisible ? $"0x{first.Id:X4} *" : $"0x{first.Id:X4}", IconFor(first.Id));
                e.Item.SubItems.Add(first.Z.ToString());
                e.Item.SubItems.Add($"{first.X}, {first.Y}");
            }

            e.Item.SubItems.Add(TileName(first.Id));
        }

        private void SelectionList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateListHighlight();
        }

        // Shift ranges in a virtual list.
        private void SelectionList_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            UpdateListHighlight();
        }

        private void SelectionList_DoubleClick(object sender, EventArgs e)
        {
            int index = _selectionList.FocusedItem?.Index ?? -1;
            if (index >= 0 && index < _selectionRows.Count)
            {
                KeepRows(_selectionRows[index]);
            }
        }

        private void SelectionList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                UnselectRows();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                KeepRows(SelectedRowTiles());
                e.Handled = e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                _selectionList.BeginUpdate();
                for (int i = 0; i < _selectionRows.Count; ++i)
                {
                    _selectionList.SelectedIndices.Add(i);
                }

                _selectionList.EndUpdate();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void SelectionList_MouseMove(object sender, MouseEventArgs e)
        {
            int row = _selectionList.GetItemAt(e.X, e.Y)?.Index ?? -1;
            if (row != _hoverRow)
            {
                _hoverRow = row;
                UpdateListHighlight();
            }
        }

        private void SelectionList_MouseLeave(object sender, EventArgs e)
        {
            _hoverRow = -1;
            UpdateListHighlight();
        }

        private void SelectionList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || _selectionList.SelectedIndices.Count == 0)
            {
                return;
            }

            _selectionMenu ??= new ContextMenuStrip();
            _selectionMenu.Items.Clear();
            _selectionMenu.Items.Add("Unselect", null, (_, _) => UnselectRows());
            _selectionMenu.Items.Add("Keep only these", null, (_, _) => KeepRows(SelectedRowTiles()));
            _selectionMenu.Show(_selectionList, e.Location);
        }

        private void FloorList_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnFloorChosen();
        }

        private void HideAbove_CheckedChanged(object sender, EventArgs e)
        {
            Apply(() => _session.HideAboveLevel = _hideAbove.Checked);
        }

        private void HideWalls_CheckedChanged(object sender, EventArgs e)
        {
            Apply(() => _session.HideWalls = _hideWalls.Checked);
        }

        private void Limit_CheckedChanged(object sender, EventArgs e)
        {
            Apply(() => _session.HeightLimit = _limit.Checked ? _limitBar.Value : null);
        }

        private void LimitBar_ValueChanged(object sender, EventArgs e)
        {
            Apply(() =>
            {
                if (_limit.Checked)
                {
                    _session.HeightLimit = _limitBar.Value;
                }
            });
        }

        private void ShowHidden_Click(object sender, EventArgs e)
        {
            _session?.ShowHiddenTiles();
        }

        // The split into levels is the user's call: names and starting Z per design.
        private void EditFloors_Click(object sender, EventArgs e)
        {
            if (_session == null)
            {
                return;
            }

            using var dialog = new FloorsDialog(_session);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _session.SetLevelDefinitions(dialog.Result);
            }
        }

        private void FirstFloor_ValueChanged(object sender, EventArgs e)
        {
            Apply(() => _session.FirstFloorZ = (int)_firstFloor.Value);
        }

        /// <summary>Attaches to a session; null detaches and shows the empty state.</summary>
        public void Bind(EditorSession session)
        {
            if (_session != null)
            {
                _session.ListHighlight = null;
                _session.SelectionChanged -= Session_SelectionChanged;
                _session.DocumentChanged -= Session_DocumentChanged;
                _session.ViewChanged -= Session_ViewChanged;
            }

            _session = session;

            if (_session != null)
            {
                _session.SelectionChanged += Session_SelectionChanged;
                _session.DocumentChanged += Session_DocumentChanged;
                _session.ViewChanged += Session_ViewChanged;
            }

            RebuildLevels();
            RefreshSelection();
            RefreshView();
        }

        private void Session_SelectionChanged()
        {
            RefreshSelection();
        }

        private void Session_DocumentChanged()
        {
            RebuildLevels();
            RefreshSelection();
            RefreshView();
        }

        private void Session_ViewChanged()
        {
            RefreshView();
        }

        /// <summary>Runs a session write unless the panel is itself refreshing its controls.</summary>
        private void Apply(Action write)
        {
            if (_updating || _session == null)
            {
                return;
            }

            write();
        }

        // ---------------------------------------------------------------- selection

        private void RefreshSelection()
        {
            bool previous = _updating;
            _updating = true;
            try
            {
                int count = _session?.Selection.Count ?? 0;
                _hint.Visible = count == 0;
                _single.Visible = count == 1;
                _group.Visible = count > 1;

                if (count == 1)
                {
                    ShowSingle(_session.SelectedTile);
                }
                else if (count > 1)
                {
                    int min = _session.Selection.Min(t => t.Z);
                    int max = _session.Selection.Max(t => t.Z);
                    _groupLabel.Text = $"{count} tiles selected, Z {min}–{max}";
                    _groupZ.Value = Math.Clamp(min, (int)_groupZ.Minimum, (int)_groupZ.Maximum);
                }

                RebuildSelectionList();
            }
            finally
            {
                _updating = previous;
            }
        }

        /// <summary>
        /// Fills the list of selected tiles (several only): one row per tile, top Z first, or one
        /// row per tile id with the most used first.
        /// </summary>
        private void RebuildSelectionList()
        {
            _selectionRows.Clear();
            _groupedRows = _groupById.Checked;
            if (_session != null && _session.Selection.Count > 1)
            {
                IEnumerable<MultiTile> sorted = _session.Selection.OrderByDescending(t => t.Z).ThenBy(t => t.Y).ThenBy(t => t.X);
                if (_groupedRows)
                {
                    _selectionRows.AddRange(sorted.GroupBy(t => t.Id).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).Select(g => g.ToArray()));
                }
                else
                {
                    _selectionRows.AddRange(sorted.Select(t => new[] { t }));
                }
            }

            selectionColumnZ.Text = _groupedRows ? "Count" : "Z";
            selectionColumnWhere.Text = _groupedRows ? "Z" : "X, Y";
            _hoverRow = -1;
            _selectionList.SelectedIndices.Clear();
            _selectionList.VirtualListSize = _selectionRows.Count;
            _selectionList.Invalidate();
            UpdateListHighlight();
        }

        private List<MultiTile> SelectedRowTiles()
        {
            var tiles = new List<MultiTile>();
            foreach (int index in _selectionList.SelectedIndices)
            {
                if (index < _selectionRows.Count)
                {
                    tiles.AddRange(_selectionRows[index]);
                }
            }

            return tiles;
        }

        /// <summary>The tiles of the row under the mouse glow on the canvas, else those of the chosen rows.</summary>
        private void UpdateListHighlight()
        {
            if (_session == null)
            {
                return;
            }

            _session.ListHighlight = _hoverRow >= 0 && _hoverRow < _selectionRows.Count ? _selectionRows[_hoverRow] : SelectedRowTiles();
        }

        /// <summary>Takes the chosen rows out of the selection and moves to the row that took their place.</summary>
        private void UnselectRows()
        {
            List<MultiTile> tiles = SelectedRowTiles();
            if (_session == null || tiles.Count == 0)
            {
                return;
            }

            int first = _selectionList.SelectedIndices[0];
            _session.Deselect(tiles);
            if (_selectionRows.Count > 0)
            {
                int index = Math.Min(first, _selectionRows.Count - 1);
                _selectionList.SelectedIndices.Add(index);
                _selectionList.FocusedItem = _selectionList.Items[index];
                _selectionList.EnsureVisible(index);
            }
        }

        private void KeepRows(IReadOnlyCollection<MultiTile> tiles)
        {
            if (_session != null && tiles.Count > 0)
            {
                // Selecting rebuilds this list, so let the ListView finish its own event first.
                var keep = tiles.ToList();
                BeginInvoke(() => _session?.SetSelection(keep, add: false));
            }
        }

        private static string TileName(ushort id) => TileBrowserThumbnails.NameOf(id);

        private void ShowSingle(MultiTile tile)
        {
            _thumb.Image = Art.GetStatic(tile.Id);
            if (TileBrowserThumbnails.HasItemData(tile.Id))
            {
                ItemData data = TileData.ItemTable[tile.Id];
                _idLabel.Text = $"0x{tile.Id:X4}  {data.Name}";
                List<string> flags = TileBrowserThumbnails.BuildFlagNames(data.Flags);
                string height = (data.Flags & TileFlag.Bridge) != 0 ? $"Height {data.Height} (calc {data.CalcHeight})" : $"Height {data.Height}";
                _infoLabel.Text = $"{height}\r\n{(flags.Count == 0 ? "no flags" : string.Join(", ", flags))}";
            }
            else
            {
                _idLabel.Text = $"0x{tile.Id:X4}";
                _infoLabel.Text = "No tiledata entry";
            }

            _x.Maximum = Math.Max(0, _session.CompList.Width - 1);
            _y.Maximum = Math.Max(0, _session.CompList.Height - 1);
            _x.Value = Math.Clamp(tile.X, 0, (int)_x.Maximum);
            _y.Value = Math.Clamp(tile.Y, 0, (int)_y.Maximum);
            _z.Value = Math.Clamp(tile.Z, (int)_z.Minimum, (int)_z.Maximum);
            _dynamic.Checked = tile.Invisible;

            // Rebuilt from the model each time; list order is top first.
            _stack.BeginUpdate();
            try
            {
                _stack.Items.Clear();
                List<MultiTile> cell = _session.CompList.GetTilesAt(tile.X, tile.Y);
                for (int i = cell.Count - 1; i >= 0; --i)
                {
                    MultiTile entry = cell[i];
                    var item = new ListViewItem(entry.Invisible ? $"0x{entry.Id:X4} *" : $"0x{entry.Id:X4}", IconFor(entry.Id)) { Tag = entry };
                    item.SubItems.Add(entry.Z.ToString());
                    item.SubItems.Add(TileName(entry.Id));
                    _stack.Items.Add(item);
                    if (entry == tile)
                    {
                        item.Selected = true;
                        item.EnsureVisible();
                    }
                }
            }
            finally
            {
                _stack.EndUpdate();
            }
        }

        private void OnPositionChanged()
        {
            MultiTile tile = _session?.SelectedTile;
            if (_updating || tile == null)
            {
                return;
            }

            int x = (int)_x.Value;
            int y = (int)_y.Value;
            if (x == tile.X && y == tile.Y)
            {
                return;
            }

            _session.CompList.TileMove(tile, x, y);
            _session.AfterEdit();
            RefreshSelection();
        }

        private void OnZChanged()
        {
            MultiTile tile = _session?.SelectedTile;
            int z = (int)_z.Value;
            if (_updating || tile == null || z == tile.Z)
            {
                return;
            }

            _session.CompList.TileZSet(tile, z);
            _session.AfterEdit(z);
        }

        private void OnDynamicChanged()
        {
            MultiTile tile = _session?.SelectedTile;
            if (_updating || tile == null || tile.Invisible == _dynamic.Checked)
            {
                return;
            }

            _session.CompList.TileSetInvisible(tile, _dynamic.Checked);
            _session.AfterEdit();
        }

        private void Stack_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating || _stack.SelectedItems.Count == 0 || _stack.SelectedItems[0].Tag is not MultiTile tile)
            {
                return;
            }

            // Selecting rebuilds this list, so let the ListView finish its own change first.
            BeginInvoke(() => _session?.SetSelection(new[] { tile }, add: false));
        }

        private void StackZ(int delta)
        {
            MultiTile tile = _session?.SelectedTile;
            if (tile == null)
            {
                return;
            }

            _session.CompList.TileZMod(tile, delta);
            _session.AfterEdit(delta > 0 ? tile.Z : null);
        }

        private void StackDelete()
        {
            MultiTile removed = _session?.SelectedTile;
            if (removed == null)
            {
                return;
            }

            // Keep working in the same cell: select the tile now on top of it.
            _session.CompList.TileRemove(removed);
            List<MultiTile> rest = _session.CompList.GetTilesAt(removed.X, removed.Y);
            if (rest.Count > 0)
            {
                _session.SetSelection(new[] { rest[^1] }, add: false);
            }
            else
            {
                _session.ClearSelection();
            }

            _session.AfterEdit();
        }

        private void StackPick()
        {
            MultiTile tile = _session?.SelectedTile;
            if (tile != null)
            {
                _session.SetDrawTile(tile.Id);
            }
        }

        private void GroupZ(int delta)
        {
            if (_session == null || _session.Selection.Count < 2)
            {
                return;
            }

            var tiles = _session.Selection.ToList();
            _session.CompList.TileZMod(tiles, delta);
            _session.AfterEdit(delta > 0 ? tiles.Max(t => t.Z) : null);
        }

        private void GroupSetZ()
        {
            if (_session == null || _session.Selection.Count < 2)
            {
                return;
            }

            int z = (int)_groupZ.Value;
            _session.CompList.TileZSet(_session.Selection.ToList(), z);
            _session.AfterEdit(z);
        }

        private void GroupDelete()
        {
            if (_session == null || _session.Selection.Count < 2)
            {
                return;
            }

            _session.CompList.TileRemoveBatch(_session.Selection.ToList());
            _session.ClearSelection();
            _session.AfterEdit();
        }

        private int IconFor(ushort id)
        {
            if (_iconIndex.TryGetValue(id, out int index))
            {
                return index;
            }

            var icon = new Bitmap(_iconSize, _iconSize);
            Bitmap art = Art.GetStatic(id);
            if (art != null)
            {
                using Graphics g = Graphics.FromImage(icon);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                float scale = Math.Min(1f, Math.Min((float)_iconSize / art.Width, (float)_iconSize / art.Height));
                int w = Math.Max(1, (int)(art.Width * scale));
                int h = Math.Max(1, (int)(art.Height * scale));
                g.DrawImage(art, (_iconSize - w) / 2, _iconSize - h, w, h);
            }

            _icons.Images.Add(icon);
            icon.Dispose();
            index = _icons.Images.Count - 1;
            _iconIndex[id] = index;
            return index;
        }

        // ---------------------------------------------------------------- floors

        /// <summary>
        /// Works out the floors of the multi (same rule as the old editor's BuildLevels): a flat
        /// surface tile at Z 7 means a foundation, with floors at 7, 27, 47 ...; otherwise floors
        /// are at 0, 20, 40 ...
        /// </summary>
        private void RebuildLevels()
        {
            // The session owns the floors, so the list here, the toolbar and Ctrl+PgUp/PgDn agree.
            _levels.Clear();
            if (_session != null)
            {
                foreach (EditorLevel level in _session.Levels)
                {
                    _levels.Add((level.Description, level.Floor, level.Ceiling));
                }
            }

            bool previous = _updating;
            _updating = true;
            try
            {
                _floorList.BeginUpdate();
                _floorList.Items.Clear();
                _floorList.Items.Add("All floors");
                foreach (var level in _levels)
                {
                    _floorList.Items.Add(level.Name);
                }

                _floorList.EndUpdate();
                SelectActiveRow();
            }
            finally
            {
                _updating = previous;
            }
        }

        private void SelectActiveRow()
        {
            int row = Math.Min(_session?.LevelIndex ?? 0, _floorList.Items.Count - 1);
            _floorList.SelectedIndex = row;
        }

        private void OnFloorChosen()
        {
            if (_updating || _session == null || _floorList.SelectedIndex < 0)
            {
                return;
            }

            _session.LevelIndex = _floorList.SelectedIndex;
        }

        private void RefreshView()
        {
            bool previous = _updating;
            _updating = true;
            try
            {
                bool enabled = _session != null;
                _floorList.Enabled = _hideAbove.Enabled = _hideWalls.Enabled = _limit.Enabled = enabled;

                if (_session == null)
                {
                    _showHidden.Enabled = false;
                    _limitBar.Enabled = false;
                    return;
                }

                SelectActiveRow();
                _hideAbove.Checked = _session.HideAboveLevel;
                _firstFloor.Value = _session.FirstFloorZ;
                _editFloors.Text = _session.CustomLevels ? "Edit floors... *" : "Edit floors...";
                _hideWalls.Checked = _session.HideWalls;

                MultiEditorComponentList list = _session.CompList;
                int min = Math.Min(list.ZMin, list.ZMax);
                int max = Math.Max(list.ZMin, list.ZMax);
                _limitBar.Minimum = Math.Min(min, sbyte.MaxValue - 1);
                _limitBar.Maximum = Math.Max(max, _limitBar.Minimum + 1);

                int? limit = _session.HeightLimit;
                _limit.Checked = limit.HasValue;
                _limitBar.Enabled = limit.HasValue;
                if (limit.HasValue)
                {
                    _limitBar.Value = Math.Clamp(limit.Value, _limitBar.Minimum, _limitBar.Maximum);
                }

                _limitLabel.Text = limit.HasValue ? limit.Value.ToString() : "all";

                int hidden = list.HiddenCount;
                _showHidden.Text = $"Show hidden tiles ({hidden})";
                _showHidden.Enabled = hidden > 0;
            }
            finally
            {
                _updating = previous;
            }
        }
    }
}
