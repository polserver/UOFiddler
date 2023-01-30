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
    partial class MapReplaceForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBoxMap = new System.Windows.Forms.CheckBox();
            this.checkBoxStatics = new System.Windows.Forms.CheckBox();
            this.numericUpDownX1 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownY1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownX2 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownY2 = new System.Windows.Forms.NumericUpDown();
            this.button2 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label5 = new System.Windows.Forms.Label();
            this.RemoveDupl = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.numericUpDownToX1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownToY1 = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBoxMapID = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY2)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownToX1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownToY1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(105, 14);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(227, 23);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.Location = new System.Drawing.Point(340, 12);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(26, 25);
            this.button1.TabIndex = 1;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickBrowse);
            // 
            // checkBoxMap
            // 
            this.checkBoxMap.AutoSize = true;
            this.checkBoxMap.Location = new System.Drawing.Point(7, 22);
            this.checkBoxMap.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxMap.Name = "checkBoxMap";
            this.checkBoxMap.Size = new System.Drawing.Size(81, 19);
            this.checkBoxMap.TabIndex = 2;
            this.checkBoxMap.Text = "Copy Map";
            this.checkBoxMap.UseVisualStyleBackColor = true;
            // 
            // checkBoxStatics
            // 
            this.checkBoxStatics.AutoSize = true;
            this.checkBoxStatics.Location = new System.Drawing.Point(13, 22);
            this.checkBoxStatics.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBoxStatics.Name = "checkBoxStatics";
            this.checkBoxStatics.Size = new System.Drawing.Size(91, 19);
            this.checkBoxStatics.TabIndex = 3;
            this.checkBoxStatics.Text = "Copy Statics";
            this.checkBoxStatics.UseVisualStyleBackColor = true;
            // 
            // numericUpDownX1
            // 
            this.numericUpDownX1.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownX1.Location = new System.Drawing.Point(99, 22);
            this.numericUpDownX1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownX1.Name = "numericUpDownX1";
            this.numericUpDownX1.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownX1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(63, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "X1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(63, 54);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(20, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Y1";
            // 
            // numericUpDownY1
            // 
            this.numericUpDownY1.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownY1.Location = new System.Drawing.Point(99, 52);
            this.numericUpDownY1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownY1.Name = "numericUpDownY1";
            this.numericUpDownY1.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownY1.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(192, 24);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "X2";
            // 
            // numericUpDownX2
            // 
            this.numericUpDownX2.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownX2.Location = new System.Drawing.Point(229, 22);
            this.numericUpDownX2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownX2.Name = "numericUpDownX2";
            this.numericUpDownX2.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownX2.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(192, 57);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 15);
            this.label4.TabIndex = 11;
            this.label4.Text = "Y2";
            // 
            // numericUpDownY2
            // 
            this.numericUpDownY2.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownY2.Location = new System.Drawing.Point(229, 54);
            this.numericUpDownY2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownY2.Name = "numericUpDownY2";
            this.numericUpDownY2.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownY2.TabIndex = 10;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(144, 318);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 27);
            this.button2.TabIndex = 12;
            this.button2.Text = "Replace";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // progressBar1
            // 
            this.progressBar1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBar1.Location = new System.Drawing.Point(0, 354);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(384, 27);
            this.progressBar1.TabIndex = 13;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 17);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 15);
            this.label5.TabIndex = 14;
            this.label5.Text = "Replace From";
            // 
            // RemoveDupl
            // 
            this.RemoveDupl.AutoSize = true;
            this.RemoveDupl.Location = new System.Drawing.Point(13, 48);
            this.RemoveDupl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.RemoveDupl.Name = "RemoveDupl";
            this.RemoveDupl.Size = new System.Drawing.Size(127, 19);
            this.RemoveDupl.TabIndex = 17;
            this.RemoveDupl.Text = "Remove Duplicates";
            this.RemoveDupl.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxMap);
            this.groupBox1.Location = new System.Drawing.Point(15, 75);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(169, 75);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Map";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBoxStatics);
            this.groupBox2.Controls.Add(this.RemoveDupl);
            this.groupBox2.Location = new System.Drawing.Point(201, 75);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(169, 75);
            this.groupBox2.TabIndex = 20;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Statics";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.numericUpDownX1);
            this.groupBox3.Controls.Add(this.numericUpDownY1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.numericUpDownX2);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.numericUpDownY2);
            this.groupBox3.Location = new System.Drawing.Point(15, 157);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(355, 85);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "From Region";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.numericUpDownToX1);
            this.groupBox4.Controls.Add(this.numericUpDownToY1);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(15, 249);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(355, 62);
            this.groupBox4.TabIndex = 22;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "To Region";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(63, 24);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 15);
            this.label6.TabIndex = 5;
            this.label6.Text = "X1";
            // 
            // numericUpDownToX1
            // 
            this.numericUpDownToX1.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownToX1.Location = new System.Drawing.Point(99, 22);
            this.numericUpDownToX1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownToX1.Name = "numericUpDownToX1";
            this.numericUpDownToX1.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownToX1.TabIndex = 4;
            // 
            // numericUpDownToY1
            // 
            this.numericUpDownToY1.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numericUpDownToY1.Location = new System.Drawing.Point(229, 22);
            this.numericUpDownToY1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDownToY1.Name = "numericUpDownToY1";
            this.numericUpDownToY1.Size = new System.Drawing.Size(63, 23);
            this.numericUpDownToY1.TabIndex = 6;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(192, 24);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 15);
            this.label7.TabIndex = 7;
            this.label7.Text = "Y1";
            // 
            // comboBoxMapID
            // 
            this.comboBoxMapID.FormattingEnabled = true;
            this.comboBoxMapID.Location = new System.Drawing.Point(105, 44);
            this.comboBoxMapID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBoxMapID.Name = "comboBoxMapID";
            this.comboBoxMapID.Size = new System.Drawing.Size(227, 23);
            this.comboBoxMapID.TabIndex = 23;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 47);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(45, 15);
            this.label8.TabIndex = 24;
            this.label8.Text = "Map ID";
            // 
            // MapReplaceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 381);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.comboBoxMapID);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(388, 285);
            this.Name = "MapReplaceForm";
            this.Text = "MapReplace";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownToX1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownToY1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBoxMap;
        private System.Windows.Forms.CheckBox checkBoxStatics;
        private System.Windows.Forms.ComboBox comboBoxMapID;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDownToX1;
        private System.Windows.Forms.NumericUpDown numericUpDownToY1;
        private System.Windows.Forms.NumericUpDown numericUpDownX1;
        private System.Windows.Forms.NumericUpDown numericUpDownX2;
        private System.Windows.Forms.NumericUpDown numericUpDownY1;
        private System.Windows.Forms.NumericUpDown numericUpDownY2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.CheckBox RemoveDupl;
        private System.Windows.Forms.TextBox textBox1;
    }
}