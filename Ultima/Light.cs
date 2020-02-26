using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public sealed class Light
    {
        private static FileIndex _mFileIndex = new FileIndex("lightidx.mul", "light.mul", 100, -1);
        private static Bitmap[] _mCache = new Bitmap[100];
        private static bool[] _mRemoved = new bool[100];
        private static byte[] _mStreamBuffer;

        /// <summary>
        /// ReReads light.mul
        /// </summary>
        public static void Reload()
        {
            _mFileIndex = new FileIndex("lightidx.mul", "light.mul", 100, -1);
            _mCache = new Bitmap[100];
            _mRemoved = new bool[100];
        }

        /// <summary>
        /// Gets count of definied lights
        /// </summary>
        /// <returns></returns>
        public static int GetCount()
        {
            string idxPath = Files.GetFilePath("lightidx.mul");
            if (idxPath == null)
                return 0;
            using (FileStream index = new FileStream(idxPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (int)(index.Length / 12);
            }
        }

        /// <summary>
        /// Tests if given index is valid
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool TestLight(int index)
        {
            if (_mRemoved[index])
                return false;
            if (_mCache[index] != null)
                return true;

            int length, extra;
            bool patched;

            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);

            if (stream == null)
                return false;
            stream.Close();
            int width = (extra & 0xFFFF);
            int height = ((extra >> 16) & 0xFFFF);
            if ((width > 0) && (height > 0))
                return true;

            return false;
        }

        /// <summary>
        /// Removes Light <see cref="_mRemoved"/>
        /// </summary>
        /// <param name="index"></param>
        public static void Remove(int index)
        {
            _mRemoved[index] = true;
        }

        /// <summary>
        /// Replaces Light
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bmp"></param>
        public static void Replace(int index, Bitmap bmp)
        {
            _mCache[index] = bmp;
            _mRemoved[index] = false;
        }

        public unsafe static byte[] GetRawLight(int index, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (_mRemoved[index])
                return null;
            int length, extra;
            bool patched;

            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);

            if (stream == null)
                return null;

            width = (extra & 0xFFFF);
            height = ((extra >> 16) & 0xFFFF);
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            stream.Close();
            return buffer;
        }
        /// <summary>
        /// Returns Bitmap of given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public unsafe static Bitmap GetLight(int index)
        {
            if (_mRemoved[index])
                return null;
            if (_mCache[index] != null)
                return _mCache[index];

            int length, extra;
            bool patched;

            Stream stream = _mFileIndex.Seek(index, out length, out extra, out patched);

            if (stream == null)
                return null;

            int width = (extra & 0xFFFF);
            int height = ((extra >> 16) & 0xFFFF);

            if (_mStreamBuffer == null || _mStreamBuffer.Length < length)
                _mStreamBuffer = new byte[length];
            stream.Read(_mStreamBuffer, 0, length);

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);

            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            fixed (byte* data = _mStreamBuffer)
            {
                sbyte* bindat = (sbyte*)data;
                for (int y = 0; y < height; ++y, line += delta)
                {
                    ushort* cur = line;
                    ushort* end = cur + width;

                    while (cur < end)
                    {
                        sbyte value = *bindat++;
                        *cur++ = (ushort)(((0x1f + value) << 10) + ((0x1F + value) << 5) + (0x1F + value));
                    }
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
            string idx = Path.Combine(path, "lightidx.mul");
            string mul = Path.Combine(path, "light.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int index = 0; index < _mCache.Length; index++)
                    {
                        if (_mCache[index] == null)
                            _mCache[index] = GetLight(index);
                        Bitmap bmp = _mCache[index];

                        if ((bmp == null) || (_mRemoved[index]))
                        {
                            binidx.Write(-1); // lookup
                            binidx.Write(0); // length
                            binidx.Write(0); // extra
                        }
                        else
                        {
                            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                            ushort* line = (ushort*)bd.Scan0;
                            int delta = bd.Stride >> 1;

                            binidx.Write((int)fsmul.Position); //lookup
                            int length = (int)fsmul.Position;

                            for (int y = 0; y < bmp.Height; ++y, line += delta)
                            {
                                ushort* cur = line;
                                ushort* end = cur + bmp.Width;
                                while (cur < end)
                                {
                                    ushort ccur = *cur++;
                                    sbyte value = 0;
                                    
                                    if (ccur > 0) // Zero should stay zero cause it means transparence
                                        value = (sbyte)(((ccur >> 10) & 0xffff) - 0x1f);
                                    if (value > 0) // wtf? but it works...
                                        --value;
                                    binmul.Write(value);
                                }
                            }
                            length = (int)fsmul.Position - length;
                            binidx.Write(length);
                            binidx.Write((bmp.Height << 16) + bmp.Width);
                            bmp.UnlockBits(bd);
                        }
                    }
                }
            }
        }
    }
}
