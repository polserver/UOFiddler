using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima
{
    public sealed class RadarCol
    {
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
                    GCHandle gc = GCHandle.Alloc(Colors, GCHandleType.Pinned);
                    var buffer = new byte[(int)fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                    Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)fs.Length);
                    gc.Free();
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

            using (var sr = new StreamReader(fileName))
            {
                string line;
                int count = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.StartsWith("ID;"))
                    {
                        continue;
                    }

                    ++count;
                }

                Colors = new ushort[count];
            }

            using (var sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.StartsWith("ID;"))
                    {
                        continue;
                    }

                    try
                    {
                        string[] split = line.Split(';');
                        if (split.Length < 2)
                        {
                            continue;
                        }

                        int id = ConvertStringToInt(split[0]);
                        int color = ConvertStringToInt(split[1]);
                        Colors[id] = (ushort)color;
                    }
                    catch
                    {
                        // TODO: ignored?
                        // ignored
                    }
                }
            }
        }

        private static int ConvertStringToInt(string text)
        {
            int result;
            if (text.Contains("0x"))
            {
                string convert = text.Replace("0x", "");
                int.TryParse(convert, NumberStyles.HexNumber, null, out result);
            }
            else
            {
                int.TryParse(text, NumberStyles.Integer, null, out result);
            }

            return result;
        }
    }
}
