/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.MultiEditor.Editing
{
    /// <summary>
    /// The tools of the editor, by the name the UI shows.
    /// </summary>
    internal enum ToolKind
    {
        Select,
        Lasso,
        Draw,
        Remove,
        Altitude,
        Pipette,
        Transparent,
        RectFill,
        Line,
        Wall,
        Floor,
        Door,
        Stair,
        Roof,
        Foundation,

        /// <summary>Lays a bordered floor pattern; chosen from the House page, not the palette.</summary>
        FloorPattern,

        /// <summary>Designs a whole house as a draft and places it.</summary>
        House
    }
}
