using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.UopPacker.Classes
{
    public class LegacyMulFileConverter
    {
        private struct IdxEntry
        {
            public int Id;
            public int Offset;
            public int Size;
            public int Extra;
        }

        private struct TableEntry
        {
            public long Offset;
            public int HeaderLength;
            public int Size;
            public int DecompressedSize;
            public ulong Identifier;
            public uint Hash;
            public short CompressionFlag;
            public bool Compressed;
        }

        //
        // IO shortcuts
        //
        private static BinaryReader OpenInput(string path)
        {
            return path == null
                       ? null
                       : new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private static BinaryWriter OpenOutput(string path)
        {
            return string.IsNullOrWhiteSpace(path)
                       ? null
                       : new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None));
        }

        /// <summary>
        /// Tiles whose multi.mul flags/extra carry bits the uop visibility word cannot represent.
        /// Thread static so parallel conversions do not mix counts.
        /// </summary>
        [ThreadStatic]
        private static int _unrepresentableMultiFlagTiles;

        /// <summary>
        /// Idx rows dropped because they carry no data.
        /// </summary>
        [ThreadStatic]
        private static int _emptyIdxEntriesSkipped;

        /// <summary>
        /// Idx rows dropped because they point outside the mul.
        /// </summary>
        [ThreadStatic]
        private static int _outOfRangeIdxEntriesSkipped;

        /// <summary>
        /// Identifier for "build/multicollection/housing.bin" inside MultiCollection.uop
        /// (0x126D1E99DDEDEE0A).
        /// </summary>
        private static readonly ulong _housingBinIdentifier = UopUtils.HashFileName(_housingBinEntryName);

        private const string _housingBinEntryName = "build/multicollection/housing.bin";

        /// <summary>
        /// Bytes of map terrain per map*LegacyMUL.uop entry: 4096 blocks of 196 bytes. Every shipped map
        /// UOP uses it, so a facet's last entry runs past the end of the mul by up to one chunk.
        /// </summary>
        private const int _mapChunkSize = 0xC4000;

        // Sentinel Id used to mark a synthetic entry that should be written from housing.bin.
        private const int _housingBinSentinelId = -1;

        /// <summary>
        /// zlib level used for MultiCollection.uop entries, chosen to land as close as possible to the
        /// size the client's own packer produced.
        /// </summary>
        /// <remarks>
        /// The shipped file's 872 entries total 522 746 compressed bytes. .NET does not use stock zlib
        /// (it ships zlib-ng), so its level mapping is not monotonic and does not reproduce stock zlib
        /// byte for byte. Re-compressing those payloads through this runtime measures:
        /// level 6 / <see cref="CompressionLevel.Optimal"/> 546 505 (+4.5%), level 9 /
        /// <see cref="CompressionLevel.SmallestSize"/> 538 500 (+3.0%), level 8 525 894 (+0.6%),
        /// and level 7 523 601 (+0.2%) - the closest available. Any level produces a valid file; this
        /// only affects size, so it is safe to revisit if a future runtime shifts the mapping.
        /// </remarks>
        private const int _multiCollectionZlibLevel = 7;

        //
        // MUL -> UOP
        //
        public static void ToUop(string inFile, string inFileIdx, string outFile, FileType type, int typeIndex, CompressionFlag compressionFlag = CompressionFlag.None, string housingBinFile = "", IProgress<int> progress = null, string componentsFile = "")
        {
            if (type == FileType.MultiCollection)
            {
                if (compressionFlag == CompressionFlag.Mythic)
                {
                    throw new ArgumentException(
                        "MultiCollection.uop does not support Mythic compression - the client only accepts stored (0) or zlib (1). Use Zlib.",
                        nameof(compressionFlag));
                }

                if (string.IsNullOrWhiteSpace(housingBinFile) || !File.Exists(housingBinFile))
                {
                    throw new FileNotFoundException(
                        "MultiCollection.uop must contain build/multicollection/housing.bin (the custom housing piece catalog). " +
                        "Extract it from the original UOP first and pass it to the packer.",
                        string.IsNullOrWhiteSpace(housingBinFile) ? "housing.bin" : housingBinFile);
                }
            }

            MultiComponentSidecar.Table componentTable = type == FileType.MultiCollection
                ? MultiComponentSidecar.Load(MultiComponentSidecar.ResolvePath(inFile, componentsFile))
                : null;

            if (type == FileType.MultiCollection && componentTable == null)
            {
                // Not fatal - a shard may have no component ids. The UI confirms first; this covers other callers.
                AppLog.For(typeof(LegacyMulFileConverter)).LogWarning(
                    "No multi component sidecar at {Path} - every tile in {OutFile} will be written with zero " +
                    "component ids, so boats lose their tiller man, hatch and planks and customisable houses lose their doors.",
                    MultiComponentSidecar.ResolvePath(inFile, componentsFile), outFile);
            }

            _unrepresentableMultiFlagTiles = 0;
            _emptyIdxEntriesSkipped = 0;
            _outOfRangeIdxEntriesSkipped = 0;

            try
            {
                WriteUop(inFile, inFileIdx, outFile, type, typeIndex, compressionFlag, housingBinFile, progress, componentTable);
            }
            catch
            {
                // Never leave a half written UOP behind - it would look like a usable file.
                TryDelete(outFile);
                throw;
            }

            ReportComponentSidecarProblems(componentTable);

            if (_emptyIdxEntriesSkipped > 0)
            {
                AppLog.For(typeof(LegacyMulFileConverter)).LogWarning(
                    "{Count} rows in {IdxFile} have a valid offset but a zero length. A zero byte entry is not a "
                    + "usable asset, so those ids were left out of {OutFile} - unpacking it writes them back as the "
                    + "-1 unused sentinel the client itself uses.",
                    _emptyIdxEntriesSkipped, inFileIdx, outFile);
            }

            if (_outOfRangeIdxEntriesSkipped > 0)
            {
                AppLog.For(typeof(LegacyMulFileConverter)).LogWarning(
                    "{Count} rows in {IdxFile} point past the end of {InFile}. Those ids were left out of {OutFile} "
                    + "rather than packed from truncated data.",
                    _outOfRangeIdxEntriesSkipped, inFileIdx, inFile, outFile);
            }

            if (_unrepresentableMultiFlagTiles > 0)
            {
                AppLog.For(typeof(LegacyMulFileConverter)).LogWarning(
                    "{Count} tiles in {InFile} carry multi.mul flag or extra bits outside the 0/1 range EA uses. " +
                    "MultiCollection.uop stores a single 16 bit visibility word, so only the visible and 0x0100 bits " +
                    "survive and the remaining bits were dropped.",
                    _unrepresentableMultiFlagTiles, inFile);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
                // Nothing useful to do; the original failure is the one that matters.
            }
            catch (UnauthorizedAccessException)
            {
                // As above.
            }
        }

        private static void WriteUop(string inFile, string inFileIdx, string outFile, FileType type, int typeIndex, CompressionFlag compressionFlag, string housingBinFile, IProgress<int> progress, MultiComponentSidecar.Table componentTable)
        {
            /*
             * The shipped client files come in two shapes, and which shape a type uses depends on the client
             * build rather than on the type alone. Measured over ten installs from 6.0.1.10 to the current
             * live client:
             *
             *   version 4, 100 entries per block, first block right behind the 0x28 byte header, every entry
             *   prefixed by a 12 byte header - MultiCollection and tileart in every build that has them,
             *   sound from 7.0.65.4 on, gumpart from 7.0.114.4 on.
             *
             *   version 5, 1000 entries per block, a large gap before the first block, entry headers of
             *   135..137 bytes whose last 128 bytes are high entropy (a signature block we cannot reproduce)
             *   - art and maps in every build, and sound/gumpart in the older ones.
             *
             * We target the newest client's shape. The declared block capacity has to agree with the blocks
             * actually written.
             */
            bool version4Layout = type == FileType.GumpartLegacyMul
                                  || type == FileType.SoundLegacyMul
                                  || type == FileType.MultiCollection;

            int tableSize = version4Layout ? 0x64 : 0x3E8;
            long firstTable = version4Layout ? 0x28 : 0x200;

            // Stamped once per file, not per entry, so a repack of the same input is byte identical. The
            // shipped files vary it per entry (a build machine timestamp), but nothing reads it back.
            long entryHeaderTimestamp = DateTime.UtcNow.ToFileTimeUtc();

            using (BinaryReader reader = OpenInput(inFile))
            using (BinaryReader readerIdx = OpenInput(inFileIdx))
            using (BinaryWriter writer = OpenOutput(outFile))
            {
                List<IdxEntry> idxEntries;

                if (type == FileType.MapLegacyMul)
                {
                    // No IDX file, just group the data into _mapChunkSize long chunks
                    int length = (int)reader.BaseStream.Length;
                    idxEntries = new List<IdxEntry>((int)Math.Ceiling((double)length / _mapChunkSize));

                    int position = 0;
                    int id = 0;

                    while (position < length)
                    {
                        IdxEntry e = new IdxEntry
                        {
                            Id = id++,
                            Offset = position,
                            Size = _mapChunkSize,
                            Extra = 0
                        };

                        idxEntries.Add(e);

                        position += _mapChunkSize;
                    }
                }
                else
                {
                    int idxEntryCount = (int)(readerIdx.BaseStream.Length / 12);
                    idxEntries = new List<IdxEntry>(idxEntryCount);

                    long mulLength = reader.BaseStream.Length;

                    for (int i = 0; i < idxEntryCount; ++i)
                    {
                        int offset = readerIdx.ReadInt32();
                        int size = readerIdx.ReadInt32();
                        int extra = readerIdx.ReadInt32();

                        // A negative offset is the unused id marker, and what FromUop writes back for an unused id.
                        if (offset < 0)
                        {
                            continue;
                        }

                        // Some patched muls mark unused ids with a zero length instead. A zero byte asset is not a
                        // thing, so drop those rows rather than pack empty uop entries; unpacking restores the -1.
                        if (size <= 0)
                        {
                            ++_emptyIdxEntriesSkipped;
                            continue;
                        }

                        // ReadBytes returns a short array at EOF instead of throwing, so a row that
                        // points past the end of the mul would otherwise be packed from truncated data.
                        if (offset >= mulLength || (long)offset + size > mulLength)
                        {
                            ++_outOfRangeIdxEntriesSkipped;
                            continue;
                        }

                        IdxEntry e = new IdxEntry
                        {
                            Id = i,
                            Offset = offset,
                            Size = size,
                            Extra = extra
                        };

                        idxEntries.Add(e);
                    }
                }

                if (type == FileType.MultiCollection && !string.IsNullOrWhiteSpace(housingBinFile) && File.Exists(housingBinFile))
                {
                    idxEntries.Add(new IdxEntry
                    {
                        Id = _housingBinSentinelId,
                        Offset = 0,
                        Size = 0,
                        Extra = 0
                    });
                }

                // File header
                writer.Write(0x50594D); // MYP
                writer.Write(version4Layout ? 4 : 5); // version
                writer.Write(0xFD23EC43); // format timestamp?
                writer.Write(firstTable); // first table
                writer.Write(tableSize); // table size
                writer.Write(idxEntries.Count); // file count
                writer.Write(0); // modified count? (wseq, version 5 only)
                writer.Write(0); // ? (cseq, version 5 only)
                writer.Write(0); // reserved

                // Padding
                for (long i = 0x28; i < firstTable; ++i)
                {
                    writer.Write((byte)0);
                }

                int tableCount = (int)Math.Ceiling((double)idxEntries.Count / tableSize);
                TableEntry[] tableEntries = new TableEntry[tableSize];

                string[] hashFormat = GetHashFormat(type, typeIndex, out int _);

                int totalEntries = idxEntries.Count;
                int lastReportedPct = -1;
                progress?.Report(0);

                for (int i = 0; i < tableCount; ++i)
                {
                    long thisTable = writer.BaseStream.Position;

                    int idxStart = i * tableSize;
                    int idxEnd = Math.Min((i + 1) * tableSize, idxEntries.Count);

                    // Table header
                    writer.Write(idxEnd - idxStart);
                    writer.Write((long)0); // next table, filled in later
                    writer.Seek(_tableEntrySize * tableSize, SeekOrigin.Current); // table entries, filled in later

                    // Data
                    int tableIdx = 0;

                    for (int j = idxStart; j < idxEnd; ++j, ++tableIdx)
                    {
                        byte[] data;

                        if (type == FileType.MultiCollection && idxEntries[j].Id == _housingBinSentinelId)
                        {
                            data = File.ReadAllBytes(housingBinFile);
                        }
                        else
                        {
                            reader.BaseStream.Seek(idxEntries[j].Offset, SeekOrigin.Begin);
                            data = reader.ReadBytes(idxEntries[j].Size);
                        }

                        tableEntries[tableIdx].Offset = writer.BaseStream.Position;
                        tableEntries[tableIdx].DecompressedSize = data.Length;
                        tableEntries[tableIdx].CompressionFlag = (short)compressionFlag;
                        tableEntries[tableIdx].HeaderLength = 0;

                        /*
                         * Every entry of every shipped version 4 UOP carries a 12 byte header block in front
                         * of its payload, and the 32 bit hash field of the table entry is the Adler32 of those
                         * 12 bytes, not of the payload - verified over 48897 version 4 entries of the current
                         * client plus 7.0.50.0, 100% header Adler32 and 0% payload Adler32. Version 5 entries
                         * use a hash we cannot reproduce, so they keep the payload Adler32 and a zero length
                         * header, which real clients accept.
                         */
                        byte[] entryHeader = null;
                        if (version4Layout)
                        {
                            entryHeader = BuildEntryHeader(entryHeaderTimestamp);
                            writer.Write(entryHeader);
                            tableEntries[tableIdx].HeaderLength = entryHeader.Length;
                        }

                        // hash 906142efe9fdb38a, which is file 0009834.tga (and no others, as 7.0.59.5) use a different name format (7 digits instead of 8);
                        //  if in newer versions more of these files will have adopted that format, someone should update this list of exceptions
                        //  (even if this seems so much like a typo from someone from the UO development team :P)
                        if ((type == FileType.GumpartLegacyMul) && (idxEntries[j].Id == 9834))
                        {
                            tableEntries[tableIdx].Identifier = HashLittle2(string.Format(hashFormat[1], idxEntries[j].Id));
                        }
                        else if (type == FileType.MultiCollection && idxEntries[j].Id == _housingBinSentinelId)
                        {
                            tableEntries[tableIdx].Identifier = _housingBinIdentifier;
                        }
                        else
                        {
                            tableEntries[tableIdx].Identifier = HashLittle2(string.Format(hashFormat[0], idxEntries[j].Id));
                        }

                        if (type == FileType.MultiCollection && idxEntries[j].Id != _housingBinSentinelId)
                        {
                            byte[] multiData = BuildMultiUopEntryFromMul(data, idxEntries[j].Id, componentTable);

                            tableEntries[tableIdx].DecompressedSize = multiData.Length;
                            tableEntries[tableIdx].Size = multiData.Length;

                            if (compressionFlag >= CompressionFlag.Zlib)
                            {
                                var result = UopUtils.Compress(multiData, _multiCollectionZlibLevel);
                                if (!result.success)
                                {
                                    throw new InvalidDataException($"Compression failed for multi {idxEntries[j].Id}.");
                                }
                                multiData = result.compressedData;
                                tableEntries[tableIdx].Size = multiData.Length;
                            }

                            tableEntries[tableIdx].Hash = HashAdler32(multiData);
                            writer.Write(multiData);
                        }
                        else if (type == FileType.GumpartLegacyMul)
                        {
                            byte[] gumpArtData = new byte[data.Length + 8];
                            using (MemoryStream ms = new MemoryStream(gumpArtData))
                            using (BinaryWriter gumpArtWriter = new BinaryWriter(ms))
                            {
                                int width = idxEntries[j].Extra >> 16 & 0xFFFF;
                                int height = idxEntries[j].Extra & 0xFFFF;

                                gumpArtWriter.Write(width);
                                gumpArtWriter.Write(height);
                                gumpArtWriter.Write(data);

                                tableEntries[tableIdx].DecompressedSize += 8;
                                tableEntries[tableIdx].Size = tableEntries[tableIdx].DecompressedSize;
                            }

                            if (compressionFlag == CompressionFlag.Mythic)
                            {
                                uint length = (uint)gumpArtData.Length;
                                gumpArtData = MythicDecompress.Transform(gumpArtData);
                                byte[] gumpArtData2 = new byte[gumpArtData.Length + 4];
                                using (MemoryStream ms2 = new MemoryStream(gumpArtData2))
                                {
                                    using (BinaryWriter writer2 = new BinaryWriter(ms2))
                                    {
                                        writer2.Write((uint)length ^ 0x8E2C9A3D);
                                        writer2.Write(gumpArtData);
                                    }
                                }
                                gumpArtData = gumpArtData2;
                                tableEntries[tableIdx].DecompressedSize = (int)gumpArtData.Length;
                                tableEntries[tableIdx].Size = tableEntries[tableIdx].DecompressedSize;
                            }
                            if (compressionFlag >= CompressionFlag.Zlib)
                            {
                                var result = UopUtils.Compress(gumpArtData);
                                if (!result.success)
                                {
                                    throw new InvalidDataException($"Compression failed for gump {idxEntries[j].Id}.");
                                }

                                tableEntries[tableIdx].Size = result.compressedData.Length;
                                gumpArtData = result.compressedData;
                            }
                            tableEntries[tableIdx].Hash = HashAdler32(gumpArtData);
                            writer.Write(gumpArtData);
                        }
                        else if (type == FileType.MultiCollection && idxEntries[j].Id == _housingBinSentinelId)
                        {
                            byte[] binData = data;
                            tableEntries[tableIdx].DecompressedSize = binData.Length;
                            tableEntries[tableIdx].Size = binData.Length;

                            if (compressionFlag >= CompressionFlag.Zlib)
                            {
                                var result = UopUtils.Compress(binData, _multiCollectionZlibLevel);
                                if (!result.success)
                                {
                                    throw new InvalidDataException("Compression failed for housing.bin.");
                                }
                                binData = result.compressedData;
                                tableEntries[tableIdx].Size = binData.Length;
                            }

                            tableEntries[tableIdx].Hash = HashAdler32(binData);
                            writer.Write(binData);
                        }
                        else
                        {
                            // Art / Map / Sound. The compression flag was already stamped on the entry above, so
                            // the data has to actually be compressed here - otherwise the entry claims zlib over
                            // raw bytes and neither the client nor FromUop can read it back.
                            byte[] payload = data;

                            if (compressionFlag == CompressionFlag.Mythic)
                            {
                                throw new ArgumentException(
                                    $"Mythic compression is only implemented for {nameof(FileType.GumpartLegacyMul)}, not for {type}.",
                                    nameof(compressionFlag));
                            }

                            if (compressionFlag == CompressionFlag.Zlib)
                            {
                                var result = UopUtils.Compress(payload);
                                if (!result.success)
                                {
                                    throw new InvalidDataException($"Compression failed for chunk {idxEntries[j].Id}.");
                                }

                                payload = result.compressedData;
                            }

                            tableEntries[tableIdx].Size = payload.Length;
                            tableEntries[tableIdx].Hash = HashAdler32(payload);
                            writer.Write(payload);
                        }

                        if (entryHeader != null)
                        {
                            tableEntries[tableIdx].Hash = HashAdler32(entryHeader);
                        }

                        if (totalEntries > 0)
                        {
                            int pct = (j + 1) * 100 / totalEntries;
                            if (pct != lastReportedPct)
                            {
                                lastReportedPct = pct;
                                progress?.Report(pct);
                            }
                        }
                    }

                    long nextTable = writer.BaseStream.Position;

                    // Go back and fix table header
                    if (i < tableCount - 1)
                    {
                        writer.BaseStream.Seek(thisTable + _nextBlockOffsetField, SeekOrigin.Begin);
                        writer.Write(nextTable);
                    }
                    else
                    {
                        writer.BaseStream.Seek(thisTable + _blockHeaderSize, SeekOrigin.Begin);
                        // No need to fix the next table address, it's the last
                    }

                    // Table entries
                    tableIdx = 0;

                    for (int j = idxStart; j < idxEnd; ++j, ++tableIdx)
                    {
                        writer.Write(tableEntries[tableIdx].Offset);
                        writer.Write(tableEntries[tableIdx].HeaderLength); // header length
                        writer.Write(tableEntries[tableIdx].Size); // compressed size
                        writer.Write(tableEntries[tableIdx].DecompressedSize); // decompressed size
                        writer.Write(tableEntries[tableIdx].Identifier);
                        writer.Write(tableEntries[tableIdx].Hash);
                        writer.Write(tableEntries[tableIdx].CompressionFlag); // compression method
                    }

                    // Fill remainder with empty entries
                    for (; tableIdx < tableSize; ++tableIdx)
                    {
                        writer.Write(_emptyTableEntry);
                    }

                    writer.BaseStream.Seek(nextTable, SeekOrigin.Begin);
                }
            }
        }

        private static void ReportComponentSidecarProblems(MultiComponentSidecar.Table componentTable)
        {
            if (componentTable == null)
            {
                return;
            }

            ILogger logger = AppLog.For(typeof(LegacyMulFileConverter));

            logger.LogInformation("UopPacker merged {RowCount} component rows from {Path}",
                componentTable.RowCount, componentTable.Path);

            foreach (string problem in componentTable.Problems)
            {
                logger.LogWarning("UopPacker component sidecar: {Problem}", problem);
            }
        }

        /// <summary>
        /// Entry count that makes <see cref="Ultima.Art.IsUOAHS"/> classify an artidx.mul as High Seas.
        /// Kept in sync with the 0x13FDC threshold in Ultima/Art.cs.
        /// </summary>
        private const int _uoahsArtIdxEntryCount = 0x13FDC;

        /// <summary>
        /// On disk size of one entry in a block's entry table:
        /// offset(8) headerLength(4) compressedSize(4) decompressedSize(4) identifier(8) hash(4) flag(2).
        /// </summary>
        private const int _tableEntrySize = 8 + 4 + 4 + 4 + 8 + 4 + 2;

        /// <summary>Size of a block header: usedEntryCount(4) nextBlockOffset(8).</summary>
        private const int _blockHeaderSize = 4 + 8;

        /// <summary>Offset of the next-block pointer inside a block header.</summary>
        private const int _nextBlockOffsetField = 4;

        private static readonly byte[] _emptyTableEntry = new byte[_tableEntrySize];

        /// <summary>
        /// The 12 byte block the client writes in front of every entry payload in a version 4 UOP:
        /// two constant shorts (3, 8) followed by a FILETIME. The (3, 8) pair holds across every
        /// version 4 entry of every install checked, from 7.0.50.0 to the current client.
        /// </summary>
        private static byte[] BuildEntryHeader(long fileTimeUtc)
        {
            byte[] header = new byte[12];

            BinaryPrimitives.WriteUInt16LittleEndian(header, 3);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(2), 8);
            BinaryPrimitives.WriteInt64LittleEndian(header.AsSpan(4), fileTimeUtc);

            return header;
        }

        //
        // UOP -> MUL
        //
        public void FromUop(string inFile, string outFile, string outFileIdx, FileType type, int typeIndex, string housingBinFile = "", IProgress<int> progress = null, string componentsFile = "")
        {
            Dictionary<ulong, int> chunkIds = new Dictionary<ulong, int>();
            Dictionary<ulong, int> chunkIds2 = new Dictionary<ulong, int>();

            string[] formats = GetHashFormat(type, typeIndex, out var maxId);

            for (int i = 0; i < maxId; ++i)
            {
                chunkIds[HashLittle2(string.Format(formats[0], i))] = i;
            }

            if (formats[1] != string.Empty)
            {
                for (int i = 0; i < maxId; ++i)
                {
                    chunkIds2[HashLittle2(string.Format(formats[1], i))] = i;
                }
            }

            bool[] used = new bool[maxId];

            // multi.mul rows have nowhere to put the per tile component ids, so they go beside it.
            string componentsPath = type != FileType.MultiCollection
                ? null
                : string.IsNullOrWhiteSpace(componentsFile)
                    ? MultiComponentSidecar.GetDefaultPath(outFile)
                    : componentsFile;

            using (BinaryReader reader = OpenInput(inFile))
            using (BinaryWriter mulWriter = OpenOutput(outFile))
            using (BinaryWriter idxWriter = OpenOutput(outFileIdx))
            using (MultiComponentSidecar.Writer componentWriter = string.IsNullOrWhiteSpace(componentsPath) ? null : MultiComponentSidecar.CreateWriter(componentsPath))
            {
                if (reader.ReadInt32() != 0x50594D) // MYP
                {
                    throw new ArgumentException("Input file is not a UOP file.");
                }

                Stream stream = reader.BaseStream;

                reader.ReadInt32(); // version ?
                reader.ReadInt32(); // format timestamp? 0xFD23EC43

                long nextTable = reader.ReadInt64();
                reader.ReadInt32(); // table size (unused)
                int totalFileCount = reader.ReadInt32();
                int processedCount = 0;
                int lastReportedPct = -1;
                progress?.Report(0);

                do
                {
                    // Table header
                    stream.Seek(nextTable, SeekOrigin.Begin);
                    int entries = reader.ReadInt32();
                    nextTable = reader.ReadInt64();

                    // Table entries
                    TableEntry[] offsets = new TableEntry[entries];

                    for (int i = 0; i < entries; ++i)
                    {
                        /*
                         * Empty entries are read too, because they do not always indicate the
                         * end of the table. (Example: 7.0.26.4+ Fel/Tram maps)
                         */
                        offsets[i].Offset = reader.ReadInt64();
                        offsets[i].HeaderLength = reader.ReadInt32(); // header length
                        offsets[i].Size = reader.ReadInt32(); // compressed size
                        offsets[i].DecompressedSize = reader.ReadInt32(); // decompressed size
                        offsets[i].Identifier = reader.ReadUInt64(); // filename hash (HashLittle2)
                        offsets[i].Hash = reader.ReadUInt32(); // data hash (Adler32)
                        offsets[i].CompressionFlag = reader.ReadInt16(); // compression method (0 = none, 1 = zlib, 3 = mythic)
                        offsets[i].Compressed = offsets[i].CompressionFlag != 0;
                    }

                    // Copy chunks
                    for (int i = 0; i < offsets.Length; ++i)
                    {
                        if (offsets[i].Offset == 0)
                        {
                            continue; // skip empty entry
                        }

                        // extract housing.bin file (not really needed for muls to work but needed later to pack files back to uop)
                        if ((type == FileType.MultiCollection) && (offsets[i].Identifier == _housingBinIdentifier))
                        {
                            // MultiCollection.uop has the file "build/multicollection/housing.bin", which has to be
                            // handled separately. It has no id in the hash lookup, so it must be consumed here even
                            // when no output path was given - otherwise it falls through as an unknown identifier.
                            if (!string.IsNullOrWhiteSpace(housingBinFile))
                            {
                                using BinaryWriter writerBin = OpenOutput(housingBinFile);

                                stream.Seek(offsets[i].Offset + offsets[i].HeaderLength, SeekOrigin.Begin);

                                byte[] binData = reader.ReadBytes(offsets[i].Size);
                                byte[] binDataToWrite;

                                if (offsets[i].Compressed)
                                {
                                    using ZLibStream zlib = new(new MemoryStream(binData), CompressionMode.Decompress);

                                    byte[] decompressed = new byte[offsets[i].DecompressedSize];
                                    zlib.ReadExactly(decompressed);
                                    binDataToWrite = decompressed;
                                }
                                else
                                {
                                    binDataToWrite = binData;
                                }

                                writerBin.Write(binDataToWrite, 0, binDataToWrite.Length);
                            }

                            if (totalFileCount > 0)
                            {
                                ++processedCount;
                                int pct = processedCount * 100 / totalFileCount;
                                if (pct != lastReportedPct)
                                {
                                    lastReportedPct = pct;
                                    progress?.Report(pct);
                                }
                            }

                            continue;
                        }

                        if (!chunkIds.TryGetValue(offsets[i].Identifier, out var chunkId))
                        {
                            if (!chunkIds2.TryGetValue(offsets[i].Identifier, out int chunkId2))
                            {
                                throw new Exception($"Unknown identifier encountered ({offsets[i].Identifier:X})");
                            }
                            else
                            {
                                // the second collection is used because in some versions GumpartLegacyMul.uop had shorter Identifier
                                chunkId = chunkId2;
                            }
                        }

                        stream.Seek(offsets[i].Offset + offsets[i].HeaderLength, SeekOrigin.Begin);

                        byte[] chunkData = reader.ReadBytes(offsets[i].Size);
                        if (offsets[i].Compressed)
                        {
                            using ZLibStream zlib = new(new MemoryStream(chunkData), CompressionMode.Decompress);

                            byte[] decompressed = new byte[offsets[i].DecompressedSize];
                            zlib.ReadExactly(decompressed);
                            chunkData = decompressed;
                        }

                        if (offsets[i].CompressionFlag == (short)CompressionFlag.Mythic)
                        {
                            uint mythicLen = MythicDecompress.PeekDecompressedLength(chunkData);
                            if (mythicLen == 0 || mythicLen > int.MaxValue)
                            {
                                throw new InvalidDataException(
                                    $"Mythic header reports invalid decompressed length {mythicLen} for chunk {chunkId}.");
                            }

                            byte[] mythicOutput = new byte[mythicLen];
                            if (!MythicDecompress.TryDecompress(chunkData, mythicOutput, out _))
                            {
                                throw new InvalidDataException(
                                    $"Mythic decompression failed for chunk {chunkId}.");
                            }
                            chunkData = mythicOutput;
                        }

                        if (type == FileType.MapLegacyMul)
                        {
                            // Write this chunk on the right position (no IDX file to point to it)
                            // Through BaseStream: BinaryWriter.Seek only takes an int, and a large
                            // custom facet can push the offset past int.MaxValue.
                            mulWriter.BaseStream.Seek((long)chunkId * _mapChunkSize, SeekOrigin.Begin);
                            mulWriter.Write(chunkData);
                        }
                        else
                        {
                            int dataOffset = 0;

                            #region Idx

                            idxWriter.Seek(chunkId * 12, SeekOrigin.Begin);
                            idxWriter.Write((uint)mulWriter.BaseStream.Position); // Position

                            switch (type)
                            {
                                case FileType.GumpartLegacyMul:
                                    {
                                        // Width and height are prepended to the data
                                        int width = chunkData[0] | chunkData[1] << 8 | chunkData[2] << 16 | chunkData[3] << 24;
                                        int height = chunkData[4] | chunkData[5] << 8 | chunkData[6] << 16 | chunkData[7] << 24;

                                        idxWriter.Write(chunkData.Length - 8);
                                        idxWriter.Write(width << 16 | height);
                                        dataOffset = 8;
                                        break;
                                    }
                                case FileType.SoundLegacyMul:
                                    {
                                        // Extra contains the ID of this sound file + 1
                                        idxWriter.Write(chunkData.Length);
                                        idxWriter.Write(chunkId + 1);
                                        break;
                                    }
                                case FileType.MultiCollection:
                                    {
                                        long startPosition = mulWriter.BaseStream.Position;
                                        WriteMultiUopEntryToMul(mulWriter, chunkData, chunkId, componentWriter);
                                        long endPosition = mulWriter.BaseStream.Position;

                                        idxWriter.Write((int)(endPosition - startPosition)); // Size
                                        idxWriter.Write(0); // Extra
                                        break;
                                    }
                                default:
                                    {
                                        idxWriter.Write(chunkData.Length); // Size
                                        idxWriter.Write(0); // Extra
                                        break;
                                    }
                            }

                            used[chunkId] = true;
                            #endregion

                            if (type != FileType.MultiCollection)
                            {
                                mulWriter.Write(chunkData, dataOffset, chunkData.Length - dataOffset);
                            }
                        }

                        if (totalFileCount > 0)
                        {
                            ++processedCount;
                            int pct = processedCount * 100 / totalFileCount;
                            if (pct != lastReportedPct)
                            {
                                lastReportedPct = pct;
                                progress?.Report(pct);
                            }
                        }
                    }

                    // Move to next table
                    if (nextTable != 0)
                    {
                        stream.Seek(nextTable, SeekOrigin.Begin);
                    }
                }
                while (nextTable != 0);

                // Fix index. Only pad up to the highest used entry — `used.Length` is the hash-lookup
                // upper bound (often 0x7FFFF), which would otherwise produce a multi-megabyte idx file
                // padded with sentinel rows beyond any real entry.
                if (idxWriter != null)
                {
                    int padCount = 0;
                    for (int i = used.Length - 1; i >= 0; --i)
                    {
                        if (used[i])
                        {
                            padCount = i + 1;
                            break;
                        }
                    }

                    /*
                     * Art is the exception: Art.IsUOAHS() classifies a client by the entry count of artidx.mul
                     * (>= 0x13FDC means High Seas), and that also picks the multi.mul row size and the tiledata
                     * layout. The highest populated art id in the shipped UOPs is around 62700, so padding only to
                     * the highest used entry downgrades every unpacked art set to pre-Stygian-Abyss limits.
                     */
                    if (type == FileType.ArtLegacyMul)
                    {
                        padCount = Math.Max(padCount, _uoahsArtIdxEntryCount);
                    }

                    for (int i = 0; i < padCount; ++i)
                    {
                        if (used[i])
                        {
                            continue;
                        }

                        idxWriter.Seek(i * 12, SeekOrigin.Begin);

                        idxWriter.Write(-1); // Position (lookup)
                        idxWriter.Write((long)0); // Size + Extra
                    }
                }
            }

            CheckAndFixMapFiles(outFile, type, typeIndex);
        }

        /// <summary>
        /// Cleanup extra blocks at the end of unpacked mul files.
        /// </summary>
        /// <remarks>
        /// For some reason some of the maps have extra 196 and the end after unpacking.
        /// A lot of tools expect exact mul file size so we remove excessive bytes.
        /// </remarks>
        private static void CheckAndFixMapFiles(string outFile, FileType type, int typeIndex)
        {
            if (type != FileType.MapLegacyMul)
            {
                return;
            }

            int expectedSize = GetExpectedMapFileSize(typeIndex);

            if (expectedSize == 0)
            {
                // do nothing. Map file is wrong, or it's some weird size we don't know about
                return;
            }

            using (var mapFile = File.Open(outFile, FileMode.Open, FileAccess.ReadWrite))
            {
                long sizeDiff = mapFile.Length - expectedSize;
                if (sizeDiff <= 0)
                {
                    return;
                }

                /*
                 * The overshoot we are here to remove is chunk padding: the UOP stores the map in 0xC4000 byte
                 * chunks, so the last one runs past the end of the facet by less than a chunk (752 640 bytes for
                 * map2, 1 372 for map4, nothing for map0/1). Anything larger is a custom map that is genuinely
                 * bigger than the stock facet, and truncating it would throw away real terrain.
                 */
                if (sizeDiff >= _mapChunkSize)
                {
                    AppLog.For(typeof(LegacyMulFileConverter)).LogInformation(
                        "{OutFile} is {Actual:N0} bytes, {Diff:N0} more than the stock facet {Index} size of {Expected:N0}. " +
                        "That is more than one {ChunkSize:N0} byte chunk of padding, so it looks like a custom map and was left untrimmed.",
                        outFile, mapFile.Length, sizeDiff, typeIndex, expectedSize, _mapChunkSize);

                    return;
                }

                mapFile.SetLength(expectedSize);
            }
        }

        private static int GetExpectedMapFileSize(int typeIndex)
        {
            return typeIndex switch
            {
                0 => 89_915_392,
                1 => 89_915_392,
                2 => 11_289_600,
                3 => 16_056_320,
                4 => 6_421_156,
                5 => 16_056_320,
                _ => 0
            };
        }

        //
        // Hash filename formats (remember: lower case!)
        //
        private static string[] GetHashFormat(FileType type, int typeIndex, out int maxId)
        {
            /*
             * MaxID is only used for constructing a lookup table.
             * Decrease to save some possibly unneeded computation.
             */
            maxId = 0x7FFFF;

            switch (type)
            {
                case FileType.ArtLegacyMul:
                    {
                        maxId = _uoahsArtIdxEntryCount;
                        return ["build/artlegacymul/{0:00000000}.tga", string.Empty];
                    }
                case FileType.GumpartLegacyMul:
                    {
                        // maxId = 0xEF3C on 7.0.8.2
                        return ["build/gumpartlegacymul/{0:00000000}.tga", "build/gumpartlegacymul/{0:0000000}.tga"];
                    }
                case FileType.MapLegacyMul:
                    {
                        // maxId = 0x71 on 7.0.8.2 for Fel/Tram
                        return [string.Concat("build/map", typeIndex, "legacymul/{0:00000000}.dat"), string.Empty];
                    }
                case FileType.SoundLegacyMul:
                    {
                        // maxId = 0x1000 on 7.0.8.2
                        return ["build/soundlegacymul/{0:00000000}.dat", string.Empty];
                    }
                case FileType.MultiCollection:
                    {
                        maxId = 0x2710; // newer clients add multis past 0x2200 (e.g. 9000); keep generous for future entries
                        return ["build/multicollection/{0:000000}.bin", string.Empty];
                    }
                default:
                    {
                        throw new ArgumentException("Unknown file type!");
                    }
            }
        }

        /// <summary>
        /// Jenkins lookup3 hashlittle2 over a UOP entry path - see <see cref="UopUtils.HashFileName"/>.
        /// </summary>
        private static ulong HashLittle2(string input) => UopUtils.HashFileName(input);

        private static uint HashAdler32(byte[] d)
        {
            uint a = 1;
            uint b = 0;

            for (int i = 0; i < d.Length; i++)
            {
                a = (a + d[i]) % 65521;
                b = (b + a) % 65521;
            }

            return b << 16 | a;
        }

        /*
         * MUL row layout: [itemId:2][x:2][y:2][z:2][flag:4][extra:4] = 16 bytes (High Seas / 7.0.9+)
         * UOP tile:       [itemId:2][x:2][y:2][z:2][flag:2][componentCount:4] = 14 bytes, followed by
         *                 componentCount 32 bit component ids.
         *
         * The two flag fields map like this - derived from the 53261 tiles that can be matched by
         * id/x/y/z between the shipped MultiCollection.uop and the shipped multi.mul, with no exception:
         *
         *     mul flag  = (uopFlag & 0x0001) != 0 ? 0 : 1
         *     mul extra = (uopFlag & 0x0100) != 0 ? 1 : 0
         *
         * So the "unknown" trailing int32 of the High Seas mul row is where bit 0x0100 lives. In the
         * shipped file 8207 of 186695 tiles have it set; folding it into bit 0 (or dropping it) loses it.
         */
        private const ushort _uopTileFlagLow = 0x0001;
        private const ushort _uopTileFlagHigh = 0x0100;
        private const int _mulRowSize = 16;
        private const int _uopTileSize = 14;

        private static void WriteMultiUopEntryToMul(BinaryWriter mulWriter, byte[] chunkData, int multiId, MultiComponentSidecar.Writer componentWriter)
        {
            ReadOnlySpan<byte> data = chunkData.AsSpan();

            if (data.Length < 8)
            {
                throw new InvalidDataException($"Multi {multiId}: entry is {data.Length} bytes, too short to hold a header.");
            }

            uint count = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
            int position = 8;

            for (int i = 0; i < count; i++)
            {
                if (position + _uopTileSize > data.Length)
                {
                    throw new InvalidDataException(
                        $"Multi {multiId}: tile {i} of {count} runs past the end of the {data.Length} byte entry.");
                }

                ReadOnlySpan<byte> tile = data[position..];

                ushort itemId = BinaryPrimitives.ReadUInt16LittleEndian(tile);
                short x = BinaryPrimitives.ReadInt16LittleEndian(tile[2..]);
                short y = BinaryPrimitives.ReadInt16LittleEndian(tile[4..]);
                short z = BinaryPrimitives.ReadInt16LittleEndian(tile[6..]);
                ushort flagValue = BinaryPrimitives.ReadUInt16LittleEndian(tile[8..]);
                uint componentCount = BinaryPrimitives.ReadUInt32LittleEndian(tile[10..]);

                long tileSize = _uopTileSize + (long)componentCount * 4;
                if (position + tileSize > data.Length)
                {
                    throw new InvalidDataException(
                        $"Multi {multiId}: tile {i} claims {componentCount} component ids, which runs past the end of the {data.Length} byte entry.");
                }

                if (componentCount > 0 && componentWriter != null)
                {
                    uint[] componentIds = new uint[componentCount];
                    for (int c = 0; c < componentIds.Length; ++c)
                    {
                        componentIds[c] = BinaryPrimitives.ReadUInt32LittleEndian(tile[(_uopTileSize + c * 4)..]);
                    }

                    componentWriter.Write(multiId, i, itemId, x, y, z, componentIds);
                }

                position += (int)tileSize;

                mulWriter.Write(itemId);
                mulWriter.Write(x);
                mulWriter.Write(y);
                mulWriter.Write(z);
                mulWriter.Write((flagValue & _uopTileFlagLow) != 0 ? 0 : 1);
                mulWriter.Write((flagValue & _uopTileFlagHigh) != 0 ? 1 : 0);
            }
        }

        private static byte[] BuildMultiUopEntryFromMul(byte[] mulData, int multiId, MultiComponentSidecar.Table components)
        {
            if (mulData.Length % _mulRowSize != 0)
            {
                throw new InvalidDataException(
                    $"Multi {multiId}: {mulData.Length} bytes is not a whole number of 16 byte rows. " +
                    "MultiCollection.uop can only be built from a High Seas (7.0.9+) multi.mul; " +
                    "the older 12 byte row format is not supported.");
            }

            int tileCount = mulData.Length / _mulRowSize;

            // Component ids make the tile records variable length, so resolve them before sizing the buffer.
            uint[][] componentIds = new uint[tileCount][];
            int totalComponents = 0;

            ReadOnlySpan<byte> source = mulData.AsSpan();

            for (int i = 0; i < tileCount; i++)
            {
                ReadOnlySpan<byte> row = source[(i * _mulRowSize)..];

                componentIds[i] = components?.GetComponentIds(
                    multiId,
                    i,
                    BinaryPrimitives.ReadUInt16LittleEndian(row),
                    BinaryPrimitives.ReadInt16LittleEndian(row[2..]),
                    BinaryPrimitives.ReadInt16LittleEndian(row[4..]),
                    BinaryPrimitives.ReadInt16LittleEndian(row[6..])) ?? Array.Empty<uint>();

                totalComponents += componentIds[i].Length;
            }

            byte[] result = new byte[8 + tileCount * _uopTileSize + totalComponents * 4];

            Span<byte> dst = result.AsSpan();
            BinaryPrimitives.WriteUInt32LittleEndian(dst, (uint)multiId);
            BinaryPrimitives.WriteUInt32LittleEndian(dst[4..], (uint)tileCount);
            dst = dst[8..];

            for (int i = 0; i < tileCount; i++)
            {
                ReadOnlySpan<byte> row = source[(i * _mulRowSize)..];

                ushort itemId = BinaryPrimitives.ReadUInt16LittleEndian(row);
                short x = BinaryPrimitives.ReadInt16LittleEndian(row[2..]);
                short y = BinaryPrimitives.ReadInt16LittleEndian(row[4..]);
                short z = BinaryPrimitives.ReadInt16LittleEndian(row[6..]);
                int mulFlag = BinaryPrimitives.ReadInt32LittleEndian(row[8..]);
                int mulExtra = BinaryPrimitives.ReadInt32LittleEndian(row[12..]);

                /*
                 * The uop side has a single 16 bit visibility word where the mul has two 32 bit ints, so only
                 * the two bits EA uses survive. Lossless for every real file - across ten installs multi.mul
                 * flags and extra are only ever 0 or 1 - but a hand authored mul using the community bit
                 * assignments (0x2 Trim, 0x8 Door, 0x20 Wall, ...) has nowhere to put them, so count and report.
                 */
                if ((mulFlag & ~1) != 0 || (mulExtra & ~1) != 0)
                {
                    ++_unrepresentableMultiFlagTiles;
                }

                // Exact inverse of WriteMultiUopEntryToMul.
                ushort uopFlag = (ushort)((mulFlag == 0 ? _uopTileFlagLow : 0) | (mulExtra != 0 ? _uopTileFlagHigh : 0));

                uint[] ids = componentIds[i];

                BinaryPrimitives.WriteUInt16LittleEndian(dst, itemId);
                BinaryPrimitives.WriteInt16LittleEndian(dst[2..], x);
                BinaryPrimitives.WriteInt16LittleEndian(dst[4..], y);
                BinaryPrimitives.WriteInt16LittleEndian(dst[6..], z);
                BinaryPrimitives.WriteUInt16LittleEndian(dst[8..], uopFlag);
                BinaryPrimitives.WriteUInt32LittleEndian(dst[10..], (uint)ids.Length);

                for (int c = 0; c < ids.Length; ++c)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(dst[(_uopTileSize + c * 4)..], ids[c]);
                }

                dst = dst[(_uopTileSize + ids.Length * 4)..];
            }

            return result;
        }
    }
}
