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
    public partial class TextureSearch : Form
    {
        public TextureSearch()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
        }

        private void SearchGraphic(object sender, EventArgs e)
        {
            int graphic;
            bool canDone;
            if (textBoxGraphic.Text.Contains("0x"))
            {
                string convert = textBoxGraphic.Text.Replace("0x", "");
                canDone = int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out graphic);
            }
            else
            {
                canDone = int.TryParse(textBoxGraphic.Text, System.Globalization.NumberStyles.Integer, null, out graphic);
            }

            if (!canDone)
            {
                return;
            }

            bool res = Options.DesignAlternative ? TextureAlternative.SearchGraphic(graphic) : Texture.SearchGraphic(graphic);
            if (res)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No texture found", "Result", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private void OnKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchGraphic(this, null);
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
