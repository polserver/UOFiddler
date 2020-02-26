using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima
{
    public sealed class Skills
    {
        private static FileIndex _mFileIndex = new FileIndex("skills.idx", "skills.mul", 16);

        private static List<SkillInfo> _mSkillEntries;
        public static List<SkillInfo> SkillEntries
        {
            get
            {
                if (_mSkillEntries == null)
                {
                    _mSkillEntries = new List<SkillInfo>();
                    for (int i = 0; i < _mFileIndex.Index.Length; ++i)
                    {
                        SkillInfo info = GetSkill(i);
                        if (info == null)
                            break;
                        _mSkillEntries.Add(info);
                    }
                }
                return _mSkillEntries;
            }
            set => _mSkillEntries = value;
        }

        public Skills()
        {

        }

        /// <summary>
        /// ReReads skills.mul
        /// </summary>
        public static void Reload()
        {
            _mFileIndex = new FileIndex("skills.idx", "skills.mul", 16);
            _mSkillEntries = new List<SkillInfo>();
            for (int i = 0; i < _mFileIndex.Index.Length; ++i)
            {
                SkillInfo info = GetSkill(i);
                if (info == null)
                    break;
                _mSkillEntries.Add(info);
            }
        }

        /// <summary>
        /// Returns <see cref="SkillInfo"/> of index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static SkillInfo GetSkill(int index)
        {
            int length, extra;
            bool patched;

            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);
            if (stream == null)
                return null;
            if (length == 0)
                return null;

            using (BinaryReader bin = new BinaryReader(stream))
            {
                bool action = bin.ReadBoolean();
                string name = ReadNameString(bin, length - 1);
                return new SkillInfo(index, name, action, extra);
            }
        }

        private static readonly byte[] _mStringBuffer = new byte[1024];
        private static string ReadNameString(BinaryReader bin, int length)
        {
            bin.Read(_mStringBuffer, 0, length);
            int count;
            for (count = 0; count < length && _mStringBuffer[count] != 0; ++count) ;

            return Encoding.Default.GetString(_mStringBuffer, 0, count);
        }

        public static void Save(string path)
        {
            string idx = Path.Combine(path, "skills.idx");
            string mul = Path.Combine(path, "skills.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int i = 0; i < _mFileIndex.Index.Length; ++i)
                    {
                        SkillInfo skill = (i < _mSkillEntries.Count) ? _mSkillEntries[i] : null;
                        if (skill == null)
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0); // length
                            binidx.Write(0); // extra
                        }
                        else
                        {
                            binidx.Write((int)fsmul.Position); //lookup
                            int length = (int)fsmul.Position;
                            binmul.Write(skill.IsAction);

                            byte[] namebytes = Encoding.Default.GetBytes(skill.Name);
                            binmul.Write(namebytes);
                            binmul.Write((byte)0); //nullterminated

                            length = (int)fsmul.Position - length;
                            binidx.Write(length);
                            binidx.Write(skill.Extra);
                        }
                    }
                }
            }
        }
    }

    public sealed class SkillInfo
    {
        private string _mName;

        public int Index { get; set; }
        public bool IsAction { get; set; }
        public string Name
        {
            get => _mName;
            set
            {
                if (value == null)
                    _mName = "";
                else
                    _mName = value;
            }
        }
        public int Extra { get; }


        public SkillInfo(int nr, string name, bool action, int extra)
        {
            Index = nr;
            _mName = name;
            IsAction = action;
            Extra = extra;
        }
    }
}
