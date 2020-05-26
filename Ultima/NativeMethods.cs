using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Ultima
{
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
        public static extern int CloseHandle(ClientProcessHandle handle);

        [DllImport("Kernel32")]
        public static extern unsafe int ReadProcessMemory(
            ClientProcessHandle process, int baseAddress, void* buffer, int size, ref int op);

        [DllImport("Kernel32")]
        public static extern unsafe int WriteProcessMemory(
            ClientProcessHandle process, int baseAddress, void* buffer, int size, int nullMe);

        [DllImport("User32")]
        public static extern int SetForegroundWindow(ClientWindowHandle hWnd);

        [DllImport("User32")]
        public static extern int SendMessage(ClientWindowHandle hWnd, int wMsg, int wParam, int lParam);

        [DllImport("User32")]
        public static extern bool PostMessage(ClientWindowHandle hWnd, int wMsg, int wParam, int lParam);

        [DllImport("User32")]
        public static extern int OemKeyScan(int wOemChar);

        [DllImport("user32")]
        public static extern ClientWindowHandle FindWindowA(string lpClassName, string lpWindowName);

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

        private static byte[] _stringBuffer;

        public static unsafe string ReadNameString(byte* buffer, int len)
        {
            if ((_stringBuffer == null) || (_stringBuffer.Length < len))
            {
                _stringBuffer = new byte[20];
            }

            int count;
            for (count = 0; count < len && *buffer != 0; ++count)
            {
                _stringBuffer[count] = *buffer++;
            }

            return Encoding.Default.GetString(_stringBuffer, 0, count);
        }

        public static string ReadNameString(byte[] buffer, int len)
        {
            int count;
            for (count = 0; count < 20 && buffer[count] != 0; ++count)
            {
                // TODO: this loop is weird
                //;
            }

            return Encoding.Default.GetString(buffer, 0, count);
        }
    }
}
