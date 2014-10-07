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
using System.IO;
using System.Windows.Forms;
using PluginInterface;
using Ultima;


namespace FiddlerPlugin
{
    public class TestPlugin : IPlugin
    {
        public TestPlugin()
        {
            PluginInterface.Events.DesignChangeEvent+=new Events.DesignChangeHandler(Events_DesignChangeEvent);
            PluginInterface.Events.ModifyItemShowContextMenuEvent+=new Events.ModifyItemShowContextMenuHandler(Events_ModifyItemShowContextMenuEvent);
        }

        string myName = "PluginTest";
        string myDescription = "An Plugin Example";
        string myAuthor = "Turley";
        string myVersion = "1.0.0";
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


        FiddlerControls.ItemShow itemshow;
        FiddlerControls.ItemShowAlternative itemshowalt;
        public override void Initialize()
        {
            if (FiddlerControls.Options.DesignAlternative)
                itemshowalt = Host.GetItemShowAltControl();
            else
                itemshow = Host.GetItemShowControl();
            //make something usefull
        }

        public override void Dispose()
        {
        }

        public override void ModifyTabPages(TabControl tabcontrol)
        {
            TabPage page = new TabPage();
            page.Tag = tabcontrol.TabCount+1; // at end used for undock/dock feature to define the order
            page.Text = "PluginTest";
            page.Controls.Add(new UserControl1());
            tabcontrol.TabPages.Add(page);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolstrip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text="PluginTest";
            item.Click+=new EventHandler(item_click);
            toolstrip.DropDownItems.Add(item);
        }

        public void item_click(object sender, EventArgs e)
        {
            new Form1().Show();
        }

        private void Events_DesignChangeEvent()
        {
            //do something usefull
        }

        public void Events_ModifyItemShowContextMenuEvent(ContextMenuStrip strip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = "Create Itemdesc entry";
            item.Click += new EventHandler(this.itemshowcontextclicked);
            strip.Items.Add(item);
        }

        public void itemshowcontextclicked(object sender, EventArgs e)
        {
            int currselected;
            if (FiddlerControls.Options.DesignAlternative)
                currselected = Host.GetSelectedItemShowAlternative();
            else
                currselected = Host.GetSelectedItemShow();
            if (currselected > -1)
            {
                if (Ultima.Art.IsValidStatic(currselected))
                {
                    ItemData data = Ultima.TileData.ItemTable[currselected];
                    string path = FiddlerControls.Options.OutputPath;
                    string FileName = Path.Combine(path,"itemdesc.cfg");
                    using (StreamWriter Tex = new StreamWriter(new FileStream(FileName, FileMode.Append, FileAccess.Write), System.Text.Encoding.GetEncoding(1252)))
                    {
                        Tex.WriteLine(String.Format("Item 0x{0:X4}", currselected));
                        Tex.WriteLine("{");
                        Tex.WriteLine(String.Format("   Name    {0}", data.Name));
                        Tex.WriteLine(String.Format("   Graphic 0x{0:X4}", currselected));
                        Tex.WriteLine(String.Format("   Weight  {0}", data.Weight));
                        Tex.WriteLine("}");
                    }
                }
            }
        }
    }
}
