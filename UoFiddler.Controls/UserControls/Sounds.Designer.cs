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
    partial class Sounds
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
            this.cmStripSounds.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStripSounds.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.ContextMenuStrip = this.cmStripSounds;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(3, 28);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(615, 276);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AfterSelect);
            this.treeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnDoubleClick);
            this.treeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TreeView_KeyDown);
            // 
            // cmStripSounds
            // 
            this.cmStripSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nameSortToolStripMenuItem,
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
            this.cmStripSounds.Size = new System.Drawing.Size(169, 198);
            // 
            // nameSortToolStripMenuItem
            // 
            this.nameSortToolStripMenuItem.CheckOnClick = true;
            this.nameSortToolStripMenuItem.Name = "nameSortToolStripMenuItem";
            this.nameSortToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.nameSortToolStripMenuItem.Text = "Name Sort";
            this.nameSortToolStripMenuItem.Click += new System.EventHandler(this.OnChangeSort);
            // 
            // tsSeparator1
            // 
            this.tsSeparator1.Name = "tsSeparator1";
            this.tsSeparator1.Size = new System.Drawing.Size(165, 6);
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            this.showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            this.showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            this.showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.showFreeSlotsToolStripMenuItem.Text = "Show free slots";
            this.showFreeSlotsToolStripMenuItem.Click += new System.EventHandler(this.ShowFreeSlotsClick);
            // 
            // nextFreeSlotToolStripMenuItem
            // 
            this.nextFreeSlotToolStripMenuItem.Enabled = false;
            this.nextFreeSlotToolStripMenuItem.Name = "nextFreeSlotToolStripMenuItem";
            this.nextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.nextFreeSlotToolStripMenuItem.Text = "Find next free slot";
            this.nextFreeSlotToolStripMenuItem.Click += new System.EventHandler(this.NextFreeSlotToolStripMenuItem_Click);
            // 
            // tsSeparator2
            // 
            this.tsSeparator2.Name = "tsSeparator2";
            this.tsSeparator2.Size = new System.Drawing.Size(165, 6);
            // 
            // playSoundToolStripMenuItem
            // 
            this.playSoundToolStripMenuItem.Enabled = false;
            this.playSoundToolStripMenuItem.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playSoundToolStripMenuItem.Name = "playSoundToolStripMenuItem";
            this.playSoundToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.playSoundToolStripMenuItem.Text = "Play";
            this.playSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickPlay);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Enabled = false;
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.replaceToolStripMenuItem.Text = "Insert/Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplace);
            // 
            // extractSoundToolStripMenuItem
            // 
            this.extractSoundToolStripMenuItem.Enabled = false;
            this.extractSoundToolStripMenuItem.Name = "extractSoundToolStripMenuItem";
            this.extractSoundToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.extractSoundToolStripMenuItem.Text = "Extract";
            this.extractSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickExtract);
            // 
            // removeSoundToolStripMenuItem
            // 
            this.removeSoundToolStripMenuItem.Enabled = false;
            this.removeSoundToolStripMenuItem.Name = "removeSoundToolStripMenuItem";
            this.removeSoundToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.removeSoundToolStripMenuItem.Text = "Remove";
            this.removeSoundToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(165, 6);
            // 
            // itemSave
            // 
            this.itemSave.Name = "itemSave";
            this.itemSave.Size = new System.Drawing.Size(168, 22);
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
            this.toolStrip1.Size = new System.Drawing.Size(621, 25);
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
            this.tableLayoutPanel1.Controls.Add(this.treeView, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.statusStripSounds, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(621, 327);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // statusStripSounds
            // 
            this.statusStripSounds.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.seconds,
            this.toolStripStatusSpacer,
            this.playing,
            this.stopButton});
            this.statusStripSounds.Location = new System.Drawing.Point(0, 307);
            this.statusStripSounds.Name = "statusStripSounds";
            this.statusStripSounds.Size = new System.Drawing.Size(621, 20);
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
            this.toolStripStatusSpacer.Size = new System.Drawing.Size(606, 15);
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
            // Sounds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.DoubleBuffered = true;
            this.Name = "Sounds";
            this.Size = new System.Drawing.Size(621, 327);
            this.Load += new System.EventHandler(this.OnLoad);
            this.cmStripSounds.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.statusStripSounds.ResumeLayout(false);
            this.statusStripSounds.PerformLayout();
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
    }
}
