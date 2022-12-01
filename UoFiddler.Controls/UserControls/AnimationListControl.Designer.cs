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
    partial class AnimationListControl
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.TreeViewMobs = new System.Windows.Forms.TreeView();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FacingBar = new System.Windows.Forms.TrackBar();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.MainPictureBox = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBMpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listView = new System.Windows.Forms.ListView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.contextMenuStrip3 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportFrameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SettingsButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.sortAlphaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.rewriteXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tryToFindNewGraphicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animationEditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GraphicLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.BaseGraphicLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.HueLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FacingBar)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainPictureBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.contextMenuStrip3.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.TreeViewMobs);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.FacingBar);
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(737, 384);
            this.splitContainer1.SplitterDistance = 239;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // TreeViewMobs
            // 
            this.TreeViewMobs.ContextMenuStrip = this.contextMenuStrip2;
            this.TreeViewMobs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeViewMobs.HideSelection = false;
            this.TreeViewMobs.LabelEdit = true;
            this.TreeViewMobs.Location = new System.Drawing.Point(0, 0);
            this.TreeViewMobs.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TreeViewMobs.Name = "TreeViewMobs";
            this.TreeViewMobs.ShowNodeToolTips = true;
            this.TreeViewMobs.Size = new System.Drawing.Size(239, 384);
            this.TreeViewMobs.TabIndex = 0;
            this.TreeViewMobs.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewMobs_AfterSelect);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(118, 26);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemove);
            // 
            // FacingBar
            // 
            this.FacingBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.FacingBar.AutoSize = false;
            this.FacingBar.LargeChange = 1;
            this.FacingBar.Location = new System.Drawing.Point(390, 361);
            this.FacingBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.FacingBar.Maximum = 7;
            this.FacingBar.Name = "FacingBar";
            this.FacingBar.Size = new System.Drawing.Size(103, 23);
            this.FacingBar.TabIndex = 2;
            this.FacingBar.Scroll += new System.EventHandler(this.OnScrollFacing);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(493, 362);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.MainPictureBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage1.Size = new System.Drawing.Size(485, 334);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Animation";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // MainPictureBox
            // 
            this.MainPictureBox.BackColor = System.Drawing.Color.White;
            this.MainPictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.MainPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPictureBox.Location = new System.Drawing.Point(4, 3);
            this.MainPictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MainPictureBox.Name = "MainPictureBox";
            this.MainPictureBox.Size = new System.Drawing.Size(477, 328);
            this.MainPictureBox.TabIndex = 0;
            this.MainPictureBox.TabStop = false;
            this.MainPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint_MainPicture);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractImageToolStripMenuItem,
            this.extractAnimationToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(174, 48);
            // 
            // extractImageToolStripMenuItem
            // 
            this.extractImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBMpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem,
            this.asPngToolStripMenuItem2});
            this.extractImageToolStripMenuItem.Name = "extractImageToolStripMenuItem";
            this.extractImageToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.extractImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBMpToolStripMenuItem
            // 
            this.asBMpToolStripMenuItem.Name = "asBMpToolStripMenuItem";
            this.asBMpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asBMpToolStripMenuItem.Text = "As Bmp";
            this.asBMpToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickJpg);
            // 
            // asPngToolStripMenuItem2
            // 
            this.asPngToolStripMenuItem2.Name = "asPngToolStripMenuItem2";
            this.asPngToolStripMenuItem2.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem2.Text = "As Png";
            this.asPngToolStripMenuItem2.Click += new System.EventHandler(this.Extract_Image_ClickPng);
            // 
            // extractAnimationToolStripMenuItem
            // 
            this.extractAnimationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem1,
            this.asTiffToolStripMenuItem1,
            this.asJpgToolStripMenuItem1,
            this.asPngToolStripMenuItem1});
            this.extractAnimationToolStripMenuItem.Name = "extractAnimationToolStripMenuItem";
            this.extractAnimationToolStripMenuItem.Size = new System.Drawing.Size(173, 22);
            this.extractAnimationToolStripMenuItem.Text = "Export Animation..";
            // 
            // asBmpToolStripMenuItem1
            // 
            this.asBmpToolStripMenuItem1.Name = "asBmpToolStripMenuItem1";
            this.asBmpToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem1.Text = "As Bmp";
            this.asBmpToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExtractAnimBmp);
            // 
            // asTiffToolStripMenuItem1
            // 
            this.asTiffToolStripMenuItem1.Name = "asTiffToolStripMenuItem1";
            this.asTiffToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem1.Text = "As Tiff";
            this.asTiffToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExtractAnimTiff);
            // 
            // asJpgToolStripMenuItem1
            // 
            this.asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            this.asJpgToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem1.Text = "As Jpg";
            this.asJpgToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExtractAnimJpg);
            // 
            // asPngToolStripMenuItem1
            // 
            this.asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            this.asPngToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem1.Text = "As Png";
            this.asPngToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExtractAnimPng);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listView);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage2.Size = new System.Drawing.Size(485, 334);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Thumbnail List";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView
            // 
            this.listView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.listView.Location = new System.Drawing.Point(4, 3);
            this.listView.Margin = new System.Windows.Forms.Padding(0);
            this.listView.MultiSelect = false;
            this.listView.Name = "listView";
            this.listView.OwnerDraw = true;
            this.listView.Size = new System.Drawing.Size(477, 328);
            this.listView.TabIndex = 0;
            this.listView.TileSize = new System.Drawing.Size(81, 110);
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Tile;
            this.listView.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.ListViewDrawItem);
            this.listView.SelectedIndexChanged += new System.EventHandler(this.SelectChanged_listView);
            this.listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.ListView_DoubleClick);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.listView1);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tabPage3.Size = new System.Drawing.Size(485, 334);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Frames";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.ContextMenuStrip = this.contextMenuStrip3;
            this.listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView1.Location = new System.Drawing.Point(4, 3);
            this.listView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.OwnerDraw = true;
            this.listView1.Size = new System.Drawing.Size(477, 328);
            this.listView1.TabIndex = 0;
            this.listView1.TileSize = new System.Drawing.Size(81, 110);
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Tile;
            this.listView1.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.Frames_ListView_DrawItem);
            // 
            // contextMenuStrip3
            // 
            this.contextMenuStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportFrameToolStripMenuItem});
            this.contextMenuStrip3.Name = "contextMenuStrip3";
            this.contextMenuStrip3.Size = new System.Drawing.Size(151, 26);
            // 
            // exportFrameToolStripMenuItem
            // 
            this.exportFrameToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem2,
            this.asTiffToolStripMenuItem2,
            this.asJpgToolStripMenuItem2,
            this.asPngToolStripMenuItem});
            this.exportFrameToolStripMenuItem.Name = "exportFrameToolStripMenuItem";
            this.exportFrameToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.exportFrameToolStripMenuItem.Text = "Export Frame..";
            // 
            // asBmpToolStripMenuItem2
            // 
            this.asBmpToolStripMenuItem2.Name = "asBmpToolStripMenuItem2";
            this.asBmpToolStripMenuItem2.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem2.Text = "As Bmp";
            this.asBmpToolStripMenuItem2.Click += new System.EventHandler(this.OnClickExportFrameBmp);
            // 
            // asTiffToolStripMenuItem2
            // 
            this.asTiffToolStripMenuItem2.Name = "asTiffToolStripMenuItem2";
            this.asTiffToolStripMenuItem2.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem2.Text = "As Tiff";
            this.asTiffToolStripMenuItem2.Click += new System.EventHandler(this.OnClickExportFrameTiff);
            // 
            // asJpgToolStripMenuItem2
            // 
            this.asJpgToolStripMenuItem2.Name = "asJpgToolStripMenuItem2";
            this.asJpgToolStripMenuItem2.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem2.Text = "As Jpg";
            this.asJpgToolStripMenuItem2.Click += new System.EventHandler(this.OnClickExportFrameJpg);
            // 
            // asPngToolStripMenuItem
            // 
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem.Text = "As Png";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportFramePng);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SettingsButton,
            this.GraphicLabel,
            this.BaseGraphicLabel,
            this.HueLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 362);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.statusStrip1.Size = new System.Drawing.Size(493, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // SettingsButton
            // 
            this.SettingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SettingsButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sortAlphaToolStripMenuItem,
            this.hueToolStripMenuItem,
            this.animateToolStripMenuItem,
            this.toolStripSeparator1,
            this.rewriteXmlToolStripMenuItem,
            this.tryToFindNewGraphicsToolStripMenuItem,
            this.animationEditToolStripMenuItem});
            this.SettingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(62, 20);
            this.SettingsButton.Text = "Settings";
            // 
            // sortAlphaToolStripMenuItem
            // 
            this.sortAlphaToolStripMenuItem.CheckOnClick = true;
            this.sortAlphaToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.sortAlphaToolStripMenuItem.Name = "sortAlphaToolStripMenuItem";
            this.sortAlphaToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.sortAlphaToolStripMenuItem.Text = "Sort alphabetically";
            this.sortAlphaToolStripMenuItem.Click += new System.EventHandler(this.OnClick_Sort);
            // 
            // hueToolStripMenuItem
            // 
            this.hueToolStripMenuItem.Name = "hueToolStripMenuItem";
            this.hueToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.hueToolStripMenuItem.Text = "Hue";
            this.hueToolStripMenuItem.Click += new System.EventHandler(this.OnClick_Hue);
            // 
            // animateToolStripMenuItem
            // 
            this.animateToolStripMenuItem.CheckOnClick = true;
            this.animateToolStripMenuItem.Name = "animateToolStripMenuItem";
            this.animateToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.animateToolStripMenuItem.Text = "Animate";
            this.animateToolStripMenuItem.Click += new System.EventHandler(this.Animate_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(198, 6);
            // 
            // rewriteXmlToolStripMenuItem
            // 
            this.rewriteXmlToolStripMenuItem.Name = "rewriteXmlToolStripMenuItem";
            this.rewriteXmlToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.rewriteXmlToolStripMenuItem.Text = "Rewrite xml";
            this.rewriteXmlToolStripMenuItem.Click += new System.EventHandler(this.RewriteXml);
            // 
            // tryToFindNewGraphicsToolStripMenuItem
            // 
            this.tryToFindNewGraphicsToolStripMenuItem.Name = "tryToFindNewGraphicsToolStripMenuItem";
            this.tryToFindNewGraphicsToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.tryToFindNewGraphicsToolStripMenuItem.Text = "Try to find new Graphics";
            this.tryToFindNewGraphicsToolStripMenuItem.Click += new System.EventHandler(this.OnClickFindNewEntries);
            // 
            // animationEditToolStripMenuItem
            // 
            this.animationEditToolStripMenuItem.Name = "animationEditToolStripMenuItem";
            this.animationEditToolStripMenuItem.Size = new System.Drawing.Size(201, 22);
            this.animationEditToolStripMenuItem.Text = "Animation Edit";
            this.animationEditToolStripMenuItem.Click += new System.EventHandler(this.OnClickAnimationEdit);
            // 
            // GraphicLabel
            // 
            this.GraphicLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.GraphicLabel.Name = "GraphicLabel";
            this.GraphicLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GraphicLabel.Size = new System.Drawing.Size(54, 17);
            this.GraphicLabel.Text = "Graphic: ";
            this.GraphicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BaseGraphicLabel
            // 
            this.BaseGraphicLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.BaseGraphicLabel.Name = "BaseGraphicLabel";
            this.BaseGraphicLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.BaseGraphicLabel.Size = new System.Drawing.Size(75, 17);
            this.BaseGraphicLabel.Text = "BaseGraphic:";
            this.BaseGraphicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // HueLabel
            // 
            this.HueLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.HueLabel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.HueLabel.Name = "HueLabel";
            this.HueLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.HueLabel.Size = new System.Drawing.Size(32, 17);
            this.HueLabel.Text = "Hue:";
            this.HueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AnimationListControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "AnimationListControl";
            this.Size = new System.Drawing.Size(737, 384);
            this.Load += new System.EventHandler(this.OnLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.FacingBar)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainPictureBox)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.contextMenuStrip3.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem animateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animationEditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBMpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem2;
        private System.Windows.Forms.ToolStripStatusLabel BaseGraphicLabel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip3;
        private System.Windows.Forms.ToolStripMenuItem exportFrameToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAnimationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractImageToolStripMenuItem;
        private System.Windows.Forms.TrackBar FacingBar;
        private System.Windows.Forms.ToolStripStatusLabel GraphicLabel;
        private System.Windows.Forms.ToolStripStatusLabel HueLabel;
        private System.Windows.Forms.ToolStripMenuItem hueToolStripMenuItem;
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.PictureBox MainPictureBox;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rewriteXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton SettingsButton;
        private System.Windows.Forms.ToolStripMenuItem sortAlphaToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TreeView TreeViewMobs;
        private System.Windows.Forms.ToolStripMenuItem tryToFindNewGraphicsToolStripMenuItem;
    }
}
