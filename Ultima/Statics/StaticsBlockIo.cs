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
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultima.Statics
{
    /// <summary>
    /// Where a filter reports what it removed. Implemented by the defrag and copy result types so
    /// each feature counts into its own report.
    /// </summary>
    public interface IStaticsFilterStats
    {
        void TileRejected(int blockX, int blockY, StaticTile tile, RejectReason reason);

        void HueNormalized();

        void OutOfBlockMasked();
    }

    /// <summary>
    /// Reads a staidx/statics pair block by block, with the bounds and length checks the format
    /// needs. Shared by the defrag, the region copy and the diff apply so those checks exist once.
    /// </summary>
    /// <remarks>
    /// The index is a flat array of 12-byte {lookup, length, extra} records addressed as
    /// blockX * blockHeight + blockY, and lookup == -1 means the block has no statics.
    /// </remarks>
    public sealed class StaticsIndexReader : IDisposable
    {
        internal const int IndexRecordSize = 12;
        internal const int TileRecordSize = 7;

        private readonly Stream _statics;
        private readonly bool _leaveOpen;
        private readonly Stream _index;

        private byte[] _buffer = Array.Empty<byte>();

        public StaticsIndexReader(Stream index, Stream statics, int blockWidth, int blockHeight,
            IList<string> warnings = null, bool leaveOpen = false, string label = null)
        {
            _index = index ?? throw new ArgumentNullException(nameof(index));
            _statics = statics ?? throw new ArgumentNullException(nameof(statics));
            _leaveOpen = leaveOpen;

            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            Label = label ?? "staidx";

            int blockCount = blockWidth * blockHeight;

            EntriesInFile = (int)Math.Min(index.Length / IndexRecordSize, int.MaxValue / IndexRecordSize);
            Entries = new Entry3D[Math.Max(EntriesInFile, blockCount)];

            index.Seek(0, SeekOrigin.Begin);
            index.ReadExactly(MemoryMarshal.AsBytes(Entries.AsSpan(0, EntriesInFile)));

            // A short index is not an error - the tail is simply empty, which is how TileMatrix
            // treats it too.
            for (int i = EntriesInFile; i < Entries.Length; ++i)
            {
                Entries[i].Lookup = -1;
                Entries[i].Length = -1;
                Entries[i].Extra = -1;
            }

            if (EntriesInFile < blockCount)
            {
                warnings?.Add(string.Format(CultureInfo.InvariantCulture,
                    "{0} only covers {1:N0} of the {2:N0} blocks the configured map size needs. The remaining blocks are empty.",
                    Label, EntriesInFile, blockCount));
            }
        }

        public static StaticsIndexReader Open(string indexPath, string staticsPath, int blockWidth, int blockHeight,
            IList<string> warnings = null)
        {
            FileStream index = null;
            FileStream statics = null;

            try
            {
                index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
                statics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20);

                return new StaticsIndexReader(index, statics, blockWidth, blockHeight, warnings, false,
                    Path.GetFileName(indexPath));
            }
            catch
            {
                index?.Dispose();
                statics?.Dispose();

                throw;
            }
        }

        public int BlockWidth { get; }

        public int BlockHeight { get; }

        /// <summary>How many 12-byte records the index file actually holds.</summary>
        public int EntriesInFile { get; }

        /// <summary>One entry per block of the configured grid, padded with the empty sentinel.</summary>
        public Entry3D[] Entries { get; }

        public string Label { get; }

        public long StaticsLength => _statics.Length;

        /// <summary>Whether a block coordinate is inside the configured grid.</summary>
        public bool Contains(int blockX, int blockY)
        {
            return blockX >= 0 && blockY >= 0 && blockX < BlockWidth && blockY < BlockHeight;
        }

        public Entry3D GetEntry(int blockX, int blockY)
        {
            if (!Contains(blockX, blockY))
            {
                return new Entry3D { Lookup = -1, Length = -1, Extra = -1 };
            }

            return Entries[(blockX * BlockHeight) + blockY];
        }

        /// <summary>
        /// Appends a block's statics to <paramref name="into"/>, which is not cleared. A block that
        /// is empty, out of range or damaged adds nothing and is reported rather than thrown.
        /// </summary>
        /// <returns>How many statics were appended.</returns>
        public int ReadBlock(int blockX, int blockY, List<StaticTile> into, IList<string> warnings = null,
            StaticsBlockProblems problems = null)
        {
            if (!Contains(blockX, blockY))
            {
                warnings?.Add(string.Format(CultureInfo.InvariantCulture,
                    "Block {0},{1} is outside the {2} x {3} block grid of {4} and was read as empty.",
                    blockX, blockY, BlockWidth, BlockHeight, Label));

                problems?.CountOutOfRange();

                return 0;
            }

            Entry3D entry = Entries[(blockX * BlockHeight) + blockY];

            if (entry.Lookup < 0 || entry.Length <= 0)
            {
                return 0;
            }

            int length = entry.Length;

            if (entry.Lookup >= _statics.Length)
            {
                warnings?.Add(string.Format(CultureInfo.InvariantCulture,
                    "Block {0},{1} points at offset {2:N0}, past the end of the statics file. It was treated as empty.",
                    blockX, blockY, entry.Lookup));

                problems?.CountBadLookup();

                return 0;
            }

            long available = _statics.Length - entry.Lookup;

            if (length > available)
            {
                warnings?.Add(string.Format(CultureInfo.InvariantCulture,
                    "Block {0},{1} claims {2:N0} bytes but only {3:N0} remain in the statics file. It was clamped.",
                    blockX, blockY, length, available));

                problems?.CountBadLookup();

                length = (int)(available - (available % TileRecordSize));
            }

            if (length % TileRecordSize != 0)
            {
                warnings?.Add(string.Format(CultureInfo.InvariantCulture,
                    "Block {0},{1} has a length of {2} bytes, which is not a whole number of {3}-byte statics. The trailing {4} bytes were dropped.",
                    blockX, blockY, length, TileRecordSize, length % TileRecordSize));

                problems?.CountBadLength();

                length -= length % TileRecordSize;
            }

            if (length <= 0)
            {
                return 0;
            }

            if (_buffer.Length < length)
            {
                _buffer = new byte[Math.Max(length, 4096)];
            }

            _statics.Seek(entry.Lookup, SeekOrigin.Begin);
            _statics.ReadExactly(_buffer, 0, length);

            ReadOnlySpan<StaticTile> source = MemoryMarshal.Cast<byte, StaticTile>(_buffer.AsSpan(0, length));

            for (int i = 0; i < source.Length; ++i)
            {
                into.Add(source[i]);
            }

            return source.Length;
        }

        public void Dispose()
        {
            if (_leaveOpen)
            {
                return;
            }

            _index.Dispose();
            _statics.Dispose();
        }
    }

    /// <summary>
    /// Counts the damaged-index conditions <see cref="StaticsIndexReader"/> works around.
    /// </summary>
    public sealed class StaticsBlockProblems
    {
        public int BadLookup { get; private set; }

        public int BadLength { get; private set; }

        public int OutOfRange { get; private set; }

        public void CountBadLookup() => ++BadLookup;

        public void CountBadLength() => ++BadLength;

        public void CountOutOfRange() => ++OutOfRange;
    }

    /// <summary>
    /// Writes a compacted staidx/statics pair: blocks laid down back to back with no gaps, one
    /// 12-byte index record per block of the grid.
    /// </summary>
    public sealed class StaticsBlockWriter : IDisposable
    {
        private readonly Stream _index;
        private readonly Stream _statics;
        private readonly bool _leaveOpen;
        private readonly EmptyBlockStyle _emptyStyle;
        private readonly byte[] _indexBytes;

        private long _staticsPosition;
        private bool _completed;

        public StaticsBlockWriter(Stream index, Stream statics, int blockWidth, int blockHeight,
            EmptyBlockStyle emptyStyle = EmptyBlockStyle.NegativeOne, bool leaveOpen = false)
        {
            _index = index ?? throw new ArgumentNullException(nameof(index));
            _statics = statics ?? throw new ArgumentNullException(nameof(statics));
            _leaveOpen = leaveOpen;
            _emptyStyle = emptyStyle;

            BlockWidth = blockWidth;
            BlockHeight = blockHeight;

            long bytes = (long)blockWidth * blockHeight * StaticsIndexReader.IndexRecordSize;

            if (bytes > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(blockWidth),
                    "The configured map size needs a statics index larger than 2 GB.");
            }

            _indexBytes = new byte[bytes];

            // Blocks that are never written stay empty rather than zero.
            for (int i = 0; i < blockWidth * blockHeight; ++i)
            {
                WriteEmptyEntry(_indexBytes.AsSpan(i * StaticsIndexReader.IndexRecordSize, StaticsIndexReader.IndexRecordSize));
            }
        }

        public int BlockWidth { get; }

        public int BlockHeight { get; }

        /// <summary>Bytes written to the statics file so far.</summary>
        public long StaticsLength => _staticsPosition;

        public int BlocksWithStatics { get; private set; }

        public long TilesWritten { get; private set; }

        public void WriteBlock(int blockX, int blockY, IReadOnlyList<StaticTile> tiles, int extra)
        {
            if (_completed)
            {
                throw new InvalidOperationException("The statics pair has already been completed.");
            }

            if (blockX < 0 || blockY < 0 || blockX >= BlockWidth || blockY >= BlockHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(blockX),
                    $"Block {blockX},{blockY} is outside the {BlockWidth} x {BlockHeight} block grid.");
            }

            int blockId = (blockX * BlockHeight) + blockY;
            Span<byte> record = _indexBytes.AsSpan(blockId * StaticsIndexReader.IndexRecordSize, StaticsIndexReader.IndexRecordSize);

            if (tiles == null || tiles.Count == 0)
            {
                WriteEmptyEntry(record);

                return;
            }

            int length = tiles.Count * StaticsIndexReader.TileRecordSize;

            if (_staticsPosition + length > int.MaxValue)
            {
                throw new InvalidOperationException(
                    "The output statics file would pass 2 GB, which the 32-bit index lookup cannot address.");
            }

            if (extra == -1)
            {
                extra = 0;
            }

            BinaryPrimitives.WriteInt32LittleEndian(record, (int)_staticsPosition);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(4), length);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(8), extra);

            if (tiles is List<StaticTile> list)
            {
                _statics.Write(MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(list)));
            }
            else
            {
                var copy = new StaticTile[tiles.Count];

                for (int i = 0; i < tiles.Count; ++i)
                {
                    copy[i] = tiles[i];
                }

                _statics.Write(MemoryMarshal.AsBytes(copy.AsSpan()));
            }

            _staticsPosition += length;
            TilesWritten += tiles.Count;
            ++BlocksWithStatics;
        }

        public void Complete()
        {
            if (_completed)
            {
                return;
            }

            _index.Write(_indexBytes, 0, _indexBytes.Length);
            _index.Flush();
            _statics.Flush();

            _completed = true;
        }

        public void Dispose()
        {
            if (_leaveOpen)
            {
                return;
            }

            _index.Dispose();
            _statics.Dispose();
        }

        private void WriteEmptyEntry(Span<byte> record)
        {
            int empty = _emptyStyle == EmptyBlockStyle.Zero ? 0 : -1;

            BinaryPrimitives.WriteInt32LittleEndian(record, -1);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(4), empty);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(8), empty);
        }
    }

    /// <summary>
    /// The per-static decisions every statics rewrite has to make: is the item id one the client
    /// knows, is the offset inside its block, is the z legal, is the hue sane, and is this static a
    /// copy of one already in the block.
    /// </summary>
    public sealed class StaticsTileFilter
    {
        private readonly Dictionary<ulong, int> _duplicateIndex = new Dictionary<ulong, int>();

        /// <summary>Drops statics whose id is above <see cref="MaxItemId"/>.</summary>
        public bool DropInvalidItemIds { get; set; }

        public int MaxItemId { get; set; } = 0xFFFF;

        /// <summary>
        /// What to do with an offset outside 0..7. The client masks those with &amp; 7 when reading,
        /// so a static with one draws in the wrong cell.
        /// </summary>
        public OutOfBlockAction OutOfBlockTiles { get; set; } = OutOfBlockAction.Keep;

        /// <summary>Drops statics at z == -128.</summary>
        public bool DropInvalidZ { get; set; }

        public bool NormalizeNegativeHue { get; set; }

        /// <summary>Removes statics sharing id, x, y and z.</summary>
        public bool RemoveDuplicates { get; set; }

        /// <summary>Adds hue to the duplicate key. Two water tiles in a cell differing only in hue then both survive.</summary>
        public bool DuplicatesCompareHue { get; set; }

        /// <summary>The highest item id any static passed through this filter carried.</summary>
        public int HighestItemIdSeen { get; private set; }

        /// <summary>
        /// Decides on one static, adjusting it in place where the filter normalises rather than drops.
        /// </summary>
        public bool Accept(ref StaticTile tile, int blockX, int blockY, IStaticsFilterStats stats)
        {
            if (tile.Id > HighestItemIdSeen)
            {
                HighestItemIdSeen = tile.Id;
            }

            if (DropInvalidItemIds && tile.Id > MaxItemId)
            {
                stats?.TileRejected(blockX, blockY, tile, RejectReason.InvalidItemId);

                return false;
            }

            if (tile.X > 7 || tile.Y > 7)
            {
                switch (OutOfBlockTiles)
                {
                    case OutOfBlockAction.Drop:
                        stats?.TileRejected(blockX, blockY, tile, RejectReason.OutOfBlockOffset);

                        return false;

                    case OutOfBlockAction.Mask:
                        tile.X &= 0x7;
                        tile.Y &= 0x7;
                        stats?.OutOfBlockMasked();

                        break;
                }
            }

            if (DropInvalidZ && tile.Z == sbyte.MinValue)
            {
                stats?.TileRejected(blockX, blockY, tile, RejectReason.InvalidZ);

                return false;
            }

            if (NormalizeNegativeHue && tile.Hue < 0)
            {
                tile.Hue = 0;
                stats?.HueNormalized();
            }

            return true;
        }

        /// <summary>
        /// Runs <see cref="Accept"/> over a block's statics in place, then removes duplicates when
        /// asked. The survivor of a duplicate keeps the position of the first occurrence and the
        /// payload of the last.
        /// </summary>
        public void Apply(List<StaticTile> tiles, int blockX, int blockY, IStaticsFilterStats stats)
        {
            if (tiles == null || tiles.Count == 0)
            {
                return;
            }

            int write = 0;

            for (int i = 0; i < tiles.Count; ++i)
            {
                StaticTile tile = tiles[i];

                if (Accept(ref tile, blockX, blockY, stats))
                {
                    tiles[write++] = tile;
                }
            }

            tiles.RemoveRange(write, tiles.Count - write);

            if (!RemoveDuplicates || tiles.Count < 2)
            {
                return;
            }

            _duplicateIndex.Clear();
            write = 0;

            for (int i = 0; i < tiles.Count; ++i)
            {
                StaticTile tile = tiles[i];
                ulong key = DuplicateKey(tile);

                if (_duplicateIndex.TryGetValue(key, out int existing))
                {
                    tiles[existing] = tile;
                    stats?.TileRejected(blockX, blockY, tile, RejectReason.Duplicate);

                    continue;
                }

                _duplicateIndex[key] = write;
                tiles[write++] = tile;
            }

            tiles.RemoveRange(write, tiles.Count - write);
        }

        private ulong DuplicateKey(StaticTile tile)
        {
            ulong key = ((ulong)tile.Id << 14) |
                        ((ulong)(byte)tile.Z << 6) |
                        ((ulong)(uint)(tile.X & 0x7) << 3) |
                        (uint)(tile.Y & 0x7);

            if (DuplicatesCompareHue)
            {
                key |= (ulong)(ushort)tile.Hue << 30;
            }

            return key;
        }
    }

    /// <summary>
    /// Turns the 8x8 grid <see cref="TileMatrix"/> hands back into the flat list the writer takes.
    /// </summary>
    public static class StaticsBlockConversion
    {
        public static int FromHuedBlock(HuedTile[][][] block, List<StaticTile> into)
        {
            if (block == null)
            {
                return 0;
            }

            int added = 0;

            for (int x = 0; x < block.Length; ++x)
            {
                HuedTile[][] column = block[x];

                if (column == null)
                {
                    continue;
                }

                for (int y = 0; y < column.Length; ++y)
                {
                    HuedTile[] cell = column[y];

                    if (cell == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < cell.Length; ++i)
                    {
                        into.Add(new StaticTile
                        {
                            Id = cell[i].Id,
                            X = (byte)x,
                            Y = (byte)y,
                            Z = cell[i].Z,
                            Hue = (short)cell[i].Hue
                        });

                        ++added;
                    }
                }
            }

            return added;
        }
    }
}