using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UoFiddler.Controls.Helpers
{
    internal static class TreeViewExtensions
    {
        [DllImport("USER32", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("USER32", EntryPoint = "SendMessage", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        public const int TvmGeteditcontrol = 0x110F;
        public const int WmSettext = 0xC;

        public static void SetEditText(this TreeView treeView, string name)
        {
            IntPtr editHandle = SendMessage(treeView.Handle, TvmGeteditcontrol, IntPtr.Zero, IntPtr.Zero);
            if (editHandle != IntPtr.Zero)
            {
                SendMessage(editHandle, WmSettext, IntPtr.Zero, name);
            }
        }
    }
}