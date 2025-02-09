// /***************************************************************************
//  *
//  * $Author: Turley
//  * 
//  * "THE BEER-WARE LICENSE"
//  * As long as you retain this notice you can do whatever you want with 
//  * this stuff. If we meet some day, and you think this stuff is worth it,
//  * you can buy me a beer in return.
//  *
//  ***************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.UserControls
{
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public class AnimatedPictureBox : PictureBox
    {
        private List<AnimatedFrame> _frames;
        private int _frameIndex;
        private readonly Timer _timer;
        private bool _animate;
        private bool _showFrameBounds;
        private Size _animationSize;
        private Point _drawCenter;
        private Point _draggedOffset = new(0, 0);
        private Point _mouseDownLocation;
        private bool _isDragging;

        public event EventHandler FrameChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<AnimatedFrame> Frames
        {
            get => _frames;
            set
            {
                _frameIndex = 0;
                _frames = value ?? [];

                if (_animate && _frames.Count != 0 && !_timer.Enabled)
                {
                    _timer.Start();
                }

                (_drawCenter, _animationSize) = _frames.GetAnimationDetails();

                FrameChanged?.Invoke(this, EventArgs.Empty);
                Invalidate(); // Force a repaint
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedFrame FirstFrame
        {
            get => _frames?.FirstOrDefault();
        }

        public AnimatedFrame CurrentFrame
        {
            get => _frames?[_frameIndex];
        }

        public int FrameIndex
        {
            get => _frameIndex;
            set
            {
                var newValue = value % Math.Max(_frames?.Count ?? 1, 1);
                if (_frameIndex == newValue)
                {
                    return;
                }

                _frameIndex = newValue;
                FrameChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        public bool ShowFrameBounds
        {
            get => _showFrameBounds;
            set
            {
                _showFrameBounds = value;
                Invalidate();
            }
        }

        public int FrameDelay
        {
            get => _timer.Interval;
            set
            {
                if (_timer.Interval != value)
                {
                    bool restart = _timer.Enabled;

                    if (restart)
                    {
                        _timer.Stop();
                    }

                    _timer.Interval = value;

                    if (restart)
                    {
                        _timer.Start();
                    }
                }
            }
        }

        public bool Animate
        {
            get => _animate;
            set
            {
                if (!value && _timer.Enabled)
                {
                    _timer.Stop();
                }
                else if (value && _frames.Count != 0 && !_timer.Enabled)
                {
                    _timer.Start();
                }

                _animate = value;
            }
        }

        public AnimatedPictureBox()
        {
            _frames = [];
            _frameIndex = 0;
            _timer = new Timer
            {
                Interval = 150,
                Enabled = false
            };

            _timer.Tick += Timer_Tick;
            DoubleBuffered = true; // To reduce flicker
            MouseDown += OnMouseDown;
            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            _mouseDownLocation = e.Location;
            _isDragging = true;
            Cursor = Cursors.SizeAll; // Change cursor to dragging cursor
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            // Calculate the new center point based on the mouse movement
            _draggedOffset.X += e.X - _mouseDownLocation.X;
            _draggedOffset.Y += e.Y - _mouseDownLocation.Y;

            // Update the mouse down location to the new location
            _mouseDownLocation = e.Location;

            // Refresh the PictureBox to trigger the Paint event
            Invalidate();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;
            Cursor = Cursors.Default; // Change cursor back to default
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_frames.Count == 0)
            {
                return;
            }

            _frameIndex = (_frameIndex + 1) % _frames.Count;
            FrameChanged?.Invoke(this, EventArgs.Empty);
            Invalidate(); // Force a repaint
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            AnimatedFrame frame = _frameIndex < _frames?.Count ? _frames[_frameIndex] : null;
            if (frame == null)
            {
                return;
            }

            var location = new Point(
                _drawCenter.X - frame.Center.X + (Width - _animationSize.Width) / 2 + _draggedOffset.X,
                _drawCenter.Y - frame.Center.Y - frame.Bitmap.Height + (Height - _animationSize.Height) / 2 + _draggedOffset.Y
            );

            e.Graphics.DrawImage(frame.Bitmap, location);

            if (_showFrameBounds)
            {
                e.Graphics.DrawRectangle(new Pen(Color.Red), new Rectangle(location, frame.Bitmap.Size));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();

                if (_frames != null)
                {
                    foreach (var frame in _frames)
                    {
                        frame.Bitmap?.Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }

        public void Reset()
        {
            _frameIndex = 0;

            if (_frames != null)
            {
                foreach (var frame in _frames)
                {
                    frame.Bitmap?.Dispose();
                }
            }

            _frames = [];
            _timer.Stop();
            _animate = false;
            _showFrameBounds = false;

            Invalidate();
        }
    }
}
