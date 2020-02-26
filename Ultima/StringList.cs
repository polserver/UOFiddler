using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima
{
    public sealed class StringList
    {
        private int _mHeader1;
        private short _mHeader2;

        public List<StringEntry> Entries { get; set; }
        public string Language { get; }

		private Dictionary<int, string> _mStringTable;
		private Dictionary<int, StringEntry> _mEntryTable;
        private static byte[] _mBuffer = new byte[1024];

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
			_mStringTable = new Dictionary<int, string>();
			_mEntryTable = new Dictionary<int, StringEntry>();

            using (BinaryReader bin = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                _mHeader1 = bin.ReadInt32();
                _mHeader2 = bin.ReadInt16();

                while (bin.BaseStream.Length != bin.BaseStream.Position)
                {
                    int number = bin.ReadInt32();
                    byte flag = bin.ReadByte();
                    int length = bin.ReadInt16();

                    if (length > _mBuffer.Length)
                        _mBuffer = new byte[(length + 1023) & ~1023];

                    bin.Read(_mBuffer, 0, length);
                    string text = Encoding.UTF8.GetString(_mBuffer, 0, length);

					StringEntry se = new StringEntry(number, text, flag);
					Entries.Add(se);

					_mStringTable[number] = text;
					_mEntryTable[number] = se;
                }
            }
        }

        /// <summary>
        /// Saves <see cref="SaveStringList"/> to FileName
        /// </summary>
        /// <param name="fileName"></param>
        public void SaveStringList(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    bin.Write(_mHeader1);
                    bin.Write(_mHeader2);
                    Entries.Sort(new NumberComparer(false));
                    foreach (StringEntry entry in Entries)
                    {
                        bin.Write(entry.Number);
                        bin.Write((byte)entry.Flag);
                        byte[] utf8String = Encoding.UTF8.GetBytes(entry.Text);
                        ushort length = (ushort)utf8String.Length;
                        bin.Write(length);
                        bin.Write(utf8String);
                    }
                }
            }
        }

		public string GetString(int number)
		{
			if (_mStringTable == null || !_mStringTable.ContainsKey(number))
				return null;

			return _mStringTable[number];
		}

		public StringEntry GetEntry(int number)
		{
			if (_mEntryTable == null || !_mEntryTable.ContainsKey(number))
				return null;

			return _mEntryTable[number];
		}
        #region SortComparer

        public class NumberComparer : IComparer<StringEntry>
        {
            private readonly bool _mDesc;

            public NumberComparer(bool desc)
            {
                _mDesc = desc;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if (objA.Number == objB.Number)
                    return 0;
                else if (_mDesc)
                    return (objA.Number < objB.Number) ? 1 : -1;
                else
                    return (objA.Number < objB.Number) ? -1 : 1;
            }
        }

        public class FlagComparer : IComparer<StringEntry>
        {
            private readonly bool _mDesc;

            public FlagComparer(bool desc)
            {
                _mDesc = desc;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if ((byte)objA.Flag == (byte)objB.Flag)
                {
                    if (objA.Number == objB.Number)
                        return 0;
                    else if (_mDesc)
                        return (objA.Number < objB.Number) ? 1 : -1;
                    else
                        return (objA.Number < objB.Number) ? -1 : 1;
                }
                else if (_mDesc)
                    return ((byte)objA.Flag < (byte)objB.Flag) ? 1 : -1;
                else
                    return ((byte)objA.Flag < (byte)objB.Flag) ? -1 : 1;
            }
        }

        public class TextComparer : IComparer<StringEntry>
        {
            private readonly bool _mDesc;

            public TextComparer(bool desc)
            {
                _mDesc = desc;
            }

            public int Compare(StringEntry objA, StringEntry objB)
            {
                if (_mDesc)
                    return string.Compare(objB.Text, objA.Text);
                else
                    return string.Compare(objA.Text, objB.Text);
            }
        }
        #endregion
    }
}