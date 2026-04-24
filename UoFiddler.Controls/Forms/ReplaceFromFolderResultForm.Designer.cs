namespace UoFiddler.Controls.Forms
{
    partial class ReplaceFromFolderResultForm
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
            reportTextBox = new System.Windows.Forms.TextBox();
            buttonPanel = new System.Windows.Forms.Panel();
            closeButton = new System.Windows.Forms.Button();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // reportTextBox
            // 
            reportTextBox.BackColor = System.Drawing.SystemColors.Window;
            reportTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            reportTextBox.Font = new System.Drawing.Font("Consolas", 9F);
            reportTextBox.Location = new System.Drawing.Point(8, 8);
            reportTextBox.Multiline = true;
            reportTextBox.Name = "reportTextBox";
            reportTextBox.ReadOnly = true;
            reportTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            reportTextBox.Size = new System.Drawing.Size(368, 210);
            reportTextBox.TabIndex = 1;
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(closeButton);
            buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            buttonPanel.Location = new System.Drawing.Point(8, 218);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new System.Windows.Forms.Padding(0, 4, 8, 4);
            buttonPanel.Size = new System.Drawing.Size(368, 43);
            buttonPanel.TabIndex = 0;
            // 
            // closeButton
            // 
            closeButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            closeButton.Location = new System.Drawing.Point(267, 7);
            closeButton.Name = "closeButton";
            closeButton.Size = new System.Drawing.Size(90, 28);
            closeButton.TabIndex = 2;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            // 
            // ReplaceFromFolderResultForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(384, 261);
            Controls.Add(reportTextBox);
            Controls.Add(buttonPanel);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(400, 300);
            Name = "ReplaceFromFolderResultForm";
            Padding = new System.Windows.Forms.Padding(8, 8, 8, 0);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Replace from Folder";
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox reportTextBox;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button closeButton;
    }
}
