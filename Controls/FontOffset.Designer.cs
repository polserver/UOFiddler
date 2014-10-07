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

namespace FiddlerControls
{
    partial class FontOffset
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
            this.offsetx.Location = new System.Drawing.Point(63, 38);
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
            this.offsetx.Size = new System.Drawing.Size(120, 20);
            this.offsetx.TabIndex = 0;
            // 
            // offsety
            // 
            this.offsety.Location = new System.Drawing.Point(63, 66);
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
            this.offsety.Size = new System.Drawing.Size(120, 20);
            this.offsety.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "OffsetX";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "OffsetY";
            // 
            // CharLabel
            // 
            this.CharLabel.AutoSize = true;
            this.CharLabel.Location = new System.Drawing.Point(12, 9);
            this.CharLabel.Name = "CharLabel";
            this.CharLabel.Size = new System.Drawing.Size(56, 13);
            this.CharLabel.TabIndex = 4;
            this.CharLabel.Text = "Character:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 92);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickOK);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(108, 92);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickCancel);
            // 
            // FontOffset
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(199, 121);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.CharLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.offsety);
            this.Controls.Add(this.offsetx);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FontOffset";
            this.Text = "Font Offset";
            ((System.ComponentModel.ISupportInitialize)(this.offsetx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.offsety)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NumericUpDown offsetx;
        private System.Windows.Forms.NumericUpDown offsety;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label CharLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}