namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class DocumentIOSaveDialog
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
            idLabel = new System.Windows.Forms.Label();
            _id = new System.Windows.Forms.TextBox();
            nextFree = new System.Windows.Forms.Button();
            _list = new System.Windows.Forms.ListView();
            columnHeaderId = new System.Windows.Forms.ColumnHeader();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderHolds = new System.Windows.Forms.ColumnHeader();
            _preview = new System.Windows.Forms.PictureBox();
            _info = new System.Windows.Forms.Label();
            _ok = new System.Windows.Forms.Button();
            cancel = new System.Windows.Forms.Button();
            writePanel = new System.Windows.Forms.Panel();
            writeLabel = new System.Windows.Forms.Label();
            _writeMul = new System.Windows.Forms.RadioButton();
            _writeUop = new System.Windows.Forms.RadioButton();
            _writeNone = new System.Windows.Forms.RadioButton();
            filterAll = new System.Windows.Forms.RadioButton();
            filterFree = new System.Windows.Forms.RadioButton();
            filterUsed = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
            writePanel.SuspendLayout();
            SuspendLayout();
            //
            // idLabel
            //
            idLabel.AutoSize = true;
            idLabel.Location = new System.Drawing.Point(10, 13);
            idLabel.Name = "idLabel";
            idLabel.Size = new System.Drawing.Size(150, 15);
            idLabel.TabIndex = 0;
            idLabel.Text = "Slot id (decimal or 0x hex):";
            //
            // _id
            //
            _id.Location = new System.Drawing.Point(170, 10);
            _id.Name = "_id";
            _id.Size = new System.Drawing.Size(90, 23);
            _id.TabIndex = 1;
            _id.TextChanged += Id_TextChanged;
            //
            // nextFree
            //
            nextFree.Location = new System.Drawing.Point(266, 8);
            nextFree.Name = "nextFree";
            nextFree.Size = new System.Drawing.Size(74, 24);
            nextFree.TabIndex = 2;
            nextFree.Text = "Next free";
            nextFree.UseVisualStyleBackColor = true;
            nextFree.Click += NextFree_Click;
            //
            // _list
            //
            _list.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderId, columnHeaderName, columnHeaderHolds });
            _list.FullRowSelect = true;
            _list.HideSelection = false;
            _list.Location = new System.Drawing.Point(10, 40);
            _list.MultiSelect = false;
            _list.Name = "_list";
            _list.Size = new System.Drawing.Size(380, 362);
            _list.TabIndex = 3;
            _list.UseCompatibleStateImageBehavior = false;
            _list.View = System.Windows.Forms.View.Details;
            _list.VirtualMode = true;
            _list.RetrieveVirtualItem += List_RetrieveVirtualItem;
            _list.SelectedIndexChanged += List_SelectedIndexChanged;
            _list.DoubleClick += List_DoubleClick;
            //
            // columnHeaderId
            //
            columnHeaderId.Text = "Id";
            columnHeaderId.Width = 100;
            //
            // columnHeaderName
            //
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 130;
            //
            // columnHeaderHolds
            //
            columnHeaderHolds.Text = "Holds";
            columnHeaderHolds.Width = 110;
            //
            // _preview
            //
            _preview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _preview.Location = new System.Drawing.Point(400, 40);
            _preview.Name = "_preview";
            _preview.Size = new System.Drawing.Size(350, 338);
            _preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            _preview.TabIndex = 4;
            _preview.TabStop = false;
            //
            // _info
            //
            _info.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _info.Location = new System.Drawing.Point(400, 380);
            _info.Name = "_info";
            _info.Size = new System.Drawing.Size(350, 28);
            _info.TabIndex = 5;
            //
            // _ok
            //
            _ok.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _ok.Location = new System.Drawing.Point(585, 445);
            _ok.Name = "_ok";
            _ok.Size = new System.Drawing.Size(84, 26);
            _ok.TabIndex = 6;
            _ok.Text = "Save here";
            _ok.UseVisualStyleBackColor = true;
            _ok.Click += Ok_Click;
            //
            // cancel
            //
            cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancel.Location = new System.Drawing.Point(675, 445);
            cancel.Name = "cancel";
            cancel.Size = new System.Drawing.Size(76, 26);
            cancel.TabIndex = 7;
            cancel.Text = "Cancel";
            cancel.UseVisualStyleBackColor = true;
            //
            // writePanel
            //
            writePanel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            writePanel.Controls.Add(writeLabel);
            writePanel.Controls.Add(_writeMul);
            writePanel.Controls.Add(_writeUop);
            writePanel.Controls.Add(_writeNone);
            writePanel.Location = new System.Drawing.Point(10, 410);
            writePanel.Name = "writePanel";
            writePanel.Size = new System.Drawing.Size(740, 26);
            writePanel.TabIndex = 8;
            //
            // writeLabel
            //
            writeLabel.AutoSize = true;
            writeLabel.Location = new System.Drawing.Point(0, 5);
            writeLabel.Name = "writeLabel";
            writeLabel.Size = new System.Drawing.Size(54, 15);
            writeLabel.TabIndex = 0;
            writeLabel.Text = "Write to:";
            //
            // _writeMul
            //
            _writeMul.AutoSize = true;
            _writeMul.Location = new System.Drawing.Point(60, 3);
            _writeMul.Name = "_writeMul";
            _writeMul.Size = new System.Drawing.Size(140, 19);
            _writeMul.TabIndex = 1;
            _writeMul.Text = "multi.mul + multi.idx";
            _writeMul.UseVisualStyleBackColor = true;
            //
            // _writeUop
            //
            _writeUop.AutoSize = true;
            _writeUop.Location = new System.Drawing.Point(206, 3);
            _writeUop.Name = "_writeUop";
            _writeUop.Size = new System.Drawing.Size(134, 19);
            _writeUop.TabIndex = 2;
            _writeUop.Text = "MultiCollection.uop";
            _writeUop.UseVisualStyleBackColor = true;
            //
            // _writeNone
            //
            _writeNone.AutoSize = true;
            _writeNone.Location = new System.Drawing.Point(346, 3);
            _writeNone.Name = "_writeNone";
            _writeNone.Size = new System.Drawing.Size(176, 19);
            _writeNone.TabIndex = 3;
            _writeNone.Text = "Not yet (Multis tab > Save)";
            _writeNone.UseVisualStyleBackColor = true;
            //
            // filterAll
            //
            filterAll.Appearance = System.Windows.Forms.Appearance.Button;
            filterAll.Location = new System.Drawing.Point(352, 8);
            filterAll.Name = "filterAll";
            filterAll.Size = new System.Drawing.Size(44, 24);
            filterAll.TabIndex = 9;
            filterAll.Text = "All";
            filterAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            filterAll.UseVisualStyleBackColor = true;
            filterAll.CheckedChanged += FilterButton_CheckedChanged;
            //
            // filterFree
            //
            filterFree.Appearance = System.Windows.Forms.Appearance.Button;
            filterFree.Location = new System.Drawing.Point(398, 8);
            filterFree.Name = "filterFree";
            filterFree.Size = new System.Drawing.Size(96, 24);
            filterFree.TabIndex = 10;
            filterFree.Text = "Free";
            filterFree.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            filterFree.UseVisualStyleBackColor = true;
            filterFree.CheckedChanged += FilterButton_CheckedChanged;
            //
            // filterUsed
            //
            filterUsed.Appearance = System.Windows.Forms.Appearance.Button;
            filterUsed.Location = new System.Drawing.Point(496, 8);
            filterUsed.Name = "filterUsed";
            filterUsed.Size = new System.Drawing.Size(96, 24);
            filterUsed.TabIndex = 11;
            filterUsed.Text = "Used";
            filterUsed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            filterUsed.UseVisualStyleBackColor = true;
            filterUsed.CheckedChanged += FilterButton_CheckedChanged;
            //
            // DocumentIOSaveDialog
            //
            AcceptButton = _ok;
            CancelButton = cancel;
            ClientSize = new System.Drawing.Size(760, 480);
            Controls.Add(idLabel);
            Controls.Add(_id);
            Controls.Add(nextFree);
            Controls.Add(_list);
            Controls.Add(_preview);
            Controls.Add(_info);
            Controls.Add(_ok);
            Controls.Add(cancel);
            Controls.Add(writePanel);
            Controls.Add(filterAll);
            Controls.Add(filterFree);
            Controls.Add(filterUsed);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(700, 340);
            Name = "DocumentIOSaveDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Save multi to client";
            ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
            writePanel.ResumeLayout(false);
            writePanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label idLabel;
        private System.Windows.Forms.TextBox _id;
        private System.Windows.Forms.Button nextFree;
        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader columnHeaderId;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderHolds;
        private System.Windows.Forms.PictureBox _preview;
        private System.Windows.Forms.Label _info;
        private System.Windows.Forms.Button _ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Panel writePanel;
        private System.Windows.Forms.Label writeLabel;
        private System.Windows.Forms.RadioButton _writeMul;
        private System.Windows.Forms.RadioButton _writeUop;
        private System.Windows.Forms.RadioButton _writeNone;
        private System.Windows.Forms.RadioButton filterAll;
        private System.Windows.Forms.RadioButton filterFree;
        private System.Windows.Forms.RadioButton filterUsed;
    }
}
