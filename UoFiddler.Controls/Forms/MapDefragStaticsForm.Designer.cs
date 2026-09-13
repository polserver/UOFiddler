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
    partial class MapDefragStaticsForm
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

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.groupBoxSource = new System.Windows.Forms.GroupBox();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.labelGeometry = new System.Windows.Forms.Label();
            this.checkBoxAllowTruncation = new System.Windows.Forms.CheckBox();
            this.groupBoxFilters = new System.Windows.Forms.GroupBox();
            this.checkBoxDropInvalidIds = new System.Windows.Forms.CheckBox();
            this.labelCeiling = new System.Windows.Forms.Label();
            this.comboBoxIdCeiling = new System.Windows.Forms.ComboBox();
            this.labelCeilingValue = new System.Windows.Forms.Label();
            this.labelOutOfBlock = new System.Windows.Forms.Label();
            this.comboBoxOutOfBlock = new System.Windows.Forms.ComboBox();
            this.checkBoxDropInvalidZ = new System.Windows.Forms.CheckBox();
            this.checkBoxNormalizeHue = new System.Windows.Forms.CheckBox();
            this.checkBoxBelowTerrain = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveDuplicates = new System.Windows.Forms.CheckBox();
            this.checkBoxDuplicatesHue = new System.Windows.Forms.CheckBox();
            this.checkBoxCollapseStacks = new System.Windows.Forms.CheckBox();
            this.labelCollapseFlags = new System.Windows.Forms.Label();
            this.checkBoxCollapseWet = new System.Windows.Forms.CheckBox();
            this.checkBoxCollapseSurface = new System.Windows.Forms.CheckBox();
            this.checkBoxCollapseIgnoreZ = new System.Windows.Forms.CheckBox();
            this.labelCollapseIds = new System.Windows.Forms.Label();
            this.textBoxCollapseIds = new System.Windows.Forms.TextBox();
            this.checkBoxSortTiles = new System.Windows.Forms.CheckBox();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonAnalyze = new System.Windows.Forms.Button();
            this.buttonDefrag = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.components.Add(this.worker);
            this.groupBoxSource.SuspendLayout();
            this.groupBoxFilters.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxSource
            //
            this.groupBoxSource.Controls.Add(this.textBoxSource);
            this.groupBoxSource.Controls.Add(this.labelGeometry);
            this.groupBoxSource.Controls.Add(this.checkBoxAllowTruncation);
            this.groupBoxSource.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSource.Name = "groupBoxSource";
            this.groupBoxSource.Size = new System.Drawing.Size(536, 146);
            this.groupBoxSource.TabIndex = 0;
            this.groupBoxSource.TabStop = false;
            this.groupBoxSource.Text = "Source";
            //
            // textBoxSource
            //
            this.textBoxSource.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxSource.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxSource.Location = new System.Drawing.Point(12, 20);
            this.textBoxSource.Multiline = true;
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.ReadOnly = true;
            this.textBoxSource.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxSource.Size = new System.Drawing.Size(512, 62);
            this.textBoxSource.TabIndex = 0;
            this.textBoxSource.TabStop = false;
            //
            // labelGeometry
            //
            this.labelGeometry.Location = new System.Drawing.Point(12, 86);
            this.labelGeometry.Name = "labelGeometry";
            this.labelGeometry.Size = new System.Drawing.Size(512, 32);
            this.labelGeometry.TabIndex = 1;
            //
            // checkBoxAllowTruncation
            //
            this.checkBoxAllowTruncation.Enabled = false;
            this.checkBoxAllowTruncation.Location = new System.Drawing.Point(12, 120);
            this.checkBoxAllowTruncation.Name = "checkBoxAllowTruncation";
            this.checkBoxAllowTruncation.Size = new System.Drawing.Size(512, 21);
            this.checkBoxAllowTruncation.TabIndex = 2;
            this.checkBoxAllowTruncation.Text = "Discard the blocks past the configured map size";
            this.checkBoxAllowTruncation.UseVisualStyleBackColor = true;
            //
            // groupBoxFilters
            //
            this.groupBoxFilters.Controls.Add(this.checkBoxDropInvalidIds);
            this.groupBoxFilters.Controls.Add(this.labelCeiling);
            this.groupBoxFilters.Controls.Add(this.comboBoxIdCeiling);
            this.groupBoxFilters.Controls.Add(this.labelCeilingValue);
            this.groupBoxFilters.Controls.Add(this.labelOutOfBlock);
            this.groupBoxFilters.Controls.Add(this.comboBoxOutOfBlock);
            this.groupBoxFilters.Controls.Add(this.checkBoxDropInvalidZ);
            this.groupBoxFilters.Controls.Add(this.checkBoxNormalizeHue);
            this.groupBoxFilters.Controls.Add(this.checkBoxBelowTerrain);
            this.groupBoxFilters.Controls.Add(this.checkBoxRemoveDuplicates);
            this.groupBoxFilters.Controls.Add(this.checkBoxDuplicatesHue);
            this.groupBoxFilters.Controls.Add(this.checkBoxCollapseStacks);
            this.groupBoxFilters.Controls.Add(this.labelCollapseFlags);
            this.groupBoxFilters.Controls.Add(this.checkBoxCollapseWet);
            this.groupBoxFilters.Controls.Add(this.checkBoxCollapseSurface);
            this.groupBoxFilters.Controls.Add(this.checkBoxCollapseIgnoreZ);
            this.groupBoxFilters.Controls.Add(this.labelCollapseIds);
            this.groupBoxFilters.Controls.Add(this.textBoxCollapseIds);
            this.groupBoxFilters.Controls.Add(this.checkBoxSortTiles);
            this.groupBoxFilters.Location = new System.Drawing.Point(12, 164);
            this.groupBoxFilters.Name = "groupBoxFilters";
            this.groupBoxFilters.Size = new System.Drawing.Size(536, 290);
            this.groupBoxFilters.TabIndex = 1;
            this.groupBoxFilters.TabStop = false;
            this.groupBoxFilters.Text = "Filters";
            //
            // checkBoxDropInvalidIds
            //
            this.checkBoxDropInvalidIds.Location = new System.Drawing.Point(12, 22);
            this.checkBoxDropInvalidIds.Name = "checkBoxDropInvalidIds";
            this.checkBoxDropInvalidIds.Size = new System.Drawing.Size(226, 21);
            this.checkBoxDropInvalidIds.TabIndex = 0;
            this.checkBoxDropInvalidIds.Text = "Drop statics with an unknown item id";
            this.checkBoxDropInvalidIds.UseVisualStyleBackColor = true;
            this.checkBoxDropInvalidIds.CheckedChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // labelCeiling
            //
            this.labelCeiling.Location = new System.Drawing.Point(244, 25);
            this.labelCeiling.Name = "labelCeiling";
            this.labelCeiling.Size = new System.Drawing.Size(50, 17);
            this.labelCeiling.TabIndex = 1;
            this.labelCeiling.Text = "ceiling:";
            //
            // comboBoxIdCeiling
            //
            this.comboBoxIdCeiling.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIdCeiling.Location = new System.Drawing.Point(296, 21);
            this.comboBoxIdCeiling.Name = "comboBoxIdCeiling";
            this.comboBoxIdCeiling.Size = new System.Drawing.Size(126, 23);
            this.comboBoxIdCeiling.TabIndex = 2;
            this.comboBoxIdCeiling.SelectedIndexChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // labelCeilingValue
            //
            this.labelCeilingValue.Location = new System.Drawing.Point(428, 25);
            this.labelCeilingValue.Name = "labelCeilingValue";
            this.labelCeilingValue.Size = new System.Drawing.Size(96, 17);
            this.labelCeilingValue.TabIndex = 3;
            //
            // labelOutOfBlock
            //
            this.labelOutOfBlock.Location = new System.Drawing.Point(12, 56);
            this.labelOutOfBlock.Name = "labelOutOfBlock";
            this.labelOutOfBlock.Size = new System.Drawing.Size(162, 17);
            this.labelOutOfBlock.TabIndex = 4;
            this.labelOutOfBlock.Text = "Out-of-block x/y offsets:";
            //
            // comboBoxOutOfBlock
            //
            this.comboBoxOutOfBlock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOutOfBlock.Location = new System.Drawing.Point(180, 52);
            this.comboBoxOutOfBlock.Name = "comboBoxOutOfBlock";
            this.comboBoxOutOfBlock.Size = new System.Drawing.Size(150, 23);
            this.comboBoxOutOfBlock.TabIndex = 5;
            //
            // checkBoxDropInvalidZ
            //
            this.checkBoxDropInvalidZ.Location = new System.Drawing.Point(12, 82);
            this.checkBoxDropInvalidZ.Name = "checkBoxDropInvalidZ";
            this.checkBoxDropInvalidZ.Size = new System.Drawing.Size(300, 21);
            this.checkBoxDropInvalidZ.TabIndex = 6;
            this.checkBoxDropInvalidZ.Text = "Drop statics at z = -128";
            this.checkBoxDropInvalidZ.UseVisualStyleBackColor = true;
            //
            // checkBoxNormalizeHue
            //
            this.checkBoxNormalizeHue.Location = new System.Drawing.Point(12, 106);
            this.checkBoxNormalizeHue.Name = "checkBoxNormalizeHue";
            this.checkBoxNormalizeHue.Size = new System.Drawing.Size(300, 21);
            this.checkBoxNormalizeHue.TabIndex = 7;
            this.checkBoxNormalizeHue.Text = "Normalize negative hues to 0";
            this.checkBoxNormalizeHue.UseVisualStyleBackColor = true;
            //
            // checkBoxBelowTerrain
            //
            this.checkBoxBelowTerrain.Location = new System.Drawing.Point(12, 130);
            this.checkBoxBelowTerrain.Name = "checkBoxBelowTerrain";
            this.checkBoxBelowTerrain.Size = new System.Drawing.Size(400, 21);
            this.checkBoxBelowTerrain.TabIndex = 8;
            this.checkBoxBelowTerrain.Text = "Drop statics buried under the land tile (never drawn)";
            this.checkBoxBelowTerrain.UseVisualStyleBackColor = true;
            //
            // checkBoxRemoveDuplicates
            //
            this.checkBoxRemoveDuplicates.Location = new System.Drawing.Point(12, 154);
            this.checkBoxRemoveDuplicates.Name = "checkBoxRemoveDuplicates";
            this.checkBoxRemoveDuplicates.Size = new System.Drawing.Size(282, 21);
            this.checkBoxRemoveDuplicates.TabIndex = 9;
            this.checkBoxRemoveDuplicates.Text = "Remove duplicates (same id, x, y and z)";
            this.checkBoxRemoveDuplicates.UseVisualStyleBackColor = true;
            this.checkBoxRemoveDuplicates.CheckedChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // checkBoxDuplicatesHue
            //
            this.checkBoxDuplicatesHue.Location = new System.Drawing.Point(300, 154);
            this.checkBoxDuplicatesHue.Name = "checkBoxDuplicatesHue";
            this.checkBoxDuplicatesHue.Size = new System.Drawing.Size(224, 21);
            this.checkBoxDuplicatesHue.TabIndex = 10;
            this.checkBoxDuplicatesHue.Text = "compare hue too (legacy)";
            this.checkBoxDuplicatesHue.UseVisualStyleBackColor = true;
            //
            // checkBoxCollapseStacks
            //
            this.checkBoxCollapseStacks.Location = new System.Drawing.Point(12, 178);
            this.checkBoxCollapseStacks.Name = "checkBoxCollapseStacks";
            this.checkBoxCollapseStacks.Size = new System.Drawing.Size(400, 21);
            this.checkBoxCollapseStacks.TabIndex = 11;
            this.checkBoxCollapseStacks.Text = "Collapse stacked statics sharing a cell down to one";
            this.checkBoxCollapseStacks.UseVisualStyleBackColor = true;
            this.checkBoxCollapseStacks.CheckedChanged += new System.EventHandler(this.OnFilterChanged);
            //
            // labelCollapseFlags
            //
            this.labelCollapseFlags.Location = new System.Drawing.Point(32, 205);
            this.labelCollapseFlags.Name = "labelCollapseFlags";
            this.labelCollapseFlags.Size = new System.Drawing.Size(42, 17);
            this.labelCollapseFlags.TabIndex = 12;
            this.labelCollapseFlags.Text = "flags:";
            //
            // checkBoxCollapseWet
            //
            this.checkBoxCollapseWet.Location = new System.Drawing.Point(76, 202);
            this.checkBoxCollapseWet.Name = "checkBoxCollapseWet";
            this.checkBoxCollapseWet.Size = new System.Drawing.Size(60, 21);
            this.checkBoxCollapseWet.TabIndex = 13;
            this.checkBoxCollapseWet.Text = "Wet";
            this.checkBoxCollapseWet.UseVisualStyleBackColor = true;
            //
            // checkBoxCollapseSurface
            //
            this.checkBoxCollapseSurface.Location = new System.Drawing.Point(140, 202);
            this.checkBoxCollapseSurface.Name = "checkBoxCollapseSurface";
            this.checkBoxCollapseSurface.Size = new System.Drawing.Size(78, 21);
            this.checkBoxCollapseSurface.TabIndex = 14;
            this.checkBoxCollapseSurface.Text = "Surface";
            this.checkBoxCollapseSurface.UseVisualStyleBackColor = true;
            //
            // checkBoxCollapseIgnoreZ
            //
            this.checkBoxCollapseIgnoreZ.Location = new System.Drawing.Point(228, 202);
            this.checkBoxCollapseIgnoreZ.Name = "checkBoxCollapseIgnoreZ";
            this.checkBoxCollapseIgnoreZ.Size = new System.Drawing.Size(120, 21);
            this.checkBoxCollapseIgnoreZ.TabIndex = 15;
            this.checkBoxCollapseIgnoreZ.Text = "ignore z";
            this.checkBoxCollapseIgnoreZ.UseVisualStyleBackColor = true;
            //
            // labelCollapseIds
            //
            this.labelCollapseIds.Location = new System.Drawing.Point(32, 231);
            this.labelCollapseIds.Name = "labelCollapseIds";
            this.labelCollapseIds.Size = new System.Drawing.Size(62, 17);
            this.labelCollapseIds.TabIndex = 16;
            this.labelCollapseIds.Text = "item ids:";
            //
            // textBoxCollapseIds
            //
            this.textBoxCollapseIds.Location = new System.Drawing.Point(96, 228);
            this.textBoxCollapseIds.Name = "textBoxCollapseIds";
            this.textBoxCollapseIds.Size = new System.Drawing.Size(428, 23);
            this.textBoxCollapseIds.TabIndex = 17;
            //
            // checkBoxSortTiles
            //
            this.checkBoxSortTiles.Location = new System.Drawing.Point(12, 258);
            this.checkBoxSortTiles.Name = "checkBoxSortTiles";
            this.checkBoxSortTiles.Size = new System.Drawing.Size(300, 21);
            this.checkBoxSortTiles.TabIndex = 18;
            this.checkBoxSortTiles.Text = "Sort the statics within each block";
            this.checkBoxSortTiles.UseVisualStyleBackColor = true;
            //
            // groupBoxOutput
            //
            this.groupBoxOutput.Controls.Add(this.textBoxOutput);
            this.groupBoxOutput.Controls.Add(this.buttonBrowse);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 462);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(536, 58);
            this.groupBoxOutput.TabIndex = 2;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output folder";
            //
            // textBoxOutput
            //
            this.textBoxOutput.Location = new System.Drawing.Point(12, 22);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(422, 23);
            this.textBoxOutput.TabIndex = 0;
            //
            // buttonBrowse
            //
            this.buttonBrowse.Location = new System.Drawing.Point(440, 21);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(84, 25);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "Browse...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.OnClickBrowse);
            //
            // progressBar
            //
            this.progressBar.Location = new System.Drawing.Point(12, 528);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(536, 18);
            this.progressBar.TabIndex = 3;
            //
            // labelStatus
            //
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 551);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(536, 17);
            this.labelStatus.TabIndex = 4;
            //
            // buttonAnalyze
            //
            this.buttonAnalyze.Location = new System.Drawing.Point(190, 574);
            this.buttonAnalyze.Name = "buttonAnalyze";
            this.buttonAnalyze.Size = new System.Drawing.Size(96, 28);
            this.buttonAnalyze.TabIndex = 5;
            this.buttonAnalyze.Text = "Analyze";
            this.buttonAnalyze.UseVisualStyleBackColor = true;
            this.buttonAnalyze.Click += new System.EventHandler(this.OnClickAnalyze);
            //
            // buttonDefrag
            //
            this.buttonDefrag.Location = new System.Drawing.Point(292, 574);
            this.buttonDefrag.Name = "buttonDefrag";
            this.buttonDefrag.Size = new System.Drawing.Size(96, 28);
            this.buttonDefrag.TabIndex = 6;
            this.buttonDefrag.Text = "Defrag";
            this.buttonDefrag.UseVisualStyleBackColor = true;
            this.buttonDefrag.Click += new System.EventHandler(this.OnClickDefrag);
            //
            // buttonCancel
            //
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Location = new System.Drawing.Point(394, 574);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(72, 28);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.OnClickCancel);
            //
            // buttonClose
            //
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(472, 574);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(76, 28);
            this.buttonClose.TabIndex = 8;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            //
            // worker
            //
            this.worker.WorkerReportsProgress = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OnWorkerDoWork);
            this.worker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.OnWorkerProgressChanged);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnWorkerCompleted);
            //
            // MapDefragStaticsForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(560, 614);
            this.Controls.Add(this.groupBoxSource);
            this.Controls.Add(this.groupBoxFilters);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonAnalyze);
            this.Controls.Add(this.buttonDefrag);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonClose);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapDefragStaticsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Defrag Statics";
            this.groupBoxSource.ResumeLayout(false);
            this.groupBoxSource.PerformLayout();
            this.groupBoxFilters.ResumeLayout(false);
            this.groupBoxFilters.PerformLayout();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Button buttonAnalyze;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonDefrag;
        private System.Windows.Forms.CheckBox checkBoxAllowTruncation;
        private System.Windows.Forms.CheckBox checkBoxBelowTerrain;
        private System.Windows.Forms.CheckBox checkBoxCollapseIgnoreZ;
        private System.Windows.Forms.CheckBox checkBoxCollapseStacks;
        private System.Windows.Forms.CheckBox checkBoxCollapseSurface;
        private System.Windows.Forms.CheckBox checkBoxCollapseWet;
        private System.Windows.Forms.CheckBox checkBoxDropInvalidIds;
        private System.Windows.Forms.CheckBox checkBoxDropInvalidZ;
        private System.Windows.Forms.CheckBox checkBoxDuplicatesHue;
        private System.Windows.Forms.CheckBox checkBoxNormalizeHue;
        private System.Windows.Forms.CheckBox checkBoxRemoveDuplicates;
        private System.Windows.Forms.CheckBox checkBoxSortTiles;
        private System.Windows.Forms.ComboBox comboBoxIdCeiling;
        private System.Windows.Forms.ComboBox comboBoxOutOfBlock;
        private System.Windows.Forms.GroupBox groupBoxFilters;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.GroupBox groupBoxSource;
        private System.Windows.Forms.Label labelCeiling;
        private System.Windows.Forms.Label labelCeilingValue;
        private System.Windows.Forms.Label labelCollapseFlags;
        private System.Windows.Forms.Label labelCollapseIds;
        private System.Windows.Forms.Label labelGeometry;
        private System.Windows.Forms.Label labelOutOfBlock;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox textBoxCollapseIds;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.TextBox textBoxSource;
    }
}