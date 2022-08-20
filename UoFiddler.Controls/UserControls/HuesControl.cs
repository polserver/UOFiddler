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
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Ultima;
using UoFiddler.Controls.Classes;
using UoFiddler.Controls.Forms;
using UoFiddler.Controls.Helpers;

namespace UoFiddler.Controls.UserControls
{
    public partial class HuesControl : UserControl
    {
        public HuesControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint, true);

            pictureBox.MouseWheel += OnMouseWheel;
            _refMarker = this;
        }

        private const int _itemHeight = 20;
        private int _selected;
        private bool _loaded;
        private int _row;
        private readonly HuesControl _refMarker;

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
                if (FormsDesignerHelper.IsInDesignMode())
                {
                    return;
                }

                if (!_loaded)
                {
                    return;
                }

                if (Hues.List.Length > 0)
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
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            Options.LoadedUltimaClass["Hues"] = true;
            if (Parent.GetType() == typeof(HuePopUpItemForm) || Parent.GetType() == typeof(HuePopUpForm) || Parent.GetType() == typeof(HuePopUpDress))
            {
                pictureBox.MouseDoubleClick -= OnMouseDoubleClick;
                pictureBox.ContextMenuStrip = new ContextMenuStrip();
            }

            vScrollBar.Maximum = Hues.List.Length;
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

        private const int _noSelectionValue = -1;

        private int GetIndex(int y)
        {
            int value = vScrollBar.Value + y;
            return Hues.List.Length > value ? value : _noSelectionValue;
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (FormsDesignerHelper.IsInDesignMode())
            {
                return;
            }

            e.Graphics.Clear(Color.White);

            for (int y = 0; y <= _row; ++y)
            {
                int index = GetIndex(y);
                if (index < 0)
                {
                    continue;
                }

                Rectangle rect = new Rectangle(0, y * _itemHeight, 200, _itemHeight);
                e.Graphics.FillRectangle(index == _selected ? SystemBrushes.Highlight : SystemBrushes.Window, rect);

                float size = (float)(pictureBox.Width - 200) / 32;
                Hue hue = Hues.List[index];
                Rectangle stringRect = new Rectangle(3, y * _itemHeight, pictureBox.Width, _itemHeight);
                e.Graphics.DrawString(
                    $"{hue.Index,-5} {$"(0x{hue.Index:X})",-7} {hue.Name}", Font, Brushes.Black, stringRect);

                for (int i = 0; i < hue.Colors.Length; ++i)
                {
                    Rectangle rectangle = new Rectangle(200 + (int)Math.Round(i * size), y * _itemHeight, (int)Math.Round(size + 1f), _itemHeight);
                    using (var brush = new SolidBrush(hue.GetColor(i)))
                    {
                        e.Graphics.FillRectangle(brush, rectangle);
                    }
                }
            }
        }

        private void OnScroll(object sender, ScrollEventArgs e)
        {
            pictureBox.Invalidate();
        }

        private const int _scrollStep = 4;

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
            var newValue = e.Delta < 0 ? vScrollBar.Value + _scrollStep : vScrollBar.Value - _scrollStep;

            if (newValue < vScrollBar.Minimum || newValue >= vScrollBar.Maximum)
            {
                return;
            }

            vScrollBar.Value = newValue;

            pictureBox.Invalidate();
        }

        private void OnResize(object sender, EventArgs e)
        {
            _row = pictureBox.Height / _itemHeight;
            if (!_loaded)
            {
                return;
            }

            pictureBox.Invalidate();
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            pictureBox.Focus();

            UpdateSelection();
        }

        private void UpdateSelection()
        {
            Point m = pictureBox.PointToClient(MousePosition);

            int index = GetIndex(m.Y / _itemHeight);

            if (index >= 0)
            {
                Selected = index;
            }
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            UpdateSelection();

            if (Selected >= 0)
            {
                new HueEditForm(Selected).Show();
            }
        }

        private void OnClickSave(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            Hues.Save(path);
            MessageBox.Show($"Hue saved to {path}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information,
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

            if (!Utils.ConvertStringToInt(ReplaceText.Text, out int index, 0, Hues.List.Length))
            {
                return;
            }

            contextMenuStrip1.Close();
            Hues.List[_selected] = Hues.List[index];
            pictureBox.Invalidate();
        }

        private void OnExport(object sender, EventArgs e)
        {
            string path = Options.OutputPath;
            string fileName = Path.Combine(path, $"Hue {_selected}.txt");
            Hues.List[_selected].Export(fileName);
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

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Hues.List[_selected].Import(dialog.FileName);
            Options.ChangedUltimaClass["Hues"] = true;
            ControlEvents.FireHueChangeEvent();
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

        private void HueIndexToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var maximumIndex = Hues.List.Length;

            if (!Utils.ConvertStringToInt(HueIndexToolStripTextBox.Text, out int indexValue))
            {
                return;
            }

            if (indexValue < 0)
            {
                indexValue = 0;
            }

            if (indexValue >= maximumIndex)
            {
                indexValue = maximumIndex - 1;
            }

            var scrollPosition = indexValue - 1;
            if (scrollPosition < 0)
            {
                scrollPosition = 0;
            }

            Selected = indexValue;
            vScrollBar.Value = scrollPosition;
        }

        private void SearchNameToolStripButton_Click(object sender, EventArgs e)
        {
            FindByName(Selected);
        }

        private void HueNameToolStripTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            FindByName(0);
        }

        private void FindByName(int startIndex)
        {
            if (HueNameToolStripTextBox.TextBox == null)
            {
                return;
            }

            var search = HueNameToolStripTextBox.TextBox.Text.Trim();

            if (string.IsNullOrEmpty(search))
            {
                return;
            }

            var searchResult = FindByNameInternal(search, startIndex);

            // try from the start
            if (searchResult == null)
            {
                searchResult = FindByNameInternal(search, 0);

                // searching from start failed we can return
                if (searchResult == null)
                {
                    return;
                }
            }

            Selected = searchResult.Index;
            vScrollBar.Value = searchResult.Index - 1 < 0 ? 0 : searchResult.Index;
        }

        private static Hue FindByNameInternal(string search, int currentIndex)
        {
            return Hues.List.FirstOrDefault(hue =>
                hue.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 && hue.Index > currentIndex);
        }
    }
}
