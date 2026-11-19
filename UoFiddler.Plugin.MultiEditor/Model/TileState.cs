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

namespace UoFiddler.Plugin.MultiEditor.Model
{
    /// <summary>
    /// The saved fields of a tile, used to put a changed tile back.
    /// </summary>
    internal readonly record struct TileState(ushort Id, int X, int Y, int Z, bool Invisible, TileLayer Layer)
    {
        public static TileState Of(MultiTile tile) => new(tile.Id, tile.X, tile.Y, tile.Z, tile.Invisible, tile.Layer);

        public void ApplyTo(MultiTile tile)
        {
            tile.Id = Id;
            tile.Invisible = Invisible;
            tile.Layer = Layer;
            tile.X = X;
            tile.Y = Y;
            tile.Z = Z;
        }
    }
}
