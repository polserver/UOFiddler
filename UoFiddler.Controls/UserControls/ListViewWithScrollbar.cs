using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class ListViewWithScrollbar : ListView
    {
        private const int WS_HSCROLL = 0x00100000;
        private const int SB_HORZ = 0;

        public ListViewWithScrollbar()
        {
            InitializeComponent();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= WS_HSCROLL; // Always show the horizontal scrollbar
                return cp;
            }
        }

        private const int WM_NCCALCSIZE = 0x0083;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCCALCSIZE)
            {
                ShowScrollBar(this.Handle, SB_HORZ, true);
            }
            base.WndProc(ref m);

        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
    }
}
