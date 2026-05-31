using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima.Caching;

namespace Ultima
{
    public static class Animations
    {
        public const int MaxAnimationValue = 2048; // bodyconv.def says it's maximum animation value so max bodyId?

        // Upper bound on the action index for UOP bodies. Mirrors the UOP loader's internal
        // scan range and is exposed so callers (e.g. the animation tree) can probe actions with
        // an early-exit loop instead of enumerating the full set.
        public const int MaxAnimActions = AnimationsUopLoader._maxAnimActions;

        public static readonly int PaletteCapacity = 0x100;

        // LRU decode cache shared by the MUL and UOP paths. Bitmaps it returns
        // are cache-owned and borrowed by callers — do NOT dispose them; clone
        // first if you need an owned copy (e.g. to feed AnimatedPictureBox).
        private static LruAnimationCache _cache = new LruAnimationCache(Files.CacheCapacityAnimations);

        internal static LruAnimationCache Cache => _cache;

        /// <summary>
        /// Override the LRU cap for the animation decode cache. Lower values
        /// bound the working set on memory-constrained machines at the cost of
        /// more re-decodes during long browsing sessions.
        /// </summary>
        public static void SetCacheCapacity(int capacity)
        {
            _cache.SetCapacity(capacity);
        }

        /// <summary>
        /// Packs the parameters that uniquely identify a decoded frame set into
        /// a single cache key. For the MUL path pass the post-Translate body,
        /// fileType and resolved hue; for the UOP path pass the raw body with
        /// <paramref name="isUop"/> set (fileType is irrelevant there).
        /// </summary>
        internal static long BuildAnimationKey(int body, int action, int direction, int fileType, bool firstFrame, int hue, bool isUop)
        {
            return ((long)(body & 0xFFFFF))
                 | ((long)(action & 0x7F) << 20)
                 | ((long)(direction & 0x7) << 27)
                 | ((long)(fileType & 0x7) << 30)
                 | ((firstFrame ? 1L : 0L) << 33)
                 | ((long)(hue & 0xFFFF) << 34)
                 | ((isUop ? 1L : 0L) << 50);
        }

        private static FileIndex _fileIndex = new FileIndex("Anim.idx", "Anim.mul", 0x40000, 6);
        private static FileIndex _fileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", 0x10000, -1);
        private static FileIndex _fileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", 0x20000, -1);
        private static FileIndex _fileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", 0x20000, -1);
        private static FileIndex _fileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", 0x20000, -1);
        private static FileIndex _fileIndex6 = new FileIndex("Anim6.idx", "Anim6.mul", 0x20000, -1);

        private static byte[] _streamBuffer;

        /// <summary>
        /// Rereads AnimX files and bodyconv, body.def
        /// </summary>
        public static void Reload()
        {
            _fileIndex?.Dispose();
            _fileIndex2?.Dispose();
            _fileIndex3?.Dispose();
            _fileIndex4?.Dispose();
            _fileIndex5?.Dispose();
            _fileIndex6?.Dispose();

            _cache?.Clear();

            _fileIndex = new FileIndex("Anim.idx", "Anim.mul", 0x40000, 6);
            _fileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", 0x10000, -1);
            _fileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", 0x20000, -1);
            _fileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", 0x20000, -1);
            _fileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", 0x20000, -1);
            _fileIndex6 = new FileIndex("Anim6.idx", "Anim6.mul", 0x20000, -1);

            BodyConverter.Initialize();
            BodyTable.Initialize();
            AnimationsUopLoader.Reload();

            // _table is derived from bodyconv.def + body.def and is built lazily/cached. The defs were
            // just reloaded above, so drop the cache to force a rebuild from the current data - otherwise a
            // stale table keeps applying the old body.def translation and ignores bodyconv.def mappings.
            _table = null;
        }

