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
    partial class LightControl
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
            splitContainer = new System.Windows.Forms.SplitContainer();
            treeViewLights = new System.Windows.Forms.TreeView();
            treeViewContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            exportImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            insertAtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            InsertText = new System.Windows.Forms.ToolStripTextBox();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pictureBoxPreview = new System.Windows.Forms.PictureBox();
            previewContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            iGPreviewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            backgroundLandTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            LandTileText = new System.Windows.Forms.ToolStripTextBox();
            lightTileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            LightTileText = new System.Windows.Forms.ToolStripTextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            treeViewContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            previewContextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 0);
            splitContainer.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add(treeViewLights);
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add(pictureBoxPreview);
            splitContainer.Size = new System.Drawing.Size(740, 380);
            splitContainer.SplitterDistance = 242;
            splitContainer.SplitterWidth = 5;
            splitContainer.TabIndex = 0;
            // 
            // treeViewLights
            // 
            treeViewLights.ContextMenuStrip = treeViewContextMenuStrip;
            treeViewLights.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewLights.HideSelection = false;
            treeViewLights.Location = new System.Drawing.Point(0, 0);
            treeViewLights.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeViewLights.Name = "treeViewLights";
            treeViewLights.Size = new System.Drawing.Size(242, 380);
            treeViewLights.TabIndex = 0;
            treeViewLights.AfterSelect += AfterSelect;
            // 
            // treeViewContextMenuStrip
            // 
            treeViewContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { exportImageToolStripMenuItem, toolStripSeparator1, removeToolStripMenuItem, replaceToolStripMenuItem, insertAtToolStripMenuItem, toolStripSeparator2, saveToolStripMenuItem });
            treeViewContextMenuStrip.Name = "contextMenuStrip1";
            treeViewContextMenuStrip.Size = new System.Drawing.Size(151, 126);
            // 
            // exportImageToolStripMenuItem
            // 
            exportImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem });
            exportImageToolStripMenuItem.Name = "exportImageToolStripMenuItem";
            exportImageToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            exportImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += OnClickExportBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += OnClickExportTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += OnClickExportJpg;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(147, 6);
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            replaceToolStripMenuItem.Text = "Replace";
            replaceToolStripMenuItem.Click += OnClickReplace;
            // 
            // insertAtToolStripMenuItem
            // 
            insertAtToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { InsertText });
            insertAtToolStripMenuItem.Name = "insertAtToolStripMenuItem";
            insertAtToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            insertAtToolStripMenuItem.Text = "Insert At..";
            // 
            // InsertText
            // 
            InsertText.Name = "InsertText";
            InsertText.Size = new System.Drawing.Size(100, 23);
            InsertText.KeyDown += OnKeyDownInsert;
            InsertText.TextChanged += OnTextChangedInsert;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(147, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            saveToolStripMenuItem.Text = "Save";
            saveToolStripMenuItem.Click += OnClickSave;
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBoxPreview.ContextMenuStrip = previewContextMenuStrip;
            pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxPreview.Location = new System.Drawing.Point(0, 0);
            pictureBoxPreview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new System.Drawing.Size(493, 380);
            pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            pictureBoxPreview.TabIndex = 0;
            pictureBoxPreview.TabStop = false;
            pictureBoxPreview.SizeChanged += OnPictureSizeChanged;
            // 
            // previewContextMenuStrip
            // 
            previewContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { iGPreviewToolStripMenuItem, backgroundLandTileToolStripMenuItem, lightTileToolStripMenuItem });
            previewContextMenuStrip.Name = "contextMenuStrip2";
            previewContextMenuStrip.Size = new System.Drawing.Size(186, 70);
            // 
            // iGPreviewToolStripMenuItem
            // 
            iGPreviewToolStripMenuItem.Name = "iGPreviewToolStripMenuItem";
            iGPreviewToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            iGPreviewToolStripMenuItem.Text = "IG Preview";
            iGPreviewToolStripMenuItem.Click += IgPreviewClicked;
            // 
            // backgroundLandTileToolStripMenuItem
            // 
            backgroundLandTileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { LandTileText });
            backgroundLandTileToolStripMenuItem.Name = "backgroundLandTileToolStripMenuItem";
            backgroundLandTileToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            backgroundLandTileToolStripMenuItem.Text = "Background LandTile";
            // 
            // LandTileText
            // 
            LandTileText.Name = "LandTileText";
            LandTileText.Size = new System.Drawing.Size(100, 23);
            LandTileText.KeyDown += LandTileKeyDown;
            LandTileText.TextChanged += LandTileTextChanged;
            // 
            // lightTileToolStripMenuItem
            // 
            lightTileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { LightTileText });
            lightTileToolStripMenuItem.Name = "lightTileToolStripMenuItem";
            lightTileToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            lightTileToolStripMenuItem.Text = "LightTile";
            // 
            // LightTileText
            // 
            LightTileText.Name = "LightTileText";
            LightTileText.Size = new System.Drawing.Size(100, 23);
            LightTileText.KeyDown += LightTileKeyDown;
            LightTileText.TextChanged += LightTileTextChanged;
            // 
            // LightControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "LightControl";
            Size = new System.Drawing.Size(740, 380);
            Load += OnLoad;
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            treeViewContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            previewContextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundLandTileToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip treeViewContextMenuStrip;
        private System.Windows.Forms.ContextMenuStrip previewContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exportImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem iGPreviewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem insertAtToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox InsertText;
        private System.Windows.Forms.ToolStripTextBox LandTileText;
        private System.Windows.Forms.ToolStripTextBox LightTileText;
        private System.Windows.Forms.ToolStripMenuItem lightTileToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TreeView treeViewLights;
    }
}
