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
using System.Drawing;
using System.IO;
using AnimatedGif;

namespace UoFiddler.Controls.Classes
{
    public static class AnimatedFrameListExtensions
    {
        public static (Point, Size) GetAnimationDetails(this IEnumerable<AnimatedFrame> frames)
        {
            // Determine the point where to draw each frame's center
            var drawCenter = new Point(int.MinValue, int.MinValue);
            foreach (var frame in frames)
            {
                drawCenter.X = Math.Max(drawCenter.X, frame.Center.X);
                drawCenter.Y = Math.Max(drawCenter.Y, frame.Center.Y + frame.Bitmap.Height);
            }

            // Knowing where to draw each frame, determine the output image size by
            // "drawing" each frame's center at the draw point.
            var outputSize = new Size(0, 0);
            foreach (var frame in frames)
            {
                var location = new Point(drawCenter.X - frame.Center.X, drawCenter.Y - frame.Center.Y - frame.Bitmap.Height);
                outputSize.Width = Math.Max(outputSize.Width, location.X + frame.Bitmap.Width);
                outputSize.Height = Math.Max(outputSize.Height, location.Y + frame.Bitmap.Height);
            }
            
            return (drawCenter, outputSize);
        }

        public static void ToGif(this IEnumerable<AnimatedFrame> frames, string outputFile, bool looping = true, int delay = 150, bool showFrameBounds = false)
        {
            var (drawCenter, outputSize) = frames.GetAnimationDetails();

            {
                using var gif = AnimatedGif.AnimatedGif.Create(outputFile, delay);
                foreach (var frame in frames)
                {
                    if (frame?.Bitmap == null)
                    {
                        continue;
                    }

                    using var target = new Bitmap(outputSize.Width, outputSize.Height);
                    using var g = Graphics.FromImage(target);
                    var location = new Point(drawCenter.X - frame.Center.X, drawCenter.Y - frame.Center.Y - frame.Bitmap.Height);

                    g.DrawImage(frame.Bitmap, location);

                    if (showFrameBounds)
                    {
                        g.FillRectangle(new SolidBrush(Color.Red), new Rectangle(drawCenter, new Size(3, 3)));
                        g.DrawRectangle(new Pen(Color.Red), new Rectangle(location, new Size(frame.Bitmap.Width - 1, frame.Bitmap.Height - 1)));
                    }

                    gif.AddFrame(target, delay: -1, quality: GifQuality.Bit8);
                }
            }

            if (!looping)
            {
                using var stream = new FileStream(outputFile, FileMode.Open, FileAccess.Write);
                stream.Seek(28, SeekOrigin.Begin);
                stream.WriteByte(0);
            }
        }
    }
}
