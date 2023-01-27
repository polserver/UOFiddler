/***************************************************************************
 *
 * $Author: Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MapReplaceTilesForm : Form
    {
        private readonly Map _map;
        private List<ModArea> _toReplace;

        public MapReplaceTilesForm(Map map)
        {
            InitializeComponent();

            _map = map;
        }

        private void OnReplace(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                button2.Enabled = false;

                richTextBox1.Text = string.Empty;
                richTextBox1.AppendText("Replacement start...\r\n");

                string file = textBox1.Text;
                if (string.IsNullOrEmpty(file))
                {
                    richTextBox1.AppendText("Please specify XML file with replacements.\r\n");
                    return;
                }

                if (!File.Exists(file))
                {
                    richTextBox1.AppendText("Specified file does not exist.\r\n");
                    return;
                }

                if (!LoadFile(file))
                {
                    richTextBox1.AppendText("Could not load replacement file.\r\n");
                    return;
                }

                string path = Options.OutputPath;

                richTextBox1.AppendText("Replacing map tiles...\r\n");
                ReplaceMap(path, _map.FileIndex, _map.Width, _map.Height);

                richTextBox1.AppendText("Replacing static tiles...\r\n");
                ReplaceStatic(path, _map.FileIndex, _map.Width, _map.Height);

                richTextBox1.AppendText("Done.");

                MessageBox.Show($"Files saved to {Options.OutputPath}", "Saved", MessageBoxButtons.OK,
                    MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            finally
            {
                button2.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }

        internal class ConvertLandClass
        {
            public ushort LandFrom { get; set; }
            public ushort LandTo { get; set; }
            public byte ZMin { get; set; }
            public byte ZMax { get; set; }
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog
                   {
                       Multiselect = false,
                       Title = "Choose xml file to open",
                       CheckFileExists = true,
                       Filter = "xml files (*.xml)|*.xml"
                   })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (!File.Exists(dialog.FileName))
                {
                    return;
                }

                textBox1.Text = dialog.FileName;
            }
        }

        private bool LoadFile(string file)
        {
            _toReplace = new List<ModArea>();
            XmlDocument dom = new XmlDocument();
            dom.Load(file);
            try
            {
                XmlNode xRoot = dom.SelectSingleNode("MapReplace");

                foreach (XmlNode xNode in xRoot.SelectNodes("Area"))
                {
                    if (xNode.NodeType == XmlNodeType.Comment)
                    {
                        continue;
                    }

                    Utils.ConvertStringToInt(xNode.Attributes["StartX"].InnerText, out int sx);
                    Utils.ConvertStringToInt(xNode.Attributes["StartY"].InnerText, out int sy);
                    Utils.ConvertStringToInt(xNode.Attributes["EndX"].InnerText, out int ex);
                    Utils.ConvertStringToInt(xNode.Attributes["EndY"].InnerText, out int ey);

                    Dictionary<ushort, ushort> convertDictLand = new Dictionary<ushort, ushort>();
                    Dictionary<ushort, ushort> convertDictStatic = new Dictionary<ushort, ushort>();

                    foreach (XmlNode xArea in xNode.ChildNodes)
                    {
                        if (xArea.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }

                        ushort convertFrom, convertTo;
                        if (Utils.ConvertStringToInt(xArea.Attributes["from"].InnerText, out int temp))
                        {
                            convertFrom = (ushort)temp;
                        }
                        else
                        {
                            continue;
                        }

                        if (Utils.ConvertStringToInt(xArea.Attributes["to"].InnerText, out temp))
                        {
                            convertTo = (ushort)temp;
                        }
                        else
                        {
                            continue;
                        }

                        switch (xArea.Name.ToLower())
                        {
                            case "static":
                                convertDictStatic.Add(convertFrom, convertTo);
                                break;
                            case "landtile":
                                convertDictLand.Add(convertFrom, convertTo);
                                break;
                        }
                    }

                    _toReplace.Add(new ModArea(_map, sx, sy, ex, ey, convertDictLand, convertDictStatic));
                }
            }
            catch (Exception ex)
            {
                richTextBox1.AppendText($"Exception while loading replacement file:\r\n{ex.Message}\r\n");
                return false;
            }

            return true;
        }

        private void ReplaceMap(string path, int map, int width, int height)
        {
            string mapPath = Files.GetFilePath($"map{map}.mul");
            BinaryReader mMapReader;
            if (mapPath != null)
            {
                var mMap = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                mMapReader = new BinaryReader(mMap);
            }
            else
            {
                return;
            }

            int blockX = width >> 3;
            int blockY = height >> 3;

            string mul = Path.Combine(path, $"map{map}.mul");
            using (FileStream mulStream = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(mulStream))
                {
                    for (int x = 0; x < blockX; ++x)
                    {
                        for (int y = 0; y < blockY; ++y)
                        {
                            try
                            {
                                mMapReader.BaseStream.Seek(((x * blockY) + y) * 196, SeekOrigin.Begin);
                                int header = mMapReader.ReadInt32();
                                binaryWriter.Write(header);
                                for (int i = 0; i < 64; ++i)
                                {
                                    ushort tileId = mMapReader.ReadUInt16();
                                    int temp = ModArea.IsLandReplace(_toReplace, tileId, x, y, i);
                                    sbyte z = mMapReader.ReadSByte();

                                    if (tileId >= 0x4000)
                                    {
                                        tileId = 0;
                                    }
                                    else if (temp != -1)
                                    {
                                        tileId = (ushort)temp;
                                    }

                                    binaryWriter.Write(tileId);
                                    binaryWriter.Write(z);
                                }
                            }
                            catch // fill rest
                            {
                                binaryWriter.BaseStream.Seek(((x * blockY) + y) * 196, SeekOrigin.Begin);
                                for (; x < blockX; ++x)
                                {
                                    for (; y < blockY; ++y)
                                    {
                                        binaryWriter.Write(0);
                                        for (int i = 0; i < 64; ++i)
                                        {
                                            binaryWriter.Write((short)0);
                                            binaryWriter.Write((sbyte)0);
                                        }
                                    }
                                    y = 0;
                                }
                                return;
                            }
                        }
                    }
                }
            }
            mMapReader.Close();
        }

        private void ReplaceStatic(string path, int map, int width, int height)
        {
            string indexPath = Files.GetFilePath($"staidx{map}.mul");
            BinaryReader mIndexReader;
            if (indexPath != null)
            {
                var mIndex = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                mIndexReader = new BinaryReader(mIndex);
            }
            else
            {
                return;
            }

            string staticsPath = Files.GetFilePath($"statics{map}.mul");

            FileStream mStatics;
            BinaryReader staticsReader;
            if (staticsPath != null)
            {
                mStatics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                staticsReader = new BinaryReader(mStatics);
            }
            else
            {
                return;
            }

            int blockX = width >> 3;
            int blockY = height >> 3;

            string idx = Path.Combine(path, $"staidx{map}.mul");
            string mul = Path.Combine(path, $"statics{map}.mul");
            using (FileStream indexFileStream = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              mulFileStream = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter indexBinaryWriter = new BinaryWriter(indexFileStream),
                                    mulBinaryWriter = new BinaryWriter(mulFileStream))
                {
                    for (int x = 0; x < blockX; ++x)
                    {
                        for (int y = 0; y < blockY; ++y)
                        {
                            try
                            {
                                mIndexReader.BaseStream.Seek(((x * blockY) + y) * 12, SeekOrigin.Begin);
                                int lookup = mIndexReader.ReadInt32();
                                int length = mIndexReader.ReadInt32();
                                int extra = mIndexReader.ReadInt32();

                                if (lookup < 0 || length <= 0)
                                {
                                    indexBinaryWriter.Write(-1); // lookup
                                    indexBinaryWriter.Write(-1); // length
                                    indexBinaryWriter.Write(-1); // extra
                                }
                                else
                                {
                                    mStatics.Seek(lookup, SeekOrigin.Begin);

                                    int fsMulLength = (int)mulFileStream.Position;
                                    int count = length / 7;

                                    bool firstItem = true;
                                    for (int i = 0; i < count; ++i)
                                    {
                                        ushort graphic = staticsReader.ReadUInt16();
                                        byte sx = staticsReader.ReadByte();
                                        byte sy = staticsReader.ReadByte();
                                        sbyte sz = staticsReader.ReadSByte();
                                        short sHue = staticsReader.ReadInt16();
                                        int temp = ModArea.IsStaticReplace(_toReplace, graphic, x, y, i);

                                        if (graphic > Art.GetMaxItemID())
                                        {
                                            continue;
                                        }

                                        if (sHue < 0)
                                        {
                                            sHue = 0;
                                        }

                                        if (firstItem)
                                        {
                                            indexBinaryWriter.Write((int)mulFileStream.Position); //lookup
                                            firstItem = false;
                                        }

                                        if (temp != -1)
                                        {
                                            graphic = (ushort)temp;
                                        }

                                        mulBinaryWriter.Write(graphic);
                                        mulBinaryWriter.Write(sx);
                                        mulBinaryWriter.Write(sy);
                                        mulBinaryWriter.Write(sz);
                                        mulBinaryWriter.Write(sHue);
                                    }

                                    fsMulLength = (int)mulFileStream.Position - fsMulLength;

                                    if (fsMulLength > 0)
                                    {
                                        indexBinaryWriter.Write(fsMulLength); //length

                                        if (extra == -1)
                                        {
                                            extra = 0;
                                        }

                                        indexBinaryWriter.Write(extra); //extra
                                    }
                                    else
                                    {
                                        indexBinaryWriter.Write(-1); //lookup
                                        indexBinaryWriter.Write(-1); //length
                                        indexBinaryWriter.Write(-1); //extra
                                    }
                                }
                            }
                            catch // fill the rest
                            {
                                indexBinaryWriter.BaseStream.Seek(((x * blockY) + y) * 12, SeekOrigin.Begin);

                                for (; x < blockX; ++x)
                                {
                                    for (; y < blockY; ++y)
                                    {
                                        indexBinaryWriter.Write(-1); //lookup
                                        indexBinaryWriter.Write(-1); //length
                                        indexBinaryWriter.Write(-1); //extra
                                    }

                                    y = 0;
                                }

                                return;
                            }
                        }
                    }
                }
            }

            mIndexReader.Close();
            staticsReader.Close();
        }
    }

    public class RectangleArea
    {
        public RectangleArea()
        {
            StartX = 0;
            StartY = 0;
            EndX = 0;
            EndY = 0;
        }

        public RectangleArea(Map map, int sx, int sy, int ex, int ey) : this()
        {
            if (map == null)
            {
                return;
            }

            if (sx < 0 || sx > map.Width)
            {
                sx = 0;
            }

            if (sy < 0 || sy > map.Height)
            {
                sy = 0;
            }

            if (ex < StartX || ex > map.Width)
            {
                ex = sx;
            }

            if (ey < StartY || ey > map.Height)
            {
                ey = sy;
            }

            StartX = sx;
            StartY = sy;
            EndX = ex;
            EndY = ey;
        }

        public int StartX { get; }
        public int StartY { get; }
        public int EndX { get; }
        public int EndY { get; }
    }

    public class ModArea
    {
        private readonly RectangleArea _area;
        private readonly Dictionary<ushort, ushort> _convertDictLand;
        private readonly Dictionary<ushort, ushort> _convertDictStatic;

        public ModArea(Map map, int sx, int sy, int ex, int ey, Dictionary<ushort, ushort> toConvLand, Dictionary<ushort, ushort> toConvStatic)
        {
            _area = new RectangleArea(map, sx, sy, ex, ey);
            _convertDictLand = toConvLand;
            _convertDictStatic = toConvStatic;
        }

        public int IsStaticReplace(ushort tileId, int x, int y)
        {
            if (x > _area.EndX || x < _area.StartX || y > _area.EndY || y < _area.StartY)
            {
                return -1;
            }

            if (_convertDictStatic.ContainsKey(tileId))
            {
                return _convertDictStatic[tileId];
            }

            return -1;
        }

        public int IsLandReplace(ushort tileId, int x, int y)
        {
            if (x > _area.EndX || x < _area.StartX || y > _area.EndY || y < _area.StartY)
            {
                return -1;
            }

            if (_convertDictLand.ContainsKey(tileId))
            {
                return _convertDictLand[tileId];
            }

            return -1;
        }

        public static int IsStaticReplace(List<ModArea> list, ushort tileId, int blockX, int blockY, int cell)
        {
            int x = (blockX * 8) + (cell % 8);
            int y = (blockY * 8) + (cell / 8);

            foreach (ModArea area in list)
            {
                int temp = area.IsStaticReplace(tileId, x, y);
                if (temp != -1)
                {
                    return temp;
                }
            }

            return -1;
        }

        public static int IsLandReplace(List<ModArea> list, ushort tileId, int blockX, int blockY, int cell)
        {
            int x = (blockX * 8) + (cell % 8);
            int y = (blockY * 8) + (cell / 8);

            foreach (ModArea area in list)
            {
                int temp = area.IsLandReplace(tileId, x, y);
                if (temp != -1)
                {
                    return temp;
                }
            }

            return -1;
        }
    }
}
