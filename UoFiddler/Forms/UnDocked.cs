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

using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    public partial class UnDocked : Form
    {
        private readonly TabPage _oldTab;

        public UnDocked(TabPage oldTab)
        {
            Control contr = oldTab.Controls[0];
            Controls.Clear();
            Controls.Add(contr);
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            // TODO: virtual member call in constructor?
            Text = oldTab.Text;
            _oldTab = oldTab;

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
            Control contr = Controls[0];
            _oldTab.Controls.Clear();
            _oldTab.Controls.Add(contr);

            MainForm.ReDock(_oldTab);
        }
    }
}
