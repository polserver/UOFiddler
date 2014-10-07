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

namespace FiddlerControls
{
    public partial class ClilocAdd : Form
    {
        public ClilocAdd()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            TopMost = true;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            int number;
            if (int.TryParse(NumberBox.Text, System.Globalization.NumberStyles.Integer, null, out number))
            {
                if (Cliloc.IsNumberFree(number))
                {
                    Cliloc.AddEntry(number);
                    Close();
                }
                else
                    MessageBox.Show(
                        "Number not free.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button1);
            }
            else
                MessageBox.Show(
                    "Error reading Number",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
        }

        private void OnClickCancel(object sender, EventArgs e)
        {
            Close();
        }
    }
}