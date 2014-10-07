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
    partial class Map
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
            if (disposing)
            {
                
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
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.CoordsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ClientLocLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ZoomLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.zoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zoomToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.getMapInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertMarkerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.gotoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TextBoxGoto = new System.Windows.Forms.ToolStripTextBox();
            this.sendClientToPosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.feluccaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trammelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ilshenarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.malasToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tokunoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.terMurToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.extractMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.PreloadWorker = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.gotoToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchVisibleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.OverlayObjectTree = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.showStaticsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.showCenterCrossToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.showMarkersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showClientCrossToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.showClientLocToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.gotoClientLocToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.sendClientToCenterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.PreloadMap = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.defragStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defragAndRemoveDuplicatesStToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meltStaticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearStaticsinMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reportStaticsUnderMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.rewriteMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.insertDiffDataToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.replaceTilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.collapsibleSplitter2 = new FiddlerControls.CollapsibleSplitter();
            this.collapsibleSplitter1 = new FiddlerControls.CollapsibleSplitter();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CoordsLabel,
            this.ClientLocLabel,
            this.ZoomLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 302);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(619, 22);
            this.statusStrip.TabIndex = 0;
            this.statusStrip.Text = "statusStrip1";
            // 
            // CoordsLabel
            // 
            this.CoordsLabel.AutoSize = false;
            this.CoordsLabel.Name = "CoordsLabel";
            this.CoordsLabel.Size = new System.Drawing.Size(120, 17);
            this.CoordsLabel.Text = "Coords: 0,0";
            this.CoordsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ClientLocLabel
            // 
            this.ClientLocLabel.AutoSize = false;
            this.ClientLocLabel.Name = "ClientLocLabel";
            this.ClientLocLabel.Size = new System.Drawing.Size(200, 17);
            this.ClientLocLabel.Text = "ClientLoc: 0,0";
            this.ClientLocLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ZoomLabel
            // 
            this.ZoomLabel.AutoSize = false;
            this.ZoomLabel.Name = "ZoomLabel";
            this.ZoomLabel.Size = new System.Drawing.Size(100, 17);
            this.ZoomLabel.Text = "Zoom: ";
            this.ZoomLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox
            // 
            this.pictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 33);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(394, 252);
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.pictureBox.Resize += new System.EventHandler(this.OnResizeMap);
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zoomToolStripMenuItem,
            this.zoomToolStripMenuItem1,
            this.getMapInfoToolStripMenuItem,
            this.insertMarkerToolStripMenuItem,
            this.toolStripSeparator4,
            this.gotoToolStripMenuItem,
            this.sendClientToPosToolStripMenuItem,
            this.toolStripSeparator2,
            this.feluccaToolStripMenuItem,
            this.trammelToolStripMenuItem,
            this.ilshenarToolStripMenuItem,
            this.malasToolStripMenuItem,
            this.tokunoToolStripMenuItem,
            this.terMurToolStripMenuItem,
            this.toolStripSeparator1,
            this.extractMapToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(175, 330);
            this.contextMenuStrip1.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.onContextClosed);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.OnOpenContext);
            // 
            // zoomToolStripMenuItem
            // 
            this.zoomToolStripMenuItem.Name = "zoomToolStripMenuItem";
            this.zoomToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.zoomToolStripMenuItem.Text = "+Zoom";
            this.zoomToolStripMenuItem.Click += new System.EventHandler(this.OnZoomPlus);
            // 
            // zoomToolStripMenuItem1
            // 
            this.zoomToolStripMenuItem1.Name = "zoomToolStripMenuItem1";
            this.zoomToolStripMenuItem1.Size = new System.Drawing.Size(174, 22);
            this.zoomToolStripMenuItem1.Text = "-Zoom";
            this.zoomToolStripMenuItem1.Click += new System.EventHandler(this.OnZoomMinus);
            // 
            // getMapInfoToolStripMenuItem
            // 
            this.getMapInfoToolStripMenuItem.Name = "getMapInfoToolStripMenuItem";
            this.getMapInfoToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.getMapInfoToolStripMenuItem.Text = "GetMapInfo";
            this.getMapInfoToolStripMenuItem.Click += new System.EventHandler(this.GetMapInfo);
            // 
            // insertMarkerToolStripMenuItem
            // 
            this.insertMarkerToolStripMenuItem.Name = "insertMarkerToolStripMenuItem";
            this.insertMarkerToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.insertMarkerToolStripMenuItem.Text = "Insert Marker";
            this.insertMarkerToolStripMenuItem.Click += new System.EventHandler(this.OnClickInsertMarker);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(171, 6);
            // 
            // gotoToolStripMenuItem
            // 
            this.gotoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TextBoxGoto});
            this.gotoToolStripMenuItem.Name = "gotoToolStripMenuItem";
            this.gotoToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.gotoToolStripMenuItem.Text = "Goto...";
            this.gotoToolStripMenuItem.DropDownClosed += new System.EventHandler(this.OnDropDownClosed);
            // 
            // TextBoxGoto
            // 
            this.TextBoxGoto.Name = "TextBoxGoto";
            this.TextBoxGoto.Size = new System.Drawing.Size(100, 21);
            this.TextBoxGoto.KeyDown += new System.Windows.Forms.KeyEventHandler(this.onKeyDownGoto);
            // 
            // sendClientToPosToolStripMenuItem
            // 
            this.sendClientToPosToolStripMenuItem.Name = "sendClientToPosToolStripMenuItem";
            this.sendClientToPosToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.sendClientToPosToolStripMenuItem.Text = "Send Client To Pos";
            this.sendClientToPosToolStripMenuItem.Click += new System.EventHandler(this.onClickSendClientToPos);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(171, 6);
            // 
            // feluccaToolStripMenuItem
            // 
            this.feluccaToolStripMenuItem.Name = "feluccaToolStripMenuItem";
            this.feluccaToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.feluccaToolStripMenuItem.Text = "Felucca";
            this.feluccaToolStripMenuItem.Click += new System.EventHandler(this.ChangeMapFelucca);
            // 
            // trammelToolStripMenuItem
            // 
            this.trammelToolStripMenuItem.Name = "trammelToolStripMenuItem";
            this.trammelToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.trammelToolStripMenuItem.Text = "Trammel";
            this.trammelToolStripMenuItem.Click += new System.EventHandler(this.ChangeMapTrammel);
            // 
            // ilshenarToolStripMenuItem
            // 
            this.ilshenarToolStripMenuItem.Name = "ilshenarToolStripMenuItem";
            this.ilshenarToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.ilshenarToolStripMenuItem.Text = "Ilshenar";
            this.ilshenarToolStripMenuItem.Click += new System.EventHandler(this.ChangeMapIlshenar);
            // 
            // malasToolStripMenuItem
            // 
            this.malasToolStripMenuItem.Name = "malasToolStripMenuItem";
            this.malasToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.malasToolStripMenuItem.Text = "Malas";
            this.malasToolStripMenuItem.Click += new System.EventHandler(this.ChangeMapMalas);
            // 
            // tokunoToolStripMenuItem
            // 
            this.tokunoToolStripMenuItem.Name = "tokunoToolStripMenuItem";
            this.tokunoToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.tokunoToolStripMenuItem.Text = "Tokuno";
            this.tokunoToolStripMenuItem.Click += new System.EventHandler(this.ChangeMapTokuno);
            // 
            // terMurToolStripMenuItem
            // 
            this.terMurToolStripMenuItem.Name = "terMurToolStripMenuItem";
            this.terMurToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.terMurToolStripMenuItem.Text = "TerMur";
            this.terMurToolStripMenuItem.Click += new System.EventHandler(this.ChangeMapTerMur);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(171, 6);
            // 
            // extractMapToolStripMenuItem
            // 
            this.extractMapToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem});
            this.extractMapToolStripMenuItem.Name = "extractMapToolStripMenuItem";
            this.extractMapToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.extractMapToolStripMenuItem.Text = "Extract Map..";
            this.extractMapToolStripMenuItem.DropDownClosed += new System.EventHandler(this.OnDropDownClosed);
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.ExtractMapBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.ExtractMapTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.ExtractMapJpg);
            // 
            // hScrollBar
            // 
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar.Location = new System.Drawing.Point(0, 285);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(394, 17);
            this.hScrollBar.TabIndex = 2;
            this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HandleScroll);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(394, 33);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 269);
            this.vScrollBar.TabIndex = 3;
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HandleScroll);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.SyncClientTimer);
            // 
            // PreloadWorker
            // 
            this.PreloadWorker.WorkerReportsProgress = true;
            this.PreloadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.PreLoadDoWork);
            this.PreloadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.PreLoadCompleted);
            this.PreloadWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.PreLoadProgressChanged);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gotoToolStripMenuItem1,
            this.removeToolStripMenuItem,
            this.switchVisibleToolStripMenuItem});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(157, 70);
            this.contextMenuStrip2.Closed += new System.Windows.Forms.ToolStripDropDownClosedEventHandler(this.onContextClosed);
            // 
            // gotoToolStripMenuItem1
            // 
            this.gotoToolStripMenuItem1.Name = "gotoToolStripMenuItem1";
            this.gotoToolStripMenuItem1.Size = new System.Drawing.Size(156, 22);
            this.gotoToolStripMenuItem1.Text = "Goto";
            this.gotoToolStripMenuItem1.Click += new System.EventHandler(this.OnClickGotoMarker);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.OnClickRemoveMarker);
            // 
            // switchVisibleToolStripMenuItem
            // 
            this.switchVisibleToolStripMenuItem.Name = "switchVisibleToolStripMenuItem";
            this.switchVisibleToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.switchVisibleToolStripMenuItem.Text = "Switch Visibility";
            this.switchVisibleToolStripMenuItem.Click += new System.EventHandler(this.OnClickSwitchVisible);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.OverlayObjectTree);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(419, 33);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 269);
            this.panel1.TabIndex = 5;
            // 
            // OverlayObjectTree
            // 
            this.OverlayObjectTree.ContextMenuStrip = this.contextMenuStrip2;
            this.OverlayObjectTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OverlayObjectTree.Location = new System.Drawing.Point(0, 0);
            this.OverlayObjectTree.Name = "OverlayObjectTree";
            this.OverlayObjectTree.Size = new System.Drawing.Size(200, 269);
            this.OverlayObjectTree.TabIndex = 5;
            this.OverlayObjectTree.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.OnDoubleClickMarker);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton2,
            this.ProgressBar,
            this.PreloadMap,
            this.toolStripDropDownButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(619, 25);
            this.toolStrip1.TabIndex = 7;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showStaticsToolStripMenuItem1,
            this.showCenterCrossToolStripMenuItem1,
            this.showMarkersToolStripMenuItem,
            this.showClientCrossToolStripMenuItem});
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(42, 22);
            this.toolStripDropDownButton1.Text = "View";
            this.toolStripDropDownButton1.DropDownClosed += new System.EventHandler(this.OnDropDownClosed);
            // 
            // showStaticsToolStripMenuItem1
            // 
            this.showStaticsToolStripMenuItem1.Checked = true;
            this.showStaticsToolStripMenuItem1.CheckOnClick = true;
            this.showStaticsToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showStaticsToolStripMenuItem1.Name = "showStaticsToolStripMenuItem1";
            this.showStaticsToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.showStaticsToolStripMenuItem1.Text = "Show Statics";
            this.showStaticsToolStripMenuItem1.Click += new System.EventHandler(this.OnChangeView);
            // 
            // showCenterCrossToolStripMenuItem1
            // 
            this.showCenterCrossToolStripMenuItem1.Checked = true;
            this.showCenterCrossToolStripMenuItem1.CheckOnClick = true;
            this.showCenterCrossToolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showCenterCrossToolStripMenuItem1.Name = "showCenterCrossToolStripMenuItem1";
            this.showCenterCrossToolStripMenuItem1.Size = new System.Drawing.Size(177, 22);
            this.showCenterCrossToolStripMenuItem1.Text = "Show Center Cross";
            this.showCenterCrossToolStripMenuItem1.Click += new System.EventHandler(this.OnChangeView);
            // 
            // showMarkersToolStripMenuItem
            // 
            this.showMarkersToolStripMenuItem.Checked = true;
            this.showMarkersToolStripMenuItem.CheckOnClick = true;
            this.showMarkersToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showMarkersToolStripMenuItem.Name = "showMarkersToolStripMenuItem";
            this.showMarkersToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.showMarkersToolStripMenuItem.Text = "Show Markers";
            this.showMarkersToolStripMenuItem.Click += new System.EventHandler(this.OnChangeView);
            // 
            // showClientCrossToolStripMenuItem
            // 
            this.showClientCrossToolStripMenuItem.Checked = true;
            this.showClientCrossToolStripMenuItem.CheckOnClick = true;
            this.showClientCrossToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showClientCrossToolStripMenuItem.Name = "showClientCrossToolStripMenuItem";
            this.showClientCrossToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.showClientCrossToolStripMenuItem.Text = "Show Client Cross";
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showClientLocToolStripMenuItem1,
            this.toolStripSeparator5,
            this.gotoClientLocToolStripMenuItem1,
            this.sendClientToCenterToolStripMenuItem});
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(89, 22);
            this.toolStripDropDownButton2.Text = "Client Interact";
            this.toolStripDropDownButton2.DropDownClosed += new System.EventHandler(this.OnDropDownClosed);
            // 
            // showClientLocToolStripMenuItem1
            // 
            this.showClientLocToolStripMenuItem1.CheckOnClick = true;
            this.showClientLocToolStripMenuItem1.Name = "showClientLocToolStripMenuItem1";
            this.showClientLocToolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
            this.showClientLocToolStripMenuItem1.Text = "Show Client Loc";
            this.showClientLocToolStripMenuItem1.Click += new System.EventHandler(this.onClick_ShowClientLoc);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(185, 6);
            // 
            // gotoClientLocToolStripMenuItem1
            // 
            this.gotoClientLocToolStripMenuItem1.Name = "gotoClientLocToolStripMenuItem1";
            this.gotoClientLocToolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
            this.gotoClientLocToolStripMenuItem1.Text = "Goto Client Loc";
            this.gotoClientLocToolStripMenuItem1.Click += new System.EventHandler(this.onClick_GotoClientLoc);
            // 
            // sendClientToCenterToolStripMenuItem
            // 
            this.sendClientToCenterToolStripMenuItem.Name = "sendClientToCenterToolStripMenuItem";
            this.sendClientToCenterToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.sendClientToCenterToolStripMenuItem.Text = "Send Client to Center";
            this.sendClientToCenterToolStripMenuItem.Click += new System.EventHandler(this.onClickSendClient);
            // 
            // ProgressBar
            // 
            this.ProgressBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(100, 22);
            // 
            // PreloadMap
            // 
            this.PreloadMap.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.PreloadMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.PreloadMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.PreloadMap.Name = "PreloadMap";
            this.PreloadMap.Size = new System.Drawing.Size(70, 22);
            this.PreloadMap.Text = "Preload Map";
            this.PreloadMap.Click += new System.EventHandler(this.OnClickPreloadMap);
            // 
            // toolStripDropDownButton3
            // 
            this.toolStripDropDownButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defragStaticsToolStripMenuItem,
            this.defragAndRemoveDuplicatesStToolStripMenuItem,
            this.importStaticsToolStripMenuItem,
            this.meltStaticsToolStripMenuItem,
            this.clearStaticsinMemoryToolStripMenuItem,
            this.reportStaticsUnderMapToolStripMenuItem,
            this.toolStripMenuItem1,
            this.toolStripSeparator3,
            this.rewriteMapToolStripMenuItem,
            this.toolStripSeparator6,
            this.copyToolStripMenuItem,
            this.insertDiffDataToolStripMenuItem,
            this.toolStripSeparator7,
            this.replaceTilesToolStripMenuItem});
            this.toolStripDropDownButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            this.toolStripDropDownButton3.Size = new System.Drawing.Size(40, 22);
            this.toolStripDropDownButton3.Text = "Misc";
            this.toolStripDropDownButton3.DropDownClosed += new System.EventHandler(this.OnDropDownClosed);
            // 
            // defragStaticsToolStripMenuItem
            // 
            this.defragStaticsToolStripMenuItem.Name = "defragStaticsToolStripMenuItem";
            this.defragStaticsToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.defragStaticsToolStripMenuItem.Text = "Defrag Statics";
            this.defragStaticsToolStripMenuItem.Click += new System.EventHandler(this.OnClickDefragStatics);
            // 
            // defragAndRemoveDuplicatesStToolStripMenuItem
            // 
            this.defragAndRemoveDuplicatesStToolStripMenuItem.Name = "defragAndRemoveDuplicatesStToolStripMenuItem";
            this.defragAndRemoveDuplicatesStToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.defragAndRemoveDuplicatesStToolStripMenuItem.Text = "Defrag and Remove Duplicates Statics";
            this.defragAndRemoveDuplicatesStToolStripMenuItem.Click += new System.EventHandler(this.OnClickDefragRemoveStatics);
            // 
            // importStaticsToolStripMenuItem
            // 
            this.importStaticsToolStripMenuItem.Name = "importStaticsToolStripMenuItem";
            this.importStaticsToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.importStaticsToolStripMenuItem.Text = "Freeze Statics.. (in Memory)";
            this.importStaticsToolStripMenuItem.Click += new System.EventHandler(this.OnClickStaticImport);
            // 
            // meltStaticsToolStripMenuItem
            // 
            this.meltStaticsToolStripMenuItem.Name = "meltStaticsToolStripMenuItem";
            this.meltStaticsToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.meltStaticsToolStripMenuItem.Text = "Melt Statics.. (in Memory)";
            this.meltStaticsToolStripMenuItem.ToolTipText = "Clears a block of statics from memory. Also generates an Export File of the items" +
                " removed.";
            this.meltStaticsToolStripMenuItem.Click += new System.EventHandler(this.OnClickMeltStatics);
            // 
            // clearStaticsinMemoryToolStripMenuItem
            // 
            this.clearStaticsinMemoryToolStripMenuItem.Name = "clearStaticsinMemoryToolStripMenuItem";
            this.clearStaticsinMemoryToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.clearStaticsinMemoryToolStripMenuItem.Text = "Clear Statics..(in Memory)";
            this.clearStaticsinMemoryToolStripMenuItem.ToolTipText = "Clears a block of statics from memory. Unlike the Melt Statics, this does not cre" +
                "ate an export file of the static items removed.";
            this.clearStaticsinMemoryToolStripMenuItem.Click += new System.EventHandler(this.OnClickClearStatics);
            // 
            // reportStaticsUnderMapToolStripMenuItem
            // 
            this.reportStaticsUnderMapToolStripMenuItem.Name = "reportStaticsUnderMapToolStripMenuItem";
            this.reportStaticsUnderMapToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.reportStaticsUnderMapToolStripMenuItem.Text = "Report Statics below Map (possible invisible)";
            this.reportStaticsUnderMapToolStripMenuItem.Click += new System.EventHandler(this.OnClickReportInvisStatics);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(296, 22);
            this.toolStripMenuItem1.Text = "Report Invalid Map IDs";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.OnClickReportInvalidMapIDs);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(293, 6);
            // 
            // rewriteMapToolStripMenuItem
            // 
            this.rewriteMapToolStripMenuItem.Name = "rewriteMapToolStripMenuItem";
            this.rewriteMapToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.rewriteMapToolStripMenuItem.Text = "Rewrite Map";
            this.rewriteMapToolStripMenuItem.Click += new System.EventHandler(this.OnClickRewriteMap);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(293, 6);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.copyToolStripMenuItem.Text = "Map and Statics Copy...";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // insertDiffDataToolStripMenuItem
            // 
            this.insertDiffDataToolStripMenuItem.Name = "insertDiffDataToolStripMenuItem";
            this.insertDiffDataToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.insertDiffDataToolStripMenuItem.Text = "Diff to Map Copy...";
            this.insertDiffDataToolStripMenuItem.Click += new System.EventHandler(this.OnClickInsertDiffData);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(293, 6);
            // 
            // replaceTilesToolStripMenuItem
            // 
            this.replaceTilesToolStripMenuItem.Name = "replaceTilesToolStripMenuItem";
            this.replaceTilesToolStripMenuItem.Size = new System.Drawing.Size(296, 22);
            this.replaceTilesToolStripMenuItem.Text = "Replace Tiles..";
            this.replaceTilesToolStripMenuItem.Click += new System.EventHandler(this.OnClickReplaceTiles);
            // 
            // collapsibleSplitter2
            // 
            this.collapsibleSplitter2.AnimationDelay = 20;
            this.collapsibleSplitter2.AnimationStep = 20;
            this.collapsibleSplitter2.BorderStyle3D = System.Windows.Forms.Border3DStyle.Flat;
            this.collapsibleSplitter2.ControlToHide = this.panel1;
            this.collapsibleSplitter2.Dock = System.Windows.Forms.DockStyle.Right;
            this.collapsibleSplitter2.ExpandParentForm = false;
            this.collapsibleSplitter2.Location = new System.Drawing.Point(411, 33);
            this.collapsibleSplitter2.Name = "collapsibleSplitter2";
            this.collapsibleSplitter2.TabIndex = 8;
            this.collapsibleSplitter2.TabStop = false;
            this.toolTip1.SetToolTip(this.collapsibleSplitter2, "Click to Show/Hide Marker list");
            this.collapsibleSplitter2.UseAnimations = true;
            this.collapsibleSplitter2.VisualStyle = FiddlerControls.VisualStyles.DoubleDots;
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
            this.collapsibleSplitter1.TabIndex = 6;
            this.collapsibleSplitter1.TabStop = false;
            this.toolTip1.SetToolTip(this.collapsibleSplitter1, "Click To Show/Hide Toolbar");
            this.collapsibleSplitter1.UseAnimations = false;
            this.collapsibleSplitter1.VisualStyle = FiddlerControls.VisualStyles.DoubleDots;
            // 
            // Map
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.hScrollBar);
            this.Controls.Add(this.vScrollBar);
            this.Controls.Add(this.collapsibleSplitter2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.collapsibleSplitter1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "Map";
            this.Size = new System.Drawing.Size(619, 324);
            this.Load += new System.EventHandler(this.OnLoad);
            this.SizeChanged += new System.EventHandler(this.OnResize);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem feluccaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem trammelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem malasToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ilshenarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tokunoToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem extractMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel CoordsLabel;
        private System.Windows.Forms.ToolStripStatusLabel ClientLocLabel;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripMenuItem getMapInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zoomToolStripMenuItem1;
        private System.Windows.Forms.ToolStripStatusLabel ZoomLabel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.ComponentModel.BackgroundWorker PreloadWorker;
        private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox TextBoxGoto;
        private System.Windows.Forms.ToolStripMenuItem sendClientToPosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem gotoToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.TreeView OverlayObjectTree;
        private System.Windows.Forms.ToolStripMenuItem switchVisibleToolStripMenuItem;
        private FiddlerControls.CollapsibleSplitter collapsibleSplitter1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem showStaticsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem showCenterCrossToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem showMarkersToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem showClientLocToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem gotoClientLocToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem sendClientToCenterToolStripMenuItem;
        private System.Windows.Forms.ToolStripProgressBar ProgressBar;
        private System.Windows.Forms.ToolStripButton PreloadMap;
        private System.Windows.Forms.ToolStripMenuItem showClientCrossToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton3;
        private System.Windows.Forms.ToolStripMenuItem defragStaticsToolStripMenuItem;
        private FiddlerControls.CollapsibleSplitter collapsibleSplitter2;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem defragAndRemoveDuplicatesStToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rewriteMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reportStaticsUnderMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem insertDiffDataToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem meltStaticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem clearStaticsinMemoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem replaceTilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem terMurToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertMarkerToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}
