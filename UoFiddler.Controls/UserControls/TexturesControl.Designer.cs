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
    partial class TexturesControl
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
            this.showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceStartingFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaceStartingFromTb = new System.Windows.Forms.ToolStripTextBox();
            this.insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.InsertText = new System.Windows.Forms.ToolStripTextBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.GraphicLabel = new System.Windows.Forms.ToolStripLabel();
            this.MiscToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAllAsBmp = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAllAsTiff = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAllAsJpeg = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportAllAsPng = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.SearchButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.SaveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.TextureTileView = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showFreeSlotsToolStripMenuItem,
            this.toolStripSeparator5,
            this.exportImageToolStripMenuItem,
            this.toolStripSeparator2,
            this.findNextFreeSlotToolStripMenuItem,
            this.removeToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.replaceStartingFromToolStripMenuItem,
            this.insertAtToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(194, 170);
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            this.showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            this.showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            this.showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.showFreeSlotsToolStripMenuItem.Text = "Show Free Slots";
            this.showFreeSlotsToolStripMenuItem.Click += new System.EventHandler(this.ShowFreeSlotsToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(190, 6);
            // 
            // exportImageToolStripMenuItem
            // 
            this.exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem,
            this.asPngToolStripMenuItem});
            this.exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            this.exportImageToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
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
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportJpg);
            // 
            // asPngToolStripMenuItem
            // 
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem.Text = "As Png";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportPng);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            this.findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            this.findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            this.findNextFreeSlotToolStripMenuItem.Click += new System.EventHandler(this.OnClickFindNext);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplace);
            // 
            // replaceStartingFromToolStripMenuItem
            // 
            this.replaceStartingFromToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ReplaceStartingFromTb});
            this.replaceStartingFromToolStripMenuItem.Name = "replaceStartingFromToolStripMenuItem";
            this.replaceStartingFromToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.replaceStartingFromToolStripMenuItem.Text = "Replace starting from..";
            // 
            // ReplaceStartingFromTb
            // 
            this.ReplaceStartingFromTb.Name = "ReplaceStartingFromTb";
            this.ReplaceStartingFromTb.Size = new System.Drawing.Size(100, 23);
            this.ReplaceStartingFromTb.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ReplaceStartingFrom_OnInsert);
            // 
            // insertAtToolStripMenuItem
            // 
            this.insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InsertText});
            this.insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            this.insertAtToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            this.insertAtToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            this.InsertText.Name = "InsertText";
            this.InsertText.Size = new System.Drawing.Size(100, 23);
            this.InsertText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownInsert);
            this.InsertText.TextChanged += new System.EventHandler(this.OnTextChangedInsert);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.GraphicLabel,
            this.MiscToolStripDropDownButton,
            this.toolStripSeparator3,
            this.SearchButton,
            this.toolStripSeparator4,
            this.SaveButton,
            this.toolStripSeparator1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(733, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // GraphicLabel
            // 
            this.GraphicLabel.Name = "GraphicLabel";
            this.GraphicLabel.Size = new System.Drawing.Size(51, 22);
            this.GraphicLabel.Text = "Graphic:";
            this.GraphicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.ExportAllAsJpeg,
            this.ExportAllAsPng});
            this.exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            this.exportAllToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            this.exportAllToolStripMenuItem.Text = "Export All..";
            // 
            // ExportAllAsBmp
            // 
            this.ExportAllAsBmp.Name = "ExportAllAsBmp";
            this.ExportAllAsBmp.Size = new System.Drawing.Size(115, 22);
            this.ExportAllAsBmp.Text = "As Bmp";
            this.ExportAllAsBmp.Click += new System.EventHandler(this.ExportAllAsBmp_Click);
            // 
            // ExportAllAsTiff
            // 
            this.ExportAllAsTiff.Name = "ExportAllAsTiff";
            this.ExportAllAsTiff.Size = new System.Drawing.Size(115, 22);
            this.ExportAllAsTiff.Text = "As Tiff";
            this.ExportAllAsTiff.Click += new System.EventHandler(this.ExportAllAsTiff_Click);
            // 
            // ExportAllAsJpeg
            // 
            this.ExportAllAsJpeg.Name = "ExportAllAsJpeg";
            this.ExportAllAsJpeg.Size = new System.Drawing.Size(115, 22);
            this.ExportAllAsJpeg.Text = "As Jpg";
            this.ExportAllAsJpeg.Click += new System.EventHandler(this.ExportAllAsJpeg_Click);
            // 
            // ExportAllAsPng
            // 
            this.ExportAllAsPng.Name = "ExportAllAsPng";
            this.ExportAllAsPng.Size = new System.Drawing.Size(115, 22);
            this.ExportAllAsPng.Text = "As Png";
            this.ExportAllAsPng.Click += new System.EventHandler(this.ExportAllAsPng_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // SearchButton
            // 
            this.SearchButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.SearchButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SearchButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchButton.Name = "SearchButton";
            this.SearchButton.Size = new System.Drawing.Size(46, 22);
            this.SearchButton.Text = "Search";
            this.SearchButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SearchButton.Click += new System.EventHandler(this.OnClickSearch);
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
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // TextureTileView
            // 
            this.TextureTileView.AutoScroll = true;
            this.TextureTileView.AutoScrollMinSize = new System.Drawing.Size(0, 134);
            this.TextureTileView.ContextMenuStrip = this.contextMenuStrip1;
            this.TextureTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextureTileView.FocusIndex = -1;
            this.TextureTileView.Location = new System.Drawing.Point(0, 25);
            this.TextureTileView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TextureTileView.MultiSelect = false;
            this.TextureTileView.Name = "TextureTileView";
            this.TextureTileView.Size = new System.Drawing.Size(733, 356);
            this.TextureTileView.TabIndex = 5;
            this.TextureTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            this.TextureTileView.TileBorderColor = System.Drawing.Color.Gray;
            this.TextureTileView.TileBorderWidth = 1F;
            this.TextureTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            this.TextureTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            this.TextureTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.TextureTileView.TilePadding = new System.Windows.Forms.Padding(1);
            this.TextureTileView.TileSize = new System.Drawing.Size(128, 128);
            this.TextureTileView.VirtualListSize = 1;
            this.TextureTileView.ItemSelectionChanged += new System.EventHandler<System.Windows.Forms.ListViewItemSelectionChangedEventArgs>(this.TextureTileView_ItemSelectionChanged);
            this.TextureTileView.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(this.TextureTileView_DrawItem);
            // 
            // TexturesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TextureTileView);
            this.Controls.Add(this.toolStrip1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "TexturesControl";
            this.Size = new System.Drawing.Size(733, 381);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel GraphicLabel;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.ToolStripButton SearchButton;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private TileView.TileViewControl TextureTileView;
        private System.Windows.Forms.ToolStripDropDownButton MiscToolStripDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsBmp;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsTiff;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsJpeg;
        private System.Windows.Forms.ToolStripMenuItem ExportAllAsPng;
        private System.Windows.Forms.ToolStripMenuItem replaceStartingFromToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox ReplaceStartingFromTb;
        private System.Windows.Forms.ToolStripMenuItem showFreeSlotsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
    }
}
