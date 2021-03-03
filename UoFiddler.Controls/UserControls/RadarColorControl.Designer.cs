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
    partial class RadarColorControl
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
            this.treeViewItem = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.selectInItemsTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInTiledataTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewLand = new System.Windows.Forms.TreeView();
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBoxArt = new System.Windows.Forms.PictureBox();
            this.pictureBoxColor = new System.Windows.Forms.PictureBox();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.numericUpDownShortCol = new System.Windows.Forms.NumericUpDown();
            this.textBoxMeanFrom = new System.Windows.Forms.TextBox();
            this.textBoxMeanTo = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.numericUpDownB = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownG = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownR = new System.Windows.Forms.NumericUpDown();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonMean = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxArt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShortCol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownR)).BeginInit();
            this.SuspendLayout();
            // 
            // treeViewItem
            // 
            this.treeViewItem.ContextMenuStrip = this.contextMenuStrip1;
            this.treeViewItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewItem.HideSelection = false;
            this.treeViewItem.Location = new System.Drawing.Point(4, 4);
            this.treeViewItem.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.treeViewItem.Name = "treeViewItem";
            this.treeViewItem.Size = new System.Drawing.Size(261, 205);
            this.treeViewItem.TabIndex = 0;
            this.treeViewItem.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AfterSelectTreeViewItem);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectInItemsTabToolStripMenuItem,
            this.selectInTiledataTabToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(219, 52);
            // 
            // selectInItemsTabToolStripMenuItem
            // 
            this.selectInItemsTabToolStripMenuItem.Name = "selectInItemsTabToolStripMenuItem";
            this.selectInItemsTabToolStripMenuItem.Size = new System.Drawing.Size(218, 24);
            this.selectInItemsTabToolStripMenuItem.Text = "Select in Items tab";
            this.selectInItemsTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectItemsTab);
            // 
            // selectInTiledataTabToolStripMenuItem
            // 
            this.selectInTiledataTabToolStripMenuItem.Name = "selectInTiledataTabToolStripMenuItem";
            this.selectInTiledataTabToolStripMenuItem.Size = new System.Drawing.Size(218, 24);
            this.selectInTiledataTabToolStripMenuItem.Text = "Select in Tiledata tab";
            this.selectInTiledataTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectItemTiledataTab);
            // 
            // treeViewLand
            // 
            this.treeViewLand.ContextMenuStrip = this.contextMenuStrip2;
            this.treeViewLand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLand.HideSelection = false;
            this.treeViewLand.Location = new System.Drawing.Point(4, 4);
            this.treeViewLand.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.treeViewLand.Name = "treeViewLand";
            this.treeViewLand.Size = new System.Drawing.Size(261, 205);
            this.treeViewLand.TabIndex = 0;
            this.treeViewLand.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AfterSelectTreeViewLand);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.contextMenuStrip2.Name = "contextMenuStrip1";
            this.contextMenuStrip2.Size = new System.Drawing.Size(224, 52);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(223, 24);
            this.toolStripMenuItem1.Text = "Select in Landtiles tab";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.OnClickSelectLandTilesTab);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(223, 24);
            this.toolStripMenuItem2.Text = "Select in Tiledata tab";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.OnClickSelectLandTiledataTab);
            // 
            // pictureBoxArt
            // 
            this.pictureBoxArt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxArt.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxArt.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBoxArt.Name = "pictureBoxArt";
            this.pictureBoxArt.Size = new System.Drawing.Size(277, 163);
            this.pictureBoxArt.TabIndex = 0;
            this.pictureBoxArt.TabStop = false;
            // 
            // pictureBoxColor
            // 
            this.pictureBoxColor.Location = new System.Drawing.Point(4, 4);
            this.pictureBoxColor.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pictureBoxColor.Name = "pictureBoxColor";
            this.pictureBoxColor.Size = new System.Drawing.Size(184, 124);
            this.pictureBoxColor.TabIndex = 0;
            this.pictureBoxColor.TabStop = false;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.button6);
            this.splitContainer5.Panel2.Controls.Add(this.button5);
            this.splitContainer5.Panel2.Controls.Add(this.button4);
            this.splitContainer5.Panel2.Controls.Add(this.numericUpDownShortCol);
            this.splitContainer5.Panel2.Controls.Add(this.textBoxMeanFrom);
            this.splitContainer5.Panel2.Controls.Add(this.textBoxMeanTo);
            this.splitContainer5.Panel2.Controls.Add(this.button3);
            this.splitContainer5.Panel2.Controls.Add(this.numericUpDownB);
            this.splitContainer5.Panel2.Controls.Add(this.numericUpDownG);
            this.splitContainer5.Panel2.Controls.Add(this.numericUpDownR);
            this.splitContainer5.Panel2.Controls.Add(this.button2);
            this.splitContainer5.Panel2.Controls.Add(this.button1);
            this.splitContainer5.Panel2.Controls.Add(this.buttonMean);
            this.splitContainer5.Panel2.Controls.Add(this.pictureBoxColor);
            this.splitContainer5.Size = new System.Drawing.Size(844, 410);
            this.splitContainer5.SplitterDistance = 277;
            this.splitContainer5.SplitterWidth = 5;
            this.splitContainer5.TabIndex = 1;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.tabControl2);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.pictureBoxArt);
            this.splitContainer6.Size = new System.Drawing.Size(277, 410);
            this.splitContainer6.SplitterDistance = 242;
            this.splitContainer6.SplitterWidth = 5;
            this.splitContainer6.TabIndex = 0;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(277, 242);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.treeViewItem);
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage3.Size = new System.Drawing.Size(269, 213);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Items";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.treeViewLand);
            this.tabPage4.Location = new System.Drawing.Point(4, 25);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPage4.Size = new System.Drawing.Size(269, 213);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Land Tiles";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(111, 247);
            this.button5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(100, 28);
            this.button5.TabIndex = 16;
            this.button5.Text = "Import..";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.OnClickImport);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(3, 247);
            this.button4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 28);
            this.button4.TabIndex = 15;
            this.button4.Text = "Export..";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnClickExport);
            // 
            // numericUpDownShortCol
            // 
            this.numericUpDownShortCol.Location = new System.Drawing.Point(252, 12);
            this.numericUpDownShortCol.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numericUpDownShortCol.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.numericUpDownShortCol.Name = "numericUpDownShortCol";
            this.numericUpDownShortCol.Size = new System.Drawing.Size(133, 22);
            this.numericUpDownShortCol.TabIndex = 14;
            this.numericUpDownShortCol.ValueChanged += new System.EventHandler(this.OnNumericShortColChanged);
            // 
            // textBoxMeanFrom
            // 
            this.textBoxMeanFrom.Location = new System.Drawing.Point(252, 140);
            this.textBoxMeanFrom.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxMeanFrom.Name = "textBoxMeanFrom";
            this.textBoxMeanFrom.Size = new System.Drawing.Size(68, 22);
            this.textBoxMeanFrom.TabIndex = 13;
            // 
            // textBoxMeanTo
            // 
            this.textBoxMeanTo.Location = new System.Drawing.Point(341, 140);
            this.textBoxMeanTo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBoxMeanTo.Name = "textBoxMeanTo";
            this.textBoxMeanTo.Size = new System.Drawing.Size(68, 22);
            this.textBoxMeanTo.TabIndex = 12;
            // 
            // button3
            // 
            this.button3.AutoSize = true;
            this.button3.Location = new System.Drawing.Point(252, 169);
            this.button3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(209, 33);
            this.button3.TabIndex = 11;
            this.button3.Text = "Average Color from-to";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OnClickMeanColorFromTo);
            // 
            // numericUpDownB
            // 
            this.numericUpDownB.Location = new System.Drawing.Point(393, 44);
            this.numericUpDownB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numericUpDownB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownB.Name = "numericUpDownB";
            this.numericUpDownB.Size = new System.Drawing.Size(63, 22);
            this.numericUpDownB.TabIndex = 9;
            this.numericUpDownB.ValueChanged += new System.EventHandler(this.OnChangeB);
            // 
            // numericUpDownG
            // 
            this.numericUpDownG.Location = new System.Drawing.Point(323, 44);
            this.numericUpDownG.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numericUpDownG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownG.Name = "numericUpDownG";
            this.numericUpDownG.Size = new System.Drawing.Size(63, 22);
            this.numericUpDownG.TabIndex = 8;
            this.numericUpDownG.ValueChanged += new System.EventHandler(this.OnChangeG);
            // 
            // numericUpDownR
            // 
            this.numericUpDownR.Location = new System.Drawing.Point(252, 44);
            this.numericUpDownR.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numericUpDownR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownR.Name = "numericUpDownR";
            this.numericUpDownR.Size = new System.Drawing.Size(63, 22);
            this.numericUpDownR.TabIndex = 7;
            this.numericUpDownR.ValueChanged += new System.EventHandler(this.OnChangeR);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(111, 210);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 28);
            this.button2.TabIndex = 5;
            this.button2.Text = "Save File";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickSaveFile);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 210);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 4;
            this.button1.Text = "Save Color";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickSaveColor);
            // 
            // buttonMean
            // 
            this.buttonMean.Location = new System.Drawing.Point(5, 137);
            this.buttonMean.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonMean.Name = "buttonMean";
            this.buttonMean.Size = new System.Drawing.Size(100, 28);
            this.buttonMean.TabIndex = 1;
            this.buttonMean.Text = "Average Color";
            this.buttonMean.UseVisualStyleBackColor = true;
            this.buttonMean.Click += new System.EventHandler(this.OnClickMeanColor);
            // 
            // button6
            // 
            this.button6.AutoSize = true;
            this.button6.Location = new System.Drawing.Point(252, 242);
            this.button6.Margin = new System.Windows.Forms.Padding(4);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(235, 33);
            this.button6.TabIndex = 17;
            this.button6.Text = "Average All (Items and Land Tiles)";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.OnClickMeanColorAll);
            // 
            // RadarColorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer5);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "RadarColorControl";
            this.Size = new System.Drawing.Size(844, 410);
            this.Load += new System.EventHandler(this.OnLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxArt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxColor)).EndInit();
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            this.splitContainer5.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownShortCol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownR)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button buttonMean;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.NumericUpDown numericUpDownB;
        private System.Windows.Forms.NumericUpDown numericUpDownG;
        private System.Windows.Forms.NumericUpDown numericUpDownR;
        private System.Windows.Forms.NumericUpDown numericUpDownShortCol;
        private System.Windows.Forms.PictureBox pictureBoxArt;
        private System.Windows.Forms.PictureBox pictureBoxColor;
        private System.Windows.Forms.ToolStripMenuItem selectInItemsTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInTiledataTabToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox textBoxMeanFrom;
        private System.Windows.Forms.TextBox textBoxMeanTo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.TreeView treeViewItem;
        private System.Windows.Forms.TreeView treeViewLand;
        private System.Windows.Forms.Button button6;
    }
}
