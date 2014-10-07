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

namespace FiddlerControls
{
    public partial class MapReplaceTiles : Form
    {
        private Ultima.Map Map;
        private List<ModArea> toReplace;

        public MapReplaceTiles(Ultima.Map map)
        {
            InitializeComponent();
            Map = map;
        }

        private void OnReplace(object sender, EventArgs e)
        {
            string file = textBox1.Text;
            if (String.IsNullOrEmpty(file))
                return;
            if (!File.Exists(file))
                return;
            if (!LoadFile(file))
                return;
            string path = FiddlerControls.Options.OutputPath;
            ReplaceMap(path, Map.FileIndex, Map.Width, Map.Height);
            ReplaceStatic(path, Map.FileIndex, Map.Width, Map.Height);
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Title = "Choose xml file to open";
            dialog.CheckFileExists = true;
            dialog.Filter = "xml files (*.xml)|*.xml";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!File.Exists(dialog.FileName))
                    return;
                textBox1.Text = dialog.FileName;
            }
            dialog.Dispose();
        }

        private bool LoadFile(string file)
        {
            toReplace = new List<ModArea>();
            XmlDocument dom = new XmlDocument();
            dom.Load(file);
            try
            {
                XmlNode xRoot = dom.SelectSingleNode("MapReplace");

                foreach (XmlNode xNode in xRoot.SelectNodes("Area"))
                {
                    if (xNode.NodeType == XmlNodeType.Comment)
                        continue;
                    
                    int sx, sy, ex, ey;
                    FiddlerControls.Utils.ConvertStringToInt(xNode.Attributes["StartX"].InnerText, out sx);
                    FiddlerControls.Utils.ConvertStringToInt(xNode.Attributes["StartY"].InnerText, out sy);
                    FiddlerControls.Utils.ConvertStringToInt(xNode.Attributes["EndX"].InnerText, out ex);
                    FiddlerControls.Utils.ConvertStringToInt(xNode.Attributes["EndY"].InnerText, out ey);

                    Dictionary<ushort, ushort> ConvertDictLand = new Dictionary<ushort, ushort>();
                    Dictionary<ushort, ushort> ConvertDictStatic = new Dictionary<ushort, ushort>();

                    foreach(XmlNode xArea in xNode.ChildNodes)
                    {
                        if (xArea.NodeType == XmlNodeType.Comment)
                            continue;

                        int temp;
                        ushort convfrom, convto;
                        if (FiddlerControls.Utils.ConvertStringToInt(xArea.Attributes["from"].InnerText, out temp))
                            convfrom = (ushort)temp;
                        else
                            continue;
                        if (FiddlerControls.Utils.ConvertStringToInt(xArea.Attributes["to"].InnerText, out temp))
                            convto = (ushort)temp;
                        else
                            continue;

                        switch (xArea.Name.ToLower())
                        {
                            case "static":
                                ConvertDictStatic.Add(convfrom, convto);
                                break;
                            case "landtile":
                                ConvertDictLand.Add(convfrom, convto);
                                break;
                            default: break;
                        }
                    }

                    toReplace.Add(new ModArea(Map, sx, sy, ex, ey, ConvertDictLand, ConvertDictStatic));
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
            string mapPath = Files.GetFilePath(String.Format("map{0}.mul", map));
            FileStream m_map;
            BinaryReader m_mapReader;
            if (mapPath != null)
            {
                m_map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                m_mapReader = new BinaryReader(m_map);
            }
            else
                return;

            int blockx = width >> 3;
            int blocky = height >> 3;

            string mul = Path.Combine(path, String.Format("map{0}.mul", map));
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
                                m_mapReader.BaseStream.Seek(((x * blocky) + y) * 196, SeekOrigin.Begin);
                                int header = m_mapReader.ReadInt32();
                                binmul.Write(header);
                                for (int i = 0; i < 64; ++i)
                                {
                                    ushort tileid = m_mapReader.ReadUInt16();
                                    int temp = ModArea.IsLandReplace(toReplace, tileid, x, y, i);
                                    sbyte z = m_mapReader.ReadSByte();
                                    if ((tileid < 0) || (tileid >= 0x4000))
                                        tileid = 0;
                                    else if (temp != -1)
                                        tileid = (ushort)temp;
                                    if (z < -128)
                                        z = -128;
                                    if (z > 127)
                                        z = 127;
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
                                        binmul.Write((int)0);
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
            m_mapReader.Close();
        }

        private void ReplaceStatic(string path, int map, int width, int height)
        {
            string indexPath = Files.GetFilePath(String.Format("staidx{0}.mul", map));
            FileStream m_Index;
            BinaryReader m_IndexReader;
            if (indexPath != null)
            {
                m_Index = new FileStream(indexPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                m_IndexReader = new BinaryReader(m_Index);
            }
            else
                return;

            string staticsPath = Files.GetFilePath(String.Format("statics{0}.mul", map));

            FileStream m_Statics;
            BinaryReader m_StaticsReader;
            if (staticsPath != null)
            {
                m_Statics = new FileStream(staticsPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                m_StaticsReader = new BinaryReader(m_Statics);
            }
            else
                return;


            int blockx = width >> 3;
            int blocky = height >> 3;

            string idx = Path.Combine(path, String.Format("staidx{0}.mul", map));
            string mul = Path.Combine(path, String.Format("statics{0}.mul", map));
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
                                m_IndexReader.BaseStream.Seek(((x * blocky) + y) * 12, SeekOrigin.Begin);
                                int lookup = m_IndexReader.ReadInt32();
                                int length = m_IndexReader.ReadInt32();
                                int extra = m_IndexReader.ReadInt32();

                                if ((lookup < 0 || length <= 0))
                                {
                                    binidx.Write((int)-1); // lookup
                                    binidx.Write((int)-1); // length
                                    binidx.Write((int)-1); // extra
                                }
                                else
                                {
                                    if ((lookup >= 0) && (length > 0))
                                        m_Statics.Seek(lookup, SeekOrigin.Begin);

                                    int fsmullength = (int)fsmul.Position;
                                    int count = length / 7;

                                    bool firstitem = true;
                                    for (int i = 0; i < count; ++i)
                                    {
                                        ushort graphic = m_StaticsReader.ReadUInt16();
                                        byte sx = m_StaticsReader.ReadByte();
                                        byte sy = m_StaticsReader.ReadByte();
                                        sbyte sz = m_StaticsReader.ReadSByte();
                                        short shue = m_StaticsReader.ReadInt16();
                                        int temp = ModArea.IsStaticReplace(toReplace, graphic, x, y, i);
                                        if ((graphic >= 0) && (graphic <= Art.GetMaxItemID()))
                                        {
                                            if (shue < 0)
                                                shue = 0;
                                            if (firstitem)
                                            {
                                                binidx.Write((int)fsmul.Position); //lookup
                                                firstitem = false;
                                            }
                                            if (temp != -1)
                                                graphic = (ushort)temp;
                                            binmul.Write(graphic);
                                            binmul.Write(sx);
                                            binmul.Write(sy);
                                            binmul.Write(sz);
                                            binmul.Write(shue);
                                        }
                                    }

                                    fsmullength = (int)fsmul.Position - fsmullength;
                                    if (fsmullength > 0)
                                    {
                                        binidx.Write(fsmullength); //length
                                        if (extra == -1)
                                            extra = 0;
                                        binidx.Write(extra); //extra
                                    }
                                    else
                                    {
                                        binidx.Write((int)-1); //lookup
                                        binidx.Write((int)-1); //length
                                        binidx.Write((int)-1); //extra
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
                                        binidx.Write((int)-1); //lookup
                                        binidx.Write((int)-1); //length
                                        binidx.Write((int)-1); //extra
                                    }
                                    y = 0;
                                }
                                return;
                            }
                        }
                    }
                }
            }
            m_IndexReader.Close();
            m_StaticsReader.Close();
        }
    }

    public class RectangleArea
    {
        private int startx;
        private int starty;
        private int endx;
        private int endy;

        public RectangleArea()
        {
            startx = 0;
            starty = 0;
            endx = 0;
            endy = 0;
        }

        public RectangleArea(Ultima.Map map, int sx, int sy, int ex, int ey) : this()
        {
            if(map == null)
                return;

            if(sx < 0 || sx > map.Width)
                sx = 0;

            if(sy < 0 || sy > map.Height)
                sy = 0;

            if(ex < startx || ex > map.Width)
                ex = sx;

            if(ey < starty || ey > map.Height)
                ey = sy;

            startx = sx;
            starty = sy;
            endx = ex;
            endy = ey;
        }

        public int startX { get { return startx; } }
        public int startY { get { return starty; } }
        public int endX { get { return endx; } }
        public int endY { get { return endy; } }
    }

    public class ModArea
    {
        private RectangleArea area;
        private Dictionary<ushort, ushort> ConvertDictLand;
        private Dictionary<ushort, ushort> ConvertDictStatic;

        public ModArea(Ultima.Map map, int sx, int sy, int ex, int ey, Dictionary<ushort, ushort> toConvLand, Dictionary<ushort, ushort> toConvStatic)
        {
            area = new RectangleArea(map, sx, sy, ex, ey);
            ConvertDictLand = toConvLand;
            ConvertDictStatic = toConvStatic;
        }

        public int IsStaticReplace(ushort tileid, int x, int y)
        {
            if (x > area.endX || x < area.startX || y > area.endY || y < area.startY)
                return -1;

            if (ConvertDictStatic.ContainsKey(tileid))
                return ConvertDictStatic[tileid];

            return -1;
        }

        public int IsLandReplace(ushort tileid, int x, int y)
        {
            if (x > area.endX || x < area.startX || y > area.endY || y < area.startY)
                return -1;

            if (ConvertDictLand.ContainsKey(tileid))
                return ConvertDictLand[tileid];

            return -1;
        }

        public static int IsStaticReplace(List<ModArea> list, ushort tileid, int BlockX, int BlockY, int Cell)
        {
            int x = ( BlockX * 8)  + ( Cell % 8 ); 
            int y = (BlockY * 8 ) + ( Cell / 8 );

            foreach(ModArea area in list)
            {
                int temp = area.IsStaticReplace(tileid, x, y);
                if (temp != -1)
                    return temp;
            }

            return -1;
        }

        public static int IsLandReplace(List<ModArea> list, ushort tileid, int BlockX, int BlockY, int Cell)
        {
            int x = (BlockX * 8) + (Cell % 8);
            int y = (BlockY * 8) + (Cell / 8);

            foreach (ModArea area in list)
            {
                int temp = area.IsLandReplace(tileid, x, y);
                if (temp != -1)
                    return temp;
            }

            return -1;
        }
    }
}
