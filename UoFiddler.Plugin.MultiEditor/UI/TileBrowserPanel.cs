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
using System.Linq;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Storage;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The editor's tile picker: one search box over pages of tiles (the multieditor.xml tree),
    /// favorites, recently placed tiles, house part styles and blueprints. Data files are read on
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
        private int _blueprintsVersion = -1;

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

        /// <summary>The user activated a blueprint: the shell starts a paste with these tiles.</summary>
        public event Action<List<ClipboardTile>> BlueprintChosen;

        /// <summary>
        /// The House page's swap button: swap the selected pieces to the same columns of this style
        /// (the GDI editor's "Swap selection to this style").
        /// </summary>
        public event Action<HouseStyle> SwapToHouseStyleRequested;

        /// <summary>Asks for the style editor, on the style chosen on the House page (or none).</summary>
        public event Action<HouseStyle> EditStylesRequested;

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

        /// <summary>Re-reads the blueprint folder.</summary>
        public void RefreshBlueprints()
        {
            if (_blueprintsLoaded)
            {
                LoadBlueprints();
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
            if (_blueprintsLoaded)
            {
                LoadBlueprints();
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
            _housePieces.Height = SavedPiecesHeight;
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

        /// <summary>Designs were saved as blueprints: the Blueprints page shows them next time, the Designs page stays up.</summary>
        private void DesignLibrary_BlueprintsSaved(string showGroup)
        {
            ReloadBlueprints(showGroup, bringToFront: false);
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
                    else if (_tabs.SelectedTab == _blueprintsPage)
                    {
                        _blueprintList.Focus();
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
            else if (_tabs.SelectedTab == _blueprintsPage && _blueprintList.Items.Count > 0)
            {
                _blueprintList.Items[0].Selected = true;
                PlaceSelectedBlueprint();
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
            else if (page == _blueprintsPage)
            {
                EnsureBlueprints();
                if (_blueprintsVersion != _filterVersion)
                {
                    RebuildBlueprintList();
                }

                StartThumbnails();
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
                list = _blueprintList.Items.Count == 0 && _query == null ? "No blueprints saved yet" : $"{_blueprintList.Items.Count} blueprints";
                if (SelectedBlueprint is { } blueprint)
                {
                    list += $"  |  {blueprint.Name}: {Blueprints.FootprintText(blueprint.Footprint, true)}, {blueprint.TileCount:N0} tiles";
                }
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
    }
}
