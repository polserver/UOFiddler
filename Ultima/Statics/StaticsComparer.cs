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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima.Statics
{
    public enum StaticsCompareMode
    {
        /// <summary>
        /// Every block must hold the same statics on both sides. Use after a run with no filters:
        /// the output should differ from the source only in layout.
        /// </summary>
        Identical,

        /// <summary>
        /// The second file may only be missing statics, never hold one the first does not. Use after
        /// a filtered run, then check the missing count against the filter counters.
        /// </summary>
        Subset
    }

    public sealed class StaticsCompareResult
    {
        public StaticsCompareMode Mode { get; set; }

        public int BlockWidth { get; set; }

        public int BlockHeight { get; set; }

        public int BlocksCompared { get; set; }

        public int BlocksDiffering { get; set; }

        public long TilesLeft { get; set; }

        public long TilesRight { get; set; }

        /// <summary>Statics the first file holds that the second does not.</summary>
        public long TilesMissing { get; set; }

        /// <summary>Statics the second file holds that the first does not. Always a defect.</summary>
        public long TilesAdded { get; set; }

        public List<string> StructuralProblems { get; } = new List<string>();

        public List<string> Differences { get; } = new List<string>();

        public bool Passed => StructuralProblems.Count == 0 && TilesAdded == 0 &&
                              (Mode == StaticsCompareMode.Subset || TilesMissing == 0);

        public string ToReport()
        {
            var sb = new StringBuilder();

            sb.AppendLine(Passed ? "PASSED" : "FAILED");
            sb.AppendLine();
            sb.AppendLine(Line("Mode             : {0}", Mode));
            sb.AppendLine(Line("Block grid       : {0} x {1}", BlockWidth, BlockHeight));
            sb.AppendLine(Line("Blocks compared  : {0:N0} ({1:N0} differ)", BlocksCompared, BlocksDiffering));
            sb.AppendLine(Line("Statics          : {0:N0} -> {1:N0}", TilesLeft, TilesRight));
            sb.AppendLine(Line("  missing        : {0:N0}", TilesMissing));
            sb.AppendLine(Line("  added          : {0:N0}", TilesAdded));

            if (StructuralProblems.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Line("Structural problems ({0}):", StructuralProblems.Count));

                foreach (string problem in StructuralProblems)
                {
                    sb.AppendLine("  " + problem);
                }
            }

            if (Differences.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(Line("Differing blocks (first {0}):", Differences.Count));

                foreach (string difference in Differences)
                {
                    sb.AppendLine("  " + difference);
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
    /// Compares two staidx/statics pairs block by block, as multisets of statics, so a defrag can be
    /// checked against the file it was produced from. Layout differences are ignored on purpose -
    /// compacting is the whole point of a defrag; what matters is that no static was lost or invented.
    /// </summary>
    public static class StaticsComparer
    {
        private const int IndexRecordSize = 12;
        private const int TileRecordSize = 7;
        private const int MaxReportedDifferences = 50;

        public static StaticsCompareResult Compare(string leftIndexPath, string leftStaticsPath,
            string rightIndexPath, string rightStaticsPath, int blockWidth, int blockHeight,
            StaticsCompareMode mode)
        {
            using (var leftIndex = File.OpenRead(leftIndexPath))
            using (var leftStatics = File.OpenRead(leftStaticsPath))
            using (var rightIndex = File.OpenRead(rightIndexPath))
            using (var rightStatics = File.OpenRead(rightStaticsPath))
            {
                return Compare(leftIndex, leftStatics, rightIndex, rightStatics, blockWidth, blockHeight, mode);
            }
        }

        public static StaticsCompareResult Compare(Stream leftIndex, Stream leftStatics,
            Stream rightIndex, Stream rightStatics, int blockWidth, int blockHeight,
            StaticsCompareMode mode)
        {
            var result = new StaticsCompareResult
            {
                Mode = mode,
                BlockWidth = blockWidth,
                BlockHeight = blockHeight
            };

            int blockCount = blockWidth * blockHeight;

            Entry3D[] left = ReadIndex(leftIndex, blockCount);
            Entry3D[] right = ReadIndex(rightIndex, blockCount);

            CheckStructure(right, rightStatics.Length, result);

            var leftTiles = new List<StaticTile>(128);
            var rightTiles = new List<StaticTile>(128);
            var counts = new Dictionary<ulong, int>();

            byte[] leftBuffer = Array.Empty<byte>();
            byte[] rightBuffer = Array.Empty<byte>();

            for (int blockId = 0; blockId < blockCount; ++blockId)
            {
                ReadBlock(leftStatics, left[blockId], ref leftBuffer, leftTiles);
                ReadBlock(rightStatics, right[blockId], ref rightBuffer, rightTiles);

                result.BlocksCompared++;
                result.TilesLeft += leftTiles.Count;
                result.TilesRight += rightTiles.Count;

                if (leftTiles.Count == 0 && rightTiles.Count == 0)
                {
                    continue;
                }

                counts.Clear();

                foreach (StaticTile tile in leftTiles)
                {
                    ulong key = TileKey(tile);
                    counts[key] = counts.TryGetValue(key, out int count) ? count + 1 : 1;
                }

                int added = 0;

                foreach (StaticTile tile in rightTiles)
                {
                    ulong key = TileKey(tile);

                    if (counts.TryGetValue(key, out int count) && count > 0)
                    {
                        counts[key] = count - 1;
                    }
                    else
                    {
                        ++added;
                    }
                }

                int missing = 0;

                foreach (int remaining in counts.Values)
                {
                    missing += remaining;
                }

                if (missing == 0 && added == 0)
                {
                    continue;
                }

                result.TilesMissing += missing;
                result.TilesAdded += added;
                result.BlocksDiffering++;

                if (result.Differences.Count < MaxReportedDifferences)
                {
                    int blockX = blockId / blockHeight;
                    int blockY = blockId % blockHeight;

                    result.Differences.Add(string.Format(CultureInfo.InvariantCulture,
                        "block {0},{1} (world {2},{3}): {4} statics -> {5}, {6} missing, {7} added",
                        blockX, blockY, blockX << 3, blockY << 3, leftTiles.Count, rightTiles.Count, missing, added));
                }
            }

            return result;
        }

        /// <summary>
        /// Invariants the output of a defrag has to satisfy on its own, regardless of the source:
        /// one index record per block, lookups walking forward with no gaps, and every block length a
        /// whole number of statics.
        /// </summary>
        private static void CheckStructure(Entry3D[] entries, long staticsLength, StaticsCompareResult result)
        {
            long expected = 0;

            for (int i = 0; i < entries.Length; ++i)
            {
                Entry3D entry = entries[i];

                if (entry.Lookup < 0 || entry.Length <= 0)
                {
                    continue;
                }

                if (entry.Length % TileRecordSize != 0)
                {
                    Add(result, string.Format(CultureInfo.InvariantCulture,
                        "block {0} has a length of {1}, which is not a multiple of {2}.", i, entry.Length, TileRecordSize));
                }

                if (entry.Lookup != expected)
                {
                    Add(result, string.Format(CultureInfo.InvariantCulture,
                        "block {0} starts at {1:N0}, expected {2:N0} - the file is not compact.", i, entry.Lookup, expected));
                }

                if (entry.Lookup + (long)entry.Length > staticsLength)
                {
                    Add(result, string.Format(CultureInfo.InvariantCulture,
                        "block {0} reaches past the end of the statics file.", i));
                }

                expected = entry.Lookup + (long)entry.Length;
            }

            if (expected != staticsLength)
            {
                Add(result, string.Format(CultureInfo.InvariantCulture,
                    "the statics file is {0:N0} bytes but the index accounts for {1:N0}.", staticsLength, expected));
            }
        }

        private static void Add(StaticsCompareResult result, string problem)
        {
            if (result.StructuralProblems.Count < MaxReportedDifferences)
            {
                result.StructuralProblems.Add(problem);
            }
        }

        private static Entry3D[] ReadIndex(Stream index, int blockCount)
        {
            var entries = new Entry3D[blockCount];
            int entriesInFile = (int)Math.Min(index.Length / IndexRecordSize, blockCount);

            index.Seek(0, SeekOrigin.Begin);
            index.ReadExactly(MemoryMarshal.AsBytes(entries.AsSpan(0, entriesInFile)));

            for (int i = entriesInFile; i < blockCount; ++i)
            {
                entries[i].Lookup = -1;
                entries[i].Length = -1;
                entries[i].Extra = -1;
            }

            return entries;
        }

        private static void ReadBlock(Stream statics, Entry3D entry, ref byte[] buffer, List<StaticTile> tiles)
        {
            tiles.Clear();

            if (entry.Lookup < 0 || entry.Length <= 0 || entry.Lookup >= statics.Length)
            {
                return;
            }

            long available = statics.Length - entry.Lookup;
            int length = (int)Math.Min(entry.Length, available);

            length -= length % TileRecordSize;

            if (length <= 0)
            {
                return;
            }

            if (buffer.Length < length)
            {
                buffer = new byte[Math.Max(length, 4096)];
            }

            statics.Seek(entry.Lookup, SeekOrigin.Begin);
            statics.ReadExactly(buffer, 0, length);

            ReadOnlySpan<StaticTile> source = MemoryMarshal.Cast<byte, StaticTile>(buffer.AsSpan(0, length));

            for (int i = 0; i < source.Length; ++i)
            {
                tiles.Add(source[i]);
            }
        }

        private static ulong TileKey(StaticTile tile)
        {
            // Raw x and y, not masked: two statics that differ only in an out-of-block offset are
            // different statics, and a comparison has to be able to see that.
            return ((ulong)(ushort)tile.Hue << 40) |
                   ((ulong)tile.Id << 24) |
                   ((ulong)(byte)tile.Z << 16) |
                   ((ulong)tile.X << 8) |
                   tile.Y;
        }
    }
}