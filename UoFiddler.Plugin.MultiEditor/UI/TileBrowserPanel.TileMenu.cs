/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Storage;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    internal sealed partial class TileBrowserPanel
    {
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
    }
}
