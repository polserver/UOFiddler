/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Plugin.MultiEditor.Model;

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    internal sealed partial class EditorSession
    {
        // Shared by every open document, so a copy in one tab pastes into another.
        private static List<ClipboardTile> _clipboard;

        // ---------------------------------------------------------------- clipboard

        /// <summary>
        /// Copies the selection, relative to its lowest X, Y and Z, for pasting in any document and
        /// as multi text lines to the Windows clipboard, so another editor or a text file can take it.
        /// </summary>
        public bool Copy()
        {
            if (_selection.Count == 0)
            {
                return false;
            }

            _clipboard = ToClipboard(_selection);
            try
            {
                Clipboard.SetText(ClipboardTile.Format(_clipboard));
            }
            catch (ExternalException)
            {
                // Clipboard busy in another program: the editor's own copy still works.
            }

            return true;
        }

        public void Cut()
        {
            if (Copy())
            {
                DeleteSelection();
            }
        }

        /// <summary>Starts placing the clipboard tiles with the mouse.</summary>
        public void Paste()
        {
            BeginPaste(ReadClipboard());
        }

        public bool CanPaste => _clipboard != null || ClipboardHasTiles();

        /// <summary>Puts a copy of the selection on the cursor to place with the mouse. The clipboard is left alone.</summary>
        public void Duplicate()
        {
            BeginPaste(SelectionAsClipboard(), "Duplicate");
        }

        /// <summary>The selection as tiles relative to its lowest X, Y and Z (also used to save a blueprint).</summary>
        public List<ClipboardTile> SelectionAsClipboard()
        {
            return _selection.Count == 0 ? null : ToClipboard(_selection);
        }

        /// <summary>Every tile of the design, relative to its lowest X, Y and Z; null when it is empty.</summary>
        public List<ClipboardTile> DesignAsClipboard()
        {
            List<MultiTile> tiles = CompList.RealTiles.ToList();
            return tiles.Count == 0 ? null : ToClipboard(tiles);
        }

        private static List<ClipboardTile> ToClipboard(IReadOnlyCollection<MultiTile> tiles)
        {
            int ox = tiles.Min(t => t.X);
            int oy = tiles.Min(t => t.Y);
            int oz = tiles.Min(t => t.Z);
            return tiles
                .OrderBy(t => t.X).ThenBy(t => t.Y).ThenBy(t => t.Z)
                .Select(t => new ClipboardTile(t.Id, t.X - ox, t.Y - oy, t.Z - oz, t.Invisible, t.Extra))
                .ToList();
        }

        /// <summary>Tiles from the Windows clipboard when it holds multi text lines, else the editor's own copy.</summary>
        private static List<ClipboardTile> ReadClipboard()
        {
            try
            {
                if (Clipboard.ContainsText() && ClipboardTile.Parse(Clipboard.GetText()) is { } fromText)
                {
                    return fromText;
                }
            }
            catch (ExternalException)
            {
                // Clipboard busy: fall back to the editor's own copy.
            }

            return _clipboard;
        }

        private static bool ClipboardHasTiles()
        {
            try
            {
                return Clipboard.ContainsText();
            }
            catch (ExternalException)
            {
                return false;
            }
        }
    }
}
