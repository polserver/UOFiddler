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
using System.Globalization;
using System.IO;
using System.Threading;
using Ultima.Statics;

namespace Ultima.Maps
{
    /// <summary>
    /// What to do with a tile an adjustment would push outside the z the files can hold.
    /// </summary>
    public enum ZOverflowAction
    {
        /// <summary>Refuse the run, before anything is written.</summary>
        Refuse,

        /// <summary>Hold the tile at the limit it ran past.</summary>
        Clamp
    }

    /// <summary>
    /// A tally of the z values in a region, one bucket per value the format can hold. Small enough
    /// to keep around, which means any question about shifting the region is answered without
    /// reading the files again.
    /// </summary>
    /// <remarks>
    /// Both a land tile's z and a static's z are a signed byte, so -128 to 127 is the whole of what
    /// either file can carry.
    /// </remarks>
    public sealed class ZHistogram
    {
        /// <summary>Lowest z either file format can hold.</summary>
        public const int MinZ = sbyte.MinValue;

        /// <summary>Highest z either file format can hold.</summary>
        public const int MaxZ = sbyte.MaxValue;

        private readonly long[] _counts = new long[256];

        public long Total { get; private set; }

        public bool HasTiles => Total > 0;

        public int Min { get; private set; } = MaxZ;

        public int Max { get; private set; } = MinZ;

        public void Add(int z)
        {
            ++_counts[z - MinZ];
            ++Total;

            if (z < Min)
            {
                Min = z;
            }

            if (z > Max)
            {
                Max = z;
            }
        }

        /// <summary>How many tiles a shift of <paramref name="adjust"/> would push past a limit.</summary>
        public long OutOfRangeAfter(int adjust)
        {
            if (!HasTiles || adjust == 0)
            {
                return 0;
            }

            long count = 0;

            for (int z = Min; z <= Max; ++z)
            {
                int shifted = z + adjust;

                if (shifted < MinZ || shifted > MaxZ)
                {
                    count += _counts[z - MinZ];
                }
            }

            return count;
        }

        /// <summary>The largest shift up that keeps every tile inside the format, or 0 when none is needed.</summary>
        public int HeadroomUp => HasTiles ? MaxZ - Max : 0;

        /// <summary>The largest shift down that keeps every tile inside the format.</summary>
        public int HeadroomDown => HasTiles ? Min - MinZ : 0;

        public override string ToString()
        {
            return HasTiles
                ? string.Format(CultureInfo.InvariantCulture, "{0} to {1}", Min, Max)
                : "none";
        }

        /// <summary>The range this becomes when shifted, clamped to what the format holds.</summary>
        public string Describe(int adjust)
        {
            if (!HasTiles)
            {
                return "none";
            }

            return string.Format(CultureInfo.InvariantCulture, "{0} to {1}",
                Math.Clamp(Min + adjust, MinZ, MaxZ), Math.Clamp(Max + adjust, MinZ, MaxZ));
        }
    }

    /// <summary>
    /// The z a region carries, land and statics separately, read straight from the files.
    /// </summary>
    public sealed class MapRegionZSurvey
    {
        public ZHistogram Land { get; } = new ZHistogram();

        public ZHistogram Statics { get; } = new ZHistogram();

        public List<string> Warnings { get; } = new List<string>();

        /// <summary>True when a shift of <paramref name="adjust"/> would push something past a limit.</summary>
        public bool Overflows(int adjust) => OutOfRange(adjust) > 0;

        public long OutOfRange(int adjust) => Land.OutOfRangeAfter(adjust) + Statics.OutOfRangeAfter(adjust);

        /// <summary>
        /// Reads the z of every land tile and static in a region of a facet on disk. Proportional to
        /// the region, not the facet, so it is worth doing before a copy rather than during one.
        /// </summary>
        public static MapRegionZSurvey Survey(string directory, int fileIndex, MapSize size,
            BlockRectangle region, bool land, bool statics,
            IProgress<MapCopyProgress> progress = null, CancellationToken cancellationToken = default)
        {
            if (size.IsEmpty)
            {
                throw new ArgumentException("The map size is not known.", nameof(size));
            }

            var survey = new MapRegionZSurvey();

            int blocks = region.BlockWidth * region.BlockHeight;
            int done = 0;

            if (land)
            {
                var matrix = new TileMatrix(fileIndex, fileIndex, size.Width, size.Height, directory);

                try
                {
                    Span<byte> block = stackalloc byte[TileMatrix.MapBlockSize];

                    for (int x = region.BlockX1; x <= region.BlockX2; ++x)
                    {
                        for (int y = region.BlockY1; y <= region.BlockY2; ++y)
                        {
                            matrix.ReadLandBlockBytes(x, y, block);

                            for (int i = 0; i < 64; ++i)
                            {
                                survey.Land.Add((sbyte)block[TileMatrix.BlockHeaderSize + (i * 3) + 2]);
                            }

                            if ((++done & 255) == 0)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                Report(progress, "Reading land heights", done, blocks);
                            }
                        }
                    }
                }
                finally
                {
                    matrix.CloseStreams();
                }
            }

            if (!statics)
            {
                return survey;
            }

            string indexPath = Path.Combine(directory, $"staidx{fileIndex}.mul");
            string staticsPath = Path.Combine(directory, $"statics{fileIndex}.mul");

            if (!File.Exists(indexPath) || !File.Exists(staticsPath))
            {
                survey.Warnings.Add($"staidx{fileIndex}.mul or statics{fileIndex}.mul was not found in {directory}.");

                return survey;
            }

            var problems = new StaticsBlockProblems();
            var tiles = new List<StaticTile>(256);

            done = 0;

            using (StaticsIndexReader reader = StaticsIndexReader.Open(indexPath, staticsPath,
                       size.BlockWidth, size.BlockHeight, survey.Warnings))
            {
                for (int x = region.BlockX1; x <= region.BlockX2; ++x)
                {
                    for (int y = region.BlockY1; y <= region.BlockY2; ++y)
                    {
                        tiles.Clear();
                        reader.ReadBlock(x, y, tiles, survey.Warnings, problems);

                        foreach (StaticTile tile in tiles)
                        {
                            survey.Statics.Add(tile.Z);
                        }

                        if ((++done & 255) == 0)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            Report(progress, "Reading static heights", done, blocks);
                        }
                    }
                }
            }

            Report(progress, "Reading static heights", blocks, blocks);

            return survey;
        }

        private static void Report(IProgress<MapCopyProgress> progress, string stage, int done, int total)
        {
            progress?.Report(new MapCopyProgress { Stage = stage, BlocksDone = done, BlocksTotal = total });
        }
    }
}