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
using Ultima;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MultiImportForm : Form
    {
        private readonly int _id;
        private readonly Action<int, MultiComponentList> _changeMultiAction;

        public MultiImportForm(int id, Action<int, MultiComponentList> changeMultiAction)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _id = id;
            _changeMultiAction = changeMultiAction;

            importTypeComboBox.SelectedIndex = 0;
        }

        private void OnClickBrowse(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Multiselect = false })
            {
                string type = "txt";

                switch (importTypeComboBox.SelectedIndex)
                {
                    case 0:
                        type = "txt";
                        break;
                    case 1:
                        type = "uoa";
                        break;
                    case 2:
                        type = "uoab";
                        break;
                    case 3:
                        type = "wsc";
                        break;
                    case 4:
                        type = "csv";
                        break;
                }

                dialog.Title = $"Choose {type} file to import";
                dialog.CheckFileExists = true;
                dialog.Filter = type == "uoab" ? "{0} file (*.uoa)|*.uoa" : string.Format("{0} file (*.{0})|*.{0}", type);

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    filenameTextBox.Text = dialog.FileName;
                }
            }
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            if (!File.Exists(filenameTextBox.Text))
            {
                return;
            }

            Multis.ImportType type = (Multis.ImportType)importTypeComboBox.SelectedIndex;
            MultiComponentList multi = Multis.ImportFromFile(_id, filenameTextBox.Text, type);

            _changeMultiAction(_id, multi);

            Options.ChangedUltimaClass["Multis"] = true;

            Close();
        }
    }
}
