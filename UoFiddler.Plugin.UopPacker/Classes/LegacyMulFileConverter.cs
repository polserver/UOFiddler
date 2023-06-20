using System;
using System.Collections.Generic;
using System.IO;

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
            public ulong Identifier;
            public uint Hash;
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

        //
        // MUL -> UOP
        //
        public static void ToUop(string inFile, string inFileIdx, string outFile, FileType type, int typeIndex)
        {
            // Same for all UOP files
            const long firstTable = 0x200;
            const int tableSize = 0x64;

#pragma warning disable 162
            // Sanity, in case firstTable is customized by you!
            if (firstTable < 0x28)
            {
                throw new Exception("At least 0x28 bytes are needed for the header.");
            }
#pragma warning restore 162

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

                // File header
                writer.Write(0x50594D); // MYP
                writer.Write(5); // version
                writer.Write(0xFD23EC43); // format timestamp?
                writer.Write(firstTable); // first table
                writer.Write(tableSize); // table size
                writer.Write(idxEntries.Count); // file count
                writer.Write(1); // modified count?
                writer.Write(1); // ?
                writer.Write(0); // ?

                // Padding
                for (int i = 0x28; i < firstTable; ++i)
                {
                    writer.Write((byte)0);
                }

                int tableCount = (int)Math.Ceiling((double)idxEntries.Count / tableSize);
                TableEntry[] tableEntries = new TableEntry[tableSize];

                string hashFormat = GetHashFormat(type, typeIndex, out int _);

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
                        reader.BaseStream.Seek(idxEntries[j].Offset, SeekOrigin.Begin);
                        byte[] data = reader.ReadBytes(idxEntries[j].Size);

                        tableEntries[tableIdx].Offset = writer.BaseStream.Position;
                        tableEntries[tableIdx].Size = data.Length;
                        tableEntries[tableIdx].Identifier = HashLittle2(string.Format(hashFormat, idxEntries[j].Id));
                        tableEntries[tableIdx].Hash = HashAdler32(data);

                        if (type == FileType.GumpartLegacyMul)
                        {
                            // Prepend width/height from IDX's extra
                            int width = idxEntries[j].Extra >> 16 & 0xFFFF;
                            int height = idxEntries[j].Extra & 0xFFFF;

                            writer.Write(width);
                            writer.Write(height);

                            tableEntries[tableIdx].Size += 8;
                        }

                        writer.Write(data);
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
                        writer.Write(0); // header length
                        writer.Write(tableEntries[tableIdx].Size); // compressed size
                        writer.Write(tableEntries[tableIdx].Size); // decompressed size
                        writer.Write(tableEntries[tableIdx].Identifier);
                        writer.Write(tableEntries[tableIdx].Hash);
                        writer.Write((short)0); // compression method, none
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

        private static readonly byte[] _emptyTableEntry = new byte[8 + 4 + 4 + 4 + 8 + 4 + 2];

        //
        // UOP -> MUL
        //
        public void FromUop(string inFile, string outFile, string outFileIdx, FileType type, int typeIndex)
        {
            Dictionary<ulong, int> chunkIds = new Dictionary<ulong, int>();

            string format = GetHashFormat(type, typeIndex, out int maxId);

            for (int i = 0; i < maxId; ++i)
            {
                chunkIds[HashLittle2(string.Format(format, i))] = i;
            }

            bool[] used = new bool[maxId];

            using (BinaryReader reader = OpenInput(inFile))
            using (BinaryWriter mulWriter = OpenOutput(outFile))
            using (BinaryWriter idxWriter = OpenOutput(outFileIdx))
            {
                if (reader.ReadInt32() != 0x50594D) // MYP
                {
                    throw new ArgumentException("inFile is not a UOP file.");
                }

                Stream stream = reader.BaseStream;

                reader.ReadInt32(); // version ?
                reader.ReadInt32(); // format timestamp? 0xFD23EC43

                long nextTable = reader.ReadInt64();

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
                        reader.ReadInt32(); // decompressed size
                        offsets[i].Identifier = reader.ReadUInt64(); // filename hash (HashLittle2)
                        offsets[i].Hash = reader.ReadUInt32(); // data hash (Adler32)
                        reader.ReadInt16(); // compression method (0 = none, 1 = zlib)
                    }

                    // Copy chunks
                    for (int i = 0; i < offsets.Length; ++i)
                    {
                        if (offsets[i].Offset == 0)
                        {
                            continue; // skip empty entry
                        }

                        if (!chunkIds.TryGetValue(offsets[i].Identifier, out int chunkId))
                        {
                            throw new Exception("Unknown identifier encountered");
                        }

                        stream.Seek(offsets[i].Offset + offsets[i].HeaderLength, SeekOrigin.Begin);
                        byte[] chunkData = reader.ReadBytes(offsets[i].Size);

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
                                default:
                                    {
                                        idxWriter.Write(chunkData.Length); // Size
                                        idxWriter.Write(0); // Extra
                                        break;
                                    }
                            }

                            used[chunkId] = true;
                            #endregion

                            mulWriter.Write(chunkData, dataOffset, chunkData.Length - dataOffset);
                        }
                    }

                    // Move to next table
                    if (nextTable != 0)
                    {
                        stream.Seek(nextTable, SeekOrigin.Begin);
                    }
                }
                while (nextTable != 0);

                // Fix index
                if (idxWriter != null)
                {
                    for (int i = 0; i < used.Length; ++i)
                    {
                        if (used[i])
                        {
                            continue;
                        }

                        idxWriter.Seek(i * 12, SeekOrigin.Begin);
                        idxWriter.Write(-1);
                        idxWriter.Write((long)0);
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
                // do nothing. Map file is wrong or it's some weird size we don't know about
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
                4 =>  6_421_156,
                5 => 16_056_320,
                _ => 0
            };
        }

        //
        // Hash filename formats (remember: lower case!)
        //
        private static string GetHashFormat(FileType type, int typeIndex, out int maxId)
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
                        return "build/artlegacymul/{0:00000000}.tga";
                    }
                case FileType.GumpartLegacyMul:
                    {
                        // MaxID = 0xEF3C on 7.0.8.2
                        return "build/gumpartlegacymul/{0:00000000}.tga";
                    }
                case FileType.MapLegacyMul:
                    {
                        // MaxID = 0x71 on 7.0.8.2 for Fel/Tram
                        return string.Concat("build/map", typeIndex, "legacymul/{0:00000000}.dat");
                    }
                case FileType.SoundLegacyMul:
                    {
                        // MaxID = 0x1000 on 7.0.8.2
                        return "build/soundlegacymul/{0:00000000}.dat";
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
    }
}
