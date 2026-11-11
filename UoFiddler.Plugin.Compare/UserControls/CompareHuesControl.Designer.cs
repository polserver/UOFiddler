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
    partial class CompareHuesControl
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
            components = new System.ComponentModel.Container();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            applyHue1ToHue2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            vScrollBar = new System.Windows.Forms.VScrollBar();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            buttonBrowse = new System.Windows.Forms.Button();
            buttonLoadSecond = new System.Windows.Forms.Button();
            buttonCopySelected = new System.Windows.Forms.Button();
            chkMultiSelect = new System.Windows.Forms.CheckBox();
            textBox1 = new System.Windows.Forms.TextBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(pictureBox2, 2, 0);
            tableLayoutPanel1.Controls.Add(vScrollBar, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(790, 318);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.ContextMenuStrip = contextMenuStrip1;
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox1.Location = new System.Drawing.Point(4, 3);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(377, 312);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.SizeChanged += OnResizeHue;
            pictureBox1.MouseClick += OnMouseClick1;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { applyHue1ToHue2ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(165, 26);
            // 
            // applyHue1ToHue2ToolStripMenuItem
            // 
            applyHue1ToHue2ToolStripMenuItem.Name = "applyHue1ToHue2ToolStripMenuItem";
            applyHue1ToHue2ToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            applyHue1ToHue2ToolStripMenuItem.Text = "Apply Hue to left";
            applyHue1ToHue2ToolStripMenuItem.Click += OnClickApplyHue1to2;
            // 
            // pictureBox2
            // 
            pictureBox2.ContextMenuStrip = contextMenuStrip1;
            pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox2.Location = new System.Drawing.Point(409, 3);
            pictureBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(377, 312);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            pictureBox2.SizeChanged += OnResizeHue;
            pictureBox2.MouseClick += OnMouseClick2;
            // 
            // vScrollBar
            // 
            vScrollBar.Dock = System.Windows.Forms.DockStyle.Fill;
            vScrollBar.Location = new System.Drawing.Point(385, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new System.Drawing.Size(20, 318);
            vScrollBar.TabIndex = 2;
            vScrollBar.Scroll += OnScroll;
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
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(buttonBrowse);
            splitContainer1.Panel2.Controls.Add(buttonLoadSecond);
            splitContainer1.Panel2.Controls.Add(buttonCopySelected);
            splitContainer1.Panel2.Controls.Add(chkMultiSelect);
            splitContainer1.Panel2.Controls.Add(textBox1);
            splitContainer1.Size = new System.Drawing.Size(790, 380);
            splitContainer1.SplitterDistance = 318;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // buttonBrowse
            // 
            buttonBrowse.AutoSize = true;
            buttonBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBrowse.Location = new System.Drawing.Point(283, 14);
            buttonBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new System.Drawing.Size(26, 25);
            buttonBrowse.TabIndex = 2;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += BrowseOnClick;
            // 
            // buttonLoadSecond
            // 
            buttonLoadSecond.Location = new System.Drawing.Point(317, 13);
            buttonLoadSecond.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonLoadSecond.Name = "buttonLoadSecond";
            buttonLoadSecond.Size = new System.Drawing.Size(100, 27);
            buttonLoadSecond.TabIndex = 1;
            buttonLoadSecond.Text = "Load Second";
            buttonLoadSecond.UseVisualStyleBackColor = true;
            buttonLoadSecond.Click += OnClickLoad;
            // 
            // buttonCopySelected
            // 
            buttonCopySelected.Location = new System.Drawing.Point(549, 13);
            buttonCopySelected.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonCopySelected.Name = "buttonCopySelected";
            buttonCopySelected.Size = new System.Drawing.Size(100, 27);
            buttonCopySelected.TabIndex = 12;
            buttonCopySelected.Text = "Copy Selected";
            buttonCopySelected.UseVisualStyleBackColor = true;
            buttonCopySelected.Click += OnClickApplyHue1to2;
            // 
            // chkMultiSelect
            // 
            chkMultiSelect.AutoSize = true;
            chkMultiSelect.Location = new System.Drawing.Point(452, 17);
            chkMultiSelect.Name = "chkMultiSelect";
            chkMultiSelect.Size = new System.Drawing.Size(90, 19);
            chkMultiSelect.TabIndex = 10;
            chkMultiSelect.Text = "Multi-Select";
            chkMultiSelect.UseVisualStyleBackColor = true;
            chkMultiSelect.CheckedChanged += OnChangeMultiSelect;
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(4, 16);
            textBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBox1.MaximumSize = new System.Drawing.Size(271, 20);
            textBox1.MinimumSize = new System.Drawing.Size(271, 20);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(271, 20);
            textBox1.TabIndex = 0;
            toolTip1.SetToolTip(textBox1, "Directory of hues.mul");
            // 
            // CompareHuesControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareHuesControl";
            Size = new System.Drawing.Size(790, 380);
            Load += OnLoad;
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem applyHue1ToHue2ToolStripMenuItem;
        private System.Windows.Forms.Button buttonLoadSecond;
        private System.Windows.Forms.CheckBox chkMultiSelect;
        private System.Windows.Forms.Button buttonCopySelected;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.VScrollBar vScrollBar;
    }
}
