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
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class RadarColorControl : UserControl
    {
        public RadarColorControl()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            _refMarker = this;
        }

        private int _selectedIndex = -1;
        private ushort _currentColor;
        private static RadarColorControl _refMarker;
        private bool _updating;
        private readonly Dictionary<int, ushort> _originalItemColors = [];
        private readonly Dictionary<int, ushort> _originalLandColors = [];
        private Timer _debounceTimer;
        private const int _debounceTimeout = 500;
        private readonly HashSet<int> _selectedItems = [];
        private readonly HashSet<int> _selectedLand = [];

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
            if (!_refMarker.IsLoaded)
            {
                _refMarker.OnLoad(_refMarker, EventArgs.Empty);
            }

            const int index = 0;
            if (land)
            {
                for (int i = index; i < _refMarker.treeViewLand.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewLand.Nodes[i];
                    if ((int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabControl2.SelectTab(1);
                    _refMarker.treeViewLand.SelectedNode = node;
                    node.EnsureVisible();
                    break;
                }
            }
            else
            {
                for (int i = index; i < _refMarker.treeViewItem.Nodes.Count; ++i)
                {
                    TreeNode node = _refMarker.treeViewItem.Nodes[i];
                    if ((int)node.Tag != graphic)
                    {
                        continue;
                    }

                    _refMarker.tabControl2.SelectTab(0);
                    _refMarker.treeViewItem.SelectedNode = node;
                    node.EnsureVisible();
                    break;
                }
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

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["TileData"] = true;
            Options.LoadedUltimaClass["Art"] = true;
            Options.LoadedUltimaClass["RadarColor"] = true;
            _selectedItems.Clear();
            _selectedLand.Clear();
            _originalItemColors.Clear();
            _originalLandColors.Clear();

            treeViewItem.BeginUpdate();
            try
            {
                treeViewItem.Nodes.Clear();
                if (TileData.ItemTable != null)
                {
                    TreeNode[] nodes = new TreeNode[Art.GetMaxItemId()];
                    for (int i = 0; i < Art.GetMaxItemId(); ++i)
                    {
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.ItemTable[i].Name))
                        {
                            Tag = i
                        };
                    }
                    treeViewItem.Nodes.AddRange(nodes);
                }
            }
            finally
            {
                treeViewItem.EndUpdate();
            }

            treeViewLand.BeginUpdate();
            try
            {
                treeViewLand.Nodes.Clear();
                if (TileData.LandTable != null)
                {
                    TreeNode[] nodes = new TreeNode[TileData.LandTable.Length];
                    for (int i = 0; i < TileData.LandTable.Length; ++i)
                    {
                        nodes[i] = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, TileData.LandTable[i].Name))
                        {
                            Tag = i
                        };
                    }
                    treeViewLand.Nodes.AddRange(nodes);
                }
            }
            finally
            {
                treeViewLand.EndUpdate();
            }

            if (!IsLoaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
            }

            IsLoaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void AfterSelectTreeViewItem(object sender, TreeViewEventArgs e)
        {
            SaveColor();

            _selectedIndex = (int)e.Node.Tag;

            if (Art.IsValidStatic(_selectedIndex))
            {
                Bitmap bitmap = Art.GetStatic(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Color.FromArgb(-1));
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

        private void AfterSelectTreeViewLand(object sender, TreeViewEventArgs e)
        {
            SaveColor();

            _selectedIndex = (int)e.Node.Tag;

            if (Art.IsValidLand(_selectedIndex))
            {
                Bitmap bitmap = Art.GetLand(_selectedIndex);
                Bitmap newBitmap = new Bitmap(pictureBoxArt.Size.Width, pictureBoxArt.Size.Height);
                using (Graphics newGraphic = Graphics.FromImage(newBitmap))
                {
                    newGraphic.Clear(Color.FromArgb(-1));
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

            CurrentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
        }

        private void OnClickSaveFile(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, "radarcol.mul");
            RadarCol.Save(fileName);

            _originalItemColors.Clear();
            _originalLandColors.Clear();

            foreach (TreeNode node in treeViewItem.Nodes)
            {
                node.ForeColor = SystemColors.WindowText;
            }

            foreach (TreeNode node in treeViewLand.Nodes)
            {
                node.ForeColor = SystemColors.WindowText;
            }

            MessageBox.Show($"RadarCol saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["RadarCol"] = false;
        }

        private void SaveColor()
        {
            SaveColor(_selectedIndex, CurrentColor, tabControl2.SelectedIndex == 0);
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
                if (color != datafileColor)
                {
                    if (_originalItemColors.TryAdd(index, datafileColor))
                    {
                        var previousNode = treeViewItem.Nodes.OfType<TreeNode>()
                                .FirstOrDefault(node => node.Tag.Equals(index));

                        if (previousNode != null)
                            previousNode.ForeColor = Color.Blue;
                    }
                }
                RadarCol.SetItemColor(index, color);
            }
            else
            {
                var datafileColor = RadarCol.GetLandColor(index);
                if (color != datafileColor)
                {
                    if (_originalLandColors.TryAdd(index, datafileColor))
                    {
                        var previousNode = treeViewLand.Nodes.OfType<TreeNode>()
                                .FirstOrDefault(node => node.Tag.Equals(index));

                        if (previousNode != null)
                            previousNode.ForeColor = Color.Blue;
                    }
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

            foreach (TreeNode node in treeViewItem.Nodes)
            {
                node.ForeColor = SystemColors.WindowText;
            }

            foreach (TreeNode node in treeViewLand.Nodes)
            {
                node.ForeColor = SystemColors.WindowText;
            }
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

                        var node = treeViewItem.Nodes.OfType<TreeNode>()
                            .FirstOrDefault(node => node.Tag.Equals(_selectedIndex));

                        if (node != null)
                            node.ForeColor = SystemColors.WindowText;

                        _originalItemColors.Remove(_selectedIndex);
                    }
                }
                else if (_originalLandColors.TryGetValue(_selectedIndex, out var color))
                {
                    CurrentColor = color;
                    RadarCol.SetLandColor(_selectedIndex, color);

                    var node = treeViewLand.Nodes.OfType<TreeNode>()
                        .FirstOrDefault(node => node.Tag.Equals(_selectedIndex));

                    if (node != null)
                        node.ForeColor = SystemColors.WindowText;

                    _originalLandColors.Remove(_selectedIndex);
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
            var node = ((TreeView)((ContextMenuStrip)((ToolStripItem)sender).Owner).SourceControl).SelectedNode;

            if (node != null)
            {
                textBoxMeanFrom.Text = node.Tag.ToString();
            }
        }

        private void OnClickSetRangeTo(object sender, EventArgs e)
        {
            var node = ((TreeView)((ContextMenuStrip)((ToolStripItem)sender).Owner).SourceControl).SelectedNode;

            if (node != null)
            {
                textBoxMeanTo.Text = node.Tag.ToString();
            }
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
            int gmeanr = 0;
            int gmeang = 0;
            int gmeanb = 0;

            foreach (int i in sequence)
            {
                Bitmap image = tabControl2.SelectedIndex == 0 ? Art.GetStatic(i) : Art.GetLand(i);
                if (image == null)
                {
                    continue;
                }

                unsafe
                {
                    BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                    ushort* line = (ushort*)bd.Scan0;
                    int delta = bd.Stride >> 1;
                    ushort* cur = line;

                    int meanr = 0;
                    int meang = 0;
                    int meanb = 0;

                    int count = 0;
                    for (int y = 0; y < image.Height; ++y, line += delta)
                    {
                        cur = line;
                        for (int x = 0; x < image.Width; ++x)
                        {
                            if (cur[x] != 0)
                            {
                                meanr += HueHelpers.HueToColorR(cur[x]);
                                meang += HueHelpers.HueToColorG(cur[x]);
                                meanb += HueHelpers.HueToColorB(cur[x]);
                                ++count;
                            }
                        }
                    }
                    image.UnlockBits(bd);

                    meanr /= count;
                    meang /= count;
                    meanb /= count;

                    gmeanr += meanr;
                    gmeang += meang;
                    gmeanb += meanb;
                }
            }

            var diff = sequence.Count();

            if (diff > 0)
            {

                gmeanr /= diff;
                gmeang /= diff;
                gmeanb /= diff;
            }

            Color col = Color.FromArgb(gmeanr, gmeang, gmeanb);
            return HueHelpers.ColorToHue(col);
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

                var color = HueHelpers.ColorToHue(AverageColorFrom(image));

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
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            var found = ItemsControl.SearchGraphic(index);
            if (!found)
            {
                MessageBox.Show("You need to load Items tab first.", "Information");
            }
        }

        private void OnClickSelectItemTiledataTab(object sender, EventArgs e)
        {
            if (treeViewItem.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewItem.SelectedNode.Tag;
            TileDataControl.Select(index, false);
        }

        private void OnClickSelectLandTilesTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
            var found = LandTilesControl.SearchGraphic(index);
            if (!found)
            {
                MessageBox.Show("You need to load LandTiles tab first.", "Information");
            }
        }

        private void OnClickSelectLandTiledataTab(object sender, EventArgs e)
        {
            if (treeViewLand.SelectedNode == null)
            {
                return;
            }

            int index = (int)treeViewLand.SelectedNode.Tag;
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
                    if (treeViewItem.SelectedNode != null)
                    {
                        AfterSelectTreeViewItem(this, new TreeViewEventArgs(treeViewItem.SelectedNode));
                    }
                }
                else
                {
                    if (treeViewLand.SelectedNode != null)
                    {
                        AfterSelectTreeViewLand(this, new TreeViewEventArgs(treeViewLand.SelectedNode));
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
            MessageBox.Show($"RadarColor saved to {fileName}", "Saved", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
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

                    var currentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
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

                    var currentColor = HueHelpers.ColorToHue(AverageColorFrom(image));
                    RadarCol.SetLandColor(i, currentColor);
                    Options.ChangedUltimaClass["RadarCol"] = true;
                }
            }

            MessageBox.Show("Done!", "Average All");

            progressBar1.Value = 0;
            progressBar2.Value = 0;
        }

        private unsafe Color AverageColorFrom(Bitmap image)
        {
            BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            ushort* cur = line;

            int meanR = 0;
            int meanG = 0;
            int meanB = 0;

            int count = 0;
            for (int y = 0; y < image.Height; ++y, line += delta)
            {
                cur = line;
                for (int x = 0; x < image.Width; ++x)
                {
                    if (cur[x] == 0)
                    {
                        continue;
                    }

                    meanR += HueHelpers.HueToColorR(cur[x]);
                    meanG += HueHelpers.HueToColorG(cur[x]);
                    meanB += HueHelpers.HueToColorB(cur[x]);
                    ++count;
                }
            }
            image.UnlockBits(bd);

            if (count > 0)
            {
                meanR /= count;
                meanG /= count;
                meanB /= count;
            }

            return Color.FromArgb(meanR, meanG, meanB);
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

        private void ApplyFilter(TreeView control, string filterText)
        {
            object table;
            int max;
            Dictionary<int, ushort> originalColors;
            HashSet<int> selected;
            Func<int, string> getName;

            if (control == treeViewItem)
            {
                table = TileData.ItemTable;
                max = Art.GetMaxItemId();
                originalColors = _originalItemColors;
                getName = (int index) => TileData.ItemTable[index].Name;
                selected = _selectedItems;
            }
            else
            {
                table = TileData.LandTable;
                max = 0x3FFF;
                originalColors = _originalLandColors;
                getName = (int index) => TileData.LandTable[index].Name;
                selected = _selectedLand;
            }

            Cursor.Current = Cursors.WaitCursor;
            control.BeginUpdate();
            try
            {
                if (table == null)
                {
                    return;
                }

                control.Nodes.Clear();

                List<TreeNode> nodes = [];
                for (int i = 0; i < max; ++i)
                {
                    var name = getName(i);
                    if (!name.ContainsCaseInsensitive(filterText))
                    {
                        continue;
                    }

                    var node = new TreeNode(string.Format("0x{0:X4} ({0}) {1}", i, name))
                    {
                        Tag = i,
                        Checked = selected.Contains(i)
                    };

                    if (originalColors.ContainsKey(i))
                    {
                        node.ForeColor = Color.Blue;
                    }

                    nodes.Add(node);
                }

                control.Nodes.AddRange(nodes.ToArray());
            }
            finally
            {
                control.EndUpdate();
                Cursor.Current = Cursors.Default;
            }
        }

        private void FilterLand(string filterText)
        {
            ApplyFilter(treeViewLand, filterText);
        }

        private void FilterItems(string filterText)
        {
            ApplyFilter(treeViewItem, filterText);
        }

        private void AfterCheckTreeViewItem(object sender, TreeViewEventArgs e)
        {
            var index = (int)e.Node.Tag;
            if (e.Node.Checked)
            {
                _selectedItems.Add(index);
            }
            else
            {
                _selectedItems.Remove(index);
            }
        }

        private void AfterCheckTreeViewLand(object sender, TreeViewEventArgs e)
        {
            var index = (int)e.Node.Tag;
            if (e.Node.Checked)
            {
                _selectedLand.Add(index);
            }
            else
            {
                _selectedLand.Remove(index);
            }
        }

        private static void SetAllCheckedStatus(TreeView treeView, bool isChecked)
        {
            treeView.BeginUpdate();
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                foreach (TreeNode node in treeView.Nodes)
                {
                    node.Checked = isChecked;
                }
            }
            finally
            {
                treeView.EndUpdate();
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnClickSelectAllItems(object sender, EventArgs e)
        {
            SetAllCheckedStatus(treeViewItem, true);
        }

        private void OnClickSelectNoneItems(object sender, EventArgs e)
        {
            SetAllCheckedStatus(treeViewItem, false);
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
            SetAllCheckedStatus(treeViewLand, true);
        }

        private void OnClickSelectNoneLand(object sender, EventArgs e)
        {
            SetAllCheckedStatus(treeViewLand, false);
        }
    }
}
