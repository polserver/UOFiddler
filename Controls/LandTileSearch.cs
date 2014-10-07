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
    public partial class LandTileSearch : Form
    {
        public LandTileSearch()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
        }

        private void SearchGraphic(object sender, EventArgs e)
        {
            int graphic;
            if (Utils.ConvertStringToInt(textBoxGraphic.Text, out graphic, 0, 0x3FFF))
            {
                bool res;
                if (Options.DesignAlternative)
                    res = LandTilesAlternative.SearchGraphic(graphic);
                else
                    res = LandTiles.SearchGraphic(graphic);
                if (!res)
                {
                    DialogResult result = MessageBox.Show(
                        "No landtile found",
                        "Result",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Error,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Cancel)
                        Close();
                }
            }
        }

        private void SearchName(object sender, EventArgs e)
        {
            bool res;
            if (Options.DesignAlternative)
                res = LandTilesAlternative.SearchName(textBoxItemName.Text, false);
            else
                res = LandTiles.SearchName(textBoxItemName.Text, false);
            if (!res)
            {
                DialogResult result = MessageBox.Show(
                    "No landtile found",
                    "Result",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    Close();
            }
        }

        private void SearchNextName(object sender, EventArgs e)
        {
            bool res;
            if (Options.DesignAlternative)
                res = LandTilesAlternative.SearchName(textBoxItemName.Text, true);
            else
                res = LandTiles.SearchName(textBoxItemName.Text, true);
            if (!res)
            {
                DialogResult result = MessageBox.Show(
                    "No landtile found",
                    "Result",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    Close();
            }
        }

        private void onKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((TextBox)sender == textBoxGraphic)
                    SearchGraphic(this, null);
                else
                    SearchName(this, null);
                e.SuppressKeyPress = true;
            }
        }
    }
}
