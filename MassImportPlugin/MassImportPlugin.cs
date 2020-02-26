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
using System.Windows.Forms;
using PluginInterface;

namespace FiddlerPlugin
{
    public class MassImportPlugin : Plugin
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get; } = "MassImportPlugin";

        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get; } = "Import xml based";

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get; } = "Turley";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get; } = "1.1.0";

        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get; set; } = null;

        public override void Initialize() { }

        public override void Dispose() { }

        public override void ModifyTabPages(TabControl tabControl) { }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "Mass Import"
            };
            item.Click += ToolStripClick;
            toolStrip.DropDownItems.Add(item);
        }

        private MassImport.MassImport _import;

        private void ToolStripClick(object sender, EventArgs e)
        {
            if (_import?.IsDisposed == false)
            {
                return;
            }

            _import = new MassImport.MassImport
            {
                TopMost = true
            };
            _import.Show();
        }
    }
}
