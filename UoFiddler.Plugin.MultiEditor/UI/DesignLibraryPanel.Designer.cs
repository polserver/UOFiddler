namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class DesignLibraryPanel
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            _list = new System.Windows.Forms.ListView();
            columnDesign = new System.Windows.Forms.ColumnHeader();
            columnSize = new System.Windows.Forms.ColumnHeader();
            columnItems = new System.Windows.Forms.ColumnHeader();
            treeSplitter = new GripSplitter();
            _tree = new System.Windows.Forms.TreeView();
            previewSplitter = new GripSplitter();
            _preview = new System.Windows.Forms.PictureBox();
            _info = new System.Windows.Forms.Label();
            buttons = new System.Windows.Forms.FlowLayoutPanel();
            _open = new System.Windows.Forms.Button();
            _place = new System.Windows.Forms.Button();
            _saveBlueprints = new System.Windows.Forms.Button();
            _filter = new System.Windows.Forms.TextBox();
            bar = new System.Windows.Forms.ToolStrip();
            chooseButton = new System.Windows.Forms.ToolStripButton();
            refreshButton = new System.Windows.Forms.ToolStripButton();
            _folderLabel = new System.Windows.Forms.ToolStripLabel();
            _previewTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)_preview).BeginInit();
            buttons.SuspendLayout();
            bar.SuspendLayout();
            SuspendLayout();
            // 
            // _list
            // 
            _list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnDesign, columnSize, columnItems });
            _list.Dock = System.Windows.Forms.DockStyle.Fill;
            _list.FullRowSelect = true;
            _list.Location = new System.Drawing.Point(0, 225);
            _list.Name = "_list";
            _list.Size = new System.Drawing.Size(150, 0);
            _list.TabIndex = 0;
            _list.UseCompatibleStateImageBehavior = false;
            _list.View = System.Windows.Forms.View.Details;
            _list.VirtualMode = true;
            _list.RetrieveVirtualItem += List_RetrieveVirtualItem;
            _list.SelectedIndexChanged += List_SelectedIndexChanged;
            _list.VirtualItemsSelectionRangeChanged += List_VirtualItemsSelectionRangeChanged;
            _list.DoubleClick += List_DoubleClick;
            _list.KeyDown += List_KeyDown;
            // 
            // columnDesign
            // 
            columnDesign.Text = "Design";
            columnDesign.Width = 170;
            // 
            // columnSize
            // 
            columnSize.Text = "Size";
            // 
            // columnItems
            // 
            columnItems.Text = "Items";
            columnItems.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnItems.Width = 56;
            // 
            // treeSplitter
            // 
            treeSplitter.Dock = System.Windows.Forms.DockStyle.Top;
            treeSplitter.Location = new System.Drawing.Point(0, 218);
            treeSplitter.MinExtra = 80;
            treeSplitter.MinSize = 60;
            treeSplitter.Name = "treeSplitter";
            treeSplitter.Size = new System.Drawing.Size(150, 7);
            treeSplitter.TabIndex = 1;
            treeSplitter.TabStop = false;
            // 
            // _tree
            // 
            _tree.Dock = System.Windows.Forms.DockStyle.Top;
            _tree.HideSelection = false;
            _tree.Location = new System.Drawing.Point(0, 48);
            _tree.Name = "_tree";
            _tree.Size = new System.Drawing.Size(150, 170);
            _tree.TabIndex = 2;
            _tree.AfterSelect += Tree_AfterSelect;
            // 
            // previewSplitter
            // 
            previewSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            previewSplitter.Location = new System.Drawing.Point(0, -143);
            previewSplitter.MinExtra = 80;
            previewSplitter.MinSize = 80;
            previewSplitter.Name = "previewSplitter";
            previewSplitter.Size = new System.Drawing.Size(150, 7);
            previewSplitter.TabIndex = 3;
            previewSplitter.TabStop = false;
            // 
            // _preview
            // 
            _preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            _preview.Dock = System.Windows.Forms.DockStyle.Bottom;
            _preview.Location = new System.Drawing.Point(0, -136);
            _preview.Name = "_preview";
            _preview.Size = new System.Drawing.Size(150, 220);
            _preview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            _preview.TabIndex = 4;
            _preview.TabStop = false;
            // 
            // _info
            // 
            _info.AutoEllipsis = true;
            _info.Dock = System.Windows.Forms.DockStyle.Bottom;
            _info.ForeColor = System.Drawing.SystemColors.GrayText;
            _info.Location = new System.Drawing.Point(0, 84);
            _info.Name = "_info";
            _info.Size = new System.Drawing.Size(150, 34);
            _info.TabIndex = 5;
            // 
            // buttons
            // 
            buttons.Controls.Add(_open);
            buttons.Controls.Add(_place);
            buttons.Controls.Add(_saveBlueprints);
            buttons.AutoSize = true;
            buttons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
            buttons.Location = new System.Drawing.Point(0, 118);
            buttons.Name = "buttons";
            buttons.Size = new System.Drawing.Size(150, 32);
            buttons.TabIndex = 6;
            // 
            // _open
            // 
            _open.Enabled = false;
            _open.Location = new System.Drawing.Point(3, 3);
            _open.Name = "_open";
            _open.Size = new System.Drawing.Size(130, 26);
            _open.TabIndex = 0;
            _open.Text = "Open in a new tab";
            _open.UseVisualStyleBackColor = true;
            _open.Click += Open_Click;
            // 
            // _place
            // 
            _place.Enabled = false;
            _place.Location = new System.Drawing.Point(3, 35);
            _place.Name = "_place";
            _place.Size = new System.Drawing.Size(130, 26);
            _place.TabIndex = 1;
            _place.Text = "Place in this multi";
            _place.UseVisualStyleBackColor = true;
            _place.Click += Place_Click;
            // 
            // _saveBlueprints
            // 
            _saveBlueprints.Enabled = false;
            _saveBlueprints.Location = new System.Drawing.Point(3, 67);
            _saveBlueprints.Name = "_saveBlueprints";
            _saveBlueprints.Size = new System.Drawing.Size(150, 26);
            _saveBlueprints.TabIndex = 2;
            _saveBlueprints.Text = "Save as blueprints...";
            _saveBlueprints.UseVisualStyleBackColor = true;
            _saveBlueprints.Click += SaveBlueprints_Click;
            // 
            // _filter
            // 
            _filter.Dock = System.Windows.Forms.DockStyle.Top;
            _filter.Location = new System.Drawing.Point(0, 25);
            _filter.Name = "_filter";
            _filter.PlaceholderText = "Filter designs by name";
            _filter.Size = new System.Drawing.Size(150, 23);
            _filter.TabIndex = 7;
            _filter.TextChanged += Filter_TextChanged;
            // 
            // bar
            // 
            bar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            bar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { chooseButton, refreshButton, _folderLabel });
            bar.Location = new System.Drawing.Point(0, 0);
            bar.Name = "bar";
            bar.Size = new System.Drawing.Size(150, 25);
            bar.TabIndex = 8;
            // 
            // chooseButton
            // 
            chooseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            chooseButton.Name = "chooseButton";
            chooseButton.Size = new System.Drawing.Size(53, 22);
            chooseButton.Text = "Folder...";
            chooseButton.ToolTipText = "Choose the folder that holds the UO Architect designs (.uoa files, also in subfolders)";
            chooseButton.Click += ChooseButton_Click;
            // 
            // refreshButton
            // 
            refreshButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new System.Drawing.Size(50, 22);
            refreshButton.Text = "Refresh";
            refreshButton.ToolTipText = "Read the folder again";
            refreshButton.Click += RefreshButton_Click;
            // 
            // _folderLabel
            // 
            _folderLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            _folderLabel.Name = "_folderLabel";
            _folderLabel.Size = new System.Drawing.Size(0, 22);
            // 
            // _previewTimer
            // 
            _previewTimer.Interval = 180;
            _previewTimer.Tick += PreviewTimer_Tick;
            // 
            // DesignLibraryPanel
            // 
            Controls.Add(_list);
            Controls.Add(treeSplitter);
            Controls.Add(_tree);
            Controls.Add(previewSplitter);
            Controls.Add(_preview);
            Controls.Add(_info);
            Controls.Add(buttons);
            Controls.Add(_filter);
            Controls.Add(bar);
            Name = "DesignLibraryPanel";
            ((System.ComponentModel.ISupportInitialize)_preview).EndInit();
            buttons.ResumeLayout(false);
            bar.ResumeLayout(false);
            bar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader columnDesign;
        private System.Windows.Forms.ColumnHeader columnSize;
        private System.Windows.Forms.ColumnHeader columnItems;
        private UoFiddler.Plugin.MultiEditor.UI.GripSplitter treeSplitter;
        private System.Windows.Forms.TreeView _tree;
        private UoFiddler.Plugin.MultiEditor.UI.GripSplitter previewSplitter;
        private System.Windows.Forms.PictureBox _preview;
        private System.Windows.Forms.Label _info;
        private System.Windows.Forms.FlowLayoutPanel buttons;
        private System.Windows.Forms.Button _open;
        private System.Windows.Forms.Button _place;
        private System.Windows.Forms.Button _saveBlueprints;
        private System.Windows.Forms.TextBox _filter;
        private System.Windows.Forms.ToolStrip bar;
        private System.Windows.Forms.ToolStripButton chooseButton;
        private System.Windows.Forms.ToolStripButton refreshButton;
        private System.Windows.Forms.ToolStripLabel _folderLabel;
        private System.Windows.Forms.Timer _previewTimer;
    }
}
