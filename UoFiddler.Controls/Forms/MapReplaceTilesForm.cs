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
            string file = textBox1.Text;
            if (string.IsNullOrEmpty(file))
            {
                return;
            }

            if (!File.Exists(file))
            {
                return;
            }

            if (!LoadFile(file))
            {
                return;
            }

            string path = Options.OutputPath;
            ReplaceMap(path, _map.FileIndex, _map.Width, _map.Height);
            ReplaceStatic(path, _map.FileIndex, _map.Width, _map.Height);
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose xml file to open",
                CheckFileExists = true,
                Filter = "xml files (*.xml)|*.xml"
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(dialog.FileName))
                {
                    return;
                }

                textBox1.Text = dialog.FileName;
            }
            dialog.Dispose();
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

                        ushort convfrom, convto;
                        if (Utils.ConvertStringToInt(xArea.Attributes["from"].InnerText, out int temp))
                        {
                            convfrom = (ushort)temp;
                        }
                        else
                        {
                            continue;
                        }

                        if (Utils.ConvertStringToInt(xArea.Attributes["to"].InnerText, out temp))
                        {
                            convto = (ushort)temp;
                        }
                        else
                        {
                            continue;
                        }

                        switch (xArea.Name.ToLower())
                        {
                            case "static":
                                convertDictStatic.Add(convfrom, convto);
                                break;
                            case "landtile":
                                convertDictLand.Add(convfrom, convto);
                                break;
                            default: break;
                        }
                    }

                    _toReplace.Add(new ModArea(_map, sx, sy, ex, ey, convertDictLand, convertDictStatic));
                }
            }
            catch
            {
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

            int blockx = width >> 3;
            int blocky = height >> 3;

            string mul = Path.Combine(path, $"map{map}.mul");
            using (FileStream fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binmul = new BinaryWriter(fsmul))
                {
                    for (int x = 0; x < blockx; ++x)
                    {
                        for (int y = 0; y < blocky; ++y)
                        {
                            try
                            {
                                mMapReader.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                int header = mMapReader.ReadInt32();
                                binmul.Write(header);
                                for (int i = 0; i < 64; ++i)
                                {
                                    ushort tileid = mMapReader.ReadUInt16();
                                    int temp = ModArea.IsLandReplace(_toReplace, tileid, x, y, i);
                                    sbyte z = mMapReader.ReadSByte();
                                    if (tileid >= 0x4000)
                                    {
                                        tileid = 0;
                                    }
                                    else if (temp != -1)
                                    {
                                        tileid = (ushort)temp;
                                    }

                                    if (z < -128)
                                    {
                                        z = -128;
                                    }

                                    if (z > 127)
                                    {
                                        z = 127;
                                    }

                                    binmul.Write(tileid);
                                    binmul.Write(z);
                                }
                            }
                            catch //fill rest
                            {
                                binmul.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                for (; x < blockx; ++x)
                                {
                                    for (; y < blocky; ++y)
                                    {
                                        binmul.Write(0);
                                        for (int i = 0; i < 64; ++i)
                                        {
                                            binmul.Write((short)0);
                                            binmul.Write((sbyte)0);
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
            BinaryReader mStaticsReader;
            if (staticsPath != null)
            {
                mStatics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                mStaticsReader = new BinaryReader(mStatics);
            }
            else
            {
                return;
            }

            int blockx = width >> 3;
            int blocky = height >> 3;

            string idx = Path.Combine(path, $"staidx{map}.mul");
            string mul = Path.Combine(path, $"statics{map}.mul");
            using (FileStream fsidx = new FileStream(idx, FileMode.Create, FileAccess.Write, FileShare.Write),
                              fsmul = new FileStream(mul, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter binidx = new BinaryWriter(fsidx),
                                    binmul = new BinaryWriter(fsmul))
                {
                    for (int x = 0; x < blockx; ++x)
                    {
                        for (int y = 0; y < blocky; ++y)
                        {
                            try
                            {
                                mIndexReader.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                int lookup = mIndexReader.ReadInt32();
                                int length = mIndexReader.ReadInt32();
                                int extra = mIndexReader.ReadInt32();

                                if (lookup < 0 || length <= 0)
                                {
                                    binidx.Write(-1); // lookup
                                    binidx.Write(-1); // length
                                    binidx.Write(-1); // extra
                                }
                                else
                                {
                                    if (lookup >= 0 && length > 0)
                                    {
                                        mStatics.Seek(lookup, SeekOrigin.Begin);
                                    }

                                    int fsMulLength = (int)fsmul.Position;
                                    int count = length / 7;

                                    bool firstItem = true;
                                    for (int i = 0; i < count; ++i)
                                    {
                                        ushort graphic = mStaticsReader.ReadUInt16();
                                        byte sx = mStaticsReader.ReadByte();
                                        byte sy = mStaticsReader.ReadByte();
                                        sbyte sz = mStaticsReader.ReadSByte();
                                        short sHue = mStaticsReader.ReadInt16();
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
                                            binidx.Write((int)fsmul.Position); //lookup
                                            firstItem = false;
                                        }
                                        if (temp != -1)
                                        {
                                            graphic = (ushort)temp;
                                        }

                                        binmul.Write(graphic);
                                        binmul.Write(sx);
                                        binmul.Write(sy);
                                        binmul.Write(sz);
                                        binmul.Write(sHue);
                                    }

                                    fsMulLength = (int)fsmul.Position - fsMulLength;
                                    if (fsMulLength > 0)
                                    {
                                        binidx.Write(fsMulLength); //length
                                        if (extra == -1)
                                        {
                                            extra = 0;
                                        }

                                        binidx.Write(extra); //extra
                                    }
                                    else
                                    {
                                        binidx.Write(-1); //lookup
                                        binidx.Write(-1); //length
                                        binidx.Write(-1); //extra
                                    }
                                }
                            }
                            catch // fill the rest
                            {
                                binidx.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                for (; x < blockx; ++x)
                                {
                                    for (; y < blocky; ++y)
                                    {
                                        binidx.Write(-1); //lookup
                                        binidx.Write(-1); //length
                                        binidx.Write(-1); //extra
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
            mStaticsReader.Close();
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

        public int IsStaticReplace(ushort tileid, int x, int y)
        {
            if (x > _area.EndX || x < _area.StartX || y > _area.EndY || y < _area.StartY)
            {
                return -1;
            }

            if (_convertDictStatic.ContainsKey(tileid))
            {
                return _convertDictStatic[tileid];
            }

            return -1;
        }

        public int IsLandReplace(ushort tileid, int x, int y)
        {
            if (x > _area.EndX || x < _area.StartX || y > _area.EndY || y < _area.StartY)
            {
                return -1;
            }

            if (_convertDictLand.ContainsKey(tileid))
            {
                return _convertDictLand[tileid];
            }

            return -1;
        }

        public static int IsStaticReplace(List<ModArea> list, ushort tileid, int blockX, int blockY, int cell)
        {
            int x = (blockX * 8) + (cell % 8);
            int y = (blockY * 8) + (cell / 8);

            foreach (ModArea area in list)
            {
                int temp = area.IsStaticReplace(tileid, x, y);
                if (temp != -1)
                {
                    return temp;
                }
            }

            return -1;
        }

        public static int IsLandReplace(List<ModArea> list, ushort tileid, int blockX, int blockY, int cell)
        {
            int x = (blockX * 8) + (cell % 8);
            int y = (blockY * 8) + (cell / 8);

            foreach (ModArea area in list)
            {
                int temp = area.IsLandReplace(tileid, x, y);
                if (temp != -1)
                {
                    return temp;
                }
            }

            return -1;
        }
    }
}
