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

using System.Windows.Forms;

namespace UoFiddler.Controls.Helpers
{
    public static class RichTextBoxExtensions
    {
        public static void AddBasicContextMenu(this RichTextBox richTextBox)
        {
            if (richTextBox.ContextMenuStrip != null)
            {
                return;
            }

            ContextMenuStrip menuStrip = new ContextMenuStrip { ShowImageMargin = false };

            ToolStripMenuItem menuItemCopy = new ToolStripMenuItem("Copy");
            menuItemCopy.Click += (sender, e) => richTextBox.Copy();
            menuStrip.Items.Add(menuItemCopy);

            menuStrip.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem menuItemSelectAll = new ToolStripMenuItem("Select All");
            menuItemSelectAll.Click += (sender, e) => richTextBox.SelectAll();
            menuStrip.Items.Add(menuItemSelectAll);

            menuStrip.Opening += (sender, cancelEventArgs) =>
            {
                menuItemCopy.Enabled = richTextBox.SelectionLength > 0;
                menuItemSelectAll.Enabled = richTextBox.TextLength > 0 && richTextBox.SelectionLength < richTextBox.TextLength;
            };

            richTextBox.ContextMenuStrip = menuStrip;
        }
    }
}