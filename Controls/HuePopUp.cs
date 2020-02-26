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
    public partial class HuePopUp : Form
    {
        private readonly AnimationList _animationListMob;

        public HuePopUp(AnimationList animationList, int hue)
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

            _animationListMob = animationList;
        }

        private void Click_OK(object sender, EventArgs e)
        {
            int selected = control.Selected;
            if (HueOnlyGray.Checked)
            {
                selected ^= 0x8000;
            }

            _animationListMob.ChangeHue(selected);
            Close();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            _animationListMob.ChangeHue(-1);
            Close();
        }
    }
}