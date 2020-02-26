using System;
using System.Windows.Forms;
using System.IO;

namespace UoFiddler
{
    public partial class LoadProfile : Form
    {
        private readonly string[] _profiles;

        private bool _exit;

        public LoadProfile()
        {
            _exit = true;
            Icon = FiddlerControls.Options.GetFiddlerIcon();
            InitializeComponent();
            _profiles = GetProfiles();

            foreach (string profile in _profiles)
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
            if (_exit)
                Application.Exit();
        }

        private static string[] GetProfiles()
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
            LoadSelectedProfile();
        }

        private void LoadSelectedProfile()
        {
            if (comboBoxLoad.SelectedIndex == -1)
            {
                return;
            }

            FiddlerControls.Options.ProfileName = $"{_profiles[comboBoxLoad.SelectedIndex]}.xml";
            Options.LoadProfile($"{_profiles[comboBoxLoad.SelectedIndex]}.xml");
            _exit = false;
            Close();
        }

        private void OnClickCreate(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxCreate.Text))
            {
                MessageBox.Show("Profile name is missing", "New Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            FiddlerControls.Options.ProfileName = $"Options_{textBoxCreate.Text}.xml";
            Options.LoadProfile($"{_profiles[comboBoxBasedOn.SelectedIndex]}.xml");
            _exit = false;
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
    }
}
