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
using Ultima;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Controls.UserControls;
using UoFiddler.Plugin.Compare.UserControls;

namespace UoFiddler.Plugin.Compare
{
    public class ComparePluginBase : PluginBase
    {
        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get; } = "ComparePlugin";

        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get; } =
            "Compares 2 art files\r\n"
            + "Compares 2 CliLocs\r\n"
            + "Compares 2 Hue files\r\n"
            + "Compares 2 Map files\r\n"
            + "Compares 2 Gump files\r\n"
            + "Compares 2 Texture files\r\n"
            + "Compares 2 AnimData files\r\n"
            + "Compares 2 RadarCol files\r\n"
            + "(Adds a Compare tab with 10 sub-tabs)";

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get; } = "Turley";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get; } = "1.9.0";

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
            ThemedTabControl compareTabs = new ThemedTabControl
            {
                Dock = DockStyle.Fill
            };
            AddPage(compareTabs, "Items", new CompareItemControl());
            AddPage(compareTabs, "Land", new CompareLandControl());
            AddPage(compareTabs, "CliLocs", new CompareCliLocControl());
            AddPage(compareTabs, "Hues", new CompareHuesControl());
            AddPage(compareTabs, "Gumps", new CompareGumpControl());
            AddPage(compareTabs, "Map", new CompareMapControl());
            AddPage(compareTabs, "Texture", new CompareTextureControl());
            AddPage(compareTabs, "AnimData", new CompareAnimDataControl());
            AddPage(compareTabs, "RadarCol", new CompareRadarColControl());
            AddPage(compareTabs, "TileData", new CompareTileDataControl());

            TabPage page = new TabPage
            {
                Tag = tabControl.TabCount + 1,
                Text = "Compare"
            };
            page.Controls.Add(compareTabs);
            tabControl.TabPages.Add(page);
        }

        private static void AddPage(TabControl tabs, string text, Control content)
        {
            TabPage page = new TabPage
            {
                Text = text,
                UseVisualStyleBackColor = true
            };
            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
        }
    }
}
