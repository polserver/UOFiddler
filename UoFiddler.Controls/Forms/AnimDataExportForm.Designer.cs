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
            components = new System.ComponentModel.Container();
            button2 = new System.Windows.Forms.Button();
            cbIncludeInvalidTiles = new System.Windows.Forms.CheckBox();
            cbIncludeMissingAnimation = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            //
            // button2
            //
            button2.Location = new System.Drawing.Point(168, 73);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 27);
            button2.TabIndex = 18;
            button2.Text = "Export";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickExport;
            //
            // cbIncludeInvalidTiles
            //
            cbIncludeInvalidTiles.AutoSize = true;
            cbIncludeInvalidTiles.Location = new System.Drawing.Point(17, 11);
            cbIncludeInvalidTiles.Name = "cbIncludeInvalidTiles";
            cbIncludeInvalidTiles.Size = new System.Drawing.Size(226, 19);
            cbIncludeInvalidTiles.TabIndex = 19;
            cbIncludeInvalidTiles.Text = "Include invalid tiles (red-listed entries)\r\n";
            cbIncludeInvalidTiles.UseVisualStyleBackColor = true;
            //
            // cbIncludeMissingAnimation
            //
            cbIncludeMissingAnimation.AutoSize = true;
            cbIncludeMissingAnimation.Location = new System.Drawing.Point(17, 36);
            cbIncludeMissingAnimation.Name = "cbIncludeMissingAnimation";
            cbIncludeMissingAnimation.Size = new System.Drawing.Size(344, 19);
            cbIncludeMissingAnimation.TabIndex = 20;
            cbIncludeMissingAnimation.Text = "Include tiles with missing animation flag (blue-listed entries)";
            cbIncludeMissingAnimation.UseVisualStyleBackColor = true;
            //
            // AnimDataExportForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(415, 112);
            Controls.Add(cbIncludeMissingAnimation);
            Controls.Add(cbIncludeInvalidTiles);
            Controls.Add(button2);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimDataExportForm";
            Text = "Export AnimData";
            ResumeLayout(false);
            PerformLayout();
        }

        private System.Windows.Forms.Button button2;

        #endregion

        private System.Windows.Forms.CheckBox cbIncludeInvalidTiles;
        private System.Windows.Forms.CheckBox cbIncludeMissingAnimation;
    }
}