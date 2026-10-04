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
using Ultima;

namespace UoFiddler.Plugin.ExamplePlugin.UserControls
{
    public partial class ExampleControl : UserControl
    {
        public ExampleControl()
        {
            InitializeComponent();
            this.Load += (s, e) => ApplyLocalization();
        }

        public void SetLocalization(Func<string, string> localizationGetter)
        {
            _localizationGetter = localizationGetter;
            
            // 如果控件已显示，立即应用汉化
            if (this.Visible)
            {
                ApplyLocalization();
            }
        }

        private Func<string, string> _localizationGetter;

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            button1.Text = _localizationGetter("Forms.PluginTestForm.SayHello") ?? "Say Hello";
        }

        private void OnClickSayHello(object sender, EventArgs e)
        {
            if (Client.Running)
            {
                Client.SendText("Hello World... I am an example plugIn form.");
            }
            else
            {
                string message = _localizationGetter?.Invoke("Forms.PluginTestForm.messagebox_hello") 
                    ?? "UO client is not running so I will say hello here. Hi!";
                MessageBox.Show(message);
            }
        }
    }
}
