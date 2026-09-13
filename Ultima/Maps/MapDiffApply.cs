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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Ultima.Statics;

namespace Ultima.Maps
{
    public sealed class MapDiffApplyOptions
    {
        /// <summary>The loaded map whose diff files are being folded in.</summary>
        public Map Map { get; set; }

        /// <summary>Region to apply, in tile coordinates, inclusive. Reversed values are normalised.</summary>
        public int X1 { get; set; }

        public int Y1 { get; set; }

        public int X2 { get; set; }

        public int Y2 { get; set; }

        public bool ApplyLand { get; set; } = true;

        public bool ApplyStatics { get; set; } = true;

        public MapOutputFormat MapFormat { get; set; } = MapOutputFormat.Mul;

        public StaticsTileFilter StaticsFilter { get; set; }

        public string OutputDirectory { get; set; }

        public IProgress<MapCopyProgress> Progress { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }

    public sealed class MapDiffApplyResult : IStaticsFilterStats
    {
        /// <summary>The facet that was patched, so a verification can read the right files back.</summary>
        public int FileIndex { get; set; }

        public BlockRectangle Region { get; set; }

        public int RequestedX1 { get; set; }

        public int RequestedY1 { get; set; }

        public int RequestedX2 { get; set; }

        public int RequestedY2 { get; set; }

        public MapSize MapSize { get; set; }

        public string OutputMapPath { get; set; }

        public string OutputIndexPath { get; set; }

        public string OutputStaticsPath { get; set; }

        /// <summary>Blocks the land diff lists, across the whole facet.</summary>
        public int LandBlocksPatched { get; set; }

        /// <summary>Blocks the statics diff lists, across the whole facet.</summary>
        public int StaticBlocksPatched { get; set; }

        public long LandBlocksApplied { get; set; }

        public long StaticBlocksApplied { get; set; }

        public long StaticsRead { get; set; }

        public long StaticsWritten { get; set; }

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

        public bool RegionWasSnapped =>
            Region.TileX1 != RequestedX1 || Region.TileY1 != RequestedY1 ||
            Region.TileX2 != RequestedX2 || Region.TileY2 != RequestedY2;

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
            sb.AppendLine(Line("Applied to       : {0}", Region));

            if (RegionWasSnapped)
            {
                sb.AppendLine("                   the request was widened to whole 8-tile blocks");
            }

            sb.AppendLine();
            sb.AppendLine(Line("Map size         : {0}", MapSize));
            sb.AppendLine();

            if (OutputMapPath != null)
            {
                sb.AppendLine(Line("Map written      : {0}", OutputMapPath));
                sb.AppendLine(Line("  land diff lists: {0:N0} blocks for this facet", LandBlocksPatched));
                sb.AppendLine(Line("  applied        : {0:N0} of them fall in the region", LandBlocksApplied));
            }

            if (OutputIndexPath != null)
            {
                sb.AppendLine(Line("Statics written  : {0}", OutputStaticsPath));
                sb.AppendLine(Line("  static diff    : {0:N0} blocks for this facet", StaticBlocksPatched));
                sb.AppendLine(Line("  applied        : {0:N0} of them fall in the region", StaticBlocksApplied));
                sb.AppendLine(Line("  statics        : {0:N0} read, {1:N0} written", StaticsRead, StaticsWritten));
                sb.AppendLine(Line("  highest id     : 0x{0:X4}", HighestItemIdSeen));

                if (DroppedInvalidItemId > 0 || DuplicatesRemoved > 0 || HuesNormalized > 0)
                {
                    sb.AppendLine(Line("  removed        : {0:N0} invalid id, {1:N0} duplicates; {2:N0} hues reset",
                        DroppedInvalidItemId, DuplicatesRemoved, HuesNormalized));
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

            return sb.ToString();
        }

        private static string Line(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }
    }

    /// <summary>
    /// Folds a facet's mapdif and stadif data into fresh map and statics files, for a chosen region.
    /// </summary>
    /// <remarks>
    /// The land side goes through <see cref="TileMatrix"/> rather than seeking into map{N}.mul, so a
    /// client whose maps are UOP-only works. The block header the mul carries is not exposed by the
    /// patch data, so a patched block is written with a zero header; the renderer ignores it.
    /// </remarks>
    public static class MapDiffApplier
    {
        private const int ProgressInterval = 256;

        public static MapDiffApplyResult Run(MapDiffApplyOptions options)
        {
            if (options?.Map == null)
            {
                throw new MapRegionCopyException("No map was given.");
            }

            var result = new MapDiffApplyResult();
            var stopwatch = Stopwatch.StartNew();

            var size = new MapSize(options.Map.Width, options.Map.Height);
            result.MapSize = size;
            result.FileIndex = options.Map.FileIndex;

            Normalise(options, result, size);

            TileMatrix tiles = options.Map.Tiles;
            TileMatrixPatch patch = tiles.Patch;

            result.LandBlocksPatched = patch.LandBlocksCount;
            result.StaticBlocksPatched = patch.StaticBlocksCount;

            if (patch.LandBlocksCount == 0 && patch.StaticBlocksCount == 0)
            {
                result.Warnings.Add(
                    $"No diff data was loaded for map {options.Map.FileIndex}. Check that mapdif{options.Map.FileIndex}.mul " +
                    $"and stadif{options.Map.FileIndex}.mul are present and that Use Map Diff is on.");
            }

            if (options.ApplyLand)
            {
                ApplyLand(options, result, size, tiles, patch);
            }

            if (options.ApplyStatics)
            {
                ApplyStatics(options, result, size, tiles, patch);
            }

            result.Elapsed = stopwatch.Elapsed;

            return result;
        }

