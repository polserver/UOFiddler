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
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Temporary tool for a paste, a blueprint or a duplicate: shows the tiles under the cursor and
    /// places them on every click, each click its own undo step. Esc or a right click leaves it.
    /// </summary>
    internal sealed class PasteTool : EditorTool
    {
        private IReadOnlyList<ClipboardTile> _tiles = new List<ClipboardTile>();
        private string _action = "Paste";

        public PasteTool(IEditorContext context) : base(context)
        {
        }

        public override bool TracksCursorCell => true;

        /// <param name="tiles">The tiles to place.</param>
        /// <param name="action">Name of the undo step each placement makes.</param>
        public void Begin(IReadOnlyList<ClipboardTile> tiles, string action)
        {
            _tiles = tiles;
            _action = action;
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            Context.CellAt(Context.MouseLoc, out int x, out int y, out int z);
            Context.CompList.TileAddBatch(_tiles.Select(t => (t.Id, x + t.Dx, y + t.Dy, z + t.Dz, t.Invisible, t.Extra)).ToList(), _action);
            Context.AfterEdit();
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            Context.CellAt(Context.MouseLoc, out int x, out int y, out int z);
            foreach (ClipboardTile tile in _tiles)
            {
                overlay.Ghost(tile.Id, x + tile.Dx, y + tile.Dy, z + tile.Dz);
            }
        }
    }
}
