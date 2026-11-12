using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima
{
    public sealed class Skills
    {
        private static FileIndex _fileIndex = new FileIndex("skills.idx", "skills.mul", 16);

        private static List<SkillInfo> _skillEntries;

        public static List<SkillInfo> SkillEntries
        {
            get
            {
                if (_skillEntries != null)
                {
                    return _skillEntries;
                }

                _skillEntries = LoadEntries();
                return _skillEntries;
            }
            set { _skillEntries = value; }
        }

        /// <summary>
        /// Reads every slot. The list position is the skill id, so an empty slot is kept as a
        /// placeholder with no name (see <see cref="SkillInfo.IsEmpty"/>) instead of ending the list.
        /// Empty slots after the last skill are left out; Save writes them back.
        /// </summary>
        private static List<SkillInfo> LoadEntries()
        {
            var entries = new List<SkillInfo>();
            for (int i = 0; i < _fileIndex.IndexLength; ++i)
            {
                entries.Add(GetSkill(i) ?? new SkillInfo(i, string.Empty, false, 0));
            }

            while (entries.Count > 0 && entries[^1].IsEmpty)
            {
                entries.RemoveAt(entries.Count - 1);
            }

            return entries;
        }

        /// <summary>
        /// ReReads skills.mul
        /// </summary>
        public static void Reload()
        {
            _fileIndex?.Dispose();
            _fileIndex = new FileIndex("skills.idx", "skills.mul", 16);
            _skillEntries = LoadEntries();
        }

        /// <summary>
        /// Returns <see cref="SkillInfo"/> of index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static SkillInfo GetSkill(int index)
        {
            Stream stream = _fileIndex.Seek(index, out int length, out int extra, out bool _);
            if (stream == null)
            {
                return null;
            }

            if (length == 0)
            {
                return null;
            }

            // leaveOpen: the stream is the index's shared file stream.
            using (var bin = new BinaryReader(stream, Encoding.Latin1, leaveOpen: true))
            {
                bool action = bin.ReadBoolean();
                string name = ReadNameString(bin, length - 1);
                return new SkillInfo(index, name, action, extra);
            }
        }

        private static string ReadNameString(BinaryReader bin, int length)
        {
            byte[] bytes = bin.ReadBytes(length);

            // The name ends at the first null byte.
            int count = System.Array.IndexOf(bytes, (byte)0);
            if (count < 0)
            {
                count = bytes.Length;
            }

            // Names are single bytes; Latin-1 keeps every byte as loaded so saving writes it back unchanged
            return Encoding.Latin1.GetString(bytes, 0, count);
        }

        public static void Save(string path)
        {
            string idx = Path.Combine(path, "skills.idx");
            string mul = Path.Combine(path, "skills.mul");

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var binidx = new BinaryWriter(fsidx))
            using (var binmul = new BinaryWriter(fsmul))
            {
                List<SkillInfo> entries = SkillEntries;
                long count = System.Math.Max(_fileIndex.IndexLength, entries.Count);
                for (int i = 0; i < count; ++i)
                {
                    SkillInfo skill = (i < entries.Count) ? entries[i] : null;
                    if (skill == null || skill.IsEmpty)
                    {
                        binidx.Write(-1); // lookup
                        binidx.Write(0);  // length
                        binidx.Write(0);  // extra
                    }
                    else
                    {
                        binidx.Write((int)fsmul.Position); // lookup
                        var length = (int)fsmul.Position;
                        binmul.Write(skill.IsAction);

                        byte[] nameBytes = Encoding.Latin1.GetBytes(skill.Name);
                        binmul.Write(nameBytes);
                        binmul.Write((byte)0); // null terminated

                        length = (int)fsmul.Position - length;
                        binidx.Write(length);
                        binidx.Write(skill.Extra);
                    }
                }
            }
        }
    }

    public sealed class SkillInfo
    {
        private string _name;

        public int Index { get; set; }

        public bool IsAction { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value ?? string.Empty;
            }
        }

        public int Extra { get; }

        /// <summary>
        /// An empty slot: a skill without a name is saved as an unused entry.
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        public bool IsEmpty => string.IsNullOrEmpty(_name);

        public SkillInfo(int nr, string name, bool action, int extra)
        {
            Index = nr;
            _name = name;
            IsAction = action;
            Extra = extra;
        }
    }
}
