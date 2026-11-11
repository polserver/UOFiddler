/***************************************************************************
 *
 * $Author: Turley
 *
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Controls.Forms
{
    partial class TileGroupNameForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            PromptLabel = new System.Windows.Forms.Label();
            NameTextBox = new System.Windows.Forms.TextBox();
            TidLabel = new System.Windows.Forms.Label();
            TidUpDown = new System.Windows.Forms.NumericUpDown();
            OkButton = new System.Windows.Forms.Button();
            CancelDialogButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)TidUpDown).BeginInit();
            SuspendLayout();
            //
            // PromptLabel
            //
            PromptLabel.AutoSize = true;
            PromptLabel.Location = new System.Drawing.Point(12, 12);
            PromptLabel.Name = "PromptLabel";
            PromptLabel.Size = new System.Drawing.Size(42, 15);
            PromptLabel.Text = "Name:";
            //
            // NameTextBox
            //
            NameTextBox.Location = new System.Drawing.Point(12, 30);
            NameTextBox.Name = "NameTextBox";
            NameTextBox.Size = new System.Drawing.Size(320, 23);
            NameTextBox.TabIndex = 0;
            //
            // TidLabel
            //
            TidLabel.AutoSize = true;
            TidLabel.Location = new System.Drawing.Point(12, 62);
            TidLabel.Name = "TidLabel";
            TidLabel.Size = new System.Drawing.Size(95, 15);
            TidLabel.Text = "Cliloc TID (optional):";
            //
            // TidUpDown
            //
            TidUpDown.Location = new System.Drawing.Point(12, 80);
            TidUpDown.Maximum = new decimal(new int[] { 9999999, 0, 0, 0 });
            TidUpDown.Name = "TidUpDown";
            TidUpDown.Size = new System.Drawing.Size(140, 23);
            TidUpDown.TabIndex = 1;
            //
            // OkButton
            //
            OkButton.Location = new System.Drawing.Point(176, 118);
            OkButton.Name = "OkButton";
            OkButton.Size = new System.Drawing.Size(75, 25);
            OkButton.TabIndex = 2;
            OkButton.Text = "OK";
            OkButton.UseVisualStyleBackColor = true;
            OkButton.Click += OnClickOk;
            //
            // CancelDialogButton
            //
            CancelDialogButton.Location = new System.Drawing.Point(257, 118);
            CancelDialogButton.Name = "CancelDialogButton";
            CancelDialogButton.Size = new System.Drawing.Size(75, 25);
            CancelDialogButton.TabIndex = 3;
            CancelDialogButton.Text = "Cancel";
            CancelDialogButton.UseVisualStyleBackColor = true;
            CancelDialogButton.Click += OnClickCancel;
            //
            // TileGroupNameForm
            //
            AcceptButton = OkButton;
            CancelButton = CancelDialogButton;
            ClientSize = new System.Drawing.Size(344, 155);
            Controls.Add(PromptLabel);
            Controls.Add(NameTextBox);
            Controls.Add(TidLabel);
            Controls.Add(TidUpDown);
            Controls.Add(OkButton);
            Controls.Add(CancelDialogButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "TileGroupNameForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Name";
            ((System.ComponentModel.ISupportInitialize)TidUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label PromptLabel;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label TidLabel;
        private System.Windows.Forms.NumericUpDown TidUpDown;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelDialogButton;
    }
}
