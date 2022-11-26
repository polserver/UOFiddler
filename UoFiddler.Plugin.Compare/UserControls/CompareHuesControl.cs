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
using System.IO;
using System.Windows.Forms;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.Compare.Classes;

namespace UoFiddler.Plugin.Compare.UserControls
{
    public partial class CompareHuesControl : UserControl
    {
        public CompareHuesControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            pictureBox1.Image = _bmp1;
            pictureBox2.Image = _bmp2;

            pictureBox1.MouseWheel += OnMouseWheel;
            pictureBox2.MouseWheel += OnMouseWheel;

            _hue2Loaded = false;
        }

        private const int _itemHeight = 20;
        private Bitmap _bmp1;
        private Bitmap _bmp2;
        private int _selected;
        private int _row;
        private bool _hue2Loaded;
        private readonly Dictionary<int, bool> _compare = new Dictionary<int, bool>();
        private bool _loaded;

        private void OnLoad(object sender, EventArgs e)
        {
            vScrollBar.Maximum = Hues.List.Length;
            vScrollBar.Minimum = 0;
            vScrollBar.Value = 0;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = 10;
            _selected = 0;
            _bmp1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            _bmp2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            _loaded = true;
            _row = pictureBox1.Height / _itemHeight;
            PaintBox1();
        }

        private int GetIndex(int y)
        {
            int value = vScrollBar.Value + y;
            return Hues.List.Length > value ? value : -1;
        }

        private void PaintBox1()
        {
            using (Graphics g = Graphics.FromImage(_bmp1))
            {
                g.Clear(Color.White);

                for (int y = 0; y <= _row; y++)
                {
                    int index = GetIndex(y);
                    if (index < 0)
                    {
                        continue;
                    }

                    Rectangle rect = new Rectangle(0, y * _itemHeight, 200, _itemHeight);
                    if (index == _selected)
                    {
                        g.FillRectangle(SystemBrushes.Highlight, rect);
                    }
                    else if (!Compare(index))
                    {
                        g.FillRectangle(Brushes.Red, rect);
                    }
                    else
                    {
                        g.FillRectangle(SystemBrushes.Window, rect);
                    }

                    float size = (float)(pictureBox1.Width - 200) / 32;
                    Hue hue = Hues.List[index];
                    Rectangle stringRect = new Rectangle(3, y * _itemHeight, pictureBox1.Width, _itemHeight);
                    g.DrawString($"{hue.Index + 1,-5} {$"(0x{hue.Index + 1:X})",-7} {hue.Name}", Font, Brushes.Black, stringRect);

                    for (int i = 0; i < hue.Colors.Length; i++)
                    {
                        Rectangle rectangle = new Rectangle(200 + (int)Math.Round(i * size), y * _itemHeight, (int)Math.Round(size + 1f), _itemHeight);
                        using (var solidBrush = new SolidBrush(hue.GetColor(i)))
                        {
                            g.FillRectangle(solidBrush, rectangle);
                        }
                    }
                }
            }
            pictureBox1.Image = _bmp1;
            pictureBox1.Update();
        }

        private void PaintBox2()
        {
            using (Graphics g = Graphics.FromImage(_bmp2))
            {
                g.Clear(Color.White);

                for (int y = 0; y <= _row; y++)
                {
                    int index = GetIndex(y);
                    if (index < 0)
                    {
                        continue;
                    }

                    Rectangle rect = new Rectangle(0, y * _itemHeight, 200, _itemHeight);
                    if (index == _selected)
                    {
                        g.FillRectangle(SystemBrushes.Highlight, rect);
                    }
                    else if (!Compare(index))
                    {
                        g.FillRectangle(Brushes.Red, rect);
                    }
                    else
                    {
                        g.FillRectangle(SystemBrushes.Window, rect);
                    }

                    float size = (float)(pictureBox2.Width - 200) / 32;
                    Hue hue = SecondHue.List[index];
                    Rectangle stringRect = new Rectangle(3, y * _itemHeight, pictureBox2.Width, _itemHeight);
                    g.DrawString($"{hue.Index + 1,-5} {$"(0x{hue.Index + 1:X})",-7} {hue.Name}", Font, Brushes.Black, stringRect);

                    for (int i = 0; i < hue.Colors.Length; i++)
                    {
                        Rectangle rectangle = new Rectangle(200 + (int)Math.Round(i * size), y * _itemHeight, (int)Math.Round(size + 1f), _itemHeight);
                        using (var solidBrush = new SolidBrush(hue.GetColor(i)))
                        {
                            g.FillRectangle(solidBrush, rectangle);
                        }
                    }
                }
            }
            pictureBox2.Image = _bmp2;
            pictureBox2.Update();
        }

