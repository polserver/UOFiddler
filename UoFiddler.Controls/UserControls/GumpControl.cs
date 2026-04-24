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
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class GumpControl : UserControl
    {
        public GumpControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint,
                true);
            if (!Files.CacheData)
            {
                Preload.Visible = false;
            }

            ProgressBar.Visible = false;

            _refMarker = this;

            pictureBox.BackColor = Options.PreviewBackgroundColor;
        }

        private sealed record GumpEntry(string Name, string[] Tags);

        private static GumpControl _refMarker;
        private bool _loaded;
        private bool _showFreeSlots;
        private Dictionary<int, GumpEntry> _gumpEntries = new();
        private string _activeNameFilter = string.Empty;
        private readonly HashSet<string> _activeTagFilters = new(StringComparer.OrdinalIgnoreCase);

        private static readonly string[] _layerTags =
        {
            "",             // 0x00
            "one-hand",     // 0x01
            "two-hand",     // 0x02
            "boots",        // 0x03
            "pants",        // 0x04
            "shirt",        // 0x05
            "helmet",       // 0x06
            "gloves",       // 0x07
            "ring",         // 0x08
            "talisman",     // 0x09
            "gorget",       // 0x0A
            "hair",         // 0x0B
            "waist",        // 0x0C
            "chest-armor",  // 0x0D
            "bracelet",     // 0x0E
            "",             // 0x0F
            "facial-hair",  // 0x10
            "tunic",        // 0x11
            "earring",      // 0x12
            "sleeves",      // 0x13
            "cloak",        // 0x14
            "backpack",     // 0x15
            "robe",         // 0x16
            "skirt",        // 0x17
            "leg-armor",    // 0x18
        };

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _loaded = false;
            OnLoad(EventArgs.Empty);
        }

        protected override void OnLoad(EventArgs e)
        {
            if (IsAncestorSiteInDesignMode || FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            if (_loaded)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            Options.LoadedUltimaClass["Gumps"] = true;
            _showFreeSlots = false;
            showFreeSlotsToolStripMenuItem.Checked = false;

            PopulateListBox(true);
            LoadGumpXml();

            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.GumpChangeEvent += OnGumpChangeEvent;
                ControlEvents.PreviewBackgroundColorChangeEvent += OnPreviewBackgroundColorChanged;
            }

            _loaded = true;
            Cursor.Current = Cursors.Default;
        }

        private void PopulateListBox(bool showOnlyValid)
        {
            listBox.BeginUpdate();
            listBox.Items.Clear();

            bool hasNameFilter = _activeNameFilter.Length > 0;
            bool hasTagFilter = _activeTagFilters.Count > 0;

            List<object> cache = new List<object>();
            for (int i = 0; i < Gumps.GetCount(); ++i)
            {
                if (showOnlyValid && !Gumps.IsValidIndex(i))
                {
                    continue;
                }

                if (hasNameFilter || hasTagFilter)
                {
                    // Gumps with no XML entry are hidden only while a filter is active.
                    // When all filters are cleared every gump reappears as normal.
                    if (!_gumpEntries.TryGetValue(i, out GumpEntry entry))
                    {
                        continue;
                    }

                    if (hasNameFilter && !entry.Name.ContainsCaseInsensitive(_activeNameFilter))
                    {
                        continue;
                    }

                    // AND logic: gump must carry every checked tag
                    if (hasTagFilter && !_activeTagFilters.All(t => entry.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                }

                cache.Add(i);
            }

            listBox.Items.AddRange(cache.ToArray());
            listBox.EndUpdate();

            if (listBox.Items.Count > 0)
            {
                listBox.SelectedIndex = 0;
            }
        }

        private void LoadGumpXml()
        {
            _gumpEntries.Clear();
            string path = Path.Combine(Options.AppDataPath, "Gumplist.xml");
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                var doc = new XmlDocument();
                doc.Load(path);
                XmlElement root = doc["Gumps"];
                if (root == null)
                {
                    return;
                }

                int maxId = Gumps.GetCount();
                foreach (XmlElement elem in root.SelectNodes("Gump"))
                {
                    string idAttr = elem.GetAttribute("id");
                    if (!Utils.ConvertStringToInt(idAttr, out int id, 0, maxId) || id >= maxId)
                    {
                        continue;
                    }

                    string name = elem.GetAttribute("name");
                    string tagsAttr = elem.GetAttribute("tags");
                    string[] tags = string.IsNullOrWhiteSpace(tagsAttr)
                        ? Array.Empty<string>()
                        : tagsAttr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    _gumpEntries[id] = new GumpEntry(name, tags);
                }
            }
            catch
            {
                _gumpEntries.Clear();
            }

            RebuildTagDropdown();
            PopulateListBox(!_showFreeSlots);
        }

        private void RebuildTagDropdown()
        {
            tagFilterDropDownButton.DropDownItems.Clear();
            _activeTagFilters.Clear();

            var allTags = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (GumpEntry entry in _gumpEntries.Values)
            {
                foreach (string tag in entry.Tags)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        allTags.Add(tag);
                    }
                }
            }

            tagFilterDropDownButton.Enabled = allTags.Count > 0;

            if (allTags.Count == 0)
            {
                return;
            }

            var clearItem = new ToolStripMenuItem("Clear All");
            clearItem.Click += OnClearTagFilters;
            tagFilterDropDownButton.DropDownItems.Add(clearItem);
            tagFilterDropDownButton.DropDownItems.Add(new ToolStripSeparator());

            foreach (string tag in allTags)
            {
                var item = new ToolStripMenuItem(tag) { CheckOnClick = true };
                item.CheckedChanged += OnTagFilterChanged;
                tagFilterDropDownButton.DropDownItems.Add(item);
            }

            // Keep dropdown open while the user checks/unchecks items
            tagFilterDropDownButton.DropDown.Closing -= OnTagDropDownClosing;
            tagFilterDropDownButton.DropDown.Closing += OnTagDropDownClosing;
        }

        private void OnTagDropDownClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
            }
        }

        private void OnClearTagFilters(object sender, EventArgs e)
        {
            foreach (ToolStripItem item in tagFilterDropDownButton.DropDownItems)
            {
                if (item is ToolStripMenuItem mi)
                {
                    mi.Checked = false;
                }
            }

            _activeTagFilters.Clear();
            PopulateListBox(!_showFreeSlots);
        }

        private void OnTagFilterChanged(object sender, EventArgs e)
        {
            _activeTagFilters.Clear();
            foreach (ToolStripItem item in tagFilterDropDownButton.DropDownItems)
            {
                if (item is ToolStripMenuItem { Checked: true } mi)
                {
                    _activeTagFilters.Add(mi.Text);
                }
            }

            PopulateListBox(!_showFreeSlots);
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnPreviewBackgroundColorChanged()
        {
            pictureBox.BackColor = Options.PreviewBackgroundColor;
        }

        private void OnGumpChangeEvent(object sender, int index)
        {
            if (!_loaded)
            {
                return;
            }

            if (sender.Equals(this))
            {
                return;
            }

            if (Gumps.IsValidIndex(index))
            {
                bool done = false;
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int j = int.Parse(listBox.Items[i].ToString());
                    if (j > index)
                    {
                        listBox.Items.Insert(i, index);
                        listBox.SelectedIndex = i;
                        done = true;
                        break;
                    }

                    if (j == index)
                    {
                        done = true;
                        break;
                    }
                }

                if (!done)
                {
                    listBox.Items.Add(index);
                }
            }
            else
            {
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int j = int.Parse(listBox.Items[i].ToString());
                    if (j == index)
                    {
                        listBox.Items.RemoveAt(i);
                        break;
                    }
                }

                listBox.Invalidate();
            }
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

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }

            Brush fontBrush = Brushes.Gray;

            int i = int.Parse(listBox.Items[e.Index].ToString());
            bool hasEntry = _gumpEntries.TryGetValue(i, out GumpEntry entry);

            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i, out bool patched);

                if (bmp != null)
                {
                    int thumbMaxH = e.Bounds.Height - 6;
                    int width = bmp.Width > 100 ? 100 : bmp.Width;
                    int height = bmp.Height > thumbMaxH ? thumbMaxH : bmp.Height;

                    if (listBox.SelectedIndex == e.Index)
                    {
                        e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, 105, e.Bounds.Height);
                    }
                    else if (patched)
                    {
                        e.Graphics.FillRectangle(Brushes.LightCoral, e.Bounds.X, e.Bounds.Y, 105, e.Bounds.Height);
                    }
                    e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 3, e.Bounds.Y + 3, width, height));
                }
                else
                {
                    fontBrush = Brushes.Red;
                }
            }
            else
            {
                if (listBox.SelectedIndex == e.Index)
                {
                    e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, 105, e.Bounds.Height);
                }

                fontBrush = Brushes.Red;
            }

            string idText = $"0x{i:X} ({i})";
            float idY = hasEntry
                ? e.Bounds.Y + 4
                : e.Bounds.Y + ((e.Bounds.Height / 2f) - (e.Graphics.MeasureString(idText, Font).Height / 2f));

            e.Graphics.DrawString(idText, Font, fontBrush, new PointF(105, idY));

            if (hasEntry)
            {
                if (!string.IsNullOrEmpty(entry.Name))
                {
                    e.Graphics.DrawString(entry.Name, Font, fontBrush, new PointF(105, e.Bounds.Y + 22));
                }

                if (entry.Tags.Length > 0)
                {
                    string tagLine = string.Join(" ", Array.ConvertAll(entry.Tags, t => "#" + t));
                    using Font smallFont = new Font(Font.FontFamily, Font.Size - 1f);
                    e.Graphics.DrawString(tagLine, smallFont, Brushes.Gray, new PointF(105, e.Bounds.Y + 42));
                }
            }
        }

        private void ListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 75;
        }

        private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            pictureBox.BackColor = Options.PreviewBackgroundColor;
            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i);
                if (bmp != null)
                {
                    pictureBox.BackgroundImage = bmp;
                    IDLabel.Text = $"ID: 0x{i:X} ({i})";
                    SizeLabel.Text = $"Size: {bmp.Width},{bmp.Height}";
                }
                else
                {
                    pictureBox.BackgroundImage = null;
                }
            }
            else
            {
                pictureBox.BackgroundImage = null;
            }

            listBox.Invalidate();
            JumpToMaleFemaleInvalidate();
        }

        private void JumpToMaleFemaleInvalidate()
        {
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            int gumpId = (int)listBox.SelectedItem;
            if (gumpId >= 50000)
            {
                if (gumpId >= 60000)
                {
                    jumpToMaleFemale.Text = "Jump to Male";
                    jumpToMaleFemale.Enabled = HasGumpId(gumpId - 10000);
                }
                else
                {
                    jumpToMaleFemale.Text = "Jump to Female";
                    jumpToMaleFemale.Enabled = HasGumpId(gumpId + 10000);
                }
            }
            else
            {
                jumpToMaleFemale.Enabled = false;
                jumpToMaleFemale.Text = "Jump to Male/Female";
            }
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (listBox.SelectedItems.Count != 1)
            {
                return;
            }

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "Choose image file to replace";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());

                    Gumps.ReplaceGump(i, bitmap);

                    ControlEvents.FireGumpChangeEvent(this, i);

                    listBox.Invalidate();
                    ListBox_SelectedIndexChanged(this, EventArgs.Empty);

                    Options.ChangedUltimaClass["Gumps"] = true;
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure? Will take a while", "Save", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            ProgressBarDialog barDialog = new ProgressBarDialog(Gumps.GetCount(), "Save");
            Gumps.Save(Options.OutputPath);
            barDialog.Dispose();
            Cursor.Current = Cursors.Default;
            Options.ChangedUltimaClass["Gumps"] = false;
            FileSavedDialog.Show(FindForm(), Options.OutputPath, "Files saved successfully.");
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            DialogResult result = MessageBox.Show($"Are you sure to remove {i}", "Remove", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Gumps.RemoveGump(i);
            ControlEvents.FireGumpChangeEvent(this, i);
            if (!_showFreeSlots)
            {
                listBox.Items.RemoveAt(listBox.SelectedIndex);
            }

            pictureBox.BackgroundImage = null;
            listBox.Invalidate();
            Options.ChangedUltimaClass["Gumps"] = true;
        }

        private void OnClickFindFree(object sender, EventArgs e)
        {
            int id = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            ++id;
            for (int i = listBox.SelectedIndex + 1; i < listBox.Items.Count; ++i, ++id)
            {
                if (id < int.Parse(listBox.Items[i].ToString()))
                {
                    listBox.SelectedIndex = i;
                    break;
                }

                if (!_showFreeSlots)
                {
                    continue;
                }

                if (!Gumps.IsValidIndex(int.Parse(listBox.Items[i].ToString())))
                {
                    listBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void OnTextChanged_InsertAt(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Gumps.GetCount()))
            {
                InsertText.ForeColor = Gumps.IsValidIndex(index) ? Color.Red : Color.Black;
            }
            else
            {
                InsertText.ForeColor = Color.Red;
            }
        }

        private void OnKeydown_InsertText(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Gumps.GetCount()))
            {
                return;
            }

            if (Gumps.IsValidIndex(index))
            {
                return;
            }

            contextMenuStrip.Close();
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                using (var bmpTemp = new Bitmap(dialog.FileName))
                {
                    Bitmap bitmap = new Bitmap(bmpTemp);

                    if (dialog.FileName.Contains(".bmp"))
                    {
                        bitmap = Utils.ConvertBmp(bitmap);
                    }

                    Gumps.ReplaceGump(index, bitmap);

                    ControlEvents.FireGumpChangeEvent(this, index);

                    bool done = false;
                    for (int i = 0; i < listBox.Items.Count; ++i)
                    {
                        int j = int.Parse(listBox.Items[i].ToString());
                        if (j > index)
                        {
                            listBox.Items.Insert(i, index);
                            listBox.SelectedIndex = i;
                            done = true;
                            break;
                        }

                        if (!_showFreeSlots)
                        {
                            continue;
                        }

                        if (j != i)
                        {
                            continue;
                        }

                        Search(index);
                        done = true;
                        break;
                    }

                    if (!done)
                    {
                        listBox.Items.Add(index);
                        listBox.SelectedIndex = listBox.Items.Count - 1;
                    }

                    Options.ChangedUltimaClass["Gumps"] = true;
                }
            }
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            ExportGumpImage(i, ImageFormat.Bmp);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            ExportGumpImage(i, ImageFormat.Tiff);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            ExportGumpImage(i, ImageFormat.Jpeg);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            int i = int.Parse(listBox.Items[listBox.SelectedIndex].ToString());
            ExportGumpImage(i, ImageFormat.Png);
        }

        private static void ExportGumpImage(int index, ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Gump 0x{index:X4}.{fileExtension}");

            using (Bitmap bit = new Bitmap(Gumps.GetGump(index)))
            {
                bit.Save(fileName, imageFormat);
            }

            MessageBox.Show(
                $"Gump saved to {fileName}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
        }

        private void OnClick_SaveAllBmp(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Bmp);
        }

        private void OnClick_SaveAllTiff(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Tiff);
        }

        private void OnClick_SaveAllJpg(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Jpeg);
        }

        private void OnClick_SaveAllPng(object sender, EventArgs e)
        {
            ExportAllGumps(ImageFormat.Png);
        }

        private void ExportAllGumps(ImageFormat imageFormat)
        {
            string fileExtension = Utils.GetFileExtensionFor(imageFormat);

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;

                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int index = int.Parse(listBox.Items[i].ToString());
                    if (index < 0)
                    {
                        continue;
                    }

                    string fileName = Path.Combine(dialog.SelectedPath, $"Gump 0x{index:X4}.{fileExtension}");
                    var gump = Gumps.GetGump(index);
                    if (gump is null)
                    {
                        continue;
                    }

                    using (Bitmap bit = new Bitmap(gump))
                    {
                        bit.Save(fileName, imageFormat);
                    }
                }

                Cursor.Current = Cursors.Default;

                FileSavedDialog.Show(FindForm(), dialog.SelectedPath, "All Gumps saved successfully.");
            }
        }

        private void OnClickShowFreeSlots(object sender, EventArgs e)
        {
            _showFreeSlots = !_showFreeSlots;
            PopulateListBox(!_showFreeSlots);
        }

        private void OnClickPreLoad(object sender, EventArgs e)
        {
            if (PreLoader.IsBusy)
            {
                return;
            }

            ProgressBar.Minimum = 1;
            ProgressBar.Maximum = Gumps.GetCount();
            ProgressBar.Step = 1;
            ProgressBar.Value = 1;
            ProgressBar.Visible = true;
            PreLoader.RunWorkerAsync();
        }

        private void PreLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < Gumps.GetCount(); ++i)
            {
                Gumps.GetGump(i);
                PreLoader.ReportProgress(1);
            }
        }

        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.PerformStep();
        }

        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }

        internal static void Select(int gumpId)
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            Search(gumpId);
        }

        public static bool HasGumpId(int gumpId)
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            return _refMarker.listBox.Items.Cast<object>().Any(id => (int)id == gumpId);
        }

        private void JumpToMaleFemale_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1)
            {
                return;
            }

            int gumpId = (int)listBox.SelectedItem;
            gumpId = gumpId < 60000 ? (gumpId % 10000) + 60000 : (gumpId % 10000) + 50000;

            Select(gumpId);
        }

        public static bool Search(int graphic)
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            for (int i = 0; i < _refMarker.listBox.Items.Count; ++i)
            {
                object id = _refMarker.listBox.Items[i];
                if ((int)id != graphic)
                {
                    continue;
                }

                _refMarker.listBox.SelectedIndex = i;
                _refMarker.listBox.TopIndex = i;

                return true;
            }

            return false;
        }

        private void Gump_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                searchByIdToolStripTextBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (e.Control && e.KeyCode == Keys.G)
            {
                searchByNameToolStripTextBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void SearchByNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            _activeNameFilter = searchByNameToolStripTextBox.Text.Trim();
            PopulateListBox(!_showFreeSlots);
        }

        private void InsertStartingFromTb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(InsertStartingFromTb.Text, out int index, 0, Gumps.GetCount()))
            {
                return;
            }

            contextMenuStrip.Close();

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = true;
                dialog.Title = $"Choose image file to insert at 0x{index:X}";
                dialog.CheckFileExists = true;
                dialog.Filter = "Image files (*.tif;*.tiff;*.bmp;*.png)|*.tif;*.tiff;*.bmp;*.png";
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var fileCount = dialog.FileNames.Length;
                if (CheckForIndexes(index, fileCount))
                {
                    for (int i = 0; i < fileCount; i++)
                    {
                        var currentIdx = index + i;
                        AddSingleGump(dialog.FileNames[i], currentIdx);
                    }

                    Search(index + (fileCount - 1));
                }
            }

            Options.ChangedUltimaClass["Gumps"] = true;
        }

        /// <summary>
        /// Check if all the indexes from baseIndex to baseIndex + count are valid
        /// </summary>
        /// <param name="baseIndex">Starting Index</param>
        /// <param name="count">Number of the indexes to check.</param>
        /// <returns></returns>
        private static bool CheckForIndexes(int baseIndex, int count)
        {
            for (int i = baseIndex; i < baseIndex + count; i++)
            {
                if (i >= Gumps.GetCount() || Gumps.IsValidIndex(i))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds a single Gump.
        /// </summary>
        /// <param name="fileName">Filename of the gump to add</param>
        /// <param name="index">Index where the gump shall be added.</param>
        private void AddSingleGump(string fileName, int index)
        {
            using (var bmpTemp = new Bitmap(fileName))
            {
                Bitmap bitmap = new Bitmap(bmpTemp);

                if (fileName.Contains(".bmp"))
                {
                    bitmap = Utils.ConvertBmp(bitmap);
                }

                Gumps.ReplaceGump(index, bitmap);

                ControlEvents.FireGumpChangeEvent(this, index);

                bool done = false;
                for (int i = 0; i < listBox.Items.Count; ++i)
                {
                    int j = int.Parse(listBox.Items[i].ToString());
                    if (j > index)
                    {
                        listBox.Items.Insert(i, index);
                        listBox.SelectedIndex = i;
                        done = true;
                        break;
                    }

                    if (!_showFreeSlots)
                    {
                        continue;
                    }

                    if (j != i)
                    {
                        continue;
                    }

                    done = true;
                    break;
                }

                if (!done)
                {
                    listBox.Items.Add(index);
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
            }
        }

        private void SearchByIdToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var max = Gumps.GetCount();
            if (!Utils.ConvertStringToInt(searchByIdToolStripTextBox.Text, out int graphic, 0, max))
            {
                return;
            }

            Search(graphic);
        }

        private void SaveGumpXml()
        {
            string path = Path.Combine(Options.AppDataPath, "Gumplist.xml");
            try
            {
                var doc = new XmlDocument();
                doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
                XmlElement root = doc.CreateElement("Gumps");
                doc.AppendChild(root);

                foreach (var kvp in _gumpEntries.OrderBy(k => k.Key))
                {
                    XmlElement elem = doc.CreateElement("Gump");
                    elem.SetAttribute("id", kvp.Key.ToString());
                    elem.SetAttribute("name", kvp.Value.Name);
                    if (kvp.Value.Tags.Length > 0)
                    {
                        elem.SetAttribute("tags", string.Join(",", kvp.Value.Tags));
                    }

                    root.AppendChild(elem);
                }

                doc.Save(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save Gumplist.xml:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnClickGenerateFromTileData(object sender, EventArgs e)
        {
            if (TileData.ItemTable == null || TileData.ItemTable.Length == 0)
            {
                MessageBox.Show("TileData is not loaded. Please open the Items or TileData tab first.",
                    "TileData Not Loaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Collect the first item name and layer per animation body ID.
            // Multiple items can share the same animation body, so we keep the first found.
            var animMap = new Dictionary<short, (string Name, byte Layer)>();
            for (int i = 0; i < TileData.ItemTable.Length; i++)
            {
                ItemData item = TileData.ItemTable[i];
                if (!item.Wearable || item.Animation <= 0)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    continue;
                }

                if (!animMap.ContainsKey(item.Animation))
                {
                    animMap[item.Animation] = (item.Name, item.Quality);
                }
            }

            int maxGumpId = Gumps.GetCount();
            int added = 0;

            foreach (var kvp in animMap)
            {
                short animId = kvp.Key;
                string name = kvp.Value.Name;
                byte layer = kvp.Value.Layer;
                string layerTag = layer < _layerTags.Length ? _layerTags[layer] : string.Empty;

                int maleId = animId + 50000;
                if (maleId < maxGumpId && Gumps.IsValidIndex(maleId) && !_gumpEntries.ContainsKey(maleId))
                {
                    string[] tags = BuildEquipTags(layerTag, "male");
                    _gumpEntries[maleId] = new GumpEntry($"[M] {name}", tags);
                    added++;
                }

                int femaleId = animId + 60000;
                if (femaleId < maxGumpId && Gumps.IsValidIndex(femaleId) && !_gumpEntries.ContainsKey(femaleId))
                {
                    string[] tags = BuildEquipTags(layerTag, "female");
                    _gumpEntries[femaleId] = new GumpEntry($"[F] {name}", tags);
                    added++;
                }
            }

            if (added == 0)
            {
                MessageBox.Show("No new entries were generated (all matching gumps already have entries or none found).",
                    "Generate Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveGumpXml();
            PopulateListBox(!_showFreeSlots);
            MessageBox.Show($"Added {added} entries and saved to Gumplist.xml.",
                "Generate Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string[] BuildEquipTags(string layerTag, string gender)
        {
            var tags = new List<string> { "equipment", gender };
            if (!string.IsNullOrEmpty(layerTag))
            {
                tags.Add(layerTag);
            }

            return tags.ToArray();
        }
    }
}