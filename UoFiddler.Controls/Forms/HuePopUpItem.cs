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
    public partial class HuePopUpItem : Form
    {
        private readonly ItemDetail _refItemDetailItem;

        public HuePopUpItem(ItemDetail refItemDetail, int hue)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            if (hue >= 0)
            {
                control.Selected = hue;
            }

            _refItemDetailItem = refItemDetail;
        }

        private void Click_OK(object sender, EventArgs e)
        {
            _refItemDetailItem.DefHue = control.Selected;
            //this.Close();
            Hide();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            _refItemDetailItem.DefHue = -1;
            Hide();
            //this.Close();
        }

        public void SetHue(int hue)
        {
            control.Selected = hue;
        }
    }
}