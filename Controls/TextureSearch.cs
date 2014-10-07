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
    public partial class TextureSearch : Form
    {
        public TextureSearch()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
        }

        private void SearchGraphic(object sender, EventArgs e)
        {
            int graphic;
            bool candone;
            if (textBoxGraphic.Text.Contains("0x"))
            {
                string convert = textBoxGraphic.Text.Replace("0x", "");
                candone = int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out graphic);
            }
            else
                candone = int.TryParse(textBoxGraphic.Text, System.Globalization.NumberStyles.Integer, null, out graphic);

            if (candone)
            {
                bool res;
                if (Options.DesignAlternative)
                    res = TextureAlternative.SearchGraphic(graphic);
                else
                    res = Texture.SearchGraphic(graphic);
                if (!res)
                {
                    DialogResult result = MessageBox.Show("No texture found", "Result", MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        Close();
                }
            }
        }

        private void onKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchGraphic(this, null);
                e.SuppressKeyPress = true;
            }
        }
    }
}
