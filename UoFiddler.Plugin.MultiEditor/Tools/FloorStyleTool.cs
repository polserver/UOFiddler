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
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Fills the dragged rectangle with a floor style, each cell a random piece of the style.
    /// Flat tiles already at that Z in those cells are replaced.
    /// </summary>
    internal sealed class FloorStyleTool : PlaneDragTool
    {
        private int _seed = Environment.TickCount;

        public FloorStyleTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Floor;

        protected override string ActionName => "Floor";

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            ushort[] pieces = Style.Slots.Values.Where(id => HouseStyle.InTileData(id)).Distinct().ToArray();
            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            MultiEditorComponentList list = Context.CompList;

            for (int x = Math.Min(startX, endX); x <= Math.Max(startX, endX); ++x)
            {
                for (int y = Math.Min(startY, endY); y <= Math.Max(startY, endY); ++y)
                {
                    if (!list.InBounds(x, y))
                    {
                        continue;
                    }

                    // Same seed for preview and edit, so what is shown is what lands.
                    ushort id = pieces[(int)((uint)HashCode.Combine(x, y, _seed) % (uint)pieces.Length)];
                    remove.AddRange(list.GetTilesAt(x, y).Where(t => t.Z == z && t.Height == 0 && list.IsShown(t)));
                    add.Add((id, x, y, z));
                }
            }

            if (final)
            {
                _seed = Environment.TickCount ^ _seed;
            }

            return (remove, add);
        }
    }
}
