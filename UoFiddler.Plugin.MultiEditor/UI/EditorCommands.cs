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

        /// <summary>Runs the command bound to a key. True when the key belongs to a command, also a disabled one.</summary>
        public bool TryRun(Keys keyData)
        {
            if (!_byKey.TryGetValue(keyData, out EditorCommand command))
            {
                return false;
            }

            if (command.Enabled)
            {
                command.Execute();
            }

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

    /// <summary>
    /// A pick-one-of-a-few toolbar item: shows the chosen text and drops a menu of the choices.
    /// Used instead of ToolStripComboBox, whose hosted combo box takes around 10 ms to repaint
    /// and made resizing the window stutter.
    /// </summary>
    internal sealed class ToolStripChoice : ToolStripDropDownButton
    {
        private readonly string[] _choices;
        private int _selectedIndex = -1;

        public ToolStripChoice(int width, params string[] choices)
        {
            _choices = choices;
            DisplayStyle = ToolStripItemDisplayStyle.Text;
            AutoSize = false;
            Width = width;
            TextAlign = ContentAlignment.MiddleLeft;
            for (int i = 0; i < choices.Length; i++)
            {
                int index = i;
                var item = new ToolStripMenuItem(choices[i]);
                item.Click += (_, _) => SelectedIndex = index;
                DropDownItems.Add(item);
            }

            SelectedIndex = 0;
        }

        public event EventHandler SelectedIndexChanged;

        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                value = Math.Clamp(value, 0, _choices.Length - 1);
                if (value == _selectedIndex)
                {
                    return;
                }

                _selectedIndex = value;
                Text = _choices[value];
                for (int i = 0; i < DropDownItems.Count; i++)
                {
                    ((ToolStripMenuItem)DropDownItems[i]).Checked = i == value;
                }

                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int IndexOf(string choice) => Array.IndexOf(_choices, choice);
    }

    /// <summary>
    /// Toolbar icons drawn from the Windows icon font, so the plugin ships no image files.
    /// </summary>
    internal static class Glyphs
    {
        public const string Select = "\uE8B0";
        public const string Lasso = "\uF408";
        public const string Draw = "\uE70F";
        public const string Remove = "\uE75C";
        public const string Altitude = "\uE8CB";
        public const string Pipette = "\uEF3C";
        public const string Transparent = "\uE7B3";
        public const string RectFill = "\uE739";
        public const string Line = "\uE738";
        public const string Wall = "\uE80F";
        public const string Floor = "\uE80A";
        public const string Door = "\uE8A7";
        public const string Stair = "\uE9D5";
        public const string Roof = "\uE80F";
        public const string Foundation = "\uE7C4";
        public const string New = "\uE7C3";
        public const string Open = "\uE8E5";
        public const string Save = "\uE74E";
        public const string Undo = "\uE7A7";
        public const string Redo = "\uE7A6";
        public const string Cut = "\uE8C6";
        public const string Copy = "\uE8C8";
        public const string Paste = "\uE77F";
        public const string Delete = "\uE74D";
        public const string Grid = "\uE80A";
        public const string ZoomIn = "\uE8A3";
        public const string ZoomOut = "\uE71F";
        public const string Fit = "\uE9A6";
        public const string Center = "\uE81D";
        public const string RotateLeft = "\uE7AD";
        public const string RotateRight = "\uE7AD";
        public const string Help = "\uE897";

        public static Bitmap Render(string glyph, int size, Color color)
        {
            var bitmap = new Bitmap(size, size);
            using Graphics g = Graphics.FromImage(bitmap);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using var font = new Font("Segoe MDL2 Assets", size * 0.72f, GraphicsUnit.Pixel);
            using var brush = new SolidBrush(color);
            using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(glyph, font, brush, new RectangleF(0, 0, size, size), format);
            return bitmap;
        }
    }

    /// <summary>
    /// Makes splitters visible: a row of dots in the middle of the bar, the usual sign that it
    /// can be dragged. A bare splitter is the same colour as the panels and easy to miss.
    /// </summary>
    internal static class SplitterGrip
    {
        public const int Thickness = 7;

        /// <summary>Draws the dots along the middle of a splitter bar.</summary>
        public static void Draw(Graphics g, Rectangle bar)
        {
            bool wide = bar.Width >= bar.Height;
            const int dots = 5, spacing = 6, size = 2;
            int length = (dots - 1) * spacing;
            int x = bar.X + (wide ? (bar.Width - length) / 2 : (bar.Width - size) / 2);
            int y = bar.Y + (wide ? (bar.Height - size) / 2 : (bar.Height - length) / 2);
            using var brush = new SolidBrush(SystemColors.GrayText);
            for (int i = 0; i < dots; i++)
            {
                g.FillRectangle(brush, x + (wide ? i * spacing : 0), y + (wide ? 0 : i * spacing), size, size);
            }
        }

        /// <summary>Gives a split container's bar the dots.</summary>
        public static void Attach(SplitContainer container)
        {
            container.SplitterWidth = Thickness;
            container.Paint += (_, e) => Draw(e.Graphics, container.SplitterRectangle);
            container.SplitterMoved += (_, _) => container.Invalidate();
            container.Resize += (_, _) => container.Invalidate();
        }
    }

    /// <summary>A docked splitter that shows the dots.</summary>
    internal sealed class GripSplitter : Splitter
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            SplitterGrip.Draw(e.Graphics, ClientRectangle);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }

    /// <summary>
    /// Small input dialogs.
    /// </summary>
    internal static class Prompt
    {
        /// <summary>Asks for whole numbers. Returns null when cancelled.</summary>
        public static int[] Numbers(IWin32Window owner, string title, params (string Label, int Value, int Min, int Max)[] fields)
        {
            using var form = new NumbersPromptForm(title, fields);
            if (form.ShowDialog(owner) != DialogResult.OK)
            {
                return null;
            }

            return form.Values;
        }

        /// <summary>Asks for one line of text. Returns null when cancelled or left empty.</summary>
        public static string Text(IWin32Window owner, string title, string label, string value = "")
        {
            using var form = new TextPromptForm(title, label, value);
            return form.ShowDialog(owner) == DialogResult.OK && form.Value.Trim().Length > 0 ? form.Value.Trim() : null;
        }

        /// <summary>Reads a tile id typed as decimal or 0x hex.</summary>
        public static bool TryParseId(string text, out ushort id)
        {
            text = (text ?? string.Empty).Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return ushort.TryParse(text.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out id);
            }

            return ushort.TryParse(text, out id);
        }
    }
}
