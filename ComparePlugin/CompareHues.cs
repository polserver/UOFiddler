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
using System.Collections;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ultima;

namespace ComparePlugin
{
    public partial class CompareHues : UserControl
    {
        public CompareHues()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            pictureBox1.Image = bmp1;
            pictureBox2.Image = bmp2;
            pictureBox1.MouseWheel += new MouseEventHandler(OnMouseWheel);
            pictureBox2.MouseWheel += new MouseEventHandler(OnMouseWheel);
            hue2loaded = false;
        }

        private const int ITEMHEIGHT = 20;
        private Bitmap bmp1;
        private Bitmap bmp2;
        private int selected;
        private int row;
        private bool hue2loaded;
        Hashtable m_Compare = new Hashtable();
        private bool Loaded;

        private void OnLoad(object sender, EventArgs e)
        {
            vScrollBar.Maximum = Ultima.Hues.List.Length;
            vScrollBar.Minimum = 0;
            vScrollBar.Value = 0;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = 10;
            selected = 0;
            bmp1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            bmp2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            Loaded = true;
            row = pictureBox1.Height / ITEMHEIGHT;
            PaintBox1();
        }

        private int GetIndex(int y)
        {
            int value = vScrollBar.Value + y;
            if (Ultima.Hues.List.Length > value)
                return value;
            else
                return -1;
        }

        private void PaintBox1()
        {
            using (Graphics g = Graphics.FromImage(bmp1))
            {
                g.Clear(Color.White);

                for (int y = 0; y <= row; y++)
                {
                    int index = GetIndex(y);
                    if (index >= 0)
                    {
                        Rectangle rect = new Rectangle(0, y * ITEMHEIGHT, 200, ITEMHEIGHT);
                        if (index == selected)
                            g.FillRectangle(SystemBrushes.Highlight, rect);
                        else if (!Compare(index))
                            g.FillRectangle(Brushes.Red, rect);
                        else
                            g.FillRectangle(SystemBrushes.Window, rect);

                        float size = ((float)(pictureBox1.Width - 200)) / 32;
                        Hue hue = Ultima.Hues.List[index];
                        Rectangle stringrect = new Rectangle(3, y * ITEMHEIGHT, pictureBox1.Width, ITEMHEIGHT);
                        g.DrawString(String.Format("{0,-5} {1,-7} {2}", hue.Index + 1, String.Format("(0x{0:X})", hue.Index + 1), hue.Name), Font, Brushes.Black, stringrect);

                        for (int i = 0; i < hue.Colors.Length; i++)
                        {
                            Rectangle rectangle = new Rectangle(200 + ((int)Math.Round((double)(i * size))), y * ITEMHEIGHT, (int)Math.Round((double)(size + 1f)), ITEMHEIGHT);
                            g.FillRectangle(new SolidBrush(hue.GetColor(i)), rectangle);
                        }
                    }
                }
            }
            pictureBox1.Image = bmp1;
            pictureBox1.Update();
        }

        private void PaintBox2()
        {
            using (Graphics g = Graphics.FromImage(bmp2))
            {
                g.Clear(Color.White);

                for (int y = 0; y <= row; y++)
                {
                    int index = GetIndex(y);
                    if (index >= 0)
                    {
                        Rectangle rect = new Rectangle(0, y * ITEMHEIGHT, 200, ITEMHEIGHT);
                        if (index == selected)
                            g.FillRectangle(SystemBrushes.Highlight, rect);
                        else if (!Compare(index))
                            g.FillRectangle(Brushes.Red, rect);
                        else
                            g.FillRectangle(SystemBrushes.Window, rect);

                        float size = ((float)(pictureBox2.Width - 200)) / 32;
                        Hue hue = SecondHue.List[index];
                        Rectangle stringrect = new Rectangle(3, y * ITEMHEIGHT, pictureBox2.Width, ITEMHEIGHT);
                        g.DrawString(String.Format("{0,-5} {1,-7} {2}", hue.Index + 1, String.Format("(0x{0:X})", hue.Index + 1), hue.Name), Font, Brushes.Black, stringrect);

                        for (int i = 0; i < hue.Colors.Length; i++)
                        {
                            Rectangle rectangle = new Rectangle(200 + ((int)Math.Round((double)(i * size))), y * ITEMHEIGHT, (int)Math.Round((double)(size + 1f)), ITEMHEIGHT);
                            g.FillRectangle(new SolidBrush(hue.GetColor(i)), rectangle);
                        }
                    }
                }
            }
            pictureBox2.Image = bmp2;
            pictureBox2.Update();
        }

