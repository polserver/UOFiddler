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
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;

namespace UoFiddler.Controls.UserControls
{
    public partial class Hues : UserControl
    {
        public Hues()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);
            pictureBox.MouseWheel += OnMouseWheel;
            _refMarker = this;
        }

        private const int ItemHeight = 20;
        private int _selected;
        private bool _loaded;
        private int _row;
        private readonly Hues _refMarker;

        /// <summary>
        /// Sets Selected Hue
        /// </summary>
        [Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                if (!_loaded)
                {
                    return;
                }

                if (Ultima.Hues.List.Length > 0)
                {
                    pictureBox.Invalidate();
                }
            }
        }

        /// <summary>
        /// Reload when loaded (file changed)
        /// </summary>
        private void Reload()
        {
            if (!_loaded)
            {
                return;
            }

            _selected = 0;
            OnLoad(this, EventArgs.Empty);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Options.LoadedUltimaClass["Hues"] = true;
            if (Parent.GetType() == typeof(HuePopUpItem) || Parent.GetType() == typeof(HuePopUp) || Parent.GetType() == typeof(HuePopUpDress))
            {
                pictureBox.MouseDoubleClick -= OnMouseDoubleClick;
                pictureBox.ContextMenuStrip = new ContextMenuStrip();
            }

            vScrollBar.Maximum = Ultima.Hues.List.Length;
            vScrollBar.Minimum = 0;
            vScrollBar.Value = 0;
            vScrollBar.SmallChange = 1;
            vScrollBar.LargeChange = 10;

            if (_selected > 0)
            {
                vScrollBar.Value = _selected;
            }

            pictureBox.Invalidate();
            if (!_loaded)
            {
                ControlEvents.FilePathChangeEvent += OnFilePathChangeEvent;
                ControlEvents.HueChangeEvent += OnHueChangeEvent;
            }
            _loaded = true;
        }

        private void OnFilePathChangeEvent()
        {
            Reload();
        }

        private void OnHueChangeEvent()
        {
            pictureBox.Invalidate();
        }

        private int GetIndex(int y)
        {
            int value = vScrollBar.Value + y;
            return Ultima.Hues.List.Length > value ? value : -1;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            for (int y = 0; y <= _row; ++y)
            {
                int index = GetIndex(y);
                if (index >= 0)
                {
                    Rectangle rect = new Rectangle(0, y * ItemHeight, 200, ItemHeight);
                    if (index == _selected)
                    {
                        e.Graphics.FillRectangle(SystemBrushes.Highlight, rect);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(SystemBrushes.Window, rect);
                    }

                    float size = (float)(pictureBox.Width - 200) / 32;
                    Hue hue = Ultima.Hues.List[index];
                    Rectangle stringRect = new Rectangle(3, y * ItemHeight, pictureBox.Width, ItemHeight);
                    e.Graphics.DrawString(
                        $"{hue.Index + 1,-5} {$"(0x{hue.Index + 1:X})",-7} {hue.Name}", Font, Brushes.Black, stringRect);

                    for (int i = 0; i < hue.Colors.Length; ++i)
                    {
                        Rectangle rectangle = new Rectangle(200 + (int)Math.Round(i * size), y * ItemHeight, (int)Math.Round(size + 1f), ItemHeight);
                        e.Graphics.FillRectangle(new SolidBrush(hue.GetColor(i)), rectangle);
                    }
                }
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (vScrollBar.Value < vScrollBar.Maximum)
                {
                    vScrollBar.Value++;
                    pictureBox.Invalidate();
                }
            }
            else
            {
                if (vScrollBar.Value > 1)
                {
                    vScrollBar.Value--;
                    pictureBox.Invalidate();
                }
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            _row = pictureBox.Height / ItemHeight;
            if (!_loaded)
            {
                return;
            }

            pictureBox.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();
            Point m = PointToClient(MousePosition);
            int index = GetIndex(m.Y / ItemHeight);
            if (index >= 0)
            {
                Selected = index;
            }
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            Point m = PointToClient(MousePosition);
            int index = GetIndex(m.Y / ItemHeight);
            if (index >= 0)
            {
                Selected = index;
            }

            new HueEdit(index, _refMarker).Show();
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            Ultima.Hues.Save(path);
            MessageBox.Show(
                $"Hue saved to {path}",
                "Saved",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            Options.ChangedUltimaClass["Hues"] = false;
        }

        private void OnTextChangedReplace(object sender, EventArgs e)
        {
            ReplaceText.ForeColor = Utils.ConvertStringToInt(ReplaceText.Text, out _, 1, 3000) ? Color.Black : Color.Red;
        }

        private void OnKeyDownReplace(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            if (!Utils.ConvertStringToInt(ReplaceText.Text, out int index, 1, 3000))
            {
                return;
            }

            contextMenuStrip1.Close();
            Ultima.Hues.List[_selected] = Ultima.Hues.List[index - 1];
            pictureBox.Invalidate();
        }

        private void OnExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Hue {_selected + 1}.txt");
            Ultima.Hues.List[_selected].Export(fileName);
            MessageBox.Show($"Hue saved to {fileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        private void OnImport(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Multiselect = false,
                Title = "Choose txt file to import",
                CheckFileExists = true,
                Filter = "txt files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Ultima.Hues.List[_selected].Import(dialog.FileName);
                Options.ChangedUltimaClass["Hues"] = true;
                ControlEvents.FireHueChangeEvent();
            }
        }

        /// <summary>
        /// Print a nice border
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            const int borderWidth = 1;

            Color borderColor = VisualStyleInformation.TextControlBorder;

            ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, borderColor,
                      borderWidth, ButtonBorderStyle.Solid, borderColor, borderWidth,
                      ButtonBorderStyle.Solid, borderColor, borderWidth, ButtonBorderStyle.Solid,
                      borderColor, borderWidth, ButtonBorderStyle.Solid);
        }
    }
}
