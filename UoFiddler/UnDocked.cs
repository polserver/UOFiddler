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

namespace UoFiddler
{
    public partial class UnDocked : Form
    {
        private TabPage m_oldtab;

        public UnDocked(TabPage oldtab)
        {
            Control contr = oldtab.Controls[0];
            this.Controls.Clear();
            this.Controls.Add(contr);
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            this.Text = oldtab.Text;
            m_oldtab = oldtab;
            if (UoFiddler.ActiveForm.TopMost)
                this.TopMost = true;
            FiddlerControls.Events.AlwaysOnTopChangeEvent += new FiddlerControls.Events.AlwaysOnTopChangeHandler(OnAlwaysOnTopChangeEvent);
        }

        private void OnAlwaysOnTopChangeEvent(bool value)
        {
            this.TopMost = value;
        }

        public void ChangeControl(Control contr)
        {
            this.Controls.Clear();
            this.Controls.Add(contr);
            this.PerformLayout();

        }
        private void OnClose(object sender, FormClosingEventArgs e)
        {
            Control contr = this.Controls[0];
            m_oldtab.Controls.Clear();
            m_oldtab.Controls.Add(contr);

            UoFiddler.ReDock(m_oldtab);
        }
    }
}
