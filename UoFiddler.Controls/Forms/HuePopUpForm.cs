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
    public partial class HuePopUpForm : Form
    {
        private readonly Action<int> _changeHueAction;
        private Func<string, string?>? _localizationGetter;

        public HuePopUpForm(Action<int> changeHueAction, int hue)
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

            if (hue != 0)
            {
                control.Selected = hue;
            }
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.HuePopUpForm.Title") ?? "Hue Picker";
            toolStripButton1.Text = _localizationGetter("Forms.HuePopUpForm.OkButton") ?? "OK";
            toolStripButton2.Text = _localizationGetter("Forms.HuePopUpForm.ClearButton") ?? "Clear";
            HueOnlyGray.Text = _localizationGetter("Forms.HuePopUpForm.HueOnlyGrayCheckBox") ?? "Hue Only Gray (+0x8000)";
            
            // 为内部 HuesControl 调用汉化
            control.SetLocalization(_localizationGetter);
        }

        private void Click_OK(object sender, EventArgs e)
        {
            int selected = control.Selected;
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