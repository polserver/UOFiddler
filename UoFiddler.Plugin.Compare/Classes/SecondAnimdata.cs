using System;
using System.Collections.Generic;
using System.IO;
using Ultima;

namespace UoFiddler.Plugin.Compare.Classes
{
    internal static class SecondAnimdata
    {
        private static Dictionary<int, Animdata.AnimdataEntry> _data;

        public static bool IsLoaded => _data != null;

        public static Animdata.AnimdataEntry GetAnimData(int id) =>
            _data != null && _data.TryGetValue(id, out Animdata.AnimdataEntry value) ? value : null;

        public static IEnumerable<int> GetKeys() =>
            _data != null ? (IEnumerable<int>)_data.Keys : Array.Empty<int>();

        /// <summary>
        /// Reads an animdata.mul from the given file path. Returns false if the file
        /// cannot be read. On success the internal dictionary is replaced.
        /// </summary>
        public static bool Initialize(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var result = new Dictionary<int, Animdata.AnimdataEntry>();

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var bin = new BinaryReader(fs))
                {
                    unsafe
                    {
                        int id = 0;
                        long chunkCount = bin.BaseStream.Length / (4 + (8 * (64 + 4)));

                        for (long h = 0; h < chunkCount; h++)
                        {
                            bin.ReadInt32(); // chunk header (discarded)

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
                                        result[id] = new Animdata.AnimdataEntry(frame, unk, frameCount, frameInterval, frameStart);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            _data = result;
            return true;
        }
    }
}
