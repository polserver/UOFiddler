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

using System;
using System.Drawing;
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.Plugin
{
    public static class PluginEvents
    {
        /// <summary>
        /// OnLoad called in ItemsControl to modify Contextmenu
        /// </summary>
        /// <param name="contextmenu"></param>
        public delegate void ModifyItemsControlContextMenuHandler(ContextMenuStrip contextmenu);

        /// <summary>
        /// OnLoad called in ItemsControl to modify Contextmenu
        /// </summary>
        public static event ModifyItemsControlContextMenuHandler ModifyItemsControlContextMenuEvent;

        public static void FireModifyItemShowContextMenuEvent(ContextMenuStrip contextmenu)
        {
            ModifyItemsControlContextMenuEvent?.Invoke(contextmenu);
        }

        /// <summary>
        /// OnLoad called in MapControl to modify its context menu. <paramref name="clickedAt"/> gives
        /// the shown map and the map location of the right click that opened the menu.
        /// </summary>
        public delegate void ModifyMapControlContextMenuHandler(ContextMenuStrip contextmenu, Func<(Map Map, Point Location)> clickedAt);

        /// <summary>
        /// OnLoad called in MapControl to modify its context menu
        /// </summary>
        public static event ModifyMapControlContextMenuHandler ModifyMapControlContextMenuEvent;

        public static void FireModifyMapControlContextMenuEvent(ContextMenuStrip contextmenu, Func<(Map Map, Point Location)> clickedAt)
        {
            ModifyMapControlContextMenuEvent?.Invoke(contextmenu, clickedAt);
        }
    }
}