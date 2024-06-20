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
using System.Windows.Forms;
using Ultima;

namespace UoFiddler.Controls.UserControls
{
    public sealed class AnimatedFrame
    {
        public Point Center { get; set; }
        public Bitmap Bitmap { get; set; }

        public AnimatedFrame(Bitmap bitmap, Point center)
        {
            Bitmap = bitmap;
            Center = center;
        }

        public AnimatedFrame(Bitmap bitmap)
        {
            Bitmap = new Bitmap(bitmap);
            Art.Measure(bitmap, out int xMin, out int yMin, out int xMax, out int yMax);
            Center = new Point((xMax - xMin) / 2, (yMax - yMin) / 2);
        }

    }

    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    internal partial class AnimatedPictureBox : PictureBox
    {
        private List<AnimatedFrame> _frames;
        private int _currentFrame;
        private Timer _timer;
        private bool _animate;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<AnimatedFrame> Frames
        {
            get => _frames;
            set
            {
                _frames = value;
                _currentFrame = 0;

                if (_animate && _frames.Count != 0 && !_timer.Enabled)
                {
                    _timer.Start();
                }

                Invalidate(); // Force a repaint
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimatedFrame FirstFrame
        {
            get => _frames?[0];
        }

        public AnimatedFrame CurrentFrame
        {
            get => _frames?[_currentFrame];
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
            _currentFrame = 0;
            _timer = new Timer
            {
                Interval = 150,
                Enabled = false
            };

            _timer.Tick += Timer_Tick;
            DoubleBuffered = true; // To reduce flicker
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_frames.Count > 0)
            {
                _currentFrame = (_currentFrame + 1) % _frames.Count;
                Invalidate(); // Force a repaint
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_frames.Count > 0 && _frames[_currentFrame] != null)
            {
                Point location = Point.Empty;
                Size size = _frames[_currentFrame].Bitmap.Size;
                location.X = (Width / 2) - _frames[_currentFrame].Center.X;
                location.Y = (Height / 2) - _frames[_currentFrame].Center.Y - _frames[_currentFrame].Bitmap.Height;

                var destRect = new Rectangle(location, size);

                e.Graphics.DrawImage(_frames[_currentFrame].Bitmap, destRect, 0, 0, _frames[_currentFrame].Bitmap.Width, _frames[_currentFrame].Bitmap.Height, GraphicsUnit.Pixel);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
                foreach (var frame in _frames)
                {
                    frame.Bitmap?.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
