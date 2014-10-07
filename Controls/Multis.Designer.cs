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
    partial class Multis
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.TreeViewMulti = new System.Windows.Forms.TreeView();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toTextfileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toUOAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toWscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapsibleSplitter1 = new FiddlerControls.CollapsibleSplitter();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.exportAllImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllPartsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toTextFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toUOAFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toWSCFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl3 = new System.Windows.Forms.TabControl();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.HeightChangeMulti = new System.Windows.Forms.TrackBar();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.MultiPictureBox = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.statusMulti = new System.Windows.Forms.StatusStrip();
            this.StatusMultiText = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.MultiComponentBox = new System.Windows.Forms.RichTextBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabControl3.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HeightChangeMulti)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MultiPictureBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.statusMulti.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.TreeViewMulti);
            this.splitContainer2.Panel1.Controls.Add(this.collapsibleSplitter1);
            this.splitContainer2.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl3);
            this.splitContainer2.Size = new System.Drawing.Size(528, 334);
            this.splitContainer2.SplitterDistance = 176;
            this.splitContainer2.TabIndex = 1;
            // 
            // TreeViewMulti
            // 
            this.TreeViewMulti.ContextMenuStrip = this.contextMenuStrip2;
            this.TreeViewMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeViewMulti.HideSelection = false;
            this.TreeViewMulti.Location = new System.Drawing.Point(0, 28);
            this.TreeViewMulti.Margin = new System.Windows.Forms.Padding(0);
            this.TreeViewMulti.Name = "TreeViewMulti";
            this.TreeViewMulti.ShowNodeToolTips = true;
            this.TreeViewMulti.Size = new System.Drawing.Size(176, 306);
            this.TreeViewMulti.TabIndex = 0;
            this.TreeViewMulti.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.afterSelect_Multi);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem4,
            this.importToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip1";
            this.contextMenuStrip2.Size = new System.Drawing.Size(157, 92);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.CheckOnClick = true;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(156, 22);
            this.toolStripMenuItem4.Text = "Show Free Slots";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.OnClickFreeSlots);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.importToolStripMenuItem.Text = "Import..";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.OnClickImport);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toTextfileToolStripMenuItem,
            this.toUOAToolStripMenuItem,
            this.toWscToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.exportToolStripMenuItem.Text = "Export..";
            // 
            // toTextfileToolStripMenuItem
            // 
            this.toTextfileToolStripMenuItem.Name = "toTextfileToolStripMenuItem";
            this.toTextfileToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.toTextfileToolStripMenuItem.Text = "To Textfile";
            this.toTextfileToolStripMenuItem.Click += new System.EventHandler(this.OnExportTextFile);
            // 
            // toUOAToolStripMenuItem
            // 
            this.toUOAToolStripMenuItem.Name = "toUOAToolStripMenuItem";
            this.toUOAToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.toUOAToolStripMenuItem.Text = "To UOA";
            this.toUOAToolStripMenuItem.Click += new System.EventHandler(this.OnExportUOAFile);
            // 
            // toWscToolStripMenuItem
            // 
            this.toWscToolStripMenuItem.Name = "toWscToolStripMenuItem";
            this.toWscToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.toWscToolStripMenuItem.Text = "To Wsc";
            this.toWscToolStripMenuItem.Click += new System.EventHandler(this.OnExportWscFile);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
            // 
            // collapsibleSplitter1
            // 
            this.collapsibleSplitter1.AnimationDelay = 20;
            this.collapsibleSplitter1.AnimationStep = 20;
            this.collapsibleSplitter1.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
            this.collapsibleSplitter1.ControlToHide = this.toolStrip1;
            this.collapsibleSplitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.collapsibleSplitter1.ExpandParentForm = false;
            this.collapsibleSplitter1.Location = new System.Drawing.Point(0, 25);
            this.collapsibleSplitter1.Name = "collapsibleSplitter1";
            this.collapsibleSplitter1.TabIndex = 2;
            this.collapsibleSplitter1.TabStop = false;
            this.collapsibleSplitter1.UseAnimations = false;
            this.collapsibleSplitter1.VisualStyle = FiddlerControls.VisualStyles.DoubleDots;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(176, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportAllImagesToolStripMenuItem,
            this.exportAllPartsToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveToolStripMenuItem1});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton1.Text = "Misc";
            // 
            // exportAllImagesToolStripMenuItem
            // 
            this.exportAllImagesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aToolStripMenuItem,
            this.asTiffToolStripMenuItem1,
            this.asJpgToolStripMenuItem});
            this.exportAllImagesToolStripMenuItem.Name = "exportAllImagesToolStripMenuItem";
            this.exportAllImagesToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.exportAllImagesToolStripMenuItem.Text = "Export All Image";
            // 
            // aToolStripMenuItem
            // 
            this.aToolStripMenuItem.Name = "aToolStripMenuItem";
            this.aToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.aToolStripMenuItem.Text = "As Bmp";
            this.aToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllBmp);
            // 
            // asTiffToolStripMenuItem1
            // 
            this.asTiffToolStripMenuItem1.Name = "asTiffToolStripMenuItem1";
            this.asTiffToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem1.Text = "As Tiff";
            this.asTiffToolStripMenuItem1.Click += new System.EventHandler(this.OnClick_SaveAllTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllJpg);
            // 
            // exportAllPartsToolStripMenuItem
            // 
            this.exportAllPartsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toTextFileToolStripMenuItem1,
            this.toUOAFileToolStripMenuItem,
            this.toWSCFileToolStripMenuItem});
            this.exportAllPartsToolStripMenuItem.Name = "exportAllPartsToolStripMenuItem";
            this.exportAllPartsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.exportAllPartsToolStripMenuItem.Text = "Export All Parts";
            // 
            // toTextFileToolStripMenuItem1
            // 
            this.toTextFileToolStripMenuItem1.Name = "toTextFileToolStripMenuItem1";
            this.toTextFileToolStripMenuItem1.Size = new System.Drawing.Size(137, 22);
            this.toTextFileToolStripMenuItem1.Text = "To Text File";
            this.toTextFileToolStripMenuItem1.Click += new System.EventHandler(this.OnClick_SaveAllText);
            // 
            // toUOAFileToolStripMenuItem
            // 
            this.toUOAFileToolStripMenuItem.Name = "toUOAFileToolStripMenuItem";
            this.toUOAFileToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.toUOAFileToolStripMenuItem.Text = "To UOA File";
            this.toUOAFileToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllUOA);
            // 
            // toWSCFileToolStripMenuItem
            // 
            this.toWSCFileToolStripMenuItem.Name = "toWSCFileToolStripMenuItem";
            this.toWSCFileToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.toWSCFileToolStripMenuItem.Text = "To WSC File";
            this.toWSCFileToolStripMenuItem.Click += new System.EventHandler(this.OnClick_SaveAllWSC);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(157, 6);
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(160, 22);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.OnClickSave);
            // 
            // tabControl3
            // 
            this.tabControl3.Controls.Add(this.tabPage5);
            this.tabControl3.Controls.Add(this.tabPage6);
            this.tabControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl3.Location = new System.Drawing.Point(0, 0);
            this.tabControl3.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl3.Name = "tabControl3";
            this.tabControl3.SelectedIndex = 0;
            this.tabControl3.Size = new System.Drawing.Size(348, 334);
            this.tabControl3.TabIndex = 1;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.splitContainer3);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(340, 308);
            this.tabPage5.TabIndex = 0;
            this.tabPage5.Text = "Graphic";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(3, 3);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.HeightChangeMulti);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer3.Size = new System.Drawing.Size(334, 302);
            this.splitContainer3.SplitterDistance = 33;
            this.splitContainer3.SplitterWidth = 1;
            this.splitContainer3.TabIndex = 0;
            // 
            // HeightChangeMulti
            // 
            this.HeightChangeMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HeightChangeMulti.Location = new System.Drawing.Point(0, 0);
            this.HeightChangeMulti.Margin = new System.Windows.Forms.Padding(0);
            this.HeightChangeMulti.MaximumSize = new System.Drawing.Size(0, 33);
            this.HeightChangeMulti.MinimumSize = new System.Drawing.Size(0, 33);
            this.HeightChangeMulti.Name = "HeightChangeMulti";
            this.HeightChangeMulti.Size = new System.Drawing.Size(334, 33);
            this.HeightChangeMulti.TabIndex = 0;
            this.HeightChangeMulti.ValueChanged += new System.EventHandler(this.onValue_HeightChangeMulti);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.MultiPictureBox);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.statusMulti);
            this.splitContainer4.Panel2MinSize = 22;
            this.splitContainer4.Size = new System.Drawing.Size(334, 268);
            this.splitContainer4.SplitterDistance = 241;
            this.splitContainer4.SplitterWidth = 1;
            this.splitContainer4.TabIndex = 1;
            // 
            // MultiPictureBox
            // 
            this.MultiPictureBox.BackColor = System.Drawing.Color.White;
            this.MultiPictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.MultiPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MultiPictureBox.Location = new System.Drawing.Point(0, 0);
            this.MultiPictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.MultiPictureBox.Name = "MultiPictureBox";
            this.MultiPictureBox.Size = new System.Drawing.Size(334, 241);
            this.MultiPictureBox.TabIndex = 0;
            this.MultiPictureBox.TabStop = false;
            this.MultiPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.onPaint_MultiPic);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractImageToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(152, 26);
            // 
            // extractImageToolStripMenuItem
            // 
            this.extractImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem1});
            this.extractImageToolStripMenuItem.Name = "extractImageToolStripMenuItem";
            this.extractImageToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.extractImageToolStripMenuItem.Text = "extract Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.extract_Image_ClickBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.extract_Image_ClickTiff);
            // 
            // asJpgToolStripMenuItem1
            // 
            this.asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            this.asJpgToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem1.Text = "As Jpg";
            this.asJpgToolStripMenuItem1.Click += new System.EventHandler(this.extract_Image_ClickJpg);
            // 
            // statusMulti
            // 
            this.statusMulti.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusMultiText});
            this.statusMulti.Location = new System.Drawing.Point(0, 4);
            this.statusMulti.Name = "statusMulti";
            this.statusMulti.Size = new System.Drawing.Size(334, 22);
            this.statusMulti.TabIndex = 0;
            this.statusMulti.Text = "statusStrip2";
            // 
            // StatusMultiText
            // 
            this.StatusMultiText.Name = "StatusMultiText";
            this.StatusMultiText.Size = new System.Drawing.Size(88, 17);
            this.StatusMultiText.Text = "statusMultiText";
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.MultiComponentBox);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(340, 308);
            this.tabPage6.TabIndex = 1;
            this.tabPage6.Text = "Component List";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // MultiComponentBox
            // 
            this.MultiComponentBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MultiComponentBox.Location = new System.Drawing.Point(3, 3);
            this.MultiComponentBox.Name = "MultiComponentBox";
            this.MultiComponentBox.ReadOnly = true;
            this.MultiComponentBox.Size = new System.Drawing.Size(334, 302);
            this.MultiComponentBox.TabIndex = 0;
            this.MultiComponentBox.Text = "";
            // 
            // Multis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer2);
            this.Name = "Multis";
            this.Size = new System.Drawing.Size(528, 334);
            this.Load += new System.EventHandler(this.OnLoad);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl3.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.HeightChangeMulti)).EndInit();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            this.splitContainer4.Panel2.PerformLayout();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MultiPictureBox)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusMulti.ResumeLayout(false);
            this.statusMulti.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView TreeViewMulti;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TrackBar HeightChangeMulti;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.PictureBox MultiPictureBox;
        private System.Windows.Forms.StatusStrip statusMulti;
        private System.Windows.Forms.ToolStripStatusLabel StatusMultiText;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.RichTextBox MultiComponentBox;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem extractImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toTextfileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toWscToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toUOAToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem exportAllImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exportAllPartsToolStripMenuItem;
        private FiddlerControls.CollapsibleSplitter collapsibleSplitter1;
        private System.Windows.Forms.ToolStripMenuItem toTextFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toUOAFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toWSCFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
    }
}
