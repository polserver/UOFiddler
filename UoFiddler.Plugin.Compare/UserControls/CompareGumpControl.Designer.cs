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
            this.components = new System.ComponentModel.Container();
            this.tileView1 = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            this.tileView2 = new UoFiddler.Controls.UserControls.TileView.TileViewControl();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyGump2To1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.textBoxSecondDir = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            //
            // tileView1
            //
            this.tileView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tileView1.Location = new System.Drawing.Point(0, 0);
            this.tileView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tileView1.Name = "tileView1";
            this.tileView1.Size = new System.Drawing.Size(174, 320);
            this.tileView1.TabIndex = 0;
            this.tileView1.TileSize = new System.Drawing.Size(174, 60);
            this.tileView1.TileMargin = new System.Windows.Forms.Padding(0);
            this.tileView1.TilePadding = new System.Windows.Forms.Padding(0);
            this.tileView1.TileBorderWidth = 0f;
            this.tileView1.TileHighLightOpacity = 0.0;
            this.tileView1.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(this.OnDrawItem1);
            this.tileView1.FocusSelectionChanged += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs>(this.OnFocusChanged1);
            this.tileView1.SizeChanged += new System.EventHandler(this.OnTileViewSizeChanged);
            //
            // tileView2
            //
            this.tileView2.ContextMenuStrip = this.contextMenuStrip1;
            this.tileView2.Dock = System.Windows.Forms.DockStyle.Right;
            this.tileView2.Location = new System.Drawing.Point(556, 0);
            this.tileView2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tileView2.Name = "tileView2";
            this.tileView2.Size = new System.Drawing.Size(174, 320);
            this.tileView2.TabIndex = 1;
            this.tileView2.TileSize = new System.Drawing.Size(174, 60);
            this.tileView2.TileMargin = new System.Windows.Forms.Padding(0);
            this.tileView2.TilePadding = new System.Windows.Forms.Padding(0);
            this.tileView2.TileBorderWidth = 0f;
            this.tileView2.TileHighLightOpacity = 0.0;
            this.tileView2.DrawItem += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.DrawTileListItemEventArgs>(this.OnDrawItem2);
            this.tileView2.FocusSelectionChanged += new System.EventHandler<UoFiddler.Controls.UserControls.TileView.TileViewControl.ListViewFocusedItemSelectionChangedEventArgs>(this.OnFocusChanged2);
            this.tileView2.SizeChanged += new System.EventHandler(this.OnTileViewSizeChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractAsToolStripMenuItem,
            this.copyGump2To1ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(171, 48);
            // 
            // extractAsToolStripMenuItem
            // 
            this.extractAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tiffToolStripMenuItem,
            this.bmpToolStripMenuItem});
            this.extractAsToolStripMenuItem.Name = "extractAsToolStripMenuItem";
            this.extractAsToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.extractAsToolStripMenuItem.Text = "Export Image..";
            // 
            // tiffToolStripMenuItem
            // 
            this.tiffToolStripMenuItem.Name = "tiffToolStripMenuItem";
            this.tiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.tiffToolStripMenuItem.Text = "As Bmp";
            this.tiffToolStripMenuItem.Click += new System.EventHandler(this.Export_Bmp);
            // 
            // bmpToolStripMenuItem
            // 
            this.bmpToolStripMenuItem.Name = "bmpToolStripMenuItem";
            this.bmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.bmpToolStripMenuItem.Text = "As Tiff";
            this.bmpToolStripMenuItem.Click += new System.EventHandler(this.Export_Tiff);
            // 
            // copyGump2To1ToolStripMenuItem
            // 
            this.copyGump2To1ToolStripMenuItem.Name = "copyGump2To1ToolStripMenuItem";
            this.copyGump2To1ToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.copyGump2To1ToolStripMenuItem.Text = "Copy Gump 2 to 1";
            this.copyGump2To1ToolStripMenuItem.Click += new System.EventHandler(this.OnClickCopy);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(174, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(382, 320);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(4, 3);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(374, 154);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(4, 163);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(374, 154);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
            this.splitContainer1.Panel1.Controls.Add(this.tileView2);
            this.splitContainer1.Panel1.Controls.Add(this.tileView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel2.Controls.Add(this.button2);
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Panel2.Controls.Add(this.textBoxSecondDir);
            this.splitContainer1.Size = new System.Drawing.Size(730, 378);
            this.splitContainer1.SplitterDistance = 320;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 3;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(493, 18);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(143, 19);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Show only Differences";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.Click += new System.EventHandler(this.ShowDiff_OnClick);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(399, 14);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 27);
            this.button2.TabIndex = 2;
            this.button2.Text = "Load";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.Load_Click);
            // 
            // button1
            // 
            this.button1.AutoSize = true;
            this.button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button1.Location = new System.Drawing.Point(362, 14);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(26, 25);
            this.button1.TabIndex = 1;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Browse_OnClick);
            // 
            // textBoxSecondDir
            // 
            this.textBoxSecondDir.Location = new System.Drawing.Point(175, 16);
            this.textBoxSecondDir.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBoxSecondDir.Name = "textBoxSecondDir";
            this.textBoxSecondDir.Size = new System.Drawing.Size(179, 23);
            this.textBoxSecondDir.TabIndex = 0;
            // 
            // CompareGumpControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "CompareGumpControl";
            this.Size = new System.Drawing.Size(730, 378);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem bmpToolStripMenuItem;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyGump2To1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAsToolStripMenuItem;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileView1;
        private UoFiddler.Controls.UserControls.TileView.TileViewControl tileView2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxSecondDir;
        private System.Windows.Forms.ToolStripMenuItem tiffToolStripMenuItem;
    }
}
