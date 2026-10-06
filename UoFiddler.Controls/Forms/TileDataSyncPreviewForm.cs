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
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileDataSyncPreviewForm : Form
    {
        private sealed class Row
        {
            public ListViewItem Item;
            public TileDataSyncChange Change;
            public bool Checked;
        }

        private readonly List<Row> _rows = new();
        private readonly Dictionary<ListViewItem, Row> _rowByItem = new();
        private Func<string, string?>? _localizationGetter;

        public IReadOnlyList<TileDataSyncChange> AcceptedChanges
        {
            get
            {
                var list = new List<TileDataSyncChange>(_rows.Count);
                foreach (var row in _rows)
                {
                    if (row.Checked)
                    {
                        list.Add(row.Change);
                    }
                }

                return list;
            }
        }

        private bool _handlerHooked;
        private bool _suppressEvents;

        public TileDataSyncPreviewForm(IReadOnlyList<TileDataSyncChange> changes)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            var items = new ListViewItem[changes.Count];
            for (int i = 0; i < changes.Count; i++)
            {
                var change = changes[i];
                var item = new ListViewItem(change.Kind.ToString())
                {
                    Checked = true,
                    Tag = change,
                };
                item.SubItems.Add(change.Number.ToString());
                item.SubItems.Add(change.OldText ?? string.Empty);
                item.SubItems.Add(change.NewText ?? string.Empty);
                items[i] = item;

                var row = new Row { Item = item, Change = change, Checked = true };
                _rows.Add(row);
                _rowByItem[item] = row;
            }

            changesListView.BeginUpdate();
            changesListView.Items.AddRange(items);
            changesListView.EndUpdate();

            UpdateSummary();
        }

        /// <summary>
        /// Set localization for TileDataSyncPreviewForm
        /// </summary>
        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// Apply localization to all UI elements
        /// </summary>
        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // 窗口标题
            this.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.Title") ?? "Sync from TileData — Preview";

            // 表格列头
            actionColumn.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.ColumnAction") ?? "Action";
            numberColumn.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.ColumnNumber") ?? "Number";
            oldTextColumn.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.ColumnCurrentText") ?? "Current text";
            newTextColumn.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.ColumnNewText") ?? "New text";

            // 按钮
            checkAllButton.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.CheckAllButton") ?? "Check all";
            uncheckAllButton.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.UncheckAllButton") ?? "Uncheck all";
            uncheckRemovesButton.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.UncheckRemovesButton") ?? "Uncheck removes";
            applyButton.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.ApplyButton") ?? "Apply";
            cancelButton.Text = _localizationGetter("Forms.TileDataSyncPreviewForm.CancelButton") ?? "Cancel";

            // 重新更新 Summary 标签以应用汉化的模板
            UpdateSummary();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            Cursor.Current = Cursors.Default;

            if (_handlerHooked)
            {
                return;
            }

            _handlerHooked = true;
            changesListView.ItemChecked += OnItemChecked;
        }

        private void OnItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_suppressEvents)
            {
                return;
            }

            if (!_rowByItem.TryGetValue(e.Item, out var row))
            {
                return;
            }

            try
            {
                row.Checked = e.Item.Checked;
            }
            catch
            {
                // Reading Checked can throw if the item's native state is mid-recreation.
                // Fall back to inverting our cached state — the event only fires on actual change.
                row.Checked = !row.Checked;
            }

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            if (summaryLabel == null)
            {
                return;
            }

            int totalAdd = 0, totalUpdate = 0, totalRemove = 0;
            int selAdd = 0, selUpdate = 0, selRemove = 0;

            foreach (var row in _rows)
            {
                switch (row.Change.Kind)
                {
                    case TileDataSyncKind.Add:
                        totalAdd++;

                        if (row.Checked)
                        {
                            selAdd++;
                        }

                        break;
                    case TileDataSyncKind.Update:
                        totalUpdate++;

                        if (row.Checked)
                        {
                            selUpdate++;
                        }

                        break;
                    case TileDataSyncKind.Remove:
                        totalRemove++;

                        if (row.Checked)
                        {
                            selRemove++;
                        }

                        break;
                }
            }

            // 使用汉化的 Summary 模板
            string template = _localizationGetter?.Invoke("Forms.TileDataSyncPreviewForm.WillApplyTemplate")
                ?? "Will apply:    Add {0}/{1}    Update {2}/{3}    Remove {4}/{5}";
            summaryLabel.Text = string.Format(template, selAdd, totalAdd, selUpdate, totalUpdate, selRemove, totalRemove);
        }

        private void SetCheckedForAll(bool value)
        {
            using (new WaitCursorScope(this))
            {
                _suppressEvents = true;
                changesListView.BeginUpdate();
                try
                {
                    foreach (var row in _rows)
                    {
                        row.Item.Checked = value;
                        row.Checked = value;
                    }
                }
                finally
                {
                    changesListView.EndUpdate();
                    _suppressEvents = false;
                }
                UpdateSummary();
            }
        }

        private void SetCheckedForKind(TileDataSyncKind kind, bool value)
        {
            using (new WaitCursorScope(this))
            {
                _suppressEvents = true;
                changesListView.BeginUpdate();
                try
                {
                    foreach (var row in _rows)
                    {
                        if (row.Change.Kind == kind)
                        {
                            row.Item.Checked = value;
                            row.Checked = value;
                        }
                    }
                }
                finally
                {
                    changesListView.EndUpdate();
                    _suppressEvents = false;
                }
                UpdateSummary();
            }
        }

        private void OnCheckAll(object sender, EventArgs e)
        {
            SetCheckedForAll(true);
        }

        private void OnUncheckAll(object sender, EventArgs e)
        {
            SetCheckedForAll(false);
        }

        private void OnUncheckRemoves(object sender, EventArgs e)
        {
            SetCheckedForKind(TileDataSyncKind.Remove, false);
        }
    }
}
