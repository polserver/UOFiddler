/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The list of keyboard shortcuts and mouse actions (F1). The command rows come from the
    /// editor's command registry, so they always match the keys that work; the mouse, tool and
    /// sidebar rows are written here.
    /// </summary>
    internal partial class ShortcutsForm : Form
    {
        private Font _headingFont;

        public ShortcutsForm()
        {
            InitializeComponent();
            Disposed += ShortcutsForm_Disposed;
        }

        /// <summary>Fills the list: every command with a key, grouped, then the mouse actions.</summary>
        public void ShowCommands(IEnumerable<EditorCommand> commands)
        {
            _headingFont ??= new Font(_list.Font, FontStyle.Bold);

            foreach (IGrouping<string, EditorCommand> group in commands.Where(c => c.Shortcut != Keys.None).GroupBy(c => c.Group))
            {
                Heading(group.Key);
                foreach (EditorCommand command in group)
                {
                    Row(command.Text, command.ShortcutText);
                }
            }

            // Keys handled outside the command registry: kept here by hand, so check them when a
            // tool or a sidebar list gets a new key.
            Section("Mouse",
                ("Pan", "Right or middle drag, Space + drag"),
                ("Zoom at the cursor", "Wheel"),
                ("Step through the floors", "Shift + wheel"),
                ("Context menu, or cancel a drag or paste", "Right click"),
                ("Select the tile under the cursor (any tool)", "Middle click"),
                ("Move the selection one cell", "Arrow keys"),
                ("Pan one cell", "Ctrl + arrow keys"));

            Section("Keys held with a tool",
                ("Move the selection (Select)", "Drag a selected tile"),
                ("Add a tile to the selection or take it out (Select)", "Ctrl + click"),
                ("Add to the selection (Select, Lasso)", "Ctrl + drag"),
                ("Keep the line straight (Line)", "Shift + drag"),
                ("One straight wall instead of a room (Walls)", "Shift + drag"),
                ("Erase walls (Walls)", "Ctrl + drag"),
                ("Fit the roof to the walls (Roof)", "Click without dragging"),
                ("Turn the ridge the other way (Roof)", "Shift"),
                ("Gables at one end or both (Roof)", "Ctrl"));

            Section("Tile browser",
                ("Go from the search box to the results", "Down"),
                ("Pick the first result", "Enter in the search box"),
                ("Clear the search", "Esc in the search box"),
                ("Move through the tiles", "Arrow keys, Home, End, Page Up, Page Down"),
                ("Pick the tile", "Enter, click"),
                ("Remove a tile from the favorites page", "Delete"),
                ("Place a blueprint", "Click, Enter"),
                ("Rename a blueprint", "F2"),
                ("Delete a blueprint", "Delete"),
                ("Open UO Architect designs (Designs page)", "Enter, double click"));

            Section("Selected tiles list",
                ("Take the chosen rows out of the selection", "Delete"),
                ("Keep only the chosen rows selected", "Enter"),
                ("Keep only this row selected", "Double click"),
                ("Choose every row", "Ctrl + A"));

            // The keys column fits its longest entry, and the window grows to show both columns whole.
            _list.AutoResizeColumn(_keysColumn.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
            _keysColumn.Width = Math.Max(_keysColumn.Width + 12, 160);
            int needed = _commandColumn.Width + _keysColumn.Width + SystemInformation.VerticalScrollBarWidth + 8;
            if (_list.ClientSize.Width < needed)
            {
                Width += needed - _list.ClientSize.Width;
            }
        }

        private void Section(string heading, params (string What, string How)[] rows)
        {
            Heading(heading);
            foreach ((string what, string how) in rows)
            {
                Row(what, how);
            }
        }

        /// <summary>
        /// Section headings are ordinary bold rows in the list's own text colour. ListView groups
        /// draw their titles in a fixed system blue, which cannot be read on a dark theme.
        /// </summary>
        private void Heading(string text)
        {
            if (_list.Items.Count > 0)
            {
                _list.Items.Add(new ListViewItem(string.Empty));
            }

            _list.Items.Add(new ListViewItem(text) { Font = _headingFont });
        }

        private void Row(string what, string how)
        {
            _list.Items.Add(new ListViewItem(new[] { "    " + what, how }));
        }

        private void ShortcutsForm_Disposed(object sender, EventArgs e)
        {
            _headingFont?.Dispose();
            _headingFont = null;
        }
    }
}
