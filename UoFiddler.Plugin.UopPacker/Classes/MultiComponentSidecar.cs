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
using System.Text;

namespace UoFiddler.Plugin.UopPacker.Classes
{
    /// <summary>
    /// Side storage for the per tile component ids carried by MultiCollection.uop entries.
    /// </summary>
    /// <remarks>
    /// A MultiCollection tile record is [itemId:2][x:2][y:2][z:2][flag:2][componentCount:4] followed by
    /// componentCount 32 bit component ids. A multi.mul row is a fixed 16 bytes and has nowhere to put
    /// those ids, so they are written next to the mul/idx pair instead and merged back in when packing.
    ///
    /// In the shipped client file 3200 of 186695 tiles carry ids, drawn from a shared vocabulary of only
    /// 59 values (119404 - 119462) reused across 304 multis, so ids for newly authored multis can be
    /// written by hand.
    ///
    /// A component id marks a tile's interactive role within the multi, not its graphic and not a cliloc:
    /// every tile carrying 119405 is a "tiller man" in tiledata, every 119406 is a "hatch", 119404 is the
    /// hull (mast/deck), 119407/119408 are the planks, and 119453/119454 sit on doors. All 24 boat multis
    /// (6 hulls x 4 facings) share the same 119404-119408 signature, and 1121 of the 1273 item ids that
    /// carry a component always carry the same one. That is why dropping them breaks a client: a boat
    /// without its tiller man cannot be steered and a house door stops being a door.
    /// </remarks>
    public static class MultiComponentSidecar
    {
        /// <summary>
        /// Companion file for a multi.mul, e.g. "multi.mul" -&gt; "multi-components.txt".
        /// </summary>
        public static string GetDefaultPath(string mulPath)
        {
            if (string.IsNullOrWhiteSpace(mulPath))
            {
                return string.Empty;
            }

            string directory = Path.GetDirectoryName(mulPath);
            string name = Path.GetFileNameWithoutExtension(mulPath) + "-components.txt";

            return string.IsNullOrEmpty(directory) ? name : Path.Combine(directory, name);
        }

        private static readonly string[] _header =
        {
            "# MultiCollection.uop per tile component ids, written by UOFiddler.",
            "# multi.mul cannot store these, so they live here and are merged back in when packing.",
            "# Format: multiId,tileIndex,itemId,x,y,z,componentId[,componentId...]",
            "# itemId/x/y/z only identify the tile; a row whose tile no longer matches multi.mul is skipped.",
            "#",
            "# A component id marks a tile's interactive role, e.g. 119404 hull, 119405 tiller man,",
            "# 119406 hatch, 119407/119408 planks, 119453/119454 doors. Only 59 ids exist (119404-119462)",
            "# and they are shared by every multi, so new multis can reuse them."
        };

        public static Writer CreateWriter(string path) => new Writer(path);

        /// <summary>
        /// Streams component rows out while a multi.mul is being written.
        /// </summary>
        public sealed class Writer : IDisposable
        {
            private readonly StreamWriter _writer;

            public int RowCount { get; private set; }

            public int ComponentCount { get; private set; }

            internal Writer(string path)
            {
                _writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None), new UTF8Encoding(false));

                foreach (string line in _header)
                {
                    _writer.WriteLine(line);
                }
            }

