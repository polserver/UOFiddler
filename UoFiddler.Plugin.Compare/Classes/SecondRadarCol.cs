using System.IO;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondRadarCol
    {
        private static ushort[] _colors;

        public static bool IsLoaded => _colors != null;

        public static ushort GetColor(int index) =>
            _colors != null && index >= 0 && index < _colors.Length ? _colors[index] : (ushort)0;

        public static int Length => _colors?.Length ?? 0;

        /// <summary>
        /// Reads a radarcol.mul from the given file path. Returns false if the file
        /// cannot be read.
        /// </summary>
        public static bool Initialize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var colors = new ushort[fs.Length / 2];
                    var buffer = new byte[(int)fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                    GCHandle gc = GCHandle.Alloc(colors, GCHandleType.Pinned);
                    Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)fs.Length);
                    gc.Free();
                    _colors = colors;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
