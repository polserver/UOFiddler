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
    public partial class HuePopUpDress : Form
    {
        private readonly Action<int> _changeHueAction;

        public HuePopUpDress(Action<int> changeHueAction, int hue)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            control.HuesEditable = false;

            _changeHueAction = changeHueAction;

            if ((hue & 0x8000) != 0)
            {
                hue ^= 0x8000;
                HueOnlyGray.Checked = true;
            }

            if (hue >= 0)
            {
                control.Selected = hue - 1;
            }
        }

        private void Click_OK(object sender, EventArgs e)
        {
            int selected = control.Selected + 1;

            if (HueOnlyGray.Checked)
            {
                selected ^= 0x8000;
            }

            _changeHueAction(selected);

            Close();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            _changeHueAction(-1);

            Close();
        }
    }
}