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
using System.Xml;
using PluginInterface;
using Ultima;

namespace FiddlerPlugin
{
    public class SendItemPlugin : IPlugin
    {
        public SendItemPlugin()
        {
            refMarker = this;
            PluginInterface.Events.DesignChangeEvent += new Events.DesignChangeHandler(Events_DesignChangeEvent);
            PluginInterface.Events.ModifyItemShowContextMenuEvent += new Events.ModifyItemShowContextMenuHandler(Events_ModifyItemShowContextMenuEvent);
        }

        private static SendItemPlugin refMarker = null;
        static string m_Cmd = ".create";
        static string m_CmdArg = "0x{1:X4}";
        static bool m_OverrideClick = false;

        public static string Cmd { get { return SendItemPlugin.m_Cmd; } set { SendItemPlugin.m_Cmd = value; } }
        public static string CmdArg { get { return SendItemPlugin.m_CmdArg; } set { SendItemPlugin.m_CmdArg = value; } }
        public static bool OverrideClick
        {
            get { return SendItemPlugin.m_OverrideClick; }
            set
            {
                if (value != SendItemPlugin.m_OverrideClick)
                    refMarker.ChangeOverrideClick(value,false);
                SendItemPlugin.m_OverrideClick = value;
            }
        }

        string myName = "SendItemPlugin";
        string myDescription = "Send custom Cmd to Client with selected ObjectType in Itemstab";
        string myAuthor = "Turley";
        string myVersion = "1.0.1";
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
            LoadXML();
            ChangeOverrideClick(OverrideClick,true);
        }

        private void PlugOnDoubleClick(object sender, MouseEventArgs e)
        {
            itemshowcontextclicked(this, EventArgs.Empty);
        }
        public override void Dispose()
        {
            SaveXML();
        }

        public override void ModifyTabPages(TabControl tabcontrol) { }

        private void Events_DesignChangeEvent()
        {
            ChangeOverrideClick(OverrideClick, true);
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolstrip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = "Send Item";
            item.Click += new EventHandler(toolstrip_click);
            toolstrip.DropDownItems.Add(item);
        }

        public void ChangeOverrideClick(bool value,bool init)
        {
            if (FiddlerControls.Options.DesignAlternative)
            {
                FiddlerControls.ItemShowAlternative itemshowalt = Host.GetItemShowAltControl();
                PictureBox picturebox = Host.GetItemShowAltPictureBox();
                if (value)
                {
                    picturebox.MouseDoubleClick -= new MouseEventHandler(itemshowalt.OnMouseDoubleClick);
                    picturebox.MouseDoubleClick += new MouseEventHandler(this.PlugOnDoubleClick);
                }
                else if (!init)
                {
                    picturebox.MouseDoubleClick -= new MouseEventHandler(this.PlugOnDoubleClick);
                    picturebox.MouseDoubleClick += new MouseEventHandler(itemshowalt.OnMouseDoubleClick);
                }
            }
            else
            {
                FiddlerControls.ItemShow itemshow = Host.GetItemShowControl();
                ListView listview = Host.GetItemShowListView();
                if (value)
                {
                    listview.MouseDoubleClick -= new MouseEventHandler(itemshow.listView_DoubleClicked);
                    listview.MouseDoubleClick += new MouseEventHandler(this.PlugOnDoubleClick);
                }
                else if (!init)
                {
                    listview.MouseDoubleClick -= new MouseEventHandler(this.PlugOnDoubleClick);
                    listview.MouseDoubleClick += new MouseEventHandler(itemshow.listView_DoubleClicked);
                }
            }
        }

        private void toolstrip_click(object sender, EventArgs e)
        {
            new Option().Show();
        }

        private void Events_ModifyItemShowContextMenuEvent(ContextMenuStrip strip)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = "Send Item to Client";
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
                if (Client.Running)
                {
                    string format = "{0} " + CmdArg;
                    Client.SendText(String.Format(format, Cmd, currselected));
                }
                else
                {
                    MessageBox.Show(
                        "No Client running/or not recognized",
                        "SendItem",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
        }

        private void LoadXML()
        {
            string path = FiddlerControls.Options.AppDataPath;
            string FileName = Path.Combine(path, @"plugins/SendItem.xml");
            if (!System.IO.File.Exists(FileName))
                return;

            XmlDocument dom = new XmlDocument();
            dom.Load(FileName);
            XmlElement xOptions = dom["Options"];

            XmlElement elem = (XmlElement)xOptions.SelectSingleNode("SendItem");
            if (elem != null)
            {
                Cmd = elem.GetAttribute("cmd");
                CmdArg = elem.GetAttribute("args");
                OverrideClick = bool.Parse(elem.GetAttribute("overrideclick"));
            }
        }

        private void SaveXML()
        {
            string path = FiddlerControls.Options.AppDataPath;
            string FileName = Path.Combine(path, @"plugins/senditem.xml");

            XmlDocument dom = new XmlDocument();
            XmlDeclaration decl = dom.CreateXmlDeclaration("1.0", "utf-8", null);
            dom.AppendChild(decl);
            XmlElement sr = dom.CreateElement("Options");

            XmlComment comment = dom.CreateComment("Definies the cmd for Item create");
            sr.AppendChild(comment);
            comment = dom.CreateComment("{1} = item objecttype");
            sr.AppendChild(comment);
            XmlElement elem = dom.CreateElement("SendItem");
            elem.SetAttribute("cmd", Cmd);
            elem.SetAttribute("args", CmdArg);
            elem.SetAttribute("overrideclick", OverrideClick.ToString());
            sr.AppendChild(elem);

            dom.AppendChild(sr);
            dom.Save(FileName);
        }
    }
}
