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
    public partial class ClilocDetailForm : Form
    {
        private readonly int _number;
        private readonly Action<int, string> _saveEntryAction;

        public ClilocDetailForm(int number, string text, Action<int, string> saveEntryAction)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            TopMost = true;
            NumberLabel.Text = $"Nr: {number}";
            TextBox.AppendText(text);

            _number = number;
            _saveEntryAction = saveEntryAction;
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            _saveEntryAction(_number, TextBox.Text);
        }
    }
}