namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class HouseStyleEditorDialog
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
            _kind = new System.Windows.Forms.ComboBox();
            _styleList = new System.Windows.Forms.ListBox();
            _up = new System.Windows.Forms.Button();
            _down = new System.Windows.Forms.Button();
            _reset = new System.Windows.Forms.Button();
            _nameLabel = new System.Windows.Forms.Label();
            _name = new System.Windows.Forms.TextBox();
            _groupLabel = new System.Windows.Forms.Label();
            _group = new System.Windows.Forms.TextBox();
            _familyLabel = new System.Windows.Forms.Label();
            _family = new System.Windows.Forms.TextBox();
            _hidden = new System.Windows.Forms.CheckBox();
            _layoutLabel = new System.Windows.Forms.Label();
            _layout = new System.Windows.Forms.ComboBox();
            _pieces = new System.Windows.Forms.ListView();
            _role = new System.Windows.Forms.ComboBox();
            _pieceId = new System.Windows.Forms.TextBox();
            _fromBrowser = new System.Windows.Forms.Button();
            _setPiece = new System.Windows.Forms.Button();
            _removePiece = new System.Windows.Forms.Button();
            _preview = new System.Windows.Forms.PictureBox();
            _status = new System.Windows.Forms.Label();
            _save = new System.Windows.Forms.Button();
            _close = new System.Windows.Forms.Button();
            _fileHint = new System.Windows.Forms.Label();
            _roleColumn = new System.Windows.Forms.ColumnHeader();
            _tileColumn = new System.Windows.Forms.ColumnHeader();
            _tileNameColumn = new System.Windows.Forms.ColumnHeader();
            _pieceImages = new System.Windows.Forms.ImageList(components);
            _toolTip = new System.Windows.Forms.ToolTip(components);
            ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
            SuspendLayout();
            //
            // _kind
            //
            _kind.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _kind.FormattingEnabled = true;
            _kind.Location = new System.Drawing.Point(8, 8);
            _kind.Name = "_kind";
            _kind.Size = new System.Drawing.Size(260, 23);
            _kind.TabIndex = 0;
            _kind.SelectedIndexChanged += Kind_SelectedIndexChanged;
            //
            // _styleList
            //
            _styleList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _styleList.IntegralHeight = false;
            _styleList.ItemHeight = 15;
            _styleList.FormattingEnabled = true;
            _styleList.Location = new System.Drawing.Point(8, 37);
            _styleList.Name = "_styleList";
            _styleList.Size = new System.Drawing.Size(260, 436);
            _styleList.TabIndex = 1;
            _styleList.SelectedIndexChanged += StyleList_SelectedIndexChanged;
            //
            // _up
            //
            _up.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _up.Location = new System.Drawing.Point(8, 480);
            _up.Name = "_up";
            _up.Size = new System.Drawing.Size(60, 25);
            _up.TabIndex = 2;
            _up.Text = "Up";
            _up.UseVisualStyleBackColor = true;
            _toolTip.SetToolTip(_up, "Moves the style up in the lists.");
            _up.Click += Up_Click;
            //
            // _down
            //
            _down.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _down.Location = new System.Drawing.Point(72, 480);
            _down.Name = "_down";
            _down.Size = new System.Drawing.Size(60, 25);
            _down.TabIndex = 3;
            _down.Text = "Down";
            _down.UseVisualStyleBackColor = true;
            _toolTip.SetToolTip(_down, "Moves the style down in the lists.");
            _down.Click += Down_Click;
            //
            // _reset
            //
            _reset.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _reset.Location = new System.Drawing.Point(136, 480);
            _reset.Name = "_reset";
            _reset.Size = new System.Drawing.Size(132, 25);
            _reset.TabIndex = 4;
            _reset.Text = "Back to shipped";
            _reset.UseVisualStyleBackColor = true;
            _toolTip.SetToolTip(_reset, "Drops your changes to this style: it is again as the shipped style file has it.");
            _reset.Click += Reset_Click;
            //
            // _nameLabel
            //
            _nameLabel.AutoSize = true;
            _nameLabel.Location = new System.Drawing.Point(280, 11);
            _nameLabel.Name = "_nameLabel";
            _nameLabel.Size = new System.Drawing.Size(50, 15);
            _nameLabel.TabIndex = 5;
            _nameLabel.Text = "Name";
            //
            // _name
            //
            _name.Location = new System.Drawing.Point(336, 8);
            _name.Name = "_name";
            _name.Size = new System.Drawing.Size(224, 23);
            _name.TabIndex = 6;
            _name.TextChanged += Meta_Changed;
            //
            // _groupLabel
            //
            _groupLabel.AutoSize = true;
            _groupLabel.Location = new System.Drawing.Point(280, 40);
            _groupLabel.Name = "_groupLabel";
            _groupLabel.Size = new System.Drawing.Size(50, 15);
            _groupLabel.TabIndex = 7;
            _groupLabel.Text = "Group";
            //
            // _group
            //
            _group.Location = new System.Drawing.Point(336, 37);
            _group.Name = "_group";
            _group.Size = new System.Drawing.Size(224, 23);
            _group.TabIndex = 8;
            _toolTip.SetToolTip(_group, "The heading the style is listed under.");
            _group.TextChanged += Meta_Changed;
            //
            // _familyLabel
            //
            _familyLabel.AutoSize = true;
            _familyLabel.Location = new System.Drawing.Point(280, 69);
            _familyLabel.Name = "_familyLabel";
            _familyLabel.Size = new System.Drawing.Size(50, 15);
            _familyLabel.TabIndex = 9;
            _familyLabel.Text = "Family";
            //
            // _family
            //
            _family.Location = new System.Drawing.Point(336, 66);
            _family.Name = "_family";
            _family.Size = new System.Drawing.Size(224, 23);
            _family.TabIndex = 10;
            _toolTip.SetToolTip(_family, "The material, shared by styles of every kind. The House page finds styles by it.");
            _family.TextChanged += Meta_Changed;
            //
            // _hidden
            //
            _hidden.AutoSize = true;
            _hidden.Location = new System.Drawing.Point(336, 95);
            _hidden.Name = "_hidden";
            _hidden.Size = new System.Drawing.Size(120, 19);
            _hidden.TabIndex = 11;
            _hidden.Text = "Hidden";
            _hidden.UseVisualStyleBackColor = true;
            _toolTip.SetToolTip(_hidden, "Keeps the style out of the lists. Its pieces stay in the tile browser.");
            _hidden.CheckedChanged += Meta_Changed;
            //
            // _layoutLabel
            //
            _layoutLabel.AutoSize = true;
            _layoutLabel.Location = new System.Drawing.Point(280, 122);
            _layoutLabel.Name = "_layoutLabel";
            _layoutLabel.Size = new System.Drawing.Size(50, 15);
            _layoutLabel.TabIndex = 12;
            _layoutLabel.Text = "Layout";
            //
            // _layout
            //
            _layout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _layout.FormattingEnabled = true;
            _layout.Items.AddRange(new object[] { "Run", "Paired", "Cycle" });
            _layout.Location = new System.Drawing.Point(336, 119);
            _layout.Name = "_layout";
            _layout.Size = new System.Drawing.Size(120, 23);
            _layout.TabIndex = 13;
            _toolTip.SetToolTip(_layout, "Walls only. Run: start, middle, end. Paired: South1+South3 and East3+East1 are two halves laid pair after pair. Cycle: CycleSouth1.. and CycleEast1.. laid one after another.");
            _layout.SelectedIndexChanged += Meta_Changed;
            //
            // _pieces
            //
            _pieces.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _pieces.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { _roleColumn, _tileColumn, _tileNameColumn });
            _pieces.SmallImageList = _pieceImages;
            _pieces.FullRowSelect = true;
            _pieces.HideSelection = false;
            _pieces.MultiSelect = false;
            _pieces.View = System.Windows.Forms.View.Details;
            _pieces.Location = new System.Drawing.Point(280, 150);
            _pieces.Name = "_pieces";
            _pieces.Size = new System.Drawing.Size(280, 293);
            _pieces.TabIndex = 14;
            _pieces.SelectedIndexChanged += Pieces_SelectedIndexChanged;
            //
            // _role
            //
            _role.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _role.FormattingEnabled = true;
            _role.Location = new System.Drawing.Point(280, 450);
            _role.Name = "_role";
            _role.Size = new System.Drawing.Size(118, 23);
            _role.TabIndex = 15;
            _toolTip.SetToolTip(_role, "The column of the piece: South1-3, East1-3, Corner, Post, WindowS, WindowE ...");
            //
            // _pieceId
            //
            _pieceId.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _pieceId.Location = new System.Drawing.Point(402, 450);
            _pieceId.Name = "_pieceId";
            _pieceId.Size = new System.Drawing.Size(62, 23);
            _pieceId.TabIndex = 16;
            _toolTip.SetToolTip(_pieceId, "Tile id of the piece, as 0x1234.");
            //
            // _fromBrowser
            //
            _fromBrowser.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _fromBrowser.Location = new System.Drawing.Point(466, 449);
            _fromBrowser.Name = "_fromBrowser";
            _fromBrowser.Size = new System.Drawing.Size(94, 25);
            _fromBrowser.TabIndex = 17;
            _fromBrowser.Text = "Drawn tile";
            _fromBrowser.UseVisualStyleBackColor = true;
            _toolTip.SetToolTip(_fromBrowser, "Takes the tile chosen in the tile browser.");
            _fromBrowser.Click += FromBrowser_Click;
            //
            // _setPiece
            //
            _setPiece.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _setPiece.Location = new System.Drawing.Point(280, 480);
            _setPiece.Name = "_setPiece";
            _setPiece.Size = new System.Drawing.Size(118, 25);
            _setPiece.TabIndex = 18;
            _setPiece.Text = "Set piece";
            _setPiece.UseVisualStyleBackColor = true;
            _toolTip.SetToolTip(_setPiece, "Gives the role this tile: a new piece, or another tile for one the style has.");
            _setPiece.Click += SetPiece_Click;
            //
            // _removePiece
            //
            _removePiece.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            _removePiece.Location = new System.Drawing.Point(402, 480);
            _removePiece.Name = "_removePiece";
            _removePiece.Size = new System.Drawing.Size(158, 25);
            _removePiece.TabIndex = 19;
            _removePiece.Text = "Remove piece";
            _removePiece.UseVisualStyleBackColor = true;
            _removePiece.Click += RemovePiece_Click;
            //
            // _preview
            //
            _preview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _preview.Location = new System.Drawing.Point(572, 8);
            _preview.Name = "_preview";
            _preview.Size = new System.Drawing.Size(320, 465);
            _preview.TabIndex = 20;
            _preview.TabStop = false;
            //
            // _status
            //
            _status.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _status.Location = new System.Drawing.Point(572, 480);
            _status.Name = "_status";
            _status.Size = new System.Drawing.Size(320, 30);
            _status.TabIndex = 21;
            //
            // _save
            //
            _save.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _save.Location = new System.Drawing.Point(734, 516);
            _save.Name = "_save";
            _save.Size = new System.Drawing.Size(76, 27);
            _save.TabIndex = 22;
            _save.Text = "Save";
            _save.UseVisualStyleBackColor = true;
            _toolTip.SetToolTip(_save, "Writes your changes to your copy of the style file and reads the styles again.");
            _save.Click += Save_Click;
            //
            // _close
            //
            _close.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            _close.Location = new System.Drawing.Point(816, 516);
            _close.Name = "_close";
            _close.Size = new System.Drawing.Size(76, 27);
            _close.TabIndex = 23;
            _close.Text = "Close";
            _close.UseVisualStyleBackColor = true;
            _close.Click += Close_Click;
            //
            // _fileHint
            //
            _fileHint.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _fileHint.Location = new System.Drawing.Point(8, 520);
            _fileHint.Name = "_fileHint";
            _fileHint.Size = new System.Drawing.Size(700, 15);
            _fileHint.TabIndex = 24;
            //
            // _roleColumn
            //
            _roleColumn.Text = "Role";
            _roleColumn.Width = 104;
            //
            // _tileColumn
            //
            _tileColumn.Text = "Tile";
            _tileColumn.Width = 60;
            //
            // _tileNameColumn
            //
            _tileNameColumn.Text = "Name";
            _tileNameColumn.Width = 92;
            //
            // _pieceImages
            //
            _pieceImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            _pieceImages.ImageSize = new System.Drawing.Size(32, 32);
            _pieceImages.TransparentColor = System.Drawing.Color.Transparent;
            //
            // HouseStyleEditorDialog
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(900, 551);
            Controls.Add(_kind);
            Controls.Add(_styleList);
            Controls.Add(_up);
            Controls.Add(_down);
            Controls.Add(_reset);
            Controls.Add(_nameLabel);
            Controls.Add(_name);
            Controls.Add(_groupLabel);
            Controls.Add(_group);
            Controls.Add(_familyLabel);
            Controls.Add(_family);
            Controls.Add(_hidden);
            Controls.Add(_layoutLabel);
            Controls.Add(_layout);
            Controls.Add(_pieces);
            Controls.Add(_role);
            Controls.Add(_pieceId);
            Controls.Add(_fromBrowser);
            Controls.Add(_setPiece);
            Controls.Add(_removePiece);
            Controls.Add(_preview);
            Controls.Add(_status);
            Controls.Add(_save);
            Controls.Add(_close);
            Controls.Add(_fileHint);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(916, 500);
            Name = "HouseStyleEditorDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "House styles";
            FormClosing += Dialog_FormClosing;
            ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ComboBox _kind;
        private System.Windows.Forms.ListBox _styleList;
        private System.Windows.Forms.Button _up;
        private System.Windows.Forms.Button _down;
        private System.Windows.Forms.Button _reset;
        private System.Windows.Forms.Label _nameLabel;
        private System.Windows.Forms.TextBox _name;
        private System.Windows.Forms.Label _groupLabel;
        private System.Windows.Forms.TextBox _group;
        private System.Windows.Forms.Label _familyLabel;
        private System.Windows.Forms.TextBox _family;
        private System.Windows.Forms.CheckBox _hidden;
        private System.Windows.Forms.Label _layoutLabel;
        private System.Windows.Forms.ComboBox _layout;
        private System.Windows.Forms.ListView _pieces;
        private System.Windows.Forms.ComboBox _role;
        private System.Windows.Forms.TextBox _pieceId;
        private System.Windows.Forms.Button _fromBrowser;
        private System.Windows.Forms.Button _setPiece;
        private System.Windows.Forms.Button _removePiece;
        private System.Windows.Forms.PictureBox _preview;
        private System.Windows.Forms.Label _status;
        private System.Windows.Forms.Button _save;
        private System.Windows.Forms.Button _close;
        private System.Windows.Forms.Label _fileHint;
        private System.Windows.Forms.ColumnHeader _roleColumn;
        private System.Windows.Forms.ColumnHeader _tileColumn;
        private System.Windows.Forms.ColumnHeader _tileNameColumn;
        private System.Windows.Forms.ImageList _pieceImages;
        private System.Windows.Forms.ToolTip _toolTip;
    }
}
