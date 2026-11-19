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
using System.Linq;
using Ultima;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Places the chosen style's stair multi: press on the bottom step and drag the way the stairs
    /// should climb (a click climbs north). The stairs rise one level (20).
    /// </summary>
    internal sealed class StairTool : PlaneDragTool
    {
        private const ushort _noDraw = 0x0001;

        public StairTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Stair;

        protected override string ActionName => "Stairs";

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            return (new List<MultiTile>(), Tiles(Style, Column(endX - startX, endY - startY), startX, startY, z));
        }

        /// <summary>The stair column for the way a drag points: the stairs climb that way, north for a click.</summary>
        public static string Column(int dx, int dy)
        {
            return Math.Abs(dx) > Math.Abs(dy)
                ? (dx > 0 ? "MultiEast" : "MultiWest")
                : (dy > 0 ? "MultiSouth" : "MultiNorth");
        }

        /// <summary>The tiles of a style's stairs with their lowest step in a cell, climbing one level from <paramref name="z"/>.</summary>
        public static List<(ushort Id, int X, int Y, int Z)> Tiles(HouseStyle style, string column, int startX, int startY, int z)
        {
            var add = new List<(ushort, int, int, int)>();
            ushort multiId = style.Get(column);
            MultiComponentList stairs = multiId != 0 ? Multis.GetComponents(multiId) : MultiComponentList.Empty;
            if (stairs == MultiComponentList.Empty || stairs.Width == 0)
            {
                return add;
            }

            // The lowest step goes under the cursor.
            (int X, int Y) bottom = (0, 0);
            int lowest = int.MaxValue;
            for (int x = 0; x < stairs.Width; ++x)
            {
                for (int y = 0; y < stairs.Height; ++y)
                {
                    int top = stairs.Tiles[x][y].Where(t => t.Id != _noDraw).Select(t => t.Z + TileData.ItemTable[t.Id].Height).DefaultIfEmpty(int.MaxValue).Max();
                    if (top < lowest)
                    {
                        lowest = top;
                        bottom = (x, y);
                    }
                }
            }

            for (int x = 0; x < stairs.Width; ++x)
            {
                for (int y = 0; y < stairs.Height; ++y)
                {
                    foreach (MTile tile in stairs.Tiles[x][y])
                    {
                        if (tile.Id != _noDraw)
                        {
                            add.Add((tile.Id, startX + x - bottom.X, startY + y - bottom.Y, z + tile.Z));
                        }
                    }
                }
            }

            return add;
        }
    }
}
