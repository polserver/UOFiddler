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
    /// The list of keyboard shortcuts and mouse actions (F1). The rows come from the editor's
    /// command registry, so the list always matches the keys that work.
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

            Heading("Mouse");
            foreach ((string what, string how) in new[]
                     {
                         ("Pan", "Right or middle drag, Space + drag"),
                         ("Zoom at the cursor", "Wheel"),
                         ("Step through the floors", "Shift + wheel"),
                         ("Context menu, or cancel a drag or paste", "Right click"),
                         ("Select the tile under the cursor (any tool)", "Middle click"),
                         ("Move the selection one cell", "Arrow keys"),
                         ("Pan one cell", "Ctrl + arrow keys")
                     })
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
