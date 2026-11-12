using System;
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
        private static FileIndex _fileIndex6 = new FileIndex("Anim6.idx", "Anim6.mul", -1);

        private static AnimIdx[] _animCache;
        private static AnimIdx[] _animCache2;
        private static AnimIdx[] _animCache3;
        private static AnimIdx[] _animCache4;
        private static AnimIdx[] _animCache5;
        private static AnimIdx[] _animCache6;

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

            if (_fileIndex6.IdxLength > 0)
            {
                _animCache6 = new AnimIdx[_fileIndex6.IdxLength / 12];
            }
        }

        /// <summary>
        /// Rereads AnimX files
        /// </summary>
        public static void Reload()
        {
            _fileIndex?.Dispose();
            _fileIndex2?.Dispose();
            _fileIndex3?.Dispose();
            _fileIndex4?.Dispose();
            _fileIndex5?.Dispose();
            _fileIndex6?.Dispose();

            _fileIndex = new FileIndex("Anim.idx", "Anim.mul", 6);
            _fileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", -1);
            _fileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", -1);
            _fileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", -1);
            _fileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", -1);
            _fileIndex6 = new FileIndex("Anim6.idx", "Anim6.mul", -1);

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
                case 6:
                    return _animCache6;
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
            // Reject actions beyond the body's physical idx block before computing
            // the index; otherwise index = base + action*5 crosses into the next
            // body's records. Replaces a prior off-by-one GetAnimLength check
            // (animCount < action) that both missed the boundary and used the
            // now-clamped category count.
            if (action < 0 || action >= Animations.GetActionCapacity(body, fileType))
            {
                return false;
            }

            AnimIdx[] cache = GetCache(fileType);

            GetFileIndex(body, fileType, action, 0, out FileIndex fileIndex, out int index);

            if (cache?[index] != null)
            {
                return cache[index].Frames?.Count > 0;
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

            for (int i = 0; i < entries.Length; ++i, ++index)
            {
                Entry3D entry = entries[i];
                if ((entry.Lookup > 0) && (entry.Lookup < bin.BaseStream.Length) && (entry.Length > 0))
                {
                    bin.BaseStream.Seek(entry.Lookup, SeekOrigin.Begin);
                    cache[index] = new AnimIdx(bin, entry.Extra);
                }
                else
                {
                    // Empty in the .vd: drop what the body had there, as LoadActionFromVD does.
                    GetAnimation(fileType, body, i / 5, i % 5).ClearFrames();
                }
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

        /// <summary>
        /// .vd header type of a file that holds one action (5 directions). Body files use 0, 1 and 2.
        /// </summary>
        public const short ActionVdType = 3;

        /// <summary>
        /// True when <paramref name="action"/> of <paramref name="body"/> has idx records in the given anim file.
        /// </summary>
        public static bool IsActionInRange(int fileType, int body, int action)
        {
            if (body < 0 || action < 0 || action >= Animations.GetActionCapacity(body, fileType))
            {
                return false;
            }

            AnimIdx[] cache = GetCache(fileType);
            if (cache == null)
            {
                return false;
            }

            GetFileIndex(body, fileType, action, 4, out FileIndex _, out int index);

            return index < cache.Length;
        }

        /// <summary>
        /// Writes one action as a .vd file: the body .vd layout with 5 index entries, one per direction.
        /// </summary>
        public static void ExportActionToVD(int fileType, int body, int action, string file)
        {
            using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var bin = new BinaryWriter(fs))
            {
                bin.Write((short)6);
                bin.Write(ActionVdType);
                long indexPos = bin.BaseStream.Position;
                long animPos = bin.BaseStream.Position + (12 * 5);
                for (int dir = 0; dir < 5; ++dir)
                {
                    GetAnimation(fileType, body, action, dir).ExportToVD(bin, ref indexPos, ref animPos);
                }
            }
        }

        /// <summary>
        /// Reads a single-action .vd, positioned after its 4-byte header, into the given action.
        /// Directions the file leaves empty are cleared. Nothing changes if the file can't be read.
        /// </summary>
        public static void LoadActionFromVD(int fileType, int body, int action, BinaryReader bin)
        {
            var entries = new Entry3D[5];
            for (int dir = 0; dir < 5; ++dir)
            {
                entries[dir].Lookup = bin.ReadInt32();
                entries[dir].Length = bin.ReadInt32();
                entries[dir].Extra = bin.ReadInt32();
            }

            var loaded = new AnimIdx[5];
            for (int dir = 0; dir < 5; ++dir)
            {
                Entry3D entry = entries[dir];
                if ((entry.Lookup > 0) && (entry.Lookup < bin.BaseStream.Length) && (entry.Length > 0))
                {
                    bin.BaseStream.Seek(entry.Lookup, SeekOrigin.Begin);
                    loaded[dir] = new AnimIdx(bin, entry.Extra);
                }
            }

            for (int dir = 0; dir < 5; ++dir)
            {
                AnimIdx target = GetAnimation(fileType, body, action, dir);
                if (loaded[dir] != null)
                {
                    target.CopyFrom(loaded[dir]);
                }
                else
                {
                    target.ClearFrames();
                }
            }
        }

        /// <summary>
        /// Copies the frames and palettes of all 5 directions of an action to another action, unchanged.
        /// </summary>
        public static void CopyAction(int fileType, int sourceBody, int sourceAction, int targetBody, int targetAction)
        {
            if (sourceBody == targetBody && sourceAction == targetAction)
            {
                return;
            }

            for (int dir = 0; dir < 5; ++dir)
            {
                AnimIdx source = GetAnimation(fileType, sourceBody, sourceAction, dir);
                GetAnimation(fileType, targetBody, targetAction, dir).CopyFrom(source);
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
                case 6:
                    filename = "anim6";
                    cache = _animCache6;
                    fileIndex = _fileIndex6;
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
        public readonly int PaletteCapacity = 0x100;

        private int _idxExtra;

        public ushort[] Palette { get; private set; }
        public List<FrameEdit> Frames { get; private set; }

        public AnimIdx(int index, FileIndex fileIndex)
        {
            Palette = new ushort[PaletteCapacity];

            Stream stream = fileIndex.Seek(index, out int length, out int extra, out bool _);
            if ((stream == null) || (length < 1))
            {
                return;
            }

            _idxExtra = extra;

            // leaveOpen: stream is owned by the shared FileIndex; disposing the
            // BinaryReader must not close it, or the next FileIndex.Seek pays a
            // full re-open.
            using (var bin = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                for (int i = 0; i < PaletteCapacity; ++i)
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
        }

        public AnimIdx(BinaryReader bin, int extra)
        {
            _idxExtra = extra;

            Palette = new ushort[PaletteCapacity];
            for (int i = 0; i < PaletteCapacity; ++i)
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
                int width = frame.Width;
                int height = frame.Height;
                if (height == 0 || width == 0)
                {
                    continue;
                }

                var bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                var pixels = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                int xBase = frame.Center.X - 0x200;
                int yBase = frame.Center.Y + height - 0x200;

                // Runs are clipped to the frame, so a bad header cannot write outside the bitmap.
                for (int j = 0; j < frame.RawData.Length; ++j)
                {
                    FrameEdit.Raw raw = frame.RawData[j];

                    int y = yBase + raw.offsetY;
                    if (y < 0 || y >= height)
                    {
                        continue;
                    }

                    ushort* row = pixels + (y * delta);
                    int x = xBase + (raw.offsetX & 0x3FF);
                    for (int k = 0; k < raw.run; ++k, ++x)
                    {
                        if (x >= 0 && x < width)
                        {
                            row[x] = Palette[raw.data[k]];
                        }
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

            FitPalette(new[] { bit });
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

            FitPalette(new[] { bit }, index);
            Frames[index] = new FrameEdit(bit, Palette, Frames[index].Center.X, Frames[index].Center.Y);
        }

        /// <summary>
        /// Makes the palette hold the colours of <paramref name="bitmaps"/> next to those of the current frames,
        /// so they can be added without losing colours. New colours go into entries no frame uses; past 256
        /// colours the palette is reduced over all frames and the current frames are remapped to it.
        /// Fitting all frames of an import at once gives a better reduction than adding them one by one.
        /// </summary>
        /// <param name="bitmaps">Frames about to be added or replaced.</param>
        /// <param name="replacedFrame">Index of the frame being replaced, whose colours no longer count.</param>
        public void FitPalette(IEnumerable<Bitmap> bitmaps, int replacedFrame = -1)
        {
            Dictionary<ushort, int> newColors = CountColors(bitmaps);
            if (newColors.Count == 0)
            {
                return;
            }

            var usedCounts = new int[PaletteCapacity];
            if (Frames != null)
            {
                for (int f = 0; f < Frames.Count; ++f)
                {
                    if (f == replacedFrame || Frames[f].RawData == null)
                    {
                        continue;
                    }

                    foreach (FrameEdit.Raw raw in Frames[f].RawData)
                    {
                        foreach (byte index in raw.data)
                        {
                            usedCounts[index]++;
                        }
                    }
                }
            }

            var reserved = new bool[PaletteCapacity];
            for (int i = 0; i < PaletteCapacity; ++i)
            {
                reserved[i] = usedCounts[i] > 0;
            }

            var missing = new List<ushort>();
            foreach (ushort color in newColors.Keys)
            {
                int index = Array.IndexOf(Palette, color);
                if (index >= 0)
                {
                    reserved[index] = true;
                }
                else
                {
                    missing.Add(color);
                }
            }

            if (missing.Count == 0)
            {
                return;
            }

            // Empty entries first, then entries no frame uses
            var free = new List<int>();
            for (int i = 0; i < PaletteCapacity; ++i)
            {
                if (!reserved[i] && Palette[i] == 0x8000)
                {
                    free.Add(i);
                }
            }

            for (int i = 0; i < PaletteCapacity; ++i)
            {
                if (!reserved[i] && Palette[i] != 0x8000)
                {
                    free.Add(i);
                }
            }

            if (missing.Count <= free.Count)
            {
                for (int i = 0; i < missing.Count; ++i)
                {
                    Palette[free[i]] = missing[i];
                }

                return;
            }

            var histogram = new Dictionary<ushort, int>(newColors);
            for (int i = 0; i < PaletteCapacity; ++i)
            {
                if (usedCounts[i] == 0)
                {
                    continue;
                }

                ushort color = FrameEdit.VisibleColor(Palette[i]);
                histogram.TryGetValue(color, out int count);
                histogram[color] = count + usedCounts[i];
            }

            ushort[] reduced = MedianCut(histogram, PaletteCapacity);

            var remap = new byte[PaletteCapacity];
            for (int i = 0; i < PaletteCapacity; ++i)
            {
                remap[i] = FrameEdit.NearestIndex(reduced, FrameEdit.VisibleColor(Palette[i]));
            }

            if (Frames != null)
            {
                foreach (FrameEdit frame in Frames)
                {
                    if (frame.RawData == null)
                    {
                        continue;
                    }

                    foreach (FrameEdit.Raw raw in frame.RawData)
                    {
                        for (int k = 0; k < raw.data.Length; ++k)
                        {
                            raw.data[k] = remap[raw.data[k]];
                        }
                    }
                }
            }

            Array.Copy(reduced, Palette, PaletteCapacity);
        }

        /// <summary>
        /// Replaces the palette with the colours of <paramref name="bitmaps"/>, reduced to 256 when there are more.
        /// Frames keep their palette indexes.
        /// </summary>
        public void SetPaletteFrom(IEnumerable<Bitmap> bitmaps)
        {
            Dictionary<ushort, int> colors = CountColors(bitmaps);
            if (colors.Count == 0)
            {
                return;
            }

            ushort[] palette;
            if (colors.Count <= PaletteCapacity)
            {
                palette = new ushort[PaletteCapacity];
                Array.Fill(palette, (ushort)0x8000);
                colors.Keys.CopyTo(palette, 0);
            }
            else
            {
                palette = MedianCut(colors, PaletteCapacity);
            }

            Array.Copy(palette, Palette, PaletteCapacity);
        }

        private static unsafe Dictionary<ushort, int> CountColors(IEnumerable<Bitmap> bitmaps)
        {
            var counts = new Dictionary<ushort, int>();
            foreach (Bitmap bit in bitmaps)
            {
                if (bit == null)
                {
                    continue;
                }

                BitmapData bd = bit.LockBits(new Rectangle(0, 0, bit.Width, bit.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                try
                {
                    for (int y = 0; y < bit.Height; ++y)
                    {
                        var line = (ushort*)((byte*)bd.Scan0 + (y * bd.Stride));
                        for (int x = 0; x < bit.Width; ++x)
                        {
                            if ((line[x] & 0x8000) == 0)
                            {
                                continue;
                            }

                            ushort color = FrameEdit.VisibleColor(line[x]);
                            counts.TryGetValue(color, out int count);
                            counts[color] = count + 1;
                        }
                    }
                }
                finally
                {
                    bit.UnlockBits(bd);
                }
            }

            return counts;
        }

        /// <summary>
        /// Reduces a colour histogram to at most <paramref name="maxColors"/> colours by weighted median cut.
        /// Unused entries of the result are 0x8000.
        /// </summary>
        private static ushort[] MedianCut(Dictionary<ushort, int> histogram, int maxColors)
        {
            var boxes = new List<List<KeyValuePair<ushort, int>>> { new List<KeyValuePair<ushort, int>>(histogram) };

            while (boxes.Count < maxColors)
            {
                int best = -1;
                int bestChannel = 0;
                long bestScore = 0;
                for (int i = 0; i < boxes.Count; ++i)
                {
                    if (boxes[i].Count < 2)
                    {
                        continue;
                    }

                    long pixels = 0;
                    var min = new[] { 31, 31, 31 };
                    var max = new[] { 0, 0, 0 };
                    foreach (KeyValuePair<ushort, int> entry in boxes[i])
                    {
                        pixels += entry.Value;
                        for (int c = 0; c < 3; ++c)
                        {
                            int value = Channel(entry.Key, c);
                            min[c] = Math.Min(min[c], value);
                            max[c] = Math.Max(max[c], value);
                        }
                    }

                    for (int c = 0; c < 3; ++c)
                    {
                        long score = (max[c] - min[c]) * pixels;
                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = i;
                            bestChannel = c;
                        }
                    }
                }

                if (best < 0)
                {
                    break;
                }

                List<KeyValuePair<ushort, int>> box = boxes[best];
                int channel = bestChannel;
                box.Sort((a, b) => Channel(a.Key, channel).CompareTo(Channel(b.Key, channel)));

                long total = 0;
                foreach (KeyValuePair<ushort, int> entry in box)
                {
                    total += entry.Value;
                }

                int split = box.Count - 1;
                long sum = 0;
                for (int i = 0; i < box.Count - 1; ++i)
                {
                    sum += box[i].Value;
                    if (sum * 2 >= total)
                    {
                        split = i + 1;
                        break;
                    }
                }

                boxes[best] = box.GetRange(0, split);
                boxes.Add(box.GetRange(split, box.Count - split));
            }

            var palette = new ushort[maxColors];
            Array.Fill(palette, (ushort)0x8000);
            for (int i = 0; i < boxes.Count; ++i)
            {
                long total = 0;
                long red = 0;
                long green = 0;
                long blue = 0;
                foreach (KeyValuePair<ushort, int> entry in boxes[i])
                {
                    total += entry.Value;
                    red += (long)Channel(entry.Key, 0) * entry.Value;
                    green += (long)Channel(entry.Key, 1) * entry.Value;
                    blue += (long)Channel(entry.Key, 2) * entry.Value;
                }

                int r = (int)((red + (total / 2)) / total);
                int g = (int)((green + (total / 2)) / total);
                int b = (int)((blue + (total / 2)) / total);
                palette[i] = FrameEdit.VisibleColor((ushort)(0x8000 | (r << 10) | (g << 5) | b));
            }

            return palette;
        }

        // 0 = red, 1 = green, 2 = blue
        private static int Channel(ushort color, int channel)
        {
            return (color >> (10 - (5 * channel))) & 0x1F;
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

        /// <summary>
        /// Makes this a copy of <paramref name="source"/>: palette, frames and idx extra. Nothing is shared.
        /// </summary>
        public void CopyFrom(AnimIdx source)
        {
            if (ReferenceEquals(source, this))
            {
                return;
            }

            Array.Copy(source.Palette, Palette, PaletteCapacity);
            Frames = source.Frames?.ConvertAll(frame => new FrameEdit(frame));
            _idxExtra = source._idxExtra;
        }

        public void ExportPalette(string filename, int type)
        {
            switch (type)
            {
                case 0:
                    using (var tex = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite)))
                    {
                        for (int i = 0; i < PaletteCapacity; ++i)
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
            using (var bmp = new Bitmap(PaletteCapacity, 20, PixelFormat.Format16bppArgb1555))
            {
                BitmapData bd = bmp.LockBits(
                    new Rectangle(0, 0, PaletteCapacity, 20), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                var line = (ushort*) bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < bd.Height; ++y, line += delta)
                {
                    ushort* cur = line;
                    for (int i = 0; i < PaletteCapacity; ++i)
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

            for (int i = 0; i < PaletteCapacity; ++i)
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

            for (int i = 0; i < PaletteCapacity; ++i)
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

        public readonly int Width;
        public readonly int Height;

        public FrameEdit(BinaryReader bin)
        {
            int xCenter = bin.ReadInt16();
            int yCenter = bin.ReadInt16();

            Width = bin.ReadUInt16();
            Height = bin.ReadUInt16();

            if (Height == 0 || Width == 0)
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

        public FrameEdit(FrameEdit source)
        {
            Center = source.Center;
            Width = source.Width;
            Height = source.Height;

            if (source.RawData == null)
            {
                return;
            }

            RawData = new Raw[source.RawData.Length];
            for (int i = 0; i < RawData.Length; ++i)
            {
                RawData[i] = source.RawData[i];
                RawData[i].data = (byte[])source.RawData[i].data.Clone();
            }
        }

        public unsafe FrameEdit(Bitmap bit, ushort[] palette, int centerX, int centerY)
        {
            Center = new Point(centerX, centerY);
            Width = bit.Width;
            Height = bit.Height;

            BitmapData bd = bit.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            var tmp = new List<Raw>();
            var indexCache = new Dictionary<ushort, byte>();

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
                        if (i < bit.Width && (cur[i] & 0x8000) != 0)
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
                        if ((cur[j] & 0x8000) == 0)
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
                        ushort col = VisibleColor(cur[r + i]);
                        if (!indexCache.TryGetValue(col, out byte index))
                        {
                            index = NearestIndex(palette, col);
                            indexCache[col] = index;
                        }

                        raw.data[r++] = index;
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

        /// <summary>
        /// Index of <paramref name="col"/> in the palette, or of the closest colour when it is missing.
        /// Entries of 0x8000 are skipped: the client draws them transparent.
        /// </summary>
        internal static byte NearestIndex(IReadOnlyList<ushort> palette, ushort col)
        {
            int red = (col >> 10) & 0x1F;
            int green = (col >> 5) & 0x1F;
            int blue = col & 0x1F;

            int best = 0;
            int bestDistance = int.MaxValue;
            for (int i = 0; i < palette.Count; i++)
            {
                ushort entry = palette[i];
                if (entry == col)
                {
                    return (byte)i;
                }

                if (entry == 0x8000)
                {
                    continue;
                }

                int dr = ((entry >> 10) & 0x1F) - red;
                int dg = ((entry >> 5) & 0x1F) - green;
                int db = (entry & 0x1F) - blue;
                int distance = (2 * dr * dr) + (4 * dg * dg) + (3 * db * db);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return (byte)best;
        }

        /// <summary>
        /// 0x8000 (black) is stored as 0, which the client draws transparent; keep it visible as 0,0,8.
        /// </summary>
        internal static ushort VisibleColor(ushort color)
        {
            return color == 0x8000 ? (ushort)0x8001 : color;
        }

        public void Save(BinaryWriter bin)
        {
            bin.Write((short)Center.X);
            bin.Write((short)Center.Y);
            bin.Write((ushort)Width);
            bin.Write((ushort)Height);

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