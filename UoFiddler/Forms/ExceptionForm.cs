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
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UoFiddler.Classes;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Forms
{
    public partial class ExceptionForm : Form
    {
        private readonly Exception _error;

        public ExceptionForm(Exception err)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _error = err;

            richTextBox.Text = err.InnerException != null
                ? $"{err.InnerException.Message}\n{err.InnerException.GetType()}"
                : string.Empty;

            richTextBox.Text = $"{richTextBox.Text}\n\n{err.Message}\n\n{err.StackTrace}";
        }

        private void OnClick(object sender, EventArgs e)
        {
            Close();
        }

        private string ReportText => $"UOFiddler {FiddlerOptions.AppVersion}{Environment.NewLine}{_error}";

        private bool TryCopyReport()
        {
            try
            {
                Clipboard.SetText(ReportText);
                return true;
            }
            catch (ExternalException)
            {
                // The clipboard can be held by another process; copying is a convenience only.
                return false;
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            CopyButton.Text = TryCopyReport() ? "Copied" : "Copy failed";
        }

        private void OnClickOpenLogFolder(object sender, EventArgs e)
        {
            string logFolder = Path.Combine(Options.AppDataPath, "log");

            try
            {
                Directory.CreateDirectory(logFolder);
                Process.Start(new ProcessStartInfo { FileName = logFolder, UseShellExecute = true });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
            {
                MessageBox.Show(this, ex.Message, "Open Log Folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Opens a new GitHub issue titled after the error. The full details go to the clipboard to be
        /// pasted in, since a stack trace easily exceeds what a URL can carry.
        /// </summary>
        private void OnClickReport(object sender, EventArgs e)
        {
            bool copied = TryCopyReport();

            string title = Uri.EscapeDataString($"Crash: {_error.GetType().Name}: {_error.Message}");
            string body = Uri.EscapeDataString(
                "**Steps to reproduce:**\n\n\n**Exception details:**\n" +
                (copied ? "(paste from clipboard)\n" : "(use Copy Details in the error window)\n"));

            string url = $"https://github.com/{FiddlerOptions.RepositoryOwner}/{FiddlerOptions.RepositoryName}/issues/new?title={title}&body={body}";

            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Report on GitHub", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
