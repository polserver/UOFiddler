using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Ultima
{
    // TODO: move import/export csv routines to separate class

    /// <summary>
    /// Represents land tile data.
    /// <seealso cref="ItemData" />
    /// <seealso cref="LandData" />
    /// </summary>
    public struct LandData
    {
        public unsafe LandData(NewLandTileDataMul mulStruct)
        {
            TextureID = mulStruct.texID;
            Flags = (TileFlag)mulStruct.flags;
            Name = TileData.ReadNameString(mulStruct.name);
        }

        public unsafe LandData(OldLandTileDataMul mulStruct)
        {
            TextureID = mulStruct.texID;
            Flags = (TileFlag)mulStruct.flags;
            Name = TileData.ReadNameString(mulStruct.name);
        }

        /// <summary>
        /// Gets the name of this land tile.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the Texture ID of this land tile.
        /// </summary>
        public ushort TextureID { get; set; }

        /// <summary>
        /// Gets a bitfield representing the 32 individual flags of this land tile.
        /// </summary>
        public TileFlag Flags { get; set; }

        public void ReadData(string[] split)
        {
            int i = 1;
            Name = split[i++];
            TextureID = (ushort)TileData.ConvertStringToInt(split[i++]);

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
                Flags |= TileFlag.ArticleThe;
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
                Flags |= TileFlag.NoHouse;
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
                Flags |= TileFlag.NoDiagonal;
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

            // Read new flags if file format support them
            if (!Art.IsUOAHS())
            {
                return;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.AlphaBlend;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.UseNewArt;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.ArtUsed;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused8;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.NoShadow;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.PixelBleed;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.PlayAnimOnce;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.MultiMovable;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused10;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused11;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused12;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused13;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused14;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused15;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused16;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused17;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused18;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused19;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused20;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused21;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused22;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused23;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused24;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused25;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused26;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused27;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused28;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused29;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused30;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused31;
            }

            temp = Convert.ToByte(split[i]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused32;
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
        public unsafe ItemData(NewItemTileDataMul mulStruct)
        {
            Name = TileData.ReadNameString(mulStruct.name);
            Flags = (TileFlag)mulStruct.flags;
            Weight = mulStruct.weight;
            Quality = mulStruct.quality;
            Quantity = mulStruct.quantity;
            Value = mulStruct.value;
            Height = mulStruct.height;
            Animation = mulStruct.anim;
            Hue = mulStruct.hue;
            StackingOffset = mulStruct.stackingOffset;
            MiscData = mulStruct.miscData;
            Unk2 = mulStruct.unk2;
            Unk3 = mulStruct.unk3;
        }

        public unsafe ItemData(OldItemTileDataMul mulStruct)
        {
            Name = TileData.ReadNameString(mulStruct.name);
            Flags = (TileFlag)mulStruct.flags;
            Weight = mulStruct.weight;
            Quality = mulStruct.quality;
            Quantity = mulStruct.quantity;
            Value = mulStruct.value;
            Height = mulStruct.height;
            Animation = mulStruct.anim;
            Hue = mulStruct.hue;
            StackingOffset = mulStruct.stackingOffset;
            MiscData = mulStruct.miscData;
            Unk2 = mulStruct.unk2;
            Unk3 = mulStruct.unk3;
        }

        /// <summary>
        /// Gets the name of this item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the animation body index of this item.
        /// <seealso cref="Animations" />
        /// </summary>
        public short Animation { get; set; }

        /// <summary>
        /// Gets a bitfield representing the 32 individual flags of this item.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public TileFlag Flags { get; set; }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Background" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Background
        {
            get { return (Flags & TileFlag.Background) != 0; }
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Bridge" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Bridge
        {
            get { return (Flags & TileFlag.Bridge) != 0; }
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Impassable" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Impassable
        {
            get { return (Flags & TileFlag.Impassable) != 0; }
        }

        /// <summary>
        /// Whether or not this item is flagged as '<see cref="TileFlag.Surface" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Surface
        {
            get { return (Flags & TileFlag.Surface) != 0; }
        }

        /// <summary>
        /// Gets the weight of this item.
        /// </summary>
        public byte Weight { get; set; }

        /// <summary>
        /// Gets the 'quality' of this item. For wearable items, this will be the layer.
        /// </summary>
        public byte Quality { get; set; }

        /// <summary>
        /// Gets the 'quantity' of this item.
        /// </summary>
        public byte Quantity { get; set; }

        /// <summary>
        /// Gets the 'value' of this item.
        /// </summary>
        public byte Value { get; set; }

        /// <summary>
        /// Gets the Hue of this item.
        /// </summary>
        public byte Hue { get; set; }

        /// <summary>
        /// Gets the stackingOffset of this item. (If flag Generic)
        /// </summary>
        public byte StackingOffset { get; set; }

        /// <summary>
        /// Gets the height of this item.
        /// </summary>
        public byte Height { get; set; }

        /// <summary>
        /// Gets the MiscData of this item. (old UO Demo weapon template definition) (Unk1)
        /// </summary>
        public short MiscData { get; set; }

        /// <summary>
        /// Gets the unk2 of this item.
        /// </summary>
        public byte Unk2 { get; set; }

        /// <summary>
        /// Gets the unk3 of this item.
        /// </summary>
        public byte Unk3 { get; set; }

        /// <summary>
        /// Gets the 'calculated height' of this item. For <see cref="Bridge">bridges</see>, this will be: <c>(<see cref="Height" /> / 2)</c>.
        /// </summary>
        public int CalcHeight
        {
            get
            {
                if ((Flags & TileFlag.Bridge) != 0)
                {
                    return Height / 2;
                }

                return Height;
            }
        }

        /// <summary>
        /// Whether or not this item is wearable as '<see cref="TileFlag.Wearable" />'.
        /// <seealso cref="TileFlag" />
        /// </summary>
        public bool Wearable
        {
            get { return (Flags & TileFlag.Wearable) != 0; }
        }

        public void ReadData(string[] split)
        {
            int i = 1;
            Name = split[i++];
            Weight = Convert.ToByte(split[i++]);
            Quality = Convert.ToByte(split[i++]);
            Animation = (short)TileData.ConvertStringToInt(split[i++]);
            Height = Convert.ToByte(split[i++]);
            Hue = Convert.ToByte(split[i++]);
            Quantity = Convert.ToByte(split[i++]);
            StackingOffset = Convert.ToByte(split[i++]);
            MiscData = Convert.ToInt16(split[i++]);
            Unk2 = Convert.ToByte(split[i++]);
            Unk3 = Convert.ToByte(split[i++]);

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
                Flags |= TileFlag.ArticleThe;
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
                Flags |= TileFlag.NoHouse;
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
                Flags |= TileFlag.NoDiagonal;
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

            // Read new flags if file format support them
            if (!Art.IsUOAHS())
            {
                return;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.AlphaBlend;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.UseNewArt;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.ArtUsed;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused8;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.NoShadow;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.PixelBleed;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.PlayAnimOnce;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.MultiMovable;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused10;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused11;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused12;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused13;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused14;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused15;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused16;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused17;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused18;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused19;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused20;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused21;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused22;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused23;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused24;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused25;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused26;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused27;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused28;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused29;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused30;
            }

            temp = Convert.ToByte(split[i++]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused31;
            }

            temp = Convert.ToByte(split[i]);
            if (temp != 0)
            {
                Flags |= TileFlag.Unused32;
            }
        }
    }

    /// <summary>
    /// An enumeration of 64 different tile flags.
    /// <seealso cref="ItemData" />
    /// <seealso cref="LandData" />
    /// </summary>
    [Flags]
    public enum TileFlag : ulong
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
        /// Probably article The prepended to the tile name.
        /// </summary>
        ArticleThe = 0x00010000,
        /// <summary>
        /// The tile becomes translucent when walked behind. Boat masts also have this flag.
        /// </summary>
        Foliage = 0x00020000,
        /// <summary>
        /// Only gray pixels will be hued
        /// </summary>
        PartialHue = 0x00040000,
        /// <summary>
        /// NoHouse or Unknown. Needs further research.
        /// </summary>
        NoHouse = 0x00080000,
        /// <summary>
        /// The tile is a map--in the cartography sense. Unknown usage.
        /// </summary>
        Map = 0x00100000,
        /// <summary>
        /// The tile is a container.
        /// </summary>
        Container = 0x00200000,
        /// <summary>
        /// The tile may be equipped.
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
        /// Gargoyles can fly over or NoDiagonal
        /// </summary>
        HoverOver = 0x02000000,
        /// <summary>
        /// NoDiagonal (Unknown3).
        /// </summary>
        NoDiagonal = 0x04000000,
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
        StairRight = 0x80000000,
        /// <summary>
        /// Blend Alphas, tile blending.
        /// </summary>
        AlphaBlend = 0x0100000000,
        /// <summary>
        /// Uses new art style? Something related to the nodraw tile?
        /// </summary>
        UseNewArt = 0x0200000000,
        /// <summary>
        /// Has art being used?
        /// </summary>
        ArtUsed = 0x0400000000,
        /// <summary>
        /// Unused8 ??
        /// </summary>
        Unused8 = 0x08000000000,
        /// <summary>
        /// Disallow shadow on this tile, lightsource? lava?
        /// </summary>
        NoShadow = 0x1000000000,
        /// <summary>
        /// Let pixels bleed in to other tiles? Is this Disabling Texture Clamp?
        /// </summary>
        PixelBleed = 0x2000000000,
        /// <summary>
        /// Play tile animation once.
        /// </summary>
        PlayAnimOnce = 0x4000000000,
        /// <summary>
        /// Movable multi? Cool ships and vehicles etc? Something related to the masts ???
        /// </summary>
        MultiMovable = 0x10000000000,
        /// <summary>
        /// Unused10
        /// </summary>
        Unused10 = 0x20000000000,
        /// <summary>
        /// Unused11
        /// </summary>
        Unused11 = 0x40000000000,
        /// <summary>
        /// Unused12
        /// </summary>
        Unused12 = 0x80000000000,
        /// <summary>
        /// Unused13
        /// </summary>
        Unused13 = 0x100000000000,
        /// <summary>
        /// Unused14
        /// </summary>
        Unused14 = 0x200000000000,
        /// <summary>
        /// Unused15
        /// </summary>
        Unused15 = 0x400000000000,
        /// <summary>
        /// Unused16
        /// </summary>
        Unused16 = 0x800000000000,
        /// <summary>
        /// Unused17
        /// </summary>
        Unused17 = 0x1000000000000,
        /// <summary>
        /// Unused18
        /// </summary>
        Unused18 = 0x2000000000000,
        /// <summary>
        /// Unused19
        /// </summary>
        Unused19 = 0x4000000000000,
        /// <summary>
        /// Unused20
        /// </summary>
        Unused20 = 0x8000000000000,
        /// <summary>
        /// Unused21
        /// </summary>
        Unused21 = 0x10000000000000,
        /// <summary>
        /// Unused22
        /// </summary>
        Unused22 = 0x20000000000000,
        /// <summary>
        /// Unused23
        /// </summary>
        Unused23 = 0x40000000000000,
        /// <summary>
        /// Unused24
        /// </summary>
        Unused24 = 0x80000000000000,
        /// <summary>
        /// Unused25
        /// </summary>
        Unused25 = 0x100000000000000,
        /// <summary>
        /// Unused26
        /// </summary>
        Unused26 = 0x200000000000000,
        /// <summary>
        /// Unused27
        /// </summary>
        Unused27 = 0x400000000000000,
        /// <summary>
        /// Unused28
        /// </summary>
        Unused28 = 0x800000000000000,
        /// <summary>
        /// Unused29
        /// </summary>
        Unused29 = 0x1000000000000000,
        /// <summary>
        /// Unused30
        /// </summary>
        Unused30 = 0x2000000000000000,
        /// <summary>
        /// Unused31
        /// </summary>
        Unused31 = 0x4000000000000000,
        /// <summary>
        /// Unused32
        /// </summary>
        Unused32 = 0x8000000000000000
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

        public static unsafe string ReadNameString(byte* buffer)
        {
            int count;
            for (count = 0; count < 20 && *buffer != 0; ++count)
            {
                _stringBuffer[count] = *buffer++;
            }

            return Encoding.ASCII.GetString(_stringBuffer, 0, count);
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

                    if (useNewTileDataFormat)
                    {
                        bin.Write((ulong)LandTable[i].Flags);
                    }
                    else
                    {
                        bin.Write((uint)LandTable[i].Flags);
                    }

                    bin.Write(LandTable[i].TextureID);
                    var b = new byte[20];
                    if (LandTable[i].Name != null)
                    {
                        byte[] bb = Encoding.ASCII.GetBytes(LandTable[i].Name);
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

                    if (useNewTileDataFormat)
                    {
                        bin.Write((ulong)ItemTable[i].Flags);
                    }
                    else
                    {
                        bin.Write((uint)ItemTable[i].Flags);
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
                        byte[] bb = Encoding.ASCII.GetBytes(ItemTable[i].Name);
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

        /// <summary>
        /// Exports <see cref="ItemData"/> to csv file
        /// </summary>
        /// <param name="fileName"></param>
        public static void ExportItemDataToCSV(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite), Encoding.GetEncoding(1252)))
            {
                tex.Write("ID;Name;Weight/Quantity;Layer/Quality;Gump/AnimID;Height;Hue;Class/Quantity;StackingOffset;miscData;Unknown2;Unknown3");

                string columnNames = GetTileFlagColumnNames();
                tex.Write($"{columnNames}\r\n");

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
                    tex.Write($";{tile.Unk2}");
                    tex.Write($";{tile.Unk3}");

                    Array enumValues = Enum.GetValues(typeof(TileFlag));
                    int maxLength = Art.IsUOAHS() ? enumValues.Length : (enumValues.Length / 2) + 1;
                    for (int t = 1; t < maxLength; ++t)
                    {
                        tex.Write($";{((tile.Flags & (TileFlag)enumValues.GetValue(t)) != 0 ? "1" : "0")}");
                    }
                    tex.Write("\r\n");
                }
            }
        }

        private static string GetTileFlagColumnNames()
        {
            string[] enumNames = Enum.GetNames(typeof(TileFlag));
            int maxLength = Art.IsUOAHS() ? enumNames.Length : (enumNames.Length / 2) + 1;
            string columnNames = string.Empty;
            for (int i = 1; i < maxLength; ++i)
            {
                columnNames += $";{enumNames[i]}";
            }

            return columnNames;
        }

        /// <summary>
        /// Exports <see cref="LandData"/> to csv file
        /// </summary>
        /// <param name="fileName"></param>
        public static void ExportLandDataToCSV(string fileName)
        {
            using (var tex = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite)))
            {
                tex.Write("ID;Name;TextureID");

                string columnNames = GetTileFlagColumnNames();
                tex.Write($"{columnNames}\r\n");

                for (int i = 0; i < LandTable.Length; ++i)
                {
                    LandData tile = LandTable[i];
                    tex.Write("0x{0:X4}", i);
                    tex.Write($";{tile.Name}");
                    tex.Write($";0x{tile.TextureID:X4}");

                    Array enumValues = Enum.GetValues(typeof(TileFlag));
                    int maxLength = Art.IsUOAHS() ? enumValues.Length : (enumValues.Length / 2) + 1;
                    for (int t = 1; t < maxLength; ++t)
                    {
                        tex.Write($";{((tile.Flags & (TileFlag)enumValues.GetValue(t)) != 0 ? "1" : "0")}");
                    }
                    tex.Write("\r\n");
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
                        if (split.Length < 44)
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
                        if (split.Length < 35)
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
        public readonly uint flags;
        public readonly ushort texID;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct NewLandTileDataMul
    {
        public readonly ulong flags;
        public readonly ushort texID;
        public fixed byte name[20];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct OldItemTileDataMul
    {
        public readonly uint flags;
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
        public readonly ulong flags;
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