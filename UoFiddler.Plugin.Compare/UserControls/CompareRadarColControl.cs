using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls.TileView;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareRadarColControl : UserControl
    {
        public CompareRadarColControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly List<int> _landDisplayIndices = new List<int>();
        private readonly List<int> _itemDisplayIndices = new List<int>();
        private bool _syncingSelection;

        private bool IsLandSection => tabControl.SelectedIndex == 0;

        private TileViewControl ActiveOrgView => IsLandSection ? tileViewOrg : tileViewItemOrg;
        private TileViewControl ActiveSecView => IsLandSection ? tileViewSec : tileViewItemSec;
        private List<int> ActiveIndices      => IsLandSection ? _landDisplayIndices : _itemDisplayIndices;

        private void OnLoad(object sender, EventArgs e)
        {
            if (Options.DarkMode)
            {
                legendSwatchDifferent.BackColor = Color.CornflowerBlue;
            }
            ConfigureTileView(tileViewOrg);
            ConfigureTileView(tileViewSec);
            ConfigureTileView(tileViewItemOrg);
            ConfigureTileView(tileViewItemSec);
            PopulateOrgOnly(isLand: true);

            tileViewSec.SelectedIndices.CollectionChanged += OnLandSecSelectedIndicesChanged;
            tileViewItemSec.SelectedIndices.CollectionChanged += OnItemSecSelectedIndicesChanged;
            contextMenuStripSec.Opening += (s, ev) =>
            {
                int count = ActiveSecView.SelectedIndices.Count;
                copyEntry2To1ToolStripMenuItem.Text = ActiveSecView.ShowCheckBoxes && count > 1
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
            tileViewItemSec.ShowCheckBoxes = chkMultiSelect.Checked;
            tileViewSec.MultiSelect = chkMultiSelect.Checked;
            tileViewItemSec.MultiSelect = chkMultiSelect.Checked;
            if (!chkMultiSelect.Checked)
            {
                tileViewSec.SelectedIndices.Clear();
                tileViewItemSec.SelectedIndices.Clear();
            }
        }

        private void OnLandSecSelectedIndicesChanged(object sender, IndicesCollection.NotifyCollectionChangedEventArgs e)
        {
            MirrorSelection(tileViewSec, tileViewOrg);
        }

        private void OnItemSecSelectedIndicesChanged(object sender, IndicesCollection.NotifyCollectionChangedEventArgs e)
        {
            MirrorSelection(tileViewItemSec, tileViewItemOrg);
        }

        private void MirrorSelection(TileViewControl source, TileViewControl target)
        {
            if (_syncingSelection)
            {
                return;
            }

            _syncingSelection = true;
            try
            {
                target.SelectedIndices.Clear();
                foreach (int idx in source.SelectedIndices)
                {
                    target.SelectedIndices.Add(idx);
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private List<int> GetCopyTargets(TileViewControl secView)
        {
            var sel = secView.SelectedIndices;
            if (sel.Count > 0)
            {
                return sel.ToList();
            }
            if (secView.FocusIndex >= 0)
            {
                return new List<int> { secView.FocusIndex };
            }
            return new List<int>();
        }

        private void OnFilePathChangeEvent()
        {
            _compare.Clear();
            PopulateOrgOnly(IsLandSection);
            ActiveOrgView.Invalidate();
        }

        private void OnTabChanged(object sender, EventArgs e)
        {
            bool isLand = IsLandSection;
            var targetLayout = isLand ? tableLayoutLand : tableLayoutItem;
            if (panelDetail.Parent != targetLayout)
            {
                var prevLayout = isLand ? tableLayoutItem : tableLayoutLand;
                prevLayout.SuspendLayout();
                targetLayout.SuspendLayout();
                prevLayout.Controls.Remove(panelDetail);
                targetLayout.Controls.Add(panelDetail);
                targetLayout.SetCellPosition(panelDetail, new System.Windows.Forms.TableLayoutPanelCellPosition(1, 0));
                targetLayout.ResumeLayout(false);
                prevLayout.ResumeLayout(false);
            }

            if (SecondRadarCol.IsLoaded)
            {
                PopulateSection(isLand, checkBoxShowDiff.Checked);
            }
            else
            {
                PopulateOrgOnly(isLand);
            }
        }

        private void PopulateOrgOnly(bool isLand)
        {
            int start = isLand ? 0x0000 : 0x4000;
            int end   = isLand ? 0x4000 : 0x8000;
            int limit = RadarCol.Colors?.Length ?? end;
            end = Math.Min(end, limit);

            var indices = isLand ? _landDisplayIndices : _itemDisplayIndices;
            indices.Clear();
            for (int i = start; i < end; i++)
            {
                indices.Add(i);
            }

            var orgView = isLand ? tileViewOrg : tileViewItemOrg;
            var secView = isLand ? tileViewSec : tileViewItemSec;
            orgView.VirtualListSize = indices.Count;
            secView.VirtualListSize = 0;
        }

        private void PopulateSection(bool isLand, bool showDiffOnly)
        {
            using (new WaitCursorScope(this))
            {
                int totalCount = Math.Max(RadarCol.Colors?.Length ?? 0,
                                           SecondRadarCol.IsLoaded ? SecondRadarCol.Length : 0);
                if (totalCount == 0)
                {
                    totalCount = 0x8000;
                }

                int start = isLand ? 0x0000 : 0x4000;
                int end   = Math.Min(isLand ? 0x4000 : 0x8000, totalCount);

                var indices = isLand ? _landDisplayIndices : _itemDisplayIndices;
                indices.Clear();
                for (int i = start; i < end; i++)
                {
                    if (!showDiffOnly || IsDifferent(i))
                    {
                        indices.Add(i);
                    }
                }

                var orgView = isLand ? tileViewOrg : tileViewItemOrg;
                var secView = isLand ? tileViewSec : tileViewItemSec;
                orgView.VirtualListSize = indices.Count;
                secView.VirtualListSize = SecondRadarCol.IsLoaded ? indices.Count : 0;
            }
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

        private void OnDrawItemLandOrg(object sender, TileViewControl.DrawTileListItemEventArgs e)
            => DrawListItem(e, _landDisplayIndices[e.Index], isSec: false);

        private void OnDrawItemLandSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
            => DrawListItem(e, _landDisplayIndices[e.Index], isSec: true);

        private void OnDrawItemItemOrg(object sender, TileViewControl.DrawTileListItemEventArgs e)
            => DrawListItem(e, _itemDisplayIndices[e.Index], isSec: false);

        private void OnDrawItemItemSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
            => DrawListItem(e, _itemDisplayIndices[e.Index], isSec: true);

        // Layout (left → right): [checkbox column from TileViewControl, if any] | [color swatch] | [text].
        // Mirrors RadarColorControl so the eye doesn't have to retrain when
        // switching between the two tabs.
        private const int SwatchSize = 12;
        private const int SwatchGap = 4;

        private void DrawListItem(TileViewControl.DrawTileListItemEventArgs e, int idx, bool isSec)
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

            // Color swatch.
            ushort radarHue = GetRadarColor(idx, isSec);
            int swatchX = e.ContentLeft + 2;
            int swatchY = e.Bounds.Y + (e.Bounds.Height - SwatchSize) / 2;
            var swatchRect = new Rectangle(swatchX, swatchY, SwatchSize, SwatchSize);
            using (var swatchBrush = new SolidBrush(HueHelpers.HueToColor(radarHue)))
            {
                e.Graphics.FillRectangle(swatchBrush, swatchRect);
            }
            using (var border = new Pen(SystemColors.ControlDark))
            {
                e.Graphics.DrawRectangle(border, swatchRect);
            }

            // Text — display id is *within* the section (0x0000-based for both
            // items and land), matching RadarColorControl's labelling.
            int displayId = idx < 0x4000 ? idx : idx - 0x4000;
            string name = GetTileName(idx);
            string text = string.IsNullOrEmpty(name)
                ? $"0x{displayId:X4} ({displayId})"
                : $"0x{displayId:X4} ({displayId}) {name}";

            Brush fontBrush = focused
                ? CompareColors.ContrastBrush(Options.TileSelectionColor)
                : SecondRadarCol.IsLoaded && IsDifferent(idx)
                    ? (Options.DarkMode ? Brushes.CornflowerBlue : Brushes.Blue)
                    : Brushes.Gray;

            int textX = swatchX + SwatchSize + SwatchGap;
            float textY = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(text, e.Font).Height) / 2f;
            e.Graphics.DrawString(text, e.Font, fontBrush, new PointF(textX, textY));
        }

        private static ushort GetRadarColor(int idx, bool isSec)
        {
            if (isSec)
            {
                return SecondRadarCol.IsLoaded ? SecondRadarCol.GetColor(idx) : (ushort)0;
            }
            return RadarCol.Colors != null && idx < RadarCol.Colors.Length
                ? RadarCol.Colors[idx]
                : (ushort)0;
        }

        private static string GetTileName(int idx)
        {
            if (idx < 0x4000)
            {
                if (TileData.LandTable != null && idx < TileData.LandTable.Length)
                {
                    return TileData.LandTable[idx].Name;
                }
            }
            else
            {
                int itemId = idx - 0x4000;
                if (TileData.ItemTable != null && itemId < TileData.ItemTable.Length)
                {
                    return TileData.ItemTable[itemId].Name;
                }
            }
            return null;
        }

        private void OnFocusChangedLandOrg(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int idx = _landDisplayIndices[e.FocusedItemIndex];
            if (SecondRadarCol.IsLoaded && tileViewSec.VirtualListSize > 0)
            {
                if (_syncingSelection)
                {
                    return;
                }

                _syncingSelection = true;
                try { tileViewSec.FocusIndex = e.FocusedItemIndex; }
                finally { _syncingSelection = false; }
            }

            UpdateDetailPanel(idx);
        }

        private void OnFocusChangedLandSec(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int idx = _landDisplayIndices[e.FocusedItemIndex];
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

            UpdateDetailPanel(idx);
        }

        private void OnFocusChangedItemOrg(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int idx = _itemDisplayIndices[e.FocusedItemIndex];
            if (SecondRadarCol.IsLoaded && tileViewItemSec.VirtualListSize > 0)
            {
                if (_syncingSelection)
                {
                    return;
                }

                _syncingSelection = true;
                try
                {
                    tileViewItemSec.FocusIndex = e.FocusedItemIndex;
                }
                finally
                {
                    _syncingSelection = false;
                }
            }

            UpdateDetailPanel(idx);
        }

        private void OnFocusChangedItemSec(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int idx = _itemDisplayIndices[e.FocusedItemIndex];
            if (_syncingSelection)
            {
                return;
            }

            _syncingSelection = true;
            try
            {
                tileViewItemOrg.FocusIndex = e.FocusedItemIndex;
            }
            finally
            {
                _syncingSelection = false;
            }

            UpdateDetailPanel(idx);
        }

        private void UpdateDetailPanel(int idx)
        {
            ushort orgColor = RadarCol.Colors != null && idx < RadarCol.Colors.Length
                ? RadarCol.Colors[idx]
                : (ushort)0;

            ushort secColor = SecondRadarCol.IsLoaded ? SecondRadarCol.GetColor(idx) : (ushort)0;

            labelOrgColorValue.Text = $"0x{orgColor:X4} ({orgColor})";
            pictureBoxOrgColor.BackColor = HueHelpers.HueToColor(orgColor);

            if (SecondRadarCol.IsLoaded)
            {
                labelSecColorValue.Text = $"0x{secColor:X4} ({secColor})";
                pictureBoxSecColor.BackColor = HueHelpers.HueToColor(secColor);
            }
            else
            {
                labelSecColorValue.Text = "-";
                pictureBoxSecColor.BackColor = SystemColors.Control;
            }
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select radarcol.mul";
                dialog.Filter = "radarcol.mul|radarcol.mul|All files (*.*)|*.*";
                dialog.FileName = "radarcol.mul";
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

            if (CompareFiles.IsLoadedClientFile(path, "radarcol.mul"))
            {
                MessageBox.Show(
                    "The selected file is the same as the currently loaded radarcol.mul.\n\n" +
                    "Choose a different file to compare against.",
                    "Same File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (new WaitCursorScope(this))
            {
                bool ok = SecondRadarCol.Initialize(path);

                if (!ok)
                {
                    MessageBox.Show("Failed to load the selected radarcol.mul file.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _compare.Clear();
                PopulateSection(IsLandSection, checkBoxShowDiff.Checked);
            }
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (!SecondRadarCol.IsLoaded)
            {
                if (checkBoxShowDiff.Checked)
                {
                    MessageBox.Show("Second RadarCol file is not loaded.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    checkBoxShowDiff.Checked = false;
                }
                return;
            }

            PopulateSection(IsLandSection, checkBoxShowDiff.Checked);
        }

        private bool IsDifferent(int idx)
        {
            if (_compare.TryGetValue(idx, out bool cached))
            {
                return !cached;
            }

            bool same = RadarCol.Colors != null
                        && idx < RadarCol.Colors.Length
                        && RadarCol.Colors[idx] == SecondRadarCol.GetColor(idx);

            _compare[idx] = same;
            return !same;
        }

        private void OnDoubleClickSec(object sender, MouseEventArgs e)
        {
            if (ActiveSecView.ShowCheckBoxes)
            {
                return;
            }
            OnClickCopySelected(sender, e);
        }
        private void OnDoubleClickOrg(object sender, MouseEventArgs e) => OnClickCopy1To2(sender, e);

        private void OnClickCopySelected(object sender, EventArgs e)
        {
            var secView = ActiveSecView;
            var orgView = ActiveOrgView;
            var indices = ActiveIndices;

            var targets = GetCopyTargets(secView);
            if (targets.Count == 0)
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                int lastIdx = -1;
                bool changed = false;

                foreach (int focusIdx in targets)
                {
                    if (focusIdx < 0 || focusIdx >= indices.Count)
                    {
                        continue;
                    }

                    int idx = indices[focusIdx];
                    CopySecToOrg(idx);
                    lastIdx = idx;
                    changed = true;
                }

                if (checkBoxShowDiff.Checked && changed)
                {
                    foreach (int displayIdx in targets.OrderByDescending(x => x))
                    {
                        if (displayIdx >= 0 && displayIdx < indices.Count)
                        {
                            indices.RemoveAt(displayIdx);
                        }
                    }
                    orgView.VirtualListSize = indices.Count;
                    secView.VirtualListSize = indices.Count;
                }
                else
                {
                    secView.SelectedIndices.Clear();
                }

                orgView.Invalidate();
                secView.Invalidate();
                if (lastIdx >= 0)
                {
                    UpdateDetailPanel(lastIdx);
                }
            }
        }

        private void OnClickCopy1To2(object sender, EventArgs e)
        {
            var orgView = ActiveOrgView;
            if (orgView.FocusIndex < 0)
            {
                return;
            }

            CopyOrgToSec(ActiveIndices[orgView.FocusIndex]);
        }

        private void OnClickCopyAllDiff(object sender, EventArgs e)
        {
            if (!SecondRadarCol.IsLoaded)
            {
                return;
            }

            bool isLand = IsLandSection;
            int start   = isLand ? 0x0000 : 0x4000;
            int end     = Math.Min(isLand ? 0x4000 : 0x8000,
                                    Math.Max(RadarCol.Colors?.Length ?? 0, SecondRadarCol.Length));
            bool changed = false;

            for (int i = start; i < end; i++)
            {
                if (IsDifferent(i))
                {
                    CopySecToOrg(i); changed = true;
                }
            }

            if (changed)
            {
                if (checkBoxShowDiff.Checked)
                {
                    PopulateSection(isLand, showDiffOnly: true);
                }

                ActiveOrgView.Invalidate();
                ActiveSecView.Invalidate();
            }
        }

        private void CopySecToOrg(int idx)
        {
            ushort value = SecondRadarCol.GetColor(idx);
            if (idx < 0x4000)
            {
                RadarCol.SetLandColor(idx, value);
            }
            else
            {
                RadarCol.SetItemColor(idx - 0x4000, value);
            }

            Options.ChangedUltimaClass["RadarCol"] = true;
            _compare[idx] = true;
        }

        private void CopyOrgToSec(int idx)
        {
            MessageBox.Show(
                "The second file is a read-only reference source.\n" +
                "Use 'Copy Entry 2 to 1' to transfer from second to original.",
                "Read-only",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
