using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ComparePlugin
{
    public sealed class SecondFileIndex
    {
        public Entry3D[] Index { get; private set; }
        public Stream Stream { get; private set; }
        public long IdxLength { get; private set; }
        private string MulPath;

        public Stream Seek(int index, out int length, out int extra)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                return null;
            }

            Entry3D e = Index[index];

            if (e.lookup < 0)
            {
                length = extra = 0;
                return null;
            }

            length = e.length & 0x7FFFFFFF;
            extra = e.extra;

            if ((Stream == null) || (!Stream.CanRead) || (!Stream.CanSeek))
            {
                if (MulPath == null)
                    Stream = null;
                else
                    Stream = new FileStream(MulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream == null)
            {
                length = extra = 0;
                return null;
            }

            Stream.Seek(e.lookup, SeekOrigin.Begin);
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

            if (e.lookup < 0)
            {
                length = extra = 0;
                return false;
            }
            if (e.length < 0)
            {
                length = extra = 0;
                return false;
            }

            length = e.length & 0x7FFFFFFF;
            extra = e.extra;

            if ((MulPath == null) || !File.Exists(MulPath))
            {
                length = extra = 0;
                return false;
            }

            if ((Stream == null) || (!Stream.CanRead) || (!Stream.CanSeek))
            {
                Stream = new FileStream(MulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream.Length < e.lookup)
            {
                length = extra = 0;
                return false;
            }
            return true;
        }

        public SecondFileIndex(string idxFile, string mulFile, int length)
        {
            Index = new Entry3D[length];

            MulPath = mulFile;
            if (!File.Exists(idxFile))
                idxFile = null;
            if (!File.Exists(MulPath))
                MulPath = null;

            if (idxFile != null && MulPath != null)
            {
                using (FileStream index = new FileStream(idxFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream = new FileStream(MulPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                    int count = (int)(index.Length / 12);
                    IdxLength = index.Length;

                    GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                    byte[] buffer = new byte[index.Length];
                    index.Read(buffer, 0, (int)index.Length);
                    Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)Math.Min(IdxLength, length * 12));
                    gc.Free();

                    for (int i = count; i < length; ++i)
                    {
                        Index[i].lookup = -1;
                        Index[i].length = -1;
                        Index[i].extra = -1;
                    }
                }
            }
            else
            {
                Stream = null;
                return;
            }
        }
    }

    public struct Entry3D
    {
        public int lookup;
        public int length;
        public int extra;
    }
}
