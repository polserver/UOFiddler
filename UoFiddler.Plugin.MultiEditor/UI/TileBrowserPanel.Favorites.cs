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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Storage;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    internal sealed partial class TileBrowserPanel
    {
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
    }
}
