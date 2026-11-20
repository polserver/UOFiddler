/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MapPreview.UI
{
    /// <summary>Edits <see cref="PreviewSettings"/>: the area a preview covers and what it draws.</summary>
    public partial class MapPreviewSettingsDialog : Form
    {
        /// <summary>What the dialog opened with, for settings it does not show.</summary>
        private readonly PreviewSettings _original;

        public MapPreviewSettingsDialog(PreviewSettings settings)
        {
            InitializeComponent();

            _original = settings;

            _hintLabel.Text = "Centred on the clicked tile. Larger areas take longer to open.";
            foreach (int size in PreviewSettings.AreaSizes)
            {
                _sizeBox.Items.Add(new AreaSize(size));
            }

            _sizeBox.SelectedIndex = Array.IndexOf(PreviewSettings.AreaSizes,
                PreviewSettings.NearestAreaSize(Math.Max(settings.AreaWidth, settings.AreaHeight)));
            _landBox.Checked = settings.ShowLand;
            _staticsBox.Checked = settings.ShowStatics;
            _stretchBox.Checked = settings.Stretch;
            _lightingBox.Checked = settings.Lighting;
            _huesBox.Checked = settings.Hues;
            _roofsBox.Checked = settings.HideRoofs;
            _autoCutBox.Checked = settings.AutoRoofCut;
        }

        /// <summary>The settings as edited. Not applied; the caller does that on OK.</summary>
        public PreviewSettings Result => new PreviewSettings
        {
            AreaWidth = SelectedSize,
            AreaHeight = SelectedSize,
            ShowLand = _landBox.Checked,
            ShowStatics = _staticsBox.Checked,
            Stretch = _stretchBox.Checked,
            Lighting = _lightingBox.Checked,
            Hues = _huesBox.Checked,
            HideRoofs = _roofsBox.Checked,
            AutoRoofCut = _autoCutBox.Checked,
            MoveStep = _original.MoveStep,
            CutLand = _original.CutLand
        };

        private int SelectedSize => _sizeBox.SelectedItem is AreaSize size ? size.Tiles : PreviewSettings.AreaSizes[0];

        /// <summary>A drop-down entry: "64 x 64".</summary>
        private sealed record AreaSize(int Tiles)
        {
            public override string ToString() => $"{Tiles} x {Tiles}";
        }
    }
}
