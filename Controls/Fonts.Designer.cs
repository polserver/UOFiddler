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

namespace FiddlerControls
{
    partial class Fonts
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importCharacterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.setOffsetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.writeTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
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
            this.treeView.Size = new System.Drawing.Size(121, 324);
            this.treeView.TabIndex = 0;
            this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.onSelect);
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(121, 0);
            this.listView1.Margin = new System.Windows.Forms.Padding(2);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(498, 302);
            this.listView1.TabIndex = 4;
            this.listView1.TileSize = new System.Drawing.Size(46, 46);
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Tile;
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.drawitem);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.onSelectChar);
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(172, 142);
            // 
            // extractCharacterToolStripMenuItem
            // 
            this.extractCharacterToolStripMenuItem.Name = "extractCharacterToolStripMenuItem";
            this.extractCharacterToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.extractCharacterToolStripMenuItem.Text = "Extract Character";
            this.extractCharacterToolStripMenuItem.Click += new System.EventHandler(this.OnClickExport);
            // 
            // importCharacterToolStripMenuItem
            // 
            this.importCharacterToolStripMenuItem.Name = "importCharacterToolStripMenuItem";
            this.importCharacterToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.importCharacterToolStripMenuItem.Text = "Import Character";
            this.importCharacterToolStripMenuItem.Click += new System.EventHandler(this.OnClickImport);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.OnClickSave);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(121, 302);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(498, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(25, 17);
            this.toolStripStatusLabel1.Text = " : ()";
            // 
            // setOffsetsToolStripMenuItem
            // 
            this.setOffsetsToolStripMenuItem.Name = "setOffsetsToolStripMenuItem";
            this.setOffsetsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.setOffsetsToolStripMenuItem.Text = "Set Offsets";
            this.setOffsetsToolStripMenuItem.Click += new System.EventHandler(this.OnClickSetOffsets);
            // 
            // writeTextToolStripMenuItem
            // 
            this.writeTextToolStripMenuItem.Name = "writeTextToolStripMenuItem";
            this.writeTextToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.writeTextToolStripMenuItem.Text = "Write Text";
            this.writeTextToolStripMenuItem.Click += new System.EventHandler(this.OnClickWriteText);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(168, 6);
            // 
            // Fonts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.treeView);
            this.Name = "Fonts";
            this.Size = new System.Drawing.Size(619, 324);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem extractCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importCharacterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setOffsetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem writeTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;


    }
}
