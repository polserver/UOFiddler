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
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Plugin.SendItem.Forms
{
    public partial class SendItemOptionsForm : Form
    {
        public SendItemOptionsForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            cmdtext.Text = SendItemPluginBase.Cmd;
            argstext.Text = SendItemPluginBase.CmdArg;
            SendOnClick.Checked = SendItemPluginBase.OverrideClick;
            
            // 在 Load 事件中应用汉化
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

            this.Text = _localizationGetter("Forms.SendItemOptionsForm.Title") ?? "SendItem Options";
            groupBox1.Text = _localizationGetter("Forms.SendItemOptionsForm.groupBox1") ?? "Send Item";
            label7.Text = _localizationGetter("Forms.SendItemOptionsForm.label7") ?? "Cmd";
            label8.Text = _localizationGetter("Forms.SendItemOptionsForm.label8") ?? "Args";
            SendOnClick.Text = _localizationGetter("Forms.SendItemOptionsForm.SendOnClick") ?? "Send on DoubleClick";
            button1.Text = _localizationGetter("Forms.SendItemOptionsForm.button1") ?? "Save";
            
            // ToolTips
            var toolTip1 = this.components?.Components.OfType<ToolTip>().FirstOrDefault();
            if (toolTip1 != null)
            {
                toolTip1.SetToolTip(label7, _localizationGetter("Forms.SendItemOptionsForm.tooltip_label7") ?? "Defines the cmd to send Client for selected Item");
                toolTip1.SetToolTip(label8, _localizationGetter("Forms.SendItemOptionsForm.tooltip_label8") ?? "{1} = Selected item ObjType");
                toolTip1.SetToolTip(SendOnClick, _localizationGetter("Forms.SendItemOptionsForm.tooltip_SendOnClick") ?? "Overrides DoubleClick");
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            SendItemPluginBase.Cmd = cmdtext.Text;
            SendItemPluginBase.CmdArg = argstext.Text;
            SendItemPluginBase.OverrideClick = SendOnClick.Checked;
            Close();
        }
    }
}
