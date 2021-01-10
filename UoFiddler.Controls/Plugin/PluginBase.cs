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
using UoFiddler.Controls.Plugin.Interfaces;

namespace UoFiddler.Controls.Plugin
{
    public abstract class PluginBase
    {
        public abstract IPluginHost Host { get; set; }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }

        public abstract void Initialize();
        public abstract void Unload();

        /// <summary>
        /// On Startup called to modify the Plugin ToolStripDropDownButton
        /// </summary>
        /// <param name="toolStrip"></param>
        public virtual void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip) { }

        /// <summary>
        /// On Startup called to modify the tab pages
        /// </summary>
        /// <param name="tabControl"></param>
        public virtual void ModifyTabPages(TabControl tabControl) { }
    }
}

