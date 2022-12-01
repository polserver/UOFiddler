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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.button6 = new System.Windows.Forms.Button();
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
            this.treeViewItem.Margin = new System.Windows.Forms.Padding(4);
            this.treeViewItem.Name = "treeViewItem";
            this.treeViewItem.Size = new System.Drawing.Size(226, 193);
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(183, 48);
            // 
            // selectInItemsTabToolStripMenuItem
            // 
            this.selectInItemsTabToolStripMenuItem.Name = "selectInItemsTabToolStripMenuItem";
            this.selectInItemsTabToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.selectInItemsTabToolStripMenuItem.Text = "Select in Items tab";
            this.selectInItemsTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectItemsTab);
            // 
            // selectInTiledataTabToolStripMenuItem
            // 
            this.selectInTiledataTabToolStripMenuItem.Name = "selectInTiledataTabToolStripMenuItem";
            this.selectInTiledataTabToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.selectInTiledataTabToolStripMenuItem.Text = "Select in Tiledata tab";
            this.selectInTiledataTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectItemTiledataTab);
            // 
            // treeViewLand
            // 
            this.treeViewLand.ContextMenuStrip = this.contextMenuStrip2;
            this.treeViewLand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLand.HideSelection = false;
            this.treeViewLand.Location = new System.Drawing.Point(4, 4);
            this.treeViewLand.Margin = new System.Windows.Forms.Padding(4);
            this.treeViewLand.Name = "treeViewLand";
            this.treeViewLand.Size = new System.Drawing.Size(226, 191);
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
            this.contextMenuStrip2.Size = new System.Drawing.Size(189, 48);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
            this.toolStripMenuItem1.Text = "Select in Landtiles tab";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.OnClickSelectLandTilesTab);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(188, 22);
            this.toolStripMenuItem2.Text = "Select in Tiledata tab";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.OnClickSelectLandTiledataTab);
            // 
            // pictureBoxArt
            // 
            this.pictureBoxArt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxArt.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxArt.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxArt.Name = "pictureBoxArt";
            this.pictureBoxArt.Size = new System.Drawing.Size(242, 154);
            this.pictureBoxArt.TabIndex = 0;
            this.pictureBoxArt.TabStop = false;
            // 
            // pictureBoxColor
            // 
            this.pictureBoxColor.Location = new System.Drawing.Point(4, 4);
            this.pictureBoxColor.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBoxColor.Name = "pictureBoxColor";
            this.pictureBoxColor.Size = new System.Drawing.Size(161, 116);
            this.pictureBoxColor.TabIndex = 0;
            this.pictureBoxColor.TabStop = false;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.label2);
            this.splitContainer5.Panel2.Controls.Add(this.label1);
            this.splitContainer5.Panel2.Controls.Add(this.progressBar2);
            this.splitContainer5.Panel2.Controls.Add(this.progressBar1);
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
            this.splitContainer5.Size = new System.Drawing.Size(739, 388);
            this.splitContainer5.SplitterDistance = 242;
            this.splitContainer5.TabIndex = 1;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Margin = new System.Windows.Forms.Padding(4);
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
            this.splitContainer6.Size = new System.Drawing.Size(242, 388);
            this.splitContainer6.SplitterDistance = 229;
            this.splitContainer6.SplitterWidth = 5;
            this.splitContainer6.TabIndex = 0;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(242, 229);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.treeViewItem);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage3.Size = new System.Drawing.Size(234, 201);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Items";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.treeViewLand);
            this.tabPage4.Location = new System.Drawing.Point(4, 24);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage4.Size = new System.Drawing.Size(234, 199);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Land Tiles";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(226, 297);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 15);
            this.label2.TabIndex = 21;
            this.label2.Text = "Land Tiles:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(255, 270);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 20;
            this.label1.Text = "Items:";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(309, 292);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(117, 22);
            this.progressBar2.TabIndex = 19;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(309, 264);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(117, 22);
            this.progressBar1.TabIndex = 18;
            // 
            // button6
            // 
            this.button6.AutoSize = true;
            this.button6.Location = new System.Drawing.Point(220, 227);
            this.button6.Margin = new System.Windows.Forms.Padding(4);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(206, 31);
            this.button6.TabIndex = 17;
            this.button6.Text = "Average All (Items and Land Tiles)";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.OnClickMeanColorAll);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(97, 232);
            this.button5.Margin = new System.Windows.Forms.Padding(4);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(88, 26);
            this.button5.TabIndex = 16;
            this.button5.Text = "Import..";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.OnClickImport);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(3, 232);
            this.button4.Margin = new System.Windows.Forms.Padding(4);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(88, 26);
            this.button4.TabIndex = 15;
            this.button4.Text = "Export..";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnClickExport);
            // 
            // numericUpDownShortCol
            // 
            this.numericUpDownShortCol.Location = new System.Drawing.Point(220, 11);
            this.numericUpDownShortCol.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownShortCol.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.numericUpDownShortCol.Name = "numericUpDownShortCol";
            this.numericUpDownShortCol.Size = new System.Drawing.Size(116, 23);
            this.numericUpDownShortCol.TabIndex = 14;
            this.numericUpDownShortCol.ValueChanged += new System.EventHandler(this.OnNumericShortColChanged);
            // 
            // textBoxMeanFrom
            // 
            this.textBoxMeanFrom.Location = new System.Drawing.Point(220, 131);
            this.textBoxMeanFrom.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxMeanFrom.Name = "textBoxMeanFrom";
            this.textBoxMeanFrom.Size = new System.Drawing.Size(60, 23);
            this.textBoxMeanFrom.TabIndex = 13;
            // 
            // textBoxMeanTo
            // 
            this.textBoxMeanTo.Location = new System.Drawing.Point(298, 131);
            this.textBoxMeanTo.Margin = new System.Windows.Forms.Padding(4);
            this.textBoxMeanTo.Name = "textBoxMeanTo";
            this.textBoxMeanTo.Size = new System.Drawing.Size(60, 23);
            this.textBoxMeanTo.TabIndex = 12;
            // 
            // button3
            // 
            this.button3.AutoSize = true;
            this.button3.Location = new System.Drawing.Point(220, 158);
            this.button3.Margin = new System.Windows.Forms.Padding(4);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(183, 31);
            this.button3.TabIndex = 11;
            this.button3.Text = "Average Color from-to";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OnClickMeanColorFromTo);
            // 
            // numericUpDownB
            // 
            this.numericUpDownB.Location = new System.Drawing.Point(344, 41);
            this.numericUpDownB.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownB.Name = "numericUpDownB";
            this.numericUpDownB.Size = new System.Drawing.Size(55, 23);
            this.numericUpDownB.TabIndex = 9;
            this.numericUpDownB.ValueChanged += new System.EventHandler(this.OnChangeB);
            // 
            // numericUpDownG
            // 
            this.numericUpDownG.Location = new System.Drawing.Point(283, 41);
            this.numericUpDownG.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownG.Name = "numericUpDownG";
            this.numericUpDownG.Size = new System.Drawing.Size(55, 23);
            this.numericUpDownG.TabIndex = 8;
            this.numericUpDownG.ValueChanged += new System.EventHandler(this.OnChangeG);
            // 
            // numericUpDownR
            // 
            this.numericUpDownR.Location = new System.Drawing.Point(220, 41);
            this.numericUpDownR.Margin = new System.Windows.Forms.Padding(4);
            this.numericUpDownR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownR.Name = "numericUpDownR";
            this.numericUpDownR.Size = new System.Drawing.Size(55, 23);
            this.numericUpDownR.TabIndex = 7;
            this.numericUpDownR.ValueChanged += new System.EventHandler(this.OnChangeR);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(97, 197);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(88, 26);
            this.button2.TabIndex = 5;
            this.button2.Text = "Save File";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnClickSaveFile);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 197);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 26);
            this.button1.TabIndex = 4;
            this.button1.Text = "Save Color";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnClickSaveColor);
            // 
            // buttonMean
            // 
            this.buttonMean.Location = new System.Drawing.Point(4, 128);
            this.buttonMean.Margin = new System.Windows.Forms.Padding(4);
            this.buttonMean.Name = "buttonMean";
            this.buttonMean.Size = new System.Drawing.Size(88, 26);
            this.buttonMean.TabIndex = 1;
            this.buttonMean.Text = "Average Color";
            this.buttonMean.UseVisualStyleBackColor = true;
            this.buttonMean.Click += new System.EventHandler(this.OnClickMeanColor);
            // 
            // RadarColorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer5);
            this.DoubleBuffered = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RadarColorControl";
            this.Size = new System.Drawing.Size(739, 388);
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
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar2;
    }
}
