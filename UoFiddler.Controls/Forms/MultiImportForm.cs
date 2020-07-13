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
using UoFiddler.Controls.UserControls;

namespace UoFiddler.Controls.Forms
{
    public partial class MultiImportForm : Form
    {
        public MultiImportForm(MultisControl parent, int id)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _id = id;
            _parent = parent;
            comboBox1.SelectedIndex = 0;
        }

        private readonly int _id;
        private readonly MultisControl _parent;

        private void OnClickBrowse(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false
            };
            string type = "txt";
            switch (comboBox1.SelectedIndex)
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
            }
            dialog.Title = $"Choose {type} file to import";
            dialog.CheckFileExists = true;
            dialog.Filter = type == "uoab" ? "{0} file (*.uoa)|*.uoa" : string.Format("{0} file (*.{0})|*.{0}", type);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.FileName;
            }

            dialog.Dispose();
        }

        private void OnClickImport(object sender, EventArgs e)
        {
            if (!File.Exists(textBox1.Text))
            {
                return;
            }

            Multis.ImportType type = (Multis.ImportType)comboBox1.SelectedIndex;
            MultiComponentList multi = Multis.ImportFromFile(_id, textBox1.Text, type);
            _parent.ChangeMulti(_id, multi);
            Options.ChangedUltimaClass["Multis"] = true;
            Close();
        }
    }
}
