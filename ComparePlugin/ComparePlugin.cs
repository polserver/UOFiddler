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
using ComparePlugin;
using PluginInterface;

namespace FiddlerPlugin
{
    public class ComparePlugin : IPlugin
    {
        string myName = "ComparePlugin";
        string myDescription =
            "Compares 2 art files\r\n"
            + "Compares 2 CliLocs\r\n"
            + "Compares 2 Hue files\r\n"
            + "Compares 2 Map files\r\n"
            + "Compares 2 Gump files\r\n"
            + "Compares 2 Texture files\r\n"
            + "(Adds 7 new Tabs)";
        string myAuthor = "Turley";
        string myVersion = "1.8.0";
        IPluginHost myHost = null;

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get { return myName; } }
        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get { return myDescription; } }
        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get { return myAuthor; } }
        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get { return myVersion; } }
        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get { return myHost; } set { myHost = value; } }

        public override void Initialize()
        {
        }

        public override void Dispose()
        {
        }

        public override void ModifyTabPages(TabControl tabcontrol)
        {
            TabPage page = new TabPage();
            page.Tag = tabcontrol.TabCount + 1;
            page.Text = "Compare Items";
            CompareItem compArt = new CompareItem();
            compArt.Dock = System.Windows.Forms.DockStyle.Fill;
            page.Controls.Add(compArt);
            tabcontrol.TabPages.Add(page);

            TabPage page2 = new TabPage();
            page2.Tag = tabcontrol.TabCount + 1;
            page2.Text = "Compare Land";
            CompareLand compLand = new CompareLand();
            compLand.Dock = System.Windows.Forms.DockStyle.Fill;
            page2.Controls.Add(compLand);
            tabcontrol.TabPages.Add(page2);

            TabPage page3 = new TabPage();
            page3.Tag = tabcontrol.TabCount + 1;
            page3.Text = "Compare CliLocs";
            CompareCliLoc compCli = new CompareCliLoc();
            compCli.Dock = System.Windows.Forms.DockStyle.Fill;
            page3.Controls.Add(compCli);
            tabcontrol.TabPages.Add(page3);

            TabPage page4 = new TabPage();
            page4.Tag = tabcontrol.TabCount + 1;
            page4.Text = "Compare Hues";
            CompareHues compH = new CompareHues();
            compH.Dock = System.Windows.Forms.DockStyle.Fill;
            page4.Controls.Add(compH);
            tabcontrol.TabPages.Add(page4);

            TabPage page5 = new TabPage();
            page5.Tag = tabcontrol.TabCount + 1;
            page5.Text = "Compare Gumps";
            CompareGump compG = new CompareGump();
            compG.Dock = System.Windows.Forms.DockStyle.Fill;
            page5.Controls.Add(compG);
            tabcontrol.TabPages.Add(page5);

            TabPage page6 = new TabPage();
            page6.Tag = tabcontrol.TabCount + 1;
            page6.Text = "Compare Map";
            CompareMap compM = new CompareMap();
            compM.Dock = System.Windows.Forms.DockStyle.Fill;
            page6.Controls.Add(compM);
            tabcontrol.TabPages.Add(page6);
            TabPage page7 = new TabPage();
            page7.Tag = tabcontrol.TabCount + 1;
            page7.Text = "Compare Texture";
            CompareTexture compTexture = new CompareTexture();
            compTexture.Dock = System.Windows.Forms.DockStyle.Fill;
            page7.Controls.Add(compTexture);
            tabcontrol.TabPages.Add(page7);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolstrip)
        {
        }
    }
}
