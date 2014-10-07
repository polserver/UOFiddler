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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace UoFiddler
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            progresslabel.Visible = false;
            checkBoxCheckOnStart.Checked = Options.UpdateCheckOnStart;
            checkBoxFormState.Checked = Options.StoreFormState;
        }

        private void OnChangeCheck(object sender, EventArgs e)
        {
            Options.UpdateCheckOnStart = checkBoxCheckOnStart.Checked;
        }

        private void OnClickUpdate(object sender, EventArgs e)
        {
            progresslabel.Text = "Checking...";
            progresslabel.Visible = true;
            string error;
            string[] match = Options.CheckForUpdate(out error);
            if (match == null)
            {
                MessageBox.Show("Error:\n" + error, "Check for Update", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                progresslabel.Text = "";
                return;
            }
            else if (match.Length == 2)
            {
                if (UoFiddler.Version.Equals(match[0]))
                {
                    MessageBox.Show("Your Version is up-to-date", "Check for Update");
                    progresslabel.Text = "";
                }
                else if (Options.VersionCheck(match[0]))
                {
                    DialogResult result =
                        MessageBox.Show(String.Format(@"A new version was found: {1} your version: {0}"
                        , UoFiddler.Version, match[0]) + "\nDownload now?", "Check for Update", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                        DownloadFile(match[1]);
                }
                else
                {
                    DialogResult result =
                        MessageBox.Show(String.Format(@"Your version differs: {0} Found: {1}"
                        , UoFiddler.Version, match[0]) + "\nDownload now?", "Check for Update", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                        DownloadFile(match[1]);
                    else
                        progresslabel.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Failed to get Versioninfo", "Check for Update", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                progresslabel.Text = "";
            }
        }

        private void DownloadFile(string file)
        {
            progresslabel.Text = "Starting download...";
            string filepath = FiddlerControls.Options.OutputPath;
            string FileName = Path.Combine(filepath, file);

            using (WebClient web = new WebClient())
            {
                web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChanged);
                web.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadFileCompleted);
                web.DownloadFileAsync(new Uri(String.Format(@"http://downloads.polserver.com/browser.php?download=./Projects/uofiddler/{0}", file)), FileName);
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progresslabel.Text = String.Format("Downloading... bytes {0}/{1}", e.BytesReceived, e.TotalBytesToReceive);
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("An error occurred while downloading UOFiddler\n" + e.Error.Message,
                    "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            progresslabel.Text = "Finished Download";
        }

        private void OnClickLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"http://uofiddler.polserver.com/");
        }

        private void OnChangeFormState(object sender, EventArgs e)
        {
            Options.StoreFormState = checkBoxFormState.Checked;
        }
    }
}
