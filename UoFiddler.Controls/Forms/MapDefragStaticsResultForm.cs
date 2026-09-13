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
using System.Windows.Forms;
using Ultima.Statics;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public sealed partial class MapDefragStaticsResultForm : Form
    {
        private readonly StaticsDefragResult _result;
        private readonly bool _filtered;

        public MapDefragStaticsResultForm(StaticsDefragResult result, bool filtered)
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            _result = result;
            _filtered = filtered;

            reportTextBox.Text = result.ToReport();

            // Nothing was written in a dry run, so there is no output to verify or to open.
            buttonVerify.Enabled = !result.DryRun;
            buttonOpenFolder.Enabled = !result.DryRun;
        }

        /// <summary>
        /// Reads the output back and compares it against the file it was made from. With no filters
        /// the two must hold exactly the same statics; with filters the output may only be missing
        /// statics, and the number missing has to match what the filters reported removing.
        /// </summary>
        private void OnClickVerify(object sender, EventArgs e)
        {
            using (new WaitCursorScope(this))
            {
                try
                {
                    StaticsCompareMode mode = _filtered ? StaticsCompareMode.Subset : StaticsCompareMode.Identical;

                    StaticsCompareResult compare = StaticsComparer.Compare(
                        _result.SourceIndexPath, _result.SourceStaticsPath,
                        _result.OutputIndexPath, _result.OutputStaticsPath,
                        _result.BlockWidth, _result.BlockHeight, mode);

                    string reconciliation = string.Empty;

                    if (_filtered)
                    {
                        bool reconciled = compare.TilesMissing == _result.TilesAccountedFor;

                        reconciliation = Environment.NewLine +
                            $"Filters reported removing {_result.TilesAccountedFor:N0} statics, the comparison found {compare.TilesMissing:N0} missing: " +
                            (reconciled ? "reconciled." : "MISMATCH - statics were lost outside the filters.") +
                            Environment.NewLine;
                    }

                    reportTextBox.Text = compare.ToReport() + reconciliation +
                                         Environment.NewLine + new string('-', 60) + Environment.NewLine +
                                         _result.ToReport();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Verify failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog
            {
                Title = "Save the defrag report",
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"defrag-statics{_result.FileIndex}.txt",
                InitialDirectory = Options.OutputPath
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, reportTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnClickCopy(object sender, EventArgs e)
        {
            if (reportTextBox.TextLength > 0)
            {
                Clipboard.SetText(reportTextBox.Text);
            }
        }

        private void OnClickOpenFolder(object sender, EventArgs e)
        {
            string folder = Path.GetDirectoryName(_result.OutputIndexPath);

            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Unable to open folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}