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
    public partial class ItemSearch : Form
    {
        public ItemSearch()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
        }

        private void Search_Graphic(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(textBoxGraphic.Text, out int graphic, 0, Ultima.Art.GetMaxItemId()))
            {
                return;
            }

            bool res = Options.DesignAlternative
                ? ItemShowAlternative.SearchGraphic(graphic)
                : ItemShow.SearchGraphic(graphic);

            if (res)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No item found", "Result",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private void Search_ItemName(object sender, EventArgs e)
        {
            _lastSearchedName = textBoxItemName.Text;
            bool res = Options.DesignAlternative
                ? ItemShowAlternative.SearchName(textBoxItemName.Text, false)
                : ItemShow.SearchName(textBoxItemName.Text, false);

            if (res)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No item found", "Result",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private void SearchNextName(object sender, EventArgs e)
        {
            bool res = Options.DesignAlternative
                ? ItemShowAlternative.SearchName(textBoxItemName.Text, true)
                : ItemShow.SearchName(textBoxItemName.Text, true);

            if (res)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No item found", "Result",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                    Search_Graphic(sender, e);
                }
                else
                {
                    if (textBoxItemName.Text != _lastSearchedName)
                    {
                        Search_ItemName(sender, e);
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