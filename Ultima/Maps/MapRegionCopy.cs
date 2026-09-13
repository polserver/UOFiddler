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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Ultima.Statics;

namespace Ultima.Maps
{
    /// <summary>
    /// A rectangle of blocks, and the tile rectangle it covers.
    /// </summary>
    public readonly struct BlockRectangle
    {
        public BlockRectangle(int blockX1, int blockY1, int blockX2, int blockY2)
        {
            BlockX1 = blockX1;
            BlockY1 = blockY1;
            BlockX2 = blockX2;
            BlockY2 = blockY2;
        }

        public int BlockX1 { get; }

        public int BlockY1 { get; }

        public int BlockX2 { get; }

        public int BlockY2 { get; }

        public int BlockWidth => BlockX2 - BlockX1 + 1;

        public int BlockHeight => BlockY2 - BlockY1 + 1;

        public int TileX1 => BlockX1 << 3;

        public int TileY1 => BlockY1 << 3;

        public int TileX2 => (BlockX2 << 3) + 7;

        public int TileY2 => (BlockY2 << 3) + 7;

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0},{1} - {2},{3}  (blocks {4},{5} - {6},{7}, {8} x {9})",
                TileX1, TileY1, TileX2, TileY2, BlockX1, BlockY1, BlockX2, BlockY2, BlockWidth, BlockHeight);
        }
    }

    public sealed class MapCopyProgress
    {
        public string Stage { get; init; }

        public int BlocksDone { get; init; }

        public int BlocksTotal { get; init; }
    }

    public sealed class MapRegionCopyException : Exception
    {
        public MapRegionCopyException(string message) : base(message)
        {
        }

        public MapRegionCopyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public sealed class MapRegionCopyOptions
    {
        /// <summary>Folder holding the client files to copy from.</summary>
        public string SourceDirectory { get; set; }

        public int SourceFileIndex { get; set; }

        /// <summary>Block grid of the source facet. Detect it rather than assume it.</summary>
        public MapSize SourceSize { get; set; }

        /// <summary>The loaded map being copied into. Its files supply everything outside the region.</summary>
        public Map Destination { get; set; }

        /// <summary>Region to take, in source tile coordinates, inclusive. Reversed values are normalised.</summary>
        public int SourceX1 { get; set; }

        public int SourceY1 { get; set; }

        public int SourceX2 { get; set; }

        public int SourceY2 { get; set; }

        /// <summary>Where the region lands, in destination tile coordinates.</summary>
        public int DestinationX { get; set; }

        public int DestinationY { get; set; }

        public bool CopyLand { get; set; } = true;

        public bool CopyStatics { get; set; } = true;

        public MapOutputFormat MapFormat { get; set; } = MapOutputFormat.Mul;

        /// <summary>
        /// Applied to the statics of every block written. Leave null to copy them through untouched.
        /// </summary>
        public StaticsTileFilter StaticsFilter { get; set; }

        /// <summary>Replaces land ids of 0x4000 and up with 0. Land art only reaches 0x3FFF.</summary>
        public bool SanitizeLandIds { get; set; }

        public string OutputDirectory { get; set; }

        public IProgress<MapCopyProgress> Progress { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }

    public sealed class MapRegionCopyResult : IStaticsFilterStats
    {
        /// <summary>Folder the region came from, so a verification can read it back.</summary>
        public string SourceDirectory { get; set; }

        public int SourceFileIndex { get; set; }

        /// <summary>The facet that was copied into.</summary>
        public int DestinationFileIndex { get; set; }

        public BlockRectangle Source { get; set; }

        public BlockRectangle DestinationRegion { get; set; }

        public int RequestedX1 { get; set; }

        public int RequestedY1 { get; set; }

        public int RequestedX2 { get; set; }

        public int RequestedY2 { get; set; }

        public MapSize SourceSize { get; set; }

        public MapSize DestinationSize { get; set; }

        public string OutputMapPath { get; set; }

        public string OutputIndexPath { get; set; }

        public string OutputStaticsPath { get; set; }

        public long LandBlocksCopied { get; set; }

        public long LandBlocksCarried { get; set; }

        public long StaticBlocksCopied { get; set; }

        public long StaticBlocksCarried { get; set; }

        public long StaticsRead { get; set; }

        public long StaticsWritten { get; set; }

        public long LandIdsSanitized { get; set; }

        public long DroppedInvalidItemId { get; set; }

        public long DroppedOutOfBlock { get; set; }

        public long MaskedOutOfBlock { get; set; }

        public long DroppedInvalidZ { get; set; }

        public long DuplicatesRemoved { get; set; }

        public long HuesNormalized { get; set; }

        public int HighestItemIdSeen { get; set; }

        public TimeSpan Elapsed { get; set; }

        public List<string> Warnings { get; } = new List<string>();

        public List<RejectedStaticTile> RejectSamples { get; } = new List<RejectedStaticTile>();

        public int RejectSampleLimit { get; set; } = 200;

        public long StaticsAccountedFor => DroppedInvalidItemId + DroppedOutOfBlock + DroppedInvalidZ + DuplicatesRemoved;

        /// <summary>Whether the block snap widened what the user asked for.</summary>
        public bool RegionWasSnapped =>
            Source.TileX1 != RequestedX1 || Source.TileY1 != RequestedY1 ||
            Source.TileX2 != RequestedX2 || Source.TileY2 != RequestedY2;

        void IStaticsFilterStats.TileRejected(int blockX, int blockY, StaticTile tile, RejectReason reason)
        {
            switch (reason)
            {
                case RejectReason.InvalidItemId:
                    ++DroppedInvalidItemId;
                    break;

                case RejectReason.OutOfBlockOffset:
                    ++DroppedOutOfBlock;
                    break;

                case RejectReason.InvalidZ:
                    ++DroppedInvalidZ;
                    break;

                case RejectReason.Duplicate:
                    ++DuplicatesRemoved;
                    break;
            }

            if (RejectSamples.Count < RejectSampleLimit)
            {
                RejectSamples.Add(new RejectedStaticTile(blockX, blockY, tile, reason));
            }
        }

        void IStaticsFilterStats.HueNormalized() => ++HuesNormalized;

        void IStaticsFilterStats.OutOfBlockMasked() => ++MaskedOutOfBlock;

        public string ToReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Line("Requested region : {0},{1} - {2},{3}", RequestedX1, RequestedY1, RequestedX2, RequestedY2));
            sb.AppendLine(Line("Copied from      : {0}", Source));
            sb.AppendLine(Line("Copied to        : {0}", DestinationRegion));

            if (RegionWasSnapped)
            {
                sb.AppendLine("                   the request was widened to whole 8-tile blocks");
            }

            sb.AppendLine();
            sb.AppendLine(Line("Source map size  : {0}", SourceSize));
            sb.AppendLine(Line("Target map size  : {0}", DestinationSize));
            sb.AppendLine();

            if (OutputMapPath != null)
            {
                sb.AppendLine(Line("Map written      : {0}", OutputMapPath));
                sb.AppendLine(Line("  blocks         : {0:N0} copied, {1:N0} carried over", LandBlocksCopied, LandBlocksCarried));

                if (LandIdsSanitized > 0)
                {
                    sb.AppendLine(Line("  land ids reset : {0:N0} were 0x4000 or above", LandIdsSanitized));
                }
            }

            if (OutputIndexPath != null)
            {
                sb.AppendLine(Line("Statics written  : {0}", OutputStaticsPath));
                sb.AppendLine(Line("  blocks         : {0:N0} copied, {1:N0} carried over", StaticBlocksCopied, StaticBlocksCarried));
                sb.AppendLine(Line("  statics        : {0:N0} read, {1:N0} written", StaticsRead, StaticsWritten));
                sb.AppendLine(Line("  highest id     : 0x{0:X4}", HighestItemIdSeen));

                if (StaticsAccountedFor > 0 || HuesNormalized > 0 || MaskedOutOfBlock > 0)
                {
                    sb.AppendLine(Line("  removed        : {0:N0} invalid id, {1:N0} out-of-block ({2:N0} masked), {3:N0} bad z, {4:N0} duplicates",
                        DroppedInvalidItemId, DroppedOutOfBlock, MaskedOutOfBlock, DroppedInvalidZ, DuplicatesRemoved));
                    sb.AppendLine(Line("  hues reset     : {0:N0}", HuesNormalized));
                }
            }

            sb.AppendLine();
            sb.AppendLine(Line("Elapsed          : {0:hh\\:mm\\:ss\\.fff}", Elapsed));

            if (Warnings.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Line("Warnings ({0}):", Warnings.Count));

                foreach (string warning in Warnings)
                {
                    sb.AppendLine("  " + warning);
                }
            }

            if (RejectSamples.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Line("Removed static samples (first {0}):", RejectSamples.Count));

                foreach (RejectedStaticTile sample in RejectSamples)
                {
                    sb.AppendLine("  " + sample);
                }
            }

            return sb.ToString();
        }

        private static string Line(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }

    /// <summary>
    /// Copies a rectangle of blocks from one client's facet into another, rewriting the destination
    /// facet's map and statics files in full.
    /// </summary>
    public static class MapRegionCopier
    {
        private const int ProgressInterval = 256;

        public static MapRegionCopyResult Run(MapRegionCopyOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Destination == null)
            {
                throw new MapRegionCopyException("No destination map was given.");
            }

            var result = new MapRegionCopyResult();
            var stopwatch = Stopwatch.StartNew();

            MapSize source = options.SourceSize;
            var destination = new MapSize(options.Destination.Width, options.Destination.Height);

            result.SourceSize = source;
            result.DestinationSize = destination;
            result.SourceDirectory = options.SourceDirectory;
            result.SourceFileIndex = options.SourceFileIndex;
            result.DestinationFileIndex = options.Destination.FileIndex;

            if (source.IsEmpty)
            {
                throw new MapRegionCopyException("The source map size is not known.");
            }

            Normalise(options, result, source, destination);

            WarnAboutOverrides(options.SourceDirectory, options.SourceFileIndex, "source", result);
            WarnAboutOverrides(ClientDirectory(options.Destination.FileIndex), options.Destination.FileIndex, "destination", result);

            if (options.CopyLand)
            {
                CopyLand(options, result, source, destination);
            }

            if (options.CopyStatics)
            {
                CopyStatics(options, result, source, destination);
            }

            result.Elapsed = stopwatch.Elapsed;

            return result;
        }

        // ---- geometry --------------------------------------------------------------------------

        private static void Normalise(MapRegionCopyOptions options, MapRegionCopyResult result,
            MapSize source, MapSize destination)
        {
            int x1 = options.SourceX1;
            int y1 = options.SourceY1;
            int x2 = options.SourceX2;
            int y2 = options.SourceY2;

            // A reversed rectangle is a slip, not an error worth refusing over.
            if (x1 > x2)
            {
                (x1, x2) = (x2, x1);
            }

            if (y1 > y2)
            {
                (y1, y2) = (y2, y1);
            }

            result.RequestedX1 = x1;
            result.RequestedY1 = y1;
            result.RequestedX2 = x2;
            result.RequestedY2 = y2;

            Require(x1 >= 0 && x1 < source.Width, $"Source X1 {x1} is outside the source map, which is {source.Width} tiles wide.");
            Require(x2 >= 0 && x2 < source.Width, $"Source X2 {x2} is outside the source map, which is {source.Width} tiles wide.");
            Require(y1 >= 0 && y1 < source.Height, $"Source Y1 {y1} is outside the source map, which is {source.Height} tiles tall.");
            Require(y2 >= 0 && y2 < source.Height, $"Source Y2 {y2} is outside the source map, which is {source.Height} tiles tall.");

            // Whole blocks only - the files have no finer granularity.
            var sourceBlocks = new BlockRectangle(x1 >> 3, y1 >> 3, x2 >> 3, y2 >> 3);

            int destinationBlockX = options.DestinationX >> 3;
            int destinationBlockY = options.DestinationY >> 3;

            var destinationBlocks = new BlockRectangle(
                destinationBlockX,
                destinationBlockY,
                destinationBlockX + sourceBlocks.BlockWidth - 1,
                destinationBlockY + sourceBlocks.BlockHeight - 1);

            result.Source = sourceBlocks;
            result.DestinationRegion = destinationBlocks;

            Require(sourceBlocks.BlockX2 < source.BlockWidth,
                $"The region reaches source block column {sourceBlocks.BlockX2}, but the source map has {source.BlockWidth}.");
            Require(sourceBlocks.BlockY2 < source.BlockHeight,
                $"The region reaches source block row {sourceBlocks.BlockY2}, but the source map has {source.BlockHeight}.");

            Require(destinationBlocks.BlockX1 >= 0 && destinationBlocks.BlockY1 >= 0,
                "The destination position is negative.");
            Require(destinationBlocks.BlockX2 < destination.BlockWidth,
                $"The region would reach destination block column {destinationBlocks.BlockX2}, but the destination map has {destination.BlockWidth}.");
            Require(destinationBlocks.BlockY2 < destination.BlockHeight,
                $"The region would reach destination block row {destinationBlocks.BlockY2}, but the destination map has {destination.BlockHeight}.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new MapRegionCopyException(message);
            }
        }

        // ---- land ------------------------------------------------------------------------------

        private static void CopyLand(MapRegionCopyOptions options, MapRegionCopyResult result,
            MapSize source, MapSize destination)
        {
            var sourceMatrix = new TileMatrix(options.SourceFileIndex, options.SourceFileIndex,
                source.Width, source.Height, options.SourceDirectory);

            TileMatrix destinationMatrix = options.Destination.Tiles;

            try
            {
                long blockCount = destination.BlockCount;
                int done = 0;

                using (IMapBlockSink sink = MapBlockSink.Create(options.OutputDirectory,
                           options.Destination.FileIndex, options.MapFormat, blockCount))
                {
                    Span<byte> block = stackalloc byte[TileMatrix.MapBlockSize];

                    for (int x = 0; x < destination.BlockWidth; ++x)
                    {
                        for (int y = 0; y < destination.BlockHeight; ++y)
                        {
                            bool inRegion = InRegion(result.DestinationRegion, x, y);

                            if (inRegion)
                            {
                                sourceMatrix.ReadLandBlockBytes(
                                    x - result.DestinationRegion.BlockX1 + result.Source.BlockX1,
                                    y - result.DestinationRegion.BlockY1 + result.Source.BlockY1,
                                    block);

                                ++result.LandBlocksCopied;
                            }
                            else
                            {
                                destinationMatrix.ReadLandBlockBytes(x, y, block);
                                ++result.LandBlocksCarried;
                            }

                            if (options.SanitizeLandIds)
                            {
                                result.LandIdsSanitized += SanitizeLandIds(block);
                            }

                            sink.WriteBlock(block);

                            if ((++done & (ProgressInterval - 1)) == 0)
                            {
                                options.CancellationToken.ThrowIfCancellationRequested();
                                Report(options, "Copying land", done, (int)blockCount);
                            }
                        }
                    }

                    sink.Complete();
                    result.OutputMapPath = sink.OutputPath;
                }

                Report(options, "Copying land", (int)blockCount, (int)blockCount);
            }
            finally
            {
                sourceMatrix.CloseStreams();
            }
        }

        /// <summary>
        /// Land art only reaches 0x3FFF; anything above that is not a land tile the client can draw.
        /// </summary>
        private static int SanitizeLandIds(Span<byte> block)
        {
            int reset = 0;

            for (int i = 0; i < 64; ++i)
            {
                int at = TileMatrix.BlockHeaderSize + (i * 3);
                ushort id = (ushort)(block[at] | (block[at + 1] << 8));

                if (id < 0x4000)
                {
                    continue;
                }

                block[at] = 0;
                block[at + 1] = 0;
                ++reset;
            }

            return reset;
        }

        // ---- statics ---------------------------------------------------------------------------

        private static void CopyStatics(MapRegionCopyOptions options, MapRegionCopyResult result,
            MapSize source, MapSize destination)
        {
            string sourceIndex = Require(Path.Combine(options.SourceDirectory, $"staidx{options.SourceFileIndex}.mul"));
            string sourceStatics = Require(Path.Combine(options.SourceDirectory, $"statics{options.SourceFileIndex}.mul"));

            int destinationIndexFile = options.Destination.FileIndex;

            string destinationIndex = ResolveLoaded($"staidx{destinationIndexFile}.mul");
            string destinationStatics = ResolveLoaded($"statics{destinationIndexFile}.mul");

            string outputIndex = Path.Combine(options.OutputDirectory, $"staidx{destinationIndexFile}.mul");
            string outputStatics = Path.Combine(options.OutputDirectory, $"statics{destinationIndexFile}.mul");

            RefuseToOverwrite(sourceIndex, outputIndex);
            RefuseToOverwrite(destinationIndex, outputIndex);
            RefuseToOverwrite(sourceStatics, outputStatics);
            RefuseToOverwrite(destinationStatics, outputStatics);

            Directory.CreateDirectory(options.OutputDirectory);

            string tempIndex = outputIndex + ".tmp-" + Guid.NewGuid().ToString("N");
            string tempStatics = outputStatics + ".tmp-" + Guid.NewGuid().ToString("N");

            var problems = new StaticsBlockProblems();
            var tiles = new List<StaticTile>(256);

            try
            {
                using (StaticsIndexReader sourceReader = StaticsIndexReader.Open(sourceIndex, sourceStatics,
                           source.BlockWidth, source.BlockHeight, result.Warnings))
                using (StaticsIndexReader destinationReader = StaticsIndexReader.Open(destinationIndex, destinationStatics,
                           destination.BlockWidth, destination.BlockHeight, result.Warnings))
                using (var outIndexStream = new FileStream(tempIndex, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 20))
                using (var outStaticsStream = new FileStream(tempStatics, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 20))
                using (var writer = new StaticsBlockWriter(outIndexStream, outStaticsStream,
                           destination.BlockWidth, destination.BlockHeight, EmptyBlockStyle.NegativeOne, true))
                {
                    int done = 0;
                    int blockCount = destination.BlockWidth * destination.BlockHeight;

                    for (int x = 0; x < destination.BlockWidth; ++x)
                    {
                        for (int y = 0; y < destination.BlockHeight; ++y)
                        {
                            tiles.Clear();

                            bool inRegion = InRegion(result.DestinationRegion, x, y);

                            int readX = inRegion ? x - result.DestinationRegion.BlockX1 + result.Source.BlockX1 : x;
                            int readY = inRegion ? y - result.DestinationRegion.BlockY1 + result.Source.BlockY1 : y;

                            StaticsIndexReader reader = inRegion ? sourceReader : destinationReader;

                            result.StaticsRead += reader.ReadBlock(readX, readY, tiles, result.Warnings, problems);

                            if (inRegion)
                            {
                                ++result.StaticBlocksCopied;
                            }
                            else
                            {
                                ++result.StaticBlocksCarried;
                            }

                            options.StaticsFilter?.Apply(tiles, x, y, result);

                            writer.WriteBlock(x, y, tiles, reader.GetEntry(readX, readY).Extra);

                            if ((++done & (ProgressInterval - 1)) == 0)
                            {
                                options.CancellationToken.ThrowIfCancellationRequested();
                                Report(options, "Copying statics", done, blockCount);
                            }
                        }
                    }

                    writer.Complete();

                    result.StaticsWritten = writer.TilesWritten;

                    Report(options, "Copying statics", blockCount, blockCount);
                }

                File.Move(tempStatics, outputStatics, true);
                tempStatics = null;

                File.Move(tempIndex, outputIndex, true);
                tempIndex = null;

                result.OutputIndexPath = Path.GetFullPath(outputIndex);
                result.OutputStaticsPath = Path.GetFullPath(outputStatics);

                if (options.StaticsFilter != null)
                {
                    result.HighestItemIdSeen = options.StaticsFilter.HighestItemIdSeen;
                }

                if (problems.BadLookup > 0 || problems.BadLength > 0 || problems.OutOfRange > 0)
                {
                    result.Warnings.Add(string.Format(CultureInfo.InvariantCulture,
                        "Damaged index records: {0:N0} bad lookup, {1:N0} bad length, {2:N0} out of range.",
                        problems.BadLookup, problems.BadLength, problems.OutOfRange));
                }
            }
            finally
            {
                MapBlockSink.TryDelete(tempIndex);
                MapBlockSink.TryDelete(tempStatics);
            }
        }

        // ---- helpers ---------------------------------------------------------------------------

        private static bool InRegion(BlockRectangle region, int x, int y)
        {
            return x >= region.BlockX1 && x <= region.BlockX2 && y >= region.BlockY1 && y <= region.BlockY2;
        }

        private static void Report(MapRegionCopyOptions options, string stage, int done, int total)
        {
            options.Progress?.Report(new MapCopyProgress { Stage = stage, BlocksDone = done, BlocksTotal = total });
        }

        private static string Require(string path)
        {
            if (!File.Exists(path))
            {
                throw new MapRegionCopyException($"{path} was not found.");
            }

            return Path.GetFullPath(path);
        }

        private static string ResolveLoaded(string fileName)
        {
            string path = Files.GetFilePath(fileName);

            if (path == null)
            {
                throw new MapRegionCopyException(
                    $"{fileName} was not found. Check the path settings for the loaded client.");
            }

            return Path.GetFullPath(path);
        }

        private static void RefuseToOverwrite(string sourcePath, string outputPath)
        {
            if (!string.Equals(sourcePath, Path.GetFullPath(outputPath), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new MapRegionCopyException(
                $"The output directory holds a file being read ({outputPath}). Choose a different output directory.");
        }

        private static string ClientDirectory(int fileIndex)
        {
            string path = Files.GetFilePath($"staidx{fileIndex}.mul") ?? Files.GetFilePath($"map{fileIndex}.mul");

            return path == null ? null : Path.GetDirectoryName(path);
        }

        private static void WarnAboutOverrides(string directory, int fileIndex, string which, MapRegionCopyResult result)
        {
            if (directory == null)
            {
                return;
            }

            if (File.Exists(Path.Combine(directory, $"staidx{fileIndex}x.mul")) ||
                File.Exists(Path.Combine(directory, $"map{fileIndex}xLegacyMUL.uop")))
            {
                result.Warnings.Add(
                    $"The {which} client ships facet {fileIndex} override files (staidx{fileIndex}x.mul or map{fileIndex}xLegacyMUL.uop). " +
                    "The client prefers those over the pair being written here, so the result may not show up in game.");
            }
        }
    }
}