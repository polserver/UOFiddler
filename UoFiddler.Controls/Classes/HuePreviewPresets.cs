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

using System.Collections.Generic;
using Ultima;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// What the hue editor previews: an item, a gump and an animation body. -1 means "none".
    /// </summary>
    public sealed class HuePreviewSubject
    {
        public HuePreviewSubject(string name, int itemId, int gumpId, int bodyId)
        {
            Name = name;
            ItemId = itemId;
            GumpId = gumpId;
            BodyId = bodyId;
        }

        public string Name { get; }

        public int ItemId { get; }

        public int GumpId { get; }

        public int BodyId { get; }

        public override string ToString() => Name;
    }

    /// <summary>The preview presets offered by the hue editor.</summary>
    public static class HuePreviewPresets
    {
        public const string CustomName = "Custom";

        private const int _maleGumpOffset = 50000;

        /// <summary>
        /// Presets that wear an item use the item's tiledata animation for the gump and the body,
        /// so they follow whatever the loaded client has. Checked against 6.0.14 and 7.0.114 clients.
        /// </summary>
        public static IReadOnlyList<HuePreviewSubject> Build()
        {
            return
            [
                Wearable("Cloth (robe)", 0x1F03),
                Wearable("Cloth (cloak)", 0x1515),
                Wearable("Cloth (shirt)", 0x1517),
                Wearable("Metal (platemail)", 0x1415),
                Wearable("Leather (tunic)", 0x13CC),
                Wearable("Hair (long)", 0x203C),
                new HuePreviewSubject("Skin (human)", -1, 0x000C, 400),
            ];
        }

        /// <summary>The paperdoll gump and body animation an item is drawn with, or -1.</summary>
        public static (int GumpId, int BodyId) WornAppearance(int itemId)
        {
            if (itemId < 0 || TileData.ItemTable == null || itemId >= TileData.ItemTable.Length)
            {
                return (-1, -1);
            }

            int animation = TileData.ItemTable[itemId].Animation;
            if (animation <= 0)
            {
                return (-1, -1);
            }

            return (_maleGumpOffset + animation, animation);
        }

        /// <summary>True when the item's tiledata says only its grey pixels take a hue.</summary>
        public static bool IsPartialHue(int itemId)
        {
            return itemId >= 0
                && TileData.ItemTable != null
                && itemId < TileData.ItemTable.Length
                && (TileData.ItemTable[itemId].Flags & TileFlag.PartialHue) != 0;
        }

        private static HuePreviewSubject Wearable(string name, int itemId)
        {
            (int gumpId, int bodyId) = WornAppearance(itemId);
            return new HuePreviewSubject(name, itemId, gumpId, bodyId);
        }
    }
}
