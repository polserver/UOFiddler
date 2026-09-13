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
using System.Drawing;
using System.Windows.Forms;

namespace UoFiddler.Controls.Classes
{
    public static class FormLayout
    {
        /// <summary>
        /// Shrinks a dialog that is laid out larger than the screen it opens on, so a designer size
        /// picked for a wide monitor does not push the buttons off the bottom of a small one. The
        /// form keeps its own minimum size, and nothing happens when it already fits.
        /// </summary>
        public static void FitToScreen(Form form)
        {
            if (form == null)
            {
                return;
            }

            Rectangle work = Screen.FromControl(form).WorkingArea;

            int width = form.Width;
            int height = form.Height;

            if (width > work.Width)
            {
                width = work.Width;
            }

            if (height > work.Height)
            {
                height = work.Height;
            }

            if (width == form.Width && height == form.Height)
            {
                return;
            }

            form.Size = new Size(width, height);

            // Centring happened against the old size, so put it back inside the screen.
            int left = form.Left;
            int top = form.Top;

            if (left + form.Width > work.Right)
            {
                left = work.Right - form.Width;
            }

            if (top + form.Height > work.Bottom)
            {
                top = work.Bottom - form.Height;
            }

            form.Location = new Point(Math.Max(work.Left, left), Math.Max(work.Top, top));
        }
    }
}