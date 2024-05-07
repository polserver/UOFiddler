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
using System.IO;
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
        }

        private void OnClickExport(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Options.OutputPath, $"animdata-{DateTime.Now:yyyyMMddHHmm}.json");

            try
            {
                var exported = ExportedAnimData.ToFile(fileName, AnimData, cbIncludeInvalidTiles.Checked, cbIncludeMissingAnimation.Checked);

                MessageBox.Show($"Exported {exported.Data.Count} animdata entries to: {fileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting animdata: {ex.Message}", "AnimData Export", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }
    }
}
