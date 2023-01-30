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

namespace UoFiddler.Controls.Forms
{
    partial class ItemDetailForm
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
            
            if (_showForm != null)
            {
                _showForm.Close();
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Graphic = new System.Windows.Forms.PictureBox();
            this.ItemDetailMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asJpgToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setHueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Data = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Graphic)).BeginInit();
            this.ItemDetailMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Graphic);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Data);
            this.splitContainer1.Size = new System.Drawing.Size(356, 325);
            this.splitContainer1.SplitterDistance = 71;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // Graphic
            // 
            this.Graphic.ContextMenuStrip = this.ItemDetailMenuStrip;
            this.Graphic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Graphic.Location = new System.Drawing.Point(0, 0);
            this.Graphic.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Graphic.Name = "Graphic";
            this.Graphic.Size = new System.Drawing.Size(356, 71);
            this.Graphic.TabIndex = 0;
            this.Graphic.TabStop = false;
            this.Graphic.SizeChanged += new System.EventHandler(this.OnSizeChange);
            this.Graphic.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            // 
            // ItemDetailMenuStrip
            // 
            this.ItemDetailMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractImageToolStripMenuItem,
            this.setHueToolStripMenuItem,
            this.animateToolStripMenuItem});
            this.ItemDetailMenuStrip.Name = "contextMenuStrip1";
            this.ItemDetailMenuStrip.Size = new System.Drawing.Size(151, 70);
            // 
            // extractImageToolStripMenuItem
            // 
            this.extractImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem,
            this.asJpgToolStripMenuItem,
            this.asPngToolStripMenuItem});
            this.extractImageToolStripMenuItem.Name = "extractImageToolStripMenuItem";
            this.extractImageToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.extractImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickTiff);
            // 
            // asJpgToolStripMenuItem
            // 
            this.asJpgToolStripMenuItem.Name = "asJpgToolStripMenuItem";
            this.asJpgToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asJpgToolStripMenuItem.Text = "As Jpg";
            this.asJpgToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickJpg);
            // 
            // asPngToolStripMenuItem
            // 
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asPngToolStripMenuItem.Text = "As Png";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.Extract_Image_ClickPng);
            // 
            // setHueToolStripMenuItem
            // 
            this.setHueToolStripMenuItem.Name = "setHueToolStripMenuItem";
            this.setHueToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.setHueToolStripMenuItem.Text = "Set Hue";
            this.setHueToolStripMenuItem.Click += new System.EventHandler(this.OnClick_Hue);
            // 
            // animateToolStripMenuItem
            // 
            this.animateToolStripMenuItem.Name = "animateToolStripMenuItem";
            this.animateToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
            this.animateToolStripMenuItem.Text = "Animate";
            this.animateToolStripMenuItem.Click += new System.EventHandler(this.OnClickAnimate);
            // 
            // Data
            // 
            this.Data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Data.Location = new System.Drawing.Point(0, 0);
            this.Data.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Data.Name = "Data";
            this.Data.ReadOnly = true;
            this.Data.Size = new System.Drawing.Size(356, 253);
            this.Data.TabIndex = 0;
            this.Data.Text = "";
            // 
            // ItemDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(356, 325);
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ItemDetailForm";
            this.Text = "ItemDetail";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClose);
            this.Load += new System.EventHandler(this.OnLoad);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Graphic)).EndInit();
            this.ItemDetailMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem animateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ItemDetailMenuStrip;
        private System.Windows.Forms.RichTextBox Data;
        private System.Windows.Forms.ToolStripMenuItem extractImageToolStripMenuItem;
        private System.Windows.Forms.PictureBox Graphic;
        private System.Windows.Forms.ToolStripMenuItem setHueToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStripMenuItem asJpgToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
    }
}