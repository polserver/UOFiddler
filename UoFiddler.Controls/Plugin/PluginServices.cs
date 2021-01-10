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
            Options.Logger.Information("FindPlugins - searching for plugins in [AppDomain.CurrentDomain.BaseDirectory = {path}]", path);

            AvailablePlugins.Clear();
            if (!Directory.Exists(path))
            {
                Options.Logger.Warning("FindPlugins - plugin directory doesn't exist: {path}", path);
                return;
            }

            foreach (string fileOn in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    AddPlugin(fileOn);
                }
                catch (Exception ex)
                {
                    Options.Logger.Fatal("FindPlugins - exception caught: {ex}", ex);
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

                Options.Logger.Information("FindPlugins - disposing plugin: {pluginOn}", pluginOn.Type.ToString());
                pluginOn.Instance.Unload();
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
                    Options.Logger.Information("FindPlugins - AddPlugin of type: {type} from file: {fileName}", pluginType.ToString(), newPlugin.AssemblyPath);
                    newPlugin.CreateInstance();
                    newPlugin.Instance.Host = this;
                    newPlugin.Instance.Initialize();
                }

                AvailablePlugins.Add(newPlugin);
            }
        }

        public ItemShowControl GetItemShowControl()
        {
            return ItemShowControl.RefMarker;
        }

        public int GetSelectedItemShow()
        {
            if (ItemShowControl.ItemListView.SelectedItems.Count > 0)
            {
                return (int)ItemShowControl.ItemListView.SelectedItems[0].Tag;
            }

            return -1;
        }

        public ListView GetItemShowListView()
        {
            return ItemShowControl.ItemListView;
        }

        public ItemShowAlternativeControl GetItemShowAltControl()
        {
            return ItemShowAlternativeControl.RefMarker;
        }

        public PictureBox GetItemShowAltPictureBox()
        {
            return ItemShowAlternativeControl.ItemPictureBox;
        }

        public int GetSelectedItemShowAlternative()
        {
            return ItemShowAlternativeControl.RefMarker.Selected;
        }
    }
}