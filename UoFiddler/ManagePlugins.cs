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
using Host;

namespace UoFiddler
{
    public partial class ManagePlugins : Form
    {
        public ManagePlugins()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();

            foreach (Host.Types.AvailablePlugin plug in GlobalPlugins.Plugins.AvailablePlugins)
            {
                bool loaded = true;
                if (plug.Instance == null)
                {
                    plug.CreateInstance();
                    loaded = false;
                }
                checkedListBox1.Items.Add(plug.Instance.Name, loaded);
            }
        }

        private void OnSelect(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            if (checkedListBox1.SelectedItem != null)
            {
                Host.Types.AvailablePlugin selPlugin = GlobalPlugins.Plugins.AvailablePlugins.Find(checkedListBox1.SelectedItem.ToString());
                if (selPlugin != null)
                {
                    System.Drawing.Font font = new System.Drawing.Font(richTextBox1.Font.FontFamily, richTextBox1.Font.Size, System.Drawing.FontStyle.Bold);
                    richTextBox1.AppendText("Name: " + selPlugin.Instance.Name + "\n");
                    richTextBox1.Select(0, 5);
                    richTextBox1.SelectionFont = font;
                    richTextBox1.AppendText("Version: " + selPlugin.Instance.Version + "\n");
                    richTextBox1.Select(richTextBox1.Text.IndexOf("Version: "), 9);
                    richTextBox1.SelectionFont = font;
                    richTextBox1.AppendText("Author: " + selPlugin.Instance.Author + "\n");
                    richTextBox1.Select(richTextBox1.Text.IndexOf("Author: "), 8);
                    richTextBox1.SelectionFont = font;
                    richTextBox1.AppendText("Description:\n" + selPlugin.Instance.Description + "\n");
                    richTextBox1.Select(richTextBox1.Text.IndexOf("Description:"), 12);
                    richTextBox1.SelectionFont = font;
                }
            }
        }

        private void onClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Host.Types.AvailablePlugin plug in GlobalPlugins.Plugins.AvailablePlugins)
            {
                if (!FiddlerControls.Options.PluginsToLoad.Contains(plug.Type.ToString()))
                {
                    if (checkedListBox1.CheckedItems.Contains(plug.Instance.Name))
                        FiddlerControls.Options.PluginsToLoad.Add(plug.Type.ToString());
                    plug.Instance = null;
                }
                else
                {
                    if (!checkedListBox1.CheckedItems.Contains(plug.Instance.Name))
                        FiddlerControls.Options.PluginsToLoad.Remove(plug.Type.ToString());
                }
            }
        }
    }
}
