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
using UoFiddler.Controls.Classes;
using Hues = UoFiddler.Controls.UserControls.Hues;

namespace UoFiddler.Controls.Forms
{
    public partial class HueEdit : Form
    {
        private readonly Hues _refMarker;
        private readonly Hue _hue;
        private readonly short[] _colors;
        private int _selected;
        private int _secondSel;
        private Bitmap _preview;

        public int Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Color c = Ultima.Hues.HueToColor(_colors[value]);
                numericUpDownR.Value = c.R;
                numericUpDownG.Value = c.G;
                numericUpDownB.Value = c.B;
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

        private void RefreshPreview()
        {
            if (_preview == null)
            {
                return;
            }

            using (Bitmap bmp = new Bitmap(_preview))
            {
                Ultima.Hues.ApplyTo(bmp, _colors, hueOnlyGreyToolStripMenuItem.Checked);
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

        public HueEdit(int index, Hues mRefMarker)
        {
            InitializeComponent();
            Icon = Options.GetFiddlerIcon();
            _refMarker = mRefMarker;
            _hue = Ultima.Hues.GetHue(index);
            _colors = new short[32];
            _hue.Colors.CopyTo(_colors, 0);
            textBoxName.Text = _hue.Name;
            Text = $"HueEdit {index}";
            Selected = 0;
            SecondSelected = -1;
            pictureBoxPreview.Image = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
        }

        private int GetIndex(int x)
        {
            return x / (pictureBox.Width / _colors.Length);
        }

        private void OnPaintPicture(object sender, PaintEventArgs e)
        {
            float size = pictureBox.Width / _colors.Length;
            for (int i = 0; i < _colors.Length; ++i)
            {
                Rectangle rectangle = new Rectangle((int)Math.Round(i * size), 5, (int)Math.Round(size + 1f), pictureBox.Height - 10);
                e.Graphics.FillRectangle(new SolidBrush(Ultima.Hues.HueToColor(_colors[i])), rectangle);
                if (rectangle.X > 0)
                {
                    e.Graphics.DrawLine(new Pen(Color.Black), rectangle.X, 5, rectangle.X, pictureBox.Height - 7);
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
            e.Graphics.Clear(Ultima.Hues.HueToColor(_colors[_selected]));
        }

        private void OnClickColorPicker(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            _colors[_selected] = Ultima.Hues.ColorToHue(colorDialog.Color);
            Selected = _selected;
            RefreshPreview();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            _colors.CopyTo(_hue.Colors, 0);
            _hue.Name = textBoxName.Text;
            _hue.TableStart = _colors[0];
            _hue.TableEnd = _colors[_hue.Colors.Length - 1];
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

            Color startC = Ultima.Hues.HueToColor(_colors[start]);
            Color endC = Ultima.Hues.HueToColor(_colors[end]);

            float rDiv = (endC.R - startC.R) / (diff - 1);
            float gDiv = (endC.G - startC.G) / (diff - 1);
            float bDiv = (endC.B - startC.B) / (diff - 1);

            for (int i = 1; i < diff; ++i)
            {
                Color newC = Color.FromArgb((int)(startC.R + (i * rDiv)), (int)(startC.G + (i * gDiv)),
                    (int)(startC.B + (i * bDiv)));

                _colors[start + i] = Ultima.Hues.ColorToHue(newC);
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

            Color startc = Ultima.Hues.HueToColor(_colors[start]);
            Color endc = Ultima.Hues.HueToColor(_colors[end]);
            double rdiv = Math.Log(Math.Abs(endc.R - startc.R), Math.E) / Math.Log(diff, Math.E);
            double gdiv = Math.Log(Math.Abs(endc.G - startc.G), Math.E) / Math.Log(diff, Math.E);
            double bdiv = Math.Log(Math.Abs(endc.B - startc.B), Math.E) / Math.Log(diff, Math.E);
            int rfac = 1;
            if (endc.R - startc.R < 0)
            {
                rfac = -1;
            }

            int gfac = 1;
            if (endc.G - startc.G < 0)
            {
                gfac = -1;
            }

            int bfac = 1;
            if (endc.B - startc.B < 0)
            {
                bfac = -1;
            }

            for (int i = 1; i < diff; ++i)
            {
                Color newc = Color.FromArgb(
                    (int)(startc.R + (rfac * Math.Pow(i, rdiv))),
                    (int)(startc.G + (gfac * Math.Pow(i, gdiv))),
                    (int)(startc.B + (bfac * Math.Pow(i, bdiv))));
                _colors[start + i] = Ultima.Hues.ColorToHue(newc);
            }
            pictureBox.Invalidate();
            RefreshPreview();
        }

        private void OnChangeRGB(object sender, EventArgs e)
        {
            Color c = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            _colors[_selected] = Ultima.Hues.ColorToHue(c);
            pictureBox.Invalidate();
            pictureBoxIndex.Invalidate();
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
                short temp = _colors[start];
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
                Color c = Ultima.Hues.HueToColor(_colors[i]);
                int r = (int)(c.R + numericUpDownR_R.Value);
                int g = (int)(c.G + numericUpDownG_R.Value);
                int b = (int)(c.B + numericUpDownB_R.Value);
                Color newc = Color.FromArgb(
                    Math.Max(0, Math.Min(255, r)),
                    Math.Max(0, Math.Min(255, g)),
                    Math.Max(0, Math.Min(255, b)));
                _colors[i] = Ultima.Hues.ColorToHue(newc);
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
            if (Utils.ConvertStringToInt(TextBoxArt.Text, out int index, 0, 0x3FFF))
            {
                TextBoxArt.ForeColor = Art.IsValidStatic(index) ? Color.Black : Color.Red;
            }
            else
            {
                TextBoxArt.ForeColor = Color.Red;
            }
        }

        private void OnKeyDownArt(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(TextBoxArt.Text, out int index, 0, 0x3FFF))
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
            Frame[] frames = Animations.GetAnimation(index, 0, 1, ref hueRef, false, true);
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
    }
}
