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
    public partial class HuePopUpDress : Form
    {
        private readonly Dress _refDressItem;
        private readonly int _layer;

        public HuePopUpDress(Dress refDress, int hue, int l)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            if ((hue & 0x8000) != 0)
            {
                hue ^= 0x8000;
                HueOnlyGray.Checked = true;
            }
            if (hue >= 0)
            {
                control.Selected = hue - 1;
            }

            _refDressItem = refDress;
            _layer = l;
        }

        private void Click_OK(object sender, EventArgs e)
        {
            int selected = control.Selected + 1;
            if (HueOnlyGray.Checked)
            {
                selected ^= 0x8000;
            }

            _refDressItem.SetHue(_layer, selected);
            _refDressItem.RefreshDrawing();
            Close();
        }

        private void OnClick_Clear(object sender, EventArgs e)
        {
            _refDressItem.SetHue(_layer, -1);
            _refDressItem.RefreshDrawing();
            Close();
        }
    }
}