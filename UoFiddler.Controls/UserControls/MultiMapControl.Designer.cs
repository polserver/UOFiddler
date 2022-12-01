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
    partial class MultiMapControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiMapControl));
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.multiMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet00ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet01ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet02ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet03ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet04ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facet05ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.multiMapFromImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facetFromImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton3 = new System.Windows.Forms.ToolStripDropDownButton();
            this.asBmpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asTiffToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.asPngToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 25);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(629, 362);
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            this.pictureBox.Resize += new System.EventHandler(this.OnResize);
            // 
            // hScrollBar
            // 
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar.Location = new System.Drawing.Point(0, 370);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(612, 17);
            this.hScrollBar.TabIndex = 1;
            this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HandleScroll);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(612, 25);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 362);
            this.vScrollBar.TabIndex = 2;
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.HandleScroll);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripDropDownButton2,
            this.toolStripDropDownButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(629, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.multiMapToolStripMenuItem,
            this.facet00ToolStripMenuItem,
            this.facet01ToolStripMenuItem,
            this.facet02ToolStripMenuItem,
            this.facet03ToolStripMenuItem,
            this.facet04ToolStripMenuItem,
            this.facet05ToolStripMenuItem});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(52, 22);
            this.toolStripDropDownButton1.Text = "Load..";
            // 
            // multiMapToolStripMenuItem
            // 
            this.multiMapToolStripMenuItem.Name = "multiMapToolStripMenuItem";
            this.multiMapToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.multiMapToolStripMenuItem.Text = "MultiMap";
            this.multiMapToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            // 
            // facet00ToolStripMenuItem
            // 
            this.facet00ToolStripMenuItem.Name = "facet00ToolStripMenuItem";
            this.facet00ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.facet00ToolStripMenuItem.Text = "Facet00";
            this.facet00ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            // 
            // facet01ToolStripMenuItem
            // 
            this.facet01ToolStripMenuItem.Name = "facet01ToolStripMenuItem";
            this.facet01ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.facet01ToolStripMenuItem.Tag = "";
            this.facet01ToolStripMenuItem.Text = "Facet01";
            this.facet01ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            // 
            // facet02ToolStripMenuItem
            // 
            this.facet02ToolStripMenuItem.Name = "facet02ToolStripMenuItem";
            this.facet02ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.facet02ToolStripMenuItem.Tag = "";
            this.facet02ToolStripMenuItem.Text = "Facet02";
            this.facet02ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            // 
            // facet03ToolStripMenuItem
            // 
            this.facet03ToolStripMenuItem.Name = "facet03ToolStripMenuItem";
            this.facet03ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.facet03ToolStripMenuItem.Tag = "";
            this.facet03ToolStripMenuItem.Text = "Facet03";
            this.facet03ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            // 
            // facet04ToolStripMenuItem
            // 
            this.facet04ToolStripMenuItem.Name = "facet04ToolStripMenuItem";
            this.facet04ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.facet04ToolStripMenuItem.Tag = "";
            this.facet04ToolStripMenuItem.Text = "Facet04";
            this.facet04ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            // 
            // facet05ToolStripMenuItem
            // 
            this.facet05ToolStripMenuItem.Name = "facet05ToolStripMenuItem";
            this.facet05ToolStripMenuItem.Size = new System.Drawing.Size(126, 22);
            this.facet05ToolStripMenuItem.Tag = "";
            this.facet05ToolStripMenuItem.Text = "Facet05";
            this.facet05ToolStripMenuItem.Click += new System.EventHandler(this.ShowImage);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.multiMapFromImageToolStripMenuItem,
            this.facetFromImageToolStripMenuItem});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(73, 22);
            this.toolStripDropDownButton2.Text = "Generate..";
            // 
            // multiMapFromImageToolStripMenuItem
            // 
            this.multiMapFromImageToolStripMenuItem.Name = "multiMapFromImageToolStripMenuItem";
            this.multiMapFromImageToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.multiMapFromImageToolStripMenuItem.Text = "MultiMap from Image";
            this.multiMapFromImageToolStripMenuItem.Click += new System.EventHandler(this.OnClickGenerateRLE);
            // 
            // facetFromImageToolStripMenuItem
            // 
            this.facetFromImageToolStripMenuItem.Name = "facetFromImageToolStripMenuItem";
            this.facetFromImageToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.facetFromImageToolStripMenuItem.Text = "Facet from Image";
            this.facetFromImageToolStripMenuItem.Click += new System.EventHandler(this.OnClickGenerateFacetFromImage);
            // 
            // toolStripDropDownButton3
            // 
            this.toolStripDropDownButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.asBmpToolStripMenuItem1,
            this.asTiffToolStripMenuItem1,
            this.asPngToolStripMenuItem,
            this.asPngToolStripMenuItem1});
            this.toolStripDropDownButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton3.Image")));
            this.toolStripDropDownButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton3.Name = "toolStripDropDownButton3";
            this.toolStripDropDownButton3.Size = new System.Drawing.Size(60, 22);
            this.toolStripDropDownButton3.Text = "Export..";
            // 
            // asBmpToolStripMenuItem1
            // 
            this.asBmpToolStripMenuItem1.Name = "asBmpToolStripMenuItem1";
            this.asBmpToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.asBmpToolStripMenuItem1.Text = "As Bmp..";
            this.asBmpToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExportBmp);
            // 
            // asTiffToolStripMenuItem1
            // 
            this.asTiffToolStripMenuItem1.Name = "asTiffToolStripMenuItem1";
            this.asTiffToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.asTiffToolStripMenuItem1.Text = "As Tiff..";
            this.asTiffToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExportTiff);
            // 
            // asPngToolStripMenuItem
            // 
            this.asPngToolStripMenuItem.Name = "asPngToolStripMenuItem";
            this.asPngToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.asPngToolStripMenuItem.Text = "As Jpg..";
            this.asPngToolStripMenuItem.Click += new System.EventHandler(this.OnClickExportJpg);
            // 
            // asPngToolStripMenuItem1
            // 
            this.asPngToolStripMenuItem1.Name = "asPngToolStripMenuItem1";
            this.asPngToolStripMenuItem1.Size = new System.Drawing.Size(121, 22);
            this.asPngToolStripMenuItem1.Text = "As Png..";
            this.asPngToolStripMenuItem1.Click += new System.EventHandler(this.OnClickExportPng);
            // 
            // MultiMapControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hScrollBar);
            this.Controls.Add(this.vScrollBar);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.toolStrip1);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MultiMapControl";
            this.Size = new System.Drawing.Size(629, 387);
            this.Load += new System.EventHandler(this.OnLoad);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem asBmpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem asPngToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem asTiffToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem facet00ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet01ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet02ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet03ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet04ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facet05ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facetFromImageToolStripMenuItem;
        private System.Windows.Forms.HScrollBar hScrollBar;
        private System.Windows.Forms.ToolStripMenuItem multiMapFromImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem multiMapToolStripMenuItem;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton3;
        private System.Windows.Forms.VScrollBar vScrollBar;
    }
}
