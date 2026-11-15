/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// A named group of tiles in a design. Every tile is on exactly one layer and new tiles go on
    /// the active one. Layers only exist in the editor: the client's multi has no notion of them.
    /// </summary>
    public sealed class TileLayer
    {
        public TileLayer(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        /// <summary>Hidden layers are not drawn, hovered, selected or removed. View only: not an undo step.</summary>
        public bool Visible { get; set; } = true;

        public override string ToString() => Name;
    }

    /// <summary>
    /// A layer as the layers file keeps it. <see cref="InFile"/> is false for a layer that was
    /// left out of the saved multi, so its tiles exist only here.
    /// </summary>
    internal sealed record SavedLayer(string Name, bool Visible, bool InFile,
        List<(ushort Id, int X, int Y, int Z, bool Invisible, int Extra)> Tiles);
}
