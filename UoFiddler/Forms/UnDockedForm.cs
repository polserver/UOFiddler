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

namespace UoFiddler.Forms
{
    public partial class UnDockedForm : Form
    {
        private readonly TabPage _oldTab;
        private readonly Action<TabPage> _reDockAction;

        public UnDockedForm(TabPage oldTab, Action<TabPage> reDockAction)
        {
            Control control = oldTab.Controls[0];
            Controls.Clear();
            Controls.Add(control);

            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            // TODO: virtual member call in constructor?
            Text = oldTab.Text;
            _oldTab = oldTab;
            _reDockAction = reDockAction;

            if (ActiveForm?.TopMost == true)
            {
                TopMost = true;
            }

            ControlEvents.AlwaysOnTopChangeEvent += OnAlwaysOnTopChangeEvent;
        }

        private void OnAlwaysOnTopChangeEvent(bool value)
        {
            TopMost = value;
        }

        // TODO: unused?
        // public void ChangeControl(Control contr)
        // {
        //     Controls.Clear();
        //     Controls.Add(contr);
        //     PerformLayout();
        // }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            Control control = Controls[0];

            _oldTab.Controls.Clear();
            _oldTab.Controls.Add(control);

            _reDockAction(_oldTab);
        }
    }
}
