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

using System;
using System.Windows.Forms;
using PluginInterface;

namespace FiddlerPlugin
{
    public class MultiEditorPlugin : IPlugin
    {
        #region Fields (6)

        MultiEditor.MultiEditor multieditor;
        string myAuthor = "MuadDib & Turley";
        string myDescription = "Plugin to Edit Multis\r\n(Adds 1 new Tab)";
        IPluginHost myHost = null;
        string myName = "MultiEditorPlugin";
        string myVersion = "1.7.0";

        #endregion Fields

        #region Constructors (1)

        public MultiEditorPlugin()
        {
            PluginInterface.Events.ModifyItemShowContextMenuEvent += new Events.ModifyItemShowContextMenuHandler(Events_ModifyItemShowContextMenuEvent);
        }

        #endregion Constructors

        #region Properties (5)

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get { return myAuthor; } }

        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get { return myDescription; } }

        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get { return myHost; } set { myHost = value; } }

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get { return myName; } }

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get { return myVersion; } }

        #endregion Properties

        #region Methods (6)

        // Public Methods (4) 

        public override void Dispose()
        {
            //fired in Fiddler OnClosing
        }

        public override void Initialize()
        {
            //fired on fiddler startup
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolstrip)
        {
            //want an entry inside the plugin dropdown?
        }

        // the magic add a new tabpage at the end
        public override void ModifyTabPages(TabControl tabcontrol)
        {
            TabPage page = new TabPage();
            page.Tag = tabcontrol.TabCount + 1; // at end used for undock/dock feature to define the order
            page.Text = "Multi Editor";
            multieditor = new MultiEditor.MultiEditor() { Dock = DockStyle.Fill };
            page.Controls.Add(multieditor);
            tabcontrol.TabPages.Add(page);
        }
        // Private Methods (2) 

        private void Events_ModifyItemShowContextMenuEvent(ContextMenuStrip strip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = "MultiEditor: Select Item";
            item.Click += new EventHandler(this.itemshowcontextclicked);
            strip.Items.Add(item);
        }

        private void itemshowcontextclicked(object sender, EventArgs e)
        {
            int currselected;
            if (FiddlerControls.Options.DesignAlternative)
                currselected = Host.GetSelectedItemShowAlternative();
            else
                currselected = Host.GetSelectedItemShow();
            if (currselected > -1)
            {
                if (multieditor != null)
                    multieditor.SelectDrawTile((ushort)currselected);
            }
        }

        #endregion Methods
    }
}
