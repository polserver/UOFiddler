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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.UserControls.TileView;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareLandControl : UserControl
    {
        public CompareLandControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly ImageConverter _ic = new ImageConverter();
        private readonly List<int> _displayIndices = new List<int>();
        private bool _syncingSelection;
        private bool _secondLoaded;

        private void OnLoad(object sender, EventArgs e)
        {
            ConfigureTileView(tileViewOrg);
            ConfigureTileView(tileViewSec);

            _displayIndices.Clear();
            for (int i = 0; i < 0x4000; i++)
            {
                _displayIndices.Add(i);
            }

            tileViewOrg.VirtualListSize = _displayIndices.Count;
            tileViewSec.VirtualListSize = 0;

            tileViewSec.SelectedIndices.CollectionChanged += OnSecSelectedIndicesChanged;
            contextMenuStrip1.Opening += (s, ev) =>
            {
                int count = tileViewSec.SelectedIndices.Count;
                copyLandTile2To1ToolStripMenuItem.Text = tileViewSec.ShowCheckBoxes && count > 1
                    ? $"Copy {count} LandTiles to left"
                    : "Copy LandTile to left";
            };

            if (comboBoxFileMode.SelectedIndex < 0)
            {
                comboBoxFileMode.SelectedIndex = 0;
            }

            SecondArt.FileIndexChanged += OnSecondArtChanged;
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
            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
        }

        private void OnSecondArtChanged()
        {
            if (!_secondLoaded)
            {
                return;
            }

            _compare.Clear();
            tileViewOrg.Invalidate();
            tileViewSec.Invalidate();
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
            DrawListItem(e, _displayIndices[e.Index], isSecondary: false);
        }

        private void OnDrawItemSec(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawListItem(e, _displayIndices[e.Index], isSecondary: true);
        }

        private void DrawListItem(TileViewControl.DrawTileListItemEventArgs e, int i, bool isSecondary)
        {
            bool focused = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            if (focused)
            {
                using var highlightBrush = new SolidBrush(Options.TileSelectionColor);
                e.Graphics.FillRectangle(highlightBrush, e.Bounds);
            }
            else
            {
                using var backBrush = new SolidBrush(e.BackColor);
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            Brush fontBrush = Brushes.Gray;
            bool valid = isSecondary ? SecondArt.IsValidLand(i) : Art.IsValidLand(i);

            if (!valid)
            {
                fontBrush = Options.DarkMode ? Brushes.OrangeRed : Brushes.Red;
            }
            else if (tileViewSec.VirtualListSize > 0 && !Compare(i))
            {
                fontBrush = Options.DarkMode ? Brushes.CornflowerBlue : Brushes.Blue;
            }

            if (focused)
            {
                fontBrush = CompareColors.ContrastBrush(Options.TileSelectionColor);
            }

            string label = $"0x{i:X}";
            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(label, Font).Height) / 2f;
            e.Graphics.DrawString(label, Font, fontBrush, new PointF(e.ContentLeft + 5, y));
        }

        private void OnFocusChangedOrg(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int i = _displayIndices[e.FocusedItemIndex];

            if (tileViewSec.VirtualListSize > 0)
            {
                if (_syncingSelection)
                {
                    return;
                }

                _syncingSelection = true;
                try { tileViewSec.FocusIndex = e.FocusedItemIndex; }
                finally { _syncingSelection = false; }
            }

            pictureBoxOrg.BackgroundImage = Art.IsValidLand(i) ? Art.GetLand(i) : null;
            pictureBoxSec.BackgroundImage = _secondLoaded && SecondArt.IsValidLand(i) ? SecondArt.GetLand(i) : null;
            tileViewOrg.Invalidate();
        }

        private void OnFocusChangedSec(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int i = _displayIndices[e.FocusedItemIndex];

            if (_syncingSelection)
            {
                return;
            }

            _syncingSelection = true;
            try { tileViewOrg.FocusIndex = e.FocusedItemIndex; }
            finally { _syncingSelection = false; }

            pictureBoxOrg.BackgroundImage = Art.IsValidLand(i) ? Art.GetLand(i) : null;
            pictureBoxSec.BackgroundImage = SecondArt.IsValidLand(i) ? SecondArt.GetLand(i) : null;
            tileViewSec.Invalidate();
        }

        private void OnClickLoadSecond(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSecondDir.Text))
            {
                return;
            }

            string path = textBoxSecondDir.Text;
            string mulFile = Path.Combine(path, "art.mul");
            string idxFile = Path.Combine(path, "artidx.mul");
            string uopFile = Path.Combine(path, "artLegacyMUL.uop");

            if (!SecondLoadHelper.TryResolveArtPaths(comboBoxFileMode.Text, idxFile, mulFile, uopFile,
                    out string resolvedIdx, out string resolvedMul, out string resolvedUop, out string error))
            {
                MessageBox.Show(error, "Missing Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (CompareFiles.IsLoadedClientFile(resolvedMul, "art.mul") || CompareFiles.IsLoadedClientFile(resolvedUop, "artLegacyMUL.uop"))
            {
                MessageBox.Show(
                    "The selected files are the same as the currently loaded art files.\n\n" +
                    "Choose a different directory to compare against.",
                    "Same File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            SecondArt.SetFileIndex(resolvedIdx, resolvedMul, resolvedUop);
            LoadSecond();
        }

        private void LoadSecond()
        {
            _secondLoaded = true;
            _compare.Clear();
            tileViewSec.VirtualListSize = _displayIndices.Count;
            tileViewOrg.Invalidate();
        }

        private bool Compare(int index)
        {
            if (_compare.ContainsKey(index))
            {
                return _compare[index];
            }

            Bitmap bitorg = Art.GetLand(index);
            Bitmap bitsec = SecondArt.GetLand(index);
            if (bitorg == null && bitsec == null) { _compare[index] = true;  return true; }
            if (bitorg == null || bitsec == null || bitorg.Size != bitsec.Size) { _compare[index] = false; return false; }

            byte[] b1 = (byte[])_ic.ConvertTo(bitorg, typeof(byte[]));
            byte[] b2 = (byte[])_ic.ConvertTo(bitsec, typeof(byte[]));
            bool res = BitConverter.ToString(_sha256.ComputeHash(b1)) == BitConverter.ToString(_sha256.ComputeHash(b2));
            _compare[index] = res;
            return res;
        }

        private void OnChangeShowDiff(object sender, EventArgs e)
        {
            if (!_secondLoaded)
            {
                if (!checkBox1.Checked)
                {
                    return;
                }

                MessageBox.Show("Second Land file is not loaded!");
                checkBox1.Checked = false;
                return;
            }

            using (new WaitCursorScope(this))
            {
                _displayIndices.Clear();
                if (checkBox1.Checked)
                {
                    for (int i = 0; i < 0x4000; i++)
                    {
                        if (!Compare(i))
                        {
                            _displayIndices.Add(i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 0x4000; i++)
                    {
                        _displayIndices.Add(i);
                    }
                }

                tileViewOrg.VirtualListSize = _displayIndices.Count;
                tileViewSec.VirtualListSize = _displayIndices.Count;
            }
        }

        private void ExportAsBmp(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Landtile(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.bmp");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Bmp);

            FileSavedDialog.Show(FindForm(), fileName, "Landtile saved successfully.");
        }

        private void ExportAsTiff(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Landtile(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.tiff");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Tiff);
            FileSavedDialog.Show(FindForm(), fileName, "Landtile saved successfully.");
        }

        private void ExportAsJpg(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Landtile(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.jpg");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Jpeg);
            FileSavedDialog.Show(FindForm(), fileName, "Landtile saved successfully.");
        }

        private void ExportAsPng(object sender, EventArgs e)
        {
            int focusIdx = tileViewSec.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondArt.IsValidLand(i))
            {
                return;
            }

            string fileName = Path.Combine(Options.OutputPath, $"Landtile(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.png");
            SecondArt.GetLand(i).Save(fileName, ImageFormat.Png);
            FileSavedDialog.Show(FindForm(), fileName, "Landtile saved successfully.");
        }

        private void BrowseOnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the art files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            var targets = GetCopyTargets();
            if (targets.Count == 0)
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                int lastCopiedId = -1;
                bool changed = false;

                foreach (int focusIdx in targets)
                {
                    if (focusIdx < 0 || focusIdx >= _displayIndices.Count)
                    {
                        continue;
                    }

                    int i = _displayIndices[focusIdx];
                    if (!SecondArt.IsValidLand(i))
                    {
                        continue;
                    }

                    Bitmap copy = new Bitmap(SecondArt.GetLand(i));
                    Art.ReplaceLand(i, copy);
                    ControlEvents.FireLandTileChangeEvent(this, i);
                    _compare[i] = true;
                    lastCopiedId = i;
                    changed = true;
                }

                if (changed)
                {
                    Options.ChangedUltimaClass["Art"] = true;
                }

                if (checkBox1.Checked && changed)
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
                if (lastCopiedId >= 0)
                {
                    pictureBoxOrg.BackgroundImage = Art.IsValidLand(lastCopiedId) ? Art.GetLand(lastCopiedId) : null;
                }
            }
        }

        private void OnDoubleClickSec(object sender, MouseEventArgs e)
        {
            if (tileViewSec.ShowCheckBoxes)
            {
                return;
            }
            OnClickCopy(sender, e);
        }

        private void OnClickCopyAllDiff(object sender, EventArgs e)
        {
            if (!_secondLoaded)
            {
                return;
            }

            using (new WaitCursorScope(this))
            {
                for (int i = 0; i < 0x4000; i++)
                {
                    if (!SecondArt.IsValidLand(i) || Compare(i))
                    {
                        continue;
                    }

                    Bitmap copy = new Bitmap(SecondArt.GetLand(i));
                    Art.ReplaceLand(i, copy);
                    ControlEvents.FireLandTileChangeEvent(this, i);
                    _compare[i] = true;
                }

                Options.ChangedUltimaClass["Art"] = true;

                if (checkBox1.Checked)
                {
                    _displayIndices.Clear();
                    for (int i = 0; i < 0x4000; i++)
                    {
                        if (!Compare(i))
                        {
                            _displayIndices.Add(i);
                        }
                    }
                    tileViewOrg.VirtualListSize = _displayIndices.Count;
                    tileViewSec.VirtualListSize = _displayIndices.Count;
                }

                tileViewOrg.Invalidate();
                tileViewSec.Invalidate();
            }
        }
    }
}
