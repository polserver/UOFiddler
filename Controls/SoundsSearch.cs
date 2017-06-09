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
    public partial class SoundsSearch : Form
    {
        public SoundsSearch()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
        }

        private void Search_Id(object sender, EventArgs e)
        {
            int graphic;
            if (Utils.ConvertStringToInt(this.textBoxId.Text, out graphic, 0, 0xFFF))
            {
                bool res;
                    res = Sounds.SearchID(graphic);
                if (!res)
                {
                    DialogResult result = MessageBox.Show("No sound found", "Result",
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Cancel)
                        Close();
                }
            }
        }

        private void Search_SoundName(object sender, EventArgs e)
        {
            lastSearchedName = textBoxSoundName.Text;
            var res = Sounds.SearchName(this.textBoxSoundName.Text, false);
            if (!res)
            {
                DialogResult result = MessageBox.Show("No sound found", "Result",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    Close();
            }
        }

        private void SearchNextName(object sender, EventArgs e)
        {
            bool res;
                res = Sounds.SearchName(this.textBoxSoundName.Text, true);
            if (!res)
            {
                DialogResult result = MessageBox.Show("No sound found", "Result",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                    Close();
            }
        }

        private string lastSearchedName = null;
        private void onKeyDownSearch(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if ((TextBox)sender == this.textBoxId)
                    this.Search_Id(sender, e);
                else
                {
                    if (textBoxSoundName.Text != lastSearchedName)
                    {
                        Search_SoundName(sender, e);
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
                this.Close();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}