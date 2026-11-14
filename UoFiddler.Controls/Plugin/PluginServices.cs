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
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Controls.UserControls;
using UoFiddler.Controls.UserControls.TileView;
using Ultima.Helpers;

namespace UoFiddler.Controls.Plugin
{
    public class PluginServices : IPluginHost
    {
        private static readonly ILogger _log = AppLog.For(typeof(PluginServices));

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
            _log.LogInformation("FindPlugins - searching for plugins in [AppDomain.CurrentDomain.BaseDirectory = {Path}]", path);

            AvailablePlugins.Clear();
            if (!Directory.Exists(path))
            {
                _log.LogWarning("FindPlugins - plugin directory doesn't exist: {Path}", path);
                return;
            }

            RegisterDependencyResolver(path);

            foreach (string fileOn in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    AddPlugin(fileOn);
                }
                catch (Exception ex)
                {
                    _log.LogCritical(ex, "FindPlugins - exception caught");
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

                _log.LogInformation("FindPlugins - disposing plugin: {Plugin}", plugin.Type.ToString());
                plugin.Instance.Unload();
                plugin.Instance = null;
            }

            AvailablePlugins.Clear();
        }

        private static string _dependencyRoot;

        /// <summary>
        /// Lets a plugin ship third-party libraries in a subfolder of the plugin directory
        /// (plugins\PluginName\). Files directly in the plugin directory are all loaded as plugins,
        /// so dependencies cannot sit there.
        /// </summary>
        private static void RegisterDependencyResolver(string pluginDirectory)
        {
            bool first = _dependencyRoot == null;
            _dependencyRoot = pluginDirectory;
            if (!first)
            {
                return;
            }

            AssemblyLoadContext.Default.Resolving += (context, name) =>
            {
                string path = FindDependency(name.Name + ".dll");
                return path == null ? null : context.LoadFromAssemblyPath(path);
            };

            AssemblyLoadContext.Default.ResolvingUnmanagedDll += (_, libraryName) =>
            {
                string path = FindDependency(libraryName)
                              ?? (libraryName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                  ? null
                                  : FindDependency(libraryName + ".dll"));
                return path != null && NativeLibrary.TryLoad(path, out IntPtr handle) ? handle : IntPtr.Zero;
            };
        }

        private static string FindDependency(string fileName)
        {
            if (!Directory.Exists(_dependencyRoot))
            {
                return null;
            }

            foreach (string directory in Directory.GetDirectories(_dependencyRoot))
            {
                string candidate = Path.Combine(directory, fileName);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return null;
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
                    _log.LogInformation("FindPlugins - AddPlugin of type: {Type} from file: {FileName}", pluginType.ToString(), newPlugin.AssemblyPath);
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