            public void Write(int multiId, int tileIndex, ushort itemId, short x, short y, short z, ReadOnlySpan<uint> componentIds)
            {
                if (componentIds.Length == 0)
                {
                    return;
                }

                var sb = new StringBuilder(64);
                sb.Append(multiId.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(tileIndex.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append("0x").Append(itemId.ToString("X4", CultureInfo.InvariantCulture)).Append(',');
                sb.Append(x.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(y.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(z.ToString(CultureInfo.InvariantCulture));

                foreach (uint id in componentIds)
                {
                    sb.Append(',').Append(id.ToString(CultureInfo.InvariantCulture));
                }

                _writer.WriteLine(sb.ToString());

                ++RowCount;
                ComponentCount += componentIds.Length;
            }

            public void Dispose() => _writer.Dispose();
        }

        /// <summary>
        /// Loads a sidecar file. Returns null when <paramref name="path"/> is empty or does not exist,
        /// in which case every tile is packed with a component count of zero.
        /// </summary>
        public static Table Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            var rows = new Dictionary<(int MultiId, int TileIndex), Row>();
            var malformed = new List<string>();

            int lineNumber = 0;
            foreach (string rawLine in File.ReadLines(path))
            {
                ++lineNumber;

                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                string[] fields = line.Split(',');
                if (fields.Length < 7)
                {
                    malformed.Add($"line {lineNumber}: expected at least 7 fields, got {fields.Length}");
                    continue;
                }

                if (!TryParse(fields[0], out long multiId) ||
                    !TryParse(fields[1], out long tileIndex) ||
                    !TryParse(fields[2], out long itemId) ||
                    !TryParse(fields[3], out long x) ||
                    !TryParse(fields[4], out long y) ||
                    !TryParse(fields[5], out long z))
                {
                    malformed.Add($"line {lineNumber}: could not parse tile identity");
                    continue;
                }

                var ids = new uint[fields.Length - 6];
                bool ok = true;

                for (int i = 0; i < ids.Length; ++i)
                {
                    if (!TryParse(fields[6 + i], out long id) || id < 0 || id > uint.MaxValue)
                    {
                        malformed.Add($"line {lineNumber}: could not parse component id '{fields[6 + i].Trim()}'");
                        ok = false;
                        break;
                    }

                    ids[i] = (uint)id;
                }

                if (!ok)
                {
                    continue;
                }

                rows[((int)multiId, (int)tileIndex)] = new Row((ushort)itemId, (short)x, (short)y, (short)z, ids);
            }

            return new Table(path, rows, malformed);
        }

        internal readonly struct Row
        {
            public Row(ushort itemId, short x, short y, short z, uint[] componentIds)
            {
                ItemId = itemId;
                X = x;
                Y = y;
                Z = z;
                ComponentIds = componentIds;
            }

            public ushort ItemId { get; }
            public short X { get; }
            public short Y { get; }
            public short Z { get; }
            public uint[] ComponentIds { get; }
        }

        public sealed class Table
        {
            private static readonly uint[] _none = Array.Empty<uint>();

            private readonly Dictionary<(int MultiId, int TileIndex), Row> _rows;
            private readonly List<string> _problems;

            internal Table(string path, Dictionary<(int MultiId, int TileIndex), Row> rows, List<string> malformed)
            {
                Path = path;
                _rows = rows;
                _problems = malformed;
            }

            public string Path { get; }

            public int RowCount => _rows.Count;

            /// <summary>Malformed lines plus rows whose tile identity no longer matches multi.mul.</summary>
            public IReadOnlyList<string> Problems => _problems;

            /// <summary>
            /// Component ids for a tile, or an empty span when the sidecar has no entry for it. A row whose
            /// itemId/x/y/z disagree with the mul row is dropped and recorded in <see cref="Problems"/> -
            /// that happens when a multi's tile list was re-authored after the sidecar was written.
            /// </summary>
            public uint[] GetComponentIds(int multiId, int tileIndex, ushort itemId, short x, short y, short z)
            {
                if (!_rows.TryGetValue((multiId, tileIndex), out Row row))
                {
                    return _none;
                }

                if (row.ItemId != itemId || row.X != x || row.Y != y || row.Z != z)
                {
                    _problems.Add(
                        $"multi {multiId} tile {tileIndex}: sidecar describes 0x{row.ItemId:X4} at ({row.X},{row.Y},{row.Z}) " +
                        $"but multi.mul has 0x{itemId:X4} at ({x},{y},{z}) - component ids dropped");
                    return _none;
                }

                return row.ComponentIds;
            }
        }

        private static bool TryParse(string field, out long value)
        {
            ReadOnlySpan<char> span = field.AsSpan().Trim();

            if (span.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return long.TryParse(span[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value);
            }

            return long.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
