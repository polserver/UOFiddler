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

namespace UoFiddler.Plugin.ExamplePlugin.Forms
{
    public partial class ExampleForm : Form
    {
        public ExampleForm()
        {
            InitializeComponent();
            this.Load += (s, e) => ApplyLocalization();
        }

        public void SetLocalization(Func<string, string> localizationGetter)
        {
            _localizationGetter = localizationGetter;
            
            // 如果窗体已显示，立即应用汉化
            if (this.Visible)
            {
                ApplyLocalization();
            }
        }

        private Func<string, string> _localizationGetter;

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.PluginTestForm.Title") ?? "Example";
        }
    }
}
