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
using UoFiddler.Plugin.MultiEditor.Styles;

namespace UoFiddler.Plugin.MultiEditor.House
{
    internal sealed partial class HouseDraft
    {
        /// <summary>The door pieces the draft hangs by itself in a wall along X and in one along Y.</summary>
        public const string DoorAlongX = "Piece3";
        public const string DoorAlongY = "Piece5";

        public const int StoreyHeight = 20;
        public const int MaxStoreys = 4;
        public const string WallsLayer = "Walls";
        public const string FloorsLayer = "Floors";
        public const string StairsLayer = "Stairs";
        public const string FoundationLayer = "Foundation";

        /// <summary>How far below the ground floor the foundation of most client houses starts.</summary>
        public const int UsualFoundationHeight = 7;

        /// <summary>Z of the ground floor.</summary>
        public int BaseZ { get; set; }

        public HouseStyle[] WallStyles { get; } = new HouseStyle[MaxStoreys];

        public HouseStyle[] FloorStyles { get; } = new HouseStyle[MaxStoreys];

        public HouseStyle RoofStyle { get; set; }

        /// <summary>The floor style a flat roof is laid in; without one, the floor style of the wing's top storey.</summary>
        public HouseStyle FlatRoofStyle { get; set; }

        public bool Foundation { get; set; }

        /// <summary>The wall style the foundation ring is built in; without one, the client houses' own (light brick).</summary>
        public HouseStyle FoundationStyle { get; set; }

        /// <summary>Rows of foundation pieces, one on the other: the stone tower, keep and castle have two.</summary>
        public int FoundationTiers { get; set; } = 1;

        /// <summary>How far below the ground floor the foundation starts.</summary>
        public int FoundationHeight { get; set; } = UsualFoundationHeight;

        /// <summary>The door style of the house: what a theme sets, and what doors made for the rooms are.</summary>
        public HouseStyle DoorStyle { get; set; }

        /// <summary>The stair style of the house.</summary>
        public HouseStyle StairStyle { get; set; }

        /// <summary>Which gable ends of the house's roofs are closed, and with what.</summary>
        public RoofGables Gables { get; set; } = RoofGables.Front;

        public RoofGableFill GableFill { get; set; } = RoofGableFill.Caps;

        /// <summary>
        /// True when flat roofs get battlements, as the client's tower, keep and castle have them:
        /// two rows of the walls' roof edging around the roof and a third of every second piece.
        /// </summary>
        public bool Parapet { get; set; }

        /// <summary>True when the doorways between rooms get a door of <see cref="DoorStyle"/>.</summary>
        public bool AutoDoors { get; set; }

        public FrontDoorSide FrontDoor { get; set; }

        /// <summary>Cells between the windows the draft puts along the outer walls; 0 for none.</summary>
        public int WindowSpacing { get; set; }

        /// <summary>Wall cells where a door or window the draft made by itself was taken out by hand.</summary>
        public HashSet<(int Storey, int X, int Y)> Suppressed { get; } = new HashSet<(int, int, int)>();

        /// <summary>Most rooms a wing can be divided into.</summary>
        public const int MaxRooms = 6;

        // Wall edges added and taken away by hand, per storey: cells with a wall on their +Y edge
        // (south) and on their +X edge (east).
        public HashSet<(int X, int Y)>[] AddedSouth { get; } = Sets();

        public HashSet<(int X, int Y)>[] AddedEast { get; } = Sets();

        public HashSet<(int X, int Y)>[] ErasedSouth { get; } = Sets();

        public HashSet<(int X, int Y)>[] ErasedEast { get; } = Sets();

        /// <summary>Doors by storey and wall cell: the door style and its piece (hinge variant).</summary>
        public Dictionary<(int Storey, int X, int Y), (HouseStyle Style, string Piece)> Doors { get; } = new Dictionary<(int, int, int), (HouseStyle, string)>();

        /// <summary>Wall cells whose piece is the style's window.</summary>
        public HashSet<(int Storey, int X, int Y)> Windows { get; } = new HashSet<(int, int, int)>();

