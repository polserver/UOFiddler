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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.UserControls
{
    /// <summary>
    /// A hue's 32 colors as cells, with a selected range. Click picks a cell, drag or Shift+click
    /// picks a range, the arrow keys move it (Shift extends) and Ctrl+A picks every cell.
    /// Optionally draws the hue's original colors as a thin strip underneath.
    /// </summary>
    [DesignerCategory("Code")]
    public class HueStripControl : Control
    {
        private const int _originalHeight = 8;
        private const int _gap = 2;
        private const int _labelHeight = 14;

        private ushort[] _colors = new ushort[HueRamp.ColorCount];
        private ushort[] _original;
        private int _anchor;
        private int _caret;
        private bool _dragging;
        private int _hoverCell = -1;
        private readonly ToolTip _toolTip = new() { InitialDelay = 300, ReshowDelay = 100 };

        public HueStripControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                     | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
        }

        /// <summary>Raised when the anchor or caret moves.</summary>
        public event EventHandler SelectionChanged;

        /// <summary>Raised with the cell that was double-clicked.</summary>
        public event EventHandler<int> CellDoubleClicked;

        /// <summary>The cell the selection started from; the color editor edits this one.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int AnchorCell => _anchor;

        /// <summary>The cell the selection was extended to.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Caret => _caret;

        [Browsable(false)]
        public int SelectionStart => Math.Min(_anchor, _caret);

        [Browsable(false)]
        public int SelectionEnd => Math.Max(_anchor, _caret);

        [Browsable(false)]
        public bool HasRange => _anchor != _caret;

        public void SetColors(ushort[] colors)
        {
            _colors = colors ?? new ushort[HueRamp.ColorCount];
            Invalidate();
        }

        /// <summary>The colors drawn in the thin comparison strip; null hides it.</summary>
        public void SetOriginal(ushort[] original)
        {
            _original = original == null ? null : (ushort[])original.Clone();
            Invalidate();
        }

        public void SelectRange(int anchor, int caret)
        {
            anchor = Math.Clamp(anchor, 0, _colors.Length - 1);
            caret = Math.Clamp(caret, 0, _colors.Length - 1);
            if (anchor == _anchor && caret == _caret)
            {
                return;
            }

            _anchor = anchor;
            _caret = caret;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public int CellAt(int x)
        {
            Rectangle area = CellArea;
            if (area.Width <= 0)
            {
                return -1;
            }

            int index = (int)((x - area.Left) / (area.Width / (double)_colors.Length));
            return Math.Clamp(index, 0, _colors.Length - 1);
        }

        private Rectangle CellArea
        {
            get
            {
                int bottom = _labelHeight + (_original != null ? _originalHeight + _gap : 0);
                return new Rectangle(1, 1, Math.Max(0, Width - 2), Math.Max(0, Height - bottom - 2));
            }
        }

        private Rectangle CellRect(int index, Rectangle area)
        {
            double size = area.Width / (double)_colors.Length;
            int left = area.Left + (int)Math.Round(index * size);
            int right = area.Left + (int)Math.Round((index + 1) * size);
            return new Rectangle(left, area.Top, right - left, area.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            Rectangle area = CellArea;
            if (area.Width <= 0 || area.Height <= 0)
            {
                return;
            }

            for (int i = 0; i < _colors.Length; i++)
            {
                Rectangle cell = CellRect(i, area);
                using var brush = new SolidBrush(HueHelpers.HueToColor(_colors[i]));
                g.FillRectangle(brush, cell);

                if (_original != null)
                {
                    var originalCell = new Rectangle(cell.X, area.Bottom + _gap, cell.Width, _originalHeight);
                    using var originalBrush = new SolidBrush(HueHelpers.HueToColor(_original[i]));
                    g.FillRectangle(originalBrush, originalCell);
                }
            }

            // Selection: dim everything outside it, then outline it.
            Rectangle first = CellRect(SelectionStart, area);
            Rectangle last = CellRect(SelectionEnd, area);
            var selection = Rectangle.FromLTRB(first.Left, area.Top, last.Right, area.Bottom);

            using (var dim = new SolidBrush(Color.FromArgb(HasRange ? 90 : 0, BackColor)))
            {
                if (HasRange)
                {
                    g.FillRectangle(dim, Rectangle.FromLTRB(area.Left, area.Top, selection.Left, area.Bottom));
                    g.FillRectangle(dim, Rectangle.FromLTRB(selection.Right, area.Top, area.Right, area.Bottom));
                }
            }

            Color accent = Focused ? SystemColors.Highlight : SystemColors.GrayText;
            using (var outline = new Pen(accent, 2) { Alignment = PenAlignment.Inset })
            {
                g.DrawRectangle(outline, selection);
            }

            // The anchor (the cell the color editor edits) gets a contrasting inner frame.
            Rectangle anchorCell = CellRect(_anchor, area);
            anchorCell.Inflate(-3, -3);
            Color anchorColor = HueHelpers.HueToColor(_colors[_anchor]);
            Color contrast = anchorColor.GetBrightness() > 0.5f ? Color.Black : Color.White;
            using (var anchorPen = new Pen(contrast, 1) { DashStyle = DashStyle.Dot })
            {
                g.DrawRectangle(anchorPen, anchorCell);
            }

            // Index labels every 4 cells, plus the last.
            int labelTop = Height - _labelHeight;
            using var labelBrush = new SolidBrush(ForeColor);
            using var font = new Font(Font.FontFamily, 7f);
            for (int i = 0; i < _colors.Length; i++)
            {
                if (i % 4 != 0 && i != _colors.Length - 1)
                {
                    continue;
                }

                Rectangle cell = CellRect(i, area);
                string text = i.ToString();
                SizeF size = g.MeasureString(text, font);
                float x = Math.Clamp(cell.Left + ((cell.Width - size.Width) / 2), 0, Width - size.Width);
                g.DrawString(text, font, labelBrush, x, labelTop);
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int index = CellAt(e.X);
            if (index < 0)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                if ((ModifierKeys & Keys.Shift) != 0)
                {
                    SelectRange(_anchor, index);
                }
                else
                {
                    SelectRange(index, index);
                }

                _dragging = true;
            }
            else if (e.Button == MouseButtons.Right && (index < SelectionStart || index > SelectionEnd))
            {
                // Right-click outside the selection picks that cell before the menu opens.
                SelectRange(index, index);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int index = CellAt(e.X);
            if (_dragging && index >= 0)
            {
                SelectRange(_anchor, index);
            }

            if (index != _hoverCell)
            {
                _hoverCell = index;
                _toolTip.SetToolTip(this, index >= 0 ? $"{index}: {HueColorText.Describe(_colors[index])}" : null);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverCell = -1;
            _toolTip.SetToolTip(this, null);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (e.Button == MouseButtons.Left)
            {
                int index = CellAt(e.X);
                if (index >= 0)
                {
                    CellDoubleClicked?.Invoke(this, index);
                }
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Home or Keys.End || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int last = _colors.Length - 1;
            int target = e.KeyCode switch
            {
                Keys.Left => _caret - 1,
                Keys.Right => _caret + 1,
                Keys.Home => 0,
                Keys.End => last,
                _ => -2,
            };

            if (e.Control && e.KeyCode == Keys.A)
            {
                SelectRange(0, last);
                e.Handled = true;
                return;
            }

            if (target == -2)
            {
                return;
            }

            target = Math.Clamp(target, 0, last);
            if (e.Shift)
            {
                SelectRange(_anchor, target);
            }
            else
            {
                SelectRange(target, target);
            }

            e.Handled = true;
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
