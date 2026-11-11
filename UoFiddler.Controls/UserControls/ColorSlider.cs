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

namespace UoFiddler.Controls.UserControls
{
    /// <summary>
    /// A horizontal slider whose track shows the colors its values would give. Arrow keys and the
    /// mouse wheel step by one (Shift: four); dragging raises <see cref="ValueChanging"/> and
    /// releasing raises <see cref="ValueCommitted"/>.
    /// </summary>
    [DesignerCategory("Code")]
    public class ColorSlider : Control
    {
        private const int _thumbHalf = 5;

        private int _minimum;
        private int _maximum = 100;
        private int _value;
        private bool _dragging;
        private Func<double, Color> _track = t => Color.FromArgb((int)(t * 255), (int)(t * 255), (int)(t * 255));

        public ColorSlider()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint
                     | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            Height = 18;
        }

        /// <summary>Raised while the value changes from the user's input.</summary>
        public event EventHandler ValueChanging;

        /// <summary>Raised once the user lets go (mouse up, or after each key/wheel step).</summary>
        public event EventHandler ValueCommitted;

        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                _value = Math.Max(_value, value);
                Invalidate();
            }
        }

        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                _value = Math.Min(_value, value);
                Invalidate();
            }
        }

        /// <summary>Setting the value from code raises no events.</summary>
        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                value = Math.Clamp(value, _minimum, _maximum);
                if (value != _value)
                {
                    _value = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Gives the track color at a position from 0 (minimum) to 1 (maximum).</summary>
        public void SetTrack(Func<double, Color> track)
        {
            _track = track;
            Invalidate();
        }

        private Rectangle TrackRect => new(_thumbHalf, 3, Math.Max(1, Width - (2 * _thumbHalf)), Math.Max(1, Height - 6));

        private int ValueToX(int value)
        {
            Rectangle track = TrackRect;
            double t = _maximum == _minimum ? 0 : (value - _minimum) / (double)(_maximum - _minimum);
            return track.Left + (int)Math.Round(t * (track.Width - 1));
        }

        private int XToValue(int x)
        {
            Rectangle track = TrackRect;
            double t = Math.Clamp((x - track.Left) / (double)Math.Max(1, track.Width - 1), 0, 1);
            return _minimum + (int)Math.Round(t * (_maximum - _minimum));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            Rectangle track = TrackRect;

            for (int x = 0; x < track.Width; x += 2)
            {
                double t = track.Width <= 1 ? 0 : x / (double)(track.Width - 1);
                using var brush = new SolidBrush(_track(t));
                g.FillRectangle(brush, track.Left + x, track.Top, 2, track.Height);
            }

            g.DrawRectangle(SystemPens.FromSystemColor(Focused ? SystemColors.Highlight : SystemColors.ControlDark), track.Left - 1, track.Top - 1, track.Width + 1, track.Height + 1);

            int thumb = ValueToX(_value);
            var outer = new Rectangle(thumb - 3, 0, 6, Height - 1);
            g.FillRectangle(Brushes.White, outer);
            g.DrawRectangle(Pens.Black, outer);
            g.DrawLine(Pens.Black, thumb, 2, thumb, Height - 3);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            Focus();
            _dragging = true;
            SetFromUser(XToValue(e.X), commit: false);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_dragging)
            {
                SetFromUser(XToValue(e.X), commit: false);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (_dragging)
            {
                _dragging = false;
                ValueCommitted?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int step = (ModifierKeys & Keys.Shift) != 0 ? 4 : 1;
            SetFromUser(_value + (Math.Sign(e.Delta) * step), commit: true);
            if (e is HandledMouseEventArgs handled)
            {
                handled.Handled = true;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return (keyData & Keys.KeyCode) is Keys.Left or Keys.Right or Keys.Up or Keys.Down or Keys.Home or Keys.End
                   || base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            int step = e.Shift ? 4 : 1;
            int? target = e.KeyCode switch
            {
                Keys.Left or Keys.Down => _value - step,
                Keys.Right or Keys.Up => _value + step,
                Keys.Home => _minimum,
                Keys.End => _maximum,
                _ => null,
            };

            if (target.HasValue)
            {
                SetFromUser(target.Value, commit: true);
                e.Handled = true;
            }
        }

        private void SetFromUser(int value, bool commit)
        {
            value = Math.Clamp(value, _minimum, _maximum);
            if (value != _value)
            {
                _value = value;
                Invalidate();
                ValueChanging?.Invoke(this, EventArgs.Empty);
            }

            if (commit)
            {
                ValueCommitted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
