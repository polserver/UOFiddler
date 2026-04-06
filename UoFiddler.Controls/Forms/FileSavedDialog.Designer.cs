namespace UoFiddler.Controls.Forms
{
    partial class FileSavedDialog
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
            mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            iconPictureBox = new System.Windows.Forms.PictureBox();
            contentLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            statusLabel = new System.Windows.Forms.Label();
            pathLabel = new System.Windows.Forms.Label();
            buttonsPanel = new System.Windows.Forms.TableLayoutPanel();
            buttonOpenFolder = new System.Windows.Forms.Button();
            buttonOk = new System.Windows.Forms.Button();
            mainLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)iconPictureBox).BeginInit();
            contentLayoutPanel.SuspendLayout();
            buttonsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            mainLayoutPanel.AutoSize = true;
            mainLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            mainLayoutPanel.ColumnCount = 2;
            mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainLayoutPanel.Controls.Add(iconPictureBox, 0, 0);
            mainLayoutPanel.Controls.Add(contentLayoutPanel, 1, 0);
            mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainLayoutPanel.Location = new System.Drawing.Point(12, 12);
            mainLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            mainLayoutPanel.Name = "mainLayoutPanel";
            mainLayoutPanel.RowCount = 1;
            mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            mainLayoutPanel.Size = new System.Drawing.Size(560, 86);
            mainLayoutPanel.TabIndex = 0;
            // 
            // iconPictureBox
            // 
            iconPictureBox.Location = new System.Drawing.Point(0, 0);
            iconPictureBox.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            iconPictureBox.Name = "iconPictureBox";
            iconPictureBox.Size = new System.Drawing.Size(32, 32);
            iconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            iconPictureBox.TabIndex = 0;
            iconPictureBox.TabStop = false;
            // 
            // contentLayoutPanel
            // 
            contentLayoutPanel.AutoSize = true;
            contentLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            contentLayoutPanel.ColumnCount = 1;
            contentLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            contentLayoutPanel.Controls.Add(statusLabel, 0, 0);
            contentLayoutPanel.Controls.Add(pathLabel, 0, 1);
            contentLayoutPanel.Controls.Add(buttonsPanel, 0, 2);
            contentLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            contentLayoutPanel.Location = new System.Drawing.Point(44, 0);
            contentLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            contentLayoutPanel.Name = "contentLayoutPanel";
            contentLayoutPanel.RowCount = 3;
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.Size = new System.Drawing.Size(516, 86);
            contentLayoutPanel.TabIndex = 1;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Location = new System.Drawing.Point(0, 0);
            statusLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new System.Drawing.Size(147, 15);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "File saved successfully.";
            // 
            // pathLabel
            // 
            pathLabel.AutoSize = true;
            pathLabel.Location = new System.Drawing.Point(0, 23);
            pathLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            pathLabel.MaximumSize = new System.Drawing.Size(516, 0);
            pathLabel.Name = "pathLabel";
            pathLabel.Size = new System.Drawing.Size(0, 15);
            pathLabel.TabIndex = 1;
            pathLabel.UseMnemonic = false;
            // 
            // buttonsPanel
            // 
            buttonsPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            buttonsPanel.AutoSize = true;
            buttonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonsPanel.ColumnCount = 2;
            buttonsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            buttonsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            buttonsPanel.Controls.Add(buttonOpenFolder, 0, 0);
            buttonsPanel.Controls.Add(buttonOk, 1, 0);
            buttonsPanel.Location = new System.Drawing.Point(260, 50);
            buttonsPanel.Margin = new System.Windows.Forms.Padding(0);
            buttonsPanel.Name = "buttonsPanel";
            buttonsPanel.RowCount = 1;
            buttonsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            buttonsPanel.Size = new System.Drawing.Size(256, 36);
            buttonsPanel.TabIndex = 2;
            // 
            // buttonOpenFolder
            // 
            buttonOpenFolder.AutoSize = false;
            buttonOpenFolder.Location = new System.Drawing.Point(0, 0);
            buttonOpenFolder.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            buttonOpenFolder.Name = "buttonOpenFolder";
            buttonOpenFolder.Size = new System.Drawing.Size(120, 32);
            buttonOpenFolder.TabIndex = 0;
            buttonOpenFolder.Text = "Open Folder";
            buttonOpenFolder.UseVisualStyleBackColor = true;
            buttonOpenFolder.Click += OnOpenFolderClick;
            // 
            // buttonOk
            // 
            buttonOk.AutoSize = false;
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOk.Location = new System.Drawing.Point(128, 0);
            buttonOk.Margin = new System.Windows.Forms.Padding(0);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(120, 32);
            buttonOk.TabIndex = 1;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            // 
            // FileSavedDialog
            // 
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            CancelButton = buttonOk;
            ClientSize = new System.Drawing.Size(584, 110);
            Controls.Add(mainLayoutPanel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FileSavedDialog";
            Padding = new System.Windows.Forms.Padding(12);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Saved";
            mainLayoutPanel.ResumeLayout(false);
            mainLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)iconPictureBox).EndInit();
            contentLayoutPanel.ResumeLayout(false);
            contentLayoutPanel.PerformLayout();
            buttonsPanel.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.PictureBox iconPictureBox;
        private System.Windows.Forms.TableLayoutPanel contentLayoutPanel;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.TableLayoutPanel buttonsPanel;
        private System.Windows.Forms.Button buttonOpenFolder;
        private System.Windows.Forms.Button buttonOk;
    }
}
