using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UoFiddler.Plugin.Compare.Classes
{
    public sealed class SecondFileIndex
    {
        private readonly string _mulPath;
        private Entry3D[] Index { get; }
        private Stream Stream { get; set; }

        public long IdxLength { get; }

        public Stream Seek(int index, out int length, out int extra)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                return null;
            }

            Entry3D e = Index[index];

            if (e.Lookup < 0)
            {
                length = extra = 0;
                return null;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            // TODO: possible bug wrong ternary or condition?
            if (Stream?.CanRead != true || !Stream.CanSeek)
            {
                Stream = _mulPath == null
                    ? null
                    : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream == null)
            {
                length = extra = 0;
                return null;
            }

            Stream.Seek(e.Lookup, SeekOrigin.Begin);
            return Stream;
        }

        public bool Valid(int index, out int length, out int extra)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                return false;
            }

            Entry3D e = Index[index];

            if (e.Lookup < 0)
            {
                length = extra = 0;
                return false;
            }
            if (e.Length < 0)
            {
                length = extra = 0;
                return false;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            if (_mulPath == null || !File.Exists(_mulPath))
            {
                length = extra = 0;
                return false;
            }

            if (Stream?.CanRead != true || !Stream.CanSeek)
            {
                Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream.Length >= e.Lookup)
            {
                return true;
            }

            length = extra = 0;
            return false;
        }

        public SecondFileIndex(string idxFile, string mulFile, int length)
        {
            Index = new Entry3D[length];

            _mulPath = mulFile;
            if (!File.Exists(idxFile))
            {
                idxFile = null;
            }

            if (!File.Exists(_mulPath))
            {
                _mulPath = null;
            }

            if (idxFile != null && _mulPath != null)
            {
                using (FileStream index = new FileStream(idxFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    int count = (int)(index.Length / 12);
                    IdxLength = index.Length;

                    GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                    byte[] buffer = new byte[index.Length];
                    index.Read(buffer, 0, (int)index.Length);
                    Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)Math.Min(IdxLength, length * 12));
                    gc.Free();

                    for (int i = count; i < length; ++i)
                    {
                        Index[i].Lookup = -1;
                        Index[i].Length = -1;
                        Index[i].Extra = -1;
                    }
                }
            }
            else
            {
                Stream = null;
            }
        }
    }

    public struct Entry3D
    {
        public int Lookup;
        public int Length;
        public int Extra;
    }
}
