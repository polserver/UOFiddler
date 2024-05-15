namespace UoFiddler.Controls.Forms
{
    partial class AnimDataExportForm
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
            btnExport = new System.Windows.Forms.Button();
            cboExportSelection = new System.Windows.Forms.ComboBox();
            lblAnimationsToExport = new System.Windows.Forms.Label();
            SuspendLayout();
            //
            // btnExport
            //
            btnExport.Location = new System.Drawing.Point(156, 60);
            btnExport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnExport.Name = "btnExport";
            btnExport.Size = new System.Drawing.Size(88, 27);
            btnExport.TabIndex = 18;
            btnExport.Text = "Export";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += OnClickExport;
            //
            // cboExportSelection
            //
            cboExportSelection.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboExportSelection.FormattingEnabled = true;
            cboExportSelection.Location = new System.Drawing.Point(12, 27);
            cboExportSelection.Name = "cboExportSelection";
            cboExportSelection.Size = new System.Drawing.Size(391, 23);
            cboExportSelection.TabIndex = 21;
            //
            // lblAnimationsToExport
            //
            lblAnimationsToExport.AutoSize = true;
            lblAnimationsToExport.Location = new System.Drawing.Point(12, 9);
            lblAnimationsToExport.Name = "lblAnimationsToExport";
            lblAnimationsToExport.Size = new System.Drawing.Size(122, 15);
            lblAnimationsToExport.TabIndex = 22;
            lblAnimationsToExport.Text = "Animations to export:";
            //
            // AnimDataExportForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(415, 99);
            Controls.Add(lblAnimationsToExport);
            Controls.Add(cboExportSelection);
            Controls.Add(btnExport);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimDataExportForm";
            Text = "Export AnimData";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button btnExport;

#endregion
        private System.Windows.Forms.ComboBox cboExportSelection;
        private System.Windows.Forms.Label lblAnimationsToExport;
    }
}