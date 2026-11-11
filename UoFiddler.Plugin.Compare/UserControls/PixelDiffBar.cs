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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    /// <summary>
    /// Options for the pixel diff, and its overlay: when on, each preview box fades the pixels its
    /// background image shares with the other box's and outlines the ones that differ. The boxes
    /// must show their image with <see cref="ImageLayout.Center"/>.
    /// </summary>
    public partial class PixelDiffBar : UserControl
    {
        private static readonly Color _outline = Color.Magenta;

        private PictureBox _firstPictureBox;
        private PictureBox _secondPictureBox;
        private PixelDiffResult _diff;
        private bool _diffStale = true;

        public PixelDiffBar()
        {
            InitializeComponent();
            UpdateOptionsEnabled();
        }

        /// <summary>The preview of the original file.</summary>
        [DefaultValue(null)]
        public PictureBox FirstPictureBox
        {
            get => _firstPictureBox;
            set
            {
                Detach(_firstPictureBox);
                _firstPictureBox = Attach(value);
            }
        }

        /// <summary>The preview of the second file.</summary>
        [DefaultValue(null)]
        public PictureBox SecondPictureBox
        {
            get => _secondPictureBox;
            set
            {
                Detach(_secondPictureBox);
                _secondPictureBox = Attach(value);
            }
        }

        private PictureBox Attach(PictureBox box)
        {
            if (box != null)
            {
                box.Paint += OnPreviewPaint;
                box.BackgroundImageChanged += OnPreviewImageChanged;
            }

            _diffStale = true;
            box?.Invalidate();
            return box;
        }

        private void Detach(PictureBox box)
        {
            if (box != null)
            {
                box.Paint -= OnPreviewPaint;
                box.BackgroundImageChanged -= OnPreviewImageChanged;
            }
        }

        private void OnPreviewImageChanged(object sender, EventArgs e) => RepaintPreviews(rebuild: true);

        private void OnEnabledChanged(object sender, EventArgs e)
        {
            UpdateOptionsEnabled();
            RepaintPreviews(rebuild: true);
        }

        private void OnToleranceChanged(object sender, EventArgs e) => RepaintPreviews(rebuild: true);

        private void OnFadeChanged(object sender, EventArgs e) => RepaintPreviews(rebuild: true);

        private void UpdateOptionsEnabled()
        {
            bool on = checkBoxPixelDiff.Checked;
            labelTolerance.Enabled = on;
            numericTolerance.Enabled = on;
            labelFade.Enabled = on;
            numericFade.Enabled = on;
            if (!on)
            {
                labelResult.Text = string.Empty;
            }
        }

        /// <summary>
        /// Marks the diff for rebuilding (it is rebuilt on the next paint, so setting both previews
        /// in a row builds it once) and repaints both previews.
        /// </summary>
        private void RepaintPreviews(bool rebuild)
        {
            if (rebuild)
            {
                _diffStale = true;
            }

            _firstPictureBox?.Invalidate();
            _secondPictureBox?.Invalidate();
        }

        private void EnsureDiff()
        {
            if (!_diffStale)
            {
                return;
            }

            _diffStale = false;
            _diff?.Dispose();
            _diff = null;
            if (!checkBoxPixelDiff.Checked)
            {
                return;
            }

            Bitmap first = _firstPictureBox?.BackgroundImage as Bitmap;
            Bitmap second = _secondPictureBox?.BackgroundImage as Bitmap;
            if (first == null || second == null)
            {
                // With one side missing every pixel would differ, which says nothing.
                labelResult.Text = first == null && second == null ? string.Empty : "Only one side";
                return;
            }

            _diff = PixelDiff.Build(first, second, (int)numericTolerance.Value,
                _firstPictureBox.BackColor, _secondPictureBox.BackColor, (int)numericFade.Value, _outline);
            Point moved = _diff.Offset;
            bool shifted = moved != Point.Empty;
            string result = _diff.Differing == 0 ? (shifted ? "Same pixels" : "No differences")
                : _diff.Differing == _diff.Visible ? "All differ"
                : PixelDiff.IsUnrelated(_diff.Differing, _diff.Visible) ? "Nearly all differ"
                : $"{_diff.Differing:N0} px differ";
            labelResult.Text = shifted ? $"{result}, moved {moved.X},{moved.Y}" : result;

            string tip = $"{_diff.Differing:N0} of {_diff.Visible:N0} visible pixels differ";
            if (shifted)
            {
                tip += $".\nThe art sits at a different spot in its canvas: it was compared with the second image moved {moved.X},{moved.Y} px from the first's top-left corner.";
            }

            toolTip.SetToolTip(labelResult, tip);
        }

        private void OnPreviewPaint(object sender, PaintEventArgs e)
        {
            if (!checkBoxPixelDiff.Checked)
            {
                return;
            }

            EnsureDiff();
            var box = (PictureBox)sender;
            Bitmap overlay = box == _firstPictureBox ? _diff?.FirstOverlay : _diff?.SecondOverlay;
            if (overlay == null)
            {
                return;
            }

            // The overlay is the size of the box's own image, so place it where the image is:
            // ImageLayout.Center centers an image that fits and pins one that doesn't to the
            // top-left corner.
            int x = Math.Max(0, (box.ClientSize.Width - overlay.Width) / 2);
            int y = Math.Max(0, (box.ClientSize.Height - overlay.Height) / 2);
            e.Graphics.DrawImageUnscaled(overlay, x, y);
        }

        private void DisposeDiff()
        {
            Detach(_firstPictureBox);
            Detach(_secondPictureBox);
            _firstPictureBox = null;
            _secondPictureBox = null;
            _diff?.Dispose();
            _diff = null;
        }
    }
}
