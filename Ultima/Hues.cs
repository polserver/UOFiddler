using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Ultima.Helpers;

namespace Ultima
{
    public static class Hues
    {
        private static int[] _header;

        public static Hue[] List { get; private set; }

        static Hues()
        {
            Initialize();
        }

        /// <summary>
        /// Reads hues.mul and fills <see cref="List"/>
        /// </summary>
        public static void Initialize()
        {
            string path = Files.GetFilePath("hues.mul");
            int index = 0;

            const int maxHueCount = 3000;
            List = new Hue[maxHueCount];

            if (path != null)
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int blockCount = (int)fs.Length / 708;

                    if (blockCount > 375)
                    {
                        blockCount = 375;
                    }

                    _header = new int[blockCount];
                    int structSize = Marshal.SizeOf(typeof(HueDataMul));
                    var buffer = new byte[blockCount * (4 + (8 * structSize))];
                    GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        fs.Read(buffer, 0, buffer.Length);
                        long currentPos = 0;

                        for (int i = 0; i < blockCount; ++i)
                        {
                            var ptrHeader = new IntPtr(gc.AddrOfPinnedObject() + currentPos);
                            currentPos += 4;
                            _header[i] = (int)Marshal.PtrToStructure(ptrHeader, typeof(int));

                            for (int j = 0; j < 8; ++j, ++index)
                            {
                                var ptr = new IntPtr(gc.AddrOfPinnedObject() + currentPos);
                                currentPos += structSize;
                                var cur = (HueDataMul)Marshal.PtrToStructure(ptr, typeof(HueDataMul));
                                List[index] = new Hue(index, cur);
                            }
                        }
                    }
                    finally
                    {
                        gc.Free();
                    }
                }
            }

            for (; index < List.Length; ++index)
            {
                List[index] = new Hue(index);
            }
        }

        public static void Save(string path)
        {
            string mul = Path.Combine(path, "hues.mul");
            using (var fsMul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            using (var binMul = new BinaryWriter(fsMul))
            {
                int index = 0;
                foreach (var blockIdx in _header)
                {
                    binMul.Write(blockIdx);
                    for (int j = 0; j < 8; ++j, ++index)
                    {
                        for (int colorIndex = 0; colorIndex < 32; ++colorIndex)
                        {
                            binMul.Write(List[index].Colors[colorIndex]);
                        }

                        binMul.Write(List[index].TableStart);
                        binMul.Write(List[index].TableEnd);

                        var nameBuffer = new byte[20];
                        if (List[index].Name != null)
                        {
                            byte[] bytes = Encoding.ASCII.GetBytes(List[index].Name);
                            if (bytes.Length > 20)
                            {
                                Array.Resize(ref bytes, 20);
                            }

                            bytes.CopyTo(nameBuffer, 0);
                        }

                        binMul.Write(nameBuffer);
                    }
                }
            }
        }

        /// <summary>
        /// Exports list of all hue names and id (as hex)
        /// </summary>
        /// <param name="fileName">Output file name</param>
        public static void ExportHueList(string fileName)
        {
            var sb = new StringBuilder(90_0000);

            foreach (var hue in List)
            {
                sb.Append("0x").AppendFormat("{0:X}", hue.Index).Append(' ').AppendLine(hue.Name);
            }

            File.WriteAllText(fileName, sb.ToString());
        }

        /// <summary>
        /// Returns <see cref="Hue"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Hue GetHue(int index)
        {
            index &= 0x3FFF;

            if (index >= 0 && index < 3000)
            {
                return List[index];
            }

            return List[0];
        }

        /// <summary>
        /// Converts RGB value to Hue color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static ushort ColorToHue(Color color)
        {
            const double scale = 31.0 / 255;

            ushort origRed = color.R;
            var newRed = (ushort)(origRed * scale);
            if (newRed == 0 && origRed != 0)
            {
                newRed = 1;
            }

            ushort origGreen = color.G;
            var newGreen = (ushort)(origGreen * scale);
            if (newGreen == 0 && origGreen != 0)
            {
                newGreen = 1;
            }

            ushort origBlue = color.B;
            var newBlue = (ushort)(origBlue * scale);
            if (newBlue == 0 && origBlue != 0)
            {
                newBlue = 1;
            }

            return (ushort)((newRed << 10) | (newGreen << 5) | newBlue);
        }

        public static int HueToColorR(ushort hue)
        {
            return ((hue & 0x7c00) >> 10) * (255 / 31);
        }

        public static int HueToColorG(ushort hue)
        {
            return ((hue & 0x3e0) >> 5) * (255 / 31);
        }

        public static int HueToColorB(ushort hue)
        {
            return (hue & 0x1f) * (255 / 31);
        }

        public static unsafe void ApplyTo(Bitmap bmp, ushort[] colors, bool onlyHueGrayPixels)
        {
            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format16bppArgb1555);

            int stride = bd.Stride >> 1;
            int width = bd.Width;
            int height = bd.Height;
            int delta = stride - width;

            ushort* pBuffer = (ushort*)bd.Scan0;
            ushort* pLineEnd = pBuffer + width;
            ushort* pImageEnd = pBuffer + (stride * height);

            if (onlyHueGrayPixels)
            {
                while (pBuffer < pImageEnd)
                {
                    while (pBuffer < pLineEnd)
                    {
                        int c = *pBuffer;
                        if (c != 0)
                        {
                            int r = (c >> 10) & 0x1F;
                            int g = (c >> 5) & 0x1F;
                            int b = c & 0x1F;
                            if (r == g && r == b)
                            {
                                *pBuffer = (ushort)(colors[(c >> 10) & 0x1F] | 0x8000);
                            }
                        }
                        ++pBuffer;
                    }

                    pBuffer += delta;
                    pLineEnd += stride;
                }
            }
            else
            {
                while (pBuffer < pImageEnd)
                {
                    while (pBuffer < pLineEnd)
                    {
                        if (*pBuffer != 0)
                        {
                            *pBuffer = (ushort)(colors[(*pBuffer >> 10) & 0x1F] | 0x8000);
                        }

                        ++pBuffer;
                    }

                    pBuffer += delta;
                    pLineEnd += stride;
                }
            }

            bmp.UnlockBits(bd);
        }
    }

    public sealed class Hue
    {
        public int Index { get; }
        public ushort[] Colors { get; }
        public string Name { get; set; }
        public ushort TableStart { get; set; }
        public ushort TableEnd { get; set; }

        public Hue(int index)
        {
            Name = "Null";
            Index = index;
            Colors = new ushort[32];
            TableStart = 0;
            TableEnd = 0;
        }

        public Color GetColor(int index)
        {
            return HueToColor(Colors[index]);
        }

        /// <summary>
        /// Converts Hue color to RGB color
        /// </summary>
        /// <param name="hue"></param>
        private static Color HueToColor(ushort hue)
        {
            const int scale = 255 / 31;

            return Color.FromArgb(
                ((hue & 0x7c00) >> 10) * scale,
                ((hue & 0x3e0) >> 5) * scale,
                (hue & 0x1f) * scale);
        }

        private static readonly byte[] _stringBuffer = new byte[20];

        public Hue(int index, BinaryReader bin)
        {
            Index = index;
            Colors = new ushort[32];

            byte[] buffer = bin.ReadBytes(88);
            unsafe
            {
                fixed (byte* bufferPtr = buffer)
                {
                    var buf = (ushort*)bufferPtr;
                    for (int i = 0; i < 32; ++i)
                    {
                        Colors[i] = *buf++;
                    }

                    TableStart = *buf++;
                    TableEnd = *buf++;

                    var stringBuffer = (byte*)buf;
                    int count;
                    for (count = 0; count < 20 && *stringBuffer != 0; ++count)
                    {
                        _stringBuffer[count] = *stringBuffer++;
                    }

                    Name = Encoding.ASCII.GetString(_stringBuffer, 0, count);
                    Name = Name.Replace("\n", " ");
                }
            }
        }

        public Hue(int index, HueDataMul mulStruct)
        {
            Index = index;
            Colors = new ushort[32];
            for (int i = 0; i < 32; ++i)
            {
                Colors[i] = mulStruct.colors[i];
            }

            TableStart = mulStruct.tableStart;
            TableEnd = mulStruct.tableEnd;

            Name = TileDataHelpers.ReadNameString(mulStruct.name, 20);
            Name = Name.Replace("\n", " ");
        }

        /// <summary>
        /// Applies Hue to Bitmap
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="onlyHueGrayPixels"></param>
        public unsafe void ApplyTo(Bitmap bmp, bool onlyHueGrayPixels)
        {
            BitmapData bd = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format16bppArgb1555);

            int stride = bd.Stride >> 1;
            int width = bd.Width;
            int height = bd.Height;
            int delta = stride - width;

            var pBuffer = (ushort*)bd.Scan0;
            ushort* pLineEnd = pBuffer + width;
            ushort* pImageEnd = pBuffer + (stride * height);

            if (onlyHueGrayPixels)
            {
                while (pBuffer < pImageEnd)
                {
                    while (pBuffer < pLineEnd)
                    {
                        int c = *pBuffer;
                        if (c != 0)
                        {
                            int r = (c >> 10) & 0x1F;
                            int g = (c >> 5) & 0x1F;
                            int b = c & 0x1F;
                            if (r == g && r == b)
                            {
                                *pBuffer = (ushort)(Colors[(c >> 10) & 0x1F] | 0x8000);
                            }
                        }
                        ++pBuffer;
                    }

                    pBuffer += delta;
                    pLineEnd += stride;
                }
            }
            else
            {
                while (pBuffer < pImageEnd)
                {
                    while (pBuffer < pLineEnd)
                    {
                        if (*pBuffer != 0)
                        {
                            *pBuffer = (ushort)(Colors[(*pBuffer >> 10) & 0x1F] | 0x8000);
                        }

                        ++pBuffer;
                    }

                    pBuffer += delta;
                    pLineEnd += stride;
                }
            }

            bmp.UnlockBits(bd);
        }

        public void Export(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.WriteLine(Name);
                tex.WriteLine(TableStart.ToString());
                tex.WriteLine(TableEnd.ToString());

                foreach (var colorValue in Colors)
                {
                    tex.WriteLine(colorValue.ToString());
                }
            }
        }

        public void Import(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (var sr = new StreamReader(fileName))
            {
                int i = -3;

                while (sr.ReadLine() is { } line)
                {
                    line = line.Trim();

                    try
                    {
                        if (i >= Colors.Length)
                        {
                            break;
                        }

                        switch (i)
                        {
                            case -3:
                                Name = line;
                                break;
                            case -2:
                                TableStart = ushort.Parse(line);
                                break;
                            case -1:
                                TableEnd = ushort.Parse(line);
                                break;
                            default:
                                Colors[i] = ushort.Parse(line);
                                break;
                        }

                        ++i;
                    }
                    catch
                    {
                        // TODO: ignored?
                        // ignored
                    }
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct HueDataMul
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly ushort[] colors;
        public readonly ushort tableStart;
        public readonly ushort tableEnd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] name;
    }
}