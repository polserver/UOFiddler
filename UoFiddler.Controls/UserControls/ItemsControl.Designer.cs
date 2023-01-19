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

using System.Windows.Forms;

namespace UoFiddler.Controls.UserControls
{
    partial class ItemsControl
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.DetailPictureBox = new System.Windows.Forms.PictureBox();
            this.DetailPictureBoxContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.changeBackgroundColorToolStripMenuItemDetail = new System.Windows.Forms.ToolStripMenuItem();
            this.DetailTextBox = new System.Windows.Forms.RichTextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ItemsTileView = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            this.TileViewContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ChangeBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.selectInTileDataTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInRadarColorTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInGumpsTabMaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInGumpsTabFemaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceStartingFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaceStartingFromText = new System.Windows.Forms.ToolStripTextBox();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.NameLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.GraphicLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.PreLoader = new System.ComponentModel.BackgroundWorker();
            this.ToolStrip = new System.Windows.Forms.ToolStrip();
            this.SearchToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.ProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.PreloadItemsToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.MiscToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.ExportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.collapsibleSplitter1 = new UoFiddler.Controls.UserControls.CollapsibleSplitter();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DetailPictureBox)).BeginInit();
            this.DetailPictureBoxContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.TileViewContextMenuStrip.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.ToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.DetailPictureBox);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.DetailTextBox);
            this.splitContainer2.Size = new System.Drawing.Size(164, 286);
            this.splitContainer2.SplitterDistance = 164;
            this.splitContainer2.TabIndex = 0;
            // 
            // DetailPictureBox
            // 
            this.DetailPictureBox.ContextMenuStrip = this.DetailPictureBoxContextMenuStrip;
            this.DetailPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailPictureBox.Location = new System.Drawing.Point(0, 0);
            this.DetailPictureBox.Name = "DetailPictureBox";
            this.DetailPictureBox.Size = new System.Drawing.Size(164, 164);
            this.DetailPictureBox.TabIndex = 0;
            this.DetailPictureBox.TabStop = false;
            // 
            // DetailPictureBoxContextMenuStrip
            // 
            this.DetailPictureBoxContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeBackgroundColorToolStripMenuItemDetail});
            this.DetailPictureBoxContextMenuStrip.Name = "contextMenuStrip2";
            this.DetailPictureBoxContextMenuStrip.Size = new System.Drawing.Size(213, 26);
            // 
            // changeBackgroundColorToolStripMenuItemDetail
            // 
            this.changeBackgroundColorToolStripMenuItemDetail.Name = "changeBackgroundColorToolStripMenuItemDetail";
            this.changeBackgroundColorToolStripMenuItemDetail.Size = new System.Drawing.Size(212, 22);
            this.changeBackgroundColorToolStripMenuItemDetail.Text = "Change background color";
            this.changeBackgroundColorToolStripMenuItemDetail.Click += new System.EventHandler(this.ChangeBackgroundColorToolStripMenuItemDetail_Click);
            // 
            // DetailTextBox
            // 
            this.DetailTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DetailTextBox.Location = new System.Drawing.Point(0, 0);
            this.DetailTextBox.Name = "DetailTextBox";
            this.DetailTextBox.Size = new System.Drawing.Size(164, 118);
            this.DetailTextBox.TabIndex = 0;
            this.DetailTextBox.Text = "";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 33);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ItemsTileView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(643, 286);
            this.splitContainer1.SplitterDistance = 475;
            this.splitContainer1.TabIndex = 6;
            // 
            // ItemsTileView
            // 
            this.ItemsTileView.AutoScroll = true;
            this.ItemsTileView.AutoScrollMinSize = new System.Drawing.Size(0, 102);
            this.ItemsTileView.BackColor = System.Drawing.SystemColors.Window;
            this.ItemsTileView.ContextMenuStrip = this.TileViewContextMenuStrip;
            this.ItemsTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemsTileView.FocusIndex = -1;
            this.ItemsTileView.Location = new System.Drawing.Point(0, 0);
            this.ItemsTileView.MultiSelect = true;
            this.ItemsTileView.Name = "ItemsTileView";
            this.ItemsTileView.Size = new System.Drawing.Size(475, 286);
            this.ItemsTileView.TabIndex = 0;
            this.ItemsTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            this.ItemsTileView.TileBorderColor = System.Drawing.Color.Gray;
            this.ItemsTileView.TileBorderWidth = 1F;
            this.ItemsTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            this.ItemsTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            this.ItemsTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.ItemsTileView.TilePadding = new System.Windows.Forms.Padding(1);
            this.ItemsTileView.TileSize = new System.Drawing.Size(96, 96);
            this.ItemsTileView.VirtualListSize = 1;
            this.ItemsTileView.ItemSelectionChanged += new System.EventHandler<System.Windows.Forms.ListViewItemSelectionChangedEventArgs>(this.ItemsTileView_ItemSelectionChanged);
            this.ItemsTileView.FocusSelectionChanged += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs>(this.ItemsTileView_FocusSelectionChanged);
            this.ItemsTileView.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(this.ItemsTileView_DrawItem);
            this.ItemsTileView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ItemsTileView_KeyDown);
            this.ItemsTileView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ItemsTileView_KeyUp);
            this.ItemsTileView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ItemsTileView_MouseDoubleClick);
            // 
            // TileViewContextMenuStrip
            // 
            this.TileViewContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFreeSlotsToolStripMenuItem,
            this.findNextFreeSlotToolStripMenuItem,
            this.ChangeBackgroundColorToolStripMenuItem,
            this.toolStripSeparator3,
            this.selectInTileDataTabToolStripMenuItem,
            this.selectInRadarColorTabToolStripMenuItem,
            this.selectInGumpsTabMaleToolStripMenuItem,
            this.selectInGumpsTabFemaleToolStripMenuItem,
            this.toolStripSeparator2,
            this.extractToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.replaceStartingFromToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.insertAtToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveToolStripMenuItem});
            this.TileViewContextMenuStrip.Name = "contextMenuStrip1";
            this.TileViewContextMenuStrip.Size = new System.Drawing.Size(213, 308);
            this.TileViewContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.TileViewContextMenuStrip_Opening);
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            this.showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            this.showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            this.showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.showFreeSlotsToolStripMenuItem.Text = "Show Free Slots";
            this.showFreeSlotsToolStripMenuItem.Click += new System.EventHandler(this.OnClickShowFreeSlots);
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            this.findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            this.findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            this.findNextFreeSlotToolStripMenuItem.Click += new System.EventHandler(this.OnClickFindFree);
            // 
            // ChangeBackgroundColorToolStripMenuItem
            // 
            this.ChangeBackgroundColorToolStripMenuItem.Name = "ChangeBackgroundColorToolStripMenuItem";
            this.ChangeBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.ChangeBackgroundColorToolStripMenuItem.Text = "Change background color";
            this.ChangeBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.ChangeBackgroundColorToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(209, 6);
            // 
            // selectInTileDataTabToolStripMenuItem
            // 
            this.selectInTileDataTabToolStripMenuItem.Name = "selectInTileDataTabToolStripMenuItem";
            this.selectInTileDataTabToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.selectInTileDataTabToolStripMenuItem.Text = "Select in TileData tab";
            this.selectInTileDataTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectTiledata);
            // 
            // selectInRadarColorTabToolStripMenuItem
            // 
            this.selectInRadarColorTabToolStripMenuItem.Name = "selectInRadarColorTabToolStripMenuItem";
            this.selectInRadarColorTabToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.selectInRadarColorTabToolStripMenuItem.Text = "Select in RadarColor tab";
            this.selectInRadarColorTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectRadarCol);
            // 
            // selectInGumpsTabMaleToolStripMenuItem
            // 
            this.selectInGumpsTabMaleToolStripMenuItem.Enabled = false;
            this.selectInGumpsTabMaleToolStripMenuItem.Name = "selectInGumpsTabMaleToolStripMenuItem";
            this.selectInGumpsTabMaleToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.selectInGumpsTabMaleToolStripMenuItem.Text = "Select in Gumps (M)";
            this.selectInGumpsTabMaleToolStripMenuItem.Click += new System.EventHandler(this.SelectInGumpsTabMaleToolStripMenuItem_Click);
            // 
            // selectInGumpsTabFemaleToolStripMenuItem
            // 
            this.selectInGumpsTabFemaleToolStripMenuItem.Enabled = false;
            this.selectInGumpsTabFemaleToolStripMenuItem.Name = "selectInGumpsTabFemaleToolStripMenuItem";
            this.selectInGumpsTabFemaleToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.selectInGumpsTabFemaleToolStripMenuItem.Text = "Select in Gumps (F)";
            this.selectInGumpsTabFemaleToolStripMenuItem.Click += new System.EventHandler(this.SelectInGumpsTabFemaleToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(209, 6);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bmpToolStripMenuItem,
            this.tiffToolStripMenuItem,
            this.asJpgToolStripMenuItem1,
            this.asPngToolStripMenuItem1});
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.extractToolStripMenuItem.Text = "Export Image..";
            // 
            // bmpToolStripMenuItem
            // 
            this.bmpToolStripMenuItem.Name = "bmpToolStripMenuItem";
            this.bmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.bmpToolStripMenuItem.Text = "As Bmp";
            this.bmpToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickBmp);
            // 
            // tiffToolStripMenuItem
            // 
            this.tiffToolStripMenuItem.Name = "tiffToolStripMenuItem";
            this.tiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.tiffToolStripMenuItem.Text = "As Tiff";
            this.tiffToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickTiff);
            // 
            // asJpgToolStripMenuItem1
            // 
            this.asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            this.asJpgToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem1.Text = "As Jpg";
            this.asJpgToolStripMenuItem1.Click += new System.EventHandler(this.Extract_Image_ClickJpg);
            // 
            // asPngToolStripMenuItem1
            // 
            this.asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            this.asPngToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem1.Text = "As Png";
            this.asPngToolStripMenuItem1.Click += new System.EventHandler(this.Extract_Image_ClickPng);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.replaceToolStripMenuItem.Text = "Replace...";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplace);
            // 
            // replaceStartingFromToolStripMenuItem
            // 
            this.replaceStartingFromToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ReplaceStartingFromText});
            this.replaceStartingFromToolStripMenuItem.Name = "replaceStartingFromToolStripMenuItem";
            this.replaceStartingFromToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.replaceStartingFromToolStripMenuItem.Text = "Replace starting from..";
            // 
            // ReplaceStartingFromText
            // 
            this.ReplaceStartingFromText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ReplaceStartingFromText.Name = "ReplaceStartingFromText";
            this.ReplaceStartingFromText.Size = new System.Drawing.Size(100, 23);
            this.ReplaceStartingFromText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ReplaceStartingFromText_KeyDown);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
            // 
            // insertAtToolStripMenuItem
            // 
            this.insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertText});
            this.insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            this.insertAtToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.insertAtToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            this.InsertText.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.InsertText.Name = "InsertText";
            this.InsertText.Size = new System.Drawing.Size(100, 23);
            this.InsertText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownInsertText);
            this.InsertText.TextChanged += new System.EventHandler(this.OnTextChangedInsert);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(209, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.OnClickSave);
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NameLabel,
            this.GraphicLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 319);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(643, 22);
            this.StatusStrip.TabIndex = 5;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = false;
            this.NameLabel.BorderStyle = System.Windows.Forms.Border3DStyle.SunkenInner;
            this.NameLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(200, 17);
            this.NameLabel.Text = "Name:";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GraphicLabel
            // 
            this.GraphicLabel.AutoSize = false;
            this.GraphicLabel.Name = "GraphicLabel";
            this.GraphicLabel.Size = new System.Drawing.Size(150, 17);
            this.GraphicLabel.Text = "Graphic:";
            this.GraphicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PreLoader
            // 
            this.PreLoader.WorkerReportsProgress = true;
            this.PreLoader.DoWork += new System.ComponentModel.DoWorkEventHandler(this.PreLoaderDoWork);
            this.PreLoader.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.PreLoaderProgressChanged);
            this.PreLoader.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.PreLoaderCompleted);
            // 
            // ToolStrip
            // 
            this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SearchToolStripButton,
            this.ProgressBar,
            this.PreloadItemsToolStripButton,
            this.MiscToolStripDropDownButton});
            this.ToolStrip.Location = new System.Drawing.Point(0, 0);
            this.ToolStrip.Name = "ToolStrip";
            this.ToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.ToolStrip.Size = new System.Drawing.Size(643, 25);
            this.ToolStrip.TabIndex = 7;
            // 
            // SearchToolStripButton
            // 
            this.SearchToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SearchToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchToolStripButton.Name = "SearchToolStripButton";
            this.SearchToolStripButton.Size = new System.Drawing.Size(46, 22);
            this.SearchToolStripButton.Text = "Search";
            this.SearchToolStripButton.Click += new System.EventHandler(this.OnSearchClick);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(100, 22);
            // 
            // PreloadItemsToolStripButton
            // 
            this.PreloadItemsToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.PreloadItemsToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.PreloadItemsToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PreloadItemsToolStripButton.Name = "PreloadItemsToolStripButton";
            this.PreloadItemsToolStripButton.Size = new System.Drawing.Size(83, 22);
            this.PreloadItemsToolStripButton.Text = "Preload Items";
            this.PreloadItemsToolStripButton.Click += new System.EventHandler(this.OnClickPreLoad);
            // 
            // MiscToolStripDropDownButton
            // 
            this.MiscToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.MiscToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportAllToolStripMenuItem});
            this.MiscToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MiscToolStripDropDownButton.Name = "MiscToolStripDropDownButton";
            this.MiscToolStripDropDownButton.Size = new System.Drawing.Size(45, 22);
            this.MiscToolStripDropDownButton.Text = "Misc";
            // 
            // ExportAllToolStripMenuItem
            // 
            this.ExportAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem,
            this.asPngToolStripMenuItem});
            this.ExportAllToolStripMenuItem.Name = "ExportAllToolStripMenuItem";
            this.ExportAllToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.ExportAllToolStripMenuItem.Text = "Export all..";
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllJpg);
            // 
            // asPngToolStripMenuItem
            // 
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem.Text = "As Png";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllPng);
            // 
            // collapsibleSplitter1
            // 
            this.collapsibleSplitter1.AnimationDelay = 20;
            this.collapsibleSplitter1.AnimationStep = 20;
            this.collapsibleSplitter1.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
            this.collapsibleSplitter1.ControlToHide = this.ToolStrip;
            this.collapsibleSplitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.collapsibleSplitter1.ExpandParentForm = false;
            this.collapsibleSplitter1.Location = new System.Drawing.Point(0, 25);
            this.collapsibleSplitter1.Name = "collapsibleSplitter1";
            this.collapsibleSplitter1.TabIndex = 8;
            this.collapsibleSplitter1.TabStop = false;
            this.collapsibleSplitter1.UseAnimations = false;
            this.collapsibleSplitter1.VisualStyle = UoFiddler.Controls.UserControls.VisualStyles.DoubleDots;
            // 
            // ItemsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.collapsibleSplitter1);
            this.Controls.Add(this.ToolStrip);
            this.DoubleBuffered = true;
            this.Name = "ItemsControl";
            this.Size = new System.Drawing.Size(643, 341);
            this.Load += new System.EventHandler(this.OnLoad);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.DetailPictureBox)).EndInit();
            this.DetailPictureBoxContextMenuStrip.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.TileViewContextMenuStrip.ResumeLayout(false);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ToolStrip.ResumeLayout(false);
            this.ToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bmpToolStripMenuItem;
        private UoFiddler.Controls.UserControls.CollapsibleSplitter collapsibleSplitter1;
        private System.Windows.Forms.ContextMenuStrip TileViewContextMenuStrip;
        private System.Windows.Forms.PictureBox DetailPictureBox;
        private System.Windows.Forms.ToolStripMenuItem ExportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel GraphicLabel;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripStatusLabel NameLabel;
        private System.ComponentModel.BackgroundWorker PreLoader;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInRadarColorTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInTileDataTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showFreeSlotsToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripMenuItem tiffToolStripMenuItem;
        private System.Windows.Forms.ToolStrip ToolStrip;
        private System.Windows.Forms.ToolStripButton SearchToolStripButton;
        private System.Windows.Forms.ToolStripButton PreloadItemsToolStripButton;
        private System.Windows.Forms.ToolStripDropDownButton MiscToolStripDropDownButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ContextMenuStrip DetailPictureBoxContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem changeBackgroundColorToolStripMenuItemDetail;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.ToolStripMenuItem ChangeBackgroundColorToolStripMenuItem;
        private TileView.TileViewControl ItemsTileView;
        private RichTextBox DetailTextBox;
        private ToolStripMenuItem selectInGumpsTabMaleToolStripMenuItem;
        private ToolStripMenuItem selectInGumpsTabFemaleToolStripMenuItem;
        private ToolStripMenuItem replaceStartingFromToolStripMenuItem;
        private ToolStripTextBox ReplaceStartingFromText;
    }
}
