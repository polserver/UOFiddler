/***************************************************************************
 *
 * $Author: MuadDib & Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.MultiEditor.Forms
{
    public partial class ShortcutsHelpForm : Form
    {
        public ShortcutsHelpForm()
        {
            InitializeComponent();
            PopulateShortcuts();
        }

        private void PopulateShortcuts()
        {
            AddHeader("Tool Switches");
            Add("S",                     "Select Tool");
            Add("D",                     "Draw Tool");
            Add("R",                     "Remove Tool");
            Add("E",                     "Apply Z Level Tool");
            Add("F",                     "Toggle Virtual Floor");
            Add("P",                     "Pipette (Pick Tile)");
            Add("T",                     "Switch Transparent");
            Add("B",                     "Rectangle Fill Tool");
            Add("L",                     "Line Draw Tool");

            AddHeader("Editing");
            Add("Ctrl+Z",                "Undo");
            Add("Ctrl+Y / Ctrl+Shift+Z", "Redo");
            Add("Ctrl+C",                "Copy Selection");
            Add("Ctrl+V",                "Paste");
            Add("Escape",                "Cancel Paste");
            Add("[",                     "Z Level -1");
            Add("]",                     "Z Level +1");

            AddHeader("Navigation");
            Add("Arrow Keys",            "Pan View");
            Add("Page Up",               "Floor Z +5");
            Add("Page Down",             "Floor Z -5");

            AddHeader("Zoom");
            Add("+ / Shift+=",           "Zoom In");
            Add("-",                     "Zoom Out");
            Add("Ctrl+0",                "Reset Zoom to 100%");
        }

        private void AddHeader(string text)
        {
            var item = new ListViewItem(text)
            {
                Font = new Font(_listView.Font, FontStyle.Bold),
                ForeColor = Options.DarkMode ? Color.OrangeRed : Color.MediumBlue,
            };
            item.SubItems.Add(string.Empty);
            _listView.Items.Add(item);
        }

        private void Add(string key, string action)
        {
            var item = new ListViewItem(key);
            item.SubItems.Add(action);
            _listView.Items.Add(item);
        }
    }
}