using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using static Ultima.Animdata;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimDataImportForm : Form
    {
        public Action OnAfterImport { get; set; }
        public AnimDataImportForm()
        {
            InitializeComponent();
            cboConflictAction.SelectedItem = "skip";
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            try
            {
                var fileName = txtImportFileName.Text;
                var imported = ExportedAnimData.FromFile(fileName);
                var overwrite = cboConflictAction.Items[cboConflictAction.SelectedIndex].ToString() == "overwrite";

                // Create a new "working copy" AnimData to update, in case there's an exception thrown while processing.
                // Shallow clone is okay, as UpdateAnimdata does not modify existing entries' members.
                var workingAnimData = cbErase.Checked ? [] : new Dictionary<int, AnimdataEntry>(AnimData);
                int importCount = imported.UpdateAnimdata(workingAnimData, overwrite);

                // Since everything processed, set AnimData
                AnimData = workingAnimData;
                Options.ChangedUltimaClass["Animdata"] = true;

                try
                {
                    OnAfterImport?.Invoke();
                }
                catch
                {
                    // Swallow any error from the OnAfterImport callback
                }

                MessageBox.Show($"Imported {importCount} animdata entries from: {fileName}\n\nDo not forget to save your changes!", "AnimData Import");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing animdata: {ex.Message}", "AnimData Import", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            var type = "json";

            using OpenFileDialog dialog = new()
            {
                Multiselect = false,
                Title = $"Choose {type} file to import",
                CheckFileExists = true,
                Filter = string.Format("{0} file (*.{0})|*.{0}", type)
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtImportFileName.Text = dialog.FileName;
            }
        }
    }
}
