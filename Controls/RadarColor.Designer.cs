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
    partial class RadarColor
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
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
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
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
            this.treeViewItem.Location = new System.Drawing.Point(3, 3);
            this.treeViewItem.Name = "treeViewItem";
            this.treeViewItem.Size = new System.Drawing.Size(191, 162);
            this.treeViewItem.TabIndex = 0;
            this.treeViewItem.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AfterSelectTreeViewitem);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectInItemsTabToolStripMenuItem,
            this.selectInTiledataTabToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(186, 48);
            // 
            // selectInItemsTabToolStripMenuItem
            // 
            this.selectInItemsTabToolStripMenuItem.Name = "selectInItemsTabToolStripMenuItem";
            this.selectInItemsTabToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.selectInItemsTabToolStripMenuItem.Text = "Select in Items tab";
            this.selectInItemsTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectItemsTab);
            // 
            // selectInTiledataTabToolStripMenuItem
            // 
            this.selectInTiledataTabToolStripMenuItem.Name = "selectInTiledataTabToolStripMenuItem";
            this.selectInTiledataTabToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.selectInTiledataTabToolStripMenuItem.Text = "Select in Tiledata tab";
            this.selectInTiledataTabToolStripMenuItem.Click += new System.EventHandler(this.OnClickSelectItemTiledataTab);
            // 
            // treeViewLand
            // 
            this.treeViewLand.ContextMenuStrip = this.contextMenuStrip2;
            this.treeViewLand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLand.HideSelection = false;
            this.treeViewLand.Location = new System.Drawing.Point(3, 3);
            this.treeViewLand.Name = "treeViewLand";
            this.treeViewLand.Size = new System.Drawing.Size(191, 162);
            this.treeViewLand.TabIndex = 0;
            this.treeViewLand.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.AfterSelectTreeViewLand);
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.contextMenuStrip2.Name = "contextMenuStrip1";
            this.contextMenuStrip2.Size = new System.Drawing.Size(190, 70);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(189, 22);
            this.toolStripMenuItem1.Text = "Select in Landtiles tab";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.OnClickSelectLandtilesTab);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(189, 22);
            this.toolStripMenuItem2.Text = "Select in Tiledata tab";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.OnClickSelectLandTiledataTab);
            // 
            // pictureBoxArt
            // 
            this.pictureBoxArt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxArt.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxArt.Name = "pictureBoxArt";
            this.pictureBoxArt.Size = new System.Drawing.Size(205, 126);
            this.pictureBoxArt.TabIndex = 0;
            this.pictureBoxArt.TabStop = false;
            // 
            // pictureBoxColor
            // 
            this.pictureBoxColor.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxColor.Name = "pictureBoxColor";
            this.pictureBoxColor.Size = new System.Drawing.Size(138, 101);
            this.pictureBoxColor.TabIndex = 0;
            this.pictureBoxColor.TabStop = false;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
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
            this.splitContainer5.Size = new System.Drawing.Size(619, 324);
            this.splitContainer5.SplitterDistance = 205;
            this.splitContainer5.TabIndex = 1;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer6.Size = new System.Drawing.Size(205, 324);
            this.splitContainer6.SplitterDistance = 194;
            this.splitContainer6.TabIndex = 0;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(205, 194);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.treeViewItem);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(197, 168);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Items";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.treeViewLand);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(197, 168);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Landtiles";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // numericUpDownShortCol
            // 
            this.numericUpDownShortCol.Location = new System.Drawing.Point(189, 10);
            this.numericUpDownShortCol.Maximum = new decimal(new int[] {
            32767,
            0,
            0,
            0});
            this.numericUpDownShortCol.Name = "numericUpDownShortCol";
            this.numericUpDownShortCol.Size = new System.Drawing.Size(100, 20);
            this.numericUpDownShortCol.TabIndex = 14;
            this.numericUpDownShortCol.ValueChanged += new System.EventHandler(this.OnNumericShortColChanged);
            // 
            // textBoxMeanFrom
            // 
            this.textBoxMeanFrom.Location = new System.Drawing.Point(189, 114);
            this.textBoxMeanFrom.Name = "textBoxMeanFrom";
            this.textBoxMeanFrom.Size = new System.Drawing.Size(52, 20);
            this.textBoxMeanFrom.TabIndex = 13;
            // 
            // textBoxMeanTo
            // 
            this.textBoxMeanTo.Location = new System.Drawing.Point(256, 114);
            this.textBoxMeanTo.Name = "textBoxMeanTo";
            this.textBoxMeanTo.Size = new System.Drawing.Size(52, 20);
            this.textBoxMeanTo.TabIndex = 12;
            // 
            // button3
            // 
            this.button3.AutoSize = true;
            this.button3.Location = new System.Drawing.Point(189, 137);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(119, 23);
            this.button3.TabIndex = 11;
            this.button3.Text = "Average Color from-to";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.OnClickmeanColorFromTo);
            // 
            // numericUpDownB
            // 
            this.numericUpDownB.Location = new System.Drawing.Point(295, 36);
            this.numericUpDownB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownB.Name = "numericUpDownB";
            this.numericUpDownB.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownB.TabIndex = 9;
            this.numericUpDownB.ValueChanged += new System.EventHandler(this.OnChangeB);
            // 
            // numericUpDownG
            // 
            this.numericUpDownG.Location = new System.Drawing.Point(242, 36);
            this.numericUpDownG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownG.Name = "numericUpDownG";
            this.numericUpDownG.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownG.TabIndex = 8;
            this.numericUpDownG.ValueChanged += new System.EventHandler(this.OnChangeG);
            // 
            // numericUpDownR
            // 
            this.numericUpDownR.Location = new System.Drawing.Point(189, 36);
            this.numericUpDownR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownR.Name = "numericUpDownR";
            this.numericUpDownR.Size = new System.Drawing.Size(47, 20);
            this.numericUpDownR.TabIndex = 7;
            this.numericUpDownR.ValueChanged += new System.EventHandler(this.onChangeR);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(83, 171);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Save File";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.onClickSaveFile);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(2, 171);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Save Color";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.onClickSaveColor);
            // 
            // buttonMean
            // 
            this.buttonMean.Location = new System.Drawing.Point(4, 111);
            this.buttonMean.Name = "buttonMean";
            this.buttonMean.Size = new System.Drawing.Size(75, 23);
            this.buttonMean.TabIndex = 1;
            this.buttonMean.Text = "Average Color";
            this.buttonMean.UseVisualStyleBackColor = true;
            this.buttonMean.Click += new System.EventHandler(this.OnClickMeanColor);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(2, 201);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 15;
            this.button4.Text = "Export..";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.OnClickExport);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(83, 201);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 16;
            this.button5.Text = "Import..";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.OnClickImport);
            // 
            // RadarColor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer5);
            this.Name = "RadarColor";
            this.Size = new System.Drawing.Size(619, 324);
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

        private System.Windows.Forms.TreeView treeViewItem;
        private System.Windows.Forms.TreeView treeViewLand;
        private System.Windows.Forms.PictureBox pictureBoxArt;
        private System.Windows.Forms.PictureBox pictureBoxColor;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button buttonMean;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown numericUpDownB;
        private System.Windows.Forms.NumericUpDown numericUpDownG;
        private System.Windows.Forms.NumericUpDown numericUpDownR;
        private System.Windows.Forms.TextBox textBoxMeanFrom;
        private System.Windows.Forms.TextBox textBoxMeanTo;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem selectInItemsTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInTiledataTabToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.NumericUpDown numericUpDownShortCol;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
    }
}
