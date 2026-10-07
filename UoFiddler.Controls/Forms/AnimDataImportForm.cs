using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using static Ultima.Animdata;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimDataImportForm : Form
    {
        private Func<string, string?>? _localizationGetter;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Action OnAfterImport { get; set; }

        public AnimDataImportForm()
        {
            InitializeComponent();
            cboConflictAction.SelectedItem = "skip";
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.AnimDataImportForm.Title") ?? "Import AnimData";
            label5.Text = _localizationGetter("Forms.AnimDataImportForm.label5") ?? "Import from:";
            btnImport.Text = _localizationGetter("Forms.AnimDataImportForm.btnImport") ?? "Import";
            label1.Text = _localizationGetter("Forms.AnimDataImportForm.label1") ?? "On conflict:";
            cbErase.Text = _localizationGetter("Forms.AnimDataImportForm.cbErase") ?? "Erase animdata before importing";

            // 更新下拉菜单项
            var skipText = _localizationGetter("Forms.AnimDataImportForm.ConflictAction.Skip") ?? "skip";
            var overwriteText = _localizationGetter("Forms.AnimDataImportForm.ConflictAction.Overwrite") ?? "overwrite";
            
            cboConflictAction.Items.Clear();
            cboConflictAction.Items.Add(skipText);
            cboConflictAction.Items.Add(overwriteText);
            cboConflictAction.SelectedIndex = 0;
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            try
            {
                var fileName = txtImportFileName.Text;
                var imported = ExportedAnimData.FromFile(fileName);
                // SelectedIndex: 0 = skip, 1 = overwrite
                var overwrite = cboConflictAction.SelectedIndex == 1;

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

                MessageBox.Show(
                    _localizationGetter?.Invoke("Forms.AnimDataImportForm.ImportSuccessMessage") ?? 
                    $"Imported {importCount} animdata entries from: {fileName}\n\nDo not forget to save your changes!",
                    _localizationGetter?.Invoke("Forms.AnimDataImportForm.ImportSuccessTitle") ?? "AnimData Import");
            }
            catch (Exception ex)
            {
                var errorTitle = _localizationGetter?.Invoke("Forms.AnimDataImportForm.ImportErrorTitle") ?? "AnimData Import";
                var errorMessageFormat = _localizationGetter?.Invoke("Forms.AnimDataImportForm.ImportErrorMessage") ?? "Error importing animdata: {0}";
                var errorMessage = string.Format(errorMessageFormat, ex.Message);
                
                MessageBox.Show(errorMessage, errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
