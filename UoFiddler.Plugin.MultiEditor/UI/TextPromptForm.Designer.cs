namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class TextPromptForm
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
            _label = new System.Windows.Forms.Label();
            _valueBox = new System.Windows.Forms.TextBox();
            _okButton = new System.Windows.Forms.Button();
            _cancelButton = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // _label
            //
            _label.Location = new System.Drawing.Point(12, 12);
            _label.Name = "_label";
            _label.Size = new System.Drawing.Size(296, 23);
            _label.TabIndex = 0;
            //
            // _valueBox
            //
            _valueBox.Location = new System.Drawing.Point(12, 36);
            _valueBox.Name = "_valueBox";
            _valueBox.Size = new System.Drawing.Size(296, 23);
            _valueBox.TabIndex = 1;
            //
            // _okButton
            //
            _okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            _okButton.Location = new System.Drawing.Point(144, 70);
            _okButton.Name = "_okButton";
            _okButton.Size = new System.Drawing.Size(76, 23);
            _okButton.TabIndex = 2;
            _okButton.Text = "OK";
            //
            // _cancelButton
            //
            _cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            _cancelButton.Location = new System.Drawing.Point(228, 70);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new System.Drawing.Size(76, 23);
            _cancelButton.TabIndex = 3;
            _cancelButton.Text = "Cancel";
            //
            // TextPromptForm
            //
            AcceptButton = _okButton;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = _cancelButton;
            ClientSize = new System.Drawing.Size(320, 112);
            Controls.Add(_label);
            Controls.Add(_valueBox);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TextPromptForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label _label;
        private System.Windows.Forms.TextBox _valueBox;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Button _cancelButton;
    }
}
