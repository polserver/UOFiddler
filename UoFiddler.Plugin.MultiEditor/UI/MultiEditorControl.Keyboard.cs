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
using UoFiddler.Plugin.MultiEditor.Editing;

namespace UoFiddler.Plugin.MultiEditor.UI
{
    public partial class MultiEditorControl
    {
        // ---------------------------------------------------------------- keyboard

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (!_built)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            Control focused = FocusedControl();
            bool sidebar = InSidebar(focused);
            Keys code = keyData & Keys.KeyCode, held = keyData & Keys.Modifiers;
            if (keyData == Keys.Escape && sidebar)
            {
                // Esc is the sidebar control's first: a search box clears itself, a name being
                // typed is dropped, an open list closes. With nothing of that, the canvas gets
                // the keyboard back; the tool and the selection stay as they are.
                if (focused is TextBoxBase { TextLength: > 0 } || focused is ComboBox { DroppedDown: true } || NativeEditHasFocus())
                {
                    return base.ProcessCmdKey(ref msg, keyData);
                }

                _canvas.Focus();
                return true;
            }

            if (keyData != Keys.Escape && IsTextInputFocused())
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            // A list, tree, grid or gallery of a sidebar has keys of its own (arrows, Delete,
            // Enter, letters to search): plain keys are its, and a button's Space and Enter the
            // button's. Ctrl and Alt shortcuts and the function keys stay the editor's.
            bool plain = (held == Keys.None || held == Keys.Shift) && !(code >= Keys.F1 && code <= Keys.F24);
            if (sidebar && plain && (TakesKeys(focused) || (focused is ButtonBase && code is Keys.Space or Keys.Enter)))
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            _messageUntil = 0;
            if (_commands.TryRun(keyData))
            {
                _commands.RefreshStates();
                return true;
            }

            EditorSession session = Session;
            if (session == null)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            // Arrows move the selection one cell; without a selection, or with Ctrl, they pan.
            (int dx, int dy) = (keyData & Keys.KeyCode) switch
            {
                Keys.Left => (-1, 0),
                Keys.Right => (1, 0),
                Keys.Up => (0, -1),
                Keys.Down => (0, 1),
                _ => (0, 0)
            };

            Keys modifiers = keyData & Keys.Modifiers;
            if ((dx, dy) == (0, 0) || (modifiers != Keys.None && modifiers != Keys.Control))
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (modifiers == Keys.None && session.Selection.Count > 0)
            {
                session.NudgeSelection(dx, dy, 0);
            }
            else
            {
                _canvas.Camera.Pan(-dx * 44, -dy * 44);
                _canvas.Invalidate();
            }

            return true;
        }

        /// <summary>True while typing in a text, number or combo box, where letters must not switch tools.</summary>
        /// <summary>The control of the editor that has the keyboard.</summary>
        private Control FocusedControl()
        {
            Control focused = this;
            while (focused is ContainerControl container && container.ActiveControl != null)
            {
                focused = container.ActiveControl;
            }

            return focused;
        }

        /// <summary>True for a control of the tile browser, the design sidebar or the house panel.</summary>
        private bool InSidebar(Control control)
        {
            for (Control at = control; at != null && at != this; at = at.Parent)
            {
                if (at == _browser || at == _inspector || at == _housePanel)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>True for a control that works with plain keys itself.</summary>
        private static bool TakesKeys(Control control)
        {
            return control is ListView or ListBox or TreeView or TrackBar or TileBrowserGrid or TileBrowserHouseGallery;
        }

        private bool IsTextInputFocused()
        {
            Control focused = FocusedControl();

            return focused is TextBoxBase || focused is NumericUpDown || focused is ComboBox ||
                   focused.Parent is NumericUpDown || focused is ToolStrip || NativeEditHasFocus();
        }

        /// <summary>
        /// True while a plain Windows edit box has the keyboard: the box a list or tree shows to
        /// rename an item (a layer's name) is one, and it is no WinForms control, so the
        /// focused control is still the list.
        /// </summary>
        private static bool NativeEditHasFocus()
        {
            IntPtr focus = GetFocus();
            if (focus == IntPtr.Zero)
            {
                return false;
            }

            var name = new System.Text.StringBuilder(32);
            return GetClassName(focus, name, name.Capacity) > 0 && name.ToString().Equals("Edit", StringComparison.OrdinalIgnoreCase);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder className, int maxCount);
    }
}
