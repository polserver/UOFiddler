//	Windows Forms Collapsible Splitter Control for .Net
//	(c)Copyright 2002-2003 NJF (furty74@yahoo.com). All rights reserved.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls
{
    /// <summary>
    /// Enumeration to specify the visual style to be applied to the CollapsibleSplitter control
    /// </summary>
    public enum VisualStyles
    {
        Mozilla = 0,
        XP,
        Win9X,
        DoubleDots,
        Lines
    }

    /// <summary>
    /// Enumeration to specify the current animation state of the control.
    /// </summary>
    public enum SplitterState
    {
        Collapsed = 0,
        Expanding,
        Expanded,
        Collapsing
    }

    /// <summary>
    /// A custom collapsible splitter that can resize, hide and show associated form controls
    /// </summary>
    [Designer(typeof(CollapsibleSplitterDesigner))]
    public partial class CollapsibleSplitter : Splitter
    {
        // declare and define some base properties
        private bool _hot;
        private readonly Color _hotColor = CalculateColor(SystemColors.Highlight, SystemColors.Window, 70);
        private Rectangle _rr;
        private Form _parentForm;
        private VisualStyles _visualStyle;

        // Border added in version 1.3
        private Border3DStyle _borderStyle = Border3DStyle.Flat;

        // animation controls introduced in version 1.22
        private readonly Timer _animationTimer;
        private int _controlWidth;
        private int _controlHeight;
        private int _parentFormWidth;
        private int _parentFormHeight;
        private SplitterState _currentState;
        private bool _useAnimations;

        /// <summary>
        /// The initial state of the Splitter. Set to True if the control to hide is not visible by default
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("False"),
        Description("The initial state of the Splitter. Set to True if the control to hide is not visible by default")]
        public bool IsCollapsed
        {
            get
            {
                if (ControlToHide != null)
                {
                    return !ControlToHide.Visible;
                }

                return true;
            }
        }

        /// <summary>
        /// The System.Windows.Forms.Control that the splitter will collapse
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue(""),
        Description("The System.Windows.Forms.Control that the splitter will collapse")]
        public Control ControlToHide { get; set; }

        /// <summary>
        /// Determines if the collapse and expanding actions will be animated
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("True"),
        Description("Determines if the collapse and expanding actions will be animated")]
        public bool UseAnimations
        {
            get => _useAnimations;
            set => _useAnimations = value;
        }

        /// <summary>
        /// The delay in milliseconds between animation steps
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("20"),
        Description("The delay in millisenconds between animation steps")]
        public int AnimationDelay
        {
            get => _animationTimer.Interval;
            set => _animationTimer.Interval = value;
        }

        /// <summary>
        /// The amount of pixels moved in each animation step
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("20"),
        Description("The amount of pixels moved in each animation step")]
        public int AnimationStep { get; set; } = 20;

        /// <summary>
        /// When true the entire parent form will be expanded and collapsed, otherwise just the contol to expand will be changed
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("False"),
        Description("When true the entire parent form will be expanded and collapsed, otherwise just the contol to expand will be changed")]
        public bool ExpandParentForm { get; set; }

        /// <summary>
        /// The visual style that will be painted on the control
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("VisualStyles.XP"),
        Description("The visual style that will be painted on the control")]
        public VisualStyles VisualStyle
        {
            get => _visualStyle;
            set
            {
                _visualStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// An optional border style to paint on the control. Set to Flat for no border
        /// </summary>
        [Bindable(true), Category("Collapsing Options"), DefaultValue("System.Windows.Forms.Border3DStyle.Flat"),
        Description("An optional border style to paint on the control. Set to Flat for no border")]
        public Border3DStyle BorderStyle3D
        {
            get => _borderStyle;
            set
            {
                _borderStyle = value;
                Invalidate();
            }
        }

        public void ToggleState()
        {
            ToggleSplitter();
        }

        public CollapsibleSplitter()
        {
            // Register mouse events
            Click += OnClick;
            Resize += OnResize;
            MouseLeave += OnMouseLeave;
            MouseMove += OnMouseMove;

            // Setup the animation timer control
            _animationTimer = new Timer
            {
                Interval = 20
            };
            _animationTimer.Tick += AnimationTimerTick;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _parentForm = FindForm();

            // set the current state
            if (ControlToHide != null)
            {
                _currentState = ControlToHide.Visible ? SplitterState.Expanded : SplitterState.Collapsed;
            }
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // if the hider control isn't hot, let the base resize action occur
            if (ControlToHide == null)
            {
                return;
            }

            if (!_hot && ControlToHide.Visible)
            {
                base.OnMouseDown(e);
            }
        }

        private void OnResize(object sender, EventArgs e)
        {
            Invalidate();
        }

        // this method was updated in version 1.11 to fix a flickering problem
        // discovered by John O'Byrne
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            // check to see if the mouse cursor position is within the bounds of our control
            if (e.X >= _rr.X && e.X <= _rr.X + _rr.Width && e.Y >= _rr.Y && e.Y <= _rr.Y + _rr.Height)
            {
                if (!_hot)
                {
                    _hot = true;
                    Cursor = Cursors.Hand;
                    Invalidate();
                }
            }
            else
            {
                if (_hot)
                {
                    _hot = false;
                    Invalidate();
                }

                Cursor = Cursors.Default;

                if (ControlToHide == null)
                {
                    return;
                }

                if (!ControlToHide.Visible)
                {
                    Cursor = Cursors.Default;
                }
                else // Changed in v1.2 to support Horizontal Splitters
                {
                    Cursor = Dock == DockStyle.Left || Dock == DockStyle.Right ? Cursors.VSplit : Cursors.HSplit;
                }
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            // ensure that the hot state is removed
            _hot = false;
            Invalidate();
        }

        private void OnClick(object sender, EventArgs e)
        {
            if (ControlToHide != null && _hot &&
                _currentState != SplitterState.Collapsing &&
                _currentState != SplitterState.Expanding)
            {
                ToggleSplitter();
            }
        }

        private void ToggleSplitter()
        {
            // if an animation is currently in progress for this control, drop out
            if (_currentState == SplitterState.Collapsing || _currentState == SplitterState.Expanding)
            {
                return;
            }

            _controlWidth = ControlToHide.Width;
            _controlHeight = ControlToHide.Height;

            if (ControlToHide.Visible)
            {
                if (_useAnimations)
                {
                    _currentState = SplitterState.Collapsing;

                    if (_parentForm != null)
                    {
                        if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                        {
                            _parentFormWidth = _parentForm.Width - _controlWidth;
                        }
                        else
                        {
                            _parentFormHeight = _parentForm.Height - _controlHeight;
                        }
                    }

                    _animationTimer.Enabled = true;
                }
                else
                {
                    // no animations, so just toggle the visible state
                    _currentState = SplitterState.Collapsed;
                    ControlToHide.Visible = false;
                    if (!ExpandParentForm || _parentForm == null)
                    {
                        return;
                    }

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        _parentForm.Width -= ControlToHide.Width;
                    }
                    else
                    {
                        _parentForm.Height -= ControlToHide.Height;
                    }
                }
            }
            else
            {
                // control to hide is collapsed
                if (_useAnimations)
                {
                    _currentState = SplitterState.Expanding;

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        if (_parentForm != null)
                        {
                            _parentFormWidth = _parentForm.Width + _controlWidth;
                        }
                        ControlToHide.Width = 0;
                    }
                    else
                    {
                        if (_parentForm != null)
                        {
                            _parentFormHeight = _parentForm.Height + _controlHeight;
                        }
                        ControlToHide.Height = 0;
                    }
                    ControlToHide.Visible = true;
                    _animationTimer.Enabled = true;
                }
                else
                {
                    // no animations, so just toggle the visible state
                    _currentState = SplitterState.Expanded;
                    ControlToHide.Visible = true;
                    if (!ExpandParentForm || _parentForm == null)
                    {
                        return;
                    }

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        _parentForm.Width += ControlToHide.Width;
                    }
                    else
                    {
                        _parentForm.Height += ControlToHide.Height;
                    }
                }
            }
        }

        private void AnimationTimerTick(object sender, EventArgs e)
        {
            switch (_currentState)
            {
                case SplitterState.Collapsing:

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        // vertical splitter
                        if (ControlToHide.Width > AnimationStep)
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Width -= AnimationStep;
                            }
                            ControlToHide.Width -= AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Width = _parentFormWidth;
                            }
                            ControlToHide.Visible = false;
                            _animationTimer.Enabled = false;
                            ControlToHide.Width = _controlWidth;
                            _currentState = SplitterState.Collapsed;
                            Invalidate();
                        }
                    }
                    else
                    {
                        // horizontal splitter
                        if (ControlToHide.Height > AnimationStep)
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Height -= AnimationStep;
                            }
                            ControlToHide.Height -= AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Height = _parentFormHeight;
                            }
                            ControlToHide.Visible = false;
                            _animationTimer.Enabled = false;
                            ControlToHide.Height = _controlHeight;
                            _currentState = SplitterState.Collapsed;
                            Invalidate();
                        }
                    }
                    break;

                case SplitterState.Expanding:

                    if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                    {
                        // vertical splitter
                        if (ControlToHide.Width < _controlWidth - AnimationStep)
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Width += AnimationStep;
                            }
                            ControlToHide.Width += AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Width = _parentFormWidth;
                            }
                            ControlToHide.Width = _controlWidth;
                            ControlToHide.Visible = true;
                            _animationTimer.Enabled = false;
                            _currentState = SplitterState.Expanded;
                            Invalidate();
                        }
                    }
                    else
                    {
                        // horizontal splitter
                        if (ControlToHide.Height < _controlHeight - AnimationStep)
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Height += AnimationStep;
                            }
                            ControlToHide.Height += AnimationStep;
                        }
                        else
                        {
                            if (ExpandParentForm && _parentForm.WindowState != FormWindowState.Maximized
                                && _parentForm != null)
                            {
                                _parentForm.Height = _parentFormHeight;
                            }
                            ControlToHide.Height = _controlHeight;
                            ControlToHide.Visible = true;
                            _animationTimer.Enabled = false;
                            _currentState = SplitterState.Expanded;
                            Invalidate();
                        }
                    }
                    break;
                case SplitterState.Collapsed:
                    break;
                case SplitterState.Expanded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // OnPaint is now an override rather than an event in version 1.1
        protected override void OnPaint(PaintEventArgs e)
        {
            // create a Graphics object
            using (Graphics g = e.Graphics)
            {
                Pen penControlLightLight = new Pen(SystemColors.ControlLightLight);
                Pen penControlLight = new Pen(SystemColors.ControlLight);
                Pen penControlDark = new Pen(SystemColors.ControlDark);
                Pen penBackColor = new Pen(BackColor);
                Pen penHotColor = new Pen(_hotColor);
                Pen penControlDarkDark = new Pen(SystemColors.ControlDarkDark);

                Brush brushControlDark = new SolidBrush(SystemColors.ControlDark);
                Brush brushControlDarkDark = new SolidBrush(SystemColors.ControlDarkDark);
                Brush brushBackColor = new SolidBrush(BackColor);
                Brush brushHotColor = new SolidBrush(_hotColor);

                // find the rectangle for the splitter and paint it
                Rectangle r = ClientRectangle; // fixed in version 1.1
                g.FillRectangle(brushBackColor, r);

                // Check the docking style and create the control rectangle accordingly
                if (Dock == DockStyle.Left || Dock == DockStyle.Right)
                {
                    // create a new rectangle in the vertical center of the splitter for our collapse control button
                    _rr = new Rectangle(r.X, r.Y + ((r.Height - 115) / 2), 8, 115);
                    // force the width to 8px so that everything always draws correctly
                    Width = 8;

                    // draw the background color for our control image
                    if (_hot)
                    {
                        g.FillRectangle(brushHotColor, new Rectangle(_rr.X + 1, _rr.Y, 6, 115));
                    }
                    else
                    {
                        g.FillRectangle(brushBackColor, new Rectangle(_rr.X + 1, _rr.Y, 6, 115));
                    }

                    // draw the top & bottom lines for our control image
                    using (Pen pn = new Pen(SystemColors.ControlDark, 1))
                    {
                        g.DrawLine(pn, _rr.X + 1, _rr.Y, _rr.X + _rr.Width - 2, _rr.Y);
                        g.DrawLine(pn, _rr.X + 1, _rr.Y + _rr.Height, _rr.X + _rr.Width - 2, _rr.Y + _rr.Height);
                    }

                    if (Enabled)
                    {
                        // draw the arrows for our control image
                        // the ArrowPointArray is a point array that defines an arrow shaped polygon
                        g.FillPolygon(brushControlDarkDark, ArrowPointArray(_rr.X + 2, _rr.Y + 3));
                        g.FillPolygon(brushControlDarkDark, ArrowPointArray(_rr.X + 2, _rr.Y + _rr.Height - 9));
                    }

                    // draw the dots for our control image using a loop
                    int x = _rr.X + 3;
                    int y = _rr.Y + 14;

                    // Visual Styles added in version 1.1
                    switch (_visualStyle)
                    {
                        case VisualStyles.Mozilla:

                            for (int i = 0; i < 30; ++i)
                            {
                                // light dot
                                g.DrawLine(penControlLightLight, x, y + (i * 3), x + 1, y + 1 + (i * 3));
                                // dark dot
                                g.DrawLine(penControlDarkDark, x + 1, y + 1 + (i * 3), x + 2, y + 2 + (i * 3));
                                // overdraw the background color as we actually drew 2px diagonal lines, not just dots
                                if (_hot)
                                {
                                    g.DrawLine(penHotColor, x + 2, y + 1 + (i * 3), x + 2, y + 2 + (i * 3));
                                }
                                else
                                {
                                    g.DrawLine(penBackColor, x + 2, y + 1 + (i * 3), x + 2, y + 2 + (i * 3));
                                }
                            }
                            break;

                        case VisualStyles.DoubleDots:
                            for (int i = 0; i < 30; ++i)
                            {
                                // light dot
                                g.DrawRectangle(penControlLightLight, x, y + 1 + (i * 3), 1, 1);
                                // dark dot
                                g.DrawRectangle(penControlDark, x - 1, y + (i * 3), 1, 1);
                                ++i;
                                // light dot
                                g.DrawRectangle(penControlLightLight, x + 2, y + 1 + (i * 3), 1, 1);
                                // dark dot
                                g.DrawRectangle(penControlDark, x + 1, y + (i * 3), 1, 1);
                            }
                            break;

                        case VisualStyles.Win9X:

                            g.DrawLine(penControlLightLight, x, y, x + 2, y);
                            g.DrawLine(penControlLightLight, x, y, x, y + 90);
                            g.DrawLine(penControlDark, x + 2, y, x + 2, y + 90);
                            g.DrawLine(penControlDark, x, y + 90, x + 2, y + 90);
                            break;

                        case VisualStyles.XP:

                            for (int i = 0; i < 18; ++i)
                            {
                                // light dot
                                g.DrawRectangle(penControlLight, x, y + (i * 5), 2, 2);
                                // light light dot
                                g.DrawRectangle(penControlLightLight, x + 1, y + 1 + (i * 5), 1, 1);
                                // dark dark dot
                                g.DrawRectangle(penControlDarkDark, x, y + (i * 5), 1, 1);
                                // dark fill
                                g.DrawLine(penControlDark, x, y + (i * 5), x, y + (i * 5) + 1);
                                g.DrawLine(penControlDark, x, y + (i * 5), x + 1, y + (i * 5));
                            }
                            break;

                        case VisualStyles.Lines:

                            for (int i = 0; i < 44; ++i)
                            {
                                g.DrawLine(penControlDark, x, y + (i * 2), x + 2, y + (i * 2));
                            }

                            break;
                    }

                    // Added in version 1.3
                    if (_borderStyle != Border3DStyle.Flat)
                    {
                        // Paint the control border
                        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Left);
                        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Right);
                    }
                }

                // Horizontal Splitter support added in v1.2

                else if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
                {
                    // create a new rectangle in the horizontal center of the splitter for our collapse control button
                    _rr = new Rectangle(r.X + ((r.Width - 115) / 2), r.Y, 115, 8);
                    // force the height to 8px
                    Height = 8;

                    // draw the background color for our control image
                    if (_hot)
                    {
                        g.FillRectangle(brushHotColor, new Rectangle(_rr.X, _rr.Y + 1, 115, 6));
                    }
                    else
                    {
                        g.FillRectangle(brushBackColor, new Rectangle(_rr.X, _rr.Y + 1, 115, 6));
                    }

                    // draw the left & right lines for our control image
                    using (Pen pn = new Pen(SystemColors.ControlDark, 1))
                    {
                        g.DrawLine(pn, _rr.X, _rr.Y + 1, _rr.X, _rr.Y + _rr.Height - 2);
                        g.DrawLine(pn, _rr.X + _rr.Width, _rr.Y + 1, _rr.X + _rr.Width, _rr.Y + _rr.Height - 2);
                    }
                    if (Enabled)
                    {
                        // draw the arrows for our control image
                        // the ArrowPointArray is a point array that defines an arrow shaped polygon
                        g.FillPolygon(brushControlDarkDark, ArrowPointArray(_rr.X + 3, _rr.Y + 2));
                        g.FillPolygon(brushControlDarkDark, ArrowPointArray(_rr.X + _rr.Width - 9, _rr.Y + 2));
                    }

                    // draw the dots for our control image using a loop
                    int x = _rr.X + 14;
                    int y = _rr.Y + 3;

                    // Visual Styles added in version 1.1
                    switch (_visualStyle)
                    {
                        case VisualStyles.Mozilla:

                            for (int i = 0; i < 30; ++i)
                            {
                                // light dot
                                g.DrawLine(penControlLightLight, x + (i * 3), y, x + 1 + (i * 3), y + 1);
                                // dark dot
                                g.DrawLine(penControlDarkDark, x + 1 + (i * 3), y + 1, x + 2 + (i * 3), y + 2);
                                // overdraw the background color as we actually drew 2px diagonal lines, not just dots
                                if (_hot)
                                {
                                    g.DrawLine(penHotColor, x + 1 + (i * 3), y + 2, x + 2 + (i * 3), y + 2);
                                }
                                else
                                {
                                    g.DrawLine(penBackColor, x + 1 + (i * 3), y + 2, x + 2 + (i * 3), y + 2);
                                }
                            }
                            break;

                        case VisualStyles.DoubleDots:

                            for (int i = 0; i < 30; ++i)
                            {
                                // light dot
                                g.DrawRectangle(penControlLightLight, x + 1 + (i * 3), y, 1, 1);
                                // dark dot
                                g.DrawRectangle(penControlDark, x + (i * 3), y - 1, 1, 1);
                                ++i;
                                // light dot
                                g.DrawRectangle(penControlLightLight, x + 1 + (i * 3), y + 2, 1, 1);
                                // dark dot
                                g.DrawRectangle(penControlDark, x + (i * 3), y + 1, 1, 1);
                            }
                            break;

                        case VisualStyles.Win9X:

                            g.DrawLine(penControlLightLight, x, y, x, y + 2);
                            g.DrawLine(penControlLightLight, x, y, x + 88, y);
                            g.DrawLine(penControlDark, x, y + 2, x + 88, y + 2);
                            g.DrawLine(penControlDark, x + 88, y, x + 88, y + 2);
                            break;

                        case VisualStyles.XP:

                            for (int i = 0; i < 18; ++i)
                            {
                                // light dot
                                g.DrawRectangle(penControlLight, x + (i * 5), y, 2, 2);
                                // light light dot
                                g.DrawRectangle(penControlLightLight, x + 1 + (i * 5), y + 1, 1, 1);
                                // dark dark dot
                                g.DrawRectangle(penControlDarkDark, x + (i * 5), y, 1, 1);
                                // dark fill
                                g.DrawLine(penControlDark, x + (i * 5), y, x + (i * 5) + 1, y);
                                g.DrawLine(penControlDark, x + (i * 5), y, x + (i * 5), y + 1);
                            }
                            break;

                        case VisualStyles.Lines:

                            for (int i = 0; i < 44; ++i)
                            {
                                g.DrawLine(penControlDark, x + (i * 2), y, x + (i * 2), y + 2);
                            }

                            break;
                    }

                    // Added in version 1.3
                    if (_borderStyle != Border3DStyle.Flat)
                    {
                        // Paint the control border
                        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Top);
                        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, _borderStyle, Border3DSide.Bottom);
                    }
                }
                else
                {
                    throw new Exception("The Collapsible Splitter control cannot have the Filled or None Dockstyle property");
                }

                // dispose the Graphics objects
                penControlLightLight.Dispose();
                penControlLight.Dispose();
                penControlDark.Dispose();
                penBackColor.Dispose();
                penHotColor.Dispose();
                penControlDarkDark.Dispose();

                brushControlDark.Dispose();
                brushControlDarkDark.Dispose();
                brushBackColor.Dispose();
                brushHotColor.Dispose();
            }
        }

        // This creates a point array to draw a arrow-like polygon
        private Point[] ArrowPointArray(int x, int y)
        {
            Point[] point = new Point[3];

            if (ControlToHide != null)
            {
                // decide which direction the arrow will point
                if (
                    (Dock == DockStyle.Right && ControlToHide.Visible)
                    || (Dock == DockStyle.Left && !ControlToHide.Visible)
                    )
                {
                    // right arrow
                    point[0] = new Point(x, y);
                    point[1] = new Point(x + 3, y + 3);
                    point[2] = new Point(x, y + 6);
                }
                else if (
                    (Dock == DockStyle.Right && !ControlToHide.Visible)
                    || (Dock == DockStyle.Left && ControlToHide.Visible)
                    )
                {
                    // left arrow
                    point[0] = new Point(x + 3, y);
                    point[1] = new Point(x, y + 3);
                    point[2] = new Point(x + 3, y + 6);
                }

                // Up/Down arrows added in v1.2

                else if (
                    (Dock == DockStyle.Top && ControlToHide.Visible)
                    || (Dock == DockStyle.Bottom && !ControlToHide.Visible)
                    )
                {
                    // up arrow
                    point[0] = new Point(x + 3, y);
                    point[1] = new Point(x + 6, y + 4);
                    point[2] = new Point(x, y + 4);
                }
                else if (
                    (Dock == DockStyle.Top && !ControlToHide.Visible)
                    || (Dock == DockStyle.Bottom && ControlToHide.Visible)
                    )
                {
                    // down arrow
                    point[0] = new Point(x, y);
                    point[1] = new Point(x + 6, y);
                    point[2] = new Point(x + 3, y + 3);
                }
            }

            return point;
        }

        // this method was borrowed from the RichUI Control library by Sajith M
        private static Color CalculateColor(Color front, Color back, int alpha)
        {
            // solid color obtained as a result of alpha-blending

            Color frontColor = Color.FromArgb(255, front);
            Color backColor = Color.FromArgb(255, back);

            float frontRed = frontColor.R;
            float frontGreen = frontColor.G;
            float frontBlue = frontColor.B;
            float backRed = backColor.R;
            float backGreen = backColor.G;
            float backBlue = backColor.B;

            float fRed = (frontRed * alpha / 255) + (backRed * ((float)(255 - alpha) / 255));
            byte newRed = (byte)fRed;
            float fGreen = (frontGreen * alpha / 255) + (backGreen * ((float)(255 - alpha) / 255));
            byte newGreen = (byte)fGreen;
            float fBlue = (frontBlue * alpha / 255) + (backBlue * ((float)(255 - alpha) / 255));
            byte newBlue = (byte)fBlue;

            return Color.FromArgb(255, newRed, newGreen, newBlue);
        }
    }

    /// <summary>
    /// A simple designer class for the CollapsibleSplitter control to remove
    /// unwanted properties at design time.
    /// </summary>
    public class CollapsibleSplitterDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        protected override void PreFilterProperties(System.Collections.IDictionary properties)
        {
            properties.Remove("IsCollapsed");
            properties.Remove("BorderStyle");
            properties.Remove("Size");
        }
    }
}
