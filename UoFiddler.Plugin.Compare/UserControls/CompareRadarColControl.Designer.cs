namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class CompareRadarColControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tabControl = new System.Windows.Forms.TabControl();
            tabPageLand = new System.Windows.Forms.TabPage();
            tableLayoutLand = new System.Windows.Forms.TableLayoutPanel();
            tileViewOrg = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            contextMenuStripOrg = new System.Windows.Forms.ContextMenuStrip(components);
            copyEntry1To2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            panelDetail = new System.Windows.Forms.Panel();
            groupBoxOrg = new System.Windows.Forms.GroupBox();
            labelOrgColorCaption = new System.Windows.Forms.Label();
            labelOrgColorValue = new System.Windows.Forms.Label();
            pictureBoxOrgColor = new System.Windows.Forms.PictureBox();
            groupBoxSec = new System.Windows.Forms.GroupBox();
            labelSecColorCaption = new System.Windows.Forms.Label();
            labelSecColorValue = new System.Windows.Forms.Label();
            pictureBoxSecColor = new System.Windows.Forms.PictureBox();
            groupBoxLegend = new System.Windows.Forms.GroupBox();
            legendSwatchDifferent = new System.Windows.Forms.Label();
            legendLabelDifferent = new System.Windows.Forms.Label();
            legendSwatchIdentical = new System.Windows.Forms.Label();
            legendLabelIdentical = new System.Windows.Forms.Label();
            tileViewSec = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            contextMenuStripSec = new System.Windows.Forms.ContextMenuStrip(components);
            copyEntry2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tabPageItem = new System.Windows.Forms.TabPage();
            tableLayoutItem = new System.Windows.Forms.TableLayoutPanel();
            tileViewItemOrg = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            tileViewItemSec = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            buttonCopyAllDiff = new System.Windows.Forms.Button();
            buttonCopySelected = new System.Windows.Forms.Button();
            checkBoxShowDiff = new System.Windows.Forms.CheckBox();
            buttonLoadSecond = new System.Windows.Forms.Button();
            buttonBrowse = new System.Windows.Forms.Button();
            textBoxSecondFile = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabControl.SuspendLayout();
            tabPageLand.SuspendLayout();
            tableLayoutLand.SuspendLayout();
            contextMenuStripOrg.SuspendLayout();
            panelDetail.SuspendLayout();
            groupBoxOrg.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrgColor).BeginInit();
            groupBoxSec.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSecColor).BeginInit();
            groupBoxLegend.SuspendLayout();
            contextMenuStripSec.SuspendLayout();
            tabPageItem.SuspendLayout();
            tableLayoutItem.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControl);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(buttonCopyAllDiff);
            splitContainer1.Panel2.Controls.Add(buttonCopySelected);
            splitContainer1.Panel2.Controls.Add(checkBoxShowDiff);
            splitContainer1.Panel2.Controls.Add(buttonLoadSecond);
            splitContainer1.Panel2.Controls.Add(buttonBrowse);
            splitContainer1.Panel2.Controls.Add(textBoxSecondFile);
            splitContainer1.Size = new System.Drawing.Size(940, 510);
            splitContainer1.SplitterDistance = 449;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPageLand);
            tabControl.Controls.Add(tabPageItem);
            tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl.Location = new System.Drawing.Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new System.Drawing.Size(940, 449);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += OnTabChanged;
            // 
            // tabPageLand
            // 
            tabPageLand.Controls.Add(tableLayoutLand);
            tabPageLand.Location = new System.Drawing.Point(4, 24);
            tabPageLand.Name = "tabPageLand";
            tabPageLand.Size = new System.Drawing.Size(932, 421);
            tabPageLand.TabIndex = 0;
            tabPageLand.Text = "Land Tiles";
            tabPageLand.UseVisualStyleBackColor = true;
            // 
            // tableLayoutLand
            // 
            tableLayoutLand.ColumnCount = 3;
            tableLayoutLand.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27F));
            tableLayoutLand.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.46F));
            tableLayoutLand.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27F));
            tableLayoutLand.Controls.Add(tileViewOrg, 0, 0);
            tableLayoutLand.Controls.Add(panelDetail, 1, 0);
            tableLayoutLand.Controls.Add(tileViewSec, 2, 0);
            tableLayoutLand.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutLand.Location = new System.Drawing.Point(0, 0);
            tableLayoutLand.Name = "tableLayoutLand";
            tableLayoutLand.RowCount = 1;
            tableLayoutLand.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutLand.Size = new System.Drawing.Size(932, 421);
            tableLayoutLand.TabIndex = 0;
            // 
            // tileViewOrg
            // 
            tileViewOrg.ContextMenuStrip = contextMenuStripOrg;
            tileViewOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewOrg.FocusIndex = -1;
            tileViewOrg.Location = new System.Drawing.Point(3, 3);
            tileViewOrg.MultiSelect = false;
            tileViewOrg.Name = "tileViewOrg";
            tileViewOrg.Size = new System.Drawing.Size(248, 415);
            tileViewOrg.TabIndex = 0;
            tileViewOrg.TileBackgroundColor = System.Drawing.SystemColors.Window;
            tileViewOrg.TileBorderColor = System.Drawing.Color.FromArgb(0, 0, 0);
            tileViewOrg.TileBorderWidth = 0F;
            tileViewOrg.TileFocusColor = System.Drawing.Color.DarkRed;
            tileViewOrg.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            tileViewOrg.TileHighLightOpacity = 0D;
            tileViewOrg.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewOrg.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewOrg.TileSize = new System.Drawing.Size(248, 15);
            tileViewOrg.VirtualListSize = 0;
            tileViewOrg.FocusSelectionChanged += OnFocusChangedLandOrg;
            tileViewOrg.DrawItem += OnDrawItemLandOrg;
            tileViewOrg.SizeChanged += OnTileViewSizeChanged;
            tileViewOrg.MouseDoubleClick += OnDoubleClickOrg;
            // 
            // contextMenuStripOrg
            // 
            contextMenuStripOrg.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyEntry1To2ToolStripMenuItem });
            contextMenuStripOrg.Name = "contextMenuStripOrg";
            contextMenuStripOrg.Size = new System.Drawing.Size(165, 26);
            // 
            // copyEntry1To2ToolStripMenuItem
            // 
            copyEntry1To2ToolStripMenuItem.Name = "copyEntry1To2ToolStripMenuItem";
            copyEntry1To2ToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            copyEntry1To2ToolStripMenuItem.Text = "Copy Entry 1 to 2";
            copyEntry1To2ToolStripMenuItem.Click += OnClickCopy1To2;
            // 
            // panelDetail
            // 
            panelDetail.BackColor = System.Drawing.SystemColors.Control;
            panelDetail.Controls.Add(groupBoxOrg);
            panelDetail.Controls.Add(groupBoxSec);
            panelDetail.Controls.Add(groupBoxLegend);
            panelDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            panelDetail.Location = new System.Drawing.Point(257, 3);
            panelDetail.Name = "panelDetail";
            panelDetail.Size = new System.Drawing.Size(417, 415);
            panelDetail.TabIndex = 1;
            // 
            // groupBoxOrg
            // 
            groupBoxOrg.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxOrg.Controls.Add(labelOrgColorCaption);
            groupBoxOrg.Controls.Add(labelOrgColorValue);
            groupBoxOrg.Controls.Add(pictureBoxOrgColor);
            groupBoxOrg.Location = new System.Drawing.Point(6, 3);
            groupBoxOrg.Name = "groupBoxOrg";
            groupBoxOrg.Size = new System.Drawing.Size(405, 155);
            groupBoxOrg.TabIndex = 0;
            groupBoxOrg.TabStop = false;
            groupBoxOrg.Text = "Left (Original)";
            // 
            // labelOrgColorCaption
            // 
            labelOrgColorCaption.AutoSize = true;
            labelOrgColorCaption.Location = new System.Drawing.Point(8, 115);
            labelOrgColorCaption.Name = "labelOrgColorCaption";
            labelOrgColorCaption.Size = new System.Drawing.Size(39, 15);
            labelOrgColorCaption.TabIndex = 0;
            labelOrgColorCaption.Text = "Color:";
            // 
            // labelOrgColorValue
            // 
            labelOrgColorValue.AutoSize = true;
            labelOrgColorValue.Location = new System.Drawing.Point(55, 115);
            labelOrgColorValue.Name = "labelOrgColorValue";
            labelOrgColorValue.Size = new System.Drawing.Size(12, 15);
            labelOrgColorValue.TabIndex = 1;
            labelOrgColorValue.Text = "-";
            // 
            // pictureBoxOrgColor
            // 
            pictureBoxOrgColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxOrgColor.Location = new System.Drawing.Point(60, 22);
            pictureBoxOrgColor.Name = "pictureBoxOrgColor";
            pictureBoxOrgColor.Size = new System.Drawing.Size(80, 80);
            pictureBoxOrgColor.TabIndex = 2;
            pictureBoxOrgColor.TabStop = false;
            // 
            // groupBoxSec
            // 
            groupBoxSec.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxSec.Controls.Add(labelSecColorCaption);
            groupBoxSec.Controls.Add(labelSecColorValue);
            groupBoxSec.Controls.Add(pictureBoxSecColor);
            groupBoxSec.Location = new System.Drawing.Point(6, 164);
            groupBoxSec.Name = "groupBoxSec";
            groupBoxSec.Size = new System.Drawing.Size(405, 155);
            groupBoxSec.TabIndex = 1;
            groupBoxSec.TabStop = false;
            groupBoxSec.Text = "Right (Second)";
            // 
            // labelSecColorCaption
            // 
            labelSecColorCaption.AutoSize = true;
            labelSecColorCaption.Location = new System.Drawing.Point(8, 115);
            labelSecColorCaption.Name = "labelSecColorCaption";
            labelSecColorCaption.Size = new System.Drawing.Size(39, 15);
            labelSecColorCaption.TabIndex = 0;
            labelSecColorCaption.Text = "Color:";
            // 
            // labelSecColorValue
            // 
            labelSecColorValue.AutoSize = true;
            labelSecColorValue.Location = new System.Drawing.Point(55, 115);
            labelSecColorValue.Name = "labelSecColorValue";
            labelSecColorValue.Size = new System.Drawing.Size(12, 15);
            labelSecColorValue.TabIndex = 1;
            labelSecColorValue.Text = "-";
            // 
            // pictureBoxSecColor
            // 
            pictureBoxSecColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            pictureBoxSecColor.Location = new System.Drawing.Point(60, 22);
            pictureBoxSecColor.Name = "pictureBoxSecColor";
            pictureBoxSecColor.Size = new System.Drawing.Size(80, 80);
            pictureBoxSecColor.TabIndex = 2;
            pictureBoxSecColor.TabStop = false;
            // 
            // groupBoxLegend
            // 
            groupBoxLegend.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxLegend.Controls.Add(legendSwatchDifferent);
            groupBoxLegend.Controls.Add(legendLabelDifferent);
            groupBoxLegend.Controls.Add(legendSwatchIdentical);
            groupBoxLegend.Controls.Add(legendLabelIdentical);
            groupBoxLegend.Location = new System.Drawing.Point(6, 325);
            groupBoxLegend.Name = "groupBoxLegend";
            groupBoxLegend.Size = new System.Drawing.Size(405, 70);
            groupBoxLegend.TabIndex = 2;
            groupBoxLegend.TabStop = false;
            groupBoxLegend.Text = "Legend";
            // 
            // legendSwatchDifferent
            // 
            legendSwatchDifferent.BackColor = System.Drawing.Color.Blue;
            legendSwatchDifferent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            legendSwatchDifferent.Location = new System.Drawing.Point(8, 22);
            legendSwatchDifferent.Name = "legendSwatchDifferent";
            legendSwatchDifferent.Size = new System.Drawing.Size(16, 16);
            legendSwatchDifferent.TabIndex = 0;
            // 
            // legendLabelDifferent
            // 
            legendLabelDifferent.AutoSize = true;
            legendLabelDifferent.Location = new System.Drawing.Point(30, 22);
            legendLabelDifferent.Name = "legendLabelDifferent";
            legendLabelDifferent.Size = new System.Drawing.Size(89, 15);
            legendLabelDifferent.TabIndex = 1;
            legendLabelDifferent.Text = "Different values";
            // 
            // legendSwatchIdentical
            // 
            legendSwatchIdentical.BackColor = System.Drawing.Color.Gray;
            legendSwatchIdentical.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            legendSwatchIdentical.Location = new System.Drawing.Point(8, 46);
            legendSwatchIdentical.Name = "legendSwatchIdentical";
            legendSwatchIdentical.Size = new System.Drawing.Size(16, 16);
            legendSwatchIdentical.TabIndex = 2;
            // 
            // legendLabelIdentical
            // 
            legendLabelIdentical.AutoSize = true;
            legendLabelIdentical.Location = new System.Drawing.Point(30, 46);
            legendLabelIdentical.Name = "legendLabelIdentical";
            legendLabelIdentical.Size = new System.Drawing.Size(52, 15);
            legendLabelIdentical.TabIndex = 3;
            legendLabelIdentical.Text = "Identical";
            // 
            // tileViewSec
            // 
            tileViewSec.ContextMenuStrip = contextMenuStripSec;
            tileViewSec.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewSec.FocusIndex = -1;
            tileViewSec.Location = new System.Drawing.Point(680, 3);
            tileViewSec.MultiSelect = false;
            tileViewSec.Name = "tileViewSec";
            tileViewSec.Size = new System.Drawing.Size(249, 415);
            tileViewSec.TabIndex = 2;
            tileViewSec.TileBackgroundColor = System.Drawing.SystemColors.Window;
            tileViewSec.TileBorderColor = System.Drawing.Color.FromArgb(0, 0, 0);
            tileViewSec.TileBorderWidth = 0F;
            tileViewSec.TileFocusColor = System.Drawing.Color.DarkRed;
            tileViewSec.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            tileViewSec.TileHighLightOpacity = 0D;
            tileViewSec.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewSec.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewSec.TileSize = new System.Drawing.Size(248, 15);
            tileViewSec.VirtualListSize = 0;
            tileViewSec.FocusSelectionChanged += OnFocusChangedLandSec;
            tileViewSec.DrawItem += OnDrawItemLandSec;
            tileViewSec.SizeChanged += OnTileViewSizeChanged;
            tileViewSec.MouseDoubleClick += OnDoubleClickSec;
            // 
            // contextMenuStripSec
            // 
            contextMenuStripSec.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyEntry2To1ToolStripMenuItem });
            contextMenuStripSec.Name = "contextMenuStripSec";
            contextMenuStripSec.Size = new System.Drawing.Size(165, 26);
            // 
            // copyEntry2To1ToolStripMenuItem
            // 
            copyEntry2To1ToolStripMenuItem.Name = "copyEntry2To1ToolStripMenuItem";
            copyEntry2To1ToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            copyEntry2To1ToolStripMenuItem.Text = "Copy Entry 2 to 1";
            copyEntry2To1ToolStripMenuItem.Click += OnClickCopySelected;
            // 
            // tabPageItem
            // 
            tabPageItem.Controls.Add(tableLayoutItem);
            tabPageItem.Location = new System.Drawing.Point(4, 24);
            tabPageItem.Name = "tabPageItem";
            tabPageItem.Size = new System.Drawing.Size(932, 421);
            tabPageItem.TabIndex = 1;
            tabPageItem.Text = "Static Tiles";
            tabPageItem.UseVisualStyleBackColor = true;
            // 
            // tableLayoutItem
            // 
            tableLayoutItem.ColumnCount = 3;
            tableLayoutItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27F));
            tableLayoutItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45.46F));
            tableLayoutItem.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.27F));
            tableLayoutItem.Controls.Add(tileViewItemOrg, 0, 0);
            tableLayoutItem.Controls.Add(tileViewItemSec, 2, 0);
            tableLayoutItem.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutItem.Location = new System.Drawing.Point(0, 0);
            tableLayoutItem.Name = "tableLayoutItem";
            tableLayoutItem.RowCount = 1;
            tableLayoutItem.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutItem.Size = new System.Drawing.Size(932, 421);
            tableLayoutItem.TabIndex = 0;
            // 
            // tileViewItemOrg
            // 
            tileViewItemOrg.ContextMenuStrip = contextMenuStripOrg;
            tileViewItemOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewItemOrg.FocusIndex = -1;
            tileViewItemOrg.Location = new System.Drawing.Point(3, 3);
            tileViewItemOrg.MultiSelect = false;
            tileViewItemOrg.Name = "tileViewItemOrg";
            tileViewItemOrg.Size = new System.Drawing.Size(248, 415);
            tileViewItemOrg.TabIndex = 0;
            tileViewItemOrg.TileBackgroundColor = System.Drawing.SystemColors.Window;
            tileViewItemOrg.TileBorderColor = System.Drawing.Color.FromArgb(0, 0, 0);
            tileViewItemOrg.TileBorderWidth = 0F;
            tileViewItemOrg.TileFocusColor = System.Drawing.Color.DarkRed;
            tileViewItemOrg.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            tileViewItemOrg.TileHighLightOpacity = 0D;
            tileViewItemOrg.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewItemOrg.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewItemOrg.TileSize = new System.Drawing.Size(248, 15);
            tileViewItemOrg.VirtualListSize = 0;
            tileViewItemOrg.FocusSelectionChanged += OnFocusChangedItemOrg;
            tileViewItemOrg.DrawItem += OnDrawItemItemOrg;
            tileViewItemOrg.SizeChanged += OnTileViewSizeChanged;
            tileViewItemOrg.MouseDoubleClick += OnDoubleClickOrg;
            // 
            // tileViewItemSec
            // 
            tileViewItemSec.ContextMenuStrip = contextMenuStripSec;
            tileViewItemSec.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewItemSec.FocusIndex = -1;
            tileViewItemSec.Location = new System.Drawing.Point(680, 3);
            tileViewItemSec.MultiSelect = false;
            tileViewItemSec.Name = "tileViewItemSec";
            tileViewItemSec.Size = new System.Drawing.Size(249, 415);
            tileViewItemSec.TabIndex = 2;
            tileViewItemSec.TileBackgroundColor = System.Drawing.SystemColors.Window;
            tileViewItemSec.TileBorderColor = System.Drawing.Color.FromArgb(0, 0, 0);
            tileViewItemSec.TileBorderWidth = 0F;
            tileViewItemSec.TileFocusColor = System.Drawing.Color.DarkRed;
            tileViewItemSec.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            tileViewItemSec.TileHighLightOpacity = 0D;
            tileViewItemSec.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewItemSec.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewItemSec.TileSize = new System.Drawing.Size(248, 15);
            tileViewItemSec.VirtualListSize = 0;
            tileViewItemSec.FocusSelectionChanged += OnFocusChangedItemSec;
            tileViewItemSec.DrawItem += OnDrawItemItemSec;
            tileViewItemSec.SizeChanged += OnTileViewSizeChanged;
            tileViewItemSec.MouseDoubleClick += OnDoubleClickSec;
            // 
            // buttonCopyAllDiff
            // 
            buttonCopyAllDiff.Location = new System.Drawing.Point(659, 11);
            buttonCopyAllDiff.Name = "buttonCopyAllDiff";
            buttonCopyAllDiff.Size = new System.Drawing.Size(90, 27);
            buttonCopyAllDiff.TabIndex = 5;
            buttonCopyAllDiff.Text = "Copy All Diff";
            buttonCopyAllDiff.UseVisualStyleBackColor = true;
            buttonCopyAllDiff.Click += OnClickCopyAllDiff;
            // 
            // buttonCopySelected
            // 
            buttonCopySelected.Location = new System.Drawing.Point(563, 11);
            buttonCopySelected.Name = "buttonCopySelected";
            buttonCopySelected.Size = new System.Drawing.Size(90, 27);
            buttonCopySelected.TabIndex = 4;
            buttonCopySelected.Text = "Copy Selected";
            buttonCopySelected.UseVisualStyleBackColor = true;
            buttonCopySelected.Click += OnClickCopySelected;
            // 
            // checkBoxShowDiff
            // 
            checkBoxShowDiff.AutoSize = true;
            checkBoxShowDiff.Location = new System.Drawing.Point(408, 15);
            checkBoxShowDiff.Name = "checkBoxShowDiff";
            checkBoxShowDiff.Size = new System.Drawing.Size(143, 19);
            checkBoxShowDiff.TabIndex = 3;
            checkBoxShowDiff.Text = "Show only Differences";
            checkBoxShowDiff.UseVisualStyleBackColor = true;
            checkBoxShowDiff.Click += OnChangeShowDiff;
            // 
            // buttonLoadSecond
            // 
            buttonLoadSecond.AutoSize = true;
            buttonLoadSecond.Location = new System.Drawing.Point(306, 11);
            buttonLoadSecond.Name = "buttonLoadSecond";
            buttonLoadSecond.Size = new System.Drawing.Size(90, 27);
            buttonLoadSecond.TabIndex = 2;
            buttonLoadSecond.Text = "Load Second";
            buttonLoadSecond.UseVisualStyleBackColor = true;
            buttonLoadSecond.Click += OnClickLoadSecond;
            // 
            // buttonBrowse
            // 
            buttonBrowse.AutoSize = true;
            buttonBrowse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttonBrowse.Location = new System.Drawing.Point(272, 13);
            buttonBrowse.Name = "buttonBrowse";
            buttonBrowse.Size = new System.Drawing.Size(26, 25);
            buttonBrowse.TabIndex = 1;
            buttonBrowse.Text = "...";
            buttonBrowse.UseVisualStyleBackColor = true;
            buttonBrowse.Click += OnClickBrowse;
            // 
            // textBoxSecondFile
            // 
            textBoxSecondFile.Location = new System.Drawing.Point(6, 15);
            textBoxSecondFile.Name = "textBoxSecondFile";
            textBoxSecondFile.Size = new System.Drawing.Size(260, 23);
            textBoxSecondFile.TabIndex = 0;
            // 
            // CompareRadarColControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Name = "CompareRadarColControl";
            Size = new System.Drawing.Size(940, 510);
            Load += OnLoad;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabControl.ResumeLayout(false);
            tabPageLand.ResumeLayout(false);
            tableLayoutLand.ResumeLayout(false);
            contextMenuStripOrg.ResumeLayout(false);
            panelDetail.ResumeLayout(false);
            groupBoxOrg.ResumeLayout(false);
            groupBoxOrg.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxOrgColor).EndInit();
            groupBoxSec.ResumeLayout(false);
            groupBoxSec.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSecColor).EndInit();
            groupBoxLegend.ResumeLayout(false);
            groupBoxLegend.PerformLayout();
            contextMenuStripSec.ResumeLayout(false);
            tabPageItem.ResumeLayout(false);
            tableLayoutItem.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageLand;
        private System.Windows.Forms.TableLayoutPanel tableLayoutLand;
        private System.Windows.Forms.TabPage tabPageItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutItem;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewOrg;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewSec;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewItemOrg;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewItemSec;
        private System.Windows.Forms.Panel panelDetail;
        private System.Windows.Forms.GroupBox groupBoxOrg;
        private System.Windows.Forms.Label labelOrgColorCaption;
        private System.Windows.Forms.Label labelOrgColorValue;
        private System.Windows.Forms.PictureBox pictureBoxOrgColor;
        private System.Windows.Forms.GroupBox groupBoxSec;
        private System.Windows.Forms.Label labelSecColorCaption;
        private System.Windows.Forms.Label labelSecColorValue;
        private System.Windows.Forms.PictureBox pictureBoxSecColor;
        private System.Windows.Forms.GroupBox groupBoxLegend;
        private System.Windows.Forms.Label legendSwatchDifferent;
        private System.Windows.Forms.Label legendLabelDifferent;
        private System.Windows.Forms.Label legendSwatchIdentical;
        private System.Windows.Forms.Label legendLabelIdentical;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripOrg;
        private System.Windows.Forms.ToolStripMenuItem copyEntry1To2ToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripSec;
        private System.Windows.Forms.ToolStripMenuItem copyEntry2To1ToolStripMenuItem;
        private System.Windows.Forms.TextBox textBoxSecondFile;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonLoadSecond;
        private System.Windows.Forms.CheckBox checkBoxShowDiff;
        private System.Windows.Forms.Button buttonCopySelected;
        private System.Windows.Forms.Button buttonCopyAllDiff;
    }
}
