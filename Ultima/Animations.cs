using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public static class Animations
    {
        public const int _maxAnimationValue = 2048; // bodyconv.def says it's maximum animation value so max bodyId?

        private static FileIndex _fileIndex = new FileIndex("Anim.idx", "Anim.mul", 0x40000, 6);
        private static FileIndex _fileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", 0x10000, -1);
        private static FileIndex _fileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", 0x20000, -1);
        private static FileIndex _fileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", 0x20000, -1);
        private static FileIndex _fileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", 0x20000, -1);

        private static byte[] _streamBuffer;

        /// <summary>
        /// Rereads AnimX files and bodyconv, body.def
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex("Anim.idx", "Anim.mul", 0x40000, 6);
            _fileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", 0x10000, -1);
            _fileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", 0x20000, -1);
            _fileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", 0x20000, -1);
            _fileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", 0x20000, -1);

            BodyConverter.Initialize();
            BodyTable.Initialize();
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
            if (preserveHue)
            {
                Translate(ref body);
            }
            else
            {
                Translate(ref body, ref hue);
            }

            int fileType = BodyConverter.Convert(ref body);

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
                var palette = new ushort[0x100];

                for (int i = 0; i < 0x100; ++i)
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

            using (var bin = new BinaryReader(stream))
            {
                var palette = new ushort[0x100];

                for (int i = 0; i < 0x100; ++i)
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
            // TODO: check why it was fixed at max 1697. Probably old code for anim.mul?
            //int count = 400 + ((_fileIndex.Index.Length - 35000) / 175);

            _table = new int[_maxAnimationValue + 1];

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
            Translate(ref body);
            int fileType = BodyConverter.Convert(ref body);

            GetFileIndex(body, action, direction, fileType, out FileIndex fileIndex, out int index);

            bool valid = fileIndex.Valid(index, out int length, out int _, out bool _);

            return valid && (length >= 1);
        }

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
            GetFileIndex(body, action, dir, fileType, out FileIndex fileIndex, out int index);

            Stream stream = fileIndex.Seek(index, out int length, out int _, out bool _);

            bool def = !((stream == null) || (length == 0));

            stream?.Close();

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
            }
        }

        /// <summary>
        /// Action count of given Body in given anim file
        /// </summary>
        /// <param name="body"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static int GetAnimLength(int body, int fileType)
        {
            int length;
            switch (fileType)
            {
                case 1:
                default:
                    if (body < 200)
                    {
                        length = 22; // high
                    }
                    else if (body < 400)
                    {
                        length = 13; // low
                    }
                    else
                    {
                        length = 35; // people
                    }

                    break;
                case 2:
                    if (body < 200)
                    {
                        length = 22; // high
                    }
                    else
                    {
                        length = 13; // low
                    }

                    break;
                case 3:
                    if (body < 300)
                    {
                        length = 13;
                    }
                    else if (body < 400)
                    {
                        length = 22;
                    }
                    else
                    {
                        length = 35;
                    }

                    break;
                case 4:
                case 5:
                    if (body < 200)
                    {
                        length = 22;
                    }
                    else if (body < 400)
                    {
                        length = 13;
                    }
                    else
                    {
                        length = 35;
                    }

                    break;
            }
            return length;
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
            Translate(ref body);
            int fileType = BodyConverter.Convert(ref body);

            return fileType == 1 ? "anim.mul" : $"anim{fileType}.mul";
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