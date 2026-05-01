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
    partial class TileDataSyncPreviewForm
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
            summaryLabel = new System.Windows.Forms.Label();
            changesListView = new System.Windows.Forms.ListView();
            actionColumn = new System.Windows.Forms.ColumnHeader();
            numberColumn = new System.Windows.Forms.ColumnHeader();
            oldTextColumn = new System.Windows.Forms.ColumnHeader();
            newTextColumn = new System.Windows.Forms.ColumnHeader();
            checkAllButton = new System.Windows.Forms.Button();
            uncheckAllButton = new System.Windows.Forms.Button();
            uncheckRemovesButton = new System.Windows.Forms.Button();
            applyButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // summaryLabel
            //
            summaryLabel.AutoSize = true;
            summaryLabel.Location = new System.Drawing.Point(12, 12);
            summaryLabel.Name = "summaryLabel";
            summaryLabel.Size = new System.Drawing.Size(0, 15);
            summaryLabel.TabIndex = 0;
            //
            // changesListView
            //
            changesListView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            changesListView.CheckBoxes = true;
            changesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { actionColumn, numberColumn, oldTextColumn, newTextColumn });
            changesListView.FullRowSelect = true;
            changesListView.GridLines = true;
            changesListView.Location = new System.Drawing.Point(12, 38);
            changesListView.Name = "changesListView";
            changesListView.Size = new System.Drawing.Size(660, 380);
            changesListView.TabIndex = 1;
            changesListView.UseCompatibleStateImageBehavior = false;
            changesListView.View = System.Windows.Forms.View.Details;
            changesListView.VirtualMode = false;
            //
            // actionColumn
            //
            actionColumn.Text = "Action";
            actionColumn.Width = 80;
            //
            // numberColumn
            //
            numberColumn.Text = "Number";
            numberColumn.Width = 90;
            //
            // oldTextColumn
            //
            oldTextColumn.Text = "Current text";
            oldTextColumn.Width = 230;
            //
            // newTextColumn
            //
            newTextColumn.Text = "New text";
            newTextColumn.Width = 230;
            //
            // checkAllButton
            //
            checkAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkAllButton.Location = new System.Drawing.Point(12, 428);
            checkAllButton.Name = "checkAllButton";
            checkAllButton.Size = new System.Drawing.Size(80, 27);
            checkAllButton.TabIndex = 2;
            checkAllButton.Text = "Check all";
            checkAllButton.UseVisualStyleBackColor = true;
            checkAllButton.Click += OnCheckAll;
            //
            // uncheckAllButton
            //
            uncheckAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            uncheckAllButton.Location = new System.Drawing.Point(96, 428);
            uncheckAllButton.Name = "uncheckAllButton";
            uncheckAllButton.Size = new System.Drawing.Size(80, 27);
            uncheckAllButton.TabIndex = 3;
            uncheckAllButton.Text = "Uncheck all";
            uncheckAllButton.UseVisualStyleBackColor = true;
            uncheckAllButton.Click += OnUncheckAll;
            //
            // uncheckRemovesButton
            //
            uncheckRemovesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            uncheckRemovesButton.Location = new System.Drawing.Point(180, 428);
            uncheckRemovesButton.Name = "uncheckRemovesButton";
            uncheckRemovesButton.Size = new System.Drawing.Size(125, 27);
            uncheckRemovesButton.TabIndex = 4;
            uncheckRemovesButton.Text = "Uncheck removes";
            uncheckRemovesButton.UseVisualStyleBackColor = true;
            uncheckRemovesButton.Click += OnUncheckRemoves;
            //
            // applyButton
            //
            applyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            applyButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            applyButton.Location = new System.Drawing.Point(497, 428);
            applyButton.Name = "applyButton";
            applyButton.Size = new System.Drawing.Size(85, 27);
            applyButton.TabIndex = 5;
            applyButton.Text = "Apply";
            applyButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Location = new System.Drawing.Point(587, 428);
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(85, 27);
            cancelButton.TabIndex = 6;
            cancelButton.Text = "Cancel";
            cancelButton.UseVisualStyleBackColor = true;
            //
            // TileDataSyncPreviewForm
            //
            AcceptButton = applyButton;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = cancelButton;
            ClientSize = new System.Drawing.Size(684, 467);
            Controls.Add(cancelButton);
            Controls.Add(applyButton);
            Controls.Add(uncheckRemovesButton);
            Controls.Add(uncheckAllButton);
            Controls.Add(checkAllButton);
            Controls.Add(changesListView);
            Controls.Add(summaryLabel);
            DoubleBuffered = true;
            MinimumSize = new System.Drawing.Size(500, 300);
            Name = "TileDataSyncPreviewForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Sync from TileData — Preview";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label summaryLabel;
        private System.Windows.Forms.ListView changesListView;
        private System.Windows.Forms.ColumnHeader actionColumn;
        private System.Windows.Forms.ColumnHeader numberColumn;
        private System.Windows.Forms.ColumnHeader oldTextColumn;
        private System.Windows.Forms.ColumnHeader newTextColumn;
        private System.Windows.Forms.Button checkAllButton;
        private System.Windows.Forms.Button uncheckAllButton;
        private System.Windows.Forms.Button uncheckRemovesButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
