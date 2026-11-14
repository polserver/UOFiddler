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
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>One row of the pick list: a client multi (with id) or one multi inside a container file.</summary>
    internal sealed record MultiPickEntry(int? Id, string Name, string Source, MultiComponentList Components);

    /// <summary>
    /// Lists multis with a filter box and a preview of the highlighted one.
    /// Double-click or Enter accepts.
    /// </summary>
    internal sealed partial class DocumentIOPickDialog : Form
    {
        private readonly List<MultiPickEntry> _entries;
        private readonly List<RadioButton> _sourceButtons = new List<RadioButton>();
        private string _sourceFilter;

        /// <param name="showSource">Shows the File column: which file each multi comes from.</param>
        /// <param name="clientSources">Adds the All / multi.mul / UOP buttons, for the client's multis.</param>
        public DocumentIOPickDialog(string title, List<MultiPickEntry> entries, bool showSource, bool clientSources)
        {
            _entries = entries;

            InitializeComponent();
            Text = title;

            // Which file the multis come from, as push buttons: all, multi.mul or the UOP collection.
            // A client without MultiCollection.uop (or without multi.mul) has that button greyed out.
            if (clientSources)
            {
                sourceMul.Enabled = entries.Exists(e => e.Source == "multi.mul");
                sourceUop.Enabled = entries.Exists(e => e.Source == "UOP");
                _sourceButtons.Add(sourceAll);
                _sourceButtons.Add(sourceMul);
                _sourceButtons.Add(sourceUop);
            }
            else
            {
                Controls.Remove(sourceAll);
                Controls.Remove(sourceMul);
                Controls.Remove(sourceUop);
                sourceAll.Dispose();
                sourceMul.Dispose();
                sourceUop.Dispose();
            }

            if (!showSource)
            {
                _list.Columns.Remove(columnHeaderFile);
                columnHeaderName.Width = 260;
            }
            else if (!clientSources)
            {
                // File names are longer than "multi.mul" or "UOP".
                columnHeaderFile.Width = 140;
                columnHeaderName.Width = 210;
                columnHeaderId.Width = 0;
            }

            Populate();
        }

        /// <summary>The accepted rows, in list order; empty when cancelled.</summary>
        public List<MultiPickEntry> SelectedEntries { get; } = new List<MultiPickEntry>();

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _filter.Focus();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _preview.Image?.Dispose();
            _preview.Image = null;
            base.OnFormClosed(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Arrow keys in the filter box move the list selection, so the list needs no focus.
            if (_filter.Focused && (keyData == Keys.Down || keyData == Keys.Up) && _list.Items.Count > 0)
            {
                int index = _list.SelectedIndices.Count > 0 ? _list.SelectedIndices[0] : -1;
                index = Math.Clamp(index + (keyData == Keys.Down ? 1 : -1), 0, _list.Items.Count - 1);
                _list.Items[index].Selected = true;
                _list.EnsureVisible(index);
                return true;
            }

            if (_filter.Focused && keyData == Keys.Enter)
            {
                Accept();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            Populate();
        }

        private void SourceButton_CheckedChanged(object sender, EventArgs e)
        {
            var button = (RadioButton)sender;
            if (button.Checked)
            {
                _sourceFilter = button.Tag as string;
                Populate();
            }
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowPreview();
        }

        private void List_DoubleClick(object sender, EventArgs e)
        {
            Accept();
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Accept();
            }
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Accept();
        }

        private void Populate()
        {
            string text = _filter.Text.Trim();
            int number = 0;
            bool numeric = text.Length > 0 && Utils.ConvertStringToInt(text, out number);
            string source = _sourceFilter;

            _list.BeginUpdate();
            try
            {
                _list.Items.Clear();
                foreach (MultiPickEntry entry in _entries)
                {
                    bool match = text.Length == 0
                        || entry.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                        || (numeric && entry.Id == number);
                    if (!match || (source != null && entry.Source != source))
                    {
                        continue;
                    }

                    string idText = entry.Id.HasValue ? $"{entry.Id.Value,5} (0x{entry.Id.Value:X})" : string.Empty;
                    var item = new ListViewItem(idText) { Tag = entry };
                    item.SubItems.Add(entry.Name);
                    if (_list.Columns.Count > 2)
                    {
                        item.SubItems.Add(entry.Source ?? string.Empty);
                    }

                    _list.Items.Add(item);
                }
            }
            finally
            {
                _list.EndUpdate();
            }

            if (_list.Items.Count > 0)
            {
                _list.Items[0].Selected = true;
            }
            else
            {
                ShowPreview();
            }
        }

        private void ShowPreview()
        {
            Image old = _preview.Image;
            _preview.Image = null;
            old?.Dispose();

            MultiPickEntry entry = Current();
            _ok.Enabled = entry != null;
            if (entry == null)
            {
                _info.Text = string.Empty;
                return;
            }

            MultiComponentList comp = entry.Components;
            _info.Text = _list.SelectedItems.Count > 1
                ? $"{_list.SelectedItems.Count} selected: each opens in its own tab"
                : $"{comp.Width}x{comp.Height}, {comp.SortedTiles.Length} tiles";
            _ok.Text = _list.SelectedItems.Count > 1 ? $"Open {_list.SelectedItems.Count}" : "Open";
            try
            {
                _preview.Image = comp.GetImage();
            }
            catch (Exception)
            {
                // A broken multi must not take the dialog down; the preview just stays empty.
            }
        }

        private MultiPickEntry Current()
        {
            return _list.SelectedItems.Count > 0 ? _list.SelectedItems[0].Tag as MultiPickEntry : null;
        }

        private void Accept()
        {
            if (_list.SelectedItems.Count == 0)
            {
                return;
            }

            SelectedEntries.Clear();
            foreach (ListViewItem item in _list.SelectedItems)
            {
                if (item.Tag is MultiPickEntry entry)
                {
                    SelectedEntries.Add(entry);
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
