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
    public sealed partial class ReplaceFromFolderResultForm : Form
    {
        public ReplaceFromFolderResultForm(string report)
        {
            InitializeComponent();
            reportTextBox.Text = report;
        }
    }
}
