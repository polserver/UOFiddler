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

namespace Ultima
{
    /// <summary>
    /// Tracks which indexes of a file type have been edited since the data was loaded or last saved.
    /// Kept deliberately apart from the replaced-bitmap dictionaries: those hold the live edits and
    /// must survive a save, whereas a mark here only means "changed and not written out yet".
    /// </summary>
    public sealed class ModifiedIndexTracker
    {
        private readonly HashSet<int> _marked = new HashSet<int>();

        /// <summary>
        /// Number of indexes currently marked as modified.
        /// </summary>
        public int Count => _marked.Count;

        /// <summary>
        /// Marks <paramref name="index"/> as modified.
        /// </summary>
        public void Mark(int index)
        {
            _marked.Add(index);
        }

        /// <summary>
        /// Tests whether <paramref name="index"/> was modified. Called once per tile per paint, so the
        /// empty case skips hashing altogether.
        /// </summary>
        public bool IsMarked(int index)
        {
            return _marked.Count != 0 && _marked.Contains(index);
        }

        /// <summary>
        /// Drops every mark. Called on reload and after a successful save.
        /// </summary>
        public void Clear()
        {
            _marked.Clear();
        }
    }
}
