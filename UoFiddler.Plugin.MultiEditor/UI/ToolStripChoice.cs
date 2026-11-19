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

namespace UoFiddler.Plugin.MultiEditor.UI
{
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
}
