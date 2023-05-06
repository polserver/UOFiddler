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
using Ultima;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Forms
{
    public partial class HueEditForm : Form
    {
        private readonly Hue _hue;
        private readonly ushort[] _colors;
        private int _selected;
        private int _secondSel;
        private Bitmap _preview;

        private int Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Color color = HueHelpers.HueToColor(_colors[value]);
                numericUpDownR.Value = color.R;
                numericUpDownG.Value = color.G;
                numericUpDownB.Value = color.B;
                pictureBox.Invalidate();
                pictureBoxIndex.Invalidate();
            }
        }

        private int SecondSelected
        {
            get => _secondSel;
            set
            {
                _secondSel = value;
                pictureBox.Invalidate();
            }
        }

        public HueEditForm(int index)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();

            Text = $"HueEdit {index} / 0x{index:X}";

            _hue = Hues.GetHue(index);
            _colors = new ushort[32];
            _hue.Colors.CopyTo(_colors, 0);
            textBoxName.Text = _hue.Name;

            Selected = 0;
            SecondSelected = -1;

            pictureBoxPreview.Image = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
        }

        private int GetIndex(int x)
        {
            return (int)(x * 1.0 / (pictureBox.Width * 1.0 / _colors.Length * 1.0));
        }

        private void OnPaintPicture(object sender, PaintEventArgs e)
        {
            double size = pictureBox.Width * 1.0 / _colors.Length;
            for (int i = 0; i < _colors.Length; ++i)
            {
                Rectangle rectangle = new Rectangle((int)Math.Round(i * size), 5, (int)Math.Round(size + 1f), pictureBox.Height - 10);
                using (var solidBrush = new SolidBrush(HueHelpers.HueToColor(_colors[i])))
                {
                    e.Graphics.FillRectangle(solidBrush, rectangle);
                }

                if (rectangle.X > 0)
                {
                    e.Graphics.DrawLine(Pens.Black, rectangle.X, 5, rectangle.X, pictureBox.Height - 7);
                }
            }

            e.Graphics.FillRectangle(Brushes.LightBlue, (int)Math.Round(_selected * size), pictureBox.Height - 4, (int)Math.Round(size + 1f), 4);

            if (SecondSelected > -1)
            {
                e.Graphics.FillRectangle(Brushes.LightBlue, (int)Math.Round(_secondSel * size), 0, (int)Math.Round(size + 1f), 4);
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            Point m = pictureBox.PointToClient(MousePosition);
            int index = GetIndex(m.X);
            if (index < 0 || index >= _colors.Length)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                Selected = index;
            }
            else
            {
                SecondSelected = index;
            }
        }

        private void OnPaintIndexColor(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(HueHelpers.HueToColor(_colors[_selected]));
        }

        private void OnClickColorPicker(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            _colors[_selected] = HueHelpers.ColorToHue(colorDialog.Color);
            Selected = _selected;
            RefreshPreview();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            _colors.CopyTo(_hue.Colors, 0);
            _hue.Name = textBoxName.Text;
            _hue.TableStart = (ushort)_colors[0];
            _hue.TableEnd = (ushort)(_colors[_hue.Colors.Length - 1] + 1057);
            ControlEvents.FireHueChangeEvent();
            Options.ChangedUltimaClass["Hues"] = true;
        }

        private void OnClickSpread(object sender, EventArgs e)
        {
            if (SecondSelected <= -1)
            {
                return;
            }

            int start = Math.Min(SecondSelected, Selected);
            int end = Math.Max(SecondSelected, Selected);
            int diff = end - start;

            if (diff <= 1)
            {
                return;
            }

            Color startC = HueHelpers.HueToColor(_colors[start]);
            Color endC = HueHelpers.HueToColor(_colors[end]);

            float rDiv = (float)(endC.R - startC.R) / (diff - 1);
            float gDiv = (float)(endC.G - startC.G) / (diff - 1);
            float bDiv = (float)(endC.B - startC.B) / (diff - 1);

            for (int i = 1; i < diff; ++i)
            {
                Color newC = Color.FromArgb((int)(startC.R + (i * rDiv)), (int)(startC.G + (i * gDiv)),
                    (int)(startC.B + (i * bDiv)));

                _colors[start + i] = HueHelpers.ColorToHue(newC);
            }

            pictureBox.Invalidate();

            RefreshPreview();
        }

        private void OnClickEpxGradient(object sender, EventArgs e)
        {
            if (SecondSelected <= -1)
            {
                return;
            }

            int start = Math.Min(SecondSelected, Selected);
            int end = Math.Max(SecondSelected, Selected);

            int diff = end - start;
            if (diff <= 1)
            {
                return;
            }

            Color startColor = HueHelpers.HueToColor(_colors[start]);
            Color endColor = HueHelpers.HueToColor(_colors[end]);

            double redDiv = Math.Log(Math.Abs(endColor.R - startColor.R), Math.E) / Math.Log(diff, Math.E);
            double greenDiv = Math.Log(Math.Abs(endColor.G - startColor.G), Math.E) / Math.Log(diff, Math.E);
            double blueDiv = Math.Log(Math.Abs(endColor.B - startColor.B), Math.E) / Math.Log(diff, Math.E);

            int redFac = 1;
            if (endColor.R - startColor.R < 0)
            {
                redFac = -1;
            }

            int greenFac = 1;
            if (endColor.G - startColor.G < 0)
            {
                greenFac = -1;
            }

            int blueFac = 1;
            if (endColor.B - startColor.B < 0)
            {
                blueFac = -1;
            }

            for (int i = 1; i < diff; ++i)
            {
                Color newColor = Color.FromArgb(
                    (int)(startColor.R + (redFac * Math.Pow(i, redDiv))),
                    (int)(startColor.G + (greenFac * Math.Pow(i, greenDiv))),
                    (int)(startColor.B + (blueFac * Math.Pow(i, blueDiv))));

                _colors[start + i] = HueHelpers.ColorToHue(newColor);
            }

            pictureBox.Invalidate();

            RefreshPreview();
        }

        private void OnClickInverse(object sender, EventArgs e)
        {
            if (SecondSelected <= -1)
            {
                return;
            }

            int start = Math.Min(SecondSelected, Selected);
            int end = Math.Max(SecondSelected, Selected);

            while (start < end)
            {
                var temp = _colors[start];
                _colors[start] = _colors[end];
                _colors[end] = temp;
                ++start;
                --end;
            }

            Selected = _selected;

            RefreshPreview();
        }

        private void OnClickModifyRange(object sender, EventArgs e)
        {
            if (SecondSelected <= -1)
            {
                return;
            }

            int start = Math.Min(SecondSelected, Selected);
            int end = Math.Max(SecondSelected, Selected);

            for (int i = start; i <= end; ++i)
            {
                Color color = HueHelpers.HueToColor(_colors[i]);
                int r = (int)(color.R + numericUpDownR_R.Value);
                int g = (int)(color.G + numericUpDownG_R.Value);
                int b = (int)(color.B + numericUpDownB_R.Value);
                Color newColor = Color.FromArgb(
                    Math.Max(0, Math.Min(255, r)),
                    Math.Max(0, Math.Min(255, g)),
                    Math.Max(0, Math.Min(255, b)));
                _colors[i] = HueHelpers.ColorToHue(newColor);
            }

            Selected = _selected;

            RefreshPreview();
        }

        private void OnClickHueOnlyGrey(object sender, EventArgs e)
        {
            RefreshPreview();
        }

        private void OnTextChangedArt(object sender, EventArgs e)
        {
            TextBoxArt.ForeColor = Utils.ConvertStringToInt(TextBoxArt.Text, out int index, 0, Art.GetMaxItemId())
                ? Art.IsValidStatic(index) ? Color.Black : Color.Red
                : Color.Red;
        }

        private void OnKeyDownArt(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(TextBoxArt.Text, out int index, 0, Art.GetMaxItemId()))
            {
                return;
            }

            if (!Art.IsValidStatic(index))
            {
                return;
            }

            contextMenuStrip1.Close();

            _preview = Art.GetStatic(index);

            RefreshPreview();
        }

        private void OnTextChangedAnim(object sender, EventArgs e)
        {
            TextBoxAnim.ForeColor = Utils.ConvertStringToInt(TextBoxAnim.Text, out int index, 1, 10000)
                ? Animations.IsActionDefined(index, 0, 0) ? Color.Black : Color.Red
                : Color.Red;
        }

        private void OnKeyDownAnim(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(TextBoxAnim.Text, out int index, 1, 10000))
            {
                return;
            }

            if (!Animations.IsActionDefined(index, 0, 0))
            {
                return;
            }

            contextMenuStrip1.Close();

            int hueRef = 0;

            AnimationFrame[] frames = Animations.GetAnimation(index, 0, 1, ref hueRef, false, true);

            if (frames == null)
            {
                return;
            }

            _preview = frames[0].Bitmap;

            RefreshPreview();
        }

        private void OnTextChangedGump(object sender, EventArgs e)
        {
            if (Utils.ConvertStringToInt(TextBoxGump.Text, out int index, 0, 0xFFFE))
            {
                TextBoxGump.ForeColor = Gumps.IsValidIndex(index) ? Color.Black : Color.Red;
            }
            else
            {
                TextBoxGump.ForeColor = Color.Red;
            }
        }

        private void OnKeyDownGump(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(TextBoxGump.Text, out int index, 0, 0xFFFE))
            {
                return;
            }

            if (!Gumps.IsValidIndex(index))
            {
                return;
            }

            contextMenuStrip1.Close();

            _preview = Gumps.GetGump(index);

            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (_preview == null)
            {
                return;
            }

            using (Bitmap bmp = new Bitmap(_preview))
            {
                Hues.ApplyTo(bmp, _colors, hueOnlyGreyToolStripMenuItem.Checked);
                using (Graphics g = Graphics.FromImage(pictureBoxPreview.Image))
                {
                    g.Clear(Color.White);
                    int x = (pictureBoxPreview.Image.Width / 2) - (bmp.Width / 2);
                    int y = (pictureBoxPreview.Image.Height / 2) - (bmp.Height / 2);
                    g.DrawImage(bmp, x, y);
                }
            }

            pictureBoxPreview.Invalidate();
        }

        private void SetColorButton_Click(object sender, EventArgs e)
        {
            _colors[_selected] = HueHelpers.ColorToHue(Color.FromArgb((int)numericUpDownR.Value,
                                                                (int)numericUpDownG.Value,
                                                                (int)numericUpDownB.Value));

            pictureBox.Invalidate();
            pictureBoxIndex.Invalidate();

            RefreshPreview();
        }
    }
}
