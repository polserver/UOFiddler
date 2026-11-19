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
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
        private void DocTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            Activate(_docTabs.SelectedIndex);
        }

        private void DocTabs_MouseUp(object sender, MouseEventArgs e)
        {
            // Middle click closes a tab, like a browser.
            if (e.Button != MouseButtons.Middle)
            {
                return;
            }

            int index = TabAt(e.Location);
            if (index >= 0)
            {
                CloseDocument(_documents[index]);
            }
        }

        // ---------------------------------------------------------------- tab close buttons

        /// <summary>Tab whose close button is under the mouse, or -1.</summary>
        private int _hotCloseTab = -1;

        /// <summary>Set while a tab is removed, so the selection that follows is never cancelled.</summary>
        private bool _removingTab;

        private int CloseGlyphSize => LogicalToDeviceUnits(14);

        private int TabAt(Point location)
        {
            for (int i = 0; i < _docTabs.TabCount; i++)
            {
                if (_docTabs.GetTabRect(i).Contains(location))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>The close button of a tab: a square at the right end of its header.</summary>
        private Rectangle CloseRect(Rectangle tab)
        {
            int size = CloseGlyphSize;
            return new Rectangle(tab.Right - size - LogicalToDeviceUnits(4), tab.Top + ((tab.Height - size) / 2), size, size);
        }

        private int CloseButtonAt(Point location)
        {
            int index = TabAt(location);
            return index >= 0 && CloseRect(_docTabs.GetTabRect(index)).Contains(location) ? index : -1;
        }

        private void DocTabs_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _docTabs.TabCount)
            {
                return;
            }

            bool selected = e.Index == _docTabs.SelectedIndex;
            Rectangle bounds = e.Bounds;
            using (var back = new SolidBrush(_docTabs.TabBackColor(selected)))
            {
                e.Graphics.FillRectangle(back, bounds);
            }

            Rectangle close = CloseRect(bounds);
            int inset = LogicalToDeviceUnits(6);
            var textBounds = new Rectangle(bounds.Left + inset, bounds.Top, close.Left - bounds.Left - inset, bounds.Height);
            TextRenderer.DrawText(e.Graphics, _docTabs.TabPages[e.Index].Text, _docTabs.Font, textBounds,
                _docTabs.TabTextColor(selected),
                // NoPadding: the tab's width was measured from the bare text.
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding);

            bool hot = e.Index == _hotCloseTab;
            if (hot)
            {
                using var hotBack = new SolidBrush(Color.FromArgb(232, 17, 35));
                e.Graphics.FillRectangle(hotBack, close);
            }

            // The cross, a little inside the button.
            int pad = LogicalToDeviceUnits(4);
            Color crossColor = hot ? Color.White : _docTabs.GlyphColor(selected);
            using var pen = new Pen(crossColor, Math.Max(1f, LogicalToDeviceUnits(1) * 1.4f));
            System.Drawing.Drawing2D.SmoothingMode previous = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(pen, close.Left + pad, close.Top + pad, close.Right - pad - 1, close.Bottom - pad - 1);
            e.Graphics.DrawLine(pen, close.Right - pad - 1, close.Top + pad, close.Left + pad, close.Bottom - pad - 1);
            e.Graphics.SmoothingMode = previous;
        }

        private void DocTabs_Selecting(object sender, TabControlCancelEventArgs e)
        {
            // A press on another tab's close button closes it without switching to it first.
            if (!_removingTab && (MouseButtons & MouseButtons.Left) != 0 && CloseButtonAt(_docTabs.PointToClient(MousePosition)) >= 0)
            {
                e.Cancel = true;
            }
        }

        private void DocTabs_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            int index = CloseButtonAt(e.Location);
            if (index >= 0)
            {
                SetHotCloseTab(-1);
                CloseDocument(_documents[index]);
            }
        }

        private void DocTabs_MouseMove(object sender, MouseEventArgs e)
        {
            SetHotCloseTab(CloseButtonAt(e.Location));
        }

        private void DocTabs_MouseLeave(object sender, EventArgs e)
        {
            SetHotCloseTab(-1);
        }

        private void SetHotCloseTab(int index)
        {
            if (index == _hotCloseTab)
            {
                return;
            }

            InvalidateTab(_hotCloseTab);
            _hotCloseTab = index;
            InvalidateTab(_hotCloseTab);
        }

        private void InvalidateTab(int index)
        {
            if (index >= 0 && index < _docTabs.TabCount)
            {
                _docTabs.Invalidate(_docTabs.GetTabRect(index));
            }
        }
    }
}
