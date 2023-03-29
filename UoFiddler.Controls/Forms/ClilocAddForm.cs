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
    public partial class ClilocAddForm : Form
    {
        private readonly Func<int, bool> _isNumberFreeAction;
        private readonly Action<int> _addEntryAction;

        public ClilocAddForm(Func<int, bool> isNumberFreeAction, Action<int> addEntryAction)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            TopMost = true;

            _isNumberFreeAction = isNumberFreeAction;
            _addEntryAction = addEntryAction;
        }

        private void OnClickAdd(object sender, EventArgs e)
        {
            if (int.TryParse(NumberBox.Text, System.Globalization.NumberStyles.Integer, null, out int number))
            {
                if (_isNumberFreeAction(number))
                {
                    _addEntryAction(number);

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