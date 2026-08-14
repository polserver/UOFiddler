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

        // Identifier for "build/multicollection/housing.bin" inside MultiCollection.uop.
        private const ulong _housingBinIdentifier = 0x126D1E99DDEDEE0A;

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
                ? MultiComponentSidecar.Load(string.IsNullOrWhiteSpace(componentsFile)
                    ? MultiComponentSidecar.GetDefaultPath(inFile)
                    : componentsFile)
                : null;

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
            const int tableSize = 0x64;

            /*
             * The shipped client files come in two shapes: version 4 with 100 entries per block and the
             * first block right behind the 0x28 byte header (MultiCollection, gumpart, sound, tileart,
             * AnimationSequence), and version 5 with 1000 entries per block and a large gap before the
             * first block (art, maps). We only ever write 100 entry blocks, so anything using that layout
             * has to declare version 4 as well - MultiCollection.uop in particular, which was previously
             * written as a version 5 header with a version 4 body.
             */
            bool version4Layout = type == FileType.GumpartLegacyMul || type == FileType.MultiCollection;
            long firstTable = version4Layout ? 0x28 : 0x200;

            using (BinaryReader reader = OpenInput(inFile))
            using (BinaryReader readerIdx = OpenInput(inFileIdx))
            using (BinaryWriter writer = OpenOutput(outFile))
            {
                List<IdxEntry> idxEntries;

                if (type == FileType.MapLegacyMul)
                {
                    // No IDX file, just group the data into 0xC4000 long chunks
                    int length = (int)reader.BaseStream.Length;
                    idxEntries = new List<IdxEntry>((int)Math.Ceiling((double)length / 0xC4000));

                    int position = 0;
                    int id = 0;

                    while (position < length)
                    {
                        IdxEntry e = new IdxEntry
                        {
                            Id = id++,
                            Offset = position,
                            Size = 0xC4000,
                            Extra = 0
                        };

                        idxEntries.Add(e);

                        position += 0xC4000;
                    }
                }
                else
                {
                    int idxEntryCount = (int)(readerIdx.BaseStream.Length / 12);
                    idxEntries = new List<IdxEntry>(idxEntryCount);

                    for (int i = 0; i < idxEntryCount; ++i)
                    {
                        int offset = readerIdx.ReadInt32();

                        if (offset < 0)
                        {
                            readerIdx.BaseStream.Seek(8, SeekOrigin.Current); // skip
                            continue;
                        }

                        IdxEntry e = new IdxEntry
                        {
                            Id = i,
                            Offset = offset,
                            Size = readerIdx.ReadInt32(),
                            Extra = readerIdx.ReadInt32()
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
                    writer.Seek(34 * tableSize, SeekOrigin.Current); // table entries, filled in later

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
                         * 12 bytes - not of the payload (verified against 48897 entries across MultiCollection,
                         * tileart, AnimationSequence, soundLegacyMUL and gumpartLegacyMUL). Reproduce that for
                         * MultiCollection; the remaining types keep their old behaviour for now, which does not
                         * match the shipped files either.
                         */
                        byte[] entryHeader = null;
                        if (type == FileType.MultiCollection)
                        {
                            entryHeader = BuildEntryHeader();
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
                        writer.BaseStream.Seek(thisTable + 4, SeekOrigin.Begin);
                        writer.Write(nextTable);
                    }
                    else
                    {
                        writer.BaseStream.Seek(thisTable + 12, SeekOrigin.Begin);
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

        private static readonly byte[] _emptyTableEntry = new byte[8 + 4 + 4 + 4 + 8 + 4 + 2];

        /// <summary>
        /// The 12 byte block the client writes in front of every entry payload in a version 4 UOP:
        /// two constant shorts (3, 8) followed by a FILETIME. Constant across all 48897 entries of the
        /// five shipped version 4 UOPs.
        /// </summary>
        private static byte[] BuildEntryHeader()
        {
            byte[] header = new byte[12];

            BinaryPrimitives.WriteUInt16LittleEndian(header, 3);
            BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(2), 8);
            BinaryPrimitives.WriteInt64LittleEndian(header.AsSpan(4), DateTime.UtcNow.ToFileTimeUtc());

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
                            mulWriter.Seek(chunkId * 0xC4000, SeekOrigin.Begin);
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
                var sizeDiff = mapFile.Length - expectedSize;
                if (sizeDiff > 0)
                {
                    mapFile.SetLength(mapFile.Length - sizeDiff);
                }
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
                        maxId = 0x13FDC; // UOFiddler requires this exact index length to recognize UOHS art files
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

        //
        // Hash functions (EA didn't write these, see http://burtleburtle.net/bob/c/lookup3.c)
        //
        private static ulong HashLittle2(string s)
        {
            int length = s.Length;

            uint a, b, c;
            a = b = c = 0xDEADBEEF + (uint)length;

            int k = 0;

            while (length > 12)
            {
                a += s[k];
                a += (uint)s[k + 1] << 8;
                a += (uint)s[k + 2] << 16;
                a += (uint)s[k + 3] << 24;
                b += s[k + 4];
                b += (uint)s[k + 5] << 8;
                b += (uint)s[k + 6] << 16;
                b += (uint)s[k + 7] << 24;
                c += s[k + 8];
                c += (uint)s[k + 9] << 8;
                c += (uint)s[k + 10] << 16;
                c += (uint)s[k + 11] << 24;

                a -= c; a ^= c << 4 | c >> 28; c += b;
                b -= a; b ^= a << 6 | a >> 26; a += c;
                c -= b; c ^= b << 8 | b >> 24; b += a;
                a -= c; a ^= c << 16 | c >> 16; c += b;
                b -= a; b ^= a << 19 | a >> 13; a += c;
                c -= b; c ^= b << 4 | b >> 28; b += a;

                length -= 12;
                k += 12;
            }

            if (length == 0)
            {
                return (ulong)b << 32 | c;
            }

            switch (length)
            {
                case 12: c += (uint)s[k + 11] << 24; goto case 11;
                case 11: c += (uint)s[k + 10] << 16; goto case 10;
                case 10: c += (uint)s[k + 9] << 8; goto case 9;
                case 9: c += s[k + 8]; goto case 8;
                case 8: b += (uint)s[k + 7] << 24; goto case 7;
                case 7: b += (uint)s[k + 6] << 16; goto case 6;
                case 6: b += (uint)s[k + 5] << 8; goto case 5;
                case 5: b += s[k + 4]; goto case 4;
                case 4: a += (uint)s[k + 3] << 24; goto case 3;
                case 3: a += (uint)s[k + 2] << 16; goto case 2;
                case 2: a += (uint)s[k + 1] << 8; goto case 1;
                case 1: a += s[k]; break;
            }

            c ^= b; c -= b << 14 | b >> 18;
            a ^= c; a -= c << 11 | c >> 21;
            b ^= a; b -= a << 25 | a >> 7;
            c ^= b; c -= b << 16 | b >> 16;
            a ^= c; a -= c << 4 | c >> 28;
            b ^= a; b -= a << 14 | a >> 18;
            c ^= b; c -= b << 24 | b >> 8;

            return (ulong)b << 32 | c;
        }

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
