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
using System.Reflection;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Controls.Plugin.Types;
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Plugin
{
    public class PluginServices : IPluginHost
    {
        /// <summary>
        /// A Collection of all Plugins Found
        /// </summary>
        public AvailablePlugins AvailablePlugins { get; set; } = new AvailablePlugins();

        /// <summary>
        /// Searches the Application's Startup Directory for Plugins
        /// </summary>
        public void FindPlugins()
        {
            FindPlugins(AppDomain.CurrentDomain.BaseDirectory);
        }

        /// <summary>
        /// Searches the passed Path for Plugins
        /// </summary>
        /// <param name="path">Directory to search for Plugins in</param>
        public void FindPlugins(string path)
        {
            AvailablePlugins.Clear();
            if (!Directory.Exists(path))
            {
                return;
            }

            foreach (string fileOn in Directory.GetFiles(path))
            {
                FileInfo file = new FileInfo(fileOn);
                if (!file.Extension.Equals(".dll"))
                {
                    continue;
                }

                try
                {
                    if (!file.Name.Equals("Controls.dll") && !file.Name.Equals("Ultima.dll"))
                    {
                        AddPlugin(fileOn);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        /// <summary>
        /// Unloads and Closes all AvailablePlugins
        /// </summary>
        public void ClosePlugins()
        {
            foreach (AvailablePlugin pluginOn in AvailablePlugins)
            {
                if (pluginOn.Instance == null)
                {
                    continue;
                }

                pluginOn.Instance.Dispose();
                pluginOn.Instance = null;
            }
            AvailablePlugins.Clear();
        }

        private void AddPlugin(string fileName)
        {
            Assembly pluginAssembly = Assembly.LoadFrom(fileName);
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (!pluginType.IsPublic || pluginType.IsAbstract)
                {
                    continue;
                }

                if (!pluginType.IsSubclassOf(typeof(PluginBase)))
                {
                    continue;
                }

                AvailablePlugin newPlugin = new AvailablePlugin
                {
                    AssemblyPath = fileName,
                    Type = pluginAssembly.GetType(pluginType.ToString())
                };

                if (Options.PluginsToLoad?.Contains(pluginType.ToString()) == true)
                {
                    newPlugin.CreateInstance();
                    newPlugin.Instance.Host = this;
                    newPlugin.Instance.Initialize();
                }

                AvailablePlugins.Add(newPlugin);
            }
        }

        public ItemShow GetItemShowControl()
        {
            return ItemShow.RefMarker;
        }

        public int GetSelectedItemShow()
        {
            if (ItemShow.ItemListView.SelectedItems.Count > 0)
            {
                return (int)ItemShow.ItemListView.SelectedItems[0].Tag;
            }

            return -1;
        }

        public ListView GetItemShowListView()
        {
            return ItemShow.ItemListView;
        }

        public ItemShowAlternative GetItemShowAltControl()
        {
            return ItemShowAlternative.RefMarker;
        }

        public PictureBox GetItemShowAltPictureBox()
        {
            return ItemShowAlternative.ItemPictureBox;
        }

        public int GetSelectedItemShowAlternative()
        {
            return ItemShowAlternative.RefMarker.Selected;
        }
    }
}