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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>One style in the gallery, with the piece that stands for it.</summary>
    /// <remarks>An item that is not a house style (a floor pattern) carries its object in Tag and has no Style.</remarks>
    internal sealed record HouseGalleryItem(HouseStyle Style, string Label, int IconId, object Tag = null);

    /// <summary>A titled run of styles, e.g. one wall material.</summary>
    internal sealed record HouseGallerySection(string Title, IReadOnlyList<HouseGalleryItem> Items);

    /// <summary>
    /// House styles as pictures: sections with a heading, each style a cell showing one of its
    /// pieces over its name. Only the visible cells are painted.
    /// </summary>
    internal sealed class TileBrowserHouseGallery : Control
    {
        private const int _cellWidth = 84;
        private const int _imageHeight = 50;
        private const int _textHeight = 28;
        private const int _cellHeight = _imageHeight + _textHeight + 6;
        private const int _headerHeight = 22;

        /// <summary>Where one thing is drawn: a section heading (Item null) or a style cell.</summary>
        private readonly record struct Slot(Rectangle Bounds, string Header, HouseGalleryItem Item);

        private readonly VScrollBar _scroll;
        private readonly ToolTip _toolTip = new ToolTip();
        private readonly List<Slot> _slots = new List<Slot>();
        private IReadOnlyList<HouseGallerySection> _sections = Array.Empty<HouseGallerySection>();
        private HouseStyle _selected;
        private object _selectedTag;
        private int _hover = -1;
        private int _contentHeight;

        public TileBrowserHouseGallery()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            BackColor = SystemColors.Window;
            _scroll = new VScrollBar { Dock = DockStyle.Right, SmallChange = 32 };
            _scroll.ValueChanged += (_, _) => Invalidate();
            Controls.Add(_scroll);
        }

        /// <summary>A style was clicked or chosen with Enter (also when it already was the selected one).</summary>
        public event Action<HouseStyle> StyleChosen;

        /// <summary>An item with a Tag was clicked or chosen with Enter.</summary>
        public event Action<object> TagChosen;

        /// <summary>The selected tagged item, when the gallery shows those.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedTag
        {
            get => _selectedTag;
            set
            {
                _selectedTag = value;
                Invalidate();
            }
        }

        private bool IsSelected(HouseGalleryItem item)
        {
            return item.Tag != null ? item.Tag == _selectedTag : item.Style != null && item.Style == _selected;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EmptyText { get; set; } = "No styles";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HouseStyle Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Invalidate();
            }
        }

        public int Count { get; private set; }

        public void SetSections(IReadOnlyList<HouseGallerySection> sections)
        {
            _sections = sections ?? Array.Empty<HouseGallerySection>();
            _hover = -1;
            _scroll.Value = 0;
            Relayout();
        }

        /// <summary>The first style shown, or null.</summary>
        public HouseStyle First()
        {
            foreach (Slot slot in _slots)
            {
                if (slot.Item != null)
                {
                    return slot.Item.Style;
                }
            }

            return null;
        }

        public void ScrollToSelected()
        {
            foreach (Slot slot in _slots)
            {
                if (slot.Item != null && IsSelected(slot.Item))
                {
                    int view = ClientSize.Height;
                    if (slot.Bounds.Top < _scroll.Value || slot.Bounds.Bottom > _scroll.Value + view)
                    {
                        SetScroll(slot.Bounds.Top - _headerHeight);
                    }

                    return;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Relayout();
        }

        private void Relayout()
        {
            _slots.Clear();
            Count = 0;
            int width = Math.Max(_cellWidth, ClientSize.Width - _scroll.Width);
            int columns = Math.Max(1, width / _cellWidth);

            // Cells share the spare width, so the row always fills the panel.
            int cell = width / columns;
            int y = 2;
            foreach (HouseGallerySection section in _sections)
            {
                if (section.Items.Count == 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(section.Title))
                {
                    _slots.Add(new Slot(new Rectangle(0, y, width, _headerHeight), section.Title, null));
                    y += _headerHeight;
                }

                for (int i = 0; i < section.Items.Count; i++)
                {
                    int column = i % columns;
                    int row = i / columns;
                    _slots.Add(new Slot(new Rectangle(column * cell, y + (row * _cellHeight), cell, _cellHeight), null, section.Items[i]));
                    Count++;
                }

                y += (((section.Items.Count - 1) / columns) + 1) * _cellHeight;
                y += 4;
            }

            _contentHeight = y;
            int view = Math.Max(1, ClientSize.Height);
            _scroll.Enabled = _contentHeight > view;
            _scroll.Maximum = Math.Max(0, _contentHeight - 1);
            _scroll.LargeChange = view;
            SetScroll(_scroll.Value);
            Invalidate();
        }

        private void SetScroll(int value)
        {
            int max = Math.Max(0, _contentHeight - Math.Max(1, ClientSize.Height));
            _scroll.Value = Math.Clamp(value, 0, max);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (Count == 0)
            {
                TextRenderer.DrawText(g, EmptyText, Font, ClientRectangle, SystemColors.GrayText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
                return;
            }

            int top = _scroll.Value;
            int bottom = top + ClientSize.Height;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            using var headerFont = new Font(Font, FontStyle.Bold);

            for (int i = 0; i < _slots.Count; i++)
            {
                Slot slot = _slots[i];
                if (slot.Bounds.Bottom < top || slot.Bounds.Top > bottom)
                {
                    continue;
                }

                Rectangle bounds = slot.Bounds;
                bounds.Offset(0, -top);
                if (slot.Item == null)
                {
                    TextRenderer.DrawText(g, slot.Header, headerFont, new Rectangle(bounds.X + 4, bounds.Y, bounds.Width - 8, bounds.Height),
                        SystemColors.GrayText, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    using var line = new Pen(SystemColors.ControlLight);
                    g.DrawLine(line, bounds.X + 4, bounds.Bottom - 2, bounds.Right - 4, bounds.Bottom - 2);
                    continue;
                }

                bool selected = IsSelected(slot.Item);
                Rectangle inner = Rectangle.Inflate(bounds, -2, -2);
                if (selected)
                {
                    using var fill = new SolidBrush(Color.FromArgb(70, SystemColors.Highlight));
                    g.FillRectangle(fill, inner);
                    using var border = new Pen(SystemColors.Highlight, 2);
                    g.DrawRectangle(border, inner);
                }
                else if (i == _hover)
                {
                    using var fill = new SolidBrush(Color.FromArgb(35, SystemColors.Highlight));
                    g.FillRectangle(fill, inner);
                }

                DrawArt(g, slot.Item.IconId, new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, _imageHeight));
                TextRenderer.DrawText(g, slot.Item.Label, Font,
                    new Rectangle(bounds.X + 2, bounds.Y + _imageHeight + 4, bounds.Width - 4, _textHeight), ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
            }
        }

        /// <summary>The art bottom-centred in the box, scaled down when too large, never up.</summary>
        private static void DrawArt(Graphics g, int id, Rectangle box)
        {
            Bitmap art = id > 0 ? Art.GetStatic(id) : null;
            if (art == null)
            {
                return;
            }

            float scale = Math.Min(1f, Math.Min((float)box.Width / art.Width, (float)box.Height / art.Height));
            int width = Math.Max(1, (int)(art.Width * scale));
            int height = Math.Max(1, (int)(art.Height * scale));
            g.DrawImage(art, box.X + ((box.Width - width) / 2), box.Bottom - height, width, height);
        }

        private int HitTest(Point point)
        {
            int y = point.Y + _scroll.Value;
            for (int i = 0; i < _slots.Count; i++)
            {
                Slot slot = _slots[i];
                if (slot.Item != null && slot.Bounds.Contains(point.X, y))
                {
                    return i;
                }
            }

            return -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int hover = HitTest(e.Location);
            if (hover == _hover)
            {
                return;
            }

            _hover = hover;
            Invalidate();
            if (hover >= 0)
            {
                HouseGalleryItem item = _slots[hover].Item;
                _toolTip.SetToolTip(this, item.Style != null
                    ? $"{item.Style.Name}\n{item.Style.Group}\n{item.Style.Slots.Count} pieces"
                    : item.Label);
            }
            else
            {
                _toolTip.SetToolTip(this, string.Empty);
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (_hover != -1)
            {
                _hover = -1;
                Invalidate();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int index = HitTest(e.Location);
            if (e.Button == MouseButtons.Left && index >= 0)
            {
                Choose(_slots[index].Item);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            SetScroll(_scroll.Value - (Math.Sign(e.Delta) * _cellHeight));
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // The styles in reading order; arrows walk them, a row at a time for up and down.
            var cells = new List<int>();
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Item != null)
                {
                    cells.Add(i);
                }
            }

            if (cells.Count == 0)
            {
                return;
            }

            int current = cells.FindIndex(i => IsSelected(_slots[i].Item));
            int columns = Math.Max(1, Math.Max(_cellWidth, ClientSize.Width - _scroll.Width) / _cellWidth);
            int next = e.KeyCode switch
            {
                Keys.Right => current + 1,
                Keys.Left => current - 1,
                Keys.Down => current + columns,
                Keys.Up => current - columns,
                Keys.Home => 0,
                Keys.End => cells.Count - 1,
                Keys.Enter => current,
                _ => int.MinValue
            };

            if (next == int.MinValue)
            {
                return;
            }

            e.Handled = true;
            next = Math.Clamp(next, 0, cells.Count - 1);
            Choose(_slots[cells[next]].Item);
            ScrollToSelected();
        }

        private void Choose(HouseGalleryItem item)
        {
            if (item.Tag != null)
            {
                _selectedTag = item.Tag;
                Invalidate();
                TagChosen?.Invoke(item.Tag);
                return;
            }

            _selected = item.Style;
            Invalidate();
            StyleChosen?.Invoke(item.Style);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _toolTip.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
