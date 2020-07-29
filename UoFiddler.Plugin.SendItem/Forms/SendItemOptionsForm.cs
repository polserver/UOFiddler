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
