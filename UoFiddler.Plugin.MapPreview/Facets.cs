/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MapPreview
{
    /// <summary>
    /// The six facets by index. A preview keeps the index, not the <see cref="Map"/>, because a
    /// client reload can put a new instance in the static field.
    /// </summary>
    internal static class Facets
    {
        public const int Count = 6;

        /// <summary>The facet's index, or -1 for a map that is not one of the six.</summary>
        public static int IndexOf(Map map)
        {
            for (int i = 0; i < Count; i++)
            {
                if (ReferenceEquals(Get(i), map))
                {
                    return i;
                }
            }

            return -1;
        }

        public static Map Get(int index)
        {
            return index switch
            {
                0 => Map.Felucca,
                1 => Map.Trammel,
                2 => Map.Ilshenar,
                3 => Map.Malas,
                4 => Map.Tokuno,
                5 => Map.TerMur,
                _ => null
            };
        }

        /// <summary>The name the map tab shows, which the user can change in the options.</summary>
        public static string Name(int index, Map map)
        {
            string[] names = Options.MapNames;
            return index >= 0 && names != null && index < names.Length && !string.IsNullOrEmpty(names[index])
                ? names[index]
                : $"Map{map?.FileIndex}";
        }
    }
}
