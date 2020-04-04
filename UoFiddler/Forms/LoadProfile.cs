using System;
using System.IO;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Classes;

namespace UoFiddler.Forms
{
    public partial class LoadProfile : Form
    {
        private readonly string[] _profiles;

        public LoadProfile()
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            _profiles = GetProfiles();
            FiddlerOptions.Logger.Information("Found profiles: {profiles}", _profiles);

            foreach (string profile in _profiles)
            {
                string name = profile.Substring(8);
                comboBoxLoad.Items.Add(name);
                comboBoxBasedOn.Items.Add(name);
            }

            comboBoxLoad.SelectedIndex = 0;
            comboBoxBasedOn.SelectedIndex = 0;
        }

        private static string[] GetProfiles()
        {
            string[] files = Directory.GetFiles(Options.AppDataPath, "Options_*.xml", SearchOption.TopDirectoryOnly);

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
            LoadSelectedProfile();
        }

        private void LoadSelectedProfile()
        {
            if (comboBoxLoad.SelectedIndex == -1)
            {
                return;
            }

            Options.ProfileName = $"{_profiles[comboBoxLoad.SelectedIndex]}.xml";
            FiddlerOptions.Logger.Information("Loading existing profile: {profileName}", Options.ProfileName);
            FiddlerOptions.LoadProfile($"{_profiles[comboBoxLoad.SelectedIndex]}.xml");

            Close();
        }

        private void OnClickCreate(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxCreate.Text))
            {
                MessageBox.Show("Profile name is missing", "New Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Options.ProfileName = $"Options_{textBoxCreate.Text}.xml";
            FiddlerOptions.Logger.Information("Creating new profile: {profileName}", Options.ProfileName);
            FiddlerOptions.LoadProfile($"{_profiles[comboBoxBasedOn.SelectedIndex]}.xml");

            Close();
        }

        private void ComboBoxLoad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadSelectedProfile();
            }
        }

        private void ComboBoxLoad_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1.Enabled = comboBoxLoad.SelectedIndex != -1;
        }

        private void ComboBoxLoad_KeyUp(object sender, KeyEventArgs e)
        {
            button1.Enabled = comboBoxLoad.SelectedIndex != -1;
        }

        private void LoadProfile_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
            {
                Application.Exit();
            }
        }
    }
}
