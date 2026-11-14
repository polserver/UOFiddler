/***************************************************************************
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using UoFiddler.Controls.Classes;
using UoFiddler.Plugin.MultiEditor.Rendering;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>
    /// The performance box over the canvas: the last second's timings as text, and a line chart of
    /// frame, GPU, draw and lag times over the last <see cref="FrameStats.HistorySeconds"/> seconds.
    /// </summary>
    internal sealed class PerformancePanel : Control
    {
        // Colours for the application's light or dark mode, chosen once (the mode is set at startup).
        private readonly Color _back;
        private readonly Color _text;
        private readonly Color _axis;
        private readonly (string Name, Color Color, Func<FrameStats.Point, double> Value)[] _series;

        private readonly Timer _timer = new Timer { Interval = 250 };
        private FrameStats _stats;
        private Func<int> _tiles;
        private string _summary = string.Empty;

        public PerformancePanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.Selectable, false);

            bool dark = Options.DarkMode;
            _back = dark ? Color.FromArgb(24, 24, 28) : Color.FromArgb(248, 248, 250);
            _text = dark ? Color.FromArgb(220, 220, 220) : Color.FromArgb(30, 30, 30);
            _axis = dark ? Color.FromArgb(70, 70, 78) : Color.FromArgb(165, 165, 172);
            _series = new (string, Color, Func<FrameStats.Point, double>)[]
            {
                ("Frame", dark ? Color.WhiteSmoke : Color.FromArgb(40, 40, 40), p => p.Frame),
                ("GPU", dark ? Color.Orange : Color.DarkOrange, p => p.Gpu),
                ("Draw", dark ? Color.DeepSkyBlue : Color.RoyalBlue, p => p.Draw),
                ("Lag", dark ? Color.HotPink : Color.MediumVioletRed, p => p.Lag)
            };

            Font = new Font(FontFamily.GenericMonospace, 8.25f);
            BackColor = _back;
            ForeColor = _text;
            _timer.Tick += (_, _) => UpdateNumbers();
        }

        /// <summary>Where the numbers come from, and the tile count of the open design.</summary>
        public void Bind(FrameStats stats, Func<int> tiles)
        {
            _stats = stats;
            _tiles = tiles;
            UpdateNumbers();
        }

        /// <summary>Reads the numbers again and repaints; the box sizes itself to the text.</summary>
        public void UpdateNumbers()
        {
            _summary = _stats?.Describe(_tiles?.Invoke() ?? 0) ?? string.Empty;
            Size text = TextRenderer.MeasureText(_summary, Font);
            int width = Math.Max(text.Width, LogicalToDeviceUnits(400)) + LogicalToDeviceUnits(16);
            int height = text.Height + LogicalToDeviceUnits(16 + 130);
            if (Width != width || Height != height)
            {
                Size = new Size(width, height);
            }

            Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            _timer.Enabled = Visible;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(_back);
            int pad = LogicalToDeviceUnits(8);
            TextRenderer.DrawText(g, _summary, Font, new Point(pad, pad), _text, TextFormatFlags.NoPrefix);
            Size text = TextRenderer.MeasureText(_summary, Font);

            var chart = new Rectangle(pad + LogicalToDeviceUnits(34), pad + text.Height + LogicalToDeviceUnits(8),
                Width - (pad * 2) - LogicalToDeviceUnits(34), Height - text.Height - (pad * 2) - LogicalToDeviceUnits(30));
            if (chart.Width <= 10 || chart.Height <= 10 || _stats == null)
            {
                return;
            }

            IReadOnlyList<FrameStats.Point> history = _stats.History;

            // Scale to the largest value shown, at least one 60 Hz frame, in whole 5 ms steps.
            double max = 16.7;
            foreach (FrameStats.Point p in history)
            {
                foreach ((_, _, Func<FrameStats.Point, double> value) in _series)
                {
                    max = Math.Max(max, value(p));
                }
            }

            max = Math.Ceiling(max / 5) * 5;
            float Y(double ms) => chart.Bottom - (float)(Math.Min(ms, max) / max * chart.Height);
            float X(int index) => chart.Left + (index * chart.Width / (float)(FrameStats.HistorySeconds - 1));

            using (var axis = new Pen(_axis))
            {
                g.DrawRectangle(axis, chart);

                // The frame budgets of 120 Hz and 60 Hz displays, labelled on the axis.
                axis.DashStyle = DashStyle.Dash;
                foreach (double budget in new[] { 1000 / 120.0, 1000 / 60.0 })
                {
                    g.DrawLine(axis, chart.Left, Y(budget), chart.Right, Y(budget));
                    TextRenderer.DrawText(g, $"{budget:F1}", Font, new Point(pad, (int)Y(budget) - LogicalToDeviceUnits(6)), _axis, TextFormatFlags.NoPrefix);
                }
            }

            TextRenderer.DrawText(g, $"{max:F0}", Font, new Point(pad, chart.Top - LogicalToDeviceUnits(4)), _axis, TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, "0 ms", Font, new Point(pad, chart.Bottom - LogicalToDeviceUnits(10)), _axis, TextFormatFlags.NoPrefix);

            // Newest on the right; a shorter history starts part way in.
            int offset = FrameStats.HistorySeconds - history.Count;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            foreach ((_, Color color, Func<FrameStats.Point, double> value) in _series)
            {
                if (history.Count < 2)
                {
                    break;
                }

                var points = new PointF[history.Count];
                for (int i = 0; i < history.Count; i++)
                {
                    points[i] = new PointF(X(offset + i), Y(value(history[i])));
                }

                using var pen = new Pen(color, LogicalToDeviceUnits(1) * 1.3f);
                g.DrawLines(pen, points);
            }

            g.SmoothingMode = SmoothingMode.None;

            // Legend under the chart.
            int x = chart.Left;
            int y = chart.Bottom + LogicalToDeviceUnits(6);
            foreach ((string name, Color color, _) in _series)
            {
                using (var brush = new SolidBrush(color))
                {
                    g.FillRectangle(brush, x, y + LogicalToDeviceUnits(5), LogicalToDeviceUnits(10), LogicalToDeviceUnits(3));
                }

                x += LogicalToDeviceUnits(14);
                TextRenderer.DrawText(g, name, Font, new Point(x, y), _text, TextFormatFlags.NoPrefix);
                x += TextRenderer.MeasureText(name, Font).Width + LogicalToDeviceUnits(8);
            }

            TextRenderer.DrawText(g, $"last {FrameStats.HistorySeconds} s", Font, new Point(x, y), _axis, TextFormatFlags.NoPrefix);
        }
    }
}
