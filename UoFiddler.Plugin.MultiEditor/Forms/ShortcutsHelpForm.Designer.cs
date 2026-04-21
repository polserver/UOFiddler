namespace UoFiddler.Plugin.MultiEditor.Forms
{
    partial class ShortcutsHelpForm
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
            _listView = new System.Windows.Forms.ListView();
            _columnKey = new System.Windows.Forms.ColumnHeader();
            _columnAction = new System.Windows.Forms.ColumnHeader();
            _btnClose = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // _listView
            // 
            _listView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { _columnKey, _columnAction });
            _listView.FullRowSelect = true;
            _listView.GridLines = true;
            _listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            _listView.Location = new System.Drawing.Point(8, 8);
            _listView.MultiSelect = false;
            _listView.Name = "_listView";
            _listView.Size = new System.Drawing.Size(388, 409);
            _listView.TabIndex = 0;
            _listView.UseCompatibleStateImageBehavior = false;
            _listView.View = System.Windows.Forms.View.Details;
            // 
            // _columnKey
            // 
            _columnKey.Text = "Shortcut";
            _columnKey.Width = 130;
            // 
            // _columnAction
            // 
            _columnAction.Text = "Action";
            _columnAction.Width = 230;
            // 
            // _btnClose
            // 
            _btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _btnClose.Location = new System.Drawing.Point(321, 425);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new System.Drawing.Size(75, 27);
            _btnClose.TabIndex = 1;
            _btnClose.Text = "Close";
            _btnClose.UseVisualStyleBackColor = true;
            // 
            // ShortcutsHelpForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = _btnClose;
            ClientSize = new System.Drawing.Size(404, 461);
            Controls.Add(_listView);
            Controls.Add(_btnClose);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ShortcutsHelpForm";
            Padding = new System.Windows.Forms.Padding(8);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Keyboard Shortcuts";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ListView _listView;
        private System.Windows.Forms.ColumnHeader _columnKey;
        private System.Windows.Forms.ColumnHeader _columnAction;
        private System.Windows.Forms.Button _btnClose;
    }
}