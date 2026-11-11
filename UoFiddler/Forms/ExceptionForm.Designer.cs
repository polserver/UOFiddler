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

namespace UoFiddler.Forms
{
    partial class ExceptionForm
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
            OK = new System.Windows.Forms.Button();
            richTextBox = new System.Windows.Forms.RichTextBox();
            buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            CopyButton = new System.Windows.Forms.Button();
            OpenLogFolderButton = new System.Windows.Forms.Button();
            ReportButton = new System.Windows.Forms.Button();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(OK);
            buttonPanel.Controls.Add(ReportButton);
            buttonPanel.Controls.Add(OpenLogFolderButton);
            buttonPanel.Controls.Add(CopyButton);
            buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonPanel.Location = new System.Drawing.Point(0, 256);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Padding = new System.Windows.Forms.Padding(2);
            buttonPanel.Size = new System.Drawing.Size(674, 35);
            buttonPanel.TabIndex = 2;
            // 
            // OK
            // 
            OK.Location = new System.Drawing.Point(567, 5);
            OK.Name = "OK";
            OK.Size = new System.Drawing.Size(100, 25);
            OK.TabIndex = 0;
            OK.Text = "Close";
            OK.UseVisualStyleBackColor = true;
            OK.Click += OnClick;
            // 
            // ReportButton
            // 
            ReportButton.Location = new System.Drawing.Point(431, 5);
            ReportButton.Name = "ReportButton";
            ReportButton.Size = new System.Drawing.Size(130, 25);
            ReportButton.TabIndex = 3;
            ReportButton.Text = "Report on GitHub...";
            ReportButton.UseVisualStyleBackColor = true;
            ReportButton.Click += OnClickReport;
            // 
            // OpenLogFolderButton
            // 
            OpenLogFolderButton.Location = new System.Drawing.Point(295, 5);
            OpenLogFolderButton.Name = "OpenLogFolderButton";
            OpenLogFolderButton.Size = new System.Drawing.Size(130, 25);
            OpenLogFolderButton.TabIndex = 2;
            OpenLogFolderButton.Text = "Open Log Folder";
            OpenLogFolderButton.UseVisualStyleBackColor = true;
            OpenLogFolderButton.Click += OnClickOpenLogFolder;
            // 
            // CopyButton
            // 
            CopyButton.Location = new System.Drawing.Point(159, 5);
            CopyButton.Name = "CopyButton";
            CopyButton.Size = new System.Drawing.Size(130, 25);
            CopyButton.TabIndex = 1;
            CopyButton.Text = "Copy Details";
            CopyButton.UseVisualStyleBackColor = true;
            CopyButton.Click += OnClickCopy;
            // 
            // richTextBox
            // 
            richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            richTextBox.Location = new System.Drawing.Point(0, 0);
            richTextBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            richTextBox.Name = "richTextBox";
            richTextBox.ReadOnly = true;
            richTextBox.Size = new System.Drawing.Size(674, 256);
            richTextBox.TabIndex = 1;
            richTextBox.Text = "";
            // 
            // ExceptionForm
            // 
            AcceptButton = OK;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = OK;
            ClientSize = new System.Drawing.Size(674, 291);
            Controls.Add(richTextBox);
            Controls.Add(buttonPanel);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "ExceptionForm";
            Text = "Exception";
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.RichTextBox richTextBox;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.Button CopyButton;
        private System.Windows.Forms.Button OpenLogFolderButton;
        private System.Windows.Forms.Button ReportButton;
    }
}
