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
    partial class SaveFormatDialog
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
            toLabel = new System.Windows.Forms.Label();
            pathLabel = new System.Windows.Forms.Label();
            formatLabel = new System.Windows.Forms.Label();
            comboBoxFormat = new System.Windows.Forms.ComboBox();
            willCreateLabel = new System.Windows.Forms.Label();
            buttonsPanel = new System.Windows.Forms.TableLayoutPanel();
            buttonSave = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
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
            mainLayoutPanel.Size = new System.Drawing.Size(516, 160);
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
            contentLayoutPanel.Controls.Add(toLabel, 0, 0);
            contentLayoutPanel.Controls.Add(pathLabel, 0, 1);
            contentLayoutPanel.Controls.Add(formatLabel, 0, 2);
            contentLayoutPanel.Controls.Add(comboBoxFormat, 0, 3);
            contentLayoutPanel.Controls.Add(willCreateLabel, 0, 4);
            contentLayoutPanel.Controls.Add(buttonsPanel, 0, 5);
            contentLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            contentLayoutPanel.Location = new System.Drawing.Point(44, 0);
            contentLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            contentLayoutPanel.Name = "contentLayoutPanel";
            contentLayoutPanel.RowCount = 6;
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            contentLayoutPanel.Size = new System.Drawing.Size(472, 160);
            contentLayoutPanel.TabIndex = 1;
            //
            // toLabel
            //
            toLabel.AutoSize = true;
            toLabel.Location = new System.Drawing.Point(0, 0);
            toLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            toLabel.Name = "toLabel";
            toLabel.Size = new System.Drawing.Size(70, 15);
            toLabel.TabIndex = 0;
            toLabel.Text = "Saving to:";
            //
            // pathLabel
            //
            pathLabel.AutoSize = true;
            pathLabel.Location = new System.Drawing.Point(0, 17);
            pathLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            pathLabel.MaximumSize = new System.Drawing.Size(472, 0);
            pathLabel.Name = "pathLabel";
            pathLabel.Size = new System.Drawing.Size(0, 15);
            pathLabel.TabIndex = 1;
            pathLabel.UseMnemonic = false;
            //
            // formatLabel
            //
            formatLabel.AutoSize = true;
            formatLabel.Location = new System.Drawing.Point(0, 44);
            formatLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 2);
            formatLabel.Name = "formatLabel";
            formatLabel.Size = new System.Drawing.Size(60, 15);
            formatLabel.TabIndex = 2;
            formatLabel.Text = "Write as:";
            //
            // comboBoxFormat
            //
            comboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFormat.Location = new System.Drawing.Point(0, 61);
            comboBoxFormat.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            comboBoxFormat.Name = "comboBoxFormat";
            comboBoxFormat.Size = new System.Drawing.Size(472, 23);
            comboBoxFormat.TabIndex = 3;
            comboBoxFormat.SelectedIndexChanged += OnFormatChanged;
            //
            // willCreateLabel
            //
            willCreateLabel.AutoSize = true;
            willCreateLabel.Location = new System.Drawing.Point(0, 92);
            willCreateLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            willCreateLabel.MaximumSize = new System.Drawing.Size(472, 0);
            willCreateLabel.Name = "willCreateLabel";
            willCreateLabel.Size = new System.Drawing.Size(0, 15);
            willCreateLabel.TabIndex = 4;
            willCreateLabel.UseMnemonic = false;
            //
            // buttonsPanel
            //
            buttonsPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            buttonsPanel.AutoSize = true;
            buttonsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonsPanel.ColumnCount = 2;
            buttonsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            buttonsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            buttonsPanel.Controls.Add(buttonSave, 0, 0);
            buttonsPanel.Controls.Add(buttonCancel, 1, 0);
            buttonsPanel.Location = new System.Drawing.Point(216, 119);
            buttonsPanel.Margin = new System.Windows.Forms.Padding(0);
            buttonsPanel.Name = "buttonsPanel";
            buttonsPanel.RowCount = 1;
            buttonsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            buttonsPanel.Size = new System.Drawing.Size(256, 36);
            buttonsPanel.TabIndex = 5;
            //
            // buttonSave
            //
            buttonSave.AutoSize = false;
            buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonSave.Location = new System.Drawing.Point(0, 0);
            buttonSave.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new System.Drawing.Size(120, 32);
            buttonSave.TabIndex = 0;
            buttonSave.Text = "Save";
            buttonSave.UseVisualStyleBackColor = true;
            //
            // buttonCancel
            //
            buttonCancel.AutoSize = false;
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(128, 0);
            buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(120, 32);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            //
            // SaveFormatDialog
            //
            AcceptButton = buttonSave;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoSize = true;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(540, 184);
            Controls.Add(mainLayoutPanel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SaveFormatDialog";
            Padding = new System.Windows.Forms.Padding(12);
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Save";
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
        private System.Windows.Forms.Label toLabel;
        private System.Windows.Forms.Label pathLabel;
        private System.Windows.Forms.Label formatLabel;
        private System.Windows.Forms.ComboBox comboBoxFormat;
        private System.Windows.Forms.Label willCreateLabel;
        private System.Windows.Forms.TableLayoutPanel buttonsPanel;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
    }
}