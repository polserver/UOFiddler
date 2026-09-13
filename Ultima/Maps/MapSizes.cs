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
using Ultima.Uop;

namespace Ultima.Maps
{
    public readonly struct MapSize : IEquatable<MapSize>
    {
        public MapSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; }

        public int Height { get; }

        public int BlockWidth => Width >> 3;

        public int BlockHeight => Height >> 3;

        public long BlockCount => (long)BlockWidth * BlockHeight;

        public bool IsEmpty => Width <= 0 || Height <= 0;

        public bool Equals(MapSize other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override bool Equals(object obj)
        {
            return obj is MapSize other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Width * 397) ^ Height;
        }

        public static bool operator ==(MapSize left, MapSize right) => left.Equals(right);

        public static bool operator !=(MapSize left, MapSize right) => !left.Equals(right);

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} x {1}", Width, Height);
        }
    }

    /// <summary>
    /// Works out how big a facet's files actually are, rather than trusting a hardcoded table.
    /// </summary>
    /// <remarks>
    /// A facet cannot be identified by file length alone - map3.mul and map5.mul are both
    /// 16,056,320 bytes for different block grids - so detection is always per file index.
    /// staidx{N}.mul is the preferred signal because it is a flat array of 12-byte records with no
    /// container around it and it ships even on clients whose maps are UOP-only.
    /// </remarks>
    public static class MapSizes
    {
        private const int IndexRecordSize = 12;
        private const int MapBlockSize = 196;

        private static readonly MapSize[] _facet01 = { new MapSize(6144, 4096), new MapSize(7168, 4096) };
        private static readonly MapSize[] _facet2 = { new MapSize(2304, 1600) };
        private static readonly MapSize[] _facet3 = { new MapSize(2560, 2048) };
        private static readonly MapSize[] _facet4 = { new MapSize(1448, 1448) };
        private static readonly MapSize[] _facet5 = { new MapSize(1280, 4096) };

        /// <summary>
        /// The shapes a facet is known to ship in. Facets 0 and 1 both have a pre-T2A 6144-wide
        /// form and the modern 7168-wide one, and an install can legitimately mix the two.
        /// </summary>
        public static IReadOnlyList<MapSize> Candidates(int fileIndex)
        {
            switch (fileIndex)
            {
                case 0:
                case 1: return _facet01;
                case 2: return _facet2;
                case 3: return _facet3;
                case 4: return _facet4;
                case 5: return _facet5;
                default: return Array.Empty<MapSize>();
            }
        }

        /// <summary>
        /// What to assume when nothing can be measured.
        /// </summary>
        public static MapSize Fallback(int fileIndex)
        {
            IReadOnlyList<MapSize> candidates = Candidates(fileIndex);

            // For facets 0 and 1 the modern shape is the safer guess: reading a 7168-wide grid off a
            // 6144-wide file yields empty tail blocks, while the reverse silently loses the east edge.
            return candidates.Count == 0 ? default : candidates[candidates.Count - 1];
        }

        /// <summary>
        /// Detects the shape of a facet in the given directory.
        /// </summary>
        /// <param name="evidence">Always set: what was measured, in a form fit to show a user.</param>
        /// <returns>True when the measured block count matched a known shape.</returns>
        public static bool TryDetect(string directory, int fileIndex, out MapSize size, out string evidence)
        {
            return TryDetect(name => Resolve(directory, name), fileIndex, out size, out evidence);
        }

        /// <summary>
        /// Detects the shape of a facet in the currently loaded client.
        /// </summary>
        public static bool TryDetect(int fileIndex, out MapSize size, out string evidence)
        {
            return TryDetect(Files.GetFilePath, fileIndex, out size, out evidence);
        }

        private static bool TryDetect(Func<string, string> resolve, int fileIndex, out MapSize size, out string evidence)
        {
            if (!TryMeasureBlocks(resolve, fileIndex, out long blocks, out string source))
            {
                size = Fallback(fileIndex);
                evidence = string.Format(CultureInfo.InvariantCulture,
                    "no map or statics index found for facet {0}; assuming {1}", fileIndex, size);

                return false;
            }

            foreach (MapSize candidate in Candidates(fileIndex))
            {
                if (candidate.BlockCount != blocks)
                {
                    continue;
                }

                size = candidate;
                evidence = string.Format(CultureInfo.InvariantCulture,
                    "{0} holds {1:N0} blocks, which is {2}", source, blocks, candidate);

                return true;
            }

            size = Fallback(fileIndex);
            evidence = string.Format(CultureInfo.InvariantCulture,
                "{0} holds {1:N0} blocks, which matches no known shape for facet {2}; assuming {3}",
                source, blocks, fileIndex, size);

            return false;
        }

        private static bool TryMeasureBlocks(Func<string, string> resolve, int fileIndex, out long blocks, out string source)
        {
            string indexPath = resolve($"staidx{fileIndex}.mul");

            if (indexPath != null)
            {
                blocks = new FileInfo(indexPath).Length / IndexRecordSize;
                source = $"staidx{fileIndex}.mul";

                return true;
            }

            string mapPath = resolve($"map{fileIndex}.mul");

            if (mapPath != null)
            {
                blocks = new FileInfo(mapPath).Length / MapBlockSize;
                source = $"map{fileIndex}.mul";

                return true;
            }

            string uopPath = resolve($"map{fileIndex}LegacyMUL.uop");

            if (uopPath != null)
            {
                // Every shipped container carries one block more than the facet has, so the payload
                // length overshoots by exactly one block.
                long payloadBlocks = MapUopReader.TotalPayloadLength(MapUopReader.ReadEntryTable(uopPath)) / MapBlockSize;

                blocks = payloadBlocks - 1;
                source = $"map{fileIndex}LegacyMUL.uop";

                return true;
            }

            blocks = 0;
            source = null;

            return false;
        }

        private static string Resolve(string directory, string fileName)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return null;
            }

            string path = Path.Combine(directory, fileName);

            return File.Exists(path) ? path : null;
        }
    }
}