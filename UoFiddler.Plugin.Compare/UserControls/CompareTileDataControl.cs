using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.UserControls.TileView;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareTileDataControl : UserControl
    {
        public CompareTileDataControl()
        {
            InitializeComponent();
        }

        // ── State ────────────────────────────────────────────────────────────────

        private SecondTileData _secondTileData;
        private readonly Dictionary<int, bool> _compareLand = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> _compareItem = new Dictionary<int, bool>();
        private readonly TileDataCompareOptions _options = new TileDataCompareOptions();
        private readonly List<int> _landDisplayIndices = new List<int>();
        private readonly List<int> _itemDisplayIndices = new List<int>();

        private bool IsLandTab => tabControl.SelectedIndex == 0;

        // ── Flag rows ─────────────────────────────────────────────────────────────

        private static readonly (string Name, TileFlag Flag)[] MeaningfulFlags =
        {
            ("Background",   TileFlag.Background),
            ("Weapon",       TileFlag.Weapon),
            ("Transparent",  TileFlag.Transparent),
            ("Translucent",  TileFlag.Translucent),
            ("Wall",         TileFlag.Wall),
            ("Damaging",     TileFlag.Damaging),
            ("Impassable",   TileFlag.Impassable),
            ("Wet",          TileFlag.Wet),
            ("Unknown1",     TileFlag.Unknown1),
            ("Surface",      TileFlag.Surface),
            ("Bridge",       TileFlag.Bridge),
            ("Generic",      TileFlag.Generic),
            ("Window",       TileFlag.Window),
            ("NoShoot",      TileFlag.NoShoot),
            ("ArticleA",     TileFlag.ArticleA),
            ("ArticleAn",    TileFlag.ArticleAn),
            ("ArticleThe",   TileFlag.ArticleThe),
            ("Foliage",      TileFlag.Foliage),
            ("PartialHue",   TileFlag.PartialHue),
            ("NoHouse",      TileFlag.NoHouse),
            ("Map",          TileFlag.Map),
            ("Container",    TileFlag.Container),
            ("Wearable",     TileFlag.Wearable),
            ("LightSource",  TileFlag.LightSource),
            ("Animation",    TileFlag.Animation),
            ("HoverOver",    TileFlag.HoverOver),
            ("NoDiagonal",   TileFlag.NoDiagonal),
            ("Armor",        TileFlag.Armor),
            ("Roof",         TileFlag.Roof),
            ("Door",         TileFlag.Door),
            ("StairBack",    TileFlag.StairBack),
            ("StairRight",   TileFlag.StairRight),
            ("AlphaBlend",   TileFlag.AlphaBlend),
            ("UseNewArt",    TileFlag.UseNewArt),
            ("ArtUsed",      TileFlag.ArtUsed),
            ("NoShadow",     TileFlag.NoShadow),
            ("PixelBleed",   TileFlag.PixelBleed),
            ("PlayAnimOnce", TileFlag.PlayAnimOnce),
            ("MultiMovable", TileFlag.MultiMovable),
        };

        private CheckBox[] _landFlagOrgChecks;
        private CheckBox[] _landFlagSecChecks;
        private Label[] _landFlagLabels;
        private TableLayoutPanel _landFlagTlp;

        private CheckBox[] _itemFlagOrgChecks;
        private CheckBox[] _itemFlagSecChecks;
        private Label[] _itemFlagLabels;
        private TableLayoutPanel _itemFlagTlp;

        // ── Load ─────────────────────────────────────────────────────────────────

        private const int ListBoxWidth = 280;

        private void OnLoad(object sender, EventArgs e)
        {
            SetupDetailPanels();
            PopulateItemOrg();
            PopulateLandOrg();
            BuildRulesPanel();
            SetInnerSplitterPositions();
        }

        private void SetupDetailPanels()
        {
            // Land detail — 2 rows (Name, TextureId); Flags shown per-flag below
            SetupDetailRow(tlpLandDetail, 0, lblLandName, "Name", txtLandOrgName, txtLandSecName);
            SetupDetailRow(tlpLandDetail, 1, lblLandTexId, "Texture ID", txtLandOrgTexId, txtLandSecTexId);
            SetupFlagsPanel(panelLandDetail,
                out _landFlagOrgChecks, out _landFlagSecChecks, out _landFlagLabels, out _landFlagTlp);
            // Push tlpLandDetail to the back so it gets the highest z-order and docks topmost
            panelLandDetail.Controls.SetChildIndex(tlpLandDetail, panelLandDetail.Controls.Count - 1);

            // Item detail — 12 rows (no Flags row); Flags shown per-flag below
            SetupDetailRow(tlpItemDetail, 0, lblItemName, "Name", txtItemOrgName, txtItemSecName);
            SetupDetailRow(tlpItemDetail, 1, lblItemAnim, "Animation", txtItemOrgAnim, txtItemSecAnim);
            SetupDetailRow(tlpItemDetail, 2, lblItemWeight, "Weight", txtItemOrgWeight, txtItemSecWeight);
            SetupDetailRow(tlpItemDetail, 3, lblItemQuality, "Quality", txtItemOrgQuality, txtItemSecQuality);
            SetupDetailRow(tlpItemDetail, 4, lblItemQty, "Quantity", txtItemOrgQty, txtItemSecQty);
            SetupDetailRow(tlpItemDetail, 5, lblItemHue, "Hue", txtItemOrgHue, txtItemSecHue);
            SetupDetailRow(tlpItemDetail, 6, lblItemStack, "StackOffset", txtItemOrgStack, txtItemSecStack);
            SetupDetailRow(tlpItemDetail, 7, lblItemValue, "Value", txtItemOrgValue, txtItemSecValue);
            SetupDetailRow(tlpItemDetail, 8, lblItemHeight, "Height", txtItemOrgHeight, txtItemSecHeight);
            SetupDetailRow(tlpItemDetail, 9, lblItemMisc, "MiscData", txtItemOrgMisc, txtItemSecMisc);
            SetupDetailRow(tlpItemDetail, 10, lblItemUnk2, "Unk2", txtItemOrgUnk2, txtItemSecUnk2);
            SetupDetailRow(tlpItemDetail, 11, lblItemUnk3, "Unk3", txtItemOrgUnk3, txtItemSecUnk3);
            SetupFlagsPanel(panelItemDetail,
                out _itemFlagOrgChecks, out _itemFlagSecChecks, out _itemFlagLabels, out _itemFlagTlp);
            // Push tlpItemDetail to the back so it gets the highest z-order and docks topmost
            panelItemDetail.Controls.SetChildIndex(tlpItemDetail, panelItemDetail.Controls.Count - 1);
        }

        private void SetupFlagsPanel(
            Panel detailPanel,
            out CheckBox[] orgChecks,
            out CheckBox[] secChecks,
            out Label[] labels,
            out TableLayoutPanel flagTlp)
        {
            int count = MeaningfulFlags.Length;
            orgChecks = new CheckBox[count];
            secChecks = new CheckBox[count];
            labels = new Label[count];

            // Header row
            var header = new Panel { Dock = DockStyle.Top, Height = 22, BackColor = SystemColors.ControlLight };
            var hLbl = new Label { Text = "Flag", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0) };
            var hSec = new Label { Text = "Sec", Width = 44, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleCenter };
            var hOrg = new Label { Text = "Org", Width = 44, Dock = DockStyle.Right, TextAlign = ContentAlignment.MiddleCenter };
            header.Controls.Add(hLbl);
            header.Controls.Add(hSec);
            header.Controls.Add(hOrg);

            // Scrollable area
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };

            // Flag rows TLP
            var tlp = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = count,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top,
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));

            tlp.SuspendLayout();
            for (int i = 0; i < count; i++)
            {
                tlp.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));

                var lbl = new Label
                {
                    Text = MeaningfulFlags[i].Name,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    Visible = false,
                };
                var orgChk = new CheckBox
                {
                    AutoCheck = false,
                    Dock = DockStyle.Fill,
                    CheckAlign = ContentAlignment.MiddleCenter,
                    Text = string.Empty,
                    Visible = false,
                };
                var secChk = new CheckBox
                {
                    AutoCheck = false,
                    Dock = DockStyle.Fill,
                    CheckAlign = ContentAlignment.MiddleCenter,
                    Text = string.Empty,
                    Visible = false,
                };

                tlp.Controls.Add(lbl, 0, i);
                tlp.Controls.Add(orgChk, 1, i);
                tlp.Controls.Add(secChk, 2, i);

                labels[i] = lbl;
                orgChecks[i] = orgChk;
                secChecks[i] = secChk;
            }
            tlp.ResumeLayout(false);

            flagTlp = tlp;
            scroll.Controls.Add(tlp);
            // Add Fill first, then Top — highest index docks first, so header ends up at top
            detailPanel.Controls.Add(scroll);
            detailPanel.Controls.Add(header);
        }

        private void UpdateFlagRows(
            CheckBox[] orgChecks, CheckBox[] secChecks, Label[] labels, TableLayoutPanel flagTlp,
            TileFlag orgFlags, TileFlag secFlags,
            bool hasOrg, bool hasSec,
            bool compareFlags)
        {
            Color def = SystemColors.Window;

            var scrollPanel = flagTlp.Parent;
            scrollPanel?.SuspendLayout();
            flagTlp.SuspendLayout();
            try
            {
                for (int i = 0; i < MeaningfulFlags.Length; i++)
                {
                    TileFlag f = MeaningfulFlags[i].Flag;

                    bool orgHas = hasOrg && (orgFlags & f) != 0;
                    bool secHas = hasSec && (secFlags & f) != 0;

                    // Hide rows where neither side has the flag set
                    bool visible = orgHas || secHas;
                    labels[i].Visible = visible;
                    orgChecks[i].Visible = visible;
                    secChecks[i].Visible = visible;
                    flagTlp.RowStyles[i].Height = visible ? 22F : 0F;

                    if (!visible)
                    {
                        continue;
                    }

                    orgChecks[i].Checked = orgHas;
                    secChecks[i].Checked = secHas;

                    bool ignored = (_options.IgnoredFlags & f) != 0;
                    bool differs = hasOrg && hasSec && compareFlags && !ignored && orgHas != secHas;

                    Color bg = differs ? DiffColor : def;
                    labels[i].BackColor = bg;
                    orgChecks[i].BackColor = bg;
                    secChecks[i].BackColor = bg;
                }
            }
            finally
            {
                flagTlp.ResumeLayout(true);
                scrollPanel?.ResumeLayout(false);
            }
        }

        private static void SetupDetailRow(
            System.Windows.Forms.TableLayoutPanel tlp, int row,
            System.Windows.Forms.Label lbl, string labelText,
            System.Windows.Forms.TextBox orgBox, System.Windows.Forms.TextBox secBox)
        {
            lbl.Text = labelText;
            lbl.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

            orgBox.ReadOnly = true;
            orgBox.Dock = System.Windows.Forms.DockStyle.Fill;

            secBox.ReadOnly = true;
            secBox.Dock = System.Windows.Forms.DockStyle.Fill;

            tlp.Controls.Add(lbl, 0, row);
            tlp.Controls.Add(orgBox, 1, row);
            tlp.Controls.Add(secBox, 2, row);
        }

        private void SetInnerSplitterPositions()
        {
            SetInnerSplitter(splitLandInner);
            SetInnerSplitter(splitItemInner);
        }

        private static void SetInnerSplitter(System.Windows.Forms.SplitContainer sc)
        {
            int dist = sc.Width - ListBoxWidth - sc.SplitterWidth;
            if (dist > sc.Panel1MinSize)
            {
                sc.SplitterDistance = dist;
            }
        }

        private void PopulateLandOrg()
        {
            _landDisplayIndices.Clear();
            for (int i = 0; i < TileData.LandTable.Length; i++)
            {
                _landDisplayIndices.Add(i);
            }

            tileViewLandOrg.VirtualListSize = _landDisplayIndices.Count;
            tileViewLandSec.VirtualListSize = 0;
        }

        private void PopulateItemOrg()
        {
            _itemDisplayIndices.Clear();
            for (int i = 0; i < TileData.ItemTable.Length; i++)
            {
                _itemDisplayIndices.Add(i);
            }

            tileViewItemOrg.VirtualListSize = _itemDisplayIndices.Count;
            tileViewItemSec.VirtualListSize = 0;
        }

        // ── Browse / Load second file ─────────────────────────────────────────────

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the UO files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            string path = textBoxSecondDir.Text?.Trim();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            string tileFile = Path.Combine(path, "tiledata.mul");
            string artFile = Path.Combine(path, "art.mul");
            string artIdx = Path.Combine(path, "artidx.mul");

            if (!File.Exists(tileFile) || !File.Exists(artFile) || !File.Exists(artIdx))
            {
                MessageBox.Show("Could not find tiledata.mul, art.mul and artidx.mul in the selected directory.",
                    "Missing Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SecondArt.SetFileIndex(artIdx, artFile);

            _secondTileData = new SecondTileData();
            _secondTileData.Initialize(tileFile, SecondArt.IsUOAHS());

            InvalidateCompareCache();
            RefreshBothLists();
        }

        // ── Compare cache ────────────────────────────────────────────────────────

        private void InvalidateCompareCache()
        {
            _compareLand.Clear();
            _compareItem.Clear();
            tileViewLandOrg.Invalidate();
            tileViewLandSec.Invalidate();
            tileViewItemOrg.Invalidate();
            tileViewItemSec.Invalidate();
        }

        private bool CompareLand(int index)
        {
            if (_secondTileData == null)
            {
                return true;
            }

            if (_compareLand.TryGetValue(index, out bool cached))
            {
                return cached;
            }

            if (index >= TileData.LandTable.Length || index >= _secondTileData.LandTable.Length)
            {
                _compareLand[index] = false;
                return false;
            }

            var left = TileData.LandTable[index];
            var right = _secondTileData.LandTable[index];

            bool same = true;

            if (_options.LandCompareName && same)
            {
                same = left.Name == right.Name;
            }

            if (_options.LandCompareTextureId && same)
            {
                same = left.TextureId == right.TextureId;
            }

            if (_options.LandCompareFlags && same)
            {
                TileFlag lf = left.Flags & ~_options.IgnoredFlags;
                TileFlag rf = right.Flags & ~_options.IgnoredFlags;
                same = lf == rf;
            }

            _compareLand[index] = same;
            return same;
        }

        private bool CompareItem(int index)
        {
            if (_secondTileData == null)
            {
                return true;
            }

            if (_compareItem.TryGetValue(index, out bool cached))
            {
                return cached;
            }

            if (index >= TileData.ItemTable.Length || index >= _secondTileData.ItemTable.Length)
            {
                _compareItem[index] = false;
                return false;
            }

            var left = TileData.ItemTable[index];
            var right = _secondTileData.ItemTable[index];

            bool same = true;

            if (_options.ItemCompareName && same)
            {
                same = left.Name == right.Name;
            }

            if (_options.ItemCompareFlags && same)
            {
                TileFlag lf = left.Flags & ~_options.IgnoredFlags;
                TileFlag rf = right.Flags & ~_options.IgnoredFlags;
                same = lf == rf;
            }

            if (_options.ItemCompareAnimation && same)
            {
                same = left.Animation == right.Animation;
            }

            if (_options.ItemCompareWeight && same)
            {
                same = left.Weight == right.Weight;
            }

            if (_options.ItemCompareQuality && same)
            {
                same = left.Quality == right.Quality;
            }

            if (_options.ItemCompareQuantity && same)
            {
                same = left.Quantity == right.Quantity;
            }

            if (_options.ItemCompareHue && same)
            {
                same = left.Hue == right.Hue;
            }

            if (_options.ItemCompareStackingOffset && same)
            {
                same = left.StackingOffset == right.StackingOffset;
            }

            if (_options.ItemCompareValue && same)
            {
                same = left.Value == right.Value;
            }

            if (_options.ItemCompareHeight && same)
            {
                same = left.Height == right.Height;
            }

            if (_options.ItemCompareMiscData && same)
            {
                same = left.MiscData == right.MiscData;
            }

            if (_options.ItemCompareUnk2 && same)
            {
                same = left.Unk2 == right.Unk2;
            }

            if (_options.ItemCompareUnk3 && same)
            {
                same = left.Unk3 == right.Unk3;
            }

            _compareItem[index] = same;
            return same;
        }

        // ── List population / refresh ─────────────────────────────────────────────

        private void RefreshBothLists()
        {
            RefreshLandLists();
            RefreshItemLists();
        }

        private void RefreshLandLists()
        {
            bool diffOnly = chkShowDiff.Checked && _secondTileData != null;
            int orgLen = TileData.LandTable.Length;
            int secLen = _secondTileData?.LandTable.Length ?? 0;
            int total = Math.Max(orgLen, secLen);

            _landDisplayIndices.Clear();
            for (int i = 0; i < total; i++)
            {
                if (!diffOnly || !CompareLand(i))
                {
                    _landDisplayIndices.Add(i);
                }
            }

            tileViewLandOrg.VirtualListSize = _landDisplayIndices.Count;
            tileViewLandSec.VirtualListSize = _landDisplayIndices.Count;
        }

        private void RefreshItemLists()
        {
            bool diffOnly = chkShowDiff.Checked && _secondTileData != null;
            int orgLen = TileData.ItemTable.Length;
            int secLen = _secondTileData?.ItemTable.Length ?? 0;
            int total = Math.Max(orgLen, secLen);

            _itemDisplayIndices.Clear();
            for (int i = 0; i < total; i++)
            {
                if (!diffOnly || !CompareItem(i))
                {
                    _itemDisplayIndices.Add(i);
                }
            }

            tileViewItemOrg.VirtualListSize = _itemDisplayIndices.Count;
            tileViewItemSec.VirtualListSize = _itemDisplayIndices.Count;
        }

        // ── Show Differences Only ─────────────────────────────────────────────────

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (chkShowDiff.Checked && _secondTileData == null)
            {
                MessageBox.Show("Second tiledata file is not loaded.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                chkShowDiff.Checked = false;
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            if (IsLandTab)
            {
                RefreshLandLists();
            }
            else
            {
                RefreshItemLists();
            }
            Cursor.Current = Cursors.Default;
        }

        private void OnTabChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (chkShowDiff.Checked && _secondTileData != null)
                {
                    if (IsLandTab)
                    {
                        RefreshLandLists();
                    }
                    else
                    {
                        RefreshItemLists();
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        // ── Owner-draw helpers ────────────────────────────────────────────────────

        private void OnTileViewSizeChanged(object sender, EventArgs e)
        {
            var tv = (TileViewControl)sender;
            int w = tv.DisplayRectangle.Width;
            if (w > 0 && tv.TileSize.Width != w)
            {
                tv.TileSize = new Size(w, tv.TileSize.Height);
            }
        }

        // Land org
        private void OnDrawItemLandOrg(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawLandItem(e, _landDisplayIndices[e.Index], isSecondary: false);
        }

        // Land sec
        private void OnDrawItemLandSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawLandItem(e, _landDisplayIndices[e.Index], isSecondary: true);
        }

        private void DrawLandItem(DrawItemEventArgs e, int i, bool isSecondary)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            Brush brush = GetLandBrush(i, isSecondary);
            string label = GetLandLabel(i, isSecondary);

            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(label, e.Font).Height) / 2f;
            e.Graphics.DrawString(label, e.Font, brush, new PointF(4, y));
        }

        private Brush GetLandBrush(int i, bool isSecondary)
        {
            bool inOrg = i < TileData.LandTable.Length;
            bool inSec = _secondTileData != null && i < _secondTileData.LandTable.Length;

            if (_secondTileData != null)
            {
                if (inOrg && !inSec)
                {
                    return Brushes.Orange;
                }

                if (!inOrg && inSec)
                {
                    return Brushes.Green;
                }

                if (!CompareLand(i))
                {
                    return Brushes.Blue;
                }
            }

            return Brushes.Gray;
        }

        private string GetLandLabel(int i, bool isSecondary)
        {
            bool inOrg = i < TileData.LandTable.Length;
            bool inSec = _secondTileData != null && i < _secondTileData.LandTable.Length;

            if (isSecondary)
            {
                if (!inSec)
                {
                    return $"0x{i:X4} ({i}) <entry doesn't exist>";
                }

                return $"0x{i:X4} ({i}) {_secondTileData.LandTable[i].Name}";
            }
            else
            {
                if (!inOrg)
                {
                    return $"0x{i:X4} ({i}) <entry doesn't exist>";
                }

                return $"0x{i:X4} ({i}) {TileData.LandTable[i].Name}";
            }
        }

        // Item org
        private void OnDrawItemItemOrg(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawItemEntry(e, _itemDisplayIndices[e.Index], isSecondary: false);
        }

        // Item sec
        private void OnDrawItemItemSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawItemEntry(e, _itemDisplayIndices[e.Index], isSecondary: true);
        }

        private void DrawItemEntry(DrawItemEventArgs e, int i, bool isSecondary)
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            Brush brush = GetItemBrush(i);
            string label = GetItemLabel(i, isSecondary);

            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(label, e.Font).Height) / 2f;
            e.Graphics.DrawString(label, e.Font, brush, new PointF(4, y));
        }

        private Brush GetItemBrush(int i)
        {
            bool inOrg = i < TileData.ItemTable.Length;
            bool inSec = _secondTileData != null && i < _secondTileData.ItemTable.Length;

            if (_secondTileData != null)
            {
                if (inOrg && !inSec)
                {
                    return Brushes.Orange;
                }

                if (!inOrg && inSec)
                {
                    return Brushes.Green;
                }

                if (!CompareItem(i))
                {
                    return Brushes.Blue;
                }
            }
            else if (!inOrg)
            {
                return Brushes.Red;
            }

            return Brushes.Gray;
        }

        private string GetItemLabel(int i, bool isSecondary)
        {
            bool inOrg = i < TileData.ItemTable.Length;
            bool inSec = _secondTileData != null && i < _secondTileData.ItemTable.Length;

            if (isSecondary)
            {
                if (!inSec)
                {
                    return $"0x{i:X4} ({i}) <entry doesn't exist>";
                }

                return $"0x{i:X4} ({i}) {_secondTileData.ItemTable[i].Name}";
            }
            else
            {
                if (!inOrg)
                {
                    return $"0x{i:X4} ({i}) <entry doesn't exist>";
                }

                return $"0x{i:X4} ({i}) {TileData.ItemTable[i].Name}";
            }
        }

        // ── Selection sync + detail panels ───────────────────────────────────────

        private bool _syncingSelection;

        private void SyncTileView(TileViewControl target, List<int> displayIndices, int tileId)
        {
            if (_syncingSelection || target.VirtualListSize == 0)
            {
                return;
            }

            _syncingSelection = true;
            try
            {
                int displayIdx = displayIndices.IndexOf(tileId);
                if (displayIdx >= 0 && displayIdx < target.VirtualListSize)
                {
                    target.FocusIndex = displayIdx;
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private void OnFocusChangedLandOrg(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int tileId = _landDisplayIndices[e.FocusedItemIndex];
            SyncTileView(tileViewLandSec, _landDisplayIndices, tileId);
            UpdateLandDetail(tileId);
        }

        private void OnFocusChangedLandSec(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int tileId = _landDisplayIndices[e.FocusedItemIndex];
            SyncTileView(tileViewLandOrg, _landDisplayIndices, tileId);
            UpdateLandDetail(tileId);
        }

        private void OnFocusChangedItemOrg(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int tileId = _itemDisplayIndices[e.FocusedItemIndex];
            SyncTileView(tileViewItemSec, _itemDisplayIndices, tileId);
            UpdateItemDetail(tileId);
        }

        private void OnFocusChangedItemSec(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int tileId = _itemDisplayIndices[e.FocusedItemIndex];
            SyncTileView(tileViewItemOrg, _itemDisplayIndices, tileId);
            UpdateItemDetail(tileId);
        }

        // ── Detail panel population ───────────────────────────────────────────────

        private static readonly Color DiffColor = Color.LightYellow;

        private void ResetLandDetailHighlights()
        {
            Color def = SystemColors.Window;
            txtLandOrgName.BackColor = def;
            txtLandOrgTexId.BackColor = def;
            txtLandSecName.BackColor = def;
            txtLandSecTexId.BackColor = def;
        }

        private void UpdateLandDetail(int id)
        {
            ResetLandDetailHighlights();

            bool inOrg = id < TileData.LandTable.Length;
            bool inSec = _secondTileData != null && id < _secondTileData.LandTable.Length;

            TileFlag orgLandFlags = default;
            TileFlag secLandFlags = default;

            if (inOrg)
            {
                var d = TileData.LandTable[id];
                txtLandOrgName.Text = d.Name;
                txtLandOrgTexId.Text = d.TextureId.ToString();
                orgLandFlags = d.Flags;
            }
            else
            {
                txtLandOrgName.Text = string.Empty;
                txtLandOrgTexId.Text = string.Empty;
            }

            if (inSec)
            {
                var d = _secondTileData.LandTable[id];
                txtLandSecName.Text = d.Name;
                txtLandSecTexId.Text = d.TextureId.ToString();
                secLandFlags = d.Flags;
            }
            else
            {
                txtLandSecName.Text = string.Empty;
                txtLandSecTexId.Text = string.Empty;
            }

            if (inOrg && inSec)
            {
                var org = TileData.LandTable[id];
                var sec = _secondTileData.LandTable[id];

                if (_options.LandCompareName && org.Name != sec.Name)
                {
                    txtLandOrgName.BackColor = DiffColor;
                    txtLandSecName.BackColor = DiffColor;
                }

                if (_options.LandCompareTextureId && org.TextureId != sec.TextureId)
                {
                    txtLandOrgTexId.BackColor = DiffColor;
                    txtLandSecTexId.BackColor = DiffColor;
                }
            }

            UpdateFlagRows(_landFlagOrgChecks, _landFlagSecChecks, _landFlagLabels, _landFlagTlp,
                orgLandFlags, secLandFlags, inOrg, inSec, _options.LandCompareFlags);
        }

        private void ResetItemDetailHighlights()
        {
            Color def = SystemColors.Window;
            txtItemOrgName.BackColor = def;
            txtItemOrgAnim.BackColor = def;
            txtItemOrgWeight.BackColor = def;
            txtItemOrgQuality.BackColor = def;
            txtItemOrgQty.BackColor = def;
            txtItemOrgHue.BackColor = def;
            txtItemOrgStack.BackColor = def;
            txtItemOrgValue.BackColor = def;
            txtItemOrgHeight.BackColor = def;
            txtItemOrgMisc.BackColor = def;
            txtItemOrgUnk2.BackColor = def;
            txtItemOrgUnk3.BackColor = def;

            txtItemSecName.BackColor = def;
            txtItemSecAnim.BackColor = def;
            txtItemSecWeight.BackColor = def;
            txtItemSecQuality.BackColor = def;
            txtItemSecQty.BackColor = def;
            txtItemSecHue.BackColor = def;
            txtItemSecStack.BackColor = def;
            txtItemSecValue.BackColor = def;
            txtItemSecHeight.BackColor = def;
            txtItemSecMisc.BackColor = def;
            txtItemSecUnk2.BackColor = def;
            txtItemSecUnk3.BackColor = def;
        }

        private void UpdateItemDetail(int id)
        {
            ResetItemDetailHighlights();

            bool inOrg = id < TileData.ItemTable.Length;
            bool inSec = _secondTileData != null && id < _secondTileData.ItemTable.Length;

            TileFlag orgFlags = TileFlag.None;
            TileFlag secFlags = TileFlag.None;

            if (inOrg)
            {
                var d = TileData.ItemTable[id];
                orgFlags = d.Flags;
                txtItemOrgName.Text = d.Name;
                txtItemOrgAnim.Text = d.Animation.ToString();
                txtItemOrgWeight.Text = d.Weight.ToString();
                txtItemOrgQuality.Text = d.Quality.ToString();
                txtItemOrgQty.Text = d.Quantity.ToString();
                txtItemOrgHue.Text = d.Hue.ToString();
                txtItemOrgStack.Text = d.StackingOffset.ToString();
                txtItemOrgValue.Text = d.Value.ToString();
                txtItemOrgHeight.Text = d.Height.ToString();
                txtItemOrgMisc.Text = d.MiscData.ToString();
                txtItemOrgUnk2.Text = d.Unk2.ToString();
                txtItemOrgUnk3.Text = d.Unk3.ToString();
            }
            else
            {
                ClearItemOrgFields();
            }

            if (inSec)
            {
                var d = _secondTileData.ItemTable[id];
                secFlags = d.Flags;
                txtItemSecName.Text = d.Name;
                txtItemSecAnim.Text = d.Animation.ToString();
                txtItemSecWeight.Text = d.Weight.ToString();
                txtItemSecQuality.Text = d.Quality.ToString();
                txtItemSecQty.Text = d.Quantity.ToString();
                txtItemSecHue.Text = d.Hue.ToString();
                txtItemSecStack.Text = d.StackingOffset.ToString();
                txtItemSecValue.Text = d.Value.ToString();
                txtItemSecHeight.Text = d.Height.ToString();
                txtItemSecMisc.Text = d.MiscData.ToString();
                txtItemSecUnk2.Text = d.Unk2.ToString();
                txtItemSecUnk3.Text = d.Unk3.ToString();
            }
            else
            {
                ClearItemSecFields();
            }

            UpdateFlagRows(_itemFlagOrgChecks, _itemFlagSecChecks, _itemFlagLabels, _itemFlagTlp,
                orgFlags, secFlags, inOrg, inSec, _options.ItemCompareFlags);

            if (inOrg && inSec)
            {
                HighlightItemDiffs(id);
            }
        }

        private void ClearItemOrgFields()
        {
            txtItemOrgName.Text = txtItemOrgAnim.Text =
            txtItemOrgWeight.Text = txtItemOrgQuality.Text = txtItemOrgQty.Text =
            txtItemOrgHue.Text = txtItemOrgStack.Text = txtItemOrgValue.Text =
            txtItemOrgHeight.Text = txtItemOrgMisc.Text = txtItemOrgUnk2.Text =
            txtItemOrgUnk3.Text = string.Empty;
        }

        private void ClearItemSecFields()
        {
            txtItemSecName.Text = txtItemSecAnim.Text =
            txtItemSecWeight.Text = txtItemSecQuality.Text = txtItemSecQty.Text =
            txtItemSecHue.Text = txtItemSecStack.Text = txtItemSecValue.Text =
            txtItemSecHeight.Text = txtItemSecMisc.Text = txtItemSecUnk2.Text =
            txtItemSecUnk3.Text = string.Empty;
        }

        private void HighlightItemDiffs(int id)
        {
            var org = TileData.ItemTable[id];
            var sec = _secondTileData.ItemTable[id];

            Highlight(_options.ItemCompareName && org.Name != sec.Name, txtItemOrgName, txtItemSecName);
            Highlight(_options.ItemCompareAnimation && org.Animation != sec.Animation, txtItemOrgAnim, txtItemSecAnim);
            Highlight(_options.ItemCompareWeight && org.Weight != sec.Weight, txtItemOrgWeight, txtItemSecWeight);
            Highlight(_options.ItemCompareQuality && org.Quality != sec.Quality, txtItemOrgQuality, txtItemSecQuality);
            Highlight(_options.ItemCompareQuantity && org.Quantity != sec.Quantity, txtItemOrgQty, txtItemSecQty);
            Highlight(_options.ItemCompareHue && org.Hue != sec.Hue, txtItemOrgHue, txtItemSecHue);
            Highlight(_options.ItemCompareStackingOffset && org.StackingOffset != sec.StackingOffset, txtItemOrgStack, txtItemSecStack);
            Highlight(_options.ItemCompareValue && org.Value != sec.Value, txtItemOrgValue, txtItemSecValue);
            Highlight(_options.ItemCompareHeight && org.Height != sec.Height, txtItemOrgHeight, txtItemSecHeight);
            Highlight(_options.ItemCompareMiscData && org.MiscData != sec.MiscData, txtItemOrgMisc, txtItemSecMisc);
            Highlight(_options.ItemCompareUnk2 && org.Unk2 != sec.Unk2, txtItemOrgUnk2, txtItemSecUnk2);
            Highlight(_options.ItemCompareUnk3 && org.Unk3 != sec.Unk3, txtItemOrgUnk3, txtItemSecUnk3);
        }

        private static void Highlight(bool differs, TextBox orgBox, TextBox secBox)
        {
            if (differs)
            {
                orgBox.BackColor = DiffColor;
                secBox.BackColor = DiffColor;
            }
        }

        // ── Transfer ─────────────────────────────────────────────────────────────

        private void OnClickCopyLandSelected(object sender, EventArgs e)
        {
            if (_secondTileData == null || tileViewLandSec.FocusIndex < 0)
            {
                return;
            }

            int id = _landDisplayIndices[tileViewLandSec.FocusIndex];
            CopyLandEntry(id);

            if (chkShowDiff.Checked)
            {
                int displayIdx = _landDisplayIndices.IndexOf(id);
                if (displayIdx >= 0)
                {
                    _landDisplayIndices.RemoveAt(displayIdx);
                    tileViewLandOrg.VirtualListSize = _landDisplayIndices.Count;
                    tileViewLandSec.VirtualListSize = _landDisplayIndices.Count;
                }
            }

            tileViewLandOrg.Invalidate();
            tileViewLandSec.Invalidate();
            UpdateLandDetail(id);
        }

        private void OnClickCopyLandAllDiff(object sender, EventArgs e)
        {
            if (_secondTileData == null)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            int total = Math.Max(TileData.LandTable.Length, _secondTileData.LandTable.Length);
            for (int i = 0; i < total; i++)
            {
                if (!CompareLand(i) && i < _secondTileData.LandTable.Length)
                {
                    CopyLandEntry(i);
                }
            }

            if (chkShowDiff.Checked)
            {
                RefreshLandLists();
            }

            Cursor.Current = Cursors.Default;
            tileViewLandOrg.Invalidate();
            tileViewLandSec.Invalidate();
        }

        private void CopyLandEntry(int id)
        {
            if (id >= _secondTileData.LandTable.Length)
            {
                return;
            }

            TileData.LandTable[id] = _secondTileData.LandTable[id];
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, id);
            _compareLand[id] = true;
        }

        private void OnClickCopyItemSelected(object sender, EventArgs e)
        {
            if (_secondTileData == null || tileViewItemSec.FocusIndex < 0)
            {
                return;
            }

            int id = _itemDisplayIndices[tileViewItemSec.FocusIndex];
            CopyItemEntry(id);

            if (chkShowDiff.Checked)
            {
                int displayIdx = _itemDisplayIndices.IndexOf(id);
                if (displayIdx >= 0)
                {
                    _itemDisplayIndices.RemoveAt(displayIdx);
                    tileViewItemOrg.VirtualListSize = _itemDisplayIndices.Count;
                    tileViewItemSec.VirtualListSize = _itemDisplayIndices.Count;
                }
            }

            tileViewItemOrg.Invalidate();
            tileViewItemSec.Invalidate();
            UpdateItemDetail(id);
        }

        private void OnClickCopyItemAllDiff(object sender, EventArgs e)
        {
            if (_secondTileData == null)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            int total = Math.Max(TileData.ItemTable.Length, _secondTileData.ItemTable.Length);
            for (int i = 0; i < total; i++)
            {
                if (!CompareItem(i) && i < _secondTileData.ItemTable.Length)
                {
                    CopyItemEntry(i);
                }
            }

            if (chkShowDiff.Checked)
            {
                RefreshItemLists();
            }

            Cursor.Current = Cursors.Default;
            tileViewItemOrg.Invalidate();
            tileViewItemSec.Invalidate();
        }

        private void OnDoubleClickItemSec(object sender, MouseEventArgs e)
        {
            OnClickCopyItemSelected(sender, e);
        }

        private void OnDoubleClickLandSec(object sender, MouseEventArgs e)
        {
            OnClickCopyLandSelected(sender, e);
        }

        private void CopyItemEntry(int id)
        {
            if (id >= _secondTileData.ItemTable.Length)
            {
                return;
            }

            TileData.ItemTable[id] = _secondTileData.ItemTable[id];
            Options.ChangedUltimaClass["TileData"] = true;
            ControlEvents.FireTileDataChangeEvent(this, id);
            _compareItem[id] = true;
        }

        // ── Compare Rules panel ───────────────────────────────────────────────────

        private void BuildRulesPanel()
        {
            // Land field checkboxes
            WireFieldCheckbox(chkLandName, () => _options.LandCompareName, v => _options.LandCompareName = v);
            WireFieldCheckbox(chkLandTexId, () => _options.LandCompareTextureId, v => _options.LandCompareTextureId = v);
            WireFieldCheckbox(chkLandFlags, () => _options.LandCompareFlags, v => _options.LandCompareFlags = v);

            // Item field checkboxes
            WireFieldCheckbox(chkItemName, () => _options.ItemCompareName, v => _options.ItemCompareName = v);
            WireFieldCheckbox(chkItemFlags, () => _options.ItemCompareFlags, v => _options.ItemCompareFlags = v);
            WireFieldCheckbox(chkItemAnim, () => _options.ItemCompareAnimation, v => _options.ItemCompareAnimation = v);
            WireFieldCheckbox(chkItemWeight, () => _options.ItemCompareWeight, v => _options.ItemCompareWeight = v);
            WireFieldCheckbox(chkItemQuality, () => _options.ItemCompareQuality, v => _options.ItemCompareQuality = v);
            WireFieldCheckbox(chkItemQty, () => _options.ItemCompareQuantity, v => _options.ItemCompareQuantity = v);
            WireFieldCheckbox(chkItemHue, () => _options.ItemCompareHue, v => _options.ItemCompareHue = v);
            WireFieldCheckbox(chkItemStack, () => _options.ItemCompareStackingOffset, v => _options.ItemCompareStackingOffset = v);
            WireFieldCheckbox(chkItemValue, () => _options.ItemCompareValue, v => _options.ItemCompareValue = v);
            WireFieldCheckbox(chkItemHeight, () => _options.ItemCompareHeight, v => _options.ItemCompareHeight = v);
            WireFieldCheckbox(chkItemMisc, () => _options.ItemCompareMiscData, v => _options.ItemCompareMiscData = v);
            WireFieldCheckbox(chkItemUnk2, () => _options.ItemCompareUnk2, v => _options.ItemCompareUnk2 = v);
            WireFieldCheckbox(chkItemUnk3, () => _options.ItemCompareUnk3, v => _options.ItemCompareUnk3 = v);

            // Build flag checkboxes dynamically into clbFlags (CheckedListBox)
            clbFlags.Items.Clear();
            foreach (var (name, _) in MeaningfulFlags)
            {
                clbFlags.Items.Add(name, isChecked: false); // unchecked = not ignored
            }

        }

        private void WireFieldCheckbox(CheckBox chk, Func<bool> getter, Action<bool> setter)
        {
            chk.Checked = getter();
            chk.CheckedChanged += (s, e) => setter(chk.Checked);
        }

        private void OnClickApplyRules(object sender, EventArgs e)
        {
            // Rebuild ignored flags mask from the CheckedListBox state
            TileFlag mask = TileFlag.None;
            for (int i = 0; i < clbFlags.Items.Count && i < MeaningfulFlags.Length; i++)
            {
                if (clbFlags.GetItemChecked(i))
                {
                    mask |= MeaningfulFlags[i].Flag;
                }
            }

            _options.IgnoredFlags = mask;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                InvalidateCompareCache();
                if (IsLandTab)
                {
                    RefreshLandLists();
                }
                else
                {
                    RefreshItemLists();
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnClickResetRules(object sender, EventArgs e)
        {
            _options.ResetToDefaults();

            chkLandName.Checked = true;
            chkLandTexId.Checked = true;
            chkLandFlags.Checked = true;
            chkItemName.Checked = true;
            chkItemFlags.Checked = true;
            chkItemAnim.Checked = true;
            chkItemWeight.Checked = true;
            chkItemQuality.Checked = true;
            chkItemQty.Checked = true;
            chkItemHue.Checked = true;
            chkItemStack.Checked = true;
            chkItemValue.Checked = true;
            chkItemHeight.Checked = true;
            chkItemMisc.Checked = true;
            chkItemUnk2.Checked = true;
            chkItemUnk3.Checked = true;

            for (int i = 0; i < clbFlags.Items.Count; i++)
            {
                clbFlags.SetItemChecked(i, false);
            }

            // Apply immediately after reset
            OnClickApplyRules(sender, e);
        }

        private void OnClickToggleRules(object sender, EventArgs e)
        {
            panelRules.Visible = !panelRules.Visible;
            btnToggleRules.Text = panelRules.Visible ? "Rules ▼" : "Rules ▲";
        }
    }
}
