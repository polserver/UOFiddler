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
using Ultima.Helpers;

namespace Ultima.Uop
{
    /// <summary>
    /// The two container shapes the shipped client files come in.
    /// </summary>
    /// <remarks>
    /// Which shape a file type uses depends on the client build rather than on the type alone.
    /// Version 4 holds 100 entries per block with the first block right behind the 0x28 byte header
    /// and a 12 byte header in front of every entry payload - MultiCollection and tileart in every
    /// build that has them, sound from 7.0.65.4 on, gumpart from 7.0.114.4 on.
    /// Version 5 holds 1000 entries per block after a gap, and its entry headers are 135..137 bytes
    /// whose tail is high entropy and cannot be reproduced, so we write none - art and maps in every
    /// build, sound and gumpart in the older ones.
    /// </remarks>
    public enum UopLayout
    {
        Version4,
        Version5
    }

    /// <summary>
    /// Writes the framing of a UOP container: the file header, the linked list of blocks and each
    /// block's entry table. Callers supply one payload per entry and own whatever transform or
    /// compression that payload needed.
    /// </summary>
    public sealed class UopContainerWriter : IDisposable
    {
        private const int Magic = 0x50594D;
        private const uint Signature = 0xFD23EC43;
        private const int FileHeaderSize = 0x28;

        /// <summary>
        /// On disk size of one entry in a block's entry table:
        /// offset(8) headerLength(4) compressedSize(4) decompressedSize(4) identifier(8) hash(4) flag(2).
        /// </summary>
        internal const int TableEntrySize = 8 + 4 + 4 + 4 + 8 + 4 + 2;

        /// <summary>Size of a block header: usedEntryCount(4) nextBlockOffset(8).</summary>
        internal const int BlockHeaderSize = 4 + 8;

        /// <summary>Offset of the next-block pointer inside a block header.</summary>
        internal const int NextBlockOffsetField = 4;

        private static readonly byte[] _emptyTableEntry = new byte[TableEntrySize];

        private readonly BinaryWriter _writer;
        private readonly bool _leaveOpen;
        private readonly int _blockCapacity;
        private readonly int _entryCount;
        private readonly TableEntry[] _tableEntries;

        private int _entriesWritten;
        private int _indexInBlock;
        private long _blockStart = -1;
        private bool _completed;

        public UopContainerWriter(Stream output, UopLayout layout, int entryCount, bool leaveOpen = false)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (entryCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entryCount));
            }

            _writer = new BinaryWriter(output);
            _leaveOpen = leaveOpen;
            _entryCount = entryCount;
            _blockCapacity = layout == UopLayout.Version4 ? 0x64 : 0x3E8;
            _tableEntries = new TableEntry[_blockCapacity];

            long firstBlock = layout == UopLayout.Version4 ? 0x28 : 0x200;

            _writer.Write(Magic);
            _writer.Write(layout == UopLayout.Version4 ? 4 : 5);
            _writer.Write(Signature);
            _writer.Write(firstBlock);
            _writer.Write(_blockCapacity);
            _writer.Write(entryCount);
            _writer.Write(0); // modified count (wseq, version 5 only)
            _writer.Write(0); // cseq, version 5 only
            _writer.Write(0); // reserved

            for (long i = FileHeaderSize; i < firstBlock; ++i)
            {
                _writer.Write((byte)0);
            }
        }

        public int BlockCapacity => _blockCapacity;

        /// <summary>
        /// Appends one entry. The entry's hash field is the Adler32 of <paramref name="entryHeader"/>
        /// when one is given and of the payload otherwise, which is what the shipped files carry.
        /// </summary>
        /// <param name="decompressedSize">
        /// Size of the payload before compression. Equal to the payload length for a stored entry.
        /// </param>
        public void WriteEntry(ulong identifier, ReadOnlySpan<byte> payload, int decompressedSize,
            short compressionFlag, ReadOnlySpan<byte> entryHeader = default)
        {
            if (_completed)
            {
                throw new InvalidOperationException("The container has already been completed.");
            }

            if (_entriesWritten >= _entryCount)
            {
                throw new InvalidOperationException(
                    $"More entries were written than the {_entryCount} declared in the header.");
            }

            if (_blockStart < 0)
            {
                BeginBlock();
            }

            ref TableEntry entry = ref _tableEntries[_indexInBlock];

            entry.Offset = _writer.BaseStream.Position;
            entry.HeaderLength = entryHeader.Length;
            entry.Size = payload.Length;
            entry.DecompressedSize = decompressedSize;
            entry.Identifier = identifier;
            entry.CompressionFlag = compressionFlag;

            if (!entryHeader.IsEmpty)
            {
                _writer.Write(entryHeader);
            }

            _writer.Write(payload);

            entry.Hash = UopUtils.HashAdler32(entryHeader.IsEmpty ? payload : entryHeader);

            ++_entriesWritten;
            ++_indexInBlock;

            if (_indexInBlock == _blockCapacity)
            {
                EndBlock(_entriesWritten < _entryCount);
            }
        }

        /// <summary>
        /// Closes the trailing block and back-fills its entry table. Must be called before the
        /// stream is used for anything else.
        /// </summary>
        public void Complete()
        {
            if (_completed)
            {
                return;
            }

            if (_entriesWritten != _entryCount)
            {
                throw new InvalidOperationException(
                    $"{_entriesWritten} entries were written but the header declares {_entryCount}.");
            }

            if (_blockStart >= 0)
            {
                EndBlock(false);
            }

            _writer.Flush();
            _completed = true;
        }

        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _writer.Dispose();
            }
        }

        private void BeginBlock()
        {
            _blockStart = _writer.BaseStream.Position;
            _indexInBlock = 0;

            int used = Math.Min(_blockCapacity, _entryCount - _entriesWritten);

            _writer.Write(used);
            _writer.Write((long)0); // next block, back-filled once this one is full
            _writer.Seek(TableEntrySize * _blockCapacity, SeekOrigin.Current);
        }

        private void EndBlock(bool moreToCome)
        {
            long afterBlock = _writer.BaseStream.Position;

            if (moreToCome)
            {
                _writer.BaseStream.Seek(_blockStart + NextBlockOffsetField, SeekOrigin.Begin);
                _writer.Write(afterBlock);
            }
            else
            {
                _writer.BaseStream.Seek(_blockStart + BlockHeaderSize, SeekOrigin.Begin);
            }

            for (int i = 0; i < _indexInBlock; ++i)
            {
                _writer.Write(_tableEntries[i].Offset);
                _writer.Write(_tableEntries[i].HeaderLength);
                _writer.Write(_tableEntries[i].Size);
                _writer.Write(_tableEntries[i].DecompressedSize);
                _writer.Write(_tableEntries[i].Identifier);
                _writer.Write(_tableEntries[i].Hash);
                _writer.Write(_tableEntries[i].CompressionFlag);
            }

            for (int i = _indexInBlock; i < _blockCapacity; ++i)
            {
                _writer.Write(_emptyTableEntry);
            }

            _writer.BaseStream.Seek(afterBlock, SeekOrigin.Begin);

            _blockStart = -1;

            if (moreToCome)
            {
                BeginBlock();
            }
        }

        private struct TableEntry
        {
            public long Offset;
            public int HeaderLength;
            public int Size;
            public int DecompressedSize;
            public ulong Identifier;
            public uint Hash;
            public short CompressionFlag;
        }
    }
}