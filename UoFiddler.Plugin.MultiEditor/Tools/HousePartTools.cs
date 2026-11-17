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
using UoFiddler.Plugin.MultiEditor.Classes;

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
            ushort[] pieces = Style.Slots.Values.Distinct().ToArray();
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
