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
    partial class HuesControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HuesControl));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ReplaceText = new System.Windows.Forms.ToolStripTextBox();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.HuesTopMenuToolStrip = new System.Windows.Forms.ToolStrip();
            this.HueIndexToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.HueIndexToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.HueNameToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this.HueNameToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.SearchNameToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.HuesTopMenuToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.importToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(150, 92);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.OnClickSave);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ReplaceText});
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.replaceToolStripMenuItem.Text = "Replace With..";
            // 
            // ReplaceText
            // 
            this.ReplaceText.Name = "ReplaceText";
            this.ReplaceText.Size = new System.Drawing.Size(100, 23);
            this.ReplaceText.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownReplace);
            this.ReplaceText.TextChanged += new System.EventHandler(this.OnTextChangedReplace);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.exportToolStripMenuItem.Text = "Export..";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.OnExport);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.importToolStripMenuItem.Text = "Import..";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.OnImport);
            // 
            // toolStripContainer
            // 
            this.toolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer.ContentPanel
            // 
            this.toolStripContainer.ContentPanel.AutoScroll = true;
            this.toolStripContainer.ContentPanel.Controls.Add(this.vScrollBar);
            this.toolStripContainer.ContentPanel.Controls.Add(this.pictureBox);
            this.toolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(744, 357);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.LeftToolStripPanelVisible = false;
            this.toolStripContainer.Location = new System.Drawing.Point(1, 1);
            this.toolStripContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.RightToolStripPanelVisible = false;
            this.toolStripContainer.Size = new System.Drawing.Size(744, 388);
            this.toolStripContainer.TabIndex = 9;
            this.toolStripContainer.Text = "toolStripContainer";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.HuesTopMenuToolStrip);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(727, 0);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 357);
            this.vScrollBar.TabIndex = 4;
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // pictureBox
            // 
            this.pictureBox.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(744, 357);
            this.pictureBox.TabIndex = 3;
            this.pictureBox.TabStop = false;
            this.pictureBox.SizeChanged += new System.EventHandler(this.OnResize);
            this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.pictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseClick);
            this.pictureBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OnMouseDoubleClick);
            // 
            // HuesTopMenuToolStrip
            // 
            this.HuesTopMenuToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.HuesTopMenuToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.HuesTopMenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HueIndexToolStripLabel,
            this.HueIndexToolStripTextBox,
            this.HueNameToolStripLabel,
            this.HueNameToolStripTextBox,
            this.SearchNameToolStripButton});
            this.HuesTopMenuToolStrip.Location = new System.Drawing.Point(0, 0);
            this.HuesTopMenuToolStrip.Name = "HuesTopMenuToolStrip";
            this.HuesTopMenuToolStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.HuesTopMenuToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.HuesTopMenuToolStrip.Size = new System.Drawing.Size(744, 31);
            this.HuesTopMenuToolStrip.Stretch = true;
            this.HuesTopMenuToolStrip.TabIndex = 0;
            // 
            // HueIndexToolStripLabel
            // 
            this.HueIndexToolStripLabel.Name = "HueIndexToolStripLabel";
            this.HueIndexToolStripLabel.Size = new System.Drawing.Size(64, 20);
            this.HueIndexToolStripLabel.Text = "Hue Index:";
            // 
            // HueIndexToolStripTextBox
            // 
            this.HueIndexToolStripTextBox.Name = "HueIndexToolStripTextBox";
            this.HueIndexToolStripTextBox.Size = new System.Drawing.Size(100, 23);
            this.HueIndexToolStripTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.HueIndexToolStripTextBox_KeyUp);
            // 
            // HueNameToolStripLabel
            // 
            this.HueNameToolStripLabel.Name = "HueNameToolStripLabel";
            this.HueNameToolStripLabel.Size = new System.Drawing.Size(67, 20);
            this.HueNameToolStripLabel.Text = "Hue Name:";
            // 
            // HueNameToolStripTextBox
            // 
            this.HueNameToolStripTextBox.Name = "HueNameToolStripTextBox";
            this.HueNameToolStripTextBox.Size = new System.Drawing.Size(100, 23);
            this.HueNameToolStripTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.HueNameToolStripTextBox_KeyUp);
            // 
            // SearchNameToolStripButton
            // 
            this.SearchNameToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.SearchNameToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("SearchNameToolStripButton.Image")));
            this.SearchNameToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SearchNameToolStripButton.Name = "SearchNameToolStripButton";
            this.SearchNameToolStripButton.Size = new System.Drawing.Size(60, 20);
            this.SearchNameToolStripButton.Text = "Find next";
            this.SearchNameToolStripButton.Click += new System.EventHandler(this.SearchNameToolStripButton_Click);
            // 
            // HuesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "HuesControl";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.Size = new System.Drawing.Size(746, 390);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.HuesTopMenuToolStrip.ResumeLayout(false);
            this.HuesTopMenuToolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox ReplaceText;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripContainer toolStripContainer;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStrip HuesTopMenuToolStrip;
        private System.Windows.Forms.ToolStripLabel HueIndexToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox HueIndexToolStripTextBox;
        private System.Windows.Forms.ToolStripLabel HueNameToolStripLabel;
        private System.Windows.Forms.ToolStripTextBox HueNameToolStripTextBox;
        private System.Windows.Forms.ToolStripButton SearchNameToolStripButton;
    }
}
