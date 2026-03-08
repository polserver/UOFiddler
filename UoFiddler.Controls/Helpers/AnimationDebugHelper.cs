using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UoFiddler.Controls.Classes;

namespace UoFiddler.Controls.Helpers
{
    public static class AnimationDebugHelper
    {
        public static void CreateDebugImage(string outputPath, Bitmap packedImage, List<PackedFrameEntry> frames)
        {
            if (packedImage == null || frames == null)
            {
                return;
            }

            using (Bitmap debugImage = new Bitmap(packedImage.Width, packedImage.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(debugImage))
                {
                    // 1. Draw the packed image
                    g.DrawImage(packedImage, 0, 0);

                    using (Pen redPen = new Pen(Color.Red, 1))
                    using (Pen blackPen = new Pen(Color.Black, 1))
                    {
                        foreach (var frame in frames)
                        {
                            if (frame.Frame == null || frame.Center == null)
                            {
                                continue;
                            }

                            // 2. Draw Red Rectangle for frame bounds
                            Rectangle rect = new Rectangle(frame.Frame.X, frame.Frame.Y, frame.Frame.W, frame.Frame.H);
                            g.DrawRectangle(redPen, rect);

                            // Calculate absolute center point (Geometric Center)
                            int centerX = frame.Frame.X + (frame.Frame.W / 2);
                            int centerY = frame.Frame.Y + (frame.Frame.H / 2);

                            // 3. Draw Black Lines (Crosshair)
                            // Horizontal line: from left of frame to right of frame at centerY
                            g.DrawLine(blackPen, frame.Frame.X, centerY, frame.Frame.X + frame.Frame.W, centerY);

                            // Vertical line: from top of frame to bottom of frame at centerX
                            g.DrawLine(blackPen, centerX, frame.Frame.Y, centerX, frame.Frame.Y + frame.Frame.H);
                        }
                    }
                }

                debugImage.Save(outputPath, ImageFormat.Png);
            }
        }
    }
}
