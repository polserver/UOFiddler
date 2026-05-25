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

namespace UoFiddler.Plugin.Compare.UserControls
{
    partial class CompareGumpControl
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
            tileView1 = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            tileView2 = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            extractAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            bmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            jpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyGump2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            pictureBox2 = new System.Windows.Forms.PictureBox();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            checkBox1 = new System.Windows.Forms.CheckBox();
            chkMultiSelect = new System.Windows.Forms.CheckBox();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            textBoxSecondDir = new System.Windows.Forms.TextBox();
            comboBoxFileMode = new System.Windows.Forms.ComboBox();
            contextMenuStrip1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // tileView1
            // 
            tileView1.Dock = System.Windows.Forms.DockStyle.Left;
            tileView1.Location = new System.Drawing.Point(0, 0);
            tileView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tileView1.Name = "tileView1";
            tileView1.Size = new System.Drawing.Size(174, 309);
            tileView1.TabIndex = 0;
            tileView1.TileHighLightOpacity = 0D;
            tileView1.FocusSelectionChanged += OnFocusChanged1;
            tileView1.DrawItem += OnDrawItem1;
            tileView1.SizeChanged += OnTileViewSizeChanged;
            // 
            // tileView2
            // 
            tileView2.ContextMenuStrip = contextMenuStrip1;
            tileView2.Dock = System.Windows.Forms.DockStyle.Right;
            tileView2.Location = new System.Drawing.Point(556, 0);
            tileView2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tileView2.Name = "tileView2";
            tileView2.Size = new System.Drawing.Size(174, 309);
            tileView2.TabIndex = 1;
            tileView2.TileHighLightOpacity = 0D;
            tileView2.FocusSelectionChanged += OnFocusChanged2;
            tileView2.DrawItem += OnDrawItem2;
            tileView2.SizeChanged += OnTileViewSizeChanged;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { extractAsToolStripMenuItem, copyGump2To1ToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(173, 48);
            // 
            // extractAsToolStripMenuItem
            // 
            extractAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { tiffToolStripMenuItem, bmpToolStripMenuItem, jpgToolStripMenuItem, pngToolStripMenuItem });
            extractAsToolStripMenuItem.Name = "extractAsToolStripMenuItem";
            extractAsToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            extractAsToolStripMenuItem.Text = "Export Image..";
            // 
            // tiffToolStripMenuItem
            // 
            tiffToolStripMenuItem.Name = "tiffToolStripMenuItem";
            tiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            tiffToolStripMenuItem.Text = "As Bmp";
            tiffToolStripMenuItem.Click += Export_Bmp;
            // 
            // bmpToolStripMenuItem
            // 
            bmpToolStripMenuItem.Name = "bmpToolStripMenuItem";
            bmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            bmpToolStripMenuItem.Text = "As Tiff";
            bmpToolStripMenuItem.Click += Export_Tiff;
            // 
            // jpgToolStripMenuItem
            // 
            jpgToolStripMenuItem.Name = "jpgToolStripMenuItem";
            jpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            jpgToolStripMenuItem.Text = "As Jpg";
            jpgToolStripMenuItem.Click += Export_Jpg;
            // 
            // pngToolStripMenuItem
            // 
            pngToolStripMenuItem.Name = "pngToolStripMenuItem";
            pngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            pngToolStripMenuItem.Text = "As Png";
            pngToolStripMenuItem.Click += Export_Png;
            // 
            // copyGump2To1ToolStripMenuItem
            // 
            copyGump2To1ToolStripMenuItem.Name = "copyGump2To1ToolStripMenuItem";
            copyGump2To1ToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            copyGump2To1ToolStripMenuItem.Text = "Copy Gump to left";
            copyGump2To1ToolStripMenuItem.Click += OnClickCopy;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(pictureBox2, 0, 1);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(174, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new System.Drawing.Size(382, 309);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // pictureBox1
            // 
            pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox1.Location = new System.Drawing.Point(4, 3);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(374, 148);
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox2.Location = new System.Drawing.Point(4, 157);
            pictureBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new System.Drawing.Size(374, 149);
            pictureBox2.TabIndex = 1;
            pictureBox2.TabStop = false;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tableLayoutPanel1);
            splitContainer1.Panel1.Controls.Add(tileView2);
            splitContainer1.Panel1.Controls.Add(tileView1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(checkBox1);
            splitContainer1.Panel2.Controls.Add(chkMultiSelect);
            splitContainer1.Panel2.Controls.Add(button2);
            splitContainer1.Panel2.Controls.Add(button1);
            splitContainer1.Panel2.Controls.Add(textBoxSecondDir);
            splitContainer1.Panel2.Controls.Add(comboBoxFileMode);
            splitContainer1.Size = new System.Drawing.Size(730, 378);
            splitContainer1.SplitterDistance = 309;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 3;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(493, 18);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(143, 19);
            checkBox1.TabIndex = 3;
            checkBox1.Text = "Show only Differences";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.Click += ShowDiff_OnClick;
            //
            // chkMultiSelect
            //
            chkMultiSelect.AutoSize = true;
            chkMultiSelect.Location = new System.Drawing.Point(493, 41);
            chkMultiSelect.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkMultiSelect.Name = "chkMultiSelect";
            chkMultiSelect.Size = new System.Drawing.Size(90, 19);
            chkMultiSelect.TabIndex = 10;
            chkMultiSelect.Text = "Multi-Select";
            chkMultiSelect.UseVisualStyleBackColor = true;
            chkMultiSelect.CheckedChanged += OnChangeMultiSelect;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(399, 14);
            button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 27);
            button2.TabIndex = 2;
            button2.Text = "Load";
            button2.UseVisualStyleBackColor = true;
            button2.Click += Load_Click;
            // 
            // button1
            // 
            button1.AutoSize = true;
            button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            button1.Location = new System.Drawing.Point(362, 14);
            button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(26, 25);
            button1.TabIndex = 1;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            button1.Click += Browse_OnClick;
            // 
            // textBoxSecondDir
            // 
            textBoxSecondDir.Location = new System.Drawing.Point(175, 16);
            textBoxSecondDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            textBoxSecondDir.Name = "textBoxSecondDir";
            textBoxSecondDir.Size = new System.Drawing.Size(179, 23);
            textBoxSecondDir.TabIndex = 0;
            // 
            // comboBoxFileMode
            // 
            comboBoxFileMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxFileMode.FormattingEnabled = true;
            comboBoxFileMode.Items.AddRange(new object[] { "Auto", "MUL", "UOP" });
            comboBoxFileMode.Location = new System.Drawing.Point(99, 16);
            comboBoxFileMode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            comboBoxFileMode.Name = "comboBoxFileMode";
            comboBoxFileMode.Size = new System.Drawing.Size(70, 23);
            comboBoxFileMode.TabIndex = 4;
            // 
            // CompareGumpControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "CompareGumpControl";
            Size = new System.Drawing.Size(730, 378);
            Load += OnLoad;
            contextMenuStrip1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem bmpToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox chkMultiSelect;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyGump2To1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem jpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pngToolStripMenuItem;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileView1;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileView2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.ToolStripMenuItem tiffToolStripMenuItem;
        private System.Windows.Forms.ComboBox comboBoxFileMode;
    }
}
