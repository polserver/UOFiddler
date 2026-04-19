using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Ultima;
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
            PopulateOrgOnly(isLand: true);
            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
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
            Cursor.Current = Cursors.WaitCursor;
            try
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
            finally
            {
                Cursor.Current = Cursors.Default;
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
            => DrawListItem(e, _landDisplayIndices[e.Index]);

        private void OnDrawItemLandSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
            => DrawListItem(e, _landDisplayIndices[e.Index]);

        private void OnDrawItemItemOrg(object sender, TileViewControl.DrawTileListItemEventArgs e)
            => DrawListItem(e, _itemDisplayIndices[e.Index]);

        private void OnDrawItemItemSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
            => DrawListItem(e, _itemDisplayIndices[e.Index]);

        private void DrawListItem(DrawItemEventArgs e, int idx)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            Brush fontBrush = SecondRadarCol.IsLoaded && IsDifferent(idx)
                ? Brushes.Blue
                : Brushes.Gray;

            string section = idx < 0x4000 ? "Land" : "Item";
            string text = $"0x{idx:X4}  [{section}]";
            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(text, e.Font).Height) / 2f;
            e.Graphics.DrawString(text, e.Font, fontBrush, new PointF(4, y));
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
            pictureBoxOrgColor.BackColor = UshortToColor(orgColor);

            if (SecondRadarCol.IsLoaded)
            {
                labelSecColorValue.Text = $"0x{secColor:X4} ({secColor})";
                pictureBoxSecColor.BackColor = UshortToColor(secColor);
            }
            else
            {
                labelSecColorValue.Text = "-";
                pictureBoxSecColor.BackColor = SystemColors.Control;
            }
        }

        private static Color UshortToColor(ushort value)
        {
            if (value == 0)
            {
                return Color.Black;
            }

            int b = (value & 0x7C00) >> 10;
            int g = (value & 0x03E0) >> 5;
            int r = value & 0x001F;
            return Color.FromArgb((r << 3) | (r >> 2), (g << 3) | (g >> 2), (b << 3) | (b >> 2));
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

            Cursor.Current = Cursors.WaitCursor;
            bool ok = SecondRadarCol.Initialize(path);
            Cursor.Current = Cursors.Default;

            if (!ok)
            {
                MessageBox.Show("Failed to load the selected radarcol.mul file.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _compare.Clear();
            PopulateSection(IsLandSection, checkBoxShowDiff.Checked);
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

        private void OnDoubleClickSec(object sender, MouseEventArgs e) => OnClickCopySelected(sender, e);
        private void OnDoubleClickOrg(object sender, MouseEventArgs e) => OnClickCopy1To2(sender, e);

        private void OnClickCopySelected(object sender, EventArgs e)
        {
            var secView = ActiveSecView;
            if (secView.FocusIndex < 0)
            {
                return;
            }

            int idx = ActiveIndices[secView.FocusIndex];
            CopySecToOrg(idx);

            if (checkBoxShowDiff.Checked)
            {
                int displayIdx = ActiveIndices.IndexOf(idx);
                if (displayIdx >= 0)
                {
                    ActiveIndices.RemoveAt(displayIdx);
                    ActiveOrgView.VirtualListSize = ActiveIndices.Count;
                    secView.VirtualListSize       = ActiveIndices.Count;
                }
            }

            ActiveOrgView.Invalidate();
            secView.Invalidate();
            UpdateDetailPanel(idx);
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
