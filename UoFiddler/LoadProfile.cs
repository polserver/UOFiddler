using System;
using System.Windows.Forms;
using System.IO;

namespace UoFiddler
{
    public partial class LoadProfile : Form
    {
        private string[] profiles;
        private bool Exit;
        public LoadProfile()
        {
            Exit = true;
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            InitializeComponent();
            profiles = GetProfiles();
            foreach (string profile in profiles)
            {
                string name = profile.Substring(8);
                comboBoxLoad.Items.Add(name);
                comboBoxBasedOn.Items.Add(name);
            }
            comboBoxLoad.SelectedIndex = 0;
            comboBoxBasedOn.SelectedIndex = 0;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (Exit)
                Application.Exit();
        }

    
        public string[] GetProfiles()
        {
            string[] files = Directory.GetFiles(FiddlerControls.Options.AppDataPath, "Options_*.xml", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < files.Length; i++)
            {
                string[] path = files[i].Split(Path.DirectorySeparatorChar);
                files[i] = path[path.Length - 1];
                files[i] = files[i].Substring(0, files[i].Length - 4);
            }

            return files;
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            FiddlerControls.Options.ProfileName = profiles[comboBoxLoad.SelectedIndex] + ".xml";
            Options.LoadProfile(profiles[comboBoxLoad.SelectedIndex] + ".xml");
            Exit = false;
            Close();
        }

        private void OnClickCreate(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxCreate.Text))
            {
                MessageBox.Show("Profile name is missing", "New Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            FiddlerControls.Options.ProfileName = "Options_" + textBoxCreate.Text + ".xml";
            Options.LoadProfile(profiles[comboBoxBasedOn.SelectedIndex] + ".xml");
            Exit = false;
            Close();
        }
    }
}
