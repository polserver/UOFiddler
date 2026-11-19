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

namespace UoFiddler.Plugin.MultiEditor.Tools
{
    /// <summary>
    /// How strongly a tool marks something it draws.
    /// </summary>
    internal enum OverlayTone
    {
        /// <summary>There, but not what is being worked on.</summary>
        Plain,

        /// <summary>Under the mouse: what a press would take hold of.</summary>
        Hover,

        /// <summary>The thing being worked on.</summary>
        Selected,

        /// <summary>Something a release would take away.</summary>
        Remove
    }
}
