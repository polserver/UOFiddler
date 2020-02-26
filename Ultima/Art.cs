using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

using Ultima.Helpers;

namespace Ultima
{
    public sealed class Art
    {
        private static FileIndex _mFileIndex = new FileIndex("Artidx.mul", "Art.mul", "artLegacyMUL.uop", 0x10000/*0x13FDC*/, 4, ".tga", 0x13FDC, false);
        private static Bitmap[] _mCache;
        private static bool[] _mRemoved;
        private static readonly Hashtable _mPatched = new Hashtable();
        public static bool Modified = false;

        private static byte[] _mStreamBuffer;
        private static byte[] _validbuffer;
       

        struct CheckSums
        {
            public byte[] Checksum;
            public int Pos;
            public int Length;
            public int Index;
        }
        private static List<CheckSums> _checksumsLand;
        private static List<CheckSums> _checksumsStatic;

        static Art()
        {
            _mCache = new Bitmap[GetIdxLength()];
            _mRemoved = new bool[GetIdxLength()];
        }

        public static int GetMaxItemId()
        {
            if (GetIdxLength() == 0xC000)
                return 0x7FFF;

            if (GetIdxLength() == 0x13FDC)
                return 0xFFDB;

            return 0x3FFF;
        }

        public static bool IsUoahs()
        {
            return (GetIdxLength() == 0x13FDC);
        }

        public static ushort GetLegalItemId(int itemId, bool checkmaxid=true)
        {
            if (itemId < 0)
                return 0;

            if (checkmaxid)
            {
                int max = GetMaxItemId();
                if (itemId > max)
                    return 0;
            }
            return (ushort)itemId;
        }

        public static int GetIdxLength()
        {
            return (int)(_mFileIndex.IdxLength / 12);
        }
        /// <summary>
        /// ReReads Art.mul
        /// </summary>
        public static void Reload()
        {
            _mFileIndex = new FileIndex("Artidx.mul", "Art.mul", "artLegacyMUL.uop", 0x10000/*0x13FDC*/, 4, ".tga", 0x13FDC, false);
            _mCache = new Bitmap[GetIdxLength()];
            _mRemoved = new bool[GetIdxLength()];
            _mPatched.Clear();
            Modified = false;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_mCache"/> of Static
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceStatic(int index, Bitmap bmp)
        {
            index = GetLegalItemId(index);
            index += 0x4000;

            _mCache[index] = bmp;
            _mRemoved[index] = false;
            if (_mPatched.Contains(index))
                _mPatched.Remove(index);
            Modified = true;
        }

        /// <summary>
        /// Sets bmp of index in <see cref="_mCache"/> of Land
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void ReplaceLand(int index, Bitmap bmp)
        {
            index &= 0x3FFF;
            _mCache[index] = bmp;
            _mRemoved[index] = false;
            if (_mPatched.Contains(index))
                _mPatched.Remove(index);
            Modified = true;
        }

        /// <summary>
        /// Removes Static index <see cref="_mRemoved"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveStatic(int index)
        {
            index = GetLegalItemId(index);
            index += 0x4000;

            _mRemoved[index] = true;
            Modified = true;
        }

        /// <summary>
        /// Removes Land index <see cref="_mRemoved"/>
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveLand(int index)
        {
            index &= 0x3FFF;
            _mRemoved[index] = true;
            Modified = true;
        }

        /// <summary>
        /// Tests if Static is definied (width and hight check)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static unsafe bool IsValidStatic(int index)
        {
            index = GetLegalItemId(index);
            index += 0x4000;
            
            if (_mRemoved[index])
                return false;
            if (_mCache[index] != null)
                return true;

            int length, extra;
            bool patched;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);

            if (stream == null)
                return false;

            if (_validbuffer == null)
                _validbuffer = new byte[4];
            stream.Seek(4, SeekOrigin.Current);
            stream.Read(_validbuffer, 0, 4);
            fixed (byte* b = _validbuffer)
            {
                short* dat = (short*)b;
                if (*dat++ <= 0 || *dat <= 0)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// Tests if LandTile is definied
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidLand(int index)
        {
            index &= 0x3FFF;
            if (_mRemoved[index])
                return false;
            if (_mCache[index] != null)
                return true;

            int length, extra;
            bool patched;

            return _mFileIndex.Valid(index, out length, out extra, out patched);
        }

        /// <summary>
        /// Returns Bitmap of LandTile (with Cache)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Bitmap GetLand(int index)
        {
            bool patched;
            return GetLand(index, out patched);
        }
        /// <summary>
        /// Returns Bitmap of LandTile (with Cache) and verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public static Bitmap GetLand(int index, out bool patched)
        {
            index &= 0x3FFF;
            if (_mPatched.Contains(index))
                patched = (bool)_mPatched[index];
            else
                patched = false;

            if (_mRemoved[index])
                return null;
            if (_mCache[index] != null)
                return _mCache[index];

            int length, extra;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);
            if (stream == null)
                return null;
            if (patched)
                _mPatched[index] = true;

            if (Files.CacheData)
                return _mCache[index] = LoadLand(stream, length);
            else
                return LoadLand(stream, length);
        }

        public static byte[] GetRawLand(int index)
        {
            index &= 0x3FFF;

            int length, extra;
            bool patched;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);
            if (stream == null)
                return null;
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }

