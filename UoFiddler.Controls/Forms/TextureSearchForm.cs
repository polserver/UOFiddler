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
    public partial class TextureSearchForm : Form
    {
        private readonly Func<int, bool> _searchByIdCallback;

        public TextureSearchForm(Func<int, bool> searchByIdCallback)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _searchByIdCallback = searchByIdCallback;
        }

        private void SearchGraphic(object sender, EventArgs e)
        {
            int graphic;
            bool isValidValue;
            if (graphicTextbox.Text.Contains("0x"))
            {
                string convert = graphicTextbox.Text.Replace("0x", "");
                isValidValue = int.TryParse(convert, System.Globalization.NumberStyles.HexNumber, null, out graphic);
            }
            else
            {
                isValidValue = int.TryParse(graphicTextbox.Text, System.Globalization.NumberStyles.Integer, null, out graphic);
            }

            if (!isValidValue)
            {
                return;
            }

            bool exists = _searchByIdCallback(graphic);
            if (exists)
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
