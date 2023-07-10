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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HuesControl));
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ReplaceText = new System.Windows.Forms.ToolStripTextBox();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            vScrollBar = new System.Windows.Forms.VScrollBar();
            pictureBox = new System.Windows.Forms.PictureBox();
            HuesTopMenuToolStrip = new System.Windows.Forms.ToolStrip();
            HueIndexToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            HueIndexToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            HueNameToolStripLabel = new System.Windows.Forms.ToolStripLabel();
            HueNameToolStripTextBox = new System.Windows.Forms.ToolStripTextBox();
            SearchNameToolStripButton = new System.Windows.Forms.ToolStripButton();
            exportAllHueNamesListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            contextMenuStrip1.SuspendLayout();
            toolStripContainer.ContentPanel.SuspendLayout();
            toolStripContainer.TopToolStripPanel.SuspendLayout();
            toolStripContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            HuesTopMenuToolStrip.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { saveToolStripMenuItem, replaceToolStripMenuItem, exportToolStripMenuItem, importToolStripMenuItem, toolStripSeparator1, exportAllHueNamesListToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(203, 142);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ReplaceText });
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            replaceToolStripMenuItem.Text = "Replace With..";
            // 
            // ReplaceText
            // 
            ReplaceText.Name = "ReplaceText";
            ReplaceText.Size = new System.Drawing.Size(100, 23);
            ReplaceText.KeyDown += OnKeyDownReplace;
            ReplaceText.TextChanged += OnTextChangedReplace;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            exportToolStripMenuItem.Text = "Export..";
            exportToolStripMenuItem.Click += OnExport;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            importToolStripMenuItem.Text = "Import..";
            importToolStripMenuItem.Click += OnImport;
            // 
            // toolStripContainer
            // 
            toolStripContainer.BottomToolStripPanelVisible = false;
            // 
            // toolStripContainer.ContentPanel
            // 
            toolStripContainer.ContentPanel.AutoScroll = true;
            toolStripContainer.ContentPanel.Controls.Add(vScrollBar);
            toolStripContainer.ContentPanel.Controls.Add(pictureBox);
            toolStripContainer.ContentPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            toolStripContainer.ContentPanel.Size = new System.Drawing.Size(740, 359);
            toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStripContainer.LeftToolStripPanelVisible = false;
            toolStripContainer.Location = new System.Drawing.Point(1, 1);
            toolStripContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            toolStripContainer.Name = "toolStripContainer";
            toolStripContainer.RightToolStripPanelVisible = false;
            toolStripContainer.Size = new System.Drawing.Size(740, 390);
            toolStripContainer.TabIndex = 9;
            toolStripContainer.Text = "toolStripContainer";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            toolStripContainer.TopToolStripPanel.Controls.Add(HuesTopMenuToolStrip);
            // 
            // vScrollBar
            // 
            vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            vScrollBar.Location = new System.Drawing.Point(723, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new System.Drawing.Size(17, 359);
            vScrollBar.TabIndex = 4;
            vScrollBar.Scroll += OnScroll;
            // 
            // pictureBox
            // 
            pictureBox.ContextMenuStrip = contextMenuStrip1;
            pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox.Location = new System.Drawing.Point(0, 0);
            pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(740, 359);
            pictureBox.TabIndex = 3;
            pictureBox.TabStop = false;
            pictureBox.SizeChanged += OnResize;
            pictureBox.Paint += OnPaint;
            pictureBox.MouseClick += OnMouseClick;
            pictureBox.MouseDoubleClick += OnMouseDoubleClick;
            // 
            // HuesTopMenuToolStrip
            // 
            HuesTopMenuToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            HuesTopMenuToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            HuesTopMenuToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { HueIndexToolStripLabel, HueIndexToolStripTextBox, HueNameToolStripLabel, HueNameToolStripTextBox, SearchNameToolStripButton });
            HuesTopMenuToolStrip.Location = new System.Drawing.Point(0, 0);
            HuesTopMenuToolStrip.Name = "HuesTopMenuToolStrip";
            HuesTopMenuToolStrip.Padding = new System.Windows.Forms.Padding(0, 4, 0, 4);
            HuesTopMenuToolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            HuesTopMenuToolStrip.Size = new System.Drawing.Size(740, 31);
            HuesTopMenuToolStrip.Stretch = true;
            HuesTopMenuToolStrip.TabIndex = 0;
            // 
            // HueIndexToolStripLabel
            // 
            HueIndexToolStripLabel.Name = "HueIndexToolStripLabel";
            HueIndexToolStripLabel.Size = new System.Drawing.Size(64, 20);
            HueIndexToolStripLabel.Text = "Hue Index:";
            // 
            // HueIndexToolStripTextBox
            // 
            HueIndexToolStripTextBox.Name = "HueIndexToolStripTextBox";
            HueIndexToolStripTextBox.Size = new System.Drawing.Size(100, 23);
            HueIndexToolStripTextBox.KeyUp += HueIndexToolStripTextBox_KeyUp;
            // 
            // HueNameToolStripLabel
            // 
            HueNameToolStripLabel.Name = "HueNameToolStripLabel";
            HueNameToolStripLabel.Size = new System.Drawing.Size(67, 20);
            HueNameToolStripLabel.Text = "Hue Name:";
            // 
            // HueNameToolStripTextBox
            // 
            HueNameToolStripTextBox.Name = "HueNameToolStripTextBox";
            HueNameToolStripTextBox.Size = new System.Drawing.Size(100, 23);
            HueNameToolStripTextBox.KeyUp += HueNameToolStripTextBox_KeyUp;
            // 
            // SearchNameToolStripButton
            // 
            SearchNameToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            SearchNameToolStripButton.Image = (System.Drawing.Image)resources.GetObject("SearchNameToolStripButton.Image");
            SearchNameToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            SearchNameToolStripButton.Name = "SearchNameToolStripButton";
            SearchNameToolStripButton.Size = new System.Drawing.Size(60, 20);
            SearchNameToolStripButton.Text = "Find next";
            SearchNameToolStripButton.Click += SearchNameToolStripButton_Click;
            // 
            // exportAllHueNamesListToolStripMenuItem
            // 
            exportAllHueNamesListToolStripMenuItem.Name = "exportAllHueNamesListToolStripMenuItem";
            exportAllHueNamesListToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            exportAllHueNamesListToolStripMenuItem.Text = "Export all hue names list";
            exportAllHueNamesListToolStripMenuItem.Click += ExportAllHueNamesListToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(199, 6);
            // 
            // HuesControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(toolStripContainer);
            DoubleBuffered = true;
            ForeColor = System.Drawing.SystemColors.ControlText;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "HuesControl";
            Padding = new System.Windows.Forms.Padding(1);
            Size = new System.Drawing.Size(742, 392);
            Load += OnLoad;
            contextMenuStrip1.ResumeLayout(false);
            toolStripContainer.ContentPanel.ResumeLayout(false);
            toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer.TopToolStripPanel.PerformLayout();
            toolStripContainer.ResumeLayout(false);
            toolStripContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            HuesTopMenuToolStrip.ResumeLayout(false);
            HuesTopMenuToolStrip.PerformLayout();
            ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exportAllHueNamesListToolStripMenuItem;
    }
}
