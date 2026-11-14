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
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;
using UoFiddler.Plugin.MultiEditor.Tools;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// A library of UO Architect designs: every .uoa file (single designs and batches) and design
    /// set (Designs.idx + .bin) under a folder, listed by the category and subsection stored in
    /// them, with a filter and a preview. A design can be opened as a new multi or placed into
    /// the one being edited.
    /// </summary>
    internal sealed partial class DesignLibraryPanel : UserControl
    {
        /// <summary>Designs with more items than this are not drawn as a preview on selection: it would stall the list.</summary>
        private const int _previewLimit = 8000;

        private const int _previewSize = 220;

        private readonly List<UoaDesign> _all = new List<UoaDesign>();
        private readonly List<UoaDesign> _shown = new List<UoaDesign>();
        private bool _loaded;

        /// <summary>The category and subsection chosen in the tree; null means any.</summary>
        private string _category;
        private string _subsection;

        public DesignLibraryPanel()
        {
            InitializeComponent();

            Disposed += DesignLibraryPanel_Disposed;
        }

        private void DesignLibraryPanel_Disposed(object sender, EventArgs e)
        {
            // The timer is disposed with the designer's components; the preview image is ours.
            _preview.Image?.Dispose();
        }

        private void ChooseButton_Click(object sender, EventArgs e)
        {
            ChooseFolder();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadLibrary();
        }

        private void Filter_TextChanged(object sender, EventArgs e)
        {
            RebuildList();
        }

        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            (_category, _subsection) = e.Node?.Tag is ValueTuple<string, string> key ? key : (null, null);
            RebuildList();
        }

        private void List_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            UoaDesign design = _shown[e.ItemIndex];
            e.Item = new ListViewItem(DocumentIO.DesignTitle(design));
            e.Item.SubItems.Add($"{design.Width}x{design.Height}");
            e.Item.SubItems.Add(design.ItemCount.ToString("N0"));
        }

        private void List_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectionChanged();
        }

        // Shift ranges in a virtual list.
        private void List_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            OnSelectionChanged();
        }

        private void List_DoubleClick(object sender, EventArgs e)
        {
            RaiseOpen();
        }

        private void List_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RaiseOpen();
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        private void Open_Click(object sender, EventArgs e)
        {
            RaiseOpen();
        }

        private void Place_Click(object sender, EventArgs e)
        {
            if (Selected is { } design)
            {
                PlaceRequested?.Invoke(design);
            }
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            ShowPreview();
        }

        /// <summary>Open the designs, each as a new multi.</summary>
        public event Action<IReadOnlyList<UoaDesign>> OpenRequested;

        /// <summary>Place the design into the multi being edited.</summary>
        public event Action<UoaDesign> PlaceRequested;

        private List<UoaDesign> SelectedDesigns
        {
            get
            {
                var designs = new List<UoaDesign>();
                foreach (int index in _list.SelectedIndices)
                {
                    if (index < _shown.Count)
                    {
                        designs.Add(_shown[index]);
                    }
                }

                return designs;
            }
        }

        private UoaDesign Selected => _list.SelectedIndices.Count > 0 && _list.SelectedIndices[0] < _shown.Count ? _shown[_list.SelectedIndices[0]] : null;

        /// <summary>A design's tiles for pasting: from the lowest X, Y and Z of its items.</summary>
        public static List<ClipboardTile> ToTiles(MultiComponentList multi)
        {
            var tiles = new List<ClipboardTile>(multi.SortedTiles.Length);
            if (multi.SortedTiles.Length == 0)
            {
                return tiles;
            }

            int minX = multi.SortedTiles.Min(t => t.OffsetX), minY = multi.SortedTiles.Min(t => t.OffsetY), minZ = multi.SortedTiles.Min(t => t.OffsetZ);
            foreach (MultiComponentList.MultiTileEntry tile in multi.SortedTiles)
            {
                tiles.Add(new ClipboardTile(tile.ItemId, tile.OffsetX - minX, tile.OffsetY - minY, tile.OffsetZ - minZ, false, 0));
            }

            return tiles;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && !_loaded)
            {
                LoadLibrary();
            }
        }

        private void ChooseFolder()
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Folder with UO Architect designs (.uoa)",
                UseDescriptionForTitle = true,
                SelectedPath = EditorSettings.Current.DesignLibraryFolder
            };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            EditorSettings settings = EditorSettings.Current.Clone();
            settings.DesignLibraryFolder = dialog.SelectedPath;
            settings.Apply();
            LoadLibrary();
        }

        /// <summary>Reads the header of every design under the folder. The items are only read when a design is used.</summary>
        private void LoadLibrary()
        {
            _loaded = true;
            _all.Clear();
            string folder = EditorSettings.Current.DesignLibraryFolder;
            _folderLabel.Text = string.IsNullOrEmpty(folder) ? "no folder chosen" : folder;
            _folderLabel.ToolTipText = folder;

            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            {
                Cursor previous = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    foreach (string pattern in new[] { "*.uoa", "*.idx" })
                    {
                        foreach (string file in Directory.EnumerateFiles(folder, pattern, SearchOption.AllDirectories))
                        {
                            // An .idx counts only with its .bin: other tools use the extension too.
                            if (pattern == "*.idx" && !File.Exists(Path.ChangeExtension(file, ".bin")))
                            {
                                continue;
                            }

                            _all.AddRange(UoaDesignFile.ReadHeaders(file).Where(d => d.ItemCount > 0));
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // What could be read is listed.
                }
                finally
                {
                    Cursor.Current = previous;
                }
            }

            RebuildTree();
            RebuildList();
        }

        private static string CategoryOf(UoaDesign design) => string.IsNullOrWhiteSpace(design.Category) ? "(no category)" : design.Category.Trim();

        private static string SubsectionOf(UoaDesign design) => string.IsNullOrWhiteSpace(design.Subsection) ? "(none)" : design.Subsection.Trim();

        private void RebuildTree()
        {
            _tree.BeginUpdate();
            try
            {
                _tree.Nodes.Clear();
                _category = _subsection = null;
                var root = new TreeNode($"All designs ({_all.Count:N0})") { Tag = ((string)null, (string)null) };
                _tree.Nodes.Add(root);
                foreach (IGrouping<string, UoaDesign> category in _all.GroupBy(CategoryOf).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
                {
                    var node = new TreeNode($"{category.Key} ({category.Count():N0})") { Tag = (category.Key, (string)null) };
                    List<IGrouping<string, UoaDesign>> subsections = category.GroupBy(SubsectionOf).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase).ToList();
                    if (subsections.Count > 1)
                    {
                        foreach (IGrouping<string, UoaDesign> subsection in subsections)
                        {
                            node.Nodes.Add(new TreeNode($"{subsection.Key} ({subsection.Count():N0})") { Tag = (category.Key, subsection.Key) });
                        }
                    }

                    root.Nodes.Add(node);
                }

                root.Expand();
                _tree.SelectedNode = root;
            }
            finally
            {
                _tree.EndUpdate();
            }
        }

        private void RebuildList()
        {
            string filter = _filter.Text.Trim();
            _shown.Clear();
            foreach (UoaDesign design in _all)
            {
                if ((_category == null || CategoryOf(design) == _category) &&
                    (_subsection == null || SubsectionOf(design) == _subsection) &&
                    (filter.Length == 0 || DocumentIO.DesignTitle(design).Contains(filter, StringComparison.OrdinalIgnoreCase)))
                {
                    _shown.Add(design);
                }
            }

            _shown.Sort((a, b) => string.Compare(DocumentIO.DesignTitle(a), DocumentIO.DesignTitle(b), StringComparison.OrdinalIgnoreCase));
            _list.SelectedIndices.Clear();
            _list.VirtualListSize = _shown.Count;
            _list.Invalidate();
            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            UoaDesign design = Selected;
            int count = _list.SelectedIndices.Count;
            _open.Enabled = design != null;
            _open.Text = count > 1 ? $"Open {count} in tabs" : "Open in a new tab";

            // Placing is a paste under the cursor: one design at a time.
            _place.Enabled = count == 1;
            if (design == null)
            {
                SetPreview(null);
                _info.Text = _all.Count == 0
                    ? "Choose a folder with UO Architect designs (.uoa files)."
                    : $"{_shown.Count:N0} designs";
                return;
            }

            _info.Text = count > 1
                ? $"{count} designs selected: each opens in its own tab"
                : $"{CategoryOf(design)} / {SubsectionOf(design)}\r\n{Path.GetFileName(design.FileName)}";
            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private void ShowPreview()
        {
            UoaDesign design = Selected;
            if (design == null)
            {
                return;
            }

            if (design.ItemCount > _previewLimit)
            {
                SetPreview(null);
                _info.Text = $"{design.ItemCount:N0} items: too large to preview here. Open it to see it.";
                return;
            }

            try
            {
                MultiComponentList multi = UoaDesignFile.Load(design);
                SetPreview(multi == MultiComponentList.Empty ? null : Prefabs.Thumbnail(ToTiles(multi), _previewSize));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                SetPreview(null);
            }
        }

        private void SetPreview(Image image)
        {
            Image old = _preview.Image;
            _preview.Image = image;
            old?.Dispose();
        }

        private void RaiseOpen()
        {
            List<UoaDesign> designs = SelectedDesigns;
            if (designs.Count > 0)
            {
                OpenRequested?.Invoke(designs);
            }
        }
    }
}
