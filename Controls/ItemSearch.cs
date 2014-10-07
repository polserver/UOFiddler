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
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
        }

        private void Search_Graphic(object sender, EventArgs e)
        {
            int graphic;
            if (Utils.ConvertStringToInt(textBoxGraphic.Text, out graphic, 0, Ultima.Art.GetMaxItemID()))
            {
                bool res;
                if (Options.DesignAlternative)
                    res = ItemShowAlternative.SearchGraphic(graphic);
                else
                    res = ItemShow.SearchGraphic(graphic);
                if (!res)
                {
                    DialogResult result = MessageBox.Show("No item found", "Result",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                        Close();
                }
            }
        }

        private void Search_ItemName(object sender, EventArgs e)
        {
            bool res;
            if (Options.DesignAlternative)
                res = ItemShowAlternative.SearchName(textBoxItemName.Text, false);
            else
                res = ItemShow.SearchName(textBoxItemName.Text, false);
            if (!res)
            {
                DialogResult result = MessageBox.Show("No item found", "Result",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    Close();
            }
        }

        private void SearchNextName(object sender, EventArgs e)
        {
            bool res;
            if (Options.DesignAlternative)
                res = ItemShowAlternative.SearchName(textBoxItemName.Text, true);
            else
                res = ItemShow.SearchName(textBoxItemName.Text, true);
            if (!res)
            {
                DialogResult result = MessageBox.Show("No item found", "Result",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    Close();
            }
        }

        private void onKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((TextBox)sender == textBoxGraphic)
                    Search_Graphic(this, null);
                else
                    Search_ItemName(this, null);
                e.SuppressKeyPress = true;
            }
        }
    }
}