        private static void Normalise(MapDiffApplyOptions options, MapDiffApplyResult result, MapSize size)
        {
            int x1 = options.X1;
            int y1 = options.Y1;
            int x2 = options.X2;
            int y2 = options.Y2;

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

            if (x1 < 0 || x2 >= size.Width)
            {
                throw new MapRegionCopyException($"The region's X runs {x1}..{x2}, but the map is {size.Width} tiles wide.");
            }

            if (y1 < 0 || y2 >= size.Height)
            {
                throw new MapRegionCopyException($"The region's Y runs {y1}..{y2}, but the map is {size.Height} tiles tall.");
            }

            result.Region = new BlockRectangle(x1 >> 3, y1 >> 3, x2 >> 3, y2 >> 3);
        }

        private static void ApplyLand(MapDiffApplyOptions options, MapDiffApplyResult result, MapSize size,
            TileMatrix tiles, TileMatrixPatch patch)
        {
            long blockCount = size.BlockCount;
            int done = 0;

            using (IMapBlockSink sink = MapBlockSink.Create(options.OutputDirectory,
                       options.Map.FileIndex, options.MapFormat, blockCount))
            {
                var block = new byte[TileMatrix.MapBlockSize];

                for (int x = 0; x < size.BlockWidth; ++x)
                {
                    for (int y = 0; y < size.BlockHeight; ++y)
                    {
                        bool inRegion = InRegion(result.Region, x, y);

                        if (inRegion && patch.IsLandBlockPatched(x, y))
                        {
                            Tile[] patched = patch.GetLandBlock(x, y);

                            Array.Clear(block);
                            MemoryMarshal.AsBytes(patched.AsSpan())
                                .CopyTo(block.AsSpan(TileMatrix.BlockHeaderSize));

                            ++result.LandBlocksApplied;
                        }
                        else
                        {
                            tiles.ReadLandBlockBytes(x, y, block);
                        }

                        sink.WriteBlock(block);

                        if ((++done & (ProgressInterval - 1)) == 0)
                        {
                            options.CancellationToken.ThrowIfCancellationRequested();
                            Report(options, "Inserting map", done, (int)blockCount);
                        }
                    }
                }

                sink.Complete();
                result.OutputMapPath = sink.OutputPath;
            }

            Report(options, "Inserting map", (int)blockCount, (int)blockCount);
        }

        private static void ApplyStatics(MapDiffApplyOptions options, MapDiffApplyResult result, MapSize size,
            TileMatrix tiles, TileMatrixPatch patch)
        {
            int fileIndex = options.Map.FileIndex;

            string sourceIndex = Resolve($"staidx{fileIndex}.mul");
            string sourceStatics = Resolve($"statics{fileIndex}.mul");

            string outputIndex = Path.Combine(options.OutputDirectory, $"staidx{fileIndex}.mul");
            string outputStatics = Path.Combine(options.OutputDirectory, $"statics{fileIndex}.mul");

            RefuseToOverwrite(sourceIndex, outputIndex);
            RefuseToOverwrite(sourceStatics, outputStatics);

            Directory.CreateDirectory(options.OutputDirectory);

            string tempIndex = outputIndex + ".tmp-" + Guid.NewGuid().ToString("N");
            string tempStatics = outputStatics + ".tmp-" + Guid.NewGuid().ToString("N");

            var problems = new StaticsBlockProblems();
            var staticTiles = new List<StaticTile>(256);

            try
            {
                using (StaticsIndexReader reader = StaticsIndexReader.Open(sourceIndex, sourceStatics,
                           size.BlockWidth, size.BlockHeight, result.Warnings))
                using (var outIndexStream = new FileStream(tempIndex, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 20))
                using (var outStaticsStream = new FileStream(tempStatics, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 20))
                using (var writer = new StaticsBlockWriter(outIndexStream, outStaticsStream,
                           size.BlockWidth, size.BlockHeight, EmptyBlockStyle.NegativeOne, true))
                {
                    int done = 0;
                    int blockCount = size.BlockWidth * size.BlockHeight;

                    for (int x = 0; x < size.BlockWidth; ++x)
                    {
                        for (int y = 0; y < size.BlockHeight; ++y)
                        {
                            staticTiles.Clear();

                            bool patched = InRegion(result.Region, x, y) && patch.IsStaticBlockPatched(x, y);

                            if (patched)
                            {
                                result.StaticsRead += StaticsBlockConversion.FromHuedBlock(patch.GetStaticBlock(x, y), staticTiles);
                                ++result.StaticBlocksApplied;
                            }
                            else
                            {
                                result.StaticsRead += reader.ReadBlock(x, y, staticTiles, result.Warnings, problems);
                            }

                            options.StaticsFilter?.Apply(staticTiles, x, y, result);

                            writer.WriteBlock(x, y, staticTiles, patched ? 0 : reader.GetEntry(x, y).Extra);

                            if ((++done & (ProgressInterval - 1)) == 0)
                            {
                                options.CancellationToken.ThrowIfCancellationRequested();
                                Report(options, "Inserting statics", done, blockCount);
                            }
                        }
                    }

                    writer.Complete();

                    result.StaticsWritten = writer.TilesWritten;

                    Report(options, "Inserting statics", blockCount, blockCount);
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

        private static bool InRegion(BlockRectangle region, int x, int y)
        {
            return x >= region.BlockX1 && x <= region.BlockX2 && y >= region.BlockY1 && y <= region.BlockY2;
        }

        private static void Report(MapDiffApplyOptions options, string stage, int done, int total)
        {
            options.Progress?.Report(new MapCopyProgress { Stage = stage, BlocksDone = done, BlocksTotal = total });
        }

        private static string Resolve(string fileName)
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
    }
}