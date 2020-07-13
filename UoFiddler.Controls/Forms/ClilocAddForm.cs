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
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class ClilocAddForm : Form
    {
        public ClilocAddForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            TopMost = true;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            if (int.TryParse(NumberBox.Text, System.Globalization.NumberStyles.Integer, null, out int number))
            {
                if (ClilocControl.IsNumberFree(number))
                {
                    ClilocControl.AddEntry(number);
                    Close();
                }
                else
                {
                    MessageBox.Show("Number not free.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
                }
            }
            else
            {
                MessageBox.Show("Error reading Number", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            Close();
        }
    }
}