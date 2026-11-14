namespace UoFiddler.Plugin.MultiEditor.UI
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
            components = new System.ComponentModel.Container();
            _hostMenuBlocker = new System.Windows.Forms.ContextMenuStrip(components);
            _centre = new System.Windows.Forms.Panel();
            _emptyHint = new System.Windows.Forms.Label();
            _canvas = new UoFiddler.Plugin.MultiEditor.Rendering.GlCanvas();
            _docTabs = new System.Windows.Forms.TabControl();
            _toolPalette = new System.Windows.Forms.ToolStrip();
            _editToolsLabel = new System.Windows.Forms.ToolStripLabel();
            _drawToolsLabel = new System.Windows.Forms.ToolStripLabel();
            _houseToolsLabel = new System.Windows.Forms.ToolStripLabel();
            _sideSplitter = new UoFiddler.Plugin.MultiEditor.UI.GripSplitter();
            _sidePanel = new System.Windows.Forms.Panel();
            _sideSplit = new System.Windows.Forms.SplitContainer();
            _browser = new UoFiddler.Plugin.MultiEditor.UI.TileBrowserPanel();
            _inspector = new UoFiddler.Plugin.MultiEditor.UI.InspectorPanel();
            _optionsStrip = new System.Windows.Forms.ToolStrip();
            _toolLabel = new System.Windows.Forms.ToolStripLabel();
            _optionsSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _placeLabel = new System.Windows.Forms.ToolStripLabel();
            _placementCombo = new UoFiddler.Plugin.MultiEditor.UI.ToolStripChoice(110, "On top", "Replace", "At plane Z");
            _brushLabel = new System.Windows.Forms.ToolStripLabel();
            _zStepLabel = new System.Windows.Forms.ToolStripLabel();
            _zStepCombo = new UoFiddler.Plugin.MultiEditor.UI.ToolStripChoice(50, "1", "2", "3", "4", "5", "10", "20");
            _optionsSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            _floorLabel = new System.Windows.Forms.ToolStripLabel();
            _optionsSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            _drawTileLabel = new System.Windows.Forms.ToolStripLabel();
            _ridgeSeparator = new System.Windows.Forms.ToolStripSeparator();
            _ridgeLabel = new System.Windows.Forms.ToolStripLabel();
            _gablesSeparator = new System.Windows.Forms.ToolStripSeparator();
            _gablesLabel = new System.Windows.Forms.ToolStripLabel();
            _gableFillSeparator = new System.Windows.Forms.ToolStripSeparator();
            _gableFillLabel = new System.Windows.Forms.ToolStripLabel();
            _mainStrip = new System.Windows.Forms.ToolStrip();
            _fileMenu = new System.Windows.Forms.ToolStripDropDownButton();
            _editMenu = new System.Windows.Forms.ToolStripDropDownButton();
            _selectMenu = new System.Windows.Forms.ToolStripDropDownButton();
            _viewMenu = new System.Windows.Forms.ToolStripDropDownButton();
            _mainSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            _mainSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            _mainSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            _mainSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            _zoomLabel = new System.Windows.Forms.ToolStripLabel();
            _statusStrip = new System.Windows.Forms.StatusStrip();
            _statusHint = new System.Windows.Forms.ToolStripStatusLabel();
            _statusCell = new System.Windows.Forms.ToolStripStatusLabel();
            _statusHover = new System.Windows.Forms.ToolStripStatusLabel();
            _statusSelection = new System.Windows.Forms.ToolStripStatusLabel();
            _statusSize = new System.Windows.Forms.ToolStripStatusLabel();
            _centre.SuspendLayout();
            _toolPalette.SuspendLayout();
            _sidePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)_sideSplit).BeginInit();
            _sideSplit.Panel1.SuspendLayout();
            _sideSplit.Panel2.SuspendLayout();
            _sideSplit.SuspendLayout();
            _optionsStrip.SuspendLayout();
            _mainStrip.SuspendLayout();
            _statusStrip.SuspendLayout();
            SuspendLayout();
            //
            // _hostMenuBlocker
            //
            _hostMenuBlocker.Name = "_hostMenuBlocker";
            _hostMenuBlocker.Size = new System.Drawing.Size(61, 4);
            _hostMenuBlocker.Opening += HostMenuBlocker_Opening;
            //
            // _centre
            //
            _centre.Controls.Add(_emptyHint);
            _centre.Controls.Add(_canvas);
            _centre.Controls.Add(_docTabs);
            _centre.Dock = System.Windows.Forms.DockStyle.Fill;
            _centre.Location = new System.Drawing.Point(96, 50);
            _centre.Name = "_centre";
            _centre.Size = new System.Drawing.Size(532, 646);
            _centre.TabIndex = 0;
            //
            // _emptyHint
            //
            _emptyHint.Dock = System.Windows.Forms.DockStyle.Fill;
            _emptyHint.Location = new System.Drawing.Point(0, 24);
            _emptyHint.Name = "_emptyHint";
            _emptyHint.Size = new System.Drawing.Size(532, 622);
            _emptyHint.TabIndex = 1;
            _emptyHint.Text = "Open a multi from the client (Ctrl+O), import files (Ctrl+I),\r\nimport from a folder (Ctrl+Shift+I) or start a new one (Ctrl+N).";
            _emptyHint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // _canvas
            //
            _canvas.Dock = System.Windows.Forms.DockStyle.Fill;
            _canvas.Location = new System.Drawing.Point(0, 24);
            _canvas.Name = "_canvas";
            _canvas.Size = new System.Drawing.Size(532, 622);
            _canvas.TabIndex = 0;
            _canvas.ContextFailed += ShowMessage;
            _canvas.CameraChanged += UpdateZoomLabel;
            _canvas.ContextRequested += ShowCanvasMenu;
            //
            // _docTabs
            //
            _docTabs.Dock = System.Windows.Forms.DockStyle.Top;
            _docTabs.Location = new System.Drawing.Point(0, 0);
            _docTabs.Name = "_docTabs";
            _docTabs.ShowToolTips = true;
            _docTabs.Size = new System.Drawing.Size(532, 24);
            _docTabs.TabIndex = 2;
            _docTabs.SelectedIndexChanged += DocTabs_SelectedIndexChanged;
            _docTabs.MouseUp += DocTabs_MouseUp;
            //
            // _toolPalette
            //
            _toolPalette.AutoSize = false;
            _toolPalette.Dock = System.Windows.Forms.DockStyle.Left;
            _toolPalette.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _toolPalette.ImageScalingSize = new System.Drawing.Size(20, 20);
            _toolPalette.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _editToolsLabel, _drawToolsLabel, _houseToolsLabel });
            _toolPalette.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            _toolPalette.Location = new System.Drawing.Point(0, 50);
            _toolPalette.Name = "_toolPalette";
            _toolPalette.Size = new System.Drawing.Size(96, 646);
            _toolPalette.TabIndex = 1;
            //
            // _editToolsLabel
            //
            _editToolsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            _editToolsLabel.Margin = new System.Windows.Forms.Padding(2, 6, 0, 2);
            _editToolsLabel.Name = "_editToolsLabel";
            _editToolsLabel.Size = new System.Drawing.Size(93, 15);
            _editToolsLabel.Text = "Edit";
            //
            // _drawToolsLabel
            //
            _drawToolsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            _drawToolsLabel.Margin = new System.Windows.Forms.Padding(2, 6, 0, 2);
            _drawToolsLabel.Name = "_drawToolsLabel";
            _drawToolsLabel.Size = new System.Drawing.Size(93, 15);
            _drawToolsLabel.Text = "Draw";
            //
            // _houseToolsLabel
            //
            _houseToolsLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            _houseToolsLabel.Margin = new System.Windows.Forms.Padding(2, 6, 0, 2);
            _houseToolsLabel.Name = "_houseToolsLabel";
            _houseToolsLabel.Size = new System.Drawing.Size(93, 15);
            _houseToolsLabel.Text = "House";
            //
            // _sideSplitter
            //
            _sideSplitter.Dock = System.Windows.Forms.DockStyle.Right;
            _sideSplitter.Location = new System.Drawing.Point(628, 50);
            _sideSplitter.MinExtra = 200;
            _sideSplitter.MinSize = 240;
            _sideSplitter.Name = "_sideSplitter";
            _sideSplitter.Size = new System.Drawing.Size(7, 646);
            _sideSplitter.TabIndex = 2;
            _sideSplitter.TabStop = false;
            _sideSplitter.SplitterMoved += SideSplitter_SplitterMoved;
            //
            // _sidePanel
            //
            _sidePanel.Controls.Add(_sideSplit);
            _sidePanel.Dock = System.Windows.Forms.DockStyle.Right;
            _sidePanel.Location = new System.Drawing.Point(635, 50);
            _sidePanel.Name = "_sidePanel";
            _sidePanel.Size = new System.Drawing.Size(645, 646);
            _sidePanel.TabIndex = 3;
            //
            // _sideSplit
            //
            _sideSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            _sideSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            _sideSplit.Location = new System.Drawing.Point(0, 0);
            _sideSplit.Name = "_sideSplit";
            //
            // _sideSplit.Panel1
            //
            _sideSplit.Panel1.Controls.Add(_browser);
            //
            // _sideSplit.Panel2
            //
            _sideSplit.Panel2.Controls.Add(_inspector);
            _sideSplit.Size = new System.Drawing.Size(645, 646);
            _sideSplit.SplitterDistance = 545;
            _sideSplit.SplitterWidth = 5;
            _sideSplit.TabIndex = 0;
            _sideSplit.SplitterMoved += SideSplit_SplitterMoved;
            //
            // _browser
            //
            _browser.Dock = System.Windows.Forms.DockStyle.Fill;
            _browser.Location = new System.Drawing.Point(0, 0);
            _browser.Name = "_browser";
            _browser.Size = new System.Drawing.Size(545, 646);
            _browser.TabIndex = 0;
            //
            // _inspector
            //
            _inspector.Dock = System.Windows.Forms.DockStyle.Fill;
            _inspector.Location = new System.Drawing.Point(0, 0);
            _inspector.Name = "_inspector";
            _inspector.Size = new System.Drawing.Size(95, 646);
            _inspector.TabIndex = 0;
            //
            // _optionsStrip
            //
            _optionsStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _optionsStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _toolLabel, _optionsSeparator1, _placeLabel, _placementCombo, _brushLabel, _zStepLabel, _zStepCombo, _optionsSeparator2, _floorLabel, _optionsSeparator3, _drawTileLabel, _ridgeSeparator, _ridgeLabel, _gablesSeparator, _gablesLabel, _gableFillSeparator, _gableFillLabel });
            _optionsStrip.Location = new System.Drawing.Point(0, 25);
            _optionsStrip.Name = "_optionsStrip";
            _optionsStrip.Size = new System.Drawing.Size(1280, 25);
            _optionsStrip.TabIndex = 4;
            //
            // _toolLabel
            //
            _toolLabel.AutoSize = false;
            _toolLabel.Name = "_toolLabel";
            _toolLabel.Size = new System.Drawing.Size(110, 23);
            _toolLabel.Text = "Select";
            _toolLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _optionsSeparator1
            //
            _optionsSeparator1.Name = "_optionsSeparator1";
            _optionsSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // _placeLabel
            //
            _placeLabel.Name = "_placeLabel";
            _placeLabel.Size = new System.Drawing.Size(35, 22);
            _placeLabel.Text = "Place";
            //
            // _placementCombo
            //
            _placementCombo.Name = "_placementCombo";
            _placementCombo.ToolTipText = "Where new tiles go: on top of the tile under the cursor, replacing the tile at its Z, or on the work plane (M)";
            _placementCombo.SelectedIndexChanged += PlacementCombo_SelectedIndexChanged;
            //
            // _brushLabel
            //
            _brushLabel.Name = "_brushLabel";
            _brushLabel.Size = new System.Drawing.Size(38, 22);
            _brushLabel.Text = "Brush";
            _brushLabel.ToolTipText = "Brush size of the Draw tool (Ctrl+1, Ctrl+2, Ctrl+3)";
            //
            // _zStepLabel
            //
            _zStepLabel.Name = "_zStepLabel";
            _zStepLabel.Size = new System.Drawing.Size(42, 22);
            _zStepLabel.Text = "Z step";
            //
            // _zStepCombo
            //
            _zStepCombo.Name = "_zStepCombo";
            _zStepCombo.ToolTipText = "Z added by the Altitude tool per click";
            _zStepCombo.SelectedIndexChanged += ZStepCombo_SelectedIndexChanged;
            //
            // _optionsSeparator2
            //
            _optionsSeparator2.Name = "_optionsSeparator2";
            _optionsSeparator2.Size = new System.Drawing.Size(6, 25);
            //
            // _floorLabel
            //
            _floorLabel.Name = "_floorLabel";
            _floorLabel.Size = new System.Drawing.Size(34, 22);
            _floorLabel.Text = "Floor";
            _floorLabel.ToolTipText = "Floor being built: PgUp / PgDn or Shift+wheel, Ctrl+Home for all";
            //
            // _optionsSeparator3
            //
            _optionsSeparator3.Name = "_optionsSeparator3";
            _optionsSeparator3.Size = new System.Drawing.Size(6, 25);
            //
            // _drawTileLabel
            //
            _drawTileLabel.AutoSize = false;
            _drawTileLabel.Name = "_drawTileLabel";
            _drawTileLabel.Size = new System.Drawing.Size(230, 23);
            _drawTileLabel.Text = "Tile: none";
            _drawTileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _ridgeSeparator
            //
            _ridgeSeparator.Name = "_ridgeSeparator";
            _ridgeSeparator.Size = new System.Drawing.Size(6, 25);
            //
            // _ridgeLabel
            //
            _ridgeLabel.Name = "_ridgeLabel";
            _ridgeLabel.Size = new System.Drawing.Size(36, 22);
            _ridgeLabel.Text = "Ridge";
            _ridgeLabel.ToolTipText = "Which way the ridge runs. Shift turns it for one stroke.";
            //
            // _gablesSeparator
            //
            _gablesSeparator.Name = "_gablesSeparator";
            _gablesSeparator.Size = new System.Drawing.Size(6, 25);
            //
            // _gablesLabel
            //
            _gablesLabel.Name = "_gablesLabel";
            _gablesLabel.Size = new System.Drawing.Size(41, 22);
            _gablesLabel.Text = "Gables";
            _gablesLabel.ToolTipText = "Which gable ends are closed. Ctrl switches between front and both for one stroke.";
            //
            // _gableFillSeparator
            //
            _gableFillSeparator.Name = "_gableFillSeparator";
            _gableFillSeparator.Size = new System.Drawing.Size(6, 25);
            //
            // _gableFillLabel
            //
            _gableFillLabel.Name = "_gableFillLabel";
            _gableFillLabel.Size = new System.Drawing.Size(23, 22);
            _gableFillLabel.Text = "Fill";
            _gableFillLabel.ToolTipText = "What closes a gable: small cap pieces, or whole wall pieces with caps on top (for big, high roofs).";
            //
            // _mainStrip
            //
            _mainStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _mainStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _fileMenu, _editMenu, _selectMenu, _viewMenu, _mainSeparator1, _mainSeparator2, _mainSeparator3, _mainSeparator4, _zoomLabel });
            _mainStrip.Location = new System.Drawing.Point(0, 0);
            _mainStrip.Name = "_mainStrip";
            _mainStrip.Size = new System.Drawing.Size(1280, 25);
            _mainStrip.TabIndex = 5;
            //
            // _fileMenu
            //
            _fileMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _fileMenu.Name = "_fileMenu";
            _fileMenu.Size = new System.Drawing.Size(38, 22);
            _fileMenu.Text = "File";
            _fileMenu.DropDownOpening += Menu_DropDownOpening;
            //
            // _editMenu
            //
            _editMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _editMenu.Name = "_editMenu";
            _editMenu.Size = new System.Drawing.Size(40, 22);
            _editMenu.Text = "Edit";
            _editMenu.DropDownOpening += Menu_DropDownOpening;
            //
            // _selectMenu
            //
            _selectMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _selectMenu.Name = "_selectMenu";
            _selectMenu.Size = new System.Drawing.Size(51, 22);
            _selectMenu.Text = "Select";
            _selectMenu.DropDownOpening += Menu_DropDownOpening;
            //
            // _viewMenu
            //
            _viewMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _viewMenu.Name = "_viewMenu";
            _viewMenu.Size = new System.Drawing.Size(45, 22);
            _viewMenu.Text = "View";
            _viewMenu.DropDownOpening += Menu_DropDownOpening;
            //
            // _mainSeparator1
            //
            _mainSeparator1.Name = "_mainSeparator1";
            _mainSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // _mainSeparator2
            //
            _mainSeparator2.Name = "_mainSeparator2";
            _mainSeparator2.Size = new System.Drawing.Size(6, 25);
            //
            // _mainSeparator3
            //
            _mainSeparator3.Name = "_mainSeparator3";
            _mainSeparator3.Size = new System.Drawing.Size(6, 25);
            //
            // _mainSeparator4
            //
            _mainSeparator4.Name = "_mainSeparator4";
            _mainSeparator4.Size = new System.Drawing.Size(6, 25);
            //
            // _zoomLabel
            //
            _zoomLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            _zoomLabel.AutoSize = false;
            _zoomLabel.Name = "_zoomLabel";
            _zoomLabel.Size = new System.Drawing.Size(48, 23);
            _zoomLabel.Text = "100%";
            //
            // _statusStrip
            //
            _statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _statusHint, _statusCell, _statusHover, _statusSelection, _statusSize });
            _statusStrip.Location = new System.Drawing.Point(0, 696);
            _statusStrip.Name = "_statusStrip";
            _statusStrip.SizingGrip = false;
            _statusStrip.Size = new System.Drawing.Size(1280, 24);
            _statusStrip.TabIndex = 6;
            //
            // _statusHint
            //
            _statusHint.Name = "_statusHint";
            _statusHint.Size = new System.Drawing.Size(605, 19);
            _statusHint.Spring = true;
            _statusHint.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _statusCell
            //
            _statusCell.AutoSize = false;
            _statusCell.Name = "_statusCell";
            _statusCell.Size = new System.Drawing.Size(110, 23);
            _statusCell.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _statusHover
            //
            _statusHover.AutoSize = false;
            _statusHover.Name = "_statusHover";
            _statusHover.Size = new System.Drawing.Size(280, 23);
            _statusHover.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _statusSelection
            //
            _statusSelection.AutoSize = false;
            _statusSelection.Name = "_statusSelection";
            _statusSelection.Size = new System.Drawing.Size(120, 23);
            _statusSelection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _statusSize
            //
            _statusSize.AutoSize = false;
            _statusSize.Name = "_statusSize";
            _statusSize.Size = new System.Drawing.Size(150, 23);
            _statusSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // MultiEditorControl
            //
            ContextMenuStrip = _hostMenuBlocker;
            Controls.Add(_centre);
            Controls.Add(_toolPalette);
            Controls.Add(_sideSplitter);
            Controls.Add(_sidePanel);
            Controls.Add(_optionsStrip);
            Controls.Add(_mainStrip);
            Controls.Add(_statusStrip);
            Name = "MultiEditorControl";
            Size = new System.Drawing.Size(1280, 720);
            _centre.ResumeLayout(false);
            _toolPalette.ResumeLayout(false);
            _toolPalette.PerformLayout();
            _sidePanel.ResumeLayout(false);
            _sideSplit.Panel1.ResumeLayout(false);
            _sideSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)_sideSplit).EndInit();
            _sideSplit.ResumeLayout(false);
            _optionsStrip.ResumeLayout(false);
            _optionsStrip.PerformLayout();
            _mainStrip.ResumeLayout(false);
            _mainStrip.PerformLayout();
            _statusStrip.ResumeLayout(false);
            _statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip _hostMenuBlocker;
        private System.Windows.Forms.Panel _centre;
        private System.Windows.Forms.Label _emptyHint;
        private UoFiddler.Plugin.MultiEditor.Rendering.GlCanvas _canvas;
        private System.Windows.Forms.TabControl _docTabs;
        private System.Windows.Forms.ToolStrip _toolPalette;
        private System.Windows.Forms.ToolStripLabel _editToolsLabel;
        private System.Windows.Forms.ToolStripLabel _drawToolsLabel;
        private System.Windows.Forms.ToolStripLabel _houseToolsLabel;
        private System.Windows.Forms.Splitter _sideSplitter;
        private System.Windows.Forms.Panel _sidePanel;
        private System.Windows.Forms.SplitContainer _sideSplit;
        private UoFiddler.Plugin.MultiEditor.UI.TileBrowserPanel _browser;
        private UoFiddler.Plugin.MultiEditor.UI.InspectorPanel _inspector;
        private System.Windows.Forms.ToolStrip _optionsStrip;
        private System.Windows.Forms.ToolStripLabel _toolLabel;
        private System.Windows.Forms.ToolStripSeparator _optionsSeparator1;
        private System.Windows.Forms.ToolStripLabel _placeLabel;
        private UoFiddler.Plugin.MultiEditor.UI.ToolStripChoice _placementCombo;
        private System.Windows.Forms.ToolStripLabel _brushLabel;
        private System.Windows.Forms.ToolStripLabel _zStepLabel;
        private UoFiddler.Plugin.MultiEditor.UI.ToolStripChoice _zStepCombo;
        private System.Windows.Forms.ToolStripSeparator _optionsSeparator2;
        private System.Windows.Forms.ToolStripLabel _floorLabel;
        private System.Windows.Forms.ToolStripSeparator _optionsSeparator3;
        private System.Windows.Forms.ToolStripLabel _drawTileLabel;
        private System.Windows.Forms.ToolStripSeparator _ridgeSeparator;
        private System.Windows.Forms.ToolStripLabel _ridgeLabel;
        private System.Windows.Forms.ToolStripSeparator _gablesSeparator;
        private System.Windows.Forms.ToolStripLabel _gablesLabel;
        private System.Windows.Forms.ToolStripSeparator _gableFillSeparator;
        private System.Windows.Forms.ToolStripLabel _gableFillLabel;
        private System.Windows.Forms.ToolStrip _mainStrip;
        private System.Windows.Forms.ToolStripDropDownButton _fileMenu;
        private System.Windows.Forms.ToolStripDropDownButton _editMenu;
        private System.Windows.Forms.ToolStripDropDownButton _selectMenu;
        private System.Windows.Forms.ToolStripDropDownButton _viewMenu;
        private System.Windows.Forms.ToolStripSeparator _mainSeparator1;
        private System.Windows.Forms.ToolStripSeparator _mainSeparator2;
        private System.Windows.Forms.ToolStripSeparator _mainSeparator3;
        private System.Windows.Forms.ToolStripSeparator _mainSeparator4;
        private System.Windows.Forms.ToolStripLabel _zoomLabel;
        private System.Windows.Forms.StatusStrip _statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel _statusHint;
        private System.Windows.Forms.ToolStripStatusLabel _statusCell;
        private System.Windows.Forms.ToolStripStatusLabel _statusHover;
        private System.Windows.Forms.ToolStripStatusLabel _statusSelection;
        private System.Windows.Forms.ToolStripStatusLabel _statusSize;
    }
}
