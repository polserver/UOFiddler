/***************************************************************************
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
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The settings dialog: colours with their opacity, and how new documents start.
    /// </summary>
    internal sealed partial class EditorSettingsDialog : Form
    {
        private readonly EditorSettings _settings;
        private readonly List<Action> _refresh = new List<Action>();

        public EditorSettingsDialog(EditorSettings current)
        {
            _settings = current.Clone();

            InitializeComponent();

            headingColours.Font = new Font(Font, FontStyle.Bold);
            headingStart.Font = new Font(Font, FontStyle.Bold);

            ColorRow(swatchBackground, null, () => _settings.EffectiveBackground);
            ColorRow(swatchWorkPlane, alphaWorkPlane, () => _settings.WorkPlane);
            ColorRow(swatchFloorPlane, alphaFloorPlane, () => _settings.FloorPlane);
            ColorRow(swatchGrid, alphaGrid, () => _settings.Grid);
            ColorRow(swatchOutline, alphaOutline, () => _settings.Outline);
            ColorRow(swatchSelection, null, () => _settings.SelectionBox);

            CheckRow(checkWorkPlane, () => _settings.StartWithWorkPlane);
            CheckRow(checkFloorPlane, () => _settings.StartWithFloorPlane);
            CheckRow(checkGrid, () => _settings.StartWithGrid);
            CheckRow(checkHideFloorsAbove, () => _settings.HideFloorsAbove);
        }

        /// <summary>The edited copy; apply it when the dialog returns OK.</summary>
        public EditorSettings Result => _settings;

        /// <summary>Shows the current value in a row's swatch and opacity box, now and again after a reset.</summary>
        private void ColorRow(Button swatch, NumericUpDown alpha, Func<Color> get)
        {
            void Refresh()
            {
                Color c = get();
                swatch.BackColor = Color.FromArgb(c.R, c.G, c.B);
                if (alpha != null)
                {
                    alpha.Value = Math.Clamp((int)Math.Round(c.A / 2.55), 5, 100);
                }
            }

            Refresh();
            _refresh.Add(Refresh);
        }

        private void CheckRow(CheckBox box, Func<bool> get)
        {
            box.Checked = get();
            _refresh.Add(() => box.Checked = get());
        }

        private void PickColor(Button swatch, Func<Color> get, Action<Color> set)
        {
            Color c = get();
            using var dialog = new ColorDialog { Color = Color.FromArgb(c.R, c.G, c.B), FullOpen = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                set(Color.FromArgb(c.A, dialog.Color.R, dialog.Color.G, dialog.Color.B));
                swatch.BackColor = Color.FromArgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
            }
        }

        private static void ChangeOpacity(NumericUpDown alpha, Func<Color> get, Action<Color> set)
        {
            Color c = get();
            set(Color.FromArgb((int)Math.Round((double)alpha.Value * 2.55), c.R, c.G, c.B));
        }

        private void SwatchBackground_Click(object sender, EventArgs e)
        {
            PickColor(swatchBackground, () => _settings.EffectiveBackground, c => _settings.Background = c);
        }

        private void SwatchWorkPlane_Click(object sender, EventArgs e)
        {
            PickColor(swatchWorkPlane, () => _settings.WorkPlane, c => _settings.WorkPlane = c);
        }

        private void SwatchFloorPlane_Click(object sender, EventArgs e)
        {
            PickColor(swatchFloorPlane, () => _settings.FloorPlane, c => _settings.FloorPlane = c);
        }

        private void SwatchGrid_Click(object sender, EventArgs e)
        {
            PickColor(swatchGrid, () => _settings.Grid, c => _settings.Grid = c);
        }

        private void SwatchOutline_Click(object sender, EventArgs e)
        {
            PickColor(swatchOutline, () => _settings.Outline, c => _settings.Outline = c);
        }

        private void SwatchSelection_Click(object sender, EventArgs e)
        {
            PickColor(swatchSelection, () => _settings.SelectionBox, c => _settings.SelectionBox = c);
        }

        private void AlphaWorkPlane_ValueChanged(object sender, EventArgs e)
        {
            ChangeOpacity(alphaWorkPlane, () => _settings.WorkPlane, c => _settings.WorkPlane = c);
        }

        private void AlphaFloorPlane_ValueChanged(object sender, EventArgs e)
        {
            ChangeOpacity(alphaFloorPlane, () => _settings.FloorPlane, c => _settings.FloorPlane = c);
        }

        private void AlphaGrid_ValueChanged(object sender, EventArgs e)
        {
            ChangeOpacity(alphaGrid, () => _settings.Grid, c => _settings.Grid = c);
        }

        private void AlphaOutline_ValueChanged(object sender, EventArgs e)
        {
            ChangeOpacity(alphaOutline, () => _settings.Outline, c => _settings.Outline = c);
        }

        private void CheckWorkPlane_CheckedChanged(object sender, EventArgs e)
        {
            _settings.StartWithWorkPlane = checkWorkPlane.Checked;
        }

        private void CheckFloorPlane_CheckedChanged(object sender, EventArgs e)
        {
            _settings.StartWithFloorPlane = checkFloorPlane.Checked;
        }

        private void CheckGrid_CheckedChanged(object sender, EventArgs e)
        {
            _settings.StartWithGrid = checkGrid.Checked;
        }

        private void CheckHideFloorsAbove_CheckedChanged(object sender, EventArgs e)
        {
            _settings.HideFloorsAbove = checkHideFloorsAbove.Checked;
        }

        private void Defaults_Click(object sender, EventArgs e)
        {
            var defaults = new EditorSettings
            {
                // The layout is not part of this dialog; a reset leaves it alone.
                PreviewTerrain = _settings.PreviewTerrain,
                ShowTileBrowser = _settings.ShowTileBrowser,
                ShowInspector = _settings.ShowInspector,
                TileBrowserWidth = _settings.TileBrowserWidth,
                InspectorWidth = _settings.InspectorWidth,
                HousePiecesHeight = _settings.HousePiecesHeight,
                DesignLibraryFolder = _settings.DesignLibraryFolder
            };
            foreach (System.Reflection.PropertyInfo property in typeof(EditorSettings).GetProperties())
            {
                if (property.CanWrite && property.DeclaringType == typeof(EditorSettings))
                {
                    property.SetValue(_settings, property.GetValue(defaults));
                }
            }

            foreach (Action refresh in _refresh)
            {
                refresh();
            }
        }
    }
}
