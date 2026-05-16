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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Ultima.Helpers;

namespace UoFiddler.Plugin.Compare.Classes
{
    public enum SecondCompressionFlag
    {
        None = 0,
        Zlib = 1,
        Mythic = 3
    }

    public interface SecondIEntry
    {
        int Lookup { get; set; }
        int Length { get; set; }
        int Extra { get; set; }
        int DecompressedLength { get; set; }
        int Extra1 { get; set; }
        int Extra2 { get; set; }
        SecondCompressionFlag Flag { get; set; }
    }

    public interface SecondIFileAccessor
    {
        SecondIEntry GetEntry(int index);
        FileStream Stream { get; set; }
        int IndexLength { get; }
        long IdxLength { get; }
        SecondIEntry this[int index] { get; set; }
    }

    // Layout-sensitive: matches the on-disk 12-byte idx entry. Do not reorder fields.
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SecondEntry3D : SecondIEntry
    {
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
            get => (Extra >> 16) & 0xFFFF;
            set => Extra = (Extra & 0x0000FFFF) | ((value & 0xFFFF) << 16);
        }

        public int Extra2
        {
            get => Extra & 0x0000FFFF;
            set => Extra = (int)((Extra & 0xFFFF0000) | (uint)(value & 0xFFFF));
        }

        public SecondCompressionFlag Flag { get => SecondCompressionFlag.None; set { } }
    }

    public struct SecondEntry6D : SecondIEntry
    {
        public int Lookup { get; set; }
        public int Length { get; set; }

        private int _extra1Backing;
        private int _extra2Backing;

        public int Extra
        {
            get => (_extra1Backing << 16) | _extra2Backing;
            set
            {
                _extra1Backing = (value >> 16) & 0xFFFF;
                _extra2Backing = value & 0xFFFF;
            }
        }

        public int DecompressedLength { get; set; }
        public int Extra1 { get; set; }
        public int Extra2 { get; set; }
        public SecondCompressionFlag Flag { get; set; }
    }

    public class SecondMulFileAccessor : SecondIFileAccessor
    {
        public SecondEntry3D[] Index { get; }
        public FileStream Stream { get; set; }
        public long IdxLength { get; }
        public int IndexLength => Index.Length;

        public SecondIEntry this[int index]
        {
            get => Index[index];
            set => Index[index] = (SecondEntry3D)value;
        }

        public SecondMulFileAccessor(string idxPath, string mulPath, int length)
        {
            Index = new SecondEntry3D[length];

            using (var idx = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Stream = new FileStream(mulPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                int count = (int)(idx.Length / 12);
                IdxLength = idx.Length;

                GCHandle gc = GCHandle.Alloc(Index, GCHandleType.Pinned);
                byte[] buffer = new byte[idx.Length];
                idx.ReadExactly(buffer, 0, (int)idx.Length);
                Marshal.Copy(buffer, 0, gc.AddrOfPinnedObject(), (int)Math.Min(IdxLength, Index.Length * 12L));
                gc.Free();

                for (int i = count; i < Index.Length; ++i)
                {
                    Index[i].Lookup = -1;
                    Index[i].Length = -1;
                    Index[i].Extra = -1;
                }
            }
        }

        public SecondIEntry GetEntry(int index)
        {
            if (index < 0 || index >= Index.Length)
            {
                return new SecondEntry3D();
            }

            return Index[index];
        }
    }

    public class SecondUopFileAccessor : SecondIFileAccessor
    {
        public SecondEntry6D[] Index { get; }
        public FileStream Stream { get; set; }
        public long IdxLength { get; }
        public int IndexLength => Index.Length;

        public SecondIEntry this[int index]
        {
            get => Index[index];
            set => Index[index] = (SecondEntry6D)value;
        }

        public SecondUopFileAccessor(string path, string uopEntryExtension, int length, int idxLength, bool hasExtra)
        {
            Index = new SecondEntry6D[length];

            if (idxLength > 0)
            {
                IdxLength = idxLength * 12L;
            }

            Stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            var fileInfo = new FileInfo(path);
            string uopPattern = fileInfo.Name.Replace(fileInfo.Extension, "").ToLowerInvariant();

            using (var br = new BinaryReader(Stream, System.Text.Encoding.Default, leaveOpen: true))
            {
                br.BaseStream.Seek(0, SeekOrigin.Begin);

                if (br.ReadInt32() != 0x50594D)
                {
                    throw new ArgumentException("Bad UOP file.");
                }

                _ = br.ReadUInt32(); // version
                _ = br.ReadUInt32(); // signature
                long nextBlock = br.ReadInt64();
                _ = br.ReadUInt32(); // block capacity
                _ = br.ReadInt32();  // count

                var hashes = new Dictionary<ulong, int>();
                for (int i = 0; i < length; i++)
                {
                    string entryName = $"build/{uopPattern}/{i:D8}{uopEntryExtension}";
                    ulong hash = UopUtils.HashFileName(entryName);
                    hashes.TryAdd(hash, i);
                }

                br.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

                // UOP entries are sparse; pre-mark all as invalid.
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
                        _ = br.ReadUInt32(); // data hash
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
                            Index[idx].Flag = (SecondCompressionFlag)flag;
                            Index[idx].Extra = (extra1 << 16) | extra2;
                            Index[idx].Extra1 = extra1;
                            Index[idx].Extra2 = extra2;
                            br.BaseStream.Seek(curPos, SeekOrigin.Begin);
                        }
                        else
                        {
                            Index[idx].Lookup = (int)offset;
                            Index[idx].Length = compressedLength;
                            Index[idx].DecompressedLength = decompressedLength;
                            Index[idx].Flag = (SecondCompressionFlag)flag;
                            Index[idx].Extra = 0x0FFFFFFF;
                        }
                    }
                }
                while (br.BaseStream.Seek(nextBlock, SeekOrigin.Begin) != 0);
            }
        }

        public SecondIEntry GetEntry(int index)
        {
            if (index < 0 || index >= Index.Length)
            {
                return new SecondEntry6D();
            }

            return Index[index];
        }
    }
}
