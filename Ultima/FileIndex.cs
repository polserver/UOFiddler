using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ultima.Helpers;

namespace Ultima
{
    public sealed class FileIndex
    {
        public IFileAccessor FileAccessor { get; }

        public long IndexLength { get => FileAccessor?.IndexLength ?? 0; }
        public long IdxLength { get => FileAccessor?.IdxLength ?? 0; }
        public IEntry this[int index]
        {
            get => FileAccessor[index];
            set => FileAccessor[index] = (Entry6D)value;
        }

        private readonly string _mulPath;

        public FileIndex(string idxFile, string mulFile, int length, int file) : this(idxFile, mulFile, null, length,
            file, ".dat", -1, false)
        {
        }

        public FileIndex(string idxFile, string mulFile, string uopFile, int length, int file, string uopEntryExtension,
            int idxLength, bool hasExtra)
        {
            string idxPath = null;
            string uopPath = null;

            _mulPath = null;

            if (Files.MulPath == null)
            {
                Files.LoadMulPath();
            }

            if (Files.MulPath.Count > 0)
            {
                idxPath = Files.MulPath[idxFile.ToLower()];
                _mulPath = Files.MulPath[mulFile.ToLower()];

                if (!string.IsNullOrEmpty(uopFile) && Files.MulPath.ContainsKey(uopFile.ToLower()))
                {
                    uopPath = Files.MulPath[uopFile.ToLower()];
                }

                if (string.IsNullOrEmpty(idxPath))
                {
                    idxPath = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(idxPath)))
                    {
                        idxPath = Path.Combine(Files.RootDir, idxPath);
                    }

                    if (!File.Exists(idxPath))
                    {
                        idxPath = null;
                    }
                }

                if (string.IsNullOrEmpty(_mulPath))
                {
                    _mulPath = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(_mulPath)))
                    {
                        _mulPath = Path.Combine(Files.RootDir, _mulPath);
                    }

