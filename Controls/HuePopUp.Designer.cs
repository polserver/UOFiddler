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
    partial class HuePopUp
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.HueOnlyGray = new System.Windows.Forms.ToolStripButton();
            this.control = new FiddlerControls.Hues();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.HueOnlyGray});
            this.toolStrip1.Location = new System.Drawing.Point(0, 265);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip1.Size = new System.Drawing.Size(520, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(26, 22);
            this.toolStripButton1.Text = "OK";
            this.toolStripButton1.Click += new System.EventHandler(this.Click_OK);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(40, 22);
            this.toolStripButton2.Text = "Clear";
            this.toolStripButton2.Click += new System.EventHandler(this.OnClick_Clear);
            // 
            // HueOnlyGray
            // 
            this.HueOnlyGray.CheckOnClick = true;
            this.HueOnlyGray.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.HueOnlyGray.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.HueOnlyGray.Name = "HueOnlyGray";
            this.HueOnlyGray.Size = new System.Drawing.Size(136, 22);
            this.HueOnlyGray.Text = "Hue Only Gray (+0x8000)";
            // 
            // control
            // 
            this.control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.control.ForeColor = System.Drawing.SystemColors.ControlText;
            this.control.Location = new System.Drawing.Point(0, 0);
            this.control.Name = "control";
            this.control.Padding = new System.Windows.Forms.Padding(1);
            this.control.Selected = 0;
            this.control.Size = new System.Drawing.Size(520, 265);
            this.control.TabIndex = 1;
            // 
            // HuePopUp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 290);
            this.Controls.Add(this.control);
            this.Controls.Add(this.toolStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "HuePopUp";
            this.Text = "Hue Picker";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private FiddlerControls.Hues control;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton HueOnlyGray;

    }
}