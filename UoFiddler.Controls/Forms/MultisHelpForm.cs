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

using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

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
            AddHeader("Preview");
            Add("Fit preview to window",        "Toggle button on toolbar — scales to fit or shows at 100% with scrollbars");

            AddHeader("Zoom (100% mode only)");
            Add("Ctrl + Mouse Wheel",           "Zoom In / Out");
            Add("Shift + = (Plus key)",         "Zoom In");
            Add("- (Minus key)",                "Zoom Out");
            Add("Numpad +",                     "Zoom In");
            Add("Numpad -",                     "Zoom Out");
            Add("Ctrl + 0",                     "Reset zoom to 100%");

            AddHeader("Panning (100% mode only)");
            Add("Left-click drag",              "Pan the view");
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
