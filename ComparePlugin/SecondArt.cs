using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Ultima;

namespace ComparePlugin
{
    class SecondArt
    {
        private static SecondFileIndex m_FileIndex;
        private static Bitmap[] m_Cache;

        private static byte[] m_StreamBuffer;
        private static byte[] Validbuffer;

        public static void SetFileIndex(string idxPath, string mulPath)
        {
            m_FileIndex = new SecondFileIndex(idxPath, mulPath, 0x14000);
            m_Cache = new Bitmap[0x14000];
        }

        public static int GetMaxItemID()
        {
            if (GetIdxLength() == 0xC000)
                return 0x7FFF;

            if (GetIdxLength() == 0x13FDC)
                return 0xFFDB;

            return 0x3FFF;
        }

        public static ushort GetLegalItemID(int itemID)
        {
            if (itemID < 0)
                return 0;

            int max = GetMaxItemID();
            if (itemID > max)
                return 0;

            return (ushort)itemID;
        }

        public static int GetIdxLength()
        {
            return (int)(m_FileIndex.IdxLength / 12);
        }

        public static unsafe bool IsValidStatic(int index)
        {
            index = SecondArt.GetLegalItemID(index);
            index += 0x4000;
            
            if (m_Cache[index] != null)
                return true;

            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);

            if (stream == null)
                return false;

            if (Validbuffer == null)
                Validbuffer = new byte[4];
            stream.Seek(4, SeekOrigin.Current);
            stream.Read(Validbuffer, 0, 4);
            fixed (byte* b = Validbuffer)
            {
                short* dat = (short*)b;
                if (*dat++ <= 0 || *dat <= 0)
                    return false;
                return true;
            }
        }

        public static Bitmap GetStatic(int index)
        {
            index = SecondArt.GetLegalItemID(index);
            index += 0x4000;
            
            if (m_Cache[index] != null)
                return m_Cache[index];

            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);
            if (stream == null)
                return null;

            if (Files.CacheData)
                return m_Cache[index] = LoadStatic(stream, length);
            else
                return LoadStatic(stream, length);
        }

        public static byte[] GetRawStatic(int index)
        {
            index = SecondArt.GetLegalItemID(index);
            index += 0x4000;

            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);
            if (stream == null)
                return null;
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        private static unsafe Bitmap LoadStatic(Stream stream, int length)
        {
            Bitmap bmp;
            if (m_StreamBuffer == null || m_StreamBuffer.Length < length)
                m_StreamBuffer = new byte[length];
            stream.Read(m_StreamBuffer, 0, length);
            stream.Close();

            fixed (byte* data = m_StreamBuffer)
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
                    lookups[i] = (int)(start + (bindata[count++]));

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

        public static bool IsValidLand(int index)
        {
            index &= 0x3FFF;
            if (m_Cache[index] != null)
                return true;

            int length, extra;
            return m_FileIndex.Valid(index, out length, out extra);
        }

        public static Bitmap GetLand(int index)
        {
            index &= 0x3FFF;

            if (m_Cache[index] != null)
                return m_Cache[index];

            int length, extra;
            Stream stream = m_FileIndex.Seek(index, out length, out extra);
            if (stream == null)
                return null;

            if (Files.CacheData)
                return m_Cache[index] = LoadLand(stream, length);
            else
                return LoadLand(stream, length);
        }

        public static byte[] GetRawLand(int index)
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

        private static unsafe Bitmap LoadLand(Stream stream, int length)
        {
            Bitmap bmp = new Bitmap(44, 44, PixelFormat.Format16bppArgb1555);
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, 44, 44), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
            if (m_StreamBuffer == null || m_StreamBuffer.Length < length)
                m_StreamBuffer = new byte[length];
            stream.Read(m_StreamBuffer, 0, length);
            stream.Close();
            fixed (byte* bindata = m_StreamBuffer)
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
    }
}
