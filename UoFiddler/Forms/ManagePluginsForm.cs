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
using UoFiddler.Localization;
using Ultima.Helpers;

namespace UoFiddler.Forms
{
    public partial class ManagePluginsForm : Form
    {
        private static readonly ILogger _log = AppLog.For(typeof(ManagePluginsForm));
        
        // 保存显示名称到原始名称的映射
        private readonly Dictionary<string, string> _displayNameToOriginalName = new Dictionary<string, string>();

        public ManagePluginsForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            
            // 汉化窗口标题
            LocalizationService.LocalizeForm(this);

            foreach (AvailablePlugin plugin in GlobalPlugins.Plugins.AvailablePlugins)
            {
                bool loaded = true;
                if (plugin.Instance == null)
                {
                    _log.LogInformation("ManagePlugins - creating plugin instance: {Plugin} path: {AssemblyPath}", plugin.Type, plugin.AssemblyPath);
                    plugin.CreateInstance();
                    loaded = false;
                }
                
                // 获取原始名称
                string originalName = plugin.Instance.Name;
                
                // 尝试获取汉化名称
                string displayName = originalName;
                string? localizedName = LocalizationService.GetString($"Forms.ManagePluginsForm.Plugins.{originalName}");
                if (!string.IsNullOrEmpty(localizedName))
                {
                    displayName = localizedName;
                }
                
                // 保存映射关系
                _displayNameToOriginalName[displayName] = originalName;
                
                // 添加到列表（使用汉化后的显示名称）
                checkedListBox1.Items.Add(displayName, loaded);
            }
        }

        private void OnSelect(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            if (checkedListBox1.SelectedItem == null)
            {
                return;
            }

            // 获取显示的名称
            string displayName = checkedListBox1.SelectedItem.ToString();
            
            // 转换为原始名称
            string originalName = displayName;
            if (_displayNameToOriginalName.ContainsKey(displayName))
            {
                originalName = _displayNameToOriginalName[displayName];
            }

            AvailablePlugin selPlugin = GlobalPlugins.Plugins.AvailablePlugins.Find(originalName);
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
                // 找到该插件在列表中的显示名称
                string originalName = plug.Instance.Name;
                string displayName = originalName;
                
                // 查找对应的显示名称
                foreach (var kvp in _displayNameToOriginalName)
                {
                    if (kvp.Value == originalName)
                    {
                        displayName = kvp.Key;
                        break;
                    }
                }

                if (Options.PluginsToLoad?.Contains(plug.Type.ToString()) == false)
                {
                    if (checkedListBox1.CheckedItems.Contains(displayName))
                    {
                        _log.LogInformation("ManagePlugins - adding plugin to profile: {Plugin}", plug.Type.ToString());
                        Options.PluginsToLoad.Add(plug.Type.ToString());
                    }

                    plug.Instance = null;
                }
                else
                {
                    if (!checkedListBox1.CheckedItems.Contains(displayName))
                    {
                        _log.LogInformation("ManagePlugins - removing plugin from profile: {Plugin}", plug.Type.ToString());
                        Options.PluginsToLoad.Remove(plug.Type.ToString());
                    }
                }
            }
        }
    }
}
