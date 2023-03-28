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
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Controls.UserControls;
using UoFiddler.Controls.UserControls.TileView;

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
            Options.Logger.Information("FindPlugins - searching for plugins in [AppDomain.CurrentDomain.BaseDirectory = {Path}]", path);

            AvailablePlugins.Clear();
            if (!Directory.Exists(path))
            {
                Options.Logger.Warning("FindPlugins - plugin directory doesn't exist: {Path}", path);
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
                    Options.Logger.Fatal(ex, "FindPlugins - exception caught");
                }
            }
        }

        /// <summary>
        /// Unloads and Closes all AvailablePlugins
        /// </summary>
        public void ClosePlugins()
        {
            foreach (AvailablePlugin plugin in AvailablePlugins)
            {
                if (plugin.Instance == null)
                {
                    continue;
                }

                Options.Logger.Information("FindPlugins - disposing plugin: {Plugin}", plugin.Type.ToString());
                plugin.Instance.Unload();
                plugin.Instance = null;
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
                    Options.Logger.Information("FindPlugins - AddPlugin of type: {Type} from file: {FileName}", pluginType.ToString(), newPlugin.AssemblyPath);
                    newPlugin.CreateInstance();
                    newPlugin.Instance.Host = this;
                    newPlugin.Instance.Initialize();
                }

                AvailablePlugins.Add(newPlugin);
            }
        }

        public TileViewControl GetItemsControlTileView()
        {
            return ItemsControl.TileView;
        }

        public ItemsControl GetItemsControl()
        {
            return ItemsControl.RefMarker;
        }

        public int GetSelectedIdFromItemsControl()
        {
            return ItemsControl.RefMarker.SelectedGraphicId;
        }
    }
}