using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using static Ultima.Animdata;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimDataExportForm : Form
    {
        private Func<string, string?>? _localizationGetter;

        public AnimDataExportForm()
        {
            InitializeComponent();
            cboExportSelection.DataSource = new List<string> {
                "All (default-, blue-, and red-colored entries)",
                "Include missing animation tile flag (default- and blue-colored entries)",
                "Only valid animations (default-colored entries)"
            };
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.AnimDataExportForm.Title") ?? "Export AnimData";
            lblAnimationsToExport.Text = _localizationGetter("Forms.AnimDataExportForm.lblAnimationsToExport") ?? "Animations to export:";
            btnExport.Text = _localizationGetter("Forms.AnimDataExportForm.btnExport") ?? "Export";

            // 更新下拉菜单项
            var items = new List<string>
            {
                _localizationGetter("Forms.AnimDataExportForm.ExportSelection.All") ?? "All (default-, blue-, and red-colored entries)",
                _localizationGetter("Forms.AnimDataExportForm.ExportSelection.IncludeMissing") ?? "Include missing animation tile flag (default- and blue-colored entries)",
                _localizationGetter("Forms.AnimDataExportForm.ExportSelection.OnlyValid") ?? "Only valid animations (default-colored entries)"
            };
            cboExportSelection.DataSource = items;
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            var type = "json";

            using SaveFileDialog dialog = new()
            {
                CheckPathExists = true,
                Title = "Choose the file to export to",
                FileName = $"animdata-{DateTime.Now:yyyyMMddHHmm}.json",
                InitialDirectory = Options.OutputPath,
                Filter = string.Format("{0} file (*.{0})|*.{0}", type)
            };

            if (dialog.ShowDialog() != DialogResult.OK || dialog.FileName == "")
            {
                return;
            }

            try
            {
                var selection = cboExportSelection.SelectedIndex switch
                {
                    0 => ExportSelection.All,
                    1 => ExportSelection.IncludeMissingTileFlag,
                    2 => ExportSelection.OnlyValid,
                    _ => ExportSelection.All
                };

                var exported = ExportedAnimData.ToFile(dialog.FileName, AnimData, selection);

                // 使用汉化的导出成功消息
                string successMessage = _localizationGetter?.Invoke("Forms.AnimDataExportForm.ExportSuccessMessage") ?? $"Exported {exported.Data.Count} animdata entries to: {dialog.FileName}";
                successMessage = string.Format(successMessage, exported.Data.Count, dialog.FileName);
                MessageBox.Show(successMessage);
            }
            catch (Exception ex)
            {
                // 使用汉化的导出错误消息
                string errorTitle = _localizationGetter?.Invoke("Forms.AnimDataExportForm.ExportErrorTitle") ?? "AnimData Export";
                string errorMessage = _localizationGetter?.Invoke("Forms.AnimDataExportForm.ExportErrorMessage") ?? $"Error exporting animdata: {ex.Message}";
                errorMessage = string.Format(errorMessage, ex.Message);
                MessageBox.Show(errorMessage, errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
