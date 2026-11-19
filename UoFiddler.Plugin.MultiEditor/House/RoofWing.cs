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

namespace UoFiddler.Plugin.MultiEditor.House
{
    /// <summary>
    /// One rectangle of a roof or a house being shaped. For a roof the box is the cells the roof
    /// covers and <see cref="EaveZ"/> the height of its eaves; for a house the box is the wall
    /// cells and <see cref="EaveZ"/> the Z its ground floor stands on.
    /// </summary>
    internal sealed class RoofWing
    {
        public int X0 { get; set; }

        public int Y0 { get; set; }

        public int X1 { get; set; }

        public int Y1 { get; set; }

        public int EaveZ { get; set; }

        /// <summary>Storeys of a house wing.</summary>
        public int Storeys { get; set; } = 1;

        /// <summary>A house wing left without a roof.</summary>
        public bool NoRoof { get; set; }

        /// <summary>
        /// Rooms a house wing is divided into on each of its storeys: partitions across its longer
        /// side, each with a doorway left in the middle. 1 leaves the wing undivided.
        /// </summary>
        public int Rooms { get; set; } = 1;

        /// <summary>How the rooms of a house wing are laid out.</summary>
        public RoomLayout Layout { get; set; }

        /// <summary>
        /// True for a house wing that keeps its walls where it meets other wings, with a doorway
        /// in each stretch of wall they share: a tower at a keep's corner, a hall in a castle's
        /// yard. Wings without it run into each other as one open space.
        /// </summary>
        public bool Walled { get; set; }

        /// <summary>
        /// Rooms and layout of the storeys above the ground floor that have their own; a storey
        /// without takes those of the storey below it. Index 0 is not used.
        /// </summary>
        public (int Rooms, RoomLayout Layout)?[] RoomsAbove { get; private set; } = new (int, RoomLayout)?[HouseDraft.MaxStoreys];

        /// <summary>Rooms and layout of a storey.</summary>
        public (int Rooms, RoomLayout Layout) RoomsOn(int storey)
        {
            for (int k = Math.Min(storey, HouseDraft.MaxStoreys - 1); k >= 1; --k)
            {
                if (RoomsAbove[k] is { } own)
                {
                    return own;
                }
            }

            return (Rooms, Layout);
        }

        public RoofShape Shape { get; set; }

        public bool RidgeAlongY { get; set; }

        /// <summary>True while the ridge follows the box's proportions as it is resized.</summary>
        public bool RidgeAuto { get; set; }

        public (int X0, int Y0, int X1, int Y1) Box => (X0, Y0, X1, Y1);

        public RoofWing Clone()
        {
            var copy = (RoofWing)MemberwiseClone();
            copy.RoomsAbove = ((int, RoomLayout)?[])RoomsAbove.Clone();
            return copy;
        }

        public void CopyFrom(RoofWing other)
        {
            (X0, Y0, X1, Y1, EaveZ) = (other.X0, other.Y0, other.X1, other.Y1, other.EaveZ);
            (Shape, RidgeAlongY, RidgeAuto, Storeys, NoRoof) = (other.Shape, other.RidgeAlongY, other.RidgeAuto, other.Storeys, other.NoRoof);
            Rooms = other.Rooms;
            Layout = other.Layout;
            Walled = other.Walled;
            RoomsAbove = ((int, RoomLayout)?[])other.RoomsAbove.Clone();
        }
    }
}
