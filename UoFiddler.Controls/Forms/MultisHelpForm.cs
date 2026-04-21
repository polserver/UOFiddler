/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    public partial class MultisHelpForm : Form
    {
        public MultisHelpForm()
        {
            InitializeComponent();
            PopulateShortcuts();
        }

        private void PopulateShortcuts()
        {
            _listView.Groups.Add(new ListViewGroup("preview", "Preview"));
            _listView.Groups.Add(new ListViewGroup("zoom", "Zoom (100% mode only)"));
            _listView.Groups.Add(new ListViewGroup("pan", "Panning (100% mode only)"));

            Add("Fit preview to window",        "Toggle button on toolbar — scales to fit or shows at 100% with scrollbars", "preview");

            Add("Ctrl + Mouse Wheel",           "Zoom In / Out",         "zoom");
            Add("Shift + = (Plus key)",         "Zoom In",               "zoom");
            Add("- (Minus key)",                "Zoom Out",              "zoom");
            Add("Numpad +",                     "Zoom In",               "zoom");
            Add("Numpad -",                     "Zoom Out",              "zoom");
            Add("Ctrl + 0",                     "Reset zoom to 100%",    "zoom");

            Add("Left-click drag",              "Pan the view",          "pan");
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
