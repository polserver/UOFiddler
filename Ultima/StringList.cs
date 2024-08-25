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

        public static class Decoder
        {
            public static byte[] Decompress(byte[] buffer)
            {
                byte[] output = null;

                using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
                {
                    uint header = reader.ReadUInt32();
                    uint len = 0u;

                    byte firstChar = reader.ReadByte();

                    ushort[] table = BuildTable(firstChar);

                    byte[] list = new byte[reader.BaseStream.Length - 4];
                    int i = 0;

                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        byte currentValue = firstChar;
                        ushort value = table[currentValue];

                        if (currentValue > 0)
                        {
                            do
                            {
                                table[currentValue] = table[currentValue - 1];
                            }
                            while (--currentValue > 0);
                        }

                        table[0] = value;

                        list[i++] = (byte)value;
                        firstChar = reader.ReadByte();
                    }

                    output = InternalDecompress(list, len);
                }

                return output;
            }

            private static ushort[] BuildTable(byte startValue)
            {
                ushort[] table = new ushort[256 * 256];
                int index = 0;
                byte firstByte = startValue;
                byte secondByte = 0;

                for (int i = 0; i < 256 * 256; i++)
                {
                    ushort val = (ushort)(firstByte + (secondByte << 8));
                    table[index++] = val;

                    firstByte++;

                    if (firstByte == 0)
                    {
                        secondByte++;
                    }
                }

                Array.Sort(table);

                return table;
            }

            private static byte[] InternalDecompress(Span<byte> input, uint len)
            {
                Span<char> symbolTable = stackalloc char[256];
                Span<char> frequency = stackalloc char[256];
                Span<int> partialInput = stackalloc int[256 * 3];

                for (int i = 0; i < 256; i++)
                {
                    symbolTable[i] = (char)i;
                }

                input.Slice(0, 1024).CopyTo(MemoryMarshal.AsBytes(partialInput));

                int sum = 0;

                for (int i = 0; i < 256; i++)
                {
                    sum += partialInput[i];
                }

                if (len == 0)
                {
                    len = (uint)sum;
                }

                if (sum != len)
                {
                    return Array.Empty<byte>();
                }

                byte[] output = new byte[len];

                int count = 0;
                int nonZeroCount = 0;

                for (int i = 0; i < 256; i++)
                {
                    if (partialInput[i] != 0)
                    {
                        nonZeroCount++;
                    }
                }

                Frequency(partialInput, frequency);

                for (int i = 0, m = 0; i < nonZeroCount; ++i)
                {
                    byte freq = (byte)frequency[i];
                    symbolTable[input[m + 1024]] = (char)freq;
                    partialInput[freq + 256] = m + 1;
                    m += partialInput[freq];
                    partialInput[freq + 512] = m;
                }

                byte val = (byte)symbolTable[0];

                if (len != 0)
                {
                    do
                    {
                        ref int firstValRef = ref partialInput[val + 256];
                        output[count] = val;

                        if (firstValRef >= partialInput[val + 512])
                        {
                            if (nonZeroCount-- > 0)
                            {
                                ShiftLeft(symbolTable, nonZeroCount);
                                val = (byte)symbolTable[0];
                            }
                        }
                        else
                        {
                            char idx = (char)input[firstValRef + 1024];
                            firstValRef++;

                            if (idx != 0)
                            {
                                ShiftLeft(symbolTable, idx);
                                symbolTable[(byte)idx] = (char)val;
                                val = (byte)symbolTable[0];
                            }
                        }

                        count++;
                    }
                    while (count < len);
                }

                return output;
            }

            private static void Frequency(Span<int> input, Span<char> output)
            {
                Span<int> tmp = stackalloc int[256];
                input.Slice(0, tmp.Length).CopyTo(tmp);

                for (int i = 0; i < 256; i++)
                {
                    uint value = 0;
                    byte index = 0;

                    for (int j = 0; j < 256; j++)
                    {
                        if (tmp[j] > value)
                        {
                            index = (byte)j;
                            value = (uint)tmp[j];
                        }
                    }

                    if (value == 0)
                    {
                        break;
                    }

                    output[i] = (char)index;
                    tmp[index] = 0;
                }
            }

            private static void ShiftLeft(Span<char> input, int max)
            {
                for (int i = 0; i < max; ++i)
                {
                    input[i] = input[i + 1];
                }
            }
        }
    }
}