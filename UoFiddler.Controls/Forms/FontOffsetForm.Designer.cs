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
    partial class FontOffsetForm
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
            this.offsetx = new System.Windows.Forms.NumericUpDown();
            this.offsety = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.CharLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.offsetx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.offsety)).BeginInit();
            this.SuspendLayout();
            // 
            // offsetx
            // 
            this.offsetx.Location = new System.Drawing.Point(74, 44);
            this.offsetx.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.offsetx.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.offsetx.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            -2147483648});
            this.offsetx.Name = "offsetx";
            this.offsetx.Size = new System.Drawing.Size(140, 23);
            this.offsetx.TabIndex = 0;
            // 
            // offsety
            // 
            this.offsety.Location = new System.Drawing.Point(74, 76);
            this.offsety.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.offsety.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.offsety.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            -2147483648});
            this.offsety.Name = "offsety";
            this.offsety.Size = new System.Drawing.Size(140, 23);
            this.offsety.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "OffsetX";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 78);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "OffsetY";
            // 
            // CharLabel
            // 
            this.CharLabel.AutoSize = true;
            this.CharLabel.Location = new System.Drawing.Point(14, 10);
            this.CharLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.CharLabel.Name = "CharLabel";
            this.CharLabel.Size = new System.Drawing.Size(61, 15);
            this.CharLabel.TabIndex = 4;
            this.CharLabel.Text = "Character:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 106);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 27);
            this.button1.TabIndex = 5;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickOK);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(126, 106);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 27);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickCancel);
            // 
            // FontOffsetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(228, 144);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.CharLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.offsety);
            this.Controls.Add(this.offsetx);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FontOffsetForm";
            this.Text = "Font Offset";
            ((System.ComponentModel.ISupportInitialize)(this.offsetx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.offsety)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label CharLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown offsetx;
        private System.Windows.Forms.NumericUpDown offsety;
    }
}