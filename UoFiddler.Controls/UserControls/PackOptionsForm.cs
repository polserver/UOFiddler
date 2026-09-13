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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class AnimationListControl
    {
        // Pack options dialog (moved here to ensure compilation visibility)
        public class PackOptionsForm : Form
        {
            private CheckedListBox _directionsBox;
            private NumericUpDown _maxWidthUpDown;
            private CheckBox _oneRowPerDirectionCheckBox;
            private CheckBox _exportAllCheckBox;
            private TrackBar _spacingTrackBar;
            private Label _spacingLabel;
            private Button _ok;
            private Button _cancel;

            public List<int> SelectedDirections { get; private set; } = new List<int> { 0, 1, 2, 3, 4 };
            public int MaxWidth { get; private set; } = 2048;
            public bool OneRowPerDirection { get; private set; }
            public int FrameSpacing { get; private set; } = 0;
            public bool ExportAllAnimations { get; private set; }

            public PackOptionsForm()
            {
                Text = "Pack Options - By Moshu";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                // increased size to accommodate taller direction list and wider right side
                ClientSize = new Size(520, 490);
                Padding = new Padding(10);

                Label lbl = new Label { Text = "Directions:", Location = new Point(12, 12), AutoSize = true };
                Controls.Add(lbl);

                _directionsBox = new CheckedListBox
                {
                    Location = new Point(12, 32),
                    // make dropdown / list taller
                    Size = new Size(160, 260),
                    CheckOnClick = true,
                    ScrollAlwaysVisible = true,
                    IntegralHeight = false
                };
                for (int i = 0; i < 8; i++)
                {
                    _directionsBox.Items.Add(i.ToString(), i <= 4);
                }
                Controls.Add(_directionsBox);

                // move the max sprite width controls further to the right
                Label lbl2 = new Label { Text = "Max sprite width:", Location = new Point(190, 32), AutoSize = true };
                Controls.Add(lbl2);

                _maxWidthUpDown = new NumericUpDown
                {
                    Location = new Point(190, 56),
                    Size = new Size(260, 30),
                    Minimum = 256,
                    Maximum = 8192,
                    Increment = 64,
                    Value = 2048,
                    ThousandsSeparator = true
                };
                Controls.Add(_maxWidthUpDown);

                // Optional quick presets (moved right and made slightly taller)
                var presetsLabel = new Label { Text = "Presets:", Location = new Point(190, 95), AutoSize = true };
                Controls.Add(presetsLabel);
                var presetSmall = new Button { Text = "1024", Location = new Point(190, 115), Size = new Size(70, 34) };
                var presetMedium = new Button { Text = "2048", Location = new Point(266, 115), Size = new Size(70, 34) };
                var presetLarge = new Button { Text = "4096", Location = new Point(342, 115), Size = new Size(70, 34) };
                presetSmall.Click += (s, e) => _maxWidthUpDown.Value = 1024;
                presetMedium.Click += (s, e) => _maxWidthUpDown.Value = 2048;
                presetLarge.Click += (s, e) => _maxWidthUpDown.Value = 4096;
                Controls.Add(presetSmall);
                Controls.Add(presetMedium);
                Controls.Add(presetLarge);

                // Spacing Slider
                var spacingTitle = new Label { Text = "Spacing:", Location = new Point(190, 160), AutoSize = true };
                Controls.Add(spacingTitle);

                _spacingLabel = new Label { Text = "0", Location = new Point(460, 160), AutoSize = true };
                Controls.Add(_spacingLabel);

                _spacingTrackBar = new TrackBar
                {
                    Location = new Point(190, 180),
                    Size = new Size(280, 45),
                    Minimum = 0,
                    Maximum = 20,
                    Value = 0,
                    TickFrequency = 1
                };
                _spacingTrackBar.Scroll += (s, e) => _spacingLabel.Text = _spacingTrackBar.Value.ToString();
                Controls.Add(_spacingTrackBar);

                _oneRowPerDirectionCheckBox = new CheckBox
                {
                    Text = "One row per direction",
                    Location = new Point(12, 300),
                    AutoSize = true
                };
                Controls.Add(_oneRowPerDirectionCheckBox);

                _exportAllCheckBox = new CheckBox
                {
                    Text = "Export all animations",
                    Location = new Point(12, 330),
                    AutoSize = true
                };
                Controls.Add(_exportAllCheckBox);

                // make OK/Cancel taller and move to the right (anchored)
                _ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(300, 400), Size = new Size(100, 40), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
                _ok.Click += Ok_Click;
                Controls.Add(_ok);

                _cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Location = new Point(410, 400), Size = new Size(100, 40), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
                Controls.Add(_cancel);

                AcceptButton = _ok;
                CancelButton = _cancel;
            }

            private void Ok_Click(object sender, EventArgs e)
            {
                // Collect checked directions as integers
                SelectedDirections = _directionsBox.CheckedItems.Cast<object>().Select(o => int.Parse(o.ToString())).ToList();
                if (SelectedDirections.Count == 0)
                {
                    MessageBox.Show("Select at least one direction.", "Pack Options", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None;
                    return;
                }

                MaxWidth = (int)_maxWidthUpDown.Value;
                OneRowPerDirection = _oneRowPerDirectionCheckBox.Checked;
                FrameSpacing = _spacingTrackBar.Value;
                ExportAllAnimations = _exportAllCheckBox.Checked;
            }
        }
    }
}
