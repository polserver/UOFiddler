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
    public partial class CompareGumpControl : UserControl
    {
        public CompareGumpControl()
        {
            InitializeComponent();
        }

        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private readonly SHA256 _sha256 = SHA256.Create();
        private readonly List<int> _displayIndices = new List<int>();
        private bool _syncingSelection;
        private bool _loaded;

        private void OnLoad(object sender, EventArgs e)
        {
            using (new WaitCursorScope(this))
            {
                Options.LoadedUltimaClass["Gumps"] = true;

                ConfigureTileView(tileView1);
                ConfigureTileView(tileView2);

                _displayIndices.Clear();
                for (int i = 0; i < 0x10000; i++)
                {
                    _displayIndices.Add(i);
                }

                tileView1.VirtualListSize = _displayIndices.Count;
                tileView2.VirtualListSize = 0;

                if (_displayIndices.Count > 0)
                {
                    tileView1.FocusIndex = 0;
                }

                if (comboBoxFileMode.SelectedIndex < 0)
                {
                    comboBoxFileMode.SelectedIndex = 0;
                }

                if (!_loaded)
                {
                    tileView2.SelectedIndices.CollectionChanged += OnSecSelectedIndicesChanged;
                    contextMenuStrip1.Opening += (s, ev) =>
                    {
                        int count = tileView2.SelectedIndices.Count;
                        copyGump2To1ToolStripMenuItem.Text = tileView2.ShowCheckBoxes && count > 1
                            ? $"Copy {count} Gumps to left"
                            : "Copy Gump to left";
                    };
                    ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                }

                _loaded = true;
            }
        }

        // TileViewControl exposes TileSize/Margin/Padding/Border with DesignerSerializationVisibility.Hidden,
        // so VS strips them when re-saving the .Designer.cs. Apply the intended values here so they survive.
        private static void ConfigureTileView(TileViewControl tv)
        {
            tv.TileSize = new Size(tv.TileSize.Width, 60);
            tv.TileMargin = new Padding(0);
            tv.TilePadding = new Padding(0);
            tv.TileBorderWidth = 0f;
            tv.TileFocusColor = Color.Transparent;
            tv.TileHighlightColor = Options.TileSelectionColor;
            tv.TileHighLightOpacity = 0.4;
        }

        private void OnChangeMultiSelect(object sender, EventArgs e)
        {
            tileView2.ShowCheckBoxes = chkMultiSelect.Checked;
            tileView2.MultiSelect = chkMultiSelect.Checked;
            if (!chkMultiSelect.Checked)
            {
                tileView2.SelectedIndices.Clear();
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
                tileView1.SelectedIndices.Clear();
                foreach (int idx in tileView2.SelectedIndices)
                {
                    tileView1.SelectedIndices.Add(idx);
                }
            }
            finally
            {
                _syncingSelection = false;
            }
        }

        private List<int> GetCopyTargets()
        {
            var sel = tileView2.SelectedIndices;
            if (sel.Count > 0)
            {
                return sel.ToList();
            }
            if (tileView2.FocusIndex >= 0)
            {
                return new List<int> { tileView2.FocusIndex };
            }
            return new List<int>();
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void Reload()
        {
            if (_loaded)
            {
                OnLoad(this, EventArgs.Empty);
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

        private void OnDrawItem1(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawGumpItem(e, _displayIndices[e.Index], isSecondary: false);
        }

        private void OnDrawItem2(object sender, TileViewControl.DrawTileListItemEventArgs e)
        {
            DrawGumpItem(e, _displayIndices[e.Index], isSecondary: true);
        }

        private void DrawGumpItem(TileViewControl.DrawTileListItemEventArgs e, int i, bool isSecondary)
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

            bool valid = isSecondary ? SecondGump.IsValidIndex(i) : Gumps.IsValidIndex(i);
            Brush fontBrush = Brushes.Gray;

            if (valid)
            {
                Bitmap bmp = isSecondary ? SecondGump.GetGump(i) : Gumps.GetGump(i);
                if (bmp != null)
                {
                    if (tileView2.VirtualListSize > 0 && !Compare(i))
                    {
                        fontBrush = Options.DarkMode ? Brushes.CornflowerBlue : Brushes.Blue;
                    }

                    int width  = bmp.Width  > 80 ? 80 : bmp.Width;
                    int height = bmp.Height > 54 ? 54 : bmp.Height;
                    e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + e.ContentLeft + 3, e.Bounds.Y + 3, width, height));
                }
                else
                {
                    fontBrush = Options.DarkMode ? Brushes.OrangeRed : Brushes.Red;
                }
            }
            else
            {
                fontBrush = Options.DarkMode ? Brushes.OrangeRed : Brushes.Red;
            }

            if (focused)
            {
                fontBrush = CompareColors.ContrastBrush(Options.TileSelectionColor);
            }

            string label = $"0x{i:X}";
            float y = e.Bounds.Y + (e.Bounds.Height - e.Graphics.MeasureString(label, Font).Height) / 2f;
            e.Graphics.DrawString(label, Font, fontBrush, new PointF(e.ContentLeft + 85, y));
        }

        private void OnFocusChanged1(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
        {
            if (e.FocusedItemIndex < 0)
            {
                return;
            }

            int i = _displayIndices[e.FocusedItemIndex];

            if (tileView2.VirtualListSize > 0)
            {
                if (_syncingSelection)
                {
                    return;
                }

                _syncingSelection = true;
                try { tileView2.FocusIndex = e.FocusedItemIndex; }
                finally { _syncingSelection = false; }
            }

            UpdatePictureBox(pictureBox1, i, isSecondary: false);
            UpdatePictureBox(pictureBox2, i, isSecondary: true);
        }

        private void OnFocusChanged2(object sender, TileViewControl.ListViewFocusedItemSelectionChangedEventArgs e)
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
            try { tileView1.FocusIndex = e.FocusedItemIndex; }
            finally { _syncingSelection = false; }

            UpdatePictureBox(pictureBox1, i, isSecondary: false);
            UpdatePictureBox(pictureBox2, i, isSecondary: true);
        }

        private void UpdatePictureBox(PictureBox box, int i, bool isSecondary)
        {
            bool valid = isSecondary ? SecondGump.IsValidIndex(i) : Gumps.IsValidIndex(i);
            if (valid)
            {
                Bitmap bmp = isSecondary ? SecondGump.GetGump(i) : Gumps.GetGump(i);
                box.BackgroundImage = bmp;
            }
            else
            {
                box.BackgroundImage = null;
            }
        }

        private void Browse_OnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the gump files";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxSecondDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void Load_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxSecondDir.Text))
            {
                return;
            }

            string path = textBoxSecondDir.Text;
            string mulFile = Path.Combine(path, "gumpart.mul");
            string idxFile = Path.Combine(path, "gumpidx.mul");
            string uopFile = Path.Combine(path, "gumpartLegacyMUL.uop");

            if (!SecondLoadHelper.TryResolveGumpPaths(comboBoxFileMode.Text, idxFile, mulFile, uopFile,
                    out string resolvedIdx, out string resolvedMul, out string resolvedUop, out string error))
            {
                MessageBox.Show(error, "Missing Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (CompareFiles.IsLoadedClientFile(resolvedMul, "gumpart.mul") || CompareFiles.IsLoadedClientFile(resolvedUop, "gumpartLegacyMUL.uop"))
            {
                MessageBox.Show(
                    "The selected files are the same as the currently loaded gump files.\n\n" +
                    "Choose a different directory to compare against.",
                    "Same File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            using (new WaitCursorScope(this))
            {
                SecondGump.SetFileIndex(resolvedIdx, resolvedMul, resolvedUop);
                LoadSecond();
            }
        }

        private void LoadSecond()
        {
            _compare.Clear();
            tileView2.VirtualListSize = _displayIndices.Count;
            tileView1.Invalidate();
        }

        private bool Compare(int index)
        {
            if (_compare.TryGetValue(index, out bool value))
            {
                return value;
            }

            byte[] org = Gumps.GetRawGump(index, out int width1, out int height1);
            byte[] sec = SecondGump.GetRawGump(index, out int width2, out int height2);
            bool res;

            if (org == null && sec == null)
            {
                res = true;
            }
            else if (org == null || sec == null || org.Length != sec.Length)
            {
                res = false;
            }
            else if (width1 != width2 || height1 != height2)
            {
                res = false;
            }
            else
            {
                string hash1 = BitConverter.ToString(_sha256.ComputeHash(org));
                string hash2 = BitConverter.ToString(_sha256.ComputeHash(sec));
                res = hash1 == hash2;
            }

            _compare[index] = res;
            return res;
        }

        private void ShowDiff_OnClick(object sender, EventArgs e)
        {
            if (tileView2.VirtualListSize == 0)
            {
                if (checkBox1.Checked)
                {
                    MessageBox.Show("Second Gump file is not loaded!");
                    checkBox1.Checked = false;
                }
                return;
            }

            using (new WaitCursorScope(this))
            {
                _displayIndices.Clear();
                if (checkBox1.Checked)
                {
                    for (int i = 0; i < 0x10000; i++)
                    {
                        if (!Compare(i))
                        {
                            _displayIndices.Add(i);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 0x10000; i++)
                    {
                        _displayIndices.Add(i);
                    }
                }

                tileView1.VirtualListSize = _displayIndices.Count;
                tileView2.VirtualListSize = _displayIndices.Count;
            }
        }

        private void Export_Bmp(object sender, EventArgs e)
        {
            int focusIdx = tileView2.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondGump.IsValidIndex(i))
            {
                return;
            }

            string path     = Options.OutputPath;
            string fileName = Path.Combine(path, $"Gump(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.bmp");
            SecondGump.GetGump(i).Save(fileName, ImageFormat.Bmp);

            FileSavedDialog.Show(FindForm(), fileName, "Gump saved successfully.");
        }

        private void Export_Tiff(object sender, EventArgs e)
        {
            int focusIdx = tileView2.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondGump.IsValidIndex(i))
            {
                return;
            }

            string path     = Options.OutputPath;
            string fileName = Path.Combine(path, $"Gump(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.tiff");
            SecondGump.GetGump(i).Save(fileName, ImageFormat.Tiff);

            FileSavedDialog.Show(FindForm(), fileName, "Gump saved successfully.");
        }

        private void Export_Jpg(object sender, EventArgs e)
        {
            int focusIdx = tileView2.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondGump.IsValidIndex(i))
            {
                return;
            }

            string path     = Options.OutputPath;
            string fileName = Path.Combine(path, $"Gump(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.jpg");
            SecondGump.GetGump(i).Save(fileName, ImageFormat.Jpeg);

            FileSavedDialog.Show(FindForm(), fileName, "Gump saved successfully.");
        }

        private void Export_Png(object sender, EventArgs e)
        {
            int focusIdx = tileView2.FocusIndex;
            if (focusIdx < 0)
            {
                return;
            }

            int i = _displayIndices[focusIdx];
            if (!SecondGump.IsValidIndex(i))
            {
                return;
            }

            string path     = Options.OutputPath;
            string fileName = Path.Combine(path, $"Gump(Sec) {UoFiddler.Controls.Classes.Utils.FormatExportId(i)}.png");
            SecondGump.GetGump(i).Save(fileName, ImageFormat.Png);

            FileSavedDialog.Show(FindForm(), fileName, "Gump saved successfully.");
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
                    if (!SecondGump.IsValidIndex(i))
                    {
                        continue;
                    }

                    Bitmap copy = new Bitmap(SecondGump.GetGump(i));
                    Gumps.ReplaceGump(i, copy);
                    ControlEvents.FireGumpChangeEvent(this, i);
                    _compare[i] = true;
                    lastCopiedId = i;
                    changed = true;
                }

                if (changed)
                {
                    Options.ChangedUltimaClass["Gumps"] = true;
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
                    tileView1.VirtualListSize = _displayIndices.Count;
                    tileView2.VirtualListSize = _displayIndices.Count;
                }
                else
                {
                    tileView2.SelectedIndices.Clear();
                }

                tileView1.Invalidate();
                tileView2.Invalidate();
                if (lastCopiedId >= 0)
                {
                    UpdatePictureBox(pictureBox1, lastCopiedId, isSecondary: false);
                }
            }
        }
    }
}
