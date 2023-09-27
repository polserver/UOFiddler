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
    partial class AnimationListNewEntriesForm
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
            components = new System.ComponentModel.Container();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tvAnimationList = new System.Windows.Forms.TreeView();
            facingbar = new System.Windows.Forms.TrackBar();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(components);
            animateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            ComboBoxActionType = new System.Windows.Forms.ToolStripComboBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)facingbar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip2.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tvAnimationList);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(facingbar);
            splitContainer1.Panel2.Controls.Add(pictureBox1);
            splitContainer1.Panel2.Controls.Add(toolStrip1);
            splitContainer1.Size = new System.Drawing.Size(624, 361);
            splitContainer1.SplitterDistance = 202;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // tvAnimationList
            // 
            tvAnimationList.Dock = System.Windows.Forms.DockStyle.Fill;
            tvAnimationList.LabelEdit = true;
            tvAnimationList.Location = new System.Drawing.Point(0, 0);
            tvAnimationList.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tvAnimationList.Name = "tvAnimationList";
            tvAnimationList.ShowNodeToolTips = true;
            tvAnimationList.Size = new System.Drawing.Size(202, 361);
            tvAnimationList.TabIndex = 0;
            tvAnimationList.AfterSelect += OnAfterSelectTreeView;
            // 
            // facingbar
            // 
            facingbar.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            facingbar.AutoSize = false;
            facingbar.LargeChange = 1;
            facingbar.Location = new System.Drawing.Point(294, 339);
            facingbar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            facingbar.Maximum = 7;
            facingbar.Name = "facingbar";
            facingbar.Size = new System.Drawing.Size(121, 23);
            facingbar.TabIndex = 2;
            facingbar.Scroll += OnScrollFacing;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.White;
            pictureBox1.ContextMenuStrip = contextMenuStrip2;
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox1.Location = new System.Drawing.Point(0, 0);
            pictureBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(417, 336);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += OnPaint_PictureBox;
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { animateToolStripMenuItem });
            contextMenuStrip2.Name = "contextMenuStrip2";
            contextMenuStrip2.Size = new System.Drawing.Size(120, 26);
            // 
            // animateToolStripMenuItem
            // 
            animateToolStripMenuItem.CheckOnClick = true;
            animateToolStripMenuItem.Name = "animateToolStripMenuItem";
            animateToolStripMenuItem.Size = new System.Drawing.Size(119, 22);
            animateToolStripMenuItem.Text = "Animate";
            animateToolStripMenuItem.Click += OnClickAnimate;
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButton1, ComboBoxActionType });
            toolStrip1.Location = new System.Drawing.Point(0, 336);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            toolStrip1.Size = new System.Drawing.Size(417, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new System.Drawing.Size(33, 22);
            toolStripButton1.Text = "Add";
            toolStripButton1.Click += OnClickAdd;
            // 
            // ComboBoxActionType
            // 
            ComboBoxActionType.Items.AddRange(new object[] { "Monster", "Sea Monster", "Animal", "Human", "Equipment" });
            ComboBoxActionType.Name = "ComboBoxActionType";
            ComboBoxActionType.Size = new System.Drawing.Size(140, 25);
            ComboBoxActionType.SelectedIndexChanged += OnChangeType;
            // 
            // AnimationListNewEntriesForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(624, 361);
            Controls.Add(splitContainer1);
            DoubleBuffered = true;
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "AnimationListNewEntriesForm";
            Text = "Animationlist New Entries";
            Load += OnLoad;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)facingbar).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip2.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.ToolStripMenuItem animateToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox ComboBoxActionType;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.TrackBar facingbar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.TreeView tvAnimationList;
    }
}