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
    public partial class HuePopUpForm : Form
    {
        private readonly AnimationListControl _animationListControlMob;

        public HuePopUpForm(AnimationListControl animationListControl, int hue)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            if ((hue & 0x8000) != 0)
            {
                hue ^= 0x8000;
                HueOnlyGray.Checked = true;
            }
            if (hue != 0)
            {
                control.Selected = hue;
            }

            _animationListControlMob = animationListControl;
        }

        private void Click_OK(object sender, EventArgs e)
        {
            int selected = control.Selected;
            if (HueOnlyGray.Checked)
            {
                selected ^= 0x8000;
            }

            _animationListControlMob.ChangeHue(selected);
            Close();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            _animationListControlMob.ChangeHue(-1);
            Close();
        }
    }
}