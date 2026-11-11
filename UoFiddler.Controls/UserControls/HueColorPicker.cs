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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Ultima.Helpers;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.UserControls
{
    /// <summary>
    /// Picks a hue color with a saturation/value square, R/G/B and H/S/V sliders, typed values and a
    /// hex box. Every value is snapped to what a hue can store (32 levels per channel), so what is
    /// shown is what gets saved. <see cref="ColorChanging"/> fires while the user drags or types,
    /// <see cref="ColorCommitted"/> when an edit is finished.
    /// </summary>
    [DesignerCategory("Code")]
    public sealed class HueColorPicker : UserControl
    {
        private const int _recentCount = 12;
        private static readonly List<Color> _recent = [];
        private static event EventHandler RecentChanged;

        private enum Channel
        {
            Red,
            Green,
            Blue,
            Hue,
            Saturation,
            Value,
        }

        private sealed class ChannelRow
        {
            public Channel Channel;
            public Label Label;
            public ColorSlider Slider;
            public NumericUpDown Number;
            public int ScrubStartX;
            public int ScrubStartValue;
            public bool Scrubbing;
        }

        private readonly ChannelRow[] _rows;
        private readonly SaturationValueBox _square;
        private readonly CheckBox _levelsCheckBox;
        private readonly TextBox _hexTextBox;
        private readonly Panel _oldSwatch;
        private readonly Panel _newSwatch;
        private readonly FlowLayoutPanel _recentPanel;
        private readonly Label _noteLabel;
        private readonly ToolTip _toolTip = new();

        private Color _color = Color.FromArgb(HueHelpers.Expand5To8(16), HueHelpers.Expand5To8(16), HueHelpers.Expand5To8(16));
        private Color _original;
        private double _hue;
        private double _saturation;
        private double _value;
        private bool _updating;

        public HueColorPicker()
        {
            AutoScaleMode = AutoScaleMode.Font;
            Size = new Size(470, 236);
            MinimumSize = new Size(440, 230);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            // Left column: the square, then old/new swatches.
            var left = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty };
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 144));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _square = new SaturationValueBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 4) };
            _square.Changing += (_, _) => OnSquare(commit: false);
            _square.Committed += (_, _) => OnSquare(commit: true);
            _toolTip.SetToolTip(_square, "Saturation (left to right) and brightness (bottom to top)");
            left.Controls.Add(_square, 0, 0);

            var swatches = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 0, 6, 0) };
            swatches.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            swatches.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            _oldSwatch = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Margin = Padding.Empty, Cursor = Cursors.Hand };
            _newSwatch = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Margin = Padding.Empty };
            _oldSwatch.Click += (_, _) => SetFromUser(_original, commit: true);
            _toolTip.SetToolTip(_oldSwatch, "Before: click to go back to this color");
            _toolTip.SetToolTip(_newSwatch, "Now");
            swatches.Controls.Add(_oldSwatch, 0, 0);
            swatches.Controls.Add(_newSwatch, 1, 0);
            left.Controls.Add(swatches, 0, 1);

            _noteLabel = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Options.DarkMode ? Color.Orange : Color.DarkOrange,
                Text = string.Empty,
                Margin = new Padding(0, 2, 6, 0),
            };
            left.Controls.Add(_noteLabel, 0, 2);
            root.Controls.Add(left, 0, 0);

            // Right column: six channel rows, then the units, hex and recent colors.
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 9, Margin = Padding.Empty };
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20));
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 58));

            _rows = new ChannelRow[6];
            string[] names = ["R", "G", "B", "H", "S", "V"];
            string[] tips =
            [
                "Red", "Green", "Blue", "Hue (degrees)", "Saturation (%)", "Value / brightness (%)",
            ];
            for (int i = 0; i < _rows.Length; i++)
            {
                right.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
                var row = new ChannelRow { Channel = (Channel)i };
                row.Label = new Label
                {
                    Text = names[i],
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.SizeWE,
                    Margin = Padding.Empty,
                };
                _toolTip.SetToolTip(row.Label, $"{tips[i]}: drag left or right to change");
                row.Slider = new ColorSlider { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 4, 4) };
                row.Number = new NumericUpDown { Dock = DockStyle.Fill, Margin = new Padding(0, 1, 0, 1), TextAlign = HorizontalAlignment.Right };
                _toolTip.SetToolTip(row.Number, $"{tips[i]}: type a value, or use Up/Down (Shift: x4) or the mouse wheel");

                ChannelRow captured = row;
                row.Slider.ValueChanging += (_, _) => OnSlider(captured, commit: false);
                row.Slider.ValueCommitted += (_, _) => OnSlider(captured, commit: true);
                row.Number.ValueChanged += (_, _) => OnNumber(captured);
                row.Number.KeyDown += (_, e) => OnNumberKeyDown(captured, e);
                row.Label.MouseDown += (_, e) => OnScrubStart(captured, e);
                row.Label.MouseMove += (_, e) => OnScrubMove(captured, e);
                row.Label.MouseUp += (_, _) => OnScrubEnd(captured);

                right.Controls.Add(row.Label, 0, i);
                right.Controls.Add(row.Slider, 1, i);
                right.Controls.Add(row.Number, 2, i);
                _rows[i] = row;
            }

            _rows[3].Slider.Maximum = 359;
            _rows[3].Number.Maximum = 359;
            _rows[4].Slider.Maximum = 100;
            _rows[4].Number.Maximum = 100;
            _rows[5].Slider.Maximum = 100;
            _rows[5].Number.Maximum = 100;

            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            _levelsCheckBox = new CheckBox
            {
                Text = "R/G/B as stored levels (0-31)",
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
            };
            _toolTip.SetToolTip(_levelsCheckBox, "Hues keep 32 levels per channel. Show R/G/B as those levels instead of 0-255.");
            _levelsCheckBox.CheckedChanged += (_, _) => ApplyUnits();
            right.Controls.Add(_levelsCheckBox, 0, 6);
            right.SetColumnSpan(_levelsCheckBox, 3);

            right.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            var hexLabel = new Label { Text = "#", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Margin = Padding.Empty };
            _hexTextBox = new TextBox { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
            _toolTip.SetToolTip(_hexTextBox, "Type or paste #RRGGBB, r,g,b or a raw 15-bit 0x7FFF, then press Enter");
            _hexTextBox.KeyDown += OnHexKeyDown;
            _hexTextBox.Leave += (_, _) => ApplyHex();
            right.Controls.Add(hexLabel, 0, 7);
            right.Controls.Add(_hexTextBox, 1, 7);
            right.SetColumnSpan(_hexTextBox, 2);

            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _recentPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Margin = Padding.Empty, WrapContents = false };
            right.Controls.Add(_recentPanel, 0, 8);
            right.SetColumnSpan(_recentPanel, 3);

            root.Controls.Add(right, 1, 0);
            Controls.Add(root);

            foreach (ChannelRow row in _rows)
            {
                ChannelRow captured = row;
                row.Slider.SetTrack(t => TrackColor(captured.Channel, t));
            }

            RecentChanged += OnRecentChanged;
            ApplyUnits();
            SetColor(_color, true);
            RebuildRecent();
        }

        /// <summary>Raised while the user changes the color.</summary>
        public event EventHandler<Color> ColorChanging;

        /// <summary>Raised when the user finishes an edit.</summary>
        public event EventHandler<Color> ColorCommitted;

        /// <summary>The current color, already snapped to what a hue stores.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Color => _color;

        /// <summary>
        /// Shows a color without raising events. With <paramref name="resetOriginal"/> it also
        /// becomes the "before" swatch.
        /// </summary>
        public void SetColor(Color color, bool resetOriginal)
        {
            ApplyRgb(color);
            if (resetOriginal)
            {
                _original = _color;
            }

            _noteLabel.Text = string.Empty;
            UpdateControls();
        }

        /// <summary>Adds a color to the recent row shared by every picker.</summary>
        public static void AddRecent(Color color)
        {
            color = HueRamp.Snap(color);
            _recent.Remove(color);
            _recent.Insert(0, color);
            if (_recent.Count > _recentCount)
            {
                _recent.RemoveAt(_recent.Count - 1);
            }

            RecentChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Opens a picker in a drop-down under <paramref name="owner"/>. <paramref name="changed"/>
        /// gets every change; the drop-down closes on a click outside it.
        /// </summary>
        public static void ShowDropDown(Control owner, Color initial, Action<Color> changed)
        {
            var picker = new HueColorPicker();
            picker.SetColor(initial, true);
            picker.ColorChanging += (_, color) => changed(color);
            picker.ColorCommitted += (_, color) => changed(color);

            var host = new ToolStripControlHost(picker) { AutoSize = false, Size = picker.Size, Margin = Padding.Empty, Padding = Padding.Empty };
            var dropDown = new ToolStripDropDown { Padding = new Padding(4), AutoClose = true };
            dropDown.Items.Add(host);
            dropDown.Closed += (_, _) =>
            {
                AddRecent(picker.Color);
                dropDown.BeginInvoke(dropDown.Dispose);
            };
            dropDown.Show(owner, new Point(0, owner.Height));
        }

        private Color TrackColor(Channel channel, double t) => channel switch
        {
            Channel.Red => Color.FromArgb((int)(t * 255), _color.G, _color.B),
            Channel.Green => Color.FromArgb(_color.R, (int)(t * 255), _color.B),
            Channel.Blue => Color.FromArgb(_color.R, _color.G, (int)(t * 255)),
            Channel.Hue => FromHsv(t * 360, 1, 1),
            Channel.Saturation => FromHsv(_hue, t, Math.Max(_value, 0.15)),
            _ => FromHsv(_hue, _saturation, t),
        };

        private void ApplyRgb(Color color)
        {
            _color = HueRamp.Snap(color);
            ToHsv(_color, out double h, out double s, out double v);
            if (s > 0 && v > 0)
            {
                _hue = h;
            }

            if (v > 0)
            {
                _saturation = s;
            }

            _value = v;
        }

        private void ApplyHsv()
        {
            _color = HueRamp.Snap(FromHsv(_hue, _saturation, _value));
        }

        private void SetFromUser(Color color, bool commit)
        {
            ApplyRgb(color);
            Raise(color, commit);
        }

        private void Raise(Color requested, bool commit)
        {
            _noteLabel.Text = HueRamp.IsClampedBlack(requested)
                ? "Pure black can't be stored in a hue; using the darkest level."
                : string.Empty;
            UpdateControls();
            ColorChanging?.Invoke(this, _color);
            if (commit)
            {
                ColorCommitted?.Invoke(this, _color);
                AddRecent(_color);
            }
        }

        private void OnSquare(bool commit)
        {
            _saturation = _square.Saturation;
            _value = _square.Value;
            Color requested = FromHsv(_hue, _saturation, _value);
            ApplyHsv();
            Raise(requested, commit);
        }

        private void OnSlider(ChannelRow row, bool commit)
        {
            if (_updating)
            {
                return;
            }

            SetChannel(row.Channel, row.Slider.Value, sliderUnits: true, commit);
        }

        private void OnNumber(ChannelRow row)
        {
            if (_updating)
            {
                return;
            }

            SetChannel(row.Channel, (int)row.Number.Value, sliderUnits: false, commit: true);
        }

        private void OnNumberKeyDown(ChannelRow row, KeyEventArgs e)
        {
            if (!e.Shift || (e.KeyCode != Keys.Up && e.KeyCode != Keys.Down))
            {
                return;
            }

            decimal step = row.Number.Increment * 4 * (e.KeyCode == Keys.Up ? 1 : -1);
            row.Number.Value = Math.Clamp(row.Number.Value + step, row.Number.Minimum, row.Number.Maximum);
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void OnScrubStart(ChannelRow row, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            row.Scrubbing = true;
            row.ScrubStartX = Cursor.Position.X;
            row.ScrubStartValue = row.Slider.Value;
        }

        private void OnScrubMove(ChannelRow row, MouseEventArgs e)
        {
            if (!row.Scrubbing)
            {
                return;
            }

            // Four pixels of mouse movement per step keeps fine control.
            int target = Math.Clamp(row.ScrubStartValue + ((Cursor.Position.X - row.ScrubStartX) / 4), row.Slider.Minimum, row.Slider.Maximum);
            if (target != row.Slider.Value)
            {
                SetChannel(row.Channel, target, sliderUnits: true, commit: false);
            }
        }

        private void OnScrubEnd(ChannelRow row)
        {
            if (!row.Scrubbing)
            {
                return;
            }

            row.Scrubbing = false;
            Raise(_color, commit: true);
        }

        /// <summary>
        /// Applies one channel. Slider units are levels (0-31) for R/G/B; number units are
        /// levels or 0-255 depending on the checkbox. H is degrees, S and V are percent.
        /// </summary>
        private void SetChannel(Channel channel, int value, bool sliderUnits, bool commit)
        {
            bool levels = sliderUnits || _levelsCheckBox.Checked;
            int ToByte(int v) => levels ? HueHelpers.Expand5To8(Math.Clamp(v, 0, 31)) : Math.Clamp(v, 0, 255);

            Color requested;
            switch (channel)
            {
                case Channel.Red:
                    requested = Color.FromArgb(ToByte(value), _color.G, _color.B);
                    SetFromUser(requested, commit);
                    return;
                case Channel.Green:
                    requested = Color.FromArgb(_color.R, ToByte(value), _color.B);
                    SetFromUser(requested, commit);
                    return;
                case Channel.Blue:
                    requested = Color.FromArgb(_color.R, _color.G, ToByte(value));
                    SetFromUser(requested, commit);
                    return;
                case Channel.Hue:
                    _hue = value;
                    break;
                case Channel.Saturation:
                    _saturation = value / 100.0;
                    break;
                default:
                    _value = value / 100.0;
                    break;
            }

            requested = FromHsv(_hue, _saturation, _value);
            ApplyHsv();
            Raise(requested, commit);
        }

        private void OnHexKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ApplyHex();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                _hexTextBox.Text = HueColorText.ToHex(_color);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void ApplyHex()
        {
            if (_updating)
            {
                return;
            }

            string text = _hexTextBox.Text;
            if (HueColorText.TryParse(text, out Color parsed))
            {
                if (HueRamp.Snap(parsed) != _color || HueRamp.IsClampedBlack(parsed))
                {
                    SetFromUser(parsed, commit: true);
                }
                else
                {
                    _hexTextBox.Text = HueColorText.ToHex(_color);
                }

                _hexTextBox.BackColor = SystemColors.Window;
            }
            else if (text.Trim().Length > 0 && text != HueColorText.ToHex(_color))
            {
                _hexTextBox.BackColor = Options.DarkMode ? Color.DarkRed : Color.MistyRose;
            }
        }

        private void ApplyUnits()
        {
            bool levels = _levelsCheckBox.Checked;
            for (int i = 0; i < 3; i++)
            {
                _rows[i].Slider.Maximum = 31;
                _updating = true;
                _rows[i].Number.Maximum = levels ? 31 : 255;
                _updating = false;
            }

            UpdateControls();
        }

        private void UpdateControls()
        {
            _updating = true;
            try
            {
                HueHelpers.HueExtract5(HueRamp.Pack(_color), out int r5, out int g5, out int b5);
                int[] levels = [r5, g5, b5];
                int[] bytes = [_color.R, _color.G, _color.B];
                for (int i = 0; i < 3; i++)
                {
                    _rows[i].Slider.Value = levels[i];
                    _rows[i].Number.Value = _levelsCheckBox.Checked ? levels[i] : bytes[i];
                }

                int h = (int)Math.Round(_hue) % 360;
                int s = (int)Math.Round(_saturation * 100);
                int v = (int)Math.Round(_value * 100);
                _rows[3].Slider.Value = h;
                _rows[3].Number.Value = h;
                _rows[4].Slider.Value = s;
                _rows[4].Number.Value = s;
                _rows[5].Slider.Value = v;
                _rows[5].Number.Value = v;

                foreach (ChannelRow row in _rows)
                {
                    row.Slider.Invalidate();
                }

                _square.SetState(_hue, _saturation, _value);
                if (!_hexTextBox.Focused)
                {
                    _hexTextBox.Text = HueColorText.ToHex(_color);
                    _hexTextBox.BackColor = SystemColors.Window;
                }

                _newSwatch.BackColor = _color;
                _oldSwatch.BackColor = _original;
            }
            finally
            {
                _updating = false;
            }
        }

        private void OnRecentChanged(object sender, EventArgs e)
        {
            if (!IsDisposed)
            {
                RebuildRecent();
            }
        }

        private void RebuildRecent()
        {
            _recentPanel.SuspendLayout();
            foreach (Control control in _recentPanel.Controls)
            {
                control.Dispose();
            }

            _recentPanel.Controls.Clear();
            foreach (Color color in _recent)
            {
                Color captured = color;
                var swatch = new Panel
                {
                    Size = new Size(18, 18),
                    Margin = new Padding(0, 2, 3, 0),
                    BackColor = color,
                    BorderStyle = BorderStyle.FixedSingle,
                    Cursor = Cursors.Hand,
                };
                swatch.Click += (_, _) => SetFromUser(captured, commit: true);
                _toolTip.SetToolTip(swatch, $"Recent: {HueColorText.ToHex(color)}");
                _recentPanel.Controls.Add(swatch);
            }

            _recentPanel.ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RecentChanged -= OnRecentChanged;
                _toolTip.Dispose();
            }

            base.Dispose(disposing);
        }

        internal static void ToHsv(Color color, out double h, out double s, out double v)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            v = max;
            s = max == 0 ? 0 : delta / max;
            if (delta == 0)
            {
                h = 0;
                return;
            }

            if (max == r)
            {
                h = 60 * (((g - b) / delta) % 6);
            }
            else if (max == g)
            {
                h = 60 * (((b - r) / delta) + 2);
            }
            else
            {
                h = 60 * (((r - g) / delta) + 4);
            }

            if (h < 0)
            {
                h += 360;
            }
        }

        internal static Color FromHsv(double h, double s, double v)
        {
            h = ((h % 360) + 360) % 360;
            s = Math.Clamp(s, 0, 1);
            v = Math.Clamp(v, 0, 1);
            double c = v * s;
            double x = c * (1 - Math.Abs(((h / 60) % 2) - 1));
            double m = v - c;
            (double r, double g, double b) = h switch
            {
                < 60 => (c, x, 0.0),
                < 120 => (x, c, 0.0),
                < 180 => (0.0, c, x),
                < 240 => (0.0, x, c),
                < 300 => (x, 0.0, c),
                _ => (c, 0.0, x),
            };

            static int Byte(double value) => Math.Clamp((int)Math.Round(value * 255), 0, 255);
            return Color.FromArgb(Byte(r + m), Byte(g + m), Byte(b + m));
        }

        /// <summary>The saturation (x) / value (y) square for the current hue.</summary>
        private sealed class SaturationValueBox : Control
        {
            private Bitmap _cache;
            private double _cacheHue = double.NaN;
            private Size _cacheSize;
            private double _hue;
            private bool _dragging;

            public SaturationValueBox()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                         | ControlStyles.ResizeRedraw, true);
                Cursor = Cursors.Cross;
            }

            public event EventHandler Changing;

            public event EventHandler Committed;

            public double Saturation { get; private set; }

            public double Value { get; private set; }

            public void SetState(double hue, double saturation, double value)
            {
                _hue = hue;
                Saturation = saturation;
                Value = value;
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (Width <= 2 || Height <= 2)
                {
                    return;
                }

                if (_cache == null || _cacheHue != _hue || _cacheSize != ClientSize)
                {
                    RebuildCache();
                }

                e.Graphics.DrawImageUnscaled(_cache, 0, 0);
                int x = (int)Math.Round(Saturation * (Width - 1));
                int y = (int)Math.Round((1 - Value) * (Height - 1));
                e.Graphics.DrawEllipse(Pens.Black, x - 5, y - 5, 10, 10);
                e.Graphics.DrawEllipse(Pens.White, x - 4, y - 4, 8, 8);
                e.Graphics.DrawRectangle(SystemPens.ControlDark, 0, 0, Width - 1, Height - 1);
            }

            private void RebuildCache()
            {
                _cache?.Dispose();
                _cacheSize = ClientSize;
                _cacheHue = _hue;
                int w = _cacheSize.Width;
                int h = _cacheSize.Height;
                _cache = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                var pixels = new int[w * h];
                for (int y = 0; y < h; y++)
                {
                    double v = 1 - (y / (double)(h - 1));
                    for (int x = 0; x < w; x++)
                    {
                        pixels[(y * w) + x] = HueRamp.Snap(FromHsv(_hue, x / (double)(w - 1), v)).ToArgb() | unchecked((int)0xFF000000);
                    }
                }

                BitmapData data = _cache.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                for (int y = 0; y < h; y++)
                {
                    Marshal.Copy(pixels, y * w, data.Scan0 + (y * data.Stride), w);
                }

                _cache.UnlockBits(data);
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (e.Button == MouseButtons.Left)
                {
                    _dragging = true;
                    Capture = true;
                    Pick(e.Location);
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                if (_dragging)
                {
                    Pick(e.Location);
                }
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                if (_dragging)
                {
                    _dragging = false;
                    Capture = false;
                    Committed?.Invoke(this, EventArgs.Empty);
                }
            }

            private void Pick(Point point)
            {
                Saturation = Math.Clamp(point.X / (double)Math.Max(1, Width - 1), 0, 1);
                Value = Math.Clamp(1 - (point.Y / (double)Math.Max(1, Height - 1)), 0, 1);
                Invalidate();
                Changing?.Invoke(this, EventArgs.Empty);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _cache?.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
