using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima
{
    /// <summary>
    /// Represents land tile data.
    /// <seealso cref="ItemData" />
    /// <seealso cref="LandData" />
    /// </summary>
    public struct LandData
    {
/*
 // TODO: unused?
        public LandData(string name, int texId, TileFlag flags, int unk1)
        {
            Name = name;
            this.TextureID = (short)texId;
            Flags = flags;
            Unk1 = unk1;
        }
*/

        public unsafe LandData(NewLandTileDataMul mulStruct)
        {
            TextureID = mulStruct.texID;
            Flags = (TileFlag)mulStruct.flags;
            Unk1 = mulStruct.unk1;
            Name = TileData.ReadNameString(mulStruct.name);
        }

        public unsafe LandData(OldLandTileDataMul mulStruct)
        {
            TextureID = mulStruct.texID;
            Flags = (TileFlag)mulStruct.flags;
            Unk1 = 0;
            Name = TileData.ReadNameString(mulStruct.name);
        }

        /// <summary>
        /// Gets the name of this land tile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the Texture ID of this land tile.
        /// </summary>
        public short TextureID { get; set; }

        /// <summary>
        /// Gets a bitfield representing the 32 individual flags of this land tile.
        /// </summary>
        public TileFlag Flags { get; set; }

        /// <summary>
        /// Gets a new UOHSA Unknown Int
        /// </summary>
        public int Unk1 { get; set; }

        public void ReadData(string[] split)
        {
            int i = 1;
            Name = split[i++];
            TextureID = (short)TileData.ConvertStringToInt(split[i++]);
            Unk1 = TileData.ConvertStringToInt(split[i++]);
            Flags = 0;
            int temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Background;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Weapon;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Transparent;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Translucent;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Wall;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Damaging;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Impassable;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Wet;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unknown1;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Surface;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Bridge;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Generic;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Window;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.NoShoot;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.ArticleA;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.ArticleAn;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Internal;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Foliage;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.PartialHue;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unknown2;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Map;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Container;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Wearable;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.LightSource;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Animation;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.HoverOver;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unknown3;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Armor;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Roof;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Door;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.StairBack;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.StairRight;
            }
        }
    }

    /// <summary>
    /// Represents item tile data.
    /// <seealso cref="TileData" />
    /// <seealso cref="LandData" />
    /// </summary>
    public struct ItemData
    {
        internal string m_Name;
        internal TileFlag m_Flags;
        internal int m_Unk1;
        internal byte m_Weight;
        internal byte m_Quality;
        internal byte m_Quantity;
        internal byte m_Value;
        internal byte m_Height;
        internal short m_Animation;
        internal byte m_Hue;
        internal byte m_StackOffset;
        internal short m_MiscData;
        internal byte m_Unk2;
        internal byte m_Unk3;

/*
 // TODO: unused?
        public ItemData(
            string name,
            TileFlag flags,
            int unk1,
            int weight,
            int quality,
            int quantity,
            int value,
            int height,
            int anim,
            int hue,
            int stackingOffset,
            int miscData,
            int unk2,
            int unk3)
        {
            m_Name = name;
            m_Flags = flags;
            m_Unk1 = unk1;
            m_Weight = (byte)weight;
            m_Quality = (byte)quality;
            m_Quantity = (byte)quantity;
            m_Value = (byte)value;
            m_Height = (byte)height;
            m_Animation = (short)anim;
            m_Hue = (byte)hue;
            m_StackOffset = (byte)stackingOffset;
            m_MiscData = (short)miscData;
            m_Unk2 = (byte)unk2;
            m_Unk3 = (byte)unk3;
        }
*/

        public unsafe ItemData(NewItemTileDataMul mulStruct)
        {
            m_Name = TileData.ReadNameString(mulStruct.name);
            m_Flags = (TileFlag)mulStruct.flags;
            m_Unk1 = mulStruct.unk1;
            m_Weight = mulStruct.weight;
            m_Quality = mulStruct.quality;
            m_Quantity = mulStruct.quantity;
            m_Value = mulStruct.value;
            m_Height = mulStruct.height;
            m_Animation = mulStruct.anim;
            m_Hue = mulStruct.hue;
            m_StackOffset = mulStruct.stackingOffset;
            m_MiscData = mulStruct.miscData;
            m_Unk2 = mulStruct.unk2;
            m_Unk3 = mulStruct.unk3;
        }

        public unsafe ItemData(OldItemTileDataMul mulStruct)
        {
            m_Name = TileData.ReadNameString(mulStruct.name);
            m_Flags = (TileFlag)mulStruct.flags;
            m_Unk1 = 0;
            m_Weight = mulStruct.weight;
            m_Quality = mulStruct.quality;
            m_Quantity = mulStruct.quantity;
            m_Value = mulStruct.value;
            m_Height = mulStruct.height;
            m_Animation = mulStruct.anim;
            m_Hue = mulStruct.hue;
            m_StackOffset = mulStruct.stackingOffset;
            m_MiscData = mulStruct.miscData;
            m_Unk2 = mulStruct.unk2;
            m_Unk3 = mulStruct.unk3;
        }

        /// <summary>
        /// Gets the name of this item.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        /// <summary>
        /// Gets the animation body index of this item.
        /// <seealso cref="Animations" />
        /// </summary>
        public short Animation
        {
            get { return m_Animation; }
            set { m_Animation = value; }
        }

        /// <summary>
        /// Gets a bitfield representing the 32 individual flags of this item.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public TileFlag Flags
        {
            get { return m_Flags; }
            set { m_Flags = value; }
        }

        /// <summary>
        /// Gets an unknown new UOAHS int
        /// </summary>
        public int Unk1
        {
            get { return m_Unk1; }
            set { m_Unk1 = value; }
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Background" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Background
        {
            get { return (m_Flags & TileFlag.Background) != 0; }
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Bridge" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Bridge
        {
            get { return (m_Flags & TileFlag.Bridge) != 0; }
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Impassable" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Impassable
        {
            get { return (m_Flags & TileFlag.Impassable) != 0; }
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Surface" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Surface
        {
            get { return (m_Flags & TileFlag.Surface) != 0; }
        }

        /// <summary>
        /// Gets the weight of this item.
        /// </summary>
        public byte Weight
        {
            get { return m_Weight; }
            set { m_Weight = value; }
        }

        /// <summary>
        /// Gets the 'quality' of this item. For wearable items, this will be the layer.
        /// </summary>
        public byte Quality
        {
            get { return m_Quality; }
            set { m_Quality = value; }
        }

        /// <summary>
        /// Gets the 'quantity' of this item.
        /// </summary>
        public byte Quantity
        {
            get { return m_Quantity; }
            set { m_Quantity = value; }
        }

        /// <summary>
        /// Gets the 'value' of this item.
        /// </summary>
        public byte Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        /// <summary>
        /// Gets the Hue of this item.
        /// </summary>
        public byte Hue
        {
            get { return m_Hue; }
            set { m_Hue = value; }
        }

        /// <summary>
        /// Gets the stackingOffset of this item. (If flag Generic)
        /// </summary>
        public byte StackingOffset
        {
            get { return m_StackOffset; }
            set { m_StackOffset = value; }
        }

        /// <summary>
        /// Gets the height of this item.
        /// </summary>
        public byte Height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        /// <summary>
        /// Gets the miscData of this item. (old UO Demo weapon template definition) (Unk1)
        /// </summary>
        public short MiscData
        {
            get { return m_MiscData; }
            set { m_MiscData = value; }
        }

        /// <summary>
        /// Gets the unk2 of this item.
        /// </summary>
        public byte Unk2
        {
            get { return m_Unk2; }
            set { m_Unk2 = value; }
        }

        /// <summary>
        /// Gets the unk3 of this item.
        /// </summary>
        public byte Unk3
        {
            get { return m_Unk3; }
            set { m_Unk3 = value; }
        }

        /// <summary>
        /// Gets the 'calculated height' of this item. For <see cref="Bridge">bridges</see>, this will be: <c>(<see cref="Height" /> / 2)</c>.
        /// </summary>
        public int CalcHeight
        {
            get
            {
                if ((m_Flags & TileFlag.Bridge) != 0)
                {
                    return m_Height / 2;
                }
                else
                {
                    return m_Height;
                }
            }
        }

        /// <summary>
        /// Whether or not this item is wearable as '<see cref="TileFlag.Wearable" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Wearable
        {
            get { return (m_Flags & TileFlag.Wearable) != 0; }
        }

        public void ReadData(string[] split)
        {
            m_Name = split[1];
            m_Weight = Convert.ToByte(split[2]);
            m_Quality = Convert.ToByte(split[3]);
            m_Animation = (short)TileData.ConvertStringToInt(split[4]);
            m_Height = Convert.ToByte(split[5]);
            m_Hue = Convert.ToByte(split[6]);
            m_Quantity = Convert.ToByte(split[7]);
            m_StackOffset = Convert.ToByte(split[8]);
            m_MiscData = Convert.ToInt16(split[9]);
            m_Unk1 = Convert.ToInt32(split[10]);
            m_Unk2 = Convert.ToByte(split[11]);
            m_Unk3 = Convert.ToByte(split[12]);

            m_Flags = 0;
            int temp = Convert.ToByte(split[13]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Background;
            }

            temp = Convert.ToByte(split[14]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Weapon;
            }

            temp = Convert.ToByte(split[15]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Transparent;
            }

            temp = Convert.ToByte(split[16]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Translucent;
            }

            temp = Convert.ToByte(split[17]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Wall;
            }

            temp = Convert.ToByte(split[18]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Damaging;
            }

            temp = Convert.ToByte(split[19]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Impassable;
            }

            temp = Convert.ToByte(split[20]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Wet;
            }

            temp = Convert.ToByte(split[21]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Unknown1;
            }

            temp = Convert.ToByte(split[22]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Surface;
            }

            temp = Convert.ToByte(split[23]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Bridge;
            }

            temp = Convert.ToByte(split[24]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Generic;
            }

            temp = Convert.ToByte(split[25]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Window;
            }

            temp = Convert.ToByte(split[26]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.NoShoot;
            }

            temp = Convert.ToByte(split[27]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.ArticleA;
            }

            temp = Convert.ToByte(split[28]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.ArticleAn;
            }

            temp = Convert.ToByte(split[29]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Internal;
            }

            temp = Convert.ToByte(split[30]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Foliage;
            }

            temp = Convert.ToByte(split[31]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.PartialHue;
            }

            temp = Convert.ToByte(split[32]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Unknown2;
            }

            temp = Convert.ToByte(split[33]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Map;
            }

            temp = Convert.ToByte(split[34]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Container;
            }

            temp = Convert.ToByte(split[35]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Wearable;
            }

            temp = Convert.ToByte(split[36]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.LightSource;
            }

            temp = Convert.ToByte(split[37]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Animation;
            }

            temp = Convert.ToByte(split[38]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.HoverOver;
            }

            temp = Convert.ToByte(split[39]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Unknown3;
            }

            temp = Convert.ToByte(split[40]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Armor;
            }

            temp = Convert.ToByte(split[41]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Roof;
            }

            temp = Convert.ToByte(split[42]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.Door;
            }

            temp = Convert.ToByte(split[43]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.StairBack;
            }

            temp = Convert.ToByte(split[44]);
            if (temp != 0)
            {
                m_Flags |= TileFlag.StairRight;
            }
        }
    }

    /// <summary>
    /// An enumeration of 32 different tile flags.
    /// <seealso cref="ItemData" />
    /// <seealso cref="LandData" />
    /// </summary>
    [Flags]
    public enum TileFlag
    {
        /// <summary>
        /// Nothing is flagged.
        /// </summary>
        None = 0x00000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Background = 0x00000001,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Weapon = 0x00000002,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Transparent = 0x00000004,
        /// <summary>
        /// The tile is rendered with partial alpha-transparency.
        /// </summary>
        Translucent = 0x00000008,
        /// <summary>
        /// The tile is a wall.
        /// </summary>
        Wall = 0x00000010,
        /// <summary>
        /// The tile can cause damage when moved over.
        /// </summary>
        Damaging = 0x00000020,
        /// <summary>
        /// The tile may not be moved over or through.
        /// </summary>
        Impassable = 0x00000040,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Wet = 0x00000080,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown1 = 0x00000100,
        /// <summary>
        /// The tile is a surface. It may be moved over, but not through.
        /// </summary>
        Surface = 0x00000200,
        /// <summary>
        /// The tile is a stair, ramp, or ladder.
        /// </summary>
        Bridge = 0x00000400,
        /// <summary>
        /// The tile is stackable
        /// </summary>
        Generic = 0x00000800,
        /// <summary>
        /// The tile is a window. Like <see cref="TileFlag.NoShoot" />, tiles with this flag block line of sight.
        /// </summary>
        Window = 0x00001000,
        /// <summary>
        /// The tile blocks line of sight.
        /// </summary>
        NoShoot = 0x00002000,
        /// <summary>
        /// For single-amount tiles, the string "a " should be prepended to the tile name.
        /// </summary>
        ArticleA = 0x00004000,
        /// <summary>
        /// For single-amount tiles, the string "an " should be prepended to the tile name.
        /// </summary>
        ArticleAn = 0x00008000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Internal = 0x00010000,
        /// <summary>
        /// The tile becomes translucent when walked behind. Boat masts also have this flag.
        /// </summary>
        Foliage = 0x00020000,
        /// <summary>
        /// Only gray pixels will be hued
        /// </summary>
        PartialHue = 0x00040000,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown2 = 0x00080000,
        /// <summary>
        /// The tile is a map--in the cartography sense. Unknown usage.
        /// </summary>
        Map = 0x00100000,
        /// <summary>
        /// The tile is a container.
        /// </summary>
        Container = 0x00200000,
        /// <summary>
        /// The tile may be equiped.
        /// </summary>
        Wearable = 0x00400000,
        /// <summary>
        /// The tile gives off light.
        /// </summary>
        LightSource = 0x00800000,
        /// <summary>
        /// The tile is animated.
        /// </summary>
        Animation = 0x01000000,
        /// <summary>
        /// Gargoyles can fly over
        /// </summary>
        HoverOver = 0x02000000,
        /// <summary>
        /// Unknown.
        /// </summary>
        Unknown3 = 0x04000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        Armor = 0x08000000,
        /// <summary>
        /// The tile is a slanted roof.
        /// </summary>
        Roof = 0x10000000,
        /// <summary>
        /// The tile is a door. Tiles with this flag can be moved through by ghosts and GMs.
        /// </summary>
        Door = 0x20000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        StairBack = 0x40000000,
        /// <summary>
        /// Not yet documented.
        /// </summary>
        StairRight = unchecked((int)0x80000000)
    }

    /// <summary>
    /// Contains lists of <see cref="LandData">land</see> and <see cref="ItemData">item</see> tile data.
    /// <seealso cref="LandData" />
    /// <seealso cref="ItemData" />
    /// </summary>
    public static class TileData
    {
        /// <summary>
        /// Gets the list of <see cref="LandData">land tile data</see>.
        /// </summary>
        public static LandData[] LandTable { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="ItemData">item tile data</see>.
        /// </summary>
        public static ItemData[] ItemTable { get; private set; }

        public static int[] HeightTable { get; private set; }

        private static readonly byte[] _stringBuffer = new byte[20];

/*
 // TODO: unused?
        private static string ReadNameString(BinaryReader bin)
        {
            bin.Read(_stringBuffer, 0, 20);

            int count;

            for (count = 0; count < 20 && _stringBuffer[count] != 0; ++count)
            {
                // TODO: this loop is weird
                //;
            }

            return Encoding.Default.GetString(_stringBuffer, 0, count);
        }
*/

        public static unsafe string ReadNameString(byte* buffer)
        {
            int count;
            for (count = 0; count < 20 && *buffer != 0; ++count)
            {
                _stringBuffer[count] = *buffer++;
            }

            return Encoding.Default.GetString(_stringBuffer, 0, count);
        }

        private static int[] _landHeader;
        private static int[] _itemHeader;

        static TileData()
        {
            Initialize();
        }

        public static unsafe void Initialize()
        {
            string filePath = Files.GetFilePath("tiledata.mul");

            if (filePath == null)
            {
                return;
            }

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bool useNeWTileDataFormat = Art.IsUOAHS();
                _landHeader = new int[512];
                int j = 0;
                LandTable = new LandData[0x4000];

                var buffer = new byte[fs.Length];
                GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                long currentPos = 0;
                try
                {
                    fs.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < 0x4000; i += 32)
                    {
                        var ptrHeader = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                        currentPos += 4;
                        _landHeader[j++] = (int)Marshal.PtrToStructure(ptrHeader, typeof(int));
                        for (int count = 0; count < 32; ++count)
                        {
                            var ptr = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                            if (useNeWTileDataFormat)
                            {
                                currentPos += sizeof(NewLandTileDataMul);
                                var cur = (NewLandTileDataMul)Marshal.PtrToStructure(ptr, typeof(NewLandTileDataMul));
                                LandTable[i + count] = new LandData(cur);
                            }
                            else
                            {
                                currentPos += sizeof(OldLandTileDataMul);
                                var cur = (OldLandTileDataMul)Marshal.PtrToStructure(ptr, typeof(OldLandTileDataMul));
                                LandTable[i + count] = new LandData(cur);
                            }
                        }
                    }

                    long remaining = buffer.Length - currentPos;

                    int structSize = useNeWTileDataFormat ? sizeof(NewItemTileDataMul) : sizeof(OldItemTileDataMul);

                    _itemHeader = new int[remaining / ((structSize * 32) + 4)];
                    int itemLength = _itemHeader.Length * 32;

                    ItemTable = new ItemData[itemLength];
                    HeightTable = new int[itemLength];

                    j = 0;
                    for (int i = 0; i < itemLength; i += 32)
                    {
                        var ptrHeader = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                        currentPos += 4;
                        _itemHeader[j++] = (int)Marshal.PtrToStructure(ptrHeader, typeof(int));
                        for (int count = 0; count < 32; ++count)
                        {
                            var ptr = new IntPtr((long)gc.AddrOfPinnedObject() + currentPos);
                            if (useNeWTileDataFormat)
                            {
                                currentPos += sizeof(NewItemTileDataMul);
                                var cur = (NewItemTileDataMul)Marshal.PtrToStructure(ptr, typeof(NewItemTileDataMul));
                                ItemTable[i + count] = new ItemData(cur);
                                HeightTable[i + count] = cur.height;
                            }
                            else
                            {
                                currentPos += sizeof(OldItemTileDataMul);
                                var cur = (OldItemTileDataMul)Marshal.PtrToStructure(ptr, typeof(OldItemTileDataMul));
                                ItemTable[i + count] = new ItemData(cur);
                                HeightTable[i + count] = cur.height;
                            }
                        }
                    }
                }
                finally
                {
                    gc.Free();
                }
            }
        }

        /// <summary>
        /// Saves <see cref="LandData"/> and <see cref="ItemData"/> to tiledata.mul
        /// </summary>
        /// <param name="fileName"></param>
        public static void SaveTileData(string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var bin = new BinaryWriter(fs))
                {
                    int j = 0;
                    bool useNewTileDataFormat = Art.IsUOAHS();
                    for (int i = 0; i < 0x4000; ++i)
                    {
                        if ((i & 0x1F) == 0)
                        {
                            bin.Write(_landHeader[j++]); // header
                        }

                        bin.Write((int)LandTable[i].Flags);
                        if (useNewTileDataFormat)
                        {
                            bin.Write(LandTable[i].Unk1);
                        }

                        bin.Write(LandTable[i].TextureID);
                        var b = new byte[20];
                        if (LandTable[i].Name != null)
                        {
                            byte[] bb = Encoding.Default.GetBytes(LandTable[i].Name);
                            if (bb.Length > 20)
                            {
                                Array.Resize(ref bb, 20);
                            }

                            bb.CopyTo(b, 0);
                        }
                        bin.Write(b);
                    }

                    j = 0;

                    for (int i = 0; i < ItemTable.Length; ++i)
                    {
                        if ((i & 0x1F) == 0)
                        {
                            bin.Write(_itemHeader[j++]); // header
                        }

                        bin.Write((int)ItemTable[i].Flags);
                        if (useNewTileDataFormat)
                        {
                            bin.Write(ItemTable[i].Unk1);
                        }

                        bin.Write(ItemTable[i].Weight);
                        bin.Write(ItemTable[i].Quality);
                        bin.Write(ItemTable[i].MiscData);
                        bin.Write(ItemTable[i].Unk2);
                        bin.Write(ItemTable[i].Quantity);
                        bin.Write(ItemTable[i].Animation);
                        bin.Write(ItemTable[i].Unk3);
                        bin.Write(ItemTable[i].Hue);
                        bin.Write(ItemTable[i].StackingOffset); // unk4
                        bin.Write(ItemTable[i].Value); // unk5
                        bin.Write(ItemTable[i].Height);

                        var b = new byte[20];
                        if (ItemTable[i].Name != null)
                        {
                            byte[] bb = Encoding.Default.GetBytes(ItemTable[i].Name);
                            if (bb.Length > 20)
                            {
                                Array.Resize(ref bb, 20);
                            }

                            bb.CopyTo(b, 0);
                        }

                        bin.Write(b);
                    }
                }
            }
        }

        /// <summary>
        /// Exports <see cref="ItemData"/> to csv file
        /// </summary>
        /// <param name="fileName"></param>
        public static void ExportItemDataToCSV(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.Write("ID;Name;Weight/Quantity;Layer/Quality;Gump/AnimID;Height;Hue;Class/Quantity;StackingOffset;miscData;Unknown1;Unknown2;Unknown3");
                tex.Write(";Background;Weapon;Transparent;Translucent;Wall;Damage;Impassible;Wet;Unknow1");
                tex.Write(";Surface;Bridge;Generic;Window;NoShoot;PrefixA;PrefixAn;Internal;Foliage;PartialHue");
                tex.Write(";Unknow2;Map;Container/Height;Wearable;Lightsource;Animation;HoverOver");
                tex.WriteLine(";Unknow3;Armor;Roof;Door;StairBack;StairRight");

                for (int i = 0; i < ItemTable.Length; ++i)
                {
                    ItemData tile = ItemTable[i];
                    tex.Write("0x{0:X4}", i);
                    tex.Write(";{0}", tile.Name);
                    tex.Write($";{tile.Weight}");
                    tex.Write($";{tile.Quality}");
                    tex.Write(";0x{0:X4}", tile.Animation);
                    tex.Write($";{tile.Height}");
                    tex.Write($";{tile.Hue}");
                    tex.Write($";{tile.Quantity}");
                    tex.Write($";{tile.StackingOffset}");
                    tex.Write($";{tile.MiscData}");
                    tex.Write($";{tile.Unk1}");
                    tex.Write($";{tile.Unk2}");
                    tex.Write($";{tile.Unk3}");

                    tex.Write($";{((tile.Flags & TileFlag.Background) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Weapon) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Transparent) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Translucent) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Wall) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Damaging) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Impassable) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Wet) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Unknown1) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Surface) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Bridge) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Generic) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Window) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.NoShoot) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.ArticleA) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.ArticleAn) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Internal) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Foliage) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.PartialHue) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Unknown2) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Map) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Container) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Wearable) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.LightSource) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Animation) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.HoverOver) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Unknown3) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Armor) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Roof) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Door) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.StairBack) != 0 ? "1" : "0")}");
                    tex.WriteLine($";{((tile.Flags & TileFlag.StairRight) != 0 ? "1" : "0")}");
                }
            }
        }

        /// <summary>
        /// Exports <see cref="LandData"/> to csv file
        /// </summary>
        /// <param name="fileName"></param>
        public static void ExportLandDataToCSV(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite)))
            {
                tex.Write("ID;Name;TextureID;HSAUnk1");
                tex.Write(";Background;Weapon;Transparent;Translucent;Wall;Damage;Impassible;Wet;Unknow1");
                tex.Write(";Surface;Bridge;Generic;Window;NoShoot;PrefixA;PrefixAn;Internal;Foliage;PartialHue");
                tex.Write(";Unknow2;Map;Container/Height;Wearable;Lightsource;Animation;HoverOver");
                tex.WriteLine(";Unknow3;Armor;Roof;Door;StairBack;StairRight");

                for (int i = 0; i < LandTable.Length; ++i)
                {
                    LandData tile = LandTable[i];
                    tex.Write("0x{0:X4}", i);
                    tex.Write($";{tile.Name}");
                    tex.Write($";0x{tile.TextureID:X4}");
                    tex.Write($";{tile.Unk1}");

                    tex.Write($";{((tile.Flags & TileFlag.Background) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Weapon) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Transparent) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Translucent) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Wall) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Damaging) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Impassable) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Wet) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Unknown1) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Surface) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Bridge) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Generic) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Window) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.NoShoot) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.ArticleA) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.ArticleAn) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Internal) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Foliage) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.PartialHue) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Unknown2) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Map) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Container) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Wearable) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.LightSource) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Animation) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.HoverOver) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Unknown3) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Armor) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Roof) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.Door) != 0 ? "1" : "0")}");
                    tex.Write($";{((tile.Flags & TileFlag.StairBack) != 0 ? "1" : "0")}");
                    tex.WriteLine($";{((tile.Flags & TileFlag.StairRight) != 0 ? "1" : "0")}");
                }
            }
        }

        public static int ConvertStringToInt(string text)
        {
            int result;
            if (text.Contains("0x"))
            {
                string convert = text.Replace("0x", "");
                int.TryParse(convert, NumberStyles.HexNumber, null, out result);
            }
            else
            {
                int.TryParse(text, NumberStyles.Integer, null, out result);
            }

            return result;
        }

        public static void ImportItemDataFromCSV(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (var sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.StartsWith("ID;"))
                    {
                        continue;
                    }

                    try
                    {
                        string[] split = line.Split(';');
                        if (split.Length < 45)
                        {
                            continue;
                        }

                        int id = ConvertStringToInt(split[0]);
                        ItemTable[id].ReadData(split);
                    }
                    catch
                    {
                        // TODO: ignored?
                        // ignored
                    }
                }
            }
        }

        public static void ImportLandDataFromCSV(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (var sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                    {
                        continue;
                    }

                    if (line.StartsWith("ID;"))
                    {
                        continue;
                    }

                    try
                    {
                        string[] split = line.Split(';');
                        if (split.Length < 36)
                        {
                            continue;
                        }

                        int id = ConvertStringToInt(split[0]);
                        LandTable[id].ReadData(split);
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
    public unsafe struct OldLandTileDataMul
    {
        public readonly int flags;
        public readonly short texID;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NewLandTileDataMul
    {
        public readonly int flags;
        public readonly int unk1;
        public readonly short texID;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OldItemTileDataMul
    {
        public readonly int flags;
        public readonly byte weight;
        public readonly byte quality;
        public readonly short miscData;
        public readonly byte unk2;
        public readonly byte quantity;
        public readonly short anim;
        public readonly byte unk3;
        public readonly byte hue;
        public readonly byte stackingOffset;
        public readonly byte value;
        public readonly byte height;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NewItemTileDataMul
    {
        public readonly int flags;
        public readonly int unk1;
        public readonly byte weight;
        public readonly byte quality;
        public readonly short miscData;
        public readonly byte unk2;
        public readonly byte quantity;
        public readonly short anim;
        public readonly byte unk3;
        public readonly byte hue;
        public readonly byte stackingOffset;
        public readonly byte value;
        public readonly byte height;
        public fixed byte name[20];
    }
}