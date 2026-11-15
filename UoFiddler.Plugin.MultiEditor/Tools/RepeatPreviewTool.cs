/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// Temporary tool while the repeat dialog is open: shows the copies it would add. Clicks do nothing.
    /// </summary>
    internal sealed class RepeatPreviewTool : EditorTool
    {
        /// <summary>Most tiles shown as ghosts; a larger repeat is still made, only not previewed.</summary>
        public const int MaxGhosts = 20000;

        private IReadOnlyList<(ushort id, int x, int y, int z, bool invisible, int extra)> _tiles =
            new List<(ushort, int, int, int, bool, int)>();

        public RepeatPreviewTool(IEditorContext context) : base(context)
        {
        }

        public void Show(IReadOnlyList<(ushort id, int x, int y, int z, bool invisible, int extra)> tiles)
        {
            _tiles = tiles;
        }

        public override void PaintOverlay(IToolOverlay overlay)
        {
            if (_tiles.Count > MaxGhosts)
            {
                return;
            }

            foreach ((ushort id, int x, int y, int z, _, _) in _tiles)
            {
                overlay.Ghost(id, x, y, z);
            }
        }

        public override void Cancel()
        {
            _tiles = new List<(ushort, int, int, int, bool, int)>();
        }
    }
}
