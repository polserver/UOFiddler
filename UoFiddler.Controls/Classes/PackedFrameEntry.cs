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

using System.Text.Json.Serialization;

namespace UoFiddler.Controls.Classes
{
    public class PackedFrameEntry
    {
        [JsonPropertyName("direction")] public int Direction { get; set; }
        [JsonPropertyName("index")] public int Index { get; set; }
        [JsonPropertyName("frame")] public Rect Frame { get; set; }
        [JsonPropertyName("center")] public PointStruct Center { get; set; }
    }
}
