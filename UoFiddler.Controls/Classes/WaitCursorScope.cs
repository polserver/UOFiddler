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
using System.Windows.Forms;

namespace UoFiddler.Controls.Classes
{
    /// <summary>
    /// Shows a wait cursor for the duration of a blocking UI-thread operation.
    /// Setting the parent form's <see cref="Control.UseWaitCursor"/> survives mouse movement
    /// (unlike <see cref="Cursor.Current"/> alone, which is reset on the next WM_SETCURSOR),
    /// and Dispose restores it even if the operation throws.
    /// </summary>
    public sealed class WaitCursorScope : IDisposable
    {
        private readonly Form _form;

        public WaitCursorScope(Control control)
        {
            _form = control?.FindForm();
            if (_form != null)
            {
                _form.UseWaitCursor = true;
            }
            Cursor.Current = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            if (_form != null)
            {
                _form.UseWaitCursor = false;
            }
            Cursor.Current = Cursors.Default;
        }
    }
}
