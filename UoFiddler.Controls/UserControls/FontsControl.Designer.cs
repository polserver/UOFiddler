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
    partial class FontsControl
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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.writeTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.setOffsetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.treeView = new System.Windows.Forms.TreeView();
            this.LoadUnicodeFontsCheckBox = new System.Windows.Forms.CheckBox();
            this.FontsTileView = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.writeTextToolStripMenuItem,
            this.toolStripSeparator1,
            this.setOffsetsToolStripMenuItem,
            this.extractCharacterToolStripMenuItem,
            this.importCharacterToolStripMenuItem,
            this.saveToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(165, 120);
            // 
            // writeTextToolStripMenuItem
            // 
            this.writeTextToolStripMenuItem.Name = "writeTextToolStripMenuItem";
            this.writeTextToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.writeTextToolStripMenuItem.Text = "Write Text";
            this.writeTextToolStripMenuItem.Click += new System.EventHandler(this.OnClickWriteText);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(161, 6);
            // 
            // setOffsetsToolStripMenuItem
            // 
            this.setOffsetsToolStripMenuItem.Name = "setOffsetsToolStripMenuItem";
            this.setOffsetsToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.setOffsetsToolStripMenuItem.Text = "Set Offsets";
            this.setOffsetsToolStripMenuItem.Click += new System.EventHandler(this.OnClickSetOffsets);
            // 
            // extractCharacterToolStripMenuItem
            // 
            this.extractCharacterToolStripMenuItem.Name = "extractCharacterToolStripMenuItem";
            this.extractCharacterToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.extractCharacterToolStripMenuItem.Text = "Extract Character";
            this.extractCharacterToolStripMenuItem.Click += new System.EventHandler(this.OnClickExport);
            // 
            // importCharacterToolStripMenuItem
            // 
            this.importCharacterToolStripMenuItem.Name = "importCharacterToolStripMenuItem";
            this.importCharacterToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.importCharacterToolStripMenuItem.Text = "Import Character";
            this.importCharacterToolStripMenuItem.Click += new System.EventHandler(this.OnClickImport);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.OnClickSave);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.FontsTileView);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(735, 381);
            this.splitContainer1.SplitterDistance = 150;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.treeView);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.LoadUnicodeFontsCheckBox);
            this.splitContainer2.Size = new System.Drawing.Size(150, 381);
            this.splitContainer2.SplitterDistance = 324;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(150, 324);
            this.treeView.TabIndex = 2;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnSelect);
            // 
            // LoadUnicodeFontsCheckBox
            // 
            this.LoadUnicodeFontsCheckBox.AutoSize = true;
            this.LoadUnicodeFontsCheckBox.Location = new System.Drawing.Point(18, 16);
            this.LoadUnicodeFontsCheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.LoadUnicodeFontsCheckBox.Name = "LoadUnicodeFontsCheckBox";
            this.LoadUnicodeFontsCheckBox.Size = new System.Drawing.Size(131, 19);
            this.LoadUnicodeFontsCheckBox.TabIndex = 0;
            this.LoadUnicodeFontsCheckBox.Text = "Load Unicode Fonts";
            this.LoadUnicodeFontsCheckBox.UseVisualStyleBackColor = true;
            this.LoadUnicodeFontsCheckBox.CheckedChanged += new System.EventHandler(this.LoadUnicodeFontsCheckBox_CheckedChanged);
            // 
            // FontsTileView
            // 
            this.FontsTileView.AutoScroll = true;
            this.FontsTileView.AutoScrollMinSize = new System.Drawing.Size(0, 34);
            this.FontsTileView.BackColor = System.Drawing.Color.White;
            this.FontsTileView.ContextMenuStrip = this.contextMenuStrip1;
            this.FontsTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FontsTileView.FocusIndex = -1;
            this.FontsTileView.Location = new System.Drawing.Point(0, 0);
            this.FontsTileView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.FontsTileView.MultiSelect = false;
            this.FontsTileView.Name = "FontsTileView";
            this.FontsTileView.Size = new System.Drawing.Size(580, 359);
            this.FontsTileView.TabIndex = 8;
            this.FontsTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            this.FontsTileView.TileBorderColor = System.Drawing.Color.Gray;
            this.FontsTileView.TileBorderWidth = 1F;
            this.FontsTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            this.FontsTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            this.FontsTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.FontsTileView.TilePadding = new System.Windows.Forms.Padding(0);
            this.FontsTileView.TileSize = new System.Drawing.Size(30, 30);
            this.FontsTileView.VirtualListSize = 1;
            this.FontsTileView.ItemSelectionChanged += new System.EventHandler<System.Windows.Forms.ListViewItemSelectionChangedEventArgs>(this.FontsTileView_ItemSelectionChanged);
            this.FontsTileView.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(this.FontsTileView_DrawItem);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 359);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(580, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(87, 17);
            this.toolStripStatusLabel1.Text = "<no selection>";
            // 
            // FontsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "FontsControl";
            this.Size = new System.Drawing.Size(735, 381);
            this.Load += new System.EventHandler(this.OnLoad);
            this.Resize += new System.EventHandler(this.FontsControl_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem extractCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setOffsetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem writeTextToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private TileView.TileViewControl FontsTileView;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.CheckBox LoadUnicodeFontsCheckBox;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}
