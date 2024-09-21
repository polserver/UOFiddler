using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        private Dictionary<int, StringEntry> _entryTable;
        private static byte[] _buffer = new byte[1024];

        public StringList(string language)
        {
            Language = language;
            LoadEntry(Files.GetFilePath($"cliloc.{language}"));
        }

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
            _entryTable = new Dictionary<int, StringEntry>();

            byte[] fileData = File.ReadAllBytes(path);

            try
            {
                ProcessCompressedFile(fileData);
            }
            catch
            {
                ProcessUncompressedFile(fileData);
            }
        }

        private void ProcessCompressedFile(byte[] compressedData)
        {
            byte[] decompressedData = Decoder.Decompress(compressedData);

            using (var ms = new MemoryStream(decompressedData))
            using (var bin = new BinaryReader(ms))
            {
                bin.BaseStream.Seek(6, SeekOrigin.Begin);

                while (bin.BaseStream.Position < bin.BaseStream.Length)
                {
                    int number = bin.ReadInt32();
                    byte flag = bin.ReadByte();
                    ushort length = bin.ReadUInt16();

                    if (length > _buffer.Length)
                    {
                        _buffer = new byte[(length + 1023) & ~1023];
                    }

                    bin.Read(_buffer, 0, length);
                    string text = Encoding.UTF8.GetString(_buffer, 0, length);

                    AddEntry(number, text, flag);
                }
            }
        }

        private void ProcessUncompressedFile(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var bin = new BinaryReader(ms))
            {
                _header1 = bin.ReadInt32();
                _header2 = bin.ReadInt16();

                while (bin.BaseStream.Position < bin.BaseStream.Length)
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

                    AddEntry(number, text, flag);
                }
            }
        }

        private void AddEntry(int number, string text, byte flag)
        {
            var se = new StringEntry(number, text, flag);
            Entries.Add(se);

            _stringTable[number] = text;
            _entryTable[number] = se;
        }

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

        public StringEntry GetEntry(int number)
        {
            return _entryTable?.ContainsKey(number) != true ? null : _entryTable[number];
        }

        public class NumberComparer : IComparer<StringEntry>
        {
            private readonly bool _sortDescending;

            public NumberComparer(bool sortDescending)
            {
                _sortDescending = sortDescending;
            }

            public int Compare(StringEntry x, StringEntry y)
            {
                if (x.Number == y.Number)
                {
                    return 0;
                }
                else if (_sortDescending)
                {
                    return (x.Number < y.Number) ? 1 : -1;
                }
                else
                {
                    return (x.Number < y.Number) ? -1 : 1;
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

            public int Compare(StringEntry x, StringEntry y)
            {
                if ((byte)x.Flag == (byte)y.Flag)
                {
                    if (x.Number == y.Number)
                    {
                        return 0;
                    }

                    if (_sortDescending)
                    {
                        return (x.Number < y.Number) ? 1 : -1;
                    }
                    else
                    {
                        return (x.Number < y.Number) ? -1 : 1;
                    }
                }

                if (_sortDescending)
                {
                    return ((byte)x.Flag < (byte)y.Flag) ? 1 : -1;
                }
                else
                {
                    return ((byte)x.Flag < (byte)y.Flag) ? -1 : 1;
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

            public int Compare(StringEntry x, StringEntry y)
            {
                return _sortDescending
                    ? string.CompareOrdinal(y.Text, x.Text)
                    : string.CompareOrdinal(x.Text, y.Text);
            }
        }        
    }
}