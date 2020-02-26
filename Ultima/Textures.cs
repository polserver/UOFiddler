using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Ultima
{
    public sealed class Textures
    {
        private static FileIndex _mFileIndex = new FileIndex("Texidx.mul", "Texmaps.mul", 0x4000, 10);
        private static Bitmap[] _mCache = new Bitmap[0x4000];
        private static bool[] _mRemoved = new bool[0x4000];
        private static readonly Hashtable _mPatched = new Hashtable();

        private static byte[] _mStreamBuffer;
        struct CheckSums
        {
            public byte[] Checksum;
            public int Pos;
            public int Length;
            public int Index;
        }
        private static List<CheckSums> _checksums;

        /// <summary>
        /// ReReads texmaps
        /// </summary>
        public static void Reload()
        {
            _mFileIndex = new FileIndex("Texidx.mul", "Texmaps.mul", 0x4000, 10);
            _mCache = new Bitmap[0x4000];
            _mRemoved = new bool[0x4000];
            _mPatched.Clear();
        }

        public static int GetIdxLength()
        {
            return (int)(_mFileIndex.IdxLength / 12);
        }

        /// <summary>
        /// Removes Texture <see cref="_mRemoved"/>
        /// </summary>
        /// <param name="index"></param>
        public static void Remove(int index)
        {
            _mRemoved[index] = true;
        }

        /// <summary>
        /// Replaces Texture
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void Replace(int index, Bitmap bmp)
        {
            _mCache[index] = bmp;
            _mRemoved[index] = false;
            if (_mPatched.Contains(index))
                _mPatched.Remove(index);
        }

        /// <summary>
        /// Tests if index is valid Texture
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TestTexture(int index)
        {
            int length, extra;
            bool patched;
            if (_mRemoved[index])
                return false;
            if (_mCache[index] != null)
                return true;
            bool valid = _mFileIndex.Valid(index, out length, out extra, out patched);
            if ((!valid) || (length == 0))
                return false;
            return true;
        }

        /// <summary>
        /// Returns Bitmap of Texture
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public unsafe static Bitmap GetTexture(int index)
        {
            bool patched;
            return GetTexture(index, out patched);
        }
        /// <summary>
        /// Returns Bitmap of Texture with verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public unsafe static Bitmap GetTexture(int index, out bool patched)
        {
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
            if (length == 0)
                return null;
            if (patched)
                _mPatched[index] = true;

            int size = extra == 0 ? 64 : 128;

            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int max = size * size * 2;

            if (_mStreamBuffer == null || _mStreamBuffer.Length < max)
                _mStreamBuffer = new byte[max];
            stream.Read(_mStreamBuffer, 0, max);

            fixed (byte* data = _mStreamBuffer)
            {
                ushort* bindat = (ushort*)data;
                for (int y = 0; y < size; ++y, line += delta)
                {
                    ushort* cur = line;
                    ushort* end = cur + size;

                    while (cur < end)
                        *cur++ = (ushort)(*bindat++ ^ 0x8000);
                }
            }

            bmp.UnlockBits(bd);

            stream.Close();
            if (!Files.CacheData)
                return _mCache[index] = bmp;
            else
                return bmp;
        }

        public unsafe static void Save(string path)
        {
            string idx = Path.Combine(path, "texidx.mul");
            string mul = Path.Combine(path, "texmaps.mul");
            _checksums = new List<CheckSums>();
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                MemoryStream memidx = new MemoryStream();
                MemoryStream memmul = new MemoryStream();
                using (BinaryWriter binidx = new BinaryWriter(memidx),
                                    binmul = new BinaryWriter(memmul))
                {
                    SHA256Managed sha = new SHA256Managed();
                    //StreamWriter Tex = new StreamWriter(new FileStream("d:/texlog.txt", FileMode.Create, FileAccess.ReadWrite));
                    for (int index = 0; index < GetIdxLength(); ++index)
                    {
                        if (_mCache[index] == null)
                            _mCache[index] = GetTexture(index);

                        Bitmap bmp = _mCache[index];
                        if ((bmp == null) || (_mRemoved[index]))
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0); // length
                            binidx.Write(-1); // extra
                        }
                        else
                        {
                            MemoryStream ms = new MemoryStream();
                            bmp.Save(ms, ImageFormat.Bmp);
                            byte[] checksum = sha.ComputeHash(ms.ToArray());
                            CheckSums sum;
                            if (CompareSaveImages(checksum, out sum))
                            {
                                binidx.Write(sum.Pos); //lookup
                                binidx.Write(sum.Length);
                                binidx.Write(0);
                                //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, (int)sum.pos, (int)sum.length));
                                //Tex.WriteLine(System.String.Format("0x{0:X4} -> 0x{1:X4}", sum.index, index));
                                continue;
                            }
                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            ushort* line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;

                            binidx.Write((int)binmul.BaseStream.Position); //lookup
                            int length = (int)binmul.BaseStream.Position;

                            for (int y = 0; y < bmp.Height; ++y, line += delta)
                            {
                                ushort* cur = line;
                                for (int x = 0; x < bmp.Width; ++x)
                                {
                                    binmul.Write((ushort)(cur[x] ^ 0x8000));
                                }
                            }
                            int start = length;
                            length = (int)binmul.BaseStream.Position - length;
                            binidx.Write(length);
                            binidx.Write(bmp.Width == 64 ? 0 : 1);
                            bmp.UnlockBits(bd);
                            CheckSums s = new CheckSums() { Pos = start, Length = length, Checksum = checksum, Index = index };
                            //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, start, length));
                            _checksums.Add(s);
                        }
                    }
                    memidx.WriteTo(fsidx);
                    memmul.WriteTo(fsmul);
                }
            }
        }

        private static bool CompareSaveImages(byte[] newchecksum, out CheckSums sum)
        {
            sum = new CheckSums();
            for (int i = 0; i < _checksums.Count; ++i)
            {
                byte[] cmp = _checksums[i].Checksum;
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
                    sum = _checksums[i];
                    return true;
                }
            }
            return false;
        }
    }
}