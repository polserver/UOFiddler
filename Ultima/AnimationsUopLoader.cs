/***************************************************************************
 *
 * $Author: UOFiddler Contributors
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
using System.Linq;
using Ultima.Helpers;

namespace Ultima
{
    internal static class AnimationsUopLoader
    {
        private const int _maxAnimActions = 80;
        private const int _maxDirections = 5;

        private static FileStream[] _uopFiles = new FileStream[6];
        private static readonly Dictionary<ulong, UopEntry> _hashTable = new();
        private static readonly Dictionary<int, int[]> _sequenceReplacements = new();
        private static bool _isLoaded;

        private struct UopEntry
        {
            public int FileIndex;
            public long Position;
            public int CompressedSize;
            public int DecompressedSize;
            public short CompressionFlag;
        }

        static AnimationsUopLoader()
        {
            Initialize();
        }

        public static bool IsLoaded => _isLoaded;

        public static void Reload()
        {
            if (_uopFiles != null)
            {
                foreach (var f in _uopFiles)
                {
                    f?.Close();
                }
            }

            _uopFiles = new FileStream[6];
            _hashTable.Clear();
            _sequenceReplacements.Clear();
            _isLoaded = false;

            Initialize();
        }

        private static void Initialize()
        {
            LoadUopFiles();
            MobTypes.Reload();
            LoadAnimationSequence();
            _isLoaded = _uopFiles.Any(f => f != null);
        }

        private static void LoadUopFiles()
        {
            for (int i = 0; i < _uopFiles.Length; i++)
            {
                string path = Files.GetFilePath($"AnimationFrame{i + 1}.uop");
                if (path == null)
                {
                    continue;
                }

                try
                {
                    _uopFiles[i] = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    BuildHashTable(_uopFiles[i], i);
                }
                catch
                {
                    _uopFiles[i] = null;
                }
            }
        }

        private static void BuildHashTable(FileStream fs, int fileIdx)
        {
            using var reader = new BinaryReader(fs, System.Text.Encoding.Default, leaveOpen: true);

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            if (reader.ReadUInt32() != 0x50594D)
            {
                return;
            }

            reader.ReadUInt32(); // version
            reader.ReadUInt32(); // format_timestamp
            long nextBlock = reader.ReadInt64();
            reader.ReadUInt32(); // block_size
            reader.ReadInt32();  // count

            reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

            do
            {
                int filesCount = reader.ReadInt32();
                nextBlock = reader.ReadInt64();

                for (int i = 0; i < filesCount; i++)
                {
                    long offset = reader.ReadInt64();
                    int headerLength = reader.ReadInt32();
                    int compressedLength = reader.ReadInt32();
                    int decompressedLength = reader.ReadInt32();
                    ulong hash = reader.ReadUInt64();
                    reader.ReadUInt32(); // data_hash
                    short flag = reader.ReadInt16();

                    if (offset == 0)
                    {
                        continue;
                    }

                    int dataSize = flag == 1 ? compressedLength : decompressedLength;

                    _hashTable[hash] = new UopEntry
                    {
                        FileIndex = fileIdx,
                        Position = offset + headerLength,
                        CompressedSize = dataSize,
                        DecompressedSize = decompressedLength,
                        CompressionFlag = flag,
                    };
                }

                if (nextBlock == 0)
                {
                    break;
                }

                reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);
            }
            while (true);
        }

        private static void LoadAnimationSequence()
        {
            string path = Files.GetFilePath("AnimationSequence.uop");
            if (path == null)
            {
                return;
            }

            try
            {
                using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new BinaryReader(fileStream);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                if (reader.ReadUInt32() != 0x50594D)
                {
                    return;
                }

                reader.ReadUInt32(); // version
                reader.ReadUInt32(); // format_timestamp
                long nextBlock = reader.ReadInt64();
                reader.ReadUInt32(); // block_size
                reader.ReadInt32();  // count

                var seqEntries = new Dictionary<ulong, UopEntry>();

                reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);

                do
                {
                    int filesCount = reader.ReadInt32();
                    nextBlock = reader.ReadInt64();

                    for (int i = 0; i < filesCount; i++)
                    {
                        long offset = reader.ReadInt64();
                        int headerLength = reader.ReadInt32();
                        int compressedLength = reader.ReadInt32();
                        int decompressedLength = reader.ReadInt32();
                        ulong hash = reader.ReadUInt64();
                        reader.ReadUInt32(); // data_hash
                        short flag = reader.ReadInt16();

                        if (offset == 0)
                        {
                            continue;
                        }

                        int dataSize = flag == 1 ? compressedLength : decompressedLength;
                        seqEntries[hash] = new UopEntry
                        {
                            FileIndex = -1,
                            Position = offset + headerLength,
                            CompressedSize = dataSize,
                            DecompressedSize = decompressedLength,
                            CompressionFlag = flag,
                        };
                    }

                    if (nextBlock == 0)
                    {
                        break;
                    }

                    reader.BaseStream.Seek(nextBlock, SeekOrigin.Begin);
                }
                while (true);

                // Scan all plausible body IDs for sequence entries
                for (int animId = 0; animId < Animations.MaxAnimationValue; animId++)
                {
                    ulong hash = UopUtils.HashFileName($"build/animationsequence/{animId:D8}.bin");
                    if (!seqEntries.TryGetValue(hash, out var entry))
                    {
                        continue;
                    }

                    fileStream.Seek(entry.Position, SeekOrigin.Begin);
                    byte[] buffer = new byte[entry.CompressedSize];
                    _ = fileStream.Read(buffer, 0, buffer.Length);

                    if (entry.CompressionFlag >= 1)
                    {
                        var (ok, dec) = UopUtils.Decompress(buffer);
                        if (!ok)
                        {
                            continue;
                        }

                        buffer = dec;
                    }

                    ParseSequenceEntry(animId, buffer);
                }
            }
            catch
            {
                // AnimationSequence.uop is optional; parsing failures are non-fatal
            }
        }

        private static void ParseSequenceEntry(int animId, byte[] data)
        {
            if (data.Length < 56)
            {
                return;
            }

            using var memoryStream = new MemoryStream(data);
            using var binaryReader = new BinaryReader(memoryStream);

            binaryReader.ReadUInt32(); // animId stored in file
            binaryReader.BaseStream.Seek(48, SeekOrigin.Current); // skip 12 × u32

            int replaces = binaryReader.ReadInt32();

            var replacements = new int[_maxAnimActions];
            for (int i = 0; i < _maxAnimActions; i++)
            {
                replacements[i] = i;
            }

            if (replaces != 48 && replaces != 68)
            {
                for (int k = 0; k < replaces; k++)
                {
                    if (binaryReader.BaseStream.Position + 72 > binaryReader.BaseStream.Length)
                    {
                        break;
                    }

                    int oldGroup = binaryReader.ReadInt32();
                    uint frameCount = binaryReader.ReadUInt32();
                    int newGroup = binaryReader.ReadInt32();

                    if (frameCount == 0 && oldGroup >= 0 && oldGroup < _maxAnimActions && newGroup >= 0)
                    {
                        replacements[oldGroup] = newGroup;
                    }

                    binaryReader.BaseStream.Seek(60, SeekOrigin.Current); // skip remaining per-replacement fields
                }
            }

            _sequenceReplacements[animId] = replacements;
        }

        public static bool IsUopBody(int body)
        {
            if (!_isLoaded)
            {
                return false;
            }

            // Preserved-as-was: 0x10000u UOP-marker bit. Originating from a
            // sentinel in mobtypes.txt flags; pre-existing behavior was to
            // gate UOP-body detection on this bit. Not in scope to revise.
            return (MobTypes.GetFlags(body) & 0x10000u) != 0;
        }

        public static int GetAnimationType(int body)
        {
            return MobTypes.TryGet(body, out MobType type, out _) ? (int)type : 0;
        }

        public static bool IsActionDefined(int body, int action)
        {
            if (!_isLoaded)
            {
                return false;
            }

            int resolved = GetResolvedAction(body, action);
            ulong hash = UopUtils.HashFileName($"build/animationlegacyframe/{body:D6}/{resolved:D2}.bin");
            return _hashTable.ContainsKey(hash);
        }

        public static List<int> GetDefinedActions(int body)
        {
            var result = new List<int>();
            if (!_isLoaded)
            {
                return result;
            }

            for (int i = 0; i < _maxAnimActions; i++)
            {
                if (IsActionDefined(body, i))
                {
                    result.Add(i);
                }
            }

            return result;
        }

        public static IEnumerable<int> GetAllUopBodyIds()
        {
            return MobTypes.GetDefinedBodies()
                .Where(id => (MobTypes.GetFlags(id) & 0x10000u) != 0)
                .OrderBy(id => id);
        }

        public static IEnumerable<int> GetAllMobTypeBodyIds()
        {
            return MobTypes.GetDefinedBodies().OrderBy(id => id);
        }

        public static string GetUopFileName(int body)
        {
            for (int action = 0; action < _maxAnimActions; action++)
            {
                ulong hash = UopUtils.HashFileName($"build/animationlegacyframe/{body:D6}/{action:D2}.bin");
                if (_hashTable.TryGetValue(hash, out var entry))
                {
                    return $"AnimationFrame{entry.FileIndex + 1}.uop";
                }
            }

            return "AnimationFrame*.uop";
        }

        private static int GetResolvedAction(int body, int action)
        {
            if (_sequenceReplacements.TryGetValue(body, out int[] repl) && action < repl.Length)
            {
                return repl[action];
            }

            return action;
        }

        public static AnimationFrame[] GetAnimation(int body, int action, int direction,
            ref int hue, bool preserveHue, bool firstFrame)
        {
            if (!_isLoaded)
            {
                return null;
            }

            // UOP path leaves `hue` unchanged for the caller, so the key uses
            // the raw input hue and the hit path does not touch `hue`.
            long cacheKey = Animations.BuildAnimationKey(body, action, direction, 0, firstFrame, hue, isUop: true);
            if (Animations.Cache.TryGet(cacheKey, out AnimationFrame[] cachedFrames))
            {
                return cachedFrames;
            }

            int resolved = GetResolvedAction(body, action);
            ulong hash = UopUtils.HashFileName($"build/animationlegacyframe/{body:D6}/{resolved:D2}.bin");

            if (!_hashTable.TryGetValue(hash, out var entry))
            {
                return null;
            }

            bool flip = direction > 4;
            int readDir = direction <= 4 ? direction : direction - ((direction - 4) * 2);

            byte[] data = ReadEntryData(entry);
            if (data == null)
            {
                return null;
            }

            AnimationFrame[] frames = ParseUopFrames(data, readDir, flip);
            if (frames == null || frames.Length == 0)
            {
                return null;
            }

            int hueIdx = (hue & 0x3FFF) - 1;
            bool onlyGray = (hue & 0x8000) != 0;
            if (hueIdx >= 0 && hueIdx < Hues.List.Length)
            {
                var hueObj = Hues.List[hueIdx];
                foreach (var frame in frames)
                {
                    if (frame?.Bitmap != null && frame != AnimationFrame.Empty)
                    {
                        hueObj.ApplyTo(frame.Bitmap, onlyGray);
                    }
                }
            }

            AnimationFrame[] result = firstFrame && frames.Length > 1
                ? new[] { frames[0] }
                : frames;

            Animations.Cache.Set(cacheKey, result);

            return result;
        }

        private static byte[] ReadEntryData(UopEntry entry)
        {
            int idx = entry.FileIndex;
            if (idx < 0 || idx >= _uopFiles.Length || _uopFiles[idx] == null)
            {
                return null;
            }

            var fileStream = _uopFiles[idx];
            byte[] buffer;

            lock (fileStream)
            {
                fileStream.Seek(entry.Position, SeekOrigin.Begin);
                buffer = new byte[entry.CompressedSize];
                _ = fileStream.Read(buffer, 0, buffer.Length);
            }

            if (entry.CompressionFlag >= 1)
            {
                var (ok, data) = UopUtils.Decompress(buffer);
                return ok ? data : null;
            }

            return buffer;
        }

        private static AnimationFrame[] ParseUopFrames(byte[] data, int direction, bool flip)
        {
            if (data.Length < 36)
            {
                return null;
            }

            using var memoryStream = new MemoryStream(data);
            using var reader = new BinaryReader(memoryStream);

            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int frameCount = reader.ReadInt32();
            uint dataStart = reader.ReadUInt32();

            if (frameCount <= 0 || dataStart >= data.Length)
            {
                return null;
            }

            reader.BaseStream.Seek(dataStart, SeekOrigin.Begin);

            var headers = new List<(long pos, ushort frameId, uint pixelOffset)>(frameCount);
            for (int i = 0; i < frameCount; i++)
            {
                long pos = reader.BaseStream.Position;
                reader.ReadUInt16(); // group
                ushort frameId = reader.ReadUInt16();
                reader.ReadInt64(); // unknown
                uint pixelOffset = reader.ReadUInt32();
                headers.Add((pos, frameId, pixelOffset));
            }

            // Gap-fill missing frame IDs with placeholder entries
            var filled = new List<(long pos, ushort frameId, uint pixelOffset)>(headers.Count);
            int lastId = 1;
            foreach (var (pos, frameId, pixelOffset) in headers)
            {
                while (frameId - lastId > 1)
                {
                    lastId++;
                    filled.Add((0L, (ushort)lastId, 0u));
                }

                filled.Add((pos, frameId, pixelOffset));
                lastId = frameId;
            }

            int realFrameCount = (int)Math.Round(filled.Count / (float)_maxDirections);
            if (realFrameCount <= 0)
            {
                return null;
            }

            var result = new List<AnimationFrame>();
            var palette = new ushort[Animations.PaletteCapacity];

            foreach (var (pos, frameId, pixelOffset) in filled)
            {
                int frameDir = (frameId - 1) / realFrameCount;
                if (frameDir < direction)
                {
                    continue;
                }

                if (frameDir > direction)
                {
                    break;
                }

                if (pos == 0)
                {
                    result.Add(AnimationFrame.Empty);
                    continue;
                }

                reader.BaseStream.Seek(pos + pixelOffset, SeekOrigin.Begin);

                // Palette is ARGB1555 stored with bit 15 cleared; set alpha bit on all non-zero entries.
                // Index 0 stays 0 (transparent); all others get bit 15 set to make them opaque.
                for (int i = 0; i < Animations.PaletteCapacity; i++)
                {
                    ushort raw = reader.ReadUInt16();
                    palette[i] = raw == 0 ? (ushort)0 : (ushort)(raw | 0x8000);
                }

                result.Add(new AnimationFrame(palette, reader, flip));
            }

            return result.Count > 0 ? result.ToArray() : null;
        }

    }
}
