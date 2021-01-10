using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima
{
    public sealed class SpeechList
    {
        public static List<SpeechEntry> Entries { get; private set; }

        private static readonly byte[] _buffer = new byte[128];

        static SpeechList()
        {
            Initialize();
        }

        /// <summary>
        /// Loads speech.mul in <see cref="SpeechList.Entries"/>
        /// </summary>
        public static void Initialize()
        {
            string path = Files.GetFilePath("speech.mul");
            if (path == null)
            {
                Entries = new List<SpeechEntry>(0);
                return;
            }

            Entries = new List<SpeechEntry>();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[fs.Length];
                unsafe
                {
                    int order = 0;
                    fs.Read(buffer, 0, buffer.Length);
                    fixed (byte* data = buffer)
                    {
                        byte* binData = data;
                        byte* binDataEnd = binData + buffer.Length;

                        while (binData != binDataEnd)
                        {
                            var id = (short)((*binData++ >> 8) | (*binData++)); // Swapped Endian
                            var length = (short)((*binData++ >> 8) | (*binData++));
                            if (length > 128)
                            {
                                length = 128;
                            }

                            for (int i = 0; i < length; ++i)
                            {
                                _buffer[i] = *binData++;
                            }

                            string keyword = Encoding.UTF8.GetString(_buffer, 0, length);
                            Entries.Add(new SpeechEntry(id, keyword, order));
                            ++order;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves speech.mul to <see cref="fileName"/>
        /// </summary>
        /// <param name="fileName"></param>
        public static void SaveSpeechList(string fileName)
        {
            Entries.Sort(new OrderComparer());
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var bin = new BinaryWriter(fs))
                {
                    foreach (SpeechEntry entry in Entries)
                    {
                        bin.Write(NativeMethods.SwapEndian(entry.ID));
                        byte[] utf8String = Encoding.UTF8.GetBytes(entry.KeyWord);
                        var length = (short)utf8String.Length;
                        bin.Write(NativeMethods.SwapEndian(length));
                        bin.Write(utf8String);
                    }
                }
            }
        }

        public static void ExportToCSV(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.Unicode))
            {
                tex.WriteLine("Order;ID;KeyWord");

                foreach (SpeechEntry entry in Entries)
                {
                    tex.WriteLine($"{entry.Order};{entry.ID};{entry.KeyWord}");
                }
            }
        }

        public static void ImportFromCSV(string fileName)
        {
            Entries = new List<SpeechEntry>(0);
            if (!File.Exists(fileName))
            {
                return;
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

                    if ((line.Contains("Order")) && (line.Contains("KeyWord")))
                    {
                        continue;
                    }

                    try
                    {
                        string[] split = line.Split(';');
                        if (split.Length < 3)
                        {
                            continue;
                        }

                        int order = ConvertStringToInt(split[0]);
                        int id = ConvertStringToInt(split[1]);
                        string word = split[2];
                        word = word.Replace("\"", "");
                        Entries.Add(new SpeechEntry((short)id, word, order));
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

        public class IDComparer : IComparer<SpeechEntry>
        {
            private readonly bool _sortDescending;

            public IDComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(SpeechEntry x, SpeechEntry y)
            {
                if (x.ID == y.ID)
                {
                    return 0;
                }

                if (_sortDescending)
                {
                    return (x.ID < y.ID) ? 1 : -1;
                }
                else
                {
                    return (x.ID < y.ID) ? -1 : 1;
                }
            }
        }

        public class KeyWordComparer : IComparer<SpeechEntry>
        {
            private readonly bool _sortDescending;

            public KeyWordComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(SpeechEntry x, SpeechEntry y)
            {
                if (_sortDescending)
                {
                    return string.CompareOrdinal(y.KeyWord, x.KeyWord);
                }
                else
                {
                    return string.CompareOrdinal(x.KeyWord, y.KeyWord);
                }
            }
        }

        private class OrderComparer : IComparer<SpeechEntry>
        {
            public int Compare(SpeechEntry x, SpeechEntry y)
            {
                if (x.Order == y.Order)
                {
                    return 0;
                }
                else
                {
                    return (x.Order < y.Order) ? -1 : 1;
                }
            }
        }
    }

    public sealed class SpeechEntry
    {
        public short ID { get; }
        public string KeyWord { get; set; }

        [Browsable(false)]
        public int Order { get; }

        public SpeechEntry(short id, string keyword, int order)
        {
            ID = id;
            KeyWord = keyword;
            Order = order;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpeechMul
    {
        public short id;
        public short length;
        public byte[] keyword;
    }
}
