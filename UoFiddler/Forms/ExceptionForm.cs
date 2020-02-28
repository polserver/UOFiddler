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

namespace UoFiddler.Forms
{
    public partial class ExceptionForm : Form
    {
        public ExceptionForm(Exception err)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            richTextBox.Text = err.InnerException != null
                ? $"{err.InnerException.Message}\n{err.InnerException.GetType()}"
                : string.Empty;

            richTextBox.Text = $"{richTextBox.Text}\n\n{err.Message}\n\n{err.StackTrace}";
        }

        private void OnClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}