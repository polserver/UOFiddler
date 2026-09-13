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
    public class PackedMeta
    {
        [JsonPropertyName("image")] public string Image { get; set; }
        [JsonPropertyName("size")] public SizeStruct Size { get; set; }
        [JsonPropertyName("format")] public string Format { get; set; }
    }
}
