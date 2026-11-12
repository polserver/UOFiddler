using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ultima
{
    public sealed class Animdata
    {
        private const int _recordSize = 64 + 4;

        private static int[] _header;
        private static byte[] _unknown;

        // Records with a frame count of 0 that still hold data. They are not entries, but Save writes
        // them back unchanged.
        private static Dictionary<int, byte[]> _emptyRecords = new Dictionary<int, byte[]>();

        public static Dictionary<int, AnimdataEntry> AnimData { get; set; }

        /// <summary>
        /// Entry slots in the loaded file, 8 per chunk. The last chunk can be short.
        /// </summary>
        public static int SlotCount { get; private set; }

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
            _emptyRecords = new Dictionary<int, byte[]>();
            _header = [];
            _unknown = null;
            SlotCount = 0;

            string path = Files.GetFilePath("animdata.mul");
            if (path == null)
            {
                return;
            }

            byte[] file = File.ReadAllBytes(path);
            var headers = new List<int>();
            int pos = 0;
            int id = 0;

            // Chunks are a 4 byte header and 8 records. The last chunk can hold fewer records,
            // and can end in a part of a record, which is kept as is.
            while (file.Length - pos >= 4 + _recordSize)
            {
                headers.Add(BitConverter.ToInt32(file, pos));
                pos += 4;

                for (int i = 0; i < 8 && file.Length - pos >= _recordSize; ++i, ++id)
                {
                    ReadRecord(file, pos, id);
                    pos += _recordSize;
                }
            }

            _header = headers.ToArray();
            SlotCount = id;

            if (pos < file.Length)
            {
                _unknown = file.AsSpan(pos).ToArray();
            }
        }

        private static void ReadRecord(byte[] file, int pos, int id)
        {
            byte frameCount = file[pos + 65];
            if (frameCount == 0)
            {
                if (file.AsSpan(pos, _recordSize).ContainsAnyExcept((byte)0))
                {
                    _emptyRecords[id] = file.AsSpan(pos, _recordSize).ToArray();
                }

                return;
            }

            sbyte[] frame = new sbyte[64];
            for (int j = 0; j < 64; ++j)
            {
                frame[j] = (sbyte)file[pos + j];
            }

            AnimData[id] = new AnimdataEntry(frame, file[pos + 64], frameCount, file[pos + 66], file[pos + 67]);
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

            int maxId = AnimData.Count > 0 ? AnimData.Keys.Max() : -1;

            // Keep the loaded slot count, including a short last chunk. Growing the file rounds it up
            // to whole chunks, and the partial record that ended the old file no longer fits.
            int slots = SlotCount;
            bool grown = maxId >= slots;
            if (grown)
            {
                slots = (maxId + 8) & ~7;
            }

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                int h = 0;
                for (int id = 0; id < slots; ++id)
                {
                    if ((id & 7) == 0)
                    {
                        int headerChunk = h < _header.Length ? _header[h] : Random.Shared.Next();
                        ++h;
                        bin.Write(headerChunk);
                    }

                    AnimdataEntry animdataEntry = GetAnimData(id);
                    if (animdataEntry != null)
                    {
                        for (int j = 0; j < 64; ++j)
                        {
                            bin.Write(animdataEntry.FrameData[j]);
                        }

                        bin.Write(animdataEntry.Unknown);
                        bin.Write(animdataEntry.FrameCount);
                        bin.Write(animdataEntry.FrameInterval);
                        bin.Write(animdataEntry.FrameStart);
                    }
                    else if (_emptyRecords.TryGetValue(id, out byte[] raw))
                    {
                        bin.Write(raw);
                    }
                    else
                    {
                        bin.Write(new byte[_recordSize]);
                    }
                }

                if (_unknown != null && !grown)
                {
                    bin.Write(_unknown);
                }
            }
        }

        public class AnimdataEntry
        {
            public sbyte[] FrameData { get; set; }
            public byte Unknown { get; set; }
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
