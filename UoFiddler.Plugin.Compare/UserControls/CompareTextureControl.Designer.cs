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
    partial class CompareTextureControl
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
            this.components = new System.ComponentModel.Container();
            this.listBoxOrg = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBoxSec = new System.Windows.Forms.PictureBox();
            this.pictureBoxOrg = new System.Windows.Forms.PictureBox();
            this.textBoxSecondDir = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.listBoxSec = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyLandTile2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CopyAddOnly = new System.Windows.Forms.Button();
            this.FromLeftToRight = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSec)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOrg)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxOrg
            // 
            this.listBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxOrg.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxOrg.FormattingEnabled = true;
            this.listBoxOrg.IntegralHeight = false;
            this.listBoxOrg.Location = new System.Drawing.Point(4, 3);
            this.listBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listBoxOrg.Name = "listBoxOrg";
            this.listBoxOrg.Size = new System.Drawing.Size(190, 374);
            this.listBoxOrg.TabIndex = 0;
            this.listBoxOrg.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.DrawitemOrg);
            this.listBoxOrg.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.MeasureOrg);
            this.listBoxOrg.SelectedIndexChanged += new System.EventHandler(this.OnIndexChangedOrg);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBoxSec, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.pictureBoxOrg, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(202, 3);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(322, 374);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // pictureBoxSec
            // 
            this.pictureBoxSec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxSec.Location = new System.Drawing.Point(5, 190);
            this.pictureBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBoxSec.Name = "pictureBoxSec";
            this.pictureBoxSec.Size = new System.Drawing.Size(312, 180);
            this.pictureBoxSec.TabIndex = 3;
            this.pictureBoxSec.TabStop = false;
            // 
            // pictureBoxOrg
            // 
            this.pictureBoxOrg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxOrg.Location = new System.Drawing.Point(5, 4);
            this.pictureBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBoxOrg.Name = "pictureBoxOrg";
            this.pictureBoxOrg.Size = new System.Drawing.Size(312, 179);
            this.pictureBoxOrg.TabIndex = 2;
            this.pictureBoxOrg.TabStop = false;
            // 
            // textBoxSecondDir
            // 
            this.textBoxSecondDir.Location = new System.Drawing.Point(25, 14);
            this.textBoxSecondDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxSecondDir.Name = "textBoxSecondDir";
            this.textBoxSecondDir.Size = new System.Drawing.Size(168, 23);
            this.textBoxSecondDir.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.45454F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            this.tableLayoutPanel2.Controls.Add(this.listBoxOrg, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.listBoxSec, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel1, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(728, 380);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // listBoxSec
            // 
            this.listBoxSec.ContextMenuStrip = this.contextMenuStrip1;
            this.listBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxSec.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.listBoxSec.FormattingEnabled = true;
            this.listBoxSec.IntegralHeight = false;
            this.listBoxSec.Location = new System.Drawing.Point(532, 3);
            this.listBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listBoxSec.Name = "listBoxSec";
            this.listBoxSec.Size = new System.Drawing.Size(192, 374);
            this.listBoxSec.TabIndex = 1;
            this.listBoxSec.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.DrawItemSec);
            this.listBoxSec.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.MeasureSec);
            this.listBoxSec.SelectedIndexChanged += new System.EventHandler(this.OnIndexChangedSec);
            this.listBoxSec.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.CopyToLeft_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportImageToolStripMenuItem,
            this.copyLandTile2To1ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(182, 48);
            // 
            // exportImageToolStripMenuItem
            // 
            this.exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem});
            this.exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            this.exportImageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.ExportAsBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.ExportAsTiff);
            // 
            // copyLandTile2To1ToolStripMenuItem
            // 
            this.copyLandTile2To1ToolStripMenuItem.Name = "copyLandTile2To1ToolStripMenuItem";
            this.copyLandTile2To1ToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.copyLandTile2To1ToolStripMenuItem.Text = "Copy LandTile 2 to 1";
            this.copyLandTile2To1ToolStripMenuItem.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(342, 16);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(143, 19);
            this.checkBox1.TabIndex = 6;
            this.checkBox1.Text = "Show only Differences";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new System.EventHandler(this.OnChangeShowDiff);
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.Location = new System.Drawing.Point(235, 10);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(99, 29);
            this.button1.TabIndex = 5;
            this.button1.Text = "Load Second";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickLoadSecond);
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.CopyAddOnly);
            this.splitContainer1.Panel2.Controls.Add(this.FromLeftToRight);
            this.splitContainer1.Panel2.Controls.Add(this.button2);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxSecondDir);
            this.splitContainer1.Panel2.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Size = new System.Drawing.Size(728, 434);
            this.splitContainer1.SplitterDistance = 380;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 10;
            // 
            // CopyAddOnly
            // 
            this.CopyAddOnly.Location = new System.Drawing.Point(598, 12);
            this.CopyAddOnly.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.CopyAddOnly.Name = "CopyAddOnly";
            this.CopyAddOnly.Size = new System.Drawing.Size(88, 27);
            this.CopyAddOnly.TabIndex = 9;
            this.CopyAddOnly.Text = "Copy Added Only";
            this.CopyAddOnly.UseVisualStyleBackColor = true;
            this.CopyAddOnly.Click += new System.EventHandler(this.CopyAddOnly_Click);
            // 
            // FromLeftToRight
            // 
            this.FromLeftToRight.Location = new System.Drawing.Point(502, 12);
            this.FromLeftToRight.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.FromLeftToRight.Name = "FromLeftToRight";
            this.FromLeftToRight.Size = new System.Drawing.Size(88, 27);
            this.FromLeftToRight.TabIndex = 8;
            this.FromLeftToRight.Text = "Copy All Diff";
            this.FromLeftToRight.UseVisualStyleBackColor = true;
            this.FromLeftToRight.Click += new System.EventHandler(this.CopyAll_Click);
            // 
            // button2
            // 
            this.button2.AutoSize = true;
            this.button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button2.Location = new System.Drawing.Point(201, 12);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(26, 25);
            this.button2.TabIndex = 7;
            this.button2.Text = "...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.BrowseOnClick);
            // 
            // CompareTextureControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CompareTextureControl";
            this.Size = new System.Drawing.Size(728, 434);
            this.Load += new System.EventHandler(this.OnLoad);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSec)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOrg)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxOrg;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBoxSec;
        private System.Windows.Forms.PictureBox pictureBoxOrg;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ListBox listBoxSec;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ToolStripMenuItem copyLandTile2To1ToolStripMenuItem;
        private System.Windows.Forms.Button FromLeftToRight;
        private System.Windows.Forms.Button CopyAddOnly;
    }
}
