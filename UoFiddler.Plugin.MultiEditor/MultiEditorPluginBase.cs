/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Plugin;
using UoFiddler.Controls.Plugin.Interfaces;
using UoFiddler.Plugin.MultiEditor.UI;

namespace UoFiddler.Plugin.MultiEditor
{
    public class MultiEditorPluginBase : PluginBase
    {
        private MultiEditorControl _editor;

        public override string Author { get; } = "MuadDib & Turley";

        public override string Description { get; } = "Plugin to Edit Multis\r\n(Adds 1 new Tab)";

        public override IPluginHost Host { get; set; }

        public override string Name { get; } = "MultiEditorPlugin";

        public override string Version { get; } = "3.0.0";

        public override void Initialize()
        {
            // The Manage Plugins form also constructs disabled plugins, so events are
            // subscribed here and not in the constructor.
            PluginEvents.ModifyItemsControlContextMenuEvent += OnItemsContextMenu;
            PluginEvents.ModifyMapControlContextMenuEvent += OnMapContextMenu;
        }

        public override void Unload()
        {
            PluginEvents.ModifyItemsControlContextMenuEvent -= OnItemsContextMenu;
            PluginEvents.ModifyMapControlContextMenuEvent -= OnMapContextMenu;
        }

        public override void ModifyPluginToolStrip(ToolStripDropDownButton toolStrip)
        {
        }

        public override void ModifyTabPages(TabControl tabControl)
        {
            var page = new TabPage
            {
                Tag = tabControl.TabCount + 1, // dock/undock order
                Text = "Multi Editor"
            };

            _editor = new MultiEditorControl { Dock = DockStyle.Fill };
            page.Controls.Add(_editor);
            tabControl.TabPages.Add(page);

            // Once, the first time the application has nothing to do.
            EventHandler warmUp = null;
            warmUp = (_, _) =>
            {
                Application.Idle -= warmUp;
                MultiEditorControl.WarmUpOpenGl();
            };
            Application.Idle += warmUp;
        }

        private void OnItemsContextMenu(ContextMenuStrip strip)
        {
            var item = new ToolStripMenuItem { Text = "MultiEditor: Draw with this item" };
            item.Click += (_, _) =>
            {
                int selected = Host.GetSelectedIdFromItemsControl();
                if (selected > -1)
                {
                    _editor?.SelectDrawTile((ushort)selected);
                }
            };
            strip.Items.Add(item);
        }

        private void OnMapContextMenu(ContextMenuStrip strip, Func<(Map Map, System.Drawing.Point Location)> clickedAt)
        {
            var item = new ToolStripMenuItem { Text = "MultiEditor: Extract area here..." };
            item.Click += (_, _) =>
            {
                if (_editor == null)
                {
                    return;
                }

                // Show the editor first, so it is built before the extract.
                if (_editor.Parent is TabPage page && page.Parent is TabControl tabs)
                {
                    tabs.SelectedTab = page;
                }

                (Map map, System.Drawing.Point location) = clickedAt();
                _editor.ExtractFromMap(map, location);
            };
            strip.Items.Add(item);
        }
    }
}
