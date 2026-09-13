// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Ultima.Statics
{
    /// <summary>
    /// Rewrites a staidx{N}.mul / statics{N}.mul pair, compacting it and optionally filtering out
    /// statics the client cannot render correctly.
    /// </summary>
    /// <remarks>
    /// The format is documented in the client decompilation notes: the index holds one 12-byte
    /// {lookup, length, extra} record per 8x8 block addressed as blockX * blockHeight + blockY, the
    /// data file holds 7-byte {id, x, y, z, hue} records, and a block with no statics is signalled by
    /// lookup == -1. The client reads x and y masked with &amp; 7 and sorts by z at draw time, so it
    /// needs no particular tile order but will happily render an out-of-block offset in the wrong cell.
    /// </remarks>
    public sealed class StaticsDefragmenter
    {
        private const int IndexRecordSize = 12;
        private const int TileRecordSize = 7;
        private const int ProgressInterval = 256;
        private const int StreamBufferSize = 1 << 20;

        private readonly StaticsDefragOptions _options;
        private readonly List<StaticTile> _tiles = new List<StaticTile>(256);
        private readonly Dictionary<ulong, int> _duplicateIndex = new Dictionary<ulong, int>();
        private readonly Dictionary<uint, int> _stackIndex = new Dictionary<uint, int>();

        private byte[] _buffer = Array.Empty<byte>();
        private int _maxItemId;
        private int _itemTableLength;

        public StaticsDefragmenter(StaticsDefragOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public static StaticsDefragResult Defrag(StaticsDefragOptions options)
        {
            return new StaticsDefragmenter(options).Run();
        }

        /// <summary>
        /// Resolves the source files, validates the geometry, writes to temporary files and moves
        /// them into place only once the whole run has succeeded.
        /// </summary>
        public StaticsDefragResult Run()
        {
            var result = new StaticsDefragResult
            {
                FileIndex = _options.FileIndex,
                DryRun = _options.DryRun
            };

            string indexPath = ResolveSource(_options.SourceIndexPath, $"staidx{_options.FileIndex}.mul");
            string staticsPath = ResolveSource(_options.SourceStaticsPath, $"statics{_options.FileIndex}.mul");

            result.SourceIndexPath = indexPath;
            result.SourceStaticsPath = staticsPath;
            result.SourceStaticsBytes = new FileInfo(staticsPath).Length;

            WarnAboutOverrideFiles(indexPath, result);

            string outputIndexPath = null;
            string outputStaticsPath = null;

            if (!_options.DryRun)
            {
                string outputDirectory = _options.OutputDirectory;
                if (string.IsNullOrWhiteSpace(outputDirectory))
                {
                    throw new StaticsDefragException("No output directory was given.");
                }

                Directory.CreateDirectory(outputDirectory);

                outputIndexPath = Path.GetFullPath(Path.Combine(outputDirectory, $"staidx{_options.FileIndex}.mul"));
                outputStaticsPath = Path.GetFullPath(Path.Combine(outputDirectory, $"statics{_options.FileIndex}.mul"));

                RefuseToOverwriteSource(indexPath, outputIndexPath);
                RefuseToOverwriteSource(staticsPath, outputStaticsPath);

                result.OutputIndexPath = outputIndexPath;
                result.OutputStaticsPath = outputStaticsPath;
            }

            string tempIndexPath = null;
            string tempStaticsPath = null;

            try
            {
                using (var index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read, StreamBufferSize, FileOptions.SequentialScan))
                using (var statics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read, StreamBufferSize))
                {
                    if (_options.DryRun)
                    {
                        Run(index, statics, Stream.Null, Stream.Null, result);
                    }
                    else
                    {
                        string suffix = Guid.NewGuid().ToString("N");
                        tempIndexPath = outputIndexPath + ".tmp-" + suffix;
                        tempStaticsPath = outputStaticsPath + ".tmp-" + suffix;

                        using (var outIndex = new FileStream(tempIndexPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, StreamBufferSize))
                        using (var outStatics = new FileStream(tempStaticsPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, StreamBufferSize))
                        {
                            Run(index, statics, outIndex, outStatics, result);
                        }
                    }
                }

                if (!_options.DryRun)
                {
                    MoveIntoPlace(tempIndexPath, outputIndexPath, tempStaticsPath, outputStaticsPath);
                    tempIndexPath = null;
                    tempStaticsPath = null;
                }
            }
            catch (StaticsDefragException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new StaticsDefragException($"Defrag of statics{_options.FileIndex}.mul failed: {ex.Message}", ex);
            }
            finally
            {
                TryDelete(tempIndexPath);
                TryDelete(tempStaticsPath);
            }

            return result;
        }

        /// <summary>
        /// The whole algorithm, over streams. Everything that touches the file system lives in the
        /// other overload, so this one can be driven from memory buffers.
        /// </summary>
        public StaticsDefragResult Run(Stream index, Stream statics, Stream outIndex, Stream outStatics, StaticsDefragResult result = null)
        {
            result ??= new StaticsDefragResult { FileIndex = _options.FileIndex, DryRun = _options.DryRun };

            var stopwatch = Stopwatch.StartNew();

            _itemTableLength = TileData.ItemTable?.Length ?? 0;
            _maxItemId = _options.ResolveMaxItemId();
            result.MaxItemIdUsed = _maxItemId;

            ResolveGeometry(index.Length, result);

            int blockWidth = result.BlockWidth;
            int blockHeight = result.BlockHeight;
            int blockCount = blockWidth * blockHeight;

            Entry3D[] entries = ReadIndex(index, blockCount, result);
            CheckForSurplusBlocks(entries, blockCount, statics.Length, result);

            if ((long)blockCount * IndexRecordSize > int.MaxValue)
            {
                throw new StaticsDefragException("The configured map size needs an index larger than 2 GB.");
            }

            TileMatrix tiles = _options.Map?.Tiles;
            var outIndexBytes = new byte[blockCount * IndexRecordSize];

            long staticsPosition = 0;

            for (int bx = 0; bx < blockWidth; ++bx)
            {
                for (int by = 0; by < blockHeight; ++by)
                {
                    int blockId = (bx * blockHeight) + by;

                    _tiles.Clear();

                    Entry3D entry = entries[blockId];
                    bool removed = tiles != null && tiles.IsStaticBlockRemoved(bx, by);

                    if (removed)
                    {
                        ++result.BlocksClearedByRemove;
                    }
                    else
                    {
                        if (entry.Lookup >= 0 && entry.Length > 0)
                        {
                            ++result.SourceBlocksWithStatics;
                            ReadBlock(statics, entry, bx, by, result);
                        }

                        CollectPendingStatics(tiles, bx, by, result);
                        ApplyBlockFilters(bx, by, result);
                    }

                    WriteBlock(outIndexBytes, blockId, entry, ref staticsPosition, outStatics, result);

                    ++result.BlocksProcessed;

                    if ((result.BlocksProcessed & (ProgressInterval - 1)) == 0)
                    {
                        _options.CancellationToken.ThrowIfCancellationRequested();
                        ReportProgress(result, blockCount);
                    }
                }
            }

            outIndex.Write(outIndexBytes, 0, outIndexBytes.Length);
            outIndex.Flush();
            outStatics.Flush();

            result.OutputStaticsBytes = staticsPosition;
            result.Elapsed = stopwatch.Elapsed;
            ReportProgress(result, blockCount);

            return result;
        }

        private void ReportProgress(StaticsDefragResult result, int blockCount)
        {
            _options.Progress?.Report(new StaticsDefragProgress
            {
                BlocksDone = result.BlocksProcessed,
                BlocksTotal = blockCount,
                TilesWritten = result.TilesWritten
            });
        }

        // ---- geometry -------------------------------------------------------------------------

        private void ResolveGeometry(long indexLength, StaticsDefragResult result)
        {
            int blockWidth = _options.BlockWidth;
            int blockHeight = _options.BlockHeight;

            if (blockWidth <= 0 || blockHeight <= 0)
            {
                Map map = _options.Map;
                if (map == null)
                {
                    throw new StaticsDefragException(
                        "No block dimensions were given and no map was supplied to derive them from.");
                }

                blockWidth = map.Width >> 3;
                blockHeight = map.Height >> 3;
            }

            if (blockWidth <= 0 || blockHeight <= 0)
            {
                throw new StaticsDefragException($"Invalid block grid {blockWidth} x {blockHeight}.");
            }

            result.BlockWidth = blockWidth;
            result.BlockHeight = blockHeight;
            result.SourceIndexEntries = indexLength / IndexRecordSize;

            if (indexLength % IndexRecordSize != 0)
            {
                result.Warnings.Add(string.Format(CultureInfo.InvariantCulture,
                    "staidx{0}.mul is {1:N0} bytes, which is not a whole number of {2}-byte records. The trailing {3} bytes were ignored.",
                    _options.FileIndex, indexLength, IndexRecordSize, indexLength % IndexRecordSize));
            }

            var evidence = new List<string>
            {
                string.Format(CultureInfo.InvariantCulture, "staidx holds {0:N0} blocks", result.SourceIndexEntries)
            };

            string mapPath = Files.GetFilePath($"map{_options.FileIndex}.mul");
            if (mapPath != null)
            {
                // Skipped for map{N}LegacyMUL.uop - the UOP chunking makes length / 196 meaningless.
                long mapBlocks = new FileInfo(mapPath).Length / 196;
                evidence.Add(string.Format(CultureInfo.InvariantCulture, "map{0}.mul holds {1:N0} blocks", _options.FileIndex, mapBlocks));

                if (mapBlocks != (long)blockWidth * blockHeight)
                {
                    result.Warnings.Add(string.Format(CultureInfo.InvariantCulture,
                        "map{0}.mul covers {1:N0} blocks but the configured map size covers {2:N0}. Check the map size setting.",
                        _options.FileIndex, mapBlocks, (long)blockWidth * blockHeight));
                }
            }

            evidence.Add(string.Format(CultureInfo.InvariantCulture, "configured grid {0} x {1} = {2:N0}",
                blockWidth, blockHeight, (long)blockWidth * blockHeight));

            result.GeometryEvidence = string.Join("; ", evidence);
        }

        private Entry3D[] ReadIndex(Stream index, int blockCount, StaticsDefragResult result)
        {
            int entriesInFile = (int)Math.Min(index.Length / IndexRecordSize, int.MaxValue / IndexRecordSize);
            var entries = new Entry3D[Math.Max(entriesInFile, blockCount)];

            index.Seek(0, SeekOrigin.Begin);
            index.ReadExactly(MemoryMarshal.AsBytes(entries.AsSpan(0, entriesInFile)));

            // A short index is not an error - the tail is simply empty, which is how TileMatrix
            // treats it too. The old routine reached this through an exception handler that then
            // zero-filled the rest of the map.
            for (int i = entriesInFile; i < entries.Length; ++i)
            {
                entries[i].Lookup = -1;
                entries[i].Length = -1;
                entries[i].Extra = -1;
            }

            if (entriesInFile < blockCount)
            {
                result.Warnings.Add(string.Format(CultureInfo.InvariantCulture,
                    "staidx{0}.mul only covers {1:N0} of the {2:N0} blocks the configured map size needs. The remaining blocks were written empty.",
                    _options.FileIndex, entriesInFile, blockCount));
            }

            return entries;
        }

        private void CheckForSurplusBlocks(Entry3D[] entries, int blockCount, long staticsLength, StaticsDefragResult result)
        {
            if (entries.Length <= blockCount)
            {
                return;
            }

            int surplusBlocks = 0;
            long surplusTiles = 0;

            for (int i = blockCount; i < entries.Length; ++i)
            {
                if (entries[i].Lookup < 0 || entries[i].Length <= 0 || entries[i].Lookup >= staticsLength)
                {
                    continue;
                }

                ++surplusBlocks;
                surplusTiles += entries[i].Length / TileRecordSize;
            }

            if (surplusBlocks == 0)
            {
                return;
            }

            string message = string.Format(CultureInfo.InvariantCulture,
                "staidx{0}.mul holds {1:N0} blocks but the configured map size covers only {2:N0}. " +
                "{3:N0} of the surplus blocks hold statics ({4:N0} tiles) and would be discarded.",
                _options.FileIndex, entries.Length, blockCount, surplusBlocks, surplusTiles);

            if (!_options.AllowGeometryTruncation)
            {
                throw new StaticsDefragException(message +
                    " Correct the map size setting, or allow truncation if the loss is intended.");
            }

            result.Warnings.Add(message + " Truncation was allowed, so they were discarded.");
        }

        // ---- reading --------------------------------------------------------------------------

        private void ReadBlock(Stream statics, Entry3D entry, int bx, int by, StaticsDefragResult result)
        {
            int length = entry.Length;

            if (entry.Lookup >= statics.Length)
            {
                ++result.BlocksWithBadLookup;
                result.Warnings.Add(string.Format(CultureInfo.InvariantCulture,
                    "Block {0},{1} points at offset {2:N0}, past the end of the statics file. It was treated as empty.",
                    bx, by, entry.Lookup));
                return;
            }

            long available = statics.Length - entry.Lookup;
            if (length > available)
            {
                ++result.BlocksWithBadLookup;
                result.Warnings.Add(string.Format(CultureInfo.InvariantCulture,
                    "Block {0},{1} claims {2:N0} bytes but only {3:N0} remain in the statics file. It was clamped.",
                    bx, by, length, available));
                length = (int)(available - (available % TileRecordSize));
            }

            if (length % TileRecordSize != 0)
            {
                ++result.BlocksWithBadLength;
                result.Warnings.Add(string.Format(CultureInfo.InvariantCulture,
                    "Block {0},{1} has a length of {2} bytes, which is not a whole number of {3}-byte statics. The trailing {4} bytes were dropped.",
                    bx, by, length, TileRecordSize, length % TileRecordSize));
                length -= length % TileRecordSize;
            }

            if (length <= 0)
            {
                return;
            }

            if (_buffer.Length < length)
            {
                _buffer = new byte[Math.Max(length, 4096)];
            }

            statics.Seek(entry.Lookup, SeekOrigin.Begin);
            statics.ReadExactly(_buffer, 0, length);

            ReadOnlySpan<StaticTile> source = MemoryMarshal.Cast<byte, StaticTile>(_buffer.AsSpan(0, length));
            result.TilesRead += source.Length;

            Tile[] land = GetLandBlock(bx, by);

            for (int i = 0; i < source.Length; ++i)
            {
                StaticTile tile = source[i];

                if (Accept(ref tile, bx, by, land, result))
                {
                    _tiles.Add(tile);
                }
            }
        }

        private void CollectPendingStatics(TileMatrix tiles, int bx, int by, StaticsDefragResult result)
        {
            if (tiles == null || !tiles.PendingStatic(bx, by))
            {
                return;
            }

            StaticTile[] pending = tiles.GetPendingStatics(bx, by);
            if (pending == null)
            {
                return;
            }

            Tile[] land = GetLandBlock(bx, by);

            for (int i = 0; i < pending.Length; ++i)
            {
                StaticTile tile = pending[i];

                if (!Accept(ref tile, bx, by, land, result))
                {
                    continue;
                }

                _tiles.Add(tile);
                ++result.PendingTilesAdded;
            }
        }

        private Tile[] GetLandBlock(int bx, int by)
        {
            if (!_options.DropBelowTerrain || _options.Map == null)
            {
                return null;
            }

            Tile[] land = _options.Map.Tiles.GetLandBlock(bx, by);

            return land != null && land.Length >= 64 ? land : null;
        }

        // ---- filtering ------------------------------------------------------------------------

        private bool Accept(ref StaticTile tile, int bx, int by, Tile[] land, StaticsDefragResult result)
        {
            if (tile.Id > result.HighestItemIdSeen)
            {
                result.HighestItemIdSeen = tile.Id;
            }

            if (_options.DropInvalidItemIds && tile.Id > _maxItemId)
            {
                ++result.DroppedInvalidItemId;
                AddSample(result, bx, by, tile, RejectReason.InvalidItemId);
                return false;
            }

            if (tile.X > 7 || tile.Y > 7)
            {
                switch (_options.OutOfBlockTiles)
                {
                    case OutOfBlockAction.Drop:
                        ++result.DroppedOutOfBlock;
                        AddSample(result, bx, by, tile, RejectReason.OutOfBlockOffset);
                        return false;

                    case OutOfBlockAction.Mask:
                        tile.X &= 0x7;
                        tile.Y &= 0x7;
                        ++result.MaskedOutOfBlock;
                        break;
                }
            }

            if (_options.DropInvalidZ && tile.Z == sbyte.MinValue)
            {
                ++result.DroppedInvalidZ;
                AddSample(result, bx, by, tile, RejectReason.InvalidZ);
                return false;
            }

            if (_options.NormalizeNegativeHue && tile.Hue < 0)
            {
                tile.Hue = 0;
                ++result.HuesNormalized;
            }

            if (land != null && IsBelowTerrain(tile, land))
            {
                ++result.DroppedBelowTerrain;
                AddSample(result, bx, by, tile, RejectReason.BelowTerrain);
                return false;
            }

            return true;
        }

        /// <summary>
        /// The predicate <see cref="Map.ReportInvisibleStatics"/> already uses: the static and
        /// everything stacked on top of it sit under the land tile, so the client never draws it.
        /// </summary>
        private bool IsBelowTerrain(StaticTile tile, Tile[] land)
        {
            if (tile.Id >= _itemTableLength)
            {
                return false;
            }

            Tile landTile = land[((tile.Y & 0x7) << 3) + (tile.X & 0x7)];

            return tile.Z < landTile.Z && TileData.ItemTable[tile.Id].Height + tile.Z < landTile.Z;
        }

        private void ApplyBlockFilters(int bx, int by, StaticsDefragResult result)
        {
            if (_options.RemoveDuplicates && _tiles.Count > 1)
            {
                RemoveDuplicates(bx, by, result);
            }

            if (_options.CollapseStacks && _tiles.Count > 1)
            {
                CollapseStacks(bx, by, result);
            }

            if (_options.SortTiles && _tiles.Count > 1)
            {
                _tiles.Sort(CompareTiles);
            }
        }

        /// <summary>
        /// Drops statics sharing id, x, y and z. Hue is left out of the key on purpose: the old
        /// routine included it, so two water tiles in the same cell differing only in hue both
        /// survived and fought over the same pixels.
        /// </summary>
        private void RemoveDuplicates(int bx, int by, StaticsDefragResult result)
        {
            _duplicateIndex.Clear();

            int write = 0;

            for (int i = 0; i < _tiles.Count; ++i)
            {
                StaticTile tile = _tiles[i];
                ulong key = DuplicateKey(tile);

                if (_duplicateIndex.TryGetValue(key, out int existing))
                {
                    // Position of the first occurrence, payload of the last - what StaticFix does.
                    _tiles[existing] = tile;
                    ++result.DuplicatesRemoved;
                    AddSample(result, bx, by, tile, RejectReason.Duplicate);
                    continue;
                }

                _duplicateIndex[key] = write;
                _tiles[write++] = tile;
            }

            _tiles.RemoveRange(write, _tiles.Count - write);
        }

        private ulong DuplicateKey(StaticTile tile)
        {
            ulong key = ((ulong)tile.Id << 14) |
                        ((ulong)(byte)tile.Z << 6) |
                        ((ulong)(uint)(tile.X & 0x7) << 3) |
                        (uint)(tile.Y & 0x7);

            if (_options.DuplicatesCompareHue)
            {
                key |= (ulong)(ushort)tile.Hue << 30;
            }

            return key;
        }

        /// <summary>
        /// Keeps a single eligible static per cell. Only tiles matching the flag mask or the id list
        /// take part, so a floor with a chair on it is left alone while a cell holding six stacked
        /// water tiles is reduced to one.
        /// </summary>
        private void CollapseStacks(int bx, int by, StaticsDefragResult result)
        {
            _stackIndex.Clear();

            int write = 0;

            for (int i = 0; i < _tiles.Count; ++i)
            {
                StaticTile tile = _tiles[i];

                if (!IsCollapsible(tile.Id))
                {
                    _tiles[write++] = tile;
                    continue;
                }

                uint key = CollapseKey(tile);

                if (_stackIndex.ContainsKey(key))
                {
                    ++result.StacksCollapsed;
                    AddSample(result, bx, by, tile, RejectReason.CollapsedStack);
                    continue;
                }

                _stackIndex[key] = write;
                _tiles[write++] = tile;
            }

            _tiles.RemoveRange(write, _tiles.Count - write);
        }

        private bool IsCollapsible(ushort id)
        {
            if (_options.CollapseIds.Contains(id))
            {
                return true;
            }

            if (_options.CollapseFlagMask == 0 || id >= _itemTableLength)
            {
                return false;
            }

            return (TileData.ItemTable[id].Flags & _options.CollapseFlagMask) != 0;
        }

        private uint CollapseKey(StaticTile tile)
        {
            uint key = ((uint)(tile.X & 0x7) << 3) | (uint)(tile.Y & 0x7);

            if (!_options.CollapseIgnoreZ)
            {
                key |= (uint)((byte)tile.Z) << 6;
            }

            return key;
        }

        private static int CompareTiles(StaticTile left, StaticTile right)
        {
            int result = left.Y.CompareTo(right.Y);
            if (result != 0)
            {
                return result;
            }

            result = left.X.CompareTo(right.X);
            if (result != 0)
            {
                return result;
            }

            result = left.Z.CompareTo(right.Z);
            if (result != 0)
            {
                return result;
            }

            result = left.Id.CompareTo(right.Id);

            return result != 0 ? result : left.Hue.CompareTo(right.Hue);
        }

        private void AddSample(StaticsDefragResult result, int bx, int by, StaticTile tile, RejectReason reason)
        {
            if (result.RejectSamples.Count < _options.RejectSampleLimit)
            {
                result.RejectSamples.Add(new RejectedStaticTile(bx, by, tile, reason));
            }
        }

        // ---- writing --------------------------------------------------------------------------

        private void WriteBlock(byte[] indexBytes, int blockId, Entry3D entry, ref long staticsPosition,
            Stream outStatics, StaticsDefragResult result)
        {
            Span<byte> record = indexBytes.AsSpan(blockId * IndexRecordSize, IndexRecordSize);

            if (_tiles.Count == 0)
            {
                WriteEmptyEntry(record);
                return;
            }

            int length = _tiles.Count * TileRecordSize;

            if (staticsPosition + length > int.MaxValue)
            {
                throw new StaticsDefragException(
                    "The output statics file would pass 2 GB, which the 32-bit index lookup cannot address.");
            }

            int extra = _options.PreserveExtra ? entry.Extra : 0;
            if (extra == -1)
            {
                extra = 0;
            }

            BinaryPrimitives.WriteInt32LittleEndian(record, (int)staticsPosition);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(4), length);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(8), extra);

            outStatics.Write(MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(_tiles)));

            staticsPosition += length;
            result.TilesWritten += _tiles.Count;
            ++result.OutputBlocksWithStatics;
        }

        private void WriteEmptyEntry(Span<byte> record)
        {
            int empty = _options.EmptyBlocks == EmptyBlockStyle.Zero ? 0 : -1;

            BinaryPrimitives.WriteInt32LittleEndian(record, -1);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(4), empty);
            BinaryPrimitives.WriteInt32LittleEndian(record.Slice(8), empty);
        }

        // ---- file handling --------------------------------------------------------------------

        private static string ResolveSource(string explicitPath, string fileName)
        {
            if (!string.IsNullOrEmpty(explicitPath))
            {
                if (!File.Exists(explicitPath))
                {
                    throw new StaticsDefragException($"{explicitPath} does not exist.");
                }

                return Path.GetFullPath(explicitPath);
            }

            string path = Files.GetFilePath(fileName);

            if (path == null)
            {
                throw new StaticsDefragException(
                    $"{fileName} was not found. Check the path settings for the loaded client.");
            }

            return Path.GetFullPath(path);
        }

        private static void RefuseToOverwriteSource(string sourcePath, string outputPath)
        {
            if (!string.Equals(sourcePath, outputPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new StaticsDefragException(
                $"The output directory holds the file being read ({outputPath}). Choose a different output directory.");
        }

        private void WarnAboutOverrideFiles(string indexPath, StaticsDefragResult result)
        {
            string directory = Path.GetDirectoryName(indexPath);
            if (directory == null)
            {
                return;
            }

            string overrideIndex = Path.Combine(directory, $"staidx{_options.FileIndex}x.mul");

            if (File.Exists(overrideIndex))
            {
                result.Warnings.Add(
                    $"staidx{_options.FileIndex}x.mul is present. The client prefers those override files over the pair being rewritten here, so the result may not show up in game.");
            }
        }

        private static void MoveIntoPlace(string tempIndexPath, string indexPath, string tempStaticsPath, string staticsPath)
        {
            // Both files are complete on disk by now, so the pair moves back to back. If the second
            // move still fails the caller is told which half made it, because an index and a data
            // file from different runs do not describe the same world.
            File.Move(tempStaticsPath, staticsPath, true);

            try
            {
                File.Move(tempIndexPath, indexPath, true);
            }
            catch (Exception ex)
            {
                throw new StaticsDefragException(
                    $"{staticsPath} was replaced but {indexPath} could not be: {ex.Message}. The two files no longer match.", ex);
            }
        }

        private static void TryDelete(string path)
        {
            if (path == null || !File.Exists(path))
            {
                return;
            }

            try
            {
                File.Delete(path);
            }
            catch (IOException)
            {
                // Nothing useful to do - the temporary file is named so it cannot be mistaken for output.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}