        /// <summary>
        /// Returns Bitmap of Static (with Cache)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Bitmap GetStatic(int index, bool checkmaxid=true)
        {
            bool patched;
            return GetStatic(index, out patched, checkmaxid);
        }
        /// <summary>
        /// Returns Bitmap of Static (with Cache) and verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public static Bitmap GetStatic(int index, out bool patched, bool checkmaxid=true)
        {
            index = GetLegalItemId(index, checkmaxid);
            index += 0x4000;
            
            if (_mPatched.Contains(index))
                patched = (bool)_mPatched[index];
            else
                patched = false;

            if (_mRemoved[index])
                return null;
            if (_mCache[index] != null)
                return _mCache[index];

            int length, extra;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);
            if (stream == null)
                return null;
            if (patched)
                _mPatched[index] = true;

            if (Files.CacheData)
                return _mCache[index] = LoadStatic(stream, length);
            else
                return LoadStatic(stream, length);
        }

        public static byte[] GetRawStatic(int index)
        {
            index = GetLegalItemId(index);
            index += 0x4000;
            
            int length, extra;
            bool patched;
            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);
            if (stream == null)
                return null;
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }

        public unsafe static void Measure(Bitmap bmp, out int xMin, out int yMin, out int xMax, out int yMax)
        {
            xMin = yMin = 0;
            xMax = yMax = -1;

            if (bmp == null || bmp.Width <= 0 || bmp.Height <= 0)
                return;

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);

            int delta = (bd.Stride >> 1) - bd.Width;
            int lineDelta = bd.Stride >> 1;

            ushort* pBuffer = (ushort*)bd.Scan0;
            ushort* pLineEnd = pBuffer + bd.Width;
            ushort* pEnd = pBuffer + (bd.Height * lineDelta);

            bool foundPixel = false;

            int x = 0, y = 0;

            while (pBuffer < pEnd)
            {
                while (pBuffer < pLineEnd)
                {
                    ushort c = *pBuffer++;

                    if ((c & 0x8000) != 0)
                    {
                        if (!foundPixel)
                        {
                            foundPixel = true;
                            xMin = xMax = x;
                            yMin = yMax = y;
                        }
                        else
                        {
                            if (x < xMin)
                                xMin = x;

                            if (y < yMin)
                                yMin = y;

                            if (x > xMax)
                                xMax = x;

                            if (y > yMax)
                                yMax = y;
                        }
                    }
                    ++x;
                }

                pBuffer += delta;
                pLineEnd += lineDelta;
                ++y;
                x = 0;
            }

            bmp.UnlockBits(bd);
        }

        private static unsafe Bitmap LoadStatic(Stream stream, int length)
        {
            Bitmap bmp;
            if (_mStreamBuffer == null || _mStreamBuffer.Length < length)
                _mStreamBuffer = new byte[length];
            stream.Read(_mStreamBuffer, 0, length);
            stream.Close();

            fixed (byte* data = _mStreamBuffer)
            {
                ushort* bindata = (ushort*)data;
                int count = 2;
                //bin.ReadInt32();
                int width = bindata[count++];
                int height = bindata[count++];

                if (width <= 0 || height <= 0)
                    return null;

                int[] lookups = new int[height];

                int start = (height + 4);

                for (int i = 0; i < height; ++i)
                    lookups[i] = start + (bindata[count++]);

                bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);


                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;


                for (int y = 0; y < height; ++y, line += delta)
                {
                    count = lookups[y];

                    ushort* cur = line;
                    ushort* end;
                    int xOffset, xRun;

                    while (((xOffset = bindata[count++]) + (xRun = bindata[count++])) != 0)
                    {
                        if (xOffset > delta)
                            break;
                        cur += xOffset;
                        if (xOffset + xRun > delta)
                            break;
                        end = cur + xRun;

                        while (cur < end)
                            *cur++ = (ushort)(bindata[count++] ^ 0x8000);
                    }
                }
                bmp.UnlockBits(bd);
            }
            return bmp;
        }

        private static unsafe Bitmap LoadLand(Stream stream, int length)
        {
            Bitmap bmp = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            if (_mStreamBuffer == null || _mStreamBuffer.Length < length)
                _mStreamBuffer = new byte[length];
            stream.Read(_mStreamBuffer, 0, length);
            stream.Close();
            fixed (byte* bindata = _mStreamBuffer)
            {
                ushort* bdata = (ushort*)bindata;
                int xOffset = 21;
                int xRun = 2;

                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < 22; ++y, --xOffset, xRun += 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                }

                xOffset = 0;
                xRun = 44;

                for (int y = 0; y < 22; ++y, ++xOffset, xRun -= 2, line += delta)
                {
                    ushort* cur = line + xOffset;
                    ushort* end = cur + xRun;

                    while (cur < end)
                        *cur++ = (ushort)(*bdata++ | 0x8000);
                }
            }
            bmp.UnlockBits(bd);
            return bmp;
        }


        /// <summary>
        /// Saves mul
        /// </summary>
        /// <param name="path"></param>
        public static unsafe void Save(string path)
        {
            _checksumsLand=new List<CheckSums>();
            _checksumsStatic = new List<CheckSums>();
            string idx = Path.Combine(path, "artidx.mul");
            string mul = Path.Combine(path, "art.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                MemoryStream memidx = new MemoryStream();
                MemoryStream memmul = new MemoryStream();
                SHA256Managed sha = new SHA256Managed();
                //StreamWriter Tex = new StreamWriter(new FileStream("d:/artlog.txt", FileMode.Create, FileAccess.ReadWrite));

                using (BinaryWriter binidx = new BinaryWriter(memidx),
                                    binmul = new BinaryWriter(memmul))
                {
                    for (int index = 0; index < GetIdxLength(); index++)
                    {
                        Files.FireFileSaveEvent();
                        if (_mCache[index] == null)
                        {
                            if (index < 0x4000)
                                _mCache[index] = GetLand(index);
                            else
                                _mCache[index] = GetStatic(index - 0x4000,false);
                        }
                        Bitmap bmp = _mCache[index];
                        if ((bmp == null) || (_mRemoved[index]))
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0); // length
                            binidx.Write(-1); // extra
                            //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, (int)-1, (int)-1));
                        }
                        else if (index < 0x4000)
                        {
                            byte[] checksum = bmp.ToArray(PixelFormat.Format16bppArgb1555).ToSha256();
                            CheckSums sum;
                            if (CompareSaveImagesLand(checksum, out sum))
                            {
                                binidx.Write(sum.Pos); //lookup
                                binidx.Write(sum.Length);
                                binidx.Write(0);
                                //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, (int)sum.pos, (int)sum.length));
                                //Tex.WriteLine(System.String.Format("0x{0:X4} -> 0x{1:X4}", sum.index, index));
                                continue;
                            }
                            //land
                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            ushort* line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;
                            binidx.Write((int)binmul.BaseStream.Position); //lookup
                            int length = (int)binmul.BaseStream.Position;
                            int x = 22;
                            int y = 0;
                            int linewidth = 2;
                            for (int m = 0; m < 22; ++m, ++y, line += delta, linewidth += 2)
                            {
                                --x;
                                ushort* cur = line;
                                for (int n = 0; n < linewidth; ++n)
                                    binmul.Write((ushort)(cur[x + n] ^ 0x8000));
                            }
                            x = 0;
                            linewidth = 44;
                            y = 22;
                            line = (ushort*)bd.Scan0;
                            line += delta * 22;
                            for (int m = 0; m < 22; m++, y++, line += delta, ++x, linewidth -= 2)
                            {
                                ushort* cur = line;
                                for (int n = 0; n < linewidth; n++)
                                    binmul.Write((ushort)(cur[x + n] ^ 0x8000));
                            }
                            int start = length;
                            length = (int)binmul.BaseStream.Position - length;
                            binidx.Write(length);
                            binidx.Write(0);
                            bmp.UnlockBits(bd);
                            CheckSums s = new CheckSums() { Pos = start, Length = length, Checksum = checksum, Index=index };
                            //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, start, length));
                            _checksumsLand.Add(s);
                        }
                        else
                        {
                            byte[] checksum = bmp.ToArray(PixelFormat.Format16bppArgb1555).ToSha256();
                            CheckSums sum;
                            if (CompareSaveImagesStatic(checksum,out sum))
                            {
                                binidx.Write(sum.Pos); //lookup
                                binidx.Write(sum.Length);
                                binidx.Write(0);
                                //Tex.WriteLine(System.String.Format("0x{0:X4} -> 0x{1:X4}", sum.index, index));
                                //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, sum.pos, sum.length));
                                continue;
                            }

                            // art
                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            ushort* line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;
                            binidx.Write((int)binmul.BaseStream.Position); //lookup
                            int length = (int)binmul.BaseStream.Position;
                            binmul.Write(1234); // header
                            binmul.Write((short)bmp.Width);
                            binmul.Write((short)bmp.Height);
                            int lookup = (int)binmul.BaseStream.Position;
                            int streamloc = lookup + bmp.Height * 2;
                            int width = 0;
                            for (int i = 0; i < bmp.Height; ++i)// fill lookup
                                binmul.Write(width);
                            int x = 0;
                            for (int y = 0; y < bmp.Height; ++y, line += delta)
                            {
                                ushort* cur = line;
                                width = (int)(binmul.BaseStream.Position - streamloc) / 2;
                                binmul.BaseStream.Seek(lookup + y * 2, SeekOrigin.Begin);
                                binmul.Write(width);
                                binmul.BaseStream.Seek(streamloc + width * 2, SeekOrigin.Begin);
                                int i = 0;
                                int j = 0;
                                x = 0;
                                while (i < bmp.Width)
                                {
                                    i = x;
                                    for (i = x; i <= bmp.Width; ++i)
                                    {
                                        //first pixel set
                                        if (i < bmp.Width)
                                        {
                                            if (cur[i] != 0)
                                                break;
                                        }
                                    }
                                    if (i < bmp.Width)
                                    {
                                        for (j = (i + 1); j < bmp.Width; ++j)
                                        {
                                            //next non set pixel
                                            if (cur[j] == 0)
                                                break;
                                        }
                                        binmul.Write((short)(i - x)); //xoffset
                                        binmul.Write((short)(j - i)); //run
                                        for (int p = i; p < j; ++p)
                                            binmul.Write((ushort)(cur[p] ^ 0x8000));
                                        x = j;
                                    }
                                }
                                binmul.Write((short)0); //xOffset
                                binmul.Write((short)0); //Run
                            }
                            int start = length;
                            length = (int)binmul.BaseStream.Position - length;
                            binidx.Write(length);
                            binidx.Write(0);
                            bmp.UnlockBits(bd);
                            CheckSums s = new CheckSums() { Pos = start, Length = length, Checksum = checksum, Index=index };
                            //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, start, length));
                            _checksumsStatic.Add(s);
                        }
                    }
                    memidx.WriteTo(fsidx);
                    memmul.WriteTo(fsmul);
                }
            }
        }

        private static bool CompareSaveImagesLand(byte[] newchecksum, out CheckSums sum)
        {
            sum = new CheckSums();
            for (int i = 0; i < _checksumsLand.Count; ++i)
            {
                byte[] cmp = _checksumsLand[i].Checksum;
                if (((cmp == null) || (newchecksum == null))
                    || (cmp.Length != newchecksum.Length))
                {
                    return false;
                }
                bool valid = true;
                for (int j = 0; j < cmp.Length; ++j)
                {
                    if (cmp[j] != newchecksum[j])
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    sum = _checksumsLand[i];
                    return true;
                }
            }
            return false;
        }
        private static bool CompareSaveImagesStatic(byte[] newchecksum, out CheckSums sum)
        {
            sum = new CheckSums();
            for (int i = 0; i < _checksumsStatic.Count; ++i)
            {
                byte[] cmp = _checksumsStatic[i].Checksum;
                if (((cmp == null) || (newchecksum == null))
                    || (cmp.Length != newchecksum.Length))
                {
                    return false;
                }
                bool valid = true;
                for (int j = 0; j < cmp.Length; ++j)
                {
                    if (cmp[j] != newchecksum[j])
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    sum = _checksumsStatic[i];
                    return true;
                }
            }
            return false;
        }
    }
}