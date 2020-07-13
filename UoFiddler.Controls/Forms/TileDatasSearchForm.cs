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
    public partial class TileDatasSearchForm : Form
    {
        private readonly bool _land;

        public TileDatasSearchForm(bool landTile)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _land = landTile;
        }

        private void SearchGraphic(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(textBoxGraphic.Text, out int graphic, 0, Ultima.Art.GetMaxItemID()))
            {
                return;
            }

            bool res = TileDataControl.SearchGraphic(graphic, _land);
            if (res)
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
            bool res = TileDataControl.SearchName(textBoxItemName.Text, false, _land);
            if (res)
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
            bool res = TileDataControl.SearchName(textBoxItemName.Text, true, _land);
            if (res)
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
