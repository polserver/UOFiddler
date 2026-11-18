namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class NewMultiDialog
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
            _custom = new System.Windows.Forms.RadioButton();
            _foundationCombo = new System.Windows.Forms.ComboBox();
            customHint = new System.Windows.Forms.Label();
            _blankFoundation = new System.Windows.Forms.RadioButton();
            _blankGround = new System.Windows.Forms.RadioButton();
            sizeLabel = new System.Windows.Forms.Label();
            _width = new System.Windows.Forms.NumericUpDown();
            byLabel = new System.Windows.Forms.Label();
            _height = new System.Windows.Forms.NumericUpDown();
            _presetLabel = new System.Windows.Forms.Label();
            _sizePreset = new System.Windows.Forms.ComboBox();
            _customLabel = new System.Windows.Forms.Label();
            _cellsLabel = new System.Windows.Forms.Label();
            buttonOk = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)_width).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_height).BeginInit();
            SuspendLayout();
            //
            // _custom
            //
            _custom.AutoSize = true;
            _custom.Location = new System.Drawing.Point(12, 40);
            _custom.Name = "_custom";
            _custom.Size = new System.Drawing.Size(222, 19);
            _custom.TabIndex = 1;
            _custom.Text = "Custom house on a client foundation";
            _custom.UseVisualStyleBackColor = true;
            _custom.CheckedChanged += Kind_CheckedChanged;
            //
            // _foundationCombo
            //
            _foundationCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _foundationCombo.Location = new System.Drawing.Point(32, 64);
            _foundationCombo.Name = "_foundationCombo";
            _foundationCombo.Size = new System.Drawing.Size(250, 23);
            _foundationCombo.TabIndex = 2;
            //
            // customHint
            //
            customHint.AutoSize = true;
            customHint.ForeColor = System.Drawing.SystemColors.GrayText;
            customHint.Location = new System.Drawing.Point(32, 90);
            customHint.Name = "customHint";
            customHint.Size = new System.Drawing.Size(329, 15);
            customHint.TabIndex = 3;
            customHint.Text = "Starts with the foundation's tiles. Floors are at Z 7, 27, 47, 67.";
            //
            // _blankFoundation
            //
            _blankFoundation.AutoSize = true;
            _blankFoundation.Location = new System.Drawing.Point(12, 116);
            _blankFoundation.Name = "_blankFoundation";
            _blankFoundation.Size = new System.Drawing.Size(266, 19);
            _blankFoundation.TabIndex = 4;
            _blankFoundation.Text = "Blank, first floor at Z 7 (as on a foundation)";
            _blankFoundation.UseVisualStyleBackColor = true;
            _blankFoundation.CheckedChanged += Kind_CheckedChanged;
            //
            // _blankGround
            //
            _blankGround.AutoSize = true;
            _blankGround.Location = new System.Drawing.Point(12, 12);
            _blankGround.Name = "_blankGround";
            _blankGround.Size = new System.Drawing.Size(268, 19);
            _blankGround.TabIndex = 0;
            _blankGround.Text = "Blank, from the ground (first floor at Z 0)";
            _blankGround.UseVisualStyleBackColor = true;
            _blankGround.CheckedChanged += Kind_CheckedChanged;
            //
            // sizeLabel
            //
            sizeLabel.AutoSize = true;
            sizeLabel.Location = new System.Drawing.Point(12, 152);
            sizeLabel.Name = "sizeLabel";
            sizeLabel.Size = new System.Drawing.Size(120, 15);
            sizeLabel.TabIndex = 5;
            sizeLabel.Text = "Size of a blank multi";
            //
            // _width
            //
            _width.Location = new System.Drawing.Point(122, 205);
            _width.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            _width.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _width.Name = "_width";
            _width.Size = new System.Drawing.Size(56, 23);
            _width.TabIndex = 9;
            _width.Value = new decimal(new int[] { 10, 0, 0, 0 });
            _width.ValueChanged += Size_ValueChanged;
            //
            // byLabel
            //
            byLabel.AutoSize = true;
            byLabel.Location = new System.Drawing.Point(184, 208);
            byLabel.Name = "byLabel";
            byLabel.Size = new System.Drawing.Size(13, 15);
            byLabel.TabIndex = 10;
            byLabel.Text = "x";
            //
            // _height
            //
            _height.Location = new System.Drawing.Point(202, 205);
            _height.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            _height.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _height.Name = "_height";
            _height.Size = new System.Drawing.Size(56, 23);
            _height.TabIndex = 11;
            _height.Value = new decimal(new int[] { 10, 0, 0, 0 });
            _height.ValueChanged += Size_ValueChanged;
            //
            // _presetLabel
            //
            _presetLabel.AutoSize = true;
            _presetLabel.Location = new System.Drawing.Point(32, 178);
            _presetLabel.Name = "_presetLabel";
            _presetLabel.Size = new System.Drawing.Size(39, 15);
            _presetLabel.TabIndex = 6;
            _presetLabel.Text = "Preset";
            //
            // _sizePreset
            //
            _sizePreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _sizePreset.FormattingEnabled = true;
            _sizePreset.Location = new System.Drawing.Point(122, 175);
            _sizePreset.Name = "_sizePreset";
            _sizePreset.Size = new System.Drawing.Size(240, 23);
            _sizePreset.TabIndex = 7;
            _sizePreset.SelectedIndexChanged += SizePreset_SelectedIndexChanged;
            //
            // _customLabel
            //
            _customLabel.AutoSize = true;
            _customLabel.Location = new System.Drawing.Point(32, 208);
            _customLabel.Name = "_customLabel";
            _customLabel.Size = new System.Drawing.Size(49, 15);
            _customLabel.TabIndex = 8;
            _customLabel.Text = "Width x depth";
            //
            // _cellsLabel
            //
            _cellsLabel.AutoSize = true;
            _cellsLabel.Location = new System.Drawing.Point(264, 208);
            _cellsLabel.Name = "_cellsLabel";
            _cellsLabel.Size = new System.Drawing.Size(30, 15);
            _cellsLabel.TabIndex = 12;
            _cellsLabel.Text = "cells";
            //
            // buttonOk
            //
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOk.Location = new System.Drawing.Point(224, 246);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(76, 23);
            buttonOk.TabIndex = 13;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(308, 246);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(76, 23);
            buttonCancel.TabIndex = 14;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // NewMultiDialog
            //
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(400, 284);
            Controls.Add(_blankGround);
            Controls.Add(_custom);
            Controls.Add(_foundationCombo);
            Controls.Add(customHint);
            Controls.Add(_blankFoundation);
            Controls.Add(sizeLabel);
            Controls.Add(_presetLabel);
            Controls.Add(_sizePreset);
            Controls.Add(_customLabel);
            Controls.Add(_width);
            Controls.Add(byLabel);
            Controls.Add(_height);
            Controls.Add(_cellsLabel);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NewMultiDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "New multi";
            ((System.ComponentModel.ISupportInitialize)_width).EndInit();
            ((System.ComponentModel.ISupportInitialize)_height).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.RadioButton _custom;
        private System.Windows.Forms.ComboBox _foundationCombo;
        private System.Windows.Forms.Label customHint;
        private System.Windows.Forms.RadioButton _blankFoundation;
        private System.Windows.Forms.RadioButton _blankGround;
        private System.Windows.Forms.Label sizeLabel;
        private System.Windows.Forms.NumericUpDown _width;
        private System.Windows.Forms.Label byLabel;
        private System.Windows.Forms.NumericUpDown _height;
        private System.Windows.Forms.Label _presetLabel;
        private System.Windows.Forms.ComboBox _sizePreset;
        private System.Windows.Forms.Label _customLabel;
        private System.Windows.Forms.Label _cellsLabel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}
