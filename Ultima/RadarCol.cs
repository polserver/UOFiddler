using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultima
{
    public sealed class RadarCol
    {
        static RadarCol()
        {
            Initialize();
        }

        private static short[] _mColors;
        public static short[] Colors => _mColors;

        public static short GetItemColor(int index)
        {
            if (index + 0x4000 < _mColors.Length)
                return _mColors[index + 0x4000];
            return 0;
        }
        public static short GetLandColor(int index)
        {
            if (index < _mColors.Length)
                return _mColors[index];
            return 0;
        }

        public static void SetItemColor(int index, short value)
        {
            _mColors[index + 0x4000] = value;
        }
        public static void SetLandColor(int index, short value)
        {
            _mColors[index] = value;
        }

        public static void Initialize()
        {
            string path = Files.GetFilePath("radarcol.mul");
            if (path != null)
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _mColors = new short[fs.Length / 2];
                    GCHandle gc = GCHandle.Alloc(_mColors, GCHandleType.Pinned);
                    byte[] buffer = new byte[(int)fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                    Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)fs.Length);
                    gc.Free();
                }
            }
            else
                _mColors = new short[0x8000];
        }

        public static void Save(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    for (int i = 0; i < _mColors.Length; ++i)
                    {
                        bin.Write(_mColors[i]);
                    }
                }
            }
        }

        public static void ExportToCsv(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), System.Text.Encoding.GetEncoding(1252)))
            {
                tex.WriteLine("ID;Color");

                for (int i = 0; i < _mColors.Length; ++i)
                {
                    tex.WriteLine($"0x{i:X4};{_mColors[i]}");
                }
            }
        }

        public static void ImportFromCsv(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                int count = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        continue;
                    if (line.StartsWith("ID;"))
                        continue;
                    ++count;
                }
                _mColors = new short[count];
            }
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        continue;
                    if (line.StartsWith("ID;"))
                        continue;
                    try
                    {
                        string[] split = line.Split(';');
                        if (split.Length < 2)
                            continue;

                        int id = ConvertStringToInt(split[0]);
                        int color = ConvertStringToInt(split[1]);
                        _mColors[id] = (short)color;
                        
                    }
                    catch { }
                }
            }
        }

        private static int ConvertStringToInt(string text)
        {
            int result;
            if (text.Contains("0x"))
            {
                string convert = text.Replace("0x", "");
                int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out result);
            }
            else
                int.TryParse(text, System.Globalization.NumberStyles.Integer, null, out result);

            return result;
        }
    }
}
