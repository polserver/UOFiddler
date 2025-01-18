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

        public long IdxLength { get; }

        private readonly string _mulPath;
        private Stream _stream;

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
                _stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                var fi = new FileInfo(_mulPath);
                string uopPattern = fi.Name.Replace(fi.Extension, "").ToLowerInvariant();

                using (var br = new BinaryReader(_stream))
                {
                    br.BaseStream.Seek(0, SeekOrigin.Begin);

                    if (br.ReadInt32() != 0x50594D)
                    {
                        throw new ArgumentException("Bad UOP file.");
                    }

                    var version = br.ReadUInt32(); // version
                    var signature = br.ReadUInt32(); // signature
                    long nextBlock = br.ReadInt64();
                    var block_size = br.ReadUInt32(); // block capacity
                    var count = br.ReadInt32(); // TODO: check if we need value from here

                    if (idxLength > 0)
                    {
                        IdxLength = idxLength * 12;
                    }

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
                            uint data_hash = br.ReadUInt32();
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
                           
                            if (hasExtra && flag != 3)
                            {
                                long curPos = br.BaseStream.Position;

                                br.BaseStream.Seek(offset, SeekOrigin.Begin);

                                var extra1 = br.ReadInt32();
                                var extra2 = br.ReadInt32();
                                Index[idx].Lookup = (int)(offset + 8);
                                Index[idx].Length = compressedLength - 8;
                                Index[idx].DecompressedLength = decompressedLength;
                                Index[idx].Flag = flag;

                                // changed from int b = extra1 << 16 | extra2;
                                // int cast removes compiler warning
                                Index[idx].Extra = extra1 << 16 | (int)extra2;
                                Index[idx].Extra1 = extra1;
                                Index[idx].Extra2 = extra2;

                                br.BaseStream.Seek(curPos, SeekOrigin.Begin);
                            }
                            else
                            {
                                Index[idx].Lookup = (int)(offset);
                                Index[idx].Length = compressedLength;
                                Index[idx].DecompressedLength = decompressedLength;
                                Index[idx].Flag = flag;
                                Index[idx].Extra = 0x0FFFFFFF; //we cant read it right now, but -1 and 0 makes this entry invalid
                            }
                        }
                    }
                    while (br.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
                }
            }
            else if ((idxPath != null) && (_mulPath != null))
            {
                using (var index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    _stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
                _stream = null;
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
                    _stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
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
                _stream = null;
                Index = new Entry3D[1];
                return;
            }

            if (file <= -1)
            {
                return;
            }

            foreach (var patch in Verdata.Patches)
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

            if ((_stream?.CanRead != true) || (!_stream.CanSeek))
            {
                _stream = _mulPath == null ? null : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (_stream == null)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            if (_stream.Length < e.Lookup)
            {
                length = extra = 0;
                patched = false;
                return null;
            }

            patched = false;

            _stream.Seek(e.Lookup, SeekOrigin.Begin);
            return _stream;
        }
        public Stream Seek(int index, out Entry3D entry)
        {
            if (index < 0 || index >= Index.Length)
            {
                entry = Entry3D.Invalid;
                return null;
            }

            Entry3D e = Index[index];

            if (e.Lookup < 0)
            {
                entry = Entry3D.Invalid;

                return null;
            }

            entry = e;


            if ((e.Length & (1 << 31)) != 0)
            {
                Verdata.Seek(e.Lookup);
                return Verdata.Stream;
            }

            if (e.Length < 0)
            {
                entry = Entry3D.Invalid;
                return null;
            }

            if ((_stream?.CanRead != true) || (!_stream.CanSeek))
            {
                _stream = _mulPath == null ? null : new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (_stream == null)
            {
                entry = Entry3D.Invalid;
                return null;
            }

            if (_stream.Length < e.Lookup)
            {
                entry = Entry3D.Invalid;
                return null;
            }


            _stream.Seek(e.Lookup, SeekOrigin.Begin);
            return _stream;
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

            if ((_stream?.CanRead != true) || (!_stream.CanSeek))
            {
                _stream = new FileStream(_mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (_stream.Length < e.Lookup)
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
        public int DecompressedLength;
        public int Extra;
        public int Extra1;
        public int Extra2;
        public int Flag;

        public static Entry3D Invalid { get =>  new Entry3D(); }
    }
}