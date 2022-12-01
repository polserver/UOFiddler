﻿/***************************************************************************
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
    partial class MultiImportForm
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
            this.importTypeComboBox = new System.Windows.Forms.ComboBox();
            this.filenameTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.importButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // importTypeComboBox
            // 
            this.importTypeComboBox.FormattingEnabled = true;
            this.importTypeComboBox.Items.AddRange(new object[] {
            "Txt file",
            "UOA file",
            "UOA Binary file",
            "WSC file",
            "CSV (punt\'s multi tool)"});
            this.importTypeComboBox.Location = new System.Drawing.Point(12, 12);
            this.importTypeComboBox.Name = "importTypeComboBox";
            this.importTypeComboBox.Size = new System.Drawing.Size(153, 23);
            this.importTypeComboBox.TabIndex = 0;
            // 
            // filenameTextBox
            // 
            this.filenameTextBox.Location = new System.Drawing.Point(12, 41);
            this.filenameTextBox.Name = "filenameTextBox";
            this.filenameTextBox.Size = new System.Drawing.Size(236, 23);
            this.filenameTextBox.TabIndex = 2;
            // 
            // browseButton
            // 
            this.browseButton.AutoSize = true;
            this.browseButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.browseButton.Location = new System.Drawing.Point(255, 41);
            this.browseButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(26, 25);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.OnClickBrowse);
            // 
            // importButton
            // 
            this.importButton.Location = new System.Drawing.Point(206, 72);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(75, 23);
            this.importButton.TabIndex = 4;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.OnClickImport);
            // 
            // MultiImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 104);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.filenameTextBox);
            this.Controls.Add(this.importTypeComboBox);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MultiImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multi Import";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.ComboBox importTypeComboBox;
        private System.Windows.Forms.TextBox filenameTextBox;
    }
}