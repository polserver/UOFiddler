using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls.TileView
{
    [Browsable(true)]
    public class TileViewControl : ScrollableControl
    {
        private int _itemsPerRow;

        /// <summary>
        /// Backwards compatibility with ListView ItemSelectionChanged event. Warning: Have a high chance to be changed in future.
        /// </summary>
        public event EventHandler<ListViewItemSelectionChangedEventArgs> ItemSelectionChanged;

        public event EventHandler<ListViewFocusedItemSelectionChangedEventArgs> FocusSelectionChanged;

        public IndicesCollection SelectedIndices = new IndicesCollection();

        private int _focusIndex = -1;

        /// <summary>
        /// Get or Set SelectedIndex, setting this property to -1 will remove selection, -2 is reserved for "do nothing".
        /// </summary>
        public int FocusIndex
        {
            get => _focusIndex;
            set
            {
                if (value >= VirtualListSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (_focusIndex == value)
                {
                    return;
                }

                if (value == -2)
                {
                    return;
                }

                int oldIndex = _focusIndex;

                _focusIndex = value;

                if (!_multiSelect)
                {
                    SelectIndex(value);
                }

                if (oldIndex != -1)
                {
                    RedrawItem(oldIndex);
                }

                FocusSelectionChanged?.Invoke(this, new ListViewFocusedItemSelectionChangedEventArgs(_focusIndex, true));

                if (value == -1)
                {
                    return;
                }

                RedrawItem(value);
                ScrollToItem(value);
            }
        }

        /// <summary>
        /// Scroll to item if it's out of view
        /// </summary>
        /// <param name="value">Item index</param>
        private void ScrollToItem(int value)
        {
            Point p = GetItemLocation(value);

            int y = -AutoScrollPosition.Y;
            if (y == p.Y)
            {
                return;
            }

            if (y > p.Y)
            {
                AutoScrollPosition = new Point(0, p.Y);
            }
            else if (p.Y - y + TotalTileSize.Height > Height)
            {
                //TODO: correct slight mispositioned while this.Height (control) is less than m_TotalTileSize.Height
                AutoScrollPosition = new Point(0, p.Y + TotalTileSize.Height - Height);
            }
        }

        private bool _multiSelect;

        [Browsable(true)]
        public bool MultiSelect
        {
            get => _multiSelect;
            set
            {
                _multiSelect = value;
                Invalidate(); // TODO: maybe just visible area? or selected items redraw?
            }
        }

        private int _virtualListSize;

        /// <summary>
        /// Get or Set amount of Items to be displayed.
        /// </summary>
        public int VirtualListSize
        {
            get => _virtualListSize;
            set
            {
                _virtualListSize = value;

                if (_focusIndex >= _virtualListSize)
                {
                    FocusIndex = -1; // we are setting public one so all events will be triggered properly
                }

                if (Size.IsEmpty)
                {
                    return;
                }

                SelectedIndices.Clear();
                _cachedIndices.Clear();
                UpdateAutoScrollSize();
            }
        }

        /// <summary>
        /// This function is being called whenever anything related to size of Tile or VirtualListSize is changed. It also updated ItemsPerRow value.
        /// </summary>
        /// <returns></returns>
        private void UpdateAutoScrollSize()
        {
            if (_virtualListSize == 0 || Size.IsEmpty)
            {
                return;
            }

            _itemsPerRow = DisplayRectangle.Width / TotalTileSize.Width;

            //if (m_ItemsPerRow < 1) m_ItemsPerRow = 1; // bug from line above, but could not reproduce

            // for now we are supporting only Vertical scrolling.
            AutoScrollMinSize = new Size(0, DivUp(_virtualListSize, _itemsPerRow) * TotalTileSize.Height);

            Invalidate();
        }

        private List<int> _cachedIndices = new List<int>();

        //http://stackoverflow.com/questions/921180/how-can-i-ensure-that-a-division-of-integers-is-always-rounded-up/924160#924160
        private static int DivUp(int a, int b)
        {
            int r = a / b;
            if (((a ^ b) >= 0) && (a % b != 0))
            {
                r++;
            }

            return r;
        }

        public Size TotalTileSize =>
            new Size(
                _tileSize.Width + _tileMargin.Horizontal + _tilePadding.Horizontal +
                (int)(_tileBorder.Width * 2),
                _tileSize.Height + _tileMargin.Vertical + _tilePadding.Vertical +
                (int)(_tileBorder.Width * 2));

        private Size _tileSize = new Size(256, 256);

        public Size TileSize
        {
            get => _tileSize;
            set
            {
                _tileSize = value;
                UpdateAutoScrollSize();
            }
        }

        private Padding _tileMargin = new Padding(2, 2, 2, 2); // external

        public Padding TileMargin
        {
            get => _tileMargin;
            set
            {
                _tileMargin = value;
                UpdateAutoScrollSize();
            }
        }

        private Padding _tilePadding = new Padding(2, 2, 2, 2);

        public Padding TilePadding
        {
            get => _tilePadding;
            set
            {
                _tilePadding = value;
                UpdateAutoScrollSize();
            }
        }

        private readonly Pen _tileBorder = new Pen(Brushes.Black, 1.0f);

        public float TileBorderWidth
        {
            get => _tileBorder.Width;
            set
            {
                _tileBorder.Width = value;
                UpdateAutoScrollSize();
            }
        }

        [Browsable(true)]
        public Color TileBorderColor
        {
            get => _tileBorder.Color;
            set
            {
                _tileBorder.Color = value;
                if (_tileBorder.Width > 0)
                {
                    Invalidate();
                }
            }
        }

        private const double _defaultOpacityValue = 0.5;
        private double _tileHighlightOpacity = _defaultOpacityValue;

        [Browsable(true)]
        [Description("Opacity value for highlight color. Allowed values 0-100% (default 50%).")]
        [TypeConverter(typeof(OpacityConverter))]
        [DefaultValue(_defaultOpacityValue)]
        public double TileHighLightOpacity
        {
            get => _tileHighlightOpacity;
            set
            {
                _tileHighlightOpacity = value;

                if (SelectedIndices.Count > 0)
                {
                    RedrawItems(SelectedIndices.ToList());
                }
            }
        }

        private Color _tileFocusColor = Color.DarkRed;

        /// <summary>
        /// Focused tile border and highlight color
        /// </summary>
        [Browsable(true)]
        public Color TileFocusColor
        {
            get => _tileFocusColor;
            set
            {
                _tileFocusColor = value;
                Invalidate();
            }
        }

        private Color _tileHighlightColor = SystemColors.Highlight;

        /// <summary>
        /// Selected tile highlight color
        /// </summary>
        [Browsable(true)]
        public Color TileHighlightColor
        {
            get => _tileHighlightColor;
            set
            {
                _tileHighlightColor = value;

                if (SelectedIndices.Count > 0)
                {
                    RedrawItems(SelectedIndices.ToList());
                }
            }
        }

        private Color _tileBackgroundColor = SystemColors.Window;
        /// <summary>
        /// Color of tile background
        /// </summary>
        [Browsable(true)]
        public Color TileBackgroundColor
        {
            get => _tileBackgroundColor;
            set
            {
                _tileBackgroundColor = value;
                Invalidate();
            }
        }

        public TileViewControl()
        {
            SetStyle(ControlStyles.UserMouse, true); // to make control focusable
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            ResizeRedraw = true;

            SizeChanged += (o, e) => UpdateAutoScrollSize();

            MouseDown += (o, e) =>
            {
                int idx = GetIndexAtLocation(e.Location);

                FocusIndex = idx;

                if (idx != -2 && e.Button == MouseButtons.Left) // no Tile at given location
                {
                    SelectIndex(idx);
                }
            };

            SelectedIndices.CollectionChanged += (o, e) =>
            {
                if (e.ItemsChanged.Count == 0)
                {
                    return;
                }

                RedrawItems(e.ItemsChanged);

                if (ItemSelectionChanged == null)
                {
                    return;
                }

                foreach (Delegate del in ItemSelectionChanged.GetInvocationList())
                {
                    del.DynamicInvoke(this,
                        new ListViewItemSelectionChangedEventArgs(null, e.ItemsChanged[0],
                            e.Action == IndicesCollection.NotifyCollectionChangedAction.Add));
                }
            };
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handling control keys so we can change focus with keys

            if (keyData == Keys.Home || keyData == Keys.End)
            {
                if (_focusIndex > -1)
                {
                    switch (keyData)
                    {
                        case Keys.Home:
                            ScrollToItem(0);
                            FocusIndex = GetVisibleIndices(Bounds)[0];
                            break;
                        case Keys.End:
                            var lastItem = VirtualListSize - 1;
                            ScrollToItem(lastItem);
                            FocusIndex = lastItem;
                            break;
                    }
                }
                else if (VirtualListSize > 0) // if there's no focus, focus on first item visible index
                {
                    FocusIndex = GetVisibleIndices(Bounds)[0];
                }
            }

            if (keyData == Keys.PageUp || keyData == Keys.PageDown)
            {
                if (_focusIndex > -1)
                {
                    int newFocusIndex = _focusIndex;
                    int visibleRows = GetVisibleIndices(Bounds).Count / _itemsPerRow;
                    switch (keyData)
                    {
                        case Keys.PageUp:
                            {
                                if (newFocusIndex - _itemsPerRow > -1)
                                {
                                    newFocusIndex -= _itemsPerRow * (visibleRows - 1);
                                }
                                break;
                            }
                        case Keys.PageDown:
                            {
                                if (newFocusIndex + _itemsPerRow < _virtualListSize)
                                {
                                    newFocusIndex += _itemsPerRow * (visibleRows - 1);
                                }
                                break;
                            }
                    }

                    FocusIndex = newFocusIndex < VirtualListSize ? newFocusIndex : VirtualListSize - 1;
                }
                else if (VirtualListSize > 0) // if there's no focus, focus on first item visible index
                {
                    FocusIndex = GetVisibleIndices(Bounds)[0];
                }
            }
            else if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
            {
                if (_focusIndex > -1)
                {
                    int newFocusIndex = _focusIndex;
                    switch (keyData)
                    {
                        case Keys.Left:
                            if (newFocusIndex - 1 > -1)
                            {
                                --newFocusIndex;
                            }
                            break;
                        case Keys.Right:
                            if (newFocusIndex + 1 < _virtualListSize)
                            {
                                ++newFocusIndex;
                            }
                            break;
                        case Keys.Up:
                            if (newFocusIndex - _itemsPerRow > -1)
                            {
                                newFocusIndex -= _itemsPerRow;
                            }
                            break;
                        case Keys.Down:
                            if (newFocusIndex + _itemsPerRow < _virtualListSize)
                            {
                                newFocusIndex += _itemsPerRow;
                            }
                            break;
                    }

                    FocusIndex = newFocusIndex;
                }
                else if (VirtualListSize > 0) // if there's no focus, focus on first item visible index
                {
                    FocusIndex = GetVisibleIndices(Bounds)[0];
                }
            }
            else
            {
                if (keyData == Keys.Space || ((keyData & Keys.Space) == Keys.Space && ((keyData & Keys.Shift) == Keys.Shift || (keyData & Keys.Control) == Keys.Control)))
                {
                    if (_focusIndex >= 0)
                    {
                        SelectIndex(_focusIndex);
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SelectIndex(int index)
        {
            switch (ModifierKeys)
            {
                case Keys.Control:
                    if (_multiSelect)
                    {
                        if (SelectedIndices.Contains(index))
                        {
                            SelectedIndices.Remove(index);
                        }
                        else
                        {
                            SelectedIndices.Add(index);
                        }
                    }
                    else
                    {
                        if (!SelectedIndices.Contains(index))
                        {
                            SelectedIndices.Clear();
                            SelectedIndices.Add(index);
                        }
                    }

                    break;
                default:
                    if (!SelectedIndices.Contains(index))
                    {
                        SelectedIndices.Clear();
                        SelectedIndices.Add(index);
                    }

                    break;
            }
        }

        /// <summary>
        /// Redraw Tile with given index
        /// </summary>
        /// <param name="index"></param>
        public void RedrawItem(int index)
        {
            Point p = new Point(index % _itemsPerRow * TotalTileSize.Width + AutoScrollPosition.X, index / _itemsPerRow * TotalTileSize.Height + AutoScrollPosition.Y);
            Invalidate(new Rectangle(p, TotalTileSize));
        }

        public void RedrawItems(List<int> indices)
        {
            indices.ForEach(RedrawItem);
        }

        private Point GetItemLocation(int index)
        {
            return new Point(index % _itemsPerRow * TotalTileSize.Width, index / _itemsPerRow * TotalTileSize.Height);
        }

        /// <summary>
        /// Find index of Tile for given location.
        /// </summary>
        /// <param name="location"></param>
        /// <returns>Index of Tile in location. Return -2 if no Tile at given location.</returns>
        public int GetIndexAtLocation(Point location)
        {
            int line = (location.Y + -AutoScrollPosition.Y) / TotalTileSize.Height;
            int row = (location.X + -AutoScrollPosition.X) / TotalTileSize.Width;
            if (DivUp(location.X, TotalTileSize.Width) > _itemsPerRow)
            {
                return -2;
            }

            int r = (line * _itemsPerRow + row);
            if (r >= VirtualListSize)
            {
                return -2;
            }

            return r;
        }

        /// <summary>
        /// Get list of indices of Tiles for given rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>List of indices. Indices may have discontinuities.</returns>
        private List<int> GetVisibleIndices(RectangleF rect)
        {
            int line = (int)rect.Y / TotalTileSize.Height;
            int row = (int)rect.X / TotalTileSize.Width;

            //int rem;
            //Math.DivRem(-this.AutoScrollPosition.Y, m_TotalTileSize.Height, out rem);
            //rem = (-this.AutoScrollPosition.Y + m_TotalTileSize.Height) - m_TotalTileSize.Height * line;

            int lines = ((int)(rect.Y + rect.Height - 1) / TotalTileSize.Height) - ((int)rect.Y / TotalTileSize.Height) + 1;
            int rows = (int)rect.Width / TotalTileSize.Width;

            if (lines == 0)
            {
                lines = 1; // Draw at last one item in visible area.
            }

            if (rows == 0)
            {
                rows = 1;
            }

            List<int> indices = new List<int>();
            for (int l = line; l < line + lines; l++)
            {
                for (int r = row; r < row + rows; r++)
                {
                    var i = l * _itemsPerRow + r;
                    if (i >= VirtualListSize)
                    {
                        break;
                    }

                    if (indices.Contains(i))
                    {
                        continue;
                    }

                    indices.Add(i);
                }
            }

            return indices;
        }

        /// <summary>
        /// Backwards compatibility with ListView DrawItem event. Warning: Have a high chance to be changed in future.
        /// </summary>
        public event EventHandler<DrawTileListItemEventArgs> DrawItem;

        public event EventHandler<CacheItemEventArgs> CacheItems;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);

            List<int> visibleIndices = GetVisibleIndices(e.Graphics.VisibleClipBounds);

            if (CacheItems != null)
            {
                Rectangle rect = Bounds;
                rect.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);

                List<int> allIndices = GetVisibleIndices(rect);
                if (allIndices.Count > 0 && (_cachedIndices.Count == 0 || _cachedIndices.Count < allIndices.Count || (_cachedIndices[0] > allIndices[0] || _cachedIndices[_cachedIndices.Count - 1] < allIndices[allIndices.Count - 1])))
                {
                    CacheItemEventArgs ne = new CacheItemEventArgs(allIndices);

                    CacheItems?.Invoke(this, ne);

                    if (ne.Success)
                    {
                        _cachedIndices = allIndices;
                    }
                }
            }

            foreach (int i in visibleIndices)
            {
                if (i >= VirtualListSize)
                {
                    throw new ArgumentException("Index out of range", nameof(i));
                }

                Size single = new Size(1, 1);
                Size borderSize = new Size((int)_tileBorder.Width * 2, (int)_tileBorder.Width * 2); // border size

                Point itemLocation = GetItemLocation(i);

                Point marginPoint = new Point(itemLocation.X + _tileMargin.Left, itemLocation.Y + _tileMargin.Top); // margin point
                Point borderPoint = new Point(marginPoint.X + (int)_tileBorder.Width, marginPoint.Y + (int)_tileBorder.Width); // bordered point
                Point paddedPoint = new Point(borderPoint.X + _tilePadding.Left, borderPoint.Y + _tilePadding.Top); // padded point

                Rectangle marginRec = new Rectangle(marginPoint, _tileSize + _tilePadding.Size + borderSize);
                Rectangle borderRec = new Rectangle(borderPoint, _tileSize + _tilePadding.Size);
                Rectangle paddedRec = new Rectangle(paddedPoint, _tileSize);

                if (DrawItem != null)
                {
                    DrawItem(this, new DrawTileListItemEventArgs(e.Graphics, Font, borderRec, i, _focusIndex == i ? DrawItemState.Selected : DrawItemState.None));
                }
                else
                {
                    using (var brush = new SolidBrush(_tileBackgroundColor))
                    {
                        e.Graphics.FillRectangle(brush, paddedRec); // tile itself
                    }

                    // TODO: Add text drawing?
                }

                if (SelectedIndices.Contains(i))
                {
                    using (var brush = new SolidBrush(Color.FromArgb((int)(TileHighLightOpacity * 255), _tileHighlightColor)))
                    {
                        e.Graphics.FillRectangle(brush, marginRec);
                    }
                }

                if (_tileBorder.Width > 0)
                {
                    e.Graphics.DrawRectangle(_tileBorder, new Rectangle(marginRec.Location, marginRec.Size - single));
                }

                // not sure yet on should it be in DrawItem or here...
                if (_focusIndex == i)
                {
                    Rectangle focusRec = new Rectangle(marginPoint + single, _tileSize + _tilePadding.Size - single);

                    if (!SelectedIndices.Contains(_focusIndex))
                    {
                        using (var brush = new SolidBrush(Color.FromArgb(32, TileFocusColor)))
                        {
                            e.Graphics.FillRectangle(brush, marginRec);
                        }
                    }

                    using (var brush = new SolidBrush(TileFocusColor))
                    using (var pen = new Pen(brush, 1f))
                    {
                        e.Graphics.DrawRectangle(pen, focusRec);
                    }
                }
            }
        }

        public class ListViewFocusedItemSelectionChangedEventArgs : EventArgs
        {
            public int FocusedItemIndex { get; }
            public bool IsFocused { get; }

            public ListViewFocusedItemSelectionChangedEventArgs(int focusedItemIndex, bool isFocused)
            {
                FocusedItemIndex = focusedItemIndex;
                IsFocused = isFocused;
            }
        }

        public class DrawTileListItemEventArgs : DrawItemEventArgs
        {
            public DrawTileListItemEventArgs(Graphics graphics, Font font, Rectangle rect, int index,
                DrawItemState state) : base(graphics, font, rect, index, state)
            {
            }
        }
    }
}