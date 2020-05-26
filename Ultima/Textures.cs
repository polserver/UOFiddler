using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace Ultima
{
    public sealed class Textures
    {
        private static FileIndex _fileIndex = new FileIndex("Texidx.mul", "Texmaps.mul", 0x4000, 10);
        private static Bitmap[] _cache = new Bitmap[0x4000];
        private static bool[] _removed = new bool[0x4000];
        private static readonly Hashtable _patched = new Hashtable();
        private static byte[] _streamBuffer;

        private struct Checksums
        {
            public byte[] checksum;
            public int pos;
            public int length;
            public int index;
        }

        private static List<Checksums> _checkSums;

        /// <summary>
        /// ReReads texmaps
        /// </summary>
        public static void Reload()
        {
            _fileIndex = new FileIndex("Texidx.mul", "Texmaps.mul", 0x4000, 10);
            _cache = new Bitmap[0x4000];
            _removed = new bool[0x4000];
            _patched.Clear();
        }

        public static int GetIdxLength()
        {
            return (int)(_fileIndex.IdxLength / 12);
        }

        /// <summary>
        /// Removes Texture <see cref="_removed"/>
        /// </summary>
        /// <param name="index"></param>
        public static void Remove(int index)
        {
            _removed[index] = true;
        }

        /// <summary>
        /// Replaces Texture
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void Replace(int index, Bitmap bmp)
        {
            _cache[index] = bmp;
            _removed[index] = false;
            if (_patched.Contains(index))
            {
                _patched.Remove(index);
            }
        }

        /// <summary>
        /// Tests if index is valid Texture
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TestTexture(int index)
        {
            if (_removed[index])
            {
                return false;
            }

            if (_cache[index] != null)
            {
                return true;
            }

            bool valid = _fileIndex.Valid(index, out int length, out int _, out bool _);

            return valid && (length != 0);
        }

        /// <summary>
        /// Returns Bitmap of Texture
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Bitmap GetTexture(int index)
        {
            return GetTexture(index, out bool _);
        }

        /// <summary>
        /// Returns Bitmap of Texture with verdata bool
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public static unsafe Bitmap GetTexture(int index, out bool patched)
        {
            if (_patched.Contains(index))
            {
                patched = (bool)_patched[index];
            }
            else
            {
                patched = false;
            }

            if (_removed[index])
            {
                return null;
            }

            if (_cache[index] != null)
            {
                return _cache[index];
            }

            Stream stream = _fileIndex.Seek(index, out int length, out int extra, out patched);
            if (stream == null)
            {
                return null;
            }

            if (length == 0)
            {
                return null;
            }

            if (patched)
            {
                _patched[index] = true;
            }

            int size = extra == 0 ? 64 : 128;

            var bmp = new Bitmap(size, size, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int max = size * size * 2;

            if (_streamBuffer == null || _streamBuffer.Length < max)
            {
                _streamBuffer = new byte[max];
            }

            stream.Read(_streamBuffer, 0, max);

            fixed (byte* data = _streamBuffer)
            {
                var binData = (ushort*)data;
                for (int y = 0; y < size; ++y, line += delta)
                {
                    ushort* cur = line;
                    ushort* end = cur + size;

                    while (cur < end)
                    {
                        *cur++ = (ushort)(*binData++ ^ 0x8000);
                    }
                }
            }

            bmp.UnlockBits(bd);

            stream.Close();

            if (!Files.CacheData)
            {
                return _cache[index] = bmp;
            }

            return bmp;
        }

        public static unsafe void Save(string path)
        {
            string idx = Path.Combine(path, "texidx.mul");
            string mul = Path.Combine(path, "texmaps.mul");
            _checkSums = new List<Checksums>();

            using (var fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var memidx = new MemoryStream();
                var memmul = new MemoryStream();
                using (BinaryWriter binidx = new BinaryWriter(memidx), binmul = new BinaryWriter(memmul))
                {
                    var sha = new SHA256Managed();
                    //StreamWriter Tex = new StreamWriter(new FileStream("d:/texlog.txt", FileMode.Create, FileAccess.ReadWrite));
                    for (int index = 0; index < GetIdxLength(); ++index)
                    {
                        if (_cache[index] == null)
                        {
                            _cache[index] = GetTexture(index);
                        }

                        Bitmap bmp = _cache[index];
                        if ((bmp == null) || (_removed[index]))
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0); // length
                            binidx.Write(-1); // extra
                        }
                        else
                        {
                            var ms = new MemoryStream();
                            bmp.Save(ms, ImageFormat.Bmp);
                            byte[] checksum = sha.ComputeHash(ms.ToArray());

                            if (compareSaveImages(checksum, out Checksums sum))
                            {
                                binidx.Write(sum.pos); //lookup
                                binidx.Write(sum.length);
                                binidx.Write(0);
                                //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, (int)sum.pos, (int)sum.length));
                                //Tex.WriteLine(System.String.Format("0x{0:X4} -> 0x{1:X4}", sum.index, index));
                                continue;
                            }

                            BitmapData bd = bmp.LockBits(
                                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly,
                                PixelFormat.Format16bppArgb1555);
                            var line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;

                            binidx.Write((int)binmul.BaseStream.Position); //lookup
                            var length = (int)binmul.BaseStream.Position;

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
                            binidx.Write((bmp.Width == 64 ? 0 : 1));
                            bmp.UnlockBits(bd);
                            var s = new Checksums {pos = start, length = length, checksum = checksum, index = index};
                            //Tex.WriteLine(System.String.Format("0x{0:X4} : 0x{1:X4} 0x{2:X4}", index, start, length));
                            _checkSums.Add(s);
                        }
                    }

                    memidx.WriteTo(fsidx);
                    memmul.WriteTo(fsmul);
                }
            }
        }

        private static bool compareSaveImages(byte[] newChecksum, out Checksums sum)
        {
            sum = new Checksums();
            for (int i = 0; i < _checkSums.Count; ++i)
            {
                byte[] cmp = _checkSums[i].checksum;
                if (((cmp == null) || (newChecksum == null)) || (cmp.Length != newChecksum.Length))
                {
                    return false;
                }

                bool valid = true;

                for (int j = 0; j < cmp.Length; ++j)
                {
                    if (cmp[j] == newChecksum[j])
                    {
                        continue;
                    }

                    valid = false;
                    break;
                }

                if (!valid)
                {
                    continue;
                }

                sum = _checkSums[i];
                return true;
            }

            return false;
        }
    }
}