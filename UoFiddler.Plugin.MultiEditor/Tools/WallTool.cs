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
using UoFiddler.Plugin.MultiEditor.Classes;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Draws walls of the chosen wall style. Dragging a rectangle walls in a room (the rectangle
    /// is the outer wall cells); dragging along a row or column, or with Shift, draws one straight
    /// wall. Ctrl erases walls of the style instead. Corners, posts and piece variants are chosen
    /// from the surrounding walls, and existing windows and doors are kept.
    /// </summary>
    internal sealed class WallTool : PlaneDragTool
    {
        public WallTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Wall;

        protected override string ActionName => CtrlHeld ? "Erase Walls" : "Draw Walls";

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            var planner = new WallPlanner(Context.CompList, Context.HouseStyles, Style, z);
            var (south, east) = WallPlanner.DragEdges(startX, startY, endX, endY, ShiftHeld);
            return planner.Plan(south, east, CtrlHeld, Context.CompList.Width, Context.CompList.Height);
        }
    }
}
