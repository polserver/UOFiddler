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
    partial class CompareItemControl
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
            tileViewOrg = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            tileViewSec = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            extractAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            jpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyItem2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            btnCopyAllDiff = new System.Windows.Forms.Button();
            pictureBoxOrg = new System.Windows.Forms.PictureBox();
            pictureBoxSec = new System.Windows.Forms.PictureBox();
            textBoxSecondDir = new System.Windows.Forms.TextBox();
            button1 = new System.Windows.Forms.Button();
            checkBox1 = new System.Windows.Forms.CheckBox();
            chkMultiSelect = new System.Windows.Forms.CheckBox();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            button2 = new System.Windows.Forms.Button();
            comboBoxFileMode = new System.Windows.Forms.ComboBox();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // tileViewOrg
            // 
            tileViewOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewOrg.Location = new System.Drawing.Point(4, 3);
            tileViewOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tileViewOrg.Name = "tileViewOrg";
            tileViewOrg.Size = new System.Drawing.Size(188, 303);
            tileViewOrg.TabIndex = 0;
            tileViewOrg.TileHighLightOpacity = 0D;
            tileViewOrg.FocusSelectionChanged += OnFocusChangedOrg;
            tileViewOrg.DrawItem += OnDrawItemOrg;
            tileViewOrg.SizeChanged += OnTileViewSizeChanged;
            // 
            // tileViewSec
            // 
            tileViewSec.ContextMenuStrip = contextMenuStrip1;
            tileViewSec.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewSec.Location = new System.Drawing.Point(527, 3);
            tileViewSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tileViewSec.Name = "tileViewSec";
            tileViewSec.Size = new System.Drawing.Size(189, 303);
            tileViewSec.TabIndex = 1;
            tileViewSec.TileHighLightOpacity = 0D;
            tileViewSec.FocusSelectionChanged += OnFocusChangedSec;
            tileViewSec.DrawItem += OnDrawItemSec;
            tileViewSec.SizeChanged += OnTileViewSizeChanged;
            tileViewSec.MouseDoubleClick += OnDoubleClickSec;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { extractAsToolStripMenuItem, copyItem2To1ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(164, 48);
            // 
            // extractAsToolStripMenuItem
            // 
            extractAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tiffToolStripMenuItem, bmpToolStripMenuItem, jpgToolStripMenuItem, pngToolStripMenuItem });
            extractAsToolStripMenuItem.Name = "extractAsToolStripMenuItem";
            extractAsToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            extractAsToolStripMenuItem.Text = "Export Image..";
            // 
            // tiffToolStripMenuItem
            // 
            tiffToolStripMenuItem.Name = "tiffToolStripMenuItem";
            tiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            tiffToolStripMenuItem.Text = "As Bmp";
            tiffToolStripMenuItem.Click += ExportAsBmp;
            // 
            // bmpToolStripMenuItem
            // 
            bmpToolStripMenuItem.Name = "bmpToolStripMenuItem";
            bmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            bmpToolStripMenuItem.Text = "As Tiff";
            bmpToolStripMenuItem.Click += ExportAsTiff;
            // 
            // jpgToolStripMenuItem
            // 
            jpgToolStripMenuItem.Name = "jpgToolStripMenuItem";
            jpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            jpgToolStripMenuItem.Text = "As Jpg";
            jpgToolStripMenuItem.Click += ExportAsJpg;
            // 
            // pngToolStripMenuItem
            // 
            pngToolStripMenuItem.Name = "pngToolStripMenuItem";
            pngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            pngToolStripMenuItem.Text = "As Png";
            pngToolStripMenuItem.Click += ExportAsPng;
            // 
            // copyItem2To1ToolStripMenuItem
            // 
            copyItem2To1ToolStripMenuItem.Name = "copyItem2To1ToolStripMenuItem";
            copyItem2To1ToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            copyItem2To1ToolStripMenuItem.Text = "Copy Item to left";
            copyItem2To1ToolStripMenuItem.Click += OnClickCopy;
            // 
            // btnCopyAllDiff
            // 
            btnCopyAllDiff.AutoSize = true;
            btnCopyAllDiff.Location = new System.Drawing.Point(598, 13);
            btnCopyAllDiff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnCopyAllDiff.Name = "btnCopyAllDiff";
            btnCopyAllDiff.Size = new System.Drawing.Size(99, 29);
            btnCopyAllDiff.TabIndex = 9;
            btnCopyAllDiff.Text = "Copy All Diff";
            btnCopyAllDiff.UseVisualStyleBackColor = true;
            btnCopyAllDiff.Click += OnClickCopyAllDiff;
            // 
            // pictureBoxOrg
            // 
            pictureBoxOrg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxOrg.Location = new System.Drawing.Point(5, 4);
            pictureBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxOrg.Name = "pictureBoxOrg";
            pictureBoxOrg.Size = new System.Drawing.Size(309, 144);
            pictureBoxOrg.TabIndex = 2;
            pictureBoxOrg.TabStop = false;
            // 
            // pictureBoxSec
            // 
            pictureBoxSec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxSec.Location = new System.Drawing.Point(5, 155);
            pictureBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxSec.Name = "pictureBoxSec";
            pictureBoxSec.Size = new System.Drawing.Size(309, 144);
            pictureBoxSec.TabIndex = 3;
            pictureBoxSec.TabStop = false;
            // 
            // textBoxSecondDir
            // 
            textBoxSecondDir.Location = new System.Drawing.Point(126, 16);
            textBoxSecondDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxSecondDir.Name = "textBoxSecondDir";
            textBoxSecondDir.Size = new System.Drawing.Size(168, 23);
            textBoxSecondDir.TabIndex = 4;
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.Location = new System.Drawing.Point(336, 13);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(99, 29);
            button1.TabIndex = 5;
            button1.Text = "Load Second";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickLoadSecond;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(443, 18);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(143, 19);
            checkBox1.TabIndex = 6;
            checkBox1.Text = "Show only Differences";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.Click += OnChangeShowDiff;
            // 
            // chkMultiSelect
            // 
            chkMultiSelect.AutoSize = true;
            chkMultiSelect.Location = new System.Drawing.Point(443, 41);
            chkMultiSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkMultiSelect.Name = "chkMultiSelect";
            chkMultiSelect.Size = new System.Drawing.Size(90, 19);
            chkMultiSelect.TabIndex = 10;
            chkMultiSelect.Text = "Multi-Select";
            chkMultiSelect.UseVisualStyleBackColor = true;
            chkMultiSelect.CheckedChanged += OnChangeMultiSelect;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pictureBoxSec, 0, 1);
            tableLayoutPanel1.Controls.Add(pictureBoxOrg, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(200, 3);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(319, 303);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 3;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.45454F));
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27273F));
            tableLayoutPanel2.Controls.Add(tileViewOrg, 0, 0);
            tableLayoutPanel2.Controls.Add(tileViewSec, 2, 0);
            tableLayoutPanel2.Controls.Add(tableLayoutPanel1, 1, 0);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Size = new System.Drawing.Size(720, 309);
            tableLayoutPanel2.TabIndex = 8;
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = System.Drawing.SystemColors.Control;
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
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(btnCopyAllDiff);
            splitContainer1.Panel2.Controls.Add(button2);
            splitContainer1.Panel2.Controls.Add(textBoxSecondDir);
            splitContainer1.Panel2.Controls.Add(checkBox1);
            splitContainer1.Panel2.Controls.Add(chkMultiSelect);
            splitContainer1.Panel2.Controls.Add(button1);
            splitContainer1.Panel2.Controls.Add(comboBoxFileMode);
            splitContainer1.Size = new System.Drawing.Size(720, 383);
            splitContainer1.SplitterDistance = 309;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 9;
            // 
            // button2
            // 
            button2.AutoSize = true;
            button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button2.Location = new System.Drawing.Point(302, 15);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(26, 25);
            button2.TabIndex = 7;
            button2.Text = "...";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickBrowse;
            // 
            // comboBoxFileMode
            // 
            comboBoxFileMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFileMode.FormattingEnabled = true;
            comboBoxFileMode.Items.AddRange(new object[] { "Auto", "MUL", "UOP" });
            comboBoxFileMode.Location = new System.Drawing.Point(5, 16);
            comboBoxFileMode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxFileMode.Name = "comboBoxFileMode";
            comboBoxFileMode.Size = new System.Drawing.Size(70, 23);
            comboBoxFileMode.TabIndex = 10;
            // 
            // CompareItemControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareItemControl";
            Size = new System.Drawing.Size(720, 383);
            Load += OnLoad;
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem bmpToolStripMenuItem;
        private System.Windows.Forms.Button btnCopyAllDiff;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox chkMultiSelect;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyItem2To1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pngToolStripMenuItem;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewOrg;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewSec;
        private System.Windows.Forms.PictureBox pictureBoxOrg;
        private System.Windows.Forms.PictureBox pictureBoxSec;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.ToolStripMenuItem tiffToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxFileMode;
    }
}
