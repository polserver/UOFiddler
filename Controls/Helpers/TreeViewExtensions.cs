using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace FiddlerControls.Helpers
{
    static class TreeViewExtensions
    {
        [DllImport("USER32", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("USER32", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public const int TVM_GETEDITCONTROL = 0x110F;
        public const int WM_SETTEXT = 0xC;

        public static void SetEditText(this TreeView treeView, string name)
        {
            var editHandle = SendMessage(treeView.Handle, TVM_GETEDITCONTROL, IntPtr.Zero, IntPtr.Zero);
            if (editHandle != IntPtr.Zero)
            {
                SendMessage(editHandle, WM_SETTEXT, IntPtr.Zero, name);
            }
        }
    }
}