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

namespace UoFiddler.Controls.Plugin
{
    /// <summary>
    /// Data Class for Available Plugin.
    /// </summary>
    public class AvailablePlugin
    {
        public bool Loaded { get; private set; }

        public Type Type { get; set; }

        public PluginBase Instance { get; set; }

        public string AssemblyPath { get; set; } = "";

        public void CreateInstance()
        {
            Instance = (PluginBase)Activator.CreateInstance(Type);
            Loaded = true;
        }
    }
}
