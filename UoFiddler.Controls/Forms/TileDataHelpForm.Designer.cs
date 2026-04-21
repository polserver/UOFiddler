namespace UoFiddler.Controls.Forms
{
    partial class TileDataHelpForm
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
            _splitContainer = new System.Windows.Forms.SplitContainer();
            _listView = new System.Windows.Forms.ListView();
            _columnField = new System.Windows.Forms.ColumnHeader();
            _descriptionBox = new System.Windows.Forms.RichTextBox();
            _btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)_splitContainer).BeginInit();
            _splitContainer.Panel1.SuspendLayout();
            _splitContainer.Panel2.SuspendLayout();
            _splitContainer.SuspendLayout();
            SuspendLayout();
            // 
            // _splitContainer
            // 
            _splitContainer.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _splitContainer.Location = new System.Drawing.Point(8, 8);
            _splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            _splitContainer.Panel1.Controls.Add(_listView);
            // 
            // _splitContainer.Panel2
            // 
            _splitContainer.Panel2.Controls.Add(_descriptionBox);
            _splitContainer.Size = new System.Drawing.Size(544, 396);
            _splitContainer.SplitterDistance = 185;
            _splitContainer.TabIndex = 0;
            // 
            // _listView
            // 
            _listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { _columnField });
            _listView.Dock = System.Windows.Forms.DockStyle.Fill;
            _listView.FullRowSelect = true;
            _listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            _listView.Location = new System.Drawing.Point(0, 0);
            _listView.MultiSelect = false;
            _listView.Name = "_listView";
            _listView.Size = new System.Drawing.Size(185, 396);
            _listView.TabIndex = 0;
            _listView.UseCompatibleStateImageBehavior = false;
            _listView.View = System.Windows.Forms.View.Details;
            _listView.SelectedIndexChanged += OnSelectionChanged;
            // 
            // _columnField
            // 
            _columnField.Text = "Field";
            _columnField.Width = 162;
            // 
            // _descriptionBox
            // 
            _descriptionBox.BackColor = System.Drawing.SystemColors.Control;
            _descriptionBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            _descriptionBox.Dock = System.Windows.Forms.DockStyle.Fill;
            _descriptionBox.Location = new System.Drawing.Point(0, 0);
            _descriptionBox.Name = "_descriptionBox";
            _descriptionBox.ReadOnly = true;
            _descriptionBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            _descriptionBox.Size = new System.Drawing.Size(355, 396);
            _descriptionBox.TabIndex = 0;
            _descriptionBox.TabStop = false;
            _descriptionBox.Text = "";
            // 
            // _btnClose
            // 
            _btnClose.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _btnClose.Location = new System.Drawing.Point(477, 412);
            _btnClose.Name = "_btnClose";
            _btnClose.Size = new System.Drawing.Size(75, 27);
            _btnClose.TabIndex = 1;
            _btnClose.Text = "Close";
            _btnClose.UseVisualStyleBackColor = true;
            // 
            // TileDataHelpForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = _btnClose;
            ClientSize = new System.Drawing.Size(560, 448);
            Controls.Add(_splitContainer);
            Controls.Add(_btnClose);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TileDataHelpForm";
            Padding = new System.Windows.Forms.Padding(8);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "TileData Field Reference";
            _splitContainer.Panel1.ResumeLayout(false);
            _splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_splitContainer).EndInit();
            _splitContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.ListView _listView;
        private System.Windows.Forms.ColumnHeader _columnField;
        private System.Windows.Forms.RichTextBox _descriptionBox;
        private System.Windows.Forms.Button _btnClose;
    }
}
