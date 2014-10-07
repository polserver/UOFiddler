using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima;

namespace ComparePlugin
{
    class SecondGump
    {
        private static SecondFileIndex m_FileIndex;

        private static Bitmap[] m_Cache = new Bitmap[0x10000];
        private static byte[] m_StreamBuffer;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            m_FileIndex = new SecondFileIndex(idxPath, mulPath, 0x10000);
            m_Cache = new Bitmap[0x10000];
        }

        /// <summary>
        /// Tests if index is definied
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool IsValidIndex(int index)
        {
            if (m_FileIndex == null)
                return false;
            if (m_Cache[index] != null)
                return true;
            int length, extra;
            if (!m_FileIndex.Valid(index, out length, out extra))
                return false;
            if (extra == -1)
                return false;
            int width = (extra >> 16) & 0xFFFF;
            int height = extra & 0xFFFF;

            if (width <= 0 || height <= 0)
                return false;

            return true;
        }

        public static byte[] GetRawGump(int index, out int width, out int height)
        {
            width = -1;
            height = -1;
            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);
            if (stream == null)
                return null;
            if (extra == -1)
                return null;
            width = (extra >> 16) & 0xFFFF;
            height = extra & 0xFFFF;
            if (width <= 0 || height <= 0)
                return null;
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// Returns Bitmap of index and if verdata patched
        /// </summary>
        /// <param name="index"></param>
        /// <param name="patched"></param>
        /// <returns></returns>
        public unsafe static Bitmap GetGump(int index)
        {
            if (m_Cache[index] != null)
                return m_Cache[index];
            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);
            if (stream == null)
                return null;
            if (extra == -1)
                return null;

            int width = (extra >> 16) & 0xFFFF;
            int height = extra & 0xFFFF;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            if (m_StreamBuffer == null || m_StreamBuffer.Length < length)
                m_StreamBuffer = new byte[length];
            stream.Read(m_StreamBuffer, 0, length);

            fixed (byte* data = m_StreamBuffer)
            {
                int* lookup = (int*)data;
                ushort* dat = (ushort*)data;

                ushort* line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                int count = 0;
                for (int y = 0; y < height; ++y, line += delta)
                {
                    count = (*lookup++ * 2);

                    ushort* cur = line;
                    ushort* end = line + bd.Width;

                    while (cur < end)
                    {
                        ushort color = dat[count++];
                        ushort* next = cur + dat[count++];

                        if (color == 0)
                            cur = next;
                        else
                        {
                            color ^= 0x8000;
                            while (cur < next)
                                *cur++ = color;
                        }
                    }
                }
            }

            bmp.UnlockBits(bd);

            if (Files.CacheData)
                return m_Cache[index] = bmp;
            else
                return bmp;
        }

    }
}