        /// <summary>
        ///     Returns animation frames
        /// </summary>
        /// <param name="body"></param>
        /// <param name="action"></param>
        /// <param name="direction"></param>
        /// <param name="hue"></param>
        /// <param name="preserveHue">
        ///     No Hue override <see cref="bodydev" />
        /// </param>
        /// <param name="firstFrame"></param>
        /// <returns></returns>
        public static AnimationFrame[] GetAnimation(int body, int action, int direction, ref int hue, bool preserveHue, bool firstFrame)
        {
            if (AnimationsUopLoader.IsUopBody(body))
            {
                return AnimationsUopLoader.GetAnimation(body, action, direction, ref hue, preserveHue, firstFrame);
            }

            if (preserveHue)
            {
                Translate(ref body);
            }
            else
            {
                Translate(ref body, ref hue);
            }

            int fileType = BodyConverter.Convert(ref body);

            // Key off the post-Translate inputs; the decode below mutates `hue`
            // into its resolved index, so reproduce that on a cache hit.
            int lookupHue = hue;
            long cacheKey = BuildAnimationKey(body, action, direction, fileType, firstFrame, lookupHue, isUop: false);
            if (_cache.TryGet(cacheKey, out AnimationFrame[] cachedFrames))
            {
                hue = (lookupHue & 0x3FFF) - 1;
                return cachedFrames;
            }

            GetFileIndex(body, action, direction, fileType, out FileIndex fileIndex, out int index);

            Stream stream = fileIndex.Seek(index, out int length, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            if (_streamBuffer == null || _streamBuffer.Length < length)
            {
                _streamBuffer = new byte[length];
            }

            _ = stream.Read(_streamBuffer, 0, length);

            var memoryStream = new MemoryStream(_streamBuffer, false);

            bool flip = direction > 4;
            AnimationFrame[] frames;
            using (var bin = new BinaryReader(memoryStream))
            {
                var palette = new ushort[PaletteCapacity];

                for (int i = 0; i < PaletteCapacity; ++i)
                {
                    palette[i] = (ushort)(bin.ReadUInt16() ^ 0x8000);
                }

                var start = (int)bin.BaseStream.Position;
                int frameCount = bin.ReadInt32();

                var lookups = new int[frameCount];

                for (int i = 0; i < frameCount; ++i)
                {
                    lookups[i] = start + bin.ReadInt32();
                }

                bool onlyHueGrayPixels = (hue & 0x8000) != 0;

                hue = (hue & 0x3FFF) - 1;

                Hue hueObject;

                if (hue >= 0 && hue < Hues.List.Length)
                {
                    hueObject = Hues.List[hue];
                }
                else
                {
                    hueObject = null;
                }

                if (firstFrame)
                {
                    frameCount = 1;
                }

                frames = new AnimationFrame[frameCount];

                for (int i = 0; i < frameCount; ++i)
                {
                    bin.BaseStream.Seek(lookups[i], SeekOrigin.Begin);
                    frames[i] = new AnimationFrame(palette, bin, flip);

                    if (hueObject != null && frames[i]?.Bitmap != null)
                    {
                        hueObject.ApplyTo(frames[i].Bitmap, onlyHueGrayPixels);
                    }
                }
            }

            memoryStream.Close();

            _cache.Set(cacheKey, frames);

            return frames;
        }

        public static AnimationFrame[] GetAnimation(int body, int action, int direction, int fileType)
        {
            GetFileIndex(body, action, direction, fileType, out FileIndex fileIndex, out int index);

            Stream stream = fileIndex.Seek(index, out int _, out int _, out bool _);
            if (stream == null)
            {
                return null;
            }

            bool flip = direction > 4;

            // leaveOpen: stream is owned by the shared FileIndex; disposing the
            // BinaryReader must not close it, or the next FileIndex.Seek pays a
            // full re-open.
            using (var bin = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                var palette = new ushort[PaletteCapacity];

                for (int i = 0; i < PaletteCapacity; ++i)
                {
                    palette[i] = (ushort)(bin.ReadUInt16() ^ 0x8000);
                }

                var start = (int)bin.BaseStream.Position;
                int frameCount = bin.ReadInt32();

                var lookups = new int[frameCount];

                for (int i = 0; i < frameCount; ++i)
                {
                    lookups[i] = start + bin.ReadInt32();
                }

                var frames = new AnimationFrame[frameCount];

                for (int i = 0; i < frameCount; ++i)
                {
                    bin.BaseStream.Seek(lookups[i], SeekOrigin.Begin);
                    frames[i] = new AnimationFrame(palette, bin, flip);
                }

                return frames;
            }
        }

        private static int[] _table;

        /// <summary>
        /// Translates body (body.def)
        /// </summary>
        /// <param name="body"></param>
        public static void Translate(ref int body)
        {
            if (_table == null)
            {
                LoadTable();
            }

            if (body <= 0 || body >= _table.Length)
            {
                body = 0;
                return;
            }

            body = _table[body] & 0x7FFF;
        }

        /// <summary>
        /// Translates body and hue (body.def)
        /// </summary>
        /// <param name="body"></param>
        /// <param name="hue"></param>
        public static void Translate(ref int body, ref int hue)
        {
            if (_table == null)
            {
                LoadTable();
            }

            if (body <= 0 || body >= _table.Length)
            {
                body = 0;
                return;
            }

            int table = _table[body];
            if ((table & (1 << 31)) == 0)
            {
                return;
            }

            body = table & 0x7FFF;

            int vhue = (hue & 0x3FFF) - 1;
            if (vhue < 0 || vhue >= Hues.List.Length)
            {
                hue = (table >> 15) & 0xFFFF;
            }
        }

        private static void LoadTable()
        {
            _table = new int[MaxAnimationValue + 1];

            for (int i = 0; i < _table.Length; ++i)
            {
                var bodyTableEntryExist = BodyTable.Entries.TryGetValue(i, out BodyTableEntry bodyTableEntry);
                if (!bodyTableEntryExist || BodyConverter.Contains(i))
                {
                    _table[i] = i;
                }
                else
                {
                    _table[i] = bodyTableEntry.OldId | (1 << 31) | ((bodyTableEntry.NewHue & 0xFFFF) << 15);
                }
            }
        }

        /// <summary>
        /// Is Body with action and direction defined
        /// </summary>
        /// <param name="body"></param>
        /// <param name="action"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsActionDefined(int body, int action, int direction)
        {
            if (AnimationsUopLoader.IsUopBody(body))
            {
                return AnimationsUopLoader.IsActionDefined(body, action);
            }

            Translate(ref body);
            int fileType = BodyConverter.Convert(ref body);

            // Guard against actions past the body's physical block (see
            // GetActionCapacity) so we never probe another body's idx records.
            if (action < 0 || action >= GetActionCapacity(body, fileType))
            {
                return false;
            }

            GetFileIndex(body, action, direction, fileType, out FileIndex fileIndex, out int index);

            bool valid = fileIndex.Valid(index, out int length, out int _, out bool _);

            return valid && (length >= 1);
        }

        public static bool IsUopBody(int body) => AnimationsUopLoader.IsUopBody(body);

        public static int GetUopAnimationType(int body) => AnimationsUopLoader.GetAnimationType(body);

        public static System.Collections.Generic.List<int> GetUopDefinedActions(int body) =>
            AnimationsUopLoader.GetDefinedActions(body);

        public static System.Collections.Generic.IEnumerable<int> GetAllUopBodies() =>
            AnimationsUopLoader.GetAllUopBodyIds();

        public static System.Collections.Generic.IEnumerable<int> GetAllMobTypeBodies() =>
            AnimationsUopLoader.GetAllMobTypeBodyIds();

        /// <summary>
        /// Is Animation in given anim file defined
        /// </summary>
        /// <param name="body"></param>
        /// <param name="action"></param>
        /// <param name="dir"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static bool IsAnimDefined(int body, int action, int dir, int fileType)
        {
            if (action < 0 || action >= GetActionCapacity(body, fileType))
            {
                return false;
            }

            GetFileIndex(body, action, dir, fileType, out FileIndex fileIndex, out int index);

            Stream stream = fileIndex.Seek(index, out int length, out int _, out bool _);

            bool def = !((stream == null) || (length == 0));

            return def;
        }

        /// <summary>
        /// Returns Animation count in given anim file
        /// </summary>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static int GetAnimCount(int fileType)
        {
            switch (fileType)
            {
                case 1:
                default:
                    return 400 + ((int)(_fileIndex.IdxLength - (35000 * 12)) / (12 * 175));
                case 2:
                    return 200 + ((int)(_fileIndex2.IdxLength - (22000 * 12)) / (12 * 65));
                case 3:
                    return 400 + ((int)(_fileIndex3.IdxLength - (35000 * 12)) / (12 * 175));
                case 4:
                    return 400 + ((int)(_fileIndex4.IdxLength - (35000 * 12)) / (12 * 175));
                case 5:
                    return 400 + ((int)(_fileIndex5.IdxLength - (35000 * 12)) / (12 * 175));
                case 6:
                    return 400 + ((int)(_fileIndex6.IdxLength - (35000 * 12)) / (12 * 175));
            }
        }

        /// <summary>
        /// Action count of given Body in given anim file.
        /// When <c>mobtypes.txt</c> is loaded, the count is taken from the
        /// body's mobtype category; otherwise falls back to the historical
        /// body-id range heuristic.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static int GetAnimLength(int body, int fileType)
        {
            // The physical idx block reserved for a body is fixed by the id-range
            // stride used in GetFileIndex. Never report more actions than that
            // block holds: callers iterate this count and read idx records at
            // index + action*5, so a count larger than the block (e.g. a body
            // classed HUMAN/35 in mobtypes.txt but sitting in a 110-record/
            // 22-action id range) would walk into the next body's records.
            int capacity = GetActionCapacity(body, fileType);

            if (MobTypes.IsLoaded)
            {
                return System.Math.Min(MobTypes.GetActionCount(GetBodyMobType(body, fileType)), capacity);
            }

            return System.Math.Min(GetAnimLengthLegacy(body, fileType), capacity);
        }

        /// <summary>
        /// Maximum number of action slots physically reserved for <paramref name="body"/>
        /// in the given anim file. This is the idx stride (records per body) divided
        /// by the 5 stored directions and mirrors the id-range branches in
        /// <see cref="GetFileIndex"/> exactly. It is the hard upper bound for any
        /// action index, independent of the body's mobtype category, and exists so
        /// action enumeration can never cross a body boundary.
        /// </summary>
        public static int GetActionCapacity(int body, int fileType)
        {
            switch (fileType)
            {
                case 2:
                    return body < 200 ? 22 : 13;
                case 3:
                    if (body < 300)
                    {
                        return 13;
                    }

                    return body < 400 ? 22 : 35;
                case 5:
                    if ((body < 200) && (body != 34))
                    {
                        return 22;
                    }

                    return body < 400 ? 13 : 35;
                case 1:
                case 4:
                case 6:
                default:
                    if (body < 200)
                    {
                        return 22;
                    }

                    return body < 400 ? 13 : 35;
            }
        }

        /// <summary>
        /// Returns the mobtype category for a body in the given file. When
        /// <c>mobtypes.txt</c> is loaded, the server body id is recovered via
        /// <see cref="BodyConverter.GetTrueBody"/> for anim2..anim6 reverse
        /// lookup; falls back to the legacy id-range heuristic if either the
        /// reverse-mapping or the mobtypes lookup misses.
        /// </summary>
        public static MobType GetBodyMobType(int body, int fileType)
        {
            if (MobTypes.IsLoaded)
            {
                // For anim.mul (fileType=1) in-file id == server id.
                // For anim2..6 reverse-lookup bodyconv.def to find the server id.
                int serverBody = fileType == 1 ? body : BodyConverter.GetTrueBody(fileType, body);
                if (serverBody >= 0 && MobTypes.TryGet(serverBody, out MobType mt, out _))
                {
                    return mt;
                }
            }

            return LegacyRangeToMobType(body, fileType);
        }

        private static MobType LegacyRangeToMobType(int body, int fileType)
        {
            switch (fileType)
            {
                case 2:
                    return body < 200 ? MobType.Monster : MobType.Animal;
                case 3:
                    if (body < 300)
                    {
                        return MobType.Animal;
                    }
                    return body < 400 ? MobType.Monster : MobType.Human;
                case 1:
                case 4:
                case 5:
                case 6:
                default:
                    if (body < 200)
                    {
                        return MobType.Monster;
                    }
                    return body < 400 ? MobType.Animal : MobType.Human;
            }
        }

        private static int GetAnimLengthLegacy(int body, int fileType)
        {
            return MobTypes.GetActionCount(LegacyRangeToMobType(body, fileType));
        }

        /// <summary>
        /// Gets files index index based on fileType, body, action and direction
        /// </summary>
        /// <param name="body"></param>
        /// <param name="action"></param>
        /// <param name="direction"></param>
        /// <param name="fileType">animX</param>
        /// <param name="fileIndex"></param>
        /// <param name="index"></param>
        private static void GetFileIndex(int body, int action, int direction, int fileType, out FileIndex fileIndex, out int index)
        {
            switch (fileType)
            {
                case 1:
                default:
                    fileIndex = _fileIndex;
                    if (body < 200)
                    {
                        index = body * 110;
                    }
                    else if (body < 400)
                    {
                        index = 22000 + ((body - 200) * 65);
                    }
                    else
                    {
                        index = 35000 + ((body - 400) * 175);
                    }

                    break;
                case 2:
                    fileIndex = _fileIndex2;
                    if (body < 200)
                    {
                        index = body * 110;
                    }
                    else
                    {
                        index = 22000 + ((body - 200) * 65);
                    }

                    break;
                case 3:
                    fileIndex = _fileIndex3;
                    if (body < 300)
                    {
                        index = body * 65;
                    }
                    else if (body < 400)
                    {
                        index = 33000 + ((body - 300) * 110);
                    }
                    else
                    {
                        index = 35000 + ((body - 400) * 175);
                    }

                    break;
                case 4:
                    fileIndex = _fileIndex4;
                    if (body < 200)
                    {
                        index = body * 110;
                    }
                    else if (body < 400)
                    {
                        index = 22000 + ((body - 200) * 65);
                    }
                    else
                    {
                        index = 35000 + ((body - 400) * 175);
                    }

                    break;
                case 5:
                    fileIndex = _fileIndex5;
                    if ((body < 200) && (body != 34)) // looks strange, though it works.
                    {
                        index = body * 110;
                    }
                    else if (body < 400)
                    {
                        index = 22000 + ((body - 200) * 65);
                    }
                    else
                    {
                        index = 35000 + ((body - 400) * 175);
                    }

                    break;
                case 6:
                    fileIndex = _fileIndex6;
                    if (body < 200)
                    {
                        index = body * 110;
                    }
                    else if (body < 400)
                    {
                        index = 22000 + ((body - 200) * 65);
                    }
                    else
                    {
                        index = 35000 + ((body - 400) * 175);
                    }

                    break;
            }

            index += action * 5;

            if (direction <= 4)
            {
                index += direction;
            }
            else
            {
                index += direction - ((direction - 4) * 2);
            }
        }

        /// <summary>
        /// Returns Filename body is in
        /// </summary>
        /// <param name="body"></param>
        /// <returns>anim{0}.mul</returns>
        public static string GetFileName(int body)
        {
            if (AnimationsUopLoader.IsUopBody(body))
            {
                return AnimationsUopLoader.GetUopFileName(body);
            }

            Translate(ref body);
            int fileType = BodyConverter.Convert(ref body);

            return fileType == 1 ? "anim.mul" : $"anim{fileType}.mul"; // covers anim2–anim6
        }
    }

