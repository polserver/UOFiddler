namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class FolderImportDialog
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
            components = new System.ComponentModel.Container();
            folderLabel = new System.Windows.Forms.Label();
            _folder = new System.Windows.Forms.TextBox();
            _browse = new System.Windows.Forms.Button();
            _recursive = new System.Windows.Forms.CheckBox();
            _scanButton = new System.Windows.Forms.Button();
            filterLabel = new System.Windows.Forms.Label();
            _filter = new System.Windows.Forms.TextBox();
            formatLabel = new System.Windows.Forms.Label();
            _format = new System.Windows.Forms.ComboBox();
            _list = new System.Windows.Forms.ListView();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderFormat = new System.Windows.Forms.ColumnHeader();
            columnHeaderTiles = new System.Windows.Forms.ColumnHeader();
            columnHeaderFile = new System.Windows.Forms.ColumnHeader();
            _preview = new System.Windows.Forms.PictureBox();
            _info = new System.Windows.Forms.Label();
            _status = new System.Windows.Forms.Label();
            _ok = new System.Windows.Forms.Button();
            cancel = new System.Windows.Forms.Button();
            _previewTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
            SuspendLayout();
            //
            // folderLabel
            //
            folderLabel.AutoSize = true;
            folderLabel.Location = new System.Drawing.Point(10, 13);
            folderLabel.Name = "folderLabel";
            folderLabel.Size = new System.Drawing.Size(43, 15);
            folderLabel.TabIndex = 0;
            folderLabel.Text = "Folder:";
            //
            // _folder
            //
            _folder.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _folder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            _folder.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystemDirectories;
            _folder.Location = new System.Drawing.Point(60, 10);
            _folder.Name = "_folder";
            _folder.Size = new System.Drawing.Size(500, 23);
            _folder.TabIndex = 1;
            //
            // _browse
            //
            _browse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _browse.Location = new System.Drawing.Point(566, 9);
            _browse.Name = "_browse";
            _browse.Size = new System.Drawing.Size(80, 25);
            _browse.TabIndex = 2;
            _browse.Text = "Browse...";
            _browse.UseVisualStyleBackColor = true;
            _browse.Click += Browse_Click;
            //
            // _recursive
            //
            _recursive.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _recursive.AutoSize = true;
            _recursive.Checked = true;
            _recursive.CheckState = System.Windows.Forms.CheckState.Checked;
            _recursive.Location = new System.Drawing.Point(656, 13);
            _recursive.Name = "_recursive";
            _recursive.Size = new System.Drawing.Size(125, 19);
            _recursive.TabIndex = 3;
            _recursive.Text = "Include subfolders";
            _recursive.UseVisualStyleBackColor = true;
            //
            // _scanButton
            //
            _scanButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _scanButton.Location = new System.Drawing.Point(805, 9);
            _scanButton.Name = "_scanButton";
            _scanButton.Size = new System.Drawing.Size(85, 25);
            _scanButton.TabIndex = 4;
            _scanButton.Text = "Scan";
            _scanButton.UseVisualStyleBackColor = true;
            _scanButton.Click += Scan_Click;
            //
            // filterLabel
            //
            filterLabel.AutoSize = true;
            filterLabel.Location = new System.Drawing.Point(10, 46);
            filterLabel.Name = "filterLabel";
            filterLabel.Size = new System.Drawing.Size(36, 15);
            filterLabel.TabIndex = 5;
            filterLabel.Text = "Filter:";
            //
            // _filter
            //
            _filter.Location = new System.Drawing.Point(60, 43);
            _filter.Name = "_filter";
            _filter.Size = new System.Drawing.Size(200, 23);
            _filter.TabIndex = 6;
            _filter.TextChanged += Filter_TextChanged;
            //
            // formatLabel
            //
            formatLabel.AutoSize = true;
            formatLabel.Location = new System.Drawing.Point(275, 46);
            formatLabel.Name = "formatLabel";
            formatLabel.Size = new System.Drawing.Size(48, 15);
            formatLabel.TabIndex = 7;
            formatLabel.Text = "Format:";
            //
            // _format
            //
            _format.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _format.Location = new System.Drawing.Point(330, 43);
            _format.MaxDropDownItems = 14;
            _format.Name = "_format";
            _format.Size = new System.Drawing.Size(220, 23);
            _format.TabIndex = 7;
            _format.SelectedIndexChanged += Format_SelectedIndexChanged;
            //
            // _list
            //
            _list.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderName, columnHeaderFormat, columnHeaderTiles, columnHeaderFile });
            _list.FullRowSelect = true;
            _list.HideSelection = false;
            _list.Location = new System.Drawing.Point(10, 76);
            _list.Name = "_list";
            _list.Size = new System.Drawing.Size(540, 434);
            _list.TabIndex = 8;
            _list.UseCompatibleStateImageBehavior = false;
            _list.View = System.Windows.Forms.View.Details;
            _list.VirtualMode = true;
            _list.RetrieveVirtualItem += List_RetrieveVirtualItem;
            _list.SelectedIndexChanged += List_SelectedIndexChanged;
            _list.VirtualItemsSelectionRangeChanged += List_VirtualItemsSelectionRangeChanged;
            _list.DoubleClick += List_DoubleClick;
            _list.KeyDown += List_KeyDown;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 200;
            //
            // columnHeaderFormat
            //
            columnHeaderFormat.Text = "Format";
            columnHeaderFormat.Width = 105;
            //
            // columnHeaderTiles
            //
            columnHeaderTiles.Text = "Tiles";
            columnHeaderTiles.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeaderTiles.Width = 55;
            //
            // columnHeaderFile
            //
            columnHeaderFile.Text = "File";
            columnHeaderFile.Width = 155;
            //
            // _preview
            //
            _preview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _preview.Location = new System.Drawing.Point(560, 76);
            _preview.Name = "_preview";
            _preview.Size = new System.Drawing.Size(330, 360);
            _preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            _preview.TabIndex = 9;
            _preview.TabStop = false;
            //
            // _info
            //
            _info.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _info.Location = new System.Drawing.Point(560, 442);
            _info.Name = "_info";
            _info.Size = new System.Drawing.Size(330, 68);
            _info.TabIndex = 10;
            //
            // _status
            //
            _status.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _status.AutoEllipsis = true;
            _status.Location = new System.Drawing.Point(10, 524);
            _status.Name = "_status";
            _status.Size = new System.Drawing.Size(700, 18);
            _status.TabIndex = 11;
            //
            // _ok
            //
            _ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _ok.Enabled = false;
            _ok.Location = new System.Drawing.Point(720, 520);
            _ok.Name = "_ok";
            _ok.Size = new System.Drawing.Size(85, 25);
            _ok.TabIndex = 12;
            _ok.Text = "Open";
            _ok.UseVisualStyleBackColor = true;
            _ok.Click += Ok_Click;
            //
            // cancel
            //
            cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancel.Location = new System.Drawing.Point(810, 520);
            cancel.Name = "cancel";
            cancel.Size = new System.Drawing.Size(80, 25);
            cancel.TabIndex = 13;
            cancel.Text = "Cancel";
            cancel.UseVisualStyleBackColor = true;
            //
            // _previewTimer
            //
            _previewTimer.Interval = 120;
            _previewTimer.Tick += PreviewTimer_Tick;
            //
            // FolderImportDialog
            //
            CancelButton = cancel;
            ClientSize = new System.Drawing.Size(900, 556);
            Controls.Add(folderLabel);
            Controls.Add(_folder);
            Controls.Add(_browse);
            Controls.Add(_recursive);
            Controls.Add(_scanButton);
            Controls.Add(filterLabel);
            Controls.Add(_filter);
            Controls.Add(formatLabel);
            Controls.Add(_format);
            Controls.Add(_list);
            Controls.Add(_preview);
            Controls.Add(_info);
            Controls.Add(_status);
            Controls.Add(_ok);
            Controls.Add(cancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(720, 400);
            Name = "FolderImportDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Import multis from folder";
            ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label folderLabel;
        private System.Windows.Forms.TextBox _folder;
        private System.Windows.Forms.Button _browse;
        private System.Windows.Forms.CheckBox _recursive;
        private System.Windows.Forms.Button _scanButton;
        private System.Windows.Forms.Label filterLabel;
        private System.Windows.Forms.TextBox _filter;
        private System.Windows.Forms.Label formatLabel;
        private System.Windows.Forms.ComboBox _format;
        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.ColumnHeader columnHeaderTiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFile;
        private System.Windows.Forms.PictureBox _preview;
        private System.Windows.Forms.Label _info;
        private System.Windows.Forms.Label _status;
        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Timer _previewTimer;
    }
}
