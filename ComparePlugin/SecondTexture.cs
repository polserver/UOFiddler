using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima;

namespace ComparePlugin
{
    class SecondTexture
    {
        private static SecondFileIndex m_FileIndex;
        private static Bitmap[] m_Cache;
        private static byte[] m_StreamBuffer;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            m_FileIndex = new SecondFileIndex(idxPath, mulPath, 0x4000);
            m_Cache = new Bitmap[0x4000];
        }

        public static int GetIdxLength()
        {
            return (int)(m_FileIndex.IdxLength / 12);
        }

        public static bool IsValidTexture(int index)
        {
            index &= 0x3FFF;
            if (m_Cache[index] != null)
                return true;

            int length, extra;
            bool valid = m_FileIndex.Valid(index, out length, out extra);
            if ((!valid) || (length == 0))
                return false;
            return true;
        }

        public static Bitmap GetTexture(int index)
        {
            index &= 0x3FFF;

            if (m_Cache[index] != null)
                return m_Cache[index];

            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);
            if (stream == null)
                return null;

            if (Files.CacheData)
                return m_Cache[index] = LoadTexture(stream, extra, length);
            else
                return LoadTexture(stream, extra, length);
        }

        public static byte[] GetRawTexture(int index)
        {
            index &= 0x3FFF;

            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);
            if (stream == null)
                return null;
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        private unsafe static Bitmap LoadTexture(Stream stream, int extra, int length)
        {
            int size = extra == 0 ? 64 : 128;

            Bitmap bmp = new Bitmap(size, size, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            ushort* line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;

            int max = size * size * 2;

            if (m_StreamBuffer == null || m_StreamBuffer.Length < max)
                m_StreamBuffer = new byte[max];
            stream.Read(m_StreamBuffer, 0, max);

            fixed (byte* data = m_StreamBuffer)
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

            return bmp;
        }
    }
}

