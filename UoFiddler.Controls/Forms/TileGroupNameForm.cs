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

using System;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class TileGroupNameForm : Form
    {
        public string EnteredName => NameTextBox.Text.Trim();
        public int EnteredTid => (int)TidUpDown.Value;

        public TileGroupNameForm(string title, string nameLabel, string initialName, bool showTidField, int initialTid)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            Text = title;
            PromptLabel.Text = nameLabel;
            NameTextBox.Text = initialName ?? string.Empty;
            NameTextBox.SelectAll();

            TidLabel.Visible = showTidField;
            TidUpDown.Visible = showTidField;
            if (showTidField)
            {
                TidUpDown.Value = Math.Min(TidUpDown.Maximum, Math.Max(TidUpDown.Minimum, initialTid));
            }
        }

        private void OnClickOk(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Name cannot be empty.", "Invalid name",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            // The name is a column of a tab-separated line in a single-byte file: a tab or line
            // break would shift or split the row, and other characters can't be stored.
            foreach (char c in NameTextBox.Text)
            {
                if (char.IsControl(c) || c > 'ÿ')
                {
                    MessageBox.Show(
                        "The name can't contain tabs, line breaks or characters outside Western European (Latin-1) text.",
                        "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
