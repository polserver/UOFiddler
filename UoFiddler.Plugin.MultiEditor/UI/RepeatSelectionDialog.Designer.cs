namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class RepeatSelectionDialog
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
            _copiesLabel = new System.Windows.Forms.Label();
            _copiesBox = new System.Windows.Forms.NumericUpDown();
            _stepXLabel = new System.Windows.Forms.Label();
            _stepXBox = new System.Windows.Forms.NumericUpDown();
            _stepYLabel = new System.Windows.Forms.Label();
            _stepYBox = new System.Windows.Forms.NumericUpDown();
            _stepZLabel = new System.Windows.Forms.Label();
            _stepZBox = new System.Windows.Forms.NumericUpDown();
            _summaryLabel = new System.Windows.Forms.Label();
            _okButton = new System.Windows.Forms.Button();
            _cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)_copiesBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_stepXBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_stepYBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)_stepZBox).BeginInit();
            SuspendLayout();
            //
            // _copiesLabel
            //
            _copiesLabel.Location = new System.Drawing.Point(12, 16);
            _copiesLabel.Name = "_copiesLabel";
            _copiesLabel.Size = new System.Drawing.Size(120, 15);
            _copiesLabel.TabIndex = 0;
            _copiesLabel.Text = "Copies:";
            //
            // _copiesBox
            //
            _copiesBox.Location = new System.Drawing.Point(140, 13);
            _copiesBox.Maximum = new decimal(new int[] { 200, 0, 0, 0 });
            _copiesBox.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            _copiesBox.Name = "_copiesBox";
            _copiesBox.Size = new System.Drawing.Size(100, 23);
            _copiesBox.TabIndex = 1;
            _copiesBox.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // _stepXLabel
            //
            _stepXLabel.Location = new System.Drawing.Point(12, 48);
            _stepXLabel.Name = "_stepXLabel";
            _stepXLabel.Size = new System.Drawing.Size(120, 15);
            _stepXLabel.TabIndex = 2;
            _stepXLabel.Text = "Step X (cells):";
            //
            // _stepXBox
            //
            _stepXBox.Location = new System.Drawing.Point(140, 45);
            _stepXBox.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            _stepXBox.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            _stepXBox.Name = "_stepXBox";
            _stepXBox.Size = new System.Drawing.Size(100, 23);
            _stepXBox.TabIndex = 3;
            //
            // _stepYLabel
            //
            _stepYLabel.Location = new System.Drawing.Point(12, 80);
            _stepYLabel.Name = "_stepYLabel";
            _stepYLabel.Size = new System.Drawing.Size(120, 15);
            _stepYLabel.TabIndex = 4;
            _stepYLabel.Text = "Step Y (cells):";
            //
            // _stepYBox
            //
            _stepYBox.Location = new System.Drawing.Point(140, 77);
            _stepYBox.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            _stepYBox.Minimum = new decimal(new int[] { 255, 0, 0, int.MinValue });
            _stepYBox.Name = "_stepYBox";
            _stepYBox.Size = new System.Drawing.Size(100, 23);
            _stepYBox.TabIndex = 5;
            //
            // _stepZLabel
            //
            _stepZLabel.Location = new System.Drawing.Point(12, 112);
            _stepZLabel.Name = "_stepZLabel";
            _stepZLabel.Size = new System.Drawing.Size(120, 15);
            _stepZLabel.TabIndex = 6;
            _stepZLabel.Text = "Step Z:";
            //
            // _stepZBox
            //
            _stepZBox.Location = new System.Drawing.Point(140, 109);
            _stepZBox.Maximum = new decimal(new int[] { 127, 0, 0, 0 });
            _stepZBox.Minimum = new decimal(new int[] { 127, 0, 0, int.MinValue });
            _stepZBox.Name = "_stepZBox";
            _stepZBox.Size = new System.Drawing.Size(100, 23);
            _stepZBox.TabIndex = 7;
            //
            // _summaryLabel
            //
            _summaryLabel.Location = new System.Drawing.Point(12, 146);
            _summaryLabel.Name = "_summaryLabel";
            _summaryLabel.Size = new System.Drawing.Size(236, 48);
            _summaryLabel.TabIndex = 8;
            //
            // _okButton
            //
            _okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            _okButton.Location = new System.Drawing.Point(84, 206);
            _okButton.Name = "_okButton";
            _okButton.Size = new System.Drawing.Size(76, 23);
            _okButton.TabIndex = 9;
            _okButton.Text = "OK";
            //
            // _cancelButton
            //
            _cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _cancelButton.Location = new System.Drawing.Point(168, 206);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(76, 23);
            _cancelButton.TabIndex = 10;
            _cancelButton.Text = "Cancel";
            //
            // RepeatSelectionDialog
            //
            AcceptButton = _okButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = _cancelButton;
            ClientSize = new System.Drawing.Size(260, 242);
            Controls.Add(_copiesLabel);
            Controls.Add(_copiesBox);
            Controls.Add(_stepXLabel);
            Controls.Add(_stepXBox);
            Controls.Add(_stepYLabel);
            Controls.Add(_stepYBox);
            Controls.Add(_stepZLabel);
            Controls.Add(_stepZBox);
            Controls.Add(_summaryLabel);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RepeatSelectionDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Repeat selection";
            ((System.ComponentModel.ISupportInitialize)_copiesBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)_stepXBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)_stepYBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)_stepZBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label _copiesLabel;
        private System.Windows.Forms.NumericUpDown _copiesBox;
        private System.Windows.Forms.Label _stepXLabel;
        private System.Windows.Forms.NumericUpDown _stepXBox;
        private System.Windows.Forms.Label _stepYLabel;
        private System.Windows.Forms.NumericUpDown _stepYBox;
        private System.Windows.Forms.Label _stepZLabel;
        private System.Windows.Forms.NumericUpDown _stepZBox;
        private System.Windows.Forms.Label _summaryLabel;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}
