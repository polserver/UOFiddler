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
    public partial class TileDataSearchForm : Form
    {
        private readonly bool _land;
        private readonly Func<int, bool, bool> _searchByIdCallback;
        private readonly Func<string, bool, bool, bool> _searchByNameCallback;

        public TileDataSearchForm(bool landTile, Func<int, bool, bool> searchByIdCallback, Func<string, bool, bool, bool> searchByNameCallback)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _land = landTile;
            _searchByIdCallback = searchByIdCallback;
            _searchByNameCallback = searchByNameCallback;
        }

        private void SearchGraphic(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(textBoxGraphic.Text, out int graphic, 0, Ultima.Art.GetMaxItemId()))
            {
                return;
            }

            bool exist = _searchByIdCallback(graphic, _land);
            if (exist)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No item found", "Result", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private void SearchName(object sender, EventArgs e)
        {
            _lastSearchedName = textBoxItemName.Text;

            bool exist = _searchByNameCallback(textBoxItemName.Text, false, _land);
            if (exist)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No item found", "Result", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private void SearchNextName(object sender, EventArgs e)
        {
            bool exist = _searchByNameCallback(textBoxItemName.Text, true, _land);
            if (exist)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No item found", "Result", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private string _lastSearchedName;

        private void OnKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((TextBox)sender == textBoxGraphic)
                {
                    SearchGraphic(sender, e);
                }
                else
                {
                    if (textBoxItemName.Text != _lastSearchedName)
                    {
                        SearchName(sender, e);
                    }
                    else
                    {
                        SearchNextName(sender, e);
                    }
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
