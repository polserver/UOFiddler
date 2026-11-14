namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class TileBrowserPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            _filterBar = new System.Windows.Forms.TableLayoutPanel();
            _searchBox = new System.Windows.Forms.TextBox();
            _typeFilter = new System.Windows.Forms.ComboBox();
            _status = new System.Windows.Forms.Label();
            _tabs = new System.Windows.Forms.TabControl();
            _tilesPage = new System.Windows.Forms.TabPage();
            _tilesSplit = new System.Windows.Forms.SplitContainer();
            _categoryTree = new System.Windows.Forms.TreeView();
            _favoritesPage = new System.Windows.Forms.TabPage();
            _favoritesBar = new System.Windows.Forms.ToolStrip();
            _favoritePageBox = new System.Windows.Forms.ToolStripComboBox();
            _addFavoriteButton = new System.Windows.Forms.ToolStripButton();
            _removeFavoriteButton = new System.Windows.Forms.ToolStripButton();
            _favoritePageMenu = new System.Windows.Forms.ToolStripDropDownButton();
            _newFavoritePageItem = new System.Windows.Forms.ToolStripMenuItem();
            _renamePageItem = new System.Windows.Forms.ToolStripMenuItem();
            _deletePageItem = new System.Windows.Forms.ToolStripMenuItem();
            _favoritePageMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
            _importUoheItem = new System.Windows.Forms.ToolStripMenuItem();
            _recentPage = new System.Windows.Forms.TabPage();
            _recentBar = new System.Windows.Forms.ToolStrip();
            _clearRecentButton = new System.Windows.Forms.ToolStripButton();
            _housePage = new System.Windows.Forms.TabPage();
            _houseGallery = new TileBrowserHouseGallery();
            _piecesSplitter = new GripSplitter();
            _swapButton = new System.Windows.Forms.Button();
            _houseHint = new System.Windows.Forms.Label();
            _houseKindBar = new System.Windows.Forms.ToolStrip();
            _foundationButton = new System.Windows.Forms.ToolStripButton();
            _patternsButton = new System.Windows.Forms.ToolStripButton();
            _prefabsPage = new System.Windows.Forms.TabPage();
            _prefabList = new System.Windows.Forms.ListView();
            _prefabIcons = new System.Windows.Forms.ImageList(components);
            _prefabGroupBar = new System.Windows.Forms.ToolStrip();
            _designsPage = new System.Windows.Forms.TabPage();
            _designLibrary = new DesignLibraryPanel();
            _toolTip = new System.Windows.Forms.ToolTip(components);
            _searchTimer = new System.Windows.Forms.Timer(components);
            _tileMenu = new System.Windows.Forms.ContextMenuStrip(components);
            _prefabMenu = new System.Windows.Forms.ContextMenuStrip(components);
            _filterBar.SuspendLayout();
            _tabs.SuspendLayout();
            _tilesPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_tilesSplit).BeginInit();
            _tilesSplit.Panel1.SuspendLayout();
            _tilesSplit.SuspendLayout();
            _favoritesPage.SuspendLayout();
            _favoritesBar.SuspendLayout();
            _recentPage.SuspendLayout();
            _recentBar.SuspendLayout();
            _housePage.SuspendLayout();
            _houseKindBar.SuspendLayout();
            _prefabsPage.SuspendLayout();
            _designsPage.SuspendLayout();
            SuspendLayout();
            // 
            // _filterBar
            // 
            _filterBar.AutoSize = true;
            _filterBar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            _filterBar.ColumnCount = 1;
            _filterBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            _filterBar.Controls.Add(_searchBox, 0, 0);
            _filterBar.Controls.Add(_typeFilter, 0, 1);
            _filterBar.Dock = System.Windows.Forms.DockStyle.Top;
            _filterBar.Location = new System.Drawing.Point(0, 0);
            _filterBar.Name = "_filterBar";
            _filterBar.Padding = new System.Windows.Forms.Padding(1);
            _filterBar.RowCount = 2;
            _filterBar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _filterBar.RowStyles.Add(new System.Windows.Forms.RowStyle());
            _filterBar.Size = new System.Drawing.Size(674, 52);
            _filterBar.TabIndex = 2;
            // 
            // _searchBox
            // 
            _searchBox.Dock = System.Windows.Forms.DockStyle.Fill;
            _searchBox.Location = new System.Drawing.Point(2, 2);
            _searchBox.Margin = new System.Windows.Forms.Padding(1);
            _searchBox.Name = "_searchBox";
            _searchBox.PlaceholderText = "Search name or id (0x1F4, 500, 0x100-0x1FF)";
            _searchBox.Size = new System.Drawing.Size(670, 23);
            _searchBox.TabIndex = 0;
            _toolTip.SetToolTip(_searchBox, "Type words of a tile name (all must match), or ids: decimal, 0x or $ hex, ranges like 0x100-0x1FF.\nEnter picks the first result, Down moves to the results, Esc clears.");
            _searchBox.TextChanged += SearchBox_TextChanged;
            _searchBox.KeyDown += SearchBox_KeyDown;
            // 
            // _typeFilter
            // 
            _typeFilter.Dock = System.Windows.Forms.DockStyle.Fill;
            _typeFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _typeFilter.Location = new System.Drawing.Point(2, 27);
            _typeFilter.Margin = new System.Windows.Forms.Padding(1);
            _typeFilter.Name = "_typeFilter";
            _typeFilter.Size = new System.Drawing.Size(670, 23);
            _typeFilter.TabIndex = 1;
            _typeFilter.SelectedIndexChanged += TypeFilter_SelectedIndexChanged;
            // 
            // _status
            // 
            _status.AutoEllipsis = true;
            _status.Dock = System.Windows.Forms.DockStyle.Bottom;
            _status.Location = new System.Drawing.Point(0, 425);
            _status.Name = "_status";
            _status.Padding = new System.Windows.Forms.Padding(2, 0, 2, 0);
            _status.Size = new System.Drawing.Size(674, 20);
            _status.TabIndex = 1;
            _status.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _tabs
            // 
            _tabs.Controls.Add(_tilesPage);
            _tabs.Controls.Add(_favoritesPage);
            _tabs.Controls.Add(_recentPage);
            _tabs.Controls.Add(_housePage);
            _tabs.Controls.Add(_prefabsPage);
            _tabs.Controls.Add(_designsPage);
            _tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            _tabs.Location = new System.Drawing.Point(0, 52);
            _tabs.Multiline = true;
            _tabs.Name = "_tabs";
            _tabs.SelectedIndex = 0;
            _tabs.Size = new System.Drawing.Size(674, 373);
            _tabs.TabIndex = 0;
            _tabs.SelectedIndexChanged += Tabs_SelectedIndexChanged;
            // 
            // _tilesPage
            // 
            _tilesPage.Controls.Add(_tilesSplit);
            _tilesPage.Location = new System.Drawing.Point(4, 24);
            _tilesPage.Name = "_tilesPage";
            _tilesPage.Size = new System.Drawing.Size(666, 345);
            _tilesPage.TabIndex = 0;
            _tilesPage.Text = "Tiles";
            // 
            // _tilesSplit
            // 
            _tilesSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            _tilesSplit.Location = new System.Drawing.Point(0, 0);
            _tilesSplit.Name = "_tilesSplit";
            _tilesSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _tilesSplit.Panel1
            // 
            _tilesSplit.Panel1.Controls.Add(_categoryTree);
            _tilesSplit.Panel1MinSize = 50;
            _tilesSplit.Panel2MinSize = 80;
            _tilesSplit.Size = new System.Drawing.Size(666, 345);
            _tilesSplit.SplitterDistance = 172;
            _tilesSplit.TabIndex = 0;
            _tilesSplit.SizeChanged += TilesSplit_SizeChanged;
            // 
            // _categoryTree
            // 
            _categoryTree.Dock = System.Windows.Forms.DockStyle.Fill;
            _categoryTree.HideSelection = false;
            _categoryTree.Location = new System.Drawing.Point(0, 0);
            _categoryTree.Name = "_categoryTree";
            _categoryTree.ShowNodeToolTips = true;
            _categoryTree.Size = new System.Drawing.Size(666, 172);
            _categoryTree.TabIndex = 0;
            _categoryTree.AfterCollapse += CategoryTree_AfterCollapse;
            _categoryTree.AfterExpand += CategoryTree_AfterExpand;
            _categoryTree.AfterSelect += CategoryTree_AfterSelect;
            // 
            // _favoritesPage
            // 
            _favoritesPage.Controls.Add(_favoritesBar);
            _favoritesPage.Location = new System.Drawing.Point(4, 44);
            _favoritesPage.Name = "_favoritesPage";
            _favoritesPage.Size = new System.Drawing.Size(192, 52);
            _favoritesPage.TabIndex = 1;
            _favoritesPage.Text = "Favorites";
            // 
            // _favoritesBar
            // 
            _favoritesBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _favoritesBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _favoritePageBox, _addFavoriteButton, _removeFavoriteButton, _favoritePageMenu });
            _favoritesBar.Location = new System.Drawing.Point(0, 0);
            _favoritesBar.Name = "_favoritesBar";
            _favoritesBar.Size = new System.Drawing.Size(192, 25);
            _favoritesBar.TabIndex = 1;
            // 
            // _favoritePageBox
            // 
            _favoritePageBox.AutoSize = false;
            _favoritePageBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _favoritePageBox.Name = "_favoritePageBox";
            _favoritePageBox.Size = new System.Drawing.Size(130, 23);
            _favoritePageBox.ToolTipText = "Favorites page";
            _favoritePageBox.SelectedIndexChanged += FavoritePageBox_SelectedIndexChanged;
            // 
            // _addFavoriteButton
            // 
            _addFavoriteButton.Name = "_addFavoriteButton";
            _addFavoriteButton.Size = new System.Drawing.Size(33, 22);
            _addFavoriteButton.Text = "Add";
            _addFavoriteButton.ToolTipText = "Add the current draw tile to this page";
            _addFavoriteButton.Click += AddFavoriteButton_Click;
            // 
            // _removeFavoriteButton
            // 
            _removeFavoriteButton.Name = "_removeFavoriteButton";
            _removeFavoriteButton.Size = new System.Drawing.Size(54, 19);
            _removeFavoriteButton.Text = "Remove";
            _removeFavoriteButton.ToolTipText = "Remove the current draw tile from this page (Delete removes the tile under the cursor)";
            _removeFavoriteButton.Click += RemoveFavoriteButton_Click;
            // 
            // _favoritePageMenu
            // 
            _favoritePageMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { _newFavoritePageItem, _renamePageItem, _deletePageItem, _favoritePageMenuSeparator, _importUoheItem });
            _favoritePageMenu.Name = "_favoritePageMenu";
            _favoritePageMenu.Size = new System.Drawing.Size(46, 19);
            _favoritePageMenu.Text = "Page";
            _favoritePageMenu.DropDownOpening += FavoritePageMenu_DropDownOpening;
            // 
            // _newFavoritePageItem
            // 
            _newFavoritePageItem.Name = "_newFavoritePageItem";
            _newFavoritePageItem.Size = new System.Drawing.Size(216, 22);
            _newFavoritePageItem.Text = "New page...";
            _newFavoritePageItem.Click += NewFavoritePageItem_Click;
            // 
            // _renamePageItem
            // 
            _renamePageItem.Name = "_renamePageItem";
            _renamePageItem.Size = new System.Drawing.Size(216, 22);
            _renamePageItem.Text = "Rename page...";
            _renamePageItem.Click += RenamePageItem_Click;
            // 
            // _deletePageItem
            // 
            _deletePageItem.Name = "_deletePageItem";
            _deletePageItem.Size = new System.Drawing.Size(216, 22);
            _deletePageItem.Text = "Delete page";
            _deletePageItem.Click += DeletePageItem_Click;
            // 
            // _favoritePageMenuSeparator
            // 
            _favoritePageMenuSeparator.Name = "_favoritePageMenuSeparator";
            _favoritePageMenuSeparator.Size = new System.Drawing.Size(213, 6);
            // 
            // _importUoheItem
            // 
            _importUoheItem.Name = "_importUoheItem";
            _importUoheItem.Size = new System.Drawing.Size(216, 22);
            _importUoheItem.Text = "Import UOHE bookmarks...";
            _importUoheItem.Click += ImportUoheItem_Click;
            // 
            // _recentPage
            // 
            _recentPage.Controls.Add(_recentBar);
            _recentPage.Location = new System.Drawing.Point(4, 44);
            _recentPage.Name = "_recentPage";
            _recentPage.Size = new System.Drawing.Size(192, 52);
            _recentPage.TabIndex = 2;
            _recentPage.Text = "Recent";
            // 
            // _recentBar
            // 
            _recentBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _recentBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _clearRecentButton });
            _recentBar.Location = new System.Drawing.Point(0, 0);
            _recentBar.Name = "_recentBar";
            _recentBar.Size = new System.Drawing.Size(192, 25);
            _recentBar.TabIndex = 1;
            // 
            // _clearRecentButton
            // 
            _clearRecentButton.Name = "_clearRecentButton";
            _clearRecentButton.Size = new System.Drawing.Size(38, 22);
            _clearRecentButton.Text = "Clear";
            _clearRecentButton.ToolTipText = "Forget the recently used tiles";
            _clearRecentButton.Click += ClearRecentButton_Click;
            // 
            // _housePage
            // 
            _housePage.Controls.Add(_houseGallery);
            _housePage.Controls.Add(_piecesSplitter);
            _housePage.Controls.Add(_swapButton);
            _housePage.Controls.Add(_houseHint);
            _housePage.Controls.Add(_houseKindBar);
            _housePage.Location = new System.Drawing.Point(4, 44);
            _housePage.Name = "_housePage";
            _housePage.Size = new System.Drawing.Size(192, 52);
            _housePage.TabIndex = 3;
            _housePage.Text = "House";
            // 
            // _houseGallery
            // 
            _houseGallery.BackColor = System.Drawing.SystemColors.Window;
            _houseGallery.Dock = System.Windows.Forms.DockStyle.Fill;
            _houseGallery.Location = new System.Drawing.Point(0, 22);
            _houseGallery.Name = "_houseGallery";
            _houseGallery.Size = new System.Drawing.Size(192, 0);
            _houseGallery.TabIndex = 0;
            _houseGallery.StyleChosen += PickHouseStyle;
            _houseGallery.TagChosen += HouseGallery_TagChosen;
            // 
            // _piecesSplitter
            // 
            _piecesSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            _piecesSplitter.Location = new System.Drawing.Point(0, -15);
            _piecesSplitter.MinExtra = 120;
            _piecesSplitter.MinSize = 80;
            _piecesSplitter.Name = "_piecesSplitter";
            _piecesSplitter.Size = new System.Drawing.Size(192, 7);
            _piecesSplitter.TabIndex = 1;
            _piecesSplitter.TabStop = false;
            _piecesSplitter.SplitterMoved += PiecesSplitter_SplitterMoved;
            // 
            // _swapButton
            // 
            _swapButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            _swapButton.Location = new System.Drawing.Point(0, -8);
            _swapButton.Name = "_swapButton";
            _swapButton.Size = new System.Drawing.Size(192, 26);
            _swapButton.TabIndex = 3;
            _swapButton.Text = "Swap selection to this style";
            _toolTip.SetToolTip(_swapButton, "Changes the selected pieces of this kind to the matching pieces of the chosen style (Ctrl+R)");
            _swapButton.Click += SwapButton_Click;
            // 
            // _houseHint
            // 
            _houseHint.Dock = System.Windows.Forms.DockStyle.Bottom;
            _houseHint.ForeColor = System.Drawing.SystemColors.GrayText;
            _houseHint.Location = new System.Drawing.Point(0, 18);
            _houseHint.Name = "_houseHint";
            _houseHint.Padding = new System.Windows.Forms.Padding(2, 2, 2, 0);
            _houseHint.Size = new System.Drawing.Size(192, 34);
            _houseHint.TabIndex = 4;
            // 
            // _houseKindBar
            // 
            _houseKindBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _houseKindBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _foundationButton, _patternsButton });
            _houseKindBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            _houseKindBar.Location = new System.Drawing.Point(0, 0);
            _houseKindBar.Name = "_houseKindBar";
            _houseKindBar.Size = new System.Drawing.Size(192, 22);
            _houseKindBar.TabIndex = 5;
            // 
            // _foundationButton
            // 
            _foundationButton.Name = "_foundationButton";
            _foundationButton.Size = new System.Drawing.Size(72, 19);
            _foundationButton.Text = "Foundation";
            _foundationButton.ToolTipText = "Drag the house area: a low wall around a dirt floor 7 above it.";
            _foundationButton.Click += FoundationButton_Click;
            // 
            // _patternsButton
            // 
            _patternsButton.Name = "_patternsButton";
            _patternsButton.Size = new System.Drawing.Size(54, 19);
            _patternsButton.Text = "Patterns";
            _patternsButton.ToolTipText = "Bordered floors: corner, side and fill pieces laid as one. Save your own from a selection (File menu).";
            _patternsButton.Click += PatternsButton_Click;
            // 
            // _prefabsPage
            // 
            _prefabsPage.Controls.Add(_prefabList);
            _prefabsPage.Controls.Add(_prefabGroupBar);
            _prefabsPage.Location = new System.Drawing.Point(4, 44);
            _prefabsPage.Name = "_prefabsPage";
            _prefabsPage.Size = new System.Drawing.Size(192, 52);
            _prefabsPage.TabIndex = 4;
            _prefabsPage.Text = "Blueprints";
            // 
            // _prefabList
            // 
            _prefabList.Dock = System.Windows.Forms.DockStyle.Fill;
            _prefabList.LargeImageList = _prefabIcons;
            _prefabList.Location = new System.Drawing.Point(0, 0);
            _prefabList.MultiSelect = false;
            _prefabList.Name = "_prefabList";
            _prefabList.ShowItemToolTips = true;
            _prefabList.Size = new System.Drawing.Size(192, 52);
            _prefabList.TabIndex = 0;
            _prefabList.UseCompatibleStateImageBehavior = false;
            _prefabList.KeyDown += PrefabList_KeyDown;
            _prefabList.MouseClick += PrefabList_MouseClick;
            // 
            // _prefabIcons
            // 
            _prefabIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            _prefabIcons.ImageSize = new System.Drawing.Size(80, 80);
            _prefabIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // _prefabGroupBar
            // 
            _prefabGroupBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _prefabGroupBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            _prefabGroupBar.Location = new System.Drawing.Point(0, 0);
            _prefabGroupBar.Name = "_prefabGroupBar";
            _prefabGroupBar.Size = new System.Drawing.Size(192, 0);
            _prefabGroupBar.TabIndex = 1;
            // 
            // _designsPage
            // 
            _designsPage.Controls.Add(_designLibrary);
            _designsPage.Location = new System.Drawing.Point(4, 44);
            _designsPage.Name = "_designsPage";
            _designsPage.Size = new System.Drawing.Size(192, 52);
            _designsPage.TabIndex = 5;
            _designsPage.Text = "Designs";
            // 
            // _designLibrary
            // 
            _designLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            _designLibrary.Location = new System.Drawing.Point(0, 0);
            _designLibrary.Name = "_designLibrary";
            _designLibrary.Size = new System.Drawing.Size(192, 52);
            _designLibrary.TabIndex = 0;
            _designLibrary.OpenRequested += DesignLibrary_OpenRequested;
            _designLibrary.PlaceRequested += DesignLibrary_PlaceRequested;
            // 
            // _searchTimer
            // 
            _searchTimer.Interval = 150;
            _searchTimer.Tick += SearchTimer_Tick;
            // 
            // _tileMenu
            // 
            _tileMenu.Name = "_tileMenu";
            _tileMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // _prefabMenu
            // 
            _prefabMenu.Name = "_prefabMenu";
            _prefabMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // TileBrowserPanel
            // 
            Controls.Add(_tabs);
            Controls.Add(_status);
            Controls.Add(_filterBar);
            Name = "TileBrowserPanel";
            Size = new System.Drawing.Size(674, 445);
            _filterBar.ResumeLayout(false);
            _filterBar.PerformLayout();
            _tabs.ResumeLayout(false);
            _tilesPage.ResumeLayout(false);
            _tilesSplit.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_tilesSplit).EndInit();
            _tilesSplit.ResumeLayout(false);
            _favoritesPage.ResumeLayout(false);
            _favoritesPage.PerformLayout();
            _favoritesBar.ResumeLayout(false);
            _favoritesBar.PerformLayout();
            _recentPage.ResumeLayout(false);
            _recentPage.PerformLayout();
            _recentBar.ResumeLayout(false);
            _recentBar.PerformLayout();
            _housePage.ResumeLayout(false);
            _housePage.PerformLayout();
            _houseKindBar.ResumeLayout(false);
            _houseKindBar.PerformLayout();
            _prefabsPage.ResumeLayout(false);
            _prefabsPage.PerformLayout();
            _designsPage.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel _filterBar;
        private System.Windows.Forms.TextBox _searchBox;
        private System.Windows.Forms.ComboBox _typeFilter;
        private System.Windows.Forms.Label _status;
        private System.Windows.Forms.TabControl _tabs;
        private System.Windows.Forms.TabPage _tilesPage;
        private System.Windows.Forms.SplitContainer _tilesSplit;
        private System.Windows.Forms.TreeView _categoryTree;
        private System.Windows.Forms.TabPage _favoritesPage;
        private System.Windows.Forms.ToolStrip _favoritesBar;
        private System.Windows.Forms.ToolStripComboBox _favoritePageBox;
        private System.Windows.Forms.ToolStripButton _addFavoriteButton;
        private System.Windows.Forms.ToolStripButton _removeFavoriteButton;
        private System.Windows.Forms.ToolStripDropDownButton _favoritePageMenu;
        private System.Windows.Forms.ToolStripMenuItem _newFavoritePageItem;
        private System.Windows.Forms.ToolStripMenuItem _renamePageItem;
        private System.Windows.Forms.ToolStripMenuItem _deletePageItem;
        private System.Windows.Forms.ToolStripSeparator _favoritePageMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem _importUoheItem;
        private System.Windows.Forms.TabPage _recentPage;
        private System.Windows.Forms.ToolStrip _recentBar;
        private System.Windows.Forms.ToolStripButton _clearRecentButton;
        private System.Windows.Forms.TabPage _housePage;
        private TileBrowserHouseGallery _houseGallery;
        private GripSplitter _piecesSplitter;
        private System.Windows.Forms.Button _swapButton;
        private System.Windows.Forms.Label _houseHint;
        private System.Windows.Forms.ToolStrip _houseKindBar;
        private System.Windows.Forms.ToolStripButton _foundationButton;
        private System.Windows.Forms.ToolStripButton _patternsButton;
        private System.Windows.Forms.TabPage _prefabsPage;
        private System.Windows.Forms.ListView _prefabList;
        private System.Windows.Forms.ImageList _prefabIcons;
        private System.Windows.Forms.ToolStrip _prefabGroupBar;
        private System.Windows.Forms.TabPage _designsPage;
        private DesignLibraryPanel _designLibrary;
        private System.Windows.Forms.ToolTip _toolTip;
        private System.Windows.Forms.Timer _searchTimer;
        private System.Windows.Forms.ContextMenuStrip _tileMenu;
        private System.Windows.Forms.ContextMenuStrip _prefabMenu;
    }
}
