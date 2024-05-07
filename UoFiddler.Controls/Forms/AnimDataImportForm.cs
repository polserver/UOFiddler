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
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using static Ultima.Animdata;

namespace UoFiddler.Controls.Forms
{
    public partial class AnimDataImportForm : Form
    {
        public AnimDataImportForm()
        {
            InitializeComponent();
            comboBoxUpsertAction.SelectedItem = "skip";
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            try
            {
                var imported = ExportedAnimData.FromFile(textBox1.Text);
                var overwrite = comboBoxUpsertAction.Items[comboBoxUpsertAction.SelectedIndex].ToString() == "overwrite";

                int count = imported.UpdateAnimdata(AnimData, overwrite);

                MessageBox.Show($"Imported {count} animdata entries from: {textBox1.Text}\n\nDo not forget to save your changes!", "AnimData Import");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing animdata: ${ex.Message}", "AnimData Import", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
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
                textBox1.Text = dialog.FileName;
            }
        }
    }
}
