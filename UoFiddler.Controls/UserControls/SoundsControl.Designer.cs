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
            components = new System.ComponentModel.Container();
            treeView = new System.Windows.Forms.TreeView();
            cmStripSounds = new System.Windows.Forms.ContextMenuStrip(components);
            nameSortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tsSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            nextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tsSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            playSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            extractSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeSoundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            itemSave = new System.Windows.Forms.ToolStripMenuItem();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            statusStripSounds = new System.Windows.Forms.StatusStrip();
            seconds = new System.Windows.Forms.ToolStripStatusLabel();
            toolStripStatusSpacer = new System.Windows.Forms.ToolStripStatusLabel();
            playing = new System.Windows.Forms.ToolStripProgressBar();
            stopButton = new System.Windows.Forms.ToolStripDropDownButton();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            groupBox1 = new System.Windows.Forms.GroupBox();
            tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            exportAllSoundsButton = new System.Windows.Forms.Button();
            ExportSoundListCsvButton = new System.Windows.Forms.Button();
            SaveFileButton = new System.Windows.Forms.Button();
            includeSoundIdCheckBox = new System.Windows.Forms.CheckBox();
            SortByNameCheckbox = new System.Windows.Forms.CheckBox();
            SelectedSoundGroup = new System.Windows.Forms.GroupBox();
            tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            PlaySoundButton = new System.Windows.Forms.Button();
            SoundPlaytimeBar = new System.Windows.Forms.ProgressBar();
            StopSoundButton = new System.Windows.Forms.Button();
            ExtractSoundButton = new System.Windows.Forms.Button();
            RemoveSoundButton = new System.Windows.Forms.Button();
            groupBox2 = new System.Windows.Forms.GroupBox();
            tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            GoPrevResultButton = new System.Windows.Forms.Button();
            SearchNameTextbox = new System.Windows.Forms.TextBox();
            GoNextResultButton = new System.Windows.Forms.Button();
            SearchByIdButton = new System.Windows.Forms.Button();
            SearchByNameButton = new System.Windows.Forms.Button();
            groupBox3 = new System.Windows.Forms.GroupBox();
            tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            label1 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            IdInsertTextbox = new System.Windows.Forms.TextBox();
            WavFileInsertTextbox = new System.Windows.Forms.TextBox();
            WavChooseInsertButton = new System.Windows.Forms.Button();
            AddInsertReplaceButton = new System.Windows.Forms.Button();
            cmStripSounds.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            statusStripSounds.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            groupBox1.SuspendLayout();
            tableLayoutPanel3.SuspendLayout();
            SelectedSoundGroup.SuspendLayout();
            tableLayoutPanel4.SuspendLayout();
            groupBox2.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            groupBox3.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            SuspendLayout();
            // 
            // treeView
            // 
            treeView.ContextMenuStrip = cmStripSounds;
            treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView.HideSelection = false;
            treeView.Location = new System.Drawing.Point(0, 0);
            treeView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeView.Name = "treeView";
            treeView.Size = new System.Drawing.Size(406, 670);
            treeView.TabIndex = 0;
            treeView.BeforeSelect += BeforeSelect;
            treeView.AfterSelect += AfterSelect;
            treeView.NodeMouseDoubleClick += OnDoubleClick;
            treeView.KeyDown += TreeView_KeyDown;
            // 
            // cmStripSounds
            // 
            cmStripSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { nameSortToolStripMenuItem, tsSeparator1, showFreeSlotsToolStripMenuItem, nextFreeSlotToolStripMenuItem, tsSeparator2, playSoundToolStripMenuItem, replaceToolStripMenuItem, extractSoundToolStripMenuItem, removeSoundToolStripMenuItem, toolStripMenuItem1, itemSave });
            cmStripSounds.Name = "contextMenuStrip1";
            cmStripSounds.Size = new System.Drawing.Size(169, 198);
            // 
            // nameSortToolStripMenuItem
            // 
            nameSortToolStripMenuItem.CheckOnClick = true;
            nameSortToolStripMenuItem.Name = "nameSortToolStripMenuItem";
            nameSortToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            nameSortToolStripMenuItem.Text = "Name Sort";
            nameSortToolStripMenuItem.Click += OnChangeSort;
            // 
            // tsSeparator1
            // 
            tsSeparator1.Name = "tsSeparator1";
            tsSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            showFreeSlotsToolStripMenuItem.Text = "Show free slots";
            showFreeSlotsToolStripMenuItem.Click += ShowFreeSlotsClick;
            // 
            // nextFreeSlotToolStripMenuItem
            // 
            nextFreeSlotToolStripMenuItem.Enabled = false;
            nextFreeSlotToolStripMenuItem.Name = "nextFreeSlotToolStripMenuItem";
            nextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            nextFreeSlotToolStripMenuItem.Text = "Find next free slot";
            nextFreeSlotToolStripMenuItem.Click += NextFreeSlotToolStripMenuItem_Click;
            // 
            // tsSeparator2
            // 
            tsSeparator2.Name = "tsSeparator2";
            tsSeparator2.Size = new System.Drawing.Size(165, 6);
            // 
            // playSoundToolStripMenuItem
            // 
            playSoundToolStripMenuItem.Enabled = false;
            playSoundToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            playSoundToolStripMenuItem.Name = "playSoundToolStripMenuItem";
            playSoundToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            playSoundToolStripMenuItem.Text = "Play";
            playSoundToolStripMenuItem.Click += OnClickPlay;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Enabled = false;
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            replaceToolStripMenuItem.Text = "Insert/Replace";
            replaceToolStripMenuItem.Click += OnClickReplace;
            // 
            // extractSoundToolStripMenuItem
            // 
            extractSoundToolStripMenuItem.Enabled = false;
            extractSoundToolStripMenuItem.Name = "extractSoundToolStripMenuItem";
            extractSoundToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            extractSoundToolStripMenuItem.Text = "Extract";
            extractSoundToolStripMenuItem.Click += OnClickExtract;
            // 
            // removeSoundToolStripMenuItem
            // 
            removeSoundToolStripMenuItem.Enabled = false;
            removeSoundToolStripMenuItem.Name = "removeSoundToolStripMenuItem";
            removeSoundToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            removeSoundToolStripMenuItem.Text = "Remove";
            removeSoundToolStripMenuItem.Click += OnClickRemove;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(165, 6);
            // 
            // itemSave
            // 
            itemSave.Name = "itemSave";
            itemSave.Size = new System.Drawing.Size(168, 22);
            itemSave.Text = "Save";
            itemSave.Click += OnClickSave;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(statusStripSounds, 0, 2);
            tableLayoutPanel1.Controls.Add(splitContainer1, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel1.Size = new System.Drawing.Size(851, 699);
            tableLayoutPanel1.TabIndex = 1;
            // 
            // statusStripSounds
            // 
            statusStripSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { seconds, toolStripStatusSpacer, playing, stopButton });
            statusStripSounds.Location = new System.Drawing.Point(0, 677);
            statusStripSounds.Name = "statusStripSounds";
            statusStripSounds.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusStripSounds.Size = new System.Drawing.Size(851, 22);
            statusStripSounds.SizingGrip = false;
            statusStripSounds.TabIndex = 1;
            statusStripSounds.Text = "statusStrip1";
            // 
            // seconds
            // 
            seconds.Name = "seconds";
            seconds.Size = new System.Drawing.Size(0, 17);
            // 
            // toolStripStatusSpacer
            // 
            toolStripStatusSpacer.Name = "toolStripStatusSpacer";
            toolStripStatusSpacer.Size = new System.Drawing.Size(649, 17);
            toolStripStatusSpacer.Spring = true;
            // 
            // playing
            // 
            playing.Name = "playing";
            playing.Size = new System.Drawing.Size(117, 16);
            playing.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            playing.Visible = false;
            // 
            // stopButton
            // 
            stopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            stopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            stopButton.Name = "stopButton";
            stopButton.ShowDropDownArrow = false;
            stopButton.Size = new System.Drawing.Size(35, 20);
            stopButton.Text = "Stop";
            stopButton.Visible = false;
            stopButton.Click += OnClickStop;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new System.Drawing.Point(4, 3);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeView);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tableLayoutPanel2);
            splitContainer1.Size = new System.Drawing.Size(843, 670);
            splitContainer1.SplitterDistance = 406;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 3;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 1;
            tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel2.Controls.Add(groupBox1, 0, 0);
            tableLayoutPanel2.Controls.Add(SelectedSoundGroup, 0, 2);
            tableLayoutPanel2.Controls.Add(groupBox2, 0, 1);
            tableLayoutPanel2.Controls.Add(groupBox3, 0, 3);
            tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 4;
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 196F));
            tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel2.Size = new System.Drawing.Size(432, 670);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(tableLayoutPanel3);
            groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox1.Location = new System.Drawing.Point(4, 3);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(424, 92);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Generic";
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 3;
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.44444F));
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            tableLayoutPanel3.Controls.Add(exportAllSoundsButton, 0, 1);
            tableLayoutPanel3.Controls.Add(ExportSoundListCsvButton, 0, 0);
            tableLayoutPanel3.Controls.Add(SaveFileButton, 2, 0);
            tableLayoutPanel3.Controls.Add(includeSoundIdCheckBox, 1, 1);
            tableLayoutPanel3.Controls.Add(SortByNameCheckbox, 1, 0);
            tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel3.Location = new System.Drawing.Point(4, 19);
            tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 2;
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel3.Size = new System.Drawing.Size(416, 70);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // exportAllSoundsButton
            // 
            exportAllSoundsButton.Location = new System.Drawing.Point(4, 38);
            exportAllSoundsButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            exportAllSoundsButton.Name = "exportAllSoundsButton";
            exportAllSoundsButton.Size = new System.Drawing.Size(144, 24);
            exportAllSoundsButton.TabIndex = 9;
            exportAllSoundsButton.Text = "Export all sounds";
            exportAllSoundsButton.UseVisualStyleBackColor = true;
            exportAllSoundsButton.Click += ExportAllSoundsButton_Click;
            // 
            // ExportSoundListCsvButton
            // 
            ExportSoundListCsvButton.AutoSize = true;
            ExportSoundListCsvButton.Location = new System.Drawing.Point(4, 3);
            ExportSoundListCsvButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ExportSoundListCsvButton.Name = "ExportSoundListCsvButton";
            ExportSoundListCsvButton.Size = new System.Drawing.Size(159, 29);
            ExportSoundListCsvButton.TabIndex = 0;
            ExportSoundListCsvButton.Text = "Export sound list (.csv)";
            ExportSoundListCsvButton.UseVisualStyleBackColor = true;
            ExportSoundListCsvButton.Click += OnClickExportSoundListCsv;
            // 
            // SaveFileButton
            // 
            SaveFileButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            SaveFileButton.Location = new System.Drawing.Point(327, 3);
            SaveFileButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SaveFileButton.Name = "SaveFileButton";
            SaveFileButton.Size = new System.Drawing.Size(85, 27);
            SaveFileButton.TabIndex = 1;
            SaveFileButton.Text = "Save";
            SaveFileButton.UseVisualStyleBackColor = true;
            SaveFileButton.Click += OnClickSave;
            // 
            // includeSoundIdCheckBox
            // 
            includeSoundIdCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            includeSoundIdCheckBox.AutoSize = true;
            tableLayoutPanel3.SetColumnSpan(includeSoundIdCheckBox, 2);
            includeSoundIdCheckBox.Location = new System.Drawing.Point(188, 38);
            includeSoundIdCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            includeSoundIdCheckBox.Name = "includeSoundIdCheckBox";
            includeSoundIdCheckBox.Size = new System.Drawing.Size(200, 29);
            includeSoundIdCheckBox.TabIndex = 10;
            includeSoundIdCheckBox.Text = "Export with sound id in file name";
            includeSoundIdCheckBox.UseVisualStyleBackColor = true;
            // 
            // SortByNameCheckbox
            // 
            SortByNameCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            SortByNameCheckbox.AutoSize = true;
            SortByNameCheckbox.Location = new System.Drawing.Point(188, 3);
            SortByNameCheckbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SortByNameCheckbox.Name = "SortByNameCheckbox";
            SortByNameCheckbox.Size = new System.Drawing.Size(119, 29);
            SortByNameCheckbox.TabIndex = 2;
            SortByNameCheckbox.Text = "Sort tree by name";
            SortByNameCheckbox.UseVisualStyleBackColor = true;
            SortByNameCheckbox.CheckedChanged += OnChangeSort;
            // 
            // SelectedSoundGroup
            // 
            SelectedSoundGroup.Controls.Add(tableLayoutPanel4);
            SelectedSoundGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            SelectedSoundGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            SelectedSoundGroup.Location = new System.Drawing.Point(4, 222);
            SelectedSoundGroup.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectedSoundGroup.Name = "SelectedSoundGroup";
            SelectedSoundGroup.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SelectedSoundGroup.Size = new System.Drawing.Size(424, 190);
            SelectedSoundGroup.TabIndex = 1;
            SelectedSoundGroup.TabStop = false;
            SelectedSoundGroup.Text = "Current Sound";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75F));
            tableLayoutPanel4.Controls.Add(PlaySoundButton, 0, 1);
            tableLayoutPanel4.Controls.Add(SoundPlaytimeBar, 0, 0);
            tableLayoutPanel4.Controls.Add(StopSoundButton, 1, 1);
            tableLayoutPanel4.Controls.Add(ExtractSoundButton, 0, 3);
            tableLayoutPanel4.Controls.Add(RemoveSoundButton, 1, 3);
            tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel4.Location = new System.Drawing.Point(4, 16);
            tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 4;
            tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel4.Size = new System.Drawing.Size(416, 171);
            tableLayoutPanel4.TabIndex = 0;
            // 
            // PlaySoundButton
            // 
            PlaySoundButton.AutoSize = true;
            PlaySoundButton.Location = new System.Drawing.Point(4, 32);
            PlaySoundButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            PlaySoundButton.Name = "PlaySoundButton";
            PlaySoundButton.Size = new System.Drawing.Size(95, 29);
            PlaySoundButton.TabIndex = 0;
            PlaySoundButton.Text = "Play";
            PlaySoundButton.UseVisualStyleBackColor = true;
            PlaySoundButton.Click += OnClickPlay;
            // 
            // SoundPlaytimeBar
            // 
            tableLayoutPanel4.SetColumnSpan(SoundPlaytimeBar, 2);
            SoundPlaytimeBar.Dock = System.Windows.Forms.DockStyle.Fill;
            SoundPlaytimeBar.Location = new System.Drawing.Point(4, 3);
            SoundPlaytimeBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SoundPlaytimeBar.Name = "SoundPlaytimeBar";
            SoundPlaytimeBar.Size = new System.Drawing.Size(408, 23);
            SoundPlaytimeBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            SoundPlaytimeBar.TabIndex = 1;
            // 
            // StopSoundButton
            // 
            StopSoundButton.AutoSize = true;
            StopSoundButton.Enabled = false;
            StopSoundButton.Location = new System.Drawing.Point(108, 32);
            StopSoundButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            StopSoundButton.Name = "StopSoundButton";
            StopSoundButton.Size = new System.Drawing.Size(88, 29);
            StopSoundButton.TabIndex = 2;
            StopSoundButton.Text = "Stop";
            StopSoundButton.UseVisualStyleBackColor = true;
            StopSoundButton.Click += OnClickStop;
            // 
            // ExtractSoundButton
            // 
            ExtractSoundButton.AutoSize = true;
            ExtractSoundButton.Location = new System.Drawing.Point(4, 90);
            ExtractSoundButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ExtractSoundButton.Name = "ExtractSoundButton";
            ExtractSoundButton.Size = new System.Drawing.Size(95, 29);
            ExtractSoundButton.TabIndex = 3;
            ExtractSoundButton.Text = "Extract";
            ExtractSoundButton.UseVisualStyleBackColor = true;
            ExtractSoundButton.Click += OnClickExtract;
            // 
            // RemoveSoundButton
            // 
            RemoveSoundButton.AutoSize = true;
            RemoveSoundButton.Location = new System.Drawing.Point(108, 90);
            RemoveSoundButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            RemoveSoundButton.Name = "RemoveSoundButton";
            RemoveSoundButton.Size = new System.Drawing.Size(88, 29);
            RemoveSoundButton.TabIndex = 4;
            RemoveSoundButton.Text = "Remove";
            RemoveSoundButton.UseVisualStyleBackColor = true;
            RemoveSoundButton.Click += OnClickRemove;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(tableLayoutPanel5);
            groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            groupBox2.Location = new System.Drawing.Point(4, 101);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(424, 115);
            groupBox2.TabIndex = 2;
            groupBox2.TabStop = false;
            groupBox2.Text = "Search";
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 4;
            tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.31579F));
            tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 31.57895F));
            tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.05263F));
            tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.05263F));
            tableLayoutPanel5.Controls.Add(GoPrevResultButton, 1, 1);
            tableLayoutPanel5.Controls.Add(SearchNameTextbox, 0, 0);
            tableLayoutPanel5.Controls.Add(GoNextResultButton, 3, 1);
            tableLayoutPanel5.Controls.Add(SearchByIdButton, 0, 1);
            tableLayoutPanel5.Controls.Add(SearchByNameButton, 1, 1);
            tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel5.Location = new System.Drawing.Point(4, 16);
            tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 2;
            tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel5.Size = new System.Drawing.Size(416, 96);
            tableLayoutPanel5.TabIndex = 0;
            // 
            // GoPrevResultButton
            // 
            GoPrevResultButton.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            GoPrevResultButton.AutoSize = true;
            GoPrevResultButton.Location = new System.Drawing.Point(247, 29);
            GoPrevResultButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            GoPrevResultButton.Name = "GoPrevResultButton";
            GoPrevResultButton.Size = new System.Drawing.Size(76, 29);
            GoPrevResultButton.TabIndex = 5;
            GoPrevResultButton.Text = "Prev";
            GoPrevResultButton.UseVisualStyleBackColor = true;
            GoPrevResultButton.Click += GoPrevResultButton_Click;
            // 
            // SearchNameTextbox
            // 
            tableLayoutPanel5.SetColumnSpan(SearchNameTextbox, 4);
            SearchNameTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            SearchNameTextbox.Location = new System.Drawing.Point(4, 3);
            SearchNameTextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchNameTextbox.Name = "SearchNameTextbox";
            SearchNameTextbox.Size = new System.Drawing.Size(408, 20);
            SearchNameTextbox.TabIndex = 0;
            // 
            // GoNextResultButton
            // 
            GoNextResultButton.AutoSize = true;
            GoNextResultButton.Location = new System.Drawing.Point(331, 29);
            GoNextResultButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            GoNextResultButton.Name = "GoNextResultButton";
            GoNextResultButton.Size = new System.Drawing.Size(76, 29);
            GoNextResultButton.TabIndex = 3;
            GoNextResultButton.Text = "Next";
            GoNextResultButton.UseVisualStyleBackColor = true;
            GoNextResultButton.Click += GoNextResultButton_Click;
            // 
            // SearchByIdButton
            // 
            SearchByIdButton.AutoSize = true;
            SearchByIdButton.Location = new System.Drawing.Point(4, 29);
            SearchByIdButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchByIdButton.Name = "SearchByIdButton";
            SearchByIdButton.Size = new System.Drawing.Size(94, 29);
            SearchByIdButton.TabIndex = 4;
            SearchByIdButton.Text = "Search by Id";
            SearchByIdButton.UseVisualStyleBackColor = true;
            SearchByIdButton.Click += SearchByIdButton_Click;
            // 
            // SearchByNameButton
            // 
            SearchByNameButton.AutoSize = true;
            SearchByNameButton.Location = new System.Drawing.Point(113, 29);
            SearchByNameButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            SearchByNameButton.Name = "SearchByNameButton";
            SearchByNameButton.Size = new System.Drawing.Size(120, 29);
            SearchByNameButton.TabIndex = 1;
            SearchByNameButton.Text = "Search by Name";
            SearchByNameButton.UseVisualStyleBackColor = true;
            SearchByNameButton.Click += SearchByNameButton_Click;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(tableLayoutPanel6);
            groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            groupBox3.Location = new System.Drawing.Point(4, 418);
            groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Name = "groupBox3";
            groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox3.Size = new System.Drawing.Size(424, 249);
            groupBox3.TabIndex = 3;
            groupBox3.TabStop = false;
            groupBox3.Text = "Insert \\ Replace";
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 4;
            tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel6.Controls.Add(label1, 0, 0);
            tableLayoutPanel6.Controls.Add(label3, 0, 1);
            tableLayoutPanel6.Controls.Add(IdInsertTextbox, 1, 0);
            tableLayoutPanel6.Controls.Add(WavFileInsertTextbox, 1, 1);
            tableLayoutPanel6.Controls.Add(WavChooseInsertButton, 2, 1);
            tableLayoutPanel6.Controls.Add(AddInsertReplaceButton, 0, 2);
            tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel6.Location = new System.Drawing.Point(4, 19);
            tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 3;
            tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 55F));
            tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            tableLayoutPanel6.Size = new System.Drawing.Size(416, 227);
            tableLayoutPanel6.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Location = new System.Drawing.Point(4, 0);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(32, 29);
            label1.TabIndex = 0;
            label1.Text = "ID";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = System.Windows.Forms.DockStyle.Fill;
            label3.Location = new System.Drawing.Point(4, 29);
            label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(32, 29);
            label3.TabIndex = 2;
            label3.Text = "WAV";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // IdInsertTextbox
            // 
            tableLayoutPanel6.SetColumnSpan(IdInsertTextbox, 2);
            IdInsertTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            IdInsertTextbox.Location = new System.Drawing.Point(44, 3);
            IdInsertTextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            IdInsertTextbox.Name = "IdInsertTextbox";
            IdInsertTextbox.ReadOnly = true;
            IdInsertTextbox.Size = new System.Drawing.Size(345, 23);
            IdInsertTextbox.TabIndex = 3;
            // 
            // WavFileInsertTextbox
            // 
            WavFileInsertTextbox.Dock = System.Windows.Forms.DockStyle.Fill;
            WavFileInsertTextbox.Location = new System.Drawing.Point(44, 32);
            WavFileInsertTextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WavFileInsertTextbox.Name = "WavFileInsertTextbox";
            WavFileInsertTextbox.Size = new System.Drawing.Size(303, 23);
            WavFileInsertTextbox.TabIndex = 5;
            // 
            // WavChooseInsertButton
            // 
            WavChooseInsertButton.Location = new System.Drawing.Point(355, 32);
            WavChooseInsertButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            WavChooseInsertButton.Name = "WavChooseInsertButton";
            WavChooseInsertButton.Size = new System.Drawing.Size(34, 22);
            WavChooseInsertButton.TabIndex = 6;
            WavChooseInsertButton.Text = "...";
            WavChooseInsertButton.UseVisualStyleBackColor = true;
            WavChooseInsertButton.Click += WavChooseInsertButton_Click;
            // 
            // AddInsertReplaceButton
            // 
            AddInsertReplaceButton.AutoSize = true;
            tableLayoutPanel6.SetColumnSpan(AddInsertReplaceButton, 2);
            AddInsertReplaceButton.Location = new System.Drawing.Point(4, 61);
            AddInsertReplaceButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            AddInsertReplaceButton.Name = "AddInsertReplaceButton";
            AddInsertReplaceButton.Size = new System.Drawing.Size(121, 29);
            AddInsertReplaceButton.TabIndex = 7;
            AddInsertReplaceButton.Text = "Add \\ Replace";
            AddInsertReplaceButton.UseVisualStyleBackColor = true;
            AddInsertReplaceButton.Click += AddInsertReplaceButton_Click;
            // 
            // SoundsControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "SoundsControl";
            Size = new System.Drawing.Size(851, 699);
            Load += OnLoad;
            cmStripSounds.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            statusStripSounds.ResumeLayout(false);
            statusStripSounds.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel2.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            tableLayoutPanel3.ResumeLayout(false);
            tableLayoutPanel3.PerformLayout();
            SelectedSoundGroup.ResumeLayout(false);
            tableLayoutPanel4.ResumeLayout(false);
            tableLayoutPanel4.PerformLayout();
            groupBox2.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            groupBox3.ResumeLayout(false);
            tableLayoutPanel6.ResumeLayout(false);
            tableLayoutPanel6.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip cmStripSounds;
        private System.Windows.Forms.ToolStripMenuItem extractSoundToolStripMenuItem;
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
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusSpacer;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripSeparator tsSeparator1;
        private System.Windows.Forms.ToolStripSeparator tsSeparator2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button ExportSoundListCsvButton;
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
        private System.Windows.Forms.Button exportAllSoundsButton;
        private System.Windows.Forms.CheckBox includeSoundIdCheckBox;
    }
}
