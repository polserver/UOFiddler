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
            components = new System.ComponentModel.Container();
            treeViewItem = new System.Windows.Forms.TreeView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            selectInItemsTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selectInTiledataTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            setAsRangeFromToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            setAsRangeToToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            treeViewLand = new System.Windows.Forms.TreeView();
            contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            setAsRangeFromToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            setAsRangeToToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            pictureBoxArt = new System.Windows.Forms.PictureBox();
            pictureBoxColor = new System.Windows.Forms.PictureBox();
            splitContainer5 = new System.Windows.Forms.SplitContainer();
            splitContainer6 = new System.Windows.Forms.SplitContainer();
            tabControl2 = new System.Windows.Forms.TabControl();
            tabPage3 = new System.Windows.Forms.TabPage();
            tabPage4 = new System.Windows.Forms.TabPage();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            buttonRangeToRangeAverage = new System.Windows.Forms.Button();
            buttonRangeToIndividualAverage = new System.Windows.Forms.Button();
            buttonRevertAll = new System.Windows.Forms.Button();
            buttonRevert = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            progressBar2 = new System.Windows.Forms.ProgressBar();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            button6 = new System.Windows.Forms.Button();
            button5 = new System.Windows.Forms.Button();
            button4 = new System.Windows.Forms.Button();
            numericUpDownShortCol = new System.Windows.Forms.NumericUpDown();
            textBoxMeanFrom = new System.Windows.Forms.TextBox();
            textBoxMeanTo = new System.Windows.Forms.TextBox();
            buttonCurrentToRangeAverage = new System.Windows.Forms.Button();
            numericUpDownB = new System.Windows.Forms.NumericUpDown();
            numericUpDownG = new System.Windows.Forms.NumericUpDown();
            numericUpDownR = new System.Windows.Forms.NumericUpDown();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            buttonMean = new System.Windows.Forms.Button();
            contextMenuStrip1.SuspendLayout();
            contextMenuStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxArt).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxColor).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer5).BeginInit();
            splitContainer5.Panel1.SuspendLayout();
            splitContainer5.Panel2.SuspendLayout();
            splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer6).BeginInit();
            splitContainer6.Panel1.SuspendLayout();
            splitContainer6.Panel2.SuspendLayout();
            splitContainer6.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownShortCol).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownG).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownR).BeginInit();
            SuspendLayout();
            // 
            // treeViewItem
            // 
            treeViewItem.ContextMenuStrip = contextMenuStrip1;
            treeViewItem.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewItem.HideSelection = false;
            treeViewItem.Location = new System.Drawing.Point(4, 4);
            treeViewItem.Margin = new System.Windows.Forms.Padding(4);
            treeViewItem.Name = "treeViewItem";
            treeViewItem.Size = new System.Drawing.Size(228, 193);
            treeViewItem.TabIndex = 0;
            treeViewItem.AfterSelect += AfterSelectTreeViewItem;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { selectInItemsTabToolStripMenuItem, selectInTiledataTabToolStripMenuItem, setAsRangeFromToolStripMenuItem, setAsRangeToToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(183, 92);
            // 
            // selectInItemsTabToolStripMenuItem
            // 
            selectInItemsTabToolStripMenuItem.Name = "selectInItemsTabToolStripMenuItem";
            selectInItemsTabToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            selectInItemsTabToolStripMenuItem.Text = "Select in Items tab";
            selectInItemsTabToolStripMenuItem.Click += OnClickSelectItemsTab;
            // 
            // selectInTiledataTabToolStripMenuItem
            // 
            selectInTiledataTabToolStripMenuItem.Name = "selectInTiledataTabToolStripMenuItem";
            selectInTiledataTabToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            selectInTiledataTabToolStripMenuItem.Text = "Select in Tiledata tab";
            selectInTiledataTabToolStripMenuItem.Click += OnClickSelectItemTiledataTab;
            // 
            // setAsRangeFromToolStripMenuItem
            // 
            setAsRangeFromToolStripMenuItem.Name = "setAsRangeFromToolStripMenuItem";
            setAsRangeFromToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            setAsRangeFromToolStripMenuItem.Text = "Set as Range \"from\"";
            setAsRangeFromToolStripMenuItem.Click += OnClickSetRangeFrom;
            // 
            // setAsRangeToToolStripMenuItem
            // 
            setAsRangeToToolStripMenuItem.Name = "setAsRangeToToolStripMenuItem";
            setAsRangeToToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            setAsRangeToToolStripMenuItem.Text = "Set as Range \"to\"";
            setAsRangeToToolStripMenuItem.Click += OnClickSetRangeTo;
            // 
            // treeViewLand
            // 
            treeViewLand.ContextMenuStrip = contextMenuStrip2;
            treeViewLand.Dock = System.Windows.Forms.DockStyle.Fill;
            treeViewLand.HideSelection = false;
            treeViewLand.Location = new System.Drawing.Point(4, 4);
            treeViewLand.Margin = new System.Windows.Forms.Padding(4);
            treeViewLand.Name = "treeViewLand";
            treeViewLand.Size = new System.Drawing.Size(228, 193);
            treeViewLand.TabIndex = 0;
            treeViewLand.AfterSelect += AfterSelectTreeViewLand;
            // 
            // contextMenuStrip2
            // 
            contextMenuStrip2.ImageScalingSize = new System.Drawing.Size(20, 20);
            contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem1, toolStripMenuItem2, setAsRangeFromToolStripMenuItem1, setAsRangeToToolStripMenuItem1 });
            contextMenuStrip2.Name = "contextMenuStrip1";
            contextMenuStrip2.Size = new System.Drawing.Size(189, 114);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
            toolStripMenuItem1.Text = "Select in Landtiles tab";
            toolStripMenuItem1.Click += OnClickSelectLandTilesTab;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(188, 22);
            toolStripMenuItem2.Text = "Select in Tiledata tab";
            toolStripMenuItem2.Click += OnClickSelectLandTiledataTab;
            // 
            // setAsRangeFromToolStripMenuItem1
            // 
            setAsRangeFromToolStripMenuItem1.Name = "setAsRangeFromToolStripMenuItem1";
            setAsRangeFromToolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
            setAsRangeFromToolStripMenuItem1.Text = "Set as Range \"from\"";
            setAsRangeFromToolStripMenuItem1.Click += OnClickSetRangeFrom;
            // 
            // setAsRangeToToolStripMenuItem1
            // 
            setAsRangeToToolStripMenuItem1.Name = "setAsRangeToToolStripMenuItem1";
            setAsRangeToToolStripMenuItem1.Size = new System.Drawing.Size(188, 22);
            setAsRangeToToolStripMenuItem1.Text = "Set as Range \"to\"";
            setAsRangeToToolStripMenuItem1.Click += OnClickSetRangeTo;
            // 
            // pictureBoxArt
            // 
            pictureBoxArt.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBoxArt.Location = new System.Drawing.Point(0, 0);
            pictureBoxArt.Margin = new System.Windows.Forms.Padding(4);
            pictureBoxArt.Name = "pictureBoxArt";
            pictureBoxArt.Size = new System.Drawing.Size(244, 154);
            pictureBoxArt.TabIndex = 0;
            pictureBoxArt.TabStop = false;
            // 
            // pictureBoxColor
            // 
            pictureBoxColor.Location = new System.Drawing.Point(4, 4);
            pictureBoxColor.Margin = new System.Windows.Forms.Padding(4);
            pictureBoxColor.Name = "pictureBoxColor";
            pictureBoxColor.Size = new System.Drawing.Size(161, 116);
            pictureBoxColor.TabIndex = 0;
            pictureBoxColor.TabStop = false;
            // 
            // splitContainer5
            // 
            splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer5.Location = new System.Drawing.Point(0, 0);
            splitContainer5.Margin = new System.Windows.Forms.Padding(4);
            splitContainer5.Name = "splitContainer5";
            // 
            // splitContainer5.Panel1
            // 
            splitContainer5.Panel1.Controls.Add(splitContainer6);
            // 
            // splitContainer5.Panel2
            // 
            splitContainer5.Panel2.Controls.Add(label4);
            splitContainer5.Panel2.Controls.Add(label3);
            splitContainer5.Panel2.Controls.Add(buttonRangeToRangeAverage);
            splitContainer5.Panel2.Controls.Add(buttonRangeToIndividualAverage);
            splitContainer5.Panel2.Controls.Add(buttonRevertAll);
            splitContainer5.Panel2.Controls.Add(buttonRevert);
            splitContainer5.Panel2.Controls.Add(label2);
            splitContainer5.Panel2.Controls.Add(label1);
            splitContainer5.Panel2.Controls.Add(progressBar2);
            splitContainer5.Panel2.Controls.Add(progressBar1);
            splitContainer5.Panel2.Controls.Add(button6);
            splitContainer5.Panel2.Controls.Add(button5);
            splitContainer5.Panel2.Controls.Add(button4);
            splitContainer5.Panel2.Controls.Add(numericUpDownShortCol);
            splitContainer5.Panel2.Controls.Add(textBoxMeanFrom);
            splitContainer5.Panel2.Controls.Add(textBoxMeanTo);
            splitContainer5.Panel2.Controls.Add(buttonCurrentToRangeAverage);
            splitContainer5.Panel2.Controls.Add(numericUpDownB);
            splitContainer5.Panel2.Controls.Add(numericUpDownG);
            splitContainer5.Panel2.Controls.Add(numericUpDownR);
            splitContainer5.Panel2.Controls.Add(button2);
            splitContainer5.Panel2.Controls.Add(button1);
            splitContainer5.Panel2.Controls.Add(buttonMean);
            splitContainer5.Panel2.Controls.Add(pictureBoxColor);
            splitContainer5.Size = new System.Drawing.Size(744, 388);
            splitContainer5.SplitterDistance = 244;
            splitContainer5.TabIndex = 1;
            // 
            // splitContainer6
            // 
            splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer6.Location = new System.Drawing.Point(0, 0);
            splitContainer6.Margin = new System.Windows.Forms.Padding(4);
            splitContainer6.Name = "splitContainer6";
            splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            splitContainer6.Panel1.Controls.Add(tabControl2);
            // 
            // splitContainer6.Panel2
            // 
            splitContainer6.Panel2.Controls.Add(pictureBoxArt);
            splitContainer6.Size = new System.Drawing.Size(244, 388);
            splitContainer6.SplitterDistance = 229;
            splitContainer6.SplitterWidth = 5;
            splitContainer6.TabIndex = 0;
            // 
            // tabControl2
            // 
            tabControl2.Controls.Add(tabPage3);
            tabControl2.Controls.Add(tabPage4);
            tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl2.Location = new System.Drawing.Point(0, 0);
            tabControl2.Margin = new System.Windows.Forms.Padding(4);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new System.Drawing.Size(244, 229);
            tabControl2.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(treeViewItem);
            tabPage3.Location = new System.Drawing.Point(4, 24);
            tabPage3.Margin = new System.Windows.Forms.Padding(4);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new System.Windows.Forms.Padding(4);
            tabPage3.Size = new System.Drawing.Size(236, 201);
            tabPage3.TabIndex = 0;
            tabPage3.Text = "Items";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(treeViewLand);
            tabPage4.Location = new System.Drawing.Point(4, 24);
            tabPage4.Margin = new System.Windows.Forms.Padding(4);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new System.Windows.Forms.Padding(4);
            tabPage4.Size = new System.Drawing.Size(236, 201);
            tabPage4.TabIndex = 1;
            tabPage4.Text = "Land Tiles";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(336, 134);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(16, 15);
            label4.TabIndex = 27;
            label4.Text = "...";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(220, 134);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(43, 15);
            label3.TabIndex = 26;
            label3.Text = "Range:";
            // 
            // buttonRangeToRangeAverage
            // 
            buttonRangeToRangeAverage.Location = new System.Drawing.Point(220, 233);
            buttonRangeToRangeAverage.Name = "buttonRangeToRangeAverage";
            buttonRangeToRangeAverage.Size = new System.Drawing.Size(220, 26);
            buttonRangeToRangeAverage.TabIndex = 25;
            buttonRangeToRangeAverage.Text = "Range tiles to range average";
            buttonRangeToRangeAverage.UseVisualStyleBackColor = true;
            buttonRangeToRangeAverage.Click += OnClickRangeToRangeAverage;
            // 
            // buttonRangeToIndividualAverage
            // 
            buttonRangeToIndividualAverage.Location = new System.Drawing.Point(220, 198);
            buttonRangeToIndividualAverage.Name = "buttonRangeToIndividualAverage";
            buttonRangeToIndividualAverage.Size = new System.Drawing.Size(220, 26);
            buttonRangeToIndividualAverage.TabIndex = 24;
            buttonRangeToIndividualAverage.Text = "Range tiles to individual average";
            buttonRangeToIndividualAverage.UseVisualStyleBackColor = true;
            buttonRangeToIndividualAverage.Click += OnClickRangeToIndividualAverage;
            // 
            // buttonRevertAll
            // 
            buttonRevertAll.Enabled = false;
            buttonRevertAll.Location = new System.Drawing.Point(4, 294);
            buttonRevertAll.Name = "buttonRevertAll";
            buttonRevertAll.Size = new System.Drawing.Size(88, 26);
            buttonRevertAll.TabIndex = 23;
            buttonRevertAll.Text = "Revert All";
            buttonRevertAll.UseVisualStyleBackColor = true;
            buttonRevertAll.Click += OnClickRevertAll;
            // 
            // buttonRevert
            // 
            buttonRevert.Enabled = false;
            buttonRevert.Location = new System.Drawing.Point(97, 128);
            buttonRevert.Name = "buttonRevert";
            buttonRevert.Size = new System.Drawing.Size(88, 26);
            buttonRevert.TabIndex = 22;
            buttonRevert.Text = "Revert";
            buttonRevert.UseVisualStyleBackColor = true;
            buttonRevert.Click += OnClickRevert;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(226, 364);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(62, 15);
            label2.TabIndex = 21;
            label2.Text = "Land Tiles:";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(255, 337);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(39, 15);
            label1.TabIndex = 20;
            label1.Text = "Items:";
            // 
            // progressBar2
            // 
            progressBar2.Location = new System.Drawing.Point(309, 359);
            progressBar2.Name = "progressBar2";
            progressBar2.Size = new System.Drawing.Size(117, 22);
            progressBar2.TabIndex = 19;
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(309, 331);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(117, 22);
            progressBar1.TabIndex = 18;
            // 
            // button6
            // 
            button6.AutoSize = true;
            button6.Location = new System.Drawing.Point(220, 294);
            button6.Margin = new System.Windows.Forms.Padding(4);
            button6.Name = "button6";
            button6.Size = new System.Drawing.Size(206, 31);
            button6.TabIndex = 17;
            button6.Text = "Average All (Items and Land Tiles)";
            button6.UseVisualStyleBackColor = true;
            button6.Click += OnClickMeanColorAll;
            // 
            // button5
            // 
            button5.Location = new System.Drawing.Point(97, 329);
            button5.Margin = new System.Windows.Forms.Padding(4);
            button5.Name = "button5";
            button5.Size = new System.Drawing.Size(88, 26);
            button5.TabIndex = 16;
            button5.Text = "Import..";
            button5.UseVisualStyleBackColor = true;
            button5.Click += OnClickImport;
            // 
            // button4
            // 
            button4.Location = new System.Drawing.Point(3, 329);
            button4.Margin = new System.Windows.Forms.Padding(4);
            button4.Name = "button4";
            button4.Size = new System.Drawing.Size(88, 26);
            button4.TabIndex = 15;
            button4.Text = "Export..";
            button4.UseVisualStyleBackColor = true;
            button4.Click += OnClickExport;
            // 
            // numericUpDownShortCol
            // 
            numericUpDownShortCol.Location = new System.Drawing.Point(220, 11);
            numericUpDownShortCol.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownShortCol.Maximum = new decimal(new int[] { 32767, 0, 0, 0 });
            numericUpDownShortCol.Name = "numericUpDownShortCol";
            numericUpDownShortCol.Size = new System.Drawing.Size(116, 23);
            numericUpDownShortCol.TabIndex = 14;
            numericUpDownShortCol.ValueChanged += OnNumericShortColChanged;
            // 
            // textBoxMeanFrom
            // 
            textBoxMeanFrom.ForeColor = System.Drawing.SystemColors.WindowText;
            textBoxMeanFrom.Location = new System.Drawing.Point(270, 131);
            textBoxMeanFrom.Margin = new System.Windows.Forms.Padding(4);
            textBoxMeanFrom.Name = "textBoxMeanFrom";
            textBoxMeanFrom.PlaceholderText = "from";
            textBoxMeanFrom.Size = new System.Drawing.Size(60, 23);
            textBoxMeanFrom.TabIndex = 12;
            // 
            // textBoxMeanTo
            // 
            textBoxMeanTo.ForeColor = System.Drawing.SystemColors.WindowText;
            textBoxMeanTo.Location = new System.Drawing.Point(360, 132);
            textBoxMeanTo.Margin = new System.Windows.Forms.Padding(4);
            textBoxMeanTo.Name = "textBoxMeanTo";
            textBoxMeanTo.PlaceholderText = "to";
            textBoxMeanTo.Size = new System.Drawing.Size(60, 23);
            textBoxMeanTo.TabIndex = 13;
            // 
            // buttonCurrentToRangeAverage
            // 
            buttonCurrentToRangeAverage.AutoSize = true;
            buttonCurrentToRangeAverage.Location = new System.Drawing.Point(220, 163);
            buttonCurrentToRangeAverage.Margin = new System.Windows.Forms.Padding(4);
            buttonCurrentToRangeAverage.Name = "buttonCurrentToRangeAverage";
            buttonCurrentToRangeAverage.Size = new System.Drawing.Size(220, 26);
            buttonCurrentToRangeAverage.TabIndex = 11;
            buttonCurrentToRangeAverage.Text = "Current tile to range average";
            buttonCurrentToRangeAverage.UseVisualStyleBackColor = true;
            buttonCurrentToRangeAverage.Click += OnClickCurrentToRangeAverage;
            // 
            // numericUpDownB
            // 
            numericUpDownB.Location = new System.Drawing.Point(344, 41);
            numericUpDownB.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownB.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownB.Name = "numericUpDownB";
            numericUpDownB.Size = new System.Drawing.Size(55, 23);
            numericUpDownB.TabIndex = 9;
            numericUpDownB.ValueChanged += OnChangeB;
            // 
            // numericUpDownG
            // 
            numericUpDownG.Location = new System.Drawing.Point(283, 41);
            numericUpDownG.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownG.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownG.Name = "numericUpDownG";
            numericUpDownG.Size = new System.Drawing.Size(55, 23);
            numericUpDownG.TabIndex = 8;
            numericUpDownG.ValueChanged += OnChangeG;
            // 
            // numericUpDownR
            // 
            numericUpDownR.Location = new System.Drawing.Point(220, 41);
            numericUpDownR.Margin = new System.Windows.Forms.Padding(4);
            numericUpDownR.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
            numericUpDownR.Name = "numericUpDownR";
            numericUpDownR.Size = new System.Drawing.Size(55, 23);
            numericUpDownR.TabIndex = 7;
            numericUpDownR.ValueChanged += OnChangeR;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(97, 294);
            button2.Margin = new System.Windows.Forms.Padding(4);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(88, 26);
            button2.TabIndex = 5;
            button2.Text = "Save File";
            button2.UseVisualStyleBackColor = true;
            button2.Click += OnClickSaveFile;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(3, 163);
            button1.Margin = new System.Windows.Forms.Padding(4);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(88, 26);
            button1.TabIndex = 4;
            button1.Text = "Save Color";
            button1.UseVisualStyleBackColor = true;
            button1.Click += OnClickSaveColor;
            // 
            // buttonMean
            // 
            buttonMean.Location = new System.Drawing.Point(4, 128);
            buttonMean.Margin = new System.Windows.Forms.Padding(4);
            buttonMean.Name = "buttonMean";
            buttonMean.Size = new System.Drawing.Size(88, 26);
            buttonMean.TabIndex = 1;
            buttonMean.Text = "Average Color";
            buttonMean.UseVisualStyleBackColor = true;
            buttonMean.Click += OnClickMeanColor;
            // 
            // RadarColorControl
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer5);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4);
            Name = "RadarColorControl";
            Size = new System.Drawing.Size(744, 388);
            Load += OnLoad;
            contextMenuStrip1.ResumeLayout(false);
            contextMenuStrip2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxArt).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxColor).EndInit();
            splitContainer5.Panel1.ResumeLayout(false);
            splitContainer5.Panel2.ResumeLayout(false);
            splitContainer5.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer5).EndInit();
            splitContainer5.ResumeLayout(false);
            splitContainer6.Panel1.ResumeLayout(false);
            splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer6).EndInit();
            splitContainer6.ResumeLayout(false);
            tabControl2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)numericUpDownShortCol).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownB).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownG).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownR).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button buttonCurrentToRangeAverage;
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
        private System.Windows.Forms.Button buttonRevert;
        private System.Windows.Forms.Button buttonRevertAll;
        private System.Windows.Forms.Button buttonRangeToRangeAverage;
        private System.Windows.Forms.Button buttonRangeToIndividualAverage;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripMenuItem setAsRangeFromToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setAsRangeToToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setAsRangeFromToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem setAsRangeToToolStripMenuItem1;
    }
}
