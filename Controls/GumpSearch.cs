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
    public partial class GumpSearch : Form
    {
        public GumpSearch()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
        }

        private void SearchId(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(textBoxGraphic.Text, out int graphic, 0, Ultima.Art.GetMaxItemID()))
            {
                if (!Gump.Search(graphic))
                {
                    DialogResult result = MessageBox.Show("No used index found", "Result",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                    {
                        Close();
                    }
                }
            }
        }

        private void OnKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((TextBox)sender == textBoxGraphic)
                {
                    SearchId(this, null);
                }

                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}