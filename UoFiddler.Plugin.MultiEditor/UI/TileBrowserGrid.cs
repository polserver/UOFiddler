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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// A virtualised, owner-drawn grid of tile thumbnails. Only visible cells are painted, and
    /// mouse moves repaint just the two cells whose hover state changed. Drawing and hit-testing
    /// share <see cref="CellBounds"/>/<see cref="HitTest"/>, so they use the same pitch.
    /// </summary>
    internal sealed class TileBrowserGrid : Control
    {
        public const int MinCellSize = 32;
        public const int MaxCellSize = 128;
        private const int _padding = 3;
        private const int _captionHeight = 12;
        private const int _tipMaxArt = 220;
        private const int _wmContextMenu = 0x007B;

        private readonly VScrollBar _scroll;
        private readonly TileBrowserThumbnails _thumbnails;
        private readonly ToolTip _toolTip;
        private readonly Timer _tipTimer;
        private readonly Font _captionFont;
        private readonly SolidBrush _currentBrush;
        private readonly SolidBrush _hoverBrush;
        private readonly Pen _currentPen;
        private readonly Pen _gridPen;

        private IReadOnlyList<int> _items = Array.Empty<int>();
        private int _cellSize = 56;
        private int _hoverIndex = -1;
        private int _focusIndex = -1;
        private int _currentId = -1;
        private int _currentIndex = -1;
        private int _wheelRemainder;
        private int _layoutColumns = 1;
        private bool _showIds;
        private int _tipId = -1;
        private string _tipText;

        public TileBrowserGrid(TileBrowserThumbnails thumbnails)
        {
            _thumbnails = thumbnails;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            DoubleBuffered = true;
            TabStop = true;
            BackColor = SystemColors.Window;

            _scroll = new VScrollBar { Dock = DockStyle.Right, Visible = false, TabStop = false };
            _scroll.ValueChanged += (_, _) => OnScrolled();
            Controls.Add(_scroll);

            _toolTip = new ToolTip { OwnerDraw = true, UseAnimation = false, UseFading = false };
            _toolTip.Popup += ToolTip_Popup;
            _toolTip.Draw += ToolTip_Draw;
            _tipTimer = new Timer { Interval = 450 };
            _tipTimer.Tick += TipTimer_Tick;

            _captionFont = new Font(Font.FontFamily, 7f);
            _currentBrush = new SolidBrush(Color.FromArgb(90, SystemColors.Highlight));
            _hoverBrush = new SolidBrush(Color.FromArgb(40, SystemColors.Highlight));
            _currentPen = new Pen(SystemColors.Highlight, 2f) { Alignment = System.Drawing.Drawing2D.PenAlignment.Inset };
            _gridPen = new Pen(SystemColors.ControlLight);
        }

        /// <summary>A tile was clicked or Enter was pressed on it.</summary>
        public event Action<TileBrowserGrid, int> TileActivated;

        /// <summary>Right-click or the menu key on a tile; the location is in grid client coordinates.</summary>
        public event Action<TileBrowserGrid, int, Point> TileContextMenuRequested;

        /// <summary>Ctrl+wheel changed the cell size, so the owner can resize its other grids.</summary>
        public event Action<int> CellSizeChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CellSize
        {
            get => _cellSize;
            set
            {
                int size = Math.Clamp(value, MinCellSize, MaxCellSize);
                if (size == _cellSize)
                {
                    return;
                }

                int first = FirstVisibleIndex();
                _cellSize = size;
                UpdateScrollRange();
                ScrollToIndexRow(first);
                Invalidate();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowIds
        {
            get => _showIds;
            set
            {
                if (_showIds != value)
                {
                    _showIds = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Tiles to flag with a corner mark (favorites).</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<int, bool> IsMarked { get; set; }

        /// <summary>Shown when the list is empty.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EmptyText { get; set; } = "No tiles";

        public int Count => _items.Count;

        public IReadOnlyList<int> Items => _items;

        /// <summary>The tile highlighted as the current draw tile, or -1.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentId
        {
            get => _currentId;
            set
            {
                if (value == _currentId)
                {
                    return;
                }

                InvalidateCell(_currentIndex);
                _currentId = value;
                _currentIndex = IndexOf(value);
                InvalidateCell(_currentIndex);
            }
        }

        /// <summary>The tile under the keyboard cursor, or -1.</summary>
        public int FocusedId => _focusIndex >= 0 && _focusIndex < _items.Count ? _items[_focusIndex] : -1;

        public void SetItems(IReadOnlyList<int> items, bool keepScroll = false)
        {
            int focusedId = FocusedId;
            _items = items ?? Array.Empty<int>();
            _currentIndex = IndexOf(_currentId);
            _focusIndex = IndexOf(focusedId);
            _hoverIndex = -1;
            HideTip();
            int top = keepScroll ? ScrollTop : 0;
            UpdateScrollRange();
            SetScrollTop(top);
            Invalidate();
        }

        /// <summary>Scrolls to the tile and puts the keyboard cursor on it. False when it is not listed.</summary>
        public bool ScrollToId(int id)
        {
            int index = IndexOf(id);
            if (index < 0)
            {
                return false;
            }

            MoveFocus(index);
            return true;
        }

        /// <summary>Puts the keyboard cursor on the first tile when none is set (for Down from the search box).</summary>
        public void FocusFirst()
        {
            if (_focusIndex < 0 && _items.Count > 0)
            {
                MoveFocus(_currentIndex >= 0 ? _currentIndex : 0);
            }
        }

        public int IndexOf(int id)
        {
            if (id < 0)
            {
                return -1;
            }

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] == id)
                {
                    return i;
                }
            }

            return -1;
        }

        // ---------------------------------------------------------------- layout

        private int ContentWidth => Math.Max(0, ClientSize.Width - (_scroll.Visible ? _scroll.Width : 0));

        private int Columns => Math.Max(1, ContentWidth / _cellSize);

        private int RowCount => (_items.Count + Columns - 1) / Columns;

        private int ScrollTop => _scroll.Visible ? _scroll.Value : 0;

        private int MaxScrollTop => Math.Max(0, (RowCount * _cellSize) - ClientSize.Height);

        public Rectangle CellBounds(int index)
        {
            int columns = Columns;
            return new Rectangle(index % columns * _cellSize, (index / columns * _cellSize) - ScrollTop, _cellSize, _cellSize);
        }

        /// <summary>The list index under a client point, or -1.</summary>
        public int HitTest(Point p)
        {
            int columns = Columns;
            if (p.X < 0 || p.Y < 0 || p.X >= columns * _cellSize || p.X >= ContentWidth || p.Y >= ClientSize.Height)
            {
                return -1;
            }

            int index = ((p.Y + ScrollTop) / _cellSize * columns) + (p.X / _cellSize);
            return index < _items.Count ? index : -1;
        }

        private void UpdateScrollRange()
        {
            if (ClientSize.Height <= 0)
            {
                return;
            }

            // Columns depend on whether the bar is shown, so decide with the full width first.
            int fullColumns = Math.Max(1, ClientSize.Width / _cellSize);
            bool needed = (_items.Count + fullColumns - 1) / fullColumns * _cellSize > ClientSize.Height;
            if (_scroll.Visible != needed)
            {
                _scroll.Visible = needed;
            }

            _layoutColumns = Columns;
            int total = RowCount * _cellSize;
            int top = Math.Min(_scroll.Value, MaxScrollTop);
            _scroll.Minimum = 0;
            _scroll.Maximum = Math.Max(0, total - 1);
            _scroll.LargeChange = Math.Max(1, ClientSize.Height);
            _scroll.SmallChange = _cellSize;
            _scroll.Value = Math.Max(0, top);
        }

        private void SetScrollTop(int top)
        {
            if (!_scroll.Visible)
            {
                return;
            }

            top = Math.Clamp(top, 0, MaxScrollTop);
            if (_scroll.Value != top)
            {
                _scroll.Value = top;
            }
        }

        private int FirstVisibleIndex() => ScrollTop / _cellSize * Columns;

        private void ScrollToIndexRow(int index)
        {
            if (index >= 0)
            {
                SetScrollTop(index / Columns * _cellSize);
            }
        }

        private void EnsureVisible(int index)
        {
            if (index < 0)
            {
                return;
            }

            int rowTop = index / Columns * _cellSize;
            if (rowTop < ScrollTop)
            {
                SetScrollTop(rowTop);
            }
            else if (rowTop + _cellSize > ScrollTop + ClientSize.Height)
            {
                SetScrollTop(rowTop + _cellSize - ClientSize.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            // Keeps the first visible row's tiles in view when the column count changes.
            int first = ScrollTop / _cellSize * _layoutColumns;
            base.OnResize(e);
            UpdateScrollRange();
            ScrollToIndexRow(first);
        }

        private void OnScrolled()
        {
            HideTip();
            Invalidate();
            // The content moved under a resting mouse.
            UpdateHover(PointToClient(MousePosition));
        }

        // ---------------------------------------------------------------- painting

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle clip = e.ClipRectangle;
            using (var background = new SolidBrush(BackColor))
            {
                g.FillRectangle(background, clip);
            }

            if (_items.Count == 0)
            {
                TextRenderer.DrawText(g, EmptyText, Font, new Rectangle(0, 0, ContentWidth, Math.Min(ClientSize.Height, 60)),
                    SystemColors.GrayText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }

            int columns = Columns;
            int top = ScrollTop;
            int firstRow = Math.Max(0, (clip.Top + top) / _cellSize);
            int lastRow = Math.Min(RowCount - 1, (clip.Bottom - 1 + top) / _cellSize);
            int firstCol = Math.Max(0, clip.Left / _cellSize);
            int lastCol = Math.Min(columns - 1, (clip.Right - 1) / _cellSize);

            for (int row = firstRow; row <= lastRow; row++)
            {
                for (int col = firstCol; col <= lastCol; col++)
                {
                    int index = (row * columns) + col;
                    if (index >= _items.Count)
                    {
                        break;
                    }

                    DrawCell(g, index, new Rectangle(col * _cellSize, (row * _cellSize) - top, _cellSize, _cellSize));
                }
            }
        }

        private void DrawCell(Graphics g, int index, Rectangle cell)
        {
            int id = _items[index];
            if (index == _currentIndex)
            {
                g.FillRectangle(_currentBrush, cell);
            }
            else if (index == _hoverIndex)
            {
                g.FillRectangle(_hoverBrush, cell);
            }

            g.DrawLine(_gridPen, cell.Right - 1, cell.Top, cell.Right - 1, cell.Bottom - 1);
            g.DrawLine(_gridPen, cell.Left, cell.Bottom - 1, cell.Right - 1, cell.Bottom - 1);

            int caption = _showIds ? _captionHeight : 0;
            var box = new Rectangle(cell.X + _padding, cell.Y + _padding, cell.Width - (2 * _padding) - 1, cell.Height - (2 * _padding) - 1 - caption);
            Bitmap thumb = box.Width > 0 && box.Height > 0 ? _thumbnails.Get(id, Math.Min(box.Width, box.Height)) : null;
            if (thumb != null)
            {
                // Bottom-centred, the way the tile stands on the ground.
                g.DrawImageUnscaled(thumb, box.X + ((box.Width - thumb.Width) / 2), box.Bottom - thumb.Height);
            }
            else
            {
                TextRenderer.DrawText(g, "?", Font, box, SystemColors.GrayText, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            if (caption > 0)
            {
                var captionBounds = new Rectangle(cell.X, box.Bottom, cell.Width - 1, caption + _padding);
                TextRenderer.DrawText(g, $"{id:X4}", _captionFont, captionBounds, SystemColors.GrayText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPadding);
            }

            if (IsMarked?.Invoke(id) == true)
            {
                Point[] corner =
                {
                    new Point(cell.Right - 10, cell.Top),
                    new Point(cell.Right - 1, cell.Top),
                    new Point(cell.Right - 1, cell.Top + 9)
                };
                g.FillPolygon(Brushes.Goldenrod, corner);
            }

            if (index == _currentIndex)
            {
                g.DrawRectangle(_currentPen, cell.X, cell.Y, cell.Width - 1, cell.Height - 1);
            }

            if (index == _focusIndex && Focused && ShowFocusCues)
            {
                ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(cell, -2, -2));
            }
        }

        private void InvalidateCell(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                Invalidate(CellBounds(index));
            }
        }

        // ---------------------------------------------------------------- mouse

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            UpdateHover(e.Location);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            SetHover(-1);
        }

        private void UpdateHover(Point p) => SetHover(ClientRectangle.Contains(p) ? HitTest(p) : -1);

        private void SetHover(int index)
        {
            if (index == _hoverIndex)
            {
                return;
            }

            InvalidateCell(_hoverIndex);
            _hoverIndex = index;
            InvalidateCell(_hoverIndex);

            HideTip();
            if (index >= 0)
            {
                _tipTimer.Start();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            HideTip();
            Focus();

            int index = HitTest(e.Location);
            if (index < 0)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                MoveFocus(index);
                TileActivated?.Invoke(this, _items[index]);
            }
            else if (e.Button == MouseButtons.Right)
            {
                MoveFocus(index);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e is HandledMouseEventArgs handled)
            {
                handled.Handled = true;
            }

            HideTip();
            _wheelRemainder += e.Delta;
            int notches = _wheelRemainder / 120;
            if (notches == 0)
            {
                return;
            }

            _wheelRemainder -= notches * 120;
            if ((ModifierKeys & Keys.Control) != 0)
            {
                int size = Math.Clamp(_cellSize + (notches * 8), MinCellSize, MaxCellSize);
                if (size != _cellSize)
                {
                    CellSize = size;
                    CellSizeChanged?.Invoke(size);
                }

                return;
            }

            int lines = SystemInformation.MouseWheelScrollLines;
            int rowsPerNotch = lines <= 0 ? Math.Max(1, ClientSize.Height / _cellSize) : Math.Clamp(lines, 1, 3);
            SetScrollTop(ScrollTop - (notches * rowsPerNotch * _cellSize));
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == _wmContextMenu)
            {
                long lParam = m.LParam.ToInt64();
                if (lParam == -1)
                {
                    // Menu key or Shift+F10: the menu belongs to the keyboard cursor.
                    if (_focusIndex >= 0 && _focusIndex < _items.Count)
                    {
                        EnsureVisible(_focusIndex);
                        Rectangle bounds = CellBounds(_focusIndex);
                        TileContextMenuRequested?.Invoke(this, _items[_focusIndex], new Point(bounds.Left + (bounds.Width / 2), bounds.Bottom));
                    }
                }
                else
                {
                    Point p = PointToClient(new Point(unchecked((short)lParam), unchecked((short)(lParam >> 16))));
                    int index = HitTest(p);
                    if (index >= 0)
                    {
                        TileContextMenuRequested?.Invoke(this, _items[index], p);
                    }
                }

                return;
            }

            base.WndProc(ref m);
        }

        // ---------------------------------------------------------------- keyboard

        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData & Keys.KeyCode)
            {
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                case Keys.Home:
                case Keys.End:
                case Keys.PageUp:
                case Keys.PageDown:
                case Keys.Enter:
                    return true;
                default:
                    return base.IsInputKey(keyData);
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled || _items.Count == 0)
            {
                return;
            }

            int columns = Columns;
            int pageRows = Math.Max(1, ClientSize.Height / _cellSize);
            int from = _focusIndex >= 0 ? _focusIndex : Math.Max(0, _currentIndex);
            int to;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    to = from - 1;
                    break;
                case Keys.Right:
                    to = from + 1;
                    break;
                case Keys.Up:
                    to = from - columns;
                    break;
                case Keys.Down:
                    to = from + columns;
                    break;
                case Keys.PageUp:
                    to = from - (pageRows * columns);
                    break;
                case Keys.PageDown:
                    to = from + (pageRows * columns);
                    break;
                case Keys.Home:
                    to = 0;
                    break;
                case Keys.End:
                    to = _items.Count - 1;
                    break;
                case Keys.Enter:
                    if (_focusIndex < 0)
                    {
                        MoveFocus(from);
                    }

                    TileActivated?.Invoke(this, _items[_focusIndex]);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    return;
                default:
                    return;
            }

            // A first key press only shows where the cursor is.
            MoveFocus(_focusIndex < 0 ? from : Math.Clamp(to, 0, _items.Count - 1));
            e.Handled = true;
        }

        private void MoveFocus(int index)
        {
            if (index < 0 || index >= _items.Count)
            {
                return;
            }

            InvalidateCell(_focusIndex);
            _focusIndex = index;
            EnsureVisible(index);
            InvalidateCell(_focusIndex);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            InvalidateCell(_focusIndex);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            InvalidateCell(_focusIndex);
        }

        // ---------------------------------------------------------------- tooltip with the full-size art

        private void TipTimer_Tick(object sender, EventArgs e)
        {
            _tipTimer.Stop();
            if (_hoverIndex < 0 || _hoverIndex >= _items.Count || !IsHandleCreated)
            {
                return;
            }

            _tipId = _items[_hoverIndex];
            _tipText = TileBrowserThumbnails.Describe(_tipId);
            Rectangle bounds = CellBounds(_hoverIndex);
            _toolTip.Show(_tipText, this, bounds.Left, bounds.Bottom + 2, 10000);
        }

        private void HideTip()
        {
            _tipTimer.Stop();
            if (_tipId >= 0)
            {
                _tipId = -1;
                _toolTip.Hide(this);
            }
        }

        private static Size TipArtSize(Bitmap art)
        {
            if (art == null)
            {
                return Size.Empty;
            }

            float scale = Math.Min(1f, Math.Min((float)_tipMaxArt / art.Width, (float)_tipMaxArt / art.Height));
            return new Size(Math.Max(1, (int)(art.Width * scale)), Math.Max(1, (int)(art.Height * scale)));
        }

        private void ToolTip_Popup(object sender, PopupEventArgs e)
        {
            Size art = _tipId >= 0 ? TipArtSize(Art.GetStatic(_tipId)) : Size.Empty;
            Size text = TextRenderer.MeasureText(_tipText ?? string.Empty, Font);
            int width = Math.Max(art.Width, text.Width) + 12;
            int height = (art.Height > 0 ? art.Height + 6 : 0) + text.Height + 10;
            e.ToolTipSize = new Size(width, height);
        }

        private void ToolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(SystemBrushes.Info, e.Bounds);
            g.DrawRectangle(SystemPens.InfoText, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);

            int y = e.Bounds.Top + 5;
            Bitmap art = _tipId >= 0 ? Art.GetStatic(_tipId) : null;
            if (art != null)
            {
                Size size = TipArtSize(art);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(art, new Rectangle(e.Bounds.Left + ((e.Bounds.Width - size.Width) / 2), y, size.Width, size.Height));
                y += size.Height + 6;
            }

            TextRenderer.DrawText(g, _tipText ?? string.Empty, Font,
                new Rectangle(e.Bounds.Left + 6, y, e.Bounds.Width - 12, e.Bounds.Bottom - y), SystemColors.InfoText,
                TextFormatFlags.Left | TextFormatFlags.Top);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _tipTimer.Dispose();
                _toolTip.Dispose();
                _captionFont.Dispose();
                _currentBrush.Dispose();
                _hoverBrush.Dispose();
                _currentPen.Dispose();
                _gridPen.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
