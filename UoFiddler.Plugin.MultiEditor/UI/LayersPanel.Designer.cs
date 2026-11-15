namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class LayersPanel
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
            layersBox = new System.Windows.Forms.GroupBox();
            _list = new System.Windows.Forms.ListView();
            columnName = new System.Windows.Forms.ColumnHeader();
            columnCount = new System.Windows.Forms.ColumnHeader();
            _strip = new System.Windows.Forms.ToolStrip();
            _addButton = new System.Windows.Forms.ToolStripButton();
            _duplicateButton = new System.Windows.Forms.ToolStripButton();
            _deleteButton = new System.Windows.Forms.ToolStripButton();
            _mergeButton = new System.Windows.Forms.ToolStripButton();
            _upButton = new System.Windows.Forms.ToolStripButton();
            _downButton = new System.Windows.Forms.ToolStripButton();
            layersBox.SuspendLayout();
            _strip.SuspendLayout();
            SuspendLayout();
            //
            // layersBox
            //
            layersBox.Controls.Add(_list);
            layersBox.Controls.Add(_strip);
            layersBox.Dock = System.Windows.Forms.DockStyle.Fill;
            layersBox.Location = new System.Drawing.Point(0, 0);
            layersBox.Name = "layersBox";
            layersBox.Size = new System.Drawing.Size(240, 170);
            layersBox.TabIndex = 0;
            layersBox.TabStop = false;
            layersBox.Text = "Layers";
            //
            // _list
            //
            _list.CheckBoxes = true;
            _list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnName, columnCount });
            _list.Dock = System.Windows.Forms.DockStyle.Fill;
            _list.FullRowSelect = true;
            _list.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            _list.HideSelection = false;
            _list.LabelEdit = true;
            _list.Location = new System.Drawing.Point(3, 19);
            _list.MultiSelect = false;
            _list.Name = "_list";
            _list.Size = new System.Drawing.Size(234, 123);
            _list.TabIndex = 0;
            _list.UseCompatibleStateImageBehavior = false;
            _list.View = System.Windows.Forms.View.Details;
            _list.AfterLabelEdit += List_AfterLabelEdit;
            _list.BeforeLabelEdit += List_BeforeLabelEdit;
            _list.ItemChecked += List_ItemChecked;
            _list.SelectedIndexChanged += List_SelectedIndexChanged;
            _list.DoubleClick += List_DoubleClick;
            _list.KeyDown += List_KeyDown;
            _list.MouseUp += List_MouseUp;
            _list.Resize += List_Resize;
            //
            // columnName
            //
            columnName.Text = "Layer";
            columnName.Width = 150;
            //
            // columnCount
            //
            columnCount.Text = "Tiles";
            columnCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnCount.Width = 56;
            //
            // _strip
            //
            _strip.Dock = System.Windows.Forms.DockStyle.Bottom;
            _strip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            _strip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { _addButton, _duplicateButton, _deleteButton, _mergeButton, _upButton, _downButton });
            _strip.Location = new System.Drawing.Point(3, 142);
            _strip.Name = "_strip";
            _strip.Size = new System.Drawing.Size(234, 25);
            _strip.TabIndex = 1;
            //
            // _addButton
            //
            _addButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _addButton.Name = "_addButton";
            _addButton.Text = "New";
            _addButton.ToolTipText = "Add a new layer on top (Ctrl+Shift+N)";
            _addButton.Click += AddButton_Click;
            //
            // _duplicateButton
            //
            _duplicateButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _duplicateButton.Name = "_duplicateButton";
            _duplicateButton.Text = "Dup";
            _duplicateButton.ToolTipText = "Duplicate the active layer with its tiles";
            _duplicateButton.Click += DuplicateButton_Click;
            //
            // _deleteButton
            //
            _deleteButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _deleteButton.Name = "_deleteButton";
            _deleteButton.Text = "Del";
            _deleteButton.ToolTipText = "Delete the active layer and its tiles";
            _deleteButton.Click += DeleteButton_Click;
            //
            // _mergeButton
            //
            _mergeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _mergeButton.Name = "_mergeButton";
            _mergeButton.Text = "Merge";
            _mergeButton.ToolTipText = "Merge the active layer into the one below";
            _mergeButton.Click += MergeButton_Click;
            //
            // _upButton
            //
            _upButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _upButton.Name = "_upButton";
            _upButton.Text = "▲";
            _upButton.ToolTipText = "Move the active layer up";
            _upButton.Click += UpButton_Click;
            //
            // _downButton
            //
            _downButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            _downButton.Name = "_downButton";
            _downButton.Text = "▼";
            _downButton.ToolTipText = "Move the active layer down";
            _downButton.Click += DownButton_Click;
            //
            // LayersPanel
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(layersBox);
            Name = "LayersPanel";
            Size = new System.Drawing.Size(240, 170);
            layersBox.ResumeLayout(false);
            layersBox.PerformLayout();
            _strip.ResumeLayout(false);
            _strip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox layersBox;
        private System.Windows.Forms.ListView _list;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnCount;
        private System.Windows.Forms.ToolStrip _strip;
        private System.Windows.Forms.ToolStripButton _addButton;
        private System.Windows.Forms.ToolStripButton _duplicateButton;
        private System.Windows.Forms.ToolStripButton _deleteButton;
        private System.Windows.Forms.ToolStripButton _mergeButton;
        private System.Windows.Forms.ToolStripButton _upButton;
        private System.Windows.Forms.ToolStripButton _downButton;
    }
}
