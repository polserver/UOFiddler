/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Plugin.MapPreview.UI;

namespace UoFiddler.Plugin.MapPreview
{
    /// <summary>Adds "In-game preview here..." to the map tab's context menu.</summary>
    public class MapPreviewPluginBase : PluginBase
    {
        public override string Author { get; } = "UOFiddler";

        public override string Description { get; } = "Previews a map area as the client draws it\r\n(Adds a map context menu entry and a settings entry under Plugins)";

        public override IPluginHost Host { get; set; }

        public override string Name { get; } = "MapPreviewPlugin";

        public override string Version { get; } = "1.0.0";

        public override void Initialize()
        {
            // The Manage Plugins form also constructs disabled plugins, so events are
            // subscribed here and not in the constructor.
            PluginEvents.ModifyMapControlContextMenuEvent += OnMapContextMenu;
        }

        public override void Unload()
        {
            PluginEvents.ModifyMapControlContextMenuEvent -= OnMapContextMenu;
            _window?.Close();
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
            var settings = new ToolStripMenuItem { Text = "In-game preview settings..." };
            settings.Click += (_, _) => MapPreviewForm.EditSettings(toolStrip.Owner?.FindForm());
            toolStrip.DropDownItems.Add(settings);
        }

        public override void ModifyTabPages(TabControl tabControl)
        {
        }

        /// <summary>The one preview window, reused for every spot until it is closed.</summary>
        private MapPreviewForm _window;

        private void OnMapContextMenu(ContextMenuStrip strip, Func<(Map Map, Point Location)> clickedAt)
        {
            var preview = new ToolStripMenuItem { Text = "In-game preview here" };
            preview.Click += (_, _) =>
            {
                (Map map, Point location) = clickedAt();
                if (map == null)
                {
                    return;
                }

                if (_window == null || _window.IsDisposed)
                {
                    _window = new MapPreviewForm();
                    _window.FormClosed += (_, _) => _window = null;
                    _window.Show(strip.SourceControl?.FindForm());
                }
                else if (_window.WindowState == FormWindowState.Minimized)
                {
                    _window.WindowState = FormWindowState.Normal;
                }

                _window.ShowArea(map, location);
                _window.Activate();
            };

            // Settings stay under Plugins only: next to this entry they were too easy to hit by mistake.
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(preview);
        }
    }
}
