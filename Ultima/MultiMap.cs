using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Ultima
{
    public sealed class MultiMap
    {
        private static byte[] _streamBuffer;

        /// <summary>
        /// Returns Bitmap
        /// </summary>
        /// <returns></returns>
        public static unsafe Bitmap GetMultiMap()
        {
            string path = Files.GetFilePath("Multimap.rle");
            if (path == null)
            {
                return null;
            }

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var bin = new BinaryReader(fs))
            {
                int x = 0;
                int width = bin.ReadInt32();
                int height = bin.ReadInt32();
                var multimap = new Bitmap(width, height, PixelFormat.Format16bppArgb1555);
                BitmapData bd = multimap.LockBits(
                    new Rectangle(0, 0, multimap.Width, multimap.Height), ImageLockMode.WriteOnly,
                    PixelFormat.Format16bppArgb1555);
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                ushort* cur = line;
                var len = (int)(bin.BaseStream.Length - bin.BaseStream.Position);
                if (_streamBuffer == null || _streamBuffer.Length < len)
                {
                    _streamBuffer = new byte[len];
                }

                bin.Read(_streamBuffer, 0, len);
                int j = 0;
                while (j != len)
                {
                    byte pixel = _streamBuffer[j++];
                    int count = (pixel & 0x7f);

                    // black or white color
                    ushort c = (pixel & 0x80) != 0 ? (ushort) 0x8000 : (ushort) 0xffff;

                    int i;
                    for (i = 0; i < count; ++i)
                    {
                        cur[x++] = c;

                        if (x < width)
                        {
                            continue;
                        }

                        cur += delta;
                        x = 0;
                    }
                }

                multimap.UnlockBits(bd);
                return multimap;
            }
        }

        /// <summary>
        /// Saves Bitmap to rle Format
        /// </summary>
        /// <param name="image"></param>
        /// <param name="bin"></param>
        public static unsafe void SaveMultiMap(Bitmap image, BinaryWriter bin)
        {
            bin.Write(2560); // width
            bin.Write(2048); // height

            byte data = 0;
            byte mask = 0x0;

            BitmapData bd = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);

            var line = (ushort*)bd.Scan0;
            int delta = bd.Stride >> 1;
            ushort* cur = line;

            ushort curColor = cur[0];

            for (int y = 0; y < image.Height; ++y, line += delta)
            {
                cur = line;

                for (int x = 0; x < image.Width; ++x)
                {
                    ushort c = cur[x];

                    if (c == curColor)
                    {
                        ++data;
                        if (data != 0x7f)
                        {
                            continue;
                        }

                        mask = curColor == 0xffff ? (byte) 0x0 : (byte) 0x80;

                        data |= mask;
                        bin.Write(data);
                        data = 0;
                    }
                    else if (data > 0)
                    {
                        mask = curColor == 0xffff ? (byte) 0x0 : (byte) 0x80;

                        data |= mask;
                        bin.Write(data);
                        curColor = c;
                        data = 1;
                    }
                    else
                    {
                        curColor = c;
                        data = 1;
                    }
                }
            }

            if (data > 0)
            {
                mask = curColor == 0xffff ? (byte) 0x0 : (byte) 0x80;

                data |= mask;
                bin.Write(data);
            }

            image.UnlockBits(bd);
        }

        /// <summary>
        /// reads facet0*.mul into Bitmap
        /// </summary>
        /// <param name="id">facet id</param>
        /// <returns>Bitmap</returns>
        public static unsafe Bitmap GetFacetImage(int id)
        {
            string path = Files.GetFilePath($"facet0{id}.mul");
            if (path == null)
            {
                return null;
            }

            Bitmap bmp;
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                int width = reader.ReadInt16();
                int height = reader.ReadInt16();

                bmp = new Bitmap(width, height);
                BitmapData bd = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;

                for (int y = 0; y < height; y++, line += delta)
                {
                    int colorsCount = reader.ReadInt32() / 3;
                    ushort* endline = line + delta;
                    ushort* cur = line;
                    for (int c = 0; c < colorsCount; c++)
                    {
                        byte count = reader.ReadByte();
                        short color = reader.ReadInt16();
                        ushort* end = cur + count;
                        while (cur < end)
                        {
                            if (cur > endline)
                            {
                                break;
                            }

                            *cur++ = (ushort)(color ^ 0x8000);
                        }
                    }
                }
                bmp.UnlockBits(bd);
            }

            return bmp;
        }

        /// <summary>
        /// Stores Image into facet.mul format
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sourceBitmap"></param>
        public static unsafe void SaveFacetImage(string path, Bitmap sourceBitmap)
        {
            int width = sourceBitmap.Width;
            int height = sourceBitmap.Height;

            using (
                var writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)))
            {
                writer.Write((short)width);
                writer.Write((short)height);
                BitmapData bd = sourceBitmap.LockBits(
                    new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format16bppArgb1555);
                var line = (ushort*)bd.Scan0;
                int delta = bd.Stride >> 1;
                for (int y = 0; y < height; y++, line += delta)
                {
                    long pos = writer.BaseStream.Position;
                    writer.Write(0);//bytes count for current line

                    int colorsAtLine = 0;
                    int colorsCount = 0;
                    int x = 0;

                    while (x < width)
                    {
                        ushort hue = line[x];
                        while (x < width && colorsCount < byte.MaxValue && hue == line[x])
                        {
                            ++colorsCount;
                            ++x;
                        }
                        writer.Write((byte)colorsCount);
                        writer.Write((ushort)(hue ^ 0x8000));

                        colorsAtLine++;
                        colorsCount = 0;
                    }
                    long currpos = writer.BaseStream.Position;
                    writer.BaseStream.Seek(pos, SeekOrigin.Begin);
                    writer.Write(colorsAtLine * 3); //byte count
                    writer.BaseStream.Seek(currpos, SeekOrigin.Begin);
                }
            }
        }
    }
}
