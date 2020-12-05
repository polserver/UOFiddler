using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Ultima
{
    public sealed class Animdata
    {
        private static int[] _header;
        private static byte[] _unknown;

        public static Dictionary<int, Data> AnimData { get; private set; }

        static Animdata()
        {
            Initialize();
        }

        /// <summary>
        /// Reads animdata.mul and fills <see cref="AnimData"/>
        /// </summary>
        public static void Initialize()
        {
            AnimData = new Dictionary<int, Data>();

            string path = Files.GetFilePath("animdata.mul");
            if (path == null)
            {
                return;
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
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
                                    sbyte[] fdata = new sbyte[64];
                                    for (int j = 0; j < 64; ++j)
                                    {
                                        fdata[j] = (sbyte)*data++;
                                    }

                                    byte unk = *data++;
                                    byte fcount = *data++;
                                    byte finter = *data++;
                                    byte fstart = *data++;
                                    if (fcount > 0)
                                    {
                                        AnimData[id] = new Data(fdata, unk, fcount, finter, fstart);
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
        }

        /// <summary>
        /// Gets Animation <see cref="Data"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Data GetAnimData(int id)
        {
            return AnimData.ContainsKey(id) ? AnimData[id] : null;
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
                        Data data = GetAnimData(id);
                        for (int j = 0; j < 64; ++j)
                        {
                            if (data != null)
                            {
                                bin.Write(data.FrameData[j]);
                            }
                            else
                            {
                                bin.Write((sbyte)0);
                            }
                        }

                        if (data != null)
                        {
                            bin.Write(data.Unknown);
                            bin.Write(data.FrameCount);
                            bin.Write(data.FrameInterval);
                            bin.Write(data.FrameStart);
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

        public class Data
        {
            public sbyte[] FrameData { get; set; }
            public byte Unknown { get; }
            public byte FrameCount { get; set; }
            public byte FrameInterval { get; set; }
            public byte FrameStart { get; set; }

            public Data(sbyte[] frame, byte unk, byte fcount, byte finter, byte fstart)
            {
                FrameData = frame;
                Unknown = unk;
                FrameCount = fcount;
                FrameInterval = finter;
                FrameStart = fstart;
            }
        }
    }
}
