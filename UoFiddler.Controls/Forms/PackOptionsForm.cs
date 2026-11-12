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
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    /// <summary>
    /// Options for packing animation frames into a sprite sheet.
    /// </summary>
    public partial class PackOptionsForm : Form
    {
        /// <summary>
        /// Directions 0-4 are stored in the anim files; 5-7 are drawn by mirroring 3-1.
        /// </summary>
        public static readonly string[] DirectionNames =
        {
            "South", "South West", "West", "North West", "North", "North East", "East", "South East"
        };

        /// <summary>Number of directions stored in the anim files.</summary>
        public const int StoredDirections = 5;

        /// <summary>
        /// The stored direction <paramref name="direction"/> (5-7) is mirrored from.
        /// </summary>
        public static int GetMirroredSource(int direction) => 8 - direction;

        public PackOptionsForm()
        {
            InitializeComponent();

            Icon = Options.GetFiddlerIcon();

            for (int i = 0; i < DirectionNames.Length; ++i)
            {
                // Mirrored directions are off by default: they import back into their stored direction
                string text = i < StoredDirections
                    ? $"{i} {DirectionNames[i]}"
                    : $"{i} {DirectionNames[i]} (mirror of {GetMirroredSource(i)})";
                DirectionsListBox.Items.Add(text, i < StoredDirections);
            }
        }

        public List<int> SelectedDirections { get; } = new List<int>();

        public int MaxWidth => (int)MaxWidthUpDown.Value;

        public int Spacing => (int)SpacingUpDown.Value;

        public bool OneRowPerDirection => OneRowPerDirectionCheckBox.Checked;

        public bool ExportAllActions => ExportAllActionsCheckBox.Checked;

        private void OnClickPreset(object sender, EventArgs e)
        {
            MaxWidthUpDown.Value = decimal.Parse(((Button)sender).Text);
        }

        private void OnCheckedChangedOneRowPerDirection(object sender, EventArgs e)
        {
            // Rows follow the directions, so the width limit does not apply
            bool limitWidth = !OneRowPerDirectionCheckBox.Checked;
            MaxWidthUpDown.Enabled = limitWidth;
            Preset1024Button.Enabled = limitWidth;
            Preset2048Button.Enabled = limitWidth;
            Preset4096Button.Enabled = limitWidth;
        }

        private void OnClickOk(object sender, EventArgs e)
        {
            SelectedDirections.Clear();
            foreach (int index in DirectionsListBox.CheckedIndices)
            {
                SelectedDirections.Add(index);
            }

            if (SelectedDirections.Count == 0)
            {
                MessageBox.Show("Select at least one direction.", "Pack Frames", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
