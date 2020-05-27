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

                _skillEntries = new List<SkillInfo>();
                for (int i = 0; i < _fileIndex.Index.Length; ++i)
                {
                    SkillInfo info = GetSkill(i);
                    if (info == null)
                    {
                        break;
                    }

                    _skillEntries.Add(info);
                }
                return _skillEntries;
            }
            set { _skillEntries = value; }
        }

        /// <summary>
        /// ReReads skills.mul
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex("skills.idx", "skills.mul", 16);
            _skillEntries = new List<SkillInfo>();
            for (int i = 0; i < _fileIndex.Index.Length; ++i)
            {
                SkillInfo info = GetSkill(i);
                if (info == null)
                {
                    break;
                }

                _skillEntries.Add(info);
            }
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

            using (var bin = new BinaryReader(stream))
            {
                bool action = bin.ReadBoolean();
                string name = ReadNameString(bin, length - 1);
                return new SkillInfo(index, name, action, extra);
            }
        }

        private static readonly byte[] _stringBuffer = new byte[1024];

        private static string ReadNameString(BinaryReader bin, int length)
        {
            bin.Read(_stringBuffer, 0, length);
            int count;
            for (count = 0; count < length && _stringBuffer[count] != 0; ++count)
            {
                // TODO: this loop is weird
                //;
            }

            return Encoding.Default.GetString(_stringBuffer, 0, count);
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
                for (int i = 0; i < _fileIndex.Index.Length; ++i)
                {
                    SkillInfo skill = (i < _skillEntries.Count) ? _skillEntries[i] : null;
                    if (skill == null)
                    {
                        binidx.Write(-1); // lookup
                        binidx.Write(0); // length
                        binidx.Write(0); // extra
                    }
                    else
                    {
                        binidx.Write((int)fsmul.Position); // lookup
                        var length = (int)fsmul.Position;
                        binmul.Write(skill.IsAction);

                        byte[] nameBytes = Encoding.Default.GetBytes(skill.Name);
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

        public SkillInfo(int nr, string name, bool action, int extra)
        {
            Index = nr;
            _name = name;
            IsAction = action;
            Extra = extra;
        }
    }
}
