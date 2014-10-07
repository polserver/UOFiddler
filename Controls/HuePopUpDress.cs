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
    public partial class HuePopUpDress : Form
    {
        private FiddlerControls.Dress refItem;
        private int layer;
        public HuePopUpDress(FiddlerControls.Dress ref_, int hue, int l)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            if ((hue & 0x8000) != 0)
            {
                hue ^= 0x8000;
                HueOnlyGray.Checked = true;
            }
            if (hue >= 0)
                control.Selected = hue - 1;
            refItem = ref_;
            layer = l;
        }

        private void Click_OK(object sender, EventArgs e)
        {
            int Selected = control.Selected + 1;
            if (HueOnlyGray.Checked)
                Selected ^= 0x8000;
            refItem.SetHue(layer, Selected);
            refItem.RefreshDrawing();
            this.Close();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            refItem.SetHue(layer, -1);
            refItem.RefreshDrawing();
            this.Close();
        }
    }
}