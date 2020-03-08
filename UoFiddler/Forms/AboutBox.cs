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
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    public partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            progresslabel.Visible = false;
            checkBoxCheckOnStart.Checked = FiddlerOptions.UpdateCheckOnStart;
            checkBoxFormState.Checked = FiddlerOptions.StoreFormState;
        }

        private void OnChangeCheck(object sender, EventArgs e)
        {
            FiddlerOptions.UpdateCheckOnStart = checkBoxCheckOnStart.Checked;
        }

        private void OnClickUpdate(object sender, EventArgs e)
        {
            CheckForUpdate();
        }

        private void CheckForUpdate()
        {
            progresslabel.Text = "Checking...";
            progresslabel.Visible = true;
            string[] match = FiddlerOptions.CheckForUpdate(out string error);
            if (match == null)
            {
                MessageBox.Show($"Error:\n{error}", "Check for Update", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                progresslabel.Text = "";
            }
            else if (match.Length == 2)
            {
                if (UoFiddler.Version.Equals(match[0]))
                {
                    MessageBox.Show("Your Version is up-to-date", "Check for Update");
                    progresslabel.Text = "";
                }
                else if (FiddlerOptions.VersionCheck(match[0]))
                {
                    DialogResult result =
                        MessageBox.Show(
                            $"A new version was found: {match[0]} your version: {UoFiddler.Version}\nDownload now?",
                            "Check for Update",
                            MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        DownloadFile(match[1]);
                    }
                }
                else
                {
                    DialogResult result = MessageBox.Show($"Your version differs: {UoFiddler.Version} Found: {match[0]}\nDownload now?", "Check for Update", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        DownloadFile(match[1]);
                    }
                    else
                    {
                        progresslabel.Text = "";
                    }
                }
            }
            else
            {
                MessageBox.Show("Failed to get version info", "Check for Update", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                progresslabel.Text = "";
            }
        }

        private void DownloadFile(string file)
        {
            progresslabel.Text = "Starting download...";
            string filepath = Options.OutputPath;
            string fileName = Path.Combine(filepath, file.Trim());

            using (WebClient web = new WebClient())
            {
                web.DownloadProgressChanged += OnDownloadProgressChanged;
                web.DownloadFileCompleted += OnDownloadFileCompleted;
                web.DownloadFileAsync(new Uri($"http://downloads.polserver.com/browser.php?download=./Projects/uofiddler/{file}"), fileName);
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progresslabel.Text = $"Downloading... bytes {e.BytesReceived}/{e.TotalBytesToReceive}";
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show($"An error occurred while downloading UOFiddler\n{e.Error.Message}",
                    "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            progresslabel.Text = "Finished Download";
        }

        private void OnClickLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://uofiddler.polserver.com/");
        }

        private void OnChangeFormState(object sender, EventArgs e)
        {
            FiddlerOptions.StoreFormState = checkBoxFormState.Checked;
        }
    }
}
