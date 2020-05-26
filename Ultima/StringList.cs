using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima
{
    public sealed class StringList
    {
        private int _header1;
        private short _header2;

        public List<StringEntry> Entries { get; private set; }
        public string Language { get; }

        private Dictionary<int, string> _stringTable;
        // private Dictionary<int, StringEntry> _entryTable; // TODO: unused?
        private static byte[] _buffer = new byte[1024];

        /// <summary>
        /// Initialize <see cref="StringList"/> of Language
        /// </summary>
        /// <param name="language"></param>
        public StringList(string language)
        {
            Language = language;
            LoadEntry(Files.GetFilePath($"cliloc.{language}"));
        }

        /// <summary>
        /// Initialize <see cref="StringList"/> of Language from path
        /// </summary>
        /// <param name="language"></param>
        /// <param name="path"></param>
        public StringList(string language, string path)
        {
            Language = language;
            LoadEntry(path);
        }

        private void LoadEntry(string path)
        {
            if (path == null)
            {
                Entries = new List<StringEntry>(0);
                return;
            }
            Entries = new List<StringEntry>();
            _stringTable = new Dictionary<int, string>();
            // _entryTable = new Dictionary<int, StringEntry>();

            using (var bin = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                _header1 = bin.ReadInt32();
                _header2 = bin.ReadInt16();

                while (bin.BaseStream.Length != bin.BaseStream.Position)
                {
                    int number = bin.ReadInt32();
                    byte flag = bin.ReadByte();
                    int length = bin.ReadInt16();

                    if (length > _buffer.Length)
                    {
                        _buffer = new byte[(length + 1023) & ~1023];
                    }

                    bin.Read(_buffer, 0, length);
                    string text = Encoding.UTF8.GetString(_buffer, 0, length);

                    var se = new StringEntry(number, text, flag);
                    Entries.Add(se);

                    _stringTable[number] = text;
                    // _entryTable[number] = se;
                }
            }
        }

        /// <summary>
        /// Saves <see cref="SaveStringList"/> to fileName
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveStringList(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var bin = new BinaryWriter(fs))
                {
                    bin.Write(_header1);
                    bin.Write(_header2);

                    Entries.Sort(new NumberComparer(false));

                    foreach (StringEntry entry in Entries)
                    {
                        bin.Write(entry.Number);
                        bin.Write((byte)entry.Flag);
                        byte[] utf8String = Encoding.UTF8.GetBytes(entry.Text);
                        var length = (ushort)utf8String.Length;
                        bin.Write(length);
                        bin.Write(utf8String);
                    }
                }
            }
        }

        public string GetString(int number)
        {
            return _stringTable?.ContainsKey(number) != true ? null : _stringTable[number];
        }

/*
 // TODO: unused?
        public StringEntry GetEntry(int number)
        {
            return _entryTable?.ContainsKey(number) != true ? null : _entryTable[number];
        }
*/

        public class NumberComparer : IComparer<StringEntry>
        {
            private readonly bool _sortDescending;

            public NumberComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if (objA.Number == objB.Number)
                {
                    return 0;
                }
                else if (_sortDescending)
                {
                    return (objA.Number < objB.Number) ? 1 : -1;
                }
                else
                {
                    return (objA.Number < objB.Number) ? -1 : 1;
                }
            }
        }

        public class FlagComparer : IComparer<StringEntry>
        {
            private readonly bool _sortDescending;

            public FlagComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if ((byte)objA.Flag == (byte)objB.Flag)
                {
                    if (objA.Number == objB.Number)
                    {
                        return 0;
                    }
                    else if (_sortDescending)
                    {
                        return (objA.Number < objB.Number) ? 1 : -1;
                    }
                    else
                    {
                        return (objA.Number < objB.Number) ? -1 : 1;
                    }
                }
                else if (_sortDescending)
                {
                    return ((byte)objA.Flag < (byte)objB.Flag) ? 1 : -1;
                }
                else
                {
                    return ((byte)objA.Flag < (byte)objB.Flag) ? -1 : 1;
                }
            }
        }

        public class TextComparer : IComparer<StringEntry>
        {
            private readonly bool _sortDescending;

            public TextComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if (_sortDescending)
                {
                    return string.CompareOrdinal(objB.Text, objA.Text);
                }
                else
                {
                    return string.CompareOrdinal(objA.Text, objB.Text);
                }
            }
        }
    }
}