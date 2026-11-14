/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Drawing;

namespace UoFiddler.Plugin.MultiEditor.Rendering
{
    /// <summary>
    /// Pan and zoom of the canvas. World space is the tile pixel space of the multi
    /// (<c>MultiTile.XMod</c>/<c>YMod</c>); screen = (world - origin) * zoom.
    /// </summary>
    internal sealed class Camera2D
    {
        public const float MinZoom = 0.25f;
        public const float MaxZoom = 8f;

        /// <summary>World position shown at the top left of the canvas.</summary>
        public float OriginX { get; private set; }

        public float OriginY { get; private set; }

        public float Zoom { get; private set; } = 1f;

        /// <summary>
        /// True while the view is the one <see cref="Fit"/> chose: a new camera, or one not panned or
        /// zoomed since. The canvas fits such a view again when its size changes.
        /// </summary>
        public bool ShowsFit { get; private set; } = true;

        public PointF ScreenToWorld(Point screen)
        {
            return new PointF(OriginX + (screen.X / Zoom), OriginY + (screen.Y / Zoom));
        }

        public PointF WorldToScreen(float x, float y)
        {
            return new PointF((x - OriginX) * Zoom, (y - OriginY) * Zoom);
        }

        public RectangleF VisibleWorld(Size view)
        {
            return new RectangleF(OriginX, OriginY, view.Width / Zoom, view.Height / Zoom);
        }

        public void Pan(int screenDx, int screenDy)
        {
            ShowsFit = false;
            OriginX -= screenDx / Zoom;
            OriginY -= screenDy / Zoom;
        }

        /// <summary>Sets the zoom, keeping the world point under <paramref name="anchor"/> in place.</summary>
        public void ZoomAt(Point anchor, float zoom)
        {
            ShowsFit = false;
            zoom = Math.Clamp(zoom, MinZoom, MaxZoom);
            PointF world = ScreenToWorld(anchor);
            Zoom = zoom;
            OriginX = world.X - (anchor.X / Zoom);
            OriginY = world.Y - (anchor.Y / Zoom);
        }

        /// <summary>
        /// One wheel notch: whole steps above 100% so pixel art stays crisp, finer steps below.
        /// </summary>
        public void ZoomStep(Point anchor, int direction)
        {
            float zoom = Zoom;
            if (direction > 0)
            {
                zoom = zoom < 1f ? Math.Min(1f, zoom + 0.25f) : (float)Math.Floor(zoom) + 1f;
            }
            else
            {
                zoom = zoom > 1f ? (float)Math.Ceiling(zoom) - 1f : zoom - 0.25f;
            }

            ZoomAt(anchor, zoom);
        }

        /// <summary>Puts the middle of a world rectangle in the middle of the view, keeping the zoom.</summary>
        public void CenterOn(RectangleF world, Size view)
        {
            OriginX = world.X + (world.Width / 2f) - (view.Width / 2f / Zoom);
            OriginY = world.Y + (world.Height / 2f) - (view.Height / 2f / Zoom);
        }

        /// <summary>
        /// Shows a world rectangle centred at the largest zoom step that fits: whole steps up to
        /// <paramref name="maxZoom"/>, quarter steps below 100%.
        /// </summary>
        public void Fit(RectangleF world, Size view, float maxZoom = 2f, int margin = 24)
        {
            // Asked for even when the view has no size yet: the canvas fits again once it has one.
            ShowsFit = true;
            if (world.Width <= 0 || world.Height <= 0 || view.Width <= 0 || view.Height <= 0)
            {
                return;
            }

            float fit = Math.Min((view.Width - (2f * margin)) / world.Width, (view.Height - (2f * margin)) / world.Height);
            float zoom = (float)Math.Floor(Math.Min(fit, maxZoom));
            if (zoom < 1f)
            {
                zoom = 1f;
                while (zoom > MinZoom && zoom > fit)
                {
                    zoom -= 0.25f;
                }
            }

            Zoom = Math.Max(MinZoom, zoom);
            OriginX = world.X + (world.Width / 2f) - (view.Width / 2f / Zoom);
            OriginY = world.Y + (world.Height / 2f) - (view.Height / 2f / Zoom);
        }
    }
}
