/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Plugin.MultiEditor.Model
{
    /// <summary>
    /// A layer as the layers file keeps it. <see cref="InFile"/> is false for a layer that was
    /// left out of the saved multi, so its tiles exist only here.
    /// </summary>
    internal sealed record SavedLayer(string Name, bool Visible, bool InFile,
        List<(ushort Id, int X, int Y, int Z, bool Invisible, int Extra)> Tiles);
}
