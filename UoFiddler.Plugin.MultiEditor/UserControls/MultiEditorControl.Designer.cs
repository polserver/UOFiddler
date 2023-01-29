/***************************************************************************
 *
 * $Author: MuadDib & Turley
 * 
 * "THE BEER-WARE LICENSE"
 * As long as you retain this notice you can do whatever you want with 
 * this stuff. If we meet some day, and you think this stuff is worth it,
 * you can buy me a beer in return.
 *
 ***************************************************************************/

namespace UoFiddler.Plugin.MultiEditor.UserControls
{
    partial class MultiEditorControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiEditorControl));
            this.TC_MultiEditorToolbox = new System.Windows.Forms.TabControl();
            this.tileTab = new System.Windows.Forms.TabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.treeViewTilesXML = new System.Windows.Forms.TreeView();
            this.DummyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.imageListTreeView = new System.Windows.Forms.ImageList(this.components);
            this.pictureBoxDrawTiles = new System.Windows.Forms.PictureBox();
            this.vScrollBarDrawTiles = new System.Windows.Forms.VScrollBar();
            this.designTab = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BTN_CreateBlank = new System.Windows.Forms.Button();
            this.numericUpDown_Size_Width = new System.Windows.Forms.NumericUpDown();
            this.BTN_Resize = new System.Windows.Forms.Button();
            this.numericUpDown_Size_Height = new System.Windows.Forms.NumericUpDown();
            this.importTab = new System.Windows.Forms.TabPage();
            this.treeViewMultiList = new System.Windows.Forms.TreeView();
            this.Save = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.BTN_Export_UOX3 = new System.Windows.Forms.Button();
            this.BTN_Export_CSV = new System.Windows.Forms.Button();
            this.BTN_Export_WSC = new System.Windows.Forms.Button();
            this.BTN_Export_UOA = new System.Windows.Forms.Button();
            this.textBox_Export = new System.Windows.Forms.TextBox();
            this.BTN_Export_Txt = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textBox_SaveToID = new System.Windows.Forms.TextBox();
            this.BTN_Save = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.BTN_Trans = new System.Windows.Forms.CheckBox();
            this.imageListTools = new System.Windows.Forms.ImageList(this.components);
            this.BTN_Pipette = new System.Windows.Forms.CheckBox();
            this.BTN_Floor = new System.Windows.Forms.CheckBox();
            this.BTN_Z = new System.Windows.Forms.CheckBox();
            this.BTN_Remove = new System.Windows.Forms.CheckBox();
            this.BTN_Draw = new System.Windows.Forms.CheckBox();
            this.BTN_Select = new System.Windows.Forms.CheckBox();
            this.numericUpDown_Floor = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Z = new System.Windows.Forms.NumericUpDown();
            this.collapsibleSplitter1 = new UoFiddler.Controls.UserControls.CollapsibleSplitter();
            this.Selectedpanel = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.DynamiccheckBox = new System.Windows.Forms.CheckBox();
            this.numericUpDown_Selected_Z = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Selected_Y = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_Selected_X = new System.Windows.Forms.NumericUpDown();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.MaxHeightTrackBar = new System.Windows.Forms.TrackBar();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.pictureBoxMulti = new System.Windows.Forms.PictureBox();
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.UndoItems = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem0 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.showWalkablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDoubleSurfaceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAllTransToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.DrawTileLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.SelectedTileLabel = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabelCoord = new System.Windows.Forms.ToolStripLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.FloatingPreviewPanel = new System.Windows.Forms.Panel();
            this.TC_MultiEditorToolbox.SuspendLayout();
            this.tileTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDrawTiles)).BeginInit();
            this.designTab.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Size_Width)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Size_Height)).BeginInit();
            this.importTab.SuspendLayout();
            this.Save.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Floor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Z)).BeginInit();
            this.Selectedpanel.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Selected_Z)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Selected_Y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Selected_X)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MaxHeightTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMulti)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // TC_MultiEditorToolbox
            // 
            this.TC_MultiEditorToolbox.Controls.Add(this.tileTab);
            this.TC_MultiEditorToolbox.Controls.Add(this.designTab);
            this.TC_MultiEditorToolbox.Controls.Add(this.importTab);
            this.TC_MultiEditorToolbox.Controls.Add(this.Save);
            this.TC_MultiEditorToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TC_MultiEditorToolbox.Location = new System.Drawing.Point(0, 104);
            this.TC_MultiEditorToolbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.TC_MultiEditorToolbox.Name = "TC_MultiEditorToolbox";
            this.TC_MultiEditorToolbox.SelectedIndex = 0;
            this.TC_MultiEditorToolbox.Size = new System.Drawing.Size(240, 314);
            this.TC_MultiEditorToolbox.TabIndex = 0;
            // 
            // tileTab
            // 
            this.tileTab.BackColor = System.Drawing.SystemColors.Window;
            this.tileTab.Controls.Add(this.splitContainer4);
            this.tileTab.Location = new System.Drawing.Point(4, 24);
            this.tileTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tileTab.Name = "tileTab";
            this.tileTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tileTab.Size = new System.Drawing.Size(232, 286);
            this.tileTab.TabIndex = 0;
            this.tileTab.Text = "Tiles";
            this.tileTab.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(4, 3);
            this.splitContainer4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.treeViewTilesXML);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.pictureBoxDrawTiles);
            this.splitContainer4.Panel2.Controls.Add(this.vScrollBarDrawTiles);
            this.splitContainer4.Size = new System.Drawing.Size(224, 280);
            this.splitContainer4.SplitterDistance = 126;
            this.splitContainer4.SplitterWidth = 5;
            this.splitContainer4.TabIndex = 0;
            // 
            // treeViewTilesXML
            // 
            this.treeViewTilesXML.ContextMenuStrip = this.DummyContextMenu;
            this.treeViewTilesXML.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewTilesXML.HideSelection = false;
            this.treeViewTilesXML.ImageIndex = 0;
            this.treeViewTilesXML.ImageList = this.imageListTreeView;
            this.treeViewTilesXML.Location = new System.Drawing.Point(0, 0);
            this.treeViewTilesXML.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.treeViewTilesXML.Name = "treeViewTilesXML";
            this.treeViewTilesXML.SelectedImageIndex = 0;
            this.treeViewTilesXML.Size = new System.Drawing.Size(224, 126);
            this.treeViewTilesXML.TabIndex = 0;
            this.treeViewTilesXML.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewTilesXML_OnAfterSelect);
            // 
            // DummyContextMenu
            // 
            this.DummyContextMenu.Name = "DummyContextMenu";
            this.DummyContextMenu.Size = new System.Drawing.Size(61, 4);
            this.DummyContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.OnDummyContextMenuOpening);
            // 
            // imageListTreeView
            // 
            this.imageListTreeView.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListTreeView.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTreeView.ImageStream")));
            this.imageListTreeView.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTreeView.Images.SetKeyName(0, "treeViewImage.bmp");
            // 
            // pictureBoxDrawTiles
            // 
            this.pictureBoxDrawTiles.ContextMenuStrip = this.DummyContextMenu;
            this.pictureBoxDrawTiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxDrawTiles.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxDrawTiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBoxDrawTiles.Name = "pictureBoxDrawTiles";
            this.pictureBoxDrawTiles.Size = new System.Drawing.Size(207, 149);
            this.pictureBoxDrawTiles.TabIndex = 3;
            this.pictureBoxDrawTiles.TabStop = false;
            this.pictureBoxDrawTiles.SizeChanged += new System.EventHandler(this.PictureBoxDrawTiles_OnResize);
            this.pictureBoxDrawTiles.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxDrawTiles_OnPaint);
            this.pictureBoxDrawTiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PictureBoxDrawTiles_OnMouseClick);
            this.pictureBoxDrawTiles.MouseLeave += new System.EventHandler(this.PictureBoxDrawTilesMouseLeave);
            this.pictureBoxDrawTiles.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxDrawTilesMouseMove);
            // 
            // vScrollBarDrawTiles
            // 
            this.vScrollBarDrawTiles.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBarDrawTiles.Location = new System.Drawing.Point(207, 0);
            this.vScrollBarDrawTiles.Name = "vScrollBarDrawTiles";
            this.vScrollBarDrawTiles.Size = new System.Drawing.Size(17, 149);
            this.vScrollBarDrawTiles.TabIndex = 0;
            this.vScrollBarDrawTiles.Scroll += new System.Windows.Forms.ScrollEventHandler(this.VScrollBarDrawTiles_Scroll);
            // 
            // designTab
            // 
            this.designTab.BackColor = System.Drawing.SystemColors.Window;
            this.designTab.Controls.Add(this.groupBox1);
            this.designTab.Location = new System.Drawing.Point(4, 24);
            this.designTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.designTab.Name = "designTab";
            this.designTab.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.designTab.Size = new System.Drawing.Size(232, 286);
            this.designTab.TabIndex = 1;
            this.designTab.Text = "Design";
            this.designTab.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Transparent;
            this.groupBox1.Controls.Add(this.BTN_CreateBlank);
            this.groupBox1.Controls.Add(this.numericUpDown_Size_Width);
            this.groupBox1.Controls.Add(this.BTN_Resize);
            this.groupBox1.Controls.Add(this.numericUpDown_Size_Height);
            this.groupBox1.Location = new System.Drawing.Point(7, 7);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox1.Size = new System.Drawing.Size(210, 90);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Multi Size";
            // 
            // BTN_CreateBlank
            // 
            this.BTN_CreateBlank.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BTN_CreateBlank.Location = new System.Drawing.Point(110, 51);
            this.BTN_CreateBlank.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_CreateBlank.Name = "BTN_CreateBlank";
            this.BTN_CreateBlank.Size = new System.Drawing.Size(92, 27);
            this.BTN_CreateBlank.TabIndex = 3;
            this.BTN_CreateBlank.Text = "Create Blank";
            this.BTN_CreateBlank.UseVisualStyleBackColor = true;
            this.BTN_CreateBlank.Click += new System.EventHandler(this.BTN_CreateBlank_Click);
            // 
            // numericUpDown_Size_Width
            // 
            this.numericUpDown_Size_Width.Location = new System.Drawing.Point(41, 22);
            this.numericUpDown_Size_Width.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_Size_Width.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_Size_Width.Name = "numericUpDown_Size_Width";
            this.numericUpDown_Size_Width.Size = new System.Drawing.Size(61, 23);
            this.numericUpDown_Size_Width.TabIndex = 0;
            // 
            // BTN_Resize
            // 
            this.BTN_Resize.Location = new System.Drawing.Point(14, 51);
            this.BTN_Resize.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Resize.Name = "BTN_Resize";
            this.BTN_Resize.Size = new System.Drawing.Size(88, 27);
            this.BTN_Resize.TabIndex = 2;
            this.BTN_Resize.Text = "Resize Multi";
            this.BTN_Resize.UseVisualStyleBackColor = true;
            this.BTN_Resize.Click += new System.EventHandler(this.BTN_ResizeMulti_Click);
            // 
            // numericUpDown_Size_Height
            // 
            this.numericUpDown_Size_Height.Location = new System.Drawing.Point(110, 22);
            this.numericUpDown_Size_Height.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_Size_Height.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDown_Size_Height.Name = "numericUpDown_Size_Height";
            this.numericUpDown_Size_Height.Size = new System.Drawing.Size(61, 23);
            this.numericUpDown_Size_Height.TabIndex = 1;
            // 
            // importTab
            // 
            this.importTab.BackColor = System.Drawing.SystemColors.Window;
            this.importTab.Controls.Add(this.treeViewMultiList);
            this.importTab.Location = new System.Drawing.Point(4, 24);
            this.importTab.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.importTab.Name = "importTab";
            this.importTab.Size = new System.Drawing.Size(232, 286);
            this.importTab.TabIndex = 2;
            this.importTab.Text = "Import";
            this.importTab.UseVisualStyleBackColor = true;
            // 
            // treeViewMultiList
            // 
            this.treeViewMultiList.ContextMenuStrip = this.DummyContextMenu;
            this.treeViewMultiList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewMultiList.HideSelection = false;
            this.treeViewMultiList.HotTracking = true;
            this.treeViewMultiList.Location = new System.Drawing.Point(0, 0);
            this.treeViewMultiList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.treeViewMultiList.Name = "treeViewMultiList";
            this.treeViewMultiList.Size = new System.Drawing.Size(232, 286);
            this.treeViewMultiList.TabIndex = 0;
            this.treeViewMultiList.NodeMouseHover += new System.Windows.Forms.TreeNodeMouseHoverEventHandler(this.TreeViewMultiList_NodeMouseHover);
            this.treeViewMultiList.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewMultiList_NodeMouseDoubleClick);
            // 
            // Save
            // 
            this.Save.Controls.Add(this.groupBox4);
            this.Save.Controls.Add(this.groupBox2);
            this.Save.Location = new System.Drawing.Point(4, 24);
            this.Save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Save.Name = "Save";
            this.Save.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Save.Size = new System.Drawing.Size(232, 286);
            this.Save.TabIndex = 3;
            this.Save.Text = "Save";
            this.Save.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.Color.Transparent;
            this.groupBox4.Controls.Add(this.BTN_Export_UOX3);
            this.groupBox4.Controls.Add(this.BTN_Export_CSV);
            this.groupBox4.Controls.Add(this.BTN_Export_WSC);
            this.groupBox4.Controls.Add(this.BTN_Export_UOA);
            this.groupBox4.Controls.Add(this.textBox_Export);
            this.groupBox4.Controls.Add(this.BTN_Export_Txt);
            this.groupBox4.Location = new System.Drawing.Point(8, 82);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox4.Size = new System.Drawing.Size(209, 129);
            this.groupBox4.TabIndex = 6;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Export";
            // 
            // BTN_Export_UOX3
            // 
            this.BTN_Export_UOX3.Location = new System.Drawing.Point(66, 87);
            this.BTN_Export_UOX3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Export_UOX3.Name = "BTN_Export_UOX3";
            this.BTN_Export_UOX3.Size = new System.Drawing.Size(75, 27);
            this.BTN_Export_UOX3.TabIndex = 5;
            this.BTN_Export_UOX3.Text = ".uox3";
            this.BTN_Export_UOX3.UseVisualStyleBackColor = true;
            this.BTN_Export_UOX3.Click += new System.EventHandler(this.BTN_Export_UOX3_Click);
            // 
            // BTN_Export_CSV
            // 
            this.BTN_Export_CSV.Location = new System.Drawing.Point(12, 87);
            this.BTN_Export_CSV.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Export_CSV.Name = "BTN_Export_CSV";
            this.BTN_Export_CSV.Size = new System.Drawing.Size(47, 27);
            this.BTN_Export_CSV.TabIndex = 4;
            this.BTN_Export_CSV.Text = ".csv";
            this.BTN_Export_CSV.UseVisualStyleBackColor = true;
            this.BTN_Export_CSV.Click += new System.EventHandler(this.BTN_Export_CSV_OnClick);
            // 
            // BTN_Export_WSC
            // 
            this.BTN_Export_WSC.Location = new System.Drawing.Point(149, 53);
            this.BTN_Export_WSC.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Export_WSC.Name = "BTN_Export_WSC";
            this.BTN_Export_WSC.Size = new System.Drawing.Size(47, 27);
            this.BTN_Export_WSC.TabIndex = 3;
            this.BTN_Export_WSC.Text = ".wsc";
            this.BTN_Export_WSC.UseVisualStyleBackColor = true;
            this.BTN_Export_WSC.Click += new System.EventHandler(this.BTN_Export_WSC_OnClick);
            // 
            // BTN_Export_UOA
            // 
            this.BTN_Export_UOA.Location = new System.Drawing.Point(65, 53);
            this.BTN_Export_UOA.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Export_UOA.Name = "BTN_Export_UOA";
            this.BTN_Export_UOA.Size = new System.Drawing.Size(76, 27);
            this.BTN_Export_UOA.TabIndex = 2;
            this.BTN_Export_UOA.Text = ".uoa.txt";
            this.BTN_Export_UOA.UseVisualStyleBackColor = true;
            this.BTN_Export_UOA.Click += new System.EventHandler(this.BTN_Export_UOA_OnClick);
            // 
            // textBox_Export
            // 
            this.textBox_Export.Location = new System.Drawing.Point(7, 22);
            this.textBox_Export.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_Export.Name = "textBox_Export";
            this.textBox_Export.Size = new System.Drawing.Size(194, 23);
            this.textBox_Export.TabIndex = 1;
            this.toolTip1.SetToolTip(this.textBox_Export, "FileName");
            // 
            // BTN_Export_Txt
            // 
            this.BTN_Export_Txt.Location = new System.Drawing.Point(12, 53);
            this.BTN_Export_Txt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Export_Txt.Name = "BTN_Export_Txt";
            this.BTN_Export_Txt.Size = new System.Drawing.Size(47, 27);
            this.BTN_Export_Txt.TabIndex = 0;
            this.BTN_Export_Txt.Text = ".txt";
            this.BTN_Export_Txt.UseVisualStyleBackColor = true;
            this.BTN_Export_Txt.Click += new System.EventHandler(this.BTN_Export_TXT_OnClick);
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.textBox_SaveToID);
            this.groupBox2.Controls.Add(this.BTN_Save);
            this.groupBox2.Location = new System.Drawing.Point(8, 7);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(209, 68);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Save";
            // 
            // textBox_SaveToID
            // 
            this.textBox_SaveToID.Location = new System.Drawing.Point(107, 25);
            this.textBox_SaveToID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_SaveToID.Name = "textBox_SaveToID";
            this.textBox_SaveToID.Size = new System.Drawing.Size(76, 23);
            this.textBox_SaveToID.TabIndex = 1;
            // 
            // BTN_Save
            // 
            this.BTN_Save.Location = new System.Drawing.Point(8, 23);
            this.BTN_Save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Save.Name = "BTN_Save";
            this.BTN_Save.Size = new System.Drawing.Size(88, 27);
            this.BTN_Save.TabIndex = 0;
            this.BTN_Save.Text = "Save to ID";
            this.BTN_Save.UseVisualStyleBackColor = true;
            this.BTN_Save.Click += new System.EventHandler(this.BTN_Save_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(808, 480);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.BTN_Trans);
            this.splitContainer3.Panel1.Controls.Add(this.BTN_Pipette);
            this.splitContainer3.Panel1.Controls.Add(this.BTN_Floor);
            this.splitContainer3.Panel1.Controls.Add(this.BTN_Z);
            this.splitContainer3.Panel1.Controls.Add(this.BTN_Remove);
            this.splitContainer3.Panel1.Controls.Add(this.BTN_Draw);
            this.splitContainer3.Panel1.Controls.Add(this.BTN_Select);
            this.splitContainer3.Panel1.Controls.Add(this.numericUpDown_Floor);
            this.splitContainer3.Panel1.Controls.Add(this.numericUpDown_Z);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.TC_MultiEditorToolbox);
            this.splitContainer3.Panel2.Controls.Add(this.collapsibleSplitter1);
            this.splitContainer3.Panel2.Controls.Add(this.Selectedpanel);
            this.splitContainer3.Size = new System.Drawing.Size(240, 480);
            this.splitContainer3.SplitterDistance = 60;
            this.splitContainer3.SplitterWidth = 2;
            this.splitContainer3.TabIndex = 1;
            // 
            // BTN_Trans
            // 
            this.BTN_Trans.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Trans.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_Trans.ImageKey = "TransButton.bmp";
            this.BTN_Trans.ImageList = this.imageListTools;
            this.BTN_Trans.Location = new System.Drawing.Point(102, 30);
            this.BTN_Trans.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Trans.Name = "BTN_Trans";
            this.BTN_Trans.Size = new System.Drawing.Size(24, 24);
            this.BTN_Trans.TabIndex = 15;
            this.toolTip1.SetToolTip(this.BTN_Trans, "Switch Transparent");
            this.BTN_Trans.UseVisualStyleBackColor = true;
            this.BTN_Trans.CheckStateChanged += new System.EventHandler(this.BTN_Toolbox_CheckedChanged);
            this.BTN_Trans.Click += new System.EventHandler(this.BTN_Trans_Clicked);
            // 
            // imageListTools
            // 
            this.imageListTools.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListTools.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListTools.ImageStream")));
            this.imageListTools.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListTools.Images.SetKeyName(0, "AltitudeButton.bmp");
            this.imageListTools.Images.SetKeyName(1, "DrawButton.bmp");
            this.imageListTools.Images.SetKeyName(2, "RemoveButton.bmp");
            this.imageListTools.Images.SetKeyName(3, "SelectButton.bmp");
            this.imageListTools.Images.SetKeyName(4, "AltitudeButton_Selected.bmp");
            this.imageListTools.Images.SetKeyName(5, "DrawButton_Selected.bmp");
            this.imageListTools.Images.SetKeyName(6, "RemoveButton_Selected.bmp");
            this.imageListTools.Images.SetKeyName(7, "SelectButton_Selected.bmp");
            this.imageListTools.Images.SetKeyName(8, "VirtualFloorButton.bmp");
            this.imageListTools.Images.SetKeyName(9, "VirtualFloorButton_Selected.bmp");
            this.imageListTools.Images.SetKeyName(10, "PipetteButton.bmp");
            this.imageListTools.Images.SetKeyName(11, "PipetteButton_Selected.bmp");
            this.imageListTools.Images.SetKeyName(12, "TransButton_Selected.bmp");
            this.imageListTools.Images.SetKeyName(13, "TransButton.bmp");
            // 
            // BTN_Pipette
            // 
            this.BTN_Pipette.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Pipette.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_Pipette.ImageKey = "PipetteButton.bmp";
            this.BTN_Pipette.ImageList = this.imageListTools;
            this.BTN_Pipette.Location = new System.Drawing.Point(98, 3);
            this.BTN_Pipette.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Pipette.Name = "BTN_Pipette";
            this.BTN_Pipette.Size = new System.Drawing.Size(24, 24);
            this.BTN_Pipette.TabIndex = 14;
            this.toolTip1.SetToolTip(this.BTN_Pipette, "Pick A Tile");
            this.BTN_Pipette.UseVisualStyleBackColor = true;
            this.BTN_Pipette.CheckStateChanged += new System.EventHandler(this.BTN_Toolbox_CheckedChanged);
            this.BTN_Pipette.Click += new System.EventHandler(this.BTN_Pipette_Click);
            // 
            // BTN_Floor
            // 
            this.BTN_Floor.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Floor.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_Floor.ImageKey = "VirtualFloorButton.bmp";
            this.BTN_Floor.ImageList = this.imageListTools;
            this.BTN_Floor.Location = new System.Drawing.Point(5, 30);
            this.BTN_Floor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Floor.Name = "BTN_Floor";
            this.BTN_Floor.Size = new System.Drawing.Size(24, 24);
            this.BTN_Floor.TabIndex = 13;
            this.toolTip1.SetToolTip(this.BTN_Floor, "Draw Virtual Floor");
            this.BTN_Floor.UseVisualStyleBackColor = true;
            this.BTN_Floor.CheckStateChanged += new System.EventHandler(this.BTN_Toolbox_CheckedChanged);
            this.BTN_Floor.Click += new System.EventHandler(this.BTN_Floor_Clicked);
            // 
            // BTN_Z
            // 
            this.BTN_Z.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Z.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_Z.ImageKey = "AltitudeButton.bmp";
            this.BTN_Z.ImageList = this.imageListTools;
            this.BTN_Z.Location = new System.Drawing.Point(130, 3);
            this.BTN_Z.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Z.Name = "BTN_Z";
            this.BTN_Z.Size = new System.Drawing.Size(24, 24);
            this.BTN_Z.TabIndex = 12;
            this.toolTip1.SetToolTip(this.BTN_Z, "Apply Z Level");
            this.BTN_Z.UseVisualStyleBackColor = true;
            this.BTN_Z.CheckStateChanged += new System.EventHandler(this.BTN_Toolbox_CheckedChanged);
            this.BTN_Z.Click += new System.EventHandler(this.BTN_Z_Click);
            // 
            // BTN_Remove
            // 
            this.BTN_Remove.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Remove.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_Remove.ImageKey = "RemoveButton.bmp";
            this.BTN_Remove.ImageList = this.imageListTools;
            this.BTN_Remove.Location = new System.Drawing.Point(68, 3);
            this.BTN_Remove.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Remove.Name = "BTN_Remove";
            this.BTN_Remove.Size = new System.Drawing.Size(24, 24);
            this.BTN_Remove.TabIndex = 11;
            this.toolTip1.SetToolTip(this.BTN_Remove, "Remove A Tile");
            this.BTN_Remove.UseVisualStyleBackColor = true;
            this.BTN_Remove.CheckStateChanged += new System.EventHandler(this.BTN_Toolbox_CheckedChanged);
            this.BTN_Remove.Click += new System.EventHandler(this.BTN_Remove_Click);
            // 
            // BTN_Draw
            // 
            this.BTN_Draw.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Draw.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_Draw.ImageKey = "DrawButton.bmp";
            this.BTN_Draw.ImageList = this.imageListTools;
            this.BTN_Draw.Location = new System.Drawing.Point(36, 3);
            this.BTN_Draw.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Draw.Name = "BTN_Draw";
            this.BTN_Draw.Size = new System.Drawing.Size(24, 24);
            this.BTN_Draw.TabIndex = 10;
            this.toolTip1.SetToolTip(this.BTN_Draw, "Draw A Tile");
            this.BTN_Draw.UseVisualStyleBackColor = true;
            this.BTN_Draw.CheckStateChanged += new System.EventHandler(this.BTN_Toolbox_CheckedChanged);
            this.BTN_Draw.Click += new System.EventHandler(this.BTN_Draw_Click);
            // 
            // BTN_Select
            // 
            this.BTN_Select.Appearance = System.Windows.Forms.Appearance.Button;
            this.BTN_Select.Checked = true;
            this.BTN_Select.CheckState = System.Windows.Forms.CheckState.Checked;
            this.BTN_Select.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.BTN_Select.ImageKey = "SelectButton.bmp";
            this.BTN_Select.ImageList = this.imageListTools;
            this.BTN_Select.Location = new System.Drawing.Point(5, 3);
            this.BTN_Select.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.BTN_Select.Name = "BTN_Select";
            this.BTN_Select.Size = new System.Drawing.Size(24, 24);
            this.BTN_Select.TabIndex = 9;
            this.toolTip1.SetToolTip(this.BTN_Select, "Select A Tile");
            this.BTN_Select.UseVisualStyleBackColor = true;
            this.BTN_Select.CheckStateChanged += new System.EventHandler(this.BTN_Toolbox_CheckedChanged);
            this.BTN_Select.Click += new System.EventHandler(this.BTN_Select_Click);
            // 
            // numericUpDown_Floor
            // 
            this.numericUpDown_Floor.Location = new System.Drawing.Point(36, 32);
            this.numericUpDown_Floor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_Floor.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDown_Floor.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            -2147483648});
            this.numericUpDown_Floor.Name = "numericUpDown_Floor";
            this.numericUpDown_Floor.Size = new System.Drawing.Size(58, 23);
            this.numericUpDown_Floor.TabIndex = 8;
            this.numericUpDown_Floor.ValueChanged += new System.EventHandler(this.NumericUpDown_Floor_Changed);
            // 
            // numericUpDown_Z
            // 
            this.numericUpDown_Z.Location = new System.Drawing.Point(161, 6);
            this.numericUpDown_Z.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_Z.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDown_Z.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            -2147483648});
            this.numericUpDown_Z.Name = "numericUpDown_Z";
            this.numericUpDown_Z.Size = new System.Drawing.Size(58, 23);
            this.numericUpDown_Z.TabIndex = 5;
            // 
            // collapsibleSplitter1
            // 
            this.collapsibleSplitter1.AnimationDelay = 20;
            this.collapsibleSplitter1.AnimationStep = 20;
            this.collapsibleSplitter1.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
            this.collapsibleSplitter1.ControlToHide = this.Selectedpanel;
            this.collapsibleSplitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.collapsibleSplitter1.ExpandParentForm = false;
            this.collapsibleSplitter1.Location = new System.Drawing.Point(0, 96);
            this.collapsibleSplitter1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.collapsibleSplitter1.Name = "collapsibleSplitter1";
            this.collapsibleSplitter1.Size = new System.Drawing.Size(240, 8);
            this.collapsibleSplitter1.TabIndex = 5;
            this.collapsibleSplitter1.TabStop = false;
            this.toolTip1.SetToolTip(this.collapsibleSplitter1, "Selected Tile Panel");
            this.collapsibleSplitter1.UseAnimations = true;
            this.collapsibleSplitter1.VisualStyle = UoFiddler.Controls.UserControls.VisualStyles.DoubleDots;
            // 
            // Selectedpanel
            // 
            this.Selectedpanel.Controls.Add(this.groupBox3);
            this.Selectedpanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.Selectedpanel.Location = new System.Drawing.Point(0, 0);
            this.Selectedpanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Selectedpanel.Name = "Selectedpanel";
            this.Selectedpanel.Size = new System.Drawing.Size(240, 96);
            this.Selectedpanel.TabIndex = 6;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.DynamiccheckBox);
            this.groupBox3.Controls.Add(this.numericUpDown_Selected_Z);
            this.groupBox3.Controls.Add(this.numericUpDown_Selected_Y);
            this.groupBox3.Controls.Add(this.numericUpDown_Selected_X);
            this.groupBox3.Location = new System.Drawing.Point(13, 3);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox3.Size = new System.Drawing.Size(209, 87);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Selected Tile X,Y,Z";
            // 
            // DynamiccheckBox
            // 
            this.DynamiccheckBox.AutoSize = true;
            this.DynamiccheckBox.Location = new System.Drawing.Point(117, 54);
            this.DynamiccheckBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.DynamiccheckBox.Name = "DynamiccheckBox";
            this.DynamiccheckBox.Size = new System.Drawing.Size(69, 19);
            this.DynamiccheckBox.TabIndex = 3;
            this.DynamiccheckBox.Text = "Invisible";
            this.DynamiccheckBox.UseVisualStyleBackColor = true;
            this.DynamiccheckBox.CheckedChanged += new System.EventHandler(this.BTN_DynamicCheckBox_Changed);
            // 
            // numericUpDown_Selected_Z
            // 
            this.numericUpDown_Selected_Z.Location = new System.Drawing.Point(23, 53);
            this.numericUpDown_Selected_Z.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_Selected_Z.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.numericUpDown_Selected_Z.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            -2147483648});
            this.numericUpDown_Selected_Z.Name = "numericUpDown_Selected_Z";
            this.numericUpDown_Selected_Z.Size = new System.Drawing.Size(71, 23);
            this.numericUpDown_Selected_Z.TabIndex = 2;
            this.numericUpDown_Selected_Z.ValueChanged += new System.EventHandler(this.NumericUpDown_Selected_Z_Changed);
            // 
            // numericUpDown_Selected_Y
            // 
            this.numericUpDown_Selected_Y.Location = new System.Drawing.Point(117, 23);
            this.numericUpDown_Selected_Y.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_Selected_Y.Name = "numericUpDown_Selected_Y";
            this.numericUpDown_Selected_Y.Size = new System.Drawing.Size(71, 23);
            this.numericUpDown_Selected_Y.TabIndex = 1;
            this.numericUpDown_Selected_Y.ValueChanged += new System.EventHandler(this.NumericUpDown_Selected_Y_Changed);
            // 
            // numericUpDown_Selected_X
            // 
            this.numericUpDown_Selected_X.Location = new System.Drawing.Point(23, 23);
            this.numericUpDown_Selected_X.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.numericUpDown_Selected_X.Name = "numericUpDown_Selected_X";
            this.numericUpDown_Selected_X.Size = new System.Drawing.Size(71, 23);
            this.numericUpDown_Selected_X.TabIndex = 0;
            this.numericUpDown_Selected_X.ValueChanged += new System.EventHandler(this.NumericUpDown_Selected_X_Changed);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.MaxHeightTrackBar);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitter1);
            this.splitContainer2.Panel2.Controls.Add(this.pictureBoxMulti);
            this.splitContainer2.Panel2.Controls.Add(this.hScrollBar);
            this.splitContainer2.Panel2.Controls.Add(this.vScrollBar);
            this.splitContainer2.Size = new System.Drawing.Size(563, 455);
            this.splitContainer2.SplitterDistance = 30;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 2;
            // 
            // MaxHeightTrackBar
            // 
            this.MaxHeightTrackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MaxHeightTrackBar.Location = new System.Drawing.Point(0, 0);
            this.MaxHeightTrackBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaxHeightTrackBar.Name = "MaxHeightTrackBar";
            this.MaxHeightTrackBar.Size = new System.Drawing.Size(563, 30);
            this.MaxHeightTrackBar.TabIndex = 0;
            this.toolTip1.SetToolTip(this.MaxHeightTrackBar, "Max Height Displayed");
            this.MaxHeightTrackBar.ValueChanged += new System.EventHandler(this.MaxHeightTrackBarOnValueChanged);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 403);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // pictureBoxMulti
            // 
            this.pictureBoxMulti.BackColor = System.Drawing.Color.White;
            this.pictureBoxMulti.ContextMenuStrip = this.DummyContextMenu;
            this.pictureBoxMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxMulti.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxMulti.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBoxMulti.Name = "pictureBoxMulti";
            this.pictureBoxMulti.Size = new System.Drawing.Size(546, 403);
            this.pictureBoxMulti.TabIndex = 0;
            this.pictureBoxMulti.TabStop = false;
            this.pictureBoxMulti.SizeChanged += new System.EventHandler(this.PictureBoxMultiOnResize);
            this.pictureBoxMulti.Paint += new System.Windows.Forms.PaintEventHandler(this.PictureBoxMultiOnPaint);
            this.pictureBoxMulti.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMultiOnMouseDown);
            this.pictureBoxMulti.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMultiOnMouseMove);
            this.pictureBoxMulti.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxMultiOnMouseUp);
            // 
            // hScrollBar
            // 
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar.Location = new System.Drawing.Point(0, 403);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(546, 17);
            this.hScrollBar.TabIndex = 2;
            this.hScrollBar.ValueChanged += new System.EventHandler(this.ScrollBarsValueChanged);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(546, 0);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 420);
            this.vScrollBar.TabIndex = 1;
            this.vScrollBar.ValueChanged += new System.EventHandler(this.ScrollBarsValueChanged);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripSeparator2,
            this.DrawTileLabel,
            this.toolStripSeparator1,
            this.SelectedTileLabel,
            this.toolStripLabelCoord});
            this.toolStrip1.Location = new System.Drawing.Point(0, 455);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(563, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UndoItems,
            this.showWalkablesToolStripMenuItem,
            this.showDoubleSurfaceMenuItem,
            this.removeAllTransToolStripMenuItem});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            this.toolStripDropDownButton1.Text = "Misc";
            // 
            // UndoItems
            // 
            this.UndoItems.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UndoItem0,
            this.UndoItem1,
            this.UndoItem2,
            this.UndoItem3,
            this.UndoItem4,
            this.UndoItem5,
            this.UndoItem6,
            this.UndoItem7,
            this.UndoItem8,
            this.UndoItem9});
            this.UndoItems.Name = "UndoItems";
            this.UndoItems.Size = new System.Drawing.Size(189, 22);
            this.UndoItems.Text = "Undo";
            this.UndoItems.DropDownOpening += new System.EventHandler(this.UndoList_BeforeOpening);
            // 
            // UndoItem0
            // 
            this.UndoItem0.Name = "UndoItem0";
            this.UndoItem0.Size = new System.Drawing.Size(89, 22);
            this.UndoItem0.Tag = 0;
            this.UndoItem0.Text = "---";
            this.UndoItem0.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem1
            // 
            this.UndoItem1.Name = "UndoItem1";
            this.UndoItem1.Size = new System.Drawing.Size(89, 22);
            this.UndoItem1.Tag = 1;
            this.UndoItem1.Text = "---";
            this.UndoItem1.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem2
            // 
            this.UndoItem2.Name = "UndoItem2";
            this.UndoItem2.Size = new System.Drawing.Size(89, 22);
            this.UndoItem2.Tag = 2;
            this.UndoItem2.Text = "---";
            this.UndoItem2.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem3
            // 
            this.UndoItem3.Name = "UndoItem3";
            this.UndoItem3.Size = new System.Drawing.Size(89, 22);
            this.UndoItem3.Tag = 3;
            this.UndoItem3.Text = "---";
            this.UndoItem3.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem4
            // 
            this.UndoItem4.Name = "UndoItem4";
            this.UndoItem4.Size = new System.Drawing.Size(89, 22);
            this.UndoItem4.Tag = 4;
            this.UndoItem4.Text = "---";
            this.UndoItem4.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem5
            // 
            this.UndoItem5.Name = "UndoItem5";
            this.UndoItem5.Size = new System.Drawing.Size(89, 22);
            this.UndoItem5.Tag = 5;
            this.UndoItem5.Text = "---";
            this.UndoItem5.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem6
            // 
            this.UndoItem6.Name = "UndoItem6";
            this.UndoItem6.Size = new System.Drawing.Size(89, 22);
            this.UndoItem6.Tag = 6;
            this.UndoItem6.Text = "---";
            this.UndoItem6.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem7
            // 
            this.UndoItem7.Name = "UndoItem7";
            this.UndoItem7.Size = new System.Drawing.Size(89, 22);
            this.UndoItem7.Tag = 7;
            this.UndoItem7.Text = "---";
            this.UndoItem7.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem8
            // 
            this.UndoItem8.Name = "UndoItem8";
            this.UndoItem8.Size = new System.Drawing.Size(89, 22);
            this.UndoItem8.Tag = 8;
            this.UndoItem8.Text = "---";
            this.UndoItem8.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // UndoItem9
            // 
            this.UndoItem9.Name = "UndoItem9";
            this.UndoItem9.Size = new System.Drawing.Size(89, 22);
            this.UndoItem9.Tag = 9;
            this.UndoItem9.Text = "---";
            this.UndoItem9.Click += new System.EventHandler(this.Undo_onClick);
            // 
            // showWalkablesToolStripMenuItem
            // 
            this.showWalkablesToolStripMenuItem.CheckOnClick = true;
            this.showWalkablesToolStripMenuItem.Name = "showWalkablesToolStripMenuItem";
            this.showWalkablesToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.showWalkablesToolStripMenuItem.Text = "Show Walkable tiles";
            this.showWalkablesToolStripMenuItem.Click += new System.EventHandler(this.BTN_ShowWalkables_Click);
            // 
            // showDoubleSurfaceMenuItem
            // 
            this.showDoubleSurfaceMenuItem.CheckOnClick = true;
            this.showDoubleSurfaceMenuItem.Name = "showDoubleSurfaceMenuItem";
            this.showDoubleSurfaceMenuItem.Size = new System.Drawing.Size(189, 22);
            this.showDoubleSurfaceMenuItem.Text = "Show double surface";
            this.showDoubleSurfaceMenuItem.Click += new System.EventHandler(this.BTN_ShowDoubleSurface);
            // 
            // removeAllTransToolStripMenuItem
            // 
            this.removeAllTransToolStripMenuItem.Name = "removeAllTransToolStripMenuItem";
            this.removeAllTransToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.removeAllTransToolStripMenuItem.Text = "Reset transparent tiles";
            this.removeAllTransToolStripMenuItem.Click += new System.EventHandler(this.BTN_ShowAllTrans);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // DrawTileLabel
            // 
            this.DrawTileLabel.AutoSize = false;
            this.DrawTileLabel.Name = "DrawTileLabel";
            this.DrawTileLabel.Size = new System.Drawing.Size(100, 22);
            this.DrawTileLabel.Text = "Draw ID:";
            this.DrawTileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // SelectedTileLabel
            // 
            this.SelectedTileLabel.Name = "SelectedTileLabel";
            this.SelectedTileLabel.Size = new System.Drawing.Size(21, 22);
            this.SelectedTileLabel.Text = "ID:";
            // 
            // toolStripLabelCoord
            // 
            this.toolStripLabelCoord.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabelCoord.Name = "toolStripLabelCoord";
            this.toolStripLabelCoord.Size = new System.Drawing.Size(31, 22);
            this.toolStripLabelCoord.Text = "0,0,0";
            this.toolStripLabelCoord.ToolTipText = "Coordinates";
            // 
            // FloatingPreviewPanel
            // 
            this.FloatingPreviewPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.FloatingPreviewPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FloatingPreviewPanel.Location = new System.Drawing.Point(292, 77);
            this.FloatingPreviewPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.FloatingPreviewPanel.Name = "FloatingPreviewPanel";
            this.FloatingPreviewPanel.Size = new System.Drawing.Size(233, 115);
            this.FloatingPreviewPanel.TabIndex = 4;
            // 
            // MultiEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FloatingPreviewPanel);
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MultiEditorControl";
            this.Size = new System.Drawing.Size(808, 480);
            this.Load += new System.EventHandler(this.OnLoad);
            this.TC_MultiEditorToolbox.ResumeLayout(false);
            this.tileTab.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDrawTiles)).EndInit();
            this.designTab.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Size_Width)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Size_Height)).EndInit();
            this.importTab.ResumeLayout(false);
            this.Save.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Floor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Z)).EndInit();
            this.Selectedpanel.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Selected_Z)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Selected_Y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_Selected_X)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MaxHeightTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMulti)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Button BTN_CreateBlank;
        private System.Windows.Forms.CheckBox BTN_Draw;
        private System.Windows.Forms.Button BTN_Export_Txt;
        private System.Windows.Forms.Button BTN_Export_UOA;
        private System.Windows.Forms.Button BTN_Export_WSC;
        private System.Windows.Forms.CheckBox BTN_Floor;
        private System.Windows.Forms.CheckBox BTN_Pipette;
        private System.Windows.Forms.CheckBox BTN_Remove;
        private System.Windows.Forms.Button BTN_Resize;
        private System.Windows.Forms.Button BTN_Save;
        private System.Windows.Forms.CheckBox BTN_Select;
        private System.Windows.Forms.CheckBox BTN_Trans;
        private System.Windows.Forms.CheckBox BTN_Z;
        private UoFiddler.Controls.UserControls.CollapsibleSplitter collapsibleSplitter1;
        private System.Windows.Forms.TabPage designTab;
        private System.Windows.Forms.ToolStripLabel DrawTileLabel;
        private System.Windows.Forms.ContextMenuStrip DummyContextMenu;
        private System.Windows.Forms.CheckBox DynamiccheckBox;
        private System.Windows.Forms.Panel FloatingPreviewPanel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.ImageList imageListTools;
        private System.Windows.Forms.ImageList imageListTreeView;
        private System.Windows.Forms.TabPage importTab;
        private System.Windows.Forms.TrackBar MaxHeightTrackBar;
        private System.Windows.Forms.NumericUpDown numericUpDown_Floor;
        private System.Windows.Forms.NumericUpDown numericUpDown_Selected_X;
        private System.Windows.Forms.NumericUpDown numericUpDown_Selected_Y;
        private System.Windows.Forms.NumericUpDown numericUpDown_Selected_Z;
        private System.Windows.Forms.NumericUpDown numericUpDown_Size_Height;
        private System.Windows.Forms.NumericUpDown numericUpDown_Size_Width;
        private System.Windows.Forms.NumericUpDown numericUpDown_Z;
        private System.Windows.Forms.PictureBox pictureBoxDrawTiles;
        private System.Windows.Forms.PictureBox pictureBoxMulti;
        private System.Windows.Forms.ToolStripMenuItem removeAllTransToolStripMenuItem;
        private System.Windows.Forms.TabPage Save;
        private System.Windows.Forms.Panel Selectedpanel;
        private System.Windows.Forms.ToolStripLabel SelectedTileLabel;
        private System.Windows.Forms.ToolStripMenuItem showDoubleSurfaceMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showWalkablesToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.TabControl TC_MultiEditorToolbox;
        private System.Windows.Forms.TextBox textBox_Export;
        private System.Windows.Forms.TextBox textBox_SaveToID;
        private System.Windows.Forms.TabPage tileTab;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripLabel toolStripLabelCoord;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TreeView treeViewMultiList;
        private System.Windows.Forms.TreeView treeViewTilesXML;
        private System.Windows.Forms.ToolStripMenuItem UndoItem0;
        private System.Windows.Forms.ToolStripMenuItem UndoItem1;
        private System.Windows.Forms.ToolStripMenuItem UndoItem2;
        private System.Windows.Forms.ToolStripMenuItem UndoItem3;
        private System.Windows.Forms.ToolStripMenuItem UndoItem4;
        private System.Windows.Forms.ToolStripMenuItem UndoItem5;
        private System.Windows.Forms.ToolStripMenuItem UndoItem6;
        private System.Windows.Forms.ToolStripMenuItem UndoItem7;
        private System.Windows.Forms.ToolStripMenuItem UndoItem8;
        private System.Windows.Forms.ToolStripMenuItem UndoItem9;
        private System.Windows.Forms.ToolStripMenuItem UndoItems;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.VScrollBar vScrollBarDrawTiles;

        #endregion

        private System.Windows.Forms.Button BTN_Export_CSV;
        private System.Windows.Forms.Button BTN_Export_UOX3;
    }
}
