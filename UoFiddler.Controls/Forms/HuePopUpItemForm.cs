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
    public partial class HuePopUpItemForm : Form
    {
        private readonly Action<int> _updateSelectedHueAction;

        public HuePopUpItemForm(Action<int> updateSelectedHueAction, int hue)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            control.HuesEditable = false;

            if (hue >= 0)
            {
                control.Selected = hue;
            }

            _updateSelectedHueAction = updateSelectedHueAction;
        }

        private void Click_OK(object sender, EventArgs e)
        {
            _updateSelectedHueAction(control.Selected);

            Hide();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            _updateSelectedHueAction(-1);

            Hide();
        }

        public void SetHue(int hue)
        {
            control.Selected = hue;
        }
    }
}