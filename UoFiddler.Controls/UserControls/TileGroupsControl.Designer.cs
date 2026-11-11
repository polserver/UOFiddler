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
    partial class TileGroupsControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            nodeContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            toolStrip = new System.Windows.Forms.ToolStrip();
            ReloadButton = new System.Windows.Forms.ToolStripButton();
            SaveButton = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            FileFilterDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            GroupsTreeView = new System.Windows.Forms.TreeView();
            rightPanel = new System.Windows.Forms.TableLayoutPanel();
            DetailPictureBox = new System.Windows.Forms.PictureBox();
            InfoLabel = new System.Windows.Forms.Label();
            editPanel = new System.Windows.Forms.FlowLayoutPanel();
            labelNewId = new System.Windows.Forms.Label();
            EditTileIdUpDown = new System.Windows.Forms.NumericUpDown();
            ApplyButton = new System.Windows.Forms.Button();
            RevertButton = new System.Windows.Forms.Button();
            toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            rightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(DetailPictureBox)).BeginInit();
            editPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(EditTileIdUpDown)).BeginInit();
            SuspendLayout();
            //
            // toolStrip
            //
            toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                ReloadButton,
                SaveButton,
                toolStripSeparator1,
                FileFilterDropDown});
            toolStrip.Location = new System.Drawing.Point(0, 0);
            toolStrip.Name = "toolStrip";
            toolStrip.Size = new System.Drawing.Size(800, 25);
            toolStrip.TabIndex = 0;
            //
            // ReloadButton
            //
            ReloadButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            ReloadButton.Name = "ReloadButton";
            ReloadButton.Size = new System.Drawing.Size(50, 22);
            ReloadButton.Text = "Reload";
            ReloadButton.Click += new System.EventHandler(OnReloadClick);
            //
            // SaveButton
            //
            SaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            SaveButton.Enabled = false;
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new System.Drawing.Size(82, 22);
            SaveButton.Text = "Save changes";
            SaveButton.Click += new System.EventHandler(OnSaveClick);
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // FileFilterDropDown
            //
            FileFilterDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            FileFilterDropDown.Name = "FileFilterDropDown";
            FileFilterDropDown.Size = new System.Drawing.Size(64, 22);
            FileFilterDropDown.Text = "Files...";
            //
            // splitContainer1
            //
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 25);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Panel1.Controls.Add(GroupsTreeView);
            splitContainer1.Panel2.Controls.Add(rightPanel);
            splitContainer1.Size = new System.Drawing.Size(800, 575);
            splitContainer1.SplitterDistance = 320;
            splitContainer1.TabIndex = 1;
            //
            // GroupsTreeView
            //
            GroupsTreeView.ContextMenuStrip = nodeContextMenu;
            GroupsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            GroupsTreeView.HideSelection = false;
            GroupsTreeView.Location = new System.Drawing.Point(0, 0);
            GroupsTreeView.Name = "GroupsTreeView";
            GroupsTreeView.Size = new System.Drawing.Size(320, 575);
            GroupsTreeView.TabIndex = 0;
            GroupsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(OnTreeAfterSelect);
            GroupsTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(OnTreeNodeMouseClick);
            //
            // nodeContextMenu
            //
            nodeContextMenu.Name = "nodeContextMenu";
            nodeContextMenu.Opening += new System.ComponentModel.CancelEventHandler(OnNodeContextMenuOpening);
            //
            // rightPanel
            //
            rightPanel.ColumnCount = 1;
            rightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rightPanel.Controls.Add(DetailPictureBox, 0, 0);
            rightPanel.Controls.Add(InfoLabel, 0, 1);
            rightPanel.Controls.Add(editPanel, 0, 2);
            rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            rightPanel.Location = new System.Drawing.Point(0, 0);
            rightPanel.Name = "rightPanel";
            rightPanel.RowCount = 3;
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            rightPanel.Size = new System.Drawing.Size(476, 575);
            rightPanel.TabIndex = 0;
            //
            // DetailPictureBox
            //
            DetailPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            DetailPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            DetailPictureBox.Location = new System.Drawing.Point(3, 3);
            DetailPictureBox.Name = "DetailPictureBox";
            DetailPictureBox.Size = new System.Drawing.Size(470, 174);
            DetailPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            DetailPictureBox.TabIndex = 0;
            DetailPictureBox.TabStop = false;
            //
            // InfoLabel
            //
            InfoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            InfoLabel.Location = new System.Drawing.Point(3, 183);
            InfoLabel.Name = "InfoLabel";
            InfoLabel.Padding = new System.Windows.Forms.Padding(4);
            InfoLabel.Size = new System.Drawing.Size(470, 345);
            InfoLabel.TabIndex = 1;
            InfoLabel.Text = "Select a tile slot in the tree to view its art and details.";
            //
            // editPanel
            //
            editPanel.Controls.Add(labelNewId);
            editPanel.Controls.Add(EditTileIdUpDown);
            editPanel.Controls.Add(ApplyButton);
            editPanel.Controls.Add(RevertButton);
            editPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            editPanel.Location = new System.Drawing.Point(3, 534);
            editPanel.Name = "editPanel";
            editPanel.Size = new System.Drawing.Size(470, 38);
            editPanel.TabIndex = 2;
            //
            // labelNewId
            //
            labelNewId.AutoSize = true;
            labelNewId.Location = new System.Drawing.Point(3, 8);
            labelNewId.Margin = new System.Windows.Forms.Padding(3, 8, 3, 0);
            labelNewId.Name = "labelNewId";
            labelNewId.Size = new System.Drawing.Size(45, 15);
            labelNewId.Text = "Tile ID:";
            //
            // EditTileIdUpDown
            //
            EditTileIdUpDown.Enabled = false;
            EditTileIdUpDown.Location = new System.Drawing.Point(54, 6);
            EditTileIdUpDown.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            EditTileIdUpDown.Name = "EditTileIdUpDown";
            EditTileIdUpDown.Size = new System.Drawing.Size(96, 23);
            EditTileIdUpDown.TabIndex = 0;
            //
            // ApplyButton
            //
            ApplyButton.Enabled = false;
            ApplyButton.Location = new System.Drawing.Point(156, 5);
            ApplyButton.Name = "ApplyButton";
            ApplyButton.Size = new System.Drawing.Size(64, 25);
            ApplyButton.TabIndex = 1;
            ApplyButton.Text = "Apply";
            ApplyButton.UseVisualStyleBackColor = true;
            ApplyButton.Click += new System.EventHandler(OnApplyClick);
            //
            // RevertButton
            //
            RevertButton.Enabled = false;
            RevertButton.Location = new System.Drawing.Point(226, 5);
            RevertButton.Name = "RevertButton";
            RevertButton.Size = new System.Drawing.Size(64, 25);
            RevertButton.TabIndex = 2;
            RevertButton.Text = "Revert";
            RevertButton.UseVisualStyleBackColor = true;
            RevertButton.Click += new System.EventHandler(OnRevertClick);
            //
            // TileGroupsControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Controls.Add(toolStrip);
            Name = "TileGroupsControl";
            Size = new System.Drawing.Size(800, 600);
            Load += new System.EventHandler(OnControlLoad);
            toolStrip.ResumeLayout(false);
            toolStrip.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            rightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(DetailPictureBox)).EndInit();
            editPanel.ResumeLayout(false);
            editPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(EditTileIdUpDown)).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton ReloadButton;
        private System.Windows.Forms.ToolStripButton SaveButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton FileFilterDropDown;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TreeView GroupsTreeView;
        private System.Windows.Forms.TableLayoutPanel rightPanel;
        private System.Windows.Forms.PictureBox DetailPictureBox;
        private System.Windows.Forms.Label InfoLabel;
        private System.Windows.Forms.FlowLayoutPanel editPanel;
        private System.Windows.Forms.Label labelNewId;
        private System.Windows.Forms.NumericUpDown EditTileIdUpDown;
        private System.Windows.Forms.Button ApplyButton;
        private System.Windows.Forms.Button RevertButton;
        private System.Windows.Forms.ContextMenuStrip nodeContextMenu;
    }
}
