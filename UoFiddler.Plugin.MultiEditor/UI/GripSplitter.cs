/***************************************************************************
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

using System;
using System.Windows.Forms;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    /// <summary>A docked splitter that shows the dots.</summary>
    internal sealed class GripSplitter : Splitter
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            SplitterGrip.Draw(e.Graphics, ClientRectangle);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}
