// /***************************************************************************
//  *
//  * $Author: Turley
//  *
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

namespace Ultima.Helpers
{
    public sealed class MapHelper
    {
        /// <summary>
        /// Checks if map1.mul exists and sets <see cref="Ultima.Map"/>
        /// </summary>
        public static void CheckForNewMapSize()
        {
            if (Files.GetFilePath("map1.mul") != null || Files.GetFilePath("map1legacymul.uop") != null)
            {
                Map.Trammel = Map.Trammel.Width == 7168
                    ? new Map(1, 1, 7168, 4096)
                    : new Map(1, 1, 6144, 4096);
            }
            else
            {
                Map.Trammel = Map.Trammel.Width == 7168
                    ? new Map(0, 1, 7168, 4096)
                    : new Map(0, 1, 6144, 4096);
            }
        }
    }
}