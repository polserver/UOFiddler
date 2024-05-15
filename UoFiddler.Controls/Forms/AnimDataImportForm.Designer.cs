namespace UoFiddler.Controls.Forms
{
    partial class AnimDataImportForm
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
            label5 = new System.Windows.Forms.Label();
            btnBrowse = new System.Windows.Forms.Button();
            txtImportFileName = new System.Windows.Forms.TextBox();
            btnImport = new System.Windows.Forms.Button();
            cboConflictAction = new System.Windows.Forms.ComboBox();
            label1 = new System.Windows.Forms.Label();
            cbErase = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            //
            // label5
            //
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(10, 10);
            label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(75, 15);
            label5.TabIndex = 17;
            label5.Text = "Import from:";
            //
            // btnBrowse
            //
            btnBrowse.AutoSize = true;
            btnBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            btnBrowse.Location = new System.Drawing.Point(369, 7);
            btnBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new System.Drawing.Size(26, 25);
            btnBrowse.TabIndex = 16;
            btnBrowse.Text = "...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += OnClickBrowse;
            //
            // txtImportFileName
            //
            txtImportFileName.Location = new System.Drawing.Point(103, 9);
            txtImportFileName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txtImportFileName.Name = "txtImportFileName";
            txtImportFileName.Size = new System.Drawing.Size(258, 23);
            txtImportFileName.TabIndex = 15;
            //
            // btnImport
            //
            btnImport.Location = new System.Drawing.Point(166, 122);
            btnImport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnImport.Name = "btnImport";
            btnImport.Size = new System.Drawing.Size(88, 27);
            btnImport.TabIndex = 18;
            btnImport.Text = "Import";
            btnImport.UseVisualStyleBackColor = true;
            btnImport.Click += OnClickImport;
            //
            // cboConflictAction
            //
            cboConflictAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboConflictAction.FormattingEnabled = true;
            cboConflictAction.Items.AddRange(new object[] { "skip", "overwrite" });
            cboConflictAction.Location = new System.Drawing.Point(103, 48);
            cboConflictAction.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            cboConflictAction.Name = "cboConflictAction";
            cboConflictAction.Size = new System.Drawing.Size(140, 23);
            cboConflictAction.TabIndex = 21;
            //
            // label1
            //
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(10, 51);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(69, 15);
            label1.TabIndex = 22;
            label1.Text = "On conflict:";
            //
            // cbErase
            //
            cbErase.AutoSize = true;
            cbErase.Location = new System.Drawing.Point(12, 86);
            cbErase.Name = "cbErase";
            cbErase.Size = new System.Drawing.Size(199, 19);
            cbErase.TabIndex = 23;
            cbErase.Text = "Erase animdata before importing";
            cbErase.UseVisualStyleBackColor = true;
            //
            // AnimDataImportForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(415, 162);
            Controls.Add(cbErase);
            Controls.Add(label1);
            Controls.Add(cboConflictAction);
            Controls.Add(btnImport);
            Controls.Add(label5);
            Controls.Add(btnBrowse);
            Controls.Add(txtImportFileName);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimDataImportForm";
            Text = "Import AnimData";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtImportFileName;

        #endregion
        private System.Windows.Forms.ComboBox cboConflictAction;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbErase;
    }
}