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
    public partial class SoundsSearch : Form
    {
        public SoundsSearch()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
        }

        private void Search_Id(object sender, EventArgs e)
        {
            if (!Utils.ConvertStringToInt(textBoxId.Text, out int graphic, 0, 0xFFF))
            {
                return;
            }

            bool res = Sounds.SearchId(graphic);
            if (res)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No sound found", "Result", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private void Search_SoundName(object sender, EventArgs e)
        {
            _lastSearchedName = textBoxSoundName.Text;
            bool res = Sounds.SearchName(textBoxSoundName.Text, false);
            if (res)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No sound found", "Result", MessageBoxButtons.OKCancel,
                MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            if (result == DialogResult.Cancel)
            {
                Close();
            }
        }

        private void SearchNextName(object sender, EventArgs e)
        {
            bool res = Sounds.SearchName(textBoxSoundName.Text, true);
            if (res)
            {
                return;
            }

            DialogResult result = MessageBox.Show("No sound found", "Result", MessageBoxButtons.OKCancel,
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
                if ((TextBox)sender == textBoxId)
                {
                    Search_Id(sender, e);
                }
                else
                {
                    if (textBoxSoundName.Text != _lastSearchedName)
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
                Close();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}