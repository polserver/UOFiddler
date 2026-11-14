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
using System.Windows.Forms;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Tools;
using UoFiddler.Plugin.MultiEditor.UI;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>An area of a facet to copy statics from.</summary>
    internal sealed record MapArea(Map Map, int X, int Y, int Width, int Height, int MinZ, int MaxZ, bool NewDesign);

    /// <summary>
    /// Copies the statics of a map area into multi tiles, so a building on the map can be edited
    /// as a design. Land tiles are not copied.
    /// </summary>
    internal static class MapExtract
    {
        public static readonly (string Name, Func<Map> Map)[] Facets =
        {
            ("Felucca", () => Map.Felucca),
            ("Trammel", () => Map.Trammel),
            ("Ilshenar", () => Map.Ilshenar),
            ("Malas", () => Map.Malas),
            ("Tokuno", () => Map.Tokuno),
            ("Ter Mur", () => Map.TerMur)
        };

        /// <summary>
        /// The statics in the area between the Z limits, placed relative to the area's corner
        /// and lowered so the lowest one sits at Z 0.
        /// </summary>
        public static List<ClipboardTile> Extract(MapArea area)
        {
            var found = new List<(ushort Id, int X, int Y, int Z)>();
            int x1 = Math.Min(area.Map.Width, area.X + area.Width);
            int y1 = Math.Min(area.Map.Height, area.Y + area.Height);
            for (int x = Math.Max(0, area.X); x < x1; ++x)
            {
                for (int y = Math.Max(0, area.Y); y < y1; ++y)
                {
                    foreach (HuedTile tile in area.Map.Tiles.GetStaticTiles(x, y))
                    {
                        if (tile.Z >= area.MinZ && tile.Z <= area.MaxZ)
                        {
                            found.Add((tile.Id, x - area.X, y - area.Y, tile.Z));
                        }
                    }
                }
            }

            var result = new List<ClipboardTile>(found.Count);
            if (found.Count == 0)
            {
                return result;
            }

            int minZ = int.MaxValue;
            foreach (var t in found)
            {
                minZ = Math.Min(minZ, t.Z);
            }

            foreach (var t in found)
            {
                result.Add(new ClipboardTile(t.Id, t.X, t.Y, t.Z - minZ, false, 0));
            }

            return result;
        }

        /// <summary>
        /// Asks for the area, starting from <paramref name="initial"/>. Returns null when cancelled.
        /// </summary>
        public static MapArea Ask(IWin32Window owner, MapArea initial, bool canPlaceIntoDesign)
        {
            using var form = new MapExtractForm(initial, canPlaceIntoDesign);
            return form.ShowDialog(owner) == DialogResult.OK ? form.Area : null;
        }
    }
}
