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

namespace UoFiddler
{
    public partial class ExceptionForm : Form
    {
        public ExceptionForm(Exception err)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();

            if (err.InnerException != null)
                richTextBox.Text = String.Format("{0}\n{1}", err.InnerException.Message, err.InnerException.GetType());
            else
                richTextBox.Text = "";
            richTextBox.Text = String.Format("{0}\n\n{1}\n\n{2}", richTextBox.Text, err.Message, err.StackTrace);
        }

        private void onclick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}