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
using System.Diagnostics;
using System.Text;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// Timings of the canvas for the performance box, summed per second: the box shows the last
    /// whole second. Recording is a few additions, so it costs nothing worth measuring.
    /// </summary>
    internal sealed class FrameStats
    {
        private sealed class Measure
        {
            public int Count;
            public double Sum;
            public double Max;

            public void Add(double value)
            {
                Count++;
                Sum += value;
                Max = Math.Max(Max, value);
            }

            public string Text => Count == 0 ? "-" : $"{Sum / Count,6:F2} ms avg {Max,6:F2} max";
        }

        private sealed class Second
        {
            public double Seconds = 1;
            public int Sprites;
            public readonly Measure Draw = new Measure();
            public readonly Measure Gpu = new Measure();
            public readonly Measure Swap = new Measure();
            public readonly Measure Frame = new Measure();
            public readonly Measure Move = new Measure();
            public readonly Measure Hit = new Measure();
            public readonly Measure Lag = new Measure();
        }

        /// <summary>One second of the history chart: averages in ms, and frames per second.</summary>
        public readonly record struct Point(double Fps, double Frame, double Gpu, double Draw, double Lag);

        /// <summary>Seconds of history kept for the chart.</summary>
        public const int HistorySeconds = 90;

        private readonly List<Point> _history = new List<Point>(HistorySeconds + 1);
        private Second _current = new Second();
        private Second _last = new Second();
        private long _start = Stopwatch.GetTimestamp();

        /// <summary>The last seconds, oldest first.</summary>
        public IReadOnlyList<Point> History
        {
            get
            {
                Roll();
                return _history;
            }
        }

        public static double Ms(long from, long to) => (to - from) * 1000.0 / Stopwatch.Frequency;

        /// <param name="draw">Building and submitting the scene (CPU).</param>
        /// <param name="swap">SwapBuffers: waiting for the GPU and vertical sync.</param>
        /// <param name="frame">The whole paint.</param>
        public void Frame(double draw, double swap, double frame, int sprites)
        {
            Roll();
            _current.Draw.Add(draw);
            _current.Swap.Add(swap);
            _current.Frame.Add(frame);
            _current.Sprites = sprites;
        }

        /// <summary>GPU time of a frame, from a timer query (it arrives a frame or two late).</summary>
        public void Gpu(double ms)
        {
            Roll();
            _current.Gpu.Add(ms);
        }

        /// <param name="handler">The whole mouse move: hit test, tool, status bar.</param>
        /// <param name="hitTest">Finding the tile under the cursor.</param>
        public void Move(double handler, double hitTest)
        {
            Roll();
            _current.Move.Add(handler);
            _current.Hit.Add(hitTest);
        }

        /// <summary>From the first mouse move not yet shown to the end of the frame that shows it.</summary>
        public void Lag(double ms)
        {
            Roll();
            _current.Lag.Add(ms);
        }

        public string Describe(int tiles)
        {
            Roll();
            Second s = _last;
            int frames = s.Frame.Count;
            var text = new StringBuilder();
            text.AppendLine($"Frames  {frames / s.Seconds,5:F0}/s");
            text.AppendLine($"Draw    {s.Draw.Text}   CPU, scene");
            text.AppendLine($"GPU     {s.Gpu.Text}");
            text.AppendLine($"Swap    {s.Swap.Text}   GPU + vsync wait");
            text.AppendLine($"Frame   {s.Frame.Text}");
            text.AppendLine($"Mouse   {s.Move.Count / s.Seconds,5:F0}/s");
            text.AppendLine($"Handler {s.Move.Text}");
            text.AppendLine($"HitTest {s.Hit.Text}");
            text.AppendLine($"Lag     {s.Lag.Text}   move to shown");
            text.Append($"Scene   {s.Sprites:N0} sprites, {tiles:N0} tiles");
            return text.ToString();
        }

        private void Roll()
        {
            long now = Stopwatch.GetTimestamp();
            double seconds = Ms(_start, now) / 1000.0;
            if (seconds < 1)
            {
                return;
            }

            _current.Seconds = seconds;
            _last = _current;
            _current = new Second();
            _start = now;

            // An idle stretch (nothing recorded for a while) counts as one quiet second.
            _history.Add(new Point(_last.Frame.Count / seconds, Avg(_last.Frame), Avg(_last.Gpu), Avg(_last.Draw), Avg(_last.Lag)));
            if (_history.Count > HistorySeconds)
            {
                _history.RemoveAt(0);
            }
        }

        private static double Avg(Measure m) => m.Count == 0 ? 0 : m.Sum / m.Count;
    }
}
