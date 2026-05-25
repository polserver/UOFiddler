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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.UserControls
{
    public partial class RadarColorControl : UserControl
    {
        public RadarColorControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;

            // TileViewControl runtime config (TileSize/Margin/Padding/Border) +
            // checkbox-toggle sync into the canonical _selectedItems/_selectedLand
            // HashSets.
            ConfigureTileView(tileViewItem);
            ConfigureTileView(tileViewLand);
            tileViewItem.SelectedIndices.CollectionChanged += OnItemSelectedIndicesChanged;
            tileViewLand.SelectedIndices.CollectionChanged += OnLandSelectedIndicesChanged;

#if DEBUG
            // Dev-only research harness. Created programmatically (rather than via the
            // designer) so the WinForms designer can't re-serialize it without the #if
            // guard and break Release builds.
            var benchmark = new Button
            {
                Location = new Point(4, 365),
                Size = new Size(488, 24),
                Margin = new Padding(4),
                Text = "Algorithm benchmark (CSV)",
                TabStop = false,
                UseVisualStyleBackColor = true,
            };
            benchmark.Click += OnClickAlgorithmBenchmark;
            splitContainer5.Panel2.Controls.Add(benchmark);
#endif
        }

        private int _selectedIndex = -1;
        // Tracks whether _selectedIndex refers to an item or a land tile. Set
        // when _selectedIndex is updated; consulted by SaveColor so that
        // committing the editor on a tab switch routes to the right table.
        // (Otherwise SaveColor would see the *new* tab and save the previous
        // tab's color to whatever index happens to be in _selectedIndex.)
        private bool _selectedIsItem;
        private ushort _currentColor;
        private static RadarColorControl _refMarker;
        private bool _updating;
        private readonly Dictionary<int, ushort> _originalItemColors = [];
        private readonly Dictionary<int, ushort> _originalLandColors = [];
        private Timer _debounceTimer;
        private const int _debounceTimeout = 500;
        // Canonical "checked" backing store, keyed by graphic id so the
        // selection survives filter changes. Sync'd both ways with the
        // TileViewControl.SelectedIndices collection (which uses *row positions*).
        private readonly HashSet<int> _selectedItems = [];
        private readonly HashSet<int> _selectedLand = [];

        // Visible row position → graphic id. Default identity; ApplyFilter narrows.
        private int[] _itemIndices = Array.Empty<int>();
        private int[] _landIndices = Array.Empty<int>();

        // Re-entrancy guard: when we mutate TileViewControl.SelectedIndices
        // programmatically (e.g. during a filter rebuild or Select All), we
        // don't want the CollectionChanged handler to re-mutate our HashSet
        // and double-count.
        private bool _syncingSelection;

        private static int[] BuildIdentity(int length)
        {
            var array = new int[length];
            for (int i = 0; i < length; ++i)
            {
                array[i] = i;
            }
            return array;
        }

        // TileViewControl strips TileSize/Margin/Padding/BorderWidth in the
        // Designer (DesignerSerializationVisibility.Hidden), so we apply them
        // here. Matches the compare plugin's ConfigureTileView.
        private static void ConfigureTileView(TileView.TileViewControl tv)
        {
            tv.TileSize = new Size(tv.TileSize.Width, 20);
            tv.TileMargin = new Padding(0);
            tv.TilePadding = new Padding(0);
            tv.TileBorderWidth = 0f;
            // Suppress the default focus-rectangle (DarkRed 1px outline). DrawRow already
            // renders a SystemBrushes.Highlight fill for the focused row, so the extra
            // border just adds a red line at the row edges.
            tv.TileFocusColor = Color.Transparent;
        }

        private void OnTileViewSizeChanged(object sender, EventArgs e)
        {
            var tv = (TileView.TileViewControl)sender;
            int w = tv.DisplayRectangle.Width;
            if (w > 0 && tv.TileSize.Width != w)
            {
                tv.TileSize = new Size(w, tv.TileSize.Height);
            }
        }

        private int GetSelectedItemGraphic()
        {
            int focus = tileViewItem.FocusIndex;
            return focus >= 0 && focus < _itemIndices.Length ? _itemIndices[focus] : -1;
        }

        private int GetSelectedLandGraphic()
        {
            int focus = tileViewLand.FocusIndex;
            return focus >= 0 && focus < _landIndices.Length ? _landIndices[focus] : -1;
        }

        private void OnDrawItemRow(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if ((uint)e.Index >= (uint)_itemIndices.Length)
            {
                return;
            }
            int graphic = _itemIndices[e.Index];
            ref readonly ItemData row = ref TileData.ItemTable[graphic];
            DrawRow(e, graphic, row.Name, _originalItemColors.ContainsKey(graphic), RadarCol.GetItemColor(graphic));
        }

        private void OnDrawLandRow(object sender, TileView.TileViewControl.DrawTileListItemEventArgs e)
        {
            if ((uint)e.Index >= (uint)_landIndices.Length)
            {
                return;
            }
            int graphic = _landIndices[e.Index];
            ref readonly LandData row = ref TileData.LandTable[graphic];
            DrawRow(e, graphic, row.Name, _originalLandColors.ContainsKey(graphic), RadarCol.GetLandColor(graphic));
        }

        // Layout (left → right): [checkbox column from TileViewControl] | [color swatch] | [text]
        private const int SwatchSize = 12;
        private const int SwatchGap = 4;

        private static void DrawRow(TileView.TileViewControl.DrawTileListItemEventArgs e, int graphic, string name, bool modified, ushort radarHue)
        {
            bool focused = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // Row background.
            if (focused)
            {
                e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(e.BackColor), e.Bounds);
            }

            // Color swatch — a small filled rectangle showing this row's radar color.
            int swatchX = e.ContentLeft + 2;
            int swatchY = e.Bounds.Y + (e.Bounds.Height - SwatchSize) / 2;
            var swatchRect = new Rectangle(swatchX, swatchY, SwatchSize, SwatchSize);
            Color swatchColor = HueHelpers.HueToColor(radarHue);
            using (var swatchBrush = new SolidBrush(swatchColor))
            {
                e.Graphics.FillRectangle(swatchBrush, swatchRect);
            }
            using (var swatchBorder = new Pen(focused ? SystemColors.HighlightText : SystemColors.ControlDark))
            {
                e.Graphics.DrawRectangle(swatchBorder, swatchRect);
            }

            // Text starts after the swatch.
            Color textColor;
            if (focused)
            {
                textColor = SystemColors.HighlightText;
            }
            else if (modified)
            {
                textColor = Options.DarkMode ? Color.CornflowerBlue : Color.Blue;
            }
            else
            {
                textColor = Options.DarkMode ? Color.White : SystemColors.WindowText;
            }

            string text = $"0x{graphic:X4} ({graphic}) {name}";
            int textX = swatchX + SwatchSize + SwatchGap;
            float textY = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(text, e.Font).Height) / 2f;
            using var brush = new SolidBrush(textColor);
            e.Graphics.DrawString(text, e.Font, brush, new PointF(textX, textY));
        }

        private void OnItemFocusChanged(object sender, TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0 || e.FocusedItemIndex >= _itemIndices.Length)
            {
                return;
            }
            UpdateSelectedItemPreview(_itemIndices[e.FocusedItemIndex]);
        }

        private void OnLandFocusChanged(object sender, TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0 || e.FocusedItemIndex >= _landIndices.Length)
            {
                return;
            }
            UpdateSelectedLandPreview(_landIndices[e.FocusedItemIndex]);
        }

        private void OnItemSelectedIndicesChanged(object sender, IndicesCollection.NotifyCollectionChangedEventArgs e)
        {
            if (_syncingSelection || e.ItemsChanged == null)
            {
                return;
            }

            foreach (int row in e.ItemsChanged)
            {
                if ((uint)row >= (uint)_itemIndices.Length)
                {
                    continue;
                }
                int graphic = _itemIndices[row];
                if (e.Action == IndicesCollection.NotifyCollectionChangedAction.Add)
                {
                    _selectedItems.Add(graphic);
                }
                else
                {
                    _selectedItems.Remove(graphic);
                }
            }
        }

        private void OnLandSelectedIndicesChanged(object sender, IndicesCollection.NotifyCollectionChangedEventArgs e)
        {
            if (_syncingSelection || e.ItemsChanged == null)
            {
                return;
            }

            foreach (int row in e.ItemsChanged)
            {
                if ((uint)row >= (uint)_landIndices.Length)
                {
                    continue;
                }
                int graphic = _landIndices[row];
                if (e.Action == IndicesCollection.NotifyCollectionChangedAction.Add)
                {
                    _selectedLand.Add(graphic);
                }
                else
                {
                    _selectedLand.Remove(graphic);
                }
            }
        }

        private void RedrawItemRow(int graphic)
        {
            int pos = Array.IndexOf(_itemIndices, graphic);
            if (pos >= 0)
            {
                tileViewItem.RedrawItem(pos);
            }
        }

        private void RedrawLandRow(int graphic)
        {
            int pos = Array.IndexOf(_landIndices, graphic);
            if (pos >= 0)
            {
                tileViewLand.RedrawItem(pos);
            }
        }

        private void SelectItemRow(int rowPos)
        {
            if ((uint)rowPos < (uint)_itemIndices.Length)
            {
                tileViewItem.FocusIndex = rowPos;
            }
        }

        private void SelectLandRow(int rowPos)
        {
            if ((uint)rowPos < (uint)_landIndices.Length)
            {
                tileViewLand.FocusIndex = rowPos;
            }
        }

        // After _itemIndices/_landIndices change (reset or filter), the row
        // positions in SelectedIndices are stale. Rebuild them from the canonical
        // _selectedItems/_selectedLand HashSets. _syncingSelection prevents the
        // CollectionChanged handler from feeding the writes back into the HashSet.
        private void SyncItemSelectedIndicesFromHashSet()
        {
            _syncingSelection = true;
            try
            {
                tileViewItem.SelectedIndices.Clear();
                for (int i = 0; i < _itemIndices.Length; ++i)
                {
                    if (_selectedItems.Contains(_itemIndices[i]))
                    {
                        tileViewItem.SelectedIndices.Add(i);
                    }
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private void SyncLandSelectedIndicesFromHashSet()
        {
            _syncingSelection = true;
            try
            {
                tileViewLand.SelectedIndices.Clear();
                for (int i = 0; i < _landIndices.Length; ++i)
                {
                    if (_selectedLand.Contains(_landIndices[i]))
                    {
                        tileViewLand.SelectedIndices.Add(i);
                    }
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private void ResetItemView()
        {
            int total = TileData.ItemTable != null ? Art.GetMaxItemId() : 0;
            _itemIndices = BuildIdentity(total);
            tileViewItem.VirtualListSize = _itemIndices.Length;
            SyncItemSelectedIndicesFromHashSet();
            tileViewItem.Invalidate();
        }

        private void ResetLandView()
        {
            int total = TileData.LandTable?.Length ?? 0;
            _landIndices = BuildIdentity(total);
            tileViewLand.VirtualListSize = _landIndices.Length;
            SyncLandSelectedIndicesFromHashSet();
            tileViewLand.Invalidate();
        }

        public bool IsLoaded { get; private set; }

        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ushort CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor == value)
                {
                    return;
                }

                _currentColor = value;
                _updating = true;
                numericUpDownShortCol.Value = _currentColor;
                Color color = HueHelpers.HueToColor(_currentColor);
                pictureBoxColor.BackColor = color;
                numericUpDownR.Value = color.R;
                numericUpDownG.Value = color.G;
                numericUpDownB.Value = color.B;
                _updating = false;
            }
        }

        public static void Select(int graphic, bool land)
        {
            if (_refMarker == null)
            {
                return;
            }

            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            TabPageNavigator.ActivateOwningTabPage(_refMarker);

            if (_refMarker.IsHandleCreated)
            {
                _refMarker.BeginInvoke(new Action(() => ApplySelect(graphic, land)));
            }
            else
            {
                ApplySelect(graphic, land);
            }
        }

        private static void ApplySelect(int graphic, bool land)
        {
            if (land)
            {
                int pos = Array.IndexOf(_refMarker._landIndices, graphic);
                if (pos < 0)
                {
                    // Filter may exclude the target — reset and retry so
                    // cross-tab navigation always lands on the row.
                    _refMarker.ResetLandView();
                    pos = Array.IndexOf(_refMarker._landIndices, graphic);
                }

                if (pos < 0)
                {
                    return;
                }

                _refMarker.tabControl2.SelectTab(1);
                _refMarker.SelectLandRow(pos);
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
                    return;
                }

                _refMarker.tabControl2.SelectTab(0);
                _refMarker.SelectItemRow(pos);
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

            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;

            // Fresh data from disk — nothing is checked or dirty until the
            // user edits it again.
            _selectedItems.Clear();
            _selectedLand.Clear();
            _originalItemColors.Clear();
            _originalLandColors.Clear();
            _selectedIndex = -1;
            _selectedIsItem = false;

            ResetItemView();
            ResetLandView();

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;

                pictureBoxArt.BackColor = Options.PreviewBackgroundColor;

                PopulateMeanStrategyCombo();
            }

            IsLoaded = true;
        }

        private void PopulateMeanStrategyCombo()
        {
            if (comboMeanStrategy.Items.Count > 0)
            {
                return;
            }

            comboMeanStrategy.BeginUpdate();
            foreach (RadarAveragingStrategy s in RadarColorAveraging.All)
            {
                comboMeanStrategy.Items.Add(new MeanStrategyItem(s));
            }
            // Select the persisted/runtime strategy.
            for (int i = 0; i < comboMeanStrategy.Items.Count; ++i)
            {
                if (((MeanStrategyItem)comboMeanStrategy.Items[i]).Strategy == Options.RadarColorStrategy)
                {
                    comboMeanStrategy.SelectedIndex = i;
                    break;
                }
            }
            if (comboMeanStrategy.SelectedIndex < 0)
            {
                comboMeanStrategy.SelectedIndex = 0;
            }
            comboMeanStrategy.EndUpdate();
        }

        private sealed class MeanStrategyItem
        {
            public RadarAveragingStrategy Strategy { get; }
            public MeanStrategyItem(RadarAveragingStrategy s) { Strategy = s; }
            public override string ToString() => RadarColorAveraging.DisplayName(Strategy);
        }

        private RadarAveragingStrategy CurrentStrategy =>
            comboMeanStrategy.SelectedItem is MeanStrategyItem item ? item.Strategy : Options.RadarColorStrategy;

        private void OnSelectedMeanStrategyChanged(object sender, EventArgs e)
        {
            if (comboMeanStrategy.SelectedItem is MeanStrategyItem item)
            {
                Options.RadarColorStrategy = item.Strategy;
            }
        }

        private void OnClickStrategyHelp(object sender, EventArgs e)
        {
            using var dlg = new StrategyHelpForm();
            dlg.ShowDialog(FindForm());
        }

        // Modal explainer for the averaging strategies. Read-only TextBox so users can
        // copy/paste text out of it. Content comes from a static string so it lives next
        // to the code that defines the strategies.
        private sealed class StrategyHelpForm : Form
        {
            public StrategyHelpForm()
            {
                Text = "Radar color — averaging strategies";
                FormBorderStyle = FormBorderStyle.Sizable;
                StartPosition = FormStartPosition.CenterParent;
                MinimumSize = new Size(560, 400);
                ClientSize = new Size(640, 540);
                ShowInTaskbar = false;
                MinimizeBox = false;
                MaximizeBox = true;

                var text = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    WordWrap = true,
                    Dock = DockStyle.Fill,
                    Font = new Font(FontFamily.GenericSansSerif, 9f),
                    Text = HelpText,
                    TabStop = false,
                };
                var ok = new Button
                {
                    Text = "Close",
                    DialogResult = DialogResult.OK,
                    Dock = DockStyle.Bottom,
                    Height = 32,
                };
                Controls.Add(text);
                Controls.Add(ok);
                AcceptButton = ok;
                CancelButton = ok;
                // Route initial focus to the Close button so the read-only TextBox
                // doesn't auto-select its entire content when the dialog opens.
                ActiveControl = ok;
            }

            // Defensive: if focus ever lands on the TextBox (e.g. user clicks into it),
            // collapse the selection to the start instead of leaving everything selected.
            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                foreach (Control c in Controls)
                {
                    if (c is TextBox tb)
                    {
                        tb.SelectionStart = 0;
                        tb.SelectionLength = 0;
                        break;
                    }
                }
            }

            private const string HelpText =
                "Radar color averaging strategies\r\n" +
                "================================\r\n\r\n" +
                "Each entry in radarcol.mul is a 16-bit (RGB555) color used to render the world map. " +
                "When you click \"Average Color\", UOFiddler computes that color from the tile's pixels. " +
                "There are several ways to compute the average; they differ in rounding, source-pixel " +
                "selection, and the color space used.\r\n\r\n" +

                "What we learned by benchmarking\r\n" +
                "-------------------------------\r\n" +
                "Each strategy was scored against the values in radarcol.mul. The findings:\r\n\r\n" +
                "  - The 24->15 bit downscale that produced the file's values uses bit-shift (>>3), not " +
                "the *31/255 rounding that older UOFiddler builds used.\r\n\r\n" +
                "  - For ITEMS (statics): values are reproducible by a per-channel arithmetic mean of " +
                "the tile's 5-bit pixels with round-half-up. \"Mean (5-bit, banker's round)\" matches " +
                "the file byte-for-byte on ~96.6% of 13,771 item entries; the remainder is " +
                "sub-1-step error.\r\n\r\n" +
                "  - For LAND tiles: 4,239 active entries use only ~103 unique colors total. That is " +
                "a hand-tuned terrain palette, not a computed result. No pixel-averaging algorithm " +
                "matches more than ~10% of land entries. \"Snap to land palette\" picks the closest " +
                "entry from the colors already in the file, preserving terrain coherence.\r\n\r\n" +

                "Gold standard (recommended defaults)\r\n" +
                "------------------------------------\r\n" +
                "  - Items tab  ->  Mean (5-bit, banker's round)     [the actual default]\r\n" +
                "  - Land tab   ->  Snap to land palette\r\n\r\n" +
                "Switch the dropdown manually when changing tabs; selection persists in this session.\r\n\r\n" +

                "Strategies in detail\r\n" +
                "--------------------\r\n\r\n" +

                "Mean (5-bit)\r\n" +
                "    Extract 5-bit R/G/B per non-transparent pixel, sum, divide by count with " +
                "truncation. Simple but biases dark by ~half a step per channel. ~13% match on items.\r\n\r\n" +

                "Mean (5-bit, rounded)\r\n" +
                "    Same as Mean (5-bit) but uses round-half-up ((sum + n/2) / n). 96.4% match on items.\r\n\r\n" +

                "Mean (5-bit, banker's round)   [DEFAULT]\r\n" +
                "    Same but with round-half-to-even tie-break. The empirical winner: 96.6% match on items.\r\n\r\n" +

                "Mean (5-bit, rounded, incl. transparent)\r\n" +
                "    Includes transparent pixels (value 0) in the divisor. Drags the average toward 0; " +
                "useful only as a diagnostic.\r\n\r\n" +

                "Mean (8-bit, >>3 pack)\r\n" +
                "    Expands each 5-bit pixel to 8-bit (via (c<<3)|(c>>2)), averages in 8-bit space, then " +
                "packs back to 555 with bit-shift. Halfway result: ~42% match on items.\r\n\r\n" +

                "Mean (8-bit, rounded pack)\r\n" +
                "    Same as Mean (8-bit) but packs with ((c*31+127)/255). ~69% match.\r\n\r\n" +

                "Mean (5-bit, rounded, no outline)\r\n" +
                "    Drops near-black pixels (5-bit Y < 2) before averaging. Tests the common 90s sprite " +
                "trick of excluding outlines. Worse than the rounded mean in practice (~44%).\r\n\r\n" +

                "Mean (linear-light)\r\n" +
                "    Gamma-corrects sRGB to linear (x*x), averages, then back to sRGB. A control candidate; " +
                "1997 art pipelines almost certainly weren't gamma-aware. Low match (~11%).\r\n\r\n" +

                "Mode (dominant pixel)\r\n" +
                "    Returns the single most common non-transparent pixel. Useful for very uniform tiles, " +
                "noisy elsewhere. ~5% match.\r\n\r\n" +

                "Median per channel\r\n" +
                "    Independent per-channel median in 5-bit space. Robust to outliers but doesn't match " +
                "the file's values (~15%).\r\n\r\n" +

                "Mean (no outline)\r\n" +
                "    Plain 5-bit truncated mean with outline rejection. (~19%.)\r\n\r\n" +

                "Snap to land palette\r\n" +
                "    Computes the banker's-rounded 5-bit mean, then snaps the result to whichever of the " +
                "~103 land colors already in radarcol.mul is closest in 5-bit Euclidean distance. The " +
                "right choice for new/edited land tiles when you want them to look like neighbours " +
                "instead of producing a freeform color.\r\n\r\n" +

                "Snap to item palette\r\n" +
                "    Analogous to Snap to land palette but uses the ~2,200 unique item colors. Less " +
                "useful for items (their values are genuinely computed); kept for symmetry.\r\n\r\n" +

                "Legacy (UOFiddler)\r\n" +
                "    Reproduces UOFiddler's earlier behavior bit-for-bit: average in *31/255-truncated " +
                "8-bit space, repack with the same truncation. Matches only ~1.9% of file values. Kept " +
                "for continuity with older versions of UOFiddler.\r\n\r\n" +

                "Transparency and clamps\r\n" +
                "-----------------------\r\n" +
                "All strategies (except \"incl. transparent\") skip pixels equal to 0 (transparent). " +
                "The clamp rule is applied on the output: if all components downscale to 0 but the " +
                "input was non-zero, force the lane to 1. Pure black opaque pixels do not survive a " +
                "24->15 bit downscale; they appear as transparent both at runtime and in our " +
                "averaging.\r\n\r\n" +

                "Tools\r\n" +
                "-----\r\n" +
                "  - In Debug builds, the \"Algorithm benchmark (CSV)\" button on this control runs " +
                "every strategy against the loaded radarcol.mul and writes a per-strategy report to " +
                "Options.OutputPath\\radarcol_eval.csv.\r\n";
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
            pictureBoxArt.BackColor = Options.PreviewBackgroundColor;

            if (_selectedIndex < 0)
            {
                return;
            }

            if (tabControl2.SelectedIndex == 0)
            {
                UpdateSelectedItemPreview(_selectedIndex);
            }
            else
            {
                UpdateSelectedLandPreview(_selectedIndex);
            }
        }

        private void UpdateSelectedItemPreview(int graphic)
        {
            SaveColor();

            _selectedIndex = graphic;
            _selectedIsItem = true;

            if (Art.IsValidStatic(_selectedIndex))
            {
                Bitmap bitmap = Art.GetStatic(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Options.PreviewBackgroundColor);
                    newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                }

                pictureBoxArt.Image = newBitmap;
            }
            else
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }

            CurrentColor = RadarCol.GetItemColor(_selectedIndex);

            buttonRevert.Enabled = _originalItemColors.ContainsKey(_selectedIndex);
            buttonRevertAll.Enabled = _originalLandColors.Count > 0 || _originalItemColors.Count > 0;
        }

        private void UpdateSelectedLandPreview(int graphic)
        {
            SaveColor();

            _selectedIndex = graphic;
            _selectedIsItem = false;

            if (Art.IsValidLand(_selectedIndex))
            {
                Bitmap bitmap = Art.GetLand(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Options.PreviewBackgroundColor);
                    newGraphic.DrawImage(bitmap, (pictureBoxArt.Size.Width - bitmap.Width) / 2, 1);
                }

                pictureBoxArt.Image = newBitmap;
            }
            else
            {
                pictureBoxArt.Image = new Bitmap(pictureBoxArt.Width, pictureBoxArt.Height);
            }

            CurrentColor = RadarCol.GetLandColor(_selectedIndex);

            buttonRevert.Enabled = _originalLandColors.ContainsKey(_selectedIndex);
            buttonRevertAll.Enabled = _originalLandColors.Count > 0 || _originalItemColors.Count > 0;
        }

        private void OnClickMeanColor(object sender, EventArgs e)
        {
            Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(_selectedIndex) : Art.GetLand(_selectedIndex);
            if (image == null)
            {
                return;
            }

            CurrentColor = RadarColorAveraging.Compute(image, CurrentStrategy);
        }

        private void OnClickSaveFile(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "radarcol.mul");
            RadarCol.Save(fileName);

            _originalItemColors.Clear();
            _originalLandColors.Clear();
            tileViewItem.Invalidate();
            tileViewLand.Invalidate();

            Options.ChangedUltimaClass["RadarCol"] = false;

            FileSavedDialog.Show(FindForm(), fileName, "RadarCol saved successfully.");
        }

        private void SaveColor()
        {
            // Use the tab the index originated from, NOT tabControl2.SelectedIndex.
            // Otherwise switching tabs and clicking a row in the new tab would
            // commit the editor's color to the previous tab's id, mis-attributed.
            SaveColor(_selectedIndex, CurrentColor, _selectedIsItem);
        }

        private void SaveColor(int index, ushort color, bool isItemTile)
        {
            if (index < 0)
            {
                return;
            }

            if (isItemTile)
            {
                var datafileColor = RadarCol.GetItemColor(index);
                if (color != datafileColor && _originalItemColors.TryAdd(index, datafileColor))
                {
                    RedrawItemRow(index);
                }
                RadarCol.SetItemColor(index, color);
            }
            else
            {
                var datafileColor = RadarCol.GetLandColor(index);
                if (color != datafileColor && _originalLandColors.TryAdd(index, datafileColor))
                {
                    RedrawLandRow(index);
                }
                RadarCol.SetLandColor(index, color);
            }
        }

        private void OnClickRevertAll(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to revert all changes to items and land tiles?",
                "Revert All",
                MessageBoxButtons.YesNo
                );

            if (result != DialogResult.Yes)
            {
                return;
            }

            foreach (var (index, color) in _originalItemColors)
            {
                RadarCol.SetItemColor(index, color);
                if (index == _selectedIndex && tabControl2.SelectedIndex == 0)
                {
                    CurrentColor = color;
                }
            }

            foreach (var (index, color) in _originalLandColors)
            {
                RadarCol.SetLandColor(index, color);
                if (index == _selectedIndex && tabControl2.SelectedIndex == 1)
                {
                    CurrentColor = color;
                }
            }

            Options.ChangedUltimaClass["RadarCol"] = false;
            buttonRevertAll.Enabled = false;
            buttonRevert.Enabled = false;

            _originalItemColors.Clear();
            _originalLandColors.Clear();
            tileViewItem.Invalidate();
            tileViewLand.Invalidate();
        }

        private void OnClickRevert(object sender, EventArgs e)
        {
            if (_selectedIndex > -1)
            {
                if (tabControl2.SelectedIndex == 0)
                {
                    if (_originalItemColors.TryGetValue(_selectedIndex, out var color))
                    {
                        CurrentColor = color;
                        RadarCol.SetItemColor(_selectedIndex, color);
                        _originalItemColors.Remove(_selectedIndex);
                        RedrawItemRow(_selectedIndex);
                    }
                }
                else if (_originalLandColors.TryGetValue(_selectedIndex, out var color))
                {
                    CurrentColor = color;
                    RadarCol.SetLandColor(_selectedIndex, color);
                    _originalLandColors.Remove(_selectedIndex);
                    RedrawLandRow(_selectedIndex);
                }
            }

            buttonRevert.Enabled = false;

            if (_originalItemColors.Count == 0 && _originalLandColors.Count == 0)
            {
                Options.ChangedUltimaClass["RadarCol"] = false;
                buttonRevertAll.Enabled = false;
            }
        }

        private void OnClickSaveColor(object sender, EventArgs e)
        {
            SaveColor();
            if (tabControl2.SelectedIndex == 0)
            {
                if (_originalItemColors.ContainsKey(_selectedIndex))
                {
                    buttonRevert.Enabled = true;
                    buttonRevertAll.Enabled = true;
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }
            else if (_originalLandColors.ContainsKey(_selectedIndex))
            {
                buttonRevert.Enabled = true;
                buttonRevertAll.Enabled = true;
                Options.ChangedUltimaClass["RadarCol"] = true;
            }
        }

        private void OnClickSetRangeFrom(object sender, EventArgs e)
        {
            int graphic = GetGraphicFromContextSource(sender);
            if (graphic >= 0)
            {
                textBoxMeanFrom.Text = graphic.ToString();
            }
        }

        private void OnClickSetRangeTo(object sender, EventArgs e)
        {
            int graphic = GetGraphicFromContextSource(sender);
            if (graphic >= 0)
            {
                textBoxMeanTo.Text = graphic.ToString();
            }
        }

        private int GetGraphicFromContextSource(object sender)
        {
            var tv = ((ContextMenuStrip)((ToolStripItem)sender).Owner).SourceControl as TileView.TileViewControl;
            if (tv == null || tv.FocusIndex < 0)
            {
                return -1;
            }
            var indices = tv == tileViewItem ? _itemIndices : _landIndices;
            if (tv.FocusIndex >= indices.Length)
            {
                return -1;
            }
            return indices[tv.FocusIndex];
        }

        private void OnChangeR(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnChangeG(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnChangeB(object sender, EventArgs e)
        {
            if (_updating)
            {
                return;
            }

            Color col = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            CurrentColor = HueHelpers.ColorToHue(col);
        }

        private void OnNumericShortColChanged(object sender, EventArgs e)
        {
            if (!_updating)
            {
                CurrentColor = (ushort)numericUpDownShortCol.Value;
            }
        }

        private IEnumerable<int> GetValidSequence()
        {
            var isItem = tabControl2.SelectedIndex == 0;

            if (radioUseRange.Checked)
            {
                var maxIndex = isItem ? Art.GetMaxItemId() : 0x3FFF;

                if (!Utils.ConvertStringToInt(textBoxMeanFrom.Text, out int from, 0, maxIndex) ||
                    !Utils.ConvertStringToInt(textBoxMeanTo.Text, out int to, 0, maxIndex))
                {
                    MessageBox.Show($"Invalid parameters. Expected [to, from] between [0, {maxIndex} (0x{maxIndex:X4})]", "Error", MessageBoxButtons.OK);
                    return null;
                }

                if (from > to)
                {
                    (from, to) = (to, from);
                }

                return Enumerable.Range(from, to - from + 1);
            }
            else
            {
                var sequence = isItem ? _selectedItems : _selectedLand;
                if (sequence.Count == 0)
                {
                    MessageBox.Show("Invalid parameters. No tiles selected/checked.", "Error", MessageBoxButtons.OK);
                    return null;
                }
                return sequence;
            }
        }

        private ushort GetSequenceAverage(IEnumerable<int> sequence)
        {
            // Pool pixels across all tiles in the sequence and run the chosen strategy once.
            // The previous implementation averaged per-tile averages, which over-weights small
            // tiles and biases the result; pooling is what you'd expect "average over a range"
            // to mean.
            bool isItem = tabControl2.SelectedIndex == 0;
            IEnumerable<Bitmap> Images()
            {
                foreach (int i in sequence)
                {
                    Bitmap image = isItem ? Art.GetStatic(i) : Art.GetLand(i);
                    if (image != null)
                    {
                        yield return image;
                    }
                }
            }
            return RadarColorAveraging.ComputeFromMany(Images(), CurrentStrategy);
        }

        private void OnClickCurrentToRangeAverage(object sender, EventArgs e)
        {
            var sequence = GetValidSequence();

            if (sequence == null)
            {
                return;
            }

            CurrentColor = GetSequenceAverage(sequence);
            SaveColor();

            var isItemTile = tabControl2.SelectedIndex == 0;
            var enableRevert = isItemTile ? _originalItemColors.ContainsKey(_selectedIndex) : _originalLandColors.ContainsKey(_selectedIndex);

            buttonRevert.Enabled = enableRevert;
            buttonRevertAll.Enabled |= enableRevert;
            Options.ChangedUltimaClass["RadarCol"] |= enableRevert;
        }

        private void OnClickRangeToRangeAverage(object sender, EventArgs e)
        {
            var sequence = GetValidSequence();

            if (sequence == null)
            {
                return;
            }

            var color = GetSequenceAverage(sequence);
            var isItemTile = tabControl2.SelectedIndex == 0;
            bool enableRevertAll = false;

            foreach (int i in sequence)
            {
                SaveColor(i, color, isItemTile);

                var enableRevert = isItemTile ? _originalItemColors.ContainsKey(i) : _originalLandColors.ContainsKey(i);

                if (i == _selectedIndex)
                {
                    CurrentColor = color;
                    buttonRevert.Enabled = enableRevert;
                }

                enableRevertAll |= enableRevert;
            }

            if (enableRevertAll)
            {
                buttonRevertAll.Enabled = true;
                Options.ChangedUltimaClass["RadarCol"] = true;
            }
        }

        private void OnClickRangeToIndividualAverage(object sender, EventArgs e)
        {
            var sequence = GetValidSequence();

            if (sequence == null)
            {
                return;
            }

            var isItemTile = tabControl2.SelectedIndex == 0;
            bool enableRevertAll = false;

            foreach (int i in sequence)
            {
                Bitmap image = isItemTile ? Art.GetStatic(i) : Art.GetLand(i);
                if (image == null)
                {
                    continue;
                }

                var color = RadarColorAveraging.Compute(image, CurrentStrategy);

                SaveColor(i, color, isItemTile);

                var enableRevert = isItemTile ? _originalItemColors.ContainsKey(i) : _originalLandColors.ContainsKey(i);

                if (i == _selectedIndex)
                {
                    CurrentColor = color;
                    buttonRevert.Enabled = enableRevert;
                }

                enableRevertAll |= enableRevert;
            }

            if (enableRevertAll)
            {
                buttonRevertAll.Enabled = true;
                Options.ChangedUltimaClass["RadarCol"] = true;
            }
        }

        private void OnClickSelectItemsTab(object sender, EventArgs e)
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

        private void OnClickSelectItemTiledataTab(object sender, EventArgs e)
        {
            int index = GetSelectedItemGraphic();
            if (index < 0)
            {
                return;
            }

            TileDataControl.Select(index, false);
        }

        private void OnClickSelectLandTilesTab(object sender, EventArgs e)
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

        private void OnClickSelectLandTiledataTab(object sender, EventArgs e)
        {
            int index = GetSelectedLandGraphic();
            if (index < 0)
            {
                return;
            }

            TileDataControl.Select(index, true);
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose csv file to import",
                CheckFileExists = true,
                Filter = "csv files (*.csv)|*.csv"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Options.ChangedUltimaClass["RadarCol"] = true;
                RadarCol.ImportFromCSV(dialog.FileName);
                if (tabControl2.SelectedTab == tabControl2.TabPages[0])
                {
                    int graphic = GetSelectedItemGraphic();
                    if (graphic >= 0)
                    {
                        UpdateSelectedItemPreview(graphic);
                    }
                }
                else
                {
                    int graphic = GetSelectedLandGraphic();
                    if (graphic >= 0)
                    {
                        UpdateSelectedLandPreview(graphic);
                    }
                }
            }
            dialog.Dispose();
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "RadarColor.csv");
            RadarCol.ExportToCSV(fileName);

            FileSavedDialog.Show(FindForm(), fileName, "RadarColor saved successfully.");
        }

        private void OnClickMeanColorAll(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Do you want to calculate and set new radar color values for all items and land tiles entries where current color is black or missing?",
                "Average All",
                MessageBoxButtons.YesNo
                );

            if (result != DialogResult.Yes)
            {
                return;
            }

            if (TileData.ItemTable != null)
            {
                int itemsLength = Art.GetMaxItemId();
                progressBar1.Maximum = itemsLength;

                for (int i = 0; i < itemsLength; ++i)
                {
                    progressBar1.Value++;
                    if (!Art.IsValidStatic(i))
                    {
                        continue;
                    }

                    if (RadarCol.GetItemColor(i) != 0)
                    {
                        continue;
                    }

                    Bitmap image = Art.GetStatic(i);
                    if (image == null)
                    {
                        continue;
                    }

                    var currentColor = RadarColorAveraging.Compute(image, CurrentStrategy);
                    RadarCol.SetItemColor(i, currentColor);
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }

            if (TileData.LandTable != null)
            {
                int landLength = TileData.LandTable.Length;
                progressBar2.Maximum = landLength;
                for (int i = 0; i < landLength; ++i)
                {
                    progressBar2.Value++;
                    if (!Art.IsValidLand(i))
                    {
                        continue;
                    }

                    if (RadarCol.GetLandColor(i) != 0)
                    {
                        continue;
                    }

                    Bitmap image = Art.GetLand(i);
                    if (image == null)
                    {
                        continue;
                    }

                    var currentColor = RadarColorAveraging.Compute(image, CurrentStrategy);
                    RadarCol.SetLandColor(i, currentColor);
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }

            MessageBox.Show("Done!", "Average All");

            progressBar1.Value = 0;
            progressBar2.Value = 0;
        }


        private void FilterChange(TextBox control, Action<string> filterCallback)
        {
            if (_debounceTimer != null)
            {
                _debounceTimer.Stop();
            }

            _debounceTimer = new Timer
            {
                Interval = _debounceTimeout
            };

            _debounceTimer.Tick += delegate (object sender, EventArgs args)
            {
                Invoke(() =>
                {
                    filterCallback(control.Text);
                });
                _debounceTimer.Stop();
            };

            _debounceTimer.Start();
        }

        private void OnTextChangedFilterLand(object sender, EventArgs e)
        {
            FilterChange(textFilterLand, FilterLand);
        }

        private void OnTextChangedFilterItems(object sender, EventArgs e)
        {
            FilterChange(textFilterItems, FilterItems);
        }

        private void FilterLand(string filterText)
        {
            int max = TileData.LandTable?.Length ?? 0;
            var matches = new List<int>(max);
            for (int i = 0; i < max; ++i)
            {
                string name = TileData.LandTable[i].Name;
                if (name.ContainsCaseInsensitive(filterText))
                {
                    matches.Add(i);
                }
            }
            _landIndices = matches.ToArray();
            tileViewLand.VirtualListSize = _landIndices.Length;
            SyncLandSelectedIndicesFromHashSet();
            tileViewLand.Invalidate();
        }

        private void FilterItems(string filterText)
        {
            int max = TileData.ItemTable != null ? Art.GetMaxItemId() : 0;
            var matches = new List<int>(max);
            for (int i = 0; i < max; ++i)
            {
                string name = TileData.ItemTable[i].Name;
                if (name.ContainsCaseInsensitive(filterText))
                {
                    matches.Add(i);
                }
            }
            _itemIndices = matches.ToArray();
            tileViewItem.VirtualListSize = _itemIndices.Length;
            SyncItemSelectedIndicesFromHashSet();
            tileViewItem.Invalidate();
        }

        private void SetAllCheckedItems(bool isChecked)
        {
            if (isChecked)
            {
                foreach (int graphic in _itemIndices)
                {
                    _selectedItems.Add(graphic);
                }
            }
            else
            {
                foreach (int graphic in _itemIndices)
                {
                    _selectedItems.Remove(graphic);
                }
            }
            SyncItemSelectedIndicesFromHashSet();
            tileViewItem.Invalidate();
        }

        private void SetAllCheckedLand(bool isChecked)
        {
            if (isChecked)
            {
                foreach (int graphic in _landIndices)
                {
                    _selectedLand.Add(graphic);
                }
            }
            else
            {
                foreach (int graphic in _landIndices)
                {
                    _selectedLand.Remove(graphic);
                }
            }
            SyncLandSelectedIndicesFromHashSet();
            tileViewLand.Invalidate();
        }

        private void OnClickSelectAllItems(object sender, EventArgs e)
        {
            SetAllCheckedItems(true);
        }

        private void OnClickSelectNoneItems(object sender, EventArgs e)
        {
            SetAllCheckedItems(false);
        }

        private void OnCheckedChangeUseSelection(object sender, EventArgs e)
        {
            textBoxMeanFrom.Enabled = false;
            textBoxMeanTo.Enabled = false;
            buttonRangeToRangeAverage.Text = "Selected tiles to selection average";
            buttonRangeToIndividualAverage.Text = "Selected tiles to individual average";
            buttonCurrentToRangeAverage.Text = "Current tile to selection average";
        }

        private void OnCheckedChangeUseRange(object sender, EventArgs e)
        {
            textBoxMeanFrom.Enabled = true;
            textBoxMeanTo.Enabled = true;
            buttonRangeToRangeAverage.Text = "Range tiles to range average";
            buttonRangeToIndividualAverage.Text = "Range tiles to individual average";
            buttonCurrentToRangeAverage.Text = "Current tile to range average";
        }

        private void OnClickSelectAllLand(object sender, EventArgs e)
        {
            SetAllCheckedLand(true);
        }

        private void OnClickSelectNoneLand(object sender, EventArgs e)
        {
            SetAllCheckedLand(false);
        }

#if DEBUG
        private void OnClickAlgorithmBenchmark(object sender, EventArgs e)
        {
            // Dev-only research harness for the radarcol generation algorithm. Sweeps
            // every tile in the currently-loaded radarcol.mul, runs each candidate
            // averaging strategy against the tile graphic, and writes a CSV of
            // exact-match rates plus per-channel error stats. Output goes to
            // Options.OutputPath. Not shipped in Release.
            string outDir = Options.OutputPath ?? Path.GetTempPath();
            string outPath = Path.Combine(outDir, "radarcol_eval.csv");

            var strategies = RadarColorAveraging.All;
            int n = strategies.Count;

            const int landCount = 0x4000;
            int itemCount = Math.Min(Art.GetMaxItemId() + 1, 0x4000);

            // Per-class stats so we can see whether land vs item behave differently.
            var land = new BenchStats(n);
            var item = new BenchStats(n);

            // Uniqueness check: if land has very few unique values, it's almost certainly
            // hand-tuned to a palette rather than computed from the tile art.
            var landUnique = new HashSet<ushort>();
            var itemUnique = new HashSet<ushort>();

            using var progress = new ProgressBarForm("Algorithm benchmark", "Iterating tiles…");
            progress.Show(FindForm());

            for (int i = 0; i < landCount; ++i)
            {
                ushort target = RadarCol.GetLandColor(i);
                if (target == 0)
                {
                    continue;
                }
                landUnique.Add(target);
                Bitmap image;
                try { image = Art.GetLand(i); }
                catch { continue; }
                if (image == null)
                {
                    continue;
                }
                for (int s = 0; s < n; ++s)
                {
                    ushort got = RadarColorAveraging.Compute(image, strategies[s]);
                    land.Tally(target, got, s);
                }
            }

            for (int i = 0; i < itemCount; ++i)
            {
                ushort target = RadarCol.GetItemColor(i);
                if (target == 0)
                {
                    continue;
                }
                itemUnique.Add(target);
                if (!Art.IsValidStatic(i))
                {
                    continue;
                }
                Bitmap image;
                try { image = Art.GetStatic(i); }
                catch { continue; }
                if (image == null)
                {
                    continue;
                }
                for (int s = 0; s < n; ++s)
                {
                    ushort got = RadarColorAveraging.Compute(image, strategies[s]);
                    item.Tally(target, got, s);
                }
            }

            progress.Close();

            using (var sw = new StreamWriter(outPath))
            {
                sw.WriteLine($"# unique_land_colors={landUnique.Count} (out of {land.Counted[0]} land tiles with a non-zero entry)");
                sw.WriteLine($"# unique_item_colors={itemUnique.Count} (out of {item.Counted[0]} item tiles with a non-zero entry)");
                sw.WriteLine("class;strategy;tiles;exact;exact_pct;mae_r5;mae_g5;mae_b5;max_r5;max_g5;max_b5");
                for (int s = 0; s < n; ++s)
                {
                    land.WriteRow(sw, "land", strategies[s], s);
                }
                for (int s = 0; s < n; ++s)
                {
                    item.WriteRow(sw, "item", strategies[s], s);
                }
                for (int s = 0; s < n; ++s)
                {
                    BenchStats.WriteCombinedRow(sw, "total", strategies[s], s, land, item);
                }
            }

            // Pick the best strategy by combined exact-match count.
            int bestIdx = 0;
            long bestExact = land.Exact[0] + item.Exact[0];
            long bestCount = land.Counted[0] + item.Counted[0];
            for (int s = 1; s < n; ++s)
            {
                long ex = land.Exact[s] + item.Exact[s];
                if (ex > bestExact)
                {
                    bestIdx = s;
                    bestExact = ex;
                    bestCount = land.Counted[s] + item.Counted[s];
                }
            }
            string summary = $"Tiles evaluated: {bestCount} (land {land.Counted[bestIdx]} + item {item.Counted[bestIdx]})\n" +
                             $"Unique colors: land {landUnique.Count}, item {itemUnique.Count}\n\n" +
                             $"Best: {RadarColorAveraging.DisplayName(strategies[bestIdx])}\n" +
                             $"  land : {land.Exact[bestIdx]}/{land.Counted[bestIdx]} " +
                             $"({(land.Counted[bestIdx] == 0 ? 0 : 100.0 * land.Exact[bestIdx] / land.Counted[bestIdx]):F2}%)\n" +
                             $"  item : {item.Exact[bestIdx]}/{item.Counted[bestIdx]} " +
                             $"({(item.Counted[bestIdx] == 0 ? 0 : 100.0 * item.Exact[bestIdx] / item.Counted[bestIdx]):F2}%)\n\n" +
                             $"Full report: {outPath}";
            MessageBox.Show(FindForm(), summary, "Algorithm benchmark");
        }

        private sealed class BenchStats
        {
            public readonly long[] Exact;
            public readonly long[] Counted;
            public readonly long[] SumAbsR;
            public readonly long[] SumAbsG;
            public readonly long[] SumAbsB;
            public readonly int[] MaxAbsR;
            public readonly int[] MaxAbsG;
            public readonly int[] MaxAbsB;

            public BenchStats(int n)
            {
                Exact = new long[n];
                Counted = new long[n];
                SumAbsR = new long[n]; SumAbsG = new long[n]; SumAbsB = new long[n];
                MaxAbsR = new int[n]; MaxAbsG = new int[n]; MaxAbsB = new int[n];
            }

            public void Tally(ushort target, ushort got, int s)
            {
                Counted[s]++;
                if (got == target) Exact[s]++;
                HueHelpers.HueExtract5(target, out int tr, out int tg, out int tb);
                HueHelpers.HueExtract5(got, out int gr, out int gg, out int gb);
                int dr = Math.Abs(tr - gr), dg = Math.Abs(tg - gg), db = Math.Abs(tb - gb);
                SumAbsR[s] += dr; SumAbsG[s] += dg; SumAbsB[s] += db;
                if (dr > MaxAbsR[s]) MaxAbsR[s] = dr;
                if (dg > MaxAbsG[s]) MaxAbsG[s] = dg;
                if (db > MaxAbsB[s]) MaxAbsB[s] = db;
            }

            public void WriteRow(StreamWriter sw, string label, RadarAveragingStrategy strat, int s)
            {
                long c = Counted[s];
                if (c == 0) { sw.WriteLine($"{label};{strat};0;0;0;0;0;0;0;0;0"); return; }
                double pct = 100.0 * Exact[s] / c;
                double mr = (double)SumAbsR[s] / c, mg = (double)SumAbsG[s] / c, mb = (double)SumAbsB[s] / c;
                sw.WriteLine(
                    $"{label};{strat};{c};{Exact[s]};{pct:F2};{mr:F3};{mg:F3};{mb:F3};{MaxAbsR[s]};{MaxAbsG[s]};{MaxAbsB[s]}");
            }

            public static void WriteCombinedRow(StreamWriter sw, string label, RadarAveragingStrategy strat, int s, BenchStats a, BenchStats b)
            {
                long c = a.Counted[s] + b.Counted[s];
                if (c == 0) { sw.WriteLine($"{label};{strat};0;0;0;0;0;0;0;0;0"); return; }
                long ex = a.Exact[s] + b.Exact[s];
                double pct = 100.0 * ex / c;
                double mr = (double)(a.SumAbsR[s] + b.SumAbsR[s]) / c;
                double mg = (double)(a.SumAbsG[s] + b.SumAbsG[s]) / c;
                double mb = (double)(a.SumAbsB[s] + b.SumAbsB[s]) / c;
                int xr = Math.Max(a.MaxAbsR[s], b.MaxAbsR[s]);
                int xg = Math.Max(a.MaxAbsG[s], b.MaxAbsG[s]);
                int xb = Math.Max(a.MaxAbsB[s], b.MaxAbsB[s]);
                sw.WriteLine($"{label};{strat};{c};{ex};{pct:F2};{mr:F3};{mg:F3};{mb:F3};{xr};{xg};{xb}");
            }
        }

        // Minimal modal progress indicator for the benchmark; nothing fancy.
        private sealed class ProgressBarForm : Form
        {
            public ProgressBarForm(string title, string label)
            {
                Text = title;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                ControlBox = false;
                StartPosition = FormStartPosition.CenterParent;
                Size = new Size(320, 90);
                var lbl = new Label { Text = label, AutoSize = true, Location = new Point(12, 14) };
                Controls.Add(lbl);
            }
        }
#endif
    }
}
