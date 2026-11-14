namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class DocumentIOPickDialog
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
            filterLabel = new System.Windows.Forms.Label();
            _filter = new System.Windows.Forms.TextBox();
            _list = new System.Windows.Forms.ListView();
            columnHeaderId = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            _preview = new System.Windows.Forms.PictureBox();
            _info = new System.Windows.Forms.Label();
            _ok = new System.Windows.Forms.Button();
            _saveBlueprints = new System.Windows.Forms.Button();
            cancel = new System.Windows.Forms.Button();
            sourceAll = new System.Windows.Forms.RadioButton();
            sourceMul = new System.Windows.Forms.RadioButton();
            sourceUop = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
            SuspendLayout();
            //
            // filterLabel
            //
            filterLabel.AutoSize = true;
            filterLabel.Location = new System.Drawing.Point(10, 13);
            filterLabel.Name = "filterLabel";
            filterLabel.Size = new System.Drawing.Size(154, 15);
            filterLabel.TabIndex = 0;
            filterLabel.Text = "Filter (name, id or 0x id):";
            //
            // _filter
            //
            _filter.Location = new System.Drawing.Point(170, 10);
            _filter.Name = "_filter";
            _filter.Size = new System.Drawing.Size(200, 23);
            _filter.TabIndex = 1;
            _filter.TextChanged += Filter_TextChanged;
            //
            // _list
            //
            _list.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderId, columnHeaderName, columnHeaderFile });
            _list.FullRowSelect = true;
            _list.HideSelection = false;
            _list.Location = new System.Drawing.Point(10, 40);
            _list.Name = "_list";
            _list.Size = new System.Drawing.Size(380, 370);
            _list.TabIndex = 2;
            _list.UseCompatibleStateImageBehavior = false;
            _list.View = System.Windows.Forms.View.Details;
            _list.SelectedIndexChanged += List_SelectedIndexChanged;
            _list.DoubleClick += List_DoubleClick;
            _list.KeyDown += List_KeyDown;
            //
            // columnHeaderId
            //
            columnHeaderId.Text = "Id";
            columnHeaderId.Width = 90;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 170;
            //
            // columnHeaderFile
            //
            columnHeaderFile.Text = "File";
            columnHeaderFile.Width = 90;
            //
            // _preview
            //
            _preview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _preview.Location = new System.Drawing.Point(400, 40);
            _preview.Name = "_preview";
            _preview.Size = new System.Drawing.Size(350, 350);
            _preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            _preview.TabIndex = 3;
            _preview.TabStop = false;
            //
            // _info
            //
            _info.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _info.Location = new System.Drawing.Point(400, 394);
            _info.Name = "_info";
            _info.Size = new System.Drawing.Size(350, 18);
            _info.TabIndex = 4;
            //
            // _ok
            //
            _ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _ok.Location = new System.Drawing.Point(595, 425);
            _ok.Name = "_ok";
            _ok.Size = new System.Drawing.Size(75, 23);
            _ok.TabIndex = 5;
            _ok.Text = "Open";
            _ok.UseVisualStyleBackColor = true;
            _ok.Click += Ok_Click;
            //
            // _saveBlueprints
            //
            _saveBlueprints.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _saveBlueprints.Location = new System.Drawing.Point(445, 425);
            _saveBlueprints.Name = "_saveBlueprints";
            _saveBlueprints.Size = new System.Drawing.Size(145, 23);
            _saveBlueprints.TabIndex = 10;
            _saveBlueprints.Text = "Save as blueprints...";
            _saveBlueprints.UseVisualStyleBackColor = true;
            _saveBlueprints.Visible = false;
            _saveBlueprints.Click += SaveBlueprints_Click;
            //
            // cancel
            //
            cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancel.Location = new System.Drawing.Point(675, 425);
            cancel.Name = "cancel";
            cancel.Size = new System.Drawing.Size(75, 23);
            cancel.TabIndex = 6;
            cancel.Text = "Cancel";
            cancel.UseVisualStyleBackColor = true;
            //
            // sourceAll
            //
            sourceAll.Appearance = System.Windows.Forms.Appearance.Button;
            sourceAll.Checked = true;
            sourceAll.Location = new System.Drawing.Point(380, 8);
            sourceAll.Name = "sourceAll";
            sourceAll.Size = new System.Drawing.Size(48, 24);
            sourceAll.TabIndex = 7;
            sourceAll.TabStop = true;
            sourceAll.Text = "All";
            sourceAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            sourceAll.UseVisualStyleBackColor = true;
            sourceAll.CheckedChanged += SourceButton_CheckedChanged;
            //
            // sourceMul
            //
            sourceMul.Appearance = System.Windows.Forms.Appearance.Button;
            sourceMul.Location = new System.Drawing.Point(430, 8);
            sourceMul.Name = "sourceMul";
            sourceMul.Size = new System.Drawing.Size(74, 24);
            sourceMul.TabIndex = 8;
            sourceMul.Tag = "multi.mul";
            sourceMul.Text = "multi.mul";
            sourceMul.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            sourceMul.UseVisualStyleBackColor = true;
            sourceMul.CheckedChanged += SourceButton_CheckedChanged;
            //
            // sourceUop
            //
            sourceUop.Appearance = System.Windows.Forms.Appearance.Button;
            sourceUop.Location = new System.Drawing.Point(506, 8);
            sourceUop.Name = "sourceUop";
            sourceUop.Size = new System.Drawing.Size(48, 24);
            sourceUop.TabIndex = 9;
            sourceUop.Tag = "UOP";
            sourceUop.Text = "UOP";
            sourceUop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            sourceUop.UseVisualStyleBackColor = true;
            sourceUop.CheckedChanged += SourceButton_CheckedChanged;
            //
            // DocumentIOPickDialog
            //
            CancelButton = cancel;
            ClientSize = new System.Drawing.Size(760, 460);
            Controls.Add(filterLabel);
            Controls.Add(_filter);
            Controls.Add(_list);
            Controls.Add(_preview);
            Controls.Add(_info);
            Controls.Add(_ok);
            Controls.Add(_saveBlueprints);
            Controls.Add(cancel);
            Controls.Add(sourceAll);
            Controls.Add(sourceMul);
            Controls.Add(sourceUop);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(560, 320);
            Name = "DocumentIOPickDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label filterLabel;
        private System.Windows.Forms.TextBox _filter;
        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.PictureBox _preview;
        private System.Windows.Forms.Label _info;
        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.Button _saveBlueprints;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.RadioButton sourceAll;
        private System.Windows.Forms.RadioButton sourceMul;
        private System.Windows.Forms.RadioButton sourceUop;
    }
}