        private void OnResizeHue(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                return;
            }

            _row = pictureBox1.Height / _itemHeight;
            _bmp1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            _bmp2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);

            PaintBox1();

            if (_hue2Loaded)
            {
                PaintBox2();
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            PaintBox1();
            if (_hue2Loaded)
            {
                PaintBox2();
            }
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value >= vScrollBar.Maximum)
                {
                    return;
                }

                vScrollBar.Value++;
                PaintBox1();
                if (_hue2Loaded)
                {
                    PaintBox2();
                }
            }
            else
            {
                if (vScrollBar.Value <= 1)
                {
                    return;
                }

                vScrollBar.Value--;
                PaintBox1();
                if (_hue2Loaded)
                {
                    PaintBox2();
                }
            }
        }

        private void OnClickLoad(object sender, EventArgs e)
        {
            if (textBox1.Text == null)
            {
                return;
            }

            string path = textBox1.Text;
            string file = Path.Combine(path, "hues.mul");
            if (!File.Exists(file))
            {
                return;
            }

            SecondHue.Initialize(file);
            _hue2Loaded = true;
            vScrollBar.Value = 0;
            _selected = 0;
            PaintBox1();
            PaintBox2();
        }

        private void OnMouseClick1(object sender, MouseEventArgs e)
        {
            pictureBox1.Focus();
            Point m = PointToClient(MousePosition);
            int index = GetIndex(m.Y / _itemHeight);
            if (index < 0)
            {
                return;
            }

            _selected = index;
            PaintBox1();
            if (_hue2Loaded)
            {
                PaintBox2();
            }
        }

        private void OnMouseClick2(object sender, MouseEventArgs e)
        {
            pictureBox2.Focus();
            Point m = PointToClient(MousePosition);
            int index = GetIndex(m.Y / _itemHeight);
            if (index < 0)
            {
                return;
            }

            _selected = index;
            PaintBox1();
            if (_hue2Loaded)
            {
                PaintBox2();
            }
        }

        private bool Compare(int index)
        {
            if (_compare.ContainsKey(index))
            {
                return _compare[index];
            }

            if (!_hue2Loaded)
            {
                return true;
            }

            Hue org = Hues.List[index];
            Hue sec = SecondHue.List[index];
            if (org == null && sec == null)
            {
                _compare[index] = true;
                return true;
            }
            if (org == null || sec == null)
            {
                _compare[index] = false;
                return false;
            }

            for (int i = 0; i < org.Colors.Length; i++)
            {
                if (org.Colors[i] != sec.Colors[i])
                {
                    _compare[index] = false;
                    return false;
                }
            }

            _compare[index] = true;
            return true;
        }

        private void OnClickApplyHue1to2(object sender, EventArgs e)
        {
            if (!_hue2Loaded)
            {
                return;
            }

            Hue org = Hues.List[_selected];
            Hue sec = SecondHue.List[_selected];
            sec.Colors.CopyTo(org.Colors, 0);
            org.Name = sec.Name;
            org.TableStart = org.Colors[0];
            org.TableEnd = (ushort)(org.Colors[org.Colors.Length - 1] + 1057);
            _compare[_selected] = true;
            PaintBox1();
            PaintBox2();
            Options.ChangedUltimaClass["Hues"] = true;
            ControlEvents.FireHueChangeEvent();
        }

        private void BrowseOnClick(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select directory containing the hue file";
                dialog.ShowNewFolderButton = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
