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
    public partial class ClilocDetail : Form
    {
        public ClilocDetail(int number, string text)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            TopMost = true;
            NumberLabel.Text = $"Nr: {number}";
            _number = number;
            TextBox.AppendText(text);
        }

        private readonly int _number;

        private void OnClickSave(object sender, EventArgs e)
        {
            Cliloc.SaveEntry(_number, TextBox.Text);
        }
    }
}