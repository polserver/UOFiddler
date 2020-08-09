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

namespace UoFiddler.Controls.UserControls
{
    partial class SoundsControl
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
      this.treeView = new System.Windows.Forms.TreeView();
      this.cmStripSounds = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.nameSortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.ToggleRightPanelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.tsSeparator1 = new System.Windows.Forms.ToolStripSeparator();
      this.showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.nextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.tsSeparator2 = new System.Windows.Forms.ToolStripSeparator();
      this.playSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.extractSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.removeSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.itemSave = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStrip1 = new System.Windows.Forms.ToolStrip();
      this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
      this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
      this.itemExtractSoundlist = new System.Windows.Forms.ToolStripMenuItem();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.statusStripSounds = new System.Windows.Forms.StatusStrip();
      this.seconds = new System.Windows.Forms.ToolStripStatusLabel();
      this.toolStripStatusSpacer = new System.Windows.Forms.ToolStripStatusLabel();
      this.playing = new System.Windows.Forms.ToolStripProgressBar();
      this.stopButton = new System.Windows.Forms.ToolStripDropDownButton();
      this.splitContainer1 = new System.Windows.Forms.SplitContainer();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.groupBox1 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
      this.ExtractSoundlistButton = new System.Windows.Forms.Button();
      this.SaveFileButton = new System.Windows.Forms.Button();
      this.SortByNameCheckbox = new System.Windows.Forms.CheckBox();
      this.SelectedSoundGroup = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
      this.SoundPlaytimeBar = new System.Windows.Forms.ProgressBar();
      this.PlaySoundButton = new System.Windows.Forms.Button();
      this.StopSoundButton = new System.Windows.Forms.Button();
      this.ExtractSoundButton = new System.Windows.Forms.Button();
      this.RemoveSoundButton = new System.Windows.Forms.Button();
      this.groupBox2 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
      this.SearchNameTextbox = new System.Windows.Forms.TextBox();
      this.SearchByNameButton = new System.Windows.Forms.Button();
      this.GoNextResultButton = new System.Windows.Forms.Button();
      this.SearchByIdButton = new System.Windows.Forms.Button();
      this.groupBox3 = new System.Windows.Forms.GroupBox();
      this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
      this.label1 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.IdInsertTextbox = new System.Windows.Forms.TextBox();
      this.WavFileInsertTextbox = new System.Windows.Forms.TextBox();
      this.WavChooseInsertButton = new System.Windows.Forms.Button();
      this.AddInsertReplaceButton = new System.Windows.Forms.Button();
      this.GoPrevResultButton = new System.Windows.Forms.Button();
      this.cmStripSounds.SuspendLayout();
      this.toolStrip1.SuspendLayout();
      this.tableLayoutPanel1.SuspendLayout();
      this.statusStripSounds.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
      this.splitContainer1.Panel1.SuspendLayout();
      this.splitContainer1.Panel2.SuspendLayout();
      this.splitContainer1.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.tableLayoutPanel3.SuspendLayout();
      this.SelectedSoundGroup.SuspendLayout();
      this.tableLayoutPanel4.SuspendLayout();
      this.groupBox2.SuspendLayout();
      this.tableLayoutPanel5.SuspendLayout();
      this.groupBox3.SuspendLayout();
      this.tableLayoutPanel6.SuspendLayout();
      this.SuspendLayout();
      // 
      // treeView
      // 
      this.treeView.ContextMenuStrip = this.cmStripSounds;
      this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.treeView.HideSelection = false;
      this.treeView.Location = new System.Drawing.Point(0, 0);
      this.treeView.Name = "treeView";
      this.treeView.Size = new System.Drawing.Size(347, 555);
      this.treeView.TabIndex = 0;
      this.treeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.BeforeSelect);
      this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AfterSelect);
      this.treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnDoubleClick);
      this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeView_KeyDown);
      // 
      // cmStripSounds
      // 
      this.cmStripSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nameSortToolStripMenuItem,
            this.ToggleRightPanelToolStripMenuItem,
            this.tsSeparator1,
            this.showFreeSlotsToolStripMenuItem,
            this.nextFreeSlotToolStripMenuItem,
            this.tsSeparator2,
            this.playSoundToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.extractSoundToolStripMenuItem,
            this.removeSoundToolStripMenuItem,
            this.toolStripMenuItem1,
            this.itemSave});
      this.cmStripSounds.Name = "contextMenuStrip1";
      this.cmStripSounds.Size = new System.Drawing.Size(173, 220);
      // 
      // nameSortToolStripMenuItem
      // 
      this.nameSortToolStripMenuItem.CheckOnClick = true;
      this.nameSortToolStripMenuItem.Name = "nameSortToolStripMenuItem";
      this.nameSortToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.nameSortToolStripMenuItem.Text = "Name Sort";
      this.nameSortToolStripMenuItem.Click += new System.EventHandler(this.OnChangeSort);
      // 
      // ToggleRightPanelToolStripMenuItem
      // 
      this.ToggleRightPanelToolStripMenuItem.CheckOnClick = true;
      this.ToggleRightPanelToolStripMenuItem.Name = "ToggleRightPanelToolStripMenuItem";
      this.ToggleRightPanelToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.ToggleRightPanelToolStripMenuItem.Text = "Toggle Right Panel";
      this.ToggleRightPanelToolStripMenuItem.CheckedChanged += new System.EventHandler(this.ToggleRightPanelToolStripMenuItem_CheckedChanged);
      // 
      // tsSeparator1
      // 
      this.tsSeparator1.Name = "tsSeparator1";
      this.tsSeparator1.Size = new System.Drawing.Size(169, 6);
      // 
      // showFreeSlotsToolStripMenuItem
      // 
      this.showFreeSlotsToolStripMenuItem.CheckOnClick = true;
      this.showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
      this.showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.showFreeSlotsToolStripMenuItem.Text = "Show free slots";
      this.showFreeSlotsToolStripMenuItem.Click += new System.EventHandler(this.ShowFreeSlotsClick);
      // 
      // nextFreeSlotToolStripMenuItem
      // 
      this.nextFreeSlotToolStripMenuItem.Enabled = false;
      this.nextFreeSlotToolStripMenuItem.Name = "nextFreeSlotToolStripMenuItem";
      this.nextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.nextFreeSlotToolStripMenuItem.Text = "Find next free slot";
      this.nextFreeSlotToolStripMenuItem.Click += new System.EventHandler(this.NextFreeSlotToolStripMenuItem_Click);
      // 
      // tsSeparator2
      // 
      this.tsSeparator2.Name = "tsSeparator2";
      this.tsSeparator2.Size = new System.Drawing.Size(169, 6);
      // 
      // playSoundToolStripMenuItem
      // 
      this.playSoundToolStripMenuItem.Enabled = false;
      this.playSoundToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.playSoundToolStripMenuItem.Name = "playSoundToolStripMenuItem";
      this.playSoundToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.playSoundToolStripMenuItem.Text = "Play";
      this.playSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickPlay);
      // 
      // replaceToolStripMenuItem
      // 
      this.replaceToolStripMenuItem.Enabled = false;
      this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
      this.replaceToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.replaceToolStripMenuItem.Text = "Insert/Replace";
      this.replaceToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplace);
      // 
      // extractSoundToolStripMenuItem
      // 
      this.extractSoundToolStripMenuItem.Enabled = false;
      this.extractSoundToolStripMenuItem.Name = "extractSoundToolStripMenuItem";
      this.extractSoundToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.extractSoundToolStripMenuItem.Text = "Extract";
      this.extractSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickExtract);
      // 
      // removeSoundToolStripMenuItem
      // 
      this.removeSoundToolStripMenuItem.Enabled = false;
      this.removeSoundToolStripMenuItem.Name = "removeSoundToolStripMenuItem";
      this.removeSoundToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
      this.removeSoundToolStripMenuItem.Text = "Remove";
      this.removeSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(169, 6);
      // 
      // itemSave
      // 
      this.itemSave.Name = "itemSave";
      this.itemSave.Size = new System.Drawing.Size(172, 22);
      this.itemSave.Text = "Save";
      this.itemSave.Click += new System.EventHandler(this.OnClickSave);
      // 
      // toolStrip1
      // 
      this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
      this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton2,
            this.toolStripDropDownButton1});
      this.toolStrip1.Location = new System.Drawing.Point(0, 0);
      this.toolStrip1.Name = "toolStrip1";
      this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
      this.toolStrip1.Size = new System.Drawing.Size(721, 25);
      this.toolStrip1.TabIndex = 2;
      this.toolStrip1.Text = "tsSoundsMenu";
      // 
      // toolStripButton2
      // 
      this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripButton2.Name = "toolStripButton2";
      this.toolStripButton2.Size = new System.Drawing.Size(46, 22);
      this.toolStripButton2.Text = "Search";
      this.toolStripButton2.Click += new System.EventHandler(this.SearchClick);
      // 
      // toolStripDropDownButton1
      // 
      this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemExtractSoundlist});
      this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
      this.toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
      this.toolStripDropDownButton1.Text = "Misc";
      // 
      // itemExtractSoundlist
      // 
      this.itemExtractSoundlist.Name = "itemExtractSoundlist";
      this.itemExtractSoundlist.Size = new System.Drawing.Size(162, 22);
      this.itemExtractSoundlist.Text = "Extract Soundlist";
      this.itemExtractSoundlist.Click += new System.EventHandler(this.OnClickExtractSoundList);
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.statusStripSounds, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(721, 606);
      this.tableLayoutPanel1.TabIndex = 1;
      // 
      // statusStripSounds
      // 
      this.statusStripSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.seconds,
            this.toolStripStatusSpacer,
            this.playing,
            this.stopButton});
      this.statusStripSounds.Location = new System.Drawing.Point(0, 586);
      this.statusStripSounds.Name = "statusStripSounds";
      this.statusStripSounds.Size = new System.Drawing.Size(721, 20);
      this.statusStripSounds.SizingGrip = false;
      this.statusStripSounds.TabIndex = 1;
      this.statusStripSounds.Text = "statusStrip1";
      // 
      // seconds
      // 
      this.seconds.Name = "seconds";
      this.seconds.Size = new System.Drawing.Size(0, 15);
      // 
      // toolStripStatusSpacer
      // 
      this.toolStripStatusSpacer.Name = "toolStripStatusSpacer";
      this.toolStripStatusSpacer.Size = new System.Drawing.Size(706, 15);
      this.toolStripStatusSpacer.Spring = true;
      // 
      // playing
      // 
      this.playing.Name = "playing";
      this.playing.Size = new System.Drawing.Size(100, 14);
      this.playing.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.playing.Visible = false;
      // 
      // stopButton
      // 
      this.stopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.stopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
      this.stopButton.Name = "stopButton";
      this.stopButton.ShowDropDownArrow = false;
      this.stopButton.Size = new System.Drawing.Size(35, 18);
      this.stopButton.Text = "Stop";
      this.stopButton.Visible = false;
      this.stopButton.Click += new System.EventHandler(this.OnClickStop);
      // 
      // splitContainer1
      // 
      this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.splitContainer1.IsSplitterFixed = true;
      this.splitContainer1.Location = new System.Drawing.Point(3, 28);
      this.splitContainer1.Name = "splitContainer1";
      // 
      // splitContainer1.Panel1
      // 
      this.splitContainer1.Panel1.Controls.Add(this.treeView);
      // 
      // splitContainer1.Panel2
      // 
      this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel2);
      this.splitContainer1.Size = new System.Drawing.Size(715, 555);
      this.splitContainer1.SplitterDistance = 347;
      this.splitContainer1.TabIndex = 3;
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 1;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.Controls.Add(this.groupBox1, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.SelectedSoundGroup, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.groupBox2, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.groupBox3, 0, 3);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 4;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 170F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(364, 555);
      this.tableLayoutPanel2.TabIndex = 0;
      // 
      // groupBox1
      // 
      this.groupBox1.Controls.Add(this.tableLayoutPanel3);
      this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox1.Location = new System.Drawing.Point(3, 3);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(358, 50);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "Generic";
      // 
      // tableLayoutPanel3
      // 
      this.tableLayoutPanel3.ColumnCount = 3;
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.44444F));
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
      this.tableLayoutPanel3.Controls.Add(this.ExtractSoundlistButton, 0, 0);
      this.tableLayoutPanel3.Controls.Add(this.SaveFileButton, 2, 0);
      this.tableLayoutPanel3.Controls.Add(this.SortByNameCheckbox, 1, 0);
      this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel3.Name = "tableLayoutPanel3";
      this.tableLayoutPanel3.RowCount = 1;
      this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel3.Size = new System.Drawing.Size(352, 31);
      this.tableLayoutPanel3.TabIndex = 0;
      // 
      // ExtractSoundlistButton
      // 
      this.ExtractSoundlistButton.AutoSize = true;
      this.ExtractSoundlistButton.Location = new System.Drawing.Point(3, 3);
      this.ExtractSoundlistButton.Name = "ExtractSoundlistButton";
      this.ExtractSoundlistButton.Size = new System.Drawing.Size(111, 23);
      this.ExtractSoundlistButton.TabIndex = 0;
      this.ExtractSoundlistButton.Text = "Extract Soundlist";
      this.ExtractSoundlistButton.UseVisualStyleBackColor = true;
      this.ExtractSoundlistButton.Click += new System.EventHandler(this.OnClickExtractSoundList);
      // 
      // SaveFileButton
      // 
      this.SaveFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.SaveFileButton.Location = new System.Drawing.Point(276, 3);
      this.SaveFileButton.Name = "SaveFileButton";
      this.SaveFileButton.Size = new System.Drawing.Size(73, 23);
      this.SaveFileButton.TabIndex = 1;
      this.SaveFileButton.Text = "Save";
      this.SaveFileButton.UseVisualStyleBackColor = true;
      this.SaveFileButton.Click += new System.EventHandler(this.OnClickSave);
      // 
      // SortByNameCheckbox
      // 
      this.SortByNameCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.SortByNameCheckbox.AutoSize = true;
      this.SortByNameCheckbox.Location = new System.Drawing.Point(161, 3);
      this.SortByNameCheckbox.Name = "SortByNameCheckbox";
      this.SortByNameCheckbox.Size = new System.Drawing.Size(109, 25);
      this.SortByNameCheckbox.TabIndex = 2;
      this.SortByNameCheckbox.Text = "Sort tree by name";
      this.SortByNameCheckbox.UseVisualStyleBackColor = true;
      this.SortByNameCheckbox.CheckedChanged += new System.EventHandler(this.OnChangeSort);
      // 
      // SelectedSoundGroup
      // 
      this.SelectedSoundGroup.Controls.Add(this.tableLayoutPanel4);
      this.SelectedSoundGroup.Dock = System.Windows.Forms.DockStyle.Fill;
      this.SelectedSoundGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.SelectedSoundGroup.Location = new System.Drawing.Point(3, 165);
      this.SelectedSoundGroup.Name = "SelectedSoundGroup";
      this.SelectedSoundGroup.Size = new System.Drawing.Size(358, 164);
      this.SelectedSoundGroup.TabIndex = 1;
      this.SelectedSoundGroup.TabStop = false;
      this.SelectedSoundGroup.Text = "Current Sound";
      // 
      // tableLayoutPanel4
      // 
      this.tableLayoutPanel4.ColumnCount = 2;
      this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
      this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
      this.tableLayoutPanel4.Controls.Add(this.PlaySoundButton, 0, 1);
      this.tableLayoutPanel4.Controls.Add(this.SoundPlaytimeBar, 0, 0);
      this.tableLayoutPanel4.Controls.Add(this.StopSoundButton, 1, 1);
      this.tableLayoutPanel4.Controls.Add(this.ExtractSoundButton, 0, 3);
      this.tableLayoutPanel4.Controls.Add(this.RemoveSoundButton, 1, 3);
      this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel4.Name = "tableLayoutPanel4";
      this.tableLayoutPanel4.RowCount = 4;
      this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel4.Size = new System.Drawing.Size(352, 145);
      this.tableLayoutPanel4.TabIndex = 0;
      // 
      // SoundPlaytimeBar
      // 
      this.tableLayoutPanel4.SetColumnSpan(this.SoundPlaytimeBar, 2);
      this.SoundPlaytimeBar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.SoundPlaytimeBar.Location = new System.Drawing.Point(3, 3);
      this.SoundPlaytimeBar.Name = "SoundPlaytimeBar";
      this.SoundPlaytimeBar.Size = new System.Drawing.Size(346, 20);
      this.SoundPlaytimeBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
      this.SoundPlaytimeBar.TabIndex = 1;
      // 
      // PlaySoundButton
      // 
      this.PlaySoundButton.AutoSize = true;
      this.PlaySoundButton.Location = new System.Drawing.Point(3, 29);
      this.PlaySoundButton.Name = "PlaySoundButton";
      this.PlaySoundButton.Size = new System.Drawing.Size(82, 23);
      this.PlaySoundButton.TabIndex = 0;
      this.PlaySoundButton.Text = "Play";
      this.PlaySoundButton.UseVisualStyleBackColor = true;
      this.PlaySoundButton.Click += new System.EventHandler(this.OnClickPlay);
      // 
      // StopSoundButton
      // 
      this.StopSoundButton.AutoSize = true;
      this.StopSoundButton.Enabled = false;
      this.StopSoundButton.Location = new System.Drawing.Point(91, 29);
      this.StopSoundButton.Name = "StopSoundButton";
      this.StopSoundButton.Size = new System.Drawing.Size(75, 23);
      this.StopSoundButton.TabIndex = 2;
      this.StopSoundButton.Text = "Stop";
      this.StopSoundButton.UseVisualStyleBackColor = true;
      this.StopSoundButton.Click += new System.EventHandler(this.OnClickStop);
      // 
      // ExtractSoundButton
      // 
      this.ExtractSoundButton.AutoSize = true;
      this.ExtractSoundButton.Location = new System.Drawing.Point(3, 78);
      this.ExtractSoundButton.Name = "ExtractSoundButton";
      this.ExtractSoundButton.Size = new System.Drawing.Size(82, 23);
      this.ExtractSoundButton.TabIndex = 3;
      this.ExtractSoundButton.Text = "Extract";
      this.ExtractSoundButton.UseVisualStyleBackColor = true;
      this.ExtractSoundButton.Click += new System.EventHandler(this.OnClickExtract);
      // 
      // RemoveSoundButton
      // 
      this.RemoveSoundButton.AutoSize = true;
      this.RemoveSoundButton.Location = new System.Drawing.Point(91, 78);
      this.RemoveSoundButton.Name = "RemoveSoundButton";
      this.RemoveSoundButton.Size = new System.Drawing.Size(75, 23);
      this.RemoveSoundButton.TabIndex = 4;
      this.RemoveSoundButton.Text = "Remove";
      this.RemoveSoundButton.UseVisualStyleBackColor = true;
      this.RemoveSoundButton.Click += new System.EventHandler(this.OnClickRemove);
      // 
      // groupBox2
      // 
      this.groupBox2.Controls.Add(this.tableLayoutPanel5);
      this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.groupBox2.Location = new System.Drawing.Point(3, 59);
      this.groupBox2.Name = "groupBox2";
      this.groupBox2.Size = new System.Drawing.Size(358, 100);
      this.groupBox2.TabIndex = 2;
      this.groupBox2.TabStop = false;
      this.groupBox2.Text = "Search";
      // 
      // tableLayoutPanel5
      // 
      this.tableLayoutPanel5.ColumnCount = 4;
      this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.31579F));
      this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31.57895F));
      this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.05263F));
      this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.05263F));
      this.tableLayoutPanel5.Controls.Add(this.GoPrevResultButton, 1, 1);
      this.tableLayoutPanel5.Controls.Add(this.SearchNameTextbox, 0, 0);
      this.tableLayoutPanel5.Controls.Add(this.GoNextResultButton, 3, 1);
      this.tableLayoutPanel5.Controls.Add(this.SearchByIdButton, 0, 1);
      this.tableLayoutPanel5.Controls.Add(this.SearchByNameButton, 1, 1);
      this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel5.Name = "tableLayoutPanel5";
      this.tableLayoutPanel5.RowCount = 2;
      this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel5.Size = new System.Drawing.Size(352, 81);
      this.tableLayoutPanel5.TabIndex = 0;
      // 
      // SearchNameTextbox
      // 
      this.tableLayoutPanel5.SetColumnSpan(this.SearchNameTextbox, 4);
      this.SearchNameTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.SearchNameTextbox.Location = new System.Drawing.Point(3, 3);
      this.SearchNameTextbox.Name = "SearchNameTextbox";
      this.SearchNameTextbox.Size = new System.Drawing.Size(346, 20);
      this.SearchNameTextbox.TabIndex = 0;
      // 
      // SearchByNameButton
      // 
      this.SearchByNameButton.AutoSize = true;
      this.SearchByNameButton.Location = new System.Drawing.Point(95, 29);
      this.SearchByNameButton.Name = "SearchByNameButton";
      this.SearchByNameButton.Size = new System.Drawing.Size(96, 23);
      this.SearchByNameButton.TabIndex = 1;
      this.SearchByNameButton.Text = "Search by Name";
      this.SearchByNameButton.UseVisualStyleBackColor = true;
      this.SearchByNameButton.Click += new System.EventHandler(this.SearchByNameButton_Click);
      // 
      // GoNextResultButton
      // 
      this.GoNextResultButton.AutoSize = true;
      this.GoNextResultButton.Location = new System.Drawing.Point(280, 29);
      this.GoNextResultButton.Name = "GoNextResultButton";
      this.GoNextResultButton.Size = new System.Drawing.Size(65, 23);
      this.GoNextResultButton.TabIndex = 3;
      this.GoNextResultButton.Text = "Next";
      this.GoNextResultButton.UseVisualStyleBackColor = true;
      this.GoNextResultButton.Click += new System.EventHandler(this.GoNextResultButton_Click);
      // 
      // SearchByIdButton
      // 
      this.SearchByIdButton.AutoSize = true;
      this.SearchByIdButton.Location = new System.Drawing.Point(3, 29);
      this.SearchByIdButton.Name = "SearchByIdButton";
      this.SearchByIdButton.Size = new System.Drawing.Size(78, 23);
      this.SearchByIdButton.TabIndex = 4;
      this.SearchByIdButton.Text = "Search by Id";
      this.SearchByIdButton.UseVisualStyleBackColor = true;
      this.SearchByIdButton.Click += new System.EventHandler(this.SearchByIdButton_Click);
      // 
      // groupBox3
      // 
      this.groupBox3.Controls.Add(this.tableLayoutPanel6);
      this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.groupBox3.Location = new System.Drawing.Point(3, 335);
      this.groupBox3.Name = "groupBox3";
      this.groupBox3.Size = new System.Drawing.Size(358, 217);
      this.groupBox3.TabIndex = 3;
      this.groupBox3.TabStop = false;
      this.groupBox3.Text = "Insert \\ Replace";
      // 
      // tableLayoutPanel6
      // 
      this.tableLayoutPanel6.ColumnCount = 4;
      this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
      this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel6.Controls.Add(this.label1, 0, 0);
      this.tableLayoutPanel6.Controls.Add(this.label3, 0, 1);
      this.tableLayoutPanel6.Controls.Add(this.IdInsertTextbox, 1, 0);
      this.tableLayoutPanel6.Controls.Add(this.WavFileInsertTextbox, 1, 1);
      this.tableLayoutPanel6.Controls.Add(this.WavChooseInsertButton, 2, 1);
      this.tableLayoutPanel6.Controls.Add(this.AddInsertReplaceButton, 0, 2);
      this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 16);
      this.tableLayoutPanel6.Name = "tableLayoutPanel6";
      this.tableLayoutPanel6.RowCount = 3;
      this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
      this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel6.Size = new System.Drawing.Size(352, 198);
      this.tableLayoutPanel6.TabIndex = 0;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(32, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "ID";
      this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label3.Location = new System.Drawing.Point(3, 25);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(32, 25);
      this.label3.TabIndex = 2;
      this.label3.Text = "WAV";
      this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
      // 
      // IdInsertTextbox
      // 
      this.tableLayoutPanel6.SetColumnSpan(this.IdInsertTextbox, 2);
      this.IdInsertTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.IdInsertTextbox.Location = new System.Drawing.Point(41, 3);
      this.IdInsertTextbox.Name = "IdInsertTextbox";
      this.IdInsertTextbox.ReadOnly = true;
      this.IdInsertTextbox.Size = new System.Drawing.Size(288, 20);
      this.IdInsertTextbox.TabIndex = 3;
      // 
      // WavFileInsertTextbox
      // 
      this.WavFileInsertTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.WavFileInsertTextbox.Location = new System.Drawing.Point(41, 28);
      this.WavFileInsertTextbox.Name = "WavFileInsertTextbox";
      this.WavFileInsertTextbox.Size = new System.Drawing.Size(252, 20);
      this.WavFileInsertTextbox.TabIndex = 5;
      // 
      // WavChooseInsertButton
      // 
      this.WavChooseInsertButton.Location = new System.Drawing.Point(299, 28);
      this.WavChooseInsertButton.Name = "WavChooseInsertButton";
      this.WavChooseInsertButton.Size = new System.Drawing.Size(30, 19);
      this.WavChooseInsertButton.TabIndex = 6;
      this.WavChooseInsertButton.Text = "...";
      this.WavChooseInsertButton.UseVisualStyleBackColor = true;
      this.WavChooseInsertButton.Click += new System.EventHandler(this.WavChooseInsertButton_Click);
      // 
      // AddInsertReplaceButton
      // 
      this.AddInsertReplaceButton.AutoSize = true;
      this.tableLayoutPanel6.SetColumnSpan(this.AddInsertReplaceButton, 2);
      this.AddInsertReplaceButton.Location = new System.Drawing.Point(3, 53);
      this.AddInsertReplaceButton.Name = "AddInsertReplaceButton";
      this.AddInsertReplaceButton.Size = new System.Drawing.Size(87, 23);
      this.AddInsertReplaceButton.TabIndex = 7;
      this.AddInsertReplaceButton.Text = "Add \\ Replace";
      this.AddInsertReplaceButton.UseVisualStyleBackColor = true;
      this.AddInsertReplaceButton.Click += new System.EventHandler(this.AddInsertReplaceButton_Click);
      // 
      // GoPrevResultButton
      // 
      this.GoPrevResultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.GoPrevResultButton.AutoSize = true;
      this.GoPrevResultButton.Location = new System.Drawing.Point(209, 29);
      this.GoPrevResultButton.Name = "GoPrevResultButton";
      this.GoPrevResultButton.Size = new System.Drawing.Size(65, 23);
      this.GoPrevResultButton.TabIndex = 5;
      this.GoPrevResultButton.Text = "Prev";
      this.GoPrevResultButton.UseVisualStyleBackColor = true;
      this.GoPrevResultButton.Click += new System.EventHandler(this.GoPrevResultButton_Click);
      // 
      // SoundsControl
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.DoubleBuffered = true;
      this.Name = "SoundsControl";
      this.Size = new System.Drawing.Size(721, 606);
      this.Load += new System.EventHandler(this.OnLoad);
      this.cmStripSounds.ResumeLayout(false);
      this.toolStrip1.ResumeLayout(false);
      this.toolStrip1.PerformLayout();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.statusStripSounds.ResumeLayout(false);
      this.statusStripSounds.PerformLayout();
      this.splitContainer1.Panel1.ResumeLayout(false);
      this.splitContainer1.Panel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
      this.splitContainer1.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.tableLayoutPanel3.ResumeLayout(false);
      this.tableLayoutPanel3.PerformLayout();
      this.SelectedSoundGroup.ResumeLayout(false);
      this.tableLayoutPanel4.ResumeLayout(false);
      this.tableLayoutPanel4.PerformLayout();
      this.groupBox2.ResumeLayout(false);
      this.tableLayoutPanel5.ResumeLayout(false);
      this.tableLayoutPanel5.PerformLayout();
      this.groupBox3.ResumeLayout(false);
      this.tableLayoutPanel6.ResumeLayout(false);
      this.tableLayoutPanel6.PerformLayout();
      this.ResumeLayout(false);

        }

        private System.Windows.Forms.ContextMenuStrip cmStripSounds;
        private System.Windows.Forms.ToolStripMenuItem extractSoundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem itemExtractSoundlist;
        private System.Windows.Forms.ToolStripMenuItem itemSave;
        private System.Windows.Forms.ToolStripMenuItem nameSortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripProgressBar playing;
        private System.Windows.Forms.ToolStripMenuItem playSoundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeSoundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel seconds;
        private System.Windows.Forms.ToolStripMenuItem showFreeSlotsToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStripSounds;
        private System.Windows.Forms.ToolStripDropDownButton stopButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusSpacer;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripSeparator tsSeparator1;
        private System.Windows.Forms.ToolStripSeparator tsSeparator2;

        #endregion

        private System.Windows.Forms.ToolStripMenuItem ToggleRightPanelToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button ExtractSoundlistButton;
        private System.Windows.Forms.GroupBox SelectedSoundGroup;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TextBox SearchNameTextbox;
        private System.Windows.Forms.Button SearchByNameButton;
        private System.Windows.Forms.Button GoNextResultButton;
        private System.Windows.Forms.Button SaveFileButton;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox IdInsertTextbox;
        private System.Windows.Forms.TextBox WavFileInsertTextbox;
        private System.Windows.Forms.Button WavChooseInsertButton;
        private System.Windows.Forms.Button AddInsertReplaceButton;
        private System.Windows.Forms.Button SearchByIdButton;
        private System.Windows.Forms.CheckBox SortByNameCheckbox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button PlaySoundButton;
        private System.Windows.Forms.ProgressBar SoundPlaytimeBar;
        private System.Windows.Forms.Button StopSoundButton;
        private System.Windows.Forms.Button ExtractSoundButton;
        private System.Windows.Forms.Button RemoveSoundButton;
        private System.Windows.Forms.Button GoPrevResultButton;
    }
}
