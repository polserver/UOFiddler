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
using Microsoft.Extensions.Logging;
using UoFiddler.Controls.Classes;
using UoFiddler.Classes;
using UoFiddler.Localization;
using Ultima.Helpers;

namespace UoFiddler.Forms
{
    public partial class LoadProfileForm : Form
    {
        private readonly string[] _profiles;
        private readonly ILogger<LoadProfileForm> _log;
        private Func<string, string?>? _localizationGetter;

        public LoadProfileForm() : this(AppLog.For<LoadProfileForm>()) { }

        public LoadProfileForm(ILogger<LoadProfileForm> logger)
        {
            _log = logger;
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();
            _profiles = GetProfiles();
            _log.LogInformation("Found profiles: {Profiles}", _profiles);

            foreach (string profile in _profiles)
            {
                string name = profile.Substring(8);
                comboBoxLoad.Items.Add(name);
                comboBoxBasedOn.Items.Add(name);
            }

            comboBoxLoad.SelectedIndex = 0;
            comboBoxBasedOn.SelectedIndex = 0;

            // 汉化 getter 在 SetLocalization() 时才初始化
            // 这样确保 LocalizationService 已完全初始化
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            if (getLocalized == null) return;
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.LoadProfileForm.Title") ?? "Choose Profile";
            groupBox1.Text = _localizationGetter("Forms.LoadProfileForm.groupBox1") ?? "Load";
            groupBox2.Text = _localizationGetter("Forms.LoadProfileForm.groupBox2") ?? "Create";
            button1.Text = _localizationGetter("Forms.LoadProfileForm.button1") ?? "Load Profile";
            button2.Text = _localizationGetter("Forms.LoadProfileForm.button2") ?? "Create Profile";
            label1.Text = _localizationGetter("Forms.LoadProfileForm.label1") ?? "Based On";
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
            _log.LogInformation("Loading profile: {ProfileName}", Options.ProfileName);
            FiddlerOptions.LoadProfile($"{_profiles[comboBoxLoad.SelectedIndex]}.xml");

            Close();
        }

        private void OnClickCreate(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxCreate.Text))
            {
                string message = _localizationGetter?.Invoke("Forms.LoadProfileForm.Messages.ProfileNameMissing") ?? "Profile name is missing";
                string title = _localizationGetter?.Invoke("Forms.LoadProfileForm.Messages.NewProfileTitle") ?? "New Profile";
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                // 不关闭对话框，让用户重新输入
                textBoxCreate.Focus();
                return;
            }

            Options.ProfileName = $"Options_{textBoxCreate.Text}.xml";
            _log.LogInformation("Creating profile: {ProfileName}", Options.ProfileName);
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
    }
}
