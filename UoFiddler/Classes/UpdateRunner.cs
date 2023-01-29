using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Classes
{
    internal static class UpdateRunner
    {
        public static async Task RunAsync(string repositoryOwner, string repositoryName, Version currentVersion, bool upToDateNotification = true)
        {
            const string updateCheckCaption = "Check for update";

            try
            {
                var updateChecker = new UpdateChecker(repositoryOwner, repositoryName, currentVersion);

                var response = await updateChecker.CheckUpdateAsync().ConfigureAwait(false);
                if (response.HasErrors)
                {
                    MessageBox.Show($"Update check failed: {response.ErrorMessage}");
                }
                else if (response.IsNewVersion)
                {
                    string text = $"A new version was found: {response.NewVersion}\r\nDownload now?";
                    DialogResult result = MessageBox.Show(text, updateCheckCaption, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = response.HtmlUrl,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    if (!upToDateNotification)
                    {
                        return;
                    }

                    MessageBox.Show("Your version is up-to-date", updateCheckCaption);
                }
            }
            catch (Exception ex)
            {
                // some other exception (connection issues probably). Put in log and continue as usual.
                if (upToDateNotification)
                {
                    MessageBox.Show("Update check failed. Check application log for details.", updateCheckCaption);
                }

                FiddlerOptions.Logger.Error(ex, "Update check failed");
            }
        }
    }
}
