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
    public partial class ClilocDetail : Form
    {
        public ClilocDetail(int Number, string Text)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            TopMost = true;
            NumberLabel.Text = String.Format("Nr: {0}", Number);
            m_Number = Number;
            TextBox.AppendText(Text);
        }

        private int m_Number;
        private void OnClickSave(object sender, EventArgs e)
        {
            Cliloc.SaveEntry(m_Number, TextBox.Text);
        }
    }
}