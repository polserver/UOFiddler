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