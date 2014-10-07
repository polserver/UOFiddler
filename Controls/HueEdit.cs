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

namespace FiddlerControls
{
    public partial class HueEdit : Form
    {
        private Hues refmarker;
        private Hue hue;
        private short[] Colors;
        private int selected;
        private int second_sel;
        private Bitmap preview;

        public int Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                Color c = Ultima.Hues.HueToColor(Colors[value]);
                numericUpDownR.Value = c.R;
                numericUpDownG.Value = c.G;
                numericUpDownB.Value = c.B;
                pictureBox.Invalidate();
                pictureBoxIndex.Invalidate();
            }
        }

        private int Second_Selected
        {
            get { return second_sel; }
            set
            {
                second_sel = value;
                pictureBox.Invalidate();
            }
        }

        private void RefreshPreview()
        {
            if (preview != null)
            {
                using (Bitmap bmp = new Bitmap(preview))
                {
                    Ultima.Hues.ApplyTo(bmp, Colors, hueOnlyGreyToolStripMenuItem.Checked);
                    using (Graphics g = Graphics.FromImage(pictureBoxPreview.Image))
                    {
                        g.Clear(Color.White);
                        int x = (int)(pictureBoxPreview.Image.Width / 2 - bmp.Width / 2);
                        int y = (int)(pictureBoxPreview.Image.Height / 2 - bmp.Height / 2);
                        g.DrawImage(bmp, x, y);
                    }
                }
                pictureBoxPreview.Invalidate();
            }
        }

        public HueEdit(int index, Hues m_refmarker)
        {
            InitializeComponent();
            this.Icon = FiddlerControls.Options.GetFiddlerIcon();
            refmarker = m_refmarker;
            hue = Ultima.Hues.GetHue(index);
            Colors = new short[32];
            hue.Colors.CopyTo(Colors, 0);
            textBoxName.Text = hue.Name;
            this.Text = String.Format("HueEdit {0}", index);
            Selected = 0;
            Second_Selected = -1;
            pictureBoxPreview.Image = new Bitmap(pictureBoxPreview.Width, pictureBoxPreview.Height);
        }

        private int GetIndex(int x)
        {
            return (x / (pictureBox.Width / Colors.Length));
        }

        private void OnPaintPicture(object sender, PaintEventArgs e)
        {
            float size = pictureBox.Width / Colors.Length;
            for (int i = 0; i < Colors.Length; ++i)
            {
                Rectangle rectangle = new Rectangle(((int)Math.Round((double)(i * size))), 5, (int)Math.Round((double)(size + 1f)), pictureBox.Height - 10);
                e.Graphics.FillRectangle(new SolidBrush(Ultima.Hues.HueToColor(Colors[i])), rectangle);
                if (rectangle.X > 0)
                    e.Graphics.DrawLine(new Pen(Color.Black), rectangle.X, 5, rectangle.X, pictureBox.Height - 7);
            }
            e.Graphics.FillRectangle(Brushes.LightBlue, (int)Math.Round((double)(selected * size)), pictureBox.Height - 4, (int)Math.Round((double)(size + 1f)), 4);
            if (Second_Selected > -1)
                e.Graphics.FillRectangle(Brushes.LightBlue, (int)Math.Round((double)(second_sel * size)), 0, (int)Math.Round((double)(size + 1f)), 4);

        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            Point m = pictureBox.PointToClient(Control.MousePosition);
            int index = GetIndex(m.X);
            if ((index >= 0) && (index < Colors.Length))
            {
                if (e.Button == MouseButtons.Left)
                    Selected = index;
                else
                    Second_Selected = index;
            }
        }

        private void onPaintIndexColor(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Ultima.Hues.HueToColor(Colors[selected]));
        }

        private void OnClickColorPicker(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() != DialogResult.Cancel)
            {
                Colors[selected] = Ultima.Hues.ColorToHue(colorDialog.Color);
                Selected = selected;
                RefreshPreview();
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            Colors.CopyTo(hue.Colors, 0);
            hue.Name = textBoxName.Text;
            hue.TableStart = Colors[0];
            hue.TableEnd = Colors[hue.Colors.Length - 1];
            FiddlerControls.Events.FireHueChangeEvent();
            Options.ChangedUltimaClass["Hues"] = true;
        }

        private void OnClickSpread(object sender, EventArgs e)
        {
            if (Second_Selected > -1)
            {
                int start = Math.Min(Second_Selected, Selected);
                int end = Math.Max(Second_Selected, Selected);
                int diff = end - start;
                if (diff > 1)
                {
                    Color startc = Ultima.Hues.HueToColor(Colors[start]);
                    Color endc = Ultima.Hues.HueToColor(Colors[end]);

                    float Rdiv = (endc.R - startc.R) / (diff - 1);
                    float Gdiv = (endc.G - startc.G) / (diff - 1);
                    float Bdiv = (endc.B - startc.B) / (diff - 1);
                    for (int i = 1; i < diff; ++i)
                    {
                        Color newc = Color.FromArgb(
                            (int)(startc.R + i * Rdiv),
                            (int)(startc.G + i * Gdiv),
                            (int)(startc.B + i * Bdiv));
                        Colors[start + i] = Ultima.Hues.ColorToHue(newc);
                    }
                    pictureBox.Invalidate();
                    RefreshPreview();
                }
            }
        }

        private void onClickEpxGradient(object sender, EventArgs e)
        {
            if (Second_Selected > -1)
            {
                int start = Math.Min(Second_Selected, Selected);
                int end = Math.Max(Second_Selected, Selected);
                int diff = end - start;
                if (diff > 1)
                {
                    Color startc = Ultima.Hues.HueToColor(Colors[start]);
                    Color endc = Ultima.Hues.HueToColor(Colors[end]);
                    double Rdiv = Math.Log(Math.Abs(endc.R - startc.R), Math.E) / Math.Log(diff, Math.E);
                    double Gdiv = Math.Log(Math.Abs(endc.G - startc.G), Math.E) / Math.Log(diff, Math.E);
                    double Bdiv = Math.Log(Math.Abs(endc.B - startc.B), Math.E) / Math.Log(diff, Math.E);
                    int Rfac = 1;
                    if (endc.R - startc.R < 0)
                        Rfac = -1;
                    int Gfac = 1;
                    if (endc.G - startc.G < 0)
                        Gfac = -1;
                    int Bfac = 1;
                    if (endc.B - startc.B < 0)
                        Bfac = -1;
                    for (int i = 1; i < diff; ++i)
                    {
                        Color newc = Color.FromArgb(
                            (int)(startc.R + Rfac * Math.Pow(i, Rdiv)),
                            (int)(startc.G + Gfac * Math.Pow(i, Gdiv)),
                            (int)(startc.B + Bfac * Math.Pow(i, Bdiv)));
                        Colors[start + i] = Ultima.Hues.ColorToHue(newc);
                    }
                    pictureBox.Invalidate();
                    RefreshPreview();
                }
            }
        }

        private void onChangeRGB(object sender, EventArgs e)
        {
            Color c = Color.FromArgb((int)numericUpDownR.Value, (int)numericUpDownG.Value, (int)numericUpDownB.Value);
            Colors[selected] = Ultima.Hues.ColorToHue(c);
            pictureBox.Invalidate();
            pictureBoxIndex.Invalidate();
            RefreshPreview();
        }

        private void OnClickInverse(object sender, EventArgs e)
        {
            if (Second_Selected > -1)
            {
                int start = Math.Min(Second_Selected, Selected);
                int end = Math.Max(Second_Selected, Selected);
                while (start < end)
                {
                    short temp = Colors[start];
                    Colors[start] = Colors[end];
                    Colors[end] = temp;
                    ++start;
                    --end;
                }
                Selected = selected;
                RefreshPreview();
            }
        }

        private void OnClickModifyRange(object sender, EventArgs e)
        {
            if (Second_Selected > -1)
            {
                int start = Math.Min(Second_Selected, Selected);
                int end = Math.Max(Second_Selected, Selected);
                for (int i = start; i <= end; ++i)
                {
                    Color c = Ultima.Hues.HueToColor(Colors[i]);
                    int r = (int)(c.R + numericUpDownR_R.Value);
                    int g = (int)(c.G + numericUpDownG_R.Value);
                    int b = (int)(c.B + numericUpDownB_R.Value);
                    Color newc = Color.FromArgb(
                        Math.Max(0, Math.Min(255, r)),
                        Math.Max(0, Math.Min(255, g)),
                        Math.Max(0, Math.Min(255, b)));
                    Colors[i] = Ultima.Hues.ColorToHue(newc);
                }
                Selected = selected;
                RefreshPreview();
            }
        }

        #region PreviewStuff
        private void onClickHueOnlyGrey(object sender, EventArgs e)
        {
            RefreshPreview();
        }


        private void onTextChangedArt(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(TextBoxArt.Text, out index, 0, 0x3FFF))
            {
                if (Art.IsValidStatic(index))
                    TextBoxArt.ForeColor = Color.Black;
                else
                    TextBoxArt.ForeColor = Color.Red;
            }
            else
                TextBoxArt.ForeColor = Color.Red;
        }

        private void onKeyDownArt(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(TextBoxArt.Text, out index, 0, 0x3FFF))
                {
                    if (!Art.IsValidStatic(index))
                        return;
                    contextMenuStrip1.Close();
                    preview = Art.GetStatic(index);
                    RefreshPreview();
                }
            }
        }

        private void onTextChangedAnim(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(TextBoxAnim.Text, out index, 1, 10000))
            {
                if (Animations.IsActionDefined(index, 0, 0))
                    TextBoxAnim.ForeColor = Color.Black;
                else
                    TextBoxAnim.ForeColor = Color.Red;
            }
            else
                TextBoxAnim.ForeColor = Color.Red;
        }

        private void onKeyDownAnim(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(TextBoxAnim.Text, out index, 1, 10000))
                {
                    if (!Animations.IsActionDefined(index, 0, 0))
                        return;
                    contextMenuStrip1.Close();
                    int hueref = 0;
                    Frame[] frames = Animations.GetAnimation(index, 0, 1, ref hueref, false, true);
                    if (frames == null)
                        return;
                    preview = frames[0].Bitmap;
                    RefreshPreview();
                }
            }
        }

        private void onTextChangedGump(object sender, EventArgs e)
        {
            int index;
            if (Utils.ConvertStringToInt(TextBoxGump.Text, out index, 0, 0xFFFE))
            {
                if (Gumps.IsValidIndex(index))
                    TextBoxGump.ForeColor = Color.Black;
                else
                    TextBoxGump.ForeColor = Color.Red;
            }
            else
                TextBoxGump.ForeColor = Color.Red;
        }

        private void onKeyDownGump(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int index;
                if (Utils.ConvertStringToInt(TextBoxGump.Text, out index, 0, 0xFFFE))
                {
                    if (!Gumps.IsValidIndex(index))
                        return;
                    contextMenuStrip1.Close();
                    preview = Gumps.GetGump(index);
                    RefreshPreview();
                }
            }
        }
        #endregion
    }
}
