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
using Ultima;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;

namespace UoFiddler.Plugin.MultiEditor
{
    public class MultiEditorPluginBase : PluginBase
    {
        private UserControls.MultiEditorControl _multiEditorControl;

        public MultiEditorPluginBase()
        {
            PluginEvents.ModifyItemsControlContextMenuEvent += EventsModifyItemsControlContextMenuEvent;
        }

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get; } = "MuadDib & Turley";

        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get; } = "Plugin to Edit Multis\r\n(Adds 1 new Tab)";

        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get; set; } = null;

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get; } = "MultiEditorPlugin";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get; } = "1.7.0";

        public override void Unload()
        {
            // fired in Fiddler OnClosing
        }

        public override void Initialize()
        {
            // fired on fiddler startup
            _ = Files.RootDir;
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
            // want an entry inside the plugin dropdown?
        }

        // the magic add a new tab page at the end
        public override void ModifyTabPages(TabControl tabControl)
        {
            TabPage page = new TabPage
            {
                Tag = tabControl.TabCount + 1, // at end used for undock/dock feature to define the order
                Text = "Multi Editor"
            };

            _multiEditorControl = new UserControls.MultiEditorControl
            {
                Dock = DockStyle.Fill
            };
            page.Controls.Add(_multiEditorControl);
            tabControl.TabPages.Add(page);
        }

        private void EventsModifyItemsControlContextMenuEvent(ContextMenuStrip strip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "MultiEditor: Select Item"
            };
            item.Click += ItemShowContextClicked;
            strip.Items.Add(item);
        }

        private void ItemShowContextClicked(object sender, EventArgs e)
        {
            int currSelected = Host.GetSelectedIdFromItemsControl();
            if (currSelected <= -1)
            {
                return;
            }

            _multiEditorControl?.SelectDrawTile((ushort)currSelected);
        }
    }
}
