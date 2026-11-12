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
    partial class ItemRangeInputForm
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
            RangeTextBox = new System.Windows.Forms.TextBox();
            OkButton = new System.Windows.Forms.Button();
            CancelDialogButton = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // PromptLabel
            //
            PromptLabel.AutoSize = true;
            PromptLabel.Location = new System.Drawing.Point(12, 12);
            PromptLabel.Name = "PromptLabel";
            PromptLabel.Size = new System.Drawing.Size(231, 15);
            PromptLabel.Text = "Item range (e.g. 100-200 or 0x64-0xC8):";
            //
            // RangeTextBox
            //
            RangeTextBox.Location = new System.Drawing.Point(12, 30);
            RangeTextBox.Name = "RangeTextBox";
            RangeTextBox.Size = new System.Drawing.Size(320, 23);
            RangeTextBox.TabIndex = 0;
            //
            // OkButton
            //
            OkButton.Location = new System.Drawing.Point(176, 66);
            OkButton.Name = "OkButton";
            OkButton.Size = new System.Drawing.Size(75, 25);
            OkButton.TabIndex = 1;
            OkButton.Text = "OK";
            OkButton.UseVisualStyleBackColor = true;
            OkButton.Click += OnClickOk;
            //
            // CancelDialogButton
            //
            CancelDialogButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            CancelDialogButton.Location = new System.Drawing.Point(257, 66);
            CancelDialogButton.Name = "CancelDialogButton";
            CancelDialogButton.Size = new System.Drawing.Size(75, 25);
            CancelDialogButton.TabIndex = 2;
            CancelDialogButton.Text = "Cancel";
            CancelDialogButton.UseVisualStyleBackColor = true;
            //
            // ItemRangeInputForm
            //
            AcceptButton = OkButton;
            CancelButton = CancelDialogButton;
            ClientSize = new System.Drawing.Size(344, 103);
            Controls.Add(PromptLabel);
            Controls.Add(RangeTextBox);
            Controls.Add(OkButton);
            Controls.Add(CancelDialogButton);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ItemRangeInputForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Export Items to Sheet";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label PromptLabel;
        private System.Windows.Forms.TextBox RangeTextBox;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelDialogButton;
    }
}
