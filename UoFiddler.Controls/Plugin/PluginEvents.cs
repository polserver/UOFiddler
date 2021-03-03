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

namespace UoFiddler.Controls.Plugin
{
    public static class PluginEvents
    {
        /// <summary>
        /// OnLoad called in ItemShow or ItemShowAlternative to modify Contextmenu
        /// </summary>
        /// <param name="contextmenu"></param>
        public delegate void ModifyItemShowContextMenuHandler(ContextMenuStrip contextmenu);

        /// <summary>
        /// OnLoad called in ItemShow or ItemShowAlternative to modify Contextmenu
        /// </summary>
        public static event ModifyItemShowContextMenuHandler ModifyItemShowContextMenuEvent;

        public static void FireModifyItemShowContextMenuEvent(ContextMenuStrip contextmenu)
        {
            ModifyItemShowContextMenuEvent?.Invoke(contextmenu);
        }
    }
}