namespace UoFiddler.Plugin.MultiEditor.UI
{
    partial class EditorSettingsDialog
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
            headingColours = new System.Windows.Forms.Label();
            labelBackground = new System.Windows.Forms.Label();
            swatchBackground = new System.Windows.Forms.Button();
            labelWorkPlane = new System.Windows.Forms.Label();
            labelWorkPlaneOpacity = new System.Windows.Forms.Label();
            alphaWorkPlane = new System.Windows.Forms.NumericUpDown();
            swatchWorkPlane = new System.Windows.Forms.Button();
            labelGrid = new System.Windows.Forms.Label();
            labelGridOpacity = new System.Windows.Forms.Label();
            alphaGrid = new System.Windows.Forms.NumericUpDown();
            swatchGrid = new System.Windows.Forms.Button();
            labelOutline = new System.Windows.Forms.Label();
            labelOutlineOpacity = new System.Windows.Forms.Label();
            alphaOutline = new System.Windows.Forms.NumericUpDown();
            swatchOutline = new System.Windows.Forms.Button();
            labelSelection = new System.Windows.Forms.Label();
            swatchSelection = new System.Windows.Forms.Button();
            headingStart = new System.Windows.Forms.Label();
            checkWorkPlane = new System.Windows.Forms.CheckBox();
            checkGrid = new System.Windows.Forms.CheckBox();
            checkHideFloorsAbove = new System.Windows.Forms.CheckBox();
            buttonDefaults = new System.Windows.Forms.Button();
            buttonOk = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)alphaWorkPlane).BeginInit();
            ((System.ComponentModel.ISupportInitialize)alphaGrid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)alphaOutline).BeginInit();
            SuspendLayout();
            // 
            // headingColours
            // 
            headingColours.AutoSize = true;
            headingColours.Location = new System.Drawing.Point(12, 12);
            headingColours.Name = "headingColours";
            headingColours.Size = new System.Drawing.Size(47, 15);
            headingColours.TabIndex = 0;
            headingColours.Text = "Colours";
            // 
            // labelBackground
            // 
            labelBackground.AutoSize = true;
            labelBackground.Location = new System.Drawing.Point(24, 40);
            labelBackground.Name = "labelBackground";
            labelBackground.Size = new System.Drawing.Size(320, 15);
            labelBackground.TabIndex = 1;
            labelBackground.Text = "Canvas background (follows the theme until changed)";
            // 
            // swatchBackground
            // 
            swatchBackground.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            swatchBackground.Location = new System.Drawing.Point(330, 36);
            swatchBackground.Name = "swatchBackground";
            swatchBackground.Size = new System.Drawing.Size(60, 24);
            swatchBackground.TabIndex = 2;
            swatchBackground.UseVisualStyleBackColor = true;
            swatchBackground.Click += SwatchBackground_Click;
            // 
            // labelWorkPlane
            // 
            labelWorkPlane.AutoSize = true;
            labelWorkPlane.Location = new System.Drawing.Point(24, 70);
            labelWorkPlane.Name = "labelWorkPlane";
            labelWorkPlane.Size = new System.Drawing.Size(72, 15);
            labelWorkPlane.TabIndex = 7;
            labelWorkPlane.Text = "Work plane";
            // 
            // labelWorkPlaneOpacity
            // 
            labelWorkPlaneOpacity.AutoSize = true;
            labelWorkPlaneOpacity.Location = new System.Drawing.Point(272, 70);
            labelWorkPlaneOpacity.Name = "labelWorkPlaneOpacity";
            labelWorkPlaneOpacity.Size = new System.Drawing.Size(59, 15);
            labelWorkPlaneOpacity.TabIndex = 8;
            labelWorkPlaneOpacity.Text = "Opacity %";
            // 
            // alphaWorkPlane
            // 
            alphaWorkPlane.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            alphaWorkPlane.Location = new System.Drawing.Point(340, 67);
            alphaWorkPlane.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            alphaWorkPlane.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            alphaWorkPlane.Name = "alphaWorkPlane";
            alphaWorkPlane.Size = new System.Drawing.Size(56, 23);
            alphaWorkPlane.TabIndex = 9;
            alphaWorkPlane.Value = new decimal(new int[] { 5, 0, 0, 0 });
            alphaWorkPlane.ValueChanged += AlphaWorkPlane_ValueChanged;
            // 
            // swatchWorkPlane
            // 
            swatchWorkPlane.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            swatchWorkPlane.Location = new System.Drawing.Point(200, 66);
            swatchWorkPlane.Name = "swatchWorkPlane";
            swatchWorkPlane.Size = new System.Drawing.Size(60, 24);
            swatchWorkPlane.TabIndex = 10;
            swatchWorkPlane.UseVisualStyleBackColor = true;
            swatchWorkPlane.Click += SwatchWorkPlane_Click;
            // 
            // labelGrid
            // 
            labelGrid.AutoSize = true;
            labelGrid.Location = new System.Drawing.Point(24, 100);
            labelGrid.Name = "labelGrid";
            labelGrid.Size = new System.Drawing.Size(66, 15);
            labelGrid.TabIndex = 11;
            labelGrid.Text = "Grid lines";
            // 
            // labelGridOpacity
            // 
            labelGridOpacity.AutoSize = true;
            labelGridOpacity.Location = new System.Drawing.Point(272, 100);
            labelGridOpacity.Name = "labelGridOpacity";
            labelGridOpacity.Size = new System.Drawing.Size(59, 15);
            labelGridOpacity.TabIndex = 12;
            labelGridOpacity.Text = "Opacity %";
            // 
            // alphaGrid
            // 
            alphaGrid.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            alphaGrid.Location = new System.Drawing.Point(340, 97);
            alphaGrid.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            alphaGrid.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            alphaGrid.Name = "alphaGrid";
            alphaGrid.Size = new System.Drawing.Size(56, 23);
            alphaGrid.TabIndex = 13;
            alphaGrid.Value = new decimal(new int[] { 5, 0, 0, 0 });
            alphaGrid.ValueChanged += AlphaGrid_ValueChanged;
            // 
            // swatchGrid
            // 
            swatchGrid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            swatchGrid.Location = new System.Drawing.Point(200, 96);
            swatchGrid.Name = "swatchGrid";
            swatchGrid.Size = new System.Drawing.Size(60, 24);
            swatchGrid.TabIndex = 14;
            swatchGrid.UseVisualStyleBackColor = true;
            swatchGrid.Click += SwatchGrid_Click;
            // 
            // labelOutline
            // 
            labelOutline.AutoSize = true;
            labelOutline.Location = new System.Drawing.Point(24, 130);
            labelOutline.Name = "labelOutline";
            labelOutline.Size = new System.Drawing.Size(84, 15);
            labelOutline.TabIndex = 15;
            labelOutline.Text = "Multi outline";
            // 
            // labelOutlineOpacity
            // 
            labelOutlineOpacity.AutoSize = true;
            labelOutlineOpacity.Location = new System.Drawing.Point(272, 130);
            labelOutlineOpacity.Name = "labelOutlineOpacity";
            labelOutlineOpacity.Size = new System.Drawing.Size(59, 15);
            labelOutlineOpacity.TabIndex = 16;
            labelOutlineOpacity.Text = "Opacity %";
            // 
            // alphaOutline
            // 
            alphaOutline.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            alphaOutline.Location = new System.Drawing.Point(340, 127);
            alphaOutline.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            alphaOutline.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            alphaOutline.Name = "alphaOutline";
            alphaOutline.Size = new System.Drawing.Size(56, 23);
            alphaOutline.TabIndex = 17;
            alphaOutline.Value = new decimal(new int[] { 5, 0, 0, 0 });
            alphaOutline.ValueChanged += AlphaOutline_ValueChanged;
            // 
            // swatchOutline
            // 
            swatchOutline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            swatchOutline.Location = new System.Drawing.Point(200, 126);
            swatchOutline.Name = "swatchOutline";
            swatchOutline.Size = new System.Drawing.Size(60, 24);
            swatchOutline.TabIndex = 18;
            swatchOutline.UseVisualStyleBackColor = true;
            swatchOutline.Click += SwatchOutline_Click;
            // 
            // labelSelection
            // 
            labelSelection.AutoSize = true;
            labelSelection.Location = new System.Drawing.Point(24, 160);
            labelSelection.Name = "labelSelection";
            labelSelection.Size = new System.Drawing.Size(146, 15);
            labelSelection.TabIndex = 19;
            labelSelection.Text = "Selection box and lasso";
            // 
            // swatchSelection
            // 
            swatchSelection.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            swatchSelection.Location = new System.Drawing.Point(330, 156);
            swatchSelection.Name = "swatchSelection";
            swatchSelection.Size = new System.Drawing.Size(60, 24);
            swatchSelection.TabIndex = 20;
            swatchSelection.UseVisualStyleBackColor = true;
            swatchSelection.Click += SwatchSelection_Click;
            // 
            // headingStart
            // 
            headingStart.AutoSize = true;
            headingStart.Location = new System.Drawing.Point(12, 194);
            headingStart.Name = "headingStart";
            headingStart.Size = new System.Drawing.Size(202, 15);
            headingStart.TabIndex = 21;
            headingStart.Text = "New and opened multis start with";
            // 
            // checkWorkPlane
            // 
            checkWorkPlane.AutoSize = true;
            checkWorkPlane.Location = new System.Drawing.Point(24, 218);
            checkWorkPlane.Name = "checkWorkPlane";
            checkWorkPlane.Size = new System.Drawing.Size(129, 19);
            checkWorkPlane.TabIndex = 23;
            checkWorkPlane.Text = "Work plane shown";
            checkWorkPlane.UseVisualStyleBackColor = true;
            checkWorkPlane.CheckedChanged += CheckWorkPlane_CheckedChanged;
            // 
            // checkGrid
            // 
            checkGrid.AutoSize = true;
            checkGrid.Location = new System.Drawing.Point(24, 242);
            checkGrid.Name = "checkGrid";
            checkGrid.Size = new System.Drawing.Size(86, 19);
            checkGrid.TabIndex = 24;
            checkGrid.Text = "Grid shown";
            checkGrid.UseVisualStyleBackColor = true;
            checkGrid.CheckedChanged += CheckGrid_CheckedChanged;
            // 
            // checkHideFloorsAbove
            // 
            checkHideFloorsAbove.AutoSize = true;
            checkHideFloorsAbove.Location = new System.Drawing.Point(24, 266);
            checkHideFloorsAbove.Name = "checkHideFloorsAbove";
            checkHideFloorsAbove.Size = new System.Drawing.Size(365, 19);
            checkHideFloorsAbove.TabIndex = 25;
            checkHideFloorsAbove.Text = "Floors above the active one hidden (off: drawn faintly)";
            checkHideFloorsAbove.UseVisualStyleBackColor = true;
            checkHideFloorsAbove.CheckedChanged += CheckHideFloorsAbove_CheckedChanged;
            // 
            // buttonDefaults
            // 
            buttonDefaults.Location = new System.Drawing.Point(12, 302);
            buttonDefaults.Name = "buttonDefaults";
            buttonDefaults.Size = new System.Drawing.Size(80, 23);
            buttonDefaults.TabIndex = 26;
            buttonDefaults.Text = "Defaults";
            buttonDefaults.UseVisualStyleBackColor = true;
            buttonDefaults.Click += Defaults_Click;
            // 
            // buttonOk
            // 
            buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            buttonOk.Location = new System.Drawing.Point(236, 302);
            buttonOk.Name = "buttonOk";
            buttonOk.Size = new System.Drawing.Size(76, 23);
            buttonOk.TabIndex = 27;
            buttonOk.Text = "OK";
            buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            buttonCancel.Location = new System.Drawing.Point(320, 302);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(76, 23);
            buttonCancel.TabIndex = 28;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // EditorSettingsDialog
            // 
            AcceptButton = buttonOk;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new System.Drawing.Size(410, 340);
            Controls.Add(headingColours);
            Controls.Add(labelBackground);
            Controls.Add(swatchBackground);
            Controls.Add(labelWorkPlane);
            Controls.Add(labelWorkPlaneOpacity);
            Controls.Add(alphaWorkPlane);
            Controls.Add(swatchWorkPlane);
            Controls.Add(labelGrid);
            Controls.Add(labelGridOpacity);
            Controls.Add(alphaGrid);
            Controls.Add(swatchGrid);
            Controls.Add(labelOutline);
            Controls.Add(labelOutlineOpacity);
            Controls.Add(alphaOutline);
            Controls.Add(swatchOutline);
            Controls.Add(labelSelection);
            Controls.Add(swatchSelection);
            Controls.Add(headingStart);
            Controls.Add(checkWorkPlane);
            Controls.Add(checkGrid);
            Controls.Add(checkHideFloorsAbove);
            Controls.Add(buttonDefaults);
            Controls.Add(buttonOk);
            Controls.Add(buttonCancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditorSettingsDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Multi Editor settings";
            ((System.ComponentModel.ISupportInitialize)alphaWorkPlane).EndInit();
            ((System.ComponentModel.ISupportInitialize)alphaGrid).EndInit();
            ((System.ComponentModel.ISupportInitialize)alphaOutline).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label headingColours;
        private System.Windows.Forms.Label labelBackground;
        private System.Windows.Forms.Button swatchBackground;
        private System.Windows.Forms.Label labelWorkPlane;
        private System.Windows.Forms.Label labelWorkPlaneOpacity;
        private System.Windows.Forms.NumericUpDown alphaWorkPlane;
        private System.Windows.Forms.Button swatchWorkPlane;
        private System.Windows.Forms.Label labelGrid;
        private System.Windows.Forms.Label labelGridOpacity;
        private System.Windows.Forms.NumericUpDown alphaGrid;
        private System.Windows.Forms.Button swatchGrid;
        private System.Windows.Forms.Label labelOutline;
        private System.Windows.Forms.Label labelOutlineOpacity;
        private System.Windows.Forms.NumericUpDown alphaOutline;
        private System.Windows.Forms.Button swatchOutline;
        private System.Windows.Forms.Label labelSelection;
        private System.Windows.Forms.Button swatchSelection;
        private System.Windows.Forms.Label headingStart;
        private System.Windows.Forms.CheckBox checkWorkPlane;
        private System.Windows.Forms.CheckBox checkGrid;
        private System.Windows.Forms.CheckBox checkHideFloorsAbove;
        private System.Windows.Forms.Button buttonDefaults;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}
