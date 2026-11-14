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
    partial class VdRemapForm
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

        private void InitializeComponent()
        {
            SourceLabel = new System.Windows.Forms.Label();
            LayoutLabel = new System.Windows.Forms.Label();
            LayoutComboBox = new System.Windows.Forms.ComboBox();
            ScaleLabel = new System.Windows.Forms.Label();
            ScaleUpDown = new System.Windows.Forms.NumericUpDown();
            DefaultsButton = new System.Windows.Forms.Button();
            MappingPanel = new System.Windows.Forms.Panel();
            MappingTable = new System.Windows.Forms.TableLayoutPanel();
            OkButton = new System.Windows.Forms.Button();
            CancelDialogButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)ScaleUpDown).BeginInit();
            MappingPanel.SuspendLayout();
            SuspendLayout();
            //
            // SourceLabel
            //
            SourceLabel.AutoSize = true;
            SourceLabel.Location = new System.Drawing.Point(12, 12);
            SourceLabel.Name = "SourceLabel";
            SourceLabel.Size = new System.Drawing.Size(46, 15);
            SourceLabel.TabIndex = 0;
            SourceLabel.Text = "Source:";
            //
            // LayoutLabel
            //
            LayoutLabel.AutoSize = true;
            LayoutLabel.Location = new System.Drawing.Point(12, 40);
            LayoutLabel.Name = "LayoutLabel";
            LayoutLabel.Size = new System.Drawing.Size(72, 15);
            LayoutLabel.TabIndex = 1;
            LayoutLabel.Text = "Save as type:";
            //
            // LayoutComboBox
            //
            LayoutComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            LayoutComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            LayoutComboBox.Location = new System.Drawing.Point(100, 37);
            LayoutComboBox.Name = "LayoutComboBox";
            LayoutComboBox.Size = new System.Drawing.Size(388, 23);
            LayoutComboBox.TabIndex = 2;
            LayoutComboBox.SelectedIndexChanged += OnLayoutChanged;
            //
            // ScaleLabel
            //
            ScaleLabel.AutoSize = true;
            ScaleLabel.Location = new System.Drawing.Point(12, 72);
            ScaleLabel.Name = "ScaleLabel";
            ScaleLabel.Size = new System.Drawing.Size(88, 15);
            ScaleLabel.TabIndex = 3;
            ScaleLabel.Text = "Frame size (%):";
            //
            // ScaleUpDown
            //
            ScaleUpDown.Increment = new decimal(new int[] { 10, 0, 0, 0 });
            ScaleUpDown.Location = new System.Drawing.Point(110, 69);
            ScaleUpDown.Maximum = new decimal(new int[] { 400, 0, 0, 0 });
            ScaleUpDown.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            ScaleUpDown.Name = "ScaleUpDown";
            ScaleUpDown.Size = new System.Drawing.Size(70, 23);
            ScaleUpDown.TabIndex = 4;
            ScaleUpDown.Value = new decimal(new int[] { 100, 0, 0, 0 });
            //
            // DefaultsButton
            //
            DefaultsButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            DefaultsButton.AutoSize = true;
            DefaultsButton.Location = new System.Drawing.Point(368, 67);
            DefaultsButton.Name = "DefaultsButton";
            DefaultsButton.Size = new System.Drawing.Size(120, 27);
            DefaultsButton.TabIndex = 5;
            DefaultsButton.Text = "Default mapping";
            DefaultsButton.UseVisualStyleBackColor = true;
            DefaultsButton.Click += OnClickDefaults;
            //
            // MappingPanel
            //
            MappingPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            MappingPanel.AutoScroll = true;
            MappingPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            MappingPanel.Controls.Add(MappingTable);
            MappingPanel.Location = new System.Drawing.Point(12, 102);
            MappingPanel.Name = "MappingPanel";
            MappingPanel.Size = new System.Drawing.Size(476, 410);
            MappingPanel.TabIndex = 6;
            //
            // MappingTable
            //
            MappingTable.AutoSize = true;
            MappingTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            MappingTable.ColumnCount = 2;
            MappingTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 210F));
            MappingTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            MappingTable.Dock = System.Windows.Forms.DockStyle.Top;
            MappingTable.Location = new System.Drawing.Point(0, 0);
            MappingTable.Name = "MappingTable";
            MappingTable.Padding = new System.Windows.Forms.Padding(4);
            MappingTable.Size = new System.Drawing.Size(457, 8);
            MappingTable.TabIndex = 0;
            //
            // OkButton
            //
            OkButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            OkButton.Location = new System.Drawing.Point(332, 522);
            OkButton.Name = "OkButton";
            OkButton.Size = new System.Drawing.Size(75, 25);
            OkButton.TabIndex = 7;
            OkButton.Text = "Export";
            OkButton.UseVisualStyleBackColor = true;
            //
            // CancelDialogButton
            //
            CancelDialogButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            CancelDialogButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            CancelDialogButton.Location = new System.Drawing.Point(413, 522);
            CancelDialogButton.Name = "CancelDialogButton";
            CancelDialogButton.Size = new System.Drawing.Size(75, 25);
            CancelDialogButton.TabIndex = 8;
            CancelDialogButton.Text = "Cancel";
            CancelDialogButton.UseVisualStyleBackColor = true;
            //
            // VdRemapForm
            //
            AcceptButton = OkButton;
            CancelButton = CancelDialogButton;
            ClientSize = new System.Drawing.Size(500, 559);
            Controls.Add(SourceLabel);
            Controls.Add(LayoutLabel);
            Controls.Add(LayoutComboBox);
            Controls.Add(ScaleLabel);
            Controls.Add(ScaleUpDown);
            Controls.Add(DefaultsButton);
            Controls.Add(MappingPanel);
            Controls.Add(OkButton);
            Controls.Add(CancelDialogButton);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(460, 400);
            Name = "VdRemapForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Body to .vd, remapped";
            ((System.ComponentModel.ISupportInitialize)ScaleUpDown).EndInit();
            MappingPanel.ResumeLayout(false);
            MappingPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label SourceLabel;
        private System.Windows.Forms.Label LayoutLabel;
        private System.Windows.Forms.ComboBox LayoutComboBox;
        private System.Windows.Forms.Label ScaleLabel;
        private System.Windows.Forms.NumericUpDown ScaleUpDown;
        private System.Windows.Forms.Button DefaultsButton;
        private System.Windows.Forms.Panel MappingPanel;
        private System.Windows.Forms.TableLayoutPanel MappingTable;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button CancelDialogButton;
    }
}
