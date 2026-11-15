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
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The layers of the design, top layer first: visibility, the active layer, tile counts and
    /// the layer operations of the session.
    /// </summary>
    internal sealed partial class LayersPanel : UserControl
    {
        private readonly Font _boldFont;
        private EditorSession _session;
        private bool _updating;
        private bool _editing;
        private ContextMenuStrip _menu;

        public LayersPanel()
        {
            InitializeComponent();

            _boldFont = new Font(Font, FontStyle.Bold);
            Disposed += LayersPanel_Disposed;

            Bind(null);
        }

        private void LayersPanel_Disposed(object sender, EventArgs e)
        {
            if (_session != null)
            {
                _session.LayersChanged -= Session_LayersChanged;
                _session = null;
            }

            _menu?.Dispose();
            _boldFont.Dispose();
        }

        /// <summary>Attaches to a session; null detaches and shows the empty state.</summary>
        public void Bind(EditorSession session)
        {
            if (_session != null)
            {
                _session.LayersChanged -= Session_LayersChanged;
            }

            _session = session;

            if (_session != null)
            {
                _session.LayersChanged += Session_LayersChanged;
            }

            Refill();
        }

        private void Session_LayersChanged()
        {
            RefreshRows();
        }

        private void List_Resize(object sender, EventArgs e)
        {
            columnName.Width = Math.Max(40, _list.ClientSize.Width - columnCount.Width);
        }

        private void List_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_updating || _session == null || e.Item.Tag is not TileLayer layer)
            {
                return;
            }

            _session.SetLayerVisible(layer, e.Item.Checked);
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_updating || _session == null)
            {
                return;
            }

            if (_list.SelectedItems.Count == 0)
            {
                // Nothing selected is also what the list reports on the way from one row to the
                // next, so the active row is put back only once the click is over and still none is.
                BeginInvoke(() =>
                {
                    if (!IsDisposed && _session != null && _list.SelectedItems.Count == 0)
                    {
                        RefreshRows();
                    }
                });
                return;
            }

            if (_list.SelectedItems[0].Tag is TileLayer layer)
            {
                _session.ActiveLayer = layer;
            }
        }

        private void List_DoubleClick(object sender, EventArgs e)
        {
            Rename();
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                Rename();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void List_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            _editing = true;
        }

        private void List_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            _editing = false;
            if (e.Label == null)
            {
                return;
            }

            if (_session == null || string.IsNullOrWhiteSpace(e.Label) || _list.Items[e.Item].Tag is not TileLayer layer)
            {
                e.CancelEdit = true;
                return;
            }

            e.CancelEdit = !_session.RenameLayer(layer, e.Label);
        }

        private void List_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right || _session == null || _list.GetItemAt(e.X, e.Y) is not { Tag: TileLayer layer } item)
            {
                return;
            }

            item.Selected = true;
            ShowMenu(layer, item, e.Location);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            _session?.AddLayer();
        }

        private void DuplicateButton_Click(object sender, EventArgs e)
        {
            Duplicate(_session?.ActiveLayer);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            Delete(_session?.ActiveLayer);
        }

        private void MergeButton_Click(object sender, EventArgs e)
        {
            MergeDown(_session?.ActiveLayer);
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            if (_session?.ActiveLayer != null)
            {
                _session.MoveLayer(_session.ActiveLayer, 1);
            }
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            if (_session?.ActiveLayer != null)
            {
                _session.MoveLayer(_session.ActiveLayer, -1);
            }
        }

        // ---------------------------------------------------------------- actions

        private void Rename()
        {
            if (_session != null && _list.SelectedItems.Count > 0)
            {
                _list.SelectedItems[0].BeginEdit();
            }
        }

        private void Duplicate(TileLayer layer)
        {
            if (layer != null)
            {
                _session.DuplicateLayer(layer);
            }
        }

        private void MergeDown(TileLayer layer)
        {
            if (layer != null)
            {
                _session.MergeLayerDown(layer);
            }
        }

        private void Delete(TileLayer layer)
        {
            if (_session == null || layer == null || _session.Layers.Count < 2)
            {
                return;
            }

            int count = _session.CompList.LayerTileCount(layer);
            if (count > 0)
            {
                string text = $"Delete layer '{layer.Name}' with its {count:N0} tiles?\r\n\r\nYou can undo this.";
                if (MessageBox.Show(this, text, "Delete layer", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                {
                    return;
                }
            }

            _session.RemoveLayer(layer);
        }

        private void ShowMenu(TileLayer layer, ListViewItem item, Point location)
        {
            _menu?.Dispose();
            _menu = new ContextMenuStrip();
            bool bottom = _session.Layers.Count > 0 && _session.Layers[0] == layer;
            _menu.Items.Add("Rename", null, (_, _) => item.BeginEdit());
            _menu.Items.Add("Duplicate", null, (_, _) => Duplicate(layer));
            _menu.Items.Add("Delete", null, (_, _) => Delete(layer)).Enabled = _session.Layers.Count > 1;
            _menu.Items.Add("Merge down", null, (_, _) => MergeDown(layer)).Enabled = !bottom;
            _menu.Items.Add(new ToolStripSeparator());
            _menu.Items.Add("Show only this layer", null, (_, _) => _session.SoloLayer(layer));
            _menu.Items.Add("Select tiles on this layer", null, (_, _) => _session.SelectLayer(layer));
            _menu.Items.Add("Move selection to this layer", null, (_, _) => _session.MoveSelectionToLayer(layer)).Enabled = _session.Selection.Count > 0;
            _menu.Show(_list, location);
        }

        // ---------------------------------------------------------------- list

        /// <summary>Updates the rows in place when the layers are the same ones in the same order, else rebuilds.</summary>
        private void RefreshRows()
        {
            int count = _session?.Layers.Count ?? 0;
            bool same = _list.Items.Count == count;
            for (int i = 0; same && i < count; ++i)
            {
                same = ReferenceEquals(_list.Items[i].Tag, _session.Layers[count - 1 - i]);
            }

            if (!same)
            {
                Refill();
                return;
            }

            bool previous = _updating;
            _updating = true;
            try
            {
                UpdateRows();
                UpdateButtons();
            }
            finally
            {
                _updating = previous;
            }
        }

        private void Refill()
        {
            bool previous = _updating;
            _updating = true;
            try
            {
                _list.BeginUpdate();
                _list.Items.Clear();
                if (_session != null)
                {
                    for (int i = _session.Layers.Count - 1; i >= 0; --i)
                    {
                        var item = new ListViewItem(string.Empty) { Tag = _session.Layers[i] };
                        item.SubItems.Add(string.Empty);
                        _list.Items.Add(item);
                    }

                    UpdateRows();
                }

                _list.EndUpdate();
                UpdateButtons();
            }
            finally
            {
                _updating = previous;
            }
        }

        // Caller holds _updating.
        private void UpdateRows()
        {
            Dictionary<TileLayer, int> counts = _session.CompList.LayerTileCounts();
            foreach (ListViewItem item in _list.Items)
            {
                var layer = (TileLayer)item.Tag;
                bool active = layer == _session.ActiveLayer;

                if (item.Text != layer.Name && !_editing)
                {
                    item.Text = layer.Name;
                }

                string tiles = counts.GetValueOrDefault(layer).ToString("N0");
                if (item.SubItems[1].Text != tiles)
                {
                    item.SubItems[1].Text = tiles;
                }

                if (item.Checked != layer.Visible)
                {
                    item.Checked = layer.Visible;
                }

                Font font = active ? _boldFont : _list.Font;
                if (!ReferenceEquals(item.Font, font))
                {
                    item.Font = font;
                }

                if (item.Selected != active)
                {
                    item.Selected = active;
                }
            }
        }

        private void UpdateButtons()
        {
            bool has = _session != null;
            int count = _session?.Layers.Count ?? 0;
            int index = has ? IndexOfActive() : -1;

            _addButton.Enabled = has;
            _duplicateButton.Enabled = has;
            _deleteButton.Enabled = has && count > 1;
            _mergeButton.Enabled = has && index > 0;
            _downButton.Enabled = has && index > 0;
            _upButton.Enabled = has && index >= 0 && index < count - 1;
        }

        private int IndexOfActive()
        {
            for (int i = 0; i < _session.Layers.Count; ++i)
            {
                if (_session.Layers[i] == _session.ActiveLayer)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
