/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    internal sealed class CommandRegistry
    {
        private readonly List<EditorCommand> _commands = new List<EditorCommand>();
        private readonly Dictionary<Keys, EditorCommand> _byKey = new Dictionary<Keys, EditorCommand>();
        private readonly List<(ToolStripItem Item, EditorCommand Command)> _items = new List<(ToolStripItem, EditorCommand)>();

        public IReadOnlyList<EditorCommand> Commands => _commands;

        public EditorCommand Add(EditorCommand command)
        {
            _commands.Add(command);
            if (command.Shortcut != Keys.None)
            {
                _byKey[command.Shortcut] = command;
            }

            if (command.AltShortcut != Keys.None)
            {
                _byKey[command.AltShortcut] = command;
            }

            return command;
        }

        /// <summary>Runs the command bound to a key. False when the key has no command or its command is disabled: the key is then free for whoever else wants it.</summary>
        public bool TryRun(Keys keyData)
        {
            if (!_byKey.TryGetValue(keyData, out EditorCommand command) || !command.Enabled)
            {
                return false;
            }

            command.Execute();
            return true;
        }

        public ToolStripMenuItem MenuItem(EditorCommand command)
        {
            var item = new ToolStripMenuItem(command.Text) { ShortcutKeyDisplayString = command.ShortcutText };
            item.Click += (_, _) => Run(command);
            _items.Add((item, command));
            return item;
        }

        public ToolStripButton Button(EditorCommand command, bool showText = false)
        {
            var button = new ToolStripButton(command.Text)
            {
                ToolTipText = command.ToolTip,
                AutoToolTip = false,
                Image = command.Glyph != null ? Glyphs.Render(command.Glyph, 18, SystemColors.ControlText) : null
            };
            button.DisplayStyle = button.Image == null
                ? ToolStripItemDisplayStyle.Text
                : showText ? ToolStripItemDisplayStyle.ImageAndText : ToolStripItemDisplayStyle.Image;
            button.Click += (_, _) => Run(command);
            _items.Add((button, command));
            return button;
        }

        /// <summary>Brings every menu item and button in line with the editor state.</summary>
        public void RefreshStates()
        {
            foreach ((ToolStripItem item, EditorCommand command) in _items)
            {
                item.Enabled = command.Enabled;
                bool isChecked = command.IsChecked != null && command.IsChecked();
                switch (item)
                {
                    case ToolStripMenuItem menu:
                        menu.Checked = isChecked;
                        break;
                    case ToolStripButton button:
                        button.Checked = isChecked;
                        break;
                }
            }
        }

        private void Run(EditorCommand command)
        {
            if (command.Enabled)
            {
                command.Execute();
            }

            RefreshStates();
        }
    }
}
