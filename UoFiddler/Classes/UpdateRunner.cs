﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UoFiddler.Classes
{
    internal static class UpdateRunner
    {
        public static async Task RunAsync(string repositoryOwner, string repositoryName, Version currentVersion, bool upToDateNotification = true)
        {
            const string updateCheckCaption = "Check for update";

            var updateChecker = new UpdateChecker(repositoryOwner, repositoryName, currentVersion);

            var response = await updateChecker.CheckUpdateAsync().ConfigureAwait(false);
            if (response.HasErrors)
            {
                MessageBox.Show($"Update checking failed: {response.ErrorMessage}");
            }
            else if (response.IsNewVersion)
            {
                string text = $"A new version was found: {response.NewVersion}\r\nDownload now?";
                DialogResult result = MessageBox.Show(text, updateCheckCaption, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start(response.HtmlUrl);
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
    }
}
