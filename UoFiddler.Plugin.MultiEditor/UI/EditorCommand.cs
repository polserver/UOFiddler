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

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// One thing the editor can do, with its name, keys and state. Menus, toolbar buttons, key
    /// handling and the shortcut list are all made from these, so they cannot drift apart.
    /// </summary>
    internal sealed class EditorCommand
    {
        public EditorCommand(string group, string text, Action execute)
        {
            Group = group;
            Text = text;
            Execute = execute;
        }

        public string Group { get; }

        public string Text { get; }

        public Action Execute { get; }

        public Keys Shortcut { get; init; }

        /// <summary>A second key for the same command.</summary>
        public Keys AltShortcut { get; init; }

        /// <summary>Segoe MDL2 Assets glyph for the toolbar, or null for a text button.</summary>
        public string Glyph { get; init; }

        public Func<bool> CanExecute { get; init; }

        public Func<bool> IsChecked { get; init; }

        public bool Enabled => CanExecute == null || CanExecute();

        public string ShortcutText => Format(Shortcut) + (AltShortcut != Keys.None ? ", " + Format(AltShortcut) : string.Empty);

        public string ToolTip => Shortcut == Keys.None ? Text : $"{Text} ({Format(Shortcut)})";

        private static string Format(Keys keys)
        {
            if (keys == Keys.None)
            {
                return string.Empty;
            }

            string text = string.Empty;
            if ((keys & Keys.Control) != 0)
            {
                text += "Ctrl+";
            }

            if ((keys & Keys.Shift) != 0)
            {
                text += "Shift+";
            }

            if ((keys & Keys.Alt) != 0)
            {
                text += "Alt+";
            }

            Keys code = keys & Keys.KeyCode;
            string name = code switch
            {
                Keys.OemOpenBrackets => "[",
                Keys.OemCloseBrackets => "]",
                Keys.Oemcomma => ",",
                Keys.OemPeriod => ".",
                Keys.Oemplus or Keys.Add => "+",
                Keys.OemMinus or Keys.Subtract => "-",
                Keys.PageUp => "PgUp",
                Keys.PageDown => "PgDn",
                Keys.Delete => "Del",
                Keys.Escape => "Esc",
                >= Keys.D0 and <= Keys.D9 => ((int)(code - Keys.D0)).ToString(),
                _ => code.ToString()
            };

            return text + name;
        }
    }
}
