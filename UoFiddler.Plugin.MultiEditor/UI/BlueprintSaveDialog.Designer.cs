namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class BlueprintSaveDialog
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
            _summary = new System.Windows.Forms.Label();
            _groupLabel = new System.Windows.Forms.Label();
            _group = new System.Windows.Forms.ComboBox();
            _split = new System.Windows.Forms.CheckBox();
            _okButton = new System.Windows.Forms.Button();
            _cancelButton = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // _summary
            //
            _summary.Location = new System.Drawing.Point(12, 12);
            _summary.Name = "_summary";
            _summary.Size = new System.Drawing.Size(356, 46);
            _summary.TabIndex = 0;
            //
            // _groupLabel
            //
            _groupLabel.AutoSize = true;
            _groupLabel.Location = new System.Drawing.Point(12, 68);
            _groupLabel.Name = "_groupLabel";
            _groupLabel.Size = new System.Drawing.Size(43, 15);
            _groupLabel.TabIndex = 1;
            _groupLabel.Text = "Group:";
            //
            // _group
            //
            _group.FormattingEnabled = true;
            _group.Location = new System.Drawing.Point(70, 64);
            _group.Name = "_group";
            _group.Size = new System.Drawing.Size(298, 23);
            _group.TabIndex = 2;
            //
            // _split
            //
            _split.Location = new System.Drawing.Point(12, 96);
            _split.Name = "_split";
            _split.Size = new System.Drawing.Size(356, 38);
            _split.TabIndex = 3;
            _split.UseVisualStyleBackColor = true;
            _split.Visible = false;
            //
            // _okButton
            //
            _okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            _okButton.Location = new System.Drawing.Point(208, 142);
            _okButton.Name = "_okButton";
            _okButton.Size = new System.Drawing.Size(76, 23);
            _okButton.TabIndex = 4;
            _okButton.Text = "Save";
            //
            // _cancelButton
            //
            _cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _cancelButton.Location = new System.Drawing.Point(292, 142);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(76, 23);
            _cancelButton.TabIndex = 5;
            _cancelButton.Text = "Cancel";
            //
            // BlueprintSaveDialog
            //
            AcceptButton = _okButton;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = _cancelButton;
            ClientSize = new System.Drawing.Size(380, 177);
            Controls.Add(_summary);
            Controls.Add(_groupLabel);
            Controls.Add(_group);
            Controls.Add(_split);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BlueprintSaveDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Save as blueprints";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label _summary;
        private System.Windows.Forms.Label _groupLabel;
        private System.Windows.Forms.ComboBox _group;
        private System.Windows.Forms.CheckBox _split;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}
