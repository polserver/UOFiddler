using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ultima.Helpers;

namespace Ultima
{
    public sealed class FileIndex
    {
        public Entry3D[] Index { get; }
        public Stream Stream { get; private set; }
        public long IdxLength { get; }

        private readonly string _mulPath;

        public FileIndex(string idxFile, string mulFile, int length, int file) : this(idxFile, mulFile, null, length,
            file, ".dat", -1, false)
        {
        }

        public FileIndex(string idxFile, string mulFile, string uopFile, int length, int file, string uopEntryExtension,
            int idxLength, bool hasExtra)
        {
            Index = new Entry3D[length];

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
                Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                var fi = new FileInfo(_mulPath);
                string uopPattern = fi.Name.Replace(fi.Extension, "").ToLowerInvariant();

                using (var br = new BinaryReader(Stream))
                {
                    br.BaseStream.Seek(0, SeekOrigin.Begin);

                    if (br.ReadInt32() != 0x50594D)
                    {
                        throw new ArgumentException("Bad UOP file.");
                    }

                    br.ReadInt64(); // version + signature
                    long nextBlock = br.ReadInt64();
                    br.ReadInt32(); // block capacity
                    _ = br.ReadInt32(); // TODO: check if we need value from here

                    if (idxLength > 0)
                    {
                        IdxLength = idxLength * 12;
                    }

                    var hashes = new Dictionary<ulong, int>();

                    for (int i = 0; i < length; i++)
                    {
                        string entryName = $"build/{uopPattern}/{i:D8}{uopEntryExtension}";
                        ulong hash = UopUtils.HashFileName(entryName);

                        if (!hashes.ContainsKey(hash))
                        {
                            hashes.Add(hash, i);
                        }
                    }

                    br.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

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
                            br.ReadUInt32(); // Adler32
                            short flag = br.ReadInt16();

                            int entryLength = flag == 1 ? compressedLength : decompressedLength;

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

                            Index[idx].Lookup = (int)(offset + headerLength);
                            Index[idx].Length = entryLength;

                            if (!hasExtra)
                            {
                                continue;
                            }

                            long curPos = br.BaseStream.Position;

                            br.BaseStream.Seek(offset + headerLength, SeekOrigin.Begin);

                            byte[] extra = br.ReadBytes(8);

                            var extra1 = (short)((extra[3] << 24) | (extra[2] << 16) | (extra[1] << 8) | extra[0]);
                            var extra2 = (short)((extra[7] << 24) | (extra[6] << 16) | (extra[5] << 8) | extra[4]);

                            Index[idx].Lookup += 8;
                            // changed from int b = extra1 << 16 | extra2;
                            // int cast removes compiler warning
                            Index[idx].Extra = extra1 << 16 | (int)extra2;

                            br.BaseStream.Seek(curPos, SeekOrigin.Begin);
                        }
                    }
                    while (br.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
                }
            }
            else if ((idxPath != null) && (_mulPath != null))
            {
                using (var index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var count = (int)(index.Length / 12);
                    IdxLength = index.Length;
                    GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                    var buffer = new byte[index.Length];
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

                Index[patch.Index].Lookup = patch.Lookup;
                Index[patch.Index].Length = patch.Length | (1 << 31);
                Index[patch.Index].Extra = patch.Extra;
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
                using (var index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var count = (int)(index.Length / 12);
                    IdxLength = index.Length;
                    Index = new Entry3D[count];
                    GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                    var buffer = new byte[index.Length];
                    index.Read(buffer, 0, (int)index.Length);
                    Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)index.Length);
                    gc.Free();
                }
            }
            else
            {
                Stream = null;
                Index = new Entry3D[1];
                return;
            }

            if (file <= -1)
            {
                return;
            }

            Entry5D[] verdataPatches = Verdata.Patches;
            foreach (var patch in verdataPatches)
            {
                if (patch.File != file || patch.Index < 0 || patch.Index >= Index.Length)
                {
                    continue;
                }

                Index[patch.Index].Lookup = patch.Lookup;
                Index[patch.Index].Length = patch.Length | (1 << 31);
                Index[patch.Index].Extra = patch.Extra;
            }
        }

        public Stream Seek(int index, out int length, out int extra, out bool patched)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            Entry3D e = Index[index];

            if (e.Lookup < 0)
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

            if ((Stream?.CanRead != true) || (!Stream.CanSeek))
            {
                Stream = _mulPath == null ? null : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream == null)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            if (Stream.Length < e.Lookup)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            patched = false;

            Stream.Seek(e.Lookup, SeekOrigin.Begin);
            return Stream;
        }

        public bool Valid(int index, out int length, out int extra, out bool patched)
        {
            if (index < 0 || index >= Index.Length)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            Entry3D e = Index[index];

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

            if ((Stream?.CanRead != true) || (!Stream.CanSeek))
            {
                Stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (Stream.Length < e.Lookup)
            {
                length = extra = 0;
                patched = false;
                return false;
            }

            patched = false;

            return true;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Entry3D
    {
        public int Lookup;
        public int Length;
        public int Extra;
    }
}