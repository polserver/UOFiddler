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
using PluginInterface;


namespace Host
{
    public class GlobalPlugins
    {
        public GlobalPlugins()
        {
        }
        public static Host.PluginServices Plugins = new PluginServices();
    }

    public class PluginServices : IPluginHost
    {
        public PluginServices()
        {
        }

        private Types.AvailablePlugins colAvailablePlugins = new Types.AvailablePlugins();

        /// <summary>
        /// A Collection of all Plugins Found
        /// </summary>
        public Types.AvailablePlugins AvailablePlugins
        {
            get { return colAvailablePlugins; }
            set { colAvailablePlugins = value; }
        }

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
        /// <param name="Path">Directory to search for Plugins in</param>
        public void FindPlugins(string Path)
        {
            colAvailablePlugins.Clear();
            if (!Directory.Exists(Path))
                return;
            foreach (string fileOn in Directory.GetFiles(Path))
            {
                FileInfo file = new FileInfo(fileOn);
                if (file.Extension.Equals(".dll"))
                {
                    try
                    {
                        if (!file.Name.Equals("Controls.dll") && (!file.Name.Equals("Ultima.dll")))
                            this.AddPlugin(fileOn);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Unloads and Closes all AvailablePlugins
        /// </summary>
        public void ClosePlugins()
        {
            foreach (Types.AvailablePlugin pluginOn in colAvailablePlugins)
            {
                if (pluginOn.Instance != null)
                {
                    pluginOn.Instance.Dispose();
                    pluginOn.Instance = null;
                }
            }
            colAvailablePlugins.Clear();
        }

        private void AddPlugin(string FileName)
        {
            Assembly pluginAssembly = Assembly.LoadFrom(FileName);
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (pluginType.IsPublic)
                {
                    if (!pluginType.IsAbstract)
                    {
                        if (pluginType.IsSubclassOf(typeof(IPlugin)))
                        {
                            Types.AvailablePlugin newPlugin = new Types.AvailablePlugin();
                            newPlugin.AssemblyPath = FileName;
                            newPlugin.Type = pluginAssembly.GetType(pluginType.ToString());
                            if (FiddlerControls.Options.PluginsToLoad.Contains(pluginType.ToString()))
                            {
                                newPlugin.CreateInstance();
                                newPlugin.Instance.Host = this;
                                newPlugin.Instance.Initialize();
                            }
                            this.colAvailablePlugins.Add(newPlugin);
                            newPlugin = null;
                        }
                    }
                }
            }
            pluginAssembly = null;
        }

        #region HostInterface
        public FiddlerControls.ItemShow GetItemShowControl()
        {
            return FiddlerControls.ItemShow.RefMarker;
        }

        public int GetSelectedItemShow()
        {
            if (FiddlerControls.ItemShow.ItemListView.SelectedItems.Count > 0)
                return (int)FiddlerControls.ItemShow.ItemListView.SelectedItems[0].Tag;
            else
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
        #endregion
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
            public void Add(Types.AvailablePlugin pluginToAdd)
            {
                this.List.Add(pluginToAdd);
            }

            /// <summary>
            /// Remove a Plugin to the collection of Available plugins
            /// </summary>
            /// <param name="pluginToRemove">The Plugin to Remove</param>
            public void Remove(Types.AvailablePlugin pluginToRemove)
            {
                this.List.Remove(pluginToRemove);
            }

            /// <summary>
            /// Finds a plugin in the available Plugins
            /// </summary>
            /// <param name="pluginNameOrPath">The name or File path of the plugin to find</param>
            /// <returns>Available Plugin, or null if the plugin is not found</returns>
            public Types.AvailablePlugin Find(string pluginNameOrPath)
            {
                Types.AvailablePlugin toReturn = null;
                foreach (Types.AvailablePlugin pluginOn in this.List)
                {
                    if ((pluginOn.Instance.Name.Equals(pluginNameOrPath)) || pluginOn.AssemblyPath.Equals(pluginNameOrPath))
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
            private IPlugin m_Instance = null;
            private string m_AssemblyPath = "";
            private Type m_type = null;
            private bool m_loaded = false;

            public bool Loaded
            {
                get { return m_loaded; }
            }

            public Type Type
            {
                get { return m_type; }
                set { m_type = value; }
            }

            public IPlugin Instance
            {
                get { return m_Instance; }
                set { m_Instance = value; }
            }
            public string AssemblyPath
            {
                get { return m_AssemblyPath; }
                set { m_AssemblyPath = value; }
            }

            public void CreateInstance()
            {
                m_Instance = (IPlugin)Activator.CreateInstance(m_type);
                m_loaded = true;
            }
        }
    }
}
