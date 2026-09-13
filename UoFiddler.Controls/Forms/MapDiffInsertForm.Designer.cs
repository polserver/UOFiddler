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
    partial class MapDiffInsertForm
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
            this.groupBoxWhat = new System.Windows.Forms.GroupBox();
            this.checkBoxMap = new System.Windows.Forms.CheckBox();
            this.labelMapFormat = new System.Windows.Forms.Label();
            this.comboBoxMapFormat = new System.Windows.Forms.ComboBox();
            this.checkBoxStatics = new System.Windows.Forms.CheckBox();
            this.RemoveDupl = new System.Windows.Forms.CheckBox();
            this.checkBoxDuplicatesHue = new System.Windows.Forms.CheckBox();
            this.groupBoxFrom = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownX1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownY1 = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownX2 = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownY2 = new System.Windows.Forms.NumericUpDown();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.textBoxPreview = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.worker = new System.ComponentModel.BackgroundWorker();
            this.components.Add(this.worker);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY2)).BeginInit();
            this.groupBoxWhat.SuspendLayout();
            this.groupBoxFrom.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxWhat
            //
            this.groupBoxWhat.Controls.Add(this.checkBoxMap);
            this.groupBoxWhat.Controls.Add(this.labelMapFormat);
            this.groupBoxWhat.Controls.Add(this.comboBoxMapFormat);
            this.groupBoxWhat.Controls.Add(this.checkBoxStatics);
            this.groupBoxWhat.Controls.Add(this.RemoveDupl);
            this.groupBoxWhat.Controls.Add(this.checkBoxDuplicatesHue);
            this.groupBoxWhat.Location = new System.Drawing.Point(12, 12);
            this.groupBoxWhat.Name = "groupBoxWhat";
            this.groupBoxWhat.Size = new System.Drawing.Size(496, 86);
            this.groupBoxWhat.TabIndex = 1;
            this.groupBoxWhat.TabStop = false;
            this.groupBoxWhat.Text = "Insert";
            //
            // checkBoxMap
            //
            this.checkBoxMap.Location = new System.Drawing.Point(16, 22);
            this.checkBoxMap.Name = "checkBoxMap";
            this.checkBoxMap.Size = new System.Drawing.Size(110, 21);
            this.checkBoxMap.TabIndex = 0;
            this.checkBoxMap.Text = "Map";
            this.checkBoxMap.UseVisualStyleBackColor = true;
            this.checkBoxMap.CheckedChanged += new System.EventHandler(this.OnOptionChanged);
            //
            // labelMapFormat
            //
            this.labelMapFormat.Location = new System.Drawing.Point(140, 25);
            this.labelMapFormat.Name = "labelMapFormat";
            this.labelMapFormat.Size = new System.Drawing.Size(80, 17);
            this.labelMapFormat.TabIndex = 1;
            this.labelMapFormat.Text = "written as:";
            //
            // comboBoxMapFormat
            //
            this.comboBoxMapFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMapFormat.Location = new System.Drawing.Point(224, 21);
            this.comboBoxMapFormat.Name = "comboBoxMapFormat";
            this.comboBoxMapFormat.Size = new System.Drawing.Size(256, 23);
            this.comboBoxMapFormat.TabIndex = 2;
            //
            // checkBoxStatics
            //
            this.checkBoxStatics.Location = new System.Drawing.Point(16, 52);
            this.checkBoxStatics.Name = "checkBoxStatics";
            this.checkBoxStatics.Size = new System.Drawing.Size(110, 21);
            this.checkBoxStatics.TabIndex = 3;
            this.checkBoxStatics.Text = "Statics";
            this.checkBoxStatics.UseVisualStyleBackColor = true;
            this.checkBoxStatics.CheckedChanged += new System.EventHandler(this.OnOptionChanged);
            //
            // RemoveDupl
            //
            this.RemoveDupl.Location = new System.Drawing.Point(140, 52);
            this.RemoveDupl.Name = "RemoveDupl";
            this.RemoveDupl.Size = new System.Drawing.Size(150, 21);
            this.RemoveDupl.TabIndex = 4;
            this.RemoveDupl.Text = "remove duplicates";
            this.RemoveDupl.UseVisualStyleBackColor = true;
            this.RemoveDupl.CheckedChanged += new System.EventHandler(this.OnOptionChanged);
            //
            // checkBoxDuplicatesHue
            //
            this.checkBoxDuplicatesHue.Location = new System.Drawing.Point(296, 52);
            this.checkBoxDuplicatesHue.Name = "checkBoxDuplicatesHue";
            this.checkBoxDuplicatesHue.Size = new System.Drawing.Size(184, 21);
            this.checkBoxDuplicatesHue.TabIndex = 5;
            this.checkBoxDuplicatesHue.Text = "comparing hue too (legacy)";
            this.checkBoxDuplicatesHue.UseVisualStyleBackColor = true;
            //
            // groupBoxFrom
            //
            this.groupBoxFrom.Controls.Add(this.label1);
            this.groupBoxFrom.Controls.Add(this.numericUpDownX1);
            this.groupBoxFrom.Controls.Add(this.label2);
            this.groupBoxFrom.Controls.Add(this.numericUpDownY1);
            this.groupBoxFrom.Controls.Add(this.label3);
            this.groupBoxFrom.Controls.Add(this.numericUpDownX2);
            this.groupBoxFrom.Controls.Add(this.label4);
            this.groupBoxFrom.Controls.Add(this.numericUpDownY2);
            this.groupBoxFrom.Location = new System.Drawing.Point(12, 104);
            this.groupBoxFrom.Name = "groupBoxFrom";
            this.groupBoxFrom.Size = new System.Drawing.Size(496, 60);
            this.groupBoxFrom.TabIndex = 2;
            this.groupBoxFrom.TabStop = false;
            this.groupBoxFrom.Text = "Region, in map tiles";
            //
            // label1
            //
            this.label1.Location = new System.Drawing.Point(16, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "X1";
            //
            // numericUpDownX1
            //
            this.numericUpDownX1.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            this.numericUpDownX1.Location = new System.Drawing.Point(44, 23);
            this.numericUpDownX1.Name = "numericUpDownX1";
            this.numericUpDownX1.Size = new System.Drawing.Size(70, 23);
            this.numericUpDownX1.TabIndex = 1;
            this.numericUpDownX1.ValueChanged += new System.EventHandler(this.OnRegionChanged);
            //
            // label2
            //
            this.label2.Location = new System.Drawing.Point(124, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(24, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Y1";
            //
            // numericUpDownY1
            //
            this.numericUpDownY1.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            this.numericUpDownY1.Location = new System.Drawing.Point(152, 23);
            this.numericUpDownY1.Name = "numericUpDownY1";
            this.numericUpDownY1.Size = new System.Drawing.Size(70, 23);
            this.numericUpDownY1.TabIndex = 3;
            this.numericUpDownY1.ValueChanged += new System.EventHandler(this.OnRegionChanged);
            //
            // label3
            //
            this.label3.Location = new System.Drawing.Point(248, 26);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "X2";
            //
            // numericUpDownX2
            //
            this.numericUpDownX2.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            this.numericUpDownX2.Location = new System.Drawing.Point(276, 23);
            this.numericUpDownX2.Name = "numericUpDownX2";
            this.numericUpDownX2.Size = new System.Drawing.Size(70, 23);
            this.numericUpDownX2.TabIndex = 5;
            this.numericUpDownX2.ValueChanged += new System.EventHandler(this.OnRegionChanged);
            //
            // label4
            //
            this.label4.Location = new System.Drawing.Point(356, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(24, 17);
            this.label4.TabIndex = 6;
            this.label4.Text = "Y2";
            //
            // numericUpDownY2
            //
            this.numericUpDownY2.Increment = new decimal(new int[] { 8, 0, 0, 0 });
            this.numericUpDownY2.Location = new System.Drawing.Point(384, 23);
            this.numericUpDownY2.Name = "numericUpDownY2";
            this.numericUpDownY2.Size = new System.Drawing.Size(70, 23);
            this.numericUpDownY2.TabIndex = 7;
            this.numericUpDownY2.ValueChanged += new System.EventHandler(this.OnRegionChanged);
            //
            // groupBoxPreview
            //
            this.groupBoxPreview.Controls.Add(this.textBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 170);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(496, 80);
            this.groupBoxPreview.TabIndex = 4;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "What will be inserted";
            //
            // textBoxPreview
            //
            this.textBoxPreview.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxPreview.Location = new System.Drawing.Point(16, 20);
            this.textBoxPreview.Multiline = true;
            this.textBoxPreview.Name = "textBoxPreview";
            this.textBoxPreview.ReadOnly = true;
            this.textBoxPreview.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxPreview.Size = new System.Drawing.Size(464, 52);
            this.textBoxPreview.TabIndex = 0;
            this.textBoxPreview.TabStop = false;
            //
            // progressBar1
            //
            this.progressBar1.Location = new System.Drawing.Point(12, 258);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(496, 18);
            this.progressBar1.TabIndex = 5;
            //
            // labelStatus
            //
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 281);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(496, 17);
            this.labelStatus.TabIndex = 6;
            //
            // buttonCopy
            //
            this.buttonCopy.Location = new System.Drawing.Point(244, 306);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(80, 28);
            this.buttonCopy.TabIndex = 7;
            this.buttonCopy.Text = "Insert";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.OnClickCopy);
            //
            // buttonCancel
            //
            this.buttonCancel.Enabled = false;
            this.buttonCancel.Location = new System.Drawing.Point(332, 306);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(72, 28);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.OnClickCancel);
            //
            // buttonClose
            //
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(420, 306);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(80, 28);
            this.buttonClose.TabIndex = 9;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.OnClickClose);
            //
            // worker
            //
            this.worker.WorkerSupportsCancellation = true;
            this.worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.OnWorkerDoWork);
            this.worker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.OnWorkerCompleted);
            //
            // MapReplaceForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(520, 346);
            this.Controls.Add(this.groupBoxWhat);
            this.Controls.Add(this.groupBoxFrom);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonClose);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MapDiffInsertForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Diff to Map Copy";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownX2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownY2)).EndInit();
            this.groupBoxWhat.ResumeLayout(false);
            this.groupBoxFrom.ResumeLayout(false);
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBoxPreview.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker worker;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.CheckBox RemoveDupl;
        private System.Windows.Forms.CheckBox checkBoxDuplicatesHue;
        private System.Windows.Forms.CheckBox checkBoxMap;
        private System.Windows.Forms.CheckBox checkBoxStatics;
        private System.Windows.Forms.ComboBox comboBoxMapFormat;
        private System.Windows.Forms.GroupBox groupBoxFrom;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.GroupBox groupBoxWhat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelMapFormat;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.NumericUpDown numericUpDownX1;
        private System.Windows.Forms.NumericUpDown numericUpDownX2;
        private System.Windows.Forms.NumericUpDown numericUpDownY1;
        private System.Windows.Forms.NumericUpDown numericUpDownY2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox textBoxPreview;
    }
}