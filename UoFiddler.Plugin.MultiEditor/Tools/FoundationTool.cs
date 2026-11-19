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
using UoFiddler.Plugin.MultiEditor.House;
using UoFiddler.Plugin.MultiEditor.Model;
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// A house foundation like the client's customizable house multis: the dragged rectangle gets
    /// a ring of 5 high foundation walls at the base Z and a dirt floor 7 above it inside.
    /// </summary>
    internal sealed class FoundationTool : PlaneDragTool
    {
        // The foundation multis use Light Brick Quarter Walls (0x63-0x66) and the dirt floor.
        public const ushort FoundationPost = 0x0066;
        private const ushort _dirt = 0x31F4;
        private const int _floorAbove = 7;

        public FoundationTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Wall;

        protected override string ActionName => "Foundation";

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            var planner = new WallPlanner(Context.CompList, Context.HouseStyles, Style, z);
            var (south, east) = WallPlanner.DragEdges(startX, startY, endX, endY, line: false);
            var (remove, add) = planner.Plan(south, east, erase: false, Context.CompList.Width, Context.CompList.Height);

            // The foundation multis floor every cell with the same dirt piece.
            for (int x = Math.Min(startX, endX) + 1; x <= Math.Max(startX, endX); ++x)
            {
                for (int y = Math.Min(startY, endY) + 1; y <= Math.Max(startY, endY); ++y)
                {
                    if (Context.CompList.InBounds(x, y))
                    {
                        add.Add((_dirt, x, y, z + _floorAbove));
                    }
                }
            }

            return (remove, add);
        }
    }
}
