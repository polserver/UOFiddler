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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace UoFiddler.Controls.UserControls
{
    /// <summary>
    /// A node of a <see cref="VirtualTreeView"/>. Nodes are plain objects: none of them owns a native item.
    /// </summary>
    public sealed class VirtualTreeNode
    {
        private string _text = string.Empty;
        private Color _foreColor = Color.Empty;
        private VirtualTreeView _rootOwner;

        public VirtualTreeNode()
        {
            Nodes = new VirtualTreeNodeCollection(null, this);
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value ?? string.Empty;
                TreeView?.Invalidate();
            }
        }

        public object Tag { get; set; }

        public Color ForeColor
        {
            get => _foreColor;
            set
            {
                if (_foreColor != value)
                {
                    _foreColor = value;
                    TreeView?.Invalidate();
                }
            }
        }

        public string ToolTipText { get; set; } = string.Empty;

        public VirtualTreeNode Parent { get; internal set; }

        public VirtualTreeNodeCollection Nodes { get; }

        public bool IsExpanded { get; internal set; }

        public int Level => Parent == null ? 0 : Parent.Level + 1;

        public VirtualTreeView TreeView => Parent != null ? Parent.TreeView : _rootOwner;

        internal void SetRootOwner(VirtualTreeView owner)
        {
            _rootOwner = owner;
        }

        public void Expand()
        {
            VirtualTreeView tree = TreeView;
            if (tree != null)
            {
                tree.SetExpanded(this, true);
            }
            else
            {
                IsExpanded = true;
            }
        }

        public void Collapse()
        {
            VirtualTreeView tree = TreeView;
            if (tree != null)
            {
                tree.SetExpanded(this, false);
            }
            else
            {
                IsExpanded = false;
            }
        }

        public void Remove()
        {
            if (Parent != null)
            {
                Parent.Nodes.Remove(this);
            }
            else
            {
                _rootOwner?.Nodes.Remove(this);
            }
        }

        public override string ToString() => Text;
    }

    public sealed class VirtualTreeNodeCollection : IEnumerable<VirtualTreeNode>
    {
        private readonly List<VirtualTreeNode> _nodes = new();
        private readonly VirtualTreeView _tree;
        private readonly VirtualTreeNode _owner;

        internal VirtualTreeNodeCollection(VirtualTreeView tree, VirtualTreeNode owner)
        {
            _tree = tree;
            _owner = owner;
        }

        public int Count => _nodes.Count;

        public VirtualTreeNode this[int index] => _nodes[index];

        private VirtualTreeView Tree => _tree ?? _owner?.TreeView;

        public void Add(VirtualTreeNode node)
        {
            Attach(node);
            _nodes.Add(node);
            Tree?.OnStructureChanged();
        }

        public void AddRange(IEnumerable<VirtualTreeNode> nodes)
        {
            foreach (VirtualTreeNode node in nodes)
            {
                Attach(node);
                _nodes.Add(node);
            }

            Tree?.OnStructureChanged();
        }

        public void Remove(VirtualTreeNode node)
        {
            if (_nodes.Remove(node))
            {
                VirtualTreeView tree = Tree;
                Detach(node);
                tree?.OnStructureChanged();
            }
        }

        public void Clear()
        {
            VirtualTreeView tree = Tree;
            foreach (VirtualTreeNode node in _nodes)
            {
                Detach(node);
            }

            _nodes.Clear();
            tree?.OnStructureChanged();
        }

        public int IndexOf(VirtualTreeNode node) => _nodes.IndexOf(node);

        public IEnumerator<VirtualTreeNode> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void Attach(VirtualTreeNode node)
        {
            node.Parent = _owner;
            node.SetRootOwner(_owner == null ? _tree : null);
        }

        private static void Detach(VirtualTreeNode node)
        {
            node.Parent = null;
            node.SetRootOwner(null);
        }
    }

    public sealed class VirtualTreeNodeEventArgs : EventArgs
    {
        public VirtualTreeNodeEventArgs(VirtualTreeNode node)
        {
            Node = node;
        }

        public VirtualTreeNode Node { get; }
    }

    /// <summary>
    /// A tree drawn on a virtual <see cref="ListView"/>: only the visible rows are ever asked for, so filling
    /// and destroying it costs the same for ten nodes or a hundred thousand, unlike <see cref="TreeView"/>,
    /// which makes and destroys a native item per node.
    /// </summary>
    public class VirtualTreeView : ListView
    {
        private const int _indent = 16;
        private const int _glyphSize = 16;

        private readonly List<VirtualTreeNode> _rows = new();
        private VirtualTreeNode _selectedNode;
        private int _updateDepth;
        private bool _rowsDirty;
        private bool _suppressSelectionEvents;

        public VirtualTreeView()
        {
            Nodes = new VirtualTreeNodeCollection(this, null);

            VirtualMode = true;
            View = View.Details;
            HeaderStyle = ColumnHeaderStyle.None;
            FullRowSelect = true;
            MultiSelect = false;
            HideSelection = false;
            OwnerDraw = true;
            DoubleBuffered = true;
            Columns.Add(string.Empty);
        }

        /// <summary>Raised before a node's children are shown; a lazy node fills them in here.</summary>
        public event EventHandler<VirtualTreeNodeEventArgs> BeforeExpand;

        /// <summary>Raised when the selected node changes, including when it is set from code.</summary>
        public event EventHandler<VirtualTreeNodeEventArgs> AfterSelect;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VirtualTreeNodeCollection Nodes { get; }

        [DefaultValue(false)]
        public bool ShowNodeToolTips
        {
            get => ShowItemToolTips;
            set => ShowItemToolTips = value;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VirtualTreeNode SelectedNode
        {
            get => _selectedNode;
            set => Select(value, raiseEvent: true);
        }

        public new void BeginUpdate()
        {
            _updateDepth++;
            base.BeginUpdate();
        }

        public new void EndUpdate()
        {
            if (_updateDepth > 0 && --_updateDepth == 0 && _rowsDirty)
            {
                RebuildRows();
            }

            base.EndUpdate();
        }

        internal void OnStructureChanged()
        {
            if (_updateDepth > 0)
            {
                _rowsDirty = true;
                return;
            }

            RebuildRows();
        }

        internal void SetExpanded(VirtualTreeNode node, bool expanded)
        {
            if (node.IsExpanded == expanded)
            {
                return;
            }

            if (expanded)
            {
                // The handler may replace the children; collect one rebuild for both changes
                BeginUpdate();
                try
                {
                    BeforeExpand?.Invoke(this, new VirtualTreeNodeEventArgs(node));
                    node.IsExpanded = node.Nodes.Count > 0;
                    _rowsDirty = true;
                }
                finally
                {
                    EndUpdate();
                }
            }
            else
            {
                node.IsExpanded = false;
                OnStructureChanged();
            }
        }

        private void RebuildRows()
        {
            _rowsDirty = false;
            int oldIndex = _selectedNode == null ? -1 : _rows.IndexOf(_selectedNode);

            _rows.Clear();
            foreach (VirtualTreeNode node in Nodes)
            {
                AddRows(node);
            }

            _suppressSelectionEvents = true;
            try
            {
                SelectedIndices.Clear();
                VirtualListSize = _rows.Count;
            }
            finally
            {
                _suppressSelectionEvents = false;
            }

            if (_selectedNode == null)
            {
                Invalidate();
                return;
            }

            if (_rows.Contains(_selectedNode))
            {
                Select(_selectedNode, raiseEvent: false);
                return;
            }

            // Like TreeView: a node hidden by a collapse passes the selection to the nearest shown parent;
            // a removed node passes it to the row that took its place.
            VirtualTreeNode replacement = null;
            if (IsInTree(_selectedNode))
            {
                replacement = _selectedNode.Parent;
                while (replacement != null && !_rows.Contains(replacement))
                {
                    replacement = replacement.Parent;
                }
            }
            else if (_rows.Count > 0 && oldIndex >= 0)
            {
                replacement = _rows[Math.Min(oldIndex, _rows.Count - 1)];
            }

            _selectedNode = null;
            Select(replacement, raiseEvent: true);
        }

        private void AddRows(VirtualTreeNode node)
        {
            _rows.Add(node);
            if (!node.IsExpanded)
            {
                return;
            }

            foreach (VirtualTreeNode child in node.Nodes)
            {
                AddRows(child);
            }
        }

        private bool IsInTree(VirtualTreeNode node)
        {
            while (node.Parent != null)
            {
                node = node.Parent;
            }

            return node.TreeView == this && Nodes.IndexOf(node) >= 0;
        }

        private void Select(VirtualTreeNode node, bool raiseEvent)
        {
            if (node != null)
            {
                for (VirtualTreeNode parent = node.Parent; parent != null; parent = parent.Parent)
                {
                    parent.Expand();
                }

                if (_rowsDirty && _updateDepth == 0)
                {
                    RebuildRows();
                }
            }

            bool changed = !ReferenceEquals(node, _selectedNode);
            _selectedNode = node;

            int index = node == null ? -1 : _rows.IndexOf(node);
            _suppressSelectionEvents = true;
            try
            {
                SelectedIndices.Clear();
                if (index >= 0)
                {
                    SelectedIndices.Add(index);
                    FocusedItem = Items[index];
                    EnsureVisible(index);
                }
            }
            finally
            {
                _suppressSelectionEvents = false;
            }

            Invalidate();
            if (changed && raiseEvent)
            {
                AfterSelect?.Invoke(this, new VirtualTreeNodeEventArgs(node));
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            if (_suppressSelectionEvents || SelectedIndices.Count == 0)
            {
                return;
            }

            int index = SelectedIndices[0];
            VirtualTreeNode node = index < _rows.Count ? _rows[index] : null;
            if (node != null && !ReferenceEquals(node, _selectedNode))
            {
                _selectedNode = node;
                AfterSelect?.Invoke(this, new VirtualTreeNodeEventArgs(node));
            }
        }

        protected override void OnRetrieveVirtualItem(RetrieveVirtualItemEventArgs e)
        {
            VirtualTreeNode node = e.ItemIndex < _rows.Count ? _rows[e.ItemIndex] : null;
            e.Item = new ListViewItem(node?.Text ?? string.Empty) { ToolTipText = node?.ToolTipText ?? string.Empty };
            base.OnRetrieveVirtualItem(e);
        }

        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            VirtualTreeNode node = e.ItemIndex < _rows.Count ? _rows[e.ItemIndex] : null;
            if (node == null)
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, e.Bounds.Y, ClientSize.Width, e.Bounds.Height);
            bool selected = ReferenceEquals(node, _selectedNode);
            bool focused = selected && Focused;
            Color back = focused ? SystemColors.Highlight : selected ? SystemColors.ControlLight : BackColor;
            using (var brush = new SolidBrush(back))
            {
                e.Graphics.FillRectangle(brush, bounds);
            }

            int x = bounds.X + (node.Level * _indent);
            if (node.Nodes.Count > 0)
            {
                DrawGlyph(e.Graphics, new Rectangle(x, bounds.Y + ((bounds.Height - _glyphSize) / 2), _glyphSize, _glyphSize), node.IsExpanded);
            }

            Color fore = focused ? SystemColors.HighlightText : node.ForeColor.IsEmpty ? ForeColor : node.ForeColor;
            var textBounds = new Rectangle(x + _glyphSize + 2, bounds.Y, Math.Max(0, bounds.Right - x - _glyphSize - 2), bounds.Height);
            TextRenderer.DrawText(e.Graphics, node.Text, Font, textBounds, fore,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
        }

        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            // The whole row is drawn in OnDrawItem
        }

        private void DrawGlyph(Graphics g, Rectangle rect, bool expanded)
        {
            VisualStyleElement element = expanded ? VisualStyleElement.TreeView.Glyph.Opened : VisualStyleElement.TreeView.Glyph.Closed;
            if (Application.RenderWithVisualStyles && VisualStyleRenderer.IsElementDefined(element))
            {
                new VisualStyleRenderer(element).DrawBackground(g, rect);
                return;
            }

            var box = new Rectangle(rect.X + 4, rect.Y + 4, 8, 8);
            g.DrawRectangle(SystemPens.GrayText, box);
            g.DrawLine(SystemPens.WindowText, box.X + 2, box.Y + 4, box.Right - 2, box.Y + 4);
            if (!expanded)
            {
                g.DrawLine(SystemPens.WindowText, box.X + 4, box.Y + 2, box.X + 4, box.Bottom - 2);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            VirtualTreeNode node = NodeAt(e.Location);
            if (node != null && e.Button == MouseButtons.Left && node.Nodes.Count > 0)
            {
                int glyphX = node.Level * _indent;
                if (e.X >= glyphX && e.X < glyphX + _glyphSize)
                {
                    Toggle(node);
                    return;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            VirtualTreeNode node = NodeAt(e.Location);
            if (node != null && e.Button == MouseButtons.Left && node.Nodes.Count > 0)
            {
                Toggle(node);
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            VirtualTreeNode node = _selectedNode;
            if (node != null)
            {
                switch (e.KeyCode)
                {
                    case Keys.Right:
                    case Keys.Add:
                        if (node.Nodes.Count > 0 && !node.IsExpanded)
                        {
                            node.Expand();
                        }
                        else if (e.KeyCode == Keys.Right && node.IsExpanded && node.Nodes.Count > 0)
                        {
                            SelectedNode = node.Nodes[0];
                        }

                        e.Handled = true;
                        return;
                    case Keys.Left:
                    case Keys.Subtract:
                        if (node.IsExpanded)
                        {
                            node.Collapse();
                        }
                        else if (e.KeyCode == Keys.Left && node.Parent != null)
                        {
                            SelectedNode = node.Parent;
                        }

                        e.Handled = true;
                        return;
                }
            }

            base.OnKeyDown(e);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (Columns.Count > 0)
            {
                Columns[0].Width = Math.Max(1, ClientSize.Width);
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        private VirtualTreeNode NodeAt(Point location)
        {
            ListViewItem item = GetItemAt(location.X, location.Y);
            return item != null && item.Index < _rows.Count ? _rows[item.Index] : null;
        }

        private void Toggle(VirtualTreeNode node)
        {
            if (node.IsExpanded)
            {
                node.Collapse();
            }
            else
            {
                node.Expand();
            }
        }
    }
}