        private void OnResizeHue(object sender, EventArgs e)
        {
            if (!Loaded)
                return;
            row = pictureBox1.Height / ITEMHEIGHT;
            bmp1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            bmp2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            PaintBox1();
            if (hue2loaded)
                PaintBox2();
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            PaintBox1();
            if (hue2loaded)
                PaintBox2();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value < vScrollBar.Maximum)
                {
                    vScrollBar.Value++;
                    PaintBox1();
                    if (hue2loaded)
                        PaintBox2();
                }
            }
            else
            {
                if (vScrollBar.Value > 1)
                {
                    vScrollBar.Value--;
                    PaintBox1();
                    if (hue2loaded)
                        PaintBox2();
                }
            }
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            if (textBox1.Text == null)
                return;
            string path = textBox1.Text;
            string file = Path.Combine(path, "hues.mul");
            if (File.Exists(file))
            {
                SecondHue.Initialize(file);
                hue2loaded = true;
                vScrollBar.Value = 0;
                selected = 0;
                PaintBox1();
                PaintBox2();
            }
        }

        private void OnMouseClick1(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
            Point m = PointToClient(Control.MousePosition);
            int index = GetIndex(m.Y / ITEMHEIGHT);
            if (index >= 0)
            {
                selected = index;
                PaintBox1();
                if (hue2loaded)
                    PaintBox2();
            }
        }

        private void OnMouseClick2(object sender, MouseEventArgs e)
        {
            pictureBox2.Focus();
            Point m = PointToClient(Control.MousePosition);
            int index = GetIndex(m.Y / ITEMHEIGHT);
            if (index >= 0)
            {
                selected = index;
                PaintBox1();
                if (hue2loaded)
                    PaintBox2();
            }
        }

        private bool Compare(int index)
        {
            if (m_Compare.Contains(index))
                return (bool)m_Compare[index];
            if (!hue2loaded)
                return true;
            Hue org = Ultima.Hues.List[index];
            Hue sec = SecondHue.List[index];
            if ((org == null) && (sec == null))
            {
                m_Compare[index] = true;
                return true;
            }
            if ((org == null) || (sec == null))
            {
                m_Compare[index] = false;
                return false;
            }
            for (int i = 0; i < org.Colors.Length; i++)
            {
                if (org.Colors[i] != sec.Colors[i])
                {
                    m_Compare[index] = false;
                    return false;
                }
            }
            m_Compare[index] = true;
            return true;
        }

        private void OnClickApplyHue1to2(object sender, EventArgs e)
        {
            if (!hue2loaded)
                return;
            Hue org = Ultima.Hues.List[selected];
            Hue sec = SecondHue.List[selected];
            sec.Colors.CopyTo(org.Colors, 0);
            org.Name = sec.Name;
            org.TableStart = org.Colors[0];
            org.TableEnd = org.Colors[org.Colors.Length - 1];
            m_Compare[selected] = true;
            PaintBox1();
            PaintBox2();
            FiddlerControls.Options.ChangedUltimaClass["Hues"] = true;
            FiddlerControls.Events.FireHueChangeEvent();
        }

        private void BrowseOnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the hue file";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                    textBox1.Text = dialog.SelectedPath;
            }
        }
    }
}
