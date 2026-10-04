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
using Ultima.Uop;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class GumpControl : UserControl
    {
        private Func<string, string> _localizationGetter;
        private string _idLabelPrefix = "ID:";
        private string _sizeLabelPrefix = "Size:";

        public GumpControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint,
                true);
            ConfigureListView();
            if (!Files.CacheData)
            {
                Preload.Visible = false;
            }

            ProgressBar.Visible = false;

            _refMarker = this;

            pictureBox.BackColor = Options.PreviewBackgroundColor;
            
            // 在加载时应用本地化
            this.Load += (s, e) => ApplyLocalization();
        }

        public void SetLocalization(Func<string, string> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // 上下文菜单项
            showFreeSlotsToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.showFreeSlotsToolStripMenuItem") ?? "Show Free Slots";
            findNextFreeSlotToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.findNextFreeSlotToolStripMenuItem") ?? "Find Next Free Slot";
            changeBackgroundColorToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.changeBackgroundColorToolStripMenuItem") ?? "Change background color";
            extractImageToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.extractImageToolStripMenuItem") ?? "Export Image..";
            asBmpToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.asBmpToolStripMenuItem") ?? "As Bmp";
            asTiffToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.asTiffToolStripMenuItem") ?? "As Tiff";
            asJpgToolStripMenuItem1.Text = _localizationGetter("Forms.GumpControl.asJpgToolStripMenuItem1") ?? "As Jpg";
            asPngToolStripMenuItem1.Text = _localizationGetter("Forms.GumpControl.asPngToolStripMenuItem1") ?? "As Png";
            jumpToMaleFemale.Text = _localizationGetter("Forms.GumpControl.jumpToMaleFemale") ?? "Jump to Male/Female";
            
            copyImageToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.copyImageToolStripMenuItem") ?? "Copy Image";
            pasteImageToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.pasteImageToolStripMenuItem") ?? "Paste Image";
            replaceGumpToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.replaceGumpToolStripMenuItem") ?? "Replace";
            insertToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.insertToolStripMenuItem") ?? "Insert At..";
            toolStripMenuItem1.Text = _localizationGetter("Forms.GumpControl.toolStripMenuItem1") ?? "Insert Starting From";
            removeToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.removeToolStripMenuItem") ?? "Remove";
            saveToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.saveToolStripMenuItem") ?? "Save";
            
            // 过滤工具栏标签
            nameTagToolStripLabel.Text = _localizationGetter("Forms.GumpControl.nameTagToolStripLabel") ?? "Name:";
            tagFilterDropDownButton.Text = _localizationGetter("Forms.GumpControl.tagFilterDropDownButton") ?? "Tags";
            
            // 顶部工具栏
            IndexToolStripLabel.Text = _localizationGetter("Forms.GumpControl.IndexToolStripLabel") ?? "Index:";
            toolStripDropDownButton1.Text = _localizationGetter("Forms.GumpControl.toolStripDropDownButton1") ?? "Misc";
            generateFromTileDataToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.generateFromTileDataToolStripMenuItem") ?? "Generate Gumps.xml entries from TileData Equipment...";
            exportAllToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.exportAllToolStripMenuItem") ?? "Export All..";
            asBmpToolStripMenuItem1.Text = _localizationGetter("Forms.GumpControl.asBmpToolStripMenuItem") ?? "As Bmp";
            asTiffToolStripMenuItem1.Text = _localizationGetter("Forms.GumpControl.asTiffToolStripMenuItem") ?? "As Tiff";
            asJpgToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.asJpgToolStripMenuItem") ?? "As Jpg";
            asPngToolStripMenuItem.Text = _localizationGetter("Forms.GumpControl.asPngToolStripMenuItem") ?? "As Png";
            saveToolStripButton.Text = _localizationGetter("Forms.GumpControl.saveToolStripButton") ?? "Save";
            
            // 底部工具栏标签 - 保存前缀用于动态更新
            IDLabel.Text = _localizationGetter("Forms.GumpControl.IDLabel") ?? "ID:";
            _idLabelPrefix = IDLabel.Text;
            
            SizeLabel.Text = _localizationGetter("Forms.GumpControl.SizeLabel") ?? "Size:";
            _sizeLabelPrefix = SizeLabel.Text;
            
            Preload.Text = _localizationGetter("Forms.GumpControl.Preload") ?? "Preload";
        }

        private sealed record GumpEntry(string Name, string[] Tags);

        private static GumpControl _refMarker;
        private bool _loaded;
        private bool _showFreeSlots;
        private Dictionary<int, GumpEntry> _gumpEntries = new();
        private string _activeNameFilter = string.Empty;
        private readonly HashSet<string> _activeTagFilters = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gump ids currently listed, ascending. The list is virtual, so this is the only place a row's
        /// identity lives - with free slots hidden a row's position is not its id.
        /// </summary>
        private readonly List<int> _ids = new();

        /// <summary>
        /// Row height. A list view has no ItemHeight of its own, so it comes from the image list.
        /// </summary>
        private const int _rowHeight = 75;

        /// <summary>
        /// Row currently selected, or -1. Kept here because <see cref="DrawListViewItemEventArgs.State"/>
        /// does not carry the selection for a virtual owner drawn list view - the item being painted is a
        /// fresh one built in <see cref="ListView_RetrieveVirtualItem"/>, so every row read as selected.
        /// </summary>
        private int _selectedPosition = -1;

        private static readonly string[] _layerTags =
        {
            "",             // 0x00
            "单手武器",      // 0x01
            "双手武器",      // 0x02
            "靴子",          // 0x03
            "裤子",          // 0x04
            "衬衣",          // 0x05
            "头盔",          // 0x06
            "手套",          // 0x07
            "戒指",          // 0x08
            "护身符",        // 0x09
            "喉甲",          // 0x0A
            "头发",          // 0x0B
            "腰部",          // 0x0C
            "胸甲",          // 0x0D
            "手镯",          // 0x0E
            "",              // 0x0F
            "胡须",          // 0x10
            "束腰外衣",      // 0x11
            "耳环",          // 0x12
            "袖子",          // 0x13
            "斗篷",          // 0x14
            "背包",          // 0x15
            "长袍",          // 0x16
            "裙子",          // 0x17
            "腿甲",          // 0x18
        };

        /// <summary>
        /// Sets up the virtual list. It replaced a ListBox because the gump id space (0x12000) is larger
        /// than the 16 bit item index a list box exposes through LB_ITEMFROMPOINT and its scroll bar, so
        /// with free slots shown every row above 65535 aliased onto a row near the start of the list.
        /// </summary>
        private void ConfigureListView()
        {
            listView.SmallImageList = new ImageList { ImageSize = new Size(1, _rowHeight) };
            listView.Columns.Add(new ColumnHeader { Width = listView.ClientSize.Width });
            listView.ClientSizeChanged += (_, _) => listView.Columns[0].Width = listView.ClientSize.Width;

            // A row is a 105 pixel thumbnail plus a name and a tag line, so width past that is empty
            // background. Keep the pane where the user put it when the form resizes, and clamp it.
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.SplitterMoved += (_, _) => ClampListWidth();
            splitContainer1.SizeChanged += (_, _) => ClampListWidth();
            ClampListWidth();
            
            // Shift+Click 范围选择（不干扰系统的 Ctrl 多选）
            listView.MouseClick += ListView_MouseClick;
        }

        /// <summary>
        /// Widest the list pane may get, whether by dragging the splitter or by the form growing.
        /// </summary>
        private const int _maxListWidth = 450;

        private void ClampListWidth()
        {
            if (splitContainer1.Width <= 0)
            {
                return;
            }

            int max = Math.Min(_maxListWidth,
                splitContainer1.Width - splitContainer1.Panel2MinSize - splitContainer1.SplitterWidth);

            // Assigning outside the panels' own bounds throws; leave a pane too small to clamp alone.
            if (max < splitContainer1.Panel1MinSize || splitContainer1.SplitterDistance <= max)
            {
                return;
            }

            splitContainer1.SplitterDistance = max;
        }

        /// <summary>Gump id of the selected row, or -1 when nothing is selected.</summary>
        private int SelectedGumpId
        {
            get
            {
                int position = listView.SelectedIndices.Count > 0 ? listView.SelectedIndices[0] : -1;

                return position >= 0 && position < _ids.Count ? _ids[position] : -1;
            }
        }

        private void SelectPosition(int position)
        {
            if (position < 0 || position >= _ids.Count)
            {
                return;
            }

            listView.SelectedIndices.Clear();
            listView.SelectedIndices.Add(position);
            _selectedPosition = position;
            listView.EnsureVisible(position);
        }

        /// <summary>
        /// Adds an id to the listed set, keeping it ascending, and selects it. Does nothing but select
        /// when the id is already listed.
        /// </summary>
        private void InsertId(int id)
        {
            int position = _ids.BinarySearch(id);
            if (position < 0)
            {
                position = ~position;
                _ids.Insert(position, id);
                listView.VirtualListSize = _ids.Count;
            }

            SelectPosition(position);
            listView.Invalidate();
        }

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

            using (new WaitCursorScope(this))
            {
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
            }
        }

        private void PopulateListBox(bool showOnlyValid)
        {
            listView.BeginUpdate();
            listView.SelectedIndices.Clear();
            _selectedPosition = -1;
            _ids.Clear();

            bool hasNameFilter = _activeNameFilter.Length > 0;
            bool hasTagFilter = _activeTagFilters.Count > 0;

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

                _ids.Add(i);
            }

            listView.VirtualListSize = _ids.Count;
            listView.EndUpdate();
            listView.Invalidate();

            SelectPosition(0);
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

            var clearItem = new ToolStripMenuItem(_localizationGetter?.Invoke("Forms.GumpControl.ClearAllTags") ?? "Clear All");
            clearItem.Click += OnClearTagFilters;
            tagFilterDropDownButton.DropDownItems.Add(clearItem);
            tagFilterDropDownButton.DropDownItems.Add(new ToolStripSeparator());

            foreach (string tag in allTags)
            {
                // 获取本地化的标签显示文本
                string displayText = GetLocalizedTagText(tag) ?? tag;
                var item = new ToolStripMenuItem(displayText) { CheckOnClick = true, Tag = tag };
                item.CheckedChanged += OnTagFilterChanged;
                tagFilterDropDownButton.DropDownItems.Add(item);
            }

            // Keep dropdown open while the user checks/unchecks items
            tagFilterDropDownButton.DropDown.Closing -= OnTagDropDownClosing;
            tagFilterDropDownButton.DropDown.Closing += OnTagDropDownClosing;
        }

        /// <summary>
        /// 获取标签的本地化文本
        /// </summary>
        private string GetLocalizedTagText(string tag)
        {
            if (_localizationGetter == null) return null;
            
            // 尝试从本地化字典中获取标签翻译
            string translatedTag = _localizationGetter($"Forms.GumpControl.Tags.{tag}");
            return translatedTag;
        }

        /// <summary>
        /// 获取选中的所有 Gump 索引
        /// </summary>
        private List<int> GetSelectedGumpIndexes()
        {
            // 在虚拟模式中，使用 SelectedIndices 而不是 SelectedItems
            return listView.SelectedIndices.Cast<int>().ToList();
        }

        /// <summary>
        /// 获取主要选中的 Gump 索引（第一个）
        /// </summary>
        private int? GetPrimarySelectedGumpIndex()
        {
            // 在虚拟模式中，使用 SelectedIndices 而不是 SelectedItems
            return listView.SelectedIndices.Count > 0 ? listView.SelectedIndices[0] : null;
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
                    // 使用 Tag 属性存储原始标签值，而不是显示文本
                    string tagValue = mi.Tag as string ?? mi.Text;
                    _activeTagFilters.Add(tagValue);
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
                InsertId(index);

                return;
            }

            // Gone. With free slots shown the row stays, it just draws as free.
            int position = _ids.BinarySearch(index);
            if (position >= 0 && !_showFreeSlots)
            {
                listView.SelectedIndices.Clear();
                _selectedPosition = -1;
                _ids.RemoveAt(position);
                listView.VirtualListSize = _ids.Count;
            }

            listView.Invalidate();
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

        private void ListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // Everything visible is painted in ListView_DrawItem; the text is here so the row still has
            // an accessible name and a keyboard type-ahead target.
            e.Item = (uint)e.ItemIndex < (uint)_ids.Count
                ? new ListViewItem(_ids[e.ItemIndex].ToString())
                : new ListViewItem(string.Empty);
        }

        private void ListView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if ((uint)e.ItemIndex >= (uint)_ids.Count)
            {
                return;
            }

            Brush fontBrush = Brushes.Gray;

            // 在虚拟模式中，需要检查该项是否在 SelectedIndices 中
            bool isSelected = listView.SelectedIndices.Contains(e.ItemIndex);
            int i = _ids[e.ItemIndex];
            bool hasEntry = _gumpEntries.TryGetValue(i, out GumpEntry entry);

            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i, out bool patched);

                if (bmp != null)
                {
                    int thumbMaxH = e.Bounds.Height - 6;
                    int width = bmp.Width > 100 ? 100 : bmp.Width;
                    int height = bmp.Height > thumbMaxH ? thumbMaxH : bmp.Height;

                    if (isSelected)
                    {
                        e.Graphics.FillRectangle(Brushes.LightSteelBlue, e.Bounds.X, e.Bounds.Y, 105, e.Bounds.Height);
                    }
                    else if (patched)
                    {
                        e.Graphics.FillRectangle(Brushes.LightCoral, e.Bounds.X, e.Bounds.Y, 105, e.Bounds.Height);
                    }
                    e.Graphics.DrawImage(bmp, new Rectangle(e.Bounds.X + 3, e.Bounds.Y + 3, width, height));

                    if (Gumps.IsModified(i))
                    {
                        ModifiedMarker.Draw(e.Graphics, new Rectangle(e.Bounds.X, e.Bounds.Y, 105, e.Bounds.Height));
                    }
                }
                else
                {
                    fontBrush = Brushes.Red;
                }
            }
            else
            {
                if (isSelected)
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

        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedPosition = listView.SelectedIndices.Count > 0 ? listView.SelectedIndices[0] : -1;

            int i = SelectedGumpId;
            if (i < 0)
            {
                pictureBox.BackgroundImage = null;
                listView.Invalidate();

                return;
            }

            pictureBox.BackColor = Options.PreviewBackgroundColor;
            if (Gumps.IsValidIndex(i))
            {
                Bitmap bmp = Gumps.GetGump(i);
                if (bmp != null)
                {
                    pictureBox.BackgroundImage = bmp;
                    
                    // 支持显示多选信息 - 在虚拟模式中使用 SelectedIndices.Count
                    if (listView.SelectedIndices.Count > 1)
                    {
                        IDLabel.Text = $"{_idLabelPrefix} 0x{i:X} ({i}) 等 {listView.SelectedIndices.Count} 项";
                    }
                    else
                    {
                        IDLabel.Text = $"{_idLabelPrefix} 0x{i:X} ({i})";
                    }
                    
                    SizeLabel.Text = $"{_sizeLabelPrefix} {bmp.Width},{bmp.Height}";
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

            listView.Invalidate();
            JumpToMaleFemaleInvalidate();
        }

        private void ListView_MouseClick(object sender, MouseEventArgs e)
        {
            // 只处理 Shift+Click 范围选择
            // 关键：如果按着 Ctrl，立即返回，让系统自动处理 Ctrl 多选
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                return; // Ctrl+Click 由系统自动处理，我们不干扰
            }

            if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
            {
                return; // 不是 Shift+Click，不处理
            }

            ListViewHitTestInfo hitTest = listView.HitTest(e.Location);
            if (hitTest.Item == null)
            {
                return;
            }

            int clickedIndex = hitTest.Item.Index;

            // Shift+Click 范围选择：只有当已有选择时才执行
            if (listView.SelectedIndices.Count > 0)
            {
                int firstSelected = listView.SelectedIndices[0];
                int startIndex = Math.Min(firstSelected, clickedIndex);
                int endIndex = Math.Max(firstSelected, clickedIndex);

                // 清除现有选择并选择范围
                listView.SelectedIndices.Clear();
                for (int i = startIndex; i <= endIndex; i++)
                {
                    if (i >= 0 && i < _ids.Count)
                    {
                        listView.SelectedIndices.Add(i);
                    }
                }
                
                // 手动触发以更新 UI
                ListView_SelectedIndexChanged(listView, EventArgs.Empty);
            }
        }

        private void JumpToMaleFemaleInvalidate()
        {
            int gumpId = SelectedGumpId;
            if (gumpId < 0)
            {
                return;
            }

            if (gumpId >= 50000)
            {
                if (gumpId >= 60000)
                {
                    jumpToMaleFemale.Text = _localizationGetter?.Invoke("Forms.GumpControl.jumpToMale") ?? "Jump to Male";
                    jumpToMaleFemale.Enabled = HasGumpId(gumpId - 10000);
                }
                else
                {
                    jumpToMaleFemale.Text = _localizationGetter?.Invoke("Forms.GumpControl.jumpToFemale") ?? "Jump to Female";
                    jumpToMaleFemale.Enabled = HasGumpId(gumpId + 10000);
                }
            }
            else
            {
                jumpToMaleFemale.Enabled = false;
                jumpToMaleFemale.Text = _localizationGetter?.Invoke("Forms.GumpControl.jumpToMaleFemale") ?? "Jump to Male/Female";
            }
        }

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int id = SelectedGumpId;
            int selectedCount = listView.SelectedIndices.Count;
            
            // 在虚拟模式下，导出菜单项应该只在有选中项时启用
            extractImageToolStripMenuItem.Enabled = selectedCount > 0;
            
            copyImageToolStripMenuItem.Enabled = id >= 0 && Gumps.IsValidIndex(id);
            pasteImageToolStripMenuItem.Enabled = id >= 0 && ImageClipboard.ContainsImage();
        }

        private void OnClickCopyImage(object sender, EventArgs e)
        {
            int id = SelectedGumpId;
            if (id < 0 || !Gumps.IsValidIndex(id))
            {
                return;
            }

            if (!ImageClipboard.TryCopy(Gumps.GetGump(id), out string error))
            {
                string title = _localizationGetter?.Invoke("Forms.GumpControl.Messages.CopyImage") ?? "Copy Image";
                MessageBox.Show(error, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnClickPasteImage(object sender, EventArgs e)
        {
            int id = SelectedGumpId;
            if (id < 0)
            {
                return;
            }

            using Bitmap pasted = ImageClipboard.TryPaste(out string error);
            if (pasted == null)
            {
                string title = _localizationGetter?.Invoke("Forms.GumpControl.Messages.PasteImage") ?? "Paste Image";
                MessageBox.Show(error, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gumps have no fixed size, but the idx packs width and height into one int and the
            // decoder refuses anything past 0xFFFF on either axis.
            if (pasted.Width == 0 || pasted.Height == 0 || pasted.Width > 0xFFFF || pasted.Height > 0xFFFF)
            {
                string template = _localizationGetter?.Invoke("Forms.GumpControl.Messages.InvalidGumpDimensions") 
                    ?? "Invalid gump dimensions!\n\nClipboard image: {0}x{1}\nGumps may be up to 65535x65535 pixels.\n\nNo changes made.";
                string message = string.Format(template, pasted.Width, pasted.Height);
                string title = _localizationGetter?.Invoke("Forms.GumpControl.Messages.InvalidSize") ?? "Invalid Size";
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Gumps.ReplaceGump(id, Utils.ToUoBitmap(pasted));
            ControlEvents.FireGumpChangeEvent(this, id);
            listView.Invalidate();
            ListView_SelectedIndexChanged(this, EventArgs.Empty);
            Options.ChangedUltimaClass["Gumps"] = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Copy/paste is handled here rather than as menu ShortcutKeys: a shortcut on a
            // ContextMenuStrip is processed for the whole form, which would swallow Ctrl+C/Ctrl+V in
            // every text box on every tab.
            if (keyData == (Keys.Control | Keys.C) && listView.Focused)
            {
                OnClickCopyImage(this, EventArgs.Empty);
                return true;
            }

            if (keyData == (Keys.Control | Keys.V) && listView.Focused)
            {
                OnClickPasteImage(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void OnClickReplace(object sender, EventArgs e)
        {
            if (SelectedGumpId < 0)
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

                    int i = SelectedGumpId;

                    Gumps.ReplaceGump(i, bitmap);

                    ControlEvents.FireGumpChangeEvent(this, i);

                    listView.Invalidate();
                    ListView_SelectedIndexChanged(this, EventArgs.Empty);

                    Options.ChangedUltimaClass["Gumps"] = true;
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            string message = _localizationGetter?.Invoke("Forms.GumpControl.Messages.SaveConfirm") ?? "Are you sure? Will take a while";
            string title = _localizationGetter?.Invoke("Forms.GumpControl.Messages.Save") ?? "Save";
            DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            ClientFileSaveCommand.Run(this, FileType.GumpartLegacyMul, Gumps.Save, "Gumps",
                createProgress: () => new ProgressBarDialog(Gumps.GetCount(), "Save"));
        }

        private void OnClickRemove(object sender, EventArgs e)
        {
            int i = SelectedGumpId;
            if (i < 0)
            {
                return;
            }

            string template = _localizationGetter?.Invoke("Forms.GumpControl.Messages.RemoveConfirm") ?? "Are you sure to remove {0}";
            string message = string.Format(template, i);
            string title = _localizationGetter?.Invoke("Forms.GumpControl.Messages.Remove") ?? "Remove";
            DialogResult result = MessageBox.Show(message, title, MessageBoxButtons.YesNo,
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            Gumps.RemoveGump(i);
            ControlEvents.FireGumpChangeEvent(this, i);
            if (!_showFreeSlots)
            {
                int position = _ids.BinarySearch(i);
                if (position >= 0)
                {
                    listView.SelectedIndices.Clear();
                    _selectedPosition = -1;
                    _ids.RemoveAt(position);
                    listView.VirtualListSize = _ids.Count;
                }
            }

            pictureBox.BackgroundImage = null;
            listView.Invalidate();
            Options.ChangedUltimaClass["Gumps"] = true;
        }

        private void OnClickFindFree(object sender, EventArgs e)
        {
            int position = listView.SelectedIndices.Count > 0 ? listView.SelectedIndices[0] : -1;
            if (position < 0)
            {
                return;
            }

            int id = _ids[position] + 1;
            for (int i = position + 1; i < _ids.Count; ++i, ++id)
            {
                // A gap in the listed ids is a free slot when free slots are hidden.
                if (id < _ids[i])
                {
                    SelectPosition(i);
                    break;
                }

                if (!_showFreeSlots)
                {
                    continue;
                }

                if (!Gumps.IsValidIndex(_ids[i]))
                {
                    SelectPosition(i);
                    break;
                }
            }
        }

        private void OnTextChanged_InsertAt(object sender, EventArgs e)
        {
            Color invalidColor = Options.DarkMode ? Color.OrangeRed : Color.Red;
            if (Utils.ConvertStringToInt(InsertText.Text, out int index, 0, Gumps.GetCount()))
            {
                InsertText.ForeColor = Gumps.IsValidIndex(index) ? invalidColor : SystemColors.ControlText;
            }
            else
            {
                InsertText.ForeColor = invalidColor;
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

                    InsertId(index);

                    Options.ChangedUltimaClass["Gumps"] = true;
                }
            }
        }

        private void Extract_Image_ClickBmp(object sender, EventArgs e)
        {
            ExportSelectedGumpImages(ImageFormat.Bmp);
        }

        private void Extract_Image_ClickTiff(object sender, EventArgs e)
        {
            ExportSelectedGumpImages(ImageFormat.Tiff);
        }

        private void Extract_Image_ClickJpg(object sender, EventArgs e)
        {
            ExportSelectedGumpImages(ImageFormat.Jpeg);
        }

        private void Extract_Image_ClickPng(object sender, EventArgs e)
        {
            ExportSelectedGumpImages(ImageFormat.Png);
        }

        private void ExportSelectedGumpImages(ImageFormat imageFormat)
        {
            List<int> selectedIndexes = GetSelectedGumpIndexes();
            if (selectedIndexes.Count == 0)
            {
                return;
            }

            if (selectedIndexes.Count == 1)
            {
                ExportGumpImage(selectedIndexes[0], imageFormat);
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择导出目录";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Cursor.Current = Cursors.WaitCursor;
                foreach (int index in selectedIndexes)
                {
                    string fileName = Path.Combine(dialog.SelectedPath, $"Gump 0x{index:X4}.{fileExtension}");
                    if (Gumps.IsValidIndex(index))
                    {
                        using (Bitmap bit = new Bitmap(Gumps.GetGump(index)))
                        {
                            bit.Save(fileName, imageFormat);
                        }
                    }
                }
                Cursor.Current = Cursors.Default;

                MessageBox.Show($"已成功导出所选的 {selectedIndexes.Count} 个 Gump。", "导出完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void ExportGumpImage(int index, ImageFormat imageFormat)
        {
            // The menu entries reach this with the selected id, which is -1 when nothing is selected.
            if (index < 0 || !Gumps.IsValidIndex(index))
            {
                return;
            }

            string fileExtension = Utils.GetFileExtensionFor(imageFormat);
            string fileName = Path.Combine(Options.OutputPath, $"Gump {Utils.FormatExportId(index)}.{fileExtension}");

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

                using (new WaitCursorScope(this))
                {
                    foreach (int index in _ids)
                    {
                        if (index < 0)
                        {
                            continue;
                        }

                        string fileName = Path.Combine(dialog.SelectedPath, $"Gump {Utils.FormatExportId(index)}.{fileExtension}");
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
                }

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
            Gumps.PreloadParallel(0, done => PreLoader.ReportProgress(done));
        }

        private void PreLoaderProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = Math.Min(ProgressBar.Maximum, Math.Max(ProgressBar.Minimum, e.ProgressPercentage));
        }

        private void PreLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ProgressBar.Visible = false;
        }

        internal static void Select(int gumpId)
        {
            if (_refMarker == null)
            {
                return;
            }

            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            TabPageNavigator.ActivateOwningTabPage(_refMarker);

            Search(gumpId);
        }

        public static bool HasGumpId(int gumpId)
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            return _refMarker._ids.BinarySearch(gumpId) >= 0;
        }

        private void JumpToMaleFemale_Click(object sender, EventArgs e)
        {
            int gumpId = SelectedGumpId;
            if (gumpId < 0)
            {
                return;
            }

            gumpId = gumpId < 60000 ? (gumpId % 10000) + 60000 : (gumpId % 10000) + 50000;

            Select(gumpId);
        }

        public static bool Search(int graphic)
        {
            if (!_refMarker._loaded)
            {
                _refMarker.OnLoad(EventArgs.Empty);
            }

            int position = _refMarker._ids.BinarySearch(graphic);
            if (position < 0)
            {
                return false;
            }

            _refMarker.SelectPosition(position);

            return true;
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

                InsertId(index);
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