    public sealed class AnimationFrame
    {
        public Point Center { get; set; }
        public Bitmap Bitmap { get; set; }

        private const int _doubleXor = (0x200 << 22) | (0x200 << 12);

        public static readonly AnimationFrame Empty = new AnimationFrame();
        //public static readonly AnimationFrame[] EmptyFrames = new AnimationFrame[1] { Empty };

        private AnimationFrame()
        {
            Bitmap = new Bitmap(1, 1);
        }

        public unsafe AnimationFrame(ushort[] palette, BinaryReader bin, bool flip)
        {
            int xCenter = bin.ReadInt16();
            int yCenter = bin.ReadInt16();

            int width = bin.ReadUInt16();
            int height = bin.ReadUInt16();
            if (height == 0 || width == 0)
            {
                return;
            }

            var bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int header;

            int xBase = xCenter - 0x200;
            int yBase = (yCenter + height) - 0x200;

            if (!flip)
            {
                line += xBase;
                line += yBase * delta;

                while ((header = bin.ReadInt32()) != 0x7FFF7FFF)
                {
                    header ^= _doubleXor;

                    ushort* cur = line + ((((header >> 12) & 0x3FF) * delta) + ((header >> 22) & 0x3FF));
                    ushort* end = cur + (header & 0xFFF);
                    while (cur < end)
                    {
                        *cur++ = palette[bin.ReadByte()];
                    }
                }
            }
            else
            {
                line -= xBase - width + 1;
                line += yBase * delta;

                while ((header = bin.ReadInt32()) != 0x7FFF7FFF)
                {
                    header ^= _doubleXor;

                    ushort* cur = line + ((((header >> 12) & 0x3FF) * delta) - ((header >> 22) & 0x3FF));
                    ushort* end = cur - (header & 0xFFF);

                    while (cur > end)
                    {
                        *cur-- = palette[bin.ReadByte()];
                    }
                }

                xCenter = width - xCenter;
            }

            bmp.UnlockBits(bd);

            Center = new Point(xCenter, yCenter);
            Bitmap = bmp;
        }
    }
}