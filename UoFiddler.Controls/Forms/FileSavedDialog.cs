using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// A reusable dialog that displays a success message when a file is saved,
    /// with options to close or open the containing folder.
    /// </summary>
    public sealed partial class FileSavedDialog : Form
    {
        private readonly string _filePath;
        private Func<string, string?>? _localizationGetter;

        public FileSavedDialog(string filePath, string message = null, string title = null)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            InitializeComponent();

            // Set the system information icon
            iconPictureBox.Image = SystemIcons.Information.ToBitmap();

            if (!string.IsNullOrWhiteSpace(title))
            {
                Text = title;
            }

            statusLabel.Text = message ?? "File saved successfully.";
            pathLabel.Text = _filePath;
        }

        /// <summary>
        /// Sets the localization getter and applies localized text.
        /// </summary>
        public void SetLocalization(Func<string, string?>? getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        /// <summary>
        /// Applies localized text to UI elements.
        /// </summary>
        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            // Localize title
            var title = _localizationGetter("Forms.FileSavedDialog.Title");
            if (title != null)
                Text = title;

            // Localize status label if not already set to custom message
            var saveSuccess = _localizationGetter("Forms.FileSavedDialog.SaveSuccess");
            if (saveSuccess != null && statusLabel.Text == "File saved successfully.")
                statusLabel.Text = saveSuccess;

            // Localize buttons
            buttonOpenFolder.Text = _localizationGetter("Forms.FileSavedDialog.OpenFolder") ?? "Open Folder";
            buttonOk.Text = _localizationGetter("Forms.FileSavedDialog.OK") ?? "OK";
        }

        private void OnOpenFolderClick(object sender, EventArgs e)
        {
            try
            {
                string folderPath = Directory.Exists(_filePath) ? _filePath : Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = folderPath,
                        UseShellExecute = true
                    });

                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    $"Unable to open folder: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shows the file saved dialog.
        /// </summary>
        /// <param name="owner">The owner form.</param>
        /// <param name="filePath">The path to the saved file.</param>
        /// <param name="message">Optional custom success message.</param>
        /// <param name="title">Optional custom dialog title.</param>
        public static void Show(IWin32Window owner, string filePath, string message = null, string title = null)
        {
            using (var dialog = new FileSavedDialog(filePath, message, title))
            {
                dialog.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Shows the file saved dialog with localization support.
        /// </summary>
        /// <param name="owner">The owner form.</param>
        /// <param name="filePath">The path to the saved file.</param>
        /// <param name="message">Optional custom success message.</param>
        /// <param name="title">Optional custom dialog title.</param>
        /// <param name="getLocalized">Localization getter function.</param>
        public static void Show(IWin32Window owner, string filePath, string message, string title, Func<string, string?> getLocalized)
        {
            using (var dialog = new FileSavedDialog(filePath, message, title))
            {
                dialog.SetLocalization(getLocalized);
                dialog.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Shows the file saved dialog without an owner.
        /// </summary>
        /// <param name="filePath">The path to the saved file.</param>
        /// <param name="message">Optional custom success message.</param>
        /// <param name="title">Optional custom dialog title.</param>
        public static void Show(string filePath, string message = null, string title = null)
        {
            using (var dialog = new FileSavedDialog(filePath, message, title))
            {
                dialog.ShowDialog();
            }
        }
    }
}
