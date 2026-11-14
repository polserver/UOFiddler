namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class ShortcutsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _list = new System.Windows.Forms.ListView();
            _commandColumn = new System.Windows.Forms.ColumnHeader();
            _keysColumn = new System.Windows.Forms.ColumnHeader();
            SuspendLayout();
            //
            // _list
            //
            _list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { _commandColumn, _keysColumn });
            _list.Dock = System.Windows.Forms.DockStyle.Fill;
            _list.FullRowSelect = true;
            _list.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            _list.Location = new System.Drawing.Point(0, 0);
            _list.Name = "_list";
            _list.Size = new System.Drawing.Size(520, 620);
            _list.TabIndex = 0;
            _list.View = System.Windows.Forms.View.Details;
            //
            // _commandColumn
            //
            _commandColumn.Text = "Command";
            _commandColumn.Width = 330;
            //
            // _keysColumn
            //
            _keysColumn.Text = "Keys";
            _keysColumn.Width = 160;
            //
            // ShortcutsForm
            //
            ClientSize = new System.Drawing.Size(520, 620);
            Controls.Add(_list);
            MinimizeBox = false;
            Name = "ShortcutsForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Multi Editor shortcuts";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader _commandColumn;
        private System.Windows.Forms.ColumnHeader _keysColumn;
    }
}
