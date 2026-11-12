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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin;
using Ultima.Helpers;

namespace UoFiddler.Forms
{
    public partial class ManagePluginsForm : Form
    {
        private static readonly ILogger _log = AppLog.For(typeof(ManagePluginsForm));

        // Instances made only to show a plugin's name and details. They are never initialized,
        // so they are dropped again on close and never unloaded.
        private readonly HashSet<AvailablePlugin> _temporaryInstances = new HashSet<AvailablePlugin>();

        public ManagePluginsForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            foreach (AvailablePlugin plugin in GlobalPlugins.Plugins.AvailablePlugins)
            {
                if (plugin.Instance == null)
                {
                    _log.LogInformation("ManagePlugins - creating plugin instance: {Plugin} path: {AssemblyPath}", plugin.Type, plugin.AssemblyPath);
                    plugin.CreateInstance();
                    _temporaryInstances.Add(plugin);
                }

                // The profile decides what is enabled; a change here takes effect on the next start.
                bool enabled = Options.PluginsToLoad?.Contains(plugin.Type.ToString()) == true;
                checkedListBox1.Items.Add(plugin.Instance.Name, enabled);
            }
        }

        private void OnSelect(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            if (checkedListBox1.SelectedItem == null)
            {
                return;
            }

            AvailablePlugin selPlugin = GlobalPlugins.Plugins.AvailablePlugins.Find(checkedListBox1.SelectedItem.ToString());
            if (selPlugin == null)
            {
                return;
            }

            Font font = new Font(richTextBox1.Font.FontFamily, richTextBox1.Font.Size, FontStyle.Bold);
            richTextBox1.AppendText($"Name: {selPlugin.Instance.Name}\n");
            richTextBox1.Select(0, 5);
            richTextBox1.SelectionFont = font;
            richTextBox1.AppendText($"Version: {selPlugin.Instance.Version}\n");
            richTextBox1.Select(richTextBox1.Text.IndexOf("Version: ", StringComparison.Ordinal), 9);
            richTextBox1.SelectionFont = font;
            richTextBox1.AppendText($"Author: {selPlugin.Instance.Author}\n");
            richTextBox1.Select(richTextBox1.Text.IndexOf("Author: ", StringComparison.Ordinal), 8);
            richTextBox1.SelectionFont = font;
            richTextBox1.AppendText($"Description:\n{selPlugin.Instance.Description}\n");
            richTextBox1.Select(richTextBox1.Text.IndexOf("Description:", StringComparison.Ordinal), 12);
            richTextBox1.SelectionFont = font;
        }

        private void OnClosing(object sender, FormClosingEventArgs e)
        {
            foreach (AvailablePlugin plug in GlobalPlugins.Plugins.AvailablePlugins)
            {
                string type = plug.Type.ToString();
                bool isChecked = checkedListBox1.CheckedItems.Contains(plug.Instance.Name);
                bool enabled = Options.PluginsToLoad?.Contains(type) == true;

                if (Options.PluginsToLoad != null && isChecked && !enabled)
                {
                    _log.LogInformation("ManagePlugins - adding plugin to profile: {Plugin}", type);
                    Options.PluginsToLoad.Add(type);
                }
                else if (Options.PluginsToLoad != null && !isChecked && enabled)
                {
                    _log.LogInformation("ManagePlugins - removing plugin from profile: {Plugin}", type);
                    Options.PluginsToLoad.Remove(type);
                }

                if (_temporaryInstances.Contains(plug))
                {
                    plug.Instance = null;
                }
            }
        }
    }
}