                    if (!File.Exists(_mulPath))
                    {
                        _mulPath = null;
                    }
                }

                if (!string.IsNullOrEmpty(uopPath))
                {
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(uopPath)))
                    {
                        uopPath = Path.Combine(Files.RootDir, uopPath);
                    }

                    if (File.Exists(uopPath))
                    {
                        _mulPath = uopPath;
                    }
                }
            }

            /* UOP files support code, written by Wyatt (c) www.ruosi.org
             * idxLength variable was added for compatibility with legacy code for art (see art.cs)
             * At the moment the only UOP file having entries with extra field is gumpartlegacy.uop,
             * and it's two DWORDs in the beginning of the entry.
             * It's possible that UOP can include some entries with unknown hash: not really unknown for me, but
             * not useful for reading legacy entries. That's why i removed unknown hash exception throwing from this code
             */
            if (_mulPath?.EndsWith(".uop") == true)
            {
                FileAccessor = new UopFileAccessor(_mulPath, uopEntryExtension, length, idxLength, hasExtra);
            }
            else if ((idxPath != null) && (_mulPath != null))
            {
                FileAccessor = new MulFileAccessor(idxPath, _mulPath, length);
            }
            else
            {
                return;
            }

            if (file <= -1)
            {
                return;
            }

            Entry5D[] verdataPatches = Verdata.Patches;
            foreach (var patch in verdataPatches)
            {
                if (patch.File != file || patch.Index < 0 || patch.Index >= length)
                {
                    continue;
                }

                FileAccessor.ApplyPatch(patch);
            }
        }

        public FileIndex(string idxFile, string mulFile, int file)
        {
            string idxPath = null;
            _mulPath = null;

            if (Files.MulPath == null)
            {
                Files.LoadMulPath();
            }

            if (Files.MulPath.Count > 0)
            {
                idxPath = Files.MulPath[idxFile.ToLower()];
                _mulPath = Files.MulPath[mulFile.ToLower()];
                if (string.IsNullOrEmpty(idxPath))
                {
                    idxPath = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(idxPath)))
                    {
                        idxPath = Path.Combine(Files.RootDir, idxPath);
                    }

                    if (!File.Exists(idxPath))
                    {
                        idxPath = null;
                    }
                }

                if (string.IsNullOrEmpty(_mulPath))
                {
                    _mulPath = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetDirectoryName(_mulPath)))
                    {
                        _mulPath = Path.Combine(Files.RootDir, _mulPath);
                    }

                    if (!File.Exists(_mulPath))
                    {
                        _mulPath = null;
                    }
                }
            }

            if ((idxPath != null) && (_mulPath != null))
            {
                FileAccessor = new MulFileAccessor(idxPath, _mulPath);
            }
            else
            {
                return;
            }

            if (file <= -1)
            {
                return;
            }

            foreach (var patch in Verdata.Patches)
            {
                if (patch.File != file || patch.Index < 0 || patch.Index >= FileAccessor.IndexLength)
                {
                    continue;
                }

                FileAccessor.ApplyPatch(patch);
            }
        }

        public Stream Seek(int index, out int length, out int extra, out bool patched)
        {
            if (FileAccessor is null)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            if (index < 0 || index >= FileAccessor.IndexLength)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            IEntry e = FileAccessor.GetEntry(index);

            if (e.Lookup < 0 || (e.Lookup > 0 && e.Length == -1))
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            if ((e.Length & (1 << 31)) != 0)
            {
                patched = true;
                Verdata.Seek(e.Lookup);
                return Verdata.Stream;
            }

            if (e.Length < 0)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            if ((FileAccessor.Stream?.CanRead != true) || (!FileAccessor.Stream.CanSeek))
            {
                FileAccessor.Stream = _mulPath == null ? null : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (FileAccessor.Stream == null)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            if (FileAccessor.Stream.Length < e.Lookup)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            patched = false;

            FileAccessor.Stream.Seek(e.Lookup, SeekOrigin.Begin);
            return FileAccessor.Stream;
        }

        public Stream Seek(int index, ref IEntry entry, out bool patched)
        {
            if (FileAccessor is null)
            {
                patched = false;
                return null;
            }

            if (index < 0 || index >= FileAccessor.IndexLength)
            {
                patched = false;
                return null;
            }

            IEntry e = FileAccessor.GetEntry(index);

            if (e.Lookup < 0)
            {
                patched = false;
                return null;
            }

            var length = e.Length & 0x7FFFFFFF;
            if (length < 0)
            {
                patched = false;
                return null;
            }

            entry = e;

            if ((e.Length & (1 << 31)) != 0)
            {
                patched = true;
                Verdata.Seek(e.Lookup);
                return Verdata.Stream;
            }

            if (e.Length < 0)
            {
                patched = false;
                return null;
            }

            if ((FileAccessor.Stream?.CanRead != true) || (!FileAccessor.Stream.CanSeek))
            {
                FileAccessor.Stream = _mulPath == null ? null : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (FileAccessor.Stream == null)
            {
                patched = false;
                return null;
            }

            if (FileAccessor.Stream.Length < e.Lookup)
            {
                patched = false;
                return null;
            }

            patched = false;

            FileAccessor.Stream.Seek(e.Lookup, SeekOrigin.Begin);
            return FileAccessor.Stream;
        }

        public bool Valid(int index, out int length, out int extra, out bool patched)
        {
            if (FileAccessor is null)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            if (index < 0 || index >= FileAccessor.IndexLength)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            IEntry e = FileAccessor.GetEntry(index);

            if (e.Lookup < 0)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            length = e.Length & 0x7FFFFFFF;
            extra = e.Extra;

            if ((e.Length & (1 << 31)) != 0)
            {
                patched = true;
                return true;
            }

            if (e.Length < 0)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            if ((_mulPath == null) || !File.Exists(_mulPath))
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            if ((FileAccessor.Stream?.CanRead != true) || (!FileAccessor.Stream.CanSeek))
            {
                FileAccessor.Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (FileAccessor.Stream.Length < e.Lookup)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            patched = false;

            return true;
        }
    }

    public enum CompressionFlag
    {
        None = 0,
        Zlib = 1,
        Mythic = 3
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry3D : IEntry
    {
        // do not mess with the fields struct layout in memory is important because of how we read the index files.
        public int lookup;
        public int length;
        public int extra;

#pragma warning disable S2292
        public int Lookup { get => lookup; set => lookup = value; }

        public int Length { get => length; set => length = value; }

        public int Extra { get => extra; set => extra = value; }

        public int DecompressedLength { get => length; set => length = value; }
#pragma warning restore S2292

        public int Extra1
        {
            get => (int)((Extra & 0xFFFF0000) >> 16);
            set => Extra = Extra & 0x0000FFFF | (value << 16);
        }

        public int Extra2
        {
            get => Extra & 0x0000FFFF;
            set => Extra = (int)((Extra & 0xFFFF0000) | (uint)value);
        }

        public CompressionFlag Flag { get => CompressionFlag.None; set { } } // No compression, means that we have only three first fields
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry6D : IEntry
    {
        public IEntry Invalid { get => new Entry6D(); }

        public int Lookup { get; set; }

        public int Length { get; set; }

        private int extra1;
        private int extra2;

        public int Extra
        {
            get => extra1 << 16 | extra2;
            set
            {
                extra1 = value & 0x0000FFFF;
                extra2 = (int)((value & 0xFFFF0000) >> 16);
            }
        }

        public int DecompressedLength { get; set; }

        public int Extra1 { get; set; }

        public int Extra2 { get; set; }

        public CompressionFlag Flag { get; set; }
    }

    // Dumb access to all possible fields of entries
    public interface IEntry
    {
        public int Lookup { get; set; }
        public int Length { get; set; }
        public int Extra { get; set; }
        public int DecompressedLength { get; set; }
        public int Extra1 { get; set; }
        public int Extra2 { get; set; }
        public CompressionFlag Flag { get; set; }
    }

    public interface IFileAccessor
    {
        public IEntry GetEntry(int index);
        void ApplyPatch(Entry5D patch);
        public FileStream Stream { get; set; }
        public int IndexLength { get; }
        public long IdxLength { get; }
        public IEntry this[int index] { get; set; }
    }

    public class MulFileAccessor : IFileAccessor
    {
        public Entry3D[] Index { get; }

        public long IdxLength { get; }

        public FileStream Stream { get; set; }

        public int IndexLength { get => Index.Length; }

        public IEntry this[int index] { get => Index[index]; set => Index[index] = (Entry3D)value; }

        public MulFileAccessor(string idxPath, string path, int length)
        {
            Index = new Entry3D[length];

            using (var index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var count = (int)(index.Length / 12);
                IdxLength = index.Length;
                GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                var buffer = new byte[index.Length];
                index.ReadExactly(buffer, 0, (int)index.Length);
                Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)Math.Min(IdxLength, Index.Length * 12));
                gc.Free();
                for (int i = count; i < Index.Length; ++i)
                {
                    Index[i].Lookup = -1;
                    Index[i].Length = -1;
                    Index[i].Extra = -1;
                }
            }
        }

        public MulFileAccessor(string idxPath, string path)
        {
            using (var index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var count = (int)(index.Length / 12);
                IdxLength = index.Length;
                Index = new Entry3D[count];
                GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                var buffer = new byte[index.Length];
                index.ReadExactly(buffer, 0, (int)index.Length);
                Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)index.Length);
                gc.Free();
            }
        }

        public void ApplyPatch(Entry5D patch)
        {
            Index[patch.Index].Lookup = patch.Lookup;
            Index[patch.Index].Length = patch.Length | (1 << 31);
            Index[patch.Index].Extra = patch.Extra;
        }

        public IEntry GetEntry(int index)
        {
            if (index < 0 || index >= Index.Length)
            {
                return new Entry3D();
            }

            return Index[index];
        }
    }

    public class UopFileAccessor : IFileAccessor
    {
        public Entry6D[] Index { get; }

        public FileStream Stream { get; set; }

        public long IdxLength { get; }

        public int IndexLength { get => Index.Length; }

        public IEntry this[int index]
        {
            get => Index[index];
            set => Index[index] = (Entry6D)value;
        }

        public UopFileAccessor(string path, string uopEntryExtension, int length, int idxLength, bool hasextra)
        {
            Index = new Entry6D[length];

            if (idxLength > 0)
            {
                IdxLength = idxLength * 12;
            }

            Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var fileInfo = new FileInfo(path);
            string uopPattern = fileInfo.Name.Replace(fileInfo.Extension, "").ToLowerInvariant();

            using (var br = new BinaryReader(Stream))
            {
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                if (br.ReadInt32() != 0x50594D)
                {
                    throw new ArgumentException("Bad UOP file.");
                }

                _ = br.ReadUInt32(); // version
                _ = br.ReadUInt32(); // signature
                long nextBlock = br.ReadInt64();
                _ = br.ReadUInt32(); // block size (capacity?)
                _ = br.ReadInt32(); // count 

                var hashes = new Dictionary<ulong, int>();

                for (int i = 0; i < length; i++)
                {
                    string entryName = $"build/{uopPattern}/{i:D8}{uopEntryExtension}";
                    ulong hash = UopUtils.HashFileName(entryName);

                    hashes.TryAdd(hash, i);
                }

                br.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

                // There are no invalid entries in .uop so we have to initialize all entries
                // as invalid and then fill the valid ones
                for (var i = 0; i < Index.Length; i++)
                {
                    Index[i].Lookup = -1;
                    Index[i].Length = -1;
                    Index[i].Extra = -1;
                }

                do
                {
                    int filesCount = br.ReadInt32();
                    nextBlock = br.ReadInt64();

                    for (int i = 0; i < filesCount; i++)
                    {
                        long offset = br.ReadInt64();
                        int headerLength = br.ReadInt32();
                        int compressedLength = br.ReadInt32();
                        int decompressedLength = br.ReadInt32();
                        ulong hash = br.ReadUInt64();
                        _ = br.ReadUInt32(); // data_hash
                        short flag = br.ReadInt16();

                        if (offset == 0)
                        {
                            continue;
                        }

                        if (!hashes.TryGetValue(hash, out int idx))
                        {
                            continue;
                        }

                        if (idx < 0 || idx > Index.Length)
                        {
                            throw new IndexOutOfRangeException("hashes dictionary and files collection have different count of entries!");
                        }

                        offset += headerLength;

                        if (hasextra && flag != 3)
                        {
                            long curPos = br.BaseStream.Position;

                            br.BaseStream.Seek(offset, SeekOrigin.Begin);

                            var extra1 = br.ReadInt32();
                            var extra2 = br.ReadInt32();
                            Index[idx].Lookup = (int)(offset + 8);
                            Index[idx].Length = compressedLength - 8;
                            Index[idx].DecompressedLength = decompressedLength;
                            Index[idx].Flag = (CompressionFlag)flag;
                            Index[idx].Extra = extra1 << 16 | extra2;
                            Index[idx].Extra1 = extra1;
                            Index[idx].Extra2 = extra2;

                            br.BaseStream.Seek(curPos, SeekOrigin.Begin);
                        }
                        else
                        {
                            Index[idx].Lookup = (int)(offset);
                            Index[idx].Length = compressedLength;
                            Index[idx].DecompressedLength = decompressedLength;
                            Index[idx].Flag = (CompressionFlag)flag;
                            Index[idx].Extra = 0x0FFFFFFF; // we cant read it right now, but -1 and 0 makes this entry invalid
                        }
                    }
                }
                while (br.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
            }
        }

        public void ApplyPatch(Entry5D patch)
        {
            Index[patch.Index].Lookup = patch.Lookup;
            Index[patch.Index].Length = patch.Length | (1 << 31);
            Index[patch.Index].Extra = patch.Extra;
        }

        public IEntry GetEntry(int index)
        {
            if (index < 0 || index >= Index.Length)
            {
                return new Entry6D();
            }

            return Index[index];
        }
    }
}