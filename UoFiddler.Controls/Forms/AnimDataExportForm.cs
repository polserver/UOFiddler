using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using static Ultima.Animdata;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimDataExportForm : Form
    {
        public AnimDataExportForm()
        {
            InitializeComponent();
            cboExportSelection.DataSource = new List<string> {
                "All (default-, blue-, and red-colored entries)",
                "Include missing animation tile flag (default- and blue-colored entries)",
                "Only valid animations (default-colored entries)"
            };
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

                MessageBox.Show($"Exported {exported.Data.Count} animdata entries to: {dialog.FileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting animdata: {ex.Message}", "AnimData Export", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
