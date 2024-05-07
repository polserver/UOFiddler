using System.Collections.Generic;
using System.IO;

namespace Ultima
{
    public sealed class Animdata
    {
        private static int[] _header;
        private static byte[] _unknown;

        public static Dictionary<int, AnimdataEntry> AnimData { get; private set; }

        static Animdata()
        {
            Initialize();
        }

        /// <summary>
        /// Reads animdata.mul and fills <see cref="AnimData"/>
        /// </summary>
        public static void Initialize()
        {
            AnimData = new Dictionary<int, AnimdataEntry>();

            string path = Files.GetFilePath("animdata.mul");
            if (path == null)
            {
                return;
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bin = new BinaryReader(fs))
            {
                unsafe
                {
                    int id = 0;
                    int h = 0;

                    _header = new int[bin.BaseStream.Length / (4 + (8 * (64 + 4)))];

                    while (h < _header.Length)
                    {
                        _header[h++] = bin.ReadInt32(); // chunk header

                        // Read 8 tiles
                        byte[] buffer = bin.ReadBytes(544);

                        fixed (byte* buf = buffer)
                        {
                            byte* data = buf;

                            for (int i = 0; i < 8; ++i, ++id)
                            {
                                sbyte[] frame = new sbyte[64];

                                for (int j = 0; j < 64; ++j)
                                {
                                    frame[j] = (sbyte)*data++;
                                }

                                byte unk = *data++;
                                byte frameCount = *data++;
                                byte frameInterval = *data++;
                                byte frameStart = *data++;

                                if (frameCount > 0)
                                {
                                    AnimData[id] = new AnimdataEntry(frame, unk, frameCount, frameInterval, frameStart);
                                }
                            }
                        }
                    }

                    var remaining = (int)(bin.BaseStream.Length - bin.BaseStream.Position);
                    if (remaining > 0)
                    {
                        _unknown = bin.ReadBytes(remaining);
                    }
                }
            }
        }

        /// <summary>
        /// Gets Animation <see cref="AnimdataEntry"/>
        /// </summary>
        /// <param name="id"></param>
        public static AnimdataEntry GetAnimData(int id)
        {
            return AnimData.TryGetValue(id, out AnimdataEntry value) ? value : null;
        }

        public static void Save(string path)
        {
            string fileName = Path.Combine(path, "animdata.mul");

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                int id = 0;
                int h = 0;
                while (id < _header.Length * 8)
                {
                    bin.Write(_header[h++]);
                    for (int i = 0; i < 8; ++i, ++id)
                    {
                        AnimdataEntry animdataEntry = GetAnimData(id);
                        for (int j = 0; j < 64; ++j)
                        {
                            if (animdataEntry != null)
                            {
                                bin.Write(animdataEntry.FrameData[j]);
                            }
                            else
                            {
                                bin.Write((sbyte)0);
                            }
                        }

                        if (animdataEntry != null)
                        {
                            bin.Write(animdataEntry.Unknown);
                            bin.Write(animdataEntry.FrameCount);
                            bin.Write(animdataEntry.FrameInterval);
                            bin.Write(animdataEntry.FrameStart);
                        }
                        else
                        {
                            bin.Write((byte)0);
                            bin.Write((byte)0);
                            bin.Write((byte)0);
                            bin.Write((byte)0);
                        }
                    }
                }

                if (_unknown != null)
                {
                    bin.Write(_unknown);
                }
            }
        }

        public class AnimdataEntry
        {
            public sbyte[] FrameData { get; set; }
            public byte Unknown { get; }
            public byte FrameCount { get; set; }
            public byte FrameInterval { get; set; }
            public byte FrameStart { get; set; }

            // Empty constructor needed for deserialization.
            public AnimdataEntry() { }

            public AnimdataEntry(sbyte[] frame, byte unk, byte frameCount, byte frameInterval, byte frameStart)
            {
                FrameData = frame;
                Unknown = unk;
                FrameCount = frameCount;
                FrameInterval = frameInterval;
                FrameStart = frameStart;
            }
        }
    }
}
