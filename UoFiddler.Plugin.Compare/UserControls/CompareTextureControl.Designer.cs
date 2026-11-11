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
            components = new System.ComponentModel.Container();
            tileViewOrg = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            pixelDiffBar = new PixelDiffBar();
            pictureBoxSec = new System.Windows.Forms.PictureBox();
            pictureBoxOrg = new System.Windows.Forms.PictureBox();
            textBoxSecondDir = new System.Windows.Forms.TextBox();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            tileViewSec = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyLandTile2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            checkBoxShowDiff = new System.Windows.Forms.CheckBox();
            chkMultiSelect = new System.Windows.Forms.CheckBox();
            buttonLoadSecond = new System.Windows.Forms.Button();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            buttonCopyAddedOnly = new System.Windows.Forms.Button();
            buttonCopyAllDiff = new System.Windows.Forms.Button();
            buttonCopySelected = new System.Windows.Forms.Button();
            buttonBrowse = new System.Windows.Forms.Button();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).BeginInit();
            tableLayoutPanel2.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
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
            tileViewOrg.Size = new System.Drawing.Size(189, 363);
            tileViewOrg.TabIndex = 0;
            tileViewOrg.TileSize = new System.Drawing.Size(189, 20);
            tileViewOrg.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewOrg.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewOrg.TileBorderWidth = 0f;
            tileViewOrg.TileHighLightOpacity = 0D;
            tileViewOrg.FocusSelectionChanged += OnFocusChangedOrg;
            tileViewOrg.DrawItem += OnDrawItemOrg;
            tileViewOrg.SizeChanged += OnTileViewSizeChanged;
            //
            // pixelDiffBar
            //
            pixelDiffBar.Dock = System.Windows.Forms.DockStyle.Fill;
            pixelDiffBar.FirstPictureBox = pictureBoxOrg;
            pixelDiffBar.Margin = new System.Windows.Forms.Padding(0);
            pixelDiffBar.Name = "pixelDiffBar";
            pixelDiffBar.SecondPictureBox = pictureBoxSec;
            pixelDiffBar.TabIndex = 20;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pixelDiffBar, 0, 0);
            tableLayoutPanel1.Controls.Add(pictureBoxSec, 0, 2);
            tableLayoutPanel1.Controls.Add(pictureBoxOrg, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(201, 3);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(321, 363);
            tableLayoutPanel1.TabIndex = 7;
            // 
            // pictureBoxSec
            // 
            pictureBoxSec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxSec.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxSec.Location = new System.Drawing.Point(5, 185);
            pictureBoxSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxSec.Name = "pictureBoxSec";
            pictureBoxSec.Size = new System.Drawing.Size(311, 174);
            pictureBoxSec.TabIndex = 3;
            pictureBoxSec.TabStop = false;
            // 
            // pictureBoxOrg
            // 
            pictureBoxOrg.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxOrg.Location = new System.Drawing.Point(5, 4);
            pictureBoxOrg.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxOrg.Name = "pictureBoxOrg";
            pictureBoxOrg.Size = new System.Drawing.Size(311, 174);
            pictureBoxOrg.TabIndex = 2;
            pictureBoxOrg.TabStop = false;
            // 
            // textBoxSecondDir
            // 
            textBoxSecondDir.Location = new System.Drawing.Point(28, 13);
            textBoxSecondDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxSecondDir.Name = "textBoxSecondDir";
            textBoxSecondDir.Size = new System.Drawing.Size(168, 23);
            textBoxSecondDir.TabIndex = 4;
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
            tableLayoutPanel2.Size = new System.Drawing.Size(724, 369);
            tableLayoutPanel2.TabIndex = 8;
            // 
            // tileViewSec
            // 
            tileViewSec.ContextMenuStrip = contextMenuStrip1;
            tileViewSec.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewSec.Location = new System.Drawing.Point(530, 3);
            tileViewSec.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tileViewSec.Name = "tileViewSec";
            tileViewSec.Size = new System.Drawing.Size(190, 363);
            tileViewSec.TabIndex = 1;
            tileViewSec.TileSize = new System.Drawing.Size(190, 20);
            tileViewSec.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewSec.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewSec.TileBorderWidth = 0f;
            tileViewSec.TileHighLightOpacity = 0D;
            tileViewSec.FocusSelectionChanged += OnFocusChangedSec;
            tileViewSec.DrawItem += OnDrawItemSec;
            tileViewSec.SizeChanged += OnTileViewSizeChanged;
            tileViewSec.MouseDoubleClick += OnDoubleClickSec;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { exportImageToolStripMenuItem, copyLandTile2To1ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(182, 48);
            // 
            // exportImageToolStripMenuItem
            // 
            exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem, asPngToolStripMenuItem });
            exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            exportImageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += ExportAsBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += ExportAsTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += ExportAsJpg;
            // 
            // asPngToolStripMenuItem
            // 
            asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem.Text = "As Png";
            asPngToolStripMenuItem.Click += ExportAsPng;
            // 
            // copyLandTile2To1ToolStripMenuItem
            // 
            copyLandTile2To1ToolStripMenuItem.Name = "copyLandTile2To1ToolStripMenuItem";
            copyLandTile2To1ToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            copyLandTile2To1ToolStripMenuItem.Text = "Copy Texture to left";
            copyLandTile2To1ToolStripMenuItem.Click += OnClickCopy;
            // 
            // checkBoxShowDiff
            // 
            checkBoxShowDiff.AutoSize = true;
            checkBoxShowDiff.Location = new System.Drawing.Point(343, 15);
            checkBoxShowDiff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBoxShowDiff.Name = "checkBoxShowDiff";
            checkBoxShowDiff.Size = new System.Drawing.Size(143, 19);
            checkBoxShowDiff.TabIndex = 6;
            checkBoxShowDiff.Text = "Show only Differences";
            checkBoxShowDiff.UseVisualStyleBackColor = true;
            checkBoxShowDiff.Click += OnChangeShowDiff;
            //
            // chkMultiSelect
            //
            chkMultiSelect.AutoSize = true;
            chkMultiSelect.Location = new System.Drawing.Point(343, 38);
            chkMultiSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkMultiSelect.Name = "chkMultiSelect";
            chkMultiSelect.Size = new System.Drawing.Size(90, 19);
            chkMultiSelect.TabIndex = 10;
            chkMultiSelect.Text = "Multi-Select";
            chkMultiSelect.UseVisualStyleBackColor = true;
            chkMultiSelect.CheckedChanged += OnChangeMultiSelect;
            //
            // buttonLoadSecond
            // 
            buttonLoadSecond.Location = new System.Drawing.Point(238, 10);
            buttonLoadSecond.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonLoadSecond.Name = "buttonLoadSecond";
            buttonLoadSecond.Size = new System.Drawing.Size(100, 27);
            buttonLoadSecond.TabIndex = 5;
            buttonLoadSecond.Text = "Load Second";
            buttonLoadSecond.UseVisualStyleBackColor = true;
            buttonLoadSecond.Click += OnClickLoadSecond;
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
            splitContainer1.Panel2.Controls.Add(buttonCopyAddedOnly);
            splitContainer1.Panel2.Controls.Add(buttonCopySelected);
            splitContainer1.Panel2.Controls.Add(buttonCopyAllDiff);
            splitContainer1.Panel2.Controls.Add(buttonBrowse);
            splitContainer1.Panel2.Controls.Add(textBoxSecondDir);
            splitContainer1.Panel2.Controls.Add(checkBoxShowDiff);
            splitContainer1.Panel2.Controls.Add(chkMultiSelect);
            splitContainer1.Panel2.Controls.Add(buttonLoadSecond);
            splitContainer1.Size = new System.Drawing.Size(820, 434);
            splitContainer1.SplitterDistance = 369;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 10;
            // 
            // buttonCopyAddedOnly
            // 
            buttonCopyAddedOnly.Location = new System.Drawing.Point(706, 12);
            buttonCopyAddedOnly.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonCopyAddedOnly.Name = "buttonCopyAddedOnly";
            buttonCopyAddedOnly.Size = new System.Drawing.Size(100, 27);
            buttonCopyAddedOnly.TabIndex = 9;
            buttonCopyAddedOnly.Text = "Copy Added Only";
            buttonCopyAddedOnly.UseVisualStyleBackColor = true;
            buttonCopyAddedOnly.Click += OnClickCopyAddedOnly;
            // 
            // buttonCopySelected
            // 
            buttonCopySelected.Location = new System.Drawing.Point(494, 12);
            buttonCopySelected.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonCopySelected.Name = "buttonCopySelected";
            buttonCopySelected.Size = new System.Drawing.Size(100, 27);
            buttonCopySelected.TabIndex = 12;
            buttonCopySelected.Text = "Copy Selected";
            buttonCopySelected.UseVisualStyleBackColor = true;
            buttonCopySelected.Click += OnClickCopy;
            // 
            // buttonCopyAllDiff
            // 
            buttonCopyAllDiff.Location = new System.Drawing.Point(600, 12);
            buttonCopyAllDiff.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonCopyAllDiff.Name = "buttonCopyAllDiff";
            buttonCopyAllDiff.Size = new System.Drawing.Size(100, 27);
            buttonCopyAllDiff.TabIndex = 8;
            buttonCopyAllDiff.Text = "Copy All Diff";
            buttonCopyAllDiff.UseVisualStyleBackColor = true;
            buttonCopyAllDiff.Click += OnClickCopyAllDiff;
            // 
            // buttonBrowse
            // 
            buttonBrowse.AutoSize = true;
            buttonBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBrowse.Location = new System.Drawing.Point(204, 12);
            buttonBrowse.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new System.Drawing.Size(26, 25);
            buttonBrowse.TabIndex = 7;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += BrowseOnClick;
            // 
            // CompareTextureControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareTextureControl";
            Size = new System.Drawing.Size(820, 434);
            Load += OnLoad;
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxSec).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrg).EndInit();
            tableLayoutPanel2.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewOrg;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private PixelDiffBar pixelDiffBar;
        private System.Windows.Forms.PictureBox pictureBoxSec;
        private System.Windows.Forms.PictureBox pictureBoxOrg;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewSec;
        private System.Windows.Forms.CheckBox checkBoxShowDiff;
        private System.Windows.Forms.CheckBox chkMultiSelect;
        private System.Windows.Forms.Button buttonLoadSecond;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.ToolStripMenuItem copyLandTile2To1ToolStripMenuItem;
        private System.Windows.Forms.Button buttonCopyAllDiff;
        private System.Windows.Forms.Button buttonCopySelected;
        private System.Windows.Forms.Button buttonCopyAddedOnly;
    }
}
