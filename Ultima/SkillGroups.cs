using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ultima
{
    public sealed class SkillGroup
    {
        public string Name { get; }

        public SkillGroup(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// skillgrp.mul: a group count, the names of groups 1..count-1 (group 0, Misc, has no stored name),
    /// then one int32 group id per skill. The order of skills inside a group is not stored.
    /// </summary>
    public sealed class SkillGroups
    {
        /// <summary>
        /// Skill entries the client reads and writes, whatever skills.idx holds (measured on client 7.0.117.0).
        /// Skills past this are not shown in the client's skill groups.
        /// </summary>
        public const int ClientSkillLimit = 58;

        /// <summary>
        /// Groups the client allows, Misc included. Its loader does not check this, so a file with more
        /// groups overruns the client's name table (measured on client 7.0.117.0).
        /// </summary>
        public const int ClientGroupLimit = 12;

        /// <summary>
        /// Longest group name: names are 17-character slots that the client reads up to a NUL.
        /// </summary>
        public const int MaxNameLength = 16;

        private const int _nameSlot = 17;

        /// <summary>
        /// Groups in file order; index 0 is always Misc.
        /// </summary>
        public static List<SkillGroup> List { get; private set; }

        /// <summary>
        /// Group index of each skill, by skill id.
        /// </summary>
        public static List<int> SkillList { get; private set; }

        /// <summary>
        /// True when the file stores group names in 16-bit slots (count preceded by -1). This is not Unicode
        /// support: the client keeps only the low byte of each character and saves the file back as 8-bit.
        /// Names should be ASCII in either form.
        /// </summary>
        public static bool Unicode { get; private set; }

        static SkillGroups()
        {
            Initialize();
        }

        public static void Initialize()
        {
            string path = Files.GetFilePath("skillgrp.mul");

            List = new List<SkillGroup> { new SkillGroup("Misc") };
            SkillList = new List<int>();
            Unicode = false;

            if (path == null)
            {
                return;
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bin = new BinaryReader(fs))
            {
                if (fs.Length < 4)
                {
                    return;
                }

                int start = 4;
                int count = bin.ReadInt32();

                if (count == -1)
                {
                    Unicode = true;
                    count = bin.ReadInt32();
                    start = 8;
                }

                int charSize = Unicode ? 2 : 1;
                int slotSize = _nameSlot * charSize;
                int names = Math.Max(0, count - 1);

                for (int i = 0; i < names; ++i)
                {
                    long offset = start + ((long)i * slotSize);
                    if (offset + slotSize > fs.Length)
                    {
                        break;
                    }

                    fs.Seek(offset, SeekOrigin.Begin);

                    var builder = new StringBuilder(_nameSlot);
                    for (int c = 0; c < _nameSlot; ++c)
                    {
                        int ch = Unicode ? bin.ReadUInt16() : bin.ReadByte();
                        if (ch == 0)
                        {
                            break;
                        }

                        builder.Append((char)ch);
                    }

                    List.Add(new SkillGroup(builder.ToString()));
                }

                long skillsStart = start + ((long)names * slotSize);
                if (skillsStart >= fs.Length)
                {
                    return;
                }

                fs.Seek(skillsStart, SeekOrigin.Begin);
                while (fs.Length - fs.Position >= 4)
                {
                    SkillList.Add(bin.ReadInt32());
                }
            }
        }

        public static void Save(string path)
        {
            string mul = Path.Combine(path, "skillgrp.mul");

            using (var fs = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                if (Unicode)
                {
                    bin.Write(-1);
                }

                bin.Write(List.Count);

                // Group 0 is Misc and has no stored name
                for (int i = 1; i < List.Count; ++i)
                {
                    bin.Write(EncodeName(List[i].Name));
                }

                foreach (int group in SkillList)
                {
                    bin.Write(group);
                }
            }
        }

        private static byte[] EncodeName(string name)
        {
            var slot = new byte[_nameSlot * (Unicode ? 2 : 1)];
            if (string.IsNullOrEmpty(name))
            {
                return slot;
            }

            if (name.Length > MaxNameLength)
            {
                name = name.Substring(0, MaxNameLength);
            }

            // 8-bit names are written back byte for byte as loaded (Latin-1)
            byte[] bytes = Unicode ? Encoding.Unicode.GetBytes(name) : Encoding.Latin1.GetBytes(name);
            bytes.CopyTo(slot, 0);

            return slot;
        }
    }
}
