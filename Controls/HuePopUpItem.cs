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
    public partial class HuePopUpItem : Form
    {
        private FiddlerControls.ItemDetail refItem;
        public HuePopUpItem(FiddlerControls.ItemDetail ref_, int hue)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            if (hue >= 0)
                control.Selected = hue;
            refItem = ref_;
        }

        private void Click_OK(object sender, EventArgs e)
        {
            int Selected = control.Selected;
            refItem.DefHue = Selected;
            //this.Close();
            this.Hide();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            refItem.DefHue = -1;
            this.Hide();
            //this.Close();
        }
        public void SetHue(int hue)
        {
            control.Selected = hue;
        }
    }
}