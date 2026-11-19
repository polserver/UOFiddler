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
    /// Puts a door of the chosen style into the wall in the clicked cell, facing along the wall.
    /// Clicking a door again turns it to its next hinge variant.
    /// </summary>
    internal sealed class DoorTool : PlaneDragTool
    {
        // Hinge variants per wall direction, in the order a click cycles through them.
        private static readonly string[] _alongX = { "Piece3", "Piece1", "Piece4", "Piece2" };
        private static readonly string[] _alongY = { "Piece5", "Piece7", "Piece6", "Piece8" };

        public DoorTool(IEditorContext context) : base(context)
        {
        }

        protected override HousePartKind Kind => HousePartKind.Door;

        protected override string ActionName => "Door";

        /// <summary>
        /// The hovered wall or door when there is one: a click on a wall means that wall, not the
        /// floor cell behind it.
        /// </summary>
        protected override void BaseCell(out int x, out int y, out int z)
        {
            if (Context.HoverTile is { } hover && Context.HouseStyles.TryGetRole(hover.Id, out HouseStyle owner, out _) &&
                owner.Kind is HousePartKind.Wall or HousePartKind.Door)
            {
                (x, y, z) = (hover.X, hover.Y, hover.Z);
                return;
            }

            base.BaseCell(out x, out y, out z);
        }

        protected override (List<MultiTile> Remove, List<(ushort Id, int X, int Y, int Z)> Add) Plan(
            int startX, int startY, int endX, int endY, int z, bool final)
        {
            var remove = new List<MultiTile>();
            var add = new List<(ushort, int, int, int)>();
            MultiEditorComponentList list = Context.CompList;
            if (!list.InBounds(startX, startY))
            {
                return (remove, add);
            }

            // Default: a wall along X. A wall piece or door already in the cell decides the direction.
            string[] variants = _alongX;
            int next = 0;
            foreach (MultiTile tile in list.GetTilesAt(startX, startY))
            {
                if (tile.Z != z || !Context.HouseStyles.TryGetRole(tile.Id, out HouseStyle owner, out string slot))
                {
                    continue;
                }

                if (owner.Kind == HousePartKind.Door)
                {
                    variants = WallPlanner.DoorAlongX(slot) ? _alongX : _alongY;
                    next = owner == Style ? (Array.IndexOf(variants, slot) + 1) % variants.Length : 0;
                    remove.Add(tile);
                }
                else if (owner.Kind == HousePartKind.Wall)
                {
                    if (slot is "Corner" or "Post")
                    {
                        // No door fits a corner.
                        return (new List<MultiTile>(), add);
                    }

                    variants = slot.StartsWith("East", StringComparison.Ordinal) || slot.EndsWith('E') ? _alongY : _alongX;
                    remove.Add(tile);
                }
            }

            // Fall back to any piece the style has, in cycle order.
            for (int i = 0; i < variants.Length; ++i)
            {
                ushort id = Style.Get(variants[(next + i) % variants.Length]);
                if (id != 0)
                {
                    add.Add((id, startX, startY, z));
                    break;
                }
            }

            return (remove, add);
        }
    }
}
