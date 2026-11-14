/***************************************************************************
 *
 * $Author: MuadDib & Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/


namespace UoFiddler.Plugin.MultiEditor.Classes
{
    /// <summary>
    /// The view options the edited multi depends on.
    /// </summary>
    internal interface IMultiViewSettings
    {
        /// <summary>Tiles flagged as walls are not shown, and so not removed by area edits.</summary>
        bool HideWalls { get; }

        bool ShowWalkables { get; }

        bool ShowDoubleSurface { get; }
    }
}
