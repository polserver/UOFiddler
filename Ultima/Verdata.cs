using System;
using System.IO;

/*
    FileIDs
    --------------
     0 - map0.mul
     1 - staidx0.mul
     2 - statics0.mul
     3 - artidx.mul
     4 - art.mul
     5 - anim.idx
     6 - anim.mul
     7 - soundidx.mul
     8 - sound.mul
     9 - texidx.mul
    10 - texmaps.mul
    11 - gumpidx.mul
    12 - gumpart.mul
    13 - multi.idx
    14 - multi.mul
    15 - skills.idx
    16 - skills.mul
    30 - tiledata.mul
    31 - animdata.mul
*/

namespace Ultima
{
    public sealed class Verdata
    {
        public static Stream Stream { get; private set; }
        public static Entry5D[] Patches { get; private set; }

        private static string _path;

        static Verdata()
        {
            Initialize();
        }

        public static void Initialize()
        {
            _path = Files.GetFilePath("verdata.mul");
            if (_path == null)
            {
                Patches = Array.Empty<Entry5D>();
                Stream = Stream.Null;
            }
            else
            {
                using (Stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var bin = new BinaryReader(Stream))
                {
                    Patches = new Entry5D[bin.ReadInt32()];

                    for (int i = 0; i < Patches.Length; ++i)
                    {
                        Patches[i].File = bin.ReadInt32();
                        Patches[i].Index = bin.ReadInt32();
                        Patches[i].Lookup = bin.ReadInt32();
                        Patches[i].Length = bin.ReadInt32();
                        Patches[i].Extra = bin.ReadInt32();
                    }
                }

                Stream.Close();
            }
        }

        private static readonly object _readLock = new object();

        /// <summary>
        /// Reads a patch into a stream of its own. Every client file shares the one verdata stream,
        /// so seek and read are done under a lock and callers never touch the shared position.
        /// </summary>
        /// <remarks>
        /// A short or missing verdata.mul gives a shorter stream; the caller's read then fails the
        /// same way it did when reading the shared stream directly.
        /// </remarks>
        public static Stream ReadPatch(int lookup, int length)
        {
            if (length <= 0)
            {
                return new MemoryStream(Array.Empty<byte>(), false);
            }

            var buffer = new byte[length];
            int read;
            lock (_readLock)
            {
                Seek(lookup);
                read = Stream.ReadAtLeast(buffer, length, throwOnEndOfStream: false);
            }

            return new MemoryStream(buffer, 0, read, false);
        }

        public static void Seek(int lookup)
        {
            if ((Stream?.CanRead != true || !Stream.CanSeek) && _path != null)
            {
                Stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            _ = Stream.Seek(lookup, SeekOrigin.Begin);
        }
    }

    public struct Entry5D
    {
        public int File;
        public int Index;
        public int Lookup;
        public int Length;
        public int Extra;
    }
}