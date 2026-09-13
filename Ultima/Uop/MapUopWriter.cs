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
    /// What to put in the extra block every shipped map container carries past the end of its facet.
    /// </summary>
    public enum MapTrailingBlock
    {
        /// <summary>Write only the facet's own blocks. Smaller, and the client reads it fine.</summary>
        None,

        /// <summary>Write one empty block past the end, matching the shape of the shipped files.</summary>
        Empty,

        /// <summary>Write a block supplied by the caller, for passing a source container's through.</summary>
        Supplied
    }

    /// <summary>
    /// Writes a map{N}LegacyMUL.uop one land block at a time, so a caller that is already producing
    /// blocks does not have to spill a whole map{N}.mul to disk first.
    /// </summary>
    /// <remarks>
    /// Blocks are packed 4096 to an entry, uncompressed, under names of the form
    /// build/map{N}legacymul/00000000.dat. Every shipped facet carries exactly one block more than
    /// its grid holds, which is why the trailing block is written by default. The reader never
    /// addresses it.
    /// </remarks>
    public sealed class MapUopWriter : IDisposable
    {
        public const int MapBlockSize = 196;

        /// <summary>Land blocks per container entry.</summary>
        public const int ChunkBlocks = 4096;

        /// <summary>Bytes per container entry, 0xC4000.</summary>
        public const int ChunkBytes = ChunkBlocks * MapBlockSize;

        private readonly UopContainerWriter _container;
        private readonly Stream _output;
        private readonly bool _leaveOpen;
        private readonly string _pattern;
        private readonly long _totalBlocks;
        private readonly byte[] _chunk = new byte[ChunkBytes];
        private readonly byte[] _trailing = new byte[MapBlockSize];

        private readonly MapTrailingBlock _trailingMode;

        private int _chunkLength;
        private int _chunkIndex;
        private long _blocksWritten;
        private bool _completed;

        public MapUopWriter(Stream output, int mapId, long blockCount,
            MapTrailingBlock trailing = MapTrailingBlock.Empty, bool leaveOpen = false)
        {
            if (blockCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockCount));
            }

            _output = output ?? throw new ArgumentNullException(nameof(output));
            _leaveOpen = leaveOpen;
            _trailingMode = trailing;
            _pattern = $"map{mapId}legacymul";

            BlockCount = blockCount;
            _totalBlocks = blockCount + (trailing == MapTrailingBlock.None ? 0 : 1);

            long bytes = _totalBlocks * MapBlockSize;
            int entryCount = (int)((bytes + ChunkBytes - 1) / ChunkBytes);

            _container = new UopContainerWriter(output, UopLayout.Version5, entryCount, true);
        }

        /// <summary>Blocks the facet itself holds, not counting the trailing one.</summary>
        public long BlockCount { get; }

        /// <summary>
        /// Supplies the block written past the end of the facet. Only used with
        /// <see cref="MapTrailingBlock.Supplied"/>.
        /// </summary>
        public void SetTrailingBlock(ReadOnlySpan<byte> block)
        {
            if (block.Length != MapBlockSize)
            {
                throw new ArgumentException($"A land block is {MapBlockSize} bytes.", nameof(block));
            }

            block.CopyTo(_trailing);
        }

        /// <summary>
        /// Appends one 196-byte land block: a 4-byte header followed by 64 three-byte tiles.
        /// Blocks must arrive in index order, blockX * blockHeight + blockY.
        /// </summary>
        public void WriteBlock(ReadOnlySpan<byte> block)
        {
            if (block.Length != MapBlockSize)
            {
                throw new ArgumentException($"A land block is {MapBlockSize} bytes.", nameof(block));
            }

            if (_completed)
            {
                throw new InvalidOperationException("The container has already been completed.");
            }

            if (_blocksWritten >= BlockCount)
            {
                throw new InvalidOperationException(
                    $"More blocks were written than the {BlockCount:N0} this facet holds.");
            }

            Append(block);
            ++_blocksWritten;
        }

        public void Complete()
        {
            if (_completed)
            {
                return;
            }

            if (_blocksWritten != BlockCount)
            {
                throw new InvalidOperationException(
                    $"{_blocksWritten:N0} blocks were written but the facet holds {BlockCount:N0}.");
            }

            if (_trailingMode != MapTrailingBlock.None)
            {
                Append(_trailing);
            }

            if (_chunkLength > 0)
            {
                FlushChunk();
            }

            _container.Complete();
            _output.Flush();
            _completed = true;
        }

        public void Dispose()
        {
            _container.Dispose();

            if (!_leaveOpen)
            {
                _output.Dispose();
            }
        }

        private void Append(ReadOnlySpan<byte> block)
        {
            block.CopyTo(_chunk.AsSpan(_chunkLength));
            _chunkLength += MapBlockSize;

            if (_chunkLength == ChunkBytes)
            {
                FlushChunk();
            }
        }

        private void FlushChunk()
        {
            ReadOnlySpan<byte> payload = _chunk.AsSpan(0, _chunkLength);

            _container.WriteEntry(
                UopUtils.HashFileName($"build/{_pattern}/{_chunkIndex:D8}.dat"),
                payload,
                _chunkLength,
                0);

            ++_chunkIndex;
            _chunkLength = 0;
        }
    }
}