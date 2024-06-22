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
            //Art.Measure(bitmap, out int xMin, out int yMin, out int xMax, out int yMax);
            Center = new Point(0, 0); // new Point(bitmap.Width / 2, bitmap.Height / 2 - (yMax - yMin) / 2); //  (xMax - xMin) / 2, );
        }

    }

    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    internal partial class AnimatedPictureBox : PictureBox
    {
        private List<AnimatedFrame> _frames;
        private int _frameIndex;
        private Timer _timer;
        private bool _animate;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<AnimatedFrame> Frames
        {
            get => _frames;
            set
            {
                _frameIndex = 0;
                _frames = value;

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
            get => _frames?[_frameIndex];
        }
        public int FrameIndex
        {
            get => _frameIndex;
            set
            {
                if (_frameIndex != value)
                {
                    _frameIndex = value % Math.Max(_frames?.Count ?? 1, 1);
                    Invalidate();
                }
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
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_frames.Count > 0)
            {
                _frameIndex = (_frameIndex + 1) % _frames.Count;
                Invalidate(); // Force a repaint
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            AnimatedFrame frame = _frameIndex <_frames?.Count ? _frames[_frameIndex] : null;
            if (frame != null)
            {
                var location = new Point((Width / 2) - frame.Center.X, (Height / 2) - frame.Center.Y - frame.Bitmap.Height);

#if DEBUG
                e.Graphics.DrawRectangle(new Pen(Color.Red), new Rectangle(location, frame.Bitmap.Size));
#endif

                e.Graphics.DrawImage(frame.Bitmap, location);
                
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
