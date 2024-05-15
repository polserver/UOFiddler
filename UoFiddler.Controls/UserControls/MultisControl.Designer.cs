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
    partial class MultisControl
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
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            TreeViewMulti = new System.Windows.Forms.TreeView();
            contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toTextfileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toUOAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toWscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toCsvToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toUOX3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            exportAllImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            ChangeBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            UseTransparencyForPNGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exportAllPartsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toTextFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toUOAFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toWSCFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toCSVFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toUOX3FileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toXMLFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            tabControl3 = new System.Windows.Forms.TabControl();
            tabPage5 = new System.Windows.Forms.TabPage();
            splitContainer3 = new System.Windows.Forms.SplitContainer();
            HeightChangeMulti = new System.Windows.Forms.TrackBar();
            splitContainer4 = new System.Windows.Forms.SplitContainer();
            MultiPictureBox = new System.Windows.Forms.PictureBox();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            extractImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            asJpgToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            statusMulti = new System.Windows.Forms.StatusStrip();
            StatusMultiText = new System.Windows.Forms.ToolStripStatusLabel();
            tabPage6 = new System.Windows.Forms.TabPage();
            MultiComponentBox = new System.Windows.Forms.RichTextBox();
            toolTip = new System.Windows.Forms.ToolTip(components);
            colorDialog = new System.Windows.Forms.ColorDialog();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            contextMenuStrip2.SuspendLayout();
            toolStrip1.SuspendLayout();
            tabControl3.SuspendLayout();
            tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)HeightChangeMulti).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer4).BeginInit();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();
            splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MultiPictureBox).BeginInit();
            contextMenuStrip1.SuspendLayout();
            statusMulti.SuspendLayout();
            tabPage6.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(TreeViewMulti);
            splitContainer2.Panel1.Controls.Add(toolStrip1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(tabControl3);
            splitContainer2.Size = new System.Drawing.Size(755, 442);
            splitContainer2.SplitterDistance = 245;
            splitContainer2.SplitterWidth = 5;
            splitContainer2.TabIndex = 1;
            // 
            // TreeViewMulti
            // 
            TreeViewMulti.ContextMenuStrip = contextMenuStrip2;
            TreeViewMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            TreeViewMulti.HideSelection = false;
            TreeViewMulti.Location = new System.Drawing.Point(0, 25);
            TreeViewMulti.Margin = new System.Windows.Forms.Padding(0);
            TreeViewMulti.Name = "TreeViewMulti";
            TreeViewMulti.ShowNodeToolTips = true;
            TreeViewMulti.Size = new System.Drawing.Size(245, 417);
            TreeViewMulti.TabIndex = 0;
            TreeViewMulti.AfterSelect += AfterSelect_Multi;
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem4, importToolStripMenuItem, exportToolStripMenuItem, removeToolStripMenuItem });
            contextMenuStrip2.Name = "contextMenuStrip1";
            contextMenuStrip2.Size = new System.Drawing.Size(157, 92);
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.CheckOnClick = true;
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new System.Drawing.Size(156, 22);
            toolStripMenuItem4.Text = "Show Free Slots";
            toolStripMenuItem4.Click += OnClickFreeSlots;
            // 
            // importToolStripMenuItem
            // 
            importToolStripMenuItem.Name = "importToolStripMenuItem";
            importToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            importToolStripMenuItem.Text = "Import..";
            importToolStripMenuItem.Click += OnClickImport;
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toTextfileToolStripMenuItem, toUOAToolStripMenuItem, toWscToolStripMenuItem, toCsvToolStripMenuItem, toUOX3ToolStripMenuItem });
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            exportToolStripMenuItem.Text = "Export..";
            // 
            // toTextfileToolStripMenuItem
            // 
            toTextfileToolStripMenuItem.Name = "toTextfileToolStripMenuItem";
            toTextfileToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            toTextfileToolStripMenuItem.Text = "To Textfile";
            toTextfileToolStripMenuItem.Click += OnExportTextFile;
            // 
            // toUOAToolStripMenuItem
            // 
            toUOAToolStripMenuItem.Name = "toUOAToolStripMenuItem";
            toUOAToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            toUOAToolStripMenuItem.Text = "To UOA";
            toUOAToolStripMenuItem.Click += OnExportUOAFile;
            // 
            // toWscToolStripMenuItem
            // 
            toWscToolStripMenuItem.Name = "toWscToolStripMenuItem";
            toWscToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            toWscToolStripMenuItem.Text = "To WSC";
            toWscToolStripMenuItem.Click += OnExportWscFile;
            // 
            // toCsvToolStripMenuItem
            // 
            toCsvToolStripMenuItem.Name = "toCsvToolStripMenuItem";
            toCsvToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            toCsvToolStripMenuItem.Text = "To CSV";
            toCsvToolStripMenuItem.Click += OnExportCsvFile;
            // 
            // toUOX3ToolStripMenuItem
            // 
            toUOX3ToolStripMenuItem.Name = "toUOX3ToolStripMenuItem";
            toUOX3ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            toUOX3ToolStripMenuItem.Text = "To UOX3";
            toUOX3ToolStripMenuItem.Click += OnExportUox3File;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += OnClickRemove;
            // 
            // toolStrip1
            // 
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripDropDownButton1 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(245, 25);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { exportAllImagesToolStripMenuItem, ChangeBackgroundColorToolStripMenuItem, UseTransparencyForPNGToolStripMenuItem, toolStripSeparator1, exportAllPartsToolStripMenuItem, toolStripSeparator2, saveToolStripMenuItem1 });
            toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new System.Drawing.Size(45, 22);
            toolStripDropDownButton1.Text = "Misc";
            // 
            // exportAllImagesToolStripMenuItem
            // 
            exportAllImagesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aToolStripMenuItem, asTiffToolStripMenuItem1, asJpgToolStripMenuItem, asPngToolStripMenuItem1 });
            exportAllImagesToolStripMenuItem.Name = "exportAllImagesToolStripMenuItem";
            exportAllImagesToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            exportAllImagesToolStripMenuItem.Text = "Export All Image";
            // 
            // aToolStripMenuItem
            // 
            aToolStripMenuItem.Name = "aToolStripMenuItem";
            aToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            aToolStripMenuItem.Text = "As Bmp";
            aToolStripMenuItem.Click += OnClick_SaveAllBmp;
            // 
            // asTiffToolStripMenuItem1
            // 
            asTiffToolStripMenuItem1.Name = "asTiffToolStripMenuItem1";
            asTiffToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem1.Text = "As Tiff";
            asTiffToolStripMenuItem1.Click += OnClick_SaveAllTiff;
            // 
            // asJpgToolStripMenuItem
            // 
            asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem.Text = "As Jpg";
            asJpgToolStripMenuItem.Click += OnClick_SaveAllJpg;
            // 
            // asPngToolStripMenuItem1
            // 
            asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            asPngToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem1.Text = "As Png";
            asPngToolStripMenuItem1.Click += OnClick_SaveAllPng;
            // 
            // ChangeBackgroundColorToolStripMenuItem
            // 
            ChangeBackgroundColorToolStripMenuItem.Name = "ChangeBackgroundColorToolStripMenuItem";
            ChangeBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            ChangeBackgroundColorToolStripMenuItem.Text = "Change background color";
            ChangeBackgroundColorToolStripMenuItem.Click += ChangeBackgroundColorToolStripMenuItem_Click;
            // 
            // UseTransparencyForPNGToolStripMenuItem
            // 
            UseTransparencyForPNGToolStripMenuItem.AutoToolTip = true;
            UseTransparencyForPNGToolStripMenuItem.Checked = true;
            UseTransparencyForPNGToolStripMenuItem.CheckOnClick = true;
            UseTransparencyForPNGToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            UseTransparencyForPNGToolStripMenuItem.Name = "UseTransparencyForPNGToolStripMenuItem";
            UseTransparencyForPNGToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            UseTransparencyForPNGToolStripMenuItem.Text = "Use transparency for PNG";
            UseTransparencyForPNGToolStripMenuItem.ToolTipText = "When checked exported PNG files will have transparent background\r\n";
            UseTransparencyForPNGToolStripMenuItem.CheckedChanged += UseTransparencyForPNGToolStripMenuItem_CheckedChanged;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(209, 6);
            // 
            // exportAllPartsToolStripMenuItem
            // 
            exportAllPartsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { toTextFileToolStripMenuItem1, toUOAFileToolStripMenuItem, toWSCFileToolStripMenuItem, toCSVFileToolStripMenuItem, toUOX3FileToolStripMenuItem, toXMLFileToolStripMenuItem });
            exportAllPartsToolStripMenuItem.Name = "exportAllPartsToolStripMenuItem";
            exportAllPartsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            exportAllPartsToolStripMenuItem.Text = "Export All Parts";
            // 
            // toTextFileToolStripMenuItem1
            // 
            toTextFileToolStripMenuItem1.Name = "toTextFileToolStripMenuItem1";
            toTextFileToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            toTextFileToolStripMenuItem1.Text = "To Text File";
            toTextFileToolStripMenuItem1.Click += OnClick_SaveAllText;
            // 
            // toUOAFileToolStripMenuItem
            // 
            toUOAFileToolStripMenuItem.Name = "toUOAFileToolStripMenuItem";
            toUOAFileToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            toUOAFileToolStripMenuItem.Text = "To UOA File";
            toUOAFileToolStripMenuItem.Click += OnClick_SaveAllUOA;
            // 
            // toWSCFileToolStripMenuItem
            // 
            toWSCFileToolStripMenuItem.Name = "toWSCFileToolStripMenuItem";
            toWSCFileToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            toWSCFileToolStripMenuItem.Text = "To WSC File";
            toWSCFileToolStripMenuItem.Click += OnClick_SaveAllWSC;
            // 
            // toCSVFileToolStripMenuItem
            // 
            toCSVFileToolStripMenuItem.Name = "toCSVFileToolStripMenuItem";
            toCSVFileToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            toCSVFileToolStripMenuItem.Text = "To CSV File";
            toCSVFileToolStripMenuItem.Click += OnClick_SaveAllCSV;
            // 
            // toUOX3FileToolStripMenuItem
            // 
            toUOX3FileToolStripMenuItem.Name = "toUOX3FileToolStripMenuItem";
            toUOX3FileToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            toUOX3FileToolStripMenuItem.Text = "To UOX3 File";
            toUOX3FileToolStripMenuItem.Click += OnClick_SaveAllUox3;
            // 
            // toXMLFileToolStripMenuItem
            // 
            toXMLFileToolStripMenuItem.Name = "toXMLFileToolStripMenuItem";
            toXMLFileToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            toXMLFileToolStripMenuItem.Text = "To XML File (CentrED+)";
            toXMLFileToolStripMenuItem.Click += OnClick_SaveAllToXML;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(209, 6);
            // 
            // saveToolStripMenuItem1
            // 
            saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            saveToolStripMenuItem1.Size = new System.Drawing.Size(212, 22);
            saveToolStripMenuItem1.Text = "Save";
            saveToolStripMenuItem1.Click += OnClickSave;
            // 
            // tabControl3
            // 
            tabControl3.Controls.Add(tabPage5);
            tabControl3.Controls.Add(tabPage6);
            tabControl3.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl3.Location = new System.Drawing.Point(0, 0);
            tabControl3.Margin = new System.Windows.Forms.Padding(0);
            tabControl3.Name = "tabControl3";
            tabControl3.SelectedIndex = 0;
            tabControl3.Size = new System.Drawing.Size(505, 442);
            tabControl3.TabIndex = 1;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(splitContainer3);
            tabPage5.Location = new System.Drawing.Point(4, 24);
            tabPage5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage5.Size = new System.Drawing.Size(497, 414);
            tabPage5.TabIndex = 0;
            tabPage5.Text = "Graphic";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer3.Location = new System.Drawing.Point(4, 3);
            splitContainer3.Margin = new System.Windows.Forms.Padding(0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(HeightChangeMulti);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(splitContainer4);
            splitContainer3.Size = new System.Drawing.Size(489, 408);
            splitContainer3.SplitterDistance = 41;
            splitContainer3.SplitterWidth = 1;
            splitContainer3.TabIndex = 0;
            // 
            // HeightChangeMulti
            // 
            HeightChangeMulti.Dock = System.Windows.Forms.DockStyle.Fill;
            HeightChangeMulti.Location = new System.Drawing.Point(0, 0);
            HeightChangeMulti.Margin = new System.Windows.Forms.Padding(0);
            HeightChangeMulti.MaximumSize = new System.Drawing.Size(0, 38);
            HeightChangeMulti.MinimumSize = new System.Drawing.Size(0, 38);
            HeightChangeMulti.Name = "HeightChangeMulti";
            HeightChangeMulti.Size = new System.Drawing.Size(489, 38);
            HeightChangeMulti.TabIndex = 0;
            HeightChangeMulti.ValueChanged += OnValue_HeightChangeMulti;
            // 
            // splitContainer4
            // 
            splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer4.Location = new System.Drawing.Point(0, 0);
            splitContainer4.Margin = new System.Windows.Forms.Padding(0);
            splitContainer4.Name = "splitContainer4";
            splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            splitContainer4.Panel1.Controls.Add(MultiPictureBox);
            // 
            // splitContainer4.Panel2
            // 
            splitContainer4.Panel2.Controls.Add(statusMulti);
            splitContainer4.Panel2MinSize = 22;
            splitContainer4.Size = new System.Drawing.Size(489, 366);
            splitContainer4.SplitterDistance = 322;
            splitContainer4.SplitterWidth = 1;
            splitContainer4.TabIndex = 1;
            // 
            // MultiPictureBox
            // 
            MultiPictureBox.BackColor = System.Drawing.Color.White;
            MultiPictureBox.ContextMenuStrip = contextMenuStrip1;
            MultiPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            MultiPictureBox.Location = new System.Drawing.Point(0, 0);
            MultiPictureBox.Margin = new System.Windows.Forms.Padding(0);
            MultiPictureBox.Name = "MultiPictureBox";
            MultiPictureBox.Size = new System.Drawing.Size(489, 322);
            MultiPictureBox.TabIndex = 0;
            MultiPictureBox.TabStop = false;
            MultiPictureBox.Paint += OnPaint_MultiPic;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { extractImageToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(153, 26);
            // 
            // extractImageToolStripMenuItem
            // 
            extractImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { asBmpToolStripMenuItem, asTiffToolStripMenuItem, asJpgToolStripMenuItem1, asPngToolStripMenuItem });
            extractImageToolStripMenuItem.Name = "extractImageToolStripMenuItem";
            extractImageToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            extractImageToolStripMenuItem.Text = "extract Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asBmpToolStripMenuItem.Text = "As Bmp";
            asBmpToolStripMenuItem.Click += Extract_Image_ClickBmp;
            // 
            // asTiffToolStripMenuItem
            // 
            asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asTiffToolStripMenuItem.Text = "As Tiff";
            asTiffToolStripMenuItem.Click += Extract_Image_ClickTiff;
            // 
            // asJpgToolStripMenuItem1
            // 
            asJpgToolStripMenuItem1.Name = "asJpgToolStripMenuItem1";
            asJpgToolStripMenuItem1.Size = new System.Drawing.Size(115, 22);
            asJpgToolStripMenuItem1.Text = "As Jpg";
            asJpgToolStripMenuItem1.Click += Extract_Image_ClickJpg;
            // 
            // asPngToolStripMenuItem
            // 
            asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            asPngToolStripMenuItem.Text = "As Png";
            asPngToolStripMenuItem.Click += Extract_Image_ClickPng;
            // 
            // statusMulti
            // 
            statusMulti.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { StatusMultiText });
            statusMulti.Location = new System.Drawing.Point(0, 21);
            statusMulti.Name = "statusMulti";
            statusMulti.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusMulti.Size = new System.Drawing.Size(489, 22);
            statusMulti.TabIndex = 0;
            statusMulti.Text = "statusStrip2";
            // 
            // StatusMultiText
            // 
            StatusMultiText.Name = "StatusMultiText";
            StatusMultiText.Size = new System.Drawing.Size(87, 17);
            StatusMultiText.Text = "statusMultiText";
            // 
            // tabPage6
            // 
            tabPage6.Controls.Add(MultiComponentBox);
            tabPage6.Location = new System.Drawing.Point(4, 24);
            tabPage6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage6.Name = "tabPage6";
            tabPage6.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tabPage6.Size = new System.Drawing.Size(497, 414);
            tabPage6.TabIndex = 1;
            tabPage6.Text = "Component List";
            tabPage6.UseVisualStyleBackColor = true;
            // 
            // MultiComponentBox
            // 
            MultiComponentBox.Dock = System.Windows.Forms.DockStyle.Fill;
            MultiComponentBox.Location = new System.Drawing.Point(4, 3);
            MultiComponentBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MultiComponentBox.Name = "MultiComponentBox";
            MultiComponentBox.ReadOnly = true;
            MultiComponentBox.Size = new System.Drawing.Size(489, 408);
            MultiComponentBox.TabIndex = 0;
            MultiComponentBox.Text = "";
            // 
            // colorDialog
            // 
            colorDialog.Color = System.Drawing.Color.White;
            // 
            // MultisControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer2);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "MultisControl";
            Size = new System.Drawing.Size(755, 442);
            Load += OnLoad;
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            contextMenuStrip2.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            tabControl3.ResumeLayout(false);
            tabPage5.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel1.PerformLayout();
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)HeightChangeMulti).EndInit();
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.Panel2.ResumeLayout(false);
            splitContainer4.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer4).EndInit();
            splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MultiPictureBox).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            statusMulti.ResumeLayout(false);
            statusMulti.PerformLayout();
            tabPage6.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem exportAllImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllPartsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractImageToolStripMenuItem;
        private System.Windows.Forms.TrackBar HeightChangeMulti;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.RichTextBox MultiComponentBox;
        private System.Windows.Forms.PictureBox MultiPictureBox;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.StatusStrip statusMulti;
        private System.Windows.Forms.ToolStripStatusLabel StatusMultiText;
        private System.Windows.Forms.TabControl tabControl3;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripMenuItem toTextfileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toTextFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toUOAFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toUOAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toWSCFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toWscToolStripMenuItem;
        private System.Windows.Forms.TreeView TreeViewMulti;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.ToolStripMenuItem ChangeBackgroundColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem UseTransparencyForPNGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toCsvToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toCSVFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toUOX3FileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toUOX3ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toXMLFileToolStripMenuItem;
    }
}
