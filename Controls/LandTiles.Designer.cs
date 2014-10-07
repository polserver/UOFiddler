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
    partial class LandTiles
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
            this.exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.selectInTileDataTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInRadarColorTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripLandTile = new System.Windows.Forms.ToolStrip();
            this.namelabel = new System.Windows.Forms.ToolStripLabel();
            this.graphiclabel = new System.Windows.Forms.ToolStripLabel();
            this.FlagsLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.ExportAll = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAsBmp = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAsTiff = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LandTilesSearchButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.listView1 = new System.Windows.Forms.ListView();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStripLandTile.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportImageToolStripMenuItem,
            this.toolStripSeparator3,
            this.selectInTileDataTabToolStripMenuItem,
            this.selectInRadarColorTabToolStripMenuItem,
            this.toolStripSeparator2,
            this.findNextFreeSlotToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.insertAtToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(202, 192);
            // 
            // exportImageToolStripMenuItem
            // 
            this.exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem1});
            this.exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            this.exportImageToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.onClickExportBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.onClickExportTiff);
            // 
            // asJpgToolStripMenuItem1
            // 
            this.asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            this.asJpgToolStripMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.asJpgToolStripMenuItem1.Text = "As Jpg";
            this.asJpgToolStripMenuItem1.Click += new System.EventHandler(this.onClickExportJpg);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(198, 6);
            // 
            // selectInTileDataTabToolStripMenuItem
            // 
            this.selectInTileDataTabToolStripMenuItem.Name = "selectInTileDataTabToolStripMenuItem";
            this.selectInTileDataTabToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.selectInTileDataTabToolStripMenuItem.Text = "Select in TileData tab";
            this.selectInTileDataTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectTiledata);
            // 
            // selectInRadarColorTabToolStripMenuItem
            // 
            this.selectInRadarColorTabToolStripMenuItem.Name = "selectInRadarColorTabToolStripMenuItem";
            this.selectInRadarColorTabToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.selectInRadarColorTabToolStripMenuItem.Text = "Select in RadarColor tab";
            this.selectInRadarColorTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectRadarCol);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(198, 6);
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            this.findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            this.findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            this.findNextFreeSlotToolStripMenuItem.Click += new System.EventHandler(this.onClickFindFree);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.onClickRemove);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplace);
            // 
            // insertAtToolStripMenuItem
            // 
            this.insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertText});
            this.insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            this.insertAtToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.insertAtToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            this.InsertText.Name = "InsertText";
            this.InsertText.Size = new System.Drawing.Size(100, 21);
            this.InsertText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown_Insert);
            this.InsertText.TextChanged += new System.EventHandler(this.onTextChanged_Insert);
            // 
            // toolStripLandTile
            // 
            this.toolStripLandTile.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStripLandTile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.namelabel,
            this.graphiclabel,
            this.FlagsLabel,
            this.toolStripDropDownButton1,
            this.LandTilesSearchButton,
            this.toolStripSeparator4,
            this.SaveButton,
            this.toolStripSeparator5});
            this.toolStripLandTile.Location = new System.Drawing.Point(0, 0);
            this.toolStripLandTile.Name = "toolStripLandTile";
            this.toolStripLandTile.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripLandTile.Size = new System.Drawing.Size(619, 25);
            this.toolStripLandTile.TabIndex = 5;
            this.toolStripLandTile.Text = "toolStrip1";
            // 
            // namelabel
            // 
            this.namelabel.AutoSize = false;
            this.namelabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.namelabel.Name = "namelabel";
            this.namelabel.Size = new System.Drawing.Size(140, 22);
            this.namelabel.Text = "Name:";
            this.namelabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // graphiclabel
            // 
            this.graphiclabel.AutoSize = false;
            this.graphiclabel.Name = "graphiclabel";
            this.graphiclabel.Size = new System.Drawing.Size(120, 22);
            this.graphiclabel.Text = "Graphic:";
            this.graphiclabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FlagsLabel
            // 
            this.FlagsLabel.Name = "FlagsLabel";
            this.FlagsLabel.Size = new System.Drawing.Size(36, 22);
            this.FlagsLabel.Text = "Flags:";
            this.FlagsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportAll});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Margin = new System.Windows.Forms.Padding(0, 1, 20, 2);
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(40, 22);
            this.toolStripDropDownButton1.Text = "Misc";
            // 
            // ExportAll
            // 
            this.ExportAll.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportAsBmp,
            this.ExportAsTiff,
            this.asJpgToolStripMenuItem});
            this.ExportAll.Name = "ExportAll";
            this.ExportAll.Size = new System.Drawing.Size(139, 22);
            this.ExportAll.Text = "Export All..";
            // 
            // ExportAsBmp
            // 
            this.ExportAsBmp.Name = "ExportAsBmp";
            this.ExportAsBmp.Size = new System.Drawing.Size(120, 22);
            this.ExportAsBmp.Text = "As Bmp";
            this.ExportAsBmp.Click += new System.EventHandler(this.OnClick_SaveAllBmp);
            // 
            // ExportAsTiff
            // 
            this.ExportAsTiff.Name = "ExportAsTiff";
            this.ExportAsTiff.Size = new System.Drawing.Size(120, 22);
            this.ExportAsTiff.Text = "As Tiff";
            this.ExportAsTiff.Click += new System.EventHandler(this.OnClick_SaveAllTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllJpg);
            // 
            // LandTilesSearchButton
            // 
            this.LandTilesSearchButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.LandTilesSearchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.LandTilesSearchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.LandTilesSearchButton.Name = "LandTilesSearchButton";
            this.LandTilesSearchButton.Size = new System.Drawing.Size(44, 22);
            this.LandTilesSearchButton.Text = "Search";
            this.LandTilesSearchButton.Click += new System.EventHandler(this.OnClickSearch);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // SaveButton
            // 
            this.SaveButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(35, 22);
            this.SaveButton.Text = "Save";
            this.SaveButton.Click += new System.EventHandler(this.OnClickSave);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 25);
            this.listView1.Margin = new System.Windows.Forms.Padding(2);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.ShowGroups = false;
            this.listView1.Size = new System.Drawing.Size(619, 299);
            this.listView1.TabIndex = 4;
            this.listView1.TileSize = new System.Drawing.Size(46, 46);
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Tile;
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.drawitem);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView_SelectedIndexChanged);
            // 
            // LandTiles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.toolStripLandTile);
            this.Name = "LandTiles";
            this.Size = new System.Drawing.Size(619, 324);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStripLandTile.ResumeLayout(false);
            this.toolStripLandTile.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInTileDataTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem selectInRadarColorTabToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStripLandTile;
        private System.Windows.Forms.ToolStripLabel namelabel;
        private System.Windows.Forms.ToolStripLabel graphiclabel;
        private System.Windows.Forms.ToolStripLabel FlagsLabel;
        private System.Windows.Forms.ToolStripButton LandTilesSearchButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem ExportAll;
        private System.Windows.Forms.ToolStripMenuItem ExportAsBmp;
        private System.Windows.Forms.ToolStripMenuItem ExportAsTiff;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem1;
    }
}
