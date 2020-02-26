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
using Host.Types;
using PluginInterface;

namespace Host
{
    public static class GlobalPlugins
    {
        public static readonly PluginServices Plugins = new PluginServices();
    }

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

                if (!pluginType.IsSubclassOf(typeof(Plugin)))
                {
                    continue;
                }

                AvailablePlugin newPlugin = new AvailablePlugin
                {
                    AssemblyPath = fileName,
                    Type = pluginAssembly.GetType(pluginType.ToString())
                };

                if (FiddlerControls.Options.PluginsToLoad.Contains(pluginType.ToString()))
                {
                    newPlugin.CreateInstance();
                    newPlugin.Instance.Host = this;
                    newPlugin.Instance.Initialize();
                }

                AvailablePlugins.Add(newPlugin);
                //newPlugin = null; // TODO: to be removed?
            }
            //pluginAssembly = null; // TODO: to be removed?
        }

        public FiddlerControls.ItemShow GetItemShowControl()
        {
            return FiddlerControls.ItemShow.RefMarker;
        }

        public int GetSelectedItemShow()
        {
            if (FiddlerControls.ItemShow.ItemListView.SelectedItems.Count > 0)
            {
                return (int)FiddlerControls.ItemShow.ItemListView.SelectedItems[0].Tag;
            }

            return -1;
        }

        public ListView GetItemShowListView()
        {
            return FiddlerControls.ItemShow.ItemListView;
        }

        public FiddlerControls.ItemShowAlternative GetItemShowAltControl()
        {
            return FiddlerControls.ItemShowAlternative.RefMarker;
        }

        public PictureBox GetItemShowAltPictureBox()
        {
            return FiddlerControls.ItemShowAlternative.ItemPictureBox;
        }

        public int GetSelectedItemShowAlternative()
        {
            return FiddlerControls.ItemShowAlternative.RefMarker.Selected;
        }
    }

    namespace Types
    {
        /// <summary>
        /// Collection for AvailablePlugin Type
        /// </summary>
        public class AvailablePlugins : System.Collections.CollectionBase
        {
            /// <summary>
            /// Add a Plugin to the collection of Available plugins
            /// </summary>
            /// <param name="pluginToAdd">The Plugin to Add</param>
            public void Add(AvailablePlugin pluginToAdd)
            {
                List.Add(pluginToAdd);
            }

            /// <summary>
            /// Remove a Plugin to the collection of Available plugins
            /// </summary>
            /// <param name="pluginToRemove">The Plugin to Remove</param>
            public void Remove(AvailablePlugin pluginToRemove)
            {
                List.Remove(pluginToRemove);
            }

            /// <summary>
            /// Finds a plugin in the available Plugins
            /// </summary>
            /// <param name="pluginNameOrPath">The name or File path of the plugin to find</param>
            /// <returns>Available Plugin, or null if the plugin is not found</returns>
            public AvailablePlugin Find(string pluginNameOrPath)
            {
                AvailablePlugin toReturn = null;
                foreach (AvailablePlugin pluginOn in List)
                {
                    if (pluginOn.Instance.Name.Equals(pluginNameOrPath) ||
                        pluginOn.AssemblyPath.Equals(pluginNameOrPath))
                    {
                        toReturn = pluginOn;
                        break;
                    }
                }
                return toReturn;
            }
        }

        /// <summary>
        /// Data Class for Available Plugin.
        /// </summary>
        public class AvailablePlugin
        {
            public bool Loaded { get; private set; }

            public Type Type { get; set; }

            public Plugin Instance { get; set; }

            public string AssemblyPath { get; set; } = "";

            public void CreateInstance()
            {
                Instance = (Plugin)Activator.CreateInstance(Type);
                Loaded = true;
            }
        }
    }
}
