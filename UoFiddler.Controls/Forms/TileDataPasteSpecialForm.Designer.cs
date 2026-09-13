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
    partial class TileDataPasteSpecialForm
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
            mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            headerLabel = new System.Windows.Forms.Label();
            contentSplitContainer = new System.Windows.Forms.SplitContainer();
            fieldsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            fieldsLabel = new System.Windows.Forms.Label();
            fieldsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            flagsGroupBox = new System.Windows.Forms.GroupBox();
            flagsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            flagsModeFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            flagsLeaveRadioButton = new System.Windows.Forms.RadioButton();
            flagsReplaceRadioButton = new System.Windows.Forms.RadioButton();
            flagsSetCheckedRadioButton = new System.Windows.Forms.RadioButton();
            flagsClearCheckedRadioButton = new System.Windows.Forms.RadioButton();
            flagsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            buttonsFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            cancelButton = new System.Windows.Forms.Button();
            applyButton = new System.Windows.Forms.Button();
            uncheckAllButton = new System.Windows.Forms.Button();
            checkAllButton = new System.Windows.Forms.Button();
            mainTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)contentSplitContainer).BeginInit();
            contentSplitContainer.Panel1.SuspendLayout();
            contentSplitContainer.Panel2.SuspendLayout();
            contentSplitContainer.SuspendLayout();
            fieldsTableLayoutPanel.SuspendLayout();
            flagsGroupBox.SuspendLayout();
            flagsTableLayoutPanel.SuspendLayout();
            flagsModeFlowLayoutPanel.SuspendLayout();
            buttonsFlowLayoutPanel.SuspendLayout();
            SuspendLayout();
            //
            // mainTableLayoutPanel
            //
            mainTableLayoutPanel.ColumnCount = 1;
            mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.Controls.Add(headerLabel, 0, 0);
            mainTableLayoutPanel.Controls.Add(contentSplitContainer, 0, 1);
            mainTableLayoutPanel.Controls.Add(buttonsFlowLayoutPanel, 0, 2);
            mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            mainTableLayoutPanel.Padding = new System.Windows.Forms.Padding(8);
            mainTableLayoutPanel.RowCount = 3;
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            mainTableLayoutPanel.Size = new System.Drawing.Size(624, 441);
            mainTableLayoutPanel.TabIndex = 0;
            //
            // headerLabel
            //
            headerLabel.AutoSize = true;
            headerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            headerLabel.Location = new System.Drawing.Point(11, 8);
            headerLabel.Name = "headerLabel";
            headerLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            headerLabel.Size = new System.Drawing.Size(602, 21);
            headerLabel.TabIndex = 0;
            headerLabel.Text = "Paste special";
            //
            // contentSplitContainer
            //
            contentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            contentSplitContainer.Location = new System.Drawing.Point(11, 32);
            contentSplitContainer.Name = "contentSplitContainer";
            //
            // contentSplitContainer.Panel1
            //
            contentSplitContainer.Panel1.Controls.Add(fieldsTableLayoutPanel);
            contentSplitContainer.Panel1MinSize = 160;
            //
            // contentSplitContainer.Panel2
            //
            contentSplitContainer.Panel2.Controls.Add(flagsGroupBox);
            contentSplitContainer.Panel2MinSize = 200;
            contentSplitContainer.Size = new System.Drawing.Size(602, 359);
            contentSplitContainer.SplitterDistance = 240;
            contentSplitContainer.TabIndex = 1;
            //
            // fieldsTableLayoutPanel
            //
            fieldsTableLayoutPanel.ColumnCount = 1;
            fieldsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fieldsTableLayoutPanel.Controls.Add(fieldsLabel, 0, 0);
            fieldsTableLayoutPanel.Controls.Add(fieldsCheckedListBox, 0, 1);
            fieldsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            fieldsTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            fieldsTableLayoutPanel.Name = "fieldsTableLayoutPanel";
            fieldsTableLayoutPanel.RowCount = 2;
            fieldsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            fieldsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            fieldsTableLayoutPanel.Size = new System.Drawing.Size(240, 359);
            fieldsTableLayoutPanel.TabIndex = 0;
            //
            // fieldsLabel
            //
            fieldsLabel.AutoSize = true;
            fieldsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            fieldsLabel.Location = new System.Drawing.Point(3, 0);
            fieldsLabel.Name = "fieldsLabel";
            fieldsLabel.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            fieldsLabel.Size = new System.Drawing.Size(234, 19);
            fieldsLabel.TabIndex = 0;
            fieldsLabel.Text = "Fields to paste";
            //
            // fieldsCheckedListBox
            //
            fieldsCheckedListBox.CheckOnClick = true;
            fieldsCheckedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            fieldsCheckedListBox.FormattingEnabled = true;
            fieldsCheckedListBox.IntegralHeight = false;
            fieldsCheckedListBox.Location = new System.Drawing.Point(3, 22);
            fieldsCheckedListBox.Name = "fieldsCheckedListBox";
            fieldsCheckedListBox.Size = new System.Drawing.Size(234, 334);
            fieldsCheckedListBox.TabIndex = 1;
            //
            // flagsGroupBox
            //
            flagsGroupBox.Controls.Add(flagsTableLayoutPanel);
            flagsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            flagsGroupBox.Location = new System.Drawing.Point(0, 0);
            flagsGroupBox.Name = "flagsGroupBox";
            flagsGroupBox.Padding = new System.Windows.Forms.Padding(6);
            flagsGroupBox.Size = new System.Drawing.Size(358, 359);
            flagsGroupBox.TabIndex = 0;
            flagsGroupBox.TabStop = false;
            flagsGroupBox.Text = "Flags";
            //
            // flagsTableLayoutPanel
            //
            flagsTableLayoutPanel.ColumnCount = 1;
            flagsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            flagsTableLayoutPanel.Controls.Add(flagsModeFlowLayoutPanel, 0, 0);
            flagsTableLayoutPanel.Controls.Add(flagsCheckedListBox, 0, 1);
            flagsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            flagsTableLayoutPanel.Location = new System.Drawing.Point(6, 22);
            flagsTableLayoutPanel.Name = "flagsTableLayoutPanel";
            flagsTableLayoutPanel.RowCount = 2;
            flagsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            flagsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            flagsTableLayoutPanel.Size = new System.Drawing.Size(346, 331);
            flagsTableLayoutPanel.TabIndex = 0;
            //
            // flagsModeFlowLayoutPanel
            //
            flagsModeFlowLayoutPanel.AutoSize = true;
            flagsModeFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flagsModeFlowLayoutPanel.Controls.Add(flagsLeaveRadioButton);
            flagsModeFlowLayoutPanel.Controls.Add(flagsReplaceRadioButton);
            flagsModeFlowLayoutPanel.Controls.Add(flagsSetCheckedRadioButton);
            flagsModeFlowLayoutPanel.Controls.Add(flagsClearCheckedRadioButton);
            flagsModeFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            flagsModeFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            flagsModeFlowLayoutPanel.Location = new System.Drawing.Point(3, 3);
            flagsModeFlowLayoutPanel.Name = "flagsModeFlowLayoutPanel";
            flagsModeFlowLayoutPanel.Size = new System.Drawing.Size(340, 92);
            flagsModeFlowLayoutPanel.TabIndex = 0;
            flagsModeFlowLayoutPanel.WrapContents = false;
            //
            // flagsLeaveRadioButton
            //
            flagsLeaveRadioButton.AutoSize = true;
            flagsLeaveRadioButton.Location = new System.Drawing.Point(3, 3);
            flagsLeaveRadioButton.Name = "flagsLeaveRadioButton";
            flagsLeaveRadioButton.Size = new System.Drawing.Size(122, 19);
            flagsLeaveRadioButton.TabIndex = 0;
            flagsLeaveRadioButton.Text = "Don't change flags";
            flagsLeaveRadioButton.UseVisualStyleBackColor = true;
            flagsLeaveRadioButton.CheckedChanged += OnFlagModeChanged;
            //
            // flagsReplaceRadioButton
            //
            flagsReplaceRadioButton.AutoSize = true;
            flagsReplaceRadioButton.Checked = true;
            flagsReplaceRadioButton.Location = new System.Drawing.Point(3, 28);
            flagsReplaceRadioButton.Name = "flagsReplaceRadioButton";
            flagsReplaceRadioButton.Size = new System.Drawing.Size(178, 19);
            flagsReplaceRadioButton.TabIndex = 1;
            flagsReplaceRadioButton.TabStop = true;
            flagsReplaceRadioButton.Text = "Replace all flags with source";
            flagsReplaceRadioButton.UseVisualStyleBackColor = true;
            flagsReplaceRadioButton.CheckedChanged += OnFlagModeChanged;
            //
            // flagsSetCheckedRadioButton
            //
            flagsSetCheckedRadioButton.AutoSize = true;
            flagsSetCheckedRadioButton.Location = new System.Drawing.Point(3, 53);
            flagsSetCheckedRadioButton.Name = "flagsSetCheckedRadioButton";
            flagsSetCheckedRadioButton.Size = new System.Drawing.Size(148, 19);
            flagsSetCheckedRadioButton.TabIndex = 2;
            flagsSetCheckedRadioButton.Text = "Set only the checked flags";
            flagsSetCheckedRadioButton.UseVisualStyleBackColor = true;
            flagsSetCheckedRadioButton.CheckedChanged += OnFlagModeChanged;
            //
            // flagsClearCheckedRadioButton
            //
            flagsClearCheckedRadioButton.AutoSize = true;
            flagsClearCheckedRadioButton.Location = new System.Drawing.Point(3, 78);
            flagsClearCheckedRadioButton.Name = "flagsClearCheckedRadioButton";
            flagsClearCheckedRadioButton.Size = new System.Drawing.Size(163, 19);
            flagsClearCheckedRadioButton.TabIndex = 3;
            flagsClearCheckedRadioButton.Text = "Clear only the checked flags";
            flagsClearCheckedRadioButton.UseVisualStyleBackColor = true;
            flagsClearCheckedRadioButton.CheckedChanged += OnFlagModeChanged;
            //
            // flagsCheckedListBox
            //
            flagsCheckedListBox.CheckOnClick = true;
            flagsCheckedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            flagsCheckedListBox.FormattingEnabled = true;
            flagsCheckedListBox.IntegralHeight = false;
            flagsCheckedListBox.Location = new System.Drawing.Point(3, 101);
            flagsCheckedListBox.MultiColumn = true;
            flagsCheckedListBox.Name = "flagsCheckedListBox";
            flagsCheckedListBox.Size = new System.Drawing.Size(340, 227);
            flagsCheckedListBox.TabIndex = 1;
            //
            // buttonsFlowLayoutPanel
            //
            buttonsFlowLayoutPanel.AutoSize = true;
            buttonsFlowLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonsFlowLayoutPanel.Controls.Add(cancelButton);
            buttonsFlowLayoutPanel.Controls.Add(applyButton);
            buttonsFlowLayoutPanel.Controls.Add(uncheckAllButton);
            buttonsFlowLayoutPanel.Controls.Add(checkAllButton);
            buttonsFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            buttonsFlowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            buttonsFlowLayoutPanel.Location = new System.Drawing.Point(11, 397);
            buttonsFlowLayoutPanel.Name = "buttonsFlowLayoutPanel";
            buttonsFlowLayoutPanel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            buttonsFlowLayoutPanel.Size = new System.Drawing.Size(602, 35);
            buttonsFlowLayoutPanel.TabIndex = 2;
            buttonsFlowLayoutPanel.WrapContents = false;
            //
            // cancelButton
            //
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(524, 9);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.TabIndex = 0;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            //
            // applyButton
            //
            applyButton.Location = new System.Drawing.Point(443, 9);
            applyButton.Name = "applyButton";
            applyButton.Size = new System.Drawing.Size(75, 23);
            applyButton.TabIndex = 1;
            applyButton.Text = "Apply";
            applyButton.UseVisualStyleBackColor = true;
            applyButton.Click += OnClickApply;
            //
            // uncheckAllButton
            //
            uncheckAllButton.Location = new System.Drawing.Point(348, 9);
            uncheckAllButton.Name = "uncheckAllButton";
            uncheckAllButton.Size = new System.Drawing.Size(89, 23);
            uncheckAllButton.TabIndex = 2;
            uncheckAllButton.Text = "Uncheck all";
            uncheckAllButton.UseVisualStyleBackColor = true;
            uncheckAllButton.Click += OnClickUncheckAll;
            //
            // checkAllButton
            //
            checkAllButton.Location = new System.Drawing.Point(253, 9);
            checkAllButton.Name = "checkAllButton";
            checkAllButton.Size = new System.Drawing.Size(89, 23);
            checkAllButton.TabIndex = 3;
            checkAllButton.Text = "Check all";
            checkAllButton.UseVisualStyleBackColor = true;
            checkAllButton.Click += OnClickCheckAll;
            //
            // TileDataPasteSpecialForm
            //
            AcceptButton = applyButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new System.Drawing.Size(624, 441);
            Controls.Add(mainTableLayoutPanel);
            MinimizeBox = false;
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(560, 400);
            Name = "TileDataPasteSpecialForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Paste special";
            mainTableLayoutPanel.ResumeLayout(false);
            mainTableLayoutPanel.PerformLayout();
            contentSplitContainer.Panel1.ResumeLayout(false);
            contentSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)contentSplitContainer).EndInit();
            contentSplitContainer.ResumeLayout(false);
            fieldsTableLayoutPanel.ResumeLayout(false);
            fieldsTableLayoutPanel.PerformLayout();
            flagsGroupBox.ResumeLayout(false);
            flagsTableLayoutPanel.ResumeLayout(false);
            flagsTableLayoutPanel.PerformLayout();
            flagsModeFlowLayoutPanel.ResumeLayout(false);
            flagsModeFlowLayoutPanel.PerformLayout();
            buttonsFlowLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.Label headerLabel;
        private System.Windows.Forms.SplitContainer contentSplitContainer;
        private System.Windows.Forms.TableLayoutPanel fieldsTableLayoutPanel;
        private System.Windows.Forms.Label fieldsLabel;
        private System.Windows.Forms.CheckedListBox fieldsCheckedListBox;
        private System.Windows.Forms.GroupBox flagsGroupBox;
        private System.Windows.Forms.TableLayoutPanel flagsTableLayoutPanel;
        private System.Windows.Forms.FlowLayoutPanel flagsModeFlowLayoutPanel;
        private System.Windows.Forms.RadioButton flagsLeaveRadioButton;
        private System.Windows.Forms.RadioButton flagsReplaceRadioButton;
        private System.Windows.Forms.RadioButton flagsSetCheckedRadioButton;
        private System.Windows.Forms.RadioButton flagsClearCheckedRadioButton;
        private System.Windows.Forms.CheckedListBox flagsCheckedListBox;
        private System.Windows.Forms.FlowLayoutPanel buttonsFlowLayoutPanel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button uncheckAllButton;
        private System.Windows.Forms.Button checkAllButton;
    }
}
