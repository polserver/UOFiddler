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

namespace UoFiddler.Forms
{
    partial class PathSettingsForm
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
            this.pgPaths = new System.Windows.Forms.PropertyGrid();
            this.tsPathSettingsMenu = new System.Windows.Forms.ToolStrip();
            this.tsBtnReloadPaths = new System.Windows.Forms.ToolStripButton();
            this.tsBtnSetPathManual = new System.Windows.Forms.ToolStripButton();
            this.tsTbRootPath = new System.Windows.Forms.ToolStripTextBox();
            this.tsPathSettingsMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgPaths
            // 
            this.pgPaths.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgPaths.HelpVisible = false;
            this.pgPaths.Location = new System.Drawing.Point(0, 25);
            this.pgPaths.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pgPaths.Name = "pgPaths";
            this.pgPaths.PropertySort = System.Windows.Forms.PropertySort.Alphabetical;
            this.pgPaths.Size = new System.Drawing.Size(750, 392);
            this.pgPaths.TabIndex = 0;
            this.pgPaths.ToolbarVisible = false;
            // 
            // tsPathSettingsMenu
            // 
            this.tsPathSettingsMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsPathSettingsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsBtnReloadPaths,
            this.tsBtnSetPathManual,
            this.tsTbRootPath});
            this.tsPathSettingsMenu.Location = new System.Drawing.Point(0, 0);
            this.tsPathSettingsMenu.Name = "tsPathSettingsMenu";
            this.tsPathSettingsMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tsPathSettingsMenu.Size = new System.Drawing.Size(750, 25);
            this.tsPathSettingsMenu.TabIndex = 1;
            this.tsPathSettingsMenu.Text = "toolStrip1";
            // 
            // tsBtnReloadPaths
            // 
            this.tsBtnReloadPaths.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsBtnReloadPaths.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnReloadPaths.Name = "tsBtnReloadPaths";
            this.tsBtnReloadPaths.Size = new System.Drawing.Size(79, 22);
            this.tsBtnReloadPaths.Text = "Reload paths";
            this.tsBtnReloadPaths.Click += new System.EventHandler(this.ReloadPath);
            // 
            // tsBtnSetPathManual
            // 
            this.tsBtnSetPathManual.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsBtnSetPathManual.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsBtnSetPathManual.Name = "tsBtnSetPathManual";
            this.tsBtnSetPathManual.Size = new System.Drawing.Size(97, 22);
            this.tsBtnSetPathManual.Text = "Set path manual";
            this.tsBtnSetPathManual.Click += new System.EventHandler(this.OnClickManual);
            // 
            // tsTbRootPath
            // 
            this.tsTbRootPath.Name = "tsTbRootPath";
            this.tsTbRootPath.Size = new System.Drawing.Size(408, 25);
            this.tsTbRootPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDownDir);
            // 
            // PathSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 417);
            this.Controls.Add(this.pgPaths);
            this.Controls.Add(this.tsPathSettingsMenu);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximumSize = new System.Drawing.Size(791, 917);
            this.MinimumSize = new System.Drawing.Size(744, 340);
            this.Name = "PathSettingsForm";
            this.Text = "Path Settings";
            this.tsPathSettingsMenu.ResumeLayout(false);
            this.tsPathSettingsMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgPaths;
        private System.Windows.Forms.ToolStripButton tsBtnReloadPaths;
        private System.Windows.Forms.ToolStripButton tsBtnSetPathManual;
        private System.Windows.Forms.ToolStrip tsPathSettingsMenu;
        private System.Windows.Forms.ToolStripTextBox tsTbRootPath;
    }
}