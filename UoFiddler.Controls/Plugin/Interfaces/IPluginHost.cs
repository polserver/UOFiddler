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

using UoFiddler.Controls.UserControls;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.Plugin.Interfaces
{
    public interface IPluginHost
    {
        /// <summary>
        /// Returns the ItemsControl
        /// </summary>
        /// <returns></returns>
        ItemsControl GetItemsControl();

        /// <summary>
        /// Gets the TileView of ItemsControl
        /// </summary>
        /// <returns></returns>
        TileViewControl GetItemsControlTileView();

        /// <summary>
        /// Gets the current selected graphic id in ItemsControl
        /// </summary>
        /// <returns>Graphic id or -1 if none selected</returns>
        int GetSelectedIdFromItemsControl();
    }
}