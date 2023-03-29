using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Ultima
{
    public class ClientWindowHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        public static ClientWindowHandle Invalid = new ClientWindowHandle(new IntPtr(-1));

        public ClientWindowHandle()
        { }

        public ClientWindowHandle(IntPtr value)
        {
            handle = value;
        }

        protected override bool ReleaseHandle()
        {
            if (!IsClosed)
            {
                return ReleaseHandle();
            }

            return true;
        }
    }

    public class ClientProcessHandle : CriticalHandleZeroOrMinusOneIsInvalid
    {
        public static ClientProcessHandle Invalid = new ClientProcessHandle(new IntPtr(-1));

        public ClientProcessHandle()
        { }

        public ClientProcessHandle(IntPtr value)
        {
            handle = value;
        }

        protected override bool ReleaseHandle()
        {
            return CloseHandle(this) == 0;
        }

        [DllImport("Kernel32")]
        private static extern int CloseHandle(ClientProcessHandle handle);
    }

    public static class NativeMethods
    {
        [DllImport("User32")]
        public static extern int IsWindow(ClientWindowHandle window);

        [DllImport("User32")]
        public static extern int GetWindowThreadProcessId(ClientWindowHandle window, ref ClientProcessHandle processId);

        [DllImport("Kernel32")]
        public static extern unsafe int _lread(SafeFileHandle hFile, void* lpBuffer, int wBytes);

        [DllImport("Kernel32")]
        public static extern ClientProcessHandle OpenProcess(
            int desiredAccess, int inheritClientHandle, ClientProcessHandle processId);

        [DllImport("Kernel32")]
        public static extern unsafe int ReadProcessMemory(
            ClientProcessHandle process, int baseAddress, void* buffer, int size, ref int op);

        [DllImport("Kernel32")]
        public static extern unsafe int WriteProcessMemory(
            ClientProcessHandle process, int baseAddress, void* buffer, int size, int nullMe);

        [DllImport("User32")]
        public static extern int SetForegroundWindow(ClientWindowHandle hWnd);

        [DllImport("User32")]
        public static extern bool ShowWindow(ClientWindowHandle handle, int nCmdShow);

        [DllImport("User32")]
        public static extern int SendMessage(ClientWindowHandle hWnd, int wMsg, int wParam, int lParam);

        [DllImport("User32")]
        public static extern bool PostMessage(ClientWindowHandle hWnd, int wMsg, int wParam, int lParam);

        [DllImport("User32")]
        public static extern int OemKeyScan(int wOemChar);

        [DllImport("user32")]
        public static extern ClientWindowHandle FindWindowA(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        // Delegate to filter which windows to include 
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// Swaps from Big to LittleEndian and vise versa
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static short SwapEndian(short x)
        {
            var y = (ushort)x;
            return (short)((y >> 8) | (y << 8));
        }
    }
}
