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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Classes.TilesInfo;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class TileGroupsControl : UserControl
    {
        private readonly List<TileGroupFile> _files = new();
        private readonly HashSet<string> _hiddenFileNames = new(StringComparer.OrdinalIgnoreCase);
        private TileSlot _currentSlot;

        public TileGroupsControl()
        {
            InitializeComponent();
        }

        private void OnControlLoad(object sender, EventArgs e)
        {
            ApplyPreviewBackgroundColor();
            ControlEvents.PreviewBackgroundColorChangeEvent += ApplyPreviewBackgroundColor;
            ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;

            ReloadFiles();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            ControlEvents.PreviewBackgroundColorChangeEvent -= ApplyPreviewBackgroundColor;
            ControlEvents.FilePathChangeEvent -= OnFilePathChangeEvent;
            base.OnHandleDestroyed(e);
        }

        /// <summary>Another client's files are in use: drop these groups, their edits and the cliloc.</summary>
        private void OnFilePathChangeEvent() => ReloadFiles();

        private void ApplyPreviewBackgroundColor()
        {
            DetailPictureBox.BackColor = Options.PreviewBackgroundColor;
            if (_currentSlot != null)
            {
                DrawPreview(_currentSlot.TileId);
            }
            else
            {
                DetailPictureBox.Image?.Dispose();
                DetailPictureBox.Image = null;
            }
        }

        private void OnReloadClick(object sender, EventArgs e)
        {
            if (AnyDirty() &&
                MessageBox.Show("Discard unsaved changes and reload from disk?", "Reload",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            ReloadFiles();
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            var saved = new List<string>();
            foreach (var file in _files)
            {
                if (!file.IsDirty) continue;
                string target = Path.Combine(Options.OutputPath, Path.GetFileName(file.FilePath));
                try
                {
                    file.Save(target);
                    saved.Add(target);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    MessageBox.Show($"Failed to save {file.DisplayName}: {ex.Message}",
                        "Save error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            UpdateSaveButtonState();
            if (saved.Count > 0)
            {
                RefreshAllTreeNodes();
                string names = string.Join(", ", saved.ConvertAll(Path.GetFileName));
                FileSavedDialog.Show(FindForm(), saved[0], $"Saved {names} to the output folder.");
            }
        }

        private void ReloadFiles()
        {
            TileGroupCliloc.Reset();
            _files.Clear();
            _files.AddRange(TileGroupRegistry.LoadAll(out List<string> errors));
            if (errors.Count > 0)
            {
                MessageBox.Show("These tile group files could not be read:" + Environment.NewLine + Environment.NewLine
                    + string.Join(Environment.NewLine, errors),
                    "Tile Groups", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            BuildFileFilterMenu();
            RebuildTree();
            ClearDetailPane();
            UpdateSaveButtonState();
        }

        private void BuildFileFilterMenu()
        {
            FileFilterDropDown.DropDownItems.Clear();
            foreach (var file in _files)
            {
                var item = new ToolStripMenuItem(file.DisplayName)
                {
                    Checked = !_hiddenFileNames.Contains(file.DisplayName),
                    CheckOnClick = true,
                    Tag = file
                };
                item.CheckedChanged += (s, e) =>
                {
                    if (item.Checked) _hiddenFileNames.Remove(file.DisplayName);
                    else _hiddenFileNames.Add(file.DisplayName);
                    RebuildTree();
                };
                FileFilterDropDown.DropDownItems.Add(item);
            }
        }

        private void RebuildTree()
        {
            // Nodes are rebuilt from the model, whose objects survive the rebuild, so the tree's
            // state is remembered by node tag.
            var expanded = new HashSet<object>(ReferenceEqualityComparer.Instance);
            CollectExpanded(GroupsTreeView.Nodes, expanded);
            object selected = GroupsTreeView.SelectedNode?.Tag;
            object top = GroupsTreeView.TopNode?.Tag;

            GroupsTreeView.BeginUpdate();
            try
            {
                GroupsTreeView.Nodes.Clear();
                foreach (var file in _files)
                {
                    if (_hiddenFileNames.Contains(file.DisplayName))
                    {
                        continue;
                    }

                    var fileNode = new TreeNode(file.DisplayName) { Tag = file };
                    foreach (var category in file.Categories)
                    {
                        var catNode = new TreeNode(FormatCategoryName(category)) { Tag = category };
                        if (file.HasStyleColumn)
                        {
                            foreach (var style in category.Styles)
                            {
                                var styleNode = new TreeNode(FormatStyleName(style)) { Tag = style };
                                AddSlotNodes(styleNode, style);
                                catNode.Nodes.Add(styleNode);
                            }
                        }
                        else if (category.Styles.Count > 0)
                        {
                            AddSlotNodes(catNode, category.Styles[0]);
                        }
                        fileNode.Nodes.Add(catNode);
                    }
                    GroupsTreeView.Nodes.Add(fileNode);
                }

                ExpandRemembered(GroupsTreeView.Nodes, expanded);
                TreeNode topNode = top == null ? null : FindNodeByTag(GroupsTreeView.Nodes, top);
                if (topNode != null)
                {
                    GroupsTreeView.TopNode = topNode;
                }

                TreeNode selectedNode = selected == null ? null : FindNodeByTag(GroupsTreeView.Nodes, selected);
                if (selectedNode != null)
                {
                    GroupsTreeView.SelectedNode = selectedNode;
                }
            }
            finally
            {
                GroupsTreeView.EndUpdate();
            }
        }

        private static void CollectExpanded(TreeNodeCollection nodes, HashSet<object> expanded)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.IsExpanded && node.Tag != null)
                {
                    expanded.Add(node.Tag);
                    CollectExpanded(node.Nodes, expanded);
                }
            }
        }

        private static void ExpandRemembered(TreeNodeCollection nodes, HashSet<object> expanded)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag != null && expanded.Contains(node.Tag))
                {
                    node.Expand();
                    ExpandRemembered(node.Nodes, expanded);
                }
            }
        }

        private static void AddSlotNodes(TreeNode parent, TileStyle style)
        {
            foreach (var slot in style.Slots)
            {
                parent.Nodes.Add(BuildSlotNode(slot));
            }
        }

        private static TreeNode BuildSlotNode(TileSlot slot)
        {
            return new TreeNode(FormatSlotText(slot)) { Tag = slot };
        }

        private static string FormatSlotText(TileSlot slot)
        {
            string suffix = slot.IsEmpty ? " (empty)" : (!slot.IsValid ? " [!]" : string.Empty);
            return $"{slot.Label}: {slot.TileId}{suffix}";
        }

        private static string FormatCategoryName(TileCategory category)
        {
            string baseText = string.IsNullOrWhiteSpace(category.Name) ? $"Category {category.Index}" : category.Name;
            return $"[{category.Index}] {baseText}";
        }

        private static string FormatStyleName(TileStyle style)
        {
            string baseText = string.IsNullOrWhiteSpace(style.Name) ? $"Style {style.Index}" : style.Name;
            return style.Index >= 0 ? $"[{style.Index}] {baseText}" : baseText;
        }

        private void OnTreeNodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                GroupsTreeView.SelectedNode = e.Node;
            }
        }

        private void OnNodeContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            nodeContextMenu.Items.Clear();
            var node = GroupsTreeView.SelectedNode;
            if (node?.Tag == null)
            {
                e.Cancel = true;
                return;
            }

            switch (node.Tag)
            {
                case TileGroupFile file:
                    nodeContextMenu.Items.Add("Add category...", null, (s, a) => AddCategory(file));
                    break;
                case TileCategory category:
                    if (category.File.HasStyleColumn)
                    {
                        nodeContextMenu.Items.Add("Add style...", null, (s, a) => AddStyle(category));
                    }
                    nodeContextMenu.Items.Add("Rename...", null, (s, a) => RenameCategory(category));
                    if (category.File.HasTidColumn)
                    {
                        nodeContextMenu.Items.Add("Set cliloc TID...", null, (s, a) => EditCategoryTid(category));
                    }
                    nodeContextMenu.Items.Add(new ToolStripSeparator());
                    nodeContextMenu.Items.Add("Delete category", null, (s, a) => DeleteCategory(category));
                    break;
                case TileStyle style when style.Index >= 0:
                    nodeContextMenu.Items.Add("Rename...", null, (s, a) => RenameStyle(style));
                    nodeContextMenu.Items.Add(new ToolStripSeparator());
                    nodeContextMenu.Items.Add("Delete style", null, (s, a) => DeleteStyle(style));
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }

        private void AddCategory(TileGroupFile file)
        {
            using var dlg = new TileGroupNameForm(
                title: $"Add category to {file.DisplayName}",
                nameLabel: "Category name:",
                initialName: string.Empty,
                showTidField: file.HasTidColumn,
                initialTid: 0);

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var newCategory = file.CreateNewCategory(dlg.EnteredName, dlg.EnteredTid);
            RebuildTree();
            SelectByTag(newCategory);
            UpdateSaveButtonState();
        }

        private void AddStyle(TileCategory category)
        {
            using var dlg = new TileGroupNameForm(
                title: $"Add style to {category.Name}",
                nameLabel: "Style name:",
                initialName: string.Empty,
                showTidField: false,
                initialTid: 0);

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            var newStyle = category.File.CreateNewStyle(category, dlg.EnteredName);
            RebuildTree();
            SelectByTag(newStyle);
            UpdateSaveButtonState();
        }

        private void RenameCategory(TileCategory category)
        {
            if (category.Styles.Count == 0) return;
            var firstStyle = category.Styles[0];

            using var dlg = new TileGroupNameForm(
                title: "Rename category",
                nameLabel: "Category name:",
                initialName: category.Name,
                showTidField: false,
                initialTid: 0);

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            category.File.RenameStyle(firstStyle, dlg.EnteredName);
            RebuildTree();
            SelectByTag(category);
            UpdateSaveButtonState();
        }

        private void RenameStyle(TileStyle style)
        {
            using var dlg = new TileGroupNameForm(
                title: "Rename style",
                nameLabel: "Style name:",
                initialName: style.Name,
                showTidField: false,
                initialTid: 0);

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            style.Category.File.RenameStyle(style, dlg.EnteredName);
            RebuildTree();
            SelectByTag(style);
            UpdateSaveButtonState();
        }

        private void EditCategoryTid(TileCategory category)
        {
            using var dlg = new TileGroupNameForm(
                title: "Set cliloc TID",
                nameLabel: "Category name:",
                initialName: category.Name,
                showTidField: true,
                initialTid: category.Tid);

            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            if (!string.Equals(dlg.EnteredName, category.Name, StringComparison.Ordinal) && category.Styles.Count > 0)
            {
                category.File.RenameStyle(category.Styles[0], dlg.EnteredName);
            }
            category.File.UpdateCategoryTid(category, dlg.EnteredTid);
            RebuildTree();
            SelectByTag(category);
            UpdateSaveButtonState();
        }

        private void DeleteCategory(TileCategory category)
        {
            if (MessageBox.Show($"Delete category \"{category.Name}\" and all its styles?",
                    "Confirm delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            category.File.RemoveCategory(category);
            RebuildTree();
            ClearDetailPane();
            UpdateSaveButtonState();
        }

        private void DeleteStyle(TileStyle style)
        {
            if (MessageBox.Show($"Delete style \"{style.Name}\"?",
                    "Confirm delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                return;
            }

            style.Category.File.RemoveStyle(style);
            RebuildTree();
            ClearDetailPane();
            UpdateSaveButtonState();
        }

        private void SelectByTag(object tag)
        {
            var node = FindNodeByTag(GroupsTreeView.Nodes, tag);
            if (node != null)
            {
                GroupsTreeView.SelectedNode = node;
                node.EnsureVisible();
            }
        }

        private static TreeNode FindNodeByTag(TreeNodeCollection nodes, object tag)
        {
            foreach (TreeNode n in nodes)
            {
                if (ReferenceEquals(n.Tag, tag)) return n;
                var found = FindNodeByTag(n.Nodes, tag);
                if (found != null) return found;
            }
            return null;
        }

        private void OnTreeAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is TileSlot slot)
            {
                ShowSlot(slot);
            }
            else
            {
                ClearDetailPane();
            }
        }

        private void ShowSlot(TileSlot slot)
        {
            _currentSlot = slot;

            DrawPreview(slot.TileId);

            var sb = new StringBuilder();
            var style = slot.Style;
            var category = style?.Category;
            var file = category?.File;

            if (file != null) sb.AppendLine($"File: {file.DisplayName} ({Path.GetFileName(file.FilePath)})");
            if (category != null) sb.AppendLine($"Category: [{category.Index}] {category.Name}");
            if (style != null && style.Index >= 0) sb.AppendLine($"Style: [{style.Index}] {style.Name}");
            sb.AppendLine($"Slot: {slot.Label}");
            sb.AppendLine($"Tile ID: {slot.TileId} (0x{slot.TileId:X4})");

            if (slot.IsEmpty)
            {
                sb.AppendLine("(slot is empty)");
            }
            else if (!slot.IsValid)
            {
                sb.AppendLine("Art: <invalid — tile ID not present in art file>");
            }
            else if (TileData.ItemTable != null && slot.TileId < TileData.ItemTable.Length)
            {
                var itemData = TileData.ItemTable[slot.TileId];
                sb.AppendLine($"Name: {itemData.Name}");
                sb.AppendLine($"Flags: {itemData.Flags}");
            }
            else
            {
                sb.AppendLine("TileData: <no entry for this tile ID>");
            }

            if (file != null && file.HasTidColumn && category != null && category.Tid != 0)
            {
                string clilocText = TileGroupCliloc.Lookup(category.Tid);
                if (!string.IsNullOrEmpty(clilocText))
                {
                    sb.AppendLine();
                    sb.AppendLine($"Cliloc {category.Tid}: {clilocText}");
                }
                else
                {
                    sb.AppendLine();
                    sb.AppendLine($"Cliloc {category.Tid}: <not found in cliloc.enu>");
                }
            }

            InfoLabel.Text = sb.ToString();
            EditTileIdUpDown.Enabled = true;
            EditTileIdUpDown.Value = Math.Min(EditTileIdUpDown.Maximum, Math.Max(EditTileIdUpDown.Minimum, slot.TileId));
            ApplyButton.Enabled = true;
            RevertButton.Enabled = true;
        }

        private void ClearDetailPane()
        {
            _currentSlot = null;
            DetailPictureBox.Image?.Dispose();
            DetailPictureBox.Image = null;
            InfoLabel.Text = "Select a tile slot in the tree to view its art and details.";
            EditTileIdUpDown.Enabled = false;
            ApplyButton.Enabled = false;
            RevertButton.Enabled = false;
        }

        private void DrawPreview(int tileId)
        {
            var size = DetailPictureBox.Size;
            if (size.Width <= 0 || size.Height <= 0) return;

            Bitmap newBit = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(newBit))
            {
                g.Clear(Options.PreviewBackgroundColor);

                Bitmap art = (tileId == 0 || !Art.IsValidStatic(tileId)) ? null : Art.GetStatic(tileId);
                if (art != null)
                {
                    g.DrawImage(art, (size.Width - art.Width) / 2, Math.Max(5, (size.Height - art.Height) / 2));
                }
                else
                {
                    DrawPlaceholder(g, size);
                }
            }

            DetailPictureBox.Image?.Dispose();
            DetailPictureBox.Image = newBit;
        }

        private static void DrawPlaceholder(Graphics g, Size size)
        {
            const int cellSize = 8;
            using var brushA = new SolidBrush(Color.FromArgb(60, 0, 0, 0));
            using var brushB = new SolidBrush(Color.FromArgb(20, 0, 0, 0));
            for (int y = 0; y < size.Height; y += cellSize)
            {
                for (int x = 0; x < size.Width; x += cellSize)
                {
                    bool a = ((x / cellSize) + (y / cellSize)) % 2 == 0;
                    g.FillRectangle(a ? brushA : brushB, x, y, cellSize, cellSize);
                }
            }

            using var pen = new Pen(Color.DarkRed, 2);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int pad = 16;
            g.DrawLine(pen, pad, pad, size.Width - pad, size.Height - pad);
            g.DrawLine(pen, size.Width - pad, pad, pad, size.Height - pad);
        }

        private void OnApplyClick(object sender, EventArgs e)
        {
            if (_currentSlot == null) return;

            int newId = (int)EditTileIdUpDown.Value;
            if (newId == _currentSlot.TileId) return;

            if (newId != 0 && !Art.IsValidStatic(newId))
            {
                var result = MessageBox.Show(
                    $"Tile ID {newId} is not a valid static art entry. Apply anyway?",
                    "Invalid tile", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;
            }

            _currentSlot.TileId = newId;
            _currentSlot.Style.Category.File.IsDirty = true;

            RefreshNodeForSlot(_currentSlot);
            DrawPreview(newId);
            ShowSlot(_currentSlot);
            UpdateSaveButtonState();
        }

        private void OnRevertClick(object sender, EventArgs e)
        {
            if (_currentSlot == null) return;
            EditTileIdUpDown.Value = Math.Min(EditTileIdUpDown.Maximum, Math.Max(EditTileIdUpDown.Minimum, _currentSlot.TileId));
        }

        private void RefreshNodeForSlot(TileSlot slot)
        {
            var selected = GroupsTreeView.SelectedNode;
            if (selected != null && ReferenceEquals(selected.Tag, slot))
            {
                selected.Text = FormatSlotText(slot);
            }
        }

        private void RefreshAllTreeNodes()
        {
            GroupsTreeView.BeginUpdate();
            try
            {
                RefreshNodesRecursive(GroupsTreeView.Nodes);
            }
            finally
            {
                GroupsTreeView.EndUpdate();
            }
        }

        private static void RefreshNodesRecursive(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag is TileSlot slot)
                {
                    node.Text = FormatSlotText(slot);
                }
                RefreshNodesRecursive(node.Nodes);
            }
        }

        private bool AnyDirty()
        {
            foreach (var file in _files)
            {
                if (file.IsDirty) return true;
            }
            return false;
        }

        private void UpdateSaveButtonState()
        {
            bool dirty = AnyDirty();
            SaveButton.Enabled = dirty;
            Options.ChangedUltimaClass["TileGroups"] = dirty;
        }
    }
}
