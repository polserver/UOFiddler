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
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using FiddlerControls;
using PluginInterface;
using Ultima;

namespace FiddlerPlugin
{
    public class ExamplePlugin : IPlugin
    {
        private const string ItemDescFileName = "itemdesc.cfg";

        public ExamplePlugin()
        {
            PluginInterface.Events.DesignChangeEvent += new PluginInterface.Events.DesignChangeHandler(Events_DesignChangeEvent);
            PluginInterface.Events.ModifyItemShowContextMenuEvent += new PluginInterface.Events.ModifyItemShowContextMenuHandler(Events_ModifyItemShowContextMenuEvent);
        }

        /// <summary>
        /// Name of the plugin
        /// </summary>
        public override string Name { get; } = "PluginTest";

        /// <summary>
        /// Description of the Plugin's purpose
        /// </summary>
        public override string Description { get; } = "This is example plugin.";

        /// <summary>
        /// Author of the plugin
        /// </summary>
        public override string Author { get; } = "Turley";

        /// <summary>
        /// Version of the plugin
        /// </summary>
        public override string Version { get; } = "1.0.0";

        /// <summary>
        /// Host of the plugin.
        /// </summary>
        public override IPluginHost Host { get; set; }

        public override void Initialize()
        {
            //make something usefull
        }

        public override void Dispose()
        {
        }

        public override void ModifyTabPages(TabControl tabcontrol)
        {
            TabPage page = new TabPage
            {
                Tag = tabcontrol.TabCount + 1, // at end used for undock/dock feature to define the order
                Text = "PluginTest"
            };
            page.Controls.Add(new ExampleUserControl());
            tabcontrol.TabPages.Add(page);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolstrip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem
            {
                Text = "PluginTest"
            };
            item.Click += Item_click;
            toolstrip.DropDownItems.Add(item);
        }

        public void Item_click(object sender, EventArgs e)
        {
            new Example().Show();
        }

        private void Events_DesignChangeEvent()
        {
            //do something usefull here
        }

        public void Events_ModifyItemShowContextMenuEvent(ContextMenuStrip strip)
        {
            strip.Items.Add( new ToolStripSeparator());

            ToolStripMenuItem exportItemDescItem = new ToolStripMenuItem
            {
                Text = "Export selected to itemdesc.cfg"
            };
            exportItemDescItem.Click += ExportToItemDescClicked;
            strip.Items.Add(exportItemDescItem);

            strip.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exportOffsetItem = new ToolStripMenuItem
            {
                Text = "Export all items to offset.cfg"
            };
            exportOffsetItem.Click += ExportToOffsetClicked;
            strip.Items.Add(exportOffsetItem);
        }

        public void ExportToOffsetClicked(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, "offset.cfg");

            string inputMessage = "Do you want to export all items to offset.cfg?\r\n"
                + "It may take some time (around 10-20 seconds).\r\n\r\n"
                + "Export will replace existing file located at: "
                + fileName
                + "\r\n\r\nContinue?\r\n";

            if (MessageBox.Show(inputMessage, "Export all items to offset.cfg?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }

            var listView = Host.GetItemShowListView();
            var sb = new StringBuilder();
            foreach (ListViewItem item in listView.Items)
            {
                var itemId = (int)item.Tag;

                if (itemId > -1 && Art.IsValidStatic(itemId))
                {
                    Art.Measure(Art.GetStatic(itemId), out int xMin, out int yMin, out int xMax, out int yMax);

                    sb.AppendFormat("Item 0x{0:X4}", itemId).AppendLine();
                    sb.AppendLine("{");
                    sb.AppendFormat("   xMin    {0}", xMin).AppendLine();
                    sb.AppendFormat("   yMin    {0}", yMin).AppendLine();
                    sb.AppendFormat("   xMax    {0}", xMax).AppendLine();
                    sb.AppendFormat("   yMax    {0}", yMax).AppendLine();
                    sb.AppendLine("}").AppendLine();
                }
            }

            File.WriteAllText(fileName, sb.ToString());

            MessageBox.Show("Done!");
        }

        public void ExportToItemDescClicked(object sender, EventArgs e)
        {
            if (Options.DesignAlternative)
            {
                ExportSingleItem(Host.GetSelectedItemShowAlternative());
            }
            else
            {
                ExportAllSelectedItems(Host.GetItemShowListView());
            }
        }

        private void ExportAllSelectedItems(ListView itemShowListView)
        {
            var selectedItems = itemShowListView.SelectedItems;
            if (selectedItems.Count > 0)
            {
                string path = Options.OutputPath;
                string fileName = Path.Combine(path, ItemDescFileName);

                using (StreamWriter Tex = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write), Encoding.GetEncoding(1252)))
                {
                    var sb = new StringBuilder();

                    foreach (ListViewItem item in selectedItems)
                    {
                        var itemId = (int)item.Tag;

                        if (Art.IsValidStatic(itemId))
                        {
                            Tex.WriteLine(GetItemDescEntry(itemId));
                        }
                    }
                }
            }
        }

        private void ExportSingleItem(int itemId)
        {
            if (itemId > -1 && Art.IsValidStatic(itemId))
            {
                string path = Options.OutputPath;
                string fileName = Path.Combine(path, ItemDescFileName);

                using (StreamWriter Tex = new StreamWriter(new FileStream(fileName, FileMode.Append, FileAccess.Write), Encoding.GetEncoding(1252)))
                {
                    Tex.WriteLine(GetItemDescEntry(itemId));
                }
            }
        }

        private string GetItemDescEntry(int itemId)
        {
            ItemData itemData = TileData.ItemTable[itemId];

            var sb = new StringBuilder();
            sb.AppendFormat("Item 0x{0:X4}", itemId).AppendLine();
            sb.AppendLine("{");
            sb.AppendFormat("   Name    {0}", itemData.Name).AppendLine();
            sb.AppendFormat("   Graphic 0x{0:X4}", itemId).AppendLine();
            sb.AppendFormat("   Weight  {0}", itemData.Weight).AppendLine();
            sb.AppendLine("}").AppendLine();

            return sb.ToString();
        }
    }
}
