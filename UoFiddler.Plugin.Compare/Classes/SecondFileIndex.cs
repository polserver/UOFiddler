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

using System;
using System.IO;
using System.Threading;

namespace UoFiddler.Plugin.Compare.Classes
{
    public sealed class SecondFileIndex : IDisposable
    {
        private readonly string _mulPath;
        private readonly Lock _entryWriteLock = new();

        public SecondIFileAccessor FileAccessor { get; }

        public long IdxLength => FileAccessor?.IdxLength ?? 0;
        public int IndexLength => FileAccessor?.IndexLength ?? 0;

        /// <summary>
        /// Entry accessor. The accessor itself does the SecondEntry3D / SecondEntry6D cast, because
        /// only it knows which one it stores.
        /// </summary>
        public SecondIEntry this[int index]
        {
            get => FileAccessor?[index];
            set
            {
                if (FileAccessor != null)
                {
                    FileAccessor[index] = value;
                }
            }
        }

        /// <summary>
        /// Persists dimensions discovered by actually decoding an entry back into the index, so a
        /// later lookup does not have to decode it again.
        /// </summary>
        /// <remarks>
        /// <see cref="Seek(int, ref SecondIEntry)"/> and the indexer hand out a <b>boxed copy</b> of
        /// the entry, so assigning to <c>entry.Extra1</c> on that copy is discarded - write-back has to
        /// go through here. Callers only ever pass values read out of the payload, so the lock is only
        /// there to stop two threads tearing the struct mid-write.
        /// </remarks>
        public void CacheDimensions(int index, int width, int height)
        {
            if (FileAccessor == null || index < 0 || index >= FileAccessor.IndexLength)
            {
                return;
            }

            lock (_entryWriteLock)
            {
                SecondIEntry entry = FileAccessor[index];
                if (entry == null)
                {
                    return;
                }

                entry.Extra1 = width;
                entry.Extra2 = height;
                FileAccessor[index] = entry;
            }
        }

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

            FileStream stream = EnsureOpen();
            if (stream == null)
            {
                length = extra = 0;
                return null;
            }

            if (stream.Length < e.Lookup)
            {
                length = extra = 0;
                return null;
            }

            stream.Seek(e.Lookup, SeekOrigin.Begin);
            return stream;
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

            FileStream stream = EnsureOpen();
            if (stream == null)
            {
                return null;
            }

            if (stream.Length < e.Lookup)
            {
                return null;
            }

            stream.Seek(e.Lookup, SeekOrigin.Begin);
            return stream;
        }

        /// <summary>
        /// Returns the cached FileAccessor.Stream, re-opening it only when genuinely required (null or
        /// disposed). Replaces the per-call CanRead/CanSeek probe that used to be duplicated in every
        /// Seek/Valid overload.
        /// </summary>
        private FileStream EnsureOpen()
        {
            FileStream stream = FileAccessor.Stream;
            if (stream != null && stream.CanRead && stream.CanSeek)
            {
                return stream;
            }

            if (_mulPath == null)
            {
                FileAccessor.Stream = null;
                return null;
            }

            stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            FileAccessor.Stream = stream;
            return stream;
        }

        /// <summary>
        /// Releases the underlying .mul / .uop FileStream so the next access re-opens fresh. Additive -
        /// a stale reference to a disposed index keeps working because <see cref="EnsureOpen"/> handles
        /// a disposed FileAccessor.Stream gracefully.
        /// </summary>
        public void Dispose()
        {
            FileAccessor?.Stream?.Dispose();
            if (FileAccessor != null)
            {
                FileAccessor.Stream = null;
            }
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

            FileStream stream = EnsureOpen();
            if (stream == null)
            {
                length = extra = 0;
                return false;
            }

            if (stream.Length < e.Lookup)
            {
                length = extra = 0;
                return false;
            }

            return true;
        }
    }
}
