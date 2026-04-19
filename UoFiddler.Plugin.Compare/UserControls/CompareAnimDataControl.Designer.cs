namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class CompareAnimDataControl
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
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            tileViewOrg = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            panelDetail = new System.Windows.Forms.Panel();
            groupBoxLegend = new System.Windows.Forms.GroupBox();
            legendSwatchOnlyInOrg = new System.Windows.Forms.Label();
            legendLabelOnlyInOrg = new System.Windows.Forms.Label();
            legendSwatchOnlyInSecond = new System.Windows.Forms.Label();
            legendLabelOnlyInSecond = new System.Windows.Forms.Label();
            legendSwatchDifferent = new System.Windows.Forms.Label();
            legendLabelDifferent = new System.Windows.Forms.Label();
            legendSwatchIdentical = new System.Windows.Forms.Label();
            legendLabelIdentical = new System.Windows.Forms.Label();
            groupBoxSec = new System.Windows.Forms.GroupBox();
            labelSecFrameCountCaption = new System.Windows.Forms.Label();
            labelSecFrameIntervalCaption = new System.Windows.Forms.Label();
            labelSecFrameStartCaption = new System.Windows.Forms.Label();
            labelSecFrameDataCaption = new System.Windows.Forms.Label();
            labelSecFrameCount = new System.Windows.Forms.Label();
            labelSecFrameInterval = new System.Windows.Forms.Label();
            labelSecFrameStart = new System.Windows.Forms.Label();
            labelSecFrameData = new System.Windows.Forms.Label();
            groupBoxOrg = new System.Windows.Forms.GroupBox();
            labelOrgFrameCountCaption = new System.Windows.Forms.Label();
            labelOrgFrameIntervalCaption = new System.Windows.Forms.Label();
            labelOrgFrameStartCaption = new System.Windows.Forms.Label();
            labelOrgFrameDataCaption = new System.Windows.Forms.Label();
            labelOrgFrameCount = new System.Windows.Forms.Label();
            labelOrgFrameInterval = new System.Windows.Forms.Label();
            labelOrgFrameStart = new System.Windows.Forms.Label();
            labelOrgFrameData = new System.Windows.Forms.Label();
            tileViewSec = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            copyEntryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            buttonCopyAddedOnly = new System.Windows.Forms.Button();
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
            tableLayoutPanel1.SuspendLayout();
            panelDetail.SuspendLayout();
            groupBoxLegend.SuspendLayout();
            groupBoxSec.SuspendLayout();
            groupBoxOrg.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(buttonCopyAddedOnly);
            splitContainer1.Panel2.Controls.Add(buttonCopyAllDiff);
            splitContainer1.Panel2.Controls.Add(buttonCopySelected);
            splitContainer1.Panel2.Controls.Add(checkBoxShowDiff);
            splitContainer1.Panel2.Controls.Add(buttonLoadSecond);
            splitContainer1.Panel2.Controls.Add(buttonBrowse);
            splitContainer1.Panel2.Controls.Add(textBoxSecondFile);
            splitContainer1.Size = new System.Drawing.Size(900, 510);
            splitContainer1.SplitterDistance = 454;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(tileViewOrg, 0, 0);
            tableLayoutPanel1.Controls.Add(panelDetail, 1, 0);
            tableLayoutPanel1.Controls.Add(tileViewSec, 2, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(900, 454);
            tableLayoutPanel1.TabIndex = 0;
            //
            // tileViewOrg
            //
            tileViewOrg.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewOrg.Location = new System.Drawing.Point(3, 3);
            tileViewOrg.Name = "tileViewOrg";
            tileViewOrg.Size = new System.Drawing.Size(219, 448);
            tileViewOrg.TabIndex = 0;
            tileViewOrg.TileSize = new System.Drawing.Size(219, 15);
            tileViewOrg.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewOrg.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewOrg.TileBorderWidth = 0f;
            tileViewOrg.TileHighLightOpacity = 0.0;
            tileViewOrg.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(OnDrawItemOrg);
            tileViewOrg.FocusSelectionChanged += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs>(OnFocusChangedOrg);
            tileViewOrg.SizeChanged += new System.EventHandler(OnTileViewSizeChanged);
            // 
            // panelDetail
            // 
            panelDetail.Controls.Add(groupBoxLegend);
            panelDetail.Controls.Add(groupBoxSec);
            panelDetail.Controls.Add(groupBoxOrg);
            panelDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            panelDetail.Location = new System.Drawing.Point(228, 3);
            panelDetail.Name = "panelDetail";
            panelDetail.Size = new System.Drawing.Size(444, 448);
            panelDetail.TabIndex = 1;
            // 
            // groupBoxLegend
            // 
            groupBoxLegend.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxLegend.Controls.Add(legendSwatchOnlyInOrg);
            groupBoxLegend.Controls.Add(legendLabelOnlyInOrg);
            groupBoxLegend.Controls.Add(legendSwatchOnlyInSecond);
            groupBoxLegend.Controls.Add(legendLabelOnlyInSecond);
            groupBoxLegend.Controls.Add(legendSwatchDifferent);
            groupBoxLegend.Controls.Add(legendLabelDifferent);
            groupBoxLegend.Controls.Add(legendSwatchIdentical);
            groupBoxLegend.Controls.Add(legendLabelIdentical);
            groupBoxLegend.Location = new System.Drawing.Point(6, 310);
            groupBoxLegend.Name = "groupBoxLegend";
            groupBoxLegend.Size = new System.Drawing.Size(432, 125);
            groupBoxLegend.TabIndex = 2;
            groupBoxLegend.TabStop = false;
            groupBoxLegend.Text = "Legend";
            // 
            // legendSwatchOnlyInOrg
            // 
            legendSwatchOnlyInOrg.BackColor = System.Drawing.Color.Orange;
            legendSwatchOnlyInOrg.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            legendSwatchOnlyInOrg.Location = new System.Drawing.Point(8, 22);
            legendSwatchOnlyInOrg.Name = "legendSwatchOnlyInOrg";
            legendSwatchOnlyInOrg.Size = new System.Drawing.Size(16, 16);
            legendSwatchOnlyInOrg.TabIndex = 0;
            // 
            // legendLabelOnlyInOrg
            // 
            legendLabelOnlyInOrg.AutoSize = true;
            legendLabelOnlyInOrg.Location = new System.Drawing.Point(30, 23);
            legendLabelOnlyInOrg.Name = "legendLabelOnlyInOrg";
            legendLabelOnlyInOrg.Size = new System.Drawing.Size(88, 15);
            legendLabelOnlyInOrg.TabIndex = 1;
            legendLabelOnlyInOrg.Text = "Only in original";
            // 
            // legendSwatchOnlyInSecond
            // 
            legendSwatchOnlyInSecond.BackColor = System.Drawing.Color.Green;
            legendSwatchOnlyInSecond.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            legendSwatchOnlyInSecond.Location = new System.Drawing.Point(8, 46);
            legendSwatchOnlyInSecond.Name = "legendSwatchOnlyInSecond";
            legendSwatchOnlyInSecond.Size = new System.Drawing.Size(16, 16);
            legendSwatchOnlyInSecond.TabIndex = 2;
            // 
            // legendLabelOnlyInSecond
            // 
            legendLabelOnlyInSecond.AutoSize = true;
            legendLabelOnlyInSecond.Location = new System.Drawing.Point(30, 47);
            legendLabelOnlyInSecond.Name = "legendLabelOnlyInSecond";
            legendLabelOnlyInSecond.Size = new System.Drawing.Size(86, 15);
            legendLabelOnlyInSecond.TabIndex = 3;
            legendLabelOnlyInSecond.Text = "Only in second";
            // 
            // legendSwatchDifferent
            // 
            legendSwatchDifferent.BackColor = System.Drawing.Color.Blue;
            legendSwatchDifferent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            legendSwatchDifferent.Location = new System.Drawing.Point(8, 70);
            legendSwatchDifferent.Name = "legendSwatchDifferent";
            legendSwatchDifferent.Size = new System.Drawing.Size(16, 16);
            legendSwatchDifferent.TabIndex = 4;
            // 
            // legendLabelDifferent
            // 
            legendLabelDifferent.AutoSize = true;
            legendLabelDifferent.Location = new System.Drawing.Point(30, 71);
            legendLabelDifferent.Name = "legendLabelDifferent";
            legendLabelDifferent.Size = new System.Drawing.Size(89, 15);
            legendLabelDifferent.TabIndex = 5;
            legendLabelDifferent.Text = "Different values";
            // 
            // legendSwatchIdentical
            // 
            legendSwatchIdentical.BackColor = System.Drawing.Color.Gray;
            legendSwatchIdentical.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            legendSwatchIdentical.Location = new System.Drawing.Point(8, 96);
            legendSwatchIdentical.Name = "legendSwatchIdentical";
            legendSwatchIdentical.Size = new System.Drawing.Size(16, 16);
            legendSwatchIdentical.TabIndex = 6;
            // 
            // legendLabelIdentical
            // 
            legendLabelIdentical.AutoSize = true;
            legendLabelIdentical.Location = new System.Drawing.Point(30, 97);
            legendLabelIdentical.Name = "legendLabelIdentical";
            legendLabelIdentical.Size = new System.Drawing.Size(52, 15);
            legendLabelIdentical.TabIndex = 7;
            legendLabelIdentical.Text = "Identical";
            // 
            // groupBoxSec
            // 
            groupBoxSec.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxSec.Controls.Add(labelSecFrameCountCaption);
            groupBoxSec.Controls.Add(labelSecFrameIntervalCaption);
            groupBoxSec.Controls.Add(labelSecFrameStartCaption);
            groupBoxSec.Controls.Add(labelSecFrameDataCaption);
            groupBoxSec.Controls.Add(labelSecFrameCount);
            groupBoxSec.Controls.Add(labelSecFrameInterval);
            groupBoxSec.Controls.Add(labelSecFrameStart);
            groupBoxSec.Controls.Add(labelSecFrameData);
            groupBoxSec.Location = new System.Drawing.Point(6, 158);
            groupBoxSec.Name = "groupBoxSec";
            groupBoxSec.Size = new System.Drawing.Size(432, 140);
            groupBoxSec.TabIndex = 1;
            groupBoxSec.TabStop = false;
            groupBoxSec.Text = "Right (Second)";
            // 
            // labelSecFrameCountCaption
            // 
            labelSecFrameCountCaption.AutoSize = true;
            labelSecFrameCountCaption.Location = new System.Drawing.Point(6, 22);
            labelSecFrameCountCaption.Name = "labelSecFrameCountCaption";
            labelSecFrameCountCaption.Size = new System.Drawing.Size(79, 15);
            labelSecFrameCountCaption.TabIndex = 0;
            labelSecFrameCountCaption.Text = "Frame Count:";
            // 
            // labelSecFrameIntervalCaption
            // 
            labelSecFrameIntervalCaption.AutoSize = true;
            labelSecFrameIntervalCaption.Location = new System.Drawing.Point(6, 44);
            labelSecFrameIntervalCaption.Name = "labelSecFrameIntervalCaption";
            labelSecFrameIntervalCaption.Size = new System.Drawing.Size(85, 15);
            labelSecFrameIntervalCaption.TabIndex = 2;
            labelSecFrameIntervalCaption.Text = "Frame Interval:";
            // 
            // labelSecFrameStartCaption
            // 
            labelSecFrameStartCaption.AutoSize = true;
            labelSecFrameStartCaption.Location = new System.Drawing.Point(6, 66);
            labelSecFrameStartCaption.Name = "labelSecFrameStartCaption";
            labelSecFrameStartCaption.Size = new System.Drawing.Size(70, 15);
            labelSecFrameStartCaption.TabIndex = 4;
            labelSecFrameStartCaption.Text = "Frame Start:";
            // 
            // labelSecFrameDataCaption
            // 
            labelSecFrameDataCaption.AutoSize = true;
            labelSecFrameDataCaption.Location = new System.Drawing.Point(6, 88);
            labelSecFrameDataCaption.Name = "labelSecFrameDataCaption";
            labelSecFrameDataCaption.Size = new System.Drawing.Size(70, 15);
            labelSecFrameDataCaption.TabIndex = 6;
            labelSecFrameDataCaption.Text = "Frame Data:";
            // 
            // labelSecFrameCount
            // 
            labelSecFrameCount.AutoSize = true;
            labelSecFrameCount.Location = new System.Drawing.Point(120, 22);
            labelSecFrameCount.Name = "labelSecFrameCount";
            labelSecFrameCount.Size = new System.Drawing.Size(12, 15);
            labelSecFrameCount.TabIndex = 1;
            labelSecFrameCount.Text = "-";
            // 
            // labelSecFrameInterval
            // 
            labelSecFrameInterval.AutoSize = true;
            labelSecFrameInterval.Location = new System.Drawing.Point(120, 44);
            labelSecFrameInterval.Name = "labelSecFrameInterval";
            labelSecFrameInterval.Size = new System.Drawing.Size(12, 15);
            labelSecFrameInterval.TabIndex = 3;
            labelSecFrameInterval.Text = "-";
            // 
            // labelSecFrameStart
            // 
            labelSecFrameStart.AutoSize = true;
            labelSecFrameStart.Location = new System.Drawing.Point(120, 66);
            labelSecFrameStart.Name = "labelSecFrameStart";
            labelSecFrameStart.Size = new System.Drawing.Size(12, 15);
            labelSecFrameStart.TabIndex = 5;
            labelSecFrameStart.Text = "-";
            // 
            // labelSecFrameData
            // 
            labelSecFrameData.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelSecFrameData.Location = new System.Drawing.Point(120, 88);
            labelSecFrameData.Name = "labelSecFrameData";
            labelSecFrameData.Size = new System.Drawing.Size(300, 44);
            labelSecFrameData.TabIndex = 7;
            labelSecFrameData.Text = "-";
            // 
            // groupBoxOrg
            // 
            groupBoxOrg.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxOrg.Controls.Add(labelOrgFrameCountCaption);
            groupBoxOrg.Controls.Add(labelOrgFrameIntervalCaption);
            groupBoxOrg.Controls.Add(labelOrgFrameStartCaption);
            groupBoxOrg.Controls.Add(labelOrgFrameDataCaption);
            groupBoxOrg.Controls.Add(labelOrgFrameCount);
            groupBoxOrg.Controls.Add(labelOrgFrameInterval);
            groupBoxOrg.Controls.Add(labelOrgFrameStart);
            groupBoxOrg.Controls.Add(labelOrgFrameData);
            groupBoxOrg.Location = new System.Drawing.Point(6, 6);
            groupBoxOrg.Name = "groupBoxOrg";
            groupBoxOrg.Size = new System.Drawing.Size(432, 140);
            groupBoxOrg.TabIndex = 0;
            groupBoxOrg.TabStop = false;
            groupBoxOrg.Text = "Left (Original)";
            // 
            // labelOrgFrameCountCaption
            // 
            labelOrgFrameCountCaption.AutoSize = true;
            labelOrgFrameCountCaption.Location = new System.Drawing.Point(6, 22);
            labelOrgFrameCountCaption.Name = "labelOrgFrameCountCaption";
            labelOrgFrameCountCaption.Size = new System.Drawing.Size(79, 15);
            labelOrgFrameCountCaption.TabIndex = 0;
            labelOrgFrameCountCaption.Text = "Frame Count:";
            // 
            // labelOrgFrameIntervalCaption
            // 
            labelOrgFrameIntervalCaption.AutoSize = true;
            labelOrgFrameIntervalCaption.Location = new System.Drawing.Point(6, 44);
            labelOrgFrameIntervalCaption.Name = "labelOrgFrameIntervalCaption";
            labelOrgFrameIntervalCaption.Size = new System.Drawing.Size(85, 15);
            labelOrgFrameIntervalCaption.TabIndex = 2;
            labelOrgFrameIntervalCaption.Text = "Frame Interval:";
            // 
            // labelOrgFrameStartCaption
            // 
            labelOrgFrameStartCaption.AutoSize = true;
            labelOrgFrameStartCaption.Location = new System.Drawing.Point(6, 66);
            labelOrgFrameStartCaption.Name = "labelOrgFrameStartCaption";
            labelOrgFrameStartCaption.Size = new System.Drawing.Size(70, 15);
            labelOrgFrameStartCaption.TabIndex = 4;
            labelOrgFrameStartCaption.Text = "Frame Start:";
            // 
            // labelOrgFrameDataCaption
            // 
            labelOrgFrameDataCaption.AutoSize = true;
            labelOrgFrameDataCaption.Location = new System.Drawing.Point(6, 88);
            labelOrgFrameDataCaption.Name = "labelOrgFrameDataCaption";
            labelOrgFrameDataCaption.Size = new System.Drawing.Size(70, 15);
            labelOrgFrameDataCaption.TabIndex = 6;
            labelOrgFrameDataCaption.Text = "Frame Data:";
            // 
            // labelOrgFrameCount
            // 
            labelOrgFrameCount.AutoSize = true;
            labelOrgFrameCount.Location = new System.Drawing.Point(120, 22);
            labelOrgFrameCount.Name = "labelOrgFrameCount";
            labelOrgFrameCount.Size = new System.Drawing.Size(12, 15);
            labelOrgFrameCount.TabIndex = 1;
            labelOrgFrameCount.Text = "-";
            // 
            // labelOrgFrameInterval
            // 
            labelOrgFrameInterval.AutoSize = true;
            labelOrgFrameInterval.Location = new System.Drawing.Point(120, 44);
            labelOrgFrameInterval.Name = "labelOrgFrameInterval";
            labelOrgFrameInterval.Size = new System.Drawing.Size(12, 15);
            labelOrgFrameInterval.TabIndex = 3;
            labelOrgFrameInterval.Text = "-";
            // 
            // labelOrgFrameStart
            // 
            labelOrgFrameStart.AutoSize = true;
            labelOrgFrameStart.Location = new System.Drawing.Point(120, 66);
            labelOrgFrameStart.Name = "labelOrgFrameStart";
            labelOrgFrameStart.Size = new System.Drawing.Size(12, 15);
            labelOrgFrameStart.TabIndex = 5;
            labelOrgFrameStart.Text = "-";
            // 
            // labelOrgFrameData
            // 
            labelOrgFrameData.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            labelOrgFrameData.Location = new System.Drawing.Point(120, 88);
            labelOrgFrameData.Name = "labelOrgFrameData";
            labelOrgFrameData.Size = new System.Drawing.Size(300, 44);
            labelOrgFrameData.TabIndex = 7;
            labelOrgFrameData.Text = "-";
            //
            // tileViewSec
            //
            tileViewSec.ContextMenuStrip = contextMenuStrip1;
            tileViewSec.Dock = System.Windows.Forms.DockStyle.Fill;
            tileViewSec.Location = new System.Drawing.Point(678, 3);
            tileViewSec.Name = "tileViewSec";
            tileViewSec.Size = new System.Drawing.Size(219, 448);
            tileViewSec.TabIndex = 2;
            tileViewSec.TileSize = new System.Drawing.Size(219, 15);
            tileViewSec.TileMargin = new System.Windows.Forms.Padding(0);
            tileViewSec.TilePadding = new System.Windows.Forms.Padding(0);
            tileViewSec.TileBorderWidth = 0f;
            tileViewSec.TileHighLightOpacity = 0.0;
            tileViewSec.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(OnDrawItemSec);
            tileViewSec.FocusSelectionChanged += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs>(OnFocusChangedSec);
            tileViewSec.SizeChanged += new System.EventHandler(OnTileViewSizeChanged);
            tileViewSec.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(OnDoubleClickSec);
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyEntryToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(165, 26);
            // 
            // copyEntryToolStripMenuItem
            // 
            copyEntryToolStripMenuItem.Name = "copyEntryToolStripMenuItem";
            copyEntryToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            copyEntryToolStripMenuItem.Text = "Copy Entry 2 to 1";
            copyEntryToolStripMenuItem.Click += OnClickCopySelected;
            // 
            // buttonCopyAddedOnly
            // 
            buttonCopyAddedOnly.Location = new System.Drawing.Point(755, 11);
            buttonCopyAddedOnly.Name = "buttonCopyAddedOnly";
            buttonCopyAddedOnly.Size = new System.Drawing.Size(100, 27);
            buttonCopyAddedOnly.TabIndex = 6;
            buttonCopyAddedOnly.Text = "Copy Added Only";
            buttonCopyAddedOnly.UseVisualStyleBackColor = true;
            buttonCopyAddedOnly.Click += OnClickCopyAddedOnly;
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
            // CompareAnimDataControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Name = "CompareAnimDataControl";
            Size = new System.Drawing.Size(900, 510);
            Load += OnLoad;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            panelDetail.ResumeLayout(false);
            groupBoxLegend.ResumeLayout(false);
            groupBoxLegend.PerformLayout();
            groupBoxSec.ResumeLayout(false);
            groupBoxSec.PerformLayout();
            groupBoxOrg.ResumeLayout(false);
            groupBoxOrg.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewOrg;
        private System.Windows.Forms.Panel panelDetail;
        private System.Windows.Forms.GroupBox groupBoxOrg;
        private System.Windows.Forms.Label labelOrgFrameCountCaption;
        private System.Windows.Forms.Label labelOrgFrameCount;
        private System.Windows.Forms.Label labelOrgFrameIntervalCaption;
        private System.Windows.Forms.Label labelOrgFrameInterval;
        private System.Windows.Forms.Label labelOrgFrameStartCaption;
        private System.Windows.Forms.Label labelOrgFrameStart;
        private System.Windows.Forms.Label labelOrgFrameDataCaption;
        private System.Windows.Forms.Label labelOrgFrameData;
        private System.Windows.Forms.GroupBox groupBoxSec;
        private System.Windows.Forms.Label labelSecFrameCountCaption;
        private System.Windows.Forms.Label labelSecFrameCount;
        private System.Windows.Forms.Label labelSecFrameIntervalCaption;
        private System.Windows.Forms.Label labelSecFrameInterval;
        private System.Windows.Forms.Label labelSecFrameStartCaption;
        private System.Windows.Forms.Label labelSecFrameStart;
        private System.Windows.Forms.Label labelSecFrameDataCaption;
        private System.Windows.Forms.Label labelSecFrameData;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileViewSec;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyEntryToolStripMenuItem;
        private System.Windows.Forms.TextBox textBoxSecondFile;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonLoadSecond;
        private System.Windows.Forms.CheckBox checkBoxShowDiff;
        private System.Windows.Forms.Button buttonCopySelected;
        private System.Windows.Forms.Button buttonCopyAllDiff;
        private System.Windows.Forms.Button buttonCopyAddedOnly;
        private System.Windows.Forms.GroupBox groupBoxLegend;
        private System.Windows.Forms.Label legendSwatchOnlyInOrg;
        private System.Windows.Forms.Label legendLabelOnlyInOrg;
        private System.Windows.Forms.Label legendSwatchOnlyInSecond;
        private System.Windows.Forms.Label legendLabelOnlyInSecond;
        private System.Windows.Forms.Label legendSwatchDifferent;
        private System.Windows.Forms.Label legendLabelDifferent;
        private System.Windows.Forms.Label legendSwatchIdentical;
        private System.Windows.Forms.Label legendLabelIdentical;
    }
}
