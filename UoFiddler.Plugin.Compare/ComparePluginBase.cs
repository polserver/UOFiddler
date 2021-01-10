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
            + "(Adds 7 new Tabs)";

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get; } = "Turley";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get; } = "1.8.0";

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
                Tag = tabControl.TabCount + 1,
                Text = "Compare Items"
            };
            CompareItemControl compArt = new CompareItemControl
            {
                Dock = DockStyle.Fill
            };
            page.Controls.Add(compArt);
            tabControl.TabPages.Add(page);

            TabPage page2 = new TabPage
            {
                Tag = tabControl.TabCount + 1,
                Text = "Compare Land"
            };
            CompareLandControl compLandControl = new CompareLandControl
            {
                Dock = DockStyle.Fill
            };
            page2.Controls.Add(compLandControl);
            tabControl.TabPages.Add(page2);

            TabPage page3 = new TabPage
            {
                Tag = tabControl.TabCount + 1,
                Text = "Compare CliLocs"
            };
            CompareCliLocControl compCli = new CompareCliLocControl
            {
                Dock = DockStyle.Fill
            };
            page3.Controls.Add(compCli);
            tabControl.TabPages.Add(page3);

            TabPage page4 = new TabPage
            {
                Tag = tabControl.TabCount + 1,
                Text = "Compare Hues"
            };
            CompareHuesControl compH = new CompareHuesControl
            {
                Dock = DockStyle.Fill
            };
            page4.Controls.Add(compH);
            tabControl.TabPages.Add(page4);

            TabPage page5 = new TabPage
            {
                Tag = tabControl.TabCount + 1,
                Text = "Compare Gumps"
            };
            CompareGumpControl compG = new CompareGumpControl
            {
                Dock = DockStyle.Fill
            };
            page5.Controls.Add(compG);
            tabControl.TabPages.Add(page5);

            TabPage page6 = new TabPage
            {
                Tag = tabControl.TabCount + 1,
                Text = "Compare Map"
            };
            CompareMapControl compM = new CompareMapControl
            {
                Dock = DockStyle.Fill
            };
            page6.Controls.Add(compM);
            tabControl.TabPages.Add(page6);
            TabPage page7 = new TabPage
            {
                Tag = tabControl.TabCount + 1,
                Text = "Compare Texture"
            };
            CompareTextureControl compTextureControl = new CompareTextureControl
            {
                Dock = DockStyle.Fill
            };
            page7.Controls.Add(compTextureControl);
            tabControl.TabPages.Add(page7);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
        }
    }
}
