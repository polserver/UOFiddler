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
using System.IO;
using Ultima.Helpers;

namespace Ultima.Uop
{
    /// <summary>
    /// One entry of a map UOP, resolved to where its payload actually starts in the file.
    /// </summary>
    public readonly struct MapUopEntry
    {
        public MapUopEntry(long offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        /// <summary>Byte offset of the payload, past the entry header.</summary>
        public long Offset { get; }

        /// <summary>Payload length in bytes. Map entries are stored, so this is also the decompressed length.</summary>
        public int Length { get; }

        public bool IsPresent => Length > 0;
    }

    /// <summary>
    /// Reads the entry table of a map{N}LegacyMUL.uop.
    /// </summary>
    /// <remarks>
    /// Originally written by Wyatt (c) www.ruosi.org as part of TileMatrix; lifted out so map size
    /// detection and the writer's verification can walk a container without building a TileMatrix.
    /// Entries are addressed by the hash of their name, not by their position in the table, so a
    /// container whose entries sit in a different order than their chunk index still reads correctly.
    /// </remarks>
    public static class MapUopReader
    {
        private const int Magic = 0x50594D;

        /// <summary>
        /// Derives the entry-name pattern from a container's file name, e.g. map0legacymul.
        /// </summary>
        public static string PatternFromPath(string path)
        {
            var info = new FileInfo(path);

            return info.Name.Replace(info.Extension, string.Empty).ToLowerInvariant();
        }

        public static MapUopEntry[] ReadEntryTable(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return ReadEntryTable(stream, PatternFromPath(path));
            }
        }

        /// <summary>
        /// Walks the block list and returns one entry per chunk index, in chunk order.
        /// </summary>
        public static MapUopEntry[] ReadEntryTable(Stream stream, string pattern)
        {
            var reader = new BinaryReader(stream);

            stream.Seek(0, SeekOrigin.Begin);

            if (reader.ReadInt32() != Magic)
            {
                throw new ArgumentException($"{pattern}: not a UOP file.");
            }

            reader.ReadInt64(); // version + signature
            long nextBlock = reader.ReadInt64();
            reader.ReadInt32(); // block capacity
            int count = reader.ReadInt32();

            var entries = new MapUopEntry[count];
            var hashes = new Dictionary<ulong, int>(count);

            for (int i = 0; i < count; i++)
            {
                hashes.TryAdd(UopUtils.HashFileName($"build/{pattern}/{i:D8}.dat"), i);
            }

            stream.Seek(nextBlock, SeekOrigin.Begin);

            do
            {
                int filesCount = reader.ReadInt32();
                nextBlock = reader.ReadInt64();

                for (int i = 0; i < filesCount; i++)
                {
                    long offset = reader.ReadInt64();
                    int headerLength = reader.ReadInt32();
                    int compressedLength = reader.ReadInt32();
                    reader.ReadInt32(); // decompressed length - equal to the compressed one while stored
                    ulong hash = reader.ReadUInt64();
                    reader.ReadUInt32(); // Adler32
                    short flag = reader.ReadInt16();

                    if (offset == 0)
                    {
                        continue;
                    }

                    // Map blocks are addressed by slicing straight into the file, so only stored
                    // entries can be handled. Every map*LegacyMUL.uop EA ships uses flag 0, but the
                    // UOP packer can be told to zlib them.
                    if (flag != 0)
                    {
                        throw new NotSupportedException(
                            $"{pattern}: compressed map UOP entries are not supported " +
                            $"(entry uses compression flag {flag}). Repack the map with compression set to None.");
                    }

                    if (!hashes.TryGetValue(hash, out int idx))
                    {
                        throw new ArgumentException(
                            $"File with hash 0x{hash:X8} was not found in hashes dictionary! EA Mythic changed UOP format!");
                    }

                    if (idx < 0 || idx >= entries.Length)
                    {
                        throw new IndexOutOfRangeException(
                            "hashes dictionary and files collection have different count of entries!");
                    }

                    entries[idx] = new MapUopEntry(offset + headerLength, compressedLength);
                }
            }
            while (stream.Seek(nextBlock, SeekOrigin.Begin) != 0);

            return entries;
        }

        /// <summary>
        /// Total payload bytes the container holds. Divided by 196 this is the number of map blocks
        /// it carries, which is normally one more than the facet actually has.
        /// </summary>
        public static long TotalPayloadLength(MapUopEntry[] entries)
        {
            long total = 0;

            foreach (MapUopEntry entry in entries)
            {
                total += entry.Length;
            }

            return total;
        }
    }
}