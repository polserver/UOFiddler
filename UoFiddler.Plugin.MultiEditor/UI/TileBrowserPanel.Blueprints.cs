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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Storage;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    internal sealed partial class TileBrowserPanel
    {
        // ---------------------------------------------------------------- blueprints page

        private const int _blueprintIconSize = 80;

        private readonly List<BlueprintEntry> _blueprints = new List<BlueprintEntry>();
        private readonly List<string> _blueprintGroups = new List<string>();
        private bool _blueprintsLoaded;

        /// <summary>The group shown: null for all, empty for the blueprints in no group, else a group name.</summary>
        private string _blueprintGroup;

        /// <summary>Blueprints whose thumbnail is not drawn yet; they show the blank image 0 meanwhile.</summary>
        private readonly HashSet<BlueprintEntry> _thumbsPending = new HashSet<BlueprintEntry>();
        private Timer _thumbTimer;

        /// <summary>
        /// A blueprint file. Group is the subfolder it is in, empty for the blueprint folder itself;
        /// ImageIndex is 0 (blank) until its thumbnail is drawn.
        /// </summary>
        private sealed class BlueprintEntry
        {
            public BlueprintEntry(string name, string group, string path, int tileCount, (int Width, int Depth, int Height) footprint,
                HashSet<int> ids)
            {
                Name = name;
                Group = group;
                Path = path;
                TileCount = tileCount;
                Footprint = footprint;
                Ids = ids;
            }

            public string Name { get; }
            public string Group { get; }
            public string Path { get; }
            public int TileCount { get; }

            /// <summary>Cells along X and Y and how high it is in Z.</summary>
            public (int Width, int Depth, int Height) Footprint { get; }

            public HashSet<int> Ids { get; }
            public int ImageIndex { get; set; }
        }

        /// <summary>
        /// One click places a blueprint, like picking a tile; a right click opens its menu
        /// without starting a paste.
        /// </summary>
        private void BlueprintList_MouseClick(object sender, MouseEventArgs e)
        {
            if (_blueprintList.GetItemAt(e.X, e.Y) is not { } item)
            {
                return;
            }

            item.Selected = true;
            if (e.Button == MouseButtons.Left)
            {
                PlaceSelectedBlueprint();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ShowBlueprintMenu(e.Location);
            }
        }

        private void BlueprintList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedBlueprint();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                PlaceSelectedBlueprint();
                e.Handled = e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F2)
            {
                RenameSelectedBlueprint();
                e.Handled = true;
            }
        }

        private void EnsureBlueprints()
        {
            if (!_blueprintsLoaded)
            {
                LoadBlueprints();
            }
        }

        /// <summary>The group a new blueprint goes to: the one shown, empty (no group) when all are shown.</summary>
        public string ShownBlueprintGroup => _blueprintGroup ?? string.Empty;

        /// <summary>
        /// Saves tiles as a blueprint in the group being shown (no group when all are shown) and
        /// brings up the Blueprints page. A name that is taken asks whether to replace it.
        /// </summary>
        public void SaveBlueprint(string name, List<ClipboardTile> tiles)
        {
            EnsureLoaded();
            string folder = Blueprints.GroupFolder(ShownBlueprintGroup);
            string path = Path.Combine(folder, Blueprints.SafeName(name) + ".txt");
            if (File.Exists(path))
            {
                DialogResult answer = MessageBox.Show(this, $"A blueprint named \"{name}\" already exists. Replace it?\n\nNo saves this one under a new name.",
                    _messageTitle, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel)
                {
                    return;
                }

                if (answer == DialogResult.No)
                {
                    (path, name) = Blueprints.UniqueName(folder, name);
                }
            }

            try
            {
                Blueprints.Write(path, tiles, name);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                MessageBox.Show(this, $"Could not save the blueprint: {ex.Message}", _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ReloadBlueprints(_blueprintGroup);
        }

        /// <summary>
        /// Re-reads the blueprint folder and shows a group (null for all) on the Blueprints page,
        /// after blueprints were written elsewhere; the page is brought up unless told not to.
        /// </summary>
        public void ReloadBlueprints(string showGroup, bool bringToFront = true)
        {
            EnsureLoaded();
            _blueprintGroup = showGroup;

            // A page never shown reads the folder when it first shows.
            bool wasLoaded = _blueprintsLoaded;
            if (bringToFront && _tabs.SelectedTab != _blueprintsPage)
            {
                _tabs.SelectedTab = _blueprintsPage;
            }

            if (wasLoaded)
            {
                LoadBlueprints();
            }
        }

        private static string GroupFolder(string group) => Blueprints.GroupFolder(group);

        /// <summary>
        /// Reads every blueprint file; unreadable files are skipped. Thumbnails are drawn afterwards,
        /// the ones in view first. A subfolder of the blueprint folder is a group, also when it is empty.
        /// </summary>
        private void LoadBlueprints()
        {
            _blueprintsLoaded = true;
            _blueprints.Clear();
            _thumbsPending.Clear();
            _blueprintGroups.Clear();
            _blueprintList.Items.Clear();
            _blueprintIcons.Images.Clear();
            using (var blank = new Bitmap(_blueprintIconSize, _blueprintIconSize))
            {
                _blueprintIcons.Images.Add(blank);
            }

            bool complete = true;
            try
            {
                if (Directory.Exists(Blueprints.Folder))
                {
                    AddBlueprintsOf(string.Empty);
                    foreach (string folder in Directory.EnumerateDirectories(Blueprints.Folder).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        string group = Path.GetFileName(folder);
                        _blueprintGroups.Add(group);
                        AddBlueprintsOf(group);
                    }
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // What could be read is shown.
                complete = false;
            }

            if (!string.IsNullOrEmpty(_blueprintGroup) && !_blueprintGroups.Contains(_blueprintGroup))
            {
                _blueprintGroup = null;
            }

            // Only a full read says a group is gone; a failed one may just have stopped early.
            if (complete)
            {
                DropMissingFavoriteGroups();
            }

            RebuildBlueprintPage();
        }

        private void AddBlueprintsOf(string group)
        {
            foreach (string path in Directory.EnumerateFiles(GroupFolder(group), "*.txt").OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                (List<ClipboardTile> tiles, string name) = Blueprints.Read(path);
                if (tiles == null || tiles.Count == 0)
                {
                    continue;
                }

                var entry = new BlueprintEntry(name, group, path, tiles.Count,
                    Blueprints.Footprint(tiles), tiles.Select(t => (int)t.Id).ToHashSet());
                _blueprints.Add(entry);
                _thumbsPending.Add(entry);
            }
        }

        private static List<ClipboardTile> LoadBlueprint(string path) => Blueprints.Read(path).Tiles;

        /// <summary>Starts drawing the thumbnails still missing, while the Blueprints page shows.</summary>
        private void StartThumbnails()
        {
            if (_thumbsPending.Count == 0)
            {
                return;
            }

            if (_thumbTimer == null)
            {
                _thumbTimer = new Timer(components) { Interval = 15 };
                _thumbTimer.Tick += ThumbTimer_Tick;
            }

            _thumbTimer.Start();
        }

        /// <summary>
        /// Draws missing thumbnails for a few milliseconds: the blueprints in view first, then the
        /// rest of the list. Stops when the page is hidden or every listed one is drawn.
        /// </summary>
        private void ThumbTimer_Tick(object sender, EventArgs e)
        {
            if (!_blueprintList.Visible || _thumbsPending.Count == 0)
            {
                _thumbTimer.Stop();
                return;
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();
            Rectangle view = _blueprintList.ClientRectangle;
            var inView = new List<ListViewItem>();
            var later = new List<ListViewItem>();
            foreach (ListViewItem item in _blueprintList.Items)
            {
                if (item.Tag is BlueprintEntry entry && _thumbsPending.Contains(entry))
                {
                    (item.Bounds.IntersectsWith(view) ? inView : later).Add(item);
                }
            }

            if (inView.Count == 0 && later.Count == 0)
            {
                // What is left is filtered out; it is drawn when it shows again.
                _thumbTimer.Stop();
                return;
            }

            // Setting an item's image repaints only that item, so no BeginUpdate (it would repaint the list).
            foreach (ListViewItem item in inView.Concat(later))
            {
                var entry = (BlueprintEntry)item.Tag;
                _thumbsPending.Remove(entry);
                if (LoadBlueprint(entry.Path) is { Count: > 0 } tiles)
                {
                    using Bitmap thumb = Blueprints.Thumbnail(tiles, _blueprintIconSize);
                    _blueprintIcons.Images.Add(thumb);
                    entry.ImageIndex = _blueprintIcons.Images.Count - 1;
                    item.ImageIndex = entry.ImageIndex;
                }

                if (watch.ElapsedMilliseconds > 30)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// The group bar: one drop-down to choose the group shown (a bulk save can make dozens of
        /// groups, too many for a button each), which also renames or removes the group shown,
        /// then New group and Refresh.
        /// </summary>
        private void RebuildBlueprintGroupBar()
        {
            _blueprintGroupBar.Items.Clear();

            int CountOf(string group) => group == null ? _blueprints.Count : _blueprints.Count(p => p.Group == group);
            static string Label(string group) => group == null ? "All" : group.Length == 0 ? "Ungrouped" : group;

            // A favorite shown is marked by its own button; the drop-down then just says what it is.
            List<string> favorites = FavoriteGroups();
            bool favoriteShown = favorites.Contains(_blueprintGroup);
            var chooser = new ToolStripDropDownButton(favoriteShown ? "Groups" : $"{Label(_blueprintGroup)} ({CountOf(_blueprintGroup)})")
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ToolTipText = "The group shown",
                AutoToolTip = false
            };

            void Add(string group)
            {
                var item = new ToolStripMenuItem($"{Label(group)} ({CountOf(group)})") { Checked = _blueprintGroup == group };
                item.Click += (_, _) =>
                {
                    _blueprintGroup = group;
                    RebuildBlueprintPage();
                };
                chooser.DropDownItems.Add(item);
            }

            Add(null);
            Add(string.Empty);
            if (_blueprintGroups.Count > 0)
            {
                chooser.DropDownItems.Add(new ToolStripSeparator());
                foreach (string group in _blueprintGroups)
                {
                    Add(group);
                }
            }

            if (!string.IsNullOrEmpty(_blueprintGroup))
            {
                string shown = _blueprintGroup;
                bool favorite = favorites.Contains(shown, StringComparer.OrdinalIgnoreCase);
                chooser.DropDownItems.Add(new ToolStripSeparator());
                chooser.DropDownItems.Add(favorite ? $"Remove \"{shown}\" from favorites" : $"Add \"{shown}\" to favorites", null,
                    (_, _) => SetFavoriteGroup(shown, !favorite));
                chooser.DropDownItems.Add($"Rename \"{shown}\"...", null, (_, _) => RenameBlueprintGroup(shown));
                chooser.DropDownItems.Add($"Remove \"{shown}\" (its blueprints become ungrouped)", null, (_, _) => RemoveBlueprintGroup(shown));
            }

            _blueprintGroupBar.Items.Add(chooser);

            // Favorite groups stay on the bar, one click away; right click takes one off.
            foreach (string group in favorites)
            {
                string favorite = group;
                var button = new ToolStripButton("★ " + favorite)
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    Checked = _blueprintGroup == favorite,
                    ToolTipText = $"{favorite}: {CountOf(favorite)} blueprints. Right click to remove it from favorites.",
                    AutoToolTip = false
                };
                button.Click += (_, _) =>
                {
                    _blueprintGroup = favorite;
                    RebuildBlueprintPage();
                };
                button.MouseUp += (_, e) =>
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        ClearMenu(_blueprintMenu.Items);
                        _blueprintMenu.Items.Add($"Remove \"{favorite}\" from favorites", null, (_, _) => SetFavoriteGroup(favorite, false));
                        _blueprintMenu.Show(_blueprintGroupBar, _blueprintGroupBar.PointToClient(Cursor.Position));
                    }
                };
                _blueprintGroupBar.Items.Add(button);
            }

            var create = new ToolStripButton("+ Group") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "Make a new group", AutoToolTip = false };
            create.Click += (_, _) => CreateBlueprintGroup(null);
            _blueprintGroupBar.Items.Add(new ToolStripSeparator());
            _blueprintGroupBar.Items.Add(create);
            var refresh = new ToolStripButton("Refresh") { DisplayStyle = ToolStripItemDisplayStyle.Text, ToolTipText = "Read the blueprint folder again", AutoToolTip = false };
            refresh.Click += (_, _) => LoadBlueprints();
            _blueprintGroupBar.Items.Add(refresh);
        }

        /// <summary>Rebuilds the group bar and the list of the blueprints page.</summary>
        private void RebuildBlueprintPage()
        {
            RebuildBlueprintGroupBar();
            RebuildBlueprintList();
        }

        /// <summary>Shows the blueprints of the chosen group whose name matches the search, or that hold a searched id.</summary>
        private void RebuildBlueprintList()
        {
            TileBrowserQuery query = _query;
            string selected = SelectedBlueprint?.Path;
            _blueprintList.BeginUpdate();
            try
            {
                _blueprintList.Items.Clear();
                foreach (BlueprintEntry blueprint in _blueprints)
                {
                    if (_blueprintGroup != null && blueprint.Group != _blueprintGroup)
                    {
                        continue;
                    }

                    if (query != null && !(query.IsIdQuery ? blueprint.Ids.Any(query.MatchesId) : query.MatchesText(blueprint.Group + " " + blueprint.Name)))
                    {
                        continue;
                    }

                    // Thumbnails are all scaled to one size, so the label says how big it really is.
                    string where = blueprint.Group.Length > 0 ? $"\ngroup {blueprint.Group}" : string.Empty;
                    var item = new ListViewItem($"{blueprint.Name} ({Blueprints.FootprintText(blueprint.Footprint, false)})", blueprint.ImageIndex)
                    {
                        Tag = blueprint,
                        ToolTipText = $"{blueprint.Name}\n{Blueprints.FootprintText(blueprint.Footprint, true)}, {blueprint.TileCount:N0} tiles{where}"
                    };
                    _blueprintList.Items.Add(item);
                    item.Selected = blueprint.Path == selected;
                }
            }
            finally
            {
                _blueprintList.EndUpdate();
            }

            StartThumbnails();
            _blueprintsVersion = _filterVersion;
            UpdateStatus();
        }

        private BlueprintEntry SelectedBlueprint => _blueprintList.SelectedItems.Count > 0 ? _blueprintList.SelectedItems[0].Tag as BlueprintEntry : null;

        private void PlaceSelectedBlueprint()
        {
            if (SelectedBlueprint is not { } blueprint)
            {
                return;
            }

            // Read again so the shell gets its own list and sees the file as it is now.
            List<ClipboardTile> tiles = LoadBlueprint(blueprint.Path);
            if (tiles != null && tiles.Count > 0)
            {
                BlueprintChosen?.Invoke(tiles);
            }
        }

        private void ShowBlueprintMenu(Point location)
        {
            if (SelectedBlueprint is not { } blueprint)
            {
                return;
            }

            ClearMenu(_blueprintMenu.Items);
            _blueprintMenu.Items.Add("Place", null, (_, _) => PlaceSelectedBlueprint());

            var move = new ToolStripMenuItem("Move to group");
            void Target(string text, string group)
            {
                var item = new ToolStripMenuItem(text) { Checked = blueprint.Group == group, Enabled = blueprint.Group != group };
                item.Click += (_, _) => MoveBlueprint(blueprint, group);
                move.DropDownItems.Add(item);
            }

            Target("Ungrouped", string.Empty);
            foreach (string group in _blueprintGroups)
            {
                Target(group, group);
            }

            move.DropDownItems.Add(new ToolStripSeparator());
            move.DropDownItems.Add("New group...", null, (_, _) => CreateBlueprintGroup(blueprint));
            _blueprintMenu.Items.Add(move);
            _blueprintMenu.Items.Add("Rename...", null, (_, _) => RenameSelectedBlueprint());
            _blueprintMenu.Items.Add("Delete", null, (_, _) => DeleteSelectedBlueprint());
            _blueprintMenu.Show(_blueprintList, location);
        }

        /// <summary>Runs a file operation on the blueprint folder, shows why when it fails, and re-reads the folder.</summary>
        private void BlueprintFileAction(string what, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                MessageBox.Show(this, $"Could not {what}: {ex.Message}", _messageTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            LoadBlueprints();
        }

        private void MoveBlueprint(BlueprintEntry blueprint, string group)
        {
            BlueprintFileAction("move the blueprint", () =>
            {
                string folder = GroupFolder(group);
                Directory.CreateDirectory(folder);
                File.Move(blueprint.Path, Path.Combine(folder, Path.GetFileName(blueprint.Path)));
            });
        }

        /// <summary>Asks for a group name and makes it; with a blueprint, moves that one into it.</summary>
        private void CreateBlueprintGroup(BlueprintEntry moveIn)
        {
            if (Prompt.Text(this, "New blueprint group", "Group name:") is not { } typed || Blueprints.SafeName(typed).Length == 0)
            {
                return;
            }

            string group = Blueprints.SafeName(typed);
            BlueprintFileAction("make the group", () =>
            {
                Directory.CreateDirectory(GroupFolder(group));
                if (moveIn != null)
                {
                    File.Move(moveIn.Path, Path.Combine(GroupFolder(group), Path.GetFileName(moveIn.Path)));
                }
            });

            _blueprintGroup = _blueprintGroups.Contains(group) ? group : _blueprintGroup;
            RebuildBlueprintPage();
        }

        /// <summary>
        /// The favorite groups that still exist, by their folder's own spelling, in the order they
        /// were added.
        /// </summary>
        private List<string> FavoriteGroups()
        {
            var favorites = new List<string>();
            foreach (string saved in EditorSettings.Current.FavoriteBlueprintGroups.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                string group = _blueprintGroups.Find(g => string.Equals(g, saved, StringComparison.OrdinalIgnoreCase));
                if (group != null && !favorites.Contains(group, StringComparer.OrdinalIgnoreCase))
                {
                    favorites.Add(group);
                }
            }

            return favorites;
        }

        private static void SaveFavoriteGroups(IEnumerable<string> groups)
        {
            string text = string.Join('|', groups.Distinct(StringComparer.OrdinalIgnoreCase));
            if (text != EditorSettings.Current.FavoriteBlueprintGroups)
            {
                EditorSettings settings = EditorSettings.Current.Clone();
                settings.FavoriteBlueprintGroups = text;
                settings.Apply();
            }
        }

        /// <summary>Adds a group to the favorites or takes it off, and redraws the bar.</summary>
        private void SetFavoriteGroup(string group, bool favorite)
        {
            List<string> favorites = FavoriteGroups();
            favorites.RemoveAll(g => string.Equals(g, group, StringComparison.OrdinalIgnoreCase));
            if (favorite)
            {
                favorites.Add(group);
            }

            SaveFavoriteGroups(favorites);
            RebuildBlueprintGroupBar();
        }

        /// <summary>
        /// Forgets favorites whose folder is gone (deleted or renamed outside the editor), after the
        /// blueprint folder was read.
        /// </summary>
        private void DropMissingFavoriteGroups()
        {
            SaveFavoriteGroups(FavoriteGroups());
        }

        private void RenameBlueprintGroup(string group)
        {
            if (Prompt.Text(this, "Rename blueprint group", "Group name:", group) is not { } typed ||
                Blueprints.SafeName(typed) is not { Length: > 0 } name || name == group)
            {
                return;
            }

            if (_blueprintGroup == group)
            {
                _blueprintGroup = name;
            }

            // A favorite keeps its place under the new name, once the folder has moved.
            List<string> favorites = FavoriteGroups().ConvertAll(g => string.Equals(g, group, StringComparison.OrdinalIgnoreCase) ? name : g);
            BlueprintFileAction("rename the group", () =>
            {
                Directory.Move(GroupFolder(group), GroupFolder(name));
                SaveFavoriteGroups(favorites);
            });
        }

        /// <summary>Moves a group's blueprints to no group and removes the group's folder once it is empty.</summary>
        private void RemoveBlueprintGroup(string group)
        {
            if (_blueprintGroup == group)
            {
                _blueprintGroup = string.Empty;
            }

            BlueprintFileAction("remove the group", () =>
            {
                foreach (string path in Directory.EnumerateFiles(GroupFolder(group), "*.txt").ToList())
                {
                    // A name already taken in no group gets " (2)"..., so nothing is overwritten or left behind.
                    (string target, _) = Blueprints.UniqueName(Blueprints.Folder, Path.GetFileNameWithoutExtension(path));
                    File.Move(path, target);
                }

                // Only an empty folder goes: anything else in it is not ours to delete.
                if (!Directory.EnumerateFileSystemEntries(GroupFolder(group)).Any())
                {
                    Directory.Delete(GroupFolder(group));
                }
            });
        }

        /// <summary>
        /// Renames a blueprint: the name line in the file and the file itself, which gets " (2)"...
        /// when another blueprint in the group has that file name.
        /// </summary>
        private void RenameSelectedBlueprint()
        {
            if (SelectedBlueprint is not { } blueprint ||
                Prompt.Text(this, "Rename blueprint", "Blueprint name:", blueprint.Name) is not { } typed ||
                typed.Trim() is not { Length: > 0 } wanted || wanted == blueprint.Name || Blueprints.SafeName(wanted).Length == 0)
            {
                return;
            }

            BlueprintFileAction("rename the blueprint", () =>
            {
                (List<ClipboardTile> tiles, _) = Blueprints.Read(blueprint.Path);
                if (tiles == null)
                {
                    throw new IOException("the file could not be read.");
                }

                (string path, string name) = Blueprints.UniqueName(Path.GetDirectoryName(blueprint.Path), wanted, own: blueprint.Path);
                Blueprints.Write(path, tiles, name);
                if (!string.Equals(Path.GetFullPath(path), Path.GetFullPath(blueprint.Path), StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(blueprint.Path);
                }
            });
        }

        private void DeleteSelectedBlueprint()
        {
            if (SelectedBlueprint is not { } blueprint ||
                MessageBox.Show(this, $"Delete the blueprint \"{blueprint.Name}\"?", _messageTitle, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            {
                return;
            }

            BlueprintFileAction("delete the blueprint", () => File.Delete(blueprint.Path));
        }
    }
}
