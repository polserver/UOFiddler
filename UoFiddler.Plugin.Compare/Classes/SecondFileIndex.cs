/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.IO;

namespace UoFiddler.Plugin.Compare.Classes
{
    public sealed class SecondFileIndex
    {
        private readonly string _mulPath;

        public SecondIFileAccessor FileAccessor { get; }

        public long IdxLength => FileAccessor?.IdxLength ?? 0;
        public int IndexLength => FileAccessor?.IndexLength ?? 0;

        public SecondFileIndex(string idxFile, string mulFile, int length)
            : this(idxFile, mulFile, null, length, ".dat", -1, false)
        {
        }

        public SecondFileIndex(string idxFile, string mulFile, string uopFile, int length,
                               string uopEntryExtension, int idxLength, bool hasExtra)
        {
            string idxPath = string.IsNullOrEmpty(idxFile) || !File.Exists(idxFile) ? null : idxFile;
            string mulPath = string.IsNullOrEmpty(mulFile) || !File.Exists(mulFile) ? null : mulFile;
            string uopPath = string.IsNullOrEmpty(uopFile) || !File.Exists(uopFile) ? null : uopFile;

            if (uopPath != null)
            {
                FileAccessor = new SecondUopFileAccessor(uopPath, uopEntryExtension, length, idxLength, hasExtra);
                _mulPath = uopPath;
                return;
            }

            if (idxPath != null && mulPath != null)
            {
                FileAccessor = new SecondMulFileAccessor(idxPath, mulPath, length);
                _mulPath = mulPath;
                return;
            }

            FileAccessor = null;
            _mulPath = null;
        }

        public Stream Seek(int index, out int length, out int extra)
        {
            length = extra = 0;
            if (FileAccessor == null || index < 0 || index >= FileAccessor.IndexLength)
            {
                return null;
            }

            SecondIEntry e = FileAccessor.GetEntry(index);

            if (e.Lookup < 0 || (e.Lookup > 0 && e.Length == -1))
            {
                return null;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            if (e.Length < 0)
            {
                length = extra = 0;
                return null;
            }

            if (FileAccessor.Stream?.CanRead != true || !FileAccessor.Stream.CanSeek)
            {
                FileAccessor.Stream = _mulPath == null
                    ? null
                    : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (FileAccessor.Stream == null)
            {
                length = extra = 0;
                return null;
            }

            if (FileAccessor.Stream.Length < e.Lookup)
            {
                length = extra = 0;
                return null;
            }

            FileAccessor.Stream.Seek(e.Lookup, SeekOrigin.Begin);
            return FileAccessor.Stream;
        }

        public Stream Seek(int index, ref SecondIEntry entry)
        {
            if (FileAccessor == null || index < 0 || index >= FileAccessor.IndexLength)
            {
                return null;
            }

            SecondIEntry e = FileAccessor.GetEntry(index);

            if (e.Lookup < 0)
            {
                return null;
            }

            int length = e.Length & 0x7FFFFFFF;
            if (length < 0)
            {
                return null;
            }

            entry = e;

            if (e.Length < 0)
            {
                return null;
            }

            if (FileAccessor.Stream?.CanRead != true || !FileAccessor.Stream.CanSeek)
            {
                FileAccessor.Stream = _mulPath == null
                    ? null
                    : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (FileAccessor.Stream == null)
            {
                return null;
            }

            if (FileAccessor.Stream.Length < e.Lookup)
            {
                return null;
            }

            FileAccessor.Stream.Seek(e.Lookup, SeekOrigin.Begin);
            return FileAccessor.Stream;
        }

        public bool Valid(int index, out int length, out int extra)
        {
            length = extra = 0;
            if (FileAccessor == null || index < 0 || index >= FileAccessor.IndexLength)
            {
                return false;
            }

            SecondIEntry e = FileAccessor.GetEntry(index);

            if (e.Lookup < 0)
            {
                return false;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            if (e.Length < 0)
            {
                length = extra = 0;
                return false;
            }

            if (_mulPath == null || !File.Exists(_mulPath))
            {
                length = extra = 0;
                return false;
            }

            if (FileAccessor.Stream?.CanRead != true || !FileAccessor.Stream.CanSeek)
            {
                FileAccessor.Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (FileAccessor.Stream.Length < e.Lookup)
            {
                length = extra = 0;
                return false;
            }

            return true;
        }
    }
}
