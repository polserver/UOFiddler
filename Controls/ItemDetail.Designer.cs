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

namespace FiddlerControls
{
    partial class ItemDetail
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
            if (showform != null)
                showform.Close();

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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asBmpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setHueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.animateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.Data = new System.Windows.Forms.RichTextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Graphic)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer1.Size = new System.Drawing.Size(292, 266);
            this.splitContainer1.SplitterDistance = 61;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // Graphic
            // 
            this.Graphic.ContextMenuStrip = this.contextMenuStrip1;
            this.Graphic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Graphic.Location = new System.Drawing.Point(0, 0);
            this.Graphic.Name = "Graphic";
            this.Graphic.Size = new System.Drawing.Size(292, 61);
            this.Graphic.TabIndex = 0;
            this.Graphic.TabStop = false;
            this.Graphic.Paint += new System.Windows.Forms.PaintEventHandler(this.onPaint);
            this.Graphic.SizeChanged += new System.EventHandler(this.OnSizeChange);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractImageToolStripMenuItem,
            this.setHueToolStripMenuItem,
            this.animateToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(154, 70);
            // 
            // extractImageToolStripMenuItem
            // 
            this.extractImageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem,
            this.asTiffToolStripMenuItem});
            this.extractImageToolStripMenuItem.Name = "extractImageToolStripMenuItem";
            this.extractImageToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.extractImageToolStripMenuItem.Text = "Export Image..";
            // 
            // asBmpToolStripMenuItem
            // 
            this.asBmpToolStripMenuItem.Name = "asBmpToolStripMenuItem";
            this.asBmpToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asBmpToolStripMenuItem.Text = "As Bmp";
            this.asBmpToolStripMenuItem.Click += new System.EventHandler(this.extract_Image_ClickBmp);
            // 
            // asTiffToolStripMenuItem
            // 
            this.asTiffToolStripMenuItem.Name = "asTiffToolStripMenuItem";
            this.asTiffToolStripMenuItem.Size = new System.Drawing.Size(115, 22);
            this.asTiffToolStripMenuItem.Text = "As Tiff";
            this.asTiffToolStripMenuItem.Click += new System.EventHandler(this.extract_Image_ClickTiff);
            // 
            // setHueToolStripMenuItem
            // 
            this.setHueToolStripMenuItem.Name = "setHueToolStripMenuItem";
            this.setHueToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.setHueToolStripMenuItem.Text = "Set Hue";
            this.setHueToolStripMenuItem.Click += new System.EventHandler(this.OnClick_Hue);
            // 
            // animateToolStripMenuItem
            // 
            this.animateToolStripMenuItem.Name = "animateToolStripMenuItem";
            this.animateToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.animateToolStripMenuItem.Text = "Animate";
            this.animateToolStripMenuItem.Click += new System.EventHandler(this.OnClickAnimate);
            // 
            // Data
            // 
            this.Data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Data.Location = new System.Drawing.Point(0, 0);
            this.Data.Name = "Data";
            this.Data.ReadOnly = true;
            this.Data.Size = new System.Drawing.Size(292, 204);
            this.Data.TabIndex = 0;
            this.Data.Text = "";
            // 
            // ItemDetail
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "ItemDetail";
            this.Text = "ItemDetail";
            this.Load += new System.EventHandler(this.onLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.onClose);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Graphic)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PictureBox Graphic;
        private System.Windows.Forms.RichTextBox Data;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem extractImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setHueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem animateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem;

    }
}