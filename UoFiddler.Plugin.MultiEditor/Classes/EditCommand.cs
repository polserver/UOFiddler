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

namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// The saved fields of a tile, used to put a changed tile back.
    /// </summary>
    internal readonly record struct TileState(ushort Id, int X, int Y, int Z, bool Invisible)
    {
        public static TileState Of(MultiTile tile) => new(tile.Id, tile.X, tile.Y, tile.Z, tile.Invisible);

        public void ApplyTo(MultiTile tile)
        {
            tile.Id = Id;
            tile.Invisible = Invisible;
            tile.X = X;
            tile.Y = Y;
            tile.Z = Z;
        }
    }

    /// <summary>
    /// One undo step: the tiles an edit added, removed and changed, and the multi size before and
    /// after. Tiles are kept as the same objects, so references to them survive undo and redo.
    /// </summary>
    internal sealed class EditCommand
    {
        public EditCommand(string action, int width, int height)
        {
            Action = action;
            OldWidth = NewWidth = width;
            OldHeight = NewHeight = height;
        }

        public string Action { get; }

        public HashSet<MultiTile> Added { get; } = new();

        public HashSet<MultiTile> Removed { get; } = new();

        /// <summary>
        /// State of each changed tile before the edit. Tiles added by the same edit are not listed.
        /// </summary>
        public Dictionary<MultiTile, TileState> Before { get; } = new();

        /// <summary>
        /// State of each changed tile after the edit, filled in when the edit ends.
        /// </summary>
        public Dictionary<MultiTile, TileState> After { get; } = new();

        public int OldWidth { get; }

        public int OldHeight { get; }

        public int NewWidth { get; set; }

        public int NewHeight { get; set; }

        public bool IsEmpty => Added.Count == 0 && Removed.Count == 0 && Before.Count == 0 &&
                               OldWidth == NewWidth && OldHeight == NewHeight;

        public void CaptureAfter()
        {
            After.Clear();
            foreach (MultiTile tile in new List<MultiTile>(Before.Keys))
            {
                TileState after = TileState.Of(tile);

                // A tile changed and changed back is no change at all.
                if (after == Before[tile])
                {
                    Before.Remove(tile);
                }
                else
                {
                    After[tile] = after;
                }
            }
        }
    }
}
