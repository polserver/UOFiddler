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
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class MapMarkerForm : Form
    {
        private readonly Action<int, int, int, Color, string> _addOverlayAction;
        private static Color _lastColor = Color.FromArgb(180, Color.Yellow);
        private Func<string, string?>? _localizationGetter;

        public MapMarkerForm(Action<int, int, int, Color, string> addOverlayAction, int x, int y, int map)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            _addOverlayAction = addOverlayAction;

            numericUpDown1.Value = x;
            numericUpDown2.Value = y;

            comboBox1.Items.AddRange(Options.MapNames);
            comboBox1.SelectedIndex = map;

            pictureBox1.BackColor = _lastColor;
        }

        public void SetLocalization(Func<string, string?> getLocalized)
        {
            _localizationGetter = getLocalized;
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            if (_localizationGetter == null) return;

            this.Text = _localizationGetter("Forms.MapMarkerForm.Title") ?? "Add MapMarker";
            label1.Text = _localizationGetter("Forms.MapMarkerForm.TextLabel") ?? "Text";
            button1.Text = _localizationGetter("Forms.MapMarkerForm.ChooseColorButton") ?? "Choose Color";
            button2.Text = _localizationGetter("Forms.MapMarkerForm.SaveButton") ?? "Save";
        }

        private void OnClickColor(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() != DialogResult.Cancel)
            {
                _lastColor = pictureBox1.BackColor = colorDialog1.Color;
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            _addOverlayAction(
                (int)numericUpDown1.Value,
                (int)numericUpDown2.Value,
                comboBox1.SelectedIndex,
                pictureBox1.BackColor,
                textBox1.Text);

            Close();
        }
    }
}
