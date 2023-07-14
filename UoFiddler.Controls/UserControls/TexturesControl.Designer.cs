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
            components = new System.ComponentModel.Container();
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceStartingFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ReplaceStartingFromTb = new System.Windows.Forms.ToolStripTextBox();
            insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            InsertText = new System.Windows.Forms.ToolStripTextBox();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            topMenuToolStrip = new System.Windows.Forms.ToolStrip();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            SaveButton = new System.Windows.Forms.ToolStripButton();
            IndexToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            searchByIdToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            MiscToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsBmp = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsTiff = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsJpeg = new System.Windows.Forms.ToolStripMenuItem();
            ExportAllAsPng = new System.Windows.Forms.ToolStripMenuItem();
            panel = new System.Windows.Forms.Panel();
            TextureTileView = new TileView.TileViewControl();
            statusStrip = new System.Windows.Forms.StatusStrip();
            GraphicLabel = new System.Windows.Forms.ToolStripStatusLabel();
            contextMenuStrip.SuspendLayout();
            topMenuToolStrip.SuspendLayout();
            panel.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { showFreeSlotsToolStripMenuItem, findNextFreeSlotToolStripMenuItem, toolStripSeparator5, exportImageToolStripMenuItem, toolStripSeparator2, replaceToolStripMenuItem, replaceStartingFromToolStripMenuItem, insertAtToolStripMenuItem, removeToolStripMenuItem, toolStripSeparator3, saveToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip1";
            contextMenuStrip.Size = new System.Drawing.Size(194, 198);
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            showFreeSlotsToolStripMenuItem.CheckOnClick = true;
            showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            showFreeSlotsToolStripMenuItem.Text = "Show Free Slots";
            showFreeSlotsToolStripMenuItem.Click += ShowFreeSlotsToolStripMenuItem_Click;
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            findNextFreeSlotToolStripMenuItem.Click += OnClickFindNext;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(190, 6);
            // 
            // exportImageToolStripMenuItem
            // 
            exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem, asPngToolStripMenuItem });
            exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            exportImageToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += OnClickExportBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += OnClickExportTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += OnClickExportJpg;
            // 
            // asPngToolStripMenuItem
            // 
            asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem.Text = "As Png";
            asPngToolStripMenuItem.Click += OnClickExportPng;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            replaceToolStripMenuItem.Text = "Replace";
            replaceToolStripMenuItem.Click += OnClickReplace;
            // 
            // replaceStartingFromToolStripMenuItem
            // 
            replaceStartingFromToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ReplaceStartingFromTb });
            replaceStartingFromToolStripMenuItem.Name = "replaceStartingFromToolStripMenuItem";
            replaceStartingFromToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            replaceStartingFromToolStripMenuItem.Text = "Replace starting from..";
            // 
            // ReplaceStartingFromTb
            // 
            ReplaceStartingFromTb.Name = "ReplaceStartingFromTb";
            ReplaceStartingFromTb.Size = new System.Drawing.Size(100, 23);
            ReplaceStartingFromTb.KeyDown += ReplaceStartingFrom_OnInsert;
            // 
            // insertAtToolStripMenuItem
            // 
            insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { InsertText });
            insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            insertAtToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            insertAtToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            InsertText.Name = "InsertText";
            InsertText.Size = new System.Drawing.Size(100, 23);
            InsertText.KeyDown += OnKeyDownInsert;
            InsertText.TextChanged += OnTextChangedInsert;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(190, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // topMenuToolStrip
            // 
            topMenuToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            topMenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { SaveButton, toolStripSeparator4, IndexToolStripLabel, searchByIdToolStripTextBox, toolStripSeparator1, MiscToolStripDropDownButton });
            topMenuToolStrip.Location = new System.Drawing.Point(0, 0);
            topMenuToolStrip.Name = "topMenuToolStrip";
            topMenuToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            topMenuToolStrip.Size = new System.Drawing.Size(675, 25);
            topMenuToolStrip.TabIndex = 4;
            topMenuToolStrip.Text = "toolStrip1";
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // SaveButton
            // 
            SaveButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            SaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(35, 22);
            SaveButton.Text = "Save";
            SaveButton.Click += OnClickSave;
            // 
            // IndexToolStripLabel
            // 
            IndexToolStripLabel.Name = "IndexToolStripLabel";
            IndexToolStripLabel.Size = new System.Drawing.Size(39, 22);
            IndexToolStripLabel.Text = "Index:";
            // 
            // searchByIdToolStripTextBox
            // 
            searchByIdToolStripTextBox.Name = "searchByIdToolStripTextBox";
            searchByIdToolStripTextBox.Size = new System.Drawing.Size(100, 25);
            searchByIdToolStripTextBox.KeyUp += SearchByIdToolStripTextBox_KeyUp;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // MiscToolStripDropDownButton
            // 
            MiscToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            MiscToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportAllToolStripMenuItem });
            MiscToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            MiscToolStripDropDownButton.Margin = new System.Windows.Forms.Padding(0, 1, 20, 2);
            MiscToolStripDropDownButton.Name = "MiscToolStripDropDownButton";
            MiscToolStripDropDownButton.Size = new System.Drawing.Size(45, 22);
            MiscToolStripDropDownButton.Text = "Misc";
            // 
            // exportAllToolStripMenuItem
            // 
            exportAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ExportAllAsBmp, ExportAllAsTiff, ExportAllAsJpeg, ExportAllAsPng });
            exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            exportAllToolStripMenuItem.Size = new System.Drawing.Size(131, 22);
            exportAllToolStripMenuItem.Text = "Export All..";
            // 
            // ExportAllAsBmp
            // 
            ExportAllAsBmp.Name = "ExportAllAsBmp";
            ExportAllAsBmp.Size = new System.Drawing.Size(115, 22);
            ExportAllAsBmp.Text = "As Bmp";
            ExportAllAsBmp.Click += ExportAllAsBmp_Click;
            // 
            // ExportAllAsTiff
            // 
            ExportAllAsTiff.Name = "ExportAllAsTiff";
            ExportAllAsTiff.Size = new System.Drawing.Size(115, 22);
            ExportAllAsTiff.Text = "As Tiff";
            ExportAllAsTiff.Click += ExportAllAsTiff_Click;
            // 
            // ExportAllAsJpeg
            // 
            ExportAllAsJpeg.Name = "ExportAllAsJpeg";
            ExportAllAsJpeg.Size = new System.Drawing.Size(115, 22);
            ExportAllAsJpeg.Text = "As Jpg";
            ExportAllAsJpeg.Click += ExportAllAsJpeg_Click;
            // 
            // ExportAllAsPng
            // 
            ExportAllAsPng.Name = "ExportAllAsPng";
            ExportAllAsPng.Size = new System.Drawing.Size(115, 22);
            ExportAllAsPng.Text = "As Png";
            ExportAllAsPng.Click += ExportAllAsPng_Click;
            // 
            // panel
            // 
            panel.Controls.Add(TextureTileView);
            panel.Dock = System.Windows.Forms.DockStyle.Fill;
            panel.Location = new System.Drawing.Point(0, 25);
            panel.Name = "panel";
            panel.Size = new System.Drawing.Size(675, 347);
            panel.TabIndex = 6;
            // 
            // TextureTileView
            // 
            TextureTileView.AutoScroll = true;
            TextureTileView.AutoScrollMinSize = new System.Drawing.Size(0, 134);
            TextureTileView.ContextMenuStrip = contextMenuStrip;
            TextureTileView.Dock = System.Windows.Forms.DockStyle.Fill;
            TextureTileView.FocusIndex = -1;
            TextureTileView.Location = new System.Drawing.Point(0, 0);
            TextureTileView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            TextureTileView.MultiSelect = false;
            TextureTileView.Name = "TextureTileView";
            TextureTileView.Size = new System.Drawing.Size(675, 347);
            TextureTileView.TabIndex = 6;
            TextureTileView.TileBackgroundColor = System.Drawing.SystemColors.Window;
            TextureTileView.TileBorderColor = System.Drawing.Color.Gray;
            TextureTileView.TileBorderWidth = 1F;
            TextureTileView.TileFocusColor = System.Drawing.Color.DarkRed;
            TextureTileView.TileHighlightColor = System.Drawing.SystemColors.Highlight;
            TextureTileView.TileMargin = new System.Windows.Forms.Padding(2, 2, 0, 0);
            TextureTileView.TilePadding = new System.Windows.Forms.Padding(1);
            TextureTileView.TileSize = new System.Drawing.Size(128, 128);
            TextureTileView.VirtualListSize = 1;
            TextureTileView.ItemSelectionChanged += TextureTileView_ItemSelectionChanged;
            TextureTileView.DrawItem += TextureTileView_DrawItem;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { GraphicLabel });
            statusStrip.Location = new System.Drawing.Point(0, 372);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(675, 22);
            statusStrip.TabIndex = 0;
            statusStrip.Text = "statusStrip1";
            // 
            // GraphicLabel
            // 
            GraphicLabel.Name = "GraphicLabel";
            GraphicLabel.Size = new System.Drawing.Size(51, 17);
            GraphicLabel.Text = "Graphic:";
            // 
            // TexturesControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(panel);
            Controls.Add(topMenuToolStrip);
            Controls.Add(statusStrip);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "TexturesControl";
            Size = new System.Drawing.Size(675, 394);
            Load += OnLoad;
            contextMenuStrip.ResumeLayout(false);
            topMenuToolStrip.ResumeLayout(false);
            topMenuToolStrip.PerformLayout();
            panel.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.ToolStrip topMenuToolStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
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
        private System.Windows.Forms.Panel panel;
        private TileView.TileViewControl TextureTileView;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel GraphicLabel;
        private System.Windows.Forms.ToolStripLabel IndexToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox searchByIdToolStripTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
    }
}
