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
using System.Text.Json.Serialization;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// JSON descriptor of a packed animation sprite sheet: where each frame of each direction sits on
    /// the sheet image and the frame's center.
    /// </summary>
    public class PackedAnimation
    {
        [JsonPropertyName("meta")] public PackedMeta Meta { get; set; }
        [JsonPropertyName("frames")] public List<PackedFrameEntry> Frames { get; set; }
    }

    /// <summary>
    /// JSON descriptor of a packed item (static art) sprite sheet.
    /// </summary>
    public class PackedItems
    {
        [JsonPropertyName("meta")] public PackedMeta Meta { get; set; }
        [JsonPropertyName("items")] public List<PackedItemEntry> Items { get; set; }
    }

    public class PackedMeta
    {
        /// <summary>Sheet image file name, next to the JSON file.</summary>
        [JsonPropertyName("image")] public string Image { get; set; }
        [JsonPropertyName("size")] public PackedSize Size { get; set; }
        [JsonPropertyName("format")] public string Format { get; set; }
    }

    public class PackedFrameEntry
    {
        [JsonPropertyName("direction")] public int Direction { get; set; }
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("frame")] public PackedRect Frame { get; set; }
        [JsonPropertyName("center")] public PackedPoint Center { get; set; }
    }

    public class PackedItemEntry
    {
        /// <summary>Item (static art) id.</summary>
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("frame")] public PackedRect Frame { get; set; }
    }

    public class PackedRect
    {
        [JsonPropertyName("x")] public int X { get; set; }
        [JsonPropertyName("y")] public int Y { get; set; }
        [JsonPropertyName("w")] public int W { get; set; }
        [JsonPropertyName("h")] public int H { get; set; }
    }

    public class PackedPoint
    {
        [JsonPropertyName("x")] public int X { get; set; }
        [JsonPropertyName("y")] public int Y { get; set; }
    }

    public class PackedSize
    {
        [JsonPropertyName("w")] public int W { get; set; }
        [JsonPropertyName("h")] public int H { get; set; }
    }
}
