using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima
{
    public sealed class RadarCol
    {
        // 0x4000 land colors followed by one color per item id up to 0xFFFF.
        private const int _maxColorIndex = 0x4000 + 0x10000 - 1;

        static RadarCol()
        {
            Initialize();
        }

        public static ushort[] Colors { get; private set; }

        public static ushort GetItemColor(int index)
        {
            return index + 0x4000 < Colors.Length ? Colors[index + 0x4000] : (ushort)0;
        }

        public static ushort GetLandColor(int index)
        {
            return index < Colors.Length ? Colors[index] : (ushort)0;
        }

        public static void SetItemColor(int index, ushort value)
        {
            Colors[index + 0x4000] = value;
        }

        public static void SetLandColor(int index, ushort value)
        {
            Colors[index] = value;
        }

        public static void Initialize()
        {
            string path = Files.GetFilePath("radarcol.mul");
            if (path != null)
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Colors = new ushort[fs.Length / 2];
                    fs.ReadExactly(MemoryMarshal.AsBytes(Colors.AsSpan()));
                }
            }
            else
            {
                Colors = new ushort[0x8000];
            }
        }

        public static void Save(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                foreach (var colorValue in Colors)
                {
                    bin.Write(colorValue);
                }
            }
        }

        public static void ExportToCSV(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("ID;Color");

                for (int i = 0; i < Colors.Length; ++i)
                {
                    tex.WriteLine("0x{0:X4};{1}", i, Colors[i]);
                }
            }
        }

        public static void ImportFromCSV(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            // Rows update the loaded table; ids past its end grow it.
            var colors = (ushort[])(Colors ?? []).Clone();

            using (var sr = new StreamReader(fileName))
            {
                while (sr.ReadLine() is { } line)
                {
                    line = line.Trim();

                    if (line.Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.StartsWith("ID;"))
                    {
                        continue;
                    }

                    string[] split = line.Split(';');
                    if (split.Length < 2 || !TryConvertStringToInt(split[0], out int id) || !TryConvertStringToInt(split[1], out int color))
                    {
                        continue;
                    }

                    if (id < 0 || id > _maxColorIndex)
                    {
                        continue;
                    }

                    if (id >= colors.Length)
                    {
                        Array.Resize(ref colors, id + 1);
                    }

                    colors[id] = (ushort)color;
                }
            }

            Colors = colors;
        }

        private static bool TryConvertStringToInt(string text, out int result)
        {
            text = text.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
            }

            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }
    }
}
