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

namespace PluginInterface
{
    public class Events
    {
        /// <summary>
        /// Fired when Design is changed
        /// </summary>
        public delegate void DesignChangeHandler();
        /// <summary>
        /// Fired when Design is changed
        /// </summary>
        public static event DesignChangeHandler DesignChangeEvent;
        public static void FireDesignChangeEvent()
        {
            if (DesignChangeEvent != null)
                DesignChangeEvent();
        }

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
            if (ModifyItemShowContextMenuEvent != null)
                ModifyItemShowContextMenuEvent(contextmenu);
        }
    }
    public abstract class IPlugin
    {
        abstract public IPluginHost Host { get; set; }

        abstract public string Name { get; }
        abstract public string Description { get; }
        abstract public string Author { get; }
        abstract public string Version { get; }

        abstract public void Initialize();
        abstract public void Dispose();

        /// <summary>
        /// On Startup called to modify the Plugin ToolStripDropDownButton
        /// </summary>
        /// <param name="toolstrip"></param>
        virtual public void ModifyPluginToolStrip(ToolStripDropDownButton toolstrip) { }
        /// <summary>
        /// On Startup called to modify the Tabpages
        /// </summary>
        /// <param name="tabcontrol"></param>
        virtual public void ModifyTabPages(TabControl tabcontrol) { }
    }

    public interface IPluginHost
    {
        /// <summary>
        /// Returns the ItemShowControl
        /// </summary>
        /// <returns></returns>
        FiddlerControls.ItemShow GetItemShowControl();
        /// <summary>
        /// Returns the ItemShowAlternativeControl
        /// </summary>
        /// <returns></returns>
        FiddlerControls.ItemShowAlternative GetItemShowAltControl();
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

