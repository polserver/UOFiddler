/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.UserControls
{
    /// <summary>
    /// A tab control for plugins: the wheel over the tab strip switches tabs, and in dark mode the
    /// control paints the strip itself. .NET's dark mode does not reach owner drawn tabs (the native
    /// control paints them and the strip light), so in dark mode owner drawn tabs get
    /// <see cref="TabControl.DrawItem"/> raised from here and other tabs are drawn plainly. The native
    /// control still sizes the tabs and handles the mouse. In light mode it is a native tab control.
    /// </summary>
    public class ThemedTabControl : TabControl
    {
        private static readonly Color _darkStrip = Color.FromArgb(32, 32, 32);
        private static readonly Color _darkTab = Color.FromArgb(43, 43, 43);
        private static readonly Color _darkSelectedTab = Color.FromArgb(60, 60, 60);
        private static readonly Color _darkBorder = Color.FromArgb(75, 75, 75);

        public ThemedTabControl()
        {
            if (Dark)
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
            }
        }

        /// <summary>The font handle given to the native control while it is user painted; ours to delete.</summary>
        private IntPtr _fontHandle;

        private static bool Dark => Options.DarkMode;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetNativeFont();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            SetNativeFont();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // After the window is gone, so the font is no longer selected into it.
            if (_fontHandle != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(_fontHandle);
                _fontHandle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// WinForms gives a user painted control's window no font, so the native control would size
        /// the tabs with the system font, not the one the text is drawn in.
        /// </summary>
        private void SetNativeFont()
        {
            if (!IsHandleCreated || !GetStyle(ControlStyles.UserPaint))
            {
                return;
            }

            IntPtr previous = _fontHandle;
            _fontHandle = Font.ToHfont();
            NativeMethods.SendMessage(Handle, NativeMethods.WM_SETFONT, _fontHandle, (IntPtr)1);
            if (previous != IntPtr.Zero)
            {
                NativeMethods.DeleteObject(previous);
            }
        }

        private static class NativeMethods
        {
            public const int WM_SETFONT = 0x0030;

            [DllImport("user32.dll")]
            public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool DeleteObject(IntPtr hObject);
        }

        public Color TabBackColor(bool selected) =>
            Dark ? (selected ? _darkSelectedTab : _darkTab) : (selected ? SystemColors.Window : SystemColors.Control);

        public Color TabTextColor(bool selected) =>
            Dark ? (selected ? Color.FromArgb(240, 240, 240) : Color.FromArgb(200, 200, 200)) : (selected ? SystemColors.WindowText : SystemColors.ControlText);

        /// <summary>A glyph drawn on a tab (a close button's cross) when the mouse is not on it.</summary>
        public Color GlyphColor(bool selected) =>
            Dark ? (selected ? Color.FromArgb(220, 220, 220) : Color.FromArgb(150, 150, 150)) : (selected ? SystemColors.WindowText : SystemColors.GrayText);

        /// <summary>
        /// The wheel over the tab strip switches tabs: down (towards you) to the next tab, up to the
        /// previous one. The wheel over a page, passed up from its content, is left alone.
        /// </summary>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (TabCount < 2 || e.Delta == 0 || DisplayRectangle.Contains(e.Location))
            {
                return;
            }

            int index = Math.Clamp(SelectedIndex + (e.Delta < 0 ? 1 : -1), 0, TabCount - 1);
            if (index == SelectedIndex)
            {
                return;
            }

            SelectedIndex = index;
            if (e is HandledMouseEventArgs handled)
            {
                handled.Handled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Only reached in dark mode, where UserPaint is set.
            using (var strip = new SolidBrush(_darkStrip))
            {
                e.Graphics.FillRectangle(strip, ClientRectangle);
            }

            using var border = new Pen(_darkBorder);
            int tabsBottom = 0;
            for (int i = 0; i < TabCount; i++)
            {
                Rectangle tab = GetTabRect(i);
                tabsBottom = Math.Max(tabsBottom, tab.Bottom);
                if (!tab.IntersectsWith(e.ClipRectangle))
                {
                    continue;
                }

                bool selected = i == SelectedIndex;
                if (DrawMode == TabDrawMode.OwnerDrawFixed)
                {
                    OnDrawItem(new DrawItemEventArgs(e.Graphics, Font, tab, i, selected ? DrawItemState.Selected : DrawItemState.None));
                }
                else
                {
                    DrawTab(e.Graphics, tab, i, selected);
                }

                e.Graphics.DrawLine(border, tab.Left, tab.Top, tab.Right - 1, tab.Top);
                e.Graphics.DrawLine(border, tab.Left, tab.Top, tab.Left, tab.Bottom - 1);
                e.Graphics.DrawLine(border, tab.Right - 1, tab.Top, tab.Right - 1, tab.Bottom - 1);
            }

            if (DisplayRectangle.Height > 0)
            {
                // A frame around the pages, its top edge along the bottom of the tabs.
                e.Graphics.DrawRectangle(border, 0, tabsBottom, Width - 1, ClientRectangle.Bottom - tabsBottom - 1);
            }
            else
            {
                // A strip with no page area: the line along its bottom.
                int y = ClientRectangle.Bottom - 1;
                e.Graphics.DrawLine(border, 0, y, Width, y);
            }
        }

        private void DrawTab(Graphics graphics, Rectangle bounds, int index, bool selected)
        {
            using (var back = new SolidBrush(TabBackColor(selected)))
            {
                graphics.FillRectangle(back, bounds);
            }

            TextRenderer.DrawText(graphics, TabPages[index].Text, Font, bounds, TabTextColor(selected),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }
    }
}
