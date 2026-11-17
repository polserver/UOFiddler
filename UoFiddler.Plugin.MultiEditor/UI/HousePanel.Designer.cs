namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class HousePanel
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
            houseBox = new System.Windows.Forms.GroupBox();
            _wingList = new System.Windows.Forms.ListBox();
            _wingHint = new System.Windows.Forms.Label();
            _storeysLabel = new System.Windows.Forms.Label();
            _storeys = new System.Windows.Forms.NumericUpDown();
            _removeWing = new System.Windows.Forms.Button();
            _roofLabel = new System.Windows.Forms.Label();
            _roofShape = new System.Windows.Forms.ComboBox();
            _ridge = new System.Windows.Forms.ComboBox();
            _materialsLabel = new System.Windows.Forms.Label();
            _storey1 = new System.Windows.Forms.Label();
            _wall1 = new System.Windows.Forms.Button();
            _floor1 = new System.Windows.Forms.Button();
            _storey2 = new System.Windows.Forms.Label();
            _wall2 = new System.Windows.Forms.Button();
            _floor2 = new System.Windows.Forms.Button();
            _storey3 = new System.Windows.Forms.Label();
            _wall3 = new System.Windows.Forms.Button();
            _floor3 = new System.Windows.Forms.Button();
            _storey4 = new System.Windows.Forms.Label();
            _wall4 = new System.Windows.Forms.Button();
            _floor4 = new System.Windows.Forms.Button();
            _roofStyleLabel = new System.Windows.Forms.Label();
            _roofStyle = new System.Windows.Forms.Button();
            _foundation = new System.Windows.Forms.CheckBox();
            _baseZLabel = new System.Windows.Forms.Label();
            _baseZ = new System.Windows.Forms.NumericUpDown();
            _undo = new System.Windows.Forms.Button();
            _redo = new System.Windows.Forms.Button();
            _place = new System.Windows.Forms.Button();
            _drop = new System.Windows.Forms.Button();
            _status = new System.Windows.Forms.Label();
            houseBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_storeys).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_baseZ).BeginInit();
            SuspendLayout();
            //
            // houseBox
            //
            houseBox.Controls.Add(_wingList);
            houseBox.Controls.Add(_wingHint);
            houseBox.Controls.Add(_storeysLabel);
            houseBox.Controls.Add(_storeys);
            houseBox.Controls.Add(_removeWing);
            houseBox.Controls.Add(_roofLabel);
            houseBox.Controls.Add(_roofShape);
            houseBox.Controls.Add(_ridge);
            houseBox.Controls.Add(_materialsLabel);
            houseBox.Controls.Add(_storey1);
            houseBox.Controls.Add(_wall1);
            houseBox.Controls.Add(_floor1);
            houseBox.Controls.Add(_storey2);
            houseBox.Controls.Add(_wall2);
            houseBox.Controls.Add(_floor2);
            houseBox.Controls.Add(_storey3);
            houseBox.Controls.Add(_wall3);
            houseBox.Controls.Add(_floor3);
            houseBox.Controls.Add(_storey4);
            houseBox.Controls.Add(_wall4);
            houseBox.Controls.Add(_floor4);
            houseBox.Controls.Add(_roofStyleLabel);
            houseBox.Controls.Add(_roofStyle);
            houseBox.Controls.Add(_foundation);
            houseBox.Controls.Add(_baseZLabel);
            houseBox.Controls.Add(_baseZ);
            houseBox.Controls.Add(_undo);
            houseBox.Controls.Add(_redo);
            houseBox.Controls.Add(_place);
            houseBox.Controls.Add(_drop);
            houseBox.Controls.Add(_status);
            houseBox.Dock = System.Windows.Forms.DockStyle.Fill;
            houseBox.Location = new System.Drawing.Point(0, 0);
            houseBox.Name = "houseBox";
            houseBox.Size = new System.Drawing.Size(250, 430);
            houseBox.TabIndex = 0;
            houseBox.TabStop = false;
            houseBox.Text = "House draft";
            //
            // _wingList
            //
            _wingList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _wingList.FormattingEnabled = true;
            _wingList.IntegralHeight = false;
            _wingList.Location = new System.Drawing.Point(6, 20);
            _wingList.Name = "_wingList";
            _wingList.Size = new System.Drawing.Size(238, 64);
            _wingList.TabIndex = 0;
            _wingList.SelectedIndexChanged += WingList_SelectedIndexChanged;
            //
            // _wingHint
            //
            _wingHint.AutoSize = true;
            _wingHint.Location = new System.Drawing.Point(6, 90);
            _wingHint.Name = "_wingHint";
            _wingHint.Size = new System.Drawing.Size(120, 15);
            _wingHint.TabIndex = 1;
            _wingHint.Text = "New wings:";
            //
            // _storeysLabel
            //
            _storeysLabel.AutoSize = true;
            _storeysLabel.Location = new System.Drawing.Point(6, 113);
            _storeysLabel.Name = "_storeysLabel";
            _storeysLabel.Size = new System.Drawing.Size(50, 15);
            _storeysLabel.TabIndex = 2;
            _storeysLabel.Text = "Storeys";
            //
            // _storeys
            //
            _storeys.Maximum = new decimal(new int[] { 4, 0, 0, 0 });
            _storeys.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _storeys.Value = new decimal(new int[] { 1, 0, 0, 0 });
            _storeys.Location = new System.Drawing.Point(60, 110);
            _storeys.Name = "_storeys";
            _storeys.Size = new System.Drawing.Size(44, 23);
            _storeys.TabIndex = 3;
            _storeys.ValueChanged += Storeys_ValueChanged;
            //
            // _removeWing
            //
            _removeWing.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            _removeWing.UseVisualStyleBackColor = true;
            _removeWing.Location = new System.Drawing.Point(150, 109);
            _removeWing.Name = "_removeWing";
            _removeWing.Size = new System.Drawing.Size(94, 25);
            _removeWing.TabIndex = 4;
            _removeWing.Text = "Remove wing";
            _removeWing.Click += RemoveWing_Click;
            //
            // _roofLabel
            //
            _roofLabel.AutoSize = true;
            _roofLabel.Location = new System.Drawing.Point(6, 143);
            _roofLabel.Name = "_roofLabel";
            _roofLabel.Size = new System.Drawing.Size(50, 15);
            _roofLabel.TabIndex = 5;
            _roofLabel.Text = "Roof";
            //
            // _roofShape
            //
            _roofShape.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _roofShape.FormattingEnabled = true;
            _roofShape.Items.AddRange(new object[] { "Gable", "None" });
            _roofShape.Location = new System.Drawing.Point(60, 140);
            _roofShape.Name = "_roofShape";
            _roofShape.Size = new System.Drawing.Size(80, 23);
            _roofShape.TabIndex = 6;
            _roofShape.SelectedIndexChanged += RoofShape_SelectedIndexChanged;
            //
            // _ridge
            //
            _ridge.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _ridge.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _ridge.FormattingEnabled = true;
            _ridge.Items.AddRange(new object[] { "Ridge: auto", "Ridge along X", "Ridge along Y" });
            _ridge.Location = new System.Drawing.Point(146, 140);
            _ridge.Name = "_ridge";
            _ridge.Size = new System.Drawing.Size(98, 23);
            _ridge.TabIndex = 7;
            _ridge.SelectedIndexChanged += Ridge_SelectedIndexChanged;
            //
            // _materialsLabel
            //
            _materialsLabel.Location = new System.Drawing.Point(6, 172);
            _materialsLabel.Name = "_materialsLabel";
            _materialsLabel.Size = new System.Drawing.Size(238, 15);
            _materialsLabel.TabIndex = 8;
            _materialsLabel.Text = "Storey: walls and floor style";
            //
            // _storey1
            //
            _storey1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _storey1.Location = new System.Drawing.Point(6, 196);
            _storey1.Name = "_storey1";
            _storey1.Size = new System.Drawing.Size(18, 17);
            _storey1.TabIndex = 9;
            _storey1.Text = "1";
            //
            // _wall1
            //
            _wall1.UseVisualStyleBackColor = true;
            _wall1.Location = new System.Drawing.Point(28, 192);
            _wall1.Name = "_wall1";
            _wall1.Size = new System.Drawing.Size(106, 25);
            _wall1.TabIndex = 10;
            _wall1.Text = "(walls)";
            //
            // _floor1
            //
            _floor1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _floor1.UseVisualStyleBackColor = true;
            _floor1.Location = new System.Drawing.Point(138, 192);
            _floor1.Name = "_floor1";
            _floor1.Size = new System.Drawing.Size(106, 25);
            _floor1.TabIndex = 11;
            _floor1.Text = "(floor)";
            //
            // _storey2
            //
            _storey2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _storey2.Location = new System.Drawing.Point(6, 223);
            _storey2.Name = "_storey2";
            _storey2.Size = new System.Drawing.Size(18, 17);
            _storey2.TabIndex = 12;
            _storey2.Text = "2";
            //
            // _wall2
            //
            _wall2.UseVisualStyleBackColor = true;
            _wall2.Location = new System.Drawing.Point(28, 219);
            _wall2.Name = "_wall2";
            _wall2.Size = new System.Drawing.Size(106, 25);
            _wall2.TabIndex = 13;
            _wall2.Text = "(walls)";
            //
            // _floor2
            //
            _floor2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _floor2.UseVisualStyleBackColor = true;
            _floor2.Location = new System.Drawing.Point(138, 219);
            _floor2.Name = "_floor2";
            _floor2.Size = new System.Drawing.Size(106, 25);
            _floor2.TabIndex = 14;
            _floor2.Text = "(floor)";
            //
            // _storey3
            //
            _storey3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _storey3.Location = new System.Drawing.Point(6, 250);
            _storey3.Name = "_storey3";
            _storey3.Size = new System.Drawing.Size(18, 17);
            _storey3.TabIndex = 15;
            _storey3.Text = "3";
            //
            // _wall3
            //
            _wall3.UseVisualStyleBackColor = true;
            _wall3.Location = new System.Drawing.Point(28, 246);
            _wall3.Name = "_wall3";
            _wall3.Size = new System.Drawing.Size(106, 25);
            _wall3.TabIndex = 16;
            _wall3.Text = "(walls)";
            //
            // _floor3
            //
            _floor3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _floor3.UseVisualStyleBackColor = true;
            _floor3.Location = new System.Drawing.Point(138, 246);
            _floor3.Name = "_floor3";
            _floor3.Size = new System.Drawing.Size(106, 25);
            _floor3.TabIndex = 17;
            _floor3.Text = "(floor)";
            //
            // _storey4
            //
            _storey4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _storey4.Location = new System.Drawing.Point(6, 277);
            _storey4.Name = "_storey4";
            _storey4.Size = new System.Drawing.Size(18, 17);
            _storey4.TabIndex = 18;
            _storey4.Text = "4";
            //
            // _wall4
            //
            _wall4.UseVisualStyleBackColor = true;
            _wall4.Location = new System.Drawing.Point(28, 273);
            _wall4.Name = "_wall4";
            _wall4.Size = new System.Drawing.Size(106, 25);
            _wall4.TabIndex = 19;
            _wall4.Text = "(walls)";
            //
            // _floor4
            //
            _floor4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _floor4.UseVisualStyleBackColor = true;
            _floor4.Location = new System.Drawing.Point(138, 273);
            _floor4.Name = "_floor4";
            _floor4.Size = new System.Drawing.Size(106, 25);
            _floor4.TabIndex = 20;
            _floor4.Text = "(floor)";
            //
            // _roofStyleLabel
            //
            _roofStyleLabel.AutoSize = true;
            _roofStyleLabel.Location = new System.Drawing.Point(6, 305);
            _roofStyleLabel.Name = "_roofStyleLabel";
            _roofStyleLabel.Size = new System.Drawing.Size(50, 15);
            _roofStyleLabel.TabIndex = 21;
            _roofStyleLabel.Text = "Roof";
            //
            // _roofStyle
            //
            _roofStyle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _roofStyle.UseVisualStyleBackColor = true;
            _roofStyle.Location = new System.Drawing.Point(60, 300);
            _roofStyle.Name = "_roofStyle";
            _roofStyle.Size = new System.Drawing.Size(184, 25);
            _roofStyle.TabIndex = 22;
            _roofStyle.Text = "(roof)";
            //
            // _foundation
            //
            _foundation.AutoSize = true;
            _foundation.UseVisualStyleBackColor = true;
            _foundation.Location = new System.Drawing.Point(6, 332);
            _foundation.Name = "_foundation";
            _foundation.Size = new System.Drawing.Size(100, 19);
            _foundation.TabIndex = 23;
            _foundation.Text = "Foundation";
            _foundation.CheckedChanged += Foundation_CheckedChanged;
            //
            // _baseZLabel
            //
            _baseZLabel.AutoSize = true;
            _baseZLabel.Location = new System.Drawing.Point(126, 334);
            _baseZLabel.Name = "_baseZLabel";
            _baseZLabel.Size = new System.Drawing.Size(44, 15);
            _baseZLabel.TabIndex = 24;
            _baseZLabel.Text = "Base Z";
            //
            // _baseZ
            //
            _baseZ.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            _baseZ.Minimum = new decimal(new int[] { 100, 0, 0, int.MinValue });
            _baseZ.Location = new System.Drawing.Point(174, 331);
            _baseZ.Name = "_baseZ";
            _baseZ.Size = new System.Drawing.Size(56, 23);
            _baseZ.TabIndex = 25;
            _baseZ.ValueChanged += BaseZ_ValueChanged;
            //
            // _undo
            //
            _undo.UseVisualStyleBackColor = true;
            _undo.Location = new System.Drawing.Point(6, 362);
            _undo.Name = "_undo";
            _undo.Size = new System.Drawing.Size(54, 25);
            _undo.TabIndex = 26;
            _undo.Text = "Undo";
            _undo.Click += Undo_Click;
            //
            // _redo
            //
            _redo.UseVisualStyleBackColor = true;
            _redo.Location = new System.Drawing.Point(62, 362);
            _redo.Name = "_redo";
            _redo.Size = new System.Drawing.Size(54, 25);
            _redo.TabIndex = 27;
            _redo.Text = "Redo";
            _redo.Click += Redo_Click;
            //
            // _place
            //
            _place.UseVisualStyleBackColor = true;
            _place.Location = new System.Drawing.Point(122, 362);
            _place.Name = "_place";
            _place.Size = new System.Drawing.Size(72, 25);
            _place.TabIndex = 28;
            _place.Text = "Place";
            _place.Click += Place_Click;
            //
            // _drop
            //
            _drop.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _drop.UseVisualStyleBackColor = true;
            _drop.Location = new System.Drawing.Point(196, 362);
            _drop.Name = "_drop";
            _drop.Size = new System.Drawing.Size(48, 25);
            _drop.TabIndex = 29;
            _drop.Text = "Drop";
            _drop.Click += Drop_Click;
            //
            // _status
            //
            _status.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            _status.Location = new System.Drawing.Point(6, 392);
            _status.Name = "_status";
            _status.Size = new System.Drawing.Size(238, 32);
            _status.TabIndex = 30;
            //
            // HousePanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(houseBox);
            Name = "HousePanel";
            Size = new System.Drawing.Size(250, 430);
            houseBox.ResumeLayout(false);
            houseBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)_storeys).EndInit();
            ((System.ComponentModel.ISupportInitialize)_baseZ).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox houseBox;
        private System.Windows.Forms.ListBox _wingList;
        private System.Windows.Forms.Label _wingHint;
        private System.Windows.Forms.Label _storeysLabel;
        private System.Windows.Forms.NumericUpDown _storeys;
        private System.Windows.Forms.Button _removeWing;
        private System.Windows.Forms.Label _roofLabel;
        private System.Windows.Forms.ComboBox _roofShape;
        private System.Windows.Forms.ComboBox _ridge;
        private System.Windows.Forms.Label _materialsLabel;
        private System.Windows.Forms.Label _storey1;
        private System.Windows.Forms.Button _wall1;
        private System.Windows.Forms.Button _floor1;
        private System.Windows.Forms.Label _storey2;
        private System.Windows.Forms.Button _wall2;
        private System.Windows.Forms.Button _floor2;
        private System.Windows.Forms.Label _storey3;
        private System.Windows.Forms.Button _wall3;
        private System.Windows.Forms.Button _floor3;
        private System.Windows.Forms.Label _storey4;
        private System.Windows.Forms.Button _wall4;
        private System.Windows.Forms.Button _floor4;
        private System.Windows.Forms.Label _roofStyleLabel;
        private System.Windows.Forms.Button _roofStyle;
        private System.Windows.Forms.CheckBox _foundation;
        private System.Windows.Forms.Label _baseZLabel;
        private System.Windows.Forms.NumericUpDown _baseZ;
        private System.Windows.Forms.Button _undo;
        private System.Windows.Forms.Button _redo;
        private System.Windows.Forms.Button _place;
        private System.Windows.Forms.Button _drop;
        private System.Windows.Forms.Label _status;
    }
}
