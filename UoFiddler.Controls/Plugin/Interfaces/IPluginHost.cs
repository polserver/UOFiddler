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

using System.Windows.Forms;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Plugin.Interfaces
{
    public interface IPluginHost
    {
        /// <summary>
        /// Returns the ItemShowControl
        /// </summary>
        /// <returns></returns>
        ItemShowControl GetItemShowControl();

        /// <summary>
        /// Returns the ItemShowAlternativeControl
        /// </summary>
        /// <returns></returns>
        ItemShowAlternativeControl GetItemShowAltControl();

        /// <summary>
        /// Gets the current selected graphic in ItemShow
        /// </summary>
        /// <returns>Graphic or -1 if none selected</returns>
        int GetSelectedItemShow();

        /// <summary>
        /// Gets the current selected graphic in ItemShowAlternative
        /// </summary>
        /// <returns>Graphic or -1 if none selected</returns>
        int GetSelectedItemShowAlternative();

        /// <summary>
        /// Gets the ListView of ItemShow
        /// </summary>
        /// <returns></returns>
        ListView GetItemShowListView();

        /// <summary>
        /// Gets the PictureBox of ItemShowAlt
        /// </summary>
        /// <returns></returns>
        PictureBox GetItemShowAltPictureBox();
    }
}