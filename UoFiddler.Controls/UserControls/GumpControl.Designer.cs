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
    partial class GumpControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GumpControl));
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            listBox = new System.Windows.Forms.ListBox();
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            extractImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            findNextFreeSlotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            jumpToMaleFemale = new System.Windows.Forms.ToolStripMenuItem();
            replaceGumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            InsertText = new System.Windows.Forms.ToolStripTextBox();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            InsertStartingFromTb = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            topMenuToolStrip = new System.Windows.Forms.ToolStrip();
            IndexToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            searchByIdToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            exportAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pictureBox = new System.Windows.Forms.PictureBox();
            bottomMenuToolStrip = new System.Windows.Forms.ToolStrip();
            IDLabel = new System.Windows.Forms.ToolStripLabel();
            SizeLabel = new System.Windows.Forms.ToolStripLabel();
            ProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            Preload = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            PreLoader = new System.ComponentModel.BackgroundWorker();
            showFreeSlotsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStrip.SuspendLayout();
            topMenuToolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            bottomMenuToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listBox);
            splitContainer1.Panel1.Controls.Add(topMenuToolStrip);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(pictureBox);
            splitContainer1.Panel2.Controls.Add(bottomMenuToolStrip);
            splitContainer1.Size = new System.Drawing.Size(886, 430);
            splitContainer1.SplitterDistance = 289;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // listBox
            // 
            listBox.ContextMenuStrip = contextMenuStrip;
            listBox.Dock = System.Windows.Forms.DockStyle.Fill;
            listBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            listBox.FormattingEnabled = true;
            listBox.IntegralHeight = false;
            listBox.ItemHeight = 60;
            listBox.Location = new System.Drawing.Point(0, 25);
            listBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listBox.Name = "listBox";
            listBox.Size = new System.Drawing.Size(289, 405);
            listBox.TabIndex = 0;
            listBox.DrawItem += ListBox_DrawItem;
            listBox.MeasureItem += ListBox_MeasureItem;
            listBox.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            listBox.KeyUp += Gump_KeyUp;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { showFreeSlotsToolStripMenuItem, findNextFreeSlotToolStripMenuItem, toolStripSeparator2, extractImageToolStripMenuItem, toolStripSeparator6, jumpToMaleFemale, toolStripSeparator5, replaceGumpToolStripMenuItem, insertToolStripMenuItem, toolStripMenuItem1, removeToolStripMenuItem, toolStripSeparator1, saveToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip1";
            contextMenuStrip.Size = new System.Drawing.Size(190, 226);
            // 
            // extractImageToolStripMenuItem
            // 
            extractImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem1, asPngToolStripMenuItem1 });
            extractImageToolStripMenuItem.Name = "extractImageToolStripMenuItem";
            extractImageToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            extractImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += Extract_Image_ClickBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += Extract_Image_ClickTiff;
            // 
            // asJpgToolStripMenuItem1
            // 
            asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            asJpgToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            asJpgToolStripMenuItem1.Text = "As Jpg";
            asJpgToolStripMenuItem1.Click += Extract_Image_ClickJpg;
            // 
            // asPngToolStripMenuItem1
            // 
            asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            asPngToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            asPngToolStripMenuItem1.Text = "As Png";
            asPngToolStripMenuItem1.Click += Extract_Image_ClickPng;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(186, 6);
            // 
            // findNextFreeSlotToolStripMenuItem
            // 
            findNextFreeSlotToolStripMenuItem.Name = "findNextFreeSlotToolStripMenuItem";
            findNextFreeSlotToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            findNextFreeSlotToolStripMenuItem.Text = "Find Next Free Slot";
            findNextFreeSlotToolStripMenuItem.Click += OnClickFindFree;
            // 
            // jumpToMaleFemale
            // 
            jumpToMaleFemale.Name = "jumpToMaleFemale";
            jumpToMaleFemale.Size = new System.Drawing.Size(189, 22);
            jumpToMaleFemale.Text = "Jump to Male/Female";
            jumpToMaleFemale.Click += JumpToMaleFemale_Click;
            // 
            // replaceGumpToolStripMenuItem
            // 
            replaceGumpToolStripMenuItem.Name = "replaceGumpToolStripMenuItem";
            replaceGumpToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            replaceGumpToolStripMenuItem.Text = "Replace";
            replaceGumpToolStripMenuItem.Click += OnClickReplace;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            // 
            // insertToolStripMenuItem
            // 
            insertToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { InsertText });
            insertToolStripMenuItem.Name = "insertToolStripMenuItem";
            insertToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            insertToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            InsertText.Name = "InsertText";
            InsertText.Size = new System.Drawing.Size(100, 23);
            InsertText.KeyDown += OnKeydown_InsertText;
            InsertText.TextChanged += OnTextChanged_InsertAt;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { InsertStartingFromTb });
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(189, 22);
            toolStripMenuItem1.Text = "Insert Starting From";
            // 
            // InsertStartingFromTb
            // 
            InsertStartingFromTb.Name = "InsertStartingFromTb";
            InsertStartingFromTb.Size = new System.Drawing.Size(100, 23);
            InsertStartingFromTb.KeyDown += InsertStartingFromTb_KeyDown;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // topMenuToolStrip
            // 
            topMenuToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            topMenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { IndexToolStripLabel, searchByIdToolStripTextBox, toolStripSeparator4, toolStripDropDownButton1, saveToolStripButton, toolStripSeparator7 });
            topMenuToolStrip.Location = new System.Drawing.Point(0, 0);
            topMenuToolStrip.Name = "topMenuToolStrip";
            topMenuToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            topMenuToolStrip.Size = new System.Drawing.Size(289, 25);
            topMenuToolStrip.TabIndex = 1;
            topMenuToolStrip.Text = "toolStrip2";
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
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportAllToolStripMenuItem });
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            toolStripDropDownButton1.Text = "Misc";
            // 
            // exportAllToolStripMenuItem
            // 
            exportAllToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem1, asTiffToolStripMenuItem1, asJpgToolStripMenuItem, asPngToolStripMenuItem });
            exportAllToolStripMenuItem.Name = "exportAllToolStripMenuItem";
            exportAllToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            exportAllToolStripMenuItem.Text = "Export All..";
            // 
            // asBmpToolStripMenuItem1
            // 
            asBmpToolStripMenuItem1.Name = "asBmpToolStripMenuItem1";
            asBmpToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            asBmpToolStripMenuItem1.Text = "As Bmp";
            asBmpToolStripMenuItem1.Click += OnClick_SaveAllBmp;
            // 
            // asTiffToolStripMenuItem1
            // 
            asTiffToolStripMenuItem1.Name = "asTiffToolStripMenuItem1";
            asTiffToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            asTiffToolStripMenuItem1.Text = "As Tiff";
            asTiffToolStripMenuItem1.Click += OnClick_SaveAllTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += OnClick_SaveAllJpg;
            // 
            // asPngToolStripMenuItem
            // 
            asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            asPngToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            asPngToolStripMenuItem.Text = "As Png";
            asPngToolStripMenuItem.Click += OnClick_SaveAllPng;
            // 
            // pictureBox
            // 
            pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBox.ContextMenuStrip = contextMenuStrip;
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox.Location = new System.Drawing.Point(0, 0);
            pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(592, 405);
            pictureBox.TabIndex = 1;
            pictureBox.TabStop = false;
            // 
            // bottomMenuToolStrip
            // 
            bottomMenuToolStrip.AutoSize = false;
            bottomMenuToolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
            bottomMenuToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            bottomMenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { IDLabel, SizeLabel, ProgressBar, Preload, toolStripSeparator3 });
            bottomMenuToolStrip.Location = new System.Drawing.Point(0, 405);
            bottomMenuToolStrip.Name = "bottomMenuToolStrip";
            bottomMenuToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            bottomMenuToolStrip.Size = new System.Drawing.Size(592, 25);
            bottomMenuToolStrip.TabIndex = 2;
            bottomMenuToolStrip.Text = "toolStrip1";
            // 
            // IDLabel
            // 
            IDLabel.AutoSize = false;
            IDLabel.Name = "IDLabel";
            IDLabel.Size = new System.Drawing.Size(110, 17);
            IDLabel.Text = "ID:";
            IDLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SizeLabel
            // 
            SizeLabel.AutoSize = false;
            SizeLabel.Name = "SizeLabel";
            SizeLabel.Size = new System.Drawing.Size(100, 17);
            SizeLabel.Text = "Size:";
            SizeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProgressBar
            // 
            ProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new System.Drawing.Size(117, 22);
            // 
            // Preload
            // 
            Preload.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            Preload.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            Preload.ImageTransparentColor = System.Drawing.Color.Magenta;
            Preload.Name = "Preload";
            Preload.Size = new System.Drawing.Size(51, 22);
            Preload.Text = "Preload";
            Preload.Click += OnClickPreLoad;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // PreLoader
            // 
            PreLoader.WorkerReportsProgress = true;
            PreLoader.DoWork += PreLoaderDoWork;
            PreLoader.ProgressChanged += PreLoaderProgressChanged;
            PreLoader.RunWorkerCompleted += PreLoaderCompleted;
            // 
            // showFreeSlotsToolStripMenuItem
            // 
            showFreeSlotsToolStripMenuItem.Name = "showFreeSlotsToolStripMenuItem";
            showFreeSlotsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            showFreeSlotsToolStripMenuItem.Text = "Show Free Slots";
            showFreeSlotsToolStripMenuItem.Click += OnClickShowFreeSlots;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new System.Drawing.Size(186, 6);
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new System.Drawing.Size(186, 6);
            // 
            // saveToolStripButton
            // 
            saveToolStripButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            saveToolStripButton.Image = (System.Drawing.Image)resources.GetObject("saveToolStripButton.Image");
            saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            saveToolStripButton.Name = "saveToolStripButton";
            saveToolStripButton.Size = new System.Drawing.Size(35, 22);
            saveToolStripButton.Text = "Save";
            saveToolStripButton.Click += OnClickSave;
            // 
            // toolStripSeparator7
            // 
            toolStripSeparator7.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSeparator7.Name = "toolStripSeparator7";
            toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // GumpControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "GumpControl";
            Size = new System.Drawing.Size(886, 430);
            KeyUp += Gump_KeyUp;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStrip.ResumeLayout(false);
            topMenuToolStrip.ResumeLayout(false);
            topMenuToolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            bottomMenuToolStrip.ResumeLayout(false);
            bottomMenuToolStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exportAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findNextFreeSlotToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel IDLabel;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripMenuItem insertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jumpToMaleFemale;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStripButton Preload;
        private System.ComponentModel.BackgroundWorker PreLoader;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceGumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripLabel SizeLabel;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip bottomMenuToolStrip;
        private System.Windows.Forms.ToolStrip topMenuToolStrip;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripTextBox InsertStartingFromTb;
        private System.Windows.Forms.ToolStripLabel IndexToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox searchByIdToolStripTextBox;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem showFreeSlotsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton saveToolStripButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    }
}