        /// <summary>Floor areas laid in another style than the storey's; later ones lie on top.</summary>
        public List<(int Storey, int X0, int Y0, int X1, int Y1, HouseStyle Style)> FloorAreas { get; } = new List<(int, int, int, int, int, HouseStyle)>();

        /// <summary>Stairs by storey: the cell of the bottom step, the stair column (MultiNorth ...) and the style.</summary>
        public List<(int Storey, int X, int Y, string Column, HouseStyle Style)> Stairs { get; } = new List<(int, int, int, string, HouseStyle)>();

        private static HashSet<(int X, int Y)>[] Sets()
        {
            var sets = new HashSet<(int X, int Y)>[MaxStoreys];
            for (int i = 0; i < MaxStoreys; ++i)
            {
                sets[i] = new HashSet<(int X, int Y)>();
            }

            return sets;
        }

        /// <summary>A copy that shares nothing but the styles.</summary>
        public HouseDraft Clone()
        {
            HouseDraft copy = Fresh();
            copy.BaseZ = BaseZ;
            for (int k = 0; k < MaxStoreys; ++k)
            {
                copy.AddedSouth[k].UnionWith(AddedSouth[k]);
                copy.AddedEast[k].UnionWith(AddedEast[k]);
                copy.ErasedSouth[k].UnionWith(ErasedSouth[k]);
                copy.ErasedEast[k].UnionWith(ErasedEast[k]);
            }

            foreach (var door in Doors)
            {
                copy.Doors[door.Key] = door.Value;
            }

            copy.Windows.UnionWith(Windows);
            copy.Suppressed.UnionWith(Suppressed);
            copy.FloorAreas.AddRange(FloorAreas);
            copy.Stairs.AddRange(Stairs);
            return copy;
        }

        /// <summary>Puts another style in the place of every style the draft holds: after the styles were read again.</summary>
        public void Remap(Func<HouseStyle, HouseStyle> successor)
        {
            for (int k = 0; k < MaxStoreys; ++k)
            {
                WallStyles[k] = successor(WallStyles[k]);
                FloorStyles[k] = successor(FloorStyles[k]);
            }

            RoofStyle = successor(RoofStyle);
            FlatRoofStyle = successor(FlatRoofStyle);
            FoundationStyle = successor(FoundationStyle);
            DoorStyle = successor(DoorStyle);
            StairStyle = successor(StairStyle);
            foreach (var key in new List<(int Storey, int X, int Y)>(Doors.Keys))
            {
                Doors[key] = (successor(Doors[key].Style), Doors[key].Piece);
            }

            for (int i = 0; i < FloorAreas.Count; ++i)
            {
                var a = FloorAreas[i];
                FloorAreas[i] = (a.Storey, a.X0, a.Y0, a.X1, a.Y1, successor(a.Style));
            }

            for (int i = 0; i < Stairs.Count; ++i)
            {
                var s = Stairs[i];
                Stairs[i] = (s.Storey, s.X, s.Y, s.Column, successor(s.Style));
            }

            _pieces.Clear();
        }

        /// <summary>A draft with the same materials and nothing changed by hand: the next house.</summary>
        public HouseDraft Fresh()
        {
            var copy = new HouseDraft
            {
                RoofStyle = RoofStyle, FlatRoofStyle = FlatRoofStyle, Foundation = Foundation,
                FoundationStyle = FoundationStyle, FoundationTiers = FoundationTiers, FoundationHeight = FoundationHeight,
                DoorStyle = DoorStyle, StairStyle = StairStyle,
                AutoDoors = AutoDoors, FrontDoor = FrontDoor, WindowSpacing = WindowSpacing,
                Gables = Gables, GableFill = GableFill, Parapet = Parapet
            };
            Array.Copy(WallStyles, copy.WallStyles, MaxStoreys);
            Array.Copy(FloorStyles, copy.FloorStyles, MaxStoreys);
            return copy;
        }
    }
}
