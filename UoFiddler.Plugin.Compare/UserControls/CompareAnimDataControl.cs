using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls.TileView;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareAnimDataControl : UserControl
    {
        public CompareAnimDataControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly List<int> _displayIndices = new List<int>();
        private bool _syncingSelection;

        private void OnLoad(object sender, EventArgs e)
        {
            if (Options.DarkMode)
            {
                legendSwatchDifferent.BackColor = Color.CornflowerBlue;
            }
            ConfigureTileView(tileViewOrg);
            ConfigureTileView(tileViewSec);
            PopulateOrgList();

            tileViewSec.SelectedIndices.CollectionChanged += OnSecSelectedIndicesChanged;
            contextMenuStrip1.Opening += (s, ev) =>
            {
                int count = tileViewSec.SelectedIndices.Count;
                copyEntryToolStripMenuItem.Text = tileViewSec.ShowCheckBoxes && count > 1
                    ? $"Copy {count} Entries to left"
                    : "Copy Entry to left";
            };

            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
        }

        // TileViewControl exposes TileSize/Margin/Padding/Border with DesignerSerializationVisibility.Hidden,
        // so VS strips them when re-saving the .Designer.cs. Apply the intended values here so they survive.
        private static void ConfigureTileView(TileViewControl tv)
        {
            tv.TileSize = new Size(tv.TileSize.Width, 20);
            tv.TileMargin = new Padding(0);
            tv.TilePadding = new Padding(0);
            tv.TileBorderWidth = 0f;
            tv.TileFocusColor = Color.Transparent;
            tv.TileHighlightColor = Options.TileSelectionColor;
            tv.TileHighLightOpacity = 0.4;
        }

        private void OnChangeMultiSelect(object sender, EventArgs e)
        {
            tileViewSec.ShowCheckBoxes = chkMultiSelect.Checked;
            tileViewSec.MultiSelect = chkMultiSelect.Checked;
            if (!chkMultiSelect.Checked)
            {
                tileViewSec.SelectedIndices.Clear();
            }
        }

        private void OnSecSelectedIndicesChanged(object sender, IndicesCollection.NotifyCollectionChangedEventArgs e)
        {
            if (_syncingSelection)
            {
                return;
            }

            _syncingSelection = true;
            try
            {
                tileViewOrg.SelectedIndices.Clear();
                foreach (int idx in tileViewSec.SelectedIndices)
                {
                    tileViewOrg.SelectedIndices.Add(idx);
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private List<int> GetCopyTargets()
        {
            var sel = tileViewSec.SelectedIndices;
            if (sel.Count > 0)
            {
                return sel.ToList();
            }
            if (tileViewSec.FocusIndex >= 0)
            {
                return new List<int> { tileViewSec.FocusIndex };
            }
            return new List<int>();
        }

        private void OnFilePathChangeEvent()
        {
            _compare.Clear();
            PopulateOrgList();
        }

        private void PopulateOrgList()
        {
            _displayIndices.Clear();
            foreach (int id in Animdata.AnimData.Keys.OrderBy(k => k))
            {
                _displayIndices.Add(id);
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = 0;
        }

        private void OnTileViewSizeChanged(object sender, EventArgs e)
        {
            var tv = (TileViewControl)sender;
            int w = tv.DisplayRectangle.Width;
            if (w > 0 && tv.TileSize.Width != w)
            {
                tv.TileSize = new Size(w, tv.TileSize.Height);
            }
        }

        private void OnDrawItemOrg(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawListItem(e, _displayIndices[e.Index]);
        }

        private void OnDrawItemSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawListItem(e, _displayIndices[e.Index]);
        }

        private void DrawListItem(TileViewControl.DrawTileListItemEventArgs e, int id)
        {
            bool focused = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            if (focused)
            {
                using var highlightBrush = new SolidBrush(Options.TileSelectionColor);
                e.Graphics.FillRectangle(highlightBrush, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            Brush fontBrush = focused
                ? CompareColors.ContrastBrush(Options.TileSelectionColor)
                : GetEntryBrush(id);
            string text = $"0x{id:X4} ({id})";
            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(text, e.Font).Height) / 2f;
            e.Graphics.DrawString(text, e.Font, fontBrush, new PointF(e.ContentLeft + 4, y));
        }

        private Brush GetEntryBrush(int id)
        {
            bool inOrg = Animdata.AnimData.ContainsKey(id);
            bool inSec = SecondAnimdata.IsLoaded && SecondAnimdata.GetAnimData(id) != null;

            if (SecondAnimdata.IsLoaded)
            {
                if (inOrg && !inSec)
                {
                    return Brushes.Orange;
                }

                if (!inOrg && inSec)
                {
                    return Brushes.Green;
                }

                if (inOrg && inSec && !Compare(id))
                {
                    return Options.DarkMode ? Brushes.CornflowerBlue : Brushes.Blue;
                }
            }

            return Brushes.Gray;
        }

        private void OnFocusChangedOrg(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int id = _displayIndices[e.FocusedItemIndex];

            if (SecondAnimdata.IsLoaded && tileViewSec.VirtualListSize > 0)
            {
                if (_syncingSelection)
                {
                    return;
                }

                _syncingSelection = true;
                try
                {
                    tileViewSec.FocusIndex = e.FocusedItemIndex;
                }
                finally
                {
                    _syncingSelection = false;
                }
            }

            UpdateDetailPanel(id);
            tileViewOrg.Invalidate();
        }

        private void OnFocusChangedSec(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int id = _displayIndices[e.FocusedItemIndex];

            if (_syncingSelection)
            {
                return;
            }

            _syncingSelection = true;
            try
            {
                tileViewOrg.FocusIndex = e.FocusedItemIndex;
            }
            finally
            {
                _syncingSelection = false;
            }

            UpdateDetailPanel(id);
            tileViewSec.Invalidate();
        }

        private void UpdateDetailPanel(int id)
        {
            var orgEntry = Animdata.GetAnimData(id);
            var secEntry = SecondAnimdata.GetAnimData(id);

            if (orgEntry != null)
            {
                labelOrgFrameCount.Text = orgEntry.FrameCount.ToString();
                labelOrgFrameInterval.Text = orgEntry.FrameInterval.ToString();
                labelOrgFrameStart.Text = orgEntry.FrameStart.ToString();
                labelOrgFrameData.Text = FormatFrameData(orgEntry.FrameData, orgEntry.FrameCount);
            }
            else
            {
                labelOrgFrameCount.Text = "-";
                labelOrgFrameInterval.Text = "-";
                labelOrgFrameStart.Text = "-";
                labelOrgFrameData.Text = "-";
            }

            if (secEntry != null)
            {
                labelSecFrameCount.Text = secEntry.FrameCount.ToString();
                labelSecFrameInterval.Text = secEntry.FrameInterval.ToString();
                labelSecFrameStart.Text = secEntry.FrameStart.ToString();
                labelSecFrameData.Text = FormatFrameData(secEntry.FrameData, secEntry.FrameCount);
            }
            else
            {
                labelSecFrameCount.Text = "-";
                labelSecFrameInterval.Text = "-";
                labelSecFrameStart.Text = "-";
                labelSecFrameData.Text = "-";
            }
        }

        private static string FormatFrameData(sbyte[] data, byte count)
        {
            if (data == null || count == 0)
            {
                return "-";
            }

            int len = Math.Min(count, data.Length);
            return string.Join(", ", data.Take(len));
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select animdata.mul";
                dialog.Filter = "animdata.mul|animdata.mul|All files (*.*)|*.*";
                dialog.FileName = "animdata.mul";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondFile.Text = dialog.FileName;
                }
            }
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            string path = textBoxSecondFile.Text?.Trim();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (CompareFiles.IsLoadedClientFile(path, "animdata.mul"))
            {
                MessageBox.Show(
                    "The selected file is the same as the currently loaded animdata.mul.\n\n" +
                    "Choose a different file to compare against.",
                    "Same File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!SecondAnimdata.Initialize(path))
            {
                MessageBox.Show("Failed to load the selected animdata.mul file.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _compare.Clear();
            RefreshLists();
        }

        private void RefreshLists()
        {
            var allIds = Animdata.AnimData.Keys
                .Union(SecondAnimdata.GetKeys())
                .OrderBy(k => k)
                .ToList();

            _displayIndices.Clear();
            foreach (int id in allIds)
            {
                _displayIndices.Add(id);
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = _displayIndices.Count;
            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (!SecondAnimdata.IsLoaded)
            {
                if (checkBoxShowDiff.Checked)
                {
                    MessageBox.Show("Second AnimData file is not loaded.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    checkBoxShowDiff.Checked = false;
                }
                return;
            }

            using (new WaitCursorScope(this))
            {
                var allIds = Animdata.AnimData.Keys
                    .Union(SecondAnimdata.GetKeys())
                    .OrderBy(k => k);

                _displayIndices.Clear();
                foreach (int id in allIds)
                {
                    if (!checkBoxShowDiff.Checked || !Compare(id))
                    {
                        _displayIndices.Add(id);
                    }
                }

                tileViewOrg.VirtualListSize = _displayIndices.Count;
                tileViewSec.VirtualListSize = _displayIndices.Count;
            }
        }

        private bool Compare(int id)
        {
            if (_compare.TryGetValue(id, out bool cached))
            {
                return cached;
            }

            var e1 = Animdata.GetAnimData(id);
            var e2 = SecondAnimdata.GetAnimData(id);

            if (e1 == null && e2 == null)
            {
                _compare[id] = true;
                return true;
            }

            if (e1 == null || e2 == null)
            {
                _compare[id] = false;
                return false;
            }

            bool same = e1.FrameCount == e2.FrameCount
                     && e1.FrameInterval == e2.FrameInterval
                     && e1.FrameStart == e2.FrameStart
                     && e1.FrameData.SequenceEqual(e2.FrameData);

            _compare[id] = same;
            return same;
        }

        private void OnDoubleClickSec(object sender, MouseEventArgs e)
        {
            if (tileViewSec.ShowCheckBoxes)
            {
                return;
            }
            OnClickCopySelected(sender, e);
        }

        private void OnClickCopySelected(object sender, EventArgs e)
        {
            var targets = GetCopyTargets();
            if (targets.Count == 0)
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                int lastId = -1;
                bool changed = false;

                foreach (int focusIdx in targets)
                {
                    if (focusIdx < 0 || focusIdx >= _displayIndices.Count)
                    {
                        continue;
                    }

                    int id = _displayIndices[focusIdx];
                    CopyEntry(id);
                    lastId = id;
                    changed = true;
                }

                if (checkBoxShowDiff.Checked && changed)
                {
                    foreach (int idx in targets.OrderByDescending(x => x))
                    {
                        if (idx >= 0 && idx < _displayIndices.Count)
                        {
                            _displayIndices.RemoveAt(idx);
                        }
                    }
                    tileViewOrg.VirtualListSize = _displayIndices.Count;
                    tileViewSec.VirtualListSize = _displayIndices.Count;
                }
                else
                {
                    tileViewSec.SelectedIndices.Clear();
                }

                tileViewOrg.Invalidate();
                tileViewSec.Invalidate();
                if (lastId >= 0)
                {
                    UpdateDetailPanel(lastId);
                }
            }
        }

        private void OnClickCopyAllDiff(object sender, EventArgs e)
        {
            if (!SecondAnimdata.IsLoaded)
            {
                return;
            }

            bool changed = false;
            var allIds = Animdata.AnimData.Keys.Union(SecondAnimdata.GetKeys()).ToList();
            foreach (int id in allIds)
            {
                if (!Compare(id) && SecondAnimdata.GetAnimData(id) != null)
                {
                    CopyEntry(id);
                    changed = true;
                }
            }

            if (changed)
            {
                if (checkBoxShowDiff.Checked)
                {
                    OnChangeShowDiff(sender, e);
                }

                tileViewOrg.Invalidate();
                tileViewSec.Invalidate();
            }
        }

        private void OnClickCopyAddedOnly(object sender, EventArgs e)
        {
            if (!SecondAnimdata.IsLoaded)
            {
                return;
            }

            bool changed = false;
            foreach (int id in SecondAnimdata.GetKeys())
            {
                if (!Animdata.AnimData.ContainsKey(id))
                {
                    CopyEntry(id);
                    changed = true;
                }
            }

            if (changed)
            {
                RefreshLists();
            }
        }

        private void CopyEntry(int id)
        {
            var src = SecondAnimdata.GetAnimData(id);
            if (src == null)
            {
                return;
            }

            Animdata.AnimData[id] = new Animdata.AnimdataEntry(
                (sbyte[])src.FrameData.Clone(),
                src.Unknown,
                src.FrameCount,
                src.FrameInterval,
                src.FrameStart);

            Options.ChangedUltimaClass["Animdata"] = true;
            _compare[id] = true;
        }
    }
}
