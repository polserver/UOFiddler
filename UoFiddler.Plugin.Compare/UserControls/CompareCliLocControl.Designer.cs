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

namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class CompareCliLocControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            decompressFileTwoCheckBox = new System.Windows.Forms.CheckBox();
            decompressFileOneCheckBox = new System.Windows.Forms.CheckBox();
            button5 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            button4 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            textBox2 = new System.Windows.Forms.TextBox();
            textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(decompressFileTwoCheckBox);
            splitContainer1.Panel2.Controls.Add(decompressFileOneCheckBox);
            splitContainer1.Panel2.Controls.Add(button5);
            splitContainer1.Panel2.Controls.Add(checkBox1);
            splitContainer1.Panel2.Controls.Add(button4);
            splitContainer1.Panel2.Controls.Add(button3);
            splitContainer1.Panel2.Controls.Add(button2);
            splitContainer1.Panel2.Controls.Add(button1);
            splitContainer1.Panel2.Controls.Add(textBox2);
            splitContainer1.Panel2.Controls.Add(textBox1);
            splitContainer1.Size = new System.Drawing.Size(729, 377);
            splitContainer1.SplitterDistance = 278;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.Location = new System.Drawing.Point(0, 0);
            dataGridView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            dataGridView1.MultiSelect = false;
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new System.Drawing.Size(729, 278);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellFormatting += CellFormatting;
            dataGridView1.ColumnHeaderMouseClick += OnHeaderClicked;
            // 
            // decompressFileTwoCheckBox
            // 
            decompressFileTwoCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            decompressFileTwoCheckBox.AutoSize = true;
            decompressFileTwoCheckBox.Location = new System.Drawing.Point(399, 39);
            decompressFileTwoCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            decompressFileTwoCheckBox.Name = "decompressFileTwoCheckBox";
            decompressFileTwoCheckBox.Size = new System.Drawing.Size(202, 19);
            decompressFileTwoCheckBox.TabIndex = 9;
            decompressFileTwoCheckBox.Text = "Decompress cliloc before reading";
            decompressFileTwoCheckBox.UseVisualStyleBackColor = true;
            // 
            // decompressFileOneCheckBox
            // 
            decompressFileOneCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            decompressFileOneCheckBox.AutoSize = true;
            decompressFileOneCheckBox.Location = new System.Drawing.Point(5, 39);
            decompressFileOneCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            decompressFileOneCheckBox.Name = "decompressFileOneCheckBox";
            decompressFileOneCheckBox.Size = new System.Drawing.Size(202, 19);
            decompressFileOneCheckBox.TabIndex = 8;
            decompressFileOneCheckBox.Text = "Decompress cliloc before reading";
            decompressFileOneCheckBox.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            button5.AutoSize = true;
            button5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button5.Location = new System.Drawing.Point(399, 7);
            button5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(90, 25);
            button5.TabIndex = 7;
            button5.Text = "Find Next Diff";
            button5.UseVisualStyleBackColor = true;
            button5.Click += OnClickFindNextDiff;
            // 
            // checkBox1
            // 
            checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(236, 11);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(145, 19);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Show Only Differences";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.Click += OnClickShowOnlyDiff;
            // 
            // button4
            // 
            button4.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button4.AutoSize = true;
            button4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button4.Location = new System.Drawing.Point(647, 63);
            button4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(23, 25);
            button4.TabIndex = 5;
            button4.Text = "..";
            button4.UseVisualStyleBackColor = true;
            button4.Click += OnClickDirFile2;
            // 
            // button3
            // 
            button3.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            button3.AutoSize = true;
            button3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button3.Location = new System.Drawing.Point(236, 63);
            button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(23, 25);
            button3.TabIndex = 4;
            button3.Text = "..";
            button3.UseVisualStyleBackColor = true;
            button3.Click += OnClickDirFile1;
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            button2.AutoSize = true;
            button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button2.Location = new System.Drawing.Point(681, 63);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(43, 25);
            button2.TabIndex = 3;
            button2.Text = "Load";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnLoad2;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            button1.AutoSize = true;
            button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button1.Location = new System.Drawing.Point(270, 63);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(43, 25);
            button1.TabIndex = 2;
            button1.Text = "Load";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnLoad;
            // 
            // textBox2
            // 
            textBox2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            textBox2.Location = new System.Drawing.Point(399, 64);
            textBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(236, 23);
            textBox2.TabIndex = 1;
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            textBox1.Location = new System.Drawing.Point(5, 64);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(223, 23);
            textBox1.TabIndex = 0;
            // 
            // CompareCliLocControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareCliLocControl";
            Size = new System.Drawing.Size(729, 377);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox decompressFileTwoCheckBox;
        private System.Windows.Forms.CheckBox decompressFileOneCheckBox;
    }
}
