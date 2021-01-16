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
            this.treeView = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.writeTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.setOffsetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.FontsTileView = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeView.HideSelection = false;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(121, 328);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnSelect);
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
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(121, 306);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(502, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(24, 17);
            this.toolStripStatusLabel1.Text = " : ()";
            // 
            // FontsTileView
            // 
            this.FontsTileView.AutoScroll = true;
            this.FontsTileView.AutoScrollMinSize = new System.Drawing.Size(0, 34);
            this.FontsTileView.BackColor = System.Drawing.Color.White;
            this.FontsTileView.ContextMenuStrip = this.contextMenuStrip1;
            this.FontsTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FontsTileView.FocusIndex = -1;
            this.FontsTileView.Location = new System.Drawing.Point(121, 0);
            this.FontsTileView.MultiSelect = false;
            this.FontsTileView.Name = "FontsTileView";
            this.FontsTileView.Size = new System.Drawing.Size(502, 306);
            this.FontsTileView.TabIndex = 6;
            this.FontsTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            this.FontsTileView.TileBorderColor = System.Drawing.Color.Gray;
            this.FontsTileView.TileBorderWidth = 1F;
            this.FontsTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            this.FontsTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.FontsTileView.TilePadding = new System.Windows.Forms.Padding(0);
            this.FontsTileView.TileSize = new System.Drawing.Size(30, 30);
            this.FontsTileView.VirtualListSize = 1;
            this.FontsTileView.ItemSelectionChanged += new System.EventHandler<System.Windows.Forms.ListViewItemSelectionChangedEventArgs>(this.FontsTileView_ItemSelectionChanged);
            this.FontsTileView.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(this.FontsTileView_DrawItem);
            // 
            // FontsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FontsTileView);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.treeView);
            this.DoubleBuffered = true;
            this.Name = "FontsControl";
            this.Size = new System.Drawing.Size(623, 328);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem extractCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setOffsetsToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripMenuItem writeTextToolStripMenuItem;

        #endregion

        private TileView.TileViewControl FontsTileView;
    }
}
