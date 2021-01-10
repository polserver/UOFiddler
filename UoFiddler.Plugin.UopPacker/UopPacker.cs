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
using Ultima;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Plugin.UopPacker.UserControls;

namespace UoFiddler.Plugin.UopPacker
{
    public class UopPacker : PluginBase
    {
        private readonly Version _ver;

        public UopPacker()
        {
            _ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get; } = "UOP Packer";

        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get; } = "UOP packer/unpacker\r\nUses RunUO UOP packer";

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get; } = "Feeh / Epila";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get { return _ver.ToString(); } }

        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get; set; }

        public override void Initialize()
        {
            _ = Files.RootDir;
        }

        public override void Unload()
        {
        }

        public override void ModifyTabPages(TabControl tabControl)
        {
            TabPage page = new TabPage
            {
                Tag = 0, // at end used for undock/dock feature to define the order
                Text = "UOP Packer"
            };
            page.Controls.Add(new UopPackerControl(Version));
            tabControl.TabPages.Add(page);
        }
    }
}
