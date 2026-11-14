/***************************************************************************
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
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The editor's tile picker: one search box over pages of tiles (the multieditor.xml tree),
    /// favorites, recently placed tiles, house part styles and prefabs. Data files are read on
    /// first show, and a missing file only leaves its page empty.
    /// </summary>
    internal sealed partial class TileBrowserPanel : UserControl
    {
        private const string _messageTitle = "Multi Editor";

        /// <summary>Tile type filter choices, tested on tiledata.</summary>
        private static readonly (string Name, Func<ItemData, bool> Matches)[] _tileTypes =
        {
            ("Type: all (pick to filter by tiledata flags)", _ => true),
            ("Walls", d => (d.Flags & TileFlag.Wall) != 0),
            ("Floors (flat surfaces)", d => (d.Flags & TileFlag.Surface) != 0 && d.Height == 0),
            ("Roofs", d => (d.Flags & TileFlag.Roof) != 0),
            ("Doors", d => (d.Flags & TileFlag.Door) != 0),
            ("Windows", d => (d.Flags & TileFlag.Window) != 0),
            ("Stairs", d => (d.Flags & (TileFlag.StairBack | TileFlag.StairRight)) != 0),
            ("Foliage", d => (d.Flags & TileFlag.Foliage) != 0),
            ("Light sources", d => (d.Flags & TileFlag.LightSource) != 0),
            ("Containers", d => (d.Flags & TileFlag.Container) != 0),
            ("Flat (height 0)", d => d.Height == 0),
            ("Tall (height 15 or more)", d => d.Height >= 15)
        };

        private readonly TileBrowserThumbnails _thumbnails = new TileBrowserThumbnails();
        private readonly List<TileBrowserGrid> _grids = new List<TileBrowserGrid>();

        // Tiles page
        private readonly TileBrowserGrid _tilesGrid;
        private readonly HashSet<string> _expandedGroups = new HashSet<string>(StringComparer.Ordinal);
        private TileBrowserCatalog _catalog;
        private TileBrowserCategory _category;
        private TileBrowserCategory _categoryBeforeSearch;
        private bool _categoryPickedDuringSearch;
        private bool _suppressTreeEvents;
        private bool _splitPlaced;

        // Recent page
        private readonly TileBrowserGrid _recentGrid;
        private List<int> _recentTiles;

        private TileBrowserQuery _query;
        private string _queryText = string.Empty;
        private bool _suppressFilterEvents;
        private int _currentTile = -1;
        private bool _loaded;
        private bool _firstFillDone;

        // Each page remembers the filter it was built for, so switching tabs keeps its scroll.
        private int _filterVersion;
        private int _tilesVersion = -1;
        private int _favoritesVersion = -1;
        private int _recentVersion = -1;
        private int _houseVersion = -1;
        private int _prefabsVersion = -1;

        /// <summary>
        /// House styles are not passed here: they arrive through <see cref="SetHouseStyles"/>,
        /// before or after the panel is first shown.
        /// </summary>
        public TileBrowserPanel()
        {
            InitializeComponent();

            // The type filter choices come from the table above; filled quietly, as the
            // selection is not a filter change.
            _suppressFilterEvents = true;
            try
            {
                foreach (var (name, _) in _tileTypes)
                {
                    _typeFilter.Items.Add(name);
                }

                _typeFilter.SelectedIndex = 0;
            }
            finally
            {
                _suppressFilterEvents = false;
            }

            // The grids share the thumbnail cache, which the designer cannot pass to a constructor,
            // so they are made here. Docking runs from the last added control (fill first, edges
            // after), so each fill grid goes to the front of its page.
            SplitterGrip.Attach(_tilesSplit);
            _tilesGrid = CreateGrid("No tiles match.");
            _tilesGrid.TabIndex = 0;
            _tilesSplit.Panel2.Controls.Add(_tilesGrid);

            BuildFavoritesPage();

            _recentGrid = CreateGrid("Tiles you place show up here.");
            _recentGrid.TabIndex = 0;
            _recentPage.Controls.Add(_recentGrid);
            _recentGrid.BringToFront();

            BuildHousePage();

            Disposed += TileBrowserPanel_Disposed;
        }

        /// <summary>The user picked a tile to draw with.</summary>
        public event Action<ushort> TileChosen;

        /// <summary>The user wants a UO Architect design from the Designs page opened as a new multi.</summary>
        public event Action<IReadOnlyList<UoaDesign>> DesignOpenRequested;

        /// <summary>The user wants a UO Architect design placed into the multi being edited.</summary>
        public event Action<UoaDesign> DesignPlaceRequested;

        /// <summary>The user picked a house part style; the shell switches to the tool for <see cref="HouseStyle.Kind"/>.</summary>
        public event Action<HouseStyle> HouseStyleChosen;

        /// <summary>The user picked the foundation entry of the House page.</summary>
        public event Action FoundationChosen;

        /// <summary>The user activated a prefab: the shell starts a paste with these tiles.</summary>
        public event Action<List<ClipboardTile>> PrefabChosen;

        /// <summary>
        /// The House page's swap button: swap the selected pieces to the same columns of this style
        /// (the GDI editor's "Swap selection to this style").
        /// </summary>
        public event Action<HouseStyle> SwapToHouseStyleRequested;

        /// <summary>The current draw tile, or -1.</summary>
        public int CurrentTile => _currentTile;

        /// <summary>The house style last picked on the House page, or null.</summary>
        public HouseStyle CurrentHouseStyle => _houseStyle;

        /// <summary>The style the foundation entry uses (walls with the foundation post), or null.</summary>
        public HouseStyle FoundationStyle => _houseStyles?.Styles.FirstOrDefault(IsFoundationStyle);

        /// <summary>
        /// Highlights the id as the current draw tile and scrolls to it when the shown list has it.
        /// Does not raise <see cref="TileChosen"/>.
        /// </summary>
        public void SetCurrentTile(ushort id)
        {
            SetCurrent(id);
            ActiveGrid?.ScrollToId(id);
        }

        /// <summary>A tool placed the tile: it moves to the front of Recent, which is saved.</summary>
        public void RecordRecent(ushort id)
        {
            EnsureRecent();
            _recentTiles.Remove(id);
            _recentTiles.Insert(0, id);
            if (_recentTiles.Count > TileRecentlyUsed.MaxRecentTiles)
            {
                _recentTiles.RemoveRange(TileRecentlyUsed.MaxRecentTiles, _recentTiles.Count - TileRecentlyUsed.MaxRecentTiles);
            }

            TileRecentlyUsed.Save(_recentTiles);
            _recentVersion = -1;
            if (_loaded && _tabs.SelectedTab == _recentPage)
            {
                RefreshRecentGrid(keepScroll: false);
            }
        }

        /// <summary>Re-reads the prefab folder.</summary>
        public void RefreshPrefabs()
        {
            if (_prefabsLoaded)
            {
                LoadPrefabs();
            }
        }

        /// <summary>Art or tiledata changed: drops thumbnails, re-reads the category tree and repaints.</summary>
        public void ReloadData()
        {
            _thumbnails.Clear();
            if (!_loaded)
            {
                return;
            }

            string categoryPath = _category?.Path;
            string beforeSearchPath = _categoryBeforeSearch?.Path;
            _catalog = TileBrowserCatalog.Load();
            _category = FindCategory(categoryPath);
            _categoryBeforeSearch = FindCategory(beforeSearchPath);

            ReloadHouseIcons();
            if (_prefabsLoaded)
            {
                LoadPrefabs();
            }

            _filterVersion++;
            RefreshSelectedPage(keepScroll: true);
            foreach (TileBrowserGrid grid in _grids)
            {
                grid.Invalidate();
            }
        }

        /// <summary>The client changed: show its house design tables.</summary>
        public void SetHouseStyles(HouseStyles houseStyles)
        {
            _houseStyles = houseStyles;
            _houseStyle = null;

            // The panel is constructed before the shell loads the settings, so the saved height is applied here.
            _housePieces.Height = Math.Clamp(EditorSettings.Current.HousePiecesHeight, 80, 800);
            if (_houseLoaded)
            {
                RebuildHouseTree();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                EnsureLoaded();
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Added to a parent that is already showing, the panel gets no VisibleChanged.
            // Posted, so the load runs once the panel has its size.
            BeginInvoke(new Action(() =>
            {
                if (Visible)
                {
                    EnsureLoaded();
                }
            }));
        }

        private void TileBrowserPanel_Disposed(object sender, EventArgs e)
        {
            // The timer, tooltip, menus and image list are in components; the thumbnail cache
            // and the (never parented) pieces caption label are not.
            _thumbnails.Dispose();
            _housePiecesLabel?.Dispose();
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSelectedPage();
        }

        private void DesignLibrary_OpenRequested(IReadOnlyList<UoaDesign> picked)
        {
            DesignOpenRequested?.Invoke(picked);
        }

        private void DesignLibrary_PlaceRequested(UoaDesign design)
        {
            DesignPlaceRequested?.Invoke(design);
        }

        private void TilesSplit_SizeChanged(object sender, EventArgs e)
        {
            PlaceSplitter();
        }

        private void EnsureLoaded()
        {
            if (_loaded || DesignMode)
            {
                return;
            }

            _loaded = true;
            EnsureRecent();
            EnsureFavorites();
            _catalog = TileBrowserCatalog.Load();
            PlaceSplitter();
            RefreshSelectedPage();
        }

        private void EnsureRecent()
        {
            _recentTiles ??= TileRecentlyUsed.Load();
        }

        private void PlaceSplitter()
        {
            int height = _tilesSplit.Height;
            if (_splitPlaced || height < _tilesSplit.Panel1MinSize + _tilesSplit.Panel2MinSize + 100)
            {
                return;
            }

            _splitPlaced = true;
            int max = height - _tilesSplit.Panel2MinSize - _tilesSplit.SplitterWidth;
            _tilesSplit.SplitterDistance = Math.Clamp(height / 3, _tilesSplit.Panel1MinSize, max);
        }

        // ---------------------------------------------------------------- grids and current tile

        private TileBrowserGrid ActiveGrid =>
            _tabs.SelectedTab == _tilesPage ? _tilesGrid :
            _tabs.SelectedTab == _favoritesPage ? _favoritesGrid :
            _tabs.SelectedTab == _recentPage ? _recentGrid : null;

        private TileBrowserGrid CreateGrid(string emptyText)
        {
            var grid = new TileBrowserGrid(_thumbnails) { Dock = DockStyle.Fill, EmptyText = emptyText, IsMarked = IsFavorite };
            grid.TileActivated += (_, id) => ChooseTile(id);
            grid.TileContextMenuRequested += ShowTileMenu;
            grid.CellSizeChanged += size =>
            {
                foreach (TileBrowserGrid other in _grids)
                {
                    other.CellSize = size;
                }
            };
            grid.KeyPress += Grid_KeyPress;
            _grids.Add(grid);
            return grid;
        }

        private void ChooseTile(int id)
        {
            if (id < 0 || id > ushort.MaxValue)
            {
                return;
            }

            SetCurrent(id);
            TileChosen?.Invoke((ushort)id);
        }

        private void SetCurrent(int id)
        {
            _currentTile = id;
            foreach (TileBrowserGrid grid in _grids)
            {
                grid.CurrentId = id;
            }

            UpdateStatus();
        }

        /// <summary>Typing on a grid goes to the search box, so search is always one keystroke away.</summary>
        private void Grid_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            _searchBox.Focus();
            _searchBox.AppendText(e.KeyChar.ToString());
            e.Handled = true;
        }

        private static List<int> Filter(IEnumerable<int> source, Func<int, bool> filter)
        {
            return filter == null ? source.ToList() : source.Where(filter).ToList();
        }

        /// <summary>The search and type filter as one test, or null when neither is set.</summary>
        private Func<int, bool> TileFilter()
        {
            TileBrowserQuery query = _query;
            Func<ItemData, bool> type = _typeFilter.SelectedIndex > 0 ? _tileTypes[_typeFilter.SelectedIndex].Matches : null;
            if (query == null && type == null)
            {
                return null;
            }

            return id => (query == null || query.MatchesTile(id)) &&
                         (type == null || (TileBrowserThumbnails.HasItemData(id) && type(TileData.ItemTable[id])));
        }

        // ---------------------------------------------------------------- search

        private void SearchTimer_Tick(object sender, EventArgs e)
        {
            ApplySearchNow();
        }

        private void TypeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_suppressFilterEvents)
            {
                OnFilterChanged();
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            if (_suppressFilterEvents)
            {
                return;
            }

            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    if (_searchBox.TextLength > 0)
                    {
                        _searchBox.Clear();
                        ApplySearchNow();
                    }

                    e.Handled = e.SuppressKeyPress = true;
                    break;
                case Keys.Down:
                    ApplySearchNow();
                    if (ActiveGrid is { } grid)
                    {
                        grid.Focus();
                        grid.FocusFirst();
                    }
                    else if (_tabs.SelectedTab == _housePage)
                    {
                        _houseGallery.Focus();
                    }
                    else if (_tabs.SelectedTab == _prefabsPage)
                    {
                        _prefabList.Focus();
                    }

                    e.Handled = true;
                    break;
                case Keys.Enter:
                    ApplySearchNow();
                    PickFirstResult();
                    e.Handled = e.SuppressKeyPress = true;
                    break;
            }
        }

        private void PickFirstResult()
        {
            if (ActiveGrid is { Count: > 0 } grid)
            {
                ChooseTile(grid.FocusedId >= 0 ? grid.FocusedId : grid.Items[0]);
                grid.ScrollToId(_currentTile);
            }
            else if (_tabs.SelectedTab == _housePage)
            {
                ActivateFirstHouseNode();
            }
            else if (_tabs.SelectedTab == _prefabsPage && _prefabList.Items.Count > 0)
            {
                _prefabList.Items[0].Selected = true;
                PlaceSelectedPrefab();
            }
        }

        private void ApplySearchNow()
        {
            _searchTimer.Stop();
            string text = _searchBox.Text.Trim();
            if (text == _queryText)
            {
                return;
            }

            TileBrowserQuery query = TileBrowserQuery.Parse(text);
            bool was = _query != null;
            bool now = query != null;
            if (!was && now)
            {
                // A search looks through every tile; picking a category in the filtered tree narrows it.
                _categoryBeforeSearch = _category;
                _category = null;
                _categoryPickedDuringSearch = false;
            }
            else if (was && !now && !_categoryPickedDuringSearch)
            {
                _category = _categoryBeforeSearch;
            }

            _queryText = text;
            _query = query;
            OnFilterChanged();
        }

        private void OnFilterChanged()
        {
            _filterVersion++;
            RefreshSelectedPage();
        }

        private void RefreshSelectedPage(bool keepScroll = false)
        {
            EnsureLoaded();
            if (!_loaded)
            {
                return;
            }

            TabPage page = _tabs.SelectedTab;
            _typeFilter.Enabled = page == _tilesPage || page == _favoritesPage || page == _recentPage;
            bool rebuilt = false;
            if (page == _tilesPage && _tilesVersion != _filterVersion)
            {
                RebuildCategoryTree();
                RefreshTilesGrid(keepScroll);
                rebuilt = true;
            }
            else if (page == _favoritesPage && _favoritesVersion != _filterVersion)
            {
                RefreshFavoritesGrid(keepScroll);
                rebuilt = true;
            }
            else if (page == _recentPage && _recentVersion != _filterVersion)
            {
                RefreshRecentGrid(keepScroll);
                rebuilt = true;
            }
            else if (page == _housePage)
            {
                EnsureHouse();
                if (_houseVersion != _filterVersion)
                {
                    RebuildHouseTree();
                }
            }
            else if (page == _prefabsPage)
            {
                EnsurePrefabs();
                if (_prefabsVersion != _filterVersion)
                {
                    RebuildPrefabList();
                }
            }

            // Results start at the top; only a first fill looks for the current tile.
            if (rebuilt && !keepScroll && _query == null && !_firstFillDone)
            {
                ActiveGrid?.ScrollToId(_currentTile);
            }

            _firstFillDone |= rebuilt;
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            TabPage page = _tabs.SelectedTab;
            string list;
            if (page == _tilesPage)
            {
                list = $"{_tilesGrid.Count:N0} tiles" + (_category != null ? $" in {_category.Name}" : string.Empty);
                if (_catalog?.Error != null)
                {
                    list += " (multieditor.xml: " + _catalog.Error + ")";
                }
            }
            else if (page == _favoritesPage)
            {
                list = $"{_favoritesGrid.Count:N0} tiles";
            }
            else if (page == _recentPage)
            {
                list = $"{_recentGrid.Count:N0} recent tiles";
            }
            else if (page == _housePage)
            {
                list = _houseStyle != null ? $"{_houseStyle.Kind}: {_houseStyle.Name}" : $"{_houseStyleCount} styles";
            }
            else
            {
                list = _prefabList.Items.Count == 0 && _query == null ? "No blueprints saved yet" : $"{_prefabList.Items.Count} blueprints";
            }

            string current = _currentTile >= 0 ? $"Current 0x{_currentTile:X4} {TileBrowserThumbnails.NameOf(_currentTile)}" : string.Empty;
            _status.Text = current.Length > 0 ? list + "  |  " + current : list;
        }

        // ---------------------------------------------------------------- tiles page

        private TileBrowserCategory FindCategory(string path)
        {
            if (path == null || _catalog == null)
            {
                return null;
            }

            foreach (TileBrowserCategory group in _catalog.Groups)
            {
                if (group.Path == path)
                {
                    return group;
                }

                foreach (TileBrowserCategory sub in group.Children)
                {
                    if (sub.Path == path)
                    {
                        return sub;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Rebuilds the tree for the current filter: while filtering it lists only categories with
        /// matches, each with its match count, so the tree shows where the results live.
        /// </summary>
        private void RebuildCategoryTree()
        {
            if (_catalog == null)
            {
                return;
            }

            Func<int, bool> filter = TileFilter();
            _suppressTreeEvents = true;
            _categoryTree.BeginUpdate();
            try
            {
                _categoryTree.Nodes.Clear();
                int allCount = filter == null ? _catalog.AllTiles.Count : _catalog.AllTiles.Count(filter);
                var allNode = new TreeNode($"All Tiles ({allCount:N0})") { ToolTipText = "Every static in the art file" };
                _categoryTree.Nodes.Add(allNode);
                TreeNode selected = allNode;

                foreach (TileBrowserCategory group in _catalog.Groups)
                {
                    int groupCount = filter == null ? group.Tiles.Count : group.Tiles.Count(filter);
                    if (filter != null && groupCount == 0)
                    {
                        continue;
                    }

                    var groupNode = new TreeNode($"{group.Name} ({groupCount})") { Tag = group };
                    foreach (TileBrowserCategory sub in group.Children)
                    {
                        int count = filter == null ? sub.Tiles.Count : sub.Tiles.Count(filter);
                        if (filter != null && count == 0)
                        {
                            continue;
                        }

                        var subNode = new TreeNode($"{sub.Name} ({count})") { Tag = sub };
                        groupNode.Nodes.Add(subNode);
                        if (sub == _category)
                        {
                            selected = subNode;
                        }
                    }

                    _categoryTree.Nodes.Add(groupNode);
                    if (group == _category)
                    {
                        selected = groupNode;
                    }

                    if (filter != null || _expandedGroups.Contains(group.Path))
                    {
                        groupNode.Expand();
                    }
                }

                if (_catalog.Groups.Count == 0)
                {
                    _categoryTree.Nodes.Add(new TreeNode(_catalog.Error != null ? "multieditor.xml could not be read" : "No multieditor.xml in the plugins folder")
                    {
                        ForeColor = SystemColors.GrayText,
                        ToolTipText = TileBrowserCatalog.FilePath
                    });
                }

                // The category may be filtered out of the tree: show everything that matches instead.
                _category = selected.Tag as TileBrowserCategory;
                _categoryTree.SelectedNode = selected;
                selected.EnsureVisible();
            }
            finally
            {
                _categoryTree.EndUpdate();
                _suppressTreeEvents = false;
            }
        }

        private void CategoryTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            RememberExpansion(e.Node, true);
        }

        private void CategoryTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            RememberExpansion(e.Node, false);
        }

        private void RememberExpansion(TreeNode node, bool expanded)
        {
            // Expansion forced by a filter is not the user's choice.
            if (_suppressTreeEvents || TileFilter() != null || node.Tag is not TileBrowserCategory { Parent: null } group)
            {
                return;
            }

            if (expanded)
            {
                _expandedGroups.Add(group.Path);
            }
            else
            {
                _expandedGroups.Remove(group.Path);
            }
        }

        private void CategoryTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_suppressTreeEvents || e.Node == null)
            {
                return;
            }

            if (e.Node.Tag is not TileBrowserCategory && e.Node.Parent == null && e.Node.Index > 0)
            {
                // The "no multieditor.xml" hint.
                return;
            }

            _category = e.Node.Tag as TileBrowserCategory;
            if (_query != null)
            {
                _categoryPickedDuringSearch = true;
            }

            RefreshTilesGrid(keepScroll: false);
            _tilesGrid.ScrollToId(_currentTile);
            UpdateStatus();
        }

        private void RefreshTilesGrid(bool keepScroll)
        {
            if (_catalog == null)
            {
                return;
            }

            Func<int, bool> filter = TileFilter();
            IReadOnlyList<int> source = _category != null ? _category.Tiles : _catalog.AllTiles;
            _tilesGrid.SetItems(filter == null ? source : Filter(source, filter), keepScroll);
            _tilesVersion = _filterVersion;
        }

        /// <summary>Opens the first category listing the tile, with the search cleared, and scrolls to it.</summary>
        private void ShowInCategory(int id)
        {
            TileBrowserCategory category = _catalog?.FindCategoryOf(id);
            if (category == null)
            {
                return;
            }

            _suppressFilterEvents = true;
            try
            {
                _searchBox.Clear();
                _typeFilter.SelectedIndex = 0;
            }
            finally
            {
                _suppressFilterEvents = false;
            }

            _searchTimer.Stop();
            _query = null;
            _queryText = string.Empty;
            _category = category;
            _filterVersion++;
            if (_tabs.SelectedTab != _tilesPage)
            {
                // Its SelectedIndexChanged refreshes the page.
                _tabs.SelectedTab = _tilesPage;
            }
            else
            {
                RefreshSelectedPage();
            }

            _tilesGrid.ScrollToId(id);
            _tilesGrid.Focus();
        }

        // ---------------------------------------------------------------- recent page

        private void RefreshRecentGrid(bool keepScroll)
        {
            EnsureRecent();
            _recentGrid.SetItems(Filter(_recentTiles, TileFilter()), keepScroll);
            _recentVersion = _filterVersion;
            UpdateStatus();
        }

        private void ClearRecentButton_Click(object sender, EventArgs e)
        {
            ClearRecent();
        }

        private void ClearRecent()
        {
            EnsureRecent();
            if (_recentTiles.Count == 0)
            {
                return;
            }

            _recentTiles.Clear();
            TileRecentlyUsed.Save(_recentTiles);
            RefreshRecentGrid(keepScroll: false);
        }

        // ---------------------------------------------------------------- tile context menu

        private void ShowTileMenu(TileBrowserGrid grid, int id, Point location)
        {
            EnsureFavorites();
            ClearMenu(_tileMenu.Items);

            _tileMenu.Items.Add(new ToolStripMenuItem($"0x{id:X4}  {TileBrowserThumbnails.NameOf(id)}") { Enabled = false });

            var use = new ToolStripMenuItem("Draw with this tile");
            use.Click += (_, _) => ChooseTile(id);
            _tileMenu.Items.Add(use);
            _tileMenu.Items.Add(new ToolStripSeparator());

            var add = new ToolStripMenuItem("Add to favorites");
            foreach (FavoritePage page in _favorites.Pages)
            {
                var item = new ToolStripMenuItem(page.Name) { Enabled = !page.Tiles.Contains(id) };
                item.Click += (_, _) => AddFavorite(page, id);
                add.DropDownItems.Add(item);
            }

            add.DropDownItems.Add(new ToolStripSeparator());
            var newPage = new ToolStripMenuItem("New page...");
            newPage.Click += (_, _) =>
            {
                if (NewFavoritePage() is { } page)
                {
                    AddFavorite(page, id);
                }
            };
            add.DropDownItems.Add(newPage);
            _tileMenu.Items.Add(add);

            // On the Favorites page "remove" means the page shown; elsewhere any page holding the tile.
            List<FavoritePage> holders = grid == _favoritesGrid && CurrentFavoritePage is { } shown
                ? (shown.Tiles.Contains(id) ? new List<FavoritePage> { shown } : new List<FavoritePage>())
                : _favorites.Pages.Where(p => p.Tiles.Contains(id)).ToList();
            if (holders.Count == 1)
            {
                FavoritePage page = holders[0];
                var remove = new ToolStripMenuItem($"Remove from favorites (\"{page.Name}\")");
                remove.Click += (_, _) => RemoveFavorite(page, id);
                _tileMenu.Items.Add(remove);
            }
            else if (holders.Count > 1)
            {
                var remove = new ToolStripMenuItem("Remove from favorites");
                foreach (FavoritePage page in holders)
                {
                    var item = new ToolStripMenuItem(page.Name);
                    item.Click += (_, _) => RemoveFavorite(page, id);
                    remove.DropDownItems.Add(item);
                }

                _tileMenu.Items.Add(remove);
            }

            _tileMenu.Items.Add(new ToolStripSeparator());
            var copy = new ToolStripMenuItem("Copy id");
            copy.Click += (_, _) => CopyId(id);
            _tileMenu.Items.Add(copy);

            TileBrowserCategory category = _catalog?.FindCategoryOf(id);
            var show = new ToolStripMenuItem(category != null ? $"Show in category ({category.Path})" : "Show in category")
            {
                Enabled = category != null && !(grid == _tilesGrid && _category == category && TileFilter() == null)
            };
            show.Click += (_, _) => ShowInCategory(id);
            _tileMenu.Items.Add(show);

            _tileMenu.Items.Add(new ToolStripSeparator());
            var showIds = new ToolStripMenuItem("Show ids under tiles") { Checked = grid.ShowIds };
            showIds.Click += (_, _) =>
            {
                bool value = !grid.ShowIds;
                foreach (TileBrowserGrid other in _grids)
                {
                    other.ShowIds = value;
                }
            };
            _tileMenu.Items.Add(showIds);

            _tileMenu.Show(grid, location);
        }

        private static void ClearMenu(ToolStripItemCollection items)
        {
            while (items.Count > 0)
            {
                ToolStripItem item = items[0];
                items.RemoveAt(0);
                item.Dispose();
            }
        }

        private static void CopyId(int id)
        {
            try
            {
                Clipboard.SetText($"0x{id:X4}");
            }
            catch (ExternalException)
            {
                // The clipboard is held by another program; nothing to do but skip it.
            }
        }

        // ---------------------------------------------------------------- favorites page

        private readonly HashSet<int> _favoriteIds = new HashSet<int>();
        private TileFavorites _favorites;
        private TileBrowserGrid _favoritesGrid;
        private int _favoritePageIndex;
        private bool _suppressFavoriteEvents;

        private FavoritePage CurrentFavoritePage =>
            _favorites != null && _favoritePageIndex >= 0 && _favoritePageIndex < _favorites.Pages.Count ? _favorites.Pages[_favoritePageIndex] : null;

        /// <summary>The page's grid; its toolbar is in the designer.</summary>
        private void BuildFavoritesPage()
        {
            _favoritesGrid = CreateGrid("This page is empty. Right-click a tile anywhere and choose \"Add to favorites\".");
            _favoritesGrid.KeyDown += FavoritesGrid_KeyDown;
            _favoritesGrid.TabIndex = 0;
            _favoritesPage.Controls.Add(_favoritesGrid);
            _favoritesGrid.BringToFront();
        }

        private void FavoritesGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && CurrentFavoritePage is { } page && _favoritesGrid.FocusedId >= 0)
            {
                RemoveFavorite(page, _favoritesGrid.FocusedId);
                e.Handled = true;
            }
        }

        private void FavoritePageBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressFavoriteEvents || _favoritePageBox.SelectedIndex < 0)
            {
                return;
            }

            _favoritePageIndex = _favoritePageBox.SelectedIndex;
            RefreshFavoritesGrid(keepScroll: false);
        }

        private void AddFavoriteButton_Click(object sender, EventArgs e)
        {
            if (_currentTile >= 0 && CurrentFavoritePage is { } page)
            {
                AddFavorite(page, _currentTile);
            }
        }

        private void RemoveFavoriteButton_Click(object sender, EventArgs e)
        {
            if (_currentTile >= 0 && CurrentFavoritePage is { } page)
            {
                RemoveFavorite(page, _currentTile);
            }
        }

        private void NewFavoritePageItem_Click(object sender, EventArgs e)
        {
            if (NewFavoritePage() is { } page)
            {
                _favoritePageIndex = _favorites.Pages.IndexOf(page);
                SaveFavorites();
                RefreshFavoritesGrid(keepScroll: false);
            }
        }

        private void RenamePageItem_Click(object sender, EventArgs e)
        {
            if (CurrentFavoritePage is { } page && Prompt.Text(this, "Rename favorites page", "Page name:", page.Name) is { } name)
            {
                page.Name = name;
                SaveFavorites();
            }
        }

        private void DeletePageItem_Click(object sender, EventArgs e)
        {
            DeleteFavoritePage();
        }

        private void ImportUoheItem_Click(object sender, EventArgs e)
        {
            ImportUoheBookmarks();
        }

        private void FavoritePageMenu_DropDownOpening(object sender, EventArgs e)
        {
            EnsureFavorites();
            _renamePageItem.Enabled = CurrentFavoritePage != null;
            _deletePageItem.Enabled = _favorites.Pages.Count > 1;
        }

        private void EnsureFavorites()
        {
            if (_favorites != null)
            {
                return;
            }

            _favorites = TileFavorites.Load();
            RebuildFavoriteIds();
            RefreshFavoritePageBox();
        }

        private bool IsFavorite(int id) => _favoriteIds.Contains(id);

        private void RebuildFavoriteIds()
        {
            _favoriteIds.Clear();
            foreach (FavoritePage page in _favorites.Pages)
            {
                _favoriteIds.UnionWith(page.Tiles);
            }
        }

        private void RefreshFavoritePageBox()
        {
            _suppressFavoriteEvents = true;
            try
            {
                _favoritePageBox.Items.Clear();
                foreach (FavoritePage page in _favorites.Pages)
                {
                    _favoritePageBox.Items.Add($"{page.Name} ({page.Tiles.Count})");
                }

                _favoritePageIndex = Math.Clamp(_favoritePageIndex, 0, Math.Max(0, _favorites.Pages.Count - 1));
                _favoritePageBox.SelectedIndex = _favorites.Pages.Count > 0 ? _favoritePageIndex : -1;
            }
            finally
            {
                _suppressFavoriteEvents = false;
            }
        }

        private void RefreshFavoritesGrid(bool keepScroll)
        {
            EnsureFavorites();
            IEnumerable<int> tiles = CurrentFavoritePage?.Tiles ?? Enumerable.Empty<int>();
            _favoritesGrid.SetItems(Filter(tiles, TileFilter()), keepScroll);
            _favoritesVersion = _filterVersion;
            UpdateStatus();
        }

        /// <summary>Saves, then brings the page list, the shown page and every grid's marks up to date.</summary>
        private void SaveFavorites()
        {
            try
            {
                _favorites.Save();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, $"Could not save the favorites: {ex.Message}", _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            RebuildFavoriteIds();
            RefreshFavoritePageBox();
            _favoritesVersion = -1;
            if (_tabs.SelectedTab == _favoritesPage)
            {
                RefreshFavoritesGrid(keepScroll: true);
            }

            foreach (TileBrowserGrid grid in _grids)
            {
                grid.Invalidate();
            }
        }

        private void AddFavorite(FavoritePage page, int id)
        {
            if (!page.Tiles.Contains(id))
            {
                page.Tiles.Add(id);
                SaveFavorites();
            }
        }

        private void RemoveFavorite(FavoritePage page, int id)
        {
            if (page.Tiles.Remove(id))
            {
                SaveFavorites();
            }
        }

        private FavoritePage NewFavoritePage()
        {
            EnsureFavorites();
            string name = Prompt.Text(this, "New favorites page", "Page name:");
            if (name == null)
            {
                return null;
            }

            var page = new FavoritePage(name);
            _favorites.Pages.Add(page);
            return page;
        }

        private void DeleteFavoritePage()
        {
            if (CurrentFavoritePage is not { } page || _favorites.Pages.Count <= 1)
            {
                return;
            }

            if (MessageBox.Show(this, $"Delete the favorites page \"{page.Name}\" ({page.Tiles.Count} tiles)?", _messageTitle,
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            _favorites.Pages.Remove(page);
            SaveFavorites();
            RefreshFavoritesGrid(keepScroll: false);
        }

        private void ImportUoheBookmarks()
        {
            EnsureFavorites();
            using var dialog = new OpenFileDialog
            {
                Filter = "UOHE bookmarks (*.ini)|*.ini|All files (*.*)|*.*",
                Title = "Import UOHE bookmarks"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                int before = _favorites.Pages.Count;
                int pages = _favorites.ImportUoheBookmarks(dialog.FileName);
                if (pages > 0)
                {
                    _favoritePageIndex = before;
                }

                SaveFavorites();
                RefreshFavoritesGrid(keepScroll: false);
                MessageBox.Show(this, $"{pages} page(s) imported.", _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, $"Could not read {dialog.FileName}: {ex.Message}", _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------------------------------------------------------------- house page

        /// <summary>The kinds of house parts, in the order you build with them.</summary>
        private static readonly (HousePartKind Kind, string Title, string IconSlot, string Hint)[] _houseKinds =
        {
            (HousePartKind.Wall, "Walls", "Corner", "Drag a rectangle for a room, or along one row for a single wall. Shift: straight wall. Ctrl: erase."),
            (HousePartKind.Floor, "Floors", "F1", "Drag an area to lay the floor. Flat tiles already there are replaced."),
            (HousePartKind.Door, "Doors", "Piece3", "Click a wall to hang the door. Click the door again to turn it."),
            (HousePartKind.Stair, "Stairs", "North", "Press on the bottom step and drag the way the stairs climb. They rise one floor."),
            (HousePartKind.Roof, "Roofs", "EWCrosspiece", "Click a walled room to roof it, or drag over the walls to size it by hand. Ridge, gables and fill are in the options bar.")
        };

        private const string _foundationHint = "Drag the house area: a low wall around a dirt floor 7 above it.";

        private readonly Dictionary<HousePartKind, HouseStyle> _lastStyleOfKind = new Dictionary<HousePartKind, HouseStyle>();
        private readonly Dictionary<HousePartKind, ToolStripButton> _houseKindButtons = new Dictionary<HousePartKind, ToolStripButton>();
        private HouseStyles _houseStyles;
        private HouseStyle _houseStyle;
        private HousePartKind _houseKind = HousePartKind.Wall;
        private bool _foundationPicked;
        private const string _swapText = "Swap selection to this style";
        private bool _patternsShown;
        private FloorPattern _pattern;
        private TileBrowserGrid _housePieces;
        private Label _housePiecesLabel;
        private int _houseStyleCount;
        private bool _houseLoaded;

        private static bool IsFoundationStyle(HouseStyle style) =>
            style.Kind == HousePartKind.Wall && style.Get("Post") == FoundationTool.FoundationPost;

        /// <summary>
        /// The House page: a row of part kinds, the styles of the chosen kind as pictures, and
        /// under them the single pieces of the chosen style. The designer holds the page's fixed
        /// parts; the kind buttons (one per <see cref="_houseKinds"/> entry) and the pieces grid
        /// are added here.
        /// </summary>
        private void BuildHousePage()
        {
            // Kind buttons go between Foundation and Patterns.
            foreach (var (kind, title, _, hint) in _houseKinds)
            {
                var button = new ToolStripButton(title) { ToolTipText = hint };
                button.Click += (_, _) => ShowHouseKind(kind, activate: true);
                _houseKindButtons[kind] = button;
                _houseKindBar.Items.Insert(_houseKindBar.Items.IndexOf(_patternsButton), button);
            }

            // The pieces of the chosen style: click one to draw with exactly that tile.
            // Roughly half the page each by default; the splitter above the pieces moves the line.
            _housePieces = new TileBrowserGrid(_thumbnails)
            {
                Dock = DockStyle.Bottom,
                Height = Math.Clamp(EditorSettings.Current.HousePiecesHeight, 80, 800),
                EmptyText = "Pick a style to see its pieces."
            };
            _housePieces.CellSize = 64;
            _housePieces.TileActivated += (_, id) => ChooseTile(id);
            _housePieces.TileContextMenuRequested += ShowTileMenu;
            _housePiecesLabel = new Label { Dock = DockStyle.Bottom, Height = 18, Text = "Pieces", ForeColor = SystemColors.GrayText };

            // Docking runs from the last added control: the pieces sit right after the splitter,
            // so they dock below it and above the swap button.
            _housePieces.TabIndex = 2;
            _housePage.Controls.Add(_housePieces);
            _housePage.Controls.SetChildIndex(_housePieces, _housePage.Controls.GetChildIndex(_piecesSplitter) + 1);
        }

        private void FoundationButton_Click(object sender, EventArgs e)
        {
            PickFoundation();
        }

        private void PatternsButton_Click(object sender, EventArgs e)
        {
            ShowPatterns(null);
        }

        private void HouseGallery_TagChosen(object tag)
        {
            if (tag is FloorPattern pattern)
            {
                PickPattern(pattern);
            }
        }

        private void SwapButton_Click(object sender, EventArgs e)
        {
            if (_patternsShown)
            {
                DeletePattern();
            }
            else if (_houseStyle != null)
            {
                SwapToHouseStyleRequested?.Invoke(_houseStyle);
            }
        }

        private void PiecesSplitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            EditorSettings settings = EditorSettings.Current.Clone();
            settings.HousePiecesHeight = _housePieces.Height;
            settings.Apply();
        }

        /// <summary>
        /// Opens the House page on a kind of part (or the foundation) and picks its style, as if the
        /// kind button had been pressed. Called when a house tool is chosen from the tool palette.
        /// </summary>
        public void ShowHousePage(HousePartKind kind, bool foundation)
        {
            EnsureLoaded();
            if (_tabs.SelectedTab != _housePage)
            {
                _tabs.SelectedTab = _housePage;
            }

            EnsureHouse();
            if (foundation)
            {
                if (_foundationButton.Enabled)
                {
                    PickFoundation();
                }
            }
            else if (_houseKindButtons[kind].Enabled)
            {
                ShowHouseKind(kind, activate: true);
            }
        }

        private void EnsureHouse()
        {
            if (!_houseLoaded)
            {
                _houseLoaded = true;
                RebuildHouseTree();
            }
        }

        private void ReloadHouseIcons()
        {
            if (_houseLoaded)
            {
                RebuildHouseTree();
            }
        }

        private bool HouseStyleMatches(HouseStyle style)
        {
            TileBrowserQuery query = _query;
            return query == null ||
                   (query.IsIdQuery ? style.Slots.Values.Any(id => query.MatchesId(id)) : query.MatchesText(style.Group + " " + style.Name));
        }

        /// <summary>
        /// Shows the styles of the current kind that match the search (by style or material name,
        /// or by a piece's id), and how many each kind has.
        /// </summary>
        private void RebuildHouseTree()
        {
            if (_patternsShown)
            {
                RebuildPatterns();
                return;
            }

            _patternsButton.Checked = false;
            _swapButton.Text = _swapText;
            _swapButton.Enabled = true;
            List<HouseStyle> styles = _houseStyles?.Styles ?? new List<HouseStyle>();
            _houseStyleCount = 0;

            foreach (var (kind, title, _, _) in _houseKinds)
            {
                int count = styles.Count(st => st.Kind == kind && HouseStyleMatches(st));
                _houseStyleCount += count;
                ToolStripButton button = _houseKindButtons[kind];
                button.Text = $"{title} ({count})";
                button.Enabled = count > 0;
                button.Checked = !_foundationPicked && kind == _houseKind;
            }

            _foundationButton.Enabled = styles.Any(IsFoundationStyle);
            _foundationButton.Checked = _foundationPicked;

            // A search that empties the current kind moves on to the first kind with matches.
            if (!_houseKindButtons[_houseKind].Enabled)
            {
                foreach (var (kind, _, _, _) in _houseKinds)
                {
                    if (_houseKindButtons[kind].Enabled)
                    {
                        _houseKind = kind;
                        _houseKindButtons[kind].Checked = !_foundationPicked;
                        break;
                    }
                }
            }

            string iconSlot = _houseKinds.First(k => k.Kind == _houseKind).IconSlot;
            var sections = new List<HouseGallerySection>();
            foreach (IGrouping<string, HouseStyle> group in styles.Where(st => st.Kind == _houseKind && HouseStyleMatches(st)).GroupBy(st => st.Group))
            {
                var items = new List<HouseGalleryItem>();
                foreach (HouseStyle style in group)
                {
                    int icon = style.Get(iconSlot) is var preferred and not 0 ? preferred : style.IconId;
                    items.Add(new HouseGalleryItem(style, ShortStyleName(style), icon));
                }

                sections.Add(new HouseGallerySection(group.Key, items));
            }

            // Headings help when a material has several styles (walls). Where most groups hold a
            // single style (floors, doors), they only repeat its name and waste a row each.
            if (sections.Count == 1 || sections.Count(section => section.Items.Count == 1) * 2 > sections.Count)
            {
                var all = new List<HouseGalleryItem>();
                foreach (HouseGallerySection section in sections)
                {
                    all.AddRange(section.Items);
                }

                sections.Clear();
                sections.Add(new HouseGallerySection(null, all));
            }

            _houseGallery.EmptyText = _houseStyles == null || styles.Count == 0
                ? "No house design tables in this client."
                : _query != null ? "No house styles match." : "No styles of this kind.";
            _houseGallery.SetSections(sections);
            _houseGallery.Selected = _foundationPicked ? null : _houseStyle;
            _houseGallery.ScrollToSelected();

            ShowHousePieces();
            _houseVersion = _filterVersion;
            UpdateStatus();
        }

        /// <summary>The style name without the words its section heading or kind button already say.</summary>
        private static string ShortStyleName(HouseStyle style)
        {
            string name = style.Name;
            foreach (string word in new[] { " Walls", " Wall", " Floors", " Floor", " Doors", " Door", " Stairs", " Roofs", " Roof" })
            {
                if (name.EndsWith(word, StringComparison.OrdinalIgnoreCase) && name.Length > word.Length)
                {
                    name = name[..^word.Length];
                    break;
                }
            }

            return name;
        }

        private void ShowHousePieces()
        {
            HouseStyle style = _houseStyle;
            var pieces = new List<int>();
            if (style != null)
            {
                foreach (KeyValuePair<string, ushort> slot in style.Slots)
                {
                    // Stair styles also list stair multis; those are multi ids, not art.
                    if (!slot.Key.StartsWith("Multi", StringComparison.Ordinal) && !pieces.Contains(slot.Value))
                    {
                        pieces.Add(slot.Value);
                    }
                }
            }

            _housePieces.SetItems(pieces);
            _housePiecesLabel.Text = style != null ? $"Pieces of {style.Name} (click one to draw with it)" : "Pieces";
            _houseHint.Text = _foundationPicked ? _foundationHint : _houseKinds.First(k => k.Kind == _houseKind).Hint;
        }

        /// <summary>
        /// Switches the gallery to a kind. With <paramref name="activate"/> the style last used for
        /// that kind (or its first one) is picked straight away, so the tool is ready to drag.
        /// </summary>
        private void ShowHouseKind(HousePartKind kind, bool activate)
        {
            _houseKind = kind;
            _foundationPicked = false;
            _patternsShown = false;
            if (activate)
            {
                _lastStyleOfKind.TryGetValue(kind, out HouseStyle last);
                _houseStyle = last;
            }

            RebuildHouseTree();
            if (!activate)
            {
                return;
            }

            HouseStyle pick = _houseStyle != null && HouseStyleMatches(_houseStyle) ? _houseStyle : _houseGallery.First();
            if (pick != null)
            {
                PickHouseStyle(pick);
            }
        }

        private void PickHouseStyle(HouseStyle style)
        {
            _foundationPicked = false;
            _patternsShown = false;
            _houseStyle = style;
            _houseKind = style.Kind;
            _lastStyleOfKind[style.Kind] = style;
            foreach (KeyValuePair<HousePartKind, ToolStripButton> button in _houseKindButtons)
            {
                button.Value.Checked = button.Key == style.Kind;
            }

            _foundationButton.Checked = false;
            _houseGallery.Selected = style;
            ShowHousePieces();
            UpdateStatus();
            HouseStyleChosen?.Invoke(style);
        }

        private void PickFoundation()
        {
            _foundationPicked = true;
            _patternsShown = false;
            _patternsButton.Checked = false;
            _swapButton.Text = _swapText;
            _houseStyle = FoundationStyle;
            foreach (ToolStripButton button in _houseKindButtons.Values)
            {
                button.Checked = false;
            }

            _foundationButton.Checked = true;
            _houseGallery.Selected = null;
            ShowHousePieces();
            UpdateStatus();
            FoundationChosen?.Invoke();
        }

        // ---------------------------------------------------------------- floor patterns

        /// <summary>The user picked a floor pattern; the shell switches to the floor pattern tool.</summary>
        public event Action<FloorPattern> FloorPatternChosen;

        /// <summary>
        /// Shows the floor patterns on the House page, re-read from disk, and picks one (the given
        /// one, the last used, or the first).
        /// </summary>
        public void ShowPatterns(FloorPattern select)
        {
            EnsureLoaded();
            if (_tabs.SelectedTab != _housePage)
            {
                _tabs.SelectedTab = _housePage;
            }

            EnsureHouse();
            _patternsShown = true;
            _foundationPicked = false;
            FloorPatterns.Reset();
            List<FloorPattern> all = FloorPatterns.All();
            FloorPattern pick = select != null ? all.Find(p => p.Name == select.Name)
                : _pattern != null ? all.Find(p => p.Name == _pattern.Name)
                : null;
            pick ??= all.Count > 0 ? all[0] : null;
            _pattern = pick;
            RebuildPatterns();
            if (pick != null)
            {
                FloorPatternChosen?.Invoke(pick);
            }
        }

        private void RebuildPatterns()
        {
            foreach (ToolStripButton button in _houseKindButtons.Values)
            {
                button.Checked = false;
            }

            _foundationButton.Checked = false;
            _patternsButton.Checked = true;

            TileBrowserQuery query = _query;
            var mine = new List<HouseGalleryItem>();
            var shipped = new List<HouseGalleryItem>();
            foreach (FloorPattern pattern in FloorPatterns.All())
            {
                if (query != null && !(query.IsIdQuery ? pattern.Pieces().Any(id => query.MatchesId(id)) : query.MatchesText(pattern.Name)))
                {
                    continue;
                }

                (pattern.BuiltIn ? shipped : mine).Add(new HouseGalleryItem(null, pattern.Name, pattern.IconId, pattern));
            }

            _houseGallery.EmptyText = "No floor patterns yet. Lay out one bordered floor by hand, select it, " +
                                      "and use File > Save selection as floor pattern.";
            _houseGallery.SetSections(new List<HouseGallerySection>
            {
                new HouseGallerySection("Your patterns", mine),
                new HouseGallerySection("Built in", shipped)
            });
            _houseGallery.Selected = null;
            _houseGallery.SelectedTag = _pattern;
            _houseGallery.ScrollToSelected();

            _housePieces.SetItems(_pattern != null ? _pattern.Pieces().Select(id => (int)id).ToList() : new List<int>());
            _housePiecesLabel.Text = _pattern != null ? $"Pieces of {_pattern.Name}" : "Pieces";
            _houseHint.Text = "Drag an area: corners, sides and fill are laid as in the pattern, repeated to fit.";
            _swapButton.Text = "Delete this pattern";
            _swapButton.Enabled = _pattern != null && !_pattern.BuiltIn;
            _houseVersion = _filterVersion;
            UpdateStatus();
        }

        private void PickPattern(FloorPattern pattern)
        {
            _pattern = pattern;
            RebuildPatterns();
            FloorPatternChosen?.Invoke(pattern);
        }

        private void DeletePattern()
        {
            if (_pattern == null || _pattern.BuiltIn ||
                MessageBox.Show(this, $"Delete the floor pattern \"{_pattern.Name}\"?", _messageTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            FloorPatterns.Delete(_pattern);
            _pattern = null;
            ShowPatterns(null);
        }

        private void ActivateFirstHouseNode()
        {
            if (_houseGallery.First() is { } style)
            {
                PickHouseStyle(style);
            }
        }

        // ---------------------------------------------------------------- blueprints page

        private const int _prefabIconSize = 80;

        private readonly List<PrefabEntry> _prefabs = new List<PrefabEntry>();
        private readonly List<string> _prefabGroups = new List<string>();
        private bool _prefabsLoaded;

        /// <summary>The group shown: null for all, empty for the blueprints in no group, else a group name.</summary>
        private string _prefabGroup;

        /// <summary>A blueprint file. Group is the subfolder it is in, empty for the blueprint folder itself.</summary>
        private sealed record PrefabEntry(string Name, string Group, string Path, int TileCount, int ImageIndex, HashSet<int> Ids);

        /// <summary>
        /// One click places a blueprint, like picking a tile; a right click opens its menu
        /// without starting a paste.
        /// </summary>
        private void PrefabList_MouseClick(object sender, MouseEventArgs e)
        {
            if (_prefabList.GetItemAt(e.X, e.Y) is not { } item)
            {
                return;
            }

            item.Selected = true;
            if (e.Button == MouseButtons.Left)
            {
                PlaceSelectedPrefab();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ShowPrefabMenu(e.Location);
            }
        }

        private void PrefabList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedPrefab();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                PlaceSelectedPrefab();
                e.Handled = e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                RenameSelectedPrefab();
                e.Handled = true;
            }
        }

        private void EnsurePrefabs()
        {
            if (!_prefabsLoaded)
            {
                LoadPrefabs();
            }
        }

        /// <summary>
        /// Saves tiles as a blueprint in the group being shown (no group when all are shown) and
        /// brings up the Blueprints page.
        /// </summary>
        public void SaveBlueprint(string name, List<ClipboardTile> tiles)
        {
            EnsureLoaded();
            try
            {
                string folder = GroupFolder(_prefabGroup ?? string.Empty);
                Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, Prefabs.SafeName(name) + ".txt"), ClipboardTile.Format(tiles));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, $"Could not save the blueprint: {ex.Message}", _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_tabs.SelectedTab != _prefabsPage)
            {
                _tabs.SelectedTab = _prefabsPage;
            }

            LoadPrefabs();
        }

        private static string GroupFolder(string group)
        {
            return string.IsNullOrEmpty(group) ? Prefabs.Folder : Path.Combine(Prefabs.Folder, group);
        }

        /// <summary>
        /// Reads every blueprint file and draws its thumbnail; unreadable files are skipped. A
        /// subfolder of the blueprint folder is a group, also when it is empty.
        /// </summary>
        private void LoadPrefabs()
        {
            _prefabsLoaded = true;
            _prefabs.Clear();
            _prefabGroups.Clear();
            _prefabList.Items.Clear();
            _prefabIcons.Images.Clear();

            try
            {
                if (Directory.Exists(Prefabs.Folder))
                {
                    AddPrefabsOf(string.Empty);
                    foreach (string folder in Directory.EnumerateDirectories(Prefabs.Folder).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        string group = Path.GetFileName(folder);
                        _prefabGroups.Add(group);
                        AddPrefabsOf(group);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // What could be read is shown.
            }

            if (!string.IsNullOrEmpty(_prefabGroup) && !_prefabGroups.Contains(_prefabGroup))
            {
                _prefabGroup = null;
            }

            RebuildPrefabGroupBar();
            RebuildPrefabList();
        }

        private void AddPrefabsOf(string group)
        {
            foreach (string path in Directory.EnumerateFiles(GroupFolder(group), "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                List<ClipboardTile> tiles = LoadPrefab(path);
                if (tiles == null || tiles.Count == 0)
                {
                    continue;
                }

                using (Bitmap thumb = Prefabs.Thumbnail(tiles, _prefabIconSize))
                {
                    _prefabIcons.Images.Add(thumb);
                }

                _prefabs.Add(new PrefabEntry(Path.GetFileNameWithoutExtension(path), group, path, tiles.Count,
                    _prefabIcons.Images.Count - 1, tiles.Select(t => (int)t.Id).ToHashSet()));
            }
        }

        private static List<ClipboardTile> LoadPrefab(string path)
        {
            try
            {
                return File.Exists(path) ? ClipboardTile.Parse(File.ReadAllText(path)) : null;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return null;
            }
        }

        private void RebuildPrefabGroupBar()
        {
            _prefabGroupBar.Items.Clear();

            void Add(string text, string group, string tip)
            {
                int count = group == null ? _prefabs.Count : _prefabs.Count(p => p.Group == group);
                var button = new ToolStripButton($"{text} ({count})")
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    Checked = _prefabGroup == group,
                    ToolTipText = tip,
                    AutoToolTip = false,
                    Margin = new Padding(1, 1, 0, 2)
                };
                button.Click += (_, _) =>
                {
                    _prefabGroup = group;
                    RebuildPrefabGroupBar();
                    RebuildPrefabList();
                };
                if (!string.IsNullOrEmpty(group))
                {
                    // A named group can be renamed or removed from its own button.
                    button.MouseUp += (_, e) =>
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            ShowGroupMenu(group, _prefabGroupBar.PointToClient(Cursor.Position));
                        }
                    };
                }

                _prefabGroupBar.Items.Add(button);
            }

            Add("All", null, "Every blueprint");
            Add("Ungrouped", string.Empty, "Blueprints in no group");
            foreach (string group in _prefabGroups)
            {
                Add(group, group, "Right click to rename or remove this group");
            }

            var create = new ToolStripButton("+ Group") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "Make a new group", AutoToolTip = false };
            create.Click += (_, _) => CreatePrefabGroup(null);
            _prefabGroupBar.Items.Add(new ToolStripSeparator());
            _prefabGroupBar.Items.Add(create);
            var refresh = new ToolStripButton("Refresh") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "Read the blueprint folder again", AutoToolTip = false };
            refresh.Click += (_, _) => LoadPrefabs();
            _prefabGroupBar.Items.Add(refresh);
        }

        /// <summary>Shows the blueprints of the chosen group whose name matches the search, or that hold a searched id.</summary>
        private void RebuildPrefabList()
        {
            TileBrowserQuery query = _query;
            string selected = (_prefabList.SelectedItems.Count > 0 ? _prefabList.SelectedItems[0].Tag as PrefabEntry : null)?.Path;
            _prefabList.BeginUpdate();
            try
            {
                _prefabList.Items.Clear();
                foreach (PrefabEntry prefab in _prefabs)
                {
                    if (_prefabGroup != null && prefab.Group != _prefabGroup)
                    {
                        continue;
                    }

                    if (query != null && !(query.IsIdQuery ? prefab.Ids.Any(query.MatchesId) : query.MatchesText(prefab.Group + " " + prefab.Name)))
                    {
                        continue;
                    }

                    string where = prefab.Group.Length > 0 ? $" (group {prefab.Group})" : string.Empty;
                    var item = new ListViewItem(prefab.Name, prefab.ImageIndex)
                    {
                        Tag = prefab,
                        ToolTipText = $"{prefab.Name}: {prefab.TileCount} tiles{where}"
                    };
                    _prefabList.Items.Add(item);
                    item.Selected = prefab.Path == selected;
                }
            }
            finally
            {
                _prefabList.EndUpdate();
            }

            _prefabsVersion = _filterVersion;
            UpdateStatus();
        }

        private PrefabEntry SelectedPrefab => _prefabList.SelectedItems.Count > 0 ? _prefabList.SelectedItems[0].Tag as PrefabEntry : null;

        private void PlaceSelectedPrefab()
        {
            if (SelectedPrefab is not { } prefab)
            {
                return;
            }

            // Read again so the shell gets its own list and sees the file as it is now.
            List<ClipboardTile> tiles = LoadPrefab(prefab.Path);
            if (tiles != null && tiles.Count > 0)
            {
                PrefabChosen?.Invoke(tiles);
            }
        }

        private void ShowPrefabMenu(Point location)
        {
            if (SelectedPrefab is not { } prefab)
            {
                return;
            }

            _prefabMenu.Items.Clear();
            _prefabMenu.Items.Add("Place", null, (_, _) => PlaceSelectedPrefab());

            var move = new ToolStripMenuItem("Move to group");
            void Target(string text, string group)
            {
                var item = new ToolStripMenuItem(text) { Checked = prefab.Group == group, Enabled = prefab.Group != group };
                item.Click += (_, _) => MovePrefab(prefab, group);
                move.DropDownItems.Add(item);
            }

            Target("Ungrouped", string.Empty);
            foreach (string group in _prefabGroups)
            {
                Target(group, group);
            }

            move.DropDownItems.Add(new ToolStripSeparator());
            move.DropDownItems.Add("New group...", null, (_, _) => CreatePrefabGroup(prefab));
            _prefabMenu.Items.Add(move);
            _prefabMenu.Items.Add("Rename...", null, (_, _) => RenameSelectedPrefab());
            _prefabMenu.Items.Add("Delete", null, (_, _) => DeleteSelectedPrefab());
            _prefabMenu.Show(_prefabList, location);
        }

        /// <summary>Runs a file operation on the blueprint folder, shows why when it fails, and re-reads the folder.</summary>
        private void PrefabFileAction(string what, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, $"Could not {what}: {ex.Message}", _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            LoadPrefabs();
        }

        private void MovePrefab(PrefabEntry prefab, string group)
        {
            PrefabFileAction("move the blueprint", () =>
            {
                string folder = GroupFolder(group);
                Directory.CreateDirectory(folder);
                File.Move(prefab.Path, Path.Combine(folder, Path.GetFileName(prefab.Path)));
            });
        }

        /// <summary>Asks for a group name and makes it; with a blueprint, moves that one into it.</summary>
        private void CreatePrefabGroup(PrefabEntry moveIn)
        {
            if (Prompt.Text(this, "New blueprint group", "Group name:") is not { } typed || Prefabs.SafeName(typed).Length == 0)
            {
                return;
            }

            string group = Prefabs.SafeName(typed);
            PrefabFileAction("make the group", () =>
            {
                Directory.CreateDirectory(GroupFolder(group));
                if (moveIn != null)
                {
                    File.Move(moveIn.Path, Path.Combine(GroupFolder(group), Path.GetFileName(moveIn.Path)));
                }
            });

            _prefabGroup = _prefabGroups.Contains(group) ? group : _prefabGroup;
            RebuildPrefabGroupBar();
            RebuildPrefabList();
        }

        private void ShowGroupMenu(string group, Point location)
        {
            _prefabMenu.Items.Clear();
            _prefabMenu.Items.Add("Rename group...", null, (_, _) =>
            {
                if (Prompt.Text(this, "Rename blueprint group", "Group name:", group) is { } typed &&
                    Prefabs.SafeName(typed) is { Length: > 0 } name && name != group)
                {
                    if (_prefabGroup == group)
                    {
                        _prefabGroup = name;
                    }

                    PrefabFileAction("rename the group", () => Directory.Move(GroupFolder(group), GroupFolder(name)));
                }
            });
            _prefabMenu.Items.Add("Remove group (its blueprints become ungrouped)", null, (_, _) =>
            {
                PrefabFileAction("remove the group", () =>
                {
                    foreach (string path in Directory.EnumerateFiles(GroupFolder(group), "*.txt").ToList())
                    {
                        File.Move(path, Path.Combine(Prefabs.Folder, Path.GetFileName(path)));
                    }

                    // Only an empty folder goes: anything else in it is not ours to delete.
                    if (!Directory.EnumerateFileSystemEntries(GroupFolder(group)).Any())
                    {
                        Directory.Delete(GroupFolder(group));
                    }
                });
            });
            _prefabMenu.Show(_prefabGroupBar, location);
        }

        private void RenameSelectedPrefab()
        {
            if (SelectedPrefab is not { } prefab ||
                Prompt.Text(this, "Rename blueprint", "Blueprint name:", prefab.Name) is not { } typed ||
                Prefabs.SafeName(typed) is not { Length: > 0 } name || name == prefab.Name)
            {
                return;
            }

            PrefabFileAction("rename the blueprint", () => File.Move(prefab.Path, Path.Combine(Path.GetDirectoryName(prefab.Path), name + ".txt")));
        }

        private void DeleteSelectedPrefab()
        {
            if (SelectedPrefab is not { } prefab ||
                MessageBox.Show(this, $"Delete the blueprint \"{prefab.Name}\"?", _messageTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            PrefabFileAction("delete the blueprint", () => File.Delete(prefab.Path));
        }
    }
}
