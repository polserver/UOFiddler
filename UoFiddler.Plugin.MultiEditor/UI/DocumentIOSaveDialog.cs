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
using Ultima;
using Ultima.Uop;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// Picks the multi slot to save into: every slot of the client's multi table is listed, free
    /// ones marked as such, with a preview of what a used slot holds now.
    /// </summary>
    internal sealed partial class DocumentIOSaveDialog : Form
    {
        private readonly Dictionary<int, string> _names;
        private readonly bool[] _used;

        /// <summary>Slots the client's MultiCollection.uop holds a multi in (all false without that file).</summary>
        private readonly bool[] _uop;
        private readonly List<int> _rows = new List<int>();
        private readonly List<RadioButton> _filters = new List<RadioButton>();
        private int _filter; // 0 all, 1 free, 2 used
        private bool _syncing;

        public DocumentIOSaveDialog(Dictionary<int, string> names, int? suggestedId)
        {
            _names = names;
            int count = Multis.MaximumMultiIndex;
            _used = new bool[count];
            _uop = new bool[count];

            // multi.mul and MultiCollection.uop do not hold the same multis, so a slot is only free
            // when neither has one there.
            bool hasUop = Multis.HasUopFile;
            int free = 0;
            for (int i = 0; i < count; i++)
            {
                _used[i] = Multis.GetComponents(i) != MultiComponentList.Empty;
                _uop[i] = hasUop && Multis.GetUopComponents(i) != MultiComponentList.Empty;
                if (IsFree(i))
                {
                    free++;
                }
            }

            InitializeComponent();

            // Which slots are listed. A new design usually wants a free slot.
            filterFree.Text = $"Free ({free})";
            filterUsed.Text = $"Used ({count - free})";
            _filters.Add(filterAll);
            _filters.Add(filterFree);
            _filters.Add(filterUsed);

            // Which file the multi table is written to. The slot itself always changes in memory; the
            // files hold every multi, so writing one writes the whole table in that container.
            // They sit in a panel of their own: radio buttons sharing a parent are one group, and the slot
            // filter buttons are radio buttons too.

            // Preselected: the container this client keeps its multis in, unless writing it would drop
            // multis. The editable table comes from multi.mul, and a modern client's MultiCollection.uop
            // can hold more than that; then multi.mul is the safe choice and the UOP one says why.
            bool clientUsesUop = ClientFileSaver.ClientUsesUop(FileType.MultiCollection);
            bool uopLosesData = clientUsesUop && ClientFileSaver.Preflight(FileType.MultiCollection, ContainerFormat.Uop).Count > 0;
            _writeUop.Checked = clientUsesUop && !uopLosesData;
            _writeMul.Checked = !_writeUop.Checked;
            if (uopLosesData)
            {
                _writeUop.Text += " (drops multis)";
                var tip = new ToolTip();
                tip.SetToolTip(_writeUop, "This client's MultiCollection.uop holds more multis than its multi.mul, which is what is edited here.\n" +
                                          "Writing the UOP saves only the multi.mul ones. You are asked before it happens.");
                _writeNone.Left += 90;
            }

            // A design that came from a slot goes back to it by default; a new one to the first free slot.
            _syncing = true;
            _filter = suggestedId.HasValue ? 0 : 1;
            _filters[_filter].Checked = true;
            _syncing = false;
            Populate(suggestedId ?? FirstFreeFrom(0));
        }

        /// <summary>The container to write the multi table to after the slot is set, or null to leave the files for later.</summary>
        public ContainerFormat? WriteFormat => _writeNone.Checked ? null : _writeUop.Checked ? ContainerFormat.Uop : ContainerFormat.Mul;

        /// <summary>The slot chosen, valid after the dialog returned OK.</summary>
        public int SelectedId { get; private set; } = -1;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // The list can only scroll to the chosen slot once it has its window.
            int id = CurrentId();
            if (id >= 0)
            {
                Select(id);
            }

            _id.Focus();
            _id.SelectAll();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _preview.Image?.Dispose();
            _preview.Image = null;
            base.OnFormClosed(e);
        }

        private void Id_TextChanged(object sender, EventArgs e)
        {
            OnIdTyped();
        }

        private void NextFree_Click(object sender, EventArgs e)
        {
            GoToNextFree();
        }

        private void FilterButton_CheckedChanged(object sender, EventArgs e)
        {
            var button = (RadioButton)sender;
            if (button.Checked && !_syncing)
            {
                _filter = _filters.IndexOf(button);
                Populate(CurrentId());
            }
        }

        private void List_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = MakeItem(_rows[e.ItemIndex]);
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnRowSelected();
        }

        private void List_DoubleClick(object sender, EventArgs e)
        {
            Accept();
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private bool IsFree(int id) => !_used[id] && !_uop[id];

        private ListViewItem MakeItem(int id)
        {
            var item = new ListViewItem($"{id,5} (0x{id:X})");
            if (_used[id])
            {
                MultiComponentList multi = Multis.GetComponents(id);
                item.SubItems.Add(_names.TryGetValue(id, out string name) ? name : string.Empty);
                item.SubItems.Add($"{multi.Width}x{multi.Height}, {multi.SortedTiles.Length} tiles");
            }
            else if (_uop[id])
            {
                MultiComponentList multi = Multis.GetUopComponents(id);
                item.SubItems.Add(_names.TryGetValue(id, out string name) ? name : string.Empty);
                item.SubItems.Add($"UOP only: {multi.Width}x{multi.Height}");
            }
            else
            {
                item.SubItems.Add(string.Empty);
                item.SubItems.Add("free");
                item.ForeColor = SystemColors.GrayText;
            }

            return item;
        }

        private int FirstFreeFrom(int start)
        {
            for (int i = Math.Max(0, start); i < _used.Length; i++)
            {
                if (IsFree(i))
                {
                    return i;
                }
            }

            return -1;
        }

        private void GoToNextFree()
        {
            int next = FirstFreeFrom(CurrentId() + 1);
            if (next < 0)
            {
                next = FirstFreeFrom(0);
            }

            if (next >= 0)
            {
                Select(next);
            }
        }

        private int CurrentId()
        {
            return Utils.ConvertStringToInt(_id.Text.Trim(), out int id, 0, _used.Length - 1) ? id : -1;
        }

        /// <summary>Fills the list for the chosen filter and selects a slot (the nearest listed one when it is filtered out).</summary>
        private void Populate(int select)
        {
            _rows.Clear();
            for (int i = 0; i < _used.Length; i++)
            {
                if (_filter == 0 || (_filter == 1) == IsFree(i))
                {
                    _rows.Add(i);
                }
            }

            _list.VirtualListSize = _rows.Count;
            _list.Invalidate();
            if (select >= 0)
            {
                Select(select);
            }
            else
            {
                ShowSlot(-1);
            }
        }

        private void Select(int id)
        {
            _syncing = true;
            _id.Text = id.ToString();
            _syncing = false;

            int row = _rows.BinarySearch(id);
            _list.SelectedIndices.Clear();
            if (row >= 0)
            {
                _list.SelectedIndices.Add(row);
                _list.EnsureVisible(row);
            }

            ShowSlot(id);
        }

        private void OnIdTyped()
        {
            if (_syncing)
            {
                return;
            }

            int id = CurrentId();
            int row = id >= 0 ? _rows.BinarySearch(id) : -1;
            _syncing = true;
            _list.SelectedIndices.Clear();
            if (row >= 0)
            {
                _list.SelectedIndices.Add(row);
                _list.EnsureVisible(row);
            }

            _syncing = false;
            ShowSlot(id);
        }

        private void OnRowSelected()
        {
            if (_syncing || _list.SelectedIndices.Count == 0)
            {
                return;
            }

            int id = _rows[_list.SelectedIndices[0]];
            _syncing = true;
            _id.Text = id.ToString();
            _syncing = false;
            ShowSlot(id);
        }

        private void ShowSlot(int id)
        {
            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();

            _ok.Enabled = id >= 0;
            if (id < 0)
            {
                _info.Text = $"Enter an id between 0 and {_used.Length - 1}.";
                return;
            }

            if (IsFree(id))
            {
                _info.Text = $"Slot {id} (0x{id:X}) is free.";
                return;
            }

            MultiComponentList multi = _used[id] ? Multis.GetComponents(id) : Multis.GetUopComponents(id);
            _info.Text = _used[id]
                ? $"Slot {id} (0x{id:X}) holds a {multi.Width}x{multi.Height} multi: saving replaces it."
                : $"Slot {id} (0x{id:X}) is empty in multi.mul, but MultiCollection.uop has a {multi.Width}x{multi.Height} multi there.";
            try
            {
                _preview.Image = multi.GetImage();
            }
            catch (Exception)
            {
                // A broken multi must not take the dialog down; the preview just stays empty.
            }
        }

        private void Accept()
        {
            int id = CurrentId();
            if (id < 0)
            {
                return;
            }

            string question = _used[id] ? $"Multi {id} (0x{id:X}) already holds a design. Overwrite it?"
                : _uop[id] ? $"Multi {id} (0x{id:X}) is empty in multi.mul, but the client's MultiCollection.uop has a multi with that id.\r\n" +
                             "Saving here gives the id a second meaning. Use it anyway?"
                : null;
            if (question != null && MessageBox.Show(this, question, "Multi Editor",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK)
            {
                return;
            }

            SelectedId = id;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
