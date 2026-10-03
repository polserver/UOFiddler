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
using Microsoft.Extensions.Logging;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Plugin.Interfaces;
using Ultima.Helpers;

namespace UoFiddler.Controls.Plugin
{
    public abstract class PluginBase
    {
        public abstract IPluginHost Host { get; set; }

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public abstract string Version { get; }

        /// <summary>
        /// 静态属性：汉化字符串获取器，由主程序在启动时设置
        /// </summary>
        public static Func<string, string> LocalizationGetter { get; set; }

        /// <summary>
        /// Per-plugin logger, categorized by the concrete plugin type. Resolved on demand
        /// since plugins are instantiated via Activator and cannot take constructor parameters.
        /// </summary>
        protected ILogger Logger => AppLog.For(GetType());

        public abstract void Initialize();
        public abstract void Unload();

        /// <summary>
        /// On Startup called to modify the Plugin ToolStripDropDownButton
        /// </summary>
        /// <param name="toolStrip"></param>
        public virtual void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip) { }

        /// <summary>
        /// On Startup called to modify the tab pages
        /// </summary>
        /// <param name="tabControl"></param>
        public virtual void ModifyTabPages(TabControl tabControl) { }
    }
}
