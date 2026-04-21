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

using System.Windows.Forms;

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
            _listView.Groups.Add(new ListViewGroup("tool-switches", "Tool Switches"));
            _listView.Groups.Add(new ListViewGroup("editing", "Editing"));
            _listView.Groups.Add(new ListViewGroup("navigation", "Navigation"));
            _listView.Groups.Add(new ListViewGroup("zoom", "Zoom"));

            Add("S",                     "Select Tool",           "tool-switches");
            Add("D",                     "Draw Tool",             "tool-switches");
            Add("R",                     "Remove Tool",           "tool-switches");
            Add("E",                     "Apply Z Level Tool",    "tool-switches");
            Add("F",                     "Toggle Virtual Floor",  "tool-switches");
            Add("P",                     "Pipette (Pick Tile)",   "tool-switches");
            Add("T",                     "Switch Transparent",    "tool-switches");
            Add("B",                     "Rectangle Fill Tool",   "tool-switches");
            Add("L",                     "Line Draw Tool",        "tool-switches");

            Add("Ctrl+Z",                "Undo",                  "editing");
            Add("Ctrl+Y / Ctrl+Shift+Z", "Redo",                  "editing");
            Add("Ctrl+C",                "Copy Selection",         "editing");
            Add("Ctrl+V",                "Paste",                  "editing");
            Add("Escape",                "Cancel Paste",           "editing");
            Add("[",                     "Z Level -1",             "editing");
            Add("]",                     "Z Level +1",             "editing");

            Add("Arrow Keys",            "Pan View",               "navigation");
            Add("Page Up",               "Floor Z +5",             "navigation");
            Add("Page Down",             "Floor Z -5",             "navigation");

            Add("+ / Shift+=",           "Zoom In",                "zoom");
            Add("-",                     "Zoom Out",               "zoom");
            Add("Ctrl+0",                "Reset Zoom to 100%",     "zoom");
        }

        private void Add(string key, string action, string groupKey)
        {
            var group = _listView.Groups[groupKey];
            var item = new ListViewItem(key, group);
            item.SubItems.Add(action);
            _listView.Items.Add(item);
        }
    }
}