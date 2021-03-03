using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public sealed class AnimationEdit
    {
        private static FileIndex _fileIndex = new FileIndex("Anim.idx", "Anim.mul", 6);
        private static FileIndex _fileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", -1);
        private static FileIndex _fileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", -1);
        private static FileIndex _fileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", -1);
        private static FileIndex _fileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", -1);

        private static AnimIdx[] _animCache;
        private static AnimIdx[] _animCache2;
        private static AnimIdx[] _animCache3;
        private static AnimIdx[] _animCache4;
        private static AnimIdx[] _animCache5;

        static AnimationEdit()
        {
            InitializeCache();
        }

        private static void InitializeCache()
        {
            if (_fileIndex.IdxLength > 0)
            {
                _animCache = new AnimIdx[_fileIndex.IdxLength / 12];
            }

            if (_fileIndex2.IdxLength > 0)
            {
                _animCache2 = new AnimIdx[_fileIndex2.IdxLength / 12];
            }

            if (_fileIndex3.IdxLength > 0)
            {
                _animCache3 = new AnimIdx[_fileIndex3.IdxLength / 12];
            }

            if (_fileIndex4.IdxLength > 0)
            {
                _animCache4 = new AnimIdx[_fileIndex4.IdxLength / 12];
            }

            if (_fileIndex5.IdxLength > 0)
            {
                _animCache5 = new AnimIdx[_fileIndex5.IdxLength / 12];
            }
        }

        /// <summary>
        /// Rereads AnimX files
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex("Anim.idx", "Anim.mul", 6);
            _fileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", -1);
            _fileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", -1);
            _fileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", -1);
            _fileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", -1);

            InitializeCache();
        }

        private static void GetFileIndex(
                int body, int fileType, int action, int direction, out FileIndex fileIndex, out int index)
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

        private static AnimIdx[] GetCache(int fileType)
        {
            switch (fileType)
            {
                case 1:
                    return _animCache;
                case 2:
                    return _animCache2;
                case 3:
                    return _animCache3;
                case 4:
                    return _animCache4;
                case 5:
                    return _animCache5;
                default:
                    return _animCache;
            }
        }

        public static AnimIdx GetAnimation(int fileType, int body, int action, int dir)
        {
            AnimIdx[] cache = GetCache(fileType);

            GetFileIndex(body, fileType, action, dir, out FileIndex fileIndex, out int index);

            if (cache?[index] != null)
            {
                return cache[index];
            }

            return cache[index] = new AnimIdx(index, fileIndex);
        }

        public static bool IsActionDefined(int fileType, int body, int action)
        {
            AnimIdx[] cache = GetCache(fileType);

            GetFileIndex(body, fileType, action, 0, out FileIndex fileIndex, out int index);

            if (cache?[index] != null)
            {
                return cache[index].Frames?.Count > 0;
            }

            int animCount = Animations.GetAnimLength(body, fileType);
            if (animCount < action)
            {
                return false;
            }

            bool valid = fileIndex.Valid(index, out int length, out int _, out bool _);

            return valid && length >= 1;
        }

        public static void LoadFromVD(int fileType, int body, BinaryReader bin)
        {
            AnimIdx[] cache = GetCache(fileType);
            GetFileIndex(body, fileType, 0, 0, out FileIndex _, out int index);
            int animLength = Animations.GetAnimLength(body, fileType) * 5;
            var entries = new Entry3D[animLength];

            for (int i = 0; i < animLength; ++i)
            {
                entries[i].Lookup = bin.ReadInt32();
                entries[i].Length = bin.ReadInt32();
                entries[i].Extra = bin.ReadInt32();
            }

            foreach (Entry3D entry in entries)
            {
                if ((entry.Lookup > 0) && (entry.Lookup < bin.BaseStream.Length) && (entry.Length > 0))
                {
                    bin.BaseStream.Seek(entry.Lookup, SeekOrigin.Begin);
                    cache[index] = new AnimIdx(bin, entry.Extra);
                }
                ++index;
            }
        }

        public static void ExportToVD(int fileType, int body, string file)
        {
            AnimIdx[] cache = GetCache(fileType);
            GetFileIndex(body, fileType, 0, 0, out FileIndex fileIndex, out int index);
            using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                bin.Write((short)6);
                int animLength = Animations.GetAnimLength(body, fileType);
                int currType = animLength == 22 ? 0 : animLength == 13 ? 1 : 2;
                bin.Write((short)currType);
                long indexPos = bin.BaseStream.Position;
                long animPos = bin.BaseStream.Position + (12 * animLength * 5);
                for (int i = index; i < index + (animLength * 5); i++)
                {
                    AnimIdx anim;
                    if (cache != null)
                    {
                        anim = cache[i] != null ? cache[i] : cache[i] = new AnimIdx(i, fileIndex);
                    }
                    else
                    {
                        anim = cache[i] = new AnimIdx(i, fileIndex);
                    }

                    if (anim == null)
                    {
                        bin.BaseStream.Seek(indexPos, SeekOrigin.Begin);
                        bin.Write(-1);
                        bin.Write(-1);
                        bin.Write(-1);
                        indexPos = bin.BaseStream.Position;
                    }
                    else
                    {
                        anim.ExportToVD(bin, ref indexPos, ref animPos);
                    }
                }
            }
        }

        public static void Save(int fileType, string path)
        {
            string filename;
            AnimIdx[] cache;
            FileIndex fileIndex;
            switch (fileType)
            {
                default:
                case 1:
                    filename = "anim";
                    cache = _animCache;
                    fileIndex = _fileIndex;
                    break;
                case 2:
                    filename = "anim2";
                    cache = _animCache2;
                    fileIndex = _fileIndex2;
                    break;
                case 3:
                    filename = "anim3";
                    cache = _animCache3;
                    fileIndex = _fileIndex3;
                    break;
                case 4:
                    filename = "anim4";
                    cache = _animCache4;
                    fileIndex = _fileIndex4;
                    break;
                case 5:
                    filename = "anim5";
                    cache = _animCache5;
                    fileIndex = _fileIndex5;
                    break;
            }

            string idx = Path.Combine(path, filename + ".idx");
            string mul = Path.Combine(path, filename + ".mul");

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var binidx = new BinaryWriter(fsidx))
            using (var binmul = new BinaryWriter(fsmul))
            {
                for (int idxc = 0; idxc < cache.Length; ++idxc)
                {
                    AnimIdx anim;
                    if (cache != null)
                    {
                        anim = cache[idxc] != null ? cache[idxc] : cache[idxc] = new AnimIdx(idxc, fileIndex);
                    }
                    else
                    {
                        anim = cache[idxc] = new AnimIdx(idxc, fileIndex);
                    }

                    if (anim == null)
                    {
                        binidx.Write(-1);
                        binidx.Write(-1);
                        binidx.Write(-1);
                    }
                    else
                    {
                        anim.Save(binmul, binidx);
                    }
                }
            }
        }
    }

    public sealed class AnimIdx
    {
        private readonly int _idxExtra;

        public ushort[] Palette { get; private set; }
        public List<FrameEdit> Frames { get; private set; }

        public AnimIdx(int index, FileIndex fileIndex)
        {
            Palette = new ushort[0x100];
            Stream stream = fileIndex.Seek(index, out int length, out int extra, out bool _);
            if ((stream == null) || (length < 1))
            {
                return;
            }

            _idxExtra = extra;
            using (var bin = new BinaryReader(stream))
            {
                for (int i = 0; i < 0x100; ++i)
                {
                    Palette[i] = (ushort)(bin.ReadUInt16() ^ 0x8000);
                }

                var start = (int)bin.BaseStream.Position;
                int frameCount = bin.ReadInt32();

                var lookups = new int[frameCount];

                for (int i = 0; i < frameCount; ++i)
                {
                    lookups[i] = start + bin.ReadInt32();
                }

                Frames = new List<FrameEdit>();

                for (int i = 0; i < frameCount; ++i)
                {
                    stream.Seek(lookups[i], SeekOrigin.Begin);
                    Frames.Add(new FrameEdit(bin));
                }
            }
            stream.Close();
        }

        public AnimIdx(BinaryReader bin, int extra)
        {
            Palette = new ushort[0x100];
            _idxExtra = extra;

            for (int i = 0; i < 0x100; ++i)
            {
                Palette[i] = (ushort)(bin.ReadUInt16() ^ 0x8000);
            }

            var start = (int)bin.BaseStream.Position;
            int frameCount = bin.ReadInt32();

            var lookups = new int[frameCount];

            for (int i = 0; i < frameCount; ++i)
            {
                lookups[i] = start + bin.ReadInt32();
            }

            Frames = new List<FrameEdit>();

            for (int i = 0; i < frameCount; ++i)
            {
                bin.BaseStream.Seek(lookups[i], SeekOrigin.Begin);
                Frames.Add(new FrameEdit(bin));
            }
        }

        public unsafe Bitmap[] GetFrames()
        {
            if ((Frames == null) || (Frames.Count == 0))
            {
                return null;
            }

            var bits = new Bitmap[Frames.Count];
            for (int i = 0; i < bits.Length; ++i)
            {
                FrameEdit frame = Frames[i];
                int width = frame.width;
                int height = frame.height;
                if (height == 0 || width == 0)
                {
                    continue;
                }

                var bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                int xBase = frame.Center.X - 0x200;
                int yBase = frame.Center.Y + height - 0x200;

                line += xBase;
                line += yBase * delta;

                for (int j = 0; j < frame.RawData.Length; ++j)
                {
                    FrameEdit.Raw raw = frame.RawData[j];

                    ushort* cur = line + (((raw.offsetY) * delta) + ((raw.offsetX) & 0x3FF));
                    ushort* end = cur + (raw.run);

                    int ii = 0;
                    while (cur < end)
                    {
                        *cur++ = Palette[raw.data[ii++]];
                    }
                }

                bmp.UnlockBits(bd);
                bits[i] = bmp;
            }

            return bits;
        }

        public void AddFrame(Bitmap bit, int centerX = 0, int centerY = 0 )
        {
            if (Frames == null)
            {
                Frames = new List<FrameEdit>();
            }

            Frames.Add(new FrameEdit(bit, Palette, centerX, centerY));
        }

        public void ReplaceFrame(Bitmap bit, int index)
        {
            if ((Frames == null) || (Frames.Count == 0))
            {
                return;
            }

            if (index > Frames.Count)
            {
                return;
            }

            Frames[index] = new FrameEdit(bit, Palette, Frames[index].Center.X, Frames[index].Center.Y);
        }

        public void RemoveFrame(int index)
        {
            if (Frames == null)
            {
                return;
            }

            if (index > Frames.Count)
            {
                return;
            }

            Frames.RemoveAt(index);
        }

        public void ClearFrames()
        {
            Frames?.Clear();
        }

        public void ExportPalette(string filename, int type)
        {
            switch (type)
            {
                case 0:
                    using (var tex = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite)))
                    {
                        for (int i = 0; i < 0x100; ++i)
                        {
                            tex.WriteLine(Palette[i]);
                        }
                    }
                    break;
                case 1:
                    SavePaletteImage(filename, ImageFormat.Bmp);
                    break;
                case 2:
                    SavePaletteImage(filename, ImageFormat.Tiff);
                    break;
            }
        }

        private unsafe void SavePaletteImage(string filename, ImageFormat imageFormat)
        {
            using (var bmp = new Bitmap(0x100, 20, PixelFormat.Format16bppArgb1555))
            {
                BitmapData bd = bmp.LockBits(
                    new Rectangle(0, 0, 0x100, 20), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                var line = (ushort*) bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < bd.Height; ++y, line += delta)
                {
                    ushort* cur = line;
                    for (int i = 0; i < 0x100; ++i)
                    {
                        *cur++ = Palette[i];
                    }
                }

                bmp.UnlockBits(bd);
                using (var b = new Bitmap(bmp))
                {
                    b.Save(filename, imageFormat);
                }
            }
        }

        public void ReplacePalette(ushort[] palette)
        {
            Palette = palette;
        }

        public void Save(BinaryWriter bin, BinaryWriter idx)
        {
            if ((Frames == null) || (Frames.Count == 0))
            {
                idx.Write(-1);
                idx.Write(-1);
                idx.Write(-1);
                return;
            }
            long start = bin.BaseStream.Position;
            idx.Write((int)start);

            for (int i = 0; i < 0x100; ++i)
            {
                bin.Write((ushort)(Palette[i] ^ 0x8000));
            }

            long startPosition = bin.BaseStream.Position;
            bin.Write(Frames.Count);
            long seek = bin.BaseStream.Position;
            long curr = bin.BaseStream.Position + (4 * Frames.Count);
            foreach (FrameEdit frame in Frames)
            {
                bin.BaseStream.Seek(seek, SeekOrigin.Begin);
                bin.Write((int)(curr - startPosition));
                seek = bin.BaseStream.Position;
                bin.BaseStream.Seek(curr, SeekOrigin.Begin);
                frame.Save(bin);
                curr = bin.BaseStream.Position;
            }

            start = bin.BaseStream.Position - start;
            idx.Write((int)start);
            idx.Write(_idxExtra);
        }

        public void ExportToVD(BinaryWriter bin, ref long indexpos, ref long animpos)
        {
            bin.BaseStream.Seek(indexpos, SeekOrigin.Begin);
            if ((Frames == null) || (Frames.Count == 0))
            {
                bin.Write(-1);
                bin.Write(-1);
                bin.Write(-1);
                indexpos = bin.BaseStream.Position;
                return;
            }
            bin.Write((int)animpos);
            indexpos = bin.BaseStream.Position;
            bin.BaseStream.Seek(animpos, SeekOrigin.Begin);

            for (int i = 0; i < 0x100; ++i)
            {
                bin.Write((ushort)(Palette[i] ^ 0x8000));
            }

            long startPosition = (int)bin.BaseStream.Position;
            bin.Write(Frames.Count);
            long seek = (int)bin.BaseStream.Position;
            long curr = bin.BaseStream.Position + (4 * Frames.Count);
            foreach (FrameEdit frame in Frames)
            {
                bin.BaseStream.Seek(seek, SeekOrigin.Begin);
                bin.Write((int)(curr - startPosition));
                seek = bin.BaseStream.Position;
                bin.BaseStream.Seek(curr, SeekOrigin.Begin);
                frame.Save(bin);
                curr = bin.BaseStream.Position;
            }

            long length = bin.BaseStream.Position - animpos;
            animpos = bin.BaseStream.Position;
            bin.BaseStream.Seek(indexpos, SeekOrigin.Begin);
            bin.Write((int)length);
            bin.Write(_idxExtra);
            indexpos = bin.BaseStream.Position;
        }
    }

    public sealed class FrameEdit
    {
        private const int _doubleXor = (0x200 << 22) | (0x200 << 12);

        public struct Raw
        {
            public int run;
            public int offsetX;
            public int offsetY;
            public byte[] data;
        }

        public Raw[] RawData { get; }
        public Point Center { get; set; }

        public readonly int width;
        public readonly int height;

        public FrameEdit(BinaryReader bin)
        {
            int xCenter = bin.ReadInt16();
            int yCenter = bin.ReadInt16();

            width = bin.ReadUInt16();
            height = bin.ReadUInt16();
            if (height == 0 || width == 0)
            {
                return;
            }

            int header;

            var tmp = new List<Raw>();

            while ((header = bin.ReadInt32()) != 0x7FFF7FFF)
            {
                var raw = new Raw();
                header ^= _doubleXor;
                raw.run = (header & 0xFFF);
                raw.offsetY = ((header >> 12) & 0x3FF);
                raw.offsetX = ((header >> 22) & 0x3FF);

                int i = 0;
                raw.data = new byte[raw.run];

                while (i < raw.run)
                {
                    raw.data[i++] = bin.ReadByte();
                }

                tmp.Add(raw);
            }

            RawData = tmp.ToArray();
            Center = new Point(xCenter, yCenter);
        }

        public unsafe FrameEdit(Bitmap bit, ushort[] palette, int centerX, int centerY)
        {
            Center = new Point(centerX, centerY);
            width = bit.Width;
            height = bit.Height;

            BitmapData bd = bit.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            var tmp = new List<Raw>();

            for (int y = 0; y < bit.Height; ++y, line += delta)
            {
                ushort* cur = line;

                int i = 0;
                int x = 0;

                while (i < bit.Width)
                {
                    for (i = x; i <= bit.Width; ++i)
                    {
                        // first pixel set
                        if (i < bit.Width && cur[i] != 0)
                        {
                            break;
                        }
                    }

                    if (i >= bit.Width)
                    {
                        continue;
                    }

                    int j;
                    for (j = (i + 1); j < bit.Width; ++j)
                    {
                        // next non set pixel
                        if (cur[j] == 0)
                        {
                            break;
                        }
                    }

                    var raw = new Raw
                    {
                        run = j - i
                    };
                    raw.offsetX = j - raw.run - centerX;
                    raw.offsetX += 512;
                    raw.offsetY = y - centerY - bit.Height;
                    raw.offsetY += 512;

                    int r = 0;
                    raw.data = new byte[raw.run];
                    while (r < raw.run)
                    {
                        ushort col = cur[r + i];
                        raw.data[r++] = GetPaletteIndex(palette, col);
                    }
                    tmp.Add(raw);
                    x = j + 1;
                    i = x;
                }
            }

            RawData = tmp.ToArray();
            bit.UnlockBits(bd);
        }

        public void ChangeCenter(int x, int y)
        {
            for (int i = 0; i < RawData.Length; i++)
            {
                RawData[i].offsetX += Center.X;
                RawData[i].offsetX -= x;
                RawData[i].offsetY += Center.Y;
                RawData[i].offsetY -= y;
            }

            Center = new Point(x, y);
        }

        private static byte GetPaletteIndex(IReadOnlyList<ushort> palette, ushort col)
        {
            for (int i = 0; i < palette.Count; i++)
            {
                if (palette[i] == col)
                {
                    return (byte)i;
                }
            }

            return 0;
        }

        public void Save(BinaryWriter bin)
        {
            bin.Write((short)Center.X);
            bin.Write((short)Center.Y);
            bin.Write((ushort)width);
            bin.Write((ushort)height);

            if (RawData != null)
            {
                for (int j = 0; j < RawData.Length; j++)
                {
                    int newHeader = RawData[j].run | (RawData[j].offsetY << 12) | (RawData[j].offsetX << 22);
                    newHeader ^= _doubleXor;
                    bin.Write(newHeader);
                    foreach (byte b in RawData[j].data)
                    {
                        bin.Write(b);
                    }
                }
            }

            bin.Write(0x7FFF7FFF);
        }
    }
}