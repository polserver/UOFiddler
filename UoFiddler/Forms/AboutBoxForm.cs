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
using System.Diagnostics;
using System.Windows.Forms;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    public partial class AboutBoxForm : Form
    {
        public AboutBoxForm()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            checkBoxCheckOnStart.Checked = FiddlerOptions.UpdateCheckOnStart;
            checkBoxFormState.Checked = FiddlerOptions.StoreFormState;
        }

        private void OnChangeCheck(object sender, EventArgs e)
        {
            FiddlerOptions.UpdateCheckOnStart = checkBoxCheckOnStart.Checked;
        }

        private async void OnClickUpdate(object sender, EventArgs e)
        {
            await UpdateRunner.RunAsync(FiddlerOptions.RepositoryOwner, FiddlerOptions.RepositoryName, FiddlerOptions.AppVersion).ConfigureAwait(false);
        }

        private void OnClickLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "http://uofiddler.polserver.com/",
                UseShellExecute = true
            });
        }

        private void OnChangeFormState(object sender, EventArgs e)
        {
            FiddlerOptions.StoreFormState = checkBoxFormState.Checked;
        }
    }
}
