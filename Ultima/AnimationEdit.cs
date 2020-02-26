using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Ultima
{
    public sealed class AnimationEdit
    {
        private static FileIndex _mFileIndex = new FileIndex("Anim.idx", "Anim.mul", 6);
        private static FileIndex _mFileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", -1);
        private static FileIndex _mFileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", -1);
        private static FileIndex _mFileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", -1);
        private static FileIndex _mFileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", -1);
  

        private static AnimIdx[] _animcache;
        private static readonly AnimIdx[] _animcache2;
        private static readonly AnimIdx[] _animcache3;
        private static readonly AnimIdx[] _animcache4;
        private static readonly AnimIdx[] _animcache5;
    

        static AnimationEdit()
        {
            if (_mFileIndex.IdxLength > 0)
                _animcache = new AnimIdx[_mFileIndex.IdxLength / 12];
            if (_mFileIndex2.IdxLength > 0)
                _animcache2 = new AnimIdx[_mFileIndex2.IdxLength / 12];
            if (_mFileIndex3.IdxLength > 0)
                _animcache3 = new AnimIdx[_mFileIndex3.IdxLength / 12];
            if (_mFileIndex4.IdxLength > 0)
                _animcache4 = new AnimIdx[_mFileIndex4.IdxLength / 12];
            if (_mFileIndex5.IdxLength > 0)
                _animcache5 = new AnimIdx[_mFileIndex5.IdxLength / 12];

        }
        /// <summary>
        /// Rereads AnimX files
        /// </summary>
        public static void Reload()
        {
            _mFileIndex = new FileIndex("Anim.idx", "Anim.mul", 6);
            _mFileIndex2 = new FileIndex("Anim2.idx", "Anim2.mul", -1);
            _mFileIndex3 = new FileIndex("Anim3.idx", "Anim3.mul", -1);
            _mFileIndex4 = new FileIndex("Anim4.idx", "Anim4.mul", -1);
            _mFileIndex5 = new FileIndex("Anim5.idx", "Anim5.mul", -1);
  
            if (_mFileIndex.IdxLength > 0)
                _animcache = new AnimIdx[_mFileIndex.IdxLength / 12];
            if (_mFileIndex2.IdxLength > 0)
                _animcache = new AnimIdx[_mFileIndex2.IdxLength / 12];
            if (_mFileIndex3.IdxLength > 0)
                _animcache = new AnimIdx[_mFileIndex3.IdxLength / 12];
            if (_mFileIndex4.IdxLength > 0)
                _animcache = new AnimIdx[_mFileIndex4.IdxLength / 12];
            if (_mFileIndex5.IdxLength > 0)
                _animcache = new AnimIdx[_mFileIndex5.IdxLength / 12];

        }

        private static void GetFileIndex(int body, int fileType, int action, int direction, out FileIndex fileIndex, out int index)
        {
            switch (fileType)
            {
                default:
                case 1:
                    fileIndex = _mFileIndex;
                    if (body < 200)
                        index = body * 110;
                    else if (body < 400)
                        index = 22000 + ((body - 200) * 65);
                    else
                        index = 35000 + ((body - 400) * 175);
                    break;
                case 2:
                    fileIndex = _mFileIndex2;
                    if (body < 200)
                        index = body * 110;
                    else
                        index = 22000 + ((body - 200) * 65);
                    break;
                case 3:
                    fileIndex = _mFileIndex3;
                    if (body < 300)
                        index = body * 65;
                    else if (body < 400)
                        index = 33000 + ((body - 300) * 110);
                    else
                        index = 35000 + ((body - 400) * 175);
                    break;
                case 4:
                    fileIndex = _mFileIndex4;
                    if (body < 200)
                        index = body * 110;
                    else if (body < 400)
                        index = 22000 + ((body - 200) * 65);
                    else
                        index = 35000 + ((body - 400) * 175);
                    break;
                case 5:
                    fileIndex = _mFileIndex5;
                    if ((body < 200) && (body != 34)) // looks strange, though it works.
                        index = body * 110;
                    else if (body < 400)
                        index = 22000 + ((body - 200) * 65);
                    else
                        index = 35000 + ((body - 400) * 175);
                    break;

            }

            index += action * 5;

            if (direction <= 4)
                index += direction;
            else
                index += direction - (direction - 4) * 2;
        }

        private static AnimIdx[] GetCache(int filetype)
        {
            switch (filetype)
            {
                case 1:
                    return _animcache;
                case 2:
                    return _animcache2;
                case 3:
                    return _animcache3;
                case 4:
                    return _animcache4;
                case 5:
                    return _animcache5;

                default:
                    return _animcache;
            }
        }

        public static AnimIdx GetAnimation(int filetype, int body, int action, int dir)
        {
            AnimIdx[] cache = GetCache(filetype);
            FileIndex fileIndex;
            int index;
            GetFileIndex(body, filetype, action, dir, out fileIndex, out index);

            if (cache?[index] != null)
                return cache[index];
            return cache[index] = new AnimIdx(index, fileIndex, filetype);
        }

        public static bool IsActionDefinied(int filetype, int body, int action)
        {
            AnimIdx[] cache = GetCache(filetype);
            FileIndex fileIndex;
            int index;
            GetFileIndex(body, filetype, action, 0, out fileIndex, out index);

            if (cache?[index] != null)
            {
                if ((cache[index].Frames != null) && (cache[index].Frames.Count > 0))
                    return true;
                else
                    return false;
            }

            int animCount = Animations.GetAnimLength(body, filetype);
            if (animCount < action)
                return false;

            int length, extra;
            bool patched;
            bool valid = fileIndex.Valid(index, out length, out extra, out patched);
            if ((!valid) || (length < 1))
                return false;
            return true;
        }

        public static void LoadFromVd(int filetype, int body, BinaryReader bin)
        {
            AnimIdx[] cache = GetCache(filetype);
            FileIndex fileIndex;
            int index;
            GetFileIndex(body, filetype, 0, 0, out fileIndex, out index);
            int animlength = Animations.GetAnimLength(body, filetype) * 5;
            Entry3D[] entries = new Entry3D[animlength];

            for (int i = 0; i < animlength; ++i)
            {
                entries[i].lookup = bin.ReadInt32();
                entries[i].length = bin.ReadInt32();
                entries[i].extra = bin.ReadInt32();
            }
            foreach (Entry3D entry in entries)
            {
                if ((entry.lookup > 0) && (entry.lookup < bin.BaseStream.Length) && (entry.length > 0))
                {
                    bin.BaseStream.Seek(entry.lookup, SeekOrigin.Begin);
                    cache[index] = new AnimIdx(bin, entry.extra);
                }
                ++index;
            }
        }

        public static void ExportToVd(int filetype, int body, string file)
        {
            AnimIdx[] cache = GetCache(filetype);
            FileIndex fileIndex;
            int index;
            GetFileIndex(body, filetype, 0, 0, out fileIndex, out index);
            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    bin.Write((short)6);
                    int animlength = Animations.GetAnimLength(body, filetype);
                    int currtype = animlength == 22 ? 0 : animlength == 13 ? 1 : 2;
                    bin.Write((short)currtype);
                    long indexpos = bin.BaseStream.Position;
                    long animpos = bin.BaseStream.Position + 12 * animlength * 5;
                    for (int i = index; i < index + animlength * 5; i++)
                    {
                        AnimIdx anim;
                        if (cache != null)
                        {
                            if (cache[i] != null)
                                anim = cache[i];
                            else
                                anim = cache[i] = new AnimIdx(i, fileIndex, filetype);
                        }
                        else
                            anim = cache[i] = new AnimIdx(i, fileIndex, filetype);

                        if (anim == null)
                        {
                            bin.BaseStream.Seek(indexpos, SeekOrigin.Begin);
                            bin.Write(-1);
                            bin.Write(-1);
                            bin.Write(-1);
                            indexpos = bin.BaseStream.Position;
                        }
                        else
                            anim.ExportToVd(bin, ref indexpos, ref animpos);
                    }
                }
            }
        }

        public static void Save(int filetype, string path)
        {
            string filename;
            AnimIdx[] cache;
            FileIndex fileindex;
            switch (filetype)
            {
                case 1: filename = "anim"; cache = _animcache; fileindex = _mFileIndex; break;
                case 2: filename = "anim2"; cache = _animcache2; fileindex = _mFileIndex2; break;
                case 3: filename = "anim3"; cache = _animcache3; fileindex = _mFileIndex3; break;
                case 4: filename = "anim4"; cache = _animcache4; fileindex = _mFileIndex4; break;
                case 5: filename = "anim5"; cache = _animcache5; fileindex = _mFileIndex5; break;

                default: filename = "anim"; cache = _animcache; fileindex = _mFileIndex; break;
            }
            string idx = Path.Combine(path, filename + ".idx");
            string mul = Path.Combine(path, filename + ".mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int idxc = 0; idxc < cache.Length; ++idxc)
                    {
                        AnimIdx anim;
                        if (cache != null)
                        {
                            if (cache[idxc] != null)
                                anim = cache[idxc];
                            else
                                anim = cache[idxc] = new AnimIdx(idxc, fileindex, filetype);
                        }
                        else
                            anim = cache[idxc] = new AnimIdx(idxc, fileindex, filetype);

                        if (anim == null)
                        {
                            binidx.Write(-1);
                            binidx.Write(-1);
                            binidx.Write(-1);
                        }
                        else
                            anim.Save(binmul, binidx);
                    }
                }
            }
        }
    }

    public sealed class AnimIdx
    {
        public int Idxextra;
        public ushort[] Palette { get; private set; }
        public List<FrameEdit> Frames { get; private set; }

        public AnimIdx(int index, FileIndex fileIndex, int filetype)
        {
            Palette = new ushort[0x100];
            int length, extra;
            bool patched;
            Stream stream = fileIndex.Seek(index, out length, out extra, out patched);
            if ((stream == null) || (length < 1))
                return;

            Idxextra = extra;
            using (BinaryReader bin = new BinaryReader(stream))
            {
                for (int i = 0; i < 0x100; ++i)
                    Palette[i] = (ushort)(bin.ReadUInt16() ^ 0x8000);

                int start = (int)bin.BaseStream.Position;
                int frameCount = bin.ReadInt32();

                int[] lookups = new int[frameCount];

                for (int i = 0; i < frameCount; ++i)
                    lookups[i] = start + bin.ReadInt32();

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
            Idxextra = extra;
            for (int i = 0; i < 0x100; ++i)
                Palette[i] = (ushort)(bin.ReadUInt16() ^ 0x8000);

            int start = (int)bin.BaseStream.Position;
            int frameCount = bin.ReadInt32();

            int[] lookups = new int[frameCount];

            for (int i = 0; i < frameCount; ++i)
                lookups[i] = start + bin.ReadInt32();

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
                return null;
            Bitmap[] bits = new Bitmap[Frames.Count];
            for (int i = 0; i < bits.Length; ++i)
            {
                FrameEdit frame = Frames[i];
                int width = frame.Width;
                int height = frame.Height;
                if (height == 0 || width == 0)
                    continue;
                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                int xBase = frame.Center.X - 0x200;
                int yBase = frame.Center.Y + height - 0x200;

                line += xBase;
                line += yBase * delta;
                for (int j = 0; j < frame.RawData.Length; ++j)
                {
                    FrameEdit.Raw raw = frame.RawData[j];

                    ushort* cur = line + (((raw.Offy) * delta) + ((raw.Offx) & 0x3FF));
                    ushort* end = cur + (raw.Run);

                    int ii = 0;
                    while (cur < end)
                    {
                        *cur++ = Palette[raw.Data[ii++]];
                    }
                }
                bmp.UnlockBits(bd);
                bits[i] = bmp;
            }
            return bits;
        }

        public void AddFrame(Bitmap bit)
        {
            if (Frames == null)
                Frames = new List<FrameEdit>();
            Frames.Add(new FrameEdit(bit, Palette, 0, 0));
        }

        public void ReplaceFrame(Bitmap bit, int index)
        {
            if ((Frames == null) || (Frames.Count == 0))
                return;
            if (index > Frames.Count)
                return;
            Frames[index] = new FrameEdit(bit, Palette, Frames[index].Center.X, Frames[index].Center.Y);
        }

        public void RemoveFrame(int index)
        {
            if (Frames == null)
                return;
            if (index > Frames.Count)
                return;
            Frames.RemoveAt(index);
        }

        public void ClearFrames()
        {
            Frames?.Clear();
        }

        //Soulblighter Modification
        public void GetGifPalette(Bitmap bit)
        {
            using (MemoryStream imageStreamSource = new MemoryStream())
            {
                ImageConverter ic = new ImageConverter();
                byte[] btImage = (byte[])ic.ConvertTo(bit, typeof(byte[]));
                imageStreamSource.Write(btImage, 0, btImage.Length);
                GifBitmapDecoder decoder = new GifBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapPalette pal = decoder.Palette;
                int i;
                for (i = 0; i < 0x100; i++)
                {
                    Palette[i] = 0;
                }
                try
                {
                    i = 0;
                    while (i < 0x100)//&& i < pal.Colors.Count)
                    {

                        int red = pal.Colors[i].R / 8;
                        int green = pal.Colors[i].G / 8;
                        int blue = pal.Colors[i].B / 8;
                        int contaFinal = (((0x400 * red) + (0x20 * green)) + blue) + 0x8000;
                        if (contaFinal == 0x8000)
                            contaFinal = 0x8001;
                        Palette[i] = (ushort)contaFinal;
                        i++;
                    }
                }
                catch (System.IndexOutOfRangeException)
                { }
                catch (System.ArgumentOutOfRangeException)
                { }
                for (i = 0; i < 0x100; i++)
                {
                    if (Palette[i] < 0x8000)
                        Palette[i] = 0x8000;
                }
            }
        }

        public unsafe void GetImagePalette(Bitmap bit)
        {
            int count = 0;
            Bitmap bmp = new Bitmap(bit);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            ushort* cur = line;
            int i = 0;
            while (i < 0x100)
            {
                Palette[i] = 0;
                i++;
            }
            int y = 0;
            while (y < bmp.Height)
            {
                cur = line;
                for (int x = 0; x < bmp.Width; x++)
                {
                    ushort c = cur[x];
                    if (c != 0)
                    {
                        bool found = false;
                        i = 0;
                        while (i < Palette.Length)
                        {
                            if (Palette[i] == c)
                            {
                                found = true;
                                break;
                            }
                            i++;
                        }
                        if (!found)
                            Palette[count++] = c;
                        if (count >= 0x100)
                            break;
                    }
                }
                for (i = 0; i < 0x100; i++)
                {
                    if (Palette[i] < 0x8000)
                        Palette[i] = 0x8000;
                }
                if (count >= 0x100)
                    break;
                y++;
                line += delta;
            }
        }

        public void PaletteConversor(int seletor)
        {
            int i;
            for (i = 0; i < 0x100; i++)
            {
                int blueTemp = (Palette[i] - 0x8000) / 0x20;
                blueTemp *= 0x20;
                blueTemp = (Palette[i] - 0x8000) - blueTemp;
                int greenTemp = (Palette[i] - 0x8000) / 0x400;
                greenTemp *= 0x400;
                greenTemp = ((Palette[i] - 0x8000) - greenTemp) - blueTemp;
                greenTemp /= 0x20;
                int redTemp = (Palette[i] - 0x8000) / 0x400;
                int contaFinal = 0;
                switch (seletor)
                {
                    case 1: contaFinal = (((0x400 * redTemp) + (0x20 * greenTemp)) + blueTemp) + 0x8000;
                        break;
                    case 2: contaFinal = (((0x400 * redTemp) + (0x20 * blueTemp)) + greenTemp) + 0x8000;
                        break;
                    case 3: contaFinal = (((0x400 * greenTemp) + (0x20 * redTemp)) + blueTemp) + 0x8000;
                        break;
                    case 4: contaFinal = (((0x400 * greenTemp) + (0x20 * blueTemp)) + redTemp) + 0x8000;
                        break;
                    case 5: contaFinal = (((0x400 * blueTemp) + (0x20 * greenTemp)) + redTemp) + 0x8000;
                        break;
                    case 6: contaFinal = (((0x400 * blueTemp) + (0x20 * redTemp)) + greenTemp) + 0x8000;
                        break;
                }
                if (contaFinal == 0x8000)
                    contaFinal = 0x8001;
                Palette[i] = (ushort)contaFinal;
            }
            for (i = 0; i < 0x100; i++)
            {
                if (Palette[i] < 0x8000)
                    Palette[i] = 0x8000;
            }
        }

        public void PaletteReductor(int redp, int greenp, int bluep)
        {
            int i;
            redp /= 8;
            greenp /= 8;
            bluep /= 8;
            for (i = 0; i < 0x100; i++)
            {
                int blueTemp = (Palette[i] - 0x8000) / 0x20;
                blueTemp *= 0x20;
                blueTemp = (Palette[i] - 0x8000) - blueTemp;
                int greenTemp = (Palette[i] - 0x8000) / 0x400;
                greenTemp *= 0x400;
                greenTemp = ((Palette[i] - 0x8000) - greenTemp) - blueTemp;
                greenTemp /= 0x20;
                int redTemp = (Palette[i] - 0x8000) / 0x400;
                redTemp += redp;
                greenTemp += greenp;
                blueTemp += bluep;
                if (redTemp < 0)
                    redTemp = 0;
                if (redTemp > 0x1f)
                    redTemp = 0x1f;
                if (greenTemp < 0)
                    greenTemp = 0;
                if (greenTemp > 0x1f)
                    greenTemp = 0x1f;
                if (blueTemp < 0)
                    blueTemp = 0;
                if (blueTemp > 0x1f)
                    blueTemp = 0x1f;
                int contaFinal = (((0x400 * redTemp) + (0x20 * greenTemp)) + blueTemp) + 0x8000;
                if (contaFinal == 0x8000)
                    contaFinal = 0x8001;
                Palette[i] = (ushort)contaFinal;
            }
            for (i = 0; i < 0x100; i++)
            {
                if (Palette[i] < 0x8000)
                    Palette[i] = 0x8000;
            }
        }
        //End of Soulblighter Modification


        public unsafe void ExportPalette(string filename, int type)
        {
            switch (type)
            {
                case 0:
                    using (StreamWriter tex = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite)))
                    {
                        for (int i = 0; i < 0x100; ++i)
                        {
                            tex.WriteLine(Palette[i]);
                        }
                    }
                    break;
                case 1:
                    {
                        Bitmap bmp = new Bitmap(0x100, 20, PixelFormat.Format16bppArgb1555);
                        BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 0x100, 20), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                        ushort* line = (ushort*)bd.Scan0;
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
                        Bitmap b = new Bitmap(bmp);
                        b.Save(filename, ImageFormat.Bmp);
                        b.Dispose();
                        bmp.Dispose();
                        break;
                    }
                case 2:
                    {
                        Bitmap bmp = new Bitmap(0x100, 20, PixelFormat.Format16bppArgb1555);
                        BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 0x100, 20), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                        ushort* line = (ushort*)bd.Scan0;
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
                        Bitmap b = new Bitmap(bmp);
                        b.Save(filename, ImageFormat.Tiff);
                        b.Dispose();
                        bmp.Dispose();
                        break;
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
                bin.Write((ushort)(Palette[i] ^ 0x8000));
            long startpos = bin.BaseStream.Position;
            bin.Write(Frames.Count);
            long seek = bin.BaseStream.Position;
            long curr = bin.BaseStream.Position + 4 * Frames.Count;
            foreach (FrameEdit frame in Frames)
            {
                bin.BaseStream.Seek(seek, SeekOrigin.Begin);
                bin.Write((int)(curr - startpos));
                seek = bin.BaseStream.Position;
                bin.BaseStream.Seek(curr, SeekOrigin.Begin);
                frame.Save(bin);
                curr = bin.BaseStream.Position;
            }

            start = bin.BaseStream.Position - start;
            idx.Write((int)start);
            idx.Write(Idxextra);
        }

        public void ExportToVd(BinaryWriter bin, ref long indexpos, ref long animpos)
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
                bin.Write((ushort)(Palette[i] ^ 0x8000));
            long startpos = (int)bin.BaseStream.Position;
            bin.Write(Frames.Count);
            long seek = (int)bin.BaseStream.Position;
            long curr = bin.BaseStream.Position + 4 * Frames.Count;
            foreach (FrameEdit frame in Frames)
            {
                bin.BaseStream.Seek(seek, SeekOrigin.Begin);
                bin.Write((int)(curr - startpos));
                seek = bin.BaseStream.Position;
                bin.BaseStream.Seek(curr, SeekOrigin.Begin);
                frame.Save(bin);
                curr = bin.BaseStream.Position;
            }

            long length = bin.BaseStream.Position - animpos;
            animpos = bin.BaseStream.Position;
            bin.BaseStream.Seek(indexpos, SeekOrigin.Begin);
            bin.Write((int)length);
            bin.Write(Idxextra);
            indexpos = bin.BaseStream.Position;
        }
    }

    public sealed class FrameEdit
    {
        private const int DoubleXor = (0x200 << 22) | (0x200 << 12);
        public struct Raw
        {
            public int Run;
            public int Offx;
            public int Offy;
            public byte[] Data;
        }
        public Raw[] RawData { get; }
        public Point Center { get; set; }
        public int Width;
        public int Height;

        public FrameEdit(BinaryReader bin)
        {
            int xCenter = bin.ReadInt16();
            int yCenter = bin.ReadInt16();

            Width = bin.ReadUInt16();
            Height = bin.ReadUInt16();
            if (Height == 0 || Width == 0)
                return;
            int header;

            List<Raw> tmp = new List<Raw>();
            while ((header = bin.ReadInt32()) != 0x7FFF7FFF)
            {
                Raw raw = new Raw();
                header ^= DoubleXor;
                raw.Run = (header & 0xFFF);
                raw.Offy = ((header >> 12) & 0x3FF);
                raw.Offx = ((header >> 22) & 0x3FF);

                int i = 0;
                raw.Data = new byte[raw.Run];
                while (i < raw.Run)
                {
                    raw.Data[i++] = bin.ReadByte();
                }
                tmp.Add(raw);
            }
            RawData = tmp.ToArray();
            Center = new Point(xCenter, yCenter);
        }

        public unsafe FrameEdit(Bitmap bit, ushort[] palette, int centerx, int centery)
        {
            Center = new Point(centerx, centery);
            Width = bit.Width;
            Height = bit.Height;
            BitmapData bd = bit.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            List<Raw> tmp = new List<Raw>();

            int x = 0;
            for (int y = 0; y < bit.Height; ++y, line += delta)
            {
                ushort* cur = line;
                int i = 0;
                int j = 0;
                x = 0;
                while (i < bit.Width)
                {
                    i = x;
                    for (i = x; i <= bit.Width; ++i)
                    {
                        //first pixel set
                        if (i < bit.Width)
                        {
                            if (cur[i] != 0)
                                break;
                        }
                    }
                    if (i < bit.Width)
                    {
                        for (j = (i + 1); j < bit.Width; ++j)
                        {
                            //next non set pixel
                            if (cur[j] == 0)
                                break;
                        }
                        Raw raw = new Raw();
                        raw.Run = j - i;
                        raw.Offx = j - raw.Run - centerx;
                        raw.Offx += 512;
                        raw.Offy = y - centery - bit.Height;
                        raw.Offy += 512;

                        int r = 0;
                        raw.Data = new byte[raw.Run];
                        while (r < raw.Run)
                        {
                            ushort col = cur[r + i];
                            raw.Data[r++] = GetPaletteIndex(palette, col);
                        }
                        tmp.Add(raw);
                        x = j + 1;
                        i = x;
                    }
                }
            }

            RawData = tmp.ToArray();
            bit.UnlockBits(bd);
        }

        public void ChangeCenter(int x, int y)
        {
            for (int i = 0; i < RawData.Length; i++)
            {
                RawData[i].Offx += Center.X;
                RawData[i].Offx -= x;
                RawData[i].Offy += Center.Y;
                RawData[i].Offy -= y;
            }
            Center = new Point(x, y);
        }

        private static byte GetPaletteIndex(ushort[] palette, ushort col)
        {
            for (int i = 0; i < palette.Length; i++)
            {
                if (palette[i] == col)
                    return (byte)i;
            }
            return 0;
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
                    int newHeader = RawData[j].Run | (RawData[j].Offy << 12) | (RawData[j].Offx << 22);
                    newHeader ^= DoubleXor;
                    bin.Write(newHeader);
                    foreach (byte b in RawData[j].Data)
                        bin.Write(b);
                }
            }
            bin.Write(0x7FFF7FFF);
        }
    }
}