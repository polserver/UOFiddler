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

using System;
using UoFiddler.Controls.UserControls.TileView;

namespace UoFiddler.Controls.UserControls
{
    partial class LandTilesControl
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
            this.LandTilesContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.selectInTileDataTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInRadarColorTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertText = new System.Windows.Forms.ToolStripTextBox();
            this.replaceStartingFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaceStartingFromTb = new System.Windows.Forms.ToolStripTextBox();
            this.LandTilesToolStrip = new System.Windows.Forms.ToolStrip();
            this.NameLabel = new System.Windows.Forms.ToolStripLabel();
            this.GraphicLabel = new System.Windows.Forms.ToolStripLabel();
            this.FlagsLabel = new System.Windows.Forms.ToolStripLabel();
            this.MiscToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAllAsBmp = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAllAsTiff = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.SearchButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.LandTilesTileView = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            this.LandTilesContextMenuStrip.SuspendLayout();
            this.LandTilesToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // LandTilesContextMenuStrip
            // 
            this.LandTilesContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFreeSlotsToolStripMenuItem,
            this.toolStripSeparator6,
            this.exportImageToolStripMenuItem,
            this.toolStripSeparator3,
            this.selectInTileDataTabToolStripMenuItem,
            this.selectInRadarColorTabToolStripMenuItem,
            this.toolStripSeparator2,
            this.findNextFreeSlotToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.replaceStartingFromToolStripMenuItem,
            this.insertAtToolStripMenuItem});
            this.LandTilesContextMenuStrip.Name = "contextMenuStrip1";
            this.LandTilesContextMenuStrip.Size = new System.Drawing.Size(201, 220);
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            this.showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            this.showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            this.showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.showFreeSlotsToolStripMenuItem.Text = "Show Free Slots";
            this.showFreeSlotsToolStripMenuItem.Click += new System.EventHandler(this.ShowFreeSlotsToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(197, 6);
            // 
            // exportImageToolStripMenuItem
            // 
            this.exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem1,
            this.asPngToolStripMenuItem});
            this.exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            this.exportImageToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportTiff);
            // 
            // asJpgToolStripMenuItem1
            // 
            this.asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            this.asJpgToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem1.Text = "As Jpg";
            this.asJpgToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExportJpg);
            // 
            // asPngToolStripMenuItem
            // 
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem.Text = "As Png";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportPng);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(197, 6);
            // 
            // selectInTileDataTabToolStripMenuItem
            // 
            this.selectInTileDataTabToolStripMenuItem.Name = "selectInTileDataTabToolStripMenuItem";
            this.selectInTileDataTabToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.selectInTileDataTabToolStripMenuItem.Text = "Select in TileData tab";
            this.selectInTileDataTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectTiledata);
            // 
            // selectInRadarColorTabToolStripMenuItem
            // 
            this.selectInRadarColorTabToolStripMenuItem.Name = "selectInRadarColorTabToolStripMenuItem";
            this.selectInRadarColorTabToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.selectInRadarColorTabToolStripMenuItem.Text = "Select in RadarColor tab";
            this.selectInRadarColorTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectRadarCol);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(197, 6);
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            this.findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            this.findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            this.findNextFreeSlotToolStripMenuItem.Click += new System.EventHandler(this.OnClickFindFree);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplace);
            // 
            // insertAtToolStripMenuItem
            // 
            this.insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertText});
            this.insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            this.insertAtToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.insertAtToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            this.InsertText.Name = "InsertText";
            this.InsertText.Size = new System.Drawing.Size(100, 23);
            this.InsertText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownInsert);
            this.InsertText.TextChanged += new System.EventHandler(this.OnTextChangedInsert);
            // 
            // replaceStartingFromToolStripMenuItem
            // 
            this.replaceStartingFromToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ReplaceStartingFromTb});
            this.replaceStartingFromToolStripMenuItem.Name = "replaceStartingFromToolStripMenuItem";
            this.replaceStartingFromToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
            this.replaceStartingFromToolStripMenuItem.Text = "Replace starting from..";
            // 
            // ReplaceStartingFromTb
            // 
            this.ReplaceStartingFromTb.Name = "ReplaceStartingFromTb";
            this.ReplaceStartingFromTb.Size = new System.Drawing.Size(100, 23);
            this.ReplaceStartingFromTb.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ReplaceStartingFromTb_KeyDown);
            // 
            // LandTilesToolStrip
            // 
            this.LandTilesToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.LandTilesToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NameLabel,
            this.GraphicLabel,
            this.FlagsLabel,
            this.MiscToolStripDropDownButton,
            this.toolStripSeparator5,
            this.SearchButton,
            this.toolStripSeparator1,
            this.SaveButton,
            this.toolStripSeparator4});
            this.LandTilesToolStrip.Location = new System.Drawing.Point(0, 0);
            this.LandTilesToolStrip.Name = "LandTilesToolStrip";
            this.LandTilesToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.LandTilesToolStrip.Size = new System.Drawing.Size(611, 25);
            this.LandTilesToolStrip.TabIndex = 5;
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = false;
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(140, 22);
            this.NameLabel.Text = "Name:";
            this.NameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // GraphicLabel
            // 
            this.GraphicLabel.AutoSize = false;
            this.GraphicLabel.Name = "GraphicLabel";
            this.GraphicLabel.Size = new System.Drawing.Size(120, 22);
            this.GraphicLabel.Text = "Graphic:";
            this.GraphicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FlagsLabel
            // 
            this.FlagsLabel.Name = "FlagsLabel";
            this.FlagsLabel.Size = new System.Drawing.Size(37, 22);
            this.FlagsLabel.Text = "Flags:";
            this.FlagsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MiscToolStripDropDownButton
            // 
            this.MiscToolStripDropDownButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.MiscToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.MiscToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAllToolStripMenuItem});
            this.MiscToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MiscToolStripDropDownButton.Margin = new System.Windows.Forms.Padding(0, 1, 20, 2);
            this.MiscToolStripDropDownButton.Name = "MiscToolStripDropDownButton";
            this.MiscToolStripDropDownButton.Size = new System.Drawing.Size(45, 22);
            this.MiscToolStripDropDownButton.Text = "Misc";
            // 
            // exportAllToolStripMenuItem
            // 
            this.exportAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportAllAsBmp,
            this.ExportAllAsTiff,
            this.asJpgToolStripMenuItem,
            this.asPngToolStripMenuItem1});
            this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.exportAllToolStripMenuItem.Text = "Export All..";
            // 
            // ExportAllAsBmp
            // 
            this.ExportAllAsBmp.Name = "ExportAllAsBmp";
            this.ExportAllAsBmp.Size = new System.Drawing.Size(115, 22);
            this.ExportAllAsBmp.Text = "As Bmp";
            this.ExportAllAsBmp.Click += new System.EventHandler(this.OnClick_SaveAllBmp);
            // 
            // ExportAllAsTiff
            // 
            this.ExportAllAsTiff.Name = "ExportAllAsTiff";
            this.ExportAllAsTiff.Size = new System.Drawing.Size(115, 22);
            this.ExportAllAsTiff.Text = "As Tiff";
            this.ExportAllAsTiff.Click += new System.EventHandler(this.OnClick_SaveAllTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllJpg);
            // 
            // asPngToolStripMenuItem1
            // 
            this.asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            this.asPngToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem1.Text = "As Png";
            this.asPngToolStripMenuItem1.Click += new System.EventHandler(this.OnClick_SaveAllPng);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // SearchButton
            // 
            this.SearchButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.SearchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SearchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(46, 22);
            this.SearchButton.Text = "Search";
            this.SearchButton.Click += new System.EventHandler(this.OnClickSearch);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
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
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // LandTilesTileView
            // 
            this.LandTilesTileView.AutoScroll = true;
            this.LandTilesTileView.AutoScrollMinSize = new System.Drawing.Size(0, 50);
            this.LandTilesTileView.BackColor = System.Drawing.SystemColors.Window;
            this.LandTilesTileView.ContextMenuStrip = this.LandTilesContextMenuStrip;
            this.LandTilesTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LandTilesTileView.FocusIndex = -1;
            this.LandTilesTileView.Location = new System.Drawing.Point(0, 25);
            this.LandTilesTileView.MultiSelect = false;
            this.LandTilesTileView.Name = "LandTilesTileView";
            this.LandTilesTileView.Size = new System.Drawing.Size(611, 320);
            this.LandTilesTileView.TabIndex = 8;
            this.LandTilesTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            this.LandTilesTileView.TileBorderColor = System.Drawing.Color.Gray;
            this.LandTilesTileView.TileBorderWidth = 1F;
            this.LandTilesTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            this.LandTilesTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            this.LandTilesTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.LandTilesTileView.TilePadding = new System.Windows.Forms.Padding(1);
            this.LandTilesTileView.TileSize = new System.Drawing.Size(44, 44);
            this.LandTilesTileView.VirtualListSize = 1;
            this.LandTilesTileView.ItemSelectionChanged += new System.EventHandler<System.Windows.Forms.ListViewItemSelectionChangedEventArgs>(this.LandTilesTileView_ItemSelectionChanged);
            this.LandTilesTileView.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(this.LandTilesTileView_DrawItem);
            // 
            // LandTilesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LandTilesTileView);
            this.Controls.Add(this.LandTilesToolStrip);
            this.DoubleBuffered = true;
            this.Name = "LandTilesControl";
            this.Size = new System.Drawing.Size(611, 345);
            this.Load += new System.EventHandler(this.OnLoad);
            this.LandTilesContextMenuStrip.ResumeLayout(false);
            this.LandTilesToolStrip.ResumeLayout(false);
            this.LandTilesToolStrip.PerformLayout();
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
        private System.Windows.Forms.ContextMenuStrip LandTilesContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsBmp;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsTiff;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel FlagsLabel;
        private System.Windows.Forms.ToolStripLabel GraphicLabel;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripLabel NameLabel;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.ToolStripButton SearchButton;
        private System.Windows.Forms.ToolStripMenuItem selectInRadarColorTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInTileDataTabToolStripMenuItem;
        private System.Windows.Forms.ToolStrip LandTilesToolStrip;
        private System.Windows.Forms.ToolStripDropDownButton MiscToolStripDropDownButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private TileView.TileViewControl LandTilesTileView;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem replaceStartingFromToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox ReplaceStartingFromTb;
        private System.Windows.Forms.ToolStripMenuItem showFreeSlotsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
    }
}
