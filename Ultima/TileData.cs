using System;
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
        private string _mName;
        private short _mTexId;
        private TileFlag _mFlags;
        private int _mUnk1;

        public LandData(string name, int texId, TileFlag flags, int unk1)
        {
            _mName = name;
            _mTexId = (short)texId;
            _mFlags = flags;
            _mUnk1 = unk1;
        }

        public unsafe LandData(NewLandTileDataMul mulstruct)
        {
            _mTexId = mulstruct.texID;
            _mFlags = (TileFlag)mulstruct.flags;
            _mUnk1 = mulstruct.unk1;
            _mName = TileData.ReadNameString(mulstruct.name);
        }

        public unsafe LandData(OldLandTileDataMul mulstruct)
        {
            _mTexId = mulstruct.texID;
            _mFlags = (TileFlag)mulstruct.flags;
            _mUnk1 = 0;
            _mName = TileData.ReadNameString(mulstruct.name);
        }

        /// <summary>
        /// Gets the name of this land tile.
        /// </summary>
        public string Name
        {
            get => _mName;
            set => _mName = value;
        }

        /// <summary>
        /// Gets the Texture ID of this land tile.
        /// </summary>
        public short TextureId
        {
            get => _mTexId;
            set => _mTexId = value;
        }

        /// <summary>
        /// Gets a bitfield representing the 32 individual flags of this land tile.
        /// </summary>
        public TileFlag Flags
        {
            get => _mFlags;
            set => _mFlags = value;
        }

        /// <summary>
        /// Gets a new UOHSA Unknown Int
        /// </summary>
        public int Unk1
        {
            get => _mUnk1;
            set => _mUnk1 = value;
        }

        public void ReadData(string[] split)
        {
            int i = 1;
            _mName = split[i++];
            _mTexId = (short)TileData.ConvertStringToInt(split[i++]);
            _mUnk1 = TileData.ConvertStringToInt(split[i++]);
            _mFlags = 0;
            int temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Background;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Weapon;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Transparent;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Translucent;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Wall;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Damaging;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Impassable;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Wet;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Unknown1;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Surface;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Bridge;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Generic;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Window;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.NoShoot;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.ArticleA;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.ArticleAn;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Internal;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Foliage;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.PartialHue;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Unknown2;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Map;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Container;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Wearable;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.LightSource;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Animation;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.HoverOver;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Unknown3;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Armor;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Roof;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.Door;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.StairBack;
            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
                _mFlags |= TileFlag.StairRight;
        }
    }

    /// <summary>
    /// Represents item tile data.
    /// <seealso cref="TileData" />
    /// <seealso cref="LandData" />
    /// </summary>
    public struct ItemData
    {
        internal string MName;
        internal TileFlag MFlags;
        internal int MUnk1;
        internal byte MWeight;
        internal byte MQuality;
        internal byte MQuantity;
        internal byte MValue;
        internal byte MHeight;
        internal short MAnimation;
        internal byte MHue;
        internal byte MStackOffset;
        internal short MMiscData;
        internal byte MUnk2;
        internal byte MUnk3;

        public ItemData(string name, TileFlag flags, int unk1, int weight, int quality, int quantity, int value, int height, int anim, int hue, int stackingoffset, int miscData, int unk2, int unk3)
        {
            MName = name;
            MFlags = flags;
            MUnk1 = unk1;
            MWeight = (byte)weight;
            MQuality = (byte)quality;
            MQuantity = (byte)quantity;
            MValue = (byte)value;
            MHeight = (byte)height;
            MAnimation = (short)anim;
            MHue = (byte)hue;
            MStackOffset = (byte)stackingoffset;
            MMiscData = (short)miscData;
            MUnk2 = (byte)unk2;
            MUnk3 = (byte)unk3;
        }

        public unsafe ItemData(NewItemTileDataMul mulstruct)
        {
            MName = TileData.ReadNameString(mulstruct.name);
            MFlags = (TileFlag)mulstruct.flags;
            MUnk1 = mulstruct.unk1;
            MWeight = mulstruct.weight;
            MQuality = mulstruct.quality;
            MQuantity = mulstruct.quantity;
            MValue = mulstruct.value;
            MHeight = mulstruct.height;
            MAnimation = mulstruct.anim;
            MHue = mulstruct.hue;
            MStackOffset = mulstruct.stackingoffset;
            MMiscData = mulstruct.miscdata;
            MUnk2 = mulstruct.unk2;
            MUnk3 = mulstruct.unk3;
        }

        public unsafe ItemData(OldItemTileDataMul mulstruct)
        {
            MName = TileData.ReadNameString(mulstruct.name);
            MFlags = (TileFlag)mulstruct.flags;
            MUnk1 = 0;
            MWeight = mulstruct.weight;
            MQuality = mulstruct.quality;
            MQuantity = mulstruct.quantity;
            MValue = mulstruct.value;
            MHeight = mulstruct.height;
            MAnimation = mulstruct.anim;
            MHue = mulstruct.hue;
            MStackOffset = mulstruct.stackingoffset;
            MMiscData = mulstruct.miscdata;
            MUnk2 = mulstruct.unk2;
            MUnk3 = mulstruct.unk3;
        }

        /// <summary>
        /// Gets the name of this item.
        /// </summary>
        public string Name
        {
            get => MName;
            set => MName = value;
        }

        /// <summary>
        /// Gets the animation body index of this item.
        /// <seealso cref="Animations" />
        /// </summary>
        public short Animation
        {
            get => MAnimation;
            set => MAnimation = value;
        }

        /// <summary>
        /// Gets a bitfield representing the 32 individual flags of this item.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public TileFlag Flags
        {
            get => MFlags;
            set => MFlags = value;
        }

        /// <summary>
        /// Gets an unknown new UOAHS int
        /// </summary>
        public int Unk1
        {
            get => MUnk1;
            set => MUnk1 = value;
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Background" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Background => ((MFlags & TileFlag.Background) != 0);

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Bridge" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Bridge => ((MFlags & TileFlag.Bridge) != 0);

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Impassable" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Impassable => ((MFlags & TileFlag.Impassable) != 0);

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Surface" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Surface => ((MFlags & TileFlag.Surface) != 0);

        /// <summary>
        /// Gets the weight of this item.
        /// </summary>
        public byte Weight
        {
            get => MWeight;
            set => MWeight = value;
        }

        /// <summary>
        /// Gets the 'quality' of this item. For wearable items, this will be the layer.
        /// </summary>
        public byte Quality
        {
            get => MQuality;
            set => MQuality = value;
        }

        /// <summary>
        /// Gets the 'quantity' of this item.
        /// </summary>
        public byte Quantity
        {
            get => MQuantity;
            set => MQuantity = value;
        }

        /// <summary>
        /// Gets the 'value' of this item.
        /// </summary>
        public byte Value
        {
            get => MValue;
            set => MValue = value;
        }

        /// <summary>
        /// Gets the Hue of this item.
        /// </summary>
        public byte Hue
        {
            get => MHue;
            set => MHue = value;
        }

        /// <summary>
        /// Gets the stackingoffset of this item. (If flag Generic)
        /// </summary>
        public byte StackingOffset
        {
            get => MStackOffset;
            set => MStackOffset = value;
        }

        /// <summary>
        /// Gets the height of this item.
        /// </summary>
        public byte Height
        {
            get => MHeight;
            set => MHeight = value;
        }

        /// <summary>
        /// Gets the MiscData of this item. (old UO Demo weapontemplate definition) (Unk1)
        /// </summary>
        public short MiscData
        {
            get => MMiscData;
            set => MMiscData = value;
        }

        /// <summary>
        /// Gets the unk2 of this item.
        /// </summary>
        public byte Unk2
        {
            get => MUnk2;
            set => MUnk2 = value;
        }

        /// <summary>
        /// Gets the unk3 of this item.
        /// </summary>
        public byte Unk3
        {
            get => MUnk3;
            set => MUnk3 = value;
        }

        /// <summary>
        /// Gets the 'calculated height' of this item. For <see cref="Bridge">bridges</see>, this will be: <c>(<see cref="Height" /> / 2)</c>.
        /// </summary>
        public int CalcHeight
        {
            get
            {
                if ((MFlags & TileFlag.Bridge) != 0)
                    return MHeight / 2;
                else
                    return MHeight;
            }
        }

        /// <summary>
        /// Whether or not this item is wearable as '<see cref="TileFlag.Wearable" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Wearable => ((MFlags & TileFlag.Wearable) != 0);

        public void ReadData(string[] split)
        {
            MName = split[1];
            MWeight = Convert.ToByte(split[2]);
            MQuality = Convert.ToByte(split[3]);
            MAnimation = (short)TileData.ConvertStringToInt(split[4]);
            MHeight = Convert.ToByte(split[5]);
            MHue = Convert.ToByte(split[6]);
            MQuantity = Convert.ToByte(split[7]);
            MStackOffset = Convert.ToByte(split[8]);
            MMiscData = Convert.ToInt16(split[9]);
            MUnk1 = Convert.ToInt32(split[10]);
            MUnk2 = Convert.ToByte(split[11]);
            MUnk3 = Convert.ToByte(split[12]);
            
            MFlags = 0;
            int temp = Convert.ToByte(split[13]);
            if (temp != 0)
                MFlags |= TileFlag.Background;
            temp = Convert.ToByte(split[14]);
            if (temp != 0)
                MFlags |= TileFlag.Weapon;
            temp = Convert.ToByte(split[15]);
            if (temp != 0)
                MFlags |= TileFlag.Transparent;
            temp = Convert.ToByte(split[16]);
            if (temp != 0)
                MFlags |= TileFlag.Translucent;
            temp = Convert.ToByte(split[17]);
            if (temp != 0)
                MFlags |= TileFlag.Wall;
            temp = Convert.ToByte(split[18]);
            if (temp != 0)
                MFlags |= TileFlag.Damaging;
            temp = Convert.ToByte(split[19]);
            if (temp != 0)
                MFlags |= TileFlag.Impassable;
            temp = Convert.ToByte(split[20]);
            if (temp != 0)
                MFlags |= TileFlag.Wet;
            temp = Convert.ToByte(split[21]);
            if (temp != 0)
                MFlags |= TileFlag.Unknown1;
            temp = Convert.ToByte(split[22]);
            if (temp != 0)
                MFlags |= TileFlag.Surface;
            temp = Convert.ToByte(split[23]);
            if (temp != 0)
                MFlags |= TileFlag.Bridge;
            temp = Convert.ToByte(split[24]);
            if (temp != 0)
                MFlags |= TileFlag.Generic;
            temp = Convert.ToByte(split[25]);
            if (temp != 0)
                MFlags |= TileFlag.Window;
            temp = Convert.ToByte(split[26]);
            if (temp != 0)
                MFlags |= TileFlag.NoShoot;
            temp = Convert.ToByte(split[27]);
            if (temp != 0)
                MFlags |= TileFlag.ArticleA;
            temp = Convert.ToByte(split[28]);
            if (temp != 0)
                MFlags |= TileFlag.ArticleAn;
            temp = Convert.ToByte(split[29]);
            if (temp != 0)
                MFlags |= TileFlag.Internal;
            temp = Convert.ToByte(split[30]);
            if (temp != 0)
                MFlags |= TileFlag.Foliage;
            temp = Convert.ToByte(split[31]);
            if (temp != 0)
                MFlags |= TileFlag.PartialHue;
            temp = Convert.ToByte(split[32]);
            if (temp != 0)
                MFlags |= TileFlag.Unknown2;
            temp = Convert.ToByte(split[33]);
            if (temp != 0)
                MFlags |= TileFlag.Map;
            temp = Convert.ToByte(split[34]);
            if (temp != 0)
                MFlags |= TileFlag.Container;
            temp = Convert.ToByte(split[35]);
            if (temp != 0)
                MFlags |= TileFlag.Wearable;
            temp = Convert.ToByte(split[36]);
            if (temp != 0)
                MFlags |= TileFlag.LightSource;
            temp = Convert.ToByte(split[37]);
            if (temp != 0)
                MFlags |= TileFlag.Animation;
            temp = Convert.ToByte(split[38]);
            if (temp != 0)
                MFlags |= TileFlag.HoverOver;
            temp = Convert.ToByte(split[39]);
            if (temp != 0)
                MFlags |= TileFlag.Unknown3;
            temp = Convert.ToByte(split[40]);
            if (temp != 0)
                MFlags |= TileFlag.Armor;
            temp = Convert.ToByte(split[41]);
            if (temp != 0)
                MFlags |= TileFlag.Roof;
            temp = Convert.ToByte(split[42]);
            if (temp != 0)
                MFlags |= TileFlag.Door;
            temp = Convert.ToByte(split[43]);
            if (temp != 0)
                MFlags |= TileFlag.StairBack;
            temp = Convert.ToByte(split[44]);
            if (temp != 0)
                MFlags |= TileFlag.StairRight;
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
    public sealed class TileData
    {
        private static LandData[] _mLandData;
        private static ItemData[] _mItemData;
        private static int[] _mHeightTable;

        /// <summary>
        /// Gets the list of <see cref="LandData">land tile data</see>.
        /// </summary>
        public static LandData[] LandTable
        {
            get => _mLandData;
            set => _mLandData = value;
        }

        /// <summary>
        /// Gets the list of <see cref="ItemData">item tile data</see>.
        /// </summary>
        public static ItemData[] ItemTable
        {
            get => _mItemData;
            set => _mItemData = value;
        }

        public static int[] HeightTable => _mHeightTable;

        private static readonly byte[] _mStringBuffer = new byte[20];
        private static string ReadNameString(BinaryReader bin)
        {
            bin.Read(_mStringBuffer, 0, 20);

            int count;

            for (count = 0; count < 20 && _mStringBuffer[count] != 0; ++count) ;

            return Encoding.Default.GetString(_mStringBuffer, 0, count);
        }

        public unsafe static string ReadNameString(byte* buffer)
        {
            int count;
            for (count = 0; count < 20 && *buffer != 0; ++count)
                _mStringBuffer[count] = *buffer++;

            return Encoding.Default.GetString(_mStringBuffer, 0, count);
        }

        private TileData()
        {
        }

        private static int[] _landheader;
        private static int[] _itemheader;

        static TileData()
        {
            Initialize();
        }

        public unsafe static void Initialize()
        {
            string filePath = Files.GetFilePath("tiledata.mul");

            if (filePath != null)
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bool useNeWTileDataFormat = Art.IsUoahs();
                    _landheader = new int[512];
                    int j = 0;
                    _mLandData = new LandData[0x4000];

                    byte[] buffer = new byte[fs.Length];
                    GCHandle gc = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    long currpos = 0;
                    try
                    {
                        fs.Read(buffer, 0, buffer.Length);
                        for (int i = 0; i < 0x4000; i += 32)
                        {
                            IntPtr ptrheader = new IntPtr((long)gc.AddrOfPinnedObject() + currpos);
                            currpos += 4;
                            _landheader[j++] = (int)Marshal.PtrToStructure(ptrheader, typeof(int));
                            for (int count = 0; count < 32; ++count)
                            {
                                IntPtr ptr = new IntPtr((long)gc.AddrOfPinnedObject() + currpos);
                                if (useNeWTileDataFormat)
                                {
                                    currpos += sizeof(NewLandTileDataMul);
                                    NewLandTileDataMul cur = (NewLandTileDataMul)Marshal.PtrToStructure(ptr, typeof(NewLandTileDataMul));
                                    _mLandData[i + count] = new LandData(cur);
                                }
                                else
                                {
                                    currpos += sizeof(OldLandTileDataMul);
                                    OldLandTileDataMul cur = (OldLandTileDataMul)Marshal.PtrToStructure(ptr, typeof(OldLandTileDataMul));
                                    _mLandData[i + count] = new LandData(cur);
                                }
                            }
                        }
                        
                        long remaining = buffer.Length - currpos;
                        int structsize = useNeWTileDataFormat ? sizeof(NewItemTileDataMul) : sizeof(OldItemTileDataMul);
                        _itemheader = new int[(remaining / ((structsize * 32) + 4))];
                        int itemlength = _itemheader.Length * 32;

                        _mItemData = new ItemData[itemlength];
                        _mHeightTable = new int[itemlength];

                        j = 0;
                        for (int i = 0; i < itemlength; i += 32)
                        {
                            IntPtr ptrheader = new IntPtr((long)gc.AddrOfPinnedObject() + currpos);
                            currpos += 4;
                            _itemheader[j++] = (int)Marshal.PtrToStructure(ptrheader, typeof(int));
                            for (int count = 0; count < 32; ++count)
                            {
                                IntPtr ptr = new IntPtr((long)gc.AddrOfPinnedObject() + currpos);
                                if (useNeWTileDataFormat)
                                {
                                    currpos += sizeof(NewItemTileDataMul);
                                    NewItemTileDataMul cur = (NewItemTileDataMul)Marshal.PtrToStructure(ptr, typeof(NewItemTileDataMul));
                                    _mItemData[i + count] = new ItemData(cur);
                                    _mHeightTable[i + count] = cur.height;
                                }
                                else
                                {
                                    currpos += sizeof(OldItemTileDataMul);
                                    OldItemTileDataMul cur = (OldItemTileDataMul)Marshal.PtrToStructure(ptr, typeof(OldItemTileDataMul));
                                    _mItemData[i + count] = new ItemData(cur);
                                    _mHeightTable[i + count] = cur.height;
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
        }

        /// <summary>
        /// Saves <see cref="LandData"/> and <see cref="ItemData"/> to tiledata.mul
        /// </summary>
        /// <param name="fileName"></param>
        public static void SaveTileData(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (BinaryWriter bin = new BinaryWriter(fs))
                {
                    int j = 0;
                    bool useNewTileDataFormat = Art.IsUoahs();
                    for (int i = 0; i < 0x4000; ++i)
                    {
                        if ((i & 0x1F) == 0)
                            bin.Write(_landheader[j++]); //header

                        bin.Write((int)_mLandData[i].Flags);
                        if(useNewTileDataFormat)
                            bin.Write(_mLandData[i].Unk1);

                        bin.Write(_mLandData[i].TextureId);
                        byte[] b = new byte[20];
                        if (_mLandData[i].Name != null)
                        {
                            byte[] bb = Encoding.Default.GetBytes(_mLandData[i].Name);
                            if (bb.Length > 20)
                                Array.Resize(ref bb, 20);
                            bb.CopyTo(b, 0);
                        }
                        bin.Write(b);
                    }
                    j = 0;
                    for (int i = 0; i < _mItemData.Length; ++i)
                    {
                        if ((i & 0x1F) == 0)
                            bin.Write(_itemheader[j++]); // header

                        bin.Write((int)_mItemData[i].Flags);
                        if(useNewTileDataFormat)
                            bin.Write(_mItemData[i].Unk1);
                           
                        bin.Write(_mItemData[i].Weight);
                        bin.Write(_mItemData[i].Quality);
                        bin.Write(_mItemData[i].MiscData);
                        bin.Write(_mItemData[i].Unk2);
                        bin.Write(_mItemData[i].Quantity);
                        bin.Write(_mItemData[i].Animation);
                        bin.Write(_mItemData[i].Unk3);
                        bin.Write(_mItemData[i].Hue);
                        bin.Write(_mItemData[i].StackingOffset); //unk4
                        bin.Write(_mItemData[i].Value); //unk5
                        bin.Write(_mItemData[i].Height);
                        byte[] b = new byte[20];
                        if (_mItemData[i].Name != null)
                        {
                            byte[] bb = Encoding.Default.GetBytes(_mItemData[i].Name);
                            if (bb.Length > 20)
                                Array.Resize(ref bb, 20);
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
        public static void ExportItemDataToCsv(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.Write("ID;Name;Weight/Quantity;Layer/Quality;Gump/AnimID;Height;Hue;Class/Quantity;StackingOffset;MiscData;Unknown1;Unknown2;Unknown3");
                tex.Write(";Background;Weapon;Transparent;Translucent;Wall;Damage;Impassible;Wet;Unknow1");
                tex.Write(";Surface;Bridge;Generic;Window;NoShoot;PrefixA;PrefixAn;Internal;Foliage;PartialHue");
                tex.Write(";Unknow2;Map;Container/Height;Wearable;Lightsource;Animation;HoverOver");
                tex.WriteLine(";Unknow3;Armor;Roof;Door;StairBack;StairRight");

                for (int i = 0; i < _mItemData.Length; ++i)
                {
                    ItemData tile = _mItemData[i];
                    tex.Write($"0x{i:X4}");
                    tex.Write($";{tile.Name}");
                    tex.Write(";" + tile.Weight);
                    tex.Write(";" + tile.Quality);
                    tex.Write($";0x{tile.Animation:X4}");
                    tex.Write(";" + tile.Height);
                    tex.Write(";" + tile.Hue);
                    tex.Write(";" + tile.Quantity);
                    tex.Write(";" + tile.StackingOffset);
                    tex.Write(";" + tile.MiscData);
                    tex.Write(";" + tile.Unk1);
                    tex.Write(";" + tile.Unk2);
                    tex.Write(";" + tile.Unk3);

                    tex.Write(";" + (((tile.Flags & TileFlag.Background) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Weapon) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Transparent) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Translucent) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Wall) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Damaging) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Impassable) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Wet) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Unknown1) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Surface) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Bridge) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Generic) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Window) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.NoShoot) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.ArticleA) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.ArticleAn) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Internal) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Foliage) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.PartialHue) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Unknown2) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Map) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Container) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Wearable) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.LightSource) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Animation) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.HoverOver) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Unknown3) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Armor) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Roof) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Door) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.StairBack) != 0) ? "1" : "0"));
                    tex.WriteLine(";" + (((tile.Flags & TileFlag.StairRight) != 0) ? "1" : "0"));
                }
            }
        }

        /// <summary>
        /// Exports <see cref="LandData"/> to csv file
        /// </summary>
        /// <param name="fileName"></param>
        public static void ExportLandDataToCsv(string fileName)
        {
            using (StreamWriter tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite)))
            {
                tex.Write("ID;Name;TextureID;HSAUnk1");
                tex.Write(";Background;Weapon;Transparent;Translucent;Wall;Damage;Impassible;Wet;Unknow1");
                tex.Write(";Surface;Bridge;Generic;Window;NoShoot;PrefixA;PrefixAn;Internal;Foliage;PartialHue");
                tex.Write(";Unknow2;Map;Container/Height;Wearable;Lightsource;Animation;HoverOver");
                tex.WriteLine(";Unknow3;Armor;Roof;Door;StairBack;StairRight");

                for (int i = 0; i < _mLandData.Length; ++i)
                {
                    LandData tile = _mLandData[i];
                    tex.Write($"0x{i:X4}");
                    tex.Write(";" + tile.Name);
                    tex.Write(";" + $"0x{tile.TextureId:X4}");
                    tex.Write(";" + tile.Unk1);

                    tex.Write(";" + (((tile.Flags & TileFlag.Background) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Weapon) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Transparent) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Translucent) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Wall) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Damaging) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Impassable) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Wet) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Unknown1) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Surface) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Bridge) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Generic) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Window) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.NoShoot) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.ArticleA) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.ArticleAn) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Internal) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Foliage) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.PartialHue) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Unknown2) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Map) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Container) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Wearable) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.LightSource) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Animation) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.HoverOver) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Unknown3) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Armor) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Roof) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.Door) != 0) ? "1" : "0"));
                    tex.Write(";" + (((tile.Flags & TileFlag.StairBack) != 0) ? "1" : "0"));
                    tex.WriteLine(";" + (((tile.Flags & TileFlag.StairRight) != 0) ? "1" : "0"));
                }
            }
        }

        public static int ConvertStringToInt(string text)
        {
            int result;
            if (text.Contains("0x"))
            {
                string convert = text.Replace("0x", "");
                int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out result);
            }
            else
                int.TryParse(text, System.Globalization.NumberStyles.Integer, null, out result);

            return result;
        }

        public static void ImportItemDataFromCsv(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        continue;
                    if (line.StartsWith("ID;"))
                        continue;
                    try
                    {
                        string[] split = line.Split(';');
                        if (split.Length < 45)
                            continue;

                        int id = ConvertStringToInt(split[0]);
                        _mItemData[id].ReadData(split);
                    }
                    catch { }

                }
            }
        }

        public static void ImportLandDataFromCsv(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if ((line = line.Trim()).Length == 0 || line.StartsWith("#"))
                        continue;
                    if (line.StartsWith("ID;"))
                        continue;
                    try
                    {
                        string[] split = line.Split(';');
                        if (split.Length < 36)
                            continue;

                        int id = ConvertStringToInt(split[0]);
                        _mLandData[id].ReadData(split);
                    }
                    catch { }
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OldLandTileDataMul
    {
        public int flags;
        public short texID;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NewLandTileDataMul
    {
        public int flags;
        public int unk1;
        public short texID;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OldItemTileDataMul
    {
        public int flags;
        public byte weight;
        public byte quality;
        public short miscdata;
        public byte unk2;
        public byte quantity;
        public short anim;
        public byte unk3;
        public byte hue;
        public byte stackingoffset;
        public byte value;
        public byte height;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NewItemTileDataMul
    {
        public int flags;
        public int unk1;
        public byte weight;
        public byte quality;
        public short miscdata;
        public byte unk2;
        public byte quantity;
        public short anim;
        public byte unk3;
        public byte hue;
        public byte stackingoffset;
        public byte value;
        public byte height;
        public fixed byte name[20];
    }

}