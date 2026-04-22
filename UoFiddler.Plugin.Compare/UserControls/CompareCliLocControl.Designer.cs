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
            splitContainerMain = new System.Windows.Forms.SplitContainer();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            toolbarPanel = new System.Windows.Forms.Panel();
            button5 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            button4 = new System.Windows.Forms.Button();
            button3 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            textBox2 = new System.Windows.Forms.TextBox();
            textBox1 = new System.Windows.Forms.TextBox();
            splitContainerDiff = new System.Windows.Forms.SplitContainer();
            diffRichTextBox1 = new System.Windows.Forms.RichTextBox();
            labelDiff1 = new System.Windows.Forms.Label();
            diffRichTextBox2 = new System.Windows.Forms.RichTextBox();
            labelDiff2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).BeginInit();
            splitContainerMain.Panel1.SuspendLayout();
            splitContainerMain.Panel2.SuspendLayout();
            splitContainerMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            toolbarPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerDiff).BeginInit();
            splitContainerDiff.Panel1.SuspendLayout();
            splitContainerDiff.Panel2.SuspendLayout();
            splitContainerDiff.SuspendLayout();
            SuspendLayout();
            //
            // splitContainerMain
            //
            splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerMain.Location = new System.Drawing.Point(0, 0);
            splitContainerMain.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainerMain.Name = "splitContainerMain";
            splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainerMain.Panel1 — dataGrid fills top, toolbar docks to bottom
            //
            splitContainerMain.Panel1.Controls.Add(dataGridView1);
            splitContainerMain.Panel1.Controls.Add(toolbarPanel);
            //
            // splitContainerMain.Panel2 — diff detail
            //
            splitContainerMain.Panel2.Controls.Add(splitContainerDiff);
            splitContainerMain.Panel2MinSize = 80;
            splitContainerMain.Size = new System.Drawing.Size(729, 500);
            splitContainerMain.SplitterDistance = 377;
            splitContainerMain.SplitterWidth = 5;
            splitContainerMain.TabIndex = 0;
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
            dataGridView1.Size = new System.Drawing.Size(729, 282);
            dataGridView1.TabIndex = 0;
            dataGridView1.CellFormatting += CellFormatting;
            dataGridView1.ColumnHeaderMouseClick += OnHeaderClicked;
            dataGridView1.SelectionChanged += OnSelectionChanged;
            //
            // toolbarPanel
            //
            toolbarPanel.Controls.Add(button5);
            toolbarPanel.Controls.Add(checkBox1);
            toolbarPanel.Controls.Add(button4);
            toolbarPanel.Controls.Add(button3);
            toolbarPanel.Controls.Add(button2);
            toolbarPanel.Controls.Add(button1);
            toolbarPanel.Controls.Add(textBox2);
            toolbarPanel.Controls.Add(textBox1);
            toolbarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            toolbarPanel.Height = 95;
            toolbarPanel.Margin = new System.Windows.Forms.Padding(0);
            toolbarPanel.Name = "toolbarPanel";
            toolbarPanel.TabIndex = 1;
            //
            // button5
            //
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
            button4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
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
            button3.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
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
            button2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
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
            button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
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
            textBox2.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            textBox2.Location = new System.Drawing.Point(399, 64);
            textBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(236, 23);
            textBox2.TabIndex = 1;
            //
            // textBox1
            //
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left;
            textBox1.Location = new System.Drawing.Point(5, 64);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(223, 23);
            textBox1.TabIndex = 0;
            //
            // splitContainerDiff
            //
            splitContainerDiff.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainerDiff.Location = new System.Drawing.Point(0, 0);
            splitContainerDiff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainerDiff.Name = "splitContainerDiff";
            //
            // splitContainerDiff.Panel1
            //
            splitContainerDiff.Panel1.Controls.Add(diffRichTextBox1);
            splitContainerDiff.Panel1.Controls.Add(labelDiff1);
            //
            // splitContainerDiff.Panel2
            //
            splitContainerDiff.Panel2.Controls.Add(diffRichTextBox2);
            splitContainerDiff.Panel2.Controls.Add(labelDiff2);
            splitContainerDiff.Size = new System.Drawing.Size(729, 118);
            splitContainerDiff.SplitterDistance = 362;
            splitContainerDiff.SplitterWidth = 5;
            splitContainerDiff.TabIndex = 0;
            //
            // labelDiff1
            //
            labelDiff1.Dock = System.Windows.Forms.DockStyle.Top;
            labelDiff1.Location = new System.Drawing.Point(0, 0);
            labelDiff1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDiff1.Name = "labelDiff1";
            labelDiff1.Size = new System.Drawing.Size(362, 18);
            labelDiff1.TabIndex = 0;
            labelDiff1.Text = "File 1:";
            //
            // diffRichTextBox1
            //
            diffRichTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            diffRichTextBox1.Location = new System.Drawing.Point(0, 18);
            diffRichTextBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            diffRichTextBox1.Name = "diffRichTextBox1";
            diffRichTextBox1.ReadOnly = true;
            diffRichTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            diffRichTextBox1.Size = new System.Drawing.Size(362, 100);
            diffRichTextBox1.TabIndex = 1;
            diffRichTextBox1.Text = "";
            diffRichTextBox1.WordWrap = true;
            //
            // labelDiff2
            //
            labelDiff2.Dock = System.Windows.Forms.DockStyle.Top;
            labelDiff2.Location = new System.Drawing.Point(0, 0);
            labelDiff2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            labelDiff2.Name = "labelDiff2";
            labelDiff2.Size = new System.Drawing.Size(362, 18);
            labelDiff2.TabIndex = 0;
            labelDiff2.Text = "File 2:";
            //
            // diffRichTextBox2
            //
            diffRichTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            diffRichTextBox2.Location = new System.Drawing.Point(0, 18);
            diffRichTextBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            diffRichTextBox2.Name = "diffRichTextBox2";
            diffRichTextBox2.ReadOnly = true;
            diffRichTextBox2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            diffRichTextBox2.Size = new System.Drawing.Size(362, 100);
            diffRichTextBox2.TabIndex = 1;
            diffRichTextBox2.Text = "";
            diffRichTextBox2.WordWrap = true;
            //
            // CompareCliLocControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainerMain);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareCliLocControl";
            Size = new System.Drawing.Size(729, 500);
            splitContainerMain.Panel1.ResumeLayout(false);
            splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerMain).EndInit();
            splitContainerMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            toolbarPanel.ResumeLayout(false);
            toolbarPanel.PerformLayout();
            splitContainerDiff.Panel1.ResumeLayout(false);
            splitContainerDiff.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerDiff).EndInit();
            splitContainerDiff.ResumeLayout(false);
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
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private System.Windows.Forms.Panel toolbarPanel;
        private System.Windows.Forms.SplitContainer splitContainerDiff;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label labelDiff1;
        private System.Windows.Forms.Label labelDiff2;
        private System.Windows.Forms.RichTextBox diffRichTextBox1;
        private System.Windows.Forms.RichTextBox diffRichTextBox2;
    }
}
