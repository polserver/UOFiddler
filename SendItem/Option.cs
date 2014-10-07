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

namespace FiddlerPlugin
{
    public partial class Option : Form
    {
        public Option()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            cmdtext.Text = SendItemPlugin.Cmd;
            argstext.Text = SendItemPlugin.CmdArg;
            SendOnClick.Checked = SendItemPlugin.OverrideClick;
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            SendItemPlugin.Cmd = cmdtext.Text;
            SendItemPlugin.CmdArg = argstext.Text;
            SendItemPlugin.OverrideClick = SendOnClick.Checked;
            Close();
        }
    }
}
