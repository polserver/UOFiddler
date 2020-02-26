using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace Ultima
{
    public sealed class SpeechList
    {
        public static List<SpeechEntry> Entries { get; private set; }

        private static readonly byte[] Buffer = new byte[128];

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
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[fs.Length];
                unsafe
                {
                    int order = 0;
                    fs.Read(buffer, 0, buffer.Length);
                    fixed (byte* data = buffer)
                    {
                        byte* binDat = data;
                        byte* binDatEnd = binDat + buffer.Length;

                        while (binDat != binDatEnd)
                        {
                            short id = (short)((*binDat++ >> 8) | (*binDat++)); // Swapped Endian
                            short length = (short)((*binDat++ >> 8) | (*binDat++));
                            if (length > 128)
                                length = 128;
                            for (int i = 0; i < length; ++i)
                                Buffer[i] = *binDat++;
                            string keyword = Encoding.UTF8.GetString(Buffer, 0, length);
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
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    foreach (SpeechEntry entry in Entries)
                    {
                        bin.Write(NativeMethods.SwapEndian(entry.Id));
                        byte[] utf8String = Encoding.UTF8.GetBytes(entry.KeyWord);
                        short length = (short)utf8String.Length;
                        bin.Write(NativeMethods.SwapEndian(length));
                        bin.Write(utf8String);
                    }
                }
            }
        }

        public static void ExportToCsv(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.Unicode))
            {
                tex.WriteLine("Order;ID;KeyWord");
                foreach (SpeechEntry entry in Entries)
                {
                    tex.WriteLine($"{entry.Order};{entry.Id};{entry.KeyWord}");
                }
            }
        }

        public static void ImportFromCsv(string fileName)
        {
            Entries = new List<SpeechEntry>(0);
            if (!File.Exists(fileName))
            {
                return;
            }

            using (StreamReader sr = new StreamReader(fileName))
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
                            continue;

                        int order = ConvertStringToInt(split[0]);
                        int id = ConvertStringToInt(split[1]);
                        string word = split[2];
                        word = word.Replace("\"", "");
                        Entries.Add(new SpeechEntry((short)id, word, order));
                    }
                    catch
                    {
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
                int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out result);
            }
            else
                int.TryParse(text, System.Globalization.NumberStyles.Integer, null, out result);

            return result;
        }

        #region SortComparer
        public class IdComparer : IComparer<SpeechEntry>
        {
            private readonly bool _mDesc;

            public IdComparer(bool desc)
            {
                _mDesc = desc;
            }

            public int Compare(SpeechEntry objA, SpeechEntry objB)
            {
                if (objA.Id == objB.Id)
                    return 0;
                else if (_mDesc)
                    return (objA.Id < objB.Id) ? 1 : -1;
                else
                    return (objA.Id < objB.Id) ? -1 : 1;
            }
        }

        public class KeyWordComparer : IComparer<SpeechEntry>
        {
            private readonly bool _mDesc;

            public KeyWordComparer(bool desc)
            {
                _mDesc = desc;
            }

            public int Compare(SpeechEntry objA, SpeechEntry objB)
            {
                return _mDesc
                    ? string.CompareOrdinal(objB.KeyWord, objA.KeyWord)
                    : string.CompareOrdinal(objA.KeyWord, objB.KeyWord);
            }
        }

        private class OrderComparer : IComparer<SpeechEntry>
        {
            public int Compare(SpeechEntry objA, SpeechEntry objB)
            {
                if (objA.Order == objB.Order)
                    return 0;
                else
                    return (objA.Order < objB.Order) ? -1 : 1;
            }
        }

        #endregion

    }

    public sealed class SpeechEntry
    {
        public short Id { get; }
        public string KeyWord { get; set; }

        [Browsable(false)]
        public int Order { get; }

        public SpeechEntry(short id, string keyword, int order)
        {
            Id = id;
            KeyWord = keyword;
            Order = order;
        }
    }

    // TODO: unused?
/*    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpeechMul
    {
        public short id;
        public short length;
        public byte[] keyword;
    }
*